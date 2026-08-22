using System;
using Barotrauma.Networking;
using FluentResults;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000401 RID: 1025
	public interface ILoggerService : IReusableService, IService, IDisposable
	{
		// Token: 0x06003AEC RID: 15084
		void Subscribe(ILoggerSubscriber subscriber);

		// Token: 0x06003AED RID: 15085
		void Unsubscribe(ILoggerSubscriber subscriber);

		// Token: 0x06003AEE RID: 15086
		void ProcessLogs();

		// Token: 0x06003AEF RID: 15087
		void HandleException(Exception exception, string prefix = null);

		// Token: 0x06003AF0 RID: 15088
		void LogError(string message);

		// Token: 0x06003AF1 RID: 15089
		void LogWarning(string message);

		// Token: 0x06003AF2 RID: 15090
		void LogMessage(string message, Color? serverColor = null, Color? clientColor = null);

		// Token: 0x06003AF3 RID: 15091
		void Log(string message, Color? color = null, ServerLog.MessageType messageType = ServerLog.MessageType.ServerMessage);

		// Token: 0x06003AF4 RID: 15092
		void LogResults(Result result);

		// Token: 0x06003AF5 RID: 15093
		void LogDebug(string message, Color? color = null);

		// Token: 0x06003AF6 RID: 15094
		void LogDebugWarning(string message);

		// Token: 0x06003AF7 RID: 15095
		void LogDebugError(string message);

		// Token: 0x06003AF8 RID: 15096 RVA: 0x0018993B File Offset: 0x00187B3B
		void HandleException(Exception ex, LuaCsMessageOrigin origin)
		{
			this.HandleException(ex, origin.ToString());
		}

		// Token: 0x06003AF9 RID: 15097 RVA: 0x00189951 File Offset: 0x00187B51
		void LogError(string message, LuaCsMessageOrigin origin)
		{
			this.LogError(message);
		}
	}
}
