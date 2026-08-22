using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000235 RID: 565
	internal class Biome : PrefabWithUintIdentifier
	{
		// Token: 0x17000B1D RID: 2845
		// (get) Token: 0x060026A7 RID: 9895 RVA: 0x000FD1D6 File Offset: 0x000FB3D6
		public float ActualMaxDifficulty
		{
			get
			{
				return this.maxDifficulty;
			}
		}

		// Token: 0x17000B1E RID: 2846
		// (get) Token: 0x060026A8 RID: 9896 RVA: 0x000FD1DE File Offset: 0x000FB3DE
		public float AdjustedMaxDifficulty
		{
			get
			{
				return this.maxDifficulty - 0.1f;
			}
		}

		// Token: 0x060026A9 RID: 9897 RVA: 0x000FD1EC File Offset: 0x000FB3EC
		public Biome(ContentXElement element, LevelGenerationParametersFile file) : base(file, Biome.ParseIdentifier(element))
		{
			this.OldIdentifier = element.GetAttributeIdentifier("oldidentifier", Identifier.Empty);
			this.DisplayName = TextManager.Get("biomename." + this.Identifier.ToString()).Fallback(element.GetAttributeString("name", "Biome"), true);
			this.Description = TextManager.Get("biomedescription." + this.Identifier.ToString()).Fallback(element.GetAttributeString("description", ""), true);
			this.IsEndBiome = element.GetAttributeBool("endbiome", false);
			this.EndBiomeLocationCount = Math.Max(1, element.GetAttributeInt("endbiomelocationcount", 1));
			this.AllowedZones = element.GetAttributeIntArray("AllowedZones", new int[]
			{
				1,
				2,
				3,
				4,
				5,
				6,
				7,
				8,
				9
			}).ToImmutableHashSet<int>();
			this.MinDifficulty = element.GetAttributeFloat("MinDifficulty", 0f);
			this.maxDifficulty = element.GetAttributeFloat("MaxDifficulty", 100f);
			float baseExperience = 0.09f;
			float difficultyRewardMultiplier = 0.25f;
			float calculateDefaultExperience = baseExperience + this.MinDifficulty * difficultyRewardMultiplier / 100f;
			this.ExperienceFromMissionRewards = element.GetAttributeFloat("ExperienceFromMissionRewards", calculateDefaultExperience);
			HashSet<Biome.SubmarineAvailability> submarineAvailabilityOverrides = new HashSet<Biome.SubmarineAvailability>();
			ContentXElement availabilityElement = element.GetChildElement("submarines");
			if (availabilityElement != null)
			{
				this.submarineAvailability = new Biome.SubmarineAvailability?(Biome.<.ctor>g__GetAvailability|17_0(availabilityElement));
				foreach (ContentXElement overrideElement in availabilityElement.GetChildElements("override"))
				{
					Biome.SubmarineAvailability availabilityOverride = Biome.<.ctor>g__GetAvailability|17_0(overrideElement);
					submarineAvailabilityOverrides.Add(availabilityOverride);
				}
			}
			this.submarineAvailabilityOverrides = submarineAvailabilityOverrides.ToImmutableHashSet<Biome.SubmarineAvailability>();
		}

		// Token: 0x060026AA RID: 9898 RVA: 0x000FD3D8 File Offset: 0x000FB5D8
		public static Identifier ParseIdentifier(ContentXElement element)
		{
			Identifier identifier = element.GetAttributeIdentifier("identifier", "");
			if (identifier.IsEmpty)
			{
				identifier = element.GetAttributeIdentifier("name", "");
				DebugConsole.ThrowError("Error in biome \"" + identifier.ToString() + "\": identifier missing, using name as the identifier.", null, null, false, false);
			}
			return identifier;
		}

		// Token: 0x060026AB RID: 9899 RVA: 0x000FD438 File Offset: 0x000FB638
		public int HighestSubmarineTierAvailable(SubmarineClass subClass, Identifier locationType)
		{
			if (this.submarineAvailability == null)
			{
				return 3;
			}
			int maxTier = this.submarineAvailability.Value.MaxTier;
			Biome.SubmarineAvailability? submarineAvailability = this.submarineAvailabilityOverrides.FirstOrNull(delegate(Biome.SubmarineAvailability a)
			{
				Identifier locationType2 = a.LocationType;
				return locationType2 == locationType && a.Class == subClass;
			});
			if (submarineAvailability != null)
			{
				maxTier = submarineAvailability.GetValueOrDefault().MaxTier;
			}
			else
			{
				submarineAvailability = this.submarineAvailabilityOverrides.FirstOrNull(delegate(Biome.SubmarineAvailability a)
				{
					Identifier locationType2 = a.LocationType;
					return locationType2 == locationType && a.Class == SubmarineClass.Undefined;
				});
				if (submarineAvailability != null)
				{
					maxTier = submarineAvailability.GetValueOrDefault().MaxTier;
				}
				else
				{
					submarineAvailability = this.submarineAvailabilityOverrides.FirstOrNull(delegate(Biome.SubmarineAvailability a)
					{
						Identifier locationType2 = a.LocationType;
						return locationType2 == Identifier.Empty && a.Class == subClass;
					});
					if (submarineAvailability != null)
					{
						maxTier = submarineAvailability.GetValueOrDefault().MaxTier;
					}
				}
			}
			return maxTier;
		}

		// Token: 0x060026AC RID: 9900 RVA: 0x000FD519 File Offset: 0x000FB719
		public bool IsSubmarineAvailable(SubmarineInfo info, Identifier locationType)
		{
			return info.Tier <= this.HighestSubmarineTierAvailable(info.SubmarineClass, locationType);
		}

		// Token: 0x060026AD RID: 9901 RVA: 0x000FD533 File Offset: 0x000FB733
		public override void Dispose()
		{
		}

		// Token: 0x060026AF RID: 9903 RVA: 0x000FD544 File Offset: 0x000FB744
		[CompilerGenerated]
		internal static Biome.SubmarineAvailability <.ctor>g__GetAvailability|17_0(ContentXElement element)
		{
			Identifier attributeIdentifier = element.GetAttributeIdentifier("locationtype", Identifier.Empty);
			string key = "class";
			SubmarineClass submarineClass = SubmarineClass.Undefined;
			return new Biome.SubmarineAvailability(attributeIdentifier, element.GetAttributeEnum<SubmarineClass>(key, submarineClass), element.GetAttributeInt("maxtier", 0));
		}

		// Token: 0x040012E5 RID: 4837
		public static readonly PrefabCollection<Biome> Prefabs = new PrefabCollection<Biome>();

		// Token: 0x040012E6 RID: 4838
		public readonly Identifier OldIdentifier;

		// Token: 0x040012E7 RID: 4839
		public readonly LocalizedString DisplayName;

		// Token: 0x040012E8 RID: 4840
		public readonly LocalizedString Description;

		// Token: 0x040012E9 RID: 4841
		public readonly bool IsEndBiome;

		// Token: 0x040012EA RID: 4842
		public readonly int EndBiomeLocationCount;

		// Token: 0x040012EB RID: 4843
		public readonly float MinDifficulty;

		// Token: 0x040012EC RID: 4844
		private readonly float maxDifficulty;

		// Token: 0x040012ED RID: 4845
		public readonly float ExperienceFromMissionRewards;

		// Token: 0x040012EE RID: 4846
		public readonly ImmutableHashSet<int> AllowedZones;

		// Token: 0x040012EF RID: 4847
		private readonly Biome.SubmarineAvailability? submarineAvailability;

		// Token: 0x040012F0 RID: 4848
		private readonly ImmutableHashSet<Biome.SubmarineAvailability> submarineAvailabilityOverrides;

		// Token: 0x02000A03 RID: 2563
		public readonly struct SubmarineAvailability : IEquatable<Biome.SubmarineAvailability>
		{
			// Token: 0x06005B95 RID: 23445 RVA: 0x001FEF07 File Offset: 0x001FD107
			public SubmarineAvailability(Identifier LocationType, SubmarineClass Class = SubmarineClass.Undefined, int MaxTier = 0)
			{
				this.LocationType = LocationType;
				this.Class = Class;
				this.MaxTier = MaxTier;
			}

			// Token: 0x1700157B RID: 5499
			// (get) Token: 0x06005B96 RID: 23446 RVA: 0x001FEF1E File Offset: 0x001FD11E
			// (set) Token: 0x06005B97 RID: 23447 RVA: 0x001FEF26 File Offset: 0x001FD126
			public Identifier LocationType { get; set; }

			// Token: 0x1700157C RID: 5500
			// (get) Token: 0x06005B98 RID: 23448 RVA: 0x001FEF2F File Offset: 0x001FD12F
			// (set) Token: 0x06005B99 RID: 23449 RVA: 0x001FEF37 File Offset: 0x001FD137
			public SubmarineClass Class { get; set; }

			// Token: 0x1700157D RID: 5501
			// (get) Token: 0x06005B9A RID: 23450 RVA: 0x001FEF40 File Offset: 0x001FD140
			// (set) Token: 0x06005B9B RID: 23451 RVA: 0x001FEF48 File Offset: 0x001FD148
			public int MaxTier { get; set; }

			// Token: 0x06005B9C RID: 23452 RVA: 0x001FEF54 File Offset: 0x001FD154
			[CompilerGenerated]
			public override string ToString()
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("SubmarineAvailability");
				stringBuilder.Append(" { ");
				if (this.PrintMembers(stringBuilder))
				{
					stringBuilder.Append(' ');
				}
				stringBuilder.Append('}');
				return stringBuilder.ToString();
			}

			// Token: 0x06005B9D RID: 23453 RVA: 0x001FEFA0 File Offset: 0x001FD1A0
			[CompilerGenerated]
			private bool PrintMembers(StringBuilder builder)
			{
				builder.Append("LocationType = ");
				builder.Append(this.LocationType.ToString());
				builder.Append(", Class = ");
				builder.Append(this.Class.ToString());
				builder.Append(", MaxTier = ");
				builder.Append(this.MaxTier.ToString());
				return true;
			}

			// Token: 0x06005B9E RID: 23454 RVA: 0x001FF023 File Offset: 0x001FD223
			[CompilerGenerated]
			public static bool operator !=(Biome.SubmarineAvailability left, Biome.SubmarineAvailability right)
			{
				return !(left == right);
			}

			// Token: 0x06005B9F RID: 23455 RVA: 0x001FF02F File Offset: 0x001FD22F
			[CompilerGenerated]
			public static bool operator ==(Biome.SubmarineAvailability left, Biome.SubmarineAvailability right)
			{
				return left.Equals(right);
			}

			// Token: 0x06005BA0 RID: 23456 RVA: 0x001FF039 File Offset: 0x001FD239
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Identifier>.Default.GetHashCode(this.<LocationType>k__BackingField) * -1521134295 + EqualityComparer<SubmarineClass>.Default.GetHashCode(this.<Class>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<MaxTier>k__BackingField);
			}

			// Token: 0x06005BA1 RID: 23457 RVA: 0x001FF079 File Offset: 0x001FD279
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Biome.SubmarineAvailability && this.Equals((Biome.SubmarineAvailability)obj);
			}

			// Token: 0x06005BA2 RID: 23458 RVA: 0x001FF094 File Offset: 0x001FD294
			[CompilerGenerated]
			public bool Equals(Biome.SubmarineAvailability other)
			{
				return EqualityComparer<Identifier>.Default.Equals(this.<LocationType>k__BackingField, other.<LocationType>k__BackingField) && EqualityComparer<SubmarineClass>.Default.Equals(this.<Class>k__BackingField, other.<Class>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<MaxTier>k__BackingField, other.<MaxTier>k__BackingField);
			}

			// Token: 0x06005BA3 RID: 23459 RVA: 0x001FF0E9 File Offset: 0x001FD2E9
			[CompilerGenerated]
			public void Deconstruct(out Identifier LocationType, out SubmarineClass Class, out int MaxTier)
			{
				LocationType = this.LocationType;
				Class = this.Class;
				MaxTier = this.MaxTier;
			}
		}
	}
}
