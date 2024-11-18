using Godot;
using System;
using System.Threading.Tasks;

public partial class WindowMonster : Sprite3D
{
    VisibleOnScreenNotifier3D visiblityNotifier;
    VisibleOnScreenNotifier3D visiblityNotifierMiddle;
    bool shouldMove = false;
    float moveSpeed = 3;

    Vector3 destination;
    Vector3 startPos;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        visiblityNotifier = GetNode<VisibleOnScreenNotifier3D>("VisibleOnScreenNotifier3D");
        visiblityNotifierMiddle = GetNode<VisibleOnScreenNotifier3D>("VisibleOnScreenNotifier3D2");
        visiblityNotifier.ScreenEntered += async () => {
            GD.Print("ENTERED SCREEN");
            await Task.Delay(100);
            shouldMove = true;
        };

        visiblityNotifierMiddle.ScreenEntered += () => {
            moveSpeed = 6;
        };

        startPos = GlobalPosition;
        destination = startPos + Vector3.Right - (Vector3.Up/4);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (shouldMove && GlobalPosition != destination)  
        {
            var currentPos = GlobalPosition;
            var newPos = currentPos.MoveToward(destination, (float)delta * moveSpeed);
            GlobalPosition = newPos;
        }
    }
}
