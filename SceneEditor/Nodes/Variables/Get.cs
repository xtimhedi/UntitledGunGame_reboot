using System;
using System.Collections.Generic;
using Godot;

namespace UGGR.SceneEditor.Nodes.Variables
{
    public partial class Get : CustomGraphNode
    {
        public override void _Ready()
        {
            Title = "Get (Impure)";
            NodeType = NodeTypeEnum.Impure;

            AddChild(new Label { Text = "Exec" });
            AddChild(new Label { Text = "Reference" });

            SetupSlot(0, true, PinTypeEnum.Execution, true, PinTypeEnum.Execution);
            SetupSlot(1, true, PinTypeEnum.Reference, false, 0);

            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.LightBlue.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.LightBlue.Darkened(0.2f)));
        }

        private PinTypeEnum MatchType(Variant.Type type)
        {
            return type switch
            {
                Variant.Type.String => PinTypeEnum.String,
                Variant.Type.Bool => PinTypeEnum.Boolean,
                Variant.Type.Int => PinTypeEnum.Integer,
                _ => 0
            };
        }

        public void UpdateVariableBinding()
        {
            if (InputValues.TryGetValue(1, out object obj) && obj is int val)
            {
                if (val >= 0 && val < GraphEdit_Internal.Variables.Count && GraphEdit_Internal.Variables[val] != null)
                {
                    IVar varC = GraphEdit_Internal.Variables[val];
                    SetupSlot(1, true, PinTypeEnum.Reference, true, MatchType(varC.type));
                }
                else
                {
                    SetupSlot(1, true, PinTypeEnum.Reference, false, 0);
                }
            }
        }

        public override void Execute(int inputExecPortIndex)
        {
            GD.Print("[Get Node] Execute triggered.");

            // 1. Check if the input pin is actually receiving data
            if (InputValues.TryGetValue(1, out object obj))
            {
                GD.Print($"[Get Node] Input slot 1 received: {obj} (Type: {obj?.GetType()})");

                try
                {
                    // 2. FORGIVING CAST: Handles long, double, string, or Godot Variant safely
                    int val = Convert.ToInt32(obj);

                    // 3. Bounds check
                    if (val >= 0 && val < GraphEdit_Internal.Variables.Count && GraphEdit_Internal.Variables[val] != null)
                    {
                        IVar varC = GraphEdit_Internal.Variables[val];
                        GD.Print($"[Get Node] Found Variable '{varC.Name}' -> Value: {varC.value}");

                        // 4. Output assignment
                        OutputValues[1] = varC.value;
                        PushOutputsToDataPins();

                        GD.Print("[Get Node] Success! Pushed to OutputValues[1].");
                    }
                    else
                    {
                        GD.PrintErr($"[Get Node] Variable index {val} is out of bounds or null. (List Size: {GraphEdit_Internal.Variables.Count})");
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"[Get Node] Failed to convert '{obj}' to int. Error: {e.Message}");
                }
            }
            else
            {
                GD.PrintErr("[Get Node] No data found in InputValues[1]! Is the ID connected or set?");
            }

            TriggerNextExecNode(0);
        }
    }
}