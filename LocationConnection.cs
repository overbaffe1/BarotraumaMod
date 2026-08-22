using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200031F RID: 799
	internal class LocationConnection
	{
		// Token: 0x170010CE RID: 4302
		// (get) Token: 0x06003FD1 RID: 16337 RVA: 0x0023AC24 File Offset: 0x00238E24
		// (set) Token: 0x06003FD2 RID: 16338 RVA: 0x0023AC2C File Offset: 0x00238E2C
		public LevelData LevelData { get; set; }

		// Token: 0x170010CF RID: 4303
		// (get) Token: 0x06003FD3 RID: 16339 RVA: 0x0023AC35 File Offset: 0x00238E35
		public Vector2 CenterPos
		{
			get
			{
				return (this.Locations[0].MapPosition + this.Locations[1].MapPosition) / 2f;
			}
		}

		// Token: 0x170010D0 RID: 4304
		// (get) Token: 0x06003FD4 RID: 16340 RVA: 0x0023AC60 File Offset: 0x00238E60
		// (set) Token: 0x06003FD5 RID: 16341 RVA: 0x0023AC68 File Offset: 0x00238E68
		public Location[] Locations { get; private set; }

		// Token: 0x170010D1 RID: 4305
		// (get) Token: 0x06003FD6 RID: 16342 RVA: 0x0023AC71 File Offset: 0x00238E71
		// (set) Token: 0x06003FD7 RID: 16343 RVA: 0x0023AC79 File Offset: 0x00238E79
		public float Length { get; private set; }

		// Token: 0x170010D2 RID: 4306
		// (get) Token: 0x06003FD8 RID: 16344 RVA: 0x0023AC82 File Offset: 0x00238E82
		public IEnumerable<Mission> AvailableMissions
		{
			get
			{
				this.availableMissions.RemoveAll((Mission m) => m.Completed || (m.Failed && !m.Prefab.AllowRetry) || m.ForceFailure);
				return this.availableMissions;
			}
		}

		// Token: 0x06003FD9 RID: 16345 RVA: 0x0023ACB8 File Offset: 0x00238EB8
		public LocationConnection(Location location1, Location location2)
		{
			if (location1 == null)
			{
				throw new ArgumentException("Invalid location connection: location1 was null");
			}
			if (location2 == null)
			{
				throw new ArgumentException("Invalid location connection: location2 was null");
			}
			if (location1 == location2)
			{
				throw new ArgumentException("Invalid location connection: location1 was the same as location2");
			}
			this.Locations = new Location[]
			{
				location1,
				location2
			};
			this.Length = Vector2.Distance(location1.MapPosition, location2.MapPosition);
		}

		// Token: 0x06003FDA RID: 16346 RVA: 0x0023AD37 File Offset: 0x00238F37
		public Location OtherLocation(Location location)
		{
			if (this.Locations[0] == location)
			{
				return this.Locations[1];
			}
			if (this.Locations[1] == location)
			{
				return this.Locations[0];
			}
			return null;
		}

		// Token: 0x0400211D RID: 8477
		public Biome Biome;

		// Token: 0x0400211E RID: 8478
		public float Difficulty;

		// Token: 0x0400211F RID: 8479
		public readonly List<Vector2[]> CrackSegments = new List<Vector2[]>();

		// Token: 0x04002120 RID: 8480
		public bool Passed;

		// Token: 0x04002121 RID: 8481
		public bool Locked;

		// Token: 0x04002125 RID: 8485
		private readonly List<Mission> availableMissions = new List<Mission>();
	}
}
