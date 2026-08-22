using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020000F9 RID: 249
	internal class TalentPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x170007BC RID: 1980
		// (get) Token: 0x060019C9 RID: 6601 RVA: 0x000C791C File Offset: 0x000C5B1C
		public string OriginalName
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x170007BD RID: 1981
		// (get) Token: 0x060019CA RID: 6602 RVA: 0x000C7929 File Offset: 0x000C5B29
		// (set) Token: 0x060019CB RID: 6603 RVA: 0x000C7931 File Offset: 0x000C5B31
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x170007BE RID: 1982
		// (get) Token: 0x060019CC RID: 6604 RVA: 0x000C793A File Offset: 0x000C5B3A
		// (set) Token: 0x060019CD RID: 6605 RVA: 0x000C7942 File Offset: 0x000C5B42
		public LocalizedString Description { get; private set; }

		// Token: 0x170007BF RID: 1983
		// (get) Token: 0x060019CE RID: 6606 RVA: 0x000C794B File Offset: 0x000C5B4B
		// (set) Token: 0x060019CF RID: 6607 RVA: 0x000C7953 File Offset: 0x000C5B53
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x060019D0 RID: 6608 RVA: 0x000C795C File Offset: 0x000C5B5C
		public TalentPrefab(ContentXElement element, TalentsFile file) : base(file, element.GetAttributeIdentifier("identifier", Identifier.Empty))
		{
			this.ConfigElement = element;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
			defaultInterpolatedStringHandler.AppendLiteral("talentname.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			this.DisplayName = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear()).Fallback(this.Identifier.Value, true);
			this.AbilityEffectsStackWithSameTalent = element.GetAttributeBool("abilityeffectsstackwithsametalent", true);
			Identifier trackedStat = element.GetAttributeIdentifier("trackedstat", Identifier.Empty);
			int trackedMax = element.GetAttributeInt("trackedmax", 100);
			Option<ValueTuple<Identifier, int>> trackedStat2;
			if (trackedStat.IsEmpty)
			{
				Option.UnspecifiedNone none = Option.None;
				trackedStat2 = none;
			}
			else
			{
				trackedStat2 = Option.Some<ValueTuple<Identifier, int>>(new ValueTuple<Identifier, int>(trackedStat, trackedMax));
			}
			this.TrackedStat = trackedStat2;
			Identifier nameIdentifier = element.GetAttributeIdentifier("nameidentifier", Identifier.Empty);
			if (!nameIdentifier.IsEmpty)
			{
				this.DisplayName = TextManager.Get(nameIdentifier).Fallback(this.Identifier.Value, true);
			}
			this.IsHiddenExtraTalent = element.GetAttributeBool("ishiddenextratalent", false);
			this.Description = string.Empty;
			ImmutableHashSet<TalentMigration>.Builder migrations = ImmutableHashSet.CreateBuilder<TalentMigration>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "icon"))
				{
					if (!(a == "description"))
					{
						if (a == "migrations")
						{
							foreach (ContentXElement migrationElement in subElement.Elements())
							{
								try
								{
									TalentMigration migration = TalentMigration.FromXML(migrationElement);
									migrations.Add(migration);
								}
								catch (Exception e)
								{
									DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(51, 1);
									defaultInterpolatedStringHandler2.AppendLiteral("Error while loading talent migration for talent \"");
									defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
									defaultInterpolatedStringHandler2.AppendLiteral("\".");
									DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), e, (element != null) ? element.ContentPackage : null, false, false);
								}
							}
						}
					}
					else
					{
						LocalizedString tempDescription = this.Description;
						TextManager.ConstructDescription(ref tempDescription, subElement, null);
						this.Description = tempDescription;
					}
				}
				else
				{
					this.Icon = new Sprite(subElement, "", "", false, 1f);
				}
			}
			this.Migrations = migrations.ToImmutable();
			if (element.GetAttribute("description") != null)
			{
				string description = element.GetAttributeString("description", string.Empty);
				this.Description = this.Description.Fallback(TextManager.Get(description), true).Fallback(description, true);
				return;
			}
			LocalizedString description2 = this.Description;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("talentdescription.");
			defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.Identifier);
			this.Description = description2.Fallback(TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear()), true).Fallback(string.Empty, true);
		}

		// Token: 0x060019D1 RID: 6609 RVA: 0x000C7CD4 File Offset: 0x000C5ED4
		public override void Dispose()
		{
		}

		// Token: 0x04000C53 RID: 3155
		public bool AbilityEffectsStackWithSameTalent;

		// Token: 0x04000C54 RID: 3156
		public readonly Sprite Icon;

		// Token: 0x04000C55 RID: 3157
		public readonly bool IsHiddenExtraTalent;

		// Token: 0x04000C56 RID: 3158
		[TupleElementNames(new string[]
		{
			"PermanentStatIdentifier",
			"Max"
		})]
		public readonly Option<ValueTuple<Identifier, int>> TrackedStat;

		// Token: 0x04000C57 RID: 3159
		public static readonly PrefabCollection<TalentPrefab> TalentPrefabs = new PrefabCollection<TalentPrefab>();

		// Token: 0x04000C58 RID: 3160
		public readonly ImmutableHashSet<TalentMigration> Migrations;
	}
}
