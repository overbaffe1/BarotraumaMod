using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200023B RID: 571
	internal class LevelGenerationParams : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x17000B36 RID: 2870
		// (get) Token: 0x060026FA RID: 9978 RVA: 0x0010066F File Offset: 0x000FE86F
		// (set) Token: 0x060026FB RID: 9979 RVA: 0x00100677 File Offset: 0x000FE877
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x17000B37 RID: 2871
		// (get) Token: 0x060026FC RID: 9980 RVA: 0x00100680 File Offset: 0x000FE880
		// (set) Token: 0x060026FD RID: 9981 RVA: 0x00100688 File Offset: 0x000FE888
		public LocalizedString Description { get; private set; }

		// Token: 0x17000B38 RID: 2872
		// (get) Token: 0x060026FE RID: 9982 RVA: 0x00100691 File Offset: 0x000FE891
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x17000B39 RID: 2873
		// (get) Token: 0x060026FF RID: 9983 RVA: 0x0010069E File Offset: 0x000FE89E
		public Identifier OldIdentifier { get; }

		// Token: 0x17000B3A RID: 2874
		// (get) Token: 0x06002700 RID: 9984 RVA: 0x001006A6 File Offset: 0x000FE8A6
		// (set) Token: 0x06002701 RID: 9985 RVA: 0x001006AE File Offset: 0x000FE8AE
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x17000B3B RID: 2875
		// (get) Token: 0x06002702 RID: 9986 RVA: 0x001006B7 File Offset: 0x000FE8B7
		// (set) Token: 0x06002703 RID: 9987 RVA: 0x001006BF File Offset: 0x000FE8BF
		[Header("General", null)]
		[Serialize(LevelData.LevelType.LocationConnection, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public LevelData.LevelType Type { get; set; }

		// Token: 0x17000B3C RID: 2876
		// (get) Token: 0x06002704 RID: 9988 RVA: 0x001006C8 File Offset: 0x000FE8C8
		// (set) Token: 0x06002705 RID: 9989 RVA: 0x001006D0 File Offset: 0x000FE8D0
		[Serialize(false, IsPropertySaveable.Yes, "If the given level is only used in PvP modes", "", false)]
		[Editable]
		public bool IsPvPLevel { get; set; }

		// Token: 0x17000B3D RID: 2877
		// (get) Token: 0x06002706 RID: 9990 RVA: 0x001006D9 File Offset: 0x000FE8D9
		// (set) Token: 0x06002707 RID: 9991 RVA: 0x001006E1 File Offset: 0x000FE8E1
		[Serialize(100f, IsPropertySaveable.Yes, "If there are multiple level generation parameters available for a level in a given biome, their commonness determines how likely it is for one to get selected.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float Commonness { get; set; }

		// Token: 0x17000B3E RID: 2878
		// (get) Token: 0x06002708 RID: 9992 RVA: 0x001006EA File Offset: 0x000FE8EA
		// (set) Token: 0x06002709 RID: 9993 RVA: 0x001006F2 File Offset: 0x000FE8F2
		[Serialize(false, IsPropertySaveable.Yes, "If the level is a transition from the previous biome to this one.", "", false)]
		[Editable]
		public bool TransitionFromPreviousBiome { get; set; }

		// Token: 0x17000B3F RID: 2879
		// (get) Token: 0x0600270A RID: 9994 RVA: 0x001006FB File Offset: 0x000FE8FB
		// (set) Token: 0x0600270B RID: 9995 RVA: 0x00100703 File Offset: 0x000FE903
		[Serialize(0f, IsPropertySaveable.Yes, "The difficulty of the level has to be above or equal to this for these parameters to get chosen for the level.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MinLevelDifficulty { get; set; }

		// Token: 0x17000B40 RID: 2880
		// (get) Token: 0x0600270C RID: 9996 RVA: 0x0010070C File Offset: 0x000FE90C
		// (set) Token: 0x0600270D RID: 9997 RVA: 0x00100714 File Offset: 0x000FE914
		[Serialize(100f, IsPropertySaveable.Yes, "The difficulty of the level has to be below or equal to this for these parameters to get chosen for the level.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MaxLevelDifficulty { get; set; }

		// Token: 0x17000B41 RID: 2881
		// (get) Token: 0x0600270E RID: 9998 RVA: 0x0010071D File Offset: 0x000FE91D
		// (set) Token: 0x0600270F RID: 9999 RVA: 0x00100725 File Offset: 0x000FE925
		[Header("Layout", null)]
		[Serialize("0,0", IsPropertySaveable.Yes, "Start position of the level (relative to the size of the level. 0,0 = top left corner, 1,1 = bottom right corner)", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 StartPosition
		{
			get
			{
				return this.startPosition;
			}
			set
			{
				this.startPosition = new Vector2(MathHelper.Clamp(value.X, 0f, 1f), MathHelper.Clamp(value.Y, 0f, 1f));
			}
		}

		// Token: 0x17000B42 RID: 2882
		// (get) Token: 0x06002710 RID: 10000 RVA: 0x0010075C File Offset: 0x000FE95C
		// (set) Token: 0x06002711 RID: 10001 RVA: 0x00100764 File Offset: 0x000FE964
		[Serialize("1,0", IsPropertySaveable.Yes, "End position of the level (relative to the size of the level. 0,0 = top left corner, 1,1 = bottom right corner)", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 EndPosition
		{
			get
			{
				return this.endPosition;
			}
			set
			{
				this.endPosition = new Vector2(MathHelper.Clamp(value.X, 0f, 1f), MathHelper.Clamp(value.Y, 0f, 1f));
			}
		}

		// Token: 0x17000B43 RID: 2883
		// (get) Token: 0x06002712 RID: 10002 RVA: 0x0010079B File Offset: 0x000FE99B
		// (set) Token: 0x06002713 RID: 10003 RVA: 0x001007A3 File Offset: 0x000FE9A3
		[Serialize("0,0", IsPropertySaveable.Yes, "Position of the outpost (relative to the size of the level. 0,0 = top left corner, 1,1 = bottom right corner). If set to 0,0, the outpost is placed in a suitable position automatically.", "", false)]
		[Editable(DecimalCount = 2)]
		public Vector2 ForceOutpostPosition
		{
			get
			{
				return this.forceOutpostPosition;
			}
			set
			{
				this.forceOutpostPosition = new Vector2(MathHelper.Clamp(value.X, 0f, 1f), MathHelper.Clamp(value.Y, 0f, 1f));
			}
		}

		// Token: 0x17000B44 RID: 2884
		// (get) Token: 0x06002714 RID: 10004 RVA: 0x001007DA File Offset: 0x000FE9DA
		// (set) Token: 0x06002715 RID: 10005 RVA: 0x001007E2 File Offset: 0x000FE9E2
		[Serialize(true, IsPropertySaveable.Yes, "Should there be a hole in the wall next to the end outpost (can be used to prevent players from having to backtrack if they approach the outpost from the wrong side of the main path's walls).", "", false)]
		[Editable]
		public bool CreateHoleNextToEnd { get; set; }

		// Token: 0x17000B45 RID: 2885
		// (get) Token: 0x06002716 RID: 10006 RVA: 0x001007EB File Offset: 0x000FE9EB
		// (set) Token: 0x06002717 RID: 10007 RVA: 0x001007F3 File Offset: 0x000FE9F3
		[Serialize(0.4f, IsPropertySaveable.Yes, "The probability for wall cells to be removed from the bottom of the map. A value of 0 will produce a completely enclosed tunnel and 1 will make the entire bottom of the level completely open.", "", false)]
		[Editable]
		public float BottomHoleProbability
		{
			get
			{
				return this.bottomHoleProbability;
			}
			set
			{
				this.bottomHoleProbability = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000B46 RID: 2886
		// (get) Token: 0x06002718 RID: 10008 RVA: 0x0010080B File Offset: 0x000FEA0B
		// (set) Token: 0x06002719 RID: 10009 RVA: 0x00100813 File Offset: 0x000FEA13
		[Serialize(100000, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public int MinWidth
		{
			get
			{
				return this.minWidth;
			}
			set
			{
				this.minWidth = MathHelper.Clamp(value, 2000, 1000000);
			}
		}

		// Token: 0x17000B47 RID: 2887
		// (get) Token: 0x0600271A RID: 10010 RVA: 0x0010082B File Offset: 0x000FEA2B
		// (set) Token: 0x0600271B RID: 10011 RVA: 0x00100833 File Offset: 0x000FEA33
		[Serialize(100000, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public int MaxWidth
		{
			get
			{
				return this.maxWidth;
			}
			set
			{
				this.maxWidth = MathHelper.Clamp(value, 2000, 1000000);
			}
		}

		// Token: 0x17000B48 RID: 2888
		// (get) Token: 0x0600271C RID: 10012 RVA: 0x0010084B File Offset: 0x000FEA4B
		// (set) Token: 0x0600271D RID: 10013 RVA: 0x00100853 File Offset: 0x000FEA53
		[Serialize(50000, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public int Height
		{
			get
			{
				return this.height;
			}
			set
			{
				this.height = MathHelper.Clamp(value, 2000, 1000000);
			}
		}

		// Token: 0x17000B49 RID: 2889
		// (get) Token: 0x0600271E RID: 10014 RVA: 0x0010086B File Offset: 0x000FEA6B
		// (set) Token: 0x0600271F RID: 10015 RVA: 0x00100873 File Offset: 0x000FEA73
		[Serialize(80000, IsPropertySaveable.Yes, "Minimum depth at the top of the level (100 corresponds to 1 meter).", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000000)]
		public int InitialDepthMin
		{
			get
			{
				return this.initialDepthMin;
			}
			set
			{
				this.initialDepthMin = Math.Max(value, 0);
			}
		}

		// Token: 0x17000B4A RID: 2890
		// (get) Token: 0x06002720 RID: 10016 RVA: 0x00100882 File Offset: 0x000FEA82
		// (set) Token: 0x06002721 RID: 10017 RVA: 0x0010088A File Offset: 0x000FEA8A
		[Serialize(80000, IsPropertySaveable.Yes, "Maximum depth at the top of the level (100 corresponds to 1 meter).", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000000)]
		public int InitialDepthMax
		{
			get
			{
				return this.initialDepthMax;
			}
			set
			{
				this.initialDepthMax = Math.Max(value, this.initialDepthMin);
			}
		}

		// Token: 0x17000B4B RID: 2891
		// (get) Token: 0x06002722 RID: 10018 RVA: 0x0010089E File Offset: 0x000FEA9E
		// (set) Token: 0x06002723 RID: 10019 RVA: 0x001008A6 File Offset: 0x000FEAA6
		[Header("Level geometry", null)]
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, no walls generate in the level. Can be useful for e.g. levels that are just supposed to consist of a pre-built outpost.", "", false)]
		[Editable]
		public bool NoLevelGeometry { get; set; }

		// Token: 0x17000B4C RID: 2892
		// (get) Token: 0x06002724 RID: 10020 RVA: 0x001008AF File Offset: 0x000FEAAF
		// (set) Token: 0x06002725 RID: 10021 RVA: 0x001008B8 File Offset: 0x000FEAB8
		[Editable]
		[Serialize("3000, 3000", IsPropertySaveable.Yes, "How far from each other voronoi sites are placed. Sites determine shape of the voronoi graph which the level walls are generated from. (Decreasing this value causes the number of sites, and the complexity of the level, to increase exponentially - be careful when adjusting)", "", false)]
		public Point VoronoiSiteInterval
		{
			get
			{
				return this.voronoiSiteInterval;
			}
			set
			{
				this.voronoiSiteInterval.X = MathHelper.Clamp(value.X, 100, this.MinWidth / 2);
				this.voronoiSiteInterval.Y = MathHelper.Clamp(value.Y, 100, this.height / 2);
			}
		}

		// Token: 0x17000B4D RID: 2893
		// (get) Token: 0x06002726 RID: 10022 RVA: 0x00100905 File Offset: 0x000FEB05
		// (set) Token: 0x06002727 RID: 10023 RVA: 0x0010090D File Offset: 0x000FEB0D
		[Editable]
		[Serialize("700,700", IsPropertySaveable.Yes, "How much random variation to apply to the positions of the voronoi sites on each axis. Small values produce roughly rectangular level walls. The larger the values are, the less uniform the shapes get.", "", false)]
		public Point VoronoiSiteVariance
		{
			get
			{
				return this.voronoiSiteVariance;
			}
			set
			{
				this.voronoiSiteVariance = new Point(MathHelper.Clamp(value.X, 0, this.voronoiSiteInterval.X), MathHelper.Clamp(value.Y, 0, this.voronoiSiteInterval.Y));
			}
		}

		// Token: 0x17000B4E RID: 2894
		// (get) Token: 0x06002728 RID: 10024 RVA: 0x00100948 File Offset: 0x000FEB48
		// (set) Token: 0x06002729 RID: 10025 RVA: 0x00100950 File Offset: 0x000FEB50
		[Editable(MinValueInt = 500, MaxValueInt = 10000)]
		[Serialize(5000, IsPropertySaveable.Yes, "The edges of the individual wall cells are subdivided into edges of this size. Can be used in conjunction with the rounding values to make the cells rounder. Smaller values will make the cells look smoother, but make the level more performance-intensive as the number of polygons used in rendering and physics calculations increases.", "", false)]
		public int CellSubdivisionLength
		{
			get
			{
				return this.cellSubdivisionLength;
			}
			set
			{
				this.cellSubdivisionLength = Math.Max(value, 10);
			}
		}

		// Token: 0x17000B4F RID: 2895
		// (get) Token: 0x0600272A RID: 10026 RVA: 0x00100960 File Offset: 0x000FEB60
		// (set) Token: 0x0600272B RID: 10027 RVA: 0x00100968 File Offset: 0x000FEB68
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much the individual wall cells are rounded. Note that the final shape of the cells is also affected by the CellSubdivisionLength parameter.", "", false)]
		public float CellRoundingAmount
		{
			get
			{
				return this.cellRoundingAmount;
			}
			set
			{
				this.cellRoundingAmount = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000B50 RID: 2896
		// (get) Token: 0x0600272C RID: 10028 RVA: 0x00100980 File Offset: 0x000FEB80
		// (set) Token: 0x0600272D RID: 10029 RVA: 0x00100988 File Offset: 0x000FEB88
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		[Serialize(0.1f, IsPropertySaveable.Yes, "How much random variance is applied to the edges of the cells. Note that the final shape of the cells is also affected by the CellSubdivisionLength parameter.", "", false)]
		public float CellIrregularity
		{
			get
			{
				return this.cellIrregularity;
			}
			set
			{
				this.cellIrregularity = MathHelper.Clamp(value, 0f, 1f);
			}
		}

		// Token: 0x17000B51 RID: 2897
		// (get) Token: 0x0600272E RID: 10030 RVA: 0x001009A0 File Offset: 0x000FEBA0
		// (set) Token: 0x0600272F RID: 10031 RVA: 0x001009A8 File Offset: 0x000FEBA8
		[Header("Tunnels", null)]
		[Serialize(6500, IsPropertySaveable.Yes, "Minimum width of the main tunnel going through the level, in pixels. Can be automatically increased by the level editor if the submarine is larger than this.", "", false)]
		[Editable(MinValueInt = 5000, MaxValueInt = 1000000)]
		public int MinTunnelRadius { get; set; }

		// Token: 0x17000B52 RID: 2898
		// (get) Token: 0x06002730 RID: 10032 RVA: 0x001009B1 File Offset: 0x000FEBB1
		// (set) Token: 0x06002731 RID: 10033 RVA: 0x001009B9 File Offset: 0x000FEBB9
		[Serialize("0,1", IsPropertySaveable.Yes, "Amount of side tunnels in the level (min,max).", "", false)]
		[Editable]
		public Point SideTunnelCount { get; set; }

		// Token: 0x17000B53 RID: 2899
		// (get) Token: 0x06002732 RID: 10034 RVA: 0x001009C2 File Offset: 0x000FEBC2
		// (set) Token: 0x06002733 RID: 10035 RVA: 0x001009CA File Offset: 0x000FEBCA
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much the side tunnels can \"zigzag\". 0 = completely straight tunnel, 1 = can go all the way from the top of the level to the bottom.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float SideTunnelVariance { get; set; }

		// Token: 0x17000B54 RID: 2900
		// (get) Token: 0x06002734 RID: 10036 RVA: 0x001009D3 File Offset: 0x000FEBD3
		// (set) Token: 0x06002735 RID: 10037 RVA: 0x001009DB File Offset: 0x000FEBDB
		[Serialize("2000,6000", IsPropertySaveable.Yes, "Minimum width of the side tunnels, in pixels. Unlike the main tunnel, does not get adjusted based on the size of the submarine.", "", false)]
		[Editable]
		public Point MinSideTunnelRadius { get; set; }

		// Token: 0x17000B55 RID: 2901
		// (get) Token: 0x06002736 RID: 10038 RVA: 0x001009E4 File Offset: 0x000FEBE4
		// (set) Token: 0x06002737 RID: 10039 RVA: 0x001009EC File Offset: 0x000FEBEC
		[Editable(VectorComponentLabels = new string[]
		{
			"editable.minvalue",
			"editable.maxvalue"
		})]
		[Serialize("5000, 10000", IsPropertySaveable.Yes, "The distance between the nodes that are used to generate the main path through the level (min, max). Larger values produce a straighter path.", "", false)]
		public Point MainPathNodeIntervalRange
		{
			get
			{
				return this.mainPathNodeIntervalRange;
			}
			set
			{
				this.mainPathNodeIntervalRange.X = MathHelper.Clamp(value.X, 100, this.MinWidth / 2);
				this.mainPathNodeIntervalRange.Y = MathHelper.Clamp(value.Y, this.mainPathNodeIntervalRange.X, this.MinWidth / 2);
			}
		}

		// Token: 0x17000B56 RID: 2902
		// (get) Token: 0x06002738 RID: 10040 RVA: 0x00100A42 File Offset: 0x000FEC42
		// (set) Token: 0x06002739 RID: 10041 RVA: 0x00100A4A File Offset: 0x000FEC4A
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much the side tunnels can \"zigzag\". 0 = completely straight tunnel, 1 = can go all the way from the top of the level to the bottom.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float MainPathVariance { get; set; }

		// Token: 0x17000B57 RID: 2903
		// (get) Token: 0x0600273A RID: 10042 RVA: 0x00100A53 File Offset: 0x000FEC53
		// (set) Token: 0x0600273B RID: 10043 RVA: 0x00100A5B File Offset: 0x000FEC5B
		[Header("Contents", null)]
		[Serialize(1000, IsPropertySaveable.Yes, "The total number of level objects (vegetation, vents, etc) in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 100000)]
		public int LevelObjectAmount { get; set; }

		// Token: 0x17000B58 RID: 2904
		// (get) Token: 0x0600273C RID: 10044 RVA: 0x00100A64 File Offset: 0x000FEC64
		// (set) Token: 0x0600273D RID: 10045 RVA: 0x00100A6C File Offset: 0x000FEC6C
		[Serialize(80, IsPropertySaveable.Yes, "The total number of decorative background creatures.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int BackgroundCreatureAmount { get; set; }

		// Token: 0x17000B59 RID: 2905
		// (get) Token: 0x0600273E RID: 10046 RVA: 0x00100A75 File Offset: 0x000FEC75
		// (set) Token: 0x0600273F RID: 10047 RVA: 0x00100A7D File Offset: 0x000FEC7D
		[Editable]
		[Serialize(5, IsPropertySaveable.Yes, "The number of caves placed along the main path.", "", false)]
		public int CaveCount
		{
			get
			{
				return this.caveCount;
			}
			set
			{
				this.caveCount = MathHelper.Clamp(value, 0, 100);
			}
		}

		// Token: 0x17000B5A RID: 2906
		// (get) Token: 0x06002740 RID: 10048 RVA: 0x00100A8E File Offset: 0x000FEC8E
		// (set) Token: 0x06002741 RID: 10049 RVA: 0x00100A96 File Offset: 0x000FEC96
		[Serialize(100, IsPropertySaveable.Yes, "The maximum number of level resources in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10000)]
		public int ItemCount { get; set; }

		// Token: 0x17000B5B RID: 2907
		// (get) Token: 0x06002742 RID: 10050 RVA: 0x00100A9F File Offset: 0x000FEC9F
		// (set) Token: 0x06002743 RID: 10051 RVA: 0x00100AA7 File Offset: 0x000FECA7
		[Serialize("19200,38400", IsPropertySaveable.Yes, "The minimum and maximum distance between two resource spawn points on a path.", "", false)]
		[Editable(100, 100000)]
		public Point ResourceIntervalRange { get; set; }

		// Token: 0x17000B5C RID: 2908
		// (get) Token: 0x06002744 RID: 10052 RVA: 0x00100AB0 File Offset: 0x000FECB0
		// (set) Token: 0x06002745 RID: 10053 RVA: 0x00100AB8 File Offset: 0x000FECB8
		[Serialize("9600,19200", IsPropertySaveable.Yes, "The minimum and maximum distance between two resource spawn points on a cave path.", "", false)]
		[Editable(100, 100000)]
		public Point CaveResourceIntervalRange { get; set; }

		// Token: 0x17000B5D RID: 2909
		// (get) Token: 0x06002746 RID: 10054 RVA: 0x00100AC1 File Offset: 0x000FECC1
		// (set) Token: 0x06002747 RID: 10055 RVA: 0x00100AC9 File Offset: 0x000FECC9
		[Serialize("3,6", IsPropertySaveable.Yes, "The minimum and maximum amount of resources in a single cluster. In addition to this, resource commonness affects the cluster size. Less common resources spawn in smaller clusters.", "", false)]
		[Editable(1, 20)]
		public Point ResourceClusterSizeRange { get; set; }

		// Token: 0x17000B5E RID: 2910
		// (get) Token: 0x06002748 RID: 10056 RVA: 0x00100AD2 File Offset: 0x000FECD2
		// (set) Token: 0x06002749 RID: 10057 RVA: 0x00100ADA File Offset: 0x000FECDA
		[Serialize(0.3f, IsPropertySaveable.Yes, "How likely a resource spawn point on a path is to contain resources.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float ResourceSpawnChance { get; set; }

		// Token: 0x17000B5F RID: 2911
		// (get) Token: 0x0600274A RID: 10058 RVA: 0x00100AE3 File Offset: 0x000FECE3
		// (set) Token: 0x0600274B RID: 10059 RVA: 0x00100AEB File Offset: 0x000FECEB
		[Serialize(1f, IsPropertySaveable.Yes, "How likely a resource spawn point on a cave path is to contain resources.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float CaveResourceSpawnChance { get; set; }

		// Token: 0x17000B60 RID: 2912
		// (get) Token: 0x0600274C RID: 10060 RVA: 0x00100AF4 File Offset: 0x000FECF4
		// (set) Token: 0x0600274D RID: 10061 RVA: 0x00100AFC File Offset: 0x000FECFC
		[Serialize(0, IsPropertySaveable.Yes, "Number of floating, destructible ice chunks in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int FloatingIceChunkCount { get; set; }

		// Token: 0x17000B61 RID: 2913
		// (get) Token: 0x0600274E RID: 10062 RVA: 0x00100B05 File Offset: 0x000FED05
		// (set) Token: 0x0600274F RID: 10063 RVA: 0x00100B0D File Offset: 0x000FED0D
		[Serialize(0, IsPropertySaveable.Yes, "Number of islands (static wall chunks along the main path) in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 100)]
		public int IslandCount { get; set; }

		// Token: 0x17000B62 RID: 2914
		// (get) Token: 0x06002750 RID: 10064 RVA: 0x00100B16 File Offset: 0x000FED16
		// (set) Token: 0x06002751 RID: 10065 RVA: 0x00100B1E File Offset: 0x000FED1E
		[Serialize(0, IsPropertySaveable.Yes, "Number of ice spires in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int IceSpireCount { get; set; }

		// Token: 0x17000B63 RID: 2915
		// (get) Token: 0x06002752 RID: 10066 RVA: 0x00100B27 File Offset: 0x000FED27
		// (set) Token: 0x06002753 RID: 10067 RVA: 0x00100B2F File Offset: 0x000FED2F
		[Header("Abyss", null)]
		[Serialize(true, IsPropertySaveable.Yes, "Should the generator force a hole to the bottom of the level to ensure there's a way to the abyss.", "", false)]
		[Editable]
		public bool CreateHoleToAbyss { get; set; }

		// Token: 0x17000B64 RID: 2916
		// (get) Token: 0x06002754 RID: 10068 RVA: 0x00100B38 File Offset: 0x000FED38
		// (set) Token: 0x06002755 RID: 10069 RVA: 0x00100B40 File Offset: 0x000FED40
		[Serialize(5, IsPropertySaveable.Yes, "Number of abyss islands in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int AbyssIslandCount { get; set; }

		// Token: 0x17000B65 RID: 2917
		// (get) Token: 0x06002756 RID: 10070 RVA: 0x00100B49 File Offset: 0x000FED49
		// (set) Token: 0x06002757 RID: 10071 RVA: 0x00100B51 File Offset: 0x000FED51
		[Serialize("4000,7000", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Point AbyssIslandSizeMin { get; set; }

		// Token: 0x17000B66 RID: 2918
		// (get) Token: 0x06002758 RID: 10072 RVA: 0x00100B5A File Offset: 0x000FED5A
		// (set) Token: 0x06002759 RID: 10073 RVA: 0x00100B62 File Offset: 0x000FED62
		[Serialize("8000,10000", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Point AbyssIslandSizeMax { get; set; }

		// Token: 0x17000B67 RID: 2919
		// (get) Token: 0x0600275A RID: 10074 RVA: 0x00100B6B File Offset: 0x000FED6B
		// (set) Token: 0x0600275B RID: 10075 RVA: 0x00100B73 File Offset: 0x000FED73
		[Serialize(0.5f, IsPropertySaveable.Yes, "The probability of an abyss island having a cave. There is always a cave in at least one of the islands regardless of this setting.", "", false)]
		[Editable]
		public float AbyssIslandCaveProbability { get; set; }

		// Token: 0x17000B68 RID: 2920
		// (get) Token: 0x0600275C RID: 10076 RVA: 0x00100B7C File Offset: 0x000FED7C
		// (set) Token: 0x0600275D RID: 10077 RVA: 0x00100B84 File Offset: 0x000FED84
		[Serialize(10, IsPropertySaveable.Yes, "Minimum number of resource clusters in the abyss (the actual number is picked between min and max according to the level difficulty)", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int AbyssResourceClustersMin { get; set; }

		// Token: 0x17000B69 RID: 2921
		// (get) Token: 0x0600275E RID: 10078 RVA: 0x00100B8D File Offset: 0x000FED8D
		// (set) Token: 0x0600275F RID: 10079 RVA: 0x00100B95 File Offset: 0x000FED95
		[Serialize(40, IsPropertySaveable.Yes, "Maximum number of resource clusters in the abyss (the actual number is picked between min and max according to the level difficulty)", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int AbyssResourceClustersMax { get; set; }

		// Token: 0x17000B6A RID: 2922
		// (get) Token: 0x06002760 RID: 10080 RVA: 0x00100B9E File Offset: 0x000FED9E
		// (set) Token: 0x06002761 RID: 10081 RVA: 0x00100BA6 File Offset: 0x000FEDA6
		[Header("Sea floor", null)]
		[Serialize(-300000, IsPropertySaveable.Yes, "How far below the level the sea floor is placed.", "", false)]
		[Editable(MinValueFloat = -1000000f, MaxValueFloat = 0f)]
		public int SeaFloorDepth
		{
			get
			{
				return this.seaFloorBaseDepth;
			}
			set
			{
				this.seaFloorBaseDepth = MathHelper.Clamp(value, -1000000, 0);
			}
		}

		// Token: 0x17000B6B RID: 2923
		// (get) Token: 0x06002762 RID: 10082 RVA: 0x00100BBA File Offset: 0x000FEDBA
		// (set) Token: 0x06002763 RID: 10083 RVA: 0x00100BC2 File Offset: 0x000FEDC2
		[Serialize(1000, IsPropertySaveable.Yes, "Variance of the depth of the sea floor. Smaller values produce a smoother sea floor.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100000f)]
		public int SeaFloorVariance
		{
			get
			{
				return this.seaFloorVariance;
			}
			set
			{
				this.seaFloorVariance = value;
			}
		}

		// Token: 0x17000B6C RID: 2924
		// (get) Token: 0x06002764 RID: 10084 RVA: 0x00100BCB File Offset: 0x000FEDCB
		// (set) Token: 0x06002765 RID: 10085 RVA: 0x00100BD3 File Offset: 0x000FEDD3
		[Serialize(0, IsPropertySaveable.Yes, "The minimum number of mountains on the sea floor.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int MountainCountMin
		{
			get
			{
				return this.mountainCountMin;
			}
			set
			{
				this.mountainCountMin = Math.Max(value, 0);
			}
		}

		// Token: 0x17000B6D RID: 2925
		// (get) Token: 0x06002766 RID: 10086 RVA: 0x00100BE2 File Offset: 0x000FEDE2
		// (set) Token: 0x06002767 RID: 10087 RVA: 0x00100BEA File Offset: 0x000FEDEA
		[Serialize(0, IsPropertySaveable.Yes, "The maximum number of mountains on the sea floor.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int MountainCountMax
		{
			get
			{
				return this.mountainCountMax;
			}
			set
			{
				this.mountainCountMax = Math.Max(value, 0);
			}
		}

		// Token: 0x17000B6E RID: 2926
		// (get) Token: 0x06002768 RID: 10088 RVA: 0x00100BF9 File Offset: 0x000FEDF9
		// (set) Token: 0x06002769 RID: 10089 RVA: 0x00100C01 File Offset: 0x000FEE01
		[Serialize(1000, IsPropertySaveable.Yes, "The minimum height of the mountains on the sea floor.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000000)]
		public int MountainHeightMin
		{
			get
			{
				return this.mountainHeightMin;
			}
			set
			{
				this.mountainHeightMin = Math.Max(value, 0);
			}
		}

		// Token: 0x17000B6F RID: 2927
		// (get) Token: 0x0600276A RID: 10090 RVA: 0x00100C10 File Offset: 0x000FEE10
		// (set) Token: 0x0600276B RID: 10091 RVA: 0x00100C18 File Offset: 0x000FEE18
		[Serialize(5000, IsPropertySaveable.Yes, "The maximum height of the mountains on the sea floor.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000000)]
		public int MountainHeightMax
		{
			get
			{
				return this.mountainHeightMax;
			}
			set
			{
				this.mountainHeightMax = Math.Max(value, 0);
			}
		}

		// Token: 0x0600276C RID: 10092 RVA: 0x00100C27 File Offset: 0x000FEE27
		public bool UseRandomRuinCount()
		{
			return this.MinRuinCount >= 0 && this.MaxRuinCount > 0;
		}

		// Token: 0x0600276D RID: 10093 RVA: 0x00100C3D File Offset: 0x000FEE3D
		public int GetMaxRuinCount()
		{
			if (!this.UseRandomRuinCount())
			{
				return this.RuinCount;
			}
			return this.MaxRuinCount;
		}

		// Token: 0x17000B70 RID: 2928
		// (get) Token: 0x0600276E RID: 10094 RVA: 0x00100C54 File Offset: 0x000FEE54
		// (set) Token: 0x0600276F RID: 10095 RVA: 0x00100C5C File Offset: 0x000FEE5C
		[Header("Ruins", null)]
		[Serialize(1, IsPropertySaveable.Yes, "The number of alien ruins in the level. Ignored, if both MinRuinCount and MaxRuinCount are defined.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int RuinCount { get; set; }

		// Token: 0x17000B71 RID: 2929
		// (get) Token: 0x06002770 RID: 10096 RVA: 0x00100C65 File Offset: 0x000FEE65
		// (set) Token: 0x06002771 RID: 10097 RVA: 0x00100C6D File Offset: 0x000FEE6D
		[Serialize(0, IsPropertySaveable.Yes, "The minimum number of alien ruins in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MinRuinCount { get; set; }

		// Token: 0x17000B72 RID: 2930
		// (get) Token: 0x06002772 RID: 10098 RVA: 0x00100C76 File Offset: 0x000FEE76
		// (set) Token: 0x06002773 RID: 10099 RVA: 0x00100C7E File Offset: 0x000FEE7E
		[Serialize(0, IsPropertySaveable.Yes, "The maximum number of alien ruins in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MaxRuinCount { get; set; }

		// Token: 0x17000B73 RID: 2931
		// (get) Token: 0x06002774 RID: 10100 RVA: 0x00100C87 File Offset: 0x000FEE87
		// (set) Token: 0x06002775 RID: 10101 RVA: 0x00100C8F File Offset: 0x000FEE8F
		[Serialize(1f, IsPropertySaveable.Yes, "The probability of spawning a ruin in the level. If the level can have multiple ruins, the probability is evaluated separately for each.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float RuinSpawnProbability { get; set; }

		// Token: 0x17000B74 RID: 2932
		// (get) Token: 0x06002776 RID: 10102 RVA: 0x00100C98 File Offset: 0x000FEE98
		// (set) Token: 0x06002777 RID: 10103 RVA: 0x00100CA0 File Offset: 0x000FEEA0
		[Header("Wrecks", null)]
		[Serialize(1, IsPropertySaveable.Yes, "The minimum number of wrecks in the level. Note that this value cannot be higher than the amount of wreck prefabs (subs).", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MinWreckCount { get; set; }

		// Token: 0x17000B75 RID: 2933
		// (get) Token: 0x06002778 RID: 10104 RVA: 0x00100CA9 File Offset: 0x000FEEA9
		// (set) Token: 0x06002779 RID: 10105 RVA: 0x00100CB1 File Offset: 0x000FEEB1
		[Serialize(1, IsPropertySaveable.Yes, "The maximum number of wrecks in the level. Note that this value cannot be higher than the amount of wreck prefabs (subs).", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MaxWreckCount { get; set; }

		// Token: 0x17000B76 RID: 2934
		// (get) Token: 0x0600277A RID: 10106 RVA: 0x00100CBA File Offset: 0x000FEEBA
		// (set) Token: 0x0600277B RID: 10107 RVA: 0x00100CC2 File Offset: 0x000FEEC2
		[Serialize(1, IsPropertySaveable.Yes, "The minimum number of corpses per wreck.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int MinCorpseCount { get; set; }

		// Token: 0x17000B77 RID: 2935
		// (get) Token: 0x0600277C RID: 10108 RVA: 0x00100CCB File Offset: 0x000FEECB
		// (set) Token: 0x0600277D RID: 10109 RVA: 0x00100CD3 File Offset: 0x000FEED3
		[Serialize(5, IsPropertySaveable.Yes, "The maximum number of corpses per wreck.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int MaxCorpseCount { get; set; }

		// Token: 0x17000B78 RID: 2936
		// (get) Token: 0x0600277E RID: 10110 RVA: 0x00100CDC File Offset: 0x000FEEDC
		// (set) Token: 0x0600277F RID: 10111 RVA: 0x00100CE4 File Offset: 0x000FEEE4
		[Serialize(0f, IsPropertySaveable.Yes, "How likely is it that a character set to be spawned as a corpse spawns as a human husk instead? Percentage from 0 to 1 per character.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float HuskProbability { get; set; }

		// Token: 0x17000B79 RID: 2937
		// (get) Token: 0x06002780 RID: 10112 RVA: 0x00100CED File Offset: 0x000FEEED
		// (set) Token: 0x06002781 RID: 10113 RVA: 0x00100CF5 File Offset: 0x000FEEF5
		[Serialize(0f, IsPropertySaveable.Yes, "How likely is it that a Thalamus inhabits a wreck. Percentage from 0 to 1 per wreck.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float ThalamusProbability { get; set; }

		// Token: 0x17000B7A RID: 2938
		// (get) Token: 0x06002782 RID: 10114 RVA: 0x00100CFE File Offset: 0x000FEEFE
		// (set) Token: 0x06002783 RID: 10115 RVA: 0x00100D06 File Offset: 0x000FEF06
		[Serialize(0.5f, IsPropertySaveable.Yes, "How likely the water level of a hull inside a wreck is randomly set.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float WreckHullFloodingChance { get; set; }

		// Token: 0x17000B7B RID: 2939
		// (get) Token: 0x06002784 RID: 10116 RVA: 0x00100D0F File Offset: 0x000FEF0F
		// (set) Token: 0x06002785 RID: 10117 RVA: 0x00100D17 File Offset: 0x000FEF17
		[Serialize(0.1f, IsPropertySaveable.Yes, "The min water percentage of randomly flooding hulls in wrecks.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float WreckFloodingHullMinWaterPercentage { get; set; }

		// Token: 0x17000B7C RID: 2940
		// (get) Token: 0x06002786 RID: 10118 RVA: 0x00100D20 File Offset: 0x000FEF20
		// (set) Token: 0x06002787 RID: 10119 RVA: 0x00100D28 File Offset: 0x000FEF28
		[Serialize(1f, IsPropertySaveable.Yes, "The min water percentage of randomly flooding hulls in wrecks.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float WreckFloodingHullMaxWaterPercentage { get; set; }

		// Token: 0x17000B7D RID: 2941
		// (get) Token: 0x06002788 RID: 10120 RVA: 0x00100D31 File Offset: 0x000FEF31
		// (set) Token: 0x06002789 RID: 10121 RVA: 0x00100D39 File Offset: 0x000FEF39
		[Serialize("", IsPropertySaveable.Yes, "Should a beacon station always spawn in this type of level?", "", false)]
		public string ForceBeaconStation { get; set; }

		// Token: 0x17000B7E RID: 2942
		// (get) Token: 0x0600278A RID: 10122 RVA: 0x00100D42 File Offset: 0x000FEF42
		// (set) Token: 0x0600278B RID: 10123 RVA: 0x00100D4A File Offset: 0x000FEF4A
		[Header("Visuals", null)]
		[Serialize(1f, IsPropertySaveable.Yes, "Scale of the water particle texture.", "", false)]
		[Editable]
		public float WaterParticleScale
		{
			get
			{
				return this.waterParticleScale;
			}
			private set
			{
				this.waterParticleScale = Math.Max(value, 0.01f);
			}
		}

		// Token: 0x17000B7F RID: 2943
		// (get) Token: 0x0600278C RID: 10124 RVA: 0x00100D5D File Offset: 0x000FEF5D
		// (set) Token: 0x0600278D RID: 10125 RVA: 0x00100D65 File Offset: 0x000FEF65
		[Serialize("0,10", IsPropertySaveable.Yes, "How fast the water particle texture scrolls.", "", false)]
		[Editable]
		public Vector2 WaterParticleVelocity
		{
			get
			{
				return this.waterParticleVelocity;
			}
			private set
			{
				this.waterParticleVelocity = value;
			}
		}

		// Token: 0x17000B80 RID: 2944
		// (get) Token: 0x0600278E RID: 10126 RVA: 0x00100D6E File Offset: 0x000FEF6E
		// (set) Token: 0x0600278F RID: 10127 RVA: 0x00100D76 File Offset: 0x000FEF76
		[Serialize(2048f, IsPropertySaveable.Yes, "Size of the level wall texture.", "", false)]
		[Editable(10f, 10000f, 1)]
		public float WallTextureSize { get; private set; }

		// Token: 0x17000B81 RID: 2945
		// (get) Token: 0x06002790 RID: 10128 RVA: 0x00100D7F File Offset: 0x000FEF7F
		// (set) Token: 0x06002791 RID: 10129 RVA: 0x00100D87 File Offset: 0x000FEF87
		[Serialize(2048f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(10f, 10000f, 1)]
		public float WallEdgeTextureWidth { get; private set; }

		// Token: 0x17000B82 RID: 2946
		// (get) Token: 0x06002792 RID: 10130 RVA: 0x00100D90 File Offset: 0x000FEF90
		// (set) Token: 0x06002793 RID: 10131 RVA: 0x00100D98 File Offset: 0x000FEF98
		[Serialize("0,0", IsPropertySaveable.Yes, "Interval of lightning-like flashes of light in the level.", "", false)]
		[Editable]
		public Vector2 FlashInterval { get; set; }

		// Token: 0x17000B83 RID: 2947
		// (get) Token: 0x06002794 RID: 10132 RVA: 0x00100DA1 File Offset: 0x000FEFA1
		// (set) Token: 0x06002795 RID: 10133 RVA: 0x00100DA9 File Offset: 0x000FEFA9
		[Serialize("0,0,0,0", IsPropertySaveable.Yes, "Color of lightning-like flashes of light in the level.", "", false)]
		[Editable]
		public Color FlashColor { get; set; }

		// Token: 0x17000B84 RID: 2948
		// (get) Token: 0x06002796 RID: 10134 RVA: 0x00100DB2 File Offset: 0x000FEFB2
		// (set) Token: 0x06002797 RID: 10135 RVA: 0x00100DBA File Offset: 0x000FEFBA
		[Serialize(120f, IsPropertySaveable.Yes, "How far the level walls' edge texture portrudes outside the actual, \"physical\" edge of the cell.", "", false)]
		[Editable(0f, 1000f, 1)]
		public float WallEdgeExpandOutwardsAmount { get; private set; }

		// Token: 0x17000B85 RID: 2949
		// (get) Token: 0x06002798 RID: 10136 RVA: 0x00100DC3 File Offset: 0x000FEFC3
		// (set) Token: 0x06002799 RID: 10137 RVA: 0x00100DCB File Offset: 0x000FEFCB
		[Serialize(1000f, IsPropertySaveable.Yes, "How far inside the level walls the edge texture continues.", "", false)]
		[Editable(0f, 10000f, 1)]
		public float WallEdgeExpandInwardsAmount { get; private set; }

		// Token: 0x17000B86 RID: 2950
		// (get) Token: 0x0600279A RID: 10138 RVA: 0x00100DD4 File Offset: 0x000FEFD4
		// (set) Token: 0x0600279B RID: 10139 RVA: 0x00100DDC File Offset: 0x000FEFDC
		[Serialize(1000f, IsPropertySaveable.Yes, "How deep inside the walls the wall texture extends to before fading to black.", "", false)]
		[Editable(0f, 10000f, 1)]
		public float WallTextureExpandInwardsAmount { get; private set; }

		// Token: 0x17000B87 RID: 2951
		// (get) Token: 0x0600279C RID: 10140 RVA: 0x00100DE5 File Offset: 0x000FEFE5
		// (set) Token: 0x0600279D RID: 10141 RVA: 0x00100DED File Offset: 0x000FEFED
		[Header("Colors", null)]
		[Serialize("27,30,36", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color AmbientLightColor { get; set; }

		// Token: 0x17000B88 RID: 2952
		// (get) Token: 0x0600279E RID: 10142 RVA: 0x00100DF6 File Offset: 0x000FEFF6
		// (set) Token: 0x0600279F RID: 10143 RVA: 0x00100DFE File Offset: 0x000FEFFE
		[Serialize("20,40,50", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color BackgroundTextureColor { get; set; }

		// Token: 0x17000B89 RID: 2953
		// (get) Token: 0x060027A0 RID: 10144 RVA: 0x00100E07 File Offset: 0x000FF007
		// (set) Token: 0x060027A1 RID: 10145 RVA: 0x00100E0F File Offset: 0x000FF00F
		[Serialize("20,40,50", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color BackgroundColor { get; set; }

		// Token: 0x17000B8A RID: 2954
		// (get) Token: 0x060027A2 RID: 10146 RVA: 0x00100E18 File Offset: 0x000FF018
		// (set) Token: 0x060027A3 RID: 10147 RVA: 0x00100E20 File Offset: 0x000FF020
		[Serialize("255,255,255", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color WallColor { get; set; }

		// Token: 0x17000B8B RID: 2955
		// (get) Token: 0x060027A4 RID: 10148 RVA: 0x00100E29 File Offset: 0x000FF029
		// (set) Token: 0x060027A5 RID: 10149 RVA: 0x00100E31 File Offset: 0x000FF031
		[Serialize("255,255,255", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color WaterParticleColor { get; set; }

		// Token: 0x17000B8C RID: 2956
		// (get) Token: 0x060027A6 RID: 10150 RVA: 0x00100E3A File Offset: 0x000FF03A
		// (set) Token: 0x060027A7 RID: 10151 RVA: 0x00100E42 File Offset: 0x000FF042
		[Header("Sounds", null)]
		[Serialize(false, IsPropertySaveable.Yes, "Should the \"ambient noise\" of the biome play in this level if it's an outpost level.", "", false)]
		[Editable]
		public bool PlayNoiseLoopInOutpostLevel { get; set; }

		// Token: 0x17000B8D RID: 2957
		// (get) Token: 0x060027A8 RID: 10152 RVA: 0x00100E4B File Offset: 0x000FF04B
		// (set) Token: 0x060027A9 RID: 10153 RVA: 0x00100E53 File Offset: 0x000FF053
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float WaterAmbienceVolume { get; set; }

		// Token: 0x17000B8E RID: 2958
		// (get) Token: 0x060027AA RID: 10154 RVA: 0x00100E5C File Offset: 0x000FF05C
		// (set) Token: 0x060027AB RID: 10155 RVA: 0x00100E64 File Offset: 0x000FF064
		public Sprite BackgroundSprite { get; private set; }

		// Token: 0x17000B8F RID: 2959
		// (get) Token: 0x060027AC RID: 10156 RVA: 0x00100E6D File Offset: 0x000FF06D
		// (set) Token: 0x060027AD RID: 10157 RVA: 0x00100E75 File Offset: 0x000FF075
		public Sprite BackgroundTopSprite { get; private set; }

		// Token: 0x17000B90 RID: 2960
		// (get) Token: 0x060027AE RID: 10158 RVA: 0x00100E7E File Offset: 0x000FF07E
		// (set) Token: 0x060027AF RID: 10159 RVA: 0x00100E86 File Offset: 0x000FF086
		public Sprite WallSprite { get; private set; }

		// Token: 0x17000B91 RID: 2961
		// (get) Token: 0x060027B0 RID: 10160 RVA: 0x00100E8F File Offset: 0x000FF08F
		// (set) Token: 0x060027B1 RID: 10161 RVA: 0x00100E97 File Offset: 0x000FF097
		public Sprite WallEdgeSprite { get; private set; }

		// Token: 0x17000B92 RID: 2962
		// (get) Token: 0x060027B2 RID: 10162 RVA: 0x00100EA0 File Offset: 0x000FF0A0
		// (set) Token: 0x060027B3 RID: 10163 RVA: 0x00100EA8 File Offset: 0x000FF0A8
		public Sprite DestructibleWallSprite { get; private set; }

		// Token: 0x17000B93 RID: 2963
		// (get) Token: 0x060027B4 RID: 10164 RVA: 0x00100EB1 File Offset: 0x000FF0B1
		// (set) Token: 0x060027B5 RID: 10165 RVA: 0x00100EB9 File Offset: 0x000FF0B9
		public Sprite DestructibleWallEdgeSprite { get; private set; }

		// Token: 0x17000B94 RID: 2964
		// (get) Token: 0x060027B6 RID: 10166 RVA: 0x00100EC2 File Offset: 0x000FF0C2
		// (set) Token: 0x060027B7 RID: 10167 RVA: 0x00100ECA File Offset: 0x000FF0CA
		public Sprite WallSpriteDestroyed { get; private set; }

		// Token: 0x17000B95 RID: 2965
		// (get) Token: 0x060027B8 RID: 10168 RVA: 0x00100ED3 File Offset: 0x000FF0D3
		// (set) Token: 0x060027B9 RID: 10169 RVA: 0x00100EDB File Offset: 0x000FF0DB
		public Sprite WaterParticles { get; private set; }

		// Token: 0x060027BA RID: 10170 RVA: 0x00100EE4 File Offset: 0x000FF0E4
		public static void CheckValidity()
		{
			foreach (Biome biome in Biome.Prefabs)
			{
				for (float i = 0f; i <= 100f; i += 0.5f)
				{
					if (LevelGenerationParams.GetRandom("test", LevelData.LevelType.LocationConnection, i, biome.Identifier, false, false) == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(130, 2);
						defaultInterpolatedStringHandler.AppendLiteral("No suitable level generation parameters found for a specific type of level (level type: LocationConnection, difficulty: ");
						defaultInterpolatedStringHandler.AppendFormatted<float>(i);
						defaultInterpolatedStringHandler.AppendLiteral(", biome: ");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(biome.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral(")");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					}
					if (LevelGenerationParams.GetRandom("test", LevelData.LevelType.Outpost, i, biome.Identifier, false, false) == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(119, 2);
						defaultInterpolatedStringHandler2.AppendLiteral("No suitable level generation parameters found for a specific type of level (level type: Outpost, difficulty: ");
						defaultInterpolatedStringHandler2.AppendFormatted<float>(i);
						defaultInterpolatedStringHandler2.AppendLiteral(", biome: ");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(biome.Identifier);
						defaultInterpolatedStringHandler2.AppendLiteral(")");
						DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
					}
				}
			}
		}

		// Token: 0x060027BB RID: 10171 RVA: 0x00101030 File Offset: 0x000FF230
		public static LevelGenerationParams GetRandom(string seed, LevelData.LevelType type, float difficulty, Identifier biomeId = default(Identifier), bool pvpOnly = false, bool biomeTransition = false)
		{
			LevelGenerationParams.<>c__DisplayClass387_0 CS$<>8__locals1 = new LevelGenerationParams.<>c__DisplayClass387_0();
			CS$<>8__locals1.type = type;
			CS$<>8__locals1.biomeId = biomeId;
			CS$<>8__locals1.difficulty = difficulty;
			Rand.SetSyncedSeed(ToolBox.StringToInt(seed));
			if (!LevelGenerationParams.LevelParams.Any<LevelGenerationParams>())
			{
				throw new InvalidOperationException("Level generation presets not found - using default presets");
			}
			IOrderedEnumerable<LevelGenerationParams> levelParamsOrdered = from l in LevelGenerationParams.LevelParams
			orderby l.UintIdentifier
			select l;
			IEnumerable<LevelGenerationParams> matchingLevelParams = from lp in levelParamsOrdered
			where lp.Type == CS$<>8__locals1.type && (lp.AnyBiomeAllowed || lp.AllowedBiomeIdentifiers.Any<Identifier>()) && !lp.AllowedBiomeIdentifiers.Contains("None".ToIdentifier())
			select lp;
			if (biomeTransition)
			{
				IEnumerable<LevelGenerationParams> biomeTransitionParams = from lp in matchingLevelParams
				where lp.TransitionFromPreviousBiome && lp.AllowedBiomeIdentifiers.Contains(CS$<>8__locals1.biomeId)
				select lp;
				if (biomeTransitionParams.Any<LevelGenerationParams>())
				{
					return ToolBox.SelectWeightedRandom<LevelGenerationParams>(biomeTransitionParams, (LevelGenerationParams p) => p.Commonness, Rand.RandSync.ServerAndClient);
				}
			}
			else
			{
				matchingLevelParams = from lp in matchingLevelParams
				where !lp.TransitionFromPreviousBiome
				select lp;
			}
			if (pvpOnly)
			{
				IEnumerable<LevelGenerationParams> pvpOnlyLevels = from lp in matchingLevelParams
				where lp.IsPvPLevel
				select lp;
				if (pvpOnlyLevels.Any<LevelGenerationParams>())
				{
					matchingLevelParams = pvpOnlyLevels;
				}
				else
				{
					DebugConsole.AddWarning("No PvP specific level generation presets found - using all level generation presets instead.", null);
				}
			}
			else
			{
				matchingLevelParams = from lp in matchingLevelParams
				where !lp.IsPvPLevel
				select lp;
			}
			if (CS$<>8__locals1.biomeId.IsEmpty || CS$<>8__locals1.biomeId == "Random")
			{
				matchingLevelParams = matchingLevelParams.Where(delegate(LevelGenerationParams lp)
				{
					if (!lp.AnyBiomeAllowed)
					{
						return !lp.AllowedBiomeIdentifiers.All((Identifier b) => Biome.Prefabs[b].IsEndBiome);
					}
					return true;
				});
			}
			else
			{
				Biome biome;
				bool isEndBiome = Biome.Prefabs.TryGet(CS$<>8__locals1.biomeId, out biome) && biome.IsEndBiome;
				if (isEndBiome && matchingLevelParams.Any((LevelGenerationParams lp) => lp.AllowedBiomeIdentifiers.Contains(CS$<>8__locals1.biomeId)))
				{
					matchingLevelParams = from lp in matchingLevelParams
					where lp.AllowedBiomeIdentifiers.Contains(CS$<>8__locals1.biomeId)
					select lp;
				}
				else
				{
					matchingLevelParams = from lp in matchingLevelParams
					where lp.AnyBiomeAllowed || lp.AllowedBiomeIdentifiers.Contains(CS$<>8__locals1.biomeId)
					select lp;
				}
			}
			if (!matchingLevelParams.Any<LevelGenerationParams>())
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(64, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Suitable level generation presets not found (biome \"");
				LevelGenerationParams.<>c__DisplayClass387_0 CS$<>8__locals2 = CS$<>8__locals1;
				Identifier identifier = "null".ToIdentifier();
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(CS$<>8__locals2.biomeId.IfEmpty(identifier));
				defaultInterpolatedStringHandler.AppendLiteral("\", type: \"");
				defaultInterpolatedStringHandler.AppendFormatted<LevelData.LevelType>(CS$<>8__locals1.type);
				defaultInterpolatedStringHandler.AppendLiteral("\")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				if (!CS$<>8__locals1.biomeId.IsEmpty)
				{
					matchingLevelParams = from lp in levelParamsOrdered
					where lp.Type == CS$<>8__locals1.type
					select lp;
					if (!matchingLevelParams.Any<LevelGenerationParams>())
					{
						matchingLevelParams = levelParamsOrdered;
					}
				}
			}
			if (!matchingLevelParams.Any((LevelGenerationParams lp) => CS$<>8__locals1.difficulty >= lp.MinLevelDifficulty && CS$<>8__locals1.difficulty <= lp.MaxLevelDifficulty))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(78, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Suitable level generation presets not found (biome \"");
				LevelGenerationParams.<>c__DisplayClass387_0 CS$<>8__locals3 = CS$<>8__locals1;
				Identifier identifier = "null".ToIdentifier();
				defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(CS$<>8__locals3.biomeId.IfEmpty(identifier));
				defaultInterpolatedStringHandler2.AppendLiteral("\", type: \"");
				defaultInterpolatedStringHandler2.AppendFormatted<LevelData.LevelType>(CS$<>8__locals1.type);
				defaultInterpolatedStringHandler2.AppendLiteral("\", difficulty: ");
				defaultInterpolatedStringHandler2.AppendFormatted<float>(CS$<>8__locals1.difficulty);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
			}
			else
			{
				matchingLevelParams = from lp in matchingLevelParams
				where CS$<>8__locals1.difficulty >= lp.MinLevelDifficulty && CS$<>8__locals1.difficulty <= lp.MaxLevelDifficulty
				select lp;
			}
			return ToolBox.SelectWeightedRandom<LevelGenerationParams>(matchingLevelParams, (LevelGenerationParams p) => p.Commonness, Rand.RandSync.ServerAndClient);
		}

		// Token: 0x060027BC RID: 10172 RVA: 0x001013AC File Offset: 0x000FF5AC
		public LevelGenerationParams(ContentXElement element, LevelGenerationParametersFile file) : base(file, element.GetAttributeIdentifier("identifier", element.Name.LocalName))
		{
			this.OldIdentifier = element.GetAttributeIdentifier("oldidentifier", Identifier.Empty);
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			if (element == null)
			{
				throw new ArgumentNullException("element is null");
			}
			HashSet<Identifier> allowedBiomeIdentifiers = element.GetAttributeIdentifierArray("biomes", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
			this.AnyBiomeAllowed = allowedBiomeIdentifiers.Contains("any".ToIdentifier());
			allowedBiomeIdentifiers.Remove("any".ToIdentifier());
			this.AllowedBiomeIdentifiers = allowedBiomeIdentifiers.ToImmutableHashSet<Identifier>();
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(10, 1);
			defaultInterpolatedStringHandler.AppendLiteral("levelname.");
			defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.Identifier);
			this.DisplayName = TextManager.Get(defaultInterpolatedStringHandler.ToStringAndClear());
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(17, 1);
			defaultInterpolatedStringHandler2.AppendLiteral("leveldescription.");
			defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(this.Identifier);
			this.Description = TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear());
			Identifier nameIdentifier = element.GetAttributeIdentifier("nameidentifier", Identifier.Empty);
			Identifier descriptionIdentifier = element.GetAttributeIdentifier("descriptionidentifier", Identifier.Empty);
			if (!nameIdentifier.IsEmpty)
			{
				this.DisplayName = TextManager.Get(nameIdentifier);
			}
			if (!descriptionIdentifier.IsEmpty)
			{
				this.Description = TextManager.Get(descriptionIdentifier);
			}
			foreach (ContentXElement subElement in element.Elements())
			{
				string text = subElement.Name.ToString().ToLowerInvariant();
				if (text != null)
				{
					int length = text.Length;
					if (length != 4)
					{
						switch (length)
						{
						case 8:
							if (text == "walledge")
							{
								this.WallEdgeSprite = new Sprite(subElement, "", "", false, 1f);
							}
							break;
						case 9:
						case 11:
						case 12:
						case 15:
							break;
						case 10:
							if (text == "background")
							{
								this.BackgroundSprite = new Sprite(subElement, "", "", false, 1f);
							}
							break;
						case 13:
						{
							char c = text[0];
							if (c != 'b')
							{
								if (c == 'w')
								{
									if (text == "walldestroyed")
									{
										this.WallSpriteDestroyed = new Sprite(subElement, "", "", false, 1f);
									}
								}
							}
							else if (text == "backgroundtop")
							{
								this.BackgroundTopSprite = new Sprite(subElement, "", "", false, 1f);
							}
							break;
						}
						case 14:
							if (text == "waterparticles")
							{
								this.WaterParticles = new Sprite(subElement, "", "", false, 1f);
							}
							break;
						case 16:
							if (text == "destructiblewall")
							{
								this.DestructibleWallSprite = new Sprite(subElement, "", "", false, 1f);
							}
							break;
						default:
							if (length == 20)
							{
								if (text == "destructiblewalledge")
								{
									this.DestructibleWallEdgeSprite = new Sprite(subElement, "", "", false, 1f);
								}
							}
							break;
						}
					}
					else if (text == "wall")
					{
						this.WallSprite = new Sprite(subElement, "", "", false, 1f);
					}
				}
			}
		}

		// Token: 0x060027BD RID: 10173 RVA: 0x00101790 File Offset: 0x000FF990
		public override void Dispose()
		{
		}

		// Token: 0x04001320 RID: 4896
		public static readonly PrefabCollection<LevelGenerationParams> LevelParams = new PrefabCollection<LevelGenerationParams>();

		// Token: 0x04001324 RID: 4900
		private int minWidth;

		// Token: 0x04001325 RID: 4901
		private int maxWidth;

		// Token: 0x04001326 RID: 4902
		private int height;

		// Token: 0x04001327 RID: 4903
		private Point voronoiSiteInterval;

		// Token: 0x04001328 RID: 4904
		private Point voronoiSiteVariance;

		// Token: 0x04001329 RID: 4905
		private Point mainPathNodeIntervalRange;

		// Token: 0x0400132A RID: 4906
		private int caveCount;

		// Token: 0x0400132B RID: 4907
		private float bottomHoleProbability;

		// Token: 0x0400132C RID: 4908
		private int seaFloorBaseDepth;

		// Token: 0x0400132D RID: 4909
		private int seaFloorVariance;

		// Token: 0x0400132E RID: 4910
		private int cellSubdivisionLength;

		// Token: 0x0400132F RID: 4911
		private float cellRoundingAmount;

		// Token: 0x04001330 RID: 4912
		private float cellIrregularity;

		// Token: 0x04001331 RID: 4913
		private int mountainCountMin;

		// Token: 0x04001332 RID: 4914
		private int mountainCountMax;

		// Token: 0x04001333 RID: 4915
		private int mountainHeightMin;

		// Token: 0x04001334 RID: 4916
		private int mountainHeightMax;

		// Token: 0x04001335 RID: 4917
		private float waterParticleScale;

		// Token: 0x04001336 RID: 4918
		private int initialDepthMin;

		// Token: 0x04001337 RID: 4919
		private int initialDepthMax;

		// Token: 0x04001338 RID: 4920
		public readonly ImmutableHashSet<Identifier> AllowedBiomeIdentifiers;

		// Token: 0x04001339 RID: 4921
		public readonly bool AnyBiomeAllowed;

		// Token: 0x04001341 RID: 4929
		private Vector2 startPosition;

		// Token: 0x04001342 RID: 4930
		private Vector2 endPosition;

		// Token: 0x04001343 RID: 4931
		private Vector2 forceOutpostPosition;

		// Token: 0x0400136B RID: 4971
		private Vector2 waterParticleVelocity;
	}
}
