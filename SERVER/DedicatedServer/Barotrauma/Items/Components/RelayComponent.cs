using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004FA RID: 1274
	internal class RelayComponent : PowerTransfer, IServerSerializable, INetSerializable
	{
		// Token: 0x1700133B RID: 4923
		// (get) Token: 0x06004797 RID: 18327 RVA: 0x001C7DE3 File Offset: 0x001C5FE3
		protected override PowerPriority Priority
		{
			get
			{
				return PowerPriority.Relay;
			}
		}

		// Token: 0x1700133C RID: 4924
		// (get) Token: 0x06004798 RID: 18328 RVA: 0x001C7DE6 File Offset: 0x001C5FE6
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

		// Token: 0x1700133D RID: 4925
		// (get) Token: 0x06004799 RID: 18329 RVA: 0x001C7E13 File Offset: 0x001C6013
		// (set) Token: 0x0600479A RID: 18330 RVA: 0x001C7E1B File Offset: 0x001C601B
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

		// Token: 0x1700133E RID: 4926
		// (get) Token: 0x0600479B RID: 18331 RVA: 0x001C7E34 File Offset: 0x001C6034
		// (set) Token: 0x0600479C RID: 18332 RVA: 0x001C7E3C File Offset: 0x001C603C
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

		// Token: 0x0600479D RID: 18333 RVA: 0x001C7E58 File Offset: 0x001C6058
		public RelayComponent(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.prevVoltage = 0f;
			this.SetLoadFormulaValues();
		}

		// Token: 0x0600479E RID: 18334 RVA: 0x001C7E7C File Offset: 0x001C607C
		private void SetLoadFormulaValues()
		{
			this.internalLoadBuffer = this.MaxPower * 2f;
			this.thirdInverseMax = 1f / (3f * this.maxPower);
			this.loadEqnConstant = 8f * this.maxPower / 3f;
		}

		// Token: 0x0600479F RID: 18335 RVA: 0x001C7ECC File Offset: 0x001C60CC
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

		// Token: 0x060047A0 RID: 18336 RVA: 0x001C8054 File Offset: 0x001C6254
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

		// Token: 0x060047A1 RID: 18337 RVA: 0x001C8140 File Offset: 0x001C6340
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

		// Token: 0x060047A2 RID: 18338 RVA: 0x001C81F4 File Offset: 0x001C63F4
		private bool RelayCanOutput()
		{
			return this.isOn && this.powerIn != null && this.powerIn.Grid != null && this.internalLoadBuffer > 0f && base.powerOut != null && base.powerOut.Grid != null && this.powerIn.Grid != base.powerOut.Grid;
		}

		// Token: 0x060047A3 RID: 18339 RVA: 0x001C8260 File Offset: 0x001C6460
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

		// Token: 0x060047A4 RID: 18340 RVA: 0x001C82D4 File Offset: 0x001C64D4
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

		// Token: 0x060047A5 RID: 18341 RVA: 0x001C8394 File Offset: 0x001C6594
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

		// Token: 0x060047A6 RID: 18342 RVA: 0x001C84AC File Offset: 0x001C66AC
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

		// Token: 0x060047A7 RID: 18343 RVA: 0x001C8574 File Offset: 0x001C6774
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

		// Token: 0x060047A8 RID: 18344 RVA: 0x001C8626 File Offset: 0x001C6826
		public void SetState(bool on, bool isNetworkMessage)
		{
			if (on != this.IsOn && GameMain.Server != null)
			{
				this.item.CreateServerEvent<RelayComponent>(this);
			}
			this.IsOn = on;
		}

		// Token: 0x060047A9 RID: 18345 RVA: 0x001C864B File Offset: 0x001C684B
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteBoolean(this.isOn);
		}

		// Token: 0x060047AA RID: 18346 RVA: 0x001C8659 File Offset: 0x001C6859
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			this.SetState(msg.ReadBoolean(), true);
		}

		// Token: 0x04002293 RID: 8851
		private float maxPower;

		// Token: 0x04002294 RID: 8852
		private bool isOn;

		// Token: 0x04002295 RID: 8853
		private float prevVoltage;

		// Token: 0x04002296 RID: 8854
		private float? newVoltage;

		// Token: 0x04002297 RID: 8855
		private float internalLoadBuffer;

		// Token: 0x04002298 RID: 8856
		private float prevInternalLoad;

		// Token: 0x04002299 RID: 8857
		private float prevExternalLoad;

		// Token: 0x0400229A RID: 8858
		private float bufferDiff;

		// Token: 0x0400229B RID: 8859
		private float thirdInverseMax;

		// Token: 0x0400229C RID: 8860
		private float loadEqnConstant;

		// Token: 0x0400229D RID: 8861
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
