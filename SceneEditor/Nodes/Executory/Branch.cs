using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor.Nodes.Executory
{
    public partial class Branch : CustomGraphNode
    {
        public override void _Ready()
        {
            NodeType = NodeTypeEnum.FlowControl;
            Title = "Branch (Impure)";
            AddChild(new Label { Text = "Exec      true" });
            AddChild(new Label { Text = "Bool     false" });
            SetupSlot(0, true, PinTypeEnum.Execution, true, PinTypeEnum.Execution);
            SetupSlot(1, true, PinTypeEnum.Boolean, true, PinTypeEnum.Execution);
            InputValues[0] = 0;
            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.Gray.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.Gray.Darkened(0.2f)));
        }
        public override void Execute(int inputExecPortIndex)
        {
            // First, check if there is an input value on port 1
            bool hasValue = InputValues.TryGetValue(1, out var val);

            // Then, parse that value as a boolean (assuming 'val' is an object or Variant)
            bool condition = false;
            if (hasValue && val is bool boolValue)
            {
                condition = boolValue;
            }
            // Alternatively, if 'val' might be an integer (0 or 1), you could use:
            // condition = hasValue && Convert.ToBoolean(val);

            // Finally, branch based on the actual condition
            if (condition)
            {
                TriggerNextExecNode(0); // True output port
            }
            else
            {
                TriggerNextExecNode(1); // False output port
            }
        }
    }
}
