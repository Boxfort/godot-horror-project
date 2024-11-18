using System.Collections.Generic;
using Godot;

public partial class ConversationData : Resource
{
    public int id;
    public Dictionary<string, bool> requirements;
    public List<ConversationNode> nodes;

    private int currentNodeIdx = 0;

    public ConversationNode GetCurrentNode()
    {
        return nodes.Find((node) => node.id == currentNodeIdx);
    }

    public ConversationNode AdvanceToNode(int id) 
    {
        currentNodeIdx = id;
        return nodes.Find((node) => node.id == currentNodeIdx);
    }

    public void Reset()
    {
        currentNodeIdx = 0;
    }
}
