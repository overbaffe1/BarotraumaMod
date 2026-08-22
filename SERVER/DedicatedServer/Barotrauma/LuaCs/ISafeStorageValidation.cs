using System;
using System.Collections.Immutable;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x0200040E RID: 1038
	public interface ISafeStorageValidation
	{
		// Token: 0x06003B37 RID: 15159
		bool IsFileAccessible(string path, bool readOnly, bool checkWhitelistOnly = true);

		// Token: 0x06003B38 RID: 15160
		void AddFileToWhitelist(string path, bool readOnly = true);

		// Token: 0x06003B39 RID: 15161
		void AddFilesToWhitelist(ImmutableArray<string> paths, bool readOnly = true);

		// Token: 0x06003B3A RID: 15162
		void RemoveFileFromAllWhitelists(string path);

		// Token: 0x06003B3B RID: 15163
		Result SetReadOnlyWhitelist(ImmutableArray<string> filePaths);

		// Token: 0x06003B3C RID: 15164
		Result SetReadWriteWhitelist(ImmutableArray<string> filePaths);

		// Token: 0x06003B3D RID: 15165
		void ClearAllWhitelists();
	}
}
