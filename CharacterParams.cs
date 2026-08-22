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
	// Token: 0x020001EA RID: 490
	internal class CharacterParams : EditableParams
	{
		// Token: 0x17000DBA RID: 3514
		// (get) Token: 0x060033A4 RID: 13220 RVA: 0x0020B591 File Offset: 0x00209791
		// (set) Token: 0x060033A5 RID: 13221 RVA: 0x0020B599 File Offset: 0x00209799
		[Serialize("", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Identifier SpeciesName { get; private set; }

		// Token: 0x17000DBB RID: 3515
		// (get) Token: 0x060033A6 RID: 13222 RVA: 0x0020B5A2 File Offset: 0x002097A2
		// (set) Token: 0x060033A7 RID: 13223 RVA: 0x0020B5B4 File Offset: 0x002097B4
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

		// Token: 0x060033A8 RID: 13224 RVA: 0x0020B5CC File Offset: 0x002097CC
		public bool HasTag(Identifier tag)
		{
			return this.tags.Contains(tag);
		}

		// Token: 0x17000DBC RID: 3516
		// (get) Token: 0x060033A9 RID: 13225 RVA: 0x0020B5DA File Offset: 0x002097DA
		// (set) Token: 0x060033AA RID: 13226 RVA: 0x0020B5E2 File Offset: 0x002097E2
		[Serialize("", IsPropertySaveable.Yes, "References to another species. Define only if the creature is a variant that needs to use a pre-existing translation.", "", false)]
		[Editable]
		public Identifier SpeciesTranslationOverride { get; private set; }

		// Token: 0x17000DBD RID: 3517
		// (get) Token: 0x060033AB RID: 13227 RVA: 0x0020B5EB File Offset: 0x002097EB
		// (set) Token: 0x060033AC RID: 13228 RVA: 0x0020B5F3 File Offset: 0x002097F3
		[Serialize("", IsPropertySaveable.Yes, "Overrides the name of the character, shown to the player. If the display name is not defined, the game first tries to find the translated name. If that is not found, the species name will be used.", "", false)]
		[Editable]
		public string DisplayName { get; private set; }

		// Token: 0x17000DBE RID: 3518
		// (get) Token: 0x060033AD RID: 13229 RVA: 0x0020B5FC File Offset: 0x002097FC
		// (set) Token: 0x060033AE RID: 13230 RVA: 0x0020B604 File Offset: 0x00209804
		[Serialize("", IsPropertySaveable.Yes, "If defined, different species of the same group consider each other friendly and do not attack each other.", "", false)]
		[Editable]
		public Identifier Group { get; private set; }

		// Token: 0x17000DBF RID: 3519
		// (get) Token: 0x060033AF RID: 13231 RVA: 0x0020B60D File Offset: 0x0020980D
		// (set) Token: 0x060033B0 RID: 13232 RVA: 0x0020B615 File Offset: 0x00209815
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, the character is a humanoid and has different animation constraints relative to non-humanoid characters.", "", false)]
		[Editable(ReadOnly = true)]
		public bool Humanoid { get; private set; }

		// Token: 0x17000DC0 RID: 3520
		// (get) Token: 0x060033B1 RID: 13233 RVA: 0x0020B61E File Offset: 0x0020981E
		// (set) Token: 0x060033B2 RID: 13234 RVA: 0x0020B626 File Offset: 0x00209826
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, jobs can be assigned to characters of this species. Should be true for the player characters.", "", false)]
		[Editable(ReadOnly = true)]
		public bool HasInfo { get; private set; }

		// Token: 0x17000DC1 RID: 3521
		// (get) Token: 0x060033B3 RID: 13235 RVA: 0x0020B62F File Offset: 0x0020982F
		// (set) Token: 0x060033B4 RID: 13236 RVA: 0x0020B637 File Offset: 0x00209837
		[Serialize(false, IsPropertySaveable.Yes, "Can the creature interact with items?", "", false)]
		[Editable]
		public bool CanInteract { get; private set; }

		// Token: 0x17000DC2 RID: 3522
		// (get) Token: 0x060033B5 RID: 13237 RVA: 0x0020B640 File Offset: 0x00209840
		// (set) Token: 0x060033B6 RID: 13238 RVA: 0x0020B648 File Offset: 0x00209848
		[Serialize(true, IsPropertySaveable.Yes, "Can the creature use ladders? Doesn't have an effect, if CanInteract is false.", "", false)]
		[Editable]
		public bool CanClimb { get; private set; }

		// Token: 0x17000DC3 RID: 3523
		// (get) Token: 0x060033B7 RID: 13239 RVA: 0x0020B651 File Offset: 0x00209851
		// (set) Token: 0x060033B8 RID: 13240 RVA: 0x0020B659 File Offset: 0x00209859
		[Serialize(false, IsPropertySaveable.Yes, "If set true, this character only uses the climbing parameters defined in the walk parameters (not run).", "", false)]
		[Editable]
		public bool ForceSlowClimbing { get; private set; }

		// Token: 0x17000DC4 RID: 3524
		// (get) Token: 0x060033B9 RID: 13241 RVA: 0x0020B662 File Offset: 0x00209862
		// (set) Token: 0x060033BA RID: 13242 RVA: 0x0020B66A File Offset: 0x0020986A
		[Serialize(false, IsPropertySaveable.Yes, "Should this character be treated as a husk?", "", false)]
		[Editable]
		public bool Husk { get; private set; }

		// Token: 0x17000DC5 RID: 3525
		// (get) Token: 0x060033BB RID: 13243 RVA: 0x0020B673 File Offset: 0x00209873
		// (set) Token: 0x060033BC RID: 13244 RVA: 0x0020B67B File Offset: 0x0020987B
		[Serialize("", IsPropertySaveable.Yes, "If this character can turn into a husk, which character it turns to? If not defined, uses the default pattern (e.g. Crawler -> Crawlerhusk, Human -> Humanhusk).", "", false)]
		[Editable]
		public Identifier HuskedSpecies { get; private set; }

		// Token: 0x17000DC6 RID: 3526
		// (get) Token: 0x060033BD RID: 13245 RVA: 0x0020B684 File Offset: 0x00209884
		// (set) Token: 0x060033BE RID: 13246 RVA: 0x0020B68C File Offset: 0x0020988C
		[Serialize("", IsPropertySaveable.Yes, "If this character is a husk, from what species it can be turned into? If not defined, uses the default pattern (e.g. Crawlerhusk -> Crawler, Humanhusk -> Human).", "", false)]
		[Editable]
		public Identifier NonHuskedSpecies { get; private set; }

		// Token: 0x17000DC7 RID: 3527
		// (get) Token: 0x060033BF RID: 13247 RVA: 0x0020B695 File Offset: 0x00209895
		// (set) Token: 0x060033C0 RID: 13248 RVA: 0x0020B69D File Offset: 0x0020989D
		[Serialize(false, IsPropertySaveable.Yes, "Should this character use a special husk appendage, attached to the ragdoll, when it turns into a husk?", "", false)]
		[Editable]
		public bool UseHuskAppendage { get; private set; }

		// Token: 0x17000DC8 RID: 3528
		// (get) Token: 0x060033C1 RID: 13249 RVA: 0x0020B6A6 File Offset: 0x002098A6
		// (set) Token: 0x060033C2 RID: 13250 RVA: 0x0020B6AE File Offset: 0x002098AE
		[Serialize(false, IsPropertySaveable.Yes, "Does this character need oxygen to survive? Enabling this also makes the character vulnerable to high pressure when swimming outside of the submarine.", "", false)]
		[Editable]
		public bool NeedsAir { get; set; }

		// Token: 0x17000DC9 RID: 3529
		// (get) Token: 0x060033C3 RID: 13251 RVA: 0x0020B6B7 File Offset: 0x002098B7
		// (set) Token: 0x060033C4 RID: 13252 RVA: 0x0020B6BF File Offset: 0x002098BF
		[Serialize(false, IsPropertySaveable.Yes, "Can the creature live without water or does it die on dry land?", "", false)]
		[Editable]
		public bool NeedsWater { get; set; }

		// Token: 0x17000DCA RID: 3530
		// (get) Token: 0x060033C5 RID: 13253 RVA: 0x0020B6C8 File Offset: 0x002098C8
		// (set) Token: 0x060033C6 RID: 13254 RVA: 0x0020B6D0 File Offset: 0x002098D0
		[Serialize(false, IsPropertySaveable.Yes, "Note: non-humans with a human AI aren't fully supported. Enabling this on a non-human character may lead to issues.", "", false)]
		public bool UseHumanAI { get; set; }

		// Token: 0x17000DCB RID: 3531
		// (get) Token: 0x060033C7 RID: 13255 RVA: 0x0020B6D9 File Offset: 0x002098D9
		// (set) Token: 0x060033C8 RID: 13256 RVA: 0x0020B6E1 File Offset: 0x002098E1
		[Serialize(false, IsPropertySaveable.Yes, "Is this creature an artificial creature, like robot or machine that shouldn't be affected by afflictions that affect only organic creatures? Overrides DoesBleed.", "", false)]
		[Editable]
		public bool IsMachine { get; set; }

		// Token: 0x17000DCC RID: 3532
		// (get) Token: 0x060033C9 RID: 13257 RVA: 0x0020B6EA File Offset: 0x002098EA
		// (set) Token: 0x060033CA RID: 13258 RVA: 0x0020B6F2 File Offset: 0x002098F2
		[Serialize(false, IsPropertySaveable.No, "Is the character able to send messages in the chat?", "", false)]
		[Editable]
		public bool CanSpeak { get; set; }

		// Token: 0x17000DCD RID: 3533
		// (get) Token: 0x060033CB RID: 13259 RVA: 0x0020B6FB File Offset: 0x002098FB
		// (set) Token: 0x060033CC RID: 13260 RVA: 0x0020B703 File Offset: 0x00209903
		[Serialize(true, IsPropertySaveable.Yes, "Is there a health bar shown above the character when it takes damage? Defaults to true.", "", false)]
		[Editable]
		public bool ShowHealthBar { get; private set; }

		// Token: 0x17000DCE RID: 3534
		// (get) Token: 0x060033CD RID: 13261 RVA: 0x0020B70C File Offset: 0x0020990C
		// (set) Token: 0x060033CE RID: 13262 RVA: 0x0020B714 File Offset: 0x00209914
		[Serialize(false, IsPropertySaveable.Yes, "Is this character's health shown at the top of the player's screen when they are in an active encounter?", "", false)]
		[Editable]
		public bool UseBossHealthBar { get; private set; }

		// Token: 0x17000DCF RID: 3535
		// (get) Token: 0x060033CF RID: 13263 RVA: 0x0020B71D File Offset: 0x0020991D
		// (set) Token: 0x060033D0 RID: 13264 RVA: 0x0020B725 File Offset: 0x00209925
		[Serialize(100f, IsPropertySaveable.Yes, "How much noise the character makes when moving?", "", false)]
		[Editable(0f, 100000f, 1)]
		public float Noise { get; set; }

		// Token: 0x17000DD0 RID: 3536
		// (get) Token: 0x060033D1 RID: 13265 RVA: 0x0020B72E File Offset: 0x0020992E
		// (set) Token: 0x060033D2 RID: 13266 RVA: 0x0020B736 File Offset: 0x00209936
		[Serialize(100f, IsPropertySaveable.Yes, "How visible the character is?", "", false)]
		[Editable(0f, 100000f, 1)]
		public float Visibility { get; set; }

		// Token: 0x17000DD1 RID: 3537
		// (get) Token: 0x060033D3 RID: 13267 RVA: 0x0020B73F File Offset: 0x0020993F
		// (set) Token: 0x060033D4 RID: 13268 RVA: 0x0020B747 File Offset: 0x00209947
		[Serialize("blood", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string BloodDecal { get; private set; }

		// Token: 0x17000DD2 RID: 3538
		// (get) Token: 0x060033D5 RID: 13269 RVA: 0x0020B750 File Offset: 0x00209950
		// (set) Token: 0x060033D6 RID: 13270 RVA: 0x0020B758 File Offset: 0x00209958
		[Serialize("blooddrop", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string BleedParticleAir { get; private set; }

		// Token: 0x17000DD3 RID: 3539
		// (get) Token: 0x060033D7 RID: 13271 RVA: 0x0020B761 File Offset: 0x00209961
		// (set) Token: 0x060033D8 RID: 13272 RVA: 0x0020B769 File Offset: 0x00209969
		[Serialize("waterblood", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public string BleedParticleWater { get; private set; }

		// Token: 0x17000DD4 RID: 3540
		// (get) Token: 0x060033D9 RID: 13273 RVA: 0x0020B772 File Offset: 0x00209972
		// (set) Token: 0x060033DA RID: 13274 RVA: 0x0020B77A File Offset: 0x0020997A
		[Serialize(1f, IsPropertySaveable.Yes, "A multiplier to increase or decrease the number of bleeding particles to create.", "", false)]
		[Editable]
		public float BleedParticleMultiplier { get; private set; }

		// Token: 0x17000DD5 RID: 3541
		// (get) Token: 0x060033DB RID: 13275 RVA: 0x0020B783 File Offset: 0x00209983
		// (set) Token: 0x060033DC RID: 13276 RVA: 0x0020B78B File Offset: 0x0020998B
		[Serialize(true, IsPropertySaveable.Yes, "Can the creature eat bodies? Used by player controlled creatures to allow them to eat. Currently applicable only to non-humanoids. To allow an AI controller to eat, just add an ai target with the state \"eat\"", "", false)]
		[Editable]
		public bool CanEat { get; set; }

		// Token: 0x17000DD6 RID: 3542
		// (get) Token: 0x060033DD RID: 13277 RVA: 0x0020B794 File Offset: 0x00209994
		// (set) Token: 0x060033DE RID: 13278 RVA: 0x0020B79C File Offset: 0x0020999C
		[Serialize(10f, IsPropertySaveable.Yes, "How effectively/easily the character eats other characters. Affects the forces, the amount of particles, and the time required before the target is eaten away", "", false)]
		[Editable(MinValueFloat = 1f, MaxValueFloat = 1000f, ValueStep = 1f)]
		public float EatingSpeed { get; set; }

		// Token: 0x17000DD7 RID: 3543
		// (get) Token: 0x060033DF RID: 13279 RVA: 0x0020B7A5 File Offset: 0x002099A5
		// (set) Token: 0x060033E0 RID: 13280 RVA: 0x0020B7AD File Offset: 0x002099AD
		[Serialize(true, IsPropertySaveable.Yes, "Should the character AI use waypoints defined in the level to find a path to its targets?", "", false)]
		[Editable]
		public bool UsePathFinding { get; set; }

		// Token: 0x17000DD8 RID: 3544
		// (get) Token: 0x060033E1 RID: 13281 RVA: 0x0020B7B6 File Offset: 0x002099B6
		// (set) Token: 0x060033E2 RID: 13282 RVA: 0x0020B7BE File Offset: 0x002099BE
		[Serialize(1f, IsPropertySaveable.Yes, "Decreases the intensive path finding call frequency. Set to a lower value for insignificant creatures to improve performance.", "", false)]
		[Editable(0f, 1f, 1)]
		public float PathFinderPriority { get; set; }

		// Token: 0x17000DD9 RID: 3545
		// (get) Token: 0x060033E3 RID: 13283 RVA: 0x0020B7C7 File Offset: 0x002099C7
		// (set) Token: 0x060033E4 RID: 13284 RVA: 0x0020B7CF File Offset: 0x002099CF
		[Serialize(false, IsPropertySaveable.Yes, "Should the character be hidden in the sonar?", "", false)]
		[Editable]
		public bool HideInSonar { get; set; }

		// Token: 0x17000DDA RID: 3546
		// (get) Token: 0x060033E5 RID: 13285 RVA: 0x0020B7D8 File Offset: 0x002099D8
		// (set) Token: 0x060033E6 RID: 13286 RVA: 0x0020B7E0 File Offset: 0x002099E0
		[Serialize(false, IsPropertySaveable.Yes, "Should the character be hidden when using thermal goggles?", "", false)]
		[Editable]
		public bool HideInThermalGoggles { get; set; }

		// Token: 0x17000DDB RID: 3547
		// (get) Token: 0x060033E7 RID: 13287 RVA: 0x0020B7E9 File Offset: 0x002099E9
		// (set) Token: 0x060033E8 RID: 13288 RVA: 0x0020B7F1 File Offset: 0x002099F1
		[Serialize(0f, IsPropertySaveable.Yes, "If set to a value greater than zero, this character creates disrupting noise on the sonar when within range.", "", false)]
		[Editable]
		public float SonarDisruption { get; set; }

		// Token: 0x17000DDC RID: 3548
		// (get) Token: 0x060033E9 RID: 13289 RVA: 0x0020B7FA File Offset: 0x002099FA
		// (set) Token: 0x060033EA RID: 13290 RVA: 0x0020B802 File Offset: 0x00209A02
		[Serialize(0f, IsPropertySaveable.Yes, "Range at which \"long distance\" blips for this character will appear on the sonar (used on some of the Abyss monsters).", "", false)]
		[Editable]
		public float DistantSonarRange { get; set; }

		// Token: 0x17000DDD RID: 3549
		// (get) Token: 0x060033EB RID: 13291 RVA: 0x0020B80B File Offset: 0x00209A0B
		// (set) Token: 0x060033EC RID: 13292 RVA: 0x0020B813 File Offset: 0x00209A13
		[Serialize(25000f, IsPropertySaveable.Yes, "If the character is farther than this (in pixels) from the sub and the players, it will be disabled. The halved value is used for triggering simple physics where the ragdoll is disabled and only the main collider is updated.", "", false)]
		[Editable(MinValueFloat = 10000f, MaxValueFloat = 100000f)]
		public float DisableDistance { get; set; }

		// Token: 0x17000DDE RID: 3550
		// (get) Token: 0x060033ED RID: 13293 RVA: 0x0020B81C File Offset: 0x00209A1C
		// (set) Token: 0x060033EE RID: 13294 RVA: 0x0020B824 File Offset: 0x00209A24
		[Serialize(10f, IsPropertySaveable.Yes, "How frequent the recurring idle and attack sounds are?", "", false)]
		[Editable(MinValueFloat = 1f, MaxValueFloat = 100f)]
		public float SoundInterval { get; set; }

		// Token: 0x17000DDF RID: 3551
		// (get) Token: 0x060033EF RID: 13295 RVA: 0x0020B82D File Offset: 0x00209A2D
		// (set) Token: 0x060033F0 RID: 13296 RVA: 0x0020B835 File Offset: 0x00209A35
		[Serialize(false, IsPropertySaveable.Yes, "Should the character be drawn on top of characters that do not have this set? This currently has no effect if the character has no deformable sprites.", "", false)]
		[Editable]
		public bool DrawLast { get; set; }

		// Token: 0x17000DE0 RID: 3552
		// (get) Token: 0x060033F1 RID: 13297 RVA: 0x0020B83E File Offset: 0x00209A3E
		// (set) Token: 0x060033F2 RID: 13298 RVA: 0x0020B846 File Offset: 0x00209A46
		[Serialize(1f, IsPropertySaveable.Yes, "Tells the bots how much they should prefer targeting this character with submarine weapons. Defaults to 1. Set 0 to tell the bots not to target this character at all. Distance to the target affects the decision making.", "", false)]
		[Editable]
		public float AITurretPriority { get; set; }

		// Token: 0x17000DE1 RID: 3553
		// (get) Token: 0x060033F3 RID: 13299 RVA: 0x0020B84F File Offset: 0x00209A4F
		// (set) Token: 0x060033F4 RID: 13300 RVA: 0x0020B857 File Offset: 0x00209A57
		[Serialize(1f, IsPropertySaveable.Yes, "Tells the bots how much they should prefer targeting this character with submarine weapons tagged as \"slowturret\", like railguns. The tag is arbitrary and can be added to any turrets, just like the priority. Defaults to 1. Not used if AITurretPriority is 0. Distance to the target affects the decision making.", "", false)]
		[Editable]
		public float AISlowTurretPriority { get; set; }

		// Token: 0x17000DE2 RID: 3554
		// (get) Token: 0x060033F5 RID: 13301 RVA: 0x0020B860 File Offset: 0x00209A60
		// (set) Token: 0x060033F6 RID: 13302 RVA: 0x0020B868 File Offset: 0x00209A68
		[Serialize("", IsPropertySaveable.Yes, "Identifier or tag of the item the character's items are placed inside when the character despawns.", "", false)]
		[Editable]
		public Identifier DespawnContainer { get; private set; }

		// Token: 0x17000DE3 RID: 3555
		// (get) Token: 0x060033F7 RID: 13303 RVA: 0x0020B871 File Offset: 0x00209A71
		// (set) Token: 0x060033F8 RID: 13304 RVA: 0x0020B879 File Offset: 0x00209A79
		[Serialize("monster", IsPropertySaveable.Yes, "If changed, this character will try to play a custom music track with the specified identifier when encountered.", "", false)]
		[Editable]
		public Identifier MusicType { get; private set; }

		// Token: 0x17000DE4 RID: 3556
		// (get) Token: 0x060033F9 RID: 13305 RVA: 0x0020B882 File Offset: 0x00209A82
		// (set) Token: 0x060033FA RID: 13306 RVA: 0x0020B88A File Offset: 0x00209A8A
		[Serialize(1f, IsPropertySaveable.Yes, "The commonness of this character's music when a random track will be chosen.", "", false)]
		[Editable]
		public float MusicCommonness { get; private set; }

		// Token: 0x17000DE5 RID: 3557
		// (get) Token: 0x060033FB RID: 13307 RVA: 0x0020B893 File Offset: 0x00209A93
		// (set) Token: 0x060033FC RID: 13308 RVA: 0x0020B89B File Offset: 0x00209A9B
		[Serialize(1f, IsPropertySaveable.Yes, "The multiplier of the minimum distance required between this character and the player/submarine before the music starts playing. The default distance is twice the length of the submarine, or a minimum of 50 meters.", "", false)]
		[Editable]
		public float MusicRangeMultiplier { get; private set; }

		// Token: 0x17000DE6 RID: 3558
		// (get) Token: 0x060033FD RID: 13309 RVA: 0x0020B8A4 File Offset: 0x00209AA4
		// (set) Token: 0x060033FE RID: 13310 RVA: 0x0020B8AC File Offset: 0x00209AAC
		[Serialize(false, IsPropertySaveable.Yes, "Should the entire crew get an achievement (assuming there is one) if someone from the crew kills the character?", "", false)]
		public bool UnlockKillAchievementForWholeCrew { get; set; }

		// Token: 0x17000DE7 RID: 3559
		// (get) Token: 0x060033FF RID: 13311 RVA: 0x0020B8B5 File Offset: 0x00209AB5
		public bool IsPet
		{
			get
			{
				CharacterParams.AIParams ai = this.AI;
				return ai != null && ai.IsPet;
			}
		}

		// Token: 0x17000DE8 RID: 3560
		// (get) Token: 0x06003400 RID: 13312 RVA: 0x0020B8C8 File Offset: 0x00209AC8
		// (set) Token: 0x06003401 RID: 13313 RVA: 0x0020B8D0 File Offset: 0x00209AD0
		public XDocument VariantFile { get; private set; }

		// Token: 0x17000DE9 RID: 3561
		// (get) Token: 0x06003402 RID: 13314 RVA: 0x0020B8D9 File Offset: 0x00209AD9
		// (set) Token: 0x06003403 RID: 13315 RVA: 0x0020B8E1 File Offset: 0x00209AE1
		public CharacterParams.HealthParams Health { get; private set; }

		// Token: 0x17000DEA RID: 3562
		// (get) Token: 0x06003404 RID: 13316 RVA: 0x0020B8EA File Offset: 0x00209AEA
		// (set) Token: 0x06003405 RID: 13317 RVA: 0x0020B8F2 File Offset: 0x00209AF2
		public CharacterParams.AIParams AI { get; private set; }

		// Token: 0x06003406 RID: 13318 RVA: 0x0020B8FC File Offset: 0x00209AFC
		public CharacterParams(CharacterFile file)
		{
			this.File = file;
			this.Load();
		}

		// Token: 0x06003407 RID: 13319 RVA: 0x0020B96A File Offset: 0x00209B6A
		protected override string GetName()
		{
			return "Character Config File";
		}

		// Token: 0x17000DEB RID: 3563
		// (get) Token: 0x06003408 RID: 13320 RVA: 0x0020B974 File Offset: 0x00209B74
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

		// Token: 0x06003409 RID: 13321 RVA: 0x0020B9B8 File Offset: 0x00209BB8
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

		// Token: 0x0600340A RID: 13322 RVA: 0x0020BB38 File Offset: 0x00209D38
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

		// Token: 0x0600340B RID: 13323 RVA: 0x0020BC83 File Offset: 0x00209E83
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

		// Token: 0x0600340C RID: 13324 RVA: 0x0020BCBC File Offset: 0x00209EBC
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

		// Token: 0x0600340D RID: 13325 RVA: 0x0020BD13 File Offset: 0x00209F13
		public static bool CompareGroup(Identifier group1, Identifier group2)
		{
			return group1 != Identifier.Empty && group2 != Identifier.Empty && group1 == group2;
		}

		// Token: 0x0600340E RID: 13326 RVA: 0x0020BD3C File Offset: 0x00209F3C
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

		// Token: 0x0600340F RID: 13327 RVA: 0x0020C050 File Offset: 0x0020A250
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

		// Token: 0x06003410 RID: 13328 RVA: 0x0020C0B8 File Offset: 0x0020A2B8
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

		// Token: 0x06003411 RID: 13329 RVA: 0x0020C0F8 File Offset: 0x0020A2F8
		public void AddToEditor(ParamsEditor editor, bool alsoChildren = true, bool recursive = true, int space = 0)
		{
			base.AddToEditor(editor, 0);
			if (alsoChildren)
			{
				this.SubParams.ForEach(delegate(CharacterParams.SubParam s)
				{
					s.AddToEditor(editor, recursive, 0, null);
				});
			}
			if (space > 0)
			{
				new GUIFrame(new RectTransform(new Point(editor.EditorBox.Rect.Width, (int)((float)space * GUI.yScale)), editor.EditorBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(ParamsEditor.Color)).CanBeFocused = false;
			}
		}

		// Token: 0x06003412 RID: 13330 RVA: 0x0020C1A8 File Offset: 0x0020A3A8
		public bool AddSound()
		{
			CharacterParams.SoundParams soundParams;
			return this.TryAddSubParam<CharacterParams.SoundParams>(base.CreateElement("sound", Array.Empty<object>()), (ContentXElement e, CharacterParams c) => new CharacterParams.SoundParams(e, c), out soundParams, this.Sounds, null);
		}

		// Token: 0x06003413 RID: 13331 RVA: 0x0020C1F4 File Offset: 0x0020A3F4
		public void AddInventory()
		{
			CharacterParams.InventoryParams inventoryParams;
			this.TryAddSubParam<CharacterParams.InventoryParams>(base.CreateElement("inventory", new object[]
			{
				new XElement("item")
			}), (ContentXElement e, CharacterParams c) => new CharacterParams.InventoryParams(e, c), out inventoryParams, this.Inventories, null);
		}

		// Token: 0x06003414 RID: 13332 RVA: 0x0020C253 File Offset: 0x0020A453
		public void AddBloodEmitter()
		{
			this.AddEmitter("bloodemitter");
		}

		// Token: 0x06003415 RID: 13333 RVA: 0x0020C260 File Offset: 0x0020A460
		public void AddGibEmitter()
		{
			this.AddEmitter("gibemitter");
		}

		// Token: 0x06003416 RID: 13334 RVA: 0x0020C26D File Offset: 0x0020A46D
		public void AddDamageEmitter()
		{
			this.AddEmitter("damageemitter");
		}

		// Token: 0x06003417 RID: 13335 RVA: 0x0020C27C File Offset: 0x0020A47C
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

		// Token: 0x06003418 RID: 13336 RVA: 0x0020C36F File Offset: 0x0020A56F
		public bool RemoveSound(CharacterParams.SoundParams soundParams)
		{
			return this.RemoveSubParam<CharacterParams.SoundParams>(soundParams, null);
		}

		// Token: 0x06003419 RID: 13337 RVA: 0x0020C379 File Offset: 0x0020A579
		public bool RemoveBloodEmitter(CharacterParams.ParticleParams emitter)
		{
			return this.RemoveSubParam<CharacterParams.ParticleParams>(emitter, this.BloodEmitters);
		}

		// Token: 0x0600341A RID: 13338 RVA: 0x0020C388 File Offset: 0x0020A588
		public bool RemoveGibEmitter(CharacterParams.ParticleParams emitter)
		{
			return this.RemoveSubParam<CharacterParams.ParticleParams>(emitter, this.GibEmitters);
		}

		// Token: 0x0600341B RID: 13339 RVA: 0x0020C397 File Offset: 0x0020A597
		public bool RemoveDamageEmitter(CharacterParams.ParticleParams emitter)
		{
			return this.RemoveSubParam<CharacterParams.ParticleParams>(emitter, this.DamageEmitters);
		}

		// Token: 0x0600341C RID: 13340 RVA: 0x0020C3A6 File Offset: 0x0020A5A6
		public bool RemoveInventory(CharacterParams.InventoryParams inventory)
		{
			return this.RemoveSubParam<CharacterParams.InventoryParams>(inventory, this.Inventories);
		}

		// Token: 0x0600341D RID: 13341 RVA: 0x0020C3B8 File Offset: 0x0020A5B8
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

		// Token: 0x0600341E RID: 13342 RVA: 0x0020C458 File Offset: 0x0020A658
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

		// Token: 0x04001AFC RID: 6908
		private HashSet<Identifier> tags = new HashSet<Identifier>();

		// Token: 0x04001B28 RID: 6952
		public readonly CharacterFile File;

		// Token: 0x04001B2A RID: 6954
		public readonly List<CharacterParams.SubParam> SubParams = new List<CharacterParams.SubParam>();

		// Token: 0x04001B2B RID: 6955
		public readonly List<CharacterParams.SoundParams> Sounds = new List<CharacterParams.SoundParams>();

		// Token: 0x04001B2C RID: 6956
		public readonly List<CharacterParams.ParticleParams> BloodEmitters = new List<CharacterParams.ParticleParams>();

		// Token: 0x04001B2D RID: 6957
		public readonly List<CharacterParams.ParticleParams> GibEmitters = new List<CharacterParams.ParticleParams>();

		// Token: 0x04001B2E RID: 6958
		public readonly List<CharacterParams.ParticleParams> DamageEmitters = new List<CharacterParams.ParticleParams>();

		// Token: 0x04001B2F RID: 6959
		public readonly List<CharacterParams.InventoryParams> Inventories = new List<CharacterParams.InventoryParams>();

		// Token: 0x02000EBA RID: 3770
		public class SoundParams : CharacterParams.SubParam
		{
			// Token: 0x17001B39 RID: 6969
			// (get) Token: 0x06008550 RID: 34128 RVA: 0x003A154D File Offset: 0x0039F74D
			public override string Name
			{
				get
				{
					return "Sound";
				}
			}

			// Token: 0x17001B3A RID: 6970
			// (get) Token: 0x06008551 RID: 34129 RVA: 0x003A1554 File Offset: 0x0039F754
			// (set) Token: 0x06008552 RID: 34130 RVA: 0x003A155C File Offset: 0x0039F75C
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string File { get; private set; }

			// Token: 0x17001B3B RID: 6971
			// (get) Token: 0x06008553 RID: 34131 RVA: 0x003A1565 File Offset: 0x0039F765
			// (set) Token: 0x06008554 RID: 34132 RVA: 0x003A156D File Offset: 0x0039F76D
			[Serialize(CharacterSound.SoundType.Idle, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public CharacterSound.SoundType State { get; private set; }

			// Token: 0x17001B3C RID: 6972
			// (get) Token: 0x06008555 RID: 34133 RVA: 0x003A1576 File Offset: 0x0039F776
			// (set) Token: 0x06008556 RID: 34134 RVA: 0x003A157E File Offset: 0x0039F77E
			[Serialize(1000f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 10000f, 1)]
			public float Range { get; private set; }

			// Token: 0x17001B3D RID: 6973
			// (get) Token: 0x06008557 RID: 34135 RVA: 0x003A1587 File Offset: 0x0039F787
			// (set) Token: 0x06008558 RID: 34136 RVA: 0x003A158F File Offset: 0x0039F78F
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 2f, 1)]
			public float Volume { get; private set; }

			// Token: 0x17001B3E RID: 6974
			// (get) Token: 0x06008559 RID: 34137 RVA: 0x003A1598 File Offset: 0x0039F798
			// (set) Token: 0x0600855A RID: 34138 RVA: 0x003A15AA File Offset: 0x0039F7AA
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

			// Token: 0x17001B3F RID: 6975
			// (get) Token: 0x0600855B RID: 34139 RVA: 0x003A15C2 File Offset: 0x0039F7C2
			// (set) Token: 0x0600855C RID: 34140 RVA: 0x003A15CA File Offset: 0x0039F7CA
			public ImmutableHashSet<Identifier> TagSet { get; private set; } = ImmutableHashSet<Identifier>.Empty;

			// Token: 0x0600855D RID: 34141 RVA: 0x003A15D4 File Offset: 0x0039F7D4
			public SoundParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
				Identifier genderFallback = element.GetAttributeIdentifier("gender", "");
				if (genderFallback != Identifier.Empty && genderFallback != "None")
				{
					this.TagSet = this.TagSet.Add(genderFallback);
				}
			}
		}

		// Token: 0x02000EBB RID: 3771
		public class ParticleParams : CharacterParams.SubParam
		{
			// Token: 0x17001B40 RID: 6976
			// (get) Token: 0x0600855E RID: 34142 RVA: 0x003A1634 File Offset: 0x0039F834
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

			// Token: 0x17001B41 RID: 6977
			// (get) Token: 0x0600855F RID: 34143 RVA: 0x003A167E File Offset: 0x0039F87E
			// (set) Token: 0x06008560 RID: 34144 RVA: 0x003A1686 File Offset: 0x0039F886
			[Serialize("", IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public string Particle { get; set; }

			// Token: 0x17001B42 RID: 6978
			// (get) Token: 0x06008561 RID: 34145 RVA: 0x003A168F File Offset: 0x0039F88F
			// (set) Token: 0x06008562 RID: 34146 RVA: 0x003A1697 File Offset: 0x0039F897
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(-360f, 360f, 0)]
			public float AngleMin { get; private set; }

			// Token: 0x17001B43 RID: 6979
			// (get) Token: 0x06008563 RID: 34147 RVA: 0x003A16A0 File Offset: 0x0039F8A0
			// (set) Token: 0x06008564 RID: 34148 RVA: 0x003A16A8 File Offset: 0x0039F8A8
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(-360f, 360f, 0)]
			public float AngleMax { get; private set; }

			// Token: 0x17001B44 RID: 6980
			// (get) Token: 0x06008565 RID: 34149 RVA: 0x003A16B1 File Offset: 0x0039F8B1
			// (set) Token: 0x06008566 RID: 34150 RVA: 0x003A16B9 File Offset: 0x0039F8B9
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 100f, 2)]
			public float ScaleMin { get; private set; }

			// Token: 0x17001B45 RID: 6981
			// (get) Token: 0x06008567 RID: 34151 RVA: 0x003A16C2 File Offset: 0x0039F8C2
			// (set) Token: 0x06008568 RID: 34152 RVA: 0x003A16CA File Offset: 0x0039F8CA
			[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 100f, 2)]
			public float ScaleMax { get; private set; }

			// Token: 0x17001B46 RID: 6982
			// (get) Token: 0x06008569 RID: 34153 RVA: 0x003A16D3 File Offset: 0x0039F8D3
			// (set) Token: 0x0600856A RID: 34154 RVA: 0x003A16DB File Offset: 0x0039F8DB
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 10000f, 0)]
			public float VelocityMin { get; private set; }

			// Token: 0x17001B47 RID: 6983
			// (get) Token: 0x0600856B RID: 34155 RVA: 0x003A16E4 File Offset: 0x0039F8E4
			// (set) Token: 0x0600856C RID: 34156 RVA: 0x003A16EC File Offset: 0x0039F8EC
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 10000f, 0)]
			public float VelocityMax { get; private set; }

			// Token: 0x17001B48 RID: 6984
			// (get) Token: 0x0600856D RID: 34157 RVA: 0x003A16F5 File Offset: 0x0039F8F5
			// (set) Token: 0x0600856E RID: 34158 RVA: 0x003A16FD File Offset: 0x0039F8FD
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, 100f, 2)]
			public float EmitInterval { get; private set; }

			// Token: 0x17001B49 RID: 6985
			// (get) Token: 0x0600856F RID: 34159 RVA: 0x003A1706 File Offset: 0x0039F906
			// (set) Token: 0x06008570 RID: 34160 RVA: 0x003A170E File Offset: 0x0039F90E
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0, 1000)]
			public int ParticlesPerSecond { get; private set; }

			// Token: 0x17001B4A RID: 6986
			// (get) Token: 0x06008571 RID: 34161 RVA: 0x003A1717 File Offset: 0x0039F917
			// (set) Token: 0x06008572 RID: 34162 RVA: 0x003A171F File Offset: 0x0039F91F
			[Serialize(0, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0, 1000)]
			public int ParticleAmount { get; private set; }

			// Token: 0x17001B4B RID: 6987
			// (get) Token: 0x06008573 RID: 34163 RVA: 0x003A1728 File Offset: 0x0039F928
			// (set) Token: 0x06008574 RID: 34164 RVA: 0x003A1730 File Offset: 0x0039F930
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool HighQualityCollisionDetection { get; private set; }

			// Token: 0x17001B4C RID: 6988
			// (get) Token: 0x06008575 RID: 34165 RVA: 0x003A1739 File Offset: 0x0039F939
			// (set) Token: 0x06008576 RID: 34166 RVA: 0x003A1741 File Offset: 0x0039F941
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool CopyEntityAngle { get; private set; }

			// Token: 0x06008577 RID: 34167 RVA: 0x003A174A File Offset: 0x0039F94A
			public ParticleParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
			}

			// Token: 0x04005330 RID: 21296
			private string name;
		}

		// Token: 0x02000EBC RID: 3772
		public class HealthParams : CharacterParams.SubParam
		{
			// Token: 0x17001B4D RID: 6989
			// (get) Token: 0x06008578 RID: 34168 RVA: 0x003A1754 File Offset: 0x0039F954
			public override string Name
			{
				get
				{
					return "Health";
				}
			}

			// Token: 0x17001B4E RID: 6990
			// (get) Token: 0x06008579 RID: 34169 RVA: 0x003A175B File Offset: 0x0039F95B
			// (set) Token: 0x0600857A RID: 34170 RVA: 0x003A1763 File Offset: 0x0039F963
			[Serialize(100f, IsPropertySaveable.Yes, "How much (max) health does the character have?", "", false)]
			[Editable(1f, 10000f, 1)]
			public float Vitality { get; set; }

			// Token: 0x17001B4F RID: 6991
			// (get) Token: 0x0600857B RID: 34171 RVA: 0x003A176C File Offset: 0x0039F96C
			// (set) Token: 0x0600857C RID: 34172 RVA: 0x003A1774 File Offset: 0x0039F974
			[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool DoesBleed { get; set; }

			// Token: 0x17001B50 RID: 6992
			// (get) Token: 0x0600857D RID: 34173 RVA: 0x003A177D File Offset: 0x0039F97D
			// (set) Token: 0x0600857E RID: 34174 RVA: 0x003A1785 File Offset: 0x0039F985
			[Serialize(float.PositiveInfinity, IsPropertySaveable.Yes, "", "", false)]
			[Editable(0f, float.PositiveInfinity, 1)]
			public float CrushDepth { get; set; }

			// Token: 0x17001B51 RID: 6993
			// (get) Token: 0x0600857F RID: 34175 RVA: 0x003A178E File Offset: 0x0039F98E
			// (set) Token: 0x06008580 RID: 34176 RVA: 0x003A1796 File Offset: 0x0039F996
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			public bool UseHealthWindow { get; set; }

			// Token: 0x17001B52 RID: 6994
			// (get) Token: 0x06008581 RID: 34177 RVA: 0x003A179F File Offset: 0x0039F99F
			// (set) Token: 0x06008582 RID: 34178 RVA: 0x003A17A7 File Offset: 0x0039F9A7
			[Serialize(0f, IsPropertySaveable.Yes, "How easily the character heals from the bleeding wounds. Default 0 (no extra healing).", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
			public float BleedingReduction { get; set; }

			// Token: 0x17001B53 RID: 6995
			// (get) Token: 0x06008583 RID: 34179 RVA: 0x003A17B0 File Offset: 0x0039F9B0
			// (set) Token: 0x06008584 RID: 34180 RVA: 0x003A17B8 File Offset: 0x0039F9B8
			[Serialize(0f, IsPropertySaveable.Yes, "How easily the character heals from the burn wounds. Default 0 (no extra healing).", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
			public float BurnReduction { get; set; }

			// Token: 0x17001B54 RID: 6996
			// (get) Token: 0x06008585 RID: 34181 RVA: 0x003A17C1 File Offset: 0x0039F9C1
			// (set) Token: 0x06008586 RID: 34182 RVA: 0x003A17C9 File Offset: 0x0039F9C9
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, DecimalCount = 2)]
			public float ConstantHealthRegeneration { get; set; }

			// Token: 0x17001B55 RID: 6997
			// (get) Token: 0x06008587 RID: 34183 RVA: 0x003A17D2 File Offset: 0x0039F9D2
			// (set) Token: 0x06008588 RID: 34184 RVA: 0x003A17DA File Offset: 0x0039F9DA
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 2)]
			public float HealthRegenerationWhenEating { get; set; }

			// Token: 0x17001B56 RID: 6998
			// (get) Token: 0x06008589 RID: 34185 RVA: 0x003A17E3 File Offset: 0x0039F9E3
			// (set) Token: 0x0600858A RID: 34186 RVA: 0x003A17EB File Offset: 0x0039F9EB
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool StunImmunity { get; set; }

			// Token: 0x17001B57 RID: 6999
			// (get) Token: 0x0600858B RID: 34187 RVA: 0x003A17F4 File Offset: 0x0039F9F4
			// (set) Token: 0x0600858C RID: 34188 RVA: 0x003A17FC File Offset: 0x0039F9FC
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool PoisonImmunity { get; set; }

			// Token: 0x17001B58 RID: 7000
			// (get) Token: 0x0600858D RID: 34189 RVA: 0x003A1805 File Offset: 0x0039FA05
			// (set) Token: 0x0600858E RID: 34190 RVA: 0x003A180D File Offset: 0x0039FA0D
			[Serialize(1f, IsPropertySaveable.Yes, "1 = default, 0 = immune.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 1)]
			public float PoisonVulnerability { get; set; }

			// Token: 0x17001B59 RID: 7001
			// (get) Token: 0x0600858F RID: 34191 RVA: 0x003A1816 File Offset: 0x0039FA16
			// (set) Token: 0x06008590 RID: 34192 RVA: 0x003A181E File Offset: 0x0039FA1E
			[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public float EmpVulnerability { get; set; }

			// Token: 0x17001B5A RID: 7002
			// (get) Token: 0x06008591 RID: 34193 RVA: 0x003A1827 File Offset: 0x0039FA27
			// (set) Token: 0x06008592 RID: 34194 RVA: 0x003A182F File Offset: 0x0039FA2F
			[Serialize(true, IsPropertySaveable.Yes, "Apply movement penalties when legs or tail limbs get damaged. Enabled by default.", "", false)]
			[Editable]
			public bool ApplyMovementPenalties { get; set; }

			// Token: 0x17001B5B RID: 7003
			// (get) Token: 0x06008593 RID: 34195 RVA: 0x003A1838 File Offset: 0x0039FA38
			// (set) Token: 0x06008594 RID: 34196 RVA: 0x003A1840 File Offset: 0x0039FA40
			[Serialize(true, IsPropertySaveable.Yes, "Normally characters die when they don't have a head. But maybe not all of them?", "", false)]
			[Editable]
			public bool DieFromBeheading { get; set; }

			// Token: 0x17001B5C RID: 7004
			// (get) Token: 0x06008595 RID: 34197 RVA: 0x003A1849 File Offset: 0x0039FA49
			// (set) Token: 0x06008596 RID: 34198 RVA: 0x003A1851 File Offset: 0x0039FA51
			[Serialize(false, IsPropertySaveable.Yes, "Severing legs doesn't work with most characters, because we'd need to take that into account with the walking animations and the standing position of the main collider etc. But there might be cases where you'll want to override this default.", "", false)]
			[Editable]
			public bool AllowSeveringLegs { get; set; }

			// Token: 0x17001B5D RID: 7005
			// (get) Token: 0x06008597 RID: 34199 RVA: 0x003A185A File Offset: 0x0039FA5A
			// (set) Token: 0x06008598 RID: 34200 RVA: 0x003A1862 File Offset: 0x0039FA62
			[Serialize(false, IsPropertySaveable.Yes, "Can afflictions affect the face/body tint of the character.", "", false)]
			[Editable]
			public bool ApplyAfflictionColors { get; private set; }

			// Token: 0x17001B5E RID: 7006
			// (get) Token: 0x06008599 RID: 34201 RVA: 0x003A186B File Offset: 0x0039FA6B
			// (set) Token: 0x0600859A RID: 34202 RVA: 0x003A1873 File Offset: 0x0039FA73
			[Serialize("", IsPropertySaveable.Yes, "A comma-separated list of identifiers of afflictions that the creature is immune to.", "", false)]
			[Editable]
			public string Immunities { get; private set; }

			// Token: 0x17001B5F RID: 7007
			// (get) Token: 0x0600859B RID: 34203 RVA: 0x003A187C File Offset: 0x0039FA7C
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

			// Token: 0x0600859C RID: 34204 RVA: 0x003A18B0 File Offset: 0x0039FAB0
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

			// Token: 0x0400534E RID: 21326
			private ImmutableHashSet<Identifier> _immunityIdentifiers;
		}

		// Token: 0x02000EBD RID: 3773
		public class InventoryParams : CharacterParams.SubParam
		{
			// Token: 0x17001B60 RID: 7008
			// (get) Token: 0x0600859D RID: 34205 RVA: 0x003A1985 File Offset: 0x0039FB85
			public override string Name
			{
				get
				{
					return "Inventory";
				}
			}

			// Token: 0x17001B61 RID: 7009
			// (get) Token: 0x0600859E RID: 34206 RVA: 0x003A198C File Offset: 0x0039FB8C
			// (set) Token: 0x0600859F RID: 34207 RVA: 0x003A1994 File Offset: 0x0039FB94
			[Serialize("Any, Any", IsPropertySaveable.Yes, "Which slots the inventory holds? Accepted types: None, Any, RightHand, LeftHand, Head, InnerClothes, OuterClothes, Headset, and Card.", "", false)]
			[Editable]
			public string Slots { get; private set; }

			// Token: 0x17001B62 RID: 7010
			// (get) Token: 0x060085A0 RID: 34208 RVA: 0x003A199D File Offset: 0x0039FB9D
			// (set) Token: 0x060085A1 RID: 34209 RVA: 0x003A19A5 File Offset: 0x0039FBA5
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool AccessibleWhenAlive { get; private set; }

			// Token: 0x17001B63 RID: 7011
			// (get) Token: 0x060085A2 RID: 34210 RVA: 0x003A19AE File Offset: 0x0039FBAE
			// (set) Token: 0x060085A3 RID: 34211 RVA: 0x003A19B6 File Offset: 0x0039FBB6
			[Serialize(1f, IsPropertySaveable.Yes, "What are the odds that this inventory is spawned on the character?", "", false)]
			[Editable(0f, 1f, 1)]
			public float Commonness { get; private set; }

			// Token: 0x17001B64 RID: 7012
			// (get) Token: 0x060085A4 RID: 34212 RVA: 0x003A19BF File Offset: 0x0039FBBF
			// (set) Token: 0x060085A5 RID: 34213 RVA: 0x003A19C7 File Offset: 0x0039FBC7
			public List<CharacterParams.InventoryParams.InventoryItem> Items { get; private set; } = new List<CharacterParams.InventoryParams.InventoryItem>();

			// Token: 0x060085A6 RID: 34214 RVA: 0x003A19D0 File Offset: 0x0039FBD0
			public InventoryParams(ContentXElement element, CharacterParams character) : base(element, character)
			{
				foreach (ContentXElement itemElement in element.GetChildElements("item"))
				{
					CharacterParams.InventoryParams.InventoryItem item = new CharacterParams.InventoryParams.InventoryItem(itemElement, character);
					base.SubParams.Add(item);
					this.Items.Add(item);
				}
			}

			// Token: 0x060085A7 RID: 34215 RVA: 0x003A1A50 File Offset: 0x0039FC50
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

			// Token: 0x060085A8 RID: 34216 RVA: 0x003A1AD2 File Offset: 0x0039FCD2
			public bool RemoveItem(CharacterParams.InventoryParams.InventoryItem item)
			{
				return base.RemoveSubParam<CharacterParams.InventoryParams.InventoryItem>(item, this.Items);
			}

			// Token: 0x02001555 RID: 5461
			public class InventoryItem : CharacterParams.SubParam
			{
				// Token: 0x17001D9C RID: 7580
				// (get) Token: 0x06009D80 RID: 40320 RVA: 0x003ED23E File Offset: 0x003EB43E
				public override string Name
				{
					get
					{
						return "Item";
					}
				}

				// Token: 0x17001D9D RID: 7581
				// (get) Token: 0x06009D81 RID: 40321 RVA: 0x003ED245 File Offset: 0x003EB445
				// (set) Token: 0x06009D82 RID: 40322 RVA: 0x003ED24D File Offset: 0x003EB44D
				[Serialize("", IsPropertySaveable.Yes, "Item identifier.", "", false)]
				[Editable]
				public string Identifier { get; private set; }

				// Token: 0x06009D83 RID: 40323 RVA: 0x003ED256 File Offset: 0x003EB456
				public InventoryItem(ContentXElement element, CharacterParams character) : base(element, character)
				{
				}
			}
		}

		// Token: 0x02000EBE RID: 3774
		public class AIParams : CharacterParams.SubParam
		{
			// Token: 0x17001B65 RID: 7013
			// (get) Token: 0x060085A9 RID: 34217 RVA: 0x003A1AE1 File Offset: 0x0039FCE1
			public override string Name
			{
				get
				{
					return "AI";
				}
			}

			// Token: 0x17001B66 RID: 7014
			// (get) Token: 0x060085AA RID: 34218 RVA: 0x003A1AE8 File Offset: 0x0039FCE8
			// (set) Token: 0x060085AB RID: 34219 RVA: 0x003A1AF0 File Offset: 0x0039FCF0
			[Serialize(1f, IsPropertySaveable.Yes, "How strong other characters think this character is? Only affects AI.", "", false)]
			[Editable]
			public float CombatStrength { get; private set; }

			// Token: 0x17001B67 RID: 7015
			// (get) Token: 0x060085AC RID: 34220 RVA: 0x003A1AF9 File Offset: 0x0039FCF9
			// (set) Token: 0x060085AD RID: 34221 RVA: 0x003A1B01 File Offset: 0x0039FD01
			[Serialize(1f, IsPropertySaveable.Yes, "Affects how far the character can see the targets. Used as a multiplier.", "", false)]
			[Editable(0f, 10f, 1)]
			public float Sight { get; private set; }

			// Token: 0x17001B68 RID: 7016
			// (get) Token: 0x060085AE RID: 34222 RVA: 0x003A1B0A File Offset: 0x0039FD0A
			// (set) Token: 0x060085AF RID: 34223 RVA: 0x003A1B12 File Offset: 0x0039FD12
			[Serialize(1f, IsPropertySaveable.Yes, "Affects how far the character can hear the targets. Used as a multiplier.", "", false)]
			[Editable(0f, 10f, 1)]
			public float Hearing { get; private set; }

			// Token: 0x17001B69 RID: 7017
			// (get) Token: 0x060085B0 RID: 34224 RVA: 0x003A1B1B File Offset: 0x0039FD1B
			// (set) Token: 0x060085B1 RID: 34225 RVA: 0x003A1B23 File Offset: 0x0039FD23
			[Serialize(-1f, IsPropertySaveable.Yes, "Hard limit to how far the character can spot targets from, regardless of the sight/hearing or how visible or how much noise the target is making. Not used if set to negative.", "", false)]
			[Editable]
			public float MaxPerceptionDistance { get; set; }

			// Token: 0x17001B6A RID: 7018
			// (get) Token: 0x060085B2 RID: 34226 RVA: 0x003A1B2C File Offset: 0x0039FD2C
			// (set) Token: 0x060085B3 RID: 34227 RVA: 0x003A1B34 File Offset: 0x0039FD34
			[Serialize(100f, IsPropertySaveable.Yes, "How much the targeting priority increases each time the character takes damage. Works like the greed value, described above. The default value is 100.", "", false)]
			[Editable(-1000f, 1000f, 1)]
			public float AggressionHurt { get; private set; }

			// Token: 0x17001B6B RID: 7019
			// (get) Token: 0x060085B4 RID: 34228 RVA: 0x003A1B3D File Offset: 0x0039FD3D
			// (set) Token: 0x060085B5 RID: 34229 RVA: 0x003A1B45 File Offset: 0x0039FD45
			[Serialize(10f, IsPropertySaveable.Yes, "How much the targeting priority increases each time the character does damage to the target. The actual priority adjustment is calculated based on the damage percentage multiplied by the greed value. The default value is 10, which means the priority will increase by 1 every time the character does damage 10% of the target's current health. If the damage is 50%, then the priority increase is 5.", "", false)]
			[Editable(0f, 1000f, 1)]
			public float AggressionGreed { get; private set; }

			// Token: 0x17001B6C RID: 7020
			// (get) Token: 0x060085B6 RID: 34230 RVA: 0x003A1B4E File Offset: 0x0039FD4E
			// (set) Token: 0x060085B7 RID: 34231 RVA: 0x003A1B56 File Offset: 0x0039FD56
			[Serialize(0f, IsPropertySaveable.Yes, "If the health drops below this threshold, the character flees. In percentages.", "", false)]
			[Editable(0f, 100f, 1)]
			public float FleeHealthThreshold { get; set; }

			// Token: 0x17001B6D RID: 7021
			// (get) Token: 0x060085B8 RID: 34232 RVA: 0x003A1B5F File Offset: 0x0039FD5F
			// (set) Token: 0x060085B9 RID: 34233 RVA: 0x003A1B67 File Offset: 0x0039FD67
			[Serialize(false, IsPropertySaveable.Yes, "Does the character attack when provoked? When enabled, overrides the predefined targeting state with Attack and increases the priority of it.", "", false)]
			[Editable]
			public bool AttackWhenProvoked { get; private set; }

			// Token: 0x17001B6E RID: 7022
			// (get) Token: 0x060085BA RID: 34234 RVA: 0x003A1B70 File Offset: 0x0039FD70
			// (set) Token: 0x060085BB RID: 34235 RVA: 0x003A1B78 File Offset: 0x0039FD78
			[Serialize(false, IsPropertySaveable.Yes, "The character will flee for a brief moment when being shot at if not performing an attack.", "", false)]
			[Editable]
			public bool AvoidGunfire { get; private set; }

			// Token: 0x17001B6F RID: 7023
			// (get) Token: 0x060085BC RID: 34236 RVA: 0x003A1B81 File Offset: 0x0039FD81
			// (set) Token: 0x060085BD RID: 34237 RVA: 0x003A1B89 File Offset: 0x0039FD89
			[Serialize(0f, IsPropertySaveable.Yes, "How much damage is required for single attack to trigger avoiding/releasing targets.", "", false)]
			[Editable(0f, 1000f, 1)]
			public float DamageThreshold { get; private set; }

			// Token: 0x17001B70 RID: 7024
			// (get) Token: 0x060085BE RID: 34238 RVA: 0x003A1B92 File Offset: 0x0039FD92
			// (set) Token: 0x060085BF RID: 34239 RVA: 0x003A1B9A File Offset: 0x0039FD9A
			[Serialize(3f, IsPropertySaveable.Yes, "How long the creature avoids gunfire. Also used when the creature is unlatched.", "", false)]
			[Editable(0f, 100f, 1)]
			public float AvoidTime { get; private set; }

			// Token: 0x17001B71 RID: 7025
			// (get) Token: 0x060085C0 RID: 34240 RVA: 0x003A1BA3 File Offset: 0x0039FDA3
			// (set) Token: 0x060085C1 RID: 34241 RVA: 0x003A1BAB File Offset: 0x0039FDAB
			[Serialize(20f, IsPropertySaveable.Yes, "How long the creature flees before returning to normal state. When the creature sees the target or is being chased, it will always flee, if it's in the flee state.", "", false)]
			[Editable(0f, 100f, 1)]
			public float MinFleeTime { get; private set; }

			// Token: 0x17001B72 RID: 7026
			// (get) Token: 0x060085C2 RID: 34242 RVA: 0x003A1BB4 File Offset: 0x0039FDB4
			// (set) Token: 0x060085C3 RID: 34243 RVA: 0x003A1BBC File Offset: 0x0039FDBC
			[Serialize(false, IsPropertySaveable.Yes, "Does the character try to break inside the sub?", "", false)]
			[Editable]
			public bool AggressiveBoarding { get; private set; }

			// Token: 0x17001B73 RID: 7027
			// (get) Token: 0x060085C4 RID: 34244 RVA: 0x003A1BC5 File Offset: 0x0039FDC5
			// (set) Token: 0x060085C5 RID: 34245 RVA: 0x003A1BCD File Offset: 0x0039FDCD
			[Serialize(true, IsPropertySaveable.Yes, "Enforce aggressive behavior if the creature is spawned as a target of a monster mission.", "", false)]
			[Editable]
			public bool EnforceAggressiveBehaviorForMissions { get; private set; }

			// Token: 0x17001B74 RID: 7028
			// (get) Token: 0x060085C6 RID: 34246 RVA: 0x003A1BD6 File Offset: 0x0039FDD6
			// (set) Token: 0x060085C7 RID: 34247 RVA: 0x003A1BDE File Offset: 0x0039FDDE
			[Serialize(true, IsPropertySaveable.Yes, "Should the character target or ignore walls when it's outside the submarine.", "", false)]
			[Editable]
			public bool TargetOuterWalls { get; private set; }

			// Token: 0x17001B75 RID: 7029
			// (get) Token: 0x060085C8 RID: 34248 RVA: 0x003A1BE7 File Offset: 0x0039FDE7
			// (set) Token: 0x060085C9 RID: 34249 RVA: 0x003A1BEF File Offset: 0x0039FDEF
			[Serialize(false, IsPropertySaveable.Yes, "If disabled (default), the character selects the limb based on a formula where the parameters are a) the priority of the attack b) the distance to the target, and c) the range of the attackIf enabled, the character chooses randomly from the available attacks. The priority is used as a weight for weighted random. The distance to the target is in this case ignored.", "", false)]
			[Editable]
			public bool RandomAttack { get; private set; }

			// Token: 0x17001B76 RID: 7030
			// (get) Token: 0x060085CA RID: 34250 RVA: 0x003A1BF8 File Offset: 0x0039FDF8
			// (set) Token: 0x060085CB RID: 34251 RVA: 0x003A1C00 File Offset: 0x0039FE00
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature know how to open doors (still requires a proper ID card). Humans can always open doors (They don't use this AI definition).", "", false)]
			[Editable]
			public bool CanOpenDoors { get; private set; }

			// Token: 0x17001B77 RID: 7031
			// (get) Token: 0x060085CC RID: 34252 RVA: 0x003A1C09 File Offset: 0x0039FE09
			// (set) Token: 0x060085CD RID: 34253 RVA: 0x003A1C11 File Offset: 0x0039FE11
			[Serialize(false, IsPropertySaveable.Yes, "Unlike human AI, monsters normally only use pathfinding when they are inside the submarine. When this is enabled, the monsters can also use pathfinding to get inside the sub. In practice, via doors and hatches.", "", false)]
			[Editable]
			public bool UsePathFindingToGetInside { get; set; }

			// Token: 0x17001B78 RID: 7032
			// (get) Token: 0x060085CE RID: 34254 RVA: 0x003A1C1A File Offset: 0x0039FE1A
			// (set) Token: 0x060085CF RID: 34255 RVA: 0x003A1C22 File Offset: 0x0039FE22
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature close the doors behind it. Humans don't use this AI definition.", "", false)]
			[Editable]
			public bool KeepDoorsClosed { get; private set; }

			// Token: 0x17001B79 RID: 7033
			// (get) Token: 0x060085D0 RID: 34256 RVA: 0x003A1C2B File Offset: 0x0039FE2B
			// (set) Token: 0x060085D1 RID: 34257 RVA: 0x003A1C33 File Offset: 0x0039FE33
			[Serialize(true, IsPropertySaveable.Yes, "Is the creature allowed to navigate from and into the depths of the abyss? When enabled, the creatures will try to avoid the depths.", "", false)]
			[Editable]
			public bool AvoidAbyss { get; set; }

			// Token: 0x17001B7A RID: 7034
			// (get) Token: 0x060085D2 RID: 34258 RVA: 0x003A1C3C File Offset: 0x0039FE3C
			// (set) Token: 0x060085D3 RID: 34259 RVA: 0x003A1C44 File Offset: 0x0039FE44
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature try to keep in the abyss? Has effect only when AvoidAbyss is false.", "", false)]
			[Editable]
			public bool StayInAbyss { get; set; }

			// Token: 0x17001B7B RID: 7035
			// (get) Token: 0x060085D4 RID: 34260 RVA: 0x003A1C4D File Offset: 0x0039FE4D
			// (set) Token: 0x060085D5 RID: 34261 RVA: 0x003A1C55 File Offset: 0x0039FE55
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature patrol the flooded hulls while idling inside a friendly submarine?", "", false)]
			[Editable]
			public bool PatrolFlooded { get; set; }

			// Token: 0x17001B7C RID: 7036
			// (get) Token: 0x060085D6 RID: 34262 RVA: 0x003A1C5E File Offset: 0x0039FE5E
			// (set) Token: 0x060085D7 RID: 34263 RVA: 0x003A1C66 File Offset: 0x0039FE66
			[Serialize(false, IsPropertySaveable.Yes, "Does the creature patrol the dry hulls while idling inside a friendly submarine?", "", false)]
			[Editable]
			public bool PatrolDry { get; set; }

			// Token: 0x17001B7D RID: 7037
			// (get) Token: 0x060085D8 RID: 34264 RVA: 0x003A1C6F File Offset: 0x0039FE6F
			// (set) Token: 0x060085D9 RID: 34265 RVA: 0x003A1C77 File Offset: 0x0039FE77
			[Serialize(0f, IsPropertySaveable.Yes, "Initial aggression used in the circle attack pattern (0-100). The aggression affects how close and how fast to the target the monster circles.", "", false)]
			[Editable]
			public float StartAggression { get; private set; }

			// Token: 0x17001B7E RID: 7038
			// (get) Token: 0x060085DA RID: 34266 RVA: 0x003A1C80 File Offset: 0x0039FE80
			// (set) Token: 0x060085DB RID: 34267 RVA: 0x003A1C88 File Offset: 0x0039FE88
			[Serialize(100f, IsPropertySaveable.Yes, "Maximum aggression used in the circle attack pattern (0-100). The aggression affects how close and how fast to the target the monster circles.", "", false)]
			[Editable]
			public float MaxAggression { get; private set; }

			// Token: 0x17001B7F RID: 7039
			// (get) Token: 0x060085DC RID: 34268 RVA: 0x003A1C91 File Offset: 0x0039FE91
			// (set) Token: 0x060085DD RID: 34269 RVA: 0x003A1C99 File Offset: 0x0039FE99
			[Serialize(0f, IsPropertySaveable.Yes, "How quickly the aggression level increases from StartAggression to MaxAggression when using the circle attack pattern. Artificial amount, applied once per attack cycle.", "", false)]
			[Editable]
			public float AggressionCumulation { get; private set; }

			// Token: 0x17001B80 RID: 7040
			// (get) Token: 0x060085DE RID: 34270 RVA: 0x003A1CA2 File Offset: 0x0039FEA2
			// (set) Token: 0x060085DF RID: 34271 RVA: 0x003A1CAA File Offset: 0x0039FEAA
			[Serialize(WallTargetingMethod.Target, IsPropertySaveable.Yes, "Defines the method of checking whether there's a blocking (submarine) wall.", "", false)]
			[Editable]
			public WallTargetingMethod WallTargetingMethod { get; private set; }

			// Token: 0x17001B81 RID: 7041
			// (get) Token: 0x060085E0 RID: 34272 RVA: 0x003A1CB3 File Offset: 0x0039FEB3
			// (set) Token: 0x060085E1 RID: 34273 RVA: 0x003A1CBB File Offset: 0x0039FEBB
			[Serialize(0f, IsPropertySaveable.Yes, "How likely it is that the creature plays dead (= ragdolls) while idling? Only allowed inside a sub (not in the open waters). Evaluated once, when the creature spawns.", "", false)]
			[Editable]
			public float PlayDeadProbability { get; set; }

			// Token: 0x17001B82 RID: 7042
			// (get) Token: 0x060085E2 RID: 34274 RVA: 0x003A1CC4 File Offset: 0x0039FEC4
			public IEnumerable<CharacterParams.TargetParams> Targets
			{
				get
				{
					return this.targets;
				}
			}

			// Token: 0x060085E3 RID: 34275 RVA: 0x003A1CCC File Offset: 0x0039FECC
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

			// Token: 0x060085E4 RID: 34276 RVA: 0x003A1D50 File Offset: 0x0039FF50
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

			// Token: 0x060085E5 RID: 34277 RVA: 0x003A1DA4 File Offset: 0x0039FFA4
			private CharacterParams.TargetParams AddTarget(ContentXElement targetElement)
			{
				CharacterParams.TargetParams target = new CharacterParams.TargetParams(targetElement, base.Character);
				this.targets.Add(target);
				base.SubParams.Add(target);
				return target;
			}

			// Token: 0x060085E6 RID: 34278 RVA: 0x003A1DD8 File Offset: 0x0039FFD8
			public bool TryAddEmptyTarget(out CharacterParams.TargetParams targetParams)
			{
				return this.TryAddNewTarget("newtarget" + this.targets.Count.ToString(), AIState.Attack, 0f, out targetParams);
			}

			// Token: 0x060085E7 RID: 34279 RVA: 0x003A1E0F File Offset: 0x003A000F
			public bool TryAddNewTarget(string tag, AIState state, float priority, out CharacterParams.TargetParams targetParams)
			{
				return this.TryAddNewTarget(tag.ToIdentifier(), state, priority, out targetParams);
			}

			// Token: 0x060085E8 RID: 34280 RVA: 0x003A1E24 File Offset: 0x003A0024
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

			// Token: 0x060085E9 RID: 34281 RVA: 0x003A1E74 File Offset: 0x003A0074
			public bool HasTag(string tag)
			{
				return this.HasTag(tag.ToIdentifier());
			}

			// Token: 0x060085EA RID: 34282 RVA: 0x003A1E84 File Offset: 0x003A0084
			public bool HasTag(Identifier tag)
			{
				return !(tag == null) && this.targets.Any(delegate(CharacterParams.TargetParams t)
				{
					Identifier tag2 = t.Tag;
					return tag2 == tag;
				});
			}

			// Token: 0x060085EB RID: 34283 RVA: 0x003A1EC5 File Offset: 0x003A00C5
			public bool RemoveTarget(CharacterParams.TargetParams target)
			{
				return base.RemoveSubParam<CharacterParams.TargetParams>(target, this.targets);
			}

			// Token: 0x060085EC RID: 34284 RVA: 0x003A1ED4 File Offset: 0x003A00D4
			public IEnumerable<CharacterParams.TargetParams> GetMatchingTargets(Func<CharacterParams.TargetParams, bool> predicate)
			{
				return this.targets.Where(predicate);
			}

			// Token: 0x060085ED RID: 34285 RVA: 0x003A1EE4 File Offset: 0x003A00E4
			public IEnumerable<CharacterParams.TargetParams> GetTargets(Identifier target)
			{
				return this.GetMatchingTargets(delegate(CharacterParams.TargetParams t)
				{
					Identifier tag = t.Tag;
					return tag == target;
				});
			}

			// Token: 0x060085EE RID: 34286 RVA: 0x003A1F10 File Offset: 0x003A0110
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

			// Token: 0x060085EF RID: 34287 RVA: 0x003A1F3C File Offset: 0x003A013C
			public CharacterParams.TargetParams GetHighestPriorityTarget(Identifier target)
			{
				return CharacterParams.AIParams.GetHighestPriorityTarget(this.GetTargets(target));
			}

			// Token: 0x060085F0 RID: 34288 RVA: 0x003A1F4A File Offset: 0x003A014A
			public CharacterParams.TargetParams GetHighestPriorityTarget(Character target)
			{
				return CharacterParams.AIParams.GetHighestPriorityTarget(this.GetTargets(target));
			}

			// Token: 0x060085F1 RID: 34289 RVA: 0x003A1F58 File Offset: 0x003A0158
			private static CharacterParams.TargetParams GetHighestPriorityTarget(IEnumerable<CharacterParams.TargetParams> targetParams)
			{
				return targetParams.MaxBy((CharacterParams.TargetParams t) => t.Priority);
			}

			// Token: 0x060085F2 RID: 34290 RVA: 0x003A1F7F File Offset: 0x003A017F
			public bool TryGetTargets(Identifier target, out IEnumerable<CharacterParams.TargetParams> targetParams)
			{
				targetParams = this.GetTargets(target);
				return targetParams.Any<CharacterParams.TargetParams>();
			}

			// Token: 0x060085F3 RID: 34291 RVA: 0x003A1F91 File Offset: 0x003A0191
			public bool TryGetTargets(Character target, out IEnumerable<CharacterParams.TargetParams> targetParams)
			{
				targetParams = this.GetTargets(target);
				return targetParams.Any<CharacterParams.TargetParams>();
			}

			// Token: 0x060085F4 RID: 34292 RVA: 0x003A1FA3 File Offset: 0x003A01A3
			public bool TryGetHighestPriorityTarget(Identifier target, out CharacterParams.TargetParams targetParams)
			{
				targetParams = this.GetHighestPriorityTarget(target);
				return targetParams != null;
			}

			// Token: 0x060085F5 RID: 34293 RVA: 0x003A1FB3 File Offset: 0x003A01B3
			public bool TryGetHighestPriorityTarget(Character target, out CharacterParams.TargetParams targetParams)
			{
				targetParams = this.GetHighestPriorityTarget(target);
				return targetParams != null;
			}

			// Token: 0x060085F6 RID: 34294 RVA: 0x003A1FC4 File Offset: 0x003A01C4
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

			// Token: 0x0400536F RID: 21359
			public readonly bool IsPet;

			// Token: 0x04005370 RID: 21360
			private readonly List<CharacterParams.TargetParams> targets = new List<CharacterParams.TargetParams>();
		}

		// Token: 0x02000EBF RID: 3775
		public class TargetParams : CharacterParams.SubParam
		{
			// Token: 0x17001B83 RID: 7043
			// (get) Token: 0x060085F9 RID: 34297 RVA: 0x003A2080 File Offset: 0x003A0280
			public override string Name
			{
				get
				{
					return "Target";
				}
			}

			// Token: 0x17001B84 RID: 7044
			// (get) Token: 0x060085FA RID: 34298 RVA: 0x003A2087 File Offset: 0x003A0287
			// (set) Token: 0x060085FB RID: 34299 RVA: 0x003A208F File Offset: 0x003A028F
			[Serialize("", IsPropertySaveable.Yes, "Can be an item tag, species name or something else. Examples: decoy, provocative, light, dead, human, crawler, wall, nasonov, sonar, door, stronger, weaker, light, human, room...", "", false)]
			[Editable]
			public Identifier Tag { get; private set; }

			// Token: 0x17001B85 RID: 7045
			// (get) Token: 0x060085FC RID: 34300 RVA: 0x003A2098 File Offset: 0x003A0298
			// (set) Token: 0x060085FD RID: 34301 RVA: 0x003A20A0 File Offset: 0x003A02A0
			[Serialize(AIState.Idle, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public AIState State { get; set; }

			// Token: 0x17001B86 RID: 7046
			// (get) Token: 0x060085FE RID: 34302 RVA: 0x003A20A9 File Offset: 0x003A02A9
			// (set) Token: 0x060085FF RID: 34303 RVA: 0x003A20B1 File Offset: 0x003A02B1
			[Serialize(0f, IsPropertySaveable.Yes, "What base priority is given to the target?", "", false)]
			[Editable(0f, 1000f, 1, ValueStep = 1f, DecimalCount = 0)]
			public float Priority { get; set; }

			// Token: 0x17001B87 RID: 7047
			// (get) Token: 0x06008600 RID: 34304 RVA: 0x003A20BA File Offset: 0x003A02BA
			// (set) Token: 0x06008601 RID: 34305 RVA: 0x003A20C2 File Offset: 0x003A02C2
			[Serialize(0f, IsPropertySaveable.Yes, "Generic distance that can be used for different purposes depending on the state. E.g. in Avoid state this defines the distance that the character tries to keep to the target. If the distance is 0, it's not used.", "", false)]
			[Editable(MinValueFloat = 0f, ValueStep = 10f, DecimalCount = 0)]
			public float ReactDistance { get; set; }

			// Token: 0x17001B88 RID: 7048
			// (get) Token: 0x06008602 RID: 34306 RVA: 0x003A20CB File Offset: 0x003A02CB
			// (set) Token: 0x06008603 RID: 34307 RVA: 0x003A20D3 File Offset: 0x003A02D3
			[Serialize(0f, IsPropertySaveable.Yes, "Used for defining the attack distance for PassiveAggressive and Aggressive states. If the distance is 0, it's not used.", "", false)]
			[Editable(MinValueFloat = 0f, ValueStep = 10f, DecimalCount = 0)]
			public float AttackDistance { get; set; }

			// Token: 0x17001B89 RID: 7049
			// (get) Token: 0x06008604 RID: 34308 RVA: 0x003A20DC File Offset: 0x003A02DC
			// (set) Token: 0x06008605 RID: 34309 RVA: 0x003A20E4 File Offset: 0x003A02E4
			[Serialize(0f, IsPropertySaveable.Yes, "Generic timer that can be used for different purposes depending on the state. E.g. in Observe state this defines how long the character in general keeps staring the targets (Some random is always applied).", "", false)]
			[Editable]
			public float Timer { get; set; }

			// Token: 0x17001B8A RID: 7050
			// (get) Token: 0x06008606 RID: 34310 RVA: 0x003A20ED File Offset: 0x003A02ED
			// (set) Token: 0x06008607 RID: 34311 RVA: 0x003A20F5 File Offset: 0x003A02F5
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored if it's inside a container/inventory. Only affects items.", "", false)]
			[Editable]
			public bool IgnoreContained { get; set; }

			// Token: 0x17001B8B RID: 7051
			// (get) Token: 0x06008608 RID: 34312 RVA: 0x003A20FE File Offset: 0x003A02FE
			// (set) Token: 0x06008609 RID: 34313 RVA: 0x003A2106 File Offset: 0x003A0306
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored while the creature is inside. Doesn't matter where the target is.", "", false)]
			[Editable]
			public bool IgnoreInside { get; set; }

			// Token: 0x17001B8C RID: 7052
			// (get) Token: 0x0600860A RID: 34314 RVA: 0x003A210F File Offset: 0x003A030F
			// (set) Token: 0x0600860B RID: 34315 RVA: 0x003A2117 File Offset: 0x003A0317
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored while the creature is outside. Doesn't matter where the target is.", "", false)]
			[Editable]
			public bool IgnoreOutside { get; set; }

			// Token: 0x17001B8D RID: 7053
			// (get) Token: 0x0600860C RID: 34316 RVA: 0x003A2120 File Offset: 0x003A0320
			// (set) Token: 0x0600860D RID: 34317 RVA: 0x003A2128 File Offset: 0x003A0328
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored if it's inside. Doesn't matter where the creature itself is.", "", false)]
			[Editable]
			public bool IgnoreTargetInside { get; set; }

			// Token: 0x17001B8E RID: 7054
			// (get) Token: 0x0600860E RID: 34318 RVA: 0x003A2131 File Offset: 0x003A0331
			// (set) Token: 0x0600860F RID: 34319 RVA: 0x003A2139 File Offset: 0x003A0339
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored if it's outside. Doesn't matter where the creature itself is.", "", false)]
			[Editable]
			public bool IgnoreTargetOutside { get; set; }

			// Token: 0x17001B8F RID: 7055
			// (get) Token: 0x06008610 RID: 34320 RVA: 0x003A2142 File Offset: 0x003A0342
			// (set) Token: 0x06008611 RID: 34321 RVA: 0x003A214A File Offset: 0x003A034A
			[Serialize(false, IsPropertySaveable.Yes, "Should the target be ignored if it's inside a different submarine than us? Normally only some targets are ignored when they are not inside the same sub.", "", false)]
			[Editable]
			public bool IgnoreIfNotInSameSub { get; set; }

			// Token: 0x17001B90 RID: 7056
			// (get) Token: 0x06008612 RID: 34322 RVA: 0x003A2153 File Offset: 0x003A0353
			// (set) Token: 0x06008613 RID: 34323 RVA: 0x003A215B File Offset: 0x003A035B
			[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
			[Editable]
			public bool IgnoreIncapacitated { get; set; }

			// Token: 0x17001B91 RID: 7057
			// (get) Token: 0x06008614 RID: 34324 RVA: 0x003A2164 File Offset: 0x003A0364
			// (set) Token: 0x06008615 RID: 34325 RVA: 0x003A216C File Offset: 0x003A036C
			[Serialize(0f, IsPropertySaveable.Yes, "A generic threshold. For example, how much damage the protected target should take from an attacker before the creature starts defending it.", "", false)]
			[Editable]
			public float Threshold { get; private set; }

			// Token: 0x17001B92 RID: 7058
			// (get) Token: 0x06008616 RID: 34326 RVA: 0x003A2175 File Offset: 0x003A0375
			// (set) Token: 0x06008617 RID: 34327 RVA: 0x003A217D File Offset: 0x003A037D
			[Serialize(-1f, IsPropertySaveable.Yes, "A generic min threshold. Not used if set to negative.", "", false)]
			[Editable]
			public float ThresholdMin { get; private set; }

			// Token: 0x17001B93 RID: 7059
			// (get) Token: 0x06008618 RID: 34328 RVA: 0x003A2186 File Offset: 0x003A0386
			// (set) Token: 0x06008619 RID: 34329 RVA: 0x003A218E File Offset: 0x003A038E
			[Serialize(-1f, IsPropertySaveable.Yes, "A generic max threshold. Not used if set to negative.", "", false)]
			[Editable]
			public float ThresholdMax { get; private set; }

			// Token: 0x17001B94 RID: 7060
			// (get) Token: 0x0600861A RID: 34330 RVA: 0x003A2197 File Offset: 0x003A0397
			// (set) Token: 0x0600861B RID: 34331 RVA: 0x003A219F File Offset: 0x003A039F
			[Serialize(1f, IsPropertySaveable.Yes, "Can be used to make the monster perceive the target further or closer than it normally can.", "", false)]
			[Editable]
			public float PerceptionDistanceMultiplier { get; private set; }

			// Token: 0x17001B95 RID: 7061
			// (get) Token: 0x0600861C RID: 34332 RVA: 0x003A21A8 File Offset: 0x003A03A8
			// (set) Token: 0x0600861D RID: 34333 RVA: 0x003A21B0 File Offset: 0x003A03B0
			[Serialize(-1f, IsPropertySaveable.Yes, "Maximum distance at which the monster can perceive the target, regardless of the sight/hearing or how visible or how much noise the target is making. Not used if set to negative.", "", false)]
			[Editable]
			public float MaxPerceptionDistance { get; private set; }

			// Token: 0x17001B96 RID: 7062
			// (get) Token: 0x0600861E RID: 34334 RVA: 0x003A21B9 File Offset: 0x003A03B9
			// (set) Token: 0x0600861F RID: 34335 RVA: 0x003A21C1 File Offset: 0x003A03C1
			[Serialize("0.0, 0.0", IsPropertySaveable.Yes, "A generic offset. Used for example for offsetting the react distance (vector length) and for offsetting the target position when a guardian flees to a pod.", "", false)]
			[Editable]
			public Vector2 Offset { get; private set; }

			// Token: 0x17001B97 RID: 7063
			// (get) Token: 0x06008620 RID: 34336 RVA: 0x003A21CA File Offset: 0x003A03CA
			// (set) Token: 0x06008621 RID: 34337 RVA: 0x003A21D2 File Offset: 0x003A03D2
			[Serialize(AttackPattern.Straight, IsPropertySaveable.Yes, "Defines the movement pattern of the character when approaching a target.", "", false)]
			[Editable]
			public AttackPattern AttackPattern { get; set; }

			// Token: 0x17001B98 RID: 7064
			// (get) Token: 0x06008622 RID: 34338 RVA: 0x003A21DB File Offset: 0x003A03DB
			// (set) Token: 0x06008623 RID: 34339 RVA: 0x003A21E3 File Offset: 0x003A03E3
			[Serialize(false, IsPropertySaveable.Yes, "If enabled, the AI will give more priority to targets close to the horizontal middle of the sub. Only applies to walls, hulls, and items like sonar. Circle and Sweep always does this regardless of this property.", "", false)]
			[Editable]
			public bool PrioritizeSubCenter { get; set; }

			// Token: 0x17001B99 RID: 7065
			// (get) Token: 0x06008624 RID: 34340 RVA: 0x003A21EC File Offset: 0x003A03EC
			// (set) Token: 0x06008625 RID: 34341 RVA: 0x003A21F4 File Offset: 0x003A03F4
			[Serialize(0f, IsPropertySaveable.Yes, "Use to define a distance at which the creature starts the sweeping movement.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f, ValueStep = 1f, DecimalCount = 0)]
			public float SweepDistance { get; private set; }

			// Token: 0x17001B9A RID: 7066
			// (get) Token: 0x06008626 RID: 34342 RVA: 0x003A21FD File Offset: 0x003A03FD
			// (set) Token: 0x06008627 RID: 34343 RVA: 0x003A2205 File Offset: 0x003A0405
			[Serialize(10f, IsPropertySaveable.Yes, "How much the sweep affects the steering?", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, ValueStep = 1f, DecimalCount = 1)]
			public float SweepStrength { get; private set; }

			// Token: 0x17001B9B RID: 7067
			// (get) Token: 0x06008628 RID: 34344 RVA: 0x003A220E File Offset: 0x003A040E
			// (set) Token: 0x06008629 RID: 34345 RVA: 0x003A2216 File Offset: 0x003A0416
			[Serialize(1f, IsPropertySaveable.Yes, "How quickly the sweep direction changes. Uses the sine wave pattern.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10f, ValueStep = 0.1f, DecimalCount = 2)]
			public float SweepSpeed { get; private set; }

			// Token: 0x17001B9C RID: 7068
			// (get) Token: 0x0600862A RID: 34346 RVA: 0x003A221F File Offset: 0x003A041F
			// (set) Token: 0x0600862B RID: 34347 RVA: 0x003A2227 File Offset: 0x003A0427
			[Serialize(5000f, IsPropertySaveable.Yes, "How close to the target the character should be, before they start using the circle pattern instead of directional approaching.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 20000f)]
			public float CircleStartDistance { get; private set; }

			// Token: 0x17001B9D RID: 7069
			// (get) Token: 0x0600862C RID: 34348 RVA: 0x003A2230 File Offset: 0x003A0430
			// (set) Token: 0x0600862D RID: 34349 RVA: 0x003A2238 File Offset: 0x003A0438
			[Serialize(false, IsPropertySaveable.Yes, "Normally the target size is taken into account when calculating the distance to the target. Set this true to skip that.", "", false)]
			public bool IgnoreTargetSize { get; private set; }

			// Token: 0x17001B9E RID: 7070
			// (get) Token: 0x0600862E RID: 34350 RVA: 0x003A2241 File Offset: 0x003A0441
			// (set) Token: 0x0600862F RID: 34351 RVA: 0x003A2249 File Offset: 0x003A0449
			[Serialize(1f, IsPropertySaveable.Yes, "Determines the rate how quickly the target movement position is rotated towards the attack target. The actual rotation is calculated once per each attack cycle, based on the current aggression level.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
			public float CircleRotationSpeed { get; private set; }

			// Token: 0x17001B9F RID: 7071
			// (get) Token: 0x06008630 RID: 34352 RVA: 0x003A2252 File Offset: 0x003A0452
			// (set) Token: 0x06008631 RID: 34353 RVA: 0x003A225A File Offset: 0x003A045A
			[Serialize(false, IsPropertySaveable.Yes, "When enabled, the circle rotation speed can change when the target is far. When this setting is disabled (default), the character will head directly towards the target when it's too far.", "", false)]
			[Editable]
			public bool DynamicCircleRotationSpeed { get; private set; }

			// Token: 0x17001BA0 RID: 7072
			// (get) Token: 0x06008632 RID: 34354 RVA: 0x003A2263 File Offset: 0x003A0463
			// (set) Token: 0x06008633 RID: 34355 RVA: 0x003A226B File Offset: 0x003A046B
			[Serialize(0f, IsPropertySaveable.Yes, "How much the turn speed can differ between attack cycles (stays constant during the cycle)", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
			public float CircleRandomRotationFactor { get; private set; }

			// Token: 0x17001BA1 RID: 7073
			// (get) Token: 0x06008634 RID: 34356 RVA: 0x003A2274 File Offset: 0x003A0474
			// (set) Token: 0x06008635 RID: 34357 RVA: 0x003A227C File Offset: 0x003A047C
			[Serialize(5f, IsPropertySaveable.Yes, "Affects how close to the target the character has to be before the strike phase of the circle behavior triggers. In the strike phase, the creature moves directly towards the target.", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
			public float CircleStrikeDistanceMultiplier { get; private set; }

			// Token: 0x17001BA2 RID: 7074
			// (get) Token: 0x06008636 RID: 34358 RVA: 0x003A2285 File Offset: 0x003A0485
			// (set) Token: 0x06008637 RID: 34359 RVA: 0x003A228D File Offset: 0x003A048D
			[Serialize(0f, IsPropertySaveable.Yes, "How much the target position is offset at maximum. Low values make the character hit the target earlier/always, higher values make it miss the target when the aggression intensity is low (early in the encounter).", "", false)]
			[Editable(MinValueFloat = 0f, MaxValueFloat = 50f)]
			public float CircleMaxRandomOffset { get; private set; }

			// Token: 0x17001BA3 RID: 7075
			// (get) Token: 0x06008638 RID: 34360 RVA: 0x003A2296 File Offset: 0x003A0496
			// (set) Token: 0x06008639 RID: 34361 RVA: 0x003A229E File Offset: 0x003A049E
			public List<PropertyConditional> Conditionals { get; private set; } = new List<PropertyConditional>();

			// Token: 0x0600863A RID: 34362 RVA: 0x003A22A7 File Offset: 0x003A04A7
			public TargetParams(string tag, AIState state, float priority, CharacterParams character) : this(CharacterParams.TargetParams.CreateNewElement(character, tag, state, priority), character)
			{
			}

			// Token: 0x0600863B RID: 34363 RVA: 0x003A22BC File Offset: 0x003A04BC
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

			// Token: 0x0600863C RID: 34364 RVA: 0x003A2348 File Offset: 0x003A0548
			public static ContentXElement CreateNewElement(CharacterParams character, Identifier tag, AIState state, float priority)
			{
				return CharacterParams.TargetParams.CreateNewElement(character, tag.Value, state, priority);
			}

			// Token: 0x0600863D RID: 34365 RVA: 0x003A235C File Offset: 0x003A055C
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

		// Token: 0x02000EC0 RID: 3776
		public abstract class SubParam : ISerializableEntity
		{
			// Token: 0x17001BA4 RID: 7076
			// (get) Token: 0x0600863E RID: 34366 RVA: 0x003A23D1 File Offset: 0x003A05D1
			// (set) Token: 0x0600863F RID: 34367 RVA: 0x003A23D9 File Offset: 0x003A05D9
			public virtual string Name { get; set; }

			// Token: 0x17001BA5 RID: 7077
			// (get) Token: 0x06008640 RID: 34368 RVA: 0x003A23E2 File Offset: 0x003A05E2
			// (set) Token: 0x06008641 RID: 34369 RVA: 0x003A23EA File Offset: 0x003A05EA
			public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

			// Token: 0x17001BA6 RID: 7078
			// (get) Token: 0x06008642 RID: 34370 RVA: 0x003A23F3 File Offset: 0x003A05F3
			// (set) Token: 0x06008643 RID: 34371 RVA: 0x003A23FB File Offset: 0x003A05FB
			public ContentXElement Element { get; set; }

			// Token: 0x17001BA7 RID: 7079
			// (get) Token: 0x06008644 RID: 34372 RVA: 0x003A2404 File Offset: 0x003A0604
			// (set) Token: 0x06008645 RID: 34373 RVA: 0x003A240C File Offset: 0x003A060C
			public List<CharacterParams.SubParam> SubParams { get; set; } = new List<CharacterParams.SubParam>();

			// Token: 0x17001BA8 RID: 7080
			// (get) Token: 0x06008646 RID: 34374 RVA: 0x003A2415 File Offset: 0x003A0615
			// (set) Token: 0x06008647 RID: 34375 RVA: 0x003A241D File Offset: 0x003A061D
			public CharacterParams Character { get; private set; }

			// Token: 0x06008648 RID: 34376 RVA: 0x003A2426 File Offset: 0x003A0626
			protected ContentXElement CreateElement(string name, params object[] attrs)
			{
				return new XElement(name, attrs).FromPackage(this.Element.ContentPackage);
			}

			// Token: 0x06008649 RID: 34377 RVA: 0x003A2444 File Offset: 0x003A0644
			public SubParam(ContentXElement element, CharacterParams character)
			{
				this.Element = element;
				this.Character = character;
				this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			}

			// Token: 0x0600864A RID: 34378 RVA: 0x003A2478 File Offset: 0x003A0678
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

			// Token: 0x0600864B RID: 34379 RVA: 0x003A24D4 File Offset: 0x003A06D4
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

			// Token: 0x0600864C RID: 34380 RVA: 0x003A2522 File Offset: 0x003A0722
			public virtual void Reset()
			{
				this.Deserialize(false);
				this.SubParams.ForEach(delegate(CharacterParams.SubParam sp)
				{
					sp.Reset();
				});
			}

			// Token: 0x0600864D RID: 34381 RVA: 0x003A2558 File Offset: 0x003A0758
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

			// Token: 0x17001BA9 RID: 7081
			// (get) Token: 0x0600864E RID: 34382 RVA: 0x003A25F7 File Offset: 0x003A07F7
			// (set) Token: 0x0600864F RID: 34383 RVA: 0x003A25FF File Offset: 0x003A07FF
			public SerializableEntityEditor SerializableEntityEditor { get; protected set; }

			// Token: 0x06008650 RID: 34384 RVA: 0x003A2608 File Offset: 0x003A0808
			public virtual void AddToEditor(ParamsEditor editor, bool recursive = true, int space = 0, GUIFont titleFont = null)
			{
				this.SerializableEntityEditor = new SerializableEntityEditor(editor.EditorBox.Content.RectTransform, this, false, true, "", 24, titleFont ?? GUIStyle.LargeFont, true);
				if (recursive)
				{
					this.SubParams.ForEach(delegate(CharacterParams.SubParam sp)
					{
						sp.AddToEditor(editor, true, 0, titleFont ?? GUIStyle.SmallFont);
					});
				}
				if (space > 0)
				{
					new GUIFrame(new RectTransform(new Point(editor.EditorBox.Rect.Width, space), editor.EditorBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(new Color(20, 20, 20, 255))).CanBeFocused = false;
				}
			}
		}
	}
}
