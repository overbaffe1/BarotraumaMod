using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.LuaCs.Events;
using Barotrauma.Networking;
using FluentResults;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003EB RID: 1003
	public class LoggerService : ILoggerService, IReusableService, IService, IDisposable
	{
		// Token: 0x060039B3 RID: 14771 RVA: 0x001810BE File Offset: 0x0017F2BE
		public void Subscribe(ILoggerSubscriber subscriber)
		{
			this.logSubscribers.Add(subscriber);
		}

		// Token: 0x060039B4 RID: 14772 RVA: 0x001810CC File Offset: 0x0017F2CC
		public void Unsubscribe(ILoggerSubscriber subscriber)
		{
			this.logSubscribers.Remove(subscriber);
		}

		// Token: 0x060039B5 RID: 14773 RVA: 0x001810DC File Offset: 0x0017F2DC
		public void ProcessLogs()
		{
			for (;;)
			{
				LoggerService.<>c__DisplayClass9_0 CS$<>8__locals1 = new LoggerService.<>c__DisplayClass9_0();
				if (!this.logQueue.TryDequeue(out CS$<>8__locals1.log))
				{
					break;
				}
				this.logSubscribers.ForEach(delegate(ILoggerSubscriber s)
				{
					s.OnLog(CS$<>8__locals1.log);
				});
				DebugConsole.NewMessage(CS$<>8__locals1.log.Message, CS$<>8__locals1.log.Color, false);
				if (GameMain.Server != null)
				{
					if (GameMain.Server.ServerSettings.SaveServerLogs)
					{
						string logMessage = "[LuaCs] " + CS$<>8__locals1.log.Message;
						GameMain.Server.ServerSettings.ServerLog.WriteLine(logMessage, CS$<>8__locals1.log.MessageType, false);
						if (!this._isInsideLogCall)
						{
							this._isInsideLogCall = true;
							LuaCsSetup instance = LuaCsSetup.Instance;
							if (instance != null)
							{
								instance.EventService.PublishEvent<IEventServerLog>(delegate(IEventServerLog x)
								{
									x.OnServerLog(logMessage, CS$<>8__locals1.log.MessageType);
								});
							}
							this._isInsideLogCall = false;
						}
					}
					for (int i = 0; i < CS$<>8__locals1.log.Message.Length; i += 1024)
					{
						string subStr = CS$<>8__locals1.log.Message.Substring(i, Math.Min(1024, CS$<>8__locals1.log.Message.Length - i));
						CS$<>8__locals1.<ProcessLogs>g__BroadcastMessage|1(subStr);
					}
				}
			}
		}

		// Token: 0x060039B6 RID: 14774 RVA: 0x0018123C File Offset: 0x0017F43C
		public void Log(string message, Color? color = null, ServerLog.MessageType messageType = ServerLog.MessageType.ServerMessage)
		{
			if (LuaCsSetup.Instance.HideUserNamesInLogs && !Environment.UserName.IsNullOrEmpty())
			{
				message = message.Replace(Environment.UserName, "USERNAME");
			}
			message = "[SV] " + message;
			this.logQueue.Enqueue(new PendingLog(message, color, messageType));
		}

		// Token: 0x060039B7 RID: 14775 RVA: 0x00181293 File Offset: 0x0017F493
		public void LogError(string message)
		{
			this.Log(message ?? "", new Color?(Color.Red), ServerLog.MessageType.Error);
		}

		// Token: 0x060039B8 RID: 14776 RVA: 0x001812B1 File Offset: 0x0017F4B1
		public void LogWarning(string message)
		{
			this.Log(message ?? "", new Color?(Color.Yellow), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060039B9 RID: 14777 RVA: 0x001812D0 File Offset: 0x0017F4D0
		public void LogMessage(string message, Color? serverColor = null, Color? clientColor = null)
		{
			Color value = serverColor.GetValueOrDefault();
			if (serverColor == null)
			{
				value = Color.MediumPurple;
				serverColor = new Color?(value);
			}
			value = clientColor.GetValueOrDefault();
			if (clientColor == null)
			{
				value = Color.Purple;
				clientColor = new Color?(value);
			}
			this.Log(message, serverColor, ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060039BA RID: 14778 RVA: 0x00181324 File Offset: 0x0017F524
		public void HandleException(Exception exception, string prefix = null)
		{
			NetRuntimeException netRuntimeException = exception as NetRuntimeException;
			string errorString;
			if (netRuntimeException == null)
			{
				InterpreterException interpreterException = exception as InterpreterException;
				if (interpreterException == null)
				{
					string text;
					if (exception.StackTrace == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 2);
						defaultInterpolatedStringHandler.AppendFormatted<Exception>(exception);
						defaultInterpolatedStringHandler.AppendLiteral("\n");
						defaultInterpolatedStringHandler.AppendFormatted(Environment.StackTrace);
						text = defaultInterpolatedStringHandler.ToStringAndClear();
					}
					else
					{
						text = exception.ToString();
					}
					string s = text;
					errorString = prefix + s;
				}
				else if (interpreterException.DecoratedMessage == null)
				{
					errorString = prefix + interpreterException.ToString();
				}
				else
				{
					errorString = prefix + interpreterException.DecoratedMessage;
				}
			}
			else if (netRuntimeException.DecoratedMessage == null)
			{
				errorString = prefix + netRuntimeException.ToString();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(2, 3);
				defaultInterpolatedStringHandler2.AppendFormatted(prefix ?? "");
				defaultInterpolatedStringHandler2.AppendFormatted(netRuntimeException.DecoratedMessage);
				defaultInterpolatedStringHandler2.AppendLiteral(": ");
				defaultInterpolatedStringHandler2.AppendFormatted<NetRuntimeException>(netRuntimeException);
				errorString = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			this.LogError(prefix + Environment.UserName + " " + errorString);
		}

		// Token: 0x060039BB RID: 14779 RVA: 0x00181438 File Offset: 0x0017F638
		public void LogResults(Result result)
		{
			if (result == null)
			{
				this.LogError("Result is null");
				return;
			}
			if (result.IsSuccess)
			{
				return;
			}
			if (result.IsFailed)
			{
				foreach (IError error in result.Errors)
				{
					ExceptionalError exceptionalError = error as ExceptionalError;
					if (exceptionalError != null)
					{
						this.HandleException(exceptionalError.Exception, null);
					}
					else
					{
						this.LogError("FluentResults::IError: " + error.Message);
					}
				}
			}
		}

		// Token: 0x060039BC RID: 14780 RVA: 0x001814D4 File Offset: 0x0017F6D4
		public void LogDebug(string message, Color? color = null)
		{
			this.Log(message, new Color?(color ?? Color.Purple), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060039BD RID: 14781 RVA: 0x00181507 File Offset: 0x0017F707
		public void LogDebugWarning(string message)
		{
			this.Log(message, new Color?(Color.Yellow), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060039BE RID: 14782 RVA: 0x0018151B File Offset: 0x0017F71B
		public void LogDebugError(string message)
		{
			this.Log(message, new Color?(Color.Red), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060039BF RID: 14783 RVA: 0x0018152F File Offset: 0x0017F72F
		public void Dispose()
		{
		}

		// Token: 0x060039C0 RID: 14784 RVA: 0x00181531 File Offset: 0x0017F731
		public Result Reset()
		{
			return Result.Ok();
		}

		// Token: 0x17000FA8 RID: 4008
		// (get) Token: 0x060039C1 RID: 14785 RVA: 0x00181538 File Offset: 0x0017F738
		public bool IsDisposed { get; }

		// Token: 0x04001CEA RID: 7402
		private List<ILoggerSubscriber> logSubscribers = new List<ILoggerSubscriber>();

		// Token: 0x04001CEB RID: 7403
		private ConcurrentQueue<PendingLog> logQueue = new ConcurrentQueue<PendingLog>();

		// Token: 0x04001CEC RID: 7404
		private const string TargetPrefix = "[SV]";

		// Token: 0x04001CED RID: 7405
		private const int NetMaxLength = 1024;

		// Token: 0x04001CEE RID: 7406
		private const int NetMaxMessages = 60;

		// Token: 0x04001CEF RID: 7407
		private bool _isInsideLogCall;
	}
}
