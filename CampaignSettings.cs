using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002CF RID: 719
	[NullableContext(1)]
	[Nullable(0)]
	internal class CampaignSettings : INetSerializableStruct, ISerializableEntity
	{
		// Token: 0x17000FFE RID: 4094
		// (get) Token: 0x06003CBA RID: 15546 RVA: 0x0022BFBC File Offset: 0x0022A1BC
		public static CampaignSettings Empty
		{
			get
			{
				return new CampaignSettings(null);
			}
		}

		// Token: 0x17000FFF RID: 4095
		// (get) Token: 0x06003CBB RID: 15547 RVA: 0x0022BFC4 File Offset: 0x0022A1C4
		public string Name
		{
			get
			{
				return "CampaignSettings";
			}
		}

		// Token: 0x17001000 RID: 4096
		// (get) Token: 0x06003CBC RID: 15548 RVA: 0x0022BFCB File Offset: 0x0022A1CB
		// (set) Token: 0x06003CBD RID: 15549 RVA: 0x0022BFD3 File Offset: 0x0022A1D3
		[Serialize("Normal", IsPropertySaveable.Yes, "", "", false)]
		public string PresetName { get; set; } = string.Empty;

		// Token: 0x17001001 RID: 4097
		// (get) Token: 0x06003CBE RID: 15550 RVA: 0x0022BFDC File Offset: 0x0022A1DC
		// (set) Token: 0x06003CBF RID: 15551 RVA: 0x0022BFE4 File Offset: 0x0022A1E4
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		public bool TutorialEnabled { get; set; }

		// Token: 0x17001002 RID: 4098
		// (get) Token: 0x06003CC0 RID: 15552 RVA: 0x0022BFED File Offset: 0x0022A1ED
		// (set) Token: 0x06003CC1 RID: 15553 RVA: 0x0022BFF5 File Offset: 0x0022A1F5
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(27)]
		public bool RadiationEnabled { get; set; }

		// Token: 0x17001003 RID: 4099
		// (get) Token: 0x06003CC2 RID: 15554 RVA: 0x0022BFFE File Offset: 0x0022A1FE
		// (set) Token: 0x06003CC3 RID: 15555 RVA: 0x0022C006 File Offset: 0x0022A206
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

		// Token: 0x17001004 RID: 4100
		// (get) Token: 0x06003CC4 RID: 15556 RVA: 0x0022C017 File Offset: 0x0022A217
		public int TotalMaxMissionCount
		{
			get
			{
				return this.MaxMissionCount + CampaignSettings.GetAddedMissionCount();
			}
		}

		// Token: 0x17001005 RID: 4101
		// (get) Token: 0x06003CC5 RID: 15557 RVA: 0x0022C025 File Offset: 0x0022A225
		// (set) Token: 0x06003CC6 RID: 15558 RVA: 0x0022C02D File Offset: 0x0022A22D
		[Serialize(WorldHostilityOption.Medium, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(45)]
		public WorldHostilityOption WorldHostility { get; set; }

		// Token: 0x17001006 RID: 4102
		// (get) Token: 0x06003CC7 RID: 15559 RVA: 0x0022C036 File Offset: 0x0022A236
		// (set) Token: 0x06003CC8 RID: 15560 RVA: 0x0022C03E File Offset: 0x0022A23E
		[Serialize("normal", IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(48)]
		public Identifier StartItemSet { get; set; }

		// Token: 0x17001007 RID: 4103
		// (get) Token: 0x06003CC9 RID: 15561 RVA: 0x0022C047 File Offset: 0x0022A247
		// (set) Token: 0x06003CCA RID: 15562 RVA: 0x0022C04F File Offset: 0x0022A24F
		[Serialize(StartingBalanceAmountOption.Medium, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(51)]
		public StartingBalanceAmountOption StartingBalanceAmount { get; set; }

		// Token: 0x17001008 RID: 4104
		// (get) Token: 0x06003CCB RID: 15563 RVA: 0x0022C058 File Offset: 0x0022A258
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

		// Token: 0x17001009 RID: 4105
		// (get) Token: 0x06003CCC RID: 15564 RVA: 0x0022C11C File Offset: 0x0022A31C
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

		// Token: 0x1700100A RID: 4106
		// (get) Token: 0x06003CCD RID: 15565 RVA: 0x0022C1DC File Offset: 0x0022A3DC
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

		// Token: 0x1700100B RID: 4107
		// (get) Token: 0x06003CCE RID: 15566 RVA: 0x0022C2A0 File Offset: 0x0022A4A0
		// (set) Token: 0x06003CCF RID: 15567 RVA: 0x0022C2A8 File Offset: 0x0022A4A8
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(154)]
		public float CrewVitalityMultiplier { get; set; }

		// Token: 0x1700100C RID: 4108
		// (get) Token: 0x06003CD0 RID: 15568 RVA: 0x0022C2B1 File Offset: 0x0022A4B1
		// (set) Token: 0x06003CD1 RID: 15569 RVA: 0x0022C2B9 File Offset: 0x0022A4B9
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(157)]
		public float NonCrewVitalityMultiplier { get; set; }

		// Token: 0x1700100D RID: 4109
		// (get) Token: 0x06003CD2 RID: 15570 RVA: 0x0022C2C2 File Offset: 0x0022A4C2
		// (set) Token: 0x06003CD3 RID: 15571 RVA: 0x0022C2CA File Offset: 0x0022A4CA
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(160)]
		public float OxygenMultiplier { get; set; }

		// Token: 0x1700100E RID: 4110
		// (get) Token: 0x06003CD4 RID: 15572 RVA: 0x0022C2D3 File Offset: 0x0022A4D3
		// (set) Token: 0x06003CD5 RID: 15573 RVA: 0x0022C2DB File Offset: 0x0022A4DB
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(163)]
		public float FuelMultiplier { get; set; }

		// Token: 0x1700100F RID: 4111
		// (get) Token: 0x06003CD6 RID: 15574 RVA: 0x0022C2E4 File Offset: 0x0022A4E4
		// (set) Token: 0x06003CD7 RID: 15575 RVA: 0x0022C2EC File Offset: 0x0022A4EC
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(166)]
		public float MissionRewardMultiplier { get; set; }

		// Token: 0x17001010 RID: 4112
		// (get) Token: 0x06003CD8 RID: 15576 RVA: 0x0022C2F5 File Offset: 0x0022A4F5
		// (set) Token: 0x06003CD9 RID: 15577 RVA: 0x0022C2FD File Offset: 0x0022A4FD
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(169)]
		public float ExperienceRewardMultiplier { get; set; }

		// Token: 0x17001011 RID: 4113
		// (get) Token: 0x06003CDA RID: 15578 RVA: 0x0022C306 File Offset: 0x0022A506
		// (set) Token: 0x06003CDB RID: 15579 RVA: 0x0022C30E File Offset: 0x0022A50E
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(172)]
		public float ShopPriceMultiplier { get; set; }

		// Token: 0x17001012 RID: 4114
		// (get) Token: 0x06003CDC RID: 15580 RVA: 0x0022C317 File Offset: 0x0022A517
		// (set) Token: 0x06003CDD RID: 15581 RVA: 0x0022C31F File Offset: 0x0022A51F
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(175)]
		public float ShipyardPriceMultiplier { get; set; }

		// Token: 0x17001013 RID: 4115
		// (get) Token: 0x06003CDE RID: 15582 RVA: 0x0022C328 File Offset: 0x0022A528
		// (set) Token: 0x06003CDF RID: 15583 RVA: 0x0022C330 File Offset: 0x0022A530
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(178)]
		public float RepairFailMultiplier { get; set; }

		// Token: 0x17001014 RID: 4116
		// (get) Token: 0x06003CE0 RID: 15584 RVA: 0x0022C339 File Offset: 0x0022A539
		// (set) Token: 0x06003CE1 RID: 15585 RVA: 0x0022C341 File Offset: 0x0022A541
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(181)]
		public bool ShowHuskWarning { get; set; }

		// Token: 0x17001015 RID: 4117
		// (get) Token: 0x06003CE2 RID: 15586 RVA: 0x0022C34A File Offset: 0x0022A54A
		// (set) Token: 0x06003CE3 RID: 15587 RVA: 0x0022C352 File Offset: 0x0022A552
		[Serialize(PatdownProbabilityOption.Medium, IsPropertySaveable.Yes, "", "", false)]
		[NetworkSerialize(184)]
		public PatdownProbabilityOption PatdownProbability { get; set; }

		// Token: 0x17001016 RID: 4118
		// (get) Token: 0x06003CE4 RID: 15588 RVA: 0x0022C35C File Offset: 0x0022A55C
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

		// Token: 0x17001017 RID: 4119
		// (get) Token: 0x06003CE5 RID: 15589 RVA: 0x0022C420 File Offset: 0x0022A620
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

		// Token: 0x17001018 RID: 4120
		// (get) Token: 0x06003CE6 RID: 15590 RVA: 0x0022C4E4 File Offset: 0x0022A6E4
		// (set) Token: 0x06003CE7 RID: 15591 RVA: 0x0022C4EC File Offset: 0x0022A6EC
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x06003CE8 RID: 15592 RVA: 0x0022C4F5 File Offset: 0x0022A6F5
		public CampaignSettings()
		{
			this.SerializableProperties = SerializableProperty.GetProperties(this);
		}

		// Token: 0x06003CE9 RID: 15593 RVA: 0x0022C514 File Offset: 0x0022A714
		[NullableContext(2)]
		public CampaignSettings(XElement element = null)
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
		}

		// Token: 0x06003CEA RID: 15594 RVA: 0x0022C534 File Offset: 0x0022A734
		public XElement Save()
		{
			XElement saveElement = new XElement("campaignsettings");
			SerializableProperty.SerializeProperties(this, saveElement, true, false);
			return saveElement;
		}

		// Token: 0x06003CEB RID: 15595 RVA: 0x0022C55C File Offset: 0x0022A75C
		private static int GetAddedMissionCount()
		{
			ImmutableHashSet<Character> characters = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			if (!characters.Any<Character>())
			{
				return 0;
			}
			return characters.Max((Character character) => (int)character.GetStatValue(StatTypes.ExtraMissionCount, true));
		}

		// Token: 0x06003CEC RID: 15596 RVA: 0x0022C5A0 File Offset: 0x0022A7A0
		public static CampaignSettings.MultiplierSettings GetMultiplierSettings(string multiplierName)
		{
			CampaignSettings.MultiplierSettings value;
			if (CampaignSettings._multiplierSettings.TryGetValue(multiplierName, out value))
			{
				return value;
			}
			return CampaignSettings._multiplierSettings["default"];
		}

		// Token: 0x04001F56 RID: 8022
		public static CampaignSettings CurrentSettings = new CampaignSettings(GameSettings.CurrentConfig.SavedCampaignSettings);

		// Token: 0x04001F57 RID: 8023
		public const string LowerCaseSaveElementName = "campaignsettings";

		// Token: 0x04001F5B RID: 8027
		public const int DefaultMaxMissionCount = 2;

		// Token: 0x04001F5C RID: 8028
		public const int MaxMissionCountLimit = 10;

		// Token: 0x04001F5D RID: 8029
		public const int MinMissionCountLimit = 1;

		// Token: 0x04001F5E RID: 8030
		private int maxMissionCount;

		// Token: 0x04001F62 RID: 8034
		private int? _initialMoney;

		// Token: 0x04001F63 RID: 8035
		public const int DefaultInitialMoney = 8000;

		// Token: 0x04001F64 RID: 8036
		private float? _extraEventManagerDifficulty;

		// Token: 0x04001F65 RID: 8037
		private const float defaultExtraEventManagerDifficulty = 0f;

		// Token: 0x04001F66 RID: 8038
		private float? _levelDifficultyMultiplier;

		// Token: 0x04001F67 RID: 8039
		private const float defaultLevelDifficultyMultiplier = 1f;

		// Token: 0x04001F68 RID: 8040
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

		// Token: 0x04001F74 RID: 8052
		private float? _minPatdownProbability;

		// Token: 0x04001F75 RID: 8053
		private float? _maxPatdownProbability;

		// Token: 0x04001F76 RID: 8054
		public const float DefaultMinPatdownProbability = 0.2f;

		// Token: 0x04001F77 RID: 8055
		public const float DefaultMaxPatdownProbability = 0.9f;

		// Token: 0x02000F7C RID: 3964
		[NullableContext(0)]
		public struct MultiplierSettings
		{
			// Token: 0x17001C23 RID: 7203
			// (get) Token: 0x06008935 RID: 35125 RVA: 0x003A7A57 File Offset: 0x003A5C57
			// (set) Token: 0x06008936 RID: 35126 RVA: 0x003A7A5F File Offset: 0x003A5C5F
			public float Min { readonly get; set; }

			// Token: 0x17001C24 RID: 7204
			// (get) Token: 0x06008937 RID: 35127 RVA: 0x003A7A68 File Offset: 0x003A5C68
			// (set) Token: 0x06008938 RID: 35128 RVA: 0x003A7A70 File Offset: 0x003A5C70
			public float Max { readonly get; set; }

			// Token: 0x17001C25 RID: 7205
			// (get) Token: 0x06008939 RID: 35129 RVA: 0x003A7A79 File Offset: 0x003A5C79
			// (set) Token: 0x0600893A RID: 35130 RVA: 0x003A7A81 File Offset: 0x003A5C81
			public float Step { readonly get; set; }
		}
	}
}
