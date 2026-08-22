using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Items.Components;
using Barotrauma.Lights;
using Barotrauma.RuinGeneration;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000114 RID: 276
	internal class LevelEditorScreen : EditorScreen
	{
		// Token: 0x17000A01 RID: 2561
		// (get) Token: 0x06002546 RID: 9542 RVA: 0x0017B9EF File Offset: 0x00179BEF
		public override Camera Cam { get; }

		// Token: 0x06002547 RID: 9543 RVA: 0x0017B9F8 File Offset: 0x00179BF8
		private void RefreshUI(bool forceCreate = false)
		{
			if (forceCreate)
			{
				this.CreateUI();
			}
			GUI.PreventPauseMenuToggle = false;
			this.pointerLightSource = new LightSource(Vector2.Zero, 1000f, Color.White, null, true);
			GameMain.LightManager.AddLight(this.pointerLightSource);
			this.topPanel.ClearChildren();
			new SerializableEntityEditor(this.topPanel.RectTransform, this.pointerLightSource.LightSourceParams, false, true, "", 24, null, true);
			this.editingSprite = null;
			this.UpdateParamsList();
			this.UpdateRuinParamsList();
			this.UpdateCaveParamsList();
			this.UpdateOutpostParamsList();
			this.UpdateLevelObjectsList();
			this.UpdateBackgroundCreatureList();
		}

		// Token: 0x06002548 RID: 9544 RVA: 0x0017BAA0 File Offset: 0x00179CA0
		private void CreateUI()
		{
			LevelEditorScreen.<>c__DisplayClass35_0 CS$<>8__locals1 = new LevelEditorScreen.<>c__DisplayClass35_0();
			CS$<>8__locals1.<>4__this = this;
			this.Frame.ClearChildren();
			GUIFrame guiframe = this.leftPanel;
			if (guiframe != null)
			{
				guiframe.ClearChildren();
			}
			GUIFrame guiframe2 = this.rightPanel;
			if (guiframe2 != null)
			{
				guiframe2.ClearChildren();
			}
			this.leftPanel = new GUIFrame(new RectTransform(new Vector2(0.125f, 0.8f), this.Frame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(150, 0)
			}, "", null);
			GUILayoutGroup paddedLeftPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.9f, 0.95f), this.leftPanel.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.02f, 0f)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			this.paramsList = new GUIListBox(new RectTransform(new Vector2(1f, 0.3f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true
			};
			GUIListBox guilistBox = this.paramsList;
			guilistBox.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(guilistBox.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
			{
				CS$<>8__locals1.<>4__this.selectedParams = (obj as LevelGenerationParams);
				LevelEditorScreen <>4__this = CS$<>8__locals1.<>4__this;
				string text4 = CS$<>8__locals1.<>4__this.seedBox.Text;
				LevelGenerationParams generationParams = CS$<>8__locals1.<>4__this.selectedParams;
				<>4__this.currentLevelData = LevelData.CreateRandom(text4, null, generationParams, default(Identifier), false, false);
				CS$<>8__locals1.<>4__this.editorContainer.ClearChildren();
				CS$<>8__locals1.<>4__this.SortLevelObjectsList(CS$<>8__locals1.<>4__this.currentLevelData);
				CS$<>8__locals1.<>4__this.SortBackgroundCreaturesList(CS$<>8__locals1.<>4__this.currentLevelData);
				new SerializableEntityEditor(CS$<>8__locals1.<>4__this.editorContainer.Content.RectTransform, CS$<>8__locals1.<>4__this.selectedParams, false, true, "", 20, GUIStyle.LargeFont, true);
				CS$<>8__locals1.<>4__this.forceDifficultyInput.FloatValue = (CS$<>8__locals1.<>4__this.selectedParams.MinLevelDifficulty + CS$<>8__locals1.<>4__this.selectedParams.MaxLevelDifficulty) / 2f;
				return true;
			}));
			RectTransform rectT = new RectTransform(new Vector2(1f, 0f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("leveleditor.ruinparams");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock ruinTitle = new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
			this.ruinParamsList = new GUIListBox(new RectTransform(new Vector2(1f, 0.1f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true
			};
			GUIListBox guilistBox2 = this.ruinParamsList;
			guilistBox2.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(guilistBox2.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
			{
				if (CS$<>8__locals1.<>4__this.selectedRuinGenerationParams == obj)
				{
					CoroutineManager.StartCoroutine(base.<CreateUI>g__DeselectRuinParams|6(), "");
				}
				else
				{
					CS$<>8__locals1.<>4__this.selectedRuinGenerationParams = (obj as RuinGenerationParams);
					CS$<>8__locals1.<>4__this.CreateOutpostGenerationParamsEditor(CS$<>8__locals1.<>4__this.selectedRuinGenerationParams);
				}
				return true;
			}));
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("leveleditor.caveparams");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock caveTitle = new GUITextBlock(rectT2, text2, null, subHeadingFont, Alignment.Left, false, "", null);
			this.caveParamsList = new GUIListBox(new RectTransform(new Vector2(1f, 0.1f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true
			};
			GUIListBox guilistBox3 = this.caveParamsList;
			guilistBox3.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(guilistBox3.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
			{
				CS$<>8__locals1.<>4__this.CreateCaveParamsEditor(obj as CaveGenerationParams);
				return true;
			}));
			RectTransform rectT3 = new RectTransform(new Vector2(1f, 0f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text3 = TextManager.Get("leveleditor.outpostparams");
			subHeadingFont = GUIStyle.SubHeadingFont;
			GUITextBlock outpostTitle = new GUITextBlock(rectT3, text3, null, subHeadingFont, Alignment.Left, false, "", null);
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				ruinTitle,
				caveTitle,
				outpostTitle
			});
			this.outpostParamsList = new GUIListBox(new RectTransform(new Vector2(1f, 0.2f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true
			};
			GUIListBox guilistBox4 = this.outpostParamsList;
			guilistBox4.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(guilistBox4.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
			{
				CS$<>8__locals1.<>4__this.selectedOutpostGenerationParams = (obj as OutpostGenerationParams);
				CS$<>8__locals1.<>4__this.CreateOutpostGenerationParamsEditor(CS$<>8__locals1.<>4__this.selectedOutpostGenerationParams);
				return true;
			}));
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(1f, 0.05f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.createlevelobj"), Alignment.Center, "", null);
			guibutton.OnClicked = delegate(GUIButton btn, object obj)
			{
				LevelEditorScreen.Wizard.Instance.Create();
				return true;
			};
			GUIButton createLevelObjButton = guibutton;
			GUITextBlock.AutoScaleAndNormalize(new GUITextBlock[]
			{
				createLevelObjButton.TextBlock
			});
			this.lightingEnabled = new GUITickBox(new RectTransform(new Vector2(1f, 0.025f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.lightingenabled"), null, "");
			this.cursorLightEnabled = new GUITickBox(new RectTransform(new Vector2(1f, 0.025f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.cursorlightenabled"), null, "");
			new GUIButton(new RectTransform(new Vector2(1f, 0.05f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.reloadtextures"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				Level loaded = Level.Loaded;
				if (loaded != null)
				{
					loaded.ReloadTextures();
				}
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(1f, 0.05f), paddedLeftPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("editor.saveall"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				CS$<>8__locals1.<>4__this.SerializeAll();
				GUI.AddMessage(TextManager.Get("leveleditor.allsaved"), GUIStyle.Green, null, true, null);
				return true;
			};
			this.rightPanel = new GUIFrame(new RectTransform(new Vector2(0.25f, 1f), this.Frame.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(450, 0)
			}, "", null);
			GUILayoutGroup paddedRightPanel = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.95f), this.rightPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal)
			{
				RelativeOffset = new Vector2(0.02f, 0f)
			}, false, Anchor.TopLeft)
			{
				Stretch = true,
				RelativeSpacing = 0.01f
			};
			this.editorContainer = new GUIListBox(new RectTransform(new Vector2(1f, 1f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			CS$<>8__locals1.seedContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.04f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			CS$<>8__locals1.randomizeButtonRelativeSize = CS$<>8__locals1.<CreateUI>g__GetRandomizeButtonRelativeSize|1();
			Vector2 elementRelativeSize = CS$<>8__locals1.<CreateUI>g__GetSeedElementRelativeSize|2();
			CS$<>8__locals1.seedLabel = new GUITextBlock(new RectTransform(elementRelativeSize, CS$<>8__locals1.seedContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.levelseed"), null, null, Alignment.Left, false, "", null);
			this.seedBox = new GUITextBox(new RectTransform(elementRelativeSize, CS$<>8__locals1.seedContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), LevelEditorScreen.<CreateUI>g__GetLevelSeed|35_3(), null, null, Alignment.Left, false, "", null, false, true);
			CS$<>8__locals1.seedButton = new GUIButton(new RectTransform(CS$<>8__locals1.randomizeButtonRelativeSize, CS$<>8__locals1.seedContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Alignment.Center, "RandomizeButton", null)
			{
				OnClicked = delegate(GUIButton button, object userData)
				{
					if (CS$<>8__locals1.<>4__this.seedBox == null)
					{
						return false;
					}
					CS$<>8__locals1.<>4__this.seedBox.Text = LevelEditorScreen.<CreateUI>g__GetLevelSeed|35_3();
					return true;
				}
			};
			CS$<>8__locals1.seedContainer.RectTransform.SizeChanged += delegate()
			{
				Vector2 randomizeButtonRelativeSize = base.<CreateUI>g__GetRandomizeButtonRelativeSize|1();
				Vector2 elementRelativeSize2 = base.<CreateUI>g__GetSeedElementRelativeSize|2();
				CS$<>8__locals1.seedLabel.RectTransform.RelativeSize = elementRelativeSize2;
				CS$<>8__locals1.<>4__this.seedBox.RectTransform.RelativeSize = elementRelativeSize2;
				CS$<>8__locals1.seedButton.RectTransform.RelativeSize = randomizeButtonRelativeSize;
			};
			GUILayoutGroup subDropDownContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.02f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), subDropDownContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("submarine"), null, null, Alignment.Left, false, "", null);
			this.selectedSubDropDown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), subDropDownContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			foreach (SubmarineInfo sub in SubmarineInfo.SavedSubmarines)
			{
				if (sub.Type == SubmarineType.Player)
				{
					this.selectedSubDropDown.AddItem(sub.DisplayName, sub, null, null, null);
				}
			}
			subDropDownContainer.RectTransform.MinSize = new Point(0, this.selectedSubDropDown.RectTransform.MinSize.Y);
			GUILayoutGroup beaconStationDropDownContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.02f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), beaconStationDropDownContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("submarinetype.beaconstation"), null, null, Alignment.Left, false, "", null);
			this.selectedBeaconStationDropdown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), beaconStationDropDownContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			this.selectedBeaconStationDropdown.AddItem(TextManager.Get("Any"), null, null, null, null);
			foreach (SubmarineInfo beaconStation in SubmarineInfo.SavedSubmarines)
			{
				if (beaconStation.Type == SubmarineType.BeaconStation)
				{
					this.selectedBeaconStationDropdown.AddItem(beaconStation.DisplayName, beaconStation, null, null, null);
				}
			}
			beaconStationDropDownContainer.RectTransform.MinSize = new Point(0, this.selectedBeaconStationDropdown.RectTransform.MinSize.Y);
			GUILayoutGroup wreckDropDownContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.02f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), wreckDropDownContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("submarinetype.wreck"), null, null, Alignment.Left, false, "", null);
			this.selectedWreckDropdown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), wreckDropDownContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
			this.selectedWreckDropdown.AddItem(TextManager.Get("Any"), null, null, null, null);
			foreach (SubmarineInfo wreck in SubmarineInfo.SavedSubmarines)
			{
				if (wreck.Type == SubmarineType.Wreck)
				{
					this.selectedWreckDropdown.AddItem(wreck.DisplayName, wreck, null, null, null);
				}
			}
			wreckDropDownContainer.RectTransform.MinSize = new Point(0, this.selectedWreckDropdown.RectTransform.MinSize.Y);
			GUILayoutGroup forceDifficultyContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.02f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), forceDifficultyContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveldifficulty"), null, null, Alignment.Left, false, "", null);
			GUINumberInput guinumberInput = new GUINumberInput(new RectTransform(new Vector2(0.5f, 1f), forceDifficultyContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
			guinumberInput.MinValueFloat = new float?(0f);
			guinumberInput.MaxValueFloat = new float?((float)100);
			float? forcedDifficulty = Level.ForcedDifficulty;
			float floatValue;
			if (forcedDifficulty == null)
			{
				LevelGenerationParams levelGenerationParams = this.selectedParams;
				floatValue = ((levelGenerationParams != null) ? levelGenerationParams.MinLevelDifficulty : 0f);
			}
			else
			{
				floatValue = forcedDifficulty.GetValueOrDefault();
			}
			guinumberInput.FloatValue = floatValue;
			guinumberInput.OnValueChanged = delegate(GUINumberInput numberInput)
			{
				if (Level.ForcedDifficulty == null)
				{
					return;
				}
				Level.ForcedDifficulty = new float?(numberInput.FloatValue);
			};
			this.forceDifficultyInput = guinumberInput;
			forceDifficultyContainer.RectTransform.MinSize = new Point(0, this.forceDifficultyInput.RectTransform.MinSize.Y);
			GUILayoutGroup tickBoxContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.04f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			this.mirrorLevel = new GUITickBox(new RectTransform(new Vector2(0.5f, 0.02f), tickBoxContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("mirrorentityx"), null, "");
			this.allowInvalidOutpost = new GUITickBox(new RectTransform(new Vector2(0.5f, 0.025f), tickBoxContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.allowinvalidoutpost"), null, "")
			{
				ToolTip = TextManager.Get("leveleditor.allowinvalidoutpost.tooltip")
			};
			new GUIButton(new RectTransform(new Vector2(1f, 0.05f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.generate"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				bool wasLevelLoaded = Level.Loaded != null;
				Submarine.Unload();
				SubmarineInfo subInfo = CS$<>8__locals1.<>4__this.selectedSubDropDown.SelectedData as SubmarineInfo;
				if (subInfo != null)
				{
					Submarine.MainSub = new Submarine(subInfo, true, null, null);
				}
				GameMain.LightManager.ClearLights();
				CS$<>8__locals1.<>4__this.currentLevelData = LevelData.CreateRandom(CS$<>8__locals1.<>4__this.seedBox.Text, new float?(CS$<>8__locals1.<>4__this.forceDifficultyInput.FloatValue), CS$<>8__locals1.<>4__this.selectedParams, default(Identifier), false, false);
				CS$<>8__locals1.<>4__this.currentLevelData.ForceOutpostGenerationParams = (CS$<>8__locals1.<>4__this.outpostParamsList.SelectedData as OutpostGenerationParams);
				CS$<>8__locals1.<>4__this.currentLevelData.ForceBeaconStation = (CS$<>8__locals1.<>4__this.selectedBeaconStationDropdown.SelectedData as SubmarineInfo);
				CS$<>8__locals1.<>4__this.currentLevelData.ForceWreck = (CS$<>8__locals1.<>4__this.selectedWreckDropdown.SelectedData as SubmarineInfo);
				CS$<>8__locals1.<>4__this.currentLevelData.ForceRuinGenerationParams = CS$<>8__locals1.<>4__this.selectedRuinGenerationParams;
				CS$<>8__locals1.<>4__this.currentLevelData.AllowInvalidOutpost = CS$<>8__locals1.<>4__this.allowInvalidOutpost.Selected;
				Location[] dummyLocations = GameSession.CreateDummyLocations(CS$<>8__locals1.<>4__this.currentLevelData, null);
				Level.Generate(CS$<>8__locals1.<>4__this.currentLevelData, CS$<>8__locals1.<>4__this.mirrorLevel.Selected, dummyLocations[0], dummyLocations[1], null, null);
				CS$<>8__locals1.<>4__this.UpdateBackgroundCreatureList();
				if (Submarine.MainSub != null)
				{
					Vector2 startPos = Level.Loaded.StartPosition;
					if (Level.Loaded.StartOutpost != null)
					{
						startPos.Y -= (float)(Level.Loaded.StartOutpost.Borders.Height / 2 + Submarine.MainSub.Borders.Height / 2);
					}
					Submarine mainSub = Submarine.MainSub;
					if (mainSub != null)
					{
						mainSub.SetPosition(startPos, null, true);
					}
				}
				GameMain.LightManager.AddLight(CS$<>8__locals1.<>4__this.pointerLightSource);
				if (!wasLevelLoaded || CS$<>8__locals1.<>4__this.Cam.Position.X < 0f || CS$<>8__locals1.<>4__this.Cam.Position.Y < 0f || CS$<>8__locals1.<>4__this.Cam.Position.Y > (float)Level.Loaded.Size.X || CS$<>8__locals1.<>4__this.Cam.Position.Y > (float)Level.Loaded.Size.Y)
				{
					CS$<>8__locals1.<>4__this.Cam.Position = new Vector2((float)(Level.Loaded.Size.X / 2), (float)(Level.Loaded.Size.Y / 2));
				}
				foreach (GUIComponent guicomponent in CS$<>8__locals1.<>4__this.paramsList.Content.Children)
				{
					GUITextBlock param = (GUITextBlock)guicomponent;
					param.TextColor = ((param.UserData == CS$<>8__locals1.<>4__this.selectedParams) ? GUIStyle.Green : param.Style.TextColor);
				}
				CS$<>8__locals1.<>4__this.seedBox.Deselect();
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(1f, 0.05f), paddedRightPanel.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.test"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				Level loaded = Level.Loaded;
				if (((loaded != null) ? loaded.LevelData : null) == null)
				{
					return false;
				}
				GameMain.GameScreen.Select();
				List<Entity> currEntities = Entity.GetEntities().ToList<Entity>();
				if (Submarine.MainSub != null)
				{
					List<Entity> toRemove = (from e in Entity.GetEntities()
					where e.Submarine == Submarine.MainSub
					select e).ToList<Entity>();
					foreach (Entity ent in toRemove)
					{
						ent.Remove();
					}
					Submarine.MainSub.Remove();
				}
				BaseSubFile[] nonPlayerFiles = ContentPackageManager.EnabledPackages.All.SelectMany((ContentPackage p) => from f in p.GetFiles<BaseSubFile>()
				where !(f is SubmarineFile)
				select f).ToArray<BaseSubFile>();
				SubmarineInfo subInfo = CS$<>8__locals1.<>4__this.selectedSubDropDown.SelectedData as SubmarineInfo;
				if (subInfo == null)
				{
					subInfo = SubmarineInfo.SavedSubmarines.GetRandomUnsynced((SubmarineInfo s) => s.IsPlayer && !s.HasTag(SubmarineTag.Shuttle) && !nonPlayerFiles.Any((BaseSubFile f) => f.Path == s.FilePath));
				}
				SubmarineInfo submarineInfo = subInfo;
				Option.UnspecifiedNone none = Option.None;
				GameSession gameSession = new GameSession(submarineInfo, none, CampaignDataPath.Empty, GameModePreset.TestMode, CampaignSettings.Empty, null, null);
				gameSession.StartRound(Level.Loaded.LevelData, CS$<>8__locals1.<>4__this.mirrorLevel.Selected, null, null);
				Func<Entity, bool> <>9__22;
				(gameSession.GameMode as TestGameMode).OnRoundEnd = delegate()
				{
					GameMain.LevelEditorScreen.Select();
					Submarine.MainSub.Remove();
					IEnumerable<Entity> entities = Entity.GetEntities();
					Func<Entity, bool> predicate;
					if ((predicate = <>9__22) == null)
					{
						predicate = (<>9__22 = ((Entity e) => !currEntities.Contains(e)));
					}
					List<Entity> toRemove2 = entities.Where(predicate).ToList<Entity>();
					foreach (Entity ent2 in toRemove2)
					{
						ent2.Remove();
					}
					Submarine.MainSub = null;
				};
				GameMain.GameSession = gameSession;
				return true;
			};
			this.bottomPanel = new GUIFrame(new RectTransform(new Vector2(0.75f, 0.22f), this.Frame.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal)
			{
				MaxSize = new Point(GameMain.GraphicsWidth - this.rightPanel.Rect.Width, 1000)
			}, "GUIFrameBottom", null);
			GUILayoutGroup bottomPanelContents = new GUILayoutGroup(new RectTransform(new Vector2(0.98f, 0.9f), this.bottomPanel.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
			{
				Stretch = true
			};
			CS$<>8__locals1.bottomPanelButtons = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), bottomPanelContents.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.25f, 1f), CS$<>8__locals1.bottomPanelButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.levelobjects"), Alignment.Center, "GUITabButton", null);
			guibutton2.Selected = true;
			guibutton2.OnClicked = delegate(GUIButton btn, object __)
			{
				CS$<>8__locals1.bottomPanelButtons.Children.ForEach(delegate(GUIComponent c)
				{
					c.Selected = (c == btn);
				});
				CS$<>8__locals1.<>4__this.levelObjectList.Visible = true;
				CS$<>8__locals1.<>4__this.levelObjectList.IgnoreLayoutGroups = false;
				CS$<>8__locals1.<>4__this.backgroundCreatureList.Visible = false;
				CS$<>8__locals1.<>4__this.backgroundCreatureList.IgnoreLayoutGroups = true;
				return true;
			};
			new GUIButton(new RectTransform(new Vector2(0.25f, 1f), CS$<>8__locals1.bottomPanelButtons.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.backgroundcreatures"), Alignment.Center, "GUITabButton", null).OnClicked = delegate(GUIButton btn, object __)
			{
				CS$<>8__locals1.bottomPanelButtons.Children.ForEach(delegate(GUIComponent c)
				{
					c.Selected = (c == btn);
				});
				CS$<>8__locals1.<>4__this.backgroundCreatureList.Visible = true;
				CS$<>8__locals1.<>4__this.backgroundCreatureList.IgnoreLayoutGroups = false;
				CS$<>8__locals1.<>4__this.levelObjectList.Visible = false;
				CS$<>8__locals1.<>4__this.levelObjectList.IgnoreLayoutGroups = true;
				return true;
			};
			CS$<>8__locals1.bottomPanelButtons.RectTransform.NonScaledSize = new Point(CS$<>8__locals1.bottomPanelButtons.Rect.Width, CS$<>8__locals1.bottomPanelButtons.Children.First<GUIComponent>().Rect.Height);
			this.levelObjectList = new GUIListBox(new RectTransform(new Vector2(1f, 0.85f), bottomPanelContents.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				UseGridLayout = true
			};
			GUIListBox guilistBox5 = this.levelObjectList;
			guilistBox5.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(guilistBox5.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
			{
				CS$<>8__locals1.<>4__this.selectedLevelObject = (obj as LevelObjectPrefab);
				CS$<>8__locals1.<>4__this.CreateLevelObjectEditor(CS$<>8__locals1.<>4__this.selectedLevelObject);
				return true;
			}));
			this.backgroundCreatureList = new GUIListBox(new RectTransform(new Vector2(1f, 0.85f), bottomPanelContents.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true,
				UseGridLayout = true,
				Visible = false,
				IgnoreLayoutGroups = true
			};
			GUIListBox guilistBox6 = this.backgroundCreatureList;
			guilistBox6.OnSelected = (GUIListBox.OnSelectedHandler)Delegate.Combine(guilistBox6.OnSelected, new GUIListBox.OnSelectedHandler(delegate(GUIComponent component, object obj)
			{
				CS$<>8__locals1.<>4__this.selectedBackgroundCreature = (obj as BackgroundCreaturePrefab);
				CS$<>8__locals1.<>4__this.CreateBackgroundCreatureEditor(CS$<>8__locals1.<>4__this.selectedBackgroundCreature);
				return true;
			}));
			this.spriteEditDoneButton = new GUIButton(new RectTransform(new Point(200, 30), null, Anchor.BottomRight, null, ScaleBasis.Normal, false)
			{
				AbsoluteOffset = new Point(20, 20)
			}, TextManager.Get("leveleditor.spriteeditdone"), Alignment.Center, "", null)
			{
				OnClicked = delegate(GUIButton btn, object userdata)
				{
					CS$<>8__locals1.<>4__this.editingSprite = null;
					return true;
				}
			};
			this.topPanel = new GUIFrame(new RectTransform(new Point(400, 100), GUI.Canvas, Anchor.TopLeft, null, ScaleBasis.Normal, false)
			{
				RelativeOffset = new Vector2(this.leftPanel.RectTransform.RelativeSize.X * 2f, 0f)
			}, "GUIFrameTop", null);
			this.prevResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x06002549 RID: 9545 RVA: 0x0017D148 File Offset: 0x0017B348
		public LevelEditorScreen()
		{
			this.Cam = new Camera
			{
				MinZoom = 0.01f,
				MaxZoom = 1f
			};
			this.RefreshUI(true);
		}

		// Token: 0x0600254A RID: 9546 RVA: 0x0017D1D8 File Offset: 0x0017B3D8
		public void TestLevelGenerationForErrors(int amountOfLevelsToGenerate)
		{
			LevelEditorScreen.<>c__DisplayClass37_0 CS$<>8__locals1 = new LevelEditorScreen.<>c__DisplayClass37_0();
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.amountOfLevelsToGenerate = amountOfLevelsToGenerate;
			if (this.selectedParams == null)
			{
				throw new InvalidOperationException("No level generation parameters selected in the level editor.");
			}
			CoroutineManager.StartCoroutine(CS$<>8__locals1.<TestLevelGenerationForErrors>g__GenerateLevels|0(), "");
		}

		// Token: 0x0600254B RID: 9547 RVA: 0x0017D21D File Offset: 0x0017B41D
		public override void Select()
		{
			base.Select();
			this.RefreshUI(false);
		}

		// Token: 0x0600254C RID: 9548 RVA: 0x0017D22C File Offset: 0x0017B42C
		protected override void DeselectEditorSpecific()
		{
			LightSource lightSource = this.pointerLightSource;
			if (lightSource != null)
			{
				lightSource.Remove();
			}
			this.pointerLightSource = null;
		}

		// Token: 0x0600254D RID: 9549 RVA: 0x0017D248 File Offset: 0x0017B448
		private void UpdateParamsList()
		{
			this.editorContainer.ClearChildren();
			this.paramsList.Content.ClearChildren();
			foreach (LevelGenerationParams genParams in from p in LevelGenerationParams.LevelParams
			orderby p.Name
			select p)
			{
				GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), this.paramsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 20)
				}, genParams.Identifier.Value, null, null, Alignment.Left, false, "", null);
				guitextBlock.Padding = Vector4.Zero;
				guitextBlock.UserData = genParams;
			}
		}

		// Token: 0x0600254E RID: 9550 RVA: 0x0017D364 File Offset: 0x0017B564
		private void UpdateCaveParamsList()
		{
			this.editorContainer.ClearChildren();
			this.caveParamsList.Content.ClearChildren();
			foreach (CaveGenerationParams genParams in from p in CaveGenerationParams.CaveParams
			orderby p.Name
			select p)
			{
				GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), this.caveParamsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 20)
				}, genParams.Name, null, null, Alignment.Left, false, "", null);
				guitextBlock.Padding = Vector4.Zero;
				guitextBlock.UserData = genParams;
			}
		}

		// Token: 0x0600254F RID: 9551 RVA: 0x0017D47C File Offset: 0x0017B67C
		private void UpdateRuinParamsList()
		{
			this.editorContainer.ClearChildren();
			this.ruinParamsList.Content.ClearChildren();
			foreach (RuinGenerationParams genParams in from p in RuinGenerationParams.RuinParams
			orderby p.Identifier
			select p)
			{
				GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), this.ruinParamsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 20)
				}, genParams.Identifier.Value, null, null, Alignment.Left, false, "", null);
				guitextBlock.Padding = Vector4.Zero;
				guitextBlock.UserData = genParams;
			}
		}

		// Token: 0x06002550 RID: 9552 RVA: 0x0017D598 File Offset: 0x0017B798
		private void UpdateOutpostParamsList()
		{
			this.editorContainer.ClearChildren();
			this.outpostParamsList.Content.ClearChildren();
			foreach (OutpostGenerationParams genParams in from p in OutpostGenerationParams.OutpostParams
			orderby p.Name
			select p)
			{
				GUITextBlock guitextBlock = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), this.outpostParamsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 20)
				}, genParams.Name, null, null, Alignment.Left, false, "", null);
				guitextBlock.Padding = Vector4.Zero;
				guitextBlock.UserData = genParams;
			}
		}

		// Token: 0x06002551 RID: 9553 RVA: 0x0017D6B0 File Offset: 0x0017B8B0
		private void UpdateLevelObjectsList()
		{
			this.editorContainer.ClearChildren();
			this.levelObjectList.Content.ClearChildren();
			int objectsPerRow = (int)Math.Ceiling((double)((float)this.levelObjectList.Content.Rect.Width / Math.Max(100f * GUI.Scale, 100f)));
			float relWidth = 1f / (float)objectsPerRow;
			foreach (LevelObjectPrefab levelObjPrefab in LevelObjectPrefab.Prefabs)
			{
				GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(relWidth, relWidth * ((float)this.levelObjectList.Content.Rect.Width / (float)this.levelObjectList.Content.Rect.Height)), this.levelObjectList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 60)
				}, "ListBoxElementSquare", null)
				{
					UserData = levelObjPrefab,
					ToolTip = levelObjPrefab.Name
				};
				GUIFrame paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null)
				{
					CanBeFocused = false
				};
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal);
				RichString text = ToolBox.LimitString(levelObjPrefab.Name, GUIStyle.SmallFont, paddedFrame.Rect.Width);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock textBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Center, false, "", null)
				{
					CanBeFocused = false,
					ToolTip = levelObjPrefab.Name
				};
				Sprite sprite2;
				if ((sprite2 = levelObjPrefab.Sprites.FirstOrDefault<Sprite>()) == null)
				{
					DeformableSprite deformableSprite = levelObjPrefab.DeformableSprite;
					sprite2 = ((deformableSprite != null) ? deformableSprite.Sprite : null);
				}
				Sprite sprite = sprite2;
				GUIImage guiimage = new GUIImage(new RectTransform(new Point(paddedFrame.Rect.Height, paddedFrame.Rect.Height - textBlock.Rect.Height), paddedFrame.RectTransform, Anchor.TopCenter, null, ScaleBasis.Normal, false), sprite, true, null);
				guiimage.LoadAsynchronously = true;
				guiimage.CanBeFocused = false;
			}
		}

		// Token: 0x06002552 RID: 9554 RVA: 0x0017D980 File Offset: 0x0017BB80
		private void UpdateBackgroundCreatureList()
		{
			this.editorContainer.ClearChildren();
			this.backgroundCreatureList.Content.ClearChildren();
			int objectsPerRow = (int)Math.Ceiling((double)((float)this.backgroundCreatureList.Content.Rect.Width / Math.Max(100f * GUI.Scale, 100f)));
			float relWidth = 1f / (float)objectsPerRow;
			foreach (BackgroundCreaturePrefab backgroundCreaturePrefab in BackgroundCreaturePrefab.Prefabs)
			{
				GUIFrame frame = new GUIFrame(new RectTransform(new Vector2(relWidth, relWidth * ((float)this.backgroundCreatureList.Content.Rect.Width / (float)this.backgroundCreatureList.Content.Rect.Height)), this.backgroundCreatureList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					MinSize = new Point(0, 60)
				}, "ListBoxElementSquare", null)
				{
					UserData = backgroundCreaturePrefab
				};
				GUIFrame paddedFrame = new GUIFrame(new RectTransform(new Vector2(0.9f, 0.9f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), null, null);
				RectTransform rectT = new RectTransform(new Vector2(1f, 0f), paddedFrame.RectTransform, Anchor.BottomCenter, null, null, null, ScaleBasis.Normal);
				RichString text = ToolBox.LimitString(backgroundCreaturePrefab.Name, GUIStyle.SmallFont, paddedFrame.Rect.Width);
				GUIFont smallFont = GUIStyle.SmallFont;
				GUITextBlock textBlock = new GUITextBlock(rectT, text, null, smallFont, Alignment.Center, false, "", null)
				{
					CanBeFocused = false,
					ToolTip = backgroundCreaturePrefab.Name
				};
				Sprite sprite2;
				if ((sprite2 = backgroundCreaturePrefab.Sprite) == null)
				{
					DeformableSprite deformableSprite = backgroundCreaturePrefab.DeformableSprite;
					sprite2 = ((deformableSprite != null) ? deformableSprite.Sprite : null);
				}
				Sprite sprite = sprite2;
				GUIImage guiimage = new GUIImage(new RectTransform(new Point(paddedFrame.Rect.Height, paddedFrame.Rect.Height - textBlock.Rect.Height), paddedFrame.RectTransform, Anchor.TopCenter, null, ScaleBasis.Normal, false), sprite, true, null);
				guiimage.LoadAsynchronously = true;
				guiimage.CanBeFocused = false;
			}
			this.SortBackgroundCreaturesList(this.currentLevelData);
		}

		// Token: 0x06002553 RID: 9555 RVA: 0x0017DC40 File Offset: 0x0017BE40
		private void CreateCaveParamsEditor(CaveGenerationParams caveGenerationParams)
		{
			this.editorContainer.ClearChildren();
			SerializableEntityEditor editor = new SerializableEntityEditor(this.editorContainer.Content.RectTransform, caveGenerationParams, false, true, "", 20, null, true);
			if (this.selectedParams != null)
			{
				GUILayoutGroup commonnessContainer = new GUILayoutGroup(new RectTransform(new Point(editor.Rect.Width, 70), null, Anchor.TopLeft, null, ScaleBasis.Normal, false)
				{
					IsFixedSize = true
				}, false, Anchor.TopCenter)
				{
					AbsoluteSpacing = 5,
					Stretch = true
				};
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.4f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("leveleditor.levelobjcommonness", "[leveltype]", this.selectedParams.Identifier.Value, FormatCapitals.No), null, null, Alignment.Center, false, "", null);
				GUINumberInput guinumberInput = new GUINumberInput(new RectTransform(new Vector2(0.5f, 0.4f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
				guinumberInput.MinValueFloat = new float?(0f);
				guinumberInput.MaxValueFloat = new float?((float)100);
				guinumberInput.FloatValue = caveGenerationParams.GetCommonness(this.currentLevelData, false);
				guinumberInput.OnValueChanged = delegate(GUINumberInput numberInput)
				{
					caveGenerationParams.OverrideCommonness[this.selectedParams.Identifier] = numberInput.FloatValue;
				};
				new GUIFrame(new RectTransform(new Vector2(1f, 0.2f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
				editor.AddCustomContent(commonnessContainer, 1);
			}
		}

		// Token: 0x06002554 RID: 9556 RVA: 0x0017DE5C File Offset: 0x0017C05C
		private void CreateOutpostGenerationParamsEditor(OutpostGenerationParams outpostGenerationParams)
		{
			this.editorContainer.ClearChildren();
			if (outpostGenerationParams == null)
			{
				return;
			}
			SerializableEntityEditor outpostParamsEditor = new SerializableEntityEditor(this.editorContainer.Content.RectTransform, outpostGenerationParams, false, true, "", 20, null, true);
			GUILayoutGroup locationTypeGroup = new GUILayoutGroup(new RectTransform(new Point(this.editorContainer.Content.Rect.Width, 20), null, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.CenterLeft)
			{
				Stretch = true
			};
			new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), locationTypeGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("outpostmoduleallowedlocationtypes"), null, null, Alignment.CenterLeft, false, "", null);
			HashSet<Identifier> availableLocationTypes = new HashSet<Identifier>
			{
				"any".ToIdentifier()
			};
			foreach (LocationType locationType in LocationType.Prefabs)
			{
				availableLocationTypes.Add(locationType.Identifier);
			}
			GUIDropDown locationTypeDropDown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), locationTypeGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), LocalizedString.Join(", ", (from lt in outpostGenerationParams.AllowedLocationTypes
			select lt.Value.Capitalize()) ?? "any".ToEnumerable<LocalizedString>()), 4, "", true, false, Alignment.CenterLeft, 1f);
			foreach (Identifier locationType2 in availableLocationTypes)
			{
				locationTypeDropDown.AddItem(locationType2.Value.Capitalize(), locationType2, null, null, null);
				if (outpostGenerationParams.AllowedLocationTypes.Contains(locationType2))
				{
					locationTypeDropDown.SelectItem(locationType2);
				}
			}
			if (!outpostGenerationParams.AllowedLocationTypes.Any<Identifier>())
			{
				locationTypeDropDown.SelectItem("any");
			}
			GUIDropDown locationTypeDropDown2 = locationTypeDropDown;
			locationTypeDropDown2.AfterSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(locationTypeDropDown2.AfterSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent _, object __)
			{
				outpostGenerationParams.SetAllowedLocationTypes(locationTypeDropDown.SelectedDataMultiple.Cast<Identifier>());
				locationTypeDropDown.Text = ToolBox.LimitString(locationTypeDropDown.Text, locationTypeDropDown.Font, locationTypeDropDown.Rect.Width);
				return true;
			}));
			locationTypeGroup.RectTransform.MinSize = new Point(locationTypeGroup.Rect.Width, locationTypeGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			outpostParamsEditor.AddCustomContent(locationTypeGroup, 100);
			using (IEnumerator<OutpostGenerationParams.ModuleCount> enumerator3 = outpostGenerationParams.ModuleCounts.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					OutpostGenerationParams.ModuleCount moduleCount = enumerator3.Current;
					SerializableEntityEditor editor = new SerializableEntityEditor(this.editorContainer.Content.RectTransform, moduleCount, false, true, "", 20, GUIStyle.Font, true);
					GUINumberInput.OnValueChangedHandler <>9__4;
					foreach (GUIComponent[] componentList in editor.Fields.Values)
					{
						foreach (GUIComponent component in componentList)
						{
							GUINumberInput numberInput = component as GUINumberInput;
							if (numberInput != null)
							{
								GUINumberInput guinumberInput = numberInput;
								Delegate onValueChanged = guinumberInput.OnValueChanged;
								GUINumberInput.OnValueChangedHandler b;
								if ((b = <>9__4) == null)
								{
									b = (<>9__4 = delegate(GUINumberInput numInput)
									{
										if (moduleCount.Count == 0)
										{
											this.outpostParamsList.Select(this.outpostParamsList.SelectedData, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
										}
									});
								}
								guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(onValueChanged, b);
							}
						}
					}
					editor.RectTransform.MaxSize = new Point(int.MaxValue, editor.Rect.Height);
					outpostParamsEditor.AddCustomContent(editor, 100);
					editor.Recalculate();
				}
			}
			GUILayoutGroup addModuleCountGroup = new GUILayoutGroup(new RectTransform(new Point(this.editorContainer.Content.Rect.Width, (int)(40f * GUI.Scale)), null, Anchor.TopLeft, null, ScaleBasis.Normal, false), true, Anchor.Center);
			HashSet<Identifier> availableFlags = new HashSet<Identifier>();
			foreach (Identifier flag3 in OutpostGenerationParams.OutpostParams.SelectMany((OutpostGenerationParams p) => from m in p.ModuleCounts
			select m.Identifier))
			{
				availableFlags.Add(flag3);
			}
			foreach (SubmarineInfo sub in SubmarineInfo.SavedSubmarines)
			{
				if (sub.OutpostModuleInfo != null)
				{
					foreach (Identifier flag2 in sub.OutpostModuleInfo.ModuleFlags)
					{
						availableFlags.Add(flag2);
					}
				}
			}
			GUIDropDown moduleTypeDropDown = new GUIDropDown(new RectTransform(new Vector2(0.8f, 0.8f), addModuleCountGroup.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("leveleditor.addmoduletype"), 4, "", false, false, Alignment.CenterLeft, 1f);
			using (HashSet<Identifier>.Enumerator enumerator8 = availableFlags.GetEnumerator())
			{
				while (enumerator8.MoveNext())
				{
					Identifier flag = enumerator8.Current;
					if (!outpostGenerationParams.ModuleCounts.Any((OutpostGenerationParams.ModuleCount mc) => mc.Identifier == flag))
					{
						moduleTypeDropDown.AddItem(flag.Value.Capitalize(), flag, null, null, null);
					}
				}
			}
			GUIDropDown guidropDown = moduleTypeDropDown;
			guidropDown.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(guidropDown.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent _, object userdata)
			{
				outpostGenerationParams.SetModuleCount((Identifier)userdata, 1, null, null, null);
				this.outpostParamsList.Select(this.outpostParamsList.SelectedData, GUIListBox.Force.No, GUIListBox.AutoScroll.Enabled);
				return true;
			}));
			addModuleCountGroup.RectTransform.MinSize = new Point(addModuleCountGroup.Rect.Width, addModuleCountGroup.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			outpostParamsEditor.AddCustomContent(addModuleCountGroup, 100);
			outpostParamsEditor.Recalculate();
		}

		// Token: 0x06002555 RID: 9557 RVA: 0x0017E618 File Offset: 0x0017C818
		private void CreateLevelObjectEditor(LevelObjectPrefab levelObjectPrefab)
		{
			LevelEditorScreen.<>c__DisplayClass48_0 CS$<>8__locals1 = new LevelEditorScreen.<>c__DisplayClass48_0();
			CS$<>8__locals1.levelObjectPrefab = levelObjectPrefab;
			CS$<>8__locals1.<>4__this = this;
			this.editorContainer.ClearChildren();
			SerializableEntityEditor editor = new SerializableEntityEditor(this.editorContainer.Content.RectTransform, CS$<>8__locals1.levelObjectPrefab, false, true, "", 20, GUIStyle.LargeFont, true);
			if (this.selectedParams != null)
			{
				List<Identifier> availableIdentifiers = new List<Identifier>();
				if (this.selectedParams != null)
				{
					availableIdentifiers.Add(this.selectedParams.Identifier);
				}
				foreach (CaveGenerationParams caveParam in CaveGenerationParams.CaveParams)
				{
					if (this.selectedParams == null || caveParam.GetCommonness(this.currentLevelData, false) > 0f)
					{
						availableIdentifiers.Add(caveParam.Identifier);
					}
				}
				availableIdentifiers.Reverse();
				using (List<Identifier>.Enumerator enumerator2 = availableIdentifiers.GetEnumerator())
				{
					while (enumerator2.MoveNext())
					{
						Identifier paramsId = enumerator2.Current;
						GUILayoutGroup commonnessContainer = new GUILayoutGroup(new RectTransform(new Point(editor.Rect.Width, 70), null, Anchor.TopLeft, null, ScaleBasis.Normal, false)
						{
							IsFixedSize = true
						}, false, Anchor.TopCenter)
						{
							AbsoluteSpacing = 5,
							Stretch = true
						};
						new GUITextBlock(new RectTransform(new Vector2(1f, 0.4f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("leveleditor.levelobjcommonness", "[leveltype]", paramsId.Value, FormatCapitals.No), null, null, Alignment.Center, false, "", null);
						GUINumberInput guinumberInput = new GUINumberInput(new RectTransform(new Vector2(0.5f, 0.4f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
						guinumberInput.MinValueFloat = new float?(0f);
						guinumberInput.MaxValueFloat = new float?((float)100);
						guinumberInput.FloatValue = ((this.selectedParams.Identifier == paramsId) ? CS$<>8__locals1.levelObjectPrefab.GetCommonness(this.currentLevelData) : CS$<>8__locals1.levelObjectPrefab.GetCommonness(CaveGenerationParams.CaveParams.Find((CaveGenerationParams p) => p.Identifier == paramsId), true));
						guinumberInput.OnValueChanged = delegate(GUINumberInput numberInput)
						{
							CS$<>8__locals1.levelObjectPrefab.OverrideCommonness[paramsId] = numberInput.FloatValue;
						};
						new GUIFrame(new RectTransform(new Vector2(1f, 0.2f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
						editor.AddCustomContent(commonnessContainer, 1);
					}
				}
			}
			LevelEditorScreen.<>c__DisplayClass48_0 CS$<>8__locals3 = CS$<>8__locals1;
			Sprite sprite;
			if ((sprite = CS$<>8__locals1.levelObjectPrefab.Sprites.FirstOrDefault<Sprite>()) == null)
			{
				DeformableSprite deformableSprite = CS$<>8__locals1.levelObjectPrefab.DeformableSprite;
				sprite = ((deformableSprite != null) ? deformableSprite.Sprite : null);
			}
			CS$<>8__locals3.sprite = sprite;
			if (CS$<>8__locals1.sprite != null)
			{
				editor.AddCustomContent(new GUIButton(new RectTransform(new Point(editor.Rect.Width / 2, (int)(25f * GUI.Scale)), null, Anchor.TopLeft, null, ScaleBasis.Normal, false)
				{
					IsFixedSize = true
				}, TextManager.Get("leveleditor.editsprite"), Alignment.Center, "", null)
				{
					OnClicked = delegate(GUIButton btn, object userdata)
					{
						CS$<>8__locals1.<>4__this.editingSprite = CS$<>8__locals1.sprite;
						GameMain.SpriteEditorScreen.SelectSprite(CS$<>8__locals1.<>4__this.editingSprite);
						return true;
					}
				}, 1);
			}
			if (CS$<>8__locals1.levelObjectPrefab.DeformableSprite != null)
			{
				GUIComponent deformEditor = CS$<>8__locals1.levelObjectPrefab.DeformableSprite.CreateEditor(editor, CS$<>8__locals1.levelObjectPrefab.SpriteDeformations, CS$<>8__locals1.levelObjectPrefab.Name);
				GUIDropDown child = deformEditor.GetChild<GUIDropDown>();
				child.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(child.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent selected, object userdata)
				{
					CS$<>8__locals1.<>4__this.CreateLevelObjectEditor(CS$<>8__locals1.<>4__this.selectedLevelObject);
					return true;
				}));
				editor.AddCustomContent(deformEditor, editor.ContentCount);
			}
			RectTransform rectT = new RectTransform(new Point(editor.Rect.Width, 40), this.editorContainer.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			RichString text = TextManager.Get("leveleditor.childobjects");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.BottomCenter, false, "", null);
			using (List<LevelObjectPrefab.ChildObject>.Enumerator enumerator3 = CS$<>8__locals1.levelObjectPrefab.ChildObjects.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					LevelObjectPrefab.ChildObject childObj = enumerator3.Current;
					GUIFrame childObjFrame = new GUIFrame(new RectTransform(new Point(editor.Rect.Width, 30), null, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", null);
					GUILayoutGroup paddedFrame = new GUILayoutGroup(new RectTransform(new Vector2(0.95f, 0.9f), childObjFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
					{
						Stretch = true,
						RelativeSpacing = 0.05f
					};
					LevelObjectPrefab.ChildObject selectedChildObj = childObj;
					GUIDropDown dropdown = new GUIDropDown(new RectTransform(new Vector2(0.5f, 1f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 10, "", true, false, Alignment.CenterLeft, 1f);
					foreach (LevelObjectPrefab objPrefab in LevelObjectPrefab.Prefabs)
					{
						dropdown.AddItem(objPrefab.Name, objPrefab, null, null, null);
						if (childObj.AllowedNames.Contains(objPrefab.Name))
						{
							dropdown.SelectItem(objPrefab);
						}
					}
					dropdown.AfterSelected = delegate(GUIComponent selected, object obj)
					{
						childObj.AllowedNames = (from d in dropdown.SelectedDataMultiple
						select ((LevelObjectPrefab)d).Name).ToList<string>();
						return true;
					};
					GUINumberInput guinumberInput2 = new GUINumberInput(new RectTransform(new Vector2(0.2f, 1f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
					guinumberInput2.MinValueInt = new int?(0);
					guinumberInput2.MaxValueInt = new int?(10);
					guinumberInput2.OnValueChanged = delegate(GUINumberInput numberInput)
					{
						selectedChildObj.MinCount = numberInput.IntValue;
						selectedChildObj.MaxCount = Math.Max(selectedChildObj.MaxCount, selectedChildObj.MinCount);
					};
					guinumberInput2.IntValue = childObj.MinCount;
					GUINumberInput guinumberInput3 = new GUINumberInput(new RectTransform(new Vector2(0.2f, 1f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
					guinumberInput3.MinValueInt = new int?(0);
					guinumberInput3.MaxValueInt = new int?(10);
					guinumberInput3.OnValueChanged = delegate(GUINumberInput numberInput)
					{
						selectedChildObj.MaxCount = numberInput.IntValue;
						selectedChildObj.MinCount = Math.Min(selectedChildObj.MaxCount, selectedChildObj.MinCount);
					};
					guinumberInput3.IntValue = childObj.MaxCount;
					new GUIButton(new RectTransform(new Vector2(0.1f, 1f), paddedFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUICancelButton", null).OnClicked = delegate(GUIButton btn, object userdata)
					{
						CS$<>8__locals1.<>4__this.selectedLevelObject.ChildObjects.Remove(selectedChildObj);
						CS$<>8__locals1.<>4__this.CreateLevelObjectEditor(CS$<>8__locals1.<>4__this.selectedLevelObject);
						return true;
					};
					childObjFrame.RectTransform.Parent = this.editorContainer.Content.RectTransform;
				}
			}
			GUIFrame buttonContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), this.editorContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIButton(new RectTransform(new Point(editor.Rect.Width / 2, 20), buttonContainer.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), TextManager.Get("leveleditor.addchildobject"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				CS$<>8__locals1.<>4__this.selectedLevelObject.ChildObjects.Add(new LevelObjectPrefab.ChildObject());
				CS$<>8__locals1.<>4__this.CreateLevelObjectEditor(CS$<>8__locals1.<>4__this.selectedLevelObject);
				return true;
			};
			buttonContainer.RectTransform.MinSize = buttonContainer.RectTransform.Children.First<RectTransform>().MinSize;
			RectTransform rectT2 = new RectTransform(new Point(editor.Rect.Width, 40), this.editorContainer.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
			RichString text2 = TextManager.Get("leveleditor.lightsources");
			subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, subHeadingFont, Alignment.BottomCenter, false, "", null);
			foreach (LightSourceParams lightSourceParams in this.selectedLevelObject.LightSourceParams)
			{
				new SerializableEntityEditor(this.editorContainer.Content.RectTransform, lightSourceParams, false, true, "", 24, null, true);
			}
			buttonContainer = new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), this.editorContainer.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			new GUIButton(new RectTransform(new Point(editor.Rect.Width / 2, 20), buttonContainer.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), TextManager.Get("leveleditor.addlightsource"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object userdata)
			{
				CS$<>8__locals1.<>4__this.selectedLevelObject.LightSourceTriggerIndex.Add(-1);
				CS$<>8__locals1.<>4__this.selectedLevelObject.LightSourceParams.Add(new LightSourceParams(100f, Color.White));
				CS$<>8__locals1.<>4__this.CreateLevelObjectEditor(CS$<>8__locals1.<>4__this.selectedLevelObject);
				return true;
			};
			buttonContainer.RectTransform.MinSize = buttonContainer.RectTransform.Children.First<RectTransform>().MinSize;
		}

		// Token: 0x06002556 RID: 9558 RVA: 0x0017F14C File Offset: 0x0017D34C
		private void SortLevelObjectsList(LevelData levelData)
		{
			foreach (GUIComponent levelObjFrame in this.levelObjectList.Content.Children)
			{
				LevelObjectPrefab levelObj = levelObjFrame.UserData as LevelObjectPrefab;
				float commonness = levelObj.GetCommonness(levelData);
				Color color = GUIStyle.Green;
				if (commonness > 0f)
				{
					LevelData levelData2 = levelData;
					if (((levelData2 != null) ? levelData2.GenerationParams : null) != null && levelObj.MinSurfaceWidth > (float)levelData.GenerationParams.CellSubdivisionLength && levelObj.SpawnPos.HasFlag(LevelObjectPrefab.SpawnPosType.Wall))
					{
						color = Color.Orange;
						GUIComponent guicomponent = levelObjFrame;
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(217, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Potential issue: the level walls in \"");
						defaultInterpolatedStringHandler.AppendFormatted<Identifier>(levelData.GenerationParams.Identifier);
						defaultInterpolatedStringHandler.AppendLiteral("\" are set to be subdivided every ");
						defaultInterpolatedStringHandler.AppendFormatted<int>(levelData.GenerationParams.CellSubdivisionLength);
						defaultInterpolatedStringHandler.AppendLiteral(" pixels, but the level object requires wall segments of at least ");
						defaultInterpolatedStringHandler.AppendFormatted<float>(levelObj.MinSurfaceWidth);
						defaultInterpolatedStringHandler.AppendLiteral(" px. The object may be rarer than intended (or fail to spawn at all) in the level.");
						guicomponent.ToolTip = defaultInterpolatedStringHandler.ToStringAndClear();
					}
				}
				levelObjFrame.Color = ((commonness > 0f) ? (color * 0.4f) : Color.Transparent);
				levelObjFrame.SelectedColor = ((commonness > 0f) ? (color * 0.6f) : (Color.White * 0.5f));
				levelObjFrame.HoverColor = ((commonness > 0f) ? (color * 0.7f) : (Color.White * 0.6f));
				levelObjFrame.GetAnyChild<GUIImage>().Color = ((commonness > 0f) ? Color.White : Color.DarkGray);
				if (commonness <= 0f)
				{
					levelObjFrame.GetAnyChild<GUITextBlock>().TextColor = Color.DarkGray;
				}
			}
			this.levelObjectList.Content.RectTransform.SortChildren(delegate(RectTransform c1, RectTransform c2)
			{
				LevelObjectPrefab levelObj2 = c1.GUIComponent.UserData as LevelObjectPrefab;
				LevelObjectPrefab levelObj3 = c2.GUIComponent.UserData as LevelObjectPrefab;
				return Math.Sign(levelObj3.GetCommonness(levelData) - levelObj2.GetCommonness(levelData));
			});
		}

		// Token: 0x06002557 RID: 9559 RVA: 0x0017F3A0 File Offset: 0x0017D5A0
		private void SortBackgroundCreaturesList(LevelData levelData)
		{
			if (levelData == null)
			{
				return;
			}
			foreach (GUIComponent child in this.backgroundCreatureList.Content.Children)
			{
				BackgroundCreaturePrefab creature = child.UserData as BackgroundCreaturePrefab;
				if (creature != null)
				{
					LevelEditorScreen.SetElementColorBasedOnCommonness(child, creature.GetCommonness(levelData));
				}
			}
			this.backgroundCreatureList.Content.RectTransform.SortChildren(delegate(RectTransform c1, RectTransform c2)
			{
				BackgroundCreaturePrefab creature2 = c1.GUIComponent.UserData as BackgroundCreaturePrefab;
				BackgroundCreaturePrefab creature3 = c2.GUIComponent.UserData as BackgroundCreaturePrefab;
				return Math.Sign(creature3.GetCommonness(levelData) - creature2.GetCommonness(levelData));
			});
		}

		// Token: 0x06002558 RID: 9560 RVA: 0x0017F448 File Offset: 0x0017D648
		private static void SetElementColorBasedOnCommonness(GUIComponent element, float commonness)
		{
			element.Color = ((commonness > 0f) ? (GUIStyle.Green * 0.4f) : Color.Transparent);
			element.SelectedColor = ((commonness > 0f) ? (GUIStyle.Green * 0.6f) : (Color.White * 0.5f));
			element.HoverColor = ((commonness > 0f) ? (GUIStyle.Green * 0.7f) : (Color.White * 0.6f));
			element.GetAnyChild<GUIImage>().Color = ((commonness > 0f) ? Color.White : Color.DarkGray);
			if (commonness <= 0f)
			{
				element.GetAnyChild<GUITextBlock>().TextColor = Color.DarkGray;
			}
		}

		// Token: 0x06002559 RID: 9561 RVA: 0x0017F50C File Offset: 0x0017D70C
		private void CreateBackgroundCreatureEditor(BackgroundCreaturePrefab backgroundCreaturePrefab)
		{
			LevelEditorScreen.<>c__DisplayClass52_0 CS$<>8__locals1 = new LevelEditorScreen.<>c__DisplayClass52_0();
			CS$<>8__locals1.backgroundCreaturePrefab = backgroundCreaturePrefab;
			CS$<>8__locals1.<>4__this = this;
			this.editorContainer.ClearChildren();
			SerializableEntityEditor editor = new SerializableEntityEditor(this.editorContainer.Content.RectTransform, CS$<>8__locals1.backgroundCreaturePrefab, false, true, "", 20, GUIStyle.LargeFont, true);
			if (this.selectedParams != null)
			{
				List<Identifier> availableIdentifiers = new List<Identifier>
				{
					this.selectedParams.Identifier
				};
				using (List<Identifier>.Enumerator enumerator = availableIdentifiers.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						Identifier paramsId = enumerator.Current;
						GUILayoutGroup commonnessContainer = new GUILayoutGroup(new RectTransform(new Point(editor.Rect.Width, 70), null, Anchor.TopLeft, null, ScaleBasis.Normal, false)
						{
							IsFixedSize = true
						}, false, Anchor.TopCenter)
						{
							AbsoluteSpacing = 5,
							Stretch = true
						};
						new GUITextBlock(new RectTransform(new Vector2(1f, 0.4f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.GetWithVariable("leveleditor.levelobjcommonness", "[leveltype]", paramsId.Value, FormatCapitals.No), null, null, Alignment.Center, false, "", null);
						GUINumberInput guinumberInput = new GUINumberInput(new RectTransform(new Vector2(0.5f, 0.4f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
						guinumberInput.MinValueFloat = new float?(0f);
						guinumberInput.MaxValueFloat = new float?((float)100);
						guinumberInput.FloatValue = CS$<>8__locals1.backgroundCreaturePrefab.GetCommonness(this.currentLevelData);
						guinumberInput.OnValueChanged = delegate(GUINumberInput numberInput)
						{
							CS$<>8__locals1.backgroundCreaturePrefab.OverrideCommonness[paramsId] = numberInput.FloatValue;
						};
						new GUIFrame(new RectTransform(new Vector2(1f, 0.2f), commonnessContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
						editor.AddCustomContent(commonnessContainer, 1);
					}
				}
			}
			LevelEditorScreen.<>c__DisplayClass52_0 CS$<>8__locals3 = CS$<>8__locals1;
			Sprite sprite;
			if ((sprite = CS$<>8__locals1.backgroundCreaturePrefab.Sprite) == null)
			{
				DeformableSprite deformableSprite = CS$<>8__locals1.backgroundCreaturePrefab.DeformableSprite;
				sprite = ((deformableSprite != null) ? deformableSprite.Sprite : null);
			}
			CS$<>8__locals3.sprite = sprite;
			if (CS$<>8__locals1.sprite != null)
			{
				editor.AddCustomContent(new GUIButton(new RectTransform(new Point(editor.Rect.Width / 2, (int)(25f * GUI.Scale)), null, Anchor.TopLeft, null, ScaleBasis.Normal, false)
				{
					IsFixedSize = true
				}, TextManager.Get("leveleditor.editsprite"), Alignment.Center, "", null)
				{
					OnClicked = delegate(GUIButton btn, object userdata)
					{
						CS$<>8__locals1.<>4__this.editingSprite = CS$<>8__locals1.sprite;
						GameMain.SpriteEditorScreen.SelectSprite(CS$<>8__locals1.<>4__this.editingSprite);
						return true;
					}
				}, 1);
			}
			if (CS$<>8__locals1.backgroundCreaturePrefab.DeformableSprite != null)
			{
				GUIComponent deformEditor = CS$<>8__locals1.backgroundCreaturePrefab.DeformableSprite.CreateEditor(editor, CS$<>8__locals1.backgroundCreaturePrefab.SpriteDeformations, CS$<>8__locals1.backgroundCreaturePrefab.Name);
				GUIDropDown child = deformEditor.GetChild<GUIDropDown>();
				child.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(child.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent selected, object userdata)
				{
					CS$<>8__locals1.<>4__this.CreateBackgroundCreatureEditor(CS$<>8__locals1.backgroundCreaturePrefab);
					return true;
				}));
				editor.AddCustomContent(deformEditor, editor.ContentCount);
			}
		}

		// Token: 0x0600255A RID: 9562 RVA: 0x0017F8C0 File Offset: 0x0017DAC0
		public override void AddToGUIUpdateList()
		{
			base.AddToGUIUpdateList();
			this.rightPanel.Visible = (this.leftPanel.Visible = (this.bottomPanel.Visible = (this.editingSprite == null)));
			if (this.editingSprite != null)
			{
				GameMain.SpriteEditorScreen.TopPanel.AddToGUIUpdateList(false, 0);
				this.spriteEditDoneButton.AddToGUIUpdateList(false, 0);
				return;
			}
			if (this.lightingEnabled.Selected && this.cursorLightEnabled.Selected)
			{
				this.topPanel.AddToGUIUpdateList(false, 0);
			}
		}

		// Token: 0x0600255B RID: 9563 RVA: 0x0017F954 File Offset: 0x0017DB54
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			LevelEditorScreen.<>c__DisplayClass54_0 CS$<>8__locals1;
			CS$<>8__locals1.<>4__this = this;
			CS$<>8__locals1.spriteBatch = spriteBatch;
			if (this.lightingEnabled.Selected)
			{
				GameMain.LightManager.RenderLightMap(graphics, CS$<>8__locals1.spriteBatch, this.Cam, null);
			}
			graphics.Clear(Color.Black);
			if (Level.Loaded != null)
			{
				Level.Loaded.DrawBack(graphics, CS$<>8__locals1.spriteBatch, this.Cam);
				Level.Loaded.DrawFront(CS$<>8__locals1.spriteBatch, this.Cam);
				CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearWrap, DepthStencilState.DepthRead, null, null, new Matrix?(this.Cam.Transform));
				Level.Loaded.DrawDebugOverlay(CS$<>8__locals1.spriteBatch, this.Cam);
				Submarine.Draw(CS$<>8__locals1.spriteBatch, false);
				Submarine.DrawFront(CS$<>8__locals1.spriteBatch, false, null);
				Submarine.DrawDamageable(CS$<>8__locals1.spriteBatch, null, false, null);
				GUI.DrawRectangle(CS$<>8__locals1.spriteBatch, new Rectangle(new Point(0, -Level.Loaded.Size.Y), Level.Loaded.Size), Color.Gray, false, 0f, (float)((int)(1f / this.Cam.Zoom)));
				for (int i = 0; i < Level.Loaded.Tunnels.Count; i++)
				{
					Level.Tunnel tunnel = Level.Loaded.Tunnels[i];
					Color tunnelColor = this.tunnelDebugColors[i % this.tunnelDebugColors.Length] * 0.2f;
					for (int j = 1; j < tunnel.Nodes.Count; j++)
					{
						Vector2 start = new Vector2((float)tunnel.Nodes[j - 1].X, (float)(-(float)tunnel.Nodes[j - 1].Y));
						Vector2 end = new Vector2((float)tunnel.Nodes[j].X, (float)(-(float)tunnel.Nodes[j].Y));
						GUI.DrawLine(CS$<>8__locals1.spriteBatch, start, end, tunnelColor, 0f, (float)((int)(2f / this.Cam.Zoom)));
					}
				}
				foreach (Level.InterestingPosition interestingPos in Level.Loaded.PositionsOfInterest)
				{
					if (interestingPos.Position.X >= this.Cam.WorldView.X && interestingPos.Position.X <= this.Cam.WorldView.Right && interestingPos.Position.Y <= this.Cam.WorldView.Y && interestingPos.Position.Y >= this.Cam.WorldView.Y - this.Cam.WorldView.Height)
					{
						Vector2 pos = new Vector2((float)interestingPos.Position.X, (float)(-(float)interestingPos.Position.Y));
						CS$<>8__locals1.spriteBatch.DrawCircle(pos, 500f, 6, Color.White * 0.5f, (float)((int)(2f / this.Cam.Zoom)));
						SpriteBatch spriteBatch2 = CS$<>8__locals1.spriteBatch;
						Vector2 pos2 = pos;
						string text = interestingPos.PositionType.ToString();
						Color white = Color.White;
						GUIFont largeFont = GUIStyle.LargeFont;
						GUI.DrawString(spriteBatch2, pos2, text, white, null, 0, largeFont, ForceUpperCase.Inherit);
					}
				}
				foreach (Level.PathPoint pathPoint in Level.Loaded.PathPoints)
				{
					Vector2 pathPointPos = new Vector2(pathPoint.Position.X, -pathPoint.Position.Y);
					GUIFont largeFont;
					foreach (Level.ClusterLocation location in pathPoint.ClusterLocations)
					{
						if (location.Resources != null)
						{
							foreach (Item resource in location.Resources)
							{
								Vector2 resourcePos = new Vector2(resource.Position.X, -resource.Position.Y);
								CS$<>8__locals1.spriteBatch.DrawCircle(resourcePos, 100f, 6, Color.DarkGreen * 0.5f, (float)((int)(2f / this.Cam.Zoom)));
								SpriteBatch spriteBatch3 = CS$<>8__locals1.spriteBatch;
								Vector2 pos3 = resourcePos;
								string name = resource.Name;
								Color darkGreen = Color.DarkGreen;
								largeFont = GUIStyle.LargeFont;
								GUI.DrawString(spriteBatch3, pos3, name, darkGreen, null, 0, largeFont, ForceUpperCase.Inherit);
								float dist = Vector2.Distance(resourcePos, pathPointPos);
								Vector2 lineStartPos = Vector2.Lerp(resourcePos, pathPointPos, 110f / dist);
								Vector2 lineEndPos = Vector2.Lerp(pathPointPos, resourcePos, 310f / dist);
								GUI.DrawLine(CS$<>8__locals1.spriteBatch, lineStartPos, lineEndPos, Color.DarkGreen * 0.5f, 0f, (float)((int)(2f / this.Cam.Zoom)));
							}
						}
					}
					Color color = pathPoint.ShouldContainResources ? Color.DarkGreen : Color.DarkRed;
					CS$<>8__locals1.spriteBatch.DrawCircle(pathPointPos, 300f, 6, color * 0.5f, (float)((int)(2f / this.Cam.Zoom)));
					SpriteBatch spriteBatch4 = CS$<>8__locals1.spriteBatch;
					Vector2 pos4 = pathPointPos;
					string text2 = "Path Point\n" + pathPoint.Id;
					Color color2 = color;
					largeFont = GUIStyle.LargeFont;
					GUI.DrawString(spriteBatch4, pos4, text2, color2, null, 0, largeFont, ForceUpperCase.Inherit);
				}
				foreach (Level.ClusterLocation location2 in Level.Loaded.AbyssResources)
				{
					if (location2.Resources != null)
					{
						foreach (Item resource2 in location2.Resources)
						{
							Vector2 resourcePos2 = new Vector2(resource2.Position.X, -resource2.Position.Y);
							CS$<>8__locals1.spriteBatch.DrawCircle(resourcePos2, 100f, 6, Color.DarkGreen * 0.5f, (float)((int)(2f / this.Cam.Zoom)));
							SpriteBatch spriteBatch5 = CS$<>8__locals1.spriteBatch;
							Vector2 pos5 = resourcePos2;
							string name2 = resource2.Name;
							Color darkGreen2 = Color.DarkGreen;
							GUIFont largeFont = GUIStyle.LargeFont;
							GUI.DrawString(spriteBatch5, pos5, name2, darkGreen2, null, 0, largeFont, ForceUpperCase.Inherit);
						}
					}
				}
				CS$<>8__locals1.spriteBatch.End();
				if (this.lightingEnabled.Selected)
				{
					CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Immediate, CustomBlendStates.Multiplicative, null, DepthStencilState.None, null, null, null);
					CS$<>8__locals1.spriteBatch.Draw(GameMain.LightManager.LightMap, new Rectangle(0, 0, GameMain.GraphicsWidth, GameMain.GraphicsHeight), Color.White);
					CS$<>8__locals1.spriteBatch.End();
				}
			}
			if (this.editingSprite != null)
			{
				GameMain.SpriteEditorScreen.Draw(deltaTime, graphics, CS$<>8__locals1.spriteBatch);
			}
			CS$<>8__locals1.spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			if (Level.Loaded != null)
			{
				float hullUpgradePrcIncrease = (float)UpgradePrefab.CrushDepthUpgradePrc / 100f;
				for (int upgradeLevel = 0; upgradeLevel <= UpgradePrefab.IncreaseWallHealthMaxLevel; upgradeLevel++)
				{
					float upgradeLevelCrushDepth = 3500f + 3500f * (float)upgradeLevel * hullUpgradePrcIncrease;
					float subCrushDepth = upgradeLevelCrushDepth / Physics.DisplayToRealWorldRatio - (float)Level.Loaded.LevelData.InitialDepth;
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Crush depth (upgrade level ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(upgradeLevel);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					string labelText = defaultInterpolatedStringHandler.ToStringAndClear();
					if (upgradeLevel == 0)
					{
						labelText = "Crush depth (no upgrade)";
					}
					this.<Draw>g__DrawCrushDepth|54_0(subCrushDepth, labelText, Color.Red, ref CS$<>8__locals1);
				}
				float abyssStartScreen = this.Cam.WorldToScreen(new Vector2(0f, (float)Level.Loaded.AbyssArea.Bottom)).Y;
				if (abyssStartScreen > 0f && abyssStartScreen < (float)GameMain.GraphicsHeight)
				{
					GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(0f, abyssStartScreen), new Vector2((float)GameMain.GraphicsWidth, abyssStartScreen), GUIStyle.Blue * 0.25f, 0f, 5f);
					GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)(GameMain.GraphicsWidth / 2), abyssStartScreen), "Abyss start", GUIStyle.Blue, new Color?(Color.Black), 0, null, ForceUpperCase.Inherit);
				}
				float abyssEndScreen = this.Cam.WorldToScreen(new Vector2(0f, (float)Level.Loaded.AbyssArea.Y)).Y;
				if (abyssEndScreen > 0f && abyssEndScreen < (float)GameMain.GraphicsHeight)
				{
					GUI.DrawLine(CS$<>8__locals1.spriteBatch, new Vector2(0f, abyssEndScreen), new Vector2((float)GameMain.GraphicsWidth, abyssEndScreen), GUIStyle.Blue * 0.25f, 0f, 5f);
					GUI.DrawString(CS$<>8__locals1.spriteBatch, new Vector2((float)(GameMain.GraphicsWidth / 2), abyssEndScreen), "Abyss end", GUIStyle.Blue, new Color?(Color.Black), 0, null, ForceUpperCase.Inherit);
				}
			}
			GUI.Draw(this.Cam, CS$<>8__locals1.spriteBatch);
			CS$<>8__locals1.spriteBatch.End();
		}

		// Token: 0x0600255C RID: 9564 RVA: 0x001803AC File Offset: 0x0017E5AC
		public override void Update(double deltaTime)
		{
			if (GameMain.GraphicsWidth != this.prevResolution.X || GameMain.GraphicsHeight != this.prevResolution.Y)
			{
				this.RefreshUI(true);
			}
			if (this.lightingEnabled.Selected)
			{
				foreach (Item item in Item.ItemList)
				{
					if (item != null && !item.IsHidden)
					{
						foreach (LightComponent light in item.GetComponents<LightComponent>())
						{
							light.Update((float)deltaTime, this.Cam);
						}
					}
				}
			}
			LightManager lightManager = GameMain.LightManager;
			if (lightManager != null)
			{
				lightManager.Update((float)deltaTime);
			}
			this.pointerLightSource.Position = this.Cam.ScreenToWorld(PlayerInput.MousePosition);
			this.pointerLightSource.Enabled = this.cursorLightEnabled.Selected;
			this.pointerLightSource.IsBackground = true;
			this.Cam.MoveCamera((float)deltaTime, true, GUI.MouseOn == null, true, null);
			this.Cam.UpdateTransform(true, true);
			Level loaded = Level.Loaded;
			if (loaded != null)
			{
				loaded.Update((float)deltaTime, this.Cam);
			}
			if (this.editingSprite != null)
			{
				GameMain.SpriteEditorScreen.Update(deltaTime);
			}
			if (Level.ForcedDifficulty != null && MathHelper.Distance(Level.ForcedDifficulty.Value, this.forceDifficultyInput.FloatValue) > 0.001f)
			{
				this.forceDifficultyInput.FloatValue = Level.ForcedDifficulty.Value;
			}
		}

		// Token: 0x0600255D RID: 9565 RVA: 0x0018056C File Offset: 0x0017E76C
		private void SerializeAll()
		{
			IEnumerable<ContentPackage> packages = ContentPackageManager.LocalPackages;
			XmlWriterSettings settings = new XmlWriterSettings
			{
				Indent = true,
				NewLineOnAttributes = true
			};
			foreach (LevelGenerationParametersFile configFile in packages.SelectMany((ContentPackage p) => p.GetFiles<LevelGenerationParametersFile>()))
			{
				XDocument doc = XMLExtensions.TryLoadXml(configFile.Path);
				if (doc != null)
				{
					foreach (LevelGenerationParams genParams in LevelGenerationParams.LevelParams)
					{
						foreach (XElement element in doc.Root.Elements())
						{
							if (element.IsOverride())
							{
								using (IEnumerator<XElement> enumerator4 = element.Elements().GetEnumerator())
								{
									while (enumerator4.MoveNext())
									{
										XElement subElement = enumerator4.Current;
										string id = element.GetAttributeString("identifier", null) ?? element.Name.ToString();
										if (id.Equals(genParams.Name, StringComparison.OrdinalIgnoreCase))
										{
											SerializableProperty.SerializeProperties(genParams, element, true, false);
										}
									}
									break;
								}
							}
							string id2 = element.GetAttributeString("identifier", null) ?? element.Name.ToString();
							if (id2.Equals(genParams.Name, StringComparison.OrdinalIgnoreCase))
							{
								SerializableProperty.SerializeProperties(genParams, element, true, false);
								break;
							}
						}
					}
					using (XmlWriter writer = XmlWriter.Create(configFile.Path.Value, settings))
					{
						doc.WriteTo(writer);
						writer.Flush();
					}
				}
			}
			foreach (CaveGenerationParametersFile configFile2 in packages.SelectMany((ContentPackage p) => p.GetFiles<CaveGenerationParametersFile>()))
			{
				XDocument doc2 = XMLExtensions.TryLoadXml(configFile2.Path);
				if (doc2 != null)
				{
					foreach (CaveGenerationParams genParams2 in CaveGenerationParams.CaveParams)
					{
						foreach (XElement element2 in doc2.Root.Elements())
						{
							if (element2.IsOverride())
							{
								using (IEnumerator<XElement> enumerator8 = element2.Elements().GetEnumerator())
								{
									while (enumerator8.MoveNext())
									{
										XElement subElement2 = enumerator8.Current;
										string id3 = subElement2.GetAttributeString("identifier", null) ?? subElement2.Name.ToString();
										if (id3.Equals(genParams2.Name, StringComparison.OrdinalIgnoreCase))
										{
											genParams2.Save(subElement2);
										}
									}
									break;
								}
							}
							string id4 = element2.GetAttributeString("identifier", null) ?? element2.Name.ToString();
							if (id4.Equals(genParams2.Name, StringComparison.OrdinalIgnoreCase))
							{
								genParams2.Save(element2);
								break;
							}
						}
					}
					using (XmlWriter writer2 = XmlWriter.Create(configFile2.Path.Value, settings))
					{
						doc2.WriteTo(writer2);
						writer2.Flush();
					}
				}
			}
			settings.NewLineOnAttributes = false;
			foreach (LevelObjectPrefabsFile configFile3 in packages.SelectMany((ContentPackage p) => p.GetFiles<LevelObjectPrefabsFile>()))
			{
				XDocument doc3 = XMLExtensions.TryLoadXml(configFile3.Path);
				if (doc3 != null)
				{
					foreach (LevelObjectPrefab levelObjPrefab in LevelObjectPrefab.Prefabs)
					{
						foreach (XElement element3 in doc3.Root.Elements())
						{
							Identifier identifier = element3.GetAttributeIdentifier("identifier", "");
							if (!(identifier != levelObjPrefab.Identifier))
							{
								levelObjPrefab.Save(element3);
								break;
							}
						}
					}
					using (XmlWriter writer3 = XmlWriter.Create(configFile3.Path.Value, settings))
					{
						doc3.WriteTo(writer3);
						writer3.Flush();
					}
				}
			}
			RuinGenerationParams.SaveAll();
		}

		// Token: 0x0600255E RID: 9566 RVA: 0x00180B8C File Offset: 0x0017ED8C
		private void Serialize(LevelGenerationParams genParams)
		{
			foreach (LevelGenerationParametersFile configFile in ContentPackageManager.AllPackages.SelectMany((ContentPackage p) => p.GetFiles<LevelGenerationParametersFile>()))
			{
				XDocument doc = XMLExtensions.TryLoadXml(configFile.Path);
				if (doc != null)
				{
					bool elementFound = false;
					foreach (XElement element in doc.Root.Elements())
					{
						string id = element.GetAttributeString("identifier", null) ?? element.Name.ToString();
						if (id.Equals(genParams.Name, StringComparison.OrdinalIgnoreCase))
						{
							SerializableProperty.SerializeProperties(genParams, element, true, false);
							elementFound = true;
						}
					}
					if (elementFound)
					{
						XmlWriterSettings settings = new XmlWriterSettings
						{
							Indent = true,
							NewLineOnAttributes = true
						};
						using (XmlWriter writer = XmlWriter.Create(configFile.Path.Value, settings))
						{
							doc.WriteTo(writer);
							writer.Flush();
							break;
						}
					}
				}
			}
		}

		// Token: 0x0600255F RID: 9567 RVA: 0x00180CE0 File Offset: 0x0017EEE0
		[CompilerGenerated]
		internal static string <CreateUI>g__GetLevelSeed|35_3()
		{
			return ToolBox.RandomSeed(8);
		}

		// Token: 0x06002560 RID: 9568 RVA: 0x00180CE8 File Offset: 0x0017EEE8
		[CompilerGenerated]
		private void <Draw>g__DrawCrushDepth|54_0(float crushDepth, string labelText, Color color, ref LevelEditorScreen.<>c__DisplayClass54_0 A_4)
		{
			float crushDepthScreen = this.Cam.WorldToScreen(new Vector2(0f, -crushDepth)).Y;
			if (crushDepthScreen > 0f && crushDepthScreen < (float)GameMain.GraphicsHeight)
			{
				GUI.DrawLine(A_4.spriteBatch, new Vector2(0f, crushDepthScreen), new Vector2((float)GameMain.GraphicsWidth, crushDepthScreen), color * 0.25f, 0f, 5f);
				GUI.DrawString(A_4.spriteBatch, new Vector2((float)(GameMain.GraphicsWidth / 2), crushDepthScreen), labelText, GUIStyle.Red, new Color?(Color.Black), 0, null, ForceUpperCase.Inherit);
			}
		}

		// Token: 0x04001293 RID: 4755
		private GUIFrame leftPanel;

		// Token: 0x04001294 RID: 4756
		private GUIFrame rightPanel;

		// Token: 0x04001295 RID: 4757
		private GUIFrame bottomPanel;

		// Token: 0x04001296 RID: 4758
		private GUIFrame topPanel;

		// Token: 0x04001297 RID: 4759
		private Point prevResolution;

		// Token: 0x04001298 RID: 4760
		private LevelGenerationParams selectedParams;

		// Token: 0x04001299 RID: 4761
		private RuinGenerationParams selectedRuinGenerationParams;

		// Token: 0x0400129A RID: 4762
		private OutpostGenerationParams selectedOutpostGenerationParams;

		// Token: 0x0400129B RID: 4763
		private LevelObjectPrefab selectedLevelObject;

		// Token: 0x0400129C RID: 4764
		private BackgroundCreaturePrefab selectedBackgroundCreature;

		// Token: 0x0400129D RID: 4765
		private GUIListBox paramsList;

		// Token: 0x0400129E RID: 4766
		private GUIListBox ruinParamsList;

		// Token: 0x0400129F RID: 4767
		private GUIListBox caveParamsList;

		// Token: 0x040012A0 RID: 4768
		private GUIListBox outpostParamsList;

		// Token: 0x040012A1 RID: 4769
		private GUIListBox levelObjectList;

		// Token: 0x040012A2 RID: 4770
		private GUIListBox backgroundCreatureList;

		// Token: 0x040012A3 RID: 4771
		private GUIListBox editorContainer;

		// Token: 0x040012A4 RID: 4772
		private GUIButton spriteEditDoneButton;

		// Token: 0x040012A5 RID: 4773
		private GUITextBox seedBox;

		// Token: 0x040012A6 RID: 4774
		private GUITickBox lightingEnabled;

		// Token: 0x040012A7 RID: 4775
		private GUITickBox cursorLightEnabled;

		// Token: 0x040012A8 RID: 4776
		private GUITickBox allowInvalidOutpost;

		// Token: 0x040012A9 RID: 4777
		private GUITickBox mirrorLevel;

		// Token: 0x040012AA RID: 4778
		private GUIDropDown selectedSubDropDown;

		// Token: 0x040012AB RID: 4779
		private GUIDropDown selectedBeaconStationDropdown;

		// Token: 0x040012AC RID: 4780
		private GUIDropDown selectedWreckDropdown;

		// Token: 0x040012AD RID: 4781
		private GUINumberInput forceDifficultyInput;

		// Token: 0x040012AE RID: 4782
		private Sprite editingSprite;

		// Token: 0x040012AF RID: 4783
		private LightSource pointerLightSource;

		// Token: 0x040012B0 RID: 4784
		private readonly Color[] tunnelDebugColors = new Color[]
		{
			Color.White,
			Color.Cyan,
			Color.LightGreen,
			Color.Red,
			Color.LightYellow,
			Color.LightSeaGreen
		};

		// Token: 0x040012B1 RID: 4785
		private LevelData currentLevelData;

		// Token: 0x02000C44 RID: 3140
		private class Wizard
		{
			// Token: 0x17001AC8 RID: 6856
			// (get) Token: 0x06007B8E RID: 31630 RVA: 0x003844B9 File Offset: 0x003826B9
			public static LevelEditorScreen.Wizard Instance
			{
				get
				{
					if (LevelEditorScreen.Wizard.instance == null)
					{
						LevelEditorScreen.Wizard.instance = new LevelEditorScreen.Wizard();
					}
					return LevelEditorScreen.Wizard.instance;
				}
			}

			// Token: 0x06007B8F RID: 31631 RVA: 0x003844D1 File Offset: 0x003826D1
			public void AddToGUIUpdateList()
			{
			}

			// Token: 0x06007B90 RID: 31632 RVA: 0x003844D4 File Offset: 0x003826D4
			public GUIMessageBox Create()
			{
				GUIMessageBox box = new GUIMessageBox(TextManager.Get("leveleditor.createlevelobj"), string.Empty, new LocalizedString[]
				{
					TextManager.Get("cancel"),
					TextManager.Get("done")
				}, new Vector2?(new Vector2(0.5f, 0.8f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				box.Content.ChildAnchor = Anchor.TopCenter;
				box.Content.AbsoluteSpacing = 20;
				int elementSize = 30;
				GUIListBox listBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.75f), box.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
				new GUITextBlock(new RectTransform(new Point(listBox.Content.Rect.Width, elementSize), listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), TextManager.Get("leveleditor.levelobjname"), null, null, Alignment.Left, false, "", null).CanBeFocused = false;
				GUITextBox nameBox = new GUITextBox(new RectTransform(new Point(listBox.Content.Rect.Width, elementSize), listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", null, null, Alignment.Left, false, "", null, false, true);
				new GUITextBlock(new RectTransform(new Point(listBox.Content.Rect.Width, elementSize), listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), TextManager.Get("leveleditor.levelobjtexturepath"), null, null, Alignment.Left, false, "", null).CanBeFocused = false;
				GUITextBox texturePathBox = new GUITextBox(new RectTransform(new Point(listBox.Content.Rect.Width, elementSize), listBox.Content.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), "", null, null, Alignment.Left, false, "", null, false, true);
				foreach (LevelObjectPrefab prefab in LevelObjectPrefab.Prefabs)
				{
					if (prefab.Sprites.FirstOrDefault<Sprite>() != null)
					{
						texturePathBox.Text = Path.GetDirectoryName(prefab.Sprites.FirstOrDefault<Sprite>().FilePath.Value);
						break;
					}
				}
				this.newPrefab = new LevelObjectPrefab(null, null, new Identifier("No identifier"));
				new SerializableEntityEditor(listBox.Content.RectTransform, this.newPrefab, false, false, "", 24, null, true);
				GUIButton guibutton = box.Buttons[0];
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					box.Close();
					return true;
				}));
				GUIButton guibutton2 = box.Buttons[1];
				Func<LevelObjectPrefab, bool> <>9__2;
				Func<RegularPackage, bool> <>9__3;
				guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					if (string.IsNullOrEmpty(nameBox.Text))
					{
						nameBox.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
						GUI.AddMessage(TextManager.Get("leveleditor.levelobjnameempty"), GUIStyle.Red, null, true, null);
						return false;
					}
					IEnumerable<LevelObjectPrefab> prefabs = LevelObjectPrefab.Prefabs;
					Func<LevelObjectPrefab, bool> predicate;
					if ((predicate = <>9__2) == null)
					{
						predicate = (<>9__2 = ((LevelObjectPrefab obj) => obj.Identifier == nameBox.Text));
					}
					if (prefabs.Any(predicate))
					{
						nameBox.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
						GUI.AddMessage(TextManager.Get("leveleditor.levelobjnametaken"), GUIStyle.Red, null, true, null);
						return false;
					}
					if (!File.Exists(texturePathBox.Text))
					{
						texturePathBox.Flash(new Color?(GUIStyle.Red), 1.5f, false, false, null);
						GUI.AddMessage(TextManager.Get("leveleditor.levelobjtexturenotfound"), GUIStyle.Red, null, true, null);
						return false;
					}
					XmlWriterSettings settings = new XmlWriterSettings
					{
						Indent = true
					};
					XElement newElement = new XElement(nameBox.Text);
					this.newPrefab.Save(newElement);
					newElement.Add(new XElement("Sprite", new object[]
					{
						new XAttribute("texture", texturePathBox.Text),
						new XAttribute("sourcerect", "0,0,100,100"),
						new XAttribute("origin", "0.5,0.5")
					}));
					string modDir = Path.Combine(new string[]
					{
						"LocalMods",
						nameBox.Text
					});
					Directory.CreateDirectory(modDir, false);
					string fileListPath = Path.Combine(new string[]
					{
						modDir,
						"filelist.xml"
					});
					string prefabFilePath = Path.Combine(new string[]
					{
						modDir,
						nameBox.Text + ".xml"
					});
					ModProject newMod = new ModProject
					{
						Name = nameBox.Text
					};
					ModProject.File newFile = ModProject.File.FromPath<LevelObjectPrefabsFile>(prefabFilePath);
					newMod.AddFile(newFile);
					XDocument fileListDoc = newMod.ToXDocument();
					Directory.CreateDirectory(Path.GetDirectoryName(fileListPath), false);
					using (XmlWriter writer = XmlWriter.Create(fileListPath, settings))
					{
						fileListDoc.Save(writer);
					}
					XDocument prefabDoc = new XDocument();
					XElement prefabFileRoot = new XElement("LevelObjects");
					prefabFileRoot.Add(newElement);
					prefabDoc.Add(prefabFileRoot);
					using (XmlWriter writer2 = XmlWriter.Create(prefabFilePath, settings))
					{
						prefabDoc.Save(writer2);
					}
					ContentPackageManager.UpdateContentPackageList();
					List<RegularPackage> newRegularList = ContentPackageManager.EnabledPackages.Regular.ToList<RegularPackage>();
					List<RegularPackage> list = newRegularList;
					IEnumerable<RegularPackage> regularPackages = ContentPackageManager.RegularPackages;
					Func<RegularPackage, bool> predicate2;
					if ((predicate2 = <>9__3) == null)
					{
						predicate2 = (<>9__3 = ((RegularPackage p) => p.Name == nameBox.Text));
					}
					list.Add(regularPackages.First(predicate2));
					ContentPackageManager.EnabledPackages.SetRegular(newRegularList);
					GameMain.LevelEditorScreen.UpdateLevelObjectsList();
					box.Close();
					return true;
				}));
				return box;
			}

			// Token: 0x04004A84 RID: 19076
			private LevelObjectPrefab newPrefab;

			// Token: 0x04004A85 RID: 19077
			private static LevelEditorScreen.Wizard instance;
		}
	}
}
