using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x0200061D RID: 1565
	internal class RelayComponent : PowerTransfer, IServerSerializable, INetSerializable
	{
		// Token: 0x1700196A RID: 6506
		// (get) Token: 0x06006467 RID: 25703 RVA: 0x00340AAF File Offset: 0x0033ECAF
		protected override PowerPriority Priority
		{
			get
			{
				return PowerPriority.Relay;
			}
		}

		// Token: 0x1700196B RID: 6507
		// (get) Token: 0x06006468 RID: 25704 RVA: 0x00340AB2 File Offset: 0x0033ECB2
		public float DisplayLoad
		{
			get
			{
				if (base.powerOut != null && base.powerOut.Grid != null)
				{
					return base.powerOut.Grid.Load;
				}
				return 0f;
			}
		}

		// Token: 0x1700196C RID: 6508
		// (get) Token: 0x06006469 RID: 25705 RVA: 0x00340ADF File Offset: 0x0033ECDF
		// (set) Token: 0x0600646A RID: 25706 RVA: 0x00340AE7 File Offset: 0x0033ECE7
		[Editable]
		[Serialize(1000f, IsPropertySaveable.Yes, "The maximum amount of power that can pass through the item.", "", false)]
		public float MaxPower
		{
			get
			{
				return this.maxPower;
			}
			set
			{
				this.maxPower = Math.Max(0f, value);
				this.SetLoadFormulaValues();
			}
		}

		// Token: 0x1700196D RID: 6509
		// (get) Token: 0x0600646B RID: 25707 RVA: 0x00340B00 File Offset: 0x0033ED00
		// (set) Token: 0x0600646C RID: 25708 RVA: 0x00340B08 File Offset: 0x0033ED08
		[InGameEditable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the relay currently pass power and signals through it.", "", true)]
		public bool IsOn
		{
			get
			{
				return this.isOn;
			}
			set
			{
				this.isOn = value;
				if (!this.isOn)
				{
					this.currPowerConsumption = 0f;
				}
			}
		}

		// Token: 0x0600646D RID: 25709 RVA: 0x00340B24 File Offset: 0x0033ED24
		public RelayComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.prevVoltage = 0f;
			this.SetLoadFormulaValues();
		}

		// Token: 0x0600646E RID: 25710 RVA: 0x00340B48 File Offset: 0x0033ED48
		private void SetLoadFormulaValues()
		{
			this.internalLoadBuffer = this.MaxPower * 2f;
			this.thirdInverseMax = 1f / (3f * this.maxPower);
			this.loadEqnConstant = 8f * this.maxPower / 3f;
		}

		// Token: 0x0600646F RID: 25711 RVA: 0x00340B98 File Offset: 0x0033ED98
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			List<Connection> connections = base.Item.Connections;
			if (connections != null)
			{
				using (Dictionary<string, string>.Enumerator enumerator = RelayComponent.connectionPairs.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						KeyValuePair<string, string> connectionPair = enumerator.Current;
						if (connections.Any((Connection c) => c.Name == connectionPair.Key) && !connections.Any((Connection c) => c.Name == connectionPair.Value))
						{
							DebugConsole.ThrowError(string.Concat(new string[]
							{
								"Error in item \"",
								base.Name,
								"\" - matching connection pair not found for the connection \"",
								connectionPair.Key,
								"\" (expecting \"",
								connectionPair.Value,
								"\")."
							}), null, null, false, false);
						}
						else if (connections.Any((Connection c) => c.Name == connectionPair.Value) && !connections.Any((Connection c) => c.Name == connectionPair.Key))
						{
							DebugConsole.ThrowError(string.Concat(new string[]
							{
								"Error in item \"",
								base.Name,
								"\" - matching connection pair not found for the connection \"",
								connectionPair.Value,
								"\" (expecting \"",
								connectionPair.Key,
								"\")."
							}), null, null, false, false);
						}
					}
				}
			}
		}

		// Token: 0x06006470 RID: 25712 RVA: 0x00340D20 File Offset: 0x0033EF20
		public override void Update(float deltaTime, Camera cam)
		{
			base.RefreshConnections();
			this.item.SendSignal(this.IsOn ? "1" : "0", "state_out");
			this.item.SendSignal(((int)Math.Round((double)(-(double)base.PowerLoad))).ToString(), "power_value_out");
			this.item.SendSignal(((int)Math.Round((double)this.DisplayLoad)).ToString(), "load_value_out");
			if (this.isBroken)
			{
				base.SetAllConnectionsDirty();
				this.isBroken = false;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			if (base.Voltage > base.OverloadVoltage && base.CanBeOverloaded && this.item.Repairables.Any<Repairable>())
			{
				this.item.Condition = 0f;
			}
		}

		// Token: 0x06006471 RID: 25713 RVA: 0x00340E0C File Offset: 0x0033F00C
		public override float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (this.isBroken)
			{
				return 0f;
			}
			if (connection == this.powerIn)
			{
				float currentLoad = this.MaxPower;
				if (this.internalLoadBuffer > this.MaxPower)
				{
					currentLoad = MathHelper.Clamp(this.internalLoadBuffer * this.internalLoadBuffer * this.thirdInverseMax - 2f * this.internalLoadBuffer + this.loadEqnConstant, 0.001f, this.MaxPower);
					currentLoad = MathHelper.Clamp((currentLoad + this.prevInternalLoad * 0.1f) / 1.1f, 0.001f, this.MaxPower);
					this.prevInternalLoad = currentLoad;
				}
				return currentLoad + base.ExtraLoad;
			}
			return -1f;
		}

		// Token: 0x06006472 RID: 25714 RVA: 0x00340EC0 File Offset: 0x0033F0C0
		private bool RelayCanOutput()
		{
			return this.isOn && this.powerIn != null && this.powerIn.Grid != null && this.internalLoadBuffer > 0f && base.powerOut != null && base.powerOut.Grid != null && this.powerIn.Grid != base.powerOut.Grid;
		}

		// Token: 0x06006473 RID: 25715 RVA: 0x00340F2C File Offset: 0x0033F12C
		public override PowerRange MinMaxPowerOut(Connection connection, float load = 0f)
		{
			if (connection == base.powerOut && this.RelayCanOutput())
			{
				float bufferDraw = MathHelper.Min(this.internalLoadBuffer, this.MaxPower);
				float voltageLimit = MathHelper.Min(this.MaxPower, load) * this.prevVoltage;
				if (this.prevVoltage < 1f)
				{
					voltageLimit *= this.prevVoltage;
				}
				float maxOutput = MathHelper.Min(voltageLimit, bufferDraw);
				return new PowerRange(0f, maxOutput);
			}
			return PowerRange.Zero;
		}

		// Token: 0x06006474 RID: 25716 RVA: 0x00340FA0 File Offset: 0x0033F1A0
		public override float GetConnectionPowerOut(Connection connection, float power, PowerRange minMaxPower, float load)
		{
			if (connection == this.powerIn)
			{
				return 0f;
			}
			if (!this.RelayCanOutput())
			{
				base.PowerLoad = 0f;
				return 0f;
			}
			float bufferDraw = MathHelper.Min(this.internalLoadBuffer, this.MaxPower);
			float voltageLimit = MathHelper.Min(this.MaxPower, load) * this.prevVoltage;
			float maxOut = MathHelper.Min(voltageLimit, bufferDraw);
			if (maxOut < 0f)
			{
				base.PowerLoad = 0f;
				return 0f;
			}
			this.prevExternalLoad = load;
			base.PowerLoad = MathHelper.Clamp((load * this.prevVoltage - power) / MathHelper.Max(minMaxPower.Max, 1E-20f) * -maxOut, -maxOut, 0f);
			return -base.PowerLoad;
		}

		// Token: 0x06006475 RID: 25717 RVA: 0x00341060 File Offset: 0x0033F260
		public override void GridResolved(Connection conn)
		{
			if (conn == this.powerIn)
			{
				if (this.powerIn != null && this.powerIn.Grid != null)
				{
					float addToBuffer = this.powerIn.Grid.Voltage * (base.CurrPowerConsumption - base.ExtraLoad);
					if (this.powerIn.Grid.Voltage > 1f)
					{
						addToBuffer = this.prevVoltage * (base.CurrPowerConsumption - base.ExtraLoad);
					}
					if (addToBuffer > this.MaxPower)
					{
						addToBuffer = this.MaxPower;
					}
					if (this.newVoltage == null)
					{
						this.newVoltage = new float?(this.powerIn.Grid.Voltage);
						this.bufferDiff = addToBuffer;
						return;
					}
					this.UpdateBuffer(addToBuffer, this.powerIn.Grid.Voltage);
					return;
				}
			}
			else
			{
				if (this.newVoltage == null)
				{
					this.newVoltage = new float?((float)-1);
					this.bufferDiff = base.PowerLoad;
					return;
				}
				this.UpdateBuffer(base.PowerLoad, this.newVoltage.Value);
			}
		}

		// Token: 0x06006476 RID: 25718 RVA: 0x00341178 File Offset: 0x0033F378
		private void UpdateBuffer(float addToBuffer, float newVoltage)
		{
			float limit = this.MaxPower * 2f;
			if (this.RelayCanOutput() && this.powerIn.Grid.Voltage > 2f)
			{
				limit = MathHelper.Min(limit, 3f * this.MaxPower - (float)Math.Sqrt((double)((3f * this.prevExternalLoad + this.MaxPower) * this.MaxPower)));
			}
			this.internalLoadBuffer = MathHelper.Clamp(this.internalLoadBuffer + this.bufferDiff + addToBuffer, 0f, limit);
			if (newVoltage > 1f)
			{
				newVoltage = MathHelper.Max(newVoltage - 0.0005f, 1f);
			}
			this.prevVoltage = newVoltage;
			this.newVoltage = null;
			this.bufferDiff = 0f;
		}

		// Token: 0x06006477 RID: 25719 RVA: 0x00341240 File Offset: 0x0033F440
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.item.Condition <= 0f || connection.IsPower)
			{
				return;
			}
			string outConnection;
			if (RelayComponent.connectionPairs.TryGetValue(connection.Name, out outConnection))
			{
				if (!this.IsOn)
				{
					return;
				}
				this.item.SendSignal(signal, outConnection);
				return;
			}
			else
			{
				if (!(connection.Name == "toggle"))
				{
					if (connection.Name == "set_state")
					{
						this.SetState(signal.value != "0", false);
					}
					return;
				}
				if (signal.value == "0")
				{
					return;
				}
				this.SetState(!this.IsOn, false);
				return;
			}
		}

		// Token: 0x06006478 RID: 25720 RVA: 0x003412F2 File Offset: 0x0033F4F2
		public void SetState(bool on, bool isNetworkMessage)
		{
			if (GameMain.Client != null && !isNetworkMessage)
			{
				return;
			}
			this.IsOn = on;
		}

		// Token: 0x06006479 RID: 25721 RVA: 0x00341306 File Offset: 0x0033F506
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.isOn);
		}

		// Token: 0x0600647A RID: 25722 RVA: 0x00341314 File Offset: 0x0033F514
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.SetState(msg.ReadBoolean(), true);
		}

		// Token: 0x04003417 RID: 13335
		private float maxPower;

		// Token: 0x04003418 RID: 13336
		private bool isOn;

		// Token: 0x04003419 RID: 13337
		private float prevVoltage;

		// Token: 0x0400341A RID: 13338
		private float? newVoltage;

		// Token: 0x0400341B RID: 13339
		private float internalLoadBuffer;

		// Token: 0x0400341C RID: 13340
		private float prevInternalLoad;

		// Token: 0x0400341D RID: 13341
		private float prevExternalLoad;

		// Token: 0x0400341E RID: 13342
		private float bufferDiff;

		// Token: 0x0400341F RID: 13343
		private float thirdInverseMax;

		// Token: 0x04003420 RID: 13344
		private float loadEqnConstant;

		// Token: 0x04003421 RID: 13345
		private static readonly Dictionary<string, string> connectionPairs = new Dictionary<string, string>
		{
			{
				"power_in",
				"power_out"
			},
			{
				"signal_in",
				"signal_out"
			},
			{
				"signal_in1",
				"signal_out1"
			},
			{
				"signal_in2",
				"signal_out2"
			},
			{
				"signal_in3",
				"signal_out3"
			},
			{
				"signal_in4",
				"signal_out4"
			},
			{
				"signal_in5",
				"signal_out5"
			}
		};
	}
}
