using Godot;

public partial class Main : Node
{
	private LevelManager _levelManager;
	private CharacterBody3D _player;
	private Hud _hud;
	private DeathMenu _deathMenu;
	private PauseMenu _pauseMenu;

	public override void _Ready()
	{
		_levelManager = GetNode<LevelManager>("LevelManager");
		_player = GetNode<CharacterBody3D>("Player");
		_hud = GetNode<Hud>("UI/HUD");
		_deathMenu = GetNode<DeathMenu>("UI/DeathMenu");
		_pauseMenu = GetNode<PauseMenu>("UI/PauseMenu");
		
		_levelManager.LevelLoaded += OnLevelLoaded; 
		_deathMenu.RetryPressed += OnRetryPressed; 
		_deathMenu.NewLevelPressed += OnNewLevelPressed; 
		_pauseMenu.ResumePressed += OnResumePressed;
		_pauseMenu.KillMePressed += OnKillMePressed;

		CallDeferred(MethodName.StartNewLevel);
	}
	
	private Node3D GetSpawnPoint() { return GetTree().GetFirstNodeInGroup("player_spawn") as Node3D;	}

	private void OnLevelLoaded(int levelNumber)
	{
		var spawnPoint = GetSpawnPoint();
		if (spawnPoint is Node3D spawn) { _player.GlobalPosition = spawn.GlobalPosition; }
		GD.Print($"Level {levelNumber} loaded");
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
		var spawnPoint = GetSpawnPoint();
		if (spawnPoint is Node3D spawn)
		{
			_player.GlobalPosition = spawn.GlobalPosition;
			GD.Print($"Invoked reset - spawn");
		}

		_hud.UpdateHealth(100);
		_hud.UpdateMana(100);
	}

	private void RecaptureMouse() { Input.MouseMode = Input.MouseModeEnum.Captured; }
}
