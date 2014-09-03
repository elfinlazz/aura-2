// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Database;
using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Shared.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		public override void UnknownPacket(ChannelClient client, Packet packet)
		{
			base.UnknownPacket(client, packet);

			if (client.Controlling != null && client.Controlling.Region != null)
			{
				//Send.ServerMessage(client.Controlling, "Unknown action.");
				//Refresh?
			}
		}

		public override void Handle(ChannelClient client, Packet packet)
		{
			try
			{
				base.Handle(client, packet);
			}
			catch (SecurityViolationException ex)
			{
				// Simplest case, where an exception comes up directly
				this.HandleSecurityException(client, ex);
			}
			catch (AggregateException ex)
			{
				// Relatively complex case involving possibly multile exceptions (from tasks),
				// so we have to check out all of them.
				var unhandled = ex.Flatten().InnerExceptions.Where(e => !this.CheckInnerSecurityException(client, e)).ToList();

				if (unhandled.Count != 0)
					throw new AggregateException(unhandled);
			}
			catch (Exception ex)
			{
				// This handles cases like an exception being thrown in a ctor
				if (!this.CheckInnerSecurityException(client, ex))
				{
					// TODO: This might not be related to security,
					//   really log it in that table?
					ChannelServer.Instance.Database.LogSecurityIncident(client, IncidentSeverityLevel.Moderate, "Unhandled exception while processing packet: " + ex, null);
					throw;
				}
			}
		}

		/// <summary>
		/// Checks for SecurityViolationExceptions in inner exceptions.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		private bool CheckInnerSecurityException(ChannelClient client, Exception ex)
		{
			var secEx = ex as SecurityViolationException;
			if (secEx != null)
			{
				this.HandleSecurityException(client, secEx);
				return true;
			}
			else if (ex.InnerException != null)
			{
				return this.CheckInnerSecurityException(client, ex.InnerException);
			}

			return false;
		}

		/// <summary>
		/// Handles SecurityViolationException.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="ex"></param>
		private void HandleSecurityException(ChannelClient client, SecurityViolationException ex)
		{
			var accName = client.Account == null ? "<NULL>" : "'" + client.Account.Id + "'";
			var charName = client.Controlling == null ? "<NULL>" : "'" + client.Controlling.Name + "'";

			if (client.Account != null)
				ChannelServer.Instance.Database.LogSecurityIncident(client, ex.Level, ex.Message, ex.StackReport);

			ChannelServer.Instance.Events.OnSecurityViolation(new SecurityViolationEventArgs(client, ex.Level, ex.Message, ex.StackReport));

			client.Kill();

			Log.Warning("Client '{0}' : Account {1} (Controlling {2}) just committed a {3} offense. Incident report: {4}", client.Address, accName, charName, ex.Level, ex.Message);
		}
	}
}
