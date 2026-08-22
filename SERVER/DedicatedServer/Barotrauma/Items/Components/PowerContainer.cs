using System;
using System.Globalization;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A1 RID: 1185
	internal class PowerContainer : Powered, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06004196 RID: 16790 RVA: 0x001A58A0 File Offset: 0x001A3AA0
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			float newRechargeSpeed = (float)msg.ReadRangedInteger(0, 10) / 10f * this.maxRechargeSpeed;
			if (this.item.CanClientAccess(c))
			{
				this.RechargeSpeed = newRechargeSpeed;
				GameServer.Log(string.Concat(new string[]
				{
					GameServer.CharacterLogName(c.Character),
					" set the recharge speed of ",
					this.item.Name,
					" to ",
					((int)(this.rechargeSpeed / this.maxRechargeSpeed * 100f)).ToString(),
					" %"
				}), ServerLog.MessageType.ItemInteraction);
			}
			this.item.CreateServerEvent<PowerContainer>(this);
		}

		// Token: 0x06004197 RID: 16791 RVA: 0x001A594C File Offset: 0x001A3B4C
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedInteger((int)(this.rechargeSpeed / this.MaxRechargeSpeed * 10f), 0, 10);
			float chargeRatio = MathHelper.Clamp(this.charge / this.adjustedCapacity, 0f, 1f);
			msg.WriteRangedSingle(chargeRatio, 0f, 1f, 8);
		}

		// Token: 0x17001173 RID: 4467
		// (get) Token: 0x06004198 RID: 16792 RVA: 0x001A59A5 File Offset: 0x001A3BA5
		protected override PowerPriority Priority
		{
			get
			{
				return PowerPriority.Battery;
			}
		}

		// Token: 0x17001174 RID: 4468
		// (get) Token: 0x06004199 RID: 16793 RVA: 0x001A59A8 File Offset: 0x001A3BA8
		// (set) Token: 0x0600419A RID: 16794 RVA: 0x001A59B0 File Offset: 0x001A3BB0
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

		// Token: 0x17001175 RID: 4469
		// (get) Token: 0x0600419B RID: 16795 RVA: 0x001A59C3 File Offset: 0x001A3BC3
		// (set) Token: 0x0600419C RID: 16796 RVA: 0x001A59CB File Offset: 0x001A3BCB
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

		// Token: 0x17001176 RID: 4470
		// (get) Token: 0x0600419D RID: 16797 RVA: 0x001A59D4 File Offset: 0x001A3BD4
		// (set) Token: 0x0600419E RID: 16798 RVA: 0x001A59DC File Offset: 0x001A3BDC
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

		// Token: 0x17001177 RID: 4471
		// (get) Token: 0x0600419F RID: 16799 RVA: 0x001A59E5 File Offset: 0x001A3BE5
		// (set) Token: 0x060041A0 RID: 16800 RVA: 0x001A59ED File Offset: 0x001A3BED
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

		// Token: 0x17001178 RID: 4472
		// (get) Token: 0x060041A2 RID: 16802 RVA: 0x001A59FF File Offset: 0x001A3BFF
		// (set) Token: 0x060041A1 RID: 16801 RVA: 0x001A59F6 File Offset: 0x001A3BF6
		[Editable]
		[Serialize(10f, IsPropertySaveable.Yes, "Maximum output of the device when fully charged (kW).", "", false)]
		public float MaxOutPut { get; set; }

		// Token: 0x17001179 RID: 4473
		// (get) Token: 0x060041A3 RID: 16803 RVA: 0x001A5A07 File Offset: 0x001A3C07
		// (set) Token: 0x060041A4 RID: 16804 RVA: 0x001A5A0F File Offset: 0x001A3C0F
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

		// Token: 0x1700117A RID: 4474
		// (get) Token: 0x060041A5 RID: 16805 RVA: 0x001A5A2E File Offset: 0x001A3C2E
		// (set) Token: 0x060041A6 RID: 16806 RVA: 0x001A5A38 File Offset: 0x001A3C38
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
					if (GameMain.Server != null && this.item.FullyInitialized)
					{
						this.item.CreateServerEvent<PowerContainer>(this);
					}
					this.lastSentCharge = this.charge;
				}
			}
		}

		// Token: 0x1700117B RID: 4475
		// (get) Token: 0x060041A7 RID: 16807 RVA: 0x001A5AB1 File Offset: 0x001A3CB1
		public float ChargePercentage
		{
			get
			{
				return MathUtils.Percentage(this.Charge, this.adjustedCapacity);
			}
		}

		// Token: 0x1700117C RID: 4476
		// (get) Token: 0x060041A8 RID: 16808 RVA: 0x001A5AC4 File Offset: 0x001A3CC4
		// (set) Token: 0x060041A9 RID: 16809 RVA: 0x001A5ACC File Offset: 0x001A3CCC
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

		// Token: 0x1700117D RID: 4477
		// (get) Token: 0x060041AA RID: 16810 RVA: 0x001A5ADF File Offset: 0x001A3CDF
		// (set) Token: 0x060041AB RID: 16811 RVA: 0x001A5AE8 File Offset: 0x001A3CE8
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

		// Token: 0x1700117E RID: 4478
		// (get) Token: 0x060041AC RID: 16812 RVA: 0x001A5B4B File Offset: 0x001A3D4B
		// (set) Token: 0x060041AD RID: 16813 RVA: 0x001A5B53 File Offset: 0x001A3D53
		[Serialize(false, IsPropertySaveable.Yes, "If true, the recharge speed (and power consumption) of the device goes up exponentially as the recharge rate is increased.", "", false)]
		public bool ExponentialRechargeSpeed { get; set; }

		// Token: 0x1700117F RID: 4479
		// (get) Token: 0x060041AE RID: 16814 RVA: 0x001A5B5C File Offset: 0x001A3D5C
		// (set) Token: 0x060041AF RID: 16815 RVA: 0x001A5B64 File Offset: 0x001A3D64
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

		// Token: 0x17001180 RID: 4480
		// (get) Token: 0x060041B0 RID: 16816 RVA: 0x001A5B7C File Offset: 0x001A3D7C
		// (set) Token: 0x060041B1 RID: 16817 RVA: 0x001A5B84 File Offset: 0x001A3D84
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

		// Token: 0x17001181 RID: 4481
		// (get) Token: 0x060041B2 RID: 16818 RVA: 0x001A5B8D File Offset: 0x001A3D8D
		// (set) Token: 0x060041B3 RID: 16819 RVA: 0x001A5B95 File Offset: 0x001A3D95
		public bool OutputDisabled { get; private set; }

		// Token: 0x17001182 RID: 4482
		// (get) Token: 0x060041B4 RID: 16820 RVA: 0x001A5B9E File Offset: 0x001A3D9E
		public float RechargeRatio
		{
			get
			{
				return this.RechargeSpeed / this.MaxRechargeSpeed;
			}
		}

		// Token: 0x17001183 RID: 4483
		// (get) Token: 0x060041B5 RID: 16821 RVA: 0x001A5BAD File Offset: 0x001A3DAD
		// (set) Token: 0x060041B6 RID: 16822 RVA: 0x001A5BB5 File Offset: 0x001A3DB5
		public bool HasBeenTuned { get; private set; }

		// Token: 0x060041B7 RID: 16823 RVA: 0x001A5BBE File Offset: 0x001A3DBE
		public PowerContainer(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.prevCharge = this.Charge;
		}

		// Token: 0x060041B8 RID: 16824 RVA: 0x001A5BDB File Offset: 0x001A3DDB
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x060041B9 RID: 16825 RVA: 0x001A5BE4 File Offset: 0x001A3DE4
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

		// Token: 0x060041BA RID: 16826 RVA: 0x001A5D44 File Offset: 0x001A3F44
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

		// Token: 0x060041BB RID: 16827 RVA: 0x001A5E0C File Offset: 0x001A400C
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

		// Token: 0x060041BC RID: 16828 RVA: 0x001A5E94 File Offset: 0x001A4094
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

		// Token: 0x060041BD RID: 16829 RVA: 0x001A5F00 File Offset: 0x001A4100
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

		// Token: 0x060041BE RID: 16830 RVA: 0x001A5F80 File Offset: 0x001A4180
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
					this.item.CreateServerEvent<PowerContainer>(this);
					this.RechargeSpeed = this.maxRechargeSpeed * targetRatio;
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
				this.item.CreateServerEvent<PowerContainer>(this);
				this.RechargeSpeed = 0f;
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

		// Token: 0x060041BF RID: 16831 RVA: 0x001A6188 File Offset: 0x001A4388
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
			}
		}

		// Token: 0x060041C0 RID: 16832 RVA: 0x001A621E File Offset: 0x001A441E
		public float GetCapacity()
		{
			return this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.BatteryCapacity, this.Capacity);
		}

		// Token: 0x04001F82 RID: 8066
		private float capacity;

		// Token: 0x04001F83 RID: 8067
		private float adjustedCapacity;

		// Token: 0x04001F84 RID: 8068
		private float charge;

		// Token: 0x04001F85 RID: 8069
		private float prevCharge;

		// Token: 0x04001F86 RID: 8070
		private float maxRechargeSpeed;

		// Token: 0x04001F87 RID: 8071
		private float rechargeSpeed;

		// Token: 0x04001F88 RID: 8072
		private float lastSentCharge;

		// Token: 0x04001F89 RID: 8073
		protected Vector2 indicatorPosition;

		// Token: 0x04001F8A RID: 8074
		protected Vector2 indicatorSize;

		// Token: 0x04001F8B RID: 8075
		protected bool isHorizontal;

		// Token: 0x04001F8C RID: 8076
		private float currPowerOutput;

		// Token: 0x04001F8F RID: 8079
		private float efficiency;

		// Token: 0x04001F90 RID: 8080
		private bool flipIndicator;

		// Token: 0x04001F92 RID: 8082
		public const float aiRechargeTargetRatio = 0.5f;

		// Token: 0x04001F93 RID: 8083
		private bool isRunning;
	}
}
