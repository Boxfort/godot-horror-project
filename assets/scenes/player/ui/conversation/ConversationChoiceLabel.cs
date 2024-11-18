using System;
using Godot;

public partial class ConversationChoiceLabel : Label
{
    PlayerChoiceData choice;

    public PlayerChoiceData Choice
    {
        get => choice;
        set => SetChoice(value);
    }
    public int ChoiceIndex { get; internal set; }

    private void SetChoice(PlayerChoiceData value)
    {
        Text = ChoiceIndex.ToString() + ". " + value.text;
        choice = value;
    }
}
