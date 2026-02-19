using Godot;

public partial class PauseMenu : CanvasLayer
{
	[Signal] public delegate void ResumePressedEventHandler();
	[Signal] public delegate void KillMePressedEventHandler();

	private Button _resumeButton;
	private Button _killMeButton;
	private Button _quitButton;

		public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;

		_resumeButton = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/ResumeButton");
		_killMeButton = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/KillMeButton");
		_quitButton = GetNode<Button>("CenterContainer/PanelContainer/VBoxContainer/QuitButton");

		_resumeButton.ProcessMode = ProcessModeEnum.Always;
		_killMeButton.ProcessMode = ProcessModeEnum.Always;
		_quitButton.ProcessMode = ProcessModeEnum.Always;

		_resumeButton.Pressed += OnResumePressed;
		_killMeButton.Pressed += OnKillMePressed;
		_quitButton.Pressed += OnQuitPressed;

		Hide();
	}

    public override void _Input(InputEvent @event)
    {
		if (@event.IsActionPressed("ui_cancel"))
		{
			( Visible ? (System.Action) HideMenu : ShowMenu )();
			GetViewport().SetInputAsHandled();
		}
    }


	public void ShowMenu()
	{
		Show();
		Input.MouseMode = Input.MouseModeEnum.Visible;
		GetTree().Paused = true;
	}

	public void HideMenu()
	{
		Hide();
		Input.MouseMode = Input.MouseModeEnum.Captured;
		GetTree().Paused = false;
	}
	
	private void OnResumePressed()
	{
		EmitSignal(SignalName.ResumePressed);
		HideMenu();
	}

	private void OnKillMePressed()
	{
		EmitSignal(SignalName.KillMePressed);
		HideMenu();
	}

	private void OnQuitPressed() { GetTree().Quit(); }
}
