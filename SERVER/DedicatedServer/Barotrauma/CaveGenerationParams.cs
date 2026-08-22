using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000236 RID: 566
	internal class CaveGenerationParams : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x17000B1F RID: 2847
		// (get) Token: 0x060026B0 RID: 9904 RVA: 0x000FD581 File Offset: 0x000FB781
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17000B20 RID: 2848
		// (get) Token: 0x060026B1 RID: 9905 RVA: 0x000FD58E File Offset: 0x000FB78E
		// (set) Token: 0x060026B2 RID: 9906 RVA: 0x000FD596 File Offset: 0x000FB796
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x17000B21 RID: 2849
		// (get) Token: 0x060026B3 RID: 9907 RVA: 0x000FD59F File Offset: 0x000FB79F
		// (set) Token: 0x060026B4 RID: 9908 RVA: 0x000FD5A7 File Offset: 0x000FB7A7
		[Editable]
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		public float Commonness { get; private set; }

		// Token: 0x17000B22 RID: 2850
		// (get) Token: 0x060026B5 RID: 9909 RVA: 0x000FD5B0 File Offset: 0x000FB7B0
		// (set) Token: 0x060026B6 RID: 9910 RVA: 0x000FD5B8 File Offset: 0x000FB7B8
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

		// Token: 0x17000B23 RID: 2851
		// (get) Token: 0x060026B7 RID: 9911 RVA: 0x000FD5CB File Offset: 0x000FB7CB
		// (set) Token: 0x060026B8 RID: 9912 RVA: 0x000FD5D3 File Offset: 0x000FB7D3
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

		// Token: 0x17000B24 RID: 2852
		// (get) Token: 0x060026B9 RID: 9913 RVA: 0x000FD5E7 File Offset: 0x000FB7E7
		// (set) Token: 0x060026BA RID: 9914 RVA: 0x000FD5EF File Offset: 0x000FB7EF
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

		// Token: 0x17000B25 RID: 2853
		// (get) Token: 0x060026BB RID: 9915 RVA: 0x000FD602 File Offset: 0x000FB802
		// (set) Token: 0x060026BC RID: 9916 RVA: 0x000FD60A File Offset: 0x000FB80A
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

		// Token: 0x17000B26 RID: 2854
		// (get) Token: 0x060026BD RID: 9917 RVA: 0x000FD61E File Offset: 0x000FB81E
		// (set) Token: 0x060026BE RID: 9918 RVA: 0x000FD626 File Offset: 0x000FB826
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

		// Token: 0x17000B27 RID: 2855
		// (get) Token: 0x060026BF RID: 9919 RVA: 0x000FD635 File Offset: 0x000FB835
		// (set) Token: 0x060026C0 RID: 9920 RVA: 0x000FD63D File Offset: 0x000FB83D
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

		// Token: 0x17000B28 RID: 2856
		// (get) Token: 0x060026C1 RID: 9921 RVA: 0x000FD651 File Offset: 0x000FB851
		// (set) Token: 0x060026C2 RID: 9922 RVA: 0x000FD659 File Offset: 0x000FB859
		[Serialize(50, IsPropertySaveable.Yes, "Total amount of level objects in the cave.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10000)]
		public int LevelObjectAmount { get; set; }

		// Token: 0x17000B29 RID: 2857
		// (get) Token: 0x060026C3 RID: 9923 RVA: 0x000FD662 File Offset: 0x000FB862
		// (set) Token: 0x060026C4 RID: 9924 RVA: 0x000FD66A File Offset: 0x000FB86A
		[Serialize(0.1f, IsPropertySaveable.Yes, "What portion of the empty cells in the cave should be turned into destructible walls? For example, 0.1 = 10%.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float DestructibleWallRatio { get; set; }

		// Token: 0x060026C5 RID: 9925 RVA: 0x000FD674 File Offset: 0x000FB874
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

		// Token: 0x060026C6 RID: 9926 RVA: 0x000FD704 File Offset: 0x000FB904
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

		// Token: 0x060026C7 RID: 9927 RVA: 0x000FD78C File Offset: 0x000FB98C
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

		// Token: 0x060026C8 RID: 9928 RVA: 0x000FD8C0 File Offset: 0x000FBAC0
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

		// Token: 0x060026C9 RID: 9929 RVA: 0x000FDA48 File Offset: 0x000FBC48
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

		// Token: 0x040012F1 RID: 4849
		public static readonly PrefabCollection<CaveGenerationParams> CaveParams = new PrefabCollection<CaveGenerationParams>();

		// Token: 0x040012F2 RID: 4850
		private int minWidth;

		// Token: 0x040012F3 RID: 4851
		private int maxWidth;

		// Token: 0x040012F4 RID: 4852
		private int minHeight;

		// Token: 0x040012F5 RID: 4853
		private int maxHeight;

		// Token: 0x040012F6 RID: 4854
		private int minBranchCount;

		// Token: 0x040012F7 RID: 4855
		private int maxBranchCount;

		// Token: 0x040012F9 RID: 4857
		public readonly Dictionary<Identifier, float> OverrideCommonness = new Dictionary<Identifier, float>();

		// Token: 0x040012FD RID: 4861
		public readonly Sprite WallSprite;

		// Token: 0x040012FE RID: 4862
		public readonly Sprite WallEdgeSprite;
	}
}
