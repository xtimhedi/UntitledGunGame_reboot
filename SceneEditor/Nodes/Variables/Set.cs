using System;
using System.Collections.Generic;
using Godot;

namespace UGGR.SceneEditor.Nodes.Variables
{
    public partial class Set : CustomGraphNode
    {
        public override void _Ready()
        {
            Title = "Set (Impure)";
            NodeType = NodeTypeEnum.Impure;

            AddChild(new Label { Text = "Exec" });
            AddChild(new Label { Text = "Reference" });
            AddChild(new Label { Text = "Value" });

            SetupSlot(0, true, PinTypeEnum.Execution, true, PinTypeEnum.Execution);
            SetupSlot(1, true, PinTypeEnum.Reference, false, 0);
            SetupSlot(2, false, 0, false, 0); // Starts disabled until reference dictates the type

            AddThemeStyleboxOverride("titlebar", NodeMaterials.Titlebar(Colors.LightBlue.Darkened(0.2f)));
            AddThemeStyleboxOverride("titlebar_selected", NodeMaterials.TitlebarSelected(Colors.LightBlue.Darkened(0.2f)));
        }

        private PinTypeEnum MatchType(Variant.Type type)
        {
            return type switch
            {
                Variant.Type.Bool => PinTypeEnum.Boolean,
                Variant.Type.Int => PinTypeEnum.Integer,
                Variant.Type.Vector3 => PinTypeEnum.Vector3,
                Variant.Type.Vector2 => PinTypeEnum.Vector2,
                Variant.Type.String => PinTypeEnum.String,
                Variant.Type.Transform3D => PinTypeEnum.Transform,
                Variant.Type.Object => PinTypeEnum.Reference,
                _ => 0
            };
        }

        // Safely extract integers even if boxed inside a Godot.Variant
        private int ParseIntForgiving(object obj)
        {
            if (obj is Variant v) return v.AsInt32();
            return Convert.ToInt32(obj);
        }

        // Safely map C# objects into Godot Variants using implicit operators
        private Variant ObjectToVariant(object obj)
        {
            if (obj == null) return new Variant();
            if (obj is Variant v) return v;

            return obj switch
            {
                int i => i,
                long l => l,
                float f => f,
                double d => d,
                bool b => b,
                string s => s,
                Vector2 v2 => v2,
                Vector3 v3 => v3,
                Transform3D t => t,
                GodotObject go => go,
                _ => default
            };
        }

        public void UpdateVariableBinding()
        {
            GD.Print("\n--- UPDATE VARIABLE BINDING TRIGGERED ---");

            if (InputValues.TryGetValue(1, out object obj))
            {
                GD.Print($"[Set Node] Success: InputValues[1] contains: {obj} (Type: {obj?.GetType()})");
                try
                {
                    int val = ParseIntForgiving(obj);
                    GD.Print($"[Set Node] Parsed ID as: {val}");

                    if (val >= 0 && val < GraphEdit_Internal.Variables.Count && GraphEdit_Internal.Variables[val] != null)
                    {
                        IVar varC = GraphEdit_Internal.Variables[val];
                        GD.Print($"[Set Node] Found Variable! Name: '{varC.Name}', Type: {varC.type}. Enabling Slot 2...");

                        // Try to set the slot
                        SetupSlot(2, true, MatchType(varC.type), true, MatchType(varC.type));
                        GD.Print("[Set Node] SetupSlot(2) called successfully.");
                        return;
                    }
                    else
                    {
                        GD.PrintErr($"[Set Node] Variable index {val} is OUT OF BOUNDS. List Size: {GraphEdit_Internal.Variables.Count}");
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"[Set Node] Failed to parse obj to int. Error: {e.Message}");
                }
            }
            else
            {
                GD.PrintErr("[Set Node] FAILED: InputValues[1] is empty/missing! The graph hasn't passed the value yet.");
            }

            GD.Print("[Set Node] Fallback reached: Disabling Slot 2.");
            SetupSlot(2, false, 0, false, 0);
        }

        public override void Execute(int inputExecPortIndex)
        {
            GD.Print("[Set Node] Execute triggered.");

            if (InputValues.TryGetValue(1, out object obj))
            {
                try
                {
                    int val = ParseIntForgiving(obj);

                    if (val >= 0 && val < GraphEdit_Internal.Variables.Count && GraphEdit_Internal.Variables[val] != null)
                    {
                        IVar varC = GraphEdit_Internal.Variables[val];

                        if (InputValues.TryGetValue(2, out object newValObj))
                        {
                            // SAFELY convert the incoming object to a Godot Variant
                            varC.value = ObjectToVariant(newValObj);

                            GD.Print($"[Set Node] Found Variable '{varC.Name}' -> Set Value to: {varC.value}");

                            OutputValues[2] = varC.value;
                            PushOutputsToDataPins();

                            GD.Print("[Set Node] Success! Pushed to OutputValues[2].");
                        }
                        else
                        {
                            GD.PrintErr($"[Set Node] No data found in InputValues[2]! Cannot set variable.");
                        }
                    }
                    else
                    {
                        GD.PrintErr($"[Set Node] Variable index {val} is out of bounds or null.");
                    }
                }
                catch (Exception e)
                {
                    GD.PrintErr($"[Set Node] Failed to process. Error: {e.Message}");
                }
            }
            else
            {
                GD.PrintErr("[Set Node] No data found in InputValues[1]! Is the ID connected or set?");
            }

            TriggerNextExecNode(0);
        }
    }
}