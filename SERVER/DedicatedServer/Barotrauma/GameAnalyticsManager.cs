using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Barotrauma.IO;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using RestSharp;

namespace Barotrauma
{
	// Token: 0x020001CF RID: 463
	[NullableContext(1)]
	[Nullable(0)]
	internal static class GameAnalyticsManager
	{
		// Token: 0x170009CD RID: 2509
		// (get) Token: 0x06002238 RID: 8760 RVA: 0x000E60E4 File Offset: 0x000E42E4
		// (set) Token: 0x06002239 RID: 8761 RVA: 0x000E60EB File Offset: 0x000E42EB
		public static GameAnalyticsManager.Consent UserConsented { get; private set; } = GameAnalyticsManager.Consent.Unknown;

		// Token: 0x170009CE RID: 2510
		// (get) Token: 0x0600223A RID: 8762 RVA: 0x000E60F3 File Offset: 0x000E42F3
		public static bool SendUserStatistics
		{
			get
			{
				return GameAnalyticsManager.UserConsented == GameAnalyticsManager.Consent.Yes && GameAnalyticsManager.loadedImplementation != null;
			}
		}

		// Token: 0x170009CF RID: 2511
		// (get) Token: 0x0600223B RID: 8763 RVA: 0x000E6107 File Offset: 0x000E4307
		private static bool ConsentTextAvailable
		{
			get
			{
				return TextManager.ContainsTag("statisticsconsentheader") && TextManager.ContainsTag("statisticsconsenttext");
			}
		}

		// Token: 0x0600223C RID: 8764 RVA: 0x000E6124 File Offset: 0x000E4324
		private static Task<GameAnalyticsManager.AuthTicket> GetAuthTicket()
		{
			GameAnalyticsManager.<GetAuthTicket>d__14 <GetAuthTicket>d__;
			<GetAuthTicket>d__.<>t__builder = AsyncTaskMethodBuilder<GameAnalyticsManager.AuthTicket>.Create();
			<GetAuthTicket>d__.<>1__state = -1;
			<GetAuthTicket>d__.<>t__builder.Start<GameAnalyticsManager.<GetAuthTicket>d__14>(ref <GetAuthTicket>d__);
			return <GetAuthTicket>d__.<>t__builder.Task;
		}

		// Token: 0x0600223D RID: 8765 RVA: 0x000E6160 File Offset: 0x000E4360
		private static Task<GameAnalyticsManager.AuthTicket> GetSteamAuthTicket()
		{
			GameAnalyticsManager.<GetSteamAuthTicket>d__15 <GetSteamAuthTicket>d__;
			<GetSteamAuthTicket>d__.<>t__builder = AsyncTaskMethodBuilder<GameAnalyticsManager.AuthTicket>.Create();
			<GetSteamAuthTicket>d__.<>1__state = -1;
			<GetSteamAuthTicket>d__.<>t__builder.Start<GameAnalyticsManager.<GetSteamAuthTicket>d__15>(ref <GetSteamAuthTicket>d__);
			return <GetSteamAuthTicket>d__.<>t__builder.Task;
		}

		// Token: 0x0600223E RID: 8766 RVA: 0x000E619C File Offset: 0x000E439C
		private static Task<GameAnalyticsManager.AuthTicket> GetEOSAuthTicket()
		{
			GameAnalyticsManager.<GetEOSAuthTicket>d__16 <GetEOSAuthTicket>d__;
			<GetEOSAuthTicket>d__.<>t__builder = AsyncTaskMethodBuilder<GameAnalyticsManager.AuthTicket>.Create();
			<GetEOSAuthTicket>d__.<>1__state = -1;
			<GetEOSAuthTicket>d__.<>t__builder.Start<GameAnalyticsManager.<GetEOSAuthTicket>d__16>(ref <GetEOSAuthTicket>d__);
			return <GetEOSAuthTicket>d__.<>t__builder.Task;
		}

		// Token: 0x0600223F RID: 8767 RVA: 0x000E61D7 File Offset: 0x000E43D7
		[NullableContext(2)]
		public static void SetConsent(GameAnalyticsManager.Consent consent, Action onAnswerSent = null)
		{
			if (consent == GameAnalyticsManager.Consent.Yes)
			{
				throw new Exception("Cannot call SetConsent with value Consent.Yes, must only be set to this value via consent prompt");
			}
			GameAnalyticsManager.SetConsentInternal(consent, onAnswerSent);
		}

		// Token: 0x06002240 RID: 8768 RVA: 0x000E61F0 File Offset: 0x000E43F0
		[NullableContext(2)]
		private static void SetConsentInternal(GameAnalyticsManager.Consent consent, Action onAnswerSent)
		{
			if (GameAnalyticsManager.UserConsented == consent)
			{
				Action onAnswerSent2 = onAnswerSent;
				if (onAnswerSent2 == null)
				{
					return;
				}
				onAnswerSent2();
				return;
			}
			else
			{
				GameAnalyticsManager.Consent consent2 = consent;
				if (consent != GameAnalyticsManager.Consent.No && consent != GameAnalyticsManager.Consent.Yes)
				{
					GameAnalyticsManager.UserConsented = consent;
					GameAnalyticsManager.ShutDown();
					return;
				}
				if (consent == GameAnalyticsManager.Consent.No)
				{
					GameAnalyticsManager.UserConsented = consent;
					GameAnalyticsManager.ShutDown();
				}
				TaskPool.Add("GameAnalyticsConsent.SendAnswerToRemoteDatabase", GameAnalyticsManager.SendAnswerToRemoteDatabase(consent), delegate(Task t)
				{
					Action onAnswerSent3 = onAnswerSent;
					if (onAnswerSent3 != null)
					{
						onAnswerSent3();
					}
					bool success;
					if (!t.TryGetResult(out success) || !success)
					{
						return;
					}
					GameAnalyticsManager.UserConsented = consent;
					if (consent == GameAnalyticsManager.Consent.Yes)
					{
						GameAnalyticsManager.Init();
					}
				});
				return;
			}
		}

		// Token: 0x06002241 RID: 8769 RVA: 0x000E6298 File Offset: 0x000E4498
		private static Task<bool> SendAnswerToRemoteDatabase(GameAnalyticsManager.Consent consent)
		{
			GameAnalyticsManager.<SendAnswerToRemoteDatabase>d__19 <SendAnswerToRemoteDatabase>d__;
			<SendAnswerToRemoteDatabase>d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
			<SendAnswerToRemoteDatabase>d__.consent = consent;
			<SendAnswerToRemoteDatabase>d__.<>1__state = -1;
			<SendAnswerToRemoteDatabase>d__.<>t__builder.Start<GameAnalyticsManager.<SendAnswerToRemoteDatabase>d__19>(ref <SendAnswerToRemoteDatabase>d__);
			return <SendAnswerToRemoteDatabase>d__.<>t__builder.Task;
		}

		// Token: 0x06002242 RID: 8770 RVA: 0x000E62DB File Offset: 0x000E44DB
		public static void ResetConsent()
		{
			TaskPool.Add("GameAnalyticsConsent.ResetConsentInternal", GameAnalyticsManager.SendAnswerToRemoteDatabase(GameAnalyticsManager.Consent.Ask), delegate(Task t)
			{
				bool success;
				if (!t.TryGetResult(out success) || !success)
				{
					return;
				}
				DebugConsole.NewMessage("Reset GameAnalytics consent.", null, false);
			});
		}

		// Token: 0x06002243 RID: 8771 RVA: 0x000E6310 File Offset: 0x000E4510
		public static void InitIfConsented()
		{
			if (!GameAnalyticsManager.ConsentTextAvailable)
			{
				GameAnalyticsManager.SetConsent(GameAnalyticsManager.Consent.Unknown, null);
				return;
			}
			if (!SteamManager.IsInitialized && EosInterface.IdQueries.GetLoggedInPuids().Length <= 0)
			{
				DebugConsole.AddWarning("Error in GameAnalyticsManager.GetConsent: Could not get a Steam or EOS authentication ticket (not connected to Steam or EOS).", null);
				GameAnalyticsManager.SetConsent(GameAnalyticsManager.Consent.Error, null);
				return;
			}
			TaskPool.Add("GameAnalyticsConsent.RequestAnswerFromRemoteDatabase", GameAnalyticsManager.RequestAnswerFromRemoteDatabase(), delegate(Task t)
			{
				GameAnalyticsManager.Consent consent;
				if (!t.TryGetResult(out consent))
				{
					return;
				}
				GameAnalyticsManager.SetConsentInternal(consent, null);
			});
		}

		// Token: 0x06002244 RID: 8772 RVA: 0x000E6388 File Offset: 0x000E4588
		private static Task<GameAnalyticsManager.Consent> RequestAnswerFromRemoteDatabase()
		{
			GameAnalyticsManager.<RequestAnswerFromRemoteDatabase>d__23 <RequestAnswerFromRemoteDatabase>d__;
			<RequestAnswerFromRemoteDatabase>d__.<>t__builder = AsyncTaskMethodBuilder<GameAnalyticsManager.Consent>.Create();
			<RequestAnswerFromRemoteDatabase>d__.<>1__state = -1;
			<RequestAnswerFromRemoteDatabase>d__.<>t__builder.Start<GameAnalyticsManager.<RequestAnswerFromRemoteDatabase>d__23>(ref <RequestAnswerFromRemoteDatabase>d__);
			return <RequestAnswerFromRemoteDatabase>d__.<>t__builder.Task;
		}

		// Token: 0x06002245 RID: 8773 RVA: 0x000E63C4 File Offset: 0x000E45C4
		private static bool CheckResponse(IRestResponse response)
		{
			if (response.ErrorException != null)
			{
				DebugConsole.ThrowErrorLocalized(TextManager.GetWithVariable("MasterServerErrorException", "[error]", response.ErrorException.ToString(), FormatCapitals.No), null, null, false, false);
				return false;
			}
			if (response.StatusCode == HttpStatusCode.OK)
			{
				return true;
			}
			HttpStatusCode statusCode = response.StatusCode;
			if (statusCode != HttpStatusCode.NotFound)
			{
				if (statusCode != HttpStatusCode.ServiceUnavailable)
				{
					DebugConsole.ThrowErrorLocalized(TextManager.GetWithVariables("MasterServerErrorDefault", new ValueTuple<string, string>[]
					{
						new ValueTuple<string, string>("[statuscode]", response.StatusCode.ToString()),
						new ValueTuple<string, string>("[statusdescription]", response.StatusDescription)
					}), null, null, false, false);
				}
				else
				{
					DebugConsole.ThrowErrorLocalized(TextManager.Get("MasterServerErrorUnavailable"), null, null, false, false);
				}
			}
			else
			{
				DebugConsole.ThrowErrorLocalized(TextManager.GetWithVariable("MasterServerError404", "[masterserverurl]", "https://barotraumagame.com/baromaster/", FormatCapitals.No), null, null, false, false);
			}
			return false;
		}

		// Token: 0x06002246 RID: 8774 RVA: 0x000E64BC File Offset: 0x000E46BC
		private static void ValidateEventID(string eventID)
		{
		}

		// Token: 0x06002247 RID: 8775 RVA: 0x000E64BE File Offset: 0x000E46BE
		public static bool ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize sampleSize = GameAnalyticsManager.DataSampleSize.Small)
		{
			return Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < GameAnalyticsManager.dataSampleSizes[sampleSize];
		}

		// Token: 0x06002248 RID: 8776 RVA: 0x000E64DD File Offset: 0x000E46DD
		public static void AddErrorEvent(GameAnalyticsManager.ErrorSeverity errorSeverity, string message)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.AddErrorEvent(errorSeverity, message);
		}

		// Token: 0x06002249 RID: 8777 RVA: 0x000E64F8 File Offset: 0x000E46F8
		public static void AddErrorEventOnce(string identifier, GameAnalyticsManager.ErrorSeverity errorSeverity, string message)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			if (GameAnalyticsManager.sentEventIdentifiers.Contains(identifier))
			{
				return;
			}
			if (ContentPackageManager.ModsEnabled)
			{
				message = "[MODDED] " + message;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation != null)
			{
				implementation.AddErrorEvent(errorSeverity, message);
			}
			GameAnalyticsManager.sentEventIdentifiers.Add(identifier);
		}

		// Token: 0x0600224A RID: 8778 RVA: 0x000E654D File Offset: 0x000E474D
		public static void AddDesignEvent(string eventID)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.ValidateEventID(eventID);
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.AddDesignEvent(eventID, null);
		}

		// Token: 0x0600224B RID: 8779 RVA: 0x000E656E File Offset: 0x000E476E
		public static void AddDesignEvent(string eventID, double value)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.ValidateEventID(eventID);
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.AddDesignEvent(eventID, value);
		}

		// Token: 0x0600224C RID: 8780 RVA: 0x000E658F File Offset: 0x000E478F
		public static void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus progressionStatus, string progression01)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.AddProgressionEvent(progressionStatus, progression01);
		}

		// Token: 0x0600224D RID: 8781 RVA: 0x000E65AA File Offset: 0x000E47AA
		public static void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus progressionStatus, string progression01, double score)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.AddProgressionEvent(progressionStatus, progression01, score);
		}

		// Token: 0x0600224E RID: 8782 RVA: 0x000E65C6 File Offset: 0x000E47C6
		public static void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus progressionStatus, string progression01, string progression02)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.AddProgressionEvent(progressionStatus, progression01, progression02);
		}

		// Token: 0x0600224F RID: 8783 RVA: 0x000E65E2 File Offset: 0x000E47E2
		public static void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus progressionStatus, string progression01, string progression02, string progression03)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.AddProgressionEvent(progressionStatus, progression01, progression02, progression03);
		}

		// Token: 0x06002250 RID: 8784 RVA: 0x000E65FF File Offset: 0x000E47FF
		public static void SetCustomDimension01(GameAnalyticsManager.CustomDimensions01 dimension)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.SetCustomDimension01(dimension.ToString());
		}

		// Token: 0x06002251 RID: 8785 RVA: 0x000E6625 File Offset: 0x000E4825
		public static void SetCustomDimension03(GameAnalyticsManager.CustomDimensions03 dimension)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.SetCustomDimension03(dimension.ToString());
		}

		// Token: 0x06002252 RID: 8786 RVA: 0x000E664C File Offset: 0x000E484C
		public static void SetCurrentLevel(LevelData levelData)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.CustomDimensions02 customDimension = GameAnalyticsManager.CustomDimensions02.None;
			if (levelData != null)
			{
				float levelDifficulty = levelData.Difficulty;
				customDimension = (GameAnalyticsManager.CustomDimensions02)MathHelper.Clamp((int)(levelDifficulty / 10f) + 1, 0, Enum.GetValues(typeof(GameAnalyticsManager.CustomDimensions02)).Length - 1);
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.SetCustomDimension02(customDimension.ToString());
		}

		// Token: 0x06002253 RID: 8787 RVA: 0x000E66B0 File Offset: 0x000E48B0
		public static void AddMoneyGainedEvent(int amount, GameAnalyticsManager.MoneySource moneySource, string eventId)
		{
			GameAnalyticsManager.AddResourceEvent(GameAnalyticsManager.ResourceFlowType.Source, GameAnalyticsManager.ResourceCurrency.Money, (float)amount, moneySource.ToString(), eventId);
		}

		// Token: 0x06002254 RID: 8788 RVA: 0x000E66C9 File Offset: 0x000E48C9
		public static void AddMoneySpentEvent(int amount, GameAnalyticsManager.MoneySink moneySink, string eventId)
		{
			GameAnalyticsManager.AddResourceEvent(GameAnalyticsManager.ResourceFlowType.Sink, GameAnalyticsManager.ResourceCurrency.Money, (float)amount, moneySink.ToString(), eventId);
		}

		// Token: 0x06002255 RID: 8789 RVA: 0x000E66E2 File Offset: 0x000E48E2
		private static void AddResourceEvent(GameAnalyticsManager.ResourceFlowType flowType, GameAnalyticsManager.ResourceCurrency currency, float amount, string eventType, string eventId)
		{
			if (!GameAnalyticsManager.SendUserStatistics)
			{
				return;
			}
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation == null)
			{
				return;
			}
			implementation.AddResourceEvent(flowType, currency.ToString(), amount, eventType, eventId);
		}

		// Token: 0x06002256 RID: 8790 RVA: 0x000E6710 File Offset: 0x000E4910
		private static void Init()
		{
			GameAnalyticsManager.ShutDown();
			try
			{
				GameAnalyticsManager.loadedImplementation = new GameAnalyticsManager.Implementation();
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Initializing GameAnalytics failed. Disabling user statistics...", e, null, false, false);
				GameAnalyticsManager.SetConsent(GameAnalyticsManager.Consent.Error, null);
				return;
			}
			string exePath = Assembly.GetEntryAssembly().Location;
			string exeName = string.Empty;
			exeName = "s";
			Md5Hash exeHash = null;
			try
			{
				exeHash = Md5Hash.CalculateForFile(exePath, Md5Hash.StringHashOptions.BytePerfect);
			}
			catch (Exception e2)
			{
				DebugConsole.ThrowError("Error while calculating MD5 hash for the executable \"" + exePath + "\"", e2, null, false, false);
			}
			try
			{
				string buildConfiguration = "Release";
				GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
				if (implementation != null)
				{
					implementation.ConfigureBuild(string.Concat(new string[]
					{
						GameMain.Version.ToString(),
						exeName,
						":",
						AssemblyInfo.GitRevision,
						":",
						buildConfiguration
					}));
				}
				GameAnalyticsManager.Implementation implementation2 = GameAnalyticsManager.loadedImplementation;
				if (implementation2 != null)
				{
					implementation2.ConfigureAvailableCustomDimensions01(Enum.GetValues(typeof(GameAnalyticsManager.CustomDimensions01)).Cast<GameAnalyticsManager.CustomDimensions01>().ToArray<GameAnalyticsManager.CustomDimensions01>());
				}
				GameAnalyticsManager.Implementation implementation3 = GameAnalyticsManager.loadedImplementation;
				if (implementation3 != null)
				{
					implementation3.ConfigureAvailableCustomDimensions02(Enum.GetValues(typeof(GameAnalyticsManager.CustomDimensions02)).Cast<GameAnalyticsManager.CustomDimensions02>().ToArray<GameAnalyticsManager.CustomDimensions02>());
				}
				GameAnalyticsManager.Implementation implementation4 = GameAnalyticsManager.loadedImplementation;
				if (implementation4 != null)
				{
					implementation4.ConfigureAvailableCustomDimensions03(Enum.GetValues(typeof(GameAnalyticsManager.CustomDimensions03)).Cast<GameAnalyticsManager.CustomDimensions03>().ToArray<GameAnalyticsManager.CustomDimensions03>());
				}
				GameAnalyticsManager.Implementation implementation5 = GameAnalyticsManager.loadedImplementation;
				if (implementation5 != null)
				{
					implementation5.ConfigureAvailableResourceCurrencies(Enum.GetValues(typeof(GameAnalyticsManager.ResourceCurrency)).Cast<GameAnalyticsManager.ResourceCurrency>().ToArray<GameAnalyticsManager.ResourceCurrency>());
				}
				GameAnalyticsManager.Implementation implementation6 = GameAnalyticsManager.loadedImplementation;
				if (implementation6 != null)
				{
					implementation6.ConfigureAvailableResourceItemTypes((from GameAnalyticsManager.MoneySink s in Enum.GetValues(typeof(GameAnalyticsManager.MoneySink))
					select s.ToString()).Union(from GameAnalyticsManager.MoneySource s in Enum.GetValues(typeof(GameAnalyticsManager.MoneySource))
					select s.ToString()).ToArray<string>());
				}
				GameAnalyticsManager.Implementation implementation7 = GameAnalyticsManager.loadedImplementation;
				if (implementation7 != null)
				{
					implementation7.AddDesignEvent(string.Concat(new string[]
					{
						"Executable:",
						GameMain.Version.ToString(),
						exeName,
						":",
						((exeHash != null) ? exeHash.ShortRepresentation : null) ?? "Unknown",
						":",
						AssemblyInfo.GitRevision,
						":",
						buildConfiguration
					}), null);
				}
				GameAnalyticsManager.SetCustomDimension01(ContentPackageManager.ModsEnabled ? GameAnalyticsManager.CustomDimensions01.Modded : GameAnalyticsManager.CustomDimensions01.Vanilla);
				GameAnalyticsManager.CustomDimensions03 platform = GameAnalyticsManager.CustomDimensions03.UnknownPlatform;
				if (SteamManager.IsInitialized)
				{
					platform = GameAnalyticsManager.CustomDimensions03.Steam;
				}
				else if (EosInterface.IdQueries.IsLoggedIntoEosConnect)
				{
					platform = GameAnalyticsManager.CustomDimensions03.EGS;
				}
				GameAnalyticsManager.SetCustomDimension03(platform);
			}
			catch (Exception e3)
			{
				DebugConsole.ThrowError("Initializing GameAnalytics failed. Disabling user statistics...", e3, null, false, false);
				GameAnalyticsManager.SetConsent(GameAnalyticsManager.Consent.Error, null);
				return;
			}
			List<ContentPackage> allPackages = ContentPackageManager.EnabledPackages.All.ToList<ContentPackage>();
			if (allPackages != null && allPackages.Count > 0)
			{
				List<string> packageNames = new List<string>();
				foreach (ContentPackage cp in allPackages)
				{
					string sanitizedName = cp.Name.Replace(":", "").Replace(" ", "");
					sanitizedName = sanitizedName.Substring(0, Math.Min(32, sanitizedName.Length));
					packageNames.Add(sanitizedName);
					GameAnalyticsManager.Implementation implementation8 = GameAnalyticsManager.loadedImplementation;
					if (implementation8 != null)
					{
						implementation8.AddDesignEvent("ContentPackage:" + sanitizedName, null);
					}
				}
				packageNames.Sort();
				GameAnalyticsManager.Implementation implementation9 = GameAnalyticsManager.loadedImplementation;
				if (implementation9 != null)
				{
					implementation9.AddDesignEvent("AllContentPackages:" + string.Join(" ", packageNames), null);
				}
			}
			GameAnalyticsManager.Implementation implementation10 = GameAnalyticsManager.loadedImplementation;
			if (implementation10 == null)
			{
				return;
			}
			implementation10.AddDesignEvent("Language:" + GameSettings.CurrentConfig.Language.ToString(), null);
		}

		// Token: 0x06002257 RID: 8791 RVA: 0x000E6B44 File Offset: 0x000E4D44
		public static void ShutDown()
		{
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation != null)
			{
				implementation.Dispose();
			}
			GameAnalyticsManager.loadedImplementation = null;
		}

		// Token: 0x06002259 RID: 8793 RVA: 0x000E6BB4 File Offset: 0x000E4DB4
		[CompilerGenerated]
		internal static void <RequestAnswerFromRemoteDatabase>g__error|23_0(string reason, [Nullable(2)] Exception exception)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 3);
			defaultInterpolatedStringHandler.AppendLiteral("Error in ");
			defaultInterpolatedStringHandler.AppendFormatted("GameAnalyticsManager");
			defaultInterpolatedStringHandler.AppendLiteral(".");
			defaultInterpolatedStringHandler.AppendFormatted("RequestAnswerFromRemoteDatabase");
			defaultInterpolatedStringHandler.AppendLiteral(": ");
			defaultInterpolatedStringHandler.AppendFormatted(reason);
			DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), exception, null, false, false);
			GameAnalyticsManager.SetConsent(GameAnalyticsManager.Consent.Error, null);
		}

		// Token: 0x04001068 RID: 4200
		private const string RemoteRequestVersion = "3";

		// Token: 0x0400106A RID: 4202
		private const string consentServerUrl = "https://barotraumagame.com/baromaster/";

		// Token: 0x0400106B RID: 4203
		private const string consentServerFile = "consentserver.php";

		// Token: 0x0400106C RID: 4204
		private static readonly HashSet<string> sentEventIdentifiers = new HashSet<string>();

		// Token: 0x0400106D RID: 4205
		[Nullable(2)]
		private static GameAnalyticsManager.Implementation loadedImplementation;

		// Token: 0x0400106E RID: 4206
		private static readonly Dictionary<GameAnalyticsManager.DataSampleSize, float> dataSampleSizes = new Dictionary<GameAnalyticsManager.DataSampleSize, float>
		{
			{
				GameAnalyticsManager.DataSampleSize.Small,
				0.01f
			},
			{
				GameAnalyticsManager.DataSampleSize.Medium,
				0.05f
			},
			{
				GameAnalyticsManager.DataSampleSize.Large,
				0.5f
			},
			{
				GameAnalyticsManager.DataSampleSize.Full,
				1f
			}
		};

		// Token: 0x02000967 RID: 2407
		[NullableContext(0)]
		public enum Consent
		{
			// Token: 0x04003302 RID: 13058
			Unknown,
			// Token: 0x04003303 RID: 13059
			Error,
			// Token: 0x04003304 RID: 13060
			Ask,
			// Token: 0x04003305 RID: 13061
			No,
			// Token: 0x04003306 RID: 13062
			Yes
		}

		// Token: 0x02000968 RID: 2408
		[NullableContext(0)]
		private enum Platform
		{
			// Token: 0x04003308 RID: 13064
			Steam,
			// Token: 0x04003309 RID: 13065
			EOS,
			// Token: 0x0400330A RID: 13066
			None
		}

		// Token: 0x02000969 RID: 2409
		[Nullable(0)]
		private class AuthTicket
		{
			// Token: 0x060059A1 RID: 22945 RVA: 0x001F98CE File Offset: 0x001F7ACE
			public AuthTicket(string token, GameAnalyticsManager.Platform platform)
			{
				this.Token = (token ?? string.Empty);
				this.Platform = platform;
			}

			// Token: 0x0400330B RID: 13067
			public readonly string Token;

			// Token: 0x0400330C RID: 13068
			public readonly GameAnalyticsManager.Platform Platform;
		}

		// Token: 0x0200096A RID: 2410
		[NullableContext(0)]
		public enum ErrorSeverity
		{
			// Token: 0x0400330E RID: 13070
			Undefined,
			// Token: 0x0400330F RID: 13071
			Debug,
			// Token: 0x04003310 RID: 13072
			Info,
			// Token: 0x04003311 RID: 13073
			Warning,
			// Token: 0x04003312 RID: 13074
			Error,
			// Token: 0x04003313 RID: 13075
			Critical
		}

		// Token: 0x0200096B RID: 2411
		[NullableContext(0)]
		public enum ProgressionStatus
		{
			// Token: 0x04003315 RID: 13077
			Undefined,
			// Token: 0x04003316 RID: 13078
			Start,
			// Token: 0x04003317 RID: 13079
			Complete,
			// Token: 0x04003318 RID: 13080
			Fail
		}

		// Token: 0x0200096C RID: 2412
		[NullableContext(0)]
		public enum CustomDimensions01
		{
			// Token: 0x0400331A RID: 13082
			Vanilla,
			// Token: 0x0400331B RID: 13083
			Modded
		}

		// Token: 0x0200096D RID: 2413
		[NullableContext(0)]
		public enum CustomDimensions02
		{
			// Token: 0x0400331D RID: 13085
			None,
			// Token: 0x0400331E RID: 13086
			Difficulty0to10,
			// Token: 0x0400331F RID: 13087
			Difficulty10to20,
			// Token: 0x04003320 RID: 13088
			Difficulty20to30,
			// Token: 0x04003321 RID: 13089
			Difficulty30to40,
			// Token: 0x04003322 RID: 13090
			Difficulty40to50,
			// Token: 0x04003323 RID: 13091
			Difficulty50to60,
			// Token: 0x04003324 RID: 13092
			Difficulty60to70,
			// Token: 0x04003325 RID: 13093
			Difficulty70to80,
			// Token: 0x04003326 RID: 13094
			Difficulty80to90,
			// Token: 0x04003327 RID: 13095
			Difficulty90to100
		}

		// Token: 0x0200096E RID: 2414
		[NullableContext(0)]
		public enum CustomDimensions03
		{
			// Token: 0x04003329 RID: 13097
			UnknownPlatform,
			// Token: 0x0400332A RID: 13098
			Steam,
			// Token: 0x0400332B RID: 13099
			EGS
		}

		// Token: 0x0200096F RID: 2415
		[NullableContext(0)]
		public enum ResourceCurrency
		{
			// Token: 0x0400332D RID: 13101
			Money
		}

		// Token: 0x02000970 RID: 2416
		[NullableContext(0)]
		public enum ResourceFlowType
		{
			// Token: 0x0400332F RID: 13103
			Undefined,
			// Token: 0x04003330 RID: 13104
			Source,
			// Token: 0x04003331 RID: 13105
			Sink
		}

		// Token: 0x02000971 RID: 2417
		[NullableContext(0)]
		public enum MoneySource
		{
			// Token: 0x04003333 RID: 13107
			Unknown,
			// Token: 0x04003334 RID: 13108
			MissionReward,
			// Token: 0x04003335 RID: 13109
			Store,
			// Token: 0x04003336 RID: 13110
			Event,
			// Token: 0x04003337 RID: 13111
			Ability,
			// Token: 0x04003338 RID: 13112
			Cheat
		}

		// Token: 0x02000972 RID: 2418
		[NullableContext(0)]
		public enum MoneySink
		{
			// Token: 0x0400333A RID: 13114
			Unknown,
			// Token: 0x0400333B RID: 13115
			Store,
			// Token: 0x0400333C RID: 13116
			Service,
			// Token: 0x0400333D RID: 13117
			Crew,
			// Token: 0x0400333E RID: 13118
			SubmarineUpgrade,
			// Token: 0x0400333F RID: 13119
			SubmarineWeapon,
			// Token: 0x04003340 RID: 13120
			SubmarinePurchase,
			// Token: 0x04003341 RID: 13121
			SubmarineSwitch
		}

		// Token: 0x02000973 RID: 2419
		[Nullable(0)]
		private class Implementation : IDisposable
		{
			// Token: 0x060059A2 RID: 22946 RVA: 0x001F98ED File Offset: 0x001F7AED
			internal void Initialize(string gameKey, string secretKey)
			{
				this.initialize(gameKey, secretKey);
			}

			// Token: 0x060059A3 RID: 22947 RVA: 0x001F98FC File Offset: 0x001F7AFC
			internal void ConfigureBuild(string config)
			{
				this.configureBuild(config);
			}

			// Token: 0x060059A4 RID: 22948 RVA: 0x001F990A File Offset: 0x001F7B0A
			internal void AddErrorEvent(GameAnalyticsManager.ErrorSeverity severity, string message)
			{
				this.addErrorEvent(severity, message);
			}

			// Token: 0x060059A5 RID: 22949 RVA: 0x001F9919 File Offset: 0x001F7B19
			internal void AddDesignEvent(string message, [Nullable(new byte[]
			{
				2,
				1,
				1
			})] IDictionary<string, object> fields = null)
			{
				this.addDesignEvent0(message, fields);
			}

			// Token: 0x060059A6 RID: 22950 RVA: 0x001F9928 File Offset: 0x001F7B28
			internal void AddDesignEvent(string message, double value)
			{
				this.addDesignEvent1(message, value);
			}

			// Token: 0x060059A7 RID: 22951 RVA: 0x001F9937 File Offset: 0x001F7B37
			internal void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus status, string progression01)
			{
				this.addProgressionEvent01(status, progression01);
			}

			// Token: 0x060059A8 RID: 22952 RVA: 0x001F9946 File Offset: 0x001F7B46
			internal void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus status, string progression01, double score)
			{
				this.addProgressionEvent01Score(status, progression01, score);
			}

			// Token: 0x060059A9 RID: 22953 RVA: 0x001F9956 File Offset: 0x001F7B56
			internal void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus status, string progression01, string progression02)
			{
				this.addProgressionEvent02(status, progression01, progression02);
			}

			// Token: 0x060059AA RID: 22954 RVA: 0x001F9966 File Offset: 0x001F7B66
			internal void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus status, string progression01, string progression02, string progression03)
			{
				this.addProgressionEvent03(status, progression01, progression02, progression03);
			}

			// Token: 0x060059AB RID: 22955 RVA: 0x001F9978 File Offset: 0x001F7B78
			internal void AddResourceEvent(GameAnalyticsManager.ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
			{
				this.addResourceEvent(flowType, currency, amount, itemType, itemId);
			}

			// Token: 0x060059AC RID: 22956 RVA: 0x001F998C File Offset: 0x001F7B8C
			internal void SetCustomDimension01(string dimension01)
			{
				this.setCustomDimension01(dimension01);
			}

			// Token: 0x060059AD RID: 22957 RVA: 0x001F999A File Offset: 0x001F7B9A
			internal void ConfigureAvailableCustomDimensions01(params GameAnalyticsManager.CustomDimensions01[] customDimensions)
			{
				this.configureAvailableCustomDimensions01((from d in customDimensions
				select d.ToString()).ToArray<string>());
			}

			// Token: 0x060059AE RID: 22958 RVA: 0x001F99D1 File Offset: 0x001F7BD1
			internal void SetCustomDimension02(string dimension02)
			{
				this.setCustomDimension02(dimension02);
			}

			// Token: 0x060059AF RID: 22959 RVA: 0x001F99DF File Offset: 0x001F7BDF
			internal void ConfigureAvailableCustomDimensions02(params GameAnalyticsManager.CustomDimensions02[] customDimensions)
			{
				this.configureAvailableCustomDimensions02((from d in customDimensions
				select d.ToString()).ToArray<string>());
			}

			// Token: 0x060059B0 RID: 22960 RVA: 0x001F9A16 File Offset: 0x001F7C16
			internal void ConfigureAvailableResourceCurrencies(params GameAnalyticsManager.ResourceCurrency[] customDimensions)
			{
				this.configureAvailableResourceCurrencies((from d in customDimensions
				select d.ToString()).ToArray<string>());
			}

			// Token: 0x060059B1 RID: 22961 RVA: 0x001F9A4D File Offset: 0x001F7C4D
			internal void ConfigureAvailableCustomDimensions03(params GameAnalyticsManager.CustomDimensions03[] customDimensions)
			{
				this.configureAvailableCustomDimensions03((from d in customDimensions
				select d.ToString()).ToArray<string>());
			}

			// Token: 0x060059B2 RID: 22962 RVA: 0x001F9A84 File Offset: 0x001F7C84
			internal void SetCustomDimension03(string dimension03)
			{
				this.setCustomDimension03(dimension03);
			}

			// Token: 0x060059B3 RID: 22963 RVA: 0x001F9A92 File Offset: 0x001F7C92
			internal void ConfigureAvailableResourceItemTypes(params string[] resourceItemTypes)
			{
				this.configureAvailableResourceItemTypes(resourceItemTypes);
			}

			// Token: 0x060059B4 RID: 22964 RVA: 0x001F9AA0 File Offset: 0x001F7CA0
			internal void SetEnabledInfoLog(bool enabled)
			{
				this.setEnabledInfoLog(enabled);
			}

			// Token: 0x060059B5 RID: 22965 RVA: 0x001F9AAE File Offset: 0x001F7CAE
			internal void SetEnabledVerboseLog(bool enabled)
			{
				this.setEnabledVerboseLog(enabled);
			}

			// Token: 0x060059B6 RID: 22966 RVA: 0x001F9ABC File Offset: 0x001F7CBC
			private Action Call(MethodInfo methodInfo)
			{
				return delegate()
				{
					MethodInfo methodInfo2 = methodInfo;
					if (methodInfo2 == null)
					{
						return;
					}
					methodInfo2.Invoke(null, null);
				};
			}

			// Token: 0x060059B7 RID: 22967 RVA: 0x001F9AE4 File Offset: 0x001F7CE4
			private Action<T> Call<[Nullable(2)] T>(MethodInfo methodInfo)
			{
				return delegate(T arg1)
				{
					this.args1[0] = arg1;
					methodInfo.Invoke(null, this.args1);
				};
			}

			// Token: 0x060059B8 RID: 22968 RVA: 0x001F9B14 File Offset: 0x001F7D14
			private Action<T1, T2> Call<[Nullable(2)] T1, [Nullable(2)] T2>(MethodInfo methodInfo)
			{
				return delegate(T1 arg1, T2 arg2)
				{
					this.args2[0] = arg1;
					this.args2[1] = arg2;
					methodInfo.Invoke(null, this.args2);
				};
			}

			// Token: 0x060059B9 RID: 22969 RVA: 0x001F9B44 File Offset: 0x001F7D44
			[NullableContext(2)]
			[return: Nullable(1)]
			private Action<T1, T2, T3> Call<T1, T2, T3>([Nullable(1)] MethodInfo methodInfo)
			{
				return delegate(T1 arg1, T2 arg2, T3 arg3)
				{
					this.args3[0] = arg1;
					this.args3[1] = arg2;
					this.args3[2] = arg3;
					methodInfo.Invoke(null, this.args3);
				};
			}

			// Token: 0x060059BA RID: 22970 RVA: 0x001F9B74 File Offset: 0x001F7D74
			[NullableContext(2)]
			[return: Nullable(1)]
			private Action<T1, T2, T3, T4> Call<T1, T2, T3, T4>([Nullable(1)] MethodInfo methodInfo)
			{
				return delegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4)
				{
					this.args4[0] = arg1;
					this.args4[1] = arg2;
					this.args4[2] = arg3;
					this.args4[3] = arg4;
					methodInfo.Invoke(null, this.args4);
				};
			}

			// Token: 0x060059BB RID: 22971 RVA: 0x001F9BA4 File Offset: 0x001F7DA4
			[NullableContext(2)]
			[return: Nullable(1)]
			private Action<T1, T2, T3, T4, T5> Call<T1, T2, T3, T4, T5>([Nullable(1)] MethodInfo methodInfo)
			{
				return delegate(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
				{
					this.args5[0] = arg1;
					this.args5[1] = arg2;
					this.args5[2] = arg3;
					this.args5[3] = arg4;
					this.args5[4] = arg5;
					methodInfo.Invoke(null, this.args5);
				};
			}

			// Token: 0x060059BC RID: 22972 RVA: 0x001F9BD1 File Offset: 0x001F7DD1
			private string GetAssemblyPath(string assemblyName)
			{
				return Path.Combine(new string[]
				{
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					assemblyName + ".dll"
				});
			}

			// Token: 0x060059BD RID: 22973 RVA: 0x001F9C00 File Offset: 0x001F7E00
			[return: Nullable(2)]
			private Assembly ResolveDependency(AssemblyLoadContext context, AssemblyName dependencyName)
			{
				if (this.resolvingDependency)
				{
					return null;
				}
				this.resolvingDependency = true;
				string name = dependencyName.Name;
				if (name == null)
				{
					throw new Exception("Dependency name was null");
				}
				Assembly dependency = context.LoadFromAssemblyPath(this.GetAssemblyPath(name));
				this.resolvingDependency = false;
				return dependency;
			}

			// Token: 0x060059BE RID: 22974 RVA: 0x001F9C48 File Offset: 0x001F7E48
			internal Implementation()
			{
				this.loadContext = new AssemblyLoadContext("GameAnalytics.NetStandard", true);
				this.loadContext.Resolving += this.ResolveDependency;
				this.assembly = this.loadContext.LoadFromAssemblyPath(this.GetAssemblyPath("GameAnalytics.NetStandard"));
				GameAnalyticsManager.Implementation.<>c__DisplayClass60_0 CS$<>8__locals1;
				CS$<>8__locals1.mainClass = this.<.ctor>g__getType|60_0("GameAnalytics");
				Type errorSeverityEnumType = this.<.ctor>g__getType|60_0("EGAErrorSeverity");
				Type progressionStatusEnumType = this.<.ctor>g__getType|60_0("EGAProgressionStatus");
				Type resourceFlowTypeEnumType = this.<.ctor>g__getType|60_0("EGAResourceFlowType");
				this.initialize = this.Call<string, string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("Initialize", new Type[]
				{
					typeof(string),
					typeof(string)
				}, ref CS$<>8__locals1));
				this.configureBuild = this.Call<string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("ConfigureBuild", new Type[]
				{
					typeof(string)
				}, ref CS$<>8__locals1));
				this.addErrorEvent = this.Call<GameAnalyticsManager.ErrorSeverity, string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("AddErrorEvent", new Type[]
				{
					errorSeverityEnumType,
					typeof(string)
				}, ref CS$<>8__locals1));
				this.addDesignEvent0 = this.Call<string, IDictionary<string, object>>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("AddDesignEvent", new Type[]
				{
					typeof(string),
					typeof(IDictionary<string, object>)
				}, ref CS$<>8__locals1));
				this.addDesignEvent1 = this.Call<string, double>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("AddDesignEvent", new Type[]
				{
					typeof(string),
					typeof(double)
				}, ref CS$<>8__locals1));
				this.addProgressionEvent01 = this.Call<GameAnalyticsManager.ProgressionStatus, string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("AddProgressionEvent", new Type[]
				{
					progressionStatusEnumType,
					typeof(string)
				}, ref CS$<>8__locals1));
				this.addProgressionEvent01Score = this.Call<GameAnalyticsManager.ProgressionStatus, string, double>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("AddProgressionEvent", new Type[]
				{
					progressionStatusEnumType,
					typeof(string),
					typeof(double)
				}, ref CS$<>8__locals1));
				this.addProgressionEvent02 = this.Call<GameAnalyticsManager.ProgressionStatus, string, string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("AddProgressionEvent", new Type[]
				{
					progressionStatusEnumType,
					typeof(string),
					typeof(string)
				}, ref CS$<>8__locals1));
				this.addProgressionEvent03 = this.Call<GameAnalyticsManager.ProgressionStatus, string, string, string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("AddProgressionEvent", new Type[]
				{
					progressionStatusEnumType,
					typeof(string),
					typeof(string),
					typeof(string)
				}, ref CS$<>8__locals1));
				this.setCustomDimension01 = this.Call<string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("SetCustomDimension01", new Type[]
				{
					typeof(string)
				}, ref CS$<>8__locals1));
				this.configureAvailableCustomDimensions01 = this.Call<string[]>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("ConfigureAvailableCustomDimensions01", new Type[]
				{
					typeof(string[])
				}, ref CS$<>8__locals1));
				this.setCustomDimension02 = this.Call<string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("SetCustomDimension02", new Type[]
				{
					typeof(string)
				}, ref CS$<>8__locals1));
				this.configureAvailableCustomDimensions02 = this.Call<string[]>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("ConfigureAvailableCustomDimensions02", new Type[]
				{
					typeof(string[])
				}, ref CS$<>8__locals1));
				this.configureAvailableCustomDimensions03 = this.Call<string[]>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("ConfigureAvailableCustomDimensions03", new Type[]
				{
					typeof(string[])
				}, ref CS$<>8__locals1));
				this.setCustomDimension03 = this.Call<string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("SetCustomDimension03", new Type[]
				{
					typeof(string)
				}, ref CS$<>8__locals1));
				this.configureAvailableResourceCurrencies = this.Call<string[]>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("ConfigureAvailableResourceCurrencies", new Type[]
				{
					typeof(string[])
				}, ref CS$<>8__locals1));
				this.configureAvailableResourceItemTypes = this.Call<string[]>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("ConfigureAvailableResourceItemTypes", new Type[]
				{
					typeof(string[])
				}, ref CS$<>8__locals1));
				this.addResourceEvent = this.Call<GameAnalyticsManager.ResourceFlowType, string, float, string, string>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("AddResourceEvent", new Type[]
				{
					resourceFlowTypeEnumType,
					typeof(string),
					typeof(float),
					typeof(string),
					typeof(string)
				}, ref CS$<>8__locals1));
				this.setEnabledInfoLog = this.Call<bool>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("SetEnabledInfoLog", new Type[]
				{
					typeof(bool)
				}, ref CS$<>8__locals1));
				this.setEnabledVerboseLog = this.Call<bool>(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("SetEnabledVerboseLog", new Type[]
				{
					typeof(bool)
				}, ref CS$<>8__locals1));
				this.onQuit = this.Call(GameAnalyticsManager.Implementation.<.ctor>g__getMethod|60_1("OnQuit", Array.Empty<Type>(), ref CS$<>8__locals1));
			}

			// Token: 0x060059BF RID: 22975 RVA: 0x001FA124 File Offset: 0x001F8324
			private void OnQuit()
			{
				try
				{
					if (this.assembly != null)
					{
						Action action = this.onQuit;
						if (action != null)
						{
							action();
						}
					}
				}
				catch (Exception e)
				{
					e = e.GetInnermost();
					DebugConsole.AddWarning("Failed to call GameAnalytics.OnQuit: " + e.Message + " " + e.StackTrace, null);
				}
			}

			// Token: 0x060059C0 RID: 22976 RVA: 0x001FA190 File Offset: 0x001F8390
			public void Dispose()
			{
				if (this.loadContext == null)
				{
					return;
				}
				this.OnQuit();
				AssemblyLoadContext assemblyLoadContext = this.loadContext;
				if (assemblyLoadContext != null)
				{
					assemblyLoadContext.Unload();
				}
				this.loadContext = null;
				this.assembly = null;
			}

			// Token: 0x060059C1 RID: 22977 RVA: 0x001FA1C0 File Offset: 0x001F83C0
			[CompilerGenerated]
			private Type <.ctor>g__getType|60_0(string name)
			{
				Type type = this.assembly.GetType("GameAnalyticsSDK.Net." + name);
				if (type == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Could not find type\"");
					defaultInterpolatedStringHandler.AppendFormatted("GameAnalyticsSDK.Net");
					defaultInterpolatedStringHandler.AppendLiteral(".");
					defaultInterpolatedStringHandler.AppendFormatted(name);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
				}
				return type;
			}

			// Token: 0x060059C2 RID: 22978 RVA: 0x001FA238 File Offset: 0x001F8438
			[CompilerGenerated]
			internal static MethodInfo <.ctor>g__getMethod|60_1(string name, Type[] types, ref GameAnalyticsManager.Implementation.<>c__DisplayClass60_0 A_2)
			{
				foreach (MethodInfo me in A_2.mainClass.GetMethods())
				{
				}
				Type mainClass = A_2.mainClass;
				MethodInfo methodInfo = (mainClass != null) ? mainClass.GetMethod(name, BindingFlags.Static | BindingFlags.Public, null, types, null) : null;
				if (methodInfo == null)
				{
					throw new Exception("Could not find method \"" + name + "\" with types " + string.Join<string>(',', from t in types
					select t.Name));
				}
				return methodInfo;
			}

			// Token: 0x04003342 RID: 13122
			private readonly Action<string, string> initialize;

			// Token: 0x04003343 RID: 13123
			private readonly Action<string> configureBuild;

			// Token: 0x04003344 RID: 13124
			private readonly Action<GameAnalyticsManager.ErrorSeverity, string> addErrorEvent;

			// Token: 0x04003345 RID: 13125
			[Nullable(new byte[]
			{
				1,
				1,
				2,
				1,
				1
			})]
			private readonly Action<string, IDictionary<string, object>> addDesignEvent0;

			// Token: 0x04003346 RID: 13126
			private readonly Action<string, double> addDesignEvent1;

			// Token: 0x04003347 RID: 13127
			private readonly Action<GameAnalyticsManager.ProgressionStatus, string> addProgressionEvent01;

			// Token: 0x04003348 RID: 13128
			private readonly Action<GameAnalyticsManager.ProgressionStatus, string, double> addProgressionEvent01Score;

			// Token: 0x04003349 RID: 13129
			private readonly Action<GameAnalyticsManager.ProgressionStatus, string, string> addProgressionEvent02;

			// Token: 0x0400334A RID: 13130
			private readonly Action<GameAnalyticsManager.ProgressionStatus, string, string, string> addProgressionEvent03;

			// Token: 0x0400334B RID: 13131
			private readonly Action<GameAnalyticsManager.ResourceFlowType, string, float, string, string> addResourceEvent;

			// Token: 0x0400334C RID: 13132
			private readonly Action<string> setCustomDimension01;

			// Token: 0x0400334D RID: 13133
			private readonly Action<string[]> configureAvailableCustomDimensions01;

			// Token: 0x0400334E RID: 13134
			private readonly Action<string> setCustomDimension02;

			// Token: 0x0400334F RID: 13135
			private readonly Action<string[]> configureAvailableCustomDimensions02;

			// Token: 0x04003350 RID: 13136
			private readonly Action<string[]> configureAvailableResourceCurrencies;

			// Token: 0x04003351 RID: 13137
			private readonly Action<string[]> configureAvailableCustomDimensions03;

			// Token: 0x04003352 RID: 13138
			private readonly Action<string> setCustomDimension03;

			// Token: 0x04003353 RID: 13139
			private readonly Action<string[]> configureAvailableResourceItemTypes;

			// Token: 0x04003354 RID: 13140
			private readonly Action<bool> setEnabledInfoLog;

			// Token: 0x04003355 RID: 13141
			private readonly Action<bool> setEnabledVerboseLog;

			// Token: 0x04003356 RID: 13142
			private const string AssemblyName = "GameAnalytics.NetStandard";

			// Token: 0x04003357 RID: 13143
			private const string Namespace = "GameAnalyticsSDK.Net";

			// Token: 0x04003358 RID: 13144
			private const string MainClass = "GameAnalytics";

			// Token: 0x04003359 RID: 13145
			private const string EnumPrefix = "EGA";

			// Token: 0x0400335A RID: 13146
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args1 = new object[1];

			// Token: 0x0400335B RID: 13147
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args2 = new object[2];

			// Token: 0x0400335C RID: 13148
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args3 = new object[3];

			// Token: 0x0400335D RID: 13149
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args4 = new object[4];

			// Token: 0x0400335E RID: 13150
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args5 = new object[5];

			// Token: 0x0400335F RID: 13151
			[Nullable(2)]
			private AssemblyLoadContext loadContext;

			// Token: 0x04003360 RID: 13152
			[Nullable(2)]
			private Assembly assembly;

			// Token: 0x04003361 RID: 13153
			private bool resolvingDependency;

			// Token: 0x04003362 RID: 13154
			[Nullable(2)]
			private readonly Action onQuit;
		}

		// Token: 0x02000974 RID: 2420
		[NullableContext(0)]
		public enum DataSampleSize
		{
			// Token: 0x04003364 RID: 13156
			Small,
			// Token: 0x04003365 RID: 13157
			Medium,
			// Token: 0x04003366 RID: 13158
			Large,
			// Token: 0x04003367 RID: 13159
			Full
		}
	}
}
