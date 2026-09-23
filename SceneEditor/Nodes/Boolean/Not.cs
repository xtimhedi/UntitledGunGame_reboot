using Godot;

namespace UGGR.SceneEditor.Nodes
{
    public partial class Not : CustomGraphNode
    {
        public override void _Ready()
        {
            NodeType = NodeTypeEnum.Pure;
            Title = "Not (Pure)";
            AddChild(new Label { Text = "A        O" });
            SetupSlot(0, true, PinTypeEnum.Boolean, true, PinTypeEnum.Boolean);
            InputValues[0] = false;
            InputValues[1] = false;
            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.LightGreen.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.LightGreen.Darkened(0.2f)));

        }

        public override void _Process(double delta)
        {
            bool a = InputValues.TryGetValue(0, out var valA) && valA is bool intA ? intA : false ;

            bool result = !a;
            OutputValues[0] = result;
            PushOutputsToDataPins();
        }
    }
}
