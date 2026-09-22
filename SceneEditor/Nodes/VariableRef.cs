using Godot;

namespace UGGR.SceneEditor.Nodes
{
    public partial class VariableRef : CustomGraphNode
    {
        public int ReferenceID = 0;
        public override void _Ready()
        {
            NodeType = NodeTypeEnum.Pure;
            Title = "";
            AddChild(new Label { Text = "  " + GraphEdit_Internal.Variables[ReferenceID].Name + " " });
            SetupSlot(0, false, 0, true, PinTypeEnum.Reference);
            AddThemeStyleboxOverride("titlebar", new StyleBoxEmpty());
            AddThemeStyleboxOverride("titlebar_selected", new StyleBoxEmpty());
            AddThemeStyleboxOverride("panel", NodeMaterials.VarTitlebarSelected(Colors.LightGreen.Darkened(0.2f)));
            AddThemeStyleboxOverride("panel_selected", NodeMaterials.VarTitlebar(Colors.LightGreen.Darkened(0.2f)));

        }

        public override void _Process(double delta)
        {
            OutputValues[0] = ReferenceID; // NOTE: we do parse this as a Reference, even though it just references back to the list
            PushOutputsToDataPins();
        }
    }
}
