using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Items.Components;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000191 RID: 401
	internal class Order
	{
		// Token: 0x17000C09 RID: 3081
		// (get) Token: 0x06002F01 RID: 12033 RVA: 0x001F3EFC File Offset: 0x001F20FC
		public bool IsCurrentOrder
		{
			get
			{
				return this.Type == Order.OrderType.Current;
			}
		}

		// Token: 0x17000C0A RID: 3082
		// (get) Token: 0x06002F02 RID: 12034 RVA: 0x001F3F07 File Offset: 0x001F2107
		public bool IsDismissal
		{
			get
			{
				return this.Prefab.IsDismissal;
			}
		}

		// Token: 0x17000C0B RID: 3083
		// (get) Token: 0x06002F03 RID: 12035 RVA: 0x001F3F14 File Offset: 0x001F2114
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

		// Token: 0x17000C0C RID: 3084
		// (get) Token: 0x06002F04 RID: 12036 RVA: 0x001F3F88 File Offset: 0x001F2188
		public Hull TargetHull
		{
			get
			{
				return this.TargetEntity as Hull;
			}
		}

		// Token: 0x17000C0D RID: 3085
		// (get) Token: 0x06002F05 RID: 12037 RVA: 0x001F3F95 File Offset: 0x001F2195
		public LocalizedString Name
		{
			get
			{
				return this.Prefab.Name;
			}
		}

		// Token: 0x17000C0E RID: 3086
		// (get) Token: 0x06002F06 RID: 12038 RVA: 0x001F3FA2 File Offset: 0x001F21A2
		public LocalizedString ContextualName
		{
			get
			{
				return this.Prefab.ContextualName;
			}
		}

		// Token: 0x17000C0F RID: 3087
		// (get) Token: 0x06002F07 RID: 12039 RVA: 0x001F3FAF File Offset: 0x001F21AF
		public Identifier Identifier
		{
			get
			{
				return this.Prefab.Identifier;
			}
		}

		// Token: 0x17000C10 RID: 3088
		// (get) Token: 0x06002F08 RID: 12040 RVA: 0x001F3FBC File Offset: 0x001F21BC
		public Type ItemComponentType
		{
			get
			{
				return this.Prefab.ItemComponentType;
			}
		}

		// Token: 0x17000C11 RID: 3089
		// (get) Token: 0x06002F09 RID: 12041 RVA: 0x001F3FC9 File Offset: 0x001F21C9
		public bool CanTypeBeSubclass
		{
			get
			{
				return this.Prefab.CanTypeBeSubclass;
			}
		}

		// Token: 0x17000C12 RID: 3090
		// (get) Token: 0x06002F0A RID: 12042 RVA: 0x001F3FD6 File Offset: 0x001F21D6
		public ref readonly ImmutableArray<Identifier> ControllerTags
		{
			get
			{
				return ref this.Prefab.ControllerTags;
			}
		}

		// Token: 0x17000C13 RID: 3091
		// (get) Token: 0x06002F0B RID: 12043 RVA: 0x001F3FE3 File Offset: 0x001F21E3
		public ref readonly ImmutableArray<Identifier> TargetItems
		{
			get
			{
				return ref this.Prefab.TargetItems;
			}
		}

		// Token: 0x17000C14 RID: 3092
		// (get) Token: 0x06002F0C RID: 12044 RVA: 0x001F3FF0 File Offset: 0x001F21F0
		public ref readonly ImmutableArray<Identifier> RequireItems
		{
			get
			{
				return ref this.Prefab.RequireItems;
			}
		}

		// Token: 0x17000C15 RID: 3093
		// (get) Token: 0x06002F0D RID: 12045 RVA: 0x001F3FFD File Offset: 0x001F21FD
		public ref readonly ImmutableArray<Identifier> Options
		{
			get
			{
				return ref this.Prefab.Options;
			}
		}

		// Token: 0x17000C16 RID: 3094
		// (get) Token: 0x06002F0E RID: 12046 RVA: 0x001F400A File Offset: 0x001F220A
		public ref readonly ImmutableArray<Identifier> HiddenOptions
		{
			get
			{
				return ref this.Prefab.HiddenOptions;
			}
		}

		// Token: 0x17000C17 RID: 3095
		// (get) Token: 0x06002F0F RID: 12047 RVA: 0x001F4017 File Offset: 0x001F2217
		public ref readonly ImmutableArray<Identifier> AllOptions
		{
			get
			{
				return ref this.Prefab.AllOptions;
			}
		}

		// Token: 0x17000C18 RID: 3096
		// (get) Token: 0x06002F10 RID: 12048 RVA: 0x001F4024 File Offset: 0x001F2224
		public Sprite SymbolSprite
		{
			get
			{
				return this.Prefab.SymbolSprite;
			}
		}

		// Token: 0x17000C19 RID: 3097
		// (get) Token: 0x06002F11 RID: 12049 RVA: 0x001F4031 File Offset: 0x001F2231
		public Color Color
		{
			get
			{
				return this.Prefab.Color;
			}
		}

		// Token: 0x17000C1A RID: 3098
		// (get) Token: 0x06002F12 RID: 12050 RVA: 0x001F403E File Offset: 0x001F223E
		public bool TargetAllCharacters
		{
			get
			{
				return this.Prefab.TargetAllCharacters;
			}
		}

		// Token: 0x17000C1B RID: 3099
		// (get) Token: 0x06002F13 RID: 12051 RVA: 0x001F404B File Offset: 0x001F224B
		public ref readonly ImmutableArray<Identifier> AppropriateJobs
		{
			get
			{
				return ref this.Prefab.AppropriateJobs;
			}
		}

		// Token: 0x17000C1C RID: 3100
		// (get) Token: 0x06002F14 RID: 12052 RVA: 0x001F4058 File Offset: 0x001F2258
		public float FadeOutTime
		{
			get
			{
				return this.Prefab.FadeOutTime;
			}
		}

		// Token: 0x17000C1D RID: 3101
		// (get) Token: 0x06002F15 RID: 12053 RVA: 0x001F4065 File Offset: 0x001F2265
		public bool MustSetTarget
		{
			get
			{
				return this.Prefab.MustSetTarget;
			}
		}

		// Token: 0x17000C1E RID: 3102
		// (get) Token: 0x06002F16 RID: 12054 RVA: 0x001F4072 File Offset: 0x001F2272
		public Identifier AppropriateSkill
		{
			get
			{
				return this.Prefab.AppropriateSkill;
			}
		}

		// Token: 0x17000C1F RID: 3103
		// (get) Token: 0x06002F17 RID: 12055 RVA: 0x001F407F File Offset: 0x001F227F
		public OrderCategory? Category
		{
			get
			{
				return this.Prefab.Category;
			}
		}

		// Token: 0x17000C20 RID: 3104
		// (get) Token: 0x06002F18 RID: 12056 RVA: 0x001F408C File Offset: 0x001F228C
		public bool MustManuallyAssign
		{
			get
			{
				return this.Prefab.MustManuallyAssign;
			}
		}

		// Token: 0x17000C21 RID: 3105
		// (get) Token: 0x06002F19 RID: 12057 RVA: 0x001F4099 File Offset: 0x001F2299
		public bool IsIgnoreOrder
		{
			get
			{
				return this.Prefab.IsIgnoreOrder;
			}
		}

		// Token: 0x17000C22 RID: 3106
		// (get) Token: 0x06002F1A RID: 12058 RVA: 0x001F40A6 File Offset: 0x001F22A6
		public bool IsDeconstructOrder
		{
			get
			{
				return this.Prefab.IsDeconstructOrder;
			}
		}

		// Token: 0x17000C23 RID: 3107
		// (get) Token: 0x06002F1B RID: 12059 RVA: 0x001F40B3 File Offset: 0x001F22B3
		public bool DrawIconWhenContained
		{
			get
			{
				return this.Prefab.DrawIconWhenContained;
			}
		}

		// Token: 0x17000C24 RID: 3108
		// (get) Token: 0x06002F1C RID: 12060 RVA: 0x001F40C0 File Offset: 0x001F22C0
		public bool Hidden
		{
			get
			{
				return this.Prefab.Hidden;
			}
		}

		// Token: 0x17000C25 RID: 3109
		// (get) Token: 0x06002F1D RID: 12061 RVA: 0x001F40CD File Offset: 0x001F22CD
		public bool IgnoreAtOutpost
		{
			get
			{
				return this.Prefab.IgnoreAtOutpost;
			}
		}

		// Token: 0x17000C26 RID: 3110
		// (get) Token: 0x06002F1E RID: 12062 RVA: 0x001F40DA File Offset: 0x001F22DA
		public bool IsReport
		{
			get
			{
				return this.Prefab.IsReport;
			}
		}

		// Token: 0x17000C27 RID: 3111
		// (get) Token: 0x06002F1F RID: 12063 RVA: 0x001F40E7 File Offset: 0x001F22E7
		public bool AutoDismiss
		{
			get
			{
				return this.Prefab.AutoDismiss;
			}
		}

		// Token: 0x17000C28 RID: 3112
		// (get) Token: 0x06002F20 RID: 12064 RVA: 0x001F40F4 File Offset: 0x001F22F4
		public int AssignmentPriority
		{
			get
			{
				return this.Prefab.AssignmentPriority;
			}
		}

		// Token: 0x17000C29 RID: 3113
		// (get) Token: 0x06002F21 RID: 12065 RVA: 0x001F4101 File Offset: 0x001F2301
		public bool ColoredWhenControllingGiver
		{
			get
			{
				return this.Prefab.ColoredWhenControllingGiver;
			}
		}

		// Token: 0x17000C2A RID: 3114
		// (get) Token: 0x06002F22 RID: 12066 RVA: 0x001F410E File Offset: 0x001F230E
		public bool DisplayGiverInTooltip
		{
			get
			{
				return this.Prefab.DisplayGiverInTooltip;
			}
		}

		// Token: 0x06002F23 RID: 12067 RVA: 0x001F411C File Offset: 0x001F231C
		public Order(OrderPrefab prefab, Entity targetEntity, ItemComponent targetItem, Character orderGiver = null, bool isAutonomous = false) : this(prefab, Identifier.Empty, 0, Order.OrderType.Current, null, targetEntity, targetItem, orderGiver, isAutonomous)
		{
		}

		// Token: 0x06002F24 RID: 12068 RVA: 0x001F4140 File Offset: 0x001F2340
		public Order(OrderPrefab prefab, Identifier option, Entity targetEntity, ItemComponent targetItem, Character orderGiver = null, bool isAutonomous = false) : this(prefab, option, 0, Order.OrderType.Current, null, targetEntity, targetItem, orderGiver, isAutonomous)
		{
		}

		// Token: 0x06002F25 RID: 12069 RVA: 0x001F415F File Offset: 0x001F235F
		public Order(OrderPrefab prefab, OrderTarget target, Character orderGiver = null) : this(prefab, prefab.Options.FirstOrDefault<Identifier>(), 0, Order.OrderType.Current, null, target, orderGiver)
		{
		}

		// Token: 0x06002F26 RID: 12070 RVA: 0x001F4178 File Offset: 0x001F2378
		public Order(OrderPrefab prefab, Identifier option, OrderTarget target, Character orderGiver = null) : this(prefab, option, 0, Order.OrderType.Current, null, target, orderGiver)
		{
		}

		// Token: 0x06002F27 RID: 12071 RVA: 0x001F4188 File Offset: 0x001F2388
		public Order(OrderPrefab prefab, Structure wall, int? sectionIndex, Character orderGiver = null) : this(prefab, Identifier.Empty, 0, Order.OrderType.Current, null, wall, sectionIndex, orderGiver)
		{
		}

		// Token: 0x06002F28 RID: 12072 RVA: 0x001F41A8 File Offset: 0x001F23A8
		public Order(OrderPrefab prefab, Identifier option, Structure wall, int? sectionIndex, Character orderGiver = null) : this(prefab, option, 0, Order.OrderType.Current, null, wall, sectionIndex, orderGiver)
		{
		}

		// Token: 0x06002F29 RID: 12073 RVA: 0x001F41C8 File Offset: 0x001F23C8
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

		// Token: 0x06002F2A RID: 12074 RVA: 0x001F428C File Offset: 0x001F248C
		private Order(OrderPrefab prefab, Identifier option, int manualPriority, Order.OrderType orderType, AIObjective aiObjective, OrderTarget target, Character orderGiver = null) : this(prefab, option, manualPriority, orderType, aiObjective, null, null, orderGiver, false)
		{
			this.TargetPosition = target;
			this.TargetType = Order.OrderTargetType.Position;
		}

		// Token: 0x06002F2B RID: 12075 RVA: 0x001F42BC File Offset: 0x001F24BC
		private Order(OrderPrefab prefab, Identifier option, int manualPriority, Order.OrderType orderType, AIObjective aiObjective, Structure wall, int? sectionIndex, Character orderGiver = null) : this(prefab, option, manualPriority, orderType, aiObjective, wall, null, orderGiver, false)
		{
			this.WallSectionIndex = sectionIndex;
			this.TargetType = Order.OrderTargetType.WallSection;
		}

		// Token: 0x06002F2C RID: 12076 RVA: 0x001F42EC File Offset: 0x001F24EC
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

		// Token: 0x06002F2D RID: 12077 RVA: 0x001F4430 File Offset: 0x001F2630
		public Order WithOption(Identifier option)
		{
			return new Order(this, null, option, null, null, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06002F2E RID: 12078 RVA: 0x001F4478 File Offset: 0x001F2678
		public Order WithManualPriority(int newPriority)
		{
			OrderPrefab prefab = null;
			int? manualPriority = new int?(newPriority);
			return new Order(this, prefab, default(Identifier), manualPriority, null, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06002F2F RID: 12079 RVA: 0x001F44CC File Offset: 0x001F26CC
		public Order WithOrderGiver(Character orderGiver)
		{
			return new Order(this, null, default(Identifier), null, null, null, null, null, null, orderGiver, null, null, null, null);
		}

		// Token: 0x06002F30 RID: 12080 RVA: 0x001F4520 File Offset: 0x001F2720
		public Order WithObjective(AIObjective objective)
		{
			return new Order(this, null, default(Identifier), null, null, objective, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06002F31 RID: 12081 RVA: 0x001F4574 File Offset: 0x001F2774
		public Order WithTargetEntity(Entity entity)
		{
			OrderPrefab prefab = null;
			Order.OrderTargetType? targetType = new Order.OrderTargetType?(Order.OrderTargetType.Entity);
			return new Order(this, prefab, default(Identifier), null, null, null, entity, null, null, null, null, targetType, null, null);
		}

		// Token: 0x06002F32 RID: 12082 RVA: 0x001F45C8 File Offset: 0x001F27C8
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

		// Token: 0x06002F33 RID: 12083 RVA: 0x001F4644 File Offset: 0x001F2844
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

		// Token: 0x06002F34 RID: 12084 RVA: 0x001F46D4 File Offset: 0x001F28D4
		public Order WithWallSection(Structure wall, int? sectionIndex)
		{
			OrderPrefab prefab = null;
			Order.OrderTargetType? targetType = new Order.OrderTargetType?(Order.OrderTargetType.WallSection);
			return new Order(this, prefab, default(Identifier), null, null, null, wall, null, null, null, null, targetType, sectionIndex, null);
		}

		// Token: 0x06002F35 RID: 12085 RVA: 0x001F4724 File Offset: 0x001F2924
		public Order WithType(Order.OrderType type)
		{
			OrderPrefab prefab = null;
			Order.OrderType? type2 = new Order.OrderType?(type);
			return new Order(this, prefab, default(Identifier), null, type2, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06002F36 RID: 12086 RVA: 0x001F4778 File Offset: 0x001F2978
		public Order WithTargetPosition(OrderTarget targetPosition)
		{
			OrderPrefab prefab = null;
			Order.OrderTargetType? targetType = new Order.OrderTargetType?(Order.OrderTargetType.Position);
			return new Order(this, prefab, default(Identifier), null, null, null, null, null, null, null, targetPosition, targetType, null, null);
		}

		// Token: 0x06002F37 RID: 12087 RVA: 0x001F47CC File Offset: 0x001F29CC
		public Order Clone()
		{
			return new Order(this, null, default(Identifier), null, null, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06002F38 RID: 12088 RVA: 0x001F4820 File Offset: 0x001F2A20
		public Order GetDismissal()
		{
			if (this.IsDismissal)
			{
				throw new InvalidOperationException("Attempted to dismiss a dismissal order");
			}
			return new Order(this, OrderPrefab.Prefabs["dismissed"], Order.GetDismissOrderOption(this), null, null, null, null, null, null, null, null, null, null, null);
		}

		// Token: 0x06002F39 RID: 12089 RVA: 0x001F488E File Offset: 0x001F2A8E
		public bool HasAppropriateJob(Character character)
		{
			return this.Prefab.HasAppropriateJob(character);
		}

		// Token: 0x06002F3A RID: 12090 RVA: 0x001F489C File Offset: 0x001F2A9C
		public bool HasPreferredJob(Character character)
		{
			return this.Prefab.HasPreferredJob(character);
		}

		// Token: 0x06002F3B RID: 12091 RVA: 0x001F48AA File Offset: 0x001F2AAA
		public string GetChatMessage(string targetCharacterName, string targetRoomName, bool givingOrderToSelf, Identifier orderOption = default(Identifier), bool isNewOrder = true)
		{
			return this.Prefab.GetChatMessage(targetCharacterName, targetRoomName, this.TargetEntity, givingOrderToSelf, orderOption, isNewOrder);
		}

		// Token: 0x06002F3C RID: 12092 RVA: 0x001F48C4 File Offset: 0x001F2AC4
		public ItemComponent GetTargetItemComponent(Item item)
		{
			return this.Prefab.GetTargetItemComponent(item);
		}

		// Token: 0x06002F3D RID: 12093 RVA: 0x001F48D2 File Offset: 0x001F2AD2
		public bool TryGetTargetItemComponent(Item item, out ItemComponent firstMatchingComponent)
		{
			return this.Prefab.TryGetTargetItemComponent(item, out firstMatchingComponent);
		}

		// Token: 0x06002F3E RID: 12094 RVA: 0x001F48E4 File Offset: 0x001F2AE4
		public List<Item> GetMatchingItems(Submarine submarine, bool mustBelongToPlayerSub, CharacterTeamType? requiredTeam = null, Character interactableFor = null)
		{
			return this.Prefab.GetMatchingItems(submarine, mustBelongToPlayerSub, requiredTeam, interactableFor, default(Identifier));
		}

		// Token: 0x06002F3F RID: 12095 RVA: 0x001F490C File Offset: 0x001F2B0C
		public List<Item> GetMatchingItems(bool mustBelongToPlayerSub, Character interactableFor = null)
		{
			return this.Prefab.GetMatchingItems(mustBelongToPlayerSub, interactableFor, default(Identifier));
		}

		// Token: 0x06002F40 RID: 12096 RVA: 0x001F492F File Offset: 0x001F2B2F
		public LocalizedString GetOptionName(string id)
		{
			return this.Prefab.GetOptionName(id);
		}

		// Token: 0x06002F41 RID: 12097 RVA: 0x001F493D File Offset: 0x001F2B3D
		public LocalizedString GetOptionName(Identifier id)
		{
			return this.Prefab.GetOptionName(id);
		}

		// Token: 0x06002F42 RID: 12098 RVA: 0x001F494B File Offset: 0x001F2B4B
		public LocalizedString GetOptionName(int index)
		{
			return this.Prefab.GetOptionName(index);
		}

		// Token: 0x06002F43 RID: 12099 RVA: 0x001F4959 File Offset: 0x001F2B59
		public static Identifier GetDismissOrderOption(Order order)
		{
			return OrderPrefab.GetDismissOrderOption(order);
		}

		// Token: 0x06002F44 RID: 12100 RVA: 0x001F4964 File Offset: 0x001F2B64
		public bool MatchesOrder(Identifier orderIdentifier, Identifier orderOption)
		{
			Identifier identifier = this.Identifier;
			return orderIdentifier == identifier && orderOption == this.Option;
		}

		// Token: 0x06002F45 RID: 12101 RVA: 0x001F4992 File Offset: 0x001F2B92
		public bool MatchesOrder(Order order)
		{
			return order != null && this.MatchesOrder(order.Identifier, order.Option);
		}

		// Token: 0x06002F46 RID: 12102 RVA: 0x001F49AC File Offset: 0x001F2BAC
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

		// Token: 0x06002F47 RID: 12103 RVA: 0x001F4A74 File Offset: 0x001F2C74
		public ImmutableArray<Identifier> GetTargetItems(Identifier option = default(Identifier))
		{
			return this.Prefab.GetTargetItems(option);
		}

		// Token: 0x06002F48 RID: 12104 RVA: 0x001F4A84 File Offset: 0x001F2C84
		public override string ToString()
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(8, 1);
			defaultInterpolatedStringHandler.AppendLiteral("Order (");
			defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(this.Name);
			defaultInterpolatedStringHandler.AppendLiteral(")");
			return defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x0400188C RID: 6284
		public readonly OrderPrefab Prefab;

		// Token: 0x0400188D RID: 6285
		public readonly Identifier Option;

		// Token: 0x0400188E RID: 6286
		public readonly int ManualPriority;

		// Token: 0x0400188F RID: 6287
		public readonly Order.OrderType Type;

		// Token: 0x04001890 RID: 6288
		public readonly AIObjective Objective;

		// Token: 0x04001891 RID: 6289
		public readonly Entity TargetEntity;

		// Token: 0x04001892 RID: 6290
		public readonly ItemComponent TargetItemComponent;

		// Token: 0x04001893 RID: 6291
		public readonly Controller ConnectedController;

		// Token: 0x04001894 RID: 6292
		public readonly Character OrderGiver;

		// Token: 0x04001895 RID: 6293
		public readonly OrderTarget TargetPosition;

		// Token: 0x04001896 RID: 6294
		private ISpatialEntity targetSpatialEntity;

		// Token: 0x04001897 RID: 6295
		public readonly Order.OrderTargetType TargetType;

		// Token: 0x04001898 RID: 6296
		public readonly int? WallSectionIndex;

		// Token: 0x04001899 RID: 6297
		public readonly bool UseController;

		// Token: 0x02000E76 RID: 3702
		public enum OrderType
		{
			// Token: 0x0400524C RID: 21068
			Current,
			// Token: 0x0400524D RID: 21069
			Previous
		}

		// Token: 0x02000E77 RID: 3703
		public enum OrderTargetType
		{
			// Token: 0x0400524F RID: 21071
			Entity,
			// Token: 0x04005250 RID: 21072
			Position,
			// Token: 0x04005251 RID: 21073
			WallSection
		}
	}
}
