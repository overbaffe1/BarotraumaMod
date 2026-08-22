using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200016D RID: 365
	public class GrimeSprite : Prefab
	{
		// Token: 0x06001D78 RID: 7544 RVA: 0x000D1FCC File Offset: 0x000D01CC
		public GrimeSprite(Sprite spr, DecalsFile file, int indexInFile)
		{
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 2);
			defaultInterpolatedStringHandler.AppendFormatted("GrimeSprite");
			defaultInterpolatedStringHandler.AppendFormatted<int>(indexInFile);
			base..ctor(file, defaultInterpolatedStringHandler.ToStringAndClear().ToIdentifier());
			this.Sprite = spr;
			this.IndexInFile = indexInFile;
		}

		// Token: 0x17000865 RID: 2149
		// (get) Token: 0x06001D79 RID: 7545 RVA: 0x000D2017 File Offset: 0x000D0217
		// (set) Token: 0x06001D7A RID: 7546 RVA: 0x000D201F File Offset: 0x000D021F
		public Sprite Sprite { get; private set; }

		// Token: 0x06001D7B RID: 7547 RVA: 0x000D2028 File Offset: 0x000D0228
		public override void Dispose()
		{
			Sprite sprite = this.Sprite;
			if (sprite != null)
			{
				sprite.Remove();
			}
			this.Sprite = null;
		}

		// Token: 0x04000D50 RID: 3408
		public readonly int IndexInFile;
	}
}
