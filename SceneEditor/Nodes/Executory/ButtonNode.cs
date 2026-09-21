using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor.Nodes.Executory
{
    public partial class ButtonNode : CustomGraphNode
    {
        public override void _Ready()
        {
            Button button = new Button { Text = "Exec" };
            NodeType = NodeTypeEnum.Impure;
            Title = "Button (Impure)";
            AddChild(button);
            SetupSlot(0, false, 0, true, PinTypeEnum.Execution);
            InputValues[0] = 0;
            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.Gray.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.Gray.Darkened(0.2f)));
            button.Pressed += ButtonPressed;
        }
        public void ButtonPressed()
        {
            TriggerNextExecNode(0);
        }
    }
}
