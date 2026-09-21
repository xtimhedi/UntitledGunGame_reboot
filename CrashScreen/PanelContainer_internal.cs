using Godot;

public partial class PanelContainer_internal : Control
{
    private bool _isDragging = false;
    private Vector2I _dragOffset = Vector2I.Zero;

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            if (mouseButton.ButtonIndex == MouseButton.Left)
            {
                if (mouseButton.Pressed)
                {
                    _isDragging = true;
                    _dragOffset = DisplayServer.MouseGetPosition() - GetWindow().Position;
                }
                else
                {
                    _isDragging = false;
                }
            }
        }
        else if (@event is InputEventMouseMotion && _isDragging)
        {
            Vector2I newPosition = DisplayServer.MouseGetPosition() - _dragOffset;
            GetWindow().Position = newPosition;
        }
    }
}
