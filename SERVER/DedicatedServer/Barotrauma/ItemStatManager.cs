using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020001FD RID: 509
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class ItemStatManager
	{
		// Token: 0x060024AD RID: 9389 RVA: 0x000F1F3C File Offset: 0x000F013C
		public ItemStatManager(Item item)
		{
			this.item = item;
		}

		// Token: 0x060024AE RID: 9390 RVA: 0x000F1F58 File Offset: 0x000F0158
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
					NetworkMember server = GameMain.NetworkMember;
					if (server != null && server.IsServer)
					{
						server.CreateEntityEvent(this.item, new Item.SetItemStatEventData(this.talentStats));
					}
					return;
				}
			}
		}

		// Token: 0x060024AF RID: 9391 RVA: 0x000F203C File Offset: 0x000F023C
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

		// Token: 0x060024B0 RID: 9392 RVA: 0x000F20E8 File Offset: 0x000F02E8
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

		// Token: 0x060024B1 RID: 9393 RVA: 0x000F215C File Offset: 0x000F035C
		public void ApplyStatDirect(TalentStatIdentifier identifier, float value)
		{
			this.talentStats[identifier] = value;
		}

		// Token: 0x060024B2 RID: 9394 RVA: 0x000F216C File Offset: 0x000F036C
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

		// Token: 0x060024B3 RID: 9395 RVA: 0x000F21DC File Offset: 0x000F03DC
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

		// Token: 0x0400121F RID: 4639
		private readonly Dictionary<TalentStatIdentifier, float> talentStats = new Dictionary<TalentStatIdentifier, float>();

		// Token: 0x04001220 RID: 4640
		private readonly Item item;
	}
}
