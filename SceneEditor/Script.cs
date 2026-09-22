using Godot;
using System;
using UGGR.CrashScreen;
using UGGR.SceneEditor.Editor;

namespace UGGR.SceneEditor
{
    public partial class Script : Control
    {
        public GraphEdit_Internal Editor;
        public VBoxContainer LabelButtons;
        private void PopulateNodes()
        {
            Editor = GetNode<GraphEdit_Internal>("HBoxContainer/VBoxContainer/GraphEdit");
            LabelButtons = GetNode<VBoxContainer>("HBoxContainer/PanelContainer/ScrollContainer/VBoxContainer/MarginContainer/PanelContainer/LabelButtons");
        }

        public override void _Ready()
        {
            PopulateNodes();
            Editor.OnContextMenuItemSelected += OnContextItemSelected;
            LabelButtons.AddChild(new BooleanVariable("TestBool"));
            LabelButtons.AddChild(new IntegerVariable("TestInt"));
        }

        private void OnContextItemSelected(long id)
        {
            Editor.PlaceNode((int)id);
        }
    }

}