using System;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x02000463 RID: 1123
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class DualStackP2PSocket : P2PSocket
	{
		// Token: 0x06004BB8 RID: 19384 RVA: 0x0029C5F3 File Offset: 0x0029A7F3
		private DualStackP2PSocket(P2PSocket.Callbacks callbacks, [Nullable(new byte[]
		{
			0,
			1
		})] Option<EosP2PSocket> eosSocket, [Nullable(new byte[]
		{
			0,
			1
		})] Option<SteamListenSocket> steamSocket, P2PSocket.OwnerOrClient type) : base(callbacks, type)
		{
			this.eosSocket = eosSocket;
			this.steamSocket = steamSocket;
		}

		// Token: 0x06004BB9 RID: 19385 RVA: 0x0029C60C File Offset: 0x0029A80C
		public static Result<P2PSocket, P2PSocket.Error> Create(P2PSocket.Callbacks callbacks, P2PSocket.OwnerOrClient type)
		{
			Result<P2PSocket, P2PSocket.Error> eosP2PSocketResult = EosP2PSocket.Create(callbacks, type);
			Result<P2PSocket, P2PSocket.Error> steamP2PSocketResult = SteamListenSocket.Create(callbacks, type);
			P2PSocket.Error eosError;
			P2PSocket.Error steamError;
			if (eosP2PSocketResult.TryUnwrapFailure(out eosError) && steamP2PSocketResult.TryUnwrapFailure(out steamError))
			{
				return Result.Failure<P2PSocket.Error>(new P2PSocket.Error(new P2PSocket.Error[]
				{
					eosError,
					steamError
				}));
			}
			P2PSocket eosP2PSocket;
			Option<EosP2PSocket> option;
			if (!eosP2PSocketResult.TryUnwrapSuccess(out eosP2PSocket))
			{
				Option.UnspecifiedNone none = Option.None;
				option = none;
			}
			else
			{
				option = Option.Some<EosP2PSocket>((EosP2PSocket)eosP2PSocket);
			}
			P2PSocket steamP2PSocket;
			Option<SteamListenSocket> option2;
			if (!steamP2PSocketResult.TryUnwrapSuccess(out steamP2PSocket))
			{
				Option.UnspecifiedNone none2 = Option.None;
				option2 = none2;
			}
			else
			{
				option2 = Option.Some<SteamListenSocket>((SteamListenSocket)steamP2PSocket);
			}
			return Result.Success<P2PSocket>(new DualStackP2PSocket(callbacks, option, option2, type));
		}

		// Token: 0x06004BBA RID: 19386 RVA: 0x0029C6C0 File Offset: 0x0029A8C0
		public override void ProcessIncomingMessages()
		{
			EosP2PSocket eosP2PSocket;
			if (this.eosSocket.TryUnwrap(out eosP2PSocket))
			{
				eosP2PSocket.ProcessIncomingMessages();
			}
			SteamListenSocket steamP2PSocket;
			if (this.steamSocket.TryUnwrap(out steamP2PSocket))
			{
				steamP2PSocket.ProcessIncomingMessages();
			}
		}

		// Token: 0x06004BBB RID: 19387 RVA: 0x0029C6F8 File Offset: 0x0029A8F8
		public override bool SendMessage(P2PEndpoint endpoint, IWriteMessage outMsg, DeliveryMethod deliveryMethod)
		{
			EosP2PEndpoint eosP2PEndpoint2 = endpoint as EosP2PEndpoint;
			if (eosP2PEndpoint2 == null)
			{
				SteamP2PEndpoint steamP2PEndpoint2 = endpoint as SteamP2PEndpoint;
				if (steamP2PEndpoint2 != null)
				{
					SteamP2PEndpoint steamP2PEndpoint = steamP2PEndpoint2;
					SteamListenSocket steamP2PSocket;
					if (this.steamSocket.TryUnwrap(out steamP2PSocket))
					{
						return steamP2PSocket.SendMessage(steamP2PEndpoint, outMsg, deliveryMethod);
					}
				}
			}
			else
			{
				EosP2PEndpoint eosP2PEndpoint = eosP2PEndpoint2;
				EosP2PSocket eosP2PSocket;
				if (this.eosSocket.TryUnwrap(out eosP2PSocket))
				{
					return eosP2PSocket.SendMessage(eosP2PEndpoint, outMsg, deliveryMethod);
				}
			}
			return false;
		}

		// Token: 0x06004BBC RID: 19388 RVA: 0x0029C768 File Offset: 0x0029A968
		public override void CloseConnection(P2PEndpoint endpoint)
		{
			EosP2PEndpoint eosP2PEndpoint = endpoint as EosP2PEndpoint;
			EosP2PSocket eosP2PSocket;
			if (eosP2PEndpoint == null)
			{
				SteamP2PEndpoint steamP2PEndpoint = endpoint as SteamP2PEndpoint;
				if (steamP2PEndpoint == null)
				{
					return;
				}
				SteamListenSocket steamP2PSocket;
				if (this.steamSocket.TryUnwrap(out steamP2PSocket))
				{
					steamP2PSocket.CloseConnection(steamP2PEndpoint);
				}
			}
			else if (this.eosSocket.TryUnwrap(out eosP2PSocket))
			{
				eosP2PSocket.CloseConnection(eosP2PEndpoint);
				return;
			}
		}

		// Token: 0x06004BBD RID: 19389 RVA: 0x0029C7B8 File Offset: 0x0029A9B8
		public override void Dispose()
		{
			EosP2PSocket eosP2PSocket;
			if (this.eosSocket.TryUnwrap(out eosP2PSocket))
			{
				eosP2PSocket.Dispose();
			}
			SteamListenSocket steamP2PSocket;
			if (this.steamSocket.TryUnwrap(out steamP2PSocket))
			{
				steamP2PSocket.Dispose();
			}
		}

		// Token: 0x0400279F RID: 10143
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly Option<EosP2PSocket> eosSocket;

		// Token: 0x040027A0 RID: 10144
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly Option<SteamListenSocket> steamSocket;
	}
}
