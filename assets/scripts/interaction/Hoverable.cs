using Godot;

public abstract partial class Hoverable : CharacterBody3D
{
    public abstract string HoverString { get; }

    bool hoverEnabled = true;
    public bool HoverEnabled
    {
        get => hoverEnabled;
        set => setHoverEnabled(value);
    }

    void setHoverEnabled(bool value)
    {
        hoverEnabled = value;
        SetCollisionLayerValue(2, hoverEnabled);
    }
}
