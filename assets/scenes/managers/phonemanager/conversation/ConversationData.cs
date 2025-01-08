using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;

public partial class ConversationData : Resource
{
    public List<ConversationNode> nodes;

    [JsonProperty(PropertyName = "entry_nodes")]
    public List<EntryNodeData> entryNodes;

    private int currentNodeIdx = 0;

    public ConversationNode EnterConversation(Dictionary<string, bool> flags) 
    {
        EntryNodeData? entryNodeData = entryNodes.Find(entry => 
            entry.requirements.All(requirement => 
                flags.GetValueOrDefault(requirement.Key, false) == requirement.Value
            )
        );

        return AdvanceToNode(entryNodeData.nodeId);
    }

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
