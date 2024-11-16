using Godot;

public abstract partial class Interfaceable : Hoverable
{
    public abstract Vector3 InteractOffset { get; }
    public abstract Vector3 StopInteractOffset { get; }
    public abstract bool LookAtOnInteract { get; }
}
