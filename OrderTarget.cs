using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000326 RID: 806
	internal class OrderTarget : ISpatialEntity
	{
		// Token: 0x1700111D RID: 4381
		// (get) Token: 0x0600407E RID: 16510 RVA: 0x0023DA7E File Offset: 0x0023BC7E
		// (set) Token: 0x0600407F RID: 16511 RVA: 0x0023DA86 File Offset: 0x0023BC86
		public Vector2 Position { get; private set; }

		// Token: 0x1700111E RID: 4382
		// (get) Token: 0x06004080 RID: 16512 RVA: 0x0023DA8F File Offset: 0x0023BC8F
		// (set) Token: 0x06004081 RID: 16513 RVA: 0x0023DA97 File Offset: 0x0023BC97
		public Hull Hull { get; private set; }

		// Token: 0x1700111F RID: 4383
		// (get) Token: 0x06004082 RID: 16514 RVA: 0x0023DAA0 File Offset: 0x0023BCA0
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

		// Token: 0x17001120 RID: 4384
		// (get) Token: 0x06004083 RID: 16515 RVA: 0x0023DAC7 File Offset: 0x0023BCC7
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x17001121 RID: 4385
		// (get) Token: 0x06004084 RID: 16516 RVA: 0x0023DAD4 File Offset: 0x0023BCD4
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

		// Token: 0x06004085 RID: 16517 RVA: 0x0023DAE7 File Offset: 0x0023BCE7
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
