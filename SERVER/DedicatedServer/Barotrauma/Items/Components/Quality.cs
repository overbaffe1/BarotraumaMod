using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020004DE RID: 1246
	internal class Quality : ItemComponent
	{
		// Token: 0x170012FA RID: 4858
		// (get) Token: 0x060046BF RID: 18111 RVA: 0x001C410C File Offset: 0x001C230C
		// (set) Token: 0x060046C0 RID: 18112 RVA: 0x001C4114 File Offset: 0x001C2314
		[Editable(MinValueInt = 0, MaxValueInt = 3)]
		[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
		public int QualityLevel
		{
			get
			{
				return this.qualityLevel;
			}
			set
			{
				if (value == this.qualityLevel)
				{
					return;
				}
				bool wasInFullCondition = this.item.IsFullCondition;
				this.qualityLevel = MathHelper.Clamp(value, 0, 3);
				this.item.RecalculateConditionValues();
				if (wasInFullCondition && this.statValues.ContainsKey(Quality.StatType.Condition))
				{
					this.item.Condition = this.item.MaxCondition;
				}
			}
		}

		// Token: 0x060046C1 RID: 18113 RVA: 0x001C4178 File Offset: 0x001C2378
		public Quality(Item item, ContentXElement element) : base(item, element)
		{
			foreach (ContentXElement cxe in element.Elements())
			{
				XElement subElement = cxe;
				string a = subElement.Name.ToString().ToLower();
				if (a == "stattype" || a == "statvalue" || a == "qualitystat")
				{
					string statTypeString = subElement.GetAttributeString("stattype", "");
					Quality.StatType statType;
					if (!Enum.TryParse<Quality.StatType>(statTypeString, true, out statType))
					{
						DebugConsole.ThrowError(string.Concat(new string[]
						{
							"Invalid stat type type \"",
							statTypeString,
							"\" in item (",
							item.Prefab.Identifier.ToString(),
							")"
						}), null, element.ContentPackage, false, false);
					}
					float statValue = subElement.GetAttributeFloat("value", 0f);
					this.statValues.TryAdd(statType, statValue);
				}
			}
		}

		// Token: 0x060046C2 RID: 18114 RVA: 0x001C42A8 File Offset: 0x001C24A8
		public float GetValue(Quality.StatType statType)
		{
			if (!this.statValues.ContainsKey(statType))
			{
				return 0f;
			}
			return this.statValues[statType] * (float)this.qualityLevel;
		}

		// Token: 0x060046C3 RID: 18115 RVA: 0x001C42D4 File Offset: 0x001C24D4
		public static int GetSpawnedItemQuality(Submarine submarine, Level level, Rand.RandSync randSync = Rand.RandSync.ServerAndClient)
		{
			if (((submarine != null) ? submarine.Info : null) == null || level == null || submarine.Info.Type == SubmarineType.Player)
			{
				return 0;
			}
			float difficultyFactor = MathHelper.Clamp(level.Difficulty, 0f, level.LevelData.Biome.ActualMaxDifficulty / 100f);
			if (level.Type == LevelData.LevelType.Outpost)
			{
				Location startLocation = level.StartLocation;
				bool flag;
				if (startLocation == null)
				{
					flag = false;
				}
				else
				{
					LocationType type = startLocation.Type;
					flag = (((type != null) ? new CharacterTeamType?(type.OutpostTeam) : null).GetValueOrDefault() == CharacterTeamType.FriendlyNPC);
				}
				if (flag)
				{
					difficultyFactor = 0f;
				}
			}
			return ToolBox.SelectWeightedRandom<int>(Enumerable.Range(0, 4), (int q) => Quality.<GetSpawnedItemQuality>g__GetCommonness|9_1(q, difficultyFactor), randSync);
		}

		// Token: 0x060046C4 RID: 18116 RVA: 0x001C4398 File Offset: 0x001C2598
		[CompilerGenerated]
		internal static float <GetSpawnedItemQuality>g__GetCommonness|9_1(int quality, float difficultyFactor)
		{
			float result;
			switch (quality)
			{
			case 0:
				result = 1f;
				break;
			case 1:
				result = MathHelper.Lerp(0f, 1f, difficultyFactor);
				break;
			case 2:
				result = MathHelper.Lerp(0f, 1f, Math.Max(difficultyFactor - 0.15f, 0f));
				break;
			case 3:
				result = MathHelper.Lerp(0f, 1f, Math.Max(difficultyFactor - 0.35f, 0f));
				break;
			default:
				result = 0f;
				break;
			}
			return result;
		}

		// Token: 0x04002222 RID: 8738
		public const int MaxQuality = 3;

		// Token: 0x04002223 RID: 8739
		public readonly Dictionary<Quality.StatType, float> statValues = new Dictionary<Quality.StatType, float>();

		// Token: 0x04002224 RID: 8740
		private int qualityLevel;

		// Token: 0x02000E2E RID: 3630
		public enum StatType
		{
			// Token: 0x040041FE RID: 16894
			Condition,
			// Token: 0x040041FF RID: 16895
			ExplosionRadius,
			// Token: 0x04004200 RID: 16896
			ExplosionDamage,
			// Token: 0x04004201 RID: 16897
			RepairSpeed,
			// Token: 0x04004202 RID: 16898
			RepairToolStructureRepairMultiplier,
			// Token: 0x04004203 RID: 16899
			RepairToolStructureDamageMultiplier,
			// Token: 0x04004204 RID: 16900
			RepairToolDeattachTimeMultiplier,
			// Token: 0x04004205 RID: 16901
			FirepowerMultiplier,
			// Token: 0x04004206 RID: 16902
			StrikingPowerMultiplier,
			// Token: 0x04004207 RID: 16903
			StrikingSpeedMultiplier,
			// Token: 0x04004208 RID: 16904
			FiringRateMultiplier
		}
	}
}
