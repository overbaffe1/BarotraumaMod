using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Barotrauma.SpriteDeformations;

namespace Barotrauma
{
	// Token: 0x020000D5 RID: 213
	internal class BackgroundCreaturePrefab : Prefab, ISerializableEntity
	{
		// Token: 0x1700074B RID: 1867
		// (get) Token: 0x06001C4A RID: 7242 RVA: 0x0011A882 File Offset: 0x00118A82
		// (set) Token: 0x06001C4B RID: 7243 RVA: 0x0011A88A File Offset: 0x00118A8A
		public Sprite Sprite { get; private set; }

		// Token: 0x1700074C RID: 1868
		// (get) Token: 0x06001C4C RID: 7244 RVA: 0x0011A893 File Offset: 0x00118A93
		// (set) Token: 0x06001C4D RID: 7245 RVA: 0x0011A89B File Offset: 0x00118A9B
		public Sprite LightSprite { get; private set; }

		// Token: 0x1700074D RID: 1869
		// (get) Token: 0x06001C4E RID: 7246 RVA: 0x0011A8A4 File Offset: 0x00118AA4
		// (set) Token: 0x06001C4F RID: 7247 RVA: 0x0011A8AC File Offset: 0x00118AAC
		public DeformableSprite DeformableSprite { get; private set; }

		// Token: 0x1700074E RID: 1870
		// (get) Token: 0x06001C50 RID: 7248 RVA: 0x0011A8B5 File Offset: 0x00118AB5
		// (set) Token: 0x06001C51 RID: 7249 RVA: 0x0011A8BD File Offset: 0x00118ABD
		public DeformableSprite DeformableLightSprite { get; private set; }

		// Token: 0x1700074F RID: 1871
		// (get) Token: 0x06001C52 RID: 7250 RVA: 0x0011A8C6 File Offset: 0x00118AC6
		// (set) Token: 0x06001C53 RID: 7251 RVA: 0x0011A8CE File Offset: 0x00118ACE
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float Speed { get; private set; }

		// Token: 0x17000750 RID: 1872
		// (get) Token: 0x06001C54 RID: 7252 RVA: 0x0011A8D7 File Offset: 0x00118AD7
		// (set) Token: 0x06001C55 RID: 7253 RVA: 0x0011A8DF File Offset: 0x00118ADF
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f, DecimalCount = 3)]
		public float WanderAmount { get; private set; }

		// Token: 0x17000751 RID: 1873
		// (get) Token: 0x06001C56 RID: 7254 RVA: 0x0011A8E8 File Offset: 0x00118AE8
		// (set) Token: 0x06001C57 RID: 7255 RVA: 0x0011A8F0 File Offset: 0x00118AF0
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f, DecimalCount = 3)]
		public float WanderZAmount { get; private set; }

		// Token: 0x17000752 RID: 1874
		// (get) Token: 0x06001C58 RID: 7256 RVA: 0x0011A8F9 File Offset: 0x00118AF9
		// (set) Token: 0x06001C59 RID: 7257 RVA: 0x0011A901 File Offset: 0x00118B01
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int SwarmMin { get; private set; }

		// Token: 0x17000753 RID: 1875
		// (get) Token: 0x06001C5A RID: 7258 RVA: 0x0011A90A File Offset: 0x00118B0A
		// (set) Token: 0x06001C5B RID: 7259 RVA: 0x0011A912 File Offset: 0x00118B12
		[Serialize(1, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int SwarmMax { get; private set; }

		// Token: 0x17000754 RID: 1876
		// (get) Token: 0x06001C5C RID: 7260 RVA: 0x0011A91B File Offset: 0x00118B1B
		// (set) Token: 0x06001C5D RID: 7261 RVA: 0x0011A923 File Offset: 0x00118B23
		[Serialize(200f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f)]
		public float SwarmRadius { get; private set; }

		// Token: 0x17000755 RID: 1877
		// (get) Token: 0x06001C5E RID: 7262 RVA: 0x0011A92C File Offset: 0x00118B2C
		// (set) Token: 0x06001C5F RID: 7263 RVA: 0x0011A934 File Offset: 0x00118B34
		[Serialize(0.2f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10f)]
		public float SwarmCohesion { get; private set; }

		// Token: 0x17000756 RID: 1878
		// (get) Token: 0x06001C60 RID: 7264 RVA: 0x0011A93D File Offset: 0x00118B3D
		// (set) Token: 0x06001C61 RID: 7265 RVA: 0x0011A945 File Offset: 0x00118B45
		[Serialize(10f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f)]
		public float MinDepth { get; private set; }

		// Token: 0x17000757 RID: 1879
		// (get) Token: 0x06001C62 RID: 7266 RVA: 0x0011A94E File Offset: 0x00118B4E
		// (set) Token: 0x06001C63 RID: 7267 RVA: 0x0011A956 File Offset: 0x00118B56
		[Serialize(1000f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 10000f)]
		public float MaxDepth { get; private set; }

		// Token: 0x17000758 RID: 1880
		// (get) Token: 0x06001C64 RID: 7268 RVA: 0x0011A95F File Offset: 0x00118B5F
		// (set) Token: 0x06001C65 RID: 7269 RVA: 0x0011A967 File Offset: 0x00118B67
		[Serialize(10000f, IsPropertySaveable.Yes, "Creatures fade out to the background color of the level the further they are from the camera. This value is the depth at which the object becomes \"maximally\" faded out.", "", false)]
		[Editable]
		public float FadeOutDepth { get; private set; }

		// Token: 0x17000759 RID: 1881
		// (get) Token: 0x06001C66 RID: 7270 RVA: 0x0011A970 File Offset: 0x00118B70
		// (set) Token: 0x06001C67 RID: 7271 RVA: 0x0011A978 File Offset: 0x00118B78
		[Serialize(true, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool FadeOut { get; private set; }

		// Token: 0x1700075A RID: 1882
		// (get) Token: 0x06001C68 RID: 7272 RVA: 0x0011A981 File Offset: 0x00118B81
		// (set) Token: 0x06001C69 RID: 7273 RVA: 0x0011A989 File Offset: 0x00118B89
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool DisableRotation { get; private set; }

		// Token: 0x1700075B RID: 1883
		// (get) Token: 0x06001C6A RID: 7274 RVA: 0x0011A992 File Offset: 0x00118B92
		// (set) Token: 0x06001C6B RID: 7275 RVA: 0x0011A99A File Offset: 0x00118B9A
		[Serialize(false, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public bool DisableFlipping { get; private set; }

		// Token: 0x1700075C RID: 1884
		// (get) Token: 0x06001C6C RID: 7276 RVA: 0x0011A9A3 File Offset: 0x00118BA3
		// (set) Token: 0x06001C6D RID: 7277 RVA: 0x0011A9AB File Offset: 0x00118BAB
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float Scale { get; private set; }

		// Token: 0x1700075D RID: 1885
		// (get) Token: 0x06001C6E RID: 7278 RVA: 0x0011A9B4 File Offset: 0x00118BB4
		// (set) Token: 0x06001C6F RID: 7279 RVA: 0x0011A9BC File Offset: 0x00118BBC
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float Commonness { get; private set; }

		// Token: 0x1700075E RID: 1886
		// (get) Token: 0x06001C70 RID: 7280 RVA: 0x0011A9C5 File Offset: 0x00118BC5
		// (set) Token: 0x06001C71 RID: 7281 RVA: 0x0011A9CD File Offset: 0x00118BCD
		[Serialize(1000, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int MaxCount { get; private set; }

		// Token: 0x1700075F RID: 1887
		// (get) Token: 0x06001C72 RID: 7282 RVA: 0x0011A9D6 File Offset: 0x00118BD6
		// (set) Token: 0x06001C73 RID: 7283 RVA: 0x0011A9DE File Offset: 0x00118BDE
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float FlashInterval { get; private set; }

		// Token: 0x17000760 RID: 1888
		// (get) Token: 0x06001C74 RID: 7284 RVA: 0x0011A9E7 File Offset: 0x00118BE7
		// (set) Token: 0x06001C75 RID: 7285 RVA: 0x0011A9EF File Offset: 0x00118BEF
		[Serialize(0f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1000f)]
		public float FlashDuration { get; private set; }

		// Token: 0x17000761 RID: 1889
		// (get) Token: 0x06001C76 RID: 7286 RVA: 0x0011A9F8 File Offset: 0x00118BF8
		public string Name
		{
			get
			{
				return this.name;
			}
		}

		// Token: 0x17000762 RID: 1890
		// (get) Token: 0x06001C77 RID: 7287 RVA: 0x0011AA00 File Offset: 0x00118C00
		// (set) Token: 0x06001C78 RID: 7288 RVA: 0x0011AA08 File Offset: 0x00118C08
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x17000763 RID: 1891
		// (get) Token: 0x06001C79 RID: 7289 RVA: 0x0011AA11 File Offset: 0x00118C11
		// (set) Token: 0x06001C7A RID: 7290 RVA: 0x0011AA19 File Offset: 0x00118C19
		public List<SpriteDeformation> SpriteDeformations { get; private set; } = new List<SpriteDeformation>();

		// Token: 0x06001C7B RID: 7291 RVA: 0x0011AA24 File Offset: 0x00118C24
		public BackgroundCreaturePrefab(ContentXElement element, BackgroundCreaturePrefabsFile file) : base(file, BackgroundCreaturePrefab.ParseIdentifier(element))
		{
			this.name = element.Name.ToString();
			this.Config = element;
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "sprite"))
				{
					if (!(a == "deformablesprite"))
					{
						if (!(a == "lightsprite"))
						{
							if (a == "deformablelightsprite")
							{
								this.DeformableLightSprite = new DeformableSprite(subElement, null, null, "", true, false, 1f);
								continue;
							}
							if (!(a == "overridecommonness"))
							{
								continue;
							}
							Identifier levelType = subElement.GetAttributeIdentifier("leveltype", Identifier.Empty);
							if (!this.OverrideCommonness.ContainsKey(levelType))
							{
								this.OverrideCommonness.Add(levelType, subElement.GetAttributeFloat("commonness", 1f));
								continue;
							}
							continue;
						}
					}
					else
					{
						this.DeformableSprite = new DeformableSprite(subElement, null, null, "", true, false, 1f);
						using (IEnumerator<ContentXElement> enumerator2 = subElement.Elements().GetEnumerator())
						{
							while (enumerator2.MoveNext())
							{
								ContentXElement cxe = enumerator2.Current;
								XElement deformElement = cxe;
								SpriteDeformation deformation = SpriteDeformation.Load(deformElement, this.Name);
								if (deformation != null)
								{
									this.SpriteDeformations.Add(deformation);
								}
							}
							continue;
						}
					}
					this.LightSprite = new Sprite(subElement, "", "", true, 1f);
				}
				else
				{
					this.Sprite = new Sprite(subElement, "", "", true, 1f);
				}
			}
		}

		// Token: 0x06001C7C RID: 7292 RVA: 0x0011AC78 File Offset: 0x00118E78
		public static Identifier ParseIdentifier(XElement element)
		{
			Identifier identifier = element.GetAttributeIdentifier("identifier", "");
			if (identifier.IsEmpty)
			{
				identifier = element.NameAsIdentifier();
			}
			return identifier;
		}

		// Token: 0x06001C7D RID: 7293 RVA: 0x0011ACA8 File Offset: 0x00118EA8
		public float GetCommonness(LevelData levelData)
		{
			LevelGenerationParams generationParams = (levelData != null) ? levelData.GenerationParams : null;
			if (generationParams == null || generationParams.Identifier.IsEmpty)
			{
				return this.Commonness;
			}
			float commonness;
			if (this.OverrideCommonness.TryGetValue(generationParams.Identifier, out commonness) || (!generationParams.OldIdentifier.IsEmpty && this.OverrideCommonness.TryGetValue(generationParams.OldIdentifier, out commonness)) || this.OverrideCommonness.TryGetValue(levelData.Biome.Identifier, out commonness))
			{
				return commonness;
			}
			return this.Commonness;
		}

		// Token: 0x06001C7E RID: 7294 RVA: 0x0011AD38 File Offset: 0x00118F38
		public override void Dispose()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.Sprite = null;
			Sprite lightSprite = this.LightSprite;
			if (lightSprite != null)
			{
				lightSprite.Remove();
			}
			this.LightSprite = null;
			DeformableSprite deformableLightSprite = this.DeformableLightSprite;
			if (deformableLightSprite != null)
			{
				deformableLightSprite.Remove();
			}
			this.DeformableLightSprite = null;
			DeformableSprite deformableSprite = this.DeformableSprite;
			if (deformableSprite != null)
			{
				deformableSprite.Remove();
			}
			this.DeformableSprite = null;
		}

		// Token: 0x04000E90 RID: 3728
		public static readonly PrefabCollection<BackgroundCreaturePrefab> Prefabs = new PrefabCollection<BackgroundCreaturePrefab>();

		// Token: 0x04000E95 RID: 3733
		private readonly string name;

		// Token: 0x04000E96 RID: 3734
		public readonly XElement Config;

		// Token: 0x04000EAB RID: 3755
		public Dictionary<Identifier, float> OverrideCommonness = new Dictionary<Identifier, float>();
	}
}
