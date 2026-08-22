using System;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs.Data
{
	// Token: 0x02000576 RID: 1398
	public interface IDisplayable
	{
		// Token: 0x06005604 RID: 22020
		void AddDisplayComponent(GUILayoutGroup layoutGroup, Vector2 relativeSize, Action<string> onSerializedValue);
	}
}
