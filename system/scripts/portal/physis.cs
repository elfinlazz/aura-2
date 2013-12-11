using Aura.Channel.Scripting.Scripts;

public class PhysisPortals : BaseScript
{
	public override void Load()
	{
		SetPropBehavior(45074655274401794, PropWarp(3200, 338404, 252538));
		SetPropBehavior(45050599166771232, PropWarp(3200, 405020, 276371));
		SetPropBehavior(45049748759118190, PropWarp(3200, 196431, 161470));
		SetPropBehavior(45036902511869959, PropWarp(3200, 290325, 212149));
		SetPropBehavior(45049740170559492, PropWarp(3202, 54993, 123796));
		SetPropBehavior(45049740173508730, PropWarp(9001, 8001, 5735));
	}
}
