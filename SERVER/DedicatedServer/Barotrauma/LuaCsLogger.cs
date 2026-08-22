using System;
using Barotrauma.LuaCs;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000211 RID: 529
	public class LuaCsLogger
	{
		// Token: 0x06002539 RID: 9529 RVA: 0x000F509E File Offset: 0x000F329E
		public static void HandleException(Exception ex, LuaCsMessageOrigin origin)
		{
			LuaCsSetup.Instance.Logger.HandleException(ex, null);
		}

		// Token: 0x0600253A RID: 9530 RVA: 0x000F50B1 File Offset: 0x000F32B1
		public static void LogError(string message, LuaCsMessageOrigin origin)
		{
			LuaCsSetup.Instance.Logger.LogError(message);
		}

		// Token: 0x0600253B RID: 9531 RVA: 0x000F50C3 File Offset: 0x000F32C3
		public static void LogError(string message)
		{
			LuaCsSetup.Instance.Logger.LogError(message);
		}

		// Token: 0x0600253C RID: 9532 RVA: 0x000F50D5 File Offset: 0x000F32D5
		public static void LogMessage(string message, Color? serverColor = null, Color? clientColor = null)
		{
			LuaCsSetup.Instance.Logger.LogMessage(message, serverColor, clientColor);
		}

		// Token: 0x0600253D RID: 9533 RVA: 0x000F50E9 File Offset: 0x000F32E9
		public static void Log(string message, Color? color = null, ServerLog.MessageType messageType = ServerLog.MessageType.ServerMessage)
		{
			LuaCsSetup.Instance.Logger.Log(message, color, messageType);
		}
	}
}
