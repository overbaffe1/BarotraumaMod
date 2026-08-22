using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using FluentResults;
using Microsoft.Xna.Framework;
using MoonSharp.Interpreter;

namespace Barotrauma.LuaCs
{
	// Token: 0x020004E4 RID: 1252
	public class LoggerService : ILoggerService, IReusableService, IService, IDisposable, IClientLoggerService
	{
		// Token: 0x06005199 RID: 20889 RVA: 0x002BEFDC File Offset: 0x002BD1DC
		private void CreateOverlay(string message)
		{
			this._overlayFrame = new GUIFrame(new RectTransform(new Vector2(0.4f, 0.03f), null, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, new Color?(new Color(50, 50, 50, 100)))
			{
				CanBeFocused = false
			};
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(new Vector2(0.8f, 0.8f), this._overlayFrame.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), false, Anchor.Center);
			this._textBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), message, null, null, Alignment.Left, false, "", null);
			this._overlayFrame.RectTransform.MinSize = new Point((int)((double)this._textBlock.TextSize.X * 1.2), 0);
			layout.Recalculate();
		}

		// Token: 0x0600519A RID: 20890 RVA: 0x002BF11E File Offset: 0x002BD31E
		public void AddToGUIUpdateList()
		{
			if (this._overlayFrame != null && Timing.TotalTime <= this._showTimer)
			{
				this._overlayFrame.AddToGUIUpdateList(false, 0);
			}
		}

		// Token: 0x0600519B RID: 20891 RVA: 0x002BF144 File Offset: 0x002BD344
		public void ShowErrorOverlay(string message, float time = 5f, float duration = 1.5f)
		{
			if (Timing.TotalTime <= this._showTimer)
			{
				return;
			}
			this.CreateOverlay(message);
			this._overlayFrame.Flash(new Color?(Color.Red), duration, true, false, null);
			this._showTimer = Timing.TotalTime + (double)time;
		}

		// Token: 0x0600519D RID: 20893 RVA: 0x002BF1B3 File Offset: 0x002BD3B3
		public void Subscribe(ILoggerSubscriber subscriber)
		{
			this.logSubscribers.Add(subscriber);
		}

		// Token: 0x0600519E RID: 20894 RVA: 0x002BF1C1 File Offset: 0x002BD3C1
		public void Unsubscribe(ILoggerSubscriber subscriber)
		{
			this.logSubscribers.Remove(subscriber);
		}

		// Token: 0x0600519F RID: 20895 RVA: 0x002BF1D0 File Offset: 0x002BD3D0
		public void ProcessLogs()
		{
			for (;;)
			{
				PendingLog log;
				if (!this.logQueue.TryDequeue(out log))
				{
					break;
				}
				this.logSubscribers.ForEach(delegate(ILoggerSubscriber s)
				{
					s.OnLog(log);
				});
				DebugConsole.NewMessage(log.Message, log.Color, false);
			}
		}

		// Token: 0x060051A0 RID: 20896 RVA: 0x002BF22C File Offset: 0x002BD42C
		public void Log(string message, Color? color = null, ServerLog.MessageType messageType = ServerLog.MessageType.ServerMessage)
		{
			if (LuaCsSetup.Instance.HideUserNamesInLogs && !Environment.UserName.IsNullOrEmpty())
			{
				message = message.Replace(Environment.UserName, "USERNAME");
			}
			message = "[CL] " + message;
			this.logQueue.Enqueue(new PendingLog(message, color, messageType));
		}

		// Token: 0x060051A1 RID: 20897 RVA: 0x002BF283 File Offset: 0x002BD483
		public void LogError(string message)
		{
			this.Log(message ?? "", new Color?(Color.Red), ServerLog.MessageType.Error);
		}

		// Token: 0x060051A2 RID: 20898 RVA: 0x002BF2A1 File Offset: 0x002BD4A1
		public void LogWarning(string message)
		{
			this.Log(message ?? "", new Color?(Color.Yellow), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060051A3 RID: 20899 RVA: 0x002BF2C0 File Offset: 0x002BD4C0
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
			this.Log(message, clientColor, ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060051A4 RID: 20900 RVA: 0x002BF314 File Offset: 0x002BD514
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

		// Token: 0x060051A5 RID: 20901 RVA: 0x002BF428 File Offset: 0x002BD628
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

		// Token: 0x060051A6 RID: 20902 RVA: 0x002BF4C4 File Offset: 0x002BD6C4
		public void LogDebug(string message, Color? color = null)
		{
			this.Log(message, new Color?(color ?? Color.Purple), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060051A7 RID: 20903 RVA: 0x002BF4F7 File Offset: 0x002BD6F7
		public void LogDebugWarning(string message)
		{
			this.Log(message, new Color?(Color.Yellow), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060051A8 RID: 20904 RVA: 0x002BF50B File Offset: 0x002BD70B
		public void LogDebugError(string message)
		{
			this.Log(message, new Color?(Color.Red), ServerLog.MessageType.ServerMessage);
		}

		// Token: 0x060051A9 RID: 20905 RVA: 0x002BF51F File Offset: 0x002BD71F
		public void Dispose()
		{
		}

		// Token: 0x060051AA RID: 20906 RVA: 0x002BF521 File Offset: 0x002BD721
		public Result Reset()
		{
			return Result.Ok();
		}

		// Token: 0x170014CC RID: 5324
		// (get) Token: 0x060051AB RID: 20907 RVA: 0x002BF528 File Offset: 0x002BD728
		public bool IsDisposed { get; }

		// Token: 0x04002B49 RID: 11081
		private GUIFrame _overlayFrame;

		// Token: 0x04002B4A RID: 11082
		private GUITextBlock _textBlock;

		// Token: 0x04002B4B RID: 11083
		private double _showTimer;

		// Token: 0x04002B4C RID: 11084
		private List<ILoggerSubscriber> logSubscribers = new List<ILoggerSubscriber>();

		// Token: 0x04002B4D RID: 11085
		private ConcurrentQueue<PendingLog> logQueue = new ConcurrentQueue<PendingLog>();

		// Token: 0x04002B4E RID: 11086
		private const string TargetPrefix = "[CL]";
	}
}
