using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001F5 RID: 501
	internal class TalentPrefab : PrefabWithUintIdentifier
	{
		// Token: 0x17000E24 RID: 3620
		// (get) Token: 0x060034CA RID: 13514 RVA: 0x0020E320 File Offset: 0x0020C520
		public string OriginalName
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17000E25 RID: 3621
		// (get) Token: 0x060034CB RID: 13515 RVA: 0x0020E32D File Offset: 0x0020C52D
		// (set) Token: 0x060034CC RID: 13516 RVA: 0x0020E335 File Offset: 0x0020C535
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x17000E26 RID: 3622
		// (get) Token: 0x060034CD RID: 13517 RVA: 0x0020E33E File Offset: 0x0020C53E
		// (set) Token: 0x060034CE RID: 13518 RVA: 0x0020E346 File Offset: 0x0020C546
		public LocalizedString Description { get; private set; }

		// Token: 0x17000E27 RID: 3623
		// (get) Token: 0x060034CF RID: 13519 RVA: 0x0020E34F File Offset: 0x0020C54F
		// (set) Token: 0x060034D0 RID: 13520 RVA: 0x0020E357 File Offset: 0x0020C557
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x060034D1 RID: 13521 RVA: 0x0020E360 File Offset: 0x0020C560
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
			string key = "coloroverride";
			Color transparentBlack = Color.TransparentBlack;
			Color colorOverride = element.GetAttributeColor(key, transparentBlack);
			this.ColorOverride = ((colorOverride != Color.TransparentBlack) ? Option<Color>.Some(colorOverride) : Option<Color>.None());
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

		// Token: 0x060034D2 RID: 13522 RVA: 0x0020E710 File Offset: 0x0020C910
		public override void Dispose()
		{
		}

		// Token: 0x04001B79 RID: 7033
		public bool AbilityEffectsStackWithSameTalent;

		// Token: 0x04001B7A RID: 7034
		public readonly Sprite Icon;

		// Token: 0x04001B7B RID: 7035
		public readonly bool IsHiddenExtraTalent;

		// Token: 0x04001B7C RID: 7036
		[TupleElementNames(new string[]
		{
			"PermanentStatIdentifier",
			"Max"
		})]
		public readonly Option<ValueTuple<Identifier, int>> TrackedStat;

		// Token: 0x04001B7D RID: 7037
		public readonly Option<Color> ColorOverride;

		// Token: 0x04001B7E RID: 7038
		public static readonly PrefabCollection<TalentPrefab> TalentPrefabs = new PrefabCollection<TalentPrefab>();

		// Token: 0x04001B7F RID: 7039
		public readonly ImmutableHashSet<TalentMigration> Migrations;
	}
}
