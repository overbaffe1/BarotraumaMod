using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005CC RID: 1484
	[NullableContext(1)]
	[Nullable(0)]
	internal class PowerDistributor : PowerTransfer, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06005E29 RID: 24105 RVA: 0x00311860 File Offset: 0x0030FA60
		protected override void CreateGUI()
		{
			if (base.GuiFrame == null)
			{
				return;
			}
			this.guiContent = new GUILayoutGroup(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, false, Anchor.TopLeft)
			{
				Stretch = true
			};
			Vector2 unitX = Vector2.UnitX;
			RectTransform rectTransform = this.guiContent.RectTransform;
			Anchor anchor = Anchor.TopLeft;
			Point? point = new Point?(new ValueTuple<int, int>(0, 125));
			GUIFrame defaultUIContainer = new GUIFrame(new RectTransform(unitX, rectTransform, anchor, null, point, null, ScaleBasis.Normal), null, null)
			{
				CanBeFocused = false
			};
			base.CreateDefaultPowerUI(defaultUIContainer);
			Vector2 one = Vector2.One;
			RectTransform rectTransform2 = this.guiContent.RectTransform;
			Anchor anchor2 = Anchor.TopLeft;
			Pivot? pivot = null;
			point = null;
			Point? minSize = point;
			point = null;
			this.groupList = new GUIListBox(new RectTransform(one, rectTransform2, anchor2, pivot, minSize, point, ScaleBasis.Normal), false, null, "", true, false)
			{
				Enabled = false
			};
			Vector2 relativeSize = new Vector2(0.8f, 0f);
			RectTransform rectTransform3 = this.groupList.Content.RectTransform;
			Anchor anchor3 = Anchor.Center;
			Pivot? pivot2 = null;
			point = null;
			Point? minSize2 = point;
			point = null;
			this.noConnectionsText = new GUITextBlock(new RectTransform(relativeSize, rectTransform3, anchor3, pivot2, minSize2, point, ScaleBasis.Normal), TextManager.Get("powerdistributor.noconnections"), null, null, Alignment.Left, true, "", null)
			{
				Visible = false
			};
			this.powerGroups.ForEach(delegate(PowerDistributor.PowerGroup group)
			{
				group.CreateGUI();
			});
		}

		// Token: 0x06005E2A RID: 24106 RVA: 0x00311A2C File Offset: 0x0030FC2C
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (base.GuiFrame == null)
			{
				return;
			}
			this.powerGroups.ForEach(delegate(PowerDistributor.PowerGroup group)
			{
				group.UpdateGUI();
			});
			this.noConnectionsText.Visible = this.powerGroups.None((PowerDistributor.PowerGroup group) => group.IsVisible);
			base.UpdateHUDComponentSpecific(character, deltaTime, cam);
		}

		// Token: 0x06005E2B RID: 24107 RVA: 0x00311AAA File Offset: 0x0030FCAA
		public void ClientEventWrite(IWriteMessage msg, [Nullable(2)] NetEntityEvent.IData extraData = null)
		{
			this.SharedEventWrite(msg, extraData);
		}

		// Token: 0x06005E2C RID: 24108 RVA: 0x00311AB4 File Offset: 0x0030FCB4
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			int msgStartPos = msg.BitPosition;
			PowerDistributor.EventType eventType;
			PowerDistributor.PowerGroup powerGroup;
			string newName;
			float newRatio;
			this.SharedEventRead(msg, out eventType, out powerGroup, out newName, out newRatio);
			if (this.correctionTimer > 0f)
			{
				int msgBits = msg.BitPosition - msgStartPos;
				msg.BitPosition -= msgBits;
				base.StartDelayedCorrection(msg.ExtractBits(msgBits), sendingTime, false);
				return;
			}
			if (eventType == PowerDistributor.EventType.NameChange)
			{
				powerGroup.Name = newName;
				return;
			}
			if (eventType != PowerDistributor.EventType.RatioChange)
			{
				return;
			}
			powerGroup.SupplyRatio = newRatio;
		}

		// Token: 0x170017B6 RID: 6070
		// (get) Token: 0x06005E2D RID: 24109 RVA: 0x00311B26 File Offset: 0x0030FD26
		protected override PowerPriority Priority
		{
			get
			{
				return PowerPriority.Relay;
			}
		}

		// Token: 0x06005E2E RID: 24110 RVA: 0x00311B29 File Offset: 0x0030FD29
		public PowerDistributor(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x06005E2F RID: 24111 RVA: 0x00311B4C File Offset: 0x0030FD4C
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			IEnumerable<Connection> ratioInputs = from conn in base.Item.Connections
			where !conn.IsOutput && conn.Name.StartsWith("set_supply_ratio")
			select conn;
			IEnumerable<Connection> ratioOutputs = from conn in base.Item.Connections
			where conn.IsOutput && conn.Name.StartsWith("supply_ratio_out")
			select conn;
			for (int i = 0; i < this.powerOuts.Count; i++)
			{
				new PowerDistributor.PowerGroup(this, this.powerOuts[i], this.cachedGroupData.ElementAtOrDefault(i), ratioInputs.ElementAtOrDefault(i), ratioOutputs.ElementAtOrDefault(i));
			}
			this.cachedGroupData.Clear();
		}

		// Token: 0x06005E30 RID: 24112 RVA: 0x00311C10 File Offset: 0x0030FE10
		public override void Clone(ItemComponent original)
		{
			PowerDistributor originalPowerDistributor = original as PowerDistributor;
			if (originalPowerDistributor == null)
			{
				return;
			}
			for (int i = 0; i < this.powerOuts.Count; i++)
			{
				this.powerGroups[i].SupplyRatio = originalPowerDistributor.powerGroups[i].SupplyRatio;
				this.powerGroups[i].Name = originalPowerDistributor.powerGroups[i].Name;
			}
		}

		// Token: 0x06005E31 RID: 24113 RVA: 0x00311C84 File Offset: 0x0030FE84
		protected override void SendSignals()
		{
			Item item = this.item;
			GridInfo grid = this.powerIn.Grid;
			item.SendSignal(MathUtils.RoundToInt((grid != null) ? grid.Power : 0f).ToString(), "power_value_out");
			this.item.SendSignal(MathUtils.RoundToInt(this.GetCurrentPowerConsumption(this.powerIn)).ToString(), "load_value_out");
			this.powerGroups.ForEach(delegate(PowerDistributor.PowerGroup group)
			{
				group.SendRatioSignal();
			});
		}

		// Token: 0x06005E32 RID: 24114 RVA: 0x00311D1C File Offset: 0x0030FF1C
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.item.Condition <= 0f || connection.IsPower)
			{
				return;
			}
			if (connection.IsOutput)
			{
				return;
			}
			PowerDistributor.PowerGroup powerGroup = this.powerGroups.FirstOrDefault((PowerDistributor.PowerGroup group) => group.RatioInput == connection);
			if (powerGroup == null)
			{
				return;
			}
			powerGroup.ReceiveRatioSignal(signal);
		}

		// Token: 0x06005E33 RID: 24115 RVA: 0x00311D86 File Offset: 0x0030FF86
		private bool IsShortCircuited(Connection conn)
		{
			return this.powerIn.Grid == conn.Grid;
		}

		// Token: 0x06005E34 RID: 24116 RVA: 0x00311D9B File Offset: 0x0030FF9B
		[NullableContext(2)]
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn)
			{
				return -1f;
			}
			if (this.isBroken)
			{
				return 0f;
			}
			return this.powerGroups.Sum(delegate(PowerDistributor.PowerGroup group)
			{
				if (!this.IsShortCircuited(group.PowerOut))
				{
					return group.ModifiedLoad;
				}
				return 0f;
			}) + base.ExtraLoad;
		}

		// Token: 0x06005E35 RID: 24117 RVA: 0x00311DD8 File Offset: 0x0030FFD8
		private float CalculatePowerOut(PowerDistributor.PowerGroup group)
		{
			if (this.isBroken || this.powerIn.Grid == null || this.IsShortCircuited(group.PowerOut))
			{
				return 0f;
			}
			return Math.Max(group.ModifiedLoad * base.Voltage, 0f);
		}

		// Token: 0x06005E36 RID: 24118 RVA: 0x00311E28 File Offset: 0x00310028
		public override float GetConnectionPowerOut(Connection connection, float power, PowerRange minMaxPower, float load)
		{
			if (connection == this.powerIn)
			{
				return 0f;
			}
			PowerDistributor.PowerGroup group2 = this.powerGroups.First((PowerDistributor.PowerGroup group) => group.PowerOut == connection);
			group2.Load = load;
			return this.CalculatePowerOut(group2);
		}

		// Token: 0x06005E37 RID: 24119 RVA: 0x00311E80 File Offset: 0x00310080
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			foreach (PowerDistributor.PowerGroup powerGroup in this.powerGroups)
			{
				componentElement.Add(new XElement("PowerGroup", new object[]
				{
					new XAttribute("name", powerGroup.Name),
					new XAttribute("ratio", powerGroup.SupplyRatio)
				}));
			}
			return componentElement;
		}

		// Token: 0x06005E38 RID: 24120 RVA: 0x00311F28 File Offset: 0x00310128
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			if (usePrefabValues)
			{
				return;
			}
			foreach (ContentXElement cxe in componentElement.Elements())
			{
				XElement element = cxe;
				this.cachedGroupData.Add(element);
			}
		}

		// Token: 0x06005E39 RID: 24121 RVA: 0x00311F90 File Offset: 0x00310190
		private void SharedEventWrite(IWriteMessage msg, [Nullable(2)] NetEntityEvent.IData extraData = null)
		{
			PowerDistributor.EventData data = base.ExtractEventData<PowerDistributor.EventData>(extraData);
			msg.WriteRangedInteger((int)data.EventType, 0, 1);
			msg.WriteRangedInteger(this.powerGroups.IndexOf(data.PowerGroup), 0, this.powerGroups.Count - 1);
			PowerDistributor.EventType eventType = data.EventType;
			if (eventType == PowerDistributor.EventType.NameChange)
			{
				msg.WriteString(data.PowerGroup.Name);
				return;
			}
			if (eventType != PowerDistributor.EventType.RatioChange)
			{
				return;
			}
			msg.WriteRangedInteger(MathUtils.RoundToInt(data.PowerGroup.SupplyRatio / 0.05f), 0, 20);
		}

		// Token: 0x06005E3A RID: 24122 RVA: 0x00312020 File Offset: 0x00310220
		private void SharedEventRead(IReadMessage msg, out PowerDistributor.EventType eventType, out PowerDistributor.PowerGroup powerGroup, out string newName, out float newRatio)
		{
			eventType = (PowerDistributor.EventType)msg.ReadRangedInteger(0, 1);
			powerGroup = this.powerGroups[msg.ReadRangedInteger(0, this.powerGroups.Count - 1)];
			newName = ((eventType == PowerDistributor.EventType.NameChange) ? string.Concat<char>(msg.ReadString().Take(32)) : powerGroup.Name);
			newRatio = ((eventType == PowerDistributor.EventType.RatioChange) ? ((float)msg.ReadRangedInteger(0, 20) * 0.05f) : powerGroup.SupplyRatio);
		}

		// Token: 0x040030A5 RID: 12453
		[Nullable(2)]
		private GUIListBox groupList;

		// Token: 0x040030A6 RID: 12454
		[Nullable(2)]
		private GUITextBlock noConnectionsText;

		// Token: 0x040030A7 RID: 12455
		private const int MaxNameLength = 32;

		// Token: 0x040030A8 RID: 12456
		private const int SupplyRatioSteps = 20;

		// Token: 0x040030A9 RID: 12457
		private const float SupplyRatioStep = 0.05f;

		// Token: 0x040030AA RID: 12458
		private readonly List<PowerDistributor.PowerGroup> powerGroups = new List<PowerDistributor.PowerGroup>();

		// Token: 0x040030AB RID: 12459
		private readonly List<XElement> cachedGroupData = new List<XElement>();

		// Token: 0x02001434 RID: 5172
		[Nullable(0)]
		private class PowerGroup
		{
			// Token: 0x17001D45 RID: 7493
			// (get) Token: 0x060099FF RID: 39423 RVA: 0x003E199B File Offset: 0x003DFB9B
			// (set) Token: 0x06009A00 RID: 39424 RVA: 0x003E19A3 File Offset: 0x003DFBA3
			public bool IsVisible { get; private set; } = true;

			// Token: 0x06009A01 RID: 39425 RVA: 0x003E19AC File Offset: 0x003DFBAC
			public void CreateGUI()
			{
				Vector2 relativeSize = new Vector2(1f, 0.25f);
				RectTransform rectTransform = this.distributor.groupList.Content.RectTransform;
				Anchor anchor = Anchor.TopLeft;
				Point? point = new Point?(new ValueTuple<int, int>(0, 130));
				this.frame = new GUIFrame(new RectTransform(relativeSize, rectTransform, anchor, null, point, null, ScaleBasis.Normal), null, null);
				this.groupContent = new GUIFrame(new RectTransform(this.frame.Rect.Size - new Point(10), this.frame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, null);
				Vector2 relativeSize2 = new Vector2(0.65f, 0.33f);
				RectTransform rectTransform2 = this.groupContent.RectTransform;
				Anchor anchor2 = Anchor.TopLeft;
				Pivot? pivot = null;
				point = null;
				Point? minSize = point;
				point = null;
				this.nameGroup = new GUILayoutGroup(new RectTransform(relativeSize2, rectTransform2, anchor2, pivot, minSize, point, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					Stretch = true
				};
				Vector2 relativeSize3 = new Vector2(0.75f);
				RectTransform rectTransform3 = this.nameGroup.RectTransform;
				Anchor anchor3 = Anchor.TopLeft;
				Pivot? pivot2 = null;
				point = null;
				Point? minSize2 = point;
				point = null;
				GUIButton guibutton = new GUIButton(new RectTransform(relativeSize3, rectTransform3, anchor3, pivot2, minSize2, point, ScaleBasis.BothHeight), Alignment.Center, "TextBoxIcon", null);
				guibutton.HoverCursor = CursorState.IBeam;
				guibutton.OnClicked = delegate(GUIButton _, object _)
				{
					this.nameBox.Select(-1, false);
					return true;
				};
				Vector2 one = Vector2.One;
				RectTransform rectTransform4 = this.nameGroup.RectTransform;
				Anchor anchor4 = Anchor.TopLeft;
				Pivot? pivot3 = null;
				point = null;
				Point? minSize3 = point;
				point = null;
				RectTransform rectT = new RectTransform(one, rectTransform4, anchor4, pivot3, minSize3, point, ScaleBasis.Normal);
				string text = (Screen.Selected == GameMain.SubEditorScreen) ? this.Name : this.DisplayName.Value;
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				GUITextBox guitextBox = new GUITextBox(rectT, text, null, subHeadingFont, Alignment.Left, false, "GUITextBoxNoStyle", null, false, true);
				guitextBox.MaxTextLength = new int?(32);
				guitextBox.OverflowClip = true;
				guitextBox.TextBlock.ForceUpperCase = ForceUpperCase.No;
				guitextBox.OnEnterPressed = delegate(GUITextBox textBox, string _)
				{
					textBox.Deselect();
					return true;
				};
				this.nameBox = guitextBox;
				this.nameBox.OnSelected += delegate(GUITextBox tb, Keys _)
				{
					if (tb.Selected)
					{
						return;
					}
					tb.Text = this.Name;
				};
				this.nameBox.OnDeselected += delegate(GUITextBox tb, Keys _)
				{
					this.Name = tb.Text;
					tb.CaretIndex = 0;
					if (GameMain.Client == null)
					{
						return;
					}
					this.distributor.item.CreateClientEvent<PowerDistributor>(this.distributor, new PowerDistributor.EventData(this, PowerDistributor.EventType.NameChange));
				};
				this.nameBox.GetChild<GUIFrame>().GetChild<GUICustomComponent>().OnDrawToolTip = delegate(GUIComponent comp)
				{
					if (Screen.Selected != GameMain.SubEditorScreen && !this.nameBox.Selected)
					{
						comp.ToolTip = null;
						return;
					}
					LocalizedString localizedText = TextManager.Get(this.nameBox.Text);
					comp.ToolTip = (localizedText.IsNullOrEmpty() ? TextManager.GetWithVariable("StringPropertyCannotTranslate", "[tag]", this.nameBox.Text, FormatCapitals.No) : TextManager.GetWithVariable("StringPropertyTranslate", "[translation]", localizedText, FormatCapitals.No));
				};
				Vector2 relativeSize4 = new Vector2(0.35f, 0.33f);
				RectTransform rectTransform5 = this.groupContent.RectTransform;
				Anchor anchor5 = Anchor.TopRight;
				Pivot? pivot4 = null;
				point = null;
				Point? minSize4 = point;
				point = null;
				GUITextBlock loadDisplayUnitLabel;
				GUITextBlock loadDisplay = GUI.CreateDigitalDisplay(new RectTransform(relativeSize4, rectTransform5, anchor5, pivot4, minSize4, point, ScaleBasis.Normal)
				{
					AbsoluteOffset = new ValueTuple<int, int>(5, 0)
				}, out this.loadDisplayNameLabel, out loadDisplayUnitLabel, TextManager.Get("PowerTransferLoadLabel"), null, TextManager.Get("PowerTransferTipLoad"), GUIStyle.Font);
				loadDisplay.TextGetter = (() => MathUtils.RoundToInt(this.Load).ToString());
				float textAndPaddingWidth = this.loadDisplayNameLabel.Font.MeasureString(this.loadDisplayNameLabel.Text, false).X + this.loadDisplayNameLabel.Padding.X + this.loadDisplayNameLabel.Padding.Z;
				float availableWidth = (float)(this.groupContent.Rect.Width - this.loadDisplayNameLabel.Parent.Rect.Width + this.loadDisplayNameLabel.Rect.Width) - textAndPaddingWidth;
				this.nameGroup.RectTransform.Resize(new Point((int)availableWidth, this.nameGroup.Rect.Height), true);
				Vector2 relativeSize5 = new Vector2(1f, 0.33f);
				RectTransform rectTransform6 = this.groupContent.RectTransform;
				Anchor anchor6 = Anchor.Center;
				Pivot? pivot5 = null;
				point = null;
				Point? minSize5 = point;
				point = null;
				this.ratioSlider = new GUIScrollBar(new RectTransform(relativeSize5, rectTransform6, anchor6, pivot5, minSize5, point, ScaleBasis.Normal), 0.15f, null, "DeviceSlider", null)
				{
					Step = 0.05f,
					BarScroll = this.SupplyRatio,
					OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
					{
						if (MathUtils.NearlyEqual(barScroll, this.SupplyRatio, 0.0001f))
						{
							return false;
						}
						this.SupplyRatio = barScroll;
						if (GameMain.Client != null)
						{
							this.distributor.item.CreateClientEvent<PowerDistributor>(this.distributor, new PowerDistributor.EventData(this, PowerDistributor.EventType.RatioChange));
							this.distributor.correctionTimer = 1f;
						}
						return true;
					}
				};
				this.ratioSlider.Bar.RectTransform.MaxSize = new Point(this.ratioSlider.Bar.Rect.Height);
				Vector2 relativeSize6 = new Vector2(0.2f, 0.33f);
				RectTransform rectTransform7 = this.groupContent.RectTransform;
				Anchor anchor7 = Anchor.BottomLeft;
				Pivot? pivot6 = null;
				point = null;
				Point? minSize6 = point;
				point = null;
				GUITextBlock guitextBlock;
				GUITextBlock ratioDisplayUnitLabel;
				GUITextBlock ratioDisplay = GUI.CreateDigitalDisplay(new RectTransform(relativeSize6, rectTransform7, anchor7, pivot6, minSize6, point, ScaleBasis.Normal), out guitextBlock, out ratioDisplayUnitLabel, null, "%", null, null);
				ratioDisplay.TextGetter = (() => this.DisplayRatio.ToString());
				Vector2 relativeSize7 = new Vector2(0.35f, 0.33f);
				RectTransform rectTransform8 = this.groupContent.RectTransform;
				Anchor anchor8 = Anchor.BottomRight;
				Pivot? pivot7 = null;
				point = null;
				Point? minSize7 = point;
				point = null;
				GUITextBlock outputDisplayNameLabel;
				GUITextBlock outputDisplayUnitLabel;
				GUITextBlock outputDisplay = GUI.CreateDigitalDisplay(new RectTransform(relativeSize7, rectTransform8, anchor8, pivot7, minSize7, point, ScaleBasis.Normal)
				{
					AbsoluteOffset = new ValueTuple<int, int>(5, 0)
				}, out outputDisplayNameLabel, out outputDisplayUnitLabel, TextManager.Get("powerdistributor.supplylabel"), null, TextManager.Get("PowerTransferTipPower"), GUIStyle.Font);
				outputDisplay.TextGetter = (() => this.distributor.IsShortCircuited(this.PowerOut) ? "err" : MathUtils.RoundToInt(this.distributor.CalculatePowerOut(this)).ToString());
				this.powerUnitLabels.Add(loadDisplayUnitLabel);
				this.powerUnitLabels.Add(ratioDisplayUnitLabel);
				this.powerUnitLabels.Add(outputDisplayUnitLabel);
				GUITextBlock.AutoScaleAndNormalize(this.powerUnitLabels, true, false, null);
				Vector2 unitX = Vector2.UnitX;
				RectTransform rectTransform9 = this.distributor.groupList.Content.RectTransform;
				Anchor anchor9 = Anchor.TopLeft;
				Pivot? pivot8 = null;
				point = null;
				Point? minSize8 = point;
				point = null;
				this.divider = new GUIFrame(new RectTransform(unitX, rectTransform9, anchor9, pivot8, minSize8, point, ScaleBasis.Normal), "HorizontalLine", null);
			}

			// Token: 0x06009A02 RID: 39426 RVA: 0x003E1FD4 File Offset: 0x003E01D4
			private void UpdateNameBox()
			{
				if (this.nameBox == null || this.nameBox.Text == this.DisplayName)
				{
					return;
				}
				GUITextBox guitextBox = this.nameBox;
				LocalizedString displayName = this.DisplayName;
				guitextBox.Text = (((displayName != null) ? displayName.Value : null) ?? string.Empty);
			}

			// Token: 0x06009A03 RID: 39427 RVA: 0x003E202D File Offset: 0x003E022D
			private void UpdateSlider()
			{
				if (this.ratioSlider == null || MathUtils.NearlyEqual(this.ratioSlider.BarScroll, this.supplyRatio, 0.0001f))
				{
					return;
				}
				this.ratioSlider.BarScroll = this.supplyRatio;
			}

			// Token: 0x06009A04 RID: 39428 RVA: 0x003E2068 File Offset: 0x003E0268
			public void UpdateGUI()
			{
				this.IsVisible = (this.PowerOut.Wires.Count >= 1);
				this.frame.Visible = this.IsVisible;
				GUIComponent guicomponent = this.divider;
				bool visible;
				if (this.IsVisible)
				{
					visible = (this.distributor.powerGroups.Last((PowerDistributor.PowerGroup group) => group.frame.Visible) != this);
				}
				else
				{
					visible = false;
				}
				guicomponent.Visible = visible;
				if (this.distributor.prevLanguage != GameSettings.CurrentConfig.Language)
				{
					GUITextBlock.AutoScaleAndNormalize(this.powerUnitLabels, true, false, null);
					float textAndPaddingWidth = this.loadDisplayNameLabel.Font.MeasureString(this.loadDisplayNameLabel.Text, false).X + this.loadDisplayNameLabel.Padding.X + this.loadDisplayNameLabel.Padding.Z;
					float availableWidth = (float)(this.groupContent.Rect.Width - this.loadDisplayNameLabel.Parent.Rect.Width + this.loadDisplayNameLabel.Rect.Width) - textAndPaddingWidth;
					this.nameGroup.RectTransform.Resize(new Point((int)availableWidth, this.nameGroup.Rect.Height), true);
				}
			}

			// Token: 0x17001D46 RID: 7494
			// (get) Token: 0x06009A05 RID: 39429 RVA: 0x003E21CE File Offset: 0x003E03CE
			// (set) Token: 0x06009A06 RID: 39430 RVA: 0x003E21D6 File Offset: 0x003E03D6
			public string Name
			{
				get
				{
					return this.name;
				}
				set
				{
					this.name = value;
					this.DisplayName = TextManager.Get(this.name).Fallback(this.name, true);
					this.UpdateNameBox();
				}
			}

			// Token: 0x17001D47 RID: 7495
			// (get) Token: 0x06009A07 RID: 39431 RVA: 0x003E2207 File Offset: 0x003E0407
			// (set) Token: 0x06009A08 RID: 39432 RVA: 0x003E220F File Offset: 0x003E040F
			public LocalizedString DisplayName { get; private set; }

			// Token: 0x17001D48 RID: 7496
			// (get) Token: 0x06009A09 RID: 39433 RVA: 0x003E2218 File Offset: 0x003E0418
			// (set) Token: 0x06009A0A RID: 39434 RVA: 0x003E2220 File Offset: 0x003E0420
			public float SupplyRatio
			{
				get
				{
					return this.supplyRatio;
				}
				set
				{
					if (!MathUtils.IsValid(value))
					{
						return;
					}
					this.supplyRatio = MathUtils.RoundTowardsClosest(MathHelper.Clamp(value, 0f, 1f), 0.05f);
					this.UpdateSlider();
				}
			}

			// Token: 0x17001D49 RID: 7497
			// (get) Token: 0x06009A0B RID: 39435 RVA: 0x003E2251 File Offset: 0x003E0451
			// (set) Token: 0x06009A0C RID: 39436 RVA: 0x003E2265 File Offset: 0x003E0465
			public float DisplayRatio
			{
				get
				{
					return (float)MathUtils.RoundToInt(this.supplyRatio * 100f);
				}
				set
				{
					this.SupplyRatio = value / 100f;
				}
			}

			// Token: 0x17001D4A RID: 7498
			// (get) Token: 0x06009A0D RID: 39437 RVA: 0x003E2274 File Offset: 0x003E0474
			public float ModifiedLoad
			{
				get
				{
					return this.Load * this.SupplyRatio;
				}
			}

			// Token: 0x06009A0E RID: 39438 RVA: 0x003E2284 File Offset: 0x003E0484
			[NullableContext(2)]
			public PowerGroup([Nullable(1)] PowerDistributor distributor, [Nullable(1)] Connection power, XElement element = null, Connection ratioInput = null, Connection ratioOutput = null)
			{
				this.distributor = distributor;
				this.PowerOut = power;
				this.RatioInput = ratioInput;
				this.RatioOutput = ratioOutput;
				distributor.powerGroups.Add(this);
				this.name = TextManager.GetWithVariable("groupx", "[num]", distributor.powerGroups.Count.ToString(), FormatCapitals.No).Value;
				this.SupplyRatio = 1f;
				if (element != null)
				{
					this.name = element.GetAttributeString("name", this.name);
					this.SupplyRatio = element.GetAttributeFloat("ratio", this.SupplyRatio);
				}
				this.DisplayName = TextManager.Get(this.name).Fallback(this.name, true);
				this.CreateGUI();
				Screen selected = Screen.Selected;
				if (selected == null || !selected.IsEditor)
				{
					this.Name = this.name;
				}
			}

			// Token: 0x06009A0F RID: 39439 RVA: 0x003E2394 File Offset: 0x003E0594
			public void ReceiveRatioSignal(Signal signal)
			{
				float receivedSignal;
				if (!float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out receivedSignal) || !MathUtils.IsValid(receivedSignal))
				{
					return;
				}
				this.DisplayRatio = receivedSignal;
			}

			// Token: 0x06009A10 RID: 39440 RVA: 0x003E23CC File Offset: 0x003E05CC
			public void SendRatioSignal()
			{
				this.distributor.item.SendSignal(new Signal(this.DisplayRatio.ToString(), 0, null, null, 0f, 1f), this.RatioOutput);
			}

			// Token: 0x040064D3 RID: 25811
			[Nullable(2)]
			private GUIFrame frame;

			// Token: 0x040064D4 RID: 25812
			[Nullable(2)]
			private GUIFrame groupContent;

			// Token: 0x040064D5 RID: 25813
			[Nullable(2)]
			private GUILayoutGroup nameGroup;

			// Token: 0x040064D6 RID: 25814
			[Nullable(2)]
			private GUITextBox nameBox;

			// Token: 0x040064D7 RID: 25815
			[Nullable(2)]
			private GUITextBlock loadDisplayNameLabel;

			// Token: 0x040064D8 RID: 25816
			[Nullable(2)]
			private GUIScrollBar ratioSlider;

			// Token: 0x040064D9 RID: 25817
			private readonly List<GUITextBlock> powerUnitLabels = new List<GUITextBlock>();

			// Token: 0x040064DA RID: 25818
			[Nullable(2)]
			private GUIFrame divider;

			// Token: 0x040064DC RID: 25820
			private readonly PowerDistributor distributor;

			// Token: 0x040064DD RID: 25821
			public readonly Connection PowerOut;

			// Token: 0x040064DE RID: 25822
			[Nullable(2)]
			public readonly Connection RatioInput;

			// Token: 0x040064DF RID: 25823
			[Nullable(2)]
			public readonly Connection RatioOutput;

			// Token: 0x040064E0 RID: 25824
			private string name;

			// Token: 0x040064E2 RID: 25826
			private float supplyRatio = 1f;

			// Token: 0x040064E3 RID: 25827
			public float Load;
		}

		// Token: 0x02001435 RID: 5173
		[NullableContext(0)]
		private enum EventType
		{
			// Token: 0x040064E5 RID: 25829
			NameChange,
			// Token: 0x040064E6 RID: 25830
			RatioChange
		}

		// Token: 0x02001436 RID: 5174
		[NullableContext(0)]
		private readonly struct EventData : ItemComponent.IEventData, IEquatable<PowerDistributor.EventData>
		{
			// Token: 0x06009A19 RID: 39449 RVA: 0x003E25F1 File Offset: 0x003E07F1
			[NullableContext(1)]
			public EventData(PowerDistributor.PowerGroup PowerGroup, PowerDistributor.EventType EventType)
			{
				this.PowerGroup = PowerGroup;
				this.EventType = EventType;
			}

			// Token: 0x17001D4B RID: 7499
			// (get) Token: 0x06009A1A RID: 39450 RVA: 0x003E2601 File Offset: 0x003E0801
			// (set) Token: 0x06009A1B RID: 39451 RVA: 0x003E2609 File Offset: 0x003E0809
			[Nullable(1)]
			public PowerDistributor.PowerGroup PowerGroup { [NullableContext(1)] get; [NullableContext(1)] set; }

			// Token: 0x17001D4C RID: 7500
			// (get) Token: 0x06009A1C RID: 39452 RVA: 0x003E2612 File Offset: 0x003E0812
			// (set) Token: 0x06009A1D RID: 39453 RVA: 0x003E261A File Offset: 0x003E081A
			public PowerDistributor.EventType EventType { get; set; }

			// Token: 0x06009A1E RID: 39454 RVA: 0x003E2624 File Offset: 0x003E0824
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("EventData");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06009A1F RID: 39455 RVA: 0x003E2670 File Offset: 0x003E0870
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("PowerGroup = ");
				builder.Append(this.PowerGroup);
				builder.Append(", EventType = ");
				builder.Append(this.EventType.ToString());
				return true;
			}

			// Token: 0x06009A20 RID: 39456 RVA: 0x003E26BE File Offset: 0x003E08BE
			[CompilerGenerated]
			public static bool operator !=(PowerDistributor.EventData left, PowerDistributor.EventData right)
			{
				return !(left == right);
			}

			// Token: 0x06009A21 RID: 39457 RVA: 0x003E26CA File Offset: 0x003E08CA
			[CompilerGenerated]
			public static bool operator ==(PowerDistributor.EventData left, PowerDistributor.EventData right)
			{
				return left.Equals(right);
			}

			// Token: 0x06009A22 RID: 39458 RVA: 0x003E26D4 File Offset: 0x003E08D4
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return EqualityComparer<PowerDistributor.PowerGroup>.Default.GetHashCode(this.<PowerGroup>k__BackingField) * -1521134295 + EqualityComparer<PowerDistributor.EventType>.Default.GetHashCode(this.<EventType>k__BackingField);
			}

			// Token: 0x06009A23 RID: 39459 RVA: 0x003E26FD File Offset: 0x003E08FD
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is PowerDistributor.EventData && this.Equals((PowerDistributor.EventData)obj);
			}

			// Token: 0x06009A24 RID: 39460 RVA: 0x003E2715 File Offset: 0x003E0915
			[CompilerGenerated]
			public bool Equals(PowerDistributor.EventData other)
			{
				return EqualityComparer<PowerDistributor.PowerGroup>.Default.Equals(this.<PowerGroup>k__BackingField, other.<PowerGroup>k__BackingField) && EqualityComparer<PowerDistributor.EventType>.Default.Equals(this.<EventType>k__BackingField, other.<EventType>k__BackingField);
			}

			// Token: 0x06009A25 RID: 39461 RVA: 0x003E2747 File Offset: 0x003E0947
			[NullableContext(1)]
			[CompilerGenerated]
			public void Deconstruct(out PowerDistributor.PowerGroup PowerGroup, out PowerDistributor.EventType EventType)
			{
				PowerGroup = this.PowerGroup;
				EventType = this.EventType;
			}
		}
	}
}
