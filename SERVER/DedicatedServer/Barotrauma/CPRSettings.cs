using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000C6 RID: 198
	internal class CPRSettings : Prefab
	{
		// Token: 0x1700066F RID: 1647
		// (get) Token: 0x0600162C RID: 5676 RVA: 0x000BBB07 File Offset: 0x000B9D07
		public static CPRSettings Active
		{
			get
			{
				return CPRSettings.Prefabs.ActivePrefab;
			}
		}

		// Token: 0x17000670 RID: 1648
		// (get) Token: 0x0600162D RID: 5677 RVA: 0x000BBB13 File Offset: 0x000B9D13
		public AfflictionPrefab InsufficientSkillAffliction
		{
			get
			{
				if (!AfflictionPrefab.Prefabs.ContainsKey(this.insufficientSkillAfflictionIdentifier))
				{
					return AfflictionPrefab.InternalDamage;
				}
				return AfflictionPrefab.Prefabs[this.insufficientSkillAfflictionIdentifier];
			}
		}

		// Token: 0x0600162E RID: 5678 RVA: 0x000BBB40 File Offset: 0x000B9D40
		public CPRSettings(XElement element, AfflictionsFile file) : base(file, file.Path.Value.ToIdentifier())
		{
			this.ReviveChancePerSkill = Math.Max(element.GetAttributeFloat("revivechanceperskill", 0.01f), 0f);
			this.ReviveChanceExponent = Math.Max(element.GetAttributeFloat("revivechanceexponent", 2f), 0f);
			this.ReviveChanceMin = MathHelper.Clamp(element.GetAttributeFloat("revivechancemin", 0.05f), 0f, 1f);
			this.ReviveChanceMax = MathHelper.Clamp(element.GetAttributeFloat("revivechancemax", 0.9f), this.ReviveChanceMin, 1f);
			this.StabilizationPerSkill = Math.Max(element.GetAttributeFloat("stabilizationperskill", 0.01f), 0f);
			this.StabilizationMin = MathHelper.Max(element.GetAttributeFloat("stabilizationmin", 0.05f), 0f);
			this.StabilizationMax = MathHelper.Max(element.GetAttributeFloat("stabilizationmax", 2f), this.StabilizationMin);
			this.DamageSkillThreshold = MathHelper.Clamp(element.GetAttributeFloat("damageskillthreshold", 40f), 0f, 100f);
			this.DamageSkillMultiplier = MathHelper.Clamp(element.GetAttributeFloat("damageskillmultiplier", 0.1f), 0f, 100f);
			this.insufficientSkillAfflictionIdentifier = element.GetAttributeString("insufficientskillaffliction", "");
		}

		// Token: 0x0600162F RID: 5679 RVA: 0x000BBCB0 File Offset: 0x000B9EB0
		public override void Dispose()
		{
		}

		// Token: 0x04000A8A RID: 2698
		public static readonly PrefabSelector<CPRSettings> Prefabs = new PrefabSelector<CPRSettings>();

		// Token: 0x04000A8B RID: 2699
		public readonly float ReviveChancePerSkill;

		// Token: 0x04000A8C RID: 2700
		public readonly float ReviveChanceExponent;

		// Token: 0x04000A8D RID: 2701
		public readonly float ReviveChanceMin;

		// Token: 0x04000A8E RID: 2702
		public readonly float ReviveChanceMax;

		// Token: 0x04000A8F RID: 2703
		public readonly float StabilizationPerSkill;

		// Token: 0x04000A90 RID: 2704
		public readonly float StabilizationMin;

		// Token: 0x04000A91 RID: 2705
		public readonly float StabilizationMax;

		// Token: 0x04000A92 RID: 2706
		public readonly float DamageSkillThreshold;

		// Token: 0x04000A93 RID: 2707
		public readonly float DamageSkillMultiplier;

		// Token: 0x04000A94 RID: 2708
		private readonly string insufficientSkillAfflictionIdentifier;
	}
}
