using Godot;
using System;
using System.Threading.Tasks;

public partial class TerminalLine : Label
{
    [Signal]
    public delegate void OnLineFinishedShowingEventHandler();

    bool shouldSlowShow = false;

    const float showTextTime = 0.005f;
    float showTextTimer = 0;
    int textIndex = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        VisibleCharacters = 0;
        AutowrapMode = TextServer.AutowrapMode.Arbitrary;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (!shouldSlowShow)
            return;

        if (VisibleCharacters >= Text.Length)
        {
            EmitSignal(SignalName.OnLineFinishedShowing);
            shouldSlowShow = false;
        }
        else if (showTextTimer >= showTextTime)
        {
            if (VisibleCharacters < Text.Length)
            {
                VisibleCharacters++;
            }

            showTextTimer = 0;
        }
        else
        {
            showTextTimer += (float)delta;
        }
    }

    public void ShowText(bool instant = false) 
    {
        if (!instant) 
        {
            shouldSlowShow = true;
        }
        else 
        {
            VisibleCharacters = -1;
            EmitSignal(SignalName.OnLineFinishedShowing);
        }
    }
}
