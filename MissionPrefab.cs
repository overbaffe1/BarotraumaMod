using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200005A RID: 90
	internal sealed class MissionPrefab : PrefabWithUintIdentifier, IImplementsVariants<MissionPrefab>
	{
		// Token: 0x1700035A RID: 858
		// (get) Token: 0x06000C1F RID: 3103 RVA: 0x00070A28 File Offset: 0x0006EC28
		public bool HasPortraits
		{
			get
			{
				return this.portraits.Length > 0;
			}
		}

		// Token: 0x1700035B RID: 859
		// (get) Token: 0x06000C20 RID: 3104 RVA: 0x00070A38 File Offset: 0x0006EC38
		// (set) Token: 0x06000C21 RID: 3105 RVA: 0x00070A40 File Offset: 0x0006EC40
		public Sprite Icon { get; private set; }

		// Token: 0x1700035C RID: 860
		// (get) Token: 0x06000C22 RID: 3106 RVA: 0x00070A49 File Offset: 0x0006EC49
		// (set) Token: 0x06000C23 RID: 3107 RVA: 0x00070A51 File Offset: 0x0006EC51
		public Color IconColor { get; private set; }

		// Token: 0x1700035D RID: 861
		// (get) Token: 0x06000C24 RID: 3108 RVA: 0x00070A5A File Offset: 0x0006EC5A
		// (set) Token: 0x06000C25 RID: 3109 RVA: 0x00070A62 File Offset: 0x0006EC62
		public bool DisplayTargetHudIcons { get; private set; }

		// Token: 0x1700035E RID: 862
		// (get) Token: 0x06000C26 RID: 3110 RVA: 0x00070A6B File Offset: 0x0006EC6B
		// (set) Token: 0x06000C27 RID: 3111 RVA: 0x00070A73 File Offset: 0x0006EC73
		public float HudIconMaxDistance { get; private set; }

		// Token: 0x1700035F RID: 863
		// (get) Token: 0x06000C28 RID: 3112 RVA: 0x00070A7C File Offset: 0x0006EC7C
		public Sprite HudIcon
		{
			get
			{
				return this.hudIcon ?? this.Icon;
			}
		}

		// Token: 0x17000360 RID: 864
		// (get) Token: 0x06000C29 RID: 3113 RVA: 0x00070A90 File Offset: 0x0006EC90
		public Color HudIconColor
		{
			get
			{
				Color? color = this.hudIconColor;
				if (color == null)
				{
					return this.IconColor;
				}
				return color.GetValueOrDefault();
			}
		}

		// Token: 0x17000361 RID: 865
		// (get) Token: 0x06000C2A RID: 3114 RVA: 0x00070ABB File Offset: 0x0006ECBB
		// (set) Token: 0x06000C2B RID: 3115 RVA: 0x00070AC3 File Offset: 0x0006ECC3
		public Color ProgressBarColor { get; private set; }

		// Token: 0x06000C2C RID: 3116 RVA: 0x00070ACC File Offset: 0x0006ECCC
		private void ParseConfigElementClient(ContentXElement element, MissionPrefab variantOf = null)
		{
			this.DisplayTargetHudIcons = element.GetAttributeBool("displaytargethudicons", false);
			this.HudIconMaxDistance = element.GetAttributeFloat("hudiconmaxdistance", 1000f);
			Dictionary<int, Identifier> overrideMusic = new Dictionary<int, Identifier>();
			List<Sprite> portraits = new List<Sprite>();
			Color color;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "icon"))
				{
					if (!(a == "hudicon"))
					{
						if (!(a == "overridemusic"))
						{
							if (a == "portrait")
							{
								Sprite portrait = new Sprite(subElement, this.GetTexturePath(subElement, variantOf), "", true, 1f);
								if (portrait != null)
								{
									portraits.Add(portrait);
								}
							}
						}
						else
						{
							overrideMusic.Add(subElement.GetAttributeInt("state", 0), subElement.GetAttributeIdentifier("type", Identifier.Empty));
						}
					}
					else
					{
						this.hudIcon = new Sprite(subElement, this.GetTexturePath(subElement, variantOf), "", false, 1f);
						this.hudIconColor = subElement.GetAttributeColor("color");
					}
				}
				else
				{
					this.Icon = new Sprite(subElement, this.GetTexturePath(subElement, variantOf), "", false, 1f);
					ContentXElement contentXElement = subElement;
					string key = "color";
					color = Color.White;
					this.IconColor = contentXElement.GetAttributeColor(key, color);
				}
			}
			this.portraits = ImmutableCollectionsMarshal.AsImmutableArray<Sprite>(portraits.ToArray());
			this.overrideMusicOnState = overrideMusic.ToImmutableDictionary<int, Identifier>();
			string key2 = "ProgressBarColor";
			color = GUIStyle.Blue;
			this.ProgressBarColor = element.GetAttributeColor(key2, color);
		}

		// Token: 0x06000C2D RID: 3117 RVA: 0x00070CA4 File Offset: 0x0006EEA4
		public Identifier GetOverrideMusicType(int state)
		{
			Identifier id;
			if (this.overrideMusicOnState.TryGetValue(state, out id))
			{
				return id;
			}
			return Identifier.Empty;
		}

		// Token: 0x06000C2E RID: 3118 RVA: 0x00070CC8 File Offset: 0x0006EEC8
		public Sprite GetPortrait(int randomSeed)
		{
			if (this.portraits.Length == 0)
			{
				return null;
			}
			return this.portraits[Math.Abs(randomSeed) % this.portraits.Length];
		}

		// Token: 0x06000C2F RID: 3119 RVA: 0x00070CF6 File Offset: 0x0006EEF6
		public string GetTexturePath(ContentXElement subElement, MissionPrefab variantOf = null)
		{
			if (!subElement.DoesAttributeReferenceFileNameAlone("texture"))
			{
				return "";
			}
			return Path.GetDirectoryName(((variantOf != null) ? variantOf.ContentFile.Path : null) ?? this.ContentFile.Path);
		}

		// Token: 0x17000362 RID: 866
		// (get) Token: 0x06000C30 RID: 3120 RVA: 0x00070D30 File Offset: 0x0006EF30
		// (set) Token: 0x06000C31 RID: 3121 RVA: 0x00070D38 File Offset: 0x0006EF38
		public Identifier Type { get; private set; }

		// Token: 0x17000363 RID: 867
		// (get) Token: 0x06000C32 RID: 3122 RVA: 0x00070D41 File Offset: 0x0006EF41
		// (set) Token: 0x06000C33 RID: 3123 RVA: 0x00070D49 File Offset: 0x0006EF49
		public Type MissionClass { get; private set; }

		// Token: 0x17000364 RID: 868
		// (get) Token: 0x06000C34 RID: 3124 RVA: 0x00070D52 File Offset: 0x0006EF52
		// (set) Token: 0x06000C35 RID: 3125 RVA: 0x00070D5A File Offset: 0x0006EF5A
		public bool CampaignOnly { get; private set; }

		// Token: 0x17000365 RID: 869
		// (get) Token: 0x06000C36 RID: 3126 RVA: 0x00070D63 File Offset: 0x0006EF63
		// (set) Token: 0x06000C37 RID: 3127 RVA: 0x00070D6B File Offset: 0x0006EF6B
		public bool MultiplayerOnly { get; private set; }

		// Token: 0x17000366 RID: 870
		// (get) Token: 0x06000C38 RID: 3128 RVA: 0x00070D74 File Offset: 0x0006EF74
		// (set) Token: 0x06000C39 RID: 3129 RVA: 0x00070D7C File Offset: 0x0006EF7C
		public bool SingleplayerOnly { get; private set; }

		// Token: 0x17000367 RID: 871
		// (get) Token: 0x06000C3A RID: 3130 RVA: 0x00070D85 File Offset: 0x0006EF85
		// (set) Token: 0x06000C3B RID: 3131 RVA: 0x00070D8D File Offset: 0x0006EF8D
		public Identifier TextIdentifier { get; private set; }

		// Token: 0x17000368 RID: 872
		// (get) Token: 0x06000C3C RID: 3132 RVA: 0x00070D96 File Offset: 0x0006EF96
		// (set) Token: 0x06000C3D RID: 3133 RVA: 0x00070D9E File Offset: 0x0006EF9E
		public ImmutableHashSet<Identifier> Tags { get; private set; }

		// Token: 0x17000369 RID: 873
		// (get) Token: 0x06000C3E RID: 3134 RVA: 0x00070DA7 File Offset: 0x0006EFA7
		// (set) Token: 0x06000C3F RID: 3135 RVA: 0x00070DAF File Offset: 0x0006EFAF
		public LocalizedString Name { get; private set; }

		// Token: 0x1700036A RID: 874
		// (get) Token: 0x06000C40 RID: 3136 RVA: 0x00070DB8 File Offset: 0x0006EFB8
		// (set) Token: 0x06000C41 RID: 3137 RVA: 0x00070DC0 File Offset: 0x0006EFC0
		public LocalizedString Description { get; private set; }

		// Token: 0x1700036B RID: 875
		// (get) Token: 0x06000C42 RID: 3138 RVA: 0x00070DC9 File Offset: 0x0006EFC9
		// (set) Token: 0x06000C43 RID: 3139 RVA: 0x00070DD1 File Offset: 0x0006EFD1
		public LocalizedString SuccessMessage { get; private set; }

		// Token: 0x1700036C RID: 876
		// (get) Token: 0x06000C44 RID: 3140 RVA: 0x00070DDA File Offset: 0x0006EFDA
		// (set) Token: 0x06000C45 RID: 3141 RVA: 0x00070DE2 File Offset: 0x0006EFE2
		public LocalizedString FailureMessage { get; private set; }

		// Token: 0x1700036D RID: 877
		// (get) Token: 0x06000C46 RID: 3142 RVA: 0x00070DEB File Offset: 0x0006EFEB
		// (set) Token: 0x06000C47 RID: 3143 RVA: 0x00070DF3 File Offset: 0x0006EFF3
		public LocalizedString SonarLabel { get; private set; }

		// Token: 0x1700036E RID: 878
		// (get) Token: 0x06000C48 RID: 3144 RVA: 0x00070DFC File Offset: 0x0006EFFC
		// (set) Token: 0x06000C49 RID: 3145 RVA: 0x00070E04 File Offset: 0x0006F004
		public Identifier SonarIconIdentifier { get; private set; }

		// Token: 0x1700036F RID: 879
		// (get) Token: 0x06000C4A RID: 3146 RVA: 0x00070E0D File Offset: 0x0006F00D
		// (set) Token: 0x06000C4B RID: 3147 RVA: 0x00070E15 File Offset: 0x0006F015
		public Identifier AchievementIdentifier { get; private set; }

		// Token: 0x17000370 RID: 880
		// (get) Token: 0x06000C4C RID: 3148 RVA: 0x00070E1E File Offset: 0x0006F01E
		// (set) Token: 0x06000C4D RID: 3149 RVA: 0x00070E26 File Offset: 0x0006F026
		public ImmutableList<MissionPrefab.ReputationReward> ReputationRewards { get; private set; }

		// Token: 0x17000371 RID: 881
		// (get) Token: 0x06000C4E RID: 3150 RVA: 0x00070E2F File Offset: 0x0006F02F
		// (set) Token: 0x06000C4F RID: 3151 RVA: 0x00070E37 File Offset: 0x0006F037
		public int Commonness { get; private set; }

		// Token: 0x17000372 RID: 882
		// (get) Token: 0x06000C50 RID: 3152 RVA: 0x00070E40 File Offset: 0x0006F040
		// (set) Token: 0x06000C51 RID: 3153 RVA: 0x00070E48 File Offset: 0x0006F048
		public int? Difficulty { get; private set; }

		// Token: 0x17000373 RID: 883
		// (get) Token: 0x06000C52 RID: 3154 RVA: 0x00070E51 File Offset: 0x0006F051
		// (set) Token: 0x06000C53 RID: 3155 RVA: 0x00070E59 File Offset: 0x0006F059
		public int MinLevelDifficulty { get; private set; }

		// Token: 0x17000374 RID: 884
		// (get) Token: 0x06000C54 RID: 3156 RVA: 0x00070E62 File Offset: 0x0006F062
		// (set) Token: 0x06000C55 RID: 3157 RVA: 0x00070E6A File Offset: 0x0006F06A
		public int MaxLevelDifficulty { get; private set; } = 100;

		// Token: 0x17000375 RID: 885
		// (get) Token: 0x06000C56 RID: 3158 RVA: 0x00070E73 File Offset: 0x0006F073
		// (set) Token: 0x06000C57 RID: 3159 RVA: 0x00070E7B File Offset: 0x0006F07B
		public int Reward { get; private set; }

		// Token: 0x17000376 RID: 886
		// (get) Token: 0x06000C58 RID: 3160 RVA: 0x00070E84 File Offset: 0x0006F084
		// (set) Token: 0x06000C59 RID: 3161 RVA: 0x00070E8C File Offset: 0x0006F08C
		public float ExperienceMultiplier { get; private set; }

		// Token: 0x17000377 RID: 887
		// (get) Token: 0x06000C5A RID: 3162 RVA: 0x00070E95 File Offset: 0x0006F095
		// (set) Token: 0x06000C5B RID: 3163 RVA: 0x00070E9D File Offset: 0x0006F09D
		public ImmutableArray<LocalizedString> Headers { get; private set; }

		// Token: 0x17000378 RID: 888
		// (get) Token: 0x06000C5C RID: 3164 RVA: 0x00070EA6 File Offset: 0x0006F0A6
		// (set) Token: 0x06000C5D RID: 3165 RVA: 0x00070EAE File Offset: 0x0006F0AE
		public ImmutableArray<LocalizedString> Messages { get; private set; }

		// Token: 0x17000379 RID: 889
		// (get) Token: 0x06000C5E RID: 3166 RVA: 0x00070EB7 File Offset: 0x0006F0B7
		// (set) Token: 0x06000C5F RID: 3167 RVA: 0x00070EBF File Offset: 0x0006F0BF
		public bool AllowRetry { get; private set; }

		// Token: 0x1700037A RID: 890
		// (get) Token: 0x06000C60 RID: 3168 RVA: 0x00070EC8 File Offset: 0x0006F0C8
		// (set) Token: 0x06000C61 RID: 3169 RVA: 0x00070ED0 File Offset: 0x0006F0D0
		public bool ShowSonarLabels { get; private set; }

		// Token: 0x1700037B RID: 891
		// (get) Token: 0x06000C62 RID: 3170 RVA: 0x00070ED9 File Offset: 0x0006F0D9
		// (set) Token: 0x06000C63 RID: 3171 RVA: 0x00070EE1 File Offset: 0x0006F0E1
		public bool ShowInMenus { get; private set; }

		// Token: 0x1700037C RID: 892
		// (get) Token: 0x06000C64 RID: 3172 RVA: 0x00070EEA File Offset: 0x0006F0EA
		// (set) Token: 0x06000C65 RID: 3173 RVA: 0x00070EF2 File Offset: 0x0006F0F2
		public bool ShowStartMessage { get; private set; }

		// Token: 0x1700037D RID: 893
		// (get) Token: 0x06000C66 RID: 3174 RVA: 0x00070EFB File Offset: 0x0006F0FB
		// (set) Token: 0x06000C67 RID: 3175 RVA: 0x00070F03 File Offset: 0x0006F103
		public bool IsSideObjective { get; private set; }

		// Token: 0x1700037E RID: 894
		// (get) Token: 0x06000C68 RID: 3176 RVA: 0x00070F0C File Offset: 0x0006F10C
		// (set) Token: 0x06000C69 RID: 3177 RVA: 0x00070F14 File Offset: 0x0006F114
		public bool AllowOtherMissionsInLevel { get; private set; }

		// Token: 0x1700037F RID: 895
		// (get) Token: 0x06000C6A RID: 3178 RVA: 0x00070F1D File Offset: 0x0006F11D
		// (set) Token: 0x06000C6B RID: 3179 RVA: 0x00070F25 File Offset: 0x0006F125
		public bool RequireWreck { get; private set; }

		// Token: 0x17000380 RID: 896
		// (get) Token: 0x06000C6C RID: 3180 RVA: 0x00070F2E File Offset: 0x0006F12E
		// (set) Token: 0x06000C6D RID: 3181 RVA: 0x00070F36 File Offset: 0x0006F136
		public bool RequireRuin { get; private set; }

		// Token: 0x17000381 RID: 897
		// (get) Token: 0x06000C6E RID: 3182 RVA: 0x00070F3F File Offset: 0x0006F13F
		// (set) Token: 0x06000C6F RID: 3183 RVA: 0x00070F47 File Offset: 0x0006F147
		public bool RequireBeaconStation { get; private set; }

		// Token: 0x17000382 RID: 898
		// (get) Token: 0x06000C70 RID: 3184 RVA: 0x00070F50 File Offset: 0x0006F150
		// (set) Token: 0x06000C71 RID: 3185 RVA: 0x00070F58 File Offset: 0x0006F158
		public bool RequireThalamusWreck { get; private set; }

		// Token: 0x17000383 RID: 899
		// (get) Token: 0x06000C72 RID: 3186 RVA: 0x00070F61 File Offset: 0x0006F161
		// (set) Token: 0x06000C73 RID: 3187 RVA: 0x00070F69 File Offset: 0x0006F169
		public bool SpawnBeaconStationInMiddle { get; private set; }

		// Token: 0x17000384 RID: 900
		// (get) Token: 0x06000C74 RID: 3188 RVA: 0x00070F72 File Offset: 0x0006F172
		// (set) Token: 0x06000C75 RID: 3189 RVA: 0x00070F7A File Offset: 0x0006F17A
		public bool AllowOutpostNPCs { get; private set; }

		// Token: 0x17000385 RID: 901
		// (get) Token: 0x06000C76 RID: 3190 RVA: 0x00070F83 File Offset: 0x0006F183
		// (set) Token: 0x06000C77 RID: 3191 RVA: 0x00070F8B File Offset: 0x0006F18B
		public Identifier ForceOutpostGenerationParameters { get; private set; }

		// Token: 0x17000386 RID: 902
		// (get) Token: 0x06000C78 RID: 3192 RVA: 0x00070F94 File Offset: 0x0006F194
		// (set) Token: 0x06000C79 RID: 3193 RVA: 0x00070F9C File Offset: 0x0006F19C
		public RespawnMode? ForceRespawnMode { get; private set; }

		// Token: 0x17000387 RID: 903
		// (get) Token: 0x06000C7A RID: 3194 RVA: 0x00070FA5 File Offset: 0x0006F1A5
		// (set) Token: 0x06000C7B RID: 3195 RVA: 0x00070FAD File Offset: 0x0006F1AD
		public Identifier AllowOutpostSelectionFromTag { get; private set; }

		// Token: 0x17000388 RID: 904
		// (get) Token: 0x06000C7C RID: 3196 RVA: 0x00070FB6 File Offset: 0x0006F1B6
		// (set) Token: 0x06000C7D RID: 3197 RVA: 0x00070FBE File Offset: 0x0006F1BE
		public bool LoadSubmarines { get; private set; } = true;

		// Token: 0x17000389 RID: 905
		// (get) Token: 0x06000C7E RID: 3198 RVA: 0x00070FC7 File Offset: 0x0006F1C7
		// (set) Token: 0x06000C7F RID: 3199 RVA: 0x00070FCF File Offset: 0x0006F1CF
		public bool BlockLocationTypeChanges { get; private set; }

		// Token: 0x1700038A RID: 906
		// (get) Token: 0x06000C80 RID: 3200 RVA: 0x00070FD8 File Offset: 0x0006F1D8
		// (set) Token: 0x06000C81 RID: 3201 RVA: 0x00070FE0 File Offset: 0x0006F1E0
		public bool ShowProgressBar { get; private set; }

		// Token: 0x1700038B RID: 907
		// (get) Token: 0x06000C82 RID: 3202 RVA: 0x00070FE9 File Offset: 0x0006F1E9
		// (set) Token: 0x06000C83 RID: 3203 RVA: 0x00070FF1 File Offset: 0x0006F1F1
		public bool ShowProgressInNumbers { get; private set; }

		// Token: 0x1700038C RID: 908
		// (get) Token: 0x06000C84 RID: 3204 RVA: 0x00070FFA File Offset: 0x0006F1FA
		// (set) Token: 0x06000C85 RID: 3205 RVA: 0x00071002 File Offset: 0x0006F202
		public int MaxProgressState { get; private set; }

		// Token: 0x1700038D RID: 909
		// (get) Token: 0x06000C86 RID: 3206 RVA: 0x0007100B File Offset: 0x0006F20B
		// (set) Token: 0x06000C87 RID: 3207 RVA: 0x00071013 File Offset: 0x0006F213
		public LocalizedString ProgressBarLabel { get; private set; }

		// Token: 0x1700038E RID: 910
		// (get) Token: 0x06000C88 RID: 3208 RVA: 0x0007101C File Offset: 0x0006F21C
		// (set) Token: 0x06000C89 RID: 3209 RVA: 0x00071024 File Offset: 0x0006F224
		[TupleElementNames(new string[]
		{
			"from",
			"to"
		})]
		public List<ValueTuple<Identifier, Identifier>> AllowedConnectionTypes { [return: TupleElementNames(new string[]
		{
			"from",
			"to"
		})] get; [param: TupleElementNames(new string[]
		{
			"from",
			"to"
		})] private set; }

		// Token: 0x1700038F RID: 911
		// (get) Token: 0x06000C8A RID: 3210 RVA: 0x0007102D File Offset: 0x0006F22D
		// (set) Token: 0x06000C8B RID: 3211 RVA: 0x00071035 File Offset: 0x0006F235
		public Identifier RequiredLocationFaction { get; private set; }

		// Token: 0x17000390 RID: 912
		// (get) Token: 0x06000C8C RID: 3212 RVA: 0x0007103E File Offset: 0x0006F23E
		// (set) Token: 0x06000C8D RID: 3213 RVA: 0x00071046 File Offset: 0x0006F246
		public List<string> UnhideEntitySubCategories { get; private set; }

		// Token: 0x17000391 RID: 913
		// (get) Token: 0x06000C8E RID: 3214 RVA: 0x0007104F File Offset: 0x0006F24F
		// (set) Token: 0x06000C8F RID: 3215 RVA: 0x00071057 File Offset: 0x0006F257
		public ContentXElement ConfigElement { get; private set; }

		// Token: 0x17000392 RID: 914
		// (get) Token: 0x06000C90 RID: 3216 RVA: 0x00071060 File Offset: 0x0006F260
		public Identifier VariantOf { get; }

		// Token: 0x17000393 RID: 915
		// (get) Token: 0x06000C91 RID: 3217 RVA: 0x00071068 File Offset: 0x0006F268
		// (set) Token: 0x06000C92 RID: 3218 RVA: 0x00071070 File Offset: 0x0006F270
		public MissionPrefab ParentPrefab { get; set; }

		// Token: 0x06000C93 RID: 3219 RVA: 0x0007107C File Offset: 0x0006F27C
		public MissionPrefab(ContentXElement element, MissionsFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.originalElement = element;
			this.ConfigElement = element;
			this.VariantOf = element.VariantOf();
			if (!this.VariantOf.IsEmpty)
			{
				return;
			}
			this.ParseConfigElement(null);
		}

		// Token: 0x06000C94 RID: 3220 RVA: 0x0007110F File Offset: 0x0006F30F
		public void InheritFrom(MissionPrefab parent)
		{
			this.ConfigElement = this.originalElement.CreateVariantXML(parent.ConfigElement, null);
			this.ParseConfigElement(parent);
		}

		// Token: 0x06000C95 RID: 3221 RVA: 0x00071130 File Offset: 0x0006F330
		private void ParseConfigElement(MissionPrefab variantOf = null)
		{
			this.TextIdentifier = this.ConfigElement.GetAttributeIdentifier("textidentifier", this.Identifier);
			this.Tags = ImmutableHashSet.Create<Identifier>(new ReadOnlySpan<Identifier>(this.ConfigElement.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true).ToArray<Identifier>()));
			this.Name = this.<ParseConfigElement>g__GetText|249_0(this.ConfigElement.GetAttributeString("name", ""), "MissionName");
			this.Description = this.<ParseConfigElement>g__GetText|249_0(this.ConfigElement.GetAttributeString("description", ""), "MissionDescription");
			this.Reward = this.ConfigElement.GetAttributeInt("Reward", 1);
			this.ExperienceMultiplier = this.ConfigElement.GetAttributeFloat("ExperienceMultiplier", 1f);
			this.AllowRetry = this.ConfigElement.GetAttributeBool("AllowRetry", false);
			this.ShowSonarLabels = this.ConfigElement.GetAttributeBool("ShowSonarLabels", true);
			this.ShowInMenus = this.ConfigElement.GetAttributeBool("ShowInMenus", true);
			this.ShowStartMessage = this.ConfigElement.GetAttributeBool("ShowStartMessage", true);
			this.IsSideObjective = this.ConfigElement.GetAttributeBool("sideobjective", false);
			this.RequireWreck = this.ConfigElement.GetAttributeBool("RequireWreck", false);
			this.RequireThalamusWreck = this.ConfigElement.GetAttributeBool("RequireThalamusWreck", false);
			this.RequireRuin = this.ConfigElement.GetAttributeBool("RequireRuin", false);
			this.RequireBeaconStation = this.ConfigElement.GetAttributeBool("RequireBeaconStation", false);
			this.SpawnBeaconStationInMiddle = this.ConfigElement.GetAttributeBool("SpawnBeaconStationInMiddle", false);
			this.RequireWreck |= this.RequireThalamusWreck;
			this.LoadSubmarines = this.ConfigElement.GetAttributeBool("LoadSubmarines", true);
			this.BlockLocationTypeChanges = this.ConfigElement.GetAttributeBool("BlockLocationTypeChanges", false);
			this.RequiredLocationFaction = this.ConfigElement.GetAttributeIdentifier("RequiredLocationFaction", Identifier.Empty);
			this.Commonness = this.ConfigElement.GetAttributeInt("Commonness", 1);
			this.AllowOtherMissionsInLevel = this.ConfigElement.GetAttributeBool("AllowOtherMissionsInLevel", true);
			if (this.ConfigElement.GetAttribute("difficulty") != null)
			{
				int difficulty = this.ConfigElement.GetAttributeInt("Difficulty", 1);
				this.Difficulty = new int?(Math.Clamp(difficulty, 1, 4));
			}
			this.MinLevelDifficulty = this.ConfigElement.GetAttributeInt("MinLevelDifficulty", this.MinLevelDifficulty);
			this.MaxLevelDifficulty = this.ConfigElement.GetAttributeInt("MaxLevelDifficulty", this.MaxLevelDifficulty);
			this.MinLevelDifficulty = Math.Clamp(this.MinLevelDifficulty, 0, Math.Min(this.MaxLevelDifficulty, 100));
			this.MaxLevelDifficulty = Math.Clamp(this.MaxLevelDifficulty, Math.Max(this.MinLevelDifficulty, 0), 100);
			this.AllowOutpostNPCs = this.ConfigElement.GetAttributeBool("AllowOutpostNPCs", true);
			this.ForceOutpostGenerationParameters = this.ConfigElement.GetAttributeIdentifier("ForceOutpostGenerationParameters", Identifier.Empty);
			this.AllowOutpostSelectionFromTag = this.ConfigElement.GetAttributeIdentifier("AllowOutpostSelectionFromTag", Identifier.Empty);
			if (this.ConfigElement.GetAttribute("ForceRespawnMode") != null)
			{
				ContentXElement configElement = this.ConfigElement;
				string key = "ForceRespawnMode";
				RespawnMode respawnMode = RespawnMode.MidRound;
				this.ForceRespawnMode = new RespawnMode?(configElement.GetAttributeEnum<RespawnMode>(key, respawnMode));
			}
			this.ShowProgressBar = this.ConfigElement.GetAttributeBool("ShowProgressBar", false);
			this.ShowProgressInNumbers = this.ConfigElement.GetAttributeBool("ShowProgressInNumbers", false);
			this.MaxProgressState = this.ConfigElement.GetAttributeInt("MaxProgressState", 1);
			string progressBarLabel = this.ConfigElement.GetAttributeString("ProgressBarLabel", "");
			this.ProgressBarLabel = TextManager.Get(progressBarLabel).Fallback(progressBarLabel, true);
			string successMessageTag = this.ConfigElement.GetAttributeString("successmessage", "");
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler.AppendLiteral("MissionSuccess.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.TextIdentifier);
			this.SuccessMessage = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			if (!string.IsNullOrEmpty(successMessageTag))
			{
				this.SuccessMessage = this.SuccessMessage.Fallback(TextManager.Get(successMessageTag), true).Fallback(successMessageTag, true);
			}
			this.SuccessMessage = this.SuccessMessage.Fallback(TextManager.Get("missioncompleted"), true);
			string failureMessageTag = this.ConfigElement.GetAttributeString("failuremessage", "");
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(15, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("MissionFailure.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.TextIdentifier);
			this.FailureMessage = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			if (!string.IsNullOrEmpty(failureMessageTag))
			{
				this.FailureMessage = this.FailureMessage.Fallback(TextManager.Get(failureMessageTag), true).Fallback(failureMessageTag, true);
			}
			this.FailureMessage = this.FailureMessage.Fallback(TextManager.Get("missionfailed"), true);
			string sonarLabelTag = this.ConfigElement.GetAttributeString("sonarlabel", "");
			LocalizedString localizedString = TextManager.Get("MissionSonarLabel." + sonarLabelTag).Fallback(TextManager.Get(sonarLabelTag), true);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(18, 1);
			defaultInterpolatedStringHandler3.AppendLiteral("MissionSonarLabel.");
			defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(this.TextIdentifier);
			this.SonarLabel = localizedString.Fallback(TextManager.Get(defaultInterpolatedStringHandler3.ToStringAndClear()), true);
			if (!string.IsNullOrEmpty(sonarLabelTag))
			{
				this.SonarLabel = this.SonarLabel.Fallback(sonarLabelTag, true);
			}
			this.SonarIconIdentifier = this.ConfigElement.GetAttributeIdentifier("sonaricon", "");
			this.CampaignOnly = this.ConfigElement.GetAttributeBool("CampaignOnly", false);
			this.MultiplayerOnly = this.ConfigElement.GetAttributeBool("MultiplayerOnly", false);
			this.SingleplayerOnly = this.ConfigElement.GetAttributeBool("SingleplayerOnly", false);
			this.AchievementIdentifier = this.ConfigElement.GetAttributeIdentifier("achievementidentifier", "");
			this.UnhideEntitySubCategories = this.ConfigElement.GetAttributeStringArray("unhideentitysubcategories", Array.Empty<string>(), false).ToList<string>();
			List<LocalizedString> headers = new List<LocalizedString>();
			List<LocalizedString> messages = new List<LocalizedString>();
			this.AllowedConnectionTypes = new List<ValueTuple<Identifier, Identifier>>();
			for (int i = 0; i < 100; i++)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(14, 2);
				defaultInterpolatedStringHandler4.AppendLiteral("MissionHeader");
				defaultInterpolatedStringHandler4.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler4.AppendLiteral(".");
				defaultInterpolatedStringHandler4.AppendFormatted<Identifier>(this.TextIdentifier);
				LocalizedString header = TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear());
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(15, 2);
				defaultInterpolatedStringHandler5.AppendLiteral("MissionMessage");
				defaultInterpolatedStringHandler5.AppendFormatted<int>(i);
				defaultInterpolatedStringHandler5.AppendLiteral(".");
				defaultInterpolatedStringHandler5.AppendFormatted<Identifier>(this.TextIdentifier);
				LocalizedString message = TextManager.Get(defaultInterpolatedStringHandler5.ToStringAndClear());
				if (!message.IsNullOrEmpty())
				{
					headers.Add(header);
					messages.Add(message);
				}
			}
			List<MissionPrefab.ReputationReward> reputationRewards = new List<MissionPrefab.ReputationReward>();
			int messageIndex = 0;
			foreach (ContentXElement subElement in this.ConfigElement.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					switch (text.Length)
					{
					case 7:
					{
						if (!(text == "message"))
						{
							continue;
						}
						if (messageIndex >= headers.Count)
						{
							headers.Add(string.Empty);
							messages.Add(string.Empty);
						}
						List<LocalizedString> list = headers;
						int index = messageIndex;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler6 = new DefaultInterpolatedStringHandler(14, 2);
						defaultInterpolatedStringHandler6.AppendLiteral("MissionHeader");
						defaultInterpolatedStringHandler6.AppendFormatted<int>(messageIndex);
						defaultInterpolatedStringHandler6.AppendLiteral(".");
						defaultInterpolatedStringHandler6.AppendFormatted<Identifier>(this.TextIdentifier);
						list[index] = TextManager.Get(defaultInterpolatedStringHandler6.ToStringAndClear()).Fallback(TextManager.Get(subElement.GetAttributeString("header", "")), true).Fallback(subElement.GetAttributeString("header", ""), true);
						List<LocalizedString> list2 = messages;
						int index2 = messageIndex;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler7 = new DefaultInterpolatedStringHandler(15, 2);
						defaultInterpolatedStringHandler7.AppendLiteral("MissionMessage");
						defaultInterpolatedStringHandler7.AppendFormatted<int>(messageIndex);
						defaultInterpolatedStringHandler7.AppendLiteral(".");
						defaultInterpolatedStringHandler7.AppendFormatted<Identifier>(this.TextIdentifier);
						list2[index2] = TextManager.Get(defaultInterpolatedStringHandler7.ToStringAndClear()).Fallback(TextManager.Get(subElement.GetAttributeString("text", "")), true).Fallback(subElement.GetAttributeString("text", ""), true);
						messageIndex++;
						continue;
					}
					case 8:
					{
						if (!(text == "metadata"))
						{
							continue;
						}
						Identifier identifier = subElement.GetAttributeIdentifier("identifier", Identifier.Empty);
						string stringValue = subElement.GetAttributeString("value", string.Empty);
						if (!string.IsNullOrWhiteSpace(stringValue) && !identifier.IsEmpty)
						{
							object value = SetDataAction.ConvertXMLValue(stringValue);
							SetDataAction.OperationType operation = SetDataAction.OperationType.Set;
							string operatingString = subElement.GetAttributeString("operation", string.Empty);
							if (!string.IsNullOrWhiteSpace(operatingString))
							{
								operation = (SetDataAction.OperationType)Enum.Parse(typeof(SetDataAction.OperationType), operatingString);
							}
							this.DataRewards.Add(new ValueTuple<Identifier, object, SetDataAction.OperationType>(identifier, value, operation));
							continue;
						}
						continue;
					}
					case 9:
					case 11:
					case 13:
					case 15:
					case 17:
						continue;
					case 10:
						if (!(text == "reputation"))
						{
							continue;
						}
						goto IL_A23;
					case 12:
					{
						char c = text[0];
						if (c != 'l')
						{
							if (c != 't')
							{
								continue;
							}
							if (!(text == "triggerevent"))
							{
								continue;
							}
							this.TriggerEvents.Add(new MissionPrefab.TriggerEvent(subElement));
							continue;
						}
						else if (!(text == "locationtype"))
						{
							continue;
						}
						break;
					}
					case 14:
						if (!(text == "connectiontype"))
						{
							continue;
						}
						break;
					case 16:
						if (!(text == "reputationreward"))
						{
							continue;
						}
						goto IL_A23;
					case 18:
						if (!(text == "locationtypechange"))
						{
							continue;
						}
						this.LocationTypeChangeOnCompleted = new LocationTypeChange(subElement.GetAttributeIdentifier("from", ""), subElement, false, 1f);
						continue;
					default:
						continue;
					}
					if (subElement.GetAttribute("identifier") != null)
					{
						this.AllowedLocationTypes.Add(subElement.GetAttributeIdentifier("identifier", ""));
						continue;
					}
					this.AllowedConnectionTypes.Add(new ValueTuple<Identifier, Identifier>(subElement.GetAttributeIdentifier("from", ""), subElement.GetAttributeIdentifier("to", "")));
					continue;
					IL_A23:
					reputationRewards.Add(new MissionPrefab.ReputationReward(subElement));
				}
			}
			this.Headers = ImmutableCollectionsMarshal.AsImmutableArray<LocalizedString>(headers.ToArray());
			this.Messages = ImmutableCollectionsMarshal.AsImmutableArray<LocalizedString>(messages.ToArray());
			this.ReputationRewards = ImmutableList.Create<MissionPrefab.ReputationReward>(new ReadOnlySpan<MissionPrefab.ReputationReward>(reputationRewards.ToArray()));
			this.MissionClass = this.FindMissionClass(this.ConfigElement);
			this.Type = this.ConfigElement.GetAttributeIdentifier("Type", Identifier.Empty);
			if (!this.LoadSubmarines && this.MissionClass != typeof(CombatMission))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler8 = new DefaultInterpolatedStringHandler(165, 1);
				defaultInterpolatedStringHandler8.AppendLiteral("Potential error in mission ");
				defaultInterpolatedStringHandler8.AppendFormatted<Identifier>(this.Identifier);
				defaultInterpolatedStringHandler8.AppendLiteral(": Disabling submarines is only intended for combat missions taking place in an outpost, and may lead to issues in other types of missions.");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler8.ToStringAndClear(), this.ConfigElement.ContentPackage);
			}
			this.constructor = this.FindMissionConstructor(this.ConfigElement, this.MissionClass);
			if (this.constructor == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler9 = new DefaultInterpolatedStringHandler(53, 1);
				defaultInterpolatedStringHandler9.AppendLiteral("Failed to find a constructor for the mission type \"");
				defaultInterpolatedStringHandler9.AppendFormatted<Identifier>(this.Type);
				defaultInterpolatedStringHandler9.AppendLiteral("\"!");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler9.ToStringAndClear(), null, this.ConfigElement.ContentPackage, false, false);
			}
			this.ParseConfigElementClient(this.ConfigElement, variantOf);
		}

		// Token: 0x06000C96 RID: 3222 RVA: 0x00071DA4 File Offset: 0x0006FFA4
		private Type FindMissionClass(ContentXElement element)
		{
			Type type = MissionPrefab.<FindMissionClass>g__TryGetClass|250_0(element.NameAsIdentifier().RemoveFromEnd("Mission"));
			if (type == null)
			{
				Identifier typeNameLegacy = element.GetAttributeIdentifier("type", Identifier.Empty).ToIdentifier<Identifier>();
				if (typeNameLegacy == "OutpostDestroy" || typeNameLegacy == "OutpostRescue")
				{
					typeNameLegacy = "AbandonedOutpost".ToIdentifier();
				}
				else if (typeNameLegacy == "clearalienruins")
				{
					typeNameLegacy = "EliminateTargets".ToIdentifier();
				}
				type = (MissionPrefab.<FindMissionClass>g__TryGetClass|250_0(typeNameLegacy) ?? MissionPrefab.<FindMissionClass>g__TryGetClass|250_0(typeNameLegacy.AppendIfMissing("Mission")));
				if (type == null)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to find the mission type \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(typeNameLegacy);
					defaultInterpolatedStringHandler.AppendLiteral("\" for the mission ");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
					return null;
				}
			}
			return type;
		}

		// Token: 0x06000C97 RID: 3223 RVA: 0x00071EB0 File Offset: 0x000700B0
		private ConstructorInfo FindMissionConstructor(ContentXElement element, Type missionClass)
		{
			if (missionClass == null)
			{
				return null;
			}
			if (missionClass != typeof(Mission) && !missionClass.IsSubclassOf(typeof(Mission)))
			{
				return null;
			}
			ConstructorInfo constructor = missionClass.GetConstructor(new Type[]
			{
				typeof(MissionPrefab),
				typeof(Location[]),
				typeof(Submarine)
			});
			if (constructor == null)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find the constructor of the mission type \"");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(missionClass);
				defaultInterpolatedStringHandler.AppendLiteral("\" for the mission ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, element.ContentPackage, false, false);
				return null;
			}
			return constructor;
		}

		// Token: 0x06000C98 RID: 3224 RVA: 0x00071F7C File Offset: 0x0007017C
		public bool IsAllowed(Location from, Location to)
		{
			if (from == to)
			{
				if (!this.RequiredLocationFaction.IsEmpty)
				{
					Faction faction = from.Faction;
					Identifier? identifier;
					Identifier? identifier2;
					if (faction == null)
					{
						identifier = null;
						identifier2 = identifier;
					}
					else
					{
						identifier2 = new Identifier?(faction.Prefab.Identifier);
					}
					identifier = identifier2;
					Identifier? identifier3 = new Identifier?(this.RequiredLocationFaction);
					if (identifier != identifier3)
					{
						return false;
					}
				}
				return this.AllowedLocationTypes.Any((Identifier lt) => lt == "any") || this.AllowedLocationTypes.Any((Identifier lt) => lt == Barotrauma.Tags.AnyOutpost && from.HasOutpost() && from.Type.IsAnyOutpost) || this.AllowedLocationTypes.Any((Identifier lt) => lt == from.Type.Identifier);
			}
			foreach (ValueTuple<Identifier, Identifier> valueTuple in this.AllowedConnectionTypes)
			{
				Identifier fromType = valueTuple.Item1;
				Identifier toType = valueTuple.Item2;
				if ((fromType == "any" || fromType == from.Type.Identifier || (fromType == Barotrauma.Tags.AnyOutpost && from.HasOutpost() && from.Type.IsAnyOutpost && from.Type.Identifier != "abandoned")) && (toType == "any" || toType == to.Type.Identifier || (toType == Barotrauma.Tags.AnyOutpost && to.HasOutpost() && to.Type.IsAnyOutpost && to.Type.Identifier != "abandoned")))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x06000C99 RID: 3225 RVA: 0x00072190 File Offset: 0x00070390
		public bool IsAllowedDifficulty(float difficulty)
		{
			return difficulty >= (float)this.MinLevelDifficulty && difficulty <= (float)this.MaxLevelDifficulty;
		}

		// Token: 0x06000C9A RID: 3226 RVA: 0x000721AB File Offset: 0x000703AB
		public Mission Instantiate(Location[] locations, Submarine sub)
		{
			ConstructorInfo constructorInfo = this.constructor;
			return ((constructorInfo != null) ? constructorInfo.Invoke(new object[]
			{
				this,
				locations,
				sub
			}) : null) as Mission;
		}

		// Token: 0x06000C9B RID: 3227 RVA: 0x000721D6 File Offset: 0x000703D6
		private void DisposeProjectSpecific()
		{
			Sprite icon = this.Icon;
			if (icon == null)
			{
				return;
			}
			icon.Remove();
		}

		// Token: 0x06000C9C RID: 3228 RVA: 0x000721E8 File Offset: 0x000703E8
		public override void Dispose()
		{
			this.DisposeProjectSpecific();
		}

		// Token: 0x06000C9D RID: 3229 RVA: 0x000721F0 File Offset: 0x000703F0
		public static IEnumerable<Identifier> GetAllMultiplayerSelectableMissionTypes()
		{
			List<Identifier> missionTypes = new List<Identifier>();
			foreach (MissionPrefab missionPrefab in MissionPrefab.Prefabs)
			{
				if ((float)missionPrefab.Commonness > 0f && !missionPrefab.CampaignOnly && !missionPrefab.SingleplayerOnly && !MissionPrefab.HiddenMissionTypes.Contains(missionPrefab.Type) && !missionTypes.Contains(missionPrefab.Type))
				{
					missionTypes.Add(missionPrefab.Type);
				}
			}
			return from t in missionTypes
			orderby t.Value
			select t;
		}

		// Token: 0x06000C9F RID: 3231 RVA: 0x00072490 File Offset: 0x00070690
		[CompilerGenerated]
		private LocalizedString <ParseConfigElement>g__GetText|249_0(string textTag, string textTagPrefix)
		{
			if (string.IsNullOrEmpty(textTag))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
				defaultInterpolatedStringHandler.AppendFormatted(textTagPrefix);
				defaultInterpolatedStringHandler.AppendLiteral(".");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.TextIdentifier);
				return TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			LocalizedString localizedString = TextManager.Get(textTag);
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(1, 2);
			defaultInterpolatedStringHandler2.AppendFormatted(textTagPrefix);
			defaultInterpolatedStringHandler2.AppendLiteral(".");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.TextIdentifier);
			return localizedString.Fallback(TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()), true).Fallback(textTag, true);
		}

		// Token: 0x06000CA0 RID: 3232 RVA: 0x0007252C File Offset: 0x0007072C
		[CompilerGenerated]
		internal static Type <FindMissionClass>g__TryGetClass|250_0(Identifier typeName)
		{
			Type coOpMissionClass;
			if (MissionPrefab.CoOpMissionClasses.TryGetValue(typeName, out coOpMissionClass))
			{
				return coOpMissionClass;
			}
			Type pvpMissionClass;
			if (MissionPrefab.PvPMissionClasses.TryGetValue(typeName, out pvpMissionClass))
			{
				return pvpMissionClass;
			}
			return null;
		}

		// Token: 0x0400063A RID: 1594
		private ImmutableArray<Sprite> portraits = ImmutableArray<Sprite>.Empty;

		// Token: 0x0400063F RID: 1599
		private Sprite hudIcon;

		// Token: 0x04000640 RID: 1600
		private Color? hudIconColor;

		// Token: 0x04000642 RID: 1602
		private ImmutableDictionary<int, Identifier> overrideMusicOnState;

		// Token: 0x04000643 RID: 1603
		public static readonly PrefabCollection<MissionPrefab> Prefabs = new PrefabCollection<MissionPrefab>();

		// Token: 0x04000644 RID: 1604
		public static readonly Dictionary<Identifier, Type> CoOpMissionClasses = new Dictionary<Identifier, Type>
		{
			{
				"Salvage".ToIdentifier(),
				typeof(SalvageMission)
			},
			{
				"Monster".ToIdentifier(),
				typeof(MonsterMission)
			},
			{
				"Cargo".ToIdentifier(),
				typeof(CargoMission)
			},
			{
				"Beacon".ToIdentifier(),
				typeof(BeaconMission)
			},
			{
				"Nest".ToIdentifier(),
				typeof(NestMission)
			},
			{
				"Mineral".ToIdentifier(),
				typeof(MineralMission)
			},
			{
				"AbandonedOutpost".ToIdentifier(),
				typeof(AbandonedOutpostMission)
			},
			{
				"Escort".ToIdentifier(),
				typeof(EscortMission)
			},
			{
				"Pirate".ToIdentifier(),
				typeof(PirateMission)
			},
			{
				"GoTo".ToIdentifier(),
				typeof(GoToMission)
			},
			{
				"ScanAlienRuins".ToIdentifier(),
				typeof(ScanMission)
			},
			{
				"EliminateTargets".ToIdentifier(),
				typeof(EliminateTargetsMission)
			},
			{
				"End".ToIdentifier(),
				typeof(EndMission)
			},
			{
				"Custom".ToIdentifier(),
				typeof(CustomMission)
			}
		};

		// Token: 0x04000645 RID: 1605
		public static readonly Dictionary<Identifier, Type> PvPMissionClasses = new Dictionary<Identifier, Type>
		{
			{
				"Combat".ToIdentifier(),
				typeof(CombatMission)
			}
		};

		// Token: 0x04000646 RID: 1606
		public static readonly HashSet<Identifier> HiddenMissionTypes = new HashSet<Identifier>
		{
			"GoTo".ToIdentifier(),
			"End".ToIdentifier()
		};

		// Token: 0x04000647 RID: 1607
		private ConstructorInfo constructor;

		// Token: 0x04000657 RID: 1623
		[TupleElementNames(new string[]
		{
			"Identifier",
			"Value",
			"OperationType"
		})]
		public readonly List<ValueTuple<Identifier, object, SetDataAction.OperationType>> DataRewards = new List<ValueTuple<Identifier, object, SetDataAction.OperationType>>();

		// Token: 0x0400065A RID: 1626
		public const int MinDifficulty = 1;

		// Token: 0x0400065B RID: 1627
		public const int MaxDifficulty = 4;

		// Token: 0x04000678 RID: 1656
		public readonly List<Identifier> AllowedLocationTypes = new List<Identifier>();

		// Token: 0x0400067B RID: 1659
		public readonly List<MissionPrefab.TriggerEvent> TriggerEvents = new List<MissionPrefab.TriggerEvent>();

		// Token: 0x0400067C RID: 1660
		public LocationTypeChange LocationTypeChangeOnCompleted;

		// Token: 0x0400067D RID: 1661
		private readonly ContentXElement originalElement;

		// Token: 0x020007F1 RID: 2033
		public class ReputationReward
		{
			// Token: 0x06006C38 RID: 27704 RVA: 0x0035EA90 File Offset: 0x0035CC90
			public ReputationReward(XElement element)
			{
				this.FactionIdentifier = element.GetAttributeIdentifier("identifier", Identifier.Empty);
				this.Amount = element.GetAttributeFloat("Amount", 0f);
				this.AmountForOpposingFaction = element.GetAttributeFloat("AmountForOpposingFaction", 0f);
			}

			// Token: 0x04003C2B RID: 15403
			public readonly Identifier FactionIdentifier;

			// Token: 0x04003C2C RID: 15404
			public readonly float Amount;

			// Token: 0x04003C2D RID: 15405
			public readonly float AmountForOpposingFaction;
		}

		// Token: 0x020007F2 RID: 2034
		public class TriggerEvent
		{
			// Token: 0x17001A05 RID: 6661
			// (get) Token: 0x06006C39 RID: 27705 RVA: 0x0035EAE5 File Offset: 0x0035CCE5
			// (set) Token: 0x06006C3A RID: 27706 RVA: 0x0035EAED File Offset: 0x0035CCED
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			public Identifier EventIdentifier { get; private set; }

			// Token: 0x17001A06 RID: 6662
			// (get) Token: 0x06006C3B RID: 27707 RVA: 0x0035EAF6 File Offset: 0x0035CCF6
			// (set) Token: 0x06006C3C RID: 27708 RVA: 0x0035EAFE File Offset: 0x0035CCFE
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			public Identifier EventTag { get; private set; }

			// Token: 0x17001A07 RID: 6663
			// (get) Token: 0x06006C3D RID: 27709 RVA: 0x0035EB07 File Offset: 0x0035CD07
			// (set) Token: 0x06006C3E RID: 27710 RVA: 0x0035EB0F File Offset: 0x0035CD0F
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			public int State { get; private set; }

			// Token: 0x17001A08 RID: 6664
			// (get) Token: 0x06006C3F RID: 27711 RVA: 0x0035EB18 File Offset: 0x0035CD18
			// (set) Token: 0x06006C40 RID: 27712 RVA: 0x0035EB20 File Offset: 0x0035CD20
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			public float Delay { get; private set; }

			// Token: 0x17001A09 RID: 6665
			// (get) Token: 0x06006C41 RID: 27713 RVA: 0x0035EB29 File Offset: 0x0035CD29
			// (set) Token: 0x06006C42 RID: 27714 RVA: 0x0035EB31 File Offset: 0x0035CD31
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			public bool CampaignOnly { get; private set; }

			// Token: 0x06006C43 RID: 27715 RVA: 0x0035EB3A File Offset: 0x0035CD3A
			public TriggerEvent(XElement element)
			{
				SerializableProperty.DeserializeProperties(this, element);
			}
		}
	}
}
