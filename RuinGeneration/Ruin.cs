using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Voronoi2;

namespace Barotrauma.RuinGeneration
{
	// Token: 0x020004D7 RID: 1239
	internal class Ruin
	{
		// Token: 0x060050CA RID: 20682 RVA: 0x002B8638 File Offset: 0x002B6838
		public void DebugDraw(SpriteBatch spriteBatch)
		{
			Rectangle drawRect = this.Area;
			drawRect.Y = -drawRect.Y - this.Area.Height;
			GUI.DrawRectangle(spriteBatch, drawRect, Color.Cyan, false, 0f, 6f);
		}

		// Token: 0x170014A1 RID: 5281
		// (get) Token: 0x060050CB RID: 20683 RVA: 0x002B867D File Offset: 0x002B687D
		// (set) Token: 0x060050CC RID: 20684 RVA: 0x002B8685 File Offset: 0x002B6885
		public Rectangle Area { get; private set; }

		// Token: 0x170014A2 RID: 5282
		// (get) Token: 0x060050CD RID: 20685 RVA: 0x002B868E File Offset: 0x002B688E
		// (set) Token: 0x060050CE RID: 20686 RVA: 0x002B8696 File Offset: 0x002B6896
		public Submarine Submarine { get; private set; }

		// Token: 0x060050CF RID: 20687 RVA: 0x002B869F File Offset: 0x002B689F
		public Ruin(Level level, RuinGenerationParams generationParams, Location location, Point position, bool mirror = false) : this(level, generationParams, location.Type, position, mirror)
		{
		}

		// Token: 0x060050D0 RID: 20688 RVA: 0x002B86B3 File Offset: 0x002B68B3
		public Ruin(Level level, RuinGenerationParams generationParams, LocationType locationType, Point position, bool mirror = false)
		{
			this.generationParams = generationParams;
			this.Generate(level, locationType, position, mirror);
		}

		// Token: 0x060050D1 RID: 20689 RVA: 0x002B86DC File Offset: 0x002B68DC
		public void Generate(Level level, LocationType locationType, Point position, bool mirror = false)
		{
			this.Submarine = OutpostGenerator.Generate(this.generationParams, locationType, false, level.LevelData.AllowInvalidOutpost);
			this.Submarine.Info.Name = "Ruin (" + level.Seed + ")";
			this.Submarine.Info.Type = SubmarineType.Ruin;
			this.Submarine.TeamID = CharacterTeamType.None;
			position.Y = Math.Min(level.Size.Y - this.Submarine.Borders.Height / 2 - 100, position.Y);
			this.Submarine.SetPosition(position.ToVector2(), null, false);
			if (mirror)
			{
				this.Submarine.FlipX(null);
			}
			Rectangle worldBorders = this.Submarine.Borders;
			worldBorders.Location += this.Submarine.WorldPosition.ToPoint();
			this.Area = new Rectangle(worldBorders.X, worldBorders.Y - worldBorders.Height, worldBorders.Width, worldBorders.Height);
			List<WayPoint> waypoints = WayPoint.WayPointList.FindAll((WayPoint wp) => wp.Ruin == this || wp.Submarine == this.Submarine);
			int interestingPosCount = 0;
			foreach (WayPoint wp2 in waypoints)
			{
				if (wp2.SpawnType == SpawnType.Enemy)
				{
					level.PositionsOfInterest.Add(new Level.InterestingPosition(wp2.WorldPosition.ToPoint(), Level.PositionType.Ruin, this, true));
					interestingPosCount++;
				}
			}
			if (interestingPosCount == 0)
			{
				level.PositionsOfInterest.Add(new Level.InterestingPosition(waypoints.GetRandom(Rand.RandSync.ServerAndClient).WorldPosition.ToPoint(), Level.PositionType.Ruin, this, true));
			}
		}

		// Token: 0x04002ABB RID: 10939
		private readonly RuinGenerationParams generationParams;

		// Token: 0x04002ABC RID: 10940
		public List<VoronoiCell> PathCells = new List<VoronoiCell>();
	}
}
