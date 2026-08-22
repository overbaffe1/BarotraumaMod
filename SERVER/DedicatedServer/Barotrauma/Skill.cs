using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000D4 RID: 212
	internal class Skill
	{
		// Token: 0x170006D3 RID: 1747
		// (get) Token: 0x0600175C RID: 5980 RVA: 0x000C2BD4 File Offset: 0x000C0DD4
		// (set) Token: 0x0600175D RID: 5981 RVA: 0x000C2BDC File Offset: 0x000C0DDC
		public float HighestLevelDuringRound { get; private set; }

		// Token: 0x170006D4 RID: 1748
		// (get) Token: 0x0600175E RID: 5982 RVA: 0x000C2BE5 File Offset: 0x000C0DE5
		// (set) Token: 0x0600175F RID: 5983 RVA: 0x000C2BED File Offset: 0x000C0DED
		public float Level
		{
			get
			{
				return this.level;
			}
			set
			{
				this.HighestLevelDuringRound = MathHelper.Max(value, this.HighestLevelDuringRound);
				this.level = value;
			}
		}

		// Token: 0x170006D5 RID: 1749
		// (get) Token: 0x06001760 RID: 5984 RVA: 0x000C2C08 File Offset: 0x000C0E08
		// (set) Token: 0x06001761 RID: 5985 RVA: 0x000C2C10 File Offset: 0x000C0E10
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x06001762 RID: 5986 RVA: 0x000C2C1C File Offset: 0x000C0E1C
		public void IncreaseSkill(float value, bool canIncreasePastDefaultMaximumSkill)
		{
			float currentMaximum = canIncreasePastDefaultMaximumSkill ? SkillSettings.Current.MaximumSkillWithTalents : 100f;
			if (this.Level > currentMaximum && value > 0f)
			{
				return;
			}
			this.Level = MathHelper.Clamp(this.level + value, 0f, currentMaximum);
		}

		// Token: 0x170006D6 RID: 1750
		// (get) Token: 0x06001763 RID: 5987 RVA: 0x000C2C6C File Offset: 0x000C0E6C
		public Sprite Icon
		{
			get
			{
				JobPrefab jobPrefab;
				if (this.iconJobId.IsEmpty || !JobPrefab.Prefabs.TryGet(this.iconJobId, out jobPrefab))
				{
					return null;
				}
				return jobPrefab.Icon;
			}
		}

		// Token: 0x06001764 RID: 5988 RVA: 0x000C2CA4 File Offset: 0x000C0EA4
		public Skill(SkillPrefab prefab, bool isPvP, Rand.RandSync randSync)
		{
			this.Identifier = prefab.Identifier;
			Range<float> levelRange = prefab.GetLevelRange(isPvP);
			this.Level = Rand.Range(levelRange.Start, levelRange.End, randSync);
			this.iconJobId = this.GetIconJobId();
			this.PriceMultiplier = prefab.PriceMultiplier;
			this.DisplayName = TextManager.Get("SkillName." + this.Identifier.ToString());
		}

		// Token: 0x06001765 RID: 5989 RVA: 0x000C2D30 File Offset: 0x000C0F30
		public Skill(Identifier identifier, float level)
		{
			this.Identifier = identifier;
			this.Level = level;
			this.iconJobId = this.GetIconJobId();
			this.DisplayName = TextManager.Get("SkillName." + this.Identifier.ToString());
		}

		// Token: 0x06001766 RID: 5990 RVA: 0x000C2D90 File Offset: 0x000C0F90
		private Identifier GetIconJobId()
		{
			Identifier jobId = Identifier.Empty;
			if (this.Identifier == "electrical")
			{
				jobId = "engineer".ToIdentifier();
			}
			else if (this.Identifier == "helm")
			{
				jobId = "captain".ToIdentifier();
			}
			else if (this.Identifier == "mechanical")
			{
				jobId = "mechanic".ToIdentifier();
			}
			else if (this.Identifier == "medical")
			{
				jobId = "medicaldoctor".ToIdentifier();
			}
			else if (this.Identifier == "weapons")
			{
				jobId = "securityofficer".ToIdentifier();
			}
			return jobId;
		}

		// Token: 0x04000B4C RID: 2892
		public readonly Identifier Identifier;

		// Token: 0x04000B4D RID: 2893
		public const float DefaultMaximumSkill = 100f;

		// Token: 0x04000B4E RID: 2894
		private float level;

		// Token: 0x04000B51 RID: 2897
		private readonly Identifier iconJobId;

		// Token: 0x04000B52 RID: 2898
		public readonly float PriceMultiplier = 1f;
	}
}
