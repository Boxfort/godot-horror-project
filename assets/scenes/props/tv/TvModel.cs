using Godot;
using System;

public partial class TvModel : Node3D
{
    [Export]
    SubViewport screenViewport;
    ShaderMaterial screenMaterial;
    public MeshInstance3D screenMesh;

    public ShaderMaterial ScreenMaterial
    {
        get => screenMaterial;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        screenMesh = GetNode<MeshInstance3D>("tv");
        screenMaterial = (ShaderMaterial)screenMesh.GetSurfaceOverrideMaterial(0);
        ViewportTexture viewportTexture = screenViewport.GetTexture();
        screenMaterial.SetShaderParameter("albedoTex", viewportTexture);
        screenMaterial.SetShaderParameter("modulate_color", new Color(1, 1, 1));
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}
