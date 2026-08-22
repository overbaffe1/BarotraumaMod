using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x0200031A RID: 794
	internal class CaveGenerationParams : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x17001092 RID: 4242
		// (get) Token: 0x06003F1F RID: 16159 RVA: 0x00235A89 File Offset: 0x00233C89
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17001093 RID: 4243
		// (get) Token: 0x06003F20 RID: 16160 RVA: 0x00235A96 File Offset: 0x00233C96
		// (set) Token: 0x06003F21 RID: 16161 RVA: 0x00235A9E File Offset: 0x00233C9E
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x17001094 RID: 4244
		// (get) Token: 0x06003F22 RID: 16162 RVA: 0x00235AA7 File Offset: 0x00233CA7
		// (set) Token: 0x06003F23 RID: 16163 RVA: 0x00235AAF File Offset: 0x00233CAF
		[Editable]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float Commonness { get; private set; }

		// Token: 0x17001095 RID: 4245
		// (get) Token: 0x06003F24 RID: 16164 RVA: 0x00235AB8 File Offset: 0x00233CB8
		// (set) Token: 0x06003F25 RID: 16165 RVA: 0x00235AC0 File Offset: 0x00233CC0
		[Serialize(8000, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueInt = 1000, MaxValueInt = 100000)]
		public int MinWidth
		{
			get
			{
				return this.minWidth;
			}
			set
			{
				this.minWidth = Math.Max(value, 1000);
			}
		}

		// Token: 0x17001096 RID: 4246
		// (get) Token: 0x06003F26 RID: 16166 RVA: 0x00235AD3 File Offset: 0x00233CD3
		// (set) Token: 0x06003F27 RID: 16167 RVA: 0x00235ADB File Offset: 0x00233CDB
		[Serialize(10000, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueInt = 1000, MaxValueInt = 1000000)]
		public int MaxWidth
		{
			get
			{
				return this.maxWidth;
			}
			set
			{
				this.maxWidth = Math.Max(value, this.minWidth);
			}
		}

		// Token: 0x17001097 RID: 4247
		// (get) Token: 0x06003F28 RID: 16168 RVA: 0x00235AEF File Offset: 0x00233CEF
		// (set) Token: 0x06003F29 RID: 16169 RVA: 0x00235AF7 File Offset: 0x00233CF7
		[Serialize(8000, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueInt = 1000, MaxValueInt = 100000)]
		public int MinHeight
		{
			get
			{
				return this.minHeight;
			}
			set
			{
				this.minHeight = Math.Max(value, 1000);
			}
		}

		// Token: 0x17001098 RID: 4248
		// (get) Token: 0x06003F2A RID: 16170 RVA: 0x00235B0A File Offset: 0x00233D0A
		// (set) Token: 0x06003F2B RID: 16171 RVA: 0x00235B12 File Offset: 0x00233D12
		[Serialize(10000, IsPropertySaveable.Yes, "", "", false)]
		[Editable(MinValueInt = 1000, MaxValueInt = 1000000)]
		public int MaxHeight
		{
			get
			{
				return this.maxHeight;
			}
			set
			{
				this.maxHeight = Math.Max(value, this.minHeight);
			}
		}

		// Token: 0x17001099 RID: 4249
		// (get) Token: 0x06003F2C RID: 16172 RVA: 0x00235B26 File Offset: 0x00233D26
		// (set) Token: 0x06003F2D RID: 16173 RVA: 0x00235B2E File Offset: 0x00233D2E
		[Serialize(2, IsPropertySaveable.Yes, "Minimum number of tunnel branches in the cave.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MinBranchCount
		{
			get
			{
				return this.minBranchCount;
			}
			set
			{
				this.minBranchCount = Math.Max(value, 0);
			}
		}

		// Token: 0x1700109A RID: 4250
		// (get) Token: 0x06003F2E RID: 16174 RVA: 0x00235B3D File Offset: 0x00233D3D
		// (set) Token: 0x06003F2F RID: 16175 RVA: 0x00235B45 File Offset: 0x00233D45
		[Serialize(4, IsPropertySaveable.Yes, "Maximum number of tunnel branches in the cave.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MaxBranchCount
		{
			get
			{
				return this.maxBranchCount;
			}
			set
			{
				this.maxBranchCount = Math.Max(value, this.minBranchCount);
			}
		}

		// Token: 0x1700109B RID: 4251
		// (get) Token: 0x06003F30 RID: 16176 RVA: 0x00235B59 File Offset: 0x00233D59
		// (set) Token: 0x06003F31 RID: 16177 RVA: 0x00235B61 File Offset: 0x00233D61
		[Serialize(50, IsPropertySaveable.Yes, "Total amount of level objects in the cave.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10000)]
		public int LevelObjectAmount { get; set; }

		// Token: 0x1700109C RID: 4252
		// (get) Token: 0x06003F32 RID: 16178 RVA: 0x00235B6A File Offset: 0x00233D6A
		// (set) Token: 0x06003F33 RID: 16179 RVA: 0x00235B72 File Offset: 0x00233D72
		[Serialize(0.1f, IsPropertySaveable.Yes, "What portion of the empty cells in the cave should be turned into destructible walls? For example, 0.1 = 10%.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float DestructibleWallRatio { get; set; }

		// Token: 0x06003F34 RID: 16180 RVA: 0x00235B7C File Offset: 0x00233D7C
		public static CaveGenerationParams GetRandom(Level level, bool abyss, Rand.RandSync rand)
		{
			List<CaveGenerationParams> caveParams = (from p in CaveGenerationParams.CaveParams
			orderby p.UintIdentifier
			select p).ToList<CaveGenerationParams>();
			if (caveParams.All((CaveGenerationParams p) => p.GetCommonness(level.LevelData, abyss) <= 0f))
			{
				return caveParams.First<CaveGenerationParams>();
			}
			return ToolBox.SelectWeightedRandom<CaveGenerationParams>(caveParams.ToList<CaveGenerationParams>(), (from p in caveParams
			select p.GetCommonness(level.LevelData, abyss)).ToList<float>(), rand);
		}

		// Token: 0x06003F35 RID: 16181 RVA: 0x00235C0C File Offset: 0x00233E0C
		public float GetCommonness(LevelData levelData, bool abyss)
		{
			float commonness;
			if (levelData.GenerationParams != null && levelData.GenerationParams.Identifier != Identifier.Empty && this.OverrideCommonness.TryGetValue(abyss ? "abyss".ToIdentifier() : levelData.GenerationParams.Identifier, out commonness))
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

		// Token: 0x06003F36 RID: 16182 RVA: 0x00235C94 File Offset: 0x00233E94
		public CaveGenerationParams(ContentXElement element, CaveGenerationParametersFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "wall"))
				{
					if (!(a == "walledge"))
					{
						if (a == "overridecommonness")
						{
							Identifier levelType = subElement.GetAttributeIdentifier("leveltype", "");
							if (!this.OverrideCommonness.ContainsKey(levelType))
							{
								this.OverrideCommonness.Add(levelType, subElement.GetAttributeFloat("commonness", 1f));
							}
						}
					}
					else
					{
						this.WallEdgeSprite = new Sprite(subElement, "", "", false, 1f);
					}
				}
				else
				{
					this.WallSprite = new Sprite(subElement, "", "", false, 1f);
				}
			}
		}

		// Token: 0x06003F37 RID: 16183 RVA: 0x00235DC8 File Offset: 0x00233FC8
		public void Save(XElement element)
		{
			SerializableProperty.SerializeProperties(this, element, true, false);
			foreach (KeyValuePair<Identifier, float> overrideCommonness in this.OverrideCommonness)
			{
				bool elementFound = false;
				foreach (XElement subElement in element.Elements())
				{
					Identifier identifier = subElement.NameAsIdentifier();
					if (identifier == "overridecommonness")
					{
						Identifier attributeIdentifier = subElement.GetAttributeIdentifier("leveltype", "");
						Identifier key = overrideCommonness.Key;
						if (attributeIdentifier == key)
						{
							subElement.Attribute("commonness").Value = overrideCommonness.Value.ToString("G", CultureInfo.InvariantCulture);
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

		// Token: 0x06003F38 RID: 16184 RVA: 0x00235F50 File Offset: 0x00234150
		public override void Dispose()
		{
			Sprite wallSprite = this.WallSprite;
			if (wallSprite != null)
			{
				wallSprite.Remove();
			}
			Sprite wallEdgeSprite = this.WallEdgeSprite;
			if (wallEdgeSprite == null)
			{
				return;
			}
			wallEdgeSprite.Remove();
		}

		// Token: 0x040020CB RID: 8395
		public static readonly PrefabCollection<CaveGenerationParams> CaveParams = new PrefabCollection<CaveGenerationParams>();

		// Token: 0x040020CC RID: 8396
		private int minWidth;

		// Token: 0x040020CD RID: 8397
		private int maxWidth;

		// Token: 0x040020CE RID: 8398
		private int minHeight;

		// Token: 0x040020CF RID: 8399
		private int maxHeight;

		// Token: 0x040020D0 RID: 8400
		private int minBranchCount;

		// Token: 0x040020D1 RID: 8401
		private int maxBranchCount;

		// Token: 0x040020D3 RID: 8403
		public readonly Dictionary<Identifier, float> OverrideCommonness = new Dictionary<Identifier, float>();

		// Token: 0x040020D7 RID: 8407
		public readonly Sprite WallSprite;

		// Token: 0x040020D8 RID: 8408
		public readonly Sprite WallEdgeSprite;
	}
}
