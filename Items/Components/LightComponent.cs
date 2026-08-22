using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Barotrauma.Lights;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005B8 RID: 1464
	internal class LightComponent : Powered, IServerSerializable, INetSerializable, IDrawableComponent
	{
		// Token: 0x170016ED RID: 5869
		// (get) Token: 0x06005B11 RID: 23313 RVA: 0x002EA886 File Offset: 0x002E8A86
		// (set) Token: 0x06005B12 RID: 23314 RVA: 0x002EA88E File Offset: 0x002E8A8E
		[Serialize(1f, IsPropertySaveable.Yes, "The scale of the light sprite.", "", false)]
		public float LightSpriteScale { get; set; }

		// Token: 0x170016EE RID: 5870
		// (get) Token: 0x06005B13 RID: 23315 RVA: 0x002EA897 File Offset: 0x002E8A97
		public Vector2 DrawSize
		{
			get
			{
				return new Vector2(this.Light.Range * 2f, this.Light.Range * 2f);
			}
		}

		// Token: 0x170016EF RID: 5871
		// (get) Token: 0x06005B14 RID: 23316 RVA: 0x002EA8C0 File Offset: 0x002E8AC0
		public LightSource Light { get; }

		// Token: 0x06005B15 RID: 23317 RVA: 0x002EA8C8 File Offset: 0x002E8AC8
		public override void OnScaleChanged()
		{
			this.Light.SpriteScale = Vector2.One * this.item.Scale;
			this.Light.Position = ((this.ParentBody != null) ? this.ParentBody.Position : this.item.Position);
			this.SetLightSourceTransformProjSpecific();
		}

		// Token: 0x06005B16 RID: 23318 RVA: 0x002EA928 File Offset: 0x002E8B28
		public void Draw(SpriteBatch spriteBatch, bool editing = false, float itemDepth = -1f, Color? overrideColor = null)
		{
			LightSource light = this.Light;
			if (((light != null) ? light.LightSprite : null) == null)
			{
				return;
			}
			if ((this.item.body == null || this.item.body.Enabled) && this.lightBrightness > 0f && this.IsOn && this.Light.Enabled)
			{
				Vector2 offset = this.LightOffset * this.item.Scale;
				if (this.item.FlippedX)
				{
					offset.X *= -1f;
				}
				if (this.item.FlippedY)
				{
					offset.Y *= -1f;
				}
				offset = Vector2.Transform(offset, Matrix.CreateRotationZ(-this.item.RotationRad));
				Vector2 origin = this.Light.LightSprite.Origin;
				if ((this.Light.LightSpriteEffect & SpriteEffects.FlipHorizontally) == SpriteEffects.FlipHorizontally)
				{
					origin.X = (float)this.Light.LightSprite.SourceRect.Width - origin.X;
				}
				if ((this.Light.LightSpriteEffect & SpriteEffects.FlipVertically) == SpriteEffects.FlipVertically)
				{
					origin.Y = (float)this.Light.LightSprite.SourceRect.Height - origin.Y;
				}
				PhysicsBody body = this.item.body;
				Vector2 drawPos = (body != null) ? body.DrawPosition : (this.item.DrawPosition + offset);
				Color color = this.lightColor;
				if (this.Light.OverrideLightSpriteAlpha != null)
				{
					color = new Color(this.lightColor, this.Light.OverrideLightSpriteAlpha.Value);
				}
				this.Light.LightSprite.Draw(spriteBatch, new Vector2(drawPos.X, -drawPos.Y), color * this.lightBrightness, origin, -this.Light.Rotation, this.item.Scale * this.LightSpriteScale, this.Light.LightSpriteEffect, new float?(itemDepth - 0.0001f));
			}
		}

		// Token: 0x06005B17 RID: 23319 RVA: 0x002EAB44 File Offset: 0x002E8D44
		public override void FlipX(bool relativeToSub)
		{
			LightSource light = this.Light;
			if (((light != null) ? light.LightSprite : null) != null && this.item.Prefab.CanSpriteFlipX)
			{
				this.Light.LightSpriteEffect ^= SpriteEffects.FlipHorizontally;
			}
			this.SetLightSourceTransformProjSpecific();
		}

		// Token: 0x06005B18 RID: 23320 RVA: 0x002EAB90 File Offset: 0x002E8D90
		public override void FlipY(bool relativeToSub)
		{
			LightSource light = this.Light;
			if (((light != null) ? light.LightSprite : null) != null && this.item.Prefab.CanSpriteFlipY)
			{
				this.Light.LightSpriteEffect ^= SpriteEffects.FlipVertically;
			}
			this.SetLightSourceTransformProjSpecific();
		}

		// Token: 0x06005B19 RID: 23321 RVA: 0x002EABDC File Offset: 0x002E8DDC
		private IEnumerable<CoroutineStatus> ResetPredictionAfterDelay()
		{
			LightComponent.<ResetPredictionAfterDelay>d__17 <ResetPredictionAfterDelay>d__ = new LightComponent.<ResetPredictionAfterDelay>d__17(-2);
			<ResetPredictionAfterDelay>d__.<>4__this = this;
			return <ResetPredictionAfterDelay>d__;
		}

		// Token: 0x06005B1A RID: 23322 RVA: 0x002EABEC File Offset: 0x002E8DEC
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.IsActive = msg.ReadBoolean();
			this.lastReceivedState = new bool?(this.IsActive);
		}

		// Token: 0x06005B1B RID: 23323 RVA: 0x002EAC0B File Offset: 0x002E8E0B
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.Light.Remove();
		}

		// Token: 0x170016F0 RID: 5872
		// (get) Token: 0x06005B1C RID: 23324 RVA: 0x002EAC1E File Offset: 0x002E8E1E
		// (set) Token: 0x06005B1D RID: 23325 RVA: 0x002EAC26 File Offset: 0x002E8E26
		[Serialize(100f, IsPropertySaveable.Yes, "The range of the emitted light. Higher values are more performance-intensive.", "", true)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 2048f)]
		public float Range
		{
			get
			{
				return this.range;
			}
			set
			{
				this.range = MathHelper.Clamp(value, 0f, 4096f);
				this.item.ResetCachedVisibleSize();
				if (this.Light != null)
				{
					this.Light.Range = this.range;
				}
			}
		}

		// Token: 0x170016F1 RID: 5873
		// (get) Token: 0x06005B1E RID: 23326 RVA: 0x002EAC62 File Offset: 0x002E8E62
		// (set) Token: 0x06005B1F RID: 23327 RVA: 0x002EAC6A File Offset: 0x002E8E6A
		public float Rotation
		{
			get
			{
				return this.rotation;
			}
			set
			{
				this.rotation = value;
				this.SetLightSourceTransformProjSpecific();
			}
		}

		// Token: 0x170016F2 RID: 5874
		// (get) Token: 0x06005B20 RID: 23328 RVA: 0x002EAC79 File Offset: 0x002E8E79
		// (set) Token: 0x06005B21 RID: 23329 RVA: 0x002EAC81 File Offset: 0x002E8E81
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Should structures cast shadows when light from this light source hits them. Disabling shadows increases the performance of the game, and is recommended for lights with a short range. Lights that are set to be drawn behind subs don't cast shadows, regardless of this setting.", "", true)]
		public bool CastShadows
		{
			get
			{
				return this.castShadows;
			}
			set
			{
				this.castShadows = value;
				if (this.Light != null)
				{
					this.Light.CastShadows = value;
				}
			}
		}

		// Token: 0x170016F3 RID: 5875
		// (get) Token: 0x06005B22 RID: 23330 RVA: 0x002EAC9E File Offset: 0x002E8E9E
		// (set) Token: 0x06005B23 RID: 23331 RVA: 0x002EACA6 File Offset: 0x002E8EA6
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Lights drawn behind submarines don't cast any shadows and are much faster to draw than shadow-casting lights. It's recommended to enable this on decorative lights outside the submarine's hull.", "", true)]
		public bool DrawBehindSubs
		{
			get
			{
				return this.drawBehindSubs;
			}
			set
			{
				this.drawBehindSubs = value;
				if (this.Light != null)
				{
					this.Light.IsBackground = this.drawBehindSubs;
				}
			}
		}

		// Token: 0x170016F4 RID: 5876
		// (get) Token: 0x06005B24 RID: 23332 RVA: 0x002EACC8 File Offset: 0x002E8EC8
		// (set) Token: 0x06005B25 RID: 23333 RVA: 0x002EACD0 File Offset: 0x002E8ED0
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Is the light currently on.", "", true)]
		public bool IsOn
		{
			get
			{
				return this.isOn;
			}
			set
			{
				if (this.isOn == value && this.IsActive == value)
				{
					return;
				}
				this.isOn = value;
				this.IsActive = value;
				bool isLightOn = this.isOn && this.item.Condition > 0f;
				this.SetLightSourceState(isLightOn, isLightOn ? this.lightBrightness : 0f);
				this.OnStateChanged();
			}
		}

		// Token: 0x170016F5 RID: 5877
		// (get) Token: 0x06005B26 RID: 23334 RVA: 0x002EAD3B File Offset: 0x002E8F3B
		// (set) Token: 0x06005B27 RID: 23335 RVA: 0x002EAD43 File Offset: 0x002E8F43
		[Editable]
		[Serialize(0f, IsPropertySaveable.No, "How heavily the light flickers. 0 = no flickering, 1 = the light will alternate between completely dark and full brightness.", "", false)]
		public float Flicker
		{
			get
			{
				return this.flicker;
			}
			set
			{
				this.flicker = MathHelper.Clamp(value, 0f, 1f);
				if (this.Light != null)
				{
					this.Light.LightSourceParams.Flicker = this.flicker;
				}
			}
		}

		// Token: 0x170016F6 RID: 5878
		// (get) Token: 0x06005B28 RID: 23336 RVA: 0x002EAD79 File Offset: 0x002E8F79
		// (set) Token: 0x06005B29 RID: 23337 RVA: 0x002EAD81 File Offset: 0x002E8F81
		[Editable]
		[Serialize(1f, IsPropertySaveable.No, "How fast the light flickers.", "", false)]
		public float FlickerSpeed
		{
			get
			{
				return this.flickerSpeed;
			}
			set
			{
				this.flickerSpeed = value;
				if (this.Light != null)
				{
					this.Light.LightSourceParams.FlickerSpeed = this.flickerSpeed;
				}
			}
		}

		// Token: 0x170016F7 RID: 5879
		// (get) Token: 0x06005B2A RID: 23338 RVA: 0x002EADA8 File Offset: 0x002E8FA8
		// (set) Token: 0x06005B2B RID: 23339 RVA: 0x002EADB0 File Offset: 0x002E8FB0
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "How rapidly the light pulsates (in Hz). 0 = no blinking.", "", false)]
		public float PulseFrequency
		{
			get
			{
				return this.pulseFrequency;
			}
			set
			{
				this.pulseFrequency = MathHelper.Clamp(value, 0f, 60f);
				if (this.Light != null)
				{
					this.Light.LightSourceParams.PulseFrequency = this.pulseFrequency;
				}
			}
		}

		// Token: 0x170016F8 RID: 5880
		// (get) Token: 0x06005B2C RID: 23340 RVA: 0x002EADE6 File Offset: 0x002E8FE6
		// (set) Token: 0x06005B2D RID: 23341 RVA: 0x002EADEE File Offset: 0x002E8FEE
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		[Serialize(0f, IsPropertySaveable.Yes, "How much light pulsates. 0 = not at all, 1 = alternates between full brightness and off.", "", false)]
		public float PulseAmount
		{
			get
			{
				return this.pulseAmount;
			}
			set
			{
				this.pulseAmount = MathHelper.Clamp(value, 0f, 1f);
				if (this.Light != null)
				{
					this.Light.LightSourceParams.PulseAmount = this.pulseAmount;
				}
			}
		}

		// Token: 0x170016F9 RID: 5881
		// (get) Token: 0x06005B2E RID: 23342 RVA: 0x002EAE24 File Offset: 0x002E9024
		// (set) Token: 0x06005B2F RID: 23343 RVA: 0x002EAE2C File Offset: 0x002E902C
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "How rapidly the light blinks on and off (in Hz). 0 = no blinking.", "", false)]
		public float BlinkFrequency
		{
			get
			{
				return this.blinkFrequency;
			}
			set
			{
				this.blinkFrequency = MathHelper.Clamp(value, 0f, 60f);
				if (this.Light != null)
				{
					this.Light.LightSourceParams.BlinkFrequency = this.blinkFrequency;
				}
			}
		}

		// Token: 0x170016FA RID: 5882
		// (get) Token: 0x06005B30 RID: 23344 RVA: 0x002EAE62 File Offset: 0x002E9062
		// (set) Token: 0x06005B31 RID: 23345 RVA: 0x002EAE6C File Offset: 0x002E906C
		[InGameEditable(FallBackTextTag = "connection.setcolor")]
		[Serialize("255,255,255,255", IsPropertySaveable.Yes, "The color of the emitted light (R,G,B,A).", "", true)]
		public Color LightColor
		{
			get
			{
				return this.lightColor;
			}
			set
			{
				this.lightColor = value;
				this.prevColorSignal = string.Empty;
				if (this.Light != null)
				{
					this.Light.Color = (this.IsOn ? this.lightColor.Multiply(this.lightColorMultiplier, false) : Color.Transparent);
				}
			}
		}

		// Token: 0x170016FB RID: 5883
		// (get) Token: 0x06005B32 RID: 23346 RVA: 0x002EAEBF File Offset: 0x002E90BF
		// (set) Token: 0x06005B33 RID: 23347 RVA: 0x002EAEC7 File Offset: 0x002E90C7
		[Serialize(false, IsPropertySaveable.No, "If enabled, the component will ignore continuous signals received in the toggle input (i.e. a continuous signal will only toggle it once).", "", false)]
		public bool IgnoreContinuousToggle { get; set; }

		// Token: 0x170016FC RID: 5884
		// (get) Token: 0x06005B34 RID: 23348 RVA: 0x002EAED0 File Offset: 0x002E90D0
		// (set) Token: 0x06005B35 RID: 23349 RVA: 0x002EAED8 File Offset: 0x002E90D8
		[Serialize(true, IsPropertySaveable.No, "Should the light sprite be drawn on the item using alpha blending, in addition to being rendered in the light map? Can be used to make the light sprite stand out more.", "", false)]
		public bool AlphaBlend { get; set; }

		// Token: 0x170016FD RID: 5885
		// (get) Token: 0x06005B36 RID: 23350 RVA: 0x002EAEE1 File Offset: 0x002E90E1
		// (set) Token: 0x06005B37 RID: 23351 RVA: 0x002EAEE9 File Offset: 0x002E90E9
		[Serialize("0,0", IsPropertySaveable.No, "Offset of the light from the position of the item (in pixels).", "", false)]
		public Vector2 LightOffset { get; set; }

		// Token: 0x170016FE RID: 5886
		// (get) Token: 0x06005B38 RID: 23352 RVA: 0x002EAEF2 File Offset: 0x002E90F2
		public bool IsRed
		{
			get
			{
				return ColorExtensions.IsRedDominant(this.LightColor, 2f, 0);
			}
		}

		// Token: 0x170016FF RID: 5887
		// (get) Token: 0x06005B39 RID: 23353 RVA: 0x002EAF05 File Offset: 0x002E9105
		public bool IsGreen
		{
			get
			{
				return ColorExtensions.IsGreenDominant(this.LightColor, 2f, 0);
			}
		}

		// Token: 0x17001700 RID: 5888
		// (get) Token: 0x06005B3A RID: 23354 RVA: 0x002EAF18 File Offset: 0x002E9118
		public bool IsBlue
		{
			get
			{
				return ColorExtensions.IsBlueDominant(this.LightColor, 2f, 0);
			}
		}

		// Token: 0x06005B3B RID: 23355 RVA: 0x002EAF2B File Offset: 0x002E912B
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			this.Light.Position += amount;
		}

		// Token: 0x17001701 RID: 5889
		// (get) Token: 0x06005B3C RID: 23356 RVA: 0x002EAF44 File Offset: 0x002E9144
		// (set) Token: 0x06005B3D RID: 23357 RVA: 0x002EAF4C File Offset: 0x002E914C
		public override bool IsActive
		{
			get
			{
				return base.IsActive;
			}
			set
			{
				if (base.IsActive == value)
				{
					return;
				}
				this.isOn = value;
				base.IsActive = value;
				this.SetLightSourceState(value, value ? this.lightBrightness : 0f);
			}
		}

		// Token: 0x06005B3E RID: 23358 RVA: 0x002EAF8C File Offset: 0x002E918C
		public LightComponent(Item item, ContentXElement element) : base(item, element)
		{
			LightSource lightSource = new LightSource(element, null);
			Hull currentHull = item.CurrentHull;
			lightSource.ParentSub = ((currentHull != null) ? currentHull.Submarine : null);
			lightSource.Position = item.Position;
			lightSource.CastShadows = this.castShadows;
			lightSource.IsBackground = this.drawBehindSubs;
			lightSource.SpriteScale = Vector2.One * item.Scale * this.LightSpriteScale;
			lightSource.Range = this.range;
			this.Light = lightSource;
			this.Light.LightSourceParams.Flicker = this.flicker;
			this.Light.LightSourceParams.FlickerSpeed = this.FlickerSpeed;
			this.Light.LightSourceParams.PulseAmount = this.pulseAmount;
			this.Light.LightSourceParams.PulseFrequency = this.pulseFrequency;
			this.Light.LightSourceParams.BlinkFrequency = this.blinkFrequency;
			this.IsActive = this.IsOn;
		}

		// Token: 0x06005B3F RID: 23359 RVA: 0x002EB094 File Offset: 0x002E9294
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.SetLightSourceState(this.IsActive, this.lightBrightness);
			this.turret = this.item.GetComponent<Turret>();
			if (this.item.body != null)
			{
				Body farseerBody = this.item.body.FarseerBody;
				farseerBody.OnEnabled = (Action)Delegate.Combine(farseerBody.OnEnabled, new Action(this.CheckIfNeedsUpdate));
				Body farseerBody2 = this.item.body.FarseerBody;
				farseerBody2.OnDisabled = (Action)Delegate.Combine(farseerBody2.OnDisabled, new Action(this.CheckIfNeedsUpdate));
			}
			base.Drawable = (this.AlphaBlend && this.Light.LightSprite != null);
			if (Screen.Selected.IsEditor)
			{
				this.OnMapLoaded();
			}
		}

		// Token: 0x06005B40 RID: 23360 RVA: 0x002EB16A File Offset: 0x002E936A
		public override void OnMapLoaded()
		{
			if (this.item.IsHidden)
			{
				this.Light.Enabled = false;
			}
			this.CheckIfNeedsUpdate();
			this.SetLightSourceTransformProjSpecific();
		}

		// Token: 0x06005B41 RID: 23361 RVA: 0x002EB194 File Offset: 0x002E9394
		public void CheckIfNeedsUpdate()
		{
			if (!this.IsOn)
			{
				base.IsActive = false;
				return;
			}
			if ((this.item.body == null || !this.item.body.Enabled) && this.powerConsumption <= 0f && base.Parent == null && this.turret == null && (this.statusEffectLists == null || !this.statusEffectLists.ContainsKey(ActionType.OnActive)) && (this.IsActiveConditionals == null || this.IsActiveConditionals.Count == 0))
			{
				PhysicsBody body = this.ParentBody ?? this.item.body;
				if ((body == null || !body.Enabled) && !this.IsVisibleInInventory())
				{
					this.lightBrightness = 0f;
					this.SetLightSourceState(false, 0f);
				}
				else
				{
					this.lightBrightness = 1f;
					this.SetLightSourceState(true, this.lightBrightness);
				}
				this.isOn = true;
				this.SetLightSourceTransformProjSpecific();
				base.IsActive = false;
				this.Light.ParentSub = this.item.Submarine;
				return;
			}
			base.IsActive = true;
		}

		// Token: 0x06005B42 RID: 23362 RVA: 0x002EB2B8 File Offset: 0x002E94B8
		private bool IsVisibleInInventory()
		{
			Character ownerCharacter = this.item.GetRootInventoryOwner() as Character;
			if (ownerCharacter != null)
			{
				Item rootContainer = this.item.RootContainer;
				Holdable holdable = (rootContainer != null) ? rootContainer.GetComponent<Holdable>() : null;
				if (holdable == null || !holdable.IsActive)
				{
					return false;
				}
			}
			return this.item.FindParentInventory(delegate(Inventory it)
			{
				ItemInventory itemInventory = it as ItemInventory;
				if (itemInventory != null)
				{
					ItemContainer container = itemInventory.Container;
					if (container != null)
					{
						return container.HideItems;
					}
				}
				return false;
			}) == null;
		}

		// Token: 0x06005B43 RID: 23363 RVA: 0x002EB32C File Offset: 0x002E952C
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.item.AiTarget != null)
			{
				this.UpdateAITarget(this.item.AiTarget);
			}
			base.UpdateOnActiveEffects(deltaTime);
			if (!this.IsActive)
			{
				return;
			}
			this.Light.ParentSub = this.item.Submarine;
			bool isVisibleInInventory = this.IsVisibleInInventory();
			Character ownerCharacter = this.item.GetRootInventoryOwner() as Character;
			if ((this.item.Container != null && !isVisibleInInventory && ownerCharacter == null) || (ownerCharacter != null && ownerCharacter.InvisibleTimer > 0f))
			{
				this.lightBrightness = 0f;
				this.SetLightSourceState(false, 0f);
				return;
			}
			this.SetLightSourceTransformProjSpecific();
			PhysicsBody body = this.ParentBody ?? this.item.body;
			if ((body == null || !body.Enabled) && !isVisibleInInventory)
			{
				this.lightBrightness = 0f;
				this.SetLightSourceState(false, 0f);
				return;
			}
			this.TemporaryFlickerTimer -= deltaTime;
			if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f && (base.Voltage < Rand.Range(0f, base.MinVoltage, Rand.RandSync.Unsynced) || this.TemporaryFlickerTimer > 0f))
			{
				if (base.Voltage > 0.1f)
				{
					string soundTag = "zap";
					Vector2 worldPosition = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					SoundPlayer.PlaySound(soundTag, worldPosition, null, null, currentHull);
				}
				this.lightBrightness = 0f;
			}
			else
			{
				this.lightBrightness = MathHelper.Lerp(this.lightBrightness, (this.powerConsumption <= 0f) ? 1f : Math.Min(base.Voltage, 1f), 0.1f);
			}
			this.SetLightSourceState(true, this.lightBrightness);
		}

		// Token: 0x06005B44 RID: 23364 RVA: 0x002EB4F5 File Offset: 0x002E96F5
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.SetLightSourceState(false, 0f);
		}

		// Token: 0x06005B45 RID: 23365 RVA: 0x002EB503 File Offset: 0x002E9703
		public override bool Use(float deltaTime, Character character = null)
		{
			return true;
		}

		// Token: 0x06005B46 RID: 23366 RVA: 0x002EB508 File Offset: 0x002E9708
		private void OnStateChanged()
		{
			if (GameMain.Client == null || this.lastReceivedState == null)
			{
				return;
			}
			this.resetPredictionTimer = 1f;
			if (this.resetPredictionCoroutine == null || !CoroutineManager.IsCoroutineRunning(this.resetPredictionCoroutine))
			{
				this.resetPredictionCoroutine = CoroutineManager.StartCoroutine(this.ResetPredictionAfterDelay(), "");
			}
		}

		// Token: 0x06005B47 RID: 23367 RVA: 0x002EB560 File Offset: 0x002E9760
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "toggle"))
			{
				if (name == "set_state")
				{
					this.IsOn = (signal.value != "0");
					return;
				}
				if (!(name == "set_color"))
				{
					return;
				}
				if (signal.value != this.prevColorSignal)
				{
					this.LightColor = XMLExtensions.ParseColor(signal.value, false);
					this.SetLightSourceState(this.Light.Enabled, this.lightColorMultiplier);
					this.prevColorSignal = signal.value;
				}
			}
			else if (signal.value != "0")
			{
				if (!this.IgnoreContinuousToggle || this.lastToggleSignalTime < Timing.TotalTime - 0.1)
				{
					this.IsOn = !this.IsOn;
				}
				this.lastToggleSignalTime = Timing.TotalTime;
				return;
			}
		}

		// Token: 0x06005B48 RID: 23368 RVA: 0x002EB64C File Offset: 0x002E984C
		private void UpdateAITarget(AITarget target)
		{
			if (!this.IsActive)
			{
				return;
			}
			if (target.MaxSightRange <= 0f)
			{
				target.MaxSightRange = this.Range * 5f;
			}
			target.SightRange = Math.Max(target.SightRange, target.MaxSightRange * this.lightBrightness);
		}

		// Token: 0x06005B49 RID: 23369 RVA: 0x002EB69F File Offset: 0x002E989F
		public override void Drop(Character dropper, bool setTransform = true)
		{
			this.SetLightSourceTransform();
		}

		// Token: 0x06005B4A RID: 23370 RVA: 0x002EB6A8 File Offset: 0x002E98A8
		private void SetLightSourceState(bool enabled, float brightness)
		{
			if (this.Light == null)
			{
				return;
			}
			if (this.item.IsHidden)
			{
				enabled = false;
			}
			this.Light.Enabled = enabled;
			this.lightColorMultiplier = brightness;
			if (enabled)
			{
				this.Light.Color = this.LightColor.Multiply(this.lightColorMultiplier, false);
			}
		}

		// Token: 0x06005B4B RID: 23371 RVA: 0x002EB701 File Offset: 0x002E9901
		public void SetLightSourceTransform()
		{
			this.SetLightSourceTransformProjSpecific();
		}

		// Token: 0x06005B4C RID: 23372 RVA: 0x002EB70C File Offset: 0x002E990C
		private void SetLightSourceTransformProjSpecific()
		{
			Vector2 offset = this.LightOffset * this.item.Scale;
			if (offset != Vector2.Zero)
			{
				if (this.item.FlippedX)
				{
					offset.X *= -1f;
				}
				if (this.item.FlippedY)
				{
					offset.Y *= -1f;
				}
				offset = Vector2.Transform(offset, Matrix.CreateRotationZ(-this.item.RotationRad));
			}
			if (this.ParentBody != null)
			{
				this.Light.ParentBody = this.ParentBody;
				this.Light.OffsetFromBody = offset;
			}
			else if (this.turret != null)
			{
				this.Light.Position = new Vector2((float)this.item.Rect.X + this.turret.TransformedBarrelPos.X, (float)this.item.Rect.Y - this.turret.TransformedBarrelPos.Y) + offset;
			}
			else if (this.item.body != null)
			{
				this.Light.ParentBody = this.item.body;
				this.Light.OffsetFromBody = offset;
			}
			else
			{
				this.Light.Position = this.item.Position + offset;
			}
			PhysicsBody body = this.Light.ParentBody;
			if (body == null)
			{
				this.Light.Rotation = -this.Rotation - this.item.RotationRad;
				this.Light.LightSpriteEffect = this.item.SpriteEffects;
				return;
			}
			this.Light.Rotation = ((body.Dir > 0f) ? body.DrawRotation : (body.DrawRotation - 3.1415927f));
			if (body.Enabled)
			{
				this.Light.LightSpriteEffect = ((body.Dir > 0f) ? SpriteEffects.None : SpriteEffects.FlipVertically);
				return;
			}
			this.Light.LightSpriteEffect = this.item.SpriteEffects;
		}

		// Token: 0x04002E6A RID: 11882
		private bool? lastReceivedState;

		// Token: 0x04002E6B RID: 11883
		private CoroutineHandle resetPredictionCoroutine;

		// Token: 0x04002E6C RID: 11884
		private float resetPredictionTimer;

		// Token: 0x04002E6D RID: 11885
		private float lightColorMultiplier;

		// Token: 0x04002E70 RID: 11888
		private Color lightColor;

		// Token: 0x04002E71 RID: 11889
		private float lightBrightness;

		// Token: 0x04002E72 RID: 11890
		private float blinkFrequency;

		// Token: 0x04002E73 RID: 11891
		private float pulseFrequency;

		// Token: 0x04002E74 RID: 11892
		private float pulseAmount;

		// Token: 0x04002E75 RID: 11893
		private float range;

		// Token: 0x04002E76 RID: 11894
		private float flicker;

		// Token: 0x04002E77 RID: 11895
		private float flickerSpeed;

		// Token: 0x04002E78 RID: 11896
		private bool castShadows;

		// Token: 0x04002E79 RID: 11897
		private bool drawBehindSubs;

		// Token: 0x04002E7A RID: 11898
		private double lastToggleSignalTime;

		// Token: 0x04002E7B RID: 11899
		private string prevColorSignal;

		// Token: 0x04002E7C RID: 11900
		public PhysicsBody ParentBody;

		// Token: 0x04002E7D RID: 11901
		private bool isOn;

		// Token: 0x04002E7E RID: 11902
		private Turret turret;

		// Token: 0x04002E7F RID: 11903
		private float rotation;

		// Token: 0x04002E83 RID: 11907
		public float TemporaryFlickerTimer;
	}
}
