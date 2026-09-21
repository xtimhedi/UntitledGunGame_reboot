using Godot;
using System;
using UGGR.CrashScreen;

namespace UGGR.SceneEditor
{
    public partial class Script : Control
    {
        public GraphEdit_Internal Editor;

        private void PopulateNodes()
        {
            Editor = GetNode<GraphEdit_Internal>("GraphEdit");
        }

        public override void _Ready()
        {
            PopulateNodes();
            Editor.OnContextMenuItemSelected += OnContextItemSelected;

        }

        private void OnContextItemSelected(long id)
        {
            Editor.PlaceNode((int)id);
        }
    }

}