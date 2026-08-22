using System;
using System.Runtime.CompilerServices;
using System.Text;
using Steamworks;

namespace Barotrauma.Networking
{
	// Token: 0x020004C0 RID: 1216
	[NullableContext(1)]
	[Nullable(0)]
	[NetworkSerialize(103)]
	internal readonly struct PeerDisconnectPacket : INetSerializableStruct
	{
		// Token: 0x06004F9B RID: 20379 RVA: 0x002AEF54 File Offset: 0x002AD154
		private PeerDisconnectPacket(DisconnectReason disconnectReason, string additionalInformation = "")
		{
			this.DisconnectReason = disconnectReason;
			this.AdditionalInformation = additionalInformation;
		}

		// Token: 0x06004F9C RID: 20380 RVA: 0x002AEF64 File Offset: 0x002AD164
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

		// Token: 0x17001445 RID: 5189
		// (get) Token: 0x06004F9D RID: 20381 RVA: 0x002AF0DC File Offset: 0x002AD2DC
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

		// Token: 0x17001446 RID: 5190
		// (get) Token: 0x06004F9E RID: 20382 RVA: 0x002AF158 File Offset: 0x002AD358
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

		// Token: 0x17001447 RID: 5191
		// (get) Token: 0x06004F9F RID: 20383 RVA: 0x002AF194 File Offset: 0x002AD394
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

		// Token: 0x17001448 RID: 5192
		// (get) Token: 0x06004FA0 RID: 20384 RVA: 0x002AF335 File Offset: 0x002AD535
		public LocalizedString ReconnectMessage
		{
			get
			{
				return this.PopupMessage + "\n\n" + TextManager.Get("ConnectionLostReconnecting");
			}
		}

		// Token: 0x17001449 RID: 5193
		// (get) Token: 0x06004FA1 RID: 20385 RVA: 0x002AF35C File Offset: 0x002AD55C
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

		// Token: 0x1700144A RID: 5194
		// (get) Token: 0x06004FA2 RID: 20386 RVA: 0x002AF388 File Offset: 0x002AD588
		public bool ShouldAttemptReconnect
		{
			get
			{
				DisconnectReason disconnectReason = this.DisconnectReason;
				return disconnectReason - DisconnectReason.Timeout <= 4;
			}
		}

		// Token: 0x1700144B RID: 5195
		// (get) Token: 0x06004FA3 RID: 20387 RVA: 0x002AF3AC File Offset: 0x002AD5AC
		public bool IsEventSyncError
		{
			get
			{
				DisconnectReason disconnectReason = this.DisconnectReason;
				return disconnectReason - DisconnectReason.ExcessiveDesyncOldEvent <= 2;
			}
		}

		// Token: 0x1700144C RID: 5196
		// (get) Token: 0x06004FA4 RID: 20388 RVA: 0x002AF3D0 File Offset: 0x002AD5D0
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

		// Token: 0x06004FA5 RID: 20389 RVA: 0x002AF42C File Offset: 0x002AD62C
		public string ToLidgrenStringRepresentation()
		{
			return this.DisconnectReason.ToString() + "}Separator[" + PeerDisconnectPacket.<ToLidgrenStringRepresentation>g__strToBase64|20_0(this.AdditionalInformation);
		}

		// Token: 0x06004FA6 RID: 20390 RVA: 0x002AF464 File Offset: 0x002AD664
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

		// Token: 0x06004FA7 RID: 20391 RVA: 0x002AF4F3 File Offset: 0x002AD6F3
		public static PeerDisconnectPacket Custom(string customMessage)
		{
			return new PeerDisconnectPacket(DisconnectReason.Unknown, customMessage);
		}

		// Token: 0x06004FA8 RID: 20392 RVA: 0x002AF4FC File Offset: 0x002AD6FC
		public static PeerDisconnectPacket WithReason(DisconnectReason disconnectReason)
		{
			return new PeerDisconnectPacket(disconnectReason, "");
		}

		// Token: 0x06004FA9 RID: 20393 RVA: 0x002AF509 File Offset: 0x002AD709
		[NullableContext(2)]
		public static PeerDisconnectPacket Kicked(string msg)
		{
			return new PeerDisconnectPacket(DisconnectReason.Kicked, msg ?? "");
		}

		// Token: 0x06004FAA RID: 20394 RVA: 0x002AF51B File Offset: 0x002AD71B
		[NullableContext(2)]
		public static PeerDisconnectPacket Banned(string msg)
		{
			return new PeerDisconnectPacket(DisconnectReason.Banned, msg ?? "");
		}

		// Token: 0x06004FAB RID: 20395 RVA: 0x002AF52D File Offset: 0x002AD72D
		public static PeerDisconnectPacket InvalidVersion()
		{
			return new PeerDisconnectPacket(DisconnectReason.InvalidVersion, GameMain.Version.ToString());
		}

		// Token: 0x06004FAC RID: 20396 RVA: 0x002AF540 File Offset: 0x002AD740
		public static PeerDisconnectPacket SteamP2PError(P2PSessionError error)
		{
			return new PeerDisconnectPacket(DisconnectReason.SteamP2PError, error.ToString());
		}

		// Token: 0x06004FAD RID: 20397 RVA: 0x002AF558 File Offset: 0x002AD758
		public static PeerDisconnectPacket SteamAuthError(BeginAuthResult error)
		{
			DisconnectReason disconnectReason = DisconnectReason.AuthenticationFailed;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted("BeginAuthResult");
			defaultInterpolatedStringHandler.AppendLiteral(".");
			defaultInterpolatedStringHandler.AppendFormatted<BeginAuthResult>(error);
			return new PeerDisconnectPacket(disconnectReason, defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06004FAE RID: 20398 RVA: 0x002AF59C File Offset: 0x002AD79C
		public static PeerDisconnectPacket SteamAuthError(AuthResponse error)
		{
			DisconnectReason disconnectReason = DisconnectReason.AuthenticationFailed;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler.AppendFormatted("AuthResponse");
			defaultInterpolatedStringHandler.AppendLiteral(".");
			defaultInterpolatedStringHandler.AppendFormatted<AuthResponse>(error);
			return new PeerDisconnectPacket(disconnectReason, defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x06004FAF RID: 20399 RVA: 0x002AF5DF File Offset: 0x002AD7DF
		[CompilerGenerated]
		internal static string <ToLidgrenStringRepresentation>g__strToBase64|20_0(string str)
		{
			return Convert.ToBase64String(Encoding.UTF8.GetBytes(str));
		}

		// Token: 0x06004FB0 RID: 20400 RVA: 0x002AF5F1 File Offset: 0x002AD7F1
		[CompilerGenerated]
		internal static string <FromLidgrenStringRepresentation>g__base64ToStr|21_0(string base64)
		{
			return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
		}

		// Token: 0x040029EC RID: 10732
		public readonly DisconnectReason DisconnectReason;

		// Token: 0x040029ED RID: 10733
		public readonly string AdditionalInformation;
	}
}
