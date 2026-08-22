using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000462 RID: 1122
	internal class OrderChatMessage : ChatMessage
	{
		// Token: 0x06004BAC RID: 19372 RVA: 0x0029C068 File Offset: 0x0029A268
		public override void ClientWrite(in SegmentTableWriter<ClientNetSegment> segmentTableWriter, IWriteMessage msg)
		{
			segmentTableWriter.StartNewSegment(ClientNetSegment.ChatMessage);
			msg.WriteUInt16(base.NetStateID);
			msg.WriteRangedInteger(8, 0, Enum.GetValues(typeof(ChatMessageType)).Length - 1);
			msg.WriteRangedInteger(0, 0, Enum.GetValues(typeof(ChatMode)).Length - 1);
			this.WriteOrder(msg);
		}

		// Token: 0x1700134A RID: 4938
		// (get) Token: 0x06004BAD RID: 19373 RVA: 0x0029C0CB File Offset: 0x0029A2CB
		public ISpatialEntity TargetEntity
		{
			get
			{
				return this.Order.TargetSpatialEntity;
			}
		}

		// Token: 0x1700134B RID: 4939
		// (get) Token: 0x06004BAE RID: 19374 RVA: 0x0029C0D8 File Offset: 0x0029A2D8
		public Identifier OrderOption
		{
			get
			{
				return this.Order.Option;
			}
		}

		// Token: 0x1700134C RID: 4940
		// (get) Token: 0x06004BAF RID: 19375 RVA: 0x0029C0E5 File Offset: 0x0029A2E5
		public int OrderPriority
		{
			get
			{
				return this.Order.ManualPriority;
			}
		}

		// Token: 0x1700134D RID: 4941
		// (get) Token: 0x06004BB0 RID: 19376 RVA: 0x0029C0F2 File Offset: 0x0029A2F2
		public int? WallSectionIndex
		{
			get
			{
				return this.Order.WallSectionIndex;
			}
		}

		// Token: 0x1700134E RID: 4942
		// (get) Token: 0x06004BB1 RID: 19377 RVA: 0x0029C0FF File Offset: 0x0029A2FF
		public bool IsNewOrder { get; }

		// Token: 0x06004BB2 RID: 19378 RVA: 0x0029C108 File Offset: 0x0029A308
		public OrderChatMessage(Order order, Character targetCharacter, Character sender, bool isNewOrder = true)
		{
			string text;
			if (order == null)
			{
				text = null;
			}
			else
			{
				string targetCharacterName = (targetCharacter != null) ? targetCharacter.DisplayName : null;
				Hull hull = (order.TargetEntity as Hull) ?? ((sender != null) ? sender.CurrentHull : null);
				string targetRoomName;
				if (hull == null)
				{
					targetRoomName = null;
				}
				else
				{
					LocalizedString displayName = hull.DisplayName;
					targetRoomName = ((displayName != null) ? displayName.Value : null);
				}
				text = order.GetChatMessage(targetCharacterName, targetRoomName, targetCharacter == sender, order.Option, isNewOrder);
			}
			this..ctor(order, text, targetCharacter, sender, isNewOrder);
		}

		// Token: 0x06004BB3 RID: 19379 RVA: 0x0029C178 File Offset: 0x0029A378
		public OrderChatMessage(Order order, string text, Character targetCharacter, Entity sender, bool isNewOrder = true) : base(OrderChatMessage.NameFromEntityOrNull(sender), text, ChatMessageType.Order, sender, GameMain.NetworkMember.ConnectedClients.Find((Client c) => c.Character == sender), PlayerConnectionChangeType.None, null)
		{
			this.Order = order;
			this.TargetCharacter = targetCharacter;
			this.IsNewOrder = isNewOrder;
		}

		// Token: 0x06004BB4 RID: 19380 RVA: 0x0029C1E8 File Offset: 0x0029A3E8
		public static string NameFromEntityOrNull(Entity entity)
		{
			string result;
			if (entity != null)
			{
				Character character = entity as Character;
				if (character == null)
				{
					Item it = entity as Item;
					if (it == null)
					{
						throw new ArgumentException("Entity is not a character or item", "entity");
					}
					result = it.Name;
				}
				else
				{
					result = character.DisplayName;
				}
			}
			else
			{
				result = null;
			}
			return result;
		}

		// Token: 0x06004BB5 RID: 19381 RVA: 0x0029C238 File Offset: 0x0029A438
		public static void WriteOrder(IWriteMessage msg, Order order, Entity targetCharacter, bool isNewOrder)
		{
			msg.WriteIdentifier(order.Prefab.Identifier);
			msg.WriteUInt16((targetCharacter == null) ? 0 : targetCharacter.ID);
			msg.WriteUInt16((order.TargetSpatialEntity is Entity) ? order.TargetEntity.ID : 0);
			if (!order.IsDismissal)
			{
				msg.WriteByte((byte)order.Options.IndexOf(order.Option));
			}
			else if (order.Option != Identifier.Empty)
			{
				msg.WriteBoolean(true);
				string[] dismissedOrder = order.Option.Value.Split('.', StringSplitOptions.None);
				msg.WriteByte((byte)dismissedOrder.Length);
				if (dismissedOrder.Length != 0)
				{
					Identifier dismissedOrderIdentifier = dismissedOrder[0].ToIdentifier();
					OrderPrefab orderPrefab = OrderPrefab.Prefabs[dismissedOrderIdentifier];
					msg.WriteIdentifier(dismissedOrderIdentifier);
					if (dismissedOrder.Length > 1)
					{
						Identifier dismissedOrderOption = dismissedOrder[1].ToIdentifier();
						msg.WriteByte((byte)orderPrefab.Options.IndexOf(dismissedOrderOption));
					}
				}
			}
			else
			{
				msg.WriteBoolean(false);
			}
			msg.WriteByte((byte)order.ManualPriority);
			msg.WriteByte((byte)order.TargetType);
			if (order.TargetType == Order.OrderTargetType.Position)
			{
				OrderTarget orderTarget = order.TargetSpatialEntity as OrderTarget;
				if (orderTarget != null)
				{
					msg.WriteBoolean(true);
					msg.WriteSingle(orderTarget.Position.X);
					msg.WriteSingle(orderTarget.Position.Y);
					msg.WriteUInt16((orderTarget.Hull == null) ? 0 : orderTarget.Hull.ID);
					goto IL_188;
				}
			}
			msg.WriteBoolean(false);
			if (order.TargetType == Order.OrderTargetType.WallSection)
			{
				msg.WriteByte((byte)order.WallSectionIndex.GetValueOrDefault());
			}
			IL_188:
			msg.WriteBoolean(isNewOrder);
		}

		// Token: 0x06004BB6 RID: 19382 RVA: 0x0029C3D4 File Offset: 0x0029A5D4
		private void WriteOrder(IWriteMessage msg)
		{
			OrderChatMessage.WriteOrder(msg, this.Order, this.TargetCharacter, this.IsNewOrder);
		}

		// Token: 0x06004BB7 RID: 19383 RVA: 0x0029C3F0 File Offset: 0x0029A5F0
		public static OrderChatMessage.OrderMessageInfo ReadOrder(IReadMessage msg)
		{
			Identifier orderIdentifier = msg.ReadIdentifier();
			ushort targetCharacterId = msg.ReadUInt16();
			Character targetCharacter = (targetCharacterId != 0) ? (Entity.FindEntityByID(targetCharacterId) as Character) : null;
			ushort targetEntityId = msg.ReadUInt16();
			Entity targetEntity = (targetEntityId != 0) ? Entity.FindEntityByID(targetEntityId) : null;
			int? optionIndex = null;
			Identifier orderOption = Identifier.Empty;
			if (orderIdentifier != Identifier.Empty)
			{
				OrderPrefab orderPrefab = OrderPrefab.Prefabs[orderIdentifier];
				if (!orderPrefab.IsDismissal)
				{
					optionIndex = new int?((int)msg.ReadByte());
				}
				else if (msg.ReadBoolean())
				{
					int identifierCount = (int)msg.ReadByte();
					if (identifierCount > 0)
					{
						Identifier dismissedOrderIdentifier = msg.ReadIdentifier();
						OrderPrefab dismissedOrderPrefab = null;
						if (dismissedOrderIdentifier != Identifier.Empty)
						{
							dismissedOrderPrefab = OrderPrefab.Prefabs[dismissedOrderIdentifier];
							orderOption = dismissedOrderPrefab.Identifier;
						}
						if (identifierCount > 1)
						{
							int dismissedOrderOptionIndex = (int)msg.ReadByte();
							if (dismissedOrderPrefab != null)
							{
								ImmutableArray<Identifier> options = dismissedOrderPrefab.Options;
								if (new ImmutableArray<Identifier>?(options) != null && dismissedOrderOptionIndex >= 0 && dismissedOrderOptionIndex < options.Length)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
									defaultInterpolatedStringHandler.AppendFormatted(orderOption.Value);
									defaultInterpolatedStringHandler.AppendLiteral(".");
									defaultInterpolatedStringHandler.AppendFormatted<Identifier>(options[dismissedOrderOptionIndex]);
									orderOption = defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier();
								}
							}
						}
					}
				}
			}
			else
			{
				optionIndex = new int?((int)msg.ReadByte());
			}
			int orderPriority = (int)msg.ReadByte();
			OrderTarget orderTargetPosition = null;
			Order.OrderTargetType orderTargetType = (Order.OrderTargetType)msg.ReadByte();
			int? wallSectionIndex = null;
			if (msg.ReadBoolean())
			{
				float x = msg.ReadSingle();
				float y = msg.ReadSingle();
				ushort hullId = msg.ReadUInt16();
				Hull hull = (hullId != 0) ? (Entity.FindEntityByID(hullId) as Hull) : null;
				orderTargetPosition = new OrderTarget(new Vector2(x, y), hull, true);
			}
			else if (orderTargetType == Order.OrderTargetType.WallSection)
			{
				wallSectionIndex = new int?((int)msg.ReadByte());
			}
			bool isNewOrder = msg.ReadBoolean();
			return new OrderChatMessage.OrderMessageInfo(orderIdentifier, orderOption, optionIndex, targetCharacter, orderTargetType, targetEntity, orderTargetPosition, wallSectionIndex, orderPriority, isNewOrder);
		}

		// Token: 0x0400279C RID: 10140
		public readonly Order Order;

		// Token: 0x0400279D RID: 10141
		public readonly Character TargetCharacter;

		// Token: 0x020011EB RID: 4587
		public readonly struct OrderMessageInfo
		{
			// Token: 0x17001CCB RID: 7371
			// (get) Token: 0x0600927E RID: 37502 RVA: 0x003C951F File Offset: 0x003C771F
			public Identifier OrderIdentifier { get; }

			// Token: 0x17001CCC RID: 7372
			// (get) Token: 0x0600927F RID: 37503 RVA: 0x003C9527 File Offset: 0x003C7727
			public OrderPrefab OrderPrefab
			{
				get
				{
					return OrderPrefab.Prefabs[this.OrderIdentifier];
				}
			}

			// Token: 0x17001CCD RID: 7373
			// (get) Token: 0x06009280 RID: 37504 RVA: 0x003C9539 File Offset: 0x003C7739
			public Identifier OrderOption { get; }

			// Token: 0x17001CCE RID: 7374
			// (get) Token: 0x06009281 RID: 37505 RVA: 0x003C9541 File Offset: 0x003C7741
			public int? OrderOptionIndex { get; }

			// Token: 0x17001CCF RID: 7375
			// (get) Token: 0x06009282 RID: 37506 RVA: 0x003C9549 File Offset: 0x003C7749
			public Character TargetCharacter { get; }

			// Token: 0x17001CD0 RID: 7376
			// (get) Token: 0x06009283 RID: 37507 RVA: 0x003C9551 File Offset: 0x003C7751
			public Order.OrderTargetType TargetType { get; }

			// Token: 0x17001CD1 RID: 7377
			// (get) Token: 0x06009284 RID: 37508 RVA: 0x003C9559 File Offset: 0x003C7759
			public Entity TargetEntity { get; }

			// Token: 0x17001CD2 RID: 7378
			// (get) Token: 0x06009285 RID: 37509 RVA: 0x003C9561 File Offset: 0x003C7761
			public OrderTarget TargetPosition { get; }

			// Token: 0x17001CD3 RID: 7379
			// (get) Token: 0x06009286 RID: 37510 RVA: 0x003C9569 File Offset: 0x003C7769
			public int? WallSectionIndex { get; }

			// Token: 0x17001CD4 RID: 7380
			// (get) Token: 0x06009287 RID: 37511 RVA: 0x003C9571 File Offset: 0x003C7771
			public int Priority { get; }

			// Token: 0x17001CD5 RID: 7381
			// (get) Token: 0x06009288 RID: 37512 RVA: 0x003C9579 File Offset: 0x003C7779
			public bool IsNewOrder { get; }

			// Token: 0x06009289 RID: 37513 RVA: 0x003C9584 File Offset: 0x003C7784
			public OrderMessageInfo(Identifier orderIdentifier, Identifier orderOption, int? orderOptionIndex, Character targetCharacter, Order.OrderTargetType targetType, Entity targetEntity, OrderTarget targetPosition, int? wallSectionIndex, int orderPriority, bool isNewOrder)
			{
				this.OrderIdentifier = orderIdentifier;
				this.OrderOption = orderOption;
				this.OrderOptionIndex = orderOptionIndex;
				this.TargetCharacter = targetCharacter;
				this.TargetType = targetType;
				this.TargetEntity = targetEntity;
				this.TargetPosition = targetPosition;
				this.WallSectionIndex = wallSectionIndex;
				this.Priority = orderPriority;
				this.IsNewOrder = isNewOrder;
			}
		}
	}
}
