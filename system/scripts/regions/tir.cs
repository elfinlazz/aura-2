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
		// Southern Fields (disabled during MabiLand event)
		CreateSpawner(race: 40001, amount: 8, region: 1, coordinates: A(9991,21498, 8678,23454, 13515,26700, 14828,24745)); // Sheep
		CreateSpawner(race: 20101, amount: 5, region: 1, coordinates: A(9991,21498, 8678,23454, 13515,26700, 14828,24745)); // Dog
		//CreateSpawner(race: 40001, amount: 8, region: 1, coordinates: A(13701,23421, 13701,24840, 14891,24840, 14891,23421)); // Sheep
		//CreateSpawner(race: 20101, amount: 5, region: 1, coordinates: A(13701,23421, 13701,24840, 14891,24840, 14891,23421)); // Dog

		CreateSpawner(race: 20001, amount: 20, region: 1, coordinates: A(6690,20303, 13585,25668, 16347,22118, 9452,16754)); // Gray Wolf
		CreateSpawner(race: 40001, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(6690,20303, 13585,25668, 16347,22118, 9452,16754)); // Sheep
		CreateSpawner(race: 20101, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(6690,20303, 13585,25668, 16347,22118, 9452,16754)); // Dog

		CreateSpawner(race: 20003, amount: 8, region: 1, coordinates: A(13481,19473, 12479,17258, 4882,20694, 5884,22909)); // White Wolf
		CreateSpawner(race: 40001, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(13481,19473, 12479,17258, 4882,20694, 5884,22909)); // Sheep
		CreateSpawner(race: 20101, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(13481,19473, 12479,17258, 4882,20694, 5884,22909)); // Dog

		// Pasture
		CreateSpawner(race: 40001, amount: 10, region: 1, coordinates: A(32785,42317, 26117,37787, 24726,39834, 31394,44365)); // Sheep
		CreateSpawner(race: 20101, amount: 5, region: 1, coordinates: A(32785,42317, 26117,37787, 24726,39834, 31394,44365)); // Dog
		CreateSpawner(race: 20001, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(32785,42317, 26117,37787, 24726,39834, 31394,44365)); // Gray Wolf

		// Eastern Fields
		CreateSpawner(race: 50002, amount: 10, region: 1, coordinates: A(36777,49202, 39864,46467, 32796,38490, 29710,41224)); // Red Fox
		CreateSpawner(race: 50005, amount: 5, region: 1, coordinates: A(36777,49202, 39864,46467, 32796,38490, 29710,41224)); // Little Red Fox
		CreateSpawner(race: 60002, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(36777,49202, 39864,46467, 32796,38490, 29710,41224)); // Cock
		CreateSpawner(race: 60003, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(36777,49202, 39864,46467, 32796,38490, 29710,41224)); // Hen

		CreateSpawner(race: 50003, amount: 15, region: 1, coordinates: A(36397,43859, 36397,47207, 44503,47207, 44503,43859)); // Gray Fox
		CreateSpawner(race: 50006, amount: 6, region: 1, coordinates: A(36397,43859, 36397,47207, 44503,47207, 44503,43859)); // Little Gray Fox

		CreateSpawner(race: 20002, amount: 15, region: 1, coordinates: A(49301,45115, 46607,40197, 41783,42839, 44477,47758)); // Black Wolf

		CreateSpawner(race: 20005, amount: 4, region: 1, coordinates: A(42417,40216, 47411,34463, 44174,31653, 39180,37406)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 6, region: 1, coordinates: A(42417,40216, 47411,34463, 44174,31653, 39180,37406)); // Little Brown Dire Wolf

		CreateSpawner(race: 20005, amount: 4, region: 1, coordinates: A(37271,31057, 37271,37138, 42145,37138, 42145,31057)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 6, region: 1, coordinates: A(37271,31057, 37271,37138, 42145,37138, 42145,31057)); // Little Brown Dire Wolf

		// Graveyard
		CreateSpawner(race: 30014, amount: 22, region: 1, coordinates: A(18595,40025, 15904,44605, 21371,47817, 24062,43237)); // White Spider
		CreateSpawner(race: 30003, amount: 1, region: 1, coordinates: A(18595,40025, 15904,44605, 21371,47817, 24062,43237)); // Red Spider

		// North
		CreateSpawner(race: 50001, amount: 6, region: 1, coordinates: A(9938,43120, 9923,47580, 13341,47591, 13356,43131)); // Brown Fox
		CreateSpawner(race: 50007, amount: 9, region: 1, coordinates: A(9938,43120, 9923,47580, 13341,47591, 13356,43131)); // Little BrownFox

		CreateSpawner(race: 50001, amount: 5, region: 1, coordinates: A(10625,49354, 8268,52432, 9059,53038, 11416,49960)); // Brown Fox
		CreateSpawner(race: 50007, amount: 5, region: 1, coordinates: A(10625,49354, 8268,52432, 9059,53038, 11416,49960)); // Little BrownFox

		// Church
		CreateSpawner(race: 60002, amount: 4, region: 1, coordinates: A(8177,40200, 8617,38376, 6586,37887, 6146,39711)); // Cock
		CreateSpawner(race: 60003, amount: 10, region: 1, coordinates: A(8177,40200, 8617,38376, 6586,37887, 6146,39711)); // Hen
		CreateSpawner(race: 60004, amount: 5, region: 1, coordinates: A(8177,40200, 8617,38376, 6586,37887, 6146,39711)); // Chicken
		CreateSpawner(race: 50002, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(8177,40200, 8617,38376, 6586,37887, 6146,39711)); // Red Fox

		CreateSpawner(race: 60002, amount: 3, region: 1, coordinates: A(12602,41429, 11861,40520, 10017,42021, 10757,42931)); // Cock
		CreateSpawner(race: 60003, amount: 6, region: 1, coordinates: A(12602,41429, 11861,40520, 10017,42021, 10757,42931)); // Hen
		CreateSpawner(race: 60004, amount: 3, region: 1, coordinates: A(12602,41429, 11861,40520, 10017,42021, 10757,42931)); // Chicken
		CreateSpawner(race: 50002, amount: 1, region: 1, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(12602,41429, 11861,40520, 10017,42021, 10757,42931)); // Red Fox
	}
}
