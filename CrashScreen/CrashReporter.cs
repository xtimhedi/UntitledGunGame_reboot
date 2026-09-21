using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGGR.CrashScreen
{
    public static class CrashReporter
    {
        public static Script REPORTER = (Script)GD.Load<PackedScene>("res://CrashScreen/Window.tscn").Instantiate();

        public static void PopupCrash(string CrashBody, string GameName, bool isSevere, Node windowParent)
        {
            var window = new Window();

            window.Title = "Godot Engine Crash Reporter";
            window.Size = new Vector2I(600, 500);
            window.Borderless = true;
            window.CloseRequested += () =>
            {
                windowParent.GetTree().Quit();
            };
            window.AddChild(REPORTER);
            windowParent.AddChild(window);
            REPORTER.SetProperties(CrashBody, GameName);
            window.PopupCentered();
        }
    }
}
