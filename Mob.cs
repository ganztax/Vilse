using Godot;

public partial class Mob : CharacterBody3D
{
    [Signal] public delegate void MobKilledEventHandler(int XPReward);
    [Signal] public delegate void MobattackedPlayerEventHandler(int damage);

    [Export] public int MinSpeed { get; set; } = 10;
    [Export] public int MaxSpeed { get; set; } = 18;
    [Export] public int XPReward { get; set; } = 10;
    [Export] public int Damage { get; set; } = 10;
    [Export] public float AttackRange { get; set; } = 3.0f;
    [Export] public float AttackCooldown { get; set; } = 2.0f;
    [Export] public float JumpForce { get; set; } = 15.0F;

    private float _attackTimer = 0f;
    private bool _hasJumped = false;
    private Node3D _player;

    public override void _Ready()
    {
        AddToGroup("enemies");

        _player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (_player != null)
        {
            Vector3 playerPosition = _player.GlobalPosition;
            Vector3 directionToPlayer = (playerPosition - GlobalPosition);

            directionToPlayer.Y = 0;

            if (directionToPlayer.Length() > 0.1f)
            {
                float angleToPlayer = Mathf.Atan2(directionToPlayer.X, directionToPlayer.Z);
                Rotation = new Vector3(0, angleToPlayer, 0);

                RotateY((float)GD.RandRange(-Mathf.Pi / 4.0, Mathf.Pi / 4));
            }
            else { RotateY((float)GD.RandRange(0, Mathf.Pi * 2)); }
        }
        else { RotateY((float)GD.RandRange(0, Mathf.Pi * 2)); }
        
        int randomSpeed = GD.RandRange(MinSpeed, MaxSpeed);
        Velocity = Vector3.Forward * randomSpeed;
        Velocity = Velocity.Rotated(Vector3.Up, Rotation.Y);
    }
    
    public override void _PhysicsProcess(double delta) 
    {
        if (_player == null) { MoveAndSlide(); return; }
        if (_attackTimer > 0) { _attackTimer -= (float)delta; }

        float distanceToplayer = GlobalPosition.DistanceTo(_player.GlobalPosition);

        if (distanceToplayer <= AttackRange && _attackTimer <= 0 && IsOnFloor()) { JumpAttack(); }

        if (IsOnFloor() && _hasJumped) { _hasJumped = false; }
        if (!IsOnFloor()) { Velocity += Vector3.Down * 25.0f * (float)delta; }

        MoveAndSlide();
        CheckPlayerCollision();
    }

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

    private void CheckPlayerCollision()
    {
        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            KinematicCollision3D collision = GetSlideCollision(i);
            
            if (collision.GetCollider() is not Node3D collider) continue;
            if (!collider.IsInGroup("player")) continue;

            HandlePlayerHit();
            return;
        }
    }

    private void HandlePlayerHit ()
    {
        if (!_hasJumped) return;

        EmitSignal(SignalName.MobattackedPlayer, Damage);
        GD.Print($"Mob hit player for damage {Damage}");

        Velocity = -Velocity.Normalized() * MinSpeed;
        _hasJumped = false;
    }
    
    private void JumpAttack()
    {
        if (_player == null) return;
        
        Vector3 directionToPlayer = (_player.GlobalPosition - GlobalPosition);
        directionToPlayer.Y = 0;

        if (directionToPlayer.Length() > 0.1f)
        {
            float angleToPlayer = Mathf.Atan2(directionToPlayer.X, directionToPlayer.Z);
            Rotation = new Vector3(0, angleToPlayer, 0);
        }

        Vector3 fullDirection = (_player.GlobalPosition - GlobalPosition).Normalized();
        Velocity = fullDirection * MaxSpeed + Vector3.Up * JumpForce;

        _hasJumped = true;
        _attackTimer = AttackCooldown;

        GD.Print($"Mob {Name} jumps at player");
    }
}
