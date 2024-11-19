using Godot;
using System;
using System.Diagnostics.Tracing;
using System.Threading.Tasks;

public partial class EventManager : Node
{
    PhoneController phoneController;
    PhoneNumberManager phoneNumberManager;
    Tv tv;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var conversationContainer = (ConversationContainer)GetTree().GetFirstNodeInGroup("conversation_container");
        conversationContainer.OnEventProduced += HandleEvent;

        phoneController = (PhoneController)GetTree().GetFirstNodeInGroup("phone");
        phoneNumberManager = (PhoneNumberManager)GetTree().GetFirstNodeInGroup("number_manager");
        tv = (Tv)GetTree().GetFirstNodeInGroup("TV");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    private async void HandleEvent(string eventName) 
    {
        GD.Print("HANDLING EVENT: " + eventName);

        switch(eventName) 
        {
            case "intro_call": 
                await Task.Delay(1000);
                phoneController.RingPhone(phoneNumberManager.GetConversationDataByName("intro_call"));
                break;
            case "tv_emergency_broadcast":
                tv.PlayEmergencyBroadcast();
                break;
            case "another_thing": 
                break;
        }
    }
}
