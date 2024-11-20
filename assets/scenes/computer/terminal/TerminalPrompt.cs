using System;
using System.Collections.Generic;
using Godot;

public partial class TerminalPrompt : Control
{
    [Signal]
    public delegate void OnLineSubmittedEventHandler(String line);

    [Signal]
    public delegate void OnGetCommandHistoryEventHandler();

    [Signal]
    public delegate void OnResetCommandHistoryEventHandler();

    String prefix = "> ";
    String caret = "_";

    bool caretBlinking = false;
    float caretBlinkInterval = 0.5f;
    float caretBlinkTime = 0.1f;
    float caretBlinkTimer = 0.1f;

    bool isActive = true;
    bool canGetHistory = true;

    LineEdit lineEdit;
    Label prefixLabel;

    public string Prefix
    {
        get => prefix;
        set => SetPrefix(value);
    }
    public bool IsActive
    {
        get => isActive;
        set => SetActive(value);
    }

    private void SetPrefix(string value)
    {
        prefix = value;
        prefixLabel.Text = prefix;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        prefixLabel = GetNode<Label>("Prefix");
        prefixLabel.Text = prefix;

        lineEdit = GetNode<LineEdit>("LineEdit");
        lineEdit.FocusMode = FocusModeEnum.All;
        GrabLineFocus();

        lineEdit.GuiInput += OnLineEditInput;
        lineEdit.TextChanged += OnLineEditChanged;
        lineEdit.FocusExited += GrabLineFocus;
    }

    public override void _Process(double delta)
    {
        if (isActive)
        {
            GrabLineFocus();
        }
    }

    public void GrabLineFocus()
    {
        lineEdit.GrabFocus();
    }

    private void OnLineEditChanged(string newText)
    {
        if (newText == "")
        {
            canGetHistory = true;
            EmitSignal(SignalName.OnResetCommandHistory);
        }
    }

    public async void OnLineEditInput(InputEvent @event)
    {
        if (!isActive)
            return;

        if (@event is InputEventKey key)
        {
            if (key.Pressed && (key.Keycode == Key.Enter || key.Keycode == Key.KpEnter))
            {
                EmitSignal(SignalName.OnLineSubmitted, lineEdit.Text);
                lineEdit.Clear();
                canGetHistory = true;
            }
            else if (key.Pressed && (key.Keycode == Key.Up))
            {
                if (canGetHistory)
                {
                    EmitSignal(SignalName.OnGetCommandHistory);
                }
                await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
                lineEdit.CaretColumn = int.MaxValue;
            }
            else if (key.Pressed && (int)key.Keycode < 128 && (int)key.Keycode > 32)
            {
                EmitSignal(SignalName.OnResetCommandHistory);
                canGetHistory = false;
            }
        }
    }

    public void SetLineText(String text)
    {
        lineEdit.Text = text;
    }

    public void SetActive(bool value)
    {
        isActive = value;
        lineEdit.Editable = value;

        if (isActive)
            lineEdit.GrabFocus();
        else
            lineEdit.ReleaseFocus();
    }
}
