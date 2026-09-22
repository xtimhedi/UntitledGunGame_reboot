using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor.Editor
{
    public partial class BooleanVariable : HBoxContainer
    {
        public BooleanVariable() { }
        public BooleanVariable(string title) { name = title; }
        public bool state;
        public string name;
        Label _title = new Label();
        CheckButton _state = new CheckButton();
        public override void _Ready()
        {
            _title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            _state.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            AddChild(_title);
            AddChild(_state);
            _title.Text = name;
        }

        public override void _Process(double delta)
        {
            state = _state.ButtonPressed;
            _state.ButtonPressed = state;

        }
    }
}
