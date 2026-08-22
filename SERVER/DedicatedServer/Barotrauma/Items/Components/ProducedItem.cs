using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004B3 RID: 1203
	[NullableContext(1)]
	[Nullable(0)]
	internal class ProducedItem
	{
		// Token: 0x17001245 RID: 4677
		// (get) Token: 0x0600448A RID: 17546 RVA: 0x001B73E5 File Offset: 0x001B55E5
		// (set) Token: 0x0600448B RID: 17547 RVA: 0x001B73ED File Offset: 0x001B55ED
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Probability { get; set; }

		// Token: 0x0600448C RID: 17548 RVA: 0x001B73F6 File Offset: 0x001B55F6
		public ProducedItem(Item producer, ItemPrefab prefab, float probability)
		{
			this.Producer = producer;
			this.Prefab = prefab;
			this.Probability = probability;
		}

		// Token: 0x0600448D RID: 17549 RVA: 0x001B7420 File Offset: 0x001B5620
		public ProducedItem(Item producer, ContentXElement element)
		{
			SerializableProperty.DeserializeProperties(this, element);
			this.Producer = producer;
			Identifier itemIdentifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
			if (!itemIdentifier.IsEmpty)
			{
				this.Prefab = ItemPrefab.Find(null, itemIdentifier);
			}
			this.LoadSubElements(element);
		}

		// Token: 0x0600448E RID: 17550 RVA: 0x001B7480 File Offset: 0x001B5680
		private void LoadSubElements(ContentXElement element)
		{
			if (!element.HasElements)
			{
				return;
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "statuseffect")
				{
					ContentXElement element2 = subElement;
					ItemPrefab prefab = this.Prefab;
					StatusEffect effect = StatusEffect.Load(element2, (prefab != null) ? prefab.Name.Value : null);
					if (effect.type != ActionType.OnProduceSpawned)
					{
						DebugConsole.ThrowError("Only OnProduceSpawned type can be used in <ProducedItem>.", null, element.ContentPackage, false, false);
					}
					else
					{
						this.StatusEffects.Add(effect);
					}
				}
			}
		}

		// Token: 0x040020C2 RID: 8386
		public readonly List<StatusEffect> StatusEffects = new List<StatusEffect>();

		// Token: 0x040020C3 RID: 8387
		public readonly Item Producer;

		// Token: 0x040020C4 RID: 8388
		[Nullable(2)]
		public readonly ItemPrefab Prefab;
	}
}
