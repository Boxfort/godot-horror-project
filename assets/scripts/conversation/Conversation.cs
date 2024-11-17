using System;
using System.Collections.Generic;
using Godot;

public partial class Conversation : Resource
{
    Dictionary<int, ConversationNode> nodes = new Dictionary<int, ConversationNode>();
    int currentNode = 0;

    float voicePitchScale = 1.0f;

    public float VoicePitchScale
    {
        get => voicePitchScale;
    }

    public Conversation(int entryNode, List<ConversationNode> nodes, float voicePitchScale = 1.0f)
    {
        this.currentNode = entryNode;
        this.voicePitchScale = voicePitchScale;

        foreach (ConversationNode n in nodes)
        {
            this.nodes.Add(n.id, n);
        }
    }

    public void Reset()
    {
        currentNode = 0;
    }

    public void AddNode(ConversationNode node)
    {
        this.nodes.Add(node.id, node);
    }

    public ConversationNode GetCurrentNode()
    {
        return nodes.GetValueOrDefault(currentNode, null);
    }

    public ConversationNode AdvanceToNode(int toNode)
    {
        currentNode = toNode;
        return GetCurrentNode();
    }
}
