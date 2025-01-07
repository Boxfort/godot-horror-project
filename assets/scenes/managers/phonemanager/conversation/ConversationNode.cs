using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

[JsonConverter(typeof(ConversationNodeConverter))]
public class ConversationNode
{
    public int id;

    public ConversationNode(int id)
    {
        this.id = id;
    }
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
            JToken flagToken = jo["set_flags"];
            Dictionary<string, bool> flags = null;

            if (flagToken != null) {
                flags = flagToken.ToObject<Dictionary<string, bool>>();
            } 

            item = new ConversationNodeNpcSpeech(
                jo["id"].ToObject<int>(),
                jo["next"].ToObject<int>(),
                jo["text"].ToObject<List<string>>(),
                flags
            );
        }
        else if (nodeType == "player_choice")
        {
            item = new ConversationNodePlayerChoice(
                jo["id"].ToObject<int>(),
                jo["choices"].ToObject<List<PlayerChoiceData>>()
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
