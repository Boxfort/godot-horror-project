using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class Terminal : Control
{
    [Export]
    PackedScene inputPrompt;

    [Export]
    Theme terminalLineTheme;

    VBoxContainer lines;
    ScrollContainer scrollContainer;

    TerminalPrompt terminalPrompt;

    public bool InputDisabled { get; private set; }

    CommandEvaluator evaluator = new CommandEvaluator();

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
        evaluator.EvaluateCommand(line, this);
    }

    public async Task<Label> AddLine(string text, bool instant = false)
    {
        TerminalLine label = new TerminalLine 
        {
            Theme = terminalLineTheme,
            Text = text,
            AutowrapMode = TextServer.AutowrapMode.Arbitrary
        };

        lines.AddChild(label);
        // Move the line before the last one.
        lines.MoveChild(label, lines.GetChildCount() - 2);

        scrollContainer.ScrollVertical = (int)scrollContainer.GetVScrollBar().MaxValue;
        label.ShowText(instant);

        await ToSignal(label, TerminalLine.SignalName.OnLineFinishedShowing);

        return label;
    }

    internal string getPromptPrefix()
    {
        return terminalPrompt.Prefix;
    }
}