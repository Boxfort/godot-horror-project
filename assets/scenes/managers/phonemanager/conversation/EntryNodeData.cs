using System.Collections.Generic;
using System.Data.Common;
using Newtonsoft.Json;

public class EntryNodeData
{
    [JsonProperty(PropertyName = "node_id")]
    public int nodeId;
    public Dictionary<string, bool> requirements = new();
}