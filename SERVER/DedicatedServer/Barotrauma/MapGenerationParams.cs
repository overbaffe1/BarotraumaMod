using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000248 RID: 584
	internal class MapGenerationParams : Prefab, ISerializableEntity
	{
		// Token: 0x17000C42 RID: 3138
		// (get) Token: 0x0600299E RID: 10654 RVA: 0x00111D3A File Offset: 0x0010FF3A
		public static MapGenerationParams Instance
		{
			get
			{
				return MapGenerationParams.Params.ActivePrefab;
			}
		}

		// Token: 0x17000C43 RID: 3139
		// (get) Token: 0x0600299F RID: 10655 RVA: 0x00111D46 File Offset: 0x0010FF46
		// (set) Token: 0x060029A0 RID: 10656 RVA: 0x00111D4E File Offset: 0x0010FF4E
		[Serialize(6, IsPropertySaveable.Yes, "", "", false)]
		public int DifficultyZones { get; set; }

		// Token: 0x17000C44 RID: 3140
		// (get) Token: 0x060029A1 RID: 10657 RVA: 0x00111D57 File Offset: 0x0010FF57
		// (set) Token: 0x060029A2 RID: 10658 RVA: 0x00111D5F File Offset: 0x0010FF5F
		[Serialize(8000, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public int Width { get; set; }

		// Token: 0x17000C45 RID: 3141
		// (get) Token: 0x060029A3 RID: 10659 RVA: 0x00111D68 File Offset: 0x0010FF68
		// (set) Token: 0x060029A4 RID: 10660 RVA: 0x00111D70 File Offset: 0x0010FF70
		[Serialize(500, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public int Height { get; set; }

		// Token: 0x17000C46 RID: 3142
		// (get) Token: 0x060029A5 RID: 10661 RVA: 0x00111D79 File Offset: 0x0010FF79
		// (set) Token: 0x060029A6 RID: 10662 RVA: 0x00111D81 File Offset: 0x0010FF81
		[Serialize(20f, IsPropertySaveable.Yes, "Connections with a length smaller or equal to this generate the smallest possible levels (using the MinWidth parameter in the level generation paramaters).", "", false)]
		[Editable(0f, 5000f, 1)]
		public float SmallLevelConnectionLength { get; set; }

		// Token: 0x17000C47 RID: 3143
		// (get) Token: 0x060029A7 RID: 10663 RVA: 0x00111D8A File Offset: 0x0010FF8A
		// (set) Token: 0x060029A8 RID: 10664 RVA: 0x00111D92 File Offset: 0x0010FF92
		[Serialize(200f, IsPropertySaveable.Yes, "Connections with a length larger or equal to this generate the largest possible levels (using the MaxWidth parameter in the level generation paramaters).", "", false)]
		[Editable(0f, 5000f, 1)]
		public float LargeLevelConnectionLength { get; set; }

		// Token: 0x17000C48 RID: 3144
		// (get) Token: 0x060029A9 RID: 10665 RVA: 0x00111D9B File Offset: 0x0010FF9B
		// (set) Token: 0x060029AA RID: 10666 RVA: 0x00111DA3 File Offset: 0x0010FFA3
		[Serialize("20,20", IsPropertySaveable.Yes, "How far from each other voronoi sites are placed. Sites determine shape of the voronoi graph. Locations are placed at the vertices of the voronoi cells. (Decreasing this value causes the number of sites, and the complexity of the map, to increase exponentially - be careful when adjusting)", "", false)]
		[Editable]
		public Point VoronoiSiteInterval { get; set; }

		// Token: 0x17000C49 RID: 3145
		// (get) Token: 0x060029AB RID: 10667 RVA: 0x00111DAC File Offset: 0x0010FFAC
		// (set) Token: 0x060029AC RID: 10668 RVA: 0x00111DB4 File Offset: 0x0010FFB4
		[Serialize("5,5", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Point VoronoiSiteVariance { get; set; }

		// Token: 0x17000C4A RID: 3146
		// (get) Token: 0x060029AD RID: 10669 RVA: 0x00111DBD File Offset: 0x0010FFBD
		// (set) Token: 0x060029AE RID: 10670 RVA: 0x00111DC5 File Offset: 0x0010FFC5
		[Serialize(10f, IsPropertySaveable.Yes, "Connections smaller than this are removed.", "", false)]
		[Editable(0f, 500f, 1)]
		public float MinConnectionDistance { get; set; }

		// Token: 0x17000C4B RID: 3147
		// (get) Token: 0x060029AF RID: 10671 RVA: 0x00111DCE File Offset: 0x0010FFCE
		// (set) Token: 0x060029B0 RID: 10672 RVA: 0x00111DD6 File Offset: 0x0010FFD6
		[Serialize(5f, IsPropertySaveable.Yes, "Locations that are closer than this to another location are removed.", "", false)]
		[Editable(0f, 100f, 1)]
		public float MinLocationDistance { get; set; }

		// Token: 0x17000C4C RID: 3148
		// (get) Token: 0x060029B1 RID: 10673 RVA: 0x00111DDF File Offset: 0x0010FFDF
		// (set) Token: 0x060029B2 RID: 10674 RVA: 0x00111DE7 File Offset: 0x0010FFE7
		[Serialize(0.1f, IsPropertySaveable.Yes, "ConnectionIterationMultiplier for the UI indicator lines between locations.", "", false)]
		[Editable(0f, 10f, 1, DecimalCount = 2)]
		public float ConnectionIndicatorIterationMultiplier { get; set; }

		// Token: 0x17000C4D RID: 3149
		// (get) Token: 0x060029B3 RID: 10675 RVA: 0x00111DF0 File Offset: 0x0010FFF0
		// (set) Token: 0x060029B4 RID: 10676 RVA: 0x00111DF8 File Offset: 0x0010FFF8
		[Serialize(0.1f, IsPropertySaveable.Yes, "ConnectionDisplacementMultiplier for the UI indicator lines between locations.", "", false)]
		[Editable(0f, 10f, 1, DecimalCount = 2)]
		public float ConnectionIndicatorDisplacementMultiplier { get; set; }

		// Token: 0x17000C4E RID: 3150
		// (get) Token: 0x060029B5 RID: 10677 RVA: 0x00111E01 File Offset: 0x00110001
		public string Name
		{
			get
			{
				return base.GetType().ToString();
			}
		}

		// Token: 0x17000C4F RID: 3151
		// (get) Token: 0x060029B6 RID: 10678 RVA: 0x00111E0E File Offset: 0x0011000E
		// (set) Token: 0x060029B7 RID: 10679 RVA: 0x00111E16 File Offset: 0x00110016
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; private set; }

		// Token: 0x060029B8 RID: 10680 RVA: 0x00111E20 File Offset: 0x00110020
		public MapGenerationParams(ContentXElement element, MapGenerationParametersFile file) : base(file, file.Path.Value.ToIdentifier())
		{
			this.SerializableProperties = SerializableProperty.DeserializeProperties(this, element);
			int[] gateCount = element.GetAttributeIntArray("gatecount", null) ?? element.GetAttributeIntArray("GateCount", null);
			if (gateCount == null)
			{
				gateCount = new int[this.DifficultyZones];
				for (int i = 0; i < this.DifficultyZones; i++)
				{
					gateCount[i] = 1;
				}
			}
			this.GateCount = gateCount.ToImmutableArray<int>();
			Dictionary<Identifier, List<Sprite>> mapTiles = new Dictionary<Identifier, List<Sprite>>();
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "radiationparams")
				{
					this.RadiationParams = new RadiationParams(subElement);
				}
			}
		}

		// Token: 0x060029B9 RID: 10681 RVA: 0x00111F2C File Offset: 0x0011012C
		public override void Dispose()
		{
		}

		// Token: 0x0400147B RID: 5243
		public static readonly PrefabSelector<MapGenerationParams> Params = new PrefabSelector<MapGenerationParams>();

		// Token: 0x0400147C RID: 5244
		public readonly bool ShowLocations = true;

		// Token: 0x0400147D RID: 5245
		public readonly bool ShowLevelTypeNames;

		// Token: 0x0400147E RID: 5246
		public readonly bool ShowOverlay = true;

		// Token: 0x0400148A RID: 5258
		public readonly ImmutableArray<int> GateCount;

		// Token: 0x0400148C RID: 5260
		public RadiationParams RadiationParams;
	}
}
