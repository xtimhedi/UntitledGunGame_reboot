using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor.Nodes.Executory
{
    public partial class ExecTest : CustomGraphNode
    {
        ColorRect rect = new ColorRect();
        public override void _Ready()
        {
            NodeType = NodeTypeEnum.FlowControl;
            Title = "Branch (Impure)";
            AddChild(rect);
            rect.CustomMinimumSize = new Vector2(32, 32);
            SetupSlot(0, true, PinTypeEnum.Execution, false, 0);
            InputValues[0] = 0;
            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.Gray.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.Gray.Darkened(0.2f)));
        }
        public override void Execute(int inputExecPortIndex)
        {
            rect.Color = new Color((float)Random.Shared.NextDouble(), (float)Random.Shared.NextDouble(), (float)Random.Shared.NextDouble(), 1.0f);
        }
    }
}
