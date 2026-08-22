using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Barotrauma.Extensions;
using Barotrauma.Steam;

namespace Barotrauma.Networking
{
	// Token: 0x0200046B RID: 1131
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class P2PClientPeer : ClientPeer<P2PEndpoint>
	{
		// Token: 0x06004BFB RID: 19451 RVA: 0x0029DB88 File Offset: 0x0029BD88
		private static P2PEndpoint GetPrimaryEndpoint([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<P2PEndpoint> allEndpoints)
		{
			Option<SteamP2PEndpoint> steamEndpointOption = allEndpoints.OfType<SteamP2PEndpoint>().FirstOrNone<SteamP2PEndpoint>();
			Option<EosP2PEndpoint> eosEndpointOption = allEndpoints.OfType<EosP2PEndpoint>().FirstOrNone<EosP2PEndpoint>();
			SteamP2PEndpoint steamEndpoint;
			if (SteamManager.IsInitialized && steamEndpointOption.TryUnwrap(out steamEndpoint))
			{
				return steamEndpoint;
			}
			EosP2PEndpoint eosEndpoint;
			if (EosInterface.Core.IsInitialized && eosEndpointOption.TryUnwrap(out eosEndpoint))
			{
				return eosEndpoint;
			}
			throw new Exception("Couldn't pick out a primary endpoint: " + string.Join(", ", from e in allEndpoints
			select e.GetType().Name));
		}

		// Token: 0x06004BFC RID: 19452 RVA: 0x0029DC18 File Offset: 0x0029BE18
		public P2PClientPeer([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<P2PEndpoint> allEndpoints, ClientPeer.Callbacks callbacks)
		{
			P2PEndpoint primaryEndpoint = P2PClientPeer.GetPrimaryEndpoint(allEndpoints);
			ImmutableArray<Endpoint> allServerEndpoints = allEndpoints.Cast<Endpoint>().ToImmutableArray<Endpoint>();
			Option.UnspecifiedNone none = Option.None;
			base..ctor(primaryEndpoint, allServerEndpoints, callbacks, none);
			base.ServerConnection = null;
			this.isActive = false;
		}

		// Token: 0x06004BFD RID: 19453 RVA: 0x0029DC8C File Offset: 0x0029BE8C
		public override void Start()
		{
			base.ContentPackageOrderReceived = false;
			base.ServerConnection = base.ServerEndpoint.MakeConnectionFromEndpoint();
			P2PSocket.Callbacks socketCallbacks = new P2PSocket.Callbacks(new Predicate<P2PEndpoint>(this.OnIncomingConnection), new Action<P2PEndpoint, PeerDisconnectPacket>(this.OnConnectionClosed), new P2POwnerDoSProtection.ExcessivePacketDelegate(this.OnExcessivePackets), new Action<P2PEndpoint, IReadMessage>(this.OnP2PData));
			P2PEndpoint serverEndpoint = base.ServerEndpoint;
			Result<P2PSocket, P2PSocket.Error> result;
			if (!(serverEndpoint is EosP2PEndpoint))
			{
				SteamP2PEndpoint steamP2PEndpoint = serverEndpoint as SteamP2PEndpoint;
				if (steamP2PEndpoint == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(26, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Invalid server endpoint: ");
					defaultInterpolatedStringHandler.AppendFormatted<Type>(base.ServerEndpoint.GetType());
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted<P2PEndpoint>(base.ServerEndpoint);
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				result = SteamConnectSocket.Create(steamP2PEndpoint, socketCallbacks, P2PSocket.OwnerOrClient.Client);
			}
			else
			{
				result = EosP2PSocket.Create(socketCallbacks, P2PSocket.OwnerOrClient.Client);
			}
			Result<P2PSocket, P2PSocket.Error> socketCreateResult = result;
			P2PSocket s;
			if (!socketCreateResult.TryUnwrapSuccess(out s))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(30, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Failed to create socket for ");
				defaultInterpolatedStringHandler2.AppendFormatted<P2PEndpoint>(base.ServerEndpoint);
				defaultInterpolatedStringHandler2.AppendLiteral(": ");
				defaultInterpolatedStringHandler2.AppendFormatted<Result<P2PSocket, P2PSocket.Error>>(socketCreateResult);
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			this.socket = s;
			TaskPool.Add("P2PClientPeer.GetAuthTicket", AuthenticationTicket.Create(base.ServerEndpoint), delegate(Task t)
			{
				Option<AuthenticationTicket> authenticationTicket;
				if (!t.TryGetResult(out authenticationTicket))
				{
					this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.AuthenticationFailed));
					return;
				}
				this.authTicket = authenticationTicket;
				PeerPacketHeaders headers = new PeerPacketHeaders
				{
					DeliveryMethod = DeliveryMethod.Reliable,
					PacketHeader = PacketHeader.IsConnectionInitializationStep,
					Initialization = new ConnectionInitialization?(ConnectionInitialization.ConnectionStarted)
				};
				this.SendMsgInternal(headers, null);
			});
			this.initializationStep = ConnectionInitialization.AuthInfoAndVersion;
			this.timeout = NetworkConnection.TimeoutThresholdNotInGame;
			this.heartbeatTimer = 1.0;
			this.isActive = true;
		}

		// Token: 0x06004BFE RID: 19454 RVA: 0x0029DE0D File Offset: 0x0029C00D
		private void OnExcessivePackets(P2PEndpoint endpoint, bool shouldBan)
		{
		}

		// Token: 0x06004BFF RID: 19455 RVA: 0x0029DE10 File Offset: 0x0029C010
		private bool OnIncomingConnection(P2PEndpoint remoteEndpoint)
		{
			if (remoteEndpoint == base.ServerEndpoint)
			{
				return true;
			}
			if (this.initializationStep != ConnectionInitialization.Password && this.initializationStep != ConnectionInitialization.ContentPackageOrder && this.initializationStep != ConnectionInitialization.Success)
			{
				string str = "Connection from incorrect endpoint was rejected: ";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler.AppendLiteral("expected ");
				defaultInterpolatedStringHandler.AppendFormatted<P2PEndpoint>(base.ServerEndpoint);
				defaultInterpolatedStringHandler.AppendLiteral(", ");
				string str2 = defaultInterpolatedStringHandler.ToStringAndClear();
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(4, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("got ");
				defaultInterpolatedStringHandler2.AppendFormatted<P2PEndpoint>(remoteEndpoint);
				DebugConsole.AddWarning(str + str2 + defaultInterpolatedStringHandler2.ToStringAndClear(), null);
			}
			return false;
		}

		// Token: 0x06004C00 RID: 19456 RVA: 0x0029DEB2 File Offset: 0x0029C0B2
		private void OnConnectionClosed(P2PEndpoint remoteEndpoint, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (remoteEndpoint != base.ServerEndpoint)
			{
				return;
			}
			this.Close(peerDisconnectPacket);
		}

		// Token: 0x06004C01 RID: 19457 RVA: 0x0029DECC File Offset: 0x0029C0CC
		private void OnP2PData(P2PEndpoint senderEndpoint, IReadMessage inc)
		{
			if (!this.isActive)
			{
				return;
			}
			this.receivedBytes += (long)inc.LengthBytes;
			if (senderEndpoint != base.ServerEndpoint)
			{
				return;
			}
			this.timeout = ((Screen.Selected == GameMain.GameScreen) ? NetworkConnection.TimeoutThresholdInGame : NetworkConnection.TimeoutThresholdNotInGame);
			DeliveryMethod deliveryMethod;
			PacketHeader packetHeader2;
			ConnectionInitialization? connectionInitialization;
			INetSerializableStruct.Read<PeerPacketHeaders>(inc).Deconstruct(out deliveryMethod, out packetHeader2, out connectionInitialization);
			PacketHeader packetHeader = packetHeader2;
			ConnectionInitialization? initialization = connectionInitialization;
			if (!packetHeader.IsServerMessage())
			{
				return;
			}
			if (packetHeader.IsConnectionInitializationStep())
			{
				if (initialization == null)
				{
					return;
				}
				P2PInitializationRelayPacket relayPacket = INetSerializableStruct.Read<P2PInitializationRelayPacket>(inc);
				if (this.initializationStep != ConnectionInitialization.Success)
				{
					this.incomingInitializationMessages.Add(new ClientPeer.IncomingInitializationMessage
					{
						InitializationStep = initialization.Value,
						Message = relayPacket.Message.GetReadMessageUncompressed()
					});
					return;
				}
			}
			else if (packetHeader.IsDataFragment())
			{
				ImmutableArray<byte> completeMessage;
				if (!this.defragmenter.ProcessIncomingFragment(INetSerializableStruct.Read<MessageFragment>(inc)).TryUnwrap(out completeMessage))
				{
					return;
				}
				int completeMessageLengthBits = completeMessage.Length * 8;
				this.incomingDataMessages.Add(new ReadWriteMessage(completeMessage.ToArray<byte>(), 0, completeMessageLengthBits, false));
				return;
			}
			else
			{
				if (packetHeader.IsHeartbeatMessage() || packetHeader.IsDoSProtectionMessage())
				{
					return;
				}
				if (packetHeader.IsDisconnectMessage())
				{
					PeerDisconnectPacket packet = INetSerializableStruct.Read<PeerDisconnectPacket>(inc);
					this.Close(packet);
					return;
				}
				PeerPacketMessage packet2 = INetSerializableStruct.Read<PeerPacketMessage>(inc);
				this.incomingDataMessages.Add(packet2.GetReadMessage(packetHeader.IsCompressed(), base.ServerConnection));
			}
		}

		// Token: 0x06004C02 RID: 19458 RVA: 0x0029E040 File Offset: 0x0029C240
		public override void Update(float deltaTime)
		{
			if (!this.isActive)
			{
				return;
			}
			if (GameMain.Client == null || !GameMain.Client.RoundStarting)
			{
				this.timeout -= (double)deltaTime;
			}
			this.heartbeatTimer -= (double)deltaTime;
			P2PSocket p2PSocket = this.socket;
			if (p2PSocket != null)
			{
				p2PSocket.ProcessIncomingMessages();
			}
			GameClient client = GameMain.Client;
			if (client != null)
			{
				NetStats netStats = client.NetStats;
				if (netStats != null)
				{
					netStats.AddValue(NetStats.NetStatType.ReceivedBytes, (float)this.receivedBytes);
				}
			}
			GameClient client2 = GameMain.Client;
			if (client2 != null)
			{
				NetStats netStats2 = client2.NetStats;
				if (netStats2 != null)
				{
					netStats2.AddValue(NetStats.NetStatType.SentBytes, (float)this.sentBytes);
				}
			}
			if (this.heartbeatTimer < 0.0)
			{
				PeerPacketHeaders headers = new PeerPacketHeaders
				{
					DeliveryMethod = DeliveryMethod.Unreliable,
					PacketHeader = PacketHeader.IsHeartbeatMessage,
					Initialization = null
				};
				this.SendMsgInternal(headers, null);
			}
			if (this.timeout < 0.0)
			{
				this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.SteamP2PTimeOut));
				return;
			}
			if (this.initializationStep != ConnectionInitialization.Success)
			{
				if (this.incomingDataMessages.Count > 0)
				{
					if (!base.ContentPackageOrderReceived)
					{
						this.<Update>g__initializationError|16_0("Error during connection initialization: completed initialization before receiving content package order.", "ContentPackageOrderNotReceived");
						return;
					}
					if (base.ServerContentPackages.Length == 0)
					{
						this.<Update>g__initializationError|16_0("Error during connection initialization: list of content packages enabled on the server was empty when completing initialization.", "NoContentPackages");
						return;
					}
					base.OnInitializationComplete();
				}
				else
				{
					foreach (ClientPeer.IncomingInitializationMessage inc in this.incomingInitializationMessages)
					{
						base.ReadConnectionInitializationStep(inc);
					}
				}
			}
			if (this.initializationStep == ConnectionInitialization.Success)
			{
				foreach (IReadMessage inc2 in this.incomingDataMessages)
				{
					this.callbacks.OnMessageReceived(inc2);
				}
			}
			this.incomingInitializationMessages.Clear();
			this.incomingDataMessages.Clear();
		}

		// Token: 0x06004C03 RID: 19459 RVA: 0x0029E250 File Offset: 0x0029C450
		public override void Send(IWriteMessage msg, DeliveryMethod deliveryMethod, bool compressPastThreshold = true)
		{
			P2PClientPeer.<>c__DisplayClass17_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (!this.isActive)
			{
				return;
			}
			bool isCompressed;
			int num;
			byte[] bufAux = msg.PrepareForSending(compressPastThreshold, out isCompressed, out num);
			PeerPacketHeaders peerPacketHeaders;
			if (bufAux.Length > 1100)
			{
				foreach (MessageFragment fragment in this.fragmenter.FragmentMessage(msg.Buffer.AsSpan<byte>().Slice(0, msg.LengthBytes)))
				{
					peerPacketHeaders = new PeerPacketHeaders
					{
						DeliveryMethod = DeliveryMethod.Reliable,
						PacketHeader = PacketHeader.IsDataFragment,
						Initialization = null
					};
					PeerPacketHeaders fragmentHeaders = peerPacketHeaders;
					this.SendMsgInternal(fragmentHeaders, fragment);
				}
				return;
			}
			peerPacketHeaders = new PeerPacketHeaders
			{
				DeliveryMethod = deliveryMethod,
				PacketHeader = (isCompressed ? PacketHeader.IsCompressed : PacketHeader.None),
				Initialization = null
			};
			CS$<>8__locals1.headers = peerPacketHeaders;
			CS$<>8__locals1.body = new PeerPacketMessage
			{
				Buffer = bufAux
			};
			this.heartbeatTimer = 5.0;
			this.<Send>g__performSend|17_0(ref CS$<>8__locals1);
		}

		// Token: 0x06004C04 RID: 19460 RVA: 0x0029E370 File Offset: 0x0029C570
		public override void SendPassword(string password)
		{
			if (!this.isActive)
			{
				return;
			}
			if (this.initializationStep != ConnectionInitialization.Password)
			{
				return;
			}
			PeerPacketHeaders headers = new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = PacketHeader.IsConnectionInitializationStep,
				Initialization = new ConnectionInitialization?(ConnectionInitialization.Password)
			};
			ClientPeerPasswordPacket body = new ClientPeerPasswordPacket
			{
				Password = ServerSettings.SaltPassword(Encoding.UTF8.GetBytes(password), this.passwordSalt)
			};
			this.SendMsgInternal(headers, body);
		}

		// Token: 0x06004C05 RID: 19461 RVA: 0x0029E3EC File Offset: 0x0029C5EC
		public override void Close(PeerDisconnectPacket peerDisconnectPacket)
		{
			if (!this.isActive)
			{
				return;
			}
			this.isActive = false;
			PeerPacketHeaders headers = new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = PacketHeader.IsDisconnectMessage,
				Initialization = null
			};
			this.SendMsgInternal(headers, peerDisconnectPacket);
			Thread.Sleep(100);
			P2PSocket p2PSocket = this.socket;
			if (p2PSocket != null)
			{
				p2PSocket.CloseConnection(base.ServerEndpoint);
			}
			P2PSocket p2PSocket2 = this.socket;
			if (p2PSocket2 != null)
			{
				p2PSocket2.Dispose();
			}
			this.socket = null;
			this.callbacks.OnDisconnect(peerDisconnectPacket);
		}

		// Token: 0x06004C06 RID: 19462 RVA: 0x0029E484 File Offset: 0x0029C684
		[NullableContext(2)]
		protected override void SendMsgInternal(PeerPacketHeaders headers, INetSerializableStruct body)
		{
			IWriteMessage msgToSend = new WriteOnlyMessage();
			msgToSend.WriteNetSerializableStruct(headers);
			if (body != null)
			{
				body.Write(msgToSend);
			}
			this.ForwardToRemotePeer(msgToSend, headers.DeliveryMethod);
		}

		// Token: 0x06004C07 RID: 19463 RVA: 0x0029E4B8 File Offset: 0x0029C6B8
		private void ForwardToRemotePeer(IWriteMessage msg, DeliveryMethod deliveryMethod)
		{
			if (!this.isActive)
			{
				return;
			}
			if (this.socket == null)
			{
				return;
			}
			this.heartbeatTimer = 5.0;
			int length = msg.LengthBytes;
			if (length + 4 >= 1170)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(84, 1);
				defaultInterpolatedStringHandler.AppendLiteral("WARNING: message length comes close to exceeding MTU, forcing reliable send (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(length);
				defaultInterpolatedStringHandler.AppendLiteral(" bytes)");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				deliveryMethod = DeliveryMethod.Reliable;
			}
			bool success = this.socket.SendMessage(base.ServerEndpoint, msg, deliveryMethod);
			this.sentBytes += (long)length;
			if (success)
			{
				return;
			}
			if (deliveryMethod == DeliveryMethod.Unreliable)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(76, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("WARNING: message couldn't be sent unreliably, forcing reliable send (");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(length);
				defaultInterpolatedStringHandler2.AppendLiteral(" bytes)");
				DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
				success = this.socket.SendMessage(base.ServerEndpoint, msg, DeliveryMethod.Reliable);
				this.sentBytes += (long)length;
			}
			if (!success)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(47, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Failed to send message to remote peer! (");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(length);
				defaultInterpolatedStringHandler3.AppendLiteral(" bytes)");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear(), null);
			}
		}

		// Token: 0x06004C08 RID: 19464 RVA: 0x0029E5F0 File Offset: 0x0029C7F0
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		protected override Task<Option<AccountId>> GetAccountId()
		{
			P2PClientPeer.<GetAccountId>d__22 <GetAccountId>d__;
			<GetAccountId>d__.<>t__builder = AsyncTaskMethodBuilder<Option<AccountId>>.Create();
			<GetAccountId>d__.<>1__state = -1;
			<GetAccountId>d__.<>t__builder.Start<P2PClientPeer.<GetAccountId>d__22>(ref <GetAccountId>d__);
			return <GetAccountId>d__.<>t__builder.Task;
		}

		// Token: 0x06004C0A RID: 19466 RVA: 0x0029E686 File Offset: 0x0029C886
		[CompilerGenerated]
		private void <Update>g__initializationError|16_0(string errorMsg, string analyticsTag)
		{
			GameAnalyticsManager.AddErrorEventOnce("SteamP2PClientPeer.OnInitializationComplete:" + analyticsTag, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			DebugConsole.ThrowError(errorMsg, null, null, false, false);
			this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.Disconnected));
		}

		// Token: 0x06004C0B RID: 19467 RVA: 0x0029E6B0 File Offset: 0x0029C8B0
		[CompilerGenerated]
		private void <Send>g__performSend|17_0(ref P2PClientPeer.<>c__DisplayClass17_0 A_1)
		{
			this.SendMsgInternal(A_1.headers, A_1.body);
		}

		// Token: 0x040027B9 RID: 10169
		private double timeout;

		// Token: 0x040027BA RID: 10170
		private double heartbeatTimer;

		// Token: 0x040027BB RID: 10171
		private long sentBytes;

		// Token: 0x040027BC RID: 10172
		private long receivedBytes;

		// Token: 0x040027BD RID: 10173
		private readonly List<ClientPeer.IncomingInitializationMessage> incomingInitializationMessages = new List<ClientPeer.IncomingInitializationMessage>();

		// Token: 0x040027BE RID: 10174
		private readonly List<IReadMessage> incomingDataMessages = new List<IReadMessage>();

		// Token: 0x040027BF RID: 10175
		private readonly MessageFragmenter fragmenter = new MessageFragmenter();

		// Token: 0x040027C0 RID: 10176
		private readonly MessageDefragmenter defragmenter = new MessageDefragmenter();

		// Token: 0x040027C1 RID: 10177
		[Nullable(2)]
		private P2PSocket socket;
	}
}
