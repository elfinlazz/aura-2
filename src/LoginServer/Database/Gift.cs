// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Shared.Database;

namespace Aura.Login.Database
{
	public class Gift : Card
	{
		public string Message { get; set; }
		public string Sender { get; set; }
		public string SenderServer { get; set; }
		public string Receiver { get; set; }
		public string ReceiverServer { get; set; }

		public DateTime Added { get; set; }

		public bool IsCharacter
		{
			get { return (this.Race == 0); }
		}
	}
}
