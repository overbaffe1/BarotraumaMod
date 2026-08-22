using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Extensions;

namespace Barotrauma
{
	// Token: 0x02000319 RID: 793
	internal class Biome : PrefabWithUintIdentifier
	{
		// Token: 0x17001090 RID: 4240
		// (get) Token: 0x06003F16 RID: 16150 RVA: 0x002356DB File Offset: 0x002338DB
		public float ActualMaxDifficulty
		{
			get
			{
				return this.maxDifficulty;
			}
		}

		// Token: 0x17001091 RID: 4241
		// (get) Token: 0x06003F17 RID: 16151 RVA: 0x002356E3 File Offset: 0x002338E3
		public float AdjustedMaxDifficulty
		{
			get
			{
				return this.maxDifficulty - 0.1f;
			}
		}

		// Token: 0x06003F18 RID: 16152 RVA: 0x002356F4 File Offset: 0x002338F4
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

		// Token: 0x06003F19 RID: 16153 RVA: 0x002358E0 File Offset: 0x00233AE0
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

		// Token: 0x06003F1A RID: 16154 RVA: 0x00235940 File Offset: 0x00233B40
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

		// Token: 0x06003F1B RID: 16155 RVA: 0x00235A21 File Offset: 0x00233C21
		public bool IsSubmarineAvailable(SubmarineInfo info, Identifier locationType)
		{
			return info.Tier <= this.HighestSubmarineTierAvailable(info.SubmarineClass, locationType);
		}

		// Token: 0x06003F1C RID: 16156 RVA: 0x00235A3B File Offset: 0x00233C3B
		public override void Dispose()
		{
		}

		// Token: 0x06003F1E RID: 16158 RVA: 0x00235A4C File Offset: 0x00233C4C
		[CompilerGenerated]
		internal static Biome.SubmarineAvailability <.ctor>g__GetAvailability|17_0(ContentXElement element)
		{
			Identifier attributeIdentifier = element.GetAttributeIdentifier("locationtype", Identifier.Empty);
			string key = "class";
			SubmarineClass submarineClass = SubmarineClass.Undefined;
			return new Biome.SubmarineAvailability(attributeIdentifier, element.GetAttributeEnum<SubmarineClass>(key, submarineClass), element.GetAttributeInt("maxtier", 0));
		}

		// Token: 0x040020BF RID: 8383
		public static readonly PrefabCollection<Biome> Prefabs = new PrefabCollection<Biome>();

		// Token: 0x040020C0 RID: 8384
		public readonly Identifier OldIdentifier;

		// Token: 0x040020C1 RID: 8385
		public readonly LocalizedString DisplayName;

		// Token: 0x040020C2 RID: 8386
		public readonly LocalizedString Description;

		// Token: 0x040020C3 RID: 8387
		public readonly bool IsEndBiome;

		// Token: 0x040020C4 RID: 8388
		public readonly int EndBiomeLocationCount;

		// Token: 0x040020C5 RID: 8389
		public readonly float MinDifficulty;

		// Token: 0x040020C6 RID: 8390
		private readonly float maxDifficulty;

		// Token: 0x040020C7 RID: 8391
		public readonly float ExperienceFromMissionRewards;

		// Token: 0x040020C8 RID: 8392
		public readonly ImmutableHashSet<int> AllowedZones;

		// Token: 0x040020C9 RID: 8393
		private readonly Biome.SubmarineAvailability? submarineAvailability;

		// Token: 0x040020CA RID: 8394
		private readonly ImmutableHashSet<Biome.SubmarineAvailability> submarineAvailabilityOverrides;

		// Token: 0x02000FDF RID: 4063
		public readonly struct SubmarineAvailability : IEquatable<Biome.SubmarineAvailability>
		{
			// Token: 0x06008A6E RID: 35438 RVA: 0x003AA6AF File Offset: 0x003A88AF
			public SubmarineAvailability(Identifier LocationType, SubmarineClass Class = SubmarineClass.Undefined, int MaxTier = 0)
			{
				this.LocationType = LocationType;
				this.Class = Class;
				this.MaxTier = MaxTier;
			}

			// Token: 0x17001C3C RID: 7228
			// (get) Token: 0x06008A6F RID: 35439 RVA: 0x003AA6C6 File Offset: 0x003A88C6
			// (set) Token: 0x06008A70 RID: 35440 RVA: 0x003AA6CE File Offset: 0x003A88CE
			public Identifier LocationType { get; set; }

			// Token: 0x17001C3D RID: 7229
			// (get) Token: 0x06008A71 RID: 35441 RVA: 0x003AA6D7 File Offset: 0x003A88D7
			// (set) Token: 0x06008A72 RID: 35442 RVA: 0x003AA6DF File Offset: 0x003A88DF
			public SubmarineClass Class { get; set; }

			// Token: 0x17001C3E RID: 7230
			// (get) Token: 0x06008A73 RID: 35443 RVA: 0x003AA6E8 File Offset: 0x003A88E8
			// (set) Token: 0x06008A74 RID: 35444 RVA: 0x003AA6F0 File Offset: 0x003A88F0
			public int MaxTier { get; set; }

			// Token: 0x06008A75 RID: 35445 RVA: 0x003AA6FC File Offset: 0x003A88FC
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

			// Token: 0x06008A76 RID: 35446 RVA: 0x003AA748 File Offset: 0x003A8948
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

			// Token: 0x06008A77 RID: 35447 RVA: 0x003AA7CB File Offset: 0x003A89CB
			[CompilerGenerated]
			public static bool operator !=(Biome.SubmarineAvailability left, Biome.SubmarineAvailability right)
			{
				return !(left == right);
			}

			// Token: 0x06008A78 RID: 35448 RVA: 0x003AA7D7 File Offset: 0x003A89D7
			[CompilerGenerated]
			public static bool operator ==(Biome.SubmarineAvailability left, Biome.SubmarineAvailability right)
			{
				return left.Equals(right);
			}

			// Token: 0x06008A79 RID: 35449 RVA: 0x003AA7E1 File Offset: 0x003A89E1
			[CompilerGenerated]
			public override int GetHashCode()
			{
				return (EqualityComparer<Identifier>.Default.GetHashCode(this.<LocationType>k__BackingField) * -1521134295 + EqualityComparer<SubmarineClass>.Default.GetHashCode(this.<Class>k__BackingField)) * -1521134295 + EqualityComparer<int>.Default.GetHashCode(this.<MaxTier>k__BackingField);
			}

			// Token: 0x06008A7A RID: 35450 RVA: 0x003AA821 File Offset: 0x003A8A21
			[CompilerGenerated]
			public override bool Equals(object obj)
			{
				return obj is Biome.SubmarineAvailability && this.Equals((Biome.SubmarineAvailability)obj);
			}

			// Token: 0x06008A7B RID: 35451 RVA: 0x003AA83C File Offset: 0x003A8A3C
			[CompilerGenerated]
			public bool Equals(Biome.SubmarineAvailability other)
			{
				return EqualityComparer<Identifier>.Default.Equals(this.<LocationType>k__BackingField, other.<LocationType>k__BackingField) && EqualityComparer<SubmarineClass>.Default.Equals(this.<Class>k__BackingField, other.<Class>k__BackingField) && EqualityComparer<int>.Default.Equals(this.<MaxTier>k__BackingField, other.<MaxTier>k__BackingField);
			}

			// Token: 0x06008A7C RID: 35452 RVA: 0x003AA891 File Offset: 0x003A8A91
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
