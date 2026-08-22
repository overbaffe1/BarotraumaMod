using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C7 RID: 1479
	internal class Sonar : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x17001778 RID: 6008
		// (get) Token: 0x06005D37 RID: 23863 RVA: 0x0030351D File Offset: 0x0030171D
		// (set) Token: 0x06005D38 RID: 23864 RVA: 0x00303525 File Offset: 0x00301725
		public GUIButton SonarModeSwitch { get; private set; }

		// Token: 0x17001779 RID: 6009
		// (get) Token: 0x06005D39 RID: 23865 RVA: 0x0030352E File Offset: 0x0030172E
		// (set) Token: 0x06005D3A RID: 23866 RVA: 0x00303536 File Offset: 0x00301736
		public float DisplayScale { get; private set; } = 1f;

		// Token: 0x1700177A RID: 6010
		// (get) Token: 0x06005D3B RID: 23867 RVA: 0x0030353F File Offset: 0x0030173F
		// (set) Token: 0x06005D3C RID: 23868 RVA: 0x00303547 File Offset: 0x00301747
		public Vector2 DisplayOffset { get; private set; }

		// Token: 0x1700177B RID: 6011
		// (get) Token: 0x06005D3D RID: 23869 RVA: 0x00303550 File Offset: 0x00301750
		// (set) Token: 0x06005D3E RID: 23870 RVA: 0x00303558 File Offset: 0x00301758
		public float DisplayRadius { get; private set; }

		// Token: 0x1700177C RID: 6012
		// (get) Token: 0x06005D3F RID: 23871 RVA: 0x00303561 File Offset: 0x00301761
		public static Vector2 GUISizeCalculation
		{
			get
			{
				return Vector2.One * Math.Min(GUI.RelativeHorizontalAspectRatio, 1f) * Sonar.sonarAreaSize;
			}
		}

		// Token: 0x1700177D RID: 6013
		// (get) Token: 0x06005D40 RID: 23872 RVA: 0x00303586 File Offset: 0x00301786
		// (set) Token: 0x06005D41 RID: 23873 RVA: 0x0030358E File Offset: 0x0030178E
		[TupleElementNames(new string[]
		{
			"center",
			"resources"
		})]
		private List<ValueTuple<Vector2, List<Item>>> MineralClusters { [return: TupleElementNames(new string[]
		{
			"center",
			"resources"
		})] get; [param: TupleElementNames(new string[]
		{
			"center",
			"resources"
		})] set; }

		// Token: 0x1700177E RID: 6014
		// (get) Token: 0x06005D42 RID: 23874 RVA: 0x00303597 File Offset: 0x00301797
		// (set) Token: 0x06005D43 RID: 23875 RVA: 0x0030359F File Offset: 0x0030179F
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool RightLayout { get; set; }

		// Token: 0x1700177F RID: 6015
		// (get) Token: 0x06005D44 RID: 23876 RVA: 0x003035A8 File Offset: 0x003017A8
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06005D45 RID: 23877 RVA: 0x003035AB File Offset: 0x003017AB
		protected override void OnResolutionChanged()
		{
			this.UpdateGUIElements();
		}

		// Token: 0x06005D46 RID: 23878 RVA: 0x003035B4 File Offset: 0x003017B4
		protected override void CreateGUI()
		{
			this.isConnectedToSteering = (this.item.GetComponent<Steering>() != null);
			Vector2 size = this.isConnectedToSteering ? Sonar.controlBoxSize : new Vector2(0.46f, 0.4f);
			this.controlContainer = new GUIFrame(new RectTransform(size, base.GuiFrame.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), "ItemUI", null);
			if (!this.isConnectedToSteering && GUI.AspectRatioDifference <= 0f)
			{
				this.controlContainer.RectTransform.MaxSize = new Point((int)(380f * GUI.xScale), (int)(300f * GUI.yScale));
			}
			GUIFrame paddedControlContainer = new GUIFrame(new RectTransform(this.controlContainer.Rect.Size - GUIStyle.ItemFrameMargin, this.controlContainer.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, null, null);
			float extraHeight = 0.0694f;
			GUIFrame sonarModeArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.4f + extraHeight), paddedControlContainer.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), null, null);
			this.SonarModeSwitch = new GUIButton(new RectTransform(new Vector2(0.2f, 1f), sonarModeArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, Alignment.Center, "SwitchVertical", null)
			{
				UserData = UIHighlightAction.ElementId.SonarModeSwitch,
				Selected = false,
				Enabled = true,
				ClickSound = GUISoundType.UISwitch,
				OnClicked = delegate(GUIButton button, object data)
				{
					button.Selected = !button.Selected;
					this.CurrentMode = (button.Selected ? Sonar.Mode.Active : Sonar.Mode.Passive);
					if (GameMain.Client != null)
					{
						this.unsentChanges = true;
						this.correctionTimer = 1f;
					}
					return true;
				}
			};
			GUIFrame sonarModeRightSide = new GUIFrame(new RectTransform(new Vector2(0.7f, 0.8f), sonarModeArea.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(this.SonarModeSwitch.RectTransform.RelativeSize.X, 0f)
			}, null, null);
			this.passiveTickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.45f), sonarModeRightSide.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SonarPassive"), GUIStyle.SubHeadingFont, "IndicatorLightRedSmall")
			{
				UserData = UIHighlightAction.ElementId.PassiveSonarIndicator,
				ToolTip = TextManager.Get("SonarTipPassive"),
				Selected = true,
				Enabled = false
			};
			this.activeTickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.45f), sonarModeRightSide.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SonarActive"), GUIStyle.SubHeadingFont, "IndicatorLightRedSmall")
			{
				UserData = UIHighlightAction.ElementId.ActiveSonarIndicator,
				ToolTip = TextManager.Get("SonarTipActive"),
				Selected = false,
				Enabled = false
			};
			this.passiveTickBox.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			this.activeTickBox.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			this.textBlocksToScaleAndNormalize.Clear();
			this.textBlocksToScaleAndNormalize.Add(this.passiveTickBox.TextBlock);
			this.textBlocksToScaleAndNormalize.Add(this.activeTickBox.TextBlock);
			this.lowerAreaFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.4f + extraHeight), paddedControlContainer.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame zoomContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.45f), this.lowerAreaFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), null, null);
			RectTransform rectT = new RectTransform(new Vector2(0.3f, 0.6f), zoomContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("SonarZoom");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock zoomText = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.CenterRight, false, "", null);
			this.textBlocksToScaleAndNormalize.Add(zoomText);
			RectTransform rectTransform = new RectTransform(new Vector2(0.5f, 0.8f), zoomContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.RelativeOffset = new Vector2(0.35f, 0f);
			float barSize = 0.15f;
			bool? isHorizontal = new bool?(true);
			this.zoomSlider = new GUIScrollBar(rectTransform, barSize, null, "DeviceSlider", isHorizontal)
			{
				OnMoved = delegate(GUIScrollBar scrollbar, float scroll)
				{
					this.zoom = MathHelper.Lerp(1f, 4f, scroll);
					if (GameMain.Client != null)
					{
						this.unsentChanges = true;
						this.correctionTimer = 1f;
					}
					return true;
				}
			};
			new GUIFrame(new RectTransform(new Vector2(0.8f, 0.01f), paddedControlContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "HorizontalLine", null).UserData = "horizontalline";
			GUIFrame directionalModeFrame = new GUIFrame(new RectTransform(new Vector2(1f, 0.45f), this.lowerAreaFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, null)
			{
				UserData = UIHighlightAction.ElementId.DirectionalSonarFrame
			};
			this.directionalModeSwitch = new GUIButton(new RectTransform(new Vector2(0.3f, 0.8f), directionalModeFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), string.Empty, Alignment.Center, "SwitchHorizontal", null)
			{
				OnClicked = delegate(GUIButton button, object data)
				{
					this.useDirectionalPing = !this.useDirectionalPing;
					button.Selected = this.useDirectionalPing;
					if (GameMain.Client != null)
					{
						this.unsentChanges = true;
						this.correctionTimer = 1f;
					}
					return true;
				}
			};
			GUITextBlock directionalModeSwitchText = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1f), directionalModeFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("SonarDirectionalPing"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null);
			this.textBlocksToScaleAndNormalize.Add(directionalModeSwitchText);
			if (this.HasMineralScanner)
			{
				this.AddMineralScannerSwitchToGUI();
			}
			else
			{
				this.mineralScannerSwitch = null;
			}
			base.GuiFrame.CanBeFocused = false;
			GUITextBlock.AutoScaleAndNormalize(this.textBlocksToScaleAndNormalize, true, false, null);
			this.sonarView = new GUICustomComponent(new RectTransform(Vector2.One * 0.7f, base.GuiFrame.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.BothHeight), delegate(SpriteBatch spriteBatch, GUICustomComponent guiCustomComponent)
			{
				this.DrawSonar(spriteBatch, guiCustomComponent.Rect);
			}, null);
			this.signalWarningText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.25f), this.sonarView.RectTransform, Anchor.Center, new Pivot?(Pivot.BottomCenter), null, null, ScaleBasis.Normal), "", new Color?(this.warningColor), GUIStyle.LargeFont, Alignment.Center, false, "", null);
			if (this.isConnectedToSteering || this.RightLayout)
			{
				this.controlContainer.RectTransform.AbsoluteOffset = Point.Zero;
				this.controlContainer.RectTransform.RelativeOffset = Sonar.controlBoxOffset;
				this.controlContainer.RectTransform.SetPosition(Anchor.TopRight, null);
				this.sonarView.RectTransform.ScaleBasis = ScaleBasis.Smallest;
				if (this.HasMineralScanner)
				{
					this.PreventMineralScannerOverlap();
				}
				this.sonarView.RectTransform.SetPosition(Anchor.CenterLeft, null);
				this.sonarView.RectTransform.Resize(Sonar.GUISizeCalculation, true);
				GUITextBlock.AutoScaleAndNormalize(this.textBlocksToScaleAndNormalize, true, false, null);
			}
			else if (GUI.RelativeHorizontalAspectRatio > 0.75f)
			{
				this.sonarView.RectTransform.RelativeOffset = new Vector2(0.13f * GUI.RelativeHorizontalAspectRatio, 0f);
				this.sonarView.RectTransform.SetPosition(Anchor.BottomRight, null);
			}
			GUIDragHandle handle = base.GuiFrame.GetChild<GUIDragHandle>();
			if (handle != null)
			{
				handle.RectTransform.Parent = this.controlContainer.RectTransform;
				handle.RectTransform.Resize(Vector2.One, true);
				handle.RectTransform.SetAsFirstChild();
			}
		}

		// Token: 0x06005D47 RID: 23879 RVA: 0x00303F5F File Offset: 0x0030215F
		private void SetPingDirection(Vector2 direction)
		{
			this.pingDirection = direction;
			if (GameMain.Client != null)
			{
				this.unsentChanges = true;
				this.correctionTimer = 1f;
			}
		}

		// Token: 0x06005D48 RID: 23880 RVA: 0x00303F84 File Offset: 0x00302184
		private Vector2 GetTransducerPos()
		{
			if (this.UseTransducers && this.connectedTransducers.Count != 0)
			{
				Vector2 transducerPosSum = Vector2.Zero;
				foreach (Sonar.ConnectedTransducer transducer in this.connectedTransducers)
				{
					if (transducer.Transducer.Item.Submarine != null && !this.CenterOnTransducers)
					{
						return transducer.Transducer.Item.Submarine.WorldPosition;
					}
					transducerPosSum += transducer.Transducer.Item.WorldPosition;
				}
				return transducerPosSum / (float)this.connectedTransducers.Count;
			}
			if (this.item.Submarine == null || this.item.body != null)
			{
				return this.item.WorldPosition;
			}
			return this.item.Submarine.WorldPosition;
		}

		// Token: 0x06005D49 RID: 23881 RVA: 0x00304080 File Offset: 0x00302280
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.zoomSlider.BarScroll = MathUtils.InverseLerp(1f, 4f, this.zoom);
			if (this.HasMineralScanner && this.mineralScannerSwitch == null)
			{
				this.AddMineralScannerSwitchToGUI();
				GUITextBlock.AutoScaleAndNormalize(this.textBlocksToScaleAndNormalize, true, false, null);
			}
			Steering component = this.item.GetComponent<Steering>();
			if (component == null)
			{
				return;
			}
			component.AttachToSonarHUD(this.sonarView);
		}

		// Token: 0x06005D4A RID: 23882 RVA: 0x003040FC File Offset: 0x003022FC
		private void AddMineralScannerSwitchToGUI()
		{
			this.controlContainer.RectTransform.RelativeSize = new Vector2(this.controlContainer.RectTransform.RelativeSize.X, this.controlContainer.RectTransform.RelativeSize.Y * 1.25f);
			this.SonarModeSwitch.Parent.RectTransform.RelativeSize = new Vector2(this.SonarModeSwitch.Parent.RectTransform.RelativeSize.X, this.SonarModeSwitch.Parent.RectTransform.RelativeSize.Y * 0.8f);
			this.lowerAreaFrame.Parent.GetChildByUserData("horizontalline").RectTransform.RelativeOffset = new Vector2(0f, -0.1f);
			this.lowerAreaFrame.RectTransform.RelativeSize = new Vector2(this.lowerAreaFrame.RectTransform.RelativeSize.X, this.lowerAreaFrame.RectTransform.RelativeSize.Y * 1.2f);
			this.zoomSlider.Parent.RectTransform.RelativeSize = new Vector2(this.zoomSlider.Parent.RectTransform.RelativeSize.X, this.zoomSlider.Parent.RectTransform.RelativeSize.Y * 0.6666667f);
			this.directionalModeSwitch.Parent.RectTransform.RelativeSize = new Vector2(this.directionalModeSwitch.Parent.RectTransform.RelativeSize.X, this.zoomSlider.Parent.RectTransform.RelativeSize.Y);
			this.directionalModeSwitch.Parent.RectTransform.SetPosition(Anchor.Center, null);
			GUIFrame mineralScannerFrame = new GUIFrame(new RectTransform(new Vector2(1f, this.zoomSlider.Parent.RectTransform.RelativeSize.Y), this.lowerAreaFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, null);
			this.mineralScannerSwitch = new GUIButton(new RectTransform(new Vector2(0.3f, 0.8f), mineralScannerFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), string.Empty, Alignment.Center, "SwitchHorizontal", null)
			{
				Selected = this.UseMineralScanner,
				OnClicked = delegate(GUIButton button, object data)
				{
					this.UseMineralScanner = !this.UseMineralScanner;
					button.Selected = this.UseMineralScanner;
					if (GameMain.Client != null)
					{
						this.unsentChanges = true;
						this.correctionTimer = 1f;
					}
					return true;
				}
			};
			GUITextBlock mineralScannerSwitchText = new GUITextBlock(new RectTransform(new Vector2(0.7f, 1f), mineralScannerFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("SonarMineralScanner"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null);
			this.textBlocksToScaleAndNormalize.Add(mineralScannerSwitchText);
			this.PreventMineralScannerOverlap();
		}

		// Token: 0x06005D4B RID: 23883 RVA: 0x00304438 File Offset: 0x00302638
		private void PreventMineralScannerOverlap()
		{
			Steering steering = this.item.GetComponent<Steering>();
			if (steering != null)
			{
				GUIFrame container = this.controlContainer;
				if (container != null)
				{
					Sonar.<>c__DisplayClass88_0 CS$<>8__locals1;
					CS$<>8__locals1.containerBottom = container.Rect.Y + container.Rect.Height;
					int steeringTop = steering.ControlContainer.Rect.Top;
					CS$<>8__locals1.amountRaised = 0;
					while (Sonar.<PreventMineralScannerOverlap>g__GetContainerBottom|88_0(ref CS$<>8__locals1) > steeringTop)
					{
						int amountRaised = CS$<>8__locals1.amountRaised;
						CS$<>8__locals1.amountRaised = amountRaised + 1;
					}
					container.RectTransform.AbsoluteOffset = new Point(0, -CS$<>8__locals1.amountRaised);
				}
			}
		}

		// Token: 0x06005D4C RID: 23884 RVA: 0x003044D0 File Offset: 0x003026D0
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			this.showDirectionalIndicatorTimer -= deltaTime;
			if (GameMain.Client != null)
			{
				if (this.unsentChanges && this.networkUpdateTimer <= 0f)
				{
					this.item.CreateClientEvent<Sonar>(this);
					this.correctionTimer = 1f;
					this.networkUpdateTimer = 0.1f;
					this.unsentChanges = false;
				}
				this.networkUpdateTimer -= deltaTime;
			}
			this.connectedSubUpdateTimer -= deltaTime;
			if (this.connectedSubUpdateTimer <= 0f)
			{
				this.connectedSubs.Clear();
				if (this.UseTransducers)
				{
					using (List<Sonar.ConnectedTransducer>.Enumerator enumerator = this.connectedTransducers.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Sonar.ConnectedTransducer transducer = enumerator.Current;
							if (transducer.Transducer.Item.Submarine != null && !this.connectedSubs.Contains(transducer.Transducer.Item.Submarine))
							{
								this.connectedSubs.AddRange(transducer.Transducer.Item.Submarine.GetConnectedSubs());
							}
						}
						goto IL_147;
					}
				}
				if (this.item.Submarine != null)
				{
					List<Submarine> list = this.connectedSubs;
					Submarine submarine = this.item.Submarine;
					list.AddRange((submarine != null) ? submarine.GetConnectedSubs() : null);
				}
				IL_147:
				this.connectedSubUpdateTimer = 1f;
			}
			Steering steering = this.item.GetComponent<Steering>();
			if (this.sonarView.Rect.Contains(PlayerInput.MousePosition))
			{
				if (GUI.MouseOn != null && GUI.MouseOn != this.sonarView && !this.sonarView.IsParentOf(GUI.MouseOn, true) && GUI.MouseOn != ((steering != null) ? steering.GuiFrame : null))
				{
					bool? flag;
					if (steering == null)
					{
						flag = null;
					}
					else
					{
						GUIFrame guiFrame = steering.GuiFrame;
						flag = ((guiFrame != null) ? new bool?(guiFrame.IsParentOf(GUI.MouseOn, true)) : null);
					}
					bool? flag2 = flag;
					if (!flag2.GetValueOrDefault())
					{
						goto IL_252;
					}
				}
				float scrollSpeed = (float)PlayerInput.ScrollWheelSpeed / 1000f;
				if (Math.Abs(scrollSpeed) > 0.0001f)
				{
					this.zoomSlider.BarScroll += (float)PlayerInput.ScrollWheelSpeed / 1000f;
					this.zoomSlider.OnMoved(this.zoomSlider, this.zoomSlider.BarScroll);
				}
			}
			IL_252:
			Vector2 transducerCenter = this.GetTransducerPos();
			if (steering != null && steering.DockingModeEnabled && steering.ActiveDockingSource != null)
			{
				Vector2 worldFocusPos = (steering.ActiveDockingSource.Item.WorldPosition + steering.DockingTarget.Item.WorldPosition) / 2f;
				this.DisplayOffset = Vector2.Lerp(this.DisplayOffset, worldFocusPos - transducerCenter, 0.1f);
			}
			else
			{
				this.DisplayOffset = Vector2.Lerp(this.DisplayOffset, Vector2.Zero, 0.1f);
			}
			transducerCenter += this.DisplayOffset;
			float distort = MathHelper.Clamp(1f - this.item.Condition / this.item.MaxCondition, 0f, 1f);
			int i;
			for (i = this.sonarBlips.Count - 1; i >= 0; i--)
			{
				this.sonarBlips[i].FadeTimer -= deltaTime * MathHelper.Lerp(0.5f, 2f, distort);
				this.sonarBlips[i].Position += this.sonarBlips[i].Velocity * deltaTime;
				if (this.sonarBlips[i].FadeTimer <= 0f)
				{
					this.sonarBlips.RemoveAt(i);
				}
			}
			this.sonarView.CanBeFocused = (Vector2.DistanceSquared(this.sonarView.Rect.Center.ToVector2(), PlayerInput.MousePosition) < (float)(this.sonarView.Rect.Width / 2 * this.sonarView.Rect.Width / 2));
			if (this.HasMineralScanner && Level.Loaded != null && !Level.Loaded.Generating)
			{
				Character c;
				if (this.MineralClusters == null)
				{
					this.MineralClusters = new List<ValueTuple<Vector2, List<Item>>>();
					Level.Loaded.PathPoints.ForEach(delegate(Level.PathPoint p)
					{
						p.ClusterLocations.ForEach(delegate(Level.ClusterLocation c)
						{
							this.<UpdateHUDComponentSpecific>g__AddIfValid|89_2(c);
						});
					});
					Level.Loaded.AbyssResources.ForEach(delegate(Level.ClusterLocation c)
					{
						this.<UpdateHUDComponentSpecific>g__AddIfValid|89_2(c);
					});
					if (GameMain.GameSession == null)
					{
						goto IL_595;
					}
					using (IEnumerator<Mission> enumerator2 = GameMain.GameSession.Missions.GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							Mission mission = enumerator2.Current;
							MineralMission mineralMission = mission as MineralMission;
							if (mineralMission != null)
							{
								foreach (List<Item> minerals in mineralMission.SpawnedResources)
								{
									this.MineralClusters.Add(new ValueTuple<Vector2, List<Item>>(new Vector2(minerals.Average((Item m) => m.WorldPosition.X), minerals.Average((Item m) => m.WorldPosition.Y)), minerals));
								}
							}
						}
						goto IL_595;
					}
				}
				this.MineralClusters.RemoveAll(delegate([TupleElementNames(new string[]
				{
					"center",
					"resources"
				})] ValueTuple<Vector2, List<Item>> c)
				{
					if (c.Item2 != null && !c.Item2.None(null))
					{
						return c.Item2.All((Item i) => i == null || i.Removed);
					}
					return true;
				});
			}
			IL_595:
			if (this.UseTransducers && this.connectedTransducers.Count == 0)
			{
				return;
			}
			if (Level.Loaded != null)
			{
				this.nearbyObjectUpdateTimer -= deltaTime;
				if (this.nearbyObjectUpdateTimer <= 0f)
				{
					this.nearbyObjects.Clear();
					foreach (LevelObject nearbyObject in Level.Loaded.LevelObjectManager.GetAllObjects(transducerCenter, this.range * this.zoom))
					{
						if (nearbyObject.VisibleOnSonar)
						{
							float objectRange = this.range + nearbyObject.SonarRadius;
							if (Vector2.DistanceSquared(transducerCenter, nearbyObject.WorldPosition) < objectRange * objectRange)
							{
								this.nearbyObjects.Add(nearbyObject);
							}
						}
					}
					this.nearbyObjectUpdateTimer = 1f;
				}
				List<LevelTrigger> ballastFloraSpores = new List<LevelTrigger>();
				Dictionary<LevelTrigger, Vector2> levelTriggerFlows = new Dictionary<LevelTrigger, Vector2>();
				for (int pingIndex = 0; pingIndex < this.activePingsCount; pingIndex++)
				{
					Sonar.ActivePing activePing = this.activePings[pingIndex];
					float pingRange = this.range * activePing.State / this.zoom;
					foreach (LevelObject levelObject in this.nearbyObjects)
					{
						if (levelObject.Triggers != null)
						{
							foreach (LevelTrigger trigger in levelObject.Triggers)
							{
								Vector2 flow = trigger.GetWaterFlowVelocity();
								if (flow.LengthSquared() >= 1f && !levelTriggerFlows.ContainsKey(trigger))
								{
									levelTriggerFlows.Add(trigger, flow);
								}
								if (!trigger.InfectIdentifier.IsEmpty && Vector2.DistanceSquared(transducerCenter, trigger.WorldPosition) < pingRange / 2f * pingRange / 2f)
								{
									ballastFloraSpores.Add(trigger);
								}
							}
						}
					}
				}
				foreach (KeyValuePair<LevelTrigger, Vector2> triggerFlow in levelTriggerFlows)
				{
					LevelTrigger trigger2 = triggerFlow.Key;
					Vector2 flow2 = triggerFlow.Value;
					float flowMagnitude = flow2.Length();
					if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < flowMagnitude / 1000f)
					{
						float edgeDist = Rand.Range(0f, 1f, Rand.RandSync.Unsynced);
						Vector2 blipPos = trigger2.WorldPosition + Rand.Vector(trigger2.ColliderRadius * edgeDist, Rand.RandSync.Unsynced);
						Vector2 blipVel = flow2;
						foreach (KeyValuePair<LevelTrigger, Vector2> triggerFlow2 in levelTriggerFlows)
						{
							LevelTrigger trigger3 = triggerFlow2.Key;
							if (trigger3 != trigger2 && Vector2.DistanceSquared(blipPos, trigger3.WorldPosition) < trigger3.ColliderRadius * trigger3.ColliderRadius)
							{
								Vector2 trigger2flow = triggerFlow2.Value;
								if (trigger3.ForceFalloff)
								{
									trigger2flow *= 1f - Vector2.Distance(blipPos, trigger3.WorldPosition) / trigger3.ColliderRadius;
								}
								blipVel += trigger2flow;
							}
						}
						SonarBlip flowBlip = new SonarBlip(blipPos, Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced), 1f, Sonar.BlipType.Default)
						{
							Velocity = blipVel * Rand.Range(1f, 5f, Rand.RandSync.Unsynced),
							Size = new Vector2(MathHelper.Lerp(0.4f, 5f, flowMagnitude / 500f), 0.2f),
							Rotation = new float?((float)Math.Atan2((double)(-(double)blipVel.Y), (double)blipVel.X))
						};
						this.sonarBlips.Add(flowBlip);
					}
				}
				foreach (LevelTrigger spore in ballastFloraSpores)
				{
					Vector2 blipPos2 = spore.WorldPosition + Rand.Vector(spore.ColliderRadius * Rand.Range(0f, 1f, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced);
					SonarBlip sporeBlip = new SonarBlip(blipPos2, Rand.Range(0.1f, 0.5f, Rand.RandSync.Unsynced), 0.5f, Sonar.BlipType.Default)
					{
						Rotation = new float?(Rand.Range(-6.2831855f, 6.2831855f, Rand.RandSync.Unsynced)),
						BlipType = Sonar.BlipType.Default,
						Velocity = Rand.Vector(100f, Rand.RandSync.Unsynced)
					};
					this.sonarBlips.Add(sporeBlip);
				}
				float outsideLevelFlow = 0f;
				if (transducerCenter.X < 0f)
				{
					outsideLevelFlow = Math.Abs(transducerCenter.X * 0.001f);
				}
				else if (transducerCenter.X > (float)Level.Loaded.Size.X)
				{
					outsideLevelFlow = -(transducerCenter.X - (float)Level.Loaded.Size.X) * 0.001f;
				}
				if (Rand.Range(0f, 100f, Rand.RandSync.Unsynced) < Math.Abs(outsideLevelFlow))
				{
					Vector2 blipPos3 = transducerCenter + Rand.Vector(Rand.Range(0f, this.range, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced);
					SonarBlip flowBlip2 = new SonarBlip(blipPos3, Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced), 1f, Sonar.BlipType.Default)
					{
						Velocity = Vector2.UnitX * outsideLevelFlow * Rand.Range(50f, 100f, Rand.RandSync.Unsynced),
						Size = new Vector2(Rand.Range(0.4f, 5f, Rand.RandSync.Unsynced), 0.2f),
						Rotation = new float?(0f)
					};
					this.sonarBlips.Add(flowBlip2);
				}
			}
			if (steering != null && steering.DockingModeEnabled && steering.ActiveDockingSource != null)
			{
				float dockingDist = Vector2.Distance(steering.ActiveDockingSource.Item.WorldPosition, steering.DockingTarget.Item.WorldPosition);
				if (this.prevDockingDist > steering.DockingAssistThreshold && dockingDist <= steering.DockingAssistThreshold)
				{
					this.zoomSlider.BarScroll = 0.25f;
					this.zoom = Math.Max(this.zoom, MathHelper.Lerp(1f, 4f, this.zoomSlider.BarScroll));
				}
				else if (this.prevDockingDist > steering.DockingAssistThreshold * 0.75f && dockingDist <= steering.DockingAssistThreshold * 0.75f)
				{
					this.zoomSlider.BarScroll = 0.5f;
					this.zoom = Math.Max(this.zoom, MathHelper.Lerp(1f, 4f, this.zoomSlider.BarScroll));
				}
				else if (this.prevDockingDist > steering.DockingAssistThreshold * 0.5f && dockingDist <= steering.DockingAssistThreshold * 0.5f)
				{
					this.zoomSlider.BarScroll = 0.25f;
					this.zoom = Math.Max(this.zoom, MathHelper.Lerp(1f, 4f, this.zoomSlider.BarScroll));
				}
				this.prevDockingDist = Math.Min(dockingDist, this.prevDockingDist);
			}
			else
			{
				this.prevDockingDist = float.MaxValue;
			}
			if (steering != null && this.directionalPingButton != null)
			{
				steering.SteerRadius = ((this.useDirectionalPing && this.pingDragDirection != null) ? new float?(-1f) : ((PlayerInput.PrimaryMouseButtonDown() || !PlayerInput.PrimaryMouseButtonHeld()) ? new float?((float)(this.sonarView.Rect.Width / 2) - this.directionalPingButton[0].size.X * (float)this.sonarView.Rect.Width / this.screenBackground.size.X) : null));
			}
			if (this.useDirectionalPing)
			{
				Vector2 newDragDir = Vector2.Normalize(PlayerInput.MousePosition - this.sonarView.Rect.Center.ToVector2());
				if (this.MouseInDirectionalPingRing(this.sonarView.Rect, true) && PlayerInput.PrimaryMouseButtonDown())
				{
					this.pingDragDirection = new Vector2?(newDragDir);
				}
				if (this.pingDragDirection != null && PlayerInput.PrimaryMouseButtonHeld())
				{
					float newAngle = MathUtils.WrapAngleTwoPi(MathUtils.VectorToAngle(newDragDir));
					this.SetPingDirection(new Vector2((float)Math.Cos((double)newAngle), (float)Math.Sin((double)newAngle)));
				}
				else
				{
					this.pingDragDirection = null;
				}
			}
			else
			{
				this.pingDragDirection = null;
			}
			this.disruptionUpdateTimer -= deltaTime;
			for (int pingIndex2 = 0; pingIndex2 < this.activePingsCount; pingIndex2++)
			{
				Sonar.ActivePing activePing2 = this.activePings[pingIndex2];
				float pingRadius = this.DisplayRadius * activePing2.State / this.zoom;
				if (this.disruptionUpdateTimer <= 0f)
				{
					this.UpdateDisruptions(transducerCenter, pingRadius / this.DisplayScale);
				}
				this.Ping(transducerCenter, transducerCenter, pingRadius, activePing2.PrevPingRadius, this.DisplayScale, this.range / this.zoom, false, 2f, null);
				activePing2.PrevPingRadius = pingRadius;
			}
			if (this.disruptionUpdateTimer <= 0f)
			{
				this.disruptionUpdateTimer = 0.2f;
			}
			this.longRangeUpdateTimer -= deltaTime;
			if (this.longRangeUpdateTimer <= 0f)
			{
				foreach (Character c in Character.CharacterList)
				{
					if (c.AnimController.CurrentHull == null && c.Enabled && !c.Params.HideInSonar && !c.IsUnconscious && c.Params.DistantSonarRange > 0f && ((c.WorldPosition - transducerCenter) * this.DisplayScale).LengthSquared() > this.DisplayRadius * this.DisplayRadius)
					{
						Vector2 targetVector = c.WorldPosition - transducerCenter;
						if (targetVector.LengthSquared() <= MathUtils.Pow2(c.Params.DistantSonarRange))
						{
							float dist = targetVector.Length();
							Vector2 targetDir = targetVector / dist;
							int blipCount = (int)MathHelper.Clamp(c.Mass, 50f, 200f);
							for (int j = 0; j < blipCount; j++)
							{
								float angle = Rand.Range(-0.5f, 0.5f, Rand.RandSync.Unsynced);
								Vector2 blipDir = MathUtils.RotatePoint(targetDir, angle);
								Vector2 invBlipDir = MathUtils.RotatePoint(targetDir, -angle);
								SonarBlip longRangeBlip = new SonarBlip(transducerCenter + blipDir * this.Range * 0.9f, Rand.Range(1.9f, 2.1f, Rand.RandSync.Unsynced), Rand.Range(1f, 1.5f, Rand.RandSync.Unsynced), Sonar.BlipType.LongRange)
								{
									Velocity = -invBlipDir * (MathUtils.Round(Rand.Range(8000f, 15000f, Rand.RandSync.Unsynced), 2000f) - Math.Abs(angle * angle * 10000f)),
									Rotation = new float?((float)Math.Atan2((double)(-(double)invBlipDir.Y), (double)invBlipDir.X)),
									Alpha = MathUtils.Pow2((c.Params.DistantSonarRange - dist) / c.Params.DistantSonarRange)
								};
								SonarBlip sonarBlip = longRangeBlip;
								sonarBlip.Size.Y = sonarBlip.Size.Y * 5f;
								this.sonarBlips.Add(longRangeBlip);
							}
						}
					}
				}
				this.longRangeUpdateTimer = 10f;
			}
			if (this.currentMode == Sonar.Mode.Active && this.currentPingIndex != -1)
			{
				return;
			}
			float passivePingRadius = (float)(Timing.TotalTime % 1.0);
			if (passivePingRadius > 0f)
			{
				if (this.activePingsCount == 0)
				{
					this.disruptedDirections.Clear();
				}
				foreach (AITarget t in AITarget.List)
				{
					Character c2 = t.Entity as Character;
					if ((c2 == null || c2.IsUnconscious || !c2.Params.HideInSonar) && t.SoundRange > 0f && !float.IsNaN(t.SoundRange) && !float.IsInfinity(t.SoundRange))
					{
						float sonarSoundRange = t.SoundRange * t.SoundRangeOnSonarMultiplier;
						float distSqr = Vector2.DistanceSquared(t.WorldPosition, transducerCenter);
						if (distSqr <= sonarSoundRange * sonarSoundRange * 2f)
						{
							float dist2 = (float)Math.Sqrt((double)distSqr);
							if (dist2 > this.prevPassivePingRadius * this.Range && dist2 <= passivePingRadius * this.Range && Rand.Int(this.sonarBlips.Count, Rand.RandSync.Unsynced) < 500)
							{
								this.Ping(t.WorldPosition, transducerCenter, sonarSoundRange * this.DisplayScale, 0f, this.DisplayScale, this.range, true, 0.5f, t);
								if (t.IsWithinSector(transducerCenter))
								{
									this.sonarBlips.Add(new SonarBlip(t.WorldPosition, 1f, MathHelper.Clamp(sonarSoundRange / 2000f, 1f, 5f), Sonar.BlipType.Default));
								}
							}
						}
					}
				}
			}
			this.prevPassivePingRadius = passivePingRadius;
		}

		// Token: 0x06005D4D RID: 23885 RVA: 0x003058DC File Offset: 0x00303ADC
		private bool MouseInDirectionalPingRing(Rectangle rect, bool onButton)
		{
			if (!this.useDirectionalPing || this.directionalPingButton == null)
			{
				return false;
			}
			float endRadius = (float)rect.Width / 2f;
			float startRadius = endRadius - this.directionalPingButton[0].size.X * (float)rect.Width / this.screenBackground.size.X;
			Vector2 center = rect.Center.ToVector2();
			float dist = Vector2.DistanceSquared(PlayerInput.MousePosition, center);
			bool retVal = dist >= startRadius * startRadius && dist < endRadius * endRadius;
			if (onButton)
			{
				float pingAngle = MathUtils.VectorToAngle(this.pingDirection);
				float mouseAngle = MathUtils.VectorToAngle(Vector2.Normalize(PlayerInput.MousePosition - center));
				retVal &= (Math.Abs(MathUtils.GetShortestAngle(mouseAngle, pingAngle)) < MathHelper.ToRadians(15f));
			}
			return retVal;
		}

		// Token: 0x06005D4E RID: 23886 RVA: 0x003059B0 File Offset: 0x00303BB0
		private void DrawSonar(SpriteBatch spriteBatch, Rectangle rect)
		{
			this.displayBorderSize = 0.2f;
			this.center = rect.Center.ToVector2();
			this.DisplayRadius = (float)rect.Width / 2f * (1f - this.displayBorderSize);
			this.DisplayScale = this.DisplayRadius / this.range * this.zoom;
			Sprite sprite = this.screenBackground;
			if (sprite != null)
			{
				sprite.Draw(spriteBatch, this.center, 0f, (float)rect.Width / this.screenBackground.size.X, SpriteEffects.None);
			}
			if (this.useDirectionalPing)
			{
				Sprite sprite2 = this.directionalPingBackground;
				if (sprite2 != null)
				{
					sprite2.Draw(spriteBatch, this.center, 0f, (float)rect.Width / this.directionalPingBackground.size.X, SpriteEffects.None);
				}
				if (this.directionalPingButton != null)
				{
					int buttonSprIndex = 0;
					if (this.pingDragDirection != null)
					{
						buttonSprIndex = 2;
					}
					else if (this.MouseInDirectionalPingRing(rect, true))
					{
						buttonSprIndex = 1;
					}
					Sprite sprite3 = this.directionalPingButton[buttonSprIndex];
					if (sprite3 != null)
					{
						sprite3.Draw(spriteBatch, this.center, MathUtils.VectorToAngle(this.pingDirection), (float)rect.Width / this.directionalPingBackground.size.X, SpriteEffects.None);
					}
				}
			}
			if (this.currentPingIndex != -1)
			{
				Sonar.ActivePing activePing = this.activePings[this.currentPingIndex];
				if (activePing.IsDirectional && this.directionalPingCircle != null)
				{
					this.directionalPingCircle.Draw(spriteBatch, this.center, Color.White * (1f - activePing.State), MathUtils.VectorToAngle(activePing.Direction), this.DisplayRadius / this.directionalPingCircle.size.X * activePing.State, SpriteEffects.None, null);
				}
				else
				{
					this.pingCircle.Draw(spriteBatch, this.center, Color.White * (1f - activePing.State), 0f, this.DisplayRadius * 2f / this.pingCircle.size.X * activePing.State, SpriteEffects.None, null);
				}
			}
			float signalStrength = 1f;
			if (this.UseTransducers)
			{
				signalStrength = 0f;
				foreach (Sonar.ConnectedTransducer connectedTransducer in this.connectedTransducers)
				{
					signalStrength = Math.Max(signalStrength, connectedTransducer.SignalStrength);
				}
			}
			Vector2 transducerCenter = this.GetTransducerPos();
			if (this.sonarBlips.Count > 0)
			{
				float blipScale = 0.08f * (float)Math.Sqrt((double)this.zoom) * ((float)rect.Width / 700f);
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, null, null, null, null, null);
				foreach (SonarBlip sonarBlip in this.sonarBlips)
				{
					this.DrawBlip(spriteBatch, sonarBlip, transducerCenter + this.DisplayOffset, this.center, sonarBlip.FadeTimer / 2f * signalStrength, blipScale);
				}
				spriteBatch.End();
				spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, null);
			}
			if (this.item.Submarine != null && !this.DetectSubmarineWalls)
			{
				transducerCenter += this.DisplayOffset;
				this.DrawDockingPorts(spriteBatch, transducerCenter, signalStrength);
				this.DrawOwnSubmarineBorders(spriteBatch, transducerCenter, signalStrength);
			}
			else
			{
				this.DisplayOffset = Vector2.Zero;
			}
			float directionalPingVisibility = (this.useDirectionalPing && this.currentMode == Sonar.Mode.Active) ? 1f : this.showDirectionalIndicatorTimer;
			if (directionalPingVisibility > 0f)
			{
				Vector2 sector = MathUtils.RotatePointAroundTarget(this.pingDirection * this.DisplayRadius, Vector2.Zero, MathHelper.ToRadians(15f), true);
				Vector2 sector2 = MathUtils.RotatePointAroundTarget(this.pingDirection * this.DisplayRadius, Vector2.Zero, MathHelper.ToRadians(-15f), true);
				this.DrawLine(spriteBatch, Vector2.Zero, sector, Color.LightCyan * 0.2f * directionalPingVisibility, 3);
				this.DrawLine(spriteBatch, Vector2.Zero, sector2, Color.LightCyan * 0.2f * directionalPingVisibility, 3);
			}
			if (GameMain.DebugDraw)
			{
				GUI.DrawString(spriteBatch, rect.Location.ToVector2(), this.sonarBlips.Count.ToString(), Color.White, null, 0, null, ForceUpperCase.Inherit);
			}
			Sprite sprite4 = this.screenOverlay;
			if (sprite4 != null)
			{
				sprite4.Draw(spriteBatch, this.center, 0f, (float)rect.Width / this.screenOverlay.size.X, SpriteEffects.None);
			}
			if (signalStrength <= 0.5f)
			{
				this.signalWarningText.Text = TextManager.Get((signalStrength <= 0f) ? "SonarNoSignal" : "SonarSignalWeak");
				this.signalWarningText.Color = ((signalStrength <= 0f) ? this.negativeColor : this.warningColor);
				this.signalWarningText.Visible = true;
				return;
			}
			this.signalWarningText.Visible = false;
			foreach (AITarget aiTarget in AITarget.List)
			{
				if (!aiTarget.InDetectable && !aiTarget.SonarLabel.IsNullOrEmpty() && aiTarget.SoundRange > 0f)
				{
					float sonarSoundRange = aiTarget.SoundRange * aiTarget.SoundRangeOnSonarMultiplier;
					if (Vector2.DistanceSquared(aiTarget.WorldPosition, transducerCenter) < sonarSoundRange * sonarSoundRange)
					{
						this.DrawMarker(spriteBatch, aiTarget.SonarLabel.Value, aiTarget.SonarIconIdentifier, aiTarget, aiTarget.WorldPosition, transducerCenter, this.DisplayScale, this.center, this.DisplayRadius * 0.975f, false);
					}
				}
			}
			if (GameMain.GameSession == null)
			{
				return;
			}
			if (Level.Loaded != null)
			{
				Location startLocation = Level.Loaded.StartLocation;
				LocationType locationType = (startLocation != null) ? startLocation.Type : null;
				if (locationType != null && locationType.ShowSonarMarker)
				{
					this.DrawMarker(spriteBatch, Level.Loaded.StartLocation.DisplayName.Value, ((Level.Loaded.StartOutpost != null) ? "outpost" : "location").ToIdentifier(), "startlocation", Level.Loaded.StartExitPosition, transducerCenter, this.DisplayScale, this.center, this.DisplayRadius, false);
				}
				Level loaded = Level.Loaded;
				if (loaded != null)
				{
					Location endLocation = loaded.EndLocation;
					if (endLocation != null)
					{
						locationType = endLocation.Type;
						if (locationType != null && locationType.ShowSonarMarker && loaded.Type == LevelData.LevelType.LocationConnection)
						{
							this.DrawMarker(spriteBatch, Level.Loaded.EndLocation.DisplayName.Value, ((Level.Loaded.EndOutpost != null) ? "outpost" : "location").ToIdentifier(), "endlocation", Level.Loaded.EndExitPosition, transducerCenter, this.DisplayScale, this.center, this.DisplayRadius, false);
						}
					}
				}
				for (int l = 0; l < Level.Loaded.Caves.Count; l++)
				{
					Level.Cave cave = Level.Loaded.Caves[l];
					if (!cave.MissionsToDisplayOnSonar.None(null))
					{
						this.DrawMarker(spriteBatch, Sonar.caveLabel.Value, "cave".ToIdentifier(), "cave" + l.ToString(), cave.StartPos.ToVector2(), transducerCenter, this.DisplayScale, this.center, this.DisplayRadius, false);
					}
				}
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null)
				{
					GameSession gameSession = GameMain.GameSession;
					if (((gameSession != null) ? gameSession.GameMode : null) is PvPMode && networkMember.ServerSettings.TrackOpponentInPvP)
					{
						Submarine coalitionSub = Submarine.MainSubs[0];
						if (coalitionSub != null)
						{
							Submarine separatistSub = Submarine.MainSubs[1];
							if (separatistSub != null)
							{
								Character player = Character.Controlled;
								if (player != null)
								{
									CharacterTeamType teamID = player.TeamID;
									Submarine submarine;
									if (teamID != CharacterTeamType.Team1)
									{
										if (teamID != CharacterTeamType.Team2)
										{
											submarine = null;
										}
										else
										{
											submarine = coalitionSub;
										}
									}
									else
									{
										submarine = separatistSub;
									}
									Submarine whichSubToDraw = submarine;
									if (whichSubToDraw != null)
									{
										this.DrawOffsetMarker(spriteBatch, Sonar.enemyLabel.Value, Tags.Submarine, Tags.Enemy, whichSubToDraw.WorldPosition, transducerCenter, new Range<float>(Sonar.<DrawSonar>g__MetersToUnits|91_0(150f), Sonar.<DrawSonar>g__MetersToUnits|91_0(1600f)), new Range<float>(Sonar.<DrawSonar>g__MetersToUnits|91_0(100f), Sonar.<DrawSonar>g__MetersToUnits|91_0(400f)), Sonar.<DrawSonar>g__MetersToUnits|91_0(10f));
									}
								}
							}
						}
					}
				}
			}
			int missionIndex = 0;
			foreach (Mission mission in GameMain.GameSession.Missions)
			{
				if (mission.Prefab.ShowSonarLabels)
				{
					int j = 0;
					foreach (ValueTuple<LocalizedString, Vector2> valueTuple in mission.SonarLabels)
					{
						LocalizedString label = valueTuple.Item1;
						Vector2 position = valueTuple.Item2;
						if (!string.IsNullOrEmpty(label.Value))
						{
							this.DrawMarker(spriteBatch, label.Value, mission.SonarIconIdentifier, "mission" + missionIndex.ToString() + ":" + j.ToString(), position, transducerCenter, this.DisplayScale, this.center, this.DisplayRadius * 0.95f, false);
						}
						j++;
					}
					missionIndex++;
				}
			}
			if (this.HasMineralScanner && this.UseMineralScanner && this.CurrentMode == Sonar.Mode.Active && this.MineralClusters != null && (this.item.CurrentHull == null || !this.DetectSubmarineWalls) && this.HasPower)
			{
				foreach (ValueTuple<Vector2, List<Item>> c in this.MineralClusters)
				{
					IEnumerable<Item> unobtainedMinerals = c.Item2.Where(delegate(Item i)
					{
						if (i != null)
						{
							Holdable component = i.GetComponent<Holdable>();
							return component != null && component.Attached;
						}
						return false;
					});
					if (!unobtainedMinerals.None(null) && this.CheckResourceMarkerVisibility(c.Item1, transducerCenter))
					{
						Item k = unobtainedMinerals.FirstOrDefault<Item>();
						if (k != null)
						{
							bool disrupted = false;
							foreach (ValueTuple<Vector2, float> valueTuple2 in this.disruptedDirections)
							{
								Vector2 disruptPos = valueTuple2.Item1;
								float disruptStrength = valueTuple2.Item2;
								float dot = Vector2.Dot(Vector2.Normalize(c.Item1 - transducerCenter), disruptPos);
								if (dot > 1f - disruptStrength)
								{
									disrupted = true;
									break;
								}
							}
							if (!disrupted)
							{
								string name = k.Name;
								Identifier iconIdentifier = "mineral".ToIdentifier();
								string str = "mineralcluster";
								Item item = k;
								this.DrawMarker(spriteBatch, name, iconIdentifier, str + ((item != null) ? item.ToString() : null), c.Item1, transducerCenter, this.DisplayScale, this.center, this.DisplayRadius * 0.95f, true);
							}
						}
					}
				}
			}
			foreach (Submarine sub in Submarine.Loaded)
			{
				if (sub.ShowSonarMarker && !this.connectedSubs.Contains(sub) && !sub.IsAboveLevel)
				{
					if (this.item.Submarine != null || Character.Controlled != null)
					{
						if (sub.TeamID == CharacterTeamType.Team1)
						{
							Submarine submarine2 = this.item.Submarine;
							if (submarine2 != null && submarine2.TeamID == CharacterTeamType.Team2)
							{
								continue;
							}
							Character controlled = Character.Controlled;
							if (controlled != null && controlled.TeamID == CharacterTeamType.Team2)
							{
								continue;
							}
						}
						if (sub.TeamID == CharacterTeamType.Team2)
						{
							Submarine submarine3 = this.item.Submarine;
							if (submarine3 != null && submarine3.TeamID == CharacterTeamType.Team1)
							{
								continue;
							}
							Character controlled2 = Character.Controlled;
							if (controlled2 != null && controlled2.TeamID == CharacterTeamType.Team1)
							{
								continue;
							}
						}
					}
					this.DrawMarker(spriteBatch, sub.Info.DisplayName.Value, (sub.Info.HasTag(SubmarineTag.Shuttle) ? "shuttle" : "submarine").ToIdentifier(), sub, sub.WorldPosition, transducerCenter, this.DisplayScale, this.center, this.DisplayRadius * 0.95f, false);
				}
			}
			if (GameMain.DebugDraw)
			{
				Steering steering = this.item.GetComponent<Steering>();
				if (steering != null)
				{
					steering.DebugDrawHUD(spriteBatch, transducerCenter, this.DisplayScale, this.DisplayRadius, this.center);
				}
			}
		}

		// Token: 0x06005D4F RID: 23887 RVA: 0x0030673C File Offset: 0x0030493C
		private void DrawOwnSubmarineBorders(SpriteBatch spriteBatch, Vector2 transducerCenter, float signalStrength)
		{
			float simScale = this.DisplayScale * 100f;
			foreach (Submarine submarine in Submarine.Loaded)
			{
				if (this.connectedSubs.Contains(submarine) && submarine.HullVertices != null)
				{
					Vector2 offset = ConvertUnits.ToSimUnits(submarine.WorldPosition - transducerCenter);
					for (int i = 0; i < submarine.HullVertices.Count; i++)
					{
						Vector2 start = (submarine.HullVertices[i] + offset) * simScale;
						start.Y = -start.Y;
						Vector2 end = (submarine.HullVertices[(i + 1) % submarine.HullVertices.Count] + offset) * simScale;
						end.Y = -end.Y;
						this.DrawLine(spriteBatch, start, end, Color.LightBlue * signalStrength * 0.5f, 4);
					}
				}
			}
		}

		// Token: 0x06005D50 RID: 23888 RVA: 0x0030686C File Offset: 0x00304A6C
		private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int width)
		{
			bool startOutside = start.LengthSquared() > this.DisplayRadius * this.DisplayRadius;
			bool endOutside = end.LengthSquared() > this.DisplayRadius * this.DisplayRadius;
			if (startOutside && endOutside)
			{
				return;
			}
			if (startOutside)
			{
				Vector2? intersection;
				Vector2? vector;
				if (MathUtils.GetLineCircleIntersections(Vector2.Zero, this.DisplayRadius, end, start, true, out intersection, out vector) == 1)
				{
					this.DrawLineSprite(spriteBatch, this.center + intersection.Value, this.center + end, color, width);
					return;
				}
			}
			else if (endOutside)
			{
				Vector2? vector;
				Vector2? intersection2;
				if (MathUtils.GetLineCircleIntersections(Vector2.Zero, this.DisplayRadius, start, end, true, out intersection2, out vector) == 1)
				{
					this.DrawLineSprite(spriteBatch, this.center + start, this.center + intersection2.Value, color, width);
					return;
				}
			}
			else
			{
				this.DrawLineSprite(spriteBatch, this.center + start, this.center + end, color, width);
			}
		}

		// Token: 0x06005D51 RID: 23889 RVA: 0x00306964 File Offset: 0x00304B64
		private void DrawLineSprite(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int width)
		{
			if (this.lineSprite == null)
			{
				GUI.DrawLine(spriteBatch, start, end, color, 0f, (float)width);
				return;
			}
			Vector2 dir = end - start;
			float angle = (float)Math.Atan2((double)dir.Y, (double)dir.X);
			this.lineSprite.Draw(spriteBatch, start, color, this.lineSprite.Origin, angle, new Vector2(dir.Length() / this.lineSprite.size.X, 1f), SpriteEffects.None, null);
		}

		// Token: 0x06005D52 RID: 23890 RVA: 0x003069F0 File Offset: 0x00304BF0
		private void DrawDockingPorts(SpriteBatch spriteBatch, Vector2 transducerCenter, float signalStrength)
		{
			float scale = this.DisplayScale;
			Steering steering = this.item.GetComponent<Steering>();
			if (steering != null && steering.DockingModeEnabled && steering.ActiveDockingSource != null)
			{
				this.DrawDockingIndicator(spriteBatch, steering, ref transducerCenter);
			}
			foreach (DockingPort dockingPort in DockingPort.List)
			{
				if (!dockingPort.Item.Submarine.IsAboveLevel && !dockingPort.Item.IsHidden && dockingPort.Item.Submarine != null && !dockingPort.Item.Submarine.Info.IsWreck && (dockingPort.Item.Submarine.ShowSonarMarker || dockingPort.Item.Submarine == this.item.Submarine || dockingPort.Item.Submarine.Info.IsOutpost || dockingPort.Item.Submarine.Info.IsBeacon) && (this.item.Submarine == null || this.item.Submarine.IsRespawnShuttle || dockingPort.Item.Submarine.IsRespawnShuttle || dockingPort.Item.Submarine.Info.IsOutpost || dockingPort.Item.Submarine.Info.IsBeacon || dockingPort.Item.Submarine.TeamID == this.item.Submarine.TeamID || dockingPort.Item.Submarine.TeamID == CharacterTeamType.FriendlyNPC))
				{
					Vector2 offset = (dockingPort.Item.WorldPosition - transducerCenter) * scale;
					offset.Y = -offset.Y;
					if (offset.LengthSquared() <= this.DisplayRadius * this.DisplayRadius)
					{
						Vector2 size = dockingPort.Item.Rect.Size.ToVector2() * scale;
						if (dockingPort.IsHorizontal)
						{
							size.X = 0f;
						}
						else
						{
							size.Y = 0f;
						}
						GUI.DrawLine(spriteBatch, this.center + offset - size - Vector2.Normalize(size) * this.zoom, this.center + offset + size + Vector2.Normalize(size) * this.zoom, Color.Black * signalStrength * 0.5f, 0f, (float)((int)(this.zoom * 5f)));
						GUI.DrawLine(spriteBatch, this.center + offset - size, this.center + offset + size, this.positiveColor * signalStrength, 0f, (float)((int)(this.zoom * 2.5f)));
					}
				}
			}
		}

		// Token: 0x06005D53 RID: 23891 RVA: 0x00306D14 File Offset: 0x00304F14
		private void DrawDockingIndicator(SpriteBatch spriteBatch, Steering steering, ref Vector2 transducerCenter)
		{
			float scale = this.DisplayScale;
			((steering.ActiveDockingSource.Item.WorldPosition + steering.DockingTarget.Item.WorldPosition) / 2f).X = steering.DockingTarget.Item.WorldPosition.X;
			Vector2 sourcePortDiff = (steering.ActiveDockingSource.Item.WorldPosition - transducerCenter) * scale;
			Vector2 sourcePortPos = new Vector2(sourcePortDiff.X, -sourcePortDiff.Y);
			Vector2 targetPortDiff = (steering.DockingTarget.Item.WorldPosition - transducerCenter) * scale;
			Vector2 targetPortPos = new Vector2(targetPortDiff.X, -targetPortDiff.Y);
			Vector2 diff = steering.DockingTarget.Item.WorldPosition - steering.ActiveDockingSource.Item.WorldPosition;
			float dist = diff.Length();
			bool readyToDock = Math.Abs(diff.X) < steering.DockingTarget.DistanceTolerance.X && Math.Abs(diff.Y) < steering.DockingTarget.DistanceTolerance.Y;
			Vector2 dockingDir = sourcePortPos - targetPortPos;
			Vector2 normalizedDockingDir = Vector2.Normalize(dockingDir);
			if (!this.dynamicDockingIndicator)
			{
				if (steering.ActiveDockingSource.IsHorizontal)
				{
					normalizedDockingDir = new Vector2((float)Math.Sign(normalizedDockingDir.X), 0f);
				}
				else
				{
					normalizedDockingDir = new Vector2(0f, (float)Math.Sign(normalizedDockingDir.Y));
				}
			}
			Color staticLineColor = Color.White * 0.2f;
			float sector = MathHelper.ToRadians(MathHelper.Lerp(10f, 45f, MathHelper.Clamp(dist / steering.DockingAssistThreshold, 0f, 1f)));
			float sectorLength = this.DisplayRadius;
			float midLength = (float)(Math.Cos((double)sector) * (double)sectorLength);
			Vector2 midNormal = new Vector2(-normalizedDockingDir.Y, normalizedDockingDir.X);
			this.DrawLine(spriteBatch, targetPortPos, targetPortPos + normalizedDockingDir * midLength, readyToDock ? this.positiveColor : staticLineColor, 2);
			this.DrawLine(spriteBatch, targetPortPos, targetPortPos + MathUtils.RotatePoint(normalizedDockingDir, sector) * sectorLength, staticLineColor, 2);
			this.DrawLine(spriteBatch, targetPortPos, targetPortPos + MathUtils.RotatePoint(normalizedDockingDir, -sector) * sectorLength, staticLineColor, 2);
			for (float z = 0f; z < 1f; z += 0.1f * this.zoom)
			{
				Vector2 linePos = targetPortPos + normalizedDockingDir * midLength * z;
				this.DrawLine(spriteBatch, linePos + midNormal * 3f, linePos - midNormal * 3f, staticLineColor, 3);
			}
			if (readyToDock)
			{
				Color indicatorColor = this.positiveColor * 0.8f;
				float indicatorSize = (float)Math.Sin((double)((float)Timing.TotalTime * 5f)) * this.DisplayRadius * 0.75f;
				Vector2 midPoint = (sourcePortPos + targetPortPos) / 2f;
				this.DrawLine(spriteBatch, midPoint + Vector2.UnitY * indicatorSize, midPoint - Vector2.UnitY * indicatorSize, indicatorColor, 3);
				this.DrawLine(spriteBatch, midPoint + Vector2.UnitX * indicatorSize, midPoint - Vector2.UnitX * indicatorSize, indicatorColor, 3);
				return;
			}
			float indicatorSector = sector * 0.75f;
			float indicatorSectorLength = (float)((double)midLength / Math.Cos((double)indicatorSector));
			Color indicatorColor2 = ((Math.Abs(diff.X) < steering.ActiveDockingSource.DistanceTolerance.X && Math.Abs(diff.Y) < steering.ActiveDockingSource.DistanceTolerance.Y) || Vector2.Dot(normalizedDockingDir, MathUtils.RotatePoint(normalizedDockingDir, indicatorSector)) < Vector2.Dot(normalizedDockingDir, Vector2.Normalize(dockingDir))) ? this.positiveColor : this.negativeColor;
			indicatorColor2 *= 0.8f;
			this.DrawLine(spriteBatch, targetPortPos, targetPortPos + MathUtils.RotatePoint(normalizedDockingDir, indicatorSector) * indicatorSectorLength, indicatorColor2, 3);
			this.DrawLine(spriteBatch, targetPortPos, targetPortPos + MathUtils.RotatePoint(normalizedDockingDir, -indicatorSector) * indicatorSectorLength, indicatorColor2, 3);
		}

		// Token: 0x06005D54 RID: 23892 RVA: 0x0030718C File Offset: 0x0030538C
		private void UpdateDisruptions(Vector2 pingSource, float worldPingRadius)
		{
			Sonar.<>c__DisplayClass97_0 CS$<>8__locals1;
			CS$<>8__locals1.pingSource = pingSource;
			CS$<>8__locals1.<>4__this = this;
			float worldPingRadiusSqr = worldPingRadius * worldPingRadius;
			this.disruptedDirections.Clear();
			for (int pingIndex = 0; pingIndex < this.activePingsCount; pingIndex++)
			{
				foreach (LevelObject levelObject in this.nearbyObjects)
				{
					LevelObjectPrefab activePrefab = levelObject.ActivePrefab;
					if (activePrefab == null || activePrefab.SonarDisruption > 0f)
					{
						float disruptionStrength = levelObject.ActivePrefab.SonarDisruption;
						Vector2 disruptionPos = new Vector2(levelObject.Position.X, levelObject.Position.Y);
						float disruptionDist = Vector2.Distance(CS$<>8__locals1.pingSource, disruptionPos);
						this.disruptedDirections.Add(new ValueTuple<Vector2, float>((disruptionPos - CS$<>8__locals1.pingSource) / disruptionDist, disruptionStrength));
						this.<UpdateDisruptions>g__CreateBlipsForDisruption|97_0(disruptionPos, disruptionStrength, ref CS$<>8__locals1);
					}
				}
				foreach (AITarget aiTarget in AITarget.List)
				{
					Character c = aiTarget.Entity as Character;
					float disruption = (c != null && !c.IsUnconscious) ? c.Params.SonarDisruption : aiTarget.SonarDisruption;
					if (disruption > 0f && !aiTarget.InDetectable)
					{
						float distSqr = Vector2.DistanceSquared(aiTarget.WorldPosition, CS$<>8__locals1.pingSource);
						if (distSqr <= worldPingRadiusSqr)
						{
							float disruptionDist2 = (float)Math.Sqrt((double)distSqr);
							this.disruptedDirections.Add(new ValueTuple<Vector2, float>((aiTarget.WorldPosition - CS$<>8__locals1.pingSource) / disruptionDist2, aiTarget.SonarDisruption));
							this.<UpdateDisruptions>g__CreateBlipsForDisruption|97_0(aiTarget.WorldPosition, disruption, ref CS$<>8__locals1);
						}
					}
				}
			}
		}

		// Token: 0x06005D55 RID: 23893 RVA: 0x00307390 File Offset: 0x00305590
		public void RegisterExplosion(Explosion explosion, Vector2 worldPosition)
		{
			Character controlled = Character.Controlled;
			if (((controlled != null) ? controlled.SelectedItem : null) == null)
			{
				return;
			}
			if (Character.Controlled.SelectedItem != base.Item && !Character.Controlled.SelectedItem.linkedTo.Contains(base.Item))
			{
				return;
			}
			if (explosion.Attack.StructureDamage <= 0f && explosion.Attack.ItemDamage <= 0f && explosion.EmpStrength <= 0f)
			{
				return;
			}
			Vector2 transducerCenter = this.GetTransducerPos();
			if (Vector2.DistanceSquared(worldPosition, transducerCenter) > this.range * this.range)
			{
				return;
			}
			int blipCount = MathHelper.Clamp((int)(explosion.Attack.Range / 100f), 0, 50);
			for (int i = 0; i < blipCount; i++)
			{
				this.sonarBlips.Add(new SonarBlip(worldPosition + Rand.Vector(Rand.Range(0f, explosion.Attack.Range, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced), 1f, Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced), Sonar.BlipType.Disruption));
			}
			if (explosion.EmpStrength > 0f)
			{
				int empBlipCount = MathHelper.Clamp((int)((float)blipCount * explosion.EmpStrength), 10, 50);
				for (int j = 0; j < empBlipCount; j++)
				{
					Vector2 dir = Rand.Vector(1f, Rand.RandSync.Unsynced);
					SonarBlip longRangeBlip = new SonarBlip(worldPosition, Rand.Range(1.9f, 2.1f, Rand.RandSync.Unsynced), Rand.Range(1f, 1.5f, Rand.RandSync.Unsynced), Sonar.BlipType.LongRange)
					{
						Velocity = dir * MathUtils.Round(Rand.Range(4000f, 6000f, Rand.RandSync.Unsynced), 1000f),
						Rotation = new float?((float)Math.Atan2((double)(-(double)dir.Y), (double)dir.X))
					};
					SonarBlip sonarBlip = longRangeBlip;
					sonarBlip.Size.Y = sonarBlip.Size.Y * 4f;
					this.sonarBlips.Add(longRangeBlip);
				}
			}
		}

		// Token: 0x06005D56 RID: 23894 RVA: 0x0030757C File Offset: 0x0030577C
		private void Ping(Vector2 pingSource, Vector2 transducerPos, float pingRadius, float prevPingRadius, float displayScale, float range, bool passive, float pingStrength = 1f, AITarget needsToBeInSector = null)
		{
			Sonar.<>c__DisplayClass99_0 CS$<>8__locals1;
			CS$<>8__locals1.passive = passive;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.transducerPos = transducerPos;
			CS$<>8__locals1.needsToBeInSector = needsToBeInSector;
			float prevPingRadiusSqr = prevPingRadius * prevPingRadius;
			float pingRadiusSqr = pingRadius * pingRadius;
			if (this.item.CurrentHull != null && this.DetectSubmarineWalls)
			{
				this.CreateBlipsForLine(new Vector2((float)this.item.CurrentHull.WorldRect.X, (float)this.item.CurrentHull.WorldRect.Y), new Vector2((float)this.item.CurrentHull.WorldRect.Right, (float)this.item.CurrentHull.WorldRect.Y), pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, 50f, 5f, range, 2f, CS$<>8__locals1.passive, Sonar.BlipType.Default, CS$<>8__locals1.needsToBeInSector);
				this.CreateBlipsForLine(new Vector2((float)this.item.CurrentHull.WorldRect.X, (float)(this.item.CurrentHull.WorldRect.Y - this.item.CurrentHull.Rect.Height)), new Vector2((float)this.item.CurrentHull.WorldRect.Right, (float)(this.item.CurrentHull.WorldRect.Y - this.item.CurrentHull.Rect.Height)), pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, 50f, 5f, range, 2f, CS$<>8__locals1.passive, Sonar.BlipType.Default, CS$<>8__locals1.needsToBeInSector);
				this.CreateBlipsForLine(new Vector2((float)this.item.CurrentHull.WorldRect.X, (float)this.item.CurrentHull.WorldRect.Y), new Vector2((float)this.item.CurrentHull.WorldRect.X, (float)(this.item.CurrentHull.WorldRect.Y - this.item.CurrentHull.Rect.Height)), pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, 50f, 5f, range, 2f, CS$<>8__locals1.passive, Sonar.BlipType.Default, CS$<>8__locals1.needsToBeInSector);
				this.CreateBlipsForLine(new Vector2((float)this.item.CurrentHull.WorldRect.Right, (float)this.item.CurrentHull.WorldRect.Y), new Vector2((float)this.item.CurrentHull.WorldRect.Right, (float)(this.item.CurrentHull.WorldRect.Y - this.item.CurrentHull.Rect.Height)), pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, 50f, 5f, range, 2f, CS$<>8__locals1.passive, Sonar.BlipType.Default, CS$<>8__locals1.needsToBeInSector);
				return;
			}
			foreach (Submarine submarine in Submarine.Loaded)
			{
				if (submarine.HullVertices != null && (this.DetectSubmarineWalls || !this.connectedSubs.Contains(submarine)))
				{
					Rectangle worldBorders = submarine.GetDockedBorders(true);
					worldBorders.Location += submarine.WorldPosition.ToPoint();
					if (!Submarine.RectContains(worldBorders, pingSource, false))
					{
						OutpostGenerationParams outpostGenerationParams = submarine.Info.OutpostGenerationParams;
						if (outpostGenerationParams == null || !outpostGenerationParams.AlwaysShowStructuresOnSonar)
						{
							for (int i = 0; i < submarine.HullVertices.Count; i++)
							{
								Vector2 start = ConvertUnits.ToDisplayUnits(submarine.HullVertices[i]);
								Vector2 end = ConvertUnits.ToDisplayUnits(submarine.HullVertices[(i + 1) % submarine.HullVertices.Count]);
								if (this.item.Submarine == submarine)
								{
									start += Rand.Vector(500f, Rand.RandSync.Unsynced);
									end += Rand.Vector(500f, Rand.RandSync.Unsynced);
								}
								this.CreateBlipsForLine(start + submarine.WorldPosition, end + submarine.WorldPosition, pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, 200f, 2f, range, 1f, CS$<>8__locals1.passive, Sonar.BlipType.Default, CS$<>8__locals1.needsToBeInSector);
							}
							continue;
						}
					}
					this.CreateBlipsForSubmarineWalls(submarine, pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, range, CS$<>8__locals1.passive);
				}
			}
			if (Level.Loaded != null && (this.item.CurrentHull == null || !this.DetectSubmarineWalls))
			{
				if ((float)Level.Loaded.Size.Y - pingSource.Y < range)
				{
					this.CreateBlipsForLine(new Vector2(pingSource.X - range, (float)Level.Loaded.Size.Y), new Vector2(pingSource.X + range, (float)Level.Loaded.Size.Y), pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, 250f, 150f, range, pingStrength, CS$<>8__locals1.passive, Sonar.BlipType.Default, CS$<>8__locals1.needsToBeInSector);
				}
				if (pingSource.Y - (float)Level.Loaded.BottomPos < range)
				{
					this.CreateBlipsForLine(new Vector2(pingSource.X - range, (float)Level.Loaded.BottomPos), new Vector2(pingSource.X + range, (float)Level.Loaded.BottomPos), pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, 250f, 150f, range, pingStrength, CS$<>8__locals1.passive, Sonar.BlipType.Default, CS$<>8__locals1.needsToBeInSector);
				}
				List<VoronoiCell> cells = Level.Loaded.GetCells(pingSource, 7);
				foreach (VoronoiCell cell in cells)
				{
					foreach (GraphEdge edge in cell.Edges)
					{
						if (edge.IsSolid)
						{
							float cellDot = Vector2.Dot(edge.Center + cell.Translation - pingSource, edge.GetNormal(cell));
							if (cellDot <= 0f)
							{
								float facingDot = Vector2.Dot(Vector2.Normalize(edge.Point1 - edge.Point2), Vector2.Normalize(cell.Center - pingSource));
								this.CreateBlipsForLine(edge.Point1 + cell.Translation, edge.Point2 + cell.Translation, pingSource, CS$<>8__locals1.transducerPos, pingRadius, prevPingRadius, 350f, 3f * (Math.Abs(facingDot) + 1f), range, pingStrength, CS$<>8__locals1.passive, cell.IsDestructible ? Sonar.BlipType.Destructible : Sonar.BlipType.Default, CS$<>8__locals1.needsToBeInSector);
							}
						}
					}
				}
			}
			foreach (Item item in Item.SonarVisibleItems)
			{
				if (item.CurrentHull == null)
				{
					if (item.ParentInventory == null)
					{
						goto IL_76E;
					}
					Holdable component = item.GetComponent<Holdable>();
					if (component != null && component.IsActive)
					{
						goto IL_76E;
					}
					Item container = item.Container;
					ItemContainer itemContainer = (container != null) ? container.GetComponent<ItemContainer>() : null;
					bool flag = itemContainer != null && !itemContainer.HideItems;
					IL_76F:
					bool isItemVisible = flag;
					if (!isItemVisible)
					{
						continue;
					}
					float pointDist = ((item.WorldPosition - pingSource) * displayScale).LengthSquared();
					if (pointDist <= prevPingRadiusSqr || pointDist >= pingRadiusSqr)
					{
						continue;
					}
					SonarBlip blip = new SonarBlip(item.WorldPosition + Rand.Vector(item.Prefab.SonarSize, Rand.RandSync.Unsynced), MathHelper.Clamp(item.Prefab.SonarSize, 0.1f, pingStrength), MathHelper.Clamp(item.Prefab.SonarSize * 0.1f, 0.1f, 10f), Sonar.BlipType.Default);
					if (this.<Ping>g__IsVisible|99_0(blip, ref CS$<>8__locals1))
					{
						this.sonarBlips.Add(blip);
						continue;
					}
					continue;
					IL_76E:
					flag = true;
					goto IL_76F;
				}
			}
			foreach (Character c in Character.CharacterList)
			{
				if (c.AnimController.CurrentHull == null && c.Enabled && (c.IsUnconscious || !c.Params.HideInSonar) && !c.InDetectable && (!this.DetectSubmarineWalls || c.AnimController.CurrentHull != null || this.item.CurrentHull == null))
				{
					if (c.AnimController.SimplePhysicsEnabled)
					{
						float pointDist2 = ((c.WorldPosition - pingSource) * displayScale).LengthSquared();
						if (pointDist2 <= this.DisplayRadius * this.DisplayRadius && pointDist2 > prevPingRadiusSqr && pointDist2 < pingRadiusSqr)
						{
							SonarBlip blip2 = new SonarBlip(c.WorldPosition, MathHelper.Clamp(c.Mass, 0.1f, pingStrength), MathHelper.Clamp(c.Mass * 0.03f, 0.1f, 2f), Sonar.BlipType.Default);
							if (this.<Ping>g__IsVisible|99_0(blip2, ref CS$<>8__locals1))
							{
								this.sonarBlips.Add(blip2);
								HintManager.OnSonarSpottedCharacter(base.Item, c);
							}
						}
					}
					else
					{
						foreach (Limb limb in c.AnimController.Limbs)
						{
							if (limb.body.Enabled)
							{
								float pointDist3 = ((limb.WorldPosition - pingSource) * displayScale).LengthSquared();
								if (!(limb.SimPosition == Vector2.Zero) && pointDist3 <= this.DisplayRadius * this.DisplayRadius && pointDist3 > prevPingRadiusSqr && pointDist3 < pingRadiusSqr)
								{
									SonarBlip blip3 = new SonarBlip(limb.WorldPosition + Rand.Vector(limb.Mass / 10f, Rand.RandSync.Unsynced), MathHelper.Clamp(limb.Mass, 0.1f, pingStrength), MathHelper.Clamp(limb.Mass * 0.1f, 0.1f, 2f), Sonar.BlipType.Default);
									if (this.<Ping>g__IsVisible|99_0(blip3, ref CS$<>8__locals1))
									{
										this.sonarBlips.Add(blip3);
										HintManager.OnSonarSpottedCharacter(base.Item, c);
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06005D57 RID: 23895 RVA: 0x003080A8 File Offset: 0x003062A8
		private void CreateBlipsForLine(Vector2 point1, Vector2 point2, Vector2 pingSource, Vector2 transducerPos, float pingRadius, float prevPingRadius, float lineStep, float zStep, float range, float pingStrength, bool passive, Sonar.BlipType blipType = Sonar.BlipType.Default, AITarget needsToBeInSector = null)
		{
			lineStep /= this.zoom;
			zStep /= this.zoom;
			range *= this.DisplayScale;
			float length = (point1 - point2).Length();
			Vector2 lineDir = (point2 - point1) / length;
			for (float x = 0f; x < length; x += lineStep * Rand.Range(0.8f, 1.2f, Rand.RandSync.Unsynced))
			{
				if (Rand.Int(this.sonarBlips.Count, Rand.RandSync.Unsynced) <= 500)
				{
					Vector2 point3 = point1 + lineDir * x;
					Vector2 transducerDiff = point3 - transducerPos;
					if ((transducerDiff * this.DisplayScale / this.zoom).LengthSquared() <= this.DisplayRadius * this.DisplayRadius)
					{
						Vector2 pointDiff = point3 - pingSource;
						float displayPointDistSqr = (pointDiff * this.DisplayScale / this.zoom).LengthSquared();
						if (displayPointDistSqr >= prevPingRadius * prevPingRadius && displayPointDistSqr <= pingRadius * pingRadius)
						{
							float transducerDist = transducerDiff.Length();
							Vector2 pingDirection = transducerDiff / transducerDist;
							bool disrupted = false;
							foreach (ValueTuple<Vector2, float> valueTuple in this.disruptedDirections)
							{
								Vector2 disruptPos = valueTuple.Item1;
								float disruptStrength = valueTuple.Item2;
								float dot = Vector2.Dot(pingDirection, disruptPos);
								if (dot > 1f - disruptStrength)
								{
									disrupted = true;
									break;
								}
							}
							if (!disrupted)
							{
								float displayPointDist = (float)Math.Sqrt((double)displayPointDistSqr);
								float alpha = pingStrength * Rand.Range(1.5f, 2f, Rand.RandSync.Unsynced);
								for (float z = 0f; z < this.DisplayRadius - transducerDist * this.DisplayScale; z += zStep)
								{
									Vector2 pos = point3 + Rand.Vector(150f / this.zoom, Rand.RandSync.Unsynced) + pingDirection * z / this.DisplayScale;
									float fadeTimer = alpha * (1f - displayPointDist / range);
									if (needsToBeInSector == null || needsToBeInSector.IsWithinSector(pos))
									{
										SonarBlip blip = new SonarBlip(pos, fadeTimer, 1f + (displayPointDist + z) / this.DisplayRadius, blipType);
										if (passive || this.CheckBlipVisibility(blip, transducerPos))
										{
											int minDist = (int)(200f / this.zoom);
											this.sonarBlips.RemoveAll((SonarBlip b) => b.FadeTimer < fadeTimer && Math.Abs(pos.X - b.Position.X) < (float)minDist && Math.Abs(pos.Y - b.Position.Y) < (float)minDist);
											this.sonarBlips.Add(blip);
											zStep += 0.5f / this.zoom;
											if (z == 0f)
											{
												alpha = Math.Min(alpha - 0.5f, 1.5f);
											}
											else
											{
												alpha -= 0.1f;
											}
											if (alpha < 0f)
											{
												break;
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		// Token: 0x06005D58 RID: 23896 RVA: 0x003083C0 File Offset: 0x003065C0
		private void CreateBlipsForSubmarineWalls(Submarine sub, Vector2 pingSource, Vector2 transducerPos, float pingRadius, float prevPingRadius, float range, bool passive)
		{
			Sonar.<>c__DisplayClass101_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.pingSource = pingSource;
			CS$<>8__locals1.transducerPos = transducerPos;
			CS$<>8__locals1.pingRadius = pingRadius;
			CS$<>8__locals1.prevPingRadius = prevPingRadius;
			CS$<>8__locals1.range = range;
			CS$<>8__locals1.passive = passive;
			foreach (Structure structure in Structure.WallList)
			{
				if (structure.Submarine == sub)
				{
					this.<CreateBlipsForSubmarineWalls>g__CreateBlips|101_0(structure.IsHorizontal, structure.WorldPosition, structure.WorldRect, -structure.RotationWithFlipping, Sonar.BlipType.Default, ref CS$<>8__locals1);
				}
			}
			foreach (Door door in Door.DoorList)
			{
				if (door.Item.Submarine == sub && !door.IsOpen)
				{
					this.<CreateBlipsForSubmarineWalls>g__CreateBlips|101_0(door.IsHorizontal, door.Item.WorldPosition, door.Item.WorldRect, 0f, Sonar.BlipType.Door, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x06005D59 RID: 23897 RVA: 0x003084EC File Offset: 0x003066EC
		private bool CheckBlipVisibility(SonarBlip blip, Vector2 transducerPos)
		{
			Vector2 pos = (blip.Position - transducerPos) * this.DisplayScale;
			pos.Y = -pos.Y;
			float posDistSqr = pos.LengthSquared();
			if (posDistSqr > this.DisplayRadius * this.DisplayRadius)
			{
				blip.FadeTimer = 0f;
				return false;
			}
			Vector2 dir = pos / (float)Math.Sqrt((double)posDistSqr);
			if (this.currentPingIndex != -1 && this.activePings[this.currentPingIndex].IsDirectional && Vector2.Dot(this.activePings[this.currentPingIndex].Direction, dir) < Sonar.DirectionalPingDotProduct)
			{
				blip.FadeTimer = 0f;
				return false;
			}
			return true;
		}

		// Token: 0x06005D5A RID: 23898 RVA: 0x003085A0 File Offset: 0x003067A0
		private bool CheckResourceMarkerVisibility(Vector2 resourcePos, Vector2 transducerPos)
		{
			float distSquared = Vector2.DistanceSquared(transducerPos, resourcePos);
			if (distSquared > this.Range * this.Range)
			{
				return false;
			}
			if (this.currentPingIndex != -1 && this.activePings[this.currentPingIndex].IsDirectional)
			{
				Vector2 pos = (resourcePos - transducerPos) * this.DisplayScale;
				pos.Y = -pos.Y;
				float length = pos.Length();
				Vector2 dir = pos / length;
				if (Vector2.Dot(this.activePings[this.currentPingIndex].Direction, dir) < Sonar.DirectionalPingDotProduct)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x06005D5B RID: 23899 RVA: 0x00308638 File Offset: 0x00306838
		private void DrawBlip(SpriteBatch spriteBatch, SonarBlip blip, Vector2 transducerPos, Vector2 center, float strength, float blipScale)
		{
			strength = MathHelper.Clamp(strength, 0f, 1f);
			float distort = 1f - this.item.Condition / this.item.MaxCondition;
			Vector2 pos = (blip.Position - transducerPos) * this.DisplayScale;
			pos.Y = -pos.Y;
			if (Rand.Range(0.5f, 2f, Rand.RandSync.Unsynced) < distort)
			{
				pos.X = -pos.X;
			}
			if (Rand.Range(0.5f, 2f, Rand.RandSync.Unsynced) < distort)
			{
				pos.Y = -pos.Y;
			}
			float posDistSqr = pos.LengthSquared();
			if (posDistSqr > this.DisplayRadius * this.DisplayRadius)
			{
				blip.FadeTimer = 0f;
				return;
			}
			if (this.sonarBlip == null)
			{
				GUI.DrawRectangle(spriteBatch, center + pos, Vector2.One * 4f, Color.Magenta, true, 0f, 1f);
				return;
			}
			Vector2 dir = pos / (float)Math.Sqrt((double)posDistSqr);
			Vector2 normal = new Vector2(dir.Y, -dir.X);
			float scale = (strength + 3f) * blip.Scale * blipScale;
			Color color = ToolBox.GradientLerp(strength, Sonar.blipColorGradient[blip.BlipType]);
			this.sonarBlip.Draw(spriteBatch, center + pos, color * blip.Alpha, this.sonarBlip.Origin, blip.Rotation ?? MathUtils.VectorToAngle(pos), blip.Size * scale * 0.5f, SpriteEffects.None, new float?(0f));
			pos += Rand.Range(0f, 1f, Rand.RandSync.Unsynced) * dir + Rand.Range(-scale, scale, Rand.RandSync.Unsynced) * normal;
			this.sonarBlip.Draw(spriteBatch, center + pos, color * 0.5f * blip.Alpha, this.sonarBlip.Origin, 0f, scale, SpriteEffects.None, new float?(0f));
		}

		// Token: 0x06005D5C RID: 23900 RVA: 0x00308874 File Offset: 0x00306A74
		private void DrawOffsetMarker(SpriteBatch spriteBatch, string label, Identifier iconIdentifier, Identifier targetIdentifier, Vector2 worldPosition, Vector2 transducerPosition, Range<float> distanceThresholds, Range<float> offset, float minOffset)
		{
			Sonar.<>c__DisplayClass106_0 CS$<>8__locals1;
			CS$<>8__locals1.worldPosition = worldPosition;
			CS$<>8__locals1.transducerPosition = transducerPosition;
			CS$<>8__locals1.offset = offset;
			CS$<>8__locals1.distanceThresholds = distanceThresholds;
			CS$<>8__locals1.minOffset = minOffset;
			CachedLocation cachedLocation;
			Vector2 pos;
			if (!this.cachedLocations.TryGetValue(targetIdentifier, out cachedLocation))
			{
				cachedLocation = Sonar.<DrawOffsetMarker>g__CreateCachedLocation|106_0(ref CS$<>8__locals1);
				this.cachedLocations.Add(targetIdentifier, cachedLocation);
				pos = cachedLocation.Location;
			}
			else
			{
				if (Timing.TotalTime > cachedLocation.RecalculationTime)
				{
					cachedLocation = Sonar.<DrawOffsetMarker>g__CreateCachedLocation|106_0(ref CS$<>8__locals1);
					this.cachedLocations[targetIdentifier] = cachedLocation;
				}
				pos = cachedLocation.Location;
			}
			this.DrawMarker(spriteBatch, label, iconIdentifier, targetIdentifier, pos, CS$<>8__locals1.transducerPosition, this.DisplayScale, this.center, this.DisplayRadius, false);
		}

		// Token: 0x06005D5D RID: 23901 RVA: 0x00308938 File Offset: 0x00306B38
		private void DrawMarker(SpriteBatch spriteBatch, string label, Identifier iconIdentifier, object targetIdentifier, Vector2 worldPosition, Vector2 transducerPosition, float scale, Vector2 center, float radius, bool onlyShowTextOnMouseOver = false)
		{
			Sonar.<>c__DisplayClass107_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.transducerPosition = transducerPosition;
			CS$<>8__locals1.worldPosition = worldPosition;
			CS$<>8__locals1.targetIdentifier = targetIdentifier;
			CS$<>8__locals1.linearDist = Vector2.Distance(CS$<>8__locals1.worldPosition, CS$<>8__locals1.transducerPosition);
			CS$<>8__locals1.dist = CS$<>8__locals1.linearDist;
			if (CS$<>8__locals1.linearDist > this.Range)
			{
				CachedDistance cachedDistance;
				if (this.markerDistances.TryGetValue(CS$<>8__locals1.targetIdentifier, out cachedDistance))
				{
					if (cachedDistance.ShouldUpdateDistance(CS$<>8__locals1.transducerPosition, CS$<>8__locals1.worldPosition, 500f))
					{
						this.markerDistances.Remove(CS$<>8__locals1.targetIdentifier);
						this.<DrawMarker>g__CalculateDistance|107_0(ref CS$<>8__locals1);
					}
					else
					{
						CS$<>8__locals1.dist = Math.Max(cachedDistance.Distance, CS$<>8__locals1.linearDist);
					}
				}
				else
				{
					this.<DrawMarker>g__CalculateDistance|107_0(ref CS$<>8__locals1);
				}
			}
			Vector2 position = CS$<>8__locals1.worldPosition - CS$<>8__locals1.transducerPosition;
			position *= scale;
			position.Y = -position.Y;
			float textAlpha = MathHelper.Clamp(1.5f - CS$<>8__locals1.dist / 50000f, 0.5f, 1f);
			Vector2 dir = Vector2.Normalize(position);
			Vector2 markerPos = (CS$<>8__locals1.linearDist * scale > radius) ? (dir * radius) : position;
			markerPos += center;
			markerPos.X = (float)((int)markerPos.X);
			markerPos.Y = (float)((int)markerPos.Y);
			float alpha = 1f;
			if (!onlyShowTextOnMouseOver)
			{
				if (CS$<>8__locals1.linearDist * scale < radius)
				{
					float normalizedDist = CS$<>8__locals1.linearDist * scale / radius;
					alpha = Math.Max(normalizedDist - 0.4f, 0f);
					float mouseDist = Vector2.Distance(PlayerInput.MousePosition, markerPos);
					float hoverThreshold = 150f;
					if (mouseDist < hoverThreshold)
					{
						alpha += (hoverThreshold - mouseDist) / hoverThreshold;
					}
				}
			}
			else
			{
				float mouseDist2 = Vector2.Distance(PlayerInput.MousePosition, markerPos);
				if (mouseDist2 > 5f)
				{
					alpha = 0f;
				}
			}
			Tuple<Sprite, Color> iconInfo;
			if (iconIdentifier == null || !this.targetIcons.TryGetValue(iconIdentifier, out iconInfo) || iconInfo.Item1 == null)
			{
				GUI.DrawRectangle(spriteBatch, new Rectangle((int)markerPos.X - 3, (int)markerPos.Y - 3, 6, 6), this.markerColor, false, 0f, 2f);
			}
			else
			{
				iconInfo.Item1.Draw(spriteBatch, markerPos, iconInfo.Item2, 0f, 1f, SpriteEffects.None, null);
			}
			if (alpha <= 0f)
			{
				return;
			}
			string wrappedLabel = ToolBox.WrapText(label, 150f, GUIStyle.SmallFont.Value, 1f);
			wrappedLabel = wrappedLabel + "\n" + ((int)(CS$<>8__locals1.dist * Physics.DisplayToRealWorldRatio)).ToString() + " m";
			Vector2 labelPos = markerPos;
			Vector2 textSize = GUIStyle.SmallFont.MeasureString(wrappedLabel, false);
			if (base.GuiFrame != null && (dir.X < 0f || labelPos.X + textSize.X + 10f > (float)base.GuiFrame.Rect.X) && labelPos.X - textSize.X > 0f)
			{
				labelPos.X -= textSize.X + 10f;
			}
			GUI.DrawString(spriteBatch, new Vector2(labelPos.X + 10f, labelPos.Y), wrappedLabel, Color.LightBlue * textAlpha * alpha, new Color?(Color.Black * textAlpha * 0.8f * alpha), 2, GUIStyle.SmallFont, ForceUpperCase.Inherit);
		}

		// Token: 0x06005D5E RID: 23902 RVA: 0x00308CD4 File Offset: 0x00306ED4
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.currentMode == Sonar.Mode.Active);
			if (this.currentMode == Sonar.Mode.Active)
			{
				msg.WriteRangedSingle(this.zoom, 1f, 4f, 8);
				msg.WriteBoolean(this.useDirectionalPing);
				if (this.useDirectionalPing)
				{
					float pingAngle = MathUtils.WrapAngleTwoPi(MathUtils.VectorToAngle(this.pingDirection));
					msg.WriteRangedSingle(MathUtils.InverseLerp(0f, 6.2831855f, pingAngle), 0f, 1f, 8);
				}
				msg.WriteBoolean(this.UseMineralScanner);
			}
		}

		// Token: 0x06005D5F RID: 23903 RVA: 0x00308D64 File Offset: 0x00306F64
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			int msgStartPos = msg.BitPosition;
			bool isActive = msg.ReadBoolean();
			float zoomT = 1f;
			bool directionalPing = this.useDirectionalPing;
			float directionT = 0f;
			bool mineralScanner = this.UseMineralScanner;
			if (isActive)
			{
				zoomT = msg.ReadRangedSingle(0f, 1f, 8);
				directionalPing = msg.ReadBoolean();
				if (directionalPing)
				{
					directionT = msg.ReadRangedSingle(0f, 1f, 8);
				}
				mineralScanner = msg.ReadBoolean();
			}
			if (this.correctionTimer > 0f)
			{
				int msgLength = msg.BitPosition - msgStartPos;
				msg.BitPosition = msgStartPos;
				base.StartDelayedCorrection(msg.ExtractBits(msgLength), sendingTime, false);
				return;
			}
			this.CurrentMode = (isActive ? Sonar.Mode.Active : Sonar.Mode.Passive);
			if (isActive)
			{
				this.zoomSlider.BarScroll = zoomT;
				this.zoom = MathHelper.Lerp(1f, 4f, zoomT);
				if (directionalPing)
				{
					float pingAngle = MathHelper.Lerp(0f, 6.2831855f, directionT);
					this.pingDirection = new Vector2((float)Math.Cos((double)pingAngle), (float)Math.Sin((double)pingAngle));
				}
				this.useDirectionalPing = (this.directionalModeSwitch.Selected = directionalPing);
				this.UseMineralScanner = mineralScanner;
				if (this.mineralScannerSwitch != null)
				{
					this.mineralScannerSwitch.Selected = mineralScanner;
				}
			}
		}

		// Token: 0x06005D60 RID: 23904 RVA: 0x00308EA0 File Offset: 0x003070A0
		private void UpdateGUIElements()
		{
			bool isActive = this.CurrentMode == Sonar.Mode.Active;
			this.SonarModeSwitch.Selected = isActive;
			this.passiveTickBox.Selected = !isActive;
			this.activeTickBox.Selected = isActive;
			this.directionalModeSwitch.Selected = this.useDirectionalPing;
			if (this.mineralScannerSwitch != null)
			{
				this.mineralScannerSwitch.Selected = this.UseMineralScanner;
			}
		}

		// Token: 0x06005D61 RID: 23905 RVA: 0x00308F08 File Offset: 0x00307108
		static Sonar()
		{
			Sonar.DirectionalPingDotProduct = (float)Math.Cos((double)(MathHelper.ToRadians(30f) * 0.5f));
		}

		// Token: 0x17001780 RID: 6016
		// (get) Token: 0x06005D62 RID: 23906 RVA: 0x00309190 File Offset: 0x00307390
		public bool UseDirectionalPing
		{
			get
			{
				return this.useDirectionalPing;
			}
		}

		// Token: 0x17001781 RID: 6017
		// (get) Token: 0x06005D63 RID: 23907 RVA: 0x00309198 File Offset: 0x00307398
		public IEnumerable<SonarTransducer> ConnectedTransducers
		{
			get
			{
				return from t in this.connectedTransducers
				select t.Transducer;
			}
		}

		// Token: 0x17001782 RID: 6018
		// (get) Token: 0x06005D64 RID: 23908 RVA: 0x003091C4 File Offset: 0x003073C4
		// (set) Token: 0x06005D65 RID: 23909 RVA: 0x003091CC File Offset: 0x003073CC
		[Serialize(10000f, IsPropertySaveable.No, "The maximum range of the sonar.", "", false)]
		public float Range
		{
			get
			{
				return this.range;
			}
			set
			{
				this.range = MathHelper.Clamp(value, 0f, 100000f);
				Item item = this.item;
				if (((item != null) ? item.AiTarget : null) != null && this.item.AiTarget.MaxSoundRange <= 0f)
				{
					this.item.AiTarget.MaxSoundRange = this.range;
				}
			}
		}

		// Token: 0x17001783 RID: 6019
		// (get) Token: 0x06005D66 RID: 23910 RVA: 0x00309230 File Offset: 0x00307430
		// (set) Token: 0x06005D67 RID: 23911 RVA: 0x00309238 File Offset: 0x00307438
		[Serialize(false, IsPropertySaveable.No, "Should the sonar display the walls of the submarine it is inside.", "", false)]
		public bool DetectSubmarineWalls { get; set; }

		// Token: 0x17001784 RID: 6020
		// (get) Token: 0x06005D68 RID: 23912 RVA: 0x00309241 File Offset: 0x00307441
		// (set) Token: 0x06005D69 RID: 23913 RVA: 0x00309249 File Offset: 0x00307449
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Does the sonar have to be connected to external transducers to work.", "", false)]
		public bool UseTransducers { get; set; }

		// Token: 0x17001785 RID: 6021
		// (get) Token: 0x06005D6A RID: 23914 RVA: 0x00309252 File Offset: 0x00307452
		// (set) Token: 0x06005D6B RID: 23915 RVA: 0x0030925A File Offset: 0x0030745A
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Should the sonar view be centered on the transducers or the submarine's center of mass. Only has an effect if UseTransducers is enabled.", "", false)]
		public bool CenterOnTransducers { get; set; }

		// Token: 0x17001786 RID: 6022
		// (get) Token: 0x06005D6C RID: 23916 RVA: 0x00309263 File Offset: 0x00307463
		// (set) Token: 0x06005D6D RID: 23917 RVA: 0x0030926B File Offset: 0x0030746B
		[Editable]
		[Serialize(false, IsPropertySaveable.No, "Does the sonar have mineral scanning mode. ", "", false)]
		public bool HasMineralScanner
		{
			get
			{
				return this.hasMineralScanner;
			}
			set
			{
				if (this.controlContainer != null && !this.hasMineralScanner && value)
				{
					this.AddMineralScannerSwitchToGUI();
				}
				this.hasMineralScanner = value;
			}
		}

		// Token: 0x17001787 RID: 6023
		// (get) Token: 0x06005D6E RID: 23918 RVA: 0x00309292 File Offset: 0x00307492
		// (set) Token: 0x06005D6F RID: 23919 RVA: 0x0030929A File Offset: 0x0030749A
		[Serialize(true, IsPropertySaveable.Yes, "", "", true)]
		public bool UseMineralScanner { get; set; }

		// Token: 0x17001788 RID: 6024
		// (get) Token: 0x06005D70 RID: 23920 RVA: 0x003092A3 File Offset: 0x003074A3
		// (set) Token: 0x06005D71 RID: 23921 RVA: 0x003092AB File Offset: 0x003074AB
		public float Zoom
		{
			get
			{
				return this.zoom;
			}
			set
			{
				this.zoom = MathHelper.Clamp(value, 1f, 4f);
				this.zoomSlider.BarScroll = MathUtils.InverseLerp(1f, 4f, this.zoom);
			}
		}

		// Token: 0x17001789 RID: 6025
		// (get) Token: 0x06005D72 RID: 23922 RVA: 0x003092E3 File Offset: 0x003074E3
		// (set) Token: 0x06005D73 RID: 23923 RVA: 0x003092EC File Offset: 0x003074EC
		public Sonar.Mode CurrentMode
		{
			get
			{
				return this.currentMode;
			}
			set
			{
				bool changed = this.currentMode != value;
				this.currentMode = value;
				if (changed)
				{
					this.prevPassivePingRadius = float.MaxValue;
				}
				this.UpdateGUIElements();
			}
		}

		// Token: 0x06005D74 RID: 23924 RVA: 0x00309324 File Offset: 0x00307524
		public Sonar(Item item, ContentXElement element) : base(item, element)
		{
			this.connectedTransducers = new List<Sonar.ConnectedTransducer>();
			this.IsActive = true;
			this.InitProjSpecific(element);
			this.CurrentMode = Sonar.Mode.Passive;
			Sonar.SonarList.Add(this);
		}

		// Token: 0x06005D75 RID: 23925 RVA: 0x0030942C File Offset: 0x0030762C
		private void InitProjSpecific(ContentXElement element)
		{
			this.sonarBlips = new List<SonarBlip>();
			Sonar.caveLabel = TextManager.Get("cave").Fallback(TextManager.Get("missiontype.nest"), true);
			Sonar.enemyLabel = TextManager.Get("enemysubmarine");
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					if (length <= 13)
					{
						if (length != 4)
						{
							if (length != 10)
							{
								if (length == 13)
								{
									if (text == "screenoverlay")
									{
										this.screenOverlay = new Sprite(subElement, "", "", false, 1f);
									}
								}
							}
							else
							{
								char c = text[0];
								if (c != 'l')
								{
									if (c == 'p')
									{
										if (text == "pingcircle")
										{
											this.pingCircle = new Sprite(subElement, "", "", false, 1f);
										}
									}
								}
								else if (text == "linesprite")
								{
									this.lineSprite = new Sprite(subElement, "", "", false, 1f);
								}
							}
						}
						else
						{
							char c = text[0];
							if (c != 'b')
							{
								if (c == 'i')
								{
									if (text == "icon")
									{
										Sprite targetIconSprite = new Sprite(subElement, "", "", false, 1f);
										ContentXElement contentXElement = subElement;
										string key = "color";
										Color white = Color.White;
										Color color = contentXElement.GetAttributeColor(key, white);
										this.targetIcons.Add(subElement.GetAttributeIdentifier("identifier", Identifier.Empty), new Tuple<Sprite, Color>(targetIconSprite, color));
									}
								}
							}
							else if (text == "blip")
							{
								this.sonarBlip = new Sprite(subElement, "", "", false, 1f);
							}
						}
					}
					else if (length != 16)
					{
						if (length != 21)
						{
							if (length == 25)
							{
								if (text == "directionalpingbackground")
								{
									this.directionalPingBackground = new Sprite(subElement, "", "", false, 1f);
								}
							}
						}
						else
						{
							char c = text[15];
							if (c != 'b')
							{
								if (c == 'c')
								{
									if (text == "directionalpingcircle")
									{
										this.directionalPingCircle = new Sprite(subElement, "", "", false, 1f);
									}
								}
							}
							else if (text == "directionalpingbutton")
							{
								if (this.directionalPingButton == null)
								{
									this.directionalPingButton = new Sprite[3];
								}
								int index = subElement.GetAttributeInt("index", 0);
								this.directionalPingButton[index] = new Sprite(subElement, "", "", false, 1f);
							}
						}
					}
					else if (text == "screenbackground")
					{
						this.screenBackground = new Sprite(subElement, "", "", false, 1f);
					}
				}
			}
			this.CreateGUI();
		}

		// Token: 0x06005D76 RID: 23926 RVA: 0x003097B4 File Offset: 0x003079B4
		public override void Update(float deltaTime, Camera cam)
		{
			base.UpdateOnActiveEffects(deltaTime);
			if (this.UseTransducers)
			{
				foreach (Sonar.ConnectedTransducer transducer in this.connectedTransducers)
				{
					transducer.DisconnectTimer -= deltaTime;
				}
				this.connectedTransducers.RemoveAll((Sonar.ConnectedTransducer t) => t.DisconnectTimer <= 0f);
			}
			for (int pingIndex = 0; pingIndex < this.activePingsCount; pingIndex++)
			{
				this.activePings[pingIndex].State += deltaTime * 0.5f;
			}
			if (this.currentMode == Sonar.Mode.Active)
			{
				if (this.HasPower && (!this.UseTransducers || this.connectedTransducers.Count > 0))
				{
					if (this.currentPingIndex != -1)
					{
						Sonar.ActivePing activePing = this.activePings[this.currentPingIndex];
						if (activePing.State > 1f)
						{
							this.aiPingCheckPending = true;
							this.currentPingIndex = -1;
						}
					}
					if (this.currentPingIndex == -1 && this.activePingsCount < this.activePings.Length)
					{
						int num = this.activePingsCount;
						this.activePingsCount = num + 1;
						this.currentPingIndex = num;
						if (this.activePings[this.currentPingIndex] == null)
						{
							this.activePings[this.currentPingIndex] = new Sonar.ActivePing();
						}
						this.activePings[this.currentPingIndex].IsDirectional = this.useDirectionalPing;
						this.activePings[this.currentPingIndex].Direction = this.pingDirection;
						this.activePings[this.currentPingIndex].State = 0f;
						this.activePings[this.currentPingIndex].PrevPingRadius = 0f;
						foreach (AITarget aiTarget in this.GetAITargets())
						{
							aiTarget.SectorDegrees = (this.useDirectionalPing ? 30f : 360f);
							aiTarget.SectorDir = new Vector2(this.pingDirection.X, -this.pingDirection.Y);
						}
						this.item.Use(deltaTime, null, null, null, null);
					}
				}
				else
				{
					this.aiPingCheckPending = false;
				}
			}
			int pingIndex2 = 0;
			while (pingIndex2 < this.activePingsCount)
			{
				foreach (AITarget aiTarget2 in this.GetAITargets())
				{
					float range = MathUtils.InverseLerp(aiTarget2.MinSoundRange, aiTarget2.MaxSoundRange, this.Range * this.activePings[pingIndex2].State / this.zoom);
					aiTarget2.SoundRange = Math.Max(aiTarget2.SoundRange, MathHelper.Lerp(aiTarget2.MinSoundRange, aiTarget2.MaxSoundRange, range));
				}
				if (this.activePings[pingIndex2].State > 1f)
				{
					int num = this.activePingsCount - 1;
					this.activePingsCount = num;
					int lastIndex = num;
					Sonar.ActivePing oldActivePing = this.activePings[pingIndex2];
					this.activePings[pingIndex2] = this.activePings[lastIndex];
					this.activePings[lastIndex] = oldActivePing;
					if (this.currentPingIndex == lastIndex)
					{
						this.currentPingIndex = pingIndex2;
					}
				}
				else
				{
					pingIndex2++;
				}
			}
		}

		// Token: 0x06005D77 RID: 23927 RVA: 0x00309B38 File Offset: 0x00307D38
		private IEnumerable<AITarget> GetAITargets()
		{
			Sonar.<GetAITargets>d__169 <GetAITargets>d__ = new Sonar.<GetAITargets>d__169(-2);
			<GetAITargets>d__.<>4__this = this;
			return <GetAITargets>d__;
		}

		// Token: 0x06005D78 RID: 23928 RVA: 0x00309B48 File Offset: 0x00307D48
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			if (this.currentMode != Sonar.Mode.Active)
			{
				return this.powerConsumption * 0.1f;
			}
			return this.powerConsumption;
		}

		// Token: 0x06005D79 RID: 23929 RVA: 0x00309B7C File Offset: 0x00307D7C
		public override bool Use(float deltaTime, Character character = null)
		{
			return this.currentPingIndex != -1 && (character == null || this.characterUsable);
		}

		// Token: 0x06005D7A RID: 23930 RVA: 0x00309B94 File Offset: 0x00307D94
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			if (this.currentMode == Sonar.Mode.Passive || !this.aiPingCheckPending)
			{
				return false;
			}
			foreach (List<Character> targetGroup in Sonar.targetGroups.Values)
			{
				targetGroup.Clear();
			}
			foreach (Character c in Character.CharacterList)
			{
				if (!c.IsDead && !c.Removed && c.Enabled && c.AnimController.CurrentHull == null && !c.Params.HideInSonar && (!this.DetectSubmarineWalls || c.AnimController.CurrentHull != null || this.item.CurrentHull == null) && Vector2.DistanceSquared(c.WorldPosition, this.item.WorldPosition) <= this.range * this.range)
				{
					string directionName = this.GetDirectionName(c.WorldPosition - this.item.WorldPosition).Value;
					if (!Sonar.targetGroups.ContainsKey(directionName))
					{
						Sonar.targetGroups.Add(directionName, new List<Character>());
					}
					Sonar.targetGroups[directionName].Add(c);
				}
			}
			foreach (KeyValuePair<string, List<Character>> targetGroup2 in Sonar.targetGroups)
			{
				if (targetGroup2.Value.Any<Character>())
				{
					string dialogTag = "DialogSonarTarget";
					if (targetGroup2.Value.Count > 1)
					{
						dialogTag = "DialogSonarTargetMultiple";
					}
					else if (targetGroup2.Value[0].Mass > 100f)
					{
						dialogTag = "DialogSonarTargetLarge";
					}
					if (character.IsOnPlayerTeam)
					{
						string value = TextManager.GetWithVariables(dialogTag, new ValueTuple<string, string, FormatCapitals>[]
						{
							new ValueTuple<string, string, FormatCapitals>("[direction]", targetGroup2.Key.ToString(), FormatCapitals.Yes),
							new ValueTuple<string, string, FormatCapitals>("[count]", targetGroup2.Value.Count.ToString(), FormatCapitals.No)
						}).Value;
						ChatMessageType? messageType = null;
						float delay = 0f;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
						defaultInterpolatedStringHandler.AppendLiteral("sonartarget");
						defaultInterpolatedStringHandler.AppendFormatted<ushort>(targetGroup2.Value[0].ID);
						character.Speak(value, messageType, delay, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier(), 60f);
					}
					for (int i = 1; i < targetGroup2.Value.Count; i++)
					{
						character.DisableLine("sonartarget" + targetGroup2.Value[i].ID.ToString());
					}
				}
			}
			return true;
		}

		// Token: 0x06005D7B RID: 23931 RVA: 0x00309ECC File Offset: 0x003080CC
		private LocalizedString GetDirectionName(Vector2 dir)
		{
			float angle = MathUtils.WrapAngleTwoPi((float)(-(float)Math.Atan2((double)dir.Y, (double)dir.X)) + 1.5707964f);
			int clockDir = (int)Math.Round((double)(angle / 6.2831855f * 12f));
			if (clockDir == 0)
			{
				clockDir = 12;
			}
			return TextManager.GetWithVariable("roomname.subdiroclock", "[dir]", clockDir.ToString(), FormatCapitals.No);
		}

		// Token: 0x06005D7C RID: 23932 RVA: 0x00309F34 File Offset: 0x00308134
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			base.ReceiveSignal(signal, connection);
			if (connection.Name == "transducer_in")
			{
				SonarTransducer transducer = signal.source.GetComponent<SonarTransducer>();
				if (transducer == null)
				{
					return;
				}
				transducer.ConnectedSonar = this;
				Sonar.ConnectedTransducer connectedTransducer = this.connectedTransducers.Find((Sonar.ConnectedTransducer t) => t.Transducer == transducer);
				if (connectedTransducer == null)
				{
					this.connectedTransducers.Add(new Sonar.ConnectedTransducer(transducer, signal.strength, 1f));
					return;
				}
				connectedTransducer.SignalStrength = signal.strength;
				connectedTransducer.DisconnectTimer = 1f;
			}
		}

		// Token: 0x06005D7D RID: 23933 RVA: 0x00309FE0 File Offset: 0x003081E0
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Sprite sprite = this.sonarBlip;
			if (sprite != null)
			{
				sprite.Remove();
			}
			Sprite sprite2 = this.pingCircle;
			if (sprite2 != null)
			{
				sprite2.Remove();
			}
			Sprite sprite3 = this.directionalPingCircle;
			if (sprite3 != null)
			{
				sprite3.Remove();
			}
			Sprite sprite4 = this.screenOverlay;
			if (sprite4 != null)
			{
				sprite4.Remove();
			}
			Sprite sprite5 = this.screenBackground;
			if (sprite5 != null)
			{
				sprite5.Remove();
			}
			Sprite sprite6 = this.lineSprite;
			if (sprite6 != null)
			{
				sprite6.Remove();
			}
			foreach (Tuple<Sprite, Color> t in this.targetIcons.Values)
			{
				t.Item1.Remove();
			}
			this.targetIcons.Clear();
			this.MineralClusters = null;
			Sonar.SonarList.Remove(this);
		}

		// Token: 0x06005D7E RID: 23934 RVA: 0x0030A0C8 File Offset: 0x003082C8
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			bool isActive = msg.ReadBoolean();
			bool directionalPing = this.useDirectionalPing;
			float zoomT = this.zoom;
			float pingDirectionT = 0f;
			bool mineralScanner = this.UseMineralScanner;
			if (isActive)
			{
				zoomT = msg.ReadRangedSingle(0f, 1f, 8);
				directionalPing = msg.ReadBoolean();
				if (directionalPing)
				{
					pingDirectionT = msg.ReadRangedSingle(0f, 1f, 8);
				}
				mineralScanner = msg.ReadBoolean();
			}
			if (!this.item.CanClientAccess(c))
			{
				return;
			}
			this.CurrentMode = (isActive ? Sonar.Mode.Active : Sonar.Mode.Passive);
			if (isActive)
			{
				this.zoom = MathHelper.Lerp(1f, 4f, zoomT);
				this.useDirectionalPing = directionalPing;
				if (this.useDirectionalPing)
				{
					float pingAngle = MathHelper.Lerp(0f, 6.2831855f, pingDirectionT);
					this.pingDirection = new Vector2((float)Math.Cos((double)pingAngle), (float)Math.Sin((double)pingAngle));
				}
				this.UseMineralScanner = mineralScanner;
				this.zoomSlider.BarScroll = zoomT;
				this.directionalModeSwitch.Selected = this.useDirectionalPing;
				if (this.mineralScannerSwitch != null)
				{
					this.mineralScannerSwitch.Selected = this.UseMineralScanner;
				}
			}
		}

		// Token: 0x06005D7F RID: 23935 RVA: 0x0030A1E8 File Offset: 0x003083E8
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.currentMode == Sonar.Mode.Active);
			if (this.currentMode == Sonar.Mode.Active)
			{
				msg.WriteRangedSingle(this.zoom, 1f, 4f, 8);
				msg.WriteBoolean(this.useDirectionalPing);
				if (this.useDirectionalPing)
				{
					float pingAngle = MathUtils.WrapAngleTwoPi(MathUtils.VectorToAngle(this.pingDirection));
					msg.WriteRangedSingle(MathUtils.InverseLerp(0f, 6.2831855f, pingAngle), 0f, 1f, 8);
				}
				msg.WriteBoolean(this.UseMineralScanner);
			}
		}

		// Token: 0x06005D85 RID: 23941 RVA: 0x0030A361 File Offset: 0x00308561
		[CompilerGenerated]
		internal static int <PreventMineralScannerOverlap>g__GetContainerBottom|88_0(ref Sonar.<>c__DisplayClass88_0 A_0)
		{
			return A_0.containerBottom - A_0.amountRaised;
		}

		// Token: 0x06005D89 RID: 23945 RVA: 0x0030A39C File Offset: 0x0030859C
		[CompilerGenerated]
		private void <UpdateHUDComponentSpecific>g__AddIfValid|89_2(Level.ClusterLocation c)
		{
			if (c.Resources == null)
			{
				return;
			}
			if (c.Resources.None((Item i) => i != null && !i.Removed && i.Tags.Contains("ore")))
			{
				return;
			}
			Vector2 pos = Vector2.Zero;
			foreach (Item r in c.Resources)
			{
				pos += r.WorldPosition;
			}
			pos /= (float)c.Resources.Count;
			this.MineralClusters.Add(new ValueTuple<Vector2, List<Item>>(pos, c.Resources));
		}

		// Token: 0x06005D8A RID: 23946 RVA: 0x0030A460 File Offset: 0x00308660
		[CompilerGenerated]
		internal static float <DrawSonar>g__MetersToUnits|91_0(float m)
		{
			return m / Physics.DisplayToRealWorldRatio;
		}

		// Token: 0x06005D8B RID: 23947 RVA: 0x0030A46C File Offset: 0x0030866C
		[CompilerGenerated]
		private void <UpdateDisruptions>g__CreateBlipsForDisruption|97_0(Vector2 disruptionPos, float disruptionStrength, ref Sonar.<>c__DisplayClass97_0 A_3)
		{
			disruptionStrength = Math.Min(disruptionStrength, 10f);
			Vector2 dir = disruptionPos - A_3.pingSource;
			int i = 0;
			while ((float)i < disruptionStrength * 10f)
			{
				Vector2 pos = disruptionPos + Rand.Vector(Rand.Range(0f, 8000f * disruptionStrength, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced);
				if (Vector2.Dot(pos - A_3.pingSource, -dir) <= 1f - disruptionStrength)
				{
					SonarBlip blip = new SonarBlip(pos, MathHelper.Lerp(0.1f, 1.5f, Math.Min(disruptionStrength, 1f)), Rand.Range(0.2f, 1f + disruptionStrength, Rand.RandSync.Unsynced), Sonar.BlipType.Disruption);
					this.sonarBlips.Add(blip);
				}
				i++;
			}
		}

		// Token: 0x06005D8C RID: 23948 RVA: 0x0030A52B File Offset: 0x0030872B
		[CompilerGenerated]
		private bool <Ping>g__IsVisible|99_0(SonarBlip blip, ref Sonar.<>c__DisplayClass99_0 A_2)
		{
			return (A_2.passive || this.CheckBlipVisibility(blip, A_2.transducerPos)) && (A_2.needsToBeInSector == null || A_2.needsToBeInSector.IsWithinSector(blip.Position));
		}

		// Token: 0x06005D8D RID: 23949 RVA: 0x0030A564 File Offset: 0x00308764
		[CompilerGenerated]
		private void <CreateBlipsForSubmarineWalls>g__CreateBlips|101_0(bool isHorizontal, Vector2 worldPos, Rectangle worldRect, float rotation, Sonar.BlipType blipType = Sonar.BlipType.Default, ref Sonar.<>c__DisplayClass101_0 A_6)
		{
			Vector2 point;
			Vector2 point2;
			if (isHorizontal)
			{
				point = new Vector2((float)worldRect.X, worldPos.Y);
				point2 = new Vector2((float)worldRect.Right, worldPos.Y);
			}
			else
			{
				point = new Vector2(worldPos.X, (float)worldRect.Y);
				point2 = new Vector2(worldPos.X, (float)(worldRect.Y - worldRect.Height));
			}
			if (!MathUtils.NearlyEqual(rotation, 0f, 0.0001f))
			{
				float rotationRad = MathHelper.ToRadians(rotation);
				point = MathUtils.RotatePointAroundTarget(point, worldPos, rotationRad, true);
				point2 = MathUtils.RotatePointAroundTarget(point2, worldPos, rotationRad, true);
			}
			this.CreateBlipsForLine(point, point2, A_6.pingSource, A_6.transducerPos, A_6.pingRadius, A_6.prevPingRadius, 50f, 5f, A_6.range, 2f, A_6.passive, blipType, null);
		}

		// Token: 0x06005D8E RID: 23950 RVA: 0x0030A644 File Offset: 0x00308844
		[CompilerGenerated]
		internal static CachedLocation <DrawOffsetMarker>g__CreateCachedLocation|106_0(ref Sonar.<>c__DisplayClass106_0 A_0)
		{
			float distance = Vector2.Distance(A_0.worldPosition, A_0.transducerPosition);
			float maxOffset = MathHelper.Lerp(A_0.offset.Start, A_0.offset.End, MathHelper.Clamp((distance - A_0.distanceThresholds.Start) / (A_0.distanceThresholds.End - A_0.distanceThresholds.Start), 0f, 1f));
			Vector2 randomPos = Rand.Vector(Rand.Range(A_0.minOffset, maxOffset, Rand.RandSync.Unsynced), Rand.RandSync.Unsynced);
			return new CachedLocation(A_0.worldPosition + randomPos, Timing.TotalTime + (double)Rand.Range(10f, 30f, Rand.RandSync.Unsynced));
		}

		// Token: 0x06005D8F RID: 23951 RVA: 0x0030A6F0 File Offset: 0x003088F0
		[CompilerGenerated]
		private void <DrawMarker>g__CalculateDistance|107_0(ref Sonar.<>c__DisplayClass107_0 A_1)
		{
			if (this.pathFinder == null)
			{
				this.pathFinder = new PathFinder(WayPoint.WayPointList, false);
			}
			SteeringPath path = this.pathFinder.FindPath(ConvertUnits.ToSimUnits(A_1.transducerPosition), ConvertUnits.ToSimUnits(A_1.worldPosition), null, null, 0f, null, null, null, true, 0f);
			if (!path.Unreachable)
			{
				CachedDistance cachedDistance = new CachedDistance(A_1.transducerPosition, A_1.worldPosition, path.TotalLength, Timing.TotalTime + (double)Rand.Range(1f, 5f, Rand.RandSync.Unsynced));
				this.markerDistances.Add(A_1.targetIdentifier, cachedDistance);
				A_1.dist = path.TotalLength;
				return;
			}
			CachedDistance cachedDistance2 = new CachedDistance(A_1.transducerPosition, A_1.worldPosition, A_1.linearDist, Timing.TotalTime + (double)Rand.Range(4f, 7f, Rand.RandSync.Unsynced));
			this.markerDistances.Add(A_1.targetIdentifier, cachedDistance2);
		}

		// Token: 0x04002FD3 RID: 12243
		private PathFinder pathFinder;

		// Token: 0x04002FD4 RID: 12244
		private readonly bool dynamicDockingIndicator = true;

		// Token: 0x04002FD5 RID: 12245
		private bool unsentChanges;

		// Token: 0x04002FD6 RID: 12246
		private float networkUpdateTimer;

		// Token: 0x04002FD8 RID: 12248
		private GUITickBox activeTickBox;

		// Token: 0x04002FD9 RID: 12249
		private GUITickBox passiveTickBox;

		// Token: 0x04002FDA RID: 12250
		private GUITextBlock signalWarningText;

		// Token: 0x04002FDB RID: 12251
		private GUIFrame lowerAreaFrame;

		// Token: 0x04002FDC RID: 12252
		private GUIScrollBar zoomSlider;

		// Token: 0x04002FDD RID: 12253
		private GUIButton directionalModeSwitch;

		// Token: 0x04002FDE RID: 12254
		private Vector2? pingDragDirection;

		// Token: 0x04002FDF RID: 12255
		private GUIButton mineralScannerSwitch;

		// Token: 0x04002FE0 RID: 12256
		private GUIFrame controlContainer;

		// Token: 0x04002FE1 RID: 12257
		private GUICustomComponent sonarView;

		// Token: 0x04002FE2 RID: 12258
		private Sprite directionalPingBackground;

		// Token: 0x04002FE3 RID: 12259
		private Sprite[] directionalPingButton;

		// Token: 0x04002FE4 RID: 12260
		private Sprite pingCircle;

		// Token: 0x04002FE5 RID: 12261
		private Sprite directionalPingCircle;

		// Token: 0x04002FE6 RID: 12262
		private Sprite screenOverlay;

		// Token: 0x04002FE7 RID: 12263
		private Sprite screenBackground;

		// Token: 0x04002FE8 RID: 12264
		private Sprite sonarBlip;

		// Token: 0x04002FE9 RID: 12265
		private Sprite lineSprite;

		// Token: 0x04002FEA RID: 12266
		private readonly Dictionary<Identifier, Tuple<Sprite, Color>> targetIcons = new Dictionary<Identifier, Tuple<Sprite, Color>>();

		// Token: 0x04002FEB RID: 12267
		private float displayBorderSize;

		// Token: 0x04002FEC RID: 12268
		private List<SonarBlip> sonarBlips;

		// Token: 0x04002FED RID: 12269
		private float prevPassivePingRadius;

		// Token: 0x04002FEE RID: 12270
		private Vector2 center;

		// Token: 0x04002FF0 RID: 12272
		private const float DisruptionUpdateInterval = 0.2f;

		// Token: 0x04002FF1 RID: 12273
		private float disruptionUpdateTimer;

		// Token: 0x04002FF2 RID: 12274
		private const float LongRangeUpdateInterval = 10f;

		// Token: 0x04002FF3 RID: 12275
		private float longRangeUpdateTimer;

		// Token: 0x04002FF4 RID: 12276
		private float showDirectionalIndicatorTimer;

		// Token: 0x04002FF5 RID: 12277
		private readonly List<LevelObject> nearbyObjects = new List<LevelObject>();

		// Token: 0x04002FF6 RID: 12278
		private const float NearbyObjectUpdateInterval = 1f;

		// Token: 0x04002FF7 RID: 12279
		private float nearbyObjectUpdateTimer;

		// Token: 0x04002FF8 RID: 12280
		private readonly List<Submarine> connectedSubs = new List<Submarine>();

		// Token: 0x04002FF9 RID: 12281
		private const float ConnectedSubUpdateInterval = 1f;

		// Token: 0x04002FFA RID: 12282
		private float connectedSubUpdateTimer;

		// Token: 0x04002FFB RID: 12283
		[TupleElementNames(new string[]
		{
			"pos",
			"strength"
		})]
		private readonly List<ValueTuple<Vector2, float>> disruptedDirections = new List<ValueTuple<Vector2, float>>();

		// Token: 0x04002FFC RID: 12284
		private readonly Dictionary<object, CachedDistance> markerDistances = new Dictionary<object, CachedDistance>();

		// Token: 0x04002FFD RID: 12285
		private readonly Color positiveColor = Color.Green;

		// Token: 0x04002FFE RID: 12286
		private readonly Color warningColor = Color.Orange;

		// Token: 0x04002FFF RID: 12287
		private readonly Color negativeColor = Color.Red;

		// Token: 0x04003000 RID: 12288
		private readonly Color markerColor = Color.Red;

		// Token: 0x04003001 RID: 12289
		public static readonly Vector2 controlBoxSize = new Vector2(0.33f, 0.32f);

		// Token: 0x04003002 RID: 12290
		public static readonly Vector2 controlBoxOffset = new Vector2(0.025f, 0f);

		// Token: 0x04003003 RID: 12291
		private static readonly float sonarAreaSize = 1.09f;

		// Token: 0x04003004 RID: 12292
		private static readonly Dictionary<Sonar.BlipType, Color[]> blipColorGradient = new Dictionary<Sonar.BlipType, Color[]>
		{
			{
				Sonar.BlipType.Default,
				new Color[]
				{
					Color.TransparentBlack,
					new Color(0, 50, 160),
					new Color(0, 133, 166),
					new Color(2, 159, 30),
					new Color(255, 255, 255)
				}
			},
			{
				Sonar.BlipType.Disruption,
				new Color[]
				{
					Color.TransparentBlack,
					new Color(254, 68, 19),
					new Color(255, 220, 62),
					new Color(255, 255, 255)
				}
			},
			{
				Sonar.BlipType.Destructible,
				new Color[]
				{
					Color.TransparentBlack,
					new Color(94, 114, 73) * 0.8f,
					new Color(255, 236, 151) * 0.8f,
					new Color(242, 243, 194) * 0.8f
				}
			},
			{
				Sonar.BlipType.Door,
				new Color[]
				{
					Color.TransparentBlack,
					new Color(73, 78, 86),
					new Color(66, 94, 100),
					new Color(47, 115, 58),
					new Color(255, 255, 255)
				}
			},
			{
				Sonar.BlipType.LongRange,
				new Color[]
				{
					Color.TransparentBlack,
					Color.TransparentBlack,
					new Color(254, 68, 19) * 0.8f,
					Color.TransparentBlack
				}
			}
		};

		// Token: 0x04003005 RID: 12293
		private float prevDockingDist;

		// Token: 0x04003009 RID: 12297
		private readonly List<GUITextBlock> textBlocksToScaleAndNormalize = new List<GUITextBlock>();

		// Token: 0x0400300A RID: 12298
		private bool isConnectedToSteering;

		// Token: 0x0400300B RID: 12299
		private static LocalizedString caveLabel;

		// Token: 0x0400300C RID: 12300
		private static LocalizedString enemyLabel;

		// Token: 0x0400300E RID: 12302
		private readonly Dictionary<Identifier, CachedLocation> cachedLocations = new Dictionary<Identifier, CachedLocation>();

		// Token: 0x0400300F RID: 12303
		public static List<Sonar> SonarList = new List<Sonar>();

		// Token: 0x04003010 RID: 12304
		public const float DefaultSonarRange = 10000f;

		// Token: 0x04003011 RID: 12305
		public const float PassivePowerConsumption = 0.1f;

		// Token: 0x04003012 RID: 12306
		private const float DirectionalPingSector = 30f;

		// Token: 0x04003013 RID: 12307
		private static readonly float DirectionalPingDotProduct;

		// Token: 0x04003014 RID: 12308
		private float range;

		// Token: 0x04003015 RID: 12309
		private const float PingFrequency = 0.5f;

		// Token: 0x04003016 RID: 12310
		private Sonar.Mode currentMode = Sonar.Mode.Passive;

		// Token: 0x04003017 RID: 12311
		private Sonar.ActivePing[] activePings = new Sonar.ActivePing[8];

		// Token: 0x04003018 RID: 12312
		private int activePingsCount;

		// Token: 0x04003019 RID: 12313
		private int currentPingIndex = -1;

		// Token: 0x0400301A RID: 12314
		private const float MinZoom = 1f;

		// Token: 0x0400301B RID: 12315
		private const float MaxZoom = 4f;

		// Token: 0x0400301C RID: 12316
		private float zoom = 1f;

		// Token: 0x0400301D RID: 12317
		private bool useDirectionalPing;

		// Token: 0x0400301E RID: 12318
		private Vector2 pingDirection = new Vector2(1f, 0f);

		// Token: 0x0400301F RID: 12319
		private bool aiPingCheckPending;

		// Token: 0x04003020 RID: 12320
		private readonly List<Sonar.ConnectedTransducer> connectedTransducers;

		// Token: 0x04003024 RID: 12324
		private bool hasMineralScanner;

		// Token: 0x04003026 RID: 12326
		private static readonly Dictionary<string, List<Character>> targetGroups = new Dictionary<string, List<Character>>();

		// Token: 0x0200141E RID: 5150
		public enum BlipType
		{
			// Token: 0x04006473 RID: 25715
			Default,
			// Token: 0x04006474 RID: 25716
			Disruption,
			// Token: 0x04006475 RID: 25717
			Destructible,
			// Token: 0x04006476 RID: 25718
			Door,
			// Token: 0x04006477 RID: 25719
			LongRange
		}

		// Token: 0x0200141F RID: 5151
		public enum Mode
		{
			// Token: 0x04006479 RID: 25721
			Active,
			// Token: 0x0400647A RID: 25722
			Passive
		}

		// Token: 0x02001420 RID: 5152
		private class ConnectedTransducer
		{
			// Token: 0x060099C1 RID: 39361 RVA: 0x003E10A5 File Offset: 0x003DF2A5
			public ConnectedTransducer(SonarTransducer transducer, float signalStrength, float disconnectTimer)
			{
				this.Transducer = transducer;
				this.SignalStrength = signalStrength;
				this.DisconnectTimer = disconnectTimer;
			}

			// Token: 0x0400647B RID: 25723
			public readonly SonarTransducer Transducer;

			// Token: 0x0400647C RID: 25724
			public float SignalStrength;

			// Token: 0x0400647D RID: 25725
			public float DisconnectTimer;
		}

		// Token: 0x02001421 RID: 5153
		private class ActivePing
		{
			// Token: 0x0400647E RID: 25726
			public float State;

			// Token: 0x0400647F RID: 25727
			public bool IsDirectional;

			// Token: 0x04006480 RID: 25728
			public Vector2 Direction;

			// Token: 0x04006481 RID: 25729
			public float PrevPingRadius;
		}
	}
}
