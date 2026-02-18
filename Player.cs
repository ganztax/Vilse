using Godot;

public partial class Player : CharacterBody3D
{
    [Export] public float Speed { get; set; } = 14.0f;
    [Export] public float SprintSpeed { get; set; } = 20.0f;
    [Export] public float CrouchSpeed { get; set; } = 7.0f;
    [Export] public float JumpVelocity { get; set; } = 15.0f;
    [Export] public float FallAcceleration { get; set; } = 75.0f;
    [Export] public float MouseSensitivity { get; set; } = 0.003f;
    [Export] public float AttackCoolDown { get; set; } = 0.5f;
    [Export] public float AttackDuration { get; set; } = 0.2f;

    private Node3D _head;
    private Camera3D _camera;
    private Area3D _attackArea;
    private CollisionShape3D _attackShape;
    private CollisionShape3D _collisionShape;
    private Vector3 _targetVelocity = Vector3.Zero;
    private AnimationPlayer _animationPlayer;
    private Vector3 _cameraDefaultPosition;
    private Vector3 _cameraDefaultRotation;

    private float _normalHeight = 2.0f;
    private float _crouchHeight = 1.0f;
    private bool _isCrouching = false;

    private float _attackCooldownTimer = 0f;
    private float _attackActiveTimer = 0f;
    private bool _isAttacking = false;


    public override void _Ready()
    {
        _head = GetNode<Node3D>("Head");
        _camera = _head.GetNode<Camera3D>("Camera3D");
        _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        _attackArea = _head.GetNode<Area3D>("AttackArea");
        _attackShape = _attackArea.GetNode<CollisionShape3D>("AttackShape");
        _animationPlayer = _head.GetNode<AnimationPlayer>("AnimationPlayer");

        _cameraDefaultPosition = _camera.Position;
        _cameraDefaultRotation = _camera.Rotation;

        if (_collisionShape.Shape is BoxShape3D box)
        {
            _normalHeight = box.Size.Y;
            _crouchHeight = _normalHeight / 2.0f;
        }

        var mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
        if (mesh != null) { mesh.Visible = false; }

        _attackArea.BodyEntered += OnAttackHit;
        _attackShape.Disabled = true;

        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);
            _head.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);

            Vector3 headRotation = _head.Rotation;
            headRotation.X = Mathf.Clamp(headRotation.X, Mathf.DegToRad(-89), Mathf.DegToRad(89));
            _head.Rotation = headRotation;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var direction = Vector3.Zero;
        float currentSpeed = Speed;

        if (Input.MouseMode != Input.MouseModeEnum.Captured) return;
        if (Input.IsActionPressed("move_forward")) { direction -= Transform.Basis.Z; }
        if (Input.IsActionPressed("move_backwards")) { direction += Transform.Basis.Z; }
        if (Input.IsActionPressed("move_right")) { direction += Transform.Basis.X; }
        if (Input.IsActionPressed("move_left")) { direction -= Transform.Basis.X; }
        if (direction != Vector3.Zero) { direction = direction.Normalized(); }

        HandleCrouch(delta);
        HandleAttack(delta);

        if (_isCrouching) 
            currentSpeed = CrouchSpeed;
        else if ( Input.IsActionPressed("sprint"))
            currentSpeed = SprintSpeed;

        _targetVelocity.X = direction.X * currentSpeed;
        _targetVelocity.Z = direction.Z * currentSpeed;

        if (IsOnFloor() && Input.IsActionJustPressed("jump") && !_isCrouching) { _targetVelocity.Y = JumpVelocity; } 
        if (!IsOnFloor()) { _targetVelocity.Y -= FallAcceleration * (float)delta; }

        Velocity = _targetVelocity;
        MoveAndSlide();
    }
    
    private void HandleAttack(double delta)
    {
        if (_attackCooldownTimer > 0) { _attackCooldownTimer -= (float)delta; }    

        if (_isAttacking)
        {
            _attackActiveTimer -= (float)delta;
            if (_attackActiveTimer <= 0) { EndAttack(); }
        }

        if (Input.IsActionJustPressed("attack") && _attackCooldownTimer <= 0 && !_isAttacking) { StartAttack(); }
    }

    private void StartAttack()
    {
        _isAttacking = true;
        _attackActiveTimer = AttackDuration;
        _attackCooldownTimer = AttackCoolDown;
        _attackShape.Disabled = false;

        if ( _animationPlayer != null && _animationPlayer.HasAnimation("swing")) 
            _animationPlayer.Play("swing");
        else 
            PlaySwingTween();
        
        GD.Print("player attacked started");
    }

    private void EndAttack()
    {
        _isAttacking = false;
        _attackShape.Disabled = true;
        GD.Print("player attacked ended");
    }

    private void OnAttackHit(Node3D body)
    {
        if (!_isAttacking) return;
        if (body.IsInGroup("enemies") && body is Mob mob) { mob.Die(); }
    }

    private void HandleCrouch(double delta)
    {
        bool wantsToCrouch = Input.IsActionPressed("crouch");

        if (wantsToCrouch && !_isCrouching) 
         StartCrouch(); 
        else if (!wantsToCrouch && _isCrouching && CanStandup()) 
         StopCrouch();
    }

    private void StartCrouch()
    {
        _isCrouching = true;
        SetCollisionHeight(_crouchHeight);
        _head.Position = new Vector3(0, _crouchHeight / 2.0f, 0);
    }

    private void StopCrouch()
    {
        _isCrouching = false;
        SetCollisionHeight(_normalHeight);
        _head.Position = new Vector3(0, _normalHeight / 2.0f, 0);
    }
    
    private void SetCollisionHeight(float height)
    {
         if (_collisionShape.Shape is BoxShape3D box) { box.Size = new Vector3(box.Size.X, height, box.Size.Z); } 
    }

    private bool CanStandup()
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(
            GlobalPosition,
            GlobalPosition + Vector3.Up * (_normalHeight - _crouchHeight)
        );
        
        query.Exclude = new Godot.Collections.Array<Rid> { GetRid() };
        var result = spaceState.IntersectRay(query);
        return result.Count == 0;
    }

    private void PlaySwingTween()
    {
        var tween = CreateTween();
        tween.SetParallel(true);

        tween.TweenProperty(_camera, "position", _cameraDefaultPosition + new Vector3(0, -0.1f, -0.3f), 0.1);
        tween.TweenProperty(_camera, "rotation", _cameraDefaultRotation + new Vector3(0.087f, 0, -0.052f), 0.1);
        
        tween.SetParallel(false);

        tween.TweenProperty(_camera, "position", _cameraDefaultPosition, 0.2);
        tween.TweenProperty(_camera, "rotaton", _cameraDefaultRotation, 0.2);
    }
}
