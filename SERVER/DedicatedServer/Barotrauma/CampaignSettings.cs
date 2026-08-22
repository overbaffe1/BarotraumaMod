using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020001E2 RID: 482
	[NullableContext(1)]
	[Nullable(0)]
	internal class CampaignSettings : INetSerializableStruct, ISerializableEntity
	{
		// Token: 0x170009F2 RID: 2546
		// (get) Token: 0x060022B8 RID: 8888 RVA: 0x000E8F5C File Offset: 0x000E715C
		public static CampaignSettings Empty
		{
			get
			{
				return new CampaignSettings(null);
			}
		}

		// Token: 0x170009F3 RID: 2547
		// (get) Token: 0x060022B9 RID: 8889 RVA: 0x000E8F64 File Offset: 0x000E7164
		public string Name
		{
			get
			{
				return "CampaignSettings";
			}
		}

		// Token: 0x170009F4 RID: 2548
		// (get) Token: 0x060022BA RID: 8890 RVA: 0x000E8F6B File Offset: 0x000E716B
		// (set) Token: 0x060022BB RID: 8891 RVA: 0x000E8F73 File Offset: 0x000E7173
		[Serialize("Normal", IsPropertySaveable.Yes, "", "", false)]
		public string PresetName { get; set; } = string.Empty;

		// Token: 0x170009F5 RID: 2549
		// (get) Token: 0x060022BC RID: 8892 RVA: 0x000E8F7C File Offset: 0x000E717C
		// (set) Token: 0x060022BD RID: 8893 RVA: 0x000E8F84 File Offset: 0x000E7184
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool TutorialEnabled { get; set; }

		// Token: 0x170009F6 RID: 2550
		// (get) Token: 0x060022BE RID: 8894 RVA: 0x000E8F8D File Offset: 0x000E718D
		// (set) Token: 0x060022BF RID: 8895 RVA: 0x000E8F95 File Offset: 0x000E7195
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(27)]
		public bool RadiationEnabled { get; set; }

		// Token: 0x170009F7 RID: 2551
		// (get) Token: 0x060022C0 RID: 8896 RVA: 0x000E8F9E File Offset: 0x000E719E
		// (set) Token: 0x060022C1 RID: 8897 RVA: 0x000E8FA6 File Offset: 0x000E71A6
		[Serialize(2, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(36, MinValueInt = 1, MaxValueInt = 10)]
		public int MaxMissionCount
		{
			get
			{
				return this.maxMissionCount;
			}
			set
			{
				this.maxMissionCount = MathHelper.Clamp(value, 1, 10);
			}
		}

		// Token: 0x170009F8 RID: 2552
		// (get) Token: 0x060022C2 RID: 8898 RVA: 0x000E8FB7 File Offset: 0x000E71B7
		public int TotalMaxMissionCount
		{
			get
			{
				return this.MaxMissionCount + CampaignSettings.GetAddedMissionCount();
			}
		}

		// Token: 0x170009F9 RID: 2553
		// (get) Token: 0x060022C3 RID: 8899 RVA: 0x000E8FC5 File Offset: 0x000E71C5
		// (set) Token: 0x060022C4 RID: 8900 RVA: 0x000E8FCD File Offset: 0x000E71CD
		[Serialize(WorldHostilityOption.Medium, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(45)]
		public WorldHostilityOption WorldHostility { get; set; }

		// Token: 0x170009FA RID: 2554
		// (get) Token: 0x060022C5 RID: 8901 RVA: 0x000E8FD6 File Offset: 0x000E71D6
		// (set) Token: 0x060022C6 RID: 8902 RVA: 0x000E8FDE File Offset: 0x000E71DE
		[Serialize("normal", IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(48)]
		public Identifier StartItemSet { get; set; }

		// Token: 0x170009FB RID: 2555
		// (get) Token: 0x060022C7 RID: 8903 RVA: 0x000E8FE7 File Offset: 0x000E71E7
		// (set) Token: 0x060022C8 RID: 8904 RVA: 0x000E8FEF File Offset: 0x000E71EF
		[Serialize(StartingBalanceAmountOption.Medium, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(51)]
		public StartingBalanceAmountOption StartingBalanceAmount { get; set; }

		// Token: 0x170009FC RID: 2556
		// (get) Token: 0x060022C9 RID: 8905 RVA: 0x000E8FF8 File Offset: 0x000E71F8
		public int InitialMoney
		{
			get
			{
				int? initialMoney = this._initialMoney;
				if (initialMoney != null)
				{
					return initialMoney.GetValueOrDefault();
				}
				this._initialMoney = new int?(8000);
				Identifier settingDefinitionIdentifier = "StartingBalanceAmount".ToIdentifier();
				Identifier attributeIdentifier = this.StartingBalanceAmount.ToIdentifier<StartingBalanceAmountOption>();
				XAttribute attribute;
				if (CampaignModePresets.TryGetAttribute(settingDefinitionIdentifier, attributeIdentifier, out attribute))
				{
					this._initialMoney = new int?(attribute.GetAttributeInt(8000));
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
					defaultInterpolatedStringHandler.AppendLiteral("CampaignSettings: Can't find value for ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(attributeIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(settingDefinitionIdentifier);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return this._initialMoney.GetValueOrDefault(8000);
			}
		}

		// Token: 0x170009FD RID: 2557
		// (get) Token: 0x060022CA RID: 8906 RVA: 0x000E90BC File Offset: 0x000E72BC
		public float ExtraEventManagerDifficulty
		{
			get
			{
				float? extraEventManagerDifficulty = this._extraEventManagerDifficulty;
				if (extraEventManagerDifficulty != null)
				{
					return extraEventManagerDifficulty.GetValueOrDefault();
				}
				this._extraEventManagerDifficulty = new float?(0f);
				Identifier settingDefinitionIdentifier = "ExtraEventManagerDifficulty".ToIdentifier();
				Identifier attributeIdentifier = this.WorldHostility.ToIdentifier<WorldHostilityOption>();
				XAttribute attribute;
				if (CampaignModePresets.TryGetAttribute(settingDefinitionIdentifier, attributeIdentifier, out attribute))
				{
					this._extraEventManagerDifficulty = new float?(attribute.GetAttributeFloat(0f));
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
					defaultInterpolatedStringHandler.AppendLiteral("CampaignSettings: Can't find value for ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(attributeIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(settingDefinitionIdentifier);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return this._extraEventManagerDifficulty.GetValueOrDefault();
			}
		}

		// Token: 0x170009FE RID: 2558
		// (get) Token: 0x060022CB RID: 8907 RVA: 0x000E917C File Offset: 0x000E737C
		public float LevelDifficultyMultiplier
		{
			get
			{
				float? levelDifficultyMultiplier = this._levelDifficultyMultiplier;
				if (levelDifficultyMultiplier != null)
				{
					return levelDifficultyMultiplier.GetValueOrDefault();
				}
				this._levelDifficultyMultiplier = new float?(1f);
				Identifier settingDefinitionIdentifier = "LevelDifficultyMultiplier".ToIdentifier();
				Identifier attributeIdentifier = this.WorldHostility.ToIdentifier<WorldHostilityOption>();
				XAttribute attribute;
				if (CampaignModePresets.TryGetAttribute(settingDefinitionIdentifier, attributeIdentifier, out attribute))
				{
					this._levelDifficultyMultiplier = new float?(attribute.GetAttributeFloat(1f));
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
					defaultInterpolatedStringHandler.AppendLiteral("CampaignSettings: Can't find value for ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(attributeIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(settingDefinitionIdentifier);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return this._levelDifficultyMultiplier.GetValueOrDefault(1f);
			}
		}

		// Token: 0x170009FF RID: 2559
		// (get) Token: 0x060022CC RID: 8908 RVA: 0x000E9240 File Offset: 0x000E7440
		// (set) Token: 0x060022CD RID: 8909 RVA: 0x000E9248 File Offset: 0x000E7448
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(154)]
		public float CrewVitalityMultiplier { get; set; }

		// Token: 0x17000A00 RID: 2560
		// (get) Token: 0x060022CE RID: 8910 RVA: 0x000E9251 File Offset: 0x000E7451
		// (set) Token: 0x060022CF RID: 8911 RVA: 0x000E9259 File Offset: 0x000E7459
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(157)]
		public float NonCrewVitalityMultiplier { get; set; }

		// Token: 0x17000A01 RID: 2561
		// (get) Token: 0x060022D0 RID: 8912 RVA: 0x000E9262 File Offset: 0x000E7462
		// (set) Token: 0x060022D1 RID: 8913 RVA: 0x000E926A File Offset: 0x000E746A
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(160)]
		public float OxygenMultiplier { get; set; }

		// Token: 0x17000A02 RID: 2562
		// (get) Token: 0x060022D2 RID: 8914 RVA: 0x000E9273 File Offset: 0x000E7473
		// (set) Token: 0x060022D3 RID: 8915 RVA: 0x000E927B File Offset: 0x000E747B
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(163)]
		public float FuelMultiplier { get; set; }

		// Token: 0x17000A03 RID: 2563
		// (get) Token: 0x060022D4 RID: 8916 RVA: 0x000E9284 File Offset: 0x000E7484
		// (set) Token: 0x060022D5 RID: 8917 RVA: 0x000E928C File Offset: 0x000E748C
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(166)]
		public float MissionRewardMultiplier { get; set; }

		// Token: 0x17000A04 RID: 2564
		// (get) Token: 0x060022D6 RID: 8918 RVA: 0x000E9295 File Offset: 0x000E7495
		// (set) Token: 0x060022D7 RID: 8919 RVA: 0x000E929D File Offset: 0x000E749D
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(169)]
		public float ExperienceRewardMultiplier { get; set; }

		// Token: 0x17000A05 RID: 2565
		// (get) Token: 0x060022D8 RID: 8920 RVA: 0x000E92A6 File Offset: 0x000E74A6
		// (set) Token: 0x060022D9 RID: 8921 RVA: 0x000E92AE File Offset: 0x000E74AE
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(172)]
		public float ShopPriceMultiplier { get; set; }

		// Token: 0x17000A06 RID: 2566
		// (get) Token: 0x060022DA RID: 8922 RVA: 0x000E92B7 File Offset: 0x000E74B7
		// (set) Token: 0x060022DB RID: 8923 RVA: 0x000E92BF File Offset: 0x000E74BF
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(175)]
		public float ShipyardPriceMultiplier { get; set; }

		// Token: 0x17000A07 RID: 2567
		// (get) Token: 0x060022DC RID: 8924 RVA: 0x000E92C8 File Offset: 0x000E74C8
		// (set) Token: 0x060022DD RID: 8925 RVA: 0x000E92D0 File Offset: 0x000E74D0
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(178)]
		public float RepairFailMultiplier { get; set; }

		// Token: 0x17000A08 RID: 2568
		// (get) Token: 0x060022DE RID: 8926 RVA: 0x000E92D9 File Offset: 0x000E74D9
		// (set) Token: 0x060022DF RID: 8927 RVA: 0x000E92E1 File Offset: 0x000E74E1
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(181)]
		public bool ShowHuskWarning { get; set; }

		// Token: 0x17000A09 RID: 2569
		// (get) Token: 0x060022E0 RID: 8928 RVA: 0x000E92EA File Offset: 0x000E74EA
		// (set) Token: 0x060022E1 RID: 8929 RVA: 0x000E92F2 File Offset: 0x000E74F2
		[Serialize(PatdownProbabilityOption.Medium, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(184)]
		public PatdownProbabilityOption PatdownProbability { get; set; }

		// Token: 0x17000A0A RID: 2570
		// (get) Token: 0x060022E2 RID: 8930 RVA: 0x000E92FC File Offset: 0x000E74FC
		public float PatdownProbabilityMin
		{
			get
			{
				float? minPatdownProbability = this._minPatdownProbability;
				if (minPatdownProbability != null)
				{
					return minPatdownProbability.GetValueOrDefault();
				}
				this._minPatdownProbability = new float?(0.2f);
				Identifier settingDefinitionIdentifier = "PatdownProbabilityMin".ToIdentifier();
				Identifier attributeIdentifier = this.PatdownProbability.ToIdentifier<PatdownProbabilityOption>();
				XAttribute attribute;
				if (CampaignModePresets.TryGetAttribute(settingDefinitionIdentifier, attributeIdentifier, out attribute))
				{
					this._minPatdownProbability = new float?(attribute.GetAttributeFloat(0.2f));
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
					defaultInterpolatedStringHandler.AppendLiteral("CampaignSettings: Can't find value for ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(attributeIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(settingDefinitionIdentifier);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return this._minPatdownProbability.GetValueOrDefault(0.2f);
			}
		}

		// Token: 0x17000A0B RID: 2571
		// (get) Token: 0x060022E3 RID: 8931 RVA: 0x000E93C0 File Offset: 0x000E75C0
		public float PatdownProbabilityMax
		{
			get
			{
				float? maxPatdownProbability = this._maxPatdownProbability;
				if (maxPatdownProbability != null)
				{
					return maxPatdownProbability.GetValueOrDefault();
				}
				this._maxPatdownProbability = new float?(0.9f);
				Identifier settingDefinitionIdentifier = "PatdownProbabilityMax".ToIdentifier();
				Identifier attributeIdentifier = this.PatdownProbability.ToIdentifier<PatdownProbabilityOption>();
				XAttribute attribute;
				if (CampaignModePresets.TryGetAttribute(settingDefinitionIdentifier, attributeIdentifier, out attribute))
				{
					this._maxPatdownProbability = new float?(attribute.GetAttributeFloat(0.9f));
				}
				else
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 2);
					defaultInterpolatedStringHandler.AppendLiteral("CampaignSettings: Can't find value for ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(attributeIdentifier);
					defaultInterpolatedStringHandler.AppendLiteral(" in ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(settingDefinitionIdentifier);
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				return this._maxPatdownProbability.GetValueOrDefault(0.9f);
			}
		}

		// Token: 0x17000A0C RID: 2572
		// (get) Token: 0x060022E4 RID: 8932 RVA: 0x000E9484 File Offset: 0x000E7684
		// (set) Token: 0x060022E5 RID: 8933 RVA: 0x000E948C File Offset: 0x000E768C
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x060022E6 RID: 8934 RVA: 0x000E9495 File Offset: 0x000E7695
		public CampaignSettings()
		{
			this.SerializableProperties = SerializableProperty.GetProperties(this);
		}

		// Token: 0x060022E7 RID: 8935 RVA: 0x000E94B4 File Offset: 0x000E76B4
		[NullableContext(2)]
		public CampaignSettings(XElement element = null)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x060022E8 RID: 8936 RVA: 0x000E94D4 File Offset: 0x000E76D4
		public XElement Save()
		{
			XElement saveElement = new XElement("campaignsettings");
			SerializableProperty.SerializeProperties(this, saveElement, true, false);
			return saveElement;
		}

		// Token: 0x060022E9 RID: 8937 RVA: 0x000E94FC File Offset: 0x000E76FC
		private static int GetAddedMissionCount()
		{
			ImmutableHashSet<Character> characters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			if (!characters.Any<Character>())
			{
				return 0;
			}
			return characters.Max((Character character) => (int)character.GetStatValue(StatTypes.ExtraMissionCount, true));
		}

		// Token: 0x060022EA RID: 8938 RVA: 0x000E9540 File Offset: 0x000E7740
		public static CampaignSettings.MultiplierSettings GetMultiplierSettings(string multiplierName)
		{
			CampaignSettings.MultiplierSettings value;
			if (CampaignSettings._multiplierSettings.TryGetValue(multiplierName, out value))
			{
				return value;
			}
			return CampaignSettings._multiplierSettings["default"];
		}

		// Token: 0x040010BA RID: 4282
		public const string LowerCaseSaveElementName = "campaignsettings";

		// Token: 0x040010BE RID: 4286
		public const int DefaultMaxMissionCount = 2;

		// Token: 0x040010BF RID: 4287
		public const int MaxMissionCountLimit = 10;

		// Token: 0x040010C0 RID: 4288
		public const int MinMissionCountLimit = 1;

		// Token: 0x040010C1 RID: 4289
		private int maxMissionCount;

		// Token: 0x040010C5 RID: 4293
		private int? _initialMoney;

		// Token: 0x040010C6 RID: 4294
		public const int DefaultInitialMoney = 8000;

		// Token: 0x040010C7 RID: 4295
		private float? _extraEventManagerDifficulty;

		// Token: 0x040010C8 RID: 4296
		private const float defaultExtraEventManagerDifficulty = 0f;

		// Token: 0x040010C9 RID: 4297
		private float? _levelDifficultyMultiplier;

		// Token: 0x040010CA RID: 4298
		private const float defaultLevelDifficultyMultiplier = 1f;

		// Token: 0x040010CB RID: 4299
		private static readonly Dictionary<string, CampaignSettings.MultiplierSettings> _multiplierSettings = new Dictionary<string, CampaignSettings.MultiplierSettings>
		{
			{
				"default",
				new CampaignSettings.MultiplierSettings
				{
					Min = 0.2f,
					Max = 2f,
					Step = 0.1f
				}
			},
			{
				"CrewVitalityMultiplier",
				new CampaignSettings.MultiplierSettings
				{
					Min = 0.5f,
					Max = 2f,
					Step = 0.1f
				}
			},
			{
				"NonCrewVitalityMultiplier",
				new CampaignSettings.MultiplierSettings
				{
					Min = 0.5f,
					Max = 3f,
					Step = 0.1f
				}
			},
			{
				"MissionRewardMultiplier",
				new CampaignSettings.MultiplierSettings
				{
					Min = 0.5f,
					Max = 2f,
					Step = 0.1f
				}
			},
			{
				"ExperienceRewardMultiplier",
				new CampaignSettings.MultiplierSettings
				{
					Min = 0.5f,
					Max = 2f,
					Step = 0.1f
				}
			},
			{
				"RepairFailMultiplier",
				new CampaignSettings.MultiplierSettings
				{
					Min = 0.5f,
					Max = 5f,
					Step = 0.5f
				}
			},
			{
				"ShopPriceMultiplier",
				new CampaignSettings.MultiplierSettings
				{
					Min = 0.1f,
					Max = 3f,
					Step = 0.1f
				}
			},
			{
				"ShipyardPriceMultiplier",
				new CampaignSettings.MultiplierSettings
				{
					Min = 0.1f,
					Max = 3f,
					Step = 0.1f
				}
			}
		};

		// Token: 0x040010D7 RID: 4311
		private float? _minPatdownProbability;

		// Token: 0x040010D8 RID: 4312
		private float? _maxPatdownProbability;

		// Token: 0x040010D9 RID: 4313
		public const float DefaultMinPatdownProbability = 0.2f;

		// Token: 0x040010DA RID: 4314
		public const float DefaultMaxPatdownProbability = 0.9f;

		// Token: 0x0200098C RID: 2444
		[NullableContext(0)]
		public struct MultiplierSettings
		{
			// Token: 0x1700155B RID: 5467
			// (get) Token: 0x060059FA RID: 23034 RVA: 0x001FB31B File Offset: 0x001F951B
			// (set) Token: 0x060059FB RID: 23035 RVA: 0x001FB323 File Offset: 0x001F9523
			public float Min { readonly get; set; }

			// Token: 0x1700155C RID: 5468
			// (get) Token: 0x060059FC RID: 23036 RVA: 0x001FB32C File Offset: 0x001F952C
			// (set) Token: 0x060059FD RID: 23037 RVA: 0x001FB334 File Offset: 0x001F9534
			public float Max { readonly get; set; }

			// Token: 0x1700155D RID: 5469
			// (get) Token: 0x060059FE RID: 23038 RVA: 0x001FB33D File Offset: 0x001F953D
			// (set) Token: 0x060059FF RID: 23039 RVA: 0x001FB345 File Offset: 0x001F9545
			public float Step { readonly get; set; }
		}
	}
}
