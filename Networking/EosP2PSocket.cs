using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000464 RID: 1124
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class EosP2PSocket : P2PSocket
	{
		// Token: 0x06004BBE RID: 19390 RVA: 0x0029C7EF File Offset: 0x0029A9EF
		private EosP2PSocket(P2PSocket.Callbacks callbacks, EosInterface.P2PSocket eosSocket, P2PSocket.OwnerOrClient type) : base(callbacks, type)
		{
			this.eosSocket = eosSocket;
		}

		// Token: 0x06004BBF RID: 19391 RVA: 0x0029C800 File Offset: 0x0029AA00
		public static Result<P2PSocket, P2PSocket.Error> Create(P2PSocket.Callbacks callbacks, P2PSocket.OwnerOrClient type)
		{
			if (!EosInterface.Core.IsInitialized)
			{
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(P2PSocket.ErrorCode.EosNotInitialized, ""));
			}
			EosInterface.SocketId eosSocketId = new EosInterface.SocketId
			{
				SocketName = "Barotrauma.EosP2PSocket"
			};
			ImmutableArray<EosInterface.ProductUserId> puids = EosInterface.IdQueries.GetLoggedInPuids();
			if (puids.Length <= 0)
			{
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(P2PSocket.ErrorCode.EosNotLoggedIn, ""));
			}
			Result<EosInterface.P2PSocket, EosInterface.P2PSocket.CreationError> socketCreateResult = EosInterface.P2PSocket.Create(puids[0], eosSocketId);
			EosInterface.P2PSocket eosSocket;
			if (!socketCreateResult.TryUnwrapSuccess(out eosSocket))
			{
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(P2PSocket.ErrorCode.FailedToCreateEosP2PSocket, socketCreateResult.ToString()));
			}
			EosP2PSocket retVal = new EosP2PSocket(callbacks, eosSocket, type);
			eosSocket.HandleIncomingConnection.Register("Event".ToIdentifier(), new Action<EosInterface.P2PSocket.IncomingConnectionRequest>(retVal.OnIncomingConnection));
			eosSocket.HandleClosedConnection.Register("Event".ToIdentifier(), new Action<EosInterface.P2PSocket.RemoteConnectionClosed>(retVal.OnConnectionClosed));
			return Result.Success<P2PSocket>(retVal);
		}

		// Token: 0x06004BC0 RID: 19392 RVA: 0x0029C8F4 File Offset: 0x0029AAF4
		public override void ProcessIncomingMessages()
		{
			foreach (EosInterface.P2PSocket.IncomingMessage msg in this.eosSocket.GetMessageBatch())
			{
				EosP2PEndpoint endpoint = new EosP2PEndpoint(msg.Sender);
				this.callbacks.OnData(endpoint, new ReadWriteMessage(msg.Buffer, 0, msg.ByteLength * 8, false));
				if (this.Type == P2PSocket.OwnerOrClient.Owner)
				{
					this.dosProtection.OnPacket(endpoint);
				}
			}
		}

		// Token: 0x06004BC1 RID: 19393 RVA: 0x0029C98C File Offset: 0x0029AB8C
		public override bool SendMessage(P2PEndpoint endpoint, IWriteMessage outMsg, DeliveryMethod deliveryMethod)
		{
			EosP2PEndpoint eosP2PEndpoint = endpoint as EosP2PEndpoint;
			if (eosP2PEndpoint != null)
			{
				EosInterface.ProductUserId puid = eosP2PEndpoint.ProductUserId;
				Result<Unit, EosInterface.P2PSocket.SendError> sendResult = this.eosSocket.SendMessage(new EosInterface.P2PSocket.OutgoingMessage(outMsg.Buffer, outMsg.LengthBytes, puid, deliveryMethod));
				return sendResult.IsSuccess;
			}
			return false;
		}

		// Token: 0x06004BC2 RID: 19394 RVA: 0x0029C9D4 File Offset: 0x0029ABD4
		private void OnIncomingConnection(EosInterface.P2PSocket.IncomingConnectionRequest request)
		{
			EosP2PEndpoint remoteEndpoint = new EosP2PEndpoint(request.RemoteUserId);
			if (this.callbacks.OnIncomingConnection(remoteEndpoint))
			{
				request.Accept();
			}
		}

		// Token: 0x06004BC3 RID: 19395 RVA: 0x0029CA08 File Offset: 0x0029AC08
		private void OnConnectionClosed(EosInterface.P2PSocket.RemoteConnectionClosed data)
		{
			EosP2PEndpoint remoteEndpoint = new EosP2PEndpoint(data.RemoteUserId);
			DisconnectReason disconnectReason;
			switch (data.Reason)
			{
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.Unknown:
				disconnectReason = DisconnectReason.Unknown;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.ClosedByLocalUser:
				disconnectReason = DisconnectReason.Disconnected;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.ClosedByPeer:
				disconnectReason = DisconnectReason.Disconnected;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.TimedOut:
				disconnectReason = DisconnectReason.Timeout;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.TooManyConnections:
				disconnectReason = DisconnectReason.ServerFull;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.InvalidMessage:
				disconnectReason = DisconnectReason.Unknown;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.InvalidData:
				disconnectReason = DisconnectReason.Unknown;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.ConnectionFailed:
				disconnectReason = DisconnectReason.AuthenticationFailed;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.ConnectionClosed:
				disconnectReason = DisconnectReason.Disconnected;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.NegotiationFailed:
				disconnectReason = DisconnectReason.AuthenticationFailed;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.UnexpectedError:
				disconnectReason = DisconnectReason.Unknown;
				break;
			case EosInterface.P2PSocket.RemoteConnectionClosed.ConnectionClosedReason.Unhandled:
				disconnectReason = DisconnectReason.Unknown;
				break;
			default:
				disconnectReason = DisconnectReason.Unknown;
				break;
			}
			PeerDisconnectPacket peerDisconnectPacket = PeerDisconnectPacket.WithReason(disconnectReason);
			this.callbacks.OnConnectionClosed(remoteEndpoint, peerDisconnectPacket);
		}

		// Token: 0x06004BC4 RID: 19396 RVA: 0x0029CAB0 File Offset: 0x0029ACB0
		public override void CloseConnection(P2PEndpoint endpoint)
		{
			EosP2PEndpoint eosP2PEndpoint = endpoint as EosP2PEndpoint;
			if (eosP2PEndpoint != null)
			{
				EosInterface.ProductUserId puid = eosP2PEndpoint.ProductUserId;
				this.eosSocket.CloseConnection(puid);
				return;
			}
		}

		// Token: 0x06004BC5 RID: 19397 RVA: 0x0029CADD File Offset: 0x0029ACDD
		public override void Dispose()
		{
			this.eosSocket.Dispose();
		}

		// Token: 0x040027A1 RID: 10145
		private readonly EosInterface.P2PSocket eosSocket;
	}
}
