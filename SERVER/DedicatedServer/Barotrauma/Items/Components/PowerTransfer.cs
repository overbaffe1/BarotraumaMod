using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004DD RID: 1245
	internal class PowerTransfer : Powered
	{
		// Token: 0x170012F1 RID: 4849
		// (get) Token: 0x0600469D RID: 18077 RVA: 0x001C32A8 File Offset: 0x001C14A8
		// (set) Token: 0x0600469E RID: 18078 RVA: 0x001C32B0 File Offset: 0x001C14B0
		public List<Connection> PowerConnections { get; private set; }

		// Token: 0x170012F2 RID: 4850
		// (get) Token: 0x0600469F RID: 18079 RVA: 0x001C32BC File Offset: 0x001C14BC
		// (set) Token: 0x060046A0 RID: 18080 RVA: 0x001C330E File Offset: 0x001C150E
		public float PowerLoad
		{
			get
			{
				if (this is RelayComponent || this.PowerConnections.Count == 0 || this.PowerConnections[0].Grid == null)
				{
					return this.powerLoad;
				}
				return this.PowerConnections[0].Grid.Load;
			}
			set
			{
				this.powerLoad = value;
			}
		}

		// Token: 0x170012F3 RID: 4851
		// (get) Token: 0x060046A1 RID: 18081 RVA: 0x001C3317 File Offset: 0x001C1517
		// (set) Token: 0x060046A2 RID: 18082 RVA: 0x001C331F File Offset: 0x001C151F
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the item be damaged if too much power is supplied to the power grid.", "", false)]
		public bool CanBeOverloaded { get; set; }

		// Token: 0x170012F4 RID: 4852
		// (get) Token: 0x060046A3 RID: 18083 RVA: 0x001C3328 File Offset: 0x001C1528
		// (set) Token: 0x060046A4 RID: 18084 RVA: 0x001C3330 File Offset: 0x001C1530
		[Editable(MinValueFloat = 1f)]
		[Serialize(2f, IsPropertySaveable.Yes, "How much power has to be supplied to the grid relative to the load before item starts taking damage. E.g. a value of 2 means that the grid has to be receiving twice as much power as the devices in the grid are consuming.", "", false)]
		public float OverloadVoltage { get; set; }

		// Token: 0x170012F5 RID: 4853
		// (get) Token: 0x060046A5 RID: 18085 RVA: 0x001C3339 File Offset: 0x001C1539
		// (set) Token: 0x060046A6 RID: 18086 RVA: 0x001C3341 File Offset: 0x001C1541
		[Serialize(0.15f, IsPropertySaveable.Yes, "The probability for a fire to start when the item breaks.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float FireProbability { get; set; }

		// Token: 0x170012F6 RID: 4854
		// (get) Token: 0x060046A7 RID: 18087 RVA: 0x001C334A File Offset: 0x001C154A
		// (set) Token: 0x060046A8 RID: 18088 RVA: 0x001C3352 File Offset: 0x001C1552
		[Serialize(false, IsPropertySaveable.No, "Is the item currently overloaded. Intended to be used by StatusEffect conditionals (setting the value from XML is not recommended).", "", false)]
		public bool Overload { get; set; }

		// Token: 0x170012F7 RID: 4855
		// (get) Token: 0x060046A9 RID: 18089 RVA: 0x001C335B File Offset: 0x001C155B
		// (set) Token: 0x060046AA RID: 18090 RVA: 0x001C3363 File Offset: 0x001C1563
		public float ExtraLoad
		{
			get
			{
				return this.extraLoad;
			}
			set
			{
				this.extraLoad = value;
				this.extraLoadSetTime = (float)Timing.TotalTime;
			}
		}

		// Token: 0x170012F8 RID: 4856
		// (get) Token: 0x060046AB RID: 18091 RVA: 0x001C3378 File Offset: 0x001C1578
		// (set) Token: 0x060046AC RID: 18092 RVA: 0x001C3380 File Offset: 0x001C1580
		public bool CanTransfer
		{
			get
			{
				return this.canTransfer;
			}
			set
			{
				if (this.canTransfer == value)
				{
					return;
				}
				this.canTransfer = value;
				this.SetAllConnectionsDirty();
			}
		}

		// Token: 0x170012F9 RID: 4857
		// (get) Token: 0x060046AD RID: 18093 RVA: 0x001C3399 File Offset: 0x001C1599
		// (set) Token: 0x060046AE RID: 18094 RVA: 0x001C33A1 File Offset: 0x001C15A1
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
				base.IsActive = value;
				this.powerLoad = 0f;
				this.currPowerConsumption = 0f;
				this.SetAllConnectionsDirty();
				if (!base.IsActive)
				{
					this.RefreshConnections();
				}
			}
		}

		// Token: 0x060046AF RID: 18095 RVA: 0x001C33DE File Offset: 0x001C15DE
		public PowerTransfer(Item item, ContentXElement element) : base(item, element)
		{
			this.IsActive = true;
			this.canTransfer = true;
		}

		// Token: 0x060046B0 RID: 18096 RVA: 0x001C3418 File Offset: 0x001C1618
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			base.UpdateBroken(deltaTime, cam);
			this.Overload = false;
			if (!this.isBroken)
			{
				this.powerLoad = 0f;
				this.currPowerConsumption = 0f;
				this.SetAllConnectionsDirty();
				PowerTransfer.recipientsToRefresh.Clear();
				foreach (HashSet<Connection> recipientList in this.connectedRecipients.Values)
				{
					foreach (Connection c in recipientList)
					{
						if (c.Item != this.item)
						{
							PowerTransfer recipientPowerTransfer = c.Item.GetComponent<PowerTransfer>();
							if (recipientPowerTransfer != null)
							{
								PowerTransfer.recipientsToRefresh.Add(recipientPowerTransfer);
							}
						}
					}
				}
				foreach (PowerTransfer recipientPowerTransfer2 in PowerTransfer.recipientsToRefresh)
				{
					recipientPowerTransfer2.SetAllConnectionsDirty();
					recipientPowerTransfer2.RefreshConnections();
				}
				this.RefreshConnections();
				this.isBroken = true;
			}
		}

		// Token: 0x060046B1 RID: 18097 RVA: 0x001C3564 File Offset: 0x001C1764
		public override void Update(float deltaTime, Camera cam)
		{
			this.RefreshConnections();
			this.UpdateExtraLoad(deltaTime);
			if (!this.CanTransfer)
			{
				return;
			}
			if (this.isBroken)
			{
				this.SetAllConnectionsDirty();
				this.isBroken = false;
			}
			base.ApplyStatusEffects(ActionType.OnActive, deltaTime, null, null, null, null, null, 1f);
			this.SendSignals();
			this.UpdateOvervoltage(deltaTime);
		}

		// Token: 0x060046B2 RID: 18098 RVA: 0x001C35C4 File Offset: 0x001C17C4
		protected virtual void UpdateExtraLoad(float deltaTime)
		{
			if (Timing.TotalTime <= (double)this.extraLoadSetTime + 1.0)
			{
				return;
			}
			if (this.extraLoad > 0f)
			{
				this.extraLoad = Math.Max(this.extraLoad - 1000f * deltaTime, 0f);
				return;
			}
			this.extraLoad = Math.Min(this.extraLoad + 1000f * deltaTime, 0f);
		}

		// Token: 0x060046B3 RID: 18099 RVA: 0x001C3634 File Offset: 0x001C1834
		protected virtual void SendSignals()
		{
			float powerReadingOut = 0f;
			float loadReadingOut = this.ExtraLoad;
			if (this.powerLoad < 0f)
			{
				powerReadingOut = -this.powerLoad;
				loadReadingOut = 0f;
			}
			if (base.powerOut != null && base.powerOut.Grid != null)
			{
				powerReadingOut = base.powerOut.Grid.Power;
				loadReadingOut = base.powerOut.Grid.Load;
			}
			if (this.prevSentPowerValue != (int)powerReadingOut || this.powerSignal == null)
			{
				this.prevSentPowerValue = (int)Math.Round((double)powerReadingOut);
				this.powerSignal = this.prevSentPowerValue.ToString();
			}
			if (this.prevSentLoadValue != (int)loadReadingOut || this.loadSignal == null)
			{
				this.prevSentLoadValue = (int)Math.Round((double)loadReadingOut);
				this.loadSignal = this.prevSentLoadValue.ToString();
			}
			this.item.SendSignal(this.powerSignal, "power_value_out");
			this.item.SendSignal(this.loadSignal, "load_value_out");
		}

		// Token: 0x060046B4 RID: 18100 RVA: 0x001C3730 File Offset: 0x001C1930
		protected virtual void UpdateOvervoltage(float deltaTime)
		{
			if (!this.item.Repairables.Any<Repairable>() || !this.CanBeOverloaded)
			{
				return;
			}
			float maxOverVoltage = Math.Max(this.OverloadVoltage, 1f);
			bool overload;
			if (base.Voltage > maxOverVoltage)
			{
				GameSession gameSession = GameMain.GameSession;
				overload = (gameSession == null || gameSession.RoundDuration >= 5f);
			}
			else
			{
				overload = false;
			}
			this.Overload = overload;
			if (this.Overload)
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (networkMember == null || !networkMember.IsClient)
				{
					if (this.overloadCooldownTimer > 0f)
					{
						this.overloadCooldownTimer -= deltaTime;
						return;
					}
					float prevCondition = this.item.Condition;
					if (Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < 0.01f)
					{
						float conditionFactor = MathHelper.Lerp(5f, 1f, this.item.Condition / this.item.MaxCondition);
						this.item.Condition -= deltaTime * Rand.Range(10f, 500f, Rand.RandSync.Unsynced) * conditionFactor;
					}
					if (this.item.Condition > 0f || prevCondition <= 0f)
					{
						return;
					}
					this.overloadCooldownTimer = 5f;
					GameSession gameSession2 = GameMain.GameSession;
					float currentIntensity = (((gameSession2 != null) ? gameSession2.EventManager : null) != null) ? GameMain.GameSession.EventManager.CurrentIntensity : 0.5f;
					if (this.FireProbability > 0f && Rand.Range(0f, 1f, Rand.RandSync.Unsynced) < MathHelper.Lerp(this.FireProbability, this.FireProbability * 0.1f, currentIntensity))
					{
						new FireSource(this.item.WorldPosition, null, null, false);
					}
					return;
				}
			}
		}

		// Token: 0x060046B5 RID: 18101 RVA: 0x001C38DD File Offset: 0x001C1ADD
		public override float GetConnectionPowerOut(Connection conn, float power, PowerRange minMaxPower, float load)
		{
			if (conn != base.powerOut)
			{
				return 0f;
			}
			return MathHelper.Max(-(base.PowerConsumption + this.ExtraLoad), 0f);
		}

		// Token: 0x060046B6 RID: 18102 RVA: 0x001C3906 File Offset: 0x001C1B06
		public override bool Pick(Character picker)
		{
			return picker != null;
		}

		// Token: 0x060046B7 RID: 18103 RVA: 0x001C390C File Offset: 0x001C1B0C
		protected void RefreshConnections()
		{
			List<Connection> connections = this.item.Connections;
			foreach (Connection c in connections)
			{
				if (!this.connectionDirty.ContainsKey(c))
				{
					this.connectionDirty[c] = true;
				}
				else if (!this.connectionDirty[c])
				{
					continue;
				}
				HashSet<Connection> tempConnected;
				if (!this.connectedRecipients.ContainsKey(c))
				{
					tempConnected = new HashSet<Connection>();
					this.connectedRecipients.Add(c, tempConnected);
				}
				else
				{
					tempConnected = this.connectedRecipients[c];
					tempConnected.Clear();
					foreach (Connection recipient in tempConnected)
					{
						PowerTransfer pt = recipient.Item.GetComponent<PowerTransfer>();
						if (pt != null)
						{
							pt.connectionDirty[recipient] = true;
						}
					}
				}
				tempConnected.Add(c);
				if (this.item.Condition > 0f)
				{
					this.GetConnected(c, tempConnected);
					foreach (Connection recipient2 in tempConnected)
					{
						if (recipient2 != c)
						{
							PowerTransfer recipientPowerTransfer = recipient2.Item.GetComponent<PowerTransfer>();
							if (recipientPowerTransfer != null)
							{
								if (!recipientPowerTransfer.connectedRecipients.ContainsKey(recipient2))
								{
									recipientPowerTransfer.connectedRecipients.Add(recipient2, new HashSet<Connection>());
								}
								else
								{
									recipientPowerTransfer.connectedRecipients[recipient2].Clear();
								}
								foreach (Connection connection in tempConnected)
								{
									recipientPowerTransfer.connectedRecipients[recipient2].Add(connection);
								}
								recipientPowerTransfer.connectionDirty[recipient2] = false;
							}
						}
					}
				}
				this.connectionDirty[c] = false;
			}
		}

		// Token: 0x060046B8 RID: 18104 RVA: 0x001C3B74 File Offset: 0x001C1D74
		private void GetConnected(Connection c, HashSet<Connection> connected)
		{
			List<Connection> recipients = c.Recipients;
			foreach (Connection recipient in recipients)
			{
				if (recipient != null && !connected.Contains(recipient))
				{
					Item it = recipient.Item;
					if (it != null && it.Condition > 0f)
					{
						connected.Add(recipient);
						PowerTransfer powerTransfer = it.GetComponent<PowerTransfer>();
						if (powerTransfer != null && powerTransfer.CanTransfer && powerTransfer.IsActive)
						{
							this.GetConnected(recipient, connected);
						}
					}
				}
			}
		}

		// Token: 0x060046B9 RID: 18105 RVA: 0x001C3C14 File Offset: 0x001C1E14
		public void SetAllConnectionsDirty()
		{
			if (this.item.Connections == null)
			{
				return;
			}
			foreach (Connection c2 in this.item.Connections)
			{
				this.connectionDirty[c2] = true;
				if (c2.IsPower)
				{
					Powered.ChangedConnections.Add(c2);
					HashSet<Connection> recipients;
					if (this.connectedRecipients.TryGetValue(c2, out recipients))
					{
						(from c in recipients
						where c.IsPower
						select c).ForEach(delegate(Connection c)
						{
							Powered.ChangedConnections.Add(c);
						});
					}
				}
			}
		}

		// Token: 0x060046BA RID: 18106 RVA: 0x001C3CF4 File Offset: 0x001C1EF4
		public void SetConnectionDirty(Connection connection)
		{
			List<Connection> connections = this.item.Connections;
			if (connections == null || !connections.Contains(connection))
			{
				return;
			}
			this.connectionDirty[connection] = true;
			if (connection.IsPower)
			{
				Powered.ChangedConnections.Add(connection);
				HashSet<Connection> recipients;
				if (this.connectedRecipients.TryGetValue(connection, out recipients))
				{
					(from c in recipients
					where c.IsPower
					select c).ForEach(delegate(Connection c)
					{
						Powered.ChangedConnections.Add(c);
					});
				}
			}
		}

		// Token: 0x060046BB RID: 18107 RVA: 0x001C3D94 File Offset: 0x001C1F94
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			List<Connection> connections = base.Item.Connections;
			List<Connection> powerConnections;
			if (connections != null)
			{
				powerConnections = connections.FindAll((Connection c) => c.IsPower);
			}
			else
			{
				powerConnections = new List<Connection>();
			}
			this.PowerConnections = powerConnections;
			if (connections == null)
			{
				this.IsActive = false;
				return;
			}
			foreach (Connection c2 in connections)
			{
				if (c2.Name.Length > 5 && c2.Name.Substring(0, 6) == "signal")
				{
					this.signalConnections.Add(c2);
				}
			}
			if (!(this is RelayComponent) && !(this is PowerDistributor))
			{
				if (this.PowerConnections.Any((Connection p) => !p.IsOutput))
				{
					if (this.PowerConnections.Any((Connection p) => p.IsOutput))
					{
						DebugConsole.ThrowError("Error in item \"" + base.Name + "\" - PowerTransfer components should not have separate power inputs and outputs, but transfer power between wires connected to the same power connection. If you want power to pass from input to output, change the component to a RelayComponent.", null, null, false, false);
					}
				}
			}
			this.SetAllConnectionsDirty();
		}

		// Token: 0x060046BC RID: 18108 RVA: 0x001C3EF0 File Offset: 0x001C20F0
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			if (this.item.Condition <= 0f || connection.IsPower)
			{
				return;
			}
			if (!this.connectedRecipients.ContainsKey(connection))
			{
				return;
			}
			if (!this.signalConnections.Contains(connection))
			{
				return;
			}
			foreach (Connection recipient in this.connectedRecipients[connection])
			{
				if (recipient.Item != this.item && recipient.Item != signal.source)
				{
					Item source = signal.source;
					if (source != null)
					{
						source.LastSentSignalRecipients.Add(recipient);
					}
					foreach (ItemComponent ic in recipient.Item.Components)
					{
						PowerTransfer powerTransfer = ic as PowerTransfer;
						if (powerTransfer == null || powerTransfer is RelayComponent || powerTransfer is PowerDistributor)
						{
							ic.ReceiveSignal(signal, recipient);
						}
					}
					if (recipient.Effects != null && signal.value != "0" && !string.IsNullOrEmpty(signal.value))
					{
						foreach (StatusEffect effect in recipient.Effects)
						{
							recipient.Item.ApplyStatusEffect(effect, ActionType.OnUse, 1f, null, null, null, false, true, null);
						}
					}
				}
			}
		}

		// Token: 0x060046BD RID: 18109 RVA: 0x001C40CC File Offset: 0x001C22CC
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			Dictionary<Connection, HashSet<Connection>> dictionary = this.connectedRecipients;
			if (dictionary != null)
			{
				dictionary.Clear();
			}
			Dictionary<Connection, bool> dictionary2 = this.connectionDirty;
			if (dictionary2 != null)
			{
				dictionary2.Clear();
			}
			PowerTransfer.recipientsToRefresh.Clear();
		}

		// Token: 0x0400220F RID: 8719
		private readonly HashSet<Connection> signalConnections = new HashSet<Connection>();

		// Token: 0x04002210 RID: 8720
		private readonly Dictionary<Connection, bool> connectionDirty = new Dictionary<Connection, bool>();

		// Token: 0x04002211 RID: 8721
		private readonly Dictionary<Connection, HashSet<Connection>> connectedRecipients = new Dictionary<Connection, HashSet<Connection>>();

		// Token: 0x04002212 RID: 8722
		private float overloadCooldownTimer;

		// Token: 0x04002213 RID: 8723
		private const float OverloadCooldown = 5f;

		// Token: 0x04002214 RID: 8724
		protected float powerLoad;

		// Token: 0x04002215 RID: 8725
		protected bool isBroken;

		// Token: 0x0400221A RID: 8730
		private float extraLoad;

		// Token: 0x0400221B RID: 8731
		private float extraLoadSetTime;

		// Token: 0x0400221C RID: 8732
		private bool canTransfer;

		// Token: 0x0400221D RID: 8733
		private static readonly HashSet<PowerTransfer> recipientsToRefresh = new HashSet<PowerTransfer>();

		// Token: 0x0400221E RID: 8734
		private int prevSentPowerValue;

		// Token: 0x0400221F RID: 8735
		private string powerSignal;

		// Token: 0x04002220 RID: 8736
		private int prevSentLoadValue;

		// Token: 0x04002221 RID: 8737
		private string loadSignal;
	}
}
