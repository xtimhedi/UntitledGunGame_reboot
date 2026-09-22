using Godot;
using Godot.Collections;
using System.Collections.Generic;

namespace UGGR.SceneEditor
{
    public partial class CustomGraphNode : GraphNode
    {
        private const float PortSize = 12.0f;
        public int FactoryId { get; set; } = -1;
        public NodeTypeEnum NodeType { get; set; }

        // Local storage for pin data
        protected System.Collections.Generic.Dictionary<int, object> InputValues = new();
        protected System.Collections.Generic.Dictionary<int, object> OutputValues = new();

        // High-performance direct references for frame tracking
        public List<NodeConnection> DownstreamConnections { get; private set; } = new();

        public struct NodeConnection
        {
            public int FromPort;
            public CustomGraphNode ToNode;
            public int ToPort;
        }

        public Color EnumPinColor(PinTypeEnum pinType)
        {
            switch (pinType)
            {
                default: return new Color(1, 1, 1, 1);
                case PinTypeEnum.Execution: return new Color(0.6f, 0.6f, 0.6f, 1); // Green/Grey for exec lines
                case PinTypeEnum.Boolean: return Colors.DarkRed;
                case PinTypeEnum.Integer: return Colors.Teal;
                case PinTypeEnum.Vector3: return Colors.Yellow;
                case PinTypeEnum.Vector2: return Colors.Green;
                case PinTypeEnum.String: return Colors.Magenta;
                case PinTypeEnum.Reference: return Colors.Blue;
                case PinTypeEnum.Transform: return Colors.Orange;
            }
        }

        public override void _Ready()
        {
        }

        public void SetupSlot(int slotIndex, bool leftEnabled, PinTypeEnum leftType, bool rightEnabled, PinTypeEnum rightType)
        {
            SetSlot(slotIndex,
                leftEnabled, (int)leftType, EnumPinColor(leftType),
                rightEnabled, (int)rightType, EnumPinColor(rightType));
        }

        // Sets values pushed into this node from upstream
        public void SetInputValue(int portIndex, object value)
        {
            InputValues[portIndex] = value;
        }

        // Helper to push cached output values downstream across data lines
        protected void PushOutputsToDataPins()
        {
            foreach (var conn in DownstreamConnections)
            {
                // Verify we're passing data, not triggering execution lines here
                if (GetOutputPortType(conn.FromPort) != (int)PinTypeEnum.Execution)
                {
                    if (OutputValues.TryGetValue(conn.FromPort, out var val))
                    {
                        conn.ToNode.SetInputValue(conn.ToPort, val);
                    }
                }
            }
        }

        // ExecFlow nodes override this to trace execution paths downstream
        protected void TriggerNextExecNode(int outputExecPortIndex)
        {
            foreach (var conn in DownstreamConnections)
            {
                if (conn.FromPort == outputExecPortIndex && conn.ToNode.NodeType != NodeTypeEnum.Pure)
                {
                    conn.ToNode.Execute(conn.ToPort);
                    break; // Execution paths are typically singular branching lines
                }
            }
        }

        // Virtual hooks for children
        public virtual object GetOutputValue(int outputPortIndex) => OutputValues.TryGetValue(outputPortIndex, out var val) ? val : null;
        public virtual void Execute(int inputExecPortIndex) { }
        public override void _DrawPort(int slotIndex, Vector2I pos, bool left, Color color)
        {
            // 1. Get the slot type depending on whether it's an input (left) or output (right) port
            int slotType = left ? GetSlotTypeLeft(slotIndex) : GetSlotTypeRight(slotIndex);

            // 2. Check if it's an execution pin
            if (slotType == (int)PinTypeEnum.Execution)
            {
                // Draw Triangle
                Vector2[] basePoints = new Vector2[]
                {
                    new Vector2(0.5f, 0.0f),   // Tip pointing right
                    new Vector2(-0.5f, -0.5f), // Bottom left
                    new Vector2(-0.5f, 0.5f)   // Top left
                };

                Vector2[] finalPoints = new Vector2[3];
                for (int i = 0; i < 3; i++)
                {
                    finalPoints[i] = (basePoints[i] * PortSize) + pos;
                }

                DrawColoredPolygon(finalPoints, color);
            }
            else if (slotType == (int)PinTypeEnum.Boolean)
            {
                DrawRect(new Rect2(pos - new Vector2I((int)PortSize / 2, (int)PortSize / 2), new Vector2(PortSize, PortSize)), color);

                DrawRect(new Rect2(pos - new Vector2I(((int)PortSize / 2) - 2, ((int)PortSize / 2) - 2), new Vector2(PortSize - 4, PortSize - 4)), color.Darkened(0.28f));
            }
            else
            {
                // Draw Circle
                float radius = PortSize * 0.35f; // Adjust radius as needed
                DrawCircle(pos, radius, color);
            }
        }
    }
}
