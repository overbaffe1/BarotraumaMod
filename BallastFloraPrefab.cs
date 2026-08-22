using System;

namespace Barotrauma
{
	// Token: 0x02000311 RID: 785
	internal class BallastFloraPrefab : Prefab
	{
		// Token: 0x17001073 RID: 4211
		// (get) Token: 0x06003ECA RID: 16074 RVA: 0x00233D1B File Offset: 0x00231F1B
		public string OriginalName { get; }

		// Token: 0x17001074 RID: 4212
		// (get) Token: 0x06003ECB RID: 16075 RVA: 0x00233D23 File Offset: 0x00231F23
		public LocalizedString DisplayName { get; }

		// Token: 0x17001075 RID: 4213
		// (get) Token: 0x06003ECC RID: 16076 RVA: 0x00233D2B File Offset: 0x00231F2B
		public ContentXElement Element { get; }

		// Token: 0x06003ECD RID: 16077 RVA: 0x00233D34 File Offset: 0x00231F34
		public BallastFloraPrefab(ContentXElement element, BallastFloraFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.OriginalName = element.GetAttributeString("name", "");
			this.DisplayName = TextManager.Get(this.Identifier).Fallback(this.OriginalName, true);
			this.Element = element;
		}

		// Token: 0x06003ECE RID: 16078 RVA: 0x00233D97 File Offset: 0x00231F97
		public static BallastFloraPrefab Find(Identifier identifier)
		{
			if (!BallastFloraPrefab.Prefabs.ContainsKey(identifier))
			{
				return null;
			}
			return BallastFloraPrefab.Prefabs[identifier];
		}

		// Token: 0x06003ECF RID: 16079 RVA: 0x00233DB3 File Offset: 0x00231FB3
		public override void Dispose()
		{
		}

		// Token: 0x0400209B RID: 8347
		public bool Disposed;

		// Token: 0x0400209C RID: 8348
		public static readonly PrefabCollection<BallastFloraPrefab> Prefabs = new PrefabCollection<BallastFloraPrefab>();
	}
}
