using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020000F6 RID: 246
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class TalentMigration
	{
		// Token: 0x060019B4 RID: 6580 RVA: 0x000C76B9 File Offset: 0x000C58B9
		public bool TryApply(Version savedVersion, CharacterInfo info)
		{
			if (this.version <= savedVersion)
			{
				return false;
			}
			this.Apply(info);
			return true;
		}

		// Token: 0x060019B5 RID: 6581
		protected abstract void Apply(CharacterInfo info);

		// Token: 0x060019B6 RID: 6582 RVA: 0x000C76D3 File Offset: 0x000C58D3
		protected TalentMigration(Version targetVersion)
		{
			this.version = targetVersion;
		}

		// Token: 0x060019B7 RID: 6583 RVA: 0x000C76E4 File Offset: 0x000C58E4
		public static TalentMigration FromXML(ContentXElement element)
		{
			Version version = element.GetAttributeVersion("version", null);
			if (version == null)
			{
				throw new Exception("Talent migration version not defined.");
			}
			Identifier name = element.Name.ToString().ToIdentifier();
			TalentMigration.TalentMigrationCtor ctor;
			if (!TalentMigration.migrationTemplates.TryGetValue(name, out ctor))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(32, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Unknown talent migration type: ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(name);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			return ctor(version, element);
		}

		// Token: 0x060019B8 RID: 6584 RVA: 0x000C7770 File Offset: 0x000C5970
		// Note: this type is marked as 'beforefieldinit'.
		static TalentMigration()
		{
			Dictionary<Identifier, TalentMigration.TalentMigrationCtor> dictionary = new Dictionary<Identifier, TalentMigration.TalentMigrationCtor>();
			Identifier key = new Identifier("AddStat");
			dictionary[key] = ((Version version, ContentXElement element) => new TalentMigrationAddStat(version, element));
			Identifier key2 = new Identifier("UpdateStatIdentifier");
			dictionary[key2] = ((Version version, ContentXElement element) => new TalentMigrationUpdateStatIdentifier(version, element));
			TalentMigration.migrationTemplates = dictionary;
		}

		// Token: 0x04000C49 RID: 3145
		private readonly Version version;

		// Token: 0x04000C4A RID: 3146
		private static readonly Dictionary<Identifier, TalentMigration.TalentMigrationCtor> migrationTemplates;

		// Token: 0x020008C0 RID: 2240
		// (Invoke) Token: 0x06005798 RID: 22424
		[NullableContext(0)]
		private delegate TalentMigration TalentMigrationCtor(Version version, ContentXElement element);
	}
}
