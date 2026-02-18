using Godot;


public partial class LevelGenerator : Node3D
{
	[Export] public PackedScene MobScene;
	[Export] public int MobCount = 10;
	[Export] public Vector3 SpawnAreaMin = new Vector3(-50, 0, -50);
	[Export] public Vector3 SpawnAreaMax = new Vector3(50, 0, 50);
	[Export] public float RaycastHeight = 100f;

	private RandomNumberGenerator _rng = new RandomNumberGenerator();
	private ulong _currentSeed;

	public void GenerateLevel()
	{
		_currentSeed = (ulong)GD.Randi();
		GenerateLevelWithSeed(_currentSeed);
	}

	public void GenerateLevelWithSeed(ulong seed)
	{
		_currentSeed = seed;
		_rng.Seed = seed;
		ClearMobs();
		CallDeferred(MethodName.SpawnMobs);
		GD.Print($"Level generated with seed: {seed}");
	}

	public ulong GetCurrentSeed() { return _currentSeed; }

	private void ClearMobs()
	{
		foreach (Node child in GetChildren()) { if (child is Node3D) { child.QueueFree(); } }
	}

	private bool TrySpawnMobAtRandomPosition(PhysicsDirectSpaceState3D spaceState)
	{
		float x = _rng.RandfRange(SpawnAreaMin.X, SpawnAreaMax.X);
		float z = _rng.RandfRange(SpawnAreaMin.Z, SpawnAreaMax.Z);

		// Find ground *Raycast
		var rayOrigin = new Vector3(x, RaycastHeight, z);
		var rayEnd = new Vector3(x, -RaycastHeight, z);

		var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);
		query.CollisionMask = 1;

		var result = spaceState.IntersectRay(query);

		if (result.Count == 0) return false;

		var mob = MobScene.Instantiate<CharacterBody3D>();
		var spawnPosition = (Vector3)result["position"];

		AddChild(mob);

		var player = GetTree().GetFirstNodeInGroup("player");
		Vector3 playerPosition = player != null ? ((Node3D)player).GlobalPosition : Vector3.Zero;

		if (mob is Mob mobScript) { mobScript.Initialize(spawnPosition, playerPosition); }

		return true;
	}

	private void SpawnMobs()
	{
		var spaceState = GetWorld3D().DirectSpaceState;

		if (spaceState == null)
		{	
			GD.PrintErr("DirectSpaceState is null!, Physcisworld not ready.");
			return;
		}
		int spawned = 0, attempts = 0;
		int maxAttempts = MobCount * 3;

		while (spawned < MobCount && attempts < maxAttempts)
		{
			attempts++;
			if (TrySpawnMobAtRandomPosition(spaceState)) { spawned++; }
		}

		GD.Print($"Spawned {spawned}/{MobCount} mobs"); // Success spawn msg
		if (spawned < MobCount) { GD.PushWarning($"Only spawned {spawned}/{MobCount} mobs"); }
	}
	
}

