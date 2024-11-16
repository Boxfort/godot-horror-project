using Godot;

public abstract partial class Interactable : Hoverable
{
    protected bool canInteract = true;
    public bool CanInteract
    {
        get => canInteract;
        set => canInteract = value;
    }
    public abstract string InteractString { get; }
    public abstract void Interact();
}
