using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x020000A9 RID: 169
	public static class HUD
	{
		// Token: 0x06001585 RID: 5509 RVA: 0x000C9F58 File Offset: 0x000C8158
		public static bool CloseHUD(Rectangle rect)
		{
			if (PlayerInput.KeyHit(Keys.Escape))
			{
				return true;
			}
			if (GUI.MouseOn != null)
			{
				return false;
			}
			if (Inventory.IsMouseOnInventory)
			{
				return false;
			}
			bool input = PlayerInput.PrimaryMouseButtonDown() || PlayerInput.SecondaryMouseButtonClicked();
			return input && !rect.Contains(PlayerInput.MousePosition);
		}
	}
}
