using Godot;

public partial class LevelManager : Node
{
	[Export] public PackedScene ProceduralLevelScene;
	[Export] public PackedScene BossLevel1Scene;
	[Export] public PackedScene HubWorldScene;

	private Node3D _currentLevelInstance;
	private Node3D _levelContainer;
	private int _currentLevelNumber = 0;
	private ulong _lastProceduralSeed;
	
	public override void _Ready()
	{
		_levelContainer = GetParent().GetNode<Node3D>("CurrentLevel");
		LoadLevel(0);
	}

	public void LoadLevel(int levelNumber)
	{
		_currentLevelNumber = levelNumber;
		ClearCurrentLevel();

		PackedScene levelScene = GetLevelScene(levelNumber);
		_currentLevelInstance = levelScene.Instantiate<Node3D>();
		_levelContainer.AddChild(_currentLevelInstance);

		if (IsProceduralLevel(levelNumber))
		{
			var generator = _currentLevelInstance.GetNode<LevelGenerator>("LevelGenerator");
			CallDeferred(MethodName.GenerateProcLevel, generator);
		}

		EmitSignal(SignalName.LevelLoaded, levelNumber);
	}
	
	public void LoadLevelWithSeed(int levelNumber, ulong seed)
	{
		_currentLevelNumber = levelNumber;
		ClearCurrentLevel();

		_currentLevelInstance = ProceduralLevelScene.Instantiate<Node3D>();
		_levelContainer.AddChild(_currentLevelInstance);

		var generator = _currentLevelInstance.GetNodeOrNull<LevelGenerator>("LevelGenerator");
		if (generator != null) { CallDeferred(MethodName.LoadProcLevelWithSeed, generator, seed); }
	}

	private void GenerateProcLevel(LevelGenerator generator)
	{
		generator.GenerateLevel();
		_lastProceduralSeed = generator.GetCurrentSeed();
	}

	private void LoadProcLevelWithSeed(LevelGenerator generator, ulong seed)
	{
		generator.GenerateLevelWithSeed(seed);
		_lastProceduralSeed = seed;
	}

	public void RetryCurrentLevel()
	{
		if (IsProceduralLevel(_currentLevelNumber))
		{
			ClearCurrentLevel();
			_currentLevelInstance = ProceduralLevelScene.Instantiate<Node3D>();
			_levelContainer.AddChild(_currentLevelInstance);

			var generator = _currentLevelInstance.GetNode<LevelGenerator>("LevelGenerator");
			CallDeferred(MethodName.RetryProcLevel, generator);
		}
		else { LoadLevel(_currentLevelNumber); }
	}

	private void RetryProcLevel(LevelGenerator generator) => generator.GenerateLevelWithSeed(_lastProceduralSeed); 

	public ulong GetLastSeed() => _lastProceduralSeed; 

	public void LoadNextLevel()
	{
		_currentLevelNumber++;
		LoadLevel(_currentLevelNumber);
	}

	private PackedScene GetLevelScene(int levelNumber)
	{
		return levelNumber switch
		{
			5 => HubWorldScene,
			8 => BossLevel1Scene,
			_ => ProceduralLevelScene
		};
	}
	
	private bool IsProceduralLevel(int levelNumber)
	{
		return levelNumber switch
		{
			5 => false,
			8 => false,
			_ => true
		};
	}

	private void ClearCurrentLevel()
	{
		if (_currentLevelInstance != null)
		{
			_currentLevelInstance.QueueFree();
			_currentLevelInstance = null;
		}
	}

	[Signal] public delegate void LevelLoadedEventHandler(int levelNumber);
}
