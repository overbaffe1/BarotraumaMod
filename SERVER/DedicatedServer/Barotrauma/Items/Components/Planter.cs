using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004D7 RID: 1239
	[NullableContext(1)]
	[Nullable(0)]
	internal class Planter : Pickable, IDrawableComponent
	{
		// Token: 0x170012E1 RID: 4833
		// (get) Token: 0x0600465F RID: 18015 RVA: 0x001C16E4 File Offset: 0x001BF8E4
		// (set) Token: 0x06004660 RID: 18016 RVA: 0x001C16EC File Offset: 0x001BF8EC
		[Serialize(0f, IsPropertySaveable.Yes, "How much fertilizer the planter has.", "", false)]
		public float Fertilizer
		{
			get
			{
				return this.fertilizer;
			}
			set
			{
				this.fertilizer = Math.Clamp(value, 0f, this.FertilizerCapacity);
			}
		}

		// Token: 0x170012E2 RID: 4834
		// (get) Token: 0x06004661 RID: 18017 RVA: 0x001C1705 File Offset: 0x001BF905
		// (set) Token: 0x06004662 RID: 18018 RVA: 0x001C170D File Offset: 0x001BF90D
		[Serialize(100f, IsPropertySaveable.Yes, "How much fertilizer can the planter hold.", "", false)]
		public float FertilizerCapacity { get; set; }

		// Token: 0x170012E3 RID: 4835
		// (get) Token: 0x06004663 RID: 18019 RVA: 0x001C1716 File Offset: 0x001BF916
		public override bool DontTransferInventoryBetweenSubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06004664 RID: 18020 RVA: 0x001C171C File Offset: 0x001BF91C
		public Planter(Item item, ContentXElement element) : base(item, element)
		{
			this.canBePicked = true;
			SerializableProperty.DeserializeProperties(this, element);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "plantslot"))
				{
					if (!(a == "suitablefertilizer"))
					{
						if (a == "suitableseed")
						{
							this.SuitableSeeds.Add(RelatedItem.Load(subElement, true, item.Name));
						}
					}
					else
					{
						this.SuitableFertilizer.Add(RelatedItem.Load(subElement, true, item.Name));
					}
				}
				else
				{
					this.PlantSlots.Add(subElement.GetAttributeInt("slot", 0), new PlantSlot(subElement));
				}
			}
		}

		// Token: 0x06004665 RID: 18021 RVA: 0x001C183C File Offset: 0x001BFA3C
		public override void OnItemLoaded()
		{
			base.OnItemLoaded();
			this.IsActive = true;
			IEnumerable<LightComponent> lights = this.item.GetComponents<LightComponent>();
			if (lights.Any<LightComponent>())
			{
				this.lightComponents = lights.ToList<LightComponent>();
				foreach (LightComponent light in this.item.GetComponents<LightComponent>())
				{
					light.IsOn = false;
				}
			}
			this.container = this.item.GetComponent<ItemContainer>();
			this.GrowableSeeds = new Growable[this.container.Capacity];
		}

		// Token: 0x06004666 RID: 18022 RVA: 0x001C18E4 File Offset: 0x001BFAE4
		public override bool HasRequiredItems(Character character, bool addMessage, [Nullable(2)] LocalizedString msg = null)
		{
			ItemContainer itemContainer = this.container;
			if (((itemContainer != null) ? itemContainer.Inventory : null) == null)
			{
				return false;
			}
			SuitablePlantItem plantItem = this.GetSuitableItem(character);
			if (!plantItem.IsNull())
			{
				Holdable component = this.item.GetComponent<Holdable>();
				if (component == null || !component.Attachable || component.Attached)
				{
					PlantItemType type = plantItem.Type;
					string msg2;
					if (type != PlantItemType.Seed)
					{
						if (type != PlantItemType.Fertilizer)
						{
							throw new ArgumentOutOfRangeException();
						}
						msg2 = "ItemMsgAddFertilizer";
					}
					else
					{
						msg2 = "ItemMsgPlantSeed";
					}
					base.Msg = msg2;
					this.ParseMsg();
					return true;
				}
			}
			if (this.GrowableSeeds.Any((Growable s) => s != null))
			{
				base.Msg = "ItemMsgHarvest";
				this.ParseMsg();
				return true;
			}
			base.Msg = string.Empty;
			this.ParseMsg();
			return false;
		}

		// Token: 0x06004667 RID: 18023 RVA: 0x001C19C0 File Offset: 0x001BFBC0
		public override bool Pick(Character character)
		{
			SuitablePlantItem plantItem = this.GetSuitableItem(character);
			base.PickingMsg = (plantItem.IsNull() ? "progressbar.uprooting" : plantItem.ProgressBarMessage);
			return base.Pick(character);
		}

		// Token: 0x06004668 RID: 18024 RVA: 0x001C19F8 File Offset: 0x001BFBF8
		public override bool OnPicked(Character character)
		{
			ItemContainer itemContainer = this.container;
			if (((itemContainer != null) ? itemContainer.Inventory : null) == null)
			{
				return false;
			}
			SuitablePlantItem plantItem = this.GetSuitableItem(character);
			if (plantItem.IsNull())
			{
				return this.TryHarvest(character);
			}
			PlantItemType type = plantItem.Type;
			if (type != PlantItemType.Seed)
			{
				if (type == PlantItemType.Fertilizer)
				{
					if (plantItem.Item != null)
					{
						float canAdd = this.FertilizerCapacity - this.Fertilizer;
						float maxAvailable = plantItem.Item.Condition;
						float toAdd = Math.Min(canAdd, maxAvailable);
						plantItem.Item.Condition -= toAdd;
						this.fertilizer += toAdd;
						base.ApplyStatusEffects(ActionType.OnPicked, 1f, character, null, null, null, null, 1f);
						return false;
					}
				}
				return false;
			}
			base.ApplyStatusEffects(ActionType.OnPicked, 1f, character, null, null, null, null, 1f);
			return (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && this.container.Inventory.TryPutItem(plantItem.Item, character, null, true, false, true);
		}

		// Token: 0x06004669 RID: 18025 RVA: 0x001C1B08 File Offset: 0x001BFD08
		[NullableContext(2)]
		private bool TryHarvest(Character character)
		{
			bool anyDecayed = this.GrowableSeeds.Any((Growable s) => s != null && (s.Decayed || s.FullyGrown));
			for (int i = 0; i < this.GrowableSeeds.Length; i++)
			{
				Growable seed = this.GrowableSeeds[i];
				if (seed != null && (!anyDecayed || seed.Decayed || seed.FullyGrown))
				{
					ItemContainer itemContainer = this.container;
					if (itemContainer != null)
					{
						itemContainer.Inventory.RemoveItem(seed.Item);
					}
					EntitySpawner spawner = Entity.Spawner;
					if (spawner != null)
					{
						spawner.AddItemToRemoveQueue(seed.Item);
					}
					this.GrowableSeeds[i] = null;
					base.ApplyStatusEffects(ActionType.OnPicked, 1f, character, null, null, null, null, 1f);
					return true;
				}
			}
			return false;
		}

		// Token: 0x0600466A RID: 18026 RVA: 0x001C1BD4 File Offset: 0x001BFDD4
		public override void Update(float deltaTime, Camera cam)
		{
			base.Update(deltaTime, cam);
			if (this.lightComponents != null && this.lightComponents.Count > 0)
			{
				bool hasSeed = false;
				foreach (Growable seed in this.GrowableSeeds)
				{
					hasSeed |= (seed != null);
				}
				foreach (LightComponent light in this.lightComponents)
				{
					light.IsOn = hasSeed;
				}
			}
			ItemContainer itemContainer = this.container;
			if (((itemContainer != null) ? itemContainer.Inventory : null) == null)
			{
				return;
			}
			bool recreateHudTexts = false;
			for (int i = 0; i < this.container.Inventory.Capacity; i++)
			{
				if (i >= 0 && this.GrowableSeeds.Length > i)
				{
					Item containedItem = this.container.Inventory.GetItemAt(i);
					Growable growable = (containedItem != null) ? containedItem.GetComponent<Growable>() : null;
					if (growable != null)
					{
						recreateHudTexts |= (this.GrowableSeeds[i] != growable);
						this.GrowableSeeds[i] = growable;
						growable.IsActive = true;
					}
					else
					{
						Growable oldGrowable = this.GrowableSeeds[i];
						if (oldGrowable != null)
						{
							oldGrowable.Decayed = true;
							oldGrowable.IsActive = false;
							recreateHudTexts = true;
						}
						this.GrowableSeeds[i] = null;
					}
				}
			}
			if (GameMain.NetworkMember != null && GameMain.NetworkMember.IsClient)
			{
				return;
			}
			float delay = Planter.GrowthTickDelay;
			if (this.Fertilizer > 0f)
			{
				delay /= 2f;
				this.Fertilizer -= deltaTime / 10f;
			}
			if (this.growthTickTimer > delay)
			{
				for (int j = 0; j < this.GrowableSeeds.Length; j++)
				{
					PlantSlot slot = this.PlantSlots.ContainsKey(j) ? this.PlantSlots[j] : Planter.NullSlot;
					Growable seed2 = this.GrowableSeeds[j];
					if (seed2 != null)
					{
						seed2.OnGrowthTick(this, slot);
					}
				}
				this.growthTickTimer = 0f;
				return;
			}
			if (base.Item.ParentInventory == null)
			{
				Holdable holdable = this.item.GetComponent<Holdable>();
				if (holdable != null && holdable.Attachable && !holdable.Attached)
				{
					return;
				}
				this.growthTickTimer += deltaTime;
			}
		}

		// Token: 0x0600466B RID: 18027 RVA: 0x001C1E28 File Offset: 0x001C0028
		private SuitablePlantItem GetSuitableItem(Character character)
		{
			using (IEnumerator<Item> enumerator = character.HeldItems.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item heldItem = enumerator.Current;
					ItemContainer itemContainer = this.container;
					if (((itemContainer != null) ? itemContainer.Inventory : null) != null && this.container.Inventory.CanBePut(heldItem) && heldItem.GetComponent<Growable>() != null && this.SuitableSeeds.Any((RelatedItem ri) => ri.MatchesItem(heldItem)))
					{
						return new SuitablePlantItem(heldItem, PlantItemType.Seed, "progressbar.planting");
					}
					if (this.SuitableFertilizer.Any((RelatedItem ri) => ri.MatchesItem(heldItem)))
					{
						return new SuitablePlantItem(heldItem, PlantItemType.Fertilizer, "progressbar.fertilizing");
					}
				}
			}
			return Planter.NullItem;
		}

		// Token: 0x0600466C RID: 18028 RVA: 0x001C1F1C File Offset: 0x001C011C
		private bool HasAnyFinishedGrowing()
		{
			return this.GrowableSeeds.Any((Growable seed) => seed != null && (seed.FullyGrown || seed.Decayed));
		}

		// Token: 0x040021DE RID: 8670
		public static readonly PlantSlot NullSlot = default(PlantSlot);

		// Token: 0x040021DF RID: 8671
		public readonly Dictionary<int, PlantSlot> PlantSlots = new Dictionary<int, PlantSlot>();

		// Token: 0x040021E0 RID: 8672
		private static readonly SuitablePlantItem NullItem = default(SuitablePlantItem);

		// Token: 0x040021E1 RID: 8673
		private const string MsgFertilizer = "ItemMsgAddFertilizer";

		// Token: 0x040021E2 RID: 8674
		private const string MsgSeed = "ItemMsgPlantSeed";

		// Token: 0x040021E3 RID: 8675
		private const string MsgHarvest = "ItemMsgHarvest";

		// Token: 0x040021E4 RID: 8676
		private const string MsgUprooting = "progressbar.uprooting";

		// Token: 0x040021E5 RID: 8677
		private const string MsgFertilizing = "progressbar.fertilizing";

		// Token: 0x040021E6 RID: 8678
		private const string MsgPlanting = "progressbar.planting";

		// Token: 0x040021E7 RID: 8679
		public static float GrowthTickDelay = 1f;

		// Token: 0x040021E8 RID: 8680
		private float fertilizer;

		// Token: 0x040021EA RID: 8682
		[Nullable(new byte[]
		{
			1,
			2
		})]
		public Growable[] GrowableSeeds = new Growable[0];

		// Token: 0x040021EB RID: 8683
		private readonly List<RelatedItem> SuitableFertilizer = new List<RelatedItem>();

		// Token: 0x040021EC RID: 8684
		private readonly List<RelatedItem> SuitableSeeds = new List<RelatedItem>();

		// Token: 0x040021ED RID: 8685
		[Nullable(2)]
		private ItemContainer container;

		// Token: 0x040021EE RID: 8686
		private float growthTickTimer;

		// Token: 0x040021EF RID: 8687
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private List<LightComponent> lightComponents;
	}
}
