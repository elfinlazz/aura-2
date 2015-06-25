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
using Aura.Channel.Network.Sending;
using System.Drawing;
using Aura.Mabi;

namespace Aura.Channel.World.Entities
{
	/// <remarks>
	/// Not all options are used in all props. Things like ExtraData, State,
	/// etc. are all very prop specific.
	/// </remarks>
	public class Prop : Entity, IShapedEntity
	{
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
		/// Temporary variables for this prop
		/// </summary>
		public PropTemp Temp { get; private set; }

		/// <summary>
		/// List of shapes for the prop (collision).
		/// </summary>
		public List<Point[]> Shapes { get; protected set; }

		/// <summary>
		/// Specifies whether other entities collide with this one's shape.
		/// </summary>
		public bool IsCollision { get { return true; } }

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
		/// Prop's ident? Name?
		/// </summary>
		/// <remarks>
		/// It's not clear what this is used for, dungeon portals specify
		/// "_upstairs" and "_downstairs", but what's displayed above them
		/// is the title, same with Guild Stones.
		/// </remarks>
		public string Ident { get; set; }

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
		/// List of extensions (TODO: needs research).
		/// </summary>
		public List<PropExtension> Extensions { get; private set; }

		/// <summary>
		/// Creates new prop with a newly generated entity id.
		/// </summary>
		/// <param name="id"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction"></param>
		/// <param name="scale"></param>
		/// <param name="altitude"></param>
		/// <param name="ident"></param>
		/// <param name="title"></param>
		public Prop(int id, int regionId, int x, int y, float direction, float scale = 1f, float altitude = 0, string state = "", string ident = "", string title = "")
			: this(0, id, regionId, x, y, direction, scale, altitude, state, ident, title)
		{
		}

		/// <summary>
		/// Creates new prop with given entity id.
		/// </summary>
		/// <param name="entityId"></param>
		/// <param name="id"></param>
		/// <param name="regionId"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="direction"></param>
		/// <param name="scale"></param>
		/// <param name="altitude"></param>
		/// <param name="state"></param>
		/// <param name="ident"></param>
		/// <param name="title"></param>
		public Prop(long entityId, int id, int regionId, int x, int y, float direction, float scale, float altitude, string state, string ident, string title)
		{
			this.Shapes = new List<Point[]>();
			this.Temp = new PropTemp();
			this.Extensions = new List<PropExtension>();

			_resource = 100;

			this.EntityId = entityId;
			this.Ident = ident;
			this.Title = title;
			this.Info.Id = id;
			this.Info.Region = regionId;
			this.Info.X = x;
			this.Info.Y = y;
			this.Info.Direction = direction;
			this.Info.Scale = scale;
			this.LastCollect = DateTime.Now;

			this.Info.Color1 =
			this.Info.Color2 =
			this.Info.Color3 =
			this.Info.Color4 =
			this.Info.Color5 =
			this.Info.Color6 =
			this.Info.Color7 =
			this.Info.Color8 =
			this.Info.Color9 = 0xFF808080;

			this.State = state;
			this.UpdateShapes();

			// Load prop data
			if ((this.Data = AuraData.PropsDb.Find(this.Info.Id)) == null)
				Log.Warning("Prop: No data found for '{0}'.", this.Info.Id);
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

		/// <summary>
		/// Removes prop from its current region.
		/// </summary>
		public override void Disappear()
		{
			if (this.Region != Region.Limbo)
				this.Region.RemoveProp(this);

			base.Disappear();
		}

		/// <summary>
		/// Sets prop's state and broadcasts update.
		/// </summary>
		/// <param name="state"></param>
		public void SetState(string state)
		{
			this.State = state;
			this.UpdateCollisions();
			if (this.Region != Region.Limbo)
				Send.PropUpdate(this);
		}

		/// <summary>
		///	Adds new extention and broadcast update.
		/// </summary>
		/// <param name="ext"></param>
		public void AddExtension(PropExtension ext)
		{
			this.Extensions.Add(ext);
			Send.AddPropExtension(this, ext);
		}

		/// <summary>
		///	Removes all extensions from prop and broadcast update.
		/// </summary>
		/// <param name="ext"></param>
		public void RemoveAllExtensions()
		{
			foreach (var ext in this.Extensions)
				Send.RemovePropExtension(this, ext);
			this.Extensions.Clear();
		}

		/// <summary>
		/// Updates shapes for current state from defaults db.
		/// </summary>
		public void UpdateShapes()
		{
			this.Shapes.Clear();

			var defaultsList = AuraData.PropDefaultsDb.Find(this.Info.Id);
			if (defaultsList != null && defaultsList.Count != 0)
			{
				var def = string.IsNullOrWhiteSpace(this.State)
					? defaultsList.First()
					: defaultsList.FirstOrDefault(a => a.State == this.State);

				if (def == null)
					Log.Warning("Prop.UpdateShapes: No defaults found for state '{0}' and prop '{1}'.", this.State, this.Info.Id);
				else
				{
					this.State = def.State;

					foreach (var shape in def.Shapes)
						this.Shapes.Add(shape.GetPoints(this.Info.Direction, (int)this.Info.X, (int)this.Info.Y));
				}
			}
		}

		/// <summary>
		/// Updates prop's shapes and collisions in current region.
		/// </summary>
		private void UpdateCollisions()
		{
			this.UpdateShapes();

			if (this.Region != Region.Limbo)
			{
				this.Region.Collisions.Remove(this.EntityId);
				this.Region.Collisions.Add(this);
			}
		}

		/// <summary>
		/// Returns targetable creatures in given range of prop.
		/// Owner of the prop and creatures he can't target are excluded.
		/// </summary>
		/// <param name="owner">Owner of the prop, who is excluded and used as reference for CanTarget (null to ignore).</param>
		/// <param name="range"></param>
		/// <returns></returns>
		public ICollection<Creature> GetTargetableCreaturesInRange(Creature owner, int range)
		{
			var pos = this.GetPosition();
			var targetable = this.Region.GetCreatures(target =>
			{
				var targetPos = target.GetPosition();

				return target != owner // Exclude owner
					&& (owner == null || owner.CanTarget(target)) // Check targetability
					&& !target.IsDead // Check if target's alive (in case owner is null)
					&& !target.Has(CreatureStates.GoodNpc) // Don't hit good NPCs (in case owner is null)
					&& targetPos.InRange(pos, range) // Check range
					&& !this.Region.Collisions.Any(pos, targetPos) // Check collisions between entities
					&& !target.Conditions.Has(ConditionsA.Invisible); // Check visiblility (GM)
			});

			return targetable;
		}
	}

	public delegate void PropFunc(Creature creature, Prop prop);

	/// <summary>
	/// Temporary prop variables
	/// </summary>
	public class PropTemp
	{
		public SkillRankData CampfireSkillRank;
		public ItemData CampfireFirewood;
	}
}
