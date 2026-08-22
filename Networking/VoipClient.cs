using System;
using System.Collections.Generic;
using System.Linq;
using Barotrauma.Items.Components;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x02000473 RID: 1139
	internal class VoipClient : IDisposable
	{
		// Token: 0x06004DBF RID: 19903 RVA: 0x002AB027 File Offset: 0x002A9227
		public VoipClient(GameClient gClient, ClientPeer nClient)
		{
			this.gameClient = gClient;
			this.netClient = nClient;
			this.queues = new List<VoipQueue>();
			this.lastSendTime = DateTime.Now;
		}

		// Token: 0x06004DC0 RID: 19904 RVA: 0x002AB053 File Offset: 0x002A9253
		public void RegisterQueue(VoipQueue queue)
		{
			if (queue == VoipCapture.Instance)
			{
				return;
			}
			if (!this.queues.Contains(queue))
			{
				this.queues.Add(queue);
			}
		}

		// Token: 0x06004DC1 RID: 19905 RVA: 0x002AB078 File Offset: 0x002A9278
		public void UnregisterQueue(VoipQueue queue)
		{
			if (this.queues.Contains(queue))
			{
				this.queues.Remove(queue);
			}
		}

		// Token: 0x06004DC2 RID: 19906 RVA: 0x002AB098 File Offset: 0x002A9298
		public unsafe void SendToServer()
		{
			if (GameSettings.CurrentConfig.Audio.VoiceSetting == VoiceMode.Disabled)
			{
				if (VoipCapture.Instance != null)
				{
					this.storedBufferID = VoipCapture.Instance.LatestBufferID;
					VoipCapture.Instance.Dispose();
				}
				return;
			}
			try
			{
				if (VoipCapture.Instance == null)
				{
					VoipCapture.Create(GameSettings.CurrentConfig.Audio.VoiceCaptureDevice, new ushort?(this.storedBufferID));
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("VoipCature.Create failed: " + e.Message + " " + e.StackTrace.CleanupStackTrace(), null, null, false, false);
				GameSettings.Config config = *GameSettings.CurrentConfig;
				config.Audio.VoiceSetting = VoiceMode.Disabled;
				GameSettings.SetCurrentConfig(config);
			}
			if (VoipCapture.Instance == null || VoipCapture.Instance.EnqueuedTotalLength <= 0)
			{
				return;
			}
			if (DateTime.Now >= this.lastSendTime + VoipConfig.SEND_INTERVAL)
			{
				IWriteMessage msg = new WriteOnlyMessage();
				msg.WriteByte(6);
				msg.WriteByte(VoipCapture.Instance.QueueID);
				VoipCapture.Instance.Write(msg);
				this.netClient.Send(msg, DeliveryMethod.Unreliable, true);
				this.lastSendTime = DateTime.Now;
			}
		}

		// Token: 0x06004DC3 RID: 19907 RVA: 0x002AB1D0 File Offset: 0x002A93D0
		public void Read(IReadMessage msg)
		{
			byte queueId = msg.ReadByte();
			float distanceFactor = msg.ReadRangedSingle(0f, 1f, 8);
			bool isRadio = msg.ReadBoolean();
			VoipQueue queue = this.queues.Find((VoipQueue q) => q.QueueID == queueId);
			if (queue == null)
			{
				return;
			}
			Client client = this.gameClient.ConnectedClients.Find((Client c) => c.VoipQueue == queue);
			if (queue.Read(msg, client.Muted || client.MutedLocally))
			{
				if (client.Muted || client.MutedLocally)
				{
					return;
				}
				if (client.VoipSound == null)
				{
					DebugConsole.Log("Recreating voipsound " + queueId.ToString());
					client.VoipSound = new VoipSound(client, GameMain.SoundManager, client.VoipQueue);
				}
				GameMain.SoundManager.ForceStreamUpdate();
				client.RadioNoise = 0f;
				if (client.Character != null && !client.Character.IsDead && !client.Character.Removed && client.Character.SpeechImpediment <= 100f)
				{
					float speechImpedimentMultiplier = 1f - client.Character.SpeechImpediment / 100f;
					bool spectating = Character.Controlled == null;
					float rangeMultiplier = spectating ? 2f : 1f;
					WifiComponent senderRadio = null;
					ChatMessageType messageType = isRadio ? ChatMessageType.Radio : ChatMessageType.Default;
					client.Character.ShowTextlessSpeechBubble(1.25f, ChatMessage.MessageColor[(int)messageType]);
					client.VoipSound.UseRadioFilter = (messageType == ChatMessageType.Radio && !GameSettings.CurrentConfig.Audio.DisableVoiceChatFilters);
					client.RadioNoise = 0f;
					if (messageType == ChatMessageType.Radio)
					{
						ChatMessage.CanUseRadio(client.Character, out senderRadio, false);
						float senderRadioRange = (senderRadio == null) ? 35000f : senderRadio.Range;
						client.VoipSound.UsingRadio = true;
						client.VoipSound.SetRange(senderRadioRange * 0.4f * speechImpedimentMultiplier * rangeMultiplier, senderRadioRange * speechImpedimentMultiplier * rangeMultiplier);
						if (distanceFactor > 0.4f && !spectating)
						{
							client.RadioNoise = MathF.Pow(MathUtils.InverseLerp(0.4f, 1f, distanceFactor), 2f);
						}
					}
					else
					{
						client.VoipSound.UsingRadio = false;
						client.VoipSound.SetRange(400f * speechImpedimentMultiplier * rangeMultiplier, 1000f * speechImpedimentMultiplier * rangeMultiplier);
					}
					client.VoipSound.UseMuffleFilter = (messageType != ChatMessageType.Radio && Character.Controlled != null && !GameSettings.CurrentConfig.Audio.DisableVoiceChatFilters && SoundPlayer.ShouldMuffleSound(Character.Controlled, client.Character.WorldPosition, 1000f, client.Character.CurrentHull));
				}
				NetLobbyScreen netLobbyScreen = GameMain.NetLobbyScreen;
				if (netLobbyScreen != null)
				{
					netLobbyScreen.SetPlayerSpeaking(client);
				}
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					CrewManager crewManager = gameSession.CrewManager;
					if (crewManager != null)
					{
						crewManager.SetClientSpeaking(client);
					}
				}
				if (client.VoipSound.CurrentAmplitude * client.VoipSound.Gain * GameMain.SoundManager.GetCategoryGainMultiplier(SoundManager.SoundCategoryVoip, -1) > 0.1f)
				{
					if (client.Character != null && !client.Character.Removed && !client.Character.IsDead)
					{
						Vector3 clientPos = new Vector3(client.Character.WorldPosition.X, client.Character.WorldPosition.Y, 0f);
						Vector3 listenerPos = GameMain.SoundManager.ListenerPosition;
						float attenuationDist = client.VoipSound.Near * 1.125f;
						if (Vector3.DistanceSquared(clientPos, listenerPos) < attenuationDist * attenuationDist)
						{
							GameMain.SoundManager.VoipAttenuatedGain = 0.5f;
							return;
						}
					}
					else
					{
						GameMain.SoundManager.VoipAttenuatedGain = 0.5f;
					}
				}
			}
		}

		// Token: 0x06004DC4 RID: 19908 RVA: 0x002AB598 File Offset: 0x002A9798
		public static void UpdateVoiceIndicator(GUIImage soundIcon, float voipAmplitude, float deltaTime)
		{
			if (VoipClient.voiceIconSheetRects == null)
			{
				GUIComponentStyle soundIconStyle = GUIStyle.GetComponentStyle("GUISoundIcon");
				Rectangle sourceRect = soundIconStyle.Sprites.First<KeyValuePair<GUIComponent.ComponentState, List<UISprite>>>().Value.First<UISprite>().Sprite.SourceRect;
				string[] indexPieces = soundIconStyle.Element.GetAttribute("sheetindices").Value.Split(';', StringSplitOptions.None);
				VoipClient.voiceIconSheetRects = new Rectangle[indexPieces.Length];
				for (int i = 0; i < indexPieces.Length; i++)
				{
					Point location = sourceRect.Location + XMLExtensions.ParsePoint(indexPieces[i].Trim(), true) * sourceRect.Size;
					VoipClient.voiceIconSheetRects[i] = new Rectangle(location, sourceRect.Size);
				}
			}
			Pair<string, float> userdata = soundIcon.UserData as Pair<string, float>;
			userdata.Second = Math.Max(voipAmplitude, userdata.Second - deltaTime);
			if (userdata.Second <= 0f)
			{
				soundIcon.Visible = false;
				return;
			}
			soundIcon.Visible = true;
			int sheetIndex = (int)Math.Floor((double)(userdata.Second * (float)VoipClient.voiceIconSheetRects.Length));
			sheetIndex = MathHelper.Clamp(sheetIndex, 0, VoipClient.voiceIconSheetRects.Length - 1);
			soundIcon.SourceRect = VoipClient.voiceIconSheetRects[sheetIndex];
			soundIcon.OverrideState = new GUIComponent.ComponentState?(GUIComponent.ComponentState.None);
			soundIcon.HoverColor = Color.White;
		}

		// Token: 0x06004DC5 RID: 19909 RVA: 0x002AB6EE File Offset: 0x002A98EE
		public void Dispose()
		{
			VoipCapture instance = VoipCapture.Instance;
			if (instance == null)
			{
				return;
			}
			instance.Dispose();
		}

		// Token: 0x040028A7 RID: 10407
		private const float RangeNear = 0.4f;

		// Token: 0x040028A8 RID: 10408
		private readonly GameClient gameClient;

		// Token: 0x040028A9 RID: 10409
		private readonly ClientPeer netClient;

		// Token: 0x040028AA RID: 10410
		private DateTime lastSendTime;

		// Token: 0x040028AB RID: 10411
		private readonly List<VoipQueue> queues;

		// Token: 0x040028AC RID: 10412
		private ushort storedBufferID;

		// Token: 0x040028AD RID: 10413
		private static Rectangle[] voiceIconSheetRects;
	}
}
