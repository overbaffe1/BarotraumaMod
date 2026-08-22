using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x0200047D RID: 1149
	public interface IConfigServiceConfig : IService, IDisposable
	{
		// Token: 0x17001044 RID: 4164
		// (get) Token: 0x06003DBB RID: 15803
		string LocalConfigPathPartial { get; }

		// Token: 0x17001045 RID: 4165
		// (get) Token: 0x06003DBC RID: 15804
		string FileNamePattern { get; }
	}
}
