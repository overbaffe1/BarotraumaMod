using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Barotrauma.Items.Components;
using Barotrauma.Sounds;
using Concentus.Structs;
using Microsoft.Xna.Framework;
using OpenAL;

namespace Barotrauma.Networking
{
	// Token: 0x02000472 RID: 1138
	internal class VoipCapture : VoipQueue, IDisposable
	{
		// Token: 0x170013F1 RID: 5105
		// (get) Token: 0x06004DAB RID: 19883 RVA: 0x002AA3E7 File Offset: 0x002A85E7
		// (set) Token: 0x06004DAC RID: 19884 RVA: 0x002AA3EE File Offset: 0x002A85EE
		public static VoipCapture Instance { get; private set; }

		// Token: 0x170013F2 RID: 5106
		// (get) Token: 0x06004DAD RID: 19885 RVA: 0x002AA3F6 File Offset: 0x002A85F6
		// (set) Token: 0x06004DAE RID: 19886 RVA: 0x002AA3FE File Offset: 0x002A85FE
		public double LastdB { get; private set; }

		// Token: 0x170013F3 RID: 5107
		// (get) Token: 0x06004DAF RID: 19887 RVA: 0x002AA407 File Offset: 0x002A8607
		// (set) Token: 0x06004DB0 RID: 19888 RVA: 0x002AA40F File Offset: 0x002A860F
		public double LastAmplitude { get; private set; }

		// Token: 0x170013F4 RID: 5108
		// (get) Token: 0x06004DB1 RID: 19889 RVA: 0x002AA418 File Offset: 0x002A8618
		public float Gain
		{
			get
			{
				return GameSettings.CurrentConfig.Audio.MicrophoneVolume;
			}
		}

		// Token: 0x170013F5 RID: 5109
		// (get) Token: 0x06004DB2 RID: 19890 RVA: 0x002AA429 File Offset: 0x002A8629
		// (set) Token: 0x06004DB3 RID: 19891 RVA: 0x002AA43B File Offset: 0x002A863B
		public override byte QueueID
		{
			get
			{
				GameClient client = GameMain.Client;
				if (client == null)
				{
					return 0;
				}
				return client.SessionId;
			}
			protected set
			{
			}
		}

		// Token: 0x170013F6 RID: 5110
		// (get) Token: 0x06004DB4 RID: 19892 RVA: 0x002AA43D File Offset: 0x002A863D
		// (set) Token: 0x06004DB5 RID: 19893 RVA: 0x002AA445 File Offset: 0x002A8645
		public bool Disconnected { get; private set; }

		// Token: 0x06004DB6 RID: 19894 RVA: 0x002AA450 File Offset: 0x002A8650
		public static void Create(string deviceName, ushort? storedBufferID = null)
		{
			if (VoipCapture.Instance != null)
			{
				throw new Exception("Tried to instance more than one VoipCapture object");
			}
			VoipCapture capture = new VoipCapture(deviceName)
			{
				LatestBufferID = storedBufferID.GetValueOrDefault(7)
			};
			if (capture.captureDevice != IntPtr.Zero)
			{
				VoipCapture.Instance = capture;
			}
		}

		// Token: 0x06004DB7 RID: 19895 RVA: 0x002AA498 File Offset: 0x002A8698
		private unsafe VoipCapture(string deviceName)
		{
			GameClient client = GameMain.Client;
			base..ctor((client != null) ? client.SessionId : 0, true, false);
			this.Disconnected = false;
			this.encoder = VoipConfig.CreateEncoder();
			this.captureDevice = Alc.CaptureOpenDevice(deviceName, 48000U, 4353, 4800);
			if (this.captureDevice == IntPtr.Zero)
			{
				DebugConsole.NewMessage("Alc.CaptureOpenDevice attempt 1 failed: error code " + Alc.GetError(IntPtr.Zero).ToString(), new Color?(Color.Orange), false);
				this.captureDevice = Alc.CaptureOpenDevice(deviceName, 48000U, 4353, 1920);
			}
			if (this.captureDevice == IntPtr.Zero)
			{
				DebugConsole.NewMessage("Alc.CaptureOpenDevice attempt 2 failed: error code " + Alc.GetError(IntPtr.Zero).ToString(), new Color?(Color.Orange), false);
				this.captureDevice = Alc.CaptureOpenDevice("", 48000U, 4353, 1920);
			}
			if (this.captureDevice == IntPtr.Zero)
			{
				string errorCode = Alc.GetError(IntPtr.Zero).ToString();
				if (!GUIMessageBox.MessageBoxes.Any((GUIComponent mb) => mb.UserData as string == "capturedevicenotfound"))
				{
					new GUIMessageBox(TextManager.Get("Error"), TextManager.Get("VoipCaptureDeviceNotFound").Fallback("Could not start voice capture, suitable capture device not found.", true) + " (" + errorCode + ")", null, null, GUIMessageBox.Type.Default).UserData = "capturedevicenotfound";
				}
				GameAnalyticsManager.AddErrorEventOnce("Alc.CaptureDeviceOpenFailed", GameAnalyticsManager.ErrorSeverity.Error, "Alc.CaptureDeviceOpen(" + deviceName + ") failed. Error code: " + errorCode);
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.Audio.VoiceSetting = VoiceMode.Disabled;
				GameSettings.SetCurrentConfig(config);
				VoipCapture instance = VoipCapture.Instance;
				if (instance != null)
				{
					instance.Dispose();
				}
				VoipCapture.Instance = null;
				return;
			}
			int alError = Al.GetError();
			int alcError = Alc.GetError(this.captureDevice);
			if (alcError != 0)
			{
				throw new Exception("Failed to open capture device: " + alcError.ToString() + " (ALC)");
			}
			if (alError != 0)
			{
				throw new Exception("Failed to open capture device: " + alError.ToString() + " (AL)");
			}
			this.CanDetectDisconnect = Alc.IsExtensionPresent(this.captureDevice, "ALC_EXT_disconnect");
			alcError = Alc.GetError(this.captureDevice);
			if (alcError != 0)
			{
				throw new Exception("Error determining if disconnect can be detected: " + alcError.ToString());
			}
			Alc.CaptureStart(this.captureDevice);
			alcError = Alc.GetError(this.captureDevice);
			if (alcError != 0)
			{
				throw new Exception("Failed to start capturing: " + alcError.ToString());
			}
			this.capturing = true;
			this.captureThread = new Thread(new ThreadStart(this.UpdateCapture))
			{
				IsBackground = true,
				Name = "VoipCapture"
			};
			this.captureThread.Start();
		}

		// Token: 0x06004DB8 RID: 19896 RVA: 0x002AA7D8 File Offset: 0x002A89D8
		public static void ChangeCaptureDevice(string deviceName)
		{
			if (VoipCapture.Instance == null)
			{
				return;
			}
			ushort storedBufferID = VoipCapture.Instance.LatestBufferID;
			VoipCapture.Instance.Dispose();
			VoipCapture.Create(GameSettings.CurrentConfig.Audio.VoiceCaptureDevice, new ushort?(storedBufferID));
		}

		// Token: 0x06004DB9 RID: 19897 RVA: 0x002AA81C File Offset: 0x002A8A1C
		public static IReadOnlyList<string> GetCaptureDeviceNames()
		{
			return Alc.GetStringList(IntPtr.Zero, 784);
		}

		// Token: 0x06004DBA RID: 19898 RVA: 0x002AA830 File Offset: 0x002A8A30
		private void UpdateCapture()
		{
			Array.Copy(this.uncompressedBuffer, 0, this.prevUncompressedBuffer, 0, 960);
			Array.Clear(this.uncompressedBuffer, 0, 960);
			this.nativeBuffer = Marshal.AllocHGlobal(1920);
			try
			{
				while (this.capturing)
				{
					int alcError;
					if (this.CanDetectDisconnect)
					{
						int isConnected;
						Alc.GetInteger(this.captureDevice, 787, out isConnected);
						alcError = Alc.GetError(this.captureDevice);
						if (alcError != 0)
						{
							throw new Exception("Failed to determine if capture device is connected: " + alcError.ToString());
						}
						if (isConnected == 0)
						{
							DebugConsole.ThrowError("Capture device has been disconnected. You can select another available device in the settings.", null, null, false, false);
							this.Disconnected = true;
							VoipCapture.TryRefreshDevice();
							break;
						}
					}
					this.FillBuffer();
					alcError = Alc.GetError(this.captureDevice);
					if (alcError != 0)
					{
						throw new Exception("Failed to capture samples: " + alcError.ToString());
					}
					double maxAmplitude = 0.0;
					for (int i = 0; i < 960; i++)
					{
						this.uncompressedBuffer[i] = (short)MathHelper.Clamp((float)this.uncompressedBuffer[i] * this.Gain, -32767f, 32767f);
						double sampleVal = (double)this.uncompressedBuffer[i] / 32767.0;
						maxAmplitude = Math.Max(maxAmplitude, Math.Abs(sampleVal));
					}
					double dB = Math.Min(20.0 * Math.Log10(maxAmplitude), 0.0);
					this.LastdB = dB;
					this.LastAmplitude = maxAmplitude;
					bool allowEnqueue = this.overrideSound != null;
					if (GameMain.WindowActive && SettingsMenu.Instance == null)
					{
						bool usingLocalMode = PlayerInput.KeyDown(InputType.LocalVoice);
						bool usingRadioMode = PlayerInput.KeyDown(InputType.RadioVoice);
						if (GameSettings.CurrentConfig.Audio.VoiceSetting == VoiceMode.Activity)
						{
							bool pttDown = (usingLocalMode || usingRadioMode) && GUI.KeyboardDispatcher.Subscriber == null;
							if (pttDown)
							{
								base.ForceLocal = usingLocalMode;
							}
							else
							{
								base.ForceLocal = (GameMain.ActiveChatMode == ChatMode.Local);
							}
							if (dB > (double)GameSettings.CurrentConfig.Audio.NoiseGateThreshold)
							{
								allowEnqueue = true;
							}
						}
						else if (GameSettings.CurrentConfig.Audio.VoiceSetting == VoiceMode.PushToTalk)
						{
							bool usingActiveMode = PlayerInput.KeyDown(InputType.Voice);
							bool pttDown2 = (usingActiveMode || usingLocalMode || usingRadioMode) && GUI.KeyboardDispatcher.Subscriber == null;
							if (pttDown2)
							{
								base.ForceLocal = ((usingActiveMode && GameMain.ActiveChatMode == ChatMode.Local) || usingLocalMode);
								allowEnqueue = true;
							}
						}
					}
					if (Screen.Selected is ModDownloadScreen)
					{
						allowEnqueue = false;
						this.captureTimer = 0;
					}
					if (allowEnqueue || this.captureTimer > 0)
					{
						this.LastEnqueueAudio = DateTime.Now;
						GameClient client = GameMain.Client;
						if (((client != null) ? client.Character : null) != null)
						{
							WifiComponent wifiComponent;
							ChatMessageType messageType = (!base.ForceLocal && ChatMessage.CanUseRadio(GameMain.Client.Character, out wifiComponent, false)) ? ChatMessageType.Radio : ChatMessageType.Default;
							if (GameMain.Client.Character.IsDead)
							{
								messageType = ChatMessageType.Dead;
							}
							GameMain.Client.Character.ShowTextlessSpeechBubble(1.25f, ChatMessage.MessageColor[(int)messageType]);
						}
						byte[][] buffers = this.buffers;
						lock (buffers)
						{
							if (!this.prevCaptured)
							{
								int compressedCountPrev = this.encoder.Encode(this.prevUncompressedBuffer, 0, 960, base.BufferToQueue, 0, 40);
								base.EnqueueBuffer(compressedCountPrev);
							}
							int compressedCount = this.encoder.Encode(this.uncompressedBuffer, 0, 960, base.BufferToQueue, 0, 40);
							base.EnqueueBuffer(compressedCount);
						}
						this.captureTimer -= 20;
						if (allowEnqueue)
						{
							this.captureTimer = GameSettings.CurrentConfig.Audio.VoiceChatCutoffPrevention;
						}
						this.prevCaptured = true;
					}
					else
					{
						this.captureTimer = 0;
						this.prevCaptured = false;
						byte[][] buffers2 = this.buffers;
						lock (buffers2)
						{
							base.EnqueueBuffer(0);
						}
					}
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("VoipCapture threw an exception. Disabling capture...", e, null, false, false);
				this.capturing = false;
			}
			finally
			{
				Marshal.FreeHGlobal(this.nativeBuffer);
			}
		}

		// Token: 0x06004DBB RID: 19899 RVA: 0x002AAC9C File Offset: 0x002A8E9C
		private void FillBuffer()
		{
			if (this.overrideSound != null)
			{
				int totalSampleCount = 0;
				while (totalSampleCount < 960)
				{
					int sampleCount = this.overrideSound.FillStreamBuffer(this.overridePos, this.overrideBuf);
					this.overridePos += sampleCount * 2;
					Array.Copy(this.overrideBuf, 0, this.uncompressedBuffer, totalSampleCount, Math.Min(sampleCount, this.uncompressedBuffer.Length - totalSampleCount));
					totalSampleCount += sampleCount;
					if (sampleCount == 0)
					{
						this.overridePos = 0;
					}
				}
				int sleepMs = 16;
				Thread.Sleep(sleepMs - 1);
				return;
			}
			int sampleCount2 = 0;
			while (sampleCount2 < 960)
			{
				Alc.GetInteger(this.captureDevice, 786, out sampleCount2);
				int alcError = Alc.GetError(this.captureDevice);
				if (alcError != 0)
				{
					throw new Exception("Failed to determine sample count: " + alcError.ToString());
				}
				if (sampleCount2 < 960)
				{
					int sleepMs2 = (960 - sampleCount2) * 800 / 48000;
					if (sleepMs2 >= 1)
					{
						Thread.Sleep(sleepMs2);
					}
				}
				if (!this.capturing)
				{
					return;
				}
			}
			Alc.CaptureSamples(this.captureDevice, this.nativeBuffer, 960);
			Marshal.Copy(this.nativeBuffer, this.uncompressedBuffer, 0, this.uncompressedBuffer.Length);
		}

		// Token: 0x06004DBC RID: 19900 RVA: 0x002AADCC File Offset: 0x002A8FCC
		public void SetOverrideSound(string fileName)
		{
			Sound sound = this.overrideSound;
			if (sound != null)
			{
				sound.Dispose();
			}
			if (string.IsNullOrEmpty(fileName))
			{
				this.overrideSound = null;
				return;
			}
			try
			{
				this.overrideSound = GameMain.SoundManager.LoadSound(fileName, true);
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Failed to load the sound " + fileName + ".", e, null, false, false);
			}
		}

		// Token: 0x06004DBD RID: 19901 RVA: 0x002AAE3C File Offset: 0x002A903C
		public override void Dispose()
		{
			VoipCapture.Instance = null;
			this.capturing = false;
			Thread thread = this.captureThread;
			if (thread != null)
			{
				thread.Join();
			}
			this.captureThread = null;
			if (this.captureDevice != IntPtr.Zero)
			{
				Alc.CaptureCloseDevice(this.captureDevice);
			}
		}

		// Token: 0x06004DBE RID: 19902 RVA: 0x002AAE7C File Offset: 0x002A907C
		public unsafe static void TryRefreshDevice()
		{
			DebugConsole.NewMessage("Refreshing audio capture device", null, false);
			List<string> deviceList = Alc.GetStringList(IntPtr.Zero, 784).ToList<string>();
			int alcError = Alc.GetError(IntPtr.Zero);
			if (alcError != 0)
			{
				DebugConsole.ThrowError("Failed to list available audio input devices: " + alcError.ToString(), null, null, false, false);
				return;
			}
			if (deviceList.Any<string>())
			{
				string availablePreviousDevice = deviceList.Find((string n) => n.Equals(GameSettings.CurrentConfig.Audio.VoiceCaptureDevice, StringComparison.OrdinalIgnoreCase));
				string device;
				if (availablePreviousDevice != null)
				{
					DebugConsole.NewMessage(" Previous device choice available: " + availablePreviousDevice, null, false);
					device = availablePreviousDevice;
				}
				else
				{
					device = Alc.GetString(IntPtr.Zero, 785);
					DebugConsole.NewMessage(" Reverting to default device: " + device, null, false);
				}
				if (string.IsNullOrEmpty(device))
				{
					device = deviceList[0];
					DebugConsole.NewMessage(" No default device found, resorting to first available device: " + device, null, false);
				}
				GameSettings.Config currentConfig = *GameSettings.CurrentConfig;
				currentConfig.Audio.VoiceCaptureDevice = device;
				GameSettings.SetCurrentConfig(currentConfig);
				VoipCapture currentCaptureInstance = VoipCapture.Instance;
				if (currentCaptureInstance != null)
				{
					currentCaptureInstance.Dispose();
				}
				VoipCapture.Create(GameSettings.CurrentConfig.Audio.VoiceCaptureDevice, null);
			}
			if (VoipCapture.Instance == null)
			{
				DebugConsole.NewMessage(" No devices found, disabling", null, false);
				GameSettings.Config currentConfig2 = *GameSettings.CurrentConfig;
				currentConfig2.Audio.VoiceSetting = VoiceMode.Disabled;
				GameSettings.SetCurrentConfig(currentConfig2);
			}
			if (GUI.SettingsMenuOpen)
			{
				SettingsMenu instance = SettingsMenu.Instance;
				if (instance == null)
				{
					return;
				}
				instance.CreateAudioAndVCTab(true);
			}
		}

		// Token: 0x04002896 RID: 10390
		private readonly IntPtr captureDevice;

		// Token: 0x04002897 RID: 10391
		private Thread captureThread;

		// Token: 0x04002898 RID: 10392
		private bool capturing;

		// Token: 0x04002899 RID: 10393
		private readonly OpusEncoder encoder;

		// Token: 0x0400289C RID: 10396
		public DateTime LastEnqueueAudio;

		// Token: 0x0400289D RID: 10397
		public readonly bool CanDetectDisconnect;

		// Token: 0x0400289F RID: 10399
		private IntPtr nativeBuffer;

		// Token: 0x040028A0 RID: 10400
		private readonly short[] uncompressedBuffer = new short[960];

		// Token: 0x040028A1 RID: 10401
		private readonly short[] prevUncompressedBuffer = new short[960];

		// Token: 0x040028A2 RID: 10402
		private bool prevCaptured = true;

		// Token: 0x040028A3 RID: 10403
		private int captureTimer;

		// Token: 0x040028A4 RID: 10404
		private Sound overrideSound;

		// Token: 0x040028A5 RID: 10405
		private int overridePos;

		// Token: 0x040028A6 RID: 10406
		private readonly short[] overrideBuf = new short[960];
	}
}
