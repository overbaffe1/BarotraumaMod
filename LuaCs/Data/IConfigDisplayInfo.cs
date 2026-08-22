using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000575 RID: 1397
	public interface IConfigDisplayInfo
	{
		// Token: 0x1700153F RID: 5439
		// (get) Token: 0x060055FE RID: 22014
		string DisplayName { get; }

		// Token: 0x17001540 RID: 5440
		// (get) Token: 0x060055FF RID: 22015
		string Description { get; }

		// Token: 0x17001541 RID: 5441
		// (get) Token: 0x06005600 RID: 22016
		string DisplayCategory { get; }

		// Token: 0x17001542 RID: 5442
		// (get) Token: 0x06005601 RID: 22017
		bool ShowInMenus { get; }

		// Token: 0x17001543 RID: 5443
		// (get) Token: 0x06005602 RID: 22018
		string Tooltip { get; }

		// Token: 0x17001544 RID: 5444
		// (get) Token: 0x06005603 RID: 22019
		ContentPath ImageIconPath { get; }
	}
}
