using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005CA RID: 1482
	[NullableContext(1)]
	[Nullable(0)]
	internal class Planter : Pickable, IDrawableComponent
	{
		// Token: 0x1700179E RID: 6046
		// (get) Token: 0x06005DE2 RID: 24034 RVA: 0x0030FA22 File Offset: 0x0030DC22
		public Vector2 DrawSize
		{
			get
			{
				return this.CalculateSize();
			}
		}

		// Token: 0x06005DE3 RID: 24035 RVA: 0x0030FA2C File Offset: 0x0030DC2C
		private Vector2 CalculateSize()
		{
			if (this.GrowableSeeds.All((Growable s) => s == null))
			{
				return Vector2.Zero;
			}
			Point pos = this.item.DrawPosition.ToPoint();
			Rectangle rect = new Rectangle(pos, Point.Zero);
			for (int i = 0; i < this.GrowableSeeds.Length; i++)
			{
				Growable seed = this.GrowableSeeds[i];
				PlantSlot slot = this.PlantSlots.ContainsKey(i) ? this.PlantSlots[i] : Planter.NullSlot;
				if (seed != null)
				{
					foreach (VineTile vine in seed.Vines)
					{
						Rectangle worldRect = vine.Rect;
						worldRect.Location += slot.Offset.ToPoint();
						worldRect.Location += pos;
						rect = Rectangle.Union(rect, worldRect);
					}
				}
			}
			Vector2 result = new Vector2(Planter.<CalculateSize>g__MaxDistance|2_1((float)pos.X, (float)rect.Left, (float)rect.Right) * 2f, Planter.<CalculateSize>g__MaxDistance|2_1((float)pos.Y, (float)rect.Top, (float)rect.Bottom) * 2f);
			return result;
		}

		// Token: 0x06005DE4 RID: 24036 RVA: 0x0030FBB0 File Offset: 0x0030DDB0
		[NullableContext(0)]
		public void Draw(SpriteBatch spriteBatch, bool editing, float itemDepth = -1f, Color? overrideColor = null)
		{
			for (int i = 0; i < this.GrowableSeeds.Length; i++)
			{
				Growable growable = this.GrowableSeeds[i];
				PlantSlot slot = this.PlantSlots.ContainsKey(i) ? this.PlantSlots[i] : Planter.NullSlot;
				if (growable != null)
				{
					growable.Draw(spriteBatch, this, slot.Offset, itemDepth);
				}
			}
		}

		// Token: 0x1700179F RID: 6047
		// (get) Token: 0x06005DE5 RID: 24037 RVA: 0x0030FC0D File Offset: 0x0030DE0D
		// (set) Token: 0x06005DE6 RID: 24038 RVA: 0x0030FC15 File Offset: 0x0030DE15
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

		// Token: 0x170017A0 RID: 6048
		// (get) Token: 0x06005DE7 RID: 24039 RVA: 0x0030FC2E File Offset: 0x0030DE2E
		// (set) Token: 0x06005DE8 RID: 24040 RVA: 0x0030FC36 File Offset: 0x0030DE36
		[Serialize(100f, IsPropertySaveable.Yes, "How much fertilizer can the planter hold.", "", false)]
		public float FertilizerCapacity { get; set; }

		// Token: 0x170017A1 RID: 6049
		// (get) Token: 0x06005DE9 RID: 24041 RVA: 0x0030FC3F File Offset: 0x0030DE3F
		public override bool DontTransferInventoryBetweenSubs
		{
			get
			{
				return true;
			}
		}

		// Token: 0x06005DEA RID: 24042 RVA: 0x0030FC44 File Offset: 0x0030DE44
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

		// Token: 0x06005DEB RID: 24043 RVA: 0x0030FD64 File Offset: 0x0030DF64
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

		// Token: 0x06005DEC RID: 24044 RVA: 0x0030FE0C File Offset: 0x0030E00C
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

		// Token: 0x06005DED RID: 24045 RVA: 0x0030FEE8 File Offset: 0x0030E0E8
		public override bool Pick(Character character)
		{
			SuitablePlantItem plantItem = this.GetSuitableItem(character);
			base.PickingMsg = (plantItem.IsNull() ? "progressbar.uprooting" : plantItem.ProgressBarMessage);
			return base.Pick(character);
		}

		// Token: 0x06005DEE RID: 24046 RVA: 0x0030FF20 File Offset: 0x0030E120
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
						character.UpdateHUDProgressBar(this, base.Item.DrawPosition, this.Fertilizer / this.FertilizerCapacity, Color.SaddleBrown, Color.SaddleBrown, "entityname.fertilizer");
						base.ApplyStatusEffects(ActionType.OnPicked, 1f, character, null, null, null, null, 1f);
						return false;
					}
				}
				return false;
			}
			base.ApplyStatusEffects(ActionType.OnPicked, 1f, character, null, null, null, null, 1f);
			return (GameMain.NetworkMember == null || GameMain.NetworkMember.IsServer) && this.container.Inventory.TryPutItem(plantItem.Item, character, null, true, false, true);
		}

		// Token: 0x06005DEF RID: 24047 RVA: 0x00310064 File Offset: 0x0030E264
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

		// Token: 0x06005DF0 RID: 24048 RVA: 0x00310130 File Offset: 0x0030E330
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
			CharacterHUD.RecreateHudTexts = (CharacterHUD.RecreateHudTexts || recreateHudTexts);
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

		// Token: 0x06005DF1 RID: 24049 RVA: 0x00310390 File Offset: 0x0030E590
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

		// Token: 0x06005DF2 RID: 24050 RVA: 0x00310484 File Offset: 0x0030E684
		private bool HasAnyFinishedGrowing()
		{
			return this.GrowableSeeds.Any((Growable seed) => seed != null && (seed.FullyGrown || seed.Decayed));
		}

		// Token: 0x06005DF4 RID: 24052 RVA: 0x003104D2 File Offset: 0x0030E6D2
		[CompilerGenerated]
		internal static float <CalculateSize>g__MaxDistance|2_1(float origin, float x, float y)
		{
			return Math.Max(Math.Abs(origin - x), Math.Abs(origin - y));
		}

		// Token: 0x0400307C RID: 12412
		public static readonly PlantSlot NullSlot = default(PlantSlot);

		// Token: 0x0400307D RID: 12413
		public readonly Dictionary<int, PlantSlot> PlantSlots = new Dictionary<int, PlantSlot>();

		// Token: 0x0400307E RID: 12414
		private static readonly SuitablePlantItem NullItem = default(SuitablePlantItem);

		// Token: 0x0400307F RID: 12415
		private const string MsgFertilizer = "ItemMsgAddFertilizer";

		// Token: 0x04003080 RID: 12416
		private const string MsgSeed = "ItemMsgPlantSeed";

		// Token: 0x04003081 RID: 12417
		private const string MsgHarvest = "ItemMsgHarvest";

		// Token: 0x04003082 RID: 12418
		private const string MsgUprooting = "progressbar.uprooting";

		// Token: 0x04003083 RID: 12419
		private const string MsgFertilizing = "progressbar.fertilizing";

		// Token: 0x04003084 RID: 12420
		private const string MsgPlanting = "progressbar.planting";

		// Token: 0x04003085 RID: 12421
		public static float GrowthTickDelay = 1f;

		// Token: 0x04003086 RID: 12422
		private float fertilizer;

		// Token: 0x04003088 RID: 12424
		[Nullable(new byte[]
		{
			1,
			2
		})]
		public Growable[] GrowableSeeds = new Growable[0];

		// Token: 0x04003089 RID: 12425
		private readonly List<RelatedItem> SuitableFertilizer = new List<RelatedItem>();

		// Token: 0x0400308A RID: 12426
		private readonly List<RelatedItem> SuitableSeeds = new List<RelatedItem>();

		// Token: 0x0400308B RID: 12427
		[Nullable(2)]
		private ItemContainer container;

		// Token: 0x0400308C RID: 12428
		private float growthTickTimer;

		// Token: 0x0400308D RID: 12429
		[Nullable(new byte[]
		{
			2,
			1
		})]
		private List<LightComponent> lightComponents;
	}
}
