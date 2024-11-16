using Godot;
using System;

public partial class SceneManager : Node
{
    [Export]
    string titleScreenScenePath = "res://Assets/Scenes/TitleScreen/TitleScreen.tscn";
    [Export]
    string gameScenePath = "res://Assets/Scenes/Apartment/ApartmentScene.tscn";
    [Export]
    string loadingScreenScenePath = "res://Assets/Scenes/LoadingScreen/LoadingScreen.tscn";

    Node currentSceneContainer;

    bool loadingScene = false;
    string loadingSceneName;

    public override void _Ready()
    {
        currentSceneContainer = GetNode<Node>("CurrentSceneContainer");
        LoadTitleScreen();
    }

    public override void _Process(double delta)
    {
        if (loadingScene) 
        {
            var loadingStatus = ResourceLoader.LoadThreadedGetStatus(loadingSceneName);
            if (loadingStatus == ResourceLoader.ThreadLoadStatus.Loaded) 
            {
                ClearCurrentScene();
                var newScene = (PackedScene)ResourceLoader.LoadThreadedGet(loadingSceneName);
                var instance = newScene.Instantiate();
                currentSceneContainer.AddChild(instance);

                if (instance is GameScene game) {
                    game.NewGameStart();
                }
            }
        }
    }

    public void LoadTitleScreen()
    {
        ClearCurrentScene();
        PackedScene titleScreenScene = (PackedScene)ResourceLoader.Load(titleScreenScenePath);
        TitleScreen titleScreenInstance = (TitleScreen)titleScreenScene.Instantiate();
        currentSceneContainer.AddChild(titleScreenInstance);

        titleScreenInstance.OnNewGamePressed += () => LoadGameScene();
    }

    public void LoadGameScene()
    {
        ClearCurrentScene();
        PackedScene loadingScreenScene = (PackedScene)ResourceLoader.Load(loadingScreenScenePath);
        currentSceneContainer.AddChild(loadingScreenScene.Instantiate());

        ResourceLoader.LoadThreadedRequest(gameScenePath);

        loadingSceneName = gameScenePath;
        loadingScene = true;
    }

    public void ClearCurrentScene()
    {
        if (currentSceneContainer.GetChildCount() == 0) return;

        currentSceneContainer.GetChild(0).QueueFree();
    }
}
