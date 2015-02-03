//--- Aura Script -----------------------------------------------------------
// Emain Macha (52)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Emain.
//---------------------------------------------------------------------------

public class EmainRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Sen Mag
		//SetPropBehavior(0x00B00034000500C3, PropWarp(52,61831,7523, 53,65425,107675));
		SetPropBehavior(0x00A0003500020003, PropWarp(53,65425,107675, 52,60692,7282));

		// Coill
		SetPropBehavior(0x00A000340015015C, PropWarp(52,22231,70403, 54,3516,5317));
		SetPropBehavior(0x00A0003600000003, PropWarp(54,3454,4430, 52,22021,69671));

		// Club
		SetPropBehavior(0x00A000340000015E, PropWarp(52,48295,48305, 57,5979,5278));
		SetPropBehavior(0x00A0003900000002, PropWarp(57,5831,5082, 52,47996,48008));

		// Lookout 1
		SetPropBehavior(0x00A0003400000160, PropWarp(52,43559,36832, 58,5431,6702));
		SetPropBehavior(0x00A0003A00000001, PropWarp(58,5472,6861, 52,43551,36956));

		// Lookout 2
		SetPropBehavior(0x00A0003400000161, PropWarp(52,43153,36289, 59,6607,6474));
		SetPropBehavior(0x00A0003B00000001, PropWarp(59,6261,6708, 58,5792,6461));
		
		//  Lookout 1 - Lookout 2
		SetPropBehavior(0x00A0003A00000002, PropWarp(58,5821,6597, 59,6097,6681));
		SetPropBehavior(0x00A0003B00000002, PropWarp(59,6622,6627, 52,43292,36285));

		// Castle
		SetPropBehavior(0x00A0003400000342, PropWarp(52,32194,48624, 60,5744,4518));
		SetPropBehavior(0x00A0003C00000001, PropWarp(60,5690,4375, 52,32432,48369));

		// Castle
		SetPropBehavior(0x00A0003400000343, PropWarp(52,29494,48941, 60,4966,5954));
		SetPropBehavior(0x00A0003C00000003, PropWarp(60,4728,5989, 52,29204,48598));

		// Castle
		SetPropBehavior(0x00A0003400000345, PropWarp(52,31861,51295, 60,6327,5953));
		SetPropBehavior(0x00A0003C00000004, PropWarp(60,6534,6012, 52,32192,51649));
		
		// Emain Macha Castle Chamber B - Emain Macha Castle
		SetPropBehavior(0x00A0004300000001, PropWarp(67,5985,5038, 60,5680,5984));

		// Cathedral
		SetPropBehavior(0x00A000340001006E, PropWarp(52,40797,29115, 61,6696,7282));
		SetPropBehavior(0x00A0003D00000002, PropWarp(61,6726,7630, 52,40810,29478));
		
		// Emain Macha Wedding Waiting - Emain Macha
		SetPropBehavior(0x00A0005000000005, PropWarp(80,3613,2465, 52,40810,29478));
		SetPropBehavior(0x00A000340001006E, PropWarp(52,40797,29115, 61,6696,7282));

		// Emain Macha Wedding Waiting - Emain Macha Wedding Ceremony
		SetPropBehavior(0x00A0005000000002, PropWarp(80,3624,4768, 81,2792,2452));
		SetPropBehavior(0x00A0005100010002, PropWarp(81,2795,2185, 80,3607,4484));

		// Emain Macha Wedding Waiting - Emain Macha Wedding Ceremony
		SetPropBehavior(0x00A0005000000003, PropWarp(80,2899,4696, 81,2162,2625));
		SetPropBehavior(0x00A0005100010003, PropWarp(81,3527,2251, 80,4321,4309));

		// Emain Macha Wedding Waiting - Emain Macha Wedding Ceremony
		SetPropBehavior(0x00A0005000000004, PropWarp(80,4346,4682, 81,3408,2600));
		SetPropBehavior(0x00A0005100010004, PropWarp(81,2087,2274, 80,3052,4295));

		// Bank
		SetPropBehavior(0x00A00034000002F7, PropWarp(52,36283,46351, 62,1237,1902));
		SetPropBehavior(0x00A0003E00010002, PropWarp(62,1064,1895, 52,36091,46366));

		// Advancement Test Waiting
		SetPropBehavior(0x00A00034001800AA, PropWarp(52,36497,70842, 212,2508,1262));
		SetPropBehavior(0x00A000D40002000F, PropWarp(212,2516,998, 52,36523,69836));

		// Abb Neagh Residential
		SetPropBehavior(0x00A0003400150009, PropWarp(52,25982,70155, 214,50776,32709));
		SetPropBehavior(0x00A000D60011009D, PropWarp(214,50861,32161, 52,25879,69414));

		// Rundal
		SetPropBehavior(0x00A00034000C005C, PropWarp(52,73196,36591, 64,5604,4291));
		SetPropBehavior(0x00A0004000000003, PropWarp(64,5599,3776, 52,73200,36074));
		
		// Rundal Hard
		SetPropBehavior(0x00A0007B00010003, PropWarp(123,5599,3776, 64,5601,9813));
		SetPropBehavior(0x00A0004000000016, (creature, prop) =>
		{
			if(creature.TotalLevel >= 250)
				creature.Warp(123, 5600, 4373);
			else
				Send.Notice(creature, "You need a cumulative level of at least 250.");
		});
	}
}
