using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma
{
	// Token: 0x02000112 RID: 274
	[NullableContext(1)]
	[Nullable(0)]
	internal class EventEditorScreen : EditorScreen
	{
		// Token: 0x170009F7 RID: 2551
		// (get) Token: 0x06002504 RID: 9476 RVA: 0x00175AE1 File Offset: 0x00173CE1
		public override Camera Cam { get; }

		// Token: 0x170009F8 RID: 2552
		// (get) Token: 0x06002505 RID: 9477 RVA: 0x00175AE9 File Offset: 0x00173CE9
		// (set) Token: 0x06002506 RID: 9478 RVA: 0x00175AF0 File Offset: 0x00173CF0
		[Nullable(2)]
		public static string DrawnTooltip { [NullableContext(2)] get; [NullableContext(2)] set; }

		// Token: 0x170009F9 RID: 2553
		// (get) Token: 0x06002507 RID: 9479 RVA: 0x00175AF8 File Offset: 0x00173CF8
		// (set) Token: 0x06002508 RID: 9480 RVA: 0x00175AFF File Offset: 0x00173CFF
		public static bool ConversationMode { get; set; }

		// Token: 0x06002509 RID: 9481 RVA: 0x00175B08 File Offset: 0x00173D08
		private static int CreateID()
		{
			int num;
			if (!EventEditorScreen.nodeList.Any<EditorNode>())
			{
				num = 0;
			}
			else
			{
				num = EventEditorScreen.nodeList.Max((EditorNode node) => node.ID);
			}
			int maxId = num;
			return maxId + 1;
		}

		// Token: 0x0600250A RID: 9482 RVA: 0x00175B54 File Offset: 0x00173D54
		public EventEditorScreen()
		{
			this.Cam = new Camera();
			EventEditorScreen.nodeList.Clear();
			this.originalLanguage = GameSettings.CurrentConfig.Language;
			this.CreateGUI();
		}

		// Token: 0x0600250B RID: 9483 RVA: 0x00175BA8 File Offset: 0x00173DA8
		public override void Select()
		{
			GUI.PreventPauseMenuToggle = false;
			EventEditorScreen.projectName = TextManager.Get("EventEditor.Unnamed").Value;
			this.UpdateLanguageDropdownSelection();
			base.Select();
		}

		// Token: 0x0600250C RID: 9484 RVA: 0x00175BD0 File Offset: 0x00173DD0
		private void UpdateLanguageDropdownSelection()
		{
			if (this.languageDropdown == null)
			{
				return;
			}
			this.languageDropdown.SelectItem(GameSettings.CurrentConfig.Language);
		}

		// Token: 0x0600250D RID: 9485 RVA: 0x00175BF8 File Offset: 0x00173DF8
		protected unsafe override void DeselectEditorSpecific()
		{
			GameSettings.Config config = *GameSettings.CurrentConfig;
			config.Language = this.originalLanguage;
			GameSettings.SetCurrentConfig(config);
			TextManager.LanguageChanged();
		}

		// Token: 0x0600250E RID: 9486 RVA: 0x00175C29 File Offset: 0x00173E29
		private static bool ShouldHideNodeInConversationMode(EditorNode node)
		{
			return EventEditorScreen.hiddenNodesInConversationMode.Contains(node);
		}

		// Token: 0x0600250F RID: 9487 RVA: 0x00175C38 File Offset: 0x00173E38
		private static void UpdateHiddenNodesInConversationMode()
		{
			EventEditorScreen.hiddenNodesInConversationMode.Clear();
			IEnumerable<EditorNode> source = EventEditorScreen.nodeList;
			Func<EditorNode, bool> predicate;
			if ((predicate = EventEditorScreen.<>O.<0>__IsEventTextDisplayNode) == null)
			{
				predicate = (EventEditorScreen.<>O.<0>__IsEventTextDisplayNode = new Func<EditorNode, bool>(EventEditorScreen.IsEventTextDisplayNode));
			}
			foreach (EditorNode textDisplayNode in source.Where(predicate))
			{
				EventEditorNodeConnection addConnection = textDisplayNode.Connections.FirstOrDefault((EventEditorNodeConnection c) => c.Type == NodeConnectionType.Add);
				if (addConnection != null && addConnection.ConnectedTo.Any<EventEditorNodeConnection>())
				{
					foreach (EventEditorNodeConnection connectedNode in addConnection.ConnectedTo)
					{
						EditorNode parent = connectedNode.Parent;
						if (((parent != null) ? parent.Name : null) == "Text")
						{
							EventEditorScreen.MarkNodeAndDescendantsAsHidden(connectedNode.Parent);
						}
					}
				}
			}
		}

		// Token: 0x06002510 RID: 9488 RVA: 0x00175D50 File Offset: 0x00173F50
		private static bool IsEventTextDisplayNode(EditorNode node)
		{
			return node is EventTextDisplayNode;
		}

		// Token: 0x06002511 RID: 9489 RVA: 0x00175D5C File Offset: 0x00173F5C
		private static void MarkNodeAndDescendantsAsHidden(EditorNode node)
		{
			EventEditorScreen.hiddenNodesInConversationMode.Add(node);
			foreach (EventEditorNodeConnection connection in node.Connections)
			{
				foreach (EventEditorNodeConnection connectedNode in connection.ConnectedTo)
				{
					if (connectedNode.Parent != null && !EventEditorScreen.hiddenNodesInConversationMode.Contains(connectedNode.Parent))
					{
						EventEditorScreen.MarkNodeAndDescendantsAsHidden(connectedNode.Parent);
					}
				}
			}
		}

		// Token: 0x06002512 RID: 9490 RVA: 0x00175E14 File Offset: 0x00174014
		private unsafe void CreateGUI()
		{
			this.GuiFrame = new GUIFrame(new RectTransform(new Vector2(0.2f, 0.5f), GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				MinSize = new Point(300, 520)
			}, "", null);
			GUILayoutGroup layoutGroup = new GUILayoutGroup(EventEditorScreen.RectTransform(0.9f, 0.9f, this.GuiFrame, Anchor.Center), false, Anchor.TopLeft)
			{
				Stretch = true,
				AbsoluteSpacing = GUI.IntScale(5f)
			};
			GUILayoutGroup buttonLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.4f, layoutGroup, Anchor.TopRight), false, Anchor.TopLeft)
			{
				RelativeSpacing = 0.04f
			};
			GUIButton newProjectButton = new GUIButton(EventEditorScreen.RectTransform(1f, 0.25f, buttonLayout, Anchor.TopRight), TextManager.Get("EventEditor.NewProject"), Alignment.Center, "", null);
			GUIButton saveProjectButton = new GUIButton(EventEditorScreen.RectTransform(1f, 0.25f, buttonLayout, Anchor.TopRight), TextManager.Get("EventEditor.SaveProject"), Alignment.Center, "", null);
			GUIButton loadProjectButton = new GUIButton(EventEditorScreen.RectTransform(1f, 0.25f, buttonLayout, Anchor.TopRight), TextManager.Get("EventEditor.LoadProject"), Alignment.Center, "", null);
			GUIButton exportProjectButton = new GUIButton(EventEditorScreen.RectTransform(1f, 0.25f, buttonLayout, Anchor.TopRight), TextManager.Get("EventEditor.Export"), Alignment.Center, "", null);
			GUILayoutGroup loadEventLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.1f, layoutGroup, Anchor.TopRight), false, Anchor.TopLeft);
			RectTransform rectT = EventEditorScreen.RectTransform(1f, 0.5f, loadEventLayout, Anchor.TopRight);
			RichString text = TextManager.Get("EventEditor.LoadEvent");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
			GUILayoutGroup loadDropdownLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.5f, loadEventLayout, Anchor.TopRight), true, Anchor.CenterLeft);
			GUIDropDown loadDropdown = new GUIDropDown(EventEditorScreen.RectTransform(0.8f, 1f, loadDropdownLayout, Anchor.TopRight), null, 10, "", false, false, Alignment.CenterLeft, 1f);
			GUIButton loadButton = new GUIButton(EventEditorScreen.RectTransform(0.2f, 1f, loadDropdownLayout, Anchor.TopRight), TextManager.Get("Load"), Alignment.Center, "", null);
			GUILayoutGroup addActionLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.1f, layoutGroup, Anchor.TopRight), false, Anchor.TopLeft);
			RectTransform rectT2 = EventEditorScreen.RectTransform(1f, 0.5f, addActionLayout, Anchor.TopRight);
			RichString text2 = TextManager.Get("EventEditor.AddAction");
			subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, subHeadingFont, Alignment.Left, false, "", null);
			GUILayoutGroup addActionDropdownLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.5f, addActionLayout, Anchor.TopRight), true, Anchor.CenterLeft);
			GUIDropDown addActionDropdown = new GUIDropDown(EventEditorScreen.RectTransform(0.8f, 1f, addActionDropdownLayout, Anchor.TopRight), null, 10, "", false, false, Alignment.CenterLeft, 1f);
			GUIButton addActionButton = new GUIButton(EventEditorScreen.RectTransform(0.2f, 1f, addActionDropdownLayout, Anchor.TopRight), TextManager.Get("EventEditor.Add"), Alignment.Center, "", null);
			GUILayoutGroup addValueLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.1f, layoutGroup, Anchor.TopRight), false, Anchor.TopLeft);
			RectTransform rectT3 = EventEditorScreen.RectTransform(1f, 0.5f, addValueLayout, Anchor.TopRight);
			RichString text3 = TextManager.Get("EventEditor.AddValue");
			subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT3, text3, null, subHeadingFont, Alignment.Left, false, "", null);
			GUILayoutGroup addValueDropdownLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.5f, addValueLayout, Anchor.TopRight), true, Anchor.CenterLeft);
			GUIDropDown addValueDropdown = new GUIDropDown(EventEditorScreen.RectTransform(0.8f, 1f, addValueDropdownLayout, Anchor.TopRight), null, 7, "", false, false, Alignment.CenterLeft, 1f);
			GUIButton addValueButton = new GUIButton(EventEditorScreen.RectTransform(0.2f, 1f, addValueDropdownLayout, Anchor.TopRight), TextManager.Get("EventEditor.Add"), Alignment.Center, "", null);
			GUILayoutGroup addSpecialLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.1f, layoutGroup, Anchor.TopRight), false, Anchor.TopLeft);
			RectTransform rectT4 = EventEditorScreen.RectTransform(1f, 0.5f, addSpecialLayout, Anchor.TopRight);
			RichString text4 = TextManager.Get("EventEditor.AddSpecial");
			subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT4, text4, null, subHeadingFont, Alignment.Left, false, "", null);
			GUILayoutGroup addSpecialDropdownLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.5f, addSpecialLayout, Anchor.TopRight), true, Anchor.CenterLeft);
			GUIDropDown addSpecialDropdown = new GUIDropDown(EventEditorScreen.RectTransform(0.8f, 1f, addSpecialDropdownLayout, Anchor.TopRight), null, 1, "", false, false, Alignment.CenterLeft, 1f);
			GUIButton addSpecialButton = new GUIButton(EventEditorScreen.RectTransform(0.2f, 1f, addSpecialDropdownLayout, Anchor.TopRight), TextManager.Get("EventEditor.Add"), Alignment.Center, "", null);
			foreach (EventPrefab eventPrefab in from p in (from p in EventSet.GetAllEventPrefabs()
			where !p.Identifier.IsEmpty
			select p).Distinct<EventPrefab>()
			orderby p.Identifier
			select p)
			{
				if (typeof(ScriptedEvent).IsAssignableFrom(eventPrefab.EventType))
				{
					GUITextBlock textBlock = loadDropdown.AddItem(eventPrefab.Identifier.Value, eventPrefab, null, null, null) as GUITextBlock;
					if (eventPrefab is TraitorEventPrefab && textBlock != null)
					{
						textBlock.TextColor = Color.MediumPurple;
					}
				}
			}
			foreach (Type type2 in from type in Assembly.GetExecutingAssembly().GetTypes()
			where type.IsSubclassOf(typeof(EventAction))
			select type into t
			orderby t.Name
			select t)
			{
				addActionDropdown.AddItem(type2.Name, type2, null, null, null);
			}
			addSpecialDropdown.AddItem("Custom", typeof(CustomNode), null, null, null);
			addValueDropdown.AddItem("Single", typeof(float), null, null, null);
			addValueDropdown.AddItem("Boolean", typeof(bool), null, null, null);
			addValueDropdown.AddItem("String", typeof(string), null, null, null);
			addValueDropdown.AddItem("SpawnType", typeof(SpawnType), null, null, null);
			addValueDropdown.AddItem("LimbType", typeof(LimbType), null, null, null);
			addValueDropdown.AddItem("ReputationType", typeof(ReputationAction.ReputationType), null, null, null);
			addValueDropdown.AddItem("SpawnLocationType", typeof(SpawnAction.SpawnLocationType), null, null, null);
			addValueDropdown.AddItem("CharacterTeamType", typeof(CharacterTeamType), null, null, null);
			GUIButton guibutton = loadButton;
			guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler((GUIButton button, object o) => this.Load(loadDropdown.SelectedData as EventPrefab)));
			GUIButton guibutton2 = addActionButton;
			guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler((GUIButton button, object o) => this.AddAction(addActionDropdown.SelectedData as Type)));
			GUIButton guibutton3 = addValueButton;
			guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler((GUIButton button, object o) => this.AddValue(addValueDropdown.SelectedData as Type)));
			GUIButton guibutton4 = addSpecialButton;
			guibutton4.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton4.OnClicked, new GUIButton.OnClickedHandler((GUIButton button, object o) => this.AddSpecial(addSpecialDropdown.SelectedData as Type)));
			GUIButton guibutton5 = exportProjectButton;
			guibutton5.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton5.OnClicked, new GUIButton.OnClickedHandler(this.ExportEventToFile));
			GUIButton guibutton6 = saveProjectButton;
			guibutton6.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton6.OnClicked, new GUIButton.OnClickedHandler(this.SaveProjectToFile));
			GUIButton guibutton7 = newProjectButton;
			guibutton7.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton7.OnClicked, new GUIButton.OnClickedHandler(this.TryCreateNewProject));
			GUIButton guibutton8 = loadProjectButton;
			guibutton8.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton8.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton button, object o)
			{
				FileSelection.OnFileSelected = delegate(string file)
				{
					XDocument document = XMLExtensions.TryLoadXml(file);
					if (((document != null) ? document.Root : null) != null)
					{
						EventEditorScreen.Load(document.Root);
					}
				};
				string directory = Path.GetFullPath("EventProjects");
				if (!Directory.Exists(directory))
				{
					Directory.CreateDirectory(directory);
				}
				FileSelection.ClearFileTypeFilters();
				FileSelection.AddFileTypeFilter("Scripted Event", "*.sevproj");
				FileSelection.SelectFileTypeFilter("*.sevproj");
				FileSelection.CurrentDirectory = directory;
				FileSelection.Open = true;
				return true;
			}));
			this.isTraitorEventBox = new GUITickBox(EventEditorScreen.RectTransform(1f, 0.1f, layoutGroup, Anchor.TopRight), "Traitor event", null, "");
			this.conversationModeBox = new GUITickBox(EventEditorScreen.RectTransform(1f, 0.1f, layoutGroup, Anchor.TopRight), "Conversation Mode", null, "");
			this.conversationModeBox.Selected = EventEditorScreen.ConversationMode;
			this.conversationModeBox.OnSelected = delegate(GUITickBox box)
			{
				EventEditorScreen.ConversationMode = !EventEditorScreen.ConversationMode;
				EventEditorScreen.UpdateHiddenNodesInConversationMode();
				return true;
			};
			GUILayoutGroup languageLayout = new GUILayoutGroup(EventEditorScreen.RectTransform(1f, 0.1f, layoutGroup, Anchor.TopRight), false, Anchor.TopLeft);
			RectTransform rectT5 = EventEditorScreen.RectTransform(1f, 0.5f, languageLayout, Anchor.TopRight);
			RichString text5 = TextManager.Get("Language");
			subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT5, text5, null, subHeadingFont, Alignment.Left, false, "", null);
			IOrderedEnumerable<LanguageIdentifier> languages = from l in TextManager.AvailableLanguages
			orderby TextManager.GetTranslatedLanguageName(l).ToIdentifier()
			select l;
			this.languageDropdown = new GUIDropDown(EventEditorScreen.RectTransform(1f, 0.5f, languageLayout, Anchor.TopRight), null, 10, "", false, false, Alignment.CenterLeft, 1f);
			foreach (LanguageIdentifier language in languages)
			{
				this.languageDropdown.AddItem(TextManager.GetTranslatedLanguageName(language), language, null, null, null);
			}
			this.languageDropdown.SelectItem(GameSettings.CurrentConfig.Language);
			this.languageDropdown.OnSelected = delegate(GUIComponent component, object userData)
			{
				if (userData is LanguageIdentifier)
				{
					LanguageIdentifier selectedLanguage = (LanguageIdentifier)userData;
					GameSettings.Config config = *GameSettings.CurrentConfig;
					config.Language = selectedLanguage;
					GameSettings.SetCurrentConfig(config);
					TextManager.LanguageChanged();
				}
				return true;
			};
			this.screenResolution = new Point(GameMain.GraphicsWidth, GameMain.GraphicsHeight);
		}

		// Token: 0x06002513 RID: 9491 RVA: 0x00176A08 File Offset: 0x00174C08
		private bool ExportEventToFile(GUIButton button, object o)
		{
			XElement save = this.ExportXML();
			if (save != null)
			{
				try
				{
					string directory = Path.GetFullPath("EventProjects");
					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}
					string exportPath = Path.Combine(new string[]
					{
						directory,
						"Exported"
					});
					if (!Directory.Exists(exportPath))
					{
						Directory.CreateDirectory(exportPath);
					}
					GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("EventEditor.ExportProjectPrompt"), "", new LocalizedString[]
					{
						TextManager.Get("Cancel"),
						TextManager.Get("EventEditor.Export")
					}, new Vector2?(new Vector2(0.2f, 0.175f)), new Point?(new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
					GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
					GUITextBox nameInput = new GUITextBox(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
					{
						Text = EventEditorScreen.projectName
					};
					msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
					{
						msgBox.Close();
						return true;
					};
					msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
					{
						foreach (char illegalChar in Path.GetInvalidFileNameCharsCrossPlatform())
						{
							if (nameInput.Text.Contains(illegalChar))
							{
								GUI.AddMessage(TextManager.GetWithVariable("SubNameIllegalCharsWarning", "[illegalchar]", illegalChar.ToString(), FormatCapitals.No), GUIStyle.Red, null, true, null);
								return false;
							}
						}
						msgBox.Close();
						string path = Path.Combine(new string[]
						{
							exportPath,
							nameInput.Text + ".xml"
						});
						File.WriteAllText(path, save.ToString(), null, false);
						EventEditorScreen.AskForConfirmation(TextManager.Get("EventEditor.OpenTextHeader"), TextManager.Get("EventEditor.OpenTextBody"), delegate
						{
							ToolBox.OpenFileWithShell(path);
							return true;
						}, null);
						GUI.AddMessage("XML exported to " + path, GUIStyle.Green, null, true, null);
						return true;
					};
					return true;
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Failed to export event", e, null, false, false);
					return true;
				}
			}
			GUI.AddMessage("Unable to export because the project contains errors", GUIStyle.Red, null, true, null);
			return true;
		}

		// Token: 0x06002514 RID: 9492 RVA: 0x00176C48 File Offset: 0x00174E48
		private bool TryCreateNewProject(GUIButton button, object o)
		{
			EventEditorScreen.AskForConfirmation(TextManager.Get("EventEditor.NewProject"), TextManager.Get("EventEditor.NewProjectPrompt"), delegate
			{
				EventEditorScreen.nodeList.Clear();
				this.markedNodes.Clear();
				this.selectedNodes.Clear();
				EventEditorScreen.projectName = TextManager.Get("EventEditor.Unnamed").Value;
				return true;
			}, null);
			return true;
		}

		// Token: 0x06002515 RID: 9493 RVA: 0x00176C88 File Offset: 0x00174E88
		public static GUIMessageBox AskForConfirmation(LocalizedString header, LocalizedString body, Func<bool> onConfirm, GUISoundType? overrideConfirmButtonSound = null)
		{
			LocalizedString[] buttons = new LocalizedString[]
			{
				TextManager.Get("Ok"),
				TextManager.Get("Cancel")
			};
			GUIMessageBox msgBox = new GUIMessageBox(header, body, buttons, null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				msgBox.Close();
				return true;
			};
			msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				onConfirm();
				msgBox.Close();
				return true;
			};
			if (overrideConfirmButtonSound != null)
			{
				msgBox.Buttons[0].ClickSound = overrideConfirmButtonSound.Value;
			}
			return msgBox;
		}

		// Token: 0x06002516 RID: 9494 RVA: 0x00176D70 File Offset: 0x00174F70
		private bool SaveProjectToFile(GUIButton button, object o)
		{
			string directory = Path.GetFullPath("EventProjects");
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("EventEditor.NameFilePrompt"), "", new LocalizedString[]
			{
				TextManager.Get("Cancel"),
				TextManager.Get("Save")
			}, new Vector2?(new Vector2(0.2f, 0.175f)), new Point?(new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.25f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUITextBox nameInput = new GUITextBox(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				Text = EventEditorScreen.projectName
			};
			msgBox.Buttons[0].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				msgBox.Close();
				return true;
			};
			msgBox.Buttons[1].OnClicked = delegate(GUIButton <p0>, object <p1>)
			{
				foreach (char illegalChar in Path.GetInvalidFileNameCharsCrossPlatform())
				{
					if (nameInput.Text.Contains(illegalChar))
					{
						GUI.AddMessage(TextManager.GetWithVariable("SubNameIllegalCharsWarning", "[illegalchar]", illegalChar.ToString(), FormatCapitals.No), GUIStyle.Red, null, true, null);
						return false;
					}
				}
				msgBox.Close();
				EventEditorScreen.projectName = nameInput.Text;
				XElement save = EventEditorScreen.SaveEvent(EventEditorScreen.projectName);
				string filePath = Path.Combine(directory, EventEditorScreen.projectName + ".sevproj");
				File.WriteAllText(Path.Combine(new string[]
				{
					directory,
					EventEditorScreen.projectName + ".sevproj"
				}), save.ToString(), null, true);
				GUI.AddMessage("Project saved to " + filePath, GUIStyle.Green, null, true, null);
				EventEditorScreen.AskForConfirmation(TextManager.Get("EventEditor.TestPromptHeader"), TextManager.Get("EventEditor.TestPromptBody"), new Func<bool>(this.CreateTestSetupMenu), null);
				return true;
			};
			return true;
		}

		// Token: 0x06002517 RID: 9495 RVA: 0x00176F24 File Offset: 0x00175124
		[NullableContext(2)]
		private bool Load(EventPrefab prefab)
		{
			if (prefab == null)
			{
				return false;
			}
			EventEditorScreen.AskForConfirmation(TextManager.Get("EventEditor.NewProject"), TextManager.Get("EventEditor.NewProjectPrompt"), delegate
			{
				EventEditorScreen.nodeList.Clear();
				this.selectedNodes.Clear();
				this.markedNodes.Clear();
				if (this.isTraitorEventBox != null)
				{
					this.isTraitorEventBox.Selected = (prefab is TraitorEventPrefab);
				}
				bool hadNodes = true;
				this.CreateNodes(prefab.ConfigElement, ref hadNodes, null, 0);
				if (!hadNodes)
				{
					GUI.NotifyPrompt(TextManager.Get("EventEditor.RandomGenerationHeader"), TextManager.Get("EventEditor.RandomGenerationBody"));
				}
				EventEditorScreen.UpdateHiddenNodesInConversationMode();
				return true;
			}, null);
			return true;
		}

		// Token: 0x06002518 RID: 9496 RVA: 0x00176F80 File Offset: 0x00175180
		[NullableContext(2)]
		private bool AddAction(Type type)
		{
			if (type == null)
			{
				return false;
			}
			Vector2 spawnPos = this.Cam.WorldViewCenter;
			spawnPos.Y = -spawnPos.Y;
			EditorNode newNode;
			if (EditorNode.IsInstanceOf(type, typeof(ConversationAction)))
			{
				newNode = new EventConversationNode(type, type.Name)
				{
					ID = EventEditorScreen.CreateID()
				};
			}
			else if (EditorNode.IsInstanceOf(type, typeof(EventLogAction)))
			{
				newNode = new EventLogNode(type, type.Name)
				{
					ID = EventEditorScreen.CreateID()
				};
			}
			else
			{
				newNode = new EventNode(type, type.Name)
				{
					ID = EventEditorScreen.CreateID()
				};
			}
			newNode.Position = spawnPos - newNode.Size / 2f;
			EventEditorScreen.nodeList.Add(newNode);
			return true;
		}

		// Token: 0x06002519 RID: 9497 RVA: 0x0017704C File Offset: 0x0017524C
		[NullableContext(2)]
		private bool AddValue(Type type)
		{
			if (type == null)
			{
				return false;
			}
			Vector2 spawnPos = this.Cam.WorldViewCenter;
			spawnPos.Y = -spawnPos.Y;
			ValueNode newValue = new ValueNode(type, type.Name)
			{
				ID = EventEditorScreen.CreateID()
			};
			newValue.Position = spawnPos - newValue.Size / 2f;
			EventEditorScreen.nodeList.Add(newValue);
			return true;
		}

		// Token: 0x0600251A RID: 9498 RVA: 0x001770C0 File Offset: 0x001752C0
		[NullableContext(2)]
		private bool AddSpecial(Type type)
		{
			if (type == null)
			{
				return false;
			}
			Vector2 spawnPos = this.Cam.WorldViewCenter;
			spawnPos.Y = -spawnPos.Y;
			ConstructorInfo constructor = type.GetConstructor(Array.Empty<Type>());
			SpecialNode newNode = null;
			if (constructor != null)
			{
				newNode = (constructor.Invoke(Array.Empty<object>()) as SpecialNode);
			}
			if (newNode != null)
			{
				newNode.ID = EventEditorScreen.CreateID();
				newNode.Position = spawnPos - newNode.Size / 2f;
				EventEditorScreen.nodeList.Add(newNode);
				return true;
			}
			return false;
		}

		// Token: 0x0600251B RID: 9499 RVA: 0x00177154 File Offset: 0x00175354
		private void CreateNodes(ContentXElement element, ref bool hadNodes, [Nullable(2)] EditorNode parent = null, int ident = 0)
		{
			EditorNode lastNode = null;
			foreach (ContentXElement subElement in element.Elements())
			{
				bool skip = true;
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (a == "failure" || a == "success" || a == "option")
				{
					this.CreateNodes(subElement, ref hadNodes, parent, ident);
				}
				else
				{
					skip = false;
				}
				if (!skip)
				{
					EventEditorScreen.<>c__DisplayClass46_0 CS$<>8__locals1 = new EventEditorScreen.<>c__DisplayClass46_0();
					Vector2 defaultNodePos = new Vector2(-16000f, -16000f);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(11, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Barotrauma.");
					defaultInterpolatedStringHandler.AppendFormatted<XName>(subElement.Name);
					Type t = Type.GetType(defaultInterpolatedStringHandler.ToStringAndClear());
					if (t != null && EditorNode.IsInstanceOf(t, typeof(EventAction)))
					{
						if (EditorNode.IsInstanceOf(t, typeof(ConversationAction)))
						{
							CS$<>8__locals1.newNode = new EventConversationNode(t, subElement.Name.ToString())
							{
								Position = new Vector2((float)ident, 0f),
								ID = EventEditorScreen.CreateID()
							};
						}
						else if (EditorNode.IsInstanceOf(t, typeof(EventLogAction)))
						{
							CS$<>8__locals1.newNode = new EventLogNode(t, subElement.Name.ToString())
							{
								Position = new Vector2((float)ident, 0f),
								ID = EventEditorScreen.CreateID()
							};
						}
						else
						{
							CS$<>8__locals1.newNode = new EventNode(t, subElement.Name.ToString())
							{
								Position = new Vector2((float)ident, 0f),
								ID = EventEditorScreen.CreateID()
							};
						}
					}
					else
					{
						CS$<>8__locals1.newNode = new CustomNode(subElement.Name.ToString())
						{
							Position = new Vector2((float)ident, 0f),
							ID = EventEditorScreen.CreateID()
						};
						foreach (XAttribute attribute3 in from attribute in subElement.Attributes()
						where !attribute.ToString().StartsWith("_")
						select attribute)
						{
							CS$<>8__locals1.newNode.Connections.Add(new EventEditorNodeConnection(CS$<>8__locals1.newNode, NodeConnectionType.Value, attribute3.Name.ToString(), typeof(string), null));
						}
					}
					Vector2 npos = subElement.GetAttributeVector2("_npos", defaultNodePos);
					if (npos != defaultNodePos)
					{
						CS$<>8__locals1.newNode.Position = npos;
					}
					else
					{
						hadNodes = false;
					}
					CS$<>8__locals1.parentElement = subElement.Parent;
					foreach (ContentXElement xElement in subElement.Elements())
					{
						string a2 = xElement.Name.ToString().ToLowerInvariant();
						if (a2 == "option")
						{
							EventEditorNodeConnection optionConnection = new EventEditorNodeConnection(CS$<>8__locals1.newNode, NodeConnectionType.Option, "", null, null)
							{
								OptionText = xElement.GetAttributeString("text", string.Empty),
								EndConversation = xElement.GetAttributeBool("endconversation", false)
							};
							CS$<>8__locals1.newNode.Connections.Add(optionConnection);
						}
					}
					foreach (EventEditorNodeConnection connection2 in CS$<>8__locals1.newNode.Connections)
					{
						if (connection2.Type == NodeConnectionType.Value)
						{
							foreach (XAttribute attribute2 in subElement.Attributes())
							{
								if (string.Equals(connection2.Attribute, attribute2.Name.ToString(), StringComparison.InvariantCultureIgnoreCase) && connection2.ValueType != null)
								{
									if (connection2.ValueType.IsEnum)
									{
										Array values = Enum.GetValues(connection2.ValueType);
										using (IEnumerator enumerator6 = values.GetEnumerator())
										{
											while (enumerator6.MoveNext())
											{
												object @enum = enumerator6.Current;
												if (string.Equals((@enum != null) ? @enum.ToString() : null, attribute2.Value, StringComparison.InvariantCultureIgnoreCase))
												{
													connection2.OverrideValue = @enum;
												}
											}
											continue;
										}
									}
									try
									{
										connection2.OverrideValue = EventEditorScreen.ChangeType(attribute2.Value, connection2.ValueType);
									}
									catch
									{
										DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(51, 3);
										defaultInterpolatedStringHandler2.AppendLiteral("Failed to convert the value ");
										defaultInterpolatedStringHandler2.AppendFormatted(attribute2.Value);
										defaultInterpolatedStringHandler2.AppendLiteral(" of the attribute ");
										defaultInterpolatedStringHandler2.AppendFormatted<XName>(attribute2.Name);
										defaultInterpolatedStringHandler2.AppendLiteral(" to ");
										defaultInterpolatedStringHandler2.AppendFormatted<Type>(connection2.ValueType);
										defaultInterpolatedStringHandler2.AppendLiteral(".");
										DebugConsole.ThrowError(defaultInterpolatedStringHandler2.ToStringAndClear(), null, null, false, false);
									}
								}
							}
						}
					}
					if (npos == defaultNodePos)
					{
						hadNodes = false;
						while (EventEditorScreen.nodeList.Any(new Func<EditorNode, bool>(CS$<>8__locals1.<CreateNodes>g__Predicate|0)))
						{
							EditorNode otherNode = EventEditorScreen.nodeList.Find(new Predicate<EditorNode>(CS$<>8__locals1.<CreateNodes>g__Predicate|0));
							if (otherNode != null)
							{
								CS$<>8__locals1.newNode.Position += new Vector2(128f, (float)(otherNode.GetDrawRectangle().Height + otherNode.HeaderRectangle.Height + new Random().Next(128, 256)));
							}
						}
					}
					string a3 = subElement.Name.ToString().ToLowerInvariant();
					bool flag = a3 == "text" || a3 == "conditional";
					if (flag)
					{
						if (parent != null)
						{
							parent.AddConnection(NodeConnectionType.Add);
						}
						if (parent != null)
						{
							parent.Connect(CS$<>8__locals1.newNode, NodeConnectionType.Add);
						}
					}
					else
					{
						ContentXElement parentElement = CS$<>8__locals1.parentElement;
						ContentXElement contentXElement = (parentElement != null) ? parentElement.FirstElement() : null;
						if (contentXElement == subElement)
						{
							ContentXElement parentElement2 = CS$<>8__locals1.parentElement;
							string a4 = (parentElement2 != null) ? parentElement2.Name.ToString().ToLowerInvariant() : null;
							if (!(a4 == "failure"))
							{
								if (!(a4 == "success"))
								{
									if (!(a4 == "onroundendaction"))
									{
										if (!(a4 == "option"))
										{
											if (parent != null)
											{
												parent.Connect(CS$<>8__locals1.newNode, NodeConnectionType.Add);
											}
										}
										else if (parent != null)
										{
											EventEditorNodeConnection activateConnection = CS$<>8__locals1.newNode.Connections.Find((EventEditorNodeConnection connection) => connection.Type == NodeConnectionType.Activate);
											EventEditorNodeConnection optionConnection2 = parent.Connections.FirstOrDefault((EventEditorNodeConnection connection) => connection.Type == NodeConnectionType.Option && string.Equals(connection.OptionText, CS$<>8__locals1.parentElement.GetAttributeString("text", string.Empty), StringComparison.Ordinal));
											if (activateConnection != null && optionConnection2 != null)
											{
												optionConnection2.ConnectedTo.Add(activateConnection);
											}
										}
									}
									else if (parent != null)
									{
										parent.Connect(CS$<>8__locals1.newNode, NodeConnectionType.Next);
									}
								}
								else if (parent != null)
								{
									parent.Connect(CS$<>8__locals1.newNode, NodeConnectionType.Success);
								}
							}
							else if (parent != null)
							{
								parent.Connect(CS$<>8__locals1.newNode, NodeConnectionType.Failure);
							}
						}
						else if (lastNode != null)
						{
							lastNode.Connect(CS$<>8__locals1.newNode, NodeConnectionType.Next);
						}
					}
					lastNode = CS$<>8__locals1.newNode;
					EventEditorScreen.nodeList.Add(CS$<>8__locals1.newNode);
					ident += 600;
					this.CreateNodes(subElement, ref hadNodes, CS$<>8__locals1.newNode, ident);
				}
			}
		}

		// Token: 0x0600251C RID: 9500 RVA: 0x001779E0 File Offset: 0x00175BE0
		private static RectTransform RectTransform(float x, float y, GUIComponent parent, Anchor anchor = Anchor.TopRight)
		{
			return new RectTransform(new Vector2(x, y), parent.RectTransform, anchor, null, null, null, ScaleBasis.Normal);
		}

		// Token: 0x0600251D RID: 9501 RVA: 0x00177A1C File Offset: 0x00175C1C
		public override void AddToGUIUpdateList()
		{
			this.GuiFrame.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x0600251E RID: 9502 RVA: 0x00177A2B File Offset: 0x00175C2B
		[return: Nullable(2)]
		public static object ChangeType(string value, Type type)
		{
			if (type == typeof(Identifier))
			{
				return value.ToIdentifier();
			}
			return Convert.ChangeType(value, type);
		}

		// Token: 0x0600251F RID: 9503 RVA: 0x00177A54 File Offset: 0x00175C54
		[NullableContext(2)]
		private XElement ExportXML()
		{
			GUITickBox guitickBox = this.isTraitorEventBox;
			XElement mainElement = new XElement((guitickBox != null && guitickBox.Selected) ? "TraitorEvent" : "ScriptedEvent", new XAttribute("identifier", EventEditorScreen.projectName.RemoveWhitespace().ToLowerInvariant()));
			EditorNode startNode = null;
			foreach (EditorNode eventNode in from node in EventEditorScreen.nodeList
			where node is EventNode || node is SpecialNode
			select node)
			{
				if (eventNode.GetParent() == null)
				{
					if (startNode != null)
					{
						DebugConsole.ThrowError("You have more than one start node, only one will be picked while the others will get ignored.", null, null, false, false);
					}
					if (startNode == null)
					{
						startNode = eventNode;
					}
				}
			}
			if (startNode == null)
			{
				return null;
			}
			this.ExportChildNodes(startNode, mainElement);
			return mainElement;
		}

		// Token: 0x06002520 RID: 9504 RVA: 0x00177B38 File Offset: 0x00175D38
		private void ExportChildNodes(EditorNode startNode, XElement parent)
		{
			XElement newElement = startNode.ToXML();
			if (newElement == null)
			{
				return;
			}
			parent.Add(newElement);
			EditorNode success = startNode.GetNext(NodeConnectionType.Success);
			EditorNode failure = startNode.GetNext(NodeConnectionType.Failure);
			EditorNode add = startNode.GetNext(NodeConnectionType.Add);
			EventNode eNode = startNode as EventNode;
			Tuple<EditorNode, string, bool>[] options = (eNode != null) ? eNode.GetOptions() : new Tuple<EditorNode, string, bool>[0];
			if (success != null)
			{
				XElement successElement = new XElement("Success");
				this.ExportChildNodes(success, successElement);
				newElement.Add(successElement);
			}
			if (failure != null)
			{
				XElement failureElement = new XElement("Failure");
				this.ExportChildNodes(failure, failureElement);
				newElement.Add(failureElement);
			}
			CustomNode custom = add as CustomNode;
			if (custom != null)
			{
				this.ExportChildNodes(custom, newElement);
			}
			Tuple<EditorNode, string, bool>[] array = options;
			for (int i = 0; i < array.Length; i++)
			{
				EditorNode editorNode;
				string text2;
				bool flag;
				array[i].Deconstruct(out editorNode, out text2, out flag);
				EditorNode node = editorNode;
				string text = text2;
				bool end = flag;
				XElement optionElement = new XElement("Option");
				optionElement.Add(new XAttribute("text", text ?? ""));
				if (end)
				{
					optionElement.Add(new XAttribute("endconversation", true));
				}
				EventNode eventNode = node as EventNode;
				if (eventNode != null)
				{
					this.ExportChildNodes(eventNode, optionElement);
				}
				newElement.Add(optionElement);
			}
			EditorNode next = startNode.GetNext();
			if (next != null)
			{
				this.ExportChildNodes(next, parent);
			}
		}

		// Token: 0x06002521 RID: 9505 RVA: 0x00177CB8 File Offset: 0x00175EB8
		private static XElement SaveEvent(string name)
		{
			XElement mainElement = new XElement("SavedEvent", new XAttribute("name", name));
			XElement nodes = new XElement("Nodes");
			foreach (EditorNode editorNode in EventEditorScreen.nodeList)
			{
				nodes.Add(editorNode.Save());
			}
			mainElement.Add(nodes);
			XElement connections = new XElement("AllConnections");
			foreach (EditorNode editorNode2 in EventEditorScreen.nodeList)
			{
				connections.Add(editorNode2.SaveConnections());
			}
			mainElement.Add(connections);
			return mainElement;
		}

		// Token: 0x06002522 RID: 9506 RVA: 0x00177DAC File Offset: 0x00175FAC
		private static void Load(XElement saveElement)
		{
			EventEditorScreen.nodeList.Clear();
			EventEditorScreen.hiddenNodesInConversationMode.Clear();
			EventEditorScreen.projectName = saveElement.GetAttributeString("name", TextManager.Get("EventEditor.Unnamed").Value);
			foreach (XElement element in saveElement.Elements())
			{
				string a = element.Name.ToString().ToLowerInvariant();
				if (!(a == "nodes"))
				{
					if (!(a == "allconnections"))
					{
						continue;
					}
				}
				else
				{
					using (IEnumerator<XElement> enumerator2 = element.Elements().GetEnumerator())
					{
						while (enumerator2.MoveNext())
						{
							XElement subElement = enumerator2.Current;
							EditorNode node = EditorNode.Load(subElement);
							if (node != null)
							{
								EventEditorScreen.nodeList.Add(node);
							}
						}
						continue;
					}
				}
				foreach (XElement subElement2 in element.Elements())
				{
					int id = subElement2.GetAttributeInt("i", -1);
					EditorNode node2 = EventEditorScreen.nodeList.Find((EditorNode editorNode) => editorNode.ID == id);
					if (node2 != null)
					{
						node2.LoadConnections(subElement2);
					}
				}
			}
			EventEditorScreen.UpdateHiddenNodesInConversationMode();
		}

		// Token: 0x06002523 RID: 9507 RVA: 0x00177F2C File Offset: 0x0017612C
		private static void CreateContextMenu(EditorNode node, [Nullable(2)] EventEditorNodeConnection connection = null)
		{
			if (GUIContextMenu.CurrentContextMenu != null)
			{
				return;
			}
			ContextMenuOption[] array = new ContextMenuOption[6];
			int num = 0;
			string label = "EventEditor.Edit";
			bool isEnabled;
			if (!(node is ValueNode))
			{
				EventEditorNodeConnection connection2 = connection;
				if (((connection2 != null) ? connection2.Type : null) != NodeConnectionType.Value)
				{
					EventEditorNodeConnection connection3 = connection;
					isEnabled = (((connection3 != null) ? connection3.Type : null) == NodeConnectionType.Option);
					goto IL_6B;
				}
			}
			isEnabled = true;
			IL_6B:
			array[num] = new ContextMenuOption(label, isEnabled, delegate()
			{
				EventEditorScreen.CreateEditMenu(node as ValueNode, connection);
			});
			array[1] = new ContextMenuOption("EventEditor.MarkEnding", connection != null && connection.Type == NodeConnectionType.Option, delegate()
			{
				if (connection == null)
				{
					return;
				}
				connection.EndConversation = !connection.EndConversation;
			});
			array[2] = new ContextMenuOption("EventEditor.RemoveConnection", connection != null, delegate()
			{
				if (connection == null)
				{
					return;
				}
				connection.ClearConnections();
				connection.OverrideValue = null;
				connection.OptionText = connection.OptionText;
			});
			array[3] = new ContextMenuOption("EventEditor.AddOption", node.CanAddConnections, new Action(node.AddOption));
			array[4] = new ContextMenuOption("EventEditor.RemoveOption", connection != null && node.RemovableTypes.Contains(connection.Type), delegate()
			{
				EventEditorNodeConnection connection4 = connection;
				if (connection4 == null)
				{
					return;
				}
				connection4.Parent.RemoveOption(connection);
			});
			array[5] = new ContextMenuOption("EventEditor.Delete", true, delegate()
			{
				EventEditorScreen.nodeList.Remove(node);
				node.ClearConnections();
			});
			GUIContextMenu.CreateContextMenu(array);
		}

		// Token: 0x06002524 RID: 9508 RVA: 0x001780B0 File Offset: 0x001762B0
		private bool CreateTestSetupMenu()
		{
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("EventEditor.TestPromptHeader"), "", new LocalizedString[]
			{
				TextManager.Get("Cancel"),
				TextManager.Get("OK")
			}, new Vector2?(new Vector2(0.2f, 0.3f)), new Point?(new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.5f), msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new Vector2(1f, 0.25f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = TextManager.Get("EventEditor.OutpostGenParams");
			GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT, text, null, subHeadingFont, Alignment.Left, false, "", null);
			GUIDropDown paramInput = new GUIDropDown(new RectTransform(new Vector2(1f, 0.25f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, OutpostGenerationParams.OutpostParams.Count<OutpostGenerationParams>(), "", false, false, Alignment.CenterLeft, 1f);
			foreach (OutpostGenerationParams param2 in OutpostGenerationParams.OutpostParams)
			{
				paramInput.AddItem(param2.Identifier.Value, param2, null, null, null);
			}
			paramInput.OnSelected = delegate(GUIComponent _, object param)
			{
				this.lastTestParam = (param as OutpostGenerationParams);
				return true;
			};
			paramInput.SelectItem(this.lastTestParam ?? OutpostGenerationParams.OutpostParams.FirstOrDefault<OutpostGenerationParams>());
			RectTransform rectT2 = new RectTransform(new Vector2(1f, 0.25f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text2 = TextManager.Get("EventEditor.LocationType");
			subHeadingFont = GUIStyle.SubHeadingFont;
			new GUITextBlock(rectT2, text2, null, subHeadingFont, Alignment.Left, false, "", null);
			GUIDropDown typeInput = new GUIDropDown(new RectTransform(new Vector2(1f, 0.25f), layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), string.Empty, LocationType.Prefabs.Count<LocationType>(), "", false, false, Alignment.CenterLeft, 1f);
			foreach (LocationType type2 in LocationType.Prefabs)
			{
				typeInput.AddItem(type2.Identifier.Value, type2, null, null, null);
			}
			typeInput.OnSelected = delegate(GUIComponent _, object type)
			{
				this.lastTestType = (type as LocationType);
				return true;
			};
			typeInput.SelectItem(this.lastTestType ?? LocationType.Prefabs.FirstOrDefault<LocationType>());
			msgBox.Buttons[0].OnClicked = delegate(GUIButton button, object o)
			{
				msgBox.Close();
				return true;
			};
			msgBox.Buttons[1].OnClicked = delegate(GUIButton button, object o)
			{
				this.TestEvent(this.lastTestParam, this.lastTestType);
				msgBox.Close();
				return true;
			};
			return true;
		}

		// Token: 0x06002525 RID: 9509 RVA: 0x001784B0 File Offset: 0x001766B0
		[NullableContext(2)]
		private static void CreateEditMenu(ValueNode node, EventEditorNodeConnection connection = null)
		{
			object newValue;
			Type type;
			if (node != null)
			{
				newValue = node.Value;
				type = node.Type;
			}
			else
			{
				if (connection == null)
				{
					return;
				}
				newValue = connection.OverrideValue;
				type = connection.ValueType;
			}
			EventEditorNodeConnection connection2 = connection;
			if (((connection2 != null) ? connection2.Type : null) == NodeConnectionType.Option)
			{
				newValue = connection.OptionText;
				type = typeof(string);
			}
			if (type == null)
			{
				return;
			}
			Vector2 size = (type == typeof(string)) ? new Vector2(0.2f, 0.3f) : new Vector2(0.2f, 0.175f);
			GUIMessageBox msgBox = new GUIMessageBox(TextManager.Get("EventEditor.Edit"), "", new LocalizedString[]
			{
				TextManager.Get("Cancel"),
				TextManager.Get("OK")
			}, new Vector2?(size), new Point?(new Point(300, 175)), Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			Vector2 layoutSize = (type == typeof(string)) ? new Vector2(1f, 0.5f) : new Vector2(1f, 0.25f);
			GUILayoutGroup layout = new GUILayoutGroup(new RectTransform(layoutSize, msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			if (type.IsEnum)
			{
				Array enums = Enum.GetValues(type);
				RectTransform rectT = new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				object newValue5 = newValue;
				GUIDropDown valueInput7 = new GUIDropDown(rectT, ((newValue5 != null) ? newValue5.ToString() : null) ?? "", enums.Length, "", false, false, Alignment.CenterLeft, 1f);
				foreach (object @enum in enums)
				{
					valueInput7.AddItem(((@enum != null) ? @enum.ToString() : null) ?? "", @enum, null, null, null);
				}
				GUIDropDown guidropDown = valueInput7;
				guidropDown.OnSelected = (GUIDropDown.OnSelectedHandler)Delegate.Combine(guidropDown.OnSelected, new GUIDropDown.OnSelectedHandler(delegate(GUIComponent component, object o)
				{
					newValue = o;
					return true;
				}));
			}
			else if (type == typeof(string))
			{
				GUIListBox listBox = new GUIListBox(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					CanBeFocused = false
				};
				GUITextBox valueInput = new GUITextBox(new RectTransform(Vector2.One, listBox.Content.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, true, "GUITextBoxNoBorder", null, false, true);
				valueInput.OnTextChanged += delegate(GUITextBox component, string o)
				{
					Vector2 textSize = valueInput.Font.MeasureString(valueInput.WrappedText, false);
					valueInput.RectTransform.NonScaledSize = new Point(valueInput.RectTransform.NonScaledSize.X, (int)textSize.Y + 10);
					listBox.UpdateScrollBarSize();
					listBox.BarScroll = 1f;
					newValue = o;
					return true;
				};
				GUITextBox valueInput6 = valueInput;
				object newValue2 = newValue;
				valueInput6.Text = (((newValue2 != null) ? newValue2.ToString() : null) ?? "<type here>");
			}
			else if (type == typeof(Identifier))
			{
				RectTransform rectT2 = new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				object newValue3 = newValue;
				GUITextBox valueInput2 = new GUITextBox(rectT2, ((newValue3 != null) ? newValue3.ToString() : null) ?? string.Empty, null, null, Alignment.Left, false, "", null, false, true);
				valueInput2.OnTextChanged += delegate(GUITextBox component, string o)
				{
					newValue = new Identifier(o);
					return true;
				};
			}
			else if (type == typeof(float))
			{
				GUINumberInput valueInput3 = new GUINumberInput(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Float, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
				if (newValue is float)
				{
					float floatVal = (float)newValue;
					valueInput3.FloatValue = floatVal;
				}
				GUINumberInput guinumberInput = valueInput3;
				guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput component)
				{
					newValue = component.FloatValue;
				}));
			}
			else if (type == typeof(int))
			{
				GUINumberInput valueInput4 = new GUINumberInput(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
				if (newValue is int)
				{
					int intVal = (int)newValue;
					valueInput4.IntValue = intVal;
				}
				GUINumberInput guinumberInput2 = valueInput4;
				guinumberInput2.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput2.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput component)
				{
					newValue = component.IntValue;
				}));
			}
			else if (type == typeof(bool))
			{
				GUITickBox valueInput5 = new GUITickBox(new RectTransform(Vector2.One, layout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "Value", null, "");
				if (newValue is bool)
				{
					bool val = (bool)newValue;
					valueInput5.Selected = val;
				}
				GUITickBox guitickBox = valueInput5;
				guitickBox.OnSelected = (GUITickBox.OnSelectedHandler)Delegate.Combine(guitickBox.OnSelected, new GUITickBox.OnSelectedHandler(delegate(GUITickBox component)
				{
					newValue = component.Selected;
					return true;
				}));
			}
			msgBox.Buttons[0].OnClicked = delegate(GUIButton button, object o)
			{
				msgBox.Close();
				return true;
			};
			msgBox.Buttons[1].OnClicked = delegate(GUIButton button, object o)
			{
				if (node != null)
				{
					node.Value = newValue;
				}
				else if (connection != null)
				{
					if (connection.Type == NodeConnectionType.Option)
					{
						EventEditorNodeConnection connection3 = connection;
						object newValue4 = newValue;
						connection3.OptionText = ((newValue4 != null) ? newValue4.ToString() : null);
					}
					else
					{
						connection.ClearConnections();
						connection.OverrideValue = newValue;
					}
				}
				msgBox.Close();
				return true;
			};
		}

		// Token: 0x06002526 RID: 9510 RVA: 0x00178BB8 File Offset: 0x00176DB8
		[NullableContext(2)]
		private bool TestEvent(OutpostGenerationParams param, LocationType type)
		{
			SubmarineInfo submarineInfo = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo info) => info.HasTag(SubmarineTag.Shuttle));
			if (submarineInfo == null)
			{
				throw new NullReferenceException("Could not test event: There are no shuttles available.");
			}
			SubmarineInfo subInfo = submarineInfo;
			XElement eventXml = this.ExportXML();
			if (eventXml == null)
			{
				GUI.AddMessage("Unable to open test enviroment because the event contains errors.", GUIStyle.Red, null, true, null);
				return false;
			}
			EventPrefab prefab = EventPrefab.Create(eventXml.FromPackage(null), null, default(Identifier));
			SubmarineInfo submarineInfo2 = subInfo;
			Option.UnspecifiedNone none = Option.None;
			GameSession gameSession = new GameSession(submarineInfo2, none, CampaignDataPath.Empty, GameModePreset.TestMode, CampaignSettings.Empty, null, null);
			TestGameMode testGameMode = (TestGameMode)gameSession.GameMode;
			if (testGameMode == null)
			{
				throw new InvalidCastException();
			}
			TestGameMode gameMode = testGameMode;
			gameMode.SpawnOutpost = true;
			gameMode.OutpostParams = param;
			gameMode.OutpostType = type;
			gameMode.TriggeredEvent = prefab;
			gameMode.OnRoundEnd = delegate()
			{
				Submarine.Unload();
				GameMain.EventEditorScreen.Select();
			};
			GameMain.GameScreen.Select();
			gameSession.StartRound(null, false, null, null);
			return true;
		}

		// Token: 0x06002527 RID: 9511 RVA: 0x00178CDC File Offset: 0x00176EDC
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			EventEditorScreen.DrawnTooltip = string.Empty;
			this.Cam.UpdateTransform(true, true);
			spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, null, null, null, null, new Matrix?(this.Cam.Transform));
			graphics.Clear(new Color(0.2f, 0.2f, 0.2f, 1f));
			foreach (EditorNode node5 in from node in EventEditorScreen.nodeList
			where node is SpecialNode
			select node)
			{
				if (!EventEditorScreen.ConversationMode || !EventEditorScreen.ShouldHideNodeInConversationMode(node5))
				{
					node5.Draw(spriteBatch);
				}
			}
			foreach (EditorNode node2 in from node in EventEditorScreen.nodeList
			where node is ValueNode
			select node)
			{
				if (!EventEditorScreen.ConversationMode || !EventEditorScreen.ShouldHideNodeInConversationMode(node2))
				{
					node2.Draw(spriteBatch);
				}
			}
			foreach (EditorNode node3 in from node in EventEditorScreen.nodeList
			where node is EventNode
			select node)
			{
				if (!EventEditorScreen.ConversationMode || !EventEditorScreen.ShouldHideNodeInConversationMode(node3))
				{
					node3.Draw(spriteBatch);
				}
			}
			EditorNode editorNode = this.draggedNode;
			if (editorNode != null)
			{
				editorNode.Draw(spriteBatch);
			}
			foreach (KeyValuePair<EditorNode, Vector2> keyValuePair in this.markedNodes)
			{
				EditorNode editorNode2;
				Vector2 vector;
				keyValuePair.Deconstruct(out editorNode2, out vector);
				EditorNode node4 = editorNode2;
				node4.Draw(spriteBatch);
			}
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, null, null, null);
			GUI.Draw(this.Cam, spriteBatch);
			if (!string.IsNullOrWhiteSpace(EventEditorScreen.DrawnTooltip) && GUIStyle.SmallFont.Value != null)
			{
				string tooltip = ToolBox.WrapText(EventEditorScreen.DrawnTooltip, 256f, GUIStyle.SmallFont.Value, 1f);
				GUI.DrawString(spriteBatch, PlayerInput.MousePosition + new Vector2(32f, 32f), tooltip, Color.White, new Color?(Color.Black * 0.8f), 4, GUIStyle.SmallFont, ForceUpperCase.Inherit);
			}
			spriteBatch.End();
		}

		// Token: 0x06002528 RID: 9512 RVA: 0x00178FA8 File Offset: 0x001771A8
		public override void Update(double deltaTime)
		{
			if (GameMain.GraphicsWidth != this.screenResolution.X || GameMain.GraphicsHeight != this.screenResolution.Y)
			{
				this.CreateGUI();
			}
			if (PlayerInput.KeyHit(Keys.R) && PlayerInput.KeyDown(Keys.LeftShift))
			{
				this.CreateGUI();
			}
			this.Cam.MoveCamera((float)deltaTime, true, GUI.MouseOn == null, true, null);
			Vector2 mousePos = this.Cam.ScreenToWorld(PlayerInput.MousePosition);
			mousePos.Y = -mousePos.Y;
			foreach (EditorNode node in EventEditorScreen.nodeList)
			{
				if (PlayerInput.PrimaryMouseButtonDown())
				{
					EventEditorNodeConnection connection = node.GetConnectionOnMouse(mousePos);
					if (connection != null && connection.Type.NodeSide == NodeConnectionType.Side.Right)
					{
						if (connection.Type != NodeConnectionType.Out && connection.ConnectedTo.Any<EventEditorNodeConnection>())
						{
							return;
						}
						EventEditorScreen.DraggedConnection = connection;
					}
				}
				if ((node.IsHighlighted = node.HeaderRectangle.Contains(mousePos)) && PlayerInput.PrimaryMouseButtonDown())
				{
					if (PlayerInput.IsCtrlDown())
					{
						if (this.selectedNodes.Contains(node))
						{
							this.selectedNodes.Remove(node);
						}
						else
						{
							this.selectedNodes.Add(node);
						}
						node.IsSelected = this.selectedNodes.Contains(node);
						break;
					}
					this.draggedNode = node;
					this.dragOffset = this.draggedNode.Position - mousePos;
					foreach (EditorNode selectedNode in this.selectedNodes)
					{
						if (!this.markedNodes.ContainsKey(selectedNode))
						{
							this.markedNodes.Add(selectedNode, selectedNode.Position - mousePos);
						}
					}
				}
				if (PlayerInput.SecondaryMouseButtonClicked())
				{
					EventEditorNodeConnection connection2 = node.GetConnectionOnMouse(mousePos);
					if (node.GetDrawRectangle().Contains(mousePos) || connection2 != null)
					{
						EventEditorScreen.CreateContextMenu(node, node.GetConnectionOnMouse(mousePos));
						break;
					}
				}
			}
			if (PlayerInput.SecondaryMouseButtonClicked())
			{
				foreach (EditorNode selectedNode2 in this.selectedNodes)
				{
					selectedNode2.IsSelected = false;
				}
				this.selectedNodes.Clear();
			}
			if (this.draggedNode != null)
			{
				if (!PlayerInput.PrimaryMouseButtonHeld())
				{
					this.draggedNode = null;
					this.markedNodes.Clear();
				}
				else
				{
					Vector2 offsetChange = Vector2.Zero;
					this.draggedNode.IsHighlighted = true;
					this.draggedNode.Position = mousePos + this.dragOffset;
					if (PlayerInput.KeyHit(Keys.Up))
					{
						offsetChange.Y -= 1f;
					}
					if (PlayerInput.KeyHit(Keys.Down))
					{
						offsetChange.Y += 1f;
					}
					if (PlayerInput.KeyHit(Keys.Left))
					{
						offsetChange.X -= 1f;
					}
					if (PlayerInput.KeyHit(Keys.Right))
					{
						offsetChange.X += 1f;
					}
					this.dragOffset += offsetChange;
					foreach (KeyValuePair<EditorNode, Vector2> keyValuePair in from pair in this.markedNodes
					where pair.Key != this.draggedNode
					select pair)
					{
						EditorNode editorNode2;
						Vector2 vector;
						keyValuePair.Deconstruct(out editorNode2, out vector);
						EditorNode editorNode = editorNode2;
						Vector2 offset = vector;
						editorNode.Position = mousePos + offset;
					}
					if (offsetChange != Vector2.Zero)
					{
						foreach (KeyValuePair<EditorNode, Vector2> keyValuePair in this.markedNodes.ToList<KeyValuePair<EditorNode, Vector2>>())
						{
							EditorNode editorNode2;
							Vector2 vector;
							keyValuePair.Deconstruct(out editorNode2, out vector);
							EditorNode key = editorNode2;
							Vector2 value = vector;
							this.markedNodes[key] = value + offsetChange;
						}
					}
				}
			}
			if (EventEditorScreen.DraggedConnection != null)
			{
				if (!PlayerInput.PrimaryMouseButtonHeld())
				{
					foreach (EditorNode node2 in EventEditorScreen.nodeList)
					{
						EventEditorNodeConnection nodeOnMouse = node2.GetConnectionOnMouse(mousePos);
						if (nodeOnMouse != null && nodeOnMouse != EventEditorScreen.DraggedConnection && nodeOnMouse.Type.NodeSide == NodeConnectionType.Side.Left && EventEditorScreen.DraggedConnection.CanConnect(nodeOnMouse))
						{
							nodeOnMouse.ClearConnections();
							EditorNode.Connect(EventEditorScreen.DraggedConnection, nodeOnMouse);
							break;
						}
					}
					EventEditorScreen.DraggedConnection = null;
				}
				else
				{
					EventEditorScreen.DraggingPosition = mousePos;
				}
			}
			else
			{
				EventEditorScreen.DraggingPosition = Vector2.Zero;
			}
			if (PlayerInput.MidButtonHeld())
			{
				Vector2 moveSpeed = PlayerInput.MouseSpeed * (float)deltaTime * 60f / this.Cam.Zoom;
				moveSpeed.X = -moveSpeed.X;
				this.Cam.Position += moveSpeed;
			}
			base.Update(deltaTime);
		}

		// Token: 0x0400126C RID: 4716
		private GUIFrame GuiFrame;

		// Token: 0x04001270 RID: 4720
		public static readonly List<EditorNode> nodeList = new List<EditorNode>();

		// Token: 0x04001271 RID: 4721
		private readonly List<EditorNode> selectedNodes = new List<EditorNode>();

		// Token: 0x04001272 RID: 4722
		public static Vector2 DraggingPosition = Vector2.Zero;

		// Token: 0x04001273 RID: 4723
		[Nullable(2)]
		public static EventEditorNodeConnection DraggedConnection;

		// Token: 0x04001274 RID: 4724
		[Nullable(2)]
		private EditorNode draggedNode;

		// Token: 0x04001275 RID: 4725
		private Vector2 dragOffset;

		// Token: 0x04001276 RID: 4726
		private readonly Dictionary<EditorNode, Vector2> markedNodes = new Dictionary<EditorNode, Vector2>();

		// Token: 0x04001277 RID: 4727
		private static string projectName = string.Empty;

		// Token: 0x04001278 RID: 4728
		[Nullable(2)]
		private OutpostGenerationParams lastTestParam;

		// Token: 0x04001279 RID: 4729
		[Nullable(2)]
		private LocationType lastTestType;

		// Token: 0x0400127A RID: 4730
		[Nullable(2)]
		private GUITickBox isTraitorEventBox;

		// Token: 0x0400127B RID: 4731
		[Nullable(2)]
		private GUITickBox conversationModeBox;

		// Token: 0x0400127C RID: 4732
		private readonly LanguageIdentifier originalLanguage;

		// Token: 0x0400127D RID: 4733
		[Nullable(2)]
		private GUIDropDown languageDropdown;

		// Token: 0x0400127E RID: 4734
		private Point screenResolution;

		// Token: 0x0400127F RID: 4735
		private static readonly HashSet<EditorNode> hiddenNodesInConversationMode = new HashSet<EditorNode>();

		// Token: 0x02000C33 RID: 3123
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004A44 RID: 19012
			[Nullable(0)]
			public static Func<EditorNode, bool> <0>__IsEventTextDisplayNode;
		}
	}
}
