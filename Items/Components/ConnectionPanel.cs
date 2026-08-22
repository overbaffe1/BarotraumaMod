using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;
using Barotrauma.Particles;
using Barotrauma.Sounds;
using FarseerPhysics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D9 RID: 1497
	internal class ConnectionPanel : ItemComponent, IServerSerializable, INetSerializable, IClientSerializable
	{
		// Token: 0x17001864 RID: 6244
		// (get) Token: 0x060060AA RID: 24746 RVA: 0x00325380 File Offset: 0x00323580
		public static bool ShouldDebugDrawWiring
		{
			get
			{
				return ConnectionPanel.DebugWiringMode || Timing.TotalTimeUnpaused < ConnectionPanel.DebugWiringEnabledUntil;
			}
		}

		// Token: 0x17001865 RID: 6245
		// (get) Token: 0x060060AB RID: 24747 RVA: 0x00325397 File Offset: 0x00323597
		public override bool RecreateGUIOnResolutionChange
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060060AC RID: 24748 RVA: 0x0032539C File Offset: 0x0032359C
		protected override void CreateGUI()
		{
			if (base.GuiFrame == null)
			{
				return;
			}
			this.CheckForLabelOverlap();
			GUICustomComponent content = new GUICustomComponent(new RectTransform(Vector2.One, base.GuiFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), new Action<SpriteBatch, GUICustomComponent>(this.DrawConnections), null)
			{
				UserData = this
			};
			content.RectTransform.SetAsFirstChild();
			this.dragArea = new GUIFrame(new RectTransform(base.GuiFrame.Rect.Size - GUIStyle.ItemFrameMargin, base.GuiFrame.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = GUIStyle.ItemFrameOffset
			}, null, null);
		}

		// Token: 0x060060AD RID: 24749 RVA: 0x00325469 File Offset: 0x00323669
		public void TriggerRewiringSound()
		{
			this.rewireSoundTimer = 5f;
		}

		// Token: 0x060060AE RID: 24750 RVA: 0x00325476 File Offset: 0x00323676
		public override void Move(Vector2 amount, bool ignoreContacts = false)
		{
			if (this.item.Submarine == null || this.item.Submarine.Loading || Screen.Selected != GameMain.SubEditorScreen)
			{
				return;
			}
			this.MoveConnectedWires(amount);
		}

		// Token: 0x060060AF RID: 24751 RVA: 0x003254AB File Offset: 0x003236AB
		protected override bool ShouldDrawHUDComponentSpecific(Character character)
		{
			return character == Character.Controlled && character == this.user && (character.SelectedItem == this.item || character.SelectedSecondaryItem == this.item);
		}

		// Token: 0x060060B0 RID: 24752 RVA: 0x003254E0 File Offset: 0x003236E0
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			if (character != Character.Controlled || character != this.user || character.SelectedItem != this.item)
			{
				return;
			}
			if (ConnectionPanel.HighlightedWire != null)
			{
				ConnectionPanel.HighlightedWire.Item.IsHighlighted = true;
				if (ConnectionPanel.HighlightedWire.Connections[0] != null && ConnectionPanel.HighlightedWire.Connections[0].Item != null)
				{
					ConnectionPanel.HighlightedWire.Connections[0].Item.IsHighlighted = true;
				}
				if (ConnectionPanel.HighlightedWire.Connections[1] != null && ConnectionPanel.HighlightedWire.Connections[1].Item != null)
				{
					ConnectionPanel.HighlightedWire.Connections[1].Item.IsHighlighted = true;
				}
			}
		}

		// Token: 0x060060B1 RID: 24753 RVA: 0x00325598 File Offset: 0x00323798
		private void DrawConnections(SpriteBatch spriteBatch, GUICustomComponent container)
		{
			if (this.user != Character.Controlled || this.user == null)
			{
				return;
			}
			ConnectionPanel.HighlightedWire = null;
			ValueTuple<Vector2, LocalizedString> tooltip;
			Connection.DrawConnections(spriteBatch, this, this.dragArea.Rect, this.user, out tooltip);
			foreach (UISprite sprite in GUIStyle.GetComponentStyle("ConnectionPanelFront").Sprites[GUIComponent.ComponentState.None])
			{
				sprite.Draw(spriteBatch, base.GuiFrame.Rect, Color.White, SpriteEffects.None, null);
			}
			if (!tooltip.Item2.IsNullOrEmpty())
			{
				GUIComponent.DrawToolTip(spriteBatch, tooltip.Item2, tooltip.Item1, null, null);
			}
		}

		// Token: 0x060060B2 RID: 24754 RVA: 0x00325684 File Offset: 0x00323884
		private void CheckForLabelOverlap()
		{
			base.GuiFrame.RectTransform.MaxSize = this.originalMaxSize;
			base.GuiFrame.RectTransform.Resize(this.originalRelativeSize, true);
			Point newRectSize;
			if (Connection.CheckConnectionLabelOverlap(this, out newRectSize))
			{
				int xCenter = (int)((float)GameMain.GraphicsWidth / 2f);
				int maxNewWidth = 2 * Math.Min(xCenter - HUDLayoutSettings.CrewArea.Right, xCenter - HUDLayoutSettings.ChatBoxArea.Right);
				int yCenter = (int)((float)GameMain.GraphicsHeight / 2f);
				int maxNewHeight = 2 * Math.Min(yCenter - HUDLayoutSettings.MessageAreaTop.Bottom, HUDLayoutSettings.InventoryTopY - yCenter);
				newRectSize = new Point(Math.Min(newRectSize.X, maxNewWidth), Math.Min(newRectSize.Y, maxNewHeight));
				base.GuiFrame.RectTransform.MaxSize = new Point(Math.Max(base.GuiFrame.RectTransform.MaxSize.X, newRectSize.X), Math.Max(base.GuiFrame.RectTransform.MaxSize.Y, newRectSize.Y));
				base.GuiFrame.RectTransform.Resize(newRectSize, true);
			}
		}

		// Token: 0x060060B3 RID: 24755 RVA: 0x003257B8 File Offset: 0x003239B8
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			if (GameMain.Client.MidRoundSyncing)
			{
				long msgStartPos = (long)msg.BitPosition;
				msg.ReadUInt16();
				byte connectionCount = msg.ReadByte();
				for (int i = 0; i < (int)connectionCount; i++)
				{
					uint wireCount = msg.ReadVariableUInt32();
					int j = 0;
					while ((long)j < (long)((ulong)wireCount))
					{
						msg.ReadUInt16();
						j++;
					}
				}
				ushort disconnectedWireCount = msg.ReadUInt16();
				for (int k = 0; k < (int)disconnectedWireCount; k++)
				{
					msg.ReadUInt16();
				}
				int msgLength = (int)((long)msg.BitPosition - msgStartPos);
				msg.BitPosition = (int)msgStartPos;
				base.StartDelayedCorrection(msg.ExtractBits(msgLength), sendingTime, true);
				return;
			}
			if (Character.Controlled == null || this.user != Character.Controlled)
			{
				this.TriggerRewiringSound();
			}
			this.ApplyRemoteState(msg);
		}

		// Token: 0x060060B4 RID: 24756 RVA: 0x00325880 File Offset: 0x00323A80
		private void ApplyRemoteState(IReadMessage msg)
		{
			List<Wire> prevWires = this.Connections.SelectMany((Connection c) => c.Wires).ToList<Wire>();
			ushort userID = msg.ReadUInt16();
			if (userID == 0)
			{
				this.user = null;
			}
			else
			{
				this.user = (Entity.FindEntityByID(userID) as Character);
				base.IsActive = true;
			}
			foreach (Connection connection2 in this.Connections)
			{
				connection2.ClearConnections();
			}
			byte connectionCount = msg.ReadByte();
			for (int i = 0; i < (int)connectionCount; i++)
			{
				HashSet<Wire> newWires = new HashSet<Wire>();
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
							newWires.Add(wireComponent);
						}
					}
					j++;
				}
				if (i < this.Connections.Count)
				{
					Connection connection = this.Connections[i];
					Wire[] oldWires = (from w in connection.Wires
					where !newWires.Contains(w)
					select w).ToArray<Wire>();
					foreach (Wire wire in oldWires)
					{
						connection.DisconnectWire(wire);
					}
					IEnumerable<Wire> newWires2 = newWires;
					Func<Wire, bool> predicate;
					Func<Wire, bool> <>9__2;
					if ((predicate = <>9__2) == null)
					{
						predicate = (<>9__2 = ((Wire w) => !connection.Wires.Contains(w)));
					}
					foreach (Wire wire2 in newWires2.Where(predicate).ToArray<Wire>())
					{
						connection.ConnectWire(wire2);
						wire2.TryConnect(connection, false, false);
					}
				}
			}
			List<Wire> previousDisconnectedWires = new List<Wire>(this.DisconnectedWires);
			this.DisconnectedWires.Clear();
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
						this.DisconnectedWires.Add(wireComponent2);
						base.IsActive = true;
					}
				}
			}
			foreach (Wire wire3 in prevWires)
			{
				bool connected = wire3.Connections[0] != null || wire3.Connections[1] != null;
				if (!connected)
				{
					foreach (Item item in Item.ItemList)
					{
						ConnectionPanel connectionPanel = item.GetComponent<ConnectionPanel>();
						if (connectionPanel != null && connectionPanel.DisconnectedWires.Contains(wire3))
						{
							connected = true;
							break;
						}
					}
				}
				if (wire3.Item.ParentInventory == null && !connected)
				{
					wire3.Item.Drop(null, true, true);
				}
			}
			foreach (Wire disconnectedWire in previousDisconnectedWires)
			{
				if (disconnectedWire.Connections[0] == null && disconnectedWire.Connections[1] == null && !this.DisconnectedWires.Contains(disconnectedWire))
				{
					disconnectedWire.Item.Drop(null, true, true);
				}
			}
		}

		// Token: 0x17001866 RID: 6246
		// (get) Token: 0x060060B5 RID: 24757 RVA: 0x00325C50 File Offset: 0x00323E50
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

		// Token: 0x17001867 RID: 6247
		// (get) Token: 0x060060B6 RID: 24758 RVA: 0x00325C8B File Offset: 0x00323E8B
		// (set) Token: 0x060060B7 RID: 24759 RVA: 0x00325C93 File Offset: 0x00323E93
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Locked connection panels cannot be rewired in-game.", "", true)]
		public bool Locked { get; set; }

		// Token: 0x17001868 RID: 6248
		// (get) Token: 0x060060B8 RID: 24760 RVA: 0x00325C9C File Offset: 0x00323E9C
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

		// Token: 0x17001869 RID: 6249
		// (get) Token: 0x060060B9 RID: 24761 RVA: 0x00325CBD File Offset: 0x00323EBD
		// (set) Token: 0x060060BA RID: 24762 RVA: 0x00325CC5 File Offset: 0x00323EC5
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

		// Token: 0x1700186A RID: 6250
		// (get) Token: 0x060060BB RID: 24763 RVA: 0x00325CC7 File Offset: 0x00323EC7
		public Character User
		{
			get
			{
				return this.user;
			}
		}

		// Token: 0x060060BC RID: 24764 RVA: 0x00325CD0 File Offset: 0x00323ED0
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
			this.InitProjSpecific();
		}

		// Token: 0x060060BD RID: 24765 RVA: 0x00325E2C File Offset: 0x0032402C
		private void InitProjSpecific()
		{
			if (base.GuiFrame == null)
			{
				return;
			}
			this.originalMaxSize = base.GuiFrame.RectTransform.MaxSize;
			this.originalRelativeSize = base.GuiFrame.RectTransform.RelativeSize;
			this.CreateGUI();
		}

		// Token: 0x060060BE RID: 24766 RVA: 0x00325E69 File Offset: 0x00324069
		public override void OnMapLoaded()
		{
			if (this.linksInitialized)
			{
				return;
			}
			this.InitializeLinks();
		}

		// Token: 0x060060BF RID: 24767 RVA: 0x00325E7C File Offset: 0x0032407C
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

		// Token: 0x060060C0 RID: 24768 RVA: 0x00325FD4 File Offset: 0x003241D4
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

		// Token: 0x060060C1 RID: 24769 RVA: 0x0032603C File Offset: 0x0032423C
		public void MoveConnectedWires(Vector2 amount)
		{
			ConnectionPanel.<>c__DisplayClass46_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.amount = amount;
			CS$<>8__locals1.wireNodeOffset = ((this.item.Submarine == null) ? Vector2.Zero : (this.item.Submarine.HiddenSubPosition + CS$<>8__locals1.amount));
			foreach (Connection c in this.Connections)
			{
				foreach (Wire wire in c.Wires)
				{
					if (wire != null)
					{
						this.<MoveConnectedWires>g__TryMoveWire|46_0(wire, ref CS$<>8__locals1);
					}
				}
			}
			foreach (Wire wire2 in this.DisconnectedWires)
			{
				this.<MoveConnectedWires>g__TryMoveWire|46_0(wire2, ref CS$<>8__locals1);
			}
		}

		// Token: 0x060060C2 RID: 24770 RVA: 0x0032615C File Offset: 0x0032435C
		public override void Update(float deltaTime, Camera cam)
		{
			this.UpdateProjSpecific(deltaTime);
			if (this.user == null || (this.user.SelectedItem != this.item && this.user.SelectedSecondaryItem != this.item))
			{
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

		// Token: 0x060060C3 RID: 24771 RVA: 0x0032623A File Offset: 0x0032443A
		public override void UpdateBroken(float deltaTime, Camera cam)
		{
			this.Update(deltaTime, cam);
		}

		// Token: 0x060060C4 RID: 24772 RVA: 0x00326244 File Offset: 0x00324444
		private void UpdateProjSpecific(float deltaTime)
		{
			foreach (Wire _ in this.DisconnectedWires)
			{
				if (Rand.Range(0f, 500f, Rand.RandSync.Unsynced) < 1f)
				{
					string soundTag = "zap";
					Vector2 worldPosition = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					SoundPlayer.PlaySound(soundTag, worldPosition, null, null, currentHull);
					Vector2 baseVel = new Vector2(0f, -100f);
					for (int i = 0; i < 5; i++)
					{
						Particle particle = GameMain.ParticleManager.CreateParticle("spark", this.item.WorldPosition, baseVel + Rand.Vector(100f, Rand.RandSync.Unsynced), 0f, this.item.CurrentHull, 0f, null);
						if (particle != null)
						{
							particle.Size *= Rand.Range(0.5f, 1f, Rand.RandSync.Unsynced);
						}
					}
				}
			}
			this.rewireSoundTimer -= deltaTime;
			if (this.user != null && this.user.SelectedItem == this.item && this.rewireSoundTimer > 0f)
			{
				if (this.rewireSoundChannel == null || !this.rewireSoundChannel.IsPlaying)
				{
					string soundTag2 = "rewire";
					Vector2 worldPosition2 = this.item.WorldPosition;
					Hull currentHull = this.item.CurrentHull;
					this.rewireSoundChannel = SoundPlayer.PlaySound(soundTag2, worldPosition2, null, null, currentHull);
					return;
				}
			}
			else
			{
				SoundChannel soundChannel = this.rewireSoundChannel;
				if (soundChannel != null)
				{
					soundChannel.FadeOutAndDispose();
				}
				this.rewireSoundChannel = null;
				this.rewireSoundTimer = 0f;
			}
		}

		// Token: 0x060060C5 RID: 24773 RVA: 0x0032641C File Offset: 0x0032461C
		public bool CanRewire()
		{
			Item container = this.item.Container;
			return ((container != null) ? container.GetComponent<CircuitBox>() : null) != null || this.item.body == null || this.item.body.BodyType != BodyType.Dynamic;
		}

		// Token: 0x060060C6 RID: 24774 RVA: 0x0032645C File Offset: 0x0032465C
		public override bool Select(Character picker)
		{
			if (!this.CanRewire())
			{
				return false;
			}
			this.user = picker;
			base.IsActive = true;
			return true;
		}

		// Token: 0x060060C7 RID: 24775 RVA: 0x00326477 File Offset: 0x00324677
		public override bool Use(float deltaTime, Character character = null)
		{
			return character != null && character == this.user;
		}

		// Token: 0x060060C8 RID: 24776 RVA: 0x00326488 File Offset: 0x00324688
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

		// Token: 0x060060C9 RID: 24777 RVA: 0x00326550 File Offset: 0x00324750
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

		// Token: 0x060060CA RID: 24778 RVA: 0x003266B4 File Offset: 0x003248B4
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

		// Token: 0x060060CB RID: 24779 RVA: 0x00326768 File Offset: 0x00324968
		protected override void ShallowRemoveComponentSpecific()
		{
		}

		// Token: 0x060060CC RID: 24780 RVA: 0x0032676C File Offset: 0x0032496C
		protected override void RemoveComponentSpecific()
		{
			base.RemoveComponentSpecific();
			foreach (Wire wire in this.DisconnectedWires.ToList<Wire>())
			{
				if (wire.OtherConnection(null) == null)
				{
					if (SubEditorScreen.IsSubEditor())
					{
						wire.Item.Remove();
					}
					else
					{
						wire.Item.Drop(null, true, true);
					}
				}
			}
			this.DisconnectedWires.Clear();
			foreach (Connection c in this.Connections)
			{
				foreach (Wire wire2 in c.Wires.ToArray<Wire>())
				{
					if (wire2.OtherConnection(c) == null)
					{
						if (SubEditorScreen.IsSubEditor())
						{
							wire2.Item.Remove();
						}
						else
						{
							wire2.Item.Drop(null, true, true);
						}
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
			SoundChannel soundChannel = this.rewireSoundChannel;
			if (soundChannel != null)
			{
				soundChannel.FadeOutAndDispose();
			}
			this.rewireSoundChannel = null;
		}

		// Token: 0x060060CD RID: 24781 RVA: 0x00326918 File Offset: 0x00324B18
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
		}

		// Token: 0x060060CE RID: 24782 RVA: 0x0032691C File Offset: 0x00324B1C
		public void ClientEventWrite(IWriteMessage msg, NetEntityEvent.IData extraData = null)
		{
			this.TriggerRewiringSound();
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

		// Token: 0x060060CF RID: 24783 RVA: 0x00326A4C File Offset: 0x00324C4C
		[CompilerGenerated]
		private void <MoveConnectedWires>g__TryMoveWire|46_0(Wire wire, ref ConnectionPanel.<>c__DisplayClass46_0 A_2)
		{
			if (wire.Item.IsSelected)
			{
				return;
			}
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

		// Token: 0x040031EE RID: 12782
		public static bool DebugWiringMode;

		// Token: 0x040031EF RID: 12783
		public static double DebugWiringEnabledUntil;

		// Token: 0x040031F0 RID: 12784
		private const float RewireSoundDuration = 5f;

		// Token: 0x040031F1 RID: 12785
		public static Wire HighlightedWire;

		// Token: 0x040031F2 RID: 12786
		private SoundChannel rewireSoundChannel;

		// Token: 0x040031F3 RID: 12787
		private float rewireSoundTimer;

		// Token: 0x040031F4 RID: 12788
		private Point originalMaxSize;

		// Token: 0x040031F5 RID: 12789
		private Vector2 originalRelativeSize;

		// Token: 0x040031F6 RID: 12790
		private GUIComponent dragArea;

		// Token: 0x040031F7 RID: 12791
		private const int MaxConnectionCount = 256;

		// Token: 0x040031F8 RID: 12792
		public readonly List<Connection> Connections = new List<Connection>();

		// Token: 0x040031F9 RID: 12793
		private Character user;

		// Token: 0x040031FA RID: 12794
		public readonly HashSet<Wire> DisconnectedWires = new HashSet<Wire>();

		// Token: 0x040031FB RID: 12795
		private List<ushort> disconnectedWireIds;

		// Token: 0x040031FD RID: 12797
		private bool linksInitialized;
	}
}
