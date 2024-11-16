using System;
using System.Collections.Generic;
using Godot;

public abstract partial class Carryable : Interactable
{
    [Signal]
    public delegate void StopCarryingEventHandler();

    const float terminalVelocity = 2f;
    protected const float gravity = 1.0f;

    [Export]
    protected bool shouldBounce = true;
    protected float velocity = 0;
    protected float velocityDecay = 20;

    [Export]
    protected float friction = 0;
    protected bool hasBeenThrown = false;
    protected bool isFalling = true;

    protected Vector3 gravityVec = Vector3.Zero;
    protected Vector3 direction = Vector3.Zero;
    Vector3 snap = Vector3.Zero;
    float maxSlope = Mathf.DegToRad(45);

    public virtual float CarryDistance => 1.0f;
    public virtual Vector3 CarryRotationDegreesOffset => new Vector3(0, 180, 0);

    Vector3 resetPosition;
    Vector3 ResetPosition
    {
        get => resetPosition;
        set => resetPosition = value;
    }

    public virtual void OnCollide(KinematicCollision3D collision) { }

    public virtual void OnCarry()
    {
        isFalling = false;
        gravityVec = Vector3.Zero;
    }

    public void QueueFreeAndDrop()
    {
        EmitSignal(SignalName.StopCarrying);
        QueueFree();
    }

    public virtual void OnThrow(Vector3 direction, float force)
    {
        hasBeenThrown = true;
        isFalling = true;
        this.direction = direction;
        velocity = force;
    }

    public virtual void OnDrop(Vector3 direction, float force)
    {
        isFalling = true;
        this.direction = direction;
        velocity = force;
    }

    public override void _Ready()
    {
        ResetPosition = GlobalPosition;
        GD.Print("Reset position set as " + ResetPosition);
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isOutOfBounds())
        {
            GD.Print("RESET TO " + ResetPosition);
            GlobalPosition = ResetPosition;
        }
        else if (isFalling)
        {
            HandleFalling(delta);
        }
    }

    private bool isOutOfBounds()
    {
        return GlobalPosition.Y < -10;
    }

    Vector3 lastRemainder = Vector3.Zero;

    private void HandleFalling(double delta)
    {
        gravityVec += Vector3.Down * gravity * (float)delta;
        gravityVec.Y = Mathf.Clamp(gravityVec.Y, -terminalVelocity, terminalVelocity);
        var movement = (direction * velocity * (float)delta) + gravityVec;
        var collision = MoveAndCollide(movement);
        bool onFloor = false;

        if (collision != null)
        {
            OnCollide(collision);

            float dotProduct = Vector3.Up.Dot(collision.GetNormal());
            lastRemainder = collision.GetRemainder();

            var reflect = lastRemainder.Bounce(collision.GetNormal());
            direction = movement.Bounce(collision.GetNormal()).Normalized();

            if (collision.GetNormal().AngleTo(Vector3.Up) <= maxSlope)
            {
                // We're only on the floor if we're colliding with the world. We don't want to stop on top of somethings head.
                if (
                    ((PhysicsBody3D)collision.GetCollider()).GetCollisionLayerValue(1)
                    && !(collision.GetCollider() is Carryable)
                )
                {
                    onFloor = true;
                    velocity = Mathf.Max(velocity - (friction * (float)delta), 0);
                    if (!shouldBounce)
                    {
                        gravityVec = Vector3.Zero;
                        direction.Y = 0;
                        reflect.Y = 0;
                    }
                }

                if (!shouldBounce)
                    reflect.Y = 0;
            }

            if (reflect.Length() > 0.1)
                MoveAndCollide(reflect);

            if (shouldBounce && dotProduct > 0)
            {
                gravityVec *= 1 - dotProduct;
            }
        }

        velocity = Mathf.Max(velocity - (velocityDecay * (float)delta), 0);

        if (onFloor && lastRemainder.Length() <= 0.05)
        {
            isFalling = false;
        }
    }

    protected void HandleGravity(double delta)
    {
        if (IsOnFloor())
        {
            snap = -GetFloorNormal();
            gravityVec = Vector3.Zero;
        }
        else
        {
            snap = Vector3.Down;
            gravityVec += Vector3.Down * gravity * (float)delta;
        }
    }

    /*
        public Dictionary<string, object> Save()
        {
            return new Dictionary<string, object> {
                {"global_transform", GlobalTransform},
                {"velocity", velocity},
                {"has_been_thrown", hasBeenThrown},
                {"gravity_vec", gravityVec},
                {"direction", direction},
                {"snap", snap},
            };
        }

        public void Load(Dictionary<string, object> data)
        {
            GlobalTransform = JsonConvert.DeserializeObject<Transform3D>(data["global_transform"].ToString());
            velocity = JsonConvert.DeserializeObject<float>(data["velocity"].ToString());
            hasBeenThrown = (bool)data["has_been_thrown"];
            gravityVec = JsonConvert.DeserializeObject<Vector3>(data["gravity_vec"].ToString());
            direction = JsonConvert.DeserializeObject<Vector3>(data["direction"].ToString());
            snap = JsonConvert.DeserializeObject<Vector3>(data["snap"].ToString());
        }
    */
}
