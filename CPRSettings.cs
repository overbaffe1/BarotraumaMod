using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001C7 RID: 455
	internal class CPRSettings : Prefab
	{
		// Token: 0x17000D15 RID: 3349
		// (get) Token: 0x060031EC RID: 12780 RVA: 0x002069D3 File Offset: 0x00204BD3
		public static CPRSettings Active
		{
			get
			{
				return CPRSettings.Prefabs.ActivePrefab;
			}
		}

		// Token: 0x17000D16 RID: 3350
		// (get) Token: 0x060031ED RID: 12781 RVA: 0x002069DF File Offset: 0x00204BDF
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

		// Token: 0x060031EE RID: 12782 RVA: 0x00206A0C File Offset: 0x00204C0C
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

		// Token: 0x060031EF RID: 12783 RVA: 0x00206B7C File Offset: 0x00204D7C
		public override void Dispose()
		{
		}

		// Token: 0x040019F5 RID: 6645
		public static readonly PrefabSelector<CPRSettings> Prefabs = new PrefabSelector<CPRSettings>();

		// Token: 0x040019F6 RID: 6646
		public readonly float ReviveChancePerSkill;

		// Token: 0x040019F7 RID: 6647
		public readonly float ReviveChanceExponent;

		// Token: 0x040019F8 RID: 6648
		public readonly float ReviveChanceMin;

		// Token: 0x040019F9 RID: 6649
		public readonly float ReviveChanceMax;

		// Token: 0x040019FA RID: 6650
		public readonly float StabilizationPerSkill;

		// Token: 0x040019FB RID: 6651
		public readonly float StabilizationMin;

		// Token: 0x040019FC RID: 6652
		public readonly float StabilizationMax;

		// Token: 0x040019FD RID: 6653
		public readonly float DamageSkillThreshold;

		// Token: 0x040019FE RID: 6654
		public readonly float DamageSkillMultiplier;

		// Token: 0x040019FF RID: 6655
		private readonly string insufficientSkillAfflictionIdentifier;
	}
}
