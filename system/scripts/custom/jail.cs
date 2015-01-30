// --- Aura Script ----------------------------------------------------------
//  Jail
// --- Description ----------------------------------------------------------
//  Puts all players who commit a security violation in "Jail" and adds
//  the "jail" command (default auth 50:-1), that can be used to jail
//  characters freely.
// --- By -------------------------------------------------------------------
//  Xcelled, exec
// --------------------------------------------------------------------------

public class JailScript : NpcScript
{
	public override void Load()
	{
		SetName("Warden");
		SetRace(8002);
		SetBody(height: 1.2f, upper: 1.1f, weight: 1.1f);
		SetFace(skinColor: 27, eyeType: 55, eyeColor: 76, mouthType: 26);

		EquipItem(Pocket.Face, 8900, 27);
		EquipItem(Pocket.Hair, 8001, 0x1000001C);
		EquipItem(Pocket.Head, 18824, 0x000000, 0x000000, 0x000000);
		EquipItem(Pocket.Shoe, 17364, 0x000000, 0x000000, 0x000000);
		EquipItem(Pocket.Armor, 15774, 0x000000, 0x000000, 0xFFFFFF);
		EquipItem(Pocket.RightHand2, 40810, 0x000000, 0xC0C0C0, 0xFFD700);
		
		SetLocation(126, 4408, 1573, 58);

		AddPhrase("Back again, eh?");
		AddPhrase("Hey! Stop that!");
		AddPhrase("What, you want to leave?");
		AddPhrase("You have the right to remain silent...");
		
		AddCommand(50, -1, "jail", "<name> <duration>", JailCommand);
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
					Msg("You'll be released after spending some quality time here with me,<br/>so you can think about what you've done.");
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

	[On("SecurityViolation")]
	public void NewPlayer(SecurityViolationEventArgs e)
	{
		if (e.Client.Controlling == null)
			return;

		Jail(e.Client.Controlling, TimeSpan.FromMinutes(30));
	}
	
	public static void Jail(Creature creature, TimeSpan time)
	{
		creature.Warp(126, 4400, 4200);
		creature.Vars.Perm["jail_free_time"] = DateTime.Now + time;
	}
	
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

	public CommandResult JailCommand(ChannelClient client, Creature sender, Creature target, string message, IList<string> args)
	{
		if (args.Count < 3)
			return CommandResult.InvalidArgument;

		var name = args[1];

		TimeSpan duration;
		if(args[2] == "life")
		{
			duration = TimeSpan.FromDays(7 * 20); // 7 days * 20 = 20 Erinn years
		}
		else if (!TimeSpan.TryParse(args[2], out duration))
		{
			Send.ServerMessage(sender, Localization.Get("Invalid duration format Examples: 2 (2 days), 2:00 (2 hours), 0:05 (5 minutes)"), name);
			return CommandResult.Okay;
		}

		var creature = ChannelServer.Instance.World.GetPlayer(name);
		if (creature == null)
		{
			Send.ServerMessage(sender, Localization.Get("Character '{0}' not found."), name);
			return CommandResult.Okay;
		}

		JailScript.Jail(creature, duration);

		Send.ServerMessage(sender, Localization.Get("Jailed '{0}' for {1}."), name, FormatTS(duration));
		Send.ServerMessage(creature, Localization.Get("You've been jailed for {0} by {1}."), FormatTS(duration), sender.Name);

		return CommandResult.Okay;
	}
}
