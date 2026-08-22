using System;
using System.Globalization;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200049C RID: 1180
	internal class Engine : Powered, IServerSerializable, INetSerializable, IClientSerializable, IDeteriorateUnderStress
	{
		// Token: 0x060040BF RID: 16575 RVA: 0x0019E92D File Offset: 0x0019CB2D
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedInteger((int)(this.targetForce / 10f), -10, 10);
			msg.WriteUInt16((this.User == null) ? 0 : this.User.ID);
		}

		// Token: 0x060040C0 RID: 16576 RVA: 0x0019E964 File Offset: 0x0019CB64
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			float newTargetForce = (float)msg.ReadRangedInteger(-10, 10) * 10f;
			if (this.item.CanClientAccess(c))
			{
				this.lastReceivedTargetForce = null;
				if (Math.Abs(newTargetForce - this.targetForce) > 0.01f)
				{
					GameServer.Log(string.Concat(new string[]
					{
						GameServer.CharacterLogName(c.Character),
						" set the force of ",
						this.item.Name,
						" to ",
						((int)newTargetForce).ToString(),
						" %"
					}), ServerLog.MessageType.ItemInteraction);
				}
				this.targetForce = newTargetForce;
				this.User = c.Character;
			}
			this.item.CreateServerEvent<Engine>(this);
		}

		// Token: 0x17001132 RID: 4402
		// (get) Token: 0x060040C1 RID: 16577 RVA: 0x0019EA25 File Offset: 0x0019CC25
		// (set) Token: 0x060040C2 RID: 16578 RVA: 0x0019EA2D File Offset: 0x0019CC2D
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

		// Token: 0x17001133 RID: 4403
		// (get) Token: 0x060040C3 RID: 16579 RVA: 0x0019EA40 File Offset: 0x0019CC40
		// (set) Token: 0x060040C4 RID: 16580 RVA: 0x0019EA48 File Offset: 0x0019CC48
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.Yes, "The position of the propeller as an offset from the item's center (in pixels). Determines where the particles spawn and the position that causes characters to take damage from the engine if the PropellerDamage is defined.", "", false)]
		public Vector2 PropellerPos { get; set; }

		// Token: 0x17001134 RID: 4404
		// (get) Token: 0x060040C5 RID: 16581 RVA: 0x0019EA51 File Offset: 0x0019CC51
		// (set) Token: 0x060040C6 RID: 16582 RVA: 0x0019EA59 File Offset: 0x0019CC59
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool DisablePropellerDamage { get; set; }

		// Token: 0x17001135 RID: 4405
		// (get) Token: 0x060040C7 RID: 16583 RVA: 0x0019EA62 File Offset: 0x0019CC62
		// (set) Token: 0x060040C8 RID: 16584 RVA: 0x0019EA6A File Offset: 0x0019CC6A
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

		// Token: 0x17001136 RID: 4406
		// (get) Token: 0x060040C9 RID: 16585 RVA: 0x0019EA82 File Offset: 0x0019CC82
		public float CurrentVolume
		{
			get
			{
				return this.CurrentStress;
			}
		}

		// Token: 0x17001137 RID: 4407
		// (get) Token: 0x060040CA RID: 16586 RVA: 0x0019EA8C File Offset: 0x0019CC8C
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

		// Token: 0x17001138 RID: 4408
		// (get) Token: 0x060040CB RID: 16587 RVA: 0x0019EADA File Offset: 0x0019CCDA
		public float CurrentStress
		{
			get
			{
				return Math.Abs(this.force / 100f * ((base.MinVoltage <= 0f) ? 1f : Math.Min(this.prevVoltage, 1f)));
			}
		}

		// Token: 0x060040CC RID: 16588 RVA: 0x0019EB14 File Offset: 0x0019CD14
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
		}

		// Token: 0x060040CD RID: 16589 RVA: 0x0019EBA4 File Offset: 0x0019CDA4
		public override void Update(float deltaTime, Camera cam)
		{
			base.UpdateOnActiveEffects(deltaTime);
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
			}
		}

		// Token: 0x060040CE RID: 16590 RVA: 0x0019EDE4 File Offset: 0x0019CFE4
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

		// Token: 0x060040CF RID: 16591 RVA: 0x0019EE52 File Offset: 0x0019D052
		public override void GridResolved(Connection connection)
		{
			if (connection == this.powerIn)
			{
				this.prevVoltage = base.Voltage;
			}
		}

		// Token: 0x060040D0 RID: 16592 RVA: 0x0019EE6C File Offset: 0x0019D06C
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

		// Token: 0x060040D1 RID: 16593 RVA: 0x0019EF24 File Offset: 0x0019D124
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

		// Token: 0x060040D2 RID: 16594 RVA: 0x0019F0D0 File Offset: 0x0019D2D0
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.force = MathHelper.Lerp(this.force, 0f, 0.1f);
		}

		// Token: 0x060040D3 RID: 16595 RVA: 0x0019F0F5 File Offset: 0x0019D2F5
		public override void FlipX(bool relativeToSub)
		{
			this.PropellerPos = new Vector2(-this.PropellerPos.X, this.PropellerPos.Y);
		}

		// Token: 0x060040D4 RID: 16596 RVA: 0x0019F119 File Offset: 0x0019D319
		public override void FlipY(bool relativeToSub)
		{
			this.PropellerPos = new Vector2(this.PropellerPos.X, -this.PropellerPos.Y);
		}

		// Token: 0x060040D5 RID: 16597 RVA: 0x0019F140 File Offset: 0x0019D340
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

		// Token: 0x060040D6 RID: 16598 RVA: 0x0019F1B4 File Offset: 0x0019D3B4
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

		// Token: 0x04001EEF RID: 7919
		private float force;

		// Token: 0x04001EF0 RID: 7920
		private float? lastReceivedTargetForce;

		// Token: 0x04001EF1 RID: 7921
		private float targetForce;

		// Token: 0x04001EF2 RID: 7922
		private const float ForceToPowerExponent = 1.5f;

		// Token: 0x04001EF3 RID: 7923
		private const float PowerToForceExponent = 0.6666667f;

		// Token: 0x04001EF4 RID: 7924
		private float maxForce;

		// Token: 0x04001EF5 RID: 7925
		private readonly Attack propellerDamage;

		// Token: 0x04001EF6 RID: 7926
		private float damageTimer;

		// Token: 0x04001EF7 RID: 7927
		private bool hasPower;

		// Token: 0x04001EF8 RID: 7928
		private float prevVoltage;

		// Token: 0x04001EF9 RID: 7929
		private float controlLockTimer;

		// Token: 0x04001EFA RID: 7930
		public Character User;

		// Token: 0x04001EFD RID: 7933
		private const float TinkeringForceIncrease = 1.5f;
	}
}
