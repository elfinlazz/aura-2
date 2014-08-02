// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Shared.Network;

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
				HandleSecurityException(client, ex);
			}
			catch (AggregateException ex)
			{
				// Relatively complex case involving possibly multile exceptions (from tasks)
				// So we have to check out all of them.
				var unhandled = ex.Flatten().InnerExceptions.Where(e => !CheckInnerSecurityException(client, e)).ToList();

				if (unhandled.Count != 0)
					throw new AggregateException(unhandled);
			}
			catch (Exception ex)
			{
				// This handles cases like an exception being thrown in a ctor
				if (!CheckInnerSecurityException(client, ex))
					throw;
			}
		}

		private bool CheckInnerSecurityException(ChannelClient client, Exception ex)
		{
			var secEx = ex as SecurityViolationException;
			if (secEx != null)
			{
				HandleSecurityException(client, secEx);
				return true;
			}
			else
			{
				if (ex.InnerException != null)
					return CheckInnerSecurityException(client, ex.InnerException);
			}

			return false;
		}

		private void HandleSecurityException(ChannelClient client, SecurityViolationException ex)
		{
			// TODO: Autoban
		}
	}
}
