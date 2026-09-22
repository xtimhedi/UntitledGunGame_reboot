using Godot;
using System;
using UGGR.SceneEditor.Nodes;

namespace UGGR.SceneEditor.Editor
{
    public partial class VariableVisualizer : HBoxContainer
    {
        public event Action<IVar> OnIVarChanged;

        private IVar _iVar;
        private Label _titleLabel;
        private Control _valueControl;
        private Button _placeNodeButton;

        public IVar BoundVar
        {
            get => _iVar;
            set
            {
                _iVar = value;
                if (_titleLabel != null && _iVar != null)
                {
                    _titleLabel.Text = _iVar.Name;
                }
                RebuildValueEditor();
            }
        }

        public VariableVisualizer() { }

        public VariableVisualizer(IVar ivar)
        {
            _iVar = ivar;
        }

        public override void _Ready()
        {
            _titleLabel = new Label
            {
                Text = _iVar?.Name ?? "",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            AddChild(_titleLabel);

            RebuildValueEditor();

            // Create and add the Place Node button
            _placeNodeButton = new Button
            {
                Text = "+ Node"
            };
            _placeNodeButton.Pressed += OnPlaceNodePressed;
            AddChild(_placeNodeButton);
        }

        private void OnPlaceNodePressed()
        {
            if (_iVar != null)
            {
                Script.Editor.PlaceNode(10, _iVar.id);
            }
        }

        public void RebuildValueEditor()
        {
            if (_titleLabel == null) return; // Node not ready in tree yet

            if (_valueControl != null)
            {
                _valueControl.QueueFree();
                _valueControl = null;
            }

            if (_iVar == null) return;

            _valueControl = CreateEditorForVariant(_iVar.type, _iVar.value);
            _valueControl.SizeFlagsHorizontal = SizeFlags.ExpandFill;

            // Insert value control before the button if it already exists
            if (_placeNodeButton != null)
            {
                AddChild(_valueControl);
                MoveChild(_valueControl, GetChildCount() - 2);
            }
            else
            {
                AddChild(_valueControl);
            }
        }

        private Control CreateEditorForVariant(Variant.Type variantType, Variant currentVal)
        {
            switch (variantType)
            {
                case Variant.Type.Bool:
                    var checkBox = new CheckBox { ButtonPressed = (bool)currentVal };
                    checkBox.Toggled += toggled => UpdateValue(toggled);
                    return checkBox;

                case Variant.Type.Int:
                    var intSpin = new SpinBox
                    {
                        MinValue = int.MinValue,
                        MaxValue = int.MaxValue,
                        Step = 1,
                        Value = (int)currentVal,
                        Rounded = true
                    };
                    intSpin.ValueChanged += v => UpdateValue((int)v);
                    return intSpin;

                case Variant.Type.Float:
                    var floatSpin = new SpinBox
                    {
                        MinValue = -1e9,
                        MaxValue = 1e9,
                        Step = 0.001,
                        Value = (double)currentVal
                    };
                    floatSpin.ValueChanged += v => UpdateValue((float)v);
                    return floatSpin;

                case Variant.Type.String:
                case Variant.Type.NodePath:
                    var lineEdit = new LineEdit { Text = (string)currentVal };
                    lineEdit.TextChanged += text => UpdateValue(text);
                    return lineEdit;

                case Variant.Type.Color:
                    var colorPicker = new ColorPickerButton { Color = (Color)currentVal };
                    colorPicker.ColorChanged += newColor => UpdateValue(newColor);
                    return colorPicker;

                case Variant.Type.Vector2:
                    return CreateVector2Editor((Vector2)currentVal);

                case Variant.Type.Vector2I:
                    return CreateVector2IEditor((Vector2I)currentVal);

                case Variant.Type.Vector3:
                    return CreateVector3Editor((Vector3)currentVal);

                case Variant.Type.Vector3I:
                    return CreateVector3IEditor((Vector3I)currentVal);

                case Variant.Type.Vector4:
                    return CreateVector4Editor((Vector4)currentVal);

                case Variant.Type.Nil:
                default:
                    return new LineEdit
                    {
                        Text = currentVal.ToString(),
                        Editable = false
                    };
            }
        }

        private Control CreateVector2Editor(Vector2 current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Vector2((float)spinX.Value, (float)spinY.Value));

            spinX.ValueChanged += _ => Update();
            spinY.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" });
            container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" });
            container.AddChild(spinY);
            return container;
        }

        private Control CreateVector2IEditor(Vector2I current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Vector2I((int)spinX.Value, (int)spinY.Value));

            spinX.ValueChanged += _ => Update();
            spinY.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" });
            container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" });
            container.AddChild(spinY);
            return container;
        }

        private Control CreateVector3Editor(Vector3 current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinZ = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Z, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Vector3((float)spinX.Value, (float)spinY.Value, (float)spinZ.Value));

            spinX.ValueChanged += _ => Update();
            spinY.ValueChanged += _ => Update();
            spinZ.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" });
            container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" });
            container.AddChild(spinY);
            container.AddChild(new Label { Text = "Z:" });
            container.AddChild(spinZ);
            return container;
        }

        private Control CreateVector3IEditor(Vector3I current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinZ = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Z, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Vector3I((int)spinX.Value, (int)spinY.Value, (int)spinZ.Value));

            spinX.ValueChanged += _ => Update();
            spinY.ValueChanged += _ => Update();
            spinZ.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" });
            container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" });
            container.AddChild(spinY);
            container.AddChild(new Label { Text = "Z:" });
            container.AddChild(spinZ);
            return container;
        }

        private Control CreateVector4Editor(Vector4 current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinZ = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Z, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinW = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.W, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Vector4((float)spinX.Value, (float)spinY.Value, (float)spinZ.Value, (float)spinW.Value));

            spinX.ValueChanged += _ => Update();
            spinY.ValueChanged += _ => Update();
            spinZ.ValueChanged += _ => Update();
            spinW.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" });
            container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" });
            container.AddChild(spinY);
            container.AddChild(new Label { Text = "Z:" });
            container.AddChild(spinZ);
            container.AddChild(new Label { Text = "W:" });
            container.AddChild(spinW);
            return container;
        }

        private void UpdateValue(Variant newValue)
        {
            if (_iVar != null)
            {
                _iVar.value = newValue;
                OnIVarChanged?.Invoke(_iVar);
            }
        }
    }
}