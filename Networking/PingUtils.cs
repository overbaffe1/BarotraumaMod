using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Extensions;
using Barotrauma.Steam;
using Steamworks;
using Steamworks.Data;

namespace Barotrauma.Networking
{
	// Token: 0x0200046E RID: 1134
	internal static class PingUtils
	{
		// Token: 0x06004C4C RID: 19532 RVA: 0x002A0B60 File Offset: 0x0029ED60
		public static void QueryPingData()
		{
			PingUtils.steamPingInfoReady = false;
			if (SteamManager.IsInitialized)
			{
				TaskPool.Add("WaitForPingDataAsync (serverlist)", SteamNetworkingUtils.WaitForPingDataAsync(300f), delegate(Task task)
				{
					PingUtils.steamPingInfoReady = true;
				});
			}
		}

		// Token: 0x06004C4D RID: 19533 RVA: 0x002A0BB0 File Offset: 0x0029EDB0
		public static void GetServerPing(ServerInfo serverInfo, Action<ServerInfo> onPingDiscovered)
		{
			if (CoroutineManager.IsCoroutineRunning("ConnectToServer"))
			{
				return;
			}
			Endpoint endpoint;
			if (!serverInfo.Endpoints.FirstOrNone((Endpoint e) => !(e is EosP2PEndpoint)).TryUnwrap(out endpoint))
			{
				return;
			}
			LidgrenEndpoint lidgrenEndpoint = endpoint as LidgrenEndpoint;
			if (lidgrenEndpoint != null)
			{
				IPEndPoint endPoint = lidgrenEndpoint.NetEndpoint;
				PingUtils.GetIPAddressPing(serverInfo, endPoint, onPingDiscovered);
				return;
			}
			if (!(endpoint is SteamP2PEndpoint))
			{
				return;
			}
			TaskPool.Add("EstimateSteamLobbyPing (" + endpoint.StringRepresentation + ")", PingUtils.EstimateSteamLobbyPing(serverInfo), delegate(Task t)
			{
				Result<int, PingUtils.SteamLobbyPingError> ping;
				if (!t.TryGetResult(out ping))
				{
					return;
				}
				ServerInfo serverInfo2 = serverInfo;
				int ms;
				Option<int> ping2;
				if (!ping.TryUnwrapSuccess(out ms))
				{
					Option.UnspecifiedNone none = Option.None;
					ping2 = none;
				}
				else
				{
					ping2 = Option.Some<int>(ms);
				}
				serverInfo2.Ping = ping2;
				onPingDiscovered(serverInfo);
			});
		}

		// Token: 0x06004C4E RID: 19534 RVA: 0x002A0C84 File Offset: 0x0029EE84
		public static Task<Lobby?> GetSteamLobbyForUser(SteamId steamId)
		{
			PingUtils.<GetSteamLobbyForUser>d__5 <GetSteamLobbyForUser>d__;
			<GetSteamLobbyForUser>d__.<>t__builder = AsyncTaskMethodBuilder<Lobby?>.Create();
			<GetSteamLobbyForUser>d__.steamId = steamId;
			<GetSteamLobbyForUser>d__.<>1__state = -1;
			<GetSteamLobbyForUser>d__.<>t__builder.Start<PingUtils.<GetSteamLobbyForUser>d__5>(ref <GetSteamLobbyForUser>d__);
			return <GetSteamLobbyForUser>d__.<>t__builder.Task;
		}

		// Token: 0x06004C4F RID: 19535 RVA: 0x002A0CC8 File Offset: 0x0029EEC8
		private static Task<Result<int, PingUtils.SteamLobbyPingError>> EstimateSteamLobbyPing(ServerInfo serverInfo)
		{
			PingUtils.<EstimateSteamLobbyPing>d__7 <EstimateSteamLobbyPing>d__;
			<EstimateSteamLobbyPing>d__.<>t__builder = AsyncTaskMethodBuilder<Result<int, PingUtils.SteamLobbyPingError>>.Create();
			<EstimateSteamLobbyPing>d__.serverInfo = serverInfo;
			<EstimateSteamLobbyPing>d__.<>1__state = -1;
			<EstimateSteamLobbyPing>d__.<>t__builder.Start<PingUtils.<EstimateSteamLobbyPing>d__7>(ref <EstimateSteamLobbyPing>d__);
			return <EstimateSteamLobbyPing>d__.<>t__builder.Task;
		}

		// Token: 0x06004C50 RID: 19536 RVA: 0x002A0D0C File Offset: 0x0029EF0C
		private static void GetIPAddressPing(ServerInfo serverInfo, IPEndPoint endPoint, Action<ServerInfo> onPingDiscovered)
		{
			if (IPAddress.IsLoopback(endPoint.Address))
			{
				serverInfo.Ping = Option<int>.Some(0);
				onPingDiscovered(serverInfo);
				return;
			}
			Dictionary<IPEndPoint, int> obj = PingUtils.activePings;
			lock (obj)
			{
				if (PingUtils.activePings.ContainsKey(endPoint))
				{
					return;
				}
				PingUtils.activePings.Add(endPoint, PingUtils.activePings.Any<KeyValuePair<IPEndPoint, int>>() ? (PingUtils.activePings.Values.Max() + 1) : 0);
			}
			serverInfo.Ping = Option<int>.None();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler.AppendLiteral("PingServerAsync (");
			defaultInterpolatedStringHandler.AppendFormatted<IPEndPoint>(endPoint);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), PingUtils.PingServerAsync(endPoint, 1000), delegate(Task rtt)
			{
				if (!rtt.TryGetResult(out serverInfo.Ping))
				{
					serverInfo.Ping = Option<int>.None();
				}
				onPingDiscovered(serverInfo);
				Dictionary<IPEndPoint, int> obj2 = PingUtils.activePings;
				lock (obj2)
				{
					PingUtils.activePings.Remove(endPoint);
				}
			});
		}

		// Token: 0x06004C51 RID: 19537 RVA: 0x002A0E44 File Offset: 0x0029F044
		private static Task<Option<int>> PingServerAsync(IPEndPoint endPoint, int timeOut)
		{
			PingUtils.<PingServerAsync>d__9 <PingServerAsync>d__;
			<PingServerAsync>d__.<>t__builder = AsyncTaskMethodBuilder<Option<int>>.Create();
			<PingServerAsync>d__.endPoint = endPoint;
			<PingServerAsync>d__.timeOut = timeOut;
			<PingServerAsync>d__.<>1__state = -1;
			<PingServerAsync>d__.<>t__builder.Start<PingUtils.<PingServerAsync>d__9>(ref <PingServerAsync>d__);
			return <PingServerAsync>d__.<>t__builder.Task;
		}

		// Token: 0x040027DA RID: 10202
		private static readonly Dictionary<IPEndPoint, int> activePings = new Dictionary<IPEndPoint, int>();

		// Token: 0x040027DB RID: 10203
		private static bool steamPingInfoReady;

		// Token: 0x0200120B RID: 4619
		private readonly struct LobbyDataChangedEventHandler : IDisposable
		{
			// Token: 0x060092F9 RID: 37625 RVA: 0x003CA87B File Offset: 0x003C8A7B
			public LobbyDataChangedEventHandler(Action<Lobby> action)
			{
				this.action = action;
				SteamMatchmaking.OnLobbyDataChanged += action;
			}

			// Token: 0x060092FA RID: 37626 RVA: 0x003CA88A File Offset: 0x003C8A8A
			public void Dispose()
			{
				SteamMatchmaking.OnLobbyDataChanged -= this.action;
			}

			// Token: 0x04005DFD RID: 24061
			private readonly Action<Lobby> action;
		}

		// Token: 0x0200120C RID: 4620
		private enum SteamLobbyPingError
		{
			// Token: 0x04005DFF RID: 24063
			SteamPingUnsupported,
			// Token: 0x04005E00 RID: 24064
			FailedToGetHostLocationData,
			// Token: 0x04005E01 RID: 24065
			FailedToParseHostLocationData,
			// Token: 0x04005E02 RID: 24066
			PingEstimationFailed
		}
	}
}
