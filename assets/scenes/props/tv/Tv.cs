using Godot;
using System;
using System.Threading.Tasks;

public partial class Tv : Node3D
{
    ScreenCanvasTV screenCanvasTV;
    TvModel tvModel;
    AudioStreamPlayer3D staticAudio;
    OmniLight3D screenLight;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screenCanvasTV = GetNode<ScreenCanvasTV>("SubViewport/ScreenCanvas");
        tvModel = GetNode<TvModel>("TVModel");
        staticAudio = GetNode<AudioStreamPlayer3D>("StaticAudio");
        screenLight = GetNode<OmniLight3D>("OmniLight3D");

        TurnOff();
        PlayEmergencyBroadcast();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }


    public void TurnOn()
    {
        tvModel.ShowScreen();
        staticAudio.Play();
        screenLight.Show();
    }

    public void TurnOff()
    {
        tvModel.HideScreen();
        staticAudio.Stop();
        screenLight.Hide();
    }

    public async void PlayEmergencyBroadcast()
    {
        await Task.Delay(2000);
        TurnOn();
        await screenCanvasTV.StartSequence();
        TurnOff();
    }
}
