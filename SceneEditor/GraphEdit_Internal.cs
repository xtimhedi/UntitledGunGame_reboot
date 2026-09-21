using Godot;
using Godot.Collections;
using System;
using UGGR.CrashScreen;
using UGGR.SceneEditor;

public partial class GraphEdit_Internal : GraphEdit
{
    public Action<long> OnContextMenuItemSelected;
    public PopupMenu menu = new PopupMenu();
    public int SelfConnectionCount = 0;

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

            // OPTIMIZATION: Cache references directly into the source node's network pipeline
            fromNode.DownstreamConnections.Add(new CustomGraphNode.NodeConnection
            {
                FromPort = (int)fromPort,
                ToNode = toNode,
                ToPort = (int)toPort
            });

            // Force an initial update sync if it's a data line (not an execution flow line)
            if (fromType != (int)PinTypeEnum.Execution)
            {
                toNode.SetInputValue((int)toPort, fromNode.GetOutputValue((int)fromPort));
            }
        }
        else
        {
            OS.Alert("Rejected: Pin types do not match!", "Connection Error");
        }
    }

    private void OnDisconnectionRequest(StringName fromNodeName, long fromPort, StringName toNodeName, long toPort)
    {
        DisconnectNode(fromNodeName, (int)fromPort, toNodeName, (int)toPort);
        CleanUpCustomConnectionCache(fromNodeName, fromPort, toNodeName, toPort);
    }

    // Extracted so it can be called safely during manual node deletions
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

        // Ensure these IDs match the actual index of the operation in NodeRegistry.NodeFactories
        math.AddItem("Add", 0);
        math.AddItem("Subtract", 1);
        math.AddItem("Multiply", 2);
        math.AddItem("Divide", 3);

        menu.AddSubmenuNodeItem("Math", math);

        // Root menu items
        menu.AddItem("Value (int)", 4);
        menu.AddItem("Display (int)", 5);
        menu.AddItem("Branch", 6);
        menu.AddItem("Button", 7);
        menu.AddItem("Exec Test", 8);

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
                AcceptEvent(); // Prevent the GraphEdit from consuming the right-click drag
            }
        }
    }

    public void PlaceNode(int id)
    {
        if (id >= 0 && id < NodeRegistry.NodeFactories.Count)
        {
            CustomGraphNode node = NodeRegistry.NodeFactories[id].Invoke();

            // CORRECT: Calculate placement based on GraphEdit's internal scroll and zoom
            Vector2 localMousePos = GetLocalMousePosition();
            node.PositionOffset = (localMousePos + ScrollOffset) / Zoom;

            AddChild(node);
        }
    }

    private int GetNodeEnginePortType(string nodeName, int portIndex, bool isInput)
    {
        var node = GetNodeOrNull<GraphNode>(nodeName);
        if (node == null) return -1;

        return isInput ? node.GetInputPortType(portIndex) : node.GetOutputPortType(portIndex);
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
            // GODOT 4 FIX: The keys are "from_node" and "to_node"
            StringName fromNode = connection["from_node"].AsStringName();
            StringName toNode = connection["to_node"].AsStringName();

            if (fromNode == nodeName || toNode == nodeName)
            {
                long fromPort = connection["from_port"].AsInt64();
                long toPort = connection["to_port"].AsInt64();

                DisconnectNode(fromNode, (int)fromPort, toNode, (int)toPort);

                // Clean up custom cached references so surviving nodes don't crash
                CleanUpCustomConnectionCache(fromNode, fromPort, toNode, toPort);
            }
        }

        node.QueueFree();
    }
}