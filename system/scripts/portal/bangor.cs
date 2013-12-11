using Aura.Channel.Scripting.Scripts;

public class BangorPortals : BaseScript
{
	public override void Load()
	{
		// Gairech
		SetPropBehavior(45036129417756692, PropWarp(30, 39167, 17906));
		SetPropBehavior(45036125123248153, PropWarp(31, 13083, 23128));
		
		// Barri
		SetPropBehavior(45036129417756782, PropWarp(32, 3210, 2441));
		SetPropBehavior(45036133712723974, PropWarp(31, 13167, 15202));
		
		// Morva Aisle
		SetPropBehavior(45036129417756810, PropWarp(96, 18358, 35028));
		SetPropBehavior(45036408590565392, PropWarp(31, 12810, 5323));
	}
}
