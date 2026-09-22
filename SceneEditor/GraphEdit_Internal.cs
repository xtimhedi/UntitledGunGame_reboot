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
        menu.AddItem("Var Ref", 10);
        menu.AddItem("Var Get", 11);

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

            // Track the ID so we know what factory to use when loading later
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
                RemoveChild(child); // Crucial! Frees the name immediately so loaded nodes can use it
                child.QueueFree();
            }
        }
    }

    public void SaveGraph(string path)
    {
        var nodesData = new List<Dictionary<string, object>>();

        foreach (Node child in GetChildren())
        {
            if (child is CustomGraphNode gnode)
            {
                var nodeData = new Dictionary<string, object>
                {
                    {"Name", gnode.Name.ToString() },
                    {"FactoryId", gnode.FactoryId }, // Save the type so we can restore it!
                    {"PosX", gnode.PositionOffset.X },
                    {"PosY", gnode.PositionOffset.Y }
                };
                nodesData.Add(nodeData);
            }
        }

        // Map Godot Collections to pure C# lists/dictionaries so System.Text.Json doesn't break
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

        var saveData = new Dictionary<string, object>
        {
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

        ClearConnections();

        foreach (Node child in GetChildren())
        {
            if (child is CustomGraphNode)
            {
                RemoveChild(child); // Crucial! Frees the name immediately so loaded nodes can use it
                child.QueueFree();
            }
        }

        if (saveData.TryGetValue("Nodes", out JsonElement nodesElement))
        {
            foreach (JsonElement nodeElement in nodesElement.EnumerateArray())
            {
                string name = nodeElement.GetProperty("Name").GetString();
                int factoryId = nodeElement.GetProperty("FactoryId").GetInt32();
                float posX = (float)nodeElement.GetProperty("PosX").GetDouble(); // Fixed typos
                float posY = (float)nodeElement.GetProperty("PosY").GetDouble(); // Fixed typos

                // Re-create the specific subclass via the registry!
                if (factoryId >= 0 && factoryId < NodeRegistry.NodeFactories.Count)
                {
                    CustomGraphNode graphNode = NodeRegistry.NodeFactories[factoryId].Invoke();
                    graphNode.FactoryId = factoryId;
                    graphNode.Name = name;
                    graphNode.PositionOffset = new Vector2(posX, posY);

                    AddChild(graphNode);
                }
            }
        }

        if (saveData.TryGetValue("Connections", out JsonElement connectionsElement))
        {
            foreach (JsonElement connElement in connectionsElement.EnumerateArray())
            {
                string fromNode = connElement.GetProperty("from_node").GetString();
                int fromPort = connElement.GetProperty("from_port").GetInt32();
                string toNode = connElement.GetProperty("to_node").GetString();
                int toPort = connElement.GetProperty("to_port").GetInt32();

                // Call YOUR method, not ConnectNode, so your `DownstreamConnections` cache gets rebuilt
                OnConnectionRequest(fromNode, fromPort, toNode, toPort);
            }
        }
    }
}