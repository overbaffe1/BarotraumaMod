using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C9 RID: 1481
	internal class Steering : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x1700178A RID: 6026
		// (get) Token: 0x06005D91 RID: 23953 RVA: 0x0030A83A File Offset: 0x00308A3A
		// (set) Token: 0x06005D92 RID: 23954 RVA: 0x0030A842 File Offset: 0x00308A42
		public GUIComponent ControlContainer { get; private set; }

		// Token: 0x1700178B RID: 6027
		// (get) Token: 0x06005D93 RID: 23955 RVA: 0x0030A84B File Offset: 0x00308A4B
		// (set) Token: 0x06005D94 RID: 23956 RVA: 0x0030A863 File Offset: 0x00308A63
		[Serialize(false, IsPropertySaveable.Yes, "", "", false, AlwaysUseInstanceValues = true)]
		public bool LevelStartSelected
		{
			get
			{
				GUITickBox guitickBox = this.levelStartTickBox;
				if (guitickBox == null)
				{
					return this.levelStartSelected;
				}
				return guitickBox.Selected;
			}
			set
			{
				this.TrySetTickBoxSelected(this.levelStartTickBox, ref this.levelStartSelected, value);
			}
		}

		// Token: 0x1700178C RID: 6028
		// (get) Token: 0x06005D95 RID: 23957 RVA: 0x0030A878 File Offset: 0x00308A78
		// (set) Token: 0x06005D96 RID: 23958 RVA: 0x0030A890 File Offset: 0x00308A90
		[Serialize(false, IsPropertySaveable.Yes, "", "", false, AlwaysUseInstanceValues = true)]
		public bool LevelEndSelected
		{
			get
			{
				GUITickBox guitickBox = this.levelEndTickBox;
				if (guitickBox == null)
				{
					return this.levelEndSelected;
				}
				return guitickBox.Selected;
			}
			set
			{
				this.TrySetTickBoxSelected(this.levelEndTickBox, ref this.levelEndSelected, value);
			}
		}

		// Token: 0x1700178D RID: 6029
		// (get) Token: 0x06005D97 RID: 23959 RVA: 0x0030A8A5 File Offset: 0x00308AA5
		// (set) Token: 0x06005D98 RID: 23960 RVA: 0x0030A8BD File Offset: 0x00308ABD
		[Serialize(false, IsPropertySaveable.Yes, "", "", false, AlwaysUseInstanceValues = true)]
		public bool MaintainPos
		{
			get
			{
				GUITickBox guitickBox = this.maintainPosTickBox;
				if (guitickBox == null)
				{
					return this.maintainPos;
				}
				return guitickBox.Selected;
			}
			set
			{
				this.TrySetTickBoxSelected(this.maintainPosTickBox, ref this.maintainPos, value);
			}
		}

		// Token: 0x1700178E RID: 6030
		// (get) Token: 0x06005D99 RID: 23961 RVA: 0x0030A8D2 File Offset: 0x00308AD2
		// (set) Token: 0x06005D9A RID: 23962 RVA: 0x0030A8E0 File Offset: 0x00308AE0
		public float? SteerRadius
		{
			get
			{
				return new float?(this.steerRadius);
			}
			set
			{
				this.steerRadius = (value ?? ((float)(this.steerArea.Rect.Width / 2)));
			}
		}

		// Token: 0x1700178F RID: 6031
		// (get) Token: 0x06005D9B RID: 23963 RVA: 0x0030A91A File Offset: 0x00308B1A
		// (set) Token: 0x06005D9C RID: 23964 RVA: 0x0030A922 File Offset: 0x00308B22
		public bool DisableControls
		{
			get
			{
				return this.disableControls;
			}
			set
			{
				if (this.disableControls == value)
				{
					return;
				}
				this.disableControls = value;
				this.UpdateGUIElements();
			}
		}

		// Token: 0x17001790 RID: 6032
		// (get) Token: 0x06005D9D RID: 23965 RVA: 0x0030A93B File Offset: 0x00308B3B
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06005D9E RID: 23966 RVA: 0x0030A940 File Offset: 0x00308B40
		protected override void CreateGUI()
		{
			this.ControlContainer = new GUIFrame(new RectTransform(new Vector2(Sonar.controlBoxSize.X, 1f - Sonar.controlBoxSize.Y * 2f), base.GuiFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "ItemUI", null);
			GUIFrame paddedControlContainer = new GUIFrame(new RectTransform(this.ControlContainer.Rect.Size - GUIStyle.ItemFrameMargin, this.ControlContainer.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, null, null);
			GUIFrame steeringModeArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.4f), paddedControlContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.steeringModeSwitch = new GUIButton(new RectTransform(new Vector2(0.2f, 1f), steeringModeArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, Alignment.Center, "SwitchVertical", null)
			{
				UserData = UIHighlightAction.ElementId.SteeringModeSwitch,
				Selected = this.autoPilot,
				Enabled = true,
				ClickSound = GUISoundType.UISwitch,
				OnClicked = delegate(GUIButton button, object data)
				{
					button.Selected = !button.Selected;
					this.AutoPilot = button.Selected;
					if (GameMain.Client != null)
					{
						this.unsentChanges = true;
						this.user = Character.Controlled;
					}
					return true;
				}
			};
			GUIFrame steeringModeRightSide = new GUIFrame(new RectTransform(new Vector2(1f - this.steeringModeSwitch.RectTransform.RelativeSize.X, 0.8f), steeringModeArea.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(this.steeringModeSwitch.RectTransform.RelativeSize.X, 0f)
			}, null, null);
			this.manualPilotIndicator = new GUITickBox(new RectTransform(new Vector2(1f, 0.45f), steeringModeRightSide.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SteeringManual"), GUIStyle.SubHeadingFont, "IndicatorLightRedSmall")
			{
				Selected = !this.autoPilot,
				Enabled = false
			};
			this.autopilotIndicator = new GUITickBox(new RectTransform(new Vector2(1f, 0.45f), steeringModeRightSide.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SteeringAutoPilot"), GUIStyle.SubHeadingFont, "IndicatorLightRedSmall")
			{
				Selected = this.autoPilot,
				Enabled = false
			};
			this.manualPilotIndicator.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			this.autopilotIndicator.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				this.manualPilotIndicator.TextBlock,
				this.autopilotIndicator.TextBlock
			});
			GUIFrame autoPilotControls = new GUIFrame(new RectTransform(new Vector2(0.75f, 0.62f), paddedControlContainer.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), "OutlineFrame", null);
			GUIFrame paddedAutoPilotControls = new GUIFrame(new RectTransform(new Vector2(0.92f, 0.88f), autoPilotControls.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			int textLimit = (int)((float)paddedAutoPilotControls.Rect.Width * 0.75f);
			this.maintainPosTickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.333f), paddedAutoPilotControls.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), ToolBox.LimitString(TextManager.Get("SteeringMaintainPos"), GUIStyle.SmallFont, textLimit), GUIStyle.SmallFont, "GUIRadioButton")
			{
				UserData = UIHighlightAction.ElementId.MaintainPosTickBox,
				Enabled = this.autoPilot,
				Selected = this.maintainPos,
				OnSelected = delegate(GUITickBox tickBox)
				{
					if (this.maintainPos != tickBox.Selected)
					{
						this.unsentChanges = true;
						this.user = Character.Controlled;
						this.maintainPos = tickBox.Selected;
						if (this.maintainPos)
						{
							if (this.controlledSub == null)
							{
								this.posToMaintain = null;
							}
							else
							{
								this.posToMaintain = new Vector2?(this.controlledSub.WorldPosition);
							}
						}
						else if (!this.LevelEndSelected && !this.LevelStartSelected)
						{
							this.AutoPilot = false;
						}
						if (!this.maintainPos)
						{
							this.posToMaintain = null;
						}
					}
					return true;
				}
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.333f), paddedAutoPilotControls.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			GameSession gameSession = GameMain.GameSession;
			this.levelStartTickBox = new GUITickBox(rectT, (((gameSession != null) ? gameSession.StartLocation : null) == null) ? "" : ToolBox.LimitString(GameMain.GameSession.StartLocation.DisplayName, GUIStyle.SmallFont, textLimit), GUIStyle.SmallFont, "GUIRadioButton")
			{
				Enabled = this.autoPilot,
				Selected = this.levelStartSelected,
				OnSelected = delegate(GUITickBox tickBox)
				{
					if (this.levelStartSelected != tickBox.Selected)
					{
						this.unsentChanges = true;
						this.user = Character.Controlled;
						this.levelStartSelected = tickBox.Selected;
						this.levelEndSelected = !this.levelStartSelected;
						if (this.levelStartSelected)
						{
							this.UpdatePath();
						}
						else if (!this.MaintainPos && !this.LevelEndSelected)
						{
							this.AutoPilot = false;
						}
					}
					return true;
				}
			};
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.333f), paddedAutoPilotControls.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal);
			GameSession gameSession2 = GameMain.GameSession;
			GUITickBox guitickBox = new GUITickBox(rectT2, (((gameSession2 != null) ? gameSession2.EndLocation : null) == null || Level.IsLoadedOutpost) ? "" : ToolBox.LimitString(GameMain.GameSession.EndLocation.DisplayName, GUIStyle.SmallFont, textLimit), GUIStyle.SmallFont, "GUIRadioButton");
			guitickBox.Enabled = this.autoPilot;
			guitickBox.Selected = this.levelEndSelected;
			GameSession gameSession3 = GameMain.GameSession;
			guitickBox.Visible = (((gameSession3 != null) ? gameSession3.EndLocation : null) != null);
			guitickBox.OnSelected = delegate(GUITickBox tickBox)
			{
				if (this.levelEndSelected != tickBox.Selected)
				{
					this.unsentChanges = true;
					this.user = Character.Controlled;
					this.levelEndSelected = tickBox.Selected;
					this.levelStartSelected = !this.levelEndSelected;
					if (this.levelEndSelected)
					{
						this.UpdatePath();
					}
					else if (!this.MaintainPos && !this.LevelStartSelected)
					{
						this.AutoPilot = false;
					}
				}
				return true;
			};
			this.levelEndTickBox = guitickBox;
			this.maintainPosTickBox.RectTransform.IsFixedSize = (this.levelStartTickBox.RectTransform.IsFixedSize = (this.levelEndTickBox.RectTransform.IsFixedSize = false));
			RectTransform rectTransform = this.maintainPosTickBox.RectTransform;
			RectTransform rectTransform2 = this.levelStartTickBox.RectTransform;
			RectTransform rectTransform3 = this.levelEndTickBox.RectTransform;
			Point point = new Point(int.MaxValue, paddedAutoPilotControls.Rect.Height / 3);
			rectTransform3.MaxSize = point;
			rectTransform.MaxSize = (rectTransform2.MaxSize = point);
			RectTransform rectTransform4 = this.maintainPosTickBox.RectTransform;
			RectTransform rectTransform5 = this.levelStartTickBox.RectTransform;
			point = (this.levelEndTickBox.RectTransform.MinSize = Point.Zero);
			rectTransform4.MinSize = (rectTransform5.MinSize = point);
			GUITextBlock.AutoScaleAndNormalize(false, true, new GUITextBlock[]
			{
				this.maintainPosTickBox.TextBlock,
				this.levelStartTickBox.TextBlock,
				this.levelEndTickBox.TextBlock
			});
			GUIRadioButtonGroup destinations = new GUIRadioButtonGroup();
			destinations.AddRadioButton(0, this.maintainPosTickBox);
			destinations.AddRadioButton(2, this.levelStartTickBox);
			destinations.AddRadioButton(1, this.levelEndTickBox);
			destinations.Selected = new int?(this.maintainPos ? 0 : (this.levelStartSelected ? 2 : 1));
			this.statusContainer = new GUIFrame(new RectTransform(Sonar.controlBoxSize, base.GuiFrame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = Sonar.controlBoxOffset
			}, "ItemUI", null);
			GUIFrame paddedStatusContainer = new GUIFrame(new RectTransform(this.statusContainer.Rect.Size - GUIStyle.ItemFrameMargin, this.statusContainer.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, null, null);
			List<GUIFrame> elements = GUI.CreateElements<GUIFrame>(3, new Vector2(1f, 0.333f), paddedStatusContainer.RectTransform, (RectTransform rt) => new GUIFrame(rt, null, null), Anchor.TopCenter, null, null, null, 0, 0.01f, null, 0, 0f, false);
			List<GUIComponent> leftElements = new List<GUIComponent>();
			List<GUIComponent> centerElements = new List<GUIComponent>();
			List<GUIComponent> rightElements = new List<GUIComponent>();
			GUIFont font;
			for (int i = 0; i < elements.Count; i++)
			{
				GUIFrame e2 = elements[i];
				GUILayoutGroup group = new GUILayoutGroup(new RectTransform(Vector2.One, e2.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					RelativeSpacing = 0.01f,
					Stretch = true
				};
				GUIFrame left = new GUIFrame(new RectTransform(new Vector2(0.45f, 1f), group.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				GUIFrame center = new GUIFrame(new RectTransform(new Vector2(0.15f, 1f), group.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				GUIFrame right = new GUIFrame(new RectTransform(new Vector2(0.4f, 0.8f), group.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				leftElements.Add(left);
				centerElements.Add(center);
				rightElements.Add(right);
				LocalizedString leftText = string.Empty;
				LocalizedString centerText = string.Empty;
				GUITextBlock.TextGetterHandler rightTextGetter = null;
				switch (i)
				{
				case 0:
					leftText = TextManager.Get("DescentVelocity");
					centerText = TextManager.Get("KilometersPerHour");
					rightTextGetter = delegate()
					{
						Vector2 vel = (this.controlledSub == null) ? Vector2.Zero : this.controlledSub.Velocity;
						float realWorldVel = ConvertUnits.ToDisplayUnits(vel.Y * Physics.DisplayToRealWorldRatio) * 3.6f;
						return (-realWorldVel).ToString("0.0");
					};
					break;
				case 1:
					leftText = TextManager.Get("Velocity");
					centerText = TextManager.Get("KilometersPerHour");
					rightTextGetter = delegate()
					{
						Vector2 vel = (this.controlledSub == null) ? Vector2.Zero : this.controlledSub.Velocity;
						float realWorldVel = ConvertUnits.ToDisplayUnits(vel.X * Physics.DisplayToRealWorldRatio) * 3.6f;
						if (this.controlledSub != null && this.controlledSub.FlippedX)
						{
							realWorldVel *= -1f;
						}
						return realWorldVel.ToString("0.0");
					};
					break;
				case 2:
					leftText = TextManager.Get("Depth");
					centerText = TextManager.Get("Meter");
					rightTextGetter = delegate()
					{
						Level loaded = Level.Loaded;
						if (loaded != null && loaded.IsEndBiome)
						{
							return (Timing.TotalTime % 5.0 < 0.5) ? Rand.Range(-9000, 9000, Rand.RandSync.Unsynced).ToString() : "ERROR";
						}
						float realWorldDepth = (this.controlledSub == null) ? -1000f : this.controlledSub.RealWorldDepth;
						return ((int)realWorldDepth).ToString();
					};
					break;
				}
				RectTransform rectT3 = new RectTransform(Vector2.One, left.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = leftText;
				font = GUIStyle.SubHeadingFont;
				bool wrap = leftText.Contains(" ", StringComparison.Ordinal);
				new GUITextBlock(rectT3, text, null, font, Alignment.CenterRight, wrap, "", null);
				RectTransform rectT4 = new RectTransform(Vector2.One, center.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text2 = centerText;
				font = GUIStyle.Font;
				new GUITextBlock(rectT4, text2, null, font, Alignment.Center, false, "", null).Padding = Vector4.Zero;
				GUIFrame digitalFrame = new GUIFrame(new RectTransform(Vector2.One, right.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "DigitalFrameDark", null);
				new GUITextBlock(new RectTransform(Vector2.One * 0.85f, digitalFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "12345", new Color?(GUIStyle.TextColorDark), GUIStyle.DigitalFont, Alignment.CenterRight, false, "", null).TextGetter = rightTextGetter;
			}
			GUITextBlock.AutoScaleAndNormalize(leftElements.SelectMany((GUIComponent e) => e.GetAllChildren<GUITextBlock>()), true, false, null);
			GUITextBlock.AutoScaleAndNormalize(centerElements.SelectMany((GUIComponent e) => e.GetAllChildren<GUITextBlock>()), true, false, null);
			GUITextBlock.AutoScaleAndNormalize(rightElements.SelectMany((GUIComponent e) => e.GetAllChildren<GUITextBlock>()), true, false, null);
			float dockingButtonSize = 1.1f;
			float elementScale = 0.6f;
			this.dockingContainer = new GUIFrame(new RectTransform(Sonar.controlBoxSize, base.GuiFrame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Smallest)
			{
				RelativeOffset = new Vector2(Sonar.controlBoxOffset.X + 0.05f, -0.05f)
			}, null, null);
			this.dockText = TextManager.Get(new string[]
			{
				"label.navterminaldock",
				"captain.dock"
			});
			this.undockText = TextManager.Get(new string[]
			{
				"label.navterminalundock",
				"captain.undock"
			});
			this.dockingButton = new GUIButton(new RectTransform(new Vector2(elementScale), this.dockingContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), this.dockText, Alignment.Center, "PowerButton", null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					GameSession gameSession6 = GameMain.GameSession;
					bool flag;
					if (gameSession6 == null)
					{
						flag = false;
					}
					else
					{
						flag = gameSession6.Missions.Any((Mission m) => !m.AllowUndocking);
					}
					if (flag)
					{
						new GUIMessageBox("", TextManager.Get("undockingdisabledbymission"), null, null, GUIMessageBox.Type.Default);
						return false;
					}
					Steering.<>c__DisplayClass60_0 CS$<>8__locals1 = new Steering.<>c__DisplayClass60_0();
					CS$<>8__locals1.<>4__this = this;
					Steering.<>c__DisplayClass60_0 CS$<>8__locals2 = CS$<>8__locals1;
					GameSession gameSession7 = GameMain.GameSession;
					CS$<>8__locals2.campaign = ((gameSession7 != null) ? gameSession7.Campaign : null);
					if (CS$<>8__locals1.campaign != null)
					{
						if (Level.IsLoadedOutpost)
						{
							if (this.DockingSources.Any(delegate(DockingPort d)
							{
								if (d.Docked)
								{
									DockingPort dockingTarget3 = d.DockingTarget;
									bool? flag4;
									if (dockingTarget3 == null)
									{
										flag4 = null;
									}
									else
									{
										Submarine submarine3 = dockingTarget3.Item.Submarine;
										if (submarine3 == null)
										{
											flag4 = null;
										}
										else
										{
											SubmarineInfo info = submarine3.Info;
											flag4 = ((info != null) ? new bool?(info.IsOutpost) : null);
										}
									}
									bool? flag5 = flag4;
									return flag5.GetValueOrDefault();
								}
								return false;
							}))
							{
								if (!ObjectiveManager.AllActiveObjectivesCompleted())
								{
									this.exitOutpostPrompt = new GUIMessageBox("", TextManager.GetWithVariable("CampaignExitTutorialOutpostPrompt", "[locationname]", CS$<>8__locals1.campaign.Map.CurrentLocation.DisplayName, FormatCapitals.No), new LocalizedString[]
									{
										TextManager.Get("yes"),
										TextManager.Get("no")
									}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
									GUIButton guibutton5 = this.exitOutpostPrompt.Buttons[0];
									guibutton5.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton5.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton _, object _)
									{
										CS$<>8__locals1.<>4__this.exitOutpostPrompt.Close();
										return Steering.<CreateGUI>g__OpenMap|60_4(CS$<>8__locals1.campaign);
									}));
									GUIButton guibutton6 = this.exitOutpostPrompt.Buttons[1];
									guibutton6.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton6.OnClicked, new GUIButton.OnClickedHandler(this.exitOutpostPrompt.Close));
									return false;
								}
								return Steering.<CreateGUI>g__OpenMap|60_4(CS$<>8__locals1.campaign);
							}
						}
						if (!Level.IsLoadedOutpost && this.DockingModeEnabled && this.ActiveDockingSource != null && !this.ActiveDockingSource.Docked)
						{
							DockingPort dockingTarget = this.DockingTarget;
							Submarine submarine;
							if (dockingTarget == null)
							{
								submarine = null;
							}
							else
							{
								Item item = dockingTarget.Item;
								submarine = ((item != null) ? item.Submarine : null);
							}
							if (submarine == Level.Loaded.StartOutpost)
							{
								DockingPort dockingTarget2 = this.DockingTarget;
								bool? flag2;
								if (dockingTarget2 == null)
								{
									flag2 = null;
								}
								else
								{
									Item item2 = dockingTarget2.Item;
									if (item2 == null)
									{
										flag2 = null;
									}
									else
									{
										Submarine submarine2 = item2.Submarine;
										flag2 = ((submarine2 != null) ? new bool?(submarine2.Info.IsOutpost) : null);
									}
								}
								bool? flag3 = flag2;
								if (flag3.GetValueOrDefault())
								{
									List<Submarine> subsToLeaveBehind = CampaignMode.GetSubsToLeaveBehind(base.Item.Submarine);
									if (subsToLeaveBehind.Any<Submarine>())
									{
										this.enterOutpostPrompt = new GUIMessageBox(TextManager.GetWithVariable("enterlocation", "[locationname]", this.DockingTarget.Item.Submarine.Info.Name, FormatCapitals.No), TextManager.Get((subsToLeaveBehind.Count == 1) ? "LeaveSubBehind" : "LeaveSubsBehind"), new LocalizedString[]
										{
											TextManager.Get("yes"),
											TextManager.Get("no")
										}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
									}
									else
									{
										this.enterOutpostPrompt = new GUIMessageBox("", TextManager.GetWithVariable("campaignenteroutpostprompt", "[locationname]", this.DockingTarget.Item.Submarine.Info.Name, FormatCapitals.No), new LocalizedString[]
										{
											TextManager.Get("yes"),
											TextManager.Get("no")
										}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
									}
									GUIButton guibutton7 = this.enterOutpostPrompt.Buttons[0];
									guibutton7.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton7.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
									{
										this.<CreateGUI>g__SendDockingSignal|60_5();
										this.enterOutpostPrompt.Close();
										return true;
									}));
									GUIButton guibutton8 = this.enterOutpostPrompt.Buttons[1];
									guibutton8.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton8.OnClicked, new GUIButton.OnClickedHandler(this.enterOutpostPrompt.Close));
									return false;
								}
							}
						}
					}
					this.<CreateGUI>g__SendDockingSignal|60_5();
					return true;
				}
			};
			this.dockingButton.Font = GUIStyle.SubHeadingFont;
			this.dockingButton.TextBlock.RectTransform.MaxSize = new Point((int)((float)this.dockingButton.Rect.Width * 0.7f), int.MaxValue);
			this.dockingButton.TextBlock.AutoScaleHorizontal = true;
			GUIComponentStyle style = GUIStyle.GetComponentStyle("DockingButtonUp");
			UISprite uisprite = style.Sprites.FirstOrDefault<KeyValuePair<GUIComponent.ComponentState, List<UISprite>>>().Value.FirstOrDefault<UISprite>();
			Sprite buttonSprite = (uisprite != null) ? uisprite.Sprite : null;
			Point buttonSize = (buttonSprite != null) ? buttonSprite.size.ToPoint() : new Point(149, 52);
			Point horizontalButtonSize = buttonSize.Multiply(elementScale * GUI.Scale * dockingButtonSize);
			Point verticalButtonSize = horizontalButtonSize.Flip();
			GUIButton guibutton = new GUIButton(new RectTransform(verticalButtonSize, this.dockingContainer.RectTransform, Anchor.CenterLeft, null, ScaleBasis.Normal, false), "", Alignment.Center, "DockingButtonLeft", null);
			guibutton.OnClicked = new GUIButton.OnClickedHandler(this.NudgeButtonClicked);
			guibutton.UserData = -Vector2.UnitX;
			GUIButton guibutton2 = new GUIButton(new RectTransform(verticalButtonSize, this.dockingContainer.RectTransform, Anchor.CenterRight, null, ScaleBasis.Normal, false), "", Alignment.Center, "DockingButtonRight", null);
			guibutton2.OnClicked = new GUIButton.OnClickedHandler(this.NudgeButtonClicked);
			guibutton2.UserData = Vector2.UnitX;
			GUIButton guibutton3 = new GUIButton(new RectTransform(horizontalButtonSize, this.dockingContainer.RectTransform, Anchor.TopCenter, null, ScaleBasis.Normal, false), "", Alignment.Center, "DockingButtonUp", null);
			guibutton3.OnClicked = new GUIButton.OnClickedHandler(this.NudgeButtonClicked);
			guibutton3.UserData = Vector2.UnitY;
			GUIButton guibutton4 = new GUIButton(new RectTransform(horizontalButtonSize, this.dockingContainer.RectTransform, Anchor.BottomCenter, null, ScaleBasis.Normal, false), "", Alignment.Center, "DockingButtonDown", null);
			guibutton4.OnClicked = new GUIButton.OnClickedHandler(this.NudgeButtonClicked);
			guibutton4.UserData = -Vector2.UnitY;
			this.steerArea = new GUICustomComponent(new RectTransform(Sonar.GUISizeCalculation, base.GuiFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Smallest), delegate(SpriteBatch spriteBatch, GUICustomComponent guiCustomComponent)
			{
				this.DrawHUD(spriteBatch, guiCustomComponent.Rect);
			}, null);
			this.steerRadius = (float)(this.steerArea.Rect.Width / 2);
			this.iceSpireWarningText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 0.25f), this.steerArea.RectTransform, Anchor.Center, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal), TextManager.Get("NavTerminalIceSpireWarning"), new Color?(GUIStyle.Red), GUIStyle.SubHeadingFont, Alignment.Center, true, "", new Color?(Color.Black * 0.8f))
			{
				Visible = false
			};
			this.pressureWarningText = new GUITextBlock(new RectTransform(new Vector2(0.5f, 0.25f), this.steerArea.RectTransform, Anchor.Center, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal), TextManager.Get("SteeringDepthWarning"), new Color?(GUIStyle.Red), GUIStyle.SubHeadingFont, Alignment.Center, false, "", new Color?(Color.Black * 0.8f))
			{
				Visible = false
			};
			RectTransform rectT5 = new RectTransform(new Vector2(0.5f, 0.1f), this.steerArea.RectTransform, Anchor.BottomCenter, new Pivot?(Pivot.TopCenter), null, null, ScaleBasis.Normal);
			RichString text3 = "";
			font = GUIStyle.Font;
			this.tipContainer = new GUITextBlock(rectT5, text3, null, font, Alignment.Center, true, "GUIToolTip", null)
			{
				AutoScaleHorizontal = true
			};
			this.noPowerTip = TextManager.Get("SteeringNoPowerTip");
			this.autoPilotMaintainPosTip = TextManager.Get("SteeringAutoPilotMaintainPosTip");
			string tag = "SteeringAutoPilotLocationTip";
			string varName = "[locationname]";
			GameSession gameSession4 = GameMain.GameSession;
			this.autoPilotLevelStartTip = TextManager.GetWithVariable(tag, varName, (((gameSession4 != null) ? gameSession4.StartLocation : null) == null) ? "Start" : GameMain.GameSession.StartLocation.DisplayName, FormatCapitals.No);
			string tag2 = "SteeringAutoPilotLocationTip";
			string varName2 = "[locationname]";
			GameSession gameSession5 = GameMain.GameSession;
			this.autoPilotLevelEndTip = TextManager.GetWithVariable(tag2, varName2, (((gameSession5 != null) ? gameSession5.EndLocation : null) == null) ? "End" : GameMain.GameSession.EndLocation.DisplayName, FormatCapitals.No);
		}

		// Token: 0x06005D9F RID: 23967 RVA: 0x0030BC96 File Offset: 0x00309E96
		protected override void OnResolutionChanged()
		{
			this.UpdateGUIElements();
		}

		// Token: 0x06005DA0 RID: 23968 RVA: 0x0030BC9E File Offset: 0x00309E9E
		public void AttachToSonarHUD(GUICustomComponent sonarView)
		{
			this.steerArea.Visible = false;
			sonarView.OnDraw = (Action<SpriteBatch, GUICustomComponent>)Delegate.Combine(sonarView.OnDraw, new Action<SpriteBatch, GUICustomComponent>(delegate(SpriteBatch spriteBatch, GUICustomComponent guiCustomComponent)
			{
				this.DrawHUD(spriteBatch, guiCustomComponent.Rect);
				this.steerArea.DrawChildren(spriteBatch, true);
			}));
		}

		// Token: 0x06005DA1 RID: 23969 RVA: 0x0030BCD0 File Offset: 0x00309ED0
		private static Vector2 MapSquareToCircle(Vector2 steeringVector)
		{
			float xSqr = steeringVector.X * steeringVector.X;
			float ySqr = steeringVector.Y * steeringVector.Y;
			float length = MathF.Sqrt(ySqr + xSqr);
			if (MathUtils.NearlyEqual(length, 0f, 0.0001f))
			{
				return Vector2.Zero;
			}
			float x = steeringVector.X * MathF.Sqrt(xSqr + ySqr - xSqr * ySqr) / length;
			float y = steeringVector.Y * MathF.Sqrt(xSqr + ySqr - xSqr * ySqr) / length;
			return new Vector2(x, y);
		}

		// Token: 0x06005DA2 RID: 23970 RVA: 0x0030BD50 File Offset: 0x00309F50
		public void DrawHUD(SpriteBatch spriteBatch, Rectangle rect)
		{
			int width = rect.Width;
			int height = rect.Height;
			int x = rect.X;
			int y = rect.Y;
			if (!this.HasPower)
			{
				return;
			}
			Rectangle velRect = new Rectangle(x + 20, y + 20, width - 40, height - 40);
			Vector2 steeringOrigin = this.steerArea.Rect.Center.ToVector2();
			if (!this.AutoPilot)
			{
				Vector2 steeringInputPos = Steering.MapSquareToCircle(this.steeringInput / 100f) * 100f;
				steeringInputPos.Y = -steeringInputPos.Y;
				steeringInputPos += steeringOrigin;
				if (this.steeringIndicator != null)
				{
					Vector2 dir = steeringInputPos - steeringOrigin;
					float angle = (float)Math.Atan2((double)dir.Y, (double)dir.X);
					this.steeringIndicator.Draw(spriteBatch, steeringOrigin, Color.White, this.steeringIndicator.Origin, angle, new Vector2(dir.Length() / this.steeringIndicator.size.X, 1f), SpriteEffects.None, null);
				}
				else
				{
					GUI.DrawLine(spriteBatch, steeringOrigin, steeringInputPos, Color.LightGray, 0f, 1f);
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)steeringInputPos.X - 5, (int)steeringInputPos.Y - 5, 10, 10), Color.White, false, 0f, 1f);
				}
				if (velRect.Contains(PlayerInput.MousePosition))
				{
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)steeringInputPos.X - 4, (int)steeringInputPos.Y - 4, 8, 8), GUIStyle.Red, false, 0f, 2f);
				}
			}
			else if (this.posToMaintain != null && !this.LevelStartSelected && !this.LevelEndSelected)
			{
				Sonar sonar = this.item.GetComponent<Sonar>();
				if (sonar != null && this.controlledSub != null)
				{
					Vector2 displayPosToMaintain = (this.posToMaintain.Value - this.controlledSub.WorldPosition) * sonar.DisplayScale;
					displayPosToMaintain.Y = -displayPosToMaintain.Y;
					displayPosToMaintain = displayPosToMaintain.ClampLength((float)(velRect.Width / 2));
					displayPosToMaintain = this.steerArea.Rect.Center.ToVector2() + displayPosToMaintain;
					Color crosshairColor = GUIStyle.Orange * (0.5f + ((float)Math.Sin(Timing.TotalTime * 5.0) + 1f) / 4f);
					if (this.maintainPosIndicator != null)
					{
						this.maintainPosIndicator.Draw(spriteBatch, displayPosToMaintain, crosshairColor, 0f, 0.5f * sonar.Zoom, SpriteEffects.None, null);
					}
					else
					{
						float crossHairSize = 8f;
						GUI.DrawLine(spriteBatch, displayPosToMaintain + Vector2.UnitY * crossHairSize, displayPosToMaintain - Vector2.UnitY * crossHairSize, crosshairColor, 0f, 3f);
						GUI.DrawLine(spriteBatch, displayPosToMaintain + Vector2.UnitX * crossHairSize, displayPosToMaintain - Vector2.UnitX * crossHairSize, crosshairColor, 0f, 3f);
					}
					if (this.maintainPosOriginIndicator != null)
					{
						this.maintainPosOriginIndicator.Draw(spriteBatch, steeringOrigin, GUIStyle.Orange, 0f, 0.5f * sonar.Zoom, SpriteEffects.None, null);
					}
					else
					{
						GUI.DrawRectangle(spriteBatch, new Rectangle((int)steeringOrigin.X - 5, (int)steeringOrigin.Y - 5, 10, 10), GUIStyle.Orange, false, 0f, 1f);
					}
				}
			}
			Vector2 steeringPos = Steering.MapSquareToCircle(this.targetVelocity / 100f) * 90f;
			steeringPos.Y = -steeringPos.Y;
			steeringPos += steeringOrigin;
			if (this.steeringIndicator != null)
			{
				Vector2 dir2 = steeringPos - steeringOrigin;
				float angle2 = (float)Math.Atan2((double)dir2.Y, (double)dir2.X);
				this.steeringIndicator.Draw(spriteBatch, steeringOrigin, Color.Gray, this.steeringIndicator.Origin, angle2, new Vector2(dir2.Length() / this.steeringIndicator.size.X, 0.7f), SpriteEffects.None, null);
				return;
			}
			GUI.DrawLine(spriteBatch, steeringOrigin, steeringPos, Color.CadetBlue, 0f, 2f);
		}

		// Token: 0x06005DA3 RID: 23971 RVA: 0x0030C1EC File Offset: 0x0030A3EC
		public void DebugDrawHUD(SpriteBatch spriteBatch, Vector2 transducerCenter, float displayScale, float displayRadius, Vector2 center)
		{
			if (this.SteeringPath == null)
			{
				return;
			}
			Vector2 prevPos = Vector2.Zero;
			foreach (WayPoint wp in this.SteeringPath.Nodes)
			{
				Vector2 pos = (wp.Position - transducerCenter) * displayScale;
				if (pos.Length() <= displayRadius)
				{
					pos.Y = -pos.Y;
					pos += center;
					GUI.DrawRectangle(spriteBatch, new Rectangle((int)pos.X - 1, (int)pos.Y - 3, 6, 6), (this.SteeringPath.CurrentNode == wp) ? Color.LightGreen : GUIStyle.Green, false, 0f, 1f);
					if (prevPos != Vector2.Zero)
					{
						GUI.DrawLine(spriteBatch, pos, prevPos, GUIStyle.Green, 0f, 1f);
					}
					prevPos = pos;
				}
			}
			foreach (Steering.ObstacleDebugInfo obstacle in this.debugDrawObstacles)
			{
				Vector2 pos2 = (obstacle.Point1 - transducerCenter) * displayScale;
				pos2.Y = -pos2.Y;
				pos2 += center;
				Vector2 pos3 = (obstacle.Point2 - transducerCenter) * displayScale;
				pos3.Y = -pos3.Y;
				pos3 += center;
				GUI.DrawLine(spriteBatch, pos2, pos3, GUIStyle.Red * 0.6f, 0f, 3f);
				if (obstacle.Intersection != null)
				{
					Vector2 intersectionPos = (obstacle.Intersection.Value - transducerCenter) * displayScale;
					intersectionPos.Y = -intersectionPos.Y;
					intersectionPos += center;
					GUI.DrawRectangle(spriteBatch, intersectionPos - Vector2.One * 2f, Vector2.One * 4f, GUIStyle.Red, false, 0f, 1f);
				}
				Vector2 obstacleCenter = (pos2 + pos3) / 2f;
				Vector2 vector = obstacle.AvoidStrength;
				if (vector.LengthSquared() > 0.01f)
				{
					GUI.DrawLine(spriteBatch, obstacleCenter, obstacleCenter + new Vector2(obstacle.AvoidStrength.X, -obstacle.AvoidStrength.Y) * 100f, Color.Lerp(GUIStyle.Green, GUIStyle.Orange, obstacle.Dot), 0f, 2f);
				}
			}
		}

		// Token: 0x06005DA4 RID: 23972 RVA: 0x0030C4EC File Offset: 0x0030A6EC
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (this.swapDestinationOrder == null)
			{
				this.swapDestinationOrder = new bool?(this.item.Submarine != null && this.item.Submarine.FlippedX);
				if (this.swapDestinationOrder.Value)
				{
					this.levelStartTickBox.RectTransform.SetAsLastChild();
				}
			}
			if (this.steerArea.Rect.Contains(PlayerInput.MousePosition) && !PlayerInput.KeyDown(InputType.Deselect) && !PlayerInput.KeyHit(InputType.Deselect))
			{
				Character.DisableControls = true;
			}
			if (this.DisableControls)
			{
				this.dockingModeEnabled = false;
			}
			this.dockingContainer.Visible = this.DockingModeEnabled;
			this.statusContainer.Visible = !this.DockingModeEnabled;
			if (!this.DockingModeEnabled)
			{
				GUIMessageBox guimessageBox = this.enterOutpostPrompt;
				if (guimessageBox != null)
				{
					guimessageBox.Close();
				}
			}
			if (this.DockingModeEnabled && this.ActiveDockingSource != null)
			{
				if (Math.Abs(this.ActiveDockingSource.Item.WorldPosition.X - this.DockingTarget.Item.WorldPosition.X) < this.ActiveDockingSource.DistanceTolerance.X && Math.Abs(this.ActiveDockingSource.Item.WorldPosition.Y - this.DockingTarget.Item.WorldPosition.Y) < this.ActiveDockingSource.DistanceTolerance.Y)
				{
					this.dockingButton.Text = this.dockText;
					if (this.dockingButton.FlashTimer <= 0f)
					{
						this.dockingButton.Flash(new Color?(GUIStyle.Blue), 0.5f, false, true, null);
						this.dockingButton.Pulsate(Vector2.One, Vector2.One * 1.2f, this.dockingButton.FlashTimer);
					}
				}
				else
				{
					GUIMessageBox guimessageBox2 = this.enterOutpostPrompt;
					if (guimessageBox2 != null)
					{
						guimessageBox2.Close();
					}
				}
			}
			else if (this.connectedPorts.Any((DockingPort d) => d.Docked))
			{
				this.dockingButton.Text = this.undockText;
				this.dockingContainer.Visible = true;
				this.statusContainer.Visible = false;
				if (this.dockingButton.FlashTimer <= 0f)
				{
					this.dockingButton.Flash(new Color?(GUIStyle.Orange), 1.5f, false, true, null);
					this.dockingButton.Pulsate(Vector2.One, Vector2.One * 1.2f, this.dockingButton.FlashTimer);
				}
			}
			else
			{
				this.dockingButton.Text = this.dockText;
			}
			if (!this.HasPower)
			{
				this.tipContainer.Visible = true;
				this.tipContainer.Text = this.noPowerTip;
				return;
			}
			this.tipContainer.Visible = this.AutoPilot;
			if (this.AutoPilot)
			{
				if (this.maintainPos)
				{
					this.tipContainer.Text = this.autoPilotMaintainPosTip;
				}
				else if (this.LevelStartSelected)
				{
					this.tipContainer.Text = this.autoPilotLevelStartTip;
				}
				else if (this.LevelEndSelected)
				{
					this.tipContainer.Text = this.autoPilotLevelEndTip;
				}
				if (this.DockingModeEnabled && this.DockingTarget != null)
				{
					this.posToMaintain += ConvertUnits.ToDisplayUnits(this.DockingTarget.Item.Submarine.Velocity) * deltaTime;
				}
			}
			this.pressureWarningText.Visible = (this.item.Submarine != null && Timing.TotalTime % 1.0 < 0.800000011920929);
			if (Level.Loaded != null && this.pressureWarningText.Visible && this.item.Submarine.RealWorldDepth > Level.Loaded.RealWorldCrushDepth - 500f && this.item.Submarine.RealWorldDepth > this.item.Submarine.RealWorldCrushDepth - 500f)
			{
				this.pressureWarningText.Visible = true;
				this.pressureWarningText.Text = (this.item.Submarine.AtDamageDepth ? TextManager.Get("SteeringDepthWarning") : TextManager.GetWithVariable("SteeringDepthWarningLow", "[crushdepth]", ((int)this.item.Submarine.RealWorldCrushDepth).ToString(), FormatCapitals.No));
			}
			else
			{
				this.pressureWarningText.Visible = false;
			}
			this.iceSpireWarningText.Visible = (this.item.Submarine != null && !this.pressureWarningText.Visible && this.showIceSpireWarning && Timing.TotalTime % 1.0 < 0.800000011920929);
			if (!this.disableControls && Vector2.DistanceSquared(PlayerInput.MousePosition, this.steerArea.Rect.Center.ToVector2()) < this.steerRadius * this.steerRadius && PlayerInput.PrimaryMouseButtonHeld() && !CrewManager.IsCommandInterfaceOpen && !GameSession.IsTabMenuOpen)
			{
				GameSession gameSession = GameMain.GameSession;
				bool? flag;
				if (gameSession == null)
				{
					flag = null;
				}
				else
				{
					CampaignMode campaign = gameSession.Campaign;
					flag = ((campaign != null) ? new bool?(!campaign.ShowCampaignUI) : null);
				}
				bool? flag2 = flag;
				if (flag2.GetValueOrDefault(true))
				{
					if (!GUIMessageBox.MessageBoxes.Any(delegate(GUIComponent msgBox)
					{
						GUIMessageBox guimessageBox3 = msgBox as GUIMessageBox;
						return guimessageBox3 != null && guimessageBox3.MessageBoxType == GUIMessageBox.Type.Default;
					}))
					{
						Vector2 inputPos = PlayerInput.MousePosition - this.steerArea.Rect.Center.ToVector2();
						inputPos.Y = -inputPos.Y;
						if (this.AutoPilot && !this.LevelStartSelected && !this.LevelEndSelected)
						{
							this.posToMaintain = new Vector2?((this.controlledSub != null) ? (this.controlledSub.WorldPosition + inputPos / this.sonar.DisplayRadius * this.sonar.Range / this.sonar.Zoom) : ((this.item.Submarine == null) ? this.item.WorldPosition : this.item.Submarine.WorldPosition));
						}
						else
						{
							this.SteeringInput = inputPos;
						}
						this.unsentChanges = true;
						this.user = Character.Controlled;
					}
				}
			}
			if (!this.AutoPilot && Character.DisableControls && GUI.KeyboardDispatcher.Subscriber == null)
			{
				this.steeringAdjustSpeed = ((character == null) ? 0.2f : MathHelper.Lerp(0.2f, 1f, character.GetSkillLevel(Tags.HelmSkill) / 100f));
				Vector2 input = Vector2.Zero;
				if (PlayerInput.KeyDown(InputType.Left))
				{
					input -= Vector2.UnitX;
				}
				if (PlayerInput.KeyDown(InputType.Right))
				{
					input += Vector2.UnitX;
				}
				if (PlayerInput.KeyDown(InputType.Up))
				{
					input += Vector2.UnitY;
				}
				if (PlayerInput.KeyDown(InputType.Down))
				{
					input -= Vector2.UnitY;
				}
				if (PlayerInput.KeyDown(InputType.Run))
				{
					this.SteeringInput += input * deltaTime * 200f;
					this.inputCumulation = 0f;
					this.keyboardInput = Vector2.Zero;
					this.unsentChanges = true;
				}
				else
				{
					float step = deltaTime * 5f;
					if (input.Length() > 0f)
					{
						this.inputCumulation += step;
					}
					else
					{
						this.inputCumulation -= step;
					}
					float maxCumulation = 1f;
					this.inputCumulation = MathHelper.Clamp(this.inputCumulation, 0f, maxCumulation);
					float length = MathHelper.Lerp(0f, 0.2f, MathUtils.InverseLerp(0f, maxCumulation, this.inputCumulation));
					Vector2 normalizedInput = Vector2.Normalize(input);
					if (MathUtils.IsValid(normalizedInput))
					{
						this.keyboardInput += normalizedInput * length;
					}
					if (this.keyboardInput.LengthSquared() > 0.01f)
					{
						this.SteeringInput += this.keyboardInput;
						this.unsentChanges = true;
						this.user = Character.Controlled;
						this.keyboardInput *= MathHelper.Clamp(1f - step, 0f, 1f);
					}
				}
			}
			else
			{
				this.inputCumulation = 0f;
				this.keyboardInput = Vector2.Zero;
			}
			if (!this.UseAutoDocking || this.DisableControls)
			{
				return;
			}
			if (this.checkConnectedPortsTimer <= 0f)
			{
				List<Connection> connections = this.item.Connections;
				Connection connection;
				if (connections == null)
				{
					connection = null;
				}
				else
				{
					connection = connections.FirstOrDefault((Connection c) => c.Name == "toggle_docking");
				}
				Connection dockingConnection = connection;
				if (dockingConnection != null)
				{
					this.connectedPorts = this.item.GetConnectedComponentsRecursive<DockingPort>(dockingConnection, true, false);
				}
				this.checkConnectedPortsTimer = 1f;
			}
			else
			{
				this.checkConnectedPortsTimer -= deltaTime;
			}
			this.DockingModeEnabled = false;
			if (this.connectedPorts.None(null))
			{
				return;
			}
			float closestDist = this.DockingAssistThreshold * this.DockingAssistThreshold;
			foreach (DockingPort sourcePort in this.connectedPorts)
			{
				if (!sourcePort.Docked && sourcePort.Item.Submarine != null && sourcePort.Item.Submarine == this.controlledSub)
				{
					int sourceDir = sourcePort.GetDir(null);
					foreach (DockingPort targetPort in DockingPort.List)
					{
						if (!targetPort.Docked && targetPort.Item.Submarine != null && targetPort.Item.Submarine != this.controlledSub && targetPort.IsHorizontal == sourcePort.IsHorizontal)
						{
							IEnumerable<Submarine> dockedTo = targetPort.Item.Submarine.DockedTo;
							if ((dockedTo == null || !dockedTo.Contains(sourcePort.Item.Submarine)) && !targetPort.Item.Submarine.IsAboveLevel && sourceDir != targetPort.GetDir(null))
							{
								float dist = Vector2.DistanceSquared(sourcePort.Item.WorldPosition, targetPort.Item.WorldPosition);
								if (dist < closestDist)
								{
									closestDist = dist;
									this.DockingModeEnabled = true;
									this.ActiveDockingSource = sourcePort;
									this.DockingTarget = targetPort;
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06005DA5 RID: 23973 RVA: 0x0030D080 File Offset: 0x0030B280
		private void TrySetTickBoxSelected(GUITickBox tickBox, ref bool backingValue, bool newValue)
		{
			if (tickBox == null)
			{
				backingValue = newValue;
				return;
			}
			tickBox.Selected = newValue;
		}

		// Token: 0x06005DA6 RID: 23974 RVA: 0x0030D090 File Offset: 0x0030B290
		private bool NudgeButtonClicked(GUIButton btn, object userdata)
		{
			if (!this.MaintainPos || !this.AutoPilot)
			{
				this.AutoPilot = true;
				this.posToMaintain = new Vector2?(this.item.Submarine.WorldPosition);
			}
			this.MaintainPos = true;
			if (userdata is Vector2)
			{
				Vector2 nudgeAmount = (Vector2)userdata;
				Sonar sonar = this.item.GetComponent<Sonar>();
				if (sonar != null)
				{
					nudgeAmount *= 500f / sonar.Zoom;
				}
				this.PosToMaintain += nudgeAmount;
			}
			this.unsentChanges = true;
			return true;
		}

		// Token: 0x06005DA7 RID: 23975 RVA: 0x0030D148 File Offset: 0x0030B348
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Sprite sprite = this.maintainPosIndicator;
			if (sprite != null)
			{
				sprite.Remove();
			}
			Sprite sprite2 = this.maintainPosOriginIndicator;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			Sprite sprite3 = this.steeringIndicator;
			if (sprite3 != null)
			{
				sprite3.Remove();
			}
			GUIMessageBox guimessageBox = this.enterOutpostPrompt;
			if (guimessageBox != null)
			{
				guimessageBox.Close();
			}
			GUIMessageBox guimessageBox2 = this.exitOutpostPrompt;
			if (guimessageBox2 != null)
			{
				guimessageBox2.Close();
			}
			this.pathFinder = null;
		}

		// Token: 0x06005DA8 RID: 23976 RVA: 0x0030D1B8 File Offset: 0x0030B3B8
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.AutoPilot);
			msg.WriteBoolean(this.dockingNetworkMessagePending);
			this.dockingNetworkMessagePending = false;
			if (!this.AutoPilot)
			{
				msg.WriteSingle(this.steeringInput.X);
				msg.WriteSingle(this.steeringInput.Y);
				return;
			}
			msg.WriteBoolean(this.posToMaintain != null);
			if (this.posToMaintain != null)
			{
				msg.WriteSingle(this.posToMaintain.Value.X);
				msg.WriteSingle(this.posToMaintain.Value.Y);
				return;
			}
			msg.WriteBoolean(this.LevelStartSelected);
		}

		// Token: 0x06005DA9 RID: 23977 RVA: 0x0030D268 File Offset: 0x0030B468
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			int msgStartPos = msg.BitPosition;
			bool autoPilot = msg.ReadBoolean();
			bool dockingButtonClicked = msg.ReadBoolean();
			ushort userID = msg.ReadUInt16();
			Vector2 newSteeringInput = this.steeringInput;
			Vector2 newTargetVelocity = this.targetVelocity;
			float newSteeringAdjustSpeed = this.steeringAdjustSpeed;
			Vector2? newPosToMaintain = null;
			bool headingToStart = false;
			if (dockingButtonClicked)
			{
				this.item.SendSignal(new Signal("1", 0, Entity.FindEntityByID(userID) as Character, null, 0f, 1f), "toggle_docking");
			}
			if (autoPilot)
			{
				if (msg.ReadBoolean())
				{
					newPosToMaintain = new Vector2?(new Vector2(msg.ReadSingle(), msg.ReadSingle()));
				}
				else
				{
					headingToStart = msg.ReadBoolean();
				}
			}
			else
			{
				newSteeringInput = new Vector2(msg.ReadSingle(), msg.ReadSingle());
				newTargetVelocity = new Vector2(msg.ReadSingle(), msg.ReadSingle());
				newSteeringAdjustSpeed = msg.ReadSingle();
			}
			if (this.correctionTimer > 0f)
			{
				int msgLength = msg.BitPosition - msgStartPos;
				msg.BitPosition = msgStartPos;
				base.StartDelayedCorrection(msg.ExtractBits(msgLength), sendingTime, false);
				return;
			}
			this.AutoPilot = autoPilot;
			if (!this.AutoPilot)
			{
				this.SteeringInput = newSteeringInput;
				this.TargetVelocity = newTargetVelocity;
				this.steeringAdjustSpeed = newSteeringAdjustSpeed;
				return;
			}
			this.MaintainPos = (newPosToMaintain != null);
			this.posToMaintain = newPosToMaintain;
			if (this.posToMaintain == null)
			{
				this.LevelStartSelected = headingToStart;
				this.LevelEndSelected = !headingToStart;
				this.UpdatePath();
				return;
			}
			this.LevelStartSelected = false;
			this.LevelEndSelected = false;
		}

		// Token: 0x06005DAA RID: 23978 RVA: 0x0030D3E8 File Offset: 0x0030B5E8
		private void UpdateGUIElements()
		{
			this.steeringModeSwitch.Selected = this.AutoPilot;
			this.autopilotIndicator.Selected = this.AutoPilot;
			this.manualPilotIndicator.Selected = !this.AutoPilot;
			if (this.DisableControls)
			{
				this.steeringModeSwitch.Enabled = false;
				this.maintainPosTickBox.Enabled = false;
				this.levelEndTickBox.Enabled = false;
				this.levelStartTickBox.Enabled = false;
				return;
			}
			this.steeringModeSwitch.Enabled = true;
			this.maintainPosTickBox.Enabled = this.AutoPilot;
			this.levelEndTickBox.Enabled = this.AutoPilot;
			this.levelStartTickBox.Enabled = this.AutoPilot;
		}

		// Token: 0x17001791 RID: 6033
		// (get) Token: 0x06005DAB RID: 23979 RVA: 0x0030D4A3 File Offset: 0x0030B6A3
		public Submarine ControlledSub
		{
			get
			{
				return this.controlledSub;
			}
		}

		// Token: 0x17001792 RID: 6034
		// (get) Token: 0x06005DAC RID: 23980 RVA: 0x0030D4AB File Offset: 0x0030B6AB
		// (set) Token: 0x06005DAD RID: 23981 RVA: 0x0030D4B3 File Offset: 0x0030B6B3
		public Vector2 AITacticalTarget { get; set; }

		// Token: 0x17001793 RID: 6035
		// (get) Token: 0x06005DAE RID: 23982 RVA: 0x0030D4BC File Offset: 0x0030B6BC
		// (set) Token: 0x06005DAF RID: 23983 RVA: 0x0030D4C4 File Offset: 0x0030B6C4
		public float AIRamTimer { get; set; }

		// Token: 0x17001794 RID: 6036
		// (get) Token: 0x06005DB0 RID: 23984 RVA: 0x0030D4CD File Offset: 0x0030B6CD
		// (set) Token: 0x06005DB1 RID: 23985 RVA: 0x0030D4D8 File Offset: 0x0030B6D8
		[Serialize(false, IsPropertySaveable.Yes, "Is autopilot currently on or not?", "", false, AlwaysUseInstanceValues = true)]
		public bool AutoPilot
		{
			get
			{
				return this.autoPilot;
			}
			set
			{
				if (value == this.autoPilot)
				{
					return;
				}
				this.autoPilot = value;
				this.UpdateGUIElements();
				if (this.autoPilot)
				{
					this.MaintainPos = true;
					if (this.posToMaintain == null)
					{
						this.RefreshPosToMaintain();
						return;
					}
				}
				else
				{
					this.PosToMaintain = null;
					this.MaintainPos = false;
					this.LevelEndSelected = false;
					this.LevelStartSelected = false;
				}
			}
		}

		// Token: 0x17001795 RID: 6037
		// (get) Token: 0x06005DB2 RID: 23986 RVA: 0x0030D543 File Offset: 0x0030B743
		// (set) Token: 0x06005DB3 RID: 23987 RVA: 0x0030D54B File Offset: 0x0030B74B
		[Editable(0f, 1f, 4)]
		[Serialize(0.5f, IsPropertySaveable.Yes, "How full the ballast tanks should be when the submarine is not being steered upwards/downwards. Can be used to compensate if the ballast tanks are too large/small relative to the size of the submarine.", "", false)]
		public float NeutralBallastLevel
		{
			get
			{
				return this.neutralBallastLevel;
			}
			set
			{
				this.neutralBallastLevel = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17001796 RID: 6038
		// (get) Token: 0x06005DB4 RID: 23988 RVA: 0x0030D563 File Offset: 0x0030B763
		// (set) Token: 0x06005DB5 RID: 23989 RVA: 0x0030D56B File Offset: 0x0030B76B
		[Serialize(1000f, IsPropertySaveable.Yes, "How close the docking port has to be to another docking port for the docking mode to become active.", "", false)]
		public float DockingAssistThreshold { get; set; }

		// Token: 0x17001797 RID: 6039
		// (get) Token: 0x06005DB6 RID: 23990 RVA: 0x0030D574 File Offset: 0x0030B774
		// (set) Token: 0x06005DB7 RID: 23991 RVA: 0x0030D57C File Offset: 0x0030B77C
		public Vector2 TargetVelocity
		{
			get
			{
				return this.targetVelocity;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					if (!MathUtils.IsValid(this.targetVelocity))
					{
						this.targetVelocity = Vector2.Zero;
					}
					return;
				}
				this.targetVelocity.X = MathHelper.Clamp(value.X, -100f, 100f);
				this.targetVelocity.Y = MathHelper.Clamp(value.Y, -100f, 100f);
			}
		}

		// Token: 0x17001798 RID: 6040
		// (get) Token: 0x06005DB8 RID: 23992 RVA: 0x0030D5EC File Offset: 0x0030B7EC
		public float TargetVelocityLengthSquared
		{
			get
			{
				return this.TargetVelocity.LengthSquared();
			}
		}

		// Token: 0x17001799 RID: 6041
		// (get) Token: 0x06005DB9 RID: 23993 RVA: 0x0030D607 File Offset: 0x0030B807
		// (set) Token: 0x06005DBA RID: 23994 RVA: 0x0030D610 File Offset: 0x0030B810
		public Vector2 SteeringInput
		{
			get
			{
				return this.steeringInput;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.steeringInput.X = MathHelper.Clamp(value.X, -100f, 100f);
				this.steeringInput.Y = MathHelper.Clamp(value.Y, -100f, 100f);
			}
		}

		// Token: 0x1700179A RID: 6042
		// (get) Token: 0x06005DBB RID: 23995 RVA: 0x0030D666 File Offset: 0x0030B866
		public SteeringPath SteeringPath
		{
			get
			{
				return this.steeringPath;
			}
		}

		// Token: 0x1700179B RID: 6043
		// (get) Token: 0x06005DBC RID: 23996 RVA: 0x0030D66E File Offset: 0x0030B86E
		// (set) Token: 0x06005DBD RID: 23997 RVA: 0x0030D676 File Offset: 0x0030B876
		public Vector2? PosToMaintain
		{
			get
			{
				return this.posToMaintain;
			}
			set
			{
				this.posToMaintain = value;
			}
		}

		// Token: 0x1700179C RID: 6044
		// (get) Token: 0x06005DBE RID: 23998 RVA: 0x0030D67F File Offset: 0x0030B87F
		// (set) Token: 0x06005DBF RID: 23999 RVA: 0x0030D691 File Offset: 0x0030B891
		public bool DockingModeEnabled
		{
			get
			{
				return this.UseAutoDocking && this.dockingModeEnabled;
			}
			set
			{
				this.dockingModeEnabled = value;
			}
		}

		// Token: 0x1700179D RID: 6045
		// (get) Token: 0x06005DC0 RID: 24000 RVA: 0x0030D69A File Offset: 0x0030B89A
		// (set) Token: 0x06005DC1 RID: 24001 RVA: 0x0030D6A2 File Offset: 0x0030B8A2
		public bool UseAutoDocking { get; set; } = true;

		// Token: 0x06005DC2 RID: 24002 RVA: 0x0030D6AC File Offset: 0x0030B8AC
		private void FindConnectedDockingPort()
		{
			this.searchedConnectedDockingPort = true;
			foreach (MapEntity linkedTo in this.item.linkedTo)
			{
				Item item = linkedTo as Item;
				if (item != null)
				{
					DockingPort port = item.GetComponent<DockingPort>();
					if (port != null)
					{
						this.DockingSources.Add(port);
					}
				}
			}
			Connection dockingConnection = this.item.Connections.FirstOrDefault((Connection c) => c.Name == "toggle_docking");
			if (dockingConnection != null)
			{
				List<DockingPort> connectedPorts = this.item.GetConnectedComponentsRecursive<DockingPort>(dockingConnection, false, false);
				this.DockingSources.AddRange(from p in connectedPorts
				where p.Item.Submarine != null && !p.Item.Submarine.Info.IsOutpost
				select p);
			}
		}

		// Token: 0x06005DC3 RID: 24003 RVA: 0x0030D79C File Offset: 0x0030B99C
		public Steering(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.InitProjSpecific(element);
		}

		// Token: 0x06005DC4 RID: 24004 RVA: 0x0030D808 File Offset: 0x0030BA08
		private void InitProjSpecific(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "steeringindicator"))
				{
					if (!(a == "maintainposindicator"))
					{
						if (a == "maintainposoriginindicator")
						{
							this.maintainPosOriginIndicator = new Sprite(subElement, "", "", false, 1f);
						}
					}
					else
					{
						this.maintainPosIndicator = new Sprite(subElement, "", "", false, 1f);
					}
				}
				else
				{
					this.steeringIndicator = new Sprite(subElement, "", "", false, 1f);
				}
			}
			this.CreateGUI();
		}

		// Token: 0x06005DC5 RID: 24005 RVA: 0x0030D8EC File Offset: 0x0030BAEC
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.sonar = this.item.GetComponent<Sonar>();
		}

		// Token: 0x06005DC6 RID: 24006 RVA: 0x0030D905 File Offset: 0x0030BB05
		public override bool Select(Character character)
		{
			if (!base.CanBeSelected)
			{
				return false;
			}
			this.user = character;
			return true;
		}

		// Token: 0x06005DC7 RID: 24007 RVA: 0x0030D91C File Offset: 0x0030BB1C
		public void RefreshPosToMaintain()
		{
			this.posToMaintain = new Vector2?((this.controlledSub != null) ? this.controlledSub.WorldPosition : ((this.item.Submarine == null) ? this.item.WorldPosition : this.item.Submarine.WorldPosition));
		}

		// Token: 0x06005DC8 RID: 24008 RVA: 0x0030D973 File Offset: 0x0030BB73
		public override void OnMapLoaded()
		{
			if (this.MaintainPos)
			{
				this.RefreshPosToMaintain();
			}
		}

		// Token: 0x06005DC9 RID: 24009 RVA: 0x0030D984 File Offset: 0x0030BB84
		public override void Update(float deltaTime, Camera cam)
		{
			if (!this.searchedConnectedDockingPort)
			{
				this.FindConnectedDockingPort();
			}
			this.networkUpdateTimer -= deltaTime;
			if (this.unsentChanges && this.networkUpdateTimer <= 0f)
			{
				if (GameMain.Client != null)
				{
					this.item.CreateClientEvent<Steering>(this);
					this.correctionTimer = 1f;
				}
				this.networkUpdateTimer = 0.1f;
				this.unsentChanges = false;
			}
			this.controlledSub = this.item.Submarine;
			Sonar sonar = this.item.GetComponent<Sonar>();
			if (sonar != null && sonar.UseTransducers)
			{
				this.controlledSub = (sonar.ConnectedTransducers.Any<SonarTransducer>() ? sonar.ConnectedTransducers.First<SonarTransducer>().Item.Submarine : null);
			}
			if (!this.HasPower)
			{
				return;
			}
			if (this.user != null && this.user.Removed)
			{
				this.user = null;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			float userSkill = 0f;
			if (this.user != null && this.controlledSub != null && (this.user.SelectedItem == this.item || this.item.linkedTo.Contains(this.user.SelectedItem)))
			{
				userSkill = this.user.GetSkillLevel(Tags.HelmSkill) / 100f;
			}
			if (this.AIRamTimer > 0f && this.controlledSub != null)
			{
				this.AIRamTimer -= deltaTime;
				this.TargetVelocity = this.GetSteeringVelocity(this.AITacticalTarget, 0f);
			}
			else if (this.AutoPilot)
			{
				if (this.lastReceivedSteeringSignalTime < Timing.TotalTime - 1.0)
				{
					this.UpdateAutoPilot(deltaTime);
					float throttle = 1f;
					if (this.controlledSub != null)
					{
						throttle = MathHelper.Clamp(Vector2.Dot(this.controlledSub.Velocity, this.TargetVelocity) / 100f, 0f, 1f);
					}
					float maxSpeed = MathHelper.Lerp(0.5f, 1f, userSkill) * 100f;
					this.TargetVelocity = this.TargetVelocity.ClampLength(MathHelper.Lerp(100f, maxSpeed, throttle));
				}
			}
			else
			{
				this.showIceSpireWarning = false;
				if (this.user != null && this.user.Info != null && this.user.SelectedItem == this.item)
				{
					this.IncreaseSkillLevel(this.user, deltaTime);
				}
				Vector2 velocityDiff = this.steeringInput - this.targetVelocity;
				if (velocityDiff != Vector2.Zero)
				{
					if (this.steeringAdjustSpeed >= 0.99f)
					{
						this.TargetVelocity = this.steeringInput;
					}
					else
					{
						float steeringChange = 1f / (1f - this.steeringAdjustSpeed);
						steeringChange *= steeringChange * 10f;
						this.TargetVelocity += Vector2.Normalize(velocityDiff) * Math.Min(steeringChange * deltaTime, velocityDiff.Length());
					}
				}
			}
			float velX = this.targetVelocity.X;
			if (this.controlledSub != null && this.controlledSub.FlippedX)
			{
				velX *= -1f;
			}
			this.item.SendSignal(new Signal(velX.ToString(CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "velocity_x_out");
			float velY = MathHelper.Lerp((this.neutralBallastLevel * 100f - 50f) * 2f, (float)(-100 * Math.Sign(this.targetVelocity.Y)), Math.Abs(this.targetVelocity.Y) / 100f);
			this.item.SendSignal(new Signal(velY.ToString(CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "velocity_y_out");
			Submarine sub = this.controlledSub;
			if (sub != null)
			{
				this.item.SendSignal(new Signal((ConvertUnits.ToDisplayUnits(sub.Velocity.X * Physics.DisplayToRealWorldRatio) * 3.6f).ToString("0.0000", CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "current_velocity_x");
				this.item.SendSignal(new Signal((ConvertUnits.ToDisplayUnits(sub.Velocity.Y * Physics.DisplayToRealWorldRatio) * -3.6f).ToString("0.0000", CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "current_velocity_y");
				Vector2 pos = new Vector2(sub.WorldPosition.X * Physics.DisplayToRealWorldRatio, sub.RealWorldDepth);
				if (sonar != null && sonar.UseTransducers && sonar.CenterOnTransducers && sonar.ConnectedTransducers.Any<SonarTransducer>())
				{
					pos = Vector2.Zero;
					foreach (SonarTransducer connectedTransducer in sonar.ConnectedTransducers)
					{
						pos += connectedTransducer.Item.WorldPosition;
					}
					pos /= (float)sonar.ConnectedTransducers.Count<SonarTransducer>();
					float x = pos.X * Physics.DisplayToRealWorldRatio;
					Level loaded = Level.Loaded;
					pos = new Vector2(x, (loaded != null) ? loaded.GetRealWorldDepth(pos.Y) : (-pos.Y * Physics.DisplayToRealWorldRatio));
				}
				this.item.SendSignal(new Signal(pos.X.ToString("0.0000", CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "current_position_x");
				this.item.SendSignal(new Signal(pos.Y.ToString("0.0000", CultureInfo.InvariantCulture), 0, this.user, null, 0f, 1f), "current_position_y");
			}
			if (this.navigateTactically && (this.user == null || this.user.SelectedItem != this.item))
			{
				this.navigateTactically = false;
				this.AIRamTimer = 0f;
				this.SetMaintainPosition();
			}
		}

		// Token: 0x06005DCA RID: 24010 RVA: 0x0030DFCC File Offset: 0x0030C1CC
		private void IncreaseSkillLevel(Character user, float deltaTime)
		{
			if (this.controlledSub == null)
			{
				return;
			}
			if (this.controlledSub.Velocity.LengthSquared() < 0.01f)
			{
				return;
			}
			if (((user != null) ? user.Info : null) == null)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			if (((gameSession != null) ? gameSession.Campaign : null) != null)
			{
				if (this.controlledSub.DockedTo.Any((Submarine d) => d.PhysicsBody.BodyType == BodyType.Static))
				{
					return;
				}
			}
			float speedMultiplier = MathHelper.Clamp(this.TargetVelocity.Length() / 100f, 0f, 1f);
			user.Info.ApplySkillGain(Tags.HelmSkill, SkillSettings.Current.SkillIncreasePerSecondWhenSteering * speedMultiplier * deltaTime, false, 2f, false);
		}

		// Token: 0x06005DCB RID: 24011 RVA: 0x0030E09C File Offset: 0x0030C29C
		private void UpdateAutoPilot(float deltaTime)
		{
			if (this.controlledSub == null)
			{
				return;
			}
			if (this.posToMaintain != null)
			{
				Vector2 steeringVel = this.GetSteeringVelocity(this.posToMaintain.Value, 10f);
				this.TargetVelocity = Vector2.Lerp(this.TargetVelocity, steeringVel, 0.1f);
				this.showIceSpireWarning = false;
				return;
			}
			this.autopilotRayCastTimer -= deltaTime;
			this.autopilotRecalculatePathTimer -= deltaTime;
			if (this.autopilotRecalculatePathTimer <= 0f)
			{
				this.UpdatePath();
				this.autopilotRecalculatePathTimer = 5f;
			}
			if (this.steeringPath == null)
			{
				this.showIceSpireWarning = false;
				return;
			}
			this.steeringPath.CheckProgress(ConvertUnits.ToSimUnits(this.controlledSub.WorldPosition), 10f);
			this.connectedSubUpdateTimer -= deltaTime;
			if (this.connectedSubUpdateTimer <= 0f)
			{
				this.connectedSubs.Clear();
				this.connectedSubs.AddRange(this.controlledSub.GetConnectedSubs());
				this.connectedSubUpdateTimer = 1f;
			}
			if (this.autopilotRayCastTimer <= 0f && this.steeringPath.NextNode != null)
			{
				Vector2 diff = ConvertUnits.ToSimUnits(this.steeringPath.NextNode.Position - this.controlledSub.WorldPosition);
				float lengthSqr = diff.LengthSquared();
				if (lengthSqr > 0.001f && lengthSqr < 900f)
				{
					diff = Vector2.Normalize(diff);
					bool nextVisible = true;
					for (int x = -1; x < 2; x += 2)
					{
						for (int y = -1; y < 2; y += 2)
						{
							Vector2 cornerPos = new Vector2((float)(this.controlledSub.Borders.Width * x), (float)(this.controlledSub.Borders.Height * y)) / 2f;
							cornerPos = ConvertUnits.ToSimUnits(cornerPos * 1.1f + this.controlledSub.WorldPosition);
							float dist = Vector2.Distance(cornerPos, this.steeringPath.NextNode.SimPosition);
							if (Submarine.PickBody(cornerPos, cornerPos + diff * dist, null, new Category?(Category.Cat8), true, null, false) != null)
							{
								nextVisible = false;
								x = 2;
								y = 2;
							}
						}
					}
					if (nextVisible)
					{
						this.steeringPath.SkipToNextNode();
					}
				}
				this.autopilotRayCastTimer = 0.5f;
			}
			Vector2 newVelocity = Vector2.Zero;
			if (this.steeringPath.CurrentNode != null)
			{
				newVelocity = this.GetSteeringVelocity(this.steeringPath.CurrentNode.WorldPosition, 2f);
			}
			Vector2 avoidDist = new Vector2(Math.Max(1000f * Math.Abs(this.controlledSub.Velocity.X), (float)this.controlledSub.Borders.Width * 0.75f), Math.Max(1000f * Math.Abs(this.controlledSub.Velocity.Y), (float)this.controlledSub.Borders.Height * 0.75f));
			float avoidRadius = avoidDist.Length();
			float damagingWallAvoidRadius = MathHelper.Clamp(avoidRadius * 1.5f, 5000f, 10000f);
			Vector2 newAvoidStrength = Vector2.Zero;
			this.debugDrawObstacles.Clear();
			this.showIceSpireWarning = false;
			List<VoronoiCell> closeCells = Level.Loaded.GetCells(this.controlledSub.WorldPosition, 4);
			foreach (VoronoiCell cell in closeCells)
			{
				if (!cell.DoesDamage)
				{
					Body body = cell.Body;
					if (body == null || body.BodyType != BodyType.Dynamic)
					{
						goto IL_4F5;
					}
				}
				using (List<GraphEdge>.Enumerator enumerator2 = cell.Edges.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						GraphEdge edge = enumerator2.Current;
						Vector2 closestPoint = MathUtils.GetClosestPointOnLineSegment(edge.Point1 + cell.Translation, edge.Point2 + cell.Translation, this.controlledSub.WorldPosition);
						Vector2 diff2 = closestPoint - this.controlledSub.WorldPosition;
						float dist2 = diff2.Length() - (float)(Math.Max(this.controlledSub.Borders.Width, this.controlledSub.Borders.Height) / 2);
						if (dist2 <= damagingWallAvoidRadius)
						{
							Vector2 normalizedDiff = Vector2.Normalize(diff2);
							float dot = Vector2.Dot(normalizedDiff, this.controlledSub.Velocity);
							float avoidStrength = MathHelper.Clamp(MathHelper.Lerp(1f, 0f, dist2 / damagingWallAvoidRadius - dot), 0f, 1f);
							Vector2 avoid = -normalizedDiff * avoidStrength;
							newAvoidStrength += avoid;
							this.debugDrawObstacles.Add(new Steering.ObstacleDebugInfo(edge, new Vector2?(edge.Center), 1f, avoid, cell.Translation));
							if (dot > 0f && cell.DoesDamage)
							{
								this.showIceSpireWarning = true;
							}
						}
					}
					continue;
				}
				IL_4F5:
				foreach (GraphEdge edge2 in cell.Edges)
				{
					Vector2 intersection;
					if (MathUtils.GetLineSegmentIntersection(edge2.Point1 + cell.Translation, edge2.Point2 + cell.Translation, this.controlledSub.WorldPosition, cell.Center, out intersection))
					{
						Vector2 diff3 = this.controlledSub.WorldPosition - intersection;
						if (Math.Abs(diff3.X) > avoidDist.X && Math.Abs(diff3.Y) > avoidDist.Y)
						{
							this.debugDrawObstacles.Add(new Steering.ObstacleDebugInfo(edge2, new Vector2?(intersection), 0f, Vector2.Zero, Vector2.Zero));
						}
						else
						{
							if (diff3.LengthSquared() < 1f)
							{
								diff3 = Vector2.UnitY;
							}
							Vector2 normalizedDiff2 = Vector2.Normalize(diff3);
							float dot2 = (this.controlledSub.Velocity == Vector2.Zero) ? 0f : Vector2.Dot(this.controlledSub.Velocity, -normalizedDiff2);
							if ((double)dot2 < 1.0)
							{
								this.debugDrawObstacles.Add(new Steering.ObstacleDebugInfo(edge2, new Vector2?(intersection), dot2, Vector2.Zero, cell.Translation));
							}
							else
							{
								Vector2 change = normalizedDiff2 * Math.Max(avoidRadius - diff3.Length(), 0f) / avoidRadius;
								if (change.LengthSquared() >= 0.001f)
								{
									newAvoidStrength += change * (dot2 - 1f);
									this.debugDrawObstacles.Add(new Steering.ObstacleDebugInfo(edge2, new Vector2?(intersection), dot2 - 1f, change * (dot2 - 1f), cell.Translation));
								}
							}
						}
					}
				}
			}
			this.avoidStrength = Vector2.Lerp(this.avoidStrength, newAvoidStrength, deltaTime * 10f);
			this.TargetVelocity = Vector2.Lerp(this.TargetVelocity, newVelocity + this.avoidStrength * 100f, 0.1f);
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub != this.controlledSub && !this.connectedSubs.Contains(sub))
				{
					Vector2 minDist = (this.controlledSub.Borders.Size + sub.Borders.Size).ToVector2() / 2f;
					Vector2 diff4 = this.controlledSub.WorldPosition - sub.WorldPosition;
					float xDist = Math.Abs(diff4.X);
					float yDist = Math.Abs(diff4.Y);
					Vector2 maxAvoidDistance = minDist * 2f;
					if (xDist <= maxAvoidDistance.X && yDist <= maxAvoidDistance.Y)
					{
						float dot3 = (this.controlledSub.Velocity == Vector2.Zero) ? 0f : Vector2.Dot(Vector2.Normalize(this.controlledSub.Velocity), -diff4);
						if (dot3 >= 0f)
						{
							float distanceFactor = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(maxAvoidDistance.X + maxAvoidDistance.Y, minDist.X + minDist.Y, xDist + yDist));
							float velocityFactor = MathHelper.Lerp(0f, 1f, MathUtils.InverseLerp(0f, 3f, this.controlledSub.Velocity.Length()));
							this.TargetVelocity += 100f * Vector2.Normalize(diff4) * distanceFactor * velocityFactor;
						}
					}
				}
			}
			float velMagnitude = this.TargetVelocity.Length();
			if (velMagnitude > 100f)
			{
				this.TargetVelocity *= 100f / velMagnitude;
			}
			HintManager.OnAutoPilotPathUpdated(this);
		}

		// Token: 0x06005DCC RID: 24012 RVA: 0x0030EA64 File Offset: 0x0030CC64
		private float? GetNodePenalty(PathNode node, PathNode nextNode)
		{
			WayPoint waypoint = node.Waypoint;
			if (((waypoint != null) ? waypoint.Tunnel : null) == null || this.controlledSub == null || node.Waypoint.Tunnel.Type == Level.TunnelType.MainPath)
			{
				return new float?(0f);
			}
			if (node.Waypoint.Tunnel.Type == Level.TunnelType.MainPath)
			{
				WayPoint waypoint2 = nextNode.Waypoint;
				bool flag;
				if (waypoint2 == null)
				{
					flag = true;
				}
				else
				{
					Level.Tunnel tunnel = waypoint2.Tunnel;
					Level.TunnelType? tunnelType = (tunnel != null) ? new Level.TunnelType?(tunnel.Type) : null;
					Level.TunnelType tunnelType2 = Level.TunnelType.MainPath;
					flag = !(tunnelType.GetValueOrDefault() == tunnelType2 & tunnelType != null);
				}
				if (flag)
				{
					return null;
				}
			}
			return new float?(1000f);
		}

		// Token: 0x06005DCD RID: 24013 RVA: 0x0030EB18 File Offset: 0x0030CD18
		private void UpdatePath()
		{
			if (Level.Loaded == null)
			{
				return;
			}
			if (this.pathFinder == null)
			{
				this.pathFinder = new PathFinder(WayPoint.WayPointList, false)
				{
					GetNodePenalty = new PathFinder.GetNodePenaltyHandler(this.GetNodePenalty)
				};
			}
			Vector2 target;
			if (this.navigateTactically)
			{
				target = ConvertUnits.ToSimUnits(this.AITacticalTarget);
			}
			else if (this.LevelEndSelected)
			{
				target = ConvertUnits.ToSimUnits(Level.Loaded.EndExitPosition);
			}
			else
			{
				target = ConvertUnits.ToSimUnits(Level.Loaded.StartExitPosition);
			}
			PathFinder pathFinder = this.pathFinder;
			Vector2 start = ConvertUnits.ToSimUnits((this.controlledSub == null) ? this.item.WorldPosition : this.controlledSub.WorldPosition);
			Vector2 end = target;
			Submarine hostSub = null;
			string str = "(Autopilot, target: ";
			Vector2 vector = target;
			this.steeringPath = pathFinder.FindPath(start, end, hostSub, str + vector.ToString() + ")", 0f, null, null, null, true, 0f);
		}

		// Token: 0x06005DCE RID: 24014 RVA: 0x0030EBFC File Offset: 0x0030CDFC
		public void SetDestinationLevelStart()
		{
			this.AutoPilot = true;
			this.MaintainPos = false;
			this.posToMaintain = null;
			this.LevelEndSelected = false;
			this.navigateTactically = false;
			if (!this.LevelStartSelected)
			{
				this.LevelStartSelected = true;
				this.UpdatePath();
			}
		}

		// Token: 0x06005DCF RID: 24015 RVA: 0x0030EC3B File Offset: 0x0030CE3B
		public void SetDestinationLevelEnd()
		{
			this.AutoPilot = true;
			this.MaintainPos = false;
			this.posToMaintain = null;
			this.LevelStartSelected = false;
			this.navigateTactically = false;
			if (!this.LevelEndSelected)
			{
				this.LevelEndSelected = true;
				this.UpdatePath();
			}
		}

		// Token: 0x06005DD0 RID: 24016 RVA: 0x0030EC7A File Offset: 0x0030CE7A
		private void SetDestinationTactical()
		{
			this.AutoPilot = true;
			this.MaintainPos = false;
			this.posToMaintain = null;
			this.LevelStartSelected = false;
			this.LevelEndSelected = false;
			if (!this.navigateTactically)
			{
				this.navigateTactically = true;
				this.UpdatePath();
			}
		}

		// Token: 0x06005DD1 RID: 24017 RVA: 0x0030ECBC File Offset: 0x0030CEBC
		private void SetMaintainPosition()
		{
			if (!this.MaintainPos)
			{
				this.unsentChanges = true;
				this.MaintainPos = true;
			}
			if (this.posToMaintain == null)
			{
				this.unsentChanges = true;
				this.posToMaintain = new Vector2?((this.controlledSub != null) ? this.controlledSub.WorldPosition : ((this.item.Submarine == null) ? this.item.WorldPosition : this.item.Submarine.WorldPosition));
			}
		}

		// Token: 0x06005DD2 RID: 24018 RVA: 0x0030ED40 File Offset: 0x0030CF40
		private Vector2 GetSteeringVelocity(Vector2 worldPosition, float slowdownAmount)
		{
			Vector2 futurePosition = ConvertUnits.ToDisplayUnits(this.controlledSub.Velocity) * slowdownAmount;
			Vector2 targetSpeed = worldPosition - this.controlledSub.WorldPosition - futurePosition;
			if (targetSpeed.LengthSquared() > 250000f)
			{
				return Vector2.Normalize(targetSpeed) * 100f;
			}
			return targetSpeed / 5f;
		}

		// Token: 0x06005DD3 RID: 24019 RVA: 0x0030EDA8 File Offset: 0x0030CFA8
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			character.AIController.SteeringManager.Reset();
			if (objective.Override && this.user != character && this.user != null && this.user.SelectedItem == this.item && character.IsOnPlayerTeam)
			{
				character.Speak(TextManager.Get("DialogSteeringTaken").Value, null, 0f, "steeringtaken".ToIdentifier(), 10f);
			}
			this.user = character;
			if (base.Item.ConditionPercentage <= 0f && AIObjectiveRepairItems.IsValidTarget(base.Item, character))
			{
				if (base.Item.Repairables.Average((Repairable r) => r.DegreeOfSuccess(character)) > 0.4f)
				{
					objective.AddSubObjective(new AIObjectiveRepairItem(character, base.Item, objective.objectiveManager, 1f, true), false);
					return false;
				}
				Character character2 = character;
				string value = TextManager.Get("DialogNavTerminalIsBroken").Value;
				Identifier identifier = "navterminalisbroken".ToIdentifier();
				character2.Speak(value, null, 0f, identifier, 30f);
			}
			if (!this.AutoPilot)
			{
				this.unsentChanges = true;
				this.AutoPilot = true;
			}
			this.IncreaseSkillLevel(this.user, deltaTime);
			if (objective.Option == "maintainposition")
			{
				if (objective.Override)
				{
					this.SetMaintainPosition();
				}
			}
			else if (!Level.IsLoadedOutpost)
			{
				if (objective.Option == "navigateback")
				{
					if (this.DockingSources.Any((DockingPort d) => d.Docked))
					{
						this.item.SendSignal("1", "toggle_docking");
					}
					if (objective.Override)
					{
						if (this.MaintainPos || this.LevelEndSelected || !this.LevelStartSelected || this.navigateTactically)
						{
							this.unsentChanges = true;
						}
						this.SetDestinationLevelStart();
					}
				}
				else if (objective.Option == "navigatetodestination")
				{
					if (this.DockingSources.Any((DockingPort d) => d.Docked))
					{
						this.item.SendSignal("1", "toggle_docking");
					}
					if (objective.Override)
					{
						if (this.MaintainPos || !this.LevelEndSelected || this.LevelStartSelected || this.navigateTactically)
						{
							this.unsentChanges = true;
						}
						this.SetDestinationLevelEnd();
					}
				}
				else if (objective.Option == "navigatetactical")
				{
					if (this.DockingSources.Any((DockingPort d) => d.Docked))
					{
						this.item.SendSignal("1", "toggle_docking");
					}
					if (objective.Override)
					{
						if (this.MaintainPos || this.LevelEndSelected || this.LevelStartSelected || !this.navigateTactically)
						{
							this.unsentChanges = true;
						}
						this.SetDestinationTactical();
					}
				}
			}
			Sonar sonar = this.sonar;
			if (sonar != null)
			{
				sonar.CrewAIOperate(deltaTime, character, objective);
			}
			if (!this.MaintainPos && this.showIceSpireWarning && character.IsOnPlayerTeam)
			{
				character.Speak(TextManager.Get("dialogicespirespottedsonar").Value, null, 0f, "icespirespottedsonar".ToIdentifier(), 60f);
			}
			return false;
		}

		// Token: 0x06005DD4 RID: 24020 RVA: 0x0030F174 File Offset: 0x0030D374
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection.Name == "velocity_in")
			{
				this.steeringAdjustSpeed = 0.2f;
				this.steeringInput = XMLExtensions.ParseVector2(signal.value, false);
				this.steeringInput.X = MathHelper.Clamp(this.steeringInput.X, -100f, 100f);
				this.steeringInput.Y = MathHelper.Clamp(-this.steeringInput.Y, -100f, 100f);
				this.TargetVelocity = this.steeringInput;
				this.lastReceivedSteeringSignalTime = Timing.TotalTime;
				return;
			}
			base.ReceiveSignal(signal, connection);
		}

		// Token: 0x06005DDE RID: 24030 RVA: 0x0030F98B File Offset: 0x0030DB8B
		[CompilerGenerated]
		internal static bool <CreateGUI>g__OpenMap|60_4(CampaignMode campaign)
		{
			campaign.ShowCampaignUI = true;
			campaign.CampaignUI.SelectTab(CampaignMode.InteractionType.Map, null);
			return false;
		}

		// Token: 0x06005DDF RID: 24031 RVA: 0x0030F9A4 File Offset: 0x0030DBA4
		[CompilerGenerated]
		private void <CreateGUI>g__SendDockingSignal|60_5()
		{
			if (GameMain.Client == null)
			{
				this.item.SendSignal(new Signal("1", 0, Character.Controlled, null, 0f, 1f), "toggle_docking");
				return;
			}
			this.dockingNetworkMessagePending = true;
			this.item.CreateClientEvent<Steering>(this);
		}

		// Token: 0x0400302F RID: 12335
		private GUIButton steeringModeSwitch;

		// Token: 0x04003030 RID: 12336
		private GUITickBox autopilotIndicator;

		// Token: 0x04003031 RID: 12337
		private GUITickBox manualPilotIndicator;

		// Token: 0x04003032 RID: 12338
		private GUITickBox maintainPosTickBox;

		// Token: 0x04003033 RID: 12339
		private GUITickBox levelEndTickBox;

		// Token: 0x04003034 RID: 12340
		private GUITickBox levelStartTickBox;

		// Token: 0x04003035 RID: 12341
		private GUIComponent statusContainer;

		// Token: 0x04003036 RID: 12342
		private GUIComponent dockingContainer;

		// Token: 0x04003038 RID: 12344
		private bool dockingNetworkMessagePending;

		// Token: 0x04003039 RID: 12345
		private GUIButton dockingButton;

		// Token: 0x0400303A RID: 12346
		private LocalizedString dockText;

		// Token: 0x0400303B RID: 12347
		private LocalizedString undockText;

		// Token: 0x0400303C RID: 12348
		private GUIComponent steerArea;

		// Token: 0x0400303D RID: 12349
		private GUITextBlock pressureWarningText;

		// Token: 0x0400303E RID: 12350
		private GUITextBlock iceSpireWarningText;

		// Token: 0x0400303F RID: 12351
		private GUITextBlock tipContainer;

		// Token: 0x04003040 RID: 12352
		private LocalizedString noPowerTip;

		// Token: 0x04003041 RID: 12353
		private LocalizedString autoPilotMaintainPosTip;

		// Token: 0x04003042 RID: 12354
		private LocalizedString autoPilotLevelStartTip;

		// Token: 0x04003043 RID: 12355
		private LocalizedString autoPilotLevelEndTip;

		// Token: 0x04003044 RID: 12356
		private Sprite maintainPosIndicator;

		// Token: 0x04003045 RID: 12357
		private Sprite maintainPosOriginIndicator;

		// Token: 0x04003046 RID: 12358
		private Sprite steeringIndicator;

		// Token: 0x04003047 RID: 12359
		private List<DockingPort> connectedPorts = new List<DockingPort>();

		// Token: 0x04003048 RID: 12360
		private float checkConnectedPortsTimer;

		// Token: 0x04003049 RID: 12361
		private const float CheckConnectedPortsInterval = 1f;

		// Token: 0x0400304A RID: 12362
		public DockingPort ActiveDockingSource;

		// Token: 0x0400304B RID: 12363
		public DockingPort DockingTarget;

		// Token: 0x0400304C RID: 12364
		private Vector2 keyboardInput = Vector2.Zero;

		// Token: 0x0400304D RID: 12365
		private float inputCumulation;

		// Token: 0x0400304E RID: 12366
		private bool? swapDestinationOrder;

		// Token: 0x0400304F RID: 12367
		private GUIMessageBox enterOutpostPrompt;

		// Token: 0x04003050 RID: 12368
		private GUIMessageBox exitOutpostPrompt;

		// Token: 0x04003051 RID: 12369
		private bool levelStartSelected;

		// Token: 0x04003052 RID: 12370
		private bool levelEndSelected;

		// Token: 0x04003053 RID: 12371
		private bool maintainPos;

		// Token: 0x04003054 RID: 12372
		private float steerRadius;

		// Token: 0x04003055 RID: 12373
		private bool disableControls;

		// Token: 0x04003056 RID: 12374
		public const float AutopilotMinDistToPathNode = 30f;

		// Token: 0x04003057 RID: 12375
		private const float AutopilotRayCastInterval = 0.5f;

		// Token: 0x04003058 RID: 12376
		private const float RecalculatePathInterval = 5f;

		// Token: 0x04003059 RID: 12377
		private const float AutoPilotSteeringLerp = 0.1f;

		// Token: 0x0400305A RID: 12378
		private const float AutoPilotMaxSpeed = 0.5f;

		// Token: 0x0400305B RID: 12379
		private const float AIPilotMaxSpeed = 1f;

		// Token: 0x0400305C RID: 12380
		public const float PressureWarningThreshold = 500f;

		// Token: 0x0400305D RID: 12381
		private const float DefaultSteeringAdjustSpeed = 0.2f;

		// Token: 0x0400305E RID: 12382
		private Vector2 targetVelocity;

		// Token: 0x0400305F RID: 12383
		private Vector2 steeringInput;

		// Token: 0x04003060 RID: 12384
		private bool autoPilot;

		// Token: 0x04003061 RID: 12385
		private Vector2? posToMaintain;

		// Token: 0x04003062 RID: 12386
		private SteeringPath steeringPath;

		// Token: 0x04003063 RID: 12387
		private PathFinder pathFinder;

		// Token: 0x04003064 RID: 12388
		private float networkUpdateTimer;

		// Token: 0x04003065 RID: 12389
		private bool unsentChanges;

		// Token: 0x04003066 RID: 12390
		private float autopilotRayCastTimer;

		// Token: 0x04003067 RID: 12391
		private float autopilotRecalculatePathTimer;

		// Token: 0x04003068 RID: 12392
		private Vector2 avoidStrength;

		// Token: 0x04003069 RID: 12393
		private float neutralBallastLevel;

		// Token: 0x0400306A RID: 12394
		private float steeringAdjustSpeed = 1f;

		// Token: 0x0400306B RID: 12395
		private Character user;

		// Token: 0x0400306C RID: 12396
		private Sonar sonar;

		// Token: 0x0400306D RID: 12397
		private Submarine controlledSub;

		// Token: 0x04003070 RID: 12400
		private bool navigateTactically;

		// Token: 0x04003071 RID: 12401
		private bool showIceSpireWarning;

		// Token: 0x04003072 RID: 12402
		private List<Submarine> connectedSubs = new List<Submarine>();

		// Token: 0x04003073 RID: 12403
		private const float ConnectedSubUpdateInterval = 1f;

		// Token: 0x04003074 RID: 12404
		private float connectedSubUpdateTimer;

		// Token: 0x04003075 RID: 12405
		private double lastReceivedSteeringSignalTime;

		// Token: 0x04003077 RID: 12407
		private List<Steering.ObstacleDebugInfo> debugDrawObstacles = new List<Steering.ObstacleDebugInfo>();

		// Token: 0x04003078 RID: 12408
		public List<DockingPort> DockingSources = new List<DockingPort>();

		// Token: 0x04003079 RID: 12409
		private bool searchedConnectedDockingPort;

		// Token: 0x0400307A RID: 12410
		private bool dockingModeEnabled;

		// Token: 0x0200142C RID: 5164
		private enum Destination
		{
			// Token: 0x040064AF RID: 25775
			MaintainPos,
			// Token: 0x040064B0 RID: 25776
			LevelEnd,
			// Token: 0x040064B1 RID: 25777
			LevelStart
		}

		// Token: 0x0200142D RID: 5165
		private struct ObstacleDebugInfo
		{
			// Token: 0x060099DA RID: 39386 RVA: 0x003E143F File Offset: 0x003DF63F
			public ObstacleDebugInfo(GraphEdge edge, Vector2? intersection, float dot, Vector2 avoidStrength, Vector2 translation)
			{
				this.Point1 = edge.Point1 + translation;
				this.Point2 = edge.Point2 + translation;
				this.Intersection = intersection;
				this.Dot = dot;
				this.AvoidStrength = avoidStrength;
			}

			// Token: 0x040064B2 RID: 25778
			public Vector2 Point1;

			// Token: 0x040064B3 RID: 25779
			public Vector2 Point2;

			// Token: 0x040064B4 RID: 25780
			public Vector2? Intersection;

			// Token: 0x040064B5 RID: 25781
			public float Dot;

			// Token: 0x040064B6 RID: 25782
			public Vector2 AvoidStrength;
		}
	}
}
