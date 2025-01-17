using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class PhoneController : Interactable
{
    [Signal]
    public delegate void OnPhoneKeyDialedEventHandler(String dialedNumber);

    [Signal]
    public delegate void OnPhoneHangUpEventHandler();

    [Signal]
    public delegate void OnPhonePickUpEventHandler();

    [Signal]
    public delegate void OnPhoneNumberClearedEventHandler();

    [Signal]
    public delegate void OnConversationStartEventHandler(ConversationData conversation);

    readonly Random rng = new();

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

    List<String> currentSequence = new();

    int sequenceLength = 6;

    bool isDialing = false;
    bool canReceiveCall = true;

    PhoneNumberManager numberManager;

    ConversationData incomingConversation;
    bool isRinging = false;

    bool isTurning = false;

    public override void _Ready()
    {
        numberManager = (PhoneNumberManager)GetTree().GetFirstNodeInGroup("number_manager");

        fingerHoles = GetNode<Node3D>("PhoneModel/Empty/FingerHoles");
        dots = GetNode<Node3D>("PhoneModel/Empty/Dots");

        foreach (Node n in fingerHoles.GetChildren())
        {
            if (n is RotaryPhoneHole r)
            {
                r.OnBeginInteraction += () => OnBeginTurning(r);
                r.OnEndInteraction += () => OnEndTurning(r);
            }
        }

        rotaryClickAudio = GetNode<AudioStreamPlayer3D>("RotaryClickAudio");
        rotaryWindingAudio = GetNode<AudioStreamPlayer3D>("RotaryWindingAudio");

        handsetModel = GetNode<Node3D>("PhoneHandset");
        dialToneAudio = GetNode<AudioStreamPlayer>("DialToneAudio");
        disconnectedToneAudio = GetNode<AudioStreamPlayer>("DisconnectedToneAudio");
        ringToneAudio = GetNode<AudioStreamPlayer>("RingToneAudio");
        hangUpAudio = GetNode<AudioStreamPlayer3D>("HangUpAudio");
        otherHangUpAudio = GetNode<AudioStreamPlayer>("OtherHangUpAudio");
        notFoundAudio = GetNode<AudioStreamPlayer>("NotFoundAudio");
        ringAudio = GetNode<AudioStreamPlayer3D>("RingAudio");
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
            var newRot = Mathf.Clamp(Mathf.Lerp(rotDeg.Y, desiredRotation, (float)delta * 30), -heldKeyAngle, -5);

            // Only turn if the change in angle is not too extreme
            var deltaRot = Mathf.Abs(rotDeg.Y - desiredRotation);
            if (deltaRot < 45)
            {
                rotDeg.Y = newRot;
                lastRotWasClockwise = fingerHoles.RotationDegrees.Y - rotDeg.Y > 0;
            }
            else if (lastRotWasClockwise)
            {
                // If the angle becomes too extreme but we were winding clockwise just keep going
                rotDeg.Y = Mathf.Clamp(Mathf.Lerp(rotDeg.Y, -heldKeyAngle, (float)delta * 15), -heldKeyAngle, -5);
            }
            fingerHoles.RotationDegrees = rotDeg;

            // Handle the winding audio
            if (deltaRot > 0)
            {
                if (!rotaryWindingAudio.Playing)
                {
                    rotaryWindingAudio.Play(lastRotaryWindingAudioPos);
                }
                float audioPitch = (Mathf.Clamp(deltaRot, 0, 1) * 0.2f) + 0.9f;
                rotaryWindingAudio.VolumeDb = -25 + Mathf.Clamp(deltaRot, 0, 0.5f) * 50;
                rotaryWindingAudio.PitchScale = audioPitch;
            }
            else
            {
                lastRotaryWindingAudioPos = rotaryWindingAudio.GetPlaybackPosition();
                rotaryWindingAudio.Stop();
            }

            // If we've hit the end then stop
            if (fingerHoles.RotationDegrees.Y <= -heldKeyAngle + 10)
            {
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

    public void IncomingRay(Vector3 from, Vector3 direction)
    {
        if (isTurning)
        {
            Vector3? point = IntersectPoint(
                fingerHoles.GlobalPosition,
                fingerHoles.GlobalTransform.Basis.Y,
                from,
                direction
            );

            if (point != null)
            {
                var debug = GetNode<Node3D>("Debug");
                debug.GlobalPosition = point.Value;
                var offset = dots.ToLocal(debug.GlobalPosition);
                var deg = Mathf.RadToDeg(Mathf.Atan2(offset.X, offset.Z)) + 180;
                var rotationOffset = 360 - heldKeyAngle;
                desiredRotation = -360 + rotationOffset + deg;
            }
        }
    }

    private void SetKeysInteractionEnabled(bool isEnabled)
    {
        foreach (Node n in fingerHoles.GetChildren())
        {
            if (n is RotaryPhoneHole r)
            {
                r.CanInteract = isEnabled;
                r.HoverEnabled = isEnabled;
            }
        }
    }

    private void RotateBackToStart(double delta)
    {
        rotaryWindingAudio.PitchScale = 1;
        rotaryWindingAudio.VolumeDb = 0;
        if (!rotaryWindingAudio.Playing) rotaryWindingAudio.Play();

        var rotDeg = fingerHoles.RotationDegrees;
        rotDeg.Y = Mathf.Min(Mathf.MoveToward(rotDeg.Y, 0, (float)delta * 300), 0f);
        fingerHoles.RotationDegrees = rotDeg;

        if (rotDeg.Y == 0)
        {
            rotaryClickAudio.PitchScale = 0.9f;
            rotaryClickAudio.Play();
            rotaryWindingAudio.Stop();
            SetKeysInteractionEnabled(true);
        }
    }


    public void OnBeginTurning(RotaryPhoneHole key)
    {
        isTurning = true;
        heldKey = key;
        heldKeyAngle = Mathf.RadToDeg(Mathf.Atan2(heldKey.Position.X, heldKey.Position.Z)) + 180;
        SetKeysInteractionEnabled(false);
    }

    public void OnEndTurning(RotaryPhoneHole key)
    {
        if (hasHitEnd)
        {
            OnKeyDialed(key.hoverString);
        }
        isTurning = false;
        heldKey = null;
        hasHitEnd = false;
        desiredRotation = 0;
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

    public async void RingPhone(ConversationData conversation)
    {
        if (isRinging) return;

        GD.Print("Incoming call");

        isRinging = true;
        incomingConversation = conversation;

        while (!canReceiveCall)
        {
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
        currentSequence.Clear();
        isDialing = false;

        hangUpAudio.Play();

        dialToneAudio.Stop();
        ringToneAudio.Stop();
        otherHangUpAudio.Stop();
        disconnectedToneAudio.Stop();
        /*
        notFoundAudio.Stop();
        */

        EmitSignal(SignalName.OnPhoneHangUp);
        canReceiveCall = true;
    }

    public void OnConversationComplete()
    {
        GD.Print("Convo complete");
        otherHangUpAudio.Play();
        disconnectedToneAudio.Play();
        isDialing = true;
    }

    private async void CallNumber(ConversationData conversation)
    {
        if (conversation == null) return;

        isDialing = false;
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

        EmitSignal(SignalName.OnPhoneNumberCleared);

        ringToneAudio.Play();

        await ToSignal(
            GetTree().CreateTimer(rng.NextInt64(3, 5)),
            SceneTreeTimer.SignalName.Timeout
        );

        ringToneAudio.Stop();

        EmitSignal(SignalName.OnConversationStart, conversation);
        isDialing = false;
    }

    public void OnKeyDialed(String key)
    {
        if (!isDialing) return;

        dialToneAudio.Stop();
        disconnectedToneAudio.Stop();

        if (currentSequence.Count >= sequenceLength)
        {
            return;
        }

        currentSequence.Add(key);

        EmitSignal(SignalName.OnPhoneKeyDialed, GetFormattedSequence());


        if ((currentSequence.Count == sequenceLength || currentSequence.Count == 3) && isDialing)
        {
            string numberString = string.Join("", currentSequence.ToArray());
            GD.Print("Try fetch convo for " + numberString);
            ConversationData? conversation = numberManager.GetConversationDataByNumber(numberString);
            if (conversation != null) CallNumber(conversation);
        }
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
