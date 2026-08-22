using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000EE RID: 238
	internal class CharacterParams : EditableParams
	{
		// Token: 0x17000753 RID: 1875
		// (get) Token: 0x060018A8 RID: 6312 RVA: 0x000C4D9D File Offset: 0x000C2F9D
		// (set) Token: 0x060018A9 RID: 6313 RVA: 0x000C4DA5 File Offset: 0x000C2FA5
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Identifier SpeciesName { get; private set; }

		// Token: 0x17000754 RID: 1876
		// (get) Token: 0x060018AA RID: 6314 RVA: 0x000C4DAE File Offset: 0x000C2FAE
		// (set) Token: 0x060018AB RID: 6315 RVA: 0x000C4DC0 File Offset: 0x000C2FC0
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string Tags
		{
			get
			{
				return this.tags.ConvertToString(",");
			}
			set
			{
				this.tags = value.ToIdentifiers(",").ToHashSet<Identifier>();
			}
		}

		// Token: 0x060018AC RID: 6316 RVA: 0x000C4DD8 File Offset: 0x000C2FD8
		public bool HasTag(Identifier tag)
		{
			return this.tags.Contains(tag);
		}

		// Token: 0x17000755 RID: 1877
		// (get) Token: 0x060018AD RID: 6317 RVA: 0x000C4DE6 File Offset: 0x000C2FE6
		// (set) Token: 0x060018AE RID: 6318 RVA: 0x000C4DEE File Offset: 0x000C2FEE
		[Serialize("", IsPropertySaveable.Yes, "References to another species. Define only if the creature is a variant that needs to use a pre-existing translation.", "", false)]
		[Editable]
		public Identifier SpeciesTranslationOverride { get; private set; }

		// Token: 0x17000756 RID: 1878
		// (get) Token: 0x060018AF RID: 6319 RVA: 0x000C4DF7 File Offset: 0x000C2FF7
		// (set) Token: 0x060018B0 RID: 6320 RVA: 0x000C4DFF File Offset: 0x000C2FFF
		[Serialize("", IsPropertySaveable.Yes, "Overrides the name of the character, shown to the player. If the display name is not defined, the game first tries to find the translated name. If that is not found, the species name will be used.", "", false)]
		[Editable]
		public string DisplayName { get; private set; }

		// Token: 0x17000757 RID: 1879
		// (get) Token: 0x060018B1 RID: 6321 RVA: 0x000C4E08 File Offset: 0x000C3008
		// (set) Token: 0x060018B2 RID: 6322 RVA: 0x000C4E10 File Offset: 0x000C3010
		[Serialize("", IsPropertySaveable.Yes, "If defined, different species of the same group consider each other friendly and do not attack each other.", "", false)]
		[Editable]
		public Identifier Group { get; private set; }

		// Token: 0x17000758 RID: 1880
		// (get) Token: 0x060018B3 RID: 6323 RVA: 0x000C4E19 File Offset: 0x000C3019
		// (set) Token: 0x060018B4 RID: 6324 RVA: 0x000C4E21 File Offset: 0x000C3021
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the character is a humanoid and has different animation constraints relative to non-humanoid characters.", "", false)]
		[Editable(ReadOnly = true)]
		public bool Humanoid { get; private set; }

		// Token: 0x17000759 RID: 1881
		// (get) Token: 0x060018B5 RID: 6325 RVA: 0x000C4E2A File Offset: 0x000C302A
		// (set) Token: 0x060018B6 RID: 6326 RVA: 0x000C4E32 File Offset: 0x000C3032
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, jobs can be assigned to characters of this species. Should be true for the player characters.", "", false)]
		[Editable(ReadOnly = true)]
		public bool HasInfo { get; private set; }

		// Token: 0x1700075A RID: 1882
		// (get) Token: 0x060018B7 RID: 6327 RVA: 0x000C4E3B File Offset: 0x000C303B
		// (set) Token: 0x060018B8 RID: 6328 RVA: 0x000C4E43 File Offset: 0x000C3043
		[Serialize(false, IsPropertySaveable.Yes, "Can the creature interact with items?", "", false)]
		[Editable]
		public bool CanInteract { get; private set; }

		// Token: 0x1700075B RID: 1883
		// (get) Token: 0x060018B9 RID: 6329 RVA: 0x000C4E4C File Offset: 0x000C304C
		// (set) Token: 0x060018BA RID: 6330 RVA: 0x000C4E54 File Offset: 0x000C3054
		[Serialize(true, IsPropertySaveable.Yes, "Can the creature use ladders? Doesn't have an effect, if CanInteract is false.", "", false)]
		[Editable]
		public bool CanClimb { get; private set; }

		// Token: 0x1700075C RID: 1884
		// (get) Token: 0x060018BB RID: 6331 RVA: 0x000C4E5D File Offset: 0x000C305D
		// (set) Token: 0x060018BC RID: 6332 RVA: 0x000C4E65 File Offset: 0x000C3065
		[Serialize(false, IsPropertySaveable.Yes, "If set true, this character only uses the climbing parameters defined in the walk parameters (not run).", "", false)]
		[Editable]
		public bool ForceSlowClimbing { get; private set; }

		// Token: 0x1700075D RID: 1885
		// (get) Token: 0x060018BD RID: 6333 RVA: 0x000C4E6E File Offset: 0x000C306E
		// (set) Token: 0x060018BE RID: 6334 RVA: 0x000C4E76 File Offset: 0x000C3076
		[Serialize(false, IsPropertySaveable.Yes, "Should this character be treated as a husk?", "", false)]
		[Editable]
		public bool Husk { get; private set; }

		// Token: 0x1700075E RID: 1886
		// (get) Token: 0x060018BF RID: 6335 RVA: 0x000C4E7F File Offset: 0x000C307F
		// (set) Token: 0x060018C0 RID: 6336 RVA: 0x000C4E87 File Offset: 0x000C3087
		[Serialize("", IsPropertySaveable.Yes, "If this character can turn into a husk, which character it turns to? If not defined, uses the default pattern (e.g. Crawler -> Crawlerhusk, Human -> Humanhusk).", "", false)]
		[Editable]
		public Identifier HuskedSpecies { get; private set; }

		// Token: 0x1700075F RID: 1887
		// (get) Token: 0x060018C1 RID: 6337 RVA: 0x000C4E90 File Offset: 0x000C3090
		// (set) Token: 0x060018C2 RID: 6338 RVA: 0x000C4E98 File Offset: 0x000C3098
		[Serialize("", IsPropertySaveable.Yes, "If this character is a husk, from what species it can be turned into? If not defined, uses the default pattern (e.g. Crawlerhusk -> Crawler, Humanhusk -> Human).", "", false)]
		[Editable]
		public Identifier NonHuskedSpecies { get; private set; }

		// Token: 0x17000760 RID: 1888
		// (get) Token: 0x060018C3 RID: 6339 RVA: 0x000C4EA1 File Offset: 0x000C30A1
		// (set) Token: 0x060018C4 RID: 6340 RVA: 0x000C4EA9 File Offset: 0x000C30A9
		[Serialize(false, IsPropertySaveable.Yes, "Should this character use a special husk appendage, attached to the ragdoll, when it turns into a husk?", "", false)]
		[Editable]
		public bool UseHuskAppendage { get; private set; }

		// Token: 0x17000761 RID: 1889
		// (get) Token: 0x060018C5 RID: 6341 RVA: 0x000C4EB2 File Offset: 0x000C30B2
		// (set) Token: 0x060018C6 RID: 6342 RVA: 0x000C4EBA File Offset: 0x000C30BA
		[Serialize(false, IsPropertySaveable.Yes, "Does this character need oxygen to survive? Enabling this also makes the character vulnerable to high pressure when swimming outside of the submarine.", "", false)]
		[Editable]
		public bool NeedsAir { get; set; }

		// Token: 0x17000762 RID: 1890
		// (get) Token: 0x060018C7 RID: 6343 RVA: 0x000C4EC3 File Offset: 0x000C30C3
		// (set) Token: 0x060018C8 RID: 6344 RVA: 0x000C4ECB File Offset: 0x000C30CB
		[Serialize(false, IsPropertySaveable.Yes, "Can the creature live without water or does it die on dry land?", "", false)]
		[Editable]
		public bool NeedsWater { get; set; }

		// Token: 0x17000763 RID: 1891
		// (get) Token: 0x060018C9 RID: 6345 RVA: 0x000C4ED4 File Offset: 0x000C30D4
		// (set) Token: 0x060018CA RID: 6346 RVA: 0x000C4EDC File Offset: 0x000C30DC
		[Serialize(false, IsPropertySaveable.Yes, "Note: non-humans with a human AI aren't fully supported. Enabling this on a non-human character may lead to issues.", "", false)]
		public bool UseHumanAI { get; set; }

		// Token: 0x17000764 RID: 1892
		// (get) Token: 0x060018CB RID: 6347 RVA: 0x000C4EE5 File Offset: 0x000C30E5
		// (set) Token: 0x060018CC RID: 6348 RVA: 0x000C4EED File Offset: 0x000C30ED
		[Serialize(false, IsPropertySaveable.Yes, "Is this creature an artificial creature, like robot or machine that shouldn't be affected by afflictions that affect only organic creatures? Overrides DoesBleed.", "", false)]
		[Editable]
		public bool IsMachine { get; set; }

		// Token: 0x17000765 RID: 1893
		// (get) Token: 0x060018CD RID: 6349 RVA: 0x000C4EF6 File Offset: 0x000C30F6
		// (set) Token: 0x060018CE RID: 6350 RVA: 0x000C4EFE File Offset: 0x000C30FE
		[Serialize(false, IsPropertySaveable.No, "Is the character able to send messages in the chat?", "", false)]
		[Editable]
		public bool CanSpeak { get; set; }

		// Token: 0x17000766 RID: 1894
		// (get) Token: 0x060018CF RID: 6351 RVA: 0x000C4F07 File Offset: 0x000C3107
		// (set) Token: 0x060018D0 RID: 6352 RVA: 0x000C4F0F File Offset: 0x000C310F
		[Serialize(true, IsPropertySaveable.Yes, "Is there a health bar shown above the character when it takes damage? Defaults to true.", "", false)]
		[Editable]
		public bool ShowHealthBar { get; private set; }

		// Token: 0x17000767 RID: 1895
		// (get) Token: 0x060018D1 RID: 6353 RVA: 0x000C4F18 File Offset: 0x000C3118
		// (set) Token: 0x060018D2 RID: 6354 RVA: 0x000C4F20 File Offset: 0x000C3120
		[Serialize(false, IsPropertySaveable.Yes, "Is this character's health shown at the top of the player's screen when they are in an active encounter?", "", false)]
		[Editable]
		public bool UseBossHealthBar { get; private set; }

		// Token: 0x17000768 RID: 1896
		// (get) Token: 0x060018D3 RID: 6355 RVA: 0x000C4F29 File Offset: 0x000C3129
		// (set) Token: 0x060018D4 RID: 6356 RVA: 0x000C4F31 File Offset: 0x000C3131
		[Serialize(100f, IsPropertySaveable.Yes, "How much noise the character makes when moving?", "", false)]
		[Editable(0f, 100000f, 1)]
		public float Noise { get; set; }

		// Token: 0x17000769 RID: 1897
		// (get) Token: 0x060018D5 RID: 6357 RVA: 0x000C4F3A File Offset: 0x000C313A
		// (set) Token: 0x060018D6 RID: 6358 RVA: 0x000C4F42 File Offset: 0x000C3142
		[Serialize(100f, IsPropertySaveable.Yes, "How visible the character is?", "", false)]
		[Editable(0f, 100000f, 1)]
		public float Visibility { get; set; }

		// Token: 0x1700076A RID: 1898
		// (get) Token: 0x060018D7 RID: 6359 RVA: 0x000C4F4B File Offset: 0x000C314B
		// (set) Token: 0x060018D8 RID: 6360 RVA: 0x000C4F53 File Offset: 0x000C3153
		[Serialize("blood", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string BloodDecal { get; private set; }

		// Token: 0x1700076B RID: 1899
		// (get) Token: 0x060018D9 RID: 6361 RVA: 0x000C4F5C File Offset: 0x000C315C
		// (set) Token: 0x060018DA RID: 6362 RVA: 0x000C4F64 File Offset: 0x000C3164
		[Serialize("blooddrop", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string BleedParticleAir { get; private set; }

		// Token: 0x1700076C RID: 1900
		// (get) Token: 0x060018DB RID: 6363 RVA: 0x000C4F6D File Offset: 0x000C316D
		// (set) Token: 0x060018DC RID: 6364 RVA: 0x000C4F75 File Offset: 0x000C3175
		[Serialize("waterblood", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string BleedParticleWater { get; private set; }

		// Token: 0x1700076D RID: 1901
		// (get) Token: 0x060018DD RID: 6365 RVA: 0x000C4F7E File Offset: 0x000C317E
		// (set) Token: 0x060018DE RID: 6366 RVA: 0x000C4F86 File Offset: 0x000C3186
		[Serialize(1f, IsPropertySaveable.Yes, "A multiplier to increase or decrease the number of bleeding particles to create.", "", false)]
		[Editable]
		public float BleedParticleMultiplier { get; private set; }

		// Token: 0x1700076E RID: 1902
		// (get) Token: 0x060018DF RID: 6367 RVA: 0x000C4F8F File Offset: 0x000C318F
		// (set) Token: 0x060018E0 RID: 6368 RVA: 0x000C4F97 File Offset: 0x000C3197
		[Serialize(true, IsPropertySaveable.Yes, "Can the creature eat bodies? Used by player controlled creatures to allow them to eat. Currently applicable only to non-humanoids. To allow an AI controller to eat, just add an ai target with the state \"eat\"", "", false)]
		[Editable]
		public bool CanEat { get; set; }

		// Token: 0x1700076F RID: 1903
		// (get) Token: 0x060018E1 RID: 6369 RVA: 0x000C4FA0 File Offset: 0x000C31A0
		// (set) Token: 0x060018E2 RID: 6370 RVA: 0x000C4FA8 File Offset: 0x000C31A8
		[Serialize(10f, IsPropertySaveable.Yes, "How effectively/easily the character eats other characters. Affects the forces, the amount of particles, and the time required before the target is eaten away", "", false)]
		[Editable(MinValueFloat = 1f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float EatingSpeed { get; set; }

		// Token: 0x17000770 RID: 1904
		// (get) Token: 0x060018E3 RID: 6371 RVA: 0x000C4FB1 File Offset: 0x000C31B1
		// (set) Token: 0x060018E4 RID: 6372 RVA: 0x000C4FB9 File Offset: 0x000C31B9
		[Serialize(true, IsPropertySaveable.Yes, "Should the character AI use waypoints defined in the level to find a path to its targets?", "", false)]
		[Editable]
		public bool UsePathFinding { get; set; }

		// Token: 0x17000771 RID: 1905
		// (get) Token: 0x060018E5 RID: 6373 RVA: 0x000C4FC2 File Offset: 0x000C31C2
		// (set) Token: 0x060018E6 RID: 6374 RVA: 0x000C4FCA File Offset: 0x000C31CA
		[Serialize(1f, IsPropertySaveable.Yes, "Decreases the intensive path finding call frequency. Set to a lower value for insignificant creatures to improve performance.", "", false)]
		[Editable(0f, 1f, 1)]
		public float PathFinderPriority { get; set; }

		// Token: 0x17000772 RID: 1906
		// (get) Token: 0x060018E7 RID: 6375 RVA: 0x000C4FD3 File Offset: 0x000C31D3
		// (set) Token: 0x060018E8 RID: 6376 RVA: 0x000C4FDB File Offset: 0x000C31DB
		[Serialize(false, IsPropertySaveable.Yes, "Should the character be hidden in the sonar?", "", false)]
		[Editable]
		public bool HideInSonar { get; set; }

		// Token: 0x17000773 RID: 1907
		// (get) Token: 0x060018E9 RID: 6377 RVA: 0x000C4FE4 File Offset: 0x000C31E4
		// (set) Token: 0x060018EA RID: 6378 RVA: 0x000C4FEC File Offset: 0x000C31EC
		[Serialize(false, IsPropertySaveable.Yes, "Should the character be hidden when using thermal goggles?", "", false)]
		[Editable]
		public bool HideInThermalGoggles { get; set; }

		// Token: 0x17000774 RID: 1908
		// (get) Token: 0x060018EB RID: 6379 RVA: 0x000C4FF5 File Offset: 0x000C31F5
		// (set) Token: 0x060018EC RID: 6380 RVA: 0x000C4FFD File Offset: 0x000C31FD
		[Serialize(0f, IsPropertySaveable.Yes, "If set to a value greater than zero, this character creates disrupting noise on the sonar when within range.", "", false)]
		[Editable]
		public float SonarDisruption { get; set; }

		// Token: 0x17000775 RID: 1909
		// (get) Token: 0x060018ED RID: 6381 RVA: 0x000C5006 File Offset: 0x000C3206
		// (set) Token: 0x060018EE RID: 6382 RVA: 0x000C500E File Offset: 0x000C320E
		[Serialize(0f, IsPropertySaveable.Yes, "Range at which \"long distance\" blips for this character will appear on the sonar (used on some of the Abyss monsters).", "", false)]
		[Editable]
		public float DistantSonarRange { get; set; }

		// Token: 0x17000776 RID: 1910
		// (get) Token: 0x060018EF RID: 6383 RVA: 0x000C5017 File Offset: 0x000C3217
		// (set) Token: 0x060018F0 RID: 6384 RVA: 0x000C501F File Offset: 0x000C321F
		[Serialize(25000f, IsPropertySaveable.Yes, "If the character is farther than this (in pixels) from the sub and the players, it will be disabled. The halved value is used for triggering simple physics where the ragdoll is disabled and only the main collider is updated.", "", false)]
		[Editable(MinValueFloat = 10000f, MaxValueFloat = 100000f)]
		public float DisableDistance { get; set; }

		// Token: 0x17000777 RID: 1911
		// (get) Token: 0x060018F1 RID: 6385 RVA: 0x000C5028 File Offset: 0x000C3228
		// (set) Token: 0x060018F2 RID: 6386 RVA: 0x000C5030 File Offset: 0x000C3230
		[Serialize(10f, IsPropertySaveable.Yes, "How frequent the recurring idle and attack sounds are?", "", false)]
		[Editable(MinValueFloat = 1f, MaxValueFloat = 100f)]
		public float SoundInterval { get; set; }

		// Token: 0x17000778 RID: 1912
		// (get) Token: 0x060018F3 RID: 6387 RVA: 0x000C5039 File Offset: 0x000C3239
		// (set) Token: 0x060018F4 RID: 6388 RVA: 0x000C5041 File Offset: 0x000C3241
		[Serialize(false, IsPropertySaveable.Yes, "Should the character be drawn on top of characters that do not have this set? This currently has no effect if the character has no deformable sprites.", "", false)]
		[Editable]
		public bool DrawLast { get; set; }

		// Token: 0x17000779 RID: 1913
		// (get) Token: 0x060018F5 RID: 6389 RVA: 0x000C504A File Offset: 0x000C324A
		// (set) Token: 0x060018F6 RID: 6390 RVA: 0x000C5052 File Offset: 0x000C3252
		[Serialize(1f, IsPropertySaveable.Yes, "Tells the bots how much they should prefer targeting this character with submarine weapons. Defaults to 1. Set 0 to tell the bots not to target this character at all. Distance to the target affects the decision making.", "", false)]
		[Editable]
		public float AITurretPriority { get; set; }

		// Token: 0x1700077A RID: 1914
		// (get) Token: 0x060018F7 RID: 6391 RVA: 0x000C505B File Offset: 0x000C325B
		// (set) Token: 0x060018F8 RID: 6392 RVA: 0x000C5063 File Offset: 0x000C3263
		[Serialize(1f, IsPropertySaveable.Yes, "Tells the bots how much they should prefer targeting this character with submarine weapons tagged as \"slowturret\", like railguns. The tag is arbitrary and can be added to any turrets, just like the priority. Defaults to 1. Not used if AITurretPriority is 0. Distance to the target affects the decision making.", "", false)]
		[Editable]
		public float AISlowTurretPriority { get; set; }

		// Token: 0x1700077B RID: 1915
		// (get) Token: 0x060018F9 RID: 6393 RVA: 0x000C506C File Offset: 0x000C326C
		// (set) Token: 0x060018FA RID: 6394 RVA: 0x000C5074 File Offset: 0x000C3274
		[Serialize("", IsPropertySaveable.Yes, "Identifier or tag of the item the character's items are placed inside when the character despawns.", "", false)]
		[Editable]
		public Identifier DespawnContainer { get; private set; }

		// Token: 0x1700077C RID: 1916
		// (get) Token: 0x060018FB RID: 6395 RVA: 0x000C507D File Offset: 0x000C327D
		// (set) Token: 0x060018FC RID: 6396 RVA: 0x000C5085 File Offset: 0x000C3285
		[Serialize("monster", IsPropertySaveable.Yes, "If changed, this character will try to play a custom music track with the specified identifier when encountered.", "", false)]
		[Editable]
		public Identifier MusicType { get; private set; }

		// Token: 0x1700077D RID: 1917
		// (get) Token: 0x060018FD RID: 6397 RVA: 0x000C508E File Offset: 0x000C328E
		// (set) Token: 0x060018FE RID: 6398 RVA: 0x000C5096 File Offset: 0x000C3296
		[Serialize(1f, IsPropertySaveable.Yes, "The commonness of this character's music when a random track will be chosen.", "", false)]
		[Editable]
		public float MusicCommonness { get; private set; }

		// Token: 0x1700077E RID: 1918
		// (get) Token: 0x060018FF RID: 6399 RVA: 0x000C509F File Offset: 0x000C329F
		// (set) Token: 0x06001900 RID: 6400 RVA: 0x000C50A7 File Offset: 0x000C32A7
		[Serialize(1f, IsPropertySaveable.Yes, "The multiplier of the minimum distance required between this character and the player/submarine before the music starts playing. The default distance is twice the length of the submarine, or a minimum of 50 meters.", "", false)]
		[Editable]
		public float MusicRangeMultiplier { get; private set; }

		// Token: 0x1700077F RID: 1919
		// (get) Token: 0x06001901 RID: 6401 RVA: 0x000C50B0 File Offset: 0x000C32B0
		// (set) Token: 0x06001902 RID: 6402 RVA: 0x000C50B8 File Offset: 0x000C32B8
		[Serialize(false, IsPropertySaveable.Yes, "Should the entire crew get an achievement (assuming there is one) if someone from the crew kills the character?", "", false)]
		public bool UnlockKillAchievementForWholeCrew { get; set; }

		// Token: 0x17000780 RID: 1920
		// (get) Token: 0x06001903 RID: 6403 RVA: 0x000C50C1 File Offset: 0x000C32C1
		public bool IsPet
		{
			get
			{
				CharacterParams.AIParams ai = this.AI;
				return ai != null && ai.IsPet;
			}
		}

		// Token: 0x17000781 RID: 1921
		// (get) Token: 0x06001904 RID: 6404 RVA: 0x000C50D4 File Offset: 0x000C32D4
		// (set) Token: 0x06001905 RID: 6405 RVA: 0x000C50DC File Offset: 0x000C32DC
		public XDocument VariantFile { get; private set; }

		// Token: 0x17000782 RID: 1922
		// (get) Token: 0x06001906 RID: 6406 RVA: 0x000C50E5 File Offset: 0x000C32E5
		// (set) Token: 0x06001907 RID: 6407 RVA: 0x000C50ED File Offset: 0x000C32ED
		public CharacterParams.HealthParams Health { get; private set; }

		// Token: 0x17000783 RID: 1923
		// (get) Token: 0x06001908 RID: 6408 RVA: 0x000C50F6 File Offset: 0x000C32F6
		// (set) Token: 0x06001909 RID: 6409 RVA: 0x000C50FE File Offset: 0x000C32FE
		public CharacterParams.AIParams AI { get; private set; }

		// Token: 0x0600190A RID: 6410 RVA: 0x000C5108 File Offset: 0x000C3308
		public CharacterParams(CharacterFile file)
		{
			this.File = file;
			this.Load();
		}

		// Token: 0x0600190B RID: 6411 RVA: 0x000C5176 File Offset: 0x000C3376
		protected override string GetName()
		{
			return "Character Config File";
		}

		// Token: 0x17000784 RID: 1924
		// (get) Token: 0x0600190C RID: 6412 RVA: 0x000C5180 File Offset: 0x000C3380
		public override ContentXElement MainElement
		{
			get
			{
				ContentXElement mainElement = base.MainElement;
				ContentXElement contentXElement = null;
				if (mainElement == contentXElement)
				{
					return null;
				}
				if (!base.MainElement.IsOverride())
				{
					return base.MainElement;
				}
				return base.MainElement.FirstElement();
			}
		}

		// Token: 0x0600190D RID: 6413 RVA: 0x000C51C4 File Offset: 0x000C33C4
		public static XElement CreateVariantXml(ContentXElement variantXML, ContentXElement baseXML)
		{
			XElement newXml = variantXML.CreateVariantXML(baseXML, null);
			XElement variantAi = variantXML.GetChildElement("ai");
			XElement baseAi = baseXML.GetChildElement("ai");
			if (baseAi == null || baseAi.Elements().None(null) || variantAi == null || variantAi.Elements().None(null))
			{
				return newXml;
			}
			XElement finalAiElement = newXml.GetChildElement("ai", StringComparison.OrdinalIgnoreCase);
			finalAiElement.Elements().Remove<XElement>();
			baseAi.Elements().ForEach(delegate(XElement e)
			{
				finalAiElement.Add(e);
			});
			List<Identifier> processedTags = new List<Identifier>();
			foreach (XElement variantTargetElement in variantAi.Elements())
			{
				Identifier tag = variantTargetElement.GetAttributeIdentifier("tag", Identifier.Empty);
				IEnumerable<XElement> matchingElements = finalAiElement.Elements().Where(delegate(XElement e)
				{
					Identifier attributeIdentifier = e.GetAttributeIdentifier("tag", Identifier.Empty);
					return attributeIdentifier == tag;
				});
				int alreadyProcessed = processedTags.Count((Identifier t) => t == tag);
				if (matchingElements.Count<XElement>() > alreadyProcessed)
				{
					matchingElements.Skip(alreadyProcessed).First<XElement>().ReplaceWith(variantTargetElement);
				}
				else
				{
					finalAiElement.Add(variantTargetElement);
				}
				processedTags.Add(tag);
			}
			return newXml;
		}

		// Token: 0x0600190E RID: 6414 RVA: 0x000C5344 File Offset: 0x000C3544
		public bool Load()
		{
			this.UpdatePath(this.File.Path);
			this.doc = XMLExtensions.TryLoadXml(base.Path);
			ContentXElement mainElement = this.MainElement;
			ContentXElement contentXElement = null;
			if (mainElement == contentXElement)
			{
				DebugConsole.ThrowError("Main element null! Failed to load character params.", null, null, false, false);
				return false;
			}
			Identifier variantOf = this.MainElement.VariantOf();
			if (!variantOf.IsEmpty)
			{
				this.VariantFile = new XDocument(this.doc);
				XElement newRoot = CharacterParams.CreateVariantXml(this.MainElement, CharacterPrefab.FindBySpeciesName(variantOf).ConfigElement);
				ContentXElement oldElement = this.MainElement;
				XContainer parentElement = oldElement.Parent ?? this.doc;
				oldElement.Remove();
				parentElement.Add(newRoot);
			}
			base.IsLoaded = this.Deserialize(this.MainElement, true, true, true);
			base.OriginalElement = new XElement(this.MainElement).FromPackage(base.Path.ContentPackage);
			if (this.SpeciesName.IsEmpty)
			{
				mainElement = this.MainElement;
				contentXElement = null;
				if (mainElement != contentXElement)
				{
					this.SpeciesName = this.MainElement.GetAttributeIdentifier("name", "");
				}
			}
			this.CreateSubParams();
			return base.IsLoaded;
		}

		// Token: 0x0600190F RID: 6415 RVA: 0x000C548F File Offset: 0x000C368F
		public bool Save(string fileNameWithoutExtension = null)
		{
			if (this.VariantFile != null)
			{
				return false;
			}
			this.Serialize(null, true, true);
			return base.Save(fileNameWithoutExtension, new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true,
				NewLineOnAttributes = false
			});
		}

		// Token: 0x06001910 RID: 6416 RVA: 0x000C54C8 File Offset: 0x000C36C8
		public override bool Reset(bool forceReload = false)
		{
			if (forceReload)
			{
				return this.Load();
			}
			this.Deserialize(base.OriginalElement, true, true, true);
			this.SubParams.ForEach(delegate(CharacterParams.SubParam sp)
			{
				sp.Reset();
			});
			return true;
		}

		// Token: 0x06001911 RID: 6417 RVA: 0x000C551F File Offset: 0x000C371F
		public static bool CompareGroup(Identifier group1, Identifier group2)
		{
			return group1 != Identifier.Empty && group2 != Identifier.Empty && group1 == group2;
		}

		// Token: 0x06001912 RID: 6418 RVA: 0x000C5548 File Offset: 0x000C3748
		protected void CreateSubParams()
		{
			ContentXElement contentXElement = this.MainElement;
			ContentXElement contentXElement2 = null;
			if (contentXElement == contentXElement2)
			{
				DebugConsole.ThrowError("Main element null, cannot create sub params!", null, null, false, false);
				return;
			}
			this.SubParams.Clear();
			ContentXElement healthElement = this.MainElement.GetChildElement("health");
			contentXElement = null;
			if (healthElement != contentXElement)
			{
				this.Health = new CharacterParams.HealthParams(healthElement, this);
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 1);
				defaultInterpolatedStringHandler.AppendLiteral("No health parameters defined for character \"");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.SpeciesName);
				defaultInterpolatedStringHandler.AppendLiteral("\".");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				this.Health = new CharacterParams.HealthParams(null, this);
			}
			this.SubParams.Add(this.Health);
			ContentXElement ai = this.MainElement.GetChildElement("ai");
			contentXElement = null;
			if (ai != contentXElement)
			{
				this.AI = new CharacterParams.AIParams(ai, this);
				this.SubParams.Add(this.AI);
			}
			foreach (ContentXElement element in this.MainElement.GetChildElements("bloodemitter"))
			{
				CharacterParams.ParticleParams emitter = new CharacterParams.ParticleParams(element, this);
				this.BloodEmitters.Add(emitter);
				this.SubParams.Add(emitter);
			}
			foreach (ContentXElement element2 in this.MainElement.GetChildElements("gibemitter"))
			{
				CharacterParams.ParticleParams emitter2 = new CharacterParams.ParticleParams(element2, this);
				this.GibEmitters.Add(emitter2);
				this.SubParams.Add(emitter2);
			}
			foreach (ContentXElement element3 in this.MainElement.GetChildElements("damageemitter"))
			{
				CharacterParams.ParticleParams emitter3 = new CharacterParams.ParticleParams(element3, this);
				this.GibEmitters.Add(emitter3);
				this.SubParams.Add(emitter3);
			}
			foreach (ContentXElement soundElement in this.MainElement.GetChildElements("sound"))
			{
				CharacterParams.SoundParams sound = new CharacterParams.SoundParams(soundElement, this);
				this.Sounds.Add(sound);
				this.SubParams.Add(sound);
			}
			foreach (ContentXElement inventoryElement in this.MainElement.GetChildElements("inventory"))
			{
				CharacterParams.InventoryParams inventory = new CharacterParams.InventoryParams(inventoryElement, this);
				this.Inventories.Add(inventory);
				this.SubParams.Add(inventory);
			}
		}

		// Token: 0x06001913 RID: 6419 RVA: 0x000C585C File Offset: 0x000C3A5C
		public bool Deserialize(XElement element = null, bool alsoChildren = true, bool recursive = true, bool loadDefaultValues = true)
		{
			if (base.Deserialize(element))
			{
				if (this.SpeciesName.IsEmpty)
				{
					this.SpeciesName = element.GetAttributeIdentifier("name", "[NAME NOT GIVEN]");
				}
				if (alsoChildren)
				{
					this.SubParams.ForEach(delegate(CharacterParams.SubParam p)
					{
						p.Deserialize(recursive);
					});
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001914 RID: 6420 RVA: 0x000C58C4 File Offset: 0x000C3AC4
		public bool Serialize(XElement element = null, bool alsoChildren = true, bool recursive = true)
		{
			if (base.Serialize(element))
			{
				if (alsoChildren)
				{
					this.SubParams.ForEach(delegate(CharacterParams.SubParam p)
					{
						p.Serialize(recursive);
					});
				}
				return true;
			}
			return false;
		}

		// Token: 0x06001915 RID: 6421 RVA: 0x000C5904 File Offset: 0x000C3B04
		public bool AddSound()
		{
			CharacterParams.SoundParams soundParams;
			return this.TryAddSubParam<CharacterParams.SoundParams>(base.CreateElement("sound", Array.Empty<object>()), (ContentXElement e, CharacterParams c) => new CharacterParams.SoundParams(e, c), out soundParams, this.Sounds, null);
		}

		// Token: 0x06001916 RID: 6422 RVA: 0x000C5950 File Offset: 0x000C3B50
		public void AddInventory()
		{
			CharacterParams.InventoryParams inventoryParams;
			this.TryAddSubParam<CharacterParams.InventoryParams>(base.CreateElement("inventory", new object[]
			{
				new XElement("item")
			}), (ContentXElement e, CharacterParams c) => new CharacterParams.InventoryParams(e, c), out inventoryParams, this.Inventories, null);
		}

		// Token: 0x06001917 RID: 6423 RVA: 0x000C59AF File Offset: 0x000C3BAF
		public void AddBloodEmitter()
		{
			this.AddEmitter("bloodemitter");
		}

		// Token: 0x06001918 RID: 6424 RVA: 0x000C59BC File Offset: 0x000C3BBC
		public void AddGibEmitter()
		{
			this.AddEmitter("gibemitter");
		}

		// Token: 0x06001919 RID: 6425 RVA: 0x000C59C9 File Offset: 0x000C3BC9
		public void AddDamageEmitter()
		{
			this.AddEmitter("damageemitter");
		}

		// Token: 0x0600191A RID: 6426 RVA: 0x000C59D8 File Offset: 0x000C3BD8
		private void AddEmitter(string type)
		{
			CharacterParams.ParticleParams particleParams;
			if (type == "gibemitter")
			{
				this.TryAddSubParam<CharacterParams.ParticleParams>(base.CreateElement(type, Array.Empty<object>()), (ContentXElement e, CharacterParams c) => new CharacterParams.ParticleParams(e, c), out particleParams, this.GibEmitters, null);
				return;
			}
			if (type == "bloodemitter")
			{
				this.TryAddSubParam<CharacterParams.ParticleParams>(base.CreateElement(type, Array.Empty<object>()), (ContentXElement e, CharacterParams c) => new CharacterParams.ParticleParams(e, c), out particleParams, this.BloodEmitters, null);
				return;
			}
			if (!(type == "damageemitter"))
			{
				throw new NotImplementedException(type);
			}
			this.TryAddSubParam<CharacterParams.ParticleParams>(base.CreateElement(type, Array.Empty<object>()), (ContentXElement e, CharacterParams c) => new CharacterParams.ParticleParams(e, c), out particleParams, this.DamageEmitters, null);
		}

		// Token: 0x0600191B RID: 6427 RVA: 0x000C5ACB File Offset: 0x000C3CCB
		public bool RemoveSound(CharacterParams.SoundParams soundParams)
		{
			return this.RemoveSubParam<CharacterParams.SoundParams>(soundParams, null);
		}

		// Token: 0x0600191C RID: 6428 RVA: 0x000C5AD5 File Offset: 0x000C3CD5
		public bool RemoveBloodEmitter(CharacterParams.ParticleParams emitter)
		{
			return this.RemoveSubParam<CharacterParams.ParticleParams>(emitter, this.BloodEmitters);
		}

		// Token: 0x0600191D RID: 6429 RVA: 0x000C5AE4 File Offset: 0x000C3CE4
		public bool RemoveGibEmitter(CharacterParams.ParticleParams emitter)
		{
			return this.RemoveSubParam<CharacterParams.ParticleParams>(emitter, this.GibEmitters);
		}

		// Token: 0x0600191E RID: 6430 RVA: 0x000C5AF3 File Offset: 0x000C3CF3
		public bool RemoveDamageEmitter(CharacterParams.ParticleParams emitter)
		{
			return this.RemoveSubParam<CharacterParams.ParticleParams>(emitter, this.DamageEmitters);
		}

		// Token: 0x0600191F RID: 6431 RVA: 0x000C5B02 File Offset: 0x000C3D02
		public bool RemoveInventory(CharacterParams.InventoryParams inventory)
		{
			return this.RemoveSubParam<CharacterParams.InventoryParams>(inventory, this.Inventories);
		}

		// Token: 0x06001920 RID: 6432 RVA: 0x000C5B14 File Offset: 0x000C3D14
		protected bool RemoveSubParam<T>(T subParam, IList<T> collection = null) where T : CharacterParams.SubParam
		{
			if (subParam != null)
			{
				ContentXElement element = subParam.Element;
				ContentXElement contentXElement = null;
				if (!(element == contentXElement))
				{
					ContentXElement parent = subParam.Element.Parent;
					ContentXElement contentXElement2 = null;
					if (!(parent == contentXElement2))
					{
						if (collection != null && !collection.Contains(subParam))
						{
							return false;
						}
						if (!this.SubParams.Contains(subParam))
						{
							return false;
						}
						if (collection != null)
						{
							collection.Remove(subParam);
						}
						this.SubParams.Remove(subParam);
						subParam.Element.Remove();
						return true;
					}
				}
			}
			return false;
		}

		// Token: 0x06001921 RID: 6433 RVA: 0x000C5BB4 File Offset: 0x000C3DB4
		protected bool TryAddSubParam<T>(ContentXElement element, Func<ContentXElement, CharacterParams, T> constructor, out T subParam, IList<T> collection = null, Func<IList<T>, bool> filter = null) where T : CharacterParams.SubParam
		{
			subParam = constructor(element, this);
			if (collection != null && filter != null && filter(collection))
			{
				return false;
			}
			this.MainElement.Add(element);
			this.SubParams.Add(subParam);
			if (collection != null)
			{
				collection.Add(subParam);
			}
			return subParam != null;
		}

		// Token: 0x04000BD7 RID: 3031
		private HashSet<Identifier> tags = new HashSet<Identifier>();

		// Token: 0x04000C03 RID: 3075
		public readonly CharacterFile File;

		// Token: 0x04000C05 RID: 3077
		public readonly List<CharacterParams.SubParam> SubParams = new List<CharacterParams.SubParam>();

		// Token: 0x04000C06 RID: 3078
		public readonly List<CharacterParams.SoundParams> Sounds = new List<CharacterParams.SoundParams>();

		// Token: 0x04000C07 RID: 3079
		public readonly List<CharacterParams.ParticleParams> BloodEmitters = new List<CharacterParams.ParticleParams>();

		// Token: 0x04000C08 RID: 3080
		public readonly List<CharacterParams.ParticleParams> GibEmitters = new List<CharacterParams.ParticleParams>();

		// Token: 0x04000C09 RID: 3081
		public readonly List<CharacterParams.ParticleParams> DamageEmitters = new List<CharacterParams.ParticleParams>();

		// Token: 0x04000C0A RID: 3082
		public readonly List<CharacterParams.InventoryParams> Inventories = new List<CharacterParams.InventoryParams>();

		// Token: 0x020008A2 RID: 2210
		public class SoundParams : CharacterParams.SubParam
		{
			// Token: 0x17001466 RID: 5222
			// (get) Token: 0x0600558C RID: 21900 RVA: 0x001F2C99 File Offset: 0x001F0E99
			public override string Name
			{
				get
				{
					return "Sound";
				}
			}

			// Token: 0x17001467 RID: 5223
			// (get) Token: 0x0600558D RID: 21901 RVA: 0x001F2CA0 File Offset: 0x001F0EA0
			// (set) Token: 0x0600558E RID: 21902 RVA: 0x001F2CA8 File Offset: 0x001F0EA8
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string File { get; private set; }

			// Token: 0x17001468 RID: 5224
			// (get) Token: 0x0600558F RID: 21903 RVA: 0x001F2CB1 File Offset: 0x001F0EB1
			// (set) Token: 0x06005590 RID: 21904 RVA: 0x001F2CB9 File Offset: 0x001F0EB9
			[Serialize(1000f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 10000f, 1)]
			public float Range { get; private set; }

			// Token: 0x17001469 RID: 5225
			// (get) Token: 0x06005591 RID: 21905 RVA: 0x001F2CC2 File Offset: 0x001F0EC2
			// (set) Token: 0x06005592 RID: 21906 RVA: 0x001F2CCA File Offset: 0x001F0ECA
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 2f, 1)]
			public float Volume { get; private set; }

			// Token: 0x1700146A RID: 5226
			// (get) Token: 0x06005593 RID: 21907 RVA: 0x001F2CD3 File Offset: 0x001F0ED3
			// (set) Token: 0x06005594 RID: 21908 RVA: 0x001F2CE5 File Offset: 0x001F0EE5
			[Serialize("", IsPropertySaveable.Yes, "Which tags are required for this sound to play?", "", false)]
			[Editable]
			public string Tags
			{
				get
				{
					return this.TagSet.ConvertToString(",");
				}
				private set
				{
					this.TagSet = value.ToIdentifiers(",").ToImmutableHashSet<Identifier>();
				}
			}

			// Token: 0x1700146B RID: 5227
			// (get) Token: 0x06005595 RID: 21909 RVA: 0x001F2CFD File Offset: 0x001F0EFD
			// (set) Token: 0x06005596 RID: 21910 RVA: 0x001F2D05 File Offset: 0x001F0F05
			public ImmutableHashSet<Identifier> TagSet { get; private set; } = ImmutableHashSet<Identifier>.Empty;

			// Token: 0x06005597 RID: 21911 RVA: 0x001F2D10 File Offset: 0x001F0F10
			public SoundParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
				Identifier genderFallback = element.GetAttributeIdentifier("gender", "");
				if (genderFallback != Identifier.Empty && genderFallback != "None")
				{
					this.TagSet = this.TagSet.Add(genderFallback);
				}
			}
		}

		// Token: 0x020008A3 RID: 2211
		public class ParticleParams : CharacterParams.SubParam
		{
			// Token: 0x1700146C RID: 5228
			// (get) Token: 0x06005598 RID: 21912 RVA: 0x001F2D70 File Offset: 0x001F0F70
			public override string Name
			{
				get
				{
					if (this.name == null)
					{
						ContentXElement element = base.Element;
						ContentXElement contentXElement = null;
						if (element != contentXElement)
						{
							this.name = base.Element.Name.ToString().FormatCamelCaseWithSpaces();
						}
					}
					return this.name;
				}
			}

			// Token: 0x1700146D RID: 5229
			// (get) Token: 0x06005599 RID: 21913 RVA: 0x001F2DBA File Offset: 0x001F0FBA
			// (set) Token: 0x0600559A RID: 21914 RVA: 0x001F2DC2 File Offset: 0x001F0FC2
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string Particle { get; set; }

			// Token: 0x1700146E RID: 5230
			// (get) Token: 0x0600559B RID: 21915 RVA: 0x001F2DCB File Offset: 0x001F0FCB
			// (set) Token: 0x0600559C RID: 21916 RVA: 0x001F2DD3 File Offset: 0x001F0FD3
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(-360f, 360f, 0)]
			public float AngleMin { get; private set; }

			// Token: 0x1700146F RID: 5231
			// (get) Token: 0x0600559D RID: 21917 RVA: 0x001F2DDC File Offset: 0x001F0FDC
			// (set) Token: 0x0600559E RID: 21918 RVA: 0x001F2DE4 File Offset: 0x001F0FE4
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(-360f, 360f, 0)]
			public float AngleMax { get; private set; }

			// Token: 0x17001470 RID: 5232
			// (get) Token: 0x0600559F RID: 21919 RVA: 0x001F2DED File Offset: 0x001F0FED
			// (set) Token: 0x060055A0 RID: 21920 RVA: 0x001F2DF5 File Offset: 0x001F0FF5
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 100f, 2)]
			public float ScaleMin { get; private set; }

			// Token: 0x17001471 RID: 5233
			// (get) Token: 0x060055A1 RID: 21921 RVA: 0x001F2DFE File Offset: 0x001F0FFE
			// (set) Token: 0x060055A2 RID: 21922 RVA: 0x001F2E06 File Offset: 0x001F1006
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 100f, 2)]
			public float ScaleMax { get; private set; }

			// Token: 0x17001472 RID: 5234
			// (get) Token: 0x060055A3 RID: 21923 RVA: 0x001F2E0F File Offset: 0x001F100F
			// (set) Token: 0x060055A4 RID: 21924 RVA: 0x001F2E17 File Offset: 0x001F1017
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 10000f, 0)]
			public float VelocityMin { get; private set; }

			// Token: 0x17001473 RID: 5235
			// (get) Token: 0x060055A5 RID: 21925 RVA: 0x001F2E20 File Offset: 0x001F1020
			// (set) Token: 0x060055A6 RID: 21926 RVA: 0x001F2E28 File Offset: 0x001F1028
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 10000f, 0)]
			public float VelocityMax { get; private set; }

			// Token: 0x17001474 RID: 5236
			// (get) Token: 0x060055A7 RID: 21927 RVA: 0x001F2E31 File Offset: 0x001F1031
			// (set) Token: 0x060055A8 RID: 21928 RVA: 0x001F2E39 File Offset: 0x001F1039
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 100f, 2)]
			public float EmitInterval { get; private set; }

			// Token: 0x17001475 RID: 5237
			// (get) Token: 0x060055A9 RID: 21929 RVA: 0x001F2E42 File Offset: 0x001F1042
			// (set) Token: 0x060055AA RID: 21930 RVA: 0x001F2E4A File Offset: 0x001F104A
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0, 1000)]
			public int ParticlesPerSecond { get; private set; }

			// Token: 0x17001476 RID: 5238
			// (get) Token: 0x060055AB RID: 21931 RVA: 0x001F2E53 File Offset: 0x001F1053
			// (set) Token: 0x060055AC RID: 21932 RVA: 0x001F2E5B File Offset: 0x001F105B
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0, 1000)]
			public int ParticleAmount { get; private set; }

			// Token: 0x17001477 RID: 5239
			// (get) Token: 0x060055AD RID: 21933 RVA: 0x001F2E64 File Offset: 0x001F1064
			// (set) Token: 0x060055AE RID: 21934 RVA: 0x001F2E6C File Offset: 0x001F106C
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool HighQualityCollisionDetection { get; private set; }

			// Token: 0x17001478 RID: 5240
			// (get) Token: 0x060055AF RID: 21935 RVA: 0x001F2E75 File Offset: 0x001F1075
			// (set) Token: 0x060055B0 RID: 21936 RVA: 0x001F2E7D File Offset: 0x001F107D
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool CopyEntityAngle { get; private set; }

			// Token: 0x060055B1 RID: 21937 RVA: 0x001F2E86 File Offset: 0x001F1086
			public ParticleParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
			}

			// Token: 0x04003055 RID: 12373
			private string name;
		}

		// Token: 0x020008A4 RID: 2212
		public class HealthParams : CharacterParams.SubParam
		{
			// Token: 0x17001479 RID: 5241
			// (get) Token: 0x060055B2 RID: 21938 RVA: 0x001F2E90 File Offset: 0x001F1090
			public override string Name
			{
				get
				{
					return "Health";
				}
			}

			// Token: 0x1700147A RID: 5242
			// (get) Token: 0x060055B3 RID: 21939 RVA: 0x001F2E97 File Offset: 0x001F1097
			// (set) Token: 0x060055B4 RID: 21940 RVA: 0x001F2E9F File Offset: 0x001F109F
			[Serialize(100f, IsPropertySaveable.Yes, "How much (max) health does the character have?", "", false)]
			[Editable(1f, 10000f, 1)]
			public float Vitality { get; set; }

			// Token: 0x1700147B RID: 5243
			// (get) Token: 0x060055B5 RID: 21941 RVA: 0x001F2EA8 File Offset: 0x001F10A8
			// (set) Token: 0x060055B6 RID: 21942 RVA: 0x001F2EB0 File Offset: 0x001F10B0
			[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool DoesBleed { get; set; }

			// Token: 0x1700147C RID: 5244
			// (get) Token: 0x060055B7 RID: 21943 RVA: 0x001F2EB9 File Offset: 0x001F10B9
			// (set) Token: 0x060055B8 RID: 21944 RVA: 0x001F2EC1 File Offset: 0x001F10C1
			[Serialize(float.PositiveInfinity, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, float.PositiveInfinity, 1)]
			public float CrushDepth { get; set; }

			// Token: 0x1700147D RID: 5245
			// (get) Token: 0x060055B9 RID: 21945 RVA: 0x001F2ECA File Offset: 0x001F10CA
			// (set) Token: 0x060055BA RID: 21946 RVA: 0x001F2ED2 File Offset: 0x001F10D2
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			public bool UseHealthWindow { get; set; }

			// Token: 0x1700147E RID: 5246
			// (get) Token: 0x060055BB RID: 21947 RVA: 0x001F2EDB File Offset: 0x001F10DB
			// (set) Token: 0x060055BC RID: 21948 RVA: 0x001F2EE3 File Offset: 0x001F10E3
			[Serialize(0f, IsPropertySaveable.Yes, "How easily the character heals from the bleeding wounds. Default 0 (no extra healing).", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
			public float BleedingReduction { get; set; }

			// Token: 0x1700147F RID: 5247
			// (get) Token: 0x060055BD RID: 21949 RVA: 0x001F2EEC File Offset: 0x001F10EC
			// (set) Token: 0x060055BE RID: 21950 RVA: 0x001F2EF4 File Offset: 0x001F10F4
			[Serialize(0f, IsPropertySaveable.Yes, "How easily the character heals from the burn wounds. Default 0 (no extra healing).", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
			public float BurnReduction { get; set; }

			// Token: 0x17001480 RID: 5248
			// (get) Token: 0x060055BF RID: 21951 RVA: 0x001F2EFD File Offset: 0x001F10FD
			// (set) Token: 0x060055C0 RID: 21952 RVA: 0x001F2F05 File Offset: 0x001F1105
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
			public float ConstantHealthRegeneration { get; set; }

			// Token: 0x17001481 RID: 5249
			// (get) Token: 0x060055C1 RID: 21953 RVA: 0x001F2F0E File Offset: 0x001F110E
			// (set) Token: 0x060055C2 RID: 21954 RVA: 0x001F2F16 File Offset: 0x001F1116
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
			public float HealthRegenerationWhenEating { get; set; }

			// Token: 0x17001482 RID: 5250
			// (get) Token: 0x060055C3 RID: 21955 RVA: 0x001F2F1F File Offset: 0x001F111F
			// (set) Token: 0x060055C4 RID: 21956 RVA: 0x001F2F27 File Offset: 0x001F1127
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool StunImmunity { get; set; }

			// Token: 0x17001483 RID: 5251
			// (get) Token: 0x060055C5 RID: 21957 RVA: 0x001F2F30 File Offset: 0x001F1130
			// (set) Token: 0x060055C6 RID: 21958 RVA: 0x001F2F38 File Offset: 0x001F1138
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool PoisonImmunity { get; set; }

			// Token: 0x17001484 RID: 5252
			// (get) Token: 0x060055C7 RID: 21959 RVA: 0x001F2F41 File Offset: 0x001F1141
			// (set) Token: 0x060055C8 RID: 21960 RVA: 0x001F2F49 File Offset: 0x001F1149
			[Serialize(1f, IsPropertySaveable.Yes, "1 = default, 0 = immune.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 1)]
			public float PoisonVulnerability { get; set; }

			// Token: 0x17001485 RID: 5253
			// (get) Token: 0x060055C9 RID: 21961 RVA: 0x001F2F52 File Offset: 0x001F1152
			// (set) Token: 0x060055CA RID: 21962 RVA: 0x001F2F5A File Offset: 0x001F115A
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float EmpVulnerability { get; set; }

			// Token: 0x17001486 RID: 5254
			// (get) Token: 0x060055CB RID: 21963 RVA: 0x001F2F63 File Offset: 0x001F1163
			// (set) Token: 0x060055CC RID: 21964 RVA: 0x001F2F6B File Offset: 0x001F116B
			[Serialize(true, IsPropertySaveable.Yes, "Apply movement penalties when legs or tail limbs get damaged. Enabled by default.", "", false)]
			[Editable]
			public bool ApplyMovementPenalties { get; set; }

			// Token: 0x17001487 RID: 5255
			// (get) Token: 0x060055CD RID: 21965 RVA: 0x001F2F74 File Offset: 0x001F1174
			// (set) Token: 0x060055CE RID: 21966 RVA: 0x001F2F7C File Offset: 0x001F117C
			[Serialize(true, IsPropertySaveable.Yes, "Normally characters die when they don't have a head. But maybe not all of them?", "", false)]
			[Editable]
			public bool DieFromBeheading { get; set; }

			// Token: 0x17001488 RID: 5256
			// (get) Token: 0x060055CF RID: 21967 RVA: 0x001F2F85 File Offset: 0x001F1185
			// (set) Token: 0x060055D0 RID: 21968 RVA: 0x001F2F8D File Offset: 0x001F118D
			[Serialize(false, IsPropertySaveable.Yes, "Severing legs doesn't work with most characters, because we'd need to take that into account with the walking animations and the standing position of the main collider etc. But there might be cases where you'll want to override this default.", "", false)]
			[Editable]
			public bool AllowSeveringLegs { get; set; }

			// Token: 0x17001489 RID: 5257
			// (get) Token: 0x060055D1 RID: 21969 RVA: 0x001F2F96 File Offset: 0x001F1196
			// (set) Token: 0x060055D2 RID: 21970 RVA: 0x001F2F9E File Offset: 0x001F119E
			[Serialize(false, IsPropertySaveable.Yes, "Can afflictions affect the face/body tint of the character.", "", false)]
			[Editable]
			public bool ApplyAfflictionColors { get; private set; }

			// Token: 0x1700148A RID: 5258
			// (get) Token: 0x060055D3 RID: 21971 RVA: 0x001F2FA7 File Offset: 0x001F11A7
			// (set) Token: 0x060055D4 RID: 21972 RVA: 0x001F2FAF File Offset: 0x001F11AF
			[Serialize("", IsPropertySaveable.Yes, "A comma-separated list of identifiers of afflictions that the creature is immune to.", "", false)]
			[Editable]
			public string Immunities { get; private set; }

			// Token: 0x1700148B RID: 5259
			// (get) Token: 0x060055D5 RID: 21973 RVA: 0x001F2FB8 File Offset: 0x001F11B8
			public IEnumerable<Identifier> ImmunityIdentifiers
			{
				get
				{
					if (this._immunityIdentifiers == null)
					{
						this._immunityIdentifiers = base.Element.GetAttributeIdentifierArray("immunities", Array.Empty<Identifier>(), true).ToImmutableHashSet<Identifier>();
					}
					return this._immunityIdentifiers;
				}
			}

			// Token: 0x060055D6 RID: 21974 RVA: 0x001F2FEC File Offset: 0x001F11EC
			public HealthParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
				if (this.CrushDepth < 0f)
				{
					float newCrushDepth = -this.CrushDepth * Physics.DisplayToRealWorldRatio + 1000f;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(41, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Character \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(character.SpeciesName);
					defaultInterpolatedStringHandler.AppendLiteral("\" has a negative crush depth. ");
					string str = defaultInterpolatedStringHandler.ToStringAndClear();
					string str2 = "Previously the crush depths were defined as display units (e.g. -30000 would correspond to 300 meters below the level), but now they're in meters (e.g. 3000 would correspond to a depth of 3000 meters displayed on the nav terminal). ";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(35, 2);
					defaultInterpolatedStringHandler2.AppendLiteral("Changing the crush depth from ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(this.CrushDepth);
					defaultInterpolatedStringHandler2.AppendLiteral(" to ");
					defaultInterpolatedStringHandler2.AppendFormatted<float>(newCrushDepth);
					defaultInterpolatedStringHandler2.AppendLiteral(".");
					DebugConsole.AddWarning(str + str2 + defaultInterpolatedStringHandler2.ToStringAndClear(), element.ContentPackage);
					this.CrushDepth = newCrushDepth;
				}
			}

			// Token: 0x04003073 RID: 12403
			private ImmutableHashSet<Identifier> _immunityIdentifiers;
		}

		// Token: 0x020008A5 RID: 2213
		public class InventoryParams : CharacterParams.SubParam
		{
			// Token: 0x1700148C RID: 5260
			// (get) Token: 0x060055D7 RID: 21975 RVA: 0x001F30C1 File Offset: 0x001F12C1
			public override string Name
			{
				get
				{
					return "Inventory";
				}
			}

			// Token: 0x1700148D RID: 5261
			// (get) Token: 0x060055D8 RID: 21976 RVA: 0x001F30C8 File Offset: 0x001F12C8
			// (set) Token: 0x060055D9 RID: 21977 RVA: 0x001F30D0 File Offset: 0x001F12D0
			[Serialize("Any, Any", IsPropertySaveable.Yes, "Which slots the inventory holds? Accepted types: None, Any, RightHand, LeftHand, Head, InnerClothes, OuterClothes, Headset, and Card.", "", false)]
			[Editable]
			public string Slots { get; private set; }

			// Token: 0x1700148E RID: 5262
			// (get) Token: 0x060055DA RID: 21978 RVA: 0x001F30D9 File Offset: 0x001F12D9
			// (set) Token: 0x060055DB RID: 21979 RVA: 0x001F30E1 File Offset: 0x001F12E1
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool AccessibleWhenAlive { get; private set; }

			// Token: 0x1700148F RID: 5263
			// (get) Token: 0x060055DC RID: 21980 RVA: 0x001F30EA File Offset: 0x001F12EA
			// (set) Token: 0x060055DD RID: 21981 RVA: 0x001F30F2 File Offset: 0x001F12F2
			[Serialize(1f, IsPropertySaveable.Yes, "What are the odds that this inventory is spawned on the character?", "", false)]
			[Editable(0f, 1f, 1)]
			public float Commonness { get; private set; }

			// Token: 0x17001490 RID: 5264
			// (get) Token: 0x060055DE RID: 21982 RVA: 0x001F30FB File Offset: 0x001F12FB
			// (set) Token: 0x060055DF RID: 21983 RVA: 0x001F3103 File Offset: 0x001F1303
			public List<CharacterParams.InventoryParams.InventoryItem> Items { get; private set; } = new List<CharacterParams.InventoryParams.InventoryItem>();

			// Token: 0x060055E0 RID: 21984 RVA: 0x001F310C File Offset: 0x001F130C
			public InventoryParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
				foreach (ContentXElement itemElement in element.GetChildElements("item"))
				{
					CharacterParams.InventoryParams.InventoryItem item = new CharacterParams.InventoryParams.InventoryItem(itemElement, character);
					base.SubParams.Add(item);
					this.Items.Add(item);
				}
			}

			// Token: 0x060055E1 RID: 21985 RVA: 0x001F318C File Offset: 0x001F138C
			public void AddItem(string identifier = null)
			{
				ContentXElement element2 = base.Element;
				ContentXElement contentXElement = null;
				if (element2 == contentXElement)
				{
					return;
				}
				if (identifier == null)
				{
					identifier = "";
				}
				ContentXElement element = base.CreateElement("item", new object[]
				{
					new XAttribute("identifier", identifier)
				});
				base.Element.Add(element);
				CharacterParams.InventoryParams.InventoryItem item = new CharacterParams.InventoryParams.InventoryItem(element, base.Character);
				base.SubParams.Add(item);
				this.Items.Add(item);
			}

			// Token: 0x060055E2 RID: 21986 RVA: 0x001F320E File Offset: 0x001F140E
			public bool RemoveItem(CharacterParams.InventoryParams.InventoryItem item)
			{
				return base.RemoveSubParam<CharacterParams.InventoryParams.InventoryItem>(item, this.Items);
			}

			// Token: 0x02000E83 RID: 3715
			public class InventoryItem : CharacterParams.SubParam
			{
				// Token: 0x170016A4 RID: 5796
				// (get) Token: 0x06006A17 RID: 27159 RVA: 0x00225AA4 File Offset: 0x00223CA4
				public override string Name
				{
					get
					{
						return "Item";
					}
				}

				// Token: 0x170016A5 RID: 5797
				// (get) Token: 0x06006A18 RID: 27160 RVA: 0x00225AAB File Offset: 0x00223CAB
				// (set) Token: 0x06006A19 RID: 27161 RVA: 0x00225AB3 File Offset: 0x00223CB3
				[Serialize("", IsPropertySaveable.Yes, "Item identifier.", "", false)]
				[Editable]
				public string Identifier { get; private set; }

				// Token: 0x06006A1A RID: 27162 RVA: 0x00225ABC File Offset: 0x00223CBC
				public InventoryItem(ContentXElement element, CharacterParams character) : base(element, character)
				{
				}
			}
		}

		// Token: 0x020008A6 RID: 2214
		public class AIParams : CharacterParams.SubParam
		{
			// Token: 0x17001491 RID: 5265
			// (get) Token: 0x060055E3 RID: 21987 RVA: 0x001F321D File Offset: 0x001F141D
			public override string Name
			{
				get
				{
					return "AI";
				}
			}

			// Token: 0x17001492 RID: 5266
			// (get) Token: 0x060055E4 RID: 21988 RVA: 0x001F3224 File Offset: 0x001F1424
			// (set) Token: 0x060055E5 RID: 21989 RVA: 0x001F322C File Offset: 0x001F142C
			[Serialize(1f, IsPropertySaveable.Yes, "How strong other characters think this character is? Only affects AI.", "", false)]
			[Editable]
			public float CombatStrength { get; private set; }

			// Token: 0x17001493 RID: 5267
			// (get) Token: 0x060055E6 RID: 21990 RVA: 0x001F3235 File Offset: 0x001F1435
			// (set) Token: 0x060055E7 RID: 21991 RVA: 0x001F323D File Offset: 0x001F143D
			[Serialize(1f, IsPropertySaveable.Yes, "Affects how far the character can see the targets. Used as a multiplier.", "", false)]
			[Editable(0f, 10f, 1)]
			public float Sight { get; private set; }

			// Token: 0x17001494 RID: 5268
			// (get) Token: 0x060055E8 RID: 21992 RVA: 0x001F3246 File Offset: 0x001F1446
			// (set) Token: 0x060055E9 RID: 21993 RVA: 0x001F324E File Offset: 0x001F144E
			[Serialize(1f, IsPropertySaveable.Yes, "Affects how far the character can hear the targets. Used as a multiplier.", "", false)]
			[Editable(0f, 10f, 1)]
			public float Hearing { get; private set; }

			// Token: 0x17001495 RID: 5269
			// (get) Token: 0x060055EA RID: 21994 RVA: 0x001F3257 File Offset: 0x001F1457
			// (set) Token: 0x060055EB RID: 21995 RVA: 0x001F325F File Offset: 0x001F145F
			[Serialize(-1f, IsPropertySaveable.Yes, "Hard limit to how far the character can spot targets from, regardless of the sight/hearing or how visible or how much noise the target is making. Not used if set to negative.", "", false)]
			[Editable]
			public float MaxPerceptionDistance { get; set; }

			// Token: 0x17001496 RID: 5270
			// (get) Token: 0x060055EC RID: 21996 RVA: 0x001F3268 File Offset: 0x001F1468
			// (set) Token: 0x060055ED RID: 21997 RVA: 0x001F3270 File Offset: 0x001F1470
			[Serialize(100f, IsPropertySaveable.Yes, "How much the targeting priority increases each time the character takes damage. Works like the greed value, described above. The default value is 100.", "", false)]
			[Editable(-1000f, 1000f, 1)]
			public float AggressionHurt { get; private set; }

			// Token: 0x17001497 RID: 5271
			// (get) Token: 0x060055EE RID: 21998 RVA: 0x001F3279 File Offset: 0x001F1479
			// (set) Token: 0x060055EF RID: 21999 RVA: 0x001F3281 File Offset: 0x001F1481
			[Serialize(10f, IsPropertySaveable.Yes, "How much the targeting priority increases each time the character does damage to the target. The actual priority adjustment is calculated based on the damage percentage multiplied by the greed value. The default value is 10, which means the priority will increase by 1 every time the character does damage 10% of the target's current health. If the damage is 50%, then the priority increase is 5.", "", false)]
			[Editable(0f, 1000f, 1)]
			public float AggressionGreed { get; private set; }

			// Token: 0x17001498 RID: 5272
			// (get) Token: 0x060055F0 RID: 22000 RVA: 0x001F328A File Offset: 0x001F148A
			// (set) Token: 0x060055F1 RID: 22001 RVA: 0x001F3292 File Offset: 0x001F1492
			[Serialize(0f, IsPropertySaveable.Yes, "If the health drops below this threshold, the character flees. In percentages.", "", false)]
			[Editable(0f, 100f, 1)]
			public float FleeHealthThreshold { get; set; }

			// Token: 0x17001499 RID: 5273
			// (get) Token: 0x060055F2 RID: 22002 RVA: 0x001F329B File Offset: 0x001F149B
			// (set) Token: 0x060055F3 RID: 22003 RVA: 0x001F32A3 File Offset: 0x001F14A3
			[Serialize(false, IsPropertySaveable.Yes, "Does the character attack when provoked? When enabled, overrides the predefined targeting state with Attack and increases the priority of it.", "", false)]
			[Editable]
			public bool AttackWhenProvoked { get; private set; }

			// Token: 0x1700149A RID: 5274
			// (get) Token: 0x060055F4 RID: 22004 RVA: 0x001F32AC File Offset: 0x001F14AC
			// (set) Token: 0x060055F5 RID: 22005 RVA: 0x001F32B4 File Offset: 0x001F14B4
			[Serialize(false, IsPropertySaveable.Yes, "The character will flee for a brief moment when being shot at if not performing an attack.", "", false)]
			[Editable]
			public bool AvoidGunfire { get; private set; }

			// Token: 0x1700149B RID: 5275
			// (get) Token: 0x060055F6 RID: 22006 RVA: 0x001F32BD File Offset: 0x001F14BD
			// (set) Token: 0x060055F7 RID: 22007 RVA: 0x001F32C5 File Offset: 0x001F14C5
			[Serialize(0f, IsPropertySaveable.Yes, "How much damage is required for single attack to trigger avoiding/releasing targets.", "", false)]
			[Editable(0f, 1000f, 1)]
			public float DamageThreshold { get; private set; }

			// Token: 0x1700149C RID: 5276
			// (get) Token: 0x060055F8 RID: 22008 RVA: 0x001F32CE File Offset: 0x001F14CE
			// (set) Token: 0x060055F9 RID: 22009 RVA: 0x001F32D6 File Offset: 0x001F14D6
			[Serialize(3f, IsPropertySaveable.Yes, "How long the creature avoids gunfire. Also used when the creature is unlatched.", "", false)]
			[Editable(0f, 100f, 1)]
			public float AvoidTime { get; private set; }

			// Token: 0x1700149D RID: 5277
			// (get) Token: 0x060055FA RID: 22010 RVA: 0x001F32DF File Offset: 0x001F14DF
			// (set) Token: 0x060055FB RID: 22011 RVA: 0x001F32E7 File Offset: 0x001F14E7
			[Serialize(20f, IsPropertySaveable.Yes, "How long the creature flees before returning to normal state. When the creature sees the target or is being chased, it will always flee, if it's in the flee state.", "", false)]
			[Editable(0f, 100f, 1)]
			public float MinFleeTime { get; private set; }

			// Token: 0x1700149E RID: 5278
			// (get) Token: 0x060055FC RID: 22012 RVA: 0x001F32F0 File Offset: 0x001F14F0
			// (set) Token: 0x060055FD RID: 22013 RVA: 0x001F32F8 File Offset: 0x001F14F8
			[Serialize(false, IsPropertySaveable.Yes, "Does the character try to break inside the sub?", "", false)]
			[Editable]
			public bool AggressiveBoarding { get; private set; }

			// Token: 0x1700149F RID: 5279
			// (get) Token: 0x060055FE RID: 22014 RVA: 0x001F3301 File Offset: 0x001F1501
			// (set) Token: 0x060055FF RID: 22015 RVA: 0x001F3309 File Offset: 0x001F1509
			[Serialize(true, IsPropertySaveable.Yes, "Enforce aggressive behavior if the creature is spawned as a target of a monster mission.", "", false)]
			[Editable]
			public bool EnforceAggressiveBehaviorForMissions { get; private set; }

			// Token: 0x170014A0 RID: 5280
			// (get) Token: 0x06005600 RID: 22016 RVA: 0x001F3312 File Offset: 0x001F1512
			// (set) Token: 0x06005601 RID: 22017 RVA: 0x001F331A File Offset: 0x001F151A
			[Serialize(true, IsPropertySaveable.Yes, "Should the character target or ignore walls when it's outside the submarine.", "", false)]
			[Editable]
			public bool TargetOuterWalls { get; private set; }

			// Token: 0x170014A1 RID: 5281
			// (get) Token: 0x06005602 RID: 22018 RVA: 0x001F3323 File Offset: 0x001F1523
			// (set) Token: 0x06005603 RID: 22019 RVA: 0x001F332B File Offset: 0x001F152B
			[Serialize(false, IsPropertySaveable.Yes, "If disabled (default), the character selects the limb based on a formula where the parameters are a) the priority of the attack b) the distance to the target, and c) the range of the attackIf enabled, the character chooses randomly from the available attacks. The priority is used as a weight for weighted random. The distance to the target is in this case ignored.", "", false)]
			[Editable]
			public bool RandomAttack { get; private set; }

			// Token: 0x170014A2 RID: 5282
			// (get) Token: 0x06005604 RID: 22020 RVA: 0x001F3334 File Offset: 0x001F1534
			// (set) Token: 0x06005605 RID: 22021 RVA: 0x001F333C File Offset: 0x001F153C
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature know how to open doors (still requires a proper ID card). Humans can always open doors (They don't use this AI definition).", "", false)]
			[Editable]
			public bool CanOpenDoors { get; private set; }

			// Token: 0x170014A3 RID: 5283
			// (get) Token: 0x06005606 RID: 22022 RVA: 0x001F3345 File Offset: 0x001F1545
			// (set) Token: 0x06005607 RID: 22023 RVA: 0x001F334D File Offset: 0x001F154D
			[Serialize(false, IsPropertySaveable.Yes, "Unlike human AI, monsters normally only use pathfinding when they are inside the submarine. When this is enabled, the monsters can also use pathfinding to get inside the sub. In practice, via doors and hatches.", "", false)]
			[Editable]
			public bool UsePathFindingToGetInside { get; set; }

			// Token: 0x170014A4 RID: 5284
			// (get) Token: 0x06005608 RID: 22024 RVA: 0x001F3356 File Offset: 0x001F1556
			// (set) Token: 0x06005609 RID: 22025 RVA: 0x001F335E File Offset: 0x001F155E
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature close the doors behind it. Humans don't use this AI definition.", "", false)]
			[Editable]
			public bool KeepDoorsClosed { get; private set; }

			// Token: 0x170014A5 RID: 5285
			// (get) Token: 0x0600560A RID: 22026 RVA: 0x001F3367 File Offset: 0x001F1567
			// (set) Token: 0x0600560B RID: 22027 RVA: 0x001F336F File Offset: 0x001F156F
			[Serialize(true, IsPropertySaveable.Yes, "Is the creature allowed to navigate from and into the depths of the abyss? When enabled, the creatures will try to avoid the depths.", "", false)]
			[Editable]
			public bool AvoidAbyss { get; set; }

			// Token: 0x170014A6 RID: 5286
			// (get) Token: 0x0600560C RID: 22028 RVA: 0x001F3378 File Offset: 0x001F1578
			// (set) Token: 0x0600560D RID: 22029 RVA: 0x001F3380 File Offset: 0x001F1580
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature try to keep in the abyss? Has effect only when AvoidAbyss is false.", "", false)]
			[Editable]
			public bool StayInAbyss { get; set; }

			// Token: 0x170014A7 RID: 5287
			// (get) Token: 0x0600560E RID: 22030 RVA: 0x001F3389 File Offset: 0x001F1589
			// (set) Token: 0x0600560F RID: 22031 RVA: 0x001F3391 File Offset: 0x001F1591
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature patrol the flooded hulls while idling inside a friendly submarine?", "", false)]
			[Editable]
			public bool PatrolFlooded { get; set; }

			// Token: 0x170014A8 RID: 5288
			// (get) Token: 0x06005610 RID: 22032 RVA: 0x001F339A File Offset: 0x001F159A
			// (set) Token: 0x06005611 RID: 22033 RVA: 0x001F33A2 File Offset: 0x001F15A2
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature patrol the dry hulls while idling inside a friendly submarine?", "", false)]
			[Editable]
			public bool PatrolDry { get; set; }

			// Token: 0x170014A9 RID: 5289
			// (get) Token: 0x06005612 RID: 22034 RVA: 0x001F33AB File Offset: 0x001F15AB
			// (set) Token: 0x06005613 RID: 22035 RVA: 0x001F33B3 File Offset: 0x001F15B3
			[Serialize(0f, IsPropertySaveable.Yes, "Initial aggression used in the circle attack pattern (0-100). The aggression affects how close and how fast to the target the monster circles.", "", false)]
			[Editable]
			public float StartAggression { get; private set; }

			// Token: 0x170014AA RID: 5290
			// (get) Token: 0x06005614 RID: 22036 RVA: 0x001F33BC File Offset: 0x001F15BC
			// (set) Token: 0x06005615 RID: 22037 RVA: 0x001F33C4 File Offset: 0x001F15C4
			[Serialize(100f, IsPropertySaveable.Yes, "Maximum aggression used in the circle attack pattern (0-100). The aggression affects how close and how fast to the target the monster circles.", "", false)]
			[Editable]
			public float MaxAggression { get; private set; }

			// Token: 0x170014AB RID: 5291
			// (get) Token: 0x06005616 RID: 22038 RVA: 0x001F33CD File Offset: 0x001F15CD
			// (set) Token: 0x06005617 RID: 22039 RVA: 0x001F33D5 File Offset: 0x001F15D5
			[Serialize(0f, IsPropertySaveable.Yes, "How quickly the aggression level increases from StartAggression to MaxAggression when using the circle attack pattern. Artificial amount, applied once per attack cycle.", "", false)]
			[Editable]
			public float AggressionCumulation { get; private set; }

			// Token: 0x170014AC RID: 5292
			// (get) Token: 0x06005618 RID: 22040 RVA: 0x001F33DE File Offset: 0x001F15DE
			// (set) Token: 0x06005619 RID: 22041 RVA: 0x001F33E6 File Offset: 0x001F15E6
			[Serialize(WallTargetingMethod.Target, IsPropertySaveable.Yes, "Defines the method of checking whether there's a blocking (submarine) wall.", "", false)]
			[Editable]
			public WallTargetingMethod WallTargetingMethod { get; private set; }

			// Token: 0x170014AD RID: 5293
			// (get) Token: 0x0600561A RID: 22042 RVA: 0x001F33EF File Offset: 0x001F15EF
			// (set) Token: 0x0600561B RID: 22043 RVA: 0x001F33F7 File Offset: 0x001F15F7
			[Serialize(0f, IsPropertySaveable.Yes, "How likely it is that the creature plays dead (= ragdolls) while idling? Only allowed inside a sub (not in the open waters). Evaluated once, when the creature spawns.", "", false)]
			[Editable]
			public float PlayDeadProbability { get; set; }

			// Token: 0x170014AE RID: 5294
			// (get) Token: 0x0600561C RID: 22044 RVA: 0x001F3400 File Offset: 0x001F1600
			public IEnumerable<CharacterParams.TargetParams> Targets
			{
				get
				{
					return this.targets;
				}
			}

			// Token: 0x0600561D RID: 22045 RVA: 0x001F3408 File Offset: 0x001F1608
			public AIParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
				ContentXElement contentXElement = null;
				if (element == contentXElement)
				{
					return;
				}
				element.GetChildElements("target").ForEach(delegate(ContentXElement t)
				{
					this.AddTarget(t);
				});
				element.GetChildElements("targetpriority").ForEach(delegate(ContentXElement t)
				{
					this.AddTarget(t);
				});
				contentXElement = element.GetChildElement("petbehavior");
				ContentXElement contentXElement2 = null;
				this.IsPet = (contentXElement != contentXElement2);
			}

			// Token: 0x0600561E RID: 22046 RVA: 0x001F348C File Offset: 0x001F168C
			private bool TryAddTarget(ContentXElement targetElement, out CharacterParams.TargetParams target)
			{
				string tag = targetElement.GetAttributeString("tag", null);
				if (this.HasTag(tag))
				{
					target = null;
					DebugConsole.AddWarning("Trying to add multiple targets with the same tag ('" + tag + "') defined! Only the first will be used!", targetElement.ContentPackage);
				}
				else
				{
					target = this.AddTarget(targetElement);
				}
				return target != null;
			}

			// Token: 0x0600561F RID: 22047 RVA: 0x001F34E0 File Offset: 0x001F16E0
			private CharacterParams.TargetParams AddTarget(ContentXElement targetElement)
			{
				CharacterParams.TargetParams target = new CharacterParams.TargetParams(targetElement, base.Character);
				this.targets.Add(target);
				base.SubParams.Add(target);
				return target;
			}

			// Token: 0x06005620 RID: 22048 RVA: 0x001F3514 File Offset: 0x001F1714
			public bool TryAddEmptyTarget(out CharacterParams.TargetParams targetParams)
			{
				return this.TryAddNewTarget("newtarget" + this.targets.Count.ToString(), AIState.Attack, 0f, out targetParams);
			}

			// Token: 0x06005621 RID: 22049 RVA: 0x001F354B File Offset: 0x001F174B
			public bool TryAddNewTarget(string tag, AIState state, float priority, out CharacterParams.TargetParams targetParams)
			{
				return this.TryAddNewTarget(tag.ToIdentifier(), state, priority, out targetParams);
			}

			// Token: 0x06005622 RID: 22050 RVA: 0x001F3560 File Offset: 0x001F1760
			public bool TryAddNewTarget(Identifier tag, AIState state, float priority, out CharacterParams.TargetParams targetParams)
			{
				ContentXElement element2 = base.Element;
				ContentXElement contentXElement = null;
				if (element2 == contentXElement)
				{
					targetParams = null;
					return false;
				}
				ContentXElement element = CharacterParams.TargetParams.CreateNewElement(base.Character, tag, state, priority);
				if (this.TryAddTarget(element, out targetParams))
				{
					base.Element.Add(element);
					return true;
				}
				return false;
			}

			// Token: 0x06005623 RID: 22051 RVA: 0x001F35B0 File Offset: 0x001F17B0
			public bool HasTag(string tag)
			{
				return this.HasTag(tag.ToIdentifier());
			}

			// Token: 0x06005624 RID: 22052 RVA: 0x001F35C0 File Offset: 0x001F17C0
			public bool HasTag(Identifier tag)
			{
				return !(tag == null) && this.targets.Any(delegate(CharacterParams.TargetParams t)
				{
					Identifier tag2 = t.Tag;
					return tag2 == tag;
				});
			}

			// Token: 0x06005625 RID: 22053 RVA: 0x001F3601 File Offset: 0x001F1801
			public bool RemoveTarget(CharacterParams.TargetParams target)
			{
				return base.RemoveSubParam<CharacterParams.TargetParams>(target, this.targets);
			}

			// Token: 0x06005626 RID: 22054 RVA: 0x001F3610 File Offset: 0x001F1810
			public IEnumerable<CharacterParams.TargetParams> GetMatchingTargets(Func<CharacterParams.TargetParams, bool> predicate)
			{
				return this.targets.Where(predicate);
			}

			// Token: 0x06005627 RID: 22055 RVA: 0x001F3620 File Offset: 0x001F1820
			public IEnumerable<CharacterParams.TargetParams> GetTargets(Identifier target)
			{
				return this.GetMatchingTargets(delegate(CharacterParams.TargetParams t)
				{
					Identifier tag = t.Tag;
					return tag == target;
				});
			}

			// Token: 0x06005628 RID: 22056 RVA: 0x001F364C File Offset: 0x001F184C
			public IEnumerable<CharacterParams.TargetParams> GetTargets(Character target)
			{
				return this.GetMatchingTargets(delegate(CharacterParams.TargetParams t)
				{
					Identifier tag = t.Tag;
					Identifier speciesName = target.SpeciesName;
					if (!(tag == speciesName))
					{
						Identifier tag2 = t.Tag;
						Identifier group = target.Params.Group;
						if (!(tag2 == group))
						{
							return target.Params.HasTag(t.Tag);
						}
					}
					return true;
				});
			}

			// Token: 0x06005629 RID: 22057 RVA: 0x001F3678 File Offset: 0x001F1878
			public CharacterParams.TargetParams GetHighestPriorityTarget(Identifier target)
			{
				return CharacterParams.AIParams.GetHighestPriorityTarget(this.GetTargets(target));
			}

			// Token: 0x0600562A RID: 22058 RVA: 0x001F3686 File Offset: 0x001F1886
			public CharacterParams.TargetParams GetHighestPriorityTarget(Character target)
			{
				return CharacterParams.AIParams.GetHighestPriorityTarget(this.GetTargets(target));
			}

			// Token: 0x0600562B RID: 22059 RVA: 0x001F3694 File Offset: 0x001F1894
			private static CharacterParams.TargetParams GetHighestPriorityTarget(IEnumerable<CharacterParams.TargetParams> targetParams)
			{
				return targetParams.MaxBy((CharacterParams.TargetParams t) => t.Priority);
			}

			// Token: 0x0600562C RID: 22060 RVA: 0x001F36BB File Offset: 0x001F18BB
			public bool TryGetTargets(Identifier target, out IEnumerable<CharacterParams.TargetParams> targetParams)
			{
				targetParams = this.GetTargets(target);
				return targetParams.Any<CharacterParams.TargetParams>();
			}

			// Token: 0x0600562D RID: 22061 RVA: 0x001F36CD File Offset: 0x001F18CD
			public bool TryGetTargets(Character target, out IEnumerable<CharacterParams.TargetParams> targetParams)
			{
				targetParams = this.GetTargets(target);
				return targetParams.Any<CharacterParams.TargetParams>();
			}

			// Token: 0x0600562E RID: 22062 RVA: 0x001F36DF File Offset: 0x001F18DF
			public bool TryGetHighestPriorityTarget(Identifier target, out CharacterParams.TargetParams targetParams)
			{
				targetParams = this.GetHighestPriorityTarget(target);
				return targetParams != null;
			}

			// Token: 0x0600562F RID: 22063 RVA: 0x001F36EF File Offset: 0x001F18EF
			public bool TryGetHighestPriorityTarget(Character target, out CharacterParams.TargetParams targetParams)
			{
				targetParams = this.GetHighestPriorityTarget(target);
				return targetParams != null;
			}

			// Token: 0x06005630 RID: 22064 RVA: 0x001F3700 File Offset: 0x001F1900
			public bool TryGetHighestPriorityTarget(IEnumerable<Identifier> tags, out CharacterParams.TargetParams target)
			{
				target = null;
				if (tags == null || tags.None(null))
				{
					return false;
				}
				float priority = -1f;
				using (List<CharacterParams.TargetParams>.Enumerator enumerator = this.targets.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						CharacterParams.TargetParams potentialTarget = enumerator.Current;
						if (potentialTarget.Priority > priority && tags.Any(delegate(Identifier t)
						{
							Identifier tag = potentialTarget.Tag;
							return t == tag;
						}))
						{
							target = potentialTarget;
							priority = target.Priority;
						}
					}
				}
				return target != null;
			}

			// Token: 0x04003094 RID: 12436
			public readonly bool IsPet;

			// Token: 0x04003095 RID: 12437
			private readonly List<CharacterParams.TargetParams> targets = new List<CharacterParams.TargetParams>();
		}

		// Token: 0x020008A7 RID: 2215
		public class TargetParams : CharacterParams.SubParam
		{
			// Token: 0x170014AF RID: 5295
			// (get) Token: 0x06005633 RID: 22067 RVA: 0x001F37BC File Offset: 0x001F19BC
			public override string Name
			{
				get
				{
					return "Target";
				}
			}

			// Token: 0x170014B0 RID: 5296
			// (get) Token: 0x06005634 RID: 22068 RVA: 0x001F37C3 File Offset: 0x001F19C3
			// (set) Token: 0x06005635 RID: 22069 RVA: 0x001F37CB File Offset: 0x001F19CB
			[Serialize("", IsPropertySaveable.Yes, "Can be an item tag, species name or something else. Examples: decoy, provocative, light, dead, human, crawler, wall, nasonov, sonar, door, stronger, weaker, light, human, room...", "", false)]
			[Editable]
			public Identifier Tag { get; private set; }

			// Token: 0x170014B1 RID: 5297
			// (get) Token: 0x06005636 RID: 22070 RVA: 0x001F37D4 File Offset: 0x001F19D4
			// (set) Token: 0x06005637 RID: 22071 RVA: 0x001F37DC File Offset: 0x001F19DC
			[Serialize(AIState.Idle, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public AIState State { get; set; }

			// Token: 0x170014B2 RID: 5298
			// (get) Token: 0x06005638 RID: 22072 RVA: 0x001F37E5 File Offset: 0x001F19E5
			// (set) Token: 0x06005639 RID: 22073 RVA: 0x001F37ED File Offset: 0x001F19ED
			[Serialize(0f, IsPropertySaveable.Yes, "What base priority is given to the target?", "", false)]
			[Editable(0f, 1000f, 1, ValueStep = 1f, DecimalCount = 0)]
			public float Priority { get; set; }

			// Token: 0x170014B3 RID: 5299
			// (get) Token: 0x0600563A RID: 22074 RVA: 0x001F37F6 File Offset: 0x001F19F6
			// (set) Token: 0x0600563B RID: 22075 RVA: 0x001F37FE File Offset: 0x001F19FE
			[Serialize(0f, IsPropertySaveable.Yes, "Generic distance that can be used for different purposes depending on the state. E.g. in Avoid state this defines the distance that the character tries to keep to the target. If the distance is 0, it's not used.", "", false)]
			[Editable(MinValueFloat = 0f, ValueStep = 10f, DecimalCount = 0)]
			public float ReactDistance { get; set; }

			// Token: 0x170014B4 RID: 5300
			// (get) Token: 0x0600563C RID: 22076 RVA: 0x001F3807 File Offset: 0x001F1A07
			// (set) Token: 0x0600563D RID: 22077 RVA: 0x001F380F File Offset: 0x001F1A0F
			[Serialize(0f, IsPropertySaveable.Yes, "Used for defining the attack distance for PassiveAggressive and Aggressive states. If the distance is 0, it's not used.", "", false)]
			[Editable(MinValueFloat = 0f, ValueStep = 10f, DecimalCount = 0)]
			public float AttackDistance { get; set; }

			// Token: 0x170014B5 RID: 5301
			// (get) Token: 0x0600563E RID: 22078 RVA: 0x001F3818 File Offset: 0x001F1A18
			// (set) Token: 0x0600563F RID: 22079 RVA: 0x001F3820 File Offset: 0x001F1A20
			[Serialize(0f, IsPropertySaveable.Yes, "Generic timer that can be used for different purposes depending on the state. E.g. in Observe state this defines how long the character in general keeps staring the targets (Some random is always applied).", "", false)]
			[Editable]
			public float Timer { get; set; }

			// Token: 0x170014B6 RID: 5302
			// (get) Token: 0x06005640 RID: 22080 RVA: 0x001F3829 File Offset: 0x001F1A29
			// (set) Token: 0x06005641 RID: 22081 RVA: 0x001F3831 File Offset: 0x001F1A31
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored if it's inside a container/inventory. Only affects items.", "", false)]
			[Editable]
			public bool IgnoreContained { get; set; }

			// Token: 0x170014B7 RID: 5303
			// (get) Token: 0x06005642 RID: 22082 RVA: 0x001F383A File Offset: 0x001F1A3A
			// (set) Token: 0x06005643 RID: 22083 RVA: 0x001F3842 File Offset: 0x001F1A42
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored while the creature is inside. Doesn't matter where the target is.", "", false)]
			[Editable]
			public bool IgnoreInside { get; set; }

			// Token: 0x170014B8 RID: 5304
			// (get) Token: 0x06005644 RID: 22084 RVA: 0x001F384B File Offset: 0x001F1A4B
			// (set) Token: 0x06005645 RID: 22085 RVA: 0x001F3853 File Offset: 0x001F1A53
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored while the creature is outside. Doesn't matter where the target is.", "", false)]
			[Editable]
			public bool IgnoreOutside { get; set; }

			// Token: 0x170014B9 RID: 5305
			// (get) Token: 0x06005646 RID: 22086 RVA: 0x001F385C File Offset: 0x001F1A5C
			// (set) Token: 0x06005647 RID: 22087 RVA: 0x001F3864 File Offset: 0x001F1A64
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored if it's inside. Doesn't matter where the creature itself is.", "", false)]
			[Editable]
			public bool IgnoreTargetInside { get; set; }

			// Token: 0x170014BA RID: 5306
			// (get) Token: 0x06005648 RID: 22088 RVA: 0x001F386D File Offset: 0x001F1A6D
			// (set) Token: 0x06005649 RID: 22089 RVA: 0x001F3875 File Offset: 0x001F1A75
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored if it's outside. Doesn't matter where the creature itself is.", "", false)]
			[Editable]
			public bool IgnoreTargetOutside { get; set; }

			// Token: 0x170014BB RID: 5307
			// (get) Token: 0x0600564A RID: 22090 RVA: 0x001F387E File Offset: 0x001F1A7E
			// (set) Token: 0x0600564B RID: 22091 RVA: 0x001F3886 File Offset: 0x001F1A86
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored if it's inside a different submarine than us? Normally only some targets are ignored when they are not inside the same sub.", "", false)]
			[Editable]
			public bool IgnoreIfNotInSameSub { get; set; }

			// Token: 0x170014BC RID: 5308
			// (get) Token: 0x0600564C RID: 22092 RVA: 0x001F388F File Offset: 0x001F1A8F
			// (set) Token: 0x0600564D RID: 22093 RVA: 0x001F3897 File Offset: 0x001F1A97
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool IgnoreIncapacitated { get; set; }

			// Token: 0x170014BD RID: 5309
			// (get) Token: 0x0600564E RID: 22094 RVA: 0x001F38A0 File Offset: 0x001F1AA0
			// (set) Token: 0x0600564F RID: 22095 RVA: 0x001F38A8 File Offset: 0x001F1AA8
			[Serialize(0f, IsPropertySaveable.Yes, "A generic threshold. For example, how much damage the protected target should take from an attacker before the creature starts defending it.", "", false)]
			[Editable]
			public float Threshold { get; private set; }

			// Token: 0x170014BE RID: 5310
			// (get) Token: 0x06005650 RID: 22096 RVA: 0x001F38B1 File Offset: 0x001F1AB1
			// (set) Token: 0x06005651 RID: 22097 RVA: 0x001F38B9 File Offset: 0x001F1AB9
			[Serialize(-1f, IsPropertySaveable.Yes, "A generic min threshold. Not used if set to negative.", "", false)]
			[Editable]
			public float ThresholdMin { get; private set; }

			// Token: 0x170014BF RID: 5311
			// (get) Token: 0x06005652 RID: 22098 RVA: 0x001F38C2 File Offset: 0x001F1AC2
			// (set) Token: 0x06005653 RID: 22099 RVA: 0x001F38CA File Offset: 0x001F1ACA
			[Serialize(-1f, IsPropertySaveable.Yes, "A generic max threshold. Not used if set to negative.", "", false)]
			[Editable]
			public float ThresholdMax { get; private set; }

			// Token: 0x170014C0 RID: 5312
			// (get) Token: 0x06005654 RID: 22100 RVA: 0x001F38D3 File Offset: 0x001F1AD3
			// (set) Token: 0x06005655 RID: 22101 RVA: 0x001F38DB File Offset: 0x001F1ADB
			[Serialize(1f, IsPropertySaveable.Yes, "Can be used to make the monster perceive the target further or closer than it normally can.", "", false)]
			[Editable]
			public float PerceptionDistanceMultiplier { get; private set; }

			// Token: 0x170014C1 RID: 5313
			// (get) Token: 0x06005656 RID: 22102 RVA: 0x001F38E4 File Offset: 0x001F1AE4
			// (set) Token: 0x06005657 RID: 22103 RVA: 0x001F38EC File Offset: 0x001F1AEC
			[Serialize(-1f, IsPropertySaveable.Yes, "Maximum distance at which the monster can perceive the target, regardless of the sight/hearing or how visible or how much noise the target is making. Not used if set to negative.", "", false)]
			[Editable]
			public float MaxPerceptionDistance { get; private set; }

			// Token: 0x170014C2 RID: 5314
			// (get) Token: 0x06005658 RID: 22104 RVA: 0x001F38F5 File Offset: 0x001F1AF5
			// (set) Token: 0x06005659 RID: 22105 RVA: 0x001F38FD File Offset: 0x001F1AFD
			[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "A generic offset. Used for example for offsetting the react distance (vector length) and for offsetting the target position when a guardian flees to a pod.", "", false)]
			[Editable]
			public Vector2 Offset { get; private set; }

			// Token: 0x170014C3 RID: 5315
			// (get) Token: 0x0600565A RID: 22106 RVA: 0x001F3906 File Offset: 0x001F1B06
			// (set) Token: 0x0600565B RID: 22107 RVA: 0x001F390E File Offset: 0x001F1B0E
			[Serialize(AttackPattern.Straight, IsPropertySaveable.Yes, "Defines the movement pattern of the character when approaching a target.", "", false)]
			[Editable]
			public AttackPattern AttackPattern { get; set; }

			// Token: 0x170014C4 RID: 5316
			// (get) Token: 0x0600565C RID: 22108 RVA: 0x001F3917 File Offset: 0x001F1B17
			// (set) Token: 0x0600565D RID: 22109 RVA: 0x001F391F File Offset: 0x001F1B1F
			[Serialize(false, IsPropertySaveable.Yes, "If enabled, the AI will give more priority to targets close to the horizontal middle of the sub. Only applies to walls, hulls, and items like sonar. Circle and Sweep always does this regardless of this property.", "", false)]
			[Editable]
			public bool PrioritizeSubCenter { get; set; }

			// Token: 0x170014C5 RID: 5317
			// (get) Token: 0x0600565E RID: 22110 RVA: 0x001F3928 File Offset: 0x001F1B28
			// (set) Token: 0x0600565F RID: 22111 RVA: 0x001F3930 File Offset: 0x001F1B30
			[Serialize(0f, IsPropertySaveable.Yes, "Use to define a distance at which the creature starts the sweeping movement.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f, ValueStep = 1f, DecimalCount = 0)]
			public float SweepDistance { get; private set; }

			// Token: 0x170014C6 RID: 5318
			// (get) Token: 0x06005660 RID: 22112 RVA: 0x001F3939 File Offset: 0x001F1B39
			// (set) Token: 0x06005661 RID: 22113 RVA: 0x001F3941 File Offset: 0x001F1B41
			[Serialize(10f, IsPropertySaveable.Yes, "How much the sweep affects the steering?", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, ValueStep = 1f, DecimalCount = 1)]
			public float SweepStrength { get; private set; }

			// Token: 0x170014C7 RID: 5319
			// (get) Token: 0x06005662 RID: 22114 RVA: 0x001F394A File Offset: 0x001F1B4A
			// (set) Token: 0x06005663 RID: 22115 RVA: 0x001F3952 File Offset: 0x001F1B52
			[Serialize(1f, IsPropertySaveable.Yes, "How quickly the sweep direction changes. Uses the sine wave pattern.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, ValueStep = 0.1f, DecimalCount = 2)]
			public float SweepSpeed { get; private set; }

			// Token: 0x170014C8 RID: 5320
			// (get) Token: 0x06005664 RID: 22116 RVA: 0x001F395B File Offset: 0x001F1B5B
			// (set) Token: 0x06005665 RID: 22117 RVA: 0x001F3963 File Offset: 0x001F1B63
			[Serialize(5000f, IsPropertySaveable.Yes, "How close to the target the character should be, before they start using the circle pattern instead of directional approaching.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 20000f)]
			public float CircleStartDistance { get; private set; }

			// Token: 0x170014C9 RID: 5321
			// (get) Token: 0x06005666 RID: 22118 RVA: 0x001F396C File Offset: 0x001F1B6C
			// (set) Token: 0x06005667 RID: 22119 RVA: 0x001F3974 File Offset: 0x001F1B74
			[Serialize(false, IsPropertySaveable.Yes, "Normally the target size is taken into account when calculating the distance to the target. Set this true to skip that.", "", false)]
			public bool IgnoreTargetSize { get; private set; }

			// Token: 0x170014CA RID: 5322
			// (get) Token: 0x06005668 RID: 22120 RVA: 0x001F397D File Offset: 0x001F1B7D
			// (set) Token: 0x06005669 RID: 22121 RVA: 0x001F3985 File Offset: 0x001F1B85
			[Serialize(1f, IsPropertySaveable.Yes, "Determines the rate how quickly the target movement position is rotated towards the attack target. The actual rotation is calculated once per each attack cycle, based on the current aggression level.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
			public float CircleRotationSpeed { get; private set; }

			// Token: 0x170014CB RID: 5323
			// (get) Token: 0x0600566A RID: 22122 RVA: 0x001F398E File Offset: 0x001F1B8E
			// (set) Token: 0x0600566B RID: 22123 RVA: 0x001F3996 File Offset: 0x001F1B96
			[Serialize(false, IsPropertySaveable.Yes, "When enabled, the circle rotation speed can change when the target is far. When this setting is disabled (default), the character will head directly towards the target when it's too far.", "", false)]
			[Editable]
			public bool DynamicCircleRotationSpeed { get; private set; }

			// Token: 0x170014CC RID: 5324
			// (get) Token: 0x0600566C RID: 22124 RVA: 0x001F399F File Offset: 0x001F1B9F
			// (set) Token: 0x0600566D RID: 22125 RVA: 0x001F39A7 File Offset: 0x001F1BA7
			[Serialize(0f, IsPropertySaveable.Yes, "How much the turn speed can differ between attack cycles (stays constant during the cycle)", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
			public float CircleRandomRotationFactor { get; private set; }

			// Token: 0x170014CD RID: 5325
			// (get) Token: 0x0600566E RID: 22126 RVA: 0x001F39B0 File Offset: 0x001F1BB0
			// (set) Token: 0x0600566F RID: 22127 RVA: 0x001F39B8 File Offset: 0x001F1BB8
			[Serialize(5f, IsPropertySaveable.Yes, "Affects how close to the target the character has to be before the strike phase of the circle behavior triggers. In the strike phase, the creature moves directly towards the target.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
			public float CircleStrikeDistanceMultiplier { get; private set; }

			// Token: 0x170014CE RID: 5326
			// (get) Token: 0x06005670 RID: 22128 RVA: 0x001F39C1 File Offset: 0x001F1BC1
			// (set) Token: 0x06005671 RID: 22129 RVA: 0x001F39C9 File Offset: 0x001F1BC9
			[Serialize(0f, IsPropertySaveable.Yes, "How much the target position is offset at maximum. Low values make the character hit the target earlier/always, higher values make it miss the target when the aggression intensity is low (early in the encounter).", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 50f)]
			public float CircleMaxRandomOffset { get; private set; }

			// Token: 0x170014CF RID: 5327
			// (get) Token: 0x06005672 RID: 22130 RVA: 0x001F39D2 File Offset: 0x001F1BD2
			// (set) Token: 0x06005673 RID: 22131 RVA: 0x001F39DA File Offset: 0x001F1BDA
			public List<PropertyConditional> Conditionals { get; private set; } = new List<PropertyConditional>();

			// Token: 0x06005674 RID: 22132 RVA: 0x001F39E3 File Offset: 0x001F1BE3
			public TargetParams(string tag, AIState state, float priority, CharacterParams character) : this(CharacterParams.TargetParams.CreateNewElement(character, tag, state, priority), character)
			{
			}

			// Token: 0x06005675 RID: 22133 RVA: 0x001F39F8 File Offset: 0x001F1BF8
			public TargetParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
				foreach (ContentXElement subElement in element.Elements())
				{
					string a = subElement.Name.ToString().ToLowerInvariant();
					if (a == "conditional")
					{
						this.Conditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
					}
				}
			}

			// Token: 0x06005676 RID: 22134 RVA: 0x001F3A84 File Offset: 0x001F1C84
			public static ContentXElement CreateNewElement(CharacterParams character, Identifier tag, AIState state, float priority)
			{
				return CharacterParams.TargetParams.CreateNewElement(character, tag.Value, state, priority);
			}

			// Token: 0x06005677 RID: 22135 RVA: 0x001F3A98 File Offset: 0x001F1C98
			public static ContentXElement CreateNewElement(CharacterParams character, string tag, AIState state, float priority)
			{
				return new XElement("target", new object[]
				{
					new XAttribute("tag", tag),
					new XAttribute("state", state),
					new XAttribute("priority", priority)
				}).FromPackage(character.File.ContentPackage);
			}
		}

		// Token: 0x020008A8 RID: 2216
		public abstract class SubParam : ISerializableEntity
		{
			// Token: 0x170014D0 RID: 5328
			// (get) Token: 0x06005678 RID: 22136 RVA: 0x001F3B0D File Offset: 0x001F1D0D
			// (set) Token: 0x06005679 RID: 22137 RVA: 0x001F3B15 File Offset: 0x001F1D15
			public virtual string Name { get; set; }

			// Token: 0x170014D1 RID: 5329
			// (get) Token: 0x0600567A RID: 22138 RVA: 0x001F3B1E File Offset: 0x001F1D1E
			// (set) Token: 0x0600567B RID: 22139 RVA: 0x001F3B26 File Offset: 0x001F1D26
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

			// Token: 0x170014D2 RID: 5330
			// (get) Token: 0x0600567C RID: 22140 RVA: 0x001F3B2F File Offset: 0x001F1D2F
			// (set) Token: 0x0600567D RID: 22141 RVA: 0x001F3B37 File Offset: 0x001F1D37
			public ContentXElement Element { get; set; }

			// Token: 0x170014D3 RID: 5331
			// (get) Token: 0x0600567E RID: 22142 RVA: 0x001F3B40 File Offset: 0x001F1D40
			// (set) Token: 0x0600567F RID: 22143 RVA: 0x001F3B48 File Offset: 0x001F1D48
			public List<CharacterParams.SubParam> SubParams { get; set; } = new List<CharacterParams.SubParam>();

			// Token: 0x170014D4 RID: 5332
			// (get) Token: 0x06005680 RID: 22144 RVA: 0x001F3B51 File Offset: 0x001F1D51
			// (set) Token: 0x06005681 RID: 22145 RVA: 0x001F3B59 File Offset: 0x001F1D59
			public CharacterParams Character { get; private set; }

			// Token: 0x06005682 RID: 22146 RVA: 0x001F3B62 File Offset: 0x001F1D62
			protected ContentXElement CreateElement(string name, params object[] attrs)
			{
				return new XElement(name, attrs).FromPackage(this.Element.ContentPackage);
			}

			// Token: 0x06005683 RID: 22147 RVA: 0x001F3B80 File Offset: 0x001F1D80
			public SubParam(ContentXElement element, CharacterParams character)
			{
				this.Element = element;
				this.Character = character;
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			}

			// Token: 0x06005684 RID: 22148 RVA: 0x001F3BB4 File Offset: 0x001F1DB4
			public virtual bool Deserialize(bool recursive = true)
			{
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, this.Element);
				if (recursive)
				{
					this.SubParams.ForEach(delegate(CharacterParams.SubParam sp)
					{
						sp.Deserialize(true);
					});
				}
				return this.SerializableProperties != null;
			}

			// Token: 0x06005685 RID: 22149 RVA: 0x001F3C10 File Offset: 0x001F1E10
			public virtual bool Serialize(bool recursive = true)
			{
				SerializableProperty.SerializeProperties(this, this.Element, true, false);
				if (recursive)
				{
					this.SubParams.ForEach(delegate(CharacterParams.SubParam sp)
					{
						sp.Serialize(true);
					});
				}
				return true;
			}

			// Token: 0x06005686 RID: 22150 RVA: 0x001F3C5E File Offset: 0x001F1E5E
			public virtual void Reset()
			{
				this.Deserialize(false);
				this.SubParams.ForEach(delegate(CharacterParams.SubParam sp)
				{
					sp.Reset();
				});
			}

			// Token: 0x06005687 RID: 22151 RVA: 0x001F3C94 File Offset: 0x001F1E94
			protected bool RemoveSubParam<T>(T subParam, IList<T> collection = null) where T : CharacterParams.SubParam
			{
				if (subParam != null)
				{
					ContentXElement element = subParam.Element;
					ContentXElement contentXElement = null;
					if (!(element == contentXElement))
					{
						ContentXElement parent = subParam.Element.Parent;
						ContentXElement contentXElement2 = null;
						if (!(parent == contentXElement2))
						{
							if (collection != null && !collection.Contains(subParam))
							{
								return false;
							}
							if (!this.SubParams.Contains(subParam))
							{
								return false;
							}
							if (collection != null)
							{
								collection.Remove(subParam);
							}
							this.SubParams.Remove(subParam);
							subParam.Element.Remove();
							return true;
						}
					}
				}
				return false;
			}
		}
	}
}
