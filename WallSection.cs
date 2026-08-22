using System;
using FarseerPhysics;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000330 RID: 816
	internal class WallSection : IIgnorable, ISpatialEntity
	{
		// Token: 0x1700115D RID: 4445
		// (get) Token: 0x0600413E RID: 16702 RVA: 0x002445D3 File Offset: 0x002427D3
		public Structure Wall { get; }

		// Token: 0x1700115E RID: 4446
		// (get) Token: 0x0600413F RID: 16703 RVA: 0x002445DB File Offset: 0x002427DB
		public Vector2 Position
		{
			get
			{
				return this.Wall.SectionPosition(this.Wall.Sections.IndexOf(this), false);
			}
		}

		// Token: 0x1700115F RID: 4447
		// (get) Token: 0x06004140 RID: 16704 RVA: 0x002445FA File Offset: 0x002427FA
		public Vector2 WorldPosition
		{
			get
			{
				return this.Wall.SectionPosition(this.Wall.Sections.IndexOf(this), true);
			}
		}

		// Token: 0x17001160 RID: 4448
		// (get) Token: 0x06004141 RID: 16705 RVA: 0x00244619 File Offset: 0x00242819
		public Vector2 SimPosition
		{
			get
			{
				return ConvertUnits.ToSimUnits(this.Position);
			}
		}

		// Token: 0x17001161 RID: 4449
		// (get) Token: 0x06004142 RID: 16706 RVA: 0x00244626 File Offset: 0x00242826
		public Submarine Submarine
		{
			get
			{
				return this.Wall.Submarine;
			}
		}

		// Token: 0x17001162 RID: 4450
		// (get) Token: 0x06004143 RID: 16707 RVA: 0x00244634 File Offset: 0x00242834
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

		// Token: 0x06004144 RID: 16708 RVA: 0x002446A7 File Offset: 0x002428A7
		public bool IgnoreByAI(Character character)
		{
			return this.OrderedToBeIgnored && character.IsOnPlayerTeam;
		}

		// Token: 0x17001163 RID: 4451
		// (get) Token: 0x06004145 RID: 16709 RVA: 0x002446B9 File Offset: 0x002428B9
		// (set) Token: 0x06004146 RID: 16710 RVA: 0x002446C1 File Offset: 0x002428C1
		public bool OrderedToBeIgnored { get; set; }

		// Token: 0x06004147 RID: 16711 RVA: 0x002446CA File Offset: 0x002428CA
		public WallSection(Rectangle rect, Structure wall, float damage = 0f)
		{
			this.rect = rect;
			this.damage = damage;
			this.Wall = wall;
		}

		// Token: 0x040021EF RID: 8687
		public Rectangle rect;

		// Token: 0x040021F0 RID: 8688
		public float damage;

		// Token: 0x040021F1 RID: 8689
		public Gap gap;

		// Token: 0x040021F2 RID: 8690
		public bool NoPhysicsBody;
	}
}
