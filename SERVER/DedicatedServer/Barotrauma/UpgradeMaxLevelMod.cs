using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002B7 RID: 695
	internal readonly struct UpgradeMaxLevelMod
	{
		// Token: 0x06002F88 RID: 12168 RVA: 0x0013C504 File Offset: 0x0013A704
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

		// Token: 0x06002F89 RID: 12169 RVA: 0x0013C54C File Offset: 0x0013A74C
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

		// Token: 0x06002F8A RID: 12170 RVA: 0x0013C5D0 File Offset: 0x0013A7D0
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

		// Token: 0x040017DB RID: 6107
		[Nullable(1)]
		private readonly Either<SubmarineClass, int> tierOrClass;

		// Token: 0x040017DC RID: 6108
		private readonly int value;

		// Token: 0x040017DD RID: 6109
		private readonly UpgradeMaxLevelMod.MaxLevelModType type;

		// Token: 0x02000B4F RID: 2895
		private enum MaxLevelModType
		{
			// Token: 0x0400392A RID: 14634
			Invalid,
			// Token: 0x0400392B RID: 14635
			Increase,
			// Token: 0x0400392C RID: 14636
			Set
		}
	}
}
