using System;
using Godot;

public partial class ComputerController : Interactable
{
    public override string HoverString => "Terminal";

    public override string InteractString => "Use";

    public bool isLookingAtScreen = false;
    public bool isInteracting = false;

    ComputerModel computerModel;
    ScreenCanvas screenCanvas;
    SubViewport screenViewport;

    AudioStreamPlayer3D dialUpAudio;
    AudioStreamPlayer3D dialToneAudio;
    AudioStreamPlayer3D keyDialAudio;
    AudioStreamPlayer3D computerRunningAudio;

    PlayerController player;

    [Export]
    AudioStream[] keyAudios;
    OmniLight3D screenLight;
    Terminal terminal;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        dialUpAudio = GetNode<AudioStreamPlayer3D>("DialUpAudio");
        dialToneAudio = GetNode<AudioStreamPlayer3D>("DialToneAudio");
        keyDialAudio = GetNode<AudioStreamPlayer3D>("KeyDialAudio");
        computerRunningAudio = GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D");

        screenCanvas = GetNode<ScreenCanvas>("SubViewport/ScreenCanvas");
        screenViewport = GetNode<SubViewport>("SubViewport");
        computerModel = GetNode<ComputerModel>("VT220Model");

        player = (PlayerController)GetTree().GetFirstNodeInGroup("player");

        terminal = screenCanvas.GetNode<Terminal>("MarginContainer/Bounds/Terminal");

        screenLight = GetNode<OmniLight3D>("OmniLight3D");
    }

    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
        screenViewport.PushInput(@event);
    }

    public void OnBeginInteracting()
    {
        GD.Print("WE DO BE INTERACTING");
        isInteracting = true;
        screenCanvas.IsInteracting = true;
        screenViewport.GuiDisableInput = false;
    }

    public void OnEndInteracting()
    {
        isInteracting = false;
        isLookingAtScreen = false;
        screenCanvas.IsInteracting = false;
        screenViewport.GuiDisableInput = true;
    }

    public void DisableScreenGuiInput(bool shouldDisable)
    {
        screenViewport.GuiDisableInput = shouldDisable;
    }

    float screenWidth = 0.32f;
    float screenHeight = 0.28f;

    Vector3? intersect = Vector3.Zero;

    public void IncomingRay(Vector3 from, Vector3 direction)
    {
        var planePos = ToGlobal(new Vector3(0, 0, 0));
        var planeNormal = GlobalTransform.Basis.X;

        intersect = IntersectPoint(planePos, planeNormal, from, direction);

        if (intersect is Vector3 value)
        {
            // Get the point in local space so it's now relative to the screen without rotation.
            Vector3 localPoint = ToLocal(value);

            // If in the screen bounds
            if (
                localPoint.Z >= -screenWidth
                && localPoint.Z <= screenWidth
                && localPoint.Y <= screenHeight
                && localPoint.Y >= -screenHeight
            )
            {
                player.zoomView = true;
                isLookingAtScreen = true;
            }
            else
            {
                player.zoomView = false;
                isLookingAtScreen = false;
            }
        }
        else
        {
            player.zoomView = false;
            isLookingAtScreen = false;
            return;
        }
    }

    Vector3? IntersectPoint(
        Vector3 planePosition,
        Vector3 planeNormal,
        Vector3 rayOrigin,
        Vector3 rayDirection
    )
    {
        //Math from http://www.scratchapixel.com/lessons/3d-basic-rendering/minimal-ray-tracer-rendering-simple-shapes/ray-plane-and-ray-disk-intersection

        float denominator = rayDirection.Dot(planeNormal);

        if (denominator > 0.00001f)
        {
            //The distance to the plane
            float t = (planePosition - rayOrigin).Dot(planeNormal) / denominator;

            //Where the ray intersects with a plane
            return rayOrigin + rayDirection * t;
        }
        else
        {
            return null;
        }
    }

    public override void Interact()
    {
        GD.Print("Begin using computer please");
    }
}
