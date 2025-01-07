using System.Collections.Generic;
using System.Linq;

public class ConversationNodePlayerChoice : ConversationNode
{
    private List<PlayerChoiceData> choices;

    public List<PlayerChoiceData> GetValidChoices(Dictionary<string, bool> flags) 
    {
        List<PlayerChoiceData> valid = choices.FindAll(choice => 
            choice.requirements.All(requirement => 
                flags.GetValueOrDefault(requirement.Key, false) == requirement.Value
            )
        );

        return valid;
    }

    public ConversationNodePlayerChoice(int id, List<PlayerChoiceData> choices) : base(id)
    {
        this.choices = choices;
    }
}
