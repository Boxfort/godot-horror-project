using System.Collections.Generic;
using System.Data.Common;
using Newtonsoft.Json;

public class PlayerChoiceData
{
    public int id;
    public string text;
    public int next;
    public Dictionary<string, bool> requirements = new();

    [JsonProperty(PropertyName = "set_flags")]
    public Dictionary<string, bool> setFlags = new();
}