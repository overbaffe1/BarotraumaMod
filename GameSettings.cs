using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.ClientSource.Settings;
using Barotrauma.Eos;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Networking;
using Barotrauma.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x0200035C RID: 860
	[NullableContext(1)]
	[Nullable(0)]
	public static class GameSettings
	{
		// Token: 0x17001193 RID: 4499
		// (get) Token: 0x060042E6 RID: 17126 RVA: 0x0024FB36 File Offset: 0x0024DD36
		public static ref readonly GameSettings.Config CurrentConfig
		{
			get
			{
				return ref GameSettings.currentConfig;
			}
		}

		// Token: 0x060042E7 RID: 17127 RVA: 0x0024FB40 File Offset: 0x0024DD40
		public static void Init()
		{
			SaveUtil.EnsureSaveFolderExists();
			XDocument currentConfigDoc = null;
			if (File.Exists("config_player.xml"))
			{
				currentConfigDoc = XMLExtensions.TryLoadXml("config_player.xml");
			}
			if (currentConfigDoc != null)
			{
				XElement root = currentConfigDoc.Root;
				if (root == null)
				{
					throw new NullReferenceException("Config XML element is invalid: document is null.");
				}
				GameSettings.Config? config = null;
				GameSettings.currentConfig = GameSettings.Config.FromElement(root, config);
				MainMenuScreen.DismissedNotifications = currentConfigDoc.Root.GetAttributeIdentifierArray("DismissedNotifications", Array.Empty<Identifier>(), true).ToHashSet<Identifier>();
				ServerListFilters.Init(currentConfigDoc.Root.GetChildElement("serverfilters", StringComparison.OrdinalIgnoreCase));
				XElement[] array = new XElement[2];
				array[0] = currentConfigDoc.Root.GetChildElement("player", StringComparison.OrdinalIgnoreCase);
				int num = 1;
				XElement childElement = currentConfigDoc.Root.GetChildElement("gameplay", StringComparison.OrdinalIgnoreCase);
				array[num] = ((childElement != null) ? childElement.GetChildElement("jobpreferences", StringComparison.OrdinalIgnoreCase) : null);
				MultiplayerPreferences.Init(array);
				IgnoredHints.Init(currentConfigDoc.Root.GetChildElement("ignoredhints", StringComparison.OrdinalIgnoreCase));
				DebugConsoleMapping.Init(currentConfigDoc.Root.GetChildElement("debugconsolemapping", StringComparison.OrdinalIgnoreCase));
				CompletedTutorials.Init(currentConfigDoc.Root.GetChildElement("tutorials", StringComparison.OrdinalIgnoreCase));
				XElement submarineSettings = currentConfigDoc.Root.GetChildElement("submarinesettings", StringComparison.OrdinalIgnoreCase);
				if (submarineSettings == null)
				{
					return;
				}
				SubmarineInfo.SubmarinePathsWithRemoteStorage.Clear();
				using (IEnumerator<XElement> enumerator = submarineSettings.Elements("SubmarineWithRemoteStorage").GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						XElement subElement = enumerator.Current;
						string path = subElement.GetAttributeString("path", "");
						if (!path.IsNullOrEmpty())
						{
							SubmarineInfo.SubmarinePathsWithRemoteStorage.Add(path);
						}
					}
					return;
				}
			}
			GameSettings.currentConfig = GameSettings.Config.GetDefault();
			GameSettings.SaveCurrentConfig();
		}

		// Token: 0x060042E8 RID: 17128 RVA: 0x0024FCF4 File Offset: 0x0024DEF4
		public static void SetCurrentConfig(in GameSettings.Config newConfig)
		{
			bool resolutionChanged = GameSettings.currentConfig.Graphics.Width != newConfig.Graphics.Width || GameSettings.currentConfig.Graphics.Height != newConfig.Graphics.Height;
			bool languageChanged = GameSettings.currentConfig.Language != newConfig.Language;
			bool audioOutputChanged = GameSettings.currentConfig.Audio.AudioOutputDevice != newConfig.Audio.AudioOutputDevice;
			bool voiceCaptureChanged = GameSettings.currentConfig.Audio.VoiceCaptureDevice != newConfig.Audio.VoiceCaptureDevice;
			bool textScaleChanged = Math.Abs(GameSettings.currentConfig.Graphics.TextScale - newConfig.Graphics.TextScale) > MathF.Pow(2f, -7f);
			bool hudScaleChanged = !MathUtils.NearlyEqual(GameSettings.currentConfig.Graphics.HUDScale, newConfig.Graphics.HUDScale, 0.0001f);
			bool setGraphicsMode = resolutionChanged || GameSettings.currentConfig.Graphics.VSync != newConfig.Graphics.VSync || GameSettings.currentConfig.Graphics.DisplayMode != newConfig.Graphics.DisplayMode || GameSettings.currentConfig.Graphics.Display != newConfig.Graphics.Display;
			bool keybindsChanged = false;
			foreach (KeyValuePair<InputType, KeyOrMouse> kvp in newConfig.KeyMap.Bindings)
			{
				KeyOrMouse existingBinding;
				if (!GameSettings.currentConfig.KeyMap.Bindings.TryGetValue(kvp.Key, out existingBinding) || existingBinding != kvp.Value)
				{
					keybindsChanged = true;
					break;
				}
			}
			GameSettings.currentConfig = newConfig;
			if (setGraphicsMode)
			{
				GameMain.Instance.ApplyGraphicsSettings(true);
			}
			else if (textScaleChanged)
			{
				GUIStyle.RecalculateFonts();
			}
			if (audioOutputChanged)
			{
				SoundManager soundManager = GameMain.SoundManager;
				if (soundManager != null)
				{
					soundManager.InitializeAlcDevice(GameSettings.currentConfig.Audio.AudioOutputDevice);
				}
			}
			if (voiceCaptureChanged)
			{
				VoipCapture.ChangeCaptureDevice(GameSettings.currentConfig.Audio.VoiceCaptureDevice);
			}
			if (hudScaleChanged)
			{
				HUDLayoutSettings.CreateAreas();
				GameSession gameSession = GameMain.GameSession;
				if (gameSession != null)
				{
					gameSession.HUDScaleChanged();
				}
			}
			if (keybindsChanged)
			{
				foreach (Item item in Item.ItemList)
				{
					foreach (ItemComponent ic in item.Components)
					{
						ic.ParseMsg();
					}
				}
			}
			SoundManager soundManager2 = GameMain.SoundManager;
			if (soundManager2 != null)
			{
				soundManager2.ApplySettings();
			}
			if (languageChanged)
			{
				TextManager.LanguageChanged();
			}
		}

		// Token: 0x060042E9 RID: 17129 RVA: 0x0024FFE4 File Offset: 0x0024E1E4
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
			root.Add(new XAttribute("DismissedNotifications", string.Join<string>(',', from n in MainMenuScreen.DismissedNotifications
			select n.Value)));
			XElement serverFiltersElement = new XElement("serverfilters");
			root.Add(serverFiltersElement);
			ServerListFilters.Instance.SaveTo(serverFiltersElement);
			XElement characterElement = new XElement("player");
			root.Add(characterElement);
			MultiplayerPreferences.Instance.SaveTo(characterElement);
			XElement ignoredHintsElement = new XElement("ignoredhints");
			root.Add(ignoredHintsElement);
			IgnoredHints.Instance.SaveTo(ignoredHintsElement);
			XElement debugConsoleMappingElement = new XElement("debugconsolemapping");
			root.Add(debugConsoleMappingElement);
			DebugConsoleMapping.Instance.SaveTo(debugConsoleMappingElement);
			XElement tutorialsElement = new XElement("tutorials");
			root.Add(tutorialsElement);
			CompletedTutorials.Instance.SaveTo(tutorialsElement);
			XElement submarineSettings = new XElement("submarinesettings");
			root.Add(submarineSettings);
			SubmarineInfo.SubmarinePathsWithRemoteStorage.ForEach(delegate(string path)
			{
				submarineSettings.Add(new XElement("SubmarineWithRemoteStorage", new XAttribute("path", path)));
			});
			XElement keyMappingElement = new XElement("keymapping", from kvp in GameSettings.currentConfig.KeyMap.Bindings
			select new XAttribute(kvp.Key.ToString(), kvp.Value.ToString()));
			root.Add(keyMappingElement);
			XElement inventoryKeyMappingElement = new XElement("inventorykeymapping", from ValueTuple<int, KeyOrMouse> kvp in Enumerable.Range(0, GameSettings.currentConfig.InventoryKeyMap.Bindings.Length).Zip(GameSettings.currentConfig.InventoryKeyMap.Bindings)
			select new XAttribute("slot" + kvp.Item1.ToString(CultureInfo.InvariantCulture), kvp.Item2.ToString()));
			root.Add(inventoryKeyMappingElement);
			SubEditorScreen.ImageManager.Save(root);
			root.Add(CampaignSettings.CurrentSettings.Save());
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

		// Token: 0x060042EA RID: 17130 RVA: 0x0025045C File Offset: 0x0024E65C
		private static void LoadSubEditorImages(XElement configElement)
		{
			XElement element = (configElement != null) ? configElement.Element("editorimages") : null;
			if (element == null)
			{
				SubEditorScreen.ImageManager.Clear(true);
				return;
			}
			SubEditorScreen.ImageManager.Load(element);
		}

		// Token: 0x040022B0 RID: 8880
		public const string PlayerConfigPath = "config_player.xml";

		// Token: 0x040022B1 RID: 8881
		private static GameSettings.Config currentConfig;

		// Token: 0x040022B2 RID: 8882
		[Nullable(2)]
		public static Action OnGameMainHasLoaded;

		// Token: 0x02001076 RID: 4214
		[Nullable(0)]
		public struct Config
		{
			// Token: 0x06008CBA RID: 36026 RVA: 0x003B014C File Offset: 0x003AE34C
			public static GameSettings.Config GetDefault()
			{
				return new GameSettings.Config
				{
					Language = LanguageIdentifier.None,
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
					Audio = GameSettings.Config.AudioSettings.GetDefault(),
					CrossplayChoice = EosSteamPrimaryLogin.CrossplayChoice.Unknown,
					DisableGlobalSpamList = false,
					KeyMap = GameSettings.Config.KeyMapping.GetDefault(),
					InventoryKeyMap = GameSettings.Config.InventoryKeyMapping.GetDefault()
				};
			}

			// Token: 0x06008CBB RID: 36027 RVA: 0x003B0290 File Offset: 0x003AE490
			public static GameSettings.Config FromElement(XElement element, in GameSettings.Config? fallback = null)
			{
				GameSettings.Config retVal = fallback ?? GameSettings.Config.GetDefault();
				ref retVal.DeserializeElement(element);
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
				IEnumerable<XElement> childElements3 = element.GetChildElements("keymapping", StringComparison.OrdinalIgnoreCase);
				GameSettings.Config.KeyMapping? keyMapping = new GameSettings.Config.KeyMapping?(retVal.KeyMap);
				retVal.KeyMap = new GameSettings.Config.KeyMapping(childElements3, ref keyMapping);
				retVal.InventoryKeyMap = new GameSettings.Config.InventoryKeyMapping(element.GetChildElements("inventorykeymapping", StringComparison.OrdinalIgnoreCase), new GameSettings.Config.InventoryKeyMapping?(retVal.InventoryKeyMap));
				retVal.SavedCampaignSettings = element.GetChildElement("campaignsettings", StringComparison.OrdinalIgnoreCase);
				GameSettings.LoadSubEditorImages(element);
				return retVal;
			}

			// Token: 0x17001C6D RID: 7277
			// (get) Token: 0x06008CBC RID: 36028 RVA: 0x003B03B4 File Offset: 0x003AE5B4
			public readonly int RemoteContentTimeoutMs
			{
				get
				{
					return (int)(this.RemoteContentTimeoutSeconds * 1000f);
				}
			}

			// Token: 0x0400588E RID: 22670
			public const float DefaultAimAssist = 0.05f;

			// Token: 0x0400588F RID: 22671
			public LanguageIdentifier Language;

			// Token: 0x04005890 RID: 22672
			public bool VerboseLogging;

			// Token: 0x04005891 RID: 22673
			public bool SaveDebugConsoleLogs;

			// Token: 0x04005892 RID: 22674
			public string SavePath;

			// Token: 0x04005893 RID: 22675
			public int SubEditorUndoBuffer;

			// Token: 0x04005894 RID: 22676
			public int MaxAutoSaves;

			// Token: 0x04005895 RID: 22677
			public int AutoSaveIntervalSeconds;

			// Token: 0x04005896 RID: 22678
			public Color SubEditorBackground;

			// Token: 0x04005897 RID: 22679
			public bool EnableSplashScreen;

			// Token: 0x04005898 RID: 22680
			public bool PauseOnFocusLost;

			// Token: 0x04005899 RID: 22681
			public float AimAssistAmount;

			// Token: 0x0400589A RID: 22682
			public bool EnableMouseLook;

			// Token: 0x0400589B RID: 22683
			public EnemyHealthBarMode ShowEnemyHealthBars;

			// Token: 0x0400589C RID: 22684
			public bool ChatSpeechBubbles;

			// Token: 0x0400589D RID: 22685
			public InteractionLabelDisplayMode InteractionLabelDisplayMode;

			// Token: 0x0400589E RID: 22686
			public bool ChatOpen;

			// Token: 0x0400589F RID: 22687
			public bool CrewMenuOpen;

			// Token: 0x040058A0 RID: 22688
			public bool ShowOffensiveServerPrompt;

			// Token: 0x040058A1 RID: 22689
			public bool TutorialSkipWarning;

			// Token: 0x040058A2 RID: 22690
			public int CorpseDespawnDelay;

			// Token: 0x040058A3 RID: 22691
			public int CorpseDespawnDelayPvP;

			// Token: 0x040058A4 RID: 22692
			public int CorpsesPerSubDespawnThreshold;

			// Token: 0x040058A5 RID: 22693
			public bool UseDualModeSockets;

			// Token: 0x040058A6 RID: 22694
			public bool DisableInGameHints;

			// Token: 0x040058A7 RID: 22695
			public bool EnableSubmarineAutoSave;

			// Token: 0x040058A8 RID: 22696
			public Identifier QuickStartSub;

			// Token: 0x040058A9 RID: 22697
			public string RemoteMainMenuContentUrl;

			// Token: 0x040058AA RID: 22698
			public float RemoteContentTimeoutSeconds;

			// Token: 0x040058AB RID: 22699
			public EosSteamPrimaryLogin.CrossplayChoice CrossplayChoice;

			// Token: 0x040058AC RID: 22700
			public XElement SavedCampaignSettings;

			// Token: 0x040058AD RID: 22701
			public bool DisableGlobalSpamList;

			// Token: 0x040058AE RID: 22702
			[StructSerialization.SkipAttribute]
			public GameSettings.Config.GraphicsSettings Graphics;

			// Token: 0x040058AF RID: 22703
			[StructSerialization.SkipAttribute]
			public GameSettings.Config.AudioSettings Audio;

			// Token: 0x040058B0 RID: 22704
			[StructSerialization.SkipAttribute]
			public GameSettings.Config.KeyMapping KeyMap;

			// Token: 0x040058B1 RID: 22705
			[StructSerialization.SkipAttribute]
			public GameSettings.Config.InventoryKeyMapping InventoryKeyMap;

			// Token: 0x02001582 RID: 5506
			[NullableContext(0)]
			public struct GraphicsSettings
			{
				// Token: 0x06009E21 RID: 40481 RVA: 0x003EE2F8 File Offset: 0x003EC4F8
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

				// Token: 0x06009E22 RID: 40482 RVA: 0x003EE3AC File Offset: 0x003EC5AC
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

				// Token: 0x040068A2 RID: 26786
				public static readonly Point MinSupportedResolution = new Point(1024, 540);

				// Token: 0x040068A3 RID: 26787
				public int Display;

				// Token: 0x040068A4 RID: 26788
				public int Width;

				// Token: 0x040068A5 RID: 26789
				public int Height;

				// Token: 0x040068A6 RID: 26790
				public bool VSync;

				// Token: 0x040068A7 RID: 26791
				public bool CompressTextures;

				// Token: 0x040068A8 RID: 26792
				public int FrameLimit;

				// Token: 0x040068A9 RID: 26793
				public WindowMode DisplayMode;

				// Token: 0x040068AA RID: 26794
				public int ParticleLimit;

				// Token: 0x040068AB RID: 26795
				public bool Specularity;

				// Token: 0x040068AC RID: 26796
				public bool ChromaticAberration;

				// Token: 0x040068AD RID: 26797
				public LosMode LosMode;

				// Token: 0x040068AE RID: 26798
				public float HUDScale;

				// Token: 0x040068AF RID: 26799
				public float InventoryScale;

				// Token: 0x040068B0 RID: 26800
				public float LightMapScale;

				// Token: 0x040068B1 RID: 26801
				public int VisibleLightLimit;

				// Token: 0x040068B2 RID: 26802
				public float TextScale;

				// Token: 0x040068B3 RID: 26803
				public bool RadialDistortion;
			}

			// Token: 0x02001583 RID: 5507
			[Nullable(0)]
			public struct AudioSettings
			{
				// Token: 0x06009E24 RID: 40484 RVA: 0x003EE414 File Offset: 0x003EC614
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

				// Token: 0x06009E25 RID: 40485 RVA: 0x003EE4A4 File Offset: 0x003EC6A4
				public static GameSettings.Config.AudioSettings FromElements(IEnumerable<XElement> elements, in GameSettings.Config.AudioSettings? fallback = null)
				{
					GameSettings.Config.AudioSettings retVal = fallback ?? GameSettings.Config.AudioSettings.GetDefault();
					elements.ForEach(delegate(XElement element)
					{
						ref retVal.DeserializeElement(element);
					});
					return retVal;
				}

				// Token: 0x040068B4 RID: 26804
				public float MusicVolume;

				// Token: 0x040068B5 RID: 26805
				public float SoundVolume;

				// Token: 0x040068B6 RID: 26806
				public float UiVolume;

				// Token: 0x040068B7 RID: 26807
				public float VoiceChatVolume;

				// Token: 0x040068B8 RID: 26808
				public int VoiceChatCutoffPrevention;

				// Token: 0x040068B9 RID: 26809
				public float MicrophoneVolume;

				// Token: 0x040068BA RID: 26810
				public bool MuteOnFocusLost;

				// Token: 0x040068BB RID: 26811
				public bool DynamicRangeCompressionEnabled;

				// Token: 0x040068BC RID: 26812
				public bool UseDirectionalVoiceChat;

				// Token: 0x040068BD RID: 26813
				public bool VoipAttenuationEnabled;

				// Token: 0x040068BE RID: 26814
				public VoiceMode VoiceSetting;

				// Token: 0x040068BF RID: 26815
				[StructSerialization.HandlerAttribute(typeof(GameSettings.Config.AudioSettings.DeviceNameHandler))]
				public string AudioOutputDevice;

				// Token: 0x040068C0 RID: 26816
				[StructSerialization.HandlerAttribute(typeof(GameSettings.Config.AudioSettings.DeviceNameHandler))]
				public string VoiceCaptureDevice;

				// Token: 0x040068C1 RID: 26817
				public float NoiseGateThreshold;

				// Token: 0x040068C2 RID: 26818
				public bool DisableVoiceChatFilters;

				// Token: 0x020015E3 RID: 5603
				[Nullable(0)]
				public static class DeviceNameHandler
				{
					// Token: 0x06009F42 RID: 40770 RVA: 0x003F49C8 File Offset: 0x003F2BC8
					public static string Read(string s)
					{
						return XmlConvert.DecodeName(s);
					}

					// Token: 0x06009F43 RID: 40771 RVA: 0x003F49D0 File Offset: 0x003F2BD0
					public static string Write(string s)
					{
						return XmlConvert.EncodeName(s);
					}
				}
			}

			// Token: 0x02001584 RID: 5508
			[Nullable(0)]
			public struct KeyMapping
			{
				// Token: 0x06009E26 RID: 40486 RVA: 0x003EE4F4 File Offset: 0x003EC6F4
				public static GameSettings.Config.KeyMapping GetDefault()
				{
					GameSettings.Config.KeyMapping result = default(GameSettings.Config.KeyMapping);
					result.Bindings = (from kvp in GameSettings.Config.KeyMapping.DefaultsQwerty
					select new ValueTuple<InputType, KeyOrMouse>(kvp.Key, (kvp.Value.MouseButton == MouseButton.None) ? Keyboard.QwertyToCurrentLayout(kvp.Value.Key) : kvp.Value.MouseButton)).ToImmutableDictionary<InputType, KeyOrMouse>();
					return result;
				}

				// Token: 0x06009E27 RID: 40487 RVA: 0x003EE540 File Offset: 0x003EC740
				public KeyMapping(IEnumerable<XElement> elements, in GameSettings.Config.KeyMapping? fallback)
				{
					ImmutableDictionary<InputType, KeyOrMouse> defaultBindings = GameSettings.Config.KeyMapping.GetDefault().Bindings;
					Dictionary<InputType, KeyOrMouse> dictionary;
					if (fallback == null)
					{
						dictionary = null;
					}
					else
					{
						ImmutableDictionary<InputType, KeyOrMouse> bindings2 = fallback.GetValueOrDefault().Bindings;
						dictionary = ((bindings2 != null) ? bindings2.ToMutable<InputType, KeyOrMouse>() : null);
					}
					Dictionary<InputType, KeyOrMouse> bindings = dictionary ?? defaultBindings.ToMutable<InputType, KeyOrMouse>();
					foreach (InputType inputType in (InputType[])Enum.GetValues(typeof(InputType)))
					{
						if (!bindings.ContainsKey(inputType))
						{
							bindings.Add(inputType, defaultBindings[inputType]);
						}
					}
					Dictionary<InputType, KeyOrMouse> savedBindings = new Dictionary<InputType, KeyOrMouse>();
					bool playerConfigContainsNewChatBinds = false;
					bool playerConfigContainsRestoredVoipBinds = false;
					foreach (XElement element in elements)
					{
						foreach (XAttribute attribute in element.Attributes())
						{
							InputType result;
							if (Enum.TryParse<InputType>(attribute.Name.LocalName, out result))
							{
								playerConfigContainsNewChatBinds |= (result == InputType.ActiveChat);
								playerConfigContainsRestoredVoipBinds |= (result == InputType.RadioVoice);
								KeyOrMouse keyOrMouse = element.GetAttributeKeyOrMouse(attribute.Name.LocalName, bindings[result]);
								savedBindings.Add(result, keyOrMouse);
								bindings[result] = keyOrMouse;
							}
						}
					}
					using (ImmutableDictionary<InputType, KeyOrMouse>.Enumerator enumerator3 = defaultBindings.GetEnumerator())
					{
						while (enumerator3.MoveNext())
						{
							KeyValuePair<InputType, KeyOrMouse> defaultBinding = enumerator3.Current;
							if (!GameSettings.Config.KeyMapping.<.ctor>g__IsSetToNone|2_0(defaultBinding.Value) && !savedBindings.ContainsKey(defaultBinding.Key))
							{
								using (Dictionary<InputType, KeyOrMouse>.Enumerator enumerator4 = savedBindings.GetEnumerator())
								{
									while (enumerator4.MoveNext())
									{
										KeyValuePair<InputType, KeyOrMouse> savedBinding = enumerator4.Current;
										InputType key = savedBinding.Key;
										bool flag = key == InputType.Run || key == InputType.TakeHalfFromInventorySlot;
										if ((!flag || defaultBinding.Key != InputType.ContextualCommand) && savedBinding.Value == defaultBinding.Value)
										{
											GameSettings.OnGameMainHasLoaded = (Action)Delegate.Combine(GameSettings.OnGameMainHasLoaded, new Action(delegate()
											{
												ValueTuple<string, string>[] array2 = new ValueTuple<string, string>[3];
												int num = 0;
												string item = "[defaultbind]";
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 1);
												defaultInterpolatedStringHandler.AppendLiteral("\"");
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(10, 1);
												defaultInterpolatedStringHandler2.AppendLiteral("inputtype.");
												defaultInterpolatedStringHandler2.AppendFormatted<InputType>(defaultBinding.Key);
												defaultInterpolatedStringHandler.AppendFormatted<LocalizedString>(TextManager.Get(defaultInterpolatedStringHandler2.ToStringAndClear()));
												defaultInterpolatedStringHandler.AppendLiteral("\"");
												array2[num] = new ValueTuple<string, string>(item, defaultInterpolatedStringHandler.ToStringAndClear());
												int num2 = 1;
												string item2 = "[savedbind]";
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(2, 1);
												defaultInterpolatedStringHandler3.AppendLiteral("\"");
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler4 = new DefaultInterpolatedStringHandler(10, 1);
												defaultInterpolatedStringHandler4.AppendLiteral("inputtype.");
												defaultInterpolatedStringHandler4.AppendFormatted<InputType>(savedBinding.Key);
												defaultInterpolatedStringHandler3.AppendFormatted<LocalizedString>(TextManager.Get(defaultInterpolatedStringHandler4.ToStringAndClear()));
												defaultInterpolatedStringHandler3.AppendLiteral("\"");
												array2[num2] = new ValueTuple<string, string>(item2, defaultInterpolatedStringHandler3.ToStringAndClear());
												int num3 = 2;
												string item3 = "[key]";
												DefaultInterpolatedStringHandler defaultInterpolatedStringHandler5 = new DefaultInterpolatedStringHandler(2, 1);
												defaultInterpolatedStringHandler5.AppendLiteral("\"");
												defaultInterpolatedStringHandler5.AppendFormatted<LocalizedString>(defaultBinding.Value.Name);
												defaultInterpolatedStringHandler5.AppendLiteral("\"");
												array2[num3] = new ValueTuple<string, string>(item3, defaultInterpolatedStringHandler5.ToStringAndClear());
												ValueTuple<string, string>[] replacements = array2;
												new GUIMessageBox(TextManager.Get("warning"), TextManager.GetWithVariables("duplicatebindwarning", replacements), null, null, GUIMessageBox.Type.Default);
											}));
											break;
										}
									}
								}
							}
						}
					}
					if (!playerConfigContainsNewChatBinds)
					{
						bindings[InputType.Chat] = Keys.None;
						bindings[InputType.RadioChat] = Keys.None;
					}
					if (!playerConfigContainsRestoredVoipBinds)
					{
						bindings[InputType.LocalVoice] = Keys.None;
						bindings[InputType.RadioVoice] = Keys.None;
					}
					this.Bindings = bindings.ToImmutableDictionary<InputType, KeyOrMouse>();
				}

				// Token: 0x06009E28 RID: 40488 RVA: 0x003EE878 File Offset: 0x003ECA78
				public GameSettings.Config.KeyMapping WithBinding(InputType type, KeyOrMouse bind)
				{
					GameSettings.Config.KeyMapping newMapping = this;
					newMapping.Bindings = newMapping.Bindings.Select(delegate(KeyValuePair<InputType, KeyOrMouse> kvp)
					{
						if (kvp.Key != type)
						{
							return new ValueTuple<InputType, KeyOrMouse>(kvp.Key, kvp.Value);
						}
						return new ValueTuple<InputType, KeyOrMouse>(type, bind);
					}).ToImmutableDictionary<InputType, KeyOrMouse>();
					return newMapping;
				}

				// Token: 0x06009E29 RID: 40489 RVA: 0x003EE8C4 File Offset: 0x003ECAC4
				public LocalizedString KeyBindText(InputType inputType)
				{
					return this.Bindings[inputType].Name;
				}

				// Token: 0x06009E2B RID: 40491 RVA: 0x003EEB1B File Offset: 0x003ECD1B
				[CompilerGenerated]
				internal static bool <.ctor>g__IsSetToNone|2_0(KeyOrMouse keyOrMouse)
				{
					return keyOrMouse == Keys.None && keyOrMouse == MouseButton.None;
				}

				// Token: 0x040068C3 RID: 26819
				private static readonly ImmutableDictionary<InputType, KeyOrMouse> DefaultsQwerty = new Dictionary<InputType, KeyOrMouse>
				{
					{
						InputType.Run,
						Keys.LeftShift
					},
					{
						InputType.ToggleRun,
						Keys.None
					},
					{
						InputType.Attack,
						Keys.R
					},
					{
						InputType.Crouch,
						Keys.LeftControl
					},
					{
						InputType.Grab,
						Keys.G
					},
					{
						InputType.Health,
						Keys.H
					},
					{
						InputType.Ragdoll,
						Keys.Space
					},
					{
						InputType.Aim,
						MouseButton.SecondaryMouse
					},
					{
						InputType.DropItem,
						Keys.None
					},
					{
						InputType.InfoTab,
						Keys.Tab
					},
					{
						InputType.Chat,
						Keys.None
					},
					{
						InputType.RadioChat,
						Keys.None
					},
					{
						InputType.ActiveChat,
						Keys.T
					},
					{
						InputType.CrewOrders,
						Keys.C
					},
					{
						InputType.ChatBox,
						Keys.B
					},
					{
						InputType.Voice,
						Keys.V
					},
					{
						InputType.RadioVoice,
						Keys.None
					},
					{
						InputType.LocalVoice,
						Keys.None
					},
					{
						InputType.ToggleChatMode,
						Keys.R
					},
					{
						InputType.Command,
						MouseButton.MiddleMouse
					},
					{
						InputType.ContextualCommand,
						Keys.LeftShift
					},
					{
						InputType.PreviousFireMode,
						MouseButton.MouseWheelDown
					},
					{
						InputType.NextFireMode,
						MouseButton.MouseWheelUp
					},
					{
						InputType.TakeHalfFromInventorySlot,
						Keys.LeftShift
					},
					{
						InputType.TakeOneFromInventorySlot,
						Keys.LeftControl
					},
					{
						InputType.Up,
						Keys.W
					},
					{
						InputType.Down,
						Keys.S
					},
					{
						InputType.Left,
						Keys.A
					},
					{
						InputType.Right,
						Keys.D
					},
					{
						InputType.ToggleInventory,
						Keys.Q
					},
					{
						InputType.SelectNextCharacter,
						Keys.Z
					},
					{
						InputType.SelectPreviousCharacter,
						Keys.X
					},
					{
						InputType.Use,
						Keys.E
					},
					{
						InputType.Select,
						MouseButton.PrimaryMouse
					},
					{
						InputType.Deselect,
						MouseButton.SecondaryMouse
					},
					{
						InputType.Shoot,
						MouseButton.PrimaryMouse
					},
					{
						InputType.ShowInteractionLabels,
						Keys.LeftAlt
					}
				}.ToImmutableDictionary<InputType, KeyOrMouse>();

				// Token: 0x040068C4 RID: 26820
				public ImmutableDictionary<InputType, KeyOrMouse> Bindings;
			}

			// Token: 0x02001585 RID: 5509
			[Nullable(0)]
			public struct InventoryKeyMapping
			{
				// Token: 0x06009E2C RID: 40492 RVA: 0x003EEB30 File Offset: 0x003ECD30
				public static GameSettings.Config.InventoryKeyMapping GetDefault()
				{
					return new GameSettings.Config.InventoryKeyMapping
					{
						Bindings = new KeyOrMouse[]
						{
							Keys.D1,
							Keys.D2,
							Keys.D3,
							Keys.D4,
							Keys.D5,
							Keys.D6,
							Keys.D7,
							Keys.D8,
							Keys.D9,
							Keys.D0
						}.ToImmutableArray<KeyOrMouse>()
					};
				}

				// Token: 0x06009E2D RID: 40493 RVA: 0x003EEBC4 File Offset: 0x003ECDC4
				public GameSettings.Config.InventoryKeyMapping WithBinding(int index, KeyOrMouse keyOrMouse)
				{
					ImmutableArray<KeyOrMouse> thisBindings = this.Bindings;
					return new GameSettings.Config.InventoryKeyMapping
					{
						Bindings = Enumerable.Range(0, thisBindings.Length).Select(delegate(int i)
						{
							if (i != index)
							{
								return thisBindings[i];
							}
							return keyOrMouse;
						}).ToImmutableArray<KeyOrMouse>()
					};
				}

				// Token: 0x06009E2E RID: 40494 RVA: 0x003EEC28 File Offset: 0x003ECE28
				public InventoryKeyMapping(IEnumerable<XElement> elements, GameSettings.Config.InventoryKeyMapping? fallback)
				{
					KeyOrMouse[] bindings = ((fallback != null) ? fallback.GetValueOrDefault().Bindings : GameSettings.Config.InventoryKeyMapping.GetDefault().Bindings).ToArray<KeyOrMouse>();
					foreach (XElement element in elements)
					{
						for (int i = 0; i < bindings.Length; i++)
						{
							KeyOrMouse[] array = bindings;
							int num = i;
							XElement element2 = element;
							DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(4, 1);
							defaultInterpolatedStringHandler.AppendLiteral("slot");
							defaultInterpolatedStringHandler.AppendFormatted<int>(i);
							array[num] = element2.GetAttributeKeyOrMouse(defaultInterpolatedStringHandler.ToStringAndClear(), bindings[i]);
						}
					}
					this.Bindings = bindings.ToImmutableArray<KeyOrMouse>();
				}

				// Token: 0x040068C5 RID: 26821
				[Nullable(new byte[]
				{
					0,
					1
				})]
				public ImmutableArray<KeyOrMouse> Bindings;
			}
		}
	}
}
