using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Lights;
using Barotrauma.Particles;
using Barotrauma.SpriteDeformations;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020000DD RID: 221
	internal class LevelObjectPrefab : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x1700081B RID: 2075
		// (get) Token: 0x06001E6F RID: 7791 RVA: 0x0012FA64 File Offset: 0x0012DC64
		// (set) Token: 0x06001E70 RID: 7792 RVA: 0x0012FA6C File Offset: 0x0012DC6C
		public List<int> ParticleEmitterTriggerIndex { get; private set; }

		// Token: 0x1700081C RID: 2076
		// (get) Token: 0x06001E71 RID: 7793 RVA: 0x0012FA75 File Offset: 0x0012DC75
		// (set) Token: 0x06001E72 RID: 7794 RVA: 0x0012FA7D File Offset: 0x0012DC7D
		public List<ParticleEmitterPrefab> ParticleEmitterPrefabs { get; private set; }

		// Token: 0x1700081D RID: 2077
		// (get) Token: 0x06001E73 RID: 7795 RVA: 0x0012FA86 File Offset: 0x0012DC86
		// (set) Token: 0x06001E74 RID: 7796 RVA: 0x0012FA8E File Offset: 0x0012DC8E
		public List<Vector2> EmitterPositions { get; private set; }

		// Token: 0x1700081E RID: 2078
		// (get) Token: 0x06001E75 RID: 7797 RVA: 0x0012FA97 File Offset: 0x0012DC97
		// (set) Token: 0x06001E76 RID: 7798 RVA: 0x0012FA9F File Offset: 0x0012DC9F
		public List<LevelObjectPrefab.SoundConfig> Sounds { get; private set; } = new List<LevelObjectPrefab.SoundConfig>();

		// Token: 0x1700081F RID: 2079
		// (get) Token: 0x06001E77 RID: 7799 RVA: 0x0012FAA8 File Offset: 0x0012DCA8
		// (set) Token: 0x06001E78 RID: 7800 RVA: 0x0012FAB0 File Offset: 0x0012DCB0
		public List<int> LightSourceTriggerIndex { get; private set; } = new List<int>();

		// Token: 0x17000820 RID: 2080
		// (get) Token: 0x06001E79 RID: 7801 RVA: 0x0012FAB9 File Offset: 0x0012DCB9
		// (set) Token: 0x06001E7A RID: 7802 RVA: 0x0012FAC1 File Offset: 0x0012DCC1
		public List<LightSourceParams> LightSourceParams { get; private set; } = new List<LightSourceParams>();

		// Token: 0x17000821 RID: 2081
		// (get) Token: 0x06001E7B RID: 7803 RVA: 0x0012FACA File Offset: 0x0012DCCA
		// (set) Token: 0x06001E7C RID: 7804 RVA: 0x0012FAD2 File Offset: 0x0012DCD2
		public List<SpriteDeformation> SpriteDeformations { get; private set; } = new List<SpriteDeformation>();

		// Token: 0x06001E7D RID: 7805 RVA: 0x0012FADC File Offset: 0x0012DCDC
		private void LoadElementsProjSpecific(ContentXElement element, int parentTriggerIndex)
		{
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "leveltrigger") && !(a == "trigger"))
				{
					if (!(a == "lightsource"))
					{
						if (!(a == "particleemitter"))
						{
							if (!(a == "sound"))
							{
								if (a == "deformablesprite")
								{
									foreach (ContentXElement cxe in subElement.Elements())
									{
										XElement deformElement = cxe;
										SpriteDeformation deformation = SpriteDeformation.Load(deformElement, this.Name);
										if (deformation != null)
										{
											this.SpriteDeformations.Add(deformation);
										}
									}
								}
							}
							else
							{
								this.Sounds.Add(new LevelObjectPrefab.SoundConfig(subElement, parentTriggerIndex));
							}
						}
						else
						{
							if (this.ParticleEmitterPrefabs == null)
							{
								this.ParticleEmitterPrefabs = new List<ParticleEmitterPrefab>();
								this.EmitterPositions = new List<Vector2>();
								this.ParticleEmitterTriggerIndex = new List<int>();
							}
							this.ParticleEmitterPrefabs.Add(new ParticleEmitterPrefab(subElement));
							this.ParticleEmitterTriggerIndex.Add(parentTriggerIndex);
							List<Vector2> emitterPositions = this.EmitterPositions;
							ContentXElement contentXElement = subElement;
							string key = "position";
							Vector2 zero = Vector2.Zero;
							emitterPositions.Add(contentXElement.GetAttributeVector2(key, zero));
						}
					}
					else
					{
						this.LightSourceTriggerIndex.Add(parentTriggerIndex);
						this.LightSourceParams.Add(new LightSourceParams(subElement));
					}
				}
				else
				{
					this.LoadElementsProjSpecific(subElement, this.LevelTriggerElements.IndexOf(subElement));
				}
			}
		}

		// Token: 0x06001E7E RID: 7806 RVA: 0x0012FCC0 File Offset: 0x0012DEC0
		public void Save(XElement element)
		{
			this.Config = element;
			SerializableProperty.SerializeProperties(this, element, false, false);
			foreach (XElement subElement in element.Elements().ToList<XElement>())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "childobject"))
				{
					if (a == "deformablesprite")
					{
						subElement.RemoveNodes();
						foreach (SpriteDeformation deformation in this.SpriteDeformations)
						{
							XElement deformationElement = new XElement("SpriteDeformation");
							deformation.Save(deformationElement);
							subElement.Add(deformationElement);
						}
					}
				}
				else
				{
					subElement.Remove();
				}
			}
			for (int i = 0; i < this.LightSourceParams.Count; i++)
			{
				int elementIndex = 0;
				bool wasSaved = false;
				foreach (XElement subElement2 in element.Elements().ToList<XElement>())
				{
					string a2 = subElement2.Name.ToString().ToLowerInvariant();
					if (a2 == "lightsource")
					{
						if (elementIndex == i)
						{
							SerializableProperty.SerializeProperties(this.LightSourceParams[i], subElement2, false, false);
							wasSaved = true;
						}
						else
						{
							elementIndex++;
						}
					}
				}
				if (!wasSaved)
				{
					XElement lightElement = new XElement("LightSource");
					SerializableProperty.SerializeProperties(this.LightSourceParams[i], lightElement, false, false);
					element.Add(lightElement);
				}
			}
			foreach (LevelObjectPrefab.ChildObject childObj in this.ChildObjects)
			{
				element.Add(new XElement("ChildObject", new object[]
				{
					new XAttribute("names", string.Join(", ", childObj.AllowedNames)),
					new XAttribute("mincount", childObj.MinCount),
					new XAttribute("maxcount", childObj.MaxCount)
				}));
			}
			foreach (KeyValuePair<Identifier, float> overrideCommonness in this.OverrideCommonness)
			{
				bool elementFound = false;
				foreach (XElement subElement3 in element.Elements())
				{
					if (subElement3.Name.ToString().Equals("overridecommonness", StringComparison.OrdinalIgnoreCase))
					{
						Identifier attributeIdentifier = subElement3.GetAttributeIdentifier("leveltype", Identifier.Empty);
						Identifier key = overrideCommonness.Key;
						if (attributeIdentifier == key)
						{
							subElement3.Attribute("commonness").Value = overrideCommonness.Value.ToString("G", CultureInfo.InvariantCulture);
							elementFound = true;
							break;
						}
					}
				}
				if (!elementFound)
				{
					element.Add(new XElement("overridecommonness", new object[]
					{
						new XAttribute("leveltype", overrideCommonness.Key),
						new XAttribute("commonness", overrideCommonness.Value.ToString("G", CultureInfo.InvariantCulture))
					}));
				}
			}
		}

		// Token: 0x17000822 RID: 2082
		// (get) Token: 0x06001E7F RID: 7807 RVA: 0x0013010C File Offset: 0x0012E30C
		// (set) Token: 0x06001E80 RID: 7808 RVA: 0x00130114 File Offset: 0x0012E314
		public List<Sprite> Sprites { get; private set; } = new List<Sprite>();

		// Token: 0x17000823 RID: 2083
		// (get) Token: 0x06001E81 RID: 7809 RVA: 0x0013011D File Offset: 0x0012E31D
		// (set) Token: 0x06001E82 RID: 7810 RVA: 0x00130125 File Offset: 0x0012E325
		public DeformableSprite DeformableSprite { get; private set; }

		// Token: 0x17000824 RID: 2084
		// (get) Token: 0x06001E83 RID: 7811 RVA: 0x0013012E File Offset: 0x0012E32E
		// (set) Token: 0x06001E84 RID: 7812 RVA: 0x00130136 File Offset: 0x0012E336
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f)]
		public float MinSize { get; private set; }

		// Token: 0x17000825 RID: 2085
		// (get) Token: 0x06001E85 RID: 7813 RVA: 0x0013013F File Offset: 0x0012E33F
		// (set) Token: 0x06001E86 RID: 7814 RVA: 0x00130147 File Offset: 0x0012E347
		[Serialize(1f, IsPropertySaveable.No, "", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f)]
		public float MaxSize { get; private set; }

		// Token: 0x17000826 RID: 2086
		// (get) Token: 0x06001E87 RID: 7815 RVA: 0x00130150 File Offset: 0x0012E350
		// (set) Token: 0x06001E88 RID: 7816 RVA: 0x00130158 File Offset: 0x0012E358
		[Serialize(Alignment.Left | Alignment.Right | Alignment.Top | Alignment.Bottom, IsPropertySaveable.Yes, "Which sides of a wall the object can spawn on.", "", false)]
		[Editable]
		public Alignment Alignment { get; private set; }

		// Token: 0x17000827 RID: 2087
		// (get) Token: 0x06001E89 RID: 7817 RVA: 0x00130161 File Offset: 0x0012E361
		// (set) Token: 0x06001E8A RID: 7818 RVA: 0x00130169 File Offset: 0x0012E369
		[Serialize(LevelObjectPrefab.SpawnPosType.Wall, IsPropertySaveable.No, "", "", false)]
		[Editable]
		public LevelObjectPrefab.SpawnPosType SpawnPos { get; private set; }

		// Token: 0x17000828 RID: 2088
		// (get) Token: 0x06001E8B RID: 7819 RVA: 0x00130172 File Offset: 0x0012E372
		// (set) Token: 0x06001E8C RID: 7820 RVA: 0x0013017A File Offset: 0x0012E37A
		public XElement Config { get; private set; }

		// Token: 0x17000829 RID: 2089
		// (get) Token: 0x06001E8D RID: 7821 RVA: 0x00130183 File Offset: 0x0012E383
		// (set) Token: 0x06001E8E RID: 7822 RVA: 0x0013018B File Offset: 0x0012E38B
		public XElement PhysicsBodyElement { get; private set; }

		// Token: 0x1700082A RID: 2090
		// (get) Token: 0x06001E8F RID: 7823 RVA: 0x00130194 File Offset: 0x0012E394
		// (set) Token: 0x06001E90 RID: 7824 RVA: 0x0013019C File Offset: 0x0012E39C
		public int PhysicsBodyTriggerIndex { get; private set; } = -1;

		// Token: 0x1700082B RID: 2091
		// (get) Token: 0x06001E91 RID: 7825 RVA: 0x001301A5 File Offset: 0x0012E3A5
		// (set) Token: 0x06001E92 RID: 7826 RVA: 0x001301AD File Offset: 0x0012E3AD
		public Dictionary<Sprite, XElement> SpriteSpecificPhysicsBodyElements { get; private set; } = new Dictionary<Sprite, XElement>();

		// Token: 0x1700082C RID: 2092
		// (get) Token: 0x06001E93 RID: 7827 RVA: 0x001301B6 File Offset: 0x0012E3B6
		// (set) Token: 0x06001E94 RID: 7828 RVA: 0x001301BE File Offset: 0x0012E3BE
		[Serialize(10000, IsPropertySaveable.No, "Maximum number of this specific object per level.", "", false)]
		[Editable(MinValueFloat = 0.01f, MaxValueFloat = 10f)]
		public int MaxCount { get; private set; }

		// Token: 0x1700082D RID: 2093
		// (get) Token: 0x06001E95 RID: 7829 RVA: 0x001301C7 File Offset: 0x0012E3C7
		// (set) Token: 0x06001E96 RID: 7830 RVA: 0x001301CF File Offset: 0x0012E3CF
		[Serialize("0.0,1.0", IsPropertySaveable.Yes, "The sprite depth of the object (min, max). Values of 0 or less make the object render in front of walls, values larger than 0 make it render behind walls with a parallax effect.", "", false)]
		[Editable]
		public Vector2 DepthRange { get; private set; }

		// Token: 0x1700082E RID: 2094
		// (get) Token: 0x06001E97 RID: 7831 RVA: 0x001301D8 File Offset: 0x0012E3D8
		// (set) Token: 0x06001E98 RID: 7832 RVA: 0x001301E0 File Offset: 0x0012E3E0
		[Serialize(3000f, IsPropertySaveable.Yes, "Objects fade out to the background color of the level the further they are from the camera. This value is the depth at which the object becomes \"maximally\" faded out.", "", false)]
		[Editable]
		public float FadeOutDepth { get; private set; }

		// Token: 0x1700082F RID: 2095
		// (get) Token: 0x06001E99 RID: 7833 RVA: 0x001301E9 File Offset: 0x0012E3E9
		// (set) Token: 0x06001E9A RID: 7834 RVA: 0x001301F1 File Offset: 0x0012E3F1
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		[Serialize(0f, IsPropertySaveable.Yes, "The tendency for the prefab to form clusters. Used as an exponent for perlin noise values that are used to determine the probability for an object to spawn at a specific position.", "", false)]
		public float ClusteringAmount { get; private set; }

		// Token: 0x17000830 RID: 2096
		// (get) Token: 0x06001E9B RID: 7835 RVA: 0x001301FA File Offset: 0x0012E3FA
		// (set) Token: 0x06001E9C RID: 7836 RVA: 0x00130202 File Offset: 0x0012E402
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		[Serialize(0f, IsPropertySaveable.Yes, "A value between 0-1 that determines the z-coordinate to sample perlin noise from when determining the probability  for an object to spawn at a specific position. Using the same (or close) value for different objects means the objects tend to form clusters in the same areas.", "", false)]
		public float ClusteringGroup { get; private set; }

		// Token: 0x17000831 RID: 2097
		// (get) Token: 0x06001E9D RID: 7837 RVA: 0x0013020B File Offset: 0x0012E40B
		// (set) Token: 0x06001E9E RID: 7838 RVA: 0x00130213 File Offset: 0x0012E413
		[Editable]
		[Serialize("0,0", IsPropertySaveable.Yes, "Random offset from the surface the object spawns on.", "", false)]
		public Vector2 RandomOffset { get; private set; }

		// Token: 0x17000832 RID: 2098
		// (get) Token: 0x06001E9F RID: 7839 RVA: 0x0013021C File Offset: 0x0012E41C
		// (set) Token: 0x06001EA0 RID: 7840 RVA: 0x00130224 File Offset: 0x0012E424
		[Editable]
		[Serialize(false, IsPropertySaveable.Yes, "Should the object be rotated to align it with the wall surface it spawns on.", "", false)]
		public bool AlignWithSurface { get; private set; }

		// Token: 0x17000833 RID: 2099
		// (get) Token: 0x06001EA1 RID: 7841 RVA: 0x0013022D File Offset: 0x0012E42D
		// (set) Token: 0x06001EA2 RID: 7842 RVA: 0x00130235 File Offset: 0x0012E435
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the object be placed near the start of the level.", "", false)]
		public bool AllowAtStart { get; private set; }

		// Token: 0x17000834 RID: 2100
		// (get) Token: 0x06001EA3 RID: 7843 RVA: 0x0013023E File Offset: 0x0012E43E
		// (set) Token: 0x06001EA4 RID: 7844 RVA: 0x00130246 File Offset: 0x0012E446
		[Editable]
		[Serialize(true, IsPropertySaveable.Yes, "Can the object be placed near the end of the level.", "", false)]
		public bool AllowAtEnd { get; private set; }

		// Token: 0x17000835 RID: 2101
		// (get) Token: 0x06001EA5 RID: 7845 RVA: 0x0013024F File Offset: 0x0012E44F
		// (set) Token: 0x06001EA6 RID: 7846 RVA: 0x00130257 File Offset: 0x0012E457
		[Serialize(0f, IsPropertySaveable.Yes, "Minimum length of a graph edge the object can spawn on.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float MinSurfaceWidth { get; private set; }

		// Token: 0x17000836 RID: 2102
		// (get) Token: 0x06001EA7 RID: 7847 RVA: 0x00130260 File Offset: 0x0012E460
		// (set) Token: 0x06001EA8 RID: 7848 RVA: 0x00130287 File Offset: 0x0012E487
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.Yes, "How much the rotation of the object can vary (min and max values in degrees).", "", false)]
		public Vector2 RandomRotation
		{
			get
			{
				return new Vector2(MathHelper.ToDegrees(this.randomRotation.X), MathHelper.ToDegrees(this.randomRotation.Y));
			}
			private set
			{
				this.randomRotation = new Vector2(MathHelper.ToRadians(value.X), MathHelper.ToRadians(value.Y));
			}
		}

		// Token: 0x17000837 RID: 2103
		// (get) Token: 0x06001EA9 RID: 7849 RVA: 0x001302AA File Offset: 0x0012E4AA
		public Vector2 RandomRotationRad
		{
			get
			{
				return this.randomRotation;
			}
		}

		// Token: 0x17000838 RID: 2104
		// (get) Token: 0x06001EAA RID: 7850 RVA: 0x001302B2 File Offset: 0x0012E4B2
		// (set) Token: 0x06001EAB RID: 7851 RVA: 0x001302BF File Offset: 0x0012E4BF
		[Serialize(0f, IsPropertySaveable.Yes, "How much the object swings (in degrees).", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 360f)]
		public float SwingAmount
		{
			get
			{
				return MathHelper.ToDegrees(this.swingAmount);
			}
			private set
			{
				this.swingAmount = MathHelper.ToRadians(value);
			}
		}

		// Token: 0x17000839 RID: 2105
		// (get) Token: 0x06001EAC RID: 7852 RVA: 0x001302CD File Offset: 0x0012E4CD
		public float SwingAmountRad
		{
			get
			{
				return this.swingAmount;
			}
		}

		// Token: 0x1700083A RID: 2106
		// (get) Token: 0x06001EAD RID: 7853 RVA: 0x001302D5 File Offset: 0x0012E4D5
		// (set) Token: 0x06001EAE RID: 7854 RVA: 0x001302DD File Offset: 0x0012E4DD
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the object swings.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float SwingFrequency { get; private set; }

		// Token: 0x1700083B RID: 2107
		// (get) Token: 0x06001EAF RID: 7855 RVA: 0x001302E6 File Offset: 0x0012E4E6
		// (set) Token: 0x06001EB0 RID: 7856 RVA: 0x001302EE File Offset: 0x0012E4EE
		[Editable]
		[Serialize("0.0,0.0", IsPropertySaveable.Yes, "How much the scale of the object oscillates on each axis. A value of 0.5,0.5 would make the object's scale oscillate from 100% to 150%.", "", false)]
		public Vector2 ScaleOscillation { get; private set; }

		// Token: 0x1700083C RID: 2108
		// (get) Token: 0x06001EB1 RID: 7857 RVA: 0x001302F7 File Offset: 0x0012E4F7
		// (set) Token: 0x06001EB2 RID: 7858 RVA: 0x001302FF File Offset: 0x0012E4FF
		[Serialize(0f, IsPropertySaveable.Yes, "How fast the object's scale oscillates.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float ScaleOscillationFrequency { get; private set; }

		// Token: 0x1700083D RID: 2109
		// (get) Token: 0x06001EB3 RID: 7859 RVA: 0x00130308 File Offset: 0x0012E508
		// (set) Token: 0x06001EB4 RID: 7860 RVA: 0x00130310 File Offset: 0x0012E510
		[Editable]
		[Serialize(1f, IsPropertySaveable.Yes, "How likely it is for the object to spawn in a level. This is relative to the commonness of the other objects - for example, having an object with a commonness of 1 and another with a commonness of 10 would mean the latter appears in levels 10 times as frequently as the former. The commonness value can be overridden on specific level types.", "", false)]
		public float Commonness { get; private set; }

		// Token: 0x1700083E RID: 2110
		// (get) Token: 0x06001EB5 RID: 7861 RVA: 0x00130319 File Offset: 0x0012E519
		// (set) Token: 0x06001EB6 RID: 7862 RVA: 0x00130321 File Offset: 0x0012E521
		[Serialize(0f, IsPropertySaveable.Yes, "How much the object disrupts submarine's sonar.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float SonarDisruption { get; private set; }

		// Token: 0x1700083F RID: 2111
		// (get) Token: 0x06001EB7 RID: 7863 RVA: 0x0013032A File Offset: 0x0012E52A
		// (set) Token: 0x06001EB8 RID: 7864 RVA: 0x00130332 File Offset: 0x0012E532
		[Serialize(false, IsPropertySaveable.Yes, "Can the object take damage from weapons/attacks that damage level walls.", "", false)]
		[Editable]
		public bool TakeLevelWallDamage { get; private set; }

		// Token: 0x17000840 RID: 2112
		// (get) Token: 0x06001EB9 RID: 7865 RVA: 0x0013033B File Offset: 0x0012E53B
		// (set) Token: 0x06001EBA RID: 7866 RVA: 0x00130343 File Offset: 0x0012E543
		[Serialize(false, IsPropertySaveable.Yes, "Should the object disappear if the object is destroyed? Only relevant if TakeLevelWallDamage is true.", "", false)]
		[Editable]
		public bool HideWhenBroken { get; private set; }

		// Token: 0x17000841 RID: 2113
		// (get) Token: 0x06001EBB RID: 7867 RVA: 0x0013034C File Offset: 0x0012E54C
		// (set) Token: 0x06001EBC RID: 7868 RVA: 0x00130354 File Offset: 0x0012E554
		[Serialize(100f, IsPropertySaveable.Yes, "Amount of health the object has. Only relevant if TakeLevelWallDamage is true.", "", false)]
		[Editable]
		public float Health { get; private set; }

		// Token: 0x17000842 RID: 2114
		// (get) Token: 0x06001EBD RID: 7869 RVA: 0x0013035D File Offset: 0x0012E55D
		// (set) Token: 0x06001EBE RID: 7870 RVA: 0x00130365 File Offset: 0x0012E565
		[Serialize("1.0,1.0,1.0,1.0", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color SpriteColor { get; private set; }

		// Token: 0x17000843 RID: 2115
		// (get) Token: 0x06001EBF RID: 7871 RVA: 0x0013036E File Offset: 0x0012E56E
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17000844 RID: 2116
		// (get) Token: 0x06001EC0 RID: 7872 RVA: 0x0013037B File Offset: 0x0012E57B
		// (set) Token: 0x06001EC1 RID: 7873 RVA: 0x00130383 File Offset: 0x0012E583
		public List<LevelObjectPrefab.ChildObject> ChildObjects { get; private set; }

		// Token: 0x17000845 RID: 2117
		// (get) Token: 0x06001EC2 RID: 7874 RVA: 0x0013038C File Offset: 0x0012E58C
		// (set) Token: 0x06001EC3 RID: 7875 RVA: 0x00130394 File Offset: 0x0012E594
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000846 RID: 2118
		// (get) Token: 0x06001EC4 RID: 7876 RVA: 0x0013039D File Offset: 0x0012E59D
		// (set) Token: 0x06001EC5 RID: 7877 RVA: 0x001303A5 File Offset: 0x0012E5A5
		public List<LevelObjectPrefab> OverrideProperties { get; private set; }

		// Token: 0x06001EC6 RID: 7878 RVA: 0x001303AE File Offset: 0x0012E5AE
		public override string ToString()
		{
			return "LevelObjectPrefab (" + this.Identifier.ToString() + ")";
		}

		// Token: 0x06001EC7 RID: 7879 RVA: 0x001303D0 File Offset: 0x0012E5D0
		public LevelObjectPrefab(ContentXElement element, LevelObjectPrefabsFile file, Identifier identifierOverride = default(Identifier)) : base(file, LevelObjectPrefab.ParseIdentifier(identifierOverride, element))
		{
			this.ChildObjects = new List<LevelObjectPrefab.ChildObject>();
			this.LevelTriggerElements = new List<ContentXElement>();
			this.OverrideProperties = new List<LevelObjectPrefab>();
			this.OverrideCommonness = new Dictionary<Identifier, float>();
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			ContentXElement contentXElement = null;
			if (element != contentXElement)
			{
				this.Config = element;
				this.LoadElements(file, element, -1);
				this.InitProjSpecific(element);
			}
			contentXElement = null;
			if (element != contentXElement && element.GetAttribute("minsurfacewidth") == null)
			{
				if (this.Sprites.Any<Sprite>())
				{
					this.MinSurfaceWidth = this.Sprites[0].size.X * this.MaxSize * 0.8f;
				}
				if (this.DeformableSprite != null)
				{
					this.MinSurfaceWidth = Math.Max(this.MinSurfaceWidth, this.DeformableSprite.Size.X * this.MaxSize * 0.8f);
				}
			}
		}

		// Token: 0x06001EC8 RID: 7880 RVA: 0x00130528 File Offset: 0x0012E728
		public static Identifier ParseIdentifier(Identifier identifierOverride, XElement element)
		{
			if (!identifierOverride.IsEmpty)
			{
				return identifierOverride;
			}
			Identifier identifier = element.GetAttributeIdentifier("identifier", "");
			if (identifier.IsEmpty)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(85, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Level object prefab \"");
				defaultInterpolatedStringHandler.AppendFormatted<XName>(element.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" has no identifier! Using the name as the identifier instead...");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				identifier = element.NameAsIdentifier();
			}
			return identifier;
		}

		// Token: 0x06001EC9 RID: 7881 RVA: 0x001305A0 File Offset: 0x0012E7A0
		private void LoadElements(LevelObjectPrefabsFile file, ContentXElement element, int parentTriggerIndex)
		{
			int propertyOverrideCount = 0;
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (a == "deformablesprite")
					{
						this.DeformableSprite = new DeformableSprite(subElement, null, null, "", true, false, 1f);
					}
				}
				else
				{
					Sprite newSprite = new Sprite(subElement, "", "", true, 1f);
					this.Sprites.Add(newSprite);
					ContentXElement contentXElement;
					if ((contentXElement = subElement.GetChildElement("PhysicsBody")) == null && (contentXElement = subElement.GetChildElement("Body")) == null)
					{
						contentXElement = (subElement.GetChildElement("physicsbody") ?? subElement.GetChildElement("body"));
					}
					ContentXElement spriteSpecificPhysicsBodyElement = contentXElement;
					ContentXElement contentXElement2 = null;
					if (spriteSpecificPhysicsBodyElement != contentXElement2)
					{
						this.SpriteSpecificPhysicsBodyElements.Add(newSprite, spriteSpecificPhysicsBodyElement);
					}
				}
			}
			foreach (ContentXElement subElement2 in element.Elements())
			{
				string text = subElement2.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					if (length <= 7)
					{
						if (length != 4)
						{
							if (length != 7)
							{
								continue;
							}
							if (!(text == "trigger"))
							{
								continue;
							}
						}
						else
						{
							if (!(text == "body"))
							{
								continue;
							}
							goto IL_379;
						}
					}
					else if (length != 11)
					{
						if (length != 12)
						{
							if (length != 18)
							{
								continue;
							}
							char c = text[8];
							if (c != 'c')
							{
								if (c != 'p')
								{
									continue;
								}
								if (!(text == "overrideproperties"))
								{
									continue;
								}
								ContentXElement element2 = subElement2;
								DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
								defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
								defaultInterpolatedStringHandler.AppendLiteral("-");
								defaultInterpolatedStringHandler.AppendFormatted<int>(propertyOverrideCount);
								LevelObjectPrefab propertyOverride = new LevelObjectPrefab(element2, file, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
								this.OverrideProperties[this.OverrideProperties.Count - 1] = propertyOverride;
								if (!propertyOverride.Sprites.Any<Sprite>() && propertyOverride.DeformableSprite == null)
								{
									propertyOverride.Sprites = this.Sprites;
									propertyOverride.DeformableSprite = this.DeformableSprite;
								}
								propertyOverrideCount++;
								continue;
							}
							else
							{
								if (!(text == "overridecommonness"))
								{
									continue;
								}
								Identifier levelType = subElement2.GetAttributeIdentifier("leveltype", Identifier.Empty);
								if (!this.OverrideCommonness.ContainsKey(levelType))
								{
									this.OverrideCommonness.Add(levelType, subElement2.GetAttributeFloat("commonness", 1f));
									continue;
								}
								continue;
							}
						}
						else if (!(text == "leveltrigger"))
						{
							continue;
						}
					}
					else
					{
						char c = text[0];
						if (c != 'c')
						{
							if (c != 'p')
							{
								continue;
							}
							if (!(text == "physicsbody"))
							{
								continue;
							}
							goto IL_379;
						}
						else
						{
							if (!(text == "childobject"))
							{
								continue;
							}
							this.ChildObjects.Add(new LevelObjectPrefab.ChildObject(subElement2));
							continue;
						}
					}
					this.OverrideProperties.Add(null);
					this.LevelTriggerElements.Add(subElement2);
					this.LoadElements(file, subElement2, this.LevelTriggerElements.Count - 1);
					continue;
					IL_379:
					this.PhysicsBodyElement = subElement2;
					this.PhysicsBodyTriggerIndex = parentTriggerIndex;
				}
			}
		}

		// Token: 0x06001ECA RID: 7882 RVA: 0x00130988 File Offset: 0x0012EB88
		private void InitProjSpecific(ContentXElement element)
		{
			this.LoadElementsProjSpecific(element, -1);
		}

		// Token: 0x06001ECB RID: 7883 RVA: 0x00130994 File Offset: 0x0012EB94
		public float GetCommonness(CaveGenerationParams generationParams, bool requireCaveSpecificOverride = true)
		{
			float commonness;
			if (generationParams != null && generationParams.Identifier != Identifier.Empty && this.OverrideCommonness.TryGetValue(generationParams.Identifier, out commonness))
			{
				return commonness;
			}
			if (!requireCaveSpecificOverride)
			{
				return this.Commonness;
			}
			return 0f;
		}

		// Token: 0x06001ECC RID: 7884 RVA: 0x001309DC File Offset: 0x0012EBDC
		public float GetCommonness(LevelData levelData)
		{
			float commonness;
			if ((levelData.GenerationParams != null && levelData.GenerationParams.Identifier != Identifier.Empty && this.OverrideCommonness.TryGetValue(levelData.GenerationParams.Identifier, out commonness)) || (!levelData.GenerationParams.OldIdentifier.IsEmpty && this.OverrideCommonness.TryGetValue(levelData.GenerationParams.OldIdentifier, out commonness)))
			{
				return commonness;
			}
			float biomeCommonness;
			if (((levelData != null) ? levelData.Biome : null) != null && this.OverrideCommonness.TryGetValue(levelData.Biome.Identifier, out biomeCommonness))
			{
				return biomeCommonness;
			}
			return this.Commonness;
		}

		// Token: 0x06001ECD RID: 7885 RVA: 0x00130A83 File Offset: 0x0012EC83
		public override void Dispose()
		{
		}

		// Token: 0x04000F8E RID: 3982
		public static readonly PrefabCollection<LevelObjectPrefab> Prefabs = new PrefabCollection<LevelObjectPrefab>();

		// Token: 0x04000F96 RID: 3990
		public readonly List<ContentXElement> LevelTriggerElements;

		// Token: 0x04000F97 RID: 3991
		public readonly Dictionary<Identifier, float> OverrideCommonness;

		// Token: 0x04000FA5 RID: 4005
		private Vector2 randomRotation;

		// Token: 0x04000FA6 RID: 4006
		private float swingAmount;

		// Token: 0x02000B46 RID: 2886
		public class SoundConfig
		{
			// Token: 0x06007816 RID: 30742 RVA: 0x0037C62C File Offset: 0x0037A82C
			public SoundConfig(ContentXElement element, int triggerIndex)
			{
				this.SoundElement = element;
				string key = "position";
				Vector2 zero = Vector2.Zero;
				this.Position = element.GetAttributeVector2(key, zero);
				this.TriggerIndex = triggerIndex;
			}

			// Token: 0x04004722 RID: 18210
			public readonly ContentXElement SoundElement;

			// Token: 0x04004723 RID: 18211
			public readonly Vector2 Position;

			// Token: 0x04004724 RID: 18212
			public readonly int TriggerIndex;
		}

		// Token: 0x02000B47 RID: 2887
		public class ChildObject
		{
			// Token: 0x06007817 RID: 30743 RVA: 0x0037C666 File Offset: 0x0037A866
			public ChildObject()
			{
				this.AllowedNames = new List<string>();
				this.MinCount = 1;
				this.MaxCount = 1;
			}

			// Token: 0x06007818 RID: 30744 RVA: 0x0037C688 File Offset: 0x0037A888
			public ChildObject(XElement element)
			{
				this.AllowedNames = element.GetAttributeStringArray("names", Array.Empty<string>(), true, false).ToList<string>();
				this.MinCount = element.GetAttributeInt("mincount", 1);
				this.MaxCount = Math.Max(element.GetAttributeInt("maxcount", 1), this.MinCount);
			}

			// Token: 0x04004725 RID: 18213
			public List<string> AllowedNames;

			// Token: 0x04004726 RID: 18214
			public int MinCount;

			// Token: 0x04004727 RID: 18215
			public int MaxCount;
		}

		// Token: 0x02000B48 RID: 2888
		[Flags]
		public enum SpawnPosType
		{
			// Token: 0x04004729 RID: 18217
			None = 0,
			// Token: 0x0400472A RID: 18218
			MainPathWall = 1,
			// Token: 0x0400472B RID: 18219
			SidePathWall = 2,
			// Token: 0x0400472C RID: 18220
			CaveWall = 4,
			// Token: 0x0400472D RID: 18221
			NestWall = 8,
			// Token: 0x0400472E RID: 18222
			RuinWall = 16,
			// Token: 0x0400472F RID: 18223
			SeaFloor = 32,
			// Token: 0x04004730 RID: 18224
			MainPath = 64,
			// Token: 0x04004731 RID: 18225
			LevelStart = 128,
			// Token: 0x04004732 RID: 18226
			LevelEnd = 256,
			// Token: 0x04004733 RID: 18227
			OutpostWall = 512,
			// Token: 0x04004734 RID: 18228
			Wall = 7
		}
	}
}
