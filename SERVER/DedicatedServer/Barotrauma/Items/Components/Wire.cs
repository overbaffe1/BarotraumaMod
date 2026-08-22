using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004AF RID: 1199
	internal class Wire : ItemComponent, IDrawableComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x060043BA RID: 17338 RVA: 0x001B2644 File Offset: 0x001B0844
		public void CreateNetworkEvent()
		{
			if (GameMain.Server == null)
			{
				return;
			}
			int eventCount = Math.Max((int)Math.Ceiling((double)((float)this.nodes.Count / 30f)), 1);
			for (int i = 0; i < eventCount; i++)
			{
				this.item.CreateServerEvent<Wire>(this, new Wire.ServerEventData(i));
			}
		}

		// Token: 0x060043BB RID: 17339 RVA: 0x001B269C File Offset: 0x001B089C
		public override bool ValidateEventData(NetEntityEvent.IData data)
		{
			Wire.ServerEventData serverEventData;
			return base.TryExtractEventData<Wire.ServerEventData>(data, out serverEventData);
		}

		// Token: 0x060043BC RID: 17340 RVA: 0x001B26B4 File Offset: 0x001B08B4
		public void ServerEventWrite(IWriteMessage msg, Client c, NetEntityEvent.IData extraData = null)
		{
			Wire.ServerEventData eventData = base.ExtractEventData<Wire.ServerEventData>(extraData);
			int eventIndex = eventData.EventIndex;
			int nodeStartIndex = eventIndex * 30;
			int nodeCount = MathHelper.Clamp(this.nodes.Count - nodeStartIndex, 0, 30);
			msg.WriteRangedInteger(eventIndex, 0, (int)Math.Ceiling(8.5));
			msg.WriteRangedInteger(nodeCount, 0, 30);
			for (int i = nodeStartIndex; i < nodeStartIndex + nodeCount; i++)
			{
				msg.WriteSingle(this.nodes[i].X);
				msg.WriteSingle(this.nodes[i].Y);
			}
		}

		// Token: 0x060043BD RID: 17341 RVA: 0x001B2750 File Offset: 0x001B0950
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			int nodeCount = (int)msg.ReadByte();
			Vector2 lastNodePos = Vector2.Zero;
			if (nodeCount > 0)
			{
				lastNodePos = new Vector2(msg.ReadSingle(), msg.ReadSingle());
			}
			if (!this.item.CanClientAccess(c))
			{
				return;
			}
			if (this.nodes.Count > nodeCount)
			{
				this.nodes.RemoveRange(nodeCount, this.nodes.Count - nodeCount);
			}
			if (nodeCount > 0)
			{
				if (nodeCount > this.nodes.Count)
				{
					this.nodes.Add(lastNodePos);
				}
				else
				{
					this.nodes[this.nodes.Count - 1] = lastNodePos;
				}
			}
			this.CreateNetworkEvent();
		}

		// Token: 0x17001206 RID: 4614
		// (get) Token: 0x060043BE RID: 17342 RVA: 0x001B27F8 File Offset: 0x001B09F8
		// (set) Token: 0x060043BF RID: 17343 RVA: 0x001B285F File Offset: 0x001B0A5F
		public bool Locked
		{
			get
			{
				NetworkMember networkMember = GameMain.NetworkMember;
				if (((networkMember != null) ? networkMember.ServerSettings : null) != null && !GameMain.NetworkMember.ServerSettings.AllowRewiring)
				{
					return false;
				}
				if (!this.locked)
				{
					return this.connections.Any((Connection c) => c != null && (c.ConnectionPanel.Locked || c.ConnectionPanel.TemporarilyLocked));
				}
				return true;
			}
			set
			{
				this.locked = value;
			}
		}

		// Token: 0x17001207 RID: 4615
		// (get) Token: 0x060043C0 RID: 17344 RVA: 0x001B2868 File Offset: 0x001B0A68
		public Connection[] Connections
		{
			get
			{
				return this.connections;
			}
		}

		// Token: 0x17001208 RID: 4616
		// (get) Token: 0x060043C1 RID: 17345 RVA: 0x001B2870 File Offset: 0x001B0A70
		// (set) Token: 0x060043C2 RID: 17346 RVA: 0x001B2878 File Offset: 0x001B0A78
		public float Length { get; private set; }

		// Token: 0x17001209 RID: 4617
		// (get) Token: 0x060043C3 RID: 17347 RVA: 0x001B2881 File Offset: 0x001B0A81
		// (set) Token: 0x060043C4 RID: 17348 RVA: 0x001B2889 File Offset: 0x001B0A89
		[Serialize(0.3f, IsPropertySaveable.No, "", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f, DecimalCount = 2)]
		public float Width { get; set; }

		// Token: 0x1700120A RID: 4618
		// (get) Token: 0x060043C5 RID: 17349 RVA: 0x001B2892 File Offset: 0x001B0A92
		// (set) Token: 0x060043C6 RID: 17350 RVA: 0x001B289A File Offset: 0x001B0A9A
		[Serialize(5000f, IsPropertySaveable.No, "The maximum distance the wire can extend (in pixels).", "", false)]
		public float MaxLength { get; set; }

		// Token: 0x1700120B RID: 4619
		// (get) Token: 0x060043C7 RID: 17351 RVA: 0x001B28A3 File Offset: 0x001B0AA3
		// (set) Token: 0x060043C8 RID: 17352 RVA: 0x001B28AB File Offset: 0x001B0AAB
		[Serialize(false, IsPropertySaveable.No, "If enabled, the wire will not be visible in connection panels outside the submarine editor.", "", false)]
		public bool HiddenInGame { get; set; }

		// Token: 0x1700120C RID: 4620
		// (get) Token: 0x060043C9 RID: 17353 RVA: 0x001B28B4 File Offset: 0x001B0AB4
		// (set) Token: 0x060043CA RID: 17354 RVA: 0x001B28BC File Offset: 0x001B0ABC
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, this wire will be ignored by the \"Lock all default wires\" setting.", "", true)]
		public bool NoAutoLock { get; set; }

		// Token: 0x1700120D RID: 4621
		// (get) Token: 0x060043CB RID: 17355 RVA: 0x001B28C5 File Offset: 0x001B0AC5
		// (set) Token: 0x060043CC RID: 17356 RVA: 0x001B28CD File Offset: 0x001B0ACD
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, this wire will use the sprite depth instead of a constant depth.", "", false)]
		public bool UseSpriteDepth { get; set; }

		// Token: 0x1700120E RID: 4622
		// (get) Token: 0x060043CD RID: 17357 RVA: 0x001B28D6 File Offset: 0x001B0AD6
		// (set) Token: 0x060043CE RID: 17358 RVA: 0x001B28DE File Offset: 0x001B0ADE
		[Serialize(true, IsPropertySaveable.Yes, "If disabled, the wire will not be dropped when connecting. Used in circuit box to store the wires inside the box.", "", false)]
		public bool DropOnConnect { get; set; }

		// Token: 0x060043CF RID: 17359 RVA: 0x001B28E8 File Offset: 0x001B0AE8
		public Wire(Item item, ContentXElement element) : base(item, element)
		{
			this.nodes = new List<Vector2>();
			this.sections = new List<Wire.WireSection>();
			this.connections = new Connection[2];
			this.IsActive = false;
			item.IsShootable = true;
		}

		// Token: 0x060043D0 RID: 17360 RVA: 0x001B2934 File Offset: 0x001B0B34
		public Connection OtherConnection(Connection connection)
		{
			if (connection == this.connections[0])
			{
				return this.connections[1];
			}
			if (connection == this.connections[1])
			{
				return this.connections[0];
			}
			return null;
		}

		// Token: 0x060043D1 RID: 17361 RVA: 0x001B295F File Offset: 0x001B0B5F
		public bool IsConnectedTo(Item item)
		{
			return (this.connections[0] != null && this.connections[0].Item == item) || (this.connections[1] != null && this.connections[1].Item == item);
		}

		// Token: 0x060043D2 RID: 17362 RVA: 0x001B299C File Offset: 0x001B0B9C
		public void RemoveConnection(Item item)
		{
			for (int i = 0; i < 2; i++)
			{
				if (this.connections[i] != null && this.connections[i].Item == item)
				{
					if (this.connections[i].Wires.Contains(this))
					{
						this.SetConnectedDirty();
						this.connections[i].DisconnectWire(this);
					}
					this.connections[i] = null;
				}
			}
		}

		// Token: 0x060043D3 RID: 17363 RVA: 0x001B2A01 File Offset: 0x001B0C01
		public void RemoveConnection(Connection connection)
		{
			if (connection == this.connections[0])
			{
				this.connections[0] = null;
			}
			if (connection == this.connections[1])
			{
				this.connections[1] = null;
			}
			this.SetConnectedDirty();
		}

		// Token: 0x060043D4 RID: 17364 RVA: 0x001B2A31 File Offset: 0x001B0C31
		public bool TryConnect(Connection newConnection, bool addNode = true, bool sendNetworkEvent = false)
		{
			if (this.connections[0] == null)
			{
				return this.Connect(newConnection, 0, addNode, sendNetworkEvent);
			}
			return this.connections[1] == null && this.Connect(newConnection, 1, addNode, sendNetworkEvent);
		}

		// Token: 0x060043D5 RID: 17365 RVA: 0x001B2A60 File Offset: 0x001B0C60
		public bool Connect(Connection newConnection, int connectionIndex, bool addNode = true, bool sendNetworkEvent = false)
		{
			for (int i = 0; i < 2; i++)
			{
				if (this.connections[i] == newConnection)
				{
					return false;
				}
			}
			if (connectionIndex < 0 || connectionIndex > 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(57, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Error while connecting a wire to ");
				defaultInterpolatedStringHandler.AppendFormatted<Item>(newConnection.Item);
				defaultInterpolatedStringHandler.AppendLiteral(": ");
				defaultInterpolatedStringHandler.AppendFormatted<int>(connectionIndex);
				defaultInterpolatedStringHandler.AppendLiteral(" is not a valid index.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return false;
			}
			if (this.connections[connectionIndex] != null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(77, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Error while connecting a wire to ");
				defaultInterpolatedStringHandler2.AppendFormatted<Item>(newConnection.Item);
				defaultInterpolatedStringHandler2.AppendLiteral(": a wire is already connected to the index ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(connectionIndex);
				defaultInterpolatedStringHandler2.AppendLiteral(".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
				return false;
			}
			for (int j = 0; j < 2; j++)
			{
				if (this.connections[j] != null && this.connections[j].Item == newConnection.Item)
				{
					addNode = false;
					break;
				}
			}
			if (this.item.body != null)
			{
				this.item.Submarine = newConnection.Item.Submarine;
			}
			newConnection.ConnectionPanel.DisconnectedWires.Remove(this);
			this.connections[connectionIndex] = newConnection;
			this.FixNodeEnds();
			if (addNode)
			{
				this.AddNode(newConnection, connectionIndex);
			}
			this.SetConnectedDirty();
			if (this.DropOnConnect && this.connections[0] != null && this.connections[1] != null)
			{
				foreach (ItemComponent ic in this.item.Components)
				{
					if (ic != this)
					{
						ic.Drop(null, true);
					}
				}
				Item container = this.item.Container;
				if (container != null)
				{
					container.RemoveContained(this.item);
				}
				if (this.item.body != null)
				{
					this.item.body.Enabled = false;
				}
				this.IsActive = false;
				this.CleanNodes();
			}
			if (this.item.body != null)
			{
				this.item.Submarine = newConnection.Item.Submarine;
			}
			if (sendNetworkEvent)
			{
				if (GameMain.Server != null)
				{
					this.CreateNetworkEvent();
				}
				this.IsActive = (this.item.ParentInventory is CharacterInventory && (this.connections[0] == null ^ this.connections[1] == null));
			}
			base.Drawable = (this.IsActive || this.nodes.Any<Vector2>());
			this.UpdateSections();
			return true;
		}

		// Token: 0x060043D6 RID: 17366 RVA: 0x001B2D10 File Offset: 0x001B0F10
		private void AddNode(Connection newConnection, int selectedIndex)
		{
			Submarine refSub = newConnection.Item.Submarine;
			if (refSub == null)
			{
				Structure attachTarget = Structure.GetAttachTarget(newConnection.Item.WorldPosition);
				if (attachTarget == null)
				{
					Holdable component = newConnection.Item.GetComponent<Holdable>();
					if (component == null || !component.Attached)
					{
						this.connections[selectedIndex] = null;
						return;
					}
				}
				refSub = ((attachTarget != null) ? attachTarget.Submarine : null);
			}
			Vector2 nodePos = Wire.RoundNode(newConnection.Item.Position);
			if (refSub != null)
			{
				nodePos -= refSub.HiddenSubPosition;
			}
			if (this.nodes.Count > 0 && this.nodes[0] == nodePos)
			{
				return;
			}
			if (this.nodes.Count > 1 && this.nodes[this.nodes.Count - 1] == nodePos)
			{
				return;
			}
			int newNodeIndex = 0;
			if (this.nodes.Count > 1)
			{
				if (this.connections[0] != null && this.connections[0] != newConnection)
				{
					if (Vector2.DistanceSquared(this.nodes[0], this.connections[0].Item.Position - ((refSub != null) ? refSub.HiddenSubPosition : Vector2.Zero)) < Vector2.DistanceSquared(this.nodes[this.nodes.Count - 1], this.connections[0].Item.Position - ((refSub != null) ? refSub.HiddenSubPosition : Vector2.Zero)))
					{
						newNodeIndex = this.nodes.Count;
					}
				}
				else if (this.connections[1] != null && this.connections[1] != newConnection)
				{
					if (Vector2.DistanceSquared(this.nodes[0], this.connections[1].Item.Position - ((refSub != null) ? refSub.HiddenSubPosition : Vector2.Zero)) < Vector2.DistanceSquared(this.nodes[this.nodes.Count - 1], this.connections[1].Item.Position - ((refSub != null) ? refSub.HiddenSubPosition : Vector2.Zero)))
					{
						newNodeIndex = this.nodes.Count;
					}
				}
				else if (Vector2.DistanceSquared(this.nodes[this.nodes.Count - 1], nodePos) < Vector2.DistanceSquared(this.nodes[0], nodePos))
				{
					newNodeIndex = this.nodes.Count;
				}
			}
			if (newNodeIndex == 0 && this.nodes.Count > 1)
			{
				this.nodes.Insert(0, nodePos);
				return;
			}
			this.nodes.Add(nodePos);
		}

		// Token: 0x060043D7 RID: 17367 RVA: 0x001B2FB3 File Offset: 0x001B11B3
		public override void Equip(Character character)
		{
			if (this.shouldClearConnections)
			{
				this.ClearConnections(character);
			}
			this.IsActive = true;
		}

		// Token: 0x060043D8 RID: 17368 RVA: 0x001B2FCB File Offset: 0x001B11CB
		public override void Unequip(Character character)
		{
			this.ClearConnections(character);
			this.IsActive = false;
		}

		// Token: 0x060043D9 RID: 17369 RVA: 0x001B2FDB File Offset: 0x001B11DB
		public override void Drop(Character dropper, bool setTransform = true)
		{
			if (this.shouldClearConnections)
			{
				this.ClearConnections(dropper);
			}
			this.IsActive = false;
		}

		// Token: 0x060043DA RID: 17370 RVA: 0x001B2FF4 File Offset: 0x001B11F4
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.nodes.Count == 0)
			{
				return;
			}
			Inventory parentInventory = this.item.ParentInventory;
			Character user = ((parentInventory != null) ? parentInventory.Owner : null) as Character;
			this.editNodeDelay = ((((user != null) ? user.SelectedItem : null) == null) ? (this.editNodeDelay - deltaTime) : 0.5f);
			Submarine sub = this.item.Submarine;
			if (this.connections[0] != null && this.connections[0].Item.Submarine != null)
			{
				sub = this.connections[0].Item.Submarine;
			}
			if (this.connections[1] != null && this.connections[1].Item.Submarine != null)
			{
				sub = this.connections[1].Item.Submarine;
			}
			if (Screen.Selected != GameMain.SubEditorScreen)
			{
				if (user != null)
				{
					this.NoAutoLock = true;
				}
				if (this.item.Submarine != sub && sub != null && this.item.Submarine != null)
				{
					this.ClearConnections(null);
					return;
				}
				if (this.item.CurrentHull == null)
				{
					Structure attachTarget = Structure.GetAttachTarget(this.item.WorldPosition);
					this.canPlaceNode = (attachTarget != null);
					if (sub == null)
					{
						sub = ((attachTarget != null) ? attachTarget.Submarine : null);
					}
					Vector2 attachPos = this.GetAttachPosition(user);
					this.newNodePos = ((sub == null) ? attachPos : (attachPos - sub.Position - sub.HiddenSubPosition));
				}
				else
				{
					this.newNodePos = this.GetAttachPosition(user);
					if (sub != null)
					{
						this.newNodePos -= sub.HiddenSubPosition;
					}
					this.canPlaceNode = true;
				}
				if (this.nodes.Count > 0)
				{
					if (user == null)
					{
						return;
					}
					Vector2 prevNodePos = this.nodes[this.nodes.Count - 1];
					if (sub != null)
					{
						prevNodePos += sub.HiddenSubPosition;
					}
					this.currLength = 0f;
					for (int i = 0; i < this.nodes.Count - 1; i++)
					{
						this.currLength += Vector2.Distance(this.nodes[i], this.nodes[i + 1]);
					}
					Vector2 itemPos = this.item.Position;
					if (sub != null && user.Submarine == null)
					{
						prevNodePos += sub.Position;
					}
					this.currLength += Vector2.Distance(prevNodePos, itemPos);
					if (this.currLength > this.MaxLength)
					{
						Vector2 diff = prevNodePos - user.Position;
						Vector2 pullBackDir = (diff == Vector2.Zero) ? Vector2.Zero : Vector2.Normalize(diff);
						Vector2 forceDir = pullBackDir;
						if (!user.AnimController.InWater)
						{
							forceDir.Y = 0f;
						}
						user.AnimController.Collider.ApplyForce(forceDir * user.Mass * 50f, 32f);
						if (diff.LengthSquared() > 2500f)
						{
							user.AnimController.UpdateUseItem(!user.IsClimbing, user.WorldPosition + pullBackDir * Math.Min(150f, diff.Length()));
						}
						if ((GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && this.currLength > this.MaxLength * 1.5f)
						{
							this.ClearConnections(null);
							this.CreateNetworkEvent();
							return;
						}
					}
				}
			}
			else
			{
				this.newNodePos = Wire.RoundNode(this.item.Position);
				if (sub != null)
				{
					this.newNodePos -= sub.HiddenSubPosition;
				}
				this.canPlaceNode = true;
			}
			if (this.item != null)
			{
				Vector2 relativeNodePos = this.newNodePos - this.item.Position;
				if (sub != null)
				{
					relativeNodePos += sub.HiddenSubPosition;
				}
				this.sectionExtents = new Vector2(Math.Max(Math.Abs(relativeNodePos.X), this.sectionExtents.X), Math.Max(Math.Abs(relativeNodePos.Y), this.sectionExtents.Y));
			}
		}

		// Token: 0x060043DB RID: 17371 RVA: 0x001B3410 File Offset: 0x001B1610
		private Vector2 GetAttachPosition(Character user)
		{
			if (user == null)
			{
				return this.item.Position;
			}
			Vector2 mouseDiff = user.CursorWorldPosition - user.WorldPosition;
			mouseDiff = mouseDiff.ClampLength(150f);
			return Wire.RoundNode(user.Position + mouseDiff);
		}

		// Token: 0x060043DC RID: 17372 RVA: 0x001B345C File Offset: 0x001B165C
		public override bool Use(float deltaTime, Character character = null)
		{
			if (character == null || character != Character.Controlled)
			{
				return false;
			}
			if (character.HasSelectedAnyItem)
			{
				return false;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				return false;
			}
			if (this.newNodePos != Vector2.Zero && this.canPlaceNode && this.editNodeDelay <= 0f && this.nodes.Count > 0 && Vector2.DistanceSquared(this.newNodePos, this.nodes[this.nodes.Count - 1]) > 49f)
			{
				if (this.nodes.Count >= 255)
				{
					this.nodes.RemoveAt(this.nodes.Count - 1);
				}
				this.nodes.Add(this.newNodePos);
				this.CleanNodes();
				this.UpdateSections();
				base.Drawable = true;
				this.newNodePos = Vector2.Zero;
			}
			this.editNodeDelay = 0.1f;
			return true;
		}

		// Token: 0x060043DD RID: 17373 RVA: 0x001B3568 File Offset: 0x001B1768
		public override bool SecondaryUse(float deltaTime, Character character = null)
		{
			if (character == null || character != Character.Controlled)
			{
				return false;
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsServer)
			{
				return false;
			}
			if (this.nodes.Count > 1 && this.editNodeDelay <= 0f)
			{
				this.nodes.RemoveAt(this.nodes.Count - 1);
				this.UpdateSections();
			}
			this.editNodeDelay = 0.1f;
			base.Drawable = (this.IsActive || this.sections.Count > 0);
			return true;
		}

		// Token: 0x060043DE RID: 17374 RVA: 0x001B35FB File Offset: 0x001B17FB
		public override bool Pick(Character picker)
		{
			this.ClearConnections(picker);
			return true;
		}

		// Token: 0x060043DF RID: 17375 RVA: 0x001B3605 File Offset: 0x001B1805
		public List<Vector2> GetNodes()
		{
			return new List<Vector2>(this.nodes);
		}

		// Token: 0x060043E0 RID: 17376 RVA: 0x001B3612 File Offset: 0x001B1812
		public void SetNodes(IEnumerable<Vector2> nodes)
		{
			this.nodes = nodes.ToList<Vector2>();
			this.UpdateSections();
		}

		// Token: 0x060043E1 RID: 17377 RVA: 0x001B3628 File Offset: 0x001B1828
		public void MoveNode(int index, Vector2 amount)
		{
			if (index < 0 || index >= this.nodes.Count)
			{
				return;
			}
			List<Vector2> list = this.nodes;
			list[index] += amount;
			this.UpdateSections();
		}

		// Token: 0x060043E2 RID: 17378 RVA: 0x001B366C File Offset: 0x001B186C
		public void MoveNodes(Vector2 amount)
		{
			for (int i = 0; i < this.nodes.Count; i++)
			{
				List<Vector2> list = this.nodes;
				int index = i;
				list[index] += amount;
			}
			this.UpdateSections();
		}

		// Token: 0x060043E3 RID: 17379 RVA: 0x001B36B4 File Offset: 0x001B18B4
		public void UpdateSections()
		{
			this.sections.Clear();
			for (int i = 0; i < this.nodes.Count - 1; i++)
			{
				this.sections.Add(new Wire.WireSection(this.nodes[i], this.nodes[i + 1]));
			}
			base.Drawable = (this.IsActive || this.sections.Count > 0);
			float length;
			if (this.sections.Count <= 0)
			{
				length = 0f;
			}
			else
			{
				length = this.sections.Sum((Wire.WireSection s) => s.Length);
			}
			this.Length = length;
			this.CalculateExtents();
		}

		// Token: 0x060043E4 RID: 17380 RVA: 0x001B3778 File Offset: 0x001B1978
		private void CalculateExtents()
		{
			this.sectionExtents = Vector2.Zero;
			if (this.sections.Count > 0)
			{
				for (int i = 0; i < this.nodes.Count; i++)
				{
					this.sectionExtents.X = Math.Max(Math.Abs(this.nodes[i].X - this.item.Position.X), this.sectionExtents.X);
					this.sectionExtents.Y = Math.Max(Math.Abs(this.nodes[i].Y - this.item.Position.Y), this.sectionExtents.Y);
				}
			}
		}

		// Token: 0x060043E5 RID: 17381 RVA: 0x001B3844 File Offset: 0x001B1A44
		public void ClearConnections(Character user = null)
		{
			this.nodes.Clear();
			this.sections.Clear();
			foreach (Item item in Item.ItemList)
			{
				ConnectionPanel connectionPanel = item.GetComponent<ConnectionPanel>();
				if (connectionPanel != null && connectionPanel.DisconnectedWires.Contains(this) && !item.Removed)
				{
					item.CreateServerEvent<ConnectionPanel>(connectionPanel);
					connectionPanel.DisconnectedWires.Remove(this);
				}
			}
			if (user != null)
			{
				if (this.connections[0] != null || this.connections[1] != null)
				{
					GameMain.Server.KarmaManager.OnWireDisconnected(user, this);
				}
				if (this.connections[0] != null && this.connections[1] != null)
				{
					GameServer.Log(string.Concat(new string[]
					{
						GameServer.CharacterLogName(user),
						" disconnected a wire from ",
						this.connections[0].Item.Name,
						" (",
						this.connections[0].Name,
						") to ",
						this.connections[1].Item.Name,
						" (",
						this.connections[1].Name,
						")"
					}), ServerLog.MessageType.ItemInteraction);
				}
				else if (this.connections[0] != null)
				{
					GameServer.Log(string.Concat(new string[]
					{
						GameServer.CharacterLogName(user),
						" disconnected a wire from ",
						this.connections[0].Item.Name,
						" (",
						this.connections[0].Name,
						")"
					}), ServerLog.MessageType.ItemInteraction);
				}
				else if (this.connections[1] != null)
				{
					GameServer.Log(string.Concat(new string[]
					{
						GameServer.CharacterLogName(user),
						" disconnected a wire from ",
						this.connections[1].Item.Name,
						" (",
						this.connections[1].Name,
						")"
					}), ServerLog.MessageType.ItemInteraction);
				}
			}
			this.SetConnectedDirty();
			for (int i = 0; i < 2; i++)
			{
				if (this.connections[i] != null)
				{
					Wire wire = this.connections[i].FindWireByItem(this.item);
					if (wire != null)
					{
						if (!this.connections[i].Item.Removed)
						{
							Submarine submarine = this.connections[i].Item.Submarine;
							if (submarine == null || !submarine.Loading)
							{
								Level loaded = Level.Loaded;
								if (loaded == null || !loaded.Generating)
								{
									this.connections[i].Item.CreateServerEvent<ConnectionPanel>(this.connections[i].Item.GetComponent<ConnectionPanel>());
								}
							}
						}
						this.connections[i].DisconnectWire(wire);
						this.connections[i] = null;
					}
				}
			}
			base.Drawable = (this.sections.Count > 0);
		}

		// Token: 0x060043E6 RID: 17382 RVA: 0x001B3B54 File Offset: 0x001B1D54
		private static Vector2 RoundNode(Vector2 position)
		{
			Vector2 halfGrid = Submarine.GridSize / 2f;
			position += halfGrid;
			position.X = MathUtils.RoundTowardsClosest(position.X, Submarine.GridSize.X / 2f);
			position.Y = MathUtils.RoundTowardsClosest(position.Y, Submarine.GridSize.Y / 2f);
			return position - halfGrid;
		}

		// Token: 0x060043E7 RID: 17383 RVA: 0x001B3BC8 File Offset: 0x001B1DC8
		public void SetConnectedDirty()
		{
			for (int i = 0; i < 2; i++)
			{
				Connection connection = this.connections[i];
				if (((connection != null) ? connection.Item : null) != null)
				{
					PowerTransfer component = this.connections[i].Item.GetComponent<PowerTransfer>();
					if (component != null)
					{
						component.SetConnectionDirty(this.connections[i]);
					}
					this.connections[i].SetRecipientsDirty();
				}
			}
		}

		// Token: 0x060043E8 RID: 17384 RVA: 0x001B3C2C File Offset: 0x001B1E2C
		private void CleanNodes()
		{
			bool removed;
			do
			{
				removed = false;
				for (int i = this.nodes.Count - 2; i > 0; i--)
				{
					if (Math.Abs(this.nodes[i - 1].X - this.nodes[i].X) < 1f && Math.Abs(this.nodes[i + 1].X - this.nodes[i].X) < 1f && Math.Sign(this.nodes[i - 1].Y - this.nodes[i].Y) != Math.Sign(this.nodes[i + 1].Y - this.nodes[i].Y))
					{
						this.nodes.RemoveAt(i);
						removed = true;
					}
					else if (Math.Abs(this.nodes[i - 1].Y - this.nodes[i].Y) < 1f && Math.Abs(this.nodes[i + 1].Y - this.nodes[i].Y) < 1f && Math.Sign(this.nodes[i - 1].X - this.nodes[i].X) != Math.Sign(this.nodes[i + 1].X - this.nodes[i].X))
					{
						this.nodes.RemoveAt(i);
						removed = true;
					}
				}
			}
			while (removed);
		}

		// Token: 0x060043E9 RID: 17385 RVA: 0x001B3DF8 File Offset: 0x001B1FF8
		public void FixNodeEnds()
		{
			Connection connection = this.connections[0];
			Item item0 = (connection != null) ? connection.Item : null;
			Connection connection2 = this.connections[1];
			Item item = (connection2 != null) ? connection2.Item : null;
			if (item0 == null && item != null)
			{
				item0 = Item.ItemList.Find(delegate(Item it)
				{
					ConnectionPanel component = it.GetComponent<ConnectionPanel>();
					return component != null && component.DisconnectedWires.Contains(this);
				});
			}
			else if (item0 != null && item == null)
			{
				item = Item.ItemList.Find(delegate(Item it)
				{
					ConnectionPanel component = it.GetComponent<ConnectionPanel>();
					return component != null && component.DisconnectedWires.Contains(this);
				});
			}
			if (item0 == null || item == null || this.nodes.Count == 0)
			{
				return;
			}
			Vector2 nodePos = this.nodes[0];
			Submarine refSub = item0.Submarine ?? item.Submarine;
			if (refSub != null)
			{
				nodePos += refSub.HiddenSubPosition;
			}
			float dist = Vector2.DistanceSquared(item0.Position, nodePos);
			float dist2 = Vector2.DistanceSquared(item.Position, nodePos);
			if (dist > dist2)
			{
				this.nodes.Reverse();
				this.UpdateSections();
			}
		}

		// Token: 0x060043EA RID: 17386 RVA: 0x001B3EE0 File Offset: 0x001B20E0
		private int GetClosestNodeIndex(Vector2 pos, float maxDist, out float closestDist)
		{
			closestDist = 0f;
			int closestIndex = -1;
			for (int i = 0; i < this.nodes.Count; i++)
			{
				float dist = Vector2.Distance(this.nodes[i], pos);
				if (dist <= maxDist && (closestIndex == -1 || dist < closestDist))
				{
					closestIndex = i;
					closestDist = dist;
				}
			}
			return closestIndex;
		}

		// Token: 0x060043EB RID: 17387 RVA: 0x001B3F34 File Offset: 0x001B2134
		private int GetClosestSectionIndex(Vector2 mousePos, float maxDist, out float closestDist)
		{
			closestDist = 0f;
			int closestIndex = -1;
			maxDist *= maxDist;
			for (int i = 0; i < this.nodes.Count - 1; i++)
			{
				if ((Math.Abs(this.nodes[i].X - this.nodes[i + 1].X) < 5f || Math.Sign(mousePos.X - this.nodes[i].X) != Math.Sign(mousePos.X - this.nodes[i + 1].X)) && (Math.Abs(this.nodes[i].Y - this.nodes[i + 1].Y) < 5f || Math.Sign(mousePos.Y - this.nodes[i].Y) != Math.Sign(mousePos.Y - this.nodes[i + 1].Y)))
				{
					float dist = MathUtils.LineToPointDistanceSquared(this.nodes[i], this.nodes[i + 1], mousePos);
					if (dist <= maxDist && (closestIndex == -1 || dist < closestDist))
					{
						closestIndex = i;
						closestDist = dist;
					}
				}
			}
			closestDist = (float)Math.Sqrt((double)closestDist);
			return closestIndex;
		}

		// Token: 0x060043EC RID: 17388 RVA: 0x001B4090 File Offset: 0x001B2290
		public override void FlipX(bool relativeToSub)
		{
			if (this.item.ParentInventory != null)
			{
				return;
			}
			if (!relativeToSub)
			{
				return;
			}
			Vector2 refPos = (this.item.Submarine == null) ? Vector2.Zero : (this.item.Position - this.item.Submarine.HiddenSubPosition);
			for (int i = 0; i < this.nodes.Count; i++)
			{
				this.nodes[i] = (relativeToSub ? new Vector2(-this.nodes[i].X, this.nodes[i].Y) : new Vector2(refPos.X - (this.nodes[i].X - refPos.X), this.nodes[i].Y));
			}
			this.UpdateSections();
		}

		// Token: 0x060043ED RID: 17389 RVA: 0x001B4170 File Offset: 0x001B2370
		public override void FlipY(bool relativeToSub)
		{
			Vector2 refPos = (this.item.Submarine == null) ? Vector2.Zero : (this.item.Position - this.item.Submarine.HiddenSubPosition);
			for (int i = 0; i < this.nodes.Count; i++)
			{
				this.nodes[i] = (relativeToSub ? new Vector2(this.nodes[i].X, -this.nodes[i].Y) : new Vector2(this.nodes[i].X, refPos.Y - (this.nodes[i].Y - refPos.Y)));
			}
			this.UpdateSections();
		}

		// Token: 0x060043EE RID: 17390 RVA: 0x001B423C File Offset: 0x001B243C
		public static IEnumerable<Vector2> ExtractNodes(XElement element)
		{
			Wire.<ExtractNodes>d__86 <ExtractNodes>d__ = new Wire.<ExtractNodes>d__86(-2);
			<ExtractNodes>d__.<>3__element = element;
			return <ExtractNodes>d__;
		}

		// Token: 0x060043EF RID: 17391 RVA: 0x001B424C File Offset: 0x001B244C
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			this.nodes.AddRange(Wire.ExtractNodes(componentElement));
			base.Drawable = this.nodes.Any<Vector2>();
		}

		// Token: 0x060043F0 RID: 17392 RVA: 0x001B4280 File Offset: 0x001B2480
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			if (this.nodes == null || this.nodes.Count == 0)
			{
				return componentElement;
			}
			string[] nodeCoords = new string[this.nodes.Count * 2];
			for (int i = 0; i < this.nodes.Count; i++)
			{
				string[] array = nodeCoords;
				int num = i * 2;
				Vector2 vector = this.nodes[i];
				array[num] = vector.X.ToString(CultureInfo.InvariantCulture);
				string[] array2 = nodeCoords;
				int num2 = i * 2 + 1;
				vector = this.nodes[i];
				array2[num2] = vector.Y.ToString(CultureInfo.InvariantCulture);
			}
			componentElement.Add(new XAttribute("nodes", string.Join(";", nodeCoords)));
			return componentElement;
		}

		// Token: 0x060043F1 RID: 17393 RVA: 0x001B433E File Offset: 0x001B253E
		protected override void ShallowRemoveComponentSpecific()
		{
		}

		// Token: 0x060043F2 RID: 17394 RVA: 0x001B4340 File Offset: 0x001B2540
		protected override void RemoveComponentSpecific()
		{
			Item container = this.item.Container;
			CircuitBox circuitBox = (container != null) ? container.GetComponent<CircuitBox>() : null;
			if (circuitBox != null)
			{
				circuitBox.RemoveWire(this);
			}
			this.ClearConnections(null);
			base.RemoveComponentSpecific();
		}

		// Token: 0x04002061 RID: 8289
		private bool shouldClearConnections = true;

		// Token: 0x04002062 RID: 8290
		private const float MaxAttachDistance = 150f;

		// Token: 0x04002063 RID: 8291
		private const float MinNodeDistance = 7f;

		// Token: 0x04002064 RID: 8292
		private const int MaxNodeCount = 255;

		// Token: 0x04002065 RID: 8293
		private const int MaxNodesPerNetworkEvent = 30;

		// Token: 0x04002066 RID: 8294
		private List<Vector2> nodes;

		// Token: 0x04002067 RID: 8295
		private readonly List<Wire.WireSection> sections;

		// Token: 0x04002068 RID: 8296
		private readonly Connection[] connections;

		// Token: 0x04002069 RID: 8297
		private bool canPlaceNode;

		// Token: 0x0400206A RID: 8298
		private Vector2 newNodePos;

		// Token: 0x0400206B RID: 8299
		private Vector2 sectionExtents;

		// Token: 0x0400206C RID: 8300
		private float currLength;

		// Token: 0x0400206D RID: 8301
		public bool Hidden;

		// Token: 0x0400206E RID: 8302
		private float editNodeDelay;

		// Token: 0x0400206F RID: 8303
		private bool locked;

		// Token: 0x02000DF6 RID: 3574
		private readonly struct ServerEventData : ItemComponent.IEventData
		{
			// Token: 0x060068EF RID: 26863 RVA: 0x00223BAD File Offset: 0x00221DAD
			public ServerEventData(int eventIndex)
			{
				this.EventIndex = eventIndex;
			}

			// Token: 0x0400414C RID: 16716
			public readonly int EventIndex;
		}

		// Token: 0x02000DF7 RID: 3575
		public class WireSection
		{
			// Token: 0x17001691 RID: 5777
			// (get) Token: 0x060068F0 RID: 26864 RVA: 0x00223BB6 File Offset: 0x00221DB6
			public Vector2 Start
			{
				get
				{
					return this.start;
				}
			}

			// Token: 0x17001692 RID: 5778
			// (get) Token: 0x060068F1 RID: 26865 RVA: 0x00223BBE File Offset: 0x00221DBE
			public Vector2 End
			{
				get
				{
					return this.end;
				}
			}

			// Token: 0x060068F2 RID: 26866 RVA: 0x00223BC6 File Offset: 0x00221DC6
			public WireSection(Vector2 start, Vector2 end)
			{
				this.start = start;
				this.end = end;
				this.angle = MathUtils.VectorToAngle(end - start);
				this.Length = Vector2.Distance(start, end);
			}

			// Token: 0x0400414D RID: 16717
			private Vector2 start;

			// Token: 0x0400414E RID: 16718
			private Vector2 end;

			// Token: 0x0400414F RID: 16719
			private readonly float angle;

			// Token: 0x04004150 RID: 16720
			public readonly float Length;
		}
	}
}
