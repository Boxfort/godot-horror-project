using System.Collections.Generic;
using System.Data.Common;

public class PlayerChoiceData
{
    public int id;
    public string text;
    public int next;
    public Dictionary<string, bool> setFlags = new();
}