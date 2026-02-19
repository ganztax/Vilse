using Godot;

public partial class Hud : CanvasLayer
{
	private ProgressBar _healthBar;
	private ProgressBar _manaBar;
	private ProgressBar _experienceBar;
	private Label _levelLabel;

	[Export] public float MaxHealth { get; set; } = 100f;
	[Export] public float MaxMana { get; set; } = 100f;

	public override void _Ready()
	{
		_healthBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/HealthBar");
		_manaBar = GetNode<ProgressBar>("MarginContainer/VBoxContainer/ManaBar");
		_experienceBar	 = GetNode<ProgressBar>("MarginContainer/VBoxContainer/ExperienceBar");
		_levelLabel = GetNode<Label>("MarginContainer/VBoxContainer/LevelLabel");

		_healthBar.MaxValue = MaxHealth;
		_manaBar.MaxValue = MaxMana;
		_experienceBar.MaxValue = 1.0;

		_healthBar.Value = MaxHealth;
		_manaBar.Value = MaxMana;
		_experienceBar.Value = 0;
	}

	public void UpdateHealth(int value) { _healthBar.Value = Mathf.Clamp(value, 0, MaxHealth); }
	public void UpdateMana(float value) { _manaBar.Value = Mathf.Clamp(value, 0, MaxMana); }
	public void UpdateExperience(float progress, int CurrentLevel)
	{
		_experienceBar.Value = Mathf.Clamp(progress, 0, 1);
		if (_levelLabel != null) { _levelLabel.Text = $"level {CurrentLevel}"; } 
	}
}
