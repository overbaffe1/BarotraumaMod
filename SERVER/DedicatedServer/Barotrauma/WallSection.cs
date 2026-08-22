using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000259 RID: 601
	internal class WallSection : IIgnorable, ISpatialEntity
	{
		// Token: 0x17000CDA RID: 3290
		// (get) Token: 0x06002B3C RID: 11068 RVA: 0x0011C303 File Offset: 0x0011A503
		public Structure Wall { get; }

		// Token: 0x17000CDB RID: 3291
		// (get) Token: 0x06002B3D RID: 11069 RVA: 0x0011C30B File Offset: 0x0011A50B
		public Vector2 Position
		{
			get
			{
				return this.Wall.SectionPosition(this.Wall.Sections.IndexOf(this), false);
			}
		}

		// Token: 0x17000CDC RID: 3292
		// (get) Token: 0x06002B3E RID: 11070 RVA: 0x0011C32A File Offset: 0x0011A52A
		public Vector2 WorldPosition
		{
			get
			{
				return this.Wall.SectionPosition(this.Wall.Sections.IndexOf(this), true);
			}
		}

		// Token: 0x17000CDD RID: 3293
		// (get) Token: 0x06002B3F RID: 11071 RVA: 0x0011C349 File Offset: 0x0011A549
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x17000CDE RID: 3294
		// (get) Token: 0x06002B40 RID: 11072 RVA: 0x0011C356 File Offset: 0x0011A556
		public Submarine Submarine
		{
			get
			{
				return this.Wall.Submarine;
			}
		}

		// Token: 0x17000CDF RID: 3295
		// (get) Token: 0x06002B41 RID: 11073 RVA: 0x0011C364 File Offset: 0x0011A564
		public Rectangle WorldRect
		{
			get
			{
				if (this.Submarine != null)
				{
					return new Rectangle((int)((float)this.rect.X + this.Submarine.Position.X), (int)((float)this.rect.Y + this.Submarine.Position.Y), this.rect.Width, this.rect.Height);
				}
				return this.rect;
			}
		}

		// Token: 0x06002B42 RID: 11074 RVA: 0x0011C3D7 File Offset: 0x0011A5D7
		public bool IgnoreByAI(Character character)
		{
			return this.OrderedToBeIgnored && character.IsOnPlayerTeam;
		}

		// Token: 0x17000CE0 RID: 3296
		// (get) Token: 0x06002B43 RID: 11075 RVA: 0x0011C3E9 File Offset: 0x0011A5E9
		// (set) Token: 0x06002B44 RID: 11076 RVA: 0x0011C3F1 File Offset: 0x0011A5F1
		public bool OrderedToBeIgnored { get; set; }

		// Token: 0x06002B45 RID: 11077 RVA: 0x0011C3FA File Offset: 0x0011A5FA
		public WallSection(Rectangle rect, Structure wall, float damage = 0f)
		{
			this.rect = rect;
			this.damage = damage;
			this.Wall = wall;
		}

		// Token: 0x0400152C RID: 5420
		public Rectangle rect;

		// Token: 0x0400152D RID: 5421
		public float damage;

		// Token: 0x0400152E RID: 5422
		public Gap gap;

		// Token: 0x0400152F RID: 5423
		public bool NoPhysicsBody;
	}
}
