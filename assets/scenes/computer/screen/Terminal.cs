using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class Terminal : Control
{
    [Export]
    PackedScene inputPrompt;

    VBoxContainer lines;
    ScrollContainer scrollContainer;

    TerminalPrompt terminalPrompt;

    public bool InputDisabled { get; private set; }

    private int commandIdx = 0;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        base._Ready();

        scrollContainer = GetNode<ScrollContainer>("TerminalScreen/MarginContainer/Control/ScrollContainer");
        lines = scrollContainer.GetNode<VBoxContainer>("Lines");
        terminalPrompt = lines.GetNode<TerminalPrompt>("TerminalPrompt");

        scrollContainer.GetVScrollBar().Changed += () =>
            scrollContainer.ScrollVertical = (int)scrollContainer.GetVScrollBar().MaxValue;

        terminalPrompt.SetActive(true);
        terminalPrompt.OnLineSubmitted += OnLineSubmitted;

        terminalPrompt.GrabLineFocus();
    }

    private void OnLineSubmitted(string line)
    {
        GD.Print("LINE SUBMITTED");
    }
}
