using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200024F RID: 591
	internal class OrderTarget : ISpatialEntity
	{
		// Token: 0x17000C9A RID: 3226
		// (get) Token: 0x06002A7C RID: 10876 RVA: 0x00115729 File Offset: 0x00113929
		// (set) Token: 0x06002A7D RID: 10877 RVA: 0x00115731 File Offset: 0x00113931
		public Vector2 Position { get; private set; }

		// Token: 0x17000C9B RID: 3227
		// (get) Token: 0x06002A7E RID: 10878 RVA: 0x0011573A File Offset: 0x0011393A
		// (set) Token: 0x06002A7F RID: 10879 RVA: 0x00115742 File Offset: 0x00113942
		public Hull Hull { get; private set; }

		// Token: 0x17000C9C RID: 3228
		// (get) Token: 0x06002A80 RID: 10880 RVA: 0x0011574B File Offset: 0x0011394B
		public Vector2 WorldPosition
		{
			get
			{
				if (this.Submarine != null)
				{
					return this.Position + this.Submarine.Position;
				}
				return this.Position;
			}
		}

		// Token: 0x17000C9D RID: 3229
		// (get) Token: 0x06002A81 RID: 10881 RVA: 0x00115772 File Offset: 0x00113972
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x17000C9E RID: 3230
		// (get) Token: 0x06002A82 RID: 10882 RVA: 0x0011577F File Offset: 0x0011397F
		public Submarine Submarine
		{
			get
			{
				Hull hull = this.Hull;
				if (hull == null)
				{
					return null;
				}
				return hull.Submarine;
			}
		}

		// Token: 0x06002A83 RID: 10883 RVA: 0x00115792 File Offset: 0x00113992
		public OrderTarget(Vector2 position, Hull hull, bool creatingFromExistingData = false)
		{
			if (!creatingFromExistingData && ((hull != null) ? hull.Submarine : null) != null)
			{
				position -= hull.Submarine.Position;
			}
			this.Position = position;
			this.Hull = hull;
		}
	}
}
