using System;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000599 RID: 1433
	public interface IConfigServiceConfig : IService, IDisposable
	{
		// Token: 0x170015AA RID: 5546
		// (get) Token: 0x06005724 RID: 22308
		string LocalConfigPathPartial { get; }

		// Token: 0x170015AB RID: 5547
		// (get) Token: 0x06005725 RID: 22309
		string FileNamePattern { get; }
	}
}
