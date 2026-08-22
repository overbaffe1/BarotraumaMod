using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Barotrauma.Networking
{
	// Token: 0x02000368 RID: 872
	internal static class ChildServerRelay
	{
		// Token: 0x06003384 RID: 13188 RVA: 0x0015C3E4 File Offset: 0x0015A5E4
		public static void Start(string writeHandle, string readHandle)
		{
			AnonymousPipeClientStream writePipe = new AnonymousPipeClientStream(PipeDirection.Out, writeHandle);
			AnonymousPipeClientStream readPipe = new AnonymousPipeClientStream(PipeDirection.In, readHandle);
			ChildServerRelay.writeStream = writePipe;
			ChildServerRelay.readStream = readPipe;
			ChildServerRelay.PrivateStart();
		}

		// Token: 0x06003385 RID: 13189 RVA: 0x0015C412 File Offset: 0x0015A612
		public static void NotifyCrash(string msg)
		{
			ChildServerRelay.errorsToWrite.Enqueue(msg);
			Thread.Sleep(1000);
		}

		// Token: 0x06003386 RID: 13190 RVA: 0x0015C429 File Offset: 0x0015A629
		public static void ShutDown()
		{
			ChildServerRelay.PrivateShutDown();
		}

		// Token: 0x17000E57 RID: 3671
		// (get) Token: 0x06003387 RID: 13191 RVA: 0x0015C430 File Offset: 0x0015A630
		public static bool HasShutDown
		{
			get
			{
				return ChildServerRelay.status == ChildServerRelay.StatusEnum.ShutDown;
			}
		}

		// Token: 0x06003388 RID: 13192 RVA: 0x0015C43C File Offset: 0x0015A63C
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

		// Token: 0x06003389 RID: 13193 RVA: 0x0015C524 File Offset: 0x0015A724
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
			AnonymousPipeClientStream anonymousPipeClientStream = ChildServerRelay.readStream;
			if (anonymousPipeClientStream != null)
			{
				anonymousPipeClientStream.Dispose();
			}
			ChildServerRelay.readStream = null;
			AnonymousPipeClientStream anonymousPipeClientStream2 = ChildServerRelay.writeStream;
			if (anonymousPipeClientStream2 != null)
			{
				anonymousPipeClientStream2.Dispose();
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

		// Token: 0x0600338A RID: 13194 RVA: 0x0015C608 File Offset: 0x0015A808
		private static Option<int> ReadIncomingMsgs()
		{
			AnonymousPipeClientStream anonymousPipeClientStream = ChildServerRelay.readStream;
			Task<int> readTask = (anonymousPipeClientStream != null) ? anonymousPipeClientStream.ReadAsync(ChildServerRelay.readTempBytes, 0, ChildServerRelay.readTempBytes.Length, ChildServerRelay.readCancellationToken.Token) : null;
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

		// Token: 0x0600338B RID: 13195 RVA: 0x0015C7B4 File Offset: 0x0015A9B4
		private static void CheckPipeConnected(string name, AnonymousPipeClientStream pipe)
		{
			if (ChildServerRelay.status == ChildServerRelay.StatusEnum.Active && (pipe == null || !pipe.IsConnected))
			{
				string exceptionMsg = name + " was disconnected unexpectedly.";
				throw new Exception(exceptionMsg);
			}
		}

		// Token: 0x0600338C RID: 13196 RVA: 0x0015C7EC File Offset: 0x0015A9EC
		private unsafe static void UpdateRead()
		{
			Span<byte> span = new Span<byte>(stackalloc byte[(UIntPtr)5], 5);
			Span<byte> msgLengthSpan = span;
			while (!ChildServerRelay.HasShutDown)
			{
				ChildServerRelay.CheckPipeConnected("readStream", ChildServerRelay.readStream);
				if (!ChildServerRelay.<UpdateRead>g__readBytes|26_0(msgLengthSpan))
				{
					ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
					return;
				}
				int msgLength = (int)(*msgLengthSpan[0]) | (int)(*msgLengthSpan[1]) << 8 | (int)(*msgLengthSpan[2]) << 16 | (int)(*msgLengthSpan[3]) << 24;
				ChildServerRelay.WriteStatus writeStatus = (ChildServerRelay.WriteStatus)(*msgLengthSpan[4]);
				byte[] msg = (msgLength > 0) ? new byte[msgLength] : Array.Empty<byte>();
				if (msg.Length != 0 && !ChildServerRelay.<UpdateRead>g__readBytes|26_0(msg.AsSpan<byte>()))
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

		// Token: 0x0600338D RID: 13197 RVA: 0x0015C8E4 File Offset: 0x0015AAE4
		private static void UpdateWrite()
		{
			while (!ChildServerRelay.HasShutDown)
			{
				ChildServerRelay.CheckPipeConnected("writeStream", ChildServerRelay.writeStream);
				if (ChildServerRelay.status == ChildServerRelay.StatusEnum.RequestedShutDown)
				{
					ChildServerRelay.<UpdateWrite>g__writeMsg|27_0(ChildServerRelay.WriteStatus.RequestShutdown, Array.Empty<byte>());
					ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
				}
				string error;
				while (ChildServerRelay.errorsToWrite.TryDequeue(out error))
				{
					ChildServerRelay.<UpdateWrite>g__writeMsg|27_0(ChildServerRelay.WriteStatus.Crash, Encoding.UTF8.GetBytes(error));
					ChildServerRelay.status = ChildServerRelay.StatusEnum.ShutDown;
				}
				byte[] msg;
				while (ChildServerRelay.msgsToWrite.TryDequeue(out msg))
				{
					ChildServerRelay.<UpdateWrite>g__writeMsg|27_0(ChildServerRelay.WriteStatus.Success, msg);
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
						ChildServerRelay.<UpdateWrite>g__writeMsg|27_0(ChildServerRelay.WriteStatus.Heartbeat, Array.Empty<byte>());
					}
				}
			}
		}

		// Token: 0x0600338E RID: 13198 RVA: 0x0015C9AF File Offset: 0x0015ABAF
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

		// Token: 0x0600338F RID: 13199 RVA: 0x0015C9DA File Offset: 0x0015ABDA
		public static IEnumerable<byte[]> Read()
		{
			return new ChildServerRelay.<Read>d__31(-2);
		}

		// Token: 0x06003390 RID: 13200 RVA: 0x0015C9E3 File Offset: 0x0015ABE3
		private static bool ReadSingleMessage(out byte[] msg)
		{
			if (ChildServerRelay.HasShutDown)
			{
				msg = null;
				return false;
			}
			return ChildServerRelay.msgsToRead.TryDequeue(out msg);
		}

		// Token: 0x06003392 RID: 13202 RVA: 0x0015CA10 File Offset: 0x0015AC10
		[CompilerGenerated]
		internal unsafe static bool <UpdateRead>g__readBytes|26_0(Span<byte> readTo)
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

		// Token: 0x06003393 RID: 13203 RVA: 0x0015CA88 File Offset: 0x0015AC88
		[CompilerGenerated]
		internal unsafe static void <UpdateWrite>g__writeMsg|27_0(ChildServerRelay.WriteStatus writeStatus, byte[] msg)
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
				AnonymousPipeClientStream anonymousPipeClientStream = ChildServerRelay.writeStream;
				if (anonymousPipeClientStream != null)
				{
					anonymousPipeClientStream.Write(headerBytes);
				}
				AnonymousPipeClientStream anonymousPipeClientStream2 = ChildServerRelay.writeStream;
				if (anonymousPipeClientStream2 != null)
				{
					anonymousPipeClientStream2.Write(msg);
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

		// Token: 0x04001989 RID: 6537
		private static AnonymousPipeClientStream writeStream;

		// Token: 0x0400198A RID: 6538
		private static AnonymousPipeClientStream readStream;

		// Token: 0x0400198B RID: 6539
		private static ManualResetEvent writeManualResetEvent;

		// Token: 0x0400198C RID: 6540
		private static volatile ChildServerRelay.StatusEnum status = ChildServerRelay.StatusEnum.NeverStarted;

		// Token: 0x0400198D RID: 6541
		private const int ReadBufferSize = 2340;

		// Token: 0x0400198E RID: 6542
		private static byte[] readTempBytes;

		// Token: 0x0400198F RID: 6543
		private static int readIncOffset;

		// Token: 0x04001990 RID: 6544
		private static int readIncTotal;

		// Token: 0x04001991 RID: 6545
		private static ConcurrentQueue<byte[]> msgsToWrite;

		// Token: 0x04001992 RID: 6546
		private static ConcurrentQueue<string> errorsToWrite;

		// Token: 0x04001993 RID: 6547
		private static ConcurrentQueue<byte[]> msgsToRead;

		// Token: 0x04001994 RID: 6548
		private static Thread readThread;

		// Token: 0x04001995 RID: 6549
		private static Thread writeThread;

		// Token: 0x04001996 RID: 6550
		private static CancellationTokenSource readCancellationToken;

		// Token: 0x04001997 RID: 6551
		private static readonly Stopwatch stopwatch = new Stopwatch();

		// Token: 0x04001998 RID: 6552
		private const int MaxMilliseconds = 8;

		// Token: 0x02000BB3 RID: 2995
		private enum WriteStatus : byte
		{
			// Token: 0x04003A0D RID: 14861
			Success,
			// Token: 0x04003A0E RID: 14862
			Heartbeat,
			// Token: 0x04003A0F RID: 14863
			RequestShutdown = 204,
			// Token: 0x04003A10 RID: 14864
			Crash = 255
		}

		// Token: 0x02000BB4 RID: 2996
		private enum StatusEnum
		{
			// Token: 0x04003A12 RID: 14866
			NeverStarted,
			// Token: 0x04003A13 RID: 14867
			Active,
			// Token: 0x04003A14 RID: 14868
			RequestedShutDown,
			// Token: 0x04003A15 RID: 14869
			ShutDown
		}

		// Token: 0x02000BB5 RID: 2997
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003A16 RID: 14870
			public static ThreadStart <0>__UpdateRead;

			// Token: 0x04003A17 RID: 14871
			public static ThreadStart <1>__UpdateWrite;
		}
	}
}
