using System;
using System.Collections.Immutable;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x020001F9 RID: 505
	internal class PreferredContainer
	{
		// Token: 0x060023CC RID: 9164 RVA: 0x000EF5E0 File Offset: 0x000ED7E0
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

		// Token: 0x040011A7 RID: 4519
		public readonly ImmutableHashSet<Identifier> Primary;

		// Token: 0x040011A8 RID: 4520
		public readonly ImmutableHashSet<Identifier> Secondary;

		// Token: 0x040011A9 RID: 4521
		public readonly float SpawnProbability;

		// Token: 0x040011AA RID: 4522
		public readonly float MaxCondition;

		// Token: 0x040011AB RID: 4523
		public readonly float MinCondition;

		// Token: 0x040011AC RID: 4524
		public readonly int MinAmount;

		// Token: 0x040011AD RID: 4525
		public readonly int MaxAmount;

		// Token: 0x040011AE RID: 4526
		public readonly int Amount;

		// Token: 0x040011AF RID: 4527
		public readonly bool CampaignOnly;

		// Token: 0x040011B0 RID: 4528
		public readonly bool NotCampaign;

		// Token: 0x040011B1 RID: 4529
		public readonly bool NotPvP;

		// Token: 0x040011B2 RID: 4530
		public readonly bool TransferOnlyOnePerContainer;

		// Token: 0x040011B3 RID: 4531
		public readonly bool AllowTransfersHere = true;

		// Token: 0x040011B4 RID: 4532
		public readonly float MinLevelDifficulty;

		// Token: 0x040011B5 RID: 4533
		public readonly float MaxLevelDifficulty;
	}
}
