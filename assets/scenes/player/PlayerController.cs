using System;
using Godot;

public partial class PlayerController : CharacterBody3D
{
    Node3D head;
    public Node3D handset;

    public const float Speed = 2.0f;

    // Get the gravity from the project settings to be synced with RigidBody nodes.
    public float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();

    private const string ZoomInputAction = "zoom";

    private const string NegativeX = "move_left";
    private const string MoveRight = "move_right";
    private const string MoveForward = "move_forward";
    private const string MoveBackward = "move_backward";

    const float footstepTime = 0.75f;
    double footstepTimer = 0;

    float mouseSensitivity = 0.25f;
    float maxHeadPitch = 90;
    public bool canMoveHead = true;

    float defaultFov = 75;
    float zoomFovDecrease = 25;
    float zoomSpeed = 5;

    Vector2 mouseMovement = new Vector2();

    public InteractionController interactionController;
    public Interactable interactingWith;

    private Camera3D playerCamera;

    Inspectable currentlyInspecting;
    public Inspectable CurrentlyInspecting
    {
        get => currentlyInspecting;
        set => currentlyInspecting = value;
    }

    [Export]
    AudioStream[] footstepAudios;
    AudioStreamPlayer stepAudioPlayer;
    Random rng = new Random();

    public override void _Ready()
    {
        head = GetNode<Node3D>("Head");
        handset = head.GetNode<Node3D>("Handset");
        interactionController = GetNode<InteractionController>("Head/InteractionController");
        playerCamera = head.GetNode<Camera3D>("PlayerCamera");
        stepAudioPlayer = GetNode<AudioStreamPlayer>("FootstepAudio");

        GetNode<PlayerStateMachine>("StateMachine").Start();
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (!canMoveHead)
            return;

        if (inputEvent is InputEventMouseMotion mouseMotion)
        {
            float xMovement = -mouseMotion.Relative.X * mouseSensitivity;
            float yMovement = -mouseMotion.Relative.Y * mouseSensitivity;
            mouseMovement = new Vector2(xMovement, yMovement);

            if (mouseMovement.Y > maxHeadPitch - head.RotationDegrees.X)
            {
                // We've moved too far up
                mouseMovement.Y = maxHeadPitch - head.RotationDegrees.X;
            }
            else if (mouseMovement.Y < -maxHeadPitch - head.RotationDegrees.X)
            {
                // We've moved too far down
                mouseMovement.Y = -maxHeadPitch - head.RotationDegrees.X;
            }
        }

        RotateY(Mathf.DegToRad(mouseMovement.X));
        head.RotateX(Mathf.DegToRad(mouseMovement.Y));

        mouseMovement = Vector2.Zero;
    }

    public void AddInteractionException(CollisionObject3D node)
    {
        interactionController.AddException(node);
    }

    public void RemoveInteractionException(CollisionObject3D node)
    {
        interactionController.RemoveException(node);
    }

    public Vector3 GetLookDirection()
    {
        return head.GlobalTransform.Basis.Z.Normalized();
    }

    public Vector3 GetHeadUpDirection()
    {
        return head.GlobalTransform.Basis.Y.Normalized();
    }

    public Vector3 GetHeadPosition()
    {
        return head.GlobalPosition;
    }

    public void LookAtSmooth(Vector3 target, float lookSpeed, double delta)
    {
        Vector3 globalPosition = head.GlobalTransform.Origin;
        Vector3 originalHeadRotation = head.RotationDegrees;
        Vector3 originalBodyRotation = RotationDegrees;

        Transform3D headLookingAtTransform = head.GlobalTransform.LookingAt(
            new Vector3(target.X, target.Y, target.Z),
            Vector3.Up
        );
        var headDesiredRotation = new Quaternion(head.GlobalTransform.Basis)
            .Normalized()
            .Slerp(
                new Quaternion(headLookingAtTransform.Basis).Normalized(),
                lookSpeed * (float)delta
            );

        Transform3D lookingAtTransform = GlobalTransform.LookingAt(
            new Vector3(target.X, target.Y, target.Z),
            Vector3.Up
        );
        var desiredRotation = new Quaternion(GlobalTransform.Basis)
            .Normalized()
            .Slerp(new Quaternion(lookingAtTransform.Basis).Normalized(), lookSpeed * (float)delta);

        head.GlobalTransform = new Transform3D(
            new Basis(headDesiredRotation),
            head.GlobalTransform.Origin
        );
        GlobalTransform = new Transform3D(new Basis(desiredRotation), GlobalTransform.Origin);

        originalBodyRotation.X = 0;
        originalBodyRotation.Y = RotationDegrees.Y;
        originalBodyRotation.Z = 0;

        originalHeadRotation.X = head.RotationDegrees.X;
        originalHeadRotation.Y = 0;
        originalHeadRotation.Z = 0;

        RotationDegrees = originalBodyRotation;
        head.RotationDegrees = originalHeadRotation;
    }

    public void HandleMovement(double delta)
    {
        Vector3 velocity = Velocity;

        // Add the gravity.
        if (!IsOnFloor())
            velocity.Y -= gravity * (float)delta;

        // Get the input direction and handle the movement/deceleration.
        // As good practice, you should replace UI actions with custom gameplay actions.
        Vector2 inputDir = Input.GetVector(NegativeX, MoveRight, MoveForward, MoveBackward);
        Vector3 direction = (Transform.Basis * new Vector3(inputDir.X, 0, inputDir.Y)).Normalized();
        if (direction != Vector3.Zero)
        {
            velocity.X = direction.X * Speed;
            velocity.Z = direction.Z * Speed;
        }
        else
        {
            return;
        }

        Velocity = velocity;
        MoveAndSlide();
        HandleFootsteps(delta, inputDir);
    }

    private void HandleFootsteps(double delta, Vector2 inputDirection)
    {
        if (IsOnFloor() && inputDirection.Length() > 0.1)
        {
            footstepTimer -= delta;
            if (footstepTimer <= 0)
            {
                stepAudioPlayer.Stream = footstepAudios[rng.NextInt64(0, footstepAudios.Length)];
                stepAudioPlayer.Play();
                footstepTimer = footstepTime;
            }
        }
    }

    public bool zoomView = false;

    public void HandleZoom(double delta)
    {
        if (
            zoomView //Input.IsActionPressed(ZoomInputAction)
            && playerCamera.Fov > (defaultFov - zoomFovDecrease)
        )
        {
            playerCamera.Fov = Mathf.Lerp(
                playerCamera.Fov,
                defaultFov - zoomFovDecrease,
                (float)delta * zoomSpeed
            );
        }
        else if (
            !zoomView //!Input.IsActionPressed(ZoomInputAction) 
            && playerCamera.Fov < defaultFov
        )
        {
            playerCamera.Fov = Mathf.Lerp(playerCamera.Fov, defaultFov, (float)delta * zoomSpeed);
        }
    }
}
