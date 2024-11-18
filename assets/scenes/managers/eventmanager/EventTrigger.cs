public enum TriggerType {
    ConversationFlag,
    Trigger
}

public class EventTrigger
{
    TriggerType type;
    string value;
    
    public EventTrigger(TriggerType type, string value)
    {
        this.type = type;
        this.value = value;
    }
}