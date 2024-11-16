using System;
using System.Collections.Generic;
using System.Diagnostics;
using Godot;

public partial class ScreenCanvas : CanvasLayer
{
    AudioStreamPlayer typingAudio;
    AudioStreamPlayer typingAudioBig;
    AudioStreamPlayer clickingAudio;
    Dictionary<Key, bool> isKeyPressed = new Dictionary<Key, bool>();
    Random rng = new Random();
    Control programContainer;
    Terminal terminal;

    bool isProgramOpening = false;

    bool isInteracting = false;
    public bool IsInteracting
    {
        get => isInteracting;
        set => isInteracting = value;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        terminal = GetNode<Terminal>("MarginContainer/Bounds/Terminal");
        typingAudio = GetNode<AudioStreamPlayer>("TypingAudio");
        typingAudioBig = GetNode<AudioStreamPlayer>("TypingAudioBig");
    }

    public override void _Input(InputEvent @event)
    {
        if (!isInteracting)
            return;

        if (@event is InputEventKey key)
        {
            if (key.Pressed && !isKeyPressed.GetValueOrDefault(key.Keycode, false))
            {
                if (key.Keycode == Key.Space || key.Keycode == Key.Enter)
                {
                    PlaySoundPitched(typingAudioBig);
                }
                else if (key.Keycode != Key.Ctrl)
                {
                    PlaySoundPitched(typingAudio);
                }

                isKeyPressed[key.Keycode] = true;
            }
            else if (!key.Pressed)
            {
                isKeyPressed[key.Keycode] = false;
            }
        }
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
    }

    private void PlaySoundPitched(AudioStreamPlayer player)
    {
        player.PitchScale = 1 + (((float)rng.NextDouble() * 0.1f) - 0.05f);
        player.Play();
    }
}
