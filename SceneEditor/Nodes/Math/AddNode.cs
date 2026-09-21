using Godot;

namespace UGGR.SceneEditor.Nodes
{
    public partial class AddNode : CustomGraphNode
    {
        public override void _Ready()
        {
            NodeType = NodeTypeEnum.Pure;
            Title = "Add Values (Pure)";
            AddChild(new Label { Text = "A        Result" });
            AddChild(new Label { Text = "B" });
            SetupSlot(0, true, PinTypeEnum.Integer, true, PinTypeEnum.Integer);
            SetupSlot(1, true, PinTypeEnum.Integer, false, PinTypeEnum.Execution);
            InputValues[0] = 0;
            InputValues[1] = 0;
            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.LightGreen.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.LightGreen.Darkened(0.2f)));

        }

        public override void _Process(double delta)
        {
            int a = InputValues.TryGetValue(0, out var valA) && valA is int intA ? intA : 0;
            int b = InputValues.TryGetValue(1, out var valB) && valB is int intB ? intB : 0;

            int result = a + b;
            OutputValues[0] = result;
            PushOutputsToDataPins();
        }
    }
}
