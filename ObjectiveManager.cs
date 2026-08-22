using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Tutorials;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000070 RID: 112
	internal static class ObjectiveManager
	{
		// Token: 0x17000426 RID: 1062
		// (get) Token: 0x06001064 RID: 4196 RVA: 0x0009BFED File Offset: 0x0009A1ED
		// (set) Token: 0x06001065 RID: 4197 RVA: 0x0009BFF4 File Offset: 0x0009A1F4
		public static bool ContentRunning { get; private set; }

		// Token: 0x17000427 RID: 1063
		// (get) Token: 0x06001066 RID: 4198 RVA: 0x0009BFFC File Offset: 0x0009A1FC
		public static VideoPlayer VideoPlayer { get; } = new VideoPlayer();

		// Token: 0x17000428 RID: 1064
		// (get) Token: 0x06001067 RID: 4199 RVA: 0x0009C003 File Offset: 0x0009A203
		// (set) Token: 0x06001068 RID: 4200 RVA: 0x0009C00A File Offset: 0x0009A20A
		private static ObjectiveManager.Segment ActiveContentSegment { get; set; }

		// Token: 0x06001069 RID: 4201 RVA: 0x0009C014 File Offset: 0x0009A214
		public static void AddToGUIUpdateList()
		{
			if (ObjectiveManager.screenSettings.HaveChanged())
			{
				ObjectiveManager.CreateObjectiveFrame();
			}
			if (ObjectiveManager.activeObjectives.Count > 0)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaignMode = (gameSession != null) ? gameSession.Campaign : null;
				if (campaignMode == null || !campaignMode.ShowCampaignUI)
				{
					GUILayoutGroup guilayoutGroup = ObjectiveManager.objectiveGroup;
					if (guilayoutGroup != null)
					{
						guilayoutGroup.AddToGUIUpdateList(false, -1);
					}
				}
			}
			GUIComponent guicomponent = ObjectiveManager.infoBox;
			if (guicomponent != null)
			{
				guicomponent.AddToGUIUpdateList(false, 100);
			}
			ObjectiveManager.VideoPlayer.AddToGUIUpdateList(false, 100);
		}

		// Token: 0x0600106A RID: 4202 RVA: 0x0009C090 File Offset: 0x0009A290
		public static bool IsSegmentActive(Identifier segmentId)
		{
			return ObjectiveManager.activeObjectives.Any((ObjectiveManager.Segment o) => o.Id == segmentId);
		}

		// Token: 0x0600106B RID: 4203 RVA: 0x0009C0C0 File Offset: 0x0009A2C0
		public static void TriggerSegment(ObjectiveManager.Segment segment, bool connectObjective = false)
		{
			if (segment.SegmentType != SegmentType.InfoBox)
			{
				ObjectiveManager.activeObjectives.Add(segment);
				ObjectiveManager.AddToObjectiveList(segment, connectObjective, false);
				return;
			}
			Inventory.DraggingItems.Clear();
			ObjectiveManager.ContentRunning = true;
			ObjectiveManager.ActiveContentSegment = segment;
			LocalizedString title = TextManager.Get(segment.Id);
			LocalizedString text = TextManager.GetFormatted(segment.TextContent.Tag, Array.Empty<object>()).Fallback(segment.TextContent.Tag.Value, true);
			text = TextManager.ParseInputTypes(text, false);
			AutoPlayVideo autoPlayVideo = segment.AutoPlayVideo;
			if (autoPlayVideo == AutoPlayVideo.Yes)
			{
				LocalizedString title2 = title;
				LocalizedString text2 = text;
				int width = segment.TextContent.Width;
				int height = segment.TextContent.Height;
				Anchor anchor = segment.TextContent.Anchor;
				bool hasButton = true;
				Action onInfoBoxClosed;
				if ((onInfoBoxClosed = ObjectiveManager.<>O.<0>__LoadActiveContentVideo) == null)
				{
					onInfoBoxClosed = (ObjectiveManager.<>O.<0>__LoadActiveContentVideo = new Action(ObjectiveManager.LoadActiveContentVideo));
				}
				ObjectiveManager.infoBox = ObjectiveManager.CreateInfoFrame(title2, text2, width, height, anchor, hasButton, onInfoBoxClosed, null);
				return;
			}
			if (autoPlayVideo != AutoPlayVideo.No)
			{
				return;
			}
			LocalizedString title3 = title;
			LocalizedString text3 = text;
			int width2 = segment.TextContent.Width;
			int height2 = segment.TextContent.Height;
			Anchor anchor2 = segment.TextContent.Anchor;
			bool hasButton2 = true;
			Action onInfoBoxClosed2;
			if ((onInfoBoxClosed2 = ObjectiveManager.<>O.<1>__StopCurrentContentSegment) == null)
			{
				onInfoBoxClosed2 = (ObjectiveManager.<>O.<1>__StopCurrentContentSegment = new Action(ObjectiveManager.StopCurrentContentSegment));
			}
			Action onVideoButtonClicked;
			if ((onVideoButtonClicked = ObjectiveManager.<>O.<0>__LoadActiveContentVideo) == null)
			{
				onVideoButtonClicked = (ObjectiveManager.<>O.<0>__LoadActiveContentVideo = new Action(ObjectiveManager.LoadActiveContentVideo));
			}
			ObjectiveManager.infoBox = ObjectiveManager.CreateInfoFrame(title3, text3, width2, height2, anchor2, hasButton2, onInfoBoxClosed2, onVideoButtonClicked);
		}

		// Token: 0x0600106C RID: 4204 RVA: 0x0009C208 File Offset: 0x0009A408
		public static void CompleteSegment(Identifier segmentId)
		{
			ObjectiveManager.Segment segment = ObjectiveManager.GetActiveObjective(segmentId);
			if (segment == null || !segment.CanBeCompleted || segment.IsCompleted)
			{
				return;
			}
			ObjectiveManager.CompleteSegment(segment, false);
		}

		// Token: 0x0600106D RID: 4205 RVA: 0x0009C238 File Offset: 0x0009A438
		public static void FailSegment(Identifier segmentId)
		{
			ObjectiveManager.Segment segment = ObjectiveManager.GetActiveObjective(segmentId);
			if (segment == null)
			{
				return;
			}
			ObjectiveManager.CompleteSegment(segment, true);
		}

		// Token: 0x0600106E RID: 4206 RVA: 0x0009C258 File Offset: 0x0009A458
		private static void CompleteSegment(ObjectiveManager.Segment segment, bool failed = false)
		{
			if (failed)
			{
				if (!ObjectiveManager.MarkSegmentFailed(segment, true))
				{
					return;
				}
			}
			else if (!ObjectiveManager.MarkSegmentCompleted(segment, true))
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			TutorialMode tutorialMode = ((gameSession != null) ? gameSession.GameMode : null) as TutorialMode;
			if (tutorialMode != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Tutorial:");
				Tutorial tutorial = tutorialMode.Tutorial;
				defaultInterpolatedStringHandler.AppendFormatted<Identifier?>((tutorial != null) ? new Identifier?(tutorial.Identifier) : null);
				defaultInterpolatedStringHandler.AppendLiteral(":");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(segment.Id);
				defaultInterpolatedStringHandler.AppendLiteral(":");
				defaultInterpolatedStringHandler.AppendFormatted(failed ? "Failed" : "Completed");
				GameAnalyticsManager.AddDesignEvent(defaultInterpolatedStringHandler.ToStringAndClear());
			}
		}

		// Token: 0x0600106F RID: 4207 RVA: 0x0009C31C File Offset: 0x0009A51C
		private static bool MarkSegmentCompleted(ObjectiveManager.Segment segment, bool flash = true)
		{
			return ObjectiveManager.MarkSegment(segment, "ObjectiveIndicatorCompleted", flash, GUIStyle.Green);
		}

		// Token: 0x06001070 RID: 4208 RVA: 0x0009C334 File Offset: 0x0009A534
		private static bool MarkSegmentFailed(ObjectiveManager.Segment segment, bool flash = true)
		{
			return ObjectiveManager.MarkSegment(segment, "MissionFailedIcon", flash, GUIStyle.Red);
		}

		// Token: 0x06001071 RID: 4209 RVA: 0x0009C34C File Offset: 0x0009A54C
		private static bool MarkSegment(ObjectiveManager.Segment segment, string iconStyleName, bool flash, Color flashColor)
		{
			segment.IsCompleted = true;
			GUIComponentStyle style = GUIStyle.GetComponentStyle(iconStyleName);
			if (style != null)
			{
				if (segment.ObjectiveStateIndicator.Style == style)
				{
					return false;
				}
				segment.ObjectiveStateIndicator.ApplyStyle(style);
			}
			if (flash)
			{
				segment.ObjectiveStateIndicator.Parent.Flash(new Color?(flashColor), 0.35f, true, false, null);
			}
			segment.ObjectiveButton.OnClicked = null;
			segment.ObjectiveButton.CanBeFocused = false;
			return true;
		}

		// Token: 0x06001072 RID: 4210 RVA: 0x0009C3C8 File Offset: 0x0009A5C8
		public static void RemoveSegment(Identifier segmentId)
		{
			ObjectiveManager.Segment segment = ObjectiveManager.GetActiveObjective(segmentId);
			if (segment == null)
			{
				return;
			}
			segment.ObjectiveStateIndicator.FadeOut(1.5f, false, 0f, null, false);
			segment.LinkedTextBlock.FadeOut(1.5f, false, 0f, null, false);
			GUIComponent parent = segment.LinkedTextBlock.Parent;
			parent.FadeOut(1.5f, true, 0f, delegate
			{
				ObjectiveManager.activeObjectives.Remove(segment);
				GUILayoutGroup guilayoutGroup = ObjectiveManager.objectiveGroup;
				if (guilayoutGroup == null)
				{
					return;
				}
				guilayoutGroup.Recalculate();
			}, false);
			parent.RectTransform.MoveOverTime(ObjectiveManager.GetObjectiveHiddenPosition(parent.RectTransform), 1.5f, null);
			segment.ObjectiveButton.OnClicked = null;
			segment.ObjectiveButton.CanBeFocused = false;
		}

		// Token: 0x06001073 RID: 4211 RVA: 0x0009C497 File Offset: 0x0009A697
		public static void CloseActiveContentGUI()
		{
			if (ObjectiveManager.VideoPlayer.IsPlaying)
			{
				ObjectiveManager.VideoPlayer.Stop();
				return;
			}
			if (ObjectiveManager.infoBox != null)
			{
				ObjectiveManager.CloseInfoFrame();
			}
		}

		// Token: 0x06001074 RID: 4212 RVA: 0x0009C4BC File Offset: 0x0009A6BC
		public static void ClearContent()
		{
			ObjectiveManager.ContentRunning = false;
			ObjectiveManager.infoBox = null;
		}

		// Token: 0x06001075 RID: 4213 RVA: 0x0009C4CA File Offset: 0x0009A6CA
		public static void ResetUI()
		{
			ObjectiveManager.ContentRunning = false;
			ObjectiveManager.infoBox = null;
			ObjectiveManager.VideoPlayer.Remove();
		}

		// Token: 0x06001076 RID: 4214 RVA: 0x0009C4E4 File Offset: 0x0009A6E4
		private static ObjectiveManager.Segment GetActiveObjective(Identifier id)
		{
			return ObjectiveManager.activeObjectives.FirstOrDefault((ObjectiveManager.Segment s) => s.Id == id);
		}

		// Token: 0x06001077 RID: 4215 RVA: 0x0009C514 File Offset: 0x0009A714
		public static void ResetObjectives()
		{
			ObjectiveManager.activeObjectives.Clear();
			ObjectiveManager.ActiveContentSegment = null;
			ObjectiveManager.CreateObjectiveFrame();
		}

		// Token: 0x06001078 RID: 4216 RVA: 0x0009C52C File Offset: 0x0009A72C
		private static void CreateObjectiveFrame()
		{
			GUIFrame objectiveListFrame = new GUIFrame(HUDLayoutSettings.ToRectTransform(HUDLayoutSettings.TutorialObjectiveListArea, GUI.Canvas), null, null)
			{
				CanBeFocused = false
			};
			ObjectiveManager.objectiveGroup = new GUILayoutGroup(new RectTransform(Vector2.One, objectiveListFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = (int)GUIStyle.Font.LineHeight
			};
			for (int i = 0; i < ObjectiveManager.activeObjectives.Count; i++)
			{
				ObjectiveManager.AddToObjectiveList(ObjectiveManager.activeObjectives[i], false, true);
			}
			ObjectiveManager.screenSettings = new ObjectiveManager.ScreenSettings(new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight), GUI.Scale, GameSettings.CurrentConfig.Graphics.DisplayMode);
		}

		// Token: 0x06001079 RID: 4217 RVA: 0x0009C602 File Offset: 0x0009A802
		private static void StopCurrentContentSegment()
		{
			if (!ObjectiveManager.ActiveContentSegment.ObjectiveText.IsNullOrEmpty())
			{
				ObjectiveManager.activeObjectives.Add(ObjectiveManager.ActiveContentSegment);
				ObjectiveManager.AddToObjectiveList(ObjectiveManager.ActiveContentSegment, false, false);
			}
			ObjectiveManager.ContentRunning = false;
			ObjectiveManager.ActiveContentSegment = null;
		}

		// Token: 0x0600107A RID: 4218 RVA: 0x0009C63C File Offset: 0x0009A83C
		private static void AddToObjectiveList(ObjectiveManager.Segment segment, bool connectExisting = false, bool useExistingIndex = false)
		{
			if (connectExisting)
			{
				ObjectiveManager.Segment existingSegment = ObjectiveManager.activeObjectives.Find((ObjectiveManager.Segment o) => o.Id == segment.Id);
				if (existingSegment != null)
				{
					existingSegment.ConnectMessageBox(segment);
					ObjectiveManager.<AddToObjectiveList>g__SetButtonBehavior|37_3(existingSegment);
				}
				return;
			}
			RectTransform frameRt = new RectTransform(new Vector2(1f, 0.1f), ObjectiveManager.objectiveGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, ObjectiveManager.objectiveGroup.AbsoluteSpacing)
			};
			ObjectiveManager.Segment parentSegment = ObjectiveManager.activeObjectives.FirstOrDefault(delegate(ObjectiveManager.Segment s)
			{
				Identifier parentId = segment.ParentId;
				return s.Id == parentId;
			});
			if (parentSegment != null)
			{
				int childIndex = useExistingIndex ? ObjectiveManager.activeObjectives.IndexOf(segment) : (ObjectiveManager.activeObjectives.IndexOf(parentSegment) + ObjectiveManager.activeObjectives.Count(delegate(ObjectiveManager.Segment s)
				{
					Identifier parentId = s.ParentId;
					Identifier parentId2 = segment.ParentId;
					return parentId == parentId2;
				}));
				if (ObjectiveManager.objectiveGroup.RectTransform.GetChildIndex(frameRt) != childIndex)
				{
					if (childIndex < 0 || childIndex >= frameRt.Parent.CountChildren)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(91, 4);
						defaultInterpolatedStringHandler.AppendLiteral("Error in ");
						defaultInterpolatedStringHandler.AppendFormatted("AddToObjectiveList");
						defaultInterpolatedStringHandler.AppendLiteral(". ");
						defaultInterpolatedStringHandler.AppendLiteral("Failed to reposition an objective in the list. Text \"");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(segment.ObjectiveText);
						defaultInterpolatedStringHandler.AppendLiteral("\", parentId: ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(segment.ParentId);
						defaultInterpolatedStringHandler.AppendLiteral(", childIndex: ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(childIndex);
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
					else
					{
						frameRt.RepositionChildInHierarchy(childIndex);
						ObjectiveManager.activeObjectives.Remove(segment);
						ObjectiveManager.activeObjectives.Insert(childIndex, segment);
					}
				}
			}
			frameRt.AbsoluteOffset = ObjectiveManager.GetObjectiveHiddenPosition(null);
			GUIFrame frame = new GUIFrame(frameRt, null, null)
			{
				CanBeFocused = true
			};
			ObjectiveManager.objectiveGroup.Recalculate();
			int textWidth = (parentSegment == null) ? (frameRt.Rect.Width - ObjectiveManager.objectiveGroup.AbsoluteSpacing) : (frameRt.Rect.Width - 2 * ObjectiveManager.objectiveGroup.AbsoluteSpacing);
			segment.LinkedTextBlock = new GUITextBlock(new RectTransform(new Point(textWidth, 0), frame.RectTransform, Anchor.TopRight, null, ScaleBasis.Normal, false), TextManager.ParseInputTypes(segment.ObjectiveText, false), null, null, Alignment.Left, true, "", null);
			Point size = new Point(segment.LinkedTextBlock.Rect.Width, segment.LinkedTextBlock.Rect.Height);
			segment.LinkedTextBlock.RectTransform.NonScaledSize = size;
			segment.LinkedTextBlock.RectTransform.MinSize = size;
			segment.LinkedTextBlock.RectTransform.MaxSize = size;
			segment.LinkedTextBlock.RectTransform.IsFixedSize = true;
			frame.RectTransform.Resize(new Point(frame.Rect.Width, segment.LinkedTextBlock.RectTransform.Rect.Height), false);
			frame.RectTransform.IsFixedSize = true;
			RectTransform indicatorRt = new RectTransform(new Point(ObjectiveManager.objectiveGroup.AbsoluteSpacing), frame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, true);
			if (parentSegment != null)
			{
				indicatorRt.AbsoluteOffset = new Point(ObjectiveManager.objectiveGroup.AbsoluteSpacing, 0);
			}
			segment.ObjectiveStateIndicator = new GUIImage(indicatorRt, "ObjectiveIndicatorIncomplete", GUIImage.ScalingMode.None);
			ObjectiveManager.<AddToObjectiveList>g__SetTransparent|37_2(segment.LinkedTextBlock);
			if (ObjectiveManager.objectiveTextTranslated == null)
			{
				ObjectiveManager.objectiveTextTranslated = TextManager.Get("Tutorial.Objective");
			}
			segment.ObjectiveButton = new GUIButton(new RectTransform(Vector2.One, segment.LinkedTextBlock.RectTransform, Anchor.TopLeft, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), Alignment.Center, null, null)
			{
				ToolTip = ObjectiveManager.objectiveTextTranslated
			};
			ObjectiveManager.<AddToObjectiveList>g__SetButtonBehavior|37_3(segment);
			ObjectiveManager.<AddToObjectiveList>g__SetTransparent|37_2(segment.ObjectiveButton);
			frameRt.MoveOverTime(new Point(0, frameRt.AbsoluteOffset.Y), 1.5f, delegate
			{
				GUILayoutGroup guilayoutGroup = ObjectiveManager.objectiveGroup;
				if (guilayoutGroup == null)
				{
					return;
				}
				guilayoutGroup.Recalculate();
			});
			if (!segment.IsCompleted)
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMetadata campaignMetadata;
				if (gameSession == null)
				{
					campaignMetadata = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					campaignMetadata = ((campaign != null) ? campaign.CampaignMetadata : null);
				}
				CampaignMetadata data = campaignMetadata;
				if (data != null && data.GetBoolean(segment.Id, null))
				{
					ObjectiveManager.MarkSegmentCompleted(segment, false);
				}
			}
		}

		// Token: 0x0600107B RID: 4219 RVA: 0x0009CB49 File Offset: 0x0009AD49
		private static void ReplaySegmentVideo(ObjectiveManager.Segment segment)
		{
			if (ObjectiveManager.ContentRunning)
			{
				return;
			}
			Inventory.DraggingItems.Clear();
			ObjectiveManager.ContentRunning = true;
			ObjectiveManager.LoadVideo(segment);
		}

		// Token: 0x0600107C RID: 4220 RVA: 0x0009CB6C File Offset: 0x0009AD6C
		private static void ShowSegmentText(ObjectiveManager.Segment segment)
		{
			if (ObjectiveManager.ContentRunning)
			{
				return;
			}
			Inventory.DraggingItems.Clear();
			ObjectiveManager.ContentRunning = true;
			ObjectiveManager.ActiveContentSegment = segment;
			ObjectiveManager.infoBox = ObjectiveManager.CreateInfoFrame(TextManager.Get(segment.Id).Fallback(segment.Id.Value, true), TextManager.Get(segment.TextContent.Tag).Fallback(segment.TextContent.Tag.Value, true), segment.TextContent.Width, segment.TextContent.Height, segment.TextContent.Anchor, true, delegate
			{
				ObjectiveManager.ContentRunning = false;
			}, delegate
			{
				ObjectiveManager.LoadVideo(segment);
			});
		}

		// Token: 0x0600107D RID: 4221 RVA: 0x0009CC72 File Offset: 0x0009AE72
		private static Point GetObjectiveHiddenPosition(RectTransform rt = null)
		{
			return new Point(GameMain.GraphicsWidth - ObjectiveManager.objectiveGroup.Rect.X, (rt != null) ? rt.AbsoluteOffset.Y : 0);
		}

		// Token: 0x0600107E RID: 4222 RVA: 0x0009CCA0 File Offset: 0x0009AEA0
		public static ObjectiveManager.Segment GetObjective(Identifier identifier)
		{
			return ObjectiveManager.activeObjectives.FirstOrDefault((ObjectiveManager.Segment o) => o.Id == identifier);
		}

		// Token: 0x0600107F RID: 4223 RVA: 0x0009CCD0 File Offset: 0x0009AED0
		public static bool AllActiveObjectivesCompleted()
		{
			if (!ObjectiveManager.activeObjectives.None(null))
			{
				return ObjectiveManager.activeObjectives.All((ObjectiveManager.Segment o) => !o.CanBeCompleted || o.IsCompleted);
			}
			return true;
		}

		// Token: 0x17000429 RID: 1065
		// (get) Token: 0x06001080 RID: 4224 RVA: 0x0009CD0A File Offset: 0x0009AF0A
		public static bool AnyObjectives
		{
			get
			{
				return ObjectiveManager.activeObjectives.Any<ObjectiveManager.Segment>();
			}
		}

		// Token: 0x06001081 RID: 4225 RVA: 0x0009CD16 File Offset: 0x0009AF16
		private static void CloseInfoFrame()
		{
			ObjectiveManager.CloseInfoFrame(null, null);
		}

		// Token: 0x06001082 RID: 4226 RVA: 0x0009CD20 File Offset: 0x0009AF20
		private static bool CloseInfoFrame(GUIButton button, object userData)
		{
			ObjectiveManager.infoBox = null;
			Action action = ObjectiveManager.infoBoxClosedCallback;
			if (action != null)
			{
				action();
			}
			return true;
		}

		// Token: 0x06001083 RID: 4227 RVA: 0x0009CD3C File Offset: 0x0009AF3C
		private static GUIComponent CreateInfoFrame(LocalizedString title, LocalizedString text, int width = 300, int height = 80, Anchor anchor = Anchor.TopRight, bool hasButton = false, Action onInfoBoxClosed = null, Action onVideoButtonClicked = null)
		{
			if (hasButton)
			{
				height += 60;
			}
			width = (int)((float)width * GUI.Scale);
			height = (int)((float)height * GUI.Scale);
			LocalizedString wrappedText = ToolBox.WrapText(text, (float)width, GUIStyle.Font, 1f);
			height += (int)GUIStyle.Font.MeasureString(wrappedText, false).Y;
			if (title.Length > 0)
			{
				height += (int)GUIStyle.Font.MeasureString(title, false).Y + (int)(150f * GUI.Scale);
			}
			GUIFrame background = new GUIFrame(new RectTransform(new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight), GUI.Canvas, Anchor.Center, null, ScaleBasis.Normal, false), "GUIBackgroundBlocker", null);
			GUIFrame infoBlock = new GUIFrame(new RectTransform(new Point(width, height), background.RectTransform, anchor, null, ScaleBasis.Normal, false), "", null);
			infoBlock.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
			GUILayoutGroup infoContent = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.9f), infoBlock.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = 5
			};
			if (title.Length > 0)
			{
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), infoContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = title;
				GUIFont largeFont = GUIStyle.LargeFont;
				GUITextBlock titleBlock = new GUITextBlock(rectT, text2, new Color?(new Color(253, 174, 0)), largeFont, Alignment.Center, false, "", null);
				titleBlock.RectTransform.IsFixedSize = true;
			}
			text = RichString.Rich(text, null);
			GUITextBlock textBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), infoContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), text, null, null, Alignment.Left, true, "", null);
			textBlock.RectTransform.IsFixedSize = true;
			ObjectiveManager.infoBoxClosedCallback = onInfoBoxClosed;
			if (hasButton)
			{
				GUILayoutGroup buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.15f), infoContent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					RelativeSpacing = 0.1f
				};
				buttonContainer.RectTransform.IsFixedSize = true;
				if (onVideoButtonClicked != null)
				{
					buttonContainer.Stretch = true;
					new GUIButton(new RectTransform(new Vector2(0.4f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Video"), Alignment.Center, "GUIButtonLarge", null).OnClicked = delegate(GUIButton button, object obj)
					{
						onVideoButtonClicked();
						return true;
					};
				}
				else
				{
					buttonContainer.Stretch = false;
					buttonContainer.ChildAnchor = Anchor.Center;
				}
				GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.4f, 1f), buttonContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("OK"), Alignment.Center, "GUIButtonLarge", null);
				GUIButton guibutton2 = guibutton;
				GUIButton.OnClickedHandler onClicked;
				if ((onClicked = ObjectiveManager.<>O.<2>__CloseInfoFrame) == null)
				{
					onClicked = (ObjectiveManager.<>O.<2>__CloseInfoFrame = new GUIButton.OnClickedHandler(ObjectiveManager.CloseInfoFrame));
				}
				guibutton2.OnClicked = onClicked;
			}
			infoBlock.RectTransform.NonScaledSize = new Point(infoBlock.Rect.Width, (int)((float)infoContent.Children.Sum((GUIComponent c) => c.Rect.Height + infoContent.AbsoluteSpacing) / infoContent.RectTransform.RelativeSize.Y));
			SoundPlayer.PlayUISound(GUISoundType.UIMessage);
			return background;
		}

		// Token: 0x06001084 RID: 4228 RVA: 0x0009D1A8 File Offset: 0x0009B3A8
		private static void LoadVideo(ObjectiveManager.Segment segment)
		{
			if (segment.AutoPlayVideo == AutoPlayVideo.Yes)
			{
				VideoPlayer videoPlayer = ObjectiveManager.VideoPlayer;
				string contentPath = segment.VideoContent.ContentPath;
				VideoPlayer.VideoSettings videoSettings = new VideoPlayer.VideoSettings(segment.VideoContent.FileName);
				VideoPlayer.TextSettings textSettings = new VideoPlayer.TextSettings(segment.VideoContent.TextTag, segment.VideoContent.Width);
				Identifier id = segment.Id;
				bool startPlayback = true;
				LocalizedString objectiveText = segment.ObjectiveText;
				Action onStop;
				if ((onStop = ObjectiveManager.<>O.<1>__StopCurrentContentSegment) == null)
				{
					onStop = (ObjectiveManager.<>O.<1>__StopCurrentContentSegment = new Action(ObjectiveManager.StopCurrentContentSegment));
				}
				videoPlayer.LoadContent(contentPath, videoSettings, textSettings, id, startPlayback, objectiveText, onStop);
				return;
			}
			ObjectiveManager.VideoPlayer.LoadContent(segment.VideoContent.ContentPath, new VideoPlayer.VideoSettings(segment.VideoContent.FileName), null, segment.Id, true, string.Empty, null);
		}

		// Token: 0x06001085 RID: 4229 RVA: 0x0009D25E File Offset: 0x0009B45E
		private static void LoadActiveContentVideo()
		{
			ObjectiveManager.LoadVideo(ObjectiveManager.ActiveContentSegment);
		}

		// Token: 0x06001087 RID: 4231 RVA: 0x0009D280 File Offset: 0x0009B480
		[CompilerGenerated]
		internal static void <AddToObjectiveList>g__SetTransparent|37_2(GUIComponent component)
		{
			component.Color = (component.HoverColor = (component.PressedColor = (component.SelectedColor = Color.Transparent)));
		}

		// Token: 0x06001088 RID: 4232 RVA: 0x0009D2B4 File Offset: 0x0009B4B4
		[CompilerGenerated]
		internal static void <AddToObjectiveList>g__SetButtonBehavior|37_3(ObjectiveManager.Segment segment)
		{
			segment.ObjectiveButton.CanBeFocused = (segment.SegmentType != SegmentType.Objective);
			segment.ObjectiveButton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				if (segment.SegmentType == SegmentType.InfoBox)
				{
					if (segment.AutoPlayVideo == AutoPlayVideo.Yes)
					{
						ObjectiveManager.ReplaySegmentVideo(segment);
					}
					else
					{
						ObjectiveManager.ShowSegmentText(segment);
					}
				}
				else if (segment.SegmentType == SegmentType.MessageBox)
				{
					Action onClickObjective = segment.OnClickObjective;
					if (onClickObjective != null)
					{
						onClickObjective();
					}
				}
				return true;
			};
		}

		// Token: 0x0400080F RID: 2063
		private const float ObjectiveComponentAnimationTime = 1.5f;

		// Token: 0x04000813 RID: 2067
		private static readonly List<ObjectiveManager.Segment> activeObjectives = new List<ObjectiveManager.Segment>();

		// Token: 0x04000814 RID: 2068
		private static GUIComponent infoBox;

		// Token: 0x04000815 RID: 2069
		private static Action infoBoxClosedCallback;

		// Token: 0x04000816 RID: 2070
		private static ObjectiveManager.ScreenSettings screenSettings;

		// Token: 0x04000817 RID: 2071
		private static GUILayoutGroup objectiveGroup;

		// Token: 0x04000818 RID: 2072
		private static LocalizedString objectiveTextTranslated;

		// Token: 0x020008EB RID: 2283
		public class Segment
		{
			// Token: 0x17001A45 RID: 6725
			// (get) Token: 0x06007021 RID: 28705 RVA: 0x00368B76 File Offset: 0x00366D76
			// (set) Token: 0x06007022 RID: 28706 RVA: 0x00368B7E File Offset: 0x00366D7E
			public bool IsCompleted { get; set; }

			// Token: 0x17001A46 RID: 6726
			// (get) Token: 0x06007023 RID: 28707 RVA: 0x00368B87 File Offset: 0x00366D87
			// (set) Token: 0x06007024 RID: 28708 RVA: 0x00368B8F File Offset: 0x00366D8F
			public bool CanBeCompleted { get; set; }

			// Token: 0x17001A47 RID: 6727
			// (get) Token: 0x06007025 RID: 28709 RVA: 0x00368B98 File Offset: 0x00366D98
			// (set) Token: 0x06007026 RID: 28710 RVA: 0x00368BA0 File Offset: 0x00366DA0
			public Identifier ParentId { get; set; }

			// Token: 0x17001A48 RID: 6728
			// (get) Token: 0x06007027 RID: 28711 RVA: 0x00368BA9 File Offset: 0x00366DA9
			// (set) Token: 0x06007028 RID: 28712 RVA: 0x00368BB1 File Offset: 0x00366DB1
			public SegmentType SegmentType { get; private set; }

			// Token: 0x06007029 RID: 28713 RVA: 0x00368BBA File Offset: 0x00366DBA
			public static ObjectiveManager.Segment CreateInfoBoxSegment(Identifier id, Identifier objectiveTextTag, AutoPlayVideo autoPlayVideo, ObjectiveManager.Segment.Text textContent = default(ObjectiveManager.Segment.Text), ObjectiveManager.Segment.Video videoContent = default(ObjectiveManager.Segment.Video))
			{
				return new ObjectiveManager.Segment(id, objectiveTextTag, autoPlayVideo, textContent, videoContent);
			}

			// Token: 0x0600702A RID: 28714 RVA: 0x00368BC7 File Offset: 0x00366DC7
			public static ObjectiveManager.Segment CreateMessageBoxSegment(Identifier id, Identifier objectiveTextTag, Action onClickObjective)
			{
				return new ObjectiveManager.Segment(id, objectiveTextTag, onClickObjective);
			}

			// Token: 0x0600702B RID: 28715 RVA: 0x00368BD1 File Offset: 0x00366DD1
			public static ObjectiveManager.Segment CreateObjectiveSegment(Identifier id, Identifier objectiveTextTag)
			{
				return new ObjectiveManager.Segment(id, objectiveTextTag);
			}

			// Token: 0x0600702C RID: 28716 RVA: 0x00368BDC File Offset: 0x00366DDC
			private Segment(Identifier id, Identifier objectiveTextTag, AutoPlayVideo autoPlayVideo, ObjectiveManager.Segment.Text textContent = default(ObjectiveManager.Segment.Text), ObjectiveManager.Segment.Video videoContent = default(ObjectiveManager.Segment.Video))
			{
				this.Id = id;
				this.ObjectiveText = TextManager.ParseInputTypes(TextManager.Get(objectiveTextTag).Fallback(objectiveTextTag.Value, true), false);
				this.AutoPlayVideo = autoPlayVideo;
				this.TextContent = textContent;
				this.VideoContent = videoContent;
				this.SegmentType = SegmentType.InfoBox;
			}

			// Token: 0x0600702D RID: 28717 RVA: 0x00368C38 File Offset: 0x00366E38
			private Segment(Identifier id, Identifier objectiveTextTag, Action onClickObjective)
			{
				this.Id = id;
				this.ObjectiveText = TextManager.ParseInputTypes(TextManager.Get(objectiveTextTag).Fallback(objectiveTextTag.Value, true), false);
				this.OnClickObjective = onClickObjective;
				this.SegmentType = SegmentType.MessageBox;
			}

			// Token: 0x0600702E RID: 28718 RVA: 0x00368C84 File Offset: 0x00366E84
			private Segment(Identifier id, Identifier objectiveTextTag)
			{
				this.Id = id;
				this.ObjectiveText = TextManager.ParseInputTypes(TextManager.Get(objectiveTextTag).Fallback(objectiveTextTag.Value, true), false);
				this.SegmentType = SegmentType.Objective;
			}

			// Token: 0x0600702F RID: 28719 RVA: 0x00368CBE File Offset: 0x00366EBE
			public void ConnectMessageBox(ObjectiveManager.Segment messageBoxSegment)
			{
				this.SegmentType = SegmentType.MessageBox;
				this.OnClickObjective = messageBoxSegment.OnClickObjective;
			}

			// Token: 0x04003FC5 RID: 16325
			private const int DefaultWidth = 450;

			// Token: 0x04003FC6 RID: 16326
			private const int DefaultHeight = 80;

			// Token: 0x04003FC7 RID: 16327
			public GUIImage ObjectiveStateIndicator;

			// Token: 0x04003FC8 RID: 16328
			public GUIButton ObjectiveButton;

			// Token: 0x04003FC9 RID: 16329
			public GUITextBlock LinkedTextBlock;

			// Token: 0x04003FCA RID: 16330
			public LocalizedString ObjectiveText;

			// Token: 0x04003FCB RID: 16331
			public readonly Identifier Id;

			// Token: 0x04003FCC RID: 16332
			public readonly ObjectiveManager.Segment.Text TextContent;

			// Token: 0x04003FCD RID: 16333
			public readonly ObjectiveManager.Segment.Video VideoContent;

			// Token: 0x04003FCE RID: 16334
			public readonly AutoPlayVideo AutoPlayVideo;

			// Token: 0x04003FCF RID: 16335
			public Action OnClickObjective;

			// Token: 0x0200152B RID: 5419
			public readonly struct Text : IEquatable<ObjectiveManager.Segment.Text>
			{
				// Token: 0x06009CB7 RID: 40119 RVA: 0x003EB06E File Offset: 0x003E926E
				public Text(Identifier Tag, int Width = 450, int Height = 80, Anchor Anchor = Anchor.Center)
				{
					this.Tag = Tag;
					this.Width = Width;
					this.Height = Height;
					this.Anchor = Anchor;
				}

				// Token: 0x17001D80 RID: 7552
				// (get) Token: 0x06009CB8 RID: 40120 RVA: 0x003EB08D File Offset: 0x003E928D
				// (set) Token: 0x06009CB9 RID: 40121 RVA: 0x003EB095 File Offset: 0x003E9295
				public Identifier Tag { get; set; }

				// Token: 0x17001D81 RID: 7553
				// (get) Token: 0x06009CBA RID: 40122 RVA: 0x003EB09E File Offset: 0x003E929E
				// (set) Token: 0x06009CBB RID: 40123 RVA: 0x003EB0A6 File Offset: 0x003E92A6
				public int Width { get; set; }

				// Token: 0x17001D82 RID: 7554
				// (get) Token: 0x06009CBC RID: 40124 RVA: 0x003EB0AF File Offset: 0x003E92AF
				// (set) Token: 0x06009CBD RID: 40125 RVA: 0x003EB0B7 File Offset: 0x003E92B7
				public int Height { get; set; }

				// Token: 0x17001D83 RID: 7555
				// (get) Token: 0x06009CBE RID: 40126 RVA: 0x003EB0C0 File Offset: 0x003E92C0
				// (set) Token: 0x06009CBF RID: 40127 RVA: 0x003EB0C8 File Offset: 0x003E92C8
				public Anchor Anchor { get; set; }

				// Token: 0x06009CC0 RID: 40128 RVA: 0x003EB0D4 File Offset: 0x003E92D4
				[CompilerGenerated]
				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("Text");
					stringBuilder.Append(" { ");
					if (this.PrintMembers(stringBuilder))
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append('}');
					return stringBuilder.ToString();
				}

				// Token: 0x06009CC1 RID: 40129 RVA: 0x003EB120 File Offset: 0x003E9320
				[CompilerGenerated]
				private bool PrintMembers(StringBuilder builder)
				{
					builder.Append("Tag = ");
					builder.Append(this.Tag.ToString());
					builder.Append(", Width = ");
					builder.Append(this.Width.ToString());
					builder.Append(", Height = ");
					builder.Append(this.Height.ToString());
					builder.Append(", Anchor = ");
					builder.Append(this.Anchor.ToString());
					return true;
				}

				// Token: 0x06009CC2 RID: 40130 RVA: 0x003EB1CA File Offset: 0x003E93CA
				[CompilerGenerated]
				public static bool operator !=(ObjectiveManager.Segment.Text left, ObjectiveManager.Segment.Text right)
				{
					return !(left == right);
				}

				// Token: 0x06009CC3 RID: 40131 RVA: 0x003EB1D6 File Offset: 0x003E93D6
				[CompilerGenerated]
				public static bool operator ==(ObjectiveManager.Segment.Text left, ObjectiveManager.Segment.Text right)
				{
					return left.Equals(right);
				}

				// Token: 0x06009CC4 RID: 40132 RVA: 0x003EB1E0 File Offset: 0x003E93E0
				[CompilerGenerated]
				public override int GetHashCode()
				{
					return ((EqualityComparer<Identifier>.Default.GetHashCode(this.<Tag>k__BackingField) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Width>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Height>k__BackingField)) * -1521134295 + EqualityComparer<Anchor>.Default.GetHashCode(this.<Anchor>k__BackingField);
				}

				// Token: 0x06009CC5 RID: 40133 RVA: 0x003EB242 File Offset: 0x003E9442
				[CompilerGenerated]
				public override bool Equals(object obj)
				{
					return obj is ObjectiveManager.Segment.Text && this.Equals((ObjectiveManager.Segment.Text)obj);
				}

				// Token: 0x06009CC6 RID: 40134 RVA: 0x003EB25C File Offset: 0x003E945C
				[CompilerGenerated]
				public bool Equals(ObjectiveManager.Segment.Text other)
				{
					return EqualityComparer<Identifier>.Default.Equals(this.<Tag>k__BackingField, other.<Tag>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Width>k__BackingField, other.<Width>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Height>k__BackingField, other.<Height>k__BackingField) && EqualityComparer<Anchor>.Default.Equals(this.<Anchor>k__BackingField, other.<Anchor>k__BackingField);
				}

				// Token: 0x06009CC7 RID: 40135 RVA: 0x003EB2C9 File Offset: 0x003E94C9
				[CompilerGenerated]
				public void Deconstruct(out Identifier Tag, out int Width, out int Height, out Anchor Anchor)
				{
					Tag = this.Tag;
					Width = this.Width;
					Height = this.Height;
					Anchor = this.Anchor;
				}
			}

			// Token: 0x0200152C RID: 5420
			public readonly struct Video : IEquatable<ObjectiveManager.Segment.Video>
			{
				// Token: 0x06009CC8 RID: 40136 RVA: 0x003EB2F0 File Offset: 0x003E94F0
				public Video(string FullPath, Identifier TextTag, int Width = 450, int Height = 80)
				{
					this.FullPath = FullPath;
					this.TextTag = TextTag;
					this.Width = Width;
					this.Height = Height;
				}

				// Token: 0x17001D84 RID: 7556
				// (get) Token: 0x06009CC9 RID: 40137 RVA: 0x003EB30F File Offset: 0x003E950F
				// (set) Token: 0x06009CCA RID: 40138 RVA: 0x003EB317 File Offset: 0x003E9517
				public string FullPath { get; set; }

				// Token: 0x17001D85 RID: 7557
				// (get) Token: 0x06009CCB RID: 40139 RVA: 0x003EB320 File Offset: 0x003E9520
				// (set) Token: 0x06009CCC RID: 40140 RVA: 0x003EB328 File Offset: 0x003E9528
				public Identifier TextTag { get; set; }

				// Token: 0x17001D86 RID: 7558
				// (get) Token: 0x06009CCD RID: 40141 RVA: 0x003EB331 File Offset: 0x003E9531
				// (set) Token: 0x06009CCE RID: 40142 RVA: 0x003EB339 File Offset: 0x003E9539
				public int Width { get; set; }

				// Token: 0x17001D87 RID: 7559
				// (get) Token: 0x06009CCF RID: 40143 RVA: 0x003EB342 File Offset: 0x003E9542
				// (set) Token: 0x06009CD0 RID: 40144 RVA: 0x003EB34A File Offset: 0x003E954A
				public int Height { get; set; }

				// Token: 0x17001D88 RID: 7560
				// (get) Token: 0x06009CD1 RID: 40145 RVA: 0x003EB353 File Offset: 0x003E9553
				public string FileName
				{
					get
					{
						return Path.GetFileName(this.FullPath.CleanUpPath());
					}
				}

				// Token: 0x17001D89 RID: 7561
				// (get) Token: 0x06009CD2 RID: 40146 RVA: 0x003EB365 File Offset: 0x003E9565
				public string ContentPath
				{
					get
					{
						return Path.GetDirectoryName(this.FullPath.CleanUpPath());
					}
				}

				// Token: 0x06009CD3 RID: 40147 RVA: 0x003EB378 File Offset: 0x003E9578
				[CompilerGenerated]
				public override string ToString()
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.Append("Video");
					stringBuilder.Append(" { ");
					if (this.PrintMembers(stringBuilder))
					{
						stringBuilder.Append(' ');
					}
					stringBuilder.Append('}');
					return stringBuilder.ToString();
				}

				// Token: 0x06009CD4 RID: 40148 RVA: 0x003EB3C4 File Offset: 0x003E95C4
				[CompilerGenerated]
				private bool PrintMembers(StringBuilder builder)
				{
					builder.Append("FullPath = ");
					builder.Append(this.FullPath);
					builder.Append(", TextTag = ");
					builder.Append(this.TextTag.ToString());
					builder.Append(", Width = ");
					builder.Append(this.Width.ToString());
					builder.Append(", Height = ");
					builder.Append(this.Height.ToString());
					builder.Append(", FileName = ");
					builder.Append(this.FileName);
					builder.Append(", ContentPath = ");
					builder.Append(this.ContentPath);
					return true;
				}

				// Token: 0x06009CD5 RID: 40149 RVA: 0x003EB492 File Offset: 0x003E9692
				[CompilerGenerated]
				public static bool operator !=(ObjectiveManager.Segment.Video left, ObjectiveManager.Segment.Video right)
				{
					return !(left == right);
				}

				// Token: 0x06009CD6 RID: 40150 RVA: 0x003EB49E File Offset: 0x003E969E
				[CompilerGenerated]
				public static bool operator ==(ObjectiveManager.Segment.Video left, ObjectiveManager.Segment.Video right)
				{
					return left.Equals(right);
				}

				// Token: 0x06009CD7 RID: 40151 RVA: 0x003EB4A8 File Offset: 0x003E96A8
				[CompilerGenerated]
				public override int GetHashCode()
				{
					return ((EqualityComparer<string>.Default.GetHashCode(this.<FullPath>k__BackingField) * -1521134295 + EqualityComparer<Identifier>.Default.GetHashCode(this.<TextTag>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Width>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<Height>k__BackingField);
				}

				// Token: 0x06009CD8 RID: 40152 RVA: 0x003EB50A File Offset: 0x003E970A
				[CompilerGenerated]
				public override bool Equals(object obj)
				{
					return obj is ObjectiveManager.Segment.Video && this.Equals((ObjectiveManager.Segment.Video)obj);
				}

				// Token: 0x06009CD9 RID: 40153 RVA: 0x003EB524 File Offset: 0x003E9724
				[CompilerGenerated]
				public bool Equals(ObjectiveManager.Segment.Video other)
				{
					return EqualityComparer<string>.Default.Equals(this.<FullPath>k__BackingField, other.<FullPath>k__BackingField) && EqualityComparer<Identifier>.Default.Equals(this.<TextTag>k__BackingField, other.<TextTag>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Width>k__BackingField, other.<Width>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<Height>k__BackingField, other.<Height>k__BackingField);
				}

				// Token: 0x06009CDA RID: 40154 RVA: 0x003EB591 File Offset: 0x003E9791
				[CompilerGenerated]
				public void Deconstruct(out string FullPath, out Identifier TextTag, out int Width, out int Height)
				{
					FullPath = this.FullPath;
					TextTag = this.TextTag;
					Width = this.Width;
					Height = this.Height;
				}
			}
		}

		// Token: 0x020008EC RID: 2284
		private readonly struct ScreenSettings : IEquatable<ObjectiveManager.ScreenSettings>
		{
			// Token: 0x06007030 RID: 28720 RVA: 0x00368CD3 File Offset: 0x00366ED3
			public ScreenSettings(Point ScreenResolution = default(Point), float UiScale = 0f, WindowMode WindowMode = WindowMode.Windowed)
			{
				this.ScreenResolution = ScreenResolution;
				this.UiScale = UiScale;
				this.WindowMode = WindowMode;
			}

			// Token: 0x17001A49 RID: 6729
			// (get) Token: 0x06007031 RID: 28721 RVA: 0x00368CEA File Offset: 0x00366EEA
			// (set) Token: 0x06007032 RID: 28722 RVA: 0x00368CF2 File Offset: 0x00366EF2
			public Point ScreenResolution { get; set; }

			// Token: 0x17001A4A RID: 6730
			// (get) Token: 0x06007033 RID: 28723 RVA: 0x00368CFB File Offset: 0x00366EFB
			// (set) Token: 0x06007034 RID: 28724 RVA: 0x00368D03 File Offset: 0x00366F03
			public float UiScale { get; set; }

			// Token: 0x17001A4B RID: 6731
			// (get) Token: 0x06007035 RID: 28725 RVA: 0x00368D0C File Offset: 0x00366F0C
			// (set) Token: 0x06007036 RID: 28726 RVA: 0x00368D14 File Offset: 0x00366F14
			public WindowMode WindowMode { get; set; }

			// Token: 0x06007037 RID: 28727 RVA: 0x00368D20 File Offset: 0x00366F20
			public bool HaveChanged()
			{
				return GameMain.GraphicsWidth != this.ScreenResolution.X || GameMain.GraphicsHeight != this.ScreenResolution.Y || GUI.Scale != this.UiScale || GameSettings.CurrentConfig.Graphics.DisplayMode != this.WindowMode;
			}

			// Token: 0x06007038 RID: 28728 RVA: 0x00368D7C File Offset: 0x00366F7C
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("ScreenSettings");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06007039 RID: 28729 RVA: 0x00368DC8 File Offset: 0x00366FC8
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("ScreenResolution = ");
				builder.Append(this.ScreenResolution.ToString());
				builder.Append(", UiScale = ");
				builder.Append(this.UiScale.ToString());
				builder.Append(", WindowMode = ");
				builder.Append(this.WindowMode.ToString());
				return true;
			}

			// Token: 0x0600703A RID: 28730 RVA: 0x00368E4B File Offset: 0x0036704B
			[CompilerGenerated]
			public static bool operator !=(ObjectiveManager.ScreenSettings left, ObjectiveManager.ScreenSettings right)
			{
				return !(left == right);
			}

			// Token: 0x0600703B RID: 28731 RVA: 0x00368E57 File Offset: 0x00367057
			[CompilerGenerated]
			public static bool operator ==(ObjectiveManager.ScreenSettings left, ObjectiveManager.ScreenSettings right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600703C RID: 28732 RVA: 0x00368E61 File Offset: 0x00367061
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Point>.Default.GetHashCode(this.<ScreenResolution>k__BackingField) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<UiScale>k__BackingField)) * -1521134295 + EqualityComparer<WindowMode>.Default.GetHashCode(this.<WindowMode>k__BackingField);
			}

			// Token: 0x0600703D RID: 28733 RVA: 0x00368EA1 File Offset: 0x003670A1
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is ObjectiveManager.ScreenSettings && this.Equals((ObjectiveManager.ScreenSettings)obj);
			}

			// Token: 0x0600703E RID: 28734 RVA: 0x00368EBC File Offset: 0x003670BC
			[CompilerGenerated]
			public bool Equals(ObjectiveManager.ScreenSettings other)
			{
				return EqualityComparer<Point>.Default.Equals(this.<ScreenResolution>k__BackingField, other.<ScreenResolution>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<UiScale>k__BackingField, other.<UiScale>k__BackingField) && EqualityComparer<WindowMode>.Default.Equals(this.<WindowMode>k__BackingField, other.<WindowMode>k__BackingField);
			}

			// Token: 0x0600703F RID: 28735 RVA: 0x00368F11 File Offset: 0x00367111
			[CompilerGenerated]
			public void Deconstruct(out Point ScreenResolution, out float UiScale, out WindowMode WindowMode)
			{
				ScreenResolution = this.ScreenResolution;
				UiScale = this.UiScale;
				WindowMode = this.WindowMode;
			}
		}

		// Token: 0x020008ED RID: 2285
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003FD7 RID: 16343
			public static Action <0>__LoadActiveContentVideo;

			// Token: 0x04003FD8 RID: 16344
			public static Action <1>__StopCurrentContentSegment;

			// Token: 0x04003FD9 RID: 16345
			public static GUIButton.OnClickedHandler <2>__CloseInfoFrame;
		}
	}
}
