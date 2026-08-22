using System;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020002E3 RID: 739
	internal class PreferredContainer
	{
		// Token: 0x06003D72 RID: 15730 RVA: 0x0022ED44 File Offset: 0x0022CF44
		public PreferredContainer(XElement element)
		{
			this.Primary = element.GetAttributeIdentifierArray("primary", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			this.Secondary = element.GetAttributeIdentifierArray("secondary", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
			this.SpawnProbability = element.GetAttributeFloat("spawnprobability", 0f);
			this.MinAmount = element.GetAttributeInt("minamount", 0);
			this.MaxAmount = Math.Max(this.MinAmount, element.GetAttributeInt("maxamount", 0));
			this.Amount = element.GetAttributeInt("amount", 0);
			this.MaxCondition = element.GetAttributeFloat("maxcondition", 100f);
			this.MinCondition = element.GetAttributeFloat("mincondition", 0f);
			this.CampaignOnly = element.GetAttributeBool("campaignonly", this.CampaignOnly);
			this.NotCampaign = element.GetAttributeBool("notcampaign", this.NotCampaign);
			this.NotPvP = element.GetAttributeBool("notpvp", this.NotPvP);
			this.TransferOnlyOnePerContainer = element.GetAttributeBool("TransferOnlyOnePerContainer", this.TransferOnlyOnePerContainer);
			this.AllowTransfersHere = element.GetAttributeBool("AllowTransfersHere", this.AllowTransfersHere);
			this.MinLevelDifficulty = element.GetAttributeFloat("MinLevelDifficulty", float.MinValue);
			this.MaxLevelDifficulty = element.GetAttributeFloat("MaxLevelDifficulty", float.MaxValue);
			if (element.GetAttribute("spawnprobability", StringComparison.OrdinalIgnoreCase) == null)
			{
				if (this.MaxAmount > 0 || this.Amount > 0)
				{
					this.SpawnProbability = 1f;
					return;
				}
			}
			else if (element.GetAttribute("minamount", StringComparison.OrdinalIgnoreCase) == null && element.GetAttribute("maxamount", StringComparison.OrdinalIgnoreCase) == null && element.GetAttribute("amount", StringComparison.OrdinalIgnoreCase) == null)
			{
				this.MinAmount = (this.MaxAmount = (this.Amount = 1));
				this.SpawnProbability = element.GetAttributeFloat("spawnprobability", 0f);
			}
		}

		// Token: 0x04002030 RID: 8240
		public readonly ImmutableHashSet<Identifier> Primary;

		// Token: 0x04002031 RID: 8241
		public readonly ImmutableHashSet<Identifier> Secondary;

		// Token: 0x04002032 RID: 8242
		public readonly float SpawnProbability;

		// Token: 0x04002033 RID: 8243
		public readonly float MaxCondition;

		// Token: 0x04002034 RID: 8244
		public readonly float MinCondition;

		// Token: 0x04002035 RID: 8245
		public readonly int MinAmount;

		// Token: 0x04002036 RID: 8246
		public readonly int MaxAmount;

		// Token: 0x04002037 RID: 8247
		public readonly int Amount;

		// Token: 0x04002038 RID: 8248
		public readonly bool CampaignOnly;

		// Token: 0x04002039 RID: 8249
		public readonly bool NotCampaign;

		// Token: 0x0400203A RID: 8250
		public readonly bool NotPvP;

		// Token: 0x0400203B RID: 8251
		public readonly bool TransferOnlyOnePerContainer;

		// Token: 0x0400203C RID: 8252
		public readonly bool AllowTransfersHere = true;

		// Token: 0x0400203D RID: 8253
		public readonly float MinLevelDifficulty;

		// Token: 0x0400203E RID: 8254
		public readonly float MaxLevelDifficulty;
	}
}
