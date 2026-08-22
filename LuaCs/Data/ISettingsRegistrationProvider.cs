using System;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x020005A5 RID: 1445
	public interface ISettingsRegistrationProvider : IService, IDisposable
	{
		// Token: 0x0600578A RID: 22410
		void RegisterTypeProviders(IConfigService configService, Func<OneOf<string, XElement, object>, bool> valueChangePredicate);
	}
}
