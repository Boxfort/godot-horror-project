using Godot;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

public partial class ScreenCanvasTV : CanvasLayer
{
    [Export]
    AudioStreamPlayer3D audio0;
    [Export]
    AudioStreamPlayer3D audio1;
    [Export]
    AudioStreamPlayer3D audio2;
    [Export]
    AudioStreamPlayer3D audio3;
    [Export]
    AudioStreamPlayer3D audio4;
    [Export]
    AudioStreamPlayer3D audio5;
    [Export]
    AudioStreamPlayer3D audio6;
    [Export]
    AudioStreamPlayer3D audio7;

    Control ebsScreen;
    Control screen1;
    Control screen2;
    Control screen3;
    Control screen4;
    Control screen5;
    Control screen6;
    Control screen7;
    Control screen8;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        ebsScreen = GetNode<Control>("MarginContainer/Bounds/EBS Screen");
        screen1 = GetNode<Control>("MarginContainer/Bounds/SCREEN 1");
        screen2 = GetNode<Control>("MarginContainer/Bounds/SCREEN 2");
        screen3 = GetNode<Control>("MarginContainer/Bounds/SCREEN 3");
        screen4 = GetNode<Control>("MarginContainer/Bounds/SCREEN 4");
        screen5 = GetNode<Control>("MarginContainer/Bounds/SCREEN 5");
        screen6 = GetNode<Control>("MarginContainer/Bounds/SCREEN 6");
        screen7 = GetNode<Control>("MarginContainer/Bounds/SCREEN 7");
        screen8 = GetNode<Control>("MarginContainer/Bounds/SCREEN 8");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public async Task StartSequence()
    {
        // BEEP
        ebsScreen.Show();
        screen1.Hide();
        screen2.Hide();
        screen3.Hide();

        audio0.Play();
        await ToSignal(audio0, AudioStreamPlayer3D.SignalName.Finished);
        await Task.Delay(2000);

        // THIS IS A BROADCAST
        ebsScreen.Hide();
        screen1.Show();

        audio1.Play();
        await ToSignal(audio1, AudioStreamPlayer3D.SignalName.Finished);
        await Task.Delay(2000);

        // DO NOT CONTACT OTHER SURVIVORS
        screen1.Hide();
        screen2.Show();

        audio2.Play();
        await ToSignal(audio2, AudioStreamPlayer3D.SignalName.Finished);
        await Task.Delay(2000);

        // REMAIN INDOORS
        screen2.Hide();
        screen3.Show();

        audio3.Play();
        await ToSignal(audio3, AudioStreamPlayer3D.SignalName.Finished);
        await Task.Delay(2000);

        // DO NOT RESPOND
        screen3.Hide();
        screen4.Show();

        audio4.Play();
        await ToSignal(audio4, AudioStreamPlayer3D.SignalName.Finished);
        await Task.Delay(2000);

        // IGNORE KNOCKS
        screen4.Hide();
        screen5.Show();

        audio5.Play();
        await ToSignal(audio5, AudioStreamPlayer3D.SignalName.Finished);
        await Task.Delay(2000);

        screen5.Hide();
        screen6.Show();

        audio6.Play();
        await ToSignal(audio6, AudioStreamPlayer3D.SignalName.Finished);
        await Task.Delay(2000);

        screen6.Hide();
        screen7.Show();

        audio7.Play();
        await ToSignal(audio7, AudioStreamPlayer3D.SignalName.Finished);
        await Task.Delay(2000);

        screen7.Hide();
        screen8.Show();

        await Task.Delay(5000);

        return;
    }
}
