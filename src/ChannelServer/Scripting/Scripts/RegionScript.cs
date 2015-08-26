// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// Simple base class for region scripts.
	/// </summary>
	public class RegionScript : GeneralScript
	{
		public override bool Init()
		{
			this.Load();
			this.LoadProperties();
			this.LoadWarps();
			this.LoadSpawns();
			this.LoadEvents();

			this.InitializeRegion();

			return true;
		}

		public virtual void InitializeRegion()
		{
		}

		public virtual void LoadWarps()
		{
		}

		public virtual void LoadSpawns()
		{
		}

		public virtual void LoadEvents()
		{
		}

		public virtual void LoadProperties()
		{
		}

		/// <summary>
		/// Sets property for given region.
		/// </summary>
		/// <param name="regionId"></param>
		/// <param name="propertyName"></param>
		/// <param name="value"></param>
		public void SetProperty(int regionId, string propertyName, object value)
		{
			var region = ChannelServer.Instance.World.GetRegion(regionId);
			if (region == null)
			{
				Log.Warning("RegionScript.SetProperty: Region '{0}' doesn't exist.", regionId);
				return;
			}

			region.Properties[propertyName] = value;
		}
	}
}
