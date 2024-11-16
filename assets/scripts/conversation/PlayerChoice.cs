using System;
using Godot;

public class PlayerChoice
{
    int nextNode = -1;
    String choiceText = "NO TEXT SET";

    public int NextNode
    {
        get => nextNode;
    }
    public string ChoiceText
    {
        get => choiceText;
    }

    public PlayerChoice(int nextNode, string choiceText)
    {
        this.nextNode = nextNode;
        this.choiceText = choiceText;
    }
}
