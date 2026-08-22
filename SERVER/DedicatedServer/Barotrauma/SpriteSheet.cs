using System;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000290 RID: 656
	public class SpriteSheet : Sprite
	{
		// Token: 0x17000D86 RID: 3462
		// (get) Token: 0x06002E10 RID: 11792 RVA: 0x00130BD4 File Offset: 0x0012EDD4
		public int FrameCount
		{
			get
			{
				return this.sourceRects.Length - this.emptyFrames;
			}
		}

		// Token: 0x17000D87 RID: 3463
		// (get) Token: 0x06002E11 RID: 11793 RVA: 0x00130BE5 File Offset: 0x0012EDE5
		// (set) Token: 0x06002E12 RID: 11794 RVA: 0x00130BED File Offset: 0x0012EDED
		public Point FrameSize { get; private set; }

		// Token: 0x06002E13 RID: 11795 RVA: 0x00130BF8 File Offset: 0x0012EDF8
		public SpriteSheet(ContentXElement element, string path = "", string file = "") : base(element, path, file, false, 1f)
		{
			int columnCount = Math.Max(element.GetAttributeInt("columns", 1), 1);
			int rowCount = Math.Max(element.GetAttributeInt("rows", 1), 1);
			string key = "origin";
			Vector2 vector = new Vector2(0.5f, 0.5f);
			this.origin = element.GetAttributeVector2(key, vector);
			this.emptyFrames = element.GetAttributeInt("emptyframes", 0);
			this.Init(columnCount, rowCount);
		}

		// Token: 0x06002E14 RID: 11796 RVA: 0x00130C77 File Offset: 0x0012EE77
		public SpriteSheet(string filePath, int columnCount, int rowCount, Vector2 origin, Rectangle? sourceRect = null) : base(filePath, origin)
		{
			this.origin = origin;
			if (sourceRect != null)
			{
				base.SourceRect = sourceRect.Value;
			}
			this.Init(columnCount, rowCount);
		}

		// Token: 0x06002E15 RID: 11797 RVA: 0x00130CA8 File Offset: 0x0012EEA8
		private void Init(int columnCount, int rowCount)
		{
			this.sourceRects = new Rectangle[rowCount * columnCount];
			float cellWidth = (float)(base.SourceRect.Width / columnCount);
			float cellHeight = (float)(base.SourceRect.Height / rowCount);
			this.FrameSize = new Point((int)cellWidth, (int)cellHeight);
			for (int x = 0; x < columnCount; x++)
			{
				for (int y = 0; y < rowCount; y++)
				{
					this.sourceRects[x + y * columnCount] = new Rectangle((int)((float)base.SourceRect.X + (float)x * cellWidth), (int)((float)base.SourceRect.Y + (float)y * cellHeight), (int)cellWidth, (int)cellHeight);
				}
			}
			this.origin.X = this.origin.X * cellWidth;
			this.origin.Y = this.origin.Y * cellHeight;
		}

		// Token: 0x04001685 RID: 5765
		private Rectangle[] sourceRects;

		// Token: 0x04001686 RID: 5766
		private int emptyFrames;
	}
}
