using System;

namespace Barotrauma
{
	// Token: 0x02000229 RID: 553
	internal class BallastFloraPrefab : Prefab
	{
		// Token: 0x17000ADB RID: 2779
		// (get) Token: 0x060025E3 RID: 9699 RVA: 0x000F6363 File Offset: 0x000F4563
		public string OriginalName { get; }

		// Token: 0x17000ADC RID: 2780
		// (get) Token: 0x060025E4 RID: 9700 RVA: 0x000F636B File Offset: 0x000F456B
		public LocalizedString DisplayName { get; }

		// Token: 0x17000ADD RID: 2781
		// (get) Token: 0x060025E5 RID: 9701 RVA: 0x000F6373 File Offset: 0x000F4573
		public ContentXElement Element { get; }

		// Token: 0x060025E6 RID: 9702 RVA: 0x000F637C File Offset: 0x000F457C
		public BallastFloraPrefab(ContentXElement element, BallastFloraFile file) : base(file, element.GetAttributeIdentifier("identifier", ""))
		{
			this.OriginalName = element.GetAttributeString("name", "");
			this.DisplayName = TextManager.Get(this.Identifier).Fallback(this.OriginalName, true);
			this.Element = element;
		}

		// Token: 0x060025E7 RID: 9703 RVA: 0x000F63DF File Offset: 0x000F45DF
		public static BallastFloraPrefab Find(Identifier identifier)
		{
			if (!BallastFloraPrefab.Prefabs.ContainsKey(identifier))
			{
				return null;
			}
			return BallastFloraPrefab.Prefabs[identifier];
		}

		// Token: 0x060025E8 RID: 9704 RVA: 0x000F63FB File Offset: 0x000F45FB
		public override void Dispose()
		{
		}

		// Token: 0x0400126F RID: 4719
		public bool Disposed;

		// Token: 0x04001270 RID: 4720
		public static readonly PrefabCollection<BallastFloraPrefab> Prefabs = new PrefabCollection<BallastFloraPrefab>();
	}
}
