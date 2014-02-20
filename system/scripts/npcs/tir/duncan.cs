//--- Aura Script -----------------------------------------------------------
// Duncan in Tir Chonaill
//--- Description -----------------------------------------------------------
// Good ol' Duncan
//---------------------------------------------------------------------------

public class DuncanBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_duncan");
		SetRace(10002);
		SetBody(height: 1.3f);
		SetFace(skinColor: 20, eyeType: 17);
		SetStand("human/male/anim/male_natural_stand_npc_duncan_new", "male_natural_stand_npc_Duncan_talk");
		SetLocation(1, 15409, 38310, 122);

		EquipItem(Pocket.Face, 4950, 0x93005C);
		EquipItem(Pocket.Hair, 4083, 0xBAAD9A);
		EquipItem(Pocket.Armor, 15004, 0x5E3E48, 0xD4975C, 0x3D3645);
		EquipItem(Pocket.Shoe, 17021, 0xCBBBAD);

		AddPhrase("Ah, that bird in the tree is still sleeping.");
		AddPhrase("Ah, who knows how many days are left in these old bones?");
		AddPhrase("Everything appears to be fine, but something feels off.");
		AddPhrase("Hmm....");
		AddPhrase("It's quite warm today.");
		AddPhrase("Sometimes, my memories sneak up on me and steal my breath away.");
		AddPhrase("That tree has been there for quite a long time, now that I think about it.");
		AddPhrase("The graveyard has been left unattended far too long.");
		AddPhrase("Watch your language.");
	}
	
	protected override async Task Talk()
	{
		SetBgm("NPC_Duncan.mp3");
		
		Intro(
			"An elderly man gazes softly at the world around him with a calm air of confidence.",
			"Although his face appears weather-beaten, and his hair and beard are gray, his large beaming eyes make him look youthful somehow.",
			"As he speaks, his voice resonates with a kind of gentle authority."
		);
		
		Msg("Please let me know if you need anything.", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Retrive Lost Items", "@lostandfound"));
		
		switch(await Select())
		{
			case "@talk":
				Msg("What did you say your name was?<br/>Anyway, welcome.");
				await StartConversation();
				return;
				
			case "@shop":
				Msg("Choose a quest you would like to do.");
				OpenShop();
				return;
				
			case "@lostandfound":
				Msg("If you are knocked unconcious in a dungeon or field, any item you've dropped will be lost unless you get resurrected right at the spot.<br/>Lost items can usually be recovered from a Town Office or a Lost-and-Found.");
				Msg("Unfortunatly, Tir Chonaill does not have a Town Office, so I run the Lost-and-Found myself.<br/>The lost items are recovered with magic,<br/>so unless you've dropped them on purpose, you can recover those items with their blessings intact.<br/>You will, however, need to pay a fee.");
				Msg("As you can see, I have limited space in my home. So I can only keep 20 items for you.<br/>If there are more than 20 lost items, I'll have to throw out the oldest items to make room.<br/>I strongly suggest you retrieve any lost items you don't want to lose as soon as possible.");
				return;
			
			default:
				Msg("...");
				return;
		}
	}
	
	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info": Msg("I'm the chief of this town..."); break;
			case "rumor":         Msg("I heard a rumor that this is just a copy of the world of Erin. Trippy, huh?"); break;
			case "about_skill":   Msg("I don't know of any skills... Why don't you ask Malcom?"); break;
			case "about_arbeit":  Msg("I don't have any jobs for you, but you can get a part time job in town."); break;
			case "about_study":   Msg("You can study different magic down at the school!"); break;
			default:              Msg("I don't know anything about that..."); break;
		}
	}
}
