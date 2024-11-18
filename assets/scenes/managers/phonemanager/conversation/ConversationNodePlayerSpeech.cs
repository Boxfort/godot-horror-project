using System.Collections.Generic;

public class ConversationNodePlayerSpeech : ConversationNode
{
    public List<string> text;
    public int next;

    public ConversationNodePlayerSpeech(int id, int next, List<string> text) : base(id)
    {
        this.id = id;
        this.next = next;
        this.text = text;
    }
}
