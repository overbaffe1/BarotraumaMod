using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000383 RID: 899
	internal readonly struct UpgradeMaxLevelMod
	{
		// Token: 0x06004420 RID: 17440 RVA: 0x00256A48 File Offset: 0x00254C48
		public int GetLevelAfter(int level)
		{
			int result;
			switch (this.type)
			{
			case UpgradeMaxLevelMod.MaxLevelModType.Invalid:
				result = level;
				break;
			case UpgradeMaxLevelMod.MaxLevelModType.Increase:
				result = level + this.value;
				break;
			case UpgradeMaxLevelMod.MaxLevelModType.Set:
				result = this.value;
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			return result;
		}

		// Token: 0x06004421 RID: 17441 RVA: 0x00256A90 File Offset: 0x00254C90
		public bool AppliesTo(SubmarineClass subClass, int subTier)
		{
			if (this.type == UpgradeMaxLevelMod.MaxLevelModType.Invalid)
			{
				return false;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMetadata campaignMetadata;
			if (gameSession == null)
			{
				campaignMetadata = null;
			}
			else
			{
				CampaignMode campaign = gameSession.Campaign;
				campaignMetadata = ((campaign != null) ? campaign.CampaignMetadata : null);
			}
			CampaignMetadata metadata = campaignMetadata;
			if (metadata != null)
			{
				int modifier = metadata.GetInt(new Identifier("tiermodifieroverride"), new int?(0));
				subTier = Math.Max(modifier, subTier);
			}
			int tier;
			if (this.tierOrClass.TryGet(out tier))
			{
				return subTier == tier;
			}
			SubmarineClass targetClass;
			return this.tierOrClass.TryGet(out targetClass) && subClass == targetClass;
		}

		// Token: 0x06004422 RID: 17442 RVA: 0x00256B14 File Offset: 0x00254D14
		[NullableContext(1)]
		public UpgradeMaxLevelMod(ContentXElement element)
		{
			bool isValid = true;
			string key = "class";
			SubmarineClass submarineClass = SubmarineClass.Undefined;
			SubmarineClass subClass = element.GetAttributeEnum<SubmarineClass>(key, submarineClass);
			int tier = element.GetAttributeInt("tier", 0);
			if (subClass != SubmarineClass.Undefined)
			{
				this.tierOrClass = subClass;
			}
			else
			{
				this.tierOrClass = tier;
			}
			string stringValue = element.GetAttributeString("level", null) ?? string.Empty;
			this.value = 0;
			if (string.IsNullOrWhiteSpace(stringValue))
			{
				isValid = false;
			}
			char firstChar = stringValue[0];
			int intValue;
			if (!int.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out intValue))
			{
				isValid = false;
			}
			this.value = intValue;
			if (firstChar.Equals('+') || firstChar.Equals('-'))
			{
				this.type = UpgradeMaxLevelMod.MaxLevelModType.Increase;
			}
			else
			{
				this.type = UpgradeMaxLevelMod.MaxLevelModType.Set;
			}
			if (!isValid)
			{
				this.type = UpgradeMaxLevelMod.MaxLevelModType.Invalid;
			}
		}

		// Token: 0x040023BE RID: 9150
		[Nullable(1)]
		private readonly Either<SubmarineClass, int> tierOrClass;

		// Token: 0x040023BF RID: 9151
		private readonly int value;

		// Token: 0x040023C0 RID: 9152
		private readonly UpgradeMaxLevelMod.MaxLevelModType type;

		// Token: 0x020010AB RID: 4267
		private enum MaxLevelModType
		{
			// Token: 0x0400595E RID: 22878
			Invalid,
			// Token: 0x0400595F RID: 22879
			Increase,
			// Token: 0x04005960 RID: 22880
			Set
		}
	}
}
