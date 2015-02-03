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
			
			if(reply == "@askforhelp")
			{
				Msg("Would you like to request help from a veteran adventurer?<br/>I happen to know quite a few people in Erinn.<br/>I can ask them if they're free to help you.");
				Msg("Now, keep in mind, if people are too busy or not around<br/>your request might not be accepted by anyone.<br/>Don't be too disappointed if that happens!<br/>Also, you can only request help once per day.", Button("Ask for Help", "@yes"), Button("Later", "@no"));
				
				if(await Select() == "@yes")
					Msg("Oh! Someone responded to your help request! Now, just wait...<br/>Though that person might change their mind if they're busy...");
				else
					Msg("All right. Whenever you need help,<br/>please send out a help request!");
			}
			else if(reply == "@tips_point")
			{
				switch(Player.RegionId)
				{
					case 1: // Tir
						Msg("This is Tir Chonaill, the town where everything started.<br/>It is like a hometown for everyone, so try to have lots of conversations with the residents.");
						break;
						
					case 2: // Tir Bank
						Msg("This is the place where you can store your money and/or items for a small fee.<br/>You can even request a Personal Shop License from the bank teller.<br/>But it requires a Personal Shop Bag,<br/>so think about it after you make a lot of money.");
						break;
						
					case 4: // Tir Church
						Msg("This is the Church that worships Lymilark.<br/>The Church not only provides people spiritual comfort,<br/>but also provides adventurers with the Holy Water of Lymilark.<br/>If you want to know more about the Holy Water of Lymilark, ask Endelyon for part-time Jobs.");
						break;
						
					case 5: // Tir Grocery Store
						Msg("This store sells food and ingredients for cooking.<br/>It's tough to concentrate when you are hungry, right?<br/>Oh, this place also sells simple Quest Scrolls,<br/>so if you have some time, try purchasing one.");
						break;
						
					case 8: // Tir General Shop
						Msg("This is the General Shop which sells various goods including clothes, hats, shoes and musical instruments.<br/>This is where you can learn the Weaving skill,<br/>so talk to Malcolm about it.");
						break;
						
					case 9: // Tir School
						Msg("Lassar works here in magic classroom at the School.<br/>Once you pay tuition, you'll be able to learn magic,<br/>but I recommend learning magic at your own pace.<br/>You'll understand when you find out how much it costs to learn magic.");
						break;
					
					case 13: // Alby
						Msg("This is the lobby of Alby Dungeon!<br/>To enter the dungeon, open your Inventory, choose any item to drop,<br/>and use your mouse to drag and drop it on the dungeon altar.");
						Msg("You seem primed to enter Alby Beginner Dungeon.<br/>The dungeon is not too difficult, but please enter with caution.");
						break;
					
					case 14: // Dunbarton
						Msg("This is Dunbarton, the commercial center of the northern continent.<br/>Um... And...<br/>That's pretty much what I heard from other people.<br/>I don't know anything else, so let's just go back to Tir Chonaill!");
						break;
					
					case 16: // Dugald
						Msg("This is Dugald Aisle, which connects Tir Chonaill and Dunbarton.<br/>Follow along the path down south,  and you will reach Dunbarton.");
						Msg(Image("map_dugald_aisle_N", true, 270, 274), "But if you stray from the path, even just a little bit, you will likely meet some dangerous animals such as bears and wolves.<br/>Please be careful when traveling, and...<br/>unless you have something to do there, let's return to Tir Chonaill.");
						break;
					
					case 52: // Emain
						Msg("Wow, what a big city!<br/>This city is called Emain Macha.<br/>Master, isn't it dangerous for you to go this far?<br/>It might be better for you to return to Tir Chonaill.");
						break;
					
					case 70: // Osna Snail
						Msg("This place is called Osna Sail, and it's a narrow path connecting Dunbarton and Emain Macha.<br/>There are many wild animals living here, ready to attack any minute.<br/>Master, you are going back to Tir Chonaill, right? Right?");
						break;
					
					default:
						RndMsg(
							"Well, master, I don't know where we are either.<br/>It would be better for you not to go to places beyond my knowledge.",
							"Um, where are we?<br/>I have no idea where we are.<br/>Don't go to places that I don't know."
						);
						break;
				}
			}
			else if(reply == "@tips_general")
			{
				while(true)
				{
					Msg("What would you like to know more of?<br/>Choose a category<br/>and click it.",
						List("Game Tips", 5, "@breakloop",
							Button("How do I fight?", "@goodfighting"),
							Button("What are hunger and camping penalties?", "@parameters"),
							Button("What is a Quest?", "@quests"),
							Button("Interacting with other players.", "@communication"),
							Button("Advice from Eiry.", "@general")
						)
					);
					
					var category = await Select();
					
					if(category == "@breakloop" || category == "@end")
						break;
					
					if(category == "@goodfighting")
					{
						while(true)
						{
							Msg("Ask me some questions... Questions on combat and stuff! Which of these questions do you want to ask?<br/>Click on a topic.",
								List("How do I fight?", 7, "@breakloop",
									Button("The basics of Combat.", "@howtocombat"),
									Button("My character attacks on its own.", "@autoattack"),
									Button("How can I control my attacks?", "@manualattack"),
									Button("What happens when I become unconscious?", "@dead"),
									Button("How can I help those who are unconscious?", "@revive"),
									Button("What is an expedition bonus?", "@wonjeonbonus"),
									Button("What is camping penalty?", "@campingpenalty")
								)
							);
						
							category = await Select();
							
							if(category == "@breakloop" || category == "@end")
								break;
							
							if(category == "@howtocombat")
							{
								Msg("The first thing in combat is<br/>choosing a stronger weapon than your enemy.<br/>You won't have any problem facing monster<br/>with me on your side.");
								Msg("Press the Inventory button at the bottom or press <hotkey name='InventoryView'/>.<br/>Look at the weapon slot in the left equipment window<br/>and check if you are holding me.", Image("npctalk_inventory", true, 312, 282));
								Msg("Your hands are empty even when you are holding me?<br/>Then, try switching between<br/>primary and secondary equipment. You can do that by<br/>pressing Tab like the picture.", Image("npctalk_equipselect", true, 390, 203));
								Msg("For your convenience, add frequently used skills<br/>to the hotkey window. If you haven't yet added<br/>the Defense skill to the window,<br/>follow the picture I show you.", Image("npctalk_skillicon", true, 694, 180));
								Msg("If you placed the Defense skill on F12,<br/>you can use it<br/>by hitting F12 on your keyboard<br/>or clicking on the Defense skill icon in the hotkey window.", Image("npctalk_skillicon", true, 694, 180));
								Msg("But!<br/>The skill will not be activated immediately<br/>after clicking its icon or pressing its designated hotkey.<br/>In Erinn, all skills have preparation time before being activated.");
								Msg("When you use a skill,<br/>the skill icon appears above your head as in the left picture.<br/>The icon keeps changing its size smaller and bigger.<br/>That means the skill is in its preparation stage!", Image("npctalk_skillprocessing", true));
								Msg("After the skill's unique length of preparation time has passed,<br/>the size of the balloon stops changing as in the right picture,<br/>meaning that it's activated and ready to be used.", Image("npctalk_skillprocessing", true));
								Msg("In the case of the Defense skill,<br/>you can only walk from this period<br/>but you will defend yourself from an attack, taking minimum amount of damage.<br/>In the case of the Smash skill, you will give high damage and knock your opponent down in a single blow.", Image("npctalk_skillprocessing", true));
								Msg("This applies to all skills.<br/>In Erinn, knowing your opponent's next move<br/>and preparing your own against it is critical in combat.<br/>I hope you'll get used to it soon.", Image("npctalk_skillprocessing", true));
								Msg("You can find out whether<br/>a monster is going to attack you<br/>or it is on its guard by exclamation marks above its name.<br/>It is shown as in this picture.", Image("npctalk_hate_battlemode", true, 306, 200));
								Msg("If 1 red exclamation mark appears<br/>and the monster changes its stance,<br/>it means that it has noticed you<br/>and it is keeping an eye on you.", Image("npctalk_hate_battlemode", true, 306, 200));
								Msg("If 2 red exclamation marks inside a spiky balloon appear,<br/>it is about to attack you,<br/>taking you as a foe.", Image("npctalk_hate_battlemode", true, 306, 200));
								Msg("Try to have less monster attacking you at once<br/>when facing a bunch of monster.<br/>That can be done by attacking them one at a time.", Image("npctalk_hate_battlemode", true, 306, 200));
								Msg("Also, check your Stamina<br/>after taking down 1 enemy.<br/>You won't be able to attack properly without sufficient Stamina.", Image("npctalk_foodgauge_1"));
								Msg("Press Ctrl to<br/>automatically aim approaching enemy.<br/>You won't have to hover the mouse cursor over your target<br/>if you keep pressing it.", Image("npctalk_targeting", true, 256, 256));
							}
							else if(category == "@autoattack")
							{
								Msg("In Auto Combat mode, you don't need to click multiple times to attack.<br/>Everyone fights in this mode at first.");
								Msg("But later on, it may be better to use Manual mode once you get used to the combat,<br/>since you can utilize your skills at appropriate moments.<br/>You can toggle between the two modes by pressing either <hotkey name='AutoCombat'/><br/>or the shield-shaped icon on the lower right corner of the screen.");
							}
							else if(category == "@manualattack")
							{
								Msg("All you need to do is just change into Manual Combat mode.<br/>In this mode, you can attack by clicking on the target.<br/>You can also develop your own combat tactics by switching up your timing.<br/>Use this mode once you get used to the general flow of combat.");
							}
							else if(category == "@dead")
							{
								Msg("Travelers who came through Soul Stream like you will never die in this world, even when facing grave danger.<br/>Instead, they will lose consciousness when suffering great injuries.<br/>In this case, they can rise again with the aid of other people.");
								Msg("If there is no one nearby to ask for help, select 'Revive in Town', or ask Nao for help.<br/>EXP losses vary in each case, so select an option that best suits your situation.");
							}
							else if(category == "@revive")
							{
								Msg("You mean those lying down with a feather over their heads, right?<br/>In that case, you will need an item called the Phoenix Feather.<br/>You can purchase it at the Healer's House, so look for one the next time you go there.");
								Msg("By the way, once you revive someone that's unconscious, you'll earn some EXP from that person!<br/>This is one of those instances where both you and the revived person benift,<br/>so don't think of Phoenix Feathers as meaningless items and always keep some in your Inventory.");
							}
							else if(category == "@wonjeonbonus")
							{
								Msg("Sometimes, you will earn some bonus EXP if the phrase 'Expedition Bonus' is displayed after defeating a monster.<br/>The farther you are from town, the more bonus EXP you earn.<br/>Therefore, try hunting at places far away if you can handle it.");
							}
							else if(category == "@campingpenalty")
							{
								Msg("Sometimes, you will earn less EXP if the phrase 'Camping Penalty' is displayed after defeating a monster.<br/>This penalty is applied when you train at one place for too long.");
								Msg("The penalty won't be applied if you hunt in different places.<br/>That's why you should hunt in a wide radius.");
								Msg("If there is a camp pitched by someone, simply go inside for a short time and come back out.<br/>Just move in and move out, and ta-daa! Camping penalty is gone!");
							}
							else if(category == "@campingpenalty")
							{
								Msg("Sometimes, you will earn less EXP if the phrase 'Camping Penalty' is displayed after defeating a monster.<br/>This penalty is applied when you train at one place for too long.");
								Msg("The penalty won't be applied if you hunt in different places.<br/>That's why you should hunt in a wide radius.");
								Msg("If there is a camp pitched by someone, simply go inside for a short time and come back out.<br/>Just move in and move out, and ta-daa! Camping penalty is gone!");
							}
						}
					}
					else if(category == "@parameters")
					{
						while(true)
						{
							Msg("What are Attributes? Select from the list above!",
								List("Regarding character status", 5, "@breakloop",
									Button("What is Stamina?", "@stamina"),
									Button("What is Hunger?", "@foodgauge"),
									Button("What is EXP?", "@exppopint"),
									Button("What is Defense?", "@defense"),
									Button("What is Protection?", "@protect")
								)
							);
						
							category = await Select();
							
							if(category == "@breakloop" || category == "@end")
								break;
							
							if(category == "@stamina")
							{
								Msg("The yellow bar on the bottom part of the screen indicates how much Stamina you have left.<br/>Stamina is your energy. It decreases when you perform an action, and decreases even more when you use a skill.<br/>Be careful not to run out of Stamina when you're fighting.");
							}
							else if(category == "@foodgauge")
							{
								Msg("Are hunger and Stamina related to each other?<br/>Yes. Stamina decreases gradually when you're hungry.<br/>The dark part in the yellow Stamina bar indicates how hungry you are.");
								Msg("Stamina regeneration rate decreases when you're feeling hungry.<br/>It may not seem like a big deal, but Stamina is very important in combat!<br/>You can quickly regain it with food, so remember to eat before a combat arises.");
							}
							else if(category == "@exppopint")
							{
								Msg("You can gain EXP in Erinn through various activities, and once it reaches a certain level, you will level up.<br/>When that happens, your attributes will increase and you will earn 1 AP, which is used to learn skills.");
								Msg("EXP is generally earned by defeating monsters, but there are other ways of earning EXP.<br/>You can also gain EXP by completing part-time jobs and quests, both of which you can get through town residents.");
								Msg("If you are not used to being in a dangerous environment, or you wish to earn EXP through other ways than fighting,<br/>it would be better to seek ways of gaining EXP in town.");
							}
							else if(category == "@defense")
							{
								Msg("Most of the clothes and armors of Erinn have similar Defense Rates, unless the items are modified.<br/>If you want to increase your own Defense Rate, you should learn skills that will help you do so.");
								Msg("You can learn defense-enhancing skills at School.<br/>If you haven't gone yet, I strongly suggest you do.");
							}
							else if(category == "@protect")
							{
								Msg("With high Protection Rate, you will be hit by fewer critical attacks.<br/>On the other hand, with low Protection Rate, chances of being hit by critical attacks will increase.");
								Msg("The damage can be significant even from nondescript weapons if you're hit with a critical attack.<br/>In order to fight against strong monsters, it would be a good idea for you to increase your Protection Rate.");
							}
						}
					}
					else if(category == "@quests")
					{
						while(true)
						{
							Msg("Yes, quests are very important.<br/>Make it a habit to press <hotkey name='QuestView'/> and check your current quests!<br/>Please select a question, master.",
								List("What is a Quest?", 4, "@breakloop",
									Button("What is a Quest Marker?", "@questmarker"),
									Button("Do I have to follow the quest order?", "@questplaysequence"),
									Button("What does Give up Quest mean?", "@quitquest"),
									Button("What are Stoyline Quests?", "@mainstream")
								)
							);
						
							category = await Select();
							
							if(category == "@breakloop" || category == "@end")
								break;
							
							if(category == "@questmarker")
							{
								Msg("See the flashing diamond-shaped symbols on the Minimap? Those are Quest Markers.<br/>It is a convenient marker that shows where the person or the location related to the quest is.");
								Msg("You will see a pillar of light shining on the road guiding your direction.<br/>All you have to do is just walk toward the light.");
							}
							else if(category == "@questplaysequence")
							{
								Msg("Not at all! You don't have to.<br/>Oh, if you see a phrase like 'Bring something first' or the next step is hidden,<br/>then of course you'll have to follow the order.<br/>Except for such cases, the quest order is usually not that important.");
								Msg("You can just put some of the quests on the side until you pass by its location<br/>unless it has a time limit and needs to be done quickly.");
								Msg("The quests I alert you with are just guidelines you can follow<br/>in case you are unsure of what to do next.");
							}
							else if(category == "@quitquest")
							{
								Msg("If you have quests that you don't need or don't want to finish,<br/>you can simply delete them by pressing 'Give Up' from the Quest window.<br/>However, some quests cannot be repeated once more, so take caution when forfeiting any quests.");
								Msg("Also, some quests cannot be forfeited, including those from me.<br/>That means they are important!");
								Msg("Quests that have a time limit, such as Party Quests,<br/>become invalid when the time limit expires.<br/>For those quests that are deemed invalid, just delete them and keep your Quest window neat and tidy.");
							}
							else if(category == "@mainstream")
							{
								Msg("One of these days, you will receive a quest called 'Main Scenario'.<br/>It is said that there is a dark and hidden side to this peaceful and pleasant world of Erinn, and<br/>you will be taking on challenging tasks related to this story...");
								Msg("The person that gives you the quest will not mention Main Scenario,<br/>but you will soon realize that the quest you are executing is a Main Scenario quest.<br/>However, I suggest you don't take on the Main Scenario quests just yet,<br/>since they offer numerous challenges only a truly skilled person can handle.");
							}
						}
					}
					else if(category == "@communication")
					{
						while(true)
						{
							Msg("How to hit it off with other travelers in Erinn!<br/>Choose a question and Eiry will explain.",
								List("Community", 5, "@breakloop",
									Button("How can I talk to others?", "@howtotalkothers"),
									Button("How can I trade with others?", "@howtotrade"),
									Button("How can I form a party with others?", "@makeparty"),
									Button("How can I share food with others?", "@eattogether"),
									Button("How can I add a friend to the Messenger?", "@listingfriends")
								)
							);
						
							category = await Select();
							
							if(category == "@breakloop" || category == "@end")
								break;
							
							if(category == "@howtotalkothers")
							{
								Msg("Press 'Enter' and a chat window will appear.<br/>Type in your message, and others will see your words.");
								Msg("Press 'Tab' while the chat window is up to talk only to your party members,<br/>whisper to someone, or talk to a designated person.");
								Msg("Just remember, you may not hear others speak while doing this, so use it wisely.");
							}
							else if(category == "@howtotrade")
							{
								Msg("Do you want to trade items with another person?<br/>If so, move the cursor to that person and right-click.<br/>Then, select 'Trade' from the menu.");
								Msg("The trade process cannot be completed unless both sides press the 'Complete' button.<br/>Check the item to make sure it's what you asked for before selecting 'Complete'.");
							}
							else if(category == "@makeparty")
							{
								Msg("Do you want to do something as a group?<br/>There are two ways to be in a party - join someone else's or create one yourself.");
								Msg("If you want to join a party created by someone else,<br/>click the Party window over that person's head.<br/>I heard that some people use the Party window as a billboard instead!");
								Msg("If you want to create your own party, press <hotkey name='PartyView'/> and bring up the Party window.<br/>Password is not required - fill it in only when needed.<br/>However, you must fill in the party name to successfully create a party.");
							}
							else if(category == "@eattogether")
							{
								Msg("Do you want to share your food with others? If so, find a campfire first.<br/>If you sit next to the campfire, the message<br/>'Food can be shared between those seated at the campfire' will appear on screen.");
								Msg("Right-click the food that you want to share, and select 'Use' from the menu.<br/>That will bring up the Sharing menu.<br/>Be careful not to use the hotkey assigned for eating food, since food cannot be shared that way.");
							}
							else if(category == "@listingfriends")
							{
								Msg("Have you met someone whose accompaniment you would like on your next adventure?<br/>Move the cursor to the person you want to add and right-click.<br/>Once you select 'Add Friend' from the menu, the person will be added to your friend list,<b/>and the person will be asked to approve.");
								Msg("The process is successfully completed when the person accepts your request.<br/>It is very convenient to use because<br/>you will easily see when a friend is online.");
							}
						}
					}
					else if(category == "@general")
					{
						while(true)
						{
							Msg("Yay! It's Eiry's corner!<br/>These are the topics I'd like to discuss with you.",
								List("Tips from Eiry", 5, "@breakloop",
									Button("Watch the Minimap.", "@seeminimap")
								)
							);
						
							category = await Select();
							
							if(category == "@breakloop" || category == "@end")
								break;
							
							if(category == "@seeminimap")
							{
								Msg("Do you use the Minimap often? You already know that you can press <hotkey name='MiniMapView'/> to bring up the Minimap, right?<br/>The top of the Minimap represents north and the bottom south,<br/>so use the Minimap to guide you to different places!");
								Msg("You should also use the map I gave you with my notes, along with the Minimap.<br/>You can see my map by pressing the 'Where am I?' button. Oh, you already knew that.");
							}
						}
					}
				}
			}
			else if(reply == "@tips_generalrandom")
			{
				// Yes, this is logged from NA, A+ Nexon.
				Msg("다음번에도 함께 모험을 하고픈 사람을 만나셨어요?<br/>그 사람에게 커서를 대고 오른쪽 클릭을 해보세요.<br/>나오는 메뉴 중 '친구 추가'라는 항목을 선택하면<br/>친구 리스트에 오르게 되고, 그 사람에게 가부를 물어보게 되지요.");
				Msg("상대방이 허락해주면 친구 등록은 끝~!<br/>등록한 친구는 언제 접속을 하는지<br/>금방 알 수 있어서 편리하답니다.");
			}
		}
		
		Close("Thanks for the help, Eiry!");
	}
}
