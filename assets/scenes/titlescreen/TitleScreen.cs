using Godot;
using System;

public partial class TitleScreen : Node
{
    [Signal]
    public delegate void OnNewGamePressedEventHandler();

    Button newGameButton;
    Button exitButton;

    public override void _Ready()
    {
        newGameButton = GetNode<Button>("CanvasLayer/MarginContainer/Control/VBoxContainer/New Game");
        exitButton = GetNode<Button>("CanvasLayer/MarginContainer/Control/VBoxContainer/Exit");

        exitButton.Pressed += () => GetTree().Quit();
        newGameButton.Pressed += () => EmitSignal(SignalName.OnNewGamePressed);

    }
}
