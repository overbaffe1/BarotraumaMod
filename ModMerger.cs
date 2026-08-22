using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.IO;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000041 RID: 65
	[NullableContext(1)]
	[Nullable(0)]
	public static class ModMerger
	{
		// Token: 0x060009A2 RID: 2466 RVA: 0x00057350 File Offset: 0x00055550
		public static void AskMerge(ContentPackage[] mods)
		{
			ModMerger.<>c__DisplayClass0_0 CS$<>8__locals1 = new ModMerger.<>c__DisplayClass0_0();
			CS$<>8__locals1.mods = mods;
			ModMerger.ErrorIfNonLocal(CS$<>8__locals1.mods);
			ModMerger.<>c__DisplayClass0_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString headerText = TextManager.Get("MergeModsHeader");
			RichString text = "";
			Vector2? relativeSize = new Vector2?(new ValueTuple<float, float>(0.5f, 0.8f));
			CS$<>8__locals2.msgBox = new GUIMessageBox(headerText, text, new LocalizedString[]
			{
				TextManager.Get("ConfirmModMerge"),
				TextManager.Get("Cancel")
			}, relativeSize, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			CS$<>8__locals1.msgBox.Buttons[1].OnClicked = new GUIButton.OnClickedHandler(CS$<>8__locals1.msgBox.Close);
			GUITextBlock desc = new GUITextBlock(new RectTransform(new ValueTuple<float, float>(1f, 0.1f), CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("MergeModsDesc"), null, null, Alignment.Left, false, "", null);
			GUIListBox guilistBox = new GUIListBox(new RectTransform(new ValueTuple<float, float>(1f, 0.5f), CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			guilistBox.OnSelected = ((GUIComponent component, object o) => false);
			guilistBox.HoverCursor = CursorState.Default;
			GUIListBox modsList = guilistBox;
			foreach (ContentPackage mod in CS$<>8__locals1.mods)
			{
				new GUITextBlock(new RectTransform(new ValueTuple<float, float>(1f, 0.11f), modsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), mod.Name, null, null, Alignment.Left, false, "", null).CanBeFocused = false;
			}
			GUITextBlock footer = new GUITextBlock(new RectTransform(new ValueTuple<float, float>(1f, 0.1f), CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("MergeModsFooter"), null, null, Alignment.Left, false, "", null);
			ModMerger.<>c__DisplayClass0_0 CS$<>8__locals3 = CS$<>8__locals1;
			GUITextBox guitextBox = new GUITextBox(new RectTransform(new ValueTuple<float, float>(1f, 0.1f), CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Left, false, "", null, false, true);
			string text2;
			if (CS$<>8__locals1.mods.Count((ContentPackage m) => m.Files.Length > 1) != 1)
			{
				text2 = "";
			}
			else
			{
				text2 = CS$<>8__locals1.mods.First((ContentPackage m) => m.Files.Length > 1).Name;
			}
			guitextBox.Text = text2;
			CS$<>8__locals3.resultName = guitextBox;
			CS$<>8__locals1.msgBox.Buttons[0].OnClicked = delegate(GUIButton button, object o)
			{
				ModMerger.<>c__DisplayClass0_1 CS$<>8__locals4 = new ModMerger.<>c__DisplayClass0_1();
				if (string.IsNullOrEmpty(CS$<>8__locals1.resultName.Text))
				{
					base.<AskMerge>g__flashText|0();
					return false;
				}
				CS$<>8__locals4.targetDir = "LocalMods/" + CS$<>8__locals1.resultName.Text;
				if (ContentPackageManager.LocalPackages.Any(new Func<ContentPackage, bool>(CS$<>8__locals4.<AskMerge>g__dirMatches|5)) && !CS$<>8__locals1.mods.Any(new Func<ContentPackage, bool>(CS$<>8__locals4.<AskMerge>g__dirMatches|5)))
				{
					base.<AskMerge>g__flashText|0();
					return false;
				}
				ModMerger.MergeMods(CS$<>8__locals1.mods, CS$<>8__locals1.resultName.Text);
				CS$<>8__locals1.msgBox.Close();
				return false;
			};
		}

		// Token: 0x060009A3 RID: 2467 RVA: 0x0005773C File Offset: 0x0005593C
		private static void MergeMods(ContentPackage[] mods, string resultName)
		{
			ModProject resultProject = new ModProject
			{
				Name = resultName
			};
			string targetDir = "LocalMods/" + resultName;
			Directory.CreateDirectory(targetDir, false);
			foreach (ContentPackage mod in mods)
			{
				using (IEnumerator<string> enumerator = (from f in Directory.GetFiles(mod.Dir, "*", SearchOption.AllDirectories)
				select f.CleanUpPathCrossPlatform(false, "")).GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						string file = enumerator.Current;
						if (!Path.GetFileName(file).Equals("filelist.xml", StringComparison.OrdinalIgnoreCase))
						{
							string targetFilePath = file.Substring(mod.Dir.Length);
							if (targetFilePath.StartsWith("/") || targetFilePath.StartsWith("\\"))
							{
								targetFilePath = targetFilePath.Substring(1);
							}
							targetFilePath = Path.Combine(new string[]
							{
								targetDir,
								targetFilePath
							}).CleanUpPathCrossPlatform(false, "");
							Directory.CreateDirectory(Path.GetDirectoryName(targetFilePath), false);
							File.Copy(file, targetFilePath, true, true);
							ModProject.File oldFileInProject = resultProject.Files.FirstOrDefault((ModProject.File f) => f.Path.Equals(targetFilePath, StringComparison.OrdinalIgnoreCase));
							if (oldFileInProject != null)
							{
								resultProject.RemoveFile(oldFileInProject);
							}
							ContentFile fileInMod = mod.Files.Find((ContentFile f) => f.Path == file);
							if (fileInMod != null)
							{
								ModProject.File newFileInProject = ModProject.File.FromPath(targetFilePath, fileInMod.GetType());
								resultProject.AddFile(newFileInProject);
							}
						}
					}
				}
			}
			resultProject.Save(Path.Combine(new string[]
			{
				targetDir,
				"filelist.xml"
			}), true);
			foreach (ContentPackage mod2 in mods)
			{
				Directory.Delete(mod2.Dir, true, true);
			}
			(SettingsMenu.Instance.WorkshopMenu as MutableWorkshopMenu).PopulateInstalledModLists(true, true);
		}

		// Token: 0x060009A4 RID: 2468 RVA: 0x000579A8 File Offset: 0x00055BA8
		private static void ErrorIfNonLocal(ContentPackage[] mods)
		{
			ContentPackage[] nonLocal = (from m in mods
			where !ContentPackageManager.LocalPackages.Contains(m)
			select m).ToArray<ContentPackage>();
			if (nonLocal.Any<ContentPackage>())
			{
				throw new Exception(string.Join(", ", from m in nonLocal
				select m.Name) + " are not local mods");
			}
		}
	}
}
