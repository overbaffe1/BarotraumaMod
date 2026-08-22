using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000374 RID: 884
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class ServerPeer<[Nullable(0)] TConnection> : ServerPeer where TConnection : NetworkConnection
	{
		// Token: 0x060034C3 RID: 13507 RVA: 0x0016C1A8 File Offset: 0x0016A3A8
		protected ServerPeer(ServerPeer.Callbacks callbacks, ServerSettings serverSettings)
		{
			Option.UnspecifiedNone none = Option.None;
			this.ownerKey = none;
			base..ctor(callbacks);
			this.serverSettings = serverSettings;
			this.connectedClients = new List<ServerPeer<TConnection>.ClientConnectionData>();
			this.pendingClients = new List<ServerPeer<TConnection>.PendingClient>();
			List<ContentPackage> contentPackageList = new List<ContentPackage>();
			foreach (ContentPackage cp2 in ContentPackageManager.EnabledPackages.All)
			{
				if (cp2.Files.Any<ContentFile>())
				{
					if (!cp2.HasMultiplayerSyncedContent)
					{
						if (!cp2.Files.All((ContentFile f) => f is SubmarineFile))
						{
							continue;
						}
					}
					ContentPackageId id1;
					if (cp2.UgcId.TryUnwrap(out id1))
					{
						ContentPackage existingPackage = contentPackageList.FirstOrDefault(delegate(ContentPackage cp)
						{
							ContentPackageId id2;
							return cp.UgcId.TryUnwrap(out id2) && id1.Equals(id2);
						});
						if (existingPackage != null)
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(80, 4);
							defaultInterpolatedStringHandler.AppendLiteral("The content package \"");
							defaultInterpolatedStringHandler.AppendFormatted(existingPackage.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\" (");
							defaultInterpolatedStringHandler.AppendFormatted(existingPackage.Path);
							defaultInterpolatedStringHandler.AppendLiteral(") has the same id as \"");
							defaultInterpolatedStringHandler.AppendFormatted(cp2.Name);
							defaultInterpolatedStringHandler.AppendLiteral("\" (");
							defaultInterpolatedStringHandler.AppendFormatted(cp2.Path);
							defaultInterpolatedStringHandler.AppendLiteral("). Ignoring the latter package.");
							DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
							continue;
						}
					}
					contentPackageList.Add(cp2);
				}
			}
			this.contentPackages = contentPackageList.ToImmutableArray<ContentPackage>();
		}

		// Token: 0x060034C4 RID: 13508 RVA: 0x0016C35C File Offset: 0x0016A55C
		protected void ReadConnectionInitializationStep([Nullable(new byte[]
		{
			1,
			0
		})] ServerPeer<TConnection>.PendingClient pendingClient, IReadMessage inc, ConnectionInitialization initializationStep)
		{
			pendingClient.TimeOut = NetworkConnection.TimeoutThresholdNotInGame;
			if (pendingClient.InitializationStep != initializationStep)
			{
				return;
			}
			pendingClient.UpdateTime = Timing.TotalTime + 0.016666666666666666;
			switch (initializationStep)
			{
			case ConnectionInitialization.AuthInfoAndVersion:
			{
				ClientAuthTicketAndVersionPacket authPacket;
				if (!INetSerializableStruct.TryRead<ClientAuthTicketAndVersionPacket>(inc, pendingClient.AccountInfo, out authPacket))
				{
					this.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.MalformedData));
					return;
				}
				if (!Client.IsValidName(authPacket.Name, this.serverSettings))
				{
					this.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.InvalidName));
					return;
				}
				Version remoteVersion;
				if (!Version.TryParse(authPacket.GameVersion, out remoteVersion) || !NetworkMember.IsCompatible(remoteVersion, GameMain.Version))
				{
					this.RemovePendingClient(pendingClient, PeerDisconnectPacket.InvalidVersion());
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler.AppendFormatted(authPacket.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<Option<AccountId>>(authPacket.AccountId);
					defaultInterpolatedStringHandler.AppendLiteral(") couldn't join the server (incompatible game version)");
					GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Error);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(56, 2);
					defaultInterpolatedStringHandler2.AppendFormatted(authPacket.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" (");
					defaultInterpolatedStringHandler2.AppendFormatted<Option<AccountId>>(authPacket.AccountId);
					defaultInterpolatedStringHandler2.AppendLiteral(") couldn't join the server (incompatible game version)");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler2.ToStringAndClear(), new Color?(Color.Red), false);
					return;
				}
				pendingClient.Connection.Language = authPacket.Language.ToLanguageIdentifier();
				Client nameTaken = GameMain.Server.ConnectedClients.Find((Client c) => Homoglyphs.Compare(c.Name.ToLower(), authPacket.Name.ToLower()));
				if (nameTaken != null)
				{
					this.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.NameTaken));
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(74, 2);
					defaultInterpolatedStringHandler3.AppendFormatted(authPacket.Name);
					defaultInterpolatedStringHandler3.AppendLiteral(" (");
					defaultInterpolatedStringHandler3.AppendFormatted<Option<AccountId>>(authPacket.AccountId);
					defaultInterpolatedStringHandler3.AppendLiteral(") couldn't join the server (name too similar to the name of the client \"");
					GameServer.Log(defaultInterpolatedStringHandler3.ToStringAndClear() + nameTaken.Name + "\").", ServerLog.MessageType.Error);
					return;
				}
				if (!pendingClient.AuthSessionStarted)
				{
					this.ProcessAuthTicket(authPacket, pendingClient);
					return;
				}
				break;
			}
			case ConnectionInitialization.ContentPackageOrder:
				pendingClient.InitializationStep = ConnectionInitialization.Success;
				pendingClient.UpdateTime = Timing.TotalTime;
				break;
			case ConnectionInitialization.Password:
			{
				ClientPeerPasswordPacket passwordPacket;
				if (!INetSerializableStruct.TryRead<ClientPeerPasswordPacket>(inc, pendingClient.AccountInfo, out passwordPacket))
				{
					this.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.MalformedData));
					return;
				}
				int? passwordSalt = pendingClient.PasswordSalt;
				if (passwordSalt == null)
				{
					DebugConsole.ThrowError("Received password message from client without salt", null, null, false, false);
					return;
				}
				if (this.serverSettings.IsPasswordCorrect(passwordPacket.Password, pendingClient.PasswordSalt.Value))
				{
					pendingClient.InitializationStep = ConnectionInitialization.ContentPackageOrder;
				}
				else
				{
					pendingClient.PasswordRetries++;
					if (this.serverSettings.BanAfterWrongPassword && pendingClient.PasswordRetries > this.serverSettings.MaxPasswordRetriesBeforeBan)
					{
						this.BanPendingClient(pendingClient, "Failed to enter correct password too many times", null);
						this.RemovePendingClient(pendingClient, PeerDisconnectPacket.Banned("Failed to enter correct password too many times"));
						return;
					}
				}
				pendingClient.UpdateTime = Timing.TotalTime;
				return;
			}
			default:
				return;
			}
		}

		// Token: 0x060034C5 RID: 13509
		protected abstract void ProcessAuthTicket(ClientAuthTicketAndVersionPacket packet, [Nullable(new byte[]
		{
			1,
			0
		})] ServerPeer<TConnection>.PendingClient pendingClient);

		// Token: 0x060034C6 RID: 13510 RVA: 0x0016C688 File Offset: 0x0016A888
		protected void BanPendingClient([Nullable(new byte[]
		{
			1,
			0
		})] ServerPeer<TConnection>.PendingClient pendingClient, string banReason, TimeSpan? duration)
		{
			ServerPeer<TConnection>.<>c__DisplayClass11_0 CS$<>8__locals1 = new ServerPeer<TConnection>.<>c__DisplayClass11_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.pendingClient = pendingClient;
			CS$<>8__locals1.banReason = banReason;
			CS$<>8__locals1.duration = duration;
			AccountInfo accountInfo = CS$<>8__locals1.pendingClient.AccountInfo;
			AccountId id;
			if (accountInfo.AccountId.TryUnwrap(out id))
			{
				CS$<>8__locals1.<BanPendingClient>g__banAccountId|0(id);
			}
			CS$<>8__locals1.pendingClient.AccountInfo.OtherMatchingIds.ForEach(new Action<AccountId>(CS$<>8__locals1.<BanPendingClient>g__banAccountId|0));
			accountInfo = CS$<>8__locals1.pendingClient.AccountInfo;
			AccountId accountId;
			if (accountInfo.AccountId.TryUnwrap(out accountId))
			{
				this.serverSettings.BanList.BanPlayer(CS$<>8__locals1.pendingClient.Name ?? "Player", accountId, CS$<>8__locals1.banReason, CS$<>8__locals1.duration);
				return;
			}
			this.serverSettings.BanList.BanPlayer(CS$<>8__locals1.pendingClient.Name ?? "Player", CS$<>8__locals1.pendingClient.Connection.Endpoint, CS$<>8__locals1.banReason, CS$<>8__locals1.duration);
		}

		// Token: 0x060034C7 RID: 13511 RVA: 0x0016C798 File Offset: 0x0016A998
		[NullableContext(2)]
		protected bool IsPendingClientBanned([Nullable(new byte[]
		{
			1,
			0
		})] ServerPeer<TConnection>.PendingClient pendingClient, out string banReason)
		{
			banReason = null;
			AccountInfo accountInfo = pendingClient.AccountInfo;
			AccountId id;
			bool isBanned = accountInfo.AccountId.TryUnwrap(out id) && this.<IsPendingClientBanned>g__isAccountIdBanned|12_0(id, out banReason);
			accountInfo = pendingClient.AccountInfo;
			foreach (AccountId otherId in accountInfo.OtherMatchingIds)
			{
				if (isBanned)
				{
					break;
				}
				isBanned |= this.<IsPendingClientBanned>g__isAccountIdBanned|12_0(otherId, out banReason);
			}
			return isBanned;
		}

		// Token: 0x060034C8 RID: 13512
		protected abstract void SendMsgInternal(TConnection conn, PeerPacketHeaders headers, [Nullable(2)] INetSerializableStruct body);

		// Token: 0x060034C9 RID: 13513 RVA: 0x0016C804 File Offset: 0x0016AA04
		protected void UpdatePendingClient([Nullable(new byte[]
		{
			1,
			0
		})] ServerPeer<TConnection>.PendingClient pendingClient)
		{
			if (this.connectedClients.Count >= this.serverSettings.MaxPlayers)
			{
				this.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.ServerFull));
			}
			string banReason;
			if (this.IsPendingClientBanned(pendingClient, out banReason))
			{
				this.RemovePendingClient(pendingClient, PeerDisconnectPacket.Banned(banReason));
				return;
			}
			if (pendingClient.InitializationStep == ConnectionInitialization.Success)
			{
				TConnection newConnection = pendingClient.Connection;
				this.connectedClients.Add(new ServerPeer<TConnection>.ClientConnectionData(newConnection));
				this.pendingClients.Remove(pendingClient);
				this.callbacks.OnInitializationComplete(newConnection, pendingClient.Name);
				this.CheckOwnership(pendingClient);
			}
			pendingClient.TimeOut -= 0.016666666666666666;
			if (pendingClient.TimeOut < 0.0)
			{
				this.RemovePendingClient(pendingClient, PeerDisconnectPacket.WithReason(DisconnectReason.Timeout));
			}
			if (Timing.TotalTime < pendingClient.UpdateTime)
			{
				return;
			}
			pendingClient.UpdateTime = Timing.TotalTime + 1.0;
			PeerPacketHeaders headers = new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = (PacketHeader.IsConnectionInitializationStep | PacketHeader.IsServerMessage),
				Initialization = new ConnectionInitialization?(pendingClient.InitializationStep)
			};
			INetSerializableStruct structToSend = null;
			ConnectionInitialization initializationStep = pendingClient.InitializationStep;
			if (initializationStep != ConnectionInitialization.ContentPackageOrder)
			{
				if (initializationStep == ConnectionInitialization.Password)
				{
					structToSend = new ServerPeerPasswordPacket
					{
						Salt = ServerPeer<TConnection>.<UpdatePendingClient>g__GetSalt|14_0(pendingClient),
						RetriesLeft = Option<int>.Some(pendingClient.PasswordRetries)
					};
				}
			}
			else
			{
				SerializableDateTime timeNow = SerializableDateTime.UtcNow;
				structToSend = new ServerPeerContentPackageOrderPacket
				{
					ServerName = GameMain.Server.ServerName,
					ContentPackages = (from contentPackage in this.contentPackages
					select new ServerContentPackage(contentPackage, timeNow)).ToImmutableArray<ServerContentPackage>(),
					AllowModDownloads = this.serverSettings.AllowModDownloads
				};
			}
			this.SendMsgInternal(pendingClient.Connection, headers, structToSend);
		}

		// Token: 0x060034CA RID: 13514 RVA: 0x0016C9E7 File Offset: 0x0016ABE7
		protected virtual void CheckOwnership([Nullable(new byte[]
		{
			1,
			0
		})] ServerPeer<TConnection>.PendingClient pendingClient)
		{
		}

		// Token: 0x060034CB RID: 13515 RVA: 0x0016C9EC File Offset: 0x0016ABEC
		public void RemovePendingClient([Nullable(new byte[]
		{
			1,
			0
		})] ServerPeer<TConnection>.PendingClient pendingClient, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (this.pendingClients.Contains(pendingClient))
			{
				this.Disconnect(pendingClient.Connection, peerDisconnectPacket);
				this.pendingClients.Remove(pendingClient);
				pendingClient.Connection.SetAccountInfo(AccountInfo.None);
				pendingClient.AuthSessionStarted = false;
			}
		}

		// Token: 0x060034CC RID: 13516 RVA: 0x0016CA42 File Offset: 0x0016AC42
		[CompilerGenerated]
		private bool <IsPendingClientBanned>g__isAccountIdBanned|12_0(AccountId accountId, [Nullable(2)] out string banReason)
		{
			return this.serverSettings.BanList.IsBanned(accountId, out banReason);
		}

		// Token: 0x060034CD RID: 13517 RVA: 0x0016CA58 File Offset: 0x0016AC58
		[NullableContext(0)]
		[CompilerGenerated]
		internal static Option<int> <UpdatePendingClient>g__GetSalt|14_0([Nullable(new byte[]
		{
			1,
			0
		})] ServerPeer<TConnection>.PendingClient client)
		{
			int? passwordSalt = client.PasswordSalt;
			int salt;
			if (passwordSalt != null)
			{
				salt = passwordSalt.GetValueOrDefault();
				return Option<int>.Some(salt);
			}
			salt = CryptoRandom.Instance.Next();
			client.PasswordSalt = new int?(salt);
			return Option<int>.Some(salt);
		}

		// Token: 0x04001A34 RID: 6708
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<ContentPackage> contentPackages;

		// Token: 0x04001A35 RID: 6709
		[Nullable(new byte[]
		{
			1,
			1,
			0
		})]
		protected readonly List<ServerPeer<TConnection>.ClientConnectionData> connectedClients;

		// Token: 0x04001A36 RID: 6710
		[Nullable(new byte[]
		{
			1,
			1,
			0
		})]
		protected readonly List<ServerPeer<TConnection>.PendingClient> pendingClients;

		// Token: 0x04001A37 RID: 6711
		protected readonly ServerSettings serverSettings;

		// Token: 0x04001A38 RID: 6712
		[Nullable(2)]
		protected TConnection OwnerConnection;

		// Token: 0x04001A39 RID: 6713
		[Nullable(0)]
		protected Option<int> ownerKey;

		// Token: 0x02000C15 RID: 3093
		[NullableContext(0)]
		public sealed class PendingClient
		{
			// Token: 0x1700161A RID: 5658
			// (get) Token: 0x060062F1 RID: 25329 RVA: 0x00210B03 File Offset: 0x0020ED03
			public AccountInfo AccountInfo
			{
				get
				{
					return this.Connection.AccountInfo;
				}
			}

			// Token: 0x060062F2 RID: 25330 RVA: 0x00210B18 File Offset: 0x0020ED18
			[NullableContext(1)]
			public PendingClient(TConnection conn)
			{
				Option.UnspecifiedNone none = Option.None;
				this.OwnerKey = none;
				this.Connection = conn;
				this.InitializationStep = ConnectionInitialization.AuthInfoAndVersion;
				this.PasswordRetries = 0;
				this.PasswordSalt = null;
				this.UpdateTime = Timing.TotalTime + 0.05;
				this.TimeOut = NetworkConnection.TimeoutThresholdNotInGame;
				this.AuthSessionStarted = false;
			}

			// Token: 0x060062F3 RID: 25331 RVA: 0x00210B86 File Offset: 0x0020ED86
			public void Heartbeat()
			{
				this.TimeOut = NetworkConnection.TimeoutThresholdNotInGame;
			}

			// Token: 0x04003B54 RID: 15188
			[Nullable(2)]
			public string Name;

			// Token: 0x04003B55 RID: 15189
			public Option<int> OwnerKey;

			// Token: 0x04003B56 RID: 15190
			[Nullable(1)]
			public readonly TConnection Connection;

			// Token: 0x04003B57 RID: 15191
			public ConnectionInitialization InitializationStep;

			// Token: 0x04003B58 RID: 15192
			public double UpdateTime;

			// Token: 0x04003B59 RID: 15193
			public double TimeOut;

			// Token: 0x04003B5A RID: 15194
			public int PasswordRetries;

			// Token: 0x04003B5B RID: 15195
			public int? PasswordSalt;

			// Token: 0x04003B5C RID: 15196
			public bool AuthSessionStarted;
		}

		// Token: 0x02000C16 RID: 3094
		[Nullable(0)]
		protected sealed class ClientConnectionData
		{
			// Token: 0x060062F4 RID: 25332 RVA: 0x00210B93 File Offset: 0x0020ED93
			public ClientConnectionData(TConnection connection)
			{
			}

			// Token: 0x060062F5 RID: 25333 RVA: 0x00210BB8 File Offset: 0x0020EDB8
			[NullableContext(2)]
			public string TryGetClientName()
			{
				GameServer server = GameMain.Server;
				IReadOnlyList<Client> connClients = (server != null) ? server.ConnectedClients : null;
				if (connClients != null)
				{
					foreach (Client client in connClients)
					{
						NetworkConnection clientConnection = (client != null) ? client.Connection : null;
						if (clientConnection != null && clientConnection.EndpointMatches(this.Connection.Endpoint))
						{
							return client.Name;
						}
					}
				}
				return null;
			}

			// Token: 0x060062F6 RID: 25334 RVA: 0x00210C44 File Offset: 0x0020EE44
			public void BanClient(ServerSettings settings, string banReason, TimeSpan? duration)
			{
				ServerPeer<TConnection>.ClientConnectionData.<>c__DisplayClass5_0 CS$<>8__locals1 = new ServerPeer<TConnection>.ClientConnectionData.<>c__DisplayClass5_0();
				CS$<>8__locals1.settings = settings;
				CS$<>8__locals1.banReason = banReason;
				CS$<>8__locals1.duration = duration;
				CS$<>8__locals1.clientName = (this.TryGetClientName() ?? "Player");
				this.Connection.AccountInfo.OtherMatchingIds.ForEach(new Action<AccountId>(CS$<>8__locals1.<BanClient>g__BanAccountId|0));
				AccountId accountId;
				if (this.Connection.AccountInfo.AccountId.TryUnwrap(out accountId))
				{
					CS$<>8__locals1.<BanClient>g__BanAccountId|0(accountId);
					return;
				}
				CS$<>8__locals1.settings.BanList.BanPlayer(CS$<>8__locals1.clientName, this.Connection.Endpoint, CS$<>8__locals1.banReason, CS$<>8__locals1.duration);
			}

			// Token: 0x04003B5D RID: 15197
			public readonly TConnection Connection = connection;

			// Token: 0x04003B5E RID: 15198
			public readonly MessageFragmenter Fragmenter = new MessageFragmenter();

			// Token: 0x04003B5F RID: 15199
			public readonly MessageDefragmenter Defragmenter = new MessageDefragmenter();
		}
	}
}
