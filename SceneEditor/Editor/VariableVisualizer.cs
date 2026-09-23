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

        // Tracks external data changes
        private Variant _lastKnownValue;
        private Variant.Type _lastKnownType;
        private Action _syncUIAction;

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

        // --- NEW: Polling loop to catch external changes ---
        public override void _Process(double delta)
        {
            if (_iVar == null) return;

            // If the TYPE changed externally, we have to completely rebuild the UI
            if (_iVar.type != _lastKnownType)
            {
                RebuildValueEditor();
                return;
            }

            // If the VALUE changed externally, update existing UI controls gracefully
            if (!_iVar.value.Equals(_lastKnownValue))
            {
                _lastKnownValue = _iVar.value;
                _syncUIAction?.Invoke();
            }
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

            _syncUIAction = null; // Clear old UI sync action

            if (_iVar == null) return;

            _lastKnownType = _iVar.type;
            _lastKnownValue = _iVar.value;

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
                    _syncUIAction = () => checkBox.SetPressedNoSignal((bool)_iVar.value);
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
                    _syncUIAction = () => intSpin.SetValueNoSignal((int)_iVar.value);
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
                    _syncUIAction = () => floatSpin.SetValueNoSignal((double)_iVar.value);
                    return floatSpin;

                case Variant.Type.String:
                case Variant.Type.StringName:
                case Variant.Type.NodePath:
                    var lineEdit = new LineEdit { Text = (string)currentVal };
                    lineEdit.TextChanged += text => UpdateValue(text);
                    _syncUIAction = () => {
                        if (!lineEdit.HasFocus()) // Don't interrupt the user if they are currently typing!
                            lineEdit.Text = (string)_iVar.value;
                    };
                    return lineEdit;

                case Variant.Type.Color:
                    var colorPicker = new ColorPickerButton { Color = (Color)currentVal };
                    colorPicker.ColorChanged += newColor => UpdateValue(newColor);
                    _syncUIAction = () => colorPicker.Color = (Color)_iVar.value; // Godot doesn't trigger signals on code assignment for this
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

                case Variant.Type.Vector4I:
                    return CreateVector4IEditor((Vector4I)currentVal);

                case Variant.Type.Rect2:
                    return CreateRect2Editor((Rect2)currentVal);

                case Variant.Type.Rect2I:
                    return CreateRect2IEditor((Rect2I)currentVal);

                case Variant.Type.Quaternion:
                    return CreateQuaternionEditor((Quaternion)currentVal);

                case Variant.Type.Plane:
                    return CreatePlaneEditor((Plane)currentVal);

                case Variant.Type.Nil:
                default:
                    var readOnlyEdit = new LineEdit { Text = currentVal.ToString(), Editable = false };
                    _syncUIAction = () => readOnlyEdit.Text = _iVar.value.ToString();
                    return readOnlyEdit;
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

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);

            _syncUIAction = () => {
                var val = (Vector2)_iVar.value;
                spinX.SetValueNoSignal(val.X);
                spinY.SetValueNoSignal(val.Y);
            };
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

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);

            _syncUIAction = () => {
                var val = (Vector2I)_iVar.value;
                spinX.SetValueNoSignal(val.X);
                spinY.SetValueNoSignal(val.Y);
            };
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

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);
            container.AddChild(new Label { Text = "Z:" }); container.AddChild(spinZ);

            _syncUIAction = () => {
                var val = (Vector3)_iVar.value;
                spinX.SetValueNoSignal(val.X);
                spinY.SetValueNoSignal(val.Y);
                spinZ.SetValueNoSignal(val.Z);
            };
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

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);
            container.AddChild(new Label { Text = "Z:" }); container.AddChild(spinZ);

            _syncUIAction = () => {
                var val = (Vector3I)_iVar.value;
                spinX.SetValueNoSignal(val.X);
                spinY.SetValueNoSignal(val.Y);
                spinZ.SetValueNoSignal(val.Z);
            };
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

            spinX.ValueChanged += _ => Update(); spinY.ValueChanged += _ => Update();
            spinZ.ValueChanged += _ => Update(); spinW.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);
            container.AddChild(new Label { Text = "Z:" }); container.AddChild(spinZ);
            container.AddChild(new Label { Text = "W:" }); container.AddChild(spinW);

            _syncUIAction = () => {
                var val = (Vector4)_iVar.value;
                spinX.SetValueNoSignal(val.X); spinY.SetValueNoSignal(val.Y);
                spinZ.SetValueNoSignal(val.Z); spinW.SetValueNoSignal(val.W);
            };
            return container;
        }

        private Control CreateVector4IEditor(Vector4I current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinZ = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Z, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinW = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.W, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Vector4I((int)spinX.Value, (int)spinY.Value, (int)spinZ.Value, (int)spinW.Value));

            spinX.ValueChanged += _ => Update(); spinY.ValueChanged += _ => Update();
            spinZ.ValueChanged += _ => Update(); spinW.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);
            container.AddChild(new Label { Text = "Z:" }); container.AddChild(spinZ);
            container.AddChild(new Label { Text = "W:" }); container.AddChild(spinW);

            _syncUIAction = () => {
                var val = (Vector4I)_iVar.value;
                spinX.SetValueNoSignal(val.X); spinY.SetValueNoSignal(val.Y);
                spinZ.SetValueNoSignal(val.Z); spinW.SetValueNoSignal(val.W);
            };
            return container;
        }

        private Control CreateRect2Editor(Rect2 current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Position.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Position.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinW = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Size.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinH = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Size.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Rect2((float)spinX.Value, (float)spinY.Value, (float)spinW.Value, (float)spinH.Value));

            spinX.ValueChanged += _ => Update(); spinY.ValueChanged += _ => Update();
            spinW.ValueChanged += _ => Update(); spinH.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);
            container.AddChild(new Label { Text = "W:" }); container.AddChild(spinW);
            container.AddChild(new Label { Text = "H:" }); container.AddChild(spinH);

            _syncUIAction = () => {
                var val = (Rect2)_iVar.value;
                spinX.SetValueNoSignal(val.Position.X); spinY.SetValueNoSignal(val.Position.Y);
                spinW.SetValueNoSignal(val.Size.X); spinH.SetValueNoSignal(val.Size.Y);
            };
            return container;
        }

        private Control CreateRect2IEditor(Rect2I current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Position.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Position.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinW = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Size.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinH = new SpinBox { MinValue = int.MinValue, MaxValue = int.MaxValue, Step = 1, Rounded = true, Value = current.Size.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Rect2I((int)spinX.Value, (int)spinY.Value, (int)spinW.Value, (int)spinH.Value));

            spinX.ValueChanged += _ => Update(); spinY.ValueChanged += _ => Update();
            spinW.ValueChanged += _ => Update(); spinH.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);
            container.AddChild(new Label { Text = "W:" }); container.AddChild(spinW);
            container.AddChild(new Label { Text = "H:" }); container.AddChild(spinH);

            _syncUIAction = () => {
                var val = (Rect2I)_iVar.value;
                spinX.SetValueNoSignal(val.Position.X); spinY.SetValueNoSignal(val.Position.Y);
                spinW.SetValueNoSignal(val.Size.X); spinH.SetValueNoSignal(val.Size.Y);
            };
            return container;
        }

        private Control CreateQuaternionEditor(Quaternion current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinZ = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Z, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinW = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.W, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Quaternion((float)spinX.Value, (float)spinY.Value, (float)spinZ.Value, (float)spinW.Value));

            spinX.ValueChanged += _ => Update(); spinY.ValueChanged += _ => Update();
            spinZ.ValueChanged += _ => Update(); spinW.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "X:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Y:" }); container.AddChild(spinY);
            container.AddChild(new Label { Text = "Z:" }); container.AddChild(spinZ);
            container.AddChild(new Label { Text = "W:" }); container.AddChild(spinW);

            _syncUIAction = () => {
                var val = (Quaternion)_iVar.value;
                spinX.SetValueNoSignal(val.X); spinY.SetValueNoSignal(val.Y);
                spinZ.SetValueNoSignal(val.Z); spinW.SetValueNoSignal(val.W);
            };
            return container;
        }

        private Control CreatePlaneEditor(Plane current)
        {
            var container = new HBoxContainer();
            var spinX = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Normal.X, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinY = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Normal.Y, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinZ = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.Normal.Z, SizeFlagsHorizontal = SizeFlags.ExpandFill };
            var spinD = new SpinBox { MinValue = -1e9, MaxValue = 1e9, Step = 0.001, Value = current.D, SizeFlagsHorizontal = SizeFlags.ExpandFill };

            void Update() => UpdateValue(new Plane((float)spinX.Value, (float)spinY.Value, (float)spinZ.Value, (float)spinD.Value));

            spinX.ValueChanged += _ => Update(); spinY.ValueChanged += _ => Update();
            spinZ.ValueChanged += _ => Update(); spinD.ValueChanged += _ => Update();

            container.AddChild(new Label { Text = "Nx:" }); container.AddChild(spinX);
            container.AddChild(new Label { Text = "Ny:" }); container.AddChild(spinY);
            container.AddChild(new Label { Text = "Nz:" }); container.AddChild(spinZ);
            container.AddChild(new Label { Text = "D:" }); container.AddChild(spinD);

            _syncUIAction = () => {
                var val = (Plane)_iVar.value;
                spinX.SetValueNoSignal(val.Normal.X); spinY.SetValueNoSignal(val.Normal.Y);
                spinZ.SetValueNoSignal(val.Normal.Z); spinD.SetValueNoSignal(val.D);
            };
            return container;
        }

        // Keep local updates from causing infinite Update loops
        private void UpdateValue(Variant newValue)
        {
            if (_iVar != null)
            {
                _lastKnownValue = newValue; // Acknowledge change so _Process ignores it
                _iVar.value = newValue;
                OnIVarChanged?.Invoke(_iVar);
            }
        }
    }
}