using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public partial class PhoneNumberManager : Node
{
    const string phoneDataPath = "res://assets/scenes/managers/phonemanager/phone_data.json";

    Dictionary<string, bool> conversationFlags = new Dictionary<string, bool>();

    PhoneNumberData phoneNumberData;

    public override void _Ready()
    {
        GD.Print("READING JSON");
        ReadData();
        GD.Print("JSON READ SUCCESSFULLY.");

        var conversationContainer = (ConversationContainer)GetTree().GetFirstNodeInGroup("conversation_container");
        conversationContainer.OnSetConversationFlag += (flag) => conversationFlags[flag] = true;
    }

    public void ReadData()
    {
        using var dataFile = Godot.FileAccess.Open(phoneDataPath, Godot.FileAccess.ModeFlags.Read);
        var dataJson = dataFile.GetAsText();
        phoneNumberData = JsonConvert.DeserializeObject<PhoneNumberData>(dataJson);
        var json = JsonConvert.SerializeObject(phoneNumberData, Formatting.Indented);
        GD.Print(json);
    }

    public ConversationData? GetConversationDataByNumber(string phoneNumber) 
    {
        List<ConversationData> conversations = phoneNumberData.numbers.GetValueOrDefault(phoneNumber, new List<ConversationData>());

        ConversationData? conversation = conversations.Find(conversation => 
            conversation.requirements.All(requirement => 
                conversationFlags.GetValueOrDefault(requirement.Key, false) == requirement.Value
            )
        );

        return conversation;
    }

        public ConversationData? GetConversationDataByName(string name) 
    {
        return phoneNumberData.named.GetValueOrDefault(name);
    }
}
