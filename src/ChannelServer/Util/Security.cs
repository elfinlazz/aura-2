// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network;
using Aura.Shared.Util;
using System;
using System.Diagnostics;
using System.Linq;

namespace Aura.Channel.Util
{
	public sealed class SecurityViolationEventArgs : EventArgs
	{
		public ChannelClient Client { get; private set; }
		public string Report { get; private set; }
		public string StackReport { get; private set; }
		public IncidentSeverityLevel Level { get; private set; }

		public SecurityViolationEventArgs(ChannelClient offender, IncidentSeverityLevel level, string report, string stacktrace)
		{
			this.Client = offender;
			this.Level = level;
			this.Report = report;
			this.StackReport = stacktrace;
		}
	}

	/// <summary>
	/// The base for the exceptions to throw when someone does something bad
	/// </summary>
	public abstract class SecurityViolationException : Exception
	{
		private const int _stackDepth = 5;

		private readonly string _message;

		/// <summary>
		/// Details on what happened
		/// </summary>
		public override string Message { get { return _message; } }

		/// <summary>
		/// A short stack trace to identify where the incident occured.
		/// </summary>
		public string StackReport { get; private set; }

		/// <summary>
		/// How bad it was
		/// </summary>
		public IncidentSeverityLevel Level { get; private set; }

		protected SecurityViolationException(IncidentSeverityLevel lvl, string report)
		{
			this.Level = lvl;

			var stacktrace = new StackTrace(2); // Skip 2 frames for this and calling ctor

			this.StackReport = string.Join(" --> ",
				stacktrace.GetFrames()
				.Take(_stackDepth)
				.Reverse()
				.Select(n => n.GetMethod().DeclaringType.Name + "." + n.GetMethod().Name));

			_message = string.Format("{0}: {1}", stacktrace.GetFrame(0).GetMethod().Name, report);
		}
	}

	/// <summary>
	/// Used to indicate something suspicious happened, but it
	/// *could* be nothing. This setting should be rarely used...
	/// </summary>
	public sealed class MildViolation : SecurityViolationException
	{
		public MildViolation(string report, params object[] args)
			: base(IncidentSeverityLevel.Mild, string.Format(report, args))
		{
		}
	}

	/// <summary>
	/// Something that is a strong indicator for a hack, but not certain.
	/// </summary>
	public sealed class ModerateViolation : SecurityViolationException
	{
		public ModerateViolation(string report, params object[] args)
			: base(IncidentSeverityLevel.Moderate, string.Format(report, args))
		{
		}
	}

	/// <summary>
	/// Something happened that could really only be caused by a hack tool
	/// </summary>
	public sealed class SevereViolation : SecurityViolationException
	{
		public SevereViolation(string report, params object[] args)
			: base(IncidentSeverityLevel.Severe, string.Format(report, args))
		{
		}
	}

	public enum IncidentSeverityLevel
	{
		Mild = 1,
		Moderate,
		Severe
	}

	public enum AutobanLengthIncrease
	{
		None,
		Linear,
		Exponential
	}

	public static class Autoban
	{
		/// <summary>
		/// Logs incident, increases ban points, and bans account if appropriate.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="level"></param>
		/// <param name="report"></param>
		/// <param name="stacktrace"></param>
		public static void Incident(ChannelClient client, IncidentSeverityLevel level, string report, string stacktrace = null)
		{
			if (client.Account == null)
				return;

			switch (level)
			{
				case IncidentSeverityLevel.Mild: client.Account.AutobanScore += ChannelServer.Instance.Conf.Autoban.MildAmount; break;
				case IncidentSeverityLevel.Moderate: client.Account.AutobanScore += ChannelServer.Instance.Conf.Autoban.ModerateAmount; break;
				case IncidentSeverityLevel.Severe: client.Account.AutobanScore += ChannelServer.Instance.Conf.Autoban.SevereAmount; break;
				default:
					Log.Warning("Autoban.Incident: Unknown severity level {0}", level);
					goto case IncidentSeverityLevel.Mild;
			}

			Log.Info("Account '{0}' total ban score: {1}", client.Account.Id, client.Account.AutobanScore);

			client.Account.LastAutobanReduction = DateTime.Now;

			if (client.Account.AutobanScore >= ChannelServer.Instance.Conf.Autoban.BanAt)
				Ban(client);
		}

		/// <summary>
		/// Bans account, length depends on ban points and previous bans.
		/// </summary>
		/// <param name="client"></param>
		private static void Ban(ChannelClient client)
		{
			var autobanCount = ++client.Account.AutobanCount;

			TimeSpan banLength;

			switch (ChannelServer.Instance.Conf.Autoban.LengthIncrease)
			{
				case AutobanLengthIncrease.None:
					banLength = ChannelServer.Instance.Conf.Autoban.InitialBanTime;
					break;

				case AutobanLengthIncrease.Linear:
					banLength = TimeSpan.FromMinutes((long)(ChannelServer.Instance.Conf.Autoban.InitialBanTime.TotalMinutes * autobanCount));
					break;

				case AutobanLengthIncrease.Exponential:
					banLength = TimeSpan.FromMinutes(ChannelServer.Instance.Conf.Autoban.InitialBanTime.TotalMinutes * (long)Math.Pow(autobanCount, 2));
					break;

				default:
					Log.Warning("Unknown AutobanLengthIncrease: {0}", ChannelServer.Instance.Conf.Autoban.LengthIncrease);
					goto case AutobanLengthIncrease.Exponential;
			}

			Log.Info("Autobanning account '{0}'. Total times they've been autobanned: {1}. Length of this ban: {2}", client.Account.Id, autobanCount, banLength);

			client.Account.BanExpiration = DateTime.Now + banLength;
			client.Account.BanReason = "Automatic ban triggered.";

			// So their score doesn't decrease while they're banned.
			client.Account.LastAutobanReduction = client.Account.BanExpiration;

			if (ChannelServer.Instance.Conf.Autoban.ResetScoreOnBan)
				client.Account.AutobanScore = 0;
		}
	}
}
