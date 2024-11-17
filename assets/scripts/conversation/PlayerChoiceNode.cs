using System;
using System.Collections.Generic;
using Godot;

public class PlayerChoiceNode : ConversationNode
{
    List<PlayerChoice> choices;

    public PlayerChoiceNode(int id, List<PlayerChoice> choices): base(id)
    {
        this.id = id;
        this.choices = choices;
    }

    public List<PlayerChoice> Choices
    {
        get => choices;
    }
}
