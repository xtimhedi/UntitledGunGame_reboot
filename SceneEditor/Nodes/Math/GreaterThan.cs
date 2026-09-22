using Godot;

namespace UGGR.SceneEditor.Nodes
{
    public partial class GreaterThan : CustomGraphNode
    {
        public override void _Ready()
        {
            NodeType = NodeTypeEnum.Impure;
            Title = "Greater Than (Impure)";
            AddChild(new Label { Text = "Exec" });
            AddChild(new Label { Text = "A        Result" });
            AddChild(new Label { Text = "B" });
            SetupSlot(0, true, PinTypeEnum.Execution, true, PinTypeEnum.Execution);
            SetupSlot(1, true, PinTypeEnum.Integer, true, PinTypeEnum.Boolean);
            SetupSlot(2, true, PinTypeEnum.Integer, false, 0);
            InputValues[1] = 0;
            InputValues[2] = 0;
            OutputValues[1] = false;
            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.LightGreen.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.LightGreen.Darkened(0.2f)));

        }

        public override void Execute(int inputExecPortIndex)
        {
            int a = InputValues.TryGetValue(1, out var valA) && valA is int intA ? intA : 0;
            int b = InputValues.TryGetValue(2, out var valB) && valB is int intB ? intB : 0;

            bool result = a > b;
            OutputValues[1] = result;
            PushOutputsToDataPins();
            TriggerNextExecNode(0);
        }
    }
}
