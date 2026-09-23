using Godot;
using System;
using UGGR.CrashScreen;
using UGGR.SceneEditor.Editor;
using UGGR.SceneEditor.Nodes;

namespace UGGR.SceneEditor
{
    public partial class Script : Control
    {
        public static GraphEdit_Internal Editor;
        public VBoxContainer LabelButtons;
        public MarginContainer container;
        public LineEdit varNameEdit;
        public OptionButton varType;
        private void PopulateNodes()
        {
            Editor = GetNode<GraphEdit_Internal>("HBoxContainer/VBoxContainer/GraphEdit");
            LabelButtons = GetNode<VBoxContainer>("HBoxContainer/PanelContainer/ScrollContainer/VBoxContainer/MarginContainer/PanelContainer/LabelButtons");
            container = GetNode<MarginContainer>("HBoxContainer/PanelContainer/ScrollContainer/VBoxContainer/NewVarContainer");
            varNameEdit = GetNode<LineEdit>("HBoxContainer/PanelContainer/ScrollContainer/VBoxContainer/NewVarContainer/PanelContainer/VBoxContainer/LineEdit");
            varType = GetNode<OptionButton>("HBoxContainer/PanelContainer/ScrollContainer/VBoxContainer/NewVarContainer/PanelContainer/VBoxContainer/OptionButton");


        }

        public override void _Ready()
        {
            PopulateNodes();
            Editor.OnContextMenuItemSelected += OnContextItemSelected;
            LabelButtons.AddChild(new VariableVisualizer(GraphEdit_Internal.Variables[0]));
            PopNVEnum();
        }

        private void OnContextItemSelected(long id)
        {
            Editor.PlaceNode((int)id);
        }

        public void NewVar(Variant.Type val, string Name)
        {
            IVar newVar = new IVar
            {
                Name = Name,
                id = GraphEdit_Internal.Variables.Count,
                type = val
            };
            GraphEdit_Internal.Variables.Add(newVar);
        }
        public void PopulateVariables()
        {
            foreach (Node child in LabelButtons.GetChildren())
            {
                child.QueueFree();
            }
            foreach (IVar var in GraphEdit_Internal.Variables)
            {
                LabelButtons.AddChild(new VariableVisualizer(var));
            }
        }

        public void AddType(Variant.Type type)
        {
            varType.AddItem(type.ToString(), (int)type);

        }

        public void PopNVEnum()
        {
            varType.Clear();
            AddType(Variant.Type.Nil);
            AddType(Variant.Type.Bool);
            AddType(Variant.Type.String);
            AddType(Variant.Type.Int);
        }

        public void NewVariable()
        {
            NewVar((Variant.Type)varType.GetSelectedId(), varNameEdit.Text);
            PopulateVariables();
        }
    }

}