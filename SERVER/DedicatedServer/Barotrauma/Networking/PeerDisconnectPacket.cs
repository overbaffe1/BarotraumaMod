using System;
using System.Runtime.CompilerServices;
using System.Text;
using Steamworks;

namespace Barotrauma.Networking
{
	// Token: 0x020003C3 RID: 963
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(103)]
	internal readonly struct PeerDisconnectPacket : INetSerializableStruct
	{
		// Token: 0x060037C6 RID: 14278 RVA: 0x00176418 File Offset: 0x00174618
		private PeerDisconnectPacket(DisconnectReason disconnectReason, string additionalInformation = "")
		{
			this.DisconnectReason = disconnectReason;
			this.AdditionalInformation = additionalInformation;
		}

		// Token: 0x060037C7 RID: 14279 RVA: 0x00176428 File Offset: 0x00174628
		public LocalizedString ChatMessage([Nullable(2)] string name)
		{
			if (string.IsNullOrEmpty(name))
			{
				name = TextManager.Get("ServerMessage.UnknownClient").Value;
			}
			LocalizedString localizedString;
			switch (this.DisconnectReason)
			{
			case DisconnectReason.Disconnected:
				localizedString = TextManager.GetWithVariable("ServerMessage.ClientLeftServer", "[client]", name, FormatCapitals.No);
				break;
			case DisconnectReason.Banned:
				localizedString = TextManager.GetWithVariable("servermessage.bannedfromserver", "[client]", name, FormatCapitals.No);
				break;
			case DisconnectReason.Kicked:
				localizedString = TextManager.GetWithVariable("servermessage.kickedfromserver", "[client]", name, FormatCapitals.No);
				break;
			default:
			{
				string tag = "ChatMsg.DisconnectedWithReason";
				ValueTuple<string, LocalizedString>[] array = new ValueTuple<string, LocalizedString>[2];
				array[0] = new ValueTuple<string, LocalizedString>("[client]", name);
				int num = 1;
				string item = "[reason]";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ChatMsg.DisconnectReason.");
				defaultInterpolatedStringHandler.AppendFormatted<DisconnectReason>(this.DisconnectReason);
				array[num] = new ValueTuple<string, LocalizedString>(item, TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()));
				localizedString = TextManager.GetWithVariables(tag, array);
				break;
			}
			}
			LocalizedString message = localizedString;
			bool flag = !string.IsNullOrEmpty(this.AdditionalInformation);
			bool flag2 = flag;
			if (flag2)
			{
				DisconnectReason disconnectReason = this.DisconnectReason;
				bool flag3 = disconnectReason - DisconnectReason.Banned <= 1;
				flag2 = flag3;
			}
			if (flag2)
			{
				message += " " + TextManager.Get("banreason") + " " + TextManager.GetServerMessage(this.AdditionalInformation);
			}
			return message;
		}

		// Token: 0x17000F4A RID: 3914
		// (get) Token: 0x060037C8 RID: 14280 RVA: 0x001765A0 File Offset: 0x001747A0
		private LocalizedString MsgWithReason
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler.AppendLiteral("DisconnectReason.");
				defaultInterpolatedStringHandler.AppendFormatted<DisconnectReason>(this.DisconnectReason);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()) + "\n\n" + TextManager.Get("banreason") + " " + TextManager.GetServerMessage(this.AdditionalInformation);
			}
		}

		// Token: 0x17000F4B RID: 3915
		// (get) Token: 0x060037C9 RID: 14281 RVA: 0x0017661C File Offset: 0x0017481C
		private LocalizedString ServerMessage
		{
			get
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
				defaultInterpolatedStringHandler.AppendLiteral("ServerMessage.");
				defaultInterpolatedStringHandler.AppendFormatted<DisconnectReason>(this.DisconnectReason);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x17000F4C RID: 3916
		// (get) Token: 0x060037CA RID: 14282 RVA: 0x00176658 File Offset: 0x00174858
		public LocalizedString PopupMessage
		{
			get
			{
				DisconnectReason disconnectReason = this.DisconnectReason;
				if (disconnectReason <= DisconnectReason.Kicked)
				{
					if (disconnectReason == DisconnectReason.Banned)
					{
						return this.MsgWithReason;
					}
					if (disconnectReason == DisconnectReason.Kicked)
					{
						return this.MsgWithReason;
					}
				}
				else
				{
					if (disconnectReason == DisconnectReason.AuthenticationFailed)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
						defaultInterpolatedStringHandler.AppendLiteral("DisconnectReason.");
						defaultInterpolatedStringHandler.AppendFormatted<DisconnectReason>(this.DisconnectReason);
						return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(TextManager.Get("ChatMsg.DisconnectReason.AuthenticationRequired"), true);
					}
					switch (disconnectReason)
					{
					case DisconnectReason.InvalidVersion:
						return TextManager.GetWithVariables("DisconnectMessage.InvalidVersion", new ValueTuple<string, string>[]
						{
							new ValueTuple<string, string>("[version]", this.AdditionalInformation),
							new ValueTuple<string, string>("[clientversion]", GameMain.Version.ToString())
						});
					case DisconnectReason.ExcessiveDesyncOldEvent:
						return this.ServerMessage;
					case DisconnectReason.ExcessiveDesyncRemovedEvent:
						return this.ServerMessage;
					case DisconnectReason.SyncTimeout:
						return this.ServerMessage;
					}
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("DisconnectReason.");
				defaultInterpolatedStringHandler2.AppendFormatted<DisconnectReason>(this.DisconnectReason);
				LocalizedString localizedString = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(TextManager.Get("ConnectionLost"));
				defaultInterpolatedStringHandler3.AppendLiteral(" (");
				defaultInterpolatedStringHandler3.AppendFormatted<DisconnectReason>(this.DisconnectReason);
				defaultInterpolatedStringHandler3.AppendLiteral(")");
				return localizedString.Fallback(defaultInterpolatedStringHandler3.ToStringAndClear(), true);
			}
		}

		// Token: 0x17000F4D RID: 3917
		// (get) Token: 0x060037CB RID: 14283 RVA: 0x001767F9 File Offset: 0x001749F9
		public LocalizedString ReconnectMessage
		{
			get
			{
				return this.PopupMessage + "\n\n" + TextManager.Get("ConnectionLostReconnecting");
			}
		}

		// Token: 0x17000F4E RID: 3918
		// (get) Token: 0x060037CC RID: 14284 RVA: 0x00176820 File Offset: 0x00174A20
		public PlayerConnectionChangeType ConnectionChangeType
		{
			get
			{
				DisconnectReason disconnectReason = this.DisconnectReason;
				PlayerConnectionChangeType result;
				if (disconnectReason != DisconnectReason.Banned)
				{
					if (disconnectReason != DisconnectReason.Kicked)
					{
						result = PlayerConnectionChangeType.Disconnected;
					}
					else
					{
						result = PlayerConnectionChangeType.Kicked;
					}
				}
				else
				{
					result = PlayerConnectionChangeType.Banned;
				}
				return result;
			}
		}

		// Token: 0x17000F4F RID: 3919
		// (get) Token: 0x060037CD RID: 14285 RVA: 0x0017684C File Offset: 0x00174A4C
		public bool ShouldAttemptReconnect
		{
			get
			{
				DisconnectReason disconnectReason = this.DisconnectReason;
				return disconnectReason - DisconnectReason.Timeout <= 4;
			}
		}

		// Token: 0x17000F50 RID: 3920
		// (get) Token: 0x060037CE RID: 14286 RVA: 0x00176870 File Offset: 0x00174A70
		public bool IsEventSyncError
		{
			get
			{
				DisconnectReason disconnectReason = this.DisconnectReason;
				return disconnectReason - DisconnectReason.ExcessiveDesyncOldEvent <= 2;
			}
		}

		// Token: 0x17000F51 RID: 3921
		// (get) Token: 0x060037CF RID: 14287 RVA: 0x00176894 File Offset: 0x00174A94
		public bool ShouldCreateAnalyticsEvent
		{
			get
			{
				bool flag;
				switch (this.DisconnectReason)
				{
				case DisconnectReason.Disconnected:
				case DisconnectReason.Banned:
				case DisconnectReason.Kicked:
				case DisconnectReason.ServerShutdown:
				case DisconnectReason.ServerFull:
				case DisconnectReason.TooManyFailedLogins:
				case DisconnectReason.InvalidVersion:
					flag = true;
					goto IL_4B;
				}
				flag = false;
				IL_4B:
				return !flag;
			}
		}

		// Token: 0x060037D0 RID: 14288 RVA: 0x001768F0 File Offset: 0x00174AF0
		public string ToLidgrenStringRepresentation()
		{
			return this.DisconnectReason.ToString() + "}Separator[" + PeerDisconnectPacket.<ToLidgrenStringRepresentation>g__strToBase64|20_0(this.AdditionalInformation);
		}

		// Token: 0x060037D1 RID: 14289 RVA: 0x00176928 File Offset: 0x00174B28
		[NullableContext(0)]
		public static Option<PeerDisconnectPacket> FromLidgrenStringRepresentation([Nullable(1)] string str)
		{
			if (str == "Failed to establish connection - no response from remote host" || str == "Connection timed out" || str == "Reconnecting")
			{
				return Option.Some<PeerDisconnectPacket>(PeerDisconnectPacket.WithReason(DisconnectReason.Timeout));
			}
			string[] split = str.Split("}Separator[", StringSplitOptions.None);
			if (split.Length != 2)
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			DisconnectReason disconnectReason;
			if (!Enum.TryParse<DisconnectReason>(split[0], out disconnectReason))
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			return Option.Some<PeerDisconnectPacket>(new PeerDisconnectPacket(disconnectReason, PeerDisconnectPacket.<FromLidgrenStringRepresentation>g__base64ToStr|21_0(split[1])));
		}

		// Token: 0x060037D2 RID: 14290 RVA: 0x001769B7 File Offset: 0x00174BB7
		public static PeerDisconnectPacket Custom(string customMessage)
		{
			return new PeerDisconnectPacket(DisconnectReason.Unknown, customMessage);
		}

		// Token: 0x060037D3 RID: 14291 RVA: 0x001769C0 File Offset: 0x00174BC0
		public static PeerDisconnectPacket WithReason(DisconnectReason disconnectReason)
		{
			return new PeerDisconnectPacket(disconnectReason, "");
		}

		// Token: 0x060037D4 RID: 14292 RVA: 0x001769CD File Offset: 0x00174BCD
		[NullableContext(2)]
		public static PeerDisconnectPacket Kicked(string msg)
		{
			return new PeerDisconnectPacket(DisconnectReason.Kicked, msg ?? "");
		}

		// Token: 0x060037D5 RID: 14293 RVA: 0x001769DF File Offset: 0x00174BDF
		[NullableContext(2)]
		public static PeerDisconnectPacket Banned(string msg)
		{
			return new PeerDisconnectPacket(DisconnectReason.Banned, msg ?? "");
		}

		// Token: 0x060037D6 RID: 14294 RVA: 0x001769F1 File Offset: 0x00174BF1
		public static PeerDisconnectPacket InvalidVersion()
		{
			return new PeerDisconnectPacket(DisconnectReason.InvalidVersion, GameMain.Version.ToString());
		}

		// Token: 0x060037D7 RID: 14295 RVA: 0x00176A04 File Offset: 0x00174C04
		public static PeerDisconnectPacket SteamP2PError(P2PSessionError error)
		{
			return new PeerDisconnectPacket(DisconnectReason.SteamP2PError, error.ToString());
		}

		// Token: 0x060037D8 RID: 14296 RVA: 0x00176A1C File Offset: 0x00174C1C
		public static PeerDisconnectPacket SteamAuthError(BeginAuthResult error)
		{
			DisconnectReason disconnectReason = DisconnectReason.AuthenticationFailed;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted("BeginAuthResult");
			defaultInterpolatedStringHandler.AppendLiteral(".");
			defaultInterpolatedStringHandler.AppendFormatted<BeginAuthResult>(error);
			return new PeerDisconnectPacket(disconnectReason, defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x060037D9 RID: 14297 RVA: 0x00176A60 File Offset: 0x00174C60
		public static PeerDisconnectPacket SteamAuthError(AuthResponse error)
		{
			DisconnectReason disconnectReason = DisconnectReason.AuthenticationFailed;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted("AuthResponse");
			defaultInterpolatedStringHandler.AppendLiteral(".");
			defaultInterpolatedStringHandler.AppendFormatted<AuthResponse>(error);
			return new PeerDisconnectPacket(disconnectReason, defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x060037DA RID: 14298 RVA: 0x00176AA3 File Offset: 0x00174CA3
		[CompilerGenerated]
		internal static string <ToLidgrenStringRepresentation>g__strToBase64|20_0(string str)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
		}

		// Token: 0x060037DB RID: 14299 RVA: 0x00176AB5 File Offset: 0x00174CB5
		[CompilerGenerated]
		internal static string <FromLidgrenStringRepresentation>g__base64ToStr|21_0(string base64)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
		}

		// Token: 0x04001BF5 RID: 7157
		public readonly DisconnectReason DisconnectReason;

		// Token: 0x04001BF6 RID: 7158
		public readonly string AdditionalInformation;
	}
}
