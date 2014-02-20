//--- Aura Script -----------------------------------------------------------
// Tin
//--- Description -----------------------------------------------------------
// There are 2 Tins, the counter part to Nao for pets in Soul Stream
// and the on in the Tir beginner area. Both are needed because the client
// sends LeaveSoulStream when ending the conversation with a special NPC.
//---------------------------------------------------------------------------

public class TinScript : NpcScript
{
	public override void Load()
	{
		SetName("_tin");
		SetRace(10002);
		SetBody(height: 0.1f);
		SetFace(skinColor: 15, eyeType: 15, eyeColor: 47);
		SetStand("human/male/anim/male_stand_Tarlach_anguish");
		SetLocation(125, 22211, 74946, 44);

		EquipItem(Pocket.Face, 4900);
		EquipItem(Pocket.Hair, 4021, 0xA64742);
		EquipItem(Pocket.Armor, 15069, 0xFFF7E1, 0xBC3412, 0x40460F);
		EquipItem(Pocket.Shoe, 17010, 0xAC6122, 0xFFFFFF, 0xFFFFFF);
		EquipItem(Pocket.Head, 18518, 0xA58E74, 0xFFFFFF, 0xFFFFFF);
	}

	protected override async Task Talk()
	{
		Msg("Hey, who are you?");
		Msg("You don't look like you're from this world. Am I right?<br/>Did you make your way down here from Soul Stream?<br/>Ahhh, so Nao sent you here!");
		Msg("She's way too obedient to the Goddess' wishes.<br/>Anyway, she's a good girl, so be nice to her.");
		//Msg("And this... is just for you.");
		//Msg("The weapon I gave you is a Spirit Weapon.<br/>If you hold it in your hand, you can talk to the spirit in the sword.<br/>I'm lending it to make your stay here much easier,<br/>so use it wisely.");
		//Msg("You're wondering how to give it back to me, aren't you?<br/>Don't worry. When the right time comes, it will leave you of its own accord.");
		//Msg("Oh my! I almost forgot.<br/>I just gave you the Spirit Weapon and almost forgot to introduce you to the spirit. <p/>The spirit's name is Eiry.<br/> If you want to talk to her, simply click on the wing-shaped button on the bottom right side of your screen.");
	}
}

public class TinSoulStreamScript : NpcScript
{
	public override void Load()
	{
		SetId(MabiId.Tin);
		SetName("_tin_soul_stream");
		SetRace(10002);
		SetLocation(22, 6313, 5712);
	}

	protected override async Task Talk()
	{
		Msg("Hi. <username/>.<br/>Nice to meet you..");
		Msg("I wish you the best of luck with all that you do here in Erinn...<br/>See ya.", Button("End Conversation"));
		await Select();
		
		Player.SetLocation(1, 15250, 38467);
		
		Close();
	}
}
