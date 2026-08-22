using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.MapCreatures.Behavior;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200049E RID: 1182
	internal class Pump : Powered, IServerSerializable, INetSerializable, IClientSerializable, IDeteriorateUnderStress
	{
		// Token: 0x06004104 RID: 16644 RVA: 0x001A13DC File Offset: 0x0019F5DC
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			float newFlowPercentage = (float)msg.ReadRangedInteger(-10, 10) * 10f;
			bool newIsActive = msg.ReadBoolean();
			if (this.item.CanClientAccess(c))
			{
				if (newFlowPercentage != this.FlowPercentage)
				{
					GameServer.Log(string.Concat(new string[]
					{
						GameServer.CharacterLogName(c.Character),
						" set the pumping speed of ",
						this.item.Name,
						" to ",
						((int)newFlowPercentage).ToString(),
						" %"
					}), ServerLog.MessageType.ItemInteraction);
				}
				if (newIsActive != this.IsActive)
				{
					GameServer.Log(GameServer.CharacterLogName(c.Character) + (newIsActive ? " turned on " : " turned off ") + this.item.Name, ServerLog.MessageType.ItemInteraction);
				}
				if (this.pumpSpeedLockTimer <= 0f)
				{
					this.TargetLevel = null;
				}
				this.FlowPercentage = newFlowPercentage;
				this.IsActive = newIsActive;
			}
			this.item.CreateServerEvent<Pump>(this);
		}

		// Token: 0x06004105 RID: 16645 RVA: 0x001A14DC File Offset: 0x0019F6DC
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteRangedInteger((int)(this.flowPercentage / 10f), -10, 10);
			msg.WriteBoolean(this.IsActive);
			msg.WriteBoolean(this.Hijacked);
			msg.WriteBoolean(this.Disabled);
			if (this.TargetLevel != null)
			{
				msg.WriteBoolean(true);
				msg.WriteSingle(this.TargetLevel.Value);
				return;
			}
			msg.WriteBoolean(false);
		}

		// Token: 0x1700113F RID: 4415
		// (get) Token: 0x06004106 RID: 16646 RVA: 0x001A1551 File Offset: 0x0019F751
		// (set) Token: 0x06004107 RID: 16647 RVA: 0x001A1559 File Offset: 0x0019F759
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
				if (!Submarine.Unloading)
				{
					this.item.CreateServerEvent<Pump>(this);
				}
			}
		}

		// Token: 0x17001140 RID: 4416
		// (get) Token: 0x06004108 RID: 16648 RVA: 0x001A1580 File Offset: 0x0019F780
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

		// Token: 0x17001141 RID: 4417
		// (get) Token: 0x06004109 RID: 16649 RVA: 0x001A15D2 File Offset: 0x0019F7D2
		// (set) Token: 0x0600410A RID: 16650 RVA: 0x001A15DA File Offset: 0x0019F7DA
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

		// Token: 0x17001142 RID: 4418
		// (get) Token: 0x0600410B RID: 16651 RVA: 0x001A1611 File Offset: 0x0019F811
		// (set) Token: 0x0600410C RID: 16652 RVA: 0x001A1619 File Offset: 0x0019F819
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

		// Token: 0x17001143 RID: 4419
		// (get) Token: 0x0600410D RID: 16653 RVA: 0x001A1622 File Offset: 0x0019F822
		// (set) Token: 0x0600410E RID: 16654 RVA: 0x001A162A File Offset: 0x0019F82A
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
				this.networkUpdateTimer = Math.Min(this.networkUpdateTimer, 0.5f);
			}
		}

		// Token: 0x17001144 RID: 4420
		// (get) Token: 0x0600410F RID: 16655 RVA: 0x001A1653 File Offset: 0x0019F853
		// (set) Token: 0x06004110 RID: 16656 RVA: 0x001A165B File Offset: 0x0019F85B
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

		// Token: 0x17001145 RID: 4421
		// (get) Token: 0x06004111 RID: 16657 RVA: 0x001A1664 File Offset: 0x0019F864
		// (set) Token: 0x06004112 RID: 16658 RVA: 0x001A166C File Offset: 0x0019F86C
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		public bool CanCauseLethalPressure { get; set; }

		// Token: 0x17001146 RID: 4422
		// (get) Token: 0x06004113 RID: 16659 RVA: 0x001A1675 File Offset: 0x0019F875
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

		// Token: 0x17001147 RID: 4423
		// (get) Token: 0x06004114 RID: 16660 RVA: 0x001A1690 File Offset: 0x0019F890
		public bool IsHullFull
		{
			get
			{
				return this.item.CurrentHull != null && this.item.CurrentHull.WaterVolume >= this.item.CurrentHull.Volume * 1.05f;
			}
		}

		// Token: 0x17001148 RID: 4424
		// (get) Token: 0x06004115 RID: 16661 RVA: 0x001A16CC File Offset: 0x0019F8CC
		public override bool HasPower
		{
			get
			{
				return this.IsActive && base.Voltage >= base.MinVoltage;
			}
		}

		// Token: 0x17001149 RID: 4425
		// (get) Token: 0x06004116 RID: 16662 RVA: 0x001A16E9 File Offset: 0x0019F8E9
		public bool IsAutoControlled
		{
			get
			{
				return this.pumpSpeedLockTimer > 0f || this.isActiveLockTimer > 0f;
			}
		}

		// Token: 0x1700114A RID: 4426
		// (get) Token: 0x06004117 RID: 16663 RVA: 0x001A1707 File Offset: 0x0019F907
		public override bool UpdateWhenInactive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x1700114B RID: 4427
		// (get) Token: 0x06004118 RID: 16664 RVA: 0x001A170A File Offset: 0x0019F90A
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

		// Token: 0x06004119 RID: 16665 RVA: 0x001A172B File Offset: 0x0019F92B
		public Pump(Item item, ContentXElement element) : base(item, element)
		{
		}

		// Token: 0x0600411A RID: 16666 RVA: 0x001A1740 File Offset: 0x0019F940
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
			this.UpdateNetworking(deltaTime);
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
		}

		// Token: 0x0600411B RID: 16667 RVA: 0x001A1A60 File Offset: 0x0019FC60
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
			hull.BallastFlora.CreateNetworkMessage(default(BallastFloraBehavior.SpawnEventData));
		}

		// Token: 0x0600411C RID: 16668 RVA: 0x001A1B80 File Offset: 0x0019FD80
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

		// Token: 0x0600411D RID: 16669 RVA: 0x001A1BEC File Offset: 0x0019FDEC
		private void UpdateNetworking(float deltaTime)
		{
			this.networkUpdateTimer -= deltaTime;
			if (this.networkUpdateTimer <= 0f)
			{
				this.item.CreateServerEvent<Pump>(this);
				this.networkUpdateTimer = 5f;
			}
		}

		// Token: 0x0600411E RID: 16670 RVA: 0x001A1C20 File Offset: 0x0019FE20
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

		// Token: 0x0600411F RID: 16671 RVA: 0x001A1D4C File Offset: 0x0019FF4C
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			string a = objective.Option.Value.ToLowerInvariant();
			if (!(a == "pumpout"))
			{
				if (!(a == "pumpin"))
				{
					if (a == "stoppumping")
					{
						if (objective.Override || this.FlowPercentage > 0f)
						{
							this.item.CreateServerEvent<Pump>(this);
						}
						this.IsActive = false;
						this.FlowPercentage = 0f;
					}
				}
				else
				{
					if (objective.Override || !this.IsActive || this.FlowPercentage < 100f)
					{
						this.item.CreateServerEvent<Pump>(this);
					}
					this.IsActive = true;
					this.FlowPercentage = 100f;
				}
			}
			else
			{
				if (objective.Override || !this.IsActive || this.FlowPercentage > -100f)
				{
					this.item.CreateServerEvent<Pump>(this);
				}
				this.IsActive = true;
				this.FlowPercentage = -100f;
			}
			return true;
		}

		// Token: 0x06004120 RID: 16672 RVA: 0x001A1E44 File Offset: 0x001A0044
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			this.linkedHulls.Clear();
		}

		// Token: 0x04001F20 RID: 7968
		private const float NetworkUpdateInterval = 5f;

		// Token: 0x04001F21 RID: 7969
		private float networkUpdateTimer;

		// Token: 0x04001F22 RID: 7970
		private float flowPercentage;

		// Token: 0x04001F23 RID: 7971
		private float maxFlow;

		// Token: 0x04001F24 RID: 7972
		public float? TargetLevel;

		// Token: 0x04001F25 RID: 7973
		private bool hijacked;

		// Token: 0x04001F26 RID: 7974
		private float pumpSpeedLockTimer;

		// Token: 0x04001F27 RID: 7975
		private float isActiveLockTimer;

		// Token: 0x04001F28 RID: 7976
		private bool disabled;

		// Token: 0x04001F2A RID: 7978
		private float currFlow;

		// Token: 0x04001F2B RID: 7979
		private const float TinkeringSpeedIncrease = 4f;

		// Token: 0x04001F2C RID: 7980
		private readonly List<Hull> linkedHulls = new List<Hull>();
	}
}
