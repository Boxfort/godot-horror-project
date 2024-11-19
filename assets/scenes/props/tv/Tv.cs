using Godot;
using System;
using System.Threading.Tasks;

public partial class Tv : Node3D
{
    ScreenCanvasTV screenCanvasTV;
    TvModel tvModel;
    AudioStreamPlayer3D staticAudio;
    AudioStreamPlayer3D tvTurnOnAudio;
    AudioStreamPlayer3D tvTurnOffAudio;
    OmniLight3D screenLight;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screenCanvasTV = GetNode<ScreenCanvasTV>("SubViewport/ScreenCanvas");
        tvModel = GetNode<TvModel>("TVModel");
        staticAudio = GetNode<AudioStreamPlayer3D>("StaticAudio");
        tvTurnOffAudio = GetNode<AudioStreamPlayer3D>("TVTurnOffAudio");
        tvTurnOnAudio = GetNode<AudioStreamPlayer3D>("TVTurnOnAudio");
        screenLight = GetNode<OmniLight3D>("OmniLight3D");

        TurnOff(true);
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
        tvTurnOnAudio.Play();
    }

    public void TurnOff(bool silent = false)
    {
        tvModel.HideScreen();
        staticAudio.Stop();
        screenLight.Hide();
        if (!silent) tvTurnOffAudio.Play();
    }

    public async void PlayEmergencyBroadcast()
    {
        await Task.Delay(2000);
        TurnOn();
        await screenCanvasTV.StartSequence();
        TurnOff();
    }
}
