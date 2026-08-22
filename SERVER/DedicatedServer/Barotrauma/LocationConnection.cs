using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000244 RID: 580
	internal class LocationConnection
	{
		// Token: 0x17000C14 RID: 3092
		// (get) Token: 0x06002919 RID: 10521 RVA: 0x0010B450 File Offset: 0x00109650
		// (set) Token: 0x0600291A RID: 10522 RVA: 0x0010B458 File Offset: 0x00109658
		public LevelData LevelData { get; set; }

		// Token: 0x17000C15 RID: 3093
		// (get) Token: 0x0600291B RID: 10523 RVA: 0x0010B461 File Offset: 0x00109661
		public Vector2 CenterPos
		{
			get
			{
				return (this.Locations[0].MapPosition + this.Locations[1].MapPosition) / 2f;
			}
		}

		// Token: 0x17000C16 RID: 3094
		// (get) Token: 0x0600291C RID: 10524 RVA: 0x0010B48C File Offset: 0x0010968C
		// (set) Token: 0x0600291D RID: 10525 RVA: 0x0010B494 File Offset: 0x00109694
		public Location[] Locations { get; private set; }

		// Token: 0x17000C17 RID: 3095
		// (get) Token: 0x0600291E RID: 10526 RVA: 0x0010B49D File Offset: 0x0010969D
		// (set) Token: 0x0600291F RID: 10527 RVA: 0x0010B4A5 File Offset: 0x001096A5
		public float Length { get; private set; }

		// Token: 0x17000C18 RID: 3096
		// (get) Token: 0x06002920 RID: 10528 RVA: 0x0010B4AE File Offset: 0x001096AE
		public IEnumerable<Mission> AvailableMissions
		{
			get
			{
				this.availableMissions.RemoveAll((Mission m) => m.Completed || (m.Failed && !m.Prefab.AllowRetry) || m.ForceFailure);
				return this.availableMissions;
			}
		}

		// Token: 0x06002921 RID: 10529 RVA: 0x0010B4E4 File Offset: 0x001096E4
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

		// Token: 0x06002922 RID: 10530 RVA: 0x0010B563 File Offset: 0x00109763
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

		// Token: 0x04001422 RID: 5154
		public Biome Biome;

		// Token: 0x04001423 RID: 5155
		public float Difficulty;

		// Token: 0x04001424 RID: 5156
		public readonly List<Vector2[]> CrackSegments = new List<Vector2[]>();

		// Token: 0x04001425 RID: 5157
		public bool Passed;

		// Token: 0x04001426 RID: 5158
		public bool Locked;

		// Token: 0x0400142A RID: 5162
		private readonly List<Mission> availableMissions = new List<Mission>();
	}
}
