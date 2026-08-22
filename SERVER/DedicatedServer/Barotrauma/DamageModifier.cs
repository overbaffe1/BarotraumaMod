using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000CD RID: 205
	internal class DamageModifier : ISerializableEntity
	{
		// Token: 0x1700069B RID: 1691
		// (get) Token: 0x060016C3 RID: 5827 RVA: 0x000C0AAD File Offset: 0x000BECAD
		public string Name
		{
			get
			{
				return "Damage Modifier";
			}
		}

		// Token: 0x1700069C RID: 1692
		// (get) Token: 0x060016C4 RID: 5828 RVA: 0x000C0AB4 File Offset: 0x000BECB4
		// (set) Token: 0x060016C5 RID: 5829 RVA: 0x000C0ABC File Offset: 0x000BECBC
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x1700069D RID: 1693
		// (get) Token: 0x060016C6 RID: 5830 RVA: 0x000C0AC5 File Offset: 0x000BECC5
		// (set) Token: 0x060016C7 RID: 5831 RVA: 0x000C0ACD File Offset: 0x000BECCD
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		[Editable(DecimalCount = 2)]
		public float DamageMultiplier { get; private set; }

		// Token: 0x1700069E RID: 1694
		// (get) Token: 0x060016C8 RID: 5832 RVA: 0x000C0AD6 File Offset: 0x000BECD6
		// (set) Token: 0x060016C9 RID: 5833 RVA: 0x000C0ADE File Offset: 0x000BECDE
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float ProbabilityMultiplier { get; private set; }

		// Token: 0x1700069F RID: 1695
		// (get) Token: 0x060016CA RID: 5834 RVA: 0x000C0AE7 File Offset: 0x000BECE7
		// (set) Token: 0x060016CB RID: 5835 RVA: 0x000C0AEF File Offset: 0x000BECEF
		[Serialize("0.0,360", IsPropertySaveable.No, "", "", false)]
		[Editable]
		public Vector2 ArmorSector { get; private set; }

		// Token: 0x170006A0 RID: 1696
		// (get) Token: 0x060016CC RID: 5836 RVA: 0x000C0AF8 File Offset: 0x000BECF8
		public Vector2 ArmorSectorInRadians
		{
			get
			{
				return new Vector2(MathHelper.ToRadians(this.ArmorSector.X), MathHelper.ToRadians(this.ArmorSector.Y));
			}
		}

		// Token: 0x170006A1 RID: 1697
		// (get) Token: 0x060016CD RID: 5837 RVA: 0x000C0B1F File Offset: 0x000BED1F
		// (set) Token: 0x060016CE RID: 5838 RVA: 0x000C0B27 File Offset: 0x000BED27
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public bool DeflectProjectiles { get; private set; }

		// Token: 0x170006A2 RID: 1698
		// (get) Token: 0x060016CF RID: 5839 RVA: 0x000C0B30 File Offset: 0x000BED30
		// (set) Token: 0x060016D0 RID: 5840 RVA: 0x000C0B38 File Offset: 0x000BED38
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string AfflictionIdentifiers
		{
			get
			{
				return this.rawAfflictionIdentifierString;
			}
			private set
			{
				this.rawAfflictionIdentifierString = value;
				this.parsedAfflictionIdentifiers = this.rawAfflictionIdentifierString.ToIdentifiers(",").ToImmutableArray<Identifier>();
			}
		}

		// Token: 0x170006A3 RID: 1699
		// (get) Token: 0x060016D1 RID: 5841 RVA: 0x000C0B5C File Offset: 0x000BED5C
		// (set) Token: 0x060016D2 RID: 5842 RVA: 0x000C0B64 File Offset: 0x000BED64
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string AfflictionTypes
		{
			get
			{
				return this.rawAfflictionTypeString;
			}
			private set
			{
				this.rawAfflictionTypeString = value;
				this.parsedAfflictionTypes = this.rawAfflictionTypeString.ToIdentifiers(",").ToImmutableArray<Identifier>();
			}
		}

		// Token: 0x170006A4 RID: 1700
		// (get) Token: 0x060016D3 RID: 5843 RVA: 0x000C0B88 File Offset: 0x000BED88
		public ref readonly ImmutableArray<Identifier> ParsedAfflictionIdentifiers
		{
			get
			{
				return ref this.parsedAfflictionIdentifiers;
			}
		}

		// Token: 0x170006A5 RID: 1701
		// (get) Token: 0x060016D4 RID: 5844 RVA: 0x000C0B90 File Offset: 0x000BED90
		public ref readonly ImmutableArray<Identifier> ParsedAfflictionTypes
		{
			get
			{
				return ref this.parsedAfflictionTypes;
			}
		}

		// Token: 0x060016D5 RID: 5845 RVA: 0x000C0B98 File Offset: 0x000BED98
		public DamageModifier(ContentXElement element, string parentDebugName, bool checkErrors = true)
		{
			DamageModifier.<>c__DisplayClass38_0 CS$<>8__locals1;
			CS$<>8__locals1.element = element;
			base..ctor();
			this.Deserialize(CS$<>8__locals1.element);
			if (CS$<>8__locals1.element.GetAttribute("afflictionnames") != null)
			{
				DebugConsole.ThrowError("Error in DamageModifier config (" + parentDebugName + ") - define afflictions using identifiers or types instead of names.", null, CS$<>8__locals1.element.ContentPackage, false, false);
			}
			if (checkErrors)
			{
				ImmutableArray<Identifier>.Enumerator enumerator = this.parsedAfflictionTypes.GetEnumerator();
				while (enumerator.MoveNext())
				{
					Identifier afflictionType = enumerator.Current;
					if (!AfflictionPrefab.Prefabs.Any((AfflictionPrefab p) => p.AfflictionType == afflictionType))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(143, 2);
						defaultInterpolatedStringHandler.AppendLiteral("Potentially invalid damage modifier in \"");
						defaultInterpolatedStringHandler.AppendFormatted(parentDebugName);
						defaultInterpolatedStringHandler.AppendLiteral("\". Could not find any afflictions of the type \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(afflictionType);
						defaultInterpolatedStringHandler.AppendLiteral("\". Did you mean to use an affliction identifier instead?");
						DamageModifier.<.ctor>g__createWarningOrError|38_0(defaultInterpolatedStringHandler.ToStringAndClear(), ref CS$<>8__locals1);
					}
				}
				foreach (Identifier afflictionIdentifier in this.parsedAfflictionIdentifiers)
				{
					if (!AfflictionPrefab.Prefabs.ContainsKey(afflictionIdentifier))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(145, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("Potentially invalid damage modifier in \"");
						defaultInterpolatedStringHandler2.AppendFormatted(parentDebugName);
						defaultInterpolatedStringHandler2.AppendLiteral("\". Could not find any afflictions with the identifier \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(afflictionIdentifier);
						defaultInterpolatedStringHandler2.AppendLiteral("\". Did you mean to use an affliction type instead?");
						DamageModifier.<.ctor>g__createWarningOrError|38_0(defaultInterpolatedStringHandler2.ToStringAndClear(), ref CS$<>8__locals1);
					}
				}
				if (!this.parsedAfflictionTypes.Any<Identifier>() && !this.parsedAfflictionIdentifiers.Any<Identifier>())
				{
					DamageModifier.<.ctor>g__createWarningOrError|38_0("Potentially invalid damage modifier in \"" + parentDebugName + "\". Neither affliction types of identifiers defined.", ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x060016D6 RID: 5846 RVA: 0x000C0D48 File Offset: 0x000BEF48
		public bool MatchesAfflictionIdentifier(string identifier)
		{
			return this.MatchesAfflictionIdentifier(identifier.ToIdentifier());
		}

		// Token: 0x060016D7 RID: 5847 RVA: 0x000C0D58 File Offset: 0x000BEF58
		public bool MatchesAfflictionIdentifier(Identifier identifier)
		{
			return this.AfflictionIdentifiers.Length == 0 || this.parsedAfflictionIdentifiers.Any((Identifier id) => id == identifier);
		}

		// Token: 0x060016D8 RID: 5848 RVA: 0x000C0D98 File Offset: 0x000BEF98
		public bool MatchesAfflictionType(string type)
		{
			return this.MatchesAfflictionType(type.ToIdentifier());
		}

		// Token: 0x060016D9 RID: 5849 RVA: 0x000C0DA8 File Offset: 0x000BEFA8
		public bool MatchesAfflictionType(Identifier type)
		{
			return this.AfflictionTypes.Length == 0 || this.parsedAfflictionTypes.Any((Identifier t) => t == type);
		}

		// Token: 0x060016DA RID: 5850 RVA: 0x000C0DE8 File Offset: 0x000BEFE8
		public bool MatchesAffliction(string identifier, string type)
		{
			return this.MatchesAffliction(identifier.ToIdentifier(), type.ToIdentifier());
		}

		// Token: 0x060016DB RID: 5851 RVA: 0x000C0DFC File Offset: 0x000BEFFC
		public bool MatchesAffliction(Identifier identifier, Identifier type)
		{
			return (this.AfflictionIdentifiers.Length == 0 && this.AfflictionTypes.Length == 0) || this.parsedAfflictionIdentifiers.Any((Identifier id) => id == identifier) || this.parsedAfflictionTypes.Any((Identifier t) => t == type);
		}

		// Token: 0x060016DC RID: 5852 RVA: 0x000C0E6B File Offset: 0x000BF06B
		public bool MatchesAffliction(Affliction affliction)
		{
			return this.MatchesAffliction(affliction.Identifier, affliction.Prefab.AfflictionType);
		}

		// Token: 0x060016DD RID: 5853 RVA: 0x000C0E84 File Offset: 0x000BF084
		public void Serialize(XElement element)
		{
			if (element == null)
			{
				return;
			}
			SerializableProperty.SerializeProperties(this, element, false, false);
		}

		// Token: 0x060016DE RID: 5854 RVA: 0x000C0E93 File Offset: 0x000BF093
		public void Deserialize(XElement element)
		{
			if (element == null)
			{
				return;
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060016DF RID: 5855 RVA: 0x000C0EA6 File Offset: 0x000BF0A6
		[CompilerGenerated]
		internal static void <.ctor>g__createWarningOrError|38_0(string msg, ref DamageModifier.<>c__DisplayClass38_0 A_1)
		{
			DebugConsole.AddWarning(msg, A_1.element.ContentPackage);
		}

		// Token: 0x04000B07 RID: 2823
		private string rawAfflictionIdentifierString;

		// Token: 0x04000B08 RID: 2824
		private string rawAfflictionTypeString;

		// Token: 0x04000B09 RID: 2825
		private ImmutableArray<Identifier> parsedAfflictionIdentifiers;

		// Token: 0x04000B0A RID: 2826
		private ImmutableArray<Identifier> parsedAfflictionTypes;
	}
}
