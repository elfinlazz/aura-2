//--- Aura Script -----------------------------------------------------------
// Ranald, Combat Instructor
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class NoraBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_ranald");
		SetRace(10002);
		SetBody(upper: 1.1f);
		SetFace(skinColor: 20);
		SetStand("human/male/anim/male_natural_stand_npc_ranald02", "human/male/anim/male_natural_stand_npc_ranald_talk");
		SetLocation(1, 4651, 32166, 195);

		EquipItem(Pocket.Face, 4900, 0xF88B4A);
		EquipItem(Pocket.Hair, 4154, 0x4D4B53);
		EquipItem(Pocket.Armor, 15652, 0xAC9271, 0x4D4F48, 0x7C6144);
		EquipItem(Pocket.Shoe, 17012, 0x9C7D6C, 0xFFC9A3, 0xF7941D);
		EquipItem(Pocket.LeftHand1, 40012, 0xDCDCDC, 0xC08B48, 0x808080);
		
		AddPhrase("I need a drink...");
		AddPhrase("I guess I drank too much last night...");
		AddPhrase("I need a nap...");
		AddPhrase("I should drink in moderation...");
		AddPhrase("I should sharpen my blade later.");
		AddPhrase("It's really dusty here.");
		AddPhrase("What's with the hair styles of today's kids?");
	}
	
	protected override async Task Talk()
	{
		SetBgm("NPC_Ranald.mp3");
	
		await Intro(
			"From his appearance and posture, there is no doubt that he is well into middle age, but he is surprisingly well-built and in good shape.",
			"Long fringes of hair cover half of his forehead and right cheek. A strong nose bridge stands high between his shining hawkish eyes.",
			"His deep, low voice has the power to command other people's attention."
		);
		
		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"), Button("Get Ciar Beginner Dungeon Pass", "@ciarpass"));
		
		switch(await Select())
		{
			case "@talk":
				Msg("My name is Ranald."); // "<br/>I assume you remember my name?"
				await StartConversation();
				return;
				
			case "@shop":
				Msg("Tell me if you need a Quest Scroll.<br/>Working on these quests can also be a good way to train yourself.");
				Msg("(Unimplemented)");
				return;
				
			case "@upgrade":
				Msg("Hmm... You want me to modify your item? You got some nerve!<br/>Ha ha. Just joking. Do you need to modify an item? Count on Ranald.<br/>Pick an item to modify.<br/>Oh, before that. Types or numbers of modifications are different depending on what item you want to modify. Always remember that.");
				Msg("(Unimplemented)");
				return;
				
			case "@ciarpass":
				GiveItem(63139); // Ciar Beginner Dungeon Pass
				Notice("Recieved Ciar Beginner Dungeon Pass from Ranald.");
				Msg("Ok, here's the pass.<br/>You can ask for it again if you need it.<br/>That doesn't mean you can fill up the iventory with a pile of passes.");
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
			default:
				RndMsg(
					"Can we change the subject?"
				);
				break;
		}
	}
}
