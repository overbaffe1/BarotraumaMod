using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma.Media
{
	// Token: 0x020004CD RID: 1229
	internal class Video : IDisposable
	{
		// Token: 0x06004FFB RID: 20475 RVA: 0x002B0454 File Offset: 0x002AE654
		public static void Init()
		{
			if (Video.VideoFrameCallback == null)
			{
				Video.Internal.EventCallback videoFrameCallback;
				if ((videoFrameCallback = Video.<>O.<0>__VideoFrameUpdate) == null)
				{
					videoFrameCallback = (Video.<>O.<0>__VideoFrameUpdate = new Video.Internal.EventCallback(Video.VideoFrameUpdate));
				}
				Video.VideoFrameCallback = videoFrameCallback;
			}
			if (Video.VideoAudioCallback == null)
			{
				Video.Internal.EventCallback videoAudioCallback;
				if ((videoAudioCallback = Video.<>O.<1>__VideoAudioUpdate) == null)
				{
					videoAudioCallback = (Video.<>O.<1>__VideoAudioUpdate = new Video.Internal.EventCallback(Video.VideoAudioUpdate));
				}
				Video.VideoAudioCallback = videoAudioCallback;
			}
			if (Video.videos == null)
			{
				Video.videos = new Dictionary<IntPtr, Video>();
			}
		}

		// Token: 0x06004FFC RID: 20476 RVA: 0x002B04C0 File Offset: 0x002AE6C0
		public static void Close()
		{
			if (Video.videos != null)
			{
				List<Video> vids = Video.videos.Values.ToList<Video>();
				foreach (Video v in vids)
				{
					v.Dispose();
				}
			}
		}

		// Token: 0x17001460 RID: 5216
		// (get) Token: 0x06004FFD RID: 20477 RVA: 0x002B0524 File Offset: 0x002AE724
		// (set) Token: 0x06004FFE RID: 20478 RVA: 0x002B052C File Offset: 0x002AE72C
		public int Width { get; private set; }

		// Token: 0x17001461 RID: 5217
		// (get) Token: 0x06004FFF RID: 20479 RVA: 0x002B0535 File Offset: 0x002AE735
		// (set) Token: 0x06005000 RID: 20480 RVA: 0x002B053D File Offset: 0x002AE73D
		public int Height { get; private set; }

		// Token: 0x17001462 RID: 5218
		// (get) Token: 0x06005001 RID: 20481 RVA: 0x002B0546 File Offset: 0x002AE746
		// (set) Token: 0x06005002 RID: 20482 RVA: 0x002B0561 File Offset: 0x002AE761
		public float AudioGain
		{
			get
			{
				if (this.sound != null)
				{
					return this.sound.BaseGain;
				}
				return 0f;
			}
			set
			{
				if (this.sound != null)
				{
					this.sound.BaseGain = value;
				}
			}
		}

		// Token: 0x17001463 RID: 5219
		// (get) Token: 0x06005003 RID: 20483 RVA: 0x002B0577 File Offset: 0x002AE777
		// (set) Token: 0x06005004 RID: 20484 RVA: 0x002B057F File Offset: 0x002AE77F
		public bool LoadFailed { get; private set; }

		// Token: 0x06005005 RID: 20485 RVA: 0x002B0588 File Offset: 0x002AE788
		public static Video Load(GraphicsDevice graphicsDevice, SoundManager soundManager, string filename)
		{
			Video video = new Video(graphicsDevice, soundManager, filename);
			if (video.LoadFailed)
			{
				video = null;
			}
			return video;
		}

		// Token: 0x06005006 RID: 20486 RVA: 0x002B05AC File Offset: 0x002AE7AC
		private Video(GraphicsDevice graphicsDevice, SoundManager soundManager, string filename)
		{
			Video.Init();
			this.videoInternal = Video.Internal.loadVideo(filename);
			if (this.videoInternal == IntPtr.Zero)
			{
				this.LoadFailed = true;
				return;
			}
			this.mutex = new object();
			this.Width = Video.Internal.getVideoWidth(this.videoInternal);
			this.Height = Video.Internal.getVideoHeight(this.videoInternal);
			this.texture = new Texture2D(graphicsDevice, this.Width, this.Height);
			this.textureData = new int[this.Width * this.Height];
			for (int i = 0; i < this.Width * this.Height; i++)
			{
				this.textureData[i] = -16777216;
			}
			this.texture.SetData<int>(this.textureData);
			Video.videos.Add(this.videoInternal, this);
			IntPtr videoFrameCallbackPtr = Marshal.GetFunctionPointerForDelegate<Video.Internal.EventCallback>(Video.VideoFrameCallback);
			Video.Internal.setVideoFrameCallback(this.videoInternal, videoFrameCallbackPtr);
			IntPtr videoAudioCallbackPtr = Marshal.GetFunctionPointerForDelegate<Video.Internal.EventCallback>(Video.VideoAudioCallback);
			Video.Internal.setVideoAudioCallback(this.videoInternal, videoAudioCallbackPtr);
			this.sound = null;
			if (Video.Internal.videoHasAudio(this.videoInternal) == 1)
			{
				int sampleRate = Video.Internal.getVideoAudioSampleRate(this.videoInternal);
				int channelCount = Video.Internal.getVideoAudioChannelCount(this.videoInternal);
				this.sound = new VideoSound(soundManager, filename, sampleRate, channelCount, this);
			}
			this.textureChanged = false;
			Video.Internal.playVideo(this.videoInternal);
		}

		// Token: 0x06005007 RID: 20487 RVA: 0x002B0708 File Offset: 0x002AE908
		public void Play()
		{
			if (this.LoadFailed)
			{
				return;
			}
			Video.Internal.playVideo(this.videoInternal);
		}

		// Token: 0x06005008 RID: 20488 RVA: 0x002B0720 File Offset: 0x002AE920
		public void Dispose()
		{
			if (this.LoadFailed)
			{
				return;
			}
			Video.Internal.deleteVideo(this.videoInternal);
			Video.videos.Remove(this.videoInternal);
			VideoSound videoSound = this.sound;
			if (videoSound != null)
			{
				videoSound.Dispose();
			}
			this.texture.Dispose();
		}

		// Token: 0x17001464 RID: 5220
		// (get) Token: 0x06005009 RID: 20489 RVA: 0x002B076E File Offset: 0x002AE96E
		public bool IsPlaying
		{
			get
			{
				return !this.LoadFailed && Video.Internal.isVideoPlaying(this.videoInternal) == 1;
			}
		}

		// Token: 0x0600500A RID: 20490 RVA: 0x002B0788 File Offset: 0x002AE988
		public Texture2D GetTexture()
		{
			if (this.LoadFailed)
			{
				return null;
			}
			object obj = this.mutex;
			lock (obj)
			{
				if (this.textureChanged)
				{
					this.texture.SetData<int>(this.textureData);
					this.textureChanged = false;
				}
				if (this.sound != null && !this.sound.IsPlaying() && this.IsPlaying)
				{
					this.sound.Play();
				}
			}
			return this.texture;
		}

		// Token: 0x0600500B RID: 20491 RVA: 0x002B081C File Offset: 0x002AEA1C
		public void SetFrameData(IntPtr data)
		{
			object obj = this.mutex;
			lock (obj)
			{
				Marshal.Copy(data, this.textureData, 0, this.Width * this.Height);
				this.textureChanged = true;
			}
		}

		// Token: 0x0600500C RID: 20492 RVA: 0x002B0878 File Offset: 0x002AEA78
		private static void VideoFrameUpdate(IntPtr videoInternal, IntPtr data, int dataElemSize, int dataLen)
		{
			Video video = Video.videos[videoInternal];
			video.SetFrameData(data);
		}

		// Token: 0x0600500D RID: 20493 RVA: 0x002B0898 File Offset: 0x002AEA98
		private static void VideoAudioUpdate(IntPtr videoInternal, IntPtr data, int dataElemSize, int dataLen)
		{
			Video video = Video.videos[videoInternal];
			if (video.sound != null && dataLen > 0)
			{
				short[] newBuf = new short[dataLen];
				Marshal.Copy(data, newBuf, 0, dataLen);
				video.sound.Enqueue(newBuf);
			}
		}

		// Token: 0x04002A23 RID: 10787
		private static Video.Internal.EventCallback VideoFrameCallback;

		// Token: 0x04002A24 RID: 10788
		private static Video.Internal.EventCallback VideoAudioCallback;

		// Token: 0x04002A25 RID: 10789
		private static Dictionary<IntPtr, Video> videos;

		// Token: 0x04002A26 RID: 10790
		private IntPtr videoInternal;

		// Token: 0x04002A27 RID: 10791
		private Texture2D texture;

		// Token: 0x04002A28 RID: 10792
		private bool textureChanged;

		// Token: 0x04002A29 RID: 10793
		private int[] textureData;

		// Token: 0x04002A2A RID: 10794
		private object mutex;

		// Token: 0x04002A2B RID: 10795
		private VideoSound sound;

		// Token: 0x02001253 RID: 4691
		private static class Internal
		{
			// Token: 0x060093FD RID: 37885
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern IntPtr loadVideo(string filename);

			// Token: 0x060093FE RID: 37886
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern int getVideoWidth(IntPtr videoInternal);

			// Token: 0x060093FF RID: 37887
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern int getVideoHeight(IntPtr videoInternal);

			// Token: 0x06009400 RID: 37888
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern int videoHasAudio(IntPtr videoInternal);

			// Token: 0x06009401 RID: 37889
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern int getVideoAudioSampleRate(IntPtr videoInternal);

			// Token: 0x06009402 RID: 37890
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern int getVideoAudioChannelCount(IntPtr videoInternal);

			// Token: 0x06009403 RID: 37891
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern void deleteVideo(IntPtr videoInternal);

			// Token: 0x06009404 RID: 37892
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern void playVideo(IntPtr videoInternal);

			// Token: 0x06009405 RID: 37893
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern void stopVideo(IntPtr videoInternal);

			// Token: 0x06009406 RID: 37894
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern int isVideoPlaying(IntPtr videoInternal);

			// Token: 0x06009407 RID: 37895
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern void setVideoFrameCallback(IntPtr videoInternal, IntPtr callback);

			// Token: 0x06009408 RID: 37896
			[DllImport("webm_mem_playback_x64.dll", CallingConvention = 2, CharSet = CharSet.Ansi)]
			public static extern void setVideoAudioCallback(IntPtr videoInternal, IntPtr callback);

			// Token: 0x04005EDB RID: 24283
			private const string DLL_NAME = "webm_mem_playback_x64.dll";

			// Token: 0x04005EDC RID: 24284
			private const CallingConvention CALLING_CONVENTION = CallingConvention.Cdecl;

			// Token: 0x020015C7 RID: 5575
			// (Invoke) Token: 0x06009F09 RID: 40713
			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			public delegate void EventCallback(IntPtr videoInternal, IntPtr data, int dataElemSize, int dataLen);
		}

		// Token: 0x02001254 RID: 4692
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005EDD RID: 24285
			public static Video.Internal.EventCallback <0>__VideoFrameUpdate;

			// Token: 0x04005EDE RID: 24286
			public static Video.Internal.EventCallback <1>__VideoAudioUpdate;
		}
	}
}
