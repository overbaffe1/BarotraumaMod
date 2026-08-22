using System;
using System.Collections.Generic;
using Barotrauma.Extensions;
using Microsoft.Xna.Framework;
using Voronoi2;

namespace Barotrauma.RuinGeneration
{
	// Token: 0x020002E3 RID: 739
	internal class Ruin
	{
		// Token: 0x17000E05 RID: 3589
		// (get) Token: 0x0600315F RID: 12639 RVA: 0x0015114A File Offset: 0x0014F34A
		// (set) Token: 0x06003160 RID: 12640 RVA: 0x00151152 File Offset: 0x0014F352
		public Rectangle Area { get; private set; }

		// Token: 0x17000E06 RID: 3590
		// (get) Token: 0x06003161 RID: 12641 RVA: 0x0015115B File Offset: 0x0014F35B
		// (set) Token: 0x06003162 RID: 12642 RVA: 0x00151163 File Offset: 0x0014F363
		public Submarine Submarine { get; private set; }

		// Token: 0x06003163 RID: 12643 RVA: 0x0015116C File Offset: 0x0014F36C
		public Ruin(Level level, RuinGenerationParams generationParams, Location location, Point position, bool mirror = false) : this(level, generationParams, location.Type, position, mirror)
		{
		}

		// Token: 0x06003164 RID: 12644 RVA: 0x00151180 File Offset: 0x0014F380
		public Ruin(Level level, RuinGenerationParams generationParams, LocationType locationType, Point position, bool mirror = false)
		{
			this.generationParams = generationParams;
			this.Generate(level, locationType, position, mirror);
		}

		// Token: 0x06003165 RID: 12645 RVA: 0x001511A8 File Offset: 0x0014F3A8
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

		// Token: 0x04001868 RID: 6248
		private readonly RuinGenerationParams generationParams;

		// Token: 0x04001869 RID: 6249
		public List<VoronoiCell> PathCells = new List<VoronoiCell>();
	}
}
