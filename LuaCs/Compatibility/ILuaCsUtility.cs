using System;

namespace Barotrauma.LuaCs.Compatibility
{
	// Token: 0x02000573 RID: 1395
	public interface ILuaCsUtility : ILuaCsShim, IService, IDisposable
	{
		// Token: 0x060055F7 RID: 22007
		bool CanReadFromPath(string file);

		// Token: 0x060055F8 RID: 22008
		bool CanWriteToPath(string file);

		// Token: 0x060055F9 RID: 22009
		bool IsPathAllowedException(string path, bool write = true, LuaCsMessageOrigin origin = LuaCsMessageOrigin.Unknown);
	}
}
