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
using Steamworks;
using Steamworks.Data;
using Steamworks.Ugc;

namespace Barotrauma.Steam
{
	// Token: 0x02000363 RID: 867
	internal static class SteamManager
	{
		// Token: 0x06003326 RID: 13094 RVA: 0x00159920 File Offset: 0x00157B20
		private static void InitializeProjectSpecific()
		{
		}

		// Token: 0x17000E49 RID: 3657
		// (get) Token: 0x06003327 RID: 13095 RVA: 0x00159922 File Offset: 0x00157B22
		private static bool IsInitializedProjectSpecific
		{
			get
			{
				return SteamServer.IsValid;
			}
		}

		// Token: 0x06003328 RID: 13096 RVA: 0x0015992C File Offset: 0x00157B2C
		public static bool CreateServer(GameServer server, bool isPublic)
		{
			SteamServerInit options = new SteamServerInit("Barotrauma", "Barotrauma")
			{
				GamePort = (ushort)server.Port,
				QueryPort = (isPublic ? ((ushort)server.QueryPort) : 0),
				Mode = (isPublic ? InitServerMode.Authentication : InitServerMode.NoAuthentication),
				IpAddress = server.ServerSettings.ListenIPAddress
			};
			SteamServer.Init(602960U, options, false);
			if (!SteamServer.IsValid)
			{
				SteamServer.Shutdown();
				DebugConsole.ThrowError("Initializing Steam server failed.", null, null, false, false);
				return false;
			}
			SteamManager.RefreshServerDetails(server);
			SteamServer.LogOnAnonymous();
			return true;
		}

		// Token: 0x06003329 RID: 13097 RVA: 0x001599C8 File Offset: 0x00157BC8
		public static bool RefreshServerDetails(GameServer server)
		{
			if (!SteamManager.IsInitialized || !SteamServer.IsValid)
			{
				return false;
			}
			NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
			string text;
			if (netLobbyScreen == null)
			{
				text = null;
			}
			else
			{
				SubmarineInfo selectedSub = netLobbyScreen.SelectedSub;
				if (selectedSub == null)
				{
					text = null;
				}
				else
				{
					LocalizedString displayName = selectedSub.DisplayName;
					text = ((displayName != null) ? displayName.Value : null);
				}
			}
			SteamServer.MapName = (text ?? "");
			ServerSettings serverSettings = server.ServerSettings;
			Action<Identifier, object> setter;
			if ((setter = SteamManager.<>O.<0>__SetServerListInfo) == null)
			{
				setter = (SteamManager.<>O.<0>__SetServerListInfo = new Action<Identifier, object>(SteamManager.SetServerListInfo));
			}
			serverSettings.UpdateServerListInfo(setter);
			SteamServer.DedicatedServer = true;
			return true;
		}

		// Token: 0x0600332A RID: 13098 RVA: 0x00159A4C File Offset: 0x00157C4C
		private static void SetServerListInfo(Identifier key, object value)
		{
			string stringValue = value as string;
			if (stringValue == null)
			{
				if (value is int)
				{
					int maxPlayers = (int)value;
					if (key == "MaxPlayers")
					{
						SteamServer.MaxPlayers = maxPlayers;
						return;
					}
				}
				else if (value is bool)
				{
					bool hasPassword = (bool)value;
					if (key == "HasPassword")
					{
						SteamServer.Passworded = hasPassword;
						return;
					}
				}
				else
				{
					IEnumerable<ContentPackage> contentPackages = value as IEnumerable<ContentPackage>;
					if (contentPackages != null)
					{
						int index = 0;
						foreach (ContentPackage contentPackage in contentPackages.Take(10))
						{
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 1);
							defaultInterpolatedStringHandler.AppendLiteral("contentpackage");
							defaultInterpolatedStringHandler.AppendFormatted<int>(index);
							SteamServer.SetKey(defaultInterpolatedStringHandler.ToStringAndClear(), new ServerListContentPackageInfo(contentPackage).ToString());
							index++;
						}
						return;
					}
				}
			}
			else
			{
				if (key == "ServerName")
				{
					SteamServer.ServerName = stringValue;
					return;
				}
				string serverMessage = stringValue;
				if (key == "message")
				{
					int maxValueLength = 127;
					int totalMaxLength = 2000;
					int chunkIndex = 0;
					int charIndex = 0;
					while (charIndex < serverMessage.Length && charIndex < totalMaxLength)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(7, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("message");
						defaultInterpolatedStringHandler2.AppendFormatted<int>(chunkIndex);
						SteamServer.SetKey(defaultInterpolatedStringHandler2.ToStringAndClear(), serverMessage.Substring(charIndex, Math.Min(maxValueLength, serverMessage.Length - charIndex)));
						chunkIndex++;
						charIndex += maxValueLength;
					}
					return;
				}
			}
			SteamServer.SetKey(key.Value.ToLowerInvariant(), value.ToString());
		}

		// Token: 0x0600332B RID: 13099 RVA: 0x00159C10 File Offset: 0x00157E10
		public static bool CloseServer()
		{
			if (!SteamManager.IsInitialized || !SteamServer.IsValid)
			{
				return false;
			}
			SteamServer.Shutdown();
			return true;
		}

		// Token: 0x0600332C RID: 13100 RVA: 0x00159C28 File Offset: 0x00157E28
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

		// Token: 0x0600332D RID: 13101 RVA: 0x00159D1C File Offset: 0x00157F1C
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public static Task<Option<AuthTicket>> GetAuthTicketForEosHostAuth()
		{
			SteamManager.<GetAuthTicketForEosHostAuth>d__11 <GetAuthTicketForEosHostAuth>d__;
			<GetAuthTicketForEosHostAuth>d__.<>t__builder = AsyncTaskMethodBuilder<Option<AuthTicket>>.Create();
			<GetAuthTicketForEosHostAuth>d__.<>1__state = -1;
			<GetAuthTicketForEosHostAuth>d__.<>t__builder.Start<SteamManager.<GetAuthTicketForEosHostAuth>d__11>(ref <GetAuthTicketForEosHostAuth>d__);
			return <GetAuthTicketForEosHostAuth>d__.<>t__builder.Task;
		}

		// Token: 0x0600332E RID: 13102 RVA: 0x00159D58 File Offset: 0x00157F58
		[return: Nullable(new byte[]
		{
			1,
			0,
			1
		})]
		public static Task<Option<AuthTicket>> GetAuthTicketForGameAnalyticsConsent()
		{
			SteamManager.<GetAuthTicketForGameAnalyticsConsent>d__14 <GetAuthTicketForGameAnalyticsConsent>d__;
			<GetAuthTicketForGameAnalyticsConsent>d__.<>t__builder = AsyncTaskMethodBuilder<Option<AuthTicket>>.Create();
			<GetAuthTicketForGameAnalyticsConsent>d__.<>1__state = -1;
			<GetAuthTicketForGameAnalyticsConsent>d__.<>t__builder.Start<SteamManager.<GetAuthTicketForGameAnalyticsConsent>d__14>(ref <GetAuthTicketForGameAnalyticsConsent>d__);
			return <GetAuthTicketForGameAnalyticsConsent>d__.<>t__builder.Task;
		}

		// Token: 0x17000E4A RID: 3658
		// (get) Token: 0x0600332F RID: 13103 RVA: 0x00159D93 File Offset: 0x00157F93
		public static bool IsInitialized
		{
			get
			{
				return SteamManager.IsInitializedProjectSpecific;
			}
		}

		// Token: 0x17000E4B RID: 3659
		// (get) Token: 0x06003330 RID: 13104 RVA: 0x00159D9A File Offset: 0x00157F9A
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

		// Token: 0x17000E4C RID: 3660
		// (get) Token: 0x06003331 RID: 13105 RVA: 0x00159DB0 File Offset: 0x00157FB0
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

		// Token: 0x06003332 RID: 13106 RVA: 0x00159E03 File Offset: 0x00158003
		public static void Initialize()
		{
			SteamManager.InitializeProjectSpecific();
		}

		// Token: 0x06003333 RID: 13107 RVA: 0x00159E0A File Offset: 0x0015800A
		public static Option<Barotrauma.Networking.SteamId> GetSteamId()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return Option<Barotrauma.Networking.SteamId>.None();
			}
			return Option<Barotrauma.Networking.SteamId>.Some(new Barotrauma.Networking.SteamId(SteamClient.SteamId));
		}

		// Token: 0x06003334 RID: 13108 RVA: 0x00159E34 File Offset: 0x00158034
		public static Option<Barotrauma.Networking.SteamId> GetOwnerSteamId()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return Option<Barotrauma.Networking.SteamId>.None();
			}
			return Option<Barotrauma.Networking.SteamId>.Some(new Barotrauma.Networking.SteamId(SteamClient.SteamId));
		}

		// Token: 0x06003335 RID: 13109 RVA: 0x00159E5E File Offset: 0x0015805E
		public static bool IsFamilyShared()
		{
			return SteamManager.IsInitialized && SteamClient.IsValid && SteamApps.IsSubscribedFromFamilySharing;
		}

		// Token: 0x06003336 RID: 13110 RVA: 0x00159E75 File Offset: 0x00158075
		public static bool IsFreeWeekend()
		{
			return SteamManager.IsInitialized && SteamClient.IsValid && SteamApps.IsSubscribedFromFreeWeekend;
		}

		// Token: 0x06003337 RID: 13111 RVA: 0x00159E8C File Offset: 0x0015808C
		public static string GetUsername()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return "";
			}
			return SteamClient.Name;
		}

		// Token: 0x06003338 RID: 13112 RVA: 0x00159EA7 File Offset: 0x001580A7
		public static uint GetNumSubscribedItems()
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return 0U;
			}
			return SteamUGC.NumSubscribedItems;
		}

		// Token: 0x06003339 RID: 13113 RVA: 0x00159EBE File Offset: 0x001580BE
		public static bool UnlockAchievement(string achievementIdentifier)
		{
			return SteamManager.UnlockAchievement(achievementIdentifier.ToIdentifier());
		}

		// Token: 0x0600333A RID: 13114 RVA: 0x00159ECC File Offset: 0x001580CC
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

		// Token: 0x0600333B RID: 13115 RVA: 0x00159F91 File Offset: 0x00158191
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

		// Token: 0x0600333C RID: 13116 RVA: 0x00159FB8 File Offset: 0x001581B8
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

		// Token: 0x0600333D RID: 13117 RVA: 0x0015A064 File Offset: 0x00158264
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

		// Token: 0x0600333E RID: 13118 RVA: 0x0015A10D File Offset: 0x0015830D
		public static int GetStatInt(AchievementStat stat)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return 0;
			}
			return SteamUserStats.GetStatInt(stat.ToString().ToLowerInvariant());
		}

		// Token: 0x0600333F RID: 13119 RVA: 0x0015A136 File Offset: 0x00158336
		public static float GetStatFloat(AchievementStat stat)
		{
			if (!SteamManager.IsInitialized || !SteamClient.IsValid)
			{
				return 0f;
			}
			return SteamUserStats.GetStatFloat(stat.ToString().ToLowerInvariant());
		}

		// Token: 0x06003340 RID: 13120 RVA: 0x0015A164 File Offset: 0x00158364
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

		// Token: 0x06003341 RID: 13121 RVA: 0x0015A1D4 File Offset: 0x001583D4
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

		// Token: 0x06003342 RID: 13122 RVA: 0x0015A210 File Offset: 0x00158410
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

		// Token: 0x06003343 RID: 13123 RVA: 0x0015A261 File Offset: 0x00158461
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

		// Token: 0x06003344 RID: 13124 RVA: 0x0015A283 File Offset: 0x00158483
		public static void Update(float deltaTime)
		{
			AchievementManager.Update(deltaTime);
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

		// Token: 0x06003345 RID: 13125 RVA: 0x0015A2AB File Offset: 0x001584AB
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

		// Token: 0x06003346 RID: 13126 RVA: 0x0015A2CD File Offset: 0x001584CD
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

		// Token: 0x06003347 RID: 13127 RVA: 0x0015A2F4 File Offset: 0x001584F4
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

		// Token: 0x06003348 RID: 13128 RVA: 0x0015A368 File Offset: 0x00158568
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

		// Token: 0x06003349 RID: 13129 RVA: 0x0015A39C File Offset: 0x0015859C
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

		// Token: 0x04001962 RID: 6498
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private static Option<AuthTicket> currentSteamHostAuthTicket;

		// Token: 0x04001963 RID: 6499
		[Nullable(1)]
		private const string EosHostAuthIdentity = "BarotraumaRemotePlayerAuth";

		// Token: 0x04001964 RID: 6500
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private static Option<AuthTicket> currentEosHostAuthTicket;

		// Token: 0x04001965 RID: 6501
		[Nullable(1)]
		private const string GameAnalyticsConsentIdentity = "BarotraumaGameAnalyticsConsent";

		// Token: 0x04001966 RID: 6502
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private static Option<AuthTicket> currentGameAnalyticsConsentTicket;

		// Token: 0x04001967 RID: 6503
		public const int STEAMP2P_OWNER_PORT = 30000;

		// Token: 0x04001968 RID: 6504
		public const uint AppID = 602960U;

		// Token: 0x04001969 RID: 6505
		private static readonly Dictionary<string, int> tagCommonness;

		// Token: 0x0400196A RID: 6506
		private static readonly List<string> popularTags;

		// Token: 0x02000BA2 RID: 2978
		[NullableContext(1)]
		[Nullable(0)]
		public static class Workshop
		{
			// Token: 0x06006162 RID: 24930 RVA: 0x0020BF60 File Offset: 0x0020A160
			private static Task<ISet<Item>> GetWorkshopItems(Query query, int? maxPages = null)
			{
				SteamManager.Workshop.<GetWorkshopItems>d__1 <GetWorkshopItems>d__;
				<GetWorkshopItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetWorkshopItems>d__.query = query;
				<GetWorkshopItems>d__.maxPages = maxPages;
				<GetWorkshopItems>d__.<>1__state = -1;
				<GetWorkshopItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetWorkshopItems>d__1>(ref <GetWorkshopItems>d__);
				return <GetWorkshopItems>d__.<>t__builder.Task;
			}

			// Token: 0x06006163 RID: 24931 RVA: 0x0020BFAB File Offset: 0x0020A1AB
			public static ImmutableHashSet<PublishedFileId> GetSubscribedItemIds()
			{
				if (!SteamManager.IsInitialized)
				{
					return ImmutableHashSet<PublishedFileId>.Empty;
				}
				return SteamUGC.GetSubscribedItems().ToImmutableHashSet<PublishedFileId>();
			}

			// Token: 0x06006164 RID: 24932 RVA: 0x0020BFC4 File Offset: 0x0020A1C4
			public static Task<ISet<Item>> GetAllSubscribedItems()
			{
				SteamManager.Workshop.<GetAllSubscribedItems>d__3 <GetAllSubscribedItems>d__;
				<GetAllSubscribedItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetAllSubscribedItems>d__.<>1__state = -1;
				<GetAllSubscribedItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetAllSubscribedItems>d__3>(ref <GetAllSubscribedItems>d__);
				return <GetAllSubscribedItems>d__.<>t__builder.Task;
			}

			// Token: 0x06006165 RID: 24933 RVA: 0x0020C000 File Offset: 0x0020A200
			public static Task<ISet<Item>> GetPopularItems()
			{
				SteamManager.Workshop.<GetPopularItems>d__4 <GetPopularItems>d__;
				<GetPopularItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetPopularItems>d__.<>1__state = -1;
				<GetPopularItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetPopularItems>d__4>(ref <GetPopularItems>d__);
				return <GetPopularItems>d__.<>t__builder.Task;
			}

			// Token: 0x06006166 RID: 24934 RVA: 0x0020C03C File Offset: 0x0020A23C
			public static Task<ISet<Item>> GetPublishedItems()
			{
				SteamManager.Workshop.<GetPublishedItems>d__5 <GetPublishedItems>d__;
				<GetPublishedItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetPublishedItems>d__.<>1__state = -1;
				<GetPublishedItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetPublishedItems>d__5>(ref <GetPublishedItems>d__);
				return <GetPublishedItems>d__.<>t__builder.Task;
			}

			// Token: 0x06006167 RID: 24935 RVA: 0x0020C077 File Offset: 0x0020A277
			[return: Nullable(new byte[]
			{
				1,
				0
			})]
			public static Task<Option<Item>> GetItem(ulong itemId)
			{
				return SteamManager.Workshop.SingleItemRequestPool.MakeRequest(itemId);
			}

			// Token: 0x06006168 RID: 24936 RVA: 0x0020C080 File Offset: 0x0020A280
			[return: Nullable(new byte[]
			{
				1,
				0
			})]
			public static Task<Option<Item>> GetItemAsap(ulong itemId, bool withLongDescription = false)
			{
				SteamManager.Workshop.<GetItemAsap>d__8 <GetItemAsap>d__;
				<GetItemAsap>d__.<>t__builder = AsyncTaskMethodBuilder<Option<Item>>.Create();
				<GetItemAsap>d__.itemId = itemId;
				<GetItemAsap>d__.withLongDescription = withLongDescription;
				<GetItemAsap>d__.<>1__state = -1;
				<GetItemAsap>d__.<>t__builder.Start<SteamManager.Workshop.<GetItemAsap>d__8>(ref <GetItemAsap>d__);
				return <GetItemAsap>d__.<>t__builder.Task;
			}

			// Token: 0x06006169 RID: 24937 RVA: 0x0020C0CC File Offset: 0x0020A2CC
			public static Task ForceRedownload(ulong itemId)
			{
				SteamManager.Workshop.<ForceRedownload>d__9 <ForceRedownload>d__;
				<ForceRedownload>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<ForceRedownload>d__.itemId = itemId;
				<ForceRedownload>d__.<>1__state = -1;
				<ForceRedownload>d__.<>t__builder.Start<SteamManager.Workshop.<ForceRedownload>d__9>(ref <ForceRedownload>d__);
				return <ForceRedownload>d__.<>t__builder.Task;
			}

			// Token: 0x0600616A RID: 24938 RVA: 0x0020C110 File Offset: 0x0020A310
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

			// Token: 0x0600616B RID: 24939 RVA: 0x0020C148 File Offset: 0x0020A348
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

			// Token: 0x0600616C RID: 24940 RVA: 0x0020C1E8 File Offset: 0x0020A3E8
			public static Task ForceRedownload(Item item, [Nullable(2)] CancellationTokenSource cancellationTokenSrc = null)
			{
				SteamManager.Workshop.<ForceRedownload>d__12 <ForceRedownload>d__;
				<ForceRedownload>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<ForceRedownload>d__.item = item;
				<ForceRedownload>d__.cancellationTokenSrc = cancellationTokenSrc;
				<ForceRedownload>d__.<>1__state = -1;
				<ForceRedownload>d__.<>t__builder.Start<SteamManager.Workshop.<ForceRedownload>d__12>(ref <ForceRedownload>d__);
				return <ForceRedownload>d__.<>t__builder.Task;
			}

			// Token: 0x0600616D RID: 24941 RVA: 0x0020C234 File Offset: 0x0020A434
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

			// Token: 0x0600616E RID: 24942 RVA: 0x0020C284 File Offset: 0x0020A484
			public static bool CanBeInstalled(ulong itemId)
			{
				Item item = new Item(itemId);
				return SteamManager.Workshop.CanBeInstalled(item);
			}

			// Token: 0x0600616F RID: 24943 RVA: 0x0020C2A4 File Offset: 0x0020A4A4
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

			// Token: 0x06006170 RID: 24944 RVA: 0x0020C2F8 File Offset: 0x0020A4F8
			public static Task DownloadModThenEnqueueInstall(Item item)
			{
				SteamManager.Workshop.<DownloadModThenEnqueueInstall>d__18 <DownloadModThenEnqueueInstall>d__;
				<DownloadModThenEnqueueInstall>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<DownloadModThenEnqueueInstall>d__.item = item;
				<DownloadModThenEnqueueInstall>d__.<>1__state = -1;
				<DownloadModThenEnqueueInstall>d__.<>t__builder.Start<SteamManager.Workshop.<DownloadModThenEnqueueInstall>d__18>(ref <DownloadModThenEnqueueInstall>d__);
				return <DownloadModThenEnqueueInstall>d__.<>t__builder.Task;
			}

			// Token: 0x06006171 RID: 24945 RVA: 0x0020C33C File Offset: 0x0020A53C
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

			// Token: 0x06006172 RID: 24946 RVA: 0x0020C3C0 File Offset: 0x0020A5C0
			public static ISet<ulong> GetInstalledItems()
			{
				return (from id in (from p in ContentPackageManager.WorkshopPackages
				select p.UgcId).NotNone<ContentPackageId>().OfType<SteamWorkshopId>()
				select id.Value).ToHashSet<ulong>();
			}

			// Token: 0x06006173 RID: 24947 RVA: 0x0020C42C File Offset: 0x0020A62C
			public static Task<ISet<Item>> GetPublishedAndSubscribedItems()
			{
				SteamManager.Workshop.<GetPublishedAndSubscribedItems>d__21 <GetPublishedAndSubscribedItems>d__;
				<GetPublishedAndSubscribedItems>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
				<GetPublishedAndSubscribedItems>d__.<>1__state = -1;
				<GetPublishedAndSubscribedItems>d__.<>t__builder.Start<SteamManager.Workshop.<GetPublishedAndSubscribedItems>d__21>(ref <GetPublishedAndSubscribedItems>d__);
				return <GetPublishedAndSubscribedItems>d__.<>t__builder.Task;
			}

			// Token: 0x06006174 RID: 24948 RVA: 0x0020C468 File Offset: 0x0020A668
			public static void DeleteUnsubscribedMods([Nullable(new byte[]
			{
				2,
				1,
				1
			})] Action<ContentPackage[]> callback = null)
			{
			}

			// Token: 0x06006175 RID: 24949 RVA: 0x0020C482 File Offset: 0x0020A682
			public static bool IsInstallingToPath(string path)
			{
				return File.Exists(Path.Combine(new string[]
				{
					Path.GetDirectoryName(path),
					".copying"
				}));
			}

			// Token: 0x06006176 RID: 24950 RVA: 0x0020C4A5 File Offset: 0x0020A6A5
			public static bool IsInstalling(Item item)
			{
				return SteamManager.Workshop.InstallTaskCounter.IsInstalling(item);
			}

			// Token: 0x06006177 RID: 24951 RVA: 0x0020C4B0 File Offset: 0x0020A6B0
			private static Task InstallMod(ulong id)
			{
				SteamManager.Workshop.<InstallMod>d__25 <InstallMod>d__;
				<InstallMod>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<InstallMod>d__.id = id;
				<InstallMod>d__.<>1__state = -1;
				<InstallMod>d__.<>t__builder.Start<SteamManager.Workshop.<InstallMod>d__25>(ref <InstallMod>d__);
				return <InstallMod>d__.<>t__builder.Task;
			}

			// Token: 0x06006178 RID: 24952 RVA: 0x0020C4F4 File Offset: 0x0020A6F4
			private static Task CorrectPaths(string fileListDir, string modName, XElement element)
			{
				SteamManager.Workshop.<CorrectPaths>d__26 <CorrectPaths>d__;
				<CorrectPaths>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<CorrectPaths>d__.fileListDir = fileListDir;
				<CorrectPaths>d__.modName = modName;
				<CorrectPaths>d__.element = element;
				<CorrectPaths>d__.<>1__state = -1;
				<CorrectPaths>d__.<>t__builder.Start<SteamManager.Workshop.<CorrectPaths>d__26>(ref <CorrectPaths>d__);
				return <CorrectPaths>d__.<>t__builder.Task;
			}

			// Token: 0x06006179 RID: 24953 RVA: 0x0020C548 File Offset: 0x0020A748
			private static Task CopyFile(string fileListDir, string modName, string from, string to, SteamManager.Workshop.ShouldCorrectPaths shouldCorrectPaths)
			{
				SteamManager.Workshop.<CopyFile>d__27 <CopyFile>d__;
				<CopyFile>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<CopyFile>d__.fileListDir = fileListDir;
				<CopyFile>d__.modName = modName;
				<CopyFile>d__.from = from;
				<CopyFile>d__.to = to;
				<CopyFile>d__.shouldCorrectPaths = shouldCorrectPaths;
				<CopyFile>d__.<>1__state = -1;
				<CopyFile>d__.<>t__builder.Start<SteamManager.Workshop.<CopyFile>d__27>(ref <CopyFile>d__);
				return <CopyFile>d__.<>t__builder.Task;
			}

			// Token: 0x0600617A RID: 24954 RVA: 0x0020C5AC File Offset: 0x0020A7AC
			public static Task CopyDirectory(string fileListDir, string modName, string from, string to, SteamManager.Workshop.ShouldCorrectPaths shouldCorrectPaths)
			{
				SteamManager.Workshop.<CopyDirectory>d__29 <CopyDirectory>d__;
				<CopyDirectory>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
				<CopyDirectory>d__.fileListDir = fileListDir;
				<CopyDirectory>d__.modName = modName;
				<CopyDirectory>d__.from = from;
				<CopyDirectory>d__.to = to;
				<CopyDirectory>d__.shouldCorrectPaths = shouldCorrectPaths;
				<CopyDirectory>d__.<>1__state = -1;
				<CopyDirectory>d__.<>t__builder.Start<SteamManager.Workshop.<CopyDirectory>d__29>(ref <CopyDirectory>d__);
				return <CopyDirectory>d__.<>t__builder.Task;
			}

			// Token: 0x0600617B RID: 24955 RVA: 0x0020C610 File Offset: 0x0020A810
			[CompilerGenerated]
			internal static string <CopyDirectory>g__convertFromTo|29_0(string from, ref SteamManager.Workshop.<>c__DisplayClass29_0 A_1)
			{
				return Path.Combine(new string[]
				{
					A_1.to,
					Path.GetFileName(from)
				});
			}

			// Token: 0x02000ECB RID: 3787
			[NullableContext(0)]
			private struct ItemEqualityComparer : IEqualityComparer<Item>
			{
				// Token: 0x06006B13 RID: 27411 RVA: 0x00227A0D File Offset: 0x00225C0D
				public bool Equals(Item x, Item y)
				{
					return x.Id == y.Id;
				}

				// Token: 0x06006B14 RID: 27412 RVA: 0x00227A22 File Offset: 0x00225C22
				public int GetHashCode(Item obj)
				{
					return (int)obj.Id.Value;
				}

				// Token: 0x04004376 RID: 17270
				public static readonly SteamManager.Workshop.ItemEqualityComparer Instance;
			}

			// Token: 0x02000ECC RID: 3788
			[Nullable(0)]
			private static class SingleItemRequestPool
			{
				// Token: 0x06006B16 RID: 27414 RVA: 0x00227A34 File Offset: 0x00225C34
				private static Task<ISet<Item>> PrepareNewBatch()
				{
					SteamManager.Workshop.SingleItemRequestPool.<PrepareNewBatch>d__4 <PrepareNewBatch>d__;
					<PrepareNewBatch>d__.<>t__builder = AsyncTaskMethodBuilder<ISet<Item>>.Create();
					<PrepareNewBatch>d__.<>1__state = -1;
					<PrepareNewBatch>d__.<>t__builder.Start<SteamManager.Workshop.SingleItemRequestPool.<PrepareNewBatch>d__4>(ref <PrepareNewBatch>d__);
					return <PrepareNewBatch>d__.<>t__builder.Task;
				}

				// Token: 0x06006B17 RID: 27415 RVA: 0x00227A70 File Offset: 0x00225C70
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

				// Token: 0x04004377 RID: 17271
				private static readonly object mutex = new object();

				// Token: 0x04004378 RID: 17272
				private static readonly TimeSpan delayAfterNewRequest = TimeSpan.FromSeconds(0.5);

				// Token: 0x04004379 RID: 17273
				private static readonly HashSet<ulong> ids = new HashSet<ulong>();

				// Token: 0x0400437A RID: 17274
				[Nullable(new byte[]
				{
					2,
					1
				})]
				private static Task<ISet<Item>> currentBatch = null;

				// Token: 0x02000F03 RID: 3843
				[CompilerGenerated]
				private static class <>O
				{
					// Token: 0x04004451 RID: 17489
					[Nullable(new byte[]
					{
						0,
						2,
						0
					})]
					public static Func<Task<ISet<Item>>> <0>__PrepareNewBatch;
				}
			}

			// Token: 0x02000ECD RID: 3789
			[NullableContext(0)]
			private class CopyIndicator : IDisposable
			{
				// Token: 0x06006B19 RID: 27417 RVA: 0x00227AE4 File Offset: 0x00225CE4
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

				// Token: 0x06006B1A RID: 27418 RVA: 0x00227B38 File Offset: 0x00225D38
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

				// Token: 0x0400437B RID: 17275
				[Nullable(1)]
				private readonly string path;
			}

			// Token: 0x02000ECE RID: 3790
			[Nullable(0)]
			private class InstallTaskCounter : IDisposable
			{
				// Token: 0x06006B1B RID: 27419 RVA: 0x00227B68 File Offset: 0x00225D68
				private InstallTaskCounter(ulong id)
				{
					this.itemId = id;
				}

				// Token: 0x06006B1C RID: 27420 RVA: 0x00227B77 File Offset: 0x00225D77
				public static bool IsInstalling(Item item)
				{
					return SteamManager.Workshop.InstallTaskCounter.IsInstalling(item.Id);
				}

				// Token: 0x06006B1D RID: 27421 RVA: 0x00227B8C File Offset: 0x00225D8C
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

				// Token: 0x06006B1E RID: 27422 RVA: 0x00227BEC File Offset: 0x00225DEC
				private Task Init()
				{
					SteamManager.Workshop.InstallTaskCounter.<Init>d__7 <Init>d__;
					<Init>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
					<Init>d__.<>4__this = this;
					<Init>d__.<>1__state = -1;
					<Init>d__.<>t__builder.Start<SteamManager.Workshop.InstallTaskCounter.<Init>d__7>(ref <Init>d__);
					return <Init>d__.<>t__builder.Task;
				}

				// Token: 0x06006B1F RID: 27423 RVA: 0x00227C30 File Offset: 0x00225E30
				public static Task<SteamManager.Workshop.InstallTaskCounter> Create(ulong itemId)
				{
					SteamManager.Workshop.InstallTaskCounter.<Create>d__8 <Create>d__;
					<Create>d__.<>t__builder = AsyncTaskMethodBuilder<SteamManager.Workshop.InstallTaskCounter>.Create();
					<Create>d__.itemId = itemId;
					<Create>d__.<>1__state = -1;
					<Create>d__.<>t__builder.Start<SteamManager.Workshop.InstallTaskCounter.<Create>d__8>(ref <Create>d__);
					return <Create>d__.<>t__builder.Task;
				}

				// Token: 0x06006B20 RID: 27424 RVA: 0x00227C74 File Offset: 0x00225E74
				public void Dispose()
				{
					object obj = SteamManager.Workshop.InstallTaskCounter.mutex;
					lock (obj)
					{
						SteamManager.Workshop.InstallTaskCounter.installers.Remove(this);
					}
				}

				// Token: 0x0400437C RID: 17276
				private static readonly HashSet<SteamManager.Workshop.InstallTaskCounter> installers = new HashSet<SteamManager.Workshop.InstallTaskCounter>();

				// Token: 0x0400437D RID: 17277
				private static readonly object mutex = new object();

				// Token: 0x0400437E RID: 17278
				private const int MaxTasks = 7;

				// Token: 0x0400437F RID: 17279
				private readonly ulong itemId;
			}

			// Token: 0x02000ECF RID: 3791
			[NullableContext(0)]
			public enum ShouldCorrectPaths
			{
				// Token: 0x04004381 RID: 17281
				Yes,
				// Token: 0x04004382 RID: 17282
				No
			}
		}

		// Token: 0x02000BA3 RID: 2979
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040039EC RID: 14828
			public static Action<Identifier, object> <0>__SetServerListInfo;
		}
	}
}
