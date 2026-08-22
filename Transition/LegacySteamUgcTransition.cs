using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Barotrauma.IO;
using Microsoft.Xna.Framework;
using Steamworks.Ugc;

namespace Barotrauma.Transition
{
	// Token: 0x0200062E RID: 1582
	[NullableContext(1)]
	[Nullable(0)]
	public static class LegacySteamUgcTransition
	{
		// Token: 0x060064F4 RID: 25844 RVA: 0x003434FC File Offset: 0x003416FC
		public static void Prepare()
		{
			TaskPool.Add("UgcTransition.Prepare", LegacySteamUgcTransition.DetermineItemsToTransition(), delegate(Task t)
			{
				LegacySteamUgcTransition.<>c__DisplayClass2_0 CS$<>8__locals1 = new LegacySteamUgcTransition.<>c__DisplayClass2_0();
				ValueTuple<LegacySteamUgcTransition.OldSubs, LegacySteamUgcTransition.OldItemAssemblies, LegacySteamUgcTransition.OldMods> result;
				if (!t.TryGetResult(out result))
				{
					return;
				}
				ValueTuple<LegacySteamUgcTransition.OldSubs, LegacySteamUgcTransition.OldItemAssemblies, LegacySteamUgcTransition.OldMods> valueTuple = result;
				LegacySteamUgcTransition.OldSubs subs = valueTuple.Item1;
				LegacySteamUgcTransition.OldItemAssemblies itemAssemblies = valueTuple.Item2;
				LegacySteamUgcTransition.OldMods mods = valueTuple.Item3;
				if (!subs.FilePaths.Any<string>() && !itemAssemblies.FilePaths.Any<string>() && !mods.Mods.Any<ValueTuple<string, string, Item?, DateTime>>())
				{
					return;
				}
				LegacySteamUgcTransition.<>c__DisplayClass2_0 CS$<>8__locals2 = CS$<>8__locals1;
				RichString headerText = TextManager.Get("Ugc.TransferTitle");
				RichString text = "";
				Vector2? relativeSize = new Vector2?(new ValueTuple<float, float>(0.5f, 0.8f));
				CS$<>8__locals2.msgBox = new GUIMessageBox(headerText, text, new LocalizedString[]
				{
					TextManager.Get("Ugc.TransferButton")
				}, relativeSize, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
				new GUIButton(new RectTransform(Vector2.One * 1.5f, CS$<>8__locals1.msgBox.Header.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.BothHeight), Alignment.Center, "GUICancelButton", null).OnClicked = delegate(GUIButton button, object o)
				{
					CS$<>8__locals1.msgBox.Close();
					return false;
				};
				GUITextBlock desc = new GUITextBlock(new RectTransform(new ValueTuple<float, float>(1f, 0.24f), CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Ugc.TransferDesc"), null, null, Alignment.CenterLeft, true, "", null);
				CS$<>8__locals1.modsList = new GUIListBox(new RectTransform(new ValueTuple<float, float>(1f, 0.6f), CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
				{
					HoverCursor = CursorState.Default
				};
				CS$<>8__locals1.pathTickboxMap = new Dictionary<string, GUITickBox>();
				CS$<>8__locals1.firstHeader = true;
				if (subs.FilePaths.Any<string>())
				{
					CS$<>8__locals1.<Prepare>g__addSpacer|3();
					CS$<>8__locals1.<Prepare>g__addHeader|1(TextManager.Get("WorkshopLabelSubmarines"));
					foreach (string sub in subs.FilePaths)
					{
						string subName = Path.GetFileNameWithoutExtension(sub);
						CS$<>8__locals1.<Prepare>g__addTickbox|2(sub, subName, !ContentPackageManager.LocalPackages.Any((ContentPackage p) => p.NameMatches(subName)));
					}
				}
				if (itemAssemblies.FilePaths.Any<string>())
				{
					CS$<>8__locals1.<Prepare>g__addSpacer|3();
					CS$<>8__locals1.<Prepare>g__addHeader|1(TextManager.Get("ItemAssemblies"));
					foreach (string itemAssembly in itemAssemblies.FilePaths)
					{
						string assemblyName = Path.GetFileNameWithoutExtension(itemAssembly);
						CS$<>8__locals1.<Prepare>g__addTickbox|2(itemAssembly, assemblyName, !ContentPackageManager.LocalPackages.Any((ContentPackage p) => p.NameMatches(assemblyName)));
					}
				}
				if (mods.Mods.Any<ValueTuple<string, string, Item?, DateTime>>())
				{
					CS$<>8__locals1.<Prepare>g__addSpacer|3();
					CS$<>8__locals1.<Prepare>g__addHeader|1(TextManager.Get("SubscribedMods"));
					foreach (ValueTuple<string, string, Item?, DateTime> mod in mods.Mods)
					{
						LegacySteamUgcTransition.<>c__DisplayClass2_0 CS$<>8__locals6 = CS$<>8__locals1;
						string item4 = mod.Item1;
						string item2 = mod.Item2;
						Item? item3 = mod.Item3;
						bool ticked;
						if (item3 != null)
						{
							Item item = item3.GetValueOrDefault();
							ticked = !ContentPackageManager.LocalPackages.Any(delegate(ContentPackage p)
							{
								ContentPackageId ugcId;
								if (p.UgcId.TryUnwrap(out ugcId))
								{
									SteamWorkshopId workshopId = ugcId as SteamWorkshopId;
									if (workshopId != null)
									{
										return workshopId.Value == item.Id;
									}
								}
								return false;
							});
						}
						else
						{
							ticked = true;
						}
						CS$<>8__locals6.<Prepare>g__addTickbox|2(item4, item2, ticked);
					}
				}
				CS$<>8__locals1.subMsgBox = null;
				CS$<>8__locals1.msgBox.Buttons[0].OnClicked = delegate(GUIButton b, object o)
				{
					string name = "TransferMods";
					Task task = LegacySteamUgcTransition.TransferMods(CS$<>8__locals1.pathTickboxMap);
					Action<Task> onCompletion;
					if ((onCompletion = CS$<>8__locals1.<>9__12) == null)
					{
						onCompletion = (CS$<>8__locals1.<>9__12 = delegate(Task t2)
						{
							if (t2.Exception != null)
							{
								DebugConsole.ThrowError("There was an error transferring mods", t2.Exception.GetInnermost(), null, false, false);
							}
							ContentPackageManager.LocalPackages.Refresh();
							string[] modsToEnable;
							if (t2.TryGetResult(out modsToEnable))
							{
								List<RegularPackage> newRegular = ContentPackageManager.EnabledPackages.Regular.ToList<RegularPackage>();
								newRegular.AddRange(from r in ContentPackageManager.LocalPackages.Regular
								where modsToEnable.Contains(r.Dir.CleanUpPathCrossPlatform(false, ""))
								select r);
								newRegular = newRegular.Distinct<RegularPackage>().ToList<RegularPackage>();
								ContentPackageManager.EnabledPackages.SetRegular(newRegular);
							}
							base.<Prepare>g__createSubMsgBox|4(TextManager.Get("Ugc.TransferComplete"), true);
						});
					}
					TaskPool.Add(name, task, onCompletion);
					CS$<>8__locals1.msgBox.Close();
					base.<Prepare>g__createSubMsgBox|4(TextManager.Get("Ugc.Transferring"), false);
					return false;
				};
			});
		}

		// Token: 0x060064F5 RID: 25845 RVA: 0x00343530 File Offset: 0x00341730
		[return: TupleElementNames(new string[]
		{
			"Subs",
			"ItemAssemblies",
			"Mods"
		})]
		[return: Nullable(new byte[]
		{
			1,
			0
		})]
		private static Task<ValueTuple<LegacySteamUgcTransition.OldSubs, LegacySteamUgcTransition.OldItemAssemblies, LegacySteamUgcTransition.OldMods>> DetermineItemsToTransition()
		{
			LegacySteamUgcTransition.<DetermineItemsToTransition>d__9 <DetermineItemsToTransition>d__;
			<DetermineItemsToTransition>d__.<>t__builder = AsyncTaskMethodBuilder<ValueTuple<LegacySteamUgcTransition.OldSubs, LegacySteamUgcTransition.OldItemAssemblies, LegacySteamUgcTransition.OldMods>>.Create();
			<DetermineItemsToTransition>d__.<>1__state = -1;
			<DetermineItemsToTransition>d__.<>t__builder.Start<LegacySteamUgcTransition.<DetermineItemsToTransition>d__9>(ref <DetermineItemsToTransition>d__);
			return <DetermineItemsToTransition>d__.<>t__builder.Task;
		}

		// Token: 0x060064F6 RID: 25846 RVA: 0x0034356B File Offset: 0x0034176B
		private static bool FolderShouldBeTransitioned(string folderName)
		{
			return Directory.Exists(folderName) && !File.Exists(Path.Combine(new string[]
			{
				folderName,
				"LOCALMODS_README.txt"
			}));
		}

		// Token: 0x060064F7 RID: 25847 RVA: 0x00343598 File Offset: 0x00341798
		private static Task<string[]> TransferMods(Dictionary<string, GUITickBox> pathTickboxMap)
		{
			LegacySteamUgcTransition.<TransferMods>d__11 <TransferMods>d__;
			<TransferMods>d__.<>t__builder = AsyncTaskMethodBuilder<string[]>.Create();
			<TransferMods>d__.pathTickboxMap = pathTickboxMap;
			<TransferMods>d__.<>1__state = -1;
			<TransferMods>d__.<>t__builder.Start<LegacySteamUgcTransition.<TransferMods>d__11>(ref <TransferMods>d__);
			return <TransferMods>d__.<>t__builder.Task;
		}

		// Token: 0x060064F8 RID: 25848 RVA: 0x003435DB File Offset: 0x003417DB
		[return: Nullable(new byte[]
		{
			1,
			2
		})]
		private static Task<string> TransferMod([Nullable(new byte[]
		{
			0,
			1,
			1
		})] KeyValuePair<string, GUITickBox> kvp)
		{
			return LegacySteamUgcTransition.TransferMod(kvp.Key, kvp.Value);
		}

		// Token: 0x060064F9 RID: 25849 RVA: 0x003435F0 File Offset: 0x003417F0
		[return: Nullable(new byte[]
		{
			1,
			2
		})]
		private static Task<string> TransferMod(string path, GUITickBox tickbox)
		{
			LegacySteamUgcTransition.<TransferMod>d__13 <TransferMod>d__;
			<TransferMod>d__.<>t__builder = AsyncTaskMethodBuilder<string>.Create();
			<TransferMod>d__.path = path;
			<TransferMod>d__.tickbox = tickbox;
			<TransferMod>d__.<>1__state = -1;
			<TransferMod>d__.<>t__builder.Start<LegacySteamUgcTransition.<TransferMod>d__13>(ref <TransferMod>d__);
			return <TransferMod>d__.<>t__builder.Task;
		}

		// Token: 0x060064FA RID: 25850 RVA: 0x0034363B File Offset: 0x0034183B
		private static void WriteReadme(string folderName)
		{
			if (!Directory.Exists(folderName))
			{
				return;
			}
			File.WriteAllText(Path.Combine(new string[]
			{
				folderName,
				"LOCALMODS_README.txt"
			}), "This folder is no longer used by Barotrauma;\nyour mods and submarines should have been transferred\nto LocalMods. If they are not being found, delete this\nreadme and relaunch the game.", Encoding.UTF8, true);
		}

		// Token: 0x060064FB RID: 25851 RVA: 0x0034366D File Offset: 0x0034186D
		[CompilerGenerated]
		internal static string[] <DetermineItemsToTransition>g__getFiles|9_0(string path, string pattern)
		{
			if (!Directory.Exists(path))
			{
				return Array.Empty<string>();
			}
			return Directory.GetFiles(path, pattern, SearchOption.TopDirectoryOnly);
		}

		// Token: 0x04003462 RID: 13410
		private const string readmeName = "LOCALMODS_README.txt";

		// Token: 0x04003463 RID: 13411
		private const string oldSubsPath = "Submarines";

		// Token: 0x04003464 RID: 13412
		private const string oldModsPath = "Mods";

		// Token: 0x04003465 RID: 13413
		private const string oldItemAssembliesPath = "ItemAssemblies";

		// Token: 0x020014C8 RID: 5320
		[NullableContext(0)]
		private enum ModsListChildType
		{
			// Token: 0x040066FF RID: 26367
			Header,
			// Token: 0x04006700 RID: 26368
			Entry
		}

		// Token: 0x020014C9 RID: 5321
		[Nullable(0)]
		private struct OldSubs
		{
			// Token: 0x06009C05 RID: 39941 RVA: 0x003E82C0 File Offset: 0x003E64C0
			public OldSubs(IReadOnlyList<string> filePaths)
			{
				this.FilePaths = filePaths;
			}

			// Token: 0x04006701 RID: 26369
			public readonly IReadOnlyList<string> FilePaths;
		}

		// Token: 0x020014CA RID: 5322
		[Nullable(0)]
		private struct OldItemAssemblies
		{
			// Token: 0x06009C06 RID: 39942 RVA: 0x003E82C9 File Offset: 0x003E64C9
			public OldItemAssemblies(IReadOnlyList<string> filePaths)
			{
				this.FilePaths = filePaths;
			}

			// Token: 0x04006702 RID: 26370
			public readonly IReadOnlyList<string> FilePaths;
		}

		// Token: 0x020014CB RID: 5323
		[NullableContext(0)]
		private struct OldMods
		{
			// Token: 0x06009C07 RID: 39943 RVA: 0x003E82D2 File Offset: 0x003E64D2
			public OldMods([TupleElementNames(new string[]
			{
				"Dir",
				"Name",
				"Item",
				"InstallTime"
			})] [Nullable(new byte[]
			{
				1,
				0,
				1,
				1
			})] IReadOnlyList<ValueTuple<string, string, Item?, DateTime>> mods)
			{
				this.Mods = mods;
			}

			// Token: 0x04006703 RID: 26371
			[TupleElementNames(new string[]
			{
				"Dir",
				"Name",
				"Item",
				"InstallTime"
			})]
			[Nullable(new byte[]
			{
				1,
				0,
				1,
				1
			})]
			public readonly IReadOnlyList<ValueTuple<string, string, Item?, DateTime>> Mods;
		}

		// Token: 0x020014CC RID: 5324
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04006704 RID: 26372
			[Nullable(0)]
			public static Func<KeyValuePair<string, GUITickBox>, Task<string>> <0>__TransferMod;
		}
	}
}
