using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200008B RID: 139
	internal class Order
	{
		// Token: 0x170004EF RID: 1263
		// (get) Token: 0x060011F9 RID: 4601 RVA: 0x000A1064 File Offset: 0x0009F264
		public bool IsCurrentOrder
		{
			get
			{
				return this.Type == Order.OrderType.Current;
			}
		}

		// Token: 0x170004F0 RID: 1264
		// (get) Token: 0x060011FA RID: 4602 RVA: 0x000A106F File Offset: 0x0009F26F
		public bool IsDismissal
		{
			get
			{
				return this.Prefab.IsDismissal;
			}
		}

		// Token: 0x170004F1 RID: 1265
		// (get) Token: 0x060011FB RID: 4603 RVA: 0x000A107C File Offset: 0x0009F27C
		public ISpatialEntity TargetSpatialEntity
		{
			get
			{
				if (this.targetSpatialEntity == null)
				{
					if (this.TargetType == Order.OrderTargetType.WallSection && this.WallSectionIndex != null)
					{
						Structure structure = this.TargetEntity as Structure;
						this.targetSpatialEntity = ((structure != null) ? structure.Sections[this.WallSectionIndex.Value] : null);
					}
					else
					{
						ISpatialEntity targetEntity = this.TargetEntity;
						this.targetSpatialEntity = (targetEntity ?? this.TargetPosition);
					}
				}
				return this.targetSpatialEntity;
			}
		}

		// Token: 0x170004F2 RID: 1266
		// (get) Token: 0x060011FC RID: 4604 RVA: 0x000A10F0 File Offset: 0x0009F2F0
		public Hull TargetHull
		{
			get
			{
				return this.TargetEntity as Hull;
			}
		}

		// Token: 0x170004F3 RID: 1267
		// (get) Token: 0x060011FD RID: 4605 RVA: 0x000A10FD File Offset: 0x0009F2FD
		public LocalizedString Name
		{
			get
			{
				return this.Prefab.Name;
			}
		}

		// Token: 0x170004F4 RID: 1268
		// (get) Token: 0x060011FE RID: 4606 RVA: 0x000A110A File Offset: 0x0009F30A
		public LocalizedString ContextualName
		{
			get
			{
				return this.Prefab.ContextualName;
			}
		}

		// Token: 0x170004F5 RID: 1269
		// (get) Token: 0x060011FF RID: 4607 RVA: 0x000A1117 File Offset: 0x0009F317
		public Identifier Identifier
		{
			get
			{
				return this.Prefab.Identifier;
			}
		}

		// Token: 0x170004F6 RID: 1270
		// (get) Token: 0x06001200 RID: 4608 RVA: 0x000A1124 File Offset: 0x0009F324
		public Type ItemComponentType
		{
			get
			{
				return this.Prefab.ItemComponentType;
			}
		}

		// Token: 0x170004F7 RID: 1271
		// (get) Token: 0x06001201 RID: 4609 RVA: 0x000A1131 File Offset: 0x0009F331
		public bool CanTypeBeSubclass
		{
			get
			{
				return this.Prefab.CanTypeBeSubclass;
			}
		}

		// Token: 0x170004F8 RID: 1272
		// (get) Token: 0x06001202 RID: 4610 RVA: 0x000A113E File Offset: 0x0009F33E
		public ref readonly ImmutableArray<Identifier> ControllerTags
		{
			get
			{
				return ref this.Prefab.ControllerTags;
			}
		}

		// Token: 0x170004F9 RID: 1273
		// (get) Token: 0x06001203 RID: 4611 RVA: 0x000A114B File Offset: 0x0009F34B
		public ref readonly ImmutableArray<Identifier> TargetItems
		{
			get
			{
				return ref this.Prefab.TargetItems;
			}
		}

		// Token: 0x170004FA RID: 1274
		// (get) Token: 0x06001204 RID: 4612 RVA: 0x000A1158 File Offset: 0x0009F358
		public ref readonly ImmutableArray<Identifier> RequireItems
		{
			get
			{
				return ref this.Prefab.RequireItems;
			}
		}

		// Token: 0x170004FB RID: 1275
		// (get) Token: 0x06001205 RID: 4613 RVA: 0x000A1165 File Offset: 0x0009F365
		public ref readonly ImmutableArray<Identifier> Options
		{
			get
			{
				return ref this.Prefab.Options;
			}
		}

		// Token: 0x170004FC RID: 1276
		// (get) Token: 0x06001206 RID: 4614 RVA: 0x000A1172 File Offset: 0x0009F372
		public ref readonly ImmutableArray<Identifier> HiddenOptions
		{
			get
			{
				return ref this.Prefab.HiddenOptions;
			}
		}

		// Token: 0x170004FD RID: 1277
		// (get) Token: 0x06001207 RID: 4615 RVA: 0x000A117F File Offset: 0x0009F37F
		public ref readonly ImmutableArray<Identifier> AllOptions
		{
			get
			{
				return ref this.Prefab.AllOptions;
			}
		}

		// Token: 0x170004FE RID: 1278
		// (get) Token: 0x06001208 RID: 4616 RVA: 0x000A118C File Offset: 0x0009F38C
		public Sprite SymbolSprite
		{
			get
			{
				return this.Prefab.SymbolSprite;
			}
		}

		// Token: 0x170004FF RID: 1279
		// (get) Token: 0x06001209 RID: 4617 RVA: 0x000A1199 File Offset: 0x0009F399
		public Color Color
		{
			get
			{
				return this.Prefab.Color;
			}
		}

		// Token: 0x17000500 RID: 1280
		// (get) Token: 0x0600120A RID: 4618 RVA: 0x000A11A6 File Offset: 0x0009F3A6
		public bool TargetAllCharacters
		{
			get
			{
				return this.Prefab.TargetAllCharacters;
			}
		}

		// Token: 0x17000501 RID: 1281
		// (get) Token: 0x0600120B RID: 4619 RVA: 0x000A11B3 File Offset: 0x0009F3B3
		public ref readonly ImmutableArray<Identifier> AppropriateJobs
		{
			get
			{
				return ref this.Prefab.AppropriateJobs;
			}
		}

		// Token: 0x17000502 RID: 1282
		// (get) Token: 0x0600120C RID: 4620 RVA: 0x000A11C0 File Offset: 0x0009F3C0
		public float FadeOutTime
		{
			get
			{
				return this.Prefab.FadeOutTime;
			}
		}

		// Token: 0x17000503 RID: 1283
		// (get) Token: 0x0600120D RID: 4621 RVA: 0x000A11CD File Offset: 0x0009F3CD
		public bool MustSetTarget
		{
			get
			{
				return this.Prefab.MustSetTarget;
			}
		}

		// Token: 0x17000504 RID: 1284
		// (get) Token: 0x0600120E RID: 4622 RVA: 0x000A11DA File Offset: 0x0009F3DA
		public Identifier AppropriateSkill
		{
			get
			{
				return this.Prefab.AppropriateSkill;
			}
		}

		// Token: 0x17000505 RID: 1285
		// (get) Token: 0x0600120F RID: 4623 RVA: 0x000A11E7 File Offset: 0x0009F3E7
		public OrderCategory? Category
		{
			get
			{
				return this.Prefab.Category;
			}
		}

		// Token: 0x17000506 RID: 1286
		// (get) Token: 0x06001210 RID: 4624 RVA: 0x000A11F4 File Offset: 0x0009F3F4
		public bool MustManuallyAssign
		{
			get
			{
				return this.Prefab.MustManuallyAssign;
			}
		}

		// Token: 0x17000507 RID: 1287
		// (get) Token: 0x06001211 RID: 4625 RVA: 0x000A1201 File Offset: 0x0009F401
		public bool IsIgnoreOrder
		{
			get
			{
				return this.Prefab.IsIgnoreOrder;
			}
		}

		// Token: 0x17000508 RID: 1288
		// (get) Token: 0x06001212 RID: 4626 RVA: 0x000A120E File Offset: 0x0009F40E
		public bool IsDeconstructOrder
		{
			get
			{
				return this.Prefab.IsDeconstructOrder;
			}
		}

		// Token: 0x17000509 RID: 1289
		// (get) Token: 0x06001213 RID: 4627 RVA: 0x000A121B File Offset: 0x0009F41B
		public bool DrawIconWhenContained
		{
			get
			{
				return this.Prefab.DrawIconWhenContained;
			}
		}

		// Token: 0x1700050A RID: 1290
		// (get) Token: 0x06001214 RID: 4628 RVA: 0x000A1228 File Offset: 0x0009F428
		public bool Hidden
		{
			get
			{
				return this.Prefab.Hidden;
			}
		}

		// Token: 0x1700050B RID: 1291
		// (get) Token: 0x06001215 RID: 4629 RVA: 0x000A1235 File Offset: 0x0009F435
		public bool IgnoreAtOutpost
		{
			get
			{
				return this.Prefab.IgnoreAtOutpost;
			}
		}

		// Token: 0x1700050C RID: 1292
		// (get) Token: 0x06001216 RID: 4630 RVA: 0x000A1242 File Offset: 0x0009F442
		public bool IsReport
		{
			get
			{
				return this.Prefab.IsReport;
			}
		}

		// Token: 0x1700050D RID: 1293
		// (get) Token: 0x06001217 RID: 4631 RVA: 0x000A124F File Offset: 0x0009F44F
		public bool AutoDismiss
		{
			get
			{
				return this.Prefab.AutoDismiss;
			}
		}

		// Token: 0x1700050E RID: 1294
		// (get) Token: 0x06001218 RID: 4632 RVA: 0x000A125C File Offset: 0x0009F45C
		public int AssignmentPriority
		{
			get
			{
				return this.Prefab.AssignmentPriority;
			}
		}

		// Token: 0x1700050F RID: 1295
		// (get) Token: 0x06001219 RID: 4633 RVA: 0x000A1269 File Offset: 0x0009F469
		public bool ColoredWhenControllingGiver
		{
			get
			{
				return this.Prefab.ColoredWhenControllingGiver;
			}
		}

		// Token: 0x17000510 RID: 1296
		// (get) Token: 0x0600121A RID: 4634 RVA: 0x000A1276 File Offset: 0x0009F476
		public bool DisplayGiverInTooltip
		{
			get
			{
				return this.Prefab.DisplayGiverInTooltip;
			}
		}

		// Token: 0x0600121B RID: 4635 RVA: 0x000A1284 File Offset: 0x0009F484
		public Order(OrderPrefab prefab, Entity targetEntity, ItemComponent targetItem, Character orderGiver = null, bool isAutonomous = false) : this(prefab, Identifier.Empty, 0, Order.OrderType.Current, null, targetEntity, targetItem, orderGiver, isAutonomous)
		{
		}

		// Token: 0x0600121C RID: 4636 RVA: 0x000A12A8 File Offset: 0x0009F4A8
		public Order(OrderPrefab prefab, Identifier option, Entity targetEntity, ItemComponent targetItem, Character orderGiver = null, bool isAutonomous = false) : this(prefab, option, 0, Order.OrderType.Current, null, targetEntity, targetItem, orderGiver, isAutonomous)
		{
		}

		// Token: 0x0600121D RID: 4637 RVA: 0x000A12C7 File Offset: 0x0009F4C7
		public Order(OrderPrefab prefab, OrderTarget target, Character orderGiver = null) : this(prefab, prefab.Options.FirstOrDefault<Identifier>(), 0, Order.OrderType.Current, null, target, orderGiver)
		{
		}

		// Token: 0x0600121E RID: 4638 RVA: 0x000A12E0 File Offset: 0x0009F4E0
		public Order(OrderPrefab prefab, Identifier option, OrderTarget target, Character orderGiver = null) : this(prefab, option, 0, Order.OrderType.Current, null, target, orderGiver)
		{
		}

		// Token: 0x0600121F RID: 4639 RVA: 0x000A12F0 File Offset: 0x0009F4F0
		public Order(OrderPrefab prefab, Structure wall, int? sectionIndex, Character orderGiver = null) : this(prefab, Identifier.Empty, 0, Order.OrderType.Current, null, wall, sectionIndex, orderGiver)
		{
		}

		// Token: 0x06001220 RID: 4640 RVA: 0x000A1310 File Offset: 0x0009F510
		public Order(OrderPrefab prefab, Identifier option, Structure wall, int? sectionIndex, Character orderGiver = null) : this(prefab, option, 0, Order.OrderType.Current, null, wall, sectionIndex, orderGiver)
		{
		}

		// Token: 0x06001221 RID: 4641 RVA: 0x000A1330 File Offset: 0x0009F530
		private unsafe Order(OrderPrefab prefab, Identifier option, int manualPriority, Order.OrderType orderType, AIObjective aiObjective, Entity targetEntity, ItemComponent targetItem, Character orderGiver = null, bool isAutonomous = false)
		{
			this.Prefab = prefab;
			this.Option = option;
			this.ManualPriority = manualPriority;
			this.Type = orderType;
			this.Objective = aiObjective;
			this.UseController = this.Prefab.UseController;
			this.OrderGiver = orderGiver;
			this.TargetEntity = targetEntity;
			if (targetItem != null)
			{
				if (this.UseController)
				{
					Item item = targetItem.Item;
					this.ConnectedController = ((item != null) ? item.FindController(new ImmutableArray<Identifier>?(*this.ControllerTags)) : null);
					if (this.ConnectedController == null)
					{
						DebugConsole.AddWarning("AI: Tried to use a controller for operating an item, but couldn't find any.", null);
						this.UseController = false;
					}
				}
				this.TargetEntity = targetItem.Item;
				this.TargetItemComponent = targetItem;
			}
			this.TargetType = Order.OrderTargetType.Entity;
		}

		// Token: 0x06001222 RID: 4642 RVA: 0x000A13F4 File Offset: 0x0009F5F4
		private Order(OrderPrefab prefab, Identifier option, int manualPriority, Order.OrderType orderType, AIObjective aiObjective, OrderTarget target, Character orderGiver = null) : this(prefab, option, manualPriority, orderType, aiObjective, null, null, orderGiver, false)
		{
			this.TargetPosition = target;
			this.TargetType = Order.OrderTargetType.Position;
		}

		// Token: 0x06001223 RID: 4643 RVA: 0x000A1424 File Offset: 0x0009F624
		private Order(OrderPrefab prefab, Identifier option, int manualPriority, Order.OrderType orderType, AIObjective aiObjective, Structure wall, int? sectionIndex, Character orderGiver = null) : this(prefab, option, manualPriority, orderType, aiObjective, wall, null, orderGiver, false)
		{
			this.WallSectionIndex = sectionIndex;
			this.TargetType = Order.OrderTargetType.WallSection;
		}

		// Token: 0x06001224 RID: 4644 RVA: 0x000A1454 File Offset: 0x0009F654
		private Order(Order other, OrderPrefab prefab = null, Identifier option = default(Identifier), int? manualPriority = null, Order.OrderType? type = null, AIObjective objective = null, Entity targetEntity = null, ItemComponent targetItemComponent = null, Controller connectedController = null, Character orderGiver = null, OrderTarget targetPosition = null, Order.OrderTargetType? targetType = null, int? wallSectionIndex = null, bool? useController = null)
		{
			this.Prefab = (prefab ?? other.Prefab);
			this.Option = option.IfEmpty(other.Option);
			this.ManualPriority = (manualPriority ?? other.ManualPriority);
			this.Type = (type ?? other.Type);
			this.Objective = (objective ?? other.Objective);
			this.TargetEntity = (targetEntity ?? other.TargetEntity);
			this.TargetItemComponent = (targetItemComponent ?? other.TargetItemComponent);
			this.ConnectedController = (connectedController ?? other.ConnectedController);
			this.OrderGiver = (orderGiver ?? other.OrderGiver);
			this.TargetPosition = (targetPosition ?? other.TargetPosition);
			this.TargetType = (targetType ?? other.TargetType);
			int? num = wallSectionIndex;
			this.WallSectionIndex = ((num != null) ? num : other.WallSectionIndex);
			this.UseController = (useController ?? other.UseController);
		}

		// Token: 0x06001225 RID: 4645 RVA: 0x000A1598 File Offset: 0x0009F798
		public Order WithOption(Identifier option)
		{
			return new Order(this, null, option, null, null, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06001226 RID: 4646 RVA: 0x000A15E0 File Offset: 0x0009F7E0
		public Order WithManualPriority(int newPriority)
		{
			OrderPrefab prefab = null;
			int? manualPriority = new int?(newPriority);
			return new Order(this, prefab, default(Identifier), manualPriority, null, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06001227 RID: 4647 RVA: 0x000A1634 File Offset: 0x0009F834
		public Order WithOrderGiver(Character orderGiver)
		{
			return new Order(this, null, default(Identifier), null, null, null, null, null, null, orderGiver, null, null, null, null);
		}

		// Token: 0x06001228 RID: 4648 RVA: 0x000A1688 File Offset: 0x0009F888
		public Order WithObjective(AIObjective objective)
		{
			return new Order(this, null, default(Identifier), null, null, objective, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06001229 RID: 4649 RVA: 0x000A16DC File Offset: 0x0009F8DC
		public Order WithTargetEntity(Entity entity)
		{
			OrderPrefab prefab = null;
			Order.OrderTargetType? targetType = new Order.OrderTargetType?(Order.OrderTargetType.Entity);
			return new Order(this, prefab, default(Identifier), null, null, null, entity, null, null, null, null, targetType, null, null);
		}

		// Token: 0x0600122A RID: 4650 RVA: 0x000A1730 File Offset: 0x0009F930
		public Order WithTargetSpatialEntity(ISpatialEntity spatialEntity)
		{
			WallSection wallSection = spatialEntity as WallSection;
			if (wallSection != null)
			{
				Structure wall = wallSection.Wall;
				int sectionIndex = wall.Sections.IndexOf(wallSection);
				return this.WithWallSection(wall, new int?(sectionIndex));
			}
			Entity entity = spatialEntity as Entity;
			if (entity != null)
			{
				return this.WithTargetEntity(entity);
			}
			OrderTarget orderTarget = spatialEntity as OrderTarget;
			if (orderTarget != null)
			{
				return this.WithTargetPosition(orderTarget);
			}
			throw new InvalidOperationException("Unexpected input type: " + spatialEntity.GetType().Name);
		}

		// Token: 0x0600122B RID: 4651 RVA: 0x000A17AC File Offset: 0x0009F9AC
		public unsafe Order WithItemComponent(Item item, ItemComponent component = null)
		{
			Controller controller = null;
			if (this.UseController)
			{
				controller = ((item != null) ? item.FindController(new ImmutableArray<Identifier>?(*this.ControllerTags)) : null);
			}
			OrderPrefab prefab = null;
			ItemComponent targetItemComponent = component ?? this.GetTargetItemComponent(item);
			Controller connectedController = controller;
			return new Order(this, prefab, default(Identifier), null, null, null, item, targetItemComponent, connectedController, null, null, null, null, null);
		}

		// Token: 0x0600122C RID: 4652 RVA: 0x000A183C File Offset: 0x0009FA3C
		public Order WithWallSection(Structure wall, int? sectionIndex)
		{
			OrderPrefab prefab = null;
			Order.OrderTargetType? targetType = new Order.OrderTargetType?(Order.OrderTargetType.WallSection);
			return new Order(this, prefab, default(Identifier), null, null, null, wall, null, null, null, null, targetType, sectionIndex, null);
		}

		// Token: 0x0600122D RID: 4653 RVA: 0x000A188C File Offset: 0x0009FA8C
		public Order WithType(Order.OrderType type)
		{
			OrderPrefab prefab = null;
			Order.OrderType? type2 = new Order.OrderType?(type);
			return new Order(this, prefab, default(Identifier), null, type2, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x0600122E RID: 4654 RVA: 0x000A18E0 File Offset: 0x0009FAE0
		public Order WithTargetPosition(OrderTarget targetPosition)
		{
			OrderPrefab prefab = null;
			Order.OrderTargetType? targetType = new Order.OrderTargetType?(Order.OrderTargetType.Position);
			return new Order(this, prefab, default(Identifier), null, null, null, null, null, null, null, targetPosition, targetType, null, null);
		}

		// Token: 0x0600122F RID: 4655 RVA: 0x000A1934 File Offset: 0x0009FB34
		public Order Clone()
		{
			return new Order(this, null, default(Identifier), null, null, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06001230 RID: 4656 RVA: 0x000A1988 File Offset: 0x0009FB88
		public Order GetDismissal()
		{
			if (this.IsDismissal)
			{
				throw new InvalidOperationException("Attempted to dismiss a dismissal order");
			}
			return new Order(this, OrderPrefab.Prefabs["dismissed"], Order.GetDismissOrderOption(this), null, null, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06001231 RID: 4657 RVA: 0x000A19F6 File Offset: 0x0009FBF6
		public bool HasAppropriateJob(Character character)
		{
			return this.Prefab.HasAppropriateJob(character);
		}

		// Token: 0x06001232 RID: 4658 RVA: 0x000A1A04 File Offset: 0x0009FC04
		public bool HasPreferredJob(Character character)
		{
			return this.Prefab.HasPreferredJob(character);
		}

		// Token: 0x06001233 RID: 4659 RVA: 0x000A1A12 File Offset: 0x0009FC12
		public string GetChatMessage(string targetCharacterName, string targetRoomName, bool givingOrderToSelf, Identifier orderOption = default(Identifier), bool isNewOrder = true)
		{
			return this.Prefab.GetChatMessage(targetCharacterName, targetRoomName, this.TargetEntity, givingOrderToSelf, orderOption, isNewOrder);
		}

		// Token: 0x06001234 RID: 4660 RVA: 0x000A1A2C File Offset: 0x0009FC2C
		public ItemComponent GetTargetItemComponent(Item item)
		{
			return this.Prefab.GetTargetItemComponent(item);
		}

		// Token: 0x06001235 RID: 4661 RVA: 0x000A1A3A File Offset: 0x0009FC3A
		public bool TryGetTargetItemComponent(Item item, out ItemComponent firstMatchingComponent)
		{
			return this.Prefab.TryGetTargetItemComponent(item, out firstMatchingComponent);
		}

		// Token: 0x06001236 RID: 4662 RVA: 0x000A1A4C File Offset: 0x0009FC4C
		public List<Item> GetMatchingItems(Submarine submarine, bool mustBelongToPlayerSub, CharacterTeamType? requiredTeam = null, Character interactableFor = null)
		{
			return this.Prefab.GetMatchingItems(submarine, mustBelongToPlayerSub, requiredTeam, interactableFor, default(Identifier));
		}

		// Token: 0x06001237 RID: 4663 RVA: 0x000A1A74 File Offset: 0x0009FC74
		public List<Item> GetMatchingItems(bool mustBelongToPlayerSub, Character interactableFor = null)
		{
			return this.Prefab.GetMatchingItems(mustBelongToPlayerSub, interactableFor, default(Identifier));
		}

		// Token: 0x06001238 RID: 4664 RVA: 0x000A1A97 File Offset: 0x0009FC97
		public LocalizedString GetOptionName(string id)
		{
			return this.Prefab.GetOptionName(id);
		}

		// Token: 0x06001239 RID: 4665 RVA: 0x000A1AA5 File Offset: 0x0009FCA5
		public LocalizedString GetOptionName(Identifier id)
		{
			return this.Prefab.GetOptionName(id);
		}

		// Token: 0x0600123A RID: 4666 RVA: 0x000A1AB3 File Offset: 0x0009FCB3
		public LocalizedString GetOptionName(int index)
		{
			return this.Prefab.GetOptionName(index);
		}

		// Token: 0x0600123B RID: 4667 RVA: 0x000A1AC1 File Offset: 0x0009FCC1
		public static Identifier GetDismissOrderOption(Order order)
		{
			return OrderPrefab.GetDismissOrderOption(order);
		}

		// Token: 0x0600123C RID: 4668 RVA: 0x000A1ACC File Offset: 0x0009FCCC
		public bool MatchesOrder(Identifier orderIdentifier, Identifier orderOption)
		{
			Identifier identifier = this.Identifier;
			return orderIdentifier == identifier && orderOption == this.Option;
		}

		// Token: 0x0600123D RID: 4669 RVA: 0x000A1AFA File Offset: 0x0009FCFA
		public bool MatchesOrder(Order order)
		{
			return order != null && this.MatchesOrder(order.Identifier, order.Option);
		}

		// Token: 0x0600123E RID: 4670 RVA: 0x000A1B14 File Offset: 0x0009FD14
		public bool MatchesDismissedOrder(Identifier dismissOrderOption)
		{
			Identifier[] dismissedOrder = (from s in dismissOrderOption.Value.Split('.', StringSplitOptions.None)
			select s.ToIdentifier()).ToArray<Identifier>();
			if (dismissedOrder != null && dismissedOrder.Length != 0)
			{
				Identifier dismissedOrderIdentifier = (dismissedOrder.Length != 0) ? dismissedOrder[0] : Identifier.Empty;
				if (!(dismissedOrderIdentifier == Identifier.Empty))
				{
					Identifier identifier = this.Identifier;
					if (!(dismissedOrderIdentifier != identifier))
					{
						Identifier dismissedOrderOption = (dismissedOrder.Length > 1) ? dismissedOrder[1] : Identifier.Empty;
						return (dismissedOrderOption == Identifier.Empty && this.Option == Identifier.Empty) || dismissedOrderOption == this.Option;
					}
				}
				return false;
			}
			return false;
		}

		// Token: 0x0600123F RID: 4671 RVA: 0x000A1BDC File Offset: 0x0009FDDC
		public ImmutableArray<Identifier> GetTargetItems(Identifier option = default(Identifier))
		{
			return this.Prefab.GetTargetItems(option);
		}

		// Token: 0x06001240 RID: 4672 RVA: 0x000A1BEC File Offset: 0x0009FDEC
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Order (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x04000892 RID: 2194
		public readonly OrderPrefab Prefab;

		// Token: 0x04000893 RID: 2195
		public readonly Identifier Option;

		// Token: 0x04000894 RID: 2196
		public readonly int ManualPriority;

		// Token: 0x04000895 RID: 2197
		public readonly Order.OrderType Type;

		// Token: 0x04000896 RID: 2198
		public readonly AIObjective Objective;

		// Token: 0x04000897 RID: 2199
		public readonly Entity TargetEntity;

		// Token: 0x04000898 RID: 2200
		public readonly ItemComponent TargetItemComponent;

		// Token: 0x04000899 RID: 2201
		public readonly Controller ConnectedController;

		// Token: 0x0400089A RID: 2202
		public readonly Character OrderGiver;

		// Token: 0x0400089B RID: 2203
		public readonly OrderTarget TargetPosition;

		// Token: 0x0400089C RID: 2204
		private ISpatialEntity targetSpatialEntity;

		// Token: 0x0400089D RID: 2205
		public readonly Order.OrderTargetType TargetType;

		// Token: 0x0400089E RID: 2206
		public readonly int? WallSectionIndex;

		// Token: 0x0400089F RID: 2207
		public readonly bool UseController;

		// Token: 0x0200082E RID: 2094
		public enum OrderType
		{
			// Token: 0x04002EEC RID: 12012
			Current,
			// Token: 0x04002EED RID: 12013
			Previous
		}

		// Token: 0x0200082F RID: 2095
		public enum OrderTargetType
		{
			// Token: 0x04002EEF RID: 12015
			Entity,
			// Token: 0x04002EF0 RID: 12016
			Position,
			// Token: 0x04002EF1 RID: 12017
			WallSection
		}
	}
}
