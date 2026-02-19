using Godot;

public partial class XPManager : Node
{
	[Signal] public delegate void XPGainedEventHandler(int amount, int newTotal);
	[Signal] public delegate void LevelUpEventHandler(int newLevel);
	
	[Export] public int CurrentXP { get; set; } = 0;
	[Export] public int CurrentLevel { get; set; } = 1;
	[Export] public int XPToNextLevel { get; set; } = 100;
	[Export] public float XpScaling { get; set; } = 1.5f;
	
	public void AddXp(int amount)
	{
		CurrentXP += amount;
		EmitSignal(SignalName.XPGained, amount, CurrentXP);
		GD.Print($"Gained {amount} XP! Total: {CurrentXP} / {XPToNextLevel}");
		
		while (CurrentXP >= XPToNextLevel) { ProcessLevelUp(); }
	}

	public float GetXPProgress() { return (float)CurrentXP / XPToNextLevel; }

	public void Reset()
	{
		CurrentXP = 0;
		CurrentLevel = 1;
		XPToNextLevel = 100 ;
		GD.Print("XP reset");
	}
	
	private void ProcessLevelUp()
	{
		CurrentXP -= XPToNextLevel;
		CurrentLevel++;

		XPToNextLevel = Mathf.RoundToInt(XPToNextLevel * XpScaling);

		EmitSignal(SignalName.LevelUp, CurrentLevel);
		GD.Print($"Level up! Now level: {CurrentLevel}, Next level in: {XPToNextLevel} : XP. ");
	}
}
