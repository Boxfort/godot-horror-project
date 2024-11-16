using System;
using System.Collections.Generic;
using Godot;

public class OtherSpeechNode : ConversationNode
{
    int nextNode = -1;
    List<String> text;

    public int NextNode
    {
        get => nextNode;
        set => nextNode = value;
    }
    public List<string> Text
    {
        get => text;
        set => text = value;
    }
}
