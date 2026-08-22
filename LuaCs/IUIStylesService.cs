using System;
using System.Collections.Immutable;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004ED RID: 1261
	public interface IUIStylesService : IReusableService, IService, IDisposable
	{
		// Token: 0x06005224 RID: 21028
		Result<GUIColor> GetColor(ContentPackage package, string internalName, string assetName);

		// Token: 0x06005225 RID: 21029
		Result<GUICursor> GetCursor(ContentPackage package, string internalName, string assetName);

		// Token: 0x06005226 RID: 21030
		Result<GUIFont> GetFont(ContentPackage package, string internalName, string assetName);

		// Token: 0x06005227 RID: 21031
		Result<GUISprite> GetSprite(ContentPackage package, string internalName, string assetName);

		// Token: 0x06005228 RID: 21032
		Result<GUISpriteSheet> GetSpriteSheet(ContentPackage package, string internalName, string assetName);

		// Token: 0x06005229 RID: 21033
		Result LoadAssets(ImmutableArray<IStylesResourceInfo> resources);

		// Token: 0x0600522A RID: 21034
		Result UnloadPackages(ImmutableArray<ContentPackage> packages);

		// Token: 0x0600522B RID: 21035
		Result UnloadPackage(ContentPackage package);

		// Token: 0x0600522C RID: 21036
		Result UnloadAllPackages();
	}
}
