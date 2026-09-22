using Godot;
using System;
using UGGR.CrashScreen;
using UGGR.SceneEditor.Editor;

namespace UGGR.SceneEditor
{
    public partial class Script : Control
    {
        public static GraphEdit_Internal Editor;
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
            LabelButtons.AddChild(new VariableVisualizer(GraphEdit_Internal.Variables[0]));
        }

        private void OnContextItemSelected(long id)
        {
            Editor.PlaceNode((int)id);
        }
    }

}