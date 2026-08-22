using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x020000D9 RID: 217
	internal class LevelGenerationParams : PrefabWithUintIdentifier, ISerializableEntity
	{
		// Token: 0x06001D50 RID: 7504 RVA: 0x00129FAC File Offset: 0x001281AC
		public void DrawBackgrounds(SpriteBatch spriteBatch, Camera cam)
		{
			if (this.BackgroundTopSprite == null)
			{
				return;
			}
			Vector2 backgroundPos = cam.WorldViewCenter.FlipY() * 0.05f;
			int backgroundSize = (int)this.BackgroundTopSprite.size.Y;
			if (backgroundPos.Y >= (float)backgroundSize)
			{
				return;
			}
			if (backgroundPos.Y < 0f)
			{
				this.BackgroundTopSprite.SourceRect = new Rectangle((int)backgroundPos.X, (int)backgroundPos.Y, backgroundSize, (int)Math.Min(-backgroundPos.Y, (float)backgroundSize));
				Sprite backgroundTopSprite = this.BackgroundTopSprite;
				Vector2 zero = Vector2.Zero;
				Vector2 targetSize = new Vector2((float)GameMain.GraphicsWidth, Math.Min(-backgroundPos.Y, (float)GameMain.GraphicsHeight));
				float rotation = 0f;
				Color? color = new Color?(this.BackgroundTextureColor);
				backgroundTopSprite.DrawTiled(spriteBatch, zero, targetSize, rotation, null, color, null, null, null);
			}
			if (-backgroundPos.Y < (float)GameMain.GraphicsHeight && this.BackgroundSprite != null)
			{
				this.BackgroundSprite.SourceRect = new Rectangle((int)backgroundPos.X, (int)Math.Max(backgroundPos.Y, 0f), backgroundSize, backgroundSize);
				Sprite backgroundSprite = this.BackgroundSprite;
				Vector2 position = (backgroundPos.Y < 0f) ? new Vector2(0f, (float)((int)(-(int)backgroundPos.Y))) : Vector2.Zero;
				Vector2 targetSize2 = new Vector2((float)GameMain.GraphicsWidth, (float)((int)Math.Min(Math.Ceiling((double)((float)backgroundSize - backgroundPos.Y)), (double)backgroundSize)));
				float rotation2 = 0f;
				Color? color = new Color?(this.BackgroundTextureColor);
				backgroundSprite.DrawTiled(spriteBatch, position, targetSize2, rotation2, null, color, null, null, null);
			}
		}

		// Token: 0x06001D51 RID: 7505 RVA: 0x0012A170 File Offset: 0x00128370
		public void DrawWaterParticles(SpriteBatch spriteBatch, Camera cam, Vector2 offset)
		{
			if (this.WaterParticles == null || cam.Zoom <= 0.05f)
			{
				return;
			}
			float textureScale = this.WaterParticleScale;
			Vector2 textureSize = new Vector2((float)this.WaterParticles.Texture.Width, (float)this.WaterParticles.Texture.Height);
			Vector2 origin = new Vector2((float)cam.WorldView.X, (float)(-(float)cam.WorldView.Y));
			offset -= origin;
			for (int i = 0; i < 4; i++)
			{
				float scale = 1f - (float)i * 0.2f;
				float alpha = MathUtils.InverseLerp(0.05f, 0.1f, cam.Zoom * scale);
				if (alpha != 0f)
				{
					Vector2 newOffset = offset * scale;
					newOffset += cam.WorldView.Size.ToVector2() * (1f - scale) * 0.5f;
					newOffset -= new Vector2(256f * (float)i);
					float newTextureScale = scale * textureScale;
					Vector2 newSize = textureSize * scale;
					while (newOffset.X <= -newSize.X)
					{
						newOffset.X += newSize.X;
					}
					while (newOffset.X > 0f)
					{
						newOffset.X -= newSize.X;
					}
					while (newOffset.Y <= -newSize.Y)
					{
						newOffset.Y += newSize.Y;
					}
					while (newOffset.Y > 0f)
					{
						newOffset.Y -= newSize.Y;
					}
					Sprite waterParticles = this.WaterParticles;
					Vector2 position = origin + newOffset;
					Vector2 targetSize = cam.WorldView.Size.ToVector2() - newOffset;
					float rotation = 0f;
					Color? color = new Color?(this.WaterParticleColor * alpha);
					Vector2? textureScale2 = new Vector2?(new Vector2(newTextureScale));
					waterParticles.DrawTiled(spriteBatch, position, targetSize, rotation, null, color, null, textureScale2, null);
				}
			}
		}

		// Token: 0x06001D52 RID: 7506 RVA: 0x0012A3A8 File Offset: 0x001285A8
		public void UpdateWaterParticleOffset(ref Vector2 offset, Vector2 velocity, float deltaTime)
		{
			if (this.WaterParticles == null)
			{
				return;
			}
			Vector2 waterTextureSize = this.WaterParticles.size * this.WaterParticleScale;
			offset += velocity.FlipY() * this.WaterParticleScale * deltaTime;
			offset.X %= waterTextureSize.X;
			offset.Y %= waterTextureSize.Y;
		}

		// Token: 0x1700079F RID: 1951
		// (get) Token: 0x06001D53 RID: 7507 RVA: 0x0012A41D File Offset: 0x0012861D
		// (set) Token: 0x06001D54 RID: 7508 RVA: 0x0012A425 File Offset: 0x00128625
		public LocalizedString DisplayName { get; private set; }

		// Token: 0x170007A0 RID: 1952
		// (get) Token: 0x06001D55 RID: 7509 RVA: 0x0012A42E File Offset: 0x0012862E
		// (set) Token: 0x06001D56 RID: 7510 RVA: 0x0012A436 File Offset: 0x00128636
		public LocalizedString Description { get; private set; }

		// Token: 0x170007A1 RID: 1953
		// (get) Token: 0x06001D57 RID: 7511 RVA: 0x0012A43F File Offset: 0x0012863F
		public string Name
		{
			get
			{
				return this.Identifier.Value;
			}
		}

		// Token: 0x170007A2 RID: 1954
		// (get) Token: 0x06001D58 RID: 7512 RVA: 0x0012A44C File Offset: 0x0012864C
		public Identifier OldIdentifier { get; }

		// Token: 0x170007A3 RID: 1955
		// (get) Token: 0x06001D59 RID: 7513 RVA: 0x0012A454 File Offset: 0x00128654
		// (set) Token: 0x06001D5A RID: 7514 RVA: 0x0012A45C File Offset: 0x0012865C
		public Dictionary<Identifier, SerializableProperty> SerializableProperties { get; set; }

		// Token: 0x170007A4 RID: 1956
		// (get) Token: 0x06001D5B RID: 7515 RVA: 0x0012A465 File Offset: 0x00128665
		// (set) Token: 0x06001D5C RID: 7516 RVA: 0x0012A46D File Offset: 0x0012866D
		[Header("General", null)]
		[Serialize(LevelData.LevelType.LocationConnection, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public LevelData.LevelType Type { get; set; }

		// Token: 0x170007A5 RID: 1957
		// (get) Token: 0x06001D5D RID: 7517 RVA: 0x0012A476 File Offset: 0x00128676
		// (set) Token: 0x06001D5E RID: 7518 RVA: 0x0012A47E File Offset: 0x0012867E
		[Serialize(false, IsPropertySaveable.Yes, "If the given level is only used in PvP modes", "", false)]
		[Editable]
		public bool IsPvPLevel { get; set; }

		// Token: 0x170007A6 RID: 1958
		// (get) Token: 0x06001D5F RID: 7519 RVA: 0x0012A487 File Offset: 0x00128687
		// (set) Token: 0x06001D60 RID: 7520 RVA: 0x0012A48F File Offset: 0x0012868F
		[Serialize(100f, IsPropertySaveable.Yes, "If there are multiple level generation parameters available for a level in a given biome, their commonness determines how likely it is for one to get selected.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float Commonness { get; set; }

		// Token: 0x170007A7 RID: 1959
		// (get) Token: 0x06001D61 RID: 7521 RVA: 0x0012A498 File Offset: 0x00128698
		// (set) Token: 0x06001D62 RID: 7522 RVA: 0x0012A4A0 File Offset: 0x001286A0
		[Serialize(false, IsPropertySaveable.Yes, "If the level is a transition from the previous biome to this one.", "", false)]
		[Editable]
		public bool TransitionFromPreviousBiome { get; set; }

		// Token: 0x170007A8 RID: 1960
		// (get) Token: 0x06001D63 RID: 7523 RVA: 0x0012A4A9 File Offset: 0x001286A9
		// (set) Token: 0x06001D64 RID: 7524 RVA: 0x0012A4B1 File Offset: 0x001286B1
		[Serialize(0f, IsPropertySaveable.Yes, "The difficulty of the level has to be above or equal to this for these parameters to get chosen for the level.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MinLevelDifficulty { get; set; }

		// Token: 0x170007A9 RID: 1961
		// (get) Token: 0x06001D65 RID: 7525 RVA: 0x0012A4BA File Offset: 0x001286BA
		// (set) Token: 0x06001D66 RID: 7526 RVA: 0x0012A4C2 File Offset: 0x001286C2
		[Serialize(100f, IsPropertySaveable.Yes, "The difficulty of the level has to be below or equal to this for these parameters to get chosen for the level.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 100f)]
		public float MaxLevelDifficulty { get; set; }

		// Token: 0x170007AA RID: 1962
		// (get) Token: 0x06001D67 RID: 7527 RVA: 0x0012A4CB File Offset: 0x001286CB
		// (set) Token: 0x06001D68 RID: 7528 RVA: 0x0012A4D3 File Offset: 0x001286D3
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

		// Token: 0x170007AB RID: 1963
		// (get) Token: 0x06001D69 RID: 7529 RVA: 0x0012A50A File Offset: 0x0012870A
		// (set) Token: 0x06001D6A RID: 7530 RVA: 0x0012A512 File Offset: 0x00128712
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

		// Token: 0x170007AC RID: 1964
		// (get) Token: 0x06001D6B RID: 7531 RVA: 0x0012A549 File Offset: 0x00128749
		// (set) Token: 0x06001D6C RID: 7532 RVA: 0x0012A551 File Offset: 0x00128751
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

		// Token: 0x170007AD RID: 1965
		// (get) Token: 0x06001D6D RID: 7533 RVA: 0x0012A588 File Offset: 0x00128788
		// (set) Token: 0x06001D6E RID: 7534 RVA: 0x0012A590 File Offset: 0x00128790
		[Serialize(true, IsPropertySaveable.Yes, "Should there be a hole in the wall next to the end outpost (can be used to prevent players from having to backtrack if they approach the outpost from the wrong side of the main path's walls).", "", false)]
		[Editable]
		public bool CreateHoleNextToEnd { get; set; }

		// Token: 0x170007AE RID: 1966
		// (get) Token: 0x06001D6F RID: 7535 RVA: 0x0012A599 File Offset: 0x00128799
		// (set) Token: 0x06001D70 RID: 7536 RVA: 0x0012A5A1 File Offset: 0x001287A1
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

		// Token: 0x170007AF RID: 1967
		// (get) Token: 0x06001D71 RID: 7537 RVA: 0x0012A5B9 File Offset: 0x001287B9
		// (set) Token: 0x06001D72 RID: 7538 RVA: 0x0012A5C1 File Offset: 0x001287C1
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

		// Token: 0x170007B0 RID: 1968
		// (get) Token: 0x06001D73 RID: 7539 RVA: 0x0012A5D9 File Offset: 0x001287D9
		// (set) Token: 0x06001D74 RID: 7540 RVA: 0x0012A5E1 File Offset: 0x001287E1
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

		// Token: 0x170007B1 RID: 1969
		// (get) Token: 0x06001D75 RID: 7541 RVA: 0x0012A5F9 File Offset: 0x001287F9
		// (set) Token: 0x06001D76 RID: 7542 RVA: 0x0012A601 File Offset: 0x00128801
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

		// Token: 0x170007B2 RID: 1970
		// (get) Token: 0x06001D77 RID: 7543 RVA: 0x0012A619 File Offset: 0x00128819
		// (set) Token: 0x06001D78 RID: 7544 RVA: 0x0012A621 File Offset: 0x00128821
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

		// Token: 0x170007B3 RID: 1971
		// (get) Token: 0x06001D79 RID: 7545 RVA: 0x0012A630 File Offset: 0x00128830
		// (set) Token: 0x06001D7A RID: 7546 RVA: 0x0012A638 File Offset: 0x00128838
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

		// Token: 0x170007B4 RID: 1972
		// (get) Token: 0x06001D7B RID: 7547 RVA: 0x0012A64C File Offset: 0x0012884C
		// (set) Token: 0x06001D7C RID: 7548 RVA: 0x0012A654 File Offset: 0x00128854
		[Header("Level geometry", null)]
		[Serialize(false, IsPropertySaveable.Yes, "If enabled, no walls generate in the level. Can be useful for e.g. levels that are just supposed to consist of a pre-built outpost.", "", false)]
		[Editable]
		public bool NoLevelGeometry { get; set; }

		// Token: 0x170007B5 RID: 1973
		// (get) Token: 0x06001D7D RID: 7549 RVA: 0x0012A65D File Offset: 0x0012885D
		// (set) Token: 0x06001D7E RID: 7550 RVA: 0x0012A668 File Offset: 0x00128868
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

		// Token: 0x170007B6 RID: 1974
		// (get) Token: 0x06001D7F RID: 7551 RVA: 0x0012A6B5 File Offset: 0x001288B5
		// (set) Token: 0x06001D80 RID: 7552 RVA: 0x0012A6BD File Offset: 0x001288BD
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

		// Token: 0x170007B7 RID: 1975
		// (get) Token: 0x06001D81 RID: 7553 RVA: 0x0012A6F8 File Offset: 0x001288F8
		// (set) Token: 0x06001D82 RID: 7554 RVA: 0x0012A700 File Offset: 0x00128900
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

		// Token: 0x170007B8 RID: 1976
		// (get) Token: 0x06001D83 RID: 7555 RVA: 0x0012A710 File Offset: 0x00128910
		// (set) Token: 0x06001D84 RID: 7556 RVA: 0x0012A718 File Offset: 0x00128918
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

		// Token: 0x170007B9 RID: 1977
		// (get) Token: 0x06001D85 RID: 7557 RVA: 0x0012A730 File Offset: 0x00128930
		// (set) Token: 0x06001D86 RID: 7558 RVA: 0x0012A738 File Offset: 0x00128938
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

		// Token: 0x170007BA RID: 1978
		// (get) Token: 0x06001D87 RID: 7559 RVA: 0x0012A750 File Offset: 0x00128950
		// (set) Token: 0x06001D88 RID: 7560 RVA: 0x0012A758 File Offset: 0x00128958
		[Header("Tunnels", null)]
		[Serialize(6500, IsPropertySaveable.Yes, "Minimum width of the main tunnel going through the level, in pixels. Can be automatically increased by the level editor if the submarine is larger than this.", "", false)]
		[Editable(MinValueInt = 5000, MaxValueInt = 1000000)]
		public int MinTunnelRadius { get; set; }

		// Token: 0x170007BB RID: 1979
		// (get) Token: 0x06001D89 RID: 7561 RVA: 0x0012A761 File Offset: 0x00128961
		// (set) Token: 0x06001D8A RID: 7562 RVA: 0x0012A769 File Offset: 0x00128969
		[Serialize("0,1", IsPropertySaveable.Yes, "Amount of side tunnels in the level (min,max).", "", false)]
		[Editable]
		public Point SideTunnelCount { get; set; }

		// Token: 0x170007BC RID: 1980
		// (get) Token: 0x06001D8B RID: 7563 RVA: 0x0012A772 File Offset: 0x00128972
		// (set) Token: 0x06001D8C RID: 7564 RVA: 0x0012A77A File Offset: 0x0012897A
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much the side tunnels can \"zigzag\". 0 = completely straight tunnel, 1 = can go all the way from the top of the level to the bottom.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float SideTunnelVariance { get; set; }

		// Token: 0x170007BD RID: 1981
		// (get) Token: 0x06001D8D RID: 7565 RVA: 0x0012A783 File Offset: 0x00128983
		// (set) Token: 0x06001D8E RID: 7566 RVA: 0x0012A78B File Offset: 0x0012898B
		[Serialize("2000,6000", IsPropertySaveable.Yes, "Minimum width of the side tunnels, in pixels. Unlike the main tunnel, does not get adjusted based on the size of the submarine.", "", false)]
		[Editable]
		public Point MinSideTunnelRadius { get; set; }

		// Token: 0x170007BE RID: 1982
		// (get) Token: 0x06001D8F RID: 7567 RVA: 0x0012A794 File Offset: 0x00128994
		// (set) Token: 0x06001D90 RID: 7568 RVA: 0x0012A79C File Offset: 0x0012899C
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

		// Token: 0x170007BF RID: 1983
		// (get) Token: 0x06001D91 RID: 7569 RVA: 0x0012A7F2 File Offset: 0x001289F2
		// (set) Token: 0x06001D92 RID: 7570 RVA: 0x0012A7FA File Offset: 0x001289FA
		[Serialize(0.5f, IsPropertySaveable.Yes, "How much the side tunnels can \"zigzag\". 0 = completely straight tunnel, 1 = can go all the way from the top of the level to the bottom.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float MainPathVariance { get; set; }

		// Token: 0x170007C0 RID: 1984
		// (get) Token: 0x06001D93 RID: 7571 RVA: 0x0012A803 File Offset: 0x00128A03
		// (set) Token: 0x06001D94 RID: 7572 RVA: 0x0012A80B File Offset: 0x00128A0B
		[Header("Contents", null)]
		[Serialize(1000, IsPropertySaveable.Yes, "The total number of level objects (vegetation, vents, etc) in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 100000)]
		public int LevelObjectAmount { get; set; }

		// Token: 0x170007C1 RID: 1985
		// (get) Token: 0x06001D95 RID: 7573 RVA: 0x0012A814 File Offset: 0x00128A14
		// (set) Token: 0x06001D96 RID: 7574 RVA: 0x0012A81C File Offset: 0x00128A1C
		[Serialize(80, IsPropertySaveable.Yes, "The total number of decorative background creatures.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int BackgroundCreatureAmount { get; set; }

		// Token: 0x170007C2 RID: 1986
		// (get) Token: 0x06001D97 RID: 7575 RVA: 0x0012A825 File Offset: 0x00128A25
		// (set) Token: 0x06001D98 RID: 7576 RVA: 0x0012A82D File Offset: 0x00128A2D
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

		// Token: 0x170007C3 RID: 1987
		// (get) Token: 0x06001D99 RID: 7577 RVA: 0x0012A83E File Offset: 0x00128A3E
		// (set) Token: 0x06001D9A RID: 7578 RVA: 0x0012A846 File Offset: 0x00128A46
		[Serialize(100, IsPropertySaveable.Yes, "The maximum number of level resources in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10000)]
		public int ItemCount { get; set; }

		// Token: 0x170007C4 RID: 1988
		// (get) Token: 0x06001D9B RID: 7579 RVA: 0x0012A84F File Offset: 0x00128A4F
		// (set) Token: 0x06001D9C RID: 7580 RVA: 0x0012A857 File Offset: 0x00128A57
		[Serialize("19200,38400", IsPropertySaveable.Yes, "The minimum and maximum distance between two resource spawn points on a path.", "", false)]
		[Editable(100, 100000)]
		public Point ResourceIntervalRange { get; set; }

		// Token: 0x170007C5 RID: 1989
		// (get) Token: 0x06001D9D RID: 7581 RVA: 0x0012A860 File Offset: 0x00128A60
		// (set) Token: 0x06001D9E RID: 7582 RVA: 0x0012A868 File Offset: 0x00128A68
		[Serialize("9600,19200", IsPropertySaveable.Yes, "The minimum and maximum distance between two resource spawn points on a cave path.", "", false)]
		[Editable(100, 100000)]
		public Point CaveResourceIntervalRange { get; set; }

		// Token: 0x170007C6 RID: 1990
		// (get) Token: 0x06001D9F RID: 7583 RVA: 0x0012A871 File Offset: 0x00128A71
		// (set) Token: 0x06001DA0 RID: 7584 RVA: 0x0012A879 File Offset: 0x00128A79
		[Serialize("3,6", IsPropertySaveable.Yes, "The minimum and maximum amount of resources in a single cluster. In addition to this, resource commonness affects the cluster size. Less common resources spawn in smaller clusters.", "", false)]
		[Editable(1, 20)]
		public Point ResourceClusterSizeRange { get; set; }

		// Token: 0x170007C7 RID: 1991
		// (get) Token: 0x06001DA1 RID: 7585 RVA: 0x0012A882 File Offset: 0x00128A82
		// (set) Token: 0x06001DA2 RID: 7586 RVA: 0x0012A88A File Offset: 0x00128A8A
		[Serialize(0.3f, IsPropertySaveable.Yes, "How likely a resource spawn point on a path is to contain resources.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float ResourceSpawnChance { get; set; }

		// Token: 0x170007C8 RID: 1992
		// (get) Token: 0x06001DA3 RID: 7587 RVA: 0x0012A893 File Offset: 0x00128A93
		// (set) Token: 0x06001DA4 RID: 7588 RVA: 0x0012A89B File Offset: 0x00128A9B
		[Serialize(1f, IsPropertySaveable.Yes, "How likely a resource spawn point on a cave path is to contain resources.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f)]
		public float CaveResourceSpawnChance { get; set; }

		// Token: 0x170007C9 RID: 1993
		// (get) Token: 0x06001DA5 RID: 7589 RVA: 0x0012A8A4 File Offset: 0x00128AA4
		// (set) Token: 0x06001DA6 RID: 7590 RVA: 0x0012A8AC File Offset: 0x00128AAC
		[Serialize(0, IsPropertySaveable.Yes, "Number of floating, destructible ice chunks in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int FloatingIceChunkCount { get; set; }

		// Token: 0x170007CA RID: 1994
		// (get) Token: 0x06001DA7 RID: 7591 RVA: 0x0012A8B5 File Offset: 0x00128AB5
		// (set) Token: 0x06001DA8 RID: 7592 RVA: 0x0012A8BD File Offset: 0x00128ABD
		[Serialize(0, IsPropertySaveable.Yes, "Number of islands (static wall chunks along the main path) in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 100)]
		public int IslandCount { get; set; }

		// Token: 0x170007CB RID: 1995
		// (get) Token: 0x06001DA9 RID: 7593 RVA: 0x0012A8C6 File Offset: 0x00128AC6
		// (set) Token: 0x06001DAA RID: 7594 RVA: 0x0012A8CE File Offset: 0x00128ACE
		[Serialize(0, IsPropertySaveable.Yes, "Number of ice spires in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int IceSpireCount { get; set; }

		// Token: 0x170007CC RID: 1996
		// (get) Token: 0x06001DAB RID: 7595 RVA: 0x0012A8D7 File Offset: 0x00128AD7
		// (set) Token: 0x06001DAC RID: 7596 RVA: 0x0012A8DF File Offset: 0x00128ADF
		[Header("Abyss", null)]
		[Serialize(true, IsPropertySaveable.Yes, "Should the generator force a hole to the bottom of the level to ensure there's a way to the abyss.", "", false)]
		[Editable]
		public bool CreateHoleToAbyss { get; set; }

		// Token: 0x170007CD RID: 1997
		// (get) Token: 0x06001DAD RID: 7597 RVA: 0x0012A8E8 File Offset: 0x00128AE8
		// (set) Token: 0x06001DAE RID: 7598 RVA: 0x0012A8F0 File Offset: 0x00128AF0
		[Serialize(5, IsPropertySaveable.Yes, "Number of abyss islands in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int AbyssIslandCount { get; set; }

		// Token: 0x170007CE RID: 1998
		// (get) Token: 0x06001DAF RID: 7599 RVA: 0x0012A8F9 File Offset: 0x00128AF9
		// (set) Token: 0x06001DB0 RID: 7600 RVA: 0x0012A901 File Offset: 0x00128B01
		[Serialize("4000,7000", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Point AbyssIslandSizeMin { get; set; }

		// Token: 0x170007CF RID: 1999
		// (get) Token: 0x06001DB1 RID: 7601 RVA: 0x0012A90A File Offset: 0x00128B0A
		// (set) Token: 0x06001DB2 RID: 7602 RVA: 0x0012A912 File Offset: 0x00128B12
		[Serialize("8000,10000", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Point AbyssIslandSizeMax { get; set; }

		// Token: 0x170007D0 RID: 2000
		// (get) Token: 0x06001DB3 RID: 7603 RVA: 0x0012A91B File Offset: 0x00128B1B
		// (set) Token: 0x06001DB4 RID: 7604 RVA: 0x0012A923 File Offset: 0x00128B23
		[Serialize(0.5f, IsPropertySaveable.Yes, "The probability of an abyss island having a cave. There is always a cave in at least one of the islands regardless of this setting.", "", false)]
		[Editable]
		public float AbyssIslandCaveProbability { get; set; }

		// Token: 0x170007D1 RID: 2001
		// (get) Token: 0x06001DB5 RID: 7605 RVA: 0x0012A92C File Offset: 0x00128B2C
		// (set) Token: 0x06001DB6 RID: 7606 RVA: 0x0012A934 File Offset: 0x00128B34
		[Serialize(10, IsPropertySaveable.Yes, "Minimum number of resource clusters in the abyss (the actual number is picked between min and max according to the level difficulty)", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int AbyssResourceClustersMin { get; set; }

		// Token: 0x170007D2 RID: 2002
		// (get) Token: 0x06001DB7 RID: 7607 RVA: 0x0012A93D File Offset: 0x00128B3D
		// (set) Token: 0x06001DB8 RID: 7608 RVA: 0x0012A945 File Offset: 0x00128B45
		[Serialize(40, IsPropertySaveable.Yes, "Maximum number of resource clusters in the abyss (the actual number is picked between min and max according to the level difficulty)", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 1000)]
		public int AbyssResourceClustersMax { get; set; }

		// Token: 0x170007D3 RID: 2003
		// (get) Token: 0x06001DB9 RID: 7609 RVA: 0x0012A94E File Offset: 0x00128B4E
		// (set) Token: 0x06001DBA RID: 7610 RVA: 0x0012A956 File Offset: 0x00128B56
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

		// Token: 0x170007D4 RID: 2004
		// (get) Token: 0x06001DBB RID: 7611 RVA: 0x0012A96A File Offset: 0x00128B6A
		// (set) Token: 0x06001DBC RID: 7612 RVA: 0x0012A972 File Offset: 0x00128B72
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

		// Token: 0x170007D5 RID: 2005
		// (get) Token: 0x06001DBD RID: 7613 RVA: 0x0012A97B File Offset: 0x00128B7B
		// (set) Token: 0x06001DBE RID: 7614 RVA: 0x0012A983 File Offset: 0x00128B83
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

		// Token: 0x170007D6 RID: 2006
		// (get) Token: 0x06001DBF RID: 7615 RVA: 0x0012A992 File Offset: 0x00128B92
		// (set) Token: 0x06001DC0 RID: 7616 RVA: 0x0012A99A File Offset: 0x00128B9A
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

		// Token: 0x170007D7 RID: 2007
		// (get) Token: 0x06001DC1 RID: 7617 RVA: 0x0012A9A9 File Offset: 0x00128BA9
		// (set) Token: 0x06001DC2 RID: 7618 RVA: 0x0012A9B1 File Offset: 0x00128BB1
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

		// Token: 0x170007D8 RID: 2008
		// (get) Token: 0x06001DC3 RID: 7619 RVA: 0x0012A9C0 File Offset: 0x00128BC0
		// (set) Token: 0x06001DC4 RID: 7620 RVA: 0x0012A9C8 File Offset: 0x00128BC8
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

		// Token: 0x06001DC5 RID: 7621 RVA: 0x0012A9D7 File Offset: 0x00128BD7
		public bool UseRandomRuinCount()
		{
			return this.MinRuinCount >= 0 && this.MaxRuinCount > 0;
		}

		// Token: 0x06001DC6 RID: 7622 RVA: 0x0012A9ED File Offset: 0x00128BED
		public int GetMaxRuinCount()
		{
			if (!this.UseRandomRuinCount())
			{
				return this.RuinCount;
			}
			return this.MaxRuinCount;
		}

		// Token: 0x170007D9 RID: 2009
		// (get) Token: 0x06001DC7 RID: 7623 RVA: 0x0012AA04 File Offset: 0x00128C04
		// (set) Token: 0x06001DC8 RID: 7624 RVA: 0x0012AA0C File Offset: 0x00128C0C
		[Header("Ruins", null)]
		[Serialize(1, IsPropertySaveable.Yes, "The number of alien ruins in the level. Ignored, if both MinRuinCount and MaxRuinCount are defined.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int RuinCount { get; set; }

		// Token: 0x170007DA RID: 2010
		// (get) Token: 0x06001DC9 RID: 7625 RVA: 0x0012AA15 File Offset: 0x00128C15
		// (set) Token: 0x06001DCA RID: 7626 RVA: 0x0012AA1D File Offset: 0x00128C1D
		[Serialize(0, IsPropertySaveable.Yes, "The minimum number of alien ruins in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MinRuinCount { get; set; }

		// Token: 0x170007DB RID: 2011
		// (get) Token: 0x06001DCB RID: 7627 RVA: 0x0012AA26 File Offset: 0x00128C26
		// (set) Token: 0x06001DCC RID: 7628 RVA: 0x0012AA2E File Offset: 0x00128C2E
		[Serialize(0, IsPropertySaveable.Yes, "The maximum number of alien ruins in the level.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MaxRuinCount { get; set; }

		// Token: 0x170007DC RID: 2012
		// (get) Token: 0x06001DCD RID: 7629 RVA: 0x0012AA37 File Offset: 0x00128C37
		// (set) Token: 0x06001DCE RID: 7630 RVA: 0x0012AA3F File Offset: 0x00128C3F
		[Serialize(1f, IsPropertySaveable.Yes, "The probability of spawning a ruin in the level. If the level can have multiple ruins, the probability is evaluated separately for each.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float RuinSpawnProbability { get; set; }

		// Token: 0x170007DD RID: 2013
		// (get) Token: 0x06001DCF RID: 7631 RVA: 0x0012AA48 File Offset: 0x00128C48
		// (set) Token: 0x06001DD0 RID: 7632 RVA: 0x0012AA50 File Offset: 0x00128C50
		[Header("Wrecks", null)]
		[Serialize(1, IsPropertySaveable.Yes, "The minimum number of wrecks in the level. Note that this value cannot be higher than the amount of wreck prefabs (subs).", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MinWreckCount { get; set; }

		// Token: 0x170007DE RID: 2014
		// (get) Token: 0x06001DD1 RID: 7633 RVA: 0x0012AA59 File Offset: 0x00128C59
		// (set) Token: 0x06001DD2 RID: 7634 RVA: 0x0012AA61 File Offset: 0x00128C61
		[Serialize(1, IsPropertySaveable.Yes, "The maximum number of wrecks in the level. Note that this value cannot be higher than the amount of wreck prefabs (subs).", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 10)]
		public int MaxWreckCount { get; set; }

		// Token: 0x170007DF RID: 2015
		// (get) Token: 0x06001DD3 RID: 7635 RVA: 0x0012AA6A File Offset: 0x00128C6A
		// (set) Token: 0x06001DD4 RID: 7636 RVA: 0x0012AA72 File Offset: 0x00128C72
		[Serialize(1, IsPropertySaveable.Yes, "The minimum number of corpses per wreck.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int MinCorpseCount { get; set; }

		// Token: 0x170007E0 RID: 2016
		// (get) Token: 0x06001DD5 RID: 7637 RVA: 0x0012AA7B File Offset: 0x00128C7B
		// (set) Token: 0x06001DD6 RID: 7638 RVA: 0x0012AA83 File Offset: 0x00128C83
		[Serialize(5, IsPropertySaveable.Yes, "The maximum number of corpses per wreck.", "", false)]
		[Editable(MinValueInt = 0, MaxValueInt = 20)]
		public int MaxCorpseCount { get; set; }

		// Token: 0x170007E1 RID: 2017
		// (get) Token: 0x06001DD7 RID: 7639 RVA: 0x0012AA8C File Offset: 0x00128C8C
		// (set) Token: 0x06001DD8 RID: 7640 RVA: 0x0012AA94 File Offset: 0x00128C94
		[Serialize(0f, IsPropertySaveable.Yes, "How likely is it that a character set to be spawned as a corpse spawns as a human husk instead? Percentage from 0 to 1 per character.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float HuskProbability { get; set; }

		// Token: 0x170007E2 RID: 2018
		// (get) Token: 0x06001DD9 RID: 7641 RVA: 0x0012AA9D File Offset: 0x00128C9D
		// (set) Token: 0x06001DDA RID: 7642 RVA: 0x0012AAA5 File Offset: 0x00128CA5
		[Serialize(0f, IsPropertySaveable.Yes, "How likely is it that a Thalamus inhabits a wreck. Percentage from 0 to 1 per wreck.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float ThalamusProbability { get; set; }

		// Token: 0x170007E3 RID: 2019
		// (get) Token: 0x06001DDB RID: 7643 RVA: 0x0012AAAE File Offset: 0x00128CAE
		// (set) Token: 0x06001DDC RID: 7644 RVA: 0x0012AAB6 File Offset: 0x00128CB6
		[Serialize(0.5f, IsPropertySaveable.Yes, "How likely the water level of a hull inside a wreck is randomly set.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float WreckHullFloodingChance { get; set; }

		// Token: 0x170007E4 RID: 2020
		// (get) Token: 0x06001DDD RID: 7645 RVA: 0x0012AABF File Offset: 0x00128CBF
		// (set) Token: 0x06001DDE RID: 7646 RVA: 0x0012AAC7 File Offset: 0x00128CC7
		[Serialize(0.1f, IsPropertySaveable.Yes, "The min water percentage of randomly flooding hulls in wrecks.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float WreckFloodingHullMinWaterPercentage { get; set; }

		// Token: 0x170007E5 RID: 2021
		// (get) Token: 0x06001DDF RID: 7647 RVA: 0x0012AAD0 File Offset: 0x00128CD0
		// (set) Token: 0x06001DE0 RID: 7648 RVA: 0x0012AAD8 File Offset: 0x00128CD8
		[Serialize(1f, IsPropertySaveable.Yes, "The min water percentage of randomly flooding hulls in wrecks.", "", false)]
		[Editable(MinValueFloat = 0f, MaxValueFloat = 1f, DecimalCount = 2)]
		public float WreckFloodingHullMaxWaterPercentage { get; set; }

		// Token: 0x170007E6 RID: 2022
		// (get) Token: 0x06001DE1 RID: 7649 RVA: 0x0012AAE1 File Offset: 0x00128CE1
		// (set) Token: 0x06001DE2 RID: 7650 RVA: 0x0012AAE9 File Offset: 0x00128CE9
		[Serialize("", IsPropertySaveable.Yes, "Should a beacon station always spawn in this type of level?", "", false)]
		public string ForceBeaconStation { get; set; }

		// Token: 0x170007E7 RID: 2023
		// (get) Token: 0x06001DE3 RID: 7651 RVA: 0x0012AAF2 File Offset: 0x00128CF2
		// (set) Token: 0x06001DE4 RID: 7652 RVA: 0x0012AAFA File Offset: 0x00128CFA
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

		// Token: 0x170007E8 RID: 2024
		// (get) Token: 0x06001DE5 RID: 7653 RVA: 0x0012AB0D File Offset: 0x00128D0D
		// (set) Token: 0x06001DE6 RID: 7654 RVA: 0x0012AB15 File Offset: 0x00128D15
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

		// Token: 0x170007E9 RID: 2025
		// (get) Token: 0x06001DE7 RID: 7655 RVA: 0x0012AB1E File Offset: 0x00128D1E
		// (set) Token: 0x06001DE8 RID: 7656 RVA: 0x0012AB26 File Offset: 0x00128D26
		[Serialize(2048f, IsPropertySaveable.Yes, "Size of the level wall texture.", "", false)]
		[Editable(10f, 10000f, 1)]
		public float WallTextureSize { get; private set; }

		// Token: 0x170007EA RID: 2026
		// (get) Token: 0x06001DE9 RID: 7657 RVA: 0x0012AB2F File Offset: 0x00128D2F
		// (set) Token: 0x06001DEA RID: 7658 RVA: 0x0012AB37 File Offset: 0x00128D37
		[Serialize(2048f, IsPropertySaveable.Yes, "", "", false)]
		[Editable(10f, 10000f, 1)]
		public float WallEdgeTextureWidth { get; private set; }

		// Token: 0x170007EB RID: 2027
		// (get) Token: 0x06001DEB RID: 7659 RVA: 0x0012AB40 File Offset: 0x00128D40
		// (set) Token: 0x06001DEC RID: 7660 RVA: 0x0012AB48 File Offset: 0x00128D48
		[Serialize("0,0", IsPropertySaveable.Yes, "Interval of lightning-like flashes of light in the level.", "", false)]
		[Editable]
		public Vector2 FlashInterval { get; set; }

		// Token: 0x170007EC RID: 2028
		// (get) Token: 0x06001DED RID: 7661 RVA: 0x0012AB51 File Offset: 0x00128D51
		// (set) Token: 0x06001DEE RID: 7662 RVA: 0x0012AB59 File Offset: 0x00128D59
		[Serialize("0,0,0,0", IsPropertySaveable.Yes, "Color of lightning-like flashes of light in the level.", "", false)]
		[Editable]
		public Color FlashColor { get; set; }

		// Token: 0x170007ED RID: 2029
		// (get) Token: 0x06001DEF RID: 7663 RVA: 0x0012AB62 File Offset: 0x00128D62
		// (set) Token: 0x06001DF0 RID: 7664 RVA: 0x0012AB6A File Offset: 0x00128D6A
		[Serialize(120f, IsPropertySaveable.Yes, "How far the level walls' edge texture portrudes outside the actual, \"physical\" edge of the cell.", "", false)]
		[Editable(0f, 1000f, 1)]
		public float WallEdgeExpandOutwardsAmount { get; private set; }

		// Token: 0x170007EE RID: 2030
		// (get) Token: 0x06001DF1 RID: 7665 RVA: 0x0012AB73 File Offset: 0x00128D73
		// (set) Token: 0x06001DF2 RID: 7666 RVA: 0x0012AB7B File Offset: 0x00128D7B
		[Serialize(1000f, IsPropertySaveable.Yes, "How far inside the level walls the edge texture continues.", "", false)]
		[Editable(0f, 10000f, 1)]
		public float WallEdgeExpandInwardsAmount { get; private set; }

		// Token: 0x170007EF RID: 2031
		// (get) Token: 0x06001DF3 RID: 7667 RVA: 0x0012AB84 File Offset: 0x00128D84
		// (set) Token: 0x06001DF4 RID: 7668 RVA: 0x0012AB8C File Offset: 0x00128D8C
		[Serialize(1000f, IsPropertySaveable.Yes, "How deep inside the walls the wall texture extends to before fading to black.", "", false)]
		[Editable(0f, 10000f, 1)]
		public float WallTextureExpandInwardsAmount { get; private set; }

		// Token: 0x170007F0 RID: 2032
		// (get) Token: 0x06001DF5 RID: 7669 RVA: 0x0012AB95 File Offset: 0x00128D95
		// (set) Token: 0x06001DF6 RID: 7670 RVA: 0x0012AB9D File Offset: 0x00128D9D
		[Header("Colors", null)]
		[Serialize("27,30,36", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color AmbientLightColor { get; set; }

		// Token: 0x170007F1 RID: 2033
		// (get) Token: 0x06001DF7 RID: 7671 RVA: 0x0012ABA6 File Offset: 0x00128DA6
		// (set) Token: 0x06001DF8 RID: 7672 RVA: 0x0012ABAE File Offset: 0x00128DAE
		[Serialize("20,40,50", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color BackgroundTextureColor { get; set; }

		// Token: 0x170007F2 RID: 2034
		// (get) Token: 0x06001DF9 RID: 7673 RVA: 0x0012ABB7 File Offset: 0x00128DB7
		// (set) Token: 0x06001DFA RID: 7674 RVA: 0x0012ABBF File Offset: 0x00128DBF
		[Serialize("20,40,50", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color BackgroundColor { get; set; }

		// Token: 0x170007F3 RID: 2035
		// (get) Token: 0x06001DFB RID: 7675 RVA: 0x0012ABC8 File Offset: 0x00128DC8
		// (set) Token: 0x06001DFC RID: 7676 RVA: 0x0012ABD0 File Offset: 0x00128DD0
		[Serialize("255,255,255", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color WallColor { get; set; }

		// Token: 0x170007F4 RID: 2036
		// (get) Token: 0x06001DFD RID: 7677 RVA: 0x0012ABD9 File Offset: 0x00128DD9
		// (set) Token: 0x06001DFE RID: 7678 RVA: 0x0012ABE1 File Offset: 0x00128DE1
		[Serialize("255,255,255", IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public Color WaterParticleColor { get; set; }

		// Token: 0x170007F5 RID: 2037
		// (get) Token: 0x06001DFF RID: 7679 RVA: 0x0012ABEA File Offset: 0x00128DEA
		// (set) Token: 0x06001E00 RID: 7680 RVA: 0x0012ABF2 File Offset: 0x00128DF2
		[Header("Sounds", null)]
		[Serialize(false, IsPropertySaveable.Yes, "Should the \"ambient noise\" of the biome play in this level if it's an outpost level.", "", false)]
		[Editable]
		public bool PlayNoiseLoopInOutpostLevel { get; set; }

		// Token: 0x170007F6 RID: 2038
		// (get) Token: 0x06001E01 RID: 7681 RVA: 0x0012ABFB File Offset: 0x00128DFB
		// (set) Token: 0x06001E02 RID: 7682 RVA: 0x0012AC03 File Offset: 0x00128E03
		[Serialize(1f, IsPropertySaveable.Yes, "", "", false)]
		[Editable]
		public float WaterAmbienceVolume { get; set; }

		// Token: 0x170007F7 RID: 2039
		// (get) Token: 0x06001E03 RID: 7683 RVA: 0x0012AC0C File Offset: 0x00128E0C
		// (set) Token: 0x06001E04 RID: 7684 RVA: 0x0012AC14 File Offset: 0x00128E14
		public Sprite BackgroundSprite { get; private set; }

		// Token: 0x170007F8 RID: 2040
		// (get) Token: 0x06001E05 RID: 7685 RVA: 0x0012AC1D File Offset: 0x00128E1D
		// (set) Token: 0x06001E06 RID: 7686 RVA: 0x0012AC25 File Offset: 0x00128E25
		public Sprite BackgroundTopSprite { get; private set; }

		// Token: 0x170007F9 RID: 2041
		// (get) Token: 0x06001E07 RID: 7687 RVA: 0x0012AC2E File Offset: 0x00128E2E
		// (set) Token: 0x06001E08 RID: 7688 RVA: 0x0012AC36 File Offset: 0x00128E36
		public Sprite WallSprite { get; private set; }

		// Token: 0x170007FA RID: 2042
		// (get) Token: 0x06001E09 RID: 7689 RVA: 0x0012AC3F File Offset: 0x00128E3F
		// (set) Token: 0x06001E0A RID: 7690 RVA: 0x0012AC47 File Offset: 0x00128E47
		public Sprite WallEdgeSprite { get; private set; }

		// Token: 0x170007FB RID: 2043
		// (get) Token: 0x06001E0B RID: 7691 RVA: 0x0012AC50 File Offset: 0x00128E50
		// (set) Token: 0x06001E0C RID: 7692 RVA: 0x0012AC58 File Offset: 0x00128E58
		public Sprite DestructibleWallSprite { get; private set; }

		// Token: 0x170007FC RID: 2044
		// (get) Token: 0x06001E0D RID: 7693 RVA: 0x0012AC61 File Offset: 0x00128E61
		// (set) Token: 0x06001E0E RID: 7694 RVA: 0x0012AC69 File Offset: 0x00128E69
		public Sprite DestructibleWallEdgeSprite { get; private set; }

		// Token: 0x170007FD RID: 2045
		// (get) Token: 0x06001E0F RID: 7695 RVA: 0x0012AC72 File Offset: 0x00128E72
		// (set) Token: 0x06001E10 RID: 7696 RVA: 0x0012AC7A File Offset: 0x00128E7A
		public Sprite WallSpriteDestroyed { get; private set; }

		// Token: 0x170007FE RID: 2046
		// (get) Token: 0x06001E11 RID: 7697 RVA: 0x0012AC83 File Offset: 0x00128E83
		// (set) Token: 0x06001E12 RID: 7698 RVA: 0x0012AC8B File Offset: 0x00128E8B
		public Sprite WaterParticles { get; private set; }

		// Token: 0x170007FF RID: 2047
		// (get) Token: 0x06001E13 RID: 7699 RVA: 0x0012AC94 File Offset: 0x00128E94
		// (set) Token: 0x06001E14 RID: 7700 RVA: 0x0012AC9C File Offset: 0x00128E9C
		public Sound FlashSound { get; private set; }

		// Token: 0x06001E15 RID: 7701 RVA: 0x0012ACA8 File Offset: 0x00128EA8
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

		// Token: 0x06001E16 RID: 7702 RVA: 0x0012ADF4 File Offset: 0x00128FF4
		public static LevelGenerationParams GetRandom(string seed, LevelData.LevelType type, float difficulty, Identifier biomeId = default(Identifier), bool pvpOnly = false, bool biomeTransition = false)
		{
			LevelGenerationParams.<>c__DisplayClass394_0 CS$<>8__locals1 = new LevelGenerationParams.<>c__DisplayClass394_0();
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
				LevelGenerationParams.<>c__DisplayClass394_0 CS$<>8__locals2 = CS$<>8__locals1;
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
				LevelGenerationParams.<>c__DisplayClass394_0 CS$<>8__locals3 = CS$<>8__locals1;
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

		// Token: 0x06001E17 RID: 7703 RVA: 0x0012B170 File Offset: 0x00129370
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
						{
							char c = text[0];
							if (c != 'b')
							{
								if (c == 'f')
								{
									if (text == "flashsound")
									{
										this.FlashSound = GameMain.SoundManager.LoadSound(subElement, false, null);
									}
								}
							}
							else if (text == "background")
							{
								this.BackgroundSprite = new Sprite(subElement, "", "", false, 1f);
							}
							break;
						}
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

		// Token: 0x06001E18 RID: 7704 RVA: 0x0012B59C File Offset: 0x0012979C
		public override void Dispose()
		{
		}

		// Token: 0x04000EF5 RID: 3829
		public static readonly PrefabCollection<LevelGenerationParams> LevelParams = new PrefabCollection<LevelGenerationParams>();

		// Token: 0x04000EF9 RID: 3833
		private int minWidth;

		// Token: 0x04000EFA RID: 3834
		private int maxWidth;

		// Token: 0x04000EFB RID: 3835
		private int height;

		// Token: 0x04000EFC RID: 3836
		private Point voronoiSiteInterval;

		// Token: 0x04000EFD RID: 3837
		private Point voronoiSiteVariance;

		// Token: 0x04000EFE RID: 3838
		private Point mainPathNodeIntervalRange;

		// Token: 0x04000EFF RID: 3839
		private int caveCount;

		// Token: 0x04000F00 RID: 3840
		private float bottomHoleProbability;

		// Token: 0x04000F01 RID: 3841
		private int seaFloorBaseDepth;

		// Token: 0x04000F02 RID: 3842
		private int seaFloorVariance;

		// Token: 0x04000F03 RID: 3843
		private int cellSubdivisionLength;

		// Token: 0x04000F04 RID: 3844
		private float cellRoundingAmount;

		// Token: 0x04000F05 RID: 3845
		private float cellIrregularity;

		// Token: 0x04000F06 RID: 3846
		private int mountainCountMin;

		// Token: 0x04000F07 RID: 3847
		private int mountainCountMax;

		// Token: 0x04000F08 RID: 3848
		private int mountainHeightMin;

		// Token: 0x04000F09 RID: 3849
		private int mountainHeightMax;

		// Token: 0x04000F0A RID: 3850
		private float waterParticleScale;

		// Token: 0x04000F0B RID: 3851
		private int initialDepthMin;

		// Token: 0x04000F0C RID: 3852
		private int initialDepthMax;

		// Token: 0x04000F0D RID: 3853
		public readonly ImmutableHashSet<Identifier> AllowedBiomeIdentifiers;

		// Token: 0x04000F0E RID: 3854
		public readonly bool AnyBiomeAllowed;

		// Token: 0x04000F16 RID: 3862
		private Vector2 startPosition;

		// Token: 0x04000F17 RID: 3863
		private Vector2 endPosition;

		// Token: 0x04000F18 RID: 3864
		private Vector2 forceOutpostPosition;

		// Token: 0x04000F40 RID: 3904
		private Vector2 waterParticleVelocity;
	}
}
