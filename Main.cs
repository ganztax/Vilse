using Godot;

public partial class Main : Node
{
	private LevelManager _levelManager;
	private XPManager _xpManager;
	private Player _player;
	private Hud _hud;
	private DeathMenu _deathMenu;
	private PauseMenu _pauseMenu;

	public override void _Ready()
	{
		_levelManager = GetNode<LevelManager>("LevelManager");
		_xpManager = GetNode<XPManager>("XPManager");		
		_player = GetNode<Player>("Player");
		_hud = GetNode<Hud>("UI/HUD");
		_deathMenu = GetNode<DeathMenu>("UI/DeathMenu");
		_pauseMenu = GetNode<PauseMenu>("UI/PauseMenu");

		_levelManager.LevelLoaded += OnLevelLoaded; 
		_deathMenu.RetryPressed += OnRetryPressed; 
		_deathMenu.NewLevelPressed += OnNewLevelPressed; 
		_pauseMenu.ResumePressed += OnResumePressed;
		_pauseMenu.KillMePressed += OnKillMePressed;
		_xpManager.XPGained += OnXpGained;
		_xpManager.LevelUp += OnLevelUp;

		CallDeferred(MethodName.StartNewLevel);
	}
	
	private Vector3 GetSpawnPoint() 
	{ 
		var spawn = GetTree().GetFirstNodeInGroup("player_spawn") as Node3D;
		return 	spawn?.GlobalPosition ?? new Vector3(0, 5, 0);
	}

	private void OnLevelLoaded(int levelNumber)
	{
		_player.GlobalPosition = GetSpawnPoint();
		_player.ResetHealth();

		ConnectToMobs();
		GD.Print($"Level {levelNumber} loaded");
	}

	private void ConnectToMobs()
	{
		var mobs = GetTree().GetNodesInGroup("enemies");

		foreach (var mob in mobs)
		{
			if (mob is Mob mobscript)
				ConnectToMob(mobscript);
		}
		GD.Print($"Connected to {mobs.Count} mobs");
	}

	private void ConnectToMob(Mob mob)
	{
		var xpCallable = Callable.From<int>(OnMobKilled);
		if (mob.IsConnected(Mob.SignalName.MobKilled, xpCallable))
			mob.Disconnect(Mob.SignalName.MobKilled, xpCallable);
		mob.MobKilled += OnMobKilled;

		var dmgCallable = Callable.From<int>(OnMobAttackedPlayer);
		if (mob.IsConnected(Mob.SignalName.MobattackedPlayer, dmgCallable))
			mob.Disconnect(Mob.SignalName.MobattackedPlayer, dmgCallable);
		mob.MobattackedPlayer += OnMobAttackedPlayer;		

	}

	private void OnMobAttackedPlayer(int damage)
	{
		if (_player is Player player) { player.TakeDamage(damage); }
	}
	private void OnMobKilled(int xpReward) { _xpManager.AddXp(xpReward); }

	private void OnXpGained(int amount, int newTotal)
	{
		float progress = _xpManager.GetXPProgress();
		_hud.UpdateExperience(progress, _xpManager.CurrentLevel);
	}

	private void OnLevelUp(int newLevel)
	{
		GD.Print($"You've reached level {newLevel}!");
		_hud.UpdateExperience(0, newLevel);
	}

	private void StartNewLevel()
	{
		_levelManager.LoadLevel(1);
		GD.Print($"New level started with seed: {_levelManager.GetLastSeed()}");
	}

	private void OnResumePressed() { GD.Print("Game Resumed"); }

	private void OnKillMePressed()
	{
		_pauseMenu.HideMenu();
		CallDeferred(MethodName.OnPlayerDied);
	}

	private void OnPlayerHealthChanged(int currentHealth, int maxHealth) { _hud.UpdateHealth(currentHealth); }

	private void OnPlayerDied() { _deathMenu.ShowMenu(_levelManager.GetLastSeed()); }

	private void OnRetryPressed()
	{
		_levelManager.RetryCurrentLevel();
		ResetPlayer();
		CallDeferred(MethodName.RecaptureMouse);
	}

	private void OnNewLevelPressed() 
	{
		StartNewLevel();
		ResetPlayer();
		CallDeferred(MethodName.RecaptureMouse);
	}

	private void ResetPlayer()
	{
		_hud.UpdateHealth(100);
		_hud.UpdateMana(100);
	}

	private void RecaptureMouse() { Input.MouseMode = Input.MouseModeEnum.Captured; }
}
