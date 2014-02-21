//--- Aura Script -----------------------------------------------------------
// Dunbarton (16)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Dunbarton.
//---------------------------------------------------------------------------

public class DunbartonRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Dugald
		SetPropBehavior(0x00A0000E00030010, PropWarp(14,44774,72731, 16,19876,6724));
		SetPropBehavior(0x00A0001000070012, PropWarp(16,19802,3900, 14,44782,70919));

		// Clothing Shop
		SetPropBehavior(0x00A0000E000A0206, PropWarp(14,35110,38777, 17,1427,1365));
		SetPropBehavior(0x00A0001100010002, PropWarp(17,1600,1377, 14,35263,38723));

		// Healer
		SetPropBehavior(0x00A0000E000A0101, PropWarp(14,43531,33092, 19,1120,1048));
		SetPropBehavior(0x00A0001300010002, PropWarp(19,1328,1310, 14,43761,33283));

		// Bank
		SetPropBehavior(0x00A0000E000A0078, PropWarp(14,35735,37455, 20,1590,847));
		SetPropBehavior(0x00A0001400010002, PropWarp(20,1754,843, 14,36037,37442));

		// Church
		SetPropBehavior(0x00A0000E000A007A, PropWarp(14,34394,43229, 21,2171,914));
		SetPropBehavior(0x00A0001500010002, PropWarp(21,2369,701, 14,34593,43030));

		// Gairech
		SetPropBehavior(0x00A0000E00050014, PropWarp(14,58489,1403, 30,31718,98421));
		SetPropBehavior(0x00A0001E00030010, PropWarp(30,31372,100353, 14,58539,2418));

		// School
		SetPropBehavior(0x00A0000E000A01DB, PropWarp(14,45565,40000, 71,9111,9613));
		SetPropBehavior(0x00A0004700000001, PropWarp(71,8934,9610, 14,44615,39995));
		
		// School
		SetPropBehavior(0x00A0001200010001, PropWarp(18,2213,2015, 71,10337,8150));
		SetPropBehavior(0x00A0004700000003, PropWarp(71,10330,7921, 18,2180,1843));
		
		// Dunbarton School Altar - Dunbarton School Library
		//SetPropBehavior(0x00B0004700000007, PropWarp(71,10319,11170, 72,10184,7420));
		SetPropBehavior(0x00A0004800000002, PropWarp(72,10177,7324, 71,10325,11022));

		// Dunbarton School Library Night - Dunbarton School Altar
		SetPropBehavior(0x00A0004C00000002, PropWarp(76,10166,7328, 71,10325,11022));
		//SetPropBehavior(0x00B0004700000007, PropWarp(71,10319,11170, 72,10184,7420));

		// Math
		SetPropBehavior(0x00A0000E00090011, PropWarp(14,58396,59080, 25,3233,2484));
		SetPropBehavior(0x00A0001900010002, PropWarp(25,3201,1992, 14,58399,58590));

		// Rabbie
		SetPropBehavior(0x00A0000E00110015, PropWarp(14,16807,59871, 24,3202,2541));
		SetPropBehavior(0x00A0001800010002, PropWarp(24,3200,1998, 14,16801,59368));
		
		// Rabbie Battle Arena - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A000610000000E, PropWarp(97,3432,7384, 98,1981,4260));
		SetPropBehavior(0x00A0006200010002, PropWarp(98,2000,2000, 24,3200,2833));

		// Rabbie Battle Arena - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A000610000000F, PropWarp(97,6956,7378, 98,1981,4260));

		// Rabbie Battle Arena - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A0006100000010, PropWarp(97,6969,3837, 98,1981,4260));

		// Rabbie Battle Arena - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A0006100000011, PropWarp(97,3429,3835, 98,1981,4260));
		
		// Rabbie Battle Arena Waiting - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A0006300000002, PropWarp(99,2504,2800, 98,1981,4260));
	}
	
	public override void LoadSpawns()
	{
		// ...
	}
}
