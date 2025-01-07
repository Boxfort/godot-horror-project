using System.Collections.Generic;

public class ConversationNodeNpcSpeech : ConversationNode
{
    public List<string> text;
    public int next;

    public Dictionary<string, bool> setFlags = new();

    public ConversationNodeNpcSpeech(int id, int next, List<string> text, Dictionary<string, bool>? setFlags) : base(id)
    {
        this.id = id;
        this.next = next;
        this.text = text;
        if (setFlags != null) this.setFlags = setFlags;
    }
}
