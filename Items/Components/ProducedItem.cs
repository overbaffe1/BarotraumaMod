using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005E6 RID: 1510
	[NullableContext(1)]
	[Nullable(0)]
	internal class ProducedItem
	{
		// Token: 0x17001917 RID: 6423
		// (get) Token: 0x0600632E RID: 25390 RVA: 0x0033AFE7 File Offset: 0x003391E7
		// (set) Token: 0x0600632F RID: 25391 RVA: 0x0033AFEF File Offset: 0x003391EF
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Probability { get; set; }

		// Token: 0x06006330 RID: 25392 RVA: 0x0033AFF8 File Offset: 0x003391F8
		public ProducedItem(Item producer, ItemPrefab prefab, float probability)
		{
			this.Producer = producer;
			this.Prefab = prefab;
			this.Probability = probability;
		}

		// Token: 0x06006331 RID: 25393 RVA: 0x0033B020 File Offset: 0x00339220
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

		// Token: 0x06006332 RID: 25394 RVA: 0x0033B080 File Offset: 0x00339280
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

		// Token: 0x0400334A RID: 13130
		public readonly List<StatusEffect> StatusEffects = new List<StatusEffect>();

		// Token: 0x0400334B RID: 13131
		public readonly Item Producer;

		// Token: 0x0400334C RID: 13132
		[Nullable(2)]
		public readonly ItemPrefab Prefab;
	}
}
