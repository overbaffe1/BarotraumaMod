using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Barotrauma.Eos
{
	// Token: 0x0200062C RID: 1580
	[NullableContext(1)]
	[Nullable(0)]
	public static class EosSteamPrimaryLogin
	{
		// Token: 0x17001988 RID: 6536
		// (get) Token: 0x060064DD RID: 25821 RVA: 0x00342A23 File Offset: 0x00340C23
		// (set) Token: 0x060064DE RID: 25822 RVA: 0x00342A30 File Offset: 0x00340C30
		public unsafe static EosSteamPrimaryLogin.CrossplayChoice EnableCrossplay
		{
			get
			{
				return GameSettings.CurrentConfig.CrossplayChoice;
			}
			set
			{
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.CrossplayChoice = value;
				GameSettings.SetCurrentConfig(config);
				GameAnalyticsManager.AddDesignEvent("Crossplay:" + value.ToString());
				GameSettings.SaveCurrentConfig();
			}
		}

		// Token: 0x060064DF RID: 25823 RVA: 0x00342A78 File Offset: 0x00340C78
		public static void Start()
		{
			string name = "EosSteamPrimaryLogin";
			Task task = EosSteamPrimaryLogin.Initialize();
			Action<Task> onCompletion;
			if ((onCompletion = EosSteamPrimaryLogin.<>O.<0>__OnTaskComplete) == null)
			{
				onCompletion = (EosSteamPrimaryLogin.<>O.<0>__OnTaskComplete = new Action<Task>(EosSteamPrimaryLogin.OnTaskComplete));
			}
			TaskPool.Add(name, task, onCompletion);
		}

		// Token: 0x060064E0 RID: 25824 RVA: 0x00342AA8 File Offset: 0x00340CA8
		private static void OnTaskComplete(Task t)
		{
			AggregateException exception2 = t.Exception;
			Exception exception = (exception2 != null) ? exception2.GetInnermost() : null;
			if (exception != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 4);
				defaultInterpolatedStringHandler.AppendFormatted("EosSteamPrimaryLogin");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted("Initialize");
				defaultInterpolatedStringHandler.AppendLiteral(" failed with exception ");
				defaultInterpolatedStringHandler.AppendFormatted(exception.Message);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted(exception.StackTrace.CleanupStackTrace());
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			Action action;
			if (!t.TryGetResult(out action))
			{
				return;
			}
			action();
		}

		// Token: 0x060064E1 RID: 25825 RVA: 0x00342B51 File Offset: 0x00340D51
		private static void Success()
		{
			EosAccount.CloseMessageBox();
			EosAccount.RefreshSelfAccountIds(null);
			EosAccount.OnLoginSuccess();
		}

		// Token: 0x060064E2 RID: 25826 RVA: 0x00342B64 File Offset: 0x00340D64
		private static Task<Action> Initialize()
		{
			EosSteamPrimaryLogin.<Initialize>d__8 <Initialize>d__;
			<Initialize>d__.<>t__builder = AsyncTaskMethodBuilder<Action>.Create();
			<Initialize>d__.<>1__state = -1;
			<Initialize>d__.<>t__builder.Start<EosSteamPrimaryLogin.<Initialize>d__8>(ref <Initialize>d__);
			return <Initialize>d__.<>t__builder.Task;
		}

		// Token: 0x060064E3 RID: 25827 RVA: 0x00342BA0 File Offset: 0x00340DA0
		private static Task<Action> SteamAccountHasLinkedPuid(EosInterface.ProductUserId _)
		{
			EosSteamPrimaryLogin.<SteamAccountHasLinkedPuid>d__9 <SteamAccountHasLinkedPuid>d__;
			<SteamAccountHasLinkedPuid>d__.<>t__builder = AsyncTaskMethodBuilder<Action>.Create();
			<SteamAccountHasLinkedPuid>d__.<>1__state = -1;
			<SteamAccountHasLinkedPuid>d__.<>t__builder.Start<EosSteamPrimaryLogin.<SteamAccountHasLinkedPuid>d__9>(ref <SteamAccountHasLinkedPuid>d__);
			return <SteamAccountHasLinkedPuid>d__.<>t__builder.Task;
		}

		// Token: 0x060064E4 RID: 25828 RVA: 0x00342BDB File Offset: 0x00340DDB
		private static Action SteamAccountHasNoLinkedPuid()
		{
			return delegate()
			{
				Action action;
				if ((action = EosSteamPrimaryLogin.<>O.<4>__AskPlayerToEnableCrossplay) == null)
				{
					action = (EosSteamPrimaryLogin.<>O.<4>__AskPlayerToEnableCrossplay = new Action(EosSteamPrimaryLogin.AskPlayerToEnableCrossplay));
				}
				GameMain.ExecuteAfterContentFinishedLoading(action);
			};
		}

		// Token: 0x060064E5 RID: 25829 RVA: 0x00342BFC File Offset: 0x00340DFC
		private static void AskPlayerToEnableCrossplay()
		{
			LocalizedString[] options = new LocalizedString[]
			{
				TextManager.Get("EnableCrossplay"),
				TextManager.Get("DisableCrossplay")
			};
			LocalizedString introText = "\n" + LocalizedString.Join("\n\n", Enumerable.Range(0, 3).Select(delegate(int i)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
				defaultInterpolatedStringHandler.AppendLiteral("EosIntro");
				defaultInterpolatedStringHandler.AppendFormatted<int>(i);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			})) + "\n";
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("EosIntroHeader"), introText, Array.Empty<LocalizedString>(), new Vector2?(new ValueTuple<float, float>(0.8f, 0.5f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Content.ChildAnchor = Anchor.TopCenter;
			msgBox.Content.Stretch = true;
			msgBox.InnerFrame.RectTransform.ScaleBasis = ScaleBasis.Smallest;
			int? selectedRadioButton = null;
			GUILayoutGroup radioButtonLayout = new GUILayoutGroup(new RectTransform(Vector2.One, msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUIRadioButtonGroup radioButtonGroup = new GUIRadioButtonGroup();
			for (int j = 0; j < options.Length; j++)
			{
				GUITickBox radioButton = new GUITickBox(new RectTransform(Vector2.One, radioButtonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), options[j], null, "GUIRadioButton");
				radioButtonGroup.AddRadioButton(j, radioButton);
				radioButton.RectTransform.MinSize = Point.Zero;
				radioButton.RectTransform.MaxSize = new Point(int.MaxValue);
				radioButton.RectTransform.ScaleBasis = ScaleBasis.Normal;
				radioButton.RectTransform.RelativeSize = Vector2.One;
			}
			new GUIFrame(new RectTransform(new Point(0), radioButtonLayout.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				MinSize = new Point(0, GUI.IntScale(30f))
			}, null, null);
			GUIButton submitButton = new GUIButton(new RectTransform(Vector2.One, radioButtonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Submit").Fallback("Submit", true), Alignment.Center, "", null)
			{
				Enabled = false
			};
			radioButtonGroup.OnSelect = delegate(GUIRadioButtonGroup rbg, int? val)
			{
				selectedRadioButton = val;
				submitButton.Enabled = true;
			};
			msgBox.ForceLayoutRecalculation();
			float maxOptionWidth = (from o in options
			select GUIStyle.Font.MeasureString(o, false).X).Max();
			int extraWidth = (int)(GUIStyle.Font.LineHeight * 4f);
			radioButtonLayout.RectTransform.IsFixedSize = true;
			radioButtonLayout.RectTransform.NonScaledSize = new Point((int)maxOptionWidth + extraWidth, (int)(GUIStyle.Font.LineHeight * (float)options.Length * 1.5f) + submitButton.Rect.Height * 2);
			msgBox.ForceLayoutRecalculation();
			EosSteamPrimaryLogin.<AskPlayerToEnableCrossplay>g__textSizeFixHack|11_3(msgBox.Header, (int)((float)msgBox.InnerFrame.Rect.Width * 0.9f));
			EosSteamPrimaryLogin.<AskPlayerToEnableCrossplay>g__textSizeFixHack|11_3(msgBox.Text, (int)((float)msgBox.InnerFrame.Rect.Width * 0.9f));
			msgBox.ForceLayoutRecalculation();
			msgBox.InnerFrame.RectTransform.IsFixedSize = true;
			msgBox.InnerFrame.RectTransform.NonScaledSize = new Point(msgBox.InnerFrame.Rect.Width, (int)(((float)(from c in msgBox.Content.Children
			select c.Rect.Height + GUI.IntScale(5f)).Sum() + GUIStyle.Font.LineHeight) / 0.9f));
			submitButton.OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				if (selectedRadioButton != null)
				{
					int valueOrDefault = selectedRadioButton.GetValueOrDefault();
					if (valueOrDefault == 0)
					{
						EosSteamPrimaryLogin.PlayerWantsToEnableCrossplay();
						return false;
					}
					if (valueOrDefault == 1)
					{
						EosSteamPrimaryLogin.PlayerWantsToDisableCrossplay();
						return false;
					}
				}
				throw new UnreachableCodeException();
			};
			EosAccount.ReplaceMessageBox(msgBox);
		}

		// Token: 0x060064E6 RID: 25830 RVA: 0x00343048 File Offset: 0x00341248
		private static void PlayerWantsToEnableCrossplay()
		{
			EosAccount.CreateLoadingMessageBox(null);
			string name = "EnableCrossplayAndCreatePuidWithOneToken";
			Task task = EosSteamPrimaryLogin.EnableCrossplayAndCreatePuidWithOneToken();
			Action<Task> onCompletion;
			if ((onCompletion = EosSteamPrimaryLogin.<>O.<0>__OnTaskComplete) == null)
			{
				onCompletion = (EosSteamPrimaryLogin.<>O.<0>__OnTaskComplete = new Action<Task>(EosSteamPrimaryLogin.OnTaskComplete));
			}
			TaskPool.Add(name, task, onCompletion);
		}

		// Token: 0x060064E7 RID: 25831 RVA: 0x00343090 File Offset: 0x00341290
		private static void PlayerWantsToDisableCrossplay()
		{
			EosInterface.Core.CleanupAndQuit();
			Action action = EosSteamPrimaryLogin.DisableCrossplay();
			action();
		}

		// Token: 0x060064E8 RID: 25832 RVA: 0x003430B0 File Offset: 0x003412B0
		private static Task<Action> EnableCrossplayAndCreatePuidWithOneToken()
		{
			EosSteamPrimaryLogin.<EnableCrossplayAndCreatePuidWithOneToken>d__14 <EnableCrossplayAndCreatePuidWithOneToken>d__;
			<EnableCrossplayAndCreatePuidWithOneToken>d__.<>t__builder = AsyncTaskMethodBuilder<Action>.Create();
			<EnableCrossplayAndCreatePuidWithOneToken>d__.<>1__state = -1;
			<EnableCrossplayAndCreatePuidWithOneToken>d__.<>t__builder.Start<EosSteamPrimaryLogin.<EnableCrossplayAndCreatePuidWithOneToken>d__14>(ref <EnableCrossplayAndCreatePuidWithOneToken>d__);
			return <EnableCrossplayAndCreatePuidWithOneToken>d__.<>t__builder.Task;
		}

		// Token: 0x060064E9 RID: 25833 RVA: 0x003430EC File Offset: 0x003412EC
		private static LocalizedString GetErrorMessage(EosInterface.Core.InitError errorCode)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
			defaultInterpolatedStringHandler.AppendLiteral("EosInterface.Core.InitError.");
			defaultInterpolatedStringHandler.AppendFormatted<EosInterface.Core.InitError>(errorCode);
			LocalizedString localizedString = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(55, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("Failed to initialize Epic Online Services (error code ");
			defaultInterpolatedStringHandler2.AppendFormatted<EosInterface.Core.InitError>(errorCode);
			defaultInterpolatedStringHandler2.AppendLiteral(")");
			return localizedString.Fallback(defaultInterpolatedStringHandler2.ToStringAndClear(), true);
		}

		// Token: 0x060064EA RID: 25834 RVA: 0x0034315F File Offset: 0x0034135F
		private static Action DisableCrossplay()
		{
			EosSteamPrimaryLogin.EnableCrossplay = EosSteamPrimaryLogin.CrossplayChoice.Disabled;
			Action result;
			if ((result = EosSteamPrimaryLogin.<>O.<1>__Success) == null)
			{
				result = (EosSteamPrimaryLogin.<>O.<1>__Success = new Action(EosSteamPrimaryLogin.Success));
			}
			return result;
		}

		// Token: 0x060064EB RID: 25835 RVA: 0x00343184 File Offset: 0x00341384
		public static void HandleCrossplayChoiceChange(EosSteamPrimaryLogin.CrossplayChoice newChoice)
		{
			if (StoreIntegration.CurrentStore != StoreIntegration.Store.Steam)
			{
				return;
			}
			if (GameSettings.CurrentConfig.CrossplayChoice == newChoice)
			{
				return;
			}
			if (newChoice != EosSteamPrimaryLogin.CrossplayChoice.Enabled)
			{
				if (newChoice == EosSteamPrimaryLogin.CrossplayChoice.Disabled)
				{
					EosInterface.Core.CleanupAndQuit();
					return;
				}
			}
			else
			{
				if (EosInterface.Core.CurrentStatus == EosInterface.Core.Status.ShutDown)
				{
					GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("EosAllowCrossplay"), TextManager.Get("RestartRequiredGeneric"), new LocalizedString[]
					{
						TextManager.Get("ok")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
					{
						DrawOnTop = true
					};
					msgBox.Buttons[0].OnClicked = delegate(GUIButton _, object _)
					{
						msgBox.Close();
						return true;
					};
					return;
				}
				EosSteamPrimaryLogin.PlayerWantsToEnableCrossplay();
			}
		}

		// Token: 0x060064EC RID: 25836 RVA: 0x00343258 File Offset: 0x00341458
		[CompilerGenerated]
		internal static void <Initialize>g__retry|8_0()
		{
			EosSteamPrimaryLogin.Start();
		}

		// Token: 0x060064ED RID: 25837 RVA: 0x0034325F File Offset: 0x0034145F
		[CompilerGenerated]
		internal static void <Initialize>g__cancel|8_1()
		{
			EosInterface.Core.CleanupAndQuit();
		}

		// Token: 0x060064EE RID: 25838 RVA: 0x00343268 File Offset: 0x00341468
		[CompilerGenerated]
		internal static void <AskPlayerToEnableCrossplay>g__textSizeFixHack|11_3(GUITextBlock textBlock, int width)
		{
			textBlock.RectTransform.IsFixedSize = true;
			textBlock.RectTransform.MinSize = Point.Zero;
			textBlock.RectTransform.MaxSize = new Point(int.MaxValue);
			textBlock.RectTransform.NonScaledSize = new Point(width, 0);
			textBlock.CalculateHeightFromText(0, false);
		}

		// Token: 0x060064EF RID: 25839 RVA: 0x003432C0 File Offset: 0x003414C0
		[CompilerGenerated]
		internal static void <EnableCrossplayAndCreatePuidWithOneToken>g__retry|14_0()
		{
			EosSteamPrimaryLogin.PlayerWantsToEnableCrossplay();
		}

		// Token: 0x060064F0 RID: 25840 RVA: 0x003432C7 File Offset: 0x003414C7
		[CompilerGenerated]
		internal static void <EnableCrossplayAndCreatePuidWithOneToken>g__cancel|14_1()
		{
			EosInterface.Core.CleanupAndQuit();
		}

		// Token: 0x04003460 RID: 13408
		public static bool IsNewEosPlayer;

		// Token: 0x020014BC RID: 5308
		[NullableContext(0)]
		public enum CrossplayChoice
		{
			// Token: 0x040066D7 RID: 26327
			Unknown,
			// Token: 0x040066D8 RID: 26328
			Enabled,
			// Token: 0x040066D9 RID: 26329
			Disabled
		}

		// Token: 0x020014BD RID: 5309
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040066DA RID: 26330
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<Task> <0>__OnTaskComplete;

			// Token: 0x040066DB RID: 26331
			[Nullable(0)]
			public static Action <1>__Success;

			// Token: 0x040066DC RID: 26332
			[Nullable(0)]
			public static Action <2>__retry;

			// Token: 0x040066DD RID: 26333
			[Nullable(0)]
			public static Action <3>__cancel;

			// Token: 0x040066DE RID: 26334
			[Nullable(0)]
			public static Action <4>__AskPlayerToEnableCrossplay;

			// Token: 0x040066DF RID: 26335
			[Nullable(0)]
			public static Action <5>__cancel;
		}
	}
}
