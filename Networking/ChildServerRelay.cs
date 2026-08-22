using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Barotrauma.Networking
{
	// Token: 0x02000459 RID: 1113
	internal static class ChildServerRelay
	{
		// Token: 0x17001315 RID: 4885
		// (get) Token: 0x06004AAD RID: 19117 RVA: 0x002910E4 File Offset: 0x0028F2E4
		public static bool IsProcessAlive
		{
			get
			{
				Process process = ChildServerRelay.Process;
				return process != null && !process.HasExited;
			}
		}

		// Token: 0x06004AAE RID: 19118 RVA: 0x00291108 File Offset: 0x0028F308
		public static void Start(ProcessStartInfo processInfo)
		{
			ChildServerRelay.CrashString = null;
			ChildServerRelay.CrashReportFilePath = null;
			ChildServerRelay.writePipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
			ChildServerRelay.readPipe = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);
			ChildServerRelay.writeStream = ChildServerRelay.writePipe;
			ChildServerRelay.readStream = ChildServerRelay.readPipe;
			ChildServerRelay.PrivateStart();
			processInfo.ArgumentList.Add("-pipes");
			processInfo.ArgumentList.Add(ChildServerRelay.writePipe.GetClientHandleAsString());
			processInfo.ArgumentList.Add(ChildServerRelay.readPipe.GetClientHandleAsString());
			try
			{
				ChildServerRelay.Process = Process.Start(processInfo);
			}
			catch
			{
				DebugConsole.ThrowError("Failed to start ChildServerRelay Process. File: " + processInfo.FileName + ", arguments: " + processInfo.Arguments, null, null, false, false);
				ChildServerRelay.ForceShutDown();
				throw;
			}
			ChildServerRelay.localHandlesDisposed = false;
		}

		// Token: 0x06004AAF RID: 19119 RVA: 0x002911DC File Offset: 0x0028F3DC
		public static void DisposeLocalHandles()
		{
			if (ChildServerRelay.localHandlesDisposed)
			{
				return;
			}
			ChildServerRelay.writePipe.DisposeLocalCopyOfClientHandle();
			ChildServerRelay.readPipe.DisposeLocalCopyOfClientHandle();
			ChildServerRelay.localHandlesDisposed = true;
		}

		// Token: 0x06004AB0 RID: 19120 RVA: 0x00291200 File Offset: 0x0028F400
		public static void AttemptGracefulShutDown(int maxAttempts = 20)
		{
			ChildServerRelay.status = ChildServerRelay.StatusEnum.RequestedShutDown;
			ManualResetEvent manualResetEvent = ChildServerRelay.writeManualResetEvent;
			if (manualResetEvent != null)
			{
				manualResetEvent.Set();
			}
			int checks = 0;
			for (;;)
			{
				Process process = ChildServerRelay.Process;
				if (process == null || process.HasExited)
				{
					goto IL_4A;
				}
				if (checks >= maxAttempts)
				{
					break;
				}
				Thread.Sleep(100);
				checks++;
			}
			DebugConsole.AddWarning("Server could not be shut down gracefully", null);
			IL_4A:
			ChildServerRelay.ForceShutDown();
		}

		// Token: 0x06004AB1 RID: 19121 RVA: 0x0029125C File Offset: 0x0028F45C
		public static void ForceShutDown()
		{
			Process process = ChildServerRelay.Process;
			if (process != null)
			{
				process.Kill();
			}
			ChildServerRelay.Process = null;
			ChildServerRelay.PrivateShutDown();
		}

		// Token: 0x17001316 RID: 4886
		// (get) Token: 0x06004AB2 RID: 19122 RVA: 0x00291279 File Offset: 0x0028F479
		// (set) Token: 0x06004AB3 RID: 19123 RVA: 0x00291280 File Offset: 0x0028F480
		public static string CrashString { get; private set; }

		// Token: 0x17001317 RID: 4887
		// (get) Token: 0x06004AB4 RID: 19124 RVA: 0x00291288 File Offset: 0x0028F488
		// (set) Token: 0x06004AB5 RID: 19125 RVA: 0x0029128F File Offset: 0x0028F48F
		public static string CrashReportFilePath { get; private set; }

		// Token: 0x17001318 RID: 4888
		// (get) Token: 0x06004AB6 RID: 19126 RVA: 0x00291297 File Offset: 0x0028F497
		public static LocalizedString CrashMessage
		{
			get
			{
				if (!string.IsNullOrEmpty(ChildServerRelay.CrashReportFilePath))
				{
					return TextManager.GetWithVariable("ServerProcessCrashed", "[reportfilepath]", ChildServerRelay.CrashReportFilePath, FormatCapitals.No);
				}
				return TextManager.Get("ServerProcessClosed");
			}
		}

		// Token: 0x17001319 RID: 4889
		// (get) Token: 0x06004AB7 RID: 19127 RVA: 0x002912CA File Offset: 0x0028F4CA
		public static bool HasShutDown
		{
			get
			{
				return ChildServerRelay.status == ChildServerRelay.StatusEnum.ShutDown;
			}
		}

		// Token: 0x06004AB8 RID: 19128 RVA: 0x002912D8 File Offset: 0x0028F4D8
		private static void PrivateStart()
		{
			ChildServerRelay.status = ChildServerRelay.StatusEnum.Active;
			ChildServerRelay.readIncOffset = 0;
			ChildServerRelay.readIncTotal = 0;
			ChildServerRelay.readTempBytes = new byte[2340];
			ChildServerRelay.msgsToWrite = new ConcurrentQueue<byte[]>();
			ChildServerRelay.errorsToWrite = new ConcurrentQueue<string>();
			ChildServerRelay.msgsToRead = new ConcurrentQueue<byte[]>();
			ChildServerRelay.readCancellationToken = new CancellationTokenSource();
			ChildServerRelay.writeManualResetEvent = new ManualResetEvent(false);
			ThreadStart start;
			if ((start = ChildServerRelay.<>O.<0>__UpdateRead) == null)
			{
				start = (ChildServerRelay.<>O.<0>__UpdateRead = new ThreadStart(ChildServerRelay.UpdateRead));
			}
			ChildServerRelay.readThread = new Thread(start)
			{
				Name = "ChildServerRelay.ReadThread",
				IsBackground = true
			};
			ThreadStart start2;
			if ((start2 = ChildServerRelay.<>O.<1>__UpdateWrite) == null)
			{
				start2 = (ChildServerRelay.<>O.<1>__UpdateWrite = new ThreadStart(ChildServerRelay.UpdateWrite));
			}
			ChildServerRelay.writeThread = new Thread(start2)
			{
				Name = "ChildServerRelay.WriteThread",
				IsBackground = true
			};
			ChildServerRelay.readThread.Start();
			ChildServerRelay.writeThread.Start();
		}

		// Token: 0x06004AB9 RID: 19129 RVA: 0x002913C0 File Offset: 0x0028F5C0
		private static void PrivateShutDown()
		{
			if (Thread.CurrentThread != GameMain.MainThread)
			{
				throw new InvalidOperationException("Cannot call ChildServerRelay.PrivateShutDown from a thread other than the main one");
			}
			if (ChildServerRelay.status == ChildServerRelay.StatusEnum.NeverStarted)
			{
				return;
			}
			ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
			ManualResetEvent manualResetEvent = ChildServerRelay.writeManualResetEvent;
			if (manualResetEvent != null)
			{
				manualResetEvent.Set();
			}
			CancellationTokenSource cancellationTokenSource = ChildServerRelay.readCancellationToken;
			if (cancellationTokenSource != null)
			{
				cancellationTokenSource.Cancel();
			}
			Thread thread = ChildServerRelay.readThread;
			if (thread != null)
			{
				thread.Join();
			}
			ChildServerRelay.readThread = null;
			Thread thread2 = ChildServerRelay.writeThread;
			if (thread2 != null)
			{
				thread2.Join();
			}
			ChildServerRelay.writeThread = null;
			CancellationTokenSource cancellationTokenSource2 = ChildServerRelay.readCancellationToken;
			if (cancellationTokenSource2 != null)
			{
				cancellationTokenSource2.Dispose();
			}
			ChildServerRelay.readCancellationToken = null;
			AnonymousPipeServerStream anonymousPipeServerStream = ChildServerRelay.readStream;
			if (anonymousPipeServerStream != null)
			{
				anonymousPipeServerStream.Dispose();
			}
			ChildServerRelay.readStream = null;
			AnonymousPipeServerStream anonymousPipeServerStream2 = ChildServerRelay.writeStream;
			if (anonymousPipeServerStream2 != null)
			{
				anonymousPipeServerStream2.Dispose();
			}
			ChildServerRelay.writeStream = null;
			ConcurrentQueue<byte[]> concurrentQueue = ChildServerRelay.msgsToRead;
			if (concurrentQueue != null)
			{
				concurrentQueue.Clear();
			}
			ConcurrentQueue<byte[]> concurrentQueue2 = ChildServerRelay.msgsToWrite;
			if (concurrentQueue2 == null)
			{
				return;
			}
			concurrentQueue2.Clear();
		}

		// Token: 0x06004ABA RID: 19130 RVA: 0x002914A4 File Offset: 0x0028F6A4
		private static Option<int> ReadIncomingMsgs()
		{
			AnonymousPipeServerStream anonymousPipeServerStream = ChildServerRelay.readStream;
			Task<int> readTask = (anonymousPipeServerStream != null) ? anonymousPipeServerStream.ReadAsync(ChildServerRelay.readTempBytes, 0, ChildServerRelay.readTempBytes.Length, ChildServerRelay.readCancellationToken.Token) : null;
			if (readTask == null)
			{
				return Option<int>.None();
			}
			int timeOutMilliseconds = 150;
			for (int i = 0; i < 150; i++)
			{
				if (ChildServerRelay.status == ChildServerRelay.StatusEnum.ShutDown)
				{
					CancellationTokenSource cancellationTokenSource = ChildServerRelay.readCancellationToken;
					if (cancellationTokenSource != null)
					{
						cancellationTokenSource.Cancel();
					}
					return Option<int>.None();
				}
				try
				{
					if (readTask.IsCompleted || readTask.Wait(timeOutMilliseconds, ChildServerRelay.readCancellationToken.Token))
					{
						break;
					}
				}
				catch (AggregateException aggregateException)
				{
					if (aggregateException.InnerException is OperationCanceledException)
					{
						return Option<int>.None();
					}
					ChildServerRelay.CheckPipeConnected("readStream", ChildServerRelay.readStream);
					throw;
				}
				catch (OperationCanceledException)
				{
					return Option<int>.None();
				}
			}
			if (readTask.Status == TaskStatus.RanToCompletion)
			{
				return Option<int>.Some(readTask.Result);
			}
			bool flag = ChildServerRelay.status != ChildServerRelay.StatusEnum.Active;
			bool flag2 = flag;
			if (flag2)
			{
				AggregateException exception = readTask.Exception;
				Exception ex = (exception != null) ? exception.InnerException : null;
				bool flag3 = ex is ObjectDisposedException || ex is IOException;
				flag2 = flag3;
			}
			bool swallowException = flag2;
			if (swallowException)
			{
				CancellationTokenSource cancellationTokenSource2 = ChildServerRelay.readCancellationToken;
				if (cancellationTokenSource2 != null)
				{
					cancellationTokenSource2.Cancel();
				}
				return Option<int>.None();
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 1);
			defaultInterpolatedStringHandler.AppendLiteral("ChildServerRelay readTask did not run to completion: status was ");
			defaultInterpolatedStringHandler.AppendFormatted<TaskStatus>(readTask.Status);
			defaultInterpolatedStringHandler.AppendLiteral(".");
			throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear(), readTask.Exception);
		}

		// Token: 0x06004ABB RID: 19131 RVA: 0x00291650 File Offset: 0x0028F850
		private static void CheckPipeConnected(string name, AnonymousPipeServerStream pipe)
		{
			if (ChildServerRelay.status == ChildServerRelay.StatusEnum.Active && (pipe == null || !pipe.IsConnected))
			{
				string exceptionMsg = name + " was disconnected unexpectedly.";
				Process process = ChildServerRelay.Process;
				if (process != null && process.HasExited)
				{
					int exitCode = process.ExitCode;
					string str = exceptionMsg;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 1);
					defaultInterpolatedStringHandler.AppendLiteral(" Child process exit code was ");
					defaultInterpolatedStringHandler.AppendFormatted<uint>((uint)exitCode, "X8");
					defaultInterpolatedStringHandler.AppendLiteral(".");
					exceptionMsg = str + defaultInterpolatedStringHandler.ToStringAndClear();
				}
				else
				{
					process = ChildServerRelay.Process;
					if (process != null && !process.HasExited)
					{
						exceptionMsg += " Child process has not exited.";
					}
				}
				throw new Exception(exceptionMsg);
			}
		}

		// Token: 0x06004ABC RID: 19132 RVA: 0x002916FF File Offset: 0x0028F8FF
		private static void HandleCrashString(string str)
		{
			DebugConsole.ThrowError("The server has crashed: " + str, null, null, false, false);
			ChildServerRelay.CrashReportFilePath = (str.Split("||", StringSplitOptions.None).FirstOrDefault<string>() ?? "servercrashreport.log");
			ChildServerRelay.CrashString = str;
		}

		// Token: 0x06004ABD RID: 19133 RVA: 0x0029173C File Offset: 0x0028F93C
		private unsafe static void UpdateRead()
		{
			Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)5], 5);
			Span<byte> msgLengthSpan = span;
			while (!ChildServerRelay.HasShutDown)
			{
				ChildServerRelay.CheckPipeConnected("readStream", ChildServerRelay.readStream);
				if (!ChildServerRelay.<UpdateRead>g__readBytes|43_0(msgLengthSpan))
				{
					ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
					return;
				}
				int msgLength = (int)(*msgLengthSpan[0]) | (int)(*msgLengthSpan[1]) << 8 | (int)(*msgLengthSpan[2]) << 16 | (int)(*msgLengthSpan[3]) << 24;
				ChildServerRelay.WriteStatus writeStatus = (ChildServerRelay.WriteStatus)(*msgLengthSpan[4]);
				byte[] msg = (msgLength > 0) ? new byte[msgLength] : Array.Empty<byte>();
				if (msg.Length != 0 && !ChildServerRelay.<UpdateRead>g__readBytes|43_0(msg.AsSpan<byte>()))
				{
					ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
					return;
				}
				if (writeStatus <= ChildServerRelay.WriteStatus.Heartbeat)
				{
					if (writeStatus != ChildServerRelay.WriteStatus.Success)
					{
						if (writeStatus != ChildServerRelay.WriteStatus.Heartbeat)
						{
						}
					}
					else
					{
						ChildServerRelay.msgsToRead.Enqueue(msg);
					}
				}
				else if (writeStatus != ChildServerRelay.WriteStatus.RequestShutdown)
				{
					if (writeStatus == ChildServerRelay.WriteStatus.Crash)
					{
						ChildServerRelay.HandleCrashString(Encoding.UTF8.GetString(msg));
						ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
					}
				}
				else
				{
					ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
				}
				Thread.Yield();
			}
		}

		// Token: 0x06004ABE RID: 19134 RVA: 0x00291844 File Offset: 0x0028FA44
		private static void UpdateWrite()
		{
			while (!ChildServerRelay.HasShutDown)
			{
				ChildServerRelay.CheckPipeConnected("writeStream", ChildServerRelay.writeStream);
				if (ChildServerRelay.status == ChildServerRelay.StatusEnum.RequestedShutDown)
				{
					ChildServerRelay.<UpdateWrite>g__writeMsg|44_0(ChildServerRelay.WriteStatus.RequestShutdown, Array.Empty<byte>());
					ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
				}
				string error;
				while (ChildServerRelay.errorsToWrite.TryDequeue(out error))
				{
					ChildServerRelay.<UpdateWrite>g__writeMsg|44_0(ChildServerRelay.WriteStatus.Crash, Encoding.UTF8.GetBytes(error));
					ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
				}
				byte[] msg;
				while (ChildServerRelay.msgsToWrite.TryDequeue(out msg))
				{
					ChildServerRelay.<UpdateWrite>g__writeMsg|44_0(ChildServerRelay.WriteStatus.Success, msg);
					if (ChildServerRelay.HasShutDown)
					{
						break;
					}
				}
				if (!ChildServerRelay.HasShutDown)
				{
					ChildServerRelay.writeManualResetEvent.Reset();
					if (!ChildServerRelay.writeManualResetEvent.WaitOne(1000))
					{
						if (ChildServerRelay.HasShutDown)
						{
							return;
						}
						ChildServerRelay.<UpdateWrite>g__writeMsg|44_0(ChildServerRelay.WriteStatus.Heartbeat, Array.Empty<byte>());
					}
				}
			}
		}

		// Token: 0x06004ABF RID: 19135 RVA: 0x0029190F File Offset: 0x0028FB0F
		public static void Write(byte[] msg)
		{
			if (ChildServerRelay.HasShutDown)
			{
				return;
			}
			if (msg.Length > 536870911)
			{
				return;
			}
			ChildServerRelay.msgsToWrite.Enqueue(msg);
			ChildServerRelay.writeManualResetEvent.Set();
		}

		// Token: 0x06004AC0 RID: 19136 RVA: 0x0029193A File Offset: 0x0028FB3A
		public static IEnumerable<byte[]> Read()
		{
			return new ChildServerRelay.<Read>d__48(-2);
		}

		// Token: 0x06004AC1 RID: 19137 RVA: 0x00291943 File Offset: 0x0028FB43
		private static bool ReadSingleMessage(out byte[] msg)
		{
			if (ChildServerRelay.HasShutDown)
			{
				msg = null;
				return false;
			}
			return ChildServerRelay.msgsToRead.TryDequeue(out msg);
		}

		// Token: 0x06004AC3 RID: 19139 RVA: 0x00291970 File Offset: 0x0028FB70
		[CompilerGenerated]
		internal unsafe static bool <UpdateRead>g__readBytes|43_0(Span<byte> readTo)
		{
			int i = 0;
			while (i < readTo.Length)
			{
				if (ChildServerRelay.readIncOffset < ChildServerRelay.readIncTotal)
				{
					goto IL_3B;
				}
				if (!ChildServerRelay.ReadIncomingMsgs().TryUnwrap(out ChildServerRelay.readIncTotal))
				{
					return false;
				}
				ChildServerRelay.readIncOffset = 0;
				if (ChildServerRelay.readIncTotal != 0)
				{
					goto IL_3B;
				}
				Thread.Yield();
				IL_5B:
				i++;
				continue;
				IL_3B:
				*readTo[i] = ChildServerRelay.readTempBytes[ChildServerRelay.readIncOffset];
				ChildServerRelay.readIncOffset++;
				goto IL_5B;
			}
			return true;
		}

		// Token: 0x06004AC4 RID: 19140 RVA: 0x002919E8 File Offset: 0x0028FBE8
		[CompilerGenerated]
		internal unsafe static void <UpdateWrite>g__writeMsg|44_0(ChildServerRelay.WriteStatus writeStatus, byte[] msg)
		{
			Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)5], 5);
			Span<byte> headerBytes = span;
			*headerBytes[0] = (byte)(msg.Length & 255);
			*headerBytes[1] = (byte)(msg.Length >> 8 & 255);
			*headerBytes[2] = (byte)(msg.Length >> 16 & 255);
			*headerBytes[3] = (byte)(msg.Length >> 24 & 255);
			*headerBytes[4] = (byte)writeStatus;
			try
			{
				AnonymousPipeServerStream anonymousPipeServerStream = ChildServerRelay.writeStream;
				if (anonymousPipeServerStream != null)
				{
					anonymousPipeServerStream.Write(headerBytes);
				}
				AnonymousPipeServerStream anonymousPipeServerStream2 = ChildServerRelay.writeStream;
				if (anonymousPipeServerStream2 != null)
				{
					anonymousPipeServerStream2.Write(msg);
				}
			}
			catch (Exception exception)
			{
				if (!(exception is ObjectDisposedException) && !(exception is IOException))
				{
					throw;
				}
				if (!ChildServerRelay.HasShutDown)
				{
					ChildServerRelay.CheckPipeConnected("writeStream", ChildServerRelay.writeStream);
					throw;
				}
			}
		}

		// Token: 0x04002712 RID: 10002
		public static Process Process;

		// Token: 0x04002713 RID: 10003
		private static bool localHandlesDisposed;

		// Token: 0x04002714 RID: 10004
		private static AnonymousPipeServerStream writePipe;

		// Token: 0x04002715 RID: 10005
		private static AnonymousPipeServerStream readPipe;

		// Token: 0x04002718 RID: 10008
		private static AnonymousPipeServerStream writeStream;

		// Token: 0x04002719 RID: 10009
		private static AnonymousPipeServerStream readStream;

		// Token: 0x0400271A RID: 10010
		private static ManualResetEvent writeManualResetEvent;

		// Token: 0x0400271B RID: 10011
		private static volatile ChildServerRelay.StatusEnum status = ChildServerRelay.StatusEnum.NeverStarted;

		// Token: 0x0400271C RID: 10012
		private const int ReadBufferSize = 2340;

		// Token: 0x0400271D RID: 10013
		private static byte[] readTempBytes;

		// Token: 0x0400271E RID: 10014
		private static int readIncOffset;

		// Token: 0x0400271F RID: 10015
		private static int readIncTotal;

		// Token: 0x04002720 RID: 10016
		private static ConcurrentQueue<byte[]> msgsToWrite;

		// Token: 0x04002721 RID: 10017
		private static ConcurrentQueue<string> errorsToWrite;

		// Token: 0x04002722 RID: 10018
		private static ConcurrentQueue<byte[]> msgsToRead;

		// Token: 0x04002723 RID: 10019
		private static Thread readThread;

		// Token: 0x04002724 RID: 10020
		private static Thread writeThread;

		// Token: 0x04002725 RID: 10021
		private static CancellationTokenSource readCancellationToken;

		// Token: 0x04002726 RID: 10022
		private static readonly Stopwatch stopwatch = new Stopwatch();

		// Token: 0x04002727 RID: 10023
		private const int MaxMilliseconds = 8;

		// Token: 0x020011BB RID: 4539
		private enum WriteStatus : byte
		{
			// Token: 0x04005CCF RID: 23759
			Success,
			// Token: 0x04005CD0 RID: 23760
			Heartbeat,
			// Token: 0x04005CD1 RID: 23761
			RequestShutdown = 204,
			// Token: 0x04005CD2 RID: 23762
			Crash = 255
		}

		// Token: 0x020011BC RID: 4540
		private enum StatusEnum
		{
			// Token: 0x04005CD4 RID: 23764
			NeverStarted,
			// Token: 0x04005CD5 RID: 23765
			Active,
			// Token: 0x04005CD6 RID: 23766
			RequestedShutDown,
			// Token: 0x04005CD7 RID: 23767
			ShutDown
		}

		// Token: 0x020011BD RID: 4541
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005CD8 RID: 23768
			public static ThreadStart <0>__UpdateRead;

			// Token: 0x04005CD9 RID: 23769
			public static ThreadStart <1>__UpdateWrite;
		}
	}
}
