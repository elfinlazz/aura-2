using Aura.Channel.World.Dungeons.Generation;
using Aura.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ChannelServer.Tests
{
	public class DungeonGeneratorTest
	{
		public class DungeonLayouts
		{
			public DungeonLayouts()
			{
				AuraData.DungeonBlocksDb.Load("../../../../system/db/dungeon_blocks.txt", true);
				AuraData.DungeonDb.Load("../../../../system/db/dungeons.txt", true);
			}

			[Fact]
			public void TirCho_Alby_DropTest_Dungeon()
			{
				var dg = new DungeonGenerator("TirCho_Alby_DropTest_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05             
04             
03       B   X 
02     X X X X 
01     X X S   
00     X X     
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void Emain_Runda_Dungeon_G9_HolyKnights1()
			{
				var dg = new DungeonGenerator("Emain_Runda_Dungeon_G9_HolyKnights1", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
08 X X X X X X X X X 
07 X X X X X X X   X 
06 X X X X X X X S X 
05 X X X     X X   X 
04   X       X X X X 
03   X   B   X X X X 
02   X X X X X X X X 
01   X X X X X X X X 
00   X X X X X X X X 
maze gen
  000102030405060708
05     X       
04   X X       
03 X X X   B   
02 X X X   X X 
01 X X X S X X 
00 X X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void Gairech_Fiodh_Dungeon_Recover()
			{
				var dg = new DungeonGenerator("Gairech_Fiodh_Dungeon_Recover", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
07 X     X X X X X 
06 X X X X X X X X 
05 X X X X X X X X 
04 X X X X   X X X 
03 X X X       X   
02 X X X   B   X X 
01 X X X X X S X X 
00 X X X X     X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void Dunbarton_Rabbie_High_2_Dungeon()
			{
				var dg = new DungeonGenerator("Dunbarton_Rabbie_High_2_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
07 X X S X X X X X 
06 X     X X X X X 
05 X X   X X X X X 
04   X X X X X X X 
03   X X X X X X X 
02   X X X X X X X 
01 B X X X X X X X 
00 X X X X X X X X 
maze gen
  000102030405060708
07   X X S X X X X 
06 X X X   X X X X 
05 X X X X X X X X 
04 X X X X X X X X 
03 X X       X X X 
02 X X   B X X X X 
01 X X X X X X X X 
00 X X X X X X X X 
maze gen
  000102030405060708
07 X S   X X X X X 
06 X X X X X X X X 
05 X   X X X X X X 
04       X X X X X 
03   B   X   X X X 
02 X X X X X X X X 
01 X X X   X X X X 
00 X X X X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void TirCho_Ciar_Low_Hardmode_Dungeon()
			{
				var dg = new DungeonGenerator("TirCho_Ciar_Low_Hardmode_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05 X S         
04 X   X X X X 
03 X X X X X X 
02 X X X X X X 
01 X X X X X   
00 X X X X B   
maze gen
  000102030405060708
06 X X           
05 X X X         
04 X X X X     S 
03 X X X X     X 
02 X X X X B X X 
01 X X X     X X 
00     X X X X   
maze gen
  000102030405060708
07   X X X X X X   
06   X X X   X X   
05   X X X   X X X 
04 B X     X X X X 
03 X X X X X X X X 
02 X X X   X X X X 
01 X X X S X X X   
00 X X X X X X     
maze gen
  000102030405060708
07 X X X X X X X   
06 X X X X X X X   
05 X X X X X X X   
04 X X X X X X X X 
03 X X X X   X X X 
02 X X X       X   
01     S   B   X   
00       X X X X   
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void JG_Neko_Dungeon3()
			{
				var dg = new DungeonGenerator("JG_Neko_Dungeon3", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
13 X X X X X                   
12 X X X X X     X X X X       
11 X S X X X X X X X X X       
10 X   X X X X X X X X X X     
09 X X X X X X X X X X X X     
08 X X X X X X X X X X X   X   
07 X X X X X X X X X X     X X 
06     X X X X X X X     X X X 
05       X X X X X X     X X X 
04         X     X   X X X     
03         X B   X   X         
02               X X X         
01               X X           
00                             
maze gen
  000102030405060708
06               
05               
04               
03           X X 
02         X X S 
01   B     X X   
00 X X X X X X   
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void Emain_Runda_Low_Dungeon()
			{
				var dg = new DungeonGenerator("Emain_Runda_Low_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
07 X X S X X X X X 
06 X X   X X X X X 
05 X X   X X X X X 
04 X X X X X X X X 
03 X X X X X X X   
02 X X X X X X X   
01   X       X X X 
00 B X           X 
maze gen
  000102030405060708
06 X X X X X X X   
05 X X X X   X X X 
04 X X   X X X B X 
03 X X X X X X   X 
02 X X X X X X X X 
01   S X X X X X X 
00     X X X X X X 
maze gen
  000102030405060708
06 X X X X X X X   
05 X X X X     X   
04 X X X X     X X 
03 X X X       X X 
02 X X X   B   X   
01 X X X X X   X S 
00 X X X X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void Bangor_Barripcbang_High_Dungeon()
			{
				var dg = new DungeonGenerator("Bangor_Barripcbang_High_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05       X X   
04   X X X X X 
03 X X B     X 
02 X   X X X X 
01 S X X X X X 
00   X X X     
maze gen
  000102030405060708
05   X X       
04 X X X X X B 
03 X   S X X   
02 X X X X X   
01 X X X X X   
00 X X X X X   
maze gen
  000102030405060708
06 X X X X X X 
05 X     X X X 
04 X       X X 
03 X   B   X X 
02 X   X X X X 
01 X X X X X X 
00 X X   S X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void SAO_TirCho_Alby_5()
			{
				var dg = new DungeonGenerator("SAO_TirCho_Alby_5", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05       X X   
04 X X X X X X 
03 X X X     X 
02 X   X     X 
01 X S     B X 
00             
maze gen
  000102030405060708
05 X X X X     
04 X B X X     
03 X   X S     
02 X X X       
01   X X       
00     X X X   
maze gen
  000102030405060708
05         X B 
04 X X   X X   
03 X X X X     
02 X X X X X   
01   X X       
00     X S     
maze gen
  000102030405060708
05         X   
04 B X   X X   
03   X X X     
02     X X     
01 X X X X S   
00 X X X X     
maze gen
  000102030405060708
05 S X X X X   
04   X X X X   
03   X X X     
02   X X       
01     X   B   
00     X X X   
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void SAO_Gairech_Fiodh_5()
			{
				var dg = new DungeonGenerator("SAO_Gairech_Fiodh_5", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05 X X X X X   
04 X X X X X   
03 X X X   X S 
02 X X X B X   
01 X X X   X X 
00 X X X X X X 
maze gen
  000102030405060708
05     X X B   
04 S X X X     
03 X X X X X X 
02 X   X X X X 
01 X X X X X X 
00 X X X X X X 
maze gen
  000102030405060708
05 X X X X X X 
04 X X X X X   
03 X X X X X X 
02   S X X X X 
01 X X X X B   
00   X X X     
maze gen
  000102030405060708
05 X X X       
04 X   X X X S 
03 X B X X X X 
02     X X X X 
01 X X X X X X 
00 X X X X X X 
maze gen
  000102030405060708
05 X X X X X X 
04 X   X X   X 
03 X X X       
02 X X X   B   
01 S X X   X X 
00     X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void Bangor_Barri_Dungeon_G9S1_normal()
			{
				var dg = new DungeonGenerator("Bangor_Barri_Dungeon_G9S1_normal", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05 X X X X X B 
04 X X X X     
03 X S X X X   
02 X   X X X X 
01 X X X X   X 
00   X X X     
maze gen
  000102030405060708
06         X X   
05         X X   
04 X X X X X X   
03 X X   X X X X 
02 X X X X   X X 
01 X     X X X X 
00 B           S 
maze gen
  000102030405060708
06 X X X X X X   
05 X X X X X X X 
04 X X X X X X X 
03 X X X X X X X 
02     S X X X   
01       X B     
00               
maze gen
  000102030405060708
06 X X X X       
05 X X X X X     
04 X     X X X X 
03 X X B X X X X 
02 X X X X X X X 
01 X X X X X   S 
00   X X X X X X 
maze gen
  000102030405060708
06 X X X X X X   
05 X X X X X     
04 X X X X X X X 
03 X X     X X X 
02 X       X X X 
01 S   B   X X X 
00     X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void G3_16_Tirnanog_Baol_Dungeon()
			{
				var dg = new DungeonGenerator("G3_16_Tirnanog_Baol_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
07 X X X X X X X 
06 S X X X X X X 
05   X X X X X X 
04 X X X X X X X 
03 X X X X X X X 
02 X X X X X X   
01 X X X X X B   
00     X X X X X 
maze gen
  000102030405060708
07 X X X X X X X 
06 X B X X X X X 
05 X   X X X X X 
04 X X X X X X X 
03 X X X   X X X 
02   X X X X X X 
01   X X X X X X 
00   S X X X X X 
maze gen
  000102030405060708
07 X X X X   X X 
06 X X X X X X B 
05 X X X X X X   
04 X X X X X X X 
03 X X X X   X X 
02 X   X X X X X 
01 X S X X X X X 
00 X X X X X X   
maze gen
  000102030405060708
07 X X X X X X X 
06 X X X X X X X 
05   X X X X X X 
04   X X X X X X 
03   X X   X X X 
02 X X X S   B X 
01 X X X X X X X 
00 X X X X X X X 
maze gen
  000102030405060708
06 X X X X X X X 
05 X X X X X X X 
04 X X X   X X X 
03 X X X     S   
02 X X       X X 
01 X X   B   X X 
00 X X X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void Senmag_Peaca_Middle_Dungeon()
			{
				var dg = new DungeonGenerator("Senmag_Peaca_Middle_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
06 S X X X X X X 
05   X X X X X X 
04 X X X X X X X 
03 X X X X X X X 
02 X X X X X X   
01 X X   B X X   
00 X X X X X X   
maze gen
  000102030405060708
06 X X X X X X X 
05 X X X X X X X 
04 X X X X X X X 
03 X   X X X S   
02 X X X X X B X 
01 X X   X X   X 
00 X X   X X X X 
maze gen
  000102030405060708
06 X X B X X X   
05 X X   X X X X 
04 X X X X   X X 
03 X X X X S X X 
02 X X X X X X X 
01   X X X X X X 
00   X X X X X X 
maze gen
  000102030405060708
06 X X X X X X   
05 X X X X X X   
04 X X X X   X   
03 X X X X S X X 
02 X X X X X X X 
01 X X X X X X X 
00 X B X X X X X 
maze gen
  000102030405060708
06 X X X X X X   
05 X X X   X X X 
04 X X X S X X X 
03 X X     X X X 
02 X       X X X 
01 X   B   X X X 
00 X X X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void G1_39_Tirnanog_Dungeon()
			{
				var dg = new DungeonGenerator("G1_39_Tirnanog_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
06 X X X S       
05 X X X         
04 X X X X       
03 X X X X B X   
02 X X X X X X   
01 X X X X X X   
00 X X X X X X   
maze gen
  000102030405060708
07           X X X 
06       B   X X X 
05 S X X X X X X X 
04   X X X X X X X 
03   X X X X X X X 
02     X X X X X X 
01     X X X X X X 
00   X X X X X X   
maze gen
  000102030405060708
06   X X         
05   B X         
04 X X X         
03 X X X X X     
02 X X X X X X   
01 X   S X X X X 
00 X X X X X   X 
maze gen
  000102030405060708
06 X S       B   
05 X X       X   
04 X X X     X X 
03 X X X X X X X 
02   X X X X X   
01 X X   X X X X 
00 X X X X X X X 
maze gen
  000102030405060708
06         X X X 
05 X S   X X   X 
04 X   X X X X X 
03 X X X X X     
02 X X X X X X X 
01 X X X X   X X 
00 X X       B X 
maze gen
  000102030405060708
08 X X     X X X X X 
07 X X X     X X X X 
06 X   X         X X 
05 X X X X       X X 
04 X X X X   B   X X 
03 S   X X X X   X X 
02 X X X X X X X X X 
01 X X X X X X X X X 
00 X X X X X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void TirCho_Alby_G15_Contract_of_Shylock()
			{
				var dg = new DungeonGenerator("TirCho_Alby_G15_Contract_of_Shylock", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05 S X X X B   
04   X   X X   
03   X   X X   
02   X X X X   
01             
00             
maze gen
  000102030405060708
05       B X   
04       X X   
03       X X X 
02       X X X 
01         S X 
00       X X X 
maze gen
  000102030405060708
05             
04 S           
03 X X X       
02 X X X X X X 
01 X X X X     
00 B           
maze gen
  000102030405060708
05   X X X     
04   X X X     
03     S X X   
02     X X X B 
01       X X   
00             
maze gen
  000102030405060708
05 X S X       
04 X   X X     
03 X   X X     
02 X   X       
01 X B X       
00 X X X       
maze gen
  000102030405060708
05         X X 
04         S X 
03           X 
02       X X X 
01 X X X X X X 
00 B   X X X X 
maze gen
  000102030405060708
05             
04             
03       X     
02   B   X     
01 X X X X S   
00   X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void Bangor_Barri_Low_Dungeon()
			{
				var dg = new DungeonGenerator("Bangor_Barri_Low_Dungeon", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05 X X X X X B 
04 X X X X     
03 X S X X X   
02 X   X X X X 
01 X X X X   X 
00   X X X     
maze gen
  000102030405060708
06         X X   
05         X X   
04 X X X X X X   
03 X X   X X X X 
02 X X X X   X X 
01 X     X X X X 
00 B           S 
maze gen
  000102030405060708
06 X X X X X X   
05 X X X X X X X 
04 X X X X X X X 
03 X X X X X X X 
02     S X X X   
01       X B     
00               
maze gen
  000102030405060708
06 X X X X       
05 X X X X X     
04 X     X X X X 
03 X X B X X X X 
02 X X X X X X X 
01 X X X X X   S 
00   X X X X X X 
maze gen
  000102030405060708
06 X X X X X X   
05 X X X X X     
04 X X X X X X X 
03 X X     X X X 
02 X       X X X 
01 S   B   X X X 
00     X X X X X 
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			[Fact]
			public void TirCho_Alby_G15_Price_Of_Love_High()
			{
				var dg = new DungeonGenerator("TirCho_Alby_G15_Price_Of_Love_High", 2000, 370545889, 0, "");

				var correct = @"
maze gen
  000102030405060708
05 S X X X B   
04   X   X X   
03   X   X X   
02   X X X X   
01             
00             
maze gen
  000102030405060708
05       B X   
04       X X   
03       X X X 
02       X X X 
01         S X 
00       X X X 
maze gen
  000102030405060708
05             
04 S           
03 X X X       
02 X X X X X X 
01 X X X X     
00 B           
maze gen
  000102030405060708
05   X X X     
04   X X X     
03     S X X   
02     X X X B 
01       X X   
00             
maze gen
  000102030405060708
05 X S X       
04 X   X X     
03 X   X X     
02 X   X       
01 X B X       
00 X X X       
maze gen
  000102030405060708
05         X X 
04         S X 
03           X 
02       X X X 
01 X X X X X X 
00 B   X X X X 
maze gen
  000102030405060708
05             
04             
03       X     
02   B   X     
01 X X X X S   
00   X X X X X     
".Trim();

				Assert.Equal(correct, GetMazeAsString(dg));
			}

			static string GetMazeAsString(DungeonGenerator dungeon)
			{
				var sw = new System.IO.StringWriter();

				for (int floorN = 0; floorN < dungeon.Floors.Count; ++floorN)
				{
					var floor = dungeon.Floors[floorN];
					var rooms = floor.MazeGenerator.Rooms;

					sw.WriteLine("maze gen");
					sw.WriteLine("  000102030405060708");
					for (int y = floor.MazeGenerator.Height - 1; y >= 0; --y)
					{
						var row = string.Format("{0,0:D2} ", y);
						for (int x = 0; x < floor.MazeGenerator.Width; ++x)
						{
							if (floor.MazeGenerator.StartPos.X == x && floor.MazeGenerator.StartPos.Y == y)
								row += "S ";
							else if (floor.MazeGenerator.EndPos.X == x && floor.MazeGenerator.EndPos.Y == y)
								row += "B ";
							else if (floor.MazeGenerator.Rooms[x][y].Visited)
								row += "X ";
							else
								row += "  ";
						}
						sw.WriteLine(row);
					}
				}

				var result = sw.ToString();
				sw.Close();
				return result.Trim();
			}
		}
	}
}
