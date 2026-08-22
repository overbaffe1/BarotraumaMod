using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D7 RID: 1495
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CircuitBox : ItemComponent, IClientSerializable, INetSerializable, IServerSerializable
	{
		// Token: 0x1700184B RID: 6219
		// (get) Token: 0x06006009 RID: 24585 RVA: 0x0031FC0E File Offset: 0x0031DE0E
		// (set) Token: 0x0600600A RID: 24586 RVA: 0x0031FC16 File Offset: 0x0031DE16
		[Nullable(2)]
		public Sprite WireSprite { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x1700184C RID: 6220
		// (get) Token: 0x0600600B RID: 24587 RVA: 0x0031FC1F File Offset: 0x0031DE1F
		// (set) Token: 0x0600600C RID: 24588 RVA: 0x0031FC27 File Offset: 0x0031DE27
		[Nullable(2)]
		public Sprite ConnectionSprite { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x1700184D RID: 6221
		// (get) Token: 0x0600600D RID: 24589 RVA: 0x0031FC30 File Offset: 0x0031DE30
		// (set) Token: 0x0600600E RID: 24590 RVA: 0x0031FC38 File Offset: 0x0031DE38
		[Nullable(2)]
		public Sprite WireConnectorSprite { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x1700184E RID: 6222
		// (get) Token: 0x0600600F RID: 24591 RVA: 0x0031FC41 File Offset: 0x0031DE41
		// (set) Token: 0x06006010 RID: 24592 RVA: 0x0031FC49 File Offset: 0x0031DE49
		[Nullable(2)]
		public Sprite ConnectionScrewSprite { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x1700184F RID: 6223
		// (get) Token: 0x06006011 RID: 24593 RVA: 0x0031FC52 File Offset: 0x0031DE52
		// (set) Token: 0x06006012 RID: 24594 RVA: 0x0031FC5A File Offset: 0x0031DE5A
		[Nullable(2)]
		public UISprite NodeFrameSprite { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x17001850 RID: 6224
		// (get) Token: 0x06006013 RID: 24595 RVA: 0x0031FC63 File Offset: 0x0031DE63
		// (set) Token: 0x06006014 RID: 24596 RVA: 0x0031FC6B File Offset: 0x0031DE6B
		[Nullable(2)]
		public UISprite NodeTopSprite { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x06006015 RID: 24597 RVA: 0x0031FC74 File Offset: 0x0031DE74
		protected override void CreateGUI()
		{
			base.CreateGUI();
			base.GuiFrame.ClearChildren();
			CircuitBoxUI ui = this.UI;
			if (ui == null)
			{
				return;
			}
			ui.CreateGUI(base.GuiFrame);
		}

		// Token: 0x06006016 RID: 24598 RVA: 0x0031FC9D File Offset: 0x0031DE9D
		protected override bool ShouldDrawHUDComponentSpecific(Character character)
		{
			return character == Character.Controlled && (character.SelectedItem == this.item || character.SelectedSecondaryItem == this.item);
		}

		// Token: 0x06006017 RID: 24599 RVA: 0x0031FCC8 File Offset: 0x0031DEC8
		public override void UpdateHUDComponentSpecific(Character character, float deltaTime, Camera cam)
		{
			CircuitBox.<>c__DisplayClass35_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			if (this.UI == null)
			{
				return;
			}
			this.UI.Update(deltaTime);
			if (GameMain.NetworkMember == null)
			{
				return;
			}
			foreach (KeyValuePair<Character, CircuitBoxCursor> keyValuePair in this.ActiveCursors)
			{
				Character character2;
				CircuitBoxCursor circuitBoxCursor;
				keyValuePair.Deconstruct(out character2, out circuitBoxCursor);
				Character cursorChar = character2;
				CircuitBoxCursor cursor = circuitBoxCursor;
				if (cursor.IsActive)
				{
					this.ActiveCursors[cursorChar].Update(deltaTime);
				}
			}
			CS$<>8__locals1.cursorPos = this.UI.GetCursorPosition();
			int lastCursorPosIndex = this.recordedCursorPositions.Length - 1;
			if (this.cursorUpdateTimer < 1f)
			{
				this.cursorUpdateTimer += deltaTime;
				int cursorIndex = (int)MathF.Floor(this.cursorUpdateTimer * (float)lastCursorPosIndex);
				this.<UpdateHUDComponentSpecific>g__RecordCursorPosition|35_1(cursorIndex, ref CS$<>8__locals1);
				return;
			}
			this.<UpdateHUDComponentSpecific>g__RecordCursorPosition|35_1(lastCursorPosIndex, ref CS$<>8__locals1);
			this.SendCursorState(this.recordedCursorPositions, this.recordedDragStart, from c in this.recordedHeldPrefab
			select c.Identifier);
			Option.UnspecifiedNone none = Option.None;
			this.recordedDragStart = none;
			none = Option.None;
			this.recordedHeldPrefab = none;
			this.cursorUpdateTimer = 0f;
		}

		// Token: 0x06006018 RID: 24600 RVA: 0x0031FE34 File Offset: 0x0031E034
		public void RemoveComponents(IReadOnlyCollection<CircuitBoxComponent> node)
		{
			if (this.IsLocked())
			{
				return;
			}
			ImmutableArray<ushort> ids = (from n in node
			select n.ID).ToImmutableArray<ushort>();
			if (GameMain.NetworkMember == null)
			{
				this.CreateRefundItemsForUsedResources(ids, Character.Controlled);
				this.RemoveComponentInternal(ids);
				return;
			}
			if (!node.Any<CircuitBoxComponent>())
			{
				return;
			}
			this.CreateClientEvent(new CircuitBoxRemoveComponentEvent(ids));
		}

		// Token: 0x06006019 RID: 24601 RVA: 0x0031FEB4 File Offset: 0x0031E0B4
		public void AddWire(CircuitBoxConnection one, CircuitBoxConnection two)
		{
			if (this.IsLocked())
			{
				return;
			}
			if (GameMain.NetworkMember == null)
			{
				this.Connect(one, two, delegate
				{
				}, CircuitBoxWire.SelectedWirePrefab);
				return;
			}
			if (!CircuitBox.VerifyConnection(one, two))
			{
				return;
			}
			this.CreateClientEvent(new CircuitBoxClientAddWireEvent(Color.White, CircuitBoxConnectorIdentifier.FromConnection(one), CircuitBoxConnectorIdentifier.FromConnection(two), CircuitBoxWire.SelectedWirePrefab.UintIdentifier));
		}

		// Token: 0x0600601A RID: 24602 RVA: 0x0031FF34 File Offset: 0x0031E134
		public void RemoveWires(IReadOnlyCollection<CircuitBoxWire> wires)
		{
			if (this.IsLocked())
			{
				return;
			}
			ImmutableArray<ushort> ids = (from w in wires
			select w.ID).ToImmutableArray<ushort>();
			if (GameMain.NetworkMember == null)
			{
				this.RemoveWireInternal(ids);
				return;
			}
			if (!ids.Any<ushort>())
			{
				return;
			}
			this.CreateClientEvent(new CircuitBoxRemoveWireEvent(ids));
		}

		// Token: 0x0600601B RID: 24603 RVA: 0x0031FFA4 File Offset: 0x0031E1A4
		public void SelectComponents(IReadOnlyCollection<CircuitBoxNode> moveables, bool overwrite)
		{
			Character controlled = Character.Controlled;
			if (controlled == null)
			{
				return;
			}
			ushort controlledId = controlled.ID;
			ImmutableArray<ushort>.Builder ids = ImmutableArray.CreateBuilder<ushort>();
			ImmutableArray<CircuitBoxInputOutputNode.Type>.Builder ios = ImmutableArray.CreateBuilder<CircuitBoxInputOutputNode.Type>();
			ImmutableArray<ushort>.Builder labelIds = ImmutableArray.CreateBuilder<ushort>();
			foreach (CircuitBoxNode moveable in moveables)
			{
				if (moveable == null || !moveable.IsSelected || moveable.IsSelectedByMe)
				{
					CircuitBoxComponent node = moveable as CircuitBoxComponent;
					if (node == null)
					{
						CircuitBoxInputOutputNode io = moveable as CircuitBoxInputOutputNode;
						if (io == null)
						{
							CircuitBoxLabelNode label = moveable as CircuitBoxLabelNode;
							if (label != null)
							{
								labelIds.Add(label.ID);
							}
						}
						else
						{
							ios.Add(io.NodeType);
						}
					}
					else
					{
						ids.Add(node.ID);
					}
				}
			}
			if (GameMain.NetworkMember == null)
			{
				this.SelectComponentsInternal(ids, controlledId, overwrite);
				this.SelectInputOutputInternal(ios, controlledId, overwrite);
				this.SelectLabelsInternal(labelIds, controlledId, overwrite);
				return;
			}
			if (!ids.Any<ushort>() && !ios.Any<CircuitBoxInputOutputNode.Type>() && !labelIds.Any<ushort>() && !overwrite)
			{
				return;
			}
			this.CreateClientEvent(new CircuitBoxSelectNodesEvent(ids.ToImmutable(), ios.ToImmutable(), labelIds.ToImmutable(), overwrite, controlledId));
		}

		// Token: 0x0600601C RID: 24604 RVA: 0x003200E0 File Offset: 0x0031E2E0
		public void SelectWires(IReadOnlyCollection<CircuitBoxWire> wires, bool overwrite)
		{
			Character controlled = Character.Controlled;
			if (controlled == null)
			{
				return;
			}
			ushort controlledId = controlled.ID;
			ImmutableArray<ushort> ids = (from wire in wires
			where !wire.IsSelected || wire.IsSelectedByMe
			select wire.ID).ToImmutableArray<ushort>();
			if (GameMain.NetworkMember == null)
			{
				this.SelectWiresInternal(ids, controlledId, overwrite);
				return;
			}
			if (!ids.Any<ushort>() && !overwrite)
			{
				return;
			}
			this.CreateClientEvent(new CircuitBoxSelectWiresEvent(ids, overwrite, Character.Controlled.ID));
		}

		// Token: 0x0600601D RID: 24605 RVA: 0x00320190 File Offset: 0x0031E390
		public void MoveComponent(Vector2 moveAmount, IReadOnlyCollection<CircuitBoxNode> moveables)
		{
			if (this.IsLocked())
			{
				return;
			}
			ImmutableArray<ushort>.Builder ids = ImmutableArray.CreateBuilder<ushort>();
			ImmutableArray<CircuitBoxInputOutputNode.Type>.Builder ios = ImmutableArray.CreateBuilder<CircuitBoxInputOutputNode.Type>();
			ImmutableArray<ushort>.Builder labelIds = ImmutableArray.CreateBuilder<ushort>();
			foreach (CircuitBoxNode move in moveables)
			{
				CircuitBoxComponent node = move as CircuitBoxComponent;
				if (node == null)
				{
					CircuitBoxInputOutputNode io = move as CircuitBoxInputOutputNode;
					if (io == null)
					{
						CircuitBoxLabelNode label = move as CircuitBoxLabelNode;
						if (label != null)
						{
							labelIds.Add(label.ID);
						}
					}
					else
					{
						ios.Add(io.NodeType);
					}
				}
				else
				{
					ids.Add(node.ID);
				}
			}
			if (GameMain.NetworkMember == null)
			{
				this.MoveNodesInternal(ids, ios, labelIds, moveAmount);
				return;
			}
			if (!ids.Any<ushort>() && !ios.Any<CircuitBoxInputOutputNode.Type>() && !labelIds.Any<ushort>())
			{
				return;
			}
			this.CreateClientEvent(new CircuitBoxMoveComponentEvent(ids.ToImmutable(), ios.ToImmutable(), labelIds.ToImmutable(), moveAmount));
		}

		// Token: 0x0600601E RID: 24606 RVA: 0x00320290 File Offset: 0x0031E490
		public void AddComponent(ItemPrefab prefab, Vector2 pos)
		{
			if (this.IsLocked())
			{
				return;
			}
			if (GameMain.NetworkMember != null)
			{
				this.CreateClientEvent(new CircuitBoxAddComponentEvent(prefab.UintIdentifier, pos));
				return;
			}
			if (this.IsFull)
			{
				return;
			}
			ItemPrefab resource;
			if (CircuitBox.IsInGame())
			{
				Item r;
				if (!CircuitBox.GetApplicableResourcePlayerHas(prefab, Character.Controlled).TryUnwrap(out r))
				{
					return;
				}
				resource = r.Prefab;
				CircuitBox.RemoveItem(r);
			}
			else
			{
				resource = ItemPrefab.Prefabs[Tags.FPGACircuit];
			}
			this.AddComponentInternal(ICircuitBoxIdentifiable.FindFreeID<CircuitBoxComponent>(this.Components), prefab, resource, pos, Character.Controlled, null);
		}

		// Token: 0x0600601F RID: 24607 RVA: 0x00320327 File Offset: 0x0031E527
		public void RenameLabel(CircuitBoxLabelNode label, Color color, NetLimitedString header, NetLimitedString body)
		{
			if (this.IsLocked())
			{
				return;
			}
			if (GameMain.NetworkMember == null)
			{
				label.EditText(header, body);
				label.Color = color;
				return;
			}
			this.CreateClientEvent(new CircuitBoxRenameLabelEvent(label.ID, color, header, body));
		}

		// Token: 0x06006020 RID: 24608 RVA: 0x00320364 File Offset: 0x0031E564
		public void SetConnectionLabelOverrides(CircuitBoxInputOutputNode node, Dictionary<string, string> newOverrides)
		{
			if (GameMain.NetworkMember == null)
			{
				node.ReplaceAllConnectionLabelOverrides(newOverrides);
				return;
			}
			this.CreateClientEvent(new CircuitBoxRenameConnectionLabelsEvent(node.NodeType, newOverrides.ToNetDictionary<string, string>()));
		}

		// Token: 0x06006021 RID: 24609 RVA: 0x00320394 File Offset: 0x0031E594
		public void ResizeNode(CircuitBoxNode node, CircuitBoxResizeDirection dir, Vector2 amount)
		{
			if (this.IsLocked())
			{
				return;
			}
			ValueTuple<Vector2, Vector2> resize = node.ResizeBy(dir, amount);
			if (GameMain.NetworkMember == null)
			{
				node.ApplyResize(resize.Item1, resize.Item2);
				return;
			}
			ICircuitBoxIdentifiable identifiable = node as ICircuitBoxIdentifiable;
			if (identifiable == null)
			{
				DebugConsole.ThrowError("Tried to resize a node that doesn't have an ID.", null, null, false, false);
				return;
			}
			this.CreateClientEvent(new CircuitBoxResizeLabelEvent(identifiable.ID, resize.Item2, resize.Item1));
		}

		// Token: 0x06006022 RID: 24610 RVA: 0x00320408 File Offset: 0x0031E608
		public void AddLabel(Vector2 pos)
		{
			if (this.IsLocked())
			{
				return;
			}
			if (GameMain.NetworkMember == null)
			{
				this.AddLabelInternal(ICircuitBoxIdentifiable.FindFreeID<CircuitBoxLabelNode>(this.Labels), GUIStyle.Blue, pos, CircuitBoxLabelNode.DefaultHeaderText, NetLimitedString.Empty);
				return;
			}
			this.CreateClientEvent(new CircuitBoxAddLabelEvent(pos, GUIStyle.Blue, CircuitBoxLabelNode.DefaultHeaderText, NetLimitedString.Empty));
		}

		// Token: 0x06006023 RID: 24611 RVA: 0x00320474 File Offset: 0x0031E674
		public void RemoveLabel(IReadOnlyCollection<CircuitBoxLabelNode> labels)
		{
			if (this.IsLocked())
			{
				return;
			}
			if (!labels.Any<CircuitBoxLabelNode>())
			{
				return;
			}
			ImmutableArray<ushort> ids = (from n in labels
			select n.ID).ToImmutableArray<ushort>();
			if (GameMain.NetworkMember == null)
			{
				this.RemoveLabelInternal(ids);
				return;
			}
			this.CreateClientEvent(new CircuitBoxRemoveLabelEvent(ids));
		}

		// Token: 0x06006024 RID: 24612 RVA: 0x003204E3 File Offset: 0x0031E6E3
		protected override void OnResolutionChanged()
		{
			base.OnResolutionChanged();
			this.CreateGUI();
		}

		// Token: 0x06006025 RID: 24613 RVA: 0x003204F4 File Offset: 0x0031E6F4
		public void ClientRead(INetSerializableStruct data)
		{
			if (data is NetCircuitBoxCursorInfo)
			{
				NetCircuitBoxCursorInfo cursorInfo = (NetCircuitBoxCursorInfo)data;
				this.ClientReadCursor(cursorInfo);
				return;
			}
			if (data is CircuitBoxErrorEvent)
			{
				DebugConsole.ThrowError("The server responded with an error: " + ((CircuitBoxErrorEvent)data).Message, null, null, false, false);
				return;
			}
			throw new ArgumentOutOfRangeException("data", data, "This data cannot be handled using direct network messages.");
		}

		// Token: 0x06006026 RID: 24614 RVA: 0x00320558 File Offset: 0x0031E758
		public void SendMessage(CircuitBoxOpcode opcode, INetSerializableStruct data)
		{
			IWriteMessage msg = new WriteOnlyMessage().WithHeader(ClientPacketHeader.CIRCUITBOX);
			msg.WriteNetSerializableStruct(new NetCircuitBoxHeader(opcode, this.item.ID, (byte)this.item.GetComponentIndex(this)));
			msg.WriteNetSerializableStruct(data);
			DeliveryMethod deliveryMethod = CircuitBox.UnrealiableOpcodes.Contains(opcode) ? DeliveryMethod.Unreliable : DeliveryMethod.Reliable;
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return;
			}
			ClientPeer clientPeer = client.ClientPeer;
			if (clientPeer == null)
			{
				return;
			}
			clientPeer.Send(msg, deliveryMethod, true);
		}

		// Token: 0x06006027 RID: 24615 RVA: 0x003205CC File Offset: 0x0031E7CC
		[NullableContext(0)]
		private void SendCursorState([Nullable(1)] Vector2[] cursorPositions, Option<Vector2> dragStart, Option<Identifier> heldComponent)
		{
			if (!CircuitBox.IsRoundRunning())
			{
				return;
			}
			NetCircuitBoxCursorInfo msg = new NetCircuitBoxCursorInfo(cursorPositions, dragStart, heldComponent, 0);
			this.SendMessage(CircuitBoxOpcode.Cursor, msg);
		}

		// Token: 0x06006028 RID: 24616 RVA: 0x003205FC File Offset: 0x0031E7FC
		public void ClientReadCursor(NetCircuitBoxCursorInfo info)
		{
			Character character = Entity.FindEntityByID(info.CharacterID) as Character;
			if (character == null)
			{
				return;
			}
			if (!this.ActiveCursors.ContainsKey(character))
			{
				CircuitBoxCursor newCursor = new CircuitBoxCursor(info);
				this.ActiveCursors.Add(character, newCursor);
				return;
			}
			CircuitBoxCursor activeCursor = this.ActiveCursors[character];
			activeCursor.UpdateInfo(info);
			activeCursor.ResetTimers();
		}

		// Token: 0x06006029 RID: 24617 RVA: 0x0032065C File Offset: 0x0031E85C
		public void CreateClientEvent(INetSerializableStruct data)
		{
			this.item.CreateClientEvent<CircuitBox>(this, new CircuitBoxEventData(data));
		}

		// Token: 0x0600602A RID: 24618 RVA: 0x00320678 File Offset: 0x0031E878
		public void ClientEventWrite(IWriteMessage msg, [Nullable(2)] NetEntityEvent.IData extraData = null)
		{
			if (extraData == null)
			{
				return;
			}
			CircuitBoxEventData eventData = base.ExtractEventData<CircuitBoxEventData>(extraData);
			msg.WriteByte((byte)eventData.Opcode);
			msg.WriteNetSerializableStruct(eventData.Data);
		}

		// Token: 0x0600602B RID: 24619 RVA: 0x003206AC File Offset: 0x0031E8AC
		public void ClientEventRead(IReadMessage msg, float sendingTime)
		{
			CircuitBoxOpcode header = (CircuitBoxOpcode)msg.ReadByte();
			switch (header)
			{
			case CircuitBoxOpcode.AddComponent:
			{
				CircuitBoxServerCreateComponentEvent data = INetSerializableStruct.Read<CircuitBoxServerCreateComponentEvent>(msg);
				this.AddComponentFromData(data);
				return;
			}
			case CircuitBoxOpcode.MoveComponent:
			{
				CircuitBoxMoveComponentEvent data2 = INetSerializableStruct.Read<CircuitBoxMoveComponentEvent>(msg);
				this.MoveNodesInternal(data2.TargetIDs, data2.IOs, data2.LabelIDs, data2.MoveAmount);
				return;
			}
			case CircuitBoxOpcode.AddWire:
			{
				CircuitBoxServerCreateWireEvent data3 = INetSerializableStruct.Read<CircuitBoxServerCreateWireEvent>(msg);
				this.AddWireFromData(data3);
				return;
			}
			case CircuitBoxOpcode.RemoveWire:
				this.RemoveWireInternal(INetSerializableStruct.Read<CircuitBoxRemoveWireEvent>(msg).TargetIDs);
				return;
			case CircuitBoxOpcode.UpdateSelection:
			{
				CircuitBoxServerUpdateSelection data4 = INetSerializableStruct.Read<CircuitBoxServerUpdateSelection>(msg);
				ImmutableDictionary<ushort, Option<ushort>> nodeDict = data4.ComponentIds.ToImmutableDictionary((CircuitBoxIdSelectionPair s) => s.ID, (CircuitBoxIdSelectionPair s) => s.SelectedBy);
				ImmutableDictionary<ushort, Option<ushort>> wireDict = data4.WireIds.ToImmutableDictionary((CircuitBoxIdSelectionPair s) => s.ID, (CircuitBoxIdSelectionPair s) => s.SelectedBy);
				ImmutableDictionary<CircuitBoxInputOutputNode.Type, Option<ushort>> ioDict = data4.InputOutputs.ToImmutableDictionary((CircuitBoxTypeSelectionPair s) => s.Type, (CircuitBoxTypeSelectionPair s) => s.SelectedBy);
				ImmutableDictionary<ushort, Option<ushort>> labelDict = data4.LabelIds.ToImmutableDictionary((CircuitBoxIdSelectionPair s) => s.ID, (CircuitBoxIdSelectionPair s) => s.SelectedBy);
				this.UpdateSelections(nodeDict, wireDict, ioDict, labelDict);
				return;
			}
			case CircuitBoxOpcode.DeleteComponent:
				this.RemoveComponentInternal(INetSerializableStruct.Read<CircuitBoxRemoveComponentEvent>(msg).TargetIDs);
				return;
			case CircuitBoxOpcode.RenameLabel:
			{
				CircuitBoxRenameLabelEvent data5 = INetSerializableStruct.Read<CircuitBoxRenameLabelEvent>(msg);
				this.RenameLabelInternal(data5.LabelId, data5.Color, data5.NewHeader, data5.NewBody);
				return;
			}
			case CircuitBoxOpcode.AddLabel:
			{
				CircuitBoxServerAddLabelEvent data6 = INetSerializableStruct.Read<CircuitBoxServerAddLabelEvent>(msg);
				this.AddLabelInternal(data6.ID, data6.Color, data6.Position, data6.Header, data6.Body);
				this.ResizeLabelInternal(data6.ID, data6.Position, data6.Size);
				return;
			}
			case CircuitBoxOpcode.RemoveLabel:
				this.RemoveLabelInternal(INetSerializableStruct.Read<CircuitBoxRemoveLabelEvent>(msg).TargetIDs);
				return;
			case CircuitBoxOpcode.ResizeLabel:
			{
				CircuitBoxResizeLabelEvent data7 = INetSerializableStruct.Read<CircuitBoxResizeLabelEvent>(msg);
				this.ResizeLabelInternal(data7.ID, data7.Position, data7.Size);
				return;
			}
			case CircuitBoxOpcode.RenameConnections:
			{
				CircuitBoxRenameConnectionLabelsEvent data8 = INetSerializableStruct.Read<CircuitBoxRenameConnectionLabelsEvent>(msg);
				this.RenameConnectionLabelsInternal(data8.Type, data8.Override.ToDictionary());
				return;
			}
			case CircuitBoxOpcode.ServerInitialize:
			{
				this.Components.Clear();
				this.Wires.Clear();
				this.Labels.Clear();
				CircuitBoxInitializeStateFromServerEvent data9 = INetSerializableStruct.Read<CircuitBoxInitializeStateFromServerEvent>(msg);
				foreach (CircuitBoxServerCreateComponentEvent compData in data9.Components)
				{
					this.AddComponentFromData(compData);
				}
				foreach (CircuitBoxServerCreateWireEvent wireData in data9.Wires)
				{
					this.AddWireFromData(wireData);
				}
				foreach (CircuitBoxServerAddLabelEvent labelData in data9.Labels)
				{
					this.AddLabelInternal(labelData.ID, labelData.Color, labelData.Position, labelData.Header, labelData.Body);
					this.ResizeLabelInternal(labelData.ID, labelData.Position, labelData.Size);
				}
				foreach (CircuitBoxInputOutputNode node in this.InputOutputNodes)
				{
					CircuitBoxInputOutputNode circuitBoxInputOutputNode = node;
					CircuitBoxInputOutputNode.Type nodeType = node.NodeType;
					Vector2 position;
					if (nodeType != CircuitBoxInputOutputNode.Type.Input)
					{
						if (nodeType != CircuitBoxInputOutputNode.Type.Output)
						{
							position = node.Position;
						}
						else
						{
							position = data9.OutputPos;
						}
					}
					else
					{
						position = data9.InputPos;
					}
					circuitBoxInputOutputNode.Position = position;
				}
				foreach (CircuitBoxRenameConnectionLabelsEvent labelOverride in data9.LabelOverrides)
				{
					this.RenameConnectionLabelsInternal(labelOverride.Type, labelOverride.Override.ToDictionary());
				}
				this.wasInitializedByServer = true;
				return;
			}
			}
			throw new ArgumentOutOfRangeException("header", header, "This opcode cannot be handled using entity events");
		}

		// Token: 0x0600602C RID: 24620 RVA: 0x00320B9C File Offset: 0x0031ED9C
		public void AddComponentFromData(CircuitBoxServerCreateComponentEvent data)
		{
			ItemPrefab prefab = ItemPrefab.Prefabs.Find((ItemPrefab p) => p.UintIdentifier == data.UsedResource);
			if (prefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(27, 1);
				defaultInterpolatedStringHandler.AppendLiteral("No item prefab found for \"");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(data.UsedResource);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.AddComponentInternalUnsafe(data.ComponentId, CircuitBox.FindItemByID(data.BackingItemId), prefab, data.Position);
		}

		// Token: 0x0600602D RID: 24621 RVA: 0x00320C40 File Offset: 0x0031EE40
		public void AddWireFromData(CircuitBoxServerCreateWireEvent data)
		{
			CircuitBoxServerCreateWireEvent circuitBoxServerCreateWireEvent = data;
			CircuitBoxClientAddWireEvent req2;
			ushort num;
			Option<ushort> option;
			circuitBoxServerCreateWireEvent.Deconstruct(out req2, out num, out option);
			CircuitBoxClientAddWireEvent req = req2;
			ushort wireId = num;
			Option<ushort> possibleItemId = option;
			ItemPrefab prefab = ItemPrefab.Prefabs.Find((ItemPrefab p) => p.UintIdentifier == req.SelectedWirePrefabIdentifier);
			if (prefab == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 1);
				defaultInterpolatedStringHandler.AppendLiteral("No prefab found for \"");
				defaultInterpolatedStringHandler.AppendFormatted<uint>(req.SelectedWirePrefabIdentifier);
				defaultInterpolatedStringHandler.AppendLiteral("\"");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			CircuitBoxConnection start;
			if (!req.Start.FindConnection(this).TryUnwrap(out start))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(26, 1);
				defaultInterpolatedStringHandler2.AppendLiteral("No connection found for (");
				defaultInterpolatedStringHandler2.AppendFormatted<CircuitBoxConnectorIdentifier>(req.Start);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			CircuitBoxConnection end;
			if (!req.End.FindConnection(this).TryUnwrap(out end))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(26, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("No connection found for (");
				defaultInterpolatedStringHandler3.AppendFormatted<CircuitBoxConnectorIdentifier>(req.Start);
				defaultInterpolatedStringHandler3.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler3.ToStringAndClear());
			}
			ushort backingItem;
			if (possibleItemId.TryUnwrap(out backingItem))
			{
				this.CreateWireWithItem(start, end, wireId, CircuitBox.FindItemByID(backingItem));
				return;
			}
			this.CreateWireWithoutItem(start, end, wireId, prefab);
		}

		// Token: 0x0600602E RID: 24622 RVA: 0x00320DBC File Offset: 0x0031EFBC
		public static Item FindItemByID(ushort id)
		{
			Item item = Entity.FindEntityByID(id) as Item;
			if (item == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 1);
				defaultInterpolatedStringHandler.AppendLiteral("No item with ID ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(id);
				defaultInterpolatedStringHandler.AppendLiteral(" exists.");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return item;
		}

		// Token: 0x0600602F RID: 24623 RVA: 0x00320E0F File Offset: 0x0031F00F
		public override void AddToGUIUpdateList(int order = 0)
		{
			base.AddToGUIUpdateList(order);
			CircuitBoxUI ui = this.UI;
			if (ui == null)
			{
				return;
			}
			ui.AddToGUIUpdateList();
		}

		// Token: 0x17001851 RID: 6225
		// (get) Token: 0x06006030 RID: 24624 RVA: 0x00320E28 File Offset: 0x0031F028
		public override bool IsActive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17001852 RID: 6226
		// (get) Token: 0x06006031 RID: 24625 RVA: 0x00320E2B File Offset: 0x0031F02B
		public override bool DontTransferInventoryBetweenSubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x17001853 RID: 6227
		// (get) Token: 0x06006032 RID: 24626 RVA: 0x00320E2E File Offset: 0x0031F02E
		public override bool DisallowSellingItemsFromContainer
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06006033 RID: 24627 RVA: 0x00320E34 File Offset: 0x0031F034
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBoxConnection> FindInputOutputConnection(Identifier connectionName)
		{
			foreach (CircuitBoxInputConnection input in this.Inputs)
			{
				if (!(input.Name != connectionName))
				{
					return Option.Some<CircuitBoxConnection>(input);
				}
			}
			foreach (CircuitBoxOutputConnection output in this.Outputs)
			{
				if (!(output.Name != connectionName))
				{
					return Option.Some<CircuitBoxConnection>(output);
				}
			}
			Option.UnspecifiedNone none = Option.None;
			return none;
		}

		// Token: 0x06006034 RID: 24628 RVA: 0x00320EBC File Offset: 0x0031F0BC
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<CircuitBoxConnection> FindInputOutputConnection(Connection connection)
		{
			foreach (CircuitBoxInputConnection input in this.Inputs)
			{
				if (input.Connection == connection)
				{
					return Option.Some<CircuitBoxConnection>(input);
				}
			}
			foreach (CircuitBoxOutputConnection output in this.Outputs)
			{
				if (output.Connection == connection)
				{
					return Option.Some<CircuitBoxConnection>(output);
				}
			}
			Option.UnspecifiedNone none = Option.None;
			return none;
		}

		// Token: 0x17001854 RID: 6228
		// (get) Token: 0x06006035 RID: 24629 RVA: 0x00320F35 File Offset: 0x0031F135
		[Nullable(2)]
		public ItemContainer ComponentContainer
		{
			[NullableContext(2)]
			get
			{
				return this.GetContainerOrNull(0);
			}
		}

		// Token: 0x17001855 RID: 6229
		// (get) Token: 0x06006036 RID: 24630 RVA: 0x00320F3E File Offset: 0x0031F13E
		[Nullable(2)]
		public ItemContainer WireContainer
		{
			[NullableContext(2)]
			get
			{
				return this.GetContainerOrNull(1) ?? this.GetContainerOrNull(0);
			}
		}

		// Token: 0x17001856 RID: 6230
		// (get) Token: 0x06006037 RID: 24631 RVA: 0x00320F54 File Offset: 0x0031F154
		public bool IsFull
		{
			get
			{
				ItemContainer componentContainer = this.ComponentContainer;
				ItemInventory inventory = (componentContainer != null) ? componentContainer.Inventory : null;
				return inventory != null && inventory.IsFull(true);
			}
		}

		// Token: 0x17001857 RID: 6231
		// (get) Token: 0x06006038 RID: 24632 RVA: 0x00320F80 File Offset: 0x0031F180
		// (set) Token: 0x06006039 RID: 24633 RVA: 0x00320F88 File Offset: 0x0031F188
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Locked circuit boxes can only be viewed and not interacted with.", "", false)]
		public bool Locked { get; private set; }

		// Token: 0x0600603A RID: 24634 RVA: 0x00320F94 File Offset: 0x0031F194
		public CircuitBox(Item item, ContentXElement element)
		{
			Option.UnspecifiedNone none = Option.None;
			this.HeldComponent = none;
			this.recordedCursorPositions = new Vector2[10];
			none = Option.None;
			this.recordedDragStart = none;
			none = Option.None;
			this.recordedHeldPrefab = none;
			this.Components = new List<CircuitBoxComponent>();
			this.InputOutputNodes = new List<CircuitBoxInputOutputNode>();
			this.Labels = new List<CircuitBoxLabelNode>();
			this.Wires = new List<CircuitBoxWire>();
			base..ctor(item, element);
			this.containers = item.GetComponents<ItemContainer>().ToArray<ItemContainer>();
			if (this.containers.Length < 1)
			{
				DebugConsole.ThrowError("Circuit box must have at least one item container to function.", null, null, false, false);
			}
			this.InitProjSpecific(element);
			ImmutableArray<CircuitBoxInputConnection>.Builder inputBuilder = ImmutableArray.CreateBuilder<CircuitBoxInputConnection>();
			ImmutableArray<CircuitBoxOutputConnection>.Builder outputBuilder = ImmutableArray.CreateBuilder<CircuitBoxOutputConnection>();
			foreach (Connection conn in from c in base.Item.Connections
			orderby c.DisplayOrder
			select c)
			{
				if (conn.IsOutput)
				{
					outputBuilder.Add(new CircuitBoxOutputConnection(Vector2.Zero, conn, this));
				}
				else
				{
					inputBuilder.Add(new CircuitBoxInputConnection(Vector2.Zero, conn, this));
				}
			}
			this.Inputs = inputBuilder.ToImmutable();
			this.Outputs = outputBuilder.ToImmutable();
			this.InputOutputNodes.Add(new CircuitBoxInputOutputNode(this.Inputs, new Vector2(-512f, 0f), CircuitBoxInputOutputNode.Type.Input, this));
			this.InputOutputNodes.Add(new CircuitBoxInputOutputNode(this.Outputs, new Vector2(512f, 0f), CircuitBoxInputOutputNode.Type.Output, this));
			item.OnDeselect = (Action<Character>)Delegate.Combine(item.OnDeselect, new Action<Character>(this.OnDeselected));
		}

		// Token: 0x0600603B RID: 24635 RVA: 0x0032118C File Offset: 0x0031F38C
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			if (this.delayedElementToLoad.IsSome())
			{
				return;
			}
			this.delayedElementToLoad = Option.Some<ContentXElement>(componentElement);
		}

		// Token: 0x0600603C RID: 24636 RVA: 0x003211B3 File Offset: 0x0031F3B3
		public override void OnInventoryChanged()
		{
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x0600603D RID: 24637 RVA: 0x003211BC File Offset: 0x0031F3BC
		public override void Update(float deltaTime, Camera cam)
		{
			if (this.wasInitializedByServer)
			{
				foreach (CircuitBoxWire w in this.Wires)
				{
					w.EnsureWireConnected();
				}
				this.wasInitializedByServer = false;
			}
			this.TryInitializeNodes();
		}

		// Token: 0x0600603E RID: 24638 RVA: 0x00321224 File Offset: 0x0031F424
		public override void OnMapLoaded()
		{
			this.TryInitializeNodes();
		}

		// Token: 0x0600603F RID: 24639 RVA: 0x0032122C File Offset: 0x0031F42C
		private void TryInitializeNodes()
		{
			ContentXElement loadElement;
			if (!this.delayedElementToLoad.TryUnwrap(out loadElement))
			{
				return;
			}
			this.LoadFromXML(loadElement);
			Option.UnspecifiedNone none = Option.None;
			this.delayedElementToLoad = none;
		}

		// Token: 0x06006040 RID: 24640 RVA: 0x00321264 File Offset: 0x0031F464
		public void LoadFromXML(ContentXElement loadElement)
		{
			foreach (ContentXElement subElement in loadElement.Elements())
			{
				string elementName = subElement.Name.ToString().ToLowerInvariant();
				string a = elementName;
				CircuitBoxComponent comp;
				if (!(a == "component"))
				{
					CircuitBoxWire wire;
					if (!(a == "wire"))
					{
						if (!(a == "inputnode"))
						{
							if (!(a == "outputnode"))
							{
								if (a == "label")
								{
									this.Labels.Add(CircuitBoxLabelNode.LoadFromXML(subElement, this));
								}
							}
							else
							{
								this.<LoadFromXML>g__LoadFor|96_0(CircuitBoxInputOutputNode.Type.Output, subElement);
							}
						}
						else
						{
							this.<LoadFromXML>g__LoadFor|96_0(CircuitBoxInputOutputNode.Type.Input, subElement);
						}
					}
					else if (CircuitBoxWire.TryLoadFromXML(subElement, this).TryUnwrap(out wire))
					{
						this.Wires.Add(wire);
					}
				}
				else if (CircuitBoxComponent.TryLoadFromXML(subElement, this).TryUnwrap(out comp))
				{
					this.Components.Add(comp);
				}
			}
		}

		// Token: 0x06006041 RID: 24641 RVA: 0x00321378 File Offset: 0x0031F578
		public void CloneFrom(CircuitBox original, Dictionary<ushort, Item> clonedContainedItems)
		{
			this.Components.Clear();
			this.Wires.Clear();
			this.Labels.Clear();
			foreach (CircuitBoxLabelNode label in original.Labels)
			{
				CircuitBoxLabelNode newLabel = new CircuitBoxLabelNode(label.ID, label.Color, label.Position, this);
				newLabel.EditText(label.HeaderText, label.BodyText);
				newLabel.ApplyResize(label.Size, label.Position);
				this.Labels.Add(newLabel);
			}
			for (int ioIndex = 0; ioIndex < original.InputOutputNodes.Count; ioIndex++)
			{
				CircuitBoxInputOutputNode origNode = original.InputOutputNodes[ioIndex];
				CircuitBoxInputOutputNode cloneNode = this.InputOutputNodes[ioIndex];
				cloneNode.Position = origNode.Position;
				cloneNode.ReplaceAllConnectionLabelOverrides(origNode.ConnectionLabelOverrides);
			}
			foreach (CircuitBoxComponent origComp in original.Components)
			{
				Item clonedItem;
				if (clonedContainedItems.TryGetValue(origComp.Item.ID, out clonedItem))
				{
					CircuitBoxComponent newComponent = new CircuitBoxComponent(origComp.ID, clonedItem, origComp.Position, this, origComp.UsedResource);
					this.Components.Add(newComponent);
				}
			}
			Func<Item, Item> <>9__0;
			foreach (CircuitBoxWire origWire in original.Wires)
			{
				Option<CircuitBoxConnection> to = CircuitBoxConnectorIdentifier.FromConnection(origWire.To).FindConnection(this);
				Option<CircuitBoxConnection> from = CircuitBoxConnectorIdentifier.FromConnection(origWire.From).FindConnection(this);
				CircuitBoxConnection toConn;
				CircuitBoxConnection fromConn;
				if (!to.TryUnwrap(out toConn) || !from.TryUnwrap(out fromConn))
				{
					DebugConsole.ThrowError("Error while cloning item \"" + base.Name + "\" - failed to find a connection for a wire. ", null, null, false, false);
				}
				else
				{
					CircuitBoxWire circuitBoxWire = origWire;
					Func<Item, Item> selector;
					if ((selector = <>9__0) == null)
					{
						selector = (<>9__0 = ((Item w) => clonedContainedItems[w.ID]));
					}
					Option<Item> wireItem = circuitBoxWire.BackingWire.Select<Item>(selector);
					CircuitBoxWire newWire = new CircuitBoxWire(this, origWire.ID, wireItem, fromConn, toConn, origWire.UsedItemPrefab);
					this.Wires.Add(newWire);
				}
			}
		}

		// Token: 0x06006042 RID: 24642 RVA: 0x00321614 File Offset: 0x0031F814
		public override XElement Save(XElement parentElement)
		{
			XElement componentElement = base.Save(parentElement);
			foreach (CircuitBoxInputOutputNode node in this.InputOutputNodes)
			{
				componentElement.Add(node.Save());
			}
			foreach (CircuitBoxComponent node2 in this.Components)
			{
				componentElement.Add(node2.Save());
			}
			foreach (CircuitBoxWire wire in this.Wires)
			{
				componentElement.Add(wire.Save());
			}
			foreach (CircuitBoxLabelNode label in this.Labels)
			{
				componentElement.Add(label.Save());
			}
			return componentElement;
		}

		// Token: 0x06006043 RID: 24643 RVA: 0x00321754 File Offset: 0x0031F954
		public void OnDeselected(Character c)
		{
			this.cursorUpdateTimer = 0f;
			if (GameMain.NetworkMember != null)
			{
				return;
			}
			this.ClearAllSelectionsInternal(c.ID);
		}

		// Token: 0x06006044 RID: 24644 RVA: 0x00321778 File Offset: 0x0031F978
		public bool Connect(CircuitBoxConnection one, CircuitBoxConnection two, Action<CircuitBox.CreatedWire> onCreated, ItemPrefab selectedWirePrefab)
		{
			if (!CircuitBox.VerifyConnection(one, two))
			{
				return false;
			}
			ushort id = ICircuitBoxIdentifiable.FindFreeID<CircuitBoxWire>(this.Wires);
			bool isOutput = one.IsOutput;
			if (isOutput)
			{
				if (!two.IsOutput)
				{
					CircuitBoxConnectorIdentifier start = CircuitBoxConnectorIdentifier.FromConnection(one);
					CircuitBoxConnectorIdentifier end = CircuitBoxConnectorIdentifier.FromConnection(two);
					if (CircuitBox.IsExternalConnection(one) || CircuitBox.IsExternalConnection(two))
					{
						this.CreateWireWithoutItem(one, two, id, selectedWirePrefab);
						Action<CircuitBox.CreatedWire> onCreated2 = onCreated;
						CircuitBoxConnectorIdentifier start3 = start;
						CircuitBoxConnectorIdentifier end3 = end;
						Option.UnspecifiedNone none = Option.None;
						onCreated2(new CircuitBox.CreatedWire(start3, end3, none, id));
						return true;
					}
					this.CreateWireWithItem(one, two, selectedWirePrefab, id, delegate(Item i)
					{
						onCreated(new CircuitBox.CreatedWire(start, end, Option.Some<Item>(i), id));
					});
					return true;
				}
			}
			else if (two.IsOutput)
			{
				CircuitBoxConnectorIdentifier start = CircuitBoxConnectorIdentifier.FromConnection(two);
				CircuitBoxConnectorIdentifier end = CircuitBoxConnectorIdentifier.FromConnection(one);
				if (CircuitBox.IsExternalConnection(one) || CircuitBox.IsExternalConnection(two))
				{
					this.CreateWireWithoutItem(two, one, id, selectedWirePrefab);
					Action<CircuitBox.CreatedWire> onCreated3 = onCreated;
					CircuitBoxConnectorIdentifier start2 = start;
					CircuitBoxConnectorIdentifier end2 = end;
					Option.UnspecifiedNone none = Option.None;
					onCreated3(new CircuitBox.CreatedWire(start2, end2, none, id));
					return true;
				}
				this.CreateWireWithItem(two, one, selectedWirePrefab, id, delegate(Item i)
				{
					onCreated(new CircuitBox.CreatedWire(start, end, Option.Some<Item>(i), id));
				});
				return true;
			}
			return false;
		}

		// Token: 0x06006045 RID: 24645 RVA: 0x00321930 File Offset: 0x0031FB30
		private static bool VerifyConnection(CircuitBoxConnection one, CircuitBoxConnection two)
		{
			if (one.IsOutput == two.IsOutput || one == two)
			{
				return false;
			}
			CircuitBoxNodeConnection oneNodeConnection = one as CircuitBoxNodeConnection;
			if (oneNodeConnection != null)
			{
				CircuitBoxNodeConnection twoNodeConnection = two as CircuitBoxNodeConnection;
				if (twoNodeConnection != null && oneNodeConnection.Component == twoNodeConnection.Component)
				{
					return false;
				}
			}
			CircuitBoxNodeConnection circuitBoxNodeConnection = one as CircuitBoxNodeConnection;
			if (circuitBoxNodeConnection == null || circuitBoxNodeConnection.HasAvailableSlots)
			{
				circuitBoxNodeConnection = (two as CircuitBoxNodeConnection);
				if (circuitBoxNodeConnection == null || circuitBoxNodeConnection.HasAvailableSlots)
				{
					return true;
				}
			}
			return !(one is CircuitBoxNodeConnection) || !(two is CircuitBoxNodeConnection);
		}

		// Token: 0x06006046 RID: 24646 RVA: 0x003219B4 File Offset: 0x0031FBB4
		private void AddLabelInternal(ushort id, Color color, Vector2 pos, NetLimitedString header, NetLimitedString body)
		{
			CircuitBoxLabelNode newLabel = new CircuitBoxLabelNode(id, color, pos, this);
			newLabel.EditText(header, body);
			this.Labels.Add(newLabel);
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x06006047 RID: 24647 RVA: 0x003219E8 File Offset: 0x0031FBE8
		private void RemoveLabelInternal(IReadOnlyCollection<ushort> ids)
		{
			foreach (CircuitBoxLabelNode node in this.Labels.ToImmutableArray<CircuitBoxLabelNode>())
			{
				if (ids.Contains(node.ID))
				{
					this.Labels.Remove(node);
				}
			}
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x06006048 RID: 24648 RVA: 0x00321A40 File Offset: 0x0031FC40
		private void ResizeLabelInternal(ushort id, Vector2 pos, Vector2 size)
		{
			size = Vector2.Max(size, CircuitBoxLabelNode.MinSize);
			foreach (CircuitBoxLabelNode node in this.Labels)
			{
				if (node.ID == id)
				{
					node.ApplyResize(size, pos);
					break;
				}
			}
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x06006049 RID: 24649 RVA: 0x00321AB4 File Offset: 0x0031FCB4
		private void RenameConnectionLabelsInternal(CircuitBoxInputOutputNode.Type type, Dictionary<string, string> overrides)
		{
			foreach (CircuitBoxInputOutputNode node in this.InputOutputNodes)
			{
				if (node.NodeType == type)
				{
					node.ReplaceAllConnectionLabelOverrides(overrides);
					break;
				}
			}
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x0600604A RID: 24650 RVA: 0x00321B18 File Offset: 0x0031FD18
		private static bool IsExternalConnection(CircuitBoxConnection conn)
		{
			return conn is CircuitBoxInputConnection || conn is CircuitBoxOutputConnection;
		}

		// Token: 0x0600604B RID: 24651 RVA: 0x00321B3C File Offset: 0x0031FD3C
		private void CreateWireWithoutItem(CircuitBoxConnection one, CircuitBoxConnection two, ushort id, ItemPrefab prefab)
		{
			bool hasExternalConnection = false;
			CircuitBoxInputConnection input = one as CircuitBoxInputConnection;
			if (input != null)
			{
				hasExternalConnection = true;
				input.ExternallyConnectedTo.Add(two);
			}
			CircuitBoxOutputConnection output = two as CircuitBoxOutputConnection;
			if (output != null)
			{
				hasExternalConnection = true;
				one.Connection.CircuitBoxConnections.Add(output);
			}
			if (hasExternalConnection)
			{
				two.ExternallyConnectedFrom.Add(one);
			}
			Option.UnspecifiedNone none = Option.None;
			this.AddWireDirect(id, prefab, none, one, two);
		}

		// Token: 0x0600604C RID: 24652 RVA: 0x00321BA8 File Offset: 0x0031FDA8
		private void CreateWireWithItem(CircuitBoxConnection one, CircuitBoxConnection two, ItemPrefab prefab, ushort wireId, Action<Item> onItemSpawned)
		{
			if (this.WireContainer == null)
			{
				return;
			}
			if (CircuitBox.IsExternalConnection(one) || CircuitBox.IsExternalConnection(two))
			{
				DebugConsole.ThrowError("Cannot add a wire between an external connection and a component connection.", null, null, false, false);
				return;
			}
			CircuitBox.SpawnItem(prefab, null, this.WireContainer, delegate(Item wire)
			{
				this.AddWireDirect(wireId, prefab, Option.Some<Item>(wire), one, two);
				onItemSpawned(wire);
			});
		}

		// Token: 0x0600604D RID: 24653 RVA: 0x00321C37 File Offset: 0x0031FE37
		private void CreateWireWithItem(CircuitBoxConnection one, CircuitBoxConnection two, ushort wireId, Item it)
		{
			if (CircuitBox.IsExternalConnection(one) || CircuitBox.IsExternalConnection(two))
			{
				DebugConsole.ThrowError("Cannot add a wire between an external connection and a component connection.", null, null, false, false);
				return;
			}
			this.AddWireDirect(wireId, it.Prefab, Option.Some<Item>(it), one, two);
		}

		// Token: 0x0600604E RID: 24654 RVA: 0x00321C6F File Offset: 0x0031FE6F
		private void AddWireDirect(ushort id, ItemPrefab prefab, [Nullable(new byte[]
		{
			0,
			1
		})] Option<Item> backingItem, CircuitBoxConnection one, CircuitBoxConnection two)
		{
			this.Wires.Add(new CircuitBoxWire(this, id, backingItem, one, two, prefab));
		}

		// Token: 0x0600604F RID: 24655 RVA: 0x00321C8C File Offset: 0x0031FE8C
		private void RenameLabelInternal(ushort id, Color color, NetLimitedString header, NetLimitedString body)
		{
			foreach (CircuitBoxLabelNode node in this.Labels)
			{
				if (node.ID == id)
				{
					node.EditText(header, body);
					node.Color = color;
					break;
				}
			}
		}

		// Token: 0x06006050 RID: 24656 RVA: 0x00321CF4 File Offset: 0x0031FEF4
		private bool AddComponentInternal(ushort id, ItemPrefab prefab, ItemPrefab usedResource, Vector2 pos, [Nullable(2)] Character user, [Nullable(new byte[]
		{
			2,
			1
		})] Action<Item> onItemSpawned)
		{
			if (id == 65535)
			{
				DebugConsole.ThrowError("Unable to add component because there are no free IDs.", null, null, false, false);
				return false;
			}
			ItemContainer componentContainer = this.ComponentContainer;
			ItemInventory inventory = (componentContainer != null) ? componentContainer.Inventory : null;
			if (inventory != null && inventory.HowManyCanBePut(prefab, null) <= 0)
			{
				DebugConsole.ThrowError("Unable to add component because there is no space in the inventory.", null, null, false, false);
				return false;
			}
			CircuitBox.SpawnItem(prefab, user, this.ComponentContainer, delegate(Item spawnedItem)
			{
				this.Components.Add(new CircuitBoxComponent(id, spawnedItem, pos, this, usedResource));
				Action<Item> onItemSpawned2 = onItemSpawned;
				if (onItemSpawned2 != null)
				{
					onItemSpawned2(spawnedItem);
				}
				this.OnViewUpdateProjSpecific();
			});
			this.OnViewUpdateProjSpecific();
			return true;
		}

		// Token: 0x06006051 RID: 24657 RVA: 0x00321DA3 File Offset: 0x0031FFA3
		private void AddComponentInternalUnsafe(ushort id, Item backingItem, ItemPrefab usedResource, Vector2 pos)
		{
			this.Components.Add(new CircuitBoxComponent(id, backingItem, pos, this, usedResource));
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x06006052 RID: 24658 RVA: 0x00321DC4 File Offset: 0x0031FFC4
		private static void ClearSelectionFor(ushort characterId, IReadOnlyCollection<CircuitBoxSelectable> nodes)
		{
			foreach (CircuitBoxSelectable node in nodes)
			{
				if (node.SelectedBy == characterId)
				{
					CircuitBoxSelectable circuitBoxSelectable = node;
					Option.UnspecifiedNone none = Option.None;
					circuitBoxSelectable.SetSelected(none);
				}
			}
		}

		// Token: 0x06006053 RID: 24659 RVA: 0x00321E24 File Offset: 0x00320024
		private void ClearAllSelectionsInternal(ushort characterId)
		{
			CircuitBox.ClearSelectionFor(characterId, this.Components);
			CircuitBox.ClearSelectionFor(characterId, this.InputOutputNodes);
			CircuitBox.ClearSelectionFor(characterId, this.Wires);
			CircuitBox.ClearSelectionFor(characterId, this.Labels);
		}

		// Token: 0x06006054 RID: 24660 RVA: 0x00321E58 File Offset: 0x00320058
		private void SelectLabelsInternal(IReadOnlyCollection<ushort> ids, ushort characterId, bool overwrite)
		{
			if (overwrite)
			{
				CircuitBox.ClearSelectionFor(characterId, this.Labels);
			}
			if (!ids.Any<ushort>())
			{
				return;
			}
			foreach (CircuitBoxLabelNode node in this.Labels)
			{
				if (ids.Contains(node.ID))
				{
					node.SetSelected(Option.Some<ushort>(characterId));
				}
			}
		}

		// Token: 0x06006055 RID: 24661 RVA: 0x00321ED8 File Offset: 0x003200D8
		private void SelectComponentsInternal(IReadOnlyCollection<ushort> ids, ushort characterId, bool overwrite)
		{
			if (overwrite)
			{
				CircuitBox.ClearSelectionFor(characterId, this.Components);
			}
			if (!ids.Any<ushort>())
			{
				return;
			}
			foreach (CircuitBoxComponent node in this.Components)
			{
				if (ids.Contains(node.ID))
				{
					node.SetSelected(Option.Some<ushort>(characterId));
				}
			}
		}

		// Token: 0x06006056 RID: 24662 RVA: 0x00321F58 File Offset: 0x00320158
		private void UpdateSelections([Nullable(new byte[]
		{
			1,
			0
		})] ImmutableDictionary<ushort, Option<ushort>> nodeIds, [Nullable(new byte[]
		{
			1,
			0
		})] ImmutableDictionary<ushort, Option<ushort>> wireIds, [Nullable(new byte[]
		{
			1,
			0
		})] ImmutableDictionary<CircuitBoxInputOutputNode.Type, Option<ushort>> inputOutputs, [Nullable(new byte[]
		{
			1,
			0
		})] ImmutableDictionary<ushort, Option<ushort>> labels)
		{
			foreach (CircuitBoxWire wire in this.Wires)
			{
				Option<ushort> selectedBy;
				if (wireIds.TryGetValue(wire.ID, out selectedBy))
				{
					ushort id;
					if (selectedBy.TryUnwrap(out id))
					{
						wire.IsSelected = true;
						wire.SelectedBy = id;
					}
					else
					{
						wire.IsSelected = false;
						wire.SelectedBy = 0;
					}
				}
			}
			foreach (CircuitBoxComponent node in this.Components)
			{
				Option<ushort> selectedBy2;
				if (nodeIds.TryGetValue(node.ID, out selectedBy2))
				{
					node.SetSelected(selectedBy2);
				}
			}
			foreach (CircuitBoxInputOutputNode node2 in this.InputOutputNodes)
			{
				Option<ushort> selectedBy3;
				if (inputOutputs.TryGetValue(node2.NodeType, out selectedBy3))
				{
					node2.SetSelected(selectedBy3);
				}
			}
			foreach (CircuitBoxLabelNode node3 in this.Labels)
			{
				Option<ushort> selectedBy4;
				if (labels.TryGetValue(node3.ID, out selectedBy4))
				{
					node3.SetSelected(selectedBy4);
				}
			}
		}

		// Token: 0x06006057 RID: 24663 RVA: 0x003220E4 File Offset: 0x003202E4
		private void SelectWiresInternal(IReadOnlyCollection<ushort> ids, ushort characterId, bool overwrite)
		{
			if (overwrite)
			{
				CircuitBox.ClearSelectionFor(characterId, this.Wires);
			}
			foreach (CircuitBoxWire wire in this.Wires)
			{
				if (ids.Contains(wire.ID))
				{
					wire.SetSelected(Option.Some<ushort>(characterId));
				}
			}
		}

		// Token: 0x06006058 RID: 24664 RVA: 0x0032215C File Offset: 0x0032035C
		private void SelectInputOutputInternal(IReadOnlyCollection<CircuitBoxInputOutputNode.Type> io, ushort characterId, bool overwrite)
		{
			if (overwrite)
			{
				CircuitBox.ClearSelectionFor(characterId, this.InputOutputNodes);
			}
			foreach (CircuitBoxInputOutputNode node in this.InputOutputNodes)
			{
				if (io.Contains(node.NodeType))
				{
					node.SetSelected(Option.Some<ushort>(characterId));
				}
			}
		}

		// Token: 0x06006059 RID: 24665 RVA: 0x003221D4 File Offset: 0x003203D4
		private void RemoveComponentInternal(IReadOnlyCollection<ushort> ids)
		{
			foreach (CircuitBoxComponent node in this.Components.ToImmutableArray<CircuitBoxComponent>())
			{
				if (ids.Contains(node.ID))
				{
					this.Components.Remove(node);
					node.Remove();
					foreach (CircuitBoxWire wire in this.Wires.ToImmutableArray<CircuitBoxWire>())
					{
						if (node.Connectors.Contains(wire.From) || node.Connectors.Contains(wire.To))
						{
							this.RemoveWireCollectionUnsafe(wire);
						}
					}
				}
			}
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x0600605A RID: 24666 RVA: 0x00322290 File Offset: 0x00320490
		private void RemoveWireInternal(IReadOnlyCollection<ushort> ids)
		{
			foreach (CircuitBoxWire wire in this.Wires.ToImmutableArray<CircuitBoxWire>())
			{
				if (ids.Contains(wire.ID))
				{
					this.RemoveWireCollectionUnsafe(wire);
				}
			}
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x0600605B RID: 24667 RVA: 0x003222E0 File Offset: 0x003204E0
		public void RemoveWire(Wire wireItem)
		{
			foreach (CircuitBoxWire wire in this.Wires.ToImmutableArray<CircuitBoxWire>())
			{
				Item backingWire;
				if (wire.BackingWire.TryUnwrap(out backingWire) && backingWire == wireItem.Item)
				{
					this.RemoveWireCollectionUnsafe(wire);
				}
			}
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x0600605C RID: 24668 RVA: 0x0032233C File Offset: 0x0032053C
		private void RemoveWireCollectionUnsafe(CircuitBoxWire wire)
		{
			foreach (CircuitBoxOutputConnection output in this.Outputs)
			{
				output.Connection.CircuitBoxConnections.Remove(wire.From);
			}
			wire.From.Connection.CircuitBoxConnections.Remove(wire.To);
			wire.To.Connection.CircuitBoxConnections.Remove(wire.From);
			CircuitBoxInputConnection input = wire.From as CircuitBoxInputConnection;
			if (input != null)
			{
				input.ExternallyConnectedTo.Remove(wire.To);
			}
			wire.To.ExternallyConnectedFrom.Remove(wire.From);
			wire.From.ExternallyConnectedFrom.Remove(wire.To);
			wire.Remove();
			this.Wires.Remove(wire);
		}

		// Token: 0x0600605D RID: 24669 RVA: 0x0032241C File Offset: 0x0032061C
		private void MoveNodesInternal(IReadOnlyCollection<ushort> ids, IReadOnlyCollection<CircuitBoxInputOutputNode.Type> ios, IReadOnlyCollection<ushort> labels, Vector2 moveAmount)
		{
			IEnumerable<CircuitBoxComponent> nodes = from node in this.Components
			where ids.Contains(node.ID)
			select node;
			foreach (CircuitBoxComponent node2 in nodes)
			{
				node2.Position += moveAmount;
			}
			IEnumerable<CircuitBoxLabelNode> labels2 = this.Labels;
			Func<CircuitBoxLabelNode, bool> <>9__1;
			Func<CircuitBoxLabelNode, bool> predicate;
			if ((predicate = <>9__1) == null)
			{
				predicate = (<>9__1 = ((CircuitBoxLabelNode n) => labels.Contains(n.ID)));
			}
			foreach (CircuitBoxLabelNode label in labels2.Where(predicate))
			{
				label.Position += moveAmount;
			}
			foreach (CircuitBoxInputOutputNode io in this.InputOutputNodes)
			{
				if (ios.Contains(io.NodeType))
				{
					io.Position += moveAmount;
				}
			}
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x0600605E RID: 24670 RVA: 0x00322578 File Offset: 0x00320778
		public override bool Select(Character character)
		{
			Holdable component = this.item.GetComponent<Holdable>();
			return (component == null || component.Attached) && base.Select(character);
		}

		// Token: 0x0600605F RID: 24671 RVA: 0x003225A5 File Offset: 0x003207A5
		public void OnViewUpdateProjSpecific()
		{
			CircuitBoxUI ui = this.UI;
			if (ui != null)
			{
				ui.MouseSnapshotHandler.UpdateConnections();
			}
			CircuitBoxUI ui2 = this.UI;
			if (ui2 == null)
			{
				return;
			}
			ui2.UpdateComponentList();
		}

		// Token: 0x06006060 RID: 24672 RVA: 0x003225D0 File Offset: 0x003207D0
		private void InitProjSpecific(ContentXElement element)
		{
			this.UI = new CircuitBoxUI(this);
			this.IsActive = true;
			this.CreateGUI();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "wiresprite"))
				{
					if (!(a == "connectionsprite"))
					{
						if (!(a == "wireconnectorsprite"))
						{
							if (a == "connectionscrewsprite")
							{
								this.ConnectionScrewSprite = new Sprite(subElement, "", "", false, 1f);
							}
						}
						else
						{
							this.WireConnectorSprite = new Sprite(subElement, "", "", false, 1f);
						}
					}
					else
					{
						this.ConnectionSprite = new Sprite(subElement, "", "", false, 1f);
					}
				}
				else
				{
					this.WireSprite = new Sprite(subElement, "", "", false, 1f);
				}
			}
			GUIComponentStyle topStyle = GUIStyle.GetComponentStyle("CircuitBoxTop");
			if (topStyle != null)
			{
				this.NodeTopSprite = topStyle.Sprites[GUIComponent.ComponentState.None][0];
			}
			GUIComponentStyle compStyle = GUIStyle.GetComponentStyle("CircuitBoxFrame");
			if (compStyle != null)
			{
				this.NodeFrameSprite = compStyle.Sprites[GUIComponent.ComponentState.None][0];
			}
		}

		// Token: 0x06006061 RID: 24673 RVA: 0x00322744 File Offset: 0x00320944
		public override void ReceiveSignal(Signal signal, Connection connection)
		{
			foreach (CircuitBoxInputConnection input in this.Inputs)
			{
				if (input.Connection == connection)
				{
					input.ReceiveSignal(signal);
					return;
				}
			}
		}

		// Token: 0x06006062 RID: 24674 RVA: 0x00322784 File Offset: 0x00320984
		public static bool IsRoundRunning()
		{
			if (!Submarine.Unloading)
			{
				GameSession gameSession = GameMain.GameSession;
				return gameSession != null && gameSession.IsRunning;
			}
			return false;
		}

		// Token: 0x06006063 RID: 24675 RVA: 0x003227AC File Offset: 0x003209AC
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<CircuitBox> FindCircuitBox(ushort itemId, byte componentIndex)
		{
			Option.UnspecifiedNone none;
			if (CircuitBox.IsRoundRunning())
			{
				Item item = Entity.FindEntityByID(itemId) as Item;
				if (item != null)
				{
					if ((int)componentIndex >= item.Components.Count)
					{
						none = Option.None;
						return none;
					}
					ItemComponent targetComponent = item.Components[(int)componentIndex];
					CircuitBox circuitBox = targetComponent as CircuitBox;
					if (circuitBox != null)
					{
						return Option.Some<CircuitBox>(circuitBox);
					}
					none = Option.None;
					return none;
				}
			}
			none = Option.None;
			return none;
		}

		// Token: 0x06006064 RID: 24676 RVA: 0x00322824 File Offset: 0x00320A24
		[NullableContext(2)]
		private ItemContainer GetContainerOrNull(int index)
		{
			if (index < 0 || index >= this.containers.Length)
			{
				return null;
			}
			return this.containers[index];
		}

		// Token: 0x06006065 RID: 24677 RVA: 0x00322840 File Offset: 0x00320A40
		public void CreateRefundItemsForUsedResources(IReadOnlyCollection<ushort> ids, [Nullable(2)] Character character)
		{
			if (!CircuitBox.IsInGame())
			{
				return;
			}
			foreach (ItemPrefab prefab in (from comp in this.Components
			where ids.Contains(comp.ID)
			select comp.UsedResource).ToImmutableArray<ItemPrefab>())
			{
				if (((character != null) ? character.Inventory : null) == null)
				{
					EntitySpawner spawner = Entity.Spawner;
					if (spawner != null)
					{
						spawner.AddItemToSpawnQueue(prefab, this.item.Position, this.item.Submarine, null, null, null);
					}
				}
				else
				{
					EntitySpawner spawner2 = Entity.Spawner;
					if (spawner2 != null)
					{
						spawner2.AddItemToSpawnQueue(prefab, character.Inventory, null, null, null, true, false, InvSlotType.None);
					}
				}
			}
		}

		// Token: 0x06006066 RID: 24678 RVA: 0x00322940 File Offset: 0x00320B40
		[NullableContext(2)]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static ImmutableArray<Item> GetSortedCircuitBoxItemsFromPlayer(Character character)
		{
			ImmutableArray<Item>? immutableArray;
			if (character == null)
			{
				immutableArray = null;
			}
			else
			{
				CharacterInventory inventory = character.Inventory;
				if (inventory == null)
				{
					immutableArray = null;
				}
				else
				{
					Func<Item, bool> predicate;
					if ((predicate = CircuitBox.<>O.<0>__CanItemBeAccessed) == null)
					{
						predicate = (CircuitBox.<>O.<0>__CanItemBeAccessed = new Func<Item, bool>(CircuitBox.CanItemBeAccessed));
					}
					immutableArray = new ImmutableArray<Item>?((from i in inventory.FindAllItems(predicate, true, null)
					orderby i.Prefab.Identifier == Tags.FPGACircuit
					select i).ToImmutableArray<Item>());
				}
			}
			ImmutableArray<Item>? immutableArray2 = immutableArray;
			if (immutableArray2 == null)
			{
				return ImmutableArray<Item>.Empty;
			}
			return immutableArray2.GetValueOrDefault();
		}

		// Token: 0x06006067 RID: 24679 RVA: 0x003229D8 File Offset: 0x00320BD8
		public static bool CanItemBeAccessed(Item item)
		{
			Inventory parentInventory = item.ParentInventory;
			ItemInventory ii = parentInventory as ItemInventory;
			return ii == null || ii.Container.DrawInventory;
		}

		// Token: 0x06006068 RID: 24680 RVA: 0x00322A07 File Offset: 0x00320C07
		public bool IsLocked()
		{
			return this.Locked || this.TemporarilyLocked;
		}

		// Token: 0x06006069 RID: 24681 RVA: 0x00322A1C File Offset: 0x00320C1C
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<Item> GetApplicableResourcePlayerHas(ItemPrefab prefab, [Nullable(2)] Character character)
		{
			if (character == null)
			{
				Option.UnspecifiedNone none = Option.None;
				return none;
			}
			return CircuitBox.GetApplicableResourcePlayerHas(prefab, CircuitBox.GetSortedCircuitBoxItemsFromPlayer(character));
		}

		// Token: 0x0600606A RID: 24682 RVA: 0x00322A48 File Offset: 0x00320C48
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<Item> GetApplicableResourcePlayerHas(ItemPrefab prefab, [Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<Item> playerItems)
		{
			foreach (Item invItem in playerItems)
			{
				if (invItem.Prefab == prefab || invItem.Prefab.Identifier == Tags.FPGACircuit)
				{
					return Option.Some<Item>(invItem);
				}
			}
			Option.UnspecifiedNone none = Option.None;
			return none;
		}

		// Token: 0x0600606B RID: 24683 RVA: 0x00322AA4 File Offset: 0x00320CA4
		public static void SpawnItem(ItemPrefab prefab, [Nullable(2)] Character user, [Nullable(2)] ItemContainer container, Action<Item> onSpawned)
		{
			if (container == null)
			{
				throw new Exception("Circuit box has no inventory");
			}
			if (!CircuitBox.IsInGame())
			{
				Item forceSpawnedItem = new Item(prefab, Vector2.Zero, null, 0, true);
				container.Inventory.TryPutItem(forceSpawnedItem, null, null, true, false, true);
				onSpawned(forceSpawnedItem);
				CircuitBox.<SpawnItem>g__AssignWifiComponentTeam|140_1(forceSpawnedItem, user);
				return;
			}
			EntitySpawner spawner = Entity.Spawner;
			if (spawner == null)
			{
				return;
			}
			spawner.AddItemToSpawnQueue(prefab, container.Inventory, null, null, delegate(Item it)
			{
				CircuitBox.<SpawnItem>g__AssignWifiComponentTeam|140_1(it, user);
				onSpawned(it);
			}, true, false, InvSlotType.None);
		}

		// Token: 0x0600606C RID: 24684 RVA: 0x00322B4B File Offset: 0x00320D4B
		public static void RemoveItem(Item item)
		{
			if (!CircuitBox.IsInGame())
			{
				item.Remove();
				return;
			}
			EntitySpawner spawner = Entity.Spawner;
			if (spawner == null)
			{
				return;
			}
			spawner.AddItemToRemoveQueue(item);
		}

		// Token: 0x0600606D RID: 24685 RVA: 0x00322B6C File Offset: 0x00320D6C
		public static bool IsInGame()
		{
			Screen selected = Screen.Selected;
			return selected == null || !selected.IsEditor;
		}

		// Token: 0x0600606E RID: 24686 RVA: 0x00322B8E File Offset: 0x00320D8E
		public static bool IsCircuitBoxSelected(Character character)
		{
			Item selectedItem = character.SelectedItem;
			return ((selectedItem != null) ? selectedItem.GetComponent<CircuitBox>() : null) != null;
		}

		// Token: 0x06006070 RID: 24688 RVA: 0x00322BB8 File Offset: 0x00320DB8
		[CompilerGenerated]
		private void <UpdateHUDComponentSpecific>g__RecordCursorPosition|35_1(int index, ref CircuitBox.<>c__DisplayClass35_0 A_2)
		{
			Option<Vector2> dragStart = this.UI.GetDragStart();
			if (dragStart.IsSome())
			{
				this.recordedDragStart = dragStart;
			}
			Option<ItemPrefab> heldComponent = this.HeldComponent;
			if (heldComponent.IsSome())
			{
				this.recordedHeldPrefab = heldComponent;
			}
			if (index >= 0 && index < this.recordedCursorPositions.Length)
			{
				this.recordedCursorPositions[index] = A_2.cursorPos;
			}
		}

		// Token: 0x06006071 RID: 24689 RVA: 0x00322C1C File Offset: 0x00320E1C
		[CompilerGenerated]
		private void <LoadFromXML>g__LoadFor|96_0(CircuitBoxInputOutputNode.Type type, ContentXElement subElement)
		{
			foreach (CircuitBoxInputOutputNode node in this.InputOutputNodes)
			{
				if (node.NodeType == type)
				{
					node.Load(subElement);
					break;
				}
			}
		}

		// Token: 0x06006072 RID: 24690 RVA: 0x00322C7C File Offset: 0x00320E7C
		[CompilerGenerated]
		internal static void <SpawnItem>g__AssignWifiComponentTeam|140_1(Item item, [Nullable(2)] Character user)
		{
			if (user == null)
			{
				return;
			}
			foreach (WifiComponent wifiComponent in item.GetComponents<WifiComponent>())
			{
				wifiComponent.TeamID = user.TeamID;
			}
		}

		// Token: 0x040031B0 RID: 12720
		[Nullable(2)]
		public CircuitBoxUI UI;

		// Token: 0x040031B1 RID: 12721
		public readonly Dictionary<Character, CircuitBoxCursor> ActiveCursors = new Dictionary<Character, CircuitBoxCursor>();

		// Token: 0x040031B2 RID: 12722
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public Option<ItemPrefab> HeldComponent;

		// Token: 0x040031B3 RID: 12723
		private const float CursorUpdateInterval = 1f;

		// Token: 0x040031B4 RID: 12724
		private float cursorUpdateTimer;

		// Token: 0x040031B5 RID: 12725
		private readonly Vector2[] recordedCursorPositions;

		// Token: 0x040031B6 RID: 12726
		[Nullable(0)]
		private Option<Vector2> recordedDragStart;

		// Token: 0x040031B7 RID: 12727
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private Option<ItemPrefab> recordedHeldPrefab;

		// Token: 0x040031B8 RID: 12728
		private bool wasInitializedByServer;

		// Token: 0x040031BF RID: 12735
		public static readonly ImmutableHashSet<CircuitBoxOpcode> UnrealiableOpcodes = ImmutableHashSet.Create<CircuitBoxOpcode>(CircuitBoxOpcode.Cursor);

		// Token: 0x040031C0 RID: 12736
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<CircuitBoxInputConnection> Inputs;

		// Token: 0x040031C1 RID: 12737
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<CircuitBoxOutputConnection> Outputs;

		// Token: 0x040031C2 RID: 12738
		public readonly List<CircuitBoxComponent> Components;

		// Token: 0x040031C3 RID: 12739
		public readonly List<CircuitBoxInputOutputNode> InputOutputNodes;

		// Token: 0x040031C4 RID: 12740
		public readonly List<CircuitBoxLabelNode> Labels;

		// Token: 0x040031C5 RID: 12741
		public readonly List<CircuitBoxWire> Wires;

		// Token: 0x040031C6 RID: 12742
		public readonly ItemContainer[] containers;

		// Token: 0x040031C7 RID: 12743
		private const int ComponentContainerIndex = 0;

		// Token: 0x040031C8 RID: 12744
		private const int WireContainerIndex = 1;

		// Token: 0x040031C9 RID: 12745
		public bool TemporarilyLocked;

		// Token: 0x040031CB RID: 12747
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private Option<ContentXElement> delayedElementToLoad;

		// Token: 0x02001457 RID: 5207
		[NullableContext(0)]
		public struct CreatedWire : IEquatable<CircuitBox.CreatedWire>
		{
			// Token: 0x06009A83 RID: 39555 RVA: 0x003E34D0 File Offset: 0x003E16D0
			public CreatedWire(CircuitBoxConnectorIdentifier Start, CircuitBoxConnectorIdentifier End, [Nullable(new byte[]
			{
				0,
				1
			})] Option<Item> Item, ushort ID)
			{
				this.Start = Start;
				this.End = End;
				this.Item = Item;
				this.ID = ID;
			}

			// Token: 0x17001D4F RID: 7503
			// (get) Token: 0x06009A84 RID: 39556 RVA: 0x003E34EF File Offset: 0x003E16EF
			// (set) Token: 0x06009A85 RID: 39557 RVA: 0x003E34F7 File Offset: 0x003E16F7
			public CircuitBoxConnectorIdentifier Start { readonly get; set; }

			// Token: 0x17001D50 RID: 7504
			// (get) Token: 0x06009A86 RID: 39558 RVA: 0x003E3500 File Offset: 0x003E1700
			// (set) Token: 0x06009A87 RID: 39559 RVA: 0x003E3508 File Offset: 0x003E1708
			public CircuitBoxConnectorIdentifier End { readonly get; set; }

			// Token: 0x17001D51 RID: 7505
			// (get) Token: 0x06009A88 RID: 39560 RVA: 0x003E3511 File Offset: 0x003E1711
			// (set) Token: 0x06009A89 RID: 39561 RVA: 0x003E3519 File Offset: 0x003E1719
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public Option<Item> Item { [return: Nullable(new byte[]
			{
				0,
				1
			})] readonly get; [param: Nullable(new byte[]
			{
				0,
				1
			})] set; }

			// Token: 0x17001D52 RID: 7506
			// (get) Token: 0x06009A8A RID: 39562 RVA: 0x003E3522 File Offset: 0x003E1722
			// (set) Token: 0x06009A8B RID: 39563 RVA: 0x003E352A File Offset: 0x003E172A
			public ushort ID { readonly get; set; }

			// Token: 0x06009A8C RID: 39564 RVA: 0x003E3534 File Offset: 0x003E1734
			[CompilerGenerated]
			public override readonly string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("CreatedWire");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06009A8D RID: 39565 RVA: 0x003E3580 File Offset: 0x003E1780
			[CompilerGenerated]
			private readonly bool PrintMembers(StringBuilder builder)
			{
				builder.Append("Start = ");
				builder.Append(this.Start.ToString());
				builder.Append(", End = ");
				builder.Append(this.End.ToString());
				builder.Append(", Item = ");
				builder.Append(this.Item.ToString());
				builder.Append(", ID = ");
				builder.Append(this.ID.ToString());
				return true;
			}

			// Token: 0x06009A8E RID: 39566 RVA: 0x003E362A File Offset: 0x003E182A
			[CompilerGenerated]
			public static bool operator !=(CircuitBox.CreatedWire left, CircuitBox.CreatedWire right)
			{
				return !(left == right);
			}

			// Token: 0x06009A8F RID: 39567 RVA: 0x003E3636 File Offset: 0x003E1836
			[CompilerGenerated]
			public static bool operator ==(CircuitBox.CreatedWire left, CircuitBox.CreatedWire right)
			{
				return left.Equals(right);
			}

			// Token: 0x06009A90 RID: 39568 RVA: 0x003E3640 File Offset: 0x003E1840
			[CompilerGenerated]
			public override readonly int GetHashCode()
			{
				return ((EqualityComparer<CircuitBoxConnectorIdentifier>.Default.GetHashCode(this.<Start>k__BackingField) * -1521134295 + EqualityComparer<CircuitBoxConnectorIdentifier>.Default.GetHashCode(this.<End>k__BackingField)) * -1521134295 + EqualityComparer<Option<Barotrauma.Item>>.Default.GetHashCode(this.<Item>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<ID>k__BackingField);
			}

			// Token: 0x06009A91 RID: 39569 RVA: 0x003E36A2 File Offset: 0x003E18A2
			[CompilerGenerated]
			public override readonly bool Equals(object obj)
			{
				return obj is CircuitBox.CreatedWire && this.Equals((CircuitBox.CreatedWire)obj);
			}

			// Token: 0x06009A92 RID: 39570 RVA: 0x003E36BC File Offset: 0x003E18BC
			[CompilerGenerated]
			public readonly bool Equals(CircuitBox.CreatedWire other)
			{
				return EqualityComparer<CircuitBoxConnectorIdentifier>.Default.Equals(this.<Start>k__BackingField, other.<Start>k__BackingField) && EqualityComparer<CircuitBoxConnectorIdentifier>.Default.Equals(this.<End>k__BackingField, other.<End>k__BackingField) && EqualityComparer<Option<Barotrauma.Item>>.Default.Equals(this.<Item>k__BackingField, other.<Item>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<ID>k__BackingField, other.<ID>k__BackingField);
			}

			// Token: 0x06009A93 RID: 39571 RVA: 0x003E3729 File Offset: 0x003E1929
			[CompilerGenerated]
			public readonly void Deconstruct(out CircuitBoxConnectorIdentifier Start, out CircuitBoxConnectorIdentifier End, [Nullable(new byte[]
			{
				0,
				1
			})] out Option<Item> Item, out ushort ID)
			{
				Start = this.Start;
				End = this.End;
				Item = this.Item;
				ID = this.ID;
			}
		}

		// Token: 0x02001458 RID: 5208
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400655A RID: 25946
			[Nullable(0)]
			public static Func<Item, bool> <0>__CanItemBeAccessed;
		}
	}
}
