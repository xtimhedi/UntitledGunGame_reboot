using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.SceneEditor.Editor
{
    public partial class IntegerVariable : HBoxContainer
    {
        public IntegerVariable() { }
        public IntegerVariable(string title) { name = title; }
        public string state;
        public string name;
        Label _title = new Label();
        LineEdit _state = new LineEdit();
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

            state = _state.Text;
            _state.Text = state;


        }
    }
}
