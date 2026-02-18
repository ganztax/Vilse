using Godot;

public partial class DeathMenu : CanvasLayer
{
	private Label _seedLabel;
	private Button _retryButton;
	private Button _newLevelButton;

	public override void _Ready()
	{
		_seedLabel = GetNode<Label>("CenterContainer/PanelContainer/VBoxContainer/SeedLabel");
		_retryButton = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/RetryButton");
		_newLevelButton = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/NewLevelButton");

		_retryButton.Pressed += OnRetryPressed;
		_newLevelButton.Pressed += OnNewLevelPressed;

		Hide();
		GetTree().Paused = false;
	}

	public void ShowMenu(ulong seed)
	{
		_seedLabel.Text = $"Seed: {seed}";
		Show();
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}
	
	public void HideMenu()
	{
		Hide();
		Input.MouseMode = Input.MouseModeEnum.Hidden;
		GetTree().Paused = false;
	}
	
	private void OnRetryPressed()
	{
		EmitSignal(SignalName.RetryPressed);
		HideMenu();
	}

	private void OnNewLevelPressed()
	{
		EmitSignal(SignalName.NewLevelPressed);
		HideMenu();
	}
	
	[Signal] public delegate void RetryPressedEventHandler();
	[Signal] public delegate void NewLevelPressedEventHandler();
}
