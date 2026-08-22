using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001F3 RID: 499
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class TalentMigrationAddStat : TalentMigration
	{
		// Token: 0x17000E1E RID: 3614
		// (get) Token: 0x060034BA RID: 13498 RVA: 0x0020E1D1 File Offset: 0x0020C3D1
		// (set) Token: 0x060034BB RID: 13499 RVA: 0x0020E1D9 File Offset: 0x0020C3D9
		[Serialize(StatTypes.None, IsPropertySaveable.Yes, "", "", false)]
		public StatTypes StatType { get; set; }

		// Token: 0x17000E1F RID: 3615
		// (get) Token: 0x060034BC RID: 13500 RVA: 0x0020E1E2 File Offset: 0x0020C3E2
		// (set) Token: 0x060034BD RID: 13501 RVA: 0x0020E1EA File Offset: 0x0020C3EA
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		public Identifier StatIdentifier { get; set; }

		// Token: 0x17000E20 RID: 3616
		// (get) Token: 0x060034BE RID: 13502 RVA: 0x0020E1F3 File Offset: 0x0020C3F3
		// (set) Token: 0x060034BF RID: 13503 RVA: 0x0020E1FB File Offset: 0x0020C3FB
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		public float Value { get; set; }

		// Token: 0x17000E21 RID: 3617
		// (get) Token: 0x060034C0 RID: 13504 RVA: 0x0020E204 File Offset: 0x0020C404
		// (set) Token: 0x060034C1 RID: 13505 RVA: 0x0020E20C File Offset: 0x0020C40C
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		public bool RemoveOnDeath { get; set; }

		// Token: 0x060034C2 RID: 13506 RVA: 0x0020E215 File Offset: 0x0020C415
		public TalentMigrationAddStat(Version targetVersion, ContentXElement element) : base(targetVersion)
		{
			SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060034C3 RID: 13507 RVA: 0x0020E22B File Offset: 0x0020C42B
		protected override void Apply(CharacterInfo info)
		{
			info.ChangeSavedStatValue(this.StatType, this.Value, this.StatIdentifier, this.RemoveOnDeath, float.MaxValue, false);
		}
	}
}
