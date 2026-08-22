using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.Extensions;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;
using Steamworks.Ugc;

namespace Barotrauma.Steam
{
	// Token: 0x02000429 RID: 1065
	[NullableContext(1)]
	[Nullable(0)]
	public static class BulkDownloader
	{
		// Token: 0x06004760 RID: 18272 RVA: 0x002717A0 File Offset: 0x0026F9A0
		private static void CloseAllMessageBoxes()
		{
			GUIMessageBox.MessageBoxes.ForEachMod(delegate(GUIComponent b)
			{
				GUIMessageBox i = b as GUIMessageBox;
				if (i != null)
				{
					i.Close();
					return;
				}
				GUIMessageBox.MessageBoxes.Remove(b);
			});
		}

		// Token: 0x06004761 RID: 18273 RVA: 0x002717CC File Offset: 0x0026F9CC
		public static void PrepareUpdates()
		{
			BulkDownloader.CloseAllMessageBoxes();
			GUIMessageBox msgBox = new GUIMessageBox("", TextManager.Get("DeterminingRequiredModUpdates"), Array.Empty<LocalizedString>(), null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			TaskPool.Add("BulkDownloader.PrepareUpdates > GetItemsThatNeedUpdating", BulkDownloader.GetItemsThatNeedUpdating(), delegate(Task t)
			{
				msgBox.Close();
				IReadOnlyList<Item> items;
				if (!t.TryGetResult(out items))
				{
					return;
				}
				BulkDownloader.InitiateDownloads(items, null);
			});
		}

		// Token: 0x06004762 RID: 18274 RVA: 0x0027184C File Offset: 0x0026FA4C
		internal static void SubscribeToServerMods(IEnumerable<ulong> missingIds, ConnectCommand rejoinCommand)
		{
			BulkDownloader.CloseAllMessageBoxes();
			GUIMessageBox msgBox = new GUIMessageBox("", TextManager.Get("PreparingWorkshopDownloads"), Array.Empty<LocalizedString>(), null, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			string name = "BulkDownloader.SubscribeToServerMods > GetItems";
			Func<ulong, Task<Option<Item>>> selector;
			if ((selector = BulkDownloader.<>O.<0>__GetItem) == null)
			{
				selector = (BulkDownloader.<>O.<0>__GetItem = new Func<ulong, Task<Option<Item>>>(SteamManager.Workshop.GetItem));
			}
			Action <>9__2;
			TaskPool.Add(name, Task.WhenAll<Option<Item>>(missingIds.Select(selector)), delegate(Task t)
			{
				msgBox.Close();
				Option<Item>[] itemOptions;
				if (!t.TryGetResult(out itemOptions))
				{
					return;
				}
				List<Item> itemsToDownload = new List<Item>();
				foreach (Option<Item> itemOption in itemOptions)
				{
					Item item;
					if (itemOption.TryUnwrap(out item))
					{
						itemsToDownload.Add(item);
					}
				}
				itemsToDownload.ForEach(delegate(Item it)
				{
					it.Subscribe();
				});
				IReadOnlyList<Item> itemsToDownload2 = itemsToDownload;
				Action onComplete;
				if ((onComplete = <>9__2) == null)
				{
					onComplete = (<>9__2 = delegate()
					{
						ContentPackageManager.UpdateContentPackageList();
						GameMain.Instance.ConnectCommand = Option<ConnectCommand>.Some(rejoinCommand);
					});
				}
				BulkDownloader.InitiateDownloads(itemsToDownload2, onComplete);
			});
		}

		// Token: 0x06004763 RID: 18275 RVA: 0x002718F4 File Offset: 0x0026FAF4
		public static Task<IReadOnlyList<Item>> GetItemsThatNeedUpdating()
		{
			BulkDownloader.<GetItemsThatNeedUpdating>d__3 <GetItemsThatNeedUpdating>d__;
			<GetItemsThatNeedUpdating>d__.<>t__builder = AsyncTaskMethodBuilder<IReadOnlyList<Item>>.Create();
			<GetItemsThatNeedUpdating>d__.<>1__state = -1;
			<GetItemsThatNeedUpdating>d__.<>t__builder.Start<BulkDownloader.<GetItemsThatNeedUpdating>d__3>(ref <GetItemsThatNeedUpdating>d__);
			return <GetItemsThatNeedUpdating>d__.<>t__builder.Task;
		}

		// Token: 0x06004764 RID: 18276 RVA: 0x00271930 File Offset: 0x0026FB30
		public static void InitiateDownloads(IReadOnlyList<Item> itemsToDownload, [Nullable(2)] Action onComplete = null)
		{
			BulkDownloader.<>c__DisplayClass4_0 CS$<>8__locals1 = new BulkDownloader.<>c__DisplayClass4_0();
			CS$<>8__locals1.onComplete = onComplete;
			BulkDownloader.<>c__DisplayClass4_0 CS$<>8__locals2 = CS$<>8__locals1;
			RichString headerText = TextManager.Get("WorkshopItemDownloading");
			RichString text2 = "";
			Vector2? relativeSize = new Vector2?(new ValueTuple<float, float>(0.5f, 0.6f));
			CS$<>8__locals2.msgBox = new GUIMessageBox(headerText, text2, new LocalizedString[]
			{
				TextManager.Get("Cancel")
			}, relativeSize, null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			CS$<>8__locals1.msgBox.Buttons[0].OnClicked = new GUIButton.OnClickedHandler(CS$<>8__locals1.msgBox.Close);
			GUIListBox modsList = new GUIListBox(new RectTransform(new ValueTuple<float, float>(1f, 0.8f), CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false)
			{
				HoverCursor = CursorState.Default
			};
			using (IEnumerator<Item> enumerator = itemsToDownload.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Item item = enumerator.Current;
					GUIFrame itemFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.08f), modsList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null)
					{
						CanBeFocused = false
					};
					GUITextBlock itemTitle = new GUITextBlock(new RectTransform(Vector2.One, itemFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), item.Title ?? "", null, null, Alignment.Left, false, "", null);
					GUIProgressBar itemDownloadProgress = new GUIProgressBar(new RectTransform(new ValueTuple<float, float>(0.5f, 0.75f), itemFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), 0f, null, "", true)
					{
						Color = GUIStyle.Green
					};
					GUITextBlock textShadow = new GUITextBlock(new RectTransform(Vector2.One, itemDownloadProgress.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
					{
						AbsoluteOffset = new Point(GUI.IntScale(3f))
					}, "", new Color?(Color.Black), null, Alignment.Center, false, "", null);
					GUITextBlock text = new GUITextBlock(new RectTransform(Vector2.One, itemDownloadProgress.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, Alignment.Center, false, "", null);
					GUICustomComponent itemDownloadProgressUpdater = new GUICustomComponent(new RectTransform(Vector2.Zero, CS$<>8__locals1.msgBox.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float f, GUICustomComponent component)
					{
						float progress = 0f;
						if (item.IsDownloading)
						{
							progress = item.DownloadAmount;
							text.Text = (textShadow.Text = TextManager.GetWithVariable("PublishPopupDownload", "[percentage]", ((int)MathF.Round(item.DownloadAmount * 100f)).ToString(), FormatCapitals.No));
						}
						else if (itemDownloadProgress.BarSize > 0f)
						{
							if (!item.IsInstalled && !SteamManager.Workshop.CanBeInstalled(item.Id))
							{
								itemDownloadProgress.Color = GUIStyle.Red;
								text.Text = (textShadow.Text = TextManager.Get("workshopiteminstallfailed"));
							}
							else
							{
								text.Text = (textShadow.Text = TextManager.Get(item.IsInstalled ? "workshopiteminstalled" : "PublishPopupInstall"));
							}
							progress = 1f;
						}
						itemDownloadProgress.BarSize = Math.Max(itemDownloadProgress.BarSize, MathHelper.Lerp(itemDownloadProgress.BarSize, progress, 0.1f));
					});
				}
			}
			TaskPool.Add("DownloadItems", BulkDownloader.DownloadItems(itemsToDownload, CS$<>8__locals1.msgBox), delegate(Task _)
			{
				if (GUIMessageBox.MessageBoxes.Contains(CS$<>8__locals1.msgBox))
				{
					Action onComplete2 = CS$<>8__locals1.onComplete;
					if (onComplete2 != null)
					{
						onComplete2();
					}
				}
				CS$<>8__locals1.msgBox.Close();
				ContentPackageManager.WorkshopPackages.Refresh();
				ContentPackageManager.EnabledPackages.RefreshUpdatedMods();
				SettingsMenu instance = SettingsMenu.Instance;
				MutableWorkshopMenu mutableWorkshopMenu = ((instance != null) ? instance.WorkshopMenu : null) as MutableWorkshopMenu;
				if (mutableWorkshopMenu != null)
				{
					mutableWorkshopMenu.PopulateInstalledModLists(true, true);
				}
				GameMain.MainMenuScreen.ResetModUpdateButton();
			});
		}

		// Token: 0x06004765 RID: 18277 RVA: 0x00271D28 File Offset: 0x0026FF28
		private static Task DownloadItems(IReadOnlyList<Item> itemsToDownload, GUIMessageBox msgBox)
		{
			BulkDownloader.<DownloadItems>d__5 <DownloadItems>d__;
			<DownloadItems>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<DownloadItems>d__.itemsToDownload = itemsToDownload;
			<DownloadItems>d__.msgBox = msgBox;
			<DownloadItems>d__.<>1__state = -1;
			<DownloadItems>d__.<>t__builder.Start<BulkDownloader.<DownloadItems>d__5>(ref <DownloadItems>d__);
			return <DownloadItems>d__.<>t__builder.Task;
		}

		// Token: 0x020010EB RID: 4331
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x040059FC RID: 23036
			[Nullable(0)]
			public static Func<ulong, Task<Option<Item>>> <0>__GetItem;
		}
	}
}
