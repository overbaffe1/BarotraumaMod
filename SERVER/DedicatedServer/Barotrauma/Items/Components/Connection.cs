using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004EA RID: 1258
	internal class Connection
	{
		// Token: 0x1700130B RID: 4875
		// (get) Token: 0x060046FF RID: 18175 RVA: 0x001C4F0B File Offset: 0x001C310B
		// (set) Token: 0x06004700 RID: 18176 RVA: 0x001C4F1D File Offset: 0x001C311D
		public LocalizedString DisplayName
		{
			get
			{
				return this.DisplayNameOverride ?? this._displayName;
			}
			private set
			{
				this._displayName = value;
			}
		}

		// Token: 0x1700130C RID: 4876
		// (get) Token: 0x06004701 RID: 18177 RVA: 0x001C4F26 File Offset: 0x001C3126
		public LocalizedString DefaultDisplayName
		{
			get
			{
				return this._displayName;
			}
		}

		// Token: 0x1700130D RID: 4877
		// (get) Token: 0x06004702 RID: 18178 RVA: 0x001C4F2E File Offset: 0x001C312E
		public IReadOnlyCollection<Wire> Wires
		{
			get
			{
				return this.wires;
			}
		}

		// Token: 0x1700130E RID: 4878
		// (get) Token: 0x06004703 RID: 18179 RVA: 0x001C4F36 File Offset: 0x001C3136
		// (set) Token: 0x06004704 RID: 18180 RVA: 0x001C4F3E File Offset: 0x001C313E
		public Signal LastSentSignal { get; private set; }

		// Token: 0x1700130F RID: 4879
		// (get) Token: 0x06004705 RID: 18181 RVA: 0x001C4F47 File Offset: 0x001C3147
		// (set) Token: 0x06004706 RID: 18182 RVA: 0x001C4F4F File Offset: 0x001C314F
		public Signal LastReceivedSignal { get; private set; }

		// Token: 0x17001310 RID: 4880
		// (get) Token: 0x06004707 RID: 18183 RVA: 0x001C4F58 File Offset: 0x001C3158
		// (set) Token: 0x06004708 RID: 18184 RVA: 0x001C4F60 File Offset: 0x001C3160
		public bool IsPower { get; private set; }

		// Token: 0x17001311 RID: 4881
		// (get) Token: 0x06004709 RID: 18185 RVA: 0x001C4F69 File Offset: 0x001C3169
		public List<Connection> Recipients
		{
			get
			{
				if (this.recipientsDirty)
				{
					this.RefreshRecipients();
				}
				return this.recipients;
			}
		}

		// Token: 0x17001312 RID: 4882
		// (get) Token: 0x0600470A RID: 18186 RVA: 0x001C4F7F File Offset: 0x001C317F
		public Item Item
		{
			get
			{
				return this.item;
			}
		}

		// Token: 0x17001313 RID: 4883
		// (get) Token: 0x0600470B RID: 18187 RVA: 0x001C4F87 File Offset: 0x001C3187
		// (set) Token: 0x0600470C RID: 18188 RVA: 0x001C4F8F File Offset: 0x001C318F
		public ConnectionPanel ConnectionPanel { get; private set; }

		// Token: 0x0600470D RID: 18189 RVA: 0x001C4F98 File Offset: 0x001C3198
		public override string ToString()
		{
			return string.Concat(new string[]
			{
				"Connection (",
				this.item.Name,
				", ",
				this.Name,
				")"
			});
		}

		// Token: 0x0600470E RID: 18190 RVA: 0x001C4FD4 File Offset: 0x001C31D4
		public Connection(ContentXElement element, int connectionIndex, ConnectionPanel connectionPanel, IdRemap idRemap, bool isItemSwap)
		{
			this.ConnectionPanel = connectionPanel;
			this.item = connectionPanel.Item;
			this.MaxWires = element.GetAttributeInt("maxwires", 5);
			this.MaxWires = Math.Max(element.Elements().Count((ContentXElement e) => e.Name.ToString().Equals("link", StringComparison.OrdinalIgnoreCase)), this.MaxWires);
			this.MaxPlayerConnectableWires = element.GetAttributeInt("maxplayerconnectablewires", this.MaxWires);
			this.wires = new HashSet<Wire>();
			this.IsOutput = (element.Name.ToString() == "output");
			this.Name = element.GetAttributeString("name", this.IsOutput ? "output" : "input");
			XAttribute displayOrderAttr = element.GetAttribute("displayorderoverride");
			int displayOrder;
			if (displayOrderAttr == null)
			{
				IEnumerable<Connection> sameElements = from c in connectionPanel.Connections
				where c.IsOutput == this.IsOutput
				select c;
				int num;
				if (sameElements.Any<Connection>())
				{
					num = sameElements.Max((Connection c) => c.DisplayOrder) + 1;
				}
				else
				{
					num = 0;
				}
				displayOrder = num;
			}
			else
			{
				displayOrder = displayOrderAttr.GetAttributeInt(0);
			}
			this.DisplayOrder = displayOrder;
			string displayNameTag = "";
			string fallbackTag = "";
			if (element.GetAttribute("displayname") == null)
			{
				using (IEnumerator<ContentXElement> enumerator = this.item.Prefab.ConfigElement.Elements().GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						ContentXElement subElement = enumerator.Current;
						if (subElement.Name.ToString().Equals("connectionpanel", StringComparison.OrdinalIgnoreCase))
						{
							int prefabConnectionIndex = 0;
							foreach (ContentXElement cxe in subElement.Elements())
							{
								XElement connectionElement = cxe;
								string prefabConnectionName = connectionElement.GetAttributeString("name", null);
								if (!prefabConnectionName.IsNullOrEmpty())
								{
									string[] aliases = connectionElement.GetAttributeStringArray("aliases", Array.Empty<string>(), true, false);
									if (prefabConnectionName == this.Name || aliases.Contains(this.Name) || (isItemSwap && connectionIndex == prefabConnectionIndex))
									{
										displayNameTag = connectionElement.GetAttributeString("displayname", "");
										fallbackTag = connectionElement.GetAttributeString("fallbackdisplayname", "");
									}
									prefabConnectionIndex++;
								}
							}
						}
					}
					goto IL_2C7;
				}
			}
			displayNameTag = element.GetAttributeString("displayname", "");
			fallbackTag = element.GetAttributeString("fallbackdisplayname", null);
			IL_2C7:
			if (!string.IsNullOrEmpty(displayNameTag))
			{
				string text;
				if (displayNameTag == null)
				{
					text = null;
				}
				else
				{
					string[] array = displayNameTag.Split('~', StringSplitOptions.None);
					text = ((array != null) ? array.FirstOrDefault<string>() : null);
				}
				string tagWithoutVariables = text;
				string text2;
				if (fallbackTag == null)
				{
					text2 = null;
				}
				else
				{
					string[] array2 = fallbackTag.Split('~', StringSplitOptions.None);
					text2 = ((array2 != null) ? array2.FirstOrDefault<string>() : null);
				}
				string fallbackTagWithoutVariables = text2;
				if (TextManager.ContainsTag(tagWithoutVariables))
				{
					this.DisplayName = TextManager.GetServerMessage(displayNameTag);
				}
				else if (TextManager.ContainsTag(fallbackTagWithoutVariables))
				{
					this.DisplayName = TextManager.GetServerMessage(fallbackTag);
				}
			}
			if (this.DisplayName.IsNullOrEmpty())
			{
				this.DisplayName = this.Name;
			}
			string name = this.Name;
			bool def = name == "power_in" || name == "power" || name == "power_out";
			this.IsPower = element.GetAttributeBool("ispower", def);
			this.LoadedWires = new List<ValueTuple<ushort, int?>>();
			foreach (ContentXElement subElement2 in element.Elements())
			{
				string a = subElement2.Name.ToString().ToLowerInvariant();
				if (!(a == "link"))
				{
					if (a == "statuseffect")
					{
						if (this.Effects == null)
						{
							this.Effects = new List<StatusEffect>();
						}
						this.Effects.Add(StatusEffect.Load(subElement2, this.item.Name + ", connection " + this.Name));
					}
				}
				else
				{
					int id = subElement2.GetAttributeInt("w", 0);
					int? i = null;
					if (subElement2.GetAttribute("i") != null)
					{
						i = new int?(subElement2.GetAttributeInt("i", 0));
					}
					if (id < 0)
					{
						id = 0;
					}
					if (this.LoadedWires.Count < this.MaxWires)
					{
						this.LoadedWires.Add(new ValueTuple<ushort, int?>(idRemap.GetOffsetId(id), i));
					}
				}
			}
		}

		// Token: 0x0600470F RID: 18191 RVA: 0x001C54F8 File Offset: 0x001C36F8
		public bool IsConnectedToSomething()
		{
			return this.wires.Count > 0 || this.CircuitBoxConnections.Count > 0;
		}

		// Token: 0x06004710 RID: 18192 RVA: 0x001C5518 File Offset: 0x001C3718
		public void SetRecipientsDirty()
		{
			this.recipientsDirty = true;
			if (this.IsPower)
			{
				Powered.ChangedConnections.Add(this);
			}
		}

		// Token: 0x06004711 RID: 18193 RVA: 0x001C5538 File Offset: 0x001C3738
		private void RefreshRecipients()
		{
			this.recipients.Clear();
			foreach (Wire wire in this.wires)
			{
				Connection recipient = wire.OtherConnection(this);
				if (recipient != null)
				{
					this.recipients.Add(recipient);
				}
			}
			this.recipientsDirty = false;
		}

		// Token: 0x06004712 RID: 18194 RVA: 0x001C55B0 File Offset: 0x001C37B0
		public Wire FindWireByItem(Item it)
		{
			return this.Wires.FirstOrDefault((Wire w) => w.Item == it);
		}

		// Token: 0x06004713 RID: 18195 RVA: 0x001C55E1 File Offset: 0x001C37E1
		public bool WireSlotsAvailable()
		{
			return this.wires.Count < this.MaxWires;
		}

		// Token: 0x06004714 RID: 18196 RVA: 0x001C55F6 File Offset: 0x001C37F6
		public bool TryAddLink(Wire wire)
		{
			if (wire == null || this.wires.Contains(wire) || !this.WireSlotsAvailable())
			{
				return false;
			}
			this.wires.Add(wire);
			return true;
		}

		// Token: 0x06004715 RID: 18197 RVA: 0x001C5624 File Offset: 0x001C3824
		public void DisconnectWire(Wire wire)
		{
			if (wire == null || !this.wires.Contains(wire))
			{
				return;
			}
			Connection prevOtherConnection = wire.OtherConnection(this);
			if (prevOtherConnection != null)
			{
				if (this.IsPower && prevOtherConnection.IsPower && this.Grid != null)
				{
					if (prevOtherConnection.recipients.Count > 1 && this.recipients.Count > 1)
					{
						Powered.ChangedConnections.Add(prevOtherConnection);
						Powered.ChangedConnections.Add(this);
					}
					else if (this.recipients.Count > 1)
					{
						GridInfo grid = prevOtherConnection.Grid;
						if (grid != null)
						{
							grid.RemoveConnection(prevOtherConnection);
						}
						prevOtherConnection.Grid = null;
					}
					else if (prevOtherConnection.recipients.Count > 1)
					{
						GridInfo grid2 = this.Grid;
						if (grid2 != null)
						{
							grid2.RemoveConnection(this);
						}
						this.Grid = null;
					}
					else if (this.Grid.Connections.Count == 2)
					{
						Powered.Grids.Remove(this.Grid.ID);
						this.Grid = null;
						prevOtherConnection.Grid = null;
					}
				}
				prevOtherConnection.recipientsDirty = true;
			}
			if (this.enumeratingWires)
			{
				this.removedWires.Add(wire);
			}
			else
			{
				this.wires.Remove(wire);
			}
			this.recipientsDirty = true;
		}

		// Token: 0x06004716 RID: 18198 RVA: 0x001C5768 File Offset: 0x001C3968
		public void ConnectWire(Wire wire)
		{
			if (wire == null || !this.TryAddLink(wire))
			{
				return;
			}
			this.ConnectionPanel.DisconnectedWires.Remove(wire);
			Connection otherConnection = wire.OtherConnection(this);
			if (otherConnection != null)
			{
				if (Powered.ValidPowerConnection(this, otherConnection))
				{
					if (this.Grid == null && otherConnection.Grid != null)
					{
						otherConnection.Grid.AddConnection(this);
						this.Grid = otherConnection.Grid;
					}
					else if (this.Grid != null && otherConnection.Grid == null)
					{
						this.Grid.AddConnection(otherConnection);
						otherConnection.Grid = this.Grid;
					}
					else
					{
						Powered.ChangedConnections.Add(this);
						Powered.ChangedConnections.Add(otherConnection);
					}
				}
				otherConnection.recipientsDirty = true;
			}
			this.recipientsDirty = true;
		}

		// Token: 0x06004717 RID: 18199 RVA: 0x001C5824 File Offset: 0x001C3A24
		public void SendSignal(Signal signal)
		{
			this.LastSentSignal = signal;
			this.enumeratingWires = true;
			foreach (Wire wire in this.wires)
			{
				Connection recipient = wire.OtherConnection(this);
				if (recipient != null && recipient.item != this.item)
				{
					Item source = signal.source;
					if (((source != null) ? source.LastSentSignalRecipients.LastOrDefault<Connection>() : null) != recipient)
					{
						Item source2 = signal.source;
						if (source2 != null)
						{
							source2.LastSentSignalRecipients.Add(recipient);
						}
						Connection.SendSignalIntoConnection(signal, recipient);
					}
				}
			}
			foreach (CircuitBoxConnection connection in this.CircuitBoxConnections)
			{
				connection.ReceiveSignal(signal);
			}
			this.enumeratingWires = false;
			foreach (Wire removedWire in this.removedWires)
			{
				this.wires.Remove(removedWire);
			}
			this.removedWires.Clear();
		}

		// Token: 0x06004718 RID: 18200 RVA: 0x001C5970 File Offset: 0x001C3B70
		public static void SendSignalIntoConnection(Signal signal, Connection conn)
		{
			conn.LastReceivedSignal = signal;
			foreach (ItemComponent ic in conn.item.Components)
			{
				ic.ReceiveSignal(signal, conn);
			}
			if (conn.Effects == null || signal.value == "0")
			{
				return;
			}
			foreach (StatusEffect effect in conn.Effects)
			{
				conn.Item.ApplyStatusEffect(effect, ActionType.OnUse, 0.016666668f, null, null, null, false, true, null);
			}
		}

		// Token: 0x06004719 RID: 18201 RVA: 0x001C5A48 File Offset: 0x001C3C48
		public void ClearConnections()
		{
			if (this.IsPower && this.Grid != null)
			{
				Powered.ChangedConnections.Add(this);
				foreach (Connection c in this.recipients)
				{
					Powered.ChangedConnections.Add(c);
				}
			}
			foreach (Wire wire in this.wires)
			{
				wire.RemoveConnection(this);
				this.recipientsDirty = true;
			}
			if (this.enumeratingWires)
			{
				using (HashSet<Wire>.Enumerator enumerator3 = this.wires.GetEnumerator())
				{
					while (enumerator3.MoveNext())
					{
						Wire wire2 = enumerator3.Current;
						this.removedWires.Add(wire2);
					}
					return;
				}
			}
			this.wires.Clear();
		}

		// Token: 0x0600471A RID: 18202 RVA: 0x001C5B64 File Offset: 0x001C3D64
		public void InitializeFromLoaded()
		{
			if (this.LoadedWires.Count == 0)
			{
				return;
			}
			foreach (ValueTuple<ushort, int?> valueTuple in this.LoadedWires)
			{
				ushort wireId = valueTuple.Item1;
				int? connectionIndex = valueTuple.Item2;
				Item wireItem = Entity.FindEntityByID(wireId) as Item;
				if (wireItem != null)
				{
					Wire wire = wireItem.GetComponent<Wire>();
					if (wire != null && this.TryAddLink(wire))
					{
						if (wire.Item.body != null)
						{
							wire.Item.body.Enabled = false;
						}
						if (connectionIndex != null)
						{
							wire.Connect(this, connectionIndex.Value, false, false);
						}
						else
						{
							wire.TryConnect(this, false, false);
						}
						wire.FixNodeEnds();
						this.recipientsDirty = true;
					}
				}
			}
			this.LoadedWires.Clear();
		}

		// Token: 0x0600471B RID: 18203 RVA: 0x001C5C58 File Offset: 0x001C3E58
		public void Save(XElement parentElement)
		{
			XElement newElement = new XElement(this.IsOutput ? "output" : "input", new XAttribute("name", this.Name));
			foreach (Wire wire in from w in this.wires
			orderby w.Item.ID
			select w)
			{
				newElement.Add(new XElement("link", new object[]
				{
					new XAttribute("w", wire.Item.ID.ToString()),
					new XAttribute("i", (wire.Connections[0] != this) ? 1 : 0)
				}));
			}
			parentElement.Add(newElement);
		}

		// Token: 0x0400223D RID: 8765
		private const int DefaultMaxWires = 5;

		// Token: 0x0400223E RID: 8766
		public readonly int MaxPlayerConnectableWires = 5;

		// Token: 0x0400223F RID: 8767
		public readonly int MaxWires = 5;

		// Token: 0x04002240 RID: 8768
		public readonly int DisplayOrder;

		// Token: 0x04002241 RID: 8769
		public readonly string Name;

		// Token: 0x04002242 RID: 8770
		private readonly LocalizedString _displayName;

		// Token: 0x04002243 RID: 8771
		public LocalizedString DisplayNameOverride;

		// Token: 0x04002244 RID: 8772
		private readonly HashSet<Wire> wires;

		// Token: 0x04002245 RID: 8773
		public List<CircuitBoxConnection> CircuitBoxConnections = new List<CircuitBoxConnection>();

		// Token: 0x04002246 RID: 8774
		private bool enumeratingWires;

		// Token: 0x04002247 RID: 8775
		private readonly HashSet<Wire> removedWires = new HashSet<Wire>();

		// Token: 0x04002248 RID: 8776
		private readonly Item item;

		// Token: 0x04002249 RID: 8777
		public readonly bool IsOutput;

		// Token: 0x0400224A RID: 8778
		public readonly List<StatusEffect> Effects;

		// Token: 0x0400224B RID: 8779
		[TupleElementNames(new string[]
		{
			"wireId",
			"connectionIndex"
		})]
		public readonly List<ValueTuple<ushort, int?>> LoadedWires;

		// Token: 0x0400224C RID: 8780
		public GridInfo Grid;

		// Token: 0x0400224D RID: 8781
		public PowerPriority Priority;

		// Token: 0x04002251 RID: 8785
		private bool recipientsDirty = true;

		// Token: 0x04002252 RID: 8786
		private readonly List<Connection> recipients = new List<Connection>();
	}
}
