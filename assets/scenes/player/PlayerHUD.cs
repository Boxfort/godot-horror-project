using System;
using Godot;

public partial class PlayerHUD : CanvasLayer
{
    Camera3D playerCamera;
    InteractionController interactionController;
    Control hoverText;

    Control interactOutlineContainer;
    Hoverable currentHoverable;

    Label phoneText;

    Control carryAction;
    Control interfaceAction;
    Control interactAction;
    Control sayResponseAction;
    Control chooseResponseAction;
    Control putDownAction;
    Control flipPageAction;
    Control hangUpAction;

    FadeIn fadeIn;

    bool isCarrying = false;
    bool isInspecting = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        playerCamera = GetNode<Camera3D>("../Head/PlayerCamera");
        interactOutlineContainer = GetNode<Control>("UI/InteractOutlineContainer");

        interactionController = GetNode<InteractionController>("../Head/InteractionController");
        interactionController.OnRaycastEnter += OnInteractRaycastEnter;
        interactionController.OnRaycastExit += OnInteractRaycastExit;
        interactionController.OnBeginCarry += OnBeginCarry;
        interactionController.OnEndCarry += OnEndCarry;

        hoverText = GetNode<Control>("UI/HoverLabel");
        phoneText = GetNode<Label>("UI/PhoneText");

        carryAction = GetNode<Control>("UI/ActionsContainer/CarryAction");
        interfaceAction = GetNode<Control>("UI/ActionsContainer/InterfaceAction");
        interactAction = GetNode<Control>("UI/ActionsContainer/InteractAction");
        sayResponseAction = GetNode<Control>("UI/ActionsContainer/SayResponseAction");
        chooseResponseAction = GetNode<Control>("UI/ActionsContainer/ChooseResponseAction");
        putDownAction = GetNode<Control>("UI/ActionsContainer/PutDownAction");
        flipPageAction = GetNode<Control>("UI/ActionsContainer/FlipPageAction");
        hangUpAction = GetNode<Control>("UI/ActionsContainer/HangUpAction");

        foreach (Inspectable notepad in GetTree().GetNodesInGroup("inspectable_pages"))
        {
            notepad.OnBeginInspecting += OnInspectPages;
            notepad.OnEndInspecting += OnEndInspectPages;
        }

        foreach (Inspectable inspect in GetTree().GetNodesInGroup("inspectable"))
        {
            inspect.OnBeginInspecting += OnInspect;
            inspect.OnEndInspecting += OnEndInspect;
        }

        fadeIn = GetNode<FadeIn>("FadeIn");
    }

    public void FadeIn()
    {
        fadeIn.DoFadeIn();
    }

    private void OnEndInspectPages()
    {
        isInspecting = false;
        putDownAction.Hide();
        flipPageAction.Hide();
        interactionController.ResetState();
        if (isCarrying) carryAction.Show();
    }

    private void OnInspectPages()
    {
        isInspecting = true;
        putDownAction.Show();
        flipPageAction.Show();
        carryAction.Hide();
    }

    private void OnInspect()
    {
        isInspecting = true;
        putDownAction.Show();
        carryAction.Hide();
    }

    private void OnEndInspect()
    {
        isInspecting = false;
        putDownAction.Hide();
        interactionController.ResetState();
        if (isCarrying) carryAction.Show();
    }

    private void OnEndCarry()
    {
        isCarrying = false;
        carryAction.GetNode<Label>("Label").Text = "Carry";
        carryAction.Hide();
    }

    private void OnBeginCarry(PhysicsBody3D carryable)
    {
        isCarrying = true;
        carryAction.GetNode<Label>("Label").Text = "Drop";
        carryAction.Show();
    }

    private void OnConversationChoicesShow()
    {
        sayResponseAction.Show();
        chooseResponseAction.Show();
    }

    private void OnPhoneNumberCleared()
    {
        phoneText.Text = "";
        phoneText.Visible = false;
    }

    private void OnPhoneKeyPressed(string dialedNumber)
    {
        phoneText.Visible = true;
        phoneText.Text = dialedNumber;
    }    
    
    private void OnPhonePickedUp()
    {
        hangUpAction.Show();
    }

    private void OnPhoneHangUp()
    {
        hangUpAction.Hide();
        OnPhoneNumberCleared();
    }

    private void OnInteractRaycastExit()
    {
        currentHoverable = null;
        interactOutlineContainer.Visible = false;
        hoverText.GetNode<Label>("Text").Text = "";
        hoverText.Visible = false;

        carryAction.Visible = false;
        interfaceAction.Visible = false;
        interactAction.Visible = false;
    }

    private void OnInteractRaycastEnter(Hoverable hoverable)
    {
        currentHoverable = hoverable;
        interactOutlineContainer.Visible = true;
        hoverText.GetNode<Label>("Text").Text = currentHoverable.HoverString;
        hoverText.Visible = true;

        carryAction.Visible = hoverable is Carryable && !isInspecting;
        interfaceAction.Visible = hoverable is Interfaceable;
        interactAction.Visible = (hoverable is Interactable i) && i.CanInteract && !isInspecting;

        if (hoverable is Interactable interactable)
        {
            interactAction.GetNode<Label>("Label").Text = interactable.InteractString;
        }
    }

    public override void _Process(double delta)
    {
        if (currentHoverable != null)
        {
            PlaceHoverUI();
        }
    }

    private void PlaceHoverUI()
    {
        Vector3[] endpoints = GetAABBGlobalEndpoints(currentHoverable);
        Vector2[] screenspacePoints = new Vector2[8];

        for (int i = 0; i < 8; i++)
        {
            screenspacePoints[i] = playerCamera.UnprojectPosition(endpoints[i]);
        }

        Vector2[] cornerPositions = GetCornerPositions(screenspacePoints);

        for (int i = 0; i < 4; i++)
        {
            ((Control)interactOutlineContainer.GetChild(i)).Position = cornerPositions[i];
        }

        hoverText.Position = cornerPositions[0];
    }

    private Vector2[] GetCornerPositions(Vector2[] pointArray)
    {
        float minX = pointArray[0].X;
        float minY = pointArray[0].Y;
        float maxX = pointArray[0].X;
        float maxY = pointArray[0].Y;

        for (int i = 0; i < 8; i++)
        {
            if (pointArray[i].X < minX)
                minX = pointArray[i].X;
            if (pointArray[i].Y < minY)
                minY = pointArray[i].Y;
            if (pointArray[i].X > maxX)
                maxX = pointArray[i].X;
            if (pointArray[i].Y > maxY)
                maxY = pointArray[i].Y;
        }

        return new Vector2[]
        {
            new Vector2(minX, minY),
            new Vector2(minX, maxY),
            new Vector2(maxX, minY),
            new Vector2(maxX, maxY)
        };
    }

    private Vector3[] GetAABBGlobalEndpoints(Hoverable interactable)
    {
        ArrayMesh mesh = interactable
            .GetNode<CollisionShape3D>("CollisionShape3D")
            .Shape.GetDebugMesh();
        Aabb aabb = mesh.GetAabb();

        Vector3[] globalEndpoints = new Vector3[8];

        for (int i = 0; i < 8; i++)
        {
            Vector3 localEndpoint = aabb.GetEndpoint(i);
            Vector3 globalEndpoint = interactable
                .GetNode<CollisionShape3D>("CollisionShape3D")
                .ToGlobal(localEndpoint);

            globalEndpoints[i] = globalEndpoint;
        }

        return globalEndpoints;
    }
}
