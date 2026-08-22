using System;
using System.Collections.Generic;
using Barotrauma.LuaCs.Data;
using FluentResults;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004EC RID: 1260
	public interface IUIStylesCollection : IService, IDisposable
	{
		// Token: 0x170014D7 RID: 5335
		// (get) Token: 0x0600521B RID: 21019
		ContentPath Path { get; }

		// Token: 0x0600521C RID: 21020
		Result<GUIFont> GetFont(string name);

		// Token: 0x0600521D RID: 21021
		Result<GUISprite> GetSprite(string name);

		// Token: 0x0600521E RID: 21022
		Result<GUISpriteSheet> GetSpriteSheet(string name);

		// Token: 0x0600521F RID: 21023
		Result<GUICursor> GetCursor(string name);

		// Token: 0x06005220 RID: 21024
		Result<GUIColor> GetColor(string name);

		// Token: 0x06005221 RID: 21025
		void LoadFile();

		// Token: 0x06005222 RID: 21026
		void UnloadFile();

		// Token: 0x06005223 RID: 21027
		void Sort();

		// Token: 0x020012AC RID: 4780
		public interface IFactory : IService, IDisposable
		{
			// Token: 0x060094DF RID: 38111
			IEnumerable<IUIStylesCollection> CreateInstance(IStylesResourceInfo info, IStorageService storageService);
		}
	}
}
