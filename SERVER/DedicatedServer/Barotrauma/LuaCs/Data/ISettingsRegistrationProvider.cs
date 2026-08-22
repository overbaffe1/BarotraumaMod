using System;
using System.Xml.Linq;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000489 RID: 1161
	public interface ISettingsRegistrationProvider : IService, IDisposable
	{
		// Token: 0x06003E1B RID: 15899
		void RegisterTypeProviders(IConfigService configService, Func<OneOf<string, XElement, object>, bool> valueChangePredicate);
	}
}
