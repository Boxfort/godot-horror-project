using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class PhoneManager : Node
{
    const string phoneDataPath = "res://assets/scenes/managers/phonemanager/phone_data.json";

    public override void _Ready()
    {
        GD.Print("READING JSON");
        ReadData();
        GD.Print("JSON READ SUCCESSFULLY.");
    }

    public void ReadData()
    {
        using var dataFile = Godot.FileAccess.Open(phoneDataPath, Godot.FileAccess.ModeFlags.Read);
        var dataJson = dataFile.GetAsText();

        PhoneData phoneData = JsonConvert.DeserializeObject<PhoneData>(dataJson);

        GD.Print("textcount : " + (phoneData.numbers["001"].conversations.First().nodes.First() as OtherSpeechNode).Text.Count);

        var json = JsonConvert.SerializeObject(phoneData, Formatting.Indented);
        GD.Print(json);
    }
}

public class PhoneData
{
    public Dictionary<string, PhoneNumberData> numbers;
}

public class PhoneNumberData
{
    public List<ConversationData> conversations;
}

public class ConversationData
{
    public int id;
    public List<string> requirements;

    public List<ConversationNode> nodes;
}

public class ConversationNodeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return typeof(ConversationNode).IsAssignableFrom(objectType);
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);

        string nodeType = jo["type"].ToString();

        ConversationNode item;
        if (nodeType == "npc_speech")
        {
            item = new OtherSpeechNode(
                jo["id"].ToObject<int>(),
                jo["next"].ToObject<int>(),
                jo["text"].Children().Select((x) => x.ToString()).ToList()
            );
        }
        else if (nodeType == "player_choice")
        {
            item = new ConversationNodePlayerChoice(
                jo["id"].ToObject<int>(),
                jo["choices"].ToObject<List<PlayerChoice>>()
            );
        }
        else
        {
            item = new ConversationNode(
                jo["id"].ToObject<int>()
            );
        }

        return item;
    }


    public override bool CanWrite => false;
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}


[JsonConverter(typeof(ConversationNodeConverter))]
public class ConversationNode
{
    public int id;

    public ConversationNode(int id)
    {
        this.id = id;
    }
}

public class ConversationNodeNpcSpeech : ConversationNode
{
    public List<string> text;

    public ConversationNodeNpcSpeech(int id) : base(id)
    {
    }
}

public class ConversationNodePlayerChoice : ConversationNode
{
    public List<PlayerChoice> choices;

    public ConversationNodePlayerChoice(int id, List<PlayerChoice> choices) : base(id)
    {
        this.choices = choices;
    }
}

public class PlayerChoice
{
    public int id;
    public string text;
    public string next;
}