using Godot;
using System;

public partial class Blink : Sprite3D
{
    [Export]
    bool shouldBlink = false;

    float blinkTimer = 0;
    float onTime = 0.75f;
    float offTime = 0.25f;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!shouldBlink) return;

        blinkTimer += (float)delta;

        if (IsVisibleInTree() && blinkTimer >= onTime) {
            Hide();
            blinkTimer = 0;
        } else if (!IsVisibleInTree() && blinkTimer >= offTime) {
            Show();
            blinkTimer = 0;
        }
    }
}
