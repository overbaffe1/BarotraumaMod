using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001D1 RID: 465
	internal class Skill
	{
		// Token: 0x17000D48 RID: 3400
		// (get) Token: 0x06003271 RID: 12913 RVA: 0x00209920 File Offset: 0x00207B20
		// (set) Token: 0x06003272 RID: 12914 RVA: 0x00209928 File Offset: 0x00207B28
		public float HighestLevelDuringRound { get; private set; }

		// Token: 0x17000D49 RID: 3401
		// (get) Token: 0x06003273 RID: 12915 RVA: 0x00209931 File Offset: 0x00207B31
		// (set) Token: 0x06003274 RID: 12916 RVA: 0x00209939 File Offset: 0x00207B39
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

		// Token: 0x17000D4A RID: 3402
		// (get) Token: 0x06003275 RID: 12917 RVA: 0x00209954 File Offset: 0x00207B54
		// (set) Token: 0x06003276 RID: 12918 RVA: 0x0020995C File Offset: 0x00207B5C
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x06003277 RID: 12919 RVA: 0x00209968 File Offset: 0x00207B68
		public void IncreaseSkill(float value, bool canIncreasePastDefaultMaximumSkill)
		{
			float currentMaximum = canIncreasePastDefaultMaximumSkill ? SkillSettings.Current.MaximumSkillWithTalents : 100f;
			if (this.Level > currentMaximum && value > 0f)
			{
				return;
			}
			this.Level = MathHelper.Clamp(this.level + value, 0f, currentMaximum);
		}

		// Token: 0x17000D4B RID: 3403
		// (get) Token: 0x06003278 RID: 12920 RVA: 0x002099B8 File Offset: 0x00207BB8
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

		// Token: 0x06003279 RID: 12921 RVA: 0x002099F0 File Offset: 0x00207BF0
		public Skill(SkillPrefab prefab, bool isPvP, Rand.RandSync randSync)
		{
			this.Identifier = prefab.Identifier;
			Range<float> levelRange = prefab.GetLevelRange(isPvP);
			this.Level = Rand.Range(levelRange.Start, levelRange.End, randSync);
			this.iconJobId = this.GetIconJobId();
			this.PriceMultiplier = prefab.PriceMultiplier;
			this.DisplayName = TextManager.Get("SkillName." + this.Identifier.ToString());
		}

		// Token: 0x0600327A RID: 12922 RVA: 0x00209A7C File Offset: 0x00207C7C
		public Skill(Identifier identifier, float level)
		{
			this.Identifier = identifier;
			this.Level = level;
			this.iconJobId = this.GetIconJobId();
			this.DisplayName = TextManager.Get("SkillName." + this.Identifier.ToString());
		}

		// Token: 0x0600327B RID: 12923 RVA: 0x00209ADC File Offset: 0x00207CDC
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

		// Token: 0x04001A78 RID: 6776
		public readonly Identifier Identifier;

		// Token: 0x04001A79 RID: 6777
		public const float DefaultMaximumSkill = 100f;

		// Token: 0x04001A7A RID: 6778
		private float level;

		// Token: 0x04001A7D RID: 6781
		private readonly Identifier iconJobId;

		// Token: 0x04001A7E RID: 6782
		public readonly float PriceMultiplier = 1f;
	}
}
