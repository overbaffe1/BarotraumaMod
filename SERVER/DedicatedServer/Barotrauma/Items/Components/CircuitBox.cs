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
	// Token: 0x020004A8 RID: 1192
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class CircuitBox : ItemComponent, IClientSerializable, INetSerializable, IServerSerializable
	{
		// Token: 0x060042CD RID: 17101 RVA: 0x001AC2BA File Offset: 0x001AA4BA
		public void MarkServerRequiredInitialization()
		{
			this.needsServerInitialization = true;
		}

		// Token: 0x060042CE RID: 17102 RVA: 0x001AC2C4 File Offset: 0x001AA4C4
		public void ServerRead(INetSerializableStruct data, Client c)
		{
			if (data is NetCircuitBoxCursorInfo)
			{
				NetCircuitBoxCursorInfo cursorInfo = (NetCircuitBoxCursorInfo)data;
				Vector2[] recordedPositions = cursorInfo.RecordedPositions;
				if (recordedPositions != null)
				{
					int num = recordedPositions.Length;
					if (num == 10)
					{
						this.RelayCursorState(cursorInfo, c);
					}
				}
			}
		}

		// Token: 0x060042CF RID: 17103 RVA: 0x001AC2FC File Offset: 0x001AA4FC
		private void RelayCursorState(NetCircuitBoxCursorInfo data, Client sender)
		{
			CircuitBox.<>c__DisplayClass3_0 CS$<>8__locals1 = new CircuitBox.<>c__DisplayClass3_0();
			CS$<>8__locals1.sender = sender;
			CS$<>8__locals1.<>4__this = this;
			if (GameMain.Server == null || !CircuitBox.IsRoundRunning())
			{
				return;
			}
			CircuitBoxOpcode opcode = CircuitBoxOpcode.Cursor;
			NetCircuitBoxCursorInfo netCircuitBoxCursorInfo = data;
			netCircuitBoxCursorInfo.CharacterID = CS$<>8__locals1.sender.CharacterID;
			this.SendToAll(opcode, netCircuitBoxCursorInfo, new Func<Client, bool>(CS$<>8__locals1.<RelayCursorState>g__FilterClients|0));
		}

		// Token: 0x060042D0 RID: 17104 RVA: 0x001AC35C File Offset: 0x001AA55C
		public void SendToClient(CircuitBoxOpcode opcode, INetSerializableStruct data, Client targetClient)
		{
			ValueTuple<IWriteMessage, DeliveryMethod> valueTuple = this.PrepareToSend(opcode, data);
			IWriteMessage msg = valueTuple.Item1;
			DeliveryMethod deliveryMethod = valueTuple.Item2;
			GameServer server = GameMain.Server;
			if (server == null)
			{
				return;
			}
			ServerPeer serverPeer = server.ServerPeer;
			if (serverPeer == null)
			{
				return;
			}
			serverPeer.Send(msg, targetClient.Connection, deliveryMethod, true);
		}

		// Token: 0x060042D1 RID: 17105 RVA: 0x001AC3A0 File Offset: 0x001AA5A0
		public void SendToAll(CircuitBoxOpcode opcode, INetSerializableStruct data, [Nullable(new byte[]
		{
			2,
			1
		})] Func<Client, bool> predicate = null)
		{
			ValueTuple<IWriteMessage, DeliveryMethod> valueTuple = this.PrepareToSend(opcode, data);
			IWriteMessage msg = valueTuple.Item1;
			DeliveryMethod deliveryMethod = valueTuple.Item2;
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				if (predicate == null || predicate(client))
				{
					GameServer server = GameMain.Server;
					if (server != null)
					{
						ServerPeer serverPeer = server.ServerPeer;
						if (serverPeer != null)
						{
							serverPeer.Send(msg, client.Connection, deliveryMethod, true);
						}
					}
				}
			}
		}

		// Token: 0x060042D2 RID: 17106 RVA: 0x001AC430 File Offset: 0x001AA630
		[return: TupleElementNames(new string[]
		{
			"Message",
			"DeliveryMethod"
		})]
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private ValueTuple<IWriteMessage, DeliveryMethod> PrepareToSend(CircuitBoxOpcode opcode, INetSerializableStruct data)
		{
			IWriteMessage msg = new WriteOnlyMessage().WithHeader(ServerPacketHeader.CIRCUITBOX);
			msg.WriteNetSerializableStruct(new NetCircuitBoxHeader(opcode, this.item.ID, (byte)this.item.GetComponentIndex(this)));
			msg.WriteNetSerializableStruct(data);
			DeliveryMethod deliveryMethod = CircuitBox.UnrealiableOpcodes.Contains(opcode) ? DeliveryMethod.Unreliable : DeliveryMethod.Reliable;
			return new ValueTuple<IWriteMessage, DeliveryMethod>(msg, deliveryMethod);
		}

		// Token: 0x060042D3 RID: 17107 RVA: 0x001AC48E File Offset: 0x001AA68E
		public void CreateServerEvent(INetSerializableStruct data)
		{
			this.item.CreateServerEvent<CircuitBox>(this, new CircuitBoxEventData(data));
		}

		// Token: 0x060042D4 RID: 17108 RVA: 0x001AC4A8 File Offset: 0x001AA6A8
		public void ServerEventWrite(IWriteMessage msg, Client c, [Nullable(2)] NetEntityEvent.IData extraData = null)
		{
			if (extraData == null)
			{
				return;
			}
			CircuitBoxEventData eventData = base.ExtractEventData<CircuitBoxEventData>(extraData);
			msg.WriteByte((byte)eventData.Opcode);
			msg.WriteNetSerializableStruct(eventData.Data);
		}

		// Token: 0x060042D5 RID: 17109 RVA: 0x001AC4DC File Offset: 0x001AA6DC
		public void ServerEventRead(IReadMessage msg, Client c)
		{
			CircuitBoxOpcode header = (CircuitBoxOpcode)msg.ReadByte();
			switch (header)
			{
			case CircuitBoxOpcode.AddComponent:
			{
				CircuitBoxAddComponentEvent data = INetSerializableStruct.Read<CircuitBoxAddComponentEvent>(msg);
				if (!this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					return;
				}
				ItemPrefab prefab = ItemPrefab.Prefabs.Find((ItemPrefab p) => p.UintIdentifier == data.PrefabIdentifier);
				if (prefab == null)
				{
					this.ThrowError("Unable to add component because the prefab was not found.", c);
					return;
				}
				Item resource;
				if (this.IsFull || !CircuitBox.GetApplicableResourcePlayerHas(prefab, c.Character).TryUnwrap(out resource))
				{
					return;
				}
				ushort id = ICircuitBoxIdentifiable.FindFreeID<CircuitBoxComponent>(this.Components);
				if (id == 65535)
				{
					this.ThrowError("Unable to add component because there are no available IDs left.", c);
					return;
				}
				if (!this.AddComponentInternal(id, prefab, resource.Prefab, data.Position, c.Character, delegate(Item it)
				{
					this.CreateServerEvent(new CircuitBoxServerCreateComponentEvent(it.ID, resource.Prefab.UintIdentifier, id, data.Position));
				}))
				{
					this.ThrowError("Unable to add component because the component could not be created.", c);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(29, 2);
				defaultInterpolatedStringHandler.AppendFormatted(NetworkMember.ClientLogName(c, null));
				defaultInterpolatedStringHandler.AppendLiteral(" added a ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(prefab.Name);
				defaultInterpolatedStringHandler.AppendLiteral(" into a circuit box.");
				GameServer.Log(defaultInterpolatedStringHandler.ToStringAndClear(), ServerLog.MessageType.Wiring);
				CircuitBox.RemoveItem(resource);
				return;
			}
			case CircuitBoxOpcode.MoveComponent:
			{
				CircuitBoxMoveComponentEvent data11 = INetSerializableStruct.Read<CircuitBoxMoveComponentEvent>(msg);
				if (this.item.CanClientAccess(c))
				{
					this.MoveNodesInternal(data11.TargetIDs, data11.IOs, data11.LabelIDs, data11.MoveAmount);
					this.CreateServerEvent(data11);
					return;
				}
				return;
			}
			case CircuitBoxOpcode.AddWire:
			{
				CircuitBoxClientAddWireEvent data = INetSerializableStruct.Read<CircuitBoxClientAddWireEvent>(msg);
				if (!this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					return;
				}
				ItemPrefab prefab2 = ItemPrefab.Prefabs.Find((ItemPrefab p) => p.UintIdentifier == data.SelectedWirePrefabIdentifier);
				if (prefab2 == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(67, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Unable to connect wire because wire by identifier \"");
					defaultInterpolatedStringHandler2.AppendFormatted<uint>(data.SelectedWirePrefabIdentifier);
					defaultInterpolatedStringHandler2.AppendLiteral("\" was not found.");
					this.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), c);
					return;
				}
				CircuitBoxConnection start;
				CircuitBoxConnection end;
				if (data.Start.FindConnection(this).TryUnwrap(out start) && data.End.FindConnection(this).TryUnwrap(out end))
				{
					if (!this.Connect(start, end, delegate(CircuitBox.CreatedWire wire)
					{
						CircuitBox <>4__this = this;
						CircuitBoxClientAddWireEvent data12 = data;
						data12.Start = wire.Start;
						data12.End = wire.End;
						<>4__this.CreateServerEvent(new CircuitBoxServerCreateWireEvent(data12, wire.ID, from i in wire.Item
						select i.ID));
					}, prefab2))
					{
						this.ThrowError("Unable to connect wire because the circuit box rejected it.", c);
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(45, 3);
					defaultInterpolatedStringHandler3.AppendFormatted(NetworkMember.ClientLogName(c, null));
					defaultInterpolatedStringHandler3.AppendLiteral(" connected a wire from ");
					defaultInterpolatedStringHandler3.AppendFormatted(start.Name);
					defaultInterpolatedStringHandler3.AppendLiteral(" to ");
					defaultInterpolatedStringHandler3.AppendFormatted(end.Name);
					defaultInterpolatedStringHandler3.AppendLiteral(" in a circuit box.");
					GameServer.Log(defaultInterpolatedStringHandler3.ToStringAndClear(), ServerLog.MessageType.Wiring);
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(90, 2);
				defaultInterpolatedStringHandler4.AppendLiteral("Unable to connect wire because the start or end connection was not found. (start: ");
				defaultInterpolatedStringHandler4.AppendFormatted<CircuitBoxConnectorIdentifier>(data.Start);
				defaultInterpolatedStringHandler4.AppendLiteral(", end: ");
				defaultInterpolatedStringHandler4.AppendFormatted<CircuitBoxConnectorIdentifier>(data.End);
				defaultInterpolatedStringHandler4.AppendLiteral(")");
				this.ThrowError(defaultInterpolatedStringHandler4.ToStringAndClear(), c);
				return;
			}
			case CircuitBoxOpcode.RemoveWire:
			{
				CircuitBoxRemoveWireEvent data2 = INetSerializableStruct.Read<CircuitBoxRemoveWireEvent>(msg);
				if (data2.TargetIDs.Any<ushort>() && this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					GameServer.Log(NetworkMember.ClientLogName(c, null) + " removed " + this.<ServerEventRead>g__GetLogWireName|9_1(data2.TargetIDs) + " from circuit box.", ServerLog.MessageType.Wiring);
					this.RemoveWireInternal(data2.TargetIDs);
					this.CreateServerEvent(data2);
					return;
				}
				return;
			}
			case CircuitBoxOpcode.SelectComponents:
			{
				CircuitBoxSelectNodesEvent data3 = INetSerializableStruct.Read<CircuitBoxSelectNodesEvent>(msg);
				if (this.item.CanClientAccess(c))
				{
					this.SelectComponentsInternal(data3.TargetIDs, c.CharacterID, data3.Overwrite);
					this.SelectInputOutputInternal(data3.IOs, c.CharacterID, data3.Overwrite);
					this.SelectLabelsInternal(data3.LabelIDs, c.CharacterID, data3.Overwrite);
					this.BroadcastSelectionStatus();
					return;
				}
				return;
			}
			case CircuitBoxOpcode.SelectWires:
			{
				CircuitBoxSelectWiresEvent data4 = INetSerializableStruct.Read<CircuitBoxSelectWiresEvent>(msg);
				if (this.item.CanClientAccess(c))
				{
					this.SelectWiresInternal(data4.TargetIDs, c.CharacterID, data4.Overwrite);
					this.BroadcastSelectionStatus();
					return;
				}
				return;
			}
			case CircuitBoxOpcode.DeleteComponent:
			{
				CircuitBoxRemoveComponentEvent data5 = INetSerializableStruct.Read<CircuitBoxRemoveComponentEvent>(msg);
				if (data5.TargetIDs.Any<ushort>() && this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					this.CreateRefundItemsForUsedResources(data5.TargetIDs, c.Character);
					GameServer.Log(NetworkMember.ClientLogName(c, null) + " removed " + this.<ServerEventRead>g__GetLogComponentName|9_0(data5.TargetIDs) + " from circuit box.", ServerLog.MessageType.Wiring);
					this.RemoveComponentInternal(data5.TargetIDs);
					this.CreateServerEvent(data5);
					return;
				}
				return;
			}
			case CircuitBoxOpcode.RenameLabel:
			{
				CircuitBoxRenameLabelEvent data6 = INetSerializableStruct.Read<CircuitBoxRenameLabelEvent>(msg);
				if (this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					this.RenameLabelInternal(data6.LabelId, data6.Color, data6.NewHeader, data6.NewBody);
					this.CreateServerEvent(data6);
					return;
				}
				return;
			}
			case CircuitBoxOpcode.AddLabel:
			{
				CircuitBoxAddLabelEvent data7 = INetSerializableStruct.Read<CircuitBoxAddLabelEvent>(msg);
				if (!this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					return;
				}
				ushort id2 = ICircuitBoxIdentifiable.FindFreeID<CircuitBoxLabelNode>(this.Labels);
				if (id2 == 65535)
				{
					this.ThrowError("Unable to add label because there are no available IDs left.", c);
					return;
				}
				this.AddLabelInternal(id2, data7.Color, data7.Position, data7.Header, data7.Body);
				this.CreateServerEvent(new CircuitBoxServerAddLabelEvent(id2, data7.Position, new Vector2(256f), data7.Color, data7.Header, data7.Body));
				return;
			}
			case CircuitBoxOpcode.RemoveLabel:
			{
				CircuitBoxRemoveLabelEvent data8 = INetSerializableStruct.Read<CircuitBoxRemoveLabelEvent>(msg);
				if (this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					this.RemoveLabelInternal(data8.TargetIDs);
					this.CreateServerEvent(data8);
					return;
				}
				return;
			}
			case CircuitBoxOpcode.ResizeLabel:
			{
				CircuitBoxResizeLabelEvent data9 = INetSerializableStruct.Read<CircuitBoxResizeLabelEvent>(msg);
				if (this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					this.ResizeLabelInternal(data9.ID, data9.Position, data9.Size);
					CircuitBoxResizeLabelEvent circuitBoxResizeLabelEvent = data9;
					circuitBoxResizeLabelEvent.Size = Vector2.Max(data9.Size, CircuitBoxLabelNode.MinSize);
					this.CreateServerEvent(circuitBoxResizeLabelEvent);
					return;
				}
				return;
			}
			case CircuitBoxOpcode.RenameConnections:
			{
				CircuitBoxRenameConnectionLabelsEvent data10 = INetSerializableStruct.Read<CircuitBoxRenameConnectionLabelsEvent>(msg);
				if (this.<ServerEventRead>g__CanAccessAndUnlocked|9_2(c))
				{
					this.RenameConnectionLabelsInternal(data10.Type, data10.Override.ToDictionary());
					this.CreateServerEvent(data10);
					return;
				}
				return;
			}
			}
			throw new ArgumentOutOfRangeException("header", header, "This opcode cannot be handled using entity events");
		}

		// Token: 0x060042D6 RID: 17110 RVA: 0x001ACBFC File Offset: 0x001AADFC
		public void CreateInitializationEvent()
		{
			Vector2 inputPos = Vector2.Zero;
			Vector2 outputPos = Vector2.Zero;
			foreach (CircuitBoxInputOutputNode ioNode in this.InputOutputNodes)
			{
				CircuitBoxInputOutputNode.Type nodeType = ioNode.NodeType;
				if (nodeType != CircuitBoxInputOutputNode.Type.Input)
				{
					if (nodeType == CircuitBoxInputOutputNode.Type.Output)
					{
						outputPos = ioNode.Position;
					}
				}
				else
				{
					inputPos = ioNode.Position;
				}
			}
			IEnumerable<CircuitBoxComponent> components = this.Components;
			Func<CircuitBoxComponent, CircuitBoxServerCreateComponentEvent> selector;
			if ((selector = CircuitBox.<>O.<0>__EventFromComponent) == null)
			{
				selector = (CircuitBox.<>O.<0>__EventFromComponent = new Func<CircuitBoxComponent, CircuitBoxServerCreateComponentEvent>(CircuitBox.<CreateInitializationEvent>g__EventFromComponent|10_0));
			}
			ImmutableArray<CircuitBoxServerCreateComponentEvent> components2 = components.Select(selector).ToImmutableArray<CircuitBoxServerCreateComponentEvent>();
			IEnumerable<CircuitBoxWire> wires = this.Wires;
			Func<CircuitBoxWire, CircuitBoxServerCreateWireEvent> selector2;
			if ((selector2 = CircuitBox.<>O.<1>__EventFromWire) == null)
			{
				selector2 = (CircuitBox.<>O.<1>__EventFromWire = new Func<CircuitBoxWire, CircuitBoxServerCreateWireEvent>(CircuitBox.<CreateInitializationEvent>g__EventFromWire|10_1));
			}
			ImmutableArray<CircuitBoxServerCreateWireEvent> wires2 = wires.Select(selector2).ToImmutableArray<CircuitBoxServerCreateWireEvent>();
			IEnumerable<CircuitBoxLabelNode> labels = this.Labels;
			Func<CircuitBoxLabelNode, CircuitBoxServerAddLabelEvent> selector3;
			if ((selector3 = CircuitBox.<>O.<2>__EventFromLabel) == null)
			{
				selector3 = (CircuitBox.<>O.<2>__EventFromLabel = new Func<CircuitBoxLabelNode, CircuitBoxServerAddLabelEvent>(CircuitBox.<CreateInitializationEvent>g__EventFromLabel|10_2));
			}
			ImmutableArray<CircuitBoxServerAddLabelEvent> labels2 = labels.Select(selector3).ToImmutableArray<CircuitBoxServerAddLabelEvent>();
			IEnumerable<CircuitBoxInputOutputNode> inputOutputNodes = this.InputOutputNodes;
			Func<CircuitBoxInputOutputNode, CircuitBoxRenameConnectionLabelsEvent> selector4;
			if ((selector4 = CircuitBox.<>O.<3>__EventFromLabelOverride) == null)
			{
				selector4 = (CircuitBox.<>O.<3>__EventFromLabelOverride = new Func<CircuitBoxInputOutputNode, CircuitBoxRenameConnectionLabelsEvent>(CircuitBox.<CreateInitializationEvent>g__EventFromLabelOverride|10_3));
			}
			CircuitBoxInitializeStateFromServerEvent data = new CircuitBoxInitializeStateFromServerEvent(components2, wires2, labels2, inputOutputNodes.Select(selector4).ToImmutableArray<CircuitBoxRenameConnectionLabelsEvent>(), inputPos, outputPos);
			this.CreateServerEvent(data);
		}

		// Token: 0x060042D7 RID: 17111 RVA: 0x001ACD40 File Offset: 0x001AAF40
		private void ThrowError(string message, Client c)
		{
			DebugConsole.ThrowError(message, null, this.item.Prefab.ContentPackage, false, false);
			this.SendToClient(CircuitBoxOpcode.Error, new CircuitBoxErrorEvent(message), c);
		}

		// Token: 0x060042D8 RID: 17112 RVA: 0x001ACD70 File Offset: 0x001AAF70
		private void BroadcastSelectionStatus()
		{
			ImmutableArray<CircuitBoxIdSelectionPair> nodes = this.Components.Select(delegate(CircuitBoxComponent c)
			{
				ushort id = c.ID;
				Option<ushort> selectedBy;
				if (!c.IsSelected)
				{
					Option.UnspecifiedNone none = Option.None;
					selectedBy = none;
				}
				else
				{
					selectedBy = Option.Some<ushort>(c.SelectedBy);
				}
				return new CircuitBoxIdSelectionPair(id, selectedBy);
			}).ToImmutableArray<CircuitBoxIdSelectionPair>();
			ImmutableArray<CircuitBoxIdSelectionPair> wires = this.Wires.Select(delegate(CircuitBoxWire w)
			{
				ushort id = w.ID;
				Option<ushort> selectedBy;
				if (!w.IsSelected)
				{
					Option.UnspecifiedNone none = Option.None;
					selectedBy = none;
				}
				else
				{
					selectedBy = Option.Some<ushort>(w.SelectedBy);
				}
				return new CircuitBoxIdSelectionPair(id, selectedBy);
			}).ToImmutableArray<CircuitBoxIdSelectionPair>();
			ImmutableArray<CircuitBoxTypeSelectionPair> ios = this.InputOutputNodes.Select(delegate(CircuitBoxInputOutputNode n)
			{
				CircuitBoxInputOutputNode.Type nodeType = n.NodeType;
				Option<ushort> selectedBy;
				if (!n.IsSelected)
				{
					Option.UnspecifiedNone none = Option.None;
					selectedBy = none;
				}
				else
				{
					selectedBy = Option.Some<ushort>(n.SelectedBy);
				}
				return new CircuitBoxTypeSelectionPair(nodeType, selectedBy);
			}).ToImmutableArray<CircuitBoxTypeSelectionPair>();
			ImmutableArray<CircuitBoxIdSelectionPair> labels = this.Labels.Select(delegate(CircuitBoxLabelNode n)
			{
				ushort id = n.ID;
				Option<ushort> selectedBy;
				if (!n.IsSelected)
				{
					Option.UnspecifiedNone none = Option.None;
					selectedBy = none;
				}
				else
				{
					selectedBy = Option.Some<ushort>(n.SelectedBy);
				}
				return new CircuitBoxIdSelectionPair(id, selectedBy);
			}).ToImmutableArray<CircuitBoxIdSelectionPair>();
			this.CreateServerEvent(new CircuitBoxServerUpdateSelection(nodes, wires, ios, labels));
		}

		// Token: 0x170011DB RID: 4571
		// (get) Token: 0x060042D9 RID: 17113 RVA: 0x001ACE51 File Offset: 0x001AB051
		public override bool IsActive
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170011DC RID: 4572
		// (get) Token: 0x060042DA RID: 17114 RVA: 0x001ACE54 File Offset: 0x001AB054
		public override bool DontTransferInventoryBetweenSubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x170011DD RID: 4573
		// (get) Token: 0x060042DB RID: 17115 RVA: 0x001ACE57 File Offset: 0x001AB057
		public override bool DisallowSellingItemsFromContainer
		{
			get
			{
				return true;
			}
		}

		// Token: 0x060042DC RID: 17116 RVA: 0x001ACE5C File Offset: 0x001AB05C
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

		// Token: 0x060042DD RID: 17117 RVA: 0x001ACEE4 File Offset: 0x001AB0E4
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

		// Token: 0x170011DE RID: 4574
		// (get) Token: 0x060042DE RID: 17118 RVA: 0x001ACF5D File Offset: 0x001AB15D
		[Nullable(2)]
		public ItemContainer ComponentContainer
		{
			[NullableContext(2)]
			get
			{
				return this.GetContainerOrNull(0);
			}
		}

		// Token: 0x170011DF RID: 4575
		// (get) Token: 0x060042DF RID: 17119 RVA: 0x001ACF66 File Offset: 0x001AB166
		[Nullable(2)]
		public ItemContainer WireContainer
		{
			[NullableContext(2)]
			get
			{
				return this.GetContainerOrNull(1) ?? this.GetContainerOrNull(0);
			}
		}

		// Token: 0x170011E0 RID: 4576
		// (get) Token: 0x060042E0 RID: 17120 RVA: 0x001ACF7C File Offset: 0x001AB17C
		public bool IsFull
		{
			get
			{
				ItemContainer componentContainer = this.ComponentContainer;
				ItemInventory inventory = (componentContainer != null) ? componentContainer.Inventory : null;
				return inventory != null && inventory.IsFull(true);
			}
		}

		// Token: 0x170011E1 RID: 4577
		// (get) Token: 0x060042E1 RID: 17121 RVA: 0x001ACFA8 File Offset: 0x001AB1A8
		// (set) Token: 0x060042E2 RID: 17122 RVA: 0x001ACFB0 File Offset: 0x001AB1B0
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Locked circuit boxes can only be viewed and not interacted with.", "", false)]
		public bool Locked { get; private set; }

		// Token: 0x060042E3 RID: 17123 RVA: 0x001ACFBC File Offset: 0x001AB1BC
		public CircuitBox(Item item, ContentXElement element) : base(item, element)
		{
			this.containers = item.GetComponents<ItemContainer>().ToArray<ItemContainer>();
			if (this.containers.Length < 1)
			{
				DebugConsole.ThrowError("Circuit box must have at least one item container to function.", null, null, false, false);
			}
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

		// Token: 0x060042E4 RID: 17124 RVA: 0x001AD158 File Offset: 0x001AB358
		public override void Load(ContentXElement componentElement, bool usePrefabValues, IdRemap idRemap, bool isItemSwap)
		{
			base.Load(componentElement, usePrefabValues, idRemap, isItemSwap);
			if (this.delayedElementToLoad.IsSome())
			{
				return;
			}
			this.delayedElementToLoad = Option.Some<ContentXElement>(componentElement);
		}

		// Token: 0x060042E5 RID: 17125 RVA: 0x001AD17F File Offset: 0x001AB37F
		public override void OnInventoryChanged()
		{
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x060042E6 RID: 17126 RVA: 0x001AD187 File Offset: 0x001AB387
		public override void Update(float deltaTime, Camera cam)
		{
			this.TryInitializeNodes();
		}

		// Token: 0x060042E7 RID: 17127 RVA: 0x001AD18F File Offset: 0x001AB38F
		public override void OnMapLoaded()
		{
			this.TryInitializeNodes();
		}

		// Token: 0x060042E8 RID: 17128 RVA: 0x001AD198 File Offset: 0x001AB398
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

		// Token: 0x060042E9 RID: 17129 RVA: 0x001AD1D0 File Offset: 0x001AB3D0
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
								this.<LoadFromXML>g__LoadFor|49_0(CircuitBoxInputOutputNode.Type.Output, subElement);
							}
						}
						else
						{
							this.<LoadFromXML>g__LoadFor|49_0(CircuitBoxInputOutputNode.Type.Input, subElement);
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
			if (this.needsServerInitialization)
			{
				this.CreateInitializationEvent();
				this.needsServerInitialization = false;
			}
		}

		// Token: 0x060042EA RID: 17130 RVA: 0x001AD2F8 File Offset: 0x001AB4F8
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

		// Token: 0x060042EB RID: 17131 RVA: 0x001AD594 File Offset: 0x001AB794
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

		// Token: 0x060042EC RID: 17132 RVA: 0x001AD6D4 File Offset: 0x001AB8D4
		public void OnDeselected(Character c)
		{
			this.ClearAllSelectionsInternal(c.ID);
			this.BroadcastSelectionStatus();
		}

		// Token: 0x060042ED RID: 17133 RVA: 0x001AD6E8 File Offset: 0x001AB8E8
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

		// Token: 0x060042EE RID: 17134 RVA: 0x001AD8A0 File Offset: 0x001ABAA0
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

		// Token: 0x060042EF RID: 17135 RVA: 0x001AD924 File Offset: 0x001ABB24
		private void AddLabelInternal(ushort id, Color color, Vector2 pos, NetLimitedString header, NetLimitedString body)
		{
			CircuitBoxLabelNode newLabel = new CircuitBoxLabelNode(id, color, pos, this);
			newLabel.EditText(header, body);
			this.Labels.Add(newLabel);
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x060042F0 RID: 17136 RVA: 0x001AD958 File Offset: 0x001ABB58
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

		// Token: 0x060042F1 RID: 17137 RVA: 0x001AD9B0 File Offset: 0x001ABBB0
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

		// Token: 0x060042F2 RID: 17138 RVA: 0x001ADA24 File Offset: 0x001ABC24
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

		// Token: 0x060042F3 RID: 17139 RVA: 0x001ADA88 File Offset: 0x001ABC88
		private static bool IsExternalConnection(CircuitBoxConnection conn)
		{
			return conn is CircuitBoxInputConnection || conn is CircuitBoxOutputConnection;
		}

		// Token: 0x060042F4 RID: 17140 RVA: 0x001ADAAC File Offset: 0x001ABCAC
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

		// Token: 0x060042F5 RID: 17141 RVA: 0x001ADB18 File Offset: 0x001ABD18
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

		// Token: 0x060042F6 RID: 17142 RVA: 0x001ADBA7 File Offset: 0x001ABDA7
		private void CreateWireWithItem(CircuitBoxConnection one, CircuitBoxConnection two, ushort wireId, Item it)
		{
			if (CircuitBox.IsExternalConnection(one) || CircuitBox.IsExternalConnection(two))
			{
				DebugConsole.ThrowError("Cannot add a wire between an external connection and a component connection.", null, null, false, false);
				return;
			}
			this.AddWireDirect(wireId, it.Prefab, Option.Some<Item>(it), one, two);
		}

		// Token: 0x060042F7 RID: 17143 RVA: 0x001ADBDF File Offset: 0x001ABDDF
		private void AddWireDirect(ushort id, ItemPrefab prefab, [Nullable(new byte[]
		{
			0,
			1
		})] Option<Item> backingItem, CircuitBoxConnection one, CircuitBoxConnection two)
		{
			this.Wires.Add(new CircuitBoxWire(this, id, backingItem, one, two, prefab));
		}

		// Token: 0x060042F8 RID: 17144 RVA: 0x001ADBFC File Offset: 0x001ABDFC
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

		// Token: 0x060042F9 RID: 17145 RVA: 0x001ADC64 File Offset: 0x001ABE64
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

		// Token: 0x060042FA RID: 17146 RVA: 0x001ADD13 File Offset: 0x001ABF13
		private void AddComponentInternalUnsafe(ushort id, Item backingItem, ItemPrefab usedResource, Vector2 pos)
		{
			this.Components.Add(new CircuitBoxComponent(id, backingItem, pos, this, usedResource));
			this.OnViewUpdateProjSpecific();
		}

		// Token: 0x060042FB RID: 17147 RVA: 0x001ADD34 File Offset: 0x001ABF34
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

		// Token: 0x060042FC RID: 17148 RVA: 0x001ADD94 File Offset: 0x001ABF94
		private void ClearAllSelectionsInternal(ushort characterId)
		{
			CircuitBox.ClearSelectionFor(characterId, this.Components);
			CircuitBox.ClearSelectionFor(characterId, this.InputOutputNodes);
			CircuitBox.ClearSelectionFor(characterId, this.Wires);
			CircuitBox.ClearSelectionFor(characterId, this.Labels);
		}

		// Token: 0x060042FD RID: 17149 RVA: 0x001ADDC8 File Offset: 0x001ABFC8
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

		// Token: 0x060042FE RID: 17150 RVA: 0x001ADE48 File Offset: 0x001AC048
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

		// Token: 0x060042FF RID: 17151 RVA: 0x001ADEC8 File Offset: 0x001AC0C8
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

		// Token: 0x06004300 RID: 17152 RVA: 0x001AE054 File Offset: 0x001AC254
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

		// Token: 0x06004301 RID: 17153 RVA: 0x001AE0CC File Offset: 0x001AC2CC
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

		// Token: 0x06004302 RID: 17154 RVA: 0x001AE144 File Offset: 0x001AC344
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

		// Token: 0x06004303 RID: 17155 RVA: 0x001AE200 File Offset: 0x001AC400
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

		// Token: 0x06004304 RID: 17156 RVA: 0x001AE250 File Offset: 0x001AC450
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

		// Token: 0x06004305 RID: 17157 RVA: 0x001AE2AC File Offset: 0x001AC4AC
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

		// Token: 0x06004306 RID: 17158 RVA: 0x001AE38C File Offset: 0x001AC58C
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

		// Token: 0x06004307 RID: 17159 RVA: 0x001AE4E8 File Offset: 0x001AC6E8
		public override bool Select(Character character)
		{
			Holdable component = this.item.GetComponent<Holdable>();
			return (component == null || component.Attached) && base.Select(character);
		}

		// Token: 0x06004308 RID: 17160 RVA: 0x001AE515 File Offset: 0x001AC715
		public void OnViewUpdateProjSpecific()
		{
		}

		// Token: 0x06004309 RID: 17161 RVA: 0x001AE518 File Offset: 0x001AC718
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

		// Token: 0x0600430A RID: 17162 RVA: 0x001AE558 File Offset: 0x001AC758
		public static bool IsRoundRunning()
		{
			if (!Submarine.Unloading)
			{
				GameSession gameSession = GameMain.GameSession;
				return gameSession != null && gameSession.IsRunning;
			}
			return false;
		}

		// Token: 0x0600430B RID: 17163 RVA: 0x001AE580 File Offset: 0x001AC780
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

		// Token: 0x0600430C RID: 17164 RVA: 0x001AE5F8 File Offset: 0x001AC7F8
		[NullableContext(2)]
		private ItemContainer GetContainerOrNull(int index)
		{
			if (index < 0 || index >= this.containers.Length)
			{
				return null;
			}
			return this.containers[index];
		}

		// Token: 0x0600430D RID: 17165 RVA: 0x001AE614 File Offset: 0x001AC814
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

		// Token: 0x0600430E RID: 17166 RVA: 0x001AE714 File Offset: 0x001AC914
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
					if ((predicate = CircuitBox.<>O.<4>__CanItemBeAccessed) == null)
					{
						predicate = (CircuitBox.<>O.<4>__CanItemBeAccessed = new Func<Item, bool>(CircuitBox.CanItemBeAccessed));
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

		// Token: 0x0600430F RID: 17167 RVA: 0x001AE7AC File Offset: 0x001AC9AC
		public static bool CanItemBeAccessed(Item item)
		{
			Inventory parentInventory = item.ParentInventory;
			ItemInventory ii = parentInventory as ItemInventory;
			return ii == null || ii.Container.DrawInventory;
		}

		// Token: 0x06004310 RID: 17168 RVA: 0x001AE7DB File Offset: 0x001AC9DB
		public bool IsLocked()
		{
			return this.Locked || this.TemporarilyLocked;
		}

		// Token: 0x06004311 RID: 17169 RVA: 0x001AE7F0 File Offset: 0x001AC9F0
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

		// Token: 0x06004312 RID: 17170 RVA: 0x001AE81C File Offset: 0x001ACA1C
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

		// Token: 0x06004313 RID: 17171 RVA: 0x001AE878 File Offset: 0x001ACA78
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
				CircuitBox.<SpawnItem>g__AssignWifiComponentTeam|93_1(forceSpawnedItem, user);
				return;
			}
			EntitySpawner spawner = Entity.Spawner;
			if (spawner == null)
			{
				return;
			}
			spawner.AddItemToSpawnQueue(prefab, container.Inventory, null, null, delegate(Item it)
			{
				CircuitBox.<SpawnItem>g__AssignWifiComponentTeam|93_1(it, user);
				onSpawned(it);
			}, true, false, InvSlotType.None);
		}

		// Token: 0x06004314 RID: 17172 RVA: 0x001AE91F File Offset: 0x001ACB1F
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

		// Token: 0x06004315 RID: 17173 RVA: 0x001AE940 File Offset: 0x001ACB40
		public static bool IsInGame()
		{
			Screen selected = Screen.Selected;
			return selected == null || !selected.IsEditor;
		}

		// Token: 0x06004316 RID: 17174 RVA: 0x001AE962 File Offset: 0x001ACB62
		public static bool IsCircuitBoxSelected(Character character)
		{
			Item selectedItem = character.SelectedItem;
			return ((selectedItem != null) ? selectedItem.GetComponent<CircuitBox>() : null) != null;
		}

		// Token: 0x06004318 RID: 17176 RVA: 0x001AE98C File Offset: 0x001ACB8C
		[CompilerGenerated]
		private string <ServerEventRead>g__GetLogComponentName|9_0(IReadOnlyList<ushort> ids)
		{
			if (ids.Count > 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
				defaultInterpolatedStringHandler.AppendFormatted<int>(ids.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" components");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			CircuitBoxComponent circuitBoxComponent = this.Components.FirstOrDefault((CircuitBoxComponent comp) => ids.Contains(comp.ID));
			return ((circuitBoxComponent != null) ? circuitBoxComponent.Item.Name : null) ?? "[UNKNOWN]";
		}

		// Token: 0x06004319 RID: 17177 RVA: 0x001AEA18 File Offset: 0x001ACC18
		[CompilerGenerated]
		private string <ServerEventRead>g__GetLogWireName|9_1(IReadOnlyList<ushort> ids)
		{
			if (ids.Count > 1)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 1);
				defaultInterpolatedStringHandler.AppendFormatted<int>(ids.Count);
				defaultInterpolatedStringHandler.AppendLiteral(" wires");
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			CircuitBoxWire wire = this.Wires.FirstOrDefault((CircuitBoxWire w) => ids.Contains(w.ID));
			if (wire == null)
			{
				return "[UNKNOWN]";
			}
			Item backingWire;
			if (!wire.BackingWire.TryUnwrap(out backingWire))
			{
				return "a wire";
			}
			return backingWire.Name;
		}

		// Token: 0x0600431A RID: 17178 RVA: 0x001AEAAB File Offset: 0x001ACCAB
		[CompilerGenerated]
		private bool <ServerEventRead>g__CanAccessAndUnlocked|9_2(Client client)
		{
			return !this.IsLocked() && this.item.CanClientAccess(client) && this.<ServerEventRead>g__ClientHasRequiredItems|9_3(client);
		}

		// Token: 0x0600431B RID: 17179 RVA: 0x001AEACC File Offset: 0x001ACCCC
		[CompilerGenerated]
		private bool <ServerEventRead>g__ClientHasRequiredItems|9_3(Client client)
		{
			Character chara = client.Character;
			return chara != null && this.HasRequiredItems(chara, false, null);
		}

		// Token: 0x0600431C RID: 17180 RVA: 0x001AEAEE File Offset: 0x001ACCEE
		[CompilerGenerated]
		internal static CircuitBoxServerCreateComponentEvent <CreateInitializationEvent>g__EventFromComponent|10_0(CircuitBoxComponent component)
		{
			return new CircuitBoxServerCreateComponentEvent(component.Item.ID, component.UsedResource.UintIdentifier, component.ID, component.Position);
		}

		// Token: 0x0600431D RID: 17181 RVA: 0x001AEB18 File Offset: 0x001ACD18
		[CompilerGenerated]
		internal static CircuitBoxServerCreateWireEvent <CreateInitializationEvent>g__EventFromWire|10_1(CircuitBoxWire wire)
		{
			Option<ushort> backingWire = from i in wire.BackingWire
			select i.ID;
			CircuitBoxConnectorIdentifier from = CircuitBoxConnectorIdentifier.FromConnection(wire.From);
			CircuitBoxConnectorIdentifier to = CircuitBoxConnectorIdentifier.FromConnection(wire.To);
			CircuitBoxClientAddWireEvent request = new CircuitBoxClientAddWireEvent(wire.Color, from, to, wire.UsedItemPrefab.UintIdentifier);
			return new CircuitBoxServerCreateWireEvent(request, wire.ID, backingWire);
		}

		// Token: 0x0600431E RID: 17182 RVA: 0x001AEB8F File Offset: 0x001ACD8F
		[CompilerGenerated]
		internal static CircuitBoxServerAddLabelEvent <CreateInitializationEvent>g__EventFromLabel|10_2(CircuitBoxLabelNode label)
		{
			return new CircuitBoxServerAddLabelEvent(label.ID, label.Position, label.Size, label.Color, label.HeaderText, label.BodyText);
		}

		// Token: 0x0600431F RID: 17183 RVA: 0x001AEBBA File Offset: 0x001ACDBA
		[CompilerGenerated]
		internal static CircuitBoxRenameConnectionLabelsEvent <CreateInitializationEvent>g__EventFromLabelOverride|10_3(CircuitBoxInputOutputNode node)
		{
			return new CircuitBoxRenameConnectionLabelsEvent(node.NodeType, node.ConnectionLabelOverrides.ToNetDictionary<string, string>());
		}

		// Token: 0x06004320 RID: 17184 RVA: 0x001AEBD4 File Offset: 0x001ACDD4
		[CompilerGenerated]
		private void <LoadFromXML>g__LoadFor|49_0(CircuitBoxInputOutputNode.Type type, ContentXElement subElement)
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

		// Token: 0x06004321 RID: 17185 RVA: 0x001AEC34 File Offset: 0x001ACE34
		[CompilerGenerated]
		internal static void <SpawnItem>g__AssignWifiComponentTeam|93_1(Item item, [Nullable(2)] Character user)
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

		// Token: 0x04002018 RID: 8216
		private bool needsServerInitialization;

		// Token: 0x04002019 RID: 8217
		public static readonly ImmutableHashSet<CircuitBoxOpcode> UnrealiableOpcodes = ImmutableHashSet.Create<CircuitBoxOpcode>(CircuitBoxOpcode.Cursor);

		// Token: 0x0400201A RID: 8218
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<CircuitBoxInputConnection> Inputs;

		// Token: 0x0400201B RID: 8219
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public ImmutableArray<CircuitBoxOutputConnection> Outputs;

		// Token: 0x0400201C RID: 8220
		public readonly List<CircuitBoxComponent> Components = new List<CircuitBoxComponent>();

		// Token: 0x0400201D RID: 8221
		public readonly List<CircuitBoxInputOutputNode> InputOutputNodes = new List<CircuitBoxInputOutputNode>();

		// Token: 0x0400201E RID: 8222
		public readonly List<CircuitBoxLabelNode> Labels = new List<CircuitBoxLabelNode>();

		// Token: 0x0400201F RID: 8223
		public readonly List<CircuitBoxWire> Wires = new List<CircuitBoxWire>();

		// Token: 0x04002020 RID: 8224
		public readonly ItemContainer[] containers;

		// Token: 0x04002021 RID: 8225
		private const int ComponentContainerIndex = 0;

		// Token: 0x04002022 RID: 8226
		private const int WireContainerIndex = 1;

		// Token: 0x04002023 RID: 8227
		public bool TemporarilyLocked;

		// Token: 0x04002025 RID: 8229
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private Option<ContentXElement> delayedElementToLoad;

		// Token: 0x02000DD4 RID: 3540
		[NullableContext(0)]
		public struct CreatedWire : IEquatable<CircuitBox.CreatedWire>
		{
			// Token: 0x0600686F RID: 26735 RVA: 0x00222C82 File Offset: 0x00220E82
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

			// Token: 0x17001678 RID: 5752
			// (get) Token: 0x06006870 RID: 26736 RVA: 0x00222CA1 File Offset: 0x00220EA1
			// (set) Token: 0x06006871 RID: 26737 RVA: 0x00222CA9 File Offset: 0x00220EA9
			public CircuitBoxConnectorIdentifier Start { readonly get; set; }

			// Token: 0x17001679 RID: 5753
			// (get) Token: 0x06006872 RID: 26738 RVA: 0x00222CB2 File Offset: 0x00220EB2
			// (set) Token: 0x06006873 RID: 26739 RVA: 0x00222CBA File Offset: 0x00220EBA
			public CircuitBoxConnectorIdentifier End { readonly get; set; }

			// Token: 0x1700167A RID: 5754
			// (get) Token: 0x06006874 RID: 26740 RVA: 0x00222CC3 File Offset: 0x00220EC3
			// (set) Token: 0x06006875 RID: 26741 RVA: 0x00222CCB File Offset: 0x00220ECB
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

			// Token: 0x1700167B RID: 5755
			// (get) Token: 0x06006876 RID: 26742 RVA: 0x00222CD4 File Offset: 0x00220ED4
			// (set) Token: 0x06006877 RID: 26743 RVA: 0x00222CDC File Offset: 0x00220EDC
			public ushort ID { readonly get; set; }

			// Token: 0x06006878 RID: 26744 RVA: 0x00222CE8 File Offset: 0x00220EE8
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

			// Token: 0x06006879 RID: 26745 RVA: 0x00222D34 File Offset: 0x00220F34
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

			// Token: 0x0600687A RID: 26746 RVA: 0x00222DDE File Offset: 0x00220FDE
			[CompilerGenerated]
			public static bool operator !=(CircuitBox.CreatedWire left, CircuitBox.CreatedWire right)
			{
				return !(left == right);
			}

			// Token: 0x0600687B RID: 26747 RVA: 0x00222DEA File Offset: 0x00220FEA
			[CompilerGenerated]
			public static bool operator ==(CircuitBox.CreatedWire left, CircuitBox.CreatedWire right)
			{
				return left.Equals(right);
			}

			// Token: 0x0600687C RID: 26748 RVA: 0x00222DF4 File Offset: 0x00220FF4
			[CompilerGenerated]
			public override readonly int GetHashCode()
			{
				return ((EqualityComparer<CircuitBoxConnectorIdentifier>.Default.GetHashCode(this.<Start>k__BackingField) * -1521134295 + EqualityComparer<CircuitBoxConnectorIdentifier>.Default.GetHashCode(this.<End>k__BackingField)) * -1521134295 + EqualityComparer<Option<Barotrauma.Item>>.Default.GetHashCode(this.<Item>k__BackingField)) * -1521134295 + EqualityComparer<ushort>.Default.GetHashCode(this.<ID>k__BackingField);
			}

			// Token: 0x0600687D RID: 26749 RVA: 0x00222E56 File Offset: 0x00221056
			[CompilerGenerated]
			public override readonly bool Equals(object obj)
			{
				return obj is CircuitBox.CreatedWire && this.Equals((CircuitBox.CreatedWire)obj);
			}

			// Token: 0x0600687E RID: 26750 RVA: 0x00222E70 File Offset: 0x00221070
			[CompilerGenerated]
			public readonly bool Equals(CircuitBox.CreatedWire other)
			{
				return EqualityComparer<CircuitBoxConnectorIdentifier>.Default.Equals(this.<Start>k__BackingField, other.<Start>k__BackingField) && EqualityComparer<CircuitBoxConnectorIdentifier>.Default.Equals(this.<End>k__BackingField, other.<End>k__BackingField) && EqualityComparer<Option<Barotrauma.Item>>.Default.Equals(this.<Item>k__BackingField, other.<Item>k__BackingField) && EqualityComparer<ushort>.Default.Equals(this.<ID>k__BackingField, other.<ID>k__BackingField);
			}

			// Token: 0x0600687F RID: 26751 RVA: 0x00222EDD File Offset: 0x002210DD
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

		// Token: 0x02000DD5 RID: 3541
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040040D7 RID: 16599
			[Nullable(0)]
			public static Func<CircuitBoxComponent, CircuitBoxServerCreateComponentEvent> <0>__EventFromComponent;

			// Token: 0x040040D8 RID: 16600
			[Nullable(0)]
			public static Func<CircuitBoxWire, CircuitBoxServerCreateWireEvent> <1>__EventFromWire;

			// Token: 0x040040D9 RID: 16601
			[Nullable(0)]
			public static Func<CircuitBoxLabelNode, CircuitBoxServerAddLabelEvent> <2>__EventFromLabel;

			// Token: 0x040040DA RID: 16602
			[Nullable(0)]
			public static Func<CircuitBoxInputOutputNode, CircuitBoxRenameConnectionLabelsEvent> <3>__EventFromLabelOverride;

			// Token: 0x040040DB RID: 16603
			[Nullable(0)]
			public static Func<Item, bool> <4>__CanItemBeAccessed;
		}
	}
}
