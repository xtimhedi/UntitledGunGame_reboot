using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor.Nodes
{
    public partial class Display : CustomGraphNode
    {
        Label display0 = new Label();
        Label display1 = new Label();
        public override void _Ready()
        {
            NodeType = NodeTypeEnum.Pure;
            Title = "Display (Pure)";
            AddChild(display0);
            AddChild(display1);
            SetupSlot(0, true, PinTypeEnum.Integer, false, 0);
            SetupSlot(1, true, PinTypeEnum.Boolean, false, 0);
            InputValues[0] = 0;
            InputValues[1] = false;
            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.LightGreen.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.LightGreen.Darkened(0.2f)));
        }

        public override void _Process(double delta)
        {
            int a = InputValues.TryGetValue(0, out var valA) && valA is int intA ? intA : 0;
            bool b = InputValues.TryGetValue(1, out var valB) && valB is bool boolA ? boolA : false;
            display0.Text = a.ToString();
            display1.Text = b.ToString();
            PushOutputsToDataPins();
        }
    }

}
