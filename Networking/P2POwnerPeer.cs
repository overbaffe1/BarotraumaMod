using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Barotrauma.Extensions;

namespace Barotrauma.Networking
{
	// Token: 0x0200046C RID: 1132
	[NullableContext(1)]
	[Nullable(new byte[]
	{
		0,
		1
	})]
	internal sealed class P2POwnerPeer : ClientPeer<PipeEndpoint>
	{
		// Token: 0x06004C0C RID: 19468 RVA: 0x0029E6CC File Offset: 0x0029C8CC
		public P2POwnerPeer(ClientPeer.Callbacks callbacks, int ownerKey, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<P2PEndpoint> allEndpoints) : base(new PipeEndpoint(), allEndpoints.Cast<Endpoint>().ToImmutableArray<Endpoint>(), callbacks, Option<int>.Some(ownerKey))
		{
			base.ServerConnection = null;
			this.isActive = false;
			Option<P2PEndpoint> selfSteamEndpoint = allEndpoints.FirstOrNone((P2PEndpoint e) => e is SteamP2PEndpoint);
			Option<P2PEndpoint> selfEosEndpoint = allEndpoints.FirstOrNone((P2PEndpoint e) => e is EosP2PEndpoint);
			P2PEndpoint selfPrimaryEndpointNotNull;
			if (!selfSteamEndpoint.Fallback(selfEosEndpoint).TryUnwrap(out selfPrimaryEndpointNotNull))
			{
				throw new Exception("Could not determine endpoint for P2POwnerPeer");
			}
			this.selfPrimaryEndpoint = selfPrimaryEndpointNotNull;
			this.selfAccountInfo = AccountInfo.None;
			this.authenticators = Authenticator.GetAuthenticatorsForHost(Option.Some<Endpoint>(this.selfPrimaryEndpoint));
		}

		// Token: 0x06004C0D RID: 19469 RVA: 0x0029E7B4 File Offset: 0x0029C9B4
		public override void Start()
		{
			if (this.isActive)
			{
				return;
			}
			this.initializationStep = ConnectionInitialization.AuthInfoAndVersion;
			Option.UnspecifiedNone none = Option.None;
			base.ServerConnection = new PipeConnection(none)
			{
				Status = NetworkConnectionStatus.Connected
			};
			this.remotePeers.Clear();
			P2PSocket.Callbacks socketCallbacks = new P2PSocket.Callbacks(new Predicate<P2PEndpoint>(this.OnIncomingConnection), new Action<P2PEndpoint, PeerDisconnectPacket>(this.OnConnectionClosed), new P2POwnerDoSProtection.ExcessivePacketDelegate(this.OnExcessivePackets), new Action<P2PEndpoint, IReadMessage>(this.OnP2PData));
			Result<P2PSocket, P2PSocket.Error> socketCreateResult = DualStackP2PSocket.Create(socketCallbacks, P2PSocket.OwnerOrClient.Owner);
			P2PSocket s;
			if (!socketCreateResult.TryUnwrapSuccess(out s))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to create dual-stack socket: ");
				defaultInterpolatedStringHandler.AppendFormatted<Result<P2PSocket, P2PSocket.Error>>(socketCreateResult);
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.socket = s;
			TaskPool.Add("P2POwnerPeer.GetAccountId", this.GetAccountId(), delegate(Task t)
			{
				Option<AccountId> accountIdOption;
				AccountId accountId;
				if (t.TryGetResult(out accountIdOption) && accountIdOption.TryUnwrap(out accountId))
				{
					this.selfAccountInfo = new AccountInfo(accountId, Array.Empty<AccountId>());
				}
				if (this.selfAccountInfo.IsNone)
				{
					this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.AuthenticationFailed));
				}
			});
			this.isActive = true;
		}

		// Token: 0x06004C0E RID: 19470 RVA: 0x0029E89C File Offset: 0x0029CA9C
		private bool OnIncomingConnection(P2PEndpoint remoteEndpoint)
		{
			if (!this.isActive)
			{
				return false;
			}
			if (this.remotePeers.None((P2POwnerPeer.RemotePeer p) => p.Endpoint == remoteEndpoint))
			{
				this.remotePeers.Add(new P2POwnerPeer.RemotePeer(remoteEndpoint));
			}
			return true;
		}

		// Token: 0x06004C0F RID: 19471 RVA: 0x0029E8F0 File Offset: 0x0029CAF0
		private void OnConnectionClosed(P2PEndpoint remoteEndpoint, PeerDisconnectPacket disconnectPacket)
		{
			P2POwnerPeer.RemotePeer remotePeer = this.remotePeers.Find((P2POwnerPeer.RemotePeer p) => p.Endpoint == remoteEndpoint);
			if (remotePeer == null)
			{
				return;
			}
			this.CommunicatePeerDisconnectToServerProcess(remotePeer, (from d in remotePeer.PendingDisconnect
			select d.Packet).Fallback(disconnectPacket));
		}

		// Token: 0x06004C10 RID: 19472 RVA: 0x0029E960 File Offset: 0x0029CB60
		private void OnP2PData(P2PEndpoint senderEndpoint, IReadMessage inc)
		{
			if (!this.isActive)
			{
				return;
			}
			this.receivedBytes += (long)inc.LengthBytes;
			P2POwnerPeer.RemotePeer remotePeer = this.remotePeers.Find((P2POwnerPeer.RemotePeer p) => p.Endpoint == senderEndpoint);
			if (remotePeer == null)
			{
				return;
			}
			if (remotePeer.PendingDisconnect.IsSome())
			{
				return;
			}
			PeerPacketHeaders peerPacketHeaders;
			if (!INetSerializableStruct.TryRead<PeerPacketHeaders>(inc, remotePeer.AccountInfo, out peerPacketHeaders))
			{
				this.CommunicateDisconnectToRemotePeer(remotePeer, PeerDisconnectPacket.WithReason(DisconnectReason.MalformedData));
				return;
			}
			PacketHeader packetHeader = peerPacketHeaders.PacketHeader;
			if (packetHeader.IsConnectionInitializationStep())
			{
				if (peerPacketHeaders.Initialization == null)
				{
					DebugConsole.ThrowErrorOnce("P2POwnerPeer.OnP2PData:" + remotePeer.Endpoint.StringRepresentation, "Failed to initialize remote peer " + remotePeer.Endpoint.StringRepresentation + ": initialization step missing.", null);
					this.CommunicateDisconnectToRemotePeer(remotePeer, PeerDisconnectPacket.WithReason(DisconnectReason.MalformedData));
					return;
				}
				ConnectionInitialization initialization = peerPacketHeaders.Initialization.Value;
				if (initialization == ConnectionInitialization.AuthInfoAndVersion && remotePeer.AuthStatus == P2POwnerPeer.RemotePeer.AuthenticationStatus.NotAuthenticated)
				{
					this.StartAuthTask(inc, remotePeer);
				}
			}
			if (remotePeer.AuthStatus == P2POwnerPeer.RemotePeer.AuthenticationStatus.AuthenticationPending)
			{
				remotePeer.UnauthedMessages.Add(new P2POwnerPeer.RemotePeer.UnauthedMessage(inc.Buffer, inc.LengthBytes));
				return;
			}
			IWriteMessage outMsg = new WriteOnlyMessage();
			outMsg.WriteNetSerializableStruct(new P2POwnerToServerHeader
			{
				EndpointStr = remotePeer.Endpoint.StringRepresentation,
				AccountInfo = remotePeer.AccountInfo
			});
			outMsg.WriteBytes(inc.Buffer, 0, inc.LengthBytes);
			P2POwnerPeer.ForwardToServerProcess(outMsg);
		}

		// Token: 0x06004C11 RID: 19473 RVA: 0x0029EADC File Offset: 0x0029CCDC
		private void OnExcessivePackets(P2PEndpoint endpoint, bool shouldBan)
		{
			IWriteMessage msg = new WriteOnlyMessage();
			msg.WriteNetSerializableStruct(new P2POwnerToServerHeader
			{
				EndpointStr = this.selfPrimaryEndpoint.StringRepresentation,
				AccountInfo = this.selfAccountInfo
			});
			msg.WriteNetSerializableStruct(new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = PacketHeader.IsDoSProtectionMessage
			});
			msg.WriteNetSerializableStruct(new DoSProtectionPacket(endpoint.StringRepresentation, shouldBan));
			string dcMsg = TextManager.Get(shouldBan ? "DoSProtectionBanned" : "DoSProtectionKicked").Fallback(TextManager.Get("DoSProtectionKicked"), true).Value;
			msg.WriteNetSerializableStruct(shouldBan ? PeerDisconnectPacket.Banned(dcMsg) : PeerDisconnectPacket.Kicked(dcMsg));
			P2POwnerPeer.ForwardToServerProcess(msg);
		}

		// Token: 0x06004C12 RID: 19474 RVA: 0x0029EB98 File Offset: 0x0029CD98
		private void StartAuthTask(IReadMessage inc, P2POwnerPeer.RemotePeer remotePeer)
		{
			P2POwnerPeer.<>c__DisplayClass14_0 CS$<>8__locals1 = new P2POwnerPeer.<>c__DisplayClass14_0();
			CS$<>8__locals1.remotePeer = remotePeer;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.remotePeer.AuthStatus = P2POwnerPeer.RemotePeer.AuthenticationStatus.AuthenticationPending;
			ClientAuthTicketAndVersionPacket packet;
			if (!INetSerializableStruct.TryRead<ClientAuthTicketAndVersionPacket>(inc, CS$<>8__locals1.remotePeer.AccountInfo, out packet))
			{
				CS$<>8__locals1.<StartAuthTask>g__failAuth|1();
				return;
			}
			AuthenticationTicket authenticationTicket;
			if (!packet.AuthTicket.TryUnwrap(out authenticationTicket))
			{
				CS$<>8__locals1.<StartAuthTask>g__failAuth|1();
				return;
			}
			Authenticator authenticator;
			if (!this.authenticators.TryGetValue(authenticationTicket.Kind, out authenticator))
			{
				CS$<>8__locals1.<StartAuthTask>g__failAuth|1();
				return;
			}
			TaskPool.Add("P2POwnerPeer.VerifyRemotePeerAccountId", authenticator.VerifyTicket(authenticationTicket), delegate(Task t)
			{
				AccountInfo accountInfo;
				if (!t.TryGetResult(out accountInfo) || accountInfo.IsNone)
				{
					base.<StartAuthTask>g__failAuth|1();
					return;
				}
				CS$<>8__locals1.remotePeer.AccountInfo = accountInfo;
				CS$<>8__locals1.remotePeer.AuthStatus = P2POwnerPeer.RemotePeer.AuthenticationStatus.SuccessfullyAuthenticated;
				foreach (P2POwnerPeer.RemotePeer.UnauthedMessage unauthedMessage in CS$<>8__locals1.remotePeer.UnauthedMessages)
				{
					IWriteMessage msg = new WriteOnlyMessage();
					msg.WriteNetSerializableStruct(new P2POwnerToServerHeader
					{
						EndpointStr = CS$<>8__locals1.remotePeer.Endpoint.StringRepresentation,
						AccountInfo = accountInfo
					});
					msg.WriteBytes(unauthedMessage.Bytes, 0, unauthedMessage.LengthBytes);
					P2POwnerPeer.ForwardToServerProcess(msg);
				}
				CS$<>8__locals1.remotePeer.UnauthedMessages.Clear();
			});
		}

		// Token: 0x06004C13 RID: 19475 RVA: 0x0029EC34 File Offset: 0x0029CE34
		public override void Update(float deltaTime)
		{
			if (!this.isActive)
			{
				return;
			}
			if (!ChildServerRelay.HasShutDown)
			{
				Process process = ChildServerRelay.Process;
				if (process != null && !process.HasExited)
				{
					if (this.selfAccountInfo.IsNone)
					{
						return;
					}
					for (int i = this.remotePeers.Count - 1; i >= 0; i--)
					{
						P2POwnerPeer.RemotePeer.DisconnectInfo pendingDisconnect;
						if (this.remotePeers[i].PendingDisconnect.TryUnwrap(out pendingDisconnect) && pendingDisconnect.TimeToGiveUp < Timing.TotalTime)
						{
							this.CommunicatePeerDisconnectToServerProcess(this.remotePeers[i], pendingDisconnect.Packet);
						}
					}
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
					foreach (byte[] incBuf in ChildServerRelay.Read())
					{
						ChildServerRelay.DisposeLocalHandles();
						IReadMessage inc = new ReadOnlyMessage(incBuf, false, 0, incBuf.Length, base.ServerConnection);
						this.HandleServerMessage(inc);
					}
					return;
				}
			}
			this.Close(PeerDisconnectPacket.WithReason(DisconnectReason.ServerCrashed));
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("ConnectionLost"), ChildServerRelay.CrashMessage, null, null, GUIMessageBox.Type.Default);
			GUIButton guibutton = msgBox.Buttons[0];
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object obj)
			{
				GameMain.MainMenuScreen.Select();
				return false;
			}));
		}

		// Token: 0x06004C14 RID: 19476 RVA: 0x0029EDFC File Offset: 0x0029CFFC
		private void HandleServerMessage(IReadMessage inc)
		{
			if (!this.isActive)
			{
				return;
			}
			P2PEndpoint recipientEndpoint;
			if (!INetSerializableStruct.Read<P2PServerToOwnerHeader>(inc).Endpoint.TryUnwrap(out recipientEndpoint))
			{
				return;
			}
			PeerPacketHeaders peerPacketHeaders = INetSerializableStruct.Read<PeerPacketHeaders>(inc);
			if (recipientEndpoint != this.selfPrimaryEndpoint)
			{
				this.HandleMessageForRemotePeer(peerPacketHeaders, recipientEndpoint, inc);
				return;
			}
			this.HandleMessageForOwner(peerPacketHeaders, inc);
		}

		// Token: 0x06004C15 RID: 19477 RVA: 0x0029EE54 File Offset: 0x0029D054
		private static byte[] GetRemainingBytes(IReadMessage msg)
		{
			return RuntimeHelpers.GetSubArray<byte>(msg.Buffer, new Range(msg.BytePosition, msg.LengthBytes));
		}

		// Token: 0x06004C16 RID: 19478 RVA: 0x0029EE7C File Offset: 0x0029D07C
		private void HandleMessageForRemotePeer(PeerPacketHeaders peerPacketHeaders, P2PEndpoint recipientEndpoint, IReadMessage inc)
		{
			PeerPacketHeaders peerPacketHeaders2 = peerPacketHeaders;
			DeliveryMethod deliveryMethod2;
			PacketHeader packetHeader2;
			ConnectionInitialization? connectionInitialization;
			peerPacketHeaders2.Deconstruct(out deliveryMethod2, out packetHeader2, out connectionInitialization);
			DeliveryMethod deliveryMethod = deliveryMethod2;
			PacketHeader packetHeader = packetHeader2;
			ConnectionInitialization? initialization = connectionInitialization;
			if (!packetHeader.IsServerMessage())
			{
				DebugConsole.ThrowError("Received non-server message meant for remote peer", null, null, false, false);
				return;
			}
			P2POwnerPeer.RemotePeer peer = this.remotePeers.Find((P2POwnerPeer.RemotePeer p) => p.Endpoint == recipientEndpoint);
			if (peer == null)
			{
				return;
			}
			if (packetHeader.IsDisconnectMessage())
			{
				PeerDisconnectPacket packet = INetSerializableStruct.Read<PeerDisconnectPacket>(inc);
				this.CommunicateDisconnectToRemotePeer(peer, packet);
				return;
			}
			IWriteMessage outMsg = new WriteOnlyMessage();
			outMsg.WriteNetSerializableStruct(new PeerPacketHeaders
			{
				DeliveryMethod = deliveryMethod,
				PacketHeader = packetHeader,
				Initialization = initialization
			});
			if (packetHeader.IsConnectionInitializationStep())
			{
				P2PInitializationRelayPacket initRelayPacket = new P2PInitializationRelayPacket
				{
					LobbyID = 0UL,
					Message = new PeerPacketMessage
					{
						Buffer = P2POwnerPeer.GetRemainingBytes(inc)
					}
				};
				outMsg.WriteNetSerializableStruct(initRelayPacket);
			}
			else
			{
				byte[] userMessage = P2POwnerPeer.GetRemainingBytes(inc);
				outMsg.WriteBytes(userMessage, 0, userMessage.Length);
			}
			this.ForwardToRemotePeer(deliveryMethod, recipientEndpoint, outMsg);
		}

		// Token: 0x06004C17 RID: 19479 RVA: 0x0029EF9C File Offset: 0x0029D19C
		private void HandleMessageForOwner(PeerPacketHeaders peerPacketHeaders, IReadMessage inc)
		{
			PeerPacketHeaders peerPacketHeaders2 = peerPacketHeaders;
			DeliveryMethod deliveryMethod;
			PacketHeader packetHeader2;
			ConnectionInitialization? connectionInitialization;
			peerPacketHeaders2.Deconstruct(out deliveryMethod, out packetHeader2, out connectionInitialization);
			PacketHeader packetHeader = packetHeader2;
			if (packetHeader.IsDisconnectMessage())
			{
				DebugConsole.ThrowError("Received disconnect message from owned server", null, null, false, false);
				return;
			}
			if (!packetHeader.IsServerMessage())
			{
				DebugConsole.ThrowError("Received non-server message from owned server", null, null, false, false);
				return;
			}
			if (packetHeader.IsHeartbeatMessage())
			{
				return;
			}
			if (!packetHeader.IsConnectionInitializationStep())
			{
				base.OnInitializationComplete();
				PeerPacketMessage packet = INetSerializableStruct.Read<PeerPacketMessage>(inc);
				IReadMessage msg = new ReadOnlyMessage(packet.Buffer, packetHeader.IsCompressed(), 0, packet.Length, base.ServerConnection);
				this.callbacks.OnMessageReceived(msg);
				return;
			}
			if (this.selfAccountInfo.IsNone)
			{
				throw new InvalidOperationException("Cannot initialize P2POwnerPeer because selfAccountInfo is not defined");
			}
			IWriteMessage outMsg = new WriteOnlyMessage();
			outMsg.WriteNetSerializableStruct(new P2POwnerToServerHeader
			{
				EndpointStr = this.selfPrimaryEndpoint.StringRepresentation,
				AccountInfo = this.selfAccountInfo
			});
			outMsg.WriteNetSerializableStruct(new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = PacketHeader.IsConnectionInitializationStep,
				Initialization = new ConnectionInitialization?(ConnectionInitialization.AuthInfoAndVersion)
			});
			outMsg.WriteNetSerializableStruct(new P2PInitializationOwnerPacket(GameMain.Client.Name, this.selfAccountInfo.AccountId.Fallback(null)));
			P2POwnerPeer.ForwardToServerProcess(outMsg);
		}

		// Token: 0x06004C18 RID: 19480 RVA: 0x0029F0EC File Offset: 0x0029D2EC
		private void CommunicateDisconnectToRemotePeer(P2POwnerPeer.RemotePeer peer, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (peer.PendingDisconnect.IsNone())
			{
				peer.PendingDisconnect = Option.Some<P2POwnerPeer.RemotePeer.DisconnectInfo>(new P2POwnerPeer.RemotePeer.DisconnectInfo(Timing.TotalTime + 3.0, peerDisconnectPacket));
			}
			IWriteMessage outMsg = new WriteOnlyMessage();
			outMsg.WriteNetSerializableStruct(new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = (PacketHeader.IsDisconnectMessage | PacketHeader.IsServerMessage)
			});
			outMsg.WriteNetSerializableStruct(peerDisconnectPacket);
			this.ForwardToRemotePeer(DeliveryMethod.Reliable, peer.Endpoint, outMsg);
		}

		// Token: 0x06004C19 RID: 19481 RVA: 0x0029F164 File Offset: 0x0029D364
		private void CommunicatePeerDisconnectToServerProcess(P2POwnerPeer.RemotePeer peer, PeerDisconnectPacket peerDisconnectPacket)
		{
			if (!this.remotePeers.Remove(peer))
			{
				return;
			}
			IWriteMessage outMsg = new WriteOnlyMessage();
			outMsg.WriteNetSerializableStruct(new P2POwnerToServerHeader
			{
				EndpointStr = peer.Endpoint.StringRepresentation,
				AccountInfo = peer.AccountInfo
			});
			outMsg.WriteNetSerializableStruct(new PeerPacketHeaders
			{
				DeliveryMethod = DeliveryMethod.Reliable,
				PacketHeader = PacketHeader.IsDisconnectMessage
			});
			outMsg.WriteNetSerializableStruct(peerDisconnectPacket);
			AccountId accountId;
			if (peer.AccountInfo.AccountId.TryUnwrap(out accountId))
			{
				this.authenticators.Values.ForEach(delegate(Authenticator authenticator)
				{
					authenticator.EndAuthSession(accountId);
				});
			}
			P2POwnerPeer.ForwardToServerProcess(outMsg);
			P2PSocket p2PSocket = this.socket;
			if (p2PSocket == null)
			{
				return;
			}
			p2PSocket.CloseConnection(peer.Endpoint);
		}

		// Token: 0x06004C1A RID: 19482 RVA: 0x0029F230 File Offset: 0x0029D430
		public override void SendPassword(string password)
		{
		}

		// Token: 0x06004C1B RID: 19483 RVA: 0x0029F234 File Offset: 0x0029D434
		public override void Close(PeerDisconnectPacket peerDisconnectPacket)
		{
			if (!this.isActive)
			{
				return;
			}
			this.isActive = false;
			for (int i = this.remotePeers.Count - 1; i >= 0; i--)
			{
				this.CommunicateDisconnectToRemotePeer(this.remotePeers[i], peerDisconnectPacket);
			}
			Thread.Sleep(100);
			for (int j = this.remotePeers.Count - 1; j >= 0; j--)
			{
				this.CommunicatePeerDisconnectToServerProcess(this.remotePeers[j], peerDisconnectPacket);
			}
			P2PSocket p2PSocket = this.socket;
			if (p2PSocket != null)
			{
				p2PSocket.Dispose();
			}
			this.socket = null;
			this.callbacks.OnDisconnect(peerDisconnectPacket);
		}

		// Token: 0x06004C1C RID: 19484 RVA: 0x0029F2D8 File Offset: 0x0029D4D8
		public override void Send(IWriteMessage msg, DeliveryMethod deliveryMethod, bool compressPastThreshold = true)
		{
			if (!this.isActive)
			{
				return;
			}
			IWriteMessage msgToSend = new WriteOnlyMessage();
			bool isCompressed;
			int num;
			byte[] msgData = msg.PrepareForSending(compressPastThreshold, out isCompressed, out num);
			msgToSend.WriteNetSerializableStruct(new P2POwnerToServerHeader
			{
				EndpointStr = this.selfPrimaryEndpoint.StringRepresentation,
				AccountInfo = this.selfAccountInfo
			});
			msgToSend.WriteNetSerializableStruct(new PeerPacketHeaders
			{
				DeliveryMethod = deliveryMethod,
				PacketHeader = (isCompressed ? PacketHeader.IsCompressed : PacketHeader.None)
			});
			msgToSend.WriteNetSerializableStruct(new PeerPacketMessage
			{
				Buffer = msgData
			});
			P2POwnerPeer.ForwardToServerProcess(msgToSend);
		}

		// Token: 0x06004C1D RID: 19485 RVA: 0x0029F373 File Offset: 0x0029D573
		[NullableContext(2)]
		protected override void SendMsgInternal(PeerPacketHeaders headers, INetSerializableStruct body)
		{
			throw new NotImplementedException();
		}

		// Token: 0x06004C1E RID: 19486 RVA: 0x0029F37C File Offset: 0x0029D57C
		private static void ForwardToServerProcess(IWriteMessage msg)
		{
			byte[] bufToSend = new byte[msg.LengthBytes];
			RuntimeHelpers.GetSubArray<byte>(msg.Buffer, Range.EndAt(msg.LengthBytes)).CopyTo(bufToSend.AsSpan<byte>());
			ChildServerRelay.Write(bufToSend);
		}

		// Token: 0x06004C1F RID: 19487 RVA: 0x0029F3C4 File Offset: 0x0029D5C4
		private void ForwardToRemotePeer(DeliveryMethod deliveryMethod, P2PEndpoint recipient, IWriteMessage outMsg)
		{
			if (this.socket == null)
			{
				return;
			}
			int length = outMsg.LengthBytes;
			if (length + 4 >= 1170)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(84, 1);
				defaultInterpolatedStringHandler.AppendLiteral("WARNING: message length comes close to exceeding MTU, forcing reliable send (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(length);
				defaultInterpolatedStringHandler.AppendLiteral(" bytes)");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
				deliveryMethod = DeliveryMethod.Reliable;
			}
			bool success = this.socket.SendMessage(recipient, outMsg, deliveryMethod);
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
				success = this.socket.SendMessage(recipient, outMsg, DeliveryMethod.Reliable);
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

		// Token: 0x06004C20 RID: 19488 RVA: 0x0029F4D8 File Offset: 0x0029D6D8
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		protected override Task<Option<AccountId>> GetAccountId()
		{
			P2POwnerPeer.<GetAccountId>d__28 <GetAccountId>d__;
			<GetAccountId>d__.<>t__builder = AsyncTaskMethodBuilder<Option<AccountId>>.Create();
			<GetAccountId>d__.<>1__state = -1;
			<GetAccountId>d__.<>t__builder.Start<P2POwnerPeer.<GetAccountId>d__28>(ref <GetAccountId>d__);
			return <GetAccountId>d__.<>t__builder.Task;
		}

		// Token: 0x040027C2 RID: 10178
		[Nullable(2)]
		private P2PSocket socket;

		// Token: 0x040027C3 RID: 10179
		private readonly ImmutableDictionary<AuthenticationTicketKind, Authenticator> authenticators;

		// Token: 0x040027C4 RID: 10180
		private readonly P2PEndpoint selfPrimaryEndpoint;

		// Token: 0x040027C5 RID: 10181
		private AccountInfo selfAccountInfo;

		// Token: 0x040027C6 RID: 10182
		private long sentBytes;

		// Token: 0x040027C7 RID: 10183
		private long receivedBytes;

		// Token: 0x040027C8 RID: 10184
		private readonly List<P2POwnerPeer.RemotePeer> remotePeers = new List<P2POwnerPeer.RemotePeer>();

		// Token: 0x020011FD RID: 4605
		[NullableContext(0)]
		private sealed class RemotePeer
		{
			// Token: 0x060092D8 RID: 37592 RVA: 0x003CA45C File Offset: 0x003C865C
			[NullableContext(1)]
			public RemotePeer(P2PEndpoint endpoint)
			{
				this.Endpoint = endpoint;
				this.AccountInfo = AccountInfo.None;
				Option.UnspecifiedNone none = Option.None;
				this.PendingDisconnect = none;
				this.AuthStatus = P2POwnerPeer.RemotePeer.AuthenticationStatus.NotAuthenticated;
				this.UnauthedMessages = new List<P2POwnerPeer.RemotePeer.UnauthedMessage>();
			}

			// Token: 0x04005DD0 RID: 24016
			[Nullable(1)]
			public readonly P2PEndpoint Endpoint;

			// Token: 0x04005DD1 RID: 24017
			public AccountInfo AccountInfo;

			// Token: 0x04005DD2 RID: 24018
			public Option<P2POwnerPeer.RemotePeer.DisconnectInfo> PendingDisconnect;

			// Token: 0x04005DD3 RID: 24019
			public P2POwnerPeer.RemotePeer.AuthenticationStatus AuthStatus;

			// Token: 0x04005DD4 RID: 24020
			[Nullable(1)]
			public readonly List<P2POwnerPeer.RemotePeer.UnauthedMessage> UnauthedMessages;

			// Token: 0x020015C4 RID: 5572
			public enum AuthenticationStatus
			{
				// Token: 0x040069C1 RID: 27073
				NotAuthenticated,
				// Token: 0x040069C2 RID: 27074
				AuthenticationPending,
				// Token: 0x040069C3 RID: 27075
				SuccessfullyAuthenticated
			}

			// Token: 0x020015C5 RID: 5573
			public readonly struct DisconnectInfo : IEquatable<P2POwnerPeer.RemotePeer.DisconnectInfo>
			{
				// Token: 0x06009EEE RID: 40686 RVA: 0x003F32CD File Offset: 0x003F14CD
				public DisconnectInfo(double TimeToGiveUp, PeerDisconnectPacket Packet)
				{
					this.TimeToGiveUp = TimeToGiveUp;
					this.Packet = Packet;
				}

				// Token: 0x17001DB4 RID: 7604
				// (get) Token: 0x06009EEF RID: 40687 RVA: 0x003F32DD File Offset: 0x003F14DD
				// (set) Token: 0x06009EF0 RID: 40688 RVA: 0x003F32E5 File Offset: 0x003F14E5
				public double TimeToGiveUp { get; set; }

				// Token: 0x17001DB5 RID: 7605
				// (get) Token: 0x06009EF1 RID: 40689 RVA: 0x003F32EE File Offset: 0x003F14EE
				// (set) Token: 0x06009EF2 RID: 40690 RVA: 0x003F32F6 File Offset: 0x003F14F6
				public PeerDisconnectPacket Packet { get; set; }

				// Token: 0x06009EF3 RID: 40691 RVA: 0x003F3300 File Offset: 0x003F1500
				[CompilerGenerated]
				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("DisconnectInfo");
					stringBuilder.Append(" { ");
					if (this.PrintMembers(stringBuilder))
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append('}');
					return stringBuilder.ToString();
				}

				// Token: 0x06009EF4 RID: 40692 RVA: 0x003F334C File Offset: 0x003F154C
				[CompilerGenerated]
				private bool PrintMembers(StringBuilder builder)
				{
					builder.Append("TimeToGiveUp = ");
					builder.Append(this.TimeToGiveUp.ToString());
					builder.Append(", Packet = ");
					builder.Append(this.Packet.ToString());
					return true;
				}

				// Token: 0x06009EF5 RID: 40693 RVA: 0x003F33A8 File Offset: 0x003F15A8
				[CompilerGenerated]
				public static bool operator !=(P2POwnerPeer.RemotePeer.DisconnectInfo left, P2POwnerPeer.RemotePeer.DisconnectInfo right)
				{
					return !(left == right);
				}

				// Token: 0x06009EF6 RID: 40694 RVA: 0x003F33B4 File Offset: 0x003F15B4
				[CompilerGenerated]
				public static bool operator ==(P2POwnerPeer.RemotePeer.DisconnectInfo left, P2POwnerPeer.RemotePeer.DisconnectInfo right)
				{
					return left.Equals(right);
				}

				// Token: 0x06009EF7 RID: 40695 RVA: 0x003F33BE File Offset: 0x003F15BE
				[CompilerGenerated]
				public override int GetHashCode()
				{
					return EqualityComparer<double>.Default.GetHashCode(this.<TimeToGiveUp>k__BackingField) * -1521134295 + EqualityComparer<PeerDisconnectPacket>.Default.GetHashCode(this.<Packet>k__BackingField);
				}

				// Token: 0x06009EF8 RID: 40696 RVA: 0x003F33E7 File Offset: 0x003F15E7
				[CompilerGenerated]
				public override bool Equals(object obj)
				{
					return obj is P2POwnerPeer.RemotePeer.DisconnectInfo && this.Equals((P2POwnerPeer.RemotePeer.DisconnectInfo)obj);
				}

				// Token: 0x06009EF9 RID: 40697 RVA: 0x003F33FF File Offset: 0x003F15FF
				[CompilerGenerated]
				public bool Equals(P2POwnerPeer.RemotePeer.DisconnectInfo other)
				{
					return EqualityComparer<double>.Default.Equals(this.<TimeToGiveUp>k__BackingField, other.<TimeToGiveUp>k__BackingField) && EqualityComparer<PeerDisconnectPacket>.Default.Equals(this.<Packet>k__BackingField, other.<Packet>k__BackingField);
				}

				// Token: 0x06009EFA RID: 40698 RVA: 0x003F3431 File Offset: 0x003F1631
				[CompilerGenerated]
				public void Deconstruct(out double TimeToGiveUp, out PeerDisconnectPacket Packet)
				{
					TimeToGiveUp = this.TimeToGiveUp;
					Packet = this.Packet;
				}
			}

			// Token: 0x020015C6 RID: 5574
			[NullableContext(1)]
			[Nullable(0)]
			public readonly struct UnauthedMessage : IEquatable<P2POwnerPeer.RemotePeer.UnauthedMessage>
			{
				// Token: 0x06009EFB RID: 40699 RVA: 0x003F3447 File Offset: 0x003F1647
				public UnauthedMessage(byte[] Bytes, int LengthBytes)
				{
					this.Bytes = Bytes;
					this.LengthBytes = LengthBytes;
				}

				// Token: 0x17001DB6 RID: 7606
				// (get) Token: 0x06009EFC RID: 40700 RVA: 0x003F3457 File Offset: 0x003F1657
				// (set) Token: 0x06009EFD RID: 40701 RVA: 0x003F345F File Offset: 0x003F165F
				public byte[] Bytes { get; set; }

				// Token: 0x17001DB7 RID: 7607
				// (get) Token: 0x06009EFE RID: 40702 RVA: 0x003F3468 File Offset: 0x003F1668
				// (set) Token: 0x06009EFF RID: 40703 RVA: 0x003F3470 File Offset: 0x003F1670
				public int LengthBytes { get; set; }

				// Token: 0x06009F00 RID: 40704 RVA: 0x003F347C File Offset: 0x003F167C
				[NullableContext(0)]
				[CompilerGenerated]
				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("UnauthedMessage");
					stringBuilder.Append(" { ");
					if (this.PrintMembers(stringBuilder))
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append('}');
					return stringBuilder.ToString();
				}

				// Token: 0x06009F01 RID: 40705 RVA: 0x003F34C8 File Offset: 0x003F16C8
				[NullableContext(0)]
				[CompilerGenerated]
				private bool PrintMembers(StringBuilder builder)
				{
					builder.Append("Bytes = ");
					builder.Append(this.Bytes);
					builder.Append(", LengthBytes = ");
					builder.Append(this.LengthBytes.ToString());
					return true;
				}

				// Token: 0x06009F02 RID: 40706 RVA: 0x003F3516 File Offset: 0x003F1716
				[CompilerGenerated]
				public static bool operator !=(P2POwnerPeer.RemotePeer.UnauthedMessage left, P2POwnerPeer.RemotePeer.UnauthedMessage right)
				{
					return !(left == right);
				}

				// Token: 0x06009F03 RID: 40707 RVA: 0x003F3522 File Offset: 0x003F1722
				[CompilerGenerated]
				public static bool operator ==(P2POwnerPeer.RemotePeer.UnauthedMessage left, P2POwnerPeer.RemotePeer.UnauthedMessage right)
				{
					return left.Equals(right);
				}

				// Token: 0x06009F04 RID: 40708 RVA: 0x003F352C File Offset: 0x003F172C
				[CompilerGenerated]
				public override int GetHashCode()
				{
					return EqualityComparer<byte[]>.Default.GetHashCode(this.<Bytes>k__BackingField) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<LengthBytes>k__BackingField);
				}

				// Token: 0x06009F05 RID: 40709 RVA: 0x003F3555 File Offset: 0x003F1755
				[NullableContext(0)]
				[CompilerGenerated]
				public override bool Equals(object obj)
				{
					return obj is P2POwnerPeer.RemotePeer.UnauthedMessage && this.Equals((P2POwnerPeer.RemotePeer.UnauthedMessage)obj);
				}

				// Token: 0x06009F06 RID: 40710 RVA: 0x003F356D File Offset: 0x003F176D
				[CompilerGenerated]
				public bool Equals(P2POwnerPeer.RemotePeer.UnauthedMessage other)
				{
					return EqualityComparer<byte[]>.Default.Equals(this.<Bytes>k__BackingField, other.<Bytes>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<LengthBytes>k__BackingField, other.<LengthBytes>k__BackingField);
				}

				// Token: 0x06009F07 RID: 40711 RVA: 0x003F359F File Offset: 0x003F179F
				[CompilerGenerated]
				public void Deconstruct(out byte[] Bytes, out int LengthBytes)
				{
					Bytes = this.Bytes;
					LengthBytes = this.LengthBytes;
				}
			}
		}
	}
}
