using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x020002AE RID: 686
	internal class UnlockPathAction : EventAction
	{
		// Token: 0x06003BB3 RID: 15283 RVA: 0x00224552 File Offset: 0x00222752
		public static void ResetPathsUnlockedThisRound()
		{
			UnlockPathAction.pathsUnlockedThisRound.Clear();
		}

		// Token: 0x06003BB4 RID: 15284 RVA: 0x0022455E File Offset: 0x0022275E
		public UnlockPathAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06003BB5 RID: 15285 RVA: 0x00224568 File Offset: 0x00222768
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06003BB6 RID: 15286 RVA: 0x00224570 File Offset: 0x00222770
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06003BB7 RID: 15287 RVA: 0x0022457C File Offset: 0x0022277C
		public override void Update(float deltaTime)
		{
			if (this.isFinished)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			bool flag;
			if (gameSession == null)
			{
				flag = (null != null);
			}
			else
			{
				Map map = gameSession.Map;
				if (map == null)
				{
					flag = (null != null);
				}
				else
				{
					Location currentLocation = map.CurrentLocation;
					flag = (((currentLocation != null) ? currentLocation.Connections : null) != null);
				}
			}
			if (flag)
			{
				GameSession gameSession2 = GameMain.GameSession;
				List<LocationConnection> list;
				if (gameSession2 == null)
				{
					list = null;
				}
				else
				{
					Map map2 = gameSession2.Map;
					if (map2 == null)
					{
						list = null;
					}
					else
					{
						Location currentLocation2 = map2.CurrentLocation;
						list = ((currentLocation2 != null) ? currentLocation2.Connections : null);
					}
				}
				foreach (LocationConnection connection in list)
				{
					if (connection.Locked)
					{
						connection.Locked = false;
						UnlockPathAction.pathsUnlockedThisRound.Add(connection);
						new GUIMessageBox(string.Empty, TextManager.Get("pathunlockedgeneric"), Array.Empty<LocalizedString>(), new Vector2?(new Vector2(0.3f, 0.15f)), new Point?(new Point(512, 128)), Alignment.TopLeft, GUIMessageBox.Type.InGame, "", null, "UnlockPathIcon", null, null, false);
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06003BB8 RID: 15288 RVA: 0x002246A8 File Offset: 0x002228A8
		public override string ToDebugString()
		{
			return ToolBox.GetDebugSymbol(this.isFinished, false) + " UnlockPathAction";
		}

		// Token: 0x04001E7E RID: 7806
		private static readonly HashSet<LocationConnection> pathsUnlockedThisRound = new HashSet<LocationConnection>();

		// Token: 0x04001E7F RID: 7807
		private bool isFinished;
	}
}
