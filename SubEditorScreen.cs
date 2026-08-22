using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.RuinGeneration;
using Barotrauma.Sounds;
using Barotrauma.Steam;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Steamworks;
using Steamworks.Ugc;

namespace Barotrauma
{
	// Token: 0x0200011F RID: 287
	internal class SubEditorScreen : EditorScreen
	{
		// Token: 0x17000A41 RID: 2625
		// (get) Token: 0x06002715 RID: 10005 RVA: 0x001A208C File Offset: 0x001A028C
		// (set) Token: 0x06002716 RID: 10006 RVA: 0x001A2093 File Offset: 0x001A0293
		private static Submarine MainSub
		{
			get
			{
				return Submarine.MainSub;
			}
			set
			{
				Submarine.MainSub = value;
			}
		}

		// Token: 0x17000A42 RID: 2626
		// (get) Token: 0x06002717 RID: 10007 RVA: 0x001A209B File Offset: 0x001A029B
		public bool TransformWidgetSelected
		{
			get
			{
				return this.TransformWidget.IsSelected;
			}
		}

		// Token: 0x06002718 RID: 10008 RVA: 0x001A20A8 File Offset: 0x001A02A8
		private static Vector2 GetSelectionCenter()
		{
			IEnumerable<MapEntity> nonWireEntities = MapEntity.FilteredSelectedList.Where(delegate(MapEntity entity)
			{
				Item item = entity as Item;
				Wire wire = (item != null) ? item.GetComponent<Wire>() : null;
				return wire == null || !wire.Drawable;
			});
			if (nonWireEntities.None(null))
			{
				return Vector2.Zero;
			}
			float minX = nonWireEntities.Min((MapEntity entity) => entity.DrawPosition.X);
			float minY = nonWireEntities.Min((MapEntity entity) => entity.DrawPosition.Y);
			float maxX = nonWireEntities.Max((MapEntity entity) => entity.DrawPosition.X);
			float maxY = nonWireEntities.Max((MapEntity entity) => entity.DrawPosition.Y);
			return new Vector2(minX + maxX, minY + maxY) / 2f;
		}

		// Token: 0x17000A43 RID: 2627
		// (get) Token: 0x06002719 RID: 10009 RVA: 0x001A21A0 File Offset: 0x001A03A0
		private Widget TransformWidget
		{
			get
			{
				if (this.transformWidget != null)
				{
					return this.transformWidget;
				}
				int size = GUI.IntScale(16f);
				this.transformWidget = new Widget("scale", size, WidgetShape.Rectangle)
				{
					Enabled = false,
					Color = GUIStyle.Yellow,
					InputAreaMargin = 20,
					RequireMouseOn = false,
					TooltipOffset = new Vector2?(new ValueTuple<float, float>((float)size / 2f, (float)(-(float)size) / 2f)),
					IsFilled = true
				};
				this.transformWidget.PreUpdate += delegate(float _)
				{
					this.transformWidget.Enabled = (MapEntity.FilteredSelectedList.Any<MapEntity>() && (this.rotateToolToggle.Selected || this.scaleToolToggle.Selected));
					if (this.transformWidget.IsSelected && PlayerInput.PrimaryMouseButtonReleased())
					{
						GUIListBox listBox = MapEntity.EditingHUD.GetChild<GUIListBox>();
						if (listBox != null)
						{
							SerializableEntityEditor.LockEditing = true;
							listBox.Content.Children.OfType<SerializableEntityEditor>().ForEach(delegate(SerializableEntityEditor editor)
							{
								editor.RefreshValues();
							});
							SerializableEntityEditor.LockEditing = false;
						}
						Widget.SelectedWidgets.Remove(this.transformWidget);
						SubEditorScreen.StoreCommand(this.transformCommand);
						this.transformWidget.Color = Color.Yellow;
					}
				};
				this.transformWidget.Selected += delegate()
				{
					this.transformWidget.Color = GUIStyle.Blue;
					IEnumerable<Item> containedItems = MapEntity.SelectedList.OfType<Item>().SelectManyRecursive((Item item) => item.ContainedItems);
					IEnumerable<MapEntity> allEntities = MapEntity.SelectedList.Concat(containedItems).Distinct<MapEntity>();
					Dictionary<MapEntity, SubEditorScreen.TransformData> oldTransformData = allEntities.ToDictionary((MapEntity entity) => entity, delegate(MapEntity entity)
					{
						SubEditorScreen.<>c__DisplayClass23_0 CS$<>8__locals1;
						CS$<>8__locals1.item = (entity as Item);
						Structure structure = entity as Structure;
						float num;
						if (!(entity is Structure))
						{
							if (!(entity is Item))
							{
								num = 0f;
							}
							else
							{
								num = CS$<>8__locals1.item.RotationRad;
							}
						}
						else
						{
							num = MathHelper.ToRadians(structure.Rotation);
						}
						float rotation = num;
						return new SubEditorScreen.TransformData(entity.Scale, rotation, entity.DrawPosition, entity.Rect, (structure != null) ? new Vector2?(structure.TextureOffset) : null, SubEditorScreen.<get_TransformWidget>g__GetPropertyDict|23_12<Wire, ValueTuple<List<Vector2>, float>>((Wire wire) => new ValueTuple<List<Vector2>, float>(wire.GetNodes(), wire.Width), ref CS$<>8__locals1), SubEditorScreen.<get_TransformWidget>g__GetPropertyDict|23_12<ItemLabel, float>((ItemLabel label) => label.TextScale, ref CS$<>8__locals1), SubEditorScreen.<get_TransformWidget>g__GetPropertyDict|23_12<LightComponent, float>((LightComponent light) => light.Range, ref CS$<>8__locals1), SubEditorScreen.<get_TransformWidget>g__GetPropertyDict|23_12<Turret, Vector2>((Turret turret) => turret.RotationLimits, ref CS$<>8__locals1));
					});
					this.transformCommand = new TransformToolCommand(oldTransformData, SubEditorScreen.GetSelectionCenter());
				};
				this.transformWidget.MouseHeld += delegate(float _)
				{
					MapEntity.DisableSelect = true;
					Vector2 widgetWorldPos = this.Cam.ScreenToWorld(this.transformWidget.DrawPos * 2f);
					if (MathUtils.NearlyEqual(widgetWorldPos, this.oldWidgetWorldPos, 0.0001f))
					{
						return;
					}
					this.oldWidgetWorldPos = widgetWorldPos;
					this.transformCommand.RotationRad = null;
					LocalizedString rotationString = null;
					if (this.rotateToolToggle.Selected)
					{
						this.transformCommand.RotationRad = new float?(MathUtils.VectorToAngle(PlayerInput.MousePosition - this.Cam.WorldToScreen(this.transformCommand.Pivot)));
						if (!PlayerInput.IsShiftDown())
						{
							this.transformCommand.RotationRad = new float?(MathUtils.RoundTowardsClosest(this.transformCommand.RotationRad.Value, 0.08726647f));
						}
						rotationString = TextManager.GetWithVariable("SubEditor.TransformWidget.Rotation", "[value]", MathHelper.ToDegrees(this.transformCommand.RotationRad.Value).ToString("0.000", CultureInfo.CurrentCulture), FormatCapitals.No);
					}
					this.transformCommand.ScaleMult = null;
					LocalizedString scaleString = null;
					if (this.scaleToolToggle.Selected)
					{
						this.transformCommand.ScaleMult = new float?(Vector2.Distance(PlayerInput.MousePosition, this.Cam.WorldToScreen(this.transformCommand.Pivot)) / (300f * GUI.Scale));
						if (!PlayerInput.IsShiftDown())
						{
							this.transformCommand.ScaleMult = new float?(MathUtils.RoundTowardsClosest(this.transformCommand.ScaleMult.Value, 0.1f));
						}
						this.transformCommand.ScaleMult = new float?(Math.Clamp(this.transformCommand.ScaleMult.Value, this.transformCommand.MinScale, this.transformCommand.MaxScale));
						scaleString = TextManager.GetWithVariable("SubEditor.TransformWidget.Scale", "[value]", this.transformCommand.ScaleMult.Value.ToString("0.000", CultureInfo.CurrentCulture), FormatCapitals.No);
					}
					this.transformWidget.Tooltip = ((!rotationString.IsNullOrEmpty() && !scaleString.IsNullOrEmpty()) ? LocalizedString.Join("\n", new LocalizedString[]
					{
						rotationString,
						scaleString
					}) : (this.transformWidget.Tooltip = (rotationString ?? scaleString)));
					this.transformCommand.Execute();
				};
				this.transformWidget.PreDraw += delegate(SpriteBatch sb, float _)
				{
					Vector2 selectionCenterScreenPos = this.Cam.WorldToScreen(this.transformWidget.IsSelected ? this.transformCommand.Pivot : SubEditorScreen.GetSelectionCenter());
					if (!GameMain.Instance.Paused)
					{
						if (this.transformWidget.IsSelected && this.scaleToolToggle.Selected)
						{
							this.transformWidget.DrawPos = PlayerInput.MousePosition;
						}
						else
						{
							Vector2 dir = this.transformWidget.IsSelected ? Vector2.Normalize(PlayerInput.MousePosition - this.Cam.WorldToScreen(this.transformCommand.Pivot)) : Vector2.UnitX;
							this.transformWidget.DrawPos = selectionCenterScreenPos + dir * 300f * GUI.Scale;
						}
					}
					GUI.DrawLine(sb, selectionCenterScreenPos, this.transformWidget.DrawPos, Color.Black, 0f, 7f);
					GUI.DrawLine(sb, selectionCenterScreenPos, this.transformWidget.DrawPos, Color.Red, 0f, 3f);
				};
				return this.transformWidget;
			}
		}

		// Token: 0x17000A44 RID: 2628
		// (get) Token: 0x0600271A RID: 10010 RVA: 0x001A228E File Offset: 0x001A048E
		// (set) Token: 0x0600271B RID: 10011 RVA: 0x001A2296 File Offset: 0x001A0496
		public bool ShowThalamus { get; private set; } = true;

		// Token: 0x17000A45 RID: 2629
		// (get) Token: 0x0600271C RID: 10012 RVA: 0x001A229F File Offset: 0x001A049F
		public GUIButton ToggleEntityMenuButton
		{
			get
			{
				return this.toggleEntityMenuButton;
			}
		}

		// Token: 0x17000A46 RID: 2630
		// (get) Token: 0x0600271D RID: 10013 RVA: 0x001A22A7 File Offset: 0x001A04A7
		private static int MaxAutoSaves
		{
			get
			{
				return GameSettings.CurrentConfig.MaxAutoSaves;
			}
		}

		// Token: 0x17000A47 RID: 2631
		// (get) Token: 0x0600271E RID: 10014 RVA: 0x001A22B3 File Offset: 0x001A04B3
		// (set) Token: 0x0600271F RID: 10015 RVA: 0x001A22BA File Offset: 0x001A04BA
		public static object BulkItemBufferInUse
		{
			get
			{
				return SubEditorScreen.bulkItemBufferinUse;
			}
			set
			{
				if (value != SubEditorScreen.bulkItemBufferinUse && SubEditorScreen.bulkItemBufferinUse != null)
				{
					SubEditorScreen.CommitBulkItemBuffer();
				}
				SubEditorScreen.bulkItemBufferinUse = value;
			}
		}

		// Token: 0x17000A48 RID: 2632
		// (get) Token: 0x06002720 RID: 10016 RVA: 0x001A22D6 File Offset: 0x001A04D6
		public override Camera Cam
		{
			get
			{
				return this.cam;
			}
		}

		// Token: 0x17000A49 RID: 2633
		// (get) Token: 0x06002721 RID: 10017 RVA: 0x001A22DE File Offset: 0x001A04DE
		public bool DrawCharacterInventory
		{
			get
			{
				return this.dummyCharacter != null && this.WiringMode;
			}
		}

		// Token: 0x06002722 RID: 10018 RVA: 0x001A22F0 File Offset: 0x001A04F0
		private static string GetSubDescription()
		{
			Submarine mainSub = SubEditorScreen.MainSub;
			if (((mainSub != null) ? mainSub.Info : null) == null)
			{
				return "";
			}
			LocalizedString localizedDescription = TextManager.Get("submarine.description." + SubEditorScreen.MainSub.Info.Name);
			if (!localizedDescription.IsNullOrEmpty())
			{
				return localizedDescription.Value;
			}
			LocalizedString description = SubEditorScreen.MainSub.Info.Description;
			return ((description != null) ? description.Value : null) ?? "";
		}

		// Token: 0x06002723 RID: 10019 RVA: 0x001A2368 File Offset: 0x001A0568
		private static LocalizedString GetTotalHullVolume()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("TotalHullVolume"));
			defaultInterpolatedStringHandler.AppendLiteral(":\n");
			defaultInterpolatedStringHandler.AppendFormatted<float>(Hull.HullList.Sum((Hull h) => h.Volume));
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x06002724 RID: 10020 RVA: 0x001A23D8 File Offset: 0x001A05D8
		private static LocalizedString GetSelectedHullVolume()
		{
			float buoyancyVol = 0f;
			float selectedVol = 0f;
			float neutralPercentage = 0.07f;
			Hull.HullList.ForEach(delegate(Hull h)
			{
				buoyancyVol += h.Volume;
				if (h.IsSelected)
				{
					selectedVol += h.Volume;
				}
			});
			buoyancyVol *= neutralPercentage;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("SelectedHullVolume"));
			defaultInterpolatedStringHandler.AppendLiteral(":\n");
			defaultInterpolatedStringHandler.AppendFormatted<float>(selectedVol);
			string retVal = defaultInterpolatedStringHandler.ToStringAndClear();
			if (selectedVol > 0f && buoyancyVol > 0f)
			{
				if (buoyancyVol / selectedVol < 1f)
				{
					string str = retVal;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 1);
					defaultInterpolatedStringHandler2.AppendLiteral(" (");
					defaultInterpolatedStringHandler2.AppendFormatted<LocalizedString>(TextManager.GetWithVariable("OptimalBallastLevel", "[value]", (buoyancyVol / selectedVol).ToString("0.0000"), FormatCapitals.No));
					defaultInterpolatedStringHandler2.AppendLiteral(")");
					retVal = str + defaultInterpolatedStringHandler2.ToStringAndClear();
				}
				else
				{
					string str2 = retVal;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(3, 1);
					defaultInterpolatedStringHandler3.AppendLiteral(" (");
					defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(TextManager.Get("InsufficientBallast"));
					defaultInterpolatedStringHandler3.AppendLiteral(")");
					retVal = str2 + defaultInterpolatedStringHandler3.ToStringAndClear();
				}
			}
			return retVal;
		}

		// Token: 0x17000A4A RID: 2634
		// (get) Token: 0x06002725 RID: 10021 RVA: 0x001A2548 File Offset: 0x001A0748
		public bool WiringMode
		{
			get
			{
				return this.mode == SubEditorScreen.Mode.Wiring;
			}
		}

		// Token: 0x06002726 RID: 10022 RVA: 0x001A2554 File Offset: 0x001A0754
		public SubEditorScreen()
		{
			this.cam = new Camera
			{
				MaxZoom = 10f
			};
			WayPoint.ShowWayPoints = false;
			WayPoint.ShowSpawnPoints = false;
			Hull.ShowHulls = false;
			Gap.ShowGaps = false;
			this.CreateUI();
		}

		// Token: 0x06002727 RID: 10023 RVA: 0x001A2618 File Offset: 0x001A0818
		private void CreateUI()
		{
			this.TopPanel = new GUIFrame(new RectTransform(new Vector2(GUI.Canvas.RelativeSize.X, 0.01f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 35)
			}, "GUIFrameTop", null);
			GUILayoutGroup paddedTopPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.8f), this.TopPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				RelativeSpacing = 0.005f
			};
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIButtonToggleLeft", null);
			guibutton.ToolTip = TextManager.Get("back");
			guibutton.OnClicked = delegate(GUIButton b, object d)
			{
				GUIMessageBox msgBox = new GUIMessageBox("", TextManager.Get("PauseMenuQuitVerificationEditor"), new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("Cancel")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false)
				{
					UserData = "verificationprompt"
				};
				msgBox.Buttons[0].OnClicked = delegate(GUIButton yesBtn, object userdata)
				{
					GUIMessageBox.CloseAll();
					GameMain.MainMenuScreen.Select();
					return true;
				};
				GUIButton guibutton12 = msgBox.Buttons[0];
				guibutton12.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton12.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
				msgBox.Buttons[1].OnClicked = delegate(GUIButton _, object userdata)
				{
					msgBox.Close();
					return true;
				};
				return true;
			};
			new GUIFrame(new RectTransform(new Vector2(0.01f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "OpenButton", null);
			guibutton2.ToolTip = TextManager.Get("OpenSubButton");
			guibutton2.OnClicked = delegate(GUIButton btn, object data)
			{
				this.saveFrame = null;
				this.CreateLoadScreen();
				return true;
			};
			new GUIFrame(new RectTransform(new Vector2(0.01f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			GUIButton guibutton3 = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "SaveButton", null);
			guibutton3.ToolTip = RichString.Rich(TextManager.Get("SaveSubButton") + "‖color:125,125,125‖\nCtrl + S‖color:end‖", null);
			guibutton3.OnClicked = delegate(GUIButton btn, object data)
			{
				this.loadFrame = null;
				this.CreateSaveScreen(false);
				return true;
			};
			new GUIFrame(new RectTransform(new Vector2(0.01f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			GUIButton guibutton4 = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "TestButton", null);
			guibutton4.ToolTip = TextManager.Get("TestSubButton");
			guibutton4.OnClicked = new GUIButton.OnClickedHandler(this.TestSubmarine);
			new GUIFrame(new RectTransform(new Vector2(0.01f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			this.visibilityButton = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "SetupVisibilityButton", null)
			{
				ToolTip = TextManager.Get("SubEditorVisibilityButton") + '\n' + TextManager.Get("SubEditorVisibilityToolTip"),
				OnClicked = delegate(GUIButton btn, object userData)
				{
					this.previouslyUsedPanel.Visible = false;
					this.undoBufferPanel.Visible = false;
					this.layerPanel.Visible = false;
					this.showEntitiesPanel.Visible = !this.showEntitiesPanel.Visible;
					this.showEntitiesPanel.RectTransform.AbsoluteOffset = new Point(Math.Max(Math.Max(btn.Rect.X, this.entityCountPanel.Rect.Right), this.saveAssemblyFrame.Rect.Right), this.TopPanel.Rect.Height);
					return true;
				}
			};
			GUIButton guibutton5 = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "EditorLayerButton", null);
			guibutton5.ToolTip = TextManager.Get("editor.layer.button") + '\n' + TextManager.Get("editor.layer.tooltip");
			guibutton5.OnClicked = delegate(GUIButton btn, object userData)
			{
				this.previouslyUsedPanel.Visible = false;
				this.showEntitiesPanel.Visible = false;
				this.undoBufferPanel.Visible = false;
				this.layerPanel.Visible = !this.layerPanel.Visible;
				this.layerPanel.RectTransform.AbsoluteOffset = new Point(Math.Max(Math.Max(btn.Rect.X, this.entityCountPanel.Rect.Right), this.saveAssemblyFrame.Rect.Right), this.TopPanel.Rect.Height);
				return true;
			};
			GUIButton guibutton6 = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "RecentlyUsedButton", null);
			guibutton6.ToolTip = TextManager.Get("PreviouslyUsedLabel");
			guibutton6.OnClicked = delegate(GUIButton btn, object userData)
			{
				this.showEntitiesPanel.Visible = false;
				this.undoBufferPanel.Visible = false;
				this.layerPanel.Visible = false;
				this.previouslyUsedPanel.Visible = !this.previouslyUsedPanel.Visible;
				this.previouslyUsedPanel.RectTransform.AbsoluteOffset = new Point(Math.Max(Math.Max(btn.Rect.X, this.entityCountPanel.Rect.Right), this.saveAssemblyFrame.Rect.Right), this.TopPanel.Rect.Height);
				return true;
			};
			GUIButton guibutton7 = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "UndoHistoryButton", null);
			guibutton7.ToolTip = TextManager.Get("Editor.UndoHistoryButton");
			guibutton7.OnClicked = delegate(GUIButton btn, object userData)
			{
				this.showEntitiesPanel.Visible = false;
				this.previouslyUsedPanel.Visible = false;
				this.layerPanel.Visible = false;
				this.undoBufferPanel.Visible = !this.undoBufferPanel.Visible;
				this.undoBufferPanel.RectTransform.AbsoluteOffset = new Point(Math.Max(Math.Max(btn.Rect.X, this.entityCountPanel.Rect.Right), this.saveAssemblyFrame.Rect.Right), this.TopPanel.Rect.Height);
				return true;
			};
			new GUIFrame(new RectTransform(new Vector2(0.01f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			RectTransform rectT = new RectTransform(new Vector2(0.3f, 0.9f), paddedTopPanel.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
			RichString text14 = TextManager.Get("unspecifiedsubfilename");
			GUIFont font = GUIStyle.LargeFont;
			this.subNameLabel = new GUITextBlock(rectT, text14, null, font, Alignment.CenterLeft, false, "", null);
			this.linkedSubBox = new GUIDropDown(new RectTransform(new Vector2(0.15f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("AddSubButton"), 20, "", false, false, Alignment.CenterLeft, 1f)
			{
				ToolTip = TextManager.Get("AddSubToolTip")
			};
			List<ValueTuple<string, SubmarineInfo>> subs = new List<ValueTuple<string, SubmarineInfo>>();
			foreach (SubmarineInfo sub in SubmarineInfo.SavedSubmarines)
			{
				if (sub.Type == SubmarineType.Player)
				{
					subs.Add(new ValueTuple<string, SubmarineInfo>(sub.Name, sub));
				}
			}
			foreach (ValueTuple<string, SubmarineInfo> valueTuple in from tuple in subs
			orderby tuple.Item1
			select tuple)
			{
				string name = valueTuple.Item1;
				SubmarineInfo sub2 = valueTuple.Item2;
				this.linkedSubBox.AddItem(name, sub2, null, null, null);
			}
			GUIDropDown guidropDown = this.linkedSubBox;
			guidropDown.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(guidropDown.OnSelected, new GUIDropDown.OnSelectedHandler(this.SelectLinkedSub));
			GUIDropDown guidropDown2 = this.linkedSubBox;
			guidropDown2.OnDropped = (GUIDropDown.OnSelectedHandler)Delegate.Combine(guidropDown2.OnDropped, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent component, object obj)
			{
				MapEntity.SelectedList.Clear();
				return true;
			}));
			GUIFrame spacing = new GUIFrame(new RectTransform(new Vector2(0.02f, 1f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIFrame(new RectTransform(new Vector2(0.1f, 0.9f), spacing.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			this.defaultModeTickBox = new GUITickBox(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", null, "EditSubButton")
			{
				ToolTip = RichString.Rich(TextManager.Get("SubEditorEditingMode") + "‖color:125,125,125‖\nCtrl + 1‖color:end‖", null),
				OnSelected = delegate(GUITickBox tBox)
				{
					if (!this.lockMode)
					{
						if (tBox.Selected)
						{
							this.SetMode(SubEditorScreen.Mode.Default);
						}
						return true;
					}
					return false;
				}
			};
			this.wiringModeTickBox = new GUITickBox(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", null, "WiringModeButton")
			{
				ToolTip = RichString.Rich(TextManager.Get("WiringModeButton") + '\n' + TextManager.Get("WiringModeToolTip") + "‖color:125,125,125‖\nCtrl + 2‖color:end‖", null),
				OnSelected = delegate(GUITickBox tBox)
				{
					if (!this.lockMode)
					{
						this.SetMode(tBox.Selected ? SubEditorScreen.Mode.Wiring : SubEditorScreen.Mode.Default);
						return true;
					}
					return false;
				}
			};
			spacing = new GUIFrame(new RectTransform(new Vector2(0.02f, 1f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIFrame(new RectTransform(new Vector2(0.1f, 0.9f), spacing.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			GUIButton guibutton8 = new GUIButton(new RectTransform(new Vector2(0.9f, 0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "GenerateWaypointsButton", null);
			guibutton8.ToolTip = TextManager.Get("GenerateWaypointsButton") + '\n' + TextManager.Get("GenerateWaypointsToolTip");
			GUITickBox tb;
			guibutton8.OnClicked = delegate(GUIButton btn, object userdata)
			{
				if (WayPoint.WayPointList.Any<WayPoint>())
				{
					GUIMessageBox generateWaypointsVerification = new GUIMessageBox("", TextManager.Get("generatewaypointsverification"), new LocalizedString[]
					{
						TextManager.Get("ok"),
						TextManager.Get("cancel")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					generateWaypointsVerification.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
					{
						if (this.GenerateWaypoints())
						{
							GUI.AddMessage(TextManager.Get("waypointsgeneratedsuccesfully"), GUIStyle.Green, null, true, null);
						}
						WayPoint.ShowWayPoints = true;
						List<GUITickBox> list2 = this.showEntitiesTickBoxes;
						GUITickBox guitickBox12;
						if (list2 == null)
						{
							guitickBox12 = null;
						}
						else
						{
							guitickBox12 = list2.Find((GUITickBox tb) => tb.UserData as string == "waypoint");
						}
						GUITickBox matchingTickBox = guitickBox12;
						if (matchingTickBox != null)
						{
							matchingTickBox.Selected = true;
						}
						generateWaypointsVerification.Close();
						return true;
					};
					generateWaypointsVerification.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(generateWaypointsVerification.Close);
				}
				else
				{
					if (this.GenerateWaypoints())
					{
						GUI.AddMessage(TextManager.Get("waypointsgeneratedsuccesfully"), GUIStyle.Green, null, true, null);
					}
					WayPoint.ShowWayPoints = true;
				}
				return true;
			};
			spacing = new GUIFrame(new RectTransform(new Vector2(0.02f, 1f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIFrame(new RectTransform(new Vector2(0.1f, 0.9f), spacing.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "VerticalLine", null);
			this.rotateToolToggle = new GUITickBox(new RectTransform(new Vector2(0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", null, "SubEditorRotateToggle")
			{
				ToolTip = TextManager.Get("SubEditor.RotateToggleToolTip")
			};
			this.scaleToolToggle = new GUITickBox(new RectTransform(new Vector2(0.9f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", null, "SubEditorScaleToggle")
			{
				ToolTip = TextManager.Get("SubEditor.ScaleToggleToolTip")
			};
			GUITextBlock selectedLayerText = new GUITextBlock(new RectTransform(new Vector2(0.15f, 1f), paddedTopPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Center, false, "", null);
			selectedLayerText.TextGetter = delegate()
			{
				string selectedLayer = this.layerList.SelectedData as string;
				if (!(selectedLayer != this.prevSelectedLayer))
				{
					return selectedLayerText.Text;
				}
				this.prevSelectedLayer = selectedLayer;
				if (!selectedLayer.IsNullOrEmpty())
				{
					return TextManager.GetWithVariable("editor.layer.editinglayer", "[layer]", selectedLayer, FormatCapitals.No);
				}
				return string.Empty;
			};
			this.TopPanel.RectTransform.MinSize = new Point(0, (int)((float)paddedTopPanel.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y) / paddedTopPanel.RectTransform.RelativeSize.Y));
			paddedTopPanel.Recalculate();
			this.previouslyUsedPanel = new GUIFrame(new RectTransform(new Vector2(0.1f, 0.2f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(200, 200)
			}, "", null)
			{
				Visible = false
			};
			Vector2 relativeSize = new Vector2(0.9f, 0.9f);
			RectTransform rectTransform = this.previouslyUsedPanel.RectTransform;
			Anchor anchor = Anchor.Center;
			Pivot? pivot = null;
			Point? minSize = null;
			Point? point = null;
			this.previouslyUsedList = new GUIListBox(new RectTransform(relativeSize, rectTransform, anchor, pivot, minSize, point, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				ScrollBarVisible = true,
				OnSelected = new GUIListBox.OnSelectedHandler(this.SelectPrefab)
			};
			Vector2 relativeSize2 = new Vector2(0.25f, 0.4f);
			RectTransform canvas = GUI.Canvas;
			Anchor anchor2 = Anchor.TopLeft;
			point = new Point?(new Point(300, 320));
			this.layerPanel = new GUIFrame(new RectTransform(relativeSize2, canvas, anchor2, null, point, null, ScaleBasis.Normal), "", null)
			{
				Visible = false
			};
			GUILayoutGroup layerGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.9f), this.layerPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			this.layerList = new GUIListBox(new RectTransform(new Vector2(1f, 0.8f), layerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				ScrollBarVisible = true,
				AutoHideScrollBar = false,
				OnSelected = delegate(GUIComponent component, object userdata)
				{
					if (GUI.MouseOn is GUITickBox)
					{
						return false;
					}
					SoundPlayer.PlayUISound(GUISoundType.Select);
					if (this.layerList.SelectedData == userdata)
					{
						this.layerSpecificButtons.ForEach(delegate(GUIButton btn)
						{
							btn.Enabled = false;
						});
						this.layerList.Deselect();
						return false;
					}
					this.layerSpecificButtons.ForEach(delegate(GUIButton btn)
					{
						btn.Enabled = true;
					});
					return true;
				}
			};
			GUILayoutGroup layerButtonGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), layerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup layerButtonTopGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), layerButtonGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup layerButtonBottomGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), layerButtonGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIButton layerAddButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), layerButtonTopGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.layer.newlayer"), Alignment.Center, "GUIButtonFreeScale", null)
			{
				OnClicked = delegate(GUIButton button, object o)
				{
					this.CreateNewLayer(null, MapEntity.SelectedList.ToList<MapEntity>());
					return true;
				}
			};
			GUIButton layerDeleteButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), layerButtonTopGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.layer.deletelayer"), Alignment.Center, "GUIButtonFreeScale", null)
			{
				Enabled = false,
				OnClicked = delegate(GUIButton button, object o)
				{
					string layer = this.layerList.SelectedData as string;
					if (layer != null)
					{
						this.RenameLayer(layer, null);
					}
					return true;
				}
			};
			this.layerSpecificButtons.Add(layerDeleteButton);
			GUIButton layerRenameButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), layerButtonBottomGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.layer.renamelayer"), Alignment.Center, "GUIButtonFreeScale", null)
			{
				Enabled = false,
				OnClicked = delegate(GUIButton button, object o)
				{
					object selectedData = this.layerList.SelectedData;
					string layer = selectedData as string;
					if (layer != null)
					{
						GUI.PromptTextInput(TextManager.Get("editor.layer.renamelayer"), layer, delegate(string newName)
						{
							this.RenameLayer(layer, newName);
						});
					}
					return true;
				}
			};
			this.layerSpecificButtons.Add(layerRenameButton);
			GUIButton selectLayerButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), layerButtonBottomGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.layer.selectlayer"), Alignment.Center, "GUIButtonFreeScale", null)
			{
				Enabled = false,
				OnClicked = delegate(GUIButton button, object o)
				{
					object selectedData = this.layerList.SelectedData;
					string layer = selectedData as string;
					if (layer != null)
					{
						IEnumerable<MapEntity> mapEntityList = MapEntity.MapEntityList;
						Func<MapEntity, bool> <>9__42;
						Func<MapEntity, bool> predicate;
						if ((predicate = <>9__42) == null)
						{
							predicate = (<>9__42 = ((MapEntity me) => !me.Removed && me.Layer == layer));
						}
						foreach (MapEntity entity in mapEntityList.Where(predicate))
						{
							if (!entity.IsSelected)
							{
								MapEntity.SelectedList.Add(entity);
							}
						}
					}
					return true;
				}
			};
			this.layerSpecificButtons.Add(selectLayerButton);
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				layerAddButton.TextBlock,
				layerDeleteButton.TextBlock,
				layerRenameButton.TextBlock,
				selectLayerButton.TextBlock
			});
			Vector2 subPanelSize = new Vector2(0.925f, 0.9f);
			this.undoBufferPanel = new GUIFrame(new RectTransform(new Vector2(0.15f, 0.2f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(200, 200)
			}, "", null)
			{
				Visible = false
			};
			GUIListBox guilistBox = new GUIListBox(new RectTransform(subPanelSize, this.undoBufferPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			guilistBox.PlaySoundOnSelect = true;
			guilistBox.ScrollBarVisible = true;
			guilistBox.OnSelected = delegate(GUIComponent _, object userData)
			{
				Command command = userData as Command;
				int index;
				if (command != null)
				{
					index = SubEditorScreen.Commands.IndexOf(command);
				}
				else
				{
					index = -1;
				}
				int diff = index - SubEditorScreen.commandIndex;
				int amount = Math.Abs(diff);
				if (diff >= 0)
				{
					SubEditorScreen.Redo(amount + 1);
				}
				else
				{
					SubEditorScreen.Undo(amount - 1);
				}
				return true;
			};
			this.undoBufferList = guilistBox;
			this.undoBufferDisclaimer = new GUIFrame(new RectTransform(subPanelSize, this.undoBufferPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null)
			{
				Color = Color.Black,
				Visible = false
			};
			RectTransform rectT2 = new RectTransform(Vector2.One, this.undoBufferDisclaimer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("editor.undounavailable");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.Center, true, "", null).TextColor = GUIStyle.Orange;
			this.UpdateUndoHistoryPanel();
			this.showEntitiesPanel = new GUIFrame(new RectTransform(new Vector2(0.15f, 0.5f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(190, 0)
			}, "", null)
			{
				Visible = false
			};
			GUILayoutGroup paddedShowEntitiesPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.98f), this.showEntitiesPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowLighting"), null, "");
			guitickBox.UserData = "lighting";
			guitickBox.Selected = this.lightingEnabled;
			guitickBox.OnSelected = delegate(GUITickBox obj)
			{
				this.lightingEnabled = obj.Selected;
				return true;
			};
			GUITickBox guitickBox2 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowWalls"), null, "");
			guitickBox2.UserData = "wall";
			guitickBox2.Selected = Structure.ShowWalls;
			guitickBox2.OnSelected = delegate(GUITickBox obj)
			{
				Structure.ShowWalls = obj.Selected;
				return true;
			};
			GUITickBox guitickBox3 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowStructures"), null, "");
			guitickBox3.UserData = "structure";
			guitickBox3.Selected = Structure.ShowStructures;
			guitickBox3.OnSelected = delegate(GUITickBox obj)
			{
				Structure.ShowStructures = obj.Selected;
				return true;
			};
			GUITickBox guitickBox4 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowItems"), null, "");
			guitickBox4.UserData = "item";
			guitickBox4.Selected = Item.ShowItems;
			guitickBox4.OnSelected = delegate(GUITickBox obj)
			{
				Item.ShowItems = obj.Selected;
				return true;
			};
			GUITickBox guitickBox5 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowWires"), null, "");
			guitickBox5.UserData = "wire";
			guitickBox5.Selected = Item.ShowWires;
			guitickBox5.OnSelected = delegate(GUITickBox obj)
			{
				Item.ShowWires = obj.Selected;
				return true;
			};
			GUITickBox guitickBox6 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowWaypoints"), null, "");
			guitickBox6.UserData = "waypoint";
			guitickBox6.Selected = WayPoint.ShowWayPoints;
			guitickBox6.OnSelected = delegate(GUITickBox obj)
			{
				WayPoint.ShowWayPoints = obj.Selected;
				return true;
			};
			GUITickBox guitickBox7 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowSpawnpoints"), null, "");
			guitickBox7.UserData = "spawnpoint";
			guitickBox7.Selected = WayPoint.ShowSpawnPoints;
			guitickBox7.OnSelected = delegate(GUITickBox obj)
			{
				WayPoint.ShowSpawnPoints = obj.Selected;
				return true;
			};
			GUITickBox guitickBox8 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowLinks"), null, "");
			guitickBox8.UserData = "link";
			guitickBox8.Selected = Item.ShowLinks;
			guitickBox8.OnSelected = delegate(GUITickBox obj)
			{
				Item.ShowLinks = obj.Selected;
				return true;
			};
			GUITickBox guitickBox9 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowHulls"), null, "");
			guitickBox9.UserData = "hull";
			guitickBox9.Selected = Hull.ShowHulls;
			guitickBox9.OnSelected = delegate(GUITickBox obj)
			{
				Hull.ShowHulls = obj.Selected;
				return true;
			};
			GUITickBox guitickBox10 = new GUITickBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ShowGaps"), null, "");
			guitickBox10.UserData = "gap";
			guitickBox10.Selected = Gap.ShowGaps;
			guitickBox10.OnSelected = delegate(GUITickBox obj)
			{
				Gap.ShowGaps = obj.Selected;
				return true;
			};
			this.showEntitiesTickBoxes.AddRange(from c in paddedShowEntitiesPanel.Children
			select c as GUITickBox);
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("subcategories");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock subcategoryHeader = new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
			subcategoryHeader.RectTransform.MinSize = new Point(0, (int)((float)subcategoryHeader.Rect.Height * 1.5f));
			GUIListBox subcategoryList = new GUIListBox(new RectTransform(new Vector2(1f, 0.1f), paddedShowEntitiesPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, this.showEntitiesPanel.Rect.Height / 3)
			}, false, null, "", true, false);
			List<string> availableSubcategories = new List<string>();
			foreach (MapEntityPrefab prefab in MapEntityPrefab.List)
			{
				if (!string.IsNullOrEmpty(prefab.Subcategory) && !availableSubcategories.Contains(prefab.Subcategory))
				{
					availableSubcategories.Add(prefab.Subcategory);
				}
			}
			GUITickBox.OnSelectedHandler <>9__54;
			foreach (string subcategory in availableSubcategories)
			{
				GUITickBox guitickBox11 = new GUITickBox(new RectTransform(new Vector2(1f, 0.15f), subcategoryList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("subcategory." + subcategory).Fallback(subcategory, true), GUIStyle.SmallFont, "");
				guitickBox11.UserData = subcategory;
				guitickBox11.Selected = !this.IsSubcategoryHidden(subcategory);
				GUITickBox.OnSelectedHandler onSelected;
				if ((onSelected = <>9__54) == null)
				{
					onSelected = (<>9__54 = delegate(GUITickBox obj)
					{
						this.hiddenSubCategories[(string)obj.UserData] = !obj.Selected;
						return true;
					});
				}
				guitickBox11.OnSelected = onSelected;
				tb = guitickBox11;
				tb.TextBlock.Wrap = true;
			}
			GUITextBlock.AutoScaleAndNormalize(from c in subcategoryList.Content.Children
			where c is GUITickBox
			select ((GUITickBox)c).TextBlock, true, false, null);
			foreach (GUIComponent child in subcategoryList.Content.Children)
			{
				GUITickBox tb2 = child as GUITickBox;
				if (tb2 != null && tb2.TextBlock.TextSize.X > (float)tb2.TextBlock.Rect.Width * 1.25f)
				{
					tb2.ToolTip = tb2.Text;
					tb2.Text = ToolBox.LimitString(tb2.Text.Value, tb2.Font, (int)((float)tb2.TextBlock.Rect.Width * 1.25f));
				}
			}
			this.showEntitiesPanel.RectTransform.NonScaledSize = new Point((int)Math.Max((float)this.showEntitiesPanel.RectTransform.NonScaledSize.X, (float)paddedShowEntitiesPanel.RectTransform.Children.Max(delegate(RectTransform c)
			{
				GUITickBox guitickBox12 = c.GUIComponent as GUITickBox;
				return (int)((guitickBox12 != null) ? guitickBox12.TextBlock.TextSize.X : 0f);
			}) / paddedShowEntitiesPanel.RectTransform.RelativeSize.X), (int)((float)paddedShowEntitiesPanel.RectTransform.Children.Sum((RectTransform c) => c.MinSize.Y) / paddedShowEntitiesPanel.RectTransform.RelativeSize.Y));
			GUITextBlock.AutoScaleAndNormalize(from c in paddedShowEntitiesPanel.Children
			where c is GUITickBox
			select ((GUITickBox)c).TextBlock, true, false, null);
			float longestTextWidth = GUIStyle.SmallFont.MeasureString(TextManager.Get("SubEditorShadowCastingLights"), false).X;
			this.entityCountPanel = new GUIFrame(new RectTransform(new Vector2(0.08f, 0.5f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(Math.Max(170, (int)(longestTextWidth * 1.5f)), 0),
				AbsoluteOffset = new Point(0, this.TopPanel.Rect.Height)
			}, "", null);
			GUILayoutGroup paddedEntityCountPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), this.entityCountPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = (int)(GUI.Scale * 4f)
			};
			RectTransform rectT4 = new RectTransform(new Vector2(0.75f, 0f), paddedEntityCountPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = TextManager.Get("Items");
			font = GUIStyle.SmallFont;
			GUITextBlock itemCountText = new GUITextBlock(rectT4, text4, null, font, Alignment.CenterLeft, false, "", null);
			GUITextBlock itemCount = new GUITextBlock(new RectTransform(new Vector2(0.33f, 1f), itemCountText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), "", null, null, Alignment.CenterRight, false, "", null);
			itemCount.TextGetter = delegate()
			{
				int count = Item.ItemList.Count;
				Character character = this.dummyCharacter;
				if (((character != null) ? character.Inventory : null) != null)
				{
					count -= this.dummyCharacter.Inventory.AllItems.Count<Item>();
				}
				itemCount.TextColor = ((count > 5000) ? GUIStyle.Red : Color.Lerp(GUIStyle.Green, GUIStyle.Orange, (float)count / 5000f));
				return count.ToString();
			};
			RectTransform rectT5 = new RectTransform(new Vector2(0.75f, 0f), paddedEntityCountPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text5 = TextManager.Get("Structures");
			font = GUIStyle.SmallFont;
			GUITextBlock structureCountText = new GUITextBlock(rectT5, text5, null, font, Alignment.CenterLeft, false, "", null);
			GUITextBlock structureCount = new GUITextBlock(new RectTransform(new Vector2(0.33f, 1f), structureCountText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), "", null, null, Alignment.CenterRight, false, "", null);
			structureCount.TextGetter = delegate()
			{
				int count = MapEntity.MapEntityList.Count - Item.ItemList.Count - Hull.HullList.Count - WayPoint.WayPointList.Count - Gap.GapList.Count;
				structureCount.TextColor = ((count > 2000) ? GUIStyle.Red : Color.Lerp(GUIStyle.Green, GUIStyle.Orange, (float)count / 2000f));
				return count.ToString();
			};
			RectTransform rectT6 = new RectTransform(new Vector2(0.75f, 0f), paddedEntityCountPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = TextManager.Get("Walls");
			font = GUIStyle.SmallFont;
			GUITextBlock wallCountText = new GUITextBlock(rectT6, text6, null, font, Alignment.CenterLeft, false, "", null);
			GUITextBlock wallCount = new GUITextBlock(new RectTransform(new Vector2(0.33f, 1f), wallCountText.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), "", null, null, Alignment.CenterRight, false, "", null);
			wallCount.TextGetter = delegate()
			{
				wallCount.TextColor = ((Structure.WallList.Count > 500) ? GUIStyle.Red : Color.Lerp(GUIStyle.Green, GUIStyle.Orange, (float)Structure.WallList.Count / 500f));
				return Structure.WallList.Count.ToString();
			};
			RectTransform rectT7 = new RectTransform(new Vector2(0.75f, 0f), paddedEntityCountPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text7 = TextManager.Get("SubEditorLights");
			font = GUIStyle.SmallFont;
			GUITextBlock lightCountLabel = new GUITextBlock(rectT7, text7, null, font, Alignment.CenterLeft, false, "", null);
			GUITextBlock lightCountText = new GUITextBlock(new RectTransform(new Vector2(0.33f, 1f), lightCountLabel.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), "", null, null, Alignment.CenterRight, false, "", null);
			lightCountText.TextGetter = delegate()
			{
				int lightCount = 0;
				foreach (Item item in Item.ItemList)
				{
					if (item.ParentInventory == null)
					{
						lightCount += item.GetComponents<LightComponent>().Count<LightComponent>();
					}
				}
				lightCountText.TextColor = ((lightCount > 600) ? GUIStyle.Red : Color.Lerp(GUIStyle.Green, GUIStyle.Orange, (float)lightCount / 600f));
				return lightCount.ToString() + "/" + 600.ToString();
			};
			RectTransform rectT8 = new RectTransform(new Vector2(0.75f, 0f), paddedEntityCountPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text8 = TextManager.Get("SubEditorShadowCastingLights");
			font = GUIStyle.SmallFont;
			GUITextBlock shadowCastingLightCountLabel = new GUITextBlock(rectT8, text8, null, font, Alignment.CenterLeft, true, "", null);
			GUITextBlock shadowCastingLightCountText = new GUITextBlock(new RectTransform(new Vector2(0.33f, 1f), shadowCastingLightCountLabel.RectTransform, Anchor.TopRight, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal), "", null, null, Alignment.CenterRight, false, "", null);
			shadowCastingLightCountText.TextGetter = delegate()
			{
				int lightCount = 0;
				foreach (Item item in Item.ItemList)
				{
					if (item.ParentInventory == null)
					{
						lightCount += item.GetComponents<LightComponent>().Count((LightComponent l) => l.CastShadows && !l.DrawBehindSubs);
					}
				}
				shadowCastingLightCountText.TextColor = ((lightCount > 100) ? GUIStyle.Red : Color.Lerp(GUIStyle.Green, GUIStyle.Orange, (float)lightCount / 100f));
				return lightCount.ToString() + "/" + 100.ToString();
			};
			this.entityCountPanel.RectTransform.NonScaledSize = new Point((int)(paddedEntityCountPanel.RectTransform.Children.Max((RectTransform c) => (float)((int)((GUITextBlock)c.GUIComponent).TextSize.X) / 0.75f) / paddedEntityCountPanel.RectTransform.RelativeSize.X), (int)((float)paddedEntityCountPanel.RectTransform.Children.Sum((RectTransform c) => (int)((float)c.NonScaledSize.Y * 1.5f) + paddedEntityCountPanel.AbsoluteSpacing) / paddedEntityCountPanel.RectTransform.RelativeSize.Y));
			this.hullVolumeFrame = new GUIFrame(new RectTransform(new Vector2(0.15f, 2f), this.TopPanel.RectTransform, Anchor.BottomLeft, new Pivot?(Pivot.TopLeft), new Point?(new Point(300, 85)), null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(this.entityCountPanel.Rect.Width, 0)
			}, "GUIToolTip", null)
			{
				Visible = false
			};
			RectTransform rectT9 = new RectTransform(new Vector2(1f, 0.5f), this.hullVolumeFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text9 = "";
			font = GUIStyle.SmallFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT9, text9, null, font, Alignment.Left, false, "", null);
			GUITextBlock guitextBlock2 = guitextBlock;
			GUITextBlock.TextGetterHandler textGetter;
			if ((textGetter = SubEditorScreen.<>O.<0>__GetTotalHullVolume) == null)
			{
				textGetter = (SubEditorScreen.<>O.<0>__GetTotalHullVolume = new GUITextBlock.TextGetterHandler(SubEditorScreen.GetTotalHullVolume));
			}
			guitextBlock2.TextGetter = textGetter;
			RectTransform rectTransform2 = new RectTransform(new Vector2(1f, 0.5f), this.hullVolumeFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform2.RelativeOffset = new Vector2(0f, 0.5f);
			RichString text10 = "";
			font = GUIStyle.SmallFont;
			guitextBlock = new GUITextBlock(rectTransform2, text10, null, font, Alignment.Left, false, "", null);
			GUITextBlock guitextBlock3 = guitextBlock;
			GUITextBlock.TextGetterHandler textGetter2;
			if ((textGetter2 = SubEditorScreen.<>O.<1>__GetSelectedHullVolume) == null)
			{
				textGetter2 = (SubEditorScreen.<>O.<1>__GetSelectedHullVolume = new GUITextBlock.TextGetterHandler(SubEditorScreen.GetSelectedHullVolume));
			}
			guitextBlock3.TextGetter = textGetter2;
			this.saveAssemblyFrame = new GUIFrame(new RectTransform(new Vector2(0.08f, 0.5f), this.TopPanel.RectTransform, Anchor.BottomLeft, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal)
			{
				MinSize = new Point((int)(250f * GUI.Scale), (int)(80f * GUI.Scale)),
				AbsoluteOffset = new Point((int)(10f * GUI.Scale), -this.entityCountPanel.Rect.Height - (int)(10f * GUI.Scale))
			}, "InnerFrame", null)
			{
				Visible = false
			};
			GUIButton saveAssemblyButton = new GUIButton(new RectTransform(new Vector2(0.9f, 0.8f), this.saveAssemblyFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("SaveItemAssembly"), Alignment.Center, "", null);
			saveAssemblyButton.TextBlock.AutoScaleHorizontal = true;
			GUIButton guibutton9 = saveAssemblyButton;
			guibutton9.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton9.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
			{
				this.CreateSaveAssemblyScreen();
				return true;
			}));
			this.saveAssemblyFrame.RectTransform.MinSize = new Point(this.saveAssemblyFrame.Rect.Width, (int)((float)saveAssemblyButton.Rect.Height / saveAssemblyButton.RectTransform.RelativeSize.Y));
			this.snapToGridFrame = new GUIFrame(new RectTransform(new Vector2(0.08f, 0.5f), this.TopPanel.RectTransform, Anchor.BottomLeft, new Pivot?(Pivot.TopLeft), null, null, ScaleBasis.Normal)
			{
				MinSize = new Point((int)(250f * GUI.Scale), (int)(80f * GUI.Scale)),
				AbsoluteOffset = new Point((int)(10f * GUI.Scale), -this.saveAssemblyFrame.Rect.Height - this.entityCountPanel.Rect.Height - (int)(10f * GUI.Scale))
			}, "InnerFrame", null)
			{
				Visible = false
			};
			GUIButton saveStampButton = new GUIButton(new RectTransform(new Vector2(0.9f, 0.8f), this.snapToGridFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get(new string[]
			{
				"subeditor.snaptogrid",
				"spriteeditor.snaptogrid"
			}), Alignment.Center, "", null);
			saveStampButton.TextBlock.AutoScaleHorizontal = true;
			GUIButton guibutton10 = saveStampButton;
			guibutton10.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton10.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
			{
				SubEditorScreen.SnapToGrid();
				return true;
			}));
			this.snapToGridFrame.RectTransform.MinSize = new Point(this.snapToGridFrame.Rect.Width, (int)((float)saveStampButton.Rect.Height / saveStampButton.RectTransform.RelativeSize.Y));
			this.EntityMenu = new GUIFrame(new RectTransform(new Point(GameMain.GraphicsWidth, (int)(359f * GUI.Scale)), GUI.Canvas, Anchor.BottomRight, null, ScaleBasis.Normal, false), "", null);
			this.toggleEntityMenuButton = new GUIButton(new RectTransform(new Vector2(0.15f, 0.08f), this.EntityMenu.RectTransform, Anchor.TopCenter, new Pivot?(Pivot.BottomCenter), null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 15)
			}, Alignment.Center, "UIToggleButtonVertical", null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					this.entityMenuOpen = !this.entityMenuOpen;
					this.SetMode(SubEditorScreen.Mode.Default);
					foreach (GUIComponent child2 in btn.Children)
					{
						child2.SpriteEffects = (this.entityMenuOpen ? SpriteEffects.None : SpriteEffects.FlipVertically);
					}
					return true;
				}
			};
			GUILayoutGroup paddedTab = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.96f), this.EntityMenu.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				RelativeSpacing = 0.04f,
				Stretch = true
			};
			GUILayoutGroup entityMenuTop = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.13f), paddedTab.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			this.selectedCategoryButton = new GUIButton(new RectTransform(new Vector2(1f, 1f), entityMenuTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "CategoryButton.All", null)
			{
				CanBeFocused = false
			};
			RectTransform rectT10 = new RectTransform(new Vector2(0.2f, 1f), entityMenuTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text11 = TextManager.Get("MapEntityCategory.All");
			font = GUIStyle.LargeFont;
			this.selectedCategoryText = new GUITextBlock(rectT10, text11, null, font, Alignment.Left, false, "", null);
			RectTransform rectT11 = new RectTransform(new Vector2(0.1f, 1f), entityMenuTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text12 = TextManager.Get("serverlog.filter");
			font = GUIStyle.SubHeadingFont;
			GUITextBlock filterText = new GUITextBlock(rectT11, text12, null, font, Alignment.Left, false, "", null);
			filterText.RectTransform.MaxSize = new Point((int)(filterText.TextSize.X * 1.5f), int.MaxValue);
			RectTransform rectT12 = new RectTransform(new Vector2(0.17f, 1f), entityMenuTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text13 = "";
			font = GUIStyle.Font;
			this.entityFilterBox = new GUITextBox(rectT12, text13, null, font, Alignment.Left, false, "", null, true, true);
			this.entityFilterBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				if (text == this.lastFilter)
				{
					return true;
				}
				this.lastFilter = text;
				this.FilterEntities(text);
				return true;
			};
			new GUIFrame(new RectTransform(new Vector2(0.075f, 1f), entityMenuTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.entityCategoryButtons.Clear();
			this.entityCategoryButtons.Add(new GUIButton(new RectTransform(new Vector2(1f, 1f), entityMenuTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "CategoryButton.All", null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					this.OpenEntityMenu(null);
					return true;
				}
			});
			GUIButton.OnClickedHandler <>9__60;
			foreach (object obj2 in Enum.GetValues(typeof(MapEntityCategory)))
			{
				MapEntityCategory category = (MapEntityCategory)obj2;
				if (category != MapEntityCategory.None)
				{
					List<GUIButton> list = this.entityCategoryButtons;
					GUIButton guibutton11 = new GUIButton(new RectTransform(new Vector2(1f, 1f), entityMenuTop.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), "", Alignment.Center, "CategoryButton." + category.ToString(), null);
					guibutton11.UserData = category;
					guibutton11.ToolTip = TextManager.Get("MapEntityCategory." + category.ToString());
					GUIButton.OnClickedHandler onClicked;
					if ((onClicked = <>9__60) == null)
					{
						onClicked = (<>9__60 = delegate(GUIButton btn, object userdata)
						{
							MapEntityCategory newCategory = (MapEntityCategory)userdata;
							this.OpenEntityMenu(new MapEntityCategory?(newCategory));
							return true;
						});
					}
					guibutton11.OnClicked = onClicked;
					list.Add(guibutton11);
				}
			}
			this.entityCategoryButtons.ForEach(delegate(GUIButton b)
			{
				b.RectTransform.MaxSize = new Point(b.Rect.Height);
			});
			new GUIFrame(new RectTransform(new Vector2(0.8f, 0.01f), paddedTab.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
			GUIFrame entityListContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.9f), paddedTab.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.categorizedEntityList = new GUIListBox(new RectTransform(Vector2.One, entityListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, true);
			GUIListBox guilistBox2 = new GUIListBox(new RectTransform(Vector2.One, entityListContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, true);
			guilistBox2.OnSelected = new GUIListBox.OnSelectedHandler(this.SelectPrefab);
			guilistBox2.UseGridLayout = true;
			GUIListBox guilistBox3 = guilistBox2;
			GUIListBox.CheckSelectedHandler checkSelected;
			if ((checkSelected = SubEditorScreen.<>O.<2>__GetSelected) == null)
			{
				checkSelected = (SubEditorScreen.<>O.<2>__GetSelected = new GUIListBox.CheckSelectedHandler(MapEntityPrefab.GetSelected));
			}
			guilistBox3.CheckSelected = checkSelected;
			guilistBox2.Visible = false;
			guilistBox2.PlaySoundOnSelect = true;
			this.allEntityList = guilistBox2;
			paddedTab.Recalculate();
			this.UpdateLayerPanel();
			this.screenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x06002728 RID: 10024 RVA: 0x001A5908 File Offset: 0x001A3B08
		private bool TestSubmarine(GUIButton button, object obj)
		{
			List<LocalizedString> errorMsgs = new List<LocalizedString>();
			if (!Hull.HullList.Any<Hull>())
			{
				errorMsgs.Add(TextManager.Get("NoHullsWarning"));
			}
			if (!WayPoint.WayPointList.Any((WayPoint wp) => wp.ShouldBeSaved && wp.SpawnType == SpawnType.Human))
			{
				errorMsgs.Add(TextManager.Get("NoHumanSpawnpointWarning"));
			}
			if (errorMsgs.Any<LocalizedString>())
			{
				new GUIMessageBox(TextManager.Get("Error"), LocalizedString.Join("\n\n", errorMsgs), new Vector2?(new Vector2(0.25f, 0f)), new Point?(new Point(400, 200)), GUIMessageBox.Type.Default);
				return true;
			}
			this.CloseItem();
			this.backedUpSubInfo = new SubmarineInfo(SubEditorScreen.MainSub);
			SubmarineInfo submarineInfo = this.backedUpSubInfo;
			Option.UnspecifiedNone none = Option.None;
			GameSession gameSession = new GameSession(submarineInfo, none, CampaignDataPath.Empty, GameModePreset.TestMode, CampaignSettings.Empty, null, null);
			if (this.backedUpSubInfo.OutpostModuleInfo != null)
			{
				gameSession.ForceOutpostModule = new SubmarineInfo(SubEditorScreen.MainSub);
			}
			GameMain.GameScreen.Select();
			gameSession.StartRound(null, false, null, null);
			foreach (KeyValuePair<string, SubEditorScreen.LayerData> keyValuePair in SubEditorScreen.Layers)
			{
				string text;
				SubEditorScreen.LayerData layerData2;
				keyValuePair.Deconstruct(out text, out layerData2);
				string layerName = text;
				SubEditorScreen.LayerData layerData = layerData2;
				Identifier identifier = layerName.ToIdentifier();
				bool enabled = layerData.IsVisible;
				SubEditorScreen.MainSub.SetLayerEnabled(identifier, enabled, false);
			}
			TestGameMode testGameMode = gameSession.GameMode as TestGameMode;
			if (testGameMode != null)
			{
				testGameMode.OnRoundEnd = delegate()
				{
					Submarine.Unload();
					GameMain.SubEditorScreen.Select(true);
					GameMain.GameSession = null;
				};
			}
			return true;
		}

		// Token: 0x06002729 RID: 10025 RVA: 0x001A5ADC File Offset: 0x001A3CDC
		public void ClearBackedUpSubInfo()
		{
			this.backedUpSubInfo = null;
		}

		// Token: 0x0600272A RID: 10026 RVA: 0x001A5AE8 File Offset: 0x001A3CE8
		private void UpdateEntityList()
		{
			this.categorizedEntityList.Content.ClearChildren();
			this.allEntityList.Content.ClearChildren();
			int maxTextWidth = (int)(GUIStyle.SubHeadingFont.MeasureString(TextManager.Get("mapentitycategory.misc"), false).X + (float)GUI.IntScale(50f));
			Dictionary<string, List<MapEntityPrefab>> entityLists = new Dictionary<string, List<MapEntityPrefab>>();
			Dictionary<string, MapEntityCategory> categoryKeys = new Dictionary<string, MapEntityCategory>();
			foreach (object obj in Enum.GetValues(typeof(MapEntityCategory)))
			{
				MapEntityCategory category = (MapEntityCategory)obj;
				if (category != MapEntityCategory.None)
				{
					LocalizedString categoryName = TextManager.Get("MapEntityCategory." + category.ToString());
					maxTextWidth = (int)Math.Max((float)maxTextWidth, GUIStyle.SubHeadingFont.MeasureString(categoryName.Replace(" ", "\n", StringComparison.Ordinal), false).X + (float)GUI.IntScale(50f));
					foreach (MapEntityPrefab ep in MapEntityPrefab.List)
					{
						if (ep.Category.HasFlag(category))
						{
							if (!entityLists.ContainsKey(category.ToString() + ep.Subcategory))
							{
								entityLists[category.ToString() + ep.Subcategory] = new List<MapEntityPrefab>();
							}
							entityLists[category.ToString() + ep.Subcategory].Add(ep);
							categoryKeys[category.ToString() + ep.Subcategory] = category;
							LocalizedString subcategoryName = TextManager.Get("subcategory." + ep.Subcategory).Fallback(ep.Subcategory, true);
							if (subcategoryName != null)
							{
								maxTextWidth = (int)Math.Max((float)maxTextWidth, GUIStyle.SubHeadingFont.MeasureString(subcategoryName.Replace(" ", "\n", StringComparison.Ordinal), false).X + (float)GUI.IntScale(50f));
							}
						}
					}
				}
			}
			this.categorizedEntityList.Content.ClampMouseRectToParent = true;
			int entitiesPerRow = (int)Math.Ceiling((double)((float)this.categorizedEntityList.Content.Rect.Width / Math.Max(125f * GUI.Scale, 60f)));
			foreach (string categoryKey in entityLists.Keys)
			{
				GUIFrame categoryFrame = new GUIFrame(new RectTransform(Vector2.One, this.categorizedEntityList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
				{
					ClampMouseRectToParent = true,
					UserData = categoryKeys[categoryKey]
				};
				new GUIFrame(new RectTransform(Vector2.One, categoryFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "HorizontalLine", null);
				LocalizedString categoryName2 = TextManager.Get("MapEntityCategory." + entityLists[categoryKey].First<MapEntityPrefab>().Category.ToString());
				LocalizedString subCategoryName = entityLists[categoryKey].First<MapEntityPrefab>().Subcategory;
				if (subCategoryName.IsNullOrEmpty())
				{
					RectTransform rectT = new RectTransform(new Point(maxTextWidth, categoryFrame.Rect.Height), categoryFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
					RichString text = categoryName2;
					GUIFont font = GUIStyle.SubHeadingFont;
					new GUITextBlock(rectT, text, null, font, Alignment.TopLeft, true, "", null).Padding = new Vector4((float)GUI.IntScale(10f));
				}
				else
				{
					LocalizedString localizedString;
					if (!subCategoryName.IsNullOrEmpty())
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(12, 1);
						defaultInterpolatedStringHandler.AppendLiteral("subcategory.");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(subCategoryName);
						localizedString = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(subCategoryName, true);
					}
					else
					{
						localizedString = TextManager.Get("mapentitycategory.misc");
					}
					subCategoryName = localizedString;
					RectTransform rectT2 = new RectTransform(new Point(maxTextWidth, categoryFrame.Rect.Height), categoryFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
					RichString text2 = categoryName2;
					GUIFont font = GUIStyle.Font;
					GUITextBlock categoryTitle = new GUITextBlock(rectT2, text2, null, font, Alignment.TopLeft, true, "", null)
					{
						Padding = new Vector4((float)GUI.IntScale(10f))
					};
					RectTransform rectTransform = new RectTransform(new Point(maxTextWidth, categoryFrame.Rect.Height), categoryFrame.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
					rectTransform.AbsoluteOffset = new Point(0, (int)(categoryTitle.TextSize.Y + (float)GUI.IntScale(10f)));
					RichString text3 = subCategoryName;
					font = GUIStyle.SubHeadingFont;
					new GUITextBlock(rectTransform, text3, null, font, Alignment.TopLeft, true, "", null).Padding = new Vector4((float)GUI.IntScale(10f));
				}
				GUIListBox guilistBox = new GUIListBox(new RectTransform(new Point(categoryFrame.Rect.Width - maxTextWidth, categoryFrame.Rect.Height), categoryFrame.RectTransform, Anchor.CenterRight, null, ScaleBasis.Normal, false), false, null, null, true, true);
				guilistBox.ScrollBarVisible = false;
				guilistBox.AutoHideScrollBar = false;
				guilistBox.OnSelected = new GUIListBox.OnSelectedHandler(this.SelectPrefab);
				guilistBox.UseGridLayout = true;
				GUIListBox guilistBox2 = guilistBox;
				GUIListBox.CheckSelectedHandler checkSelected;
				if ((checkSelected = SubEditorScreen.<>O.<2>__GetSelected) == null)
				{
					checkSelected = (SubEditorScreen.<>O.<2>__GetSelected = new GUIListBox.CheckSelectedHandler(MapEntityPrefab.GetSelected));
				}
				guilistBox2.CheckSelected = checkSelected;
				guilistBox.ClampMouseRectToParent = true;
				guilistBox.PlaySoundOnSelect = true;
				GUIListBox entityListInner = guilistBox;
				entityListInner.ContentBackground.ClampMouseRectToParent = true;
				entityListInner.Content.ClampMouseRectToParent = true;
				foreach (MapEntityPrefab ep2 in entityLists[categoryKey])
				{
					if ((!ep2.HideInMenus && !ep2.HideInEditors) || GameMain.DebugDraw)
					{
						this.CreateEntityElement(ep2, entitiesPerRow, entityListInner.Content);
					}
				}
				entityListInner.UpdateScrollBarSize();
				int contentHeight = (int)(entityListInner.TotalSize + entityListInner.Padding.Y + entityListInner.Padding.W);
				categoryFrame.RectTransform.NonScaledSize = new Point(categoryFrame.Rect.Width, contentHeight);
				categoryFrame.RectTransform.MinSize = new Point(0, contentHeight);
				entityListInner.RectTransform.NonScaledSize = new Point(entityListInner.Rect.Width, contentHeight);
				entityListInner.RectTransform.MinSize = new Point(0, contentHeight);
				entityListInner.Content.RectTransform.SortChildren(delegate(RectTransform i1, RectTransform i2)
				{
					MapEntityPrefab mapEntityPrefab = (MapEntityPrefab)i1.GUIComponent.UserData;
					string strA = (mapEntityPrefab != null) ? mapEntityPrefab.Name.Value : null;
					MapEntityPrefab mapEntityPrefab2 = i2.GUIComponent.UserData as MapEntityPrefab;
					return string.Compare(strA, (mapEntityPrefab2 != null) ? mapEntityPrefab2.Name.Value : null, StringComparison.Ordinal);
				});
			}
			foreach (MapEntityPrefab ep3 in MapEntityPrefab.List)
			{
				if ((!ep3.HideInMenus && !ep3.HideInEditors) || GameMain.DebugDraw)
				{
					this.CreateEntityElement(ep3, entitiesPerRow, this.allEntityList.Content);
				}
			}
			this.allEntityList.Content.RectTransform.SortChildren(delegate(RectTransform i1, RectTransform i2)
			{
				MapEntityPrefab mapEntityPrefab = (MapEntityPrefab)i1.GUIComponent.UserData;
				string strA = (mapEntityPrefab != null) ? mapEntityPrefab.Name.Value : null;
				MapEntityPrefab mapEntityPrefab2 = i2.GUIComponent.UserData as MapEntityPrefab;
				return string.Compare(strA, (mapEntityPrefab2 != null) ? mapEntityPrefab2.Name.Value : null, StringComparison.Ordinal);
			});
		}

		// Token: 0x0600272B RID: 10027 RVA: 0x001A637C File Offset: 0x001A457C
		private void CreateEntityElement(MapEntityPrefab ep, int entitiesPerRow, GUIComponent parent)
		{
			bool legacy = ep.Category.HasFlag(MapEntityCategory.Legacy);
			float relWidth = 1f / (float)entitiesPerRow;
			GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(relWidth, relWidth * ((float)parent.Rect.Width / (float)parent.Rect.Height)), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(0, 50)
			}, "GUITextBox", null)
			{
				UserData = ep,
				ClampMouseRectToParent = true
			};
			frame.RectTransform.MinSize = new Point(0, frame.Rect.Width);
			frame.RectTransform.MaxSize = new Point(int.MaxValue, frame.Rect.Width);
			LocalizedString name = legacy ? TextManager.GetWithVariable("legacyitemformat", "[name]", ep.Name, FormatCapitals.No) : ep.Name;
			frame.ToolTip = ep.CreateTooltipText();
			if (ep.IsModded)
			{
				frame.Color = Color.Magenta;
			}
			if (ep.HideInMenus || ep.HideInEditors)
			{
				frame.Color = Color.Red;
				name = "[HIDDEN] " + name;
			}
			frame.ToolTip = RichString.Rich(frame.ToolTip, null);
			GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.8f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
			{
				Stretch = true,
				RelativeSpacing = 0.03f,
				CanBeFocused = false
			};
			Sprite icon = ep.Sprite;
			Color iconColor = Color.White;
			ItemPrefab itemPrefab = ep as ItemPrefab;
			if (itemPrefab != null)
			{
				if (itemPrefab.InventoryIcon != null)
				{
					icon = itemPrefab.InventoryIcon;
					iconColor = itemPrefab.InventoryIconColor;
				}
				else
				{
					iconColor = itemPrefab.SpriteColor;
				}
			}
			GUIImage img = null;
			if (ep.Sprite != null)
			{
				img = new GUIImage(new RectTransform(new Vector2(1f, 0.8f), paddedFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), icon, null, GUIImage.ScalingMode.None)
				{
					CanBeFocused = false,
					LoadAsynchronously = true,
					SpriteEffects = icon.effects,
					Color = (legacy ? (iconColor * 0.6f) : iconColor)
				};
			}
			ItemAssemblyPrefab itemAssemblyPrefab = ep as ItemAssemblyPrefab;
			if (itemAssemblyPrefab != null)
			{
				GUICustomComponent guicustomComponent = new GUICustomComponent(new RectTransform(new Vector2(1f, 0.75f), paddedFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent customComponent)
				{
					if (GUIImage.LoadingTextures)
					{
						return;
					}
					itemAssemblyPrefab.DrawIcon(sb, customComponent);
				}, null);
				guicustomComponent.HideElementsOutsideFrame = true;
				guicustomComponent.ToolTip = frame.ToolTip.SanitizedString;
			}
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal);
			RichString text = RichString.Rich(name, null);
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock textBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Center, false, "", null)
			{
				CanBeFocused = false
			};
			if (legacy)
			{
				textBlock.TextColor *= 0.6f;
			}
			if (name.IsNullOrEmpty())
			{
				DebugConsole.AddWarning("Entity \"" + ep.Identifier.Value + "\" has no name!", ep.ContentPackage);
				textBlock.Text = (frame.ToolTip = ep.Identifier.Value);
				textBlock.TextColor = GUIStyle.Red;
			}
			textBlock.Text = ToolBox.LimitString(textBlock.Text.SanitizedString, textBlock.Font, textBlock.Rect.Width);
			if (ep.Category == MapEntityCategory.ItemAssembly)
			{
				ContentPackage contentPackage = ep.ContentPackage;
				if (contentPackage != null && contentPackage.Files.Length == 1 && ContentPackageManager.LocalPackages.Contains(ep.ContentPackage))
				{
					GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 0.2f), paddedFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 20)
					}, TextManager.Get("Delete"), Alignment.Center, "GUIButtonSmall", null);
					guibutton.UserData = ep;
					guibutton.OnClicked = delegate(GUIButton btn, object userData)
					{
						ItemAssemblyPrefab assemblyPrefab = (ItemAssemblyPrefab)userData;
						if (assemblyPrefab != null)
						{
							GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("DeleteDialogLabel"), TextManager.GetWithVariable("DeleteDialogQuestion", "[file]", assemblyPrefab.Name, FormatCapitals.No), new LocalizedString[]
							{
								TextManager.Get("Yes"),
								TextManager.Get("Cancel")
							}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
							GUIButton guibutton2 = msgBox.Buttons[0];
							guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton deleteBtn, object userData2)
							{
								try
								{
									assemblyPrefab.Delete();
									this.OpenEntityMenu(new MapEntityCategory?(MapEntityCategory.ItemAssembly));
								}
								catch (Exception e)
								{
									DebugConsole.ThrowErrorLocalized(TextManager.GetWithVariable("DeleteFileError", "[file]", assemblyPrefab.Name, FormatCapitals.No), e, null, false, false);
								}
								return true;
							}));
							GUIButton guibutton3 = msgBox.Buttons[0];
							guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
							GUIButton guibutton4 = msgBox.Buttons[1];
							guibutton4.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton4.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
						}
						return true;
					};
				}
			}
			paddedFrame.Recalculate();
			if (img != null)
			{
				img.Scale = Math.Min(Math.Min((float)img.Rect.Width / img.Sprite.size.X, (float)img.Rect.Height / img.Sprite.size.Y), 1.5f);
				img.RectTransform.NonScaledSize = new Point((int)(img.Sprite.size.X * img.Scale), img.Rect.Height);
			}
		}

		// Token: 0x0600272C RID: 10028 RVA: 0x001A692B File Offset: 0x001A4B2B
		public override void Select()
		{
			this.Select(true);
		}

		// Token: 0x0600272D RID: 10029 RVA: 0x001A6934 File Offset: 0x001A4B34
		public void Select(bool enableAutoSave = true)
		{
			base.Select();
			TaskPool.Add("DeterminePublishedItemIds", SteamManager.Workshop.GetPublishedItems(), delegate(Task t)
			{
				ISet<Item> items;
				if (!t.TryGetResult(out items))
				{
					return;
				}
				this.publishedWorkshopItemIds.Clear();
				this.publishedWorkshopItemIds.UnionWith(from it in items
				select it.Id.Value);
			});
			GUI.PreventPauseMenuToggle = false;
			if (!Directory.Exists(SubEditorScreen.autoSavePath))
			{
				DirectoryInfo directoryInfo = Directory.CreateDirectory(SubEditorScreen.autoSavePath, true);
				if (directoryInfo != null && directoryInfo.Exists)
				{
					directoryInfo.Attributes = (FileAttributes.Hidden | FileAttributes.Directory);
				}
				else
				{
					DebugConsole.ThrowError("Failed to create auto save directory!", null, null, false, false);
				}
			}
			if (!File.Exists(SubEditorScreen.autoSaveInfoPath))
			{
				try
				{
					SubEditorScreen.AutoSaveInfo = new XDocument(new object[]
					{
						new XElement("AutoSaves")
					});
					SubEditorScreen.AutoSaveInfo.SaveSafe(SubEditorScreen.autoSaveInfoPath, SaveOptions.None, true, 0);
					goto IL_D6;
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Saving auto save info to \"" + SubEditorScreen.autoSaveInfoPath + "\" failed!", e, null, false, false);
					goto IL_D6;
				}
			}
			SubEditorScreen.AutoSaveInfo = XMLExtensions.TryLoadXml(SubEditorScreen.autoSaveInfoPath);
			IL_D6:
			LightManager lightManager = GameMain.LightManager;
			Level loaded = Level.Loaded;
			Color? color;
			if (loaded == null)
			{
				color = null;
			}
			else
			{
				LevelGenerationParams generationParams = loaded.GenerationParams;
				color = ((generationParams != null) ? new Color?(generationParams.AmbientLightColor) : null);
			}
			lightManager.AmbientLight = (color ?? new Color(3, 3, 3, 3));
			SubEditorScreen.isAutoSaving = false;
			if (!this.wasSelectedBefore)
			{
				this.OpenEntityMenu(null);
				this.wasSelectedBefore = true;
			}
			else
			{
				this.OpenEntityMenu(this.selectedCategory);
			}
			if (this.backedUpSubInfo != null)
			{
				Submarine.Unload();
			}
			string name = (SubEditorScreen.MainSub == null) ? TextManager.Get("unspecifiedsubfilename").Value : SubEditorScreen.MainSub.Info.Name;
			if (this.backedUpSubInfo != null)
			{
				name = this.backedUpSubInfo.Name;
			}
			this.subNameLabel.Text = ToolBox.LimitString(name, this.subNameLabel.Font, this.subNameLabel.Rect.Width);
			this.editorSelectedTime = Option<DateTime>.Some(DateTime.Now);
			GUI.ForceMouseOn(null);
			this.SetMode(SubEditorScreen.Mode.Default);
			this.rotateToolToggle.Selected = false;
			this.scaleToolToggle.Selected = false;
			if (this.backedUpSubInfo != null)
			{
				SubEditorScreen.MainSub = new Submarine(this.backedUpSubInfo, true, null, null);
				if (this.previewImage != null)
				{
					Sprite sprite = this.backedUpSubInfo.PreviewImage;
					if (((sprite != null) ? sprite.Texture : null) != null && !this.backedUpSubInfo.PreviewImage.Texture.IsDisposed)
					{
						this.previewImage.Sprite = this.backedUpSubInfo.PreviewImage;
					}
				}
				this.backedUpSubInfo = null;
			}
			else if (SubEditorScreen.MainSub == null)
			{
				SubmarineInfo subInfo = new SubmarineInfo();
				SubEditorScreen.MainSub = new Submarine(subInfo, false, null, null);
				this.ReconstructLayers();
			}
			SubEditorScreen.MainSub.UpdateTransform(false);
			this.cam.Position = SubEditorScreen.MainSub.Position + SubEditorScreen.MainSub.HiddenSubPosition;
			GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryDefault, 0f, 0);
			GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryWaterAmbience, 0f, 0);
			string downloadFolder = Path.GetFullPath(SaveUtil.SubmarineDownloadFolder);
			this.linkedSubBox.ClearChildren();
			List<ValueTuple<string, SubmarineInfo>> subs = new List<ValueTuple<string, SubmarineInfo>>();
			foreach (SubmarineInfo sub in SubmarineInfo.SavedSubmarines)
			{
				if (sub.Type == SubmarineType.Player && !(Path.GetDirectoryName(Path.GetFullPath(sub.FilePath)) == downloadFolder))
				{
					subs.Add(new ValueTuple<string, SubmarineInfo>(sub.Name, sub));
				}
			}
			foreach (ValueTuple<string, SubmarineInfo> valueTuple in from tuple in subs
			orderby tuple.Item1
			select tuple)
			{
				string subName = valueTuple.Item1;
				SubmarineInfo sub2 = valueTuple.Item2;
				this.linkedSubBox.AddItem(subName, sub2, null, null, null);
			}
			this.cam.UpdateTransform(true, true);
			this.CreateDummyCharacter();
			if (GameSettings.CurrentConfig.EnableSubmarineAutoSave && enableAutoSave)
			{
				CoroutineManager.StartCoroutine(SubEditorScreen.AutoSaveCoroutine(), "SubEditorAutoSave");
			}
			SubEditorScreen.ImageManager.OnEditorSelected();
			if (SubEditorScreen.Layers.None(null))
			{
				this.ReconstructLayers();
			}
		}

		// Token: 0x0600272E RID: 10030 RVA: 0x001A6DC0 File Offset: 0x001A4FC0
		public override void OnFileDropped(string filePath, string extension)
		{
			if (!(extension == ".sub"))
			{
				if (!(extension == ".xml"))
				{
					if (!(extension == ".png") && !(extension == ".jpg") && !(extension == ".jpeg"))
					{
						DebugConsole.ThrowError("Could not drag and drop the file. \"" + extension + "\" is not a valid file extension! (expected .xml, .sub, .png or .jpg)", null, null, false, false);
					}
					else if (this.saveFrame != null)
					{
						Texture2D texture = Sprite.LoadTexture(filePath, false, null);
						this.previewImage.Sprite = new Sprite(texture, null, null, 0f, null);
						if (SubEditorScreen.MainSub != null)
						{
							SubEditorScreen.MainSub.Info.PreviewImage = this.previewImage.Sprite;
							return;
						}
					}
					return;
				}
				string text = File.ReadAllText(filePath, null, true);
				Vector2 mousePos = Mouse.GetState().Position.ToVector2();
				this.PasteAssembly(text, new Vector2?(this.cam.ScreenToWorld(mousePos)));
				return;
			}
			else
			{
				SubmarineInfo info = new SubmarineInfo(filePath, "", null, true, false);
				if (info.IsFileCorrupted)
				{
					DebugConsole.ThrowError("Could not drag and drop the file. File \"" + filePath + "\" is corrupted!", null, null, false, false);
					info.Dispose();
					return;
				}
				LocalizedString body = TextManager.GetWithVariable("SubEditor.LoadConfirmBody", "[submarine]", info.Name, FormatCapitals.No);
				GUI.AskForConfirmation(TextManager.Get("Load"), body, delegate
				{
					this.LoadSub(info, true);
				}, delegate
				{
					info.Dispose();
				}, null, null);
				return;
			}
		}

		// Token: 0x0600272F RID: 10031 RVA: 0x001A6F84 File Offset: 0x001A5184
		private static IEnumerable<CoroutineStatus> AutoSaveCoroutine()
		{
			return new SubEditorScreen.<AutoSaveCoroutine>d__132(-2);
		}

		// Token: 0x06002730 RID: 10032 RVA: 0x001A6F90 File Offset: 0x001A5190
		protected override void DeselectEditorSpecific()
		{
			this.CloseItem();
			GUIComponent guicomponent = SubEditorScreen.autoSaveLabel;
			if (guicomponent != null)
			{
				GUIComponent parent = guicomponent.Parent;
				if (parent != null)
				{
					parent.RemoveChild(SubEditorScreen.autoSaveLabel);
				}
			}
			SubEditorScreen.autoSaveLabel = null;
			DateTime selectedTime;
			if (this.editorSelectedTime.TryUnwrap(out selectedTime))
			{
				TimeSpan timeInEditor = DateTime.Now - selectedTime;
				if (timeInEditor.TotalSeconds > Timing.TotalTime * 1.5)
				{
					string gaIdentifier = "SubEditorScreen.DeselectEditorSpecific:InvalidTimeInEditor";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(102, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Error in sub editor screen. Calculated time in editor ");
					defaultInterpolatedStringHandler.AppendFormatted<TimeSpan>(timeInEditor);
					defaultInterpolatedStringHandler.AppendLiteral(" was larger than the time the game has run (");
					defaultInterpolatedStringHandler.AppendFormatted<double>(Timing.TotalTime);
					defaultInterpolatedStringHandler.AppendLiteral(" s).");
					DebugConsole.ThrowErrorAndLogToGA(gaIdentifier, defaultInterpolatedStringHandler.ToStringAndClear());
				}
				else
				{
					AchievementManager.IncrementStat(AchievementStat.HoursInEditor, (float)timeInEditor.TotalHours);
					this.editorSelectedTime = Option<DateTime>.None();
				}
			}
			GUI.ForceMouseOn(null);
			if (SubEditorScreen.ImageManager.EditorMode)
			{
				GameSettings.SaveCurrentConfig();
			}
			MapEntityPrefab.Selected = null;
			this.saveFrame = null;
			this.loadFrame = null;
			MapEntity.DeselectAll();
			SubEditorScreen.ClearUndoBuffer();
			DebugConsole.DeactivateCheats();
			this.SetMode(SubEditorScreen.Mode.Default);
			SoundPlayer.OverrideMusicType = Identifier.Empty;
			GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryDefault, GameSettings.CurrentConfig.Audio.SoundVolume, 0);
			GameMain.SoundManager.SetCategoryGainMultiplier(SoundManager.SoundCategoryWaterAmbience, GameSettings.CurrentConfig.Audio.SoundVolume, 0);
			if (CoroutineManager.IsCoroutineRunning("SubEditorAutoSave"))
			{
				CoroutineManager.StopCoroutines("SubEditorAutoSave");
			}
			if (this.dummyCharacter != null)
			{
				this.dummyCharacter.Remove();
				this.dummyCharacter = null;
				GameMain.World.ProcessChanges();
			}
			GUIMessageBox.MessageBoxes.ForEachMod(delegate(GUIComponent component)
			{
				GUIMessageBox msgBox = component as GUIMessageBox;
				if (msgBox != null && !msgBox.Closed)
				{
					string text = component.UserData as string;
					if (text != null && text == "colorpicker")
					{
						foreach (GUIColorPicker colorPicker in msgBox.GetAllChildren<GUIColorPicker>())
						{
							colorPicker.Dispose();
						}
						msgBox.Close();
					}
				}
			});
			this.ClearFilter();
			Widget.SelectedWidgets.Remove(this.TransformWidget);
			this.TransformWidget.Color = Color.Yellow;
		}

		// Token: 0x06002731 RID: 10033 RVA: 0x001A7180 File Offset: 0x001A5380
		private void CreateDummyCharacter()
		{
			if (this.dummyCharacter != null)
			{
				this.RemoveDummyCharacter();
			}
			this.dummyCharacter = Character.Create(CharacterPrefab.HumanSpeciesName, Vector2.Zero, "", null, 65533, false, false, true, null, true, true);
			this.dummyCharacter.Info.Name = "Galldren";
			for (int i = 0; i < this.dummyCharacter.Inventory.SlotPositions.Length; i++)
			{
				if (!(InvSlotType.Head | InvSlotType.InnerClothes | InvSlotType.OuterClothes | InvSlotType.Headset | InvSlotType.Card | InvSlotType.Bag).HasFlag(this.dummyCharacter.Inventory.SlotTypes[i]) && this.dummyCharacter.Inventory.SlotPositions[i].Y > (float)(GameMain.GraphicsHeight / 2))
				{
					Vector2[] slotPositions = this.dummyCharacter.Inventory.SlotPositions;
					int num = i;
					slotPositions[num].Y = slotPositions[num].Y - 50f * GUI.Scale;
				}
			}
			this.dummyCharacter.Inventory.CreateSlots();
			Character.Controlled = this.dummyCharacter;
			GameMain.World.ProcessChanges();
		}

		// Token: 0x06002732 RID: 10034 RVA: 0x001A7294 File Offset: 0x001A5494
		private static void AutoSave()
		{
			if (MapEntity.MapEntityList.Any<MapEntity>() && GameSettings.CurrentConfig.EnableSubmarineAutoSave && !SubEditorScreen.isAutoSaving && SubEditorScreen.MainSub != null)
			{
				SubEditorScreen.isAutoSaving = true;
				if (!Directory.Exists(SubEditorScreen.autoSavePath))
				{
					return;
				}
				XDocument doc = new XDocument(new object[]
				{
					new XElement("Submarine")
				});
				SubEditorScreen.MainSub.SaveToXElement(doc.Root);
				Thread saveThread = new Thread(delegate(object start)
				{
					try
					{
						SubEditorScreen.<>c__DisplayClass135_1 CS$<>8__locals2 = new SubEditorScreen.<>c__DisplayClass135_1();
						Validation.SkipValidationInDebugBuilds = true;
						CS$<>8__locals2.time = DateTime.UtcNow - DateTime.MinValue;
						SubEditorScreen.<>c__DisplayClass135_1 CS$<>8__locals3 = CS$<>8__locals2;
						string[] array = new string[2];
						array[0] = SubEditorScreen.autoSavePath;
						int num = 1;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(13, 1);
						defaultInterpolatedStringHandler.AppendLiteral("AutoSave_");
						defaultInterpolatedStringHandler.AppendFormatted<ulong>((ulong)CS$<>8__locals2.time.TotalMilliseconds);
						defaultInterpolatedStringHandler.AppendLiteral(".sub");
						array[num] = defaultInterpolatedStringHandler.ToStringAndClear();
						CS$<>8__locals3.filePath = Path.Combine(array);
						SaveUtil.CompressStringToFile(CS$<>8__locals2.filePath, doc.ToString());
						CrossThread.RequestExecutionOnMainThread(delegate
						{
							XDocument autoSaveInfo = SubEditorScreen.AutoSaveInfo;
							if (((autoSaveInfo != null) ? autoSaveInfo.Root : null) != null)
							{
								Submarine mainSub = SubEditorScreen.MainSub;
								if (((mainSub != null) ? mainSub.Info : null) != null)
								{
									int saveCount = SubEditorScreen.AutoSaveInfo.Root.Elements().Count<XElement>();
									while (SubEditorScreen.AutoSaveInfo.Root.Elements().Count<XElement>() > SubEditorScreen.MaxAutoSaves)
									{
										XElement min = (from element in SubEditorScreen.AutoSaveInfo.Root.Elements()
										orderby element.GetAttributeUInt64("time", 0UL)
										select element).FirstOrDefault<XElement>();
										string path = min.GetAttributeStringUnrestricted("file", "");
										if (!string.IsNullOrWhiteSpace(path))
										{
											if (File.Exists(path))
											{
												File.Delete(path, true);
											}
											if (min != null)
											{
												min.Remove();
											}
										}
									}
									XElement newElement = new XElement("AutoSave", new object[]
									{
										new XAttribute("file", CS$<>8__locals2.filePath),
										new XAttribute("name", SubEditorScreen.MainSub.Info.Name),
										new XAttribute("time", (ulong)CS$<>8__locals2.time.TotalSeconds)
									});
									SubEditorScreen.AutoSaveInfo.Root.Add(newElement);
									try
									{
										SubEditorScreen.AutoSaveInfo.SaveSafe(SubEditorScreen.autoSaveInfoPath, SaveOptions.None, false, 0);
									}
									catch (Exception e2)
									{
										DebugConsole.ThrowError("Saving auto save info to \"" + SubEditorScreen.autoSaveInfoPath + "\" failed!", e2, null, false, false);
									}
									return;
								}
							}
						});
						Validation.SkipValidationInDebugBuilds = false;
						CrossThread.TaskDelegate deleg;
						if ((deleg = SubEditorScreen.<>O.<3>__DisplayAutoSavePrompt) == null)
						{
							deleg = (SubEditorScreen.<>O.<3>__DisplayAutoSavePrompt = new CrossThread.TaskDelegate(SubEditorScreen.DisplayAutoSavePrompt));
						}
						CrossThread.RequestExecutionOnMainThread(deleg);
					}
					catch (Exception ex)
					{
						Exception e2 = ex;
						Exception e = e2;
						CrossThread.RequestExecutionOnMainThread(delegate
						{
							DebugConsole.ThrowError("Auto saving submarine failed!", e, null, false, false);
						});
					}
					SubEditorScreen.isAutoSaving = false;
				})
				{
					Name = "Auto Save Thread"
				};
				saveThread.Start();
			}
		}

		// Token: 0x06002733 RID: 10035 RVA: 0x001A7344 File Offset: 0x001A5544
		private static void DisplayAutoSavePrompt()
		{
			if (Screen.Selected != GameMain.SubEditorScreen)
			{
				return;
			}
			GUIComponent guicomponent = SubEditorScreen.autoSaveLabel;
			if (guicomponent != null)
			{
				GUIComponent parent = guicomponent.Parent;
				if (parent != null)
				{
					parent.RemoveChild(SubEditorScreen.autoSaveLabel);
				}
			}
			LocalizedString label = TextManager.Get("AutoSaved");
			SubEditorScreen.autoSaveLabel = new GUILayoutGroup(new RectTransform(new Point(GUI.IntScale(150f), GUI.IntScale(32f)), GameMain.SubEditorScreen.EntityMenu.RectTransform, Anchor.TopRight, null, ScaleBasis.Normal, false)
			{
				ScreenSpaceOffset = new Point(-GUI.IntScale(16f), -GUI.IntScale(48f))
			}, true, Anchor.TopLeft)
			{
				CanBeFocused = false
			};
			GUIImage checkmark = new GUIImage(new RectTransform(new Vector2(0.25f, 1f), SubEditorScreen.autoSaveLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "MissionCompletedIcon", true);
			RectTransform rectT = new RectTransform(new Vector2(0.75f, 1f), SubEditorScreen.autoSaveLabel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = label;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			Color? color = new Color?(GUIStyle.Green);
			GUITextBlock labelComponent = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", color)
			{
				Padding = Vector4.Zero,
				AutoScaleHorizontal = true,
				AutoScaleVertical = true
			};
			labelComponent.FadeOut(0.5f, true, 1f, null, false);
			checkmark.FadeOut(0.5f, true, 1f, null, false);
			GUIComponent guicomponent2 = SubEditorScreen.autoSaveLabel;
			if (guicomponent2 == null)
			{
				return;
			}
			guicomponent2.FadeOut(0.5f, true, 1f, null, false);
		}

		// Token: 0x06002734 RID: 10036 RVA: 0x001A7510 File Offset: 0x001A5710
		private bool SaveSub(ContentPackage packageToSaveTo)
		{
			SubEditorScreen.<>c__DisplayClass137_0 CS$<>8__locals1 = new SubEditorScreen.<>c__DisplayClass137_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.packageToSaveTo = packageToSaveTo;
			if (string.IsNullOrWhiteSpace(this.nameBox.Text))
			{
				GUI.AddMessage(TextManager.Get("SubNameMissingWarning"), GUIStyle.Red, null, true, null);
				this.nameBox.Flash(null, 1.5f, false, false, null);
				return false;
			}
			if (CS$<>8__locals1.packageToSaveTo == null)
			{
				IEnumerable<SubmarineFile> subFiles = ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => p.GetFiles<SubmarineFile>());
				SubmarineFile nameConflictFile = subFiles.FirstOrDefault((SubmarineFile file) => Path.GetFileNameWithoutExtension(file.Path.Value).Equals(CS$<>8__locals1.<>4__this.nameBox.Text, StringComparison.InvariantCultureIgnoreCase));
				if (nameConflictFile != null)
				{
					new GUIMessageBox(TextManager.Get("error"), TextManager.GetWithVariable("subeditor.duplicatefilenameerror", "[packagename]", nameConflictFile.ContentPackage.Name, FormatCapitals.No), null, null, GUIMessageBox.Type.Default);
					return false;
				}
			}
			if (SubEditorScreen.MainSub.Info.Type != SubmarineType.Player)
			{
				if (SubEditorScreen.MainSub.Info.Type == SubmarineType.OutpostModule && SubEditorScreen.MainSub.Info.OutpostModuleInfo != null)
				{
					SubEditorScreen.MainSub.Info.PreviewImage = null;
				}
			}
			else if (SubEditorScreen.MainSub.Info.SubmarineClass == SubmarineClass.Undefined && !SubEditorScreen.MainSub.Info.HasTag(SubmarineTag.Shuttle))
			{
				GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("warning"), TextManager.Get("undefinedsubmarineclasswarning"), new LocalizedString[]
				{
					TextManager.Get("yes"),
					TextManager.Get("no")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				msgBox.Buttons[0].OnClicked = delegate(GUIButton bt, object userdata)
				{
					SubEditorScreen.<>c__DisplayClass137_0 CS$<>8__locals3 = CS$<>8__locals1;
					Action action;
					if ((action = CS$<>8__locals1.<>9__6) == null)
					{
						action = (CS$<>8__locals1.<>9__6 = delegate()
						{
							CS$<>8__locals1.<>4__this.SaveSubToFile(CS$<>8__locals1.<>4__this.nameBox.Text, CS$<>8__locals1.packageToSaveTo);
						});
					}
					CS$<>8__locals3.<SaveSub>g__handleExceptions|0(action);
					CS$<>8__locals1.<>4__this.saveFrame = null;
					msgBox.Close();
					return true;
				};
				msgBox.Buttons[1].OnClicked = delegate(GUIButton bt, object userdata)
				{
					msgBox.Close();
					return true;
				};
				return true;
			}
			CS$<>8__locals1.result = false;
			CS$<>8__locals1.<SaveSub>g__handleExceptions|0(delegate
			{
				CS$<>8__locals1.result = CS$<>8__locals1.<>4__this.SaveSubToFile(CS$<>8__locals1.<>4__this.nameBox.Text, CS$<>8__locals1.packageToSaveTo);
			});
			this.saveFrame = null;
			return CS$<>8__locals1.result;
		}

		// Token: 0x06002735 RID: 10037 RVA: 0x001A7788 File Offset: 0x001A5988
		private void ReloadModifiedPackage(ContentPackage p)
		{
			if (p == null)
			{
				return;
			}
			p.ReloadSubsAndItemAssemblies();
			if (p.Files.Length == 0)
			{
				Directory.Delete(p.Dir, true, true);
				ContentPackageManager.LocalPackages.Refresh();
				ContentPackageManager.EnabledPackages.DisableRemovedMods();
			}
		}

		// Token: 0x06002736 RID: 10038 RVA: 0x001A77CC File Offset: 0x001A59CC
		public static Type DetermineSubFileType(SubmarineType type)
		{
			Type result;
			switch (type)
			{
			case SubmarineType.Player:
				result = typeof(SubmarineFile);
				break;
			case SubmarineType.Outpost:
				result = typeof(OutpostFile);
				break;
			case SubmarineType.OutpostModule:
				result = typeof(OutpostModuleFile);
				break;
			case SubmarineType.Wreck:
				result = typeof(WreckFile);
				break;
			case SubmarineType.BeaconStation:
				result = typeof(BeaconStationFile);
				break;
			case SubmarineType.EnemySubmarine:
				result = typeof(EnemySubmarineFile);
				break;
			case SubmarineType.Ruin:
				result = typeof(OutpostModuleFile);
				break;
			default:
				result = null;
				break;
			}
			return result;
		}

		// Token: 0x06002737 RID: 10039 RVA: 0x001A785C File Offset: 0x001A5A5C
		private bool SaveSubToFile(string name, ContentPackage packageToSaveTo)
		{
			SubEditorScreen.<>c__DisplayClass140_0 CS$<>8__locals1 = new SubEditorScreen.<>c__DisplayClass140_0();
			CS$<>8__locals1.packageToSaveTo = packageToSaveTo;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.remoteStorageWasEnabled = Submarine.MainSub.Info.SaveToRemoteStorage;
			SubEditorScreen.<>c__DisplayClass140_0 CS$<>8__locals2 = CS$<>8__locals1;
			Submarine mainSub = SubEditorScreen.MainSub;
			CS$<>8__locals2.subFileType = SubEditorScreen.DetermineSubFileType((mainSub != null) ? mainSub.Info.Type : SubmarineType.Player);
			if (!GameMain.DebugDraw)
			{
				if (Submarine.GetLightCount() > 600)
				{
					new GUIMessageBox(TextManager.Get("error"), TextManager.GetWithVariable("subeditor.lightcounterror", "[max]", 600.ToString(), FormatCapitals.No), null, null, GUIMessageBox.Type.Default);
					return false;
				}
				if (Submarine.GetShadowCastingLightCount() > 100)
				{
					new GUIMessageBox(TextManager.Get("error"), TextManager.GetWithVariable("subeditor.shadowcastinglightcounterror", "[max]", 100.ToString(), FormatCapitals.No), null, null, GUIMessageBox.Type.Default);
					return false;
				}
			}
			if (string.IsNullOrWhiteSpace(name))
			{
				GUI.AddMessage(TextManager.Get("SubNameMissingWarning"), GUIStyle.Red, null, true, null);
				return false;
			}
			foreach (char illegalChar in Path.GetInvalidFileNameCharsCrossPlatform())
			{
				if (name.Contains(illegalChar))
				{
					GUI.AddMessage(TextManager.GetWithVariable("SubNameIllegalCharsWarning", "[illegalchar]", illegalChar.ToString(), FormatCapitals.No), GUIStyle.Red, null, true, null);
					return false;
				}
			}
			name = name.Trim();
			CS$<>8__locals1.newLocalModDir = "LocalMods/" + name;
			CS$<>8__locals1.savePath = name + ".sub";
			CS$<>8__locals1.prevSavePath = null;
			if (CS$<>8__locals1.packageToSaveTo != null)
			{
				ModProject modProject = new ModProject(CS$<>8__locals1.packageToSaveTo);
				string fileListPath = CS$<>8__locals1.packageToSaveTo.Path;
				if (CS$<>8__locals1.packageToSaveTo == ContentPackageManager.VanillaCorePackage)
				{
					throw new InvalidOperationException("Cannot save to Vanilla package");
				}
				string existingFilePath = SubEditorScreen.<SaveSubToFile>g__getExistingFilePath|140_0(CS$<>8__locals1.packageToSaveTo, CS$<>8__locals1.savePath);
				if (existingFilePath != null)
				{
					CS$<>8__locals1.savePath = existingFilePath;
					CS$<>8__locals1.<SaveSubToFile>g__addSubAndSave|2(modProject, CS$<>8__locals1.savePath, fileListPath);
					return true;
				}
				SubmarineInfo existingSubInContentPackage = SubmarineInfo.SavedSubmarines.FirstOrDefault(delegate(SubmarineInfo s)
				{
					SubmarineType type = s.Type;
					Submarine mainSub2 = SubEditorScreen.MainSub;
					SubmarineType? submarineType;
					if (mainSub2 == null)
					{
						submarineType = null;
					}
					else
					{
						SubmarineInfo info = mainSub2.Info;
						submarineType = ((info != null) ? new SubmarineType?(info.Type) : null);
					}
					SubmarineType? submarineType2 = submarineType;
					return (type == submarineType2.GetValueOrDefault() & submarineType2 != null) && CS$<>8__locals1.packageToSaveTo.GetFiles<BaseSubFile>().Any((BaseSubFile f) => f.Path == s.FilePath);
				});
				if (existingSubInContentPackage != null)
				{
					string directoryName = Path.GetDirectoryName(existingSubInContentPackage.FilePath);
					string directoryNameRelativeToPackage = Path.GetRelativePath(Path.GetDirectoryName(CS$<>8__locals1.packageToSaveTo.Path), directoryName);
					GUIMessageBox verification = new GUIMessageBox(string.Empty, TextManager.GetWithVariable("subeditor.saveinexistingfolderprompt", "[folder]", directoryNameRelativeToPackage, FormatCapitals.No), new LocalizedString[]
					{
						TextManager.GetWithVariable("subeditor.saveinexistingfolderprompt.yes", "[folder]", directoryNameRelativeToPackage, FormatCapitals.No),
						TextManager.Get("subeditor.saveinexistingfolderprompt.no")
					}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					verification.Buttons[0].OnClicked = delegate(GUIButton _, object _)
					{
						CS$<>8__locals1.savePath = Path.Combine(new string[]
						{
							directoryNameRelativeToPackage,
							CS$<>8__locals1.savePath
						});
						CS$<>8__locals1.<SaveSubToFile>g__trySaveWithDuplicateCheck|1(modProject, fileListPath);
						verification.Close();
						return true;
					};
					verification.Buttons[1].OnClicked = delegate(GUIButton _, object _)
					{
						CS$<>8__locals1.<SaveSubToFile>g__trySaveWithDuplicateCheck|1(modProject, fileListPath);
						verification.Close();
						return true;
					};
					return true;
				}
				CS$<>8__locals1.<SaveSubToFile>g__trySaveWithDuplicateCheck|1(modProject, fileListPath);
				return true;
			}
			else
			{
				CS$<>8__locals1.savePath = Path.Combine(new string[]
				{
					CS$<>8__locals1.newLocalModDir,
					CS$<>8__locals1.savePath
				});
				if (File.Exists(CS$<>8__locals1.savePath))
				{
					new GUIMessageBox(TextManager.Get("warning"), TextManager.GetWithVariable("subeditor.packagealreadyexists", "[name]", name, FormatCapitals.No), null, null, GUIMessageBox.Type.Default);
					return false;
				}
				ModProject modProject2 = new ModProject
				{
					Name = name
				};
				CS$<>8__locals1.<SaveSubToFile>g__addSubAndSave|2(modProject2, CS$<>8__locals1.savePath, Path.Combine(new string[]
				{
					Path.GetDirectoryName(CS$<>8__locals1.savePath),
					"filelist.xml"
				}));
				return true;
			}
			bool result;
			return result;
		}

		// Token: 0x06002738 RID: 10040 RVA: 0x001A7CFC File Offset: 0x001A5EFC
		private void CreateSaveScreen(bool quickSave = false)
		{
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals1 = new SubEditorScreen.<>c__DisplayClass141_0();
			CS$<>8__locals1.<>4__this = this;
			if (this.saveFrame != null)
			{
				return;
			}
			if (!quickSave)
			{
				this.CloseItem();
				this.SetMode(SubEditorScreen.Mode.Default);
			}
			Vector2 one = Vector2.One;
			RectTransform canvas = GUI.Canvas;
			Anchor anchor = Anchor.Center;
			Pivot? pivot = null;
			Point? point = null;
			Point? minSize = point;
			point = null;
			this.saveFrame = new GUIFrame(new RectTransform(one, canvas, anchor, pivot, minSize, point, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			Vector2 relativeSize = new Vector2(0.6f, 0.7f);
			RectTransform rectTransform = this.saveFrame.RectTransform;
			Anchor anchor2 = Anchor.Center;
			Pivot? pivot2 = null;
			point = null;
			Point? minSize2 = point;
			point = null;
			GUIFrame innerFrame = new GUIFrame(new RectTransform(relativeSize, rectTransform, anchor2, pivot2, minSize2, point, ScaleBasis.Normal)
			{
				MinSize = new Point(750, 500)
			}, "", null);
			Vector2 relativeSize2 = new Vector2(0.95f, 0.9f);
			RectTransform rectTransform2 = innerFrame.RectTransform;
			Anchor anchor3 = Anchor.Center;
			Pivot? pivot3 = null;
			point = null;
			Point? minSize3 = point;
			point = null;
			GUILayoutGroup paddedSaveFrame = new GUILayoutGroup(new RectTransform(relativeSize2, rectTransform2, anchor3, pivot3, minSize3, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.02f
			};
			Vector2 relativeSize3 = new Vector2(1f, 0.9f);
			RectTransform rectTransform3 = paddedSaveFrame.RectTransform;
			Anchor anchor4 = Anchor.TopLeft;
			Pivot? pivot4 = null;
			point = null;
			Point? minSize4 = point;
			point = null;
			GUILayoutGroup columnArea = new GUILayoutGroup(new RectTransform(relativeSize3, rectTransform3, anchor4, pivot4, minSize4, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			Vector2 relativeSize4 = new Vector2(0.55f, 1f);
			RectTransform rectTransform4 = columnArea.RectTransform;
			Anchor anchor5 = Anchor.TopLeft;
			Pivot? pivot5 = null;
			point = null;
			Point? minSize5 = point;
			point = null;
			GUILayoutGroup leftColumn = new GUILayoutGroup(new RectTransform(relativeSize4, rectTransform4, anchor5, pivot5, minSize5, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.01f,
				Stretch = true
			};
			Vector2 relativeSize5 = new Vector2(0.42f, 1f);
			RectTransform rectTransform5 = columnArea.RectTransform;
			Anchor anchor6 = Anchor.TopLeft;
			Pivot? pivot6 = null;
			point = null;
			Point? minSize6 = point;
			point = null;
			GUILayoutGroup rightColumn = new GUILayoutGroup(new RectTransform(relativeSize5, rectTransform5, anchor6, pivot6, minSize6, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.02f,
				Stretch = true
			};
			Vector2 relativeSize6 = new Vector2(0.975f, 0.03f);
			RectTransform rectTransform6 = leftColumn.RectTransform;
			Anchor anchor7 = Anchor.TopLeft;
			Pivot? pivot7 = null;
			point = null;
			Point? minSize7 = point;
			point = null;
			GUILayoutGroup nameHeaderGroup = new GUILayoutGroup(new RectTransform(relativeSize6, rectTransform6, anchor7, pivot7, minSize7, point, ScaleBasis.Normal), true, Anchor.TopLeft);
			Vector2 relativeSize7 = new Vector2(0.5f, 1f);
			RectTransform rectTransform7 = nameHeaderGroup.RectTransform;
			Anchor anchor8 = Anchor.TopLeft;
			Pivot? pivot8 = null;
			point = null;
			Point? minSize8 = point;
			point = null;
			RectTransform rectT = new RectTransform(relativeSize7, rectTransform7, anchor8, pivot8, minSize8, point, ScaleBasis.Normal);
			RichString text6 = TextManager.Get("SaveSubDialogName");
			GUIFont font = GUIStyle.SubHeadingFont;
			GUITextBlock saveSubLabel = new GUITextBlock(rectT, text6, null, font, Alignment.Left, false, "", null);
			Vector2 relativeSize8 = new Vector2(0.5f, 1f);
			RectTransform rectTransform8 = nameHeaderGroup.RectTransform;
			Anchor anchor9 = Anchor.TopLeft;
			Pivot? pivot9 = null;
			point = null;
			Point? minSize9 = point;
			point = null;
			this.submarineNameCharacterCount = new GUITextBlock(new RectTransform(relativeSize8, rectTransform8, anchor9, pivot9, minSize9, point, ScaleBasis.Normal), string.Empty, null, null, Alignment.TopRight, false, "", null);
			Vector2 relativeSize9 = new Vector2(1f, 0.05f);
			RectTransform rectTransform9 = leftColumn.RectTransform;
			Anchor anchor10 = Anchor.TopLeft;
			Pivot? pivot10 = null;
			point = null;
			Point? minSize10 = point;
			point = null;
			this.nameBox = new GUITextBox(new RectTransform(relativeSize9, rectTransform9, anchor10, pivot10, minSize10, point, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				OnEnterPressed = new GUITextBox.OnEnterHandler(this.ChangeSubName)
			};
			this.nameBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				if (text.Length > 30)
				{
					CS$<>8__locals1.<>4__this.nameBox.Text = text.Substring(0, 30);
					CS$<>8__locals1.<>4__this.nameBox.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
					return true;
				}
				CS$<>8__locals1.<>4__this.submarineNameCharacterCount.Text = text.Length.ToString() + " / " + 30.ToString();
				return true;
			};
			GUITextBox guitextBox = this.nameBox;
			Submarine mainSub = SubEditorScreen.MainSub;
			guitextBox.Text = (((mainSub != null) ? mainSub.Info.Name : null) ?? "");
			this.submarineNameCharacterCount.Text = this.nameBox.Text.Length.ToString() + " / " + 30.ToString();
			Vector2 relativeSize10 = new Vector2(0.975f, 0.03f);
			RectTransform rectTransform10 = leftColumn.RectTransform;
			Anchor anchor11 = Anchor.TopLeft;
			Pivot? pivot11 = null;
			point = null;
			Point? minSize11 = point;
			point = null;
			GUILayoutGroup descriptionHeaderGroup = new GUILayoutGroup(new RectTransform(relativeSize10, rectTransform10, anchor11, pivot11, minSize11, point, ScaleBasis.Normal), true, Anchor.TopLeft);
			Vector2 relativeSize11 = new Vector2(0.5f, 1f);
			RectTransform rectTransform11 = descriptionHeaderGroup.RectTransform;
			Anchor anchor12 = Anchor.TopLeft;
			Pivot? pivot12 = null;
			point = null;
			Point? minSize12 = point;
			point = null;
			RectTransform rectT2 = new RectTransform(relativeSize11, rectTransform11, anchor12, pivot12, minSize12, point, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("SaveSubDialogDescription");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, font, Alignment.Left, false, "", null);
			Vector2 relativeSize12 = new Vector2(0.5f, 1f);
			RectTransform rectTransform12 = descriptionHeaderGroup.RectTransform;
			Anchor anchor13 = Anchor.TopLeft;
			Pivot? pivot13 = null;
			point = null;
			Point? minSize13 = point;
			point = null;
			this.submarineDescriptionCharacterCount = new GUITextBlock(new RectTransform(relativeSize12, rectTransform12, anchor13, pivot13, minSize13, point, ScaleBasis.Normal), string.Empty, null, null, Alignment.TopRight, false, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals2 = CS$<>8__locals1;
			Vector2 relativeSize13 = new Vector2(1f, 0.25f);
			RectTransform rectTransform13 = leftColumn.RectTransform;
			Anchor anchor14 = Anchor.TopLeft;
			Pivot? pivot14 = null;
			point = null;
			Point? minSize14 = point;
			point = null;
			CS$<>8__locals2.descriptionContainer = new GUIListBox(new RectTransform(relativeSize13, rectTransform13, anchor14, pivot14, minSize14, point, ScaleBasis.Normal), false, null, "", true, false);
			Vector2 one2 = Vector2.One;
			RectTransform rectTransform14 = CS$<>8__locals1.descriptionContainer.Content.RectTransform;
			Anchor anchor15 = Anchor.Center;
			Pivot? pivot15 = null;
			point = null;
			Point? minSize15 = point;
			point = null;
			RectTransform rectT3 = new RectTransform(one2, rectTransform14, anchor15, pivot15, minSize15, point, ScaleBasis.Normal);
			string text3 = "";
			font = GUIStyle.SmallFont;
			this.descriptionBox = new GUITextBox(rectT3, text3, null, font, Alignment.TopLeft, true, "GUITextBoxNoBorder", null, false, true)
			{
				Padding = new Vector4(10f * GUI.Scale)
			};
			this.descriptionBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				if (text.Length > 500)
				{
					CS$<>8__locals1.<>4__this.descriptionBox.Text = text.Substring(0, 500);
					CS$<>8__locals1.<>4__this.descriptionBox.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
					return true;
				}
				Vector2 textSize = textBox.Font.MeasureString(CS$<>8__locals1.<>4__this.descriptionBox.WrappedText, false);
				textBox.RectTransform.NonScaledSize = new Point(textBox.RectTransform.NonScaledSize.X, Math.Max(CS$<>8__locals1.descriptionContainer.Content.Rect.Height, (int)textSize.Y + 10));
				CS$<>8__locals1.descriptionContainer.UpdateScrollBarSize();
				CS$<>8__locals1.descriptionContainer.BarScroll = 1f;
				CS$<>8__locals1.<>4__this.ChangeSubDescription(textBox, text);
				return true;
			};
			this.descriptionBox.Text = SubEditorScreen.GetSubDescription();
			Vector2 relativeSize14 = new Vector2(1f, 0.01f);
			RectTransform rectTransform15 = leftColumn.RectTransform;
			Anchor anchor16 = Anchor.TopLeft;
			Pivot? pivot16 = null;
			point = null;
			Point? minSize16 = point;
			point = null;
			GUILayoutGroup subTypeContainer = new GUILayoutGroup(new RectTransform(relativeSize14, rectTransform15, anchor16, pivot16, minSize16, point, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize15 = new Vector2(0.4f, 1f);
			RectTransform rectTransform16 = subTypeContainer.RectTransform;
			Anchor anchor17 = Anchor.TopLeft;
			Pivot? pivot17 = null;
			point = null;
			Point? minSize17 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize15, rectTransform16, anchor17, pivot17, minSize17, point, ScaleBasis.Normal), TextManager.Get("submarinetype"), null, null, Alignment.Left, false, "", null);
			Vector2 relativeSize16 = new Vector2(0.6f, 1f);
			RectTransform rectTransform17 = subTypeContainer.RectTransform;
			Anchor anchor18 = Anchor.TopLeft;
			Pivot? pivot18 = null;
			point = null;
			Point? minSize18 = point;
			point = null;
			GUIDropDown subTypeDropdown = new GUIDropDown(new RectTransform(relativeSize16, rectTransform17, anchor18, pivot18, minSize18, point, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			subTypeContainer.RectTransform.MinSize = new Point(0, subTypeContainer.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			foreach (object obj in Enum.GetValues(typeof(SubmarineType)))
			{
				SubmarineType subType = (SubmarineType)obj;
				if (subType != SubmarineType.Ruin)
				{
					string textTag = "SubmarineType." + subType.ToString();
					if (subType == SubmarineType.EnemySubmarine && !TextManager.ContainsTag(textTag))
					{
						textTag = "MissionType.Pirate";
					}
					subTypeDropdown.AddItem(TextManager.Get(textTag), subType, null, null, null);
				}
			}
			Point minSize22;
			if (SubEditorScreen.Layers.Any<KeyValuePair<string, SubEditorScreen.LayerData>>())
			{
				SubEditorScreen.<>c__DisplayClass141_1 CS$<>8__locals3 = new SubEditorScreen.<>c__DisplayClass141_1();
				CS$<>8__locals3.CS$<>8__locals1 = CS$<>8__locals1;
				Vector2 relativeSize17 = new Vector2(1f, 0.01f);
				RectTransform rectTransform18 = leftColumn.RectTransform;
				Anchor anchor19 = Anchor.TopLeft;
				Pivot? pivot19 = null;
				point = null;
				Point? minSize19 = point;
				point = null;
				GUILayoutGroup layerVisibilityGroup = new GUILayoutGroup(new RectTransform(relativeSize17, rectTransform18, anchor19, pivot19, minSize19, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
				IEnumerable<KeyValuePair<string, SubEditorScreen.LayerData>> visibleLayers = from l in SubEditorScreen.Layers
				where !SubEditorScreen.MainSub.Info.LayersHiddenByDefault.Contains(l.Key.ToIdentifier())
				select l;
				LocalizedString visibleLayersString = LocalizedString.Join(", ", (from l in visibleLayers
				select l.Key.Capitalize()) ?? "None".ToEnumerable<LocalizedString>());
				Vector2 relativeSize18 = new Vector2(0.5f, 1f);
				RectTransform rectTransform19 = layerVisibilityGroup.RectTransform;
				Anchor anchor20 = Anchor.TopLeft;
				Pivot? pivot20 = null;
				point = null;
				Point? minSize20 = point;
				point = null;
				new GUITextBlock(new RectTransform(relativeSize18, rectTransform19, anchor20, pivot20, minSize20, point, ScaleBasis.Normal), TextManager.Get("editor.layer.visiblebydefault"), null, null, Alignment.CenterLeft, false, "", null);
				SubEditorScreen.<>c__DisplayClass141_1 CS$<>8__locals4 = CS$<>8__locals3;
				Vector2 relativeSize19 = new Vector2(0.5f, 1f);
				RectTransform rectTransform20 = layerVisibilityGroup.RectTransform;
				Anchor anchor21 = Anchor.TopLeft;
				Pivot? pivot21 = null;
				point = null;
				Point? minSize21 = point;
				point = null;
				CS$<>8__locals4.layerVisibilityDropDown = new GUIDropDown(new RectTransform(relativeSize19, rectTransform20, anchor21, pivot21, minSize21, point, ScaleBasis.Normal), visibleLayersString, 4, "", true, false, Alignment.CenterLeft, 1f);
				foreach (KeyValuePair<string, SubEditorScreen.LayerData> layer in SubEditorScreen.Layers)
				{
					string layerName = layer.Key;
					CS$<>8__locals3.layerVisibilityDropDown.AddItem(layerName.Capitalize(), layerName, null, null, null);
					if (visibleLayers.Contains(layer))
					{
						CS$<>8__locals3.layerVisibilityDropDown.SelectItem(layerName);
					}
				}
				GUIDropDown layerVisibilityDropDown = CS$<>8__locals3.layerVisibilityDropDown;
				layerVisibilityDropDown.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(layerVisibilityDropDown.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent button, object _)
				{
					SubEditorScreen.MainSub.Info.LayersHiddenByDefault.Clear();
					foreach (KeyValuePair<string, SubEditorScreen.LayerData> layer2 in SubEditorScreen.Layers)
					{
						if (!CS$<>8__locals3.layerVisibilityDropDown.SelectedDataMultiple.Contains(layer2.Key))
						{
							SubEditorScreen.MainSub.Info.LayersHiddenByDefault.Add(layer2.Key.ToIdentifier());
						}
					}
					CS$<>8__locals3.CS$<>8__locals1.<>4__this.UpdateLayerPanel();
					CS$<>8__locals3.layerVisibilityDropDown.Text = ToolBox.LimitString(CS$<>8__locals3.layerVisibilityDropDown.Text.Value, CS$<>8__locals3.layerVisibilityDropDown.Font, CS$<>8__locals3.layerVisibilityDropDown.Rect.Width);
					return true;
				}));
				RectTransform rectTransform21 = layerVisibilityGroup.RectTransform;
				RectTransform rectTransform22 = CS$<>8__locals3.layerVisibilityDropDown.RectTransform;
				minSize22 = new Point(0, CS$<>8__locals3.layerVisibilityDropDown.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
				rectTransform22.MinSize = minSize22;
				rectTransform21.MinSize = minSize22;
			}
			Vector2 relativeSize20 = new ValueTuple<float, float>(1f, 0.6f);
			RectTransform rectTransform23 = leftColumn.RectTransform;
			Anchor anchor22 = Anchor.TopLeft;
			Pivot? pivot22 = null;
			point = null;
			Point? minSize23 = point;
			point = null;
			GUIFrame subTypeDependentSettingFrame = new GUIFrame(new RectTransform(relativeSize20, rectTransform23, anchor22, pivot22, minSize23, point, ScaleBasis.Normal), "InnerFrame", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals5 = CS$<>8__locals1;
			Vector2 one3 = Vector2.One;
			RectTransform rectTransform24 = subTypeDependentSettingFrame.RectTransform;
			Anchor anchor23 = Anchor.TopLeft;
			Pivot? pivot23 = null;
			point = null;
			Point? minSize24 = point;
			point = null;
			CS$<>8__locals5.outpostModuleSettingsContainer = new GUILayoutGroup(new RectTransform(one3, rectTransform24, anchor23, pivot23, minSize24, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = true,
				Visible = false,
				Stretch = true
			};
			Vector2 relativeSize21 = new Vector2(0.975f, 0.1f);
			RectTransform rectTransform25 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			Anchor anchor24 = Anchor.TopLeft;
			Pivot? pivot24 = null;
			point = null;
			Point? minSize25 = point;
			point = null;
			GUILayoutGroup outpostModuleGroup = new GUILayoutGroup(new RectTransform(relativeSize21, rectTransform25, anchor24, pivot24, minSize25, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
			Vector2 relativeSize22 = new Vector2(0.5f, 1f);
			RectTransform rectTransform26 = outpostModuleGroup.RectTransform;
			Anchor anchor25 = Anchor.TopLeft;
			Pivot? pivot25 = null;
			point = null;
			Point? minSize26 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize22, rectTransform26, anchor25, pivot25, minSize26, point, ScaleBasis.Normal), TextManager.Get("outpostmoduletype"), null, null, Alignment.CenterLeft, false, "", null);
			HashSet<Identifier> availableFlags = new HashSet<Identifier>();
			foreach (Identifier flag in OutpostGenerationParams.OutpostParams.SelectMany((OutpostGenerationParams p) => from m in p.ModuleCounts
			select m.Identifier))
			{
				availableFlags.Add(flag);
			}
			foreach (Identifier flag2 in RuinGenerationParams.RuinParams.SelectMany((RuinGenerationParams p) => from m in p.ModuleCounts
			select m.Identifier))
			{
				availableFlags.Add(flag2);
			}
			foreach (SubmarineInfo sub in SubmarineInfo.SavedSubmarines)
			{
				if (sub.OutpostModuleInfo != null)
				{
					foreach (Identifier flag3 in sub.OutpostModuleInfo.ModuleFlags)
					{
						if (!(flag3 == "none"))
						{
							availableFlags.Add(flag3);
						}
					}
				}
			}
			Submarine mainSub2 = SubEditorScreen.MainSub;
			OutpostModuleInfo outpostModuleInfo2;
			if (mainSub2 == null)
			{
				outpostModuleInfo2 = null;
			}
			else
			{
				SubmarineInfo info = mainSub2.Info;
				outpostModuleInfo2 = ((info != null) ? info.OutpostModuleInfo : null);
			}
			OutpostModuleInfo moduleInfo = outpostModuleInfo2;
			if (moduleInfo != null)
			{
				foreach (Identifier moduleType in moduleInfo.ModuleFlags)
				{
					availableFlags.Add(moduleType);
				}
			}
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals6 = CS$<>8__locals1;
			Vector2 relativeSize23 = new Vector2(0.5f, 1f);
			RectTransform rectTransform27 = outpostModuleGroup.RectTransform;
			Anchor anchor26 = Anchor.TopLeft;
			Pivot? pivot26 = null;
			point = null;
			Point? minSize27 = point;
			point = null;
			RectTransform rectT4 = new RectTransform(relativeSize23, rectTransform27, anchor26, pivot26, minSize27, point, ScaleBasis.Normal);
			string separator = ", ";
			Submarine mainSub3 = SubEditorScreen.MainSub;
			IEnumerable<LocalizedString> enumerable;
			if (mainSub3 == null)
			{
				enumerable = null;
			}
			else
			{
				SubmarineInfo info2 = mainSub3.Info;
				if (info2 == null)
				{
					enumerable = null;
				}
				else
				{
					OutpostModuleInfo outpostModuleInfo3 = info2.OutpostModuleInfo;
					if (outpostModuleInfo3 == null)
					{
						enumerable = null;
					}
					else
					{
						enumerable = from s in outpostModuleInfo3.ModuleFlags
						select s.Value.Capitalize();
					}
				}
			}
			CS$<>8__locals6.moduleTypeDropDown = new GUIDropDown(rectT4, LocalizedString.Join(separator, enumerable ?? "None".ToEnumerable<LocalizedString>()), 4, "", true, false, Alignment.CenterLeft, 1f);
			foreach (Identifier flag4 in availableFlags.OrderBy((Identifier f) => f.Value, StringComparer.InvariantCultureIgnoreCase))
			{
				CS$<>8__locals1.moduleTypeDropDown.AddItem(flag4.Value.Capitalize(), flag4, null, null, null);
				Submarine mainSub4 = SubEditorScreen.MainSub;
				bool flag6;
				if (mainSub4 == null)
				{
					flag6 = (null != null);
				}
				else
				{
					SubmarineInfo info3 = mainSub4.Info;
					flag6 = (((info3 != null) ? info3.OutpostModuleInfo : null) != null);
				}
				if (flag6 && SubEditorScreen.MainSub.Info.OutpostModuleInfo.ModuleFlags.Contains(flag4))
				{
					CS$<>8__locals1.moduleTypeDropDown.SelectItem(flag4);
				}
			}
			GUIDropDown moduleTypeDropDown = CS$<>8__locals1.moduleTypeDropDown;
			moduleTypeDropDown.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(moduleTypeDropDown.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent _, object __)
			{
				Submarine mainSub25 = SubEditorScreen.MainSub;
				bool flag14;
				if (mainSub25 == null)
				{
					flag14 = (null != null);
				}
				else
				{
					SubmarineInfo info18 = mainSub25.Info;
					flag14 = (((info18 != null) ? info18.OutpostModuleInfo : null) != null);
				}
				if (!flag14)
				{
					return false;
				}
				SubEditorScreen.MainSub.Info.OutpostModuleInfo.SetFlags(CS$<>8__locals1.moduleTypeDropDown.SelectedDataMultiple.Cast<Identifier>());
				CS$<>8__locals1.moduleTypeDropDown.Text = ToolBox.LimitString(SubEditorScreen.MainSub.Info.OutpostModuleInfo.ModuleFlags.Any((Identifier f) => f != "none") ? CS$<>8__locals1.moduleTypeDropDown.Text : "None", CS$<>8__locals1.moduleTypeDropDown.Font, CS$<>8__locals1.moduleTypeDropDown.Rect.Width);
				return true;
			}));
			outpostModuleGroup.RectTransform.MinSize = new Point(0, outpostModuleGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			Vector2 relativeSize24 = new Vector2(0.975f, 0.1f);
			RectTransform rectTransform28 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			Anchor anchor27 = Anchor.TopLeft;
			Pivot? pivot27 = null;
			point = null;
			Point? minSize28 = point;
			point = null;
			GUILayoutGroup addTypeGroup = new GUILayoutGroup(new RectTransform(relativeSize24, rectTransform28, anchor27, pivot27, minSize28, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
			Vector2 relativeSize25 = new Vector2(0.5f, 1f);
			RectTransform rectTransform29 = addTypeGroup.RectTransform;
			Anchor anchor28 = Anchor.TopLeft;
			Pivot? pivot28 = null;
			point = null;
			Point? minSize29 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize25, rectTransform29, anchor28, pivot28, minSize29, point, ScaleBasis.Normal), TextManager.Get("leveleditor.addmoduletype"), null, null, Alignment.CenterLeft, false, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals7 = CS$<>8__locals1;
			Vector2 relativeSize26 = new Vector2(0.4f, 1f);
			RectTransform rectTransform30 = addTypeGroup.RectTransform;
			Anchor anchor29 = Anchor.TopLeft;
			Pivot? pivot29 = null;
			point = null;
			Point? minSize30 = point;
			point = null;
			CS$<>8__locals7.textBox = new GUITextBox(new RectTransform(relativeSize26, rectTransform30, anchor29, pivot29, minSize30, point, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			Vector2 relativeSize27 = new Vector2(0.1f, 0.9f);
			RectTransform rectTransform31 = addTypeGroup.RectTransform;
			Anchor anchor30 = Anchor.TopLeft;
			Pivot? pivot30 = null;
			point = null;
			Point? minSize31 = point;
			point = null;
			new GUIButton(new RectTransform(relativeSize27, rectTransform31, anchor30, pivot30, minSize31, point, ScaleBasis.Normal), "+", Alignment.Center, "GUIButtonSmallFreeScale", null).OnClicked = delegate(GUIButton btn, object _)
			{
				if (CS$<>8__locals1.textBox.Text.IsNullOrEmpty())
				{
					CS$<>8__locals1.textBox.Flash(null, 1.5f, false, false, null);
					return false;
				}
				Submarine mainSub25 = SubEditorScreen.MainSub;
				OutpostModuleInfo outpostModuleInfo9;
				if (mainSub25 == null)
				{
					outpostModuleInfo9 = null;
				}
				else
				{
					SubmarineInfo info18 = mainSub25.Info;
					outpostModuleInfo9 = ((info18 != null) ? info18.OutpostModuleInfo : null);
				}
				OutpostModuleInfo moduleInfo2 = outpostModuleInfo9;
				if (moduleInfo2 != null)
				{
					moduleInfo2.SetFlags(moduleInfo2.ModuleFlags.Append(CS$<>8__locals1.textBox.Text.ToIdentifier()).ToList<Identifier>());
					CS$<>8__locals1.<>4__this.saveFrame = null;
					CS$<>8__locals1.<>4__this.CreateSaveScreen(false);
				}
				return true;
			};
			addTypeGroup.RectTransform.MinSize = new Point(0, addTypeGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			Vector2 relativeSize28 = new Vector2(0.975f, 0.1f);
			RectTransform rectTransform32 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			Anchor anchor31 = Anchor.TopLeft;
			Pivot? pivot31 = null;
			point = null;
			Point? minSize32 = point;
			point = null;
			GUILayoutGroup allowAttachGroup = new GUILayoutGroup(new RectTransform(relativeSize28, rectTransform32, anchor31, pivot31, minSize32, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
			Vector2 relativeSize29 = new Vector2(0.5f, 1f);
			RectTransform rectTransform33 = allowAttachGroup.RectTransform;
			Anchor anchor32 = Anchor.TopLeft;
			Pivot? pivot32 = null;
			point = null;
			Point? minSize33 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize29, rectTransform33, anchor32, pivot32, minSize33, point, ScaleBasis.Normal), TextManager.Get("outpostmoduleallowattachto"), null, null, Alignment.CenterLeft, false, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals8 = CS$<>8__locals1;
			Vector2 relativeSize30 = new Vector2(0.5f, 1f);
			RectTransform rectTransform34 = allowAttachGroup.RectTransform;
			Anchor anchor33 = Anchor.TopLeft;
			Pivot? pivot33 = null;
			point = null;
			Point? minSize34 = point;
			point = null;
			RectTransform rectT5 = new RectTransform(relativeSize30, rectTransform34, anchor33, pivot33, minSize34, point, ScaleBasis.Normal);
			string separator2 = ", ";
			Submarine mainSub5 = SubEditorScreen.MainSub;
			IEnumerable<LocalizedString> enumerable2;
			if (mainSub5 == null)
			{
				enumerable2 = null;
			}
			else
			{
				SubmarineInfo info4 = mainSub5.Info;
				if (info4 == null)
				{
					enumerable2 = null;
				}
				else
				{
					OutpostModuleInfo outpostModuleInfo4 = info4.OutpostModuleInfo;
					if (outpostModuleInfo4 == null)
					{
						enumerable2 = null;
					}
					else
					{
						enumerable2 = from s in outpostModuleInfo4.AllowAttachToModules
						select s.Value.Capitalize();
					}
				}
			}
			CS$<>8__locals8.allowAttachDropDown = new GUIDropDown(rectT5, LocalizedString.Join(separator2, enumerable2 ?? "Any".ToEnumerable<LocalizedString>()), 4, "", true, false, Alignment.CenterLeft, 1f);
			CS$<>8__locals1.allowAttachDropDown.AddItem("any".Capitalize(), "any".ToIdentifier(), null, null, null);
			if (SubEditorScreen.MainSub.Info.OutpostModuleInfo != null && SubEditorScreen.MainSub.Info.OutpostModuleInfo.AllowAttachToModules.Any<Identifier>())
			{
				if (!SubEditorScreen.MainSub.Info.OutpostModuleInfo.AllowAttachToModules.All((Identifier s) => s == "any"))
				{
					goto IL_140A;
				}
			}
			CS$<>8__locals1.allowAttachDropDown.SelectItem("any".ToIdentifier());
			IL_140A:
			foreach (Identifier flag5 in availableFlags.OrderBy((Identifier f) => f.Value, StringComparer.InvariantCultureIgnoreCase))
			{
				if (!(flag5 == "any") && !(flag5 == "none"))
				{
					CS$<>8__locals1.allowAttachDropDown.AddItem(flag5.Value.Capitalize(), flag5, null, null, null);
					Submarine mainSub6 = SubEditorScreen.MainSub;
					bool flag7;
					if (mainSub6 == null)
					{
						flag7 = (null != null);
					}
					else
					{
						SubmarineInfo info5 = mainSub6.Info;
						flag7 = (((info5 != null) ? info5.OutpostModuleInfo : null) != null);
					}
					if (flag7 && SubEditorScreen.MainSub.Info.OutpostModuleInfo.AllowAttachToModules.Contains(flag5))
					{
						CS$<>8__locals1.allowAttachDropDown.SelectItem(flag5);
					}
				}
			}
			GUIDropDown allowAttachDropDown = CS$<>8__locals1.allowAttachDropDown;
			allowAttachDropDown.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(allowAttachDropDown.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent _, object __)
			{
				Submarine mainSub25 = SubEditorScreen.MainSub;
				bool flag14;
				if (mainSub25 == null)
				{
					flag14 = (null != null);
				}
				else
				{
					SubmarineInfo info18 = mainSub25.Info;
					flag14 = (((info18 != null) ? info18.OutpostModuleInfo : null) != null);
				}
				if (!flag14)
				{
					return false;
				}
				SubEditorScreen.MainSub.Info.OutpostModuleInfo.SetAllowAttachTo(CS$<>8__locals1.allowAttachDropDown.SelectedDataMultiple.Cast<Identifier>());
				CS$<>8__locals1.allowAttachDropDown.Text = ToolBox.LimitString(SubEditorScreen.MainSub.Info.OutpostModuleInfo.ModuleFlags.Any((Identifier f) => f != "none") ? CS$<>8__locals1.allowAttachDropDown.Text.Value : "None", CS$<>8__locals1.allowAttachDropDown.Font, CS$<>8__locals1.allowAttachDropDown.Rect.Width);
				return true;
			}));
			allowAttachGroup.RectTransform.MinSize = new Point(0, allowAttachGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			Vector2 relativeSize31 = new Vector2(0.975f, 0.1f);
			RectTransform rectTransform35 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			Anchor anchor34 = Anchor.TopLeft;
			Pivot? pivot34 = null;
			point = null;
			Point? minSize35 = point;
			point = null;
			GUILayoutGroup locationTypeGroup = new GUILayoutGroup(new RectTransform(relativeSize31, rectTransform35, anchor34, pivot34, minSize35, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
			Vector2 relativeSize32 = new Vector2(0.5f, 1f);
			RectTransform rectTransform36 = locationTypeGroup.RectTransform;
			Anchor anchor35 = Anchor.TopLeft;
			Pivot? pivot35 = null;
			point = null;
			Point? minSize36 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize32, rectTransform36, anchor35, pivot35, minSize36, point, ScaleBasis.Normal), TextManager.Get("outpostmoduleallowedlocationtypes"), null, null, Alignment.CenterLeft, false, "", null);
			HashSet<Identifier> availableLocationTypes = new HashSet<Identifier>();
			foreach (LocationType locationType in LocationType.Prefabs)
			{
				availableLocationTypes.Add(locationType.Identifier);
			}
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals9 = CS$<>8__locals1;
			Vector2 relativeSize33 = new Vector2(0.5f, 1f);
			RectTransform rectTransform37 = locationTypeGroup.RectTransform;
			Anchor anchor36 = Anchor.TopLeft;
			Pivot? pivot36 = null;
			point = null;
			Point? minSize37 = point;
			point = null;
			RectTransform rectT6 = new RectTransform(relativeSize33, rectTransform37, anchor36, pivot36, minSize37, point, ScaleBasis.Normal);
			string separator3 = ", ";
			Submarine mainSub7 = SubEditorScreen.MainSub;
			IEnumerable<LocalizedString> enumerable3;
			if (mainSub7 == null)
			{
				enumerable3 = null;
			}
			else
			{
				SubmarineInfo info6 = mainSub7.Info;
				if (info6 == null)
				{
					enumerable3 = null;
				}
				else
				{
					OutpostModuleInfo outpostModuleInfo5 = info6.OutpostModuleInfo;
					if (outpostModuleInfo5 == null)
					{
						enumerable3 = null;
					}
					else
					{
						enumerable3 = from lt in outpostModuleInfo5.AllowedLocationTypes
						select lt.Value.Capitalize();
					}
				}
			}
			CS$<>8__locals9.locationTypeDropDown = new GUIDropDown(rectT6, LocalizedString.Join(separator3, enumerable3 ?? "any".ToEnumerable<LocalizedString>()), 4, "", true, false, Alignment.CenterLeft, 1f);
			CS$<>8__locals1.locationTypeDropDown.AddItem("any".Capitalize(), "any".ToIdentifier(), null, null, null);
			foreach (Identifier locationType2 in availableLocationTypes.OrderBy((Identifier f) => f.Value, StringComparer.InvariantCultureIgnoreCase))
			{
				CS$<>8__locals1.locationTypeDropDown.AddItem(locationType2.Value.Capitalize(), locationType2, null, null, null);
				Submarine mainSub8 = SubEditorScreen.MainSub;
				bool flag8;
				if (mainSub8 == null)
				{
					flag8 = (null != null);
				}
				else
				{
					SubmarineInfo info7 = mainSub8.Info;
					flag8 = (((info7 != null) ? info7.OutpostModuleInfo : null) != null);
				}
				if (flag8 && SubEditorScreen.MainSub.Info.OutpostModuleInfo.AllowedLocationTypes.Contains(locationType2))
				{
					CS$<>8__locals1.locationTypeDropDown.SelectItem(locationType2);
				}
			}
			SubmarineInfo info8 = SubEditorScreen.MainSub.Info;
			bool? flag9;
			if (info8 == null)
			{
				flag9 = null;
			}
			else
			{
				OutpostModuleInfo outpostModuleInfo6 = info8.OutpostModuleInfo;
				if (outpostModuleInfo6 == null)
				{
					flag9 = null;
				}
				else
				{
					IEnumerable<Identifier> allowedLocationTypes = outpostModuleInfo6.AllowedLocationTypes;
					flag9 = ((allowedLocationTypes != null) ? new bool?(!allowedLocationTypes.Any<Identifier>()) : null);
				}
			}
			bool? flag10 = flag9;
			if (flag10.GetValueOrDefault(true))
			{
				CS$<>8__locals1.locationTypeDropDown.SelectItem("any".ToIdentifier());
			}
			GUIDropDown locationTypeDropDown = CS$<>8__locals1.locationTypeDropDown;
			locationTypeDropDown.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(locationTypeDropDown.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent _, object __)
			{
				Submarine mainSub25 = SubEditorScreen.MainSub;
				if (mainSub25 != null)
				{
					SubmarineInfo info18 = mainSub25.Info;
					if (info18 != null)
					{
						OutpostModuleInfo outpostModuleInfo9 = info18.OutpostModuleInfo;
						if (outpostModuleInfo9 != null)
						{
							outpostModuleInfo9.SetAllowedLocationTypes(CS$<>8__locals1.locationTypeDropDown.SelectedDataMultiple.Cast<Identifier>());
						}
					}
				}
				CS$<>8__locals1.locationTypeDropDown.Text = ToolBox.LimitString(CS$<>8__locals1.locationTypeDropDown.Text.Value, CS$<>8__locals1.locationTypeDropDown.Font, CS$<>8__locals1.locationTypeDropDown.Rect.Width);
				return true;
			}));
			locationTypeGroup.RectTransform.MinSize = new Point(0, locationTypeGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			Vector2 relativeSize34 = new Vector2(0.975f, 0.1f);
			RectTransform rectTransform38 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			Anchor anchor37 = Anchor.TopLeft;
			Pivot? pivot37 = null;
			point = null;
			Point? minSize38 = point;
			point = null;
			GUILayoutGroup gapPositionGroup = new GUILayoutGroup(new RectTransform(relativeSize34, rectTransform38, anchor37, pivot37, minSize38, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
			Vector2 relativeSize35 = new Vector2(0.5f, 1f);
			RectTransform rectTransform39 = gapPositionGroup.RectTransform;
			Anchor anchor38 = Anchor.TopLeft;
			Pivot? pivot38 = null;
			point = null;
			Point? minSize39 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize35, rectTransform39, anchor38, pivot38, minSize39, point, ScaleBasis.Normal), TextManager.Get("outpostmodulegappositions"), null, null, Alignment.CenterLeft, false, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals10 = CS$<>8__locals1;
			Vector2 relativeSize36 = new Vector2(0.5f, 1f);
			RectTransform rectTransform40 = gapPositionGroup.RectTransform;
			Anchor anchor39 = Anchor.TopLeft;
			Pivot? pivot39 = null;
			point = null;
			Point? minSize40 = point;
			point = null;
			CS$<>8__locals10.gapPositionDropDown = new GUIDropDown(new RectTransform(relativeSize36, rectTransform40, anchor39, pivot39, minSize40, point, ScaleBasis.Normal), "", 4, "", true, false, Alignment.CenterLeft, 1f);
			SubmarineInfo info9 = SubEditorScreen.MainSub.Info;
			OutpostModuleInfo outpostModuleInfo = (info9 != null) ? info9.OutpostModuleInfo : null;
			if (outpostModuleInfo != null)
			{
				if (outpostModuleInfo.GapPositions == OutpostModuleInfo.GapPosition.None)
				{
					outpostModuleInfo.DetermineGapPositions(SubEditorScreen.MainSub);
				}
				foreach (object obj2 in Enum.GetValues(typeof(OutpostModuleInfo.GapPosition)))
				{
					OutpostModuleInfo.GapPosition gapPos = (OutpostModuleInfo.GapPosition)obj2;
					if (gapPos != OutpostModuleInfo.GapPosition.None)
					{
						CS$<>8__locals1.gapPositionDropDown.AddItem(gapPos.ToString().Capitalize(), gapPos, null, null, null);
						if (outpostModuleInfo.GapPositions.HasFlag(gapPos))
						{
							CS$<>8__locals1.gapPositionDropDown.SelectItem(gapPos);
						}
					}
				}
			}
			GUIDropDown gapPositionDropDown = CS$<>8__locals1.gapPositionDropDown;
			gapPositionDropDown.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(gapPositionDropDown.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent _, object __)
			{
				SubmarineInfo info18 = SubEditorScreen.MainSub.Info;
				if (((info18 != null) ? info18.OutpostModuleInfo : null) == null)
				{
					return false;
				}
				SubEditorScreen.MainSub.Info.OutpostModuleInfo.GapPositions = OutpostModuleInfo.GapPosition.None;
				if (CS$<>8__locals1.gapPositionDropDown.SelectedDataMultiple.Any<object>())
				{
					List<LocalizedString> gapPosTexts = new List<LocalizedString>();
					foreach (object obj5 in CS$<>8__locals1.gapPositionDropDown.SelectedDataMultiple)
					{
						OutpostModuleInfo.GapPosition gapPos3 = (OutpostModuleInfo.GapPosition)obj5;
						SubEditorScreen.MainSub.Info.OutpostModuleInfo.GapPositions |= gapPos3;
						gapPosTexts.Add(gapPos3.ToString().Capitalize());
					}
					CS$<>8__locals1.gapPositionDropDown.Text = ToolBox.LimitString(string.Join<LocalizedString>(", ", gapPosTexts), CS$<>8__locals1.gapPositionDropDown.Font, CS$<>8__locals1.gapPositionDropDown.Rect.Width);
				}
				else
				{
					CS$<>8__locals1.gapPositionDropDown.Text = ToolBox.LimitString("None", CS$<>8__locals1.gapPositionDropDown.Font, CS$<>8__locals1.gapPositionDropDown.Rect.Width);
				}
				return true;
			}));
			gapPositionGroup.RectTransform.MinSize = new Point(0, gapPositionGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			Vector2 relativeSize37 = new Vector2(0.975f, 0.1f);
			RectTransform rectTransform41 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			Anchor anchor40 = Anchor.TopLeft;
			Pivot? pivot40 = null;
			point = null;
			Point? minSize41 = point;
			point = null;
			GUILayoutGroup canAttachToPrevGroup = new GUILayoutGroup(new RectTransform(relativeSize37, rectTransform41, anchor40, pivot40, minSize41, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
			Vector2 relativeSize38 = new Vector2(0.5f, 1f);
			RectTransform rectTransform42 = canAttachToPrevGroup.RectTransform;
			Anchor anchor41 = Anchor.TopLeft;
			Pivot? pivot41 = null;
			point = null;
			Point? minSize42 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize38, rectTransform42, anchor41, pivot41, minSize42, point, ScaleBasis.Normal), TextManager.Get("canattachtoprevious"), null, null, Alignment.CenterLeft, false, "", null).ToolTip = TextManager.Get("canattachtoprevious.tooltip");
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals11 = CS$<>8__locals1;
			Vector2 relativeSize39 = new Vector2(0.5f, 1f);
			RectTransform rectTransform43 = canAttachToPrevGroup.RectTransform;
			Anchor anchor42 = Anchor.TopLeft;
			Pivot? pivot42 = null;
			point = null;
			Point? minSize43 = point;
			point = null;
			CS$<>8__locals11.canAttachToPrevDropDown = new GUIDropDown(new RectTransform(relativeSize39, rectTransform43, anchor42, pivot42, minSize43, point, ScaleBasis.Normal), "", 4, "", true, false, Alignment.CenterLeft, 1f);
			if (outpostModuleInfo != null)
			{
				foreach (object obj3 in Enum.GetValues(typeof(OutpostModuleInfo.GapPosition)))
				{
					OutpostModuleInfo.GapPosition gapPos2 = (OutpostModuleInfo.GapPosition)obj3;
					if (gapPos2 != OutpostModuleInfo.GapPosition.None)
					{
						CS$<>8__locals1.canAttachToPrevDropDown.AddItem(gapPos2.ToString().Capitalize(), gapPos2, null, null, null);
						if (outpostModuleInfo.CanAttachToPrevious.HasFlag(gapPos2))
						{
							CS$<>8__locals1.canAttachToPrevDropDown.SelectItem(gapPos2);
						}
					}
				}
			}
			GUIDropDown canAttachToPrevDropDown = CS$<>8__locals1.canAttachToPrevDropDown;
			canAttachToPrevDropDown.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(canAttachToPrevDropDown.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent _, object __)
			{
				SubmarineInfo info18 = Submarine.MainSub.Info;
				if (((info18 != null) ? info18.OutpostModuleInfo : null) == null)
				{
					return false;
				}
				Submarine.MainSub.Info.OutpostModuleInfo.CanAttachToPrevious = OutpostModuleInfo.GapPosition.None;
				if (CS$<>8__locals1.canAttachToPrevDropDown.SelectedDataMultiple.Any<object>())
				{
					List<string> gapPosTexts = new List<string>();
					foreach (object obj5 in CS$<>8__locals1.canAttachToPrevDropDown.SelectedDataMultiple)
					{
						OutpostModuleInfo.GapPosition gapPos3 = (OutpostModuleInfo.GapPosition)obj5;
						Submarine.MainSub.Info.OutpostModuleInfo.CanAttachToPrevious |= gapPos3;
						gapPosTexts.Add(gapPos3.ToString().Capitalize().Value);
					}
					CS$<>8__locals1.canAttachToPrevDropDown.Text = ToolBox.LimitString(string.Join(", ", gapPosTexts), CS$<>8__locals1.canAttachToPrevDropDown.Font, CS$<>8__locals1.canAttachToPrevDropDown.Rect.Width);
				}
				else
				{
					CS$<>8__locals1.canAttachToPrevDropDown.Text = ToolBox.LimitString("None", CS$<>8__locals1.canAttachToPrevDropDown.Font, CS$<>8__locals1.canAttachToPrevDropDown.Rect.Width);
				}
				return true;
			}));
			canAttachToPrevGroup.RectTransform.MinSize = new Point(0, gapPositionGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			Vector2 relativeSize40 = new Vector2(1f, 0.05f);
			RectTransform rectTransform44 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			Anchor anchor43 = Anchor.TopLeft;
			Pivot? pivot43 = null;
			point = null;
			Point? minSize44 = point;
			point = null;
			GUILayoutGroup maxModuleCountGroup = new GUILayoutGroup(new RectTransform(relativeSize40, rectTransform44, anchor43, pivot43, minSize44, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize41 = new Vector2(0.6f, 1f);
			RectTransform rectTransform45 = maxModuleCountGroup.RectTransform;
			Anchor anchor44 = Anchor.TopLeft;
			Pivot? pivot44 = null;
			point = null;
			Point? minSize45 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize41, rectTransform45, anchor44, pivot44, minSize45, point, ScaleBasis.Normal), TextManager.Get("OutPostModuleMaxCount"), null, null, Alignment.CenterLeft, true, "", null).ToolTip = TextManager.Get("OutPostModuleMaxCountToolTip");
			Vector2 relativeSize42 = new Vector2(0.4f, 1f);
			RectTransform rectTransform46 = maxModuleCountGroup.RectTransform;
			Anchor anchor45 = Anchor.TopLeft;
			Pivot? pivot45 = null;
			point = null;
			Point? minSize46 = point;
			point = null;
			GUINumberInput guinumberInput = new GUINumberInput(new RectTransform(relativeSize42, rectTransform46, anchor45, pivot45, minSize46, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
			guinumberInput.ToolTip = TextManager.Get("OutPostModuleMaxCountToolTip");
			Submarine mainSub9 = SubEditorScreen.MainSub;
			int? num;
			if (mainSub9 == null)
			{
				num = null;
			}
			else
			{
				SubmarineInfo info10 = mainSub9.Info;
				if (info10 == null)
				{
					num = null;
				}
				else
				{
					OutpostModuleInfo outpostModuleInfo7 = info10.OutpostModuleInfo;
					num = ((outpostModuleInfo7 != null) ? new int?(outpostModuleInfo7.MaxCount) : null);
				}
			}
			int? num2 = num;
			guinumberInput.IntValue = num2.GetValueOrDefault(1000);
			guinumberInput.MinValueInt = new int?(0);
			guinumberInput.MaxValueInt = new int?(1000);
			guinumberInput.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				SubEditorScreen.MainSub.Info.OutpostModuleInfo.MaxCount = numberInput.IntValue;
			};
			maxModuleCountGroup.RectTransform.MinSize = new Point(0, maxModuleCountGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			Vector2 relativeSize43 = new Vector2(1f, 0.05f);
			RectTransform rectTransform47 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			Anchor anchor46 = Anchor.TopLeft;
			Pivot? pivot46 = null;
			point = null;
			Point? minSize47 = point;
			point = null;
			GUILayoutGroup commonnessGroup = new GUILayoutGroup(new RectTransform(relativeSize43, rectTransform47, anchor46, pivot46, minSize47, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize44 = new Vector2(0.6f, 1f);
			RectTransform rectTransform48 = commonnessGroup.RectTransform;
			Anchor anchor47 = Anchor.TopLeft;
			Pivot? pivot47 = null;
			point = null;
			Point? minSize48 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize44, rectTransform48, anchor47, pivot47, minSize48, point, ScaleBasis.Normal), TextManager.Get("subeditor.outpostcommonness"), null, null, Alignment.CenterLeft, true, "", null);
			Vector2 relativeSize45 = new Vector2(0.4f, 1f);
			RectTransform rectTransform49 = commonnessGroup.RectTransform;
			Anchor anchor48 = Anchor.TopLeft;
			Pivot? pivot48 = null;
			point = null;
			Point? minSize49 = point;
			point = null;
			GUINumberInput guinumberInput2 = new GUINumberInput(new RectTransform(relativeSize45, rectTransform49, anchor48, pivot48, minSize49, point, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
			Submarine mainSub10 = SubEditorScreen.MainSub;
			float? num3;
			if (mainSub10 == null)
			{
				num3 = null;
			}
			else
			{
				SubmarineInfo info11 = mainSub10.Info;
				if (info11 == null)
				{
					num3 = null;
				}
				else
				{
					OutpostModuleInfo outpostModuleInfo8 = info11.OutpostModuleInfo;
					num3 = ((outpostModuleInfo8 != null) ? new float?(outpostModuleInfo8.Commonness) : null);
				}
			}
			float? num4 = num3;
			guinumberInput2.FloatValue = num4.GetValueOrDefault(10f);
			guinumberInput2.MinValueFloat = new float?(0f);
			guinumberInput2.MaxValueFloat = new float?((float)100);
			guinumberInput2.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				SubEditorScreen.MainSub.Info.OutpostModuleInfo.Commonness = numberInput.FloatValue;
			};
			commonnessGroup.RectTransform.MinSize = new Point(0, commonnessGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform.MinSize = new Point(0, CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform.Children.Sum(delegate(RectTransform c)
			{
				if (!c.Children.Any<RectTransform>())
				{
					return 0;
				}
				return c.Children.Max((RectTransform c2) => c2.MinSize.Y);
			}));
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals12 = CS$<>8__locals1;
			Vector2 relativeSize46 = new Vector2(1f, 0.75f);
			RectTransform rectTransform50 = subTypeDependentSettingFrame.RectTransform;
			Anchor anchor49 = Anchor.TopLeft;
			Pivot? pivot49 = null;
			point = null;
			Point? minSize50 = point;
			point = null;
			CS$<>8__locals12.extraSettingsContainer = new GUILayoutGroup(new RectTransform(relativeSize46, rectTransform50, anchor49, pivot49, minSize50, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = true,
				Visible = false,
				Stretch = true
			};
			Submarine mainSub11 = SubEditorScreen.MainSub;
			ExtraSubmarineInfo extraSubInfo = SubEditorScreen.GetExtraSubmarineInfo((mainSub11 != null) ? mainSub11.Info : null);
			Vector2 relativeSize47 = new Vector2(1f, 0.25f);
			RectTransform rectTransform51 = CS$<>8__locals1.extraSettingsContainer.RectTransform;
			Anchor anchor50 = Anchor.TopLeft;
			Pivot? pivot50 = null;
			point = null;
			Point? minSize51 = point;
			point = null;
			GUILayoutGroup minDifficultyGroup = new GUILayoutGroup(new RectTransform(relativeSize47, rectTransform51, anchor50, pivot50, minSize51, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize48 = new Vector2(0.6f, 1f);
			RectTransform rectTransform52 = minDifficultyGroup.RectTransform;
			Anchor anchor51 = Anchor.TopLeft;
			Pivot? pivot51 = null;
			point = null;
			Point? minSize52 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize48, rectTransform52, anchor51, pivot51, minSize52, point, ScaleBasis.Normal), TextManager.Get("minleveldifficulty"), null, null, Alignment.CenterLeft, true, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals13 = CS$<>8__locals1;
			Vector2 relativeSize49 = new Vector2(0.4f, 1f);
			RectTransform rectTransform53 = minDifficultyGroup.RectTransform;
			Anchor anchor52 = Anchor.TopLeft;
			Pivot? pivot52 = null;
			point = null;
			Point? minSize53 = point;
			point = null;
			GUINumberInput guinumberInput3 = new GUINumberInput(new RectTransform(relativeSize49, rectTransform53, anchor52, pivot52, minSize53, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
			guinumberInput3.IntValue = (int)((extraSubInfo != null) ? extraSubInfo.MinLevelDifficulty : 0f);
			guinumberInput3.MinValueInt = new int?(0);
			guinumberInput3.MaxValueInt = new int?(100);
			guinumberInput3.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				Submarine mainSub25 = SubEditorScreen.MainSub;
				ExtraSubmarineInfo extraSubInfo2 = SubEditorScreen.GetExtraSubmarineInfo((mainSub25 != null) ? mainSub25.Info : null);
				if (extraSubInfo2 != null)
				{
					extraSubInfo2.MinLevelDifficulty = (float)numberInput.IntValue;
				}
			};
			CS$<>8__locals13.minLevelDifficultyInput = guinumberInput3;
			minDifficultyGroup.RectTransform.MaxSize = CS$<>8__locals1.minLevelDifficultyInput.TextBox.RectTransform.MaxSize;
			Vector2 relativeSize50 = new Vector2(1f, 0.25f);
			RectTransform rectTransform54 = CS$<>8__locals1.extraSettingsContainer.RectTransform;
			Anchor anchor53 = Anchor.TopLeft;
			Pivot? pivot53 = null;
			point = null;
			Point? minSize54 = point;
			point = null;
			GUILayoutGroup maxDifficultyGroup = new GUILayoutGroup(new RectTransform(relativeSize50, rectTransform54, anchor53, pivot53, minSize54, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize51 = new Vector2(0.6f, 1f);
			RectTransform rectTransform55 = maxDifficultyGroup.RectTransform;
			Anchor anchor54 = Anchor.TopLeft;
			Pivot? pivot54 = null;
			point = null;
			Point? minSize55 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize51, rectTransform55, anchor54, pivot54, minSize55, point, ScaleBasis.Normal), TextManager.Get("maxleveldifficulty"), null, null, Alignment.CenterLeft, true, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals14 = CS$<>8__locals1;
			Vector2 relativeSize52 = new Vector2(0.4f, 1f);
			RectTransform rectTransform56 = maxDifficultyGroup.RectTransform;
			Anchor anchor55 = Anchor.TopLeft;
			Pivot? pivot55 = null;
			point = null;
			Point? minSize56 = point;
			point = null;
			GUINumberInput guinumberInput4 = new GUINumberInput(new RectTransform(relativeSize52, rectTransform56, anchor55, pivot55, minSize56, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
			guinumberInput4.IntValue = (int)((extraSubInfo != null) ? extraSubInfo.MaxLevelDifficulty : 100f);
			guinumberInput4.MinValueInt = new int?(0);
			guinumberInput4.MaxValueInt = new int?(100);
			guinumberInput4.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				Submarine mainSub25 = SubEditorScreen.MainSub;
				ExtraSubmarineInfo extraSubInfo2 = SubEditorScreen.GetExtraSubmarineInfo((mainSub25 != null) ? mainSub25.Info : null);
				if (extraSubInfo2 != null)
				{
					extraSubInfo2.MaxLevelDifficulty = (float)numberInput.IntValue;
				}
			};
			CS$<>8__locals14.maxLevelDifficultyInput = guinumberInput4;
			maxDifficultyGroup.RectTransform.MaxSize = CS$<>8__locals1.maxLevelDifficultyInput.TextBox.RectTransform.MaxSize;
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals15 = CS$<>8__locals1;
			GUIComponent extraSettingsContainer = CS$<>8__locals1.extraSettingsContainer;
			IEnumerable<Identifier> enumerable4 = (extraSubInfo != null) ? extraSubInfo.MissionTags : null;
			CS$<>8__locals15.missionTagsBox = SubEditorScreen.CreateMissionTagsUI(extraSettingsContainer, enumerable4 ?? Enumerable.Empty<Identifier>(), new GUITextBox.OnEnterHandler(this.ChangeMissionTags));
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals16 = CS$<>8__locals1;
			Vector2 one4 = Vector2.One;
			RectTransform rectTransform57 = subTypeDependentSettingFrame.RectTransform;
			Anchor anchor56 = Anchor.TopLeft;
			Pivot? pivot56 = null;
			point = null;
			Point? minSize57 = point;
			point = null;
			CS$<>8__locals16.outpostSettingsContainer = new GUILayoutGroup(new RectTransform(one4, rectTransform57, anchor56, pivot56, minSize57, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = true,
				Visible = false,
				Stretch = true
			};
			Vector2 relativeSize53 = new Vector2(1f, 0.25f);
			RectTransform rectTransform58 = CS$<>8__locals1.outpostSettingsContainer.RectTransform;
			Anchor anchor57 = Anchor.TopLeft;
			Pivot? pivot57 = null;
			point = null;
			Point? minSize58 = point;
			point = null;
			GUILayoutGroup outpostTagsGroup = new GUILayoutGroup(new RectTransform(relativeSize53, rectTransform58, anchor57, pivot57, minSize58, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize54 = new Vector2(0.6f, 1f);
			RectTransform rectTransform59 = outpostTagsGroup.RectTransform;
			Anchor anchor58 = Anchor.TopLeft;
			Pivot? pivot58 = null;
			point = null;
			Point? minSize59 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize54, rectTransform59, anchor58, pivot58, minSize59, point, ScaleBasis.Normal), TextManager.Get("sp.item.tags.name"), null, null, Alignment.CenterLeft, true, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals17 = CS$<>8__locals1;
			Vector2 relativeSize55 = new Vector2(0.4f, 1f);
			RectTransform rectTransform60 = outpostTagsGroup.RectTransform;
			Anchor anchor59 = Anchor.TopLeft;
			Pivot? pivot59 = null;
			point = null;
			Point? minSize60 = point;
			point = null;
			GUITextBox guitextBox2 = new GUITextBox(new RectTransform(relativeSize55, rectTransform60, anchor59, pivot59, minSize60, point, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			guitextBox2.OnEnterPressed = delegate(GUITextBox textBox, string text)
			{
				SubEditorScreen.MainSub.Info.OutpostTags = text.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
				return true;
			};
			guitextBox2.OverflowClip = true;
			guitextBox2.Text = "default";
			CS$<>8__locals17.outpostTagsBox = guitextBox2;
			CS$<>8__locals1.outpostTagsBox.OnDeselected += delegate(GUITextBox textbox, Keys _)
			{
				SubEditorScreen.MainSub.Info.OutpostTags = CS$<>8__locals1.outpostTagsBox.Text.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
			};
			if (SubEditorScreen.MainSub.Info.OutpostTags != null)
			{
				CS$<>8__locals1.outpostTagsBox.Text = SubEditorScreen.MainSub.Info.OutpostTags.ConvertToString(",");
			}
			outpostTagsGroup.RectTransform.MaxSize = CS$<>8__locals1.outpostTagsBox.RectTransform.MaxSize;
			Vector2 relativeSize56 = new Vector2(1f, 0.25f);
			RectTransform rectTransform61 = CS$<>8__locals1.outpostSettingsContainer.RectTransform;
			Anchor anchor60 = Anchor.TopLeft;
			Pivot? pivot60 = null;
			point = null;
			Point? minSize61 = point;
			point = null;
			GUILayoutGroup triggerMissionTagsGroup = new GUILayoutGroup(new RectTransform(relativeSize56, rectTransform61, anchor60, pivot60, minSize61, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize57 = new Vector2(0.6f, 1f);
			RectTransform rectTransform62 = triggerMissionTagsGroup.RectTransform;
			Anchor anchor61 = Anchor.TopLeft;
			Pivot? pivot61 = null;
			point = null;
			Point? minSize62 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize57, rectTransform62, anchor61, pivot61, minSize62, point, ScaleBasis.Normal), TextManager.Get("outpost.triggeroutpostmissionevents"), null, null, Alignment.CenterLeft, true, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals18 = CS$<>8__locals1;
			Vector2 relativeSize58 = new Vector2(0.4f, 1f);
			RectTransform rectTransform63 = triggerMissionTagsGroup.RectTransform;
			Anchor anchor62 = Anchor.TopLeft;
			Pivot? pivot62 = null;
			point = null;
			Point? minSize63 = point;
			point = null;
			GUITextBox guitextBox3 = new GUITextBox(new RectTransform(relativeSize58, rectTransform63, anchor62, pivot62, minSize63, point, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			guitextBox3.OnEnterPressed = delegate(GUITextBox textBox, string text)
			{
				SubEditorScreen.MainSub.Info.TriggerOutpostMissionEvents = text.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
				return true;
			};
			guitextBox3.ToolTip = TextManager.Get("outpost.triggeroutpostmissionevents.tooltip");
			guitextBox3.OverflowClip = true;
			guitextBox3.Text = "default";
			CS$<>8__locals18.triggerMissionTagsBox = guitextBox3;
			CS$<>8__locals1.triggerMissionTagsBox.OnDeselected += delegate(GUITextBox textbox, Keys _)
			{
				SubEditorScreen.MainSub.Info.TriggerOutpostMissionEvents = CS$<>8__locals1.triggerMissionTagsBox.Text.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
			};
			if (SubEditorScreen.MainSub.Info.TriggerOutpostMissionEvents != null)
			{
				CS$<>8__locals1.triggerMissionTagsBox.Text = SubEditorScreen.MainSub.Info.TriggerOutpostMissionEvents.ConvertToString(",");
			}
			triggerMissionTagsGroup.RectTransform.MaxSize = CS$<>8__locals1.triggerMissionTagsBox.RectTransform.MaxSize;
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals19 = CS$<>8__locals1;
			Vector2 one5 = Vector2.One;
			RectTransform rectTransform64 = CS$<>8__locals1.extraSettingsContainer.RectTransform;
			Anchor anchor63 = Anchor.TopLeft;
			Pivot? pivot63 = null;
			point = null;
			Point? minSize64 = point;
			point = null;
			CS$<>8__locals19.enemySubmarineSettingsContainer = new GUILayoutGroup(new RectTransform(one5, rectTransform64, anchor63, pivot63, minSize64, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = true,
				Visible = false,
				Stretch = true
			};
			Vector2 relativeSize59 = new Vector2(1f, 0.25f);
			RectTransform rectTransform65 = CS$<>8__locals1.enemySubmarineSettingsContainer.RectTransform;
			Anchor anchor64 = Anchor.TopLeft;
			Pivot? pivot64 = null;
			point = null;
			Point? minSize65 = point;
			point = null;
			GUILayoutGroup enemySubmarineRewardGroup = new GUILayoutGroup(new RectTransform(relativeSize59, rectTransform65, anchor64, pivot64, minSize65, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize60 = new Vector2(0.6f, 1f);
			RectTransform rectTransform66 = enemySubmarineRewardGroup.RectTransform;
			Anchor anchor65 = Anchor.TopLeft;
			Pivot? pivot65 = null;
			point = null;
			Point? minSize66 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize60, rectTransform66, anchor65, pivot65, minSize66, point, ScaleBasis.Normal), TextManager.Get("enemysub.reward"), null, null, Alignment.CenterLeft, true, "", null);
			Vector2 relativeSize61 = new Vector2(0.4f, 1f);
			RectTransform rectTransform67 = enemySubmarineRewardGroup.RectTransform;
			Anchor anchor66 = Anchor.TopLeft;
			Pivot? pivot66 = null;
			point = null;
			Point? minSize67 = point;
			point = null;
			GUINumberInput guinumberInput5 = new GUINumberInput(new RectTransform(relativeSize61, rectTransform67, anchor66, pivot66, minSize67, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.ForceHidden, null);
			Submarine mainSub12 = SubEditorScreen.MainSub;
			float? num5;
			if (mainSub12 == null)
			{
				num5 = null;
			}
			else
			{
				SubmarineInfo info12 = mainSub12.Info;
				if (info12 == null)
				{
					num5 = null;
				}
				else
				{
					EnemySubmarineInfo enemySubmarineInfo = info12.EnemySubmarineInfo;
					num5 = ((enemySubmarineInfo != null) ? new float?(enemySubmarineInfo.Reward) : null);
				}
			}
			num4 = num5;
			guinumberInput5.IntValue = (int)num4.GetValueOrDefault(4000f);
			guinumberInput5.MinValueInt = new int?(0);
			guinumberInput5.MaxValueInt = new int?(999999);
			guinumberInput5.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				SubEditorScreen.MainSub.Info.EnemySubmarineInfo.Reward = (float)numberInput.IntValue;
			};
			GUINumberInput enemySubRewardInput = guinumberInput5;
			enemySubmarineRewardGroup.RectTransform.MaxSize = enemySubRewardInput.TextBox.RectTransform.MaxSize;
			Vector2 relativeSize62 = new Vector2(1f, 0.25f);
			RectTransform rectTransform68 = CS$<>8__locals1.enemySubmarineSettingsContainer.RectTransform;
			Anchor anchor67 = Anchor.TopLeft;
			Pivot? pivot67 = null;
			point = null;
			Point? minSize68 = point;
			point = null;
			GUILayoutGroup enemySubmarineDifficultyGroup = new GUILayoutGroup(new RectTransform(relativeSize62, rectTransform68, anchor67, pivot67, minSize68, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize63 = new Vector2(0.6f, 1f);
			RectTransform rectTransform69 = enemySubmarineDifficultyGroup.RectTransform;
			Anchor anchor68 = Anchor.TopLeft;
			Pivot? pivot68 = null;
			point = null;
			Point? minSize69 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize63, rectTransform69, anchor68, pivot68, minSize69, point, ScaleBasis.Normal), TextManager.Get("preferreddifficulty"), null, null, Alignment.CenterLeft, true, "", null);
			Vector2 relativeSize64 = new Vector2(0.4f, 1f);
			RectTransform rectTransform70 = enemySubmarineDifficultyGroup.RectTransform;
			Anchor anchor69 = Anchor.TopLeft;
			Pivot? pivot69 = null;
			point = null;
			Point? minSize70 = point;
			point = null;
			GUINumberInput guinumberInput6 = new GUINumberInput(new RectTransform(relativeSize64, rectTransform70, anchor69, pivot69, minSize70, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
			Submarine mainSub13 = SubEditorScreen.MainSub;
			float? num6;
			if (mainSub13 == null)
			{
				num6 = null;
			}
			else
			{
				SubmarineInfo info13 = mainSub13.Info;
				if (info13 == null)
				{
					num6 = null;
				}
				else
				{
					EnemySubmarineInfo enemySubmarineInfo2 = info13.EnemySubmarineInfo;
					num6 = ((enemySubmarineInfo2 != null) ? new float?(enemySubmarineInfo2.PreferredDifficulty) : null);
				}
			}
			num4 = num6;
			guinumberInput6.IntValue = (int)num4.GetValueOrDefault(50f);
			guinumberInput6.MinValueInt = new int?(0);
			guinumberInput6.MaxValueInt = new int?(100);
			guinumberInput6.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				SubEditorScreen.MainSub.Info.EnemySubmarineInfo.PreferredDifficulty = (float)numberInput.IntValue;
			};
			GUINumberInput enemySubDifficultyInput = guinumberInput6;
			enemySubmarineDifficultyGroup.RectTransform.MaxSize = enemySubDifficultyInput.TextBox.RectTransform.MaxSize;
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals20 = CS$<>8__locals1;
			Vector2 one6 = Vector2.One;
			RectTransform rectTransform71 = CS$<>8__locals1.extraSettingsContainer.RectTransform;
			Anchor anchor70 = Anchor.TopLeft;
			Pivot? pivot70 = null;
			point = null;
			Point? minSize71 = point;
			point = null;
			CS$<>8__locals20.beaconSettingsContainer = new GUILayoutGroup(new RectTransform(one6, rectTransform71, anchor70, pivot70, minSize71, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				CanBeFocused = true,
				Visible = false,
				Stretch = true
			};
			Vector2 relativeSize65 = new Vector2(1f, 0.25f);
			RectTransform rectTransform72 = CS$<>8__locals1.beaconSettingsContainer.RectTransform;
			Anchor anchor71 = Anchor.TopLeft;
			Pivot? pivot71 = null;
			point = null;
			Point? minSize72 = point;
			point = null;
			GUITickBox guitickBox = new GUITickBox(new RectTransform(relativeSize65, rectTransform72, anchor71, pivot71, minSize72, point, ScaleBasis.Normal), TextManager.Get("allowdamagedwalls"), null, "");
			Submarine mainSub14 = SubEditorScreen.MainSub;
			bool? flag11;
			if (mainSub14 == null)
			{
				flag11 = null;
			}
			else
			{
				SubmarineInfo info14 = mainSub14.Info;
				if (info14 == null)
				{
					flag11 = null;
				}
				else
				{
					BeaconStationInfo beaconStationInfo = info14.BeaconStationInfo;
					flag11 = ((beaconStationInfo != null) ? new bool?(beaconStationInfo.AllowDamagedWalls) : null);
				}
			}
			flag10 = flag11;
			guitickBox.Selected = flag10.GetValueOrDefault(true);
			guitickBox.OnSelected = delegate(GUITickBox tb)
			{
				SubEditorScreen.MainSub.Info.BeaconStationInfo.AllowDamagedWalls = tb.Selected;
				return true;
			};
			Vector2 relativeSize66 = new Vector2(1f, 0.25f);
			RectTransform rectTransform73 = CS$<>8__locals1.beaconSettingsContainer.RectTransform;
			Anchor anchor72 = Anchor.TopLeft;
			Pivot? pivot72 = null;
			point = null;
			Point? minSize73 = point;
			point = null;
			GUITickBox guitickBox2 = new GUITickBox(new RectTransform(relativeSize66, rectTransform73, anchor72, pivot72, minSize73, point, ScaleBasis.Normal), TextManager.Get("allowdamageddevices"), null, "");
			Submarine mainSub15 = SubEditorScreen.MainSub;
			bool? flag12;
			if (mainSub15 == null)
			{
				flag12 = null;
			}
			else
			{
				SubmarineInfo info15 = mainSub15.Info;
				if (info15 == null)
				{
					flag12 = null;
				}
				else
				{
					BeaconStationInfo beaconStationInfo2 = info15.BeaconStationInfo;
					flag12 = ((beaconStationInfo2 != null) ? new bool?(beaconStationInfo2.AllowDamagedDevices) : null);
				}
			}
			flag10 = flag12;
			guitickBox2.Selected = flag10.GetValueOrDefault(true);
			guitickBox2.OnSelected = delegate(GUITickBox tb)
			{
				SubEditorScreen.MainSub.Info.BeaconStationInfo.AllowDamagedDevices = tb.Selected;
				return true;
			};
			Vector2 relativeSize67 = new Vector2(1f, 0.25f);
			RectTransform rectTransform74 = CS$<>8__locals1.beaconSettingsContainer.RectTransform;
			Anchor anchor73 = Anchor.TopLeft;
			Pivot? pivot73 = null;
			point = null;
			Point? minSize74 = point;
			point = null;
			GUITickBox guitickBox3 = new GUITickBox(new RectTransform(relativeSize67, rectTransform74, anchor73, pivot73, minSize74, point, ScaleBasis.Normal), TextManager.Get("allowdisconnectedwires"), null, "");
			Submarine mainSub16 = SubEditorScreen.MainSub;
			bool? flag13;
			if (mainSub16 == null)
			{
				flag13 = null;
			}
			else
			{
				SubmarineInfo info16 = mainSub16.Info;
				if (info16 == null)
				{
					flag13 = null;
				}
				else
				{
					BeaconStationInfo beaconStationInfo3 = info16.BeaconStationInfo;
					flag13 = ((beaconStationInfo3 != null) ? new bool?(beaconStationInfo3.AllowDisconnectedWires) : null);
				}
			}
			flag10 = flag13;
			guitickBox3.Selected = flag10.GetValueOrDefault(true);
			guitickBox3.OnSelected = delegate(GUITickBox tb)
			{
				SubEditorScreen.MainSub.Info.BeaconStationInfo.AllowDisconnectedWires = tb.Selected;
				return true;
			};
			Vector2 relativeSize68 = new Vector2(1f, 0.25f);
			RectTransform rectTransform75 = CS$<>8__locals1.beaconSettingsContainer.RectTransform;
			Anchor anchor74 = Anchor.TopLeft;
			Pivot? pivot74 = null;
			point = null;
			Point? minSize75 = point;
			point = null;
			GUITickBox guitickBox4 = new GUITickBox(new RectTransform(relativeSize68, rectTransform75, anchor74, pivot74, minSize75, point, ScaleBasis.Normal), TextManager.Get("beaconstationplacement"), null, "");
			BeaconStationInfo beaconStationInfo4 = SubEditorScreen.MainSub.Info.BeaconStationInfo;
			guitickBox4.Selected = (beaconStationInfo4 != null && beaconStationInfo4.Placement == Level.PlacementType.Top);
			guitickBox4.OnSelected = delegate(GUITickBox tb)
			{
				SubEditorScreen.MainSub.Info.BeaconStationInfo.Placement = (tb.Selected ? Level.PlacementType.Top : Level.PlacementType.Bottom);
				return true;
			};
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals21 = CS$<>8__locals1;
			Vector2 one7 = Vector2.One;
			RectTransform rectTransform76 = subTypeDependentSettingFrame.RectTransform;
			Anchor anchor75 = Anchor.TopLeft;
			Pivot? pivot75 = null;
			point = null;
			Point? minSize76 = point;
			point = null;
			CS$<>8__locals21.subSettingsContainer = new GUILayoutGroup(new RectTransform(one7, rectTransform76, anchor75, pivot75, minSize76, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize69 = new Vector2(1f, 0.05f);
			RectTransform rectTransform77 = CS$<>8__locals1.subSettingsContainer.RectTransform;
			Anchor anchor76 = Anchor.TopLeft;
			Pivot? pivot76 = null;
			point = null;
			Point? minSize77 = point;
			point = null;
			GUILayoutGroup priceGroup = new GUILayoutGroup(new RectTransform(relativeSize69, rectTransform77, anchor76, pivot76, minSize77, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize70 = new Vector2(0.6f, 1f);
			RectTransform rectTransform78 = priceGroup.RectTransform;
			Anchor anchor77 = Anchor.TopLeft;
			Pivot? pivot77 = null;
			point = null;
			Point? minSize78 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize70, rectTransform78, anchor77, pivot77, minSize78, point, ScaleBasis.Normal), TextManager.Get("subeditor.price"), null, null, Alignment.CenterLeft, true, "", null);
			int? num7;
			if (!GameMain.DebugDraw)
			{
				Submarine mainSub17 = SubEditorScreen.MainSub;
				num7 = ((mainSub17 != null) ? new int?(mainSub17.CalculateBasePrice()) : null);
			}
			else
			{
				num7 = new int?(0);
			}
			num2 = num7;
			int basePrice = num2.GetValueOrDefault(1000);
			Vector2 relativeSize71 = new Vector2(0.4f, 1f);
			RectTransform rectTransform79 = priceGroup.RectTransform;
			Anchor anchor78 = Anchor.TopLeft;
			Pivot? pivot78 = null;
			point = null;
			Point? minSize79 = point;
			point = null;
			GUINumberInput guinumberInput7 = new GUINumberInput(new RectTransform(relativeSize71, rectTransform79, anchor78, pivot78, minSize79, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.ForceHidden, null);
			Submarine mainSub18 = SubEditorScreen.MainSub;
			int? num8;
			if (mainSub18 == null)
			{
				num8 = null;
			}
			else
			{
				SubmarineInfo info17 = mainSub18.Info;
				num8 = ((info17 != null) ? new int?(info17.Price) : null);
			}
			num2 = num8;
			guinumberInput7.IntValue = Math.Max(num2.GetValueOrDefault(basePrice), basePrice);
			guinumberInput7.MinValueInt = new int?(basePrice);
			guinumberInput7.MaxValueInt = new int?(999999);
			guinumberInput7.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				SubEditorScreen.MainSub.Info.Price = numberInput.IntValue;
			};
			Submarine mainSub19 = SubEditorScreen.MainSub;
			if (((mainSub19 != null) ? mainSub19.Info : null) != null)
			{
				SubEditorScreen.MainSub.Info.Price = Math.Max(SubEditorScreen.MainSub.Info.Price, basePrice);
			}
			Vector2 relativeSize72 = new Vector2(1f, 0.05f);
			RectTransform rectTransform80 = CS$<>8__locals1.subSettingsContainer.RectTransform;
			Anchor anchor79 = Anchor.TopLeft;
			Pivot? pivot79 = null;
			point = null;
			Point? minSize80 = point;
			point = null;
			GUILayoutGroup classGroup = new GUILayoutGroup(new RectTransform(relativeSize72, rectTransform80, anchor79, pivot79, minSize80, point, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize73 = new Vector2(0.6f, 1f);
			RectTransform rectTransform81 = classGroup.RectTransform;
			Anchor anchor80 = Anchor.TopLeft;
			Pivot? pivot80 = null;
			point = null;
			Point? minSize81 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize73, rectTransform81, anchor80, pivot80, minSize81, point, ScaleBasis.Normal), TextManager.Get("submarineclass"), null, null, Alignment.CenterLeft, true, "", null).ToolTip = TextManager.Get("submarineclass.description");
			Vector2 relativeSize74 = new Vector2(0.4f, 1f);
			RectTransform rectTransform82 = classGroup.RectTransform;
			Anchor anchor81 = Anchor.TopLeft;
			Pivot? pivot81 = null;
			point = null;
			Point? minSize82 = point;
			point = null;
			GUIDropDown classDropDown = new GUIDropDown(new RectTransform(relativeSize74, rectTransform82, anchor81, pivot81, minSize82, point, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			classDropDown.RectTransform.MinSize = new Point(0, subTypeContainer.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			foreach (object obj4 in Enum.GetValues(typeof(SubmarineClass)))
			{
				SubmarineClass subClass = (SubmarineClass)obj4;
				GUIDropDown guidropDown = classDropDown;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted("SubmarineClass");
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<SubmarineClass>(subClass);
				LocalizedString text4 = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
				object userData2 = subClass;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(27, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("submarineclass.");
				defaultInterpolatedStringHandler2.AppendFormatted<SubmarineClass>(subClass);
				defaultInterpolatedStringHandler2.AppendLiteral(".description");
				guidropDown.AddItem(text4, userData2, TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()), null, null);
			}
			classDropDown.AddItem(TextManager.Get("Shuttle"), SubmarineTag.Shuttle, null, null, null);
			GUIDropDown guidropDown2 = classDropDown;
			guidropDown2.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(guidropDown2.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent selected, object userdata)
			{
				if (userdata is SubmarineClass)
				{
					SubmarineClass submarineClass = (SubmarineClass)userdata;
					SubEditorScreen.MainSub.Info.RemoveTag(SubmarineTag.Shuttle);
					SubEditorScreen.MainSub.Info.SubmarineClass = submarineClass;
				}
				else if (userdata is SubmarineTag)
				{
					SubmarineTag submarineTag = (SubmarineTag)userdata;
					if (submarineTag == SubmarineTag.Shuttle)
					{
						SubEditorScreen.MainSub.Info.AddTag(SubmarineTag.Shuttle);
						SubEditorScreen.MainSub.Info.SubmarineClass = SubmarineClass.Undefined;
					}
				}
				return true;
			}));
			classDropDown.SelectItem((!SubEditorScreen.MainSub.Info.HasTag(SubmarineTag.Shuttle)) ? SubEditorScreen.MainSub.Info.SubmarineClass : SubmarineTag.Shuttle);
			Vector2 relativeSize75 = new Vector2(1f, 0.05f);
			RectTransform rectTransform83 = CS$<>8__locals1.subSettingsContainer.RectTransform;
			Anchor anchor82 = Anchor.TopLeft;
			Pivot? pivot82 = null;
			point = null;
			Point? minSize83 = point;
			point = null;
			GUILayoutGroup tierGroup = new GUILayoutGroup(new RectTransform(relativeSize75, rectTransform83, anchor82, pivot82, minSize83, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 relativeSize76 = new Vector2(0.6f, 1f);
			RectTransform rectTransform84 = tierGroup.RectTransform;
			Anchor anchor83 = Anchor.TopLeft;
			Pivot? pivot83 = null;
			point = null;
			Point? minSize84 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize76, rectTransform84, anchor83, pivot83, minSize84, point, ScaleBasis.Normal), TextManager.Get("subeditor.tier"), null, null, Alignment.CenterLeft, true, "", null).ToolTip = TextManager.Get("submarinetier.description");
			Vector2 relativeSize77 = new Vector2(0.4f, 1f);
			RectTransform rectTransform85 = tierGroup.RectTransform;
			Anchor anchor84 = Anchor.TopLeft;
			Pivot? pivot84 = null;
			point = null;
			Point? minSize85 = point;
			point = null;
			GUINumberInput guinumberInput8 = new GUINumberInput(new RectTransform(relativeSize77, rectTransform85, anchor84, pivot84, minSize85, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
			guinumberInput8.IntValue = SubEditorScreen.MainSub.Info.Tier;
			guinumberInput8.MinValueInt = new int?(1);
			guinumberInput8.MaxValueInt = new int?(3);
			guinumberInput8.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				SubEditorScreen.MainSub.Info.Tier = numberInput.IntValue;
			};
			Submarine mainSub20 = SubEditorScreen.MainSub;
			if (((mainSub20 != null) ? mainSub20.Info : null) != null)
			{
				SubEditorScreen.MainSub.Info.Tier = Math.Clamp(SubEditorScreen.MainSub.Info.Tier, 1, 3);
			}
			Vector2 relativeSize78 = new Vector2(1f, 0.05f);
			RectTransform rectTransform86 = CS$<>8__locals1.subSettingsContainer.RectTransform;
			Anchor anchor85 = Anchor.TopLeft;
			Pivot? pivot85 = null;
			point = null;
			Point? minSize86 = point;
			point = null;
			GUILayoutGroup crewSizeArea = new GUILayoutGroup(new RectTransform(relativeSize78, rectTransform86, anchor85, pivot85, minSize86, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = 5
			};
			Vector2 relativeSize79 = new Vector2(0.6f, 1f);
			RectTransform rectTransform87 = crewSizeArea.RectTransform;
			Anchor anchor86 = Anchor.TopLeft;
			Pivot? pivot86 = null;
			point = null;
			Point? minSize87 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize79, rectTransform87, anchor86, pivot86, minSize87, point, ScaleBasis.Normal), TextManager.Get("RecommendedCrewSize"), null, null, Alignment.CenterLeft, true, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals22 = CS$<>8__locals1;
			Vector2 relativeSize80 = new Vector2(0.17f, 1f);
			RectTransform rectTransform88 = crewSizeArea.RectTransform;
			Anchor anchor87 = Anchor.TopLeft;
			Pivot? pivot87 = null;
			point = null;
			Point? minSize88 = point;
			point = null;
			CS$<>8__locals22.crewSizeMin = new GUINumberInput(new RectTransform(relativeSize80, rectTransform88, anchor87, pivot87, minSize88, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, new float?(0.25f), GUINumberInput.ButtonVisibility.Automatic, null)
			{
				MinValueInt = new int?(1),
				MaxValueInt = new int?(128)
			};
			Vector2 relativeSize81 = new Vector2(0.06f, 1f);
			RectTransform rectTransform89 = crewSizeArea.RectTransform;
			Anchor anchor88 = Anchor.TopLeft;
			Pivot? pivot88 = null;
			point = null;
			Point? minSize89 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize81, rectTransform89, anchor88, pivot88, minSize89, point, ScaleBasis.Normal), "-", null, null, Alignment.Center, false, "", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals23 = CS$<>8__locals1;
			Vector2 relativeSize82 = new Vector2(0.17f, 1f);
			RectTransform rectTransform90 = crewSizeArea.RectTransform;
			Anchor anchor89 = Anchor.TopLeft;
			Pivot? pivot89 = null;
			point = null;
			Point? minSize90 = point;
			point = null;
			CS$<>8__locals23.crewSizeMax = new GUINumberInput(new RectTransform(relativeSize82, rectTransform90, anchor89, pivot89, minSize90, point, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, new float?(0.25f), GUINumberInput.ButtonVisibility.Automatic, null)
			{
				MinValueInt = new int?(1),
				MaxValueInt = new int?(128)
			};
			GUINumberInput crewSizeMin = CS$<>8__locals1.crewSizeMin;
			crewSizeMin.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(crewSizeMin.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numberInput)
			{
				CS$<>8__locals1.crewSizeMax.IntValue = Math.Max(CS$<>8__locals1.crewSizeMax.IntValue, numberInput.IntValue);
				SubEditorScreen.MainSub.Info.RecommendedCrewSizeMin = CS$<>8__locals1.crewSizeMin.IntValue;
				SubEditorScreen.MainSub.Info.RecommendedCrewSizeMax = CS$<>8__locals1.crewSizeMax.IntValue;
			}));
			GUINumberInput crewSizeMax = CS$<>8__locals1.crewSizeMax;
			crewSizeMax.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(crewSizeMax.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numberInput)
			{
				CS$<>8__locals1.crewSizeMin.IntValue = Math.Min(CS$<>8__locals1.crewSizeMin.IntValue, numberInput.IntValue);
				SubEditorScreen.MainSub.Info.RecommendedCrewSizeMin = CS$<>8__locals1.crewSizeMin.IntValue;
				SubEditorScreen.MainSub.Info.RecommendedCrewSizeMax = CS$<>8__locals1.crewSizeMax.IntValue;
			}));
			Vector2 relativeSize83 = new Vector2(1f, 0.05f);
			RectTransform rectTransform91 = CS$<>8__locals1.subSettingsContainer.RectTransform;
			Anchor anchor90 = Anchor.TopLeft;
			Pivot? pivot90 = null;
			point = null;
			Point? minSize91 = point;
			point = null;
			GUILayoutGroup crewExpArea = new GUILayoutGroup(new RectTransform(relativeSize83, rectTransform91, anchor90, pivot90, minSize91, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = 5
			};
			Vector2 relativeSize84 = new Vector2(0.6f, 1f);
			RectTransform rectTransform92 = crewExpArea.RectTransform;
			Anchor anchor91 = Anchor.TopLeft;
			Pivot? pivot91 = null;
			point = null;
			Point? minSize92 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize84, rectTransform92, anchor91, pivot91, minSize92, point, ScaleBasis.Normal), TextManager.Get("RecommendedCrewExperience"), null, null, Alignment.CenterLeft, true, "", null);
			Vector2 relativeSize85 = new Vector2(0.05f, 1f);
			RectTransform rectTransform93 = crewExpArea.RectTransform;
			Anchor anchor92 = Anchor.TopLeft;
			Pivot? pivot92 = null;
			point = null;
			Point? minSize93 = point;
			point = null;
			GUIButton toggleExpLeft = new GUIButton(new RectTransform(relativeSize85, rectTransform93, anchor92, pivot92, minSize93, point, ScaleBasis.Normal), Alignment.Center, "GUIButtonToggleLeft", null);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals24 = CS$<>8__locals1;
			Vector2 relativeSize86 = new Vector2(0.3f, 1f);
			RectTransform rectTransform94 = crewExpArea.RectTransform;
			Anchor anchor93 = Anchor.TopLeft;
			Pivot? pivot93 = null;
			point = null;
			Point? minSize94 = point;
			point = null;
			CS$<>8__locals24.experienceText = new GUITextBlock(new RectTransform(relativeSize86, rectTransform94, anchor93, pivot93, minSize94, point, ScaleBasis.Normal), TextManager.Get(SubmarineInfo.CrewExperienceLevel.CrewExperienceLow.ToIdentifier<SubmarineInfo.CrewExperienceLevel>()), null, null, Alignment.Center, false, "", null);
			Vector2 relativeSize87 = new Vector2(0.05f, 1f);
			RectTransform rectTransform95 = crewExpArea.RectTransform;
			Anchor anchor94 = Anchor.TopLeft;
			Pivot? pivot94 = null;
			point = null;
			Point? minSize95 = point;
			point = null;
			GUIButton toggleExpRight = new GUIButton(new RectTransform(relativeSize87, rectTransform95, anchor94, pivot94, minSize95, point, ScaleBasis.Normal), Alignment.Center, "GUIButtonToggleRight", null);
			GUIButton guibutton = toggleExpLeft;
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userData)
			{
				SubEditorScreen.MainSub.Info.RecommendedCrewExperience--;
				if (SubEditorScreen.MainSub.Info.RecommendedCrewExperience < SubmarineInfo.CrewExperienceLevel.CrewExperienceLow)
				{
					SubEditorScreen.MainSub.Info.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceHigh;
				}
				CS$<>8__locals1.experienceText.Text = TextManager.Get(SubEditorScreen.MainSub.Info.RecommendedCrewExperience.ToIdentifier<SubmarineInfo.CrewExperienceLevel>());
				return true;
			}));
			GUIButton guibutton2 = toggleExpRight;
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userData)
			{
				SubEditorScreen.MainSub.Info.RecommendedCrewExperience++;
				if (SubEditorScreen.MainSub.Info.RecommendedCrewExperience > SubmarineInfo.CrewExperienceLevel.CrewExperienceHigh)
				{
					SubEditorScreen.MainSub.Info.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceLow;
				}
				CS$<>8__locals1.experienceText.Text = TextManager.Get(SubEditorScreen.MainSub.Info.RecommendedCrewExperience.ToIdentifier<SubmarineInfo.CrewExperienceLevel>());
				return true;
			}));
			Vector2 relativeSize88 = new Vector2(1f, 0.05f);
			RectTransform rectTransform96 = CS$<>8__locals1.subSettingsContainer.RectTransform;
			Anchor anchor95 = Anchor.TopLeft;
			Pivot? pivot95 = null;
			point = null;
			Point? minSize96 = point;
			point = null;
			GUILayoutGroup hideInMenusArea = new GUILayoutGroup(new RectTransform(relativeSize88, rectTransform96, anchor95, pivot95, minSize96, point, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				AbsoluteSpacing = 5
			};
			Vector2 relativeSize89 = new Vector2(0.6f, 1f);
			RectTransform rectTransform97 = hideInMenusArea.RectTransform;
			Anchor anchor96 = Anchor.TopLeft;
			Pivot? pivot96 = null;
			point = null;
			Point? minSize97 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize89, rectTransform97, anchor96, pivot96, minSize97, point, ScaleBasis.Normal), TextManager.Get("HideInMenus"), null, null, Alignment.CenterLeft, true, "", null);
			Vector2 relativeSize90 = new ValueTuple<float, float>(0.4f, 1f);
			RectTransform rectTransform98 = hideInMenusArea.RectTransform;
			Anchor anchor97 = Anchor.TopLeft;
			Pivot? pivot97 = null;
			point = null;
			Point? minSize98 = point;
			point = null;
			GUITickBox guitickBox5 = new GUITickBox(new RectTransform(relativeSize90, rectTransform98, anchor97, pivot97, minSize98, point, ScaleBasis.Normal), "", null, "");
			guitickBox5.Selected = SubEditorScreen.MainSub.Info.HasTag(SubmarineTag.HideInMenus);
			guitickBox5.OnSelected = delegate(GUITickBox box)
			{
				if (box.Selected)
				{
					SubEditorScreen.MainSub.Info.AddTag(SubmarineTag.HideInMenus);
				}
				else
				{
					SubEditorScreen.MainSub.Info.RemoveTag(SubmarineTag.HideInMenus);
				}
				return true;
			};
			Vector2 relativeSize91 = new Vector2(1f, 0.05f);
			RectTransform rectTransform99 = CS$<>8__locals1.subSettingsContainer.RectTransform;
			Anchor anchor98 = Anchor.TopLeft;
			Pivot? pivot98 = null;
			point = null;
			Point? minSize99 = point;
			point = null;
			GUILayoutGroup outFittingArea = new GUILayoutGroup(new RectTransform(relativeSize91, rectTransform99, anchor98, pivot98, minSize99, point, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				AbsoluteSpacing = 5
			};
			Vector2 relativeSize92 = new Vector2(0.6f, 1f);
			RectTransform rectTransform100 = outFittingArea.RectTransform;
			Anchor anchor99 = Anchor.TopLeft;
			Pivot? pivot99 = null;
			point = null;
			Point? minSize100 = point;
			point = null;
			new GUITextBlock(new RectTransform(relativeSize92, rectTransform100, anchor99, pivot99, minSize100, point, ScaleBasis.Normal), TextManager.Get("ManuallyOutfitted"), null, null, Alignment.CenterLeft, true, "", null).ToolTip = TextManager.Get("manuallyoutfittedtooltip");
			Vector2 relativeSize93 = new ValueTuple<float, float>(0.4f, 1f);
			RectTransform rectTransform101 = outFittingArea.RectTransform;
			Anchor anchor100 = Anchor.TopLeft;
			Pivot? pivot100 = null;
			point = null;
			Point? minSize101 = point;
			point = null;
			GUITickBox guitickBox6 = new GUITickBox(new RectTransform(relativeSize93, rectTransform101, anchor100, pivot100, minSize101, point, ScaleBasis.Normal), "", null, "");
			guitickBox6.ToolTip = TextManager.Get("manuallyoutfittedtooltip");
			guitickBox6.Selected = SubEditorScreen.MainSub.Info.IsManuallyOutfitted;
			guitickBox6.OnSelected = delegate(GUITickBox box)
			{
				SubEditorScreen.MainSub.Info.IsManuallyOutfitted = box.Selected;
				return true;
			};
			if (SubEditorScreen.MainSub != null)
			{
				int min = SubEditorScreen.MainSub.Info.RecommendedCrewSizeMin;
				int max = SubEditorScreen.MainSub.Info.RecommendedCrewSizeMax;
				CS$<>8__locals1.crewSizeMin.IntValue = min;
				CS$<>8__locals1.crewSizeMax.IntValue = max;
				if (SubEditorScreen.MainSub.Info.RecommendedCrewExperience == SubmarineInfo.CrewExperienceLevel.Unknown)
				{
					SubEditorScreen.MainSub.Info.RecommendedCrewExperience = SubmarineInfo.CrewExperienceLevel.CrewExperienceLow;
				}
				CS$<>8__locals1.experienceText.Text = TextManager.Get(SubEditorScreen.MainSub.Info.RecommendedCrewExperience.ToIdentifier<SubmarineInfo.CrewExperienceLevel>());
			}
			GUIDropDown guidropDown3 = subTypeDropdown;
			guidropDown3.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(guidropDown3.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent selected, object userdata)
			{
				SubmarineType type = (SubmarineType)userdata;
				SubEditorScreen.MainSub.Info.Type = type;
				if (type == SubmarineType.OutpostModule)
				{
					SubmarineInfo info18 = SubEditorScreen.MainSub.Info;
					if (info18.OutpostModuleInfo == null)
					{
						info18.OutpostModuleInfo = new OutpostModuleInfo(SubEditorScreen.MainSub.Info);
					}
				}
				else if (type == SubmarineType.BeaconStation)
				{
					SubmarineInfo info18 = SubEditorScreen.MainSub.Info;
					if (info18.BeaconStationInfo == null)
					{
						info18.BeaconStationInfo = new BeaconStationInfo(SubEditorScreen.MainSub.Info);
					}
				}
				else if (type == SubmarineType.Wreck)
				{
					SubmarineInfo info18 = SubEditorScreen.MainSub.Info;
					if (info18.WreckInfo == null)
					{
						info18.WreckInfo = new WreckInfo(SubEditorScreen.MainSub.Info);
					}
				}
				else if (type == SubmarineType.EnemySubmarine)
				{
					SubmarineInfo info18 = SubEditorScreen.MainSub.Info;
					if (info18.EnemySubmarineInfo == null)
					{
						info18.EnemySubmarineInfo = new EnemySubmarineInfo(SubEditorScreen.MainSub.Info);
					}
				}
				ExtraSubmarineInfo newExtraSubInfo = SubEditorScreen.GetExtraSubmarineInfo(SubEditorScreen.MainSub.Info);
				if (newExtraSubInfo != null)
				{
					CS$<>8__locals1.missionTagsBox.Text = string.Join<Identifier>(',', newExtraSubInfo.MissionTags);
					ExtraSubmarineInfo extraSubmarineInfo = newExtraSubInfo;
					GUINumberInput minLevelDifficultyInput = CS$<>8__locals1.minLevelDifficultyInput;
					extraSubmarineInfo.MinLevelDifficulty = (float)((minLevelDifficultyInput != null) ? minLevelDifficultyInput.IntValue : 0);
					ExtraSubmarineInfo extraSubmarineInfo2 = newExtraSubInfo;
					GUINumberInput maxLevelDifficultyInput = CS$<>8__locals1.maxLevelDifficultyInput;
					extraSubmarineInfo2.MaxLevelDifficulty = (float)((maxLevelDifficultyInput != null) ? maxLevelDifficultyInput.IntValue : 100);
				}
				CS$<>8__locals1.<>4__this.previewImageButtonHolder.Children.ForEach(delegate(GUIComponent c)
				{
					c.Enabled = SubEditorScreen.MainSub.Info.AllowPreviewImage;
				});
				CS$<>8__locals1.outpostModuleSettingsContainer.Visible = (type == SubmarineType.OutpostModule);
				CS$<>8__locals1.extraSettingsContainer.Visible = (newExtraSubInfo != null);
				CS$<>8__locals1.beaconSettingsContainer.Visible = (type == SubmarineType.BeaconStation);
				CS$<>8__locals1.beaconSettingsContainer.IgnoreLayoutGroups = !CS$<>8__locals1.beaconSettingsContainer.Visible;
				CS$<>8__locals1.enemySubmarineSettingsContainer.Visible = (type == SubmarineType.EnemySubmarine);
				CS$<>8__locals1.enemySubmarineSettingsContainer.IgnoreLayoutGroups = !CS$<>8__locals1.enemySubmarineSettingsContainer.Visible;
				CS$<>8__locals1.subSettingsContainer.Visible = (type == SubmarineType.Player);
				CS$<>8__locals1.outpostSettingsContainer.Visible = (type == SubmarineType.Outpost);
				CS$<>8__locals1.extraSettingsContainer.Recalculate();
				return true;
			}));
			CS$<>8__locals1.subSettingsContainer.RectTransform.MinSize = new Point(0, CS$<>8__locals1.subSettingsContainer.RectTransform.Children.Sum(delegate(RectTransform c)
			{
				if (!c.Children.Any<RectTransform>())
				{
					return 0;
				}
				return c.Children.Max((RectTransform c2) => c2.MinSize.Y);
			}));
			int minHeight = CS$<>8__locals1.subSettingsContainer.Children.First<GUIComponent>().Children.Max((GUIComponent c) => c.RectTransform.MinSize.Y);
			foreach (GUIComponent child in CS$<>8__locals1.subSettingsContainer.Children)
			{
				child.RectTransform.MinSize = new Point(0, minHeight);
			}
			Vector2 relativeSize94 = new Vector2(1f, 0f);
			RectTransform rectTransform102 = rightColumn.RectTransform;
			Anchor anchor101 = Anchor.TopLeft;
			Pivot? pivot101 = null;
			point = null;
			Point? minSize102 = point;
			point = null;
			RectTransform rectT7 = new RectTransform(relativeSize94, rectTransform102, anchor101, pivot101, minSize102, point, ScaleBasis.Normal);
			RichString text5 = TextManager.Get("SubPreviewImage");
			font = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT7, text5, null, font, Alignment.Left, false, "", null);
			Vector2 relativeSize95 = new Vector2(1f, 0.4f);
			RectTransform rectTransform103 = rightColumn.RectTransform;
			Anchor anchor102 = Anchor.TopLeft;
			Pivot? pivot102 = null;
			point = null;
			Point? minSize103 = point;
			point = null;
			GUIFrame previewImageHolder = new GUIFrame(new RectTransform(relativeSize95, rectTransform103, anchor102, pivot102, minSize103, point, ScaleBasis.Normal), null, null)
			{
				Color = Color.Black,
				CanBeFocused = false
			};
			Vector2 one8 = Vector2.One;
			RectTransform rectTransform104 = previewImageHolder.RectTransform;
			Anchor anchor103 = Anchor.TopLeft;
			Pivot? pivot103 = null;
			point = null;
			Point? minSize104 = point;
			point = null;
			RectTransform rectT8 = new RectTransform(one8, rectTransform104, anchor103, pivot103, minSize104, point, ScaleBasis.Normal);
			Submarine mainSub21 = SubEditorScreen.MainSub;
			this.previewImage = new GUIImage(rectT8, (mainSub21 != null) ? mainSub21.Info.PreviewImage : null, true, null);
			Vector2 relativeSize96 = new Vector2(1f, 0.05f);
			RectTransform rectTransform105 = rightColumn.RectTransform;
			Anchor anchor104 = Anchor.TopLeft;
			Pivot? pivot104 = null;
			point = null;
			Point? minSize105 = point;
			point = null;
			this.previewImageButtonHolder = new GUILayoutGroup(new RectTransform(relativeSize96, rectTransform105, anchor104, pivot104, minSize105, point, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.05f
			};
			Vector2 relativeSize97 = new Vector2(0.5f, 1f);
			RectTransform rectTransform106 = this.previewImageButtonHolder.RectTransform;
			Anchor anchor105 = Anchor.TopLeft;
			Pivot? pivot105 = null;
			point = null;
			Point? minSize106 = point;
			point = null;
			GUIButton guibutton3 = new GUIButton(new RectTransform(relativeSize97, rectTransform106, anchor105, pivot105, minSize106, point, ScaleBasis.Normal), TextManager.Get("SubPreviewImageCreate"), Alignment.Center, "GUIButtonSmall", null);
			Submarine mainSub22 = SubEditorScreen.MainSub;
			guibutton3.Enabled = (mainSub22 != null && mainSub22.Info.AllowPreviewImage);
			guibutton3.OnClicked = delegate(GUIButton btn, object userdata)
			{
				using (MemoryStream imgStream = new MemoryStream())
				{
					CS$<>8__locals1.<>4__this.CreateImage(CS$<>8__locals1.<>4__this.defaultPreviewImageSize.X, CS$<>8__locals1.<>4__this.defaultPreviewImageSize.Y, imgStream);
					CS$<>8__locals1.<>4__this.previewImage.Sprite = new Sprite(TextureLoader.FromStream(imgStream, null, false, false, null), null, null, 0f, null);
					if (SubEditorScreen.MainSub != null)
					{
						SubEditorScreen.MainSub.Info.PreviewImage = CS$<>8__locals1.<>4__this.previewImage.Sprite;
					}
				}
				return true;
			};
			Vector2 relativeSize98 = new Vector2(0.5f, 1f);
			RectTransform rectTransform107 = this.previewImageButtonHolder.RectTransform;
			Anchor anchor106 = Anchor.TopLeft;
			Pivot? pivot106 = null;
			point = null;
			Point? minSize107 = point;
			point = null;
			GUIButton guibutton4 = new GUIButton(new RectTransform(relativeSize98, rectTransform107, anchor106, pivot106, minSize107, point, ScaleBasis.Normal), TextManager.Get("SubPreviewImageBrowse"), Alignment.Center, "GUIButtonSmall", null);
			Submarine mainSub23 = SubEditorScreen.MainSub;
			guibutton4.Enabled = (mainSub23 != null && mainSub23.Info.AllowPreviewImage);
			guibutton4.OnClicked = delegate(GUIButton btn, object userdata)
			{
				Action<string> onFileSelected;
				if ((onFileSelected = CS$<>8__locals1.<>9__73) == null)
				{
					onFileSelected = (CS$<>8__locals1.<>9__73 = delegate(string file)
					{
						if (new FileInfo(file).Length > 4194304L)
						{
							new GUIMessageBox(TextManager.Get("Error"), TextManager.Get("WorkshopItemPreviewImageTooLarge"), null, null, GUIMessageBox.Type.Default);
							return;
						}
						CS$<>8__locals1.<>4__this.previewImage.Sprite = new Sprite(file, null, null, 0f);
						if (SubEditorScreen.MainSub != null)
						{
							SubEditorScreen.MainSub.Info.PreviewImage = CS$<>8__locals1.<>4__this.previewImage.Sprite;
						}
					});
				}
				FileSelection.OnFileSelected = onFileSelected;
				FileSelection.ClearFileTypeFilters();
				FileSelection.AddFileTypeFilter("PNG", "*.png");
				FileSelection.AddFileTypeFilter("JPEG", "*.jpg, *.jpeg");
				FileSelection.AddFileTypeFilter("All files", "*.*");
				FileSelection.SelectFileTypeFilter("*.png");
				FileSelection.Open = true;
				return false;
			};
			this.previewImageButtonHolder.RectTransform.MinSize = new Point(0, this.previewImageButtonHolder.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			if (SteamManager.IsInitialized)
			{
				Vector2 relativeSize99 = new Vector2(1f, 0.05f);
				RectTransform rectTransform108 = rightColumn.RectTransform;
				Anchor anchor107 = Anchor.TopLeft;
				point = new Point?(new Point(0, minHeight));
				GUILayoutGroup remoteStorageArea = new GUILayoutGroup(new RectTransform(relativeSize99, rectTransform108, anchor107, null, point, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					Stretch = true,
					AbsoluteSpacing = 5
				};
				Vector2 one9 = Vector2.One;
				RectTransform rectTransform109 = remoteStorageArea.RectTransform;
				Anchor anchor108 = Anchor.TopLeft;
				Pivot? pivot107 = null;
				point = null;
				Point? minSize108 = point;
				point = null;
				new GUITextBlock(new RectTransform(one9, rectTransform109, anchor108, pivot107, minSize108, point, ScaleBasis.Normal), TextManager.Get("RemoteStorageToggle.Title"), null, null, Alignment.CenterLeft, true, "", null);
				Vector2 one10 = Vector2.One;
				RectTransform rectTransform110 = remoteStorageArea.RectTransform;
				Anchor anchor109 = Anchor.TopLeft;
				Pivot? pivot108 = null;
				point = null;
				Point? minSize109 = point;
				point = null;
				GUITickBox guitickBox7 = new GUITickBox(new RectTransform(one10, rectTransform110, anchor109, pivot108, minSize109, point, ScaleBasis.Normal), "", null, "");
				guitickBox7.OnAddedToGUIUpdateList = delegate(GUIComponent component)
				{
					component.Enabled = SteamRemoteStorage.IsCloudEnabledForAccount;
					component.ToolTip = ((!SteamRemoteStorage.IsCloudEnabledForAccount) ? TextManager.Get("RemoteStorageToggle.Disabled") : "");
					((GUITickBox)component).SetSelected(SteamRemoteStorage.IsCloudEnabled && SubEditorScreen.MainSub.Info.SaveToRemoteStorage, false);
				};
				guitickBox7.OnSelected = delegate(GUITickBox tickBox)
				{
					if (tickBox.Selected && !SteamRemoteStorage.IsCloudEnabledForApp)
					{
						RemoteStorageHelper.AskToEnable(delegate
						{
							SubEditorScreen.MainSub.Info.SaveToRemoteStorage = true;
						}, null);
						return false;
					}
					return SubEditorScreen.MainSub.Info.SaveToRemoteStorage = tickBox.Selected;
				};
			}
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals25 = CS$<>8__locals1;
			Vector2 relativeSize100 = new ValueTuple<float, float>(1f, 0.075f);
			RectTransform rectTransform111 = rightColumn.RectTransform;
			Anchor anchor110 = Anchor.TopLeft;
			Pivot? pivot109 = null;
			point = null;
			Point? minSize110 = point;
			point = null;
			CS$<>8__locals25.contentPackageTabber = new GUILayoutGroup(new RectTransform(relativeSize100, rectTransform111, anchor110, pivot109, minSize110, point, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIButton saveToPackageTabBtn = CS$<>8__locals1.<CreateSaveScreen>g__createTabberBtn|22("SaveToLocalPackage");
			saveToPackageTabBtn.Selected = true;
			GUIButton reqPackagesTabBtn = CS$<>8__locals1.<CreateSaveScreen>g__createTabberBtn|22("RequiredContentPackages");
			reqPackagesTabBtn.Selected = false;
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals26 = CS$<>8__locals1;
			Vector2 relativeSize101 = new Vector2(1f, 0.45f);
			RectTransform rectTransform112 = rightColumn.RectTransform;
			Anchor anchor111 = Anchor.TopLeft;
			Pivot? pivot110 = null;
			point = null;
			Point? minSize111 = point;
			point = null;
			CS$<>8__locals26.horizontalArea = new GUIFrame(new RectTransform(relativeSize101, rectTransform112, anchor111, pivot110, minSize111, point, ScaleBasis.Normal), null, null);
			Vector2 one11 = Vector2.One;
			RectTransform rectTransform113 = CS$<>8__locals1.horizontalArea.RectTransform;
			Anchor anchor112 = Anchor.BottomRight;
			Pivot? pivot111 = null;
			point = null;
			Point? minSize112 = point;
			point = null;
			GUILayoutGroup saveInPackageLayout = new GUILayoutGroup(new RectTransform(one11, rectTransform113, anchor112, pivot111, minSize112, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals27 = CS$<>8__locals1;
			Vector2 relativeSize102 = new Vector2(1f, 1f);
			RectTransform rectTransform114 = saveInPackageLayout.RectTransform;
			Anchor anchor113 = Anchor.TopLeft;
			Pivot? pivot112 = null;
			point = null;
			Point? minSize113 = point;
			point = null;
			CS$<>8__locals27.packageToSaveInList = new GUIListBox(new RectTransform(relativeSize102, rectTransform114, anchor113, pivot112, minSize113, point, ScaleBasis.Normal), false, null, "", true, false);
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals28 = CS$<>8__locals1;
			Vector2 relativeSize103 = new ValueTuple<float, float>(1f, 0.15f);
			RectTransform rectTransform115 = saveInPackageLayout.RectTransform;
			Anchor anchor114 = Anchor.TopLeft;
			Pivot? pivot113 = null;
			point = null;
			Point? minSize114 = point;
			point = null;
			CS$<>8__locals28.packToSaveInFilter = new GUITextBox(new RectTransform(relativeSize103, rectTransform115, anchor114, pivot113, minSize114, point, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, true, true);
			CS$<>8__locals1.packToSaveInFilter.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				foreach (GUIComponent child2 in CS$<>8__locals1.packageToSaveInList.Content.Children)
				{
					GUIComponent guicomponent = child2;
					GUILayoutGroup child3 = child2.GetChild<GUILayoutGroup>();
					GUITextBlock textBlock = (child3 != null) ? child3.GetChild<GUITextBlock>() : null;
					guicomponent.Visible = (textBlock == null || textBlock.Text.Contains(CS$<>8__locals1.packToSaveInFilter.Text, StringComparison.OrdinalIgnoreCase));
				}
				return true;
			};
			ContentPackage ownerPkg = null;
			GUILayoutGroup newPackageListItem = CS$<>8__locals1.<CreateSaveScreen>g__addItemToPackageToSaveList|24(TextManager.Get("CreateNewLocalPackage"), null);
			GUIFrame newPackageListIcon = newPackageListItem.GetChild<GUIFrame>();
			GUITextBlock newPackageListText = newPackageListItem.GetChild<GUITextBlock>();
			GUIStyle.Apply(newPackageListIcon, "NewContentPackageIcon", null);
			if (ownerPkg == null)
			{
				Submarine mainSub24 = SubEditorScreen.MainSub;
				if (((mainSub24 != null) ? mainSub24.Info : null) != null)
				{
					ownerPkg = SubEditorScreen.GetLocalPackageThatOwnsSub(SubEditorScreen.MainSub.Info);
				}
			}
			foreach (ContentPackage p2 in ContentPackageManager.LocalPackages)
			{
				GUILayoutGroup packageListItem = CS$<>8__locals1.<CreateSaveScreen>g__addItemToPackageToSaveList|24(p2.Name, p2);
				if (p2 == ownerPkg)
				{
					GUIFrame packageListIcon = packageListItem.GetChild<GUIFrame>();
					GUITextBlock packageListText = packageListItem.GetChild<GUITextBlock>();
					GUIStyle.Apply(packageListIcon, "WorkshopMenu.EditButton", null);
					packageListText.Text = TextManager.GetWithVariable("UpdateExistingLocalPackage", "[mod]", p2.Name, FormatCapitals.No);
				}
			}
			if (ownerPkg != null)
			{
				GUIComponent element = CS$<>8__locals1.packageToSaveInList.Content.FindChild(ownerPkg, false);
				if (element != null)
				{
					element.RectTransform.SetAsFirstChild();
				}
			}
			CS$<>8__locals1.packageToSaveInList.Select(0, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
			Vector2 one12 = Vector2.One;
			RectTransform rectTransform116 = CS$<>8__locals1.horizontalArea.RectTransform;
			Anchor anchor115 = Anchor.BottomRight;
			Pivot? pivot114 = null;
			point = null;
			Point? minSize115 = point;
			point = null;
			GUILayoutGroup requiredContentPackagesLayout = new GUILayoutGroup(new RectTransform(one12, rectTransform116, anchor115, pivot114, minSize115, point, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				Visible = false
			};
			SubEditorScreen.<>c__DisplayClass141_0 CS$<>8__locals29 = CS$<>8__locals1;
			Vector2 relativeSize104 = new Vector2(1f, 1f);
			RectTransform rectTransform117 = requiredContentPackagesLayout.RectTransform;
			Anchor anchor116 = Anchor.TopLeft;
			Pivot? pivot115 = null;
			point = null;
			Point? minSize116 = point;
			point = null;
			CS$<>8__locals29.requiredContentPackList = new GUIListBox(new RectTransform(relativeSize104, rectTransform117, anchor116, pivot115, minSize116, point, ScaleBasis.Normal), false, null, "", true, false);
			Vector2 relativeSize105 = new ValueTuple<float, float>(1f, 0.15f);
			RectTransform rectTransform118 = requiredContentPackagesLayout.RectTransform;
			Anchor anchor117 = Anchor.TopLeft;
			Pivot? pivot116 = null;
			point = null;
			Point? minSize117 = point;
			point = null;
			GUILayoutGroup filterLayout = new GUILayoutGroup(new RectTransform(relativeSize105, rectTransform118, anchor117, pivot116, minSize117, point, ScaleBasis.Normal), true, Anchor.CenterLeft);
			Vector2 relativeSize106 = new ValueTuple<float, float>(0.6f, 1f);
			RectTransform rectTransform119 = filterLayout.RectTransform;
			Anchor anchor118 = Anchor.TopLeft;
			Pivot? pivot117 = null;
			point = null;
			Point? minSize118 = point;
			point = null;
			GUITextBox contentPackFilter = new GUITextBox(new RectTransform(relativeSize106, rectTransform119, anchor118, pivot117, minSize118, point, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, true, true);
			contentPackFilter.OnTextChanged += delegate(GUITextBox box, string text)
			{
				CS$<>8__locals1.requiredContentPackList.Content.Children.ForEach(delegate(GUIComponent c)
				{
					GUITickBox tb = c as GUITickBox;
					c.Visible = (tb == null || tb.Text.Contains(text, StringComparison.OrdinalIgnoreCase));
				});
				return true;
			};
			Vector2 relativeSize107 = new ValueTuple<float, float>(0.4f, 1f);
			RectTransform rectTransform120 = filterLayout.RectTransform;
			Anchor anchor119 = Anchor.TopLeft;
			Pivot? pivot118 = null;
			point = null;
			Point? minSize119 = point;
			point = null;
			new GUIButton(new RectTransform(relativeSize107, rectTransform120, anchor119, pivot118, minSize119, point, ScaleBasis.Normal), TextManager.Get("AutoDetectRequiredPackages"), Alignment.Center, "GUIButtonSmall", null).OnClicked = delegate(GUIButton button, object o)
			{
				HashSet<string> requiredPackages = (from p in (from cp in MapEntity.MapEntityList.Select(delegate(MapEntity e)
				{
					if (e == null)
					{
						return null;
					}
					MapEntityPrefab prefab = e.Prefab;
					if (prefab == null)
					{
						return null;
					}
					return prefab.ContentPackage;
				})
				where cp != null
				select cp).Distinct<ContentPackage>().OfType<ContentPackage>()
				select p.Name).ToHashSet<string>();
				GUITickBox[] tickboxes = CS$<>8__locals1.requiredContentPackList.Content.Children.OfType<GUITickBox>().ToArray<GUITickBox>();
				tickboxes.ForEach(delegate(GUITickBox tb)
				{
					tb.Selected = requiredPackages.Contains((tb.UserData as string) ?? "");
				});
				return false;
			};
			if (SubEditorScreen.MainSub != null)
			{
				List<string> allContentPacks = SubEditorScreen.MainSub.Info.RequiredContentPackages.ToList<string>();
				Func<string, bool> <>9__85;
				foreach (ContentPackage contentPack in ContentPackageManager.AllPackages)
				{
					if (!contentPack.Files.All((ContentFile f) => f is SubmarineFile || f is ItemAssemblyFile) && !allContentPacks.Contains(contentPack.Name))
					{
						ImmutableArray<string> altNames = contentPack.AltNames;
						Func<string, bool> predicate;
						if ((predicate = <>9__85) == null)
						{
							predicate = (<>9__85 = ((string n) => allContentPacks.Contains(n)));
						}
						string altName = altNames.FirstOrDefault(predicate);
						if (!string.IsNullOrEmpty(altName))
						{
							if (SubEditorScreen.MainSub.Info.RequiredContentPackages.Contains(altName))
							{
								SubEditorScreen.MainSub.Info.RequiredContentPackages.Remove(altName);
								SubEditorScreen.MainSub.Info.RequiredContentPackages.Add(contentPack.Name);
							}
							allContentPacks.Remove(altName);
						}
						allContentPacks.Add(contentPack.Name);
					}
				}
				foreach (string contentPackageName in allContentPacks)
				{
					Vector2 relativeSize108 = new Vector2(1f, 0.2f);
					RectTransform rectTransform121 = CS$<>8__locals1.requiredContentPackList.Content.RectTransform;
					Anchor anchor120 = Anchor.TopLeft;
					Pivot? pivot119 = null;
					point = null;
					Point? minSize120 = point;
					point = null;
					GUITickBox cpTickBox = new GUITickBox(new RectTransform(relativeSize108, rectTransform121, anchor120, pivot119, minSize120, point, ScaleBasis.Normal), contentPackageName, GUIStyle.SmallFont, "")
					{
						Selected = SubEditorScreen.MainSub.Info.RequiredContentPackages.Contains(contentPackageName),
						UserData = contentPackageName
					};
					GUITickBox guitickBox8 = cpTickBox;
					guitickBox8.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(guitickBox8.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox tickBox)
					{
						if (tickBox.Selected)
						{
							SubEditorScreen.MainSub.Info.RequiredContentPackages.Add((string)tickBox.UserData);
						}
						else
						{
							SubEditorScreen.MainSub.Info.RequiredContentPackages.Remove((string)tickBox.UserData);
						}
						return true;
					}));
				}
			}
			saveToPackageTabBtn.OnClicked = CS$<>8__locals1.<CreateSaveScreen>g__switchToTab|26(saveToPackageTabBtn, saveInPackageLayout);
			reqPackagesTabBtn.OnClicked = CS$<>8__locals1.<CreateSaveScreen>g__switchToTab|26(reqPackagesTabBtn, requiredContentPackagesLayout);
			Vector2 relativeSize109 = new Vector2(1f, 0.05f);
			RectTransform rectTransform122 = paddedSaveFrame.RectTransform;
			Anchor anchor121 = Anchor.BottomCenter;
			point = new Point?(new Point(0, 30));
			GUIFrame buttonArea = new GUIFrame(new RectTransform(relativeSize109, rectTransform122, anchor121, null, point, null, ScaleBasis.Normal), null, null);
			Vector2 relativeSize110 = new Vector2(0.3f, 1f);
			RectTransform rectTransform123 = buttonArea.RectTransform;
			Anchor anchor122 = Anchor.BottomLeft;
			Pivot? pivot120 = null;
			point = null;
			Point? minSize121 = point;
			point = null;
			new GUIButton(new RectTransform(relativeSize110, rectTransform123, anchor122, pivot120, minSize121, point, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				CS$<>8__locals1.<>4__this.saveFrame = null;
				return true;
			};
			Vector2 relativeSize111 = new Vector2(0.3f, 1f);
			RectTransform rectTransform124 = buttonArea.RectTransform;
			Anchor anchor123 = Anchor.BottomRight;
			Pivot? pivot121 = null;
			point = null;
			Point? minSize122 = point;
			point = null;
			new GUIButton(new RectTransform(relativeSize111, rectTransform124, anchor123, pivot121, minSize122, point, ScaleBasis.Normal), TextManager.Get("SaveSubButton").Fallback(TextManager.Get("save"), true), Alignment.Center, "", null).OnClicked = ((GUIButton button, object o) => CS$<>8__locals1.<>4__this.SaveSub(CS$<>8__locals1.packageToSaveInList.SelectedData as ContentPackage));
			paddedSaveFrame.Recalculate();
			leftColumn.Recalculate();
			RectTransform rectTransform125 = CS$<>8__locals1.subSettingsContainer.RectTransform;
			RectTransform rectTransform126 = CS$<>8__locals1.outpostModuleSettingsContainer.RectTransform;
			RectTransform rectTransform127 = CS$<>8__locals1.beaconSettingsContainer.RectTransform;
			Point minSize123 = new Point(0, Math.Max(CS$<>8__locals1.subSettingsContainer.Rect.Height, CS$<>8__locals1.outpostModuleSettingsContainer.Rect.Height));
			rectTransform127.MinSize = minSize123;
			minSize22 = (rectTransform126.MinSize = minSize123);
			rectTransform125.MinSize = minSize22;
			CS$<>8__locals1.subSettingsContainer.Recalculate();
			CS$<>8__locals1.outpostModuleSettingsContainer.Recalculate();
			CS$<>8__locals1.beaconSettingsContainer.Recalculate();
			CS$<>8__locals1.enemySubmarineSettingsContainer.Recalculate();
			this.descriptionBox.Text = ((SubEditorScreen.MainSub == null) ? "" : SubEditorScreen.MainSub.Info.Description.Value);
			this.submarineDescriptionCharacterCount.Text = this.descriptionBox.Text.Length.ToString() + " / " + 500.ToString();
			subTypeDropdown.SelectItem(SubEditorScreen.MainSub.Info.Type);
			if (quickSave)
			{
				this.SaveSub(CS$<>8__locals1.packageToSaveInList.SelectedData as ContentPackage);
			}
		}

		// Token: 0x06002739 RID: 10041 RVA: 0x001ACE28 File Offset: 0x001AB028
		private static ExtraSubmarineInfo GetExtraSubmarineInfo(SubmarineInfo subInfo)
		{
			if (subInfo == null)
			{
				return null;
			}
			ExtraSubmarineInfo result;
			switch (subInfo.Type)
			{
			case SubmarineType.Wreck:
				result = subInfo.WreckInfo;
				break;
			case SubmarineType.BeaconStation:
				result = subInfo.BeaconStationInfo;
				break;
			case SubmarineType.EnemySubmarine:
				result = subInfo.EnemySubmarineInfo;
				break;
			default:
				result = null;
				break;
			}
			return result;
		}

		// Token: 0x0600273A RID: 10042 RVA: 0x001ACE78 File Offset: 0x001AB078
		private void CreateSaveAssemblyScreen()
		{
			this.SetMode(SubEditorScreen.Mode.Default);
			this.saveFrame = new GUIButton(new RectTransform(Vector2.One, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					if (GUI.MouseOn == btn || GUI.MouseOn == btn.TextBlock)
					{
						this.saveFrame = null;
					}
					return true;
				}
			};
			new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, this.saveFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			GUIFrame innerFrame = new GUIFrame(new RectTransform(new Vector2(0.25f, 0.35f), this.saveFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(400, 350)
			}, "", null);
			GUILayoutGroup paddedSaveFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.9f), innerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				AbsoluteSpacing = GUI.IntScale(5f),
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), paddedSaveFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("SaveItemAssemblyDialogHeader");
			GUIFont font = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text3, null, font, Alignment.Left, false, "", null);
			new GUITextBlock(new RectTransform(new Vector2(1f, 0f), paddedSaveFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("SaveItemAssemblyDialogName"), null, null, Alignment.Left, false, "", null);
			this.nameBox = new GUITextBox(new RectTransform(new Vector2(0.6f, 0.1f), paddedSaveFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			GUIListBox descriptionContainer = new GUIListBox(new RectTransform(new Vector2(1f, 0.5f), paddedSaveFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			RectTransform rectT2 = new RectTransform(Vector2.One, descriptionContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text2 = "";
			font = GUIStyle.SmallFont;
			this.descriptionBox = new GUITextBox(rectT2, text2, null, font, Alignment.TopLeft, true, "GUITextBoxNoBorder", null, false, true)
			{
				Padding = new Vector4(10f * GUI.Scale)
			};
			this.descriptionBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				Vector2 textSize = textBox.Font.MeasureString(this.descriptionBox.WrappedText, false);
				textBox.RectTransform.NonScaledSize = new Point(textBox.RectTransform.NonScaledSize.X, Math.Max(descriptionContainer.Content.Rect.Height, (int)textSize.Y + 10));
				descriptionContainer.UpdateScrollBarSize();
				descriptionContainer.BarScroll = 1f;
				return true;
			};
			GUIFrame buttonArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), paddedSaveFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIButton(new RectTransform(new Vector2(0.25f, 1f), buttonArea.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				this.saveFrame = null;
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(0.25f, 1f), buttonArea.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("SaveSubButton"), Alignment.Center, "", null).OnClicked = new GUIButton.OnClickedHandler(this.SaveAssembly);
			buttonArea.RectTransform.MinSize = new Point(0, buttonArea.Children.First<GUIComponent>().RectTransform.MinSize.Y);
		}

		// Token: 0x0600273B RID: 10043 RVA: 0x001AD38C File Offset: 0x001AB58C
		private List<Item> LoadItemAssemblyInventorySafe(ItemAssemblyPrefab assemblyPrefab)
		{
			List<MapEntity> realItems = assemblyPrefab.CreateInstance(Vector2.Zero, SubEditorScreen.MainSub, false);
			List<Item> itemInstance = new List<Item>();
			realItems.ForEach(delegate(MapEntity entity)
			{
				Item it = entity as Item;
				if (it != null && it.ParentInventory == null)
				{
					itemInstance.Add(it);
				}
			});
			return itemInstance;
		}

		// Token: 0x0600273C RID: 10044 RVA: 0x001AD3D4 File Offset: 0x001AB5D4
		private bool SaveAssembly(GUIButton button, object obj)
		{
			SubEditorScreen.<>c__DisplayClass145_0 CS$<>8__locals1 = new SubEditorScreen.<>c__DisplayClass145_0();
			CS$<>8__locals1.<>4__this = this;
			if (string.IsNullOrWhiteSpace(this.nameBox.Text))
			{
				GUI.AddMessage(TextManager.Get("ItemAssemblyNameMissingWarning"), GUIStyle.Red, null, true, null);
				this.nameBox.Flash(null, 1.5f, false, false, null);
				return false;
			}
			foreach (char illegalChar in Path.GetInvalidFileNameCharsCrossPlatform())
			{
				if (this.nameBox.Text.Contains(illegalChar))
				{
					GUI.AddMessage(TextManager.GetWithVariable("ItemAssemblyNameIllegalCharsWarning", "[illegalchar]", illegalChar.ToString(), FormatCapitals.No), GUIStyle.Red, null, true, null);
					this.nameBox.Flash(null, 1.5f, false, false, null);
					return false;
				}
			}
			this.nameBox.Text = this.nameBox.Text.Trim();
			SubEditorScreen.<>c__DisplayClass145_0 CS$<>8__locals2 = CS$<>8__locals1;
			GUITickBox hideInMenusTickBox = this.nameBox.Parent.GetChildByUserData("hideinmenus") as GUITickBox;
			CS$<>8__locals2.hideInMenus = (hideInMenusTickBox != null && hideInMenusTickBox.Selected);
			string saveFolder = Path.Combine(new string[]
			{
				"LocalMods",
				this.nameBox.Text
			});
			CS$<>8__locals1.filePath = Path.Combine(new string[]
			{
				saveFolder,
				this.nameBox.Text + ".xml"
			}).CleanUpPathCrossPlatform(true, "");
			if (File.Exists(CS$<>8__locals1.filePath))
			{
				GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("Warning"), TextManager.Get("ItemAssemblyFileExistsWarning"), new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("No")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				msgBox.Buttons[0].OnClicked = delegate(GUIButton btn, object userdata)
				{
					msgBox.Close();
					CS$<>8__locals1.<SaveAssembly>g__Save|0();
					return true;
				};
				msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
			}
			else
			{
				CS$<>8__locals1.<SaveAssembly>g__Save|0();
			}
			this.saveFrame = null;
			return false;
		}

		// Token: 0x0600273D RID: 10045 RVA: 0x001AD69C File Offset: 0x001AB89C
		private static void SnapToGrid()
		{
			foreach (MapEntity e in MapEntity.SelectedList)
			{
				Vector2 offset = e.Position;
				offset = new Vector2((MathF.Floor(offset.X / Submarine.GridSize.X) + 0.5f) * Submarine.GridSize.X - offset.X, (MathF.Floor(offset.Y / Submarine.GridSize.Y) + 0.5f) * Submarine.GridSize.Y - offset.Y);
				Item item = e as Item;
				if (item != null)
				{
					if (item.GetComponent<Wire>() == null)
					{
						item.Move(offset, true);
						Door component = item.GetComponent<Door>();
						Gap linkedGap = (component != null) ? component.LinkedGap : null;
						if (linkedGap != null)
						{
							linkedGap.Move(item.Position - linkedGap.Position, true);
						}
					}
				}
				else
				{
					Structure structure = e as Structure;
					if (structure != null)
					{
						structure.Move(offset, true);
					}
				}
			}
			foreach (Item item2 in (from entity in MapEntity.SelectedList
			where entity is Item
			select entity).Cast<Item>())
			{
				Wire wire = item2.GetComponent<Wire>();
				if (wire != null)
				{
					for (int i = 0; i < wire.GetNodes().Count; i++)
					{
						Vector2 offset2 = wire.GetNodes()[i] + Submarine.MainSub.HiddenSubPosition;
						offset2 = new Vector2((MathF.Floor(offset2.X / Submarine.GridSize.X) + 0.5f) * Submarine.GridSize.X - offset2.X, (MathF.Floor(offset2.Y / Submarine.GridSize.Y) + 0.5f) * Submarine.GridSize.Y - offset2.Y);
						wire.MoveNode(i, offset2);
					}
				}
			}
		}

		// Token: 0x0600273E RID: 10046 RVA: 0x001AD8EC File Offset: 0x001ABAEC
		private static IEnumerable<SubmarineInfo> GetLoadableSubs()
		{
			string downloadFolder = Path.GetFullPath(SaveUtil.SubmarineDownloadFolder);
			return from s in SubmarineInfo.SavedSubmarines
			where Path.GetDirectoryName(Path.GetFullPath(s.FilePath)) != downloadFolder
			select s;
		}

		// Token: 0x0600273F RID: 10047 RVA: 0x001AD928 File Offset: 0x001ABB28
		private void CreateLoadScreen()
		{
			SubEditorScreen.<>c__DisplayClass148_0 CS$<>8__locals1 = new SubEditorScreen.<>c__DisplayClass148_0();
			CS$<>8__locals1.<>4__this = this;
			this.CloseItem();
			SubmarineInfo.RefreshSavedSubs();
			this.SetMode(SubEditorScreen.Mode.Default);
			this.loadFrame = new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, GUI.Canvas, Anchor.Center, null, null, null, ScaleBasis.Normal), "GUIBackgroundBlocker", null);
			new GUIButton(new RectTransform(Vector2.One, this.loadFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), Alignment.Center, null, null).OnClicked = delegate(GUIButton _, object _)
			{
				CS$<>8__locals1.<>4__this.loadFrame = null;
				return true;
			};
			GUIFrame innerFrame = new GUIFrame(new RectTransform(new Vector2(0.53f, 0.75f), this.loadFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Smallest)
			{
				MinSize = new Point(350, 500)
			}, "", null);
			GUILayoutGroup paddedLoadFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.9f), innerFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			CS$<>8__locals1.deleteButtonHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), paddedLoadFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.1f,
				Stretch = true
			};
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.1f), paddedLoadFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			string text7 = "";
			GUIFont font = GUIStyle.Font;
			GUITextBox searchBox = new GUITextBox(rectT, text7, null, font, Alignment.Left, false, "", null, true, true);
			SubEditorScreen.<>c__DisplayClass148_0 CS$<>8__locals2 = CS$<>8__locals1;
			RectTransform rectT2 = new RectTransform(Vector2.One, searchBox.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("serverlog.filter");
			font = GUIStyle.Font;
			CS$<>8__locals2.searchTitle = new GUITextBlock(rectT2, text2, null, font, Alignment.CenterLeft, false, "", null)
			{
				CanBeFocused = false,
				IgnoreLayoutGroups = true
			};
			CS$<>8__locals1.searchTitle.TextColor *= 0.5f;
			CS$<>8__locals1.subList = new GUIListBox(new RectTransform(new Vector2(1f, 0.7f), paddedLoadFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				ScrollBarVisible = true,
				OnSelected = delegate(GUIComponent selected, object userData)
				{
					GUIButton deleteBtn = CS$<>8__locals1.deleteButtonHolder.FindChild("delete", false) as GUIButton;
					if (deleteBtn != null)
					{
						deleteBtn.ToolTip = string.Empty;
						SubmarineInfo subInfo2 = userData as SubmarineInfo;
						if (subInfo2 == null)
						{
							deleteBtn.Enabled = false;
							return true;
						}
						if (SubEditorScreen.GetLocalPackageThatOwnsSub(subInfo2) != null || subInfo2.IsFromRemoteStorage)
						{
							deleteBtn.Enabled = true;
						}
						else
						{
							deleteBtn.Enabled = false;
							if (SubEditorScreen.IsVanillaSub(subInfo2))
							{
								deleteBtn.ToolTip = TextManager.Get("cantdeletevanillasub");
							}
							else
							{
								ContentPackage subPackage2 = SubEditorScreen.GetPackageThatOwnsSub(subInfo2, ContentPackageManager.AllPackages);
								if (subPackage2 != null)
								{
									deleteBtn.ToolTip = TextManager.GetWithVariable("cantdeletemodsub", "[modname]", subPackage2.Name, FormatCapitals.No);
								}
							}
						}
					}
					return true;
				}
			};
			searchBox.OnSelected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = false;
			};
			searchBox.OnDeselected += delegate(GUITextBox sender, Keys userdata)
			{
				CS$<>8__locals1.searchTitle.Visible = sender.Text.IsNullOrEmpty();
			};
			searchBox.OnTextChanged += delegate(GUITextBox textBox, string text)
			{
				CS$<>8__locals1.<>4__this.FilterSubs(CS$<>8__locals1.subList, text);
				return true;
			};
			List<SubmarineInfo> allSubs = SubmarineInfo.SavedSubmarines.ToList<SubmarineInfo>();
			foreach (SteamRemoteStorage.RemoteFile remoteFile in from file in SteamRemoteStorage.Files
			where file.Filename.EndsWith(".sub")
			select file)
			{
				byte[] bytes;
				if (remoteFile.TryRead(out bytes, true))
				{
					using (MemoryStream stream = new MemoryStream(bytes))
					{
						using (GZipStream zipStream = new GZipStream(stream, CompressionMode.Decompress))
						{
							XDocument doc = XMLExtensions.TryLoadXml(zipStream);
							if (doc == null)
							{
								DebugConsole.ThrowError(RemoteStorageHelper.DebugPrefix + " Failed to load submarine \"" + remoteFile.Filename + "\" from remote storage: file is not a valid XML document.", null, null, false, false);
							}
							else
							{
								SubmarineInfo subInfo = new SubmarineInfo(remoteFile.Filename, "", doc.Root, false, false)
								{
									IsFromRemoteStorage = true
								};
								allSubs.Add(subInfo);
							}
						}
					}
				}
			}
			IOrderedEnumerable<SubmarineInfo> sortedSubs = from kvp in allSubs
			orderby kvp.Type, kvp.Name, kvp.IsFromRemoteStorage
			select kvp;
			SubmarineInfo prevSub = null;
			using (IEnumerator<SubmarineInfo> enumerator2 = sortedSubs.GetEnumerator())
			{
				while (enumerator2.MoveNext())
				{
					SubmarineInfo sub = enumerator2.Current;
					if (prevSub == null || prevSub.Type != sub.Type)
					{
						string textTag = "SubmarineType." + sub.Type.ToString();
						if (sub.Type == SubmarineType.EnemySubmarine && !TextManager.ContainsTag(textTag))
						{
							textTag = "MissionType.Pirate";
						}
						RectTransform rectTransform = new RectTransform(new Vector2(1f, 0f), CS$<>8__locals1.subList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
						rectTransform.MinSize = new Point(0, 35);
						RichString text3 = TextManager.Get(textTag);
						font = GUIStyle.LargeFont;
						new GUITextBlock(rectTransform, text3, null, font, Alignment.Center, false, "ListBoxElement", null).CanBeFocused = false;
						prevSub = sub;
					}
					string displayPath = sub.FilePath;
					if (sub.IsFromRemoteStorage)
					{
						string str = displayPath;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
						defaultInterpolatedStringHandler.AppendLiteral(" ");
						defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("RemoteStorage"));
						displayPath = str + defaultInterpolatedStringHandler.ToStringAndClear();
					}
					else
					{
						string saveFolder = Path.GetFullPath(SaveUtil.DefaultSaveFolder);
						string fullPath = Path.GetFullPath(displayPath);
						if (fullPath.StartsWith(saveFolder))
						{
							displayPath = "..." + fullPath.Substring(saveFolder.Length);
						}
					}
					LocalizedString limitedName = ToolBox.LimitString(sub.Name, GUIStyle.Font, CS$<>8__locals1.subList.Rect.Width - 80);
					GUITextBlock textBlock = new GUITextBlock(new RectTransform(Vector2.UnitX, CS$<>8__locals1.subList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(0, 30)
					}, limitedName, null, null, Alignment.Left, false, "", null)
					{
						UserData = sub,
						ToolTip = displayPath
					};
					if (sub.IsFromRemoteStorage)
					{
						textBlock.OverrideTextColor(RemoteStorageHelper.SteamColor);
					}
					else if (ContentPackageManager.VanillaCorePackage == null || ContentPackageManager.VanillaCorePackage.Files.None((ContentFile f) => f.Path == sub.FilePath))
					{
						if (SubEditorScreen.GetLocalPackageThatOwnsSub(sub) == null)
						{
							Func<ContentFile, bool> <>9__12;
							ContentPackage subPackage = ContentPackageManager.AllPackages.FirstOrDefault(delegate(ContentPackage p)
							{
								ImmutableArray<ContentFile> files = p.Files;
								Func<ContentFile, bool> predicate;
								if ((predicate = <>9__12) == null)
								{
									predicate = (<>9__12 = ((ContentFile f) => f.Path == sub.FilePath));
								}
								return files.Any(predicate);
							});
							if (subPackage != null)
							{
								textBlock.OverrideTextColor(Color.MediumPurple);
								goto IL_7EE;
							}
						}
						textBlock.OverrideTextColor(GUIStyle.TextColorBright);
					}
					IL_7EE:
					if (sub.HasTag(SubmarineTag.Shuttle))
					{
						RectTransform rectT3 = new RectTransform(new Vector2(0.2f, 1f), textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
						RichString text4 = TextManager.Get(new string[]
						{
							"Shuttle",
							"RespawnShuttle"
						});
						font = GUIStyle.SmallFont;
						GUITextBlock guitextBlock = new GUITextBlock(rectT3, text4, null, font, Alignment.CenterRight, false, "", null);
						guitextBlock.TextColor = textBlock.TextColor * 0.8f;
						guitextBlock.ToolTip = textBlock.ToolTip.SanitizedString;
					}
					else if (sub.IsPlayer)
					{
						RectTransform rectT4 = new RectTransform(new Vector2(0.2f, 1f), textBlock.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal);
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("submarineclass.");
						defaultInterpolatedStringHandler2.AppendFormatted<SubmarineClass>(sub.SubmarineClass);
						RichString text5 = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
						font = GUIStyle.SmallFont;
						GUITextBlock guitextBlock2 = new GUITextBlock(rectT4, text5, null, font, Alignment.CenterRight, false, "", null);
						guitextBlock2.TextColor = textBlock.TextColor * 0.8f;
						guitextBlock2.ToolTip = textBlock.ToolTip.SanitizedString;
					}
				}
			}
			CS$<>8__locals1.deleteButton = new GUIButton(new RectTransform(Vector2.One, CS$<>8__locals1.deleteButtonHolder.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), TextManager.Get("Delete"), Alignment.Center, "", null)
			{
				Enabled = false,
				UserData = "delete"
			};
			CS$<>8__locals1.deleteButton.OnClicked = delegate(GUIButton btn, object userdata)
			{
				if (CS$<>8__locals1.subList.SelectedComponent != null)
				{
					CS$<>8__locals1.<>4__this.TryDeleteSub(CS$<>8__locals1.subList.SelectedComponent.UserData as SubmarineInfo);
				}
				CS$<>8__locals1.deleteButton.Enabled = false;
				return true;
			};
			RectTransform rectT5 = new RectTransform(new Vector2(1f, 0.1f), CS$<>8__locals1.deleteButtonHolder.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text6 = TextManager.Get("DragAndDropSubmarineTip").Fallback(LocalizedString.EmptyString, true);
			font = GUIStyle.Font;
			new GUITextBlock(rectT5, text6, null, font, Alignment.Center, false, "", null).Wrap = true;
			XDocument autoSaveInfo = SubEditorScreen.AutoSaveInfo;
			if (((autoSaveInfo != null) ? autoSaveInfo.Root : null) != null)
			{
				int min = Math.Min(6, SubEditorScreen.AutoSaveInfo.Root.Elements().Count<XElement>());
				GUIDropDown loadAutoSave = new GUIDropDown(new RectTransform(Vector2.One, CS$<>8__locals1.deleteButtonHolder.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), TextManager.Get("LoadAutoSave"), min, "", false, false, Alignment.CenterLeft, 1f)
				{
					ToolTip = TextManager.Get("LoadAutoSaveTooltip"),
					UserData = "loadautosave",
					OnSelected = delegate(GUIComponent button, object o)
					{
						CS$<>8__locals1.<>4__this.LoadAutoSave(o);
						return true;
					}
				};
				foreach (XElement saveElement in SubEditorScreen.AutoSaveInfo.Root.Elements().Reverse<XElement>())
				{
					DateTime time = DateTime.MinValue.AddSeconds(saveElement.GetAttributeUInt64("time", 0UL));
					TimeSpan difference = DateTime.UtcNow - time;
					LocalizedString tooltip = TextManager.GetWithVariables("subeditor.autosaveage", new ValueTuple<string, string>[]
					{
						new ValueTuple<string, string>("[hours]", ((int)Math.Floor(difference.TotalHours)).ToString()),
						new ValueTuple<string, string>("[minutes]", difference.Minutes.ToString()),
						new ValueTuple<string, string>("[seconds]", difference.Seconds.ToString())
					});
					string submarineName = saveElement.GetAttributeString("name", TextManager.Get("UnspecifiedSubFileName").Value);
					double totalMinutes = difference.TotalMinutes;
					LocalizedString timeFormat;
					if (totalMinutes < 1.0)
					{
						timeFormat = TextManager.Get("subeditor.savedjustnow");
					}
					else if (totalMinutes > 60.0)
					{
						timeFormat = TextManager.Get("subeditor.savedmorethanhour");
					}
					else
					{
						timeFormat = TextManager.GetWithVariable("subeditor.saveageminutes", "[minutes]", difference.Minutes.ToString(), FormatCapitals.No);
					}
					LocalizedString entryName = TextManager.GetWithVariables("subeditor.autosaveentry", new ValueTuple<string, LocalizedString>[]
					{
						new ValueTuple<string, LocalizedString>("[submarine]", submarineName),
						new ValueTuple<string, LocalizedString>("[saveage]", timeFormat)
					});
					loadAutoSave.AddItem(entryName, saveElement, tooltip, null, null);
				}
			}
			GUILayoutGroup controlBtnHolder = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), paddedLoadFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				RelativeSpacing = 0.2f,
				Stretch = true
			};
			new GUIButton(new RectTransform(new Vector2(0.5f, 1f), controlBtnHolder.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				CS$<>8__locals1.<>4__this.loadFrame = null;
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(0.5f, 1f), controlBtnHolder.RectTransform, Anchor.BottomRight, null, null, null, ScaleBasis.Normal), TextManager.Get("Load"), Alignment.Center, "", null).OnClicked = new GUIButton.OnClickedHandler(this.HitLoadSubButton);
			controlBtnHolder.RectTransform.MaxSize = new Point(int.MaxValue, controlBtnHolder.Children.First<GUIComponent>().Rect.Height);
		}

		// Token: 0x06002740 RID: 10048 RVA: 0x001AE850 File Offset: 0x001ACA50
		private void FilterSubs(GUIListBox subList, string filter)
		{
			foreach (GUIComponent child in subList.Content.Children)
			{
				SubmarineInfo sub = child.UserData as SubmarineInfo;
				if (sub != null)
				{
					child.Visible = (string.IsNullOrEmpty(filter) || sub.Name.ToLower().Contains(filter.ToLower()));
				}
			}
			bool subVisibleInCategory = false;
			foreach (GUIComponent child2 in subList.Content.Children.Reverse<GUIComponent>())
			{
				if (!(child2.UserData is SubmarineInfo))
				{
					if (child2.Enabled)
					{
						child2.Visible = subVisibleInCategory;
					}
					subVisibleInCategory = false;
				}
				else
				{
					subVisibleInCategory |= child2.Visible;
				}
			}
		}

		// Token: 0x06002741 RID: 10049 RVA: 0x001AE94C File Offset: 0x001ACB4C
		private void LoadAutoSave(object userData)
		{
			XElement element = userData as XElement;
			if (element == null)
			{
				return;
			}
			string filePath = element.GetAttributeStringUnrestricted("file", "");
			if (string.IsNullOrWhiteSpace(filePath))
			{
				return;
			}
			Submarine loadedSub = Submarine.Load(new SubmarineInfo(filePath, "", null, true, false), true, null);
			try
			{
				loadedSub.Info.Name = loadedSub.Info.SubmarineElement.GetAttributeString("name", loadedSub.Info.Name);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to find a name for the submarine.", e, null, false, false);
				LocalizedString unspecifiedFileName = TextManager.Get("UnspecifiedSubFileName");
				loadedSub.Info.Name = unspecifiedFileName.Value;
			}
			SubEditorScreen.MainSub = loadedSub;
			SubEditorScreen.MainSub.SetPrevTransform(SubEditorScreen.MainSub.Position);
			SubEditorScreen.MainSub.UpdateTransform(true);
			SubEditorScreen.MainSub.Info.Name = loadedSub.Info.Name;
			this.subNameLabel.Text = ToolBox.LimitString(loadedSub.Info.Name, this.subNameLabel.Font, this.subNameLabel.Rect.Width);
			this.ReconstructLayers();
			this.CreateDummyCharacter();
			this.cam.Position = SubEditorScreen.MainSub.Position + SubEditorScreen.MainSub.HiddenSubPosition;
			this.loadFrame = null;
		}

		// Token: 0x06002742 RID: 10050 RVA: 0x001AEAB4 File Offset: 0x001ACCB4
		private bool HitLoadSubButton(GUIButton button, object obj)
		{
			if (this.loadFrame == null)
			{
				DebugConsole.NewMessage("load frame null", new Color?(Color.Red), false);
				return false;
			}
			GUIListBox subList = this.loadFrame.GetAnyChild<GUIListBox>();
			if (subList == null)
			{
				DebugConsole.NewMessage("Sublist null", new Color?(Color.Red), false);
				return false;
			}
			GUIComponent selectedComponent = subList.SelectedComponent;
			SubmarineInfo selectedSubInfo = ((selectedComponent != null) ? selectedComponent.UserData : null) as SubmarineInfo;
			if (selectedSubInfo == null)
			{
				return false;
			}
			if (!selectedSubInfo.IsFromRemoteStorage && SubEditorScreen.GetLocalPackageThatOwnsSub(selectedSubInfo) == null)
			{
				if (SubEditorScreen.IsVanillaSub(selectedSubInfo))
				{
					this.AskLoadVanillaSub(selectedSubInfo);
				}
				else
				{
					ContentPackage workshopPackage = SubEditorScreen.GetWorkshopPackageThatOwnsSub(selectedSubInfo);
					if (workshopPackage != null)
					{
						SteamWorkshopId workshopId;
						if (workshopPackage.TryExtractSteamWorkshopId(out workshopId) && this.publishedWorkshopItemIds.Contains(workshopId.Value))
						{
							this.AskLoadPublishedSub(selectedSubInfo, workshopPackage);
						}
						else
						{
							this.AskLoadSubscribedSub(selectedSubInfo);
						}
					}
				}
			}
			else
			{
				this.LoadSub(selectedSubInfo, true);
			}
			return false;
		}

		// Token: 0x06002743 RID: 10051 RVA: 0x001AEB88 File Offset: 0x001ACD88
		private void AskLoadSub(SubmarineInfo info, LocalizedString header, LocalizedString desc)
		{
			GUIMessageBox msgBox = new GUIMessageBox(header, desc, new LocalizedString[]
			{
				TextManager.Get("LoadAnyway"),
				TextManager.Get("Cancel")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Buttons[0].OnClicked = delegate(GUIButton button, object o)
			{
				this.LoadSub(info, true);
				msgBox.Close();
				return false;
			};
			msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(msgBox.Close);
		}

		// Token: 0x06002744 RID: 10052 RVA: 0x001AEC4C File Offset: 0x001ACE4C
		private void AskLoadPublishedSub(SubmarineInfo info, ContentPackage pkg)
		{
			this.AskLoadSub(info, TextManager.Get("LoadingPublishedSubmarineHeader"), TextManager.GetWithVariable("LoadingPublishedSubmarineDesc", "[modname]", pkg.Name, FormatCapitals.No));
		}

		// Token: 0x06002745 RID: 10053 RVA: 0x001AEC7A File Offset: 0x001ACE7A
		private void AskLoadSubscribedSub(SubmarineInfo info)
		{
			this.AskLoadSub(info, TextManager.Get("LoadingSubscribedSubmarineHeader"), TextManager.Get("LoadingSubscribedSubmarineDesc"));
		}

		// Token: 0x06002746 RID: 10054 RVA: 0x001AEC97 File Offset: 0x001ACE97
		private void AskLoadVanillaSub(SubmarineInfo info)
		{
			this.AskLoadSub(info, TextManager.Get("LoadingVanillaSubmarineHeader"), TextManager.Get("LoadingVanillaSubmarineDesc"));
		}

		// Token: 0x06002747 RID: 10055 RVA: 0x001AECB4 File Offset: 0x001ACEB4
		public void LoadSub(SubmarineInfo info, bool checkIdConflicts = true)
		{
			SubEditorScreen.<>c__DisplayClass156_0 CS$<>8__locals1 = new SubEditorScreen.<>c__DisplayClass156_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.info = info;
			Submarine.Unload();
			Submarine selectedSub = null;
			if (checkIdConflicts)
			{
				Dictionary<int, Identifier> entities = new Dictionary<int, Identifier>();
				using (IEnumerator<XElement> enumerator = CS$<>8__locals1.info.SubmarineElement.Elements().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						SubEditorScreen.<>c__DisplayClass156_1 CS$<>8__locals2 = new SubEditorScreen.<>c__DisplayClass156_1();
						CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
						CS$<>8__locals2.subElement = enumerator.Current;
						int id = CS$<>8__locals2.subElement.GetAttributeInt("ID", -1);
						if (id != -1)
						{
							Identifier identifier = CS$<>8__locals2.subElement.GetAttributeIdentifier("identifier", string.Empty);
							Identifier duplicateEntity;
							if (entities.TryGetValue(id, out duplicateEntity))
							{
								SubEditorScreen.<>c__DisplayClass156_1 CS$<>8__locals3 = CS$<>8__locals2;
								RichString headerText = TextManager.Get("error");
								string tag = "subeditor.duplicateiderror";
								ValueTuple<string, string>[] array = new ValueTuple<string, string>[2];
								int num = 0;
								string item = "[entity1]";
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(duplicateEntity);
								defaultInterpolatedStringHandler.AppendLiteral(" (");
								defaultInterpolatedStringHandler.AppendFormatted<int>(id);
								defaultInterpolatedStringHandler.AppendLiteral(")");
								array[num] = new ValueTuple<string, string>(item, defaultInterpolatedStringHandler.ToStringAndClear());
								int num2 = 1;
								string item2 = "[entity2]";
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(3, 2);
								defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(identifier);
								defaultInterpolatedStringHandler2.AppendLiteral(" (");
								defaultInterpolatedStringHandler2.AppendFormatted<int>(id);
								defaultInterpolatedStringHandler2.AppendLiteral(")");
								array[num2] = new ValueTuple<string, string>(item2, defaultInterpolatedStringHandler2.ToStringAndClear());
								CS$<>8__locals3.errorMsg = new GUIMessageBox(headerText, TextManager.GetWithVariables(tag, array), new LocalizedString[]
								{
									TextManager.Get("Yes"),
									TextManager.Get("No")
								}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
								CS$<>8__locals2.errorMsg.Buttons[0].OnClicked = delegate(GUIButton bnt, object userdata)
								{
									CS$<>8__locals2.subElement.Remove();
									CS$<>8__locals2.CS$<>8__locals1.<>4__this.LoadSub(CS$<>8__locals2.CS$<>8__locals1.info, false);
									CS$<>8__locals2.errorMsg.Close();
									return true;
								};
								CS$<>8__locals2.errorMsg.Buttons[1].OnClicked = delegate(GUIButton bnt, object userdata)
								{
									CS$<>8__locals2.CS$<>8__locals1.<>4__this.LoadSub(CS$<>8__locals2.CS$<>8__locals1.info, false);
									CS$<>8__locals2.errorMsg.Close();
									return true;
								};
								return;
							}
							entities.Add(id, identifier);
						}
					}
				}
			}
			try
			{
				selectedSub = new Submarine(CS$<>8__locals1.info, true, null, null);
				SubEditorScreen.MainSub = selectedSub;
				SubEditorScreen.MainSub.UpdateTransform(false);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to load the submarine. The submarine file might be corrupted.", e, null, false, false);
				return;
			}
			SubEditorScreen.ClearUndoBuffer();
			this.CreateDummyCharacter();
			string name = SubEditorScreen.MainSub.Info.Name;
			this.subNameLabel.Text = ToolBox.LimitString(name, this.subNameLabel.Font, this.subNameLabel.Rect.Width);
			this.cam.Position = SubEditorScreen.MainSub.Position + SubEditorScreen.MainSub.HiddenSubPosition;
			this.loadFrame = null;
			if (selectedSub.Info.GameVersion < new Version("0.8.9.0"))
			{
				GUIMessageBox adjustLightsPrompt = new GUIMessageBox(TextManager.Get("Warning"), TextManager.Get("AdjustLightsPrompt"), new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("No")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				GUIButton guibutton = adjustLightsPrompt.Buttons[0];
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(adjustLightsPrompt.Close));
				GUIButton guibutton2 = adjustLightsPrompt.Buttons[0];
				guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userdata)
				{
					foreach (Item item3 in Item.ItemList)
					{
						if (item3.ParentInventory == null && item3.body == null)
						{
							LightComponent lightComponent = item3.GetComponent<LightComponent>();
							foreach (LightComponent light in item3.GetComponents<LightComponent>())
							{
								light.LightColor = new Color(light.LightColor, (float)light.LightColor.A / 255f * 0.5f);
							}
						}
					}
					new GUIMessageBox("", TextManager.Get("AdjustedLightsNotification"), null, null, GUIMessageBox.Type.Default);
					return true;
				}));
				GUIButton guibutton3 = adjustLightsPrompt.Buttons[1];
				guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(adjustLightsPrompt.Close));
			}
			this.ReconstructLayers();
		}

		// Token: 0x06002748 RID: 10056 RVA: 0x001AF0F4 File Offset: 0x001AD2F4
		private static ContentPackage GetPackageThatOwnsSub(SubmarineInfo sub, IEnumerable<ContentPackage> packages)
		{
			Func<ContentFile, bool> <>9__1;
			return packages.FirstOrDefault(delegate(ContentPackage package)
			{
				ImmutableArray<ContentFile> files = package.Files;
				Func<ContentFile, bool> predicate;
				if ((predicate = <>9__1) == null)
				{
					predicate = (<>9__1 = ((ContentFile f) => f.Path == sub.FilePath));
				}
				return files.Any(predicate);
			});
		}

		// Token: 0x06002749 RID: 10057 RVA: 0x001AF120 File Offset: 0x001AD320
		private static ContentPackage GetLocalPackageThatOwnsSub(SubmarineInfo sub)
		{
			return SubEditorScreen.GetPackageThatOwnsSub(sub, ContentPackageManager.LocalPackages);
		}

		// Token: 0x0600274A RID: 10058 RVA: 0x001AF12D File Offset: 0x001AD32D
		private static ContentPackage GetWorkshopPackageThatOwnsSub(SubmarineInfo sub)
		{
			return SubEditorScreen.GetPackageThatOwnsSub(sub, ContentPackageManager.WorkshopPackages);
		}

		// Token: 0x0600274B RID: 10059 RVA: 0x001AF13A File Offset: 0x001AD33A
		private static bool IsVanillaSub(SubmarineInfo sub)
		{
			return SubEditorScreen.GetPackageThatOwnsSub(sub, ContentPackageManager.VanillaCorePackage.ToEnumerable<CorePackage>()) != null;
		}

		// Token: 0x0600274C RID: 10060 RVA: 0x001AF150 File Offset: 0x001AD350
		private void TryDeleteSub(SubmarineInfo sub)
		{
			if (sub == null)
			{
				return;
			}
			ContentPackage subPackage = SubEditorScreen.GetLocalPackageThatOwnsSub(sub);
			if (!ContentPackageManager.LocalPackages.Regular.Contains(subPackage))
			{
				subPackage = null;
			}
			if (!sub.IsFromRemoteStorage && subPackage == null)
			{
				return;
			}
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("DeleteDialogLabel"), TextManager.GetWithVariable("DeleteDialogQuestion", "[file]", sub.Name, FormatCapitals.No), new LocalizedString[]
			{
				TextManager.Get("Yes"),
				TextManager.Get("Cancel")
			}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUIButton guibutton = msgBox.Buttons[0];
			Func<ModProject.File, bool> <>9__1;
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton btn, object userData)
			{
				if (sub.IsFromRemoteStorage)
				{
					RemoteStorageHelper.TryDelete(sub.FilePath, true);
				}
				else if (subPackage != null)
				{
					try
					{
						File.Delete(sub.FilePath, false);
						ModProject modProject = new ModProject(subPackage);
						ModProject modProject2 = modProject;
						IEnumerable<ModProject.File> files = modProject.Files;
						Func<ModProject.File, bool> predicate;
						if ((predicate = <>9__1) == null)
						{
							predicate = (<>9__1 = ((ModProject.File f) => ContentPath.FromRaw(subPackage, f.Path) == sub.FilePath));
						}
						modProject2.RemoveFile(files.First(predicate));
						modProject.Save(subPackage.Path, true);
						this.ReloadModifiedPackage(subPackage);
						Submarine mainSub = SubEditorScreen.MainSub;
						if (((mainSub != null) ? mainSub.Info : null) != null && SubEditorScreen.MainSub.Info.FilePath == sub.FilePath)
						{
							SubEditorScreen.MainSub.Info.FilePath = null;
						}
					}
					catch (Exception e)
					{
						DebugConsole.ThrowErrorLocalized(TextManager.GetWithVariable("DeleteFileError", "[file]", sub.FilePath, FormatCapitals.No), e, null, false, false);
					}
				}
				sub.Dispose();
				this.CreateLoadScreen();
				return msgBox.Close(btn, userData);
			}));
			GUIButton guibutton2 = msgBox.Buttons[1];
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(msgBox.Close));
		}

		// Token: 0x0600274D RID: 10061 RVA: 0x001AF2AC File Offset: 0x001AD4AC
		private void OpenEntityMenu(MapEntityCategory? entityCategory)
		{
			this.UpdateEntityList();
			foreach (GUIButton categoryButton in this.entityCategoryButtons)
			{
				GUIComponent guicomponent = categoryButton;
				bool selected;
				if (entityCategory == null)
				{
					selected = (categoryButton.UserData == null);
				}
				else
				{
					object userData = categoryButton.UserData;
					if (userData is MapEntityCategory)
					{
						MapEntityCategory category = (MapEntityCategory)userData;
						selected = (entityCategory.Value == category);
					}
					else
					{
						selected = false;
					}
				}
				guicomponent.Selected = selected;
				string categoryName = (entityCategory != null) ? entityCategory.Value.ToString() : "All";
				this.selectedCategoryText.Text = TextManager.Get("MapEntityCategory." + categoryName);
				this.selectedCategoryButton.ApplyStyle(GUIStyle.GetComponentStyle("CategoryButton." + categoryName));
			}
			this.selectedCategory = entityCategory;
			this.SetMode(SubEditorScreen.Mode.Default);
			this.saveFrame = null;
			this.loadFrame = null;
			foreach (GUIComponent child in this.toggleEntityMenuButton.Children)
			{
				child.SpriteEffects = (this.entityMenuOpen ? SpriteEffects.None : SpriteEffects.FlipVertically);
			}
			foreach (GUIComponent child2 in this.categorizedEntityList.Content.Children)
			{
				GUIComponent guicomponent2 = child2;
				bool visible;
				if (entityCategory != null)
				{
					MapEntityCategory mapEntityCategory = (MapEntityCategory)child2.UserData;
					MapEntityCategory? mapEntityCategory2 = entityCategory;
					visible = (mapEntityCategory == mapEntityCategory2.GetValueOrDefault() & mapEntityCategory2 != null);
				}
				else
				{
					visible = true;
				}
				guicomponent2.Visible = visible;
				GUIListBox innerList = child2.GetChild<GUIListBox>();
				foreach (GUIComponent grandChild in innerList.Content.Children)
				{
					grandChild.Visible = true;
				}
			}
			if (!string.IsNullOrEmpty(this.entityFilterBox.Text))
			{
				this.FilterEntities(this.entityFilterBox.Text);
			}
			this.categorizedEntityList.UpdateScrollBarSize();
			this.categorizedEntityList.BarScroll = 0f;
		}

		// Token: 0x0600274E RID: 10062 RVA: 0x001AF524 File Offset: 0x001AD724
		private void FilterEntities(string filter)
		{
			if (string.IsNullOrWhiteSpace(filter))
			{
				this.allEntityList.Visible = false;
				this.categorizedEntityList.Visible = true;
				foreach (GUIComponent child in this.categorizedEntityList.Content.Children)
				{
					GUIComponent guicomponent = child;
					bool visible;
					if (this.selectedCategory != null)
					{
						MapEntityCategory? mapEntityCategory = this.selectedCategory;
						MapEntityCategory mapEntityCategory2 = (MapEntityCategory)child.UserData;
						visible = (mapEntityCategory.GetValueOrDefault() == mapEntityCategory2 & mapEntityCategory != null);
					}
					else
					{
						visible = true;
					}
					guicomponent.Visible = visible;
					if (!child.Visible)
					{
						return;
					}
					GUIListBox innerList = child.GetChild<GUIListBox>();
					foreach (GUIComponent grandChild in innerList.Content.Children)
					{
						grandChild.Visible = ((MapEntityPrefab)grandChild.UserData).Name.Value.Contains(filter, StringComparison.OrdinalIgnoreCase);
					}
				}
				this.categorizedEntityList.UpdateScrollBarSize();
				this.categorizedEntityList.BarScroll = 0f;
				return;
			}
			this.allEntityList.Visible = true;
			this.categorizedEntityList.Visible = false;
			filter = filter.ToLower();
			foreach (GUIComponent child2 in this.allEntityList.Content.Children)
			{
				child2.Visible = ((this.selectedCategory == null || ((MapEntityPrefab)child2.UserData).Category.HasFlag(this.selectedCategory)) && ((MapEntityPrefab)child2.UserData).Name.Value.Contains(filter, StringComparison.OrdinalIgnoreCase));
			}
			this.allEntityList.UpdateScrollBarSize();
			this.allEntityList.BarScroll = 0f;
		}

		// Token: 0x0600274F RID: 10063 RVA: 0x001AF74C File Offset: 0x001AD94C
		private void ClearFilter()
		{
			this.FilterEntities("");
			this.categorizedEntityList.UpdateScrollBarSize();
			this.categorizedEntityList.BarScroll = 0f;
			this.entityFilterBox.Text = "";
		}

		// Token: 0x06002750 RID: 10064 RVA: 0x001AF784 File Offset: 0x001AD984
		public void SetMode(SubEditorScreen.Mode newMode)
		{
			if (newMode == this.mode)
			{
				return;
			}
			this.mode = newMode;
			this.lockMode = true;
			this.defaultModeTickBox.Selected = (newMode == SubEditorScreen.Mode.Default);
			this.wiringModeTickBox.Selected = (newMode == SubEditorScreen.Mode.Wiring);
			this.lockMode = false;
			MapEntity.ClearHighlightedEntities();
			MapEntity.DeselectAll();
			MapEntity.FilteredSelectedList.Clear();
			SubEditorScreen.ClearUndoBuffer();
			this.CreateDummyCharacter();
			if (newMode == SubEditorScreen.Mode.Wiring)
			{
				Item item = new Item(MapEntityPrefab.Find(null, "screwdriver", true) as ItemPrefab, Vector2.Zero, null, 0, true);
				this.dummyCharacter.Inventory.TryPutItem(item, null, new List<InvSlotType>
				{
					InvSlotType.RightHand
				}, true, false, true);
				Point wirePos = new Point((int)(10f * GUI.Scale), this.TopPanel.Rect.Height + this.entityCountPanel.Rect.Height + (int)(10f * GUI.Scale));
				this.wiringToolPanel = SubEditorScreen.CreateWiringPanel(wirePos, new GUIListBox.OnSelectedHandler(this.SelectWire));
			}
		}

		// Token: 0x06002751 RID: 10065 RVA: 0x001AF890 File Offset: 0x001ADA90
		private void RemoveDummyCharacter()
		{
			if (this.dummyCharacter == null || this.dummyCharacter.Removed)
			{
				return;
			}
			this.dummyCharacter.Inventory.AllItems.ForEachMod(delegate(Item it)
			{
				it.Remove();
			});
			this.dummyCharacter.Remove();
			this.dummyCharacter = null;
		}

		// Token: 0x06002752 RID: 10066 RVA: 0x001AF8FC File Offset: 0x001ADAFC
		private void CreateContextMenu()
		{
			if (GUIContextMenu.CurrentContextMenu != null)
			{
				return;
			}
			List<MapEntity> targets = MapEntity.HighlightedEntities.Any((MapEntity me) => !MapEntity.SelectedList.Contains(me)) ? MapEntity.HighlightedEntities.ToList<MapEntity>() : new List<MapEntity>(MapEntity.SelectedList);
			Item targetItem = ((targets.Count == 1) ? targets.Single<MapEntity>() : null) as Item;
			bool flag;
			if (targetItem != null)
			{
				flag = targetItem.Components.Any(delegate(ItemComponent ic)
				{
					if (!(ic is ConnectionPanel) && !(ic is Repairable))
					{
						ItemContainer itemContainer = ic as ItemContainer;
						if (itemContainer == null || itemContainer.DrawInventory)
						{
							return ic.GuiFrame != null;
						}
					}
					return false;
				});
			}
			else
			{
				flag = false;
			}
			bool allowOpening = flag;
			bool hasTargets = targets.Count > 0;
			if (PlayerInput.IsShiftDown())
			{
				ContextMenuOption[] array = new ContextMenuOption[7];
				array[0] = new ContextMenuOption("SubEditor.EditBackgroundColor", true, new Action(base.CreateBackgroundColorPicker));
				array[1] = new ContextMenuOption("SubEditor.ToggleTransparency", true, delegate()
				{
					SubEditorScreen.TransparentWiringMode = !SubEditorScreen.TransparentWiringMode;
				});
				array[2] = new ContextMenuOption("SubEditor.ToggleGrid", true, delegate()
				{
					SubEditorScreen.ShouldDrawGrid = !SubEditorScreen.ShouldDrawGrid;
				});
				array[3] = new ContextMenuOption("SubEditor.PasteAssembly", true, delegate()
				{
					this.PasteAssembly(null, null);
				});
				Func<MapEntity, bool> <>9__8;
				array[4] = new ContextMenuOption("Editor.SelectSame", hasTargets, delegate()
				{
					bool doorGapSelected = targets.Any(delegate(MapEntity t)
					{
						Gap gap2 = t as Gap;
						return gap2 != null && gap2.ConnectedDoor != null;
					});
					IEnumerable<MapEntity> mapEntityList = MapEntity.MapEntityList;
					Func<MapEntity, bool> predicate;
					if ((predicate = <>9__8) == null)
					{
						predicate = (<>9__8 = ((MapEntity e) => e.Prefab != null && targets.Any(delegate(MapEntity t)
						{
							MapEntityPrefab prefab = t.Prefab;
							Identifier? identifier;
							Identifier? identifier2;
							if (prefab == null)
							{
								identifier = null;
								identifier2 = identifier;
							}
							else
							{
								identifier2 = new Identifier?(prefab.Identifier);
							}
							identifier = identifier2;
							Identifier? identifier3 = new Identifier?(e.Prefab.Identifier);
							return identifier == identifier3;
						}) && !MapEntity.SelectedList.Contains(e)));
					}
					foreach (MapEntity match in mapEntityList.Where(predicate))
					{
						if (!MapEntity.SelectedList.Contains(match))
						{
							Gap gap = match as Gap;
							if (gap != null)
							{
								if (gap.ConnectedDoor == null == doorGapSelected)
								{
									continue;
								}
							}
							else
							{
								Item item = match as Item;
								if (item != null)
								{
									Door door = item.GetComponent<Door>();
									if (((door != null) ? door.LinkedGap : null) != null && !MapEntity.SelectedList.Contains(door.LinkedGap))
									{
										MapEntity.SelectedList.Add(door.LinkedGap);
									}
								}
							}
							MapEntity.SelectedList.Add(match);
						}
					}
				});
				array[5] = new ContextMenuOption("SubEditor.AddImage", true, new Action(SubEditorScreen.ImageManager.CreateImageWizard));
				array[6] = new ContextMenuOption("SubEditor.ToggleImageEditing", true, delegate()
				{
					SubEditorScreen.ImageManager.EditorMode = !SubEditorScreen.ImageManager.EditorMode;
					if (!SubEditorScreen.ImageManager.EditorMode)
					{
						GameSettings.SaveCurrentConfig();
					}
				});
				GUIContextMenu.CreateContextMenu(array);
				return;
			}
			List<ContextMenuOption> availableLayers = new List<ContextMenuOption>
			{
				new ContextMenuOption("editor.layer.nolayer", true, delegate()
				{
					this.MoveToLayer(null, targets);
				})
			};
			availableLayers.AddRange(from layer in SubEditorScreen.Layers
			select new ContextMenuOption(layer.Key, true, delegate()
			{
				this.MoveToLayer(layer.Key, targets);
			}));
			Func<MapEntity, bool> <>9__25;
			List<ContextMenuOption> availableLayerOptions = new List<ContextMenuOption>
			{
				new ContextMenuOption("editor.layer.movetolayer", hasTargets, availableLayers.ToArray()),
				new ContextMenuOption("editor.layer.createlayer", hasTargets, delegate()
				{
					this.CreateNewLayer(null, targets);
				}),
				new ContextMenuOption("editor.layer.selectall", hasTargets, delegate()
				{
					IEnumerable<MapEntity> mapEntityList = MapEntity.MapEntityList;
					Func<MapEntity, bool> predicate;
					if ((predicate = <>9__25) == null)
					{
						predicate = (<>9__25 = ((MapEntity e) => targets.Any((MapEntity t) => !string.IsNullOrWhiteSpace(t.Layer) && t.Layer == e.Layer && !MapEntity.SelectedList.Contains(e))));
					}
					foreach (MapEntity match in mapEntityList.Where(predicate))
					{
						if (!MapEntity.SelectedList.Contains(match))
						{
							MapEntity.SelectedList.Add(match);
						}
					}
				})
			};
			availableLayerOptions.AddRange(from layer in SubEditorScreen.Layers
			select new ContextMenuOption(layer.Key, true, delegate()
			{
				this.MoveToLayer(layer.Key, targets);
			}));
			ContextMenuOption[] array2 = new ContextMenuOption[10];
			array2[0] = new ContextMenuOption("label.openlabel", allowOpening, delegate()
			{
				this.OpenItem(targetItem);
			});
			array2[1] = new ContextMenuOption("editor.cut", hasTargets, delegate()
			{
				MapEntity.Cut(targets);
			});
			array2[2] = new ContextMenuOption("editor.copytoclipboard", hasTargets, delegate()
			{
				MapEntity.Copy(targets);
			});
			array2[3] = new ContextMenuOption("editor.paste", MapEntity.CopiedList.Any<MapEntity>(), delegate()
			{
				MapEntity.Paste(this.cam.ScreenToWorld(PlayerInput.MousePosition));
			});
			array2[4] = new ContextMenuOption("delete", hasTargets, delegate()
			{
				SubEditorScreen.StoreCommand(new AddOrDeleteCommand(targets, true, true));
				foreach (MapEntity me in targets)
				{
					if (!me.Removed)
					{
						me.Remove();
					}
				}
			});
			array2[5] = new ContextMenuOption(string.Empty, false, delegate()
			{
			});
			int num = 6;
			string label = "editor.layer.movetoactivelayer";
			GUIListBox guilistBox = this.layerList;
			array2[num] = new ContextMenuOption(label, !(((guilistBox != null) ? guilistBox.SelectedData : null) as string).IsNullOrEmpty(), delegate()
			{
				this.MoveToLayer(this.layerList.SelectedData as string, targets);
			});
			array2[7] = new ContextMenuOption("editor.layer.removefromlayer", targets.Any((MapEntity t) => t.Layer != string.Empty), delegate()
			{
				targets.ForEach(delegate(MapEntity t)
				{
					t.Layer = string.Empty;
				});
			});
			array2[8] = new ContextMenuOption("editor.layeroptions", hasTargets, availableLayerOptions.ToArray());
			array2[9] = new ContextMenuOption(TextManager.GetWithVariable("editortip.shiftforextraoptions", "[button]", PlayerInput.SecondaryMouseLabel, FormatCapitals.No) + '\n' + TextManager.Get("editortip.altforruler"), false, null);
			GUIContextMenu.CreateContextMenu(array2);
		}

		// Token: 0x06002753 RID: 10067 RVA: 0x001AFD6C File Offset: 0x001ADF6C
		private void MoveToLayer(string layer, List<MapEntity> content)
		{
			if (layer == null)
			{
				layer = string.Empty;
			}
			foreach (MapEntity entity in content)
			{
				if (MapEntity.SelectedList.Contains(entity))
				{
					MapEntity.ResetEditingHUD();
				}
				entity.Layer = layer;
			}
		}

		// Token: 0x06002754 RID: 10068 RVA: 0x001AFDD8 File Offset: 0x001ADFD8
		private void CreateNewLayer(string name, List<MapEntity> content)
		{
			if (string.IsNullOrWhiteSpace(name))
			{
				name = TextManager.Get("editor.layer.newlayer").Value;
			}
			string incrementedName = name;
			int i = 1;
			while (SubEditorScreen.Layers.ContainsKey(incrementedName))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendFormatted(name);
				defaultInterpolatedStringHandler.AppendLiteral(" (");
				defaultInterpolatedStringHandler.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				incrementedName = defaultInterpolatedStringHandler.ToStringAndClear();
				i++;
			}
			name = incrementedName;
			if (content != null)
			{
				this.MoveToLayer(name, content);
			}
			SubEditorScreen.Layers.Add(name, default(SubEditorScreen.LayerData));
			this.UpdateLayerPanel();
		}

		// Token: 0x06002755 RID: 10069 RVA: 0x001AFE78 File Offset: 0x001AE078
		private void RenameLayer(string original, string newName)
		{
			SubEditorScreen.LayerData originalData;
			SubEditorScreen.Layers.Remove(original, out originalData);
			IEnumerable<MapEntity> mapEntityList = MapEntity.MapEntityList;
			Func<MapEntity, bool> <>9__0;
			Func<MapEntity, bool> predicate;
			if ((predicate = <>9__0) == null)
			{
				predicate = (<>9__0 = ((MapEntity entity) => entity.Layer == original));
			}
			foreach (MapEntity entity2 in mapEntityList.Where(predicate))
			{
				entity2.Layer = (newName ?? string.Empty);
			}
			if (!string.IsNullOrWhiteSpace(newName))
			{
				SubEditorScreen.Layers.TryAdd(newName, originalData);
			}
			this.UpdateLayerPanel();
		}

		// Token: 0x06002756 RID: 10070 RVA: 0x001AFF30 File Offset: 0x001AE130
		public void ReconstructLayers()
		{
			Dictionary<string, SubEditorScreen.LayerData> previousLayers = SubEditorScreen.Layers.ToDictionary<string, SubEditorScreen.LayerData>();
			this.ClearLayers();
			foreach (MapEntity entity in MapEntity.MapEntityList)
			{
				if (!string.IsNullOrWhiteSpace(entity.Layer))
				{
					SubEditorScreen.Layers.TryAdd(entity.Layer, new SubEditorScreen.LayerData(!entity.IsLayerHidden, false));
				}
			}
			foreach (KeyValuePair<string, SubEditorScreen.LayerData> keyValuePair in previousLayers)
			{
				string text;
				SubEditorScreen.LayerData layerData;
				keyValuePair.Deconstruct(out text, out layerData);
				string layerName = text;
				SubEditorScreen.LayerData data = layerData;
				if (SubEditorScreen.Layers.ContainsKey(layerName))
				{
					SubEditorScreen.Layers[layerName] = data;
				}
			}
			this.UpdateLayerPanel();
		}

		// Token: 0x06002757 RID: 10071 RVA: 0x001B0028 File Offset: 0x001AE228
		private void ClearLayers()
		{
			SubEditorScreen.Layers.Clear();
			this.UpdateLayerPanel();
		}

		// Token: 0x06002758 RID: 10072 RVA: 0x001B003C File Offset: 0x001AE23C
		private static void SetLayerVisibility(string layerName, bool isVisible)
		{
			SubEditorScreen.LayerData layerData;
			if (SubEditorScreen.Layers.Remove(layerName, out layerData))
			{
				Dictionary<string, SubEditorScreen.LayerData> layers = SubEditorScreen.Layers;
				SubEditorScreen.LayerData value = layerData;
				value.IsVisible = isVisible;
				layers.Add(layerName, value);
				return;
			}
			SubEditorScreen.Layers.Add(layerName, new SubEditorScreen.LayerData(isVisible, false));
		}

		// Token: 0x06002759 RID: 10073 RVA: 0x001B0084 File Offset: 0x001AE284
		private void PasteAssembly(string text = null, Vector2? pos = null)
		{
			Vector2 value = pos.GetValueOrDefault();
			if (pos == null)
			{
				value = this.cam.ScreenToWorld(PlayerInput.MousePosition);
				pos = new Vector2?(value);
			}
			if (text == null)
			{
				text = Clipboard.GetText();
			}
			if (string.IsNullOrWhiteSpace(text))
			{
				DebugConsole.ThrowError("Unable to paste assembly: Clipboard content is empty.", null, null, false, false);
				return;
			}
			XElement element = null;
			try
			{
				element = XDocument.Parse(text).Root;
			}
			catch (Exception)
			{
			}
			if (element == null)
			{
				DebugConsole.ThrowError("Unable to paste assembly: Clipboard content is not valid XML.", null, null, false, false);
				return;
			}
			Submarine sub = SubEditorScreen.MainSub;
			List<MapEntity> entities;
			try
			{
				entities = ItemAssemblyPrefab.PasteEntities(pos.Value, sub, element, null, true);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Unable to paste assembly: Failed to load items.", e, null, false, false);
				return;
			}
			if (!entities.Any<MapEntity>())
			{
				return;
			}
			SubEditorScreen.StoreCommand(new AddOrDeleteCommand(entities, false, false));
		}

		// Token: 0x0600275A RID: 10074 RVA: 0x001B0160 File Offset: 0x001AE360
		public static GUIMessageBox CreatePropertyColorPicker(Color originalColor, SerializableProperty property, ISerializableEntity entity)
		{
			List<ValueTuple<ISerializableEntity, Color, SerializableProperty>> entities = new List<ValueTuple<ISerializableEntity, Color, SerializableProperty>>
			{
				new ValueTuple<ISerializableEntity, Color, SerializableProperty>(entity, originalColor, property)
			};
			IEnumerable<MapEntity> selectedList = MapEntity.SelectedList;
			Func<MapEntity, bool> <>9__14;
			Func<MapEntity, bool> predicate;
			if ((predicate = <>9__14) == null)
			{
				predicate = (<>9__14 = ((MapEntity selectedEntity) => selectedEntity is ISerializableEntity && entity != selectedEntity));
			}
			foreach (ISerializableEntity selectedEntity2 in selectedList.Where(predicate).Cast<ISerializableEntity>())
			{
				ISerializableEntity entity2 = entity;
				if (entity2 is ItemComponent)
				{
					Item item = selectedEntity2 as Item;
					if (item != null)
					{
						using (List<ItemComponent>.Enumerator enumerator2 = item.Components.GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								ItemComponent component2 = enumerator2.Current;
								if (component2.GetType() == entity.GetType() && component2 != entity)
								{
									entities.Add(new ValueTuple<ISerializableEntity, Color, SerializableProperty>(component2, (Color)property.GetValue(component2), property));
								}
							}
							continue;
						}
					}
				}
				if (selectedEntity2.GetType() == entity.GetType())
				{
					entities.Add(new ValueTuple<ISerializableEntity, Color, SerializableProperty>(selectedEntity2, (Color)property.GetValue(selectedEntity2), property));
				}
				else if (selectedEntity2 != null)
				{
					Dictionary<Identifier, SerializableProperty> props = selectedEntity2.SerializableProperties;
					SerializableProperty foundProp;
					if (props != null && props.TryGetValue(property.Name.ToIdentifier(), out foundProp))
					{
						entities.Add(new ValueTuple<ISerializableEntity, Color, SerializableProperty>(selectedEntity2, (Color)foundProp.GetValue(selectedEntity2), foundProp));
					}
				}
			}
			bool setValues = true;
			object sliderMutex = new object();
			object sliderTextMutex = new object();
			object pickerMutex = new object();
			object hexMutex = new object();
			Vector2 relativeSize = new Vector2(0.4f * GUI.AspectRatioAdjustment, 0.3f);
			GUIMessageBox msgBox = new GUIMessageBox(string.Empty, string.Empty, Array.Empty<LocalizedString>(), new Vector2?(relativeSize), null, Alignment.TopLeft, GUIMessageBox.Type.Vote, "", null, "", null, null, false)
			{
				UserData = "colorpicker",
				Draggable = true
			};
			GUILayoutGroup contentLayout = new GUILayoutGroup(new RectTransform(Vector2.One, msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.1f), contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text5 = property.Name;
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text5, null, subHeadingFont, Alignment.TopCenter, false, "", null).AutoScaleVertical = true;
			GUILayoutGroup colorLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.7f), contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUILayoutGroup buttonLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.2f), contentLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft)
			{
				RelativeSpacing = 0.1f,
				Stretch = true
			};
			GUIButton closeButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("OK"), Alignment.Center, "", null);
			GUIButton cancelButton = new GUIButton(new RectTransform(new Vector2(0.5f, 1f), buttonLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Cancel"), Alignment.Center, "", null);
			contentLayout.Recalculate();
			colorLayout.Recalculate();
			GUIColorPicker colorPicker = new GUIColorPicker(new RectTransform(new Point(colorLayout.Rect.Height), colorLayout.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null);
			float num;
			float num2;
			float num3;
			ToolBox.RGBToHSV(originalColor).Deconstruct(out num, out num2, out num3);
			float h = num;
			float s = num2;
			float v = num3;
			colorPicker.SelectedHue = (float.IsNaN(h) ? 0f : h);
			colorPicker.SelectedSaturation = s;
			colorPicker.SelectedValue = v;
			colorLayout.Recalculate();
			GUILayoutGroup sliderLayout = new GUILayoutGroup(new RectTransform(new Vector2(1f - colorPicker.RectTransform.RelativeSize.X, 1f), colorLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopRight);
			float currentHue = colorPicker.SelectedHue / 360f;
			GUILayoutGroup hueSliderLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.25f), sliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			RectTransform rectT2 = new RectTransform(new Vector2(0.1f, 0.2f), hueSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = "H:";
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock = new GUITextBlock(rectT2, text2, null, subHeadingFont, Alignment.Left, false, "", null);
			guitextBlock.Padding = Vector4.Zero;
			guitextBlock.ToolTip = "Hue";
			GUIScrollBar hueScrollBar = new GUIScrollBar(new RectTransform(new Vector2(0.7f, 1f), hueSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.05f, null, "GUISlider", null)
			{
				BarScroll = currentHue
			};
			GUINumberInput hueTextBox = new GUINumberInput(new RectTransform(new Vector2(0.2f, 1f), hueSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(GUI.IntScale(100f), 0)
			}, NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				FloatValue = currentHue,
				MaxValueFloat = new float?(1f),
				MinValueFloat = new float?(0f),
				DecimalsToDisplay = 2
			};
			GUILayoutGroup satSliderLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.2f), sliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			RectTransform rectT3 = new RectTransform(new Vector2(0.1f, 0.2f), satSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = "S:";
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock2 = new GUITextBlock(rectT3, text3, null, subHeadingFont, Alignment.Left, false, "", null);
			guitextBlock2.Padding = Vector4.Zero;
			guitextBlock2.ToolTip = "Saturation";
			GUIScrollBar satScrollBar = new GUIScrollBar(new RectTransform(new Vector2(0.7f, 1f), satSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.05f, null, "GUISlider", null)
			{
				BarScroll = colorPicker.SelectedSaturation
			};
			GUINumberInput satTextBox = new GUINumberInput(new RectTransform(new Vector2(0.2f, 1f), satSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(GUI.IntScale(100f), 0)
			}, NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				FloatValue = colorPicker.SelectedSaturation,
				MaxValueFloat = new float?(1f),
				MinValueFloat = new float?(0f),
				DecimalsToDisplay = 2
			};
			GUILayoutGroup valueSliderLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.2f), sliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			RectTransform rectT4 = new RectTransform(new Vector2(0.1f, 0.2f), valueSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text4 = "V:";
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock guitextBlock3 = new GUITextBlock(rectT4, text4, null, subHeadingFont, Alignment.Left, false, "", null);
			guitextBlock3.Padding = Vector4.Zero;
			guitextBlock3.ToolTip = "Value";
			GUIScrollBar valueScrollBar = new GUIScrollBar(new RectTransform(new Vector2(0.7f, 1f), valueSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), 0.05f, null, "GUISlider", null)
			{
				BarScroll = colorPicker.SelectedValue
			};
			GUINumberInput valueTextBox = new GUINumberInput(new RectTransform(new Vector2(0.2f, 1f), valueSliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(GUI.IntScale(100f), 0)
			}, NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
			{
				FloatValue = colorPicker.SelectedValue,
				MaxValueFloat = new float?(1f),
				MinValueFloat = new float?(0f),
				DecimalsToDisplay = 2
			};
			GUILayoutGroup colorInfoLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.3f), sliderLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.1f
			};
			new GUICustomComponent(new RectTransform(Vector2.One, colorInfoLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), delegate(SpriteBatch batch, GUICustomComponent component)
			{
				Rectangle rect = component.Rect;
				Point areaSize = new Point(rect.Width, rect.Height / 2);
				Rectangle newColorRect = new Rectangle(rect.Location, areaSize);
				Rectangle oldColorRect = new Rectangle(new Point(newColorRect.Left, newColorRect.Bottom), areaSize);
				GUI.DrawRectangle(batch, newColorRect, ToolBoxCore.HSVToRGB(colorPicker.SelectedHue, colorPicker.SelectedSaturation, colorPicker.SelectedValue), true, 0f, 1f);
				GUI.DrawRectangle(batch, oldColorRect, originalColor, true, 0f, 1f);
				GUI.DrawRectangle(batch, rect, Color.Black, false, 0f, 1f);
			}, null);
			GUITextBox hexValueBox = new GUITextBox(new RectTransform(new Vector2(0.3f, 1f), colorInfoLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), SubEditorScreen.<CreatePropertyColorPicker>g__ColorToHex|175_13(originalColor), null, null, Alignment.Left, false, "", null, false, false)
			{
				OverflowClip = true
			};
			hueScrollBar.OnMoved = delegate(GUIScrollBar bar, float scroll)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(sliderMutex);
				return true;
			};
			hueTextBox.OnValueChanged = delegate(GUINumberInput input)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(sliderTextMutex);
			};
			satScrollBar.OnMoved = delegate(GUIScrollBar bar, float scroll)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(sliderMutex);
				return true;
			};
			satTextBox.OnValueChanged = delegate(GUINumberInput input)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(sliderTextMutex);
			};
			valueScrollBar.OnMoved = delegate(GUIScrollBar bar, float scroll)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(sliderMutex);
				return true;
			};
			valueTextBox.OnValueChanged = delegate(GUINumberInput input)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(sliderTextMutex);
			};
			colorPicker.OnColorSelected = delegate(GUIColorPicker component, Color color)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(pickerMutex);
				return true;
			};
			hexValueBox.OnEnterPressed = delegate(GUITextBox box, string text)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(hexMutex);
				return true;
			};
			hexValueBox.OnDeselected += delegate(GUITextBox sender, Keys key)
			{
				base.<CreatePropertyColorPicker>g__SetColor|12(hexMutex);
			};
			closeButton.OnClicked = delegate(GUIButton button, object o)
			{
				colorPicker.Dispose();
				msgBox.Close();
				Color newColor = base.<CreatePropertyColorPicker>g__SetColor|12(null);
				if (!SubEditorScreen.IsSubEditor())
				{
					return true;
				}
				Dictionary<object, List<ISerializableEntity>> oldProperties = new Dictionary<object, List<ISerializableEntity>>();
				foreach (ValueTuple<ISerializableEntity, Color, SerializableProperty> valueTuple in entities)
				{
					ISerializableEntity sEntity = valueTuple.Item1;
					Color color = valueTuple.Item2;
					MapEntity mapEntity = sEntity as MapEntity;
					if (mapEntity == null || !mapEntity.Removed)
					{
						if (!oldProperties.ContainsKey(color))
						{
							oldProperties.Add(color, new List<ISerializableEntity>());
						}
						oldProperties[color].Add(sEntity);
					}
				}
				List<ISerializableEntity> affected = (from t in entities
				select t.Item1).Where(delegate(ISerializableEntity se)
				{
					MapEntity mapEntity2 = se as MapEntity;
					return (mapEntity2 != null && !mapEntity2.Removed) || se is ItemComponent;
				}).ToList<ISerializableEntity>();
				SubEditorScreen.StoreCommand(new PropertyCommand(affected, property.Name.ToIdentifier(), newColor, oldProperties));
				if (MapEntity.EditingHUD != null)
				{
					if (MapEntity.EditingHUD.UserData != entity)
					{
						ItemComponent ic = entity as ItemComponent;
						if (ic != null && MapEntity.EditingHUD.UserData != ic.Item)
						{
							return true;
						}
					}
					GUIListBox list = MapEntity.EditingHUD.GetChild<GUIListBox>();
					if (list != null)
					{
						IEnumerable<SerializableEntityEditor> editors = list.Content.FindChildren((GUIComponent comp) => comp is SerializableEntityEditor).Cast<SerializableEntityEditor>();
						SerializableEntityEditor.LockEditing = true;
						foreach (SerializableEntityEditor editor in editors)
						{
							GUIComponent[] array;
							if (editor.UserData == entity && editor.Fields.TryGetValue(property.Name.ToIdentifier(), out array))
							{
								editor.UpdateValue(property, newColor, false);
							}
						}
						SerializableEntityEditor.LockEditing = false;
					}
				}
				return true;
			};
			cancelButton.OnClicked = delegate(GUIButton button, object o)
			{
				colorPicker.Dispose();
				msgBox.Close();
				foreach (ValueTuple<ISerializableEntity, Color, SerializableProperty> valueTuple in entities)
				{
					ISerializableEntity e = valueTuple.Item1;
					Color color = valueTuple.Item2;
					SerializableProperty prop = valueTuple.Item3;
					MapEntity mapEntity = e as MapEntity;
					if (mapEntity == null || !mapEntity.Removed)
					{
						prop.TrySetValue(e, color);
					}
				}
				return true;
			};
			return msgBox;
		}

		// Token: 0x0600275B RID: 10075 RVA: 0x001B0F28 File Offset: 0x001AF128
		public static GUIFrame CreateWiringPanel(Point offset, GUIListBox.OnSelectedHandler onWireSelected)
		{
			GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(0.03f, 0.35f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(120, 300),
				AbsoluteOffset = offset
			}, "", null);
			GUIListBox listBox = new GUIListBox(new RectTransform(new Vector2(0.9f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				OnSelected = onWireSelected,
				CanTakeKeyBoardFocus = false
			};
			List<ItemPrefab> wirePrefabs = new List<ItemPrefab>();
			foreach (ItemPrefab itemPrefab in ItemPrefab.Prefabs)
			{
				if (!itemPrefab.Name.IsNullOrEmpty() && !itemPrefab.HideInMenus && !itemPrefab.HideInEditors && itemPrefab.Tags.Contains(Tags.WireItem) && (!CircuitBox.IsInGame() || (!itemPrefab.Tags.Contains(Tags.Thalamus) && !itemPrefab.Tags.Contains("alien"))))
				{
					wirePrefabs.Add(itemPrefab);
				}
			}
			foreach (ItemPrefab itemPrefab2 in from w in wirePrefabs
			orderby !w.CanBeBought, w.UintIdentifier
			select w)
			{
				GUIFrame imgFrame = new GUIFrame(new RectTransform(new Point(listBox.Content.Rect.Width, listBox.Rect.Width / 2), listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "ListBoxElement", null)
				{
					UserData = itemPrefab2
				};
				GUIImage guiimage = new GUIImage(new RectTransform(new Vector2(0.9f), imgFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), itemPrefab2.Sprite, true, null);
				guiimage.UserData = itemPrefab2;
				guiimage.Color = itemPrefab2.SpriteColor;
				guiimage.HoverColor = Color.Lerp(itemPrefab2.SpriteColor, Color.White, 0.3f);
				guiimage.SelectedColor = Color.Lerp(itemPrefab2.SpriteColor, Color.White, 0.6f);
			}
			return frame;
		}

		// Token: 0x0600275C RID: 10076 RVA: 0x001B1248 File Offset: 0x001AF448
		private bool SelectLinkedSub(GUIComponent selected, object userData)
		{
			SubmarineInfo submarine = userData as SubmarineInfo;
			if (submarine == null)
			{
				return false;
			}
			LinkedSubmarinePrefab prefab = new LinkedSubmarinePrefab(submarine);
			MapEntityPrefab.SelectPrefab(prefab);
			return true;
		}

		// Token: 0x0600275D RID: 10077 RVA: 0x001B1270 File Offset: 0x001AF470
		private bool SelectWire(GUIComponent component, object userData)
		{
			if (this.dummyCharacter == null)
			{
				return false;
			}
			Item existingWire = this.dummyCharacter.HeldItems.FirstOrDefault((Item i) => i.Prefab == userData as ItemPrefab);
			if (existingWire != null)
			{
				existingWire.Drop(null, true, true);
				existingWire.Remove();
				return false;
			}
			Item wire = new Item(userData as ItemPrefab, Vector2.Zero, null, 0, true);
			int slotIndex = this.dummyCharacter.Inventory.FindLimbSlot(InvSlotType.LeftHand);
			existingWire = this.dummyCharacter.Inventory.GetItemAt(slotIndex);
			if (existingWire != null && existingWire.Prefab != userData as ItemPrefab)
			{
				existingWire.Drop(null, true, true);
				existingWire.Remove();
			}
			this.dummyCharacter.Inventory.TryPutItem(wire, slotIndex, false, false, this.dummyCharacter, true, false, true);
			return true;
		}

		// Token: 0x0600275E RID: 10078 RVA: 0x001B1348 File Offset: 0x001AF548
		private void OpenItem(Item item)
		{
			if (this.dummyCharacter == null || item == null)
			{
				return;
			}
			Holdable component = item.GetComponent<Holdable>();
			if (((component != null && !component.Attached) || item.GetComponent<Wearable>() != null) && item.GetComponent<ItemContainer>() != null)
			{
				this.oldItemPosition = item.SimPosition;
				this.TeleportDummyCharacter(this.oldItemPosition);
				ItemContainer container = item.GetComponent<ItemContainer>();
				if (container != null)
				{
					container.KeepOpenWhenEquipped = true;
				}
				List<InvSlotType> allowedSlots = new List<InvSlotType>();
				item.AllowedSlots.ForEach(delegate(InvSlotType type)
				{
					if (type != InvSlotType.Any)
					{
						allowedSlots.Add(type);
					}
				});
				bool success = this.dummyCharacter.Inventory.TryPutItem(item, this.dummyCharacter, allowedSlots, true, false, true);
				if (!success)
				{
					return;
				}
				this.OpenedItem = item;
			}
			MapEntity.SelectedList.Clear();
			MapEntity.FilteredSelectedList.Clear();
			MapEntity.SelectEntity(item);
			this.dummyCharacter.SelectedItem = item;
			this.FilterEntities(this.entityFilterBox.Text);
			MapEntity.StopSelection();
		}

		// Token: 0x0600275F RID: 10079 RVA: 0x001B1444 File Offset: 0x001AF644
		private void CloseItem()
		{
			if (this.dummyCharacter == null)
			{
				return;
			}
			if (SubEditorScreen.DraggedItemPrefab == null)
			{
				Character character = this.dummyCharacter;
				if (((character != null) ? character.SelectedItem : null) == null && this.OpenedItem == null)
				{
					return;
				}
			}
			SubEditorScreen.DraggedItemPrefab = null;
			this.dummyCharacter.SelectedItem = null;
			Item openedItem = this.OpenedItem;
			if (openedItem != null)
			{
				openedItem.Drop(this.dummyCharacter, true, true);
			}
			Item openedItem2 = this.OpenedItem;
			if (openedItem2 != null)
			{
				openedItem2.SetTransform(this.oldItemPosition, 0f, true, true, null);
			}
			this.OpenedItem = null;
			this.FilterEntities(this.entityFilterBox.Text);
		}

		// Token: 0x06002760 RID: 10080 RVA: 0x001B14E0 File Offset: 0x001AF6E0
		private void TeleportDummyCharacter(Vector2 pos)
		{
			if (this.dummyCharacter != null)
			{
				foreach (Limb limb in this.dummyCharacter.AnimController.Limbs)
				{
					limb.body.SetTransform(pos, 0f, true);
				}
				this.dummyCharacter.AnimController.Collider.SetTransform(pos, 0f, true);
			}
		}

		// Token: 0x06002761 RID: 10081 RVA: 0x001B1548 File Offset: 0x001AF748
		private bool ChangeSubName(GUITextBox textBox, string text)
		{
			if (string.IsNullOrWhiteSpace(text))
			{
				textBox.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
				return false;
			}
			if (SubEditorScreen.MainSub != null)
			{
				SubEditorScreen.MainSub.Info.Name = text;
			}
			textBox.Deselect();
			textBox.Text = text;
			textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
			return true;
		}

		// Token: 0x06002762 RID: 10082 RVA: 0x001B15D0 File Offset: 0x001AF7D0
		private bool ChangeMissionTags(GUITextBox textBox, string text)
		{
			Submarine mainSub = SubEditorScreen.MainSub;
			ExtraSubmarineInfo extraSubInfo = SubEditorScreen.GetExtraSubmarineInfo((mainSub != null) ? mainSub.Info : null);
			if (((extraSubInfo != null) ? extraSubInfo.MissionTags : null) != null)
			{
				extraSubInfo.MissionTags.Clear();
				string[] tags = text.Split(',', StringSplitOptions.None);
				foreach (string tag in tags)
				{
					extraSubInfo.MissionTags.Add(tag.ToIdentifier());
				}
			}
			textBox.Text = text;
			textBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
			return true;
		}

		// Token: 0x06002763 RID: 10083 RVA: 0x001B1670 File Offset: 0x001AF870
		private static GUITextBox CreateMissionTagsUI(GUIComponent parent, IEnumerable<Identifier> missionTags, GUITextBox.OnEnterHandler onEnterPressed)
		{
			GUILayoutGroup missionTagsGroup = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
			{
				Stretch = true
			};
			new GUITextBlock(new RectTransform(new Vector2(0.6f, 1f), missionTagsGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("subeditor.missiontags"), null, null, Alignment.CenterLeft, true, "", null);
			GUITextBox tagsBox = new GUITextBox(new RectTransform(new Vector2(0.4f, 1f), missionTagsGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				ToolTip = TextManager.Get("subeditor.missiontags.tooltip"),
				OnEnterPressed = onEnterPressed,
				OverflowClip = true,
				Text = ((missionTags != null) ? string.Join<Identifier>(',', missionTags) : "")
			};
			tagsBox.OnDeselected += delegate(GUITextBox textbox, Keys _)
			{
				onEnterPressed(textbox, textbox.Text);
			};
			missionTagsGroup.RectTransform.MaxSize = tagsBox.RectTransform.MaxSize;
			return tagsBox;
		}

		// Token: 0x06002764 RID: 10084 RVA: 0x001B1808 File Offset: 0x001AFA08
		private void ChangeSubDescription(GUITextBox textBox, string text)
		{
			if (SubEditorScreen.MainSub != null)
			{
				SubEditorScreen.MainSub.Info.Description = text;
			}
			else
			{
				textBox.UserData = text;
			}
			this.submarineDescriptionCharacterCount.Text = text.Length.ToString() + " / " + 500.ToString();
		}

		// Token: 0x06002765 RID: 10085 RVA: 0x001B1870 File Offset: 0x001AFA70
		private bool SelectPrefab(GUIComponent component, object obj)
		{
			SubEditorScreen.<>c__DisplayClass186_0 CS$<>8__locals1 = new SubEditorScreen.<>c__DisplayClass186_0();
			CS$<>8__locals1.<>4__this = this;
			this.allEntityList.Deselect();
			this.categorizedEntityList.Deselect();
			if (!(GUI.MouseOn is GUIButton))
			{
				GUIComponent mouseOn = GUI.MouseOn;
				if (!(((mouseOn != null) ? mouseOn.Parent : null) is GUIButton))
				{
					this.AddPreviouslyUsed(obj as MapEntityPrefab);
					CS$<>8__locals1.prefab = (obj as CoreEntityPrefab);
					if (CS$<>8__locals1.prefab != null)
					{
						GUITickBox matchingTickBox = this.showEntitiesTickBoxes.Find((GUITickBox tb) => tb.UserData as string == CS$<>8__locals1.prefab.Identifier);
						if (matchingTickBox != null && !matchingTickBox.Selected)
						{
							this.previouslyUsedPanel.Visible = false;
							this.showEntitiesPanel.Visible = true;
							this.showEntitiesPanel.RectTransform.AbsoluteOffset = new Point(Math.Max(this.entityCountPanel.Rect.Right, this.saveAssemblyFrame.Rect.Right), this.TopPanel.Rect.Height);
							matchingTickBox.Selected = true;
							matchingTickBox.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
						}
					}
					Character character = this.dummyCharacter;
					if (((character != null) ? character.SelectedItem : null) != null)
					{
						SubEditorScreen.<>c__DisplayClass186_1 CS$<>8__locals2 = new SubEditorScreen.<>c__DisplayClass186_1();
						CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
						SubEditorScreen.<>c__DisplayClass186_1 CS$<>8__locals3 = CS$<>8__locals2;
						Character character2 = this.dummyCharacter;
						ItemInventory inv;
						if (character2 == null)
						{
							inv = null;
						}
						else
						{
							Item selectedItem = character2.SelectedItem;
							inv = ((selectedItem != null) ? selectedItem.OwnInventory : null);
						}
						CS$<>8__locals3.inv = inv;
						if (CS$<>8__locals2.inv != null)
						{
							ItemAssemblyPrefab assemblyPrefab = obj as ItemAssemblyPrefab;
							if (assemblyPrefab == null)
							{
								ItemPrefab itemPrefab = obj as ItemPrefab;
								if (itemPrefab == null)
								{
									return false;
								}
								if (PlayerInput.IsShiftDown())
								{
									Item item = new Item(itemPrefab, Vector2.Zero, SubEditorScreen.MainSub, 0, true);
									if (!CS$<>8__locals2.inv.TryPutItem(item, this.dummyCharacter, null, true, false, true))
									{
										SoundPlayer.PlayUISound(GUISoundType.PickItemFail);
										item.Remove();
									}
									else
									{
										SoundPlayer.PlayUISound(GUISoundType.PickItem);
									}
									if (!item.Removed)
									{
										SubEditorScreen.StoreCommand(new AddOrDeleteCommand(new List<MapEntity>
										{
											item
										}, false, true));
										return false;
									}
									return false;
								}
							}
							else if (PlayerInput.IsShiftDown())
							{
								List<Item> itemInstance = this.LoadItemAssemblyInventorySafe(assemblyPrefab);
								CS$<>8__locals2.spawnedItem = false;
								itemInstance.ForEach(delegate(Item newItem)
								{
									if (newItem != null)
									{
										bool placedItem = CS$<>8__locals2.inv.TryPutItem(newItem, CS$<>8__locals2.CS$<>8__locals1.<>4__this.dummyCharacter, null, true, false, true);
										CS$<>8__locals2.spawnedItem = (CS$<>8__locals2.spawnedItem || placedItem);
										if (!placedItem)
										{
											ItemInventory ownInventory = newItem.OwnInventory;
											if (ownInventory != null)
											{
												ownInventory.DeleteAllItems();
											}
											newItem.Remove();
										}
									}
								});
								List<MapEntity> placedEntities = (from it in itemInstance
								where !it.Removed
								select it).Cast<MapEntity>().ToList<MapEntity>();
								if (placedEntities.Any<MapEntity>())
								{
									SubEditorScreen.StoreCommand(new AddOrDeleteCommand(placedEntities, false, true));
								}
								SoundPlayer.PlayUISound(CS$<>8__locals2.spawnedItem ? GUISoundType.PickItem : GUISoundType.PickItemFail);
								return false;
							}
							SubEditorScreen.DraggedItemPrefab = (MapEntityPrefab)obj;
							SoundPlayer.PlayUISound(GUISoundType.PickItem);
						}
					}
					else
					{
						SoundPlayer.PlayUISound(GUISoundType.PickItem);
						MapEntityPrefab.SelectPrefab(obj);
					}
					return false;
				}
			}
			return false;
		}

		// Token: 0x06002766 RID: 10086 RVA: 0x001B1B42 File Offset: 0x001AFD42
		private bool GenerateWaypoints()
		{
			return SubEditorScreen.MainSub != null && WayPoint.GenerateSubWaypoints(SubEditorScreen.MainSub);
		}

		// Token: 0x06002767 RID: 10087 RVA: 0x001B1B58 File Offset: 0x001AFD58
		private void AddPreviouslyUsed(MapEntityPrefab mapEntityPrefab)
		{
			if (this.previouslyUsedList == null || mapEntityPrefab == null)
			{
				return;
			}
			this.previouslyUsedList.Deselect();
			if (this.previouslyUsedList.CountChildren == 10)
			{
				this.previouslyUsedList.RemoveChild(this.previouslyUsedList.Content.Children.Last<GUIComponent>());
			}
			GUIComponent existing = this.previouslyUsedList.Content.FindChild(mapEntityPrefab, false);
			if (existing != null)
			{
				this.previouslyUsedList.Content.RemoveChild(existing);
			}
			RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.05f), this.previouslyUsedList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MinSize = new Point(0, 15);
			RichString text = ToolBox.LimitString(mapEntityPrefab.Name.Value, GUIStyle.SmallFont, this.previouslyUsedList.Content.Rect.Width);
			GUIFont smallFont = GUIStyle.SmallFont;
			GUITextBlock textBlock = new GUITextBlock(rectTransform, text, null, smallFont, Alignment.Left, false, "", null)
			{
				UserData = mapEntityPrefab
			};
			textBlock.RectTransform.SetAsFirstChild();
		}

		// Token: 0x06002768 RID: 10088 RVA: 0x001B1C8C File Offset: 0x001AFE8C
		public void AutoHull()
		{
			for (int i = 0; i < MapEntity.MapEntityList.Count; i++)
			{
				MapEntity h = MapEntity.MapEntityList[i];
				if (h is Hull || h is Gap)
				{
					h.Remove();
					i--;
				}
			}
			List<Vector2> wallPoints = new List<Vector2>();
			List<MapEntity> mapEntityList = new List<MapEntity>();
			foreach (MapEntity e in MapEntity.MapEntityList)
			{
				Item it = e as Item;
				if (it != null)
				{
					Door door = it.GetComponent<Door>();
					if (door != null)
					{
						int halfW = it.WorldRect.Width / 2;
						wallPoints.Add(new Vector2((float)(it.WorldRect.X + halfW), (float)(-(float)it.WorldRect.Y + it.WorldRect.Height)));
						mapEntityList.Add(it);
					}
				}
				else if (e is Structure)
				{
					Structure s = e as Structure;
					if (s.HasBody)
					{
						mapEntityList.Add(e);
						if (e.Rect.Width > e.Rect.Height)
						{
							int halfH = e.WorldRect.Height / 2;
							wallPoints.Add(new Vector2((float)e.WorldRect.X, (float)(-(float)e.WorldRect.Y + halfH)));
							wallPoints.Add(new Vector2((float)(e.WorldRect.X + e.WorldRect.Width), (float)(-(float)e.WorldRect.Y + halfH)));
						}
						else
						{
							int halfW2 = e.WorldRect.Width / 2;
							wallPoints.Add(new Vector2((float)(e.WorldRect.X + halfW2), (float)(-(float)e.WorldRect.Y)));
							wallPoints.Add(new Vector2((float)(e.WorldRect.X + halfW2), (float)(-(float)e.WorldRect.Y + e.WorldRect.Height)));
						}
					}
				}
			}
			if (wallPoints.Count < 4)
			{
				DebugConsole.ThrowError("Generating hulls for the submarine failed. Not enough wall structures to generate hulls.", null, null, false, false);
				return;
			}
			Vector2 min = wallPoints[0];
			Vector2 max = wallPoints[0];
			for (int j = 0; j < wallPoints.Count; j++)
			{
				min.X = Math.Min(min.X, wallPoints[j].X);
				min.Y = Math.Min(min.Y, wallPoints[j].Y);
				max.X = Math.Max(max.X, wallPoints[j].X);
				max.Y = Math.Max(max.Y, wallPoints[j].Y);
			}
			List<Rectangle> hullRects = new List<Rectangle>
			{
				new Rectangle((int)min.X, (int)min.Y, (int)(max.X - min.X), (int)(max.Y - min.Y))
			};
			foreach (Vector2 point in wallPoints)
			{
				MathUtils.SplitRectanglesHorizontal(hullRects, point);
				MathUtils.SplitRectanglesVertical(hullRects, point);
			}
			hullRects.Sort(delegate(Rectangle a, Rectangle b)
			{
				if (a.Y < b.Y)
				{
					return -1;
				}
				if (a.Y > b.Y)
				{
					return 1;
				}
				if (a.X < b.X)
				{
					return -1;
				}
				if (a.X > b.X)
				{
					return 1;
				}
				return 0;
			});
			for (int k = 0; k < hullRects.Count - 1; k++)
			{
				Rectangle rect = hullRects[k];
				if (hullRects[k + 1].Y <= rect.Y)
				{
					Vector2 hullRPoint = new Vector2((float)(rect.X + rect.Width - 8), (float)(rect.Y + rect.Height / 2));
					Vector2 hullLPoint = new Vector2((float)rect.X, (float)(rect.Y + rect.Height / 2));
					MapEntity container = null;
					foreach (MapEntity e2 in mapEntityList)
					{
						Rectangle entRect = e2.WorldRect;
						entRect.Y = -entRect.Y;
						if (entRect.Contains(hullRPoint))
						{
							if (!entRect.Contains(hullLPoint))
							{
								container = e2;
								break;
							}
							break;
						}
					}
					if (container == null)
					{
						rect.Width += hullRects[k + 1].Width;
						hullRects[k] = rect;
						hullRects.RemoveAt(k + 1);
						k--;
					}
				}
			}
			foreach (MapEntity e3 in mapEntityList)
			{
				Rectangle entRect2 = e3.WorldRect;
				if (entRect2.Width >= entRect2.Height)
				{
					entRect2.Y = -entRect2.Y - 16;
					for (int l = 0; l < hullRects.Count; l++)
					{
						Rectangle hullRect = hullRects[l];
						if (entRect2.Intersects(hullRect))
						{
							if (hullRect.Y < entRect2.Y)
							{
								hullRect.Height = Math.Max(entRect2.Y + 16 + entRect2.Height / 2 - hullRect.Y, hullRect.Height);
								hullRects[l] = hullRect;
							}
							else if (hullRect.Y + hullRect.Height <= entRect2.Y + 16 + entRect2.Height)
							{
								hullRects.RemoveAt(l);
								l--;
							}
						}
					}
				}
			}
			foreach (MapEntity e4 in mapEntityList)
			{
				Rectangle entRect3 = e4.WorldRect;
				if (entRect3.Width >= entRect3.Height)
				{
					entRect3.Y = -entRect3.Y;
					for (int m = 0; m < hullRects.Count; m++)
					{
						Rectangle hullRect2 = hullRects[m];
						if (entRect3.Intersects(hullRect2) && hullRect2.Y >= entRect3.Y - 8 && hullRect2.Y + hullRect2.Height <= entRect3.Y + entRect3.Height + 8)
						{
							hullRects.RemoveAt(m);
							m--;
						}
					}
				}
			}
			int n = 0;
			while (n < hullRects.Count)
			{
				Rectangle hullRect3 = hullRects[n];
				Vector2 point2 = new Vector2((float)(hullRect3.X + 2), (float)(hullRect3.Y + hullRect3.Height / 2));
				MapEntity container2 = null;
				foreach (MapEntity e5 in mapEntityList)
				{
					Rectangle entRect4 = e5.WorldRect;
					entRect4.Y = -entRect4.Y;
					if (entRect4.Contains(point2))
					{
						container2 = e5;
						break;
					}
				}
				if (container2 == null)
				{
					hullRects.RemoveAt(n);
				}
				else
				{
					while (hullRects[n].Y <= hullRect3.Y)
					{
						n++;
						if (n >= hullRects.Count)
						{
							break;
						}
					}
				}
			}
			int i2 = hullRects.Count - 1;
			while (i2 >= 0)
			{
				Rectangle hullRect4 = hullRects[i2];
				Vector2 point3 = new Vector2((float)(hullRect4.X + hullRect4.Width - 2), (float)(hullRect4.Y + hullRect4.Height / 2));
				MapEntity container3 = null;
				foreach (MapEntity e6 in mapEntityList)
				{
					Rectangle entRect5 = e6.WorldRect;
					entRect5.Y = -entRect5.Y;
					if (entRect5.Contains(point3))
					{
						container3 = e6;
						break;
					}
				}
				if (container3 == null)
				{
					hullRects.RemoveAt(i2);
					i2--;
				}
				else
				{
					while (hullRects[i2].Y >= hullRect4.Y)
					{
						i2--;
						if (i2 < 0)
						{
							break;
						}
					}
				}
			}
			hullRects.Sort(delegate(Rectangle a, Rectangle b)
			{
				if (a.X < b.X)
				{
					return -1;
				}
				if (a.X > b.X)
				{
					return 1;
				}
				if (a.Y < b.Y)
				{
					return -1;
				}
				if (a.Y > b.Y)
				{
					return 1;
				}
				return 0;
			});
			for (int i3 = 0; i3 < hullRects.Count - 1; i3++)
			{
				Rectangle rect2 = hullRects[i3];
				if (hullRects[i3 + 1].Width == rect2.Width && hullRects[i3 + 1].X <= rect2.X)
				{
					Vector2 hullBPoint = new Vector2((float)(rect2.X + rect2.Width / 2), (float)(rect2.Y + rect2.Height - 8));
					Vector2 hullUPoint = new Vector2((float)(rect2.X + rect2.Width / 2), (float)rect2.Y);
					MapEntity container4 = null;
					foreach (MapEntity e7 in mapEntityList)
					{
						Rectangle entRect6 = e7.WorldRect;
						entRect6.Y = -entRect6.Y;
						if (entRect6.Contains(hullBPoint))
						{
							if (!entRect6.Contains(hullUPoint))
							{
								container4 = e7;
								break;
							}
							break;
						}
					}
					if (container4 == null)
					{
						rect2.Height += hullRects[i3 + 1].Height;
						hullRects[i3] = rect2;
						hullRects.RemoveAt(i3 + 1);
						i3--;
					}
				}
			}
			for (int i4 = 0; i4 < hullRects.Count; i4++)
			{
				Rectangle rect3 = hullRects[i4];
				rect3.Y -= 16;
				rect3.Height += 32;
				hullRects[i4] = rect3;
			}
			hullRects.Sort(delegate(Rectangle a, Rectangle b)
			{
				if (a.Y < b.Y)
				{
					return -1;
				}
				if (a.Y > b.Y)
				{
					return 1;
				}
				if (a.X < b.X)
				{
					return -1;
				}
				if (a.X > b.X)
				{
					return 1;
				}
				return 0;
			});
			for (int i5 = 0; i5 < hullRects.Count; i5++)
			{
				for (int j2 = i5 + 1; j2 < hullRects.Count; j2++)
				{
					if (hullRects[j2].Y > hullRects[i5].Y && hullRects[j2].Intersects(hullRects[i5]))
					{
						Rectangle rect4 = hullRects[i5];
						rect4.Height = hullRects[j2].Y - rect4.Y;
						hullRects[i5] = rect4;
						break;
					}
				}
			}
			foreach (Rectangle rect5 in hullRects)
			{
				Rectangle hullRect5 = rect5;
				hullRect5.Y = -hullRect5.Y;
				Hull newHull = new Hull(hullRect5, SubEditorScreen.MainSub, 0);
			}
			foreach (MapEntity e8 in mapEntityList)
			{
				if (e8 is Structure && (e8 as Structure).IsPlatform)
				{
					Rectangle gapRect = e8.WorldRect;
					gapRect.Y -= 8;
					gapRect.Height = 16;
					new Gap(gapRect);
				}
			}
		}

		// Token: 0x06002769 RID: 10089 RVA: 0x001B28FC File Offset: 0x001B0AFC
		public override void AddToGUIUpdateList()
		{
			if (GUI.DisableHUD)
			{
				return;
			}
			MapEntity mapEntity = MapEntity.FilteredSelectedList.FirstOrDefault<MapEntity>();
			if (mapEntity != null)
			{
				mapEntity.AddToGUIUpdateList(0);
			}
			this.EntityMenu.AddToGUIUpdateList(false, 0);
			this.showEntitiesPanel.AddToGUIUpdateList(false, 0);
			this.previouslyUsedPanel.AddToGUIUpdateList(false, 0);
			this.undoBufferPanel.AddToGUIUpdateList(false, 0);
			this.entityCountPanel.AddToGUIUpdateList(false, 0);
			this.layerPanel.AddToGUIUpdateList(false, 0);
			this.TopPanel.AddToGUIUpdateList(false, 0);
			if (this.WiringMode)
			{
				this.wiringToolPanel.AddToGUIUpdateList(false, 0);
			}
			if (MapEntity.HighlightedListBox != null)
			{
				MapEntity.HighlightedListBox.AddToGUIUpdateList(false, 0);
			}
			if (this.dummyCharacter != null)
			{
				CharacterHUD.AddToGUIUpdateList(this.dummyCharacter);
				if (this.dummyCharacter.SelectedItem != null)
				{
					this.dummyCharacter.SelectedItem.AddToGUIUpdateList(0);
				}
				else if (this.WiringMode)
				{
					Item item = MapEntity.SelectedList.FirstOrDefault<MapEntity>() as Item;
					if (item != null && item.GetComponent<Wire>() != null)
					{
						MapEntity mapEntity2 = MapEntity.SelectedList.FirstOrDefault<MapEntity>();
						if (mapEntity2 != null)
						{
							mapEntity2.AddToGUIUpdateList(0);
						}
					}
				}
			}
			if (this.loadFrame != null)
			{
				this.loadFrame.AddToGUIUpdateList(false, 0);
				return;
			}
			GUIComponent guicomponent = this.saveFrame;
			if (guicomponent == null)
			{
				return;
			}
			guicomponent.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x0600276A RID: 10090 RVA: 0x001B2A40 File Offset: 0x001B0C40
		public bool IsMouseOnEditorGUI()
		{
			if (GUI.MouseOn == null)
			{
				return false;
			}
			GUIComponent entityMenu = this.EntityMenu;
			if (entityMenu == null || !entityMenu.MouseRect.Contains(PlayerInput.MousePosition))
			{
				GUIComponent guicomponent = this.entityCountPanel;
				if (guicomponent == null || !guicomponent.MouseRect.Contains(PlayerInput.MousePosition))
				{
					GUIComponent editingHUD = MapEntity.EditingHUD;
					if (editingHUD == null || !editingHUD.MouseRect.Contains(PlayerInput.MousePosition))
					{
						GUIComponent topPanel = this.TopPanel;
						return topPanel != null && topPanel.MouseRect.Contains(PlayerInput.MousePosition);
					}
				}
			}
			return true;
		}

		// Token: 0x0600276B RID: 10091 RVA: 0x001B2AD8 File Offset: 0x001B0CD8
		private static void Redo(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				if (SubEditorScreen.commandIndex < SubEditorScreen.Commands.Count)
				{
					Command command = SubEditorScreen.Commands[SubEditorScreen.commandIndex++];
					command.Execute();
				}
			}
			GameMain.SubEditorScreen.UpdateUndoHistoryPanel();
		}

		// Token: 0x0600276C RID: 10092 RVA: 0x001B2B2C File Offset: 0x001B0D2C
		private static void Undo(int amount)
		{
			for (int i = 0; i < amount; i++)
			{
				if (SubEditorScreen.commandIndex > 0)
				{
					Command command = SubEditorScreen.Commands[--SubEditorScreen.commandIndex];
					command.UnExecute();
				}
			}
			GameMain.SubEditorScreen.UpdateUndoHistoryPanel();
		}

		// Token: 0x0600276D RID: 10093 RVA: 0x001B2B78 File Offset: 0x001B0D78
		private static void ClearUndoBuffer()
		{
			SerializableEntityEditor.PropertyChangesActive = false;
			SerializableEntityEditor.CommandBuffer = null;
			SubEditorScreen.Commands.ForEach(delegate(Command cmd)
			{
				cmd.Cleanup();
			});
			SubEditorScreen.Commands.Clear();
			SubEditorScreen.commandIndex = 0;
			GameMain.SubEditorScreen.UpdateUndoHistoryPanel();
		}

		// Token: 0x0600276E RID: 10094 RVA: 0x001B2BD4 File Offset: 0x001B0DD4
		public static void StoreCommand(Command command)
		{
			if (SubEditorScreen.commandIndex != SubEditorScreen.Commands.Count)
			{
				SubEditorScreen.Commands.RemoveRange(SubEditorScreen.commandIndex, SubEditorScreen.Commands.Count - SubEditorScreen.commandIndex);
			}
			SubEditorScreen.Commands.Add(command);
			SubEditorScreen.commandIndex++;
			if (SubEditorScreen.Commands.Count > Math.Clamp(GameSettings.CurrentConfig.SubEditorUndoBuffer, 1, 10240))
			{
				Command command2 = SubEditorScreen.Commands.First<Command>();
				if (command2 != null)
				{
					command2.Cleanup();
				}
				SubEditorScreen.Commands.RemoveRange(0, 1);
				SubEditorScreen.commandIndex = SubEditorScreen.Commands.Count;
			}
			GameMain.SubEditorScreen.UpdateUndoHistoryPanel();
			AddOrDeleteCommand addOrDelete = command as AddOrDeleteCommand;
			if (addOrDelete != null)
			{
				GameMain.SubEditorScreen.EntityAddedOrDeleted(addOrDelete.Receivers);
			}
		}

		// Token: 0x0600276F RID: 10095 RVA: 0x001B2CA0 File Offset: 0x001B0EA0
		private void EntityAddedOrDeleted(IEnumerable<MapEntity> entities)
		{
			GUIListBox guilistBox = this.layerList;
			string selectedLayer = ((guilistBox != null) ? guilistBox.SelectedData : null) as string;
			if (selectedLayer != null)
			{
				foreach (MapEntity entity in entities)
				{
					if (!entity.Removed)
					{
						entity.Layer = selectedLayer;
					}
				}
				GUIComponent layerElement = this.layerList.Content.FindChild(selectedLayer, false);
				if (layerElement != null)
				{
					layerElement.Flash(new Color?(GUIStyle.Green), 1.5f, false, false, null);
				}
			}
		}

		// Token: 0x06002770 RID: 10096 RVA: 0x001B2D48 File Offset: 0x001B0F48
		private void UpdateLayerPanel()
		{
			if (this.layerPanel == null || this.layerList == null)
			{
				return;
			}
			this.layerList.Content.ClearChildren();
			this.layerList.Deselect();
			this.layerSpecificButtons.ForEach(delegate(GUIButton btn)
			{
				btn.Enabled = false;
			});
			GUILayoutGroup buttonHeaders = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.075f), this.layerList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.BottomLeft);
			new GUIButton(new RectTransform(new Vector2(0.25f, 1f), buttonHeaders.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.layer.headervisible"), Alignment.Center, "GUIButtonSmallFreeScale", null).ForceUpperCase = ForceUpperCase.Yes;
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.15f, 1f), buttonHeaders.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.layer.headerlink"), Alignment.Center, "GUIButtonSmallFreeScale", null);
			guibutton.ForceUpperCase = ForceUpperCase.Yes;
			guibutton.ToolTip = TextManager.Get("editor.layer.headerlink.tooltip");
			new GUIButton(new RectTransform(new Vector2(0.6f, 1f), buttonHeaders.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("name"), Alignment.Center, "GUIButtonSmallFreeScale", null).ForceUpperCase = ForceUpperCase.Yes;
			foreach (KeyValuePair<string, SubEditorScreen.LayerData> keyValuePair in SubEditorScreen.Layers)
			{
				string layer3;
				SubEditorScreen.LayerData layerData;
				keyValuePair.Deconstruct(out layer3, out layerData);
				SubEditorScreen.LayerData layerData2 = layerData;
				bool flag;
				bool flag2;
				layerData2.Deconstruct(out flag, out flag2);
				string layer = layer3;
				bool isVisible = flag;
				bool isGrouped = flag2;
				GUIFrame parent = new GUIFrame(new RectTransform(new Vector2(1f, 0.1f), this.layerList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
				{
					UserData = layer
				};
				GUILayoutGroup layerGroup = new GUILayoutGroup(new RectTransform(Vector2.One, parent.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
				GUILayoutGroup layerVisibilityLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.25f, 1f), layerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
				GUITickBox guitickBox = new GUITickBox(new RectTransform(Vector2.One, layerVisibilityLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), string.Empty, null, "");
				guitickBox.Selected = isVisible;
				guitickBox.OnSelected = delegate(GUITickBox box)
				{
					SubEditorScreen.LayerData data;
					if (!SubEditorScreen.Layers.TryGetValue(layer, out data))
					{
						this.UpdateLayerPanel();
						return false;
					}
					if (!box.Selected && this.layerList.SelectedData as string == layer && !box.Selected)
					{
						this.layerList.Deselect();
					}
					Dictionary<string, SubEditorScreen.LayerData> layers = SubEditorScreen.Layers;
					string layer2 = layer;
					SubEditorScreen.LayerData value = data;
					value.IsVisible = box.Selected;
					layers[layer2] = value;
					return true;
				};
				GUILayoutGroup layerChainLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.15f, 1f), layerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
				GUITickBox guitickBox2 = new GUITickBox(new RectTransform(Vector2.One, layerChainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), string.Empty, null, "");
				guitickBox2.Selected = isGrouped;
				guitickBox2.OnSelected = delegate(GUITickBox box)
				{
					SubEditorScreen.LayerData data;
					if (!SubEditorScreen.Layers.TryGetValue(layer, out data))
					{
						this.UpdateLayerPanel();
						return false;
					}
					Dictionary<string, SubEditorScreen.LayerData> layers = SubEditorScreen.Layers;
					string layer2 = layer;
					SubEditorScreen.LayerData value = data;
					value.IsGrouped = box.Selected;
					layers[layer2] = value;
					return true;
				};
				layerGroup.Recalculate();
				GUITextBlock textBlock = new GUITextBlock(new RectTransform(new Vector2(0.6f, 1f), layerGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), layer, null, null, Alignment.CenterLeft, false, "", null);
				if (textBlock.TextSize.X > (float)textBlock.Rect.Width)
				{
					textBlock.ToolTip = textBlock.Text;
					textBlock.Text = ToolBox.LimitString(textBlock.Text, textBlock.Font, textBlock.Rect.Width);
				}
				layerGroup.Recalculate();
				layerChainLayout.Recalculate();
				layerVisibilityLayout.Recalculate();
			}
			this.layerList.RecalculateChildren();
			buttonHeaders.Recalculate();
			foreach (GUIComponent child in buttonHeaders.Children)
			{
				GUIButton btn2 = child as GUIButton;
				string originalBtnText = btn2.Text.Value;
				btn2.Text = ToolBox.LimitString(btn2.Text, btn2.Font, btn2.Rect.Width);
				if (originalBtnText != btn2.Text && btn2.ToolTip.IsNullOrEmpty())
				{
					btn2.ToolTip = originalBtnText;
				}
			}
		}

		// Token: 0x06002771 RID: 10097 RVA: 0x001B3328 File Offset: 0x001B1528
		public void UpdateUndoHistoryPanel()
		{
			if (this.undoBufferPanel == null)
			{
				return;
			}
			this.undoBufferDisclaimer.Visible = (this.mode == SubEditorScreen.Mode.Wiring);
			this.undoBufferList.Content.Children.ForEachMod(delegate(GUIComponent component)
			{
				this.undoBufferList.Content.RemoveChild(component);
			});
			for (int i = 0; i < SubEditorScreen.Commands.Count; i++)
			{
				Command command = SubEditorScreen.Commands[i];
				LocalizedString description = command.GetDescription();
				this.<UpdateUndoHistoryPanel>g__CreateTextBlock|199_1(description, description, i + 1, command).RectTransform.SetAsFirstChild();
			}
			this.<UpdateUndoHistoryPanel>g__CreateTextBlock|199_1(TextManager.Get("undo.beginning"), TextManager.Get("undo.beginningtooltip"), 0, null);
		}

		// Token: 0x06002772 RID: 10098 RVA: 0x001B33D0 File Offset: 0x001B15D0
		private static void CommitBulkItemBuffer()
		{
			if (SubEditorScreen.BulkItemBuffer.Any<AddOrDeleteCommand>())
			{
				AddOrDeleteCommand master = SubEditorScreen.BulkItemBuffer[0];
				for (int i = 1; i < SubEditorScreen.BulkItemBuffer.Count; i++)
				{
					AddOrDeleteCommand command = SubEditorScreen.BulkItemBuffer[i];
					command.MergeInto(master);
				}
				SubEditorScreen.StoreCommand(master);
				SubEditorScreen.BulkItemBuffer.Clear();
			}
			SubEditorScreen.bulkItemBufferinUse = null;
		}

		// Token: 0x06002773 RID: 10099 RVA: 0x001B3434 File Offset: 0x001B1634
		public override void Update(double deltaTime)
		{
			SubEditorScreen.SkipInventorySlotUpdate = false;
			SubEditorScreen.ImageManager.Update((float)deltaTime);
			Hull.UpdateCheats((float)deltaTime, this.cam);
			if (GameMain.GraphicsWidth != this.screenResolution.X || GameMain.GraphicsHeight != this.screenResolution.Y)
			{
				this.saveFrame = null;
				this.loadFrame = null;
				this.saveAssemblyFrame = null;
				this.snapToGridFrame = null;
				this.CreateUI();
				this.UpdateEntityList();
			}
			if (this.OpenedItem != null && this.OpenedItem.Removed)
			{
				this.OpenedItem = null;
			}
			if (this.WiringMode && this.dummyCharacter != null)
			{
				Character controlled = Character.Controlled;
				bool flag;
				if (controlled == null)
				{
					flag = (null != null);
				}
				else
				{
					Item item9 = controlled.HeldItems.FirstOrDefault((Item it) => it.GetComponent<Wire>() != null);
					flag = (((item9 != null) ? item9.GetComponent<Wire>() : null) != null);
				}
				if ((flag ?? Wire.DraggingWire) == null)
				{
					if (MapEntity.HighlightedListBox != null)
					{
						GUIListBox lBox = MapEntity.HighlightedListBox;
						foreach (GUIComponent child in lBox.Content.Children)
						{
							Item item = child.UserData as Item;
							if (item != null)
							{
								item.ExternalHighlight = GUI.IsMouseOn(child);
							}
						}
					}
					List<MapEntity> highlightedEntities = new List<MapEntity>();
					foreach (Item item2 in (from entity in MapEntity.MapEntityList
					where entity is Item
					select entity).Cast<Item>())
					{
						Wire wire = item2.GetComponent<Wire>();
						if (wire != null && wire.IsMouseOn())
						{
							highlightedEntities.Add(item2);
						}
					}
					MapEntity.UpdateHighlighting(highlightedEntities, true);
				}
			}
			this.hullVolumeFrame.Visible = MapEntity.SelectedList.Any((MapEntity s) => s is Hull);
			this.hullVolumeFrame.RectTransform.AbsoluteOffset = new Point(Math.Max(this.showEntitiesPanel.Rect.Right, this.previouslyUsedPanel.Rect.Right), 0);
			Character character = this.dummyCharacter;
			object obj;
			if (character == null)
			{
				obj = null;
			}
			else
			{
				Item selectedItem = character.SelectedItem;
				obj = ((selectedItem != null) ? selectedItem.GetComponent<CircuitBox>() : null);
			}
			bool isCircuitBoxOpened = obj != null;
			this.saveAssemblyFrame.Visible = (MapEntity.SelectedList.Count > 0 && !this.WiringMode && !isCircuitBoxOpened);
			this.snapToGridFrame.Visible = (MapEntity.SelectedList.Count > 0 && !this.WiringMode && !isCircuitBoxOpened);
			float offset = (float)this.cam.WorldView.Top - this.cam.ScreenToWorld(new Vector2(0f, (float)(GameMain.GraphicsHeight - this.EntityMenu.Rect.Top))).Y;
			if (this.camTargetFocus != Vector2.Zero)
			{
				if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Up].IsDown() || GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Down].IsDown() || GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Left].IsDown() || GameSettings.CurrentConfig.KeyMap.Bindings[InputType.Right].IsDown())
				{
					this.camTargetFocus = Vector2.Zero;
				}
				else
				{
					Vector2 targetWithOffset = new Vector2(this.camTargetFocus.X, this.camTargetFocus.Y - offset / 2f);
					if (Math.Abs(this.cam.Position.X - targetWithOffset.X) < 1f && Math.Abs(this.cam.Position.Y - targetWithOffset.Y) < 1f)
					{
						this.camTargetFocus = Vector2.Zero;
					}
					else
					{
						this.cam.Position += (targetWithOffset - this.cam.Position) / this.cam.MoveSmoothness;
					}
				}
			}
			if (this.undoBufferPanel.Visible)
			{
				this.undoBufferList.Deselect();
			}
			if (GUI.KeyboardDispatcher.Subscriber != null)
			{
				if (MapEntity.EditingHUD == null)
				{
					goto IL_4DF;
				}
				GUIComponent sub = GUI.KeyboardDispatcher.Subscriber as GUIComponent;
				if (sub == null || !MapEntity.EditingHUD.Children.Contains(sub))
				{
					goto IL_4DF;
				}
			}
			if (PlayerInput.IsCtrlDown() && !this.WiringMode)
			{
				if (PlayerInput.KeyHit(Keys.Z))
				{
					if (PlayerInput.IsShiftDown())
					{
						SubEditorScreen.Redo(1);
					}
					else
					{
						SubEditorScreen.Undo(1);
					}
				}
				if (PlayerInput.KeyHit(Keys.Y))
				{
					SubEditorScreen.Redo(1);
				}
			}
			IL_4DF:
			if (GUI.KeyboardDispatcher.Subscriber == null)
			{
				if (this.WiringMode && this.dummyCharacter != null)
				{
					GUIListBox listBox = this.wiringToolPanel.GetChild<GUIListBox>();
					if (listBox != null)
					{
						if (!this.dummyCharacter.HeldItems.Any((Item it) => it.HasTag(Tags.WireItem)))
						{
							listBox.Deselect();
						}
						List<Keys> numberKeys = PlayerInput.NumberKeys;
						List<Keys> list = numberKeys;
						Predicate<Keys> match;
						if ((match = SubEditorScreen.<>O.<4>__KeyHit) == null)
						{
							match = (SubEditorScreen.<>O.<4>__KeyHit = new Predicate<Keys>(PlayerInput.KeyHit));
						}
						Keys key = list.Find(match);
						if (key != Keys.None)
						{
							int index = (key == Keys.D0) ? numberKeys.Count : (numberKeys.IndexOf(key) - 1);
							if (index > -1 && index < listBox.Content.CountChildren)
							{
								listBox.Select(index, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled, GUIListBox.TakeKeyBoardFocus.No, GUIListBox.PlaySelectSound.No);
								SubEditorScreen.SkipInventorySlotUpdate = true;
							}
						}
					}
				}
				if (this.mode == SubEditorScreen.Mode.Default)
				{
					if (PlayerInput.KeyHit(InputType.Use) && this.dummyCharacter != null)
					{
						if (this.dummyCharacter.SelectedItem == null)
						{
							using (IEnumerator<MapEntity> enumerator3 = MapEntity.HighlightedEntities.GetEnumerator())
							{
								while (enumerator3.MoveNext())
								{
									MapEntity entity2 = enumerator3.Current;
									Item item3 = entity2 as Item;
									if (item3 != null)
									{
										if (item3.Components.Any((ItemComponent ic) => !(ic is ConnectionPanel) && !(ic is Repairable) && ic.GuiFrame != null))
										{
											List<ItemContainer> container = item3.GetComponents<ItemContainer>().ToList<ItemContainer>();
											if (!container.None(null))
											{
												if (!container.Any((ItemContainer ic) => ic != null && ic.DrawInventory) && item3.GetComponent<CircuitBox>() == null)
												{
													continue;
												}
											}
											this.OpenItem(item3);
											break;
										}
									}
								}
								goto IL_6BE;
							}
						}
						this.CloseItem();
					}
					IL_6BE:
					if (PlayerInput.KeyHit(Keys.F))
					{
						HashSet<MapEntity> selected = MapEntity.SelectedList;
						if (selected.Count > 0)
						{
							Rectangle dRect = selected.First<MapEntity>().Rect;
							Rectangle rect = new Rectangle(dRect.Left, dRect.Top, dRect.Width, dRect.Height * -1);
							if (selected.Count > 1)
							{
								selected.Skip(1).ForEach(delegate(MapEntity me)
								{
									Rectangle wRect = me.Rect;
									rect = Rectangle.Union(rect, new Rectangle(wRect.Left, wRect.Top, wRect.Width, wRect.Height * -1));
								});
							}
							this.camTargetFocus = rect.Center.ToVector2();
						}
					}
					if (PlayerInput.KeyHit(Keys.Tab))
					{
						this.entityFilterBox.Select(-1, false);
					}
				}
				if (this.toggleEntityListBind != GameSettings.CurrentConfig.KeyMap.Bindings[InputType.ToggleInventory])
				{
					GUIComponent guicomponent = this.toggleEntityMenuButton;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 2);
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("EntityMenuToggleTooltip"));
					defaultInterpolatedStringHandler.AppendLiteral("\n‖color:125,125,125‖");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(GameSettings.CurrentConfig.KeyMap.Bindings[InputType.ToggleInventory].Name);
					defaultInterpolatedStringHandler.AppendLiteral("‖color:end‖");
					guicomponent.ToolTip = RichString.Rich(defaultInterpolatedStringHandler.ToStringAndClear(), null);
					this.toggleEntityListBind = GameSettings.CurrentConfig.KeyMap.Bindings[InputType.ToggleInventory];
				}
				if (GameSettings.CurrentConfig.KeyMap.Bindings[InputType.ToggleInventory].IsHit() && this.mode == SubEditorScreen.Mode.Default)
				{
					GUIButton.OnClickedHandler onClicked = this.toggleEntityMenuButton.OnClicked;
					if (onClicked != null)
					{
						onClicked(this.toggleEntityMenuButton, this.toggleEntityMenuButton.UserData);
					}
				}
				if (PlayerInput.IsCtrlDown() && MapEntity.StartMovingPos == Vector2.Zero)
				{
					this.cam.MoveCamera((float)deltaTime, false, GUI.MouseOn == null, true, null);
					if (PlayerInput.KeyHit(Keys.S))
					{
						if (PlayerInput.IsShiftDown())
						{
							this.CreateSaveScreen(this.subNameLabel != null && this.subNameLabel.Text != TextManager.Get("unspecifiedsubfilename"));
						}
						else
						{
							this.CreateSaveScreen(false);
						}
					}
					if (PlayerInput.KeyHit(Keys.A) && this.mode == SubEditorScreen.Mode.Default)
					{
						if (MapEntity.SelectedList.Any<MapEntity>())
						{
							MapEntity.DeselectAll();
						}
						else
						{
							List<MapEntity> selectables = (from entity in MapEntity.MapEntityList
							where entity.SelectableInEditor
							select entity).ToList<MapEntity>();
							foreach (Item item4 in Item.ItemList)
							{
								Wire wire2 = item4.GetComponent<Wire>();
								if (wire2 != null)
								{
									if (wire2.Connections.None((Connection c) => c == null) && !selectables.Contains(item4))
									{
										selectables.Add(item4);
									}
								}
							}
							List<MapEntity> obj2 = selectables;
							lock (obj2)
							{
								List<MapEntity> list2 = selectables;
								Action<MapEntity> action;
								if ((action = SubEditorScreen.<>O.<5>__AddSelection) == null)
								{
									action = (SubEditorScreen.<>O.<5>__AddSelection = new Action<MapEntity>(MapEntity.AddSelection));
								}
								list2.ForEach(action);
							}
						}
					}
					if (PlayerInput.KeyHit(Keys.D1))
					{
						this.SetMode(SubEditorScreen.Mode.Default);
					}
					if (PlayerInput.KeyHit(Keys.D2))
					{
						this.SetMode(SubEditorScreen.Mode.Wiring);
					}
				}
				else
				{
					this.cam.MoveCamera((float)deltaTime, !CircuitBox.IsCircuitBoxSelected(this.dummyCharacter), GUI.MouseOn == null, true, null);
				}
			}
			else
			{
				this.cam.MoveCamera((float)deltaTime, false, GUI.MouseOn == null, true, null);
			}
			if (PlayerInput.MidButtonHeld() && !CircuitBox.IsCircuitBoxSelected(this.dummyCharacter))
			{
				Vector2 moveSpeed = PlayerInput.MouseSpeed * (float)deltaTime * 60f / this.cam.Zoom;
				moveSpeed.X = -moveSpeed.X;
				this.cam.Position += moveSpeed;
				this.camTargetFocus = Vector2.Zero;
			}
			if (PlayerInput.KeyHit(Keys.Escape) && this.dummyCharacter != null)
			{
				this.CloseItem();
			}
			if (this.lightingEnabled)
			{
				foreach (Item item5 in Item.ItemList)
				{
					foreach (LightComponent lightComponent in item5.GetComponents<LightComponent>())
					{
						bool visibleInContainer = item5.FindParentInventory(delegate(Inventory it)
						{
							ItemInventory itemInventory = it as ItemInventory;
							if (itemInventory != null)
							{
								ItemContainer container2 = itemInventory.Container;
								if (container2 != null)
								{
									return container2.HideItems;
								}
							}
							return false;
						}) == null;
						lightComponent.Light.Color = ((((item5.body == null || !item5.body.Enabled) && !visibleInContainer) || lightComponent.Parent is Wearable) ? Color.Transparent : lightComponent.LightColor);
						lightComponent.Light.LightSpriteEffect = lightComponent.Item.SpriteEffects;
					}
				}
				LightManager lightManager = GameMain.LightManager;
				if (lightManager != null)
				{
					lightManager.Update((float)deltaTime);
				}
			}
			if (this.dummyCharacter != null && Entity.FindEntityByID(this.dummyCharacter.ID) == this.dummyCharacter)
			{
				if (this.WiringMode)
				{
					MapEntity.ClearHighlightedEntities();
					if (this.dummyCharacter.SelectedItem == null)
					{
						List<Wire> wires = new List<Wire>();
						foreach (Item item6 in Item.ItemList)
						{
							Wire wire3 = item6.GetComponent<Wire>();
							if (wire3 != null)
							{
								wires.Add(wire3);
							}
						}
						Wire.UpdateEditing(wires);
					}
				}
				if (!this.WiringMode)
				{
					this.dummyCharacter.Inventory.visualSlots.ForEach(delegate(VisualSlot slot)
					{
						slot.Rect.Y = this.EntityMenu.Rect.Top;
						slot.Rect.X = this.EntityMenu.Rect.X + this.EntityMenu.Rect.Width / 2 - slot.Rect.Width / 2;
					});
				}
				if (this.dummyCharacter.SelectedItem == null || this.dummyCharacter.SelectedItem.GetComponent<Pickable>() != null)
				{
					if (this.WiringMode && PlayerInput.IsShiftDown())
					{
						Character controlled2 = Character.Controlled;
						Wire wire5;
						if (controlled2 == null)
						{
							wire5 = null;
						}
						else
						{
							Item item10 = controlled2.HeldItems.FirstOrDefault((Item i) => i.GetComponent<Wire>() != null);
							wire5 = ((item10 != null) ? item10.GetComponent<Wire>() : null);
						}
						Wire equippedWire = wire5;
						if (equippedWire != null && equippedWire.GetNodes().Count > 0)
						{
							Vector2 lastNode = equippedWire.GetNodes().Last<Vector2>();
							if (equippedWire.Item.Submarine != null)
							{
								lastNode += equippedWire.Item.Submarine.HiddenSubPosition + equippedWire.Item.Submarine.Position;
							}
							float num;
							float num2;
							this.dummyCharacter.CursorPosition.Deconstruct(out num, out num2);
							float cursorX = num;
							float cursorY = num2;
							bool isHorizontal = Math.Abs(cursorX - lastNode.X) < Math.Abs(cursorY - lastNode.Y);
							float roundedY = MathUtils.Round(cursorY, Submarine.GridSize.Y / 2f);
							float roundedX = MathUtils.Round(cursorX, Submarine.GridSize.X / 2f);
							this.dummyCharacter.CursorPosition = (isHorizontal ? new Vector2(lastNode.X, roundedY) : new Vector2(roundedX, lastNode.Y));
						}
					}
					if (this.OpenedItem != null)
					{
						this.TeleportDummyCharacter(this.oldItemPosition);
					}
					if (this.WiringMode)
					{
						Character character2 = this.dummyCharacter;
						if (((character2 != null) ? character2.SelectedItem : null) == null)
						{
							this.TeleportDummyCharacter(ConvertUnits.ToSimUnits(this.dummyCharacter.CursorPosition));
						}
					}
				}
				if (this.WiringMode)
				{
					this.dummyCharacter.ControlLocalPlayer((float)deltaTime, this.cam, false);
					this.dummyCharacter.Control((float)deltaTime, this.cam);
				}
				this.cam.TargetPos = Vector2.Zero;
				this.dummyCharacter.Submarine = SubEditorScreen.MainSub;
			}
			Character character3 = this.dummyCharacter;
			if (((character3 != null) ? character3.SelectedItem : null) != null)
			{
				this.<Update>g__TryDragItemsToItem|201_2(this.dummyCharacter.SelectedItem);
				foreach (Item linkedItem in this.dummyCharacter.SelectedItem.linkedTo.OfType<Item>())
				{
					this.<Update>g__TryDragItemsToItem|201_2(linkedItem);
				}
			}
			if (PlayerInput.PrimaryMouseButtonReleased() && SubEditorScreen.BulkItemBufferInUse != null)
			{
				SubEditorScreen.CommitBulkItemBuffer();
			}
			if (SerializableEntityEditor.PropertyChangesActive && (SerializableEntityEditor.NextCommandPush < DateTime.Now || MapEntity.EditingHUD == null))
			{
				SerializableEntityEditor.CommitCommandBuffer();
			}
			if (PlayerInput.PrimaryMouseButtonHeld())
			{
				if (SubEditorScreen.MouseDragStart == Vector2.Zero)
				{
					SubEditorScreen.MouseDragStart = PlayerInput.MousePosition;
				}
			}
			else
			{
				SubEditorScreen.MouseDragStart = Vector2.Zero;
			}
			if (GUI.MouseOn == null || !GUI.MouseOn.IsChildOf(this.TopPanel, true))
			{
				Character character4 = this.dummyCharacter;
				if (((character4 != null) ? character4.SelectedItem : null) == null && !this.WiringMode && (GUI.MouseOn == null || MapEntity.SelectedAny || MapEntity.SelectionPos != Vector2.Zero))
				{
					GUIListBox guilistBox = this.layerList;
					if (guilistBox != null && guilistBox.Visible && GUI.KeyboardDispatcher.Subscriber == this.layerList)
					{
						GUI.KeyboardDispatcher.Subscriber = null;
					}
					MapEntity.UpdateSelecting(this.cam);
				}
			}
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				this.MeasurePositionStart = Vector2.Zero;
			}
			if ((PlayerInput.KeyDown(Keys.LeftAlt) || PlayerInput.KeyDown(Keys.RightAlt)) && PlayerInput.PrimaryMouseButtonDown())
			{
				this.MeasurePositionStart = this.cam.ScreenToWorld(PlayerInput.MousePosition);
			}
			if (!this.WiringMode)
			{
				Character character5 = this.dummyCharacter;
				bool shouldCloseHud = ((character5 != null) ? character5.SelectedItem : null) != null && HUD.CloseHUD(this.dummyCharacter.SelectedItem.Rect) && SubEditorScreen.DraggedItemPrefab == null;
				if (MapEntityPrefab.Selected != null)
				{
					MapEntityPrefab.Selected.UpdatePlacing(this.cam);
				}
				else
				{
					if (PlayerInput.SecondaryMouseButtonClicked() && !shouldCloseHud)
					{
						if (GUI.IsMouseOn(this.entityFilterBox))
						{
							this.ClearFilter();
						}
						else
						{
							Character character6 = this.dummyCharacter;
							if (((character6 != null) ? character6.SelectedItem : null) == null)
							{
								this.CreateContextMenu();
							}
							SubEditorScreen.DraggedItemPrefab = null;
						}
					}
					if (shouldCloseHud)
					{
						this.CloseItem();
					}
				}
				MapEntity.UpdateEditor(this.cam, (float)deltaTime);
			}
			this.entityMenuOpenState = ((this.entityMenuOpen && !this.WiringMode) ? ((float)Math.Min((double)this.entityMenuOpenState + deltaTime * 5.0, 1.0)) : ((float)Math.Max((double)this.entityMenuOpenState - deltaTime * 5.0, 0.0)));
			this.EntityMenu.RectTransform.ScreenSpaceOffset = Vector2.Lerp(new Vector2(0f, (float)(this.EntityMenu.Rect.Height - 10)), Vector2.Zero, this.entityMenuOpenState).ToPoint();
			if (PlayerInput.PrimaryMouseButtonClicked() && !GUI.IsMouseOn(this.entityFilterBox))
			{
				this.entityFilterBox.Deselect();
			}
			if (this.loadFrame != null)
			{
				if (PlayerInput.SecondaryMouseButtonClicked())
				{
					this.loadFrame = null;
				}
			}
			else if (this.saveFrame != null && PlayerInput.SecondaryMouseButtonClicked())
			{
				this.saveFrame = null;
			}
			if (this.dummyCharacter != null)
			{
				this.dummyCharacter.AnimController.FindHull(new Vector2?(this.dummyCharacter.CursorWorldPosition), false, false);
				foreach (Item item7 in this.dummyCharacter.Inventory.AllItems)
				{
					item7.SetTransform(this.dummyCharacter.SimPosition, 0f, true, true, null);
					item7.UpdateTransform();
					if (item7.body != null)
					{
						item7.SetTransform(item7.body.SimPosition, 0f, true, true, null);
					}
					Wire wire4 = item7.GetComponent<Wire>();
					if (wire4 != null)
					{
						wire4.Update((float)deltaTime, this.cam);
					}
				}
				if (this.dummyCharacter.SelectedItem != null)
				{
					if (MapEntity.SelectedList.Contains(this.dummyCharacter.SelectedItem) || this.WiringMode)
					{
						Item selectedItem2 = this.dummyCharacter.SelectedItem;
						if (selectedItem2 != null)
						{
							selectedItem2.UpdateHUD(this.cam, this.dummyCharacter, (float)deltaTime);
						}
					}
					else
					{
						this.CloseItem();
					}
				}
				else if (MapEntity.SelectedList.Count == 1 && this.WiringMode)
				{
					Item item8 = MapEntity.SelectedList.FirstOrDefault<MapEntity>() as Item;
					if (item8 != null)
					{
						item8.UpdateHUD(this.cam, this.dummyCharacter, (float)deltaTime);
					}
				}
				CharacterHUD.Update((float)deltaTime, this.dummyCharacter, this.cam);
			}
			this.TransformWidget.Update((float)deltaTime);
		}

		// Token: 0x06002774 RID: 10100 RVA: 0x001B4870 File Offset: 0x001B2A70
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			this.cam.UpdateTransform(true, true);
			if (this.lightingEnabled)
			{
				GameMain.LightManager.RenderLightMap(graphics, spriteBatch, this.cam, null);
			}
			foreach (Submarine sub in Submarine.Loaded)
			{
				sub.UpdateTransform(true);
			}
			graphics.Clear(EditorScreen.BackgroundColor);
			SubEditorScreen.ImageManager.Draw(spriteBatch, this.cam);
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.cam.Transform));
			if (GameMain.DebugDraw)
			{
				GUI.DrawLine(spriteBatch, new Vector2(SubEditorScreen.MainSub.HiddenSubPosition.X, (float)(-(float)this.cam.WorldView.Y)), new Vector2(SubEditorScreen.MainSub.HiddenSubPosition.X, (float)(-(float)(this.cam.WorldView.Y - this.cam.WorldView.Height))), Color.White * 0.5f, 1f, (float)((int)(2f / this.cam.Zoom)));
				GUI.DrawLine(spriteBatch, new Vector2((float)this.cam.WorldView.X, -SubEditorScreen.MainSub.HiddenSubPosition.Y), new Vector2((float)this.cam.WorldView.Right, -SubEditorScreen.MainSub.HiddenSubPosition.Y), Color.White * 0.5f, 1f, (float)((int)(2f / this.cam.Zoom)));
			}
			Submarine.DrawBack(spriteBatch, true, delegate(MapEntity e)
			{
				Structure s = e as Structure;
				if (s != null)
				{
					MapEntityPrefab prefab = e.Prefab;
					if (!this.IsSubcategoryHidden((prefab != null) ? prefab.Subcategory : null))
					{
						return e.SpriteDepth >= 0.9f || s.Prefab.BackgroundSprite != null;
					}
				}
				return false;
			});
			Submarine.DrawPaintedColors(spriteBatch, true, null);
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.cam.Transform));
			Item openedItem = this.OpenedItem;
			if (((openedItem != null) ? openedItem.GetComponent<Wearable>() : null) != null)
			{
				Sprite sprite = this.OpenedItem.Sprite;
				Vector2 pos = new Vector2(this.OpenedItem.DrawPosition.X, -this.OpenedItem.DrawPosition.Y);
				float scale = this.OpenedItem.Scale;
				sprite.Draw(spriteBatch, pos, this.OpenedItem.SpriteColor, 0f, scale, SpriteEffects.None, new float?(this.OpenedItem.SpriteDepth));
				GUI.DrawRectangle(spriteBatch, new Vector2((float)this.OpenedItem.WorldRect.X, (float)(-(float)this.OpenedItem.WorldRect.Y)), new Vector2((float)this.OpenedItem.Rect.Width, (float)this.OpenedItem.Rect.Height), Color.White, false, 0f, (float)((int)Math.Max(2f / this.cam.Zoom, 1f)));
			}
			Submarine.DrawBack(spriteBatch, true, delegate(MapEntity e)
			{
				if (!(e is Structure) || e.SpriteDepth < 0.9f)
				{
					MapEntityPrefab prefab = e.Prefab;
					return !this.IsSubcategoryHidden((prefab != null) ? prefab.Subcategory : null);
				}
				return false;
			});
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.cam.Transform));
			Submarine.DrawDamageable(spriteBatch, null, true, delegate(MapEntity e)
			{
				MapEntityPrefab prefab = e.Prefab;
				return !this.IsSubcategoryHidden((prefab != null) ? prefab.Subcategory : null);
			});
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.cam.Transform));
			Submarine.DrawFront(spriteBatch, true, delegate(MapEntity e)
			{
				MapEntityPrefab prefab = e.Prefab;
				return !this.IsSubcategoryHidden((prefab != null) ? prefab.Subcategory : null);
			});
			if (!this.WiringMode)
			{
				MapEntityPrefab selected = MapEntityPrefab.Selected;
				if (selected != null)
				{
					selected.DrawPlacing(spriteBatch, this.cam);
				}
				MapEntity.DrawSelecting(spriteBatch, this.cam);
			}
			if (this.dummyCharacter != null && this.WiringMode)
			{
				foreach (Item heldItem in this.dummyCharacter.HeldItems)
				{
					heldItem.Draw(spriteBatch, false, true, null, null);
				}
			}
			this.DrawGrid(spriteBatch);
			spriteBatch.End();
			SubEditorScreen.ImageManager.DrawEditing(spriteBatch, this.cam);
			if (GameMain.LightManager.LightingEnabled && this.lightingEnabled)
			{
				spriteBatch.Begin(SpriteSortMode.Deferred, CustomBlendStates.Multiplicative, null, DepthStencilState.None, null, null, null);
				spriteBatch.Draw(GameMain.LightManager.LightMap, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.White);
				spriteBatch.End();
			}
			if (GameMain.LightManager.DebugLos)
			{
				GameMain.LightManager.DebugDrawLos(spriteBatch, this.cam);
			}
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, null, null, null);
			if (SubEditorScreen.MainSub != null && this.cam.Zoom < 5f)
			{
				Vector2 position = (SubEditorScreen.MainSub.SubBody != null) ? SubEditorScreen.MainSub.WorldPosition : SubEditorScreen.MainSub.HiddenSubPosition;
				GUI.DrawIndicator(spriteBatch, position, this.cam, (float)this.cam.WorldView.Width, GUIStyle.SubmarineLocationIcon.Value.Sprite, Color.LightBlue * 0.5f, true, 1f, null);
			}
			GUIComponentStyle notificationIcon = GUIStyle.GetComponentStyle("GUINotificationButton");
			GUIComponentStyle tooltipStyle = GUIStyle.GetComponentStyle("GUIToolTip");
			foreach (Gap gap in Gap.GapList)
			{
				if (gap.linkedTo.Count == 2 && gap.linkedTo[0] == gap.linkedTo[1])
				{
					Vector2 screenPos = this.Cam.WorldToScreen(gap.WorldPosition);
					Rectangle rect = new Rectangle(screenPos.ToPoint() - new Point(20), new Point(40));
					tooltipStyle.Sprites[GUIComponent.ComponentState.None][0].Draw(spriteBatch, rect, Color.White, SpriteEffects.None, null);
					notificationIcon.Sprites[GUIComponent.ComponentState.None][0].Draw(spriteBatch, rect, GUIStyle.Orange, SpriteEffects.None, null);
					if (Vector2.Distance(PlayerInput.MousePosition, screenPos) < 30f * this.Cam.Zoom)
					{
						GUIComponent.DrawToolTip(spriteBatch, TextManager.Get("gapinsidehullwarning"), new Rectangle(screenPos.ToPoint(), new Point(10)), Anchor.BottomCenter, Pivot.TopLeft);
					}
				}
			}
			if (this.DrawCharacterInventory)
			{
				this.dummyCharacter.DrawHUD(spriteBatch, this.cam, false);
				this.wiringToolPanel.DrawManually(spriteBatch, false, true);
			}
			MapEntity.DrawEditor(spriteBatch, this.cam);
			if (this.TransformWidget.Enabled)
			{
				this.TransformWidget.Draw(spriteBatch, (float)deltaTime);
			}
			GUI.Draw(this.Cam, spriteBatch);
			if (this.MeasurePositionStart != Vector2.Zero)
			{
				Vector2 startPos = this.MeasurePositionStart;
				Vector2 mouseWorldPos = this.cam.ScreenToWorld(PlayerInput.MousePosition);
				if (PlayerInput.IsShiftDown())
				{
					startPos = SubEditorScreen.<Draw>g__RoundToGrid|202_4(startPos);
					mouseWorldPos = SubEditorScreen.<Draw>g__RoundToGrid|202_4(mouseWorldPos);
				}
				GUI.DrawLine(spriteBatch, this.cam.WorldToScreen(startPos), this.cam.WorldToScreen(mouseWorldPos), GUIStyle.Green, 0f, 4f);
				decimal realWorldDistance = decimal.Round((decimal)(Vector2.Distance(startPos, mouseWorldPos) * Physics.DisplayToRealWorldRatio), 2);
				Vector2 offset = new Vector2((float)GUI.IntScale(24f));
				Vector2 pos2 = PlayerInput.MousePosition + offset;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
				defaultInterpolatedStringHandler.AppendFormatted<decimal>(realWorldDistance);
				defaultInterpolatedStringHandler.AppendLiteral(" m");
				string text = defaultInterpolatedStringHandler.ToStringAndClear();
				Color color = GUIStyle.TextColorNormal;
				GUIFont font = GUIStyle.Font;
				GUI.DrawString(spriteBatch, pos2, text, color, new Color?(Color.Black), 4, font, ForceUpperCase.Inherit);
			}
			spriteBatch.End();
		}

		// Token: 0x06002775 RID: 10101 RVA: 0x001B50B8 File Offset: 0x001B32B8
		private void CreateImage(int width, int height, Stream stream)
		{
			MapEntity.SelectedList.Clear();
			MapEntity.ClearHighlightedEntities();
			Rectangle prevScissorRect = GameMain.Instance.GraphicsDevice.ScissorRectangle;
			Rectangle subDimensions = Submarine.MainSub.CalculateDimensions(false);
			Vector2 viewPos = subDimensions.Center.ToVector2();
			float scale = Math.Min((float)width / (float)subDimensions.Width, (float)height / (float)subDimensions.Height);
			Matrix viewMatrix = Matrix.CreateTranslation(new Vector3((float)width / 2f, (float)height / 2f, 0f));
			Matrix transform = Matrix.CreateTranslation(new Vector3(-viewPos.X, viewPos.Y, 0f)) * Matrix.CreateScale(new Vector3(scale, scale, 1f)) * viewMatrix;
			using (RenderTarget2D rt = new RenderTarget2D(GameMain.Instance.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None))
			{
				using (SpriteBatch spriteBatch = new SpriteBatch(GameMain.Instance.GraphicsDevice))
				{
					GameMain.Instance.GraphicsDevice.SetRenderTarget(rt);
					GameMain.Instance.GraphicsDevice.Clear(new Color(8, 13, 19));
					spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(transform));
					Submarine.Draw(spriteBatch, false);
					Submarine.DrawFront(spriteBatch, false, null);
					Submarine.DrawDamageable(spriteBatch, null, false, null);
					spriteBatch.End();
					GameMain.Instance.GraphicsDevice.SetRenderTarget(null);
					rt.SaveAsPng(stream, width, height);
				}
			}
			GameMain.Instance.ResetViewPort();
		}

		// Token: 0x06002776 RID: 10102 RVA: 0x001B525C File Offset: 0x001B345C
		private void DrawGrid(SpriteBatch spriteBatch)
		{
			if (!SubEditorScreen.ShouldDrawGrid)
			{
				return;
			}
			float num;
			float num2;
			Submarine.GridSize.Deconstruct(out num, out num2);
			float gridX = num;
			float gridY = num2;
			SubEditorScreen.DrawGrid(spriteBatch, this.cam, gridX, gridY, true);
		}

		// Token: 0x06002777 RID: 10103 RVA: 0x001B5298 File Offset: 0x001B3498
		public static void DrawGrid(SpriteBatch spriteBatch, Camera cam, float sizeX, float sizeY, bool zoomTreshold)
		{
			SubEditorScreen.<>c__DisplayClass206_0 CS$<>8__locals1;
			CS$<>8__locals1.sizeX = sizeX;
			CS$<>8__locals1.sizeY = sizeY;
			if (zoomTreshold && cam.Zoom < 0.5f)
			{
				return;
			}
			int scale = Math.Max(1, GUI.IntScale(1f));
			float zoom = cam.Zoom / 2f;
			float lineThickness = Math.Max(1f, (float)scale / zoom);
			Color gridColor = SubEditorScreen.gridBaseColor;
			if (zoomTreshold && cam.Zoom < 1f)
			{
				gridColor *= Math.Max(0f, (cam.Zoom - 0.5f) * 2f);
			}
			Rectangle camRect = cam.WorldView;
			for (float x = SubEditorScreen.<DrawGrid>g__snapX|206_0(camRect.X, ref CS$<>8__locals1); x < SubEditorScreen.<DrawGrid>g__snapX|206_0(camRect.X + camRect.Width, ref CS$<>8__locals1) + CS$<>8__locals1.sizeX; x += CS$<>8__locals1.sizeX)
			{
				spriteBatch.DrawLine(new Vector2(x, (float)(-(float)camRect.Y)), new Vector2(x, (float)(-(float)(camRect.Y - camRect.Height))), gridColor, lineThickness);
			}
			for (float y = SubEditorScreen.<DrawGrid>g__snapY|206_1(camRect.Y, ref CS$<>8__locals1); y >= SubEditorScreen.<DrawGrid>g__snapY|206_1(camRect.Y - camRect.Height, ref CS$<>8__locals1) - CS$<>8__locals1.sizeY; y -= CS$<>8__locals1.sizeY)
			{
				spriteBatch.DrawLine(new Vector2((float)camRect.X, -y), new Vector2((float)camRect.Right, -y), gridColor, lineThickness);
			}
		}

		// Token: 0x06002778 RID: 10104 RVA: 0x001B5410 File Offset: 0x001B3610
		public static void DrawOutOfBoundsArea(SpriteBatch spriteBatch, Camera cam, float playableAreaSize, Color color)
		{
			Rectangle camRect = cam.WorldView;
			RectangleF playableArea = new RectangleF(-playableAreaSize / 2f, -playableAreaSize / 2f, playableAreaSize, playableAreaSize);
			RectangleF topRect = new RectangleF((float)camRect.Left, (float)(-(float)camRect.Top), (float)camRect.Width, playableArea.Top + (float)camRect.Top);
			float camRectBottom = (float)(-(float)camRect.Top + camRect.Height);
			RectangleF bottomRect = new RectangleF((float)camRect.Left, playableArea.Bottom, (float)camRect.Width, camRectBottom + playableArea.Bottom);
			RectangleF rightRect = new RectangleF(playableArea.Right, playableArea.Top, (float)camRect.Right - playableArea.Right, playableArea.Height);
			RectangleF leftRect = new RectangleF(playableArea.Left, playableArea.Top, (float)camRect.Left - playableArea.Left, playableArea.Height);
			GUI.DrawFilledRectangle(spriteBatch, topRect, color, 0f);
			GUI.DrawFilledRectangle(spriteBatch, leftRect, color, 0f);
			GUI.DrawFilledRectangle(spriteBatch, rightRect, color, 0f);
			GUI.DrawFilledRectangle(spriteBatch, bottomRect, color, 0f);
		}

		// Token: 0x06002779 RID: 10105 RVA: 0x001B5530 File Offset: 0x001B3730
		public void SaveScreenShot(int width, int height, string filePath)
		{
			Stream stream = File.OpenWrite(filePath, true);
			this.CreateImage(width, height, stream);
			stream.Dispose();
		}

		// Token: 0x0600277A RID: 10106 RVA: 0x001B5554 File Offset: 0x001B3754
		public bool IsSubcategoryHidden(string subcategory)
		{
			return !string.IsNullOrEmpty(subcategory) && this.hiddenSubCategories.ContainsKey(subcategory) && this.hiddenSubCategories[subcategory];
		}

		// Token: 0x0600277B RID: 10107 RVA: 0x001B557A File Offset: 0x001B377A
		public static bool IsSubEditor()
		{
			return Screen.Selected is SubEditorScreen && !Submarine.Unloading;
		}

		// Token: 0x0600277C RID: 10108 RVA: 0x001B5592 File Offset: 0x001B3792
		public static bool IsWiringMode()
		{
			return Screen.Selected == GameMain.SubEditorScreen && GameMain.SubEditorScreen.WiringMode && !Submarine.Unloading;
		}

		// Token: 0x0600277D RID: 10109 RVA: 0x001B55B8 File Offset: 0x001B37B8
		public static bool IsLayerVisible(MapEntity entity)
		{
			if (!SubEditorScreen.IsSubEditor() || string.IsNullOrWhiteSpace(entity.Layer))
			{
				return true;
			}
			SubEditorScreen.LayerData data;
			if (!SubEditorScreen.Layers.TryGetValue(entity.Layer, out data))
			{
				SubEditorScreen.Layers.TryAdd(entity.Layer, new SubEditorScreen.LayerData(!entity.IsLayerHidden, false));
				return true;
			}
			return data.IsVisible;
		}

		// Token: 0x0600277E RID: 10110 RVA: 0x001B5618 File Offset: 0x001B3818
		public static bool IsLayerLinked(MapEntity entity)
		{
			if (!SubEditorScreen.IsSubEditor() || string.IsNullOrWhiteSpace(entity.Layer))
			{
				return false;
			}
			SubEditorScreen.LayerData data;
			if (!SubEditorScreen.Layers.TryGetValue(entity.Layer, out data))
			{
				SubEditorScreen.Layers.TryAdd(entity.Layer, new SubEditorScreen.LayerData(!entity.IsLayerHidden, false));
				return true;
			}
			return data.IsGrouped;
		}

		// Token: 0x0600277F RID: 10111 RVA: 0x001B5678 File Offset: 0x001B3878
		public static ImmutableHashSet<MapEntity> GetEntitiesInSameLayer(MapEntity entity)
		{
			if (string.IsNullOrWhiteSpace(entity.Layer))
			{
				return ImmutableHashSet<MapEntity>.Empty;
			}
			return (from me in MapEntity.MapEntityList
			where me.Layer == entity.Layer
			select me).ToImmutableHashSet<MapEntity>();
		}

		// Token: 0x06002783 RID: 10115 RVA: 0x001B5914 File Offset: 0x001B3B14
		[CompilerGenerated]
		internal static Dictionary<TComponent, TProperties> <get_TransformWidget>g__GetPropertyDict|23_12<TComponent, TProperties>(Func<TComponent, TProperties> propSelector, ref SubEditorScreen.<>c__DisplayClass23_0 A_1) where TComponent : ItemComponent
		{
			Item item = A_1.item;
			if (item == null)
			{
				return null;
			}
			return item.GetComponents<TComponent>().ToDictionary((TComponent comp) => comp, propSelector);
		}

		// Token: 0x06002787 RID: 10119 RVA: 0x001B5D1C File Offset: 0x001B3F1C
		[CompilerGenerated]
		internal static string <SaveSubToFile>g__getExistingFilePath|140_0(ContentPackage package, string fileName)
		{
			Submarine mainSub = Submarine.MainSub;
			if (((mainSub != null) ? mainSub.Info : null) == null)
			{
				return null;
			}
			if (package.Files.Any((ContentFile f) => f.Path == SubEditorScreen.MainSub.Info.FilePath && Path.GetFileName(f.Path.Value) == fileName))
			{
				return SubEditorScreen.MainSub.Info.FilePath;
			}
			return null;
		}

		// Token: 0x06002788 RID: 10120 RVA: 0x001B5D78 File Offset: 0x001B3F78
		[CompilerGenerated]
		internal static string <CreatePropertyColorPicker>g__ColorToHex|175_13(Color color)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
			defaultInterpolatedStringHandler.AppendLiteral("#");
			defaultInterpolatedStringHandler.AppendFormatted<int>((int)color.R << 16 | (int)color.G << 8 | (int)color.B, "X6");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0600278A RID: 10122 RVA: 0x001B5DDC File Offset: 0x001B3FDC
		[CompilerGenerated]
		private GUITextBlock <UpdateUndoHistoryPanel>g__CreateTextBlock|199_1(LocalizedString name, LocalizedString description, int index, Command command)
		{
			RectTransform rectTransform = new RectTransform(new Vector2(1f, 0.05f), this.undoBufferList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			rectTransform.MinSize = new Point(0, 15);
			RichString text = ToolBox.LimitString(name.Value, GUIStyle.SmallFont, this.undoBufferList.Content.Rect.Width);
			GUIFont smallFont = GUIStyle.SmallFont;
			return new GUITextBlock(rectTransform, text, (index == SubEditorScreen.commandIndex) ? new Color?(GUIStyle.Green) : null, smallFont, Alignment.Left, false, "", null)
			{
				UserData = command,
				ToolTip = description
			};
		}

		// Token: 0x0600278C RID: 10124 RVA: 0x001B5F1C File Offset: 0x001B411C
		[CompilerGenerated]
		private void <Update>g__TryDragItemsToItem|201_2(Item item)
		{
			foreach (ItemContainer ic in item.GetComponents<ItemContainer>())
			{
				ItemInventory inventory = ic.Inventory;
				if (((inventory != null) ? inventory.visualSlots : null) != null)
				{
					this.<Update>g__TryDragItemsToInventory|201_3(ic.Inventory);
				}
			}
		}

		// Token: 0x0600278D RID: 10125 RVA: 0x001B5F84 File Offset: 0x001B4184
		[CompilerGenerated]
		private void <Update>g__TryDragItemsToInventory|201_3(Inventory inv)
		{
			if (PlayerInput.IsCtrlDown())
			{
				return;
			}
			bool draggingMouse = SubEditorScreen.MouseDragStart != Vector2.Zero && Vector2.Distance(PlayerInput.MousePosition, SubEditorScreen.MouseDragStart) >= GUI.Scale * 20f;
			if (SubEditorScreen.DraggedItemPrefab != null)
			{
				Inventory.DraggingItems.Clear();
			}
			MapEntityPrefab draggedItemPrefab = SubEditorScreen.DraggedItemPrefab;
			ItemPrefab itemPrefab = draggedItemPrefab as ItemPrefab;
			if (itemPrefab == null)
			{
				ItemAssemblyPrefab assemblyPrefab = draggedItemPrefab as ItemAssemblyPrefab;
				if (assemblyPrefab == null)
				{
					return;
				}
				if (PlayerInput.PrimaryMouseButtonClicked())
				{
					bool spawnedItems = false;
					for (int i = 0; i < inv.visualSlots.Length; i++)
					{
						VisualSlot slot = inv.visualSlots[i];
						Item item = (inv != null) ? inv.GetItemAt(i) : null;
						ItemContainer itemContainer2 = (item != null) ? item.GetComponent<ItemContainer>() : null;
						if (item == null && Inventory.IsMouseOnSlot(slot))
						{
							List<Item> itemInstance = this.LoadItemAssemblyInventorySafe(assemblyPrefab);
							int failedCount = 0;
							for (int j = 0; j < itemInstance.Count; j++)
							{
								Item newItem = itemInstance[j];
								int newSpot = i + j - failedCount;
								while (inv.visualSlots.Length > newSpot && inv.GetItemAt(newSpot) != null)
								{
									newSpot++;
								}
								if (inv.visualSlots.Length > newSpot)
								{
									bool placedItem = inv.TryPutItem(newItem, newSpot, false, true, this.dummyCharacter, true, false, true);
									spawnedItems = (spawnedItems || placedItem);
									if (!placedItem)
									{
										failedCount++;
										if (newItem != null)
										{
											ItemInventory ownInventory = newItem.OwnInventory;
											if (ownInventory != null)
											{
												ownInventory.DeleteAllItems();
											}
										}
										newItem.Remove();
									}
								}
								else
								{
									bool placedItem2 = inv.TryPutItem(newItem, this.dummyCharacter, null, true, false, true);
									spawnedItems = (spawnedItems || placedItem2);
									if (!placedItem2)
									{
										if (newItem != null)
										{
											ItemInventory ownInventory2 = newItem.OwnInventory;
											if (ownInventory2 != null)
											{
												ownInventory2.DeleteAllItems();
											}
										}
										newItem.Remove();
									}
								}
							}
							List<MapEntity> placedEntities = (from it in itemInstance
							where !it.Removed
							select it).Cast<MapEntity>().ToList<MapEntity>();
							if (placedEntities.Any<MapEntity>())
							{
								SubEditorScreen.BulkItemBufferInUse = SubEditorScreen.ItemAddMutex;
								SubEditorScreen.BulkItemBuffer.Add(new AddOrDeleteCommand(placedEntities, false, true));
							}
						}
					}
					SoundPlayer.PlayUISound(spawnedItems ? GUISoundType.PickItem : GUISoundType.PickItemFail);
				}
			}
			else if (PlayerInput.PrimaryMouseButtonClicked() || draggingMouse)
			{
				bool spawnedItem = false;
				for (int k = 0; k < inv.Capacity; k++)
				{
					VisualSlot slot2 = inv.visualSlots[k];
					Item itemAt = inv.GetItemAt(k);
					ItemContainer itemContainer = (itemAt != null) ? itemAt.GetComponent<ItemContainer>() : null;
					if (Inventory.IsMouseOnSlot(slot2))
					{
						Item newItem2 = new Item(itemPrefab, Vector2.Zero, SubEditorScreen.MainSub, 0, true);
						if (inv.CanBePutInSlot(itemPrefab, k, null, null))
						{
							bool placedItem3 = inv.TryPutItem(newItem2, k, false, true, this.dummyCharacter, true, false, true);
							spawnedItem = (spawnedItem || placedItem3);
							if (!placedItem3)
							{
								newItem2.Remove();
							}
						}
						else if (itemContainer != null && itemContainer.Inventory.CanProbablyBePut(itemPrefab, null, null))
						{
							bool placedItem4 = itemContainer.Inventory.TryPutItem(newItem2, this.dummyCharacter, null, true, false, true);
							spawnedItem = (spawnedItem || placedItem4);
							if (!placedItem4)
							{
								newItem2.Remove();
							}
							else
							{
								slot2.ShowBorderHighlight(GUIStyle.Green, 0.1f, 0.4f, 0.5f);
							}
						}
						else
						{
							newItem2.Remove();
							slot2.ShowBorderHighlight(GUIStyle.Red, 0.1f, 0.4f, 0.5f);
						}
						if (!newItem2.Removed)
						{
							SubEditorScreen.BulkItemBufferInUse = SubEditorScreen.ItemAddMutex;
							SubEditorScreen.BulkItemBuffer.Add(new AddOrDeleteCommand(new List<MapEntity>
							{
								newItem2
							}, false, true));
						}
						if (!draggingMouse)
						{
							SoundPlayer.PlayUISound(spawnedItem ? GUISoundType.PickItem : GUISoundType.PickItemFail);
						}
					}
				}
				return;
			}
		}

		// Token: 0x06002792 RID: 10130 RVA: 0x001B6410 File Offset: 0x001B4610
		[CompilerGenerated]
		internal static Vector2 <Draw>g__RoundToGrid|202_4(Vector2 position)
		{
			position.X = (float)Math.Round((double)(position.X / Submarine.GridSize.X)) * Submarine.GridSize.X;
			position.Y = (float)Math.Round((double)(position.Y / Submarine.GridSize.Y)) * Submarine.GridSize.Y;
			return position;
		}

		// Token: 0x06002793 RID: 10131 RVA: 0x001B6472 File Offset: 0x001B4672
		[CompilerGenerated]
		internal static float <DrawGrid>g__snapX|206_0(int x, ref SubEditorScreen.<>c__DisplayClass206_0 A_1)
		{
			return (float)Math.Floor((double)((float)x / A_1.sizeX)) * A_1.sizeX;
		}

		// Token: 0x06002794 RID: 10132 RVA: 0x001B648B File Offset: 0x001B468B
		[CompilerGenerated]
		internal static float <DrawGrid>g__snapY|206_1(int y, ref SubEditorScreen.<>c__DisplayClass206_0 A_1)
		{
			return (float)Math.Ceiling((double)((float)y / A_1.sizeY)) * A_1.sizeY;
		}

		// Token: 0x040013CF RID: 5071
		public const int MaxStructures = 2000;

		// Token: 0x040013D0 RID: 5072
		public const int MaxWalls = 500;

		// Token: 0x040013D1 RID: 5073
		public const int MaxItems = 5000;

		// Token: 0x040013D2 RID: 5074
		public const int MaxLights = 600;

		// Token: 0x040013D3 RID: 5075
		public const int MaxShadowCastingLights = 100;

		// Token: 0x040013D4 RID: 5076
		private const float TransformWidgetOffset = 300f;

		// Token: 0x040013D5 RID: 5077
		private const float RotationSnapIncrement = 0.08726647f;

		// Token: 0x040013D6 RID: 5078
		private const float ScaleSnapIncrement = 0.1f;

		// Token: 0x040013D7 RID: 5079
		private GUITickBox rotateToolToggle;

		// Token: 0x040013D8 RID: 5080
		private GUITickBox scaleToolToggle;

		// Token: 0x040013D9 RID: 5081
		private Vector2 oldWidgetWorldPos;

		// Token: 0x040013DA RID: 5082
		private TransformToolCommand transformCommand;

		// Token: 0x040013DB RID: 5083
		private Widget transformWidget;

		// Token: 0x040013DC RID: 5084
		public static Vector2 MouseDragStart = Vector2.Zero;

		// Token: 0x040013DD RID: 5085
		private readonly Point defaultPreviewImageSize = new Point(640, 368);

		// Token: 0x040013DE RID: 5086
		private readonly Camera cam;

		// Token: 0x040013DF RID: 5087
		private Vector2 camTargetFocus = Vector2.Zero;

		// Token: 0x040013E0 RID: 5088
		private SubmarineInfo backedUpSubInfo;

		// Token: 0x040013E1 RID: 5089
		private readonly HashSet<ulong> publishedWorkshopItemIds = new HashSet<ulong>();

		// Token: 0x040013E2 RID: 5090
		private Point screenResolution;

		// Token: 0x040013E3 RID: 5091
		private bool lightingEnabled;

		// Token: 0x040013E4 RID: 5092
		private bool wasSelectedBefore;

		// Token: 0x040013E5 RID: 5093
		public GUIComponent TopPanel;

		// Token: 0x040013E6 RID: 5094
		public GUIComponent showEntitiesPanel;

		// Token: 0x040013E7 RID: 5095
		public GUIComponent entityCountPanel;

		// Token: 0x040013E8 RID: 5096
		private readonly List<GUITickBox> showEntitiesTickBoxes = new List<GUITickBox>();

		// Token: 0x040013E9 RID: 5097
		private readonly Dictionary<string, bool> hiddenSubCategories = new Dictionary<string, bool>();

		// Token: 0x040013EA RID: 5098
		private GUITextBlock subNameLabel;

		// Token: 0x040013EC RID: 5100
		private bool entityMenuOpen = true;

		// Token: 0x040013ED RID: 5101
		private float entityMenuOpenState = 1f;

		// Token: 0x040013EE RID: 5102
		private string lastFilter;

		// Token: 0x040013EF RID: 5103
		public GUIComponent EntityMenu;

		// Token: 0x040013F0 RID: 5104
		private GUITextBox entityFilterBox;

		// Token: 0x040013F1 RID: 5105
		private GUIListBox categorizedEntityList;

		// Token: 0x040013F2 RID: 5106
		private GUIListBox allEntityList;

		// Token: 0x040013F3 RID: 5107
		private GUIButton toggleEntityMenuButton;

		// Token: 0x040013F4 RID: 5108
		private GUITickBox defaultModeTickBox;

		// Token: 0x040013F5 RID: 5109
		private GUITickBox wiringModeTickBox;

		// Token: 0x040013F6 RID: 5110
		private GUIComponent loadFrame;

		// Token: 0x040013F7 RID: 5111
		private GUIComponent saveFrame;

		// Token: 0x040013F8 RID: 5112
		private GUITextBox nameBox;

		// Token: 0x040013F9 RID: 5113
		private GUITextBox descriptionBox;

		// Token: 0x040013FA RID: 5114
		private GUIButton selectedCategoryButton;

		// Token: 0x040013FB RID: 5115
		private GUITextBlock selectedCategoryText;

		// Token: 0x040013FC RID: 5116
		private readonly List<GUIButton> entityCategoryButtons = new List<GUIButton>();

		// Token: 0x040013FD RID: 5117
		private MapEntityCategory? selectedCategory;

		// Token: 0x040013FE RID: 5118
		private GUIFrame hullVolumeFrame;

		// Token: 0x040013FF RID: 5119
		private GUIFrame saveAssemblyFrame;

		// Token: 0x04001400 RID: 5120
		private GUIFrame snapToGridFrame;

		// Token: 0x04001401 RID: 5121
		private const int PreviouslyUsedCount = 10;

		// Token: 0x04001402 RID: 5122
		private GUIFrame previouslyUsedPanel;

		// Token: 0x04001403 RID: 5123
		private GUIListBox previouslyUsedList;

		// Token: 0x04001404 RID: 5124
		private GUIButton visibilityButton;

		// Token: 0x04001405 RID: 5125
		private GUIFrame layerPanel;

		// Token: 0x04001406 RID: 5126
		private GUIListBox layerList;

		// Token: 0x04001407 RID: 5127
		private List<GUIButton> layerSpecificButtons = new List<GUIButton>();

		// Token: 0x04001408 RID: 5128
		private GUIFrame undoBufferPanel;

		// Token: 0x04001409 RID: 5129
		private GUIFrame undoBufferDisclaimer;

		// Token: 0x0400140A RID: 5130
		private GUIListBox undoBufferList;

		// Token: 0x0400140B RID: 5131
		private GUIDropDown linkedSubBox;

		// Token: 0x0400140C RID: 5132
		private static GUIComponent autoSaveLabel;

		// Token: 0x0400140D RID: 5133
		public static readonly object ItemAddMutex = new object();

		// Token: 0x0400140E RID: 5134
		public static readonly object ItemRemoveMutex = new object();

		// Token: 0x0400140F RID: 5135
		public static bool TransparentWiringMode = true;

		// Token: 0x04001410 RID: 5136
		public static bool SkipInventorySlotUpdate;

		// Token: 0x04001411 RID: 5137
		private static object bulkItemBufferinUse;

		// Token: 0x04001412 RID: 5138
		public static List<AddOrDeleteCommand> BulkItemBuffer = new List<AddOrDeleteCommand>();

		// Token: 0x04001413 RID: 5139
		public static List<SubEditorScreen.WarningType> SuppressedWarnings = new List<SubEditorScreen.WarningType>();

		// Token: 0x04001414 RID: 5140
		public static readonly EditorImageManager ImageManager = new EditorImageManager();

		// Token: 0x04001415 RID: 5141
		public static bool ShouldDrawGrid = false;

		// Token: 0x04001416 RID: 5142
		private Character dummyCharacter;

		// Token: 0x04001417 RID: 5143
		public static MapEntityPrefab DraggedItemPrefab;

		// Token: 0x04001418 RID: 5144
		private Item OpenedItem;

		// Token: 0x04001419 RID: 5145
		private Vector2 oldItemPosition;

		// Token: 0x0400141A RID: 5146
		public static readonly List<Command> Commands = new List<Command>();

		// Token: 0x0400141B RID: 5147
		private static int commandIndex;

		// Token: 0x0400141C RID: 5148
		private GUIFrame wiringToolPanel;

		// Token: 0x0400141D RID: 5149
		private Option<DateTime> editorSelectedTime;

		// Token: 0x0400141E RID: 5150
		private GUIImage previewImage;

		// Token: 0x0400141F RID: 5151
		private GUILayoutGroup previewImageButtonHolder;

		// Token: 0x04001420 RID: 5152
		private GUITextBlock submarineNameCharacterCount;

		// Token: 0x04001421 RID: 5153
		private GUITextBlock submarineDescriptionCharacterCount;

		// Token: 0x04001422 RID: 5154
		private SubEditorScreen.Mode mode;

		// Token: 0x04001423 RID: 5155
		private Vector2 MeasurePositionStart = Vector2.Zero;

		// Token: 0x04001424 RID: 5156
		private bool lockMode;

		// Token: 0x04001425 RID: 5157
		private static bool isAutoSaving;

		// Token: 0x04001426 RID: 5158
		private KeyOrMouse toggleEntityListBind;

		// Token: 0x04001427 RID: 5159
		public static XDocument AutoSaveInfo;

		// Token: 0x04001428 RID: 5160
		private static readonly string autoSavePath = Path.Combine(new string[]
		{
			"Submarines",
			".AutoSaves"
		});

		// Token: 0x04001429 RID: 5161
		private static readonly string autoSaveInfoPath = Path.Combine(new string[]
		{
			SubEditorScreen.autoSavePath,
			"autosaves.xml"
		});

		// Token: 0x0400142A RID: 5162
		private static readonly Dictionary<string, SubEditorScreen.LayerData> Layers = new Dictionary<string, SubEditorScreen.LayerData>();

		// Token: 0x0400142B RID: 5163
		private string prevSelectedLayer;

		// Token: 0x0400142C RID: 5164
		private static readonly Color gridBaseColor = Color.White * 0.1f;

		// Token: 0x02000CD6 RID: 3286
		private readonly struct LayerData : IEquatable<SubEditorScreen.LayerData>
		{
			// Token: 0x06007E31 RID: 32305 RVA: 0x0038CA2C File Offset: 0x0038AC2C
			public LayerData(bool IsVisible = true, bool IsGrouped = false)
			{
				this.IsVisible = IsVisible;
				this.IsGrouped = IsGrouped;
			}

			// Token: 0x17001AD2 RID: 6866
			// (get) Token: 0x06007E32 RID: 32306 RVA: 0x0038CA3C File Offset: 0x0038AC3C
			// (set) Token: 0x06007E33 RID: 32307 RVA: 0x0038CA44 File Offset: 0x0038AC44
			public bool IsVisible { get; set; }

			// Token: 0x17001AD3 RID: 6867
			// (get) Token: 0x06007E34 RID: 32308 RVA: 0x0038CA4D File Offset: 0x0038AC4D
			// (set) Token: 0x06007E35 RID: 32309 RVA: 0x0038CA55 File Offset: 0x0038AC55
			public bool IsGrouped { get; set; }

			// Token: 0x06007E36 RID: 32310 RVA: 0x0038CA60 File Offset: 0x0038AC60
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("LayerData");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06007E37 RID: 32311 RVA: 0x0038CAAC File Offset: 0x0038ACAC
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("IsVisible = ");
				builder.Append(this.IsVisible.ToString());
				builder.Append(", IsGrouped = ");
				builder.Append(this.IsGrouped.ToString());
				return true;
			}

			// Token: 0x06007E38 RID: 32312 RVA: 0x0038CB08 File Offset: 0x0038AD08
			[CompilerGenerated]
			public static bool operator !=(SubEditorScreen.LayerData left, SubEditorScreen.LayerData right)
			{
				return !(left == right);
			}

			// Token: 0x06007E39 RID: 32313 RVA: 0x0038CB14 File Offset: 0x0038AD14
			[CompilerGenerated]
			public static bool operator ==(SubEditorScreen.LayerData left, SubEditorScreen.LayerData right)
			{
				return left.Equals(right);
			}

			// Token: 0x06007E3A RID: 32314 RVA: 0x0038CB1E File Offset: 0x0038AD1E
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<bool>.Default.GetHashCode(this.<IsVisible>k__BackingField) * -1521134295 + EqualityComparer<bool>.Default.GetHashCode(this.<IsGrouped>k__BackingField);
			}

			// Token: 0x06007E3B RID: 32315 RVA: 0x0038CB47 File Offset: 0x0038AD47
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is SubEditorScreen.LayerData && this.Equals((SubEditorScreen.LayerData)obj);
			}

			// Token: 0x06007E3C RID: 32316 RVA: 0x0038CB5F File Offset: 0x0038AD5F
			[CompilerGenerated]
			public bool Equals(SubEditorScreen.LayerData other)
			{
				return EqualityComparer<bool>.Default.Equals(this.<IsVisible>k__BackingField, other.<IsVisible>k__BackingField) && EqualityComparer<bool>.Default.Equals(this.<IsGrouped>k__BackingField, other.<IsGrouped>k__BackingField);
			}

			// Token: 0x06007E3D RID: 32317 RVA: 0x0038CB91 File Offset: 0x0038AD91
			[CompilerGenerated]
			public void Deconstruct(out bool IsVisible, out bool IsGrouped)
			{
				IsVisible = this.IsVisible;
				IsGrouped = this.IsGrouped;
			}
		}

		// Token: 0x02000CD7 RID: 3287
		public enum Mode
		{
			// Token: 0x04004CB8 RID: 19640
			Default,
			// Token: 0x04004CB9 RID: 19641
			Wiring
		}

		// Token: 0x02000CD8 RID: 3288
		public struct TransformData : IEquatable<SubEditorScreen.TransformData>
		{
			// Token: 0x06007E3E RID: 32318 RVA: 0x0038CBA4 File Offset: 0x0038ADA4
			public TransformData(float Scale, float RotationRad, Vector2 Pos, Rectangle Rect, Vector2? TexOffset, [TupleElementNames(new string[]
			{
				"Nodes",
				"Width"
			})] Dictionary<Wire, ValueTuple<List<Vector2>, float>> Wires, Dictionary<ItemLabel, float> TextScales, Dictionary<LightComponent, float> LightRanges, Dictionary<Turret, Vector2> TurretLimits)
			{
				this.Scale = Scale;
				this.RotationRad = RotationRad;
				this.Pos = Pos;
				this.Rect = Rect;
				this.TexOffset = TexOffset;
				this.Wires = Wires;
				this.TextScales = TextScales;
				this.LightRanges = LightRanges;
				this.TurretLimits = TurretLimits;
			}

			// Token: 0x17001AD4 RID: 6868
			// (get) Token: 0x06007E3F RID: 32319 RVA: 0x0038CBF6 File Offset: 0x0038ADF6
			// (set) Token: 0x06007E40 RID: 32320 RVA: 0x0038CBFE File Offset: 0x0038ADFE
			public float Scale { readonly get; set; }

			// Token: 0x17001AD5 RID: 6869
			// (get) Token: 0x06007E41 RID: 32321 RVA: 0x0038CC07 File Offset: 0x0038AE07
			// (set) Token: 0x06007E42 RID: 32322 RVA: 0x0038CC0F File Offset: 0x0038AE0F
			public float RotationRad { readonly get; set; }

			// Token: 0x17001AD6 RID: 6870
			// (get) Token: 0x06007E43 RID: 32323 RVA: 0x0038CC18 File Offset: 0x0038AE18
			// (set) Token: 0x06007E44 RID: 32324 RVA: 0x0038CC20 File Offset: 0x0038AE20
			public Vector2 Pos { readonly get; set; }

			// Token: 0x17001AD7 RID: 6871
			// (get) Token: 0x06007E45 RID: 32325 RVA: 0x0038CC29 File Offset: 0x0038AE29
			// (set) Token: 0x06007E46 RID: 32326 RVA: 0x0038CC31 File Offset: 0x0038AE31
			public Rectangle Rect { readonly get; set; }

			// Token: 0x17001AD8 RID: 6872
			// (get) Token: 0x06007E47 RID: 32327 RVA: 0x0038CC3A File Offset: 0x0038AE3A
			// (set) Token: 0x06007E48 RID: 32328 RVA: 0x0038CC42 File Offset: 0x0038AE42
			public Vector2? TexOffset { readonly get; set; }

			// Token: 0x17001AD9 RID: 6873
			// (get) Token: 0x06007E49 RID: 32329 RVA: 0x0038CC4B File Offset: 0x0038AE4B
			// (set) Token: 0x06007E4A RID: 32330 RVA: 0x0038CC53 File Offset: 0x0038AE53
			[TupleElementNames(new string[]
			{
				"Nodes",
				"Width"
			})]
			public Dictionary<Wire, ValueTuple<List<Vector2>, float>> Wires { [return: TupleElementNames(new string[]
			{
				"Nodes",
				"Width"
			})] readonly get; [param: TupleElementNames(new string[]
			{
				"Nodes",
				"Width"
			})] set; }

			// Token: 0x17001ADA RID: 6874
			// (get) Token: 0x06007E4B RID: 32331 RVA: 0x0038CC5C File Offset: 0x0038AE5C
			// (set) Token: 0x06007E4C RID: 32332 RVA: 0x0038CC64 File Offset: 0x0038AE64
			public Dictionary<ItemLabel, float> TextScales { readonly get; set; }

			// Token: 0x17001ADB RID: 6875
			// (get) Token: 0x06007E4D RID: 32333 RVA: 0x0038CC6D File Offset: 0x0038AE6D
			// (set) Token: 0x06007E4E RID: 32334 RVA: 0x0038CC75 File Offset: 0x0038AE75
			public Dictionary<LightComponent, float> LightRanges { readonly get; set; }

			// Token: 0x17001ADC RID: 6876
			// (get) Token: 0x06007E4F RID: 32335 RVA: 0x0038CC7E File Offset: 0x0038AE7E
			// (set) Token: 0x06007E50 RID: 32336 RVA: 0x0038CC86 File Offset: 0x0038AE86
			public Dictionary<Turret, Vector2> TurretLimits { readonly get; set; }

			// Token: 0x06007E51 RID: 32337 RVA: 0x0038CC90 File Offset: 0x0038AE90
			[CompilerGenerated]
			public override readonly string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("TransformData");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06007E52 RID: 32338 RVA: 0x0038CCDC File Offset: 0x0038AEDC
			[CompilerGenerated]
			private readonly bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Scale = ");
				builder.Append(this.Scale.ToString());
				builder.Append(", RotationRad = ");
				builder.Append(this.RotationRad.ToString());
				builder.Append(", Pos = ");
				builder.Append(this.Pos.ToString());
				builder.Append(", Rect = ");
				builder.Append(this.Rect.ToString());
				builder.Append(", TexOffset = ");
				builder.Append(this.TexOffset.ToString());
				builder.Append(", Wires = ");
				builder.Append(this.Wires);
				builder.Append(", TextScales = ");
				builder.Append(this.TextScales);
				builder.Append(", LightRanges = ");
				builder.Append(this.LightRanges);
				builder.Append(", TurretLimits = ");
				builder.Append(this.TurretLimits);
				return true;
			}

			// Token: 0x06007E53 RID: 32339 RVA: 0x0038CE11 File Offset: 0x0038B011
			[CompilerGenerated]
			public static bool operator !=(SubEditorScreen.TransformData left, SubEditorScreen.TransformData right)
			{
				return !(left == right);
			}

			// Token: 0x06007E54 RID: 32340 RVA: 0x0038CE1D File Offset: 0x0038B01D
			[CompilerGenerated]
			public static bool operator ==(SubEditorScreen.TransformData left, SubEditorScreen.TransformData right)
			{
				return left.Equals(right);
			}

			// Token: 0x06007E55 RID: 32341 RVA: 0x0038CE28 File Offset: 0x0038B028
			[CompilerGenerated]
			public override readonly int GetHashCode()
			{
				return (((((((EqualityComparer<float>.Default.GetHashCode(this.<Scale>k__BackingField) * -1521134295 + EqualityComparer<float>.Default.GetHashCode(this.<RotationRad>k__BackingField)) * -1521134295 + EqualityComparer<Vector2>.Default.GetHashCode(this.<Pos>k__BackingField)) * -1521134295 + EqualityComparer<Rectangle>.Default.GetHashCode(this.<Rect>k__BackingField)) * -1521134295 + EqualityComparer<Vector2?>.Default.GetHashCode(this.<TexOffset>k__BackingField)) * -1521134295 + EqualityComparer<Dictionary<Wire, ValueTuple<List<Vector2>, float>>>.Default.GetHashCode(this.<Wires>k__BackingField)) * -1521134295 + EqualityComparer<Dictionary<ItemLabel, float>>.Default.GetHashCode(this.<TextScales>k__BackingField)) * -1521134295 + EqualityComparer<Dictionary<LightComponent, float>>.Default.GetHashCode(this.<LightRanges>k__BackingField)) * -1521134295 + EqualityComparer<Dictionary<Turret, Vector2>>.Default.GetHashCode(this.<TurretLimits>k__BackingField);
			}

			// Token: 0x06007E56 RID: 32342 RVA: 0x0038CEFD File Offset: 0x0038B0FD
			[CompilerGenerated]
			public override readonly bool Equals(object obj)
			{
				return obj is SubEditorScreen.TransformData && this.Equals((SubEditorScreen.TransformData)obj);
			}

			// Token: 0x06007E57 RID: 32343 RVA: 0x0038CF18 File Offset: 0x0038B118
			[CompilerGenerated]
			public readonly bool Equals(SubEditorScreen.TransformData other)
			{
				return EqualityComparer<float>.Default.Equals(this.<Scale>k__BackingField, other.<Scale>k__BackingField) && EqualityComparer<float>.Default.Equals(this.<RotationRad>k__BackingField, other.<RotationRad>k__BackingField) && EqualityComparer<Vector2>.Default.Equals(this.<Pos>k__BackingField, other.<Pos>k__BackingField) && EqualityComparer<Rectangle>.Default.Equals(this.<Rect>k__BackingField, other.<Rect>k__BackingField) && EqualityComparer<Vector2?>.Default.Equals(this.<TexOffset>k__BackingField, other.<TexOffset>k__BackingField) && EqualityComparer<Dictionary<Wire, ValueTuple<List<Vector2>, float>>>.Default.Equals(this.<Wires>k__BackingField, other.<Wires>k__BackingField) && EqualityComparer<Dictionary<ItemLabel, float>>.Default.Equals(this.<TextScales>k__BackingField, other.<TextScales>k__BackingField) && EqualityComparer<Dictionary<LightComponent, float>>.Default.Equals(this.<LightRanges>k__BackingField, other.<LightRanges>k__BackingField) && EqualityComparer<Dictionary<Turret, Vector2>>.Default.Equals(this.<TurretLimits>k__BackingField, other.<TurretLimits>k__BackingField);
			}

			// Token: 0x06007E58 RID: 32344 RVA: 0x0038D008 File Offset: 0x0038B208
			[CompilerGenerated]
			public readonly void Deconstruct(out float Scale, out float RotationRad, out Vector2 Pos, out Rectangle Rect, out Vector2? TexOffset, [TupleElementNames(new string[]
			{
				"Nodes",
				"Width"
			})] out Dictionary<Wire, ValueTuple<List<Vector2>, float>> Wires, out Dictionary<ItemLabel, float> TextScales, out Dictionary<LightComponent, float> LightRanges, out Dictionary<Turret, Vector2> TurretLimits)
			{
				Scale = this.Scale;
				RotationRad = this.RotationRad;
				Pos = this.Pos;
				Rect = this.Rect;
				TexOffset = this.TexOffset;
				Wires = this.Wires;
				TextScales = this.TextScales;
				LightRanges = this.LightRanges;
				TurretLimits = this.TurretLimits;
			}
		}

		// Token: 0x02000CD9 RID: 3289
		public enum WarningType
		{
			// Token: 0x04004CC4 RID: 19652
			NoWaypoints,
			// Token: 0x04004CC5 RID: 19653
			NoHulls,
			// Token: 0x04004CC6 RID: 19654
			DisconnectedVents,
			// Token: 0x04004CC7 RID: 19655
			NoHumanSpawnpoints,
			// Token: 0x04004CC8 RID: 19656
			NoCargoSpawnpoints,
			// Token: 0x04004CC9 RID: 19657
			NoBallastTag,
			// Token: 0x04004CCA RID: 19658
			NonLinkedGaps,
			// Token: 0x04004CCB RID: 19659
			NoHiddenContainers,
			// Token: 0x04004CCC RID: 19660
			InsufficientFreeConnectionsWarning,
			// Token: 0x04004CCD RID: 19661
			StructureCount,
			// Token: 0x04004CCE RID: 19662
			WallCount,
			// Token: 0x04004CCF RID: 19663
			ItemCount,
			// Token: 0x04004CD0 RID: 19664
			LightCount,
			// Token: 0x04004CD1 RID: 19665
			ShadowCastingLightCount,
			// Token: 0x04004CD2 RID: 19666
			WaterInHulls,
			// Token: 0x04004CD3 RID: 19667
			LowOxygenOutputWarning,
			// Token: 0x04004CD4 RID: 19668
			TooLargeForEndGame,
			// Token: 0x04004CD5 RID: 19669
			NotEnoughContainers,
			// Token: 0x04004CD6 RID: 19670
			NoSuitableBrainRooms
		}

		// Token: 0x02000CDA RID: 3290
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004CD7 RID: 19671
			public static GUITextBlock.TextGetterHandler <0>__GetTotalHullVolume;

			// Token: 0x04004CD8 RID: 19672
			public static GUITextBlock.TextGetterHandler <1>__GetSelectedHullVolume;

			// Token: 0x04004CD9 RID: 19673
			public static GUIListBox.CheckSelectedHandler <2>__GetSelected;

			// Token: 0x04004CDA RID: 19674
			public static CrossThread.TaskDelegate <3>__DisplayAutoSavePrompt;

			// Token: 0x04004CDB RID: 19675
			public static Predicate<Keys> <4>__KeyHit;

			// Token: 0x04004CDC RID: 19676
			public static Action<MapEntity> <5>__AddSelection;
		}
	}
}
