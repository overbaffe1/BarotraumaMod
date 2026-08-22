using System;
using System.Collections.Immutable;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000521 RID: 1313
	public interface ISafeStorageValidation
	{
		// Token: 0x06005453 RID: 21587
		bool IsFileAccessible(string path, bool readOnly, bool checkWhitelistOnly = true);

		// Token: 0x06005454 RID: 21588
		void AddFileToWhitelist(string path, bool readOnly = true);

		// Token: 0x06005455 RID: 21589
		void AddFilesToWhitelist(ImmutableArray<string> paths, bool readOnly = true);

		// Token: 0x06005456 RID: 21590
		void RemoveFileFromAllWhitelists(string path);

		// Token: 0x06005457 RID: 21591
		Result SetReadOnlyWhitelist(ImmutableArray<string> filePaths);

		// Token: 0x06005458 RID: 21592
		Result SetReadWriteWhitelist(ImmutableArray<string> filePaths);

		// Token: 0x06005459 RID: 21593
		void ClearAllWhitelists();
	}
}
