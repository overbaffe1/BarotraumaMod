using System;
using System.Collections.Generic;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000578 RID: 1400
	public interface ISettingControl : ISettingBase, IDisplayable, IDataInfo, IEqualityComparer<IDataInfo>, IEquatable<IDataInfo>, IEquatable<ISettingBase>, IDisposable
	{
		// Token: 0x17001546 RID: 5446
		// (get) Token: 0x0600560F RID: 22031
		KeyOrMouse Value { get; }

		// Token: 0x06005610 RID: 22032
		bool TrySetValue(KeyOrMouse value);

		// Token: 0x06005611 RID: 22033
		bool IsDown();

		// Token: 0x06005612 RID: 22034
		bool IsHit();
	}
}
