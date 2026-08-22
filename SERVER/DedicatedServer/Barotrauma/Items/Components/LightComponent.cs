using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x02000499 RID: 1177
	internal class LightComponent : Powered, IServerSerializable, INetSerializable, IDrawableComponent
	{
		// Token: 0x0600402A RID: 16426 RVA: 0x0019B7E7 File Offset: 0x001999E7
		private IEnumerable<CoroutineStatus> SendStateAfterDelay()
		{
			LightComponent.<SendStateAfterDelay>d__3 <SendStateAfterDelay>d__ = new LightComponent.<SendStateAfterDelay>d__3(-2);
			<SendStateAfterDelay>d__.<>4__this = this;
			return <SendStateAfterDelay>d__;
		}

		// Token: 0x0600402B RID: 16427 RVA: 0x0019B7F7 File Offset: 0x001999F7
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.IsActive);
			this.lastSentState = this.IsActive;
		}

		// Token: 0x17001100 RID: 4352
		// (get) Token: 0x0600402C RID: 16428 RVA: 0x0019B811 File Offset: 0x00199A11
		// (set) Token: 0x0600402D RID: 16429 RVA: 0x0019B819 File Offset: 0x00199A19
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
			}
		}

		// Token: 0x17001101 RID: 4353
		// (get) Token: 0x0600402E RID: 16430 RVA: 0x0019B831 File Offset: 0x00199A31
		// (set) Token: 0x0600402F RID: 16431 RVA: 0x0019B839 File Offset: 0x00199A39
		public float Rotation
		{
			get
			{
				return this.rotation;
			}
			set
			{
				this.rotation = value;
			}
		}

		// Token: 0x17001102 RID: 4354
		// (get) Token: 0x06004030 RID: 16432 RVA: 0x0019B842 File Offset: 0x00199A42
		// (set) Token: 0x06004031 RID: 16433 RVA: 0x0019B84A File Offset: 0x00199A4A
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
			}
		}

		// Token: 0x17001103 RID: 4355
		// (get) Token: 0x06004032 RID: 16434 RVA: 0x0019B853 File Offset: 0x00199A53
		// (set) Token: 0x06004033 RID: 16435 RVA: 0x0019B85B File Offset: 0x00199A5B
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
			}
		}

		// Token: 0x17001104 RID: 4356
		// (get) Token: 0x06004034 RID: 16436 RVA: 0x0019B864 File Offset: 0x00199A64
		// (set) Token: 0x06004035 RID: 16437 RVA: 0x0019B86C File Offset: 0x00199A6C
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
				bool flag = this.isOn && this.item.Condition > 0f;
				this.OnStateChanged();
			}
		}

		// Token: 0x17001105 RID: 4357
		// (get) Token: 0x06004036 RID: 16438 RVA: 0x0019B8C0 File Offset: 0x00199AC0
		// (set) Token: 0x06004037 RID: 16439 RVA: 0x0019B8C8 File Offset: 0x00199AC8
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
			}
		}

		// Token: 0x17001106 RID: 4358
		// (get) Token: 0x06004038 RID: 16440 RVA: 0x0019B8E0 File Offset: 0x00199AE0
		// (set) Token: 0x06004039 RID: 16441 RVA: 0x0019B8E8 File Offset: 0x00199AE8
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
			}
		}

		// Token: 0x17001107 RID: 4359
		// (get) Token: 0x0600403A RID: 16442 RVA: 0x0019B8F1 File Offset: 0x00199AF1
		// (set) Token: 0x0600403B RID: 16443 RVA: 0x0019B8F9 File Offset: 0x00199AF9
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
			}
		}

		// Token: 0x17001108 RID: 4360
		// (get) Token: 0x0600403C RID: 16444 RVA: 0x0019B911 File Offset: 0x00199B11
		// (set) Token: 0x0600403D RID: 16445 RVA: 0x0019B919 File Offset: 0x00199B19
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
			}
		}

		// Token: 0x17001109 RID: 4361
		// (get) Token: 0x0600403E RID: 16446 RVA: 0x0019B931 File Offset: 0x00199B31
		// (set) Token: 0x0600403F RID: 16447 RVA: 0x0019B939 File Offset: 0x00199B39
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
			}
		}

		// Token: 0x1700110A RID: 4362
		// (get) Token: 0x06004040 RID: 16448 RVA: 0x0019B951 File Offset: 0x00199B51
		// (set) Token: 0x06004041 RID: 16449 RVA: 0x0019B959 File Offset: 0x00199B59
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
			}
		}

		// Token: 0x1700110B RID: 4363
		// (get) Token: 0x06004042 RID: 16450 RVA: 0x0019B96D File Offset: 0x00199B6D
		// (set) Token: 0x06004043 RID: 16451 RVA: 0x0019B975 File Offset: 0x00199B75
		[Serialize(false, IsPropertySaveable.No, "If enabled, the component will ignore continuous signals received in the toggle input (i.e. a continuous signal will only toggle it once).", "", false)]
		public bool IgnoreContinuousToggle { get; set; }

		// Token: 0x1700110C RID: 4364
		// (get) Token: 0x06004044 RID: 16452 RVA: 0x0019B97E File Offset: 0x00199B7E
		// (set) Token: 0x06004045 RID: 16453 RVA: 0x0019B986 File Offset: 0x00199B86
		[Serialize(true, IsPropertySaveable.No, "Should the light sprite be drawn on the item using alpha blending, in addition to being rendered in the light map? Can be used to make the light sprite stand out more.", "", false)]
		public bool AlphaBlend { get; set; }

		// Token: 0x1700110D RID: 4365
		// (get) Token: 0x06004046 RID: 16454 RVA: 0x0019B98F File Offset: 0x00199B8F
		// (set) Token: 0x06004047 RID: 16455 RVA: 0x0019B997 File Offset: 0x00199B97
		[Serialize("0,0", IsPropertySaveable.No, "Offset of the light from the position of the item (in pixels).", "", false)]
		public Vector2 LightOffset { get; set; }

		// Token: 0x1700110E RID: 4366
		// (get) Token: 0x06004048 RID: 16456 RVA: 0x0019B9A0 File Offset: 0x00199BA0
		public bool IsRed
		{
			get
			{
				return ColorExtensions.IsRedDominant(this.LightColor, 2f, 0);
			}
		}

		// Token: 0x1700110F RID: 4367
		// (get) Token: 0x06004049 RID: 16457 RVA: 0x0019B9B3 File Offset: 0x00199BB3
		public bool IsGreen
		{
			get
			{
				return ColorExtensions.IsGreenDominant(this.LightColor, 2f, 0);
			}
		}

		// Token: 0x17001110 RID: 4368
		// (get) Token: 0x0600404A RID: 16458 RVA: 0x0019B9C6 File Offset: 0x00199BC6
		public bool IsBlue
		{
			get
			{
				return ColorExtensions.IsBlueDominant(this.LightColor, 2f, 0);
			}
		}

		// Token: 0x0600404B RID: 16459 RVA: 0x0019B9D9 File Offset: 0x00199BD9
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
		}

		// Token: 0x17001111 RID: 4369
		// (get) Token: 0x0600404C RID: 16460 RVA: 0x0019B9DB File Offset: 0x00199BDB
		// (set) Token: 0x0600404D RID: 16461 RVA: 0x0019B9E4 File Offset: 0x00199BE4
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
			}
		}

		// Token: 0x0600404E RID: 16462 RVA: 0x0019BA0B File Offset: 0x00199C0B
		public LightComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = this.IsOn;
		}

		// Token: 0x0600404F RID: 16463 RVA: 0x0019BA24 File Offset: 0x00199C24
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.turret = this.item.GetComponent<Turret>();
			if (this.item.body != null)
			{
				Body farseerBody = this.item.body.FarseerBody;
				farseerBody.OnEnabled = (Action)Delegate.Combine(farseerBody.OnEnabled, new Action(this.CheckIfNeedsUpdate));
				Body farseerBody2 = this.item.body.FarseerBody;
				farseerBody2.OnDisabled = (Action)Delegate.Combine(farseerBody2.OnDisabled, new Action(this.CheckIfNeedsUpdate));
			}
		}

		// Token: 0x06004050 RID: 16464 RVA: 0x0019BAB7 File Offset: 0x00199CB7
		public override void OnMapLoaded()
		{
			this.CheckIfNeedsUpdate();
		}

		// Token: 0x06004051 RID: 16465 RVA: 0x0019BAC0 File Offset: 0x00199CC0
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
				}
				else
				{
					this.lightBrightness = 1f;
				}
				this.isOn = true;
				base.IsActive = false;
				return;
			}
			base.IsActive = true;
		}

		// Token: 0x06004052 RID: 16466 RVA: 0x0019BBA4 File Offset: 0x00199DA4
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

		// Token: 0x06004053 RID: 16467 RVA: 0x0019BC18 File Offset: 0x00199E18
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
			bool isVisibleInInventory = this.IsVisibleInInventory();
			Character ownerCharacter = this.item.GetRootInventoryOwner() as Character;
			if ((this.item.Container != null && !isVisibleInInventory && ownerCharacter == null) || (ownerCharacter != null && ownerCharacter.InvisibleTimer > 0f))
			{
				this.lightBrightness = 0f;
				return;
			}
			PhysicsBody body = this.ParentBody ?? this.item.body;
			if ((body == null || !body.Enabled) && !isVisibleInInventory)
			{
				this.lightBrightness = 0f;
				return;
			}
			this.TemporaryFlickerTimer -= deltaTime;
			if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.05f && (base.Voltage < Rand.Range(0f, base.MinVoltage, Rand.RandSync.Unsynced) || this.TemporaryFlickerTimer > 0f))
			{
				this.lightBrightness = 0f;
				return;
			}
			this.lightBrightness = MathHelper.Lerp(this.lightBrightness, (this.powerConsumption <= 0f) ? 1f : Math.Min(base.Voltage, 1f), 0.1f);
		}

		// Token: 0x06004054 RID: 16468 RVA: 0x0019BD5B File Offset: 0x00199F5B
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
		}

		// Token: 0x06004055 RID: 16469 RVA: 0x0019BD5D File Offset: 0x00199F5D
		public override bool Use(float deltaTime, Character character = null)
		{
			return true;
		}

		// Token: 0x06004056 RID: 16470 RVA: 0x0019BD60 File Offset: 0x00199F60
		private void OnStateChanged()
		{
			this.sendStateTimer = 0.5f;
			if (this.sendStateCoroutine == null)
			{
				this.sendStateCoroutine = CoroutineManager.StartCoroutine(this.SendStateAfterDelay(), "");
			}
		}

		// Token: 0x06004057 RID: 16471 RVA: 0x0019BD8C File Offset: 0x00199F8C
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

		// Token: 0x06004058 RID: 16472 RVA: 0x0019BE60 File Offset: 0x0019A060
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

		// Token: 0x06004059 RID: 16473 RVA: 0x0019BEB3 File Offset: 0x0019A0B3
		public override void Drop(Character dropper, bool setTransform = true)
		{
			this.SetLightSourceTransform();
		}

		// Token: 0x0600405A RID: 16474 RVA: 0x0019BEBB File Offset: 0x0019A0BB
		public void SetLightSourceTransform()
		{
		}

		// Token: 0x04001EA6 RID: 7846
		private CoroutineHandle sendStateCoroutine;

		// Token: 0x04001EA7 RID: 7847
		private bool lastSentState;

		// Token: 0x04001EA8 RID: 7848
		private float sendStateTimer;

		// Token: 0x04001EA9 RID: 7849
		private Color lightColor;

		// Token: 0x04001EAA RID: 7850
		private float lightBrightness;

		// Token: 0x04001EAB RID: 7851
		private float blinkFrequency;

		// Token: 0x04001EAC RID: 7852
		private float pulseFrequency;

		// Token: 0x04001EAD RID: 7853
		private float pulseAmount;

		// Token: 0x04001EAE RID: 7854
		private float range;

		// Token: 0x04001EAF RID: 7855
		private float flicker;

		// Token: 0x04001EB0 RID: 7856
		private float flickerSpeed;

		// Token: 0x04001EB1 RID: 7857
		private bool castShadows;

		// Token: 0x04001EB2 RID: 7858
		private bool drawBehindSubs;

		// Token: 0x04001EB3 RID: 7859
		private double lastToggleSignalTime;

		// Token: 0x04001EB4 RID: 7860
		private string prevColorSignal;

		// Token: 0x04001EB5 RID: 7861
		public PhysicsBody ParentBody;

		// Token: 0x04001EB6 RID: 7862
		private bool isOn;

		// Token: 0x04001EB7 RID: 7863
		private Turret turret;

		// Token: 0x04001EB8 RID: 7864
		private float rotation;

		// Token: 0x04001EBC RID: 7868
		public float TemporaryFlickerTimer;
	}
}
