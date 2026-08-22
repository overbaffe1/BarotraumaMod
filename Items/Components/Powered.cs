using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005CD RID: 1485
	internal class Powered : ItemComponent
	{
		// Token: 0x170017B7 RID: 6071
		// (get) Token: 0x06005E3C RID: 24124 RVA: 0x003120B7 File Offset: 0x003102B7
		public static IEnumerable<Powered> PoweredList
		{
			get
			{
				return Powered.poweredList;
			}
		}

		// Token: 0x170017B8 RID: 6072
		// (get) Token: 0x06005E3D RID: 24125 RVA: 0x003120C0 File Offset: 0x003102C0
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

		// Token: 0x170017B9 RID: 6073
		// (get) Token: 0x06005E3E RID: 24126 RVA: 0x0031217C File Offset: 0x0031037C
		protected bool powerInIsPowerOut
		{
			get
			{
				return this.powerOuts.Contains(this.powerIn);
			}
		}

		// Token: 0x170017BA RID: 6074
		// (get) Token: 0x06005E3F RID: 24127 RVA: 0x0031218F File Offset: 0x0031038F
		protected virtual PowerPriority Priority
		{
			get
			{
				return PowerPriority.Default;
			}
		}

		// Token: 0x170017BB RID: 6075
		// (get) Token: 0x06005E40 RID: 24128 RVA: 0x00312192 File Offset: 0x00310392
		// (set) Token: 0x06005E41 RID: 24129 RVA: 0x003121AD File Offset: 0x003103AD
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

		// Token: 0x170017BC RID: 6076
		// (get) Token: 0x06005E42 RID: 24130 RVA: 0x003121B6 File Offset: 0x003103B6
		// (set) Token: 0x06005E43 RID: 24131 RVA: 0x003121BE File Offset: 0x003103BE
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

		// Token: 0x170017BD RID: 6077
		// (get) Token: 0x06005E44 RID: 24132 RVA: 0x003121C7 File Offset: 0x003103C7
		// (set) Token: 0x06005E45 RID: 24133 RVA: 0x003121CF File Offset: 0x003103CF
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

		// Token: 0x170017BE RID: 6078
		// (get) Token: 0x06005E46 RID: 24134 RVA: 0x003121E6 File Offset: 0x003103E6
		// (set) Token: 0x06005E47 RID: 24135 RVA: 0x003121EE File Offset: 0x003103EE
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

		// Token: 0x170017BF RID: 6079
		// (get) Token: 0x06005E48 RID: 24136 RVA: 0x003121F8 File Offset: 0x003103F8
		// (set) Token: 0x06005E49 RID: 24137 RVA: 0x003122DF File Offset: 0x003104DF
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

		// Token: 0x170017C0 RID: 6080
		// (get) Token: 0x06005E4A RID: 24138 RVA: 0x003122F2 File Offset: 0x003104F2
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

		// Token: 0x170017C1 RID: 6081
		// (get) Token: 0x06005E4B RID: 24139 RVA: 0x00312323 File Offset: 0x00310523
		public virtual bool HasPower
		{
			get
			{
				return this.Voltage >= this.MinVoltage;
			}
		}

		// Token: 0x170017C2 RID: 6082
		// (get) Token: 0x06005E4C RID: 24140 RVA: 0x00312336 File Offset: 0x00310536
		// (set) Token: 0x06005E4D RID: 24141 RVA: 0x0031233E File Offset: 0x0031053E
		public bool PoweredByTinkering { get; set; }

		// Token: 0x170017C3 RID: 6083
		// (get) Token: 0x06005E4E RID: 24142 RVA: 0x00312347 File Offset: 0x00310547
		// (set) Token: 0x06005E4F RID: 24143 RVA: 0x0031234F File Offset: 0x0031054F
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the item be damaged by electomagnetic pulses.", "", false)]
		public bool VulnerableToEMP { get; set; }

		// Token: 0x06005E50 RID: 24144 RVA: 0x00312358 File Offset: 0x00310558
		public Powered(Item item, ContentXElement element) : base(item, element)
		{
			Powered.poweredList.Add(this);
			this.InitProjectSpecific(element);
		}

		// Token: 0x06005E51 RID: 24145 RVA: 0x00312380 File Offset: 0x00310580
		private void InitProjectSpecific(ContentXElement element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "poweronsound")
				{
					this.powerOnSound = RoundSound.Load(subElement);
				}
			}
		}

		// Token: 0x06005E52 RID: 24146 RVA: 0x003123F0 File Offset: 0x003105F0
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
			if (this.Voltage > this.minVoltage)
			{
				if (!this.powerOnSoundPlayed && this.powerOnSound != null)
				{
					RoundSound sound = this.powerOnSound;
					Vector2 worldPosition = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					SoundPlayer.PlaySound(sound, worldPosition, null, currentHull);
					this.powerOnSoundPlayed = true;
					return;
				}
			}
			else if (this.Voltage < 0.1f)
			{
				this.powerOnSoundPlayed = false;
			}
		}

		// Token: 0x06005E53 RID: 24147 RVA: 0x003124C1 File Offset: 0x003106C1
		public override void Update(float deltaTime, Camera cam)
		{
			this.UpdateOnActiveEffects(deltaTime);
		}

		// Token: 0x06005E54 RID: 24148 RVA: 0x003124CC File Offset: 0x003106CC
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

		// Token: 0x06005E55 RID: 24149 RVA: 0x00312688 File Offset: 0x00310888
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

		// Token: 0x06005E56 RID: 24150 RVA: 0x0031295C File Offset: 0x00310B5C
		private static GridInfo PropagateGrid(Connection conn)
		{
			int id = Rand.Int(int.MaxValue, Rand.RandSync.Unsynced);
			while (Powered.Grids.ContainsKey(id))
			{
				id = Rand.Int(int.MaxValue, Rand.RandSync.Unsynced);
			}
			return Powered.PropagateGrid(conn, id);
		}

		// Token: 0x06005E57 RID: 24151 RVA: 0x00312998 File Offset: 0x00310B98
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

		// Token: 0x06005E58 RID: 24152 RVA: 0x00312A68 File Offset: 0x00310C68
		public static void UpdatePower(float deltaTime)
		{
			if (GameMain.GameSession != null && GameMain.GameSession.RoundEnding)
			{
				return;
			}
			Stopwatch sw = new Stopwatch();
			sw.Start();
			Powered.UpdateGrids(true);
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:Power", sw.ElapsedTicks);
			sw.Restart();
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
			sw.Stop();
			GameMain.PerformanceCounter.AddElapsedTicks("Update:Power", sw.ElapsedTicks);
		}

		// Token: 0x06005E59 RID: 24153 RVA: 0x003130CC File Offset: 0x003112CC
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

		// Token: 0x06005E5A RID: 24154 RVA: 0x0031312A File Offset: 0x0031132A
		public virtual PowerRange MinMaxPowerOut(Connection conn, float load = 0f)
		{
			return PowerRange.Zero;
		}

		// Token: 0x06005E5B RID: 24155 RVA: 0x00313131 File Offset: 0x00311331
		public virtual float GetConnectionPowerOut(Connection conn, float power, PowerRange minMaxPower, float load)
		{
			if (!this.powerOuts.Contains(conn))
			{
				return 0f;
			}
			return MathHelper.Max(-this.CurrPowerConsumption, 0f);
		}

		// Token: 0x06005E5C RID: 24156 RVA: 0x00313158 File Offset: 0x00311358
		public virtual void GridResolved(Connection conn)
		{
		}

		// Token: 0x06005E5D RID: 24157 RVA: 0x0031315C File Offset: 0x0031135C
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

		// Token: 0x06005E5E RID: 24158 RVA: 0x00313234 File Offset: 0x00311434
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

		// Token: 0x06005E5F RID: 24159 RVA: 0x00313318 File Offset: 0x00311518
		protected IEnumerable<PowerContainer> GetDirectlyConnectedBatteries()
		{
			Powered.<GetDirectlyConnectedBatteries>d__63 <GetDirectlyConnectedBatteries>d__ = new Powered.<GetDirectlyConnectedBatteries>d__63(-2);
			<GetDirectlyConnectedBatteries>d__.<>4__this = this;
			return <GetDirectlyConnectedBatteries>d__;
		}

		// Token: 0x06005E60 RID: 24160 RVA: 0x00313328 File Offset: 0x00311528
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

		// Token: 0x040030AC RID: 12460
		private RoundSound powerOnSound;

		// Token: 0x040030AD RID: 12461
		private bool powerOnSoundPlayed;

		// Token: 0x040030AE RID: 12462
		protected const float UpdateInterval = 0.016666668f;

		// Token: 0x040030AF RID: 12463
		private static readonly List<Powered> poweredList = new List<Powered>();

		// Token: 0x040030B0 RID: 12464
		public static readonly HashSet<Connection> ChangedConnections = new HashSet<Connection>();

		// Token: 0x040030B1 RID: 12465
		public static readonly Dictionary<int, GridInfo> Grids = new Dictionary<int, GridInfo>();

		// Token: 0x040030B2 RID: 12466
		protected float currPowerConsumption;

		// Token: 0x040030B3 RID: 12467
		private float voltage;

		// Token: 0x040030B4 RID: 12468
		private float minVoltage;

		// Token: 0x040030B5 RID: 12469
		protected float powerConsumption;

		// Token: 0x040030B6 RID: 12470
		protected Connection powerIn;

		// Token: 0x040030B7 RID: 12471
		protected List<Connection> powerOuts = new List<Connection>();

		// Token: 0x040030B8 RID: 12472
		protected const float MaxOverVoltageFactor = 2f;
	}
}
