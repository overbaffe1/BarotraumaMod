using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000079 RID: 121
	[NullableContext(2)]
	[Nullable(0)]
	public static class FileSelection
	{
		// Token: 0x17000437 RID: 1079
		// (get) Token: 0x06001124 RID: 4388 RVA: 0x000A89D1 File Offset: 0x000A6BD1
		// (set) Token: 0x06001125 RID: 4389 RVA: 0x000A89D8 File Offset: 0x000A6BD8
		public static bool Open
		{
			get
			{
				return FileSelection.open;
			}
			set
			{
				if (value)
				{
					FileSelection.InitIfNecessary();
				}
				if (!value)
				{
					FileSystemWatcher fileSystemWatcher = FileSelection.fileSystemWatcher;
					if (fileSystemWatcher != null)
					{
						fileSystemWatcher.Dispose();
					}
					FileSelection.fileSystemWatcher = null;
				}
				FileSelection.open = value;
			}
		}

		// Token: 0x17000438 RID: 1080
		// (get) Token: 0x06001126 RID: 4390 RVA: 0x000A8A01 File Offset: 0x000A6C01
		// (set) Token: 0x06001127 RID: 4391 RVA: 0x000A8A08 File Offset: 0x000A6C08
		[Nullable(1)]
		public static string CurrentDirectory
		{
			[NullableContext(1)]
			get
			{
				return FileSelection.currentDirectory;
			}
			[NullableContext(1)]
			set
			{
				string[] dirSplit = value.Replace('\\', '/').Split('/', StringSplitOptions.None);
				List<string> dirs = new List<string>();
				for (int i = 0; i < dirSplit.Length; i++)
				{
					if (dirSplit[i].Trim() == "..")
					{
						if (dirs.Count > 1)
						{
							dirs.RemoveAt(dirs.Count - 1);
						}
					}
					else if (dirSplit[i].Trim() != ".")
					{
						dirs.Add(dirSplit[i]);
					}
				}
				FileSelection.currentDirectory = string.Join("/", dirs);
				if (!FileSelection.currentDirectory.EndsWith("/"))
				{
					FileSelection.currentDirectory += "/";
				}
				try
				{
					FileSystemWatcher fileSystemWatcher = FileSelection.fileSystemWatcher;
					if (fileSystemWatcher != null)
					{
						fileSystemWatcher.Dispose();
					}
					FileSelection.fileSystemWatcher = new FileSystemWatcher(FileSelection.currentDirectory)
					{
						Filter = "*",
						NotifyFilter = (NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite)
					};
					FileSystemWatcher fileSystemWatcher2 = FileSelection.fileSystemWatcher;
					FileSystemEventHandler value2;
					if ((value2 = FileSelection.<>O.<0>__OnFileSystemChanges) == null)
					{
						value2 = (FileSelection.<>O.<0>__OnFileSystemChanges = new FileSystemEventHandler(FileSelection.OnFileSystemChanges));
					}
					fileSystemWatcher2.Created += value2;
					FileSystemWatcher fileSystemWatcher3 = FileSelection.fileSystemWatcher;
					FileSystemEventHandler value3;
					if ((value3 = FileSelection.<>O.<0>__OnFileSystemChanges) == null)
					{
						value3 = (FileSelection.<>O.<0>__OnFileSystemChanges = new FileSystemEventHandler(FileSelection.OnFileSystemChanges));
					}
					fileSystemWatcher3.Deleted += value3;
					FileSystemWatcher fileSystemWatcher4 = FileSelection.fileSystemWatcher;
					RenamedEventHandler value4;
					if ((value4 = FileSelection.<>O.<1>__OnFileSystemChanges) == null)
					{
						value4 = (FileSelection.<>O.<1>__OnFileSystemChanges = new RenamedEventHandler(FileSelection.OnFileSystemChanges));
					}
					fileSystemWatcher4.Renamed += value4;
					FileSelection.fileSystemWatcher.EnableRaisingEvents = true;
				}
				catch (FileNotFoundException exception)
				{
					DebugConsole.ThrowError("Failed to set the current directory, possibly due to insufficient access permissions.", exception, null, false, false);
				}
				catch (ArgumentException exception2)
				{
					DebugConsole.ThrowError("Failed to set the current directory, possibly because it was deleted.", exception2, null, false, false);
				}
				catch (Exception exception3)
				{
					DebugConsole.ThrowError("Failed to set the current directory for an unknown reason.", exception3, null, false, false);
				}
				FileSelection.RefreshFileList();
			}
		}

		// Token: 0x17000439 RID: 1081
		// (get) Token: 0x06001128 RID: 4392 RVA: 0x000A8BD0 File Offset: 0x000A6DD0
		// (set) Token: 0x06001129 RID: 4393 RVA: 0x000A8BD7 File Offset: 0x000A6DD7
		[Nullable(new byte[]
		{
			2,
			1
		})]
		public static Action<string> OnFileSelected { [return: Nullable(new byte[]
		{
			2,
			1
		})] get; [param: Nullable(new byte[]
		{
			2,
			1
		})] set; }

		// Token: 0x0600112A RID: 4394 RVA: 0x000A8BE0 File Offset: 0x000A6DE0
		[NullableContext(1)]
		private static void OnFileSystemChanges(object sender, FileSystemEventArgs e)
		{
			if (FileSelection.fileList == null)
			{
				return;
			}
			WatcherChangeTypes changeType = e.ChangeType;
			object userData;
			if (changeType != WatcherChangeTypes.Created)
			{
				if (changeType != WatcherChangeTypes.Deleted)
				{
					if (changeType != WatcherChangeTypes.Renamed)
					{
						return;
					}
					FileSelection.<>c__DisplayClass25_1 CS$<>8__locals2 = new FileSelection.<>c__DisplayClass25_1();
					FileSelection.<>c__DisplayClass25_1 CS$<>8__locals3 = CS$<>8__locals2;
					RenamedEventArgs renamedEventArgs = e as RenamedEventArgs;
					if (renamedEventArgs == null)
					{
						throw new InvalidCastException("Unable to cast FileSystemEventArgs to RenamedEventArgs.");
					}
					CS$<>8__locals3.renameArgs = renamedEventArgs;
					GUITextBlock guitextBlock = FileSelection.fileList.Content.FindChild(delegate(GUIComponent c)
					{
						GUITextBlock tb = c as GUITextBlock;
						return tb != null && (tb.Text == CS$<>8__locals2.renameArgs.OldName || tb.Text == CS$<>8__locals2.renameArgs.OldName + "/");
					}, false) as GUITextBlock;
					if (guitextBlock == null)
					{
						throw new Exception("Could not find file list item with name \"" + CS$<>8__locals2.renameArgs.OldName + "\"");
					}
					GUITextBlock itemFrame = guitextBlock;
					itemFrame.UserData = (Directory.Exists(e.FullPath) ? FileSelection.ItemIsDirectory.Yes : FileSelection.ItemIsDirectory.No);
					itemFrame.Text = (CS$<>8__locals2.renameArgs.Name ?? string.Empty);
					userData = itemFrame.UserData;
					if (userData is FileSelection.ItemIsDirectory && (FileSelection.ItemIsDirectory)userData == FileSelection.ItemIsDirectory.Yes)
					{
						GUITextBlock guitextBlock2 = itemFrame;
						RichString text = guitextBlock2.Text;
						guitextBlock2.Text = ((text != null) ? text.ToString() : null) + "/";
					}
					RectTransform rectTransform = FileSelection.fileList.Content.RectTransform;
					Comparison<RectTransform> comparison;
					if ((comparison = FileSelection.<>O.<2>__SortFiles) == null)
					{
						comparison = (FileSelection.<>O.<2>__SortFiles = new Comparison<RectTransform>(FileSelection.SortFiles));
					}
					rectTransform.SortChildren(comparison);
				}
				else
				{
					GUIComponent itemFrame2 = FileSelection.fileList.Content.FindChild(delegate(GUIComponent c)
					{
						GUITextBlock tb = c as GUITextBlock;
						return tb != null && (tb.Text == e.Name || tb.Text == e.Name + "/");
					}, false);
					if (itemFrame2 != null)
					{
						FileSelection.fileList.RemoveChild(itemFrame2);
						return;
					}
				}
				return;
			}
			GUITextBlock itemFrame3 = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), FileSelection.fileList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), e.Name ?? string.Empty, null, null, Alignment.Left, false, "", null)
			{
				UserData = (Directory.Exists(e.FullPath) ? FileSelection.ItemIsDirectory.Yes : FileSelection.ItemIsDirectory.No)
			};
			userData = itemFrame3.UserData;
			if (userData is FileSelection.ItemIsDirectory && (FileSelection.ItemIsDirectory)userData == FileSelection.ItemIsDirectory.Yes)
			{
				GUITextBlock guitextBlock3 = itemFrame3;
				RichString text2 = guitextBlock3.Text;
				guitextBlock3.Text = ((text2 != null) ? text2.ToString() : null) + "/";
			}
			RectTransform rectTransform2 = FileSelection.fileList.Content.RectTransform;
			Comparison<RectTransform> comparison2;
			if ((comparison2 = FileSelection.<>O.<2>__SortFiles) == null)
			{
				comparison2 = (FileSelection.<>O.<2>__SortFiles = new Comparison<RectTransform>(FileSelection.SortFiles));
			}
			rectTransform2.SortChildren(comparison2);
		}

		// Token: 0x0600112B RID: 4395 RVA: 0x000A8E88 File Offset: 0x000A7088
		[NullableContext(1)]
		private static int SortFiles(RectTransform r1, RectTransform r2)
		{
			GUITextBlock guitextBlock = r1.GUIComponent as GUITextBlock;
			string text;
			if (guitextBlock == null)
			{
				text = null;
			}
			else
			{
				RichString text2 = guitextBlock.Text;
				text = ((text2 != null) ? text2.SanitizedValue : null);
			}
			string file = text ?? "";
			GUITextBlock guitextBlock2 = r2.GUIComponent as GUITextBlock;
			string text3;
			if (guitextBlock2 == null)
			{
				text3 = null;
			}
			else
			{
				RichString text4 = guitextBlock2.Text;
				text3 = ((text4 != null) ? text4.SanitizedValue : null);
			}
			string file2 = text3 ?? "";
			object userData = r1.GUIComponent.UserData;
			bool dir = userData is FileSelection.ItemIsDirectory && (FileSelection.ItemIsDirectory)userData == FileSelection.ItemIsDirectory.Yes;
			userData = r2.GUIComponent.UserData;
			bool dir2 = userData is FileSelection.ItemIsDirectory && (FileSelection.ItemIsDirectory)userData == FileSelection.ItemIsDirectory.Yes;
			if (dir && !dir2)
			{
				return -1;
			}
			if (!dir && dir2)
			{
				return 1;
			}
			return string.Compare(file, file2, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x0600112C RID: 4396 RVA: 0x000A8F51 File Offset: 0x000A7151
		private static void InitIfNecessary()
		{
			if (FileSelection.backgroundFrame == null)
			{
				FileSelection.Init();
			}
		}

		// Token: 0x0600112D RID: 4397 RVA: 0x000A8F60 File Offset: 0x000A7160
		public static void Init()
		{
			FileSelection.backgroundFrame = new GUIFrame(new RectTransform(GUI.Canvas.RelativeSize, GUI.Canvas, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
			{
				Color = Color.Black * 0.5f,
				HoverColor = Color.Black * 0.5f,
				SelectedColor = Color.Black * 0.5f,
				PressedColor = Color.Black * 0.5f
			};
			FileSelection.window = new GUIFrame(new RectTransform(Vector2.One * 0.8f, FileSelection.backgroundFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null);
			GUILayoutGroup horizontalLayout = new GUILayoutGroup(new RectTransform(Vector2.One * 0.9f, FileSelection.window.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			FileSelection.sidebar = new GUIListBox(new RectTransform(new Vector2(0.29f, 1f), horizontalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				PlaySoundOnSelect = true
			};
			DriveInfo[] drives = DriveInfo.GetDrives();
			DriveInfo[] array = drives;
			for (int i = 0; i < array.Length; i++)
			{
				DriveInfo drive = array[i];
				if (drive.DriveType != DriveType.Ram && !FileSelection.ignoredDrivePrefixes.Any((string p) => drive.Name.StartsWith(p)))
				{
					new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), FileSelection.sidebar.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), drive.Name.Replace('\\', '/'), null, null, Alignment.Left, false, "", null);
				}
			}
			FileSelection.sidebar.OnSelected = delegate(GUIComponent child, object userdata)
			{
				GUITextBlock guitextBlock = child as GUITextBlock;
				string text = (guitextBlock != null) ? guitextBlock.Text.SanitizedValue : null;
				if (text == null)
				{
					throw new Exception("Sidebar selection is invalid");
				}
				FileSelection.CurrentDirectory = text;
				return false;
			};
			new GUIFrame(new RectTransform(new Vector2(0.01f, 1f), horizontalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup fileListLayout = new GUILayoutGroup(new RectTransform(new Vector2(0.7f, 1f), horizontalLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			GUILayoutGroup firstRow = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.04f), fileListLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.CenterLeft);
			GUIButton guibutton = new GUIButton(new RectTransform(new Vector2(0.05f, 1f), firstRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "^", Alignment.Center, "", null);
			GUIButton.OnClickedHandler onClicked;
			if ((onClicked = FileSelection.<>O.<3>__MoveToParentDirectory) == null)
			{
				onClicked = (FileSelection.<>O.<3>__MoveToParentDirectory = new GUIButton.OnClickedHandler(FileSelection.MoveToParentDirectory));
			}
			guibutton.OnClicked = onClicked;
			GUITextBox guitextBox = new GUITextBox(new RectTransform(new Vector2(0.7f, 1f), firstRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			guitextBox.OverflowClip = true;
			guitextBox.OnEnterPressed = delegate(GUITextBox tb, string txt)
			{
				if (!Directory.Exists(txt))
				{
					tb.Text = FileSelection.CurrentDirectory;
					return false;
				}
				FileAttributes attributes = File.GetAttributes(txt);
				if (attributes.HasAnyFlag(FileAttributes.System) || attributes.HasAnyFlag(FileAttributes.Hidden))
				{
					tb.Text = FileSelection.CurrentDirectory;
					return false;
				}
				FileSelection.CurrentDirectory = txt;
				return true;
			};
			FileSelection.directoryBox = guitextBox;
			FileSelection.filterBox = new GUITextBox(new RectTransform(new Vector2(0.25f, 1f), firstRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true)
			{
				OverflowClip = true
			};
			firstRow.RectTransform.MinSize = new Point(0, firstRow.RectTransform.Children.Max((RectTransform c) => c.MinSize.Y));
			FileSelection.filterBox.OnTextChanged += delegate(GUITextBox txtbox, string txt)
			{
				FileSelection.RefreshFileList();
				return true;
			};
			new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), fileListLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIListBox guilistBox = new GUIListBox(new RectTransform(new Vector2(1f, 0.85f), fileListLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			guilistBox.PlaySoundOnSelect = true;
			guilistBox.OnSelected = delegate(GUIComponent child, object userdata)
			{
				if (userdata == null)
				{
					return false;
				}
				if (FileSelection.fileBox == null)
				{
					return false;
				}
				string fileName = (child as GUITextBlock).Text.SanitizedValue;
				FileSelection.fileBox.Text = fileName;
				if (PlayerInput.DoubleClicked())
				{
					bool isDir = userdata is FileSelection.ItemIsDirectory && (FileSelection.ItemIsDirectory)userdata == FileSelection.ItemIsDirectory.Yes;
					if (isDir)
					{
						FileSelection.CurrentDirectory += fileName;
					}
					else
					{
						Action<string> onFileSelected = FileSelection.OnFileSelected;
						if (onFileSelected != null)
						{
							onFileSelected(FileSelection.CurrentDirectory + fileName);
						}
						FileSelection.Open = false;
					}
				}
				return true;
			};
			FileSelection.fileList = guilistBox;
			new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), fileListLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup thirdRow = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.04f), fileListLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			GUITextBox guitextBox2 = new GUITextBox(new RectTransform(new Vector2(0.7f, 1f), thirdRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			guitextBox2.OnEnterPressed = delegate(GUITextBox tb, string txt)
			{
				GUIButton guibutton3 = FileSelection.openButton;
				bool? flag;
				if (guibutton3 == null)
				{
					flag = null;
				}
				else
				{
					GUIButton.OnClickedHandler onClicked2 = guibutton3.OnClicked;
					flag = ((onClicked2 != null) ? new bool?(onClicked2(FileSelection.openButton, null)) : null);
				}
				bool? flag2 = flag;
				return flag2.GetValueOrDefault();
			};
			FileSelection.fileBox = guitextBox2;
			GUIDropDown guidropDown = new GUIDropDown(new RectTransform(new Vector2(0.3f, 1f), thirdRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, 4, "", false, true, Alignment.CenterLeft, 1f);
			guidropDown.OnSelected = delegate(GUIComponent child, object userdata)
			{
				FileSelection.currentFileTypePattern = ((child as GUITextBlock).UserData as string);
				FileSelection.RefreshFileList();
				return true;
			};
			FileSelection.fileTypeDropdown = guidropDown;
			FileSelection.fileTypeDropdown.Select(4);
			new GUIFrame(new RectTransform(new Vector2(1f, 0.01f), fileListLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUILayoutGroup fourthRow = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.04f), fileListLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			new GUIFrame(new RectTransform(new Vector2(0.7f, 1f), fourthRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUIButton guibutton2 = new GUIButton(new RectTransform(new Vector2(0.15f, 1f), fourthRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("opensubbutton"), Alignment.Center, "", null);
			guibutton2.OnClicked = delegate(GUIButton btn, object obj)
			{
				if (Directory.Exists(Path.Combine(new string[]
				{
					FileSelection.CurrentDirectory,
					FileSelection.fileBox.Text
				})))
				{
					FileSelection.CurrentDirectory += FileSelection.fileBox.Text;
				}
				if (!File.Exists(FileSelection.CurrentDirectory + FileSelection.fileBox.Text))
				{
					return false;
				}
				Action<string> onFileSelected = FileSelection.OnFileSelected;
				if (onFileSelected != null)
				{
					onFileSelected(FileSelection.CurrentDirectory + FileSelection.fileBox.Text);
				}
				FileSelection.Open = false;
				return false;
			};
			FileSelection.openButton = guibutton2;
			new GUIButton(new RectTransform(new Vector2(0.15f, 1f), fourthRow.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("cancel"), Alignment.Center, "", null).OnClicked = delegate(GUIButton btn, object obj)
			{
				FileSelection.Open = false;
				return false;
			};
			FileSelection.CurrentDirectory = Directory.GetCurrentDirectory();
		}

		// Token: 0x0600112E RID: 4398 RVA: 0x000A9914 File Offset: 0x000A7B14
		public static void ClearFileTypeFilters()
		{
			FileSelection.InitIfNecessary();
			FileSelection.fileTypeDropdown.ClearChildren();
		}

		// Token: 0x0600112F RID: 4399 RVA: 0x000A9928 File Offset: 0x000A7B28
		[NullableContext(1)]
		public static void AddFileTypeFilter(string name, string pattern)
		{
			FileSelection.InitIfNecessary();
			FileSelection.fileTypeDropdown.AddItem(name + " (" + pattern + ")", pattern, null, null, null);
		}

		// Token: 0x06001130 RID: 4400 RVA: 0x000A996F File Offset: 0x000A7B6F
		[NullableContext(1)]
		public static void SelectFileTypeFilter(string pattern)
		{
			FileSelection.InitIfNecessary();
			FileSelection.fileTypeDropdown.SelectItem(pattern);
		}

		// Token: 0x06001131 RID: 4401 RVA: 0x000A9984 File Offset: 0x000A7B84
		public static void RefreshFileList()
		{
			FileSelection.InitIfNecessary();
			FileSelection.fileList.Content.ClearChildren();
			FileSelection.fileList.BarScroll = 0f;
			try
			{
				IEnumerable<string> directories = Directory.EnumerateDirectories(FileSelection.currentDirectory, "*" + FileSelection.filterBox.Text + "*");
				foreach (string directory in directories)
				{
					try
					{
						Directory.GetDirectories(directory);
					}
					catch (UnauthorizedAccessException)
					{
						continue;
					}
					string txt = directory;
					if (txt.StartsWith(FileSelection.currentDirectory))
					{
						txt = txt.Substring(FileSelection.currentDirectory.Length);
					}
					if (!txt.EndsWith("/"))
					{
						txt += "/";
					}
					GUITextBlock itemFrame = new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), FileSelection.fileList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), txt, null, null, Alignment.Left, false, "", null)
					{
						UserData = FileSelection.ItemIsDirectory.Yes
					};
					GUIImage folderIcon = new GUIImage(new RectTransform(new Point((int)((float)itemFrame.Rect.Height * 0.8f)), itemFrame.RectTransform, Anchor.CenterLeft, null, ScaleBasis.Normal, false)
					{
						AbsoluteOffset = new Point((int)((float)itemFrame.Rect.Height * 0.25f), 0)
					}, "OpenButton", true);
					itemFrame.Padding = new Vector4((float)folderIcon.Rect.Width * 1.5f, itemFrame.Padding.Y, itemFrame.Padding.Z, itemFrame.Padding.W);
				}
				IEnumerable<string> files = Enumerable.Empty<string>();
				if (FileSelection.currentFileTypePattern.IsNullOrEmpty())
				{
					files = Directory.GetFiles(FileSelection.currentDirectory);
				}
				else
				{
					foreach (string pattern in FileSelection.currentFileTypePattern.Split(',', StringSplitOptions.None))
					{
						string patternTrimmed = pattern.Trim();
						patternTrimmed = "*" + FileSelection.filterBox.Text + "*" + patternTrimmed;
						if (files.None(null))
						{
							files = Directory.EnumerateFiles(FileSelection.currentDirectory, patternTrimmed);
						}
						else
						{
							files = files.Concat(Directory.EnumerateFiles(FileSelection.currentDirectory, patternTrimmed));
						}
					}
				}
				foreach (string file in files)
				{
					string txt2 = file;
					if (txt2.StartsWith(FileSelection.currentDirectory))
					{
						txt2 = txt2.Substring(FileSelection.currentDirectory.Length);
					}
					new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), FileSelection.fileList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), txt2, null, null, Alignment.Left, false, "", null).UserData = FileSelection.ItemIsDirectory.No;
				}
			}
			catch (Exception e)
			{
				new GUITextBlock(new RectTransform(new Vector2(1f, 0.05f), FileSelection.fileList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "Could not list items in directory: " + e.Message, null, null, Alignment.Left, false, "", null).CanBeFocused = false;
			}
			RectTransform rectTransform = FileSelection.fileList.Content.RectTransform;
			Comparison<RectTransform> comparison;
			if ((comparison = FileSelection.<>O.<2>__SortFiles) == null)
			{
				comparison = (FileSelection.<>O.<2>__SortFiles = new Comparison<RectTransform>(FileSelection.SortFiles));
			}
			rectTransform.SortChildren(comparison);
			FileSelection.directoryBox.Text = FileSelection.currentDirectory;
			FileSelection.fileBox.Text = "";
			FileSelection.fileList.Deselect();
		}

		// Token: 0x06001132 RID: 4402 RVA: 0x000A9E18 File Offset: 0x000A8018
		[NullableContext(1)]
		public static bool MoveToParentDirectory(GUIButton button, object userdata)
		{
			string dir = FileSelection.CurrentDirectory;
			if (dir.EndsWith("/"))
			{
				dir = dir.Substring(0, dir.Length - 1);
			}
			int index = dir.LastIndexOf("/");
			if (index < 0)
			{
				return false;
			}
			FileSelection.CurrentDirectory = FileSelection.CurrentDirectory.Substring(0, index + 1);
			return true;
		}

		// Token: 0x06001133 RID: 4403 RVA: 0x000A9E6E File Offset: 0x000A806E
		public static void AddToGUIUpdateList()
		{
			if (!FileSelection.Open)
			{
				return;
			}
			GUIFrame guiframe = FileSelection.backgroundFrame;
			if (guiframe == null)
			{
				return;
			}
			guiframe.AddToGUIUpdateList(false, 0);
		}

		// Token: 0x0400088F RID: 2191
		private static bool open;

		// Token: 0x04000890 RID: 2192
		private static GUIFrame backgroundFrame;

		// Token: 0x04000891 RID: 2193
		private static GUIFrame window;

		// Token: 0x04000892 RID: 2194
		private static GUIListBox sidebar;

		// Token: 0x04000893 RID: 2195
		private static GUIListBox fileList;

		// Token: 0x04000894 RID: 2196
		private static GUITextBox directoryBox;

		// Token: 0x04000895 RID: 2197
		private static GUITextBox filterBox;

		// Token: 0x04000896 RID: 2198
		private static GUITextBox fileBox;

		// Token: 0x04000897 RID: 2199
		private static GUIDropDown fileTypeDropdown;

		// Token: 0x04000898 RID: 2200
		private static GUIButton openButton;

		// Token: 0x04000899 RID: 2201
		private static FileSystemWatcher fileSystemWatcher;

		// Token: 0x0400089A RID: 2202
		private static string currentFileTypePattern;

		// Token: 0x0400089B RID: 2203
		[Nullable(1)]
		private static readonly string[] ignoredDrivePrefixes = new string[]
		{
			"/sys/",
			"/snap/"
		};

		// Token: 0x0400089C RID: 2204
		[Nullable(1)]
		private static string currentDirectory = "";

		// Token: 0x02000915 RID: 2325
		[NullableContext(0)]
		private enum ItemIsDirectory
		{
			// Token: 0x04004042 RID: 16450
			Yes,
			// Token: 0x04004043 RID: 16451
			No
		}

		// Token: 0x02000916 RID: 2326
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004044 RID: 16452
			[Nullable(0)]
			public static FileSystemEventHandler <0>__OnFileSystemChanges;

			// Token: 0x04004045 RID: 16453
			[Nullable(0)]
			public static RenamedEventHandler <1>__OnFileSystemChanges;

			// Token: 0x04004046 RID: 16454
			[Nullable(0)]
			public static Comparison<RectTransform> <2>__SortFiles;

			// Token: 0x04004047 RID: 16455
			[Nullable(0)]
			public static GUIButton.OnClickedHandler <3>__MoveToParentDirectory;
		}
	}
}
