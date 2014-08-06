// --- Aura Script ----------------------------------------------------------
//  Jail
// --- Description ----------------------------------------------------------
//  Puts all players who commit a security violation in "Jail".
// --- By -------------------------------------------------------------------
//  Xcelled
// --------------------------------------------------------------------------

public class JailScript : NpcScript
{
	private static readonly TimeSpan _time = new TimeSpan(0, 30, 0);

	private static string FormatTS(TimeSpan ts)
	{
		var r = new List<string>();

		if (ts.Days != 0)
			r.Add(string.Format("{0} days", ts.Days));
		if (ts.Hours != 0)
			r.Add(string.Format("{0} hours", ts.Hours));
		if (ts.Minutes != 0)
			r.Add(string.Format("{0} minutes", ts.Minutes));
		if (ts.Seconds != 0)
			r.Add(string.Format("{0} seconds", ts.Seconds));

		return string.Join(", ", r);
	}

	[On("SecurityViolation")]
	public void NewPlayer(SecurityViolationEventArgs e)
	{
		if (e.Client.Controlling == null)
			return;

		e.Client.Controlling.Warp(126, 4400, 4200);
		e.Client.Controlling.Vars.Perm["jail_free_time"] = DateTime.Now + _time;
	}

	public override void Load()
	{
		SetName("Warden");
		SetRace(10002);
		SetBody(height: 1.2f);
		SetFace(skinColor: 15, eyeType: 3, eyeColor: 3);

		EquipItem(Pocket.Face, 4908, 16);
		EquipItem(Pocket.Hair, 4089);
		EquipItem(Pocket.Head, 18824, 0xCD, 0x0, 0xFFD700);
		EquipItem(Pocket.Shoe, 17364, 0x0, 0xFFD700);
		EquipItem(Pocket.Armor, 15774, 0xCD, 0xFFD700, 0x0);
		EquipItem(Pocket.RightHand2, 40810, 0x0, 0xC0C0C0, 0xFFD700);

		SetLocation(126, 4408, 1573, 58);

		AddPhrase("Back again, eh?");
		AddPhrase("Hey! Stop that!");
		AddPhrase("What, you want to leave?");
		AddPhrase("You have the right to remain silent...");
	}

	protected override async Task Talk()
	{
		SetBgm("Dungeon_17.mp3");

		while (true)
		{
			Msg("<username/>...<br/>Our newest \"inmate\"...<br/>Do you need something?",
				Button("End Conversation", "@end"),
				Button("Why am I here?", "@why"),
				Button("Can I leave?", "@leave"));

			switch (await Select())
			{
				case "@why":
					Msg("You've been imprisoned here because you were<br/>caught trying to do something you shouldn't have.");
					Msg("That's a big no no, <username/>...<br/>Do you know what happens to repeat offenders?");
					Msg(Hide.Both, "(<npcname/> draws his finger across his neck menacingly)");
					Msg("You'll be released after spending " + FormatTS(_time) + "<br/>of quality time here with me so you can think<br/>about what you've done.");
					Msg("Oh, one more thing, <username/>...<br/>See that you don't return.<br/>Subsequent visits can be... unpleaseant.");
					break;

				case "@leave":
					Msg("You think your time is up, <username/>?<br/>Well, let's see...");

					var end = Player.Vars.Perm["jail_free_time"];
					if (end == null || end < DateTime.Now)
					{
						Msg("Congratulations. Your time has been served.<br/>You are free to go.", Button("Get me out of here", "@go"), Button("I want to stay a bit", "@stay"));

						switch (await Select())
						{
							case "@go":
								await Warp();
								return;

							default:
								Msg("Going to hang around a bit?<br/>No problem, just talk to me when you're ready to leave.");
								break;
						}
					}
					else
					{
						Msg("I'm sorry, but you need to wait an additional<br/>" + FormatTS(end - DateTime.Now) + " before you can leave.");
					}
					break;
			}
		}
	}

	private async Task Warp()
	{
		Msg("Remember, <username/>...<br/>I'm watching...", Button("Got it"));

		await Select();

		Player.Warp(1, 12801, 38397);

		Close();
	}
}
