using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000045 RID: 69
	internal class ConversationAction : EventAction
	{
		// Token: 0x170002C9 RID: 713
		// (get) Token: 0x06000A17 RID: 2583 RVA: 0x00060031 File Offset: 0x0005E231
		public static bool IsDialogOpen
		{
			get
			{
				return GUIMessageBox.MessageBoxes.Any(delegate(GUIComponent mb)
				{
					if (!(mb.UserData as string == "ConversationAction"))
					{
						Pair<string, ushort> pair = mb.UserData as Pair<string, ushort>;
						return pair != null && pair.First == "ConversationAction";
					}
					return true;
				});
			}
		}

		// Token: 0x170002CA RID: 714
		// (get) Token: 0x06000A18 RID: 2584 RVA: 0x0006005C File Offset: 0x0005E25C
		public static bool FadeScreenToBlack
		{
			get
			{
				return ConversationAction.IsDialogOpen && ConversationAction.shouldFadeToBlack;
			}
		}

		// Token: 0x06000A19 RID: 2585 RVA: 0x0006006C File Offset: 0x0005E26C
		private bool IsBlockedByAnotherConversation(IEnumerable<Entity> _, float duration)
		{
			return ConversationAction.lastActiveAction != null && !ConversationAction.lastActiveAction.ParentEvent.IsFinished && ConversationAction.lastActiveAction.ParentEvent != this.ParentEvent && Timing.TotalTime < ConversationAction.lastActiveAction.lastActiveTime + (double)duration;
		}

		// Token: 0x06000A1A RID: 2586 RVA: 0x000600BC File Offset: 0x0005E2BC
		public static void CreateDialog(LocalizedString text, Character speaker, IEnumerable<string> options, int[] closingOptions, string eventSprite, ushort actionId, bool fadeToBlack, ConversationAction.DialogTypes dialogType, bool continueConversation = false)
		{
			ushort? actionId2 = new ushort?(actionId);
			ConversationAction.CreateDialog(text, speaker, options, closingOptions, eventSprite, null, actionId2, fadeToBlack, dialogType, continueConversation);
		}

		// Token: 0x06000A1B RID: 2587 RVA: 0x000600E8 File Offset: 0x0005E2E8
		private static void CreateDialog(LocalizedString text, Character speaker, IEnumerable<string> options, int[] closingOptions, string spriteIdentifier = null, ConversationAction actionInstance = null, ushort? actionId = null, bool fadeToBlack = false, ConversationAction.DialogTypes dialogType = ConversationAction.DialogTypes.Regular, bool continueConversation = false)
		{
			ConversationAction.<>c__DisplayClass10_0 CS$<>8__locals1 = new ConversationAction.<>c__DisplayClass10_0();
			CS$<>8__locals1.actionInstance = actionInstance;
			CS$<>8__locals1.actionId = actionId;
			CS$<>8__locals1.options = options;
			CS$<>8__locals1.continueConversation = continueConversation;
			CS$<>8__locals1.closingOptions = closingOptions;
			if (GUI.InputBlockingMenuOpen)
			{
				if (CS$<>8__locals1.actionId != null)
				{
					ConversationAction.SendIgnore(CS$<>8__locals1.actionId.Value);
				}
				return;
			}
			ConversationAction.shouldFadeToBlack = fadeToBlack;
			Sprite eventSprite = EventSet.GetEventSprite(spriteIdentifier);
			if (ConversationAction.lastMessageBox != null && !ConversationAction.lastMessageBox.Closed && GUIMessageBox.MessageBoxes.Contains(ConversationAction.lastMessageBox))
			{
				if (eventSprite == null || ConversationAction.lastMessageBox.BackgroundIcon != null)
				{
					if (CS$<>8__locals1.actionId != null)
					{
						Pair<string, ushort> userData = ConversationAction.lastMessageBox.UserData as Pair<string, ushort>;
						if (userData != null)
						{
							int second = (int)userData.Second;
							ushort? actionId2 = CS$<>8__locals1.actionId;
							int? num = (actionId2 != null) ? new int?((int)actionId2.GetValueOrDefault()) : null;
							if (second == num.GetValueOrDefault() & num != null)
							{
								return;
							}
							ConversationAction.lastMessageBox.UserData = new Pair<string, ushort>("ConversationAction", CS$<>8__locals1.actionId.Value);
						}
					}
					GUIListBox conversationList = ConversationAction.lastMessageBox.FindChild("conversationlist", true) as GUIListBox;
					ConversationAction.DisableButtons(conversationList.Content.GetAllChildren<GUIButton>(), null);
					GUILayoutGroup lastElement = conversationList.Content.Children.LastOrDefault<GUIComponent>() as GUILayoutGroup;
					if (lastElement != null)
					{
						GUITextBlock textLayout = lastElement.FindChild("text", true) as GUITextBlock;
						if (textLayout != null)
						{
							textLayout.OverrideTextColor(Color.DarkGray * 0.8f);
						}
					}
					float prevSize = conversationList.TotalSize;
					List<GUIButton> extraButtons = ConversationAction.CreateConversation(conversationList, text, speaker, CS$<>8__locals1.options, string.IsNullOrWhiteSpace(spriteIdentifier));
					CS$<>8__locals1.<CreateDialog>g__AssignActionsToButtons|2(extraButtons, ConversationAction.lastMessageBox);
					ConversationAction.<CreateDialog>g__RecalculateLastMessage|10_1(conversationList, true);
					conversationList.BarScroll = (prevSize - (float)conversationList.Content.Rect.Height) / (conversationList.TotalSize - (float)conversationList.Content.Rect.Height);
					conversationList.ScrollToEnd(0.5f);
					ConversationAction.lastMessageBox.SetBackgroundIcon(eventSprite);
					CS$<>8__locals1.<CreateDialog>g__MarkMessageBoxAsLastAction|0(ConversationAction.lastMessageBox);
					return;
				}
				ConversationAction.lastMessageBox.Close();
			}
			Vector2 vector;
			Point point;
			ConversationAction.GetSizes(dialogType).Deconstruct(out vector, out point);
			Vector2 relative = vector;
			Point min = point;
			CS$<>8__locals1.messageBox = new GUIMessageBox(string.Empty, string.Empty, Array.Empty<LocalizedString>(), new Vector2?(relative), new Point?(min), Alignment.TopLeft, GUIMessageBox.Type.InGame, "", null, "", EventSet.GetEventSprite(spriteIdentifier), null, false)
			{
				UserData = "ConversationAction"
			};
			GUIMessageBox messageBox = CS$<>8__locals1.messageBox;
			messageBox.OnAddedToGUIUpdateList = (Action<GUIComponent>)Delegate.Combine(messageBox.OnAddedToGUIUpdateList, new Action<GUIComponent>(delegate(GUIComponent component)
			{
				if (!(Screen.Selected is GameScreen))
				{
					CS$<>8__locals1.messageBox.Close();
				}
			}));
			ConversationAction.lastMessageBox = CS$<>8__locals1.messageBox;
			CS$<>8__locals1.messageBox.InnerFrame.ClearChildren();
			CS$<>8__locals1.messageBox.AutoClose = false;
			GUIStyle.Apply(CS$<>8__locals1.messageBox.InnerFrame, "DialogBox", null);
			CS$<>8__locals1.<CreateDialog>g__MarkMessageBoxAsLastAction|0(CS$<>8__locals1.messageBox);
			int padding = GUI.IntScale(16f);
			GUIListBox listBox = new GUIListBox(new RectTransform(CS$<>8__locals1.messageBox.InnerFrame.Rect.Size - new Point(padding * 2), CS$<>8__locals1.messageBox.InnerFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), false, null, null, true, false)
			{
				KeepSpaceForScrollBar = true,
				HoverCursor = CursorState.Default,
				UserData = "conversationlist"
			};
			List<GUIButton> buttons = ConversationAction.CreateConversation(listBox, text, speaker, CS$<>8__locals1.options, string.IsNullOrWhiteSpace(spriteIdentifier));
			CS$<>8__locals1.<CreateDialog>g__AssignActionsToButtons|2(buttons, CS$<>8__locals1.messageBox);
			ConversationAction.<CreateDialog>g__RecalculateLastMessage|10_1(listBox, false);
			CS$<>8__locals1.messageBox.InnerFrame.RectTransform.MinSize = new Point(0, Math.Max(listBox.RectTransform.MinSize.Y + padding * 2, (int)(100f * GUI.yScale)));
			GUIFrame shadow = new GUIFrame(new RectTransform(CS$<>8__locals1.messageBox.InnerFrame.Rect.Size + new Point(padding * 4), CS$<>8__locals1.messageBox.InnerFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), "OuterGlow", null)
			{
				Color = Color.Black * 0.7f
			};
			shadow.SetAsFirstChild();
		}

		// Token: 0x06000A1C RID: 2588 RVA: 0x00060570 File Offset: 0x0005E770
		public static void SelectOption(ushort actionId, int option)
		{
			Pair<string, ushort> userData = ConversationAction.lastMessageBox.UserData as Pair<string, ushort>;
			if (userData != null)
			{
				if (userData.Second != actionId)
				{
					return;
				}
				GUIListBox conversationList = ConversationAction.lastMessageBox.FindChild("conversationlist", true) as GUIListBox;
				ConversationAction.DisableButtons(conversationList.Content.GetAllChildren<GUIButton>(), delegate(GUIButton btn)
				{
					object userData2 = btn.UserData;
					if (userData2 is int)
					{
						int i = (int)userData2;
						return i == option;
					}
					return false;
				});
			}
		}

		// Token: 0x06000A1D RID: 2589 RVA: 0x000605DC File Offset: 0x0005E7DC
		private static Tuple<Vector2, Point> GetSizes(ConversationAction.DialogTypes dialogTypes)
		{
			Tuple<Vector2, Point> result;
			if (dialogTypes == ConversationAction.DialogTypes.Regular)
			{
				result = Tuple.Create<Vector2, Point>(new Vector2(0.3f, 0.2f), new Point(512, 256));
			}
			else
			{
				result = Tuple.Create<Vector2, Point>(new Vector2(0.3f, 0.15f), new Point(512, 128));
			}
			return result;
		}

		// Token: 0x06000A1E RID: 2590 RVA: 0x00060638 File Offset: 0x0005E838
		private static List<GUIButton> CreateConversation(GUIListBox parentBox, LocalizedString text, Character speaker, IEnumerable<string> options, bool drawChathead = true)
		{
			GUILayoutGroup content = new GUILayoutGroup(new RectTransform(Vector2.One, parentBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				CanBeFocused = true,
				AlwaysOverrideCursor = true
			};
			LocalizedString translatedText = text.Replace("\\n", "\n", StringComparison.Ordinal);
			Character speaker2 = speaker;
			if (((speaker2 != null) ? speaker2.DisplayName : null) != null)
			{
				translatedText = translatedText.Replace("[speakername]", speaker.DisplayName, StringComparison.Ordinal);
			}
			translatedText = TextManager.ParseInputTypes(translatedText, false).Fallback(text, true);
			Character speaker3 = speaker;
			if (((speaker3 != null) ? speaker3.Info : null) != null && drawChathead)
			{
				int chatHeadWidth = (int)((float)content.RectTransform.Rect.Width * 0.15f);
				new GUICustomComponent(new RectTransform(new Point(chatHeadWidth, chatHeadWidth), content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true), delegate(SpriteBatch sb, GUICustomComponent customComponent)
				{
					speaker.Info.DrawIcon(sb, customComponent.Rect.Center.ToVector2(), customComponent.Rect.Size.ToVector2(), false);
				}, null);
			}
			GUILayoutGroup textContent = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			GUITextBlock textBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), textContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), RichString.Rich(translatedText, null), null, null, Alignment.Left, true, "", null)
			{
				AlwaysOverrideCursor = true,
				UserData = "text"
			};
			List<GUIButton> buttons = new List<GUIButton>();
			if (options.Any<string>())
			{
				foreach (string option in options)
				{
					GUIButton btn = new GUIButton(new RectTransform(new Vector2(0.9f, 0.01f), textContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(option).Fallback(option, true), Alignment.Center, "ListBoxElement", null);
					btn.TextBlock.TextAlignment = Alignment.CenterLeft;
					btn.TextColor = (btn.HoverTextColor = GUIStyle.Green);
					btn.TextBlock.Wrap = true;
					buttons.Add(btn);
				}
			}
			content.Recalculate();
			textContent.Recalculate();
			textBlock.CalculateHeightFromText(0, false);
			textBlock.RectTransform.MinSize = new Point(0, textBlock.Rect.Height);
			foreach (GUIButton btn2 in buttons)
			{
				btn2.TextBlock.SetTextPos();
				btn2.TextBlock.CalculateHeightFromText(0, false);
				btn2.RectTransform.MinSize = new Point(0, (int)((float)btn2.TextBlock.Rect.Height * 1.2f));
			}
			textContent.RectTransform.MinSize = new Point(0, textContent.Children.Sum((GUIComponent c) => c.Rect.Height + textContent.AbsoluteSpacing) + GUI.IntScale(16f));
			content.RectTransform.MinSize = textContent.RectTransform.MinSize;
			textBlock.CalculateHeightFromText(0, false);
			textBlock.TextAlignment = Alignment.TopLeft;
			return buttons;
		}

		// Token: 0x06000A1F RID: 2591 RVA: 0x00060A50 File Offset: 0x0005EC50
		private static void DisableButtons(IEnumerable<GUIButton> buttons, GUIButton selectedButton)
		{
			ConversationAction.DisableButtons(buttons, (GUIButton btn) => btn == selectedButton);
		}

		// Token: 0x06000A20 RID: 2592 RVA: 0x00060A7C File Offset: 0x0005EC7C
		private static void DisableButtons(IEnumerable<GUIButton> buttons, Func<GUIButton, bool> isSelectedButton)
		{
			foreach (GUIButton btn in buttons)
			{
				if (btn.CanBeFocused)
				{
					btn.CanBeFocused = false;
					if (isSelectedButton(btn))
					{
						btn.Selected = true;
					}
					else
					{
						btn.TextBlock.OverrideTextColor(Color.DarkGray * 0.8f);
					}
				}
			}
		}

		// Token: 0x06000A21 RID: 2593 RVA: 0x00060AF8 File Offset: 0x0005ECF8
		private static void SendResponse(ushort actionId, int selectedOption)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(12);
			outmsg.WriteUInt16(actionId);
			outmsg.WriteByte((byte)selectedOption);
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(outmsg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x06000A22 RID: 2594 RVA: 0x00060B40 File Offset: 0x0005ED40
		private static void SendIgnore(ushort actionId)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(12);
			outmsg.WriteUInt16(actionId);
			outmsg.WriteByte(byte.MaxValue);
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(outmsg, DeliveryMethod.Reliable, true);
		}

		// Token: 0x170002CB RID: 715
		// (get) Token: 0x06000A23 RID: 2595 RVA: 0x00060B89 File Offset: 0x0005ED89
		// (set) Token: 0x06000A24 RID: 2596 RVA: 0x00060B91 File Offset: 0x0005ED91
		[Serialize("", IsPropertySaveable.Yes, "The text to display in the prompt. Can be the text as-is, or a tag referring to a line in a text file.", "", false)]
		public string Text { get; set; }

		// Token: 0x170002CC RID: 716
		// (get) Token: 0x06000A25 RID: 2597 RVA: 0x00060B9A File Offset: 0x0005ED9A
		// (set) Token: 0x06000A26 RID: 2598 RVA: 0x00060BA2 File Offset: 0x0005EDA2
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the speaker will send the Text in chat, or if ForceSayText is not empty, will send that instead. Note: requires a valid SpeakerTag to be defined.", "", false)]
		public bool ForceSay { get; set; }

		// Token: 0x170002CD RID: 717
		// (get) Token: 0x06000A27 RID: 2599 RVA: 0x00060BAB File Offset: 0x0005EDAB
		// (set) Token: 0x06000A28 RID: 2600 RVA: 0x00060BB3 File Offset: 0x0005EDB3
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the message sent in chat by the speaker will be sent in radio chat instead.", "", false)]
		public bool ForceSayInRadio { get; set; }

		// Token: 0x170002CE RID: 718
		// (get) Token: 0x06000A29 RID: 2601 RVA: 0x00060BBC File Offset: 0x0005EDBC
		// (set) Token: 0x06000A2A RID: 2602 RVA: 0x00060BC4 File Offset: 0x0005EDC4
		[Serialize("", IsPropertySaveable.Yes, "Message sent in chat by the speaker, if empty, Text is used instead.", "", false)]
		public string ForceSayText { get; set; }

		// Token: 0x170002CF RID: 719
		// (get) Token: 0x06000A2B RID: 2603 RVA: 0x00060BCD File Offset: 0x0005EDCD
		// (set) Token: 0x06000A2C RID: 2604 RVA: 0x00060BD5 File Offset: 0x0005EDD5
		[Serialize(true, IsPropertySaveable.Yes, "Should the chat message be stripped of any quotation mark characters?", "", false)]
		public bool ForceSayRemoveQuotes { get; set; }

		// Token: 0x170002D0 RID: 720
		// (get) Token: 0x06000A2D RID: 2605 RVA: 0x00060BDE File Offset: 0x0005EDDE
		// (set) Token: 0x06000A2E RID: 2606 RVA: 0x00060BE6 File Offset: 0x0005EDE6
		[Serialize("", IsPropertySaveable.Yes, "Tag of the character who's speaking. Makes a speech bubble icon appear above the character to indicate you can speak with them, and stops the character in place when the conversation triggers. Also allows the conversation to be interrupted if the speaker dies or becomes incapacitated mid-conversation.", "", false)]
		public Identifier SpeakerTag { get; set; }

		// Token: 0x170002D1 RID: 721
		// (get) Token: 0x06000A2F RID: 2607 RVA: 0x00060BEF File Offset: 0x0005EDEF
		// (set) Token: 0x06000A30 RID: 2608 RVA: 0x00060BF7 File Offset: 0x0005EDF7
		[Serialize("", IsPropertySaveable.Yes, "Tag of the player the conversation is shown to. If empty, the conversation is shown to everyone. If SpeakerTag is defined, the conversation is always only shown to the player who interacts with the speaker.", "", false)]
		public Identifier TargetTag { get; set; }

		// Token: 0x170002D2 RID: 722
		// (get) Token: 0x06000A31 RID: 2609 RVA: 0x00060C00 File Offset: 0x0005EE00
		// (set) Token: 0x06000A32 RID: 2610 RVA: 0x00060C08 File Offset: 0x0005EE08
		[Serialize(true, IsPropertySaveable.Yes, "Should someone interact with the speaker for the conversation to trigger?", "", false)]
		public bool WaitForInteraction { get; set; }

		// Token: 0x170002D3 RID: 723
		// (get) Token: 0x06000A33 RID: 2611 RVA: 0x00060C11 File Offset: 0x0005EE11
		// (set) Token: 0x06000A34 RID: 2612 RVA: 0x00060C19 File Offset: 0x0005EE19
		[Serialize("", IsPropertySaveable.Yes, "Tag to assign to whoever invokes the conversation.", "", false)]
		public Identifier InvokerTag { get; set; }

		// Token: 0x170002D4 RID: 724
		// (get) Token: 0x06000A35 RID: 2613 RVA: 0x00060C22 File Offset: 0x0005EE22
		// (set) Token: 0x06000A36 RID: 2614 RVA: 0x00060C2A File Offset: 0x0005EE2A
		[Serialize(false, IsPropertySaveable.Yes, "Should the screen fade to black when the conversation is active?", "", false)]
		public bool FadeToBlack { get; set; }

		// Token: 0x170002D5 RID: 725
		// (get) Token: 0x06000A37 RID: 2615 RVA: 0x00060C33 File Offset: 0x0005EE33
		// (set) Token: 0x06000A38 RID: 2616 RVA: 0x00060C3B File Offset: 0x0005EE3B
		[Serialize(true, IsPropertySaveable.Yes, "Should the event end if the conversations is interrupted (e.g. if the speaker dies or falls unconscious mid-conversation). Defaults to true.", "", false)]
		public bool EndEventIfInterrupted { get; set; }

		// Token: 0x170002D6 RID: 726
		// (get) Token: 0x06000A39 RID: 2617 RVA: 0x00060C44 File Offset: 0x0005EE44
		// (set) Token: 0x06000A3A RID: 2618 RVA: 0x00060C4C File Offset: 0x0005EE4C
		[Serialize("", IsPropertySaveable.Yes, "Identifier of an event sprite to display in the corner of the conversation prompt.", "", false)]
		public string EventSprite { get; set; }

		// Token: 0x170002D7 RID: 727
		// (get) Token: 0x06000A3B RID: 2619 RVA: 0x00060C55 File Offset: 0x0005EE55
		// (set) Token: 0x06000A3C RID: 2620 RVA: 0x00060C5D File Offset: 0x0005EE5D
		[Serialize(ConversationAction.DialogTypes.Regular, IsPropertySaveable.Yes, "Type of the dialog prompt.", "", false)]
		public ConversationAction.DialogTypes DialogType { get; set; }

		// Token: 0x170002D8 RID: 728
		// (get) Token: 0x06000A3D RID: 2621 RVA: 0x00060C66 File Offset: 0x0005EE66
		// (set) Token: 0x06000A3E RID: 2622 RVA: 0x00060C6E File Offset: 0x0005EE6E
		[Serialize(false, IsPropertySaveable.Yes, "Does this conversation continue after this ConversationAction? If you have multiple successive ConversationActions, perhaps with some actions happening in between, you can enable this to prevent the dialog prompt from closing between the actions. Not necessary if the ConversationActions are nested inside each other: those are always considered parts of the same conversation, and shown in the same prompt.", "", false)]
		public bool ContinueConversation { get; set; }

		// Token: 0x170002D9 RID: 729
		// (get) Token: 0x06000A3F RID: 2623 RVA: 0x00060C77 File Offset: 0x0005EE77
		// (set) Token: 0x06000A40 RID: 2624 RVA: 0x00060C7F File Offset: 0x0005EE7F
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the event will not stop to wait for the conversation to be dismissed.", "", false)]
		public bool ContinueAutomatically { get; set; }

		// Token: 0x170002DA RID: 730
		// (get) Token: 0x06000A41 RID: 2625 RVA: 0x00060C88 File Offset: 0x0005EE88
		// (set) Token: 0x06000A42 RID: 2626 RVA: 0x00060C90 File Offset: 0x0005EE90
		[Serialize(false, IsPropertySaveable.Yes, "If SpeakerTag is defined, the conversation is interrupted by default if the speaker and the target end up too far from each other. This can be used to disable that behavior, keeping the dialog prompt open regardless of the distance.", "", false)]
		public bool IgnoreInterruptDistance { get; set; }

		// Token: 0x170002DB RID: 731
		// (get) Token: 0x06000A43 RID: 2627 RVA: 0x00060C99 File Offset: 0x0005EE99
		// (set) Token: 0x06000A44 RID: 2628 RVA: 0x00060CA1 File Offset: 0x0005EEA1
		public Character Speaker { get; private set; }

		// Token: 0x170002DC RID: 732
		// (get) Token: 0x06000A45 RID: 2629 RVA: 0x00060CAA File Offset: 0x0005EEAA
		// (set) Token: 0x06000A46 RID: 2630 RVA: 0x00060CB2 File Offset: 0x0005EEB2
		public List<ConversationAction.OptionActionGroup> Options { get; private set; }

		// Token: 0x170002DD RID: 733
		// (get) Token: 0x06000A47 RID: 2631 RVA: 0x00060CBB File Offset: 0x0005EEBB
		// (set) Token: 0x06000A48 RID: 2632 RVA: 0x00060CC3 File Offset: 0x0005EEC3
		public EventAction.SubactionGroup Interrupted { get; private set; }

		// Token: 0x06000A49 RID: 2633 RVA: 0x00060CCC File Offset: 0x0005EECC
		public ConversationAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
			ConversationAction.actionCount += 1;
			this.Identifier = ConversationAction.actionCount;
			this.Options = new List<ConversationAction.OptionActionGroup>();
			foreach (ContentXElement elem in element.Elements())
			{
				if (elem.Name.LocalName.Equals("option", StringComparison.OrdinalIgnoreCase))
				{
					this.Options.Add(new ConversationAction.OptionActionGroup(this.ParentEvent, elem));
				}
				else if (elem.Name.LocalName.Equals("interrupt", StringComparison.OrdinalIgnoreCase))
				{
					this.Interrupted = new EventAction.SubactionGroup(this.ParentEvent, elem);
				}
				else if (elem.Name.LocalName.Equals("text", StringComparison.OrdinalIgnoreCase))
				{
					this.Text = elem.GetAttributeString("tag", string.Empty);
					this.textElement = elem;
				}
				else
				{
					string thisName = "ConversationAction";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(148, 5);
					defaultInterpolatedStringHandler.AppendLiteral("Error in ");
					defaultInterpolatedStringHandler.AppendFormatted(thisName);
					defaultInterpolatedStringHandler.AppendLiteral(" in the event \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(parentEvent.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					defaultInterpolatedStringHandler.AppendLiteral(" - unrecognized child element \"");
					defaultInterpolatedStringHandler.AppendFormatted<XName>(elem.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\". If it's an action intended to execute after the ");
					defaultInterpolatedStringHandler.AppendFormatted(thisName);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendLiteral("it should be after the ");
					defaultInterpolatedStringHandler.AppendFormatted(thisName);
					defaultInterpolatedStringHandler.AppendLiteral(", not inside it.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				}
			}
		}

		// Token: 0x06000A4A RID: 2634 RVA: 0x00060EB8 File Offset: 0x0005F0B8
		public LocalizedString GetDisplayText()
		{
			LocalizedString text = string.Empty;
			if (this.textElement != null)
			{
				TextManager.ConstructDescription(ref text, this.textElement, new Func<string, string>(this.ParentEvent.GetTextForReplacementElement));
			}
			else
			{
				text = TextManager.Get(this.Text).Fallback(this.Text, true);
				if (text.Value.IsNullOrEmpty())
				{
					text = text.Fallback(this.Text, true);
				}
			}
			return this.ParentEvent.ReplaceVariablesInEventText(text);
		}

		// Token: 0x06000A4B RID: 2635 RVA: 0x00060F42 File Offset: 0x0005F142
		public override IEnumerable<EventAction> GetSubActions()
		{
			return this.Options.SelectMany((ConversationAction.OptionActionGroup group) => group.Actions);
		}

		// Token: 0x06000A4C RID: 2636 RVA: 0x00060F70 File Offset: 0x0005F170
		public override bool IsFinished(ref string goTo)
		{
			if (this.interrupt)
			{
				if (this.dialogOpened)
				{
					GUIMessageBox guimessageBox = this.dialogBox;
					if (guimessageBox != null)
					{
						guimessageBox.Close();
					}
					GUIMessageBox.MessageBoxes.ForEachMod(delegate(GUIComponent mb)
					{
						if (mb.UserData as string == "ConversationAction")
						{
							GUIMessageBox guimessageBox2 = mb as GUIMessageBox;
							if (guimessageBox2 == null)
							{
								return;
							}
							guimessageBox2.Close();
						}
					});
					this.ResetSpeaker();
					this.dialogOpened = false;
				}
				if (this.Interrupted == null)
				{
					if (this.EndEventIfInterrupted)
					{
						goTo = "_end";
					}
					return true;
				}
				return this.Interrupted.IsFinished(ref goTo);
			}
			else
			{
				if (this.ContinueAutomatically && this.Options.None(null))
				{
					return this.dialogOpened;
				}
				if (this.selectedOption >= 0 && (this.Options.None(null) || this.Options[this.selectedOption].IsFinished(ref goTo)))
				{
					this.ResetSpeaker();
					return true;
				}
				return false;
			}
		}

		// Token: 0x06000A4D RID: 2637 RVA: 0x00061050 File Offset: 0x0005F250
		public override void Reset()
		{
			this.Options.ForEach(delegate(ConversationAction.OptionActionGroup a)
			{
				a.Reset();
			});
			this.ResetSpeaker();
			this.selectedOption = -1;
			this.interrupt = false;
			this.dialogOpened = false;
			this.Speaker = null;
			GUIMessageBox guimessageBox = this.dialogBox;
			if (guimessageBox != null)
			{
				guimessageBox.Close();
			}
			this.dialogBox = null;
		}

		// Token: 0x06000A4E RID: 2638 RVA: 0x000610C1 File Offset: 0x0005F2C1
		public void RetriggerAfter(float delay)
		{
			this.startDelay = delay;
			this.dialogOpened = false;
			this.selectedOption = -1;
		}

		// Token: 0x06000A4F RID: 2639 RVA: 0x000610D8 File Offset: 0x0005F2D8
		public override bool SetGoToTarget(string goTo)
		{
			this.selectedOption = -1;
			for (int i = 0; i < this.Options.Count; i++)
			{
				if (this.Options[i].SetGoToTarget(goTo))
				{
					this.selectedOption = i;
					this.interrupt = false;
					this.dialogOpened = true;
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000A50 RID: 2640 RVA: 0x00061130 File Offset: 0x0005F330
		private void ResetSpeaker()
		{
			if (this.Speaker == null)
			{
				return;
			}
			this.Speaker.CampaignInteractionType = CampaignMode.InteractionType.None;
			this.Speaker.ActiveConversation = null;
			this.Speaker.SetCustomInteract(null, null);
			HumanAIController humanAI = this.Speaker.AIController as HumanAIController;
			if (humanAI != null && !this.Speaker.IsDead && !this.Speaker.Removed)
			{
				humanAI.ClearForcedOrder();
				if (this.prevIdleObjective != null)
				{
					humanAI.ObjectiveManager.AddObjective(this.prevIdleObjective);
				}
				if (this.prevGotoObjective != null && !this.prevGotoObjective.Abandon)
				{
					humanAI.ObjectiveManager.AddObjective(this.prevGotoObjective);
				}
				humanAI.ObjectiveManager.SortObjectives();
			}
		}

		// Token: 0x06000A51 RID: 2641 RVA: 0x000611EC File Offset: 0x0005F3EC
		public int[] GetEndingOptions()
		{
			List<int> endings = (from @group in this.Options.Where(delegate(ConversationAction.OptionActionGroup @group)
			{
				if (@group.EndConversation || !@group.Actions.Any<EventAction>())
				{
					return true;
				}
				if (@group.Actions.None((EventAction a) => a is ConversationAction))
				{
					return @group.Actions.Any(delegate(EventAction a)
					{
						GoTo goTo = a as GoTo;
						return goTo != null && goTo.EndConversation;
					});
				}
				return false;
			})
			select this.Options.IndexOf(@group)).ToList<int>();
			if (!this.ContinueConversation)
			{
				endings.Add(-1);
			}
			return endings.ToArray();
		}

		// Token: 0x06000A52 RID: 2642 RVA: 0x00061250 File Offset: 0x0005F450
		public override void Update(float deltaTime)
		{
			this.startDelay -= deltaTime;
			if (this.startDelay > 0f)
			{
				return;
			}
			if (this.interrupt)
			{
				EventAction.SubactionGroup interrupted = this.Interrupted;
				if (interrupted == null)
				{
					return;
				}
				interrupted.Update(deltaTime);
				return;
			}
			else if (this.selectedOption < 0)
			{
				if (this.dialogOpened)
				{
					this.lastActiveTime = Timing.TotalTime;
					if (GUIMessageBox.MessageBoxes.Any((GUIComponent mb) => mb.UserData as string == "ConversationAction"))
					{
						Character.DisableControls = true;
					}
					else
					{
						this.Reset();
					}
					if (this.ShouldInterrupt(true))
					{
						this.ResetSpeaker();
						this.interrupt = true;
					}
					return;
				}
				if (this.SpeakerTag.IsEmpty)
				{
					this.TryStartConversation(this.Speaker, null);
					return;
				}
				if (this.npcWaitObjective != null)
				{
					this.npcWaitObjective.ForceHighestPriority = true;
				}
				if (this.Speaker != null && !this.Speaker.Removed && this.Speaker.CampaignInteractionType == CampaignMode.InteractionType.Talk)
				{
					ConversationAction activeConversation = this.Speaker.ActiveConversation;
					if (((activeConversation != null) ? activeConversation.ParentEvent : null) != this.ParentEvent)
					{
						return;
					}
				}
				this.Speaker = (this.ParentEvent.GetTargets(this.SpeakerTag).FirstOrDefault((Entity e) => e is Character) as Character);
				if (this.Speaker == null || this.Speaker.Removed)
				{
					return;
				}
				if (this.Speaker.CampaignInteractionType == CampaignMode.InteractionType.Talk)
				{
					ConversationAction activeConversation2 = this.Speaker.ActiveConversation;
					if (((activeConversation2 != null) ? activeConversation2.ParentEvent : null) != this.ParentEvent)
					{
						return;
					}
				}
				if (!this.WaitForInteraction)
				{
					this.TryStartConversation(this.Speaker, null);
					return;
				}
				if (this.Speaker.ActiveConversation != this)
				{
					this.Speaker.CampaignInteractionType = CampaignMode.InteractionType.Talk;
					this.Speaker.ActiveConversation = this;
					Character speaker = this.Speaker;
					Action<Character, Character> onCustomInteract = new Action<Character, Character>(this.TryStartConversation);
					string tag = "CampaignInteraction.Talk";
					string varName = "[key]";
					GameSettings.Config.KeyMapping keyMap = GameSettings.CurrentConfig.KeyMap;
					speaker.SetCustomInteract(onCustomInteract, TextManager.GetWithVariable(tag, varName, keyMap.KeyBindText(InputType.Use), FormatCapitals.No));
				}
				return;
			}
			else
			{
				if (this.ShouldInterrupt(false))
				{
					this.ResetSpeaker();
					this.interrupt = true;
					return;
				}
				if (this.Options.Any<ConversationAction.OptionActionGroup>())
				{
					this.Options[this.selectedOption].Update(deltaTime);
				}
				return;
			}
		}

		// Token: 0x06000A53 RID: 2643 RVA: 0x000614B0 File Offset: 0x0005F6B0
		private bool ShouldInterrupt(bool requireTarget)
		{
			IEnumerable<Entity> targets = Enumerable.Empty<Entity>();
			if (!this.TargetTag.IsEmpty & requireTarget)
			{
				targets = from e in this.ParentEvent.GetTargets(this.TargetTag)
				where this.IsValidTarget(e, requireTarget)
				select e;
				if (!targets.Any<Entity>())
				{
					return true;
				}
			}
			if (this.Speaker == null)
			{
				return false;
			}
			if ((!this.TargetTag.IsEmpty & requireTarget) && !this.IgnoreInterruptDistance && targets.All((Entity t) => Vector2.DistanceSquared(t.WorldPosition, this.Speaker.WorldPosition) > 90000f))
			{
				return true;
			}
			HumanAIController humanAI = this.Speaker.AIController as HumanAIController;
			return (humanAI != null && !humanAI.AllowCampaignInteraction()) || this.Speaker.Removed || this.Speaker.IsDead || this.Speaker.IsIncapacitated;
		}

		// Token: 0x06000A54 RID: 2644 RVA: 0x000615A8 File Offset: 0x0005F7A8
		private bool IsValidTarget(Entity e, bool requirePlayerControlled = true)
		{
			Character character = e as Character;
			bool isValid = character != null && !character.Removed && !character.IsDead && !character.IsIncapacitated && (character == Character.Controlled || character.IsRemotePlayer || !requirePlayerControlled);
			bool block = GUI.InputBlockingMenuOpen && !this.dialogOpened;
			return isValid & (e != Character.Controlled || !block);
		}

		// Token: 0x06000A55 RID: 2645 RVA: 0x0006161C File Offset: 0x0005F81C
		private void TryStartConversation(Character speaker, Character targetCharacter = null)
		{
			IEnumerable<Entity> targets = Enumerable.Empty<Entity>();
			if (!this.TargetTag.IsEmpty)
			{
				targets = from e in this.ParentEvent.GetTargets(this.TargetTag)
				where this.IsValidTarget(e, true)
				select e;
				if (!targets.Any<Entity>() || this.IsBlockedByAnotherConversation(targets, 5f))
				{
					return;
				}
				if (targetCharacter != null && !targets.Contains(targetCharacter))
				{
					return;
				}
			}
			else if (this.IsBlockedByAnotherConversation((targetCharacter != null) ? targetCharacter.ToEnumerable<Character>() : null, 5f))
			{
				return;
			}
			Character speaker2 = speaker;
			HumanAIController humanAI = ((speaker2 != null) ? speaker2.AIController : null) as HumanAIController;
			if (humanAI != null)
			{
				this.prevIdleObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveIdle>();
				this.prevGotoObjective = humanAI.ObjectiveManager.GetObjective<AIObjectiveGoTo>();
				this.npcWaitObjective = humanAI.SetForcedOrder(new Order(OrderPrefab.Prefabs["wait"], Barotrauma.Identifier.Empty, null, null));
				if (targets.Any<Entity>() || targetCharacter != null)
				{
					Entity closestTarget = targetCharacter;
					float closestDist = float.MaxValue;
					foreach (Entity entity in targets)
					{
						float dist = Vector2.DistanceSquared(entity.WorldPosition, speaker.WorldPosition);
						if (dist < closestDist)
						{
							closestTarget = entity;
							closestDist = dist;
						}
					}
					if (closestTarget != null)
					{
						humanAI.FaceTarget(closestTarget);
					}
				}
			}
			if (targetCharacter != null && !this.InvokerTag.IsEmpty)
			{
				this.ParentEvent.AddTarget(this.InvokerTag, targetCharacter);
			}
			if (this.ForceSay)
			{
				Character speaker3 = speaker;
				if (speaker3 != null)
				{
					speaker3.ForceSay(this.ForceSayText.IsNullOrEmpty() ? TextManager.Get(this.Text).Fallback(this.Text, true) : TextManager.Get(this.ForceSayText).Fallback(this.ForceSayText, true), this.ForceSayInRadio, this.ForceSayRemoveQuotes, 0.7f);
				}
			}
			this.ShowDialog(this.Speaker, targetCharacter);
			this.dialogOpened = true;
			if (this.Speaker != null)
			{
				this.Speaker = speaker;
				this.Options.SelectMany((ConversationAction.OptionActionGroup op) => op.Actions).OfType<ConversationAction>().ForEach(delegate(ConversationAction action)
				{
					action.Speaker = speaker;
				});
				speaker.CampaignInteractionType = CampaignMode.InteractionType.None;
				speaker.SetCustomInteract(null, null);
			}
		}

		// Token: 0x06000A56 RID: 2646 RVA: 0x000618B4 File Offset: 0x0005FAB4
		private void ShowDialog(Character speaker, Character targetCharacter)
		{
			LocalizedString displayText = this.GetDisplayText();
			IEnumerable<string> options = from opt in this.Options
			select opt.Text;
			int[] endingOptions = this.GetEndingOptions();
			string eventSprite = this.EventSprite;
			bool fadeToBlack = this.FadeToBlack;
			ConversationAction.DialogTypes dialogType = this.DialogType;
			bool continueConversation = this.ContinueConversation;
			ConversationAction.CreateDialog(displayText, speaker, options, endingOptions, eventSprite, this, null, fadeToBlack, dialogType, continueConversation);
		}

		// Token: 0x06000A57 RID: 2647 RVA: 0x00061928 File Offset: 0x0005FB28
		public override string ToDebugString()
		{
			if (!this.interrupt)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 3);
				defaultInterpolatedStringHandler.AppendFormatted(ToolBox.GetDebugSymbol(this.selectedOption > -1, this.selectedOption < 0 && this.dialogOpened));
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendFormatted("ConversationAction");
				defaultInterpolatedStringHandler.AppendLiteral(" -> (Selected option: ");
				defaultInterpolatedStringHandler.AppendFormatted(this.selectedOption.ColorizeObject());
				defaultInterpolatedStringHandler.AppendLiteral(")");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			return ToolBox.GetDebugSymbol(true, this.selectedOption < 0 && this.dialogOpened) + " ConversationAction -> (Interrupted)";
		}

		// Token: 0x06000A58 RID: 2648 RVA: 0x000619E4 File Offset: 0x0005FBE4
		[CompilerGenerated]
		internal static void <CreateDialog>g__RecalculateLastMessage|10_1(GUIListBox conversationList, bool append)
		{
			GUILayoutGroup lastElement = conversationList.Content.Children.LastOrDefault<GUIComponent>() as GUILayoutGroup;
			if (lastElement != null)
			{
				GUILayoutGroup textLayout = lastElement.GetChild<GUILayoutGroup>();
				if (textLayout != null)
				{
					if (lastElement.Rect.Size.Y < textLayout.Rect.Size.Y && !append)
					{
						lastElement.RectTransform.MinSize = textLayout.Rect.Size;
					}
					int textHeight = textLayout.Children.Sum((GUIComponent c) => c.Rect.Height);
					textLayout.RectTransform.MaxSize = new Point(lastElement.RectTransform.MaxSize.X, textHeight);
					textLayout.Recalculate();
				}
				int sumHeight = lastElement.Children.Sum((GUIComponent c) => c.Rect.Height);
				lastElement.RectTransform.MaxSize = new Point(lastElement.RectTransform.MaxSize.X, sumHeight);
				lastElement.Recalculate();
				conversationList.RecalculateChildren();
				if (!append || textLayout == null)
				{
					return;
				}
				foreach (GUIComponent child in textLayout.Children)
				{
					conversationList.UpdateScrollBarSize();
					float wait = (conversationList.BarSize < 1f) ? 0.5f : 0f;
					if (child is GUITextBlock)
					{
						child.FadeIn(wait, 0.5f, false);
					}
					GUIButton btn = child as GUIButton;
					if (btn != null)
					{
						btn.FadeIn(wait, 1f, false);
						btn.TextBlock.FadeIn(wait, 0.5f, false);
					}
				}
			}
		}

		// Token: 0x0400052E RID: 1326
		private GUIMessageBox dialogBox;

		// Token: 0x0400052F RID: 1327
		private static ConversationAction lastActiveAction;

		// Token: 0x04000530 RID: 1328
		private static GUIMessageBox lastMessageBox;

		// Token: 0x04000531 RID: 1329
		private static bool shouldFadeToBlack;

		// Token: 0x04000532 RID: 1330
		private const float InterruptDistance = 300f;

		// Token: 0x04000533 RID: 1331
		private const float BlockOtherConversationsDuration = 5f;

		// Token: 0x04000545 RID: 1349
		private AIObjective prevIdleObjective;

		// Token: 0x04000546 RID: 1350
		private AIObjective prevGotoObjective;

		// Token: 0x04000547 RID: 1351
		private AIObjective npcWaitObjective;

		// Token: 0x0400054A RID: 1354
		private static ushort actionCount;

		// Token: 0x0400054B RID: 1355
		public readonly ushort Identifier;

		// Token: 0x0400054C RID: 1356
		private float startDelay;

		// Token: 0x0400054D RID: 1357
		private int selectedOption = -1;

		// Token: 0x0400054E RID: 1358
		private bool dialogOpened;

		// Token: 0x0400054F RID: 1359
		private double lastActiveTime;

		// Token: 0x04000550 RID: 1360
		private bool interrupt;

		// Token: 0x04000551 RID: 1361
		private readonly XElement textElement;

		// Token: 0x02000794 RID: 1940
		public class OptionActionGroup : EventAction.SubactionGroup
		{
			// Token: 0x170019EA RID: 6634
			// (get) Token: 0x06006AE0 RID: 27360 RVA: 0x0035BBBB File Offset: 0x00359DBB
			// (set) Token: 0x06006AE1 RID: 27361 RVA: 0x0035BBC3 File Offset: 0x00359DC3
			[Serialize("", IsPropertySaveable.Yes, "The text to display in the option.", "", false)]
			public string Text { get; set; }

			// Token: 0x170019EB RID: 6635
			// (get) Token: 0x06006AE2 RID: 27362 RVA: 0x0035BBCC File Offset: 0x00359DCC
			// (set) Token: 0x06006AE3 RID: 27363 RVA: 0x0035BBD4 File Offset: 0x00359DD4
			[Serialize(false, IsPropertySaveable.Yes, "Should this option end the conversation (closing the conversation prompt?). By default, options that don't have any actions inside them, or that only have a GoTo action, end the conversation. But if there are other actions inside the option, the game assumes there may be some kind of a follow-up coming to the conversation, and by default leaves it open.", "", false)]
			public bool EndConversation { get; set; }

			// Token: 0x170019EC RID: 6636
			// (get) Token: 0x06006AE4 RID: 27364 RVA: 0x0035BBDD File Offset: 0x00359DDD
			// (set) Token: 0x06006AE5 RID: 27365 RVA: 0x0035BBE5 File Offset: 0x00359DE5
			[Serialize(false, IsPropertySaveable.Yes, "If enabled, the player will send the Text in chat when selecting the option, or if ForceSayText is not empty, will send that instead.", "", false)]
			public bool ForceSay { get; set; }

			// Token: 0x170019ED RID: 6637
			// (get) Token: 0x06006AE6 RID: 27366 RVA: 0x0035BBEE File Offset: 0x00359DEE
			// (set) Token: 0x06006AE7 RID: 27367 RVA: 0x0035BBF6 File Offset: 0x00359DF6
			[Serialize(false, IsPropertySaveable.Yes, "If enabled, the message sent in chat will be sent in radio chat instead.", "", false)]
			public bool ForceSayInRadio { get; set; }

			// Token: 0x170019EE RID: 6638
			// (get) Token: 0x06006AE8 RID: 27368 RVA: 0x0035BBFF File Offset: 0x00359DFF
			// (set) Token: 0x06006AE9 RID: 27369 RVA: 0x0035BC07 File Offset: 0x00359E07
			[Serialize("", IsPropertySaveable.Yes, "Message sent in chat, if empty, Text is used instead.", "", false)]
			public string ForceSayText { get; set; }

			// Token: 0x170019EF RID: 6639
			// (get) Token: 0x06006AEA RID: 27370 RVA: 0x0035BC10 File Offset: 0x00359E10
			// (set) Token: 0x06006AEB RID: 27371 RVA: 0x0035BC18 File Offset: 0x00359E18
			[Serialize(true, IsPropertySaveable.Yes, "Should the chat message be stripped of any quotation mark characters?", "", false)]
			public bool ForceSayRemoveQuotes { get; set; }

			// Token: 0x06006AEC RID: 27372 RVA: 0x0035BC21 File Offset: 0x00359E21
			public OptionActionGroup(ScriptedEvent scriptedEvent, ContentXElement element) : base(scriptedEvent, element)
			{
			}
		}

		// Token: 0x02000795 RID: 1941
		public enum DialogTypes
		{
			// Token: 0x04003B03 RID: 15107
			Regular,
			// Token: 0x04003B04 RID: 15108
			Small,
			// Token: 0x04003B05 RID: 15109
			Mission
		}
	}
}
