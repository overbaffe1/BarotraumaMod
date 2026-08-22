using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000371 RID: 881
	internal class OrderChatMessage : ChatMessage
	{
		// Token: 0x0600349A RID: 13466 RVA: 0x00169E98 File Offset: 0x00168098
		public override void ServerWrite(in SegmentTableWriter<ServerNetSegment> segmentTable, IWriteMessage msg, Client c)
		{
			segmentTable.StartNewSegment(ServerNetSegment.ChatMessage);
			msg.WriteUInt16(base.NetStateID);
			msg.WriteRangedInteger(8, 0, Enum.GetValues(typeof(ChatMessageType)).Length - 1);
			msg.WriteString(this.Text);
			msg.WriteString(this.SenderName);
			msg.WriteBoolean(this.SenderClient != null);
			if (this.SenderClient != null)
			{
				AccountId accountId;
				msg.WriteString(this.SenderClient.AccountId.TryUnwrap(out accountId) ? accountId.StringRepresentation : this.SenderClient.SessionId.ToString());
			}
			msg.WriteBoolean(this.Sender != null && c.InGame);
			if (this.Sender != null && c.InGame)
			{
				msg.WriteUInt16(this.Sender.ID);
			}
			msg.WriteBoolean(false);
			msg.WritePadBits();
			this.WriteOrder(msg);
		}

		// Token: 0x17000E8A RID: 3722
		// (get) Token: 0x0600349B RID: 13467 RVA: 0x00169F86 File Offset: 0x00168186
		public ISpatialEntity TargetEntity
		{
			get
			{
				return this.Order.TargetSpatialEntity;
			}
		}

		// Token: 0x17000E8B RID: 3723
		// (get) Token: 0x0600349C RID: 13468 RVA: 0x00169F93 File Offset: 0x00168193
		public Identifier OrderOption
		{
			get
			{
				return this.Order.Option;
			}
		}

		// Token: 0x17000E8C RID: 3724
		// (get) Token: 0x0600349D RID: 13469 RVA: 0x00169FA0 File Offset: 0x001681A0
		public int OrderPriority
		{
			get
			{
				return this.Order.ManualPriority;
			}
		}

		// Token: 0x17000E8D RID: 3725
		// (get) Token: 0x0600349E RID: 13470 RVA: 0x00169FAD File Offset: 0x001681AD
		public int? WallSectionIndex
		{
			get
			{
				return this.Order.WallSectionIndex;
			}
		}

		// Token: 0x17000E8E RID: 3726
		// (get) Token: 0x0600349F RID: 13471 RVA: 0x00169FBA File Offset: 0x001681BA
		public bool IsNewOrder { get; }

		// Token: 0x060034A0 RID: 13472 RVA: 0x00169FC4 File Offset: 0x001681C4
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

		// Token: 0x060034A1 RID: 13473 RVA: 0x0016A034 File Offset: 0x00168234
		public OrderChatMessage(Order order, string text, Character targetCharacter, Entity sender, bool isNewOrder = true) : base(OrderChatMessage.NameFromEntityOrNull(sender), text, ChatMessageType.Order, sender, GameMain.NetworkMember.ConnectedClients.Find((Client c) => c.Character == sender), PlayerConnectionChangeType.None, null)
		{
			this.Order = order;
			this.TargetCharacter = targetCharacter;
			this.IsNewOrder = isNewOrder;
		}

		// Token: 0x060034A2 RID: 13474 RVA: 0x0016A0A4 File Offset: 0x001682A4
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

		// Token: 0x060034A3 RID: 13475 RVA: 0x0016A0F4 File Offset: 0x001682F4
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

		// Token: 0x060034A4 RID: 13476 RVA: 0x0016A290 File Offset: 0x00168490
		private void WriteOrder(IWriteMessage msg)
		{
			OrderChatMessage.WriteOrder(msg, this.Order, this.TargetCharacter, this.IsNewOrder);
		}

		// Token: 0x060034A5 RID: 13477 RVA: 0x0016A2AC File Offset: 0x001684AC
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

		// Token: 0x04001A2B RID: 6699
		public readonly Order Order;

		// Token: 0x04001A2C RID: 6700
		public readonly Character TargetCharacter;

		// Token: 0x02000C02 RID: 3074
		public readonly struct OrderMessageInfo
		{
			// Token: 0x1700160F RID: 5647
			// (get) Token: 0x060062B6 RID: 25270 RVA: 0x0021062E File Offset: 0x0020E82E
			public Identifier OrderIdentifier { get; }

			// Token: 0x17001610 RID: 5648
			// (get) Token: 0x060062B7 RID: 25271 RVA: 0x00210636 File Offset: 0x0020E836
			public OrderPrefab OrderPrefab
			{
				get
				{
					return OrderPrefab.Prefabs[this.OrderIdentifier];
				}
			}

			// Token: 0x17001611 RID: 5649
			// (get) Token: 0x060062B8 RID: 25272 RVA: 0x00210648 File Offset: 0x0020E848
			public Identifier OrderOption { get; }

			// Token: 0x17001612 RID: 5650
			// (get) Token: 0x060062B9 RID: 25273 RVA: 0x00210650 File Offset: 0x0020E850
			public int? OrderOptionIndex { get; }

			// Token: 0x17001613 RID: 5651
			// (get) Token: 0x060062BA RID: 25274 RVA: 0x00210658 File Offset: 0x0020E858
			public Character TargetCharacter { get; }

			// Token: 0x17001614 RID: 5652
			// (get) Token: 0x060062BB RID: 25275 RVA: 0x00210660 File Offset: 0x0020E860
			public Order.OrderTargetType TargetType { get; }

			// Token: 0x17001615 RID: 5653
			// (get) Token: 0x060062BC RID: 25276 RVA: 0x00210668 File Offset: 0x0020E868
			public Entity TargetEntity { get; }

			// Token: 0x17001616 RID: 5654
			// (get) Token: 0x060062BD RID: 25277 RVA: 0x00210670 File Offset: 0x0020E870
			public OrderTarget TargetPosition { get; }

			// Token: 0x17001617 RID: 5655
			// (get) Token: 0x060062BE RID: 25278 RVA: 0x00210678 File Offset: 0x0020E878
			public int? WallSectionIndex { get; }

			// Token: 0x17001618 RID: 5656
			// (get) Token: 0x060062BF RID: 25279 RVA: 0x00210680 File Offset: 0x0020E880
			public int Priority { get; }

			// Token: 0x17001619 RID: 5657
			// (get) Token: 0x060062C0 RID: 25280 RVA: 0x00210688 File Offset: 0x0020E888
			public bool IsNewOrder { get; }

			// Token: 0x060062C1 RID: 25281 RVA: 0x00210690 File Offset: 0x0020E890
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
