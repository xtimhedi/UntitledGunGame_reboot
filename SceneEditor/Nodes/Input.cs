using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor.Nodes
{
    public partial class Input : CustomGraphNode
    {
        LineEdit NumEdit = new LineEdit();
        CheckBox BoolEdit = new CheckBox();
        public override void _Ready()
        {
            NodeType = NodeTypeEnum.Pure;
            Title = "Input (Pure)";
            AddChild(NumEdit);
            AddChild(BoolEdit);
            SetupSlot(0, false, 0, true, PinTypeEnum.Integer);
            SetupSlot(1, false, 0, true, PinTypeEnum.Boolean);
            InputValues[0] = 0;
            InputValues[1] = 0;
            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.LightGreen.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.LightGreen.Darkened(0.2f)));
        }
        public override void _Process(double delta)
        {
            int.TryParse(NumEdit.Text, out int vala);
            
            OutputValues[0] = vala;
            OutputValues[1] = BoolEdit.ButtonPressed;

            PushOutputsToDataPins();
        }


    }
}
