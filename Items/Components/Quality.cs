using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma.Items.Components
{
	// Token: 0x020005D0 RID: 1488
	internal class Quality : ItemComponent
	{
		// Token: 0x06005EF2 RID: 24306 RVA: 0x00317A1C File Offset: 0x00315C1C
		public override void AddTooltipInfo(ref LocalizedString name, ref LocalizedString description)
		{
			foreach (KeyValuePair<Quality.StatType, float> statValue in this.statValues)
			{
				int roundedValue = (int)Math.Round((double)(statValue.Value * (float)this.qualityLevel * 100f));
				if (roundedValue == 0)
				{
					break;
				}
				string colorStr = XMLExtensions.ColorToString(GUIStyle.Green);
				LocalizedString left = description;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(24, 3);
				defaultInterpolatedStringHandler.AppendLiteral("\n  ‖color:");
				defaultInterpolatedStringHandler.AppendFormatted(colorStr);
				defaultInterpolatedStringHandler.AppendLiteral("‖");
				defaultInterpolatedStringHandler.AppendFormatted(roundedValue.ToString("+0;-#"));
				defaultInterpolatedStringHandler.AppendLiteral("%‖color:end‖ ");
				defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get("qualitystattypenames." + statValue.Key.ToString()).Fallback(statValue.Key.ToString(), true));
				description = left + defaultInterpolatedStringHandler.ToStringAndClear();
			}
		}

		// Token: 0x170017F1 RID: 6129
		// (get) Token: 0x06005EF3 RID: 24307 RVA: 0x00317B50 File Offset: 0x00315D50
		// (set) Token: 0x06005EF4 RID: 24308 RVA: 0x00317B58 File Offset: 0x00315D58
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

		// Token: 0x06005EF5 RID: 24309 RVA: 0x00317BBC File Offset: 0x00315DBC
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

		// Token: 0x06005EF6 RID: 24310 RVA: 0x00317CEC File Offset: 0x00315EEC
		public float GetValue(Quality.StatType statType)
		{
			if (!this.statValues.ContainsKey(statType))
			{
				return 0f;
			}
			return this.statValues[statType] * (float)this.qualityLevel;
		}

		// Token: 0x06005EF7 RID: 24311 RVA: 0x00317D18 File Offset: 0x00315F18
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
			return ToolBox.SelectWeightedRandom<int>(Enumerable.Range(0, 4), (int q) => Quality.<GetSpawnedItemQuality>g__GetCommonness|10_1(q, difficultyFactor), randSync);
		}

		// Token: 0x06005EF8 RID: 24312 RVA: 0x00317DDC File Offset: 0x00315FDC
		[CompilerGenerated]
		internal static float <GetSpawnedItemQuality>g__GetCommonness|10_1(int quality, float difficultyFactor)
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

		// Token: 0x0400310E RID: 12558
		public const int MaxQuality = 3;

		// Token: 0x0400310F RID: 12559
		public readonly Dictionary<Quality.StatType, float> statValues = new Dictionary<Quality.StatType, float>();

		// Token: 0x04003110 RID: 12560
		private int qualityLevel;

		// Token: 0x02001446 RID: 5190
		public enum StatType
		{
			// Token: 0x04006523 RID: 25891
			Condition,
			// Token: 0x04006524 RID: 25892
			ExplosionRadius,
			// Token: 0x04006525 RID: 25893
			ExplosionDamage,
			// Token: 0x04006526 RID: 25894
			RepairSpeed,
			// Token: 0x04006527 RID: 25895
			RepairToolStructureRepairMultiplier,
			// Token: 0x04006528 RID: 25896
			RepairToolStructureDamageMultiplier,
			// Token: 0x04006529 RID: 25897
			RepairToolDeattachTimeMultiplier,
			// Token: 0x0400652A RID: 25898
			FirepowerMultiplier,
			// Token: 0x0400652B RID: 25899
			StrikingPowerMultiplier,
			// Token: 0x0400652C RID: 25900
			StrikingSpeedMultiplier,
			// Token: 0x0400652D RID: 25901
			FiringRateMultiplier
		}
	}
}
