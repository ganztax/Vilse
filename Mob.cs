using Godot;

public partial class Mob : CharacterBody3D
{
    [Signal] public delegate void MobKilledEventHandler(int XPReward);

    [Export] public int MinSpeed { get; set; } = 10;
    [Export] public int MaxSpeed { get; set; } = 18;
    [Export] public int XPReward { get; set; } = 10;

    public override void _Ready()
    {
        AddToGroup("enemies");

        var player = GetTree().GetFirstNodeInGroup("player");
        if (player != null)
        {
            Vector3 playerPosition = ((Node3D)player).GlobalPosition;
            LookAt(playerPosition, Vector3.Up);
            RotateY((float)GD.RandRange(-Mathf.Pi / 4.0, Mathf.Pi / 4.0));
        }
        else { RotateY((float)GD.RandRange(0, Mathf.Pi * 2)); }
        
        int randomSpeed = GD.RandRange(MinSpeed, MaxSpeed);
        Velocity = Vector3.Forward * randomSpeed;
        Velocity = Velocity.Rotated(Vector3.Up, Rotation.Y);
    }
    
    public override void _PhysicsProcess(double delta) { MoveAndSlide(); }

    public void Die()
    {
        GD.Print($"Mob {Name} killed! Recieved {XPReward} XP.");
        EmitSignal(SignalName.MobKilled, XPReward);
        QueueFree();
    }

    public void Initialize(Vector3 startPosition, Vector3 playerPosition)
    {
        LookAtFromPosition(startPosition, playerPosition, Vector3.Up);
        RotateY((float)GD.RandRange(-Mathf.Pi / 4.0, Mathf.Pi / 4.0));

        int randomSpeed = GD.RandRange(MinSpeed, MaxSpeed);
        Velocity = Vector3.Forward * randomSpeed;    
        Velocity = Velocity.Rotated(Vector3.Up, Rotation.Y);    
    }
}
