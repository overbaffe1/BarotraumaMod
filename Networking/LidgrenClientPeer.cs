using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Barotrauma.Extensions;
using Lidgren.Network;

namespace Barotrauma.Networking
{
	// Token: 0x0200046A RID: 1130
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class LidgrenClientPeer : ClientPeer<LidgrenEndpoint>
	{
		// Token: 0x06004BF0 RID: 19440 RVA: 0x0029D504 File Offset: 0x0029B704
		[NullableContext(0)]
		public LidgrenClientPeer([Nullable(1)] LidgrenEndpoint endpoint, ClientPeer.Callbacks callbacks, Option<int> ownerKey) : base(endpoint, endpoint.ToEnumerable<Endpoint>().ToImmutableArray<Endpoint>(), callbacks, ownerKey)
		{
			base.ServerConnection = null;
			this.netClient = null;
			this.isActive = false;
			this.netPeerConfiguration = new NetPeerConfiguration("barotrauma")
			{
				DualStack = GameSettings.CurrentConfig.UseDualModeSockets
			};
			if (NetConfig.UseLenientHandshake)
			{
				this.netPeerConfiguration.ConnectionTimeout = 60f;
				this.netPeerConfiguration.ResendHandshakeInterval = 5f;
				this.netPeerConfiguration.MaximumHandshakeAttempts = 20;
			}
			if (endpoint.NetEndpoint.Address.AddressFamily == AddressFamily.InterNetworkV6)
			{
				this.netPeerConfiguration.LocalAddress = IPAddress.IPv6Any;
			}
			this.netPeerConfiguration.DisableMessageType((NetIncomingMessageType)1808);
			this.incomingLidgrenMessages = new List<NetIncomingMessage>();
		}

		// Token: 0x06004BF1 RID: 19441 RVA: 0x0029D5D0 File Offset: 0x0029B7D0
		public override void Start()
		{
			if (this.isActive)
			{
				return;
			}
			this.incomingLidgrenMessages.Clear();
			base.ContentPackageOrderReceived = false;
			this.netClient = new NetClient(this.netPeerConfiguration);
			this.initializationStep = ConnectionInitialization.AuthInfoAndVersion;
			LidgrenEndpoint lidgrenEndpointValue = base.ServerEndpoint;
			if (lidgrenEndpointValue == null)
			{
				throw new InvalidCastException("Endpoint is not LidgrenEndpoint");
			}
			if (base.ServerConnection != null)
			{
				throw new InvalidOperationException("ServerConnection is not null");
			}
			this.netClient.Start();
			TaskPool.Add("LidgrenClientPeer.GetAuthTicket", AuthenticationTicket.Create(base.ServerEndpoint), delegate(Task t)
			{
				Option<AuthenticationTicket> authenticationTicket;
				if (!t.TryGetResult(out authenticationTicket))
				{
					this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.AuthenticationFailed));
					return;
				}
				this.authTicket = authenticationTicket;
				NetConnection netConnection = this.netClient.Connect(lidgrenEndpointValue.NetEndpoint);
				this.ServerConnection = new LidgrenConnection(netConnection)
				{
					Status = NetworkConnectionStatus.Connected
				};
			});
			this.isActive = true;
		}

		// Token: 0x06004BF2 RID: 19442 RVA: 0x0029D684 File Offset: 0x0029B884
		public override void Update(float deltaTime)
		{
			if (!this.isActive)
			{
				return;
			}
			ToolBox.ThrowIfNull<NetClient>(this.netClient);
			ToolBox.ThrowIfNull<List<NetIncomingMessage>>(this.incomingLidgrenMessages);
			if (base.IsOwner && !ChildServerRelay.IsProcessAlive)
			{
				GameClient gameClient = GameMain.Client;
				this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.ServerCrashed));
				if (gameClient != null)
				{
					gameClient.CreateServerCrashMessage();
				}
				return;
			}
			this.incomingLidgrenMessages.Clear();
			this.netClient.ReadMessages(this.incomingLidgrenMessages);
			GameClient client = GameMain.Client;
			if (client != null)
			{
				NetStats netStats = client.NetStats;
				if (netStats != null)
				{
					netStats.AddValue(NetStats.NetStatType.ReceivedBytes, (float)this.netClient.Statistics.ReceivedBytes);
				}
			}
			GameClient client2 = GameMain.Client;
			if (client2 != null)
			{
				NetStats netStats2 = client2.NetStats;
				if (netStats2 != null)
				{
					netStats2.AddValue(NetStats.NetStatType.SentBytes, (float)this.netClient.Statistics.SentBytes);
				}
			}
			foreach (NetIncomingMessage inc in this.incomingLidgrenMessages)
			{
				LidgrenEndpoint remoteEndpoint = new LidgrenEndpoint(inc.SenderEndPoint);
				if (remoteEndpoint != base.ServerEndpoint)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Mismatched endpoint: expected ");
					defaultInterpolatedStringHandler.AppendFormatted<IPEndPoint>(base.ServerEndpoint.NetEndpoint);
					defaultInterpolatedStringHandler.AppendLiteral(", got ");
					defaultInterpolatedStringHandler.AppendFormatted<IPEndPoint>(inc.SenderConnection.RemoteEndPoint);
					DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
				else
				{
					NetIncomingMessageType messageType = inc.MessageType;
					if (messageType != NetIncomingMessageType.StatusChanged)
					{
						if (messageType == NetIncomingMessageType.Data)
						{
							this.HandleDataMessage(inc);
						}
					}
					else
					{
						this.HandleStatusChanged(inc);
					}
				}
			}
		}

		// Token: 0x06004BF3 RID: 19443 RVA: 0x0029D828 File Offset: 0x0029BA28
		private void HandleDataMessage(NetIncomingMessage lidgrenMsg)
		{
			if (!this.isActive)
			{
				return;
			}
			ToolBox.ThrowIfNull<NetworkConnection>(base.ServerConnection);
			IReadMessage inc = lidgrenMsg.ToReadMessage();
			DeliveryMethod deliveryMethod;
			PacketHeader packetHeader2;
			ConnectionInitialization? connectionInitialization;
			INetSerializableStruct.Read<PeerPacketHeaders>(inc).Deconstruct(out deliveryMethod, out packetHeader2, out connectionInitialization);
			PacketHeader packetHeader = packetHeader2;
			ConnectionInitialization? initialization = connectionInitialization;
			if (!packetHeader.IsConnectionInitializationStep())
			{
				base.OnInitializationComplete();
				PeerPacketMessage packet = INetSerializableStruct.Read<PeerPacketMessage>(inc);
				this.callbacks.OnMessageReceived(packet.GetReadMessage(packetHeader.IsCompressed(), base.ServerConnection));
				return;
			}
			if (this.initializationStep == ConnectionInitialization.Success)
			{
				return;
			}
			ClientPeer.IncomingInitializationMessage inc2 = default(ClientPeer.IncomingInitializationMessage);
			connectionInitialization = initialization;
			if (connectionInitialization == null)
			{
				throw new Exception("Initialization step missing");
			}
			inc2.InitializationStep = connectionInitialization.GetValueOrDefault();
			inc2.Message = inc;
			base.ReadConnectionInitializationStep(inc2);
		}

		// Token: 0x06004BF4 RID: 19444 RVA: 0x0029D8EC File Offset: 0x0029BAEC
		private void HandleStatusChanged(NetIncomingMessage inc)
		{
			if (!this.isActive)
			{
				return;
			}
			NetConnectionStatus status = inc.ReadHeader<NetConnectionStatus>();
			if (status == NetConnectionStatus.Disconnected)
			{
				string disconnectMsg = inc.ReadString();
				this.Close(PeerDisconnectPacket.FromLidgrenStringRepresentation(disconnectMsg).Fallback(PeerDisconnectPacket.WithReason(DisconnectReason.Unknown)));
			}
		}

		// Token: 0x06004BF5 RID: 19445 RVA: 0x0029D930 File Offset: 0x0029BB30
		public override void SendPassword(string password)
		{
			if (!this.isActive)
			{
				return;
			}
			ToolBox.ThrowIfNull<NetClient>(this.netClient);
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

		// Token: 0x06004BF6 RID: 19446 RVA: 0x0029D9B8 File Offset: 0x0029BBB8
		public override void Close(PeerDisconnectPacket peerDisconnectPacket)
		{
			if (!this.isActive)
			{
				return;
			}
			ToolBox.ThrowIfNull<NetClient>(this.netClient);
			this.isActive = false;
			this.netClient.Shutdown(peerDisconnectPacket.ToLidgrenStringRepresentation());
			this.netClient = null;
			this.callbacks.OnDisconnect(peerDisconnectPacket);
		}

		// Token: 0x06004BF7 RID: 19447 RVA: 0x0029DA0C File Offset: 0x0029BC0C
		public override void Send(IWriteMessage msg, DeliveryMethod deliveryMethod, bool compressPastThreshold = true)
		{
			if (!this.isActive)
			{
				return;
			}
			ToolBox.ThrowIfNull<NetClient>(this.netClient);
			ToolBox.ThrowIfNull<NetPeerConfiguration>(this.netPeerConfiguration);
			bool isCompressed;
			int num;
			byte[] bufAux = msg.PrepareForSending(compressPastThreshold, out isCompressed, out num);
			PeerPacketHeaders headers = new PeerPacketHeaders
			{
				DeliveryMethod = deliveryMethod,
				PacketHeader = (isCompressed ? PacketHeader.IsCompressed : PacketHeader.None),
				Initialization = null
			};
			PeerPacketMessage body = new PeerPacketMessage
			{
				Buffer = bufAux
			};
			this.SendMsgInternal(headers, body);
		}

		// Token: 0x06004BF8 RID: 19448 RVA: 0x0029DA94 File Offset: 0x0029BC94
		[NullableContext(2)]
		protected override void SendMsgInternal(PeerPacketHeaders headers, INetSerializableStruct body)
		{
			ToolBox.ThrowIfNull<NetClient>(this.netClient);
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteNetSerializableStruct(headers);
			if (body != null)
			{
				body.Write(msg);
			}
			NetSendResult result = this.ForwardToLidgren(msg, DeliveryMethod.Reliable);
			if (result != NetSendResult.Queued && result != NetSendResult.Sent)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to send message to host: ");
				defaultInterpolatedStringHandler.AppendFormatted<NetSendResult>(result);
				defaultInterpolatedStringHandler.AppendLiteral("\n");
				defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace);
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
			}
		}

		// Token: 0x06004BF9 RID: 19449 RVA: 0x0029DB20 File Offset: 0x0029BD20
		private NetSendResult ForwardToLidgren(IWriteMessage msg, DeliveryMethod deliveryMethod)
		{
			ToolBox.ThrowIfNull<NetClient>(this.netClient);
			return this.netClient.SendMessage(msg.ToLidgren(this.netClient), deliveryMethod.ToLidgren());
		}

		// Token: 0x06004BFA RID: 19450 RVA: 0x0029DB4C File Offset: 0x0029BD4C
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		protected override Task<Option<AccountId>> GetAccountId()
		{
			LidgrenClientPeer.<GetAccountId>d__13 <GetAccountId>d__;
			<GetAccountId>d__.<>t__builder = AsyncTaskMethodBuilder<Option<AccountId>>.Create();
			<GetAccountId>d__.<>1__state = -1;
			<GetAccountId>d__.<>t__builder.Start<LidgrenClientPeer.<GetAccountId>d__13>(ref <GetAccountId>d__);
			return <GetAccountId>d__.<>t__builder.Task;
		}

		// Token: 0x040027B6 RID: 10166
		[Nullable(2)]
		private NetClient netClient;

		// Token: 0x040027B7 RID: 10167
		private readonly NetPeerConfiguration netPeerConfiguration;

		// Token: 0x040027B8 RID: 10168
		private readonly List<NetIncomingMessage> incomingLidgrenMessages;
	}
}
