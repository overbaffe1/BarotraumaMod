using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005CB RID: 1483
	internal class PowerContainer : Powered, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x170017A2 RID: 6050
		// (get) Token: 0x06005DF5 RID: 24053 RVA: 0x003104E9 File Offset: 0x0030E6E9
		// (set) Token: 0x06005DF6 RID: 24054 RVA: 0x003104F1 File Offset: 0x0030E6F1
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float RechargeWarningIndicatorLow { get; set; }

		// Token: 0x170017A3 RID: 6051
		// (get) Token: 0x06005DF7 RID: 24055 RVA: 0x003104FA File Offset: 0x0030E6FA
		// (set) Token: 0x06005DF8 RID: 24056 RVA: 0x00310502 File Offset: 0x0030E702
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float RechargeWarningIndicatorHigh { get; set; }

		// Token: 0x170017A4 RID: 6052
		// (get) Token: 0x06005DF9 RID: 24057 RVA: 0x0031050B File Offset: 0x0030E70B
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06005DFA RID: 24058 RVA: 0x00310512 File Offset: 0x0030E712
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			if (this.rechargeSpeedSlider != null)
			{
				this.rechargeSpeedSlider.BarScroll = this.rechargeSpeed / this.MaxRechargeSpeed;
			}
		}

		// Token: 0x06005DFB RID: 24059 RVA: 0x0031053C File Offset: 0x0030E73C
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (this.chargeIndicator != null)
			{
				float chargeRatio = this.charge / this.adjustedCapacity;
				this.chargeIndicator.Color = ToolBox.GradientLerp(chargeRatio, new Color[]
				{
					Color.Red,
					Color.Orange,
					Color.Green
				});
			}
		}

		// Token: 0x06005DFC RID: 24060 RVA: 0x0031059C File Offset: 0x0030E79C
		public void Draw(SpriteBatch spriteBatch, bool editing = false, float itemDepth = -1f, Color? overrideColor = null)
		{
			Vector2 scaledIndicatorSize = this.indicatorSize * this.item.Scale;
			if (scaledIndicatorSize.X <= 2f || scaledIndicatorSize.Y <= 2f)
			{
				return;
			}
			Vector2 itemSize = new Vector2((float)this.item.Sprite.SourceRect.Width, (float)this.item.Sprite.SourceRect.Height) * this.item.Scale;
			Vector2 indicatorPos = -itemSize / 2f + this.indicatorPosition * this.item.Scale;
			Vector2 itemPosition = new Vector2(this.item.DrawPosition.X, -this.item.DrawPosition.Y);
			Vector2 flip = new Vector2((this.item.FlippedX && this.item.Prefab.CanSpriteFlipX) ? -1f : 1f, (this.item.FlippedY && this.item.Prefab.CanSpriteFlipY) ? -1f : 1f);
			Matrix rotate = Matrix.CreateRotationZ(this.item.RotationRad);
			Vector2 center = Vector2.Transform((indicatorPos + scaledIndicatorSize * 0.5f) * flip, rotate) + itemPosition;
			if (this.charge > 0f && this.adjustedCapacity > 0f)
			{
				float chargeRatio = MathHelper.Clamp(this.charge / this.adjustedCapacity, 0f, 1f);
				Color indicatorColor = ToolBox.GradientLerp(chargeRatio, new Color[]
				{
					Color.Red,
					Color.Orange,
					Color.Green
				});
				Vector2 indicatorCenter = (indicatorPos + scaledIndicatorSize * 0.5f) * flip;
				Vector2 indicatorSize;
				if (this.isHorizontal)
				{
					float indicatorLength = (scaledIndicatorSize.X - 2f) * chargeRatio;
					indicatorCenter.X += -scaledIndicatorSize.X * 0.5f + (this.flipIndicator ? (scaledIndicatorSize.X - 1f - indicatorLength * 0.5f) : (1f + indicatorLength * 0.5f));
					indicatorSize = new Vector2(indicatorLength, scaledIndicatorSize.Y);
				}
				else
				{
					float indicatorLength2 = (scaledIndicatorSize.Y - 2f) * chargeRatio;
					indicatorCenter.Y += -scaledIndicatorSize.Y * 0.5f + (this.flipIndicator ? (1f + indicatorLength2 * 0.5f) : (scaledIndicatorSize.Y - 1f - indicatorLength2 * 0.5f));
					indicatorSize = new Vector2(scaledIndicatorSize.X, indicatorLength2);
				}
				indicatorCenter = Vector2.Transform(indicatorCenter, rotate) + itemPosition;
				GUI.DrawFilledRectangle(spriteBatch, indicatorCenter, indicatorSize, indicatorSize * 0.5f, this.item.RotationRad, indicatorColor, this.item.SpriteDepth - 1E-05f);
			}
			GUI.DrawRectangle(spriteBatch, center, scaledIndicatorSize, scaledIndicatorSize * 0.5f, this.item.RotationRad, Color.Black, this.item.SpriteDepth - 1.5E-05f, 1f, GUI.OutlinePosition.Inside);
		}

		// Token: 0x06005DFD RID: 24061 RVA: 0x003108E8 File Offset: 0x0030EAE8
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData)
		{
			msg.WriteRangedInteger((int)(this.rechargeSpeed / this.MaxRechargeSpeed * 10f), 0, 10);
		}

		// Token: 0x06005DFE RID: 24062 RVA: 0x00310908 File Offset: 0x0030EB08
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			if (this.correctionTimer > 0f)
			{
				base.StartDelayedCorrection(msg.ExtractBits(12), sendingTime, false);
				return;
			}
			float rechargeRate = (float)msg.ReadRangedInteger(0, 10) / 10f;
			this.RechargeSpeed = rechargeRate * this.MaxRechargeSpeed;
			if (this.rechargeSpeedSlider != null)
			{
				this.rechargeSpeedSlider.BarScroll = rechargeRate;
			}
			this.Charge = msg.ReadRangedSingle(0f, 1f, 8) * this.adjustedCapacity;
		}

		// Token: 0x170017A5 RID: 6053
		// (get) Token: 0x06005DFF RID: 24063 RVA: 0x00310984 File Offset: 0x0030EB84
		protected override PowerPriority Priority
		{
			get
			{
				return PowerPriority.Battery;
			}
		}

		// Token: 0x170017A6 RID: 6054
		// (get) Token: 0x06005E00 RID: 24064 RVA: 0x00310987 File Offset: 0x0030EB87
		// (set) Token: 0x06005E01 RID: 24065 RVA: 0x0031098F File Offset: 0x0030EB8F
		public float CurrPowerOutput
		{
			get
			{
				return this.currPowerOutput;
			}
			private set
			{
				this.currPowerOutput = Math.Max(0f, value);
			}
		}

		// Token: 0x170017A7 RID: 6055
		// (get) Token: 0x06005E02 RID: 24066 RVA: 0x003109A2 File Offset: 0x0030EBA2
		// (set) Token: 0x06005E03 RID: 24067 RVA: 0x003109AA File Offset: 0x0030EBAA
		[Serialize("0,0", IsPropertySaveable.Yes, "The position of the progress bar indicating the charge of the item. In pixels as an offset from the upper left corner of the sprite.", "", false)]
		public Vector2 IndicatorPosition
		{
			get
			{
				return this.indicatorPosition;
			}
			set
			{
				this.indicatorPosition = value;
			}
		}

		// Token: 0x170017A8 RID: 6056
		// (get) Token: 0x06005E04 RID: 24068 RVA: 0x003109B3 File Offset: 0x0030EBB3
		// (set) Token: 0x06005E05 RID: 24069 RVA: 0x003109BB File Offset: 0x0030EBBB
		[Serialize("0,0", IsPropertySaveable.Yes, "The size of the progress bar indicating the charge of the item (in pixels).", "", false)]
		public Vector2 IndicatorSize
		{
			get
			{
				return this.indicatorSize;
			}
			set
			{
				this.indicatorSize = value;
			}
		}

		// Token: 0x170017A9 RID: 6057
		// (get) Token: 0x06005E06 RID: 24070 RVA: 0x003109C4 File Offset: 0x0030EBC4
		// (set) Token: 0x06005E07 RID: 24071 RVA: 0x003109CC File Offset: 0x0030EBCC
		[Serialize(false, IsPropertySaveable.Yes, "Should the progress bar indicating the charge of the item fill up horizontally or vertically.", "", false)]
		public bool IsHorizontal
		{
			get
			{
				return this.isHorizontal;
			}
			set
			{
				this.isHorizontal = value;
			}
		}

		// Token: 0x170017AA RID: 6058
		// (get) Token: 0x06005E09 RID: 24073 RVA: 0x003109DE File Offset: 0x0030EBDE
		// (set) Token: 0x06005E08 RID: 24072 RVA: 0x003109D5 File Offset: 0x0030EBD5
		[Editable]
		[Serialize(10f, IsPropertySaveable.Yes, "Maximum output of the device when fully charged (kW).", "", false)]
		public float MaxOutPut { get; set; }

		// Token: 0x170017AB RID: 6059
		// (get) Token: 0x06005E0A RID: 24074 RVA: 0x003109E6 File Offset: 0x0030EBE6
		// (set) Token: 0x06005E0B RID: 24075 RVA: 0x003109EE File Offset: 0x0030EBEE
		[Editable]
		[Serialize(10f, IsPropertySaveable.Yes, "The maximum capacity of the device (kW * min). For example, a value of 1000 means the device can output 100 kilowatts of power for 10 minutes, or 1000 kilowatts for 1 minute.", "", false)]
		public float Capacity
		{
			get
			{
				return this.capacity;
			}
			set
			{
				this.capacity = Math.Max(value, 1f);
				this.adjustedCapacity = this.GetCapacity();
			}
		}

		// Token: 0x170017AC RID: 6060
		// (get) Token: 0x06005E0C RID: 24076 RVA: 0x00310A0D File Offset: 0x0030EC0D
		// (set) Token: 0x06005E0D RID: 24077 RVA: 0x00310A18 File Offset: 0x0030EC18
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "The current charge of the device.", "", false)]
		public float Charge
		{
			get
			{
				return this.charge;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.charge = MathHelper.Clamp(value, 0f, this.adjustedCapacity);
				if (Math.Abs(this.charge - this.lastSentCharge) / this.adjustedCapacity > 0.05f)
				{
					this.lastSentCharge = this.charge;
				}
			}
		}

		// Token: 0x170017AD RID: 6061
		// (get) Token: 0x06005E0E RID: 24078 RVA: 0x00310A71 File Offset: 0x0030EC71
		public float ChargePercentage
		{
			get
			{
				return MathUtils.Percentage(this.Charge, this.adjustedCapacity);
			}
		}

		// Token: 0x170017AE RID: 6062
		// (get) Token: 0x06005E0F RID: 24079 RVA: 0x00310A84 File Offset: 0x0030EC84
		// (set) Token: 0x06005E10 RID: 24080 RVA: 0x00310A8C File Offset: 0x0030EC8C
		[Editable]
		[Serialize(10f, IsPropertySaveable.Yes, "How fast the device can be recharged. For example, a recharge speed of 100 kW and a capacity of 1000 kW*min would mean it takes 10 minutes to fully charge the device.", "", false)]
		public float MaxRechargeSpeed
		{
			get
			{
				return this.maxRechargeSpeed;
			}
			set
			{
				this.maxRechargeSpeed = Math.Max(value, 1f);
			}
		}

		// Token: 0x170017AF RID: 6063
		// (get) Token: 0x06005E11 RID: 24081 RVA: 0x00310A9F File Offset: 0x0030EC9F
		// (set) Token: 0x06005E12 RID: 24082 RVA: 0x00310AA8 File Offset: 0x0030ECA8
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "The current recharge speed of the device.", "", false)]
		public float RechargeSpeed
		{
			get
			{
				return this.rechargeSpeed;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.rechargeSpeed = MathHelper.Clamp(value, 0f, this.maxRechargeSpeed);
				this.rechargeSpeed = MathUtils.RoundTowardsClosest(this.rechargeSpeed, Math.Max(this.maxRechargeSpeed * 0.1f, 1f));
				if (this.isRunning)
				{
					this.HasBeenTuned = true;
				}
			}
		}

		// Token: 0x170017B0 RID: 6064
		// (get) Token: 0x06005E13 RID: 24083 RVA: 0x00310B0B File Offset: 0x0030ED0B
		// (set) Token: 0x06005E14 RID: 24084 RVA: 0x00310B13 File Offset: 0x0030ED13
		[Serialize(false, IsPropertySaveable.Yes, "If true, the recharge speed (and power consumption) of the device goes up exponentially as the recharge rate is increased.", "", false)]
		public bool ExponentialRechargeSpeed { get; set; }

		// Token: 0x170017B1 RID: 6065
		// (get) Token: 0x06005E15 RID: 24085 RVA: 0x00310B1C File Offset: 0x0030ED1C
		// (set) Token: 0x06005E16 RID: 24086 RVA: 0x00310B24 File Offset: 0x0030ED24
		[Editable(0f, 1f, 2)]
		[Serialize(0.95f, IsPropertySaveable.Yes, "The amount of power you can get out of a item relative to the amount of power that's put into it.", "", false)]
		public float Efficiency
		{
			get
			{
				return this.efficiency;
			}
			set
			{
				this.efficiency = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x170017B2 RID: 6066
		// (get) Token: 0x06005E17 RID: 24087 RVA: 0x00310B3C File Offset: 0x0030ED3C
		// (set) Token: 0x06005E18 RID: 24088 RVA: 0x00310B44 File Offset: 0x0030ED44
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the progress bar indicating the charge be flipped to fill from the other side.", "", false)]
		public bool FlipIndicator
		{
			get
			{
				return this.flipIndicator;
			}
			set
			{
				this.flipIndicator = value;
			}
		}

		// Token: 0x170017B3 RID: 6067
		// (get) Token: 0x06005E19 RID: 24089 RVA: 0x00310B4D File Offset: 0x0030ED4D
		// (set) Token: 0x06005E1A RID: 24090 RVA: 0x00310B55 File Offset: 0x0030ED55
		public bool OutputDisabled { get; private set; }

		// Token: 0x170017B4 RID: 6068
		// (get) Token: 0x06005E1B RID: 24091 RVA: 0x00310B5E File Offset: 0x0030ED5E
		public float RechargeRatio
		{
			get
			{
				return this.RechargeSpeed / this.MaxRechargeSpeed;
			}
		}

		// Token: 0x170017B5 RID: 6069
		// (get) Token: 0x06005E1C RID: 24092 RVA: 0x00310B6D File Offset: 0x0030ED6D
		// (set) Token: 0x06005E1D RID: 24093 RVA: 0x00310B75 File Offset: 0x0030ED75
		public bool HasBeenTuned { get; private set; }

		// Token: 0x06005E1E RID: 24094 RVA: 0x00310B7E File Offset: 0x0030ED7E
		public PowerContainer(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.InitProjSpecific();
			this.prevCharge = this.Charge;
		}

		// Token: 0x06005E1F RID: 24095 RVA: 0x00310BA4 File Offset: 0x0030EDA4
		private void InitProjSpecific()
		{
			if (base.GuiFrame == null)
			{
				return;
			}
			GUIFrame paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.75f, 0.75f), base.GuiFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame upperArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.4f), paddedFrame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame lowerArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.6f), paddedFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame rechargeRateContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.4f), upperArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUITextBlock rechargeLabel = new GUITextBlock(new RectTransform(new Vector2(0.4f, 0f), rechargeRateContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("rechargerate"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null);
			LocalizedString kW = TextManager.Get("kilowatt");
			GUITextBlock rechargeText = new GUITextBlock(new RectTransform(new Vector2(0.6f, 1f), rechargeRateContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", new Color?(GUIStyle.TextColorNormal), GUIStyle.Font, Alignment.CenterRight, false, "", null)
			{
				TextGetter = delegate()
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 3);
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)MathF.Round(this.currPowerConsumption));
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(kW);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)MathF.Round(this.RechargeRatio * 100f));
					defaultInterpolatedStringHandler.AppendLiteral(" %)");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
			};
			if (rechargeText.TextSize.X > (float)rechargeText.Rect.Width)
			{
				rechargeText.Font = GUIStyle.SmallFont;
			}
			GUIFrame rechargeSliderContainer = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.4f), upperArea.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), "", null);
			if (this.RechargeWarningIndicatorLow > 0f || this.RechargeWarningIndicatorHigh > 0f)
			{
				GUICustomComponent rechargeSliderFill = new GUICustomComponent(new RectTransform(new Vector2(0.95f, 0.9f), rechargeSliderContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), delegate(SpriteBatch sb, GUICustomComponent c)
				{
					if (this.RechargeWarningIndicatorLow > 0f)
					{
						float warningLow = (float)c.Rect.Width * this.RechargeWarningIndicatorLow;
						GUI.DrawRectangle(sb, new Vector2((float)c.Rect.X + warningLow, (float)c.Rect.Y), new Vector2((float)c.Rect.Width - warningLow, (float)c.Rect.Height), GUIStyle.Orange, true, 0f, 1f);
					}
					if (this.RechargeWarningIndicatorHigh > 0f)
					{
						float warningHigh = (float)c.Rect.Width * this.RechargeWarningIndicatorHigh;
						GUI.DrawRectangle(sb, new Vector2((float)c.Rect.X + warningHigh, (float)c.Rect.Y), new Vector2((float)c.Rect.Width - warningHigh, (float)c.Rect.Height), GUIStyle.Red, true, 0f, 1f);
					}
				}, null);
			}
			this.rechargeSpeedSlider = new GUIScrollBar(new RectTransform(Vector2.One, rechargeSliderContainer.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), 0.15f, null, "DeviceSliderSeeThrough", null)
			{
				Step = 0.1f,
				OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
				{
					float newRechargeSpeed = this.maxRechargeSpeed * barScroll;
					if (Math.Abs(newRechargeSpeed - this.rechargeSpeed) < 0.1f)
					{
						return false;
					}
					this.RechargeSpeed = newRechargeSpeed;
					if (GameMain.Client != null)
					{
						this.item.CreateClientEvent<PowerContainer>(this);
						this.correctionTimer = 1f;
					}
					return true;
				}
			};
			this.rechargeSpeedSlider.Bar.RectTransform.MaxSize = new Point(this.rechargeSpeedSlider.Bar.Rect.Height);
			this.rechargeSpeedSlider.Frame.UserData = UIHighlightAction.ElementId.RechargeSpeedSlider;
			GUIFrame chargeTextContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.4f), lowerArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUITextBlock(new RectTransform(new Vector2(0.4f, 0f), chargeTextContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("charge"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null).ToolTip = TextManager.Get("PowerTransferTipPower");
			LocalizedString kWmin = TextManager.Get("kilowattminute");
			GUITextBlock chargeText = new GUITextBlock(new RectTransform(new Vector2(0.6f, 1f), chargeTextContainer.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), "", new Color?(GUIStyle.TextColorNormal), GUIStyle.Font, Alignment.CenterRight, false, "", null)
			{
				TextGetter = delegate()
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(7, 4);
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)MathF.Round(this.charge));
					defaultInterpolatedStringHandler.AppendLiteral("/");
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)this.adjustedCapacity);
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(kWmin);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted<int>((int)MathF.Round(MathUtils.Percentage(this.charge, this.adjustedCapacity)));
					defaultInterpolatedStringHandler.AppendLiteral(" %)");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
			};
			if (chargeText.TextSize.X > (float)chargeText.Rect.Width)
			{
				chargeText.Font = GUIStyle.SmallFont;
			}
			this.chargeIndicator = new GUIProgressBar(new RectTransform(new Vector2(1.1f, 0.5f), lowerArea.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, 0f, null, "DeviceProgressBar", true)
			{
				ProgressGetter = delegate()
				{
					if (this.adjustedCapacity > 0f)
					{
						return this.charge / this.adjustedCapacity;
					}
					return 1f;
				}
			};
		}

		// Token: 0x06005E20 RID: 24096 RVA: 0x003111B5 File Offset: 0x0030F3B5
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x06005E21 RID: 24097 RVA: 0x003111BC File Offset: 0x0030F3BC
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.item.Connections == null)
			{
				this.IsActive = false;
				return;
			}
			this.adjustedCapacity = this.GetCapacity();
			this.isRunning = true;
			float chargeRatio = this.charge / this.adjustedCapacity;
			if (chargeRatio > 0f)
			{
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			}
			float loadReading = 0f;
			if (base.powerOut != null && base.powerOut.Grid != null)
			{
				loadReading = base.powerOut.Grid.Load;
			}
			this.item.SendSignal(((int)Math.Round((double)this.CurrPowerOutput)).ToString(), "power_value_out");
			this.item.SendSignal(((int)Math.Round((double)loadReading)).ToString(), "load_value_out");
			this.item.SendSignal(((int)Math.Round((double)this.Charge)).ToString(), "charge");
			this.item.SendSignal(((int)Math.Round((double)(this.Charge / this.adjustedCapacity * 100f))).ToString(), "charge_%");
			this.item.SendSignal(((int)Math.Round((double)(this.RechargeSpeed / this.maxRechargeSpeed * 100f))).ToString(), "charge_rate");
		}

		// Token: 0x06005E22 RID: 24098 RVA: 0x0031131C File Offset: 0x0030F51C
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn)
			{
				this.CurrPowerOutput = 0f;
				return (float)((this.charge > 0f) ? -1 : 0);
			}
			if (this.charge >= this.adjustedCapacity)
			{
				this.charge = this.adjustedCapacity;
				return 0f;
			}
			if (this.item.Condition <= 0f)
			{
				return 0f;
			}
			float missingCharge = this.adjustedCapacity - this.charge;
			float targetRechargeSpeed = this.rechargeSpeed;
			if (this.ExponentialRechargeSpeed)
			{
				targetRechargeSpeed = MathF.Pow(this.rechargeSpeed / this.maxRechargeSpeed, 2f) * this.maxRechargeSpeed;
			}
			if (missingCharge < 1f)
			{
				targetRechargeSpeed *= missingCharge;
			}
			return MathHelper.Clamp(targetRechargeSpeed, 0f, this.MaxRechargeSpeed);
		}

		// Token: 0x06005E23 RID: 24099 RVA: 0x003113E4 File Offset: 0x0030F5E4
		public override PowerRange MinMaxPowerOut(Connection connection, float load = 0f)
		{
			if (this.OutputDisabled)
			{
				return PowerRange.Zero;
			}
			if (connection == base.powerOut)
			{
				float chargeRatio = this.prevCharge / this.adjustedCapacity;
				float maxOutput;
				if (chargeRatio < 0.1f)
				{
					maxOutput = Math.Max(chargeRatio * 10f, 0f) * this.MaxOutPut;
				}
				else
				{
					maxOutput = this.MaxOutPut;
				}
				maxOutput = Math.Min(maxOutput, this.prevCharge * 60f / 0.016666668f);
				return new PowerRange(0f, maxOutput);
			}
			return PowerRange.Zero;
		}

		// Token: 0x06005E24 RID: 24100 RVA: 0x0031146C File Offset: 0x0030F66C
		public override float GetConnectionPowerOut(Connection connection, float power, PowerRange minMaxPower, float load)
		{
			if (this.OutputDisabled)
			{
				return 0f;
			}
			if (connection == base.powerOut && minMaxPower.Max > 0f)
			{
				this.CurrPowerOutput = MathHelper.Clamp((load - power) / minMaxPower.Max, 0f, 1f) * this.MinMaxPowerOut(connection, load).Max;
				return this.CurrPowerOutput;
			}
			return 0f;
		}

		// Token: 0x06005E25 RID: 24101 RVA: 0x003114D8 File Offset: 0x0030F6D8
		public override void GridResolved(Connection conn)
		{
			if (conn == this.powerIn)
			{
				this.Charge += base.CurrPowerConsumption * base.Voltage / 60f * 0.016666668f * this.efficiency;
				return;
			}
			this.Charge = Math.Clamp(this.Charge - this.CurrPowerOutput / 60f * 0.016666668f, 0f, this.adjustedCapacity);
			this.prevCharge = this.Charge;
		}

		// Token: 0x06005E26 RID: 24102 RVA: 0x00311558 File Offset: 0x0030F758
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return false;
			}
			if (objective.Override)
			{
				this.HasBeenTuned = false;
			}
			if (this.HasBeenTuned)
			{
				return true;
			}
			float targetRatio = (objective.Option.IsEmpty || objective.Option == "charge") ? 0.5f : -1f;
			if (targetRatio > 0f || float.TryParse(objective.Option.Value, out targetRatio))
			{
				if (Math.Abs(this.rechargeSpeed - this.maxRechargeSpeed * targetRatio) > 0.05f)
				{
					this.RechargeSpeed = this.maxRechargeSpeed * targetRatio;
					if (this.rechargeSpeedSlider != null)
					{
						this.rechargeSpeedSlider.BarScroll = this.RechargeSpeed / Math.Max(this.maxRechargeSpeed, 1f);
					}
					if (character.IsOnPlayerTeam)
					{
						character.Speak(TextManager.GetWithVariables("DialogChargeBatteries", new ValueTuple<string, string, FormatCapitals>[]
						{
							new ValueTuple<string, string, FormatCapitals>("[itemname]", this.item.Name, FormatCapitals.Yes),
							new ValueTuple<string, string, FormatCapitals>("[rate]", ((int)(this.rechargeSpeed / this.maxRechargeSpeed * 100f)).ToString(), FormatCapitals.No)
						}).Value, null, 1f, "chargebattery".ToIdentifier(), 10f);
					}
				}
			}
			else if (this.rechargeSpeed > 0f)
			{
				this.RechargeSpeed = 0f;
				if (this.rechargeSpeedSlider != null)
				{
					this.rechargeSpeedSlider.BarScroll = this.RechargeSpeed / Math.Max(this.maxRechargeSpeed, 1f);
				}
				if (character.IsOnPlayerTeam)
				{
					character.Speak(TextManager.GetWithVariables("DialogStopChargingBatteries", new ValueTuple<string, string, FormatCapitals>[]
					{
						new ValueTuple<string, string, FormatCapitals>("[itemname]", this.item.Name, FormatCapitals.Yes),
						new ValueTuple<string, string, FormatCapitals>("[rate]", ((int)(this.rechargeSpeed / this.maxRechargeSpeed * 100f)).ToString(), FormatCapitals.No)
					}).Value, null, 1f, "chargebattery".ToIdentifier(), 10f);
				}
			}
			return true;
		}

		// Token: 0x06005E27 RID: 24103 RVA: 0x0031179C File Offset: 0x0030F99C
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (connection.IsPower)
			{
				return;
			}
			string name = connection.Name;
			if (name == "disable_output")
			{
				this.OutputDisabled = (signal.value != "0");
				return;
			}
			if (!(name == "set_rate"))
			{
				return;
			}
			float tempSpeed;
			if (float.TryParse(signal.value, NumberStyles.Any, CultureInfo.InvariantCulture, out tempSpeed))
			{
				if (!MathUtils.IsValid(tempSpeed))
				{
					return;
				}
				float rechargeRate = MathHelper.Clamp(tempSpeed / 100f, 0f, 1f);
				this.RechargeSpeed = rechargeRate * this.MaxRechargeSpeed;
				if (this.rechargeSpeedSlider != null)
				{
					this.rechargeSpeedSlider.BarScroll = rechargeRate;
				}
			}
		}

		// Token: 0x06005E28 RID: 24104 RVA: 0x00311846 File Offset: 0x0030FA46
		public float GetCapacity()
		{
			return this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.BatteryCapacity, this.Capacity);
		}

		// Token: 0x0400308E RID: 12430
		private GUIProgressBar chargeIndicator;

		// Token: 0x0400308F RID: 12431
		private GUIScrollBar rechargeSpeedSlider;

		// Token: 0x04003092 RID: 12434
		private float capacity;

		// Token: 0x04003093 RID: 12435
		private float adjustedCapacity;

		// Token: 0x04003094 RID: 12436
		private float charge;

		// Token: 0x04003095 RID: 12437
		private float prevCharge;

		// Token: 0x04003096 RID: 12438
		private float maxRechargeSpeed;

		// Token: 0x04003097 RID: 12439
		private float rechargeSpeed;

		// Token: 0x04003098 RID: 12440
		private float lastSentCharge;

		// Token: 0x04003099 RID: 12441
		protected Vector2 indicatorPosition;

		// Token: 0x0400309A RID: 12442
		protected Vector2 indicatorSize;

		// Token: 0x0400309B RID: 12443
		protected bool isHorizontal;

		// Token: 0x0400309C RID: 12444
		private float currPowerOutput;

		// Token: 0x0400309F RID: 12447
		private float efficiency;

		// Token: 0x040030A0 RID: 12448
		private bool flipIndicator;

		// Token: 0x040030A2 RID: 12450
		public const float aiRechargeTargetRatio = 0.5f;

		// Token: 0x040030A3 RID: 12451
		private bool isRunning;
	}
}
