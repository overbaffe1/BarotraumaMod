using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Eos
{
	// Token: 0x02000629 RID: 1577
	[NullableContext(1)]
	[Nullable(0)]
	internal static class EosAccount
	{
		// Token: 0x17001987 RID: 6535
		// (get) Token: 0x060064C9 RID: 25801 RVA: 0x0034249F File Offset: 0x0034069F
		// (set) Token: 0x060064CA RID: 25802 RVA: 0x003424A6 File Offset: 0x003406A6
		public static ImmutableHashSet<AccountId> SelfAccountIds { get; private set; } = ImmutableHashSet<AccountId>.Empty;

		// Token: 0x060064CB RID: 25803 RVA: 0x003424B0 File Offset: 0x003406B0
		[NullableContext(2)]
		public static void RefreshSelfAccountIds(Action onRefreshComplete = null)
		{
			EosAccount.<>c__DisplayClass6_0 CS$<>8__locals1 = new EosAccount.<>c__DisplayClass6_0();
			CS$<>8__locals1.onRefreshComplete = onRefreshComplete;
			EosAccount.SelfAccountIds = ImmutableHashSet<AccountId>.Empty;
			ImmutableArray<EosInterface.ProductUserId> selfPuids = EosInterface.IdQueries.GetLoggedInPuids();
			if (selfPuids.Length != 0)
			{
				CS$<>8__locals1.collectedIds = new Option<ImmutableArray<AccountId>>[selfPuids.Length];
				for (int i = 0; i < selfPuids.Length; i++)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(35, 1);
					defaultInterpolatedStringHandler.AppendLiteral("SelfPlayerRowWithExternalAccountIds");
					defaultInterpolatedStringHandler.AppendFormatted<int>(i);
					TaskPool.Add(defaultInterpolatedStringHandler.ToStringAndClear(), EosInterface.IdQueries.GetSelfExternalAccountIds(selfPuids[i]), CS$<>8__locals1.<RefreshSelfAccountIds>g__taskDoneHandler|0(i));
				}
				return;
			}
			Action onRefreshComplete2 = CS$<>8__locals1.onRefreshComplete;
			if (onRefreshComplete2 == null)
			{
				return;
			}
			onRefreshComplete2();
		}

		// Token: 0x060064CC RID: 25804 RVA: 0x00342557 File Offset: 0x00340757
		[NullableContext(2)]
		public static void ReplaceMessageBox(GUIMessageBox newMessageBox)
		{
			GUIMessageBox guimessageBox = EosAccount.messageBox;
			if (guimessageBox != null)
			{
				guimessageBox.Close();
			}
			EosAccount.messageBox = newMessageBox;
		}

		// Token: 0x060064CD RID: 25805 RVA: 0x0034256F File Offset: 0x0034076F
		public static void CloseMessageBox()
		{
			EosAccount.ReplaceMessageBox(null);
		}

		// Token: 0x060064CE RID: 25806 RVA: 0x00342578 File Offset: 0x00340778
		public static GUIMessageBox CreateLoadingMessageBox([TupleElementNames(new string[]
		{
			"CanCancel",
			"Cancel"
		})] [Nullable(new byte[]
		{
			0,
			1,
			1
		})] ValueTuple<Func<bool>, Action>? actions = null)
		{
			EosAccount.<>c__DisplayClass10_0 CS$<>8__locals1 = new EosAccount.<>c__DisplayClass10_0();
			CS$<>8__locals1.actions = actions;
			GUIMessageBox guimessageBox = EosAccount.messageBox;
			Vector2 relativeSize = (guimessageBox != null) ? guimessageBox.InnerFrame.RectTransform.RelativeSize : new ValueTuple<float, float>(0.35f, 0.3f);
			EosAccount.<>c__DisplayClass10_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString headerText = LocalizedString.EmptyString;
			RichString text = LocalizedString.EmptyString;
			Vector2? relativeSize2 = new Vector2?(relativeSize);
			LocalizedString[] buttons;
			if (CS$<>8__locals1.actions == null)
			{
				buttons = Array.Empty<LocalizedString>();
			}
			else
			{
				(buttons = new LocalizedString[1])[0] = TextManager.Get("Cancel");
			}
			CS$<>8__locals2.newMessageBox = new GUIMessageBox(headerText, text, buttons, relativeSize2, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			if (CS$<>8__locals1.actions != null)
			{
				CS$<>8__locals1.newMessageBox.Buttons[0].Visible = false;
				CS$<>8__locals1.newMessageBox.Buttons[0].OnClicked = delegate(GUIButton _, object _)
				{
					CS$<>8__locals1.actions.Value.Item2();
					return false;
				};
				new GUICustomComponent(new RectTransform(Vector2.Zero, CS$<>8__locals1.newMessageBox.InnerFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float _, GUICustomComponent _)
				{
					bool canCancel = CS$<>8__locals1.actions.Value.Item1();
					CS$<>8__locals1.newMessageBox.Buttons[0].Visible = canCancel;
					CS$<>8__locals1.newMessageBox.Buttons[0].Enabled = canCancel;
				});
			}
			new GUICustomComponent(new RectTransform(Vector2.One * 0.25f, CS$<>8__locals1.newMessageBox.InnerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Smallest), delegate(SpriteBatch sb, GUICustomComponent component)
			{
				GUIStyle.GenericThrobber.Draw(sb, (int)(Timing.TotalTime * 20.0) % GUIStyle.GenericThrobber.FrameCount, component.Rect.Center.ToVector2(), Color.White, GUIStyle.GenericThrobber.FrameSize.ToVector2() * 0.5f, 0f, component.Rect.Size.ToVector2() / GUIStyle.GenericThrobber.FrameSize.ToVector2(), SpriteEffects.None, null);
			}, null);
			EosAccount.ReplaceMessageBox(CS$<>8__locals1.newMessageBox);
			return CS$<>8__locals1.newMessageBox;
		}

		// Token: 0x060064CF RID: 25807 RVA: 0x00342738 File Offset: 0x00340938
		public static Action RetryAction(LocalizedString intro, LocalizedString reason, Action retryAction, Action cancelAction)
		{
			Action <>9__1;
			return delegate()
			{
				Action action;
				if ((action = <>9__1) == null)
				{
					action = (<>9__1 = delegate()
					{
						EosAccount.AskRetry(intro, reason, retryAction, cancelAction);
					});
				}
				GameMain.ExecuteAfterContentFinishedLoading(action);
			};
		}

		// Token: 0x060064D0 RID: 25808 RVA: 0x00342774 File Offset: 0x00340974
		private static void AskRetry(LocalizedString intro, LocalizedString failureReason, Action retryAction, Action cancelAction)
		{
			LocalizedString[] options = new LocalizedString[]
			{
				TextManager.Get("Retry"),
				TextManager.Get("Cancel")
			};
			LocalizedString askHowToProceed = TextManager.Get("AskHowToProceed");
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("EosIntroHeader"), intro + "\n\n" + failureReason + "\n\n" + askHowToProceed, options, new Vector2?(new ValueTuple<float, float>(0.4f, 0.4f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				retryAction();
				EosAccount.CloseMessageBox();
				return false;
			};
			msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				cancelAction();
				EosAccount.CloseMessageBox();
				return false;
			};
			EosAccount.ReplaceMessageBox(msgBox);
		}

		// Token: 0x060064D1 RID: 25809 RVA: 0x0034287C File Offset: 0x00340A7C
		public static void LoginPlatformSpecific()
		{
			string exchangeCode;
			if (GameMain.Instance.EgsExchangeCode.TryUnwrap(out exchangeCode))
			{
				EosAccount.LoginEpic(exchangeCode);
				return;
			}
			if (SteamManager.IsInitialized)
			{
				EosAccount.LoginSteam();
			}
		}

		// Token: 0x060064D2 RID: 25810 RVA: 0x003428AF File Offset: 0x00340AAF
		private static void LoginSteam()
		{
			EosSteamPrimaryLogin.Start();
		}

		// Token: 0x060064D3 RID: 25811 RVA: 0x003428B6 File Offset: 0x00340AB6
		private static void LoginEpic(string exchangeCode)
		{
			EosEpicPrimaryLogin.Start(exchangeCode);
		}

		// Token: 0x060064D4 RID: 25812 RVA: 0x003428C0 File Offset: 0x00340AC0
		public static void OnLoginSuccess()
		{
			EosAccount.isLoggedIn = true;
			Action action;
			while (EosAccount.postLoginActions.TryDequeue(out action))
			{
				action();
			}
		}

		// Token: 0x060064D5 RID: 25813 RVA: 0x003428E9 File Offset: 0x00340AE9
		public static void ExecuteAfterLogin(Action action)
		{
			if (EosAccount.isLoggedIn)
			{
				action();
				return;
			}
			EosAccount.postLoginActions.Enqueue(action);
		}

		// Token: 0x0400345D RID: 13405
		private static readonly Queue<Action> postLoginActions = new Queue<Action>();

		// Token: 0x0400345E RID: 13406
		private static bool isLoggedIn;

		// Token: 0x0400345F RID: 13407
		[Nullable(2)]
		private static GUIMessageBox messageBox;
	}
}
