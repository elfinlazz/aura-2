//--- Aura Script -----------------------------------------------------------
// Nora, the inn helper in Tir Chonaill
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class NoraBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_nora");
		SetRace(10001);
		SetBody(height: 0.85f);
		SetFace(skinColor: 17);
		SetStand("human/female/anim/female_natural_stand_npc_nora02");
		SetLocation(1, 15933, 33363, 186);

		EquipItem(Pocket.Face, 3900, 0xDED7EA, 0xA2C034, 0x004A18);
		EquipItem(Pocket.Hair, 3025, 0xD39A81, 0xD39A81, 0xD39A81);
		EquipItem(Pocket.Armor, 15010, 0x34696E, 0xFDEEEA, 0xC6D8EA);
		EquipItem(Pocket.Shoe, 17006, 0x34696E, 0x9C558F, 0x901D55);
		
		AddPhrase("I hope the clothes dry quickly.");
		AddPhrase("I would love to listen to some music, but I don't see any musicians around.");
		AddPhrase("No way! There's no such thing as a huge spider.");
		AddPhrase("Oh no! Rats!");
		AddPhrase("Perhaps I should consider taking a day off.");
		AddPhrase("Please wait.");
		AddPhrase("Wait a second.");
		AddPhrase("Wow! Look at that owl! Beautiful!");
	}
	
	public override IEnumerable Talk()
	{
		SetBgm("NPC_Nora.mp3");
	
		Intro(
			"A girl wearing a well-ironed green apron leans forward, gazing cheerfully at her sorroundings.",
			"Her bright eyes are azure blue and a faint smile plays on her lips.",
			"Cross-shaped earrings dangle from her ears, dancing playfully between her honey-blonde hair.",
			"Her hands are always busy, as she engages in some chore or another, though she often looks into the distance as if deep in thought."
		);
		
		Call(Hook("after_intro"));
		
		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));
		var selected = Select();
		
		switch(selected)
		{
			case "@talk":
				Msg("Welcome!");
				
				while(true)
				{
					Msg(Hide.Name, "(Nora is looking in my direction.)");
					ShowKeywords();
					var keyword = Select();
					
					Call(Hook("before_keywords", keyword));
					
					switch (keyword)
					{
						default: Msg("Can we change the subject?"); break;
					}
				}
				
			case "@shop":
				Msg("Are you looking for a Tailoring Kit and materials?<br/>If so, you've come to the right place.");
				OpenShop();
				Return();
				
			case "@repair":
				Msg("Do you want to repair your clothes?<br/>Well I can't say I'm perfect at it,<br/>but I'll do my best.<br/>Just in case, when in doubt, you can always go to a professional tailor.");
				Msg("(Unimplemented)");
				Return();

			default:
				Msg("...");
				Return();
		}
	}
}
