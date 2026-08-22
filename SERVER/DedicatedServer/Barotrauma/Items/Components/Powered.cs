using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004DA RID: 1242
	internal class Powered : ItemComponent
	{
		// Token: 0x170012E4 RID: 4836
		// (get) Token: 0x06004673 RID: 18035 RVA: 0x001C1FEE File Offset: 0x001C01EE
		public static IEnumerable<Powered> PoweredList
		{
			get
			{
				return Powered.poweredList;
			}
		}

		// Token: 0x170012E5 RID: 4837
		// (get) Token: 0x06004674 RID: 18036 RVA: 0x001C1FF8 File Offset: 0x001C01F8
		protected Connection powerOut
		{
			get
			{
				if (this.powerOuts.Count > 1)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(17, 1);
					defaultInterpolatedStringHandler.AppendFormatted<ushort>(this.item.ID);
					defaultInterpolatedStringHandler.AppendLiteral(".multiplePowerOut");
					string identifier = defaultInterpolatedStringHandler.ToStringAndClear();
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(59, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Item ");
					defaultInterpolatedStringHandler2.AppendFormatted(this.item.Name);
					defaultInterpolatedStringHandler2.AppendLiteral(" (");
					defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.item.Prefab.Identifier);
					defaultInterpolatedStringHandler2.AppendLiteral(") has multiple power outputs, but only supports one!");
					DebugConsole.ThrowErrorOnce(identifier, defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				}
				return this.powerOuts.FirstOrDefault<Connection>();
			}
		}

		// Token: 0x170012E6 RID: 4838
		// (get) Token: 0x06004675 RID: 18037 RVA: 0x001C20B4 File Offset: 0x001C02B4
		protected bool powerInIsPowerOut
		{
			get
			{
				return this.powerOuts.Contains(this.powerIn);
			}
		}

		// Token: 0x170012E7 RID: 4839
		// (get) Token: 0x06004676 RID: 18038 RVA: 0x001C20C7 File Offset: 0x001C02C7
		protected virtual PowerPriority Priority
		{
			get
			{
				return PowerPriority.Default;
			}
		}

		// Token: 0x170012E8 RID: 4840
		// (get) Token: 0x06004677 RID: 18039 RVA: 0x001C20CA File Offset: 0x001C02CA
		// (set) Token: 0x06004678 RID: 18040 RVA: 0x001C20E5 File Offset: 0x001C02E5
		[Header("", "sp.powered.propertyheader")]
		[Editable]
		[Serialize(0.5f, IsPropertySaveable.Yes, "The minimum voltage required for the device to function. The voltage is calculated as power / powerconsumption, meaning that a device with a power consumption of 1000 kW would need at least 500 kW of power to work if the minimum voltage is set to 0.5.", "", false)]
		public float MinVoltage
		{
			get
			{
				if (this.powerConsumption > 0f)
				{
					return this.minVoltage;
				}
				return 0f;
			}
			set
			{
				this.minVoltage = value;
			}
		}

		// Token: 0x170012E9 RID: 4841
		// (get) Token: 0x06004679 RID: 18041 RVA: 0x001C20EE File Offset: 0x001C02EE
		// (set) Token: 0x0600467A RID: 18042 RVA: 0x001C20F6 File Offset: 0x001C02F6
		[Editable]
		[Serialize(0f, IsPropertySaveable.Yes, "How much power the device draws (or attempts to draw) from the electrical grid when active.", "", false)]
		public float PowerConsumption
		{
			get
			{
				return this.powerConsumption;
			}
			set
			{
				this.powerConsumption = value;
			}
		}

		// Token: 0x170012EA RID: 4842
		// (get) Token: 0x0600467B RID: 18043 RVA: 0x001C20FF File Offset: 0x001C02FF
		// (set) Token: 0x0600467C RID: 18044 RVA: 0x001C2107 File Offset: 0x001C0307
		[Serialize(false, IsPropertySaveable.Yes, "Is the device currently active. Inactive devices don't consume power.", "", false)]
		public override bool IsActive
		{
			get
			{
				return base.IsActive;
			}
			set
			{
				base.IsActive = value;
				if (!value)
				{
					this.currPowerConsumption = 0f;
				}
			}
		}

		// Token: 0x170012EB RID: 4843
		// (get) Token: 0x0600467D RID: 18045 RVA: 0x001C211E File Offset: 0x001C031E
		// (set) Token: 0x0600467E RID: 18046 RVA: 0x001C2126 File Offset: 0x001C0326
		[Serialize(0f, IsPropertySaveable.Yes, "The current power consumption of the device. Intended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public float CurrPowerConsumption
		{
			get
			{
				return this.currPowerConsumption;
			}
			set
			{
				this.currPowerConsumption = value;
			}
		}

		// Token: 0x170012EC RID: 4844
		// (get) Token: 0x0600467F RID: 18047 RVA: 0x001C2130 File Offset: 0x001C0330
		// (set) Token: 0x06004680 RID: 18048 RVA: 0x001C2217 File Offset: 0x001C0417
		[Serialize(0f, IsPropertySaveable.Yes, "The current voltage of the item (calculated as power consumption / available power). Intended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public float Voltage
		{
			get
			{
				if (this.PoweredByTinkering)
				{
					return 1f;
				}
				if (this.powerIn != null)
				{
					Connection connection = this.powerIn;
					if (((connection != null) ? connection.Grid : null) != null)
					{
						return this.powerIn.Grid.Voltage;
					}
				}
				else if (this.powerOuts.Any<Connection>())
				{
					IEnumerable<Connection> gridConnections = from conn in this.powerOuts
					where conn.Grid != null
					select conn;
					if (gridConnections.Any<Connection>())
					{
						return gridConnections.Average((Connection conn) => conn.Grid.Voltage);
					}
				}
				if (this is PowerTransfer && this.item.Condition <= 0f)
				{
					return 0f;
				}
				if (this.PowerConsumption > 0f)
				{
					return this.voltage;
				}
				return 1f;
			}
			set
			{
				this.voltage = Math.Max(0f, value);
			}
		}

		// Token: 0x170012ED RID: 4845
		// (get) Token: 0x06004681 RID: 18049 RVA: 0x001C222A File Offset: 0x001C042A
		public float RelativeVoltage
		{
			get
			{
				if (this.minVoltage > 0f)
				{
					return MathHelper.Clamp(this.Voltage / this.minVoltage, 0f, 1f);
				}
				return 1f;
			}
		}

		// Token: 0x170012EE RID: 4846
		// (get) Token: 0x06004682 RID: 18050 RVA: 0x001C225B File Offset: 0x001C045B
		public virtual bool HasPower
		{
			get
			{
				return this.Voltage >= this.MinVoltage;
			}
		}

		// Token: 0x170012EF RID: 4847
		// (get) Token: 0x06004683 RID: 18051 RVA: 0x001C226E File Offset: 0x001C046E
		// (set) Token: 0x06004684 RID: 18052 RVA: 0x001C2276 File Offset: 0x001C0476
		public bool PoweredByTinkering { get; set; }

		// Token: 0x170012F0 RID: 4848
		// (get) Token: 0x06004685 RID: 18053 RVA: 0x001C227F File Offset: 0x001C047F
		// (set) Token: 0x06004686 RID: 18054 RVA: 0x001C2287 File Offset: 0x001C0487
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the item be damaged by electomagnetic pulses.", "", false)]
		public bool VulnerableToEMP { get; set; }

		// Token: 0x06004687 RID: 18055 RVA: 0x001C2290 File Offset: 0x001C0490
		public Powered(Item item, ContentXElement element) : base(item, element)
		{
			Powered.poweredList.Add(this);
		}

		// Token: 0x06004688 RID: 18056 RVA: 0x001C22B0 File Offset: 0x001C04B0
		protected void UpdateOnActiveEffects(float deltaTime)
		{
			if (this.currPowerConsumption <= 0f && this.PowerConsumption <= 0f)
			{
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
				return;
			}
			if (this.Voltage > this.minVoltage)
			{
				base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			}
		}

		// Token: 0x06004689 RID: 18057 RVA: 0x001C231A File Offset: 0x001C051A
		public override void Update(float deltaTime, Camera cam)
		{
			this.UpdateOnActiveEffects(deltaTime);
		}

		// Token: 0x0600468A RID: 18058 RVA: 0x001C2324 File Offset: 0x001C0524
		public override void OnItemLoaded()
		{
			if (this.item.Connections == null)
			{
				return;
			}
			foreach (Connection c in this.item.Connections)
			{
				if (c.IsPower)
				{
					PowerTransfer pt = this as PowerTransfer;
					if (pt != null)
					{
						if (c.Name == "power_in")
						{
							this.powerIn = c;
						}
						else if (c.Name == "power")
						{
							this.powerIn = c;
							this.powerOuts.Add(c);
						}
						else if (c.IsOutput)
						{
							this.powerOuts.Add(c);
							if (this.Priority > c.Priority)
							{
								c.Priority = this.Priority;
							}
						}
					}
					else if (c.IsOutput)
					{
						if (c.Name == "power_in")
						{
							DebugConsole.NewMessage("Item \"" + this.item.Name + "\" has a power output connection called power_in. If the item is supposed to receive power through the connection, change it to an input connection.", new Color?(Color.Orange), false);
						}
						this.powerOuts.Add(c);
						if (this.Priority > c.Priority)
						{
							c.Priority = this.Priority;
						}
					}
					else
					{
						if (c.Name == "power_out")
						{
							DebugConsole.NewMessage("Item \"" + this.item.Name + "\" has a power input connection called power_out. If the item is supposed to output power through the connection, change it to an output connection.", new Color?(Color.Orange), false);
						}
						this.powerIn = c;
					}
				}
			}
		}

		// Token: 0x0600468B RID: 18059 RVA: 0x001C24E0 File Offset: 0x001C06E0
		public static void UpdateGrids(bool useCache = true)
		{
			if (Powered.Grids.Count > 0 && useCache)
			{
				foreach (Connection c in Powered.ChangedConnections)
				{
					if (c.Grid != null)
					{
						Powered.Grids.Remove(c.Grid.ID);
						c.Grid = null;
					}
				}
				using (HashSet<Connection>.Enumerator enumerator2 = Powered.ChangedConnections.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Connection c2 = enumerator2.Current;
						if (c2.Grid == null && c2.Recipients.Count > 0 && c2.Item.Condition > 0f)
						{
							GridInfo grid = Powered.PropagateGrid(c2);
							Powered.Grids[grid.ID] = grid;
						}
					}
					goto IL_270;
				}
			}
			foreach (Powered powered in Powered.poweredList)
			{
				if (powered.powerIn != null)
				{
					powered.powerIn.Grid = null;
				}
				foreach (Connection powerOut in powered.powerOuts)
				{
					powerOut.Grid = null;
				}
			}
			Powered.Grids.Clear();
			foreach (Powered powered2 in Powered.poweredList)
			{
				if (powered2.Item.Condition > 0f)
				{
					if (powered2.powerIn != null && powered2.powerIn.Grid == null && !powered2.powerInIsPowerOut && powered2.powerIn.Recipients.Count > 0)
					{
						GridInfo grid2 = Powered.PropagateGrid(powered2.powerIn);
						Powered.Grids[grid2.ID] = grid2;
					}
					foreach (Connection powerOut2 in powered2.powerOuts)
					{
						if (powerOut2 != null && powerOut2.Grid == null && powerOut2.Recipients.Count > 0)
						{
							GridInfo grid3 = Powered.PropagateGrid(powerOut2);
							Powered.Grids[grid3.ID] = grid3;
						}
					}
				}
			}
			IL_270:
			Powered.ChangedConnections.Clear();
		}

		// Token: 0x0600468C RID: 18060 RVA: 0x001C27B4 File Offset: 0x001C09B4
		private static GridInfo PropagateGrid(Connection conn)
		{
			int id = Rand.Int(int.MaxValue, Rand.RandSync.Unsynced);
			while (Powered.Grids.ContainsKey(id))
			{
				id = Rand.Int(int.MaxValue, Rand.RandSync.Unsynced);
			}
			return Powered.PropagateGrid(conn, id);
		}

		// Token: 0x0600468D RID: 18061 RVA: 0x001C27F0 File Offset: 0x001C09F0
		private static GridInfo PropagateGrid(Connection conn, int gridID)
		{
			Stack<Connection> probeStack = new Stack<Connection>();
			GridInfo grid = new GridInfo(gridID);
			probeStack.Push(conn);
			while (probeStack.Count > 0)
			{
				Connection c = probeStack.Pop();
				c.Grid = grid;
				grid.AddConnection(c);
				foreach (Connection otherC in c.Recipients)
				{
					if (otherC.Grid != grid && (otherC.Grid == null || !Powered.Grids.ContainsKey(otherC.Grid.ID)) && Powered.ValidPowerConnection(c, otherC))
					{
						otherC.Grid = grid;
						probeStack.Push(otherC);
					}
				}
			}
			return grid;
		}

		// Token: 0x0600468E RID: 18062 RVA: 0x001C28C0 File Offset: 0x001C0AC0
		public static void UpdatePower(float deltaTime)
		{
			if (GameMain.GameSession != null && GameMain.GameSession.RoundEnding)
			{
				return;
			}
			Powered.UpdateGrids(true);
			foreach (GridInfo grid in Powered.Grids.Values)
			{
				grid.PowerSourceGroups.Clear();
				grid.Power = 0f;
				grid.Load = 0f;
			}
			foreach (Powered powered in Powered.poweredList)
			{
				powered.Voltage -= deltaTime;
				if (powered.powerIn != null && !powered.powerInIsPowerOut)
				{
					float currLoad = powered.GetCurrentPowerConsumption(powered.powerIn);
					if (currLoad >= 0f)
					{
						if (powered.PoweredByTinkering)
						{
							currLoad = 0f;
						}
						powered.CurrPowerConsumption = currLoad;
						if (powered.powerIn.Grid != null)
						{
							powered.powerIn.Grid.Load += currLoad;
						}
					}
					else if (powered.powerIn.Grid != null)
					{
						powered.powerIn.Grid.AddSrc(powered.powerIn);
					}
					else
					{
						powered.CurrPowerConsumption = -powered.GetConnectionPowerOut(powered.powerIn, 0f, powered.MinMaxPowerOut(powered.powerIn, 0f), 0f);
						powered.GridResolved(powered.powerIn);
					}
				}
				foreach (Connection powerOut in powered.powerOuts)
				{
					float currLoad2 = powered.GetCurrentPowerConsumption(powerOut);
					PowerTransfer pt = powered as PowerTransfer;
					if (pt != null)
					{
						pt.PowerLoad = currLoad2;
					}
					else if (!(powered is PowerContainer))
					{
						powered.CurrPowerConsumption = currLoad2;
					}
					if (currLoad2 >= 0f)
					{
						if (powerOut.Grid != null)
						{
							powerOut.Grid.Load += currLoad2;
						}
					}
					else if (powerOut.Grid != null)
					{
						powerOut.Grid.AddSrc(powerOut);
					}
					else
					{
						float loadOut = -powered.GetConnectionPowerOut(powerOut, 0f, powered.MinMaxPowerOut(powerOut, 0f), 0f);
						PowerTransfer pt2 = powered as PowerTransfer;
						if (pt2 != null)
						{
							pt2.PowerLoad = loadOut;
						}
						else if (!(powered is PowerContainer))
						{
							powered.CurrPowerConsumption = loadOut;
						}
						powered.GridResolved(powerOut);
					}
				}
			}
			foreach (GridInfo grid2 in Powered.Grids.Values)
			{
				foreach (PowerSourceGroup scrGroup in grid2.PowerSourceGroups.Values)
				{
					scrGroup.MinMaxPower = PowerRange.Zero;
					foreach (Connection c in scrGroup.Connections)
					{
						foreach (Powered device in c.Item.GetComponents<Powered>())
						{
							scrGroup.MinMaxPower += device.MinMaxPowerOut(c, grid2.Load);
						}
					}
					float addedPower = 0f;
					foreach (Connection c2 in scrGroup.Connections)
					{
						foreach (Powered device2 in c2.Item.GetComponents<Powered>())
						{
							addedPower += device2.GetConnectionPowerOut(c2, grid2.Power, scrGroup.MinMaxPower, grid2.Load);
						}
					}
					grid2.Power += addedPower;
				}
				float newVoltage = MathHelper.Min(grid2.Power / MathHelper.Max(grid2.Load, 1E-10f), 1000f);
				if (float.IsNegative(newVoltage))
				{
					newVoltage = 0f;
				}
				grid2.Voltage = newVoltage;
				foreach (Connection c3 in grid2.Connections)
				{
					foreach (Powered device3 in c3.Item.GetComponents<Powered>())
					{
						if (device3 != null)
						{
							device3.GridResolved(c3);
						}
					}
				}
			}
		}

		// Token: 0x0600468F RID: 18063 RVA: 0x001C2EB8 File Offset: 0x001C10B8
		public virtual float GetCurrentPowerConsumption(Connection connection = null)
		{
			if (this.powerIn == null && this.powerOuts.None(null))
			{
				return 0f;
			}
			PowerTransfer pt = this as PowerTransfer;
			if (pt != null)
			{
				return this.PowerConsumption + pt.ExtraLoad;
			}
			if (connection != this.powerIn || !this.IsActive)
			{
				return 0f;
			}
			return this.PowerConsumption;
		}

		// Token: 0x06004690 RID: 18064 RVA: 0x001C2F16 File Offset: 0x001C1116
		public virtual PowerRange MinMaxPowerOut(Connection conn, float load = 0f)
		{
			return PowerRange.Zero;
		}

		// Token: 0x06004691 RID: 18065 RVA: 0x001C2F1D File Offset: 0x001C111D
		public virtual float GetConnectionPowerOut(Connection conn, float power, PowerRange minMaxPower, float load)
		{
			if (!this.powerOuts.Contains(conn))
			{
				return 0f;
			}
			return MathHelper.Max(-this.CurrPowerConsumption, 0f);
		}

		// Token: 0x06004692 RID: 18066 RVA: 0x001C2F44 File Offset: 0x001C1144
		public virtual void GridResolved(Connection conn)
		{
		}

		// Token: 0x06004693 RID: 18067 RVA: 0x001C2F48 File Offset: 0x001C1148
		public static bool ValidPowerConnection(Connection conn1, Connection conn2)
		{
			if (conn1.IsPower && conn2.IsPower && conn1.Item.Condition > 0f && conn2.Item.Condition > 0f)
			{
				PowerTransfer component = conn1.Item.GetComponent<PowerTransfer>();
				if (component == null || component.CanTransfer)
				{
					component = conn2.Item.GetComponent<PowerTransfer>();
					if (component == null || component.CanTransfer)
					{
						return conn1.Item.HasTag(Tags.JunctionBox) || conn2.Item.HasTag(Tags.JunctionBox) || conn1.Item.HasTag(Tags.DockingPort) || conn2.Item.HasTag(Tags.DockingPort) || conn1.IsOutput != conn2.IsOutput;
					}
				}
			}
			return false;
		}

		// Token: 0x06004694 RID: 18068 RVA: 0x001C3020 File Offset: 0x001C1220
		protected float GetAvailableInstantaneousBatteryPower()
		{
			if (this.item.Connections == null || this.powerIn == null)
			{
				return 0f;
			}
			float availablePower = 0f;
			List<Connection> recipients = this.powerIn.Recipients;
			foreach (Connection recipient in recipients)
			{
				if (recipient.IsPower && recipient.IsOutput)
				{
					Item item = recipient.Item;
					PowerContainer battery = (item != null) ? item.GetComponent<PowerContainer>() : null;
					if (battery != null && battery.Item.Condition > 0f && !battery.OutputDisabled)
					{
						float maxOutputPerFrame = battery.MaxOutPut / 60f;
						float framesPerMinute = 3600f;
						availablePower += Math.Min(battery.Charge * framesPerMinute, maxOutputPerFrame);
					}
				}
			}
			return availablePower;
		}

		// Token: 0x06004695 RID: 18069 RVA: 0x001C3104 File Offset: 0x001C1304
		protected IEnumerable<PowerContainer> GetDirectlyConnectedBatteries()
		{
			Powered.<GetDirectlyConnectedBatteries>d__61 <GetDirectlyConnectedBatteries>d__ = new Powered.<GetDirectlyConnectedBatteries>d__61(-2);
			<GetDirectlyConnectedBatteries>d__.<>4__this = this;
			return <GetDirectlyConnectedBatteries>d__;
		}

		// Token: 0x06004696 RID: 18070 RVA: 0x001C3114 File Offset: 0x001C1314
		protected override void RemoveComponentSpecific()
		{
			if (this.item.Connections != null)
			{
				foreach (Connection c in this.item.Connections)
				{
					if (c.IsPower && c.Grid != null)
					{
						Powered.ChangedConnections.Add(c);
					}
				}
			}
			base.RemoveComponentSpecific();
			Powered.poweredList.Remove(this);
		}

		// Token: 0x040021F9 RID: 8697
		protected const float UpdateInterval = 0.016666668f;

		// Token: 0x040021FA RID: 8698
		private static readonly List<Powered> poweredList = new List<Powered>();

		// Token: 0x040021FB RID: 8699
		public static readonly HashSet<Connection> ChangedConnections = new HashSet<Connection>();

		// Token: 0x040021FC RID: 8700
		public static readonly Dictionary<int, GridInfo> Grids = new Dictionary<int, GridInfo>();

		// Token: 0x040021FD RID: 8701
		protected float currPowerConsumption;

		// Token: 0x040021FE RID: 8702
		private float voltage;

		// Token: 0x040021FF RID: 8703
		private float minVoltage;

		// Token: 0x04002200 RID: 8704
		protected float powerConsumption;

		// Token: 0x04002201 RID: 8705
		protected Connection powerIn;

		// Token: 0x04002202 RID: 8706
		protected List<Connection> powerOuts = new List<Connection>();

		// Token: 0x04002203 RID: 8707
		protected const float MaxOverVoltageFactor = 2f;
	}
}
