using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000261 RID: 609
	public class GrimeSprite : Prefab
	{
		// Token: 0x06003844 RID: 14404 RVA: 0x00217604 File Offset: 0x00215804
		public GrimeSprite(Sprite spr, DecalsFile file, int indexInFile)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted("GrimeSprite");
			defaultInterpolatedStringHandler.AppendFormatted<int>(indexInFile);
			base..ctor(file, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
			this.Sprite = spr;
			this.IndexInFile = indexInFile;
		}

		// Token: 0x17000EB9 RID: 3769
		// (get) Token: 0x06003845 RID: 14405 RVA: 0x0021764F File Offset: 0x0021584F
		// (set) Token: 0x06003846 RID: 14406 RVA: 0x00217657 File Offset: 0x00215857
		public Sprite Sprite { get; private set; }

		// Token: 0x06003847 RID: 14407 RVA: 0x00217660 File Offset: 0x00215860
		public override void Dispose()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.Sprite = null;
		}

		// Token: 0x04001C47 RID: 7239
		public readonly int IndexInFile;
	}
}
