using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001F2 RID: 498
	[NullableContext(1)]
	[Nullable(0)]
	internal abstract class TalentMigration
	{
		// Token: 0x060034B5 RID: 13493 RVA: 0x0020E0BD File Offset: 0x0020C2BD
		public bool TryApply(Version savedVersion, CharacterInfo info)
		{
			if (this.version <= savedVersion)
			{
				return false;
			}
			this.Apply(info);
			return true;
		}

		// Token: 0x060034B6 RID: 13494
		protected abstract void Apply(CharacterInfo info);

		// Token: 0x060034B7 RID: 13495 RVA: 0x0020E0D7 File Offset: 0x0020C2D7
		protected TalentMigration(Version targetVersion)
		{
			this.version = targetVersion;
		}

		// Token: 0x060034B8 RID: 13496 RVA: 0x0020E0E8 File Offset: 0x0020C2E8
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

		// Token: 0x060034B9 RID: 13497 RVA: 0x0020E174 File Offset: 0x0020C374
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

		// Token: 0x04001B6F RID: 7023
		private readonly Version version;

		// Token: 0x04001B70 RID: 7024
		private static readonly Dictionary<Identifier, TalentMigration.TalentMigrationCtor> migrationTemplates;

		// Token: 0x02000ED9 RID: 3801
		// (Invoke) Token: 0x06008777 RID: 34679
		[NullableContext(0)]
		private delegate TalentMigration TalentMigrationCtor(Version version, ContentXElement element);
	}
}
