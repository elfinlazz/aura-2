//--- Aura Script -----------------------------------------------------------
// Tir Chonaill (1)
//--- Description -----------------------------------------------------------
// Warp, prop, and spawn definitions for Tir.
//---------------------------------------------------------------------------

public class TirRegionScript : BaseScript
{
	public override void Load()
	{
		LoadWarps();
		LoadSpawns();
		LoadPropDrops();
	}
	
	public void LoadWarps()
	{
		//SetPropBehavior(0xA0007D00060274, PropWarp(1, 15388, 38706));

		// Bank
		SetPropBehavior(0x00A000010009042A, PropWarp(2, 2102, 1184));
		SetPropBehavior(0x00A0000200010001, PropWarp(1, 11475, 39606));
                        
		// Duncan       
		SetPropBehavior(0x00A00001000901BC, PropWarp(3, 2351, 1880));
		SetPropBehavior(0x00A0000300010001, PropWarp(1, 16061, 38154));
                        
		// Church       
		SetPropBehavior(0x00A00001000902CB, PropWarp(4, 1693, 687));
		SetPropBehavior(0x00A0000400010002, PropWarp(1, 5888, 37471));
                        
		// Grocery Store
		SetPropBehavior(0x00A0000100090338, PropWarp(5, 2310, 1712));
		SetPropBehavior(0x00A0000500010001, PropWarp(1, 11482, 37274));
		                
		// Healer       
		SetPropBehavior(0x00A000010009016B, PropWarp(6, 664, 854));
		SetPropBehavior(0x00A0000600010001, PropWarp(1, 13575, 44611));
                        
		// Inn          
		SetPropBehavior(0x00A00001000901C9, PropWarp(7, 1419, 685));
		SetPropBehavior(0x00A0000700010001, PropWarp(1, 15814, 33691));
		                
		// General Shop 
		SetPropBehavior(0x00A00001000900F0, PropWarp(8, 845, 2440));
		SetPropBehavior(0x00A0000800010003, PropWarp(1, 13113, 36454));
                        
		// Shool        
		SetPropBehavior(0x00A0000100090395, PropWarp(9, 2348, 886));
		SetPropBehavior(0x00A0000900010001, PropWarp(1, 4093, 32909));
                        
		// Ciar         
		SetPropBehavior(0x00A0000100050009, PropWarp(11, 3886, 3297));
		SetPropBehavior(0x00A0000B00010002, PropWarp(1, 28761, 30725));
                        
		// Alby         
		SetPropBehavior(0x00A000010008003E, PropWarp(13, 3197, 2518));
		SetPropBehavior(0x00A0000D00010006, PropWarp(1, 9756, 59227));
                        
		// Dugald Aisle 
		SetPropBehavior(0x00A0000100030067, PropWarp(16, 28545, 96881));
		SetPropBehavior(0x00A0001000020028, PropWarp(1, 5067, 17156));
                        
		// Sidhe        
		SetPropBehavior(0x00A0000100080067, PropWarp(47, 9985, 6522));
		SetPropBehavior(0x00A0002F000100C9, PropWarp(1, 1748, 59187));
                        
		// Beginner Tutorial
		SetPropBehavior(0x00A003EC000100C4, PropWarp(1, 12785, 38383));
		SetPropBehavior(0x00A003EC00010231, PropWarp(1, 12785, 38383));
		SetPropBehavior(0x00A003EC0001011B, PropWarp(1004, 18517, 12543));
		SetPropBehavior(0x00A003EC0001011C, PropWarp(1004, 17654, 14772));
		                
		SetPropBehavior(0x00A000790001000A, PropWarp(11, 3193, 4319));
		SetPropBehavior(0x00A0000B00010025, (cr, pr) =>
		{
			if(cr.LevelTotal >= 250)
				cr.Warp(121, 3206, 2085);
			else
				Send.Notice(cr, "You need a cumulative level of at least 250.");
		});

		// TNN indoor -> Tir ...?
		//SetPropBehavior(0x00A0002400010002, PropWarp(1, 16061, 38154));
		//SetPropBehavior(0x00A0002500010002, PropWarp(1, 5888, 37471));
		//SetPropBehavior(0x00A0002600010001, PropWarp(1, 11482, 37274));
		//SetPropBehavior(0x00A0002700010002, PropWarp(1, 13575, 44611));
		//SetPropBehavior(0x00A0002800010002, PropWarp(1, 15814, 33691));
		//SetPropBehavior(0x00A0002900010001, PropWarp(1, 13113, 36454));
		//SetPropBehavior(0x00A0002A00010002, PropWarp(1, 4093, 32909));
		//SetPropBehavior(0x00A0002B00010002, PropWarp(1, 11475, 39606));
	}
	
	public void LoadSpawns()
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
	
	public void LoadPropDrops()
	{
		SetPropBehavior(0x00A000010001000F, PropDrop(2));
		SetPropBehavior(0x00A0000100010012, PropDrop(2));
		SetPropBehavior(0x00A0000100010016, PropDrop(2));
		SetPropBehavior(0x00A0000100010019, PropDrop(2));
		SetPropBehavior(0x00A000010001001B, PropDrop(2));
		SetPropBehavior(0x00A0000100030004, PropDrop(2));
		SetPropBehavior(0x00A0000100030005, PropDrop(2));
		SetPropBehavior(0x00A000010003000A, PropDrop(2));
		SetPropBehavior(0x00A000010003000C, PropDrop(2));
		SetPropBehavior(0x00A0000100030020, PropDrop(1));
		SetPropBehavior(0x00A0000100030022, PropDrop(1));
		SetPropBehavior(0x00A0000100030028, PropDrop(2));
		SetPropBehavior(0x00A0000100030031, PropDrop(2));
		SetPropBehavior(0x00A0000100030038, PropDrop(1));
		SetPropBehavior(0x00A0000100030041, PropDrop(2));
		SetPropBehavior(0x00A0000100030046, PropDrop(2));
		SetPropBehavior(0x00A000010003004D, PropDrop(2));
		SetPropBehavior(0x00A000010003006B, PropDrop(1));
		SetPropBehavior(0x00A000010004000C, PropDrop(2));
		SetPropBehavior(0x00A000010004000F, PropDrop(2));
		SetPropBehavior(0x00A0000100040013, PropDrop(1));
		SetPropBehavior(0x00A0000100040015, PropDrop(2));
		SetPropBehavior(0x00A0000100050006, PropDrop(2));
		SetPropBehavior(0x00A000010005000A, PropDrop(1));
		SetPropBehavior(0x00A0000100050016, PropDrop(1));
		SetPropBehavior(0x00A0000100050024, PropDrop(1));
		SetPropBehavior(0x00A000010005002C, PropDrop(1));
		SetPropBehavior(0x00A0000100050034, PropDrop(1));
		SetPropBehavior(0x00A0000100050035, PropDrop(1));
		SetPropBehavior(0x00A0000100050045, PropDrop(2));
		SetPropBehavior(0x00A000010005004A, PropDrop(2));
		SetPropBehavior(0x00A0000100060011, PropDrop(2));
		SetPropBehavior(0x00A0000100060013, PropDrop(1));
		SetPropBehavior(0x00A0000100060019, PropDrop(2));
		SetPropBehavior(0x00A000010006001F, PropDrop(2));
		SetPropBehavior(0x00A0000100060031, PropDrop(2));
		SetPropBehavior(0x00A0000100060032, PropDrop(1));
		SetPropBehavior(0x00A0000100070002, PropDrop(1));
		SetPropBehavior(0x00A000010007000C, PropDrop(2));
		SetPropBehavior(0x00A000010007000F, PropDrop(2));
		SetPropBehavior(0x00A0000100070014, PropDrop(2));
		SetPropBehavior(0x00A0000100070021, PropDrop(1));
		SetPropBehavior(0x00A0000100070023, PropDrop(2));
		SetPropBehavior(0x00A0000100070024, PropDrop(54));
		SetPropBehavior(0x00A000010007002A, PropDrop(2));
		SetPropBehavior(0x00A000010007002B, PropDrop(2));
		SetPropBehavior(0x00A000010007002C, PropDrop(1));
		SetPropBehavior(0x00A0000100080009, PropDrop(1));
		SetPropBehavior(0x00A000010008000A, PropDrop(2));
		SetPropBehavior(0x00A000010008000C, PropDrop(2));
		SetPropBehavior(0x00A000010008001C, PropDrop(2));
		SetPropBehavior(0x00A0000100080020, PropDrop(1));
		SetPropBehavior(0x00A0000100080024, PropDrop(2));
		SetPropBehavior(0x00A0000100080027, PropDrop(1));
		SetPropBehavior(0x00A0000100080028, PropDrop(1));
		SetPropBehavior(0x00A000010008002D, PropDrop(1));
		SetPropBehavior(0x00A000010008003C, PropDrop(2));
		SetPropBehavior(0x00A0000100080048, PropDrop(1));
		SetPropBehavior(0x00A000010008008B, PropDrop(1));
		SetPropBehavior(0x00A000010008008C, PropDrop(1));
		SetPropBehavior(0x00A000010008008D, PropDrop(1));
		SetPropBehavior(0x00A000010008008E, PropDrop(1));
		SetPropBehavior(0x00A000010008008F, PropDrop(1));
		SetPropBehavior(0x00A0000100080090, PropDrop(1));
		SetPropBehavior(0x00A0000100080091, PropDrop(1));
		SetPropBehavior(0x00A0000100080092, PropDrop(1));
		SetPropBehavior(0x00A0000100080093, PropDrop(1));
		SetPropBehavior(0x00A000010008009A, PropDrop(1));
		SetPropBehavior(0x00A0000100090001, PropDrop(1));
		SetPropBehavior(0x00A0000100090003, PropDrop(2));
		SetPropBehavior(0x00A0000100090005, PropDrop(2));
		SetPropBehavior(0x00A000010009000D, PropDrop(1));
		SetPropBehavior(0x00A000010009000F, PropDrop(2));
		SetPropBehavior(0x00A0000100090015, PropDrop(2));
		SetPropBehavior(0x00A000010009001D, PropDrop(1));
		SetPropBehavior(0x00A0000100090020, PropDrop(1));
		SetPropBehavior(0x00A0000100090024, PropDrop(1));
		SetPropBehavior(0x00A0000100090027, PropDrop(2));
		SetPropBehavior(0x00A000010009002C, PropDrop(2));
		SetPropBehavior(0x00A000010009002F, PropDrop(1));
		SetPropBehavior(0x00A0000100090032, PropDrop(1));
		SetPropBehavior(0x00A0000100090034, PropDrop(1));
		SetPropBehavior(0x00A0000100090037, PropDrop(1));
		SetPropBehavior(0x00A000010009003A, PropDrop(1));
		SetPropBehavior(0x00A000010009003B, PropDrop(1));
		SetPropBehavior(0x00A000010009003E, PropDrop(2));
		SetPropBehavior(0x00A0000100090044, PropDrop(2));
		SetPropBehavior(0x00A0000100090045, PropDrop(2));
		SetPropBehavior(0x00A0000100090059, PropDrop(2));
		SetPropBehavior(0x00A000010009005A, PropDrop(2));
		SetPropBehavior(0x00A0000100090060, PropDrop(1));
		SetPropBehavior(0x00A0000100090061, PropDrop(2));
		SetPropBehavior(0x00A0000100090068, PropDrop(1));
		SetPropBehavior(0x00A0000100090072, PropDrop(2));
		SetPropBehavior(0x00A0000100090075, PropDrop(2));
		SetPropBehavior(0x00A000010009007A, PropDrop(2));
		SetPropBehavior(0x00A000010009007F, PropDrop(1));
		SetPropBehavior(0x00A0000100090085, PropDrop(2));
		SetPropBehavior(0x00A000010009008C, PropDrop(2));
		SetPropBehavior(0x00A000010009009E, PropDrop(1));
		SetPropBehavior(0x00A00001000900A1, PropDrop(1));
		SetPropBehavior(0x00A00001000900A3, PropDrop(2));
		SetPropBehavior(0x00A00001000900A5, PropDrop(2));
		SetPropBehavior(0x00A00001000900A6, PropDrop(1));
		SetPropBehavior(0x00A00001000900A8, PropDrop(1));
		SetPropBehavior(0x00A00001000900A9, PropDrop(2));
		SetPropBehavior(0x00A00001000900AA, PropDrop(1));
		SetPropBehavior(0x00A00001000900AC, PropDrop(1));
		SetPropBehavior(0x00A00001000900B3, PropDrop(1));
		SetPropBehavior(0x00A00001000900BD, PropDrop(2));
		SetPropBehavior(0x00A00001000900BF, PropDrop(2));
		SetPropBehavior(0x00A00001000900C2, PropDrop(2));
		SetPropBehavior(0x00A00001000900C7, PropDrop(2));
		SetPropBehavior(0x00A00001000900C8, PropDrop(2));
		SetPropBehavior(0x00A00001000900CA, PropDrop(2));
		SetPropBehavior(0x00A00001000900DA, PropDrop(1));
		SetPropBehavior(0x00A00001000900DE, PropDrop(1));
		SetPropBehavior(0x00A00001000900E4, PropDrop(2));
		SetPropBehavior(0x00A00001000900EC, PropDrop(2));
		SetPropBehavior(0x00A00001000900EF, PropDrop(1));
		SetPropBehavior(0x00A00001000900F3, PropDrop(2));
		SetPropBehavior(0x00A00001000900FF, PropDrop(2));
		SetPropBehavior(0x00A0000100090116, PropDrop(2));
		SetPropBehavior(0x00A000010009011B, PropDrop(2));
		SetPropBehavior(0x00A000010009011D, PropDrop(1));
		SetPropBehavior(0x00A0000100090125, PropDrop(2));
		SetPropBehavior(0x00A000010009012D, PropDrop(1));
		SetPropBehavior(0x00A0000100090133, PropDrop(2));
		SetPropBehavior(0x00A0000100090135, PropDrop(2));
		SetPropBehavior(0x00A0000100090138, PropDrop(1));
		SetPropBehavior(0x00A000010009013B, PropDrop(2));
		SetPropBehavior(0x00A0000100090142, PropDrop(2));
		SetPropBehavior(0x00A0000100090146, PropDrop(2));
		SetPropBehavior(0x00A0000100090147, PropDrop(2));
		SetPropBehavior(0x00A0000100090149, PropDrop(2));
		SetPropBehavior(0x00A0000100090151, PropDrop(2));
		SetPropBehavior(0x00A000010009015D, PropDrop(2));
		SetPropBehavior(0x00A000010009015F, PropDrop(2));
		SetPropBehavior(0x00A000010009016C, PropDrop(2));
		SetPropBehavior(0x00A0000100090170, PropDrop(2));
		SetPropBehavior(0x00A0000100090173, PropDrop(1));
		SetPropBehavior(0x00A000010009017F, PropDrop(2));
		SetPropBehavior(0x00A0000100090191, PropDrop(1));
		SetPropBehavior(0x00A0000100090192, PropDrop(2));
		SetPropBehavior(0x00A0000100090194, PropDrop(1));
		SetPropBehavior(0x00A0000100090196, PropDrop(2));
		SetPropBehavior(0x00A00001000901A5, PropDrop(2));
		SetPropBehavior(0x00A00001000901AA, PropDrop(2));
		SetPropBehavior(0x00A00001000901BB, PropDrop(1));
		SetPropBehavior(0x00A00001000901BE, PropDrop(2));
		SetPropBehavior(0x00A00001000901C5, PropDrop(2));
		SetPropBehavior(0x00A00001000901CC, PropDrop(1));
		SetPropBehavior(0x00A00001000901CD, PropDrop(2));
		SetPropBehavior(0x00A00001000901E2, PropDrop(2));
		SetPropBehavior(0x00A00001000901EB, PropDrop(2));
		SetPropBehavior(0x00A00001000901EE, PropDrop(1));
		SetPropBehavior(0x00A00001000901F0, PropDrop(1));
		SetPropBehavior(0x00A00001000901FA, PropDrop(2));
		SetPropBehavior(0x00A0000100090203, PropDrop(1));
		SetPropBehavior(0x00A0000100090204, PropDrop(2));
		SetPropBehavior(0x00A0000100090207, PropDrop(2));
		SetPropBehavior(0x00A000010009020E, PropDrop(1));
		SetPropBehavior(0x00A0000100090212, PropDrop(2));
		SetPropBehavior(0x00A000010009021C, PropDrop(1));
		SetPropBehavior(0x00A0000100090224, PropDrop(2));
		SetPropBehavior(0x00A0000100090225, PropDrop(2));
		SetPropBehavior(0x00A0000100090234, PropDrop(1));
		SetPropBehavior(0x00A0000100090235, PropDrop(2));
		SetPropBehavior(0x00A0000100090236, PropDrop(1));
		SetPropBehavior(0x00A0000100090237, PropDrop(2));
		SetPropBehavior(0x00A0000100090244, PropDrop(1));
		SetPropBehavior(0x00A000010009025F, PropDrop(2));
		SetPropBehavior(0x00A0000100090262, PropDrop(2));
		SetPropBehavior(0x00A0000100090271, PropDrop(2));
		SetPropBehavior(0x00A0000100090279, PropDrop(1));
		SetPropBehavior(0x00A000010009027A, PropDrop(2));
		SetPropBehavior(0x00A0000100090284, PropDrop(1));
		SetPropBehavior(0x00A0000100090286, PropDrop(1));
		SetPropBehavior(0x00A0000100090289, PropDrop(1));
		SetPropBehavior(0x00A000010009028D, PropDrop(1));
		SetPropBehavior(0x00A0000100090293, PropDrop(1));
		SetPropBehavior(0x00A00001000902AB, PropDrop(2));
		SetPropBehavior(0x00A00001000902AD, PropDrop(2));
		SetPropBehavior(0x00A00001000902AE, PropDrop(2));
		SetPropBehavior(0x00A00001000902B1, PropDrop(1));
		SetPropBehavior(0x00A00001000902B6, PropDrop(1));
		SetPropBehavior(0x00A00001000902B9, PropDrop(1));
		SetPropBehavior(0x00A00001000902BE, PropDrop(2));
		SetPropBehavior(0x00A00001000902BF, PropDrop(2));
		SetPropBehavior(0x00A00001000902C5, PropDrop(1));
		SetPropBehavior(0x00A00001000902D2, PropDrop(2));
		SetPropBehavior(0x00A00001000902D3, PropDrop(1));
		SetPropBehavior(0x00A00001000902D4, PropDrop(2));
		SetPropBehavior(0x00A00001000902D5, PropDrop(1));
		SetPropBehavior(0x00A00001000902E3, PropDrop(2));
		SetPropBehavior(0x00A00001000902E7, PropDrop(2));
		SetPropBehavior(0x00A00001000902ED, PropDrop(2));
		SetPropBehavior(0x00A00001000902F0, PropDrop(2));
		SetPropBehavior(0x00A00001000902F8, PropDrop(2));
		SetPropBehavior(0x00A00001000902FE, PropDrop(2));
		SetPropBehavior(0x00A0000100090305, PropDrop(1));
		SetPropBehavior(0x00A000010009030A, PropDrop(2));
		SetPropBehavior(0x00A000010009030E, PropDrop(1));
		SetPropBehavior(0x00A0000100090342, PropDrop(1));
		SetPropBehavior(0x00A000010009034D, PropDrop(1));
		SetPropBehavior(0x00A0000100090359, PropDrop(2));
		SetPropBehavior(0x00A000010009035A, PropDrop(2));
		SetPropBehavior(0x00A000010009035B, PropDrop(2));
		SetPropBehavior(0x00A0000100090362, PropDrop(2));
		SetPropBehavior(0x00A000010009036C, PropDrop(2));
		SetPropBehavior(0x00A000010009037D, PropDrop(1));
		SetPropBehavior(0x00A000010009038B, PropDrop(2));
		SetPropBehavior(0x00A000010009038F, PropDrop(2));
		SetPropBehavior(0x00A0000100090399, PropDrop(2));
		SetPropBehavior(0x00A000010009039C, PropDrop(1));
		SetPropBehavior(0x00A00001000903A1, PropDrop(2));
		SetPropBehavior(0x00A00001000903AA, PropDrop(2));
		SetPropBehavior(0x00A00001000903B7, PropDrop(2));
		SetPropBehavior(0x00A00001000903BF, PropDrop(2));
		SetPropBehavior(0x00A00001000903C2, PropDrop(2));
		SetPropBehavior(0x00A00001000903D5, PropDrop(1));
		SetPropBehavior(0x00A00001000903D6, PropDrop(2));
		SetPropBehavior(0x00A00001000903DB, PropDrop(2));
		SetPropBehavior(0x00A00001000903E1, PropDrop(2));
		SetPropBehavior(0x00A00001000903E4, PropDrop(2));
		SetPropBehavior(0x00A00001000903E7, PropDrop(2));
		SetPropBehavior(0x00A00001000903EB, PropDrop(1));
		SetPropBehavior(0x00A00001000903F0, PropDrop(2));
		SetPropBehavior(0x00A00001000903F3, PropDrop(1));
		SetPropBehavior(0x00A00001000903F4, PropDrop(2));
		SetPropBehavior(0x00A000010009040D, PropDrop(1));
		SetPropBehavior(0x00A0000100090411, PropDrop(2));
		SetPropBehavior(0x00A0000100090413, PropDrop(2));
		SetPropBehavior(0x00A000010009041E, PropDrop(2));
		SetPropBehavior(0x00A0000100090432, PropDrop(2));
		SetPropBehavior(0x00A0000100090433, PropDrop(1));
		SetPropBehavior(0x00A000010009043A, PropDrop(2));
		SetPropBehavior(0x00A0000100090441, PropDrop(1));
		SetPropBehavior(0x00A0000100090445, PropDrop(2));
		SetPropBehavior(0x00A000010009044A, PropDrop(1));
		SetPropBehavior(0x00A0000100090452, PropDrop(2));
		SetPropBehavior(0x00A000010009045D, PropDrop(1));
		SetPropBehavior(0x00A0000100090466, PropDrop(1));
		SetPropBehavior(0x00A0000100090468, PropDrop(1));
		SetPropBehavior(0x00A0000100090474, PropDrop(2));
		SetPropBehavior(0x00A0000100090477, PropDrop(1));
		SetPropBehavior(0x00A0000100090479, PropDrop(2));
		SetPropBehavior(0x00A0000100090484, PropDrop(2));
		SetPropBehavior(0x00A000010009048B, PropDrop(2));
		SetPropBehavior(0x00A0000100090495, PropDrop(1));
		SetPropBehavior(0x00A0000100090497, PropDrop(2));
		SetPropBehavior(0x00A0000100090498, PropDrop(2));
		SetPropBehavior(0x00A0000100090499, PropDrop(2));
		SetPropBehavior(0x00A000010009049E, PropDrop(1));
		SetPropBehavior(0x00A000010009049F, PropDrop(2));
		SetPropBehavior(0x00A00001000904A3, PropDrop(2));
		SetPropBehavior(0x00A00001000904B3, PropDrop(2));
		SetPropBehavior(0x00A00001000904BD, PropDrop(2));
		SetPropBehavior(0x00A00001000904BF, PropDrop(2));
		SetPropBehavior(0x00A00001000904C1, PropDrop(2));
		SetPropBehavior(0x00A00001000904C4, PropDrop(1));
		SetPropBehavior(0x00A00001000904CE, PropDrop(2));
		SetPropBehavior(0x00A00001000904CF, PropDrop(2));
		SetPropBehavior(0x00A00001000904DA, PropDrop(1));
		SetPropBehavior(0x00A00001000904DB, PropDrop(1));
		SetPropBehavior(0x00A00001000904DF, PropDrop(2));
		SetPropBehavior(0x00A00001000904E0, PropDrop(2));
		SetPropBehavior(0x00A00001000904E2, PropDrop(1));
		SetPropBehavior(0x00A0000100090504, PropDrop(841));
	}
}
