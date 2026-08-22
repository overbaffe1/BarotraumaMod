using System;
using Barotrauma.Networking;
using FluentResults;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000514 RID: 1300
	public interface ILoggerService : IReusableService, IService, IDisposable
	{
		// Token: 0x06005408 RID: 21512
		void Subscribe(ILoggerSubscriber subscriber);

		// Token: 0x06005409 RID: 21513
		void Unsubscribe(ILoggerSubscriber subscriber);

		// Token: 0x0600540A RID: 21514
		void ProcessLogs();

		// Token: 0x0600540B RID: 21515
		void HandleException(Exception exception, string prefix = null);

		// Token: 0x0600540C RID: 21516
		void LogError(string message);

		// Token: 0x0600540D RID: 21517
		void LogWarning(string message);

		// Token: 0x0600540E RID: 21518
		void LogMessage(string message, Color? serverColor = null, Color? clientColor = null);

		// Token: 0x0600540F RID: 21519
		void Log(string message, Color? color = null, ServerLog.MessageType messageType = ServerLog.MessageType.ServerMessage);

		// Token: 0x06005410 RID: 21520
		void LogResults(Result result);

		// Token: 0x06005411 RID: 21521
		void LogDebug(string message, Color? color = null);

		// Token: 0x06005412 RID: 21522
		void LogDebugWarning(string message);

		// Token: 0x06005413 RID: 21523
		void LogDebugError(string message);

		// Token: 0x06005414 RID: 21524 RVA: 0x002CE0D3 File Offset: 0x002CC2D3
		void HandleException(Exception ex, LuaCsMessageOrigin origin)
		{
			this.HandleException(ex, origin.ToString());
		}

		// Token: 0x06005415 RID: 21525 RVA: 0x002CE0E9 File Offset: 0x002CC2E9
		void LogError(string message, LuaCsMessageOrigin origin)
		{
			this.LogError(message);
		}
	}
}
