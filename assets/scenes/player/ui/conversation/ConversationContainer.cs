using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Godot;

public partial class ConversationContainer : Control
{
    [Signal]
    public delegate void OnConversationChoiceShowEventHandler();

    [Signal]
    public delegate void OnConversationChoiceHideEventHandler();

    [Signal]
    public delegate void OnEventProducedEventHandler(string eventName);

    [Signal]
    public delegate void OnSetConversationFlagEventHandler(string flagName);

    [Export]
    Theme LabelTheme;

    VBoxContainer optionsContainer;
    Label textLabel;

    ConversationData currentConversation;

    int optionIdx = 0;
    bool showingOptions = false;
    bool showingText = false;

    float showTextTimer = 0;
    const float showTextTime = 0.01f;

    float betweenTextTimer = 0;
    const float betweenTextTime = 2.0f;

    int textIndex = 0;
    List<String> currentText;

    // TODO: Support multiple phones maybe?
    PhoneController phone;
    AudioStreamPlayer phoneAudio;
    PhoneNumberManager numberManager;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        numberManager = (PhoneNumberManager)GetTree().GetFirstNodeInGroup("number_manager");
        optionsContainer = GetNode<VBoxContainer>("VBoxContainer");
        textLabel = GetNode<Label>("Text");

        phoneAudio = GetNode<AudioStreamPlayer>("PhoneAudio");
        phone = (PhoneController)GetTree().GetFirstNodeInGroup("phone");
        phone.OnConversationStart += OnConversationStart;
        phone.OnPhoneHangUp += OnPhoneHangUp;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        HandleShowingOptions();
        HandleShowingText((float)delta);
    }

    private void HandleShowingOptions()
    {
        if (!showingOptions)
            return;

        if (Input.IsActionJustPressed("choice_up"))
        {
            if (optionIdx > 0)
            {
                SetOptionSelected(optionIdx, false);
                optionIdx--;
                SetOptionSelected(optionIdx, true);
            }
        }

        if (
            Input.IsActionJustPressed("choice_down")
        )
        {
            if (optionIdx < optionsContainer.GetChildCount() - 1)
            {
                SetOptionSelected(optionIdx, false);
                optionIdx++;
                SetOptionSelected(optionIdx, true);
            }
        }

        if (Input.IsActionJustPressed("fire"))
        {
            SelectOption();
        }
    }

    private void HandleShowingText(float delta)
    {
        if (!showingText)
            return;

        if (textLabel.VisibleCharacters >= textLabel.Text.Length)
        {
            if (betweenTextTimer >= betweenTextTime)
            {
                betweenTextTimer = 0;
                NextText();
            }
            else
            {
                betweenTextTimer += delta;
            }
        }
        else if (showTextTimer >= showTextTime)
        {
            if (textLabel.VisibleCharacters < textLabel.Text.Length)
            {
                textLabel.VisibleCharacters++;
            }

            showTextTimer = 0;
        }
        else
        {
            showTextTimer += delta;
        }
    }

    private void SelectOption()
    {
        EmitSignal(SignalName.OnConversationChoiceHide);
        showingOptions = false;
        ClearOptions();
        NextNode();
    }

    private void SetOptionSelected(int idx, bool selected)
    {
        if (idx > optionsContainer.GetChildCount() - 1)
            return;

        Label optionLabel = (Label)optionsContainer.GetChild(idx);
        optionLabel.ThemeTypeVariation = selected
            ? "LabelSelected"
            : "LabelDefault";
    }

    private void OnConversationStart(ConversationData conversation)
    {
        currentConversation = conversation;
        HandleNode(conversation.EnterConversation(numberManager.conversationFlags));
    }

    private void OnPhoneHangUp()
    {
        EndConversation();
    }

    private void OnConversationComplete()
    {
        EndConversation();
        phone.OnConversationComplete();
    }

    private void EndConversation()
    {
        if (currentConversation == null)
            return;

        currentConversation.Reset();
        currentConversation = null;

        showingText = false;
        showingOptions = false;
        ClearOptions();
        textLabel.Hide();
    }

    public void HandleNode(ConversationNode node)
    {
        if (node is ConversationNodePlayerSpeech ps)
        {
            BeginShowingText(ps.text);
        }
        else if (node is ConversationNodePlayerChoice pc)
        {
            ShowOptions(pc);
        }
        else if (node is ConversationNodeNpcSpeech ns)
        {
            BeginShowingText(ns.text);
        }
        else
        {
            OnConversationComplete();
        }
    }

    private void BeginShowingText(List<String> text)
    {
        showingText = true;
        textIndex = 0;
        currentText = text;

        textLabel.Text = currentText[textIndex];
        textLabel.Show();
        textLabel.VisibleCharacters = 0;
    }

    private void NextText()
    {
        textIndex++;

        // If we have no more text to show
        if (textIndex > currentText.Count - 1)
        {
            showingText = false;
            textLabel.Hide();
            textIndex = 0;
            NextNode();
        }
        else
        {
            textLabel.Text = currentText[textIndex];
            textLabel.VisibleCharacters = 0;
        }
    }

    private void NextNode()
    {
        ConversationNode node = currentConversation.GetCurrentNode();
        int nextNodeId = -1;

        if (node is ConversationNodePlayerSpeech ps)
        {
            nextNodeId = ps.next;
        }
        else if (node is ConversationNodeNpcSpeech os)
        {
            numberManager.SetAllFlags(os.setFlags);
            nextNodeId = os.next;
        }
        else if (node is ConversationNodePlayerChoice pc)
        {
            PlayerChoiceData choice = pc.GetValidChoices(numberManager.conversationFlags)[optionIdx];
            numberManager.SetAllFlags(choice.setFlags);
            nextNodeId = choice.next;
        }

        if (nextNodeId != -1)
        {
            HandleNode(currentConversation.AdvanceToNode(nextNodeId));
        }
        else
        {
            OnConversationComplete();
        }
    }

    public void ShowOptions(ConversationNodePlayerChoice node)
    {
        ClearOptions();
        optionIdx = 0;

        int i = 1;
        foreach (PlayerChoiceData choice in node.GetValidChoices(numberManager.conversationFlags))
        {
            ConversationChoiceLabel optionLabel = new ConversationChoiceLabel();
            optionLabel.ChoiceIndex = i;
            optionLabel.Choice = choice;
            optionLabel.Theme = LabelTheme;
            optionLabel.ThemeTypeVariation =
                i == 1 ? "LabelSelected" : "LabelDefault";
            optionsContainer.AddChild(optionLabel);
            i++;
        }

        // If we don't have any valid options exit the call.
        if (i == 1)
        {
            OnConversationComplete();
        }
        else
        {
            showingOptions = true;
            EmitSignal(SignalName.OnConversationChoiceShow);
        }
    }

    public void ClearOptions()
    {
        foreach (Node c in optionsContainer.GetChildren())
        {
            c.QueueFree();
        }
    }

}
