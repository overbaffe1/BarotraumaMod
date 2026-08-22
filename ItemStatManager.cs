using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002E6 RID: 742
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class ItemStatManager
	{
		// Token: 0x06003D8B RID: 15755 RVA: 0x0022F4A4 File Offset: 0x0022D6A4
		public ItemStatManager(Item item)
		{
			this.item = item;
		}

		// Token: 0x06003D8C RID: 15756 RVA: 0x0022F4C0 File Offset: 0x0022D6C0
		public void ApplyStat(ItemTalentStats stat, bool stackable, bool save, float value, CharacterTalent talent)
		{
			Character character = talent.Character;
			ushort? num = (character != null) ? new ushort?(character.ID) : null;
			if (num != null)
			{
				ushort characterId = num.GetValueOrDefault();
				TalentPrefab prefab = talent.Prefab;
				Identifier? identifier2 = (prefab != null) ? new Identifier?(prefab.Identifier) : null;
				if (identifier2 != null)
				{
					Identifier talentIdentifier = identifier2.GetValueOrDefault();
					TalentStatIdentifier identifier = stackable ? TalentStatIdentifier.CreateStackable(stat, talentIdentifier, (uint)characterId) : TalentStatIdentifier.CreateUnstackable(stat, talentIdentifier, save);
					float existingValue;
					if (!stackable && this.talentStats.TryGetValue(identifier, out existingValue) && existingValue > value)
					{
						return;
					}
					this.talentStats[identifier] = value;
					return;
				}
			}
		}

		// Token: 0x06003D8D RID: 15757 RVA: 0x0022F578 File Offset: 0x0022D778
		public void Save(XElement parent)
		{
			XElement element = new XElement("itemstats");
			foreach (KeyValuePair<TalentStatIdentifier, float> keyValuePair in this.talentStats)
			{
				TalentStatIdentifier talentStatIdentifier;
				float num;
				keyValuePair.Deconstruct(out talentStatIdentifier, out num);
				TalentStatIdentifier key = talentStatIdentifier;
				float value = num;
				if (key.Save)
				{
					XElement statElement = key.Serialize();
					statElement.Add(new XAttribute("value", value));
					element.Add(statElement);
				}
			}
			parent.Add(element);
		}

		// Token: 0x06003D8E RID: 15758 RVA: 0x0022F624 File Offset: 0x0022D824
		public void Load(XElement element)
		{
			foreach (XElement statElement in element.Elements())
			{
				TalentStatIdentifier identifier;
				if (TalentStatIdentifier.TryLoadFromXML(statElement).TryUnwrap(out identifier))
				{
					float value = statElement.GetAttributeFloat("value", 0f);
					this.ApplyStatDirect(identifier, value);
				}
			}
		}

		// Token: 0x06003D8F RID: 15759 RVA: 0x0022F698 File Offset: 0x0022D898
		public void ApplyStatDirect(TalentStatIdentifier identifier, float value)
		{
			this.talentStats[identifier] = value;
		}

		// Token: 0x06003D90 RID: 15760 RVA: 0x0022F6A8 File Offset: 0x0022D8A8
		public float GetAdjustedValueMultiplicative(ItemTalentStats stat, float originalValue)
		{
			float total = originalValue;
			foreach (KeyValuePair<TalentStatIdentifier, float> keyValuePair in this.talentStats)
			{
				TalentStatIdentifier talentStatIdentifier;
				float num;
				keyValuePair.Deconstruct(out talentStatIdentifier, out num);
				TalentStatIdentifier key = talentStatIdentifier;
				float value = num;
				if (key.Stat == stat)
				{
					total *= value;
				}
			}
			return total;
		}

		// Token: 0x06003D91 RID: 15761 RVA: 0x0022F718 File Offset: 0x0022D918
		public float GetAdjustedValueAdditive(ItemTalentStats stat, float originalValue)
		{
			float total = originalValue;
			foreach (KeyValuePair<TalentStatIdentifier, float> keyValuePair in this.talentStats)
			{
				TalentStatIdentifier talentStatIdentifier;
				float num;
				keyValuePair.Deconstruct(out talentStatIdentifier, out num);
				TalentStatIdentifier key = talentStatIdentifier;
				float value = num;
				if (key.Stat == stat)
				{
					total += value;
				}
			}
			return total;
		}

		// Token: 0x0400204B RID: 8267
		private readonly Dictionary<TalentStatIdentifier, float> talentStats = new Dictionary<TalentStatIdentifier, float>();

		// Token: 0x0400204C RID: 8268
		private readonly Item item;
	}
}
