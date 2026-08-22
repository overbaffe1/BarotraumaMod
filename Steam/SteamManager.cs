using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;

namespace Barotrauma.Steam
{
	// Token: 0x0200042A RID: 1066
	internal static class SteamManager
	{
		// Token: 0x17001239 RID: 4665
		// (get) Token: 0x06004766 RID: 18278 RVA: 0x00271D74 File Offset: 0x0026FF74
		public static ulong CurrentLobbyID
		{
			get
			{
				return (SteamManager.currentLobby != null) ? SteamManager.currentLobby.GetValueOrDefault().Id : 0UL;
			}
		}

		// Token: 0x06004767 RID: 18279 RVA: 0x00271DAC File Offset: 0x0026FFAC
		public static void CreateLobby(ServerSettings serverSettings)
		{
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			if (SteamManager.lobbyState != SteamManager.LobbyState.NotConnected)
			{
				return;
			}
			SteamManager.lobbyState = SteamManager.LobbyState.Creating;
			TaskPool.Add("CreateLobbyAsync", SteamMatchmaking.CreateLobbyAsync(serverSettings.MaxPlayers + 10), delegate(Task lobby)
			{
				if (SteamManager.lobbyState != SteamManager.LobbyState.Creating)
				{
					SteamManager.LeaveLobby();
					return;
				}
				SteamManager.currentLobby = ((Task<Lobby?>)lobby).Result;
				if (SteamManager.currentLobby == null)
				{
					DebugConsole.ThrowError("Failed to create Steam lobby", null, null, false, false);
					SteamManager.lobbyState = SteamManager.LobbyState.NotConnected;
					return;
				}
				DebugConsole.NewMessage("Lobby created!", new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Lime), false);
				SteamManager.lobbyState = SteamManager.LobbyState.Owner;
				SteamManager.lobbyID = ((SteamManager.currentLobby != null) ? new Steamworks.SteamId?(SteamManager.currentLobby.GetValueOrDefault().Id) : null).Value;
				SteamManager.SetLobbyPublic(serverSettings.IsPublic);
				if (SteamManager.currentLobby != null)
				{
					SteamManager.currentLobby.GetValueOrDefault().SetJoinable(true);
				}
				SteamManager.UpdateLobby(serverSettings);
			});
		}

		// Token: 0x06004768 RID: 18280 RVA: 0x00271E08 File Offset: 0x00270008
		public static void SetLobbyPublic(bool isPublic)
		{
			if (isPublic)
			{
				if (SteamManager.currentLobby == null)
				{
					return;
				}
				SteamManager.currentLobby.GetValueOrDefault().SetPublic();
				return;
			}
			else
			{
				if (SteamManager.currentLobby == null)
				{
					return;
				}
				SteamManager.currentLobby.GetValueOrDefault().SetFriendsOnly();
				return;
			}
		}

		// Token: 0x06004769 RID: 18281 RVA: 0x00271E54 File Offset: 0x00270054
		public static void UpdateLobby(ServerSettings serverSettings)
		{
			if (GameMain.Client == null)
			{
				SteamManager.LeaveLobby();
				return;
			}
			if (SteamManager.lobbyState == SteamManager.LobbyState.NotConnected)
			{
				SteamManager.CreateLobby(serverSettings);
			}
			if (SteamManager.lobbyState != SteamManager.LobbyState.Owner)
			{
				return;
			}
			Action<Identifier, object> setter;
			if ((setter = SteamManager.<>O.<0>__SetServerListInfo) == null)
			{
				setter = (SteamManager.<>O.<0>__SetServerListInfo = new Action<Identifier, object>(SteamManager.SetServerListInfo));
			}
			serverSettings.UpdateServerListInfo(setter);
			if (SteamManager.currentLobby != null)
			{
				Lobby valueOrDefault = SteamManager.currentLobby.GetValueOrDefault();
				string key = "lobbyowner";
				Barotrauma.Networking.SteamId steamId;
				if (!SteamManager.GetSteamId().TryUnwrap(out steamId))
				{
					throw new InvalidOperationException("Steamworks not initialized");
				}
				valueOrDefault.SetData(key, steamId.StringRepresentation);
			}
			ImmutableArray<EosInterface.ProductUserId> puids = EosInterface.IdQueries.GetLoggedInPuids();
			if (puids.Length > 0)
			{
				if (SteamManager.currentLobby != null)
				{
					Lobby valueOrDefault = SteamManager.currentLobby.GetValueOrDefault();
					valueOrDefault.SetData("EosEndpoint", puids[0].Value);
				}
			}
			DebugConsole.Log("Lobby updated!");
		}

		// Token: 0x0600476A RID: 18282 RVA: 0x00271F38 File Offset: 0x00270138
		private static void SetServerListInfo(Identifier key, object value)
		{
			IEnumerable<ContentPackage> contentPackages = value as IEnumerable<ContentPackage>;
			if (contentPackages != null)
			{
				Lobby valueOrDefault;
				if (SteamManager.currentLobby != null)
				{
					valueOrDefault = SteamManager.currentLobby.GetValueOrDefault();
					valueOrDefault.SetData("contentpackage", (from p in contentPackages
					select p.Name).JoinEscaped(','));
				}
				if (SteamManager.currentLobby != null)
				{
					valueOrDefault = SteamManager.currentLobby.GetValueOrDefault();
					valueOrDefault.SetData("contentpackagehash", (from p in contentPackages
					select p.Hash.StringRepresentation).JoinEscaped(','));
				}
				if (SteamManager.currentLobby == null)
				{
					return;
				}
				valueOrDefault = SteamManager.currentLobby.GetValueOrDefault();
				valueOrDefault.SetData("contentpackageid", (from p in contentPackages
				select (from ugcId in p.UgcId
				select ugcId.StringRepresentation).Fallback("")).JoinEscaped(','));
				return;
			}
			else
			{
				if (SteamManager.currentLobby == null)
				{
					return;
				}
				Lobby valueOrDefault = SteamManager.currentLobby.GetValueOrDefault();
				valueOrDefault.SetData(key.Value.ToLowerInvariant(), value.ToString());
				return;
			}
		}

		// Token: 0x0600476B RID: 18283 RVA: 0x00272070 File Offset: 0x00270270
		public static void LeaveLobby()
		{
			if (SteamManager.lobbyState != SteamManager.LobbyState.NotConnected)
			{
				if (SteamManager.currentLobby != null)
				{
					SteamManager.currentLobby.GetValueOrDefault().Leave();
				}
				SteamManager.currentLobby = null;
				SteamManager.lobbyState = SteamManager.LobbyState.NotConnected;
				SteamManager.lobbyID = 0UL;
				SteamMatchmaking.ResetActions();
			}
		}

		// Token: 0x0600476C RID: 18284 RVA: 0x002720C0 File Offset: 0x002702C0
		public static void JoinLobby(ulong id, bool joinServer)
		{
			if (SteamManager.currentLobby != null && SteamManager.currentLobby.Value.Id == id)
			{
				return;
			}
			if (SteamManager.lobbyID == id)
			{
				return;
			}
			SteamManager.lobbyState = SteamManager.LobbyState.Joining;
			SteamManager.lobbyID = id;
			TaskPool.Add("JoinLobbyAsync", SteamMatchmaking.JoinLobbyAsync(SteamManager.lobbyID), delegate(Task lobby)
			{
				SteamManager.currentLobby = ((Task<Lobby?>)lobby).Result;
				SteamManager.lobbyState = SteamManager.LobbyState.Joined;
				SteamManager.lobbyID = ((SteamManager.currentLobby != null) ? new Steamworks.SteamId?(SteamManager.currentLobby.GetValueOrDefault().Id) : null).Value;
				if (joinServer)
				{
					GameMain.Instance.ConnectCommand = Option<ConnectCommand>.Some(new ConnectCommand(((SteamManager.currentLobby != null) ? SteamManager.currentLobby.GetValueOrDefault().GetData("servername") : null) ?? "Server", new SteamP2PEndpoint(new Barotrauma.Networking.SteamId((SteamManager.currentLobby != null) ? SteamManager.currentLobby.GetValueOrDefault().Owner.Id : 0UL))));
				}
			});
		}

		// Token: 0x1700123A RID: 4666
		// (get) Token: 0x0600476D RID: 18285 RVA: 0x0027213C File Offset: 0x0027033C
		public static IReadOnlyList<Identifier> InitializationErrors
		{
			get
			{
				return SteamManager.initializationErrors;
			}
		}

		// Token: 0x1700123B RID: 4667
		// (get) Token: 0x0600476E RID: 18286 RVA: 0x00272143 File Offset: 0x00270343
		private static bool IsInitializedProjectSpecific
		{
			get
			{
				return SteamClient.IsValid && SteamClient.IsLoggedOn;
			}
		}

		// Token: 0x0600476F RID: 18287 RVA: 0x00272154 File Offset: 0x00270354
		private static void InitializeProjectSpecific()
		{
			if (SteamManager.IsInitialized)
			{
				return;
			}
			try
			{
				SteamClient.Init(602960U, false);
				if (SteamManager.IsInitialized)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Logged in as ");
					defaultInterpolatedStringHandler.AppendFormatted(SteamManager.GetUsername());
					defaultInterpolatedStringHandler.AppendLiteral(" (SteamID ");
					Barotrauma.Networking.SteamId steamId;
					defaultInterpolatedStringHandler.AppendFormatted(SteamManager.GetSteamId().TryUnwrap(out steamId) ? steamId.ToString() : "[NULL]");
					defaultInterpolatedStringHandler.AppendLiteral(")");
					DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), null, false);
					SteamManager.popularTags.Clear();
					int i = 0;
					foreach (KeyValuePair<string, int> commonness in SteamManager.tagCommonness)
					{
						SteamManager.popularTags.Insert(i, commonness.Key);
						i++;
					}
				}
				SteamNetworkingUtils.OnDebugOutput += delegate(NetDebugOutput nType, string pszMsg)
				{
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						SteamManager.LogSteamworksNetworking(nType, pszMsg);
					}
				};
				SteamFriends.ListenForFriendsMessages = true;
			}
			catch (DllNotFoundException)
			{
				SteamManager.initializationErrors.Add("SteamDllNotFound".ToIdentifier());
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("SteamManager initialization threw an exception", e, null, false, false);
				SteamManager.initializationErrors.Add("SteamClientInitFailed".ToIdentifier());
			}
			if (!SteamManager.IsInitialized)
			{
				try
				{
					if (SteamClient.IsValid)
					{
						SteamClient.Shutdown();
					}
					goto IL_1CB;
				}
				catch (Exception e2)
				{
					if (GameSettings.CurrentConfig.VerboseLogging)
					{
						DebugConsole.ThrowError("Disposing Steam client failed.", e2, null, false, false);
					}
					goto IL_1CB;
				}
			}
			SteamUGC.OnItemInstalled += delegate(AppId appId, PublishedFileId itemId)
			{
				SteamManager.Workshop.OnItemDownloadComplete(itemId, false);
			};
			SteamUGC.OnDownloadItemResult += delegate(Result result, PublishedFileId id)
			{
				if (result == Result.OK)
				{
					SteamManager.Workshop.OnItemDownloadComplete(id, false);
				}
			};
			IL_1CB:
			SteamTimelineManager.Initialize();
		}

		// Token: 0x1700123C RID: 4668
		// (get) Token: 0x06004770 RID: 18288 RVA: 0x00272398 File Offset: 0x00270598
		// (set) Token: 0x06004771 RID: 18289 RVA: 0x0027239F File Offset: 0x0027059F
		public static bool NetworkingDebugLog { get; private set; } = false;

		// Token: 0x06004772 RID: 18290 RVA: 0x002723A8 File Offset: 0x002705A8
		private static void LogSteamworksNetworking(NetDebugOutput nType, string pszMsg)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
			defaultInterpolatedStringHandler.AppendLiteral("(");
			defaultInterpolatedStringHandler.AppendFormatted<NetDebugOutput>(nType);
			defaultInterpolatedStringHandler.AppendLiteral(") ");
			defaultInterpolatedStringHandler.AppendFormatted(pszMsg);
			DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Microsoft.Xna.Framework.Color?(Microsoft.Xna.Framework.Color.Orange), false);
		}

		// Token: 0x06004773 RID: 18291 RVA: 0x002723FD File Offset: 0x002705FD
		public static void SetSteamworksNetworkingDebugLog(bool enabled)
		{
			if (enabled == SteamManager.NetworkingDebugLog)
			{
				return;
			}
			if (enabled)
			{
				SteamNetworkingUtils.DebugLevel = NetDebugOutput.Everything;
			}
			else
			{
				SteamNetworkingUtils.DebugLevel = NetDebugOutput.None;
			}
			SteamManager.NetworkingDebugLog = enabled;
		}

		// Token: 0x06004774 RID: 18292 RVA: 0x00272420 File Offset: 0x00270620
		public static Task InitRelayNetworkAccess()
		{
			SteamManager.<InitRelayNetworkAccess>d__24 <InitRelayNetworkAccess>d__;
			<InitRelayNetworkAccess>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<InitRelayNetworkAccess>d__.<>1__state = -1;
			<InitRelayNetworkAccess>d__.<>t__builder.Start<SteamManager.<InitRelayNetworkAccess>d__24>(ref <InitRelayNetworkAccess>d__);
			return <InitRelayNetworkAccess>d__.<>t__builder.Task;
		}

		// Token: 0x06004775 RID: 18293 RVA: 0x0027245B File Offset: 0x0027065B
		public static bool OverlayCustomUrl(string url)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return false;
			}
			SteamFriends.OpenWebOverlay(url, false);
			return true;
		}

		// Token: 0x06004776 RID: 18294 RVA: 0x00272478 File Offset: 0x00270678
		[NullableContext(1)]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<AuthTicket> GetAuthSessionTicketForSteamHost(Endpoint remoteHostEndpoint)
		{
			Option.UnspecifiedNone none;
			if (!SteamManager.IsInitialized)
			{
				none = Option.None;
				return none;
			}
			AuthTicket ticketToCancel;
			if (SteamManager.currentSteamHostAuthTicket.TryUnwrap(out ticketToCancel))
			{
				ticketToCancel.Cancel();
			}
			none = Option.None;
			SteamManager.currentSteamHostAuthTicket = none;
			LidgrenEndpoint lidgrenEndpoint = remoteHostEndpoint as LidgrenEndpoint;
			NetIdentity netIdentity2;
			if (lidgrenEndpoint != null)
			{
				Address address = remoteHostEndpoint.Address;
				LidgrenAddress lidgrenAddress = address as LidgrenAddress;
				if (lidgrenAddress != null)
				{
					IPAddress ipAddr = lidgrenAddress.NetAddress;
					int ipPort = lidgrenEndpoint.Port;
					netIdentity2 = NetAddress.From(ipAddr, (ushort)ipPort);
					goto IL_BA;
				}
			}
			else
			{
				SteamP2PEndpoint steamP2PEndpoint = remoteHostEndpoint as SteamP2PEndpoint;
				if (steamP2PEndpoint != null)
				{
					Barotrauma.Networking.SteamId steamId = steamP2PEndpoint.SteamId;
					netIdentity2 = steamId.Value;
					goto IL_BA;
				}
			}
			throw new ArgumentOutOfRangeException("remoteHostEndpoint");
			IL_BA:
			NetIdentity netIdentity = netIdentity2;
			AuthTicket newTicket = SteamUser.GetAuthSessionTicket(netIdentity);
			Option<AuthTicket> option;
			if (newTicket == null)
			{
				none = Option.None;
				option = none;
			}
			else
			{
				option = Option.Some<AuthTicket>(newTicket);
			}
			SteamManager.currentSteamHostAuthTicket = option;
			return SteamManager.currentSteamHostAuthTicket;
		}

		// Token: 0x06004777 RID: 18295 RVA: 0x0027256C File Offset: 0x0027076C
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public static Task<Option<AuthTicket>> GetAuthTicketForEosHostAuth()
		{
			SteamManager.<GetAuthTicketForEosHostAuth>d__31 <GetAuthTicketForEosHostAuth>d__;
			<GetAuthTicketForEosHostAuth>d__.<>t__builder = AsyncTaskMethodBuilder<Option<AuthTicket>>.Create();
			<GetAuthTicketForEosHostAuth>d__.<>1__state = -1;
			<GetAuthTicketForEosHostAuth>d__.<>t__builder.Start<SteamManager.<GetAuthTicketForEosHostAuth>d__31>(ref <GetAuthTicketForEosHostAuth>d__);
			return <GetAuthTicketForEosHostAuth>d__.<>t__builder.Task;
		}

		// Token: 0x06004778 RID: 18296 RVA: 0x002725A8 File Offset: 0x002707A8
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public static Task<Option<AuthTicket>> GetAuthTicketForGameAnalyticsConsent()
		{
			SteamManager.<GetAuthTicketForGameAnalyticsConsent>d__34 <GetAuthTicketForGameAnalyticsConsent>d__;
			<GetAuthTicketForGameAnalyticsConsent>d__.<>t__builder = AsyncTaskMethodBuilder<Option<AuthTicket>>.Create();
			<GetAuthTicketForGameAnalyticsConsent>d__.<>1__state = -1;
			<GetAuthTicketForGameAnalyticsConsent>d__.<>t__builder.Start<SteamManager.<GetAuthTicketForGameAnalyticsConsent>d__34>(ref <GetAuthTicketForGameAnalyticsConsent>d__);
			return <GetAuthTicketForGameAnalyticsConsent>d__.<>t__builder.Task;
		}

		// Token: 0x1700123D RID: 4669
		// (get) Token: 0x06004779 RID: 18297 RVA: 0x002725E3 File Offset: 0x002707E3
		public static bool IsInitialized
		{
			get
			{
				return SteamManager.IsInitializedProjectSpecific;
			}
		}

		// Token: 0x1700123E RID: 4670
		// (get) Token: 0x0600477A RID: 18298 RVA: 0x002725EA File Offset: 0x002707EA
		public static IEnumerable<string> PopularTags
		{
			get
			{
				if (!SteamManager.IsInitialized)
				{
					return Enumerable.Empty<string>();
				}
				return SteamManager.popularTags;
			}
		}

		// Token: 0x1700123F RID: 4671
		// (get) Token: 0x0600477B RID: 18299 RVA: 0x00272600 File Offset: 0x00270800
		public static bool SteamworksLibExists
		{
			get
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					return File.Exists("steam_api64.dll");
				}
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					return RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && File.Exists("libsteam_api64.so");
				}
				return File.Exists("libsteam_api64.dylib");
			}
		}

		// Token: 0x0600477C RID: 18300 RVA: 0x00272653 File Offset: 0x00270853
		public static void Initialize()
		{
			SteamManager.InitializeProjectSpecific();
		}

		// Token: 0x0600477D RID: 18301 RVA: 0x0027265A File Offset: 0x0027085A
		public static Option<Barotrauma.Networking.SteamId> GetSteamId()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return Option<Barotrauma.Networking.SteamId>.None();
			}
			return Option<Barotrauma.Networking.SteamId>.Some(new Barotrauma.Networking.SteamId(SteamClient.SteamId));
		}

		// Token: 0x0600477E RID: 18302 RVA: 0x00272684 File Offset: 0x00270884
		public static Option<Barotrauma.Networking.SteamId> GetOwnerSteamId()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return Option<Barotrauma.Networking.SteamId>.None();
			}
			return Option<Barotrauma.Networking.SteamId>.Some(new Barotrauma.Networking.SteamId(SteamClient.SteamId));
		}

		// Token: 0x0600477F RID: 18303 RVA: 0x002726AE File Offset: 0x002708AE
		public static bool IsFamilyShared()
		{
			return SteamManager.IsInitialized && SteamClient.IsValid && SteamApps.IsSubscribedFromFamilySharing;
		}

		// Token: 0x06004780 RID: 18304 RVA: 0x002726C5 File Offset: 0x002708C5
		public static bool IsFreeWeekend()
		{
			return SteamManager.IsInitialized && SteamClient.IsValid && SteamApps.IsSubscribedFromFreeWeekend;
		}

		// Token: 0x06004781 RID: 18305 RVA: 0x002726DC File Offset: 0x002708DC
		public static string GetUsername()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return "";
			}
			return SteamClient.Name;
		}

		// Token: 0x06004782 RID: 18306 RVA: 0x002726F7 File Offset: 0x002708F7
		public static uint GetNumSubscribedItems()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return 0U;
			}
			return SteamUGC.NumSubscribedItems;
		}

		// Token: 0x06004783 RID: 18307 RVA: 0x0027270E File Offset: 0x0027090E
		public static bool UnlockAchievement(string achievementIdentifier)
		{
			return SteamManager.UnlockAchievement(achievementIdentifier.ToIdentifier());
		}

		// Token: 0x06004784 RID: 18308 RVA: 0x0027271C File Offset: 0x0027091C
		public static bool UnlockAchievement(Identifier achievementIdentifier)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return false;
			}
			DebugConsole.Log("Unlocked achievement \"" + achievementIdentifier.ToString() + "\"");
			List<Achievement> achievements = SteamUserStats.Achievements.ToList<Achievement>();
			int achIndex = achievements.FindIndex((Achievement ach) => ach.Identifier == achievementIdentifier);
			bool unlocked = achIndex >= 0 && achievements[achIndex].Trigger(true);
			if (!unlocked)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to unlock achievement \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(achievementIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return unlocked;
		}

		// Token: 0x06004785 RID: 18309 RVA: 0x002727E1 File Offset: 0x002709E1
		public static void IncrementStats([TupleElementNames(new string[]
		{
			"Identifier",
			"Increment"
		})] params ValueTuple<AchievementStat, float>[] stats)
		{
			Array.ForEach<ValueTuple<AchievementStat, float>>(stats, delegate([TupleElementNames(new string[]
			{
				"Identifier",
				"Increment"
			})] ValueTuple<AchievementStat, float> s)
			{
				SteamManager.IncrementStat(s.Item1, s.Item2, false);
			});
		}

		// Token: 0x06004786 RID: 18310 RVA: 0x00272808 File Offset: 0x00270A08
		public static bool IncrementStat(AchievementStat statName, int increment, bool storeStats = true)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return false;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Incremented stat \"");
			defaultInterpolatedStringHandler.AppendFormatted<AchievementStat>(statName);
			defaultInterpolatedStringHandler.AppendLiteral("\" by ");
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear() + increment.ToString());
			bool success = SteamUserStats.AddStatInt(statName.ToIdentifier<AchievementStat>().Value.ToLowerInvariant(), increment);
			if (!success)
			{
				DebugConsole.Log("Failed to increment stat \"" + statName.ToString() + "\".");
			}
			else if (storeStats)
			{
				SteamManager.StoreStats();
			}
			return success;
		}

		// Token: 0x06004787 RID: 18311 RVA: 0x002728B4 File Offset: 0x00270AB4
		public static bool IncrementStat(AchievementStat statName, float increment, bool storeStats = true)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return false;
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(23, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Incremented stat \"");
			defaultInterpolatedStringHandler.AppendFormatted<AchievementStat>(statName);
			defaultInterpolatedStringHandler.AppendLiteral("\" by ");
			DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear() + increment.ToString());
			bool success = SteamUserStats.AddStatFloat(statName.ToIdentifier<AchievementStat>().Value.ToLowerInvariant(), increment);
			if (!success)
			{
				DebugConsole.Log("Failed to increment stat \"" + statName.ToString() + "\".");
			}
			else if (storeStats)
			{
				SteamManager.StoreStats();
			}
			return success;
		}

		// Token: 0x06004788 RID: 18312 RVA: 0x0027295D File Offset: 0x00270B5D
		public static int GetStatInt(AchievementStat stat)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return 0;
			}
			return SteamUserStats.GetStatInt(stat.ToString().ToLowerInvariant());
		}

		// Token: 0x06004789 RID: 18313 RVA: 0x00272986 File Offset: 0x00270B86
		public static float GetStatFloat(AchievementStat stat)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return 0f;
			}
			return SteamUserStats.GetStatFloat(stat.ToString().ToLowerInvariant());
		}

		// Token: 0x0600478A RID: 18314 RVA: 0x002729B4 File Offset: 0x00270BB4
		public static ImmutableDictionary<AchievementStat, float> GetAllStats()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return ImmutableDictionary<AchievementStat, float>.Empty;
			}
			ImmutableDictionary<AchievementStat, float>.Builder builder = ImmutableDictionary.CreateBuilder<AchievementStat, float>();
			foreach (AchievementStat stat in AchievementStatExtension.SteamStats)
			{
				if (stat.IsFloatStat())
				{
					builder.Add(stat, SteamManager.GetStatFloat(stat));
				}
				else
				{
					builder.Add(stat, (float)SteamManager.GetStatInt(stat));
				}
			}
			return builder.ToImmutable();
		}

		// Token: 0x0600478B RID: 18315 RVA: 0x00272A24 File Offset: 0x00270C24
		public static bool StoreStats()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return false;
			}
			DebugConsole.Log("Storing Steam stats...");
			bool success = SteamUserStats.StoreStats();
			if (!success)
			{
				DebugConsole.Log("Failed to store Steam stats.");
			}
			return success;
		}

		// Token: 0x0600478C RID: 18316 RVA: 0x00272A60 File Offset: 0x00270C60
		public static bool TryGetUnlockedAchievements(out List<Achievement> achievements)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				achievements = null;
				return false;
			}
			achievements = (from a in SteamUserStats.Achievements
			where a.State
			select a).ToList<Achievement>();
			return true;
		}

		// Token: 0x0600478D RID: 18317 RVA: 0x00272AB1 File Offset: 0x00270CB1
		public static bool TryGetAllAvailableAchievements(out List<Achievement> achievements)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				achievements = null;
				return false;
			}
			achievements = SteamUserStats.Achievements.ToList<Achievement>();
			return true;
		}

		// Token: 0x0600478E RID: 18318 RVA: 0x00272AD3 File Offset: 0x00270CD3
		public static void Update(float deltaTime)
		{
			AchievementManager.Update(deltaTime);
			SteamTimelineManager.Update(deltaTime);
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			if (SteamClient.IsValid)
			{
				SteamClient.RunCallbacks();
			}
			if (SteamServer.IsValid)
			{
				SteamServer.RunCallbacks();
			}
		}

		// Token: 0x0600478F RID: 18319 RVA: 0x00272B01 File Offset: 0x00270D01
		public static void ShutDown()
		{
			if (!SteamManager.IsInitialized)
			{
				return;
			}
			if (SteamClient.IsValid)
			{
				SteamClient.Shutdown();
			}
			if (SteamServer.IsValid)
			{
				SteamServer.Shutdown();
			}
		}

		// Token: 0x06004790 RID: 18320 RVA: 0x00272B23 File Offset: 0x00270D23
		public static IEnumerable<ulong> WorkshopUrlsToIds(IEnumerable<string> urls)
		{
			return urls.Select(delegate(string u)
			{
				if (string.IsNullOrEmpty(u))
				{
					return 0UL;
				}
				return SteamManager.GetWorkshopItemIDFromUrl(u);
			});
		}

		// Token: 0x06004791 RID: 18321 RVA: 0x00272B4C File Offset: 0x00270D4C
		public static ulong GetWorkshopItemIDFromUrl(string url)
		{
			try
			{
				Uri uri = new Uri(url);
				string idStr = HttpUtility.ParseQueryString(uri.Query)["id".ToIdentifier()];
				ulong id;
				if (ulong.TryParse(idStr, out id))
				{
					return id;
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to get Workshop item ID from the url \"" + url + "\"!", e, null, false, false);
			}
			return 0UL;
		}

		// Token: 0x06004792 RID: 18322 RVA: 0x00272BC0 File Offset: 0x00270DC0
		[NullableContext(1)]
		public static bool TryExtractSteamWorkshopId(this ContentPackage contentPackage, [Nullable(2)] [NotNullWhen(true)] out SteamWorkshopId workshopId)
		{
			workshopId = null;
			ContentPackageId ugcId;
			if (!contentPackage.UgcId.TryUnwrap(out ugcId))
			{
				return false;
			}
			SteamWorkshopId steamWorkshopId = ugcId as SteamWorkshopId;
			if (steamWorkshopId == null)
			{
				return false;
			}
			workshopId = steamWorkshopId;
			return true;
		}

		// Token: 0x06004793 RID: 18323 RVA: 0x00272BF4 File Offset: 0x00270DF4
		// Note: this type is marked as 'beforefieldinit'.
		static SteamManager()
		{
			Option.UnspecifiedNone none = Option.None;
			SteamManager.currentSteamHostAuthTicket = none;
			none = Option.None;
			SteamManager.currentEosHostAuthTicket = none;
			none = Option.None;
			SteamManager.currentGameAnalyticsConsentTicket = none;
			SteamManager.tagCommonness = new Dictionary<string, int>
			{
				{
					"submarine",
					10
				},
				{
					"item",
					10
				},
				{
					"monster",
					8
				},
				{
					"art",
					8
				},
				{
					"mission",
					8
				},
				{
					"event set",
					8
				},
				{
					"total conversion",
					5
				},
				{
					"environment",
					5
				},
				{
					"item assembly",
					5
				},
				{
					"language",
					5
				}
			};
			SteamManager.popularTags = new List<string>();
		}

		// Token: 0x04002523 RID: 9507
		private static ulong lobbyID = 0UL;

		// Token: 0x04002524 RID: 9508
		private static SteamManager.LobbyState lobbyState = SteamManager.LobbyState.NotConnected;

		// Token: 0x04002525 RID: 9509
		private static Lobby? currentLobby;

		// Token: 0x04002526 RID: 9510
		private static readonly List<Identifier> initializationErrors = new List<Identifier>();

		// Token: 0x04002528 RID: 9512
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private static Option<AuthTicket> currentSteamHostAuthTicket;

		// Token: 0x04002529 RID: 9513
		[Nullable(1)]
		private const string EosHostAuthIdentity = "BarotraumaRemotePlayerAuth";

		// Token: 0x0400252A RID: 9514
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private static Option<AuthTicket> currentEosHostAuthTicket;

		// Token: 0x0400252B RID: 9515
		[Nullable(1)]
		private const string GameAnalyticsConsentIdentity = "BarotraumaGameAnalyticsConsent";

		// Token: 0x0400252C RID: 9516
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private static Option<AuthTicket> currentGameAnalyticsConsentTicket;

		// Token: 0x0400252D RID: 9517
		public const int STEAMP2P_OWNER_PORT = 30000;

		// Token: 0x0400252E RID: 9518
		public const uint AppID = 602960U;

		// Token: 0x0400252F RID: 9519
		private static readonly Dictionary<string, int> tagCommonness;

		// Token: 0x04002530 RID: 9520
		private static readonly List<string> popularTags;

		// Token: 0x020010F3 RID: 4339
		private enum LobbyState
		{
			// Token: 0x04005A1A RID: 23066
			NotConnected,
			// Token: 0x04005A1B RID: 23067
			Creating,
			// Token: 0x04005A1C RID: 23068
			Owner,
			// Token: 0x04005A1D RID: 23069
			Joining,
			// Token: 0x04005A1E RID: 23070
			Joined
		}

		// Token: 0x020010F4 RID: 4340
		[NullableContext(1)]
		[Nullable(0)]
		public static class Workshop
		{
			// Token: 0x06008E47 RID: 36423 RVA: 0x003B3D82 File Offset: 0x003B1F82
			public static void DeletePublishStagingCopy()
			{
				if (Directory.Exists("WorkshopStaging"))
				{
					Directory.Delete("WorkshopStaging", true, true);
				}
			}

			// Token: 0x06008E48 RID: 36424 RVA: 0x003B3D9C File Offset: 0x003B1F9C
			private static void RefreshLocalMods()
			{
				CrossThread.RequestExecutionOnMainThread(delegate
				{
					ContentPackageManager.LocalPackages.Refresh();
				});
			}

			// Token: 0x06008E49 RID: 36425 RVA: 0x003B3DC4 File Offset: 0x003B1FC4
			public static Task CreatePublishStagingCopy(string title, string modVersion, ContentPackage contentPackage)
			{
				SteamManager.Workshop.<CreatePublishStagingCopy>d__6 <CreatePublishStagingCopy>d__;
				<CreatePublishStagingCopy>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<CreatePublishStagingCopy>d__.title = title;
				<CreatePublishStagingCopy>d__.modVersion = modVersion;
				<CreatePublishStagingCopy>d__.contentPackage = contentPackage;
				<CreatePublishStagingCopy>d__.<>1__state = -1;
				<CreatePublishStagingCopy>d__.<>t__builder.Start<SteamManager.Workshop.<CreatePublishStagingCopy>d__6>(ref <CreatePublishStagingCopy>d__);
				return <CreatePublishStagingCopy>d__.<>t__builder.Task;
			}

			// Token: 0x06008E4A RID: 36426 RVA: 0x003B3E18 File Offset: 0x003B2018
			[return: Nullable(new byte[]
			{
				1,
				0,
				1
			})]
			public static Task<Option<ContentPackage>> CreateLocalCopy(ContentPackage contentPackage)
			{
				SteamManager.Workshop.<CreateLocalCopy>d__7 <CreateLocalCopy>d__;
				<CreateLocalCopy>d__.<>t__builder = AsyncTaskMethodBuilder<Option<ContentPackage>>.Create();
				<CreateLocalCopy>d__.contentPackage = contentPackage;
				<CreateLocalCopy>d__.<>1__state = -1;
				<CreateLocalCopy>d__.<>t__builder.Start<SteamManager.Workshop.<CreateLocalCopy>d__7>(ref <CreateLocalCopy>d__);
				return <CreateLocalCopy>d__.<>t__builder.Task;
			}

			// Token: 0x06008E4B RID: 36427 RVA: 0x003B3E5C File Offset: 0x003B205C
			public static Task Reinstall(Item workshopItem)
			{
				SteamManager.Workshop.<Reinstall>d__9 <Reinstall>d__;
				<Reinstall>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<Reinstall>d__.workshopItem = workshopItem;
				<Reinstall>d__.<>1__state = -1;
				<Reinstall>d__.<>t__builder.Start<SteamManager.Workshop.<Reinstall>d__9>(ref <Reinstall>d__);
				return <Reinstall>d__.<>t__builder.Task;
			}

			// Token: 0x06008E4C RID: 36428 RVA: 0x003B3EA0 File Offset: 0x003B20A0
			public static Task WaitForInstall(Item item)
			{
				SteamManager.Workshop.<WaitForInstall>d__10 <WaitForInstall>d__;
				<WaitForInstall>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<WaitForInstall>d__.item = item;
				<WaitForInstall>d__.<>1__state = -1;
				<WaitForInstall>d__.<>t__builder.Start<SteamManager.Workshop.<WaitForInstall>d__10>(ref <WaitForInstall>d__);
				return <WaitForInstall>d__.<>t__builder.Task;
			}

			// Token: 0x06008E4D RID: 36429 RVA: 0x003B3EE4 File Offset: 0x003B20E4
			public static Task WaitForInstall(ulong item)
			{
				SteamManager.Workshop.<WaitForInstall>d__11 <WaitForInstall>d__;
				<WaitForInstall>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<WaitForInstall>d__.item = item;
				<WaitForInstall>d__.<>1__state = -1;
				<WaitForInstall>d__.<>t__builder.Start<SteamManager.Workshop.<WaitForInstall>d__11>(ref <WaitForInstall>d__);
				return <WaitForInstall>d__.<>t__builder.Task;
			}

			// Token: 0x06008E4E RID: 36430 RVA: 0x003B3F28 File Offset: 0x003B2128
			public static void OnItemDownloadComplete(ulong id, bool forceInstall = false)
			{
				if (!(Screen.Selected is MainMenuScreen) && !forceInstall)
				{
					if (!MainMenuScreen.WorkshopItemsToUpdate.Contains(id))
					{
						MainMenuScreen.WorkshopItemsToUpdate.Enqueue(id);
					}
					return;
				}
				if (!SteamManager.Workshop.CanBeInstalled(id))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Cannot install ");
					defaultInterpolatedStringHandler.AppendFormatted<ulong>(id);
					DebugConsole.Log(defaultInterpolatedStringHandler.ToStringAndClear());
					SteamManager.Workshop.InstallWaiter.StopWaiting(id);
					return;
				}
				if (ContentPackageManager.WorkshopPackages.Any(delegate(ContentPackage p)
				{
					ContentPackageId ugcId;
					if (p.UgcId.TryUnwrap(out ugcId))
					{
						SteamWorkshopId workshopId = ugcId as SteamWorkshopId;
						if (workshopId != null)
						{
							return workshopId.Value == id;
						}
					}
					return false;
				}))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(19, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Already installed ");
					defaultInterpolatedStringHandler2.AppendFormatted<ulong>(id);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					DebugConsole.Log(defaultInterpolatedStringHandler2.ToStringAndClear());
					SteamManager.Workshop.InstallWaiter.StopWaiting(id);
					return;
				}
				if (SteamManager.Workshop.InstallTaskCounter.IsInstalling(id))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(20, 1);
					defaultInterpolatedStringHandler3.AppendLiteral("Already installing ");
					defaultInterpolatedStringHandler3.AppendFormatted<ulong>(id);
					defaultInterpolatedStringHandler3.AppendLiteral(".");
					DebugConsole.Log(defaultInterpolatedStringHandler3.ToStringAndClear());
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(36, 1);
				defaultInterpolatedStringHandler4.AppendLiteral("Finished downloading ");
				defaultInterpolatedStringHandler4.AppendFormatted<ulong>(id);
				defaultInterpolatedStringHandler4.AppendLiteral(", installing...");
				DebugConsole.Log(defaultInterpolatedStringHandler4.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler5.AppendLiteral("InstallItem");
				defaultInterpolatedStringHandler5.AppendFormatted<ulong>(id);
				TaskPool.Add(defaultInterpolatedStringHandler5.ToStringAndClear(), SteamManager.Workshop.InstallMod(id), delegate(Task t)
				{
					SteamManager.Workshop.InstallWaiter.StopWaiting(id);
				});
			}

			// Token: 0x06008E4F RID: 36431 RVA: 0x003B40E8 File Offset: 0x003B22E8
			private static Task<ISet<Item>> GetWorkshopItems(Query query, int? maxPages = null)
			{
				SteamManager.Workshop.<GetWorkshopItems>d__14 <GetWorkshopItems>d__;
				<GetWorkshopItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetWorkshopItems>d__.query = query;
				<GetWorkshopItems>d__.maxPages = maxPages;
				<GetWorkshopItems>d__.<>1__state = -1;
				<GetWorkshopItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetWorkshopItems>d__14>(ref <GetWorkshopItems>d__);
				return <GetWorkshopItems>d__.<>t__builder.Task;
			}

			// Token: 0x06008E50 RID: 36432 RVA: 0x003B4133 File Offset: 0x003B2333
			public static ImmutableHashSet<PublishedFileId> GetSubscribedItemIds()
			{
				if (!SteamManager.IsInitialized)
				{
					return ImmutableHashSet<PublishedFileId>.Empty;
				}
				return SteamUGC.GetSubscribedItems().ToImmutableHashSet<PublishedFileId>();
			}

			// Token: 0x06008E51 RID: 36433 RVA: 0x003B414C File Offset: 0x003B234C
			public static Task<ISet<Item>> GetAllSubscribedItems()
			{
				SteamManager.Workshop.<GetAllSubscribedItems>d__16 <GetAllSubscribedItems>d__;
				<GetAllSubscribedItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetAllSubscribedItems>d__.<>1__state = -1;
				<GetAllSubscribedItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetAllSubscribedItems>d__16>(ref <GetAllSubscribedItems>d__);
				return <GetAllSubscribedItems>d__.<>t__builder.Task;
			}

			// Token: 0x06008E52 RID: 36434 RVA: 0x003B4188 File Offset: 0x003B2388
			public static Task<ISet<Item>> GetPopularItems()
			{
				SteamManager.Workshop.<GetPopularItems>d__17 <GetPopularItems>d__;
				<GetPopularItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetPopularItems>d__.<>1__state = -1;
				<GetPopularItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetPopularItems>d__17>(ref <GetPopularItems>d__);
				return <GetPopularItems>d__.<>t__builder.Task;
			}

			// Token: 0x06008E53 RID: 36435 RVA: 0x003B41C4 File Offset: 0x003B23C4
			public static Task<ISet<Item>> GetPublishedItems()
			{
				SteamManager.Workshop.<GetPublishedItems>d__18 <GetPublishedItems>d__;
				<GetPublishedItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetPublishedItems>d__.<>1__state = -1;
				<GetPublishedItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetPublishedItems>d__18>(ref <GetPublishedItems>d__);
				return <GetPublishedItems>d__.<>t__builder.Task;
			}

			// Token: 0x06008E54 RID: 36436 RVA: 0x003B41FF File Offset: 0x003B23FF
			[return: Nullable(new byte[]
			{
				1,
				0
			})]
			public static Task<Option<Item>> GetItem(ulong itemId)
			{
				return SteamManager.Workshop.SingleItemRequestPool.MakeRequest(itemId);
			}

			// Token: 0x06008E55 RID: 36437 RVA: 0x003B4208 File Offset: 0x003B2408
			[return: Nullable(new byte[]
			{
				1,
				0
			})]
			public static Task<Option<Item>> GetItemAsap(ulong itemId, bool withLongDescription = false)
			{
				SteamManager.Workshop.<GetItemAsap>d__21 <GetItemAsap>d__;
				<GetItemAsap>d__.<>t__builder = AsyncTaskMethodBuilder<Option<Item>>.Create();
				<GetItemAsap>d__.itemId = itemId;
				<GetItemAsap>d__.withLongDescription = withLongDescription;
				<GetItemAsap>d__.<>1__state = -1;
				<GetItemAsap>d__.<>t__builder.Start<SteamManager.Workshop.<GetItemAsap>d__21>(ref <GetItemAsap>d__);
				return <GetItemAsap>d__.<>t__builder.Task;
			}

			// Token: 0x06008E56 RID: 36438 RVA: 0x003B4254 File Offset: 0x003B2454
			public static Task ForceRedownload(ulong itemId)
			{
				SteamManager.Workshop.<ForceRedownload>d__22 <ForceRedownload>d__;
				<ForceRedownload>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<ForceRedownload>d__.itemId = itemId;
				<ForceRedownload>d__.<>1__state = -1;
				<ForceRedownload>d__.<>t__builder.Start<SteamManager.Workshop.<ForceRedownload>d__22>(ref <ForceRedownload>d__);
				return <ForceRedownload>d__.<>t__builder.Task;
			}

			// Token: 0x06008E57 RID: 36439 RVA: 0x003B4298 File Offset: 0x003B2498
			public static void NukeDownload(Item item)
			{
				try
				{
					Directory.Delete(item.Directory ?? "", true);
				}
				catch
				{
				}
			}

			// Token: 0x06008E58 RID: 36440 RVA: 0x003B42D0 File Offset: 0x003B24D0
			public static void Uninstall(Item workshopItem)
			{
				SteamManager.Workshop.NukeDownload(workshopItem);
				HashSet<ContentPackage> toUninstall = ContentPackageManager.WorkshopPackages.Where(delegate(ContentPackage p)
				{
					ContentPackageId ugcId;
					if (p.UgcId.TryUnwrap(out ugcId))
					{
						SteamWorkshopId steamWorkshopId = ugcId as SteamWorkshopId;
						if (steamWorkshopId != null)
						{
							ulong itemId = steamWorkshopId.Value;
							return itemId == workshopItem.Id;
						}
					}
					return false;
				}).ToHashSet<ContentPackage>();
				ContentPackageManager.EnabledPackages.DisableMods(toUninstall);
				(from p in toUninstall
				select p.Dir).ForEach(delegate(string d)
				{
					Directory.Delete(d, true, true);
				});
				ContentPackageManager.WorkshopPackages.Refresh();
				ContentPackageManager.EnabledPackages.DisableRemovedMods();
			}

			// Token: 0x06008E59 RID: 36441 RVA: 0x003B4370 File Offset: 0x003B2570
			public static Task ForceRedownload(Item item, [Nullable(2)] CancellationTokenSource cancellationTokenSrc = null)
			{
				SteamManager.Workshop.<ForceRedownload>d__25 <ForceRedownload>d__;
				<ForceRedownload>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<ForceRedownload>d__.item = item;
				<ForceRedownload>d__.cancellationTokenSrc = cancellationTokenSrc;
				<ForceRedownload>d__.<>1__state = -1;
				<ForceRedownload>d__.<>t__builder.Start<SteamManager.Workshop.<ForceRedownload>d__25>(ref <ForceRedownload>d__);
				return <ForceRedownload>d__.<>t__builder.Task;
			}

			// Token: 0x06008E5A RID: 36442 RVA: 0x003B43BC File Offset: 0x003B25BC
			public static bool IsItemDirectoryUpToDate(in Item item)
			{
				Item item2 = item;
				string itemDirectory = item2.Directory ?? "";
				if (Directory.Exists(itemDirectory))
				{
					DateTime t = File.GetLastWriteTime(itemDirectory).ToUniversalTime();
					item2 = item;
					return t >= item2.LatestUpdateTime;
				}
				return false;
			}

			// Token: 0x06008E5B RID: 36443 RVA: 0x003B440C File Offset: 0x003B260C
			public static bool CanBeInstalled(ulong itemId)
			{
				Item item = new Item(itemId);
				return SteamManager.Workshop.CanBeInstalled(item);
			}

			// Token: 0x06008E5C RID: 36444 RVA: 0x003B442C File Offset: 0x003B262C
			public static bool CanBeInstalled(in Item item)
			{
				Item item2 = item;
				bool needsUpdate = item2.NeedsUpdate;
				item2 = item;
				bool isDownloading = item2.IsDownloading;
				item2 = item;
				bool isInstalled = item2.IsInstalled;
				bool directoryIsUpToDate = SteamManager.Workshop.IsItemDirectoryUpToDate(item);
				return !needsUpdate && !isDownloading && isInstalled && directoryIsUpToDate;
			}

			// Token: 0x06008E5D RID: 36445 RVA: 0x003B4480 File Offset: 0x003B2680
			public static Task DownloadModThenEnqueueInstall(Item item)
			{
				SteamManager.Workshop.<DownloadModThenEnqueueInstall>d__31 <DownloadModThenEnqueueInstall>d__;
				<DownloadModThenEnqueueInstall>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<DownloadModThenEnqueueInstall>d__.item = item;
				<DownloadModThenEnqueueInstall>d__.<>1__state = -1;
				<DownloadModThenEnqueueInstall>d__.<>t__builder.Start<SteamManager.Workshop.<DownloadModThenEnqueueInstall>d__31>(ref <DownloadModThenEnqueueInstall>d__);
				return <DownloadModThenEnqueueInstall>d__.<>t__builder.Task;
			}

			// Token: 0x06008E5E RID: 36446 RVA: 0x003B44C4 File Offset: 0x003B26C4
			public static void DeleteFailedCopies()
			{
				if (Directory.Exists(ContentPackage.WorkshopModsDir))
				{
					foreach (string dir in Directory.EnumerateDirectories(ContentPackage.WorkshopModsDir, "**"))
					{
						string copyingIndicatorPath = Path.Combine(new string[]
						{
							dir,
							".copying"
						});
						if (File.Exists(copyingIndicatorPath))
						{
							Directory.Delete(dir, true, true);
						}
					}
				}
			}

			// Token: 0x06008E5F RID: 36447 RVA: 0x003B4548 File Offset: 0x003B2748
			public static ISet<ulong> GetInstalledItems()
			{
				return (from id in (from p in ContentPackageManager.WorkshopPackages
				select p.UgcId).NotNone<ContentPackageId>().OfType<SteamWorkshopId>()
				select id.Value).ToHashSet<ulong>();
			}

			// Token: 0x06008E60 RID: 36448 RVA: 0x003B45B4 File Offset: 0x003B27B4
			public static Task<ISet<Item>> GetPublishedAndSubscribedItems()
			{
				SteamManager.Workshop.<GetPublishedAndSubscribedItems>d__34 <GetPublishedAndSubscribedItems>d__;
				<GetPublishedAndSubscribedItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetPublishedAndSubscribedItems>d__.<>1__state = -1;
				<GetPublishedAndSubscribedItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetPublishedAndSubscribedItems>d__34>(ref <GetPublishedAndSubscribedItems>d__);
				return <GetPublishedAndSubscribedItems>d__.<>t__builder.Task;
			}

			// Token: 0x06008E61 RID: 36449 RVA: 0x003B45F0 File Offset: 0x003B27F0
			public static void DeleteUnsubscribedMods([Nullable(new byte[]
			{
				2,
				1,
				1
			})] Action<ContentPackage[]> callback = null)
			{
				if (!SteamManager.IsInitialized)
				{
					return;
				}
				if (!SteamClient.IsValid)
				{
					return;
				}
				if (!SteamClient.IsLoggedOn)
				{
					return;
				}
				TaskPool.Add("DeleteUnsubscribedMods", SteamManager.Workshop.GetPublishedAndSubscribedItems().WaitForLoadingScreen<ISet<Item>>(), delegate(Task t)
				{
					ISet<Item> items;
					if (!t.TryGetResult(out items))
					{
						return;
					}
					HashSet<ulong> ids = (from it in items
					select it.Id.Value).ToHashSet<ulong>();
					ContentPackage[] toUninstall = ContentPackageManager.WorkshopPackages.Where(delegate(ContentPackage pkg)
					{
						SteamWorkshopId workshopId;
						return !pkg.UgcId.TryUnwrap<SteamWorkshopId>(out workshopId) || !ids.Contains(workshopId.Value);
					}).ToArray<ContentPackage>();
					if (toUninstall.Any<ContentPackage>())
					{
						foreach (ContentPackage pkg2 in toUninstall)
						{
							Directory.TryDelete(pkg2.Dir, true);
						}
						ContentPackageManager.UpdateContentPackageList();
					}
					Action<ContentPackage[]> callback2 = callback;
					if (callback2 == null)
					{
						return;
					}
					callback2(toUninstall);
				});
			}

			// Token: 0x06008E62 RID: 36450 RVA: 0x003B4643 File Offset: 0x003B2843
			public static bool IsInstallingToPath(string path)
			{
				return File.Exists(Path.Combine(new string[]
				{
					Path.GetDirectoryName(path),
					".copying"
				}));
			}

			// Token: 0x06008E63 RID: 36451 RVA: 0x003B4666 File Offset: 0x003B2866
			public static bool IsInstalling(Item item)
			{
				return SteamManager.Workshop.InstallTaskCounter.IsInstalling(item);
			}

			// Token: 0x06008E64 RID: 36452 RVA: 0x003B4670 File Offset: 0x003B2870
			private static Task InstallMod(ulong id)
			{
				SteamManager.Workshop.<InstallMod>d__38 <InstallMod>d__;
				<InstallMod>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<InstallMod>d__.id = id;
				<InstallMod>d__.<>1__state = -1;
				<InstallMod>d__.<>t__builder.Start<SteamManager.Workshop.<InstallMod>d__38>(ref <InstallMod>d__);
				return <InstallMod>d__.<>t__builder.Task;
			}

			// Token: 0x06008E65 RID: 36453 RVA: 0x003B46B4 File Offset: 0x003B28B4
			private static Task CorrectPaths(string fileListDir, string modName, XElement element)
			{
				SteamManager.Workshop.<CorrectPaths>d__39 <CorrectPaths>d__;
				<CorrectPaths>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<CorrectPaths>d__.fileListDir = fileListDir;
				<CorrectPaths>d__.modName = modName;
				<CorrectPaths>d__.element = element;
				<CorrectPaths>d__.<>1__state = -1;
				<CorrectPaths>d__.<>t__builder.Start<SteamManager.Workshop.<CorrectPaths>d__39>(ref <CorrectPaths>d__);
				return <CorrectPaths>d__.<>t__builder.Task;
			}

			// Token: 0x06008E66 RID: 36454 RVA: 0x003B4708 File Offset: 0x003B2908
			private static Task CopyFile(string fileListDir, string modName, string from, string to, SteamManager.Workshop.ShouldCorrectPaths shouldCorrectPaths)
			{
				SteamManager.Workshop.<CopyFile>d__40 <CopyFile>d__;
				<CopyFile>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<CopyFile>d__.fileListDir = fileListDir;
				<CopyFile>d__.modName = modName;
				<CopyFile>d__.from = from;
				<CopyFile>d__.to = to;
				<CopyFile>d__.shouldCorrectPaths = shouldCorrectPaths;
				<CopyFile>d__.<>1__state = -1;
				<CopyFile>d__.<>t__builder.Start<SteamManager.Workshop.<CopyFile>d__40>(ref <CopyFile>d__);
				return <CopyFile>d__.<>t__builder.Task;
			}

			// Token: 0x06008E67 RID: 36455 RVA: 0x003B476C File Offset: 0x003B296C
			public static Task CopyDirectory(string fileListDir, string modName, string from, string to, SteamManager.Workshop.ShouldCorrectPaths shouldCorrectPaths)
			{
				SteamManager.Workshop.<CopyDirectory>d__42 <CopyDirectory>d__;
				<CopyDirectory>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<CopyDirectory>d__.fileListDir = fileListDir;
				<CopyDirectory>d__.modName = modName;
				<CopyDirectory>d__.from = from;
				<CopyDirectory>d__.to = to;
				<CopyDirectory>d__.shouldCorrectPaths = shouldCorrectPaths;
				<CopyDirectory>d__.<>1__state = -1;
				<CopyDirectory>d__.<>t__builder.Start<SteamManager.Workshop.<CopyDirectory>d__42>(ref <CopyDirectory>d__);
				return <CopyDirectory>d__.<>t__builder.Task;
			}

			// Token: 0x06008E69 RID: 36457 RVA: 0x003B48C7 File Offset: 0x003B2AC7
			[CompilerGenerated]
			internal static string <CopyDirectory>g__convertFromTo|42_0(string from, ref SteamManager.Workshop.<>c__DisplayClass42_0 A_1)
			{
				return Path.Combine(new string[]
				{
					A_1.to,
					Path.GetFileName(from)
				});
			}

			// Token: 0x04005A1F RID: 23071
			public const int MaxThumbnailSize = 1048576;

			// Token: 0x04005A20 RID: 23072
			[Nullable(0)]
			public static readonly ImmutableArray<Identifier> Tags = new string[]
			{
				"submarine",
				"item",
				"monster",
				"mission",
				"outpost",
				"beacon station",
				"wreck",
				"ruin",
				"weapons",
				"medical",
				"equipment",
				"art",
				"event set",
				"total conversion",
				"game mode",
				"gameplay mechanics",
				"environment",
				"item assembly",
				"language",
				"qol",
				"client-side",
				"server-side",
				"outdated",
				"library"
			}.ToIdentifiers().ToImmutableArray<Identifier>();

			// Token: 0x04005A21 RID: 23073
			public const string PublishStagingDir = "WorkshopStaging";

			// Token: 0x0200158B RID: 5515
			[NullableContext(0)]
			public class ItemThumbnail : IDisposable
			{
				// Token: 0x17001DAD RID: 7597
				// (get) Token: 0x06009E38 RID: 40504 RVA: 0x003EEEBE File Offset: 0x003ED0BE
				// (set) Token: 0x06009E39 RID: 40505 RVA: 0x003EEEC6 File Offset: 0x003ED0C6
				public ulong ItemId { get; private set; }

				// Token: 0x17001DAE RID: 7598
				// (get) Token: 0x06009E3A RID: 40506 RVA: 0x003EEED0 File Offset: 0x003ED0D0
				[Nullable(2)]
				public Texture2D Texture
				{
					[NullableContext(2)]
					get
					{
						Dictionary<ulong, SteamManager.Workshop.ItemThumbnail.RefCounter> textureRefs = SteamManager.Workshop.ItemThumbnail.TextureRefs;
						lock (textureRefs)
						{
							SteamManager.Workshop.ItemThumbnail.RefCounter refCounter;
							if (SteamManager.Workshop.ItemThumbnail.TextureRefs.TryGetValue(this.ItemId, out refCounter))
							{
								return refCounter.Texture;
							}
						}
						return null;
					}
				}

				// Token: 0x17001DAF RID: 7599
				// (get) Token: 0x06009E3B RID: 40507 RVA: 0x003EEF2C File Offset: 0x003ED12C
				public bool Loading
				{
					get
					{
						Dictionary<ulong, SteamManager.Workshop.ItemThumbnail.RefCounter> textureRefs = SteamManager.Workshop.ItemThumbnail.TextureRefs;
						lock (textureRefs)
						{
							SteamManager.Workshop.ItemThumbnail.RefCounter refCounter;
							if (SteamManager.Workshop.ItemThumbnail.TextureRefs.TryGetValue(this.ItemId, out refCounter))
							{
								return refCounter.Loading;
							}
						}
						return false;
					}
				}

				// Token: 0x06009E3C RID: 40508 RVA: 0x003EEF88 File Offset: 0x003ED188
				public ItemThumbnail(in Item item, CancellationToken cancellationToken)
				{
					Item item2 = item;
					this.ItemId = item2.Id;
					Dictionary<ulong, SteamManager.Workshop.ItemThumbnail.RefCounter> textureRefs = SteamManager.Workshop.ItemThumbnail.TextureRefs;
					lock (textureRefs)
					{
						SteamManager.Workshop.ItemThumbnail.RefCounter refCounter;
						if (SteamManager.Workshop.ItemThumbnail.TextureRefs.TryGetValue(this.ItemId, out refCounter))
						{
							SteamManager.Workshop.ItemThumbnail.TextureRefs[this.ItemId] = new SteamManager.Workshop.ItemThumbnail.RefCounter
							{
								Texture = refCounter.Texture,
								Count = refCounter.Count + 1,
								Loading = refCounter.Loading
							};
						}
						else
						{
							SteamManager.Workshop.ItemThumbnail.TextureRefs[this.ItemId] = new SteamManager.Workshop.ItemThumbnail.RefCounter
							{
								Texture = null,
								Count = 1,
								Loading = true
							};
							string name = "Workshop thumbnail " + item.Title;
							Task texture = SteamManager.Workshop.ItemThumbnail.GetTexture(item, cancellationToken);
							item2 = item;
							TaskPool.Add(name, texture, SteamManager.Workshop.ItemThumbnail.SaveTextureToRefCounter(item2.Id));
						}
					}
				}

				// Token: 0x06009E3D RID: 40509 RVA: 0x003EF0A8 File Offset: 0x003ED2A8
				public void Dispose()
				{
					if (this.ItemId == 0UL)
					{
						return;
					}
					Dictionary<ulong, SteamManager.Workshop.ItemThumbnail.RefCounter> textureRefs = SteamManager.Workshop.ItemThumbnail.TextureRefs;
					lock (textureRefs)
					{
						SteamManager.Workshop.ItemThumbnail.RefCounter refCounter = SteamManager.Workshop.ItemThumbnail.TextureRefs[this.ItemId];
						SteamManager.Workshop.ItemThumbnail.TextureRefs[this.ItemId] = new SteamManager.Workshop.ItemThumbnail.RefCounter
						{
							Texture = refCounter.Texture,
							Count = refCounter.Count - 1
						};
						if (SteamManager.Workshop.ItemThumbnail.TextureRefs[this.ItemId].Count <= 0)
						{
							Texture2D texture = SteamManager.Workshop.ItemThumbnail.TextureRefs[this.ItemId].Texture;
							if (texture != null)
							{
								texture.Dispose();
							}
							SteamManager.Workshop.ItemThumbnail.TextureRefs.Remove(this.ItemId);
						}
						this.ItemId = 0UL;
					}
				}

				// Token: 0x06009E3E RID: 40510 RVA: 0x003EF184 File Offset: 0x003ED384
				[return: Nullable(new byte[]
				{
					1,
					2
				})]
				private static Task<Texture2D> GetTexture(Item item, CancellationToken cancellationToken)
				{
					SteamManager.Workshop.ItemThumbnail.<GetTexture>d__12 <GetTexture>d__;
					<GetTexture>d__.<>t__builder = AsyncTaskMethodBuilder<Texture2D>.Create();
					<GetTexture>d__.item = item;
					<GetTexture>d__.cancellationToken = cancellationToken;
					<GetTexture>d__.<>1__state = -1;
					<GetTexture>d__.<>t__builder.Start<SteamManager.Workshop.ItemThumbnail.<GetTexture>d__12>(ref <GetTexture>d__);
					return <GetTexture>d__.<>t__builder.Task;
				}

				// Token: 0x06009E3F RID: 40511 RVA: 0x003EF1D0 File Offset: 0x003ED3D0
				[NullableContext(1)]
				private static Action<Task> SaveTextureToRefCounter(ulong itemId)
				{
					return delegate(Task t)
					{
						if (t.IsCanceled)
						{
							return;
						}
						Texture2D texture = ((Task<Texture2D>)t).Result;
						Dictionary<ulong, SteamManager.Workshop.ItemThumbnail.RefCounter> textureRefs = SteamManager.Workshop.ItemThumbnail.TextureRefs;
						lock (textureRefs)
						{
							SteamManager.Workshop.ItemThumbnail.RefCounter refCounter;
							if (SteamManager.Workshop.ItemThumbnail.TextureRefs.TryGetValue(itemId, out refCounter))
							{
								SteamManager.Workshop.ItemThumbnail.TextureRefs[itemId] = new SteamManager.Workshop.ItemThumbnail.RefCounter
								{
									Texture = texture,
									Count = refCounter.Count,
									Loading = false
								};
							}
							else if (texture != null)
							{
								texture.Dispose();
							}
						}
					};
				}

				// Token: 0x06009E40 RID: 40512 RVA: 0x003EF1F6 File Offset: 0x003ED3F6
				public override int GetHashCode()
				{
					return (int)this.ItemId;
				}

				// Token: 0x06009E41 RID: 40513 RVA: 0x003EF200 File Offset: 0x003ED400
				[NullableContext(2)]
				public override bool Equals(object obj)
				{
					SteamManager.Workshop.ItemThumbnail itemThumbnail = obj as SteamManager.Workshop.ItemThumbnail;
					if (itemThumbnail != null)
					{
						ulong otherId = itemThumbnail.ItemId;
						return otherId == this.ItemId;
					}
					return false;
				}

				// Token: 0x040068D6 RID: 26838
				[Nullable(1)]
				private static readonly Dictionary<ulong, SteamManager.Workshop.ItemThumbnail.RefCounter> TextureRefs = new Dictionary<ulong, SteamManager.Workshop.ItemThumbnail.RefCounter>();

				// Token: 0x020015EA RID: 5610
				private struct RefCounter
				{
					// Token: 0x04006A3F RID: 27199
					internal bool Loading;

					// Token: 0x04006A40 RID: 27200
					[Nullable(2)]
					internal Texture2D Texture;

					// Token: 0x04006A41 RID: 27201
					internal int Count;
				}
			}

			// Token: 0x0200158C RID: 5516
			[NullableContext(0)]
			private struct InstallWaiter
			{
				// Token: 0x17001DB0 RID: 7600
				// (get) Token: 0x06009E43 RID: 40515 RVA: 0x003EF235 File Offset: 0x003ED435
				// (set) Token: 0x06009E44 RID: 40516 RVA: 0x003EF23D File Offset: 0x003ED43D
				public ulong Id { readonly get; private set; }

				// Token: 0x06009E45 RID: 40517 RVA: 0x003EF248 File Offset: 0x003ED448
				public InstallWaiter(ulong id)
				{
					this.Id = id;
					HashSet<ulong> obj = SteamManager.Workshop.InstallWaiter.waitingIds;
					lock (obj)
					{
						SteamManager.Workshop.InstallWaiter.waitingIds.Add(this.Id);
					}
				}

				// Token: 0x17001DB1 RID: 7601
				// (get) Token: 0x06009E46 RID: 40518 RVA: 0x003EF29C File Offset: 0x003ED49C
				public bool Waiting
				{
					get
					{
						if (this.Id == 0UL)
						{
							return false;
						}
						HashSet<ulong> obj = SteamManager.Workshop.InstallWaiter.waitingIds;
						bool result;
						lock (obj)
						{
							result = SteamManager.Workshop.InstallWaiter.waitingIds.Contains(this.Id);
						}
						return result;
					}
				}

				// Token: 0x06009E47 RID: 40519 RVA: 0x003EF2F4 File Offset: 0x003ED4F4
				public static void StopWaiting(ulong id)
				{
					HashSet<ulong> obj = SteamManager.Workshop.InstallWaiter.waitingIds;
					lock (obj)
					{
						SteamManager.Workshop.InstallWaiter.waitingIds.Remove(id);
					}
				}

				// Token: 0x040068D8 RID: 26840
				[Nullable(1)]
				private static readonly HashSet<ulong> waitingIds = new HashSet<ulong>();
			}

			// Token: 0x0200158D RID: 5517
			[NullableContext(0)]
			private struct ItemEqualityComparer : IEqualityComparer<Item>
			{
				// Token: 0x06009E49 RID: 40521 RVA: 0x003EF348 File Offset: 0x003ED548
				public bool Equals(Item x, Item y)
				{
					return x.Id == y.Id;
				}

				// Token: 0x06009E4A RID: 40522 RVA: 0x003EF35D File Offset: 0x003ED55D
				public int GetHashCode(Item obj)
				{
					return (int)obj.Id.Value;
				}

				// Token: 0x040068DA RID: 26842
				public static readonly SteamManager.Workshop.ItemEqualityComparer Instance;
			}

			// Token: 0x0200158E RID: 5518
			[Nullable(0)]
			private static class SingleItemRequestPool
			{
				// Token: 0x06009E4C RID: 40524 RVA: 0x003EF370 File Offset: 0x003ED570
				private static Task<ISet<Item>> PrepareNewBatch()
				{
					SteamManager.Workshop.SingleItemRequestPool.<PrepareNewBatch>d__4 <PrepareNewBatch>d__;
					<PrepareNewBatch>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
					<PrepareNewBatch>d__.<>1__state = -1;
					<PrepareNewBatch>d__.<>t__builder.Start<SteamManager.Workshop.SingleItemRequestPool.<PrepareNewBatch>d__4>(ref <PrepareNewBatch>d__);
					return <PrepareNewBatch>d__.<>t__builder.Task;
				}

				// Token: 0x06009E4D RID: 40525 RVA: 0x003EF3AC File Offset: 0x003ED5AC
				[return: Nullable(new byte[]
				{
					1,
					0
				})]
				public static Task<Option<Item>> MakeRequest(ulong id)
				{
					SteamManager.Workshop.SingleItemRequestPool.<MakeRequest>d__5 <MakeRequest>d__;
					<MakeRequest>d__.<>t__builder = AsyncTaskMethodBuilder<Option<Item>>.Create();
					<MakeRequest>d__.id = id;
					<MakeRequest>d__.<>1__state = -1;
					<MakeRequest>d__.<>t__builder.Start<SteamManager.Workshop.SingleItemRequestPool.<MakeRequest>d__5>(ref <MakeRequest>d__);
					return <MakeRequest>d__.<>t__builder.Task;
				}

				// Token: 0x040068DB RID: 26843
				private static readonly object mutex = new object();

				// Token: 0x040068DC RID: 26844
				private static readonly TimeSpan delayAfterNewRequest = TimeSpan.FromSeconds(0.5);

				// Token: 0x040068DD RID: 26845
				private static readonly HashSet<ulong> ids = new HashSet<ulong>();

				// Token: 0x040068DE RID: 26846
				[Nullable(new byte[]
				{
					2,
					1
				})]
				private static Task<ISet<Item>> currentBatch = null;

				// Token: 0x020015ED RID: 5613
				[CompilerGenerated]
				private static class <>O
				{
					// Token: 0x04006A4B RID: 27211
					[Nullable(new byte[]
					{
						0,
						2,
						0
					})]
					public static Func<Task<ISet<Item>>> <0>__PrepareNewBatch;
				}
			}

			// Token: 0x0200158F RID: 5519
			[NullableContext(0)]
			private class CopyIndicator : IDisposable
			{
				// Token: 0x06009E4F RID: 40527 RVA: 0x003EF420 File Offset: 0x003ED620
				[NullableContext(1)]
				public CopyIndicator(string path)
				{
					this.path = path;
					using (FileStream f = File.Create(path, true))
					{
						if (f == null)
						{
							throw new Exception("File.Create returned null");
						}
						f.WriteByte(0);
					}
				}

				// Token: 0x06009E50 RID: 40528 RVA: 0x003EF474 File Offset: 0x003ED674
				public void Dispose()
				{
					try
					{
						File.Delete(this.path, true);
					}
					catch
					{
					}
				}

				// Token: 0x040068DF RID: 26847
				[Nullable(1)]
				private readonly string path;
			}

			// Token: 0x02001590 RID: 5520
			[Nullable(0)]
			private class InstallTaskCounter : IDisposable
			{
				// Token: 0x06009E51 RID: 40529 RVA: 0x003EF4A4 File Offset: 0x003ED6A4
				private InstallTaskCounter(ulong id)
				{
					this.itemId = id;
				}

				// Token: 0x06009E52 RID: 40530 RVA: 0x003EF4B3 File Offset: 0x003ED6B3
				public static bool IsInstalling(Item item)
				{
					return SteamManager.Workshop.InstallTaskCounter.IsInstalling(item.Id);
				}

				// Token: 0x06009E53 RID: 40531 RVA: 0x003EF4C8 File Offset: 0x003ED6C8
				public static bool IsInstalling(ulong itemId)
				{
					object obj = SteamManager.Workshop.InstallTaskCounter.mutex;
					bool result;
					lock (obj)
					{
						result = SteamManager.Workshop.InstallTaskCounter.installers.Any((SteamManager.Workshop.InstallTaskCounter i) => i.itemId == itemId);
					}
					return result;
				}

				// Token: 0x06009E54 RID: 40532 RVA: 0x003EF528 File Offset: 0x003ED728
				private Task Init()
				{
					SteamManager.Workshop.InstallTaskCounter.<Init>d__7 <Init>d__;
					<Init>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
					<Init>d__.<>4__this = this;
					<Init>d__.<>1__state = -1;
					<Init>d__.<>t__builder.Start<SteamManager.Workshop.InstallTaskCounter.<Init>d__7>(ref <Init>d__);
					return <Init>d__.<>t__builder.Task;
				}

				// Token: 0x06009E55 RID: 40533 RVA: 0x003EF56C File Offset: 0x003ED76C
				public static Task<SteamManager.Workshop.InstallTaskCounter> Create(ulong itemId)
				{
					SteamManager.Workshop.InstallTaskCounter.<Create>d__8 <Create>d__;
					<Create>d__.<>t__builder = AsyncTaskMethodBuilder<SteamManager.Workshop.InstallTaskCounter>.Create();
					<Create>d__.itemId = itemId;
					<Create>d__.<>1__state = -1;
					<Create>d__.<>t__builder.Start<SteamManager.Workshop.InstallTaskCounter.<Create>d__8>(ref <Create>d__);
					return <Create>d__.<>t__builder.Task;
				}

				// Token: 0x06009E56 RID: 40534 RVA: 0x003EF5B0 File Offset: 0x003ED7B0
				public void Dispose()
				{
					object obj = SteamManager.Workshop.InstallTaskCounter.mutex;
					lock (obj)
					{
						SteamManager.Workshop.InstallTaskCounter.installers.Remove(this);
					}
				}

				// Token: 0x040068E0 RID: 26848
				private static readonly HashSet<SteamManager.Workshop.InstallTaskCounter> installers = new HashSet<SteamManager.Workshop.InstallTaskCounter>();

				// Token: 0x040068E1 RID: 26849
				private static readonly object mutex = new object();

				// Token: 0x040068E2 RID: 26850
				private const int MaxTasks = 7;

				// Token: 0x040068E3 RID: 26851
				private readonly ulong itemId;
			}

			// Token: 0x02001591 RID: 5521
			[NullableContext(0)]
			public enum ShouldCorrectPaths
			{
				// Token: 0x040068E5 RID: 26853
				Yes,
				// Token: 0x040068E6 RID: 26854
				No
			}
		}

		// Token: 0x020010F5 RID: 4341
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005A22 RID: 23074
			public static Action<Identifier, object> <0>__SetServerListInfo;
		}
	}
}
