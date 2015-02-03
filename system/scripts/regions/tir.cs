//--- Aura Script -----------------------------------------------------------
// Tir Chonaill (1)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Tir.
//---------------------------------------------------------------------------

public class TirRegionScript : RegionScript
{
	public override void InitializeRegion()
	{
		// Set up windmill prop
		ChannelServer.Instance.World.GetRegion(1).GetProp(0xA000010009042B).Xml.SetAttributeValue("EventText", "The Mill is currently not in operation.\nOnce you operate it, you can grind the crops into flour.");
	}

	public override void LoadWarps()
	{
		// Bank
		SetPropBehavior(0x00A000010009042A, PropWarp(1,11300,39744, 2,2102,1184));
		SetPropBehavior(0x00A0000200010001, PropWarp(2,2249,1031, 1,11475,39606));

		// Duncan
		SetPropBehavior(0x00A00001000901BC, PropWarp(1,16075,37997, 3,2351,1880));
		SetPropBehavior(0x00A0000300010001, PropWarp(3,2325,2058, 1,16061,38154));

		// Church
		SetPropBehavior(0x00A00001000902CB, PropWarp(1,5843,37600, 4,1693,687));
		SetPropBehavior(0x00A0000400010002, PropWarp(4,1779,457, 1,5888,37471));

		// Grocery Store
		SetPropBehavior(0x00A0000100090338, PropWarp(1,11259,37258, 5,2310,1712));
		SetPropBehavior(0x00A0000500010001, PropWarp(5,2448,1705, 1,11482,37274));

		// Healer
		SetPropBehavior(0x00A000010009016B, PropWarp(1,13767,44662, 6,664,854));
		SetPropBehavior(0x00A0000600010001, PropWarp(6,516,826, 1,13575,44611));

		// Inn
		SetPropBehavior(0x00A00001000901C9, PropWarp(1,15850,33972, 7,1419,685));
		SetPropBehavior(0x00A0000700010001, PropWarp(7,1416,542, 1,15814,33691));

		// General Shop
		SetPropBehavior(0x00A00001000900F0, PropWarp(1,13218,36376, 8,845,2440));
		SetPropBehavior(0x00A0000800010003, PropWarp(8,725,2576, 1,13113,36454));

		// School
		SetPropBehavior(0x00A0000100090395, PropWarp(1,3877,32969, 9,2348,886));
		SetPropBehavior(0x00A0000900010001, PropWarp(9,2479,949, 1,4093,32909));

		// Dugald
		SetPropBehavior(0x00A0000100030067, PropWarp(1,4916,15545, 16,28545,96881));
		SetPropBehavior(0x00A0001000020028, PropWarp(16,28517,98372, 1,5067,17156));

		// Ciar
		SetPropBehavior(0x00A0000100050009, PropWarp(1,27690,30381, 11,3886,3297));
		SetPropBehavior(0x00A0000B00010002, PropWarp(11,4433,3194, 1,28761,30725));

		// Ciar Hard
		SetPropBehavior(0x00A000790001000A, PropWarp(121,3202,1795, 11,3193,4319));
		SetPropBehavior(0x00A0000B00010025, (creature, prop) =>
		{
			if(creature.TotalLevel >= 250)
				creature.Warp(121, 3206, 2085);
			else
				Send.Notice(creature, "You need a cumulative level of at least 250.");
		});
		
		// Alby
		SetPropBehavior(0x00A000010008003E, PropWarp(1,9748,60215, 13,3197,2518));
		SetPropBehavior(0x00A0000D00010006, PropWarp(13,3177,1982, 1,9756,59227));

		// Alby Hard
		SetPropBehavior(0x00A0001B00000009, PropWarp(27,3203,1814, 13,3198,4319));
		SetPropBehavior(0x00A0000D00010007, (creature, prop) =>
		{
			if(creature.TotalLevel >= 250)
				creature.Warp(27, 3209, 2084);
			else
				Send.Notice(creature, "You need a cumulative level of at least 250.");
		});
		
		// Alby Arena Lobby - Alby Altar
		SetPropBehavior(0x00A0001C00020003, PropWarp(28,1189,1167, 13,3198,2828));
		SetPropBehavior(0x00A0000D00010007, PropWarp(13,3196,4585, 27,3209,2084));

		// Alby Arena - Alby Arena Lobby
		SetPropBehavior(0x00A0001D0001000C, PropWarp(29,1605,1615, 28,1202,3404));
		SetPropBehavior(0x00A0001C00020003, PropWarp(28,1189,1167, 13,3198,2828));

		// Alby Arena - Alby Arena Lobby
		SetPropBehavior(0x00A0001D00010015, PropWarp(29,1610,4812, 28,1202,3404));

		// Alby Arena - Alby Arena Lobby
		SetPropBehavior(0x00A0001D0001001A, PropWarp(29,4796,1599, 28,1202,3404));

		// Alby Arena - Alby Arena Lobby
		SetPropBehavior(0x00A0001D0001001B, PropWarp(29,4816,4814, 28,1202,3404));
	}
	
	public override void LoadSpawns()
	{
		// Southern Fields
		CreatureSpawn(20001, 15, 1, 11926,25589, 15256,22203, 11553,18350, 4721,17364, 4035,21583); // Gray Wolf
		CreatureSpawn(20003, 5,  1, 7375,17002,  13727,17316, 16027,21792, 11708,19855);            // White Wolf
		
		CreatureSpawn(40001, 4,  1, 11110,25113, 15132,28149, 15825,23645); // Sheep
		CreatureSpawn(20101, 1,  1, 11110,25113, 15132,28149, 15825,23645); // Dog
		
		// North
		CreatureSpawn(50001, 10, 1, 9796,43776, 10352,48765, 12167,45980, 12139,42264); // Brown Fox
		CreatureSpawn(50007, 6,  1, 9796,43776, 10352,48765, 12167,45980, 12139,42264); // Young Brown Fox
		CreatureSpawn(50007, 1,  1, 9796,43776, 10352,48765, 12167,45980, 12139,42264); // Red Fox
		
		CreatureSpawn(50001, 5, 1, 12077,49344, 7718,54523, 6915,53857, 10288,49285); // Brown Fox
		CreatureSpawn(50007, 5, 1, 12077,49344, 7718,54523, 6915,53857, 10288,49285); // Young Brown Fox
		
		CreatureSpawn(40001, 6,  1, 27031,43284, 30730,38996, 25747,37402); // Sheep
		
		// Graveyard
		CreatureSpawn(30001, 20, 1, 16475,46411, 22344,47653, 21819,40695, 16047,40496); // White Spider
		CreatureSpawn(30003, 1,  1, 16475,46411, 22344,47653, 21819,40695, 16047,40496); // Red Spider
		
		// Eastern Fields
		CreatureSpawn(50002, 10, 1, 32776,44588, 34600,41400, 39100,44400, 37500,48200); // Red Fox
		CreatureSpawn(50005, 5,  1, 32776,44588, 34600,41400, 39100,44400, 37500,48200); // Young Red Fox
		CreatureSpawn(50003, 4,  1, 39732,46937, 41440,43889, 45191,42226, 44339,46377); // Gray Fox
		CreatureSpawn(50006, 2,  1, 39732,46937, 41440,43889, 45191,42226, 44339,46377); // Young Gray Fox
		
		CreatureSpawn(20002, 5,  1, 45155,47360, 46286,40727, 47877,42053, 49088,45648); // Black Wolf
		CreatureSpawn(20005, 2,  1, 45144,37935, 45716,33852, 41942,30286, 38000,36000); // Brown Dire Wolf
		CreatureSpawn(20009, 4,  1, 45144,37935, 45716,33852, 41942,30286, 38000,36000); // Brown Dire Wolf Cub
		
		// Church
		CreatureSpawn(60002, 1,  1, 9370,39024, 7556,40150, 5927,39309, 6512,38073); // Rooster
		CreatureSpawn(60003, 4,  1, 9370,39024, 7556,40150, 5927,39309, 6512,38073); // Hen
		CreatureSpawn(60004, 8,  1, 9370,39024, 7556,40150, 5927,39309, 6512,38073); // Chick
	}
}
