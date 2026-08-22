using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.IO;
using Barotrauma.Networking;
using Concentus.Structs;

namespace Barotrauma
{
	// Token: 0x02000044 RID: 68
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class VoipServerDecoder
	{
		// Token: 0x17000307 RID: 775
		// (get) Token: 0x06000B35 RID: 2869 RVA: 0x0006C258 File Offset: 0x0006A458
		// (set) Token: 0x06000B36 RID: 2870 RVA: 0x0006C260 File Offset: 0x0006A460
		public float Amplitude { get; private set; }

		// Token: 0x06000B37 RID: 2871 RVA: 0x0006C269 File Offset: 0x0006A469
		public VoipServerDecoder(VoipQueue q, Client owner)
		{
			this.ownerClient = owner;
			this.decoder = VoipConfig.CreateDecoder();
			this.queue = q;
			this.lastRetrievedBufferID = (int)q.LatestBufferID;
		}

		// Token: 0x17000308 RID: 776
		// (get) Token: 0x06000B38 RID: 2872 RVA: 0x0006C2A1 File Offset: 0x0006A4A1
		// (set) Token: 0x06000B39 RID: 2873 RVA: 0x0006C2A8 File Offset: 0x0006A4A8
		public static bool DebugVoip
		{
			get
			{
				return VoipServerDecoder.debugVoip;
			}
			set
			{
				VoipServerDecoder.debugVoip = false;
				if (value)
				{
					DebugConsole.ThrowError("DebugVoip is only available in debug builds of the game", null, null, false, false);
				}
			}
		}

		// Token: 0x17000309 RID: 777
		// (get) Token: 0x06000B3A RID: 2874 RVA: 0x0006C2C1 File Offset: 0x0006A4C1
		// (set) Token: 0x06000B3B RID: 2875 RVA: 0x0006C2C9 File Offset: 0x0006A4C9
		private float DebugWriteTimer
		{
			get
			{
				return this.debugWriteTimerBacking;
			}
			set
			{
				this.debugWriteTimerBacking = Math.Clamp(value, 0f, 3f);
			}
		}

		// Token: 0x06000B3C RID: 2876 RVA: 0x0006C2E4 File Offset: 0x0006A4E4
		public void OnNewVoiceReceived()
		{
			float amplitude = 0f;
			for (int i = this.lastRetrievedBufferID + 1; i <= (int)this.queue.LatestBufferID; i++)
			{
				int compressedSize;
				byte[] compressedBuffer;
				this.queue.RetrieveBuffer(i, out compressedSize, out compressedBuffer);
				if (compressedSize > 0)
				{
					short[] buffer = new short[960];
					this.decoder.Decode(compressedBuffer, 0, compressedSize, buffer, 0, 960, false);
					amplitude = Math.Max(amplitude, VoipServerDecoder.GetAmplitude(buffer));
					this.lastRetrievedBufferID = i;
					if (VoipServerDecoder.DebugVoip)
					{
						List<short[]> obj = this.debugStoredSamples;
						lock (obj)
						{
							this.debugStoredSamples.Add(buffer);
						}
					}
				}
			}
			this.Amplitude = amplitude;
			if (VoipServerDecoder.DebugVoip)
			{
				this.DebugWriteTimer = 3f;
			}
		}

		// Token: 0x06000B3D RID: 2877 RVA: 0x0006C3C8 File Offset: 0x0006A5C8
		public void DebugUpdate(float deltaTime)
		{
			if (!VoipServerDecoder.DebugVoip)
			{
				return;
			}
			if (this.DebugWriteTimer > 0f)
			{
				this.DebugWriteTimer -= deltaTime;
				if (this.DebugWriteTimer <= 0f)
				{
					this.shouldWriteDebugFile = true;
				}
				return;
			}
			if (!this.shouldWriteDebugFile)
			{
				return;
			}
			List<short[]> obj = this.debugStoredSamples;
			lock (obj)
			{
				this.debugStoredSamples.Clear();
				this.shouldWriteDebugFile = false;
			}
		}

		// Token: 0x06000B3E RID: 2878 RVA: 0x0006C458 File Offset: 0x0006A658
		private static float GetAmplitude(short[] values)
		{
			float max = 0f;
			foreach (short v in values)
			{
				max = Math.Max(max, ToolBox.ShortAudioSampleToFloat(v));
			}
			return max;
		}

		// Token: 0x06000B3F RID: 2879 RVA: 0x0006C490 File Offset: 0x0006A690
		private static void WriteSamplesToWaveFile(IReadOnlyList<short[]> samples, string filename, int sampleRate, short channels)
		{
			if (!samples.Any<short[]>())
			{
				return;
			}
			string path = Path.Combine(new string[]
			{
				Path.GetFullPath("AudioDebug")
			});
			if (!Directory.Exists(path))
			{
				DirectoryInfo dir = Directory.CreateDirectory(path, false);
				if (dir == null || !dir.Exists)
				{
					return;
				}
			}
			using (FileStream outFile = File.Create(Path.Combine(new string[]
			{
				path,
				ToolBox.RemoveInvalidFileNameChars(filename)
			}), true))
			{
				if (outFile == null)
				{
					DebugConsole.ThrowError("Failed to create audio debug file", null, null, false, false);
				}
				else
				{
					using (BinaryWriter writer = new BinaryWriter(outFile))
					{
						int byteRate = sampleRate * 16 * (int)channels / 8;
						short blockAlign = 16 * channels / 8;
						writer.Write(Encoding.ASCII.GetBytes("RIFF"));
						long sizePos = outFile.Position;
						writer.Write(0);
						writer.Write(Encoding.ASCII.GetBytes("WAVE"));
						writer.Write(Encoding.ASCII.GetBytes("fmt "));
						writer.Write(16);
						writer.Write(1);
						writer.Write(channels);
						writer.Write(sampleRate);
						writer.Write(byteRate);
						writer.Write(blockAlign);
						writer.Write(16);
						writer.Write(Encoding.ASCII.GetBytes("data"));
						writer.Flush();
						long dataPos = outFile.Position;
						writer.Write(0);
						foreach (short[] sample in samples)
						{
							foreach (short s in sample)
							{
								writer.Write(s);
							}
						}
						writer.Flush();
						writer.Seek((int)sizePos, SeekOrigin.Begin);
						writer.Write((int)(outFile.Length - 8L));
						writer.Seek((int)dataPos, SeekOrigin.Begin);
						writer.Write((int)(outFile.Length - dataPos));
						writer.Flush();
					}
				}
			}
		}

		// Token: 0x06000B40 RID: 2880 RVA: 0x0006C6CC File Offset: 0x0006A8CC
		private void ClearStoredDebugSamples()
		{
			List<short[]> obj = this.debugStoredSamples;
			lock (obj)
			{
				this.debugStoredSamples.Clear();
			}
			this.DebugWriteTimer = 0f;
			this.shouldWriteDebugFile = false;
		}

		// Token: 0x040004DB RID: 1243
		private readonly OpusDecoder decoder;

		// Token: 0x040004DC RID: 1244
		private readonly VoipQueue queue;

		// Token: 0x040004DD RID: 1245
		private int lastRetrievedBufferID;

		// Token: 0x040004DF RID: 1247
		private readonly Client ownerClient;

		// Token: 0x040004E0 RID: 1248
		private static bool debugVoip;

		// Token: 0x040004E1 RID: 1249
		private readonly List<short[]> debugStoredSamples = new List<short[]>();

		// Token: 0x040004E2 RID: 1250
		private float debugWriteTimerBacking;

		// Token: 0x040004E3 RID: 1251
		private bool shouldWriteDebugFile;

		// Token: 0x040004E4 RID: 1252
		private const float DebugWriteTimeout = 3f;
	}
}
