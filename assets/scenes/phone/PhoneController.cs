using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class PhoneController : Interactable
{
    [Signal]
    public delegate void OnPhoneKeyPressedEventHandler(String dialedNumber);

    [Signal]
    public delegate void OnPhoneHangUpEventHandler();

    [Signal]
    public delegate void OnPhonePickUpEventHandler();

    [Signal]
    public delegate void OnPhoneNumberClearedEventHandler();

    [Signal]
    public delegate void OnConversationStartEventHandler(Conversation conversation);

    readonly Random rng = new();

    //public override Vector3 InteractOffset => new Vector3(0.5f, 0.1f, 0);
    //public override Vector3 StopInteractOffset => new Vector3(0.5f, 0.1f, 0);

    public override string HoverString => "Phone";

    public override string InteractString => "Use";

    Node3D handsetModel;
    Node3D fingerHoles;
    Node3D dots;
    Node3D heldKey;
    float heldKeyAngle;

    AudioStreamPlayer3D rotaryClickAudio;
    AudioStreamPlayer3D rotaryWindingAudio;

    AudioStreamPlayer dialToneAudio;
    AudioStreamPlayer disconnectedToneAudio;
    AudioStreamPlayer ringToneAudio;
    AudioStreamPlayer3D ringAudio;
    AudioStreamPlayer3D hangUpAudio;
    AudioStreamPlayer otherHangUpAudio;
    AudioStreamPlayer notFoundAudio;
    AudioStreamPlayer modemAudio;

    List<String> currentSequence = new();

    int sequenceLength = 10;

    bool isDialing = false;
    bool canReceiveCall = true;

    //PhoneNumberManager numberManager;
    //NetworkManager networkManager;

    Conversation incomingConversation;
    bool isRinging = false;

    bool isTurning = false;

    public override void _Ready()
    {
        //numberManager = (PhoneNumberManager)GetTree().GetFirstNodeInGroup("number_manager");
        //networkManager = (NetworkManager)GetTree().GetFirstNodeInGroup("network_manager");

        fingerHoles = GetNode<Node3D>("PhoneModel/Empty/FingerHoles");
        dots = GetNode<Node3D>("PhoneModel/Empty/Dots");

        foreach(Node n in fingerHoles.GetChildren()) {
            if (n is RotaryPhoneHole r) {
                r.OnBeginInteraction += () => OnBeginTurning(r);
                r.OnEndInteraction += () => OnEndTurning(r);
            }
        }

        rotaryClickAudio = GetNode<AudioStreamPlayer3D>("RotaryClickAudio");
        rotaryWindingAudio = GetNode<AudioStreamPlayer3D>("RotaryWindingAudio");

        handsetModel = GetNode<Node3D>("Phone/Handset");
        dialToneAudio = GetNode<AudioStreamPlayer>("DialToneAudio");
        disconnectedToneAudio = GetNode<AudioStreamPlayer>("DisconnectedToneAudio");
        ringToneAudio = GetNode<AudioStreamPlayer>("RingToneAudio");
        hangUpAudio = GetNode<AudioStreamPlayer3D>("HangUpAudio");
        otherHangUpAudio = GetNode<AudioStreamPlayer>("OtherHangUpAudio");
        notFoundAudio = GetNode<AudioStreamPlayer>("NotFoundAudio");
        ringAudio = GetNode<AudioStreamPlayer3D>("RingAudio");
        modemAudio = GetNode<AudioStreamPlayer>("ModemAudio");
    }


    float desiredRotation = 0;
    bool hasHitEnd = false;
    bool lastRotWasClockwise = false;

    float lastRotaryWindingAudioPos = 0;

    public override void _Process(double delta)
    {
        if (isTurning && !hasHitEnd && fingerHoles.RotationDegrees.Y != desiredRotation) 
        {
            var rotDeg = fingerHoles.RotationDegrees;
            var newRot = Mathf.Clamp(lerp(rotDeg.Y, desiredRotation, (float)delta * 30), -heldKeyAngle, -5);

            // Only turn if the change in angle is not too extreme
            var deltaRot = Mathf.Abs(rotDeg.Y - newRot) ;
            if ( deltaRot < 45) 
            {
                rotDeg.Y = newRot;
            } 
            else if (lastRotWasClockwise) 
            {
                // If we were turning too hard then just keep going anyway
                rotDeg.Y = Mathf.Clamp(lerp(rotDeg.Y, -heldKeyAngle, (float)delta * 15), -heldKeyAngle, -5);
            }
            lastRotWasClockwise = fingerHoles.RotationDegrees.Y - rotDeg.Y > 0;
            fingerHoles.RotationDegrees = rotDeg;

            if (deltaRot > 0) {
                if (!rotaryWindingAudio.Playing) {
                    rotaryWindingAudio.Play(lastRotaryWindingAudioPos);
                }

                float audioPitch = (Mathf.Clamp(deltaRot, 0, 1) * 0.2f) + 0.9f;
                rotaryWindingAudio.VolumeDb = -25 + Mathf.Clamp(deltaRot, 0, 0.5f) * 50;
                rotaryWindingAudio.PitchScale = audioPitch;
            } else {
                lastRotaryWindingAudioPos = rotaryWindingAudio.GetPlaybackPosition();
                rotaryWindingAudio.Stop();
            }

            // If we've hit the end then stop
            if (fingerHoles.RotationDegrees.Y <= -heldKeyAngle+10) 
            {
                GD.Print("HIT END");
                hasHitEnd = true;
                rotaryClickAudio.PitchScale = 1f;
                rotaryClickAudio.Play();
                rotaryWindingAudio.Stop();
            }
        } 
        else if (!isTurning && fingerHoles.RotationDegrees.Y != 0) 
        {
            RotateBackToStart(delta);
        }
    }

    float lerp (float a, float b, float f) {
        return a + f * (b - a);
    }
    
    
    public void IncomingRay(Vector3 from, Vector3 direction) 
    {
        if (isTurning) {
            Vector3? point = IntersectPoint(
                fingerHoles.GlobalPosition,
                fingerHoles.GlobalTransform.Basis.Y,
                from,
                direction
            );

            if (point != null) {
                var debug = GetNode<Node3D>("Debug");
                debug.GlobalPosition = point.Value;
                var offset = dots.ToLocal(debug.GlobalPosition);
                var deg = (MathF.Atan2(offset.X, offset.Z) * (180/MathF.PI)) + 180;
                var rotationOffset = 360 - heldKeyAngle;
                desiredRotation = -360 + rotationOffset + deg;
                GD.Print("desired: " + desiredRotation);
                GD.Print("heldangle: " + heldKeyAngle);
                GD.Print("limit: " + heldKeyAngle);
            }
        }
    }

    private void RotateBackToStart(double delta)
    {
        rotaryWindingAudio.PitchScale = 1;
        rotaryWindingAudio.VolumeDb = 0;
        if (!rotaryWindingAudio.Playing) rotaryWindingAudio.Play();

        var rotDeg = fingerHoles.RotationDegrees;
        rotDeg.Y = Mathf.Min(rotDeg.Y + ((float)delta * 300), 0);
        fingerHoles.RotationDegrees = rotDeg;

        if (rotDeg.Y == 0) {
            rotaryClickAudio.PitchScale = 0.9f;
            rotaryClickAudio.Play();
            rotaryWindingAudio.Stop();
        }
    }
        
    
    public void OnBeginTurning(RotaryPhoneHole key) {
        GD.Print("Turning");
        isTurning = true;
        heldKey = key;
        heldKeyAngle = (MathF.Atan2(heldKey.Position.X, heldKey.Position.Z) * (180/MathF.PI)) + 180;
    }

    public void OnEndTurning(RotaryPhoneHole key) {
        GD.Print("Not Turning");
        if (hasHitEnd) {
            GD.Print("DIALED KEY " + key.hoverString);
        }
        isTurning = false;
        heldKey = null;
        hasHitEnd = false;
        desiredRotation = 0;
        // disable all buttons
        // rotate back to the beginning at a fix rate
        // re-enable buttons
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

    public async void RingPhone(Conversation conversation)
    {
        if (isRinging) return;

        isRinging = true;
        incomingConversation = conversation;

        while (!canReceiveCall) {
            await Task.Delay(100);
        }

        ringAudio.Play();
    }

    public async void PickupPhone()
    {
        handsetModel.Visible = false;
        canReceiveCall = false;
        EmitSignal(SignalName.OnPhonePickUp);

        if (isRinging)
        {
            ringAudio.Stop();
            isRinging = false;

            await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);
            if (incomingConversation != null)
            {
                EmitSignal(SignalName.OnConversationStart, incomingConversation);
            }
            else
            {
                disconnectedToneAudio.Play();
            }

            incomingConversation = null;
        }
        else
        {
            dialToneAudio.Play();
            isDialing = true;
        }
    }

    public void HangupPhone()
    {
        handsetModel.Visible = true;
        dialToneAudio.Stop();
        disconnectedToneAudio.Stop();
        notFoundAudio.Stop();
        otherHangUpAudio.Stop();
        ringToneAudio.Stop();
        currentSequence.Clear();
        hangUpAudio.Play();
        isDialing = false;
        modemAudio.Stop();

        EmitSignal(SignalName.OnPhoneHangUp);
        canReceiveCall = true;
    }

    public void OnConversationComplete()
    {
        otherHangUpAudio.Play();
        isDialing = true;
    }

    private async void CallNumber()
    {
        isDialing = false;
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

        foreach (String s in currentSequence)
        {
            await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
            //GetNode<PhoneKey>(s).PlayTone();
        }

        EmitSignal(SignalName.OnPhoneNumberCleared);

        await ToSignal(GetTree().CreateTimer(1.0f), SceneTreeTimer.SignalName.Timeout);

        string numberString = string.Join("", currentSequence.ToArray());

        /*
        Conversation conversation = numberManager.GetPhoneNumber(numberString);

        if (conversation != null)
        {
            ringToneAudio.Play();

            await ToSignal(
                GetTree().CreateTimer(rng.NextInt64(3, 6)),
                SceneTreeTimer.SignalName.Timeout
            );

            ringToneAudio.Stop();

            EmitSignal(SignalName.OnConversationStart, conversation);
            isDialing = false;
        }
        else
        {
            if (networkManager.GetNetwork(numberString) != null)
            {
                modemAudio.Play();
            }
            else
            {
                notFoundAudio.Play();
            }
            isDialing = true;
        }
        */
    }

    public void OnKeyDialed(String key)
    {
        if (!isDialing) return;

        dialToneAudio.Stop();

        if (currentSequence.Count >= sequenceLength)
        {
            return;
        }

        currentSequence.Add(key);

        EmitSignal(SignalName.OnPhoneKeyPressed, GetFormattedSequence());

        if (currentSequence.Count == sequenceLength && isDialing)
            CallNumber();
    }

    private String GetFormattedSequence()
    {
        String formattedString = "";

        for (int i = 0; i < currentSequence.Count; i++)
        {
            if (i == 3 || i == 6)
            {
                formattedString += "-";
            }

            formattedString += currentSequence[i];
        }

        return formattedString;
    }

    public override void Interact()
    {
        GD.Print("Interacted with phone");
    }
}
