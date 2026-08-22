using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020000F7 RID: 247
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class TalentMigrationAddStat : TalentMigration
	{
		// Token: 0x170007B6 RID: 1974
		// (get) Token: 0x060019B9 RID: 6585 RVA: 0x000C77CD File Offset: 0x000C59CD
		// (set) Token: 0x060019BA RID: 6586 RVA: 0x000C77D5 File Offset: 0x000C59D5
		[Serialize(StatTypes.None, IsPropertySaveable.Yes, "", "", false)]
		public StatTypes StatType { get; set; }

		// Token: 0x170007B7 RID: 1975
		// (get) Token: 0x060019BB RID: 6587 RVA: 0x000C77DE File Offset: 0x000C59DE
		// (set) Token: 0x060019BC RID: 6588 RVA: 0x000C77E6 File Offset: 0x000C59E6
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier StatIdentifier { get; set; }

		// Token: 0x170007B8 RID: 1976
		// (get) Token: 0x060019BD RID: 6589 RVA: 0x000C77EF File Offset: 0x000C59EF
		// (set) Token: 0x060019BE RID: 6590 RVA: 0x000C77F7 File Offset: 0x000C59F7
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Value { get; set; }

		// Token: 0x170007B9 RID: 1977
		// (get) Token: 0x060019BF RID: 6591 RVA: 0x000C7800 File Offset: 0x000C5A00
		// (set) Token: 0x060019C0 RID: 6592 RVA: 0x000C7808 File Offset: 0x000C5A08
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool RemoveOnDeath { get; set; }

		// Token: 0x060019C1 RID: 6593 RVA: 0x000C7811 File Offset: 0x000C5A11
		public TalentMigrationAddStat(Version targetVersion, ContentXElement element) : base(targetVersion)
		{
			SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060019C2 RID: 6594 RVA: 0x000C7827 File Offset: 0x000C5A27
		protected override void Apply(CharacterInfo info)
		{
			info.ChangeSavedStatValue(this.StatType, this.Value, this.StatIdentifier, this.RemoveOnDeath, float.MaxValue, false);
		}
	}
}
