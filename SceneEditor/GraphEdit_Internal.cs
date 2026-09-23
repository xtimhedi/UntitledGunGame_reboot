using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using UGGR.CrashScreen;
using UGGR.SceneEditor;
using UGGR.SceneEditor.Nodes;
using UGGR.SceneEditor.Nodes.Variables;

public partial class GraphEdit_Internal : GraphEdit
{
    public Action<long> OnContextMenuItemSelected;
    public PopupMenu menu = new PopupMenu();
    public int SelfConnectionCount = 0;

    public static List<IVar> Variables = new List<IVar>()
    {
        new IVar
        {
            Name = "Test",
            id = 0,
            type = Variant.Type.Bool,
            value = true
        }
    };

    public override void _Ready()
    {
        ConnectionRequest += OnConnectionRequest;
        DisconnectionRequest += OnDisconnectionRequest;
        DeleteNodesRequest += OnDeleteNodesRequest;
        CreateNodeSelectMenu();
    }

    private void OnConnectionRequest(StringName fromNodeName, long fromPort, StringName toNodeName, long toPort)
    {
        var fromNode = GetNodeOrNull<CustomGraphNode>(fromNodeName.ToString());
        var toNode = GetNodeOrNull<CustomGraphNode>(toNodeName.ToString());

        if (fromNode == null || toNode == null) return;

        int fromType = fromNode.GetOutputPortType((int)fromPort);
        int toType = toNode.GetInputPortType((int)toPort);

        if (fromType == toType)
        {
            ConnectNode(fromNodeName, (int)fromPort, toNodeName, (int)toPort);

            fromNode.DownstreamConnections.Add(new CustomGraphNode.NodeConnection
            {
                FromPort = (int)fromPort,
                ToNode = toNode,
                ToPort = (int)toPort
            });

            if (fromType != (int)PinTypeEnum.Execution)
            {
                toNode.SetInputValue((int)toPort, fromNode.GetOutputValue((int)fromPort));
            }
            if (toNode is Get getNode)
            {
                getNode.UpdateVariableBinding();
            }
            if (fromNode is Get getNode2)
            {
                getNode2.UpdateVariableBinding();
            }
            if (toNode is Set setNode)
            {
                setNode.UpdateVariableBinding();
            }
            if (fromNode is Set setNode2)
            {
                setNode2.UpdateVariableBinding();
            }
        }
        else
        {
            OS.Alert("Rejected: Pin types do not match!", "Connection Error");
        }
    }

    private void OnDisconnectionRequest(StringName fromNodeName, long fromPort, StringName toNodeName, long toPort)
    {

        var fromNode = GetNodeOrNull<CustomGraphNode>(fromNodeName.ToString());
        var toNode = GetNodeOrNull<CustomGraphNode>(toNodeName.ToString());
        if (toNode is Get getNode)
        {
            getNode.UpdateVariableBinding();
        }
        if (fromNode is Get getNode2)
        {
            getNode2.UpdateVariableBinding();
        }
        if (toNode is Set setNode)
        {
            setNode.UpdateVariableBinding();
        }
        if (fromNode is Set setNode2)
        {
            setNode2.UpdateVariableBinding();
        }
        DisconnectNode(fromNodeName, (int)fromPort, toNodeName, (int)toPort);
        CleanUpCustomConnectionCache(fromNodeName, fromPort, toNodeName, toPort);
        
    }

    private void CleanUpCustomConnectionCache(StringName fromNodeName, long fromPort, StringName toNodeName, long toPort)
    {
        var fromNode = GetNodeOrNull<CustomGraphNode>(fromNodeName.ToString());
        if (fromNode != null)
        {
            fromNode.DownstreamConnections.RemoveAll(conn =>
                conn.FromPort == (int)fromPort &&
                conn.ToNode.Name == toNodeName &&
                conn.ToPort == (int)toPort
            );
        }

        var toNode = GetNodeOrNull<CustomGraphNode>(toNodeName.ToString());
        if (toNode != null)
        {
            toNode.SetInputValue((int)toPort, null);
        }
    }

    private void CreateNodeSelectMenu()
    {
        PopupMenu math = new PopupMenu();

        menu.IdPressed += OnItemPressed;
        math.IdPressed += OnItemPressed;

        math.AddItem("Add", 0);
        math.AddItem("Subtract", 1);
        math.AddItem("Multiply", 2);
        math.AddItem("Divide", 3);
        math.AddItem(">", 9);

        menu.AddSubmenuNodeItem("Math", math);

        menu.AddItem("Value (int)", 4);
        menu.AddItem("Display (int)", 5);
        menu.AddItem("Branch", 6);
        menu.AddItem("Button", 7);
        menu.AddItem("Exec Test", 8);

        menu.AddItem("Var Get", 11);
        menu.AddItem("Var Set", 12);
        menu.AddItem("Not", 13);

        menu.AddChild(math);
        AddChild(menu);

        void OnItemPressed(long id)
        {
            OnContextMenuItemSelected?.Invoke(id);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed)
        {
            if (mouseEvent.ButtonIndex == MouseButton.Right)
            {
                menu.Popup(new Rect2I((Vector2I)mouseEvent.GlobalPosition, Vector2I.Zero));
                AcceptEvent();
            }
        }
    }

    public void PlaceNode(int id, int VarRef = -1)
    {
        if (id >= 0 && id < NodeRegistry.NodeFactories.Count)
        {
            CustomGraphNode node = NodeRegistry.NodeFactories[id].Invoke();
            Vector2 localMousePos = GetLocalMousePosition();

            if (node is VariableRef vref)
            {
                vref.ReferenceID = VarRef;
                localMousePos = GetWindow().Size / 2;
            } 
            node.FactoryId = id;

            node.PositionOffset = (localMousePos + ScrollOffset) / Zoom;

            AddChild(node);
        }
    }
    private void OnDeleteNodesRequest(Godot.Collections.Array<StringName> nodes)
    {
        foreach (StringName nodeName in nodes)
        {
            GraphNode node = GetNodeOrNull<GraphNode>(nodeName.ToString());
            if (node != null)
            {
                RemoveNodeAndConnections(node);
            }
        }
    }

    private void RemoveNodeAndConnections(GraphNode node)
    {
        StringName nodeName = node.Name;
        var connections = GetConnectionList();

        foreach (Godot.Collections.Dictionary connection in connections)
        {
            StringName fromNode = connection["from_node"].AsStringName();
            StringName toNode = connection["to_node"].AsStringName();

            if (fromNode == nodeName || toNode == nodeName)
            {
                long fromPort = connection["from_port"].AsInt64();
                long toPort = connection["to_port"].AsInt64();

                DisconnectNode(fromNode, (int)fromPort, toNode, (int)toPort);
                CleanUpCustomConnectionCache(fromNode, fromPort, toNode, toPort);
            }
        }

        node.QueueFree();
    }

    public void ClearGraph()
    {
        ClearConnections();

        foreach (Node child in GetChildren())
        {
            if (child is CustomGraphNode)
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }
    }

    public void SaveGraph(string path)
    {
        // 1. Serialize Variables
        var varsData = new List<Dictionary<string, object>>();
        foreach (IVar v in Variables)
        {
            varsData.Add(new Dictionary<string, object>
            {
                { "Name", v.Name },
                { "id", v.id },
                { "type", (int)v.type },
                // Use Godot's built-in tool to turn any Variant (Color, Vector3, etc.) into a safe string
                { "value", GD.VarToStr(v.value) }
            });
        }

        // 2. Serialize Nodes
        var nodesData = new List<Dictionary<string, object>>();
        foreach (Node child in GetChildren())
        {
            if (child is CustomGraphNode gnode)
            {
                var nodeData = new Dictionary<string, object>
                {
                    {"Name", gnode.Name.ToString() },
                    {"FactoryId", gnode.FactoryId },
                    {"PosX", gnode.PositionOffset.X },
                    {"PosY", gnode.PositionOffset.Y }
                };

                // If this is a VariableRef node, we should probably save its ReferenceID too!
                if (gnode is VariableRef vref)
                {
                    nodeData.Add("VarRefID", vref.ReferenceID);
                }

                nodesData.Add(nodeData);
            }
        }

        // 3. Serialize Connections
        var csharpConns = new List<Dictionary<string, object>>();
        foreach (Godot.Collections.Dictionary conn in GetConnectionList())
        {
            csharpConns.Add(new Dictionary<string, object>
            {
                { "from_node", conn["from_node"].AsStringName().ToString() },
                { "from_port", conn["from_port"].AsInt64() },
                { "to_node", conn["to_node"].AsStringName().ToString() },
                { "to_port", conn["to_port"].AsInt64() }
            });
        }

        // Combine everything
        var saveData = new Dictionary<string, object>
        {
            { "Variables", varsData },
            { "Nodes", nodesData },
            { "Connections", csharpConns }
        };

        string jstring = JsonSerializer.Serialize(saveData, new JsonSerializerOptions { WriteIndented = true });

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Write);
        if (file != null)
        {
            file.StoreString(jstring);
        }
    }

    public void LoadGraph(string path)
    {
        if (!FileAccess.FileExists(path)) return;

        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) return;

        string jsonString = file.GetAsText();
        var saveData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonString);

        // --- 1. LOAD VARIABLES FIRST ---
        // We load these first so that when nodes are created, the Variables list is already populated
        if (saveData.TryGetValue("Variables", out JsonElement varsElement))
        {
            Variables.Clear();
            foreach (JsonElement varElement in varsElement.EnumerateArray())
            {
                Variables.Add(new IVar
                {
                    Name = varElement.GetProperty("Name").GetString(),
                    id = varElement.GetProperty("id").GetInt32(),
                    type = (Variant.Type)varElement.GetProperty("type").GetInt32(),
                    // Convert the string back into a proper Godot Variant
                    value = GD.StrToVar(varElement.GetProperty("value").GetString())
                });
            }
        }

        // --- 2. CLEAR EXISTING GRAPH ---
        ClearConnections();
        foreach (Node child in GetChildren())
        {
            if (child is CustomGraphNode)
            {
                RemoveChild(child);
                child.QueueFree();
            }
        }

        // --- 3. LOAD NODES ---
        if (saveData.TryGetValue("Nodes", out JsonElement nodesElement))
        {
            foreach (JsonElement nodeElement in nodesElement.EnumerateArray())
            {
                string name = nodeElement.GetProperty("Name").GetString();
                int factoryId = nodeElement.GetProperty("FactoryId").GetInt32();
                float posX = (float)nodeElement.GetProperty("PosX").GetDouble();
                float posY = (float)nodeElement.GetProperty("PosY").GetDouble();

                if (factoryId >= 0 && factoryId < NodeRegistry.NodeFactories.Count)
                {
                    CustomGraphNode graphNode = NodeRegistry.NodeFactories[factoryId].Invoke();
                    graphNode.FactoryId = factoryId;
                    graphNode.Name = name;
                    graphNode.PositionOffset = new Vector2(posX, posY);

                    // Restore the Variable Reference ID if this node is a VarRef
                    if (graphNode is VariableRef vref && nodeElement.TryGetProperty("VarRefID", out JsonElement varRefIdElement))
                    {
                        vref.ReferenceID = varRefIdElement.GetInt32();
                    }

                    AddChild(graphNode);
                }
            }
        }

        // --- 4. LOAD CONNECTIONS ---
        if (saveData.TryGetValue("Connections", out JsonElement connectionsElement))
        {
            foreach (JsonElement connElement in connectionsElement.EnumerateArray())
            {
                string fromNode = connElement.GetProperty("from_node").GetString();
                int fromPort = connElement.GetProperty("from_port").GetInt32();
                string toNode = connElement.GetProperty("to_node").GetString();
                int toPort = connElement.GetProperty("to_port").GetInt32();

                OnConnectionRequest(fromNode, fromPort, toNode, toPort);
            }
        }
    }
}