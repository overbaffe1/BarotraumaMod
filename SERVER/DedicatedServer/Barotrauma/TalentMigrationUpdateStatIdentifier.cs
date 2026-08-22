using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020000F8 RID: 248
	[NullableContext(1)]
	[Nullable(0)]
	internal class TalentMigrationUpdateStatIdentifier : TalentMigration
	{
		// Token: 0x170007BA RID: 1978
		// (get) Token: 0x060019C3 RID: 6595 RVA: 0x000C784D File Offset: 0x000C5A4D
		// (set) Token: 0x060019C4 RID: 6596 RVA: 0x000C7855 File Offset: 0x000C5A55
		[Serialize("", IsPropertySaveable.Yes, "The old identifier to update.", "", false)]
		public Identifier Old { get; set; }

		// Token: 0x170007BB RID: 1979
		// (get) Token: 0x060019C5 RID: 6597 RVA: 0x000C785E File Offset: 0x000C5A5E
		// (set) Token: 0x060019C6 RID: 6598 RVA: 0x000C7866 File Offset: 0x000C5A66
		[Serialize("", IsPropertySaveable.Yes, "What to change the old identifier to.", "", false)]
		public Identifier New { get; set; }

		// Token: 0x060019C7 RID: 6599 RVA: 0x000C786F File Offset: 0x000C5A6F
		public TalentMigrationUpdateStatIdentifier(Version targetVersion, ContentXElement element) : base(targetVersion)
		{
			SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060019C8 RID: 6600 RVA: 0x000C7888 File Offset: 0x000C5A88
		protected override void Apply(CharacterInfo info)
		{
			foreach (SavedStatValue statValue in info.SavedStatValues.Values.SelectMany((List<SavedStatValue> s) => s))
			{
				Identifier statIdentifier = statValue.StatIdentifier;
				Identifier old = this.Old;
				if (!(statIdentifier != old))
				{
					statValue.StatIdentifier = this.New;
				}
			}
		}
	}
}
