//--- Aura Script -----------------------------------------------------------
// Eiry
//--- Description -----------------------------------------------------------
// Ego Weapon
//---------------------------------------------------------------------------

public class EiryScript : NpcScript
{
	public override void Load()
	{
		SetName("_ego_eiry");
		SetRace(1);
		SetLocation(22, 5800, 7100, 0);
	}

	protected override async Task Talk()
	{
		while(true)
		{
			Msg("If you have any questions, ASK ME!", Button("Ask for Help", "@askforhelp"), Button("Main Locations", "@tips_point"), Button("Ask a Question", "@tips_general"), Button("Tip for Today", "@tips_generalrandom"), Button("End Conversation", "@endconvo"));
			var reply = await Select();
			
			if(reply == "@endconvo")
				break;
			
			Msg("(Unimplemented)");
		}
		
		Close("Thanks for the help, Eiry!");
	}
}
