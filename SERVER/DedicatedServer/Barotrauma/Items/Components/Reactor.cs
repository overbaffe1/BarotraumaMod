using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200049F RID: 1183
	internal class Reactor : Powered, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06004121 RID: 16673 RVA: 0x001A1E58 File Offset: 0x001A0058
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			bool autoTemp = msg.ReadBoolean();
			bool powerOn = msg.ReadBoolean();
			float fissionRate = msg.ReadRangedSingle(0f, 100f, 8);
			float turbineOutput = msg.ReadRangedSingle(0f, 100f, 8);
			float temperatureBoostAmount = msg.ReadRangedSingle(-25f, 25f, 8);
			if (!this.item.CanClientAccess(c))
			{
				return;
			}
			this.IsActive = true;
			if (!autoTemp && this.AutoTemp)
			{
				this.blameOnBroken = c;
			}
			if (turbineOutput < this.TargetTurbineOutput)
			{
				this.blameOnBroken = c;
			}
			if (fissionRate > this.TargetFissionRate)
			{
				this.blameOnBroken = c;
			}
			if (!this._powerOn && powerOn)
			{
				this.blameOnBroken = c;
			}
			this.AutoTemp = autoTemp;
			this._powerOn = powerOn;
			this.TargetFissionRate = fissionRate;
			this.TargetTurbineOutput = turbineOutput;
			if (this.AllowTemperatureBoost)
			{
				this.temperatureBoost = temperatureBoostAmount;
			}
			this.LastUser = c.Character;
			if (this.nextServerLogWriteTime == null)
			{
				this.nextServerLogWriteTime = new float?(Math.Max(this.lastServerLogWriteTime + 1f, (float)Timing.TotalTime));
			}
			this.unsentChanges = true;
		}

		// Token: 0x06004122 RID: 16674 RVA: 0x001A1F74 File Offset: 0x001A0174
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.autoTemp);
			msg.WriteBoolean(this._powerOn);
			msg.WriteRangedSingle(this.temperature, 0f, 100f, 8);
			msg.WriteRangedSingle(this.TargetFissionRate, 0f, 100f, 8);
			msg.WriteRangedSingle(this.TargetTurbineOutput, 0f, 100f, 8);
			msg.WriteRangedSingle(this.degreeOfSuccess, 0f, 1f, 8);
			msg.WriteRangedSingle(this.temperatureBoost, -25f, 25f, 8);
		}

		// Token: 0x1700114C RID: 4428
		// (get) Token: 0x06004123 RID: 16675 RVA: 0x001A200C File Offset: 0x001A020C
		public bool AllowTemperatureBoost
		{
			get
			{
				return Math.Abs(this.temperatureBoost) < 22.5f;
			}
		}

		// Token: 0x1700114D RID: 4429
		// (get) Token: 0x06004124 RID: 16676 RVA: 0x001A2020 File Offset: 0x001A0220
		// (set) Token: 0x06004125 RID: 16677 RVA: 0x001A2028 File Offset: 0x001A0228
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool PowerOn
		{
			get
			{
				return this._powerOn;
			}
			set
			{
				this._powerOn = value;
			}
		}

		// Token: 0x1700114E RID: 4430
		// (get) Token: 0x06004126 RID: 16678 RVA: 0x001A2031 File Offset: 0x001A0231
		protected override PowerPriority Priority
		{
			get
			{
				return PowerPriority.Reactor;
			}
		}

		// Token: 0x1700114F RID: 4431
		// (get) Token: 0x06004127 RID: 16679 RVA: 0x001A2034 File Offset: 0x001A0234
		// (set) Token: 0x06004128 RID: 16680 RVA: 0x001A203C File Offset: 0x001A023C
		public Character LastAIUser { get; private set; }

		// Token: 0x17001150 RID: 4432
		// (get) Token: 0x06004129 RID: 16681 RVA: 0x001A2045 File Offset: 0x001A0245
		// (set) Token: 0x0600412A RID: 16682 RVA: 0x001A204D File Offset: 0x001A024D
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool LastUserWasPlayer { get; private set; }

		// Token: 0x17001151 RID: 4433
		// (get) Token: 0x0600412B RID: 16683 RVA: 0x001A2056 File Offset: 0x001A0256
		// (set) Token: 0x0600412C RID: 16684 RVA: 0x001A2060 File Offset: 0x001A0260
		public Character LastUser
		{
			get
			{
				return this.lastUser;
			}
			private set
			{
				if (this.lastUser == value)
				{
					return;
				}
				if (Screen.Selected.IsEditor)
				{
					return;
				}
				this.lastUser = value;
				if (this.lastUser == null)
				{
					this.degreeOfSuccess = 0f;
					this.LastUserWasPlayer = false;
					return;
				}
				this.degreeOfSuccess = Math.Min(base.DegreeOfSuccess(this.lastUser), 1f);
				this.LastUserWasPlayer = this.lastUser.IsPlayer;
			}
		}

		// Token: 0x17001152 RID: 4434
		// (get) Token: 0x0600412D RID: 16685 RVA: 0x001A20D3 File Offset: 0x001A02D3
		// (set) Token: 0x0600412E RID: 16686 RVA: 0x001A20DB File Offset: 0x001A02DB
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(10000f, IsPropertySaveable.Yes, "How much power (kW) the reactor generates when operating at full capacity.", "", true)]
		public float MaxPowerOutput
		{
			get
			{
				return this.maxPowerOutput;
			}
			set
			{
				this.maxPowerOutput = Math.Max(0f, value);
			}
		}

		// Token: 0x17001153 RID: 4435
		// (get) Token: 0x0600412F RID: 16687 RVA: 0x001A20EE File Offset: 0x001A02EE
		// (set) Token: 0x06004130 RID: 16688 RVA: 0x001A20F6 File Offset: 0x001A02F6
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(120f, IsPropertySaveable.Yes, "How long the temperature has to stay critical until a meltdown occurs.", "", false)]
		public float MeltdownDelay
		{
			get
			{
				return this.meltDownDelay;
			}
			set
			{
				this.meltDownDelay = Math.Max(value, 0f);
			}
		}

		// Token: 0x17001154 RID: 4436
		// (get) Token: 0x06004131 RID: 16689 RVA: 0x001A2109 File Offset: 0x001A0309
		// (set) Token: 0x06004132 RID: 16690 RVA: 0x001A2111 File Offset: 0x001A0311
		[Editable(0f, 3.4028235E+38f, 1)]
		[Serialize(30f, IsPropertySaveable.Yes, "How long the temperature has to stay critical until the reactor catches fire.", "", false)]
		public float FireDelay
		{
			get
			{
				return this.fireDelay;
			}
			set
			{
				this.fireDelay = Math.Max(value, 0f);
			}
		}

		// Token: 0x17001155 RID: 4437
		// (get) Token: 0x06004133 RID: 16691 RVA: 0x001A2124 File Offset: 0x001A0324
		// (set) Token: 0x06004134 RID: 16692 RVA: 0x001A212C File Offset: 0x001A032C
		[Serialize(0f, IsPropertySaveable.Yes, "Current temperature of the reactor (0% - 100%). Indended to be used by StatusEffect conditionals.", "", false)]
		public float Temperature
		{
			get
			{
				return this.temperature;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.temperature = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17001156 RID: 4438
		// (get) Token: 0x06004135 RID: 16693 RVA: 0x001A214D File Offset: 0x001A034D
		// (set) Token: 0x06004136 RID: 16694 RVA: 0x001A2155 File Offset: 0x001A0355
		[Serialize(0f, IsPropertySaveable.Yes, "Current fission rate of the reactor (0% - 100%). Intended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public float FissionRate
		{
			get
			{
				return this.fissionRate;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.fissionRate = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17001157 RID: 4439
		// (get) Token: 0x06004137 RID: 16695 RVA: 0x001A2176 File Offset: 0x001A0376
		// (set) Token: 0x06004138 RID: 16696 RVA: 0x001A217E File Offset: 0x001A037E
		[Serialize(0f, IsPropertySaveable.Yes, "Current turbine output of the reactor (0% - 100%). Intended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public float TurbineOutput
		{
			get
			{
				return this.turbineOutput;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.turbineOutput = MathHelper.Clamp(value, 0f, 100f);
			}
		}

		// Token: 0x17001158 RID: 4440
		// (get) Token: 0x06004139 RID: 16697 RVA: 0x001A219F File Offset: 0x001A039F
		// (set) Token: 0x0600413A RID: 16698 RVA: 0x001A21A7 File Offset: 0x001A03A7
		[Serialize(0.2f, IsPropertySaveable.Yes, "How fast the condition of the contained fuel rods deteriorates per second.", "", false)]
		[Editable(0f, 1000f, 3)]
		public float FuelConsumptionRate
		{
			get
			{
				return this.fuelConsumptionRate;
			}
			set
			{
				if (!MathUtils.IsValid(value))
				{
					return;
				}
				this.fuelConsumptionRate = Math.Max(value, 0f);
			}
		}

		// Token: 0x17001159 RID: 4441
		// (get) Token: 0x0600413B RID: 16699 RVA: 0x001A21C3 File Offset: 0x001A03C3
		// (set) Token: 0x0600413C RID: 16700 RVA: 0x001A21D8 File Offset: 0x001A03D8
		[Serialize(false, IsPropertySaveable.Yes, "Is the temperature currently critical. Intended to be used by StatusEffect conditionals (setting the value from XML has no effect).", "", false)]
		public bool TemperatureCritical
		{
			get
			{
				return this.temperature > this.allowedTemperature.Y;
			}
			set
			{
			}
		}

		// Token: 0x1700115A RID: 4442
		// (get) Token: 0x0600413D RID: 16701 RVA: 0x001A21DA File Offset: 0x001A03DA
		// (set) Token: 0x0600413E RID: 16702 RVA: 0x001A21E2 File Offset: 0x001A03E2
		[Serialize(false, IsPropertySaveable.Yes, "Is the automatic temperature control currently on. Indended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public bool AutoTemp
		{
			get
			{
				return this.autoTemp;
			}
			set
			{
				this.autoTemp = value;
			}
		}

		// Token: 0x1700115B RID: 4443
		// (get) Token: 0x0600413F RID: 16703 RVA: 0x001A21EB File Offset: 0x001A03EB
		// (set) Token: 0x06004140 RID: 16704 RVA: 0x001A21F3 File Offset: 0x001A03F3
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float AvailableFuel { get; set; }

		// Token: 0x1700115C RID: 4444
		// (get) Token: 0x06004141 RID: 16705 RVA: 0x001A21FC File Offset: 0x001A03FC
		// (set) Token: 0x06004142 RID: 16706 RVA: 0x001A2204 File Offset: 0x001A0404
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public new float Load { get; private set; }

		// Token: 0x1700115D RID: 4445
		// (get) Token: 0x06004143 RID: 16707 RVA: 0x001A220D File Offset: 0x001A040D
		// (set) Token: 0x06004144 RID: 16708 RVA: 0x001A2215 File Offset: 0x001A0415
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float TargetFissionRate { get; set; }

		// Token: 0x1700115E RID: 4446
		// (get) Token: 0x06004145 RID: 16709 RVA: 0x001A221E File Offset: 0x001A041E
		// (set) Token: 0x06004146 RID: 16710 RVA: 0x001A2226 File Offset: 0x001A0426
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float TargetTurbineOutput { get; set; }

		// Token: 0x1700115F RID: 4447
		// (get) Token: 0x06004147 RID: 16711 RVA: 0x001A222F File Offset: 0x001A042F
		// (set) Token: 0x06004148 RID: 16712 RVA: 0x001A2237 File Offset: 0x001A0437
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float CorrectTurbineOutput { get; set; }

		// Token: 0x17001160 RID: 4448
		// (get) Token: 0x06004149 RID: 16713 RVA: 0x001A2240 File Offset: 0x001A0440
		// (set) Token: 0x0600414A RID: 16714 RVA: 0x001A2248 File Offset: 0x001A0448
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool ExplosionDamagesOtherSubs { get; set; }

		// Token: 0x17001161 RID: 4449
		// (get) Token: 0x0600414B RID: 16715 RVA: 0x001A2251 File Offset: 0x001A0451
		// (set) Token: 0x0600414C RID: 16716 RVA: 0x001A2259 File Offset: 0x001A0459
		public bool MeltedDownThisRound { get; private set; }

		// Token: 0x0600414D RID: 16717 RVA: 0x001A2262 File Offset: 0x001A0462
		public Reactor(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
		}

		// Token: 0x0600414E RID: 16718 RVA: 0x001A2274 File Offset: 0x001A0474
		public override void Update(float deltaTime, Camera cam)
		{
			if (GameMain.Server != null && this.nextServerLogWriteTime != null && Timing.TotalTime >= (double)this.nextServerLogWriteTime.Value)
			{
				GameServer.Log(string.Concat(new string[]
				{
					GameServer.CharacterLogName(this.lastUser),
					" adjusted reactor settings: Temperature: ",
					((int)(this.temperature * 100f)).ToString(),
					", Fission rate: ",
					((int)this.TargetFissionRate).ToString(),
					", Turbine output: ",
					((int)this.TargetTurbineOutput).ToString(),
					this.autoTemp ? ", Autotemp ON" : ", Autotemp OFF"
				}), ServerLog.MessageType.ItemInteraction);
				this.nextServerLogWriteTime = null;
				this.lastServerLogWriteTime = (float)Timing.TotalTime;
			}
			if (this.LastAIUser != null && this.LastAIUser.SelectedItem != this.item && this.LastAIUser.CanInteractWith(this.item, true))
			{
				this.AutoTemp = true;
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember != null && networkMember.IsServer)
				{
					this.unsentChanges = true;
				}
				this.LastAIUser = null;
			}
			bool fissionRateControlledBySignals = this.signalControlledTargetFissionRate != null && this.lastReceivedFissionRateSignalTime > Timing.TotalTime - 1.0;
			bool turbineOutputRateControlledBySignals = this.signalControlledTargetTurbineOutput != null && this.lastReceivedTurbineOutputSignalTime > Timing.TotalTime - 1.0;
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null && gameSession.RoundDuration < 5f)
			{
				Character character = this.lastUser;
				if ((character == null || !character.IsPlayer) && this.PowerOn && this.AutoTemp && !fissionRateControlledBySignals && !turbineOutputRateControlledBySignals)
				{
					this.UpdateAutoTemp(100f, 0.16666667f);
				}
			}
			float maxPowerOut = this.GetMaxOutput();
			if (fissionRateControlledBySignals)
			{
				this.TargetFissionRate = Reactor.<Update>g__adjustValueWithoutOverShooting|112_0(this.TargetFissionRate, this.signalControlledTargetFissionRate.Value, deltaTime * 5f);
			}
			else
			{
				this.signalControlledTargetFissionRate = null;
			}
			if (turbineOutputRateControlledBySignals)
			{
				this.TargetTurbineOutput = Reactor.<Update>g__adjustValueWithoutOverShooting|112_0(this.TargetTurbineOutput, this.signalControlledTargetTurbineOutput.Value, deltaTime * 5f);
			}
			else
			{
				this.signalControlledTargetTurbineOutput = null;
			}
			this.prevAvailableFuel = this.AvailableFuel;
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			if (!MathUtils.NearlyEqual(maxPowerOut, 0f, 0.0001f))
			{
				this.CorrectTurbineOutput += MathHelper.Clamp(this.Load / maxPowerOut * 100f - this.CorrectTurbineOutput, -20f, 20f) * deltaTime;
			}
			float tolerance = MathHelper.Lerp(2.5f, 10f, this.degreeOfSuccess);
			this.optimalTurbineOutput = new Vector2(this.CorrectTurbineOutput - tolerance, this.CorrectTurbineOutput + tolerance);
			tolerance = MathHelper.Lerp(5f, 20f, this.degreeOfSuccess);
			this.allowedTurbineOutput = new Vector2(this.CorrectTurbineOutput - tolerance, this.CorrectTurbineOutput + tolerance);
			this.optimalTemperature = Vector2.Lerp(new Vector2(40f, 60f), new Vector2(30f, 70f), this.degreeOfSuccess);
			this.allowedTemperature = Vector2.Lerp(new Vector2(30f, 70f), new Vector2(10f, 90f), this.degreeOfSuccess);
			this.optimalFissionRate = Vector2.Lerp(new Vector2(30f, this.AvailableFuel - 20f), new Vector2(20f, this.AvailableFuel - 10f), this.degreeOfSuccess);
			this.optimalFissionRate.X = Math.Min(this.optimalFissionRate.X, this.optimalFissionRate.Y - 10f);
			this.allowedFissionRate = Vector2.Lerp(new Vector2(20f, this.AvailableFuel), new Vector2(10f, this.AvailableFuel), this.degreeOfSuccess);
			this.allowedFissionRate.X = Math.Min(this.allowedFissionRate.X, this.allowedFissionRate.Y - 10f);
			float heatAmount = this.GetGeneratedHeat(this.fissionRate);
			float temperatureDiff = heatAmount - this.turbineOutput - this.Temperature;
			this.Temperature += MathHelper.Clamp((float)Math.Sign(temperatureDiff) * 10f * deltaTime, -Math.Abs(temperatureDiff), Math.Abs(temperatureDiff));
			this.temperatureBoost = Reactor.<Update>g__adjustValueWithoutOverShooting|112_0(this.temperatureBoost, 0f, deltaTime);
			this.FissionRate = MathHelper.Lerp(this.fissionRate, Math.Min(this.TargetFissionRate, this.AvailableFuel), deltaTime);
			this.TurbineOutput = MathHelper.Lerp(this.turbineOutput, this.TargetTurbineOutput, deltaTime);
			float temperatureFactor = Math.Min(this.temperature / 50f, 1f);
			if (!this.PowerOn)
			{
				this.TargetFissionRate = 0f;
				this.TargetTurbineOutput = 0f;
			}
			else if (this.autoTemp)
			{
				this.UpdateAutoTemp(2f, deltaTime);
			}
			float fuelLeft = 0f;
			ItemInventory ownInventory = this.item.OwnInventory;
			IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
			if (containedItems != null)
			{
				foreach (Item item in containedItems)
				{
					if (item.HasTag(Tags.ReactorFuel))
					{
						if (this.fissionRate > 0f)
						{
							if (!Level.IsLoadedOutpost)
							{
								goto IL_5D9;
							}
							Submarine submarine = base.Item.Submarine;
							if (submarine == null || submarine.TeamID != CharacterTeamType.Team1)
							{
								goto IL_5D9;
							}
							bool flag = base.Item.Submarine.GetConnectedSubs().Any((Submarine s) => s.Info.IsOutpost && s.TeamID == CharacterTeamType.FriendlyNPC);
							IL_5DA:
							if (!flag)
							{
								item.Condition -= this.fissionRate / 100f * this.GetFuelConsumption() * deltaTime;
								goto IL_603;
							}
							goto IL_603;
							IL_5D9:
							flag = false;
							goto IL_5DA;
						}
						IL_603:
						fuelLeft += item.ConditionPercentage;
					}
				}
			}
			if (this.fissionRate > 0f && this.item.AiTarget != null && maxPowerOut > 0f)
			{
				AITarget aiTarget = this.item.AiTarget;
				float range = Math.Abs(this.currPowerConsumption) / maxPowerOut;
				aiTarget.SoundRange = MathHelper.Lerp(aiTarget.MinSoundRange, aiTarget.MaxSoundRange, range);
				if (this.item.CurrentHull != null)
				{
					AITarget hullAITarget = this.item.CurrentHull.AiTarget;
					if (hullAITarget != null)
					{
						hullAITarget.SoundRange = Math.Max(hullAITarget.SoundRange, aiTarget.SoundRange);
					}
				}
			}
			this.item.SendSignal(((int)(this.temperature * 100f)).ToString(), "temperature_out");
			this.item.SendSignal(((int)(-(int)base.CurrPowerConsumption)).ToString(), "power_value_out");
			this.item.SendSignal(((int)this.Load).ToString(), "load_value_out");
			this.item.SendSignal(((int)this.AvailableFuel).ToString(), "fuel_out");
			this.item.SendSignal(((int)fuelLeft).ToString(), "fuel_percentage_left");
			this.UpdateFailures(deltaTime);
			this.AvailableFuel = 0f;
			this.sendUpdateTimer -= deltaTime;
			if (this.sendUpdateTimer < -10f || (this.unsentChanges && this.sendUpdateTimer <= 0f))
			{
				if (GameMain.Server != null)
				{
					this.item.CreateServerEvent<Reactor>(this);
				}
				this.sendUpdateTimer = 0.5f;
				this.unsentChanges = false;
			}
		}

		// Token: 0x0600414F RID: 16719 RVA: 0x001A2A60 File Offset: 0x001A0C60
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			return (float)((connection != null && connection.IsPower && connection.IsOutput) ? -1 : 0);
		}

		// Token: 0x06004150 RID: 16720 RVA: 0x001A2A7C File Offset: 0x001A0C7C
		public override PowerRange MinMaxPowerOut(Connection conn, float load)
		{
			float tolerance = 1f;
			if (this.turbineOutput > this.optimalTurbineOutput.X && this.turbineOutput < this.optimalTurbineOutput.Y && this.temperature > this.optimalTemperature.X && this.temperature < this.optimalTemperature.Y)
			{
				tolerance = 3f;
			}
			float maxPowerOut = this.GetMaxOutput();
			float temperatureFactor = Math.Min(this.temperature / 50f, 1f);
			float minOutput = maxPowerOut * Math.Clamp(Math.Min((this.turbineOutput - tolerance) / 100f, temperatureFactor), 0f, 1f);
			float maxOutput = maxPowerOut * Math.Min((this.turbineOutput + tolerance) / 100f, temperatureFactor);
			this.minUpdatePowerOut = minOutput;
			this.maxUpdatePowerOut = maxOutput;
			float reactorMax = this.PowerOn ? maxPowerOut : this.maxUpdatePowerOut;
			return new PowerRange(minOutput, maxOutput, reactorMax);
		}

		// Token: 0x06004151 RID: 16721 RVA: 0x001A2B6C File Offset: 0x001A0D6C
		public override float GetConnectionPowerOut(Connection conn, float power, PowerRange minMaxPower, float load)
		{
			float loadLeft = MathHelper.Max(load - power, 0f);
			float expectedPower = MathHelper.Clamp(loadLeft, minMaxPower.Min, minMaxPower.Max);
			float ratio = MathHelper.Max((loadLeft - minMaxPower.Min) / (minMaxPower.Max - minMaxPower.Min), 0f);
			if (float.IsInfinity(ratio))
			{
				ratio = 0f;
			}
			float output = MathHelper.Clamp(ratio * (this.maxUpdatePowerOut - this.minUpdatePowerOut) + this.minUpdatePowerOut, this.minUpdatePowerOut, this.maxUpdatePowerOut);
			float newLoad = loadLeft;
			float maxOutput = this.GetMaxOutput();
			if (maxOutput != minMaxPower.ReactorMaxOutput)
			{
				float idealLoad = maxOutput / minMaxPower.ReactorMaxOutput * loadLeft;
				float loadAdjust = MathHelper.Clamp((ratio - 0.5f) * 25f + idealLoad - this.turbineOutput / 100f * maxOutput, -maxOutput / 100f, maxOutput / 100f);
				newLoad = MathHelper.Clamp(loadLeft - (expectedPower - output) + loadAdjust, 0f, loadLeft);
			}
			if (float.IsNegative(newLoad))
			{
				newLoad = 0f;
			}
			this.Load = newLoad;
			this.currPowerConsumption = -output;
			return output;
		}

		// Token: 0x06004152 RID: 16722 RVA: 0x001A2C82 File Offset: 0x001A0E82
		private float GetGeneratedHeat(float fissionRate)
		{
			return fissionRate * (this.prevAvailableFuel / 100f) * 2f + this.temperatureBoost;
		}

		// Token: 0x06004153 RID: 16723 RVA: 0x001A2CA0 File Offset: 0x001A0EA0
		private bool NeedMoreFuel(float minimumOutputRatio, float minCondition = 0f)
		{
			float remainingFuel = this.item.ContainedItems.Sum((Item i) => i.Condition);
			if (remainingFuel <= minCondition && this.Load > 0f)
			{
				return true;
			}
			float maxFissionRate = Math.Min(this.prevAvailableFuel, 100f);
			if (maxFissionRate >= 100f)
			{
				return false;
			}
			float maxTurbineOutput = 100f;
			float theoreticalMaxHeat = this.GetGeneratedHeat(maxFissionRate);
			float temperatureFactor = Math.Min(theoreticalMaxHeat / 50f, 1f);
			float theoreticalMaxOutput = Math.Min(maxTurbineOutput / 100f, temperatureFactor) * this.GetMaxOutput();
			return theoreticalMaxOutput < this.Load * minimumOutputRatio;
		}

		// Token: 0x06004154 RID: 16724 RVA: 0x001A2D50 File Offset: 0x001A0F50
		private bool TooMuchFuel()
		{
			ItemInventory ownInventory = this.item.OwnInventory;
			IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
			if (containedItems != null && containedItems.Count<Item>() <= 1)
			{
				return false;
			}
			float minimumHeat = this.GetGeneratedHeat(this.optimalFissionRate.X);
			return minimumHeat > Math.Min(this.CorrectTurbineOutput * 1.5f, 90f);
		}

		// Token: 0x06004155 RID: 16725 RVA: 0x001A2DB0 File Offset: 0x001A0FB0
		private void UpdateFailures(float deltaTime)
		{
			if (this.temperature > this.allowedTemperature.Y)
			{
				this.item.SendSignal("1", "meltdown_warning");
				if (!this.item.InvulnerableToDamage)
				{
					this.meltDownTimer += MathHelper.Lerp(deltaTime * 2f, deltaTime, this.item.Condition / this.item.MaxCondition);
					if (this.meltDownTimer > this.MeltdownDelay)
					{
						this.MeltDown();
						return;
					}
				}
			}
			else
			{
				this.item.SendSignal("0", "meltdown_warning");
				this.meltDownTimer = Math.Max(0f, this.meltDownTimer - deltaTime);
			}
			if (this.temperature > this.optimalTemperature.Y)
			{
				this.fireTimer += MathHelper.Lerp(deltaTime * 2f, deltaTime, this.item.Condition / this.item.MaxCondition);
				if (this.fireTimer > Math.Min(5f, this.FireDelay / 2f))
				{
					Client client = this.blameOnBroken;
					if (((client != null) ? client.Character : null) != null)
					{
						GameMain.Server.KarmaManager.OnReactorOverHeating(this.item, this.blameOnBroken.Character, deltaTime);
					}
				}
				if (this.fireTimer >= this.FireDelay)
				{
					new FireSource(this.item.WorldPosition, null, null, false);
					this.fireTimer = 0f;
					return;
				}
			}
			else
			{
				this.fireTimer = Math.Max(0f, this.fireTimer - deltaTime);
			}
		}

		// Token: 0x06004156 RID: 16726 RVA: 0x001A2F48 File Offset: 0x001A1148
		public void UpdateAutoTemp(float speed, float deltaTime)
		{
			float desiredTurbineOutput = (this.optimalTurbineOutput.X + this.optimalTurbineOutput.Y) / 2f;
			this.TargetTurbineOutput += MathHelper.Clamp(desiredTurbineOutput - this.TargetTurbineOutput, -speed, speed) * deltaTime;
			this.TargetTurbineOutput = MathHelper.Clamp(this.TargetTurbineOutput, 0f, 100f);
			float desiredFissionRate = (this.optimalFissionRate.X + this.optimalFissionRate.Y) / 2f;
			this.TargetFissionRate += MathHelper.Clamp(desiredFissionRate - this.TargetFissionRate, -speed, speed) * deltaTime;
			if (this.temperature > (this.optimalTemperature.X + this.optimalTemperature.Y) / 2f)
			{
				this.TargetFissionRate = Math.Min(this.TargetFissionRate - speed * 2f * deltaTime, this.allowedFissionRate.Y);
			}
			else if (-this.currPowerConsumption < this.Load)
			{
				this.TargetFissionRate = Math.Min(this.TargetFissionRate + speed * 2f * deltaTime, 100f);
			}
			this.TargetFissionRate = MathHelper.Clamp(this.TargetFissionRate, 0f, 100f);
			this.TargetFissionRate = MathHelper.Clamp(this.TargetFissionRate, this.FissionRate - 5f, this.FissionRate + 5f);
		}

		// Token: 0x06004157 RID: 16727 RVA: 0x001A30AC File Offset: 0x001A12AC
		public void PowerUpImmediately()
		{
			this.PowerOn = true;
			this.AutoTemp = true;
			this.prevAvailableFuel = this.AvailableFuel;
			for (int i = 0; i < 100; i++)
			{
				this.Update(0.16666667f, null);
				this.UpdateAutoTemp(100f, 0.16666667f);
				this.AvailableFuel = this.prevAvailableFuel;
			}
		}

		// Token: 0x06004158 RID: 16728 RVA: 0x001A3108 File Offset: 0x001A1308
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.item.SendSignal(((int)(this.temperature * 100f)).ToString(), "temperature_out");
			this.currPowerConsumption = 0f;
			this.Temperature -= deltaTime * 1000f;
			this.TargetFissionRate = Math.Max(this.TargetFissionRate - deltaTime * 10f, 0f);
			this.TargetTurbineOutput = Math.Max(this.TargetTurbineOutput - deltaTime * 10f, 0f);
		}

		// Token: 0x06004159 RID: 16729 RVA: 0x001A31A0 File Offset: 0x001A13A0
		private void MeltDown()
		{
			if (this.item.Condition <= 0f)
			{
				return;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			if (!this.ExplosionDamagesOtherSubs)
			{
				Dictionary<ActionType, List<StatusEffect>> statusEffectLists = this.statusEffectLists;
				if (statusEffectLists != null && statusEffectLists.ContainsKey(ActionType.OnBroken))
				{
					foreach (StatusEffect statusEffect in this.statusEffectLists[ActionType.OnBroken])
					{
						foreach (Explosion explosion in statusEffect.Explosions)
						{
							foreach (Submarine sub in Submarine.Loaded)
							{
								if (sub != this.item.Submarine)
								{
									explosion.IgnoredSubmarines.Add(sub);
								}
							}
						}
					}
				}
			}
			this.item.Condition = 0f;
			this.fireTimer = 0f;
			this.meltDownTimer = 0f;
			this.MeltedDownThisRound = true;
			ItemInventory ownInventory = this.item.OwnInventory;
			IEnumerable<Item> containedItems = (ownInventory != null) ? ownInventory.AllItems : null;
			if (containedItems != null)
			{
				foreach (Item containedItem in containedItems)
				{
					containedItem.Condition = 0f;
				}
			}
			GameServer.Log("Reactor meltdown!", ServerLog.MessageType.ItemInteraction);
			if (GameMain.Server != null)
			{
				KarmaManager karmaManager = GameMain.Server.KarmaManager;
				Item item = this.item;
				Client client = this.blameOnBroken;
				karmaManager.OnReactorMeltdown(item, (client != null) ? client.Character : null);
			}
		}

		// Token: 0x0600415A RID: 16730 RVA: 0x001A3398 File Offset: 0x001A1598
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x0600415B RID: 16731 RVA: 0x001A33A0 File Offset: 0x001A15A0
		public override bool CrewAIOperate(float deltaTime, Character character, AIObjectiveOperateItem objective)
		{
			Reactor.<>c__DisplayClass125_0 CS$<>8__locals1 = new Reactor.<>c__DisplayClass125_0();
			CS$<>8__locals1.character = character;
			CS$<>8__locals1.<>4__this = this;
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return false;
			}
			CS$<>8__locals1.character.AIController.SteeringManager.Reset();
			bool shutDown = objective.Option == "shutdown";
			this.IsActive = true;
			if (!shutDown)
			{
				float degreeOfSuccess = Math.Min(base.DegreeOfSuccess(CS$<>8__locals1.character), 1f);
				float refuelLimit = 0.3f;
				if (degreeOfSuccess > refuelLimit)
				{
					if (this.aiUpdateTimer > 0f)
					{
						this.aiUpdateTimer -= deltaTime;
						return false;
					}
					this.aiUpdateTimer = 0.2f;
					float minCondition = this.GetFuelConsumption() * MathUtils.Pow2((degreeOfSuccess - refuelLimit) * 2f);
					if (this.NeedMoreFuel(0.5f, minCondition))
					{
						Reactor.<>c__DisplayClass125_1 CS$<>8__locals2 = new Reactor.<>c__DisplayClass125_1();
						CS$<>8__locals2.CS$<>8__locals1 = CS$<>8__locals1;
						CS$<>8__locals2.outOfFuel = false;
						ItemContainer container = this.item.GetComponent<ItemContainer>();
						if (objective.SubObjectives.None(null))
						{
							AIObjectiveContainItem containObjective = base.AIContainItems<Reactor>(container, CS$<>8__locals2.CS$<>8__locals1.character, objective, 1, true, true, !CS$<>8__locals2.CS$<>8__locals1.character.IsOnPlayerTeam, true);
							containObjective.Completed += CS$<>8__locals2.<CrewAIOperate>g__ReportFuelRodCount|2;
							containObjective.Abandoned += CS$<>8__locals2.<CrewAIOperate>g__ReportFuelRodCount|2;
							CS$<>8__locals2.CS$<>8__locals1.character.Speak(TextManager.Get("DialogReactorFuel").Value, null, 0f, Tags.ReactorFuel, 30f);
						}
						return CS$<>8__locals2.outOfFuel;
					}
					if (base.Item.ConditionPercentage <= 0f && AIObjectiveRepairItems.IsValidTarget(base.Item, CS$<>8__locals1.character))
					{
						if (base.Item.Repairables.Average((Repairable r) => r.DegreeOfSuccess(CS$<>8__locals1.character)) > 0.4f)
						{
							objective.AddSubObjective(new AIObjectiveRepairItem(CS$<>8__locals1.character, base.Item, objective.objectiveManager, 1f, true), false);
							return false;
						}
						Character character2 = CS$<>8__locals1.character;
						string value = TextManager.Get("DialogReactorIsBroken").Value;
						Identifier identifier = "reactorisbroken".ToIdentifier();
						character2.Speak(value, null, 0f, identifier, 30f);
					}
					if (this.TooMuchFuel())
					{
						CS$<>8__locals1.<CrewAIOperate>g__DropFuel|0(0.1f, 100f);
					}
					else
					{
						CS$<>8__locals1.<CrewAIOperate>g__DropFuel|0(0f, 0f);
					}
				}
			}
			if (objective.Override)
			{
				if (this.lastUser != null && this.lastUser != CS$<>8__locals1.character && this.lastUser != this.LastAIUser && this.lastUser.SelectedItem == this.item && CS$<>8__locals1.character.IsOnPlayerTeam)
				{
					CS$<>8__locals1.character.Speak(TextManager.Get("DialogReactorTaken").Value, null, 0f, "reactortaken".ToIdentifier(), 10f);
				}
			}
			else if (this.LastUserWasPlayer && this.lastUser != null && this.lastUser.TeamID == CS$<>8__locals1.character.TeamID)
			{
				return true;
			}
			this.LastUser = (this.LastAIUser = CS$<>8__locals1.character);
			bool prevAutoTemp = this.autoTemp;
			bool prevPowerOn = this._powerOn;
			float prevFissionRate = this.TargetFissionRate;
			float prevTurbineOutput = this.TargetTurbineOutput;
			if (shutDown)
			{
				this.PowerOn = false;
				this.TargetFissionRate = 0f;
				this.TargetTurbineOutput = 0f;
				this.unsentChanges = true;
				return true;
			}
			this.PowerOn = true;
			if (objective.Override || !this.autoTemp)
			{
				if (this.degreeOfSuccess < 0.5f)
				{
					this.AutoTemp = true;
				}
				else
				{
					this.AutoTemp = false;
					this.UpdateAutoTemp(MathHelper.Lerp(0.5f, 2f, this.degreeOfSuccess), 1f);
				}
			}
			if (this.autoTemp != prevAutoTemp || prevPowerOn != this._powerOn || Math.Abs(prevFissionRate - this.TargetFissionRate) > 1f || Math.Abs(prevTurbineOutput - this.TargetTurbineOutput) > 1f)
			{
				this.unsentChanges = true;
			}
			this.aiUpdateTimer = 0.2f;
			return false;
		}

		// Token: 0x0600415C RID: 16732 RVA: 0x001A37E8 File Offset: 0x001A19E8
		public override void OnMapLoaded()
		{
			this.prevAvailableFuel = this.AvailableFuel;
		}

		// Token: 0x0600415D RID: 16733 RVA: 0x001A37F8 File Offset: 0x001A19F8
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			string name = connection.Name;
			if (!(name == "shutdown"))
			{
				float newFissionRate;
				if (!(name == "set_fissionrate"))
				{
					if (!(name == "set_turbineoutput"))
					{
						return;
					}
					float newTurbineOutput;
					if (this.PowerOn && float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newTurbineOutput))
					{
						this.signalControlledTargetTurbineOutput = new float?(MathHelper.Clamp(newTurbineOutput, 0f, 100f));
						this.lastReceivedTurbineOutputSignalTime = Timing.TotalTime;
						this.<ReceiveSignal>g__registerUnsentChanges|127_0();
					}
				}
				else if (this.PowerOn && float.TryParse(signal.value, NumberStyles.Float, CultureInfo.InvariantCulture, out newFissionRate))
				{
					this.signalControlledTargetFissionRate = new float?(MathHelper.Clamp(newFissionRate, 0f, 100f));
					this.lastReceivedFissionRateSignalTime = Timing.TotalTime;
					this.<ReceiveSignal>g__registerUnsentChanges|127_0();
					return;
				}
			}
			else if (this.TargetFissionRate > 0f || this.TargetTurbineOutput > 0f)
			{
				this.PowerOn = false;
				this.AutoTemp = false;
				this.TargetFissionRate = 0f;
				this.TargetTurbineOutput = 0f;
				this.<ReceiveSignal>g__registerUnsentChanges|127_0();
				return;
			}
		}

		// Token: 0x0600415E RID: 16734 RVA: 0x001A391D File Offset: 0x001A1B1D
		private float GetMaxOutput()
		{
			return this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.ReactorMaxOutput, this.MaxPowerOutput);
		}

		// Token: 0x0600415F RID: 16735 RVA: 0x001A3936 File Offset: 0x001A1B36
		private float GetFuelConsumption()
		{
			return this.item.StatManager.GetAdjustedValueMultiplicative(ItemTalentStats.ReactorFuelConsumption, this.fuelConsumptionRate);
		}

		// Token: 0x06004160 RID: 16736 RVA: 0x001A394F File Offset: 0x001A1B4F
		[CompilerGenerated]
		internal static float <Update>g__adjustValueWithoutOverShooting|112_0(float current, float target, float speed)
		{
			if (target >= current)
			{
				return Math.Min(target, current + speed);
			}
			return Math.Max(target, current - speed);
		}

		// Token: 0x06004161 RID: 16737 RVA: 0x001A3968 File Offset: 0x001A1B68
		[CompilerGenerated]
		private void <ReceiveSignal>g__registerUnsentChanges|127_0()
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			if (networkMember != null && networkMember.IsServer)
			{
				this.unsentChanges = true;
			}
		}

		// Token: 0x04001F2D RID: 7981
		private const float NetworkUpdateIntervalLow = 10f;

		// Token: 0x04001F2E RID: 7982
		private Client blameOnBroken;

		// Token: 0x04001F2F RID: 7983
		private float? nextServerLogWriteTime;

		// Token: 0x04001F30 RID: 7984
		private float lastServerLogWriteTime;

		// Token: 0x04001F31 RID: 7985
		private const float NetworkUpdateIntervalHigh = 0.5f;

		// Token: 0x04001F32 RID: 7986
		private const float TemperatureBoostAmount = 25f;

		// Token: 0x04001F33 RID: 7987
		private float fissionRate;

		// Token: 0x04001F34 RID: 7988
		private float turbineOutput;

		// Token: 0x04001F35 RID: 7989
		private float temperature;

		// Token: 0x04001F36 RID: 7990
		private bool autoTemp;

		// Token: 0x04001F37 RID: 7991
		private float fuelConsumptionRate;

		// Token: 0x04001F38 RID: 7992
		private float meltDownTimer;

		// Token: 0x04001F39 RID: 7993
		private float meltDownDelay;

		// Token: 0x04001F3A RID: 7994
		private float fireTimer;

		// Token: 0x04001F3B RID: 7995
		private float fireDelay;

		// Token: 0x04001F3C RID: 7996
		private float maxPowerOutput;

		// Token: 0x04001F3D RID: 7997
		private float minUpdatePowerOut;

		// Token: 0x04001F3E RID: 7998
		private float maxUpdatePowerOut;

		// Token: 0x04001F3F RID: 7999
		private bool unsentChanges;

		// Token: 0x04001F40 RID: 8000
		private float sendUpdateTimer;

		// Token: 0x04001F41 RID: 8001
		private float degreeOfSuccess;

		// Token: 0x04001F42 RID: 8002
		private Vector2 optimalTemperature;

		// Token: 0x04001F43 RID: 8003
		private Vector2 allowedTemperature;

		// Token: 0x04001F44 RID: 8004
		private Vector2 optimalFissionRate;

		// Token: 0x04001F45 RID: 8005
		private Vector2 allowedFissionRate;

		// Token: 0x04001F46 RID: 8006
		private Vector2 optimalTurbineOutput;

		// Token: 0x04001F47 RID: 8007
		private Vector2 allowedTurbineOutput;

		// Token: 0x04001F48 RID: 8008
		private float? signalControlledTargetFissionRate;

		// Token: 0x04001F49 RID: 8009
		private float? signalControlledTargetTurbineOutput;

		// Token: 0x04001F4A RID: 8010
		private double lastReceivedFissionRateSignalTime;

		// Token: 0x04001F4B RID: 8011
		private double lastReceivedTurbineOutputSignalTime;

		// Token: 0x04001F4C RID: 8012
		private float temperatureBoost;

		// Token: 0x04001F4D RID: 8013
		private bool _powerOn;

		// Token: 0x04001F50 RID: 8016
		private Character lastUser;

		// Token: 0x04001F51 RID: 8017
		private float prevAvailableFuel;
	}
}
