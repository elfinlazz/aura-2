// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Aura.Mabi.Structs;
using Aura.Mabi.Const;
using System.Threading;
using Aura.Data;
using Aura.Channel.Network;
using Aura.Shared.Util;
using Aura.Data.Database;

namespace Aura.Channel.World.Entities
{
	/// <remarks>
	/// Not all options are used in all props. Things like ExtraData, State,
	/// etc. are all very prop specific.
	/// </remarks>
	public class Prop : Entity
	{
		/// <summary>
		/// Base prop id that is increased for every new prop.
		/// </summary>
		/// <remarks>
		/// TODO: Given the way prop ids work, Mabi is limited to 32k/64k props
		///   per area. But since we use one global id Aura is actually limited
		///   to that amount globally. This could become a problem for a busy,
		///   highly customized server. (Custom props, Dungeons, SMs, etc.)
		/// </remarks>
		private static long _propId = MabiId.ServerProps;

		/// <summary>
		/// Returns entity data type "Prop".
		/// </summary>
		public override DataType DataType { get { return DataType.Prop; } }

		/// <summary>
		/// Marshable prop information used for packets.
		/// </summary>
		public PropInfo Info;

		/// <summary>
		/// Data about the prop from the db.
		/// </summary>
		public PropsDbData Data;

		/// <summary>
		/// True if this prop was spawned by the server.
		/// </summary>
		/// <remarks>
		/// *sigh* Yes, we're checking the id, happy now, devCAT? .__.
		/// </remarks>
		public bool ServerSide { get { return (this.EntityId >= MabiId.ServerProps); } }

		/// <summary>
		/// Returns true if prop is not server sided and has a state or extra data.
		/// </summary>
		public bool ModifiedClientSide { get { return !this.ServerSide && (!string.IsNullOrWhiteSpace(this.State) || this.HasXml); } }

		/// <summary>
		/// Called when a player interacts with the prop (touch, attack).
		/// </summary>
		public PropFunc Behavior { get; set; }

		/// <summary>
		/// Prop's name (only supported by specific props)
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Prop's title (only supported by specific props)
		/// </summary>
		public string Title { get; set; }

		public float _resource;
		/// <summary>
		/// Remaining resource amount
		/// </summary>
		public float Resource { get { return _resource; } set { _resource = Math2.Clamp(0, 100, value); } }

		/// <summary>
		/// Time at which something was collected from the prop last.
		/// </summary>
		public DateTime LastCollect { get; set; }

		/// <summary>
		/// Prop's state (only supported by specific props)
		/// </summary>
		/// <remarks>
		/// Some known states: single, closed, open, state1-3
		/// </remarks>
		public string State { get; set; }

		private XElement _xml;
		/// <summary>
		/// Additional options as XML.
		/// </summary>
		public XElement Xml { get { return _xml ?? (_xml = new XElement("xml")); } }

		/// <summary>
		/// True if prop has an XML element.
		/// </summary>
		public bool HasXml { get { return _xml != null; } }

		/// <summary>
		/// Gets or sets the prop's region, forwarding to Info.Region.
		/// </summary>
		public override int RegionId
		{
			get { return this.Info.Region; }
			set { this.Info.Region = value; }
		}

		/// <summary>
		/// New prop
		/// </summary>
		/// <param name="id"></param>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction"></param>
		/// <param name="scale"></param>
		/// <param name="altitude"></param>
		public Prop(int id, int region, int x, int y, float direction, float scale = 1f, float altitude = 0)
			: this("", "", "", id, region, x, y, direction, scale, altitude)
		{
		}

		/// <summary>
		/// New prop
		/// </summary>
		/// <param name="name"></param>
		/// <param name="title"></param>
		/// <param name="extra"></param>
		/// <param name="id"></param>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction"></param>
		/// <param name="scale"></param>
		/// <param name="altitude"></param>
		public Prop(string name, string title, string extra, int id, int region, int x, int y, float direction, float scale = 1, float altitude = 0)
			: this(0, name, title, id, region, x, y, direction, scale, altitude)
		{
			this.EntityId = Interlocked.Increment(ref _propId);
			this.EntityId += (long)region << 32;
			this.EntityId += AuraData.RegionInfoDb.GetAreaId(region, x, y) << 16;
		}

		/// <summary>
		/// New prop
		/// </summary>
		/// <param name="entityId"></param>
		/// <param name="name"></param>
		/// <param name="title"></param>
		/// <param name="id"></param>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction"></param>
		/// <param name="scale"></param>
		/// <param name="altitude"></param>
		public Prop(long entityId, string name, string title, int id, int region, int x, int y, float direction, float scale = 1, float altitude = 0)
		{
			this.EntityId = entityId;

			this.Name = name;
			this.Title = title;

			this.Info.Id = id;
			this.Info.Region = region;
			this.Info.X = x;
			this.Info.Y = y;
			this.Info.Direction = direction;
			this.Info.Scale = scale;

			this.Info.Color1 =
			this.Info.Color2 =
			this.Info.Color3 =
			this.Info.Color4 =
			this.Info.Color5 =
			this.Info.Color6 =
			this.Info.Color7 =
			this.Info.Color8 =
			this.Info.Color9 = 0xFF808080;

			_resource = 100;
			this.LastCollect = DateTime.Now;

			this.LoadDefault();
		}

		/// <summary>
		/// Loads prop data from db.
		/// </summary>
		private void LoadDefault()
		{
			if ((this.Data = AuraData.PropsDb.Find(this.Info.Id)) == null)
			{
				Log.Warning("Prop.LoadDefault: No data found for '{0}'.", this.Info.Id);
				return;
			}

			// Add state for wells, otherwise they aren't interactable
			// and you can walk through them o,o
			// TODO: Can we generalize this somehow?
			// They also have XML, but that doesn't seem to be needed.
			// <xml _RESOURCE="100.000000" _LAST_COLLECT_TIME="63559712423945"/>
			if (this.Data.HasTag("/water/well/"))
				this.State = "default";
		}

		/// <summary>
		/// Returns prop's static position (Info.X|Y).
		/// </summary>
		/// <returns></returns>
		public override Position GetPosition()
		{
			return new Position((int)this.Info.X, (int)this.Info.Y);
		}

		/// <summary>
		/// Returns information about the prop as string.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return string.Format("Prop: 0x{0}, Region: {1}, X: {2}, Y: {3}", this.EntityIdHex, this.Info.Region, this.Info.X, this.Info.Y);
		}

		/// <summary>
		/// Returns prop behavior for dropping.
		/// </summary>
		/// <param name="dropType"></param>
		/// <returns></returns>
		public static PropFunc GetDropBehavior(int dropType)
		{
			return (creature, prop) =>
			{
				if (RandomProvider.Get().NextDouble() > ChannelServer.Instance.Conf.World.PropDropChance)
					return;

				var dropInfo = AuraData.PropDropDb.Find(dropType);
				if (dropInfo == null)
				{
					Log.Warning("GetDropBehavior: Unknown prop drop type '{0}'.", dropType);
					return;
				}

				var rnd = RandomProvider.Get();

				// Get random item from potential drops
				var dropItemInfo = dropInfo.GetRndItem(rnd);
				var rndAmount = (dropItemInfo.Amount > 1 ? (ushort)rnd.Next(1, dropItemInfo.Amount) : (ushort)1);

				var item = new Item(dropItemInfo.ItemClass);
				item.Info.Amount = rndAmount;
				item.Drop(prop.Region, creature.GetPosition());
			};
		}

		/// <summary>
		/// Returns prop behavior for warping.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public static PropFunc GetWarpBehavior(int region, int x, int y)
		{
			return (creature, prop) =>
			{
				creature.Warp(region, x, y);
			};
		}

		/// <summary>
		///  Returns true if prop's data has the tag.
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public override bool HasTag(string tag)
		{
			if (this.Data == null)
				return false;

			return this.Data.HasTag(tag);
		}
	}

	public delegate void PropFunc(Creature creature, Prop prop);
}
