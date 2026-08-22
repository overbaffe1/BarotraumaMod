using System;
using System.Globalization;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005BB RID: 1467
	internal class Engine : Powered, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable, IDeteriorateUnderStress
	{
		// Token: 0x17001728 RID: 5928
		// (get) Token: 0x06005BCF RID: 23503 RVA: 0x002EFEC2 File Offset: 0x002EE0C2
		// (set) Token: 0x06005BD0 RID: 23504 RVA: 0x002EFECA File Offset: 0x002EE0CA
		public float AnimSpeed { get; private set; }

		// Token: 0x17001729 RID: 5929
		// (get) Token: 0x06005BD1 RID: 23505 RVA: 0x002EFED3 File Offset: 0x002EE0D3
		public Vector2 DrawSize
		{
			get
			{
				return Vector2.Zero;
			}
		}

		// Token: 0x06005BD2 RID: 23506 RVA: 0x002EFEDC File Offset: 0x002EE0DC
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			this.powerIndicator.Selected = (this.hasPower && this.IsActive);
			this.autoControlIndicator.Selected = (this.controlLockTimer > 0f);
			this.forceSlider.Enabled = (this.controlLockTimer <= 0f);
			if (!PlayerInput.PrimaryMouseButtonHeld())
			{
				float newScroll = (this.targetForce + 100f) / 200f;
				if (Math.Abs(newScroll - this.forceSlider.BarScroll) > 0.01f)
				{
					this.forceSlider.BarScroll = newScroll;
				}
			}
		}

		// Token: 0x06005BD3 RID: 23507 RVA: 0x002EFF78 File Offset: 0x002EE178
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			if (this.propellerSprite != null)
			{
				Vector2 drawPos = this.item.DrawPosition;
				drawPos += this.PropellerPos;
				drawPos.Y = -drawPos.Y;
				this.propellerSprite.Draw(spriteBatch, (int)Math.Floor((double)this.spriteIndex), drawPos, overrideColor ?? Color.White, this.propellerSprite.Origin, 0f, Vector2.One, SpriteEffects.None, null);
			}
			if (editing && !this.DisablePropellerDamage && this.propellerDamage != null && !GUI.DisableHUD)
			{
				Vector2 drawPos2 = this.item.DrawPosition;
				drawPos2 += this.PropellerPos * this.item.Scale;
				drawPos2.Y = -drawPos2.Y;
				spriteBatch.DrawCircle(drawPos2, this.propellerDamage.DamageRange * this.item.Scale, 16, GUIStyle.Red, 2f);
			}
		}

		// Token: 0x06005BD4 RID: 23508 RVA: 0x002F0087 File Offset: 0x002EE287
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedInteger((int)(this.targetForce / 10f), -10, 10);
		}

		// Token: 0x06005BD5 RID: 23509 RVA: 0x002F00A0 File Offset: 0x002EE2A0
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			if (this.correctionTimer > 0f)
			{
				base.StartDelayedCorrection(msg.ExtractBits(21), sendingTime, false);
				return;
			}
			this.targetForce = (float)msg.ReadRangedInteger(-10, 10) * 10f;
			ushort userID = msg.ReadUInt16();
			if (userID != 0)
			{
				this.User = (Entity.FindEntityByID(userID) as Character);
			}
		}

		// Token: 0x1700172A RID: 5930
		// (get) Token: 0x06005BD6 RID: 23510 RVA: 0x002F00FD File Offset: 0x002EE2FD
		// (set) Token: 0x06005BD7 RID: 23511 RVA: 0x002F0105 File Offset: 0x002EE305
		[Editable(0f, 10000000f, 1)]
		[Serialize(500f, IsPropertySaveable.Yes, "The amount of force exerted on the submarine when the engine is operating at full power.", "", false)]
		public float MaxForce
		{
			get
			{
				return this.maxForce;
			}
			set
			{
				this.maxForce = Math.Max(0f, value);
			}
		}

		// Token: 0x1700172B RID: 5931
		// (get) Token: 0x06005BD8 RID: 23512 RVA: 0x002F0118 File Offset: 0x002EE318
		// (set) Token: 0x06005BD9 RID: 23513 RVA: 0x002F0120 File Offset: 0x002EE320
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.Yes, "The position of the propeller as an offset from the item's center (in pixels). Determines where the particles spawn and the position that causes characters to take damage from the engine if the PropellerDamage is defined.", "", false)]
		public Vector2 PropellerPos { get; set; }

		// Token: 0x1700172C RID: 5932
		// (get) Token: 0x06005BDA RID: 23514 RVA: 0x002F0129 File Offset: 0x002EE329
		// (set) Token: 0x06005BDB RID: 23515 RVA: 0x002F0131 File Offset: 0x002EE331
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DisablePropellerDamage { get; set; }

		// Token: 0x1700172D RID: 5933
		// (get) Token: 0x06005BDC RID: 23516 RVA: 0x002F013A File Offset: 0x002EE33A
		// (set) Token: 0x06005BDD RID: 23517 RVA: 0x002F0142 File Offset: 0x002EE342
		public float Force
		{
			get
			{
				return this.force;
			}
			set
			{
				this.force = MathHelper.Clamp(value, -100f, 100f);
			}
		}

		// Token: 0x1700172E RID: 5934
		// (get) Token: 0x06005BDE RID: 23518 RVA: 0x002F015A File Offset: 0x002EE35A
		public float CurrentVolume
		{
			get
			{
				return this.CurrentStress;
			}
		}

		// Token: 0x1700172F RID: 5935
		// (get) Token: 0x06005BDF RID: 23519 RVA: 0x002F0164 File Offset: 0x002EE364
		public float CurrentBrokenVolume
		{
			get
			{
				if (this.item.ConditionPercentage > 10f)
				{
					return 0f;
				}
				return Math.Abs(this.targetForce / 100f) * (1f - this.item.ConditionPercentage / 10f);
			}
		}

		// Token: 0x17001730 RID: 5936
		// (get) Token: 0x06005BE0 RID: 23520 RVA: 0x002F01B2 File Offset: 0x002EE3B2
		public float CurrentStress
		{
			get
			{
				return Math.Abs(this.force / 100f * ((base.MinVoltage <= 0f) ? 1f : Math.Min(this.prevVoltage, 1f)));
			}
		}

		// Token: 0x06005BE1 RID: 23521 RVA: 0x002F01EC File Offset: 0x002EE3EC
		public Engine(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "propellerdamage")
				{
					this.propellerDamage = new Attack(subElement, item.Name + ", Engine");
				}
			}
			this.InitProjSpecific(element);
		}

		// Token: 0x06005BE2 RID: 23522 RVA: 0x002F028C File Offset: 0x002EE48C
		private void InitProjSpecific(ContentXElement element)
		{
			GUIFrame paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.85f, 0.65f), base.GuiFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0f, 0.04f)
			}, null, null);
			GUIFrame lightsArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.38f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			this.powerIndicator = new GUITickBox(new RectTransform(new Vector2(0.45f, 0.8f), lightsArea.RectTransform, Anchor.Center, new Pivot?(Pivot.CenterRight), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(-0.05f, 0f)
			}, TextManager.Get("EnginePowered"), GUIStyle.SubHeadingFont, "IndicatorLightGreen")
			{
				CanBeFocused = false
			};
			this.autoControlIndicator = new GUITickBox(new RectTransform(new Vector2(0.45f, 0.8f), lightsArea.RectTransform, Anchor.Center, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.05f, 0f)
			}, TextManager.Get(new string[]
			{
				"PumpAutoControl",
				"ReactorAutoControl"
			}), GUIStyle.SubHeadingFont, "IndicatorLightYellow")
			{
				Selected = false,
				Enabled = false,
				ToolTip = TextManager.Get("AutoControlTip")
			};
			this.powerIndicator.TextBlock.Wrap = (this.autoControlIndicator.TextBlock.Wrap = true);
			this.powerIndicator.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			this.autoControlIndicator.TextBlock.OverrideTextColor(GUIStyle.TextColorNormal);
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				this.powerIndicator.TextBlock,
				this.autoControlIndicator.TextBlock
			});
			GUIFrame sliderArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.6f), paddedFrame.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), null, null);
			LocalizedString powerLabel = TextManager.Get("EngineForce");
			GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.3f), sliderArea.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), "", new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.Center, false, "", null);
			guitextBlock.AutoScaleHorizontal = true;
			guitextBlock.TextGetter = (() => TextManager.AddPunctuation(':', new LocalizedString[]
			{
				powerLabel,
				TextManager.GetWithVariable("percentageformat", "[value]", ((int)MathF.Round(this.targetForce)).ToString(), FormatCapitals.No)
			}));
			this.forceSlider = new GUIScrollBar(new RectTransform(new Vector2(0.95f, 0.45f), sliderArea.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), 0.1f, null, "DeviceSlider", null)
			{
				Step = 0.05f,
				OnMoved = delegate(GUIScrollBar scrollBar, float barScroll)
				{
					this.lastReceivedTargetForce = null;
					float newTargetForce = barScroll * 200f - 100f;
					if ((double)Math.Abs(newTargetForce - this.targetForce) < 0.01)
					{
						return false;
					}
					this.targetForce = newTargetForce;
					this.User = Character.Controlled;
					if (GameMain.Client != null)
					{
						this.correctionTimer = 1f;
						this.item.CreateClientEvent<Engine>(this);
					}
					return true;
				}
			};
			GUIFrame textsArea = new GUIFrame(new RectTransform(new Vector2(1f, 0.25f), sliderArea.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal), null, null);
			GUITextBlock backwardsLabel = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1f), textsArea.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("EngineBackwards"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterLeft, false, "", null);
			GUITextBlock forwardsLabel = new GUITextBlock(new RectTransform(new Vector2(0.4f, 1f), textsArea.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), TextManager.Get("EngineForwards"), new Color?(GUIStyle.TextColorNormal), GUIStyle.SubHeadingFont, Alignment.CenterRight, false, "", null);
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				backwardsLabel,
				forwardsLabel
			});
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "propellersprite")
				{
					this.propellerSprite = new SpriteSheet(subElement, "", "");
					this.AnimSpeed = subElement.GetAttributeFloat("animspeed", 1f);
				}
			}
		}

		// Token: 0x06005BE3 RID: 23523 RVA: 0x002F0828 File Offset: 0x002EEA28
		public override void Update(float deltaTime, Camera cam)
		{
			base.UpdateOnActiveEffects(deltaTime);
			this.UpdateAnimation(deltaTime);
			this.controlLockTimer -= deltaTime;
			if (this.powerConsumption == 0f)
			{
				this.prevVoltage = 1f;
				this.hasPower = true;
			}
			else
			{
				this.hasPower = this.HasPower;
			}
			if (this.lastReceivedTargetForce != null)
			{
				this.targetForce = this.lastReceivedTargetForce.Value;
			}
			this.Force = MathHelper.Lerp(this.force, (base.Voltage < base.MinVoltage) ? 0f : this.targetForce, deltaTime * 10f);
			if (Math.Abs(this.Force) > 1f)
			{
				float voltageFactor = (base.MinVoltage <= 0f) ? 1f : Math.Min(base.Voltage, 2f);
				float currForce = this.force * MathF.Pow(voltageFactor, 0.6666667f);
				float condition = (this.item.MaxCondition <= 0f) ? 0f : (this.item.Condition / this.item.MaxCondition);
				float noise = Math.Abs(currForce) * MathHelper.Lerp(1.5f, 1f, condition);
				this.UpdateAITargets(noise);
				float forceMultiplier = 0.1f;
				if (this.User != null)
				{
					forceMultiplier *= MathHelper.Lerp(0.5f, 2f, (float)Math.Sqrt((double)(this.User.GetSkillLevel(Tags.HelmSkill) / 100f)));
				}
				currForce *= this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.EngineMaxSpeed, this.MaxForce) * forceMultiplier;
				Repairable repairable = this.item.GetComponent<Repairable>();
				if (repairable != null && repairable.IsTinkering)
				{
					currForce *= 1f + repairable.TinkeringStrength * 1.5f;
				}
				currForce = this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.EngineSpeed, currForce);
				currForce *= MathHelper.Lerp(0.5f, 2f, condition);
				if (this.item.Submarine.FlippedX)
				{
					currForce *= -1f;
				}
				Vector2 forceVector = new Vector2(currForce, 0f);
				this.item.Submarine.ApplyForce(forceVector * deltaTime * 60f);
				this.UpdatePropellerDamage(deltaTime);
				float particleInterval = 1f / (float)this.particlesPerSec;
				this.particleTimer += deltaTime;
				while (this.particleTimer > particleInterval)
				{
					Vector2 particleVel = -forceVector.ClampLength(5000f) / 5f;
					GameMain.ParticleManager.CreateParticle("bubbles", this.item.WorldPosition + this.PropellerPos * this.item.Scale, particleVel * Rand.Range(0.8f, 1.1f, Rand.RandSync.Unsynced), 0f, this.item.CurrentHull, 0f, null);
					this.particleTimer -= particleInterval;
				}
			}
		}

		// Token: 0x06005BE4 RID: 23524 RVA: 0x002F0B30 File Offset: 0x002EED30
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			this.currPowerConsumption = MathF.Pow(Math.Abs(this.targetForce) / 100f, 1.5f) * this.powerConsumption;
			Repairable component = this.item.GetComponent<Repairable>();
			if (component != null)
			{
				component.AdjustPowerConsumption(ref this.currPowerConsumption);
			}
			return this.currPowerConsumption;
		}

		// Token: 0x06005BE5 RID: 23525 RVA: 0x002F0B9E File Offset: 0x002EED9E
		public override void GridResolved(Connection connection)
		{
			if (connection == this.powerIn)
			{
				this.prevVoltage = base.Voltage;
			}
		}

		// Token: 0x06005BE6 RID: 23526 RVA: 0x002F0BB8 File Offset: 0x002EEDB8
		private void UpdateAITargets(float noise)
		{
			if (this.item.AiTarget != null)
			{
				this.item.AiTarget.SoundRange = MathHelper.Lerp(this.item.AiTarget.MinSoundRange, this.item.AiTarget.MaxSoundRange, noise / 100f);
				if (this.item.CurrentHull != null && this.item.CurrentHull.AiTarget != null)
				{
					this.item.CurrentHull.AiTarget.SoundRange = Math.Max(this.item.CurrentHull.AiTarget.SoundRange, this.item.AiTarget.SoundRange);
				}
			}
		}

		// Token: 0x06005BE7 RID: 23527 RVA: 0x002F0C70 File Offset: 0x002EEE70
		private void UpdatePropellerDamage(float deltaTime)
		{
			if (this.DisablePropellerDamage)
			{
				return;
			}
			this.damageTimer += deltaTime;
			if (this.damageTimer < 0.5f)
			{
				return;
			}
			this.damageTimer = 0.1f;
			if (this.propellerDamage == null)
			{
				return;
			}
			float scaledDamageRange = this.propellerDamage.DamageRange * this.item.Scale;
			Vector2 propellerWorldPos = this.item.WorldPosition + this.PropellerPos * this.item.Scale;
			float broadRange = Math.Max(scaledDamageRange * 2f, 500f);
			foreach (Character character in Character.CharacterList)
			{
				if (character.Enabled && !character.Removed && Math.Abs(character.WorldPosition.X - propellerWorldPos.X) <= broadRange && Math.Abs(character.WorldPosition.Y - propellerWorldPos.Y) <= broadRange)
				{
					foreach (Limb limb in character.AnimController.Limbs)
					{
						if (!limb.IsSevered && limb.body.Enabled)
						{
							float distSqr = Vector2.DistanceSquared(limb.WorldPosition, propellerWorldPos);
							if (distSqr <= scaledDamageRange * scaledDamageRange)
							{
								character.LastDamageSource = this.item;
								this.propellerDamage.DoDamage(null, character, propellerWorldPos, 1f, true, null, null);
								break;
							}
						}
					}
				}
			}
		}

		// Token: 0x06005BE8 RID: 23528 RVA: 0x002F0E1C File Offset: 0x002EF01C
		private void UpdateAnimation(float deltaTime)
		{
			if (this.propellerSprite == null)
			{
				return;
			}
			this.spriteIndex += this.force / 100f * this.AnimSpeed * deltaTime;
			if (this.spriteIndex < 0f)
			{
				this.spriteIndex = (float)this.propellerSprite.FrameCount - Math.Abs(this.spriteIndex) % (float)this.propellerSprite.FrameCount;
				return;
			}
			this.spriteIndex %= (float)this.propellerSprite.FrameCount;
		}

		// Token: 0x06005BE9 RID: 23529 RVA: 0x002F0EA6 File Offset: 0x002EF0A6
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.force = MathHelper.Lerp(this.force, 0f, 0.1f);
		}

		// Token: 0x06005BEA RID: 23530 RVA: 0x002F0ECB File Offset: 0x002EF0CB
		public override void FlipX(bool relativeToSub)
		{
			this.PropellerPos = new Vector2(-this.PropellerPos.X, this.PropellerPos.Y);
		}

		// Token: 0x06005BEB RID: 23531 RVA: 0x002F0EEF File Offset: 0x002EF0EF
		public override void FlipY(bool relativeToSub)
		{
			this.PropellerPos = new Vector2(this.PropellerPos.X, -this.PropellerPos.Y);
		}

		// Token: 0x06005BEC RID: 23532 RVA: 0x002F0F14 File Offset: 0x002EF114
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			base.ReceiveSignal(signal, connection);
			float tempForce;
			if (connection.Name == "set_force" && float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out tempForce))
			{
				this.controlLockTimer = 0.1f;
				this.lastReceivedTargetForce = new float?(MathHelper.Clamp(tempForce, -100f, 100f));
				this.User = signal.sender;
			}
		}

		// Token: 0x06005BED RID: 23533 RVA: 0x002F0F88 File Offset: 0x002EF188
		public override XElement Save(XElement parentElement)
		{
			Vector2 prevPropellerPos = this.PropellerPos;
			if (this.item.FlippedX)
			{
				this.PropellerPos = new Vector2(-this.PropellerPos.X, this.PropellerPos.Y);
			}
			if (this.item.FlippedY)
			{
				this.PropellerPos = new Vector2(this.PropellerPos.X, -this.PropellerPos.Y);
			}
			XElement element = base.Save(parentElement);
			this.PropellerPos = prevPropellerPos;
			return element;
		}

		// Token: 0x04002EC2 RID: 11970
		private float spriteIndex;

		// Token: 0x04002EC3 RID: 11971
		private SpriteSheet propellerSprite;

		// Token: 0x04002EC4 RID: 11972
		private GUITickBox powerIndicator;

		// Token: 0x04002EC5 RID: 11973
		private GUIScrollBar forceSlider;

		// Token: 0x04002EC6 RID: 11974
		private GUITickBox autoControlIndicator;

		// Token: 0x04002EC7 RID: 11975
		private int particlesPerSec = 60;

		// Token: 0x04002EC8 RID: 11976
		private float particleTimer;

		// Token: 0x04002ECA RID: 11978
		private float force;

		// Token: 0x04002ECB RID: 11979
		private float? lastReceivedTargetForce;

		// Token: 0x04002ECC RID: 11980
		private float targetForce;

		// Token: 0x04002ECD RID: 11981
		private const float ForceToPowerExponent = 1.5f;

		// Token: 0x04002ECE RID: 11982
		private const float PowerToForceExponent = 0.6666667f;

		// Token: 0x04002ECF RID: 11983
		private float maxForce;

		// Token: 0x04002ED0 RID: 11984
		private readonly Attack propellerDamage;

		// Token: 0x04002ED1 RID: 11985
		private float damageTimer;

		// Token: 0x04002ED2 RID: 11986
		private bool hasPower;

		// Token: 0x04002ED3 RID: 11987
		private float prevVoltage;

		// Token: 0x04002ED4 RID: 11988
		private float controlLockTimer;

		// Token: 0x04002ED5 RID: 11989
		public Character User;

		// Token: 0x04002ED8 RID: 11992
		private const float TinkeringForceIncrease = 1.5f;
	}
}
