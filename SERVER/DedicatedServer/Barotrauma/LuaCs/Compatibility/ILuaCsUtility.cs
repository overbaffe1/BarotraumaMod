using System;

namespace Barotrauma.LuaCs.Compatibility
{
	// Token: 0x02000490 RID: 1168
	public interface ILuaCsUtility : ILuaCsShim, IService, IDisposable
	{
		// Token: 0x06003E3F RID: 15935
		bool CanReadFromPath(string file);

		// Token: 0x06003E40 RID: 15936
		bool CanWriteToPath(string file);

		// Token: 0x06003E41 RID: 15937
		bool IsPathAllowedException(string path, bool write = true, LuaCsMessageOrigin origin = LuaCsMessageOrigin.Unknown);
	}
}
