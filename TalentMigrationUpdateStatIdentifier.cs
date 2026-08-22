using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001F4 RID: 500
	[NullableContext(1)]
	[Nullable(0)]
	internal class TalentMigrationUpdateStatIdentifier : TalentMigration
	{
		// Token: 0x17000E22 RID: 3618
		// (get) Token: 0x060034C4 RID: 13508 RVA: 0x0020E251 File Offset: 0x0020C451
		// (set) Token: 0x060034C5 RID: 13509 RVA: 0x0020E259 File Offset: 0x0020C459
		[Serialize("", IsPropertySaveable.Yes, "The old identifier to update.", "", false)]
		public Identifier Old { get; set; }

		// Token: 0x17000E23 RID: 3619
		// (get) Token: 0x060034C6 RID: 13510 RVA: 0x0020E262 File Offset: 0x0020C462
		// (set) Token: 0x060034C7 RID: 13511 RVA: 0x0020E26A File Offset: 0x0020C46A
		[Serialize("", IsPropertySaveable.Yes, "What to change the old identifier to.", "", false)]
		public Identifier New { get; set; }

		// Token: 0x060034C8 RID: 13512 RVA: 0x0020E273 File Offset: 0x0020C473
		public TalentMigrationUpdateStatIdentifier(Version targetVersion, ContentXElement element) : base(targetVersion)
		{
			SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060034C9 RID: 13513 RVA: 0x0020E28C File Offset: 0x0020C48C
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
