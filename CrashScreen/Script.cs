using Godot;
using System;

namespace UGGR.CrashScreen
{
    public partial class Script : Control
    {
        public Label programLabel;
        public Label bodyLabel;

        public override void _Ready()
        {
            programLabel = GetNode<Label>("PanelContainer2/VBoxContainer/Label3");
            bodyLabel = GetNode<Label>("PanelContainer2/VBoxContainer/MarginContainer2/Panel/ScrollContainer/Label");
        }
        public void SetProperties(string Callstack, string ProgramName)
        {
            programLabel.Text = ProgramName;
            bodyLabel.Text = bodyLabel.Text + "\n" + Callstack;
        }
        public void CloseWindow()
        {
            GetTree().Quit();
        }
        public void MinimizeWindow()
        {
            // GetWindow().Mode = Window.ModeEnum.Minimized;
        }

        public void CloseWithSend()
        {
            CloseWindow();
        }

        public void CopyCallstack()
        {

        }
    }

}