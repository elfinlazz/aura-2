using Aura.Mabi;
using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Entities
{
	public class PropExtension
	{
		//010 [........000000CA] Int    : 202
		//011 [........0000044C] Int    : 1100
		//012 [................] String : 
		//013 [..............02] Byte   : 2
		//014 [................] String : message:s:Do you wish to enter the room?;condition:s:notin(220189,194241,1354);

		//010 [........000000CA] Int    : 202
		//011 [........0000044C] Int    : 1100
		//012 [................] String : 
		//013 [..............02] Byte   : 2
		//014 [................] String : condition:s:haskey(chest);message:s:Do you wish to open this chest?;
		//015 [............0000] Short  : 0

		//011 [........000000CA] Int    : 202
		//012 [........00000064] Int    : 100
		//013 [................] String : GotoLobby
		//014 [..............02] Byte   : 2
		//015 [................] String : globalname:s:Uladh_Dungeon_Beginners_Hall1/_Uladh_Dungeon_Beginners_Hall1/Dungeon_Beginners_Outer_Spawn;

		// 010 [........000000CA] Int    : 202
		// 011 [........00000064] Int    : 100
		// 012 [................] String : Gate_Start_RDungeon_10003
		// 013 [..............02] Byte   : 2
		// 014 [................] String : globalname:s:RDungeon_10002/_Uladh_Dungeon_Black_Wolfs_Hall2/Indoor_RDungeon_10002_EB;
		// 015 [............0000] Short  : 0

		//016 [........000000CA] Int    : 202
		//017 [........0000044C] Int    : 1100
		//018 [................] String : Doyouwant
		//019 [..............02] Byte   : 2
		//020 [................] String : message:s:_LT[code.standard.msg.dungeon_exit_notice_msg];title:s:_LT[code.standard.msg.dungeon_exit_notice_title];

		// 010 [........000000CA] Int    : 202
		// 011 [........00000A28] Int    : 2600
		// 012 [................] String : SafeZone_RDungeon_10032
		// 013 [..............02] Byte   : 2
		// 014 [................] String : globalname:s:RDungeon_10032/10032_2_2/Start_RDungeon_10032_EB;

		// 1100 - Ok/Cancel confirmation
		// message: MsgBox content
		// [title]: MsgBox title
		// [condition]: Condition for the msg to appear?
		//     Examples:
		//     - haskey(chest): Has a key with the meta data? Isn't actually checked by the client?
		//     - notin(1,2): Checks if not on dungeon tile x,y.
		//     - notin(220189,194241,1354): Checks if not in radius (?) of x,y.

		// 100 - Warp? Maybe the behavior.
		// globalname: Location path

		// 2600 - Save statue, marks save point?
		// globalname: Location path

		public SignalType SignalType { get; protected set; }
		public EventType EventType { get; protected set; }
		public string Name { get; protected set; }
		public byte Mode { get; protected set; }
		public MabiDictionary Value { get; protected set; }

		public PropExtension(SignalType signalType, EventType eventType, string name, byte mode)
		{
			this.SignalType = signalType;
			this.EventType = eventType;
			this.Name = name;
			this.Mode = mode;
			this.Value = new MabiDictionary();
		}
	}

	/// <summary>
	/// Extension for props that shows a confirmation message before actually
	/// touching the prop, running its behavior.
	/// </summary>
	public class ConfirmationPropExtension : PropExtension
	{
		public ConfirmationPropExtension(string name, string message, string title = null, string condition = null)
			: base(SignalType.Touch, EventType.Confirmation, name, 2)
		{
			this.Value.SetString("message", message);

			if (title != null)
				this.Value.SetString("title", title);

			if (condition != null)
				this.Value.SetString("condition", condition);
		}
	}
}
