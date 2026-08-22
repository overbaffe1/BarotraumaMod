using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005C5 RID: 1477
	internal class Pump : Powered, IServerSerializable, INetSerializable, IClientSerializable, IDeteriorateUnderStress
	{
		// Token: 0x1700174D RID: 5965
		// (get) Token: 0x06005CB4 RID: 23732 RVA: 0x002FD459 File Offset: 0x002FB659
		// (set) Token: 0x06005CB5 RID: 23733 RVA: 0x002FD461 File Offset: 0x002FB661
		public GUIButton PowerButton { get; private set; }

		// Token: 0x06005CB6 RID: 23734 RVA: 0x002FD46A File Offset: 0x002FB66A
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			if (this.pumpSpeedSlider != null)
			{
				this.pumpSpeedSlider.BarScroll = (this.flowPercentage + 100f) / 200f;
			}
		}

		// Token: 0x06005CB7 RID: 23735 RVA: 0x002FD498 File Offset: 0x002FB698
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			this.autoControlIndicator.Selected = this.IsAutoControlled;
			this.PowerButton.Enabled = (this.isActiveLockTimer <= 0f);
			if (this.HasPower && !this.Disabled)
			{
				this.flickerTimer = 0f;
				this.powerLight.Selected = this.IsActive;
			}
			else if (this.IsActive)
			{
				this.flickerTimer += deltaTime;
				if (this.flickerTimer > this.flickerFrequency)
				{
					this.flickerTimer = 0f;
					this.powerLight.Selected = !this.powerLight.Selected;
				}
			}
			else
			{
				this.flickerTimer = 0f;
				this.powerLight.Selected = false;
			}
			this.pumpSpeedSlider.Enabled = (this.pumpSpeedLockTimer <= 0f && this.IsActive);
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				float pumpSpeedScroll = (this.FlowPercentage + 100f) / 200f;
				if (Math.Abs(pumpSpeedScroll - this.pumpSpeedSlider.BarScroll) > 0.01f)
				{
					this.pumpSpeedSlider.BarScroll = pumpSpeedScroll;
				}
			}
		}

		// Token: 0x06005CB8 RID: 23736 RVA: 0x002FD5C0 File Offset: 0x002FB7C0
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedInteger((int)(this.flowPercentage / 10f), -10, 10);
			msg.WriteBoolean(this.IsActive);
		}

		// Token: 0x06005CB9 RID: 23737 RVA: 0x002FD5E8 File Offset: 0x002FB7E8
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			int msgStartPos = msg.BitPosition;
			float flowPercentage = (float)msg.ReadRangedInteger(-10, 10) * 10f;
			bool isActive = msg.ReadBoolean();
			bool hijacked = msg.ReadBoolean();
			bool disabled = msg.ReadBoolean();
			float? targetLevel;
			if (msg.ReadBoolean())
			{
				targetLevel = new float?(msg.ReadSingle());
			}
			else
			{
				targetLevel = null;
			}
			if (this.correctionTimer > 0f)
			{
				int msgLength = msg.BitPosition - msgStartPos;
				msg.BitPosition = msgStartPos;
				base.StartDelayedCorrection(msg.ExtractBits(msgLength), sendingTime, false);
				return;
			}
			this.FlowPercentage = flowPercentage;
			this.IsActive = isActive;
			this.Hijacked = hijacked;
			this.Disabled = disabled;
			this.TargetLevel = targetLevel;
		}

		// Token: 0x1700174E RID: 5966
		// (get) Token: 0x06005CBA RID: 23738 RVA: 0x002FD697 File Offset: 0x002FB897
		// (set) Token: 0x06005CBB RID: 23739 RVA: 0x002FD69F File Offset: 0x002FB89F
		public bool Hijacked
		{
			get
			{
				return this.hijacked;
			}
			set
			{
				if (value == this.hijacked)
				{
					return;
				}
				this.hijacked = value;
			}
		}

		// Token: 0x1700174F RID: 5967
		// (get) Token: 0x06005CBC RID: 23740 RVA: 0x002FD6B4 File Offset: 0x002FB8B4
		public float CurrentBrokenVolume
		{
			get
			{
				if (this.item.ConditionPercentage > 10f || !this.IsActive || this.Disabled)
				{
					return 0f;
				}
				return (1f - this.item.ConditionPercentage / 10f) * 100f;
			}
		}

		// Token: 0x17001750 RID: 5968
		// (get) Token: 0x06005CBD RID: 23741 RVA: 0x002FD706 File Offset: 0x002FB906
		// (set) Token: 0x06005CBE RID: 23742 RVA: 0x002FD70E File Offset: 0x002FB90E
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the item is currently pumping water (-100 = full speed out, 100 = full speed in). Intended to be used by StatusEffect conditionals (setting this value in XML has no effect).", "", false)]
		public float FlowPercentage
		{
			get
			{
				return this.flowPercentage;
			}
			set
			{
				if (!MathUtils.IsValid(this.flowPercentage))
				{
					return;
				}
				this.flowPercentage = MathHelper.Clamp(value, -100f, 100f);
				this.flowPercentage = MathF.Round(this.flowPercentage);
			}
		}

		// Token: 0x17001751 RID: 5969
		// (get) Token: 0x06005CBF RID: 23743 RVA: 0x002FD745 File Offset: 0x002FB945
		// (set) Token: 0x06005CC0 RID: 23744 RVA: 0x002FD74D File Offset: 0x002FB94D
		[Editable]
		[Serialize(80f, IsPropertySaveable.No, "How fast the item pumps water in/out when operating at 100%.", "", true)]
		public float MaxFlow
		{
			get
			{
				return this.maxFlow;
			}
			set
			{
				this.maxFlow = value;
			}
		}

		// Token: 0x17001752 RID: 5970
		// (get) Token: 0x06005CC1 RID: 23745 RVA: 0x002FD756 File Offset: 0x002FB956
		// (set) Token: 0x06005CC2 RID: 23746 RVA: 0x002FD75E File Offset: 0x002FB95E
		[Serialize(false, IsPropertySaveable.Yes, "If true, the pump is unable to pump water.", "", true)]
		public bool Disabled
		{
			get
			{
				return this.disabled;
			}
			set
			{
				if (this.disabled == value)
				{
					return;
				}
				this.disabled = value;
			}
		}

		// Token: 0x17001753 RID: 5971
		// (get) Token: 0x06005CC3 RID: 23747 RVA: 0x002FD771 File Offset: 0x002FB971
		// (set) Token: 0x06005CC4 RID: 23748 RVA: 0x002FD779 File Offset: 0x002FB979
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "", "", true)]
		public bool IsOn
		{
			get
			{
				return this.IsActive;
			}
			set
			{
				this.IsActive = value;
			}
		}

		// Token: 0x17001754 RID: 5972
		// (get) Token: 0x06005CC5 RID: 23749 RVA: 0x002FD782 File Offset: 0x002FB982
		// (set) Token: 0x06005CC6 RID: 23750 RVA: 0x002FD78A File Offset: 0x002FB98A
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool CanCauseLethalPressure { get; set; }

		// Token: 0x17001755 RID: 5973
		// (get) Token: 0x06005CC7 RID: 23751 RVA: 0x002FD793 File Offset: 0x002FB993
		public float CurrFlow
		{
			get
			{
				if (!this.IsActive)
				{
					return 0f;
				}
				return Math.Abs(this.currFlow);
			}
		}

		// Token: 0x17001756 RID: 5974
		// (get) Token: 0x06005CC8 RID: 23752 RVA: 0x002FD7AE File Offset: 0x002FB9AE
		public bool IsHullFull
		{
			get
			{
				return this.item.CurrentHull != null && this.item.CurrentHull.WaterVolume >= this.item.CurrentHull.Volume * 1.05f;
			}
		}

		// Token: 0x17001757 RID: 5975
		// (get) Token: 0x06005CC9 RID: 23753 RVA: 0x002FD7EA File Offset: 0x002FB9EA
		public override bool HasPower
		{
			get
			{
				return this.IsActive && base.Voltage >= base.MinVoltage;
			}
		}

		// Token: 0x17001758 RID: 5976
		// (get) Token: 0x06005CCA RID: 23754 RVA: 0x002FD807 File Offset: 0x002FBA07
		public bool IsAutoControlled
		{
			get
			{
				return this.pumpSpeedLockTimer > 0f || this.isActiveLockTimer > 0f;
			}
		}

		// Token: 0x17001759 RID: 5977
		// (get) Token: 0x06005CCB RID: 23755 RVA: 0x002FD825 File Offset: 0x002FBA25
		public override bool UpdateWhenInactive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700175A RID: 5978
		// (get) Token: 0x06005CCC RID: 23756 RVA: 0x002FD828 File Offset: 0x002FBA28
		public float CurrentStress
		{
			get
			{
				if (!this.IsActive)
				{
					return 0f;
				}
				return Math.Abs(this.flowPercentage / 100f);
			}
		}

		// Token: 0x06005CCD RID: 23757 RVA: 0x002FD849 File Offset: 0x002FBA49
		public Pump(Item item, ContentXElement element) : base(item, element)
		{
			this.InitProjSpecific(element);
		}

		// Token: 0x06005CCE RID: 23758 RVA: 0x002FD888 File Offset: 0x002FBA88
		private void InitProjSpecific(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "pumpoutemitter"))
				{
					if (a == "pumpinemitter")
					{
						List<ValueTuple<Vector2, ParticleEmitter>> list = this.pumpInEmitters;
						ContentXElement contentXElement = subElement;
						string key = "position";
						Vector2 zero = Vector2.Zero;
						list.Add(new ValueTuple<Vector2, ParticleEmitter>(contentXElement.GetAttributeVector2(key, zero), new ParticleEmitter(subElement)));
					}
				}
				else
				{
					List<ValueTuple<Vector2, ParticleEmitter>> list2 = this.pumpOutEmitters;
					ContentXElement contentXElement2 = subElement;
					string key2 = "position";
					Vector2 zero = Vector2.Zero;
					list2.Add(new ValueTuple<Vector2, ParticleEmitter>(contentXElement2.GetAttributeVector2(key2, zero), new ParticleEmitter(subElement)));
				}
			}
			if (base.GuiFrame == null)
			{
				return;
			}
			GUIFrame paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.85f, 0.65f), base.GuiFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.04f)
			}, null, null);
			float powerButtonSize = 1f;
			GUIFrame powerArea = new GUIFrame(new RectTransform(new Vector2(0.3f, 1f) * powerButtonSize, paddedFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIFrame paddedPowerArea = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.8f), powerArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "PowerButtonFrame", null);
			GUIFrame powerLightArea = new GUIFrame(new RectTransform(new Vector2(0.87f, 0.2f), powerArea.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), null, null);
			this.powerLight = new GUITickBox(new RectTransform(Vector2.One, powerLightArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), TextManager.Get("PowerLabel"), GUIStyle.SubHeadingFont, "IndicatorLightPower")
			{
				CanBeFocused = false
			};
			this.powerLight.TextBlock.AutoScaleHorizontal = true;
			this.powerLight.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			this.PowerButton = new GUIButton(new RectTransform(new Vector2(0.8f, 0.75f), paddedPowerArea.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.1f)
			}, Alignment.Center, "PowerButton", null)
			{
				UserData = UIHighlightAction.ElementId.PowerButton,
				OnClicked = delegate(GUIButton button, object data)
				{
					this.TargetLevel = null;
					this.IsActive = !this.IsActive;
					if (GameMain.Client != null)
					{
						this.correctionTimer = 1f;
						this.item.CreateClientEvent<Pump>(this);
					}
					this.powerLight.Selected = this.IsActive;
					return true;
				}
			};
			GUIFrame rightArea = new GUIFrame(new RectTransform(new Vector2(0.65f, 1f), paddedFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, null);
			this.autoControlIndicator = new GUITickBox(new RectTransform(new Vector2(1f, 0.25f), rightArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get(new string[]
			{
				"PumpAutoControl",
				"ReactorAutoControl"
			}), GUIStyle.SubHeadingFont, "IndicatorLightYellow")
			{
				Selected = false,
				Enabled = false,
				ToolTip = TextManager.Get("AutoControlTip")
			};
			this.autoControlIndicator.TextBlock.AutoScaleHorizontal = true;
			this.autoControlIndicator.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			GUIFrame sliderArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.65f), rightArea.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUITextBlock pumpSpeedText = new GUITextBlock(new RectTransform(new Vector2(1f, 0.3f), sliderArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null)
			{
				AutoScaleHorizontal = true
			};
			LocalizedString pumpSpeedStr = TextManager.Get("PumpSpeed");
			pumpSpeedText.TextGetter = (() => TextManager.AddPunctuation(':', new LocalizedString[]
			{
				pumpSpeedStr,
				((int)Math.Round((double)this.flowPercentage)).ToString() + " %"
			}));
			this.pumpSpeedSlider = new GUIScrollBar(new RectTransform(new Vector2(1f, 0.35f), sliderArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), 0.1f, null, "DeviceSlider", null)
			{
				Step = 0.05f,
				OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
				{
					if (this.pumpSpeedLockTimer <= 0f)
					{
						this.TargetLevel = null;
					}
					float newValue = barScroll * 200f - 100f;
					if (Math.Abs(newValue - this.FlowPercentage) < 0.1f)
					{
						return false;
					}
					this.FlowPercentage = newValue;
					if (GameMain.Client != null)
					{
						this.correctionTimer = 1f;
						this.item.CreateClientEvent<Pump>(this);
					}
					return true;
				}
			};
			this.pumpSpeedSlider.Frame.UserData = UIHighlightAction.ElementId.PumpSpeedSlider;
			GUIFrame textsArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.25f), sliderArea.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, null);
			GUITextBlock outLabel = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), textsArea.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("PumpOut"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null);
			GUITextBlock inLabel = new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), textsArea.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("PumpIn"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterRight, false, "", null);
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				outLabel,
				inLabel
			});
		}

		// Token: 0x06005CCF RID: 23759 RVA: 0x002FDFB8 File Offset: 0x002FC1B8
		public override void Update(float deltaTime, Camera cam)
		{
			this.pumpSpeedLockTimer -= deltaTime;
			this.isActiveLockTimer -= deltaTime;
			this.currFlow = 0f;
			if (this.item.CurrentHull == null)
			{
				if (this.TargetLevel != null)
				{
					this.FlowPercentage = 0f;
				}
				return;
			}
			if (this.TargetLevel != null)
			{
				float hullWaterVolume = this.item.CurrentHull.WaterVolume;
				float totalHullVolume = this.item.CurrentHull.Volume;
				this.linkedHulls.Clear();
				this.item.CurrentHull.GetLinkedHulls(this.linkedHulls, true);
				foreach (Hull linkedHull in this.linkedHulls)
				{
					if (linkedHull != this.item.CurrentHull)
					{
						hullWaterVolume += linkedHull.WaterVolume;
						totalHullVolume += linkedHull.Volume;
					}
				}
				float hullPercentage = hullWaterVolume / totalHullVolume * 100f;
				this.FlowPercentage = (this.TargetLevel.Value - hullPercentage) * 10f;
			}
			if (!this.IsActive || this.Disabled)
			{
				return;
			}
			if (this.flowPercentage <= 0f && this.item.CurrentHull.WaterVolume <= 0f)
			{
				return;
			}
			float powerFactor = Math.Min((base.PowerConsumption <= 0f || base.MinVoltage <= 0f) ? 1f : base.Voltage, 2f);
			this.currFlow = this.flowPercentage / 100f * this.MaxFlow * powerFactor;
			Repairable repairable = this.item.GetComponent<Repairable>();
			if (repairable != null && repairable.IsTinkering)
			{
				this.currFlow *= 1f + repairable.TinkeringStrength * 4f;
			}
			this.currFlow = this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.PumpSpeed, this.currFlow);
			this.currFlow *= MathHelper.Lerp(0.5f, 1f, this.item.Condition / this.item.MaxCondition);
			if (MathUtils.NearlyEqual(this.currFlow, 0f, 0.01f))
			{
				this.currFlow = 0f;
				return;
			}
			this.item.CurrentHull.WaterVolume += this.currFlow * deltaTime * 60f;
			if (this.flowPercentage > 0f && this.item.CurrentHull.WaterVolume > this.item.CurrentHull.Volume)
			{
				this.item.CurrentHull.Pressure += 30f * deltaTime;
				if (this.CanCauseLethalPressure)
				{
					this.item.CurrentHull.LethalPressure += 15f * deltaTime;
				}
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			this.UpdateProjSpecific(deltaTime);
		}

		// Token: 0x06005CD0 RID: 23760 RVA: 0x002FE2D8 File Offset: 0x002FC4D8
		public void InfectBallast(Identifier identifier, bool allowMultiplePerShip = false)
		{
			Hull hull = this.item.CurrentHull;
			if (hull == null)
			{
				return;
			}
			if (!allowMultiplePerShip)
			{
				if ((from h in Hull.HullList
				where h.Submarine == hull.Submarine
				select h).Any((Hull h) => h.BallastFlora != null))
				{
					return;
				}
			}
			if (hull.BallastFlora != null)
			{
				return;
			}
			BallastFloraPrefab ballastFloraPrefab = BallastFloraPrefab.Find(identifier);
			if (ballastFloraPrefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(96, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Failed to infect a ballast pump (could not find a ballast flora prefab with the identifier \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(identifier);
				defaultInterpolatedStringHandler.AppendLiteral("\").\n");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear() + Environment.StackTrace, null, null, false, false);
				return;
			}
			Vector2 offset = this.item.WorldPosition - hull.WorldPosition;
			hull.BallastFlora = new BallastFloraBehavior(hull, ballastFloraPrefab, offset, true);
		}

		// Token: 0x06005CD1 RID: 23761 RVA: 0x002FE3D8 File Offset: 0x002FC5D8
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive || this.Disabled)
			{
				return 0f;
			}
			this.currPowerConsumption = this.powerConsumption * Math.Abs(this.flowPercentage / 100f);
			Repairable component = this.item.GetComponent<Repairable>();
			if (component != null)
			{
				component.AdjustPowerConsumption(ref this.currPowerConsumption);
			}
			return this.currPowerConsumption;
		}

		// Token: 0x06005CD2 RID: 23762 RVA: 0x002FE444 File Offset: 0x002FC644
		private void UpdateProjSpecific(float deltaTime)
		{
			Pump.<>c__DisplayClass63_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.deltaTime = deltaTime;
			if (this.currFlow < 0f)
			{
				using (List<ValueTuple<Vector2, ParticleEmitter>>.Enumerator enumerator = this.pumpOutEmitters.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ValueTuple<Vector2, ParticleEmitter> valueTuple = enumerator.Current;
						Vector2 position = valueTuple.Item1;
						ParticleEmitter emitter = valueTuple.Item2;
						if (this.item.CurrentHull == null || this.item.CurrentHull.Surface >= (float)this.item.Rect.Location.Y + position.Y)
						{
							this.<UpdateProjSpecific>g__Emit|63_0(position, emitter, ref CS$<>8__locals1);
						}
					}
					return;
				}
			}
			if (this.currFlow > 0f)
			{
				foreach (ValueTuple<Vector2, ParticleEmitter> valueTuple2 in this.pumpInEmitters)
				{
					Vector2 position2 = valueTuple2.Item1;
					ParticleEmitter emitter2 = valueTuple2.Item2;
					this.<UpdateProjSpecific>g__Emit|63_0(position2, emitter2, ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x06005CD3 RID: 23763 RVA: 0x002FE56C File Offset: 0x002FC76C
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.Hijacked)
			{
				return;
			}
			if (connection.Name == "toggle")
			{
				this.IsActive = !this.IsActive;
				this.isActiveLockTimer = 0.1f;
				return;
			}
			if (connection.Name == "set_active")
			{
				this.IsActive = (signal.value != "0");
				this.isActiveLockTimer = 0.1f;
				return;
			}
			float tempTarget;
			if (connection.Name == "set_speed")
			{
				float tempSpeed;
				if (float.TryParse(signal.value, NumberStyles.Any, CultureInfo.InvariantCulture, out tempSpeed))
				{
					this.flowPercentage = MathHelper.Clamp(tempSpeed, -100f, 100f);
					this.TargetLevel = null;
					this.pumpSpeedLockTimer = 0.1f;
					return;
				}
			}
			else if (connection.Name == "set_targetlevel" && float.TryParse(signal.value, NumberStyles.Any, CultureInfo.InvariantCulture, out tempTarget))
			{
				this.TargetLevel = new float?(MathUtils.InverseLerp(-100f, 100f, tempTarget) * 100f);
				this.pumpSpeedLockTimer = 0.1f;
			}
		}

		// Token: 0x06005CD4 RID: 23764 RVA: 0x002FE698 File Offset: 0x002FC898
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			if (GameMain.Client != null)
			{
				return false;
			}
			string a = objective.Option.Value.ToLowerInvariant();
			if (!(a == "pumpout"))
			{
				if (!(a == "pumpin"))
				{
					if (a == "stoppumping")
					{
						this.IsActive = false;
						this.FlowPercentage = 0f;
					}
				}
				else
				{
					this.IsActive = true;
					this.FlowPercentage = 100f;
				}
			}
			else
			{
				this.IsActive = true;
				this.FlowPercentage = -100f;
			}
			return true;
		}

		// Token: 0x06005CD5 RID: 23765 RVA: 0x002FE723 File Offset: 0x002FC923
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.linkedHulls.Clear();
		}

		// Token: 0x06005CD6 RID: 23766 RVA: 0x002FE738 File Offset: 0x002FC938
		[CompilerGenerated]
		private void <UpdateProjSpecific>g__Emit|63_0(Vector2 position, ParticleEmitter emitter, ref Pump.<>c__DisplayClass63_0 A_3)
		{
			Vector2 relativeParticlePos = this.item.WorldRect.Location.ToVector2() + position * this.item.Scale - this.item.WorldPosition;
			if (this.item.FlippedX)
			{
				relativeParticlePos.X = -relativeParticlePos.X;
			}
			if (this.item.FlippedY)
			{
				relativeParticlePos.Y = -relativeParticlePos.Y;
			}
			relativeParticlePos = MathUtils.RotatePoint(relativeParticlePos, -this.item.RotationRad);
			float angle = -this.item.RotationRad;
			if (this.item.FlippedX)
			{
				angle += 3.1415927f;
			}
			float deltaTime = A_3.deltaTime;
			Vector2 position2 = this.item.WorldPosition + relativeParticlePos;
			Hull currentHull = this.item.CurrentHull;
			float angle2 = angle;
			float particleRotation = 0f;
			float velocityMultiplier = MathHelper.Lerp(0.5f, 1f, this.currFlow / this.maxFlow);
			float sizeMultiplier = 1f;
			float amountMultiplier = 1f;
			bool mirrorAngle = this.item.FlippedX ^ this.item.FlippedY;
			emitter.Emit(deltaTime, position2, currentHull, angle2, particleRotation, velocityMultiplier, sizeMultiplier, amountMultiplier, null, null, mirrorAngle, null);
		}

		// Token: 0x04002F70 RID: 12144
		private GUIScrollBar pumpSpeedSlider;

		// Token: 0x04002F71 RID: 12145
		private GUITickBox powerLight;

		// Token: 0x04002F72 RID: 12146
		private GUITickBox autoControlIndicator;

		// Token: 0x04002F73 RID: 12147
		[TupleElementNames(new string[]
		{
			"position",
			"emitter"
		})]
		private readonly List<ValueTuple<Vector2, ParticleEmitter>> pumpOutEmitters = new List<ValueTuple<Vector2, ParticleEmitter>>();

		// Token: 0x04002F74 RID: 12148
		[TupleElementNames(new string[]
		{
			"position",
			"emitter"
		})]
		private readonly List<ValueTuple<Vector2, ParticleEmitter>> pumpInEmitters = new List<ValueTuple<Vector2, ParticleEmitter>>();

		// Token: 0x04002F75 RID: 12149
		private float flickerTimer;

		// Token: 0x04002F76 RID: 12150
		private readonly float flickerFrequency = 1f;

		// Token: 0x04002F77 RID: 12151
		private float flowPercentage;

		// Token: 0x04002F78 RID: 12152
		private float maxFlow;

		// Token: 0x04002F79 RID: 12153
		public float? TargetLevel;

		// Token: 0x04002F7A RID: 12154
		private bool hijacked;

		// Token: 0x04002F7B RID: 12155
		private float pumpSpeedLockTimer;

		// Token: 0x04002F7C RID: 12156
		private float isActiveLockTimer;

		// Token: 0x04002F7D RID: 12157
		private bool disabled;

		// Token: 0x04002F7F RID: 12159
		private float currFlow;

		// Token: 0x04002F80 RID: 12160
		private const float TinkeringSpeedIncrease = 4f;

		// Token: 0x04002F81 RID: 12161
		private readonly List<Hull> linkedHulls = new List<Hull>();
	}
}
