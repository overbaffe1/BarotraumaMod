using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004A9 RID: 1193
	internal class ConnectionPanel : ItemComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x06004322 RID: 17186 RVA: 0x001AEC8C File Offset: 0x001ACE8C
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			List<Wire>[] wires = new List<Wire>[this.Connections.Count];
			byte connectionCount = msg.ReadByte();
			int i = 0;
			while (i < this.Connections.Count && i < (int)connectionCount)
			{
				wires[i] = new List<Wire>();
				uint wireCount = msg.ReadVariableUInt32();
				int j = 0;
				while ((long)j < (long)((ulong)wireCount))
				{
					ushort wireId = msg.ReadUInt16();
					Item wireItem = Entity.FindEntityByID(wireId) as Item;
					if (wireItem != null)
					{
						Wire wireComponent = wireItem.GetComponent<Wire>();
						if (wireComponent != null)
						{
							wires[i].Add(wireComponent);
						}
					}
					j++;
				}
				i++;
			}
			List<Wire> clientSideDisconnectedWires = new List<Wire>();
			ushort disconnectedWireCount = msg.ReadUInt16();
			for (int k = 0; k < (int)disconnectedWireCount; k++)
			{
				ushort wireId2 = msg.ReadUInt16();
				Item wireItem2 = Entity.FindEntityByID(wireId2) as Item;
				if (wireItem2 != null)
				{
					Wire wireComponent2 = wireItem2.GetComponent<Wire>();
					if (wireComponent2 != null)
					{
						clientSideDisconnectedWires.Add(wireComponent2);
					}
				}
			}
			if (this.Locked || this.TemporarilyLocked || !GameMain.NetworkMember.ServerSettings.AllowRewiring)
			{
				return;
			}
			this.item.CreateServerEvent<ConnectionPanel>(this);
			if (!this.item.CanClientAccess(c))
			{
				return;
			}
			for (int l = 0; l < this.Connections.Count; l++)
			{
				using (List<Wire>.Enumerator enumerator = wires[l].GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Wire wire = enumerator.Current;
						if (!this.Connections.Any((Connection connection) => connection.Wires.Contains(wire)) && !this.DisconnectedWires.Contains(wire) && !wire.Item.CanClientAccess(c))
						{
							return;
						}
					}
				}
			}
			if (this.CheckCharacterSuccess(c.Character))
			{
				for (int m = 0; m < this.Connections.Count; m++)
				{
					Wire[] array = this.Connections[m].Wires.ToArray<Wire>();
					for (int num = 0; num < array.Length; num++)
					{
						Wire existingWire = array[num];
						if (!wires[m].Contains(existingWire))
						{
							if (existingWire.Locked || existingWire.Item.IsLayerHidden)
							{
								GameServer.Log(string.Concat(new string[]
								{
									GameServer.CharacterLogName(c.Character),
									" attempted to disconnect a locked wire from ",
									this.Connections[m].Item.Name,
									" (",
									this.Connections[m].Name,
									")"
								}), ServerLog.MessageType.Error);
							}
							else
							{
								existingWire.RemoveConnection(this.item);
								if (existingWire.Item.ParentInventory == null)
								{
									ConnectionPanel component = this.item.GetComponent<ConnectionPanel>();
									if (component != null)
									{
										component.DisconnectedWires.Add(existingWire);
									}
								}
								if (!wires.Any((List<Wire> w) => w.Contains(existingWire)))
								{
									GameMain.Server.KarmaManager.OnWireDisconnected(c.Character, existingWire);
								}
								if (existingWire.Connections[0] == null && existingWire.Connections[1] == null)
								{
									GameServer.Log(string.Concat(new string[]
									{
										GameServer.CharacterLogName(c.Character),
										" disconnected a wire from ",
										this.Connections[m].Item.Name,
										" (",
										this.Connections[m].Name,
										")"
									}), ServerLog.MessageType.Wiring);
									if (existingWire.Item.ParentInventory != null)
									{
										existingWire.ClearConnections(null);
									}
									else if (!clientSideDisconnectedWires.Contains(existingWire))
									{
										existingWire.Item.Drop(c.Character, true, true);
									}
								}
								else if (existingWire.Connections[0] != null)
								{
									GameServer.Log(string.Concat(new string[]
									{
										GameServer.CharacterLogName(c.Character),
										" disconnected a wire from ",
										this.Connections[m].Item.Name,
										" (",
										this.Connections[m].Name,
										") to ",
										existingWire.Connections[0].Item.Name,
										" (",
										existingWire.Connections[0].Name,
										")"
									}), ServerLog.MessageType.Wiring);
								}
								else if (existingWire.Connections[1] != null)
								{
									GameServer.Log(string.Concat(new string[]
									{
										GameServer.CharacterLogName(c.Character),
										" disconnected a wire from ",
										this.Connections[m].Item.Name,
										" (",
										this.Connections[m].Name,
										") to ",
										existingWire.Connections[1].Item.Name,
										" (",
										existingWire.Connections[1].Name,
										")"
									}), ServerLog.MessageType.Wiring);
								}
								this.Connections[m].DisconnectWire(existingWire);
							}
						}
					}
				}
				for (int n = 0; n < this.Connections.Count; n++)
				{
					foreach (Wire newWire in wires[n])
					{
						if (!this.Connections[n].Wires.Contains(newWire))
						{
							newWire.TryConnect(this.Connections[n], true, true);
							this.Connections[n].TryAddLink(newWire);
							Connection otherConnection = newWire.OtherConnection(this.Connections[n]);
							if (otherConnection == null)
							{
								GameServer.Log(string.Concat(new string[]
								{
									GameServer.CharacterLogName(c.Character),
									" connected a wire to ",
									this.Connections[n].Item.Name,
									" (",
									this.Connections[n].Name,
									")"
								}), ServerLog.MessageType.Wiring);
							}
							else
							{
								GameServer.Log(string.Concat(new string[]
								{
									GameServer.CharacterLogName(c.Character),
									" connected a wire from ",
									this.Connections[n].Item.Name,
									" (",
									this.Connections[n].Name,
									") to ",
									(otherConnection == null) ? "none" : (otherConnection.Item.Name + " (" + otherConnection.Name + ")")
								}), ServerLog.MessageType.Wiring);
							}
						}
					}
				}
				foreach (Wire disconnectedWire in this.DisconnectedWires.ToList<Wire>())
				{
					if (disconnectedWire.Connections[0] == null && disconnectedWire.Connections[1] == null && !clientSideDisconnectedWires.Contains(disconnectedWire) && disconnectedWire.Item.ParentInventory == null)
					{
						disconnectedWire.Item.Drop(c.Character, true, true);
						GameServer.Log(GameServer.CharacterLogName(c.Character) + " dropped " + disconnectedWire.Name, ServerLog.MessageType.Inventory);
					}
				}
				return;
			}
			this.item.CreateServerEvent<ConnectionPanel>(this);
			CharacterInventory inventory = c.Character.Inventory;
			if (inventory != null)
			{
				inventory.CreateNetworkEvent();
			}
			foreach (Item heldItem in c.Character.HeldItems)
			{
				ConnectionPanel.<>c__DisplayClass0_1 CS$<>8__locals3 = new ConnectionPanel.<>c__DisplayClass0_1();
				CS$<>8__locals3.<>4__this = this;
				CS$<>8__locals3.selectedWire = ((heldItem != null) ? heldItem.GetComponent<Wire>() : null);
				if (CS$<>8__locals3.selectedWire != null)
				{
					CS$<>8__locals3.selectedWire.CreateNetworkEvent();
					ConnectionPanel.<>c__DisplayClass0_1 CS$<>8__locals4 = CS$<>8__locals3;
					Connection connection3 = CS$<>8__locals3.selectedWire.Connections[0];
					CS$<>8__locals4.panel1 = ((connection3 != null) ? connection3.ConnectionPanel : null);
					if (CS$<>8__locals3.panel1 != null && CS$<>8__locals3.panel1 != this)
					{
						CS$<>8__locals3.panel1.item.CreateServerEvent<ConnectionPanel>(CS$<>8__locals3.panel1);
					}
					ConnectionPanel.<>c__DisplayClass0_1 CS$<>8__locals5 = CS$<>8__locals3;
					Connection connection2 = CS$<>8__locals3.selectedWire.Connections[1];
					CS$<>8__locals5.panel2 = ((connection2 != null) ? connection2.ConnectionPanel : null);
					if (CS$<>8__locals3.panel2 != null && CS$<>8__locals3.panel2 != this)
					{
						CS$<>8__locals3.panel2.item.CreateServerEvent<ConnectionPanel>(CS$<>8__locals3.panel2);
					}
					CoroutineManager.Invoke(delegate
					{
						CS$<>8__locals3.<>4__this.item.CreateServerEvent<ConnectionPanel>(CS$<>8__locals3.<>4__this);
						if (CS$<>8__locals3.panel1 != null && CS$<>8__locals3.panel1 != CS$<>8__locals3.<>4__this)
						{
							CS$<>8__locals3.panel1.item.CreateServerEvent<ConnectionPanel>(CS$<>8__locals3.panel1);
						}
						if (CS$<>8__locals3.panel2 != null && CS$<>8__locals3.panel2 != CS$<>8__locals3.<>4__this)
						{
							CS$<>8__locals3.panel2.item.CreateServerEvent<ConnectionPanel>(CS$<>8__locals3.panel2);
						}
						if (!CS$<>8__locals3.selectedWire.Item.Removed)
						{
							CS$<>8__locals3.selectedWire.CreateNetworkEvent();
						}
					}, 1f);
				}
			}
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			server.CreateEntityEvent(this.item, new Item.ApplyStatusEffectEventData(ActionType.OnFailure, this, c.Character, null, null, null));
		}

		// Token: 0x06004323 RID: 17187 RVA: 0x001AF690 File Offset: 0x001AD890
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			msg.WriteUInt16((this.user == null) ? 0 : this.user.ID);
			this.ClientEventWrite(msg, extraData);
		}

		// Token: 0x170011E2 RID: 4578
		// (get) Token: 0x06004324 RID: 17188 RVA: 0x001AF6B8 File Offset: 0x001AD8B8
		public bool AlwaysAllowRewiring
		{
			get
			{
				if (this.item.Submarine == null)
				{
					return true;
				}
				SubmarineType type = this.item.Submarine.Info.Type;
				return type - SubmarineType.Wreck <= 3;
			}
		}

		// Token: 0x170011E3 RID: 4579
		// (get) Token: 0x06004325 RID: 17189 RVA: 0x001AF6F3 File Offset: 0x001AD8F3
		// (set) Token: 0x06004326 RID: 17190 RVA: 0x001AF6FB File Offset: 0x001AD8FB
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Locked connection panels cannot be rewired in-game.", "", true)]
		public bool Locked { get; set; }

		// Token: 0x170011E4 RID: 4580
		// (get) Token: 0x06004327 RID: 17191 RVA: 0x001AF704 File Offset: 0x001AD904
		public bool TemporarilyLocked
		{
			get
			{
				if (Level.IsLoadedOutpost)
				{
					DockingPort component = this.item.GetComponent<DockingPort>();
					return component != null && component.Docked;
				}
				return false;
			}
		}

		// Token: 0x170011E5 RID: 4581
		// (get) Token: 0x06004328 RID: 17192 RVA: 0x001AF725 File Offset: 0x001AD925
		// (set) Token: 0x06004329 RID: 17193 RVA: 0x001AF72D File Offset: 0x001AD92D
		public override bool IsActive
		{
			get
			{
				return base.IsActive;
			}
			set
			{
			}
		}

		// Token: 0x170011E6 RID: 4582
		// (get) Token: 0x0600432A RID: 17194 RVA: 0x001AF72F File Offset: 0x001AD92F
		public Character User
		{
			get
			{
				return this.user;
			}
		}

		// Token: 0x0600432B RID: 17195 RVA: 0x001AF738 File Offset: 0x001AD938
		public ConnectionPanel(Item item, ContentXElement element) : base(item, element)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				if (this.Connections.Count == 256)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(39, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Too many connections in the item ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(item.Prefab.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(" (> ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(256);
					defaultInterpolatedStringHandler.AppendLiteral(").");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					break;
				}
				string a = subElement.Name.ToString();
				if (!(a == "input"))
				{
					if (a == "output")
					{
						this.Connections.Add(new Connection(subElement, this.Connections.Count, this, IdRemap.DiscardId, false));
					}
				}
				else
				{
					this.Connections.Add(new Connection(subElement, this.Connections.Count, this, IdRemap.DiscardId, false));
				}
			}
			base.IsActive = true;
		}

		// Token: 0x0600432C RID: 17196 RVA: 0x001AF88C File Offset: 0x001ADA8C
		public override void OnMapLoaded()
		{
			if (this.linksInitialized)
			{
				return;
			}
			this.InitializeLinks();
		}

		// Token: 0x0600432D RID: 17197 RVA: 0x001AF8A0 File Offset: 0x001ADAA0
		public void InitializeLinks()
		{
			foreach (Connection c in this.Connections)
			{
				c.InitializeFromLoaded();
			}
			if (this.disconnectedWireIds != null)
			{
				foreach (ushort disconnectedWireId in this.disconnectedWireIds)
				{
					Item wireItem = Entity.FindEntityByID(disconnectedWireId) as Item;
					if (wireItem != null)
					{
						Wire wire = wireItem.GetComponent<Wire>();
						if (wire != null)
						{
							if (Item.ItemList.Any(delegate(Item it)
							{
								if (it != this.item)
								{
									ConnectionPanel component = it.GetComponent<ConnectionPanel>();
									return component != null && component.DisconnectedWires.Contains(wire);
								}
								return false;
							}))
							{
								if (wire.Item.body != null)
								{
									wire.Item.body.Enabled = false;
								}
								wire.IsActive = false;
								wire.UpdateSections();
							}
							this.DisconnectedWires.Add(wire);
							base.IsActive = true;
						}
					}
				}
			}
			this.linksInitialized = true;
		}

		// Token: 0x0600432E RID: 17198 RVA: 0x001AF9F8 File Offset: 0x001ADBF8
		public override void OnItemLoaded()
		{
			if (this.item.body != null && this.item.body.BodyType == BodyType.Dynamic)
			{
				Holdable holdable = this.item.GetComponent<Holdable>();
				if (holdable == null || !holdable.Attachable)
				{
					DebugConsole.ThrowError("Item \"" + this.item.Name + "\" has a ConnectionPanel component, but cannot be wired because it has an active physics body that cannot be attached to a wall. Remove the physics body or add a Holdable component with the Attachable attribute set to true.", null, null, false, false);
				}
			}
		}

		// Token: 0x0600432F RID: 17199 RVA: 0x001AFA60 File Offset: 0x001ADC60
		public void MoveConnectedWires(Vector2 amount)
		{
			ConnectionPanel.<>c__DisplayClass26_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.amount = amount;
			CS$<>8__locals1.wireNodeOffset = ((this.item.Submarine == null) ? Vector2.Zero : (this.item.Submarine.HiddenSubPosition + CS$<>8__locals1.amount));
			foreach (Connection c in this.Connections)
			{
				foreach (Wire wire in c.Wires)
				{
					if (wire != null)
					{
						this.<MoveConnectedWires>g__TryMoveWire|26_0(wire, ref CS$<>8__locals1);
					}
				}
			}
			foreach (Wire wire2 in this.DisconnectedWires)
			{
				this.<MoveConnectedWires>g__TryMoveWire|26_0(wire2, ref CS$<>8__locals1);
			}
		}

		// Token: 0x06004330 RID: 17200 RVA: 0x001AFB80 File Offset: 0x001ADD80
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.user == null || (this.user.SelectedItem != this.item && this.user.SelectedSecondaryItem != this.item))
			{
				if (this.user != null)
				{
					this.item.CreateServerEvent<ConnectionPanel>(this);
				}
				this.user = null;
				if (this.DisconnectedWires.Count == 0)
				{
					base.IsActive = false;
				}
				return;
			}
			if (!this.user.Enabled || !this.HasRequiredItems(this.user, false, null))
			{
				this.user = null;
				base.IsActive = false;
				return;
			}
			this.user.AnimController.UpdateUseItem(!this.user.IsClimbing, this.item.WorldPosition + new Vector2(0f, 100f) * ((float)Timing.TotalTime / 10f % 0.1f));
		}

		// Token: 0x06004331 RID: 17201 RVA: 0x001AFC6B File Offset: 0x001ADE6B
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x06004332 RID: 17202 RVA: 0x001AFC75 File Offset: 0x001ADE75
		public bool CanRewire()
		{
			Item container = this.item.Container;
			return ((container != null) ? container.GetComponent<CircuitBox>() : null) != null || this.item.body == null || this.item.body.BodyType != BodyType.Dynamic;
		}

		// Token: 0x06004333 RID: 17203 RVA: 0x001AFCB5 File Offset: 0x001ADEB5
		public override bool Select(Character picker)
		{
			if (!this.CanRewire())
			{
				return false;
			}
			this.user = picker;
			if (this.user != null)
			{
				this.item.CreateServerEvent<ConnectionPanel>(this);
			}
			base.IsActive = true;
			return true;
		}

		// Token: 0x06004334 RID: 17204 RVA: 0x001AFCE4 File Offset: 0x001ADEE4
		public override bool Use(float deltaTime, Character character = null)
		{
			return character != null && character == this.user;
		}

		// Token: 0x06004335 RID: 17205 RVA: 0x001AFCF8 File Offset: 0x001ADEF8
		public bool CheckCharacterSuccess(Character character)
		{
			if (character == null)
			{
				return false;
			}
			if (Screen.Selected == GameMain.SubEditorScreen)
			{
				return true;
			}
			Reactor reactor = this.item.GetComponent<Reactor>();
			if (reactor != null && MathUtils.NearlyEqual(reactor.CurrPowerConsumption, 0f, 0.0001f))
			{
				return true;
			}
			PowerContainer powerContainer = this.item.GetComponent<PowerContainer>();
			if (powerContainer != null && powerContainer.Charge <= 0f)
			{
				return true;
			}
			Powered powered = this.item.GetComponent<Powered>();
			if (powered != null && powerContainer == null && powered.Voltage < 0.1f)
			{
				return true;
			}
			float degreeOfSuccess = base.DegreeOfSuccess(character);
			if (Rand.Range(0f, 0.5f, Rand.RandSync.Unsynced) < degreeOfSuccess)
			{
				return true;
			}
			base.ApplyStatusEffects(ActionType.OnFailure, 1f, character, null, null, null, null, 1f);
			return false;
		}

		// Token: 0x06004336 RID: 17206 RVA: 0x001AFDC0 File Offset: 0x001ADFC0
		public override void Load(ContentXElement element, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(element, usePrefabValues, idRemap, isItemSwap);
			List<Connection> loadedConnections = new List<Connection>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString();
				if (!(a == "input"))
				{
					if (a == "output")
					{
						loadedConnections.Add(new Connection(subElement, loadedConnections.Count, this, idRemap, isItemSwap));
					}
				}
				else
				{
					loadedConnections.Add(new Connection(subElement, loadedConnections.Count, this, idRemap, isItemSwap));
				}
			}
			int i = 0;
			while (i < loadedConnections.Count && i < this.Connections.Count)
			{
				this.Connections[i].LoadedWires.Clear();
				this.Connections[i].LoadedWires.AddRange(loadedConnections[i].LoadedWires);
				i++;
			}
			this.disconnectedWireIds = element.GetAttributeUshortArray("disconnectedwires", Array.Empty<ushort>()).ToList<ushort>();
			for (int j = 0; j < this.disconnectedWireIds.Count; j++)
			{
				this.disconnectedWireIds[j] = idRemap.GetOffsetId((int)this.disconnectedWireIds[j]);
			}
		}

		// Token: 0x06004337 RID: 17207 RVA: 0x001AFF24 File Offset: 0x001AE124
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			foreach (Connection c in this.Connections)
			{
				c.Save(componentElement);
			}
			if (this.DisconnectedWires.Count > 0)
			{
				componentElement.Add(new XAttribute("disconnectedwires", string.Join<ushort>(",", from w in this.DisconnectedWires
				select w.Item.ID)));
			}
			return componentElement;
		}

		// Token: 0x06004338 RID: 17208 RVA: 0x001AFFD8 File Offset: 0x001AE1D8
		protected override void ShallowRemoveComponentSpecific()
		{
		}

		// Token: 0x06004339 RID: 17209 RVA: 0x001AFFDC File Offset: 0x001AE1DC
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			foreach (Wire wire in this.DisconnectedWires.ToList<Wire>())
			{
				if (wire.OtherConnection(null) == null)
				{
					wire.Item.Drop(null, true, true);
				}
			}
			this.DisconnectedWires.Clear();
			foreach (Connection c in this.Connections)
			{
				foreach (Wire wire2 in c.Wires.ToArray<Wire>())
				{
					if (wire2.OtherConnection(c) == null)
					{
						wire2.Item.Drop(null, true, true);
					}
					else
					{
						wire2.RemoveConnection(this.item);
					}
				}
				c.Grid = null;
			}
			foreach (Connection connection in this.Connections)
			{
				Powered.ChangedConnections.Remove(connection);
				connection.Recipients.Clear();
			}
			this.Connections.Clear();
		}

		// Token: 0x0600433A RID: 17210 RVA: 0x001B0148 File Offset: 0x001AE348
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
		}

		// Token: 0x0600433B RID: 17211 RVA: 0x001B014C File Offset: 0x001AE34C
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			msg.WriteByte((byte)this.Connections.Count);
			foreach (Connection connection in this.Connections)
			{
				msg.WriteVariableUInt32((uint)connection.Wires.Count);
				foreach (Wire wire in connection.Wires)
				{
					msg.WriteUInt16((((wire != null) ? wire.Item : null) == null) ? 0 : wire.Item.ID);
				}
			}
			msg.WriteUInt16((ushort)this.DisconnectedWires.Count);
			foreach (Wire disconnectedWire in this.DisconnectedWires)
			{
				msg.WriteUInt16(disconnectedWire.Item.ID);
			}
		}

		// Token: 0x0600433C RID: 17212 RVA: 0x001B0274 File Offset: 0x001AE474
		[CompilerGenerated]
		private void <MoveConnectedWires>g__TryMoveWire|26_0(Wire wire, ref ConnectionPanel.<>c__DisplayClass26_0 A_2)
		{
			List<Vector2> wireNodes = wire.GetNodes();
			if (wireNodes.Count == 0)
			{
				return;
			}
			if (Submarine.RectContains(this.item.Rect, wireNodes[0] + A_2.wireNodeOffset, false))
			{
				wire.MoveNode(0, A_2.amount);
				return;
			}
			if (Submarine.RectContains(this.item.Rect, wireNodes[wireNodes.Count - 1] + A_2.wireNodeOffset, false))
			{
				wire.MoveNode(wireNodes.Count - 1, A_2.amount);
			}
		}

		// Token: 0x04002026 RID: 8230
		private const int MaxConnectionCount = 256;

		// Token: 0x04002027 RID: 8231
		public readonly List<Connection> Connections = new List<Connection>();

		// Token: 0x04002028 RID: 8232
		private Character user;

		// Token: 0x04002029 RID: 8233
		public readonly HashSet<Wire> DisconnectedWires = new HashSet<Wire>();

		// Token: 0x0400202A RID: 8234
		private List<ushort> disconnectedWireIds;

		// Token: 0x0400202C RID: 8236
		private bool linksInitialized;
	}
}
