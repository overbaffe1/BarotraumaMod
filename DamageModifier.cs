using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200002D RID: 45
	internal class DamageModifier : ISerializableEntity
	{
		// Token: 0x17000217 RID: 535
		// (get) Token: 0x060007A8 RID: 1960 RVA: 0x00047A69 File Offset: 0x00045C69
		// (set) Token: 0x060007A9 RID: 1961 RVA: 0x00047A71 File Offset: 0x00045C71
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		[Editable]
		public string DamageSound { get; private set; }

		// Token: 0x17000218 RID: 536
		// (get) Token: 0x060007AA RID: 1962 RVA: 0x00047A7A File Offset: 0x00045C7A
		// (set) Token: 0x060007AB RID: 1963 RVA: 0x00047A82 File Offset: 0x00045C82
		[Serialize("", IsPropertySaveable.No, "", "", false)]
		[Editable]
		public string DamageParticle { get; private set; }

		// Token: 0x17000219 RID: 537
		// (get) Token: 0x060007AC RID: 1964 RVA: 0x00047A8B File Offset: 0x00045C8B
		public string Name
		{
			get
			{
				return "Damage Modifier";
			}
		}

		// Token: 0x1700021A RID: 538
		// (get) Token: 0x060007AD RID: 1965 RVA: 0x00047A92 File Offset: 0x00045C92
		// (set) Token: 0x060007AE RID: 1966 RVA: 0x00047A9A File Offset: 0x00045C9A
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x1700021B RID: 539
		// (get) Token: 0x060007AF RID: 1967 RVA: 0x00047AA3 File Offset: 0x00045CA3
		// (set) Token: 0x060007B0 RID: 1968 RVA: 0x00047AAB File Offset: 0x00045CAB
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		[Editable(DecimalCount = 2)]
		public float DamageMultiplier { get; private set; }

		// Token: 0x1700021C RID: 540
		// (get) Token: 0x060007B1 RID: 1969 RVA: 0x00047AB4 File Offset: 0x00045CB4
		// (set) Token: 0x060007B2 RID: 1970 RVA: 0x00047ABC File Offset: 0x00045CBC
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		[Editable(DecimalCount = 2, MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float ProbabilityMultiplier { get; private set; }

		// Token: 0x1700021D RID: 541
		// (get) Token: 0x060007B3 RID: 1971 RVA: 0x00047AC5 File Offset: 0x00045CC5
		// (set) Token: 0x060007B4 RID: 1972 RVA: 0x00047ACD File Offset: 0x00045CCD
		[Serialize("0.0,360", IsPropertySaveable.No, "", "", false)]
		[Editable]
		public Vector2 ArmorSector { get; private set; }

		// Token: 0x1700021E RID: 542
		// (get) Token: 0x060007B5 RID: 1973 RVA: 0x00047AD6 File Offset: 0x00045CD6
		public Vector2 ArmorSectorInRadians
		{
			get
			{
				return new Vector2(MathHelper.ToRadians(this.ArmorSector.X), MathHelper.ToRadians(this.ArmorSector.Y));
			}
		}

		// Token: 0x1700021F RID: 543
		// (get) Token: 0x060007B6 RID: 1974 RVA: 0x00047AFD File Offset: 0x00045CFD
		// (set) Token: 0x060007B7 RID: 1975 RVA: 0x00047B05 File Offset: 0x00045D05
		[Serialize(false, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public bool DeflectProjectiles { get; private set; }

		// Token: 0x17000220 RID: 544
		// (get) Token: 0x060007B8 RID: 1976 RVA: 0x00047B0E File Offset: 0x00045D0E
		// (set) Token: 0x060007B9 RID: 1977 RVA: 0x00047B16 File Offset: 0x00045D16
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

		// Token: 0x17000221 RID: 545
		// (get) Token: 0x060007BA RID: 1978 RVA: 0x00047B3A File Offset: 0x00045D3A
		// (set) Token: 0x060007BB RID: 1979 RVA: 0x00047B42 File Offset: 0x00045D42
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

		// Token: 0x17000222 RID: 546
		// (get) Token: 0x060007BC RID: 1980 RVA: 0x00047B66 File Offset: 0x00045D66
		public ref readonly ImmutableArray<Identifier> ParsedAfflictionIdentifiers
		{
			get
			{
				return ref this.parsedAfflictionIdentifiers;
			}
		}

		// Token: 0x17000223 RID: 547
		// (get) Token: 0x060007BD RID: 1981 RVA: 0x00047B6E File Offset: 0x00045D6E
		public ref readonly ImmutableArray<Identifier> ParsedAfflictionTypes
		{
			get
			{
				return ref this.parsedAfflictionTypes;
			}
		}

		// Token: 0x060007BE RID: 1982 RVA: 0x00047B78 File Offset: 0x00045D78
		public DamageModifier(ContentXElement element, string parentDebugName, bool checkErrors = true)
		{
			DamageModifier.<>c__DisplayClass46_0 CS$<>8__locals1;
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
						DamageModifier.<.ctor>g__createWarningOrError|46_0(defaultInterpolatedStringHandler.ToStringAndClear(), ref CS$<>8__locals1);
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
						DamageModifier.<.ctor>g__createWarningOrError|46_0(defaultInterpolatedStringHandler2.ToStringAndClear(), ref CS$<>8__locals1);
					}
				}
				if (!this.parsedAfflictionTypes.Any<Identifier>() && !this.parsedAfflictionIdentifiers.Any<Identifier>())
				{
					DamageModifier.<.ctor>g__createWarningOrError|46_0("Potentially invalid damage modifier in \"" + parentDebugName + "\". Neither affliction types of identifiers defined.", ref CS$<>8__locals1);
				}
			}
		}

		// Token: 0x060007BF RID: 1983 RVA: 0x00047D28 File Offset: 0x00045F28
		public bool MatchesAfflictionIdentifier(string identifier)
		{
			return this.MatchesAfflictionIdentifier(identifier.ToIdentifier());
		}

		// Token: 0x060007C0 RID: 1984 RVA: 0x00047D38 File Offset: 0x00045F38
		public bool MatchesAfflictionIdentifier(Identifier identifier)
		{
			return this.AfflictionIdentifiers.Length == 0 || this.parsedAfflictionIdentifiers.Any((Identifier id) => id == identifier);
		}

		// Token: 0x060007C1 RID: 1985 RVA: 0x00047D78 File Offset: 0x00045F78
		public bool MatchesAfflictionType(string type)
		{
			return this.MatchesAfflictionType(type.ToIdentifier());
		}

		// Token: 0x060007C2 RID: 1986 RVA: 0x00047D88 File Offset: 0x00045F88
		public bool MatchesAfflictionType(Identifier type)
		{
			return this.AfflictionTypes.Length == 0 || this.parsedAfflictionTypes.Any((Identifier t) => t == type);
		}

		// Token: 0x060007C3 RID: 1987 RVA: 0x00047DC8 File Offset: 0x00045FC8
		public bool MatchesAffliction(string identifier, string type)
		{
			return this.MatchesAffliction(identifier.ToIdentifier(), type.ToIdentifier());
		}

		// Token: 0x060007C4 RID: 1988 RVA: 0x00047DDC File Offset: 0x00045FDC
		public bool MatchesAffliction(Identifier identifier, Identifier type)
		{
			return (this.AfflictionIdentifiers.Length == 0 && this.AfflictionTypes.Length == 0) || this.parsedAfflictionIdentifiers.Any((Identifier id) => id == identifier) || this.parsedAfflictionTypes.Any((Identifier t) => t == type);
		}

		// Token: 0x060007C5 RID: 1989 RVA: 0x00047E4B File Offset: 0x0004604B
		public bool MatchesAffliction(Affliction affliction)
		{
			return this.MatchesAffliction(affliction.Identifier, affliction.Prefab.AfflictionType);
		}

		// Token: 0x060007C6 RID: 1990 RVA: 0x00047E64 File Offset: 0x00046064
		public void Serialize(XElement element)
		{
			if (element == null)
			{
				return;
			}
			SerializableProperty.SerializeProperties(this, element, false, false);
		}

		// Token: 0x060007C7 RID: 1991 RVA: 0x00047E73 File Offset: 0x00046073
		public void Deserialize(XElement element)
		{
			if (element == null)
			{
				return;
			}
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060007C8 RID: 1992 RVA: 0x00047E86 File Offset: 0x00046086
		[CompilerGenerated]
		internal static void <.ctor>g__createWarningOrError|46_0(string msg, ref DamageModifier.<>c__DisplayClass46_0 A_1)
		{
			DebugConsole.AddWarning(msg, A_1.element.ContentPackage);
		}

		// Token: 0x0400040A RID: 1034
		private string rawAfflictionIdentifierString;

		// Token: 0x0400040B RID: 1035
		private string rawAfflictionTypeString;

		// Token: 0x0400040C RID: 1036
		private ImmutableArray<Identifier> parsedAfflictionIdentifiers;

		// Token: 0x0400040D RID: 1037
		private ImmutableArray<Identifier> parsedAfflictionTypes;
	}
}
