using System;
using System.Collections.Generic;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x020001BC RID: 444
	internal class UnlockPathAction : EventAction
	{
		// Token: 0x06002112 RID: 8466 RVA: 0x000DE61E File Offset: 0x000DC81E
		public static void ResetPathsUnlockedThisRound()
		{
			UnlockPathAction.pathsUnlockedThisRound.Clear();
		}

		// Token: 0x06002113 RID: 8467 RVA: 0x000DE62A File Offset: 0x000DC82A
		public UnlockPathAction(ScriptedEvent parentEvent, ContentXElement element) : base(parentEvent, element)
		{
		}

		// Token: 0x06002114 RID: 8468 RVA: 0x000DE634 File Offset: 0x000DC834
		public override bool IsFinished(ref string goTo)
		{
			return this.isFinished;
		}

		// Token: 0x06002115 RID: 8469 RVA: 0x000DE63C File Offset: 0x000DC83C
		public override void Reset()
		{
			this.isFinished = false;
		}

		// Token: 0x06002116 RID: 8470 RVA: 0x000DE648 File Offset: 0x000DC848
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
						UnlockPathAction.NotifyUnlock(connection);
					}
				}
			}
			this.isFinished = true;
		}

		// Token: 0x06002117 RID: 8471 RVA: 0x000DE714 File Offset: 0x000DC914
		public override string ToDebugString()
		{
			return ToolBox.GetDebugSymbol(this.isFinished, false) + " UnlockPathAction";
		}

		// Token: 0x06002118 RID: 8472 RVA: 0x000DE72C File Offset: 0x000DC92C
		public static void NotifyPathsUnlockedThisRound(Client client)
		{
			foreach (LocationConnection connection in UnlockPathAction.pathsUnlockedThisRound)
			{
				UnlockPathAction.NotifyUnlock(connection, client);
			}
		}

		// Token: 0x06002119 RID: 8473 RVA: 0x000DE780 File Offset: 0x000DC980
		private static void NotifyUnlock(LocationConnection connection)
		{
			foreach (Client client in GameMain.Server.ConnectedClients)
			{
				UnlockPathAction.NotifyUnlock(connection, client);
			}
		}

		// Token: 0x0600211A RID: 8474 RVA: 0x000DE7D4 File Offset: 0x000DC9D4
		private static void NotifyUnlock(LocationConnection connection, Client client)
		{
			IWriteMessage outmsg = new WriteOnlyMessage();
			outmsg.WriteByte(21);
			outmsg.WriteByte(4);
			outmsg.WriteUInt16((ushort)GameMain.GameSession.Map.Connections.IndexOf(connection));
			GameMain.Server.ServerPeer.Send(outmsg, client.Connection, DeliveryMethod.Reliable, true);
		}

		// Token: 0x04000F9A RID: 3994
		private static readonly HashSet<LocationConnection> pathsUnlockedThisRound = new HashSet<LocationConnection>();

		// Token: 0x04000F9B RID: 3995
		private bool isFinished;
	}
}
