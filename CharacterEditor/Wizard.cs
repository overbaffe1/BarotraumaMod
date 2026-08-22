using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Barotrauma.CharacterEditor
{
	// Token: 0x0200044D RID: 1101
	internal class Wizard
	{
		// Token: 0x170012B0 RID: 4784
		// (get) Token: 0x060049AF RID: 18863 RVA: 0x0028C644 File Offset: 0x0028A844
		// (set) Token: 0x060049B0 RID: 18864 RVA: 0x0028C64C File Offset: 0x0028A84C
		public bool IsCopy { get; private set; }

		// Token: 0x170012B1 RID: 4785
		// (get) Token: 0x060049B1 RID: 18865 RVA: 0x0028C655 File Offset: 0x0028A855
		// (set) Token: 0x060049B2 RID: 18866 RVA: 0x0028C65D File Offset: 0x0028A85D
		public CharacterParams SourceCharacter { get; private set; }

		// Token: 0x170012B2 RID: 4786
		// (get) Token: 0x060049B3 RID: 18867 RVA: 0x0028C666 File Offset: 0x0028A866
		// (set) Token: 0x060049B4 RID: 18868 RVA: 0x0028C66E File Offset: 0x0028A86E
		public RagdollParams SourceRagdoll { get; private set; }

		// Token: 0x170012B3 RID: 4787
		// (get) Token: 0x060049B5 RID: 18869 RVA: 0x0028C677 File Offset: 0x0028A877
		// (set) Token: 0x060049B6 RID: 18870 RVA: 0x0028C67F File Offset: 0x0028A87F
		public IEnumerable<AnimationParams> SourceAnimations { get; private set; }

		// Token: 0x060049B7 RID: 18871 RVA: 0x0028C688 File Offset: 0x0028A888
		public void CopyExisting(CharacterParams character, RagdollParams ragdoll, IEnumerable<AnimationParams> animations)
		{
			this.IsCopy = true;
			this.SourceCharacter = character;
			this.SourceRagdoll = ragdoll;
			this.SourceAnimations = animations;
			this.name = character.SpeciesName;
			this.isHumanoid = character.Humanoid;
			this.canEnterSubmarine = ragdoll.CanEnterSubmarine;
			this.canWalk = ragdoll.CanWalk;
			this.texturePath = ragdoll.Texture;
			if (string.IsNullOrEmpty(this.texturePath) && this.name != CharacterPrefab.HumanSpeciesName)
			{
				RagdollParams.LimbParams limbParams = ragdoll.Limbs.FirstOrDefault<RagdollParams.LimbParams>();
				this.texturePath = ((limbParams != null) ? limbParams.GetSprite().Texture : null);
			}
		}

		// Token: 0x170012B4 RID: 4788
		// (get) Token: 0x060049B8 RID: 18872 RVA: 0x0028C72E File Offset: 0x0028A92E
		public static Wizard Instance
		{
			get
			{
				if (Wizard.instance == null)
				{
					Wizard.instance = new Wizard();
				}
				return Wizard.instance;
			}
		}

		// Token: 0x060049B9 RID: 18873 RVA: 0x0028C746 File Offset: 0x0028A946
		public static LocalizedString GetCharacterEditorTranslation(string text)
		{
			return CharacterEditorScreen.GetCharacterEditorTranslation(text);
		}

		// Token: 0x060049BA RID: 18874 RVA: 0x0028C74E File Offset: 0x0028A94E
		public void Reset()
		{
			Wizard.CharacterView.Get().Release();
			Wizard.RagdollView.Get().Release();
			Wizard.instance = null;
		}

		// Token: 0x060049BB RID: 18875 RVA: 0x0028C76C File Offset: 0x0028A96C
		public void SelectTab(Wizard.Tab tab)
		{
			this.currentTab = tab;
			Wizard.View view = this.activeView;
			if (view != null)
			{
				view.Box.Close();
			}
			switch (this.currentTab)
			{
			case Wizard.Tab.Character:
				this.activeView = Wizard.CharacterView.Get();
				return;
			case Wizard.Tab.Ragdoll:
				this.activeView = Wizard.RagdollView.Get();
				return;
			}
			this.Reset();
		}

		// Token: 0x060049BC RID: 18876 RVA: 0x0028C7CF File Offset: 0x0028A9CF
		public void AddToGUIUpdateList()
		{
			Wizard.View view = this.activeView;
			if (view == null)
			{
				return;
			}
			view.Box.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x060049BD RID: 18877 RVA: 0x0028C7E8 File Offset: 0x0028A9E8
		public void CreateCharacter(XElement ragdollElement, XElement characterElement = null, IEnumerable<AnimationParams> animations = null)
		{
			if (CharacterPrefab.Find((CharacterPrefab p) => p.Identifier == this.name) != null)
			{
				LocalizedString verificationText = this.contentPackage.GetFiles<CharacterFile>().Any((CharacterFile f) => Path.GetFileNameWithoutExtension(f.Path.Value) == this.name) ? Wizard.GetCharacterEditorTranslation("existingcharacterfoundreplaceverification") : Wizard.GetCharacterEditorTranslation("existingcharacterfoundoverrideverification");
				GUIMessageBox msgBox = new GUIMessageBox("", verificationText, new LocalizedString[]
				{
					TextManager.Get("Yes"),
					TextManager.Get("No")
				}, null, null, Alignment.TopLeft, GUIMessageBox.Type.Warning, "", null, "", null, null, false)
				{
					UserData = "verificationprompt"
				};
				msgBox.Buttons[0].OnClicked = delegate(GUIButton _, object userdata)
				{
					msgBox.Close();
					if (CharacterEditorScreen.Instance.CreateCharacter(this.name, Path.GetDirectoryName(this.xmlPath), this.isHumanoid, this.contentPackage, ragdollElement, characterElement, animations))
					{
						LocalizedString message2 = Wizard.GetCharacterEditorTranslation("CharacterCreated").Replace("[name]", this.name.Value, StringComparison.Ordinal);
						Color color2 = GUIStyle.Green;
						GUIFont font2 = GUIStyle.Font;
						GUI.AddMessage(message2, color2, null, true, font2);
					}
					Wizard.Instance.SelectTab(Wizard.Tab.None);
					return true;
				};
				msgBox.Buttons[1].OnClicked = delegate(GUIButton _, object userdata)
				{
					msgBox.Close();
					return true;
				};
				return;
			}
			if (CharacterEditorScreen.Instance.CreateCharacter(this.name, Path.GetDirectoryName(this.xmlPath), this.isHumanoid, this.contentPackage, ragdollElement, characterElement, animations))
			{
				LocalizedString message = Wizard.GetCharacterEditorTranslation("CharacterCreated").Replace("[name]", this.name.Value, StringComparison.Ordinal);
				Color color = GUIStyle.Green;
				GUIFont font = GUIStyle.Font;
				GUI.AddMessage(message, color, null, true, font);
			}
			Wizard.Instance.SelectTab(Wizard.Tab.None);
		}

		// Token: 0x04002662 RID: 9826
		private Identifier name;

		// Token: 0x04002663 RID: 9827
		private bool isHumanoid;

		// Token: 0x04002664 RID: 9828
		private CanEnterSubmarine canEnterSubmarine = CanEnterSubmarine.True;

		// Token: 0x04002665 RID: 9829
		private bool canWalk;

		// Token: 0x04002666 RID: 9830
		private string texturePath;

		// Token: 0x04002667 RID: 9831
		private string xmlPath;

		// Token: 0x04002668 RID: 9832
		private ContentPackage contentPackage;

		// Token: 0x04002669 RID: 9833
		private Dictionary<string, XElement> limbXElements = new Dictionary<string, XElement>();

		// Token: 0x0400266A RID: 9834
		private List<GUIComponent> limbGUIElements = new List<GUIComponent>();

		// Token: 0x0400266B RID: 9835
		private List<XElement> jointXElements = new List<XElement>();

		// Token: 0x0400266C RID: 9836
		private List<GUIComponent> jointGUIElements = new List<GUIComponent>();

		// Token: 0x04002671 RID: 9841
		public static Wizard instance;

		// Token: 0x04002672 RID: 9842
		private Wizard.View activeView;

		// Token: 0x04002673 RID: 9843
		private Wizard.Tab currentTab;

		// Token: 0x020011AE RID: 4526
		public enum Tab
		{
			// Token: 0x04005CB2 RID: 23730
			None,
			// Token: 0x04005CB3 RID: 23731
			Character,
			// Token: 0x04005CB4 RID: 23732
			Ragdoll
		}

		// Token: 0x020011AF RID: 4527
		private class CharacterView : Wizard.View
		{
			// Token: 0x0600914C RID: 37196 RVA: 0x003C2CF2 File Offset: 0x003C0EF2
			public static Wizard.CharacterView Get()
			{
				return Wizard.View.Get<Wizard.CharacterView>(ref Wizard.CharacterView.instance);
			}

			// Token: 0x0600914D RID: 37197 RVA: 0x003C2CFE File Offset: 0x003C0EFE
			public override void Release()
			{
				Wizard.CharacterView.instance = null;
			}

			// Token: 0x0600914E RID: 37198 RVA: 0x003C2D08 File Offset: 0x003C0F08
			protected override GUIMessageBox Create()
			{
				Wizard.CharacterView.<>c__DisplayClass3_0 CS$<>8__locals1 = new Wizard.CharacterView.<>c__DisplayClass3_0();
				CS$<>8__locals1.<>4__this = this;
				GUIMessageBox box = new GUIMessageBox(Wizard.GetCharacterEditorTranslation("CreateNewCharacter"), string.Empty, new LocalizedString[]
				{
					TextManager.Get("Cancel"),
					base.IsCopy ? TextManager.Get("Create") : TextManager.Get("Next")
				}, new Vector2?(new Vector2(0.65f, 0.9f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				box.Header.Font = GUIStyle.LargeFont;
				box.Content.ChildAnchor = Anchor.TopCenter;
				box.Content.AbsoluteSpacing = 20;
				int elementSize = 30;
				GUIFrame frame = new GUIFrame(new RectTransform(new Point(box.Content.Rect.Width - (int)(40f * GUI.xScale), box.Content.Rect.Height - (int)(50f * GUI.yScale)), box.Content.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, new Color?(ParamsEditor.Color))
				{
					CanBeFocused = false
				};
				GUILayoutGroup topGroup = new GUILayoutGroup(new RectTransform(new Vector2(0.99f, 1f), frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					AbsoluteSpacing = 2
				};
				List<GUIComponent> fields = new List<GUIComponent>();
				CS$<>8__locals1.nameField = null;
				CS$<>8__locals1.texturePathElement = null;
				CS$<>8__locals1.xmlPathElement = null;
				CS$<>8__locals1.contentPackageDropDown = null;
				CS$<>8__locals1.updateTexturePath = !base.IsCopy;
				CS$<>8__locals1.isTextureSelected = false;
				for (int i = 0; i < 7; i++)
				{
					GUIFrame mainElement = new GUIFrame(new RectTransform(new Point(topGroup.RectTransform.Rect.Width, elementSize), topGroup.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(Color.Gray * 0.25f));
					fields.Add(mainElement);
					switch (i)
					{
					case 0:
					{
						new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), mainElement.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Name"), null, null, Alignment.Left, false, "", null);
						CS$<>8__locals1.nameField = new GUITextBox(new RectTransform(new Vector2(0.7f, 1f), mainElement.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), base.Name.Value ?? Wizard.GetCharacterEditorTranslation("DefaultName").Value, null, null, Alignment.Left, false, "", null, false, true)
						{
							CaretColor = new Color?(Color.White)
						};
						base.Name = Wizard.CharacterView.<Create>g__ProcessText|3_2(CS$<>8__locals1.nameField.Text).ToIdentifier();
						GUITextBox nameField = CS$<>8__locals1.nameField;
						GUITextBox.OnTextChangedHandler value;
						if ((value = CS$<>8__locals1.<>9__3) == null)
						{
							value = (CS$<>8__locals1.<>9__3 = delegate(GUITextBox tb, string text)
							{
								CS$<>8__locals1.<>4__this.Name = Wizard.CharacterView.<Create>g__ProcessText|3_2(text).ToIdentifier();
								base.<Create>g__UpdatePaths|0();
								return true;
							});
						}
						nameField.OnTextChanged += value;
						break;
					}
					case 1:
					{
						GUITextBlock label = new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), mainElement.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("IsHumanoid"), null, null, Alignment.Left, false, "", null);
						GUITickBox guitickBox = new GUITickBox(new RectTransform(new Vector2(0.7f, 1f), mainElement.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Empty, null, "");
						guitickBox.Selected = base.IsHumanoid;
						guitickBox.Enabled = !base.IsCopy;
						GUITickBox.OnSelectedHandler onSelected;
						if ((onSelected = CS$<>8__locals1.<>9__5) == null)
						{
							onSelected = (CS$<>8__locals1.<>9__5 = ((GUITickBox tB) => CS$<>8__locals1.<>4__this.IsHumanoid = tB.Selected));
						}
						guitickBox.OnSelected = onSelected;
						GUITickBox tickBox = guitickBox;
						if (!tickBox.Enabled)
						{
							label.TextColor *= 0.6f;
						}
						break;
					}
					case 2:
					{
						GUITextBlock j = new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), mainElement.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("CanEnterSubmarines"), null, null, Alignment.Left, false, "", null);
						GUITickBox guitickBox2 = new GUITickBox(new RectTransform(new Vector2(0.7f, 1f), mainElement.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Empty, null, "");
						guitickBox2.Selected = (base.CanEnterSubmarine == CanEnterSubmarine.True);
						guitickBox2.Enabled = !base.IsCopy;
						GUITickBox.OnSelectedHandler onSelected2;
						if ((onSelected2 = CS$<>8__locals1.<>9__6) == null)
						{
							onSelected2 = (CS$<>8__locals1.<>9__6 = delegate(GUITickBox tB)
							{
								CS$<>8__locals1.<>4__this.CanEnterSubmarine = (tB.Selected ? CanEnterSubmarine.True : CanEnterSubmarine.False);
								return true;
							});
						}
						guitickBox2.OnSelected = onSelected2;
						GUITickBox t = guitickBox2;
						if (!t.Enabled)
						{
							j.TextColor *= 0.6f;
						}
						break;
					}
					case 3:
					{
						GUITextBlock lbl = new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), mainElement.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("CanWalk"), null, null, Alignment.Left, false, "", null);
						GUITickBox guitickBox3 = new GUITickBox(new RectTransform(new Vector2(0.7f, 1f), mainElement.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Empty, null, "");
						guitickBox3.Selected = base.CanWalk;
						guitickBox3.Enabled = !base.IsCopy;
						GUITickBox.OnSelectedHandler onSelected3;
						if ((onSelected3 = CS$<>8__locals1.<>9__7) == null)
						{
							onSelected3 = (CS$<>8__locals1.<>9__7 = ((GUITickBox tB) => CS$<>8__locals1.<>4__this.CanWalk = tB.Selected));
						}
						guitickBox3.OnSelected = onSelected3;
						GUITickBox txt = guitickBox3;
						if (!txt.Enabled)
						{
							lbl.TextColor *= 0.6f;
						}
						break;
					}
					case 4:
					{
						new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), mainElement.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("ConfigFileOutput"), null, null, Alignment.Left, false, "", null);
						CS$<>8__locals1.xmlPathElement = new GUITextBox(new RectTransform(new Vector2(0.7f, 1f), mainElement.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null, false, true)
						{
							Text = base.XMLPath,
							CaretColor = new Color?(Color.White)
						};
						GUITextBox xmlPathElement = CS$<>8__locals1.xmlPathElement;
						GUITextBox.OnTextChangedHandler value2;
						if ((value2 = CS$<>8__locals1.<>9__4) == null)
						{
							value2 = (CS$<>8__locals1.<>9__4 = delegate(GUITextBox tb, string text)
							{
								CS$<>8__locals1.<>4__this.XMLPath = text;
								return true;
							});
						}
						xmlPathElement.OnTextChanged += value2;
						break;
					}
					case 5:
					{
						new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), mainElement.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("TexturePath"), null, null, Alignment.Left, false, "", null);
						GUIFrame rightContainer = new GUIFrame(new RectTransform(new Vector2(0.7f, 1f), mainElement.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, null);
						CS$<>8__locals1.texturePathElement = new GUITextBox(new RectTransform(new Vector2(0.7f, 1f), rightContainer.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), string.Empty, null, null, Alignment.Left, false, "", null, false, true)
						{
							Text = base.TexturePath,
							CaretColor = new Color?(Color.White)
						};
						GUITextBox texturePathElement = CS$<>8__locals1.texturePathElement;
						GUITextBox.OnTextChangedHandler value3;
						if ((value3 = CS$<>8__locals1.<>9__8) == null)
						{
							value3 = (CS$<>8__locals1.<>9__8 = delegate(GUITextBox tb, string text)
							{
								CS$<>8__locals1.updateTexturePath = false;
								CS$<>8__locals1.<>4__this.TexturePath = text;
								return true;
							});
						}
						texturePathElement.OnTextChanged += value3;
						LocalizedString title = Wizard.GetCharacterEditorTranslation("SelectTexture");
						GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.3f / CS$<>8__locals1.texturePathElement.RectTransform.RelativeSize.X, 1f), CS$<>8__locals1.texturePathElement.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), title, Alignment.Center, "GUIButtonSmall", null);
						GUIButton.OnClickedHandler onClicked;
						if ((onClicked = CS$<>8__locals1.<>9__9) == null)
						{
							onClicked = (CS$<>8__locals1.<>9__9 = delegate(GUIButton button, object data)
							{
								Action<string> onFileSelected;
								if ((onFileSelected = CS$<>8__locals1.<>9__10) == null)
								{
									onFileSelected = (CS$<>8__locals1.<>9__10 = delegate(string file)
									{
										string relativePath = Path.GetRelativePath(Environment.CurrentDirectory, Path.GetFullPath(file));
										if (relativePath.StartsWith("LocalMods"))
										{
											string[] pathSplit = relativePath.Split(new char[]
											{
												'/',
												'\\'
											});
											string modDirName = "LocalMods/" + pathSplit[1];
											ContentPackage contentPackage2 = CS$<>8__locals1.contentPackageDropDown.ListBox.SelectedData as ContentPackage;
											string selectedModDir = ((contentPackage2 != null) ? contentPackage2.Dir.CleanUpPathCrossPlatform(false, "") : null) ?? "";
											if (modDirName == selectedModDir)
											{
												relativePath = "%ModDir%/" + string.Join("/", RuntimeHelpers.GetSubArray<string>(pathSplit, Range.StartAt(2)));
											}
											else
											{
												relativePath = string.Format("%ModDir:{0}%", pathSplit[1]) + "/" + string.Join("/", RuntimeHelpers.GetSubArray<string>(pathSplit, Range.StartAt(2)));
											}
										}
										string destinationPath = relativePath;
										if (relativePath.StartsWith("..") || Path.GetPathRoot(Environment.CurrentDirectory) != Path.GetPathRoot(file))
										{
											destinationPath = Path.Combine(new string[]
											{
												Path.GetDirectoryName(CS$<>8__locals1.<>4__this.XMLPath),
												Path.GetFileName(file)
											});
											string destinationDir = Path.GetDirectoryName(destinationPath);
											if (!Directory.Exists(destinationDir))
											{
												Directory.CreateDirectory(destinationDir, true);
											}
											if (!File.Exists(destinationPath))
											{
												File.Copy(file, Path.GetFullPath(destinationPath), true, true);
											}
										}
										CS$<>8__locals1.isTextureSelected = true;
										CS$<>8__locals1.texturePathElement.Text = destinationPath.CleanUpPath();
									});
								}
								FileSelection.OnFileSelected = onFileSelected;
								FileSelection.ClearFileTypeFilters();
								FileSelection.AddFileTypeFilter("PNG", "*.png");
								FileSelection.AddFileTypeFilter("JPEG", "*.jpg, *.jpeg");
								FileSelection.AddFileTypeFilter("All files", "*.*");
								FileSelection.SelectFileTypeFilter("*.png");
								FileSelection.Open = true;
								return true;
							});
						}
						guibutton.OnClicked = onClicked;
						break;
					}
					case 6:
					{
						mainElement.RectTransform.NonScaledSize = new Point(mainElement.RectTransform.NonScaledSize.X, mainElement.RectTransform.NonScaledSize.Y * 2);
						new GUITextBlock(new RectTransform(new Vector2(0.3f, 1f), mainElement.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("ContentPackage"), null, null, Alignment.Left, false, "", null);
						GUIFrame rightContainer2 = new GUIFrame(new RectTransform(new Vector2(0.7f, 1f), mainElement.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), null, null);
						CS$<>8__locals1.contentPackageDropDown = new GUIDropDown(new RectTransform(new Vector2(1f, 0.5f), rightContainer2.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), null, 4, "", false, false, Alignment.CenterLeft, 1f);
						foreach (ContentPackage contentPackage in ContentPackageManager.EnabledPackages.All)
						{
							if (contentPackage != GameMain.VanillaContent)
							{
								CS$<>8__locals1.contentPackageDropDown.AddItem(contentPackage.Name, contentPackage, contentPackage.Path, null, null);
							}
						}
						GUIDropDown contentPackageDropDown = CS$<>8__locals1.contentPackageDropDown;
						GUIDropDown.OnSelectedHandler onSelected4;
						if ((onSelected4 = CS$<>8__locals1.<>9__11) == null)
						{
							onSelected4 = (CS$<>8__locals1.<>9__11 = delegate(GUIComponent obj, object userdata)
							{
								CS$<>8__locals1.<>4__this.ContentPackage = (userdata as ContentPackage);
								CS$<>8__locals1.updateTexturePath = (!CS$<>8__locals1.isTextureSelected && !CS$<>8__locals1.<>4__this.IsCopy);
								base.<Create>g__UpdatePaths|0();
								return true;
							});
						}
						contentPackageDropDown.OnSelected = onSelected4;
						CS$<>8__locals1.contentPackageDropDown.Select(0);
						GUITextBox contentPackageNameElement = new GUITextBox(new RectTransform(new Vector2(0.7f, 0.5f), rightContainer2.RectTransform, Anchor.BottomLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("NewContentPackage").Value, null, null, Alignment.Left, false, "", null, false, true)
						{
							CaretColor = new Color?(Color.White)
						};
						Func<ContentPackage, bool> <>9__15;
						GUIButton createNewPackageButton = new GUIButton(new RectTransform(new Vector2(0.3f / contentPackageNameElement.RectTransform.RelativeSize.X, 1f), contentPackageNameElement.RectTransform, Anchor.CenterRight, new Pivot?(Pivot.CenterLeft), null, null, ScaleBasis.Normal), TextManager.Get("CreateNew"), Alignment.Center, "GUIButtonSmall", null)
						{
							OnClicked = delegate(GUIButton btn, object userdata)
							{
								if (string.IsNullOrEmpty(contentPackageNameElement.Text))
								{
									contentPackageNameElement.Flash(null, 1.5f, true, false, null);
									return false;
								}
								IEnumerable<ContentPackage> allPackages = ContentPackageManager.AllPackages;
								Func<ContentPackage, bool> predicate;
								if ((predicate = <>9__15) == null)
								{
									predicate = (<>9__15 = ((ContentPackage cp) => cp.Name.ToLower() == contentPackageNameElement.Text.ToLower()));
								}
								if (allPackages.Any(predicate))
								{
									new GUIMessageBox("", TextManager.Get(new string[]
									{
										"charactereditor.contentpackagenameinuse",
										"leveleditorlevelobjnametaken"
									}), null, null, GUIMessageBox.Type.Warning);
									return false;
								}
								string modName = contentPackageNameElement.Text;
								ModProject modProject = new ModProject
								{
									Name = modName
								};
								CS$<>8__locals1.<>4__this.ContentPackage = ContentPackageManager.LocalPackages.SaveAndEnableRegularMod(modProject);
								CS$<>8__locals1.contentPackageDropDown.AddItem(CS$<>8__locals1.<>4__this.ContentPackage.Name, CS$<>8__locals1.<>4__this.ContentPackage, CS$<>8__locals1.<>4__this.ContentPackage.Path, null, null);
								CS$<>8__locals1.contentPackageDropDown.SelectItem(CS$<>8__locals1.<>4__this.ContentPackage);
								contentPackageNameElement.Text = "";
								return true;
							},
							Enabled = false
						};
						Color textColor = contentPackageNameElement.TextColor;
						contentPackageNameElement.TextColor *= 0.6f;
						contentPackageNameElement.OnSelected += delegate(GUITextBox sender, Keys key)
						{
							contentPackageNameElement.Text = "";
						};
						contentPackageNameElement.OnTextChanged += delegate(GUITextBox textBox, string text)
						{
							textBox.TextColor = textColor;
							createNewPackageButton.Enabled = !string.IsNullOrWhiteSpace(text);
							return true;
						};
						rightContainer2.RectTransform.MinSize = new Point(0, CS$<>8__locals1.contentPackageDropDown.RectTransform.MinSize.Y + Math.Max(contentPackageNameElement.RectTransform.MinSize.Y, createNewPackageButton.RectTransform.MinSize.Y));
						break;
					}
					}
					int contentSize = mainElement.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y);
					mainElement.RectTransform.Resize(new Point(mainElement.Rect.Width, Math.Max(mainElement.Rect.Height, contentSize)), true);
				}
				CS$<>8__locals1.<Create>g__UpdatePaths|0();
				box.Buttons[0].Parent.RectTransform.SetAsLastChild();
				box.Buttons[1].RectTransform.SetAsLastChild();
				GUIButton guibutton2 = box.Buttons[0];
				guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					Wizard.Instance.SelectTab(Wizard.Tab.None);
					return true;
				}));
				GUIButton guibutton3 = box.Buttons[1];
				guibutton3.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton3.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					if (CS$<>8__locals1.<>4__this.ContentPackage == null)
					{
						CS$<>8__locals1.contentPackageDropDown.Flash(null, 1.5f, true, false, null);
						return false;
					}
					Identifier name = CS$<>8__locals1.<>4__this.Name;
					if (name.Value.IsNullOrWhiteSpace())
					{
						GUITextBox nameField2 = CS$<>8__locals1.nameField;
						if (nameField2 != null)
						{
							nameField2.Flash(null, 1.5f, true, false, null);
						}
						return false;
					}
					string evaluatedTexturePath = ContentPath.FromRaw(CS$<>8__locals1.contentPackageDropDown.SelectedData as ContentPackage, CS$<>8__locals1.<>4__this.TexturePath).Value;
					CharacterParams sourceCharacter = CS$<>8__locals1.<>4__this.SourceCharacter;
					Identifier? identifier;
					Identifier? identifier2;
					if (sourceCharacter == null)
					{
						identifier = null;
						identifier2 = identifier;
					}
					else
					{
						identifier2 = new Identifier?(sourceCharacter.SpeciesName);
					}
					identifier = identifier2;
					Identifier? identifier3 = new Identifier?(CharacterPrefab.HumanSpeciesName);
					if (identifier != identifier3 && !File.Exists(evaluatedTexturePath))
					{
						GUI.AddMessage(Wizard.GetCharacterEditorTranslation("TextureDoesNotExist"), GUIStyle.Red, null, true, null);
						CS$<>8__locals1.texturePathElement.Flash(null, 1.5f, true, false, null);
						return false;
					}
					string path = Path.GetFileName(evaluatedTexturePath);
					if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
					{
						GUI.AddMessage(TextManager.Get("WrongFileType"), GUIStyle.Red, null, true, null);
						CS$<>8__locals1.texturePathElement.Flash(null, 1.5f, true, false, null);
						return false;
					}
					name = CS$<>8__locals1.<>4__this.Name;
					if (name == CharacterPrefab.HumanSpeciesName && !CS$<>8__locals1.<>4__this.IsCopy)
					{
						if (!CharacterEditorScreen.Instance.SpawnedCharacter.IsHuman)
						{
							CharacterEditorScreen.Instance.SpawnCharacter(CharacterPrefab.HumanSpeciesName, null);
						}
						CharacterEditorScreen.Instance.PrepareCharacterCopy();
					}
					if (CS$<>8__locals1.<>4__this.IsCopy)
					{
						CS$<>8__locals1.<>4__this.SourceRagdoll.Texture = evaluatedTexturePath;
						CS$<>8__locals1.<>4__this.SourceRagdoll.CanEnterSubmarine = CS$<>8__locals1.<>4__this.CanEnterSubmarine;
						CS$<>8__locals1.<>4__this.SourceRagdoll.CanWalk = CS$<>8__locals1.<>4__this.CanWalk;
						CS$<>8__locals1.<>4__this.SourceRagdoll.Serialize(null, true, true);
						Wizard.Instance.CreateCharacter(CS$<>8__locals1.<>4__this.SourceRagdoll.MainElement, CS$<>8__locals1.<>4__this.SourceCharacter.MainElement, CS$<>8__locals1.<>4__this.SourceAnimations);
					}
					else
					{
						Wizard.Instance.SelectTab(Wizard.Tab.Ragdoll);
					}
					return true;
				}));
				return box;
			}

			// Token: 0x06009150 RID: 37200 RVA: 0x003C3C70 File Offset: 0x003C1E70
			[CompilerGenerated]
			internal static string <Create>g__ProcessText|3_2(string text)
			{
				return text.RemoveWhitespace().CapitaliseFirstInvariant();
			}

			// Token: 0x04005CB5 RID: 23733
			private static Wizard.CharacterView instance;
		}

		// Token: 0x020011B0 RID: 4528
		private class RagdollView : Wizard.View
		{
			// Token: 0x06009151 RID: 37201 RVA: 0x003C3C7D File Offset: 0x003C1E7D
			public static Wizard.RagdollView Get()
			{
				return Wizard.View.Get<Wizard.RagdollView>(ref Wizard.RagdollView.instance);
			}

			// Token: 0x06009152 RID: 37202 RVA: 0x003C3C89 File Offset: 0x003C1E89
			public override void Release()
			{
				Wizard.RagdollView.instance = null;
			}

			// Token: 0x06009153 RID: 37203 RVA: 0x003C3C94 File Offset: 0x003C1E94
			protected override GUIMessageBox Create()
			{
				Wizard.RagdollView.<>c__DisplayClass3_0 CS$<>8__locals1 = new Wizard.RagdollView.<>c__DisplayClass3_0();
				CS$<>8__locals1.<>4__this = this;
				GUIMessageBox box = new GUIMessageBox(Wizard.GetCharacterEditorTranslation("DefineRagdoll"), string.Empty, new LocalizedString[]
				{
					TextManager.Get("Previous"),
					TextManager.Get("Create")
				}, new Vector2?(new Vector2(0.65f, 1f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				box.Header.Font = GUIStyle.LargeFont;
				box.Content.ChildAnchor = Anchor.TopCenter;
				box.Content.AbsoluteSpacing = (int)(20f * GUI.Scale);
				CS$<>8__locals1.elementSize = (int)(40f * GUI.Scale);
				GUIFrame frame = new GUIFrame(new RectTransform(new Point(box.Content.Rect.Width - (int)(80f * GUI.xScale), box.Content.Rect.Height - (int)(200f * GUI.yScale)), box.Content.RectTransform, Anchor.Center, null, ScaleBasis.Normal, false), null, new Color?(ParamsEditor.Color))
				{
					CanBeFocused = false
				};
				GUILayoutGroup content = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.9f), frame.RectTransform, Anchor.TopCenter, null, null, null, ScaleBasis.Normal), false, Anchor.TopCenter)
				{
					Stretch = true,
					RelativeSpacing = 0.02f
				};
				GUIFrame limbsElement = new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
				{
					CanBeFocused = false
				};
				GUILayoutGroup limbEditLayout = new GUILayoutGroup(new RectTransform(Vector2.One, limbsElement.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.02f
				};
				RectTransform rectT = new RectTransform(new Vector2(0.2f, 1f), limbEditLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text = Wizard.GetCharacterEditorTranslation("Limbs");
				GUIFont font = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT, text, null, font, Alignment.Left, false, "", null);
				CS$<>8__locals1.limbsList = new GUIListBox(new RectTransform(new Vector2(1f, 0.45f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					PlaySoundOnSelect = true
				};
				Vector2 limbButtonSize = Vector2.One * 0.8f;
				new GUIButton(new RectTransform(limbButtonSize, limbEditLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIMinusButton", null).OnClicked = delegate(GUIButton b, object d)
				{
					GUIComponent element2 = CS$<>8__locals1.<>4__this.LimbGUIElements.LastOrDefault<GUIComponent>();
					if (element2 == null)
					{
						return false;
					}
					element2.RectTransform.Parent = null;
					CS$<>8__locals1.<>4__this.LimbGUIElements.Remove(element2);
					return true;
				};
				new GUIButton(new RectTransform(limbButtonSize, limbEditLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUIPlusButton", null).OnClicked = delegate(GUIButton b, object d)
				{
					LimbType limbType = LimbType.None;
					int count = CS$<>8__locals1.<>4__this.LimbGUIElements.Count;
					if (count != 0)
					{
						if (count == 1)
						{
							limbType = LimbType.Head;
						}
					}
					else
					{
						limbType = LimbType.Torso;
					}
					CS$<>8__locals1.<>4__this.CreateLimbGUIElement(CS$<>8__locals1.limbsList.Content.RectTransform, CS$<>8__locals1.elementSize, CS$<>8__locals1.<>4__this.LimbGUIElements.Count, "", limbType, null);
					return true;
				};
				CS$<>8__locals1._x = 1;
				CS$<>8__locals1._y = 1;
				CS$<>8__locals1.w = 100;
				CS$<>8__locals1.h = 100;
				GUILayoutGroup inputArea = new GUILayoutGroup(new RectTransform(new Vector2(0.5f, 1f), limbEditLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft)
				{
					Stretch = true,
					RelativeSpacing = 0.01f
				};
				for (int i = 3; i >= 0; i--)
				{
					GUIFrame element = new GUIFrame(new RectTransform(new Vector2(0.22f, 1f), inputArea.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						MinSize = new Point(50, 0),
						MaxSize = new Point(150, 50)
					}, null, null);
					RectTransform rectT2 = new RectTransform(new Vector2(0.3f, 1f), element.RectTransform, Anchor.CenterLeft, null, null, null, ScaleBasis.Normal);
					RichString text2 = GUI.RectComponentLabels[i];
					font = GUIStyle.SmallFont;
					new GUITextBlock(rectT2, text2, null, font, Alignment.CenterLeft, false, "", null);
					GUINumberInput numberInput = new GUINumberInput(new RectTransform(new Vector2(0.7f, 1f), element.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
					{
						Font = GUIStyle.SmallFont
					};
					if (i > 1)
					{
						if (i - 2 <= 1)
						{
							numberInput.IntValue = 100;
							numberInput.MinValueInt = new int?(0);
							numberInput.MaxValueInt = new int?(999);
						}
					}
					else
					{
						numberInput.IntValue = 1;
						numberInput.MinValueInt = new int?(1);
						numberInput.MaxValueInt = new int?(100);
					}
					int comp = i;
					GUINumberInput guinumberInput = numberInput;
					guinumberInput.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(guinumberInput.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput numInput)
					{
						switch (comp)
						{
						case 0:
							CS$<>8__locals1._x = numInput.IntValue;
							return;
						case 1:
							CS$<>8__locals1._y = numInput.IntValue;
							return;
						case 2:
							CS$<>8__locals1.w = numInput.IntValue;
							return;
						case 3:
							CS$<>8__locals1.h = numInput.IntValue;
							return;
						default:
							return;
						}
					}));
				}
				inputArea.Recalculate();
				new GUIButton(new RectTransform(new Vector2(0.15f, 1f), limbEditLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("AddMultipleLimbsButton"), Alignment.Center, "", null).OnClicked = delegate(GUIButton b, object d)
				{
					base.<Create>g__CreateMultipleLimbs|1(CS$<>8__locals1._x, CS$<>8__locals1._y);
					return true;
				};
				limbsElement.RectTransform.MinSize = new Point(0, limbEditLayout.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
				if (base.LimbGUIElements.None(null))
				{
					if (base.IsHumanoid)
					{
						CS$<>8__locals1.<Create>g__CreateMultipleLimbs|1(2, 6);
						this.CreateLimbGUIElement(CS$<>8__locals1.limbsList.Content.RectTransform, CS$<>8__locals1.elementSize, base.LimbGUIElements.Count, "", LimbType.Waist, new Rectangle?(new Rectangle(CS$<>8__locals1._x, CS$<>8__locals1.h * base.LimbGUIElements.Count / 2, CS$<>8__locals1.w, CS$<>8__locals1.h)));
					}
					else
					{
						CS$<>8__locals1.<Create>g__CreateMultipleLimbs|1(1, 2);
					}
				}
				new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null).CanBeFocused = false;
				GUIFrame jointsElement = new GUIFrame(new RectTransform(new Vector2(1f, 0.05f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
				{
					CanBeFocused = false
				};
				RectTransform rectT3 = new RectTransform(new Vector2(0.2f, 1f), jointsElement.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
				RichString text3 = Wizard.GetCharacterEditorTranslation("Joints");
				font = GUIStyle.SubHeadingFont;
				new GUITextBlock(rectT3, text3, null, font, Alignment.Left, false, "", null);
				GUIFrame jointButtonElement = new GUIFrame(new RectTransform(new Vector2(0.5f, 1f), jointsElement.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
				{
					RelativeOffset = new Vector2(0.15f, 0f)
				}, null, null)
				{
					CanBeFocused = false
				};
				CS$<>8__locals1.jointsList = new GUIListBox(new RectTransform(new Vector2(1f, 0.45f), content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					PlaySoundOnSelect = true
				};
				GUIButton removeJointButton = new GUIButton(new RectTransform(new Point(jointButtonElement.Rect.Height, jointButtonElement.Rect.Height), jointButtonElement.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), Alignment.Center, "GUIMinusButton", null)
				{
					OnClicked = delegate(GUIButton b, object d)
					{
						GUIComponent element2 = CS$<>8__locals1.<>4__this.JointGUIElements.LastOrDefault<GUIComponent>();
						if (element2 == null)
						{
							return false;
						}
						element2.RectTransform.Parent = null;
						CS$<>8__locals1.<>4__this.JointGUIElements.Remove(element2);
						return true;
					}
				};
				new GUIButton(new RectTransform(new Point(jointButtonElement.Rect.Height), jointButtonElement.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false)
				{
					AbsoluteOffset = new Point(removeJointButton.Rect.Width + 10, 0)
				}, Alignment.Center, "GUIPlusButton", null).OnClicked = delegate(GUIButton b, object d)
				{
					CS$<>8__locals1.<>4__this.CreateJointGUIElement(CS$<>8__locals1.jointsList.Content.RectTransform, CS$<>8__locals1.elementSize, 0, 1, null, null, "");
					return true;
				};
				GUIButton guibutton = box.Buttons[0];
				guibutton.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					Wizard.Instance.SelectTab(Wizard.Tab.Character);
					return true;
				}));
				GUIButton guibutton2 = box.Buttons[1];
				guibutton2.OnClicked = (GUIButton.OnClickedHandler)Delegate.Combine(guibutton2.OnClicked, new GUIButton.OnClickedHandler(delegate(GUIButton b, object d)
				{
					CS$<>8__locals1.<>4__this.ParseLimbsFromGUIElements();
					CS$<>8__locals1.<>4__this.ParseJointsFromGUIElements();
					XAttribute xattribute;
					if ((xattribute = (from xe in CS$<>8__locals1.<>4__this.LimbXElements.Values
					select xe.Attribute("type") into a
					where a.Value.Equals("torso", StringComparison.OrdinalIgnoreCase)
					select a).FirstOrDefault<XAttribute>()) == null)
					{
						xattribute = (from xe in CS$<>8__locals1.<>4__this.LimbXElements.Values
						select xe.Attribute("type") into a
						where a.Value.Equals("head", StringComparison.OrdinalIgnoreCase)
						select a).FirstOrDefault<XAttribute>();
					}
					XAttribute main = xattribute;
					if (main == null)
					{
						GUI.AddMessage(Wizard.GetCharacterEditorTranslation("MissingTorsoOrHead"), GUIStyle.Red, null, true, null);
						return false;
					}
					string missingType;
					if (CS$<>8__locals1.<>4__this.IsHumanoid && !Wizard.View.IsValid(CS$<>8__locals1.<>4__this.LimbXElements.Values, true, out missingType))
					{
						GUI.AddMessage(Wizard.GetCharacterEditorTranslation("MissingLimbType").Replace("[limbtype]", missingType.FormatCamelCaseWithSpaces(), StringComparison.Ordinal), GUIStyle.Red, null, true, null);
						return false;
					}
					XElement mainLimb = main.Parent;
					int radius = mainLimb.GetAttributeInt("radius", -1);
					int height = mainLimb.GetAttributeInt("height", -1);
					int width = mainLimb.GetAttributeInt("width", -1);
					int colliderHeight = -1;
					if (radius == -1)
					{
						if (width == height)
						{
							radius = width / 2;
							colliderHeight = width - radius * 2;
						}
						else if (height > width)
						{
							radius = width / 2;
							colliderHeight = height - radius * 2;
						}
						else
						{
							radius = height / 2;
							colliderHeight = width - radius * 2;
						}
						radius = Math.Max(radius, 1);
					}
					else if (height > -1 || width > -1)
					{
						colliderHeight = ((width > height) ? width : height);
					}
					List<XAttribute> colliderAttributes = new List<XAttribute>
					{
						new XAttribute("radius", radius)
					};
					if (colliderHeight > -1)
					{
						colliderHeight = Math.Max(colliderHeight, 1);
						if (height > width)
						{
							colliderAttributes.Add(new XAttribute("height", colliderHeight));
						}
						else
						{
							colliderAttributes.Add(new XAttribute("width", colliderHeight));
						}
					}
					List<XElement> colliderElements = new List<XElement>
					{
						new XElement("collider", colliderAttributes)
					};
					if (CS$<>8__locals1.<>4__this.IsHumanoid)
					{
						XElement secondaryCollider = new XElement("collider", new XAttribute("radius", radius));
						if (colliderHeight > -1)
						{
							colliderHeight = Math.Max(colliderHeight, 1);
							if (height > width)
							{
								secondaryCollider.Add(new XAttribute("height", (float)colliderHeight * 0.75f));
							}
							else
							{
								secondaryCollider.Add(new XAttribute("width", (float)colliderHeight * 0.75f));
							}
						}
						colliderElements.Add(secondaryCollider);
					}
					XElement mainElement = new XElement("Ragdoll", new object[]
					{
						new XAttribute("type", CS$<>8__locals1.<>4__this.Name),
						new XAttribute("texture", CS$<>8__locals1.<>4__this.TexturePath),
						new XAttribute("canentersubmarine", CS$<>8__locals1.<>4__this.CanEnterSubmarine),
						new XAttribute("canwalk", CS$<>8__locals1.<>4__this.CanWalk),
						colliderElements,
						CS$<>8__locals1.<>4__this.LimbXElements.Values,
						CS$<>8__locals1.<>4__this.JointXElements
					});
					Wizard.Instance.CreateCharacter(mainElement, null, null);
					return true;
				}));
				return box;
			}

			// Token: 0x06009154 RID: 37204 RVA: 0x003C471C File Offset: 0x003C291C
			private void CreateLimbGUIElement(RectTransform parent, int elementSize, int id, string name = "", LimbType limbType = LimbType.None, Rectangle? sourceRect = null)
			{
				Wizard.RagdollView.<>c__DisplayClass4_0 CS$<>8__locals1 = new Wizard.RagdollView.<>c__DisplayClass4_0();
				CS$<>8__locals1.id = id;
				GUIFrame limbElement = new GUIFrame(new RectTransform(new Point(parent.Rect.Width, elementSize * 5 + 40), parent, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(Color.Gray * 0.25f))
				{
					CanBeFocused = false
				};
				GUILayoutGroup group = new GUILayoutGroup(new RectTransform(Vector2.One, limbElement.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					AbsoluteSpacing = 16
				};
				Wizard.RagdollView.<>c__DisplayClass4_0 CS$<>8__locals2 = CS$<>8__locals1;
				RectTransform rectT = new RectTransform(new Point(group.Rect.Width, elementSize), group.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				RichString text2 = name;
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				CS$<>8__locals2.label = new GUITextBlock(rectT, text2, null, subHeadingFont, Alignment.Left, false, "", null);
				GUIFrame idField = new GUIFrame(new RectTransform(new Point(group.Rect.Width, elementSize), group.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
				CS$<>8__locals1.nameField = new GUIFrame(new RectTransform(new Point(group.Rect.Width, elementSize), group.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
				GUIComponent limbTypeField = GUI.CreateEnumField(limbType, elementSize, Wizard.GetCharacterEditorTranslation("LimbType"), group.RectTransform, null, GUIStyle.Font);
				GUIComponent sourceRectField = GUI.CreateRectangleField(sourceRect ?? new Rectangle(0, 100 * base.LimbGUIElements.Count, 100, 100), elementSize, Wizard.GetCharacterEditorTranslation("SourceRectangle"), group.RectTransform, null, GUIStyle.Font);
				new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), idField.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("ID"), null, null, Alignment.Left, false, "", null);
				GUINumberInput guinumberInput = new GUINumberInput(new RectTransform(new Vector2(0.5f, 1f), idField.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null);
				guinumberInput.MinValueInt = new int?(0);
				guinumberInput.MaxValueInt = new int?(255);
				guinumberInput.IntValue = CS$<>8__locals1.id;
				guinumberInput.OnValueChanged = delegate(GUINumberInput numInput)
				{
					CS$<>8__locals1.id = numInput.IntValue;
					string text2 = CS$<>8__locals1.nameField.GetChild<GUITextBox>().Text;
					string t = string.IsNullOrWhiteSpace(text2) ? CS$<>8__locals1.id.ToString() : text2;
					CS$<>8__locals1.label.Text = t;
				};
				new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), CS$<>8__locals1.nameField.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Name"), null, null, Alignment.Left, false, "", null);
				GUITextBox nameInput = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), CS$<>8__locals1.nameField.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), name, null, null, Alignment.Left, false, "", null, false, true)
				{
					CaretColor = new Color?(Color.White)
				};
				nameInput.OnTextChanged += delegate(GUITextBox tb, string text)
				{
					string t = string.IsNullOrWhiteSpace(text) ? CS$<>8__locals1.id.ToString() : text;
					CS$<>8__locals1.label.Text = t;
					return true;
				};
				base.LimbGUIElements.Add(limbElement);
			}

			// Token: 0x06009155 RID: 37205 RVA: 0x003C4B10 File Offset: 0x003C2D10
			private void CreateJointGUIElement(RectTransform parent, int elementSize, int id1 = 0, int id2 = 1, Vector2? anchor1 = null, Vector2? anchor2 = null, string jointName = "")
			{
				Wizard.RagdollView.<>c__DisplayClass5_0 CS$<>8__locals1 = new Wizard.RagdollView.<>c__DisplayClass5_0();
				CS$<>8__locals1.jointName = jointName;
				GUIFrame jointElement = new GUIFrame(new RectTransform(new Point(parent.Rect.Width, elementSize * 6 + 40), parent, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, new Color?(Color.Gray * 0.25f))
				{
					CanBeFocused = false
				};
				GUILayoutGroup group = new GUILayoutGroup(new RectTransform(Vector2.One, jointElement.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft)
				{
					AbsoluteSpacing = 2
				};
				Wizard.RagdollView.<>c__DisplayClass5_0 CS$<>8__locals2 = CS$<>8__locals1;
				RectTransform rectT = new RectTransform(new Point(group.Rect.Width, elementSize), group.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false);
				RichString text2 = CS$<>8__locals1.jointName;
				GUIFont subHeadingFont = GUIStyle.SubHeadingFont;
				CS$<>8__locals2.label = new GUITextBlock(rectT, text2, null, subHeadingFont, Alignment.Left, false, "", null);
				GUIFrame nameField = new GUIFrame(new RectTransform(new Point(group.Rect.Width, elementSize), group.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
				new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), nameField.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Name"), null, null, Alignment.Left, false, "", null);
				GUITextBox nameInput = new GUITextBox(new RectTransform(new Vector2(0.5f, 1f), nameField.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), CS$<>8__locals1.jointName, null, null, Alignment.Left, false, "", null, false, true)
				{
					CaretColor = new Color?(Color.White)
				};
				nameInput.OnTextChanged += delegate(GUITextBox textB, string text)
				{
					CS$<>8__locals1.jointName = text;
					CS$<>8__locals1.label.Text = CS$<>8__locals1.jointName;
					return true;
				};
				GUIFrame limb1Field = new GUIFrame(new RectTransform(new Point(group.Rect.Width, elementSize), group.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
				new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), limb1Field.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("LimbWithIndex").Replace("[index]", "1", StringComparison.Ordinal), null, null, Alignment.Left, false, "", null);
				CS$<>8__locals1.limb1InputField = new GUINumberInput(new RectTransform(new Vector2(0.5f, 1f), limb1Field.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					MinValueInt = new int?(0),
					MaxValueInt = new int?(255),
					IntValue = id1
				};
				GUIFrame limb2Field = new GUIFrame(new RectTransform(new Point(group.Rect.Width, elementSize), group.RectTransform, Anchor.TopLeft, null, ScaleBasis.Normal, false), null, null);
				new GUITextBlock(new RectTransform(new Vector2(0.5f, 1f), limb2Field.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), Wizard.GetCharacterEditorTranslation("LimbWithIndex").Replace("[index]", "2", StringComparison.Ordinal), null, null, Alignment.Left, false, "", null);
				CS$<>8__locals1.limb2InputField = new GUINumberInput(new RectTransform(new Vector2(0.5f, 1f), limb2Field.RectTransform, Anchor.TopRight, null, null, null, ScaleBasis.Normal), NumberType.Int, "", Alignment.Center, null, GUINumberInput.ButtonVisibility.Automatic, null)
				{
					MinValueInt = new int?(0),
					MaxValueInt = new int?(255),
					IntValue = id2
				};
				GUI.CreateVector2Field(anchor1 ?? Vector2.Zero, elementSize, Wizard.GetCharacterEditorTranslation("LimbWithIndexAnchor").Replace("[index]", "1", StringComparison.Ordinal), group.RectTransform, null, GUIStyle.Font, 2);
				GUI.CreateVector2Field(anchor2 ?? Vector2.Zero, elementSize, Wizard.GetCharacterEditorTranslation("LimbWithIndexAnchor").Replace("[index]", "2", StringComparison.Ordinal), group.RectTransform, null, GUIStyle.Font, 2);
				CS$<>8__locals1.label.Text = CS$<>8__locals1.<CreateJointGUIElement>g__GetJointName|1(CS$<>8__locals1.jointName);
				GUINumberInput limb1InputField = CS$<>8__locals1.limb1InputField;
				limb1InputField.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(limb1InputField.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput nInput)
				{
					CS$<>8__locals1.label.Text = base.<CreateJointGUIElement>g__GetJointName|1(CS$<>8__locals1.jointName);
				}));
				GUINumberInput limb2InputField = CS$<>8__locals1.limb2InputField;
				limb2InputField.OnValueChanged = (GUINumberInput.OnValueChangedHandler)Delegate.Combine(limb2InputField.OnValueChanged, new GUINumberInput.OnValueChangedHandler(delegate(GUINumberInput nInput)
				{
					CS$<>8__locals1.label.Text = base.<CreateJointGUIElement>g__GetJointName|1(CS$<>8__locals1.jointName);
				}));
				base.JointGUIElements.Add(jointElement);
			}

			// Token: 0x04005CB6 RID: 23734
			private static Wizard.RagdollView instance;
		}

		// Token: 0x020011B1 RID: 4529
		private abstract class View
		{
			// Token: 0x17001C9F RID: 7327
			// (get) Token: 0x06009157 RID: 37207 RVA: 0x003C50E4 File Offset: 0x003C32E4
			public bool IsCopy
			{
				get
				{
					return Wizard.Instance.IsCopy;
				}
			}

			// Token: 0x17001CA0 RID: 7328
			// (get) Token: 0x06009158 RID: 37208 RVA: 0x003C50F0 File Offset: 0x003C32F0
			public IEnumerable<AnimationParams> SourceAnimations
			{
				get
				{
					return Wizard.Instance.SourceAnimations;
				}
			}

			// Token: 0x17001CA1 RID: 7329
			// (get) Token: 0x06009159 RID: 37209 RVA: 0x003C50FC File Offset: 0x003C32FC
			public CharacterParams SourceCharacter
			{
				get
				{
					return Wizard.Instance.SourceCharacter;
				}
			}

			// Token: 0x17001CA2 RID: 7330
			// (get) Token: 0x0600915A RID: 37210 RVA: 0x003C5108 File Offset: 0x003C3308
			public RagdollParams SourceRagdoll
			{
				get
				{
					return Wizard.Instance.SourceRagdoll;
				}
			}

			// Token: 0x17001CA3 RID: 7331
			// (get) Token: 0x0600915B RID: 37211 RVA: 0x003C5114 File Offset: 0x003C3314
			// (set) Token: 0x0600915C RID: 37212 RVA: 0x003C5120 File Offset: 0x003C3320
			public Identifier Name
			{
				get
				{
					return Wizard.Instance.name;
				}
				set
				{
					Wizard.Instance.name = value;
				}
			}

			// Token: 0x17001CA4 RID: 7332
			// (get) Token: 0x0600915D RID: 37213 RVA: 0x003C512D File Offset: 0x003C332D
			// (set) Token: 0x0600915E RID: 37214 RVA: 0x003C5139 File Offset: 0x003C3339
			public bool IsHumanoid
			{
				get
				{
					return Wizard.Instance.isHumanoid;
				}
				set
				{
					Wizard.Instance.isHumanoid = value;
				}
			}

			// Token: 0x17001CA5 RID: 7333
			// (get) Token: 0x0600915F RID: 37215 RVA: 0x003C5146 File Offset: 0x003C3346
			// (set) Token: 0x06009160 RID: 37216 RVA: 0x003C5152 File Offset: 0x003C3352
			public CanEnterSubmarine CanEnterSubmarine
			{
				get
				{
					return Wizard.Instance.canEnterSubmarine;
				}
				set
				{
					Wizard.Instance.canEnterSubmarine = value;
				}
			}

			// Token: 0x17001CA6 RID: 7334
			// (get) Token: 0x06009161 RID: 37217 RVA: 0x003C515F File Offset: 0x003C335F
			// (set) Token: 0x06009162 RID: 37218 RVA: 0x003C516B File Offset: 0x003C336B
			public bool CanWalk
			{
				get
				{
					return Wizard.Instance.canWalk;
				}
				set
				{
					Wizard.Instance.canWalk = value;
				}
			}

			// Token: 0x17001CA7 RID: 7335
			// (get) Token: 0x06009163 RID: 37219 RVA: 0x003C5178 File Offset: 0x003C3378
			// (set) Token: 0x06009164 RID: 37220 RVA: 0x003C5184 File Offset: 0x003C3384
			public ContentPackage ContentPackage
			{
				get
				{
					return Wizard.Instance.contentPackage;
				}
				set
				{
					Wizard.Instance.contentPackage = value;
				}
			}

			// Token: 0x17001CA8 RID: 7336
			// (get) Token: 0x06009165 RID: 37221 RVA: 0x003C5191 File Offset: 0x003C3391
			// (set) Token: 0x06009166 RID: 37222 RVA: 0x003C519D File Offset: 0x003C339D
			public string TexturePath
			{
				get
				{
					return Wizard.Instance.texturePath;
				}
				set
				{
					Wizard.Instance.texturePath = value;
				}
			}

			// Token: 0x17001CA9 RID: 7337
			// (get) Token: 0x06009167 RID: 37223 RVA: 0x003C51AA File Offset: 0x003C33AA
			// (set) Token: 0x06009168 RID: 37224 RVA: 0x003C51B6 File Offset: 0x003C33B6
			public string XMLPath
			{
				get
				{
					return Wizard.Instance.xmlPath;
				}
				set
				{
					Wizard.Instance.xmlPath = value;
				}
			}

			// Token: 0x17001CAA RID: 7338
			// (get) Token: 0x06009169 RID: 37225 RVA: 0x003C51C3 File Offset: 0x003C33C3
			// (set) Token: 0x0600916A RID: 37226 RVA: 0x003C51CF File Offset: 0x003C33CF
			public Dictionary<string, XElement> LimbXElements
			{
				get
				{
					return Wizard.Instance.limbXElements;
				}
				set
				{
					Wizard.Instance.limbXElements = value;
				}
			}

			// Token: 0x17001CAB RID: 7339
			// (get) Token: 0x0600916B RID: 37227 RVA: 0x003C51DC File Offset: 0x003C33DC
			// (set) Token: 0x0600916C RID: 37228 RVA: 0x003C51E8 File Offset: 0x003C33E8
			public List<GUIComponent> LimbGUIElements
			{
				get
				{
					return Wizard.Instance.limbGUIElements;
				}
				set
				{
					Wizard.Instance.limbGUIElements = value;
				}
			}

			// Token: 0x17001CAC RID: 7340
			// (get) Token: 0x0600916D RID: 37229 RVA: 0x003C51F5 File Offset: 0x003C33F5
			// (set) Token: 0x0600916E RID: 37230 RVA: 0x003C5201 File Offset: 0x003C3401
			public List<XElement> JointXElements
			{
				get
				{
					return Wizard.Instance.jointXElements;
				}
				set
				{
					Wizard.Instance.jointXElements = value;
				}
			}

			// Token: 0x17001CAD RID: 7341
			// (get) Token: 0x0600916F RID: 37231 RVA: 0x003C520E File Offset: 0x003C340E
			// (set) Token: 0x06009170 RID: 37232 RVA: 0x003C521A File Offset: 0x003C341A
			public List<GUIComponent> JointGUIElements
			{
				get
				{
					return Wizard.Instance.jointGUIElements;
				}
				set
				{
					Wizard.Instance.jointGUIElements = value;
				}
			}

			// Token: 0x17001CAE RID: 7342
			// (get) Token: 0x06009171 RID: 37233 RVA: 0x003C5227 File Offset: 0x003C3427
			public GUIMessageBox Box
			{
				get
				{
					if (this.box == null)
					{
						this.box = this.Create();
					}
					return this.box;
				}
			}

			// Token: 0x06009172 RID: 37234
			protected abstract GUIMessageBox Create();

			// Token: 0x06009173 RID: 37235 RVA: 0x003C5243 File Offset: 0x003C3443
			protected static T Get<T>(ref T instance) where T : Wizard.View, new()
			{
				if (instance == null)
				{
					instance = Activator.CreateInstance<T>();
				}
				return instance;
			}

			// Token: 0x06009174 RID: 37236
			public abstract void Release();

			// Token: 0x06009175 RID: 37237 RVA: 0x003C5264 File Offset: 0x003C3464
			protected void ParseLimbsFromGUIElements()
			{
				this.LimbXElements.Clear();
				for (int i = 0; i < this.LimbGUIElements.Count; i++)
				{
					GUIComponent limbGUIElement = this.LimbGUIElements[i];
					Wizard.View.<>c__DisplayClass47_0 CS$<>8__locals1;
					CS$<>8__locals1.allChildren = limbGUIElement.GetAllChildren();
					int id = Wizard.View.<ParseLimbsFromGUIElements>g__GetField|47_0(Wizard.GetCharacterEditorTranslation("ID"), ref CS$<>8__locals1).Parent.GetChild<GUINumberInput>().IntValue;
					string limbName = Wizard.View.<ParseLimbsFromGUIElements>g__GetField|47_0(TextManager.Get("Name"), ref CS$<>8__locals1).Parent.GetChild<GUITextBox>().Text;
					LimbType limbType = (LimbType)Wizard.View.<ParseLimbsFromGUIElements>g__GetField|47_0(Wizard.GetCharacterEditorTranslation("LimbType"), ref CS$<>8__locals1).Parent.GetChild<GUIDropDown>().SelectedData;
					GUINumberInput[] rectInputs = Wizard.View.<ParseLimbsFromGUIElements>g__GetField|47_0(Wizard.GetCharacterEditorTranslation("SourceRectangle"), ref CS$<>8__locals1).Parent.GetAllChildren<GUINumberInput>().Reverse<GUINumberInput>().ToArray<GUINumberInput>();
					int width = rectInputs[2].IntValue;
					int height = rectInputs[3].IntValue;
					List<XAttribute> colliderAttributes = new List<XAttribute>();
					colliderAttributes.Add(new XAttribute("height", (int)((float)height * 0.85f)));
					colliderAttributes.Add(new XAttribute("width", (int)((float)width * 0.85f)));
					Dictionary<string, XElement> limbXElements = this.LimbXElements;
					string key = id.ToString();
					XName name = "limb";
					object[] array = new object[6];
					array[0] = new XAttribute("id", id);
					array[1] = new XAttribute("name", limbName);
					array[2] = new XAttribute("type", limbType.ToString());
					array[3] = colliderAttributes;
					int num = 4;
					XName name2 = "sprite";
					object[] array2 = new object[2];
					array2[0] = new XAttribute("texture", "");
					int num2 = 1;
					XName name3 = "sourcerect";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(6, 4);
					defaultInterpolatedStringHandler.AppendFormatted<int>(rectInputs[0].IntValue);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(rectInputs[1].IntValue);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(width);
					defaultInterpolatedStringHandler.AppendLiteral(", ");
					defaultInterpolatedStringHandler.AppendFormatted<int>(height);
					array2[num2] = new XAttribute(name3, defaultInterpolatedStringHandler.ToStringAndClear());
					array[num] = new XElement(name2, array2);
					array[5] = new XAttribute("notes", string.Empty);
					limbXElements.Add(key, new XElement(name, array));
				}
			}

			// Token: 0x06009176 RID: 37238 RVA: 0x003C54E0 File Offset: 0x003C36E0
			protected void ParseJointsFromGUIElements()
			{
				this.JointXElements.Clear();
				for (int i = 0; i < this.JointGUIElements.Count; i++)
				{
					GUIComponent jointGUIElement = this.JointGUIElements[i];
					Wizard.View.<>c__DisplayClass48_0 CS$<>8__locals1;
					CS$<>8__locals1.allChildren = jointGUIElement.GetAllChildren();
					string jointName = Wizard.View.<ParseJointsFromGUIElements>g__GetField|48_0(TextManager.Get("Name"), ref CS$<>8__locals1).Parent.GetChild<GUITextBox>().Text;
					int limb1ID = Wizard.View.<ParseJointsFromGUIElements>g__GetField|48_0(Wizard.GetCharacterEditorTranslation("LimbWithIndex").Replace("[index]", "1", StringComparison.Ordinal), ref CS$<>8__locals1).Parent.GetChild<GUINumberInput>().IntValue;
					int limb2ID = Wizard.View.<ParseJointsFromGUIElements>g__GetField|48_0(Wizard.GetCharacterEditorTranslation("LimbWithIndex").Replace("[index]", "2", StringComparison.Ordinal), ref CS$<>8__locals1).Parent.GetChild<GUINumberInput>().IntValue;
					GUINumberInput[] anchor1Inputs = Wizard.View.<ParseJointsFromGUIElements>g__GetField|48_0(Wizard.GetCharacterEditorTranslation("LimbWithIndexAnchor").Replace("[index]", "1", StringComparison.Ordinal), ref CS$<>8__locals1).Parent.GetAllChildren<GUINumberInput>().Reverse<GUINumberInput>().ToArray<GUINumberInput>();
					GUINumberInput[] anchor2Inputs = Wizard.View.<ParseJointsFromGUIElements>g__GetField|48_0(Wizard.GetCharacterEditorTranslation("LimbWithIndexAnchor").Replace("[index]", "2", StringComparison.Ordinal), ref CS$<>8__locals1).Parent.GetAllChildren<GUINumberInput>().Reverse<GUINumberInput>().ToArray<GUINumberInput>();
					this.JointXElements.Add(new XElement("joint", new object[]
					{
						new XAttribute("name", jointName),
						new XAttribute("limb1", limb1ID),
						new XAttribute("limb2", limb2ID),
						new XAttribute("limb1anchor", anchor1Inputs[0].FloatValue.Format(2) + ", " + anchor1Inputs[1].FloatValue.Format(2)),
						new XAttribute("limb2anchor", anchor2Inputs[0].FloatValue.Format(2) + ", " + anchor2Inputs[1].FloatValue.Format(2))
					}));
				}
			}

			// Token: 0x06009177 RID: 37239 RVA: 0x003C5710 File Offset: 0x003C3910
			protected LimbType ParseLimbType(string limbName)
			{
				LimbType limbType = LimbType.None;
				string i = limbName.ToLowerInvariant();
				if (!(i == "head"))
				{
					if (!(i == "torso"))
					{
						if (!(i == "waist") && !(i == "pelvis"))
						{
							if (i == "tail")
							{
								limbType = LimbType.Tail;
							}
						}
						else
						{
							limbType = LimbType.Waist;
						}
					}
					else
					{
						limbType = LimbType.Torso;
					}
				}
				else
				{
					limbType = LimbType.Head;
				}
				if (limbType == LimbType.None)
				{
					if (i.Contains("tail"))
					{
						limbType = LimbType.Tail;
					}
					else if (i.Contains("arm") && !i.Contains("lower"))
					{
						if (i.Contains("right"))
						{
							limbType = LimbType.RightArm;
						}
						else if (i.Contains("left"))
						{
							limbType = LimbType.LeftArm;
						}
					}
					else if (i.Contains("hand") || i.Contains("palm"))
					{
						if (i.Contains("right"))
						{
							limbType = LimbType.RightHand;
						}
						else if (i.Contains("left"))
						{
							limbType = LimbType.LeftHand;
						}
					}
					else if (i.Contains("thigh") || i.Contains("upperleg"))
					{
						if (i.Contains("right"))
						{
							limbType = LimbType.RightThigh;
						}
						else if (i.Contains("left"))
						{
							limbType = LimbType.LeftThigh;
						}
					}
					else if (i.Contains("shin") || i.Contains("lowerleg"))
					{
						if (i.Contains("right"))
						{
							limbType = LimbType.RightLeg;
						}
						else if (i.Contains("left"))
						{
							limbType = LimbType.LeftLeg;
						}
					}
					else if (i.Contains("foot"))
					{
						if (i.Contains("right"))
						{
							limbType = LimbType.RightFoot;
						}
						else if (i.Contains("left"))
						{
							limbType = LimbType.LeftFoot;
						}
					}
				}
				return limbType;
			}

			// Token: 0x06009178 RID: 37240 RVA: 0x003C58CC File Offset: 0x003C3ACC
			public static bool IsValid(IEnumerable<XElement> elements, bool isHumanoid, out string missingType)
			{
				missingType = "none";
				if (!Wizard.View.HasAtLeastOneLimbOfType(elements, "torso") && !Wizard.View.HasAtLeastOneLimbOfType(elements, "head"))
				{
					missingType = "TorsoOrHead";
					return false;
				}
				if (isHumanoid)
				{
					string type;
					missingType = (type = "LeftArm");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "LeftHand");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "RightArm");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "RightHand");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "Waist");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "LeftThigh");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "LeftLeg");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "LeftFoot");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "RightThigh");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "RightLeg");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
					missingType = (type = "RightFoot");
					if (!Wizard.View.HasOnlyOneLimbOfType(elements, type))
					{
						return false;
					}
				}
				return true;
			}

			// Token: 0x06009179 RID: 37241 RVA: 0x003C59E8 File Offset: 0x003C3BE8
			public static bool HasAtLeastOneLimbOfType(IEnumerable<XElement> elements, string type)
			{
				return elements.Any((XElement e) => Wizard.View.IsType(e, type));
			}

			// Token: 0x0600917A RID: 37242 RVA: 0x003C5A14 File Offset: 0x003C3C14
			public static bool HasOnlyOneLimbOfType(IEnumerable<XElement> elements, string type)
			{
				return elements.Count((XElement e) => Wizard.View.IsType(e, type)) == 1;
			}

			// Token: 0x0600917B RID: 37243 RVA: 0x003C5A43 File Offset: 0x003C3C43
			private static bool IsType(XElement element, string type)
			{
				return element.GetAttributeString("type", "").Equals(type, StringComparison.OrdinalIgnoreCase);
			}

			// Token: 0x0600917D RID: 37245 RVA: 0x003C5A64 File Offset: 0x003C3C64
			[CompilerGenerated]
			internal static GUITextBlock <ParseLimbsFromGUIElements>g__GetField|47_0(LocalizedString n, ref Wizard.View.<>c__DisplayClass47_0 A_1)
			{
				return A_1.allChildren.First(delegate(GUIComponent c)
				{
					GUITextBlock textBlock = c as GUITextBlock;
					return textBlock != null && textBlock.Text == n;
				}) as GUITextBlock;
			}

			// Token: 0x0600917E RID: 37246 RVA: 0x003C5A9C File Offset: 0x003C3C9C
			[CompilerGenerated]
			internal static GUITextBlock <ParseJointsFromGUIElements>g__GetField|48_0(LocalizedString n, ref Wizard.View.<>c__DisplayClass48_0 A_1)
			{
				return A_1.allChildren.First(delegate(GUIComponent c)
				{
					GUITextBlock textBlock = c as GUITextBlock;
					return textBlock != null && textBlock.Text == n;
				}) as GUITextBlock;
			}

			// Token: 0x04005CB7 RID: 23735
			private GUIMessageBox box;
		}
	}
}
