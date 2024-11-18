using System.Collections.Generic;

public class ConversationNodePlayerChoice : ConversationNode
{
    public List<PlayerChoiceData> choices;

    public ConversationNodePlayerChoice(int id, List<PlayerChoiceData> choices) : base(id)
    {
        this.choices = choices;
    }
}
