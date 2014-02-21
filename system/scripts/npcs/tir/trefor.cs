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
	
	protected override async Task Talk()
	{
		await Intro(
			"Quite a specimen of physical fitness appears before you wearing well-polished armor that fits closely the contours of his body.",
			"A medium-length sword hangs delicately from the scabbard at his waist. While definitely a sight to behold,",
			"it's difficult to see much of his face because of his lowered visor, but one cannot help but notice the flash in his eyes",
			"occasionally catching the light between the slits on his helmet. His tightly pursed lips seem to belie his desire to not shot any emotion."
		);
		
		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop"), Button("Upgrade Item", "@upgrade"), Button("Get Alby Beginner Dungeon Pass", "@pass"));
		
		switch(await Select())
		{
			case "@talk":
				Msg("Hmm? Are you a new traveler?");
				await StartConversation();
				return;
				
			case "@shop":
				Msg("Do you need a Quest Scroll?");
				OpenShop();
				return;
				
			case "@upgrade":
				Msg("Do you want to modify an item?<br/>You don't need to go too far; I'll do it for you. Select an item that you'd like me to modify.<br/>I'm sure you know that the number of times it can be modified, as well as the types of modifications available depend on the item, right?");
				Msg("(Unimplemented)");
				return;

			case "@pass":
				GiveItem(63140);
				Notice("Recieved Alby Beginner Dungeon Pass from Trefor");
				Msg("Do you need an Alby Beginner Dungeon Pass?<br/>No problem. Here you go.<br/>Drop by anytime when you need more.<br/>I'm a generous man, ha ha.");
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
			case "about_skill": Msg("I've been observing your combat style for some time now.<br/>If you want to be a warrior, you shouldn't limit yourself to just melee attacks.<p/>I'm sure Ranald at the School can teach you some things about ranged attacks<br/>which will allow you to attack monsters from a distance."); break;
			default:            Msg("Oh, is that so?"); break;
		}
	}
}
