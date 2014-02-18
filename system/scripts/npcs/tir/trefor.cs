//--- Aura Script -----------------------------------------------------------
// Trefor in Tir Chonaill
//--- Description -----------------------------------------------------------
// 
//---------------------------------------------------------------------------

public class TreforBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_trefor");
		SetRace(10002);
		SetBody(height: 1.35f);
		SetFace(skinColor: 20, eyeColor: 27);
		SetStand("human/male/anim/male_natural_stand_npc_trefor02", "human/male/anim/male_natural_stand_npc_trefor_talk");
		SetLocation(1, 8692, 52637, 220);

		EquipItem(Pocket.Face, 4909, 0x93005C);
		EquipItem(Pocket.Hair, 4023, 0xD43F34);
		EquipItem(Pocket.Armor, 14076, 0x1F1F1F, 0x303F36, 0x1F1F1F);
		EquipItem(Pocket.Glove, 16097, 0x303F36, 0x000000, 0x000000);
		EquipItem(Pocket.Shoe, 17282, 0x1F1F1F, 0x1F1F1F);
		EquipItem(Pocket.Head, 18405, 0x191919, 0x293D52);
		EquipItem(Pocket.LeftHand2, 40005, 0xB6B6C2, 0x404332, 0x22B653);
		
		AddPhrase("(Fart)...");
		AddPhrase("(Spits out a loogie)");
		AddPhrase("Ah-choo!");
		AddPhrase("Ahem");
		AddPhrase("Burp.");
		AddPhrase("Cough cough...");
		AddPhrase("I heard people can go bald when they wear a helmet for too long...");
		AddPhrase("I need to get married...");
		AddPhrase("It's been a while since I took a shower");
		AddPhrase("Seems like I caught a cold...");
		AddPhrase("Soo itchy... and I can't scratch it!");
		AddPhrase("This helmet's really making me sweat");
	}
	
	public override IEnumerable Talk()
	{
		Intro(
			"Quite a specimen of physical fitness appears before you wearing well-polished armor that fits closely the contours of his body.",
			"A medium-length sword hangs delicately from the scabbard at his waist. While definitely a sight to behold,",
			"it's difficult to see much of his face because of his lowered visor, but one cannot help but notice the flash in his eyes",
			"occasionally catching the light between the slits on his helmet. His tightly pursed lips seem to belie his desire to not shot any emotion."
		);
		
		Call(Hook("after_intro"));
		
		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop"), Button("Upgrade Item", "@upgrade"), Button("Get Alby Beginner Dungeon Pass", "@pass"));
		var selected = Select();
		
		switch(selected)
		{
			case "@talk":
				Msg("Hmm? Are you a new traveler?");
				
				while(true)
				{
					Msg(Hide.Name, "(Trefor is waiting for me to say something.)");
					ShowKeywords();
					var keyword = Select();
					
					Call(Hook("before_keywords", keyword));
					
					switch (keyword)
					{
						//case "personal_info": Msg("I'm the chief of this town..."); break;
						//case "rumor":         Msg("I heard a rumor that this is just a copy of the world of Erin. Trippy, huh?"); break;
						//case "about_skill":   Msg("I don't know of any skills... Why don't you ask Malcom?"); break;
						//case "about_arbeit":  Msg("I don't have any jobs for you, but you can get a part time job in town."); break;
						//case "about_study":   Msg("You can study different magic down at the school!"); break;
						default:              Msg("Oh, is that so?"); break;
					}
				}
				
			case "@shop":
				Msg("Do you need a Quest Scroll?");
				OpenShop();
				Return();
				
			case "@upgrade":
				Msg("Do you want to modify an item?<br/>You don't need to go too far; I'll do it for you. Select an item that you'd like me to modify.<br/>I'm sure you know that the number of times it can be modified, as well as the types of modifications available depend on the item, right?");
				Msg("(Unimplemented)");
				Return();

			case "@pass":
				GiveItem(63140);
				Notice("Recieved Alby Beginner Dungeon Pass from Trefor");
				Msg("Do you need an Alby Beginner Dungeon Pass?<br/>No problem. Here you go.<br/>Drop by anytime when you need more.<br/>I'm a generous man, ha ha.");
				Return();
			
			default:
				Msg("...");
				Return();
		}
	}
}
