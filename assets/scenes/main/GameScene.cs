using Godot;
using System;

public partial class GameScene : Node
{
    PlayerController playerNode;
    PlayerHUD playerHUD;

    public override void _Ready()
    {
        playerNode = (PlayerController)GetTree().GetFirstNodeInGroup("player");
        playerHUD = (PlayerHUD)GetTree().GetFirstNodeInGroup("player_hud");
    }

    public void LoadGame()
    {

    }

    public void NewGameStart()
    {
        // Move player to bedroom spawn point.
        playerNode.Position = new Vector3(1.45f, 1, -11.25f);
        playerNode.RotationDegrees = new Vector3(0, 145f, 0);
        // Fade in
        playerHUD.FadeIn();
    }
}
