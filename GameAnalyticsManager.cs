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
	// Token: 0x02000061 RID: 97
	[NullableContext(1)]
	[Nullable(0)]
	internal static class GameAnalyticsManager
	{
		// Token: 0x170003AE RID: 942
		// (get) Token: 0x06000D23 RID: 3363 RVA: 0x00079AE6 File Offset: 0x00077CE6
		// (set) Token: 0x06000D24 RID: 3364 RVA: 0x00079AED File Offset: 0x00077CED
		public static GameAnalyticsManager.Consent UserConsented { get; private set; } = GameAnalyticsManager.Consent.Unknown;

		// Token: 0x170003AF RID: 943
		// (get) Token: 0x06000D25 RID: 3365 RVA: 0x00079AF5 File Offset: 0x00077CF5
		public static bool SendUserStatistics
		{
			get
			{
				return GameAnalyticsManager.UserConsented == GameAnalyticsManager.Consent.Yes && GameAnalyticsManager.loadedImplementation != null;
			}
		}

		// Token: 0x170003B0 RID: 944
		// (get) Token: 0x06000D26 RID: 3366 RVA: 0x00079B09 File Offset: 0x00077D09
		private static bool ConsentTextAvailable
		{
			get
			{
				return TextManager.ContainsTag("statisticsconsentheader") && TextManager.ContainsTag("statisticsconsenttext");
			}
		}

		// Token: 0x06000D27 RID: 3367 RVA: 0x00079B24 File Offset: 0x00077D24
		private static Task<GameAnalyticsManager.AuthTicket> GetAuthTicket()
		{
			GameAnalyticsManager.<GetAuthTicket>d__14 <GetAuthTicket>d__;
			<GetAuthTicket>d__.<>t__builder = AsyncTaskMethodBuilder<GameAnalyticsManager.AuthTicket>.Create();
			<GetAuthTicket>d__.<>1__state = -1;
			<GetAuthTicket>d__.<>t__builder.Start<GameAnalyticsManager.<GetAuthTicket>d__14>(ref <GetAuthTicket>d__);
			return <GetAuthTicket>d__.<>t__builder.Task;
		}

		// Token: 0x06000D28 RID: 3368 RVA: 0x00079B60 File Offset: 0x00077D60
		private static Task<GameAnalyticsManager.AuthTicket> GetSteamAuthTicket()
		{
			GameAnalyticsManager.<GetSteamAuthTicket>d__15 <GetSteamAuthTicket>d__;
			<GetSteamAuthTicket>d__.<>t__builder = AsyncTaskMethodBuilder<GameAnalyticsManager.AuthTicket>.Create();
			<GetSteamAuthTicket>d__.<>1__state = -1;
			<GetSteamAuthTicket>d__.<>t__builder.Start<GameAnalyticsManager.<GetSteamAuthTicket>d__15>(ref <GetSteamAuthTicket>d__);
			return <GetSteamAuthTicket>d__.<>t__builder.Task;
		}

		// Token: 0x06000D29 RID: 3369 RVA: 0x00079B9C File Offset: 0x00077D9C
		private static Task<GameAnalyticsManager.AuthTicket> GetEOSAuthTicket()
		{
			GameAnalyticsManager.<GetEOSAuthTicket>d__16 <GetEOSAuthTicket>d__;
			<GetEOSAuthTicket>d__.<>t__builder = AsyncTaskMethodBuilder<GameAnalyticsManager.AuthTicket>.Create();
			<GetEOSAuthTicket>d__.<>1__state = -1;
			<GetEOSAuthTicket>d__.<>t__builder.Start<GameAnalyticsManager.<GetEOSAuthTicket>d__16>(ref <GetEOSAuthTicket>d__);
			return <GetEOSAuthTicket>d__.<>t__builder.Task;
		}

		// Token: 0x06000D2A RID: 3370 RVA: 0x00079BD7 File Offset: 0x00077DD7
		[NullableContext(2)]
		public static void SetConsent(GameAnalyticsManager.Consent consent, Action onAnswerSent = null)
		{
			if (consent == GameAnalyticsManager.Consent.Yes)
			{
				throw new Exception("Cannot call SetConsent with value Consent.Yes, must only be set to this value via consent prompt");
			}
			GameAnalyticsManager.SetConsentInternal(consent, onAnswerSent);
		}

		// Token: 0x06000D2B RID: 3371 RVA: 0x00079BF0 File Offset: 0x00077DF0
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
				if (consent == GameAnalyticsManager.Consent.Ask)
				{
					Action action;
					if ((action = GameAnalyticsManager.<>O.<0>__CreateConsentPrompt) == null)
					{
						action = (GameAnalyticsManager.<>O.<0>__CreateConsentPrompt = new Action(GameAnalyticsManager.CreateConsentPrompt));
					}
					GameMain.ExecuteAfterContentFinishedLoading(action);
				}
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

		// Token: 0x06000D2C RID: 3372 RVA: 0x00079CB8 File Offset: 0x00077EB8
		private static Task<bool> SendAnswerToRemoteDatabase(GameAnalyticsManager.Consent consent)
		{
			GameAnalyticsManager.<SendAnswerToRemoteDatabase>d__19 <SendAnswerToRemoteDatabase>d__;
			<SendAnswerToRemoteDatabase>d__.<>t__builder = AsyncTaskMethodBuilder<bool>.Create();
			<SendAnswerToRemoteDatabase>d__.consent = consent;
			<SendAnswerToRemoteDatabase>d__.<>1__state = -1;
			<SendAnswerToRemoteDatabase>d__.<>t__builder.Start<GameAnalyticsManager.<SendAnswerToRemoteDatabase>d__19>(ref <SendAnswerToRemoteDatabase>d__);
			return <SendAnswerToRemoteDatabase>d__.<>t__builder.Task;
		}

		// Token: 0x06000D2D RID: 3373 RVA: 0x00079CFB File Offset: 0x00077EFB
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

		// Token: 0x06000D2E RID: 3374 RVA: 0x00079D30 File Offset: 0x00077F30
		private static void CreateConsentPrompt()
		{
			if (GameAnalyticsManager.ConsentTextAvailable)
			{
				GameAnalyticsManager.<>c__DisplayClass21_0 CS$<>8__locals1 = new GameAnalyticsManager.<>c__DisplayClass21_0();
				CS$<>8__locals1.background = new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
				GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(0.5f, 0.7f), CS$<>8__locals1.background.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(800, 0),
					MaxSize = new Point(1500, int.MaxValue)
				}, "", null);
				CS$<>8__locals1.content = new GUILayoutGroup(new RectTransform(new Vector2(0.95f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					Stretch = true,
					AbsoluteSpacing = GUI.IntScale(15f)
				};
				string consentTextTag = "statisticsconsenttext";
				if (EosInterface.IdQueries.IsLoggedIntoEosConnect)
				{
					consentTextTag = "statisticsconsenteostext";
				}
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = TextManager.Get("statisticsconsentheader");
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT, text, new Color?(Color.White), subHeadingFont, Alignment.Left, false, "", null);
				GUITextBlock mainText = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(TextManager.Get(consentTextTag), null), null, null, Alignment.Left, true, "", null);
				foreach (RichTextData data in mainText.RichTextData.Value)
				{
					List<GUITextBlock.ClickableArea> clickableAreas = mainText.ClickableAreas;
					GUITextBlock.ClickableArea item = default(GUITextBlock.ClickableArea);
					item.Data = data;
					item.OnClick = delegate(GUITextBlock component, GUITextBlock.ClickableArea area)
					{
						GameMain.ShowOpenUriPrompt("https://gameanalytics.com/privacy/", "openlinkinbrowserprompt", null);
					};
					clickableAreas.Add(item);
				}
				string privacyPolicyText = File.ReadAllText("daedalic_privacypolicy.txt", null, true);
				GUIListBox privacyPolicyBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.5f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MaxSize = new Point(int.MaxValue, GUI.IntScale(200f))
				}, false, null, "", true, false);
				GUITextBlock privacyPolicy = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), privacyPolicyBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), privacyPolicyText, null, null, Alignment.Left, true, "", null)
				{
					CanBeFocused = false
				};
				privacyPolicy.RectTransform.MinSize = new Point(0, (int)privacyPolicy.TextSize.Y);
				new GUITextBlock(new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("statisticsconsentstatement"), null, null, Alignment.Left, true, "", null);
				CS$<>8__locals1.buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				CS$<>8__locals1.<CreateConsentPrompt>g__buttonContainerSpacing|0(0.1f);
				GUIButton yesBtn = new GUIButton(new RectTransform(new Vector2(0.3f, 1f), CS$<>8__locals1.buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Yes"), Alignment.Center, "", null);
				GUIButton guibutton = yesBtn;
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
				{
					GUIMessageBox.MessageBoxes.Remove(CS$<>8__locals1.background);
					GUIMessageBox loadingBox = GUIMessageBox.CreateLoadingBox(TextManager.Get("PleaseWait"), null, null);
					GameAnalyticsManager.SetConsentInternal(GameAnalyticsManager.Consent.Yes, new Action(loadingBox.Close));
					return true;
				}));
				yesBtn.Enabled = false;
				CS$<>8__locals1.<CreateConsentPrompt>g__buttonContainerSpacing|0(0.2f);
				GUIButton noBtn = new GUIButton(new RectTransform(new Vector2(0.3f, 1f), CS$<>8__locals1.buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("No"), Alignment.Center, "", null);
				GUIButton guibutton2 = noBtn;
				guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
				{
					GUIMessageBox.MessageBoxes.Remove(CS$<>8__locals1.background);
					GUIMessageBox loadingBox = GUIMessageBox.CreateLoadingBox(TextManager.Get("PleaseWait"), null, null);
					GameAnalyticsManager.SetConsent(GameAnalyticsManager.Consent.No, new Action(loadingBox.Close));
					return true;
				}));
				noBtn.Enabled = false;
				CoroutineManager.StartCoroutine(GameAnalyticsManager.<CreateConsentPrompt>g__enableAfterTime|21_1(new WaitForSeconds(0.3f, true), new GUIComponent[]
				{
					yesBtn,
					noBtn
				}), "");
				CS$<>8__locals1.<CreateConsentPrompt>g__buttonContainerSpacing|0(0.1f);
				CS$<>8__locals1.buttonContainer.RectTransform.MinSize = new Point(0, yesBtn.RectTransform.MinSize.Y);
				CS$<>8__locals1.buttonContainer.RectTransform.MaxSize = new Point(int.MaxValue, yesBtn.RectTransform.MinSize.Y);
				CS$<>8__locals1.content.Recalculate();
				foreach (GUIComponent child in CS$<>8__locals1.content.Children)
				{
					GUITextBlock textBlock = child as GUITextBlock;
					if (textBlock != null)
					{
						textBlock.TextScale = MathHelper.Min(1f, 1f / GameSettings.CurrentConfig.Graphics.TextScale);
						textBlock.RectTransform.MinSize = new Point(0, (int)textBlock.TextSize.Y);
						textBlock.RectTransform.MaxSize = new Point(int.MaxValue, (int)textBlock.TextSize.Y);
					}
				}
				int contentHeight = CS$<>8__locals1.content.Children.Sum((GUIComponent c) => c.RectTransform.MaxSize.Y + CS$<>8__locals1.content.AbsoluteSpacing);
				frame.RectTransform.MinSize = new Point(frame.RectTransform.MinSize.X, (int)((float)contentHeight / CS$<>8__locals1.content.RectTransform.RelativeSize.Y));
				frame.RectTransform.MaxSize = new Point(frame.RectTransform.MaxSize.X, (int)((float)contentHeight / CS$<>8__locals1.content.RectTransform.RelativeSize.Y));
				GUIMessageBox.MessageBoxes.Add(CS$<>8__locals1.background);
				return;
			}
			GameAnalyticsManager.SetConsent(GameAnalyticsManager.Consent.Unknown, null);
		}

		// Token: 0x06000D2F RID: 3375 RVA: 0x0007A4CC File Offset: 0x000786CC
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

		// Token: 0x06000D30 RID: 3376 RVA: 0x0007A544 File Offset: 0x00078744
		private static Task<GameAnalyticsManager.Consent> RequestAnswerFromRemoteDatabase()
		{
			GameAnalyticsManager.<RequestAnswerFromRemoteDatabase>d__23 <RequestAnswerFromRemoteDatabase>d__;
			<RequestAnswerFromRemoteDatabase>d__.<>t__builder = AsyncTaskMethodBuilder<GameAnalyticsManager.Consent>.Create();
			<RequestAnswerFromRemoteDatabase>d__.<>1__state = -1;
			<RequestAnswerFromRemoteDatabase>d__.<>t__builder.Start<GameAnalyticsManager.<RequestAnswerFromRemoteDatabase>d__23>(ref <RequestAnswerFromRemoteDatabase>d__);
			return <RequestAnswerFromRemoteDatabase>d__.<>t__builder.Task;
		}

		// Token: 0x06000D31 RID: 3377 RVA: 0x0007A580 File Offset: 0x00078780
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

		// Token: 0x06000D32 RID: 3378 RVA: 0x0007A678 File Offset: 0x00078878
		private static void ValidateEventID(string eventID)
		{
		}

		// Token: 0x06000D33 RID: 3379 RVA: 0x0007A67A File Offset: 0x0007887A
		public static bool ShouldLogRandomSample(GameAnalyticsManager.DataSampleSize sampleSize = GameAnalyticsManager.DataSampleSize.Small)
		{
			return Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < GameAnalyticsManager.dataSampleSizes[sampleSize];
		}

		// Token: 0x06000D34 RID: 3380 RVA: 0x0007A699 File Offset: 0x00078899
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

		// Token: 0x06000D35 RID: 3381 RVA: 0x0007A6B4 File Offset: 0x000788B4
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

		// Token: 0x06000D36 RID: 3382 RVA: 0x0007A709 File Offset: 0x00078909
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

		// Token: 0x06000D37 RID: 3383 RVA: 0x0007A72A File Offset: 0x0007892A
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

		// Token: 0x06000D38 RID: 3384 RVA: 0x0007A74B File Offset: 0x0007894B
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

		// Token: 0x06000D39 RID: 3385 RVA: 0x0007A766 File Offset: 0x00078966
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

		// Token: 0x06000D3A RID: 3386 RVA: 0x0007A782 File Offset: 0x00078982
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

		// Token: 0x06000D3B RID: 3387 RVA: 0x0007A79E File Offset: 0x0007899E
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

		// Token: 0x06000D3C RID: 3388 RVA: 0x0007A7BB File Offset: 0x000789BB
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

		// Token: 0x06000D3D RID: 3389 RVA: 0x0007A7E1 File Offset: 0x000789E1
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

		// Token: 0x06000D3E RID: 3390 RVA: 0x0007A808 File Offset: 0x00078A08
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

		// Token: 0x06000D3F RID: 3391 RVA: 0x0007A86C File Offset: 0x00078A6C
		public static void AddMoneyGainedEvent(int amount, GameAnalyticsManager.MoneySource moneySource, string eventId)
		{
			GameAnalyticsManager.AddResourceEvent(GameAnalyticsManager.ResourceFlowType.Source, GameAnalyticsManager.ResourceCurrency.Money, (float)amount, moneySource.ToString(), eventId);
		}

		// Token: 0x06000D40 RID: 3392 RVA: 0x0007A885 File Offset: 0x00078A85
		public static void AddMoneySpentEvent(int amount, GameAnalyticsManager.MoneySink moneySink, string eventId)
		{
			GameAnalyticsManager.AddResourceEvent(GameAnalyticsManager.ResourceFlowType.Sink, GameAnalyticsManager.ResourceCurrency.Money, (float)amount, moneySink.ToString(), eventId);
		}

		// Token: 0x06000D41 RID: 3393 RVA: 0x0007A89E File Offset: 0x00078A9E
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

		// Token: 0x06000D42 RID: 3394 RVA: 0x0007A8CC File Offset: 0x00078ACC
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

		// Token: 0x06000D43 RID: 3395 RVA: 0x0007ACFC File Offset: 0x00078EFC
		public static void ShutDown()
		{
			GameAnalyticsManager.Implementation implementation = GameAnalyticsManager.loadedImplementation;
			if (implementation != null)
			{
				implementation.Dispose();
			}
			GameAnalyticsManager.loadedImplementation = null;
		}

		// Token: 0x06000D45 RID: 3397 RVA: 0x0007AD6C File Offset: 0x00078F6C
		[NullableContext(0)]
		[CompilerGenerated]
		internal static IEnumerable<CoroutineStatus> <CreateConsentPrompt>g__enableAfterTime|21_1(WaitForSeconds time, params GUIComponent[] components)
		{
			GameAnalyticsManager.<<CreateConsentPrompt>g__enableAfterTime|21_1>d <<CreateConsentPrompt>g__enableAfterTime|21_1>d = new GameAnalyticsManager.<<CreateConsentPrompt>g__enableAfterTime|21_1>d(-2);
			<<CreateConsentPrompt>g__enableAfterTime|21_1>d.<>3__time = time;
			<<CreateConsentPrompt>g__enableAfterTime|21_1>d.<>3__components = components;
			return <<CreateConsentPrompt>g__enableAfterTime|21_1>d;
		}

		// Token: 0x06000D46 RID: 3398 RVA: 0x0007AD90 File Offset: 0x00078F90
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

		// Token: 0x040006CE RID: 1742
		private const string RemoteRequestVersion = "3";

		// Token: 0x040006D0 RID: 1744
		private const string consentServerUrl = "https://barotraumagame.com/baromaster/";

		// Token: 0x040006D1 RID: 1745
		private const string consentServerFile = "consentserver.php";

		// Token: 0x040006D2 RID: 1746
		private static readonly HashSet<string> sentEventIdentifiers = new HashSet<string>();

		// Token: 0x040006D3 RID: 1747
		[Nullable(2)]
		private static GameAnalyticsManager.Implementation loadedImplementation;

		// Token: 0x040006D4 RID: 1748
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

		// Token: 0x0200081D RID: 2077
		[NullableContext(0)]
		public enum Consent
		{
			// Token: 0x04003CC1 RID: 15553
			Unknown,
			// Token: 0x04003CC2 RID: 15554
			Error,
			// Token: 0x04003CC3 RID: 15555
			Ask,
			// Token: 0x04003CC4 RID: 15556
			No,
			// Token: 0x04003CC5 RID: 15557
			Yes
		}

		// Token: 0x0200081E RID: 2078
		[NullableContext(0)]
		private enum Platform
		{
			// Token: 0x04003CC7 RID: 15559
			Steam,
			// Token: 0x04003CC8 RID: 15560
			EOS,
			// Token: 0x04003CC9 RID: 15561
			None
		}

		// Token: 0x0200081F RID: 2079
		[Nullable(0)]
		private class AuthTicket
		{
			// Token: 0x06006CDC RID: 27868 RVA: 0x0036038F File Offset: 0x0035E58F
			public AuthTicket(string token, GameAnalyticsManager.Platform platform)
			{
				this.Token = (token ?? string.Empty);
				this.Platform = platform;
			}

			// Token: 0x04003CCA RID: 15562
			public readonly string Token;

			// Token: 0x04003CCB RID: 15563
			public readonly GameAnalyticsManager.Platform Platform;
		}

		// Token: 0x02000820 RID: 2080
		[NullableContext(0)]
		public enum ErrorSeverity
		{
			// Token: 0x04003CCD RID: 15565
			Undefined,
			// Token: 0x04003CCE RID: 15566
			Debug,
			// Token: 0x04003CCF RID: 15567
			Info,
			// Token: 0x04003CD0 RID: 15568
			Warning,
			// Token: 0x04003CD1 RID: 15569
			Error,
			// Token: 0x04003CD2 RID: 15570
			Critical
		}

		// Token: 0x02000821 RID: 2081
		[NullableContext(0)]
		public enum ProgressionStatus
		{
			// Token: 0x04003CD4 RID: 15572
			Undefined,
			// Token: 0x04003CD5 RID: 15573
			Start,
			// Token: 0x04003CD6 RID: 15574
			Complete,
			// Token: 0x04003CD7 RID: 15575
			Fail
		}

		// Token: 0x02000822 RID: 2082
		[NullableContext(0)]
		public enum CustomDimensions01
		{
			// Token: 0x04003CD9 RID: 15577
			Vanilla,
			// Token: 0x04003CDA RID: 15578
			Modded
		}

		// Token: 0x02000823 RID: 2083
		[NullableContext(0)]
		public enum CustomDimensions02
		{
			// Token: 0x04003CDC RID: 15580
			None,
			// Token: 0x04003CDD RID: 15581
			Difficulty0to10,
			// Token: 0x04003CDE RID: 15582
			Difficulty10to20,
			// Token: 0x04003CDF RID: 15583
			Difficulty20to30,
			// Token: 0x04003CE0 RID: 15584
			Difficulty30to40,
			// Token: 0x04003CE1 RID: 15585
			Difficulty40to50,
			// Token: 0x04003CE2 RID: 15586
			Difficulty50to60,
			// Token: 0x04003CE3 RID: 15587
			Difficulty60to70,
			// Token: 0x04003CE4 RID: 15588
			Difficulty70to80,
			// Token: 0x04003CE5 RID: 15589
			Difficulty80to90,
			// Token: 0x04003CE6 RID: 15590
			Difficulty90to100
		}

		// Token: 0x02000824 RID: 2084
		[NullableContext(0)]
		public enum CustomDimensions03
		{
			// Token: 0x04003CE8 RID: 15592
			UnknownPlatform,
			// Token: 0x04003CE9 RID: 15593
			Steam,
			// Token: 0x04003CEA RID: 15594
			EGS
		}

		// Token: 0x02000825 RID: 2085
		[NullableContext(0)]
		public enum ResourceCurrency
		{
			// Token: 0x04003CEC RID: 15596
			Money
		}

		// Token: 0x02000826 RID: 2086
		[NullableContext(0)]
		public enum ResourceFlowType
		{
			// Token: 0x04003CEE RID: 15598
			Undefined,
			// Token: 0x04003CEF RID: 15599
			Source,
			// Token: 0x04003CF0 RID: 15600
			Sink
		}

		// Token: 0x02000827 RID: 2087
		[NullableContext(0)]
		public enum MoneySource
		{
			// Token: 0x04003CF2 RID: 15602
			Unknown,
			// Token: 0x04003CF3 RID: 15603
			MissionReward,
			// Token: 0x04003CF4 RID: 15604
			Store,
			// Token: 0x04003CF5 RID: 15605
			Event,
			// Token: 0x04003CF6 RID: 15606
			Ability,
			// Token: 0x04003CF7 RID: 15607
			Cheat
		}

		// Token: 0x02000828 RID: 2088
		[NullableContext(0)]
		public enum MoneySink
		{
			// Token: 0x04003CF9 RID: 15609
			Unknown,
			// Token: 0x04003CFA RID: 15610
			Store,
			// Token: 0x04003CFB RID: 15611
			Service,
			// Token: 0x04003CFC RID: 15612
			Crew,
			// Token: 0x04003CFD RID: 15613
			SubmarineUpgrade,
			// Token: 0x04003CFE RID: 15614
			SubmarineWeapon,
			// Token: 0x04003CFF RID: 15615
			SubmarinePurchase,
			// Token: 0x04003D00 RID: 15616
			SubmarineSwitch
		}

		// Token: 0x02000829 RID: 2089
		[Nullable(0)]
		private class Implementation : IDisposable
		{
			// Token: 0x06006CDD RID: 27869 RVA: 0x003603AE File Offset: 0x0035E5AE
			internal void Initialize(string gameKey, string secretKey)
			{
				this.initialize(gameKey, secretKey);
			}

			// Token: 0x06006CDE RID: 27870 RVA: 0x003603BD File Offset: 0x0035E5BD
			internal void ConfigureBuild(string config)
			{
				this.configureBuild(config);
			}

			// Token: 0x06006CDF RID: 27871 RVA: 0x003603CB File Offset: 0x0035E5CB
			internal void AddErrorEvent(GameAnalyticsManager.ErrorSeverity severity, string message)
			{
				this.addErrorEvent(severity, message);
			}

			// Token: 0x06006CE0 RID: 27872 RVA: 0x003603DA File Offset: 0x0035E5DA
			internal void AddDesignEvent(string message, [Nullable(new byte[]
			{
				2,
				1,
				1
			})] IDictionary<string, object> fields = null)
			{
				this.addDesignEvent0(message, fields);
			}

			// Token: 0x06006CE1 RID: 27873 RVA: 0x003603E9 File Offset: 0x0035E5E9
			internal void AddDesignEvent(string message, double value)
			{
				this.addDesignEvent1(message, value);
			}

			// Token: 0x06006CE2 RID: 27874 RVA: 0x003603F8 File Offset: 0x0035E5F8
			internal void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus status, string progression01)
			{
				this.addProgressionEvent01(status, progression01);
			}

			// Token: 0x06006CE3 RID: 27875 RVA: 0x00360407 File Offset: 0x0035E607
			internal void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus status, string progression01, double score)
			{
				this.addProgressionEvent01Score(status, progression01, score);
			}

			// Token: 0x06006CE4 RID: 27876 RVA: 0x00360417 File Offset: 0x0035E617
			internal void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus status, string progression01, string progression02)
			{
				this.addProgressionEvent02(status, progression01, progression02);
			}

			// Token: 0x06006CE5 RID: 27877 RVA: 0x00360427 File Offset: 0x0035E627
			internal void AddProgressionEvent(GameAnalyticsManager.ProgressionStatus status, string progression01, string progression02, string progression03)
			{
				this.addProgressionEvent03(status, progression01, progression02, progression03);
			}

			// Token: 0x06006CE6 RID: 27878 RVA: 0x00360439 File Offset: 0x0035E639
			internal void AddResourceEvent(GameAnalyticsManager.ResourceFlowType flowType, string currency, float amount, string itemType, string itemId)
			{
				this.addResourceEvent(flowType, currency, amount, itemType, itemId);
			}

			// Token: 0x06006CE7 RID: 27879 RVA: 0x0036044D File Offset: 0x0035E64D
			internal void SetCustomDimension01(string dimension01)
			{
				this.setCustomDimension01(dimension01);
			}

			// Token: 0x06006CE8 RID: 27880 RVA: 0x0036045B File Offset: 0x0035E65B
			internal void ConfigureAvailableCustomDimensions01(params GameAnalyticsManager.CustomDimensions01[] customDimensions)
			{
				this.configureAvailableCustomDimensions01((from d in customDimensions
				select d.ToString()).ToArray<string>());
			}

			// Token: 0x06006CE9 RID: 27881 RVA: 0x00360492 File Offset: 0x0035E692
			internal void SetCustomDimension02(string dimension02)
			{
				this.setCustomDimension02(dimension02);
			}

			// Token: 0x06006CEA RID: 27882 RVA: 0x003604A0 File Offset: 0x0035E6A0
			internal void ConfigureAvailableCustomDimensions02(params GameAnalyticsManager.CustomDimensions02[] customDimensions)
			{
				this.configureAvailableCustomDimensions02((from d in customDimensions
				select d.ToString()).ToArray<string>());
			}

			// Token: 0x06006CEB RID: 27883 RVA: 0x003604D7 File Offset: 0x0035E6D7
			internal void ConfigureAvailableResourceCurrencies(params GameAnalyticsManager.ResourceCurrency[] customDimensions)
			{
				this.configureAvailableResourceCurrencies((from d in customDimensions
				select d.ToString()).ToArray<string>());
			}

			// Token: 0x06006CEC RID: 27884 RVA: 0x0036050E File Offset: 0x0035E70E
			internal void ConfigureAvailableCustomDimensions03(params GameAnalyticsManager.CustomDimensions03[] customDimensions)
			{
				this.configureAvailableCustomDimensions03((from d in customDimensions
				select d.ToString()).ToArray<string>());
			}

			// Token: 0x06006CED RID: 27885 RVA: 0x00360545 File Offset: 0x0035E745
			internal void SetCustomDimension03(string dimension03)
			{
				this.setCustomDimension03(dimension03);
			}

			// Token: 0x06006CEE RID: 27886 RVA: 0x00360553 File Offset: 0x0035E753
			internal void ConfigureAvailableResourceItemTypes(params string[] resourceItemTypes)
			{
				this.configureAvailableResourceItemTypes(resourceItemTypes);
			}

			// Token: 0x06006CEF RID: 27887 RVA: 0x00360561 File Offset: 0x0035E761
			internal void SetEnabledInfoLog(bool enabled)
			{
				this.setEnabledInfoLog(enabled);
			}

			// Token: 0x06006CF0 RID: 27888 RVA: 0x0036056F File Offset: 0x0035E76F
			internal void SetEnabledVerboseLog(bool enabled)
			{
				this.setEnabledVerboseLog(enabled);
			}

			// Token: 0x06006CF1 RID: 27889 RVA: 0x00360580 File Offset: 0x0035E780
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

			// Token: 0x06006CF2 RID: 27890 RVA: 0x003605A8 File Offset: 0x0035E7A8
			private Action<T> Call<[Nullable(2)] T>(MethodInfo methodInfo)
			{
				return delegate(T arg1)
				{
					this.args1[0] = arg1;
					methodInfo.Invoke(null, this.args1);
				};
			}

			// Token: 0x06006CF3 RID: 27891 RVA: 0x003605D8 File Offset: 0x0035E7D8
			private Action<T1, T2> Call<[Nullable(2)] T1, [Nullable(2)] T2>(MethodInfo methodInfo)
			{
				return delegate(T1 arg1, T2 arg2)
				{
					this.args2[0] = arg1;
					this.args2[1] = arg2;
					methodInfo.Invoke(null, this.args2);
				};
			}

			// Token: 0x06006CF4 RID: 27892 RVA: 0x00360608 File Offset: 0x0035E808
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

			// Token: 0x06006CF5 RID: 27893 RVA: 0x00360638 File Offset: 0x0035E838
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

			// Token: 0x06006CF6 RID: 27894 RVA: 0x00360668 File Offset: 0x0035E868
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

			// Token: 0x06006CF7 RID: 27895 RVA: 0x00360695 File Offset: 0x0035E895
			private string GetAssemblyPath(string assemblyName)
			{
				return Path.Combine(new string[]
				{
					Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
					assemblyName + ".dll"
				});
			}

			// Token: 0x06006CF8 RID: 27896 RVA: 0x003606C4 File Offset: 0x0035E8C4
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

			// Token: 0x06006CF9 RID: 27897 RVA: 0x0036070C File Offset: 0x0035E90C
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

			// Token: 0x06006CFA RID: 27898 RVA: 0x00360BE8 File Offset: 0x0035EDE8
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

			// Token: 0x06006CFB RID: 27899 RVA: 0x00360C54 File Offset: 0x0035EE54
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

			// Token: 0x06006CFC RID: 27900 RVA: 0x00360C84 File Offset: 0x0035EE84
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

			// Token: 0x06006CFD RID: 27901 RVA: 0x00360CFC File Offset: 0x0035EEFC
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

			// Token: 0x04003D01 RID: 15617
			private readonly Action<string, string> initialize;

			// Token: 0x04003D02 RID: 15618
			private readonly Action<string> configureBuild;

			// Token: 0x04003D03 RID: 15619
			private readonly Action<GameAnalyticsManager.ErrorSeverity, string> addErrorEvent;

			// Token: 0x04003D04 RID: 15620
			[Nullable(new byte[]
			{
				1,
				1,
				2,
				1,
				1
			})]
			private readonly Action<string, IDictionary<string, object>> addDesignEvent0;

			// Token: 0x04003D05 RID: 15621
			private readonly Action<string, double> addDesignEvent1;

			// Token: 0x04003D06 RID: 15622
			private readonly Action<GameAnalyticsManager.ProgressionStatus, string> addProgressionEvent01;

			// Token: 0x04003D07 RID: 15623
			private readonly Action<GameAnalyticsManager.ProgressionStatus, string, double> addProgressionEvent01Score;

			// Token: 0x04003D08 RID: 15624
			private readonly Action<GameAnalyticsManager.ProgressionStatus, string, string> addProgressionEvent02;

			// Token: 0x04003D09 RID: 15625
			private readonly Action<GameAnalyticsManager.ProgressionStatus, string, string, string> addProgressionEvent03;

			// Token: 0x04003D0A RID: 15626
			private readonly Action<GameAnalyticsManager.ResourceFlowType, string, float, string, string> addResourceEvent;

			// Token: 0x04003D0B RID: 15627
			private readonly Action<string> setCustomDimension01;

			// Token: 0x04003D0C RID: 15628
			private readonly Action<string[]> configureAvailableCustomDimensions01;

			// Token: 0x04003D0D RID: 15629
			private readonly Action<string> setCustomDimension02;

			// Token: 0x04003D0E RID: 15630
			private readonly Action<string[]> configureAvailableCustomDimensions02;

			// Token: 0x04003D0F RID: 15631
			private readonly Action<string[]> configureAvailableResourceCurrencies;

			// Token: 0x04003D10 RID: 15632
			private readonly Action<string[]> configureAvailableCustomDimensions03;

			// Token: 0x04003D11 RID: 15633
			private readonly Action<string> setCustomDimension03;

			// Token: 0x04003D12 RID: 15634
			private readonly Action<string[]> configureAvailableResourceItemTypes;

			// Token: 0x04003D13 RID: 15635
			private readonly Action<bool> setEnabledInfoLog;

			// Token: 0x04003D14 RID: 15636
			private readonly Action<bool> setEnabledVerboseLog;

			// Token: 0x04003D15 RID: 15637
			private const string AssemblyName = "GameAnalytics.NetStandard";

			// Token: 0x04003D16 RID: 15638
			private const string Namespace = "GameAnalyticsSDK.Net";

			// Token: 0x04003D17 RID: 15639
			private const string MainClass = "GameAnalytics";

			// Token: 0x04003D18 RID: 15640
			private const string EnumPrefix = "EGA";

			// Token: 0x04003D19 RID: 15641
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args1 = new object[1];

			// Token: 0x04003D1A RID: 15642
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args2 = new object[2];

			// Token: 0x04003D1B RID: 15643
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args3 = new object[3];

			// Token: 0x04003D1C RID: 15644
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args4 = new object[4];

			// Token: 0x04003D1D RID: 15645
			[Nullable(new byte[]
			{
				1,
				2
			})]
			private readonly object[] args5 = new object[5];

			// Token: 0x04003D1E RID: 15646
			[Nullable(2)]
			private AssemblyLoadContext loadContext;

			// Token: 0x04003D1F RID: 15647
			[Nullable(2)]
			private Assembly assembly;

			// Token: 0x04003D20 RID: 15648
			private bool resolvingDependency;

			// Token: 0x04003D21 RID: 15649
			[Nullable(2)]
			private readonly Action onQuit;
		}

		// Token: 0x0200082A RID: 2090
		[NullableContext(0)]
		public enum DataSampleSize
		{
			// Token: 0x04003D23 RID: 15651
			Small,
			// Token: 0x04003D24 RID: 15652
			Medium,
			// Token: 0x04003D25 RID: 15653
			Large,
			// Token: 0x04003D26 RID: 15654
			Full
		}

		// Token: 0x0200082C RID: 2092
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003D2E RID: 15662
			[Nullable(0)]
			public static Action <0>__CreateConsentPrompt;
		}
	}
}
