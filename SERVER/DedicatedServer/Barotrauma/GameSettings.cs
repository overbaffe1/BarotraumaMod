using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200028B RID: 651
	[NullableContext(1)]
	[Nullable(0)]
	public static class GameSettings
	{
		// Token: 0x17000D71 RID: 3441
		// (get) Token: 0x06002DD3 RID: 11731 RVA: 0x0012F96A File Offset: 0x0012DB6A
		public static ref readonly GameSettings.Config CurrentConfig
		{
			get
			{
				return ref GameSettings.currentConfig;
			}
		}

		// Token: 0x06002DD4 RID: 11732 RVA: 0x0012F974 File Offset: 0x0012DB74
		public static void Init()
		{
			SaveUtil.EnsureSaveFolderExists();
			XDocument currentConfigDoc = null;
			if (File.Exists("config_player.xml"))
			{
				currentConfigDoc = XMLExtensions.TryLoadXml("config_player.xml");
			}
			if (currentConfigDoc == null)
			{
				GameSettings.currentConfig = GameSettings.Config.GetDefault();
				GameSettings.SaveCurrentConfig();
				return;
			}
			XElement root = currentConfigDoc.Root;
			if (root == null)
			{
				throw new NullReferenceException("Config XML element is invalid: document is null.");
			}
			GameSettings.Config? config = null;
			GameSettings.currentConfig = GameSettings.Config.FromElement(root, config);
		}

		// Token: 0x06002DD5 RID: 11733 RVA: 0x0012F9DC File Offset: 0x0012DBDC
		public static void SetCurrentConfig(in GameSettings.Config newConfig)
		{
			bool resolutionChanged = GameSettings.currentConfig.Graphics.Width != newConfig.Graphics.Width || GameSettings.currentConfig.Graphics.Height != newConfig.Graphics.Height;
			bool languageChanged = GameSettings.currentConfig.Language != newConfig.Language;
			bool audioOutputChanged = GameSettings.currentConfig.Audio.AudioOutputDevice != newConfig.Audio.AudioOutputDevice;
			bool voiceCaptureChanged = GameSettings.currentConfig.Audio.VoiceCaptureDevice != newConfig.Audio.VoiceCaptureDevice;
			bool textScaleChanged = Math.Abs(GameSettings.currentConfig.Graphics.TextScale - newConfig.Graphics.TextScale) > MathF.Pow(2f, -7f);
			bool hudScaleChanged = !MathUtils.NearlyEqual(GameSettings.currentConfig.Graphics.HUDScale, newConfig.Graphics.HUDScale, 0.0001f);
			bool flag = resolutionChanged || GameSettings.currentConfig.Graphics.VSync != newConfig.Graphics.VSync || GameSettings.currentConfig.Graphics.DisplayMode != newConfig.Graphics.DisplayMode || GameSettings.currentConfig.Graphics.Display != newConfig.Graphics.Display;
			GameSettings.currentConfig = newConfig;
			if (languageChanged)
			{
				TextManager.LanguageChanged();
			}
		}

		// Token: 0x06002DD6 RID: 11734 RVA: 0x0012FB4C File Offset: 0x0012DD4C
		public static void SaveCurrentConfig()
		{
			XDocument configDoc = new XDocument();
			XElement root = new XElement("config");
			configDoc.Add(root);
			ref GameSettings.currentConfig.SerializeElement(root);
			XElement graphicsElement = new XElement("graphicssettings");
			root.Add(graphicsElement);
			ref GameSettings.currentConfig.Graphics.SerializeElement(graphicsElement);
			XElement audioElement = new XElement("audio");
			root.Add(audioElement);
			ref GameSettings.currentConfig.Audio.SerializeElement(audioElement);
			XElement contentPackagesElement = new XElement("contentpackages");
			root.Add(contentPackagesElement);
			CorePackage core = ContentPackageManager.EnabledPackages.Core;
			XComment corePackageComment = new XComment(((core != null) ? core.Name : null) ?? "Vanilla");
			contentPackagesElement.Add(corePackageComment);
			XElement corePackageElement = new XElement("corepackage");
			contentPackagesElement.Add(corePackageElement);
			XElement xelement = corePackageElement;
			XName name = "path";
			CorePackage core2 = ContentPackageManager.EnabledPackages.Core;
			xelement.SetAttributeValue(name, ((core2 != null) ? core2.Path : null) ?? "Content/ContentPackages/Vanilla.xml");
			XElement regularPackagesElement = new XElement("regularpackages");
			contentPackagesElement.Add(regularPackagesElement);
			foreach (RegularPackage regularPackage in ContentPackageManager.EnabledPackages.Regular)
			{
				XComment packageComment = new XComment(regularPackage.Name);
				regularPackagesElement.Add(packageComment);
				XElement packageElement = new XElement("package");
				regularPackagesElement.Add(packageElement);
				packageElement.SetAttributeValue("path", regularPackage.Path);
			}
			configDoc.SaveSafe("config_player.xml", SaveOptions.None, false, 4);
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				OmitXmlDeclaration = true,
				NewLineOnAttributes = true
			};
			try
			{
				using (XmlWriter writer = XmlWriter.Create("config_player.xml", settings))
				{
					configDoc.WriteTo(writer);
					writer.Flush();
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Saving game settings failed.", e, null, false, false);
				GameAnalyticsManager.AddErrorEventOnce("GameSettings.Save:SaveFailed", GameAnalyticsManager.ErrorSeverity.Error, "Saving game settings failed.\n" + e.Message + "\n" + e.StackTrace.CleanupStackTrace());
			}
		}

		// Token: 0x0400166C RID: 5740
		public const string PlayerConfigPath = "config_player.xml";

		// Token: 0x0400166D RID: 5741
		private static GameSettings.Config currentConfig;

		// Token: 0x02000AFE RID: 2814
		[Nullable(0)]
		public struct Config
		{
			// Token: 0x06005F2C RID: 24364 RVA: 0x00206934 File Offset: 0x00204B34
			public static GameSettings.Config GetDefault()
			{
				return new GameSettings.Config
				{
					Language = TextManager.DefaultLanguage,
					SubEditorUndoBuffer = 32,
					MaxAutoSaves = 8,
					AutoSaveIntervalSeconds = 300,
					SubEditorBackground = new Color(13, 37, 69, 255),
					EnableSplashScreen = true,
					PauseOnFocusLost = true,
					RemoteMainMenuContentUrl = "https://www.barotraumagame.com/gamedata/",
					RemoteContentTimeoutSeconds = 15f,
					AimAssistAmount = 0.05f,
					ShowEnemyHealthBars = EnemyHealthBarMode.ShowAll,
					ChatSpeechBubbles = true,
					InteractionLabelDisplayMode = InteractionLabelDisplayMode.Everything,
					EnableMouseLook = true,
					ChatOpen = true,
					CrewMenuOpen = true,
					ShowOffensiveServerPrompt = true,
					TutorialSkipWarning = true,
					CorpseDespawnDelay = 600,
					CorpseDespawnDelayPvP = 60,
					CorpsesPerSubDespawnThreshold = 5,
					UseDualModeSockets = true,
					DisableInGameHints = false,
					EnableSubmarineAutoSave = true,
					Graphics = GameSettings.Config.GraphicsSettings.GetDefault(),
					Audio = GameSettings.Config.AudioSettings.GetDefault()
				};
			}

			// Token: 0x06005F2D RID: 24365 RVA: 0x00206A50 File Offset: 0x00204C50
			public static GameSettings.Config FromElement(XElement element, in GameSettings.Config? fallback = null)
			{
				GameSettings.Config retVal = fallback ?? GameSettings.Config.GetDefault();
				ref retVal.DeserializeElement(element);
				if (retVal.Language == LanguageIdentifier.None)
				{
					retVal.Language = TextManager.DefaultLanguage;
				}
				XAttribute attribute = element.GetAttribute("RemoteMainMenuContentUrl", StringComparison.OrdinalIgnoreCase);
				if (((attribute != null) ? attribute.Value : null) == string.Empty)
				{
					retVal.RemoteMainMenuContentUrl = string.Empty;
				}
				IEnumerable<XElement> childElements = element.GetChildElements(new string[]
				{
					"graphicsmode",
					"graphicssettings"
				});
				GameSettings.Config.GraphicsSettings? graphicsSettings = new GameSettings.Config.GraphicsSettings?(retVal.Graphics);
				retVal.Graphics = GameSettings.Config.GraphicsSettings.FromElements(childElements, graphicsSettings);
				IEnumerable<XElement> childElements2 = element.GetChildElements("audio", StringComparison.OrdinalIgnoreCase);
				GameSettings.Config.AudioSettings? audioSettings = new GameSettings.Config.AudioSettings?(retVal.Audio);
				retVal.Audio = GameSettings.Config.AudioSettings.FromElements(childElements2, audioSettings);
				return retVal;
			}

			// Token: 0x170015B0 RID: 5552
			// (get) Token: 0x06005F2E RID: 24366 RVA: 0x00206B2F File Offset: 0x00204D2F
			public readonly int RemoteContentTimeoutMs
			{
				get
				{
					return (int)(this.RemoteContentTimeoutSeconds * 1000f);
				}
			}

			// Token: 0x040037E7 RID: 14311
			public const float DefaultAimAssist = 0.05f;

			// Token: 0x040037E8 RID: 14312
			public LanguageIdentifier Language;

			// Token: 0x040037E9 RID: 14313
			public bool VerboseLogging;

			// Token: 0x040037EA RID: 14314
			public bool SaveDebugConsoleLogs;

			// Token: 0x040037EB RID: 14315
			public string SavePath;

			// Token: 0x040037EC RID: 14316
			public int SubEditorUndoBuffer;

			// Token: 0x040037ED RID: 14317
			public int MaxAutoSaves;

			// Token: 0x040037EE RID: 14318
			public int AutoSaveIntervalSeconds;

			// Token: 0x040037EF RID: 14319
			public Color SubEditorBackground;

			// Token: 0x040037F0 RID: 14320
			public bool EnableSplashScreen;

			// Token: 0x040037F1 RID: 14321
			public bool PauseOnFocusLost;

			// Token: 0x040037F2 RID: 14322
			public float AimAssistAmount;

			// Token: 0x040037F3 RID: 14323
			public bool EnableMouseLook;

			// Token: 0x040037F4 RID: 14324
			public EnemyHealthBarMode ShowEnemyHealthBars;

			// Token: 0x040037F5 RID: 14325
			public bool ChatSpeechBubbles;

			// Token: 0x040037F6 RID: 14326
			public InteractionLabelDisplayMode InteractionLabelDisplayMode;

			// Token: 0x040037F7 RID: 14327
			public bool ChatOpen;

			// Token: 0x040037F8 RID: 14328
			public bool CrewMenuOpen;

			// Token: 0x040037F9 RID: 14329
			public bool ShowOffensiveServerPrompt;

			// Token: 0x040037FA RID: 14330
			public bool TutorialSkipWarning;

			// Token: 0x040037FB RID: 14331
			public int CorpseDespawnDelay;

			// Token: 0x040037FC RID: 14332
			public int CorpseDespawnDelayPvP;

			// Token: 0x040037FD RID: 14333
			public int CorpsesPerSubDespawnThreshold;

			// Token: 0x040037FE RID: 14334
			public bool UseDualModeSockets;

			// Token: 0x040037FF RID: 14335
			public bool DisableInGameHints;

			// Token: 0x04003800 RID: 14336
			public bool EnableSubmarineAutoSave;

			// Token: 0x04003801 RID: 14337
			public Identifier QuickStartSub;

			// Token: 0x04003802 RID: 14338
			public string RemoteMainMenuContentUrl;

			// Token: 0x04003803 RID: 14339
			public float RemoteContentTimeoutSeconds;

			// Token: 0x04003804 RID: 14340
			[StructSerialization.SkipAttribute]
			public GameSettings.Config.GraphicsSettings Graphics;

			// Token: 0x04003805 RID: 14341
			[StructSerialization.SkipAttribute]
			public GameSettings.Config.AudioSettings Audio;

			// Token: 0x02000EC2 RID: 3778
			[NullableContext(0)]
			public struct GraphicsSettings
			{
				// Token: 0x06006B05 RID: 27397 RVA: 0x00227794 File Offset: 0x00225994
				public static GameSettings.Config.GraphicsSettings GetDefault()
				{
					GameSettings.Config.GraphicsSettings gfxSettings = new GameSettings.Config.GraphicsSettings
					{
						Display = 0,
						RadialDistortion = true,
						InventoryScale = 1f,
						LightMapScale = 1f,
						VisibleLightLimit = 100,
						TextScale = 1f,
						HUDScale = 1f,
						Specularity = true,
						ChromaticAberration = true,
						ParticleLimit = 1500,
						LosMode = LosMode.Transparent
					};
					gfxSettings.RadialDistortion = true;
					gfxSettings.CompressTextures = true;
					gfxSettings.FrameLimit = 300;
					gfxSettings.VSync = true;
					gfxSettings.DisplayMode = WindowMode.BorderlessWindowed;
					return gfxSettings;
				}

				// Token: 0x06006B06 RID: 27398 RVA: 0x00227848 File Offset: 0x00225A48
				[NullableContext(1)]
				public static GameSettings.Config.GraphicsSettings FromElements(IEnumerable<XElement> elements, in GameSettings.Config.GraphicsSettings? fallback = null)
				{
					GameSettings.Config.GraphicsSettings retVal = fallback ?? GameSettings.Config.GraphicsSettings.GetDefault();
					elements.ForEach(delegate(XElement element)
					{
						ref retVal.DeserializeElement(element);
					});
					return retVal;
				}

				// Token: 0x0400433E RID: 17214
				public static readonly Point MinSupportedResolution = new Point(1024, 540);

				// Token: 0x0400433F RID: 17215
				public int Display;

				// Token: 0x04004340 RID: 17216
				public int Width;

				// Token: 0x04004341 RID: 17217
				public int Height;

				// Token: 0x04004342 RID: 17218
				public bool VSync;

				// Token: 0x04004343 RID: 17219
				public bool CompressTextures;

				// Token: 0x04004344 RID: 17220
				public int FrameLimit;

				// Token: 0x04004345 RID: 17221
				public WindowMode DisplayMode;

				// Token: 0x04004346 RID: 17222
				public int ParticleLimit;

				// Token: 0x04004347 RID: 17223
				public bool Specularity;

				// Token: 0x04004348 RID: 17224
				public bool ChromaticAberration;

				// Token: 0x04004349 RID: 17225
				public LosMode LosMode;

				// Token: 0x0400434A RID: 17226
				public float HUDScale;

				// Token: 0x0400434B RID: 17227
				public float InventoryScale;

				// Token: 0x0400434C RID: 17228
				public float LightMapScale;

				// Token: 0x0400434D RID: 17229
				public int VisibleLightLimit;

				// Token: 0x0400434E RID: 17230
				public float TextScale;

				// Token: 0x0400434F RID: 17231
				public bool RadialDistortion;
			}

			// Token: 0x02000EC3 RID: 3779
			[Nullable(0)]
			public struct AudioSettings
			{
				// Token: 0x06006B08 RID: 27400 RVA: 0x002278B0 File Offset: 0x00225AB0
				public static GameSettings.Config.AudioSettings GetDefault()
				{
					return new GameSettings.Config.AudioSettings
					{
						MusicVolume = 0.3f,
						SoundVolume = 0.5f,
						UiVolume = 0.3f,
						VoiceChatVolume = 0.5f,
						VoiceChatCutoffPrevention = 200,
						MicrophoneVolume = 5f,
						MuteOnFocusLost = false,
						DynamicRangeCompressionEnabled = true,
						UseDirectionalVoiceChat = true,
						VoipAttenuationEnabled = true,
						VoiceSetting = VoiceMode.PushToTalk,
						DisableVoiceChatFilters = false
					};
				}

				// Token: 0x06006B09 RID: 27401 RVA: 0x00227940 File Offset: 0x00225B40
				public static GameSettings.Config.AudioSettings FromElements(IEnumerable<XElement> elements, in GameSettings.Config.AudioSettings? fallback = null)
				{
					GameSettings.Config.AudioSettings retVal = fallback ?? GameSettings.Config.AudioSettings.GetDefault();
					elements.ForEach(delegate(XElement element)
					{
						ref retVal.DeserializeElement(element);
					});
					return retVal;
				}

				// Token: 0x04004350 RID: 17232
				public float MusicVolume;

				// Token: 0x04004351 RID: 17233
				public float SoundVolume;

				// Token: 0x04004352 RID: 17234
				public float UiVolume;

				// Token: 0x04004353 RID: 17235
				public float VoiceChatVolume;

				// Token: 0x04004354 RID: 17236
				public int VoiceChatCutoffPrevention;

				// Token: 0x04004355 RID: 17237
				public float MicrophoneVolume;

				// Token: 0x04004356 RID: 17238
				public bool MuteOnFocusLost;

				// Token: 0x04004357 RID: 17239
				public bool DynamicRangeCompressionEnabled;

				// Token: 0x04004358 RID: 17240
				public bool UseDirectionalVoiceChat;

				// Token: 0x04004359 RID: 17241
				public bool VoipAttenuationEnabled;

				// Token: 0x0400435A RID: 17242
				public VoiceMode VoiceSetting;

				// Token: 0x0400435B RID: 17243
				[StructSerialization.HandlerAttribute(typeof(GameSettings.Config.AudioSettings.DeviceNameHandler))]
				public string AudioOutputDevice;

				// Token: 0x0400435C RID: 17244
				[StructSerialization.HandlerAttribute(typeof(GameSettings.Config.AudioSettings.DeviceNameHandler))]
				public string VoiceCaptureDevice;

				// Token: 0x0400435D RID: 17245
				public float NoiseGateThreshold;

				// Token: 0x0400435E RID: 17246
				public bool DisableVoiceChatFilters;

				// Token: 0x02000F01 RID: 3841
				[Nullable(0)]
				public static class DeviceNameHandler
				{
					// Token: 0x06006B95 RID: 27541 RVA: 0x0022AB53 File Offset: 0x00228D53
					public static string Read(string s)
					{
						return XmlConvert.DecodeName(s);
					}

					// Token: 0x06006B96 RID: 27542 RVA: 0x0022AB5B File Offset: 0x00228D5B
					public static string Write(string s)
					{
						return XmlConvert.EncodeName(s);
					}
				}
			}
		}
	}
}
