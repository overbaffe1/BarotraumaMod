using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Barotrauma.Extensions;
using Barotrauma.IO;
using Barotrauma.Networking;
using Barotrauma.Steam;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Barotrauma
{
	// Token: 0x02000116 RID: 278
	[NullableContext(1)]
	[Nullable(0)]
	internal class ModDownloadScreen : Screen
	{
		// Token: 0x17000A03 RID: 2563
		// (get) Token: 0x06002596 RID: 9622 RVA: 0x00185E58 File Offset: 0x00184058
		public IEnumerable<ContentPackage> DownloadedPackages
		{
			get
			{
				return this.downloadedPackages;
			}
		}

		// Token: 0x06002597 RID: 9623 RVA: 0x00185E60 File Offset: 0x00184060
		public void Reset()
		{
			this.pendingDownloads.Clear();
			this.downloadedPackages.Clear();
			this.currentDownload = null;
			this.confirmDownload = false;
		}

		// Token: 0x06002598 RID: 9624 RVA: 0x00185E86 File Offset: 0x00184086
		private void DeletePrevDownloads()
		{
			if (Directory.Exists("TempMods_Download"))
			{
				Directory.Delete("TempMods_Download", true, true);
			}
		}

		// Token: 0x06002599 RID: 9625 RVA: 0x00185EA0 File Offset: 0x001840A0
		[DoesNotReturn]
		private static void LogAndThrowException(string errorMsg, string analyticsId)
		{
			GameAnalyticsManager.AddErrorEventOnce(analyticsId, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
			throw new InvalidOperationException(errorMsg);
		}

		// Token: 0x0600259A RID: 9626 RVA: 0x00185EB0 File Offset: 0x001840B0
		public override void Select()
		{
			ModDownloadScreen.<>c__DisplayClass9_0 CS$<>8__locals1 = new ModDownloadScreen.<>c__DisplayClass9_0();
			CS$<>8__locals1.<>4__this = this;
			base.Select();
			this.DeletePrevDownloads();
			this.Reset();
			ClientPeer clientPeer = GameMain.Client.ClientPeer;
			bool allowDownloads = clientPeer != null && clientPeer.AllowModDownloads;
			this.Frame.ClearChildren();
			GUIFrame mainVisibleFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(0.6f, 0.8f), this.Frame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), "", null);
			CS$<>8__locals1.mainLayout = new GUILayoutGroup(new RectTransform(Vector2.One * 0.93f, mainVisibleFrame.RectTransform, Anchor.Center, null, null, null, ScaleBasis.Normal), false, Anchor.TopLeft);
			RectTransform rectT = new RectTransform(new ValueTuple<float, float>(1f, 0.08f), CS$<>8__locals1.mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal);
			RichString text = "";
			GUIFont largeFont = GUIStyle.LargeFont;
			new GUITextBlock(rectT, text, null, largeFont, Alignment.CenterLeft, false, "", null).TextGetter = (() => GameMain.Client.ServerName);
			CS$<>8__locals1.<Select>g__mainLayoutSpacing|0();
			GUIListBox downloadList = new GUIListBox(new RectTransform(new ValueTuple<float, float>(1f, 0.76f), CS$<>8__locals1.mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			CS$<>8__locals1.<Select>g__mainLayoutSpacing|0();
			new GUIButton(new RectTransform(new ValueTuple<float, float>(0.3f, 0.1f), CS$<>8__locals1.mainLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), TextManager.Get("Disconnect"), Alignment.Center, "", null).OnClicked = delegate(GUIButton guiButton, object o)
			{
				GameClient client = GameMain.Client;
				if (client != null)
				{
					client.Quit();
				}
				GameMain.MainMenuScreen.Select();
				return false;
			};
			if (!GameMain.Client.IsServerOwner && GameMain.Client.ClientPeer.ServerContentPackages.Length == 0)
			{
				string str = "Error in ModDownloadScreen: the list of mods the server has enabled was empty. ";
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(31, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Content package list received: ");
				defaultInterpolatedStringHandler.AppendFormatted<bool>(GameMain.Client.ClientPeer.ContentPackageOrderReceived);
				ModDownloadScreen.LogAndThrowException(str + defaultInterpolatedStringHandler.ToStringAndClear(), "ModDownloadScreen.Select:NoContentPackages");
			}
			ServerContentPackage[] missingPackages = (from sp in GameMain.Client.ClientPeer.ServerContentPackages
			where sp.ContentPackage == null
			select sp).ToArray<ServerContentPackage>();
			if (!missingPackages.Any((ServerContentPackage p) => p.IsMandatory))
			{
				if (!GameMain.Client.IsServerOwner)
				{
					CorePackage corePackage = (from p in GameMain.Client.ClientPeer.ServerContentPackages
					select p.CorePackage).OfType<CorePackage>().FirstOrDefault<CorePackage>();
					if (corePackage == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(120, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Error in ModDownloadScreen: no core packages in the list of mods the server has enabled. ");
						defaultInterpolatedStringHandler2.AppendLiteral("Content package list received: ");
						defaultInterpolatedStringHandler2.AppendFormatted<bool>(GameMain.Client.ClientPeer.ContentPackageOrderReceived);
						ModDownloadScreen.LogAndThrowException(defaultInterpolatedStringHandler2.ToStringAndClear(), "ModDownloadScreen.Select:NoCorePackage");
					}
					ContentPackageManager.EnabledPackages.BackUp();
					ContentPackageManager.EnabledPackages.SetCore(corePackage);
					List<RegularPackage> regularPackages = (from p in GameMain.Client.ClientPeer.ServerContentPackages
					select p.RegularPackage).OfType<RegularPackage>().ToList<RegularPackage>();
					regularPackages.AddRange(from p in ContentPackageManager.EnabledPackages.Regular
					where !p.HasMultiplayerSyncedContent && !regularPackages.Contains(p)
					select p);
					ContentPackageManager.EnabledPackages.SetRegular(regularPackages);
				}
				GameMain.NetLobbyScreen.Select();
				return;
			}
			ServerContentPackage mismatchedVanilla = missingPackages.FirstOrDefault((ServerContentPackage p) => p.IsVanilla);
			if (mismatchedVanilla != null)
			{
				string[] array = new string[6];
				array[0] = "Error in ModDownloadScreen: mismatched Vanilla package: local hash is ";
				int num = 1;
				CorePackage vanillaCorePackage = ContentPackageManager.VanillaCorePackage;
				array[num] = (((vanillaCorePackage != null) ? vanillaCorePackage.Hash.StringRepresentation : null) ?? "[NULL]");
				array[2] = ", remote hash is ";
				array[3] = mismatchedVanilla.Hash.StringRepresentation;
				array[4] = ". ";
				int num2 = 5;
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(31, 1);
				defaultInterpolatedStringHandler3.AppendLiteral("Content package list received: ");
				defaultInterpolatedStringHandler3.AppendFormatted<bool>(GameMain.Client.ClientPeer.ContentPackageOrderReceived);
				array[num2] = defaultInterpolatedStringHandler3.ToStringAndClear();
				ModDownloadScreen.LogAndThrowException(string.Concat(array), "ModDownloadScreen.Select:MismatchedVanilla");
			}
			CS$<>8__locals1.msgBox = new GUIMessageBox(TextManager.Get("ModDownloadTitle"), "", Array.Empty<LocalizedString>(), new Vector2?(new ValueTuple<float, float>(0.5f, 0.75f)), null, Alignment.TopLeft, GUIMessageBox.Type.Default, "", null, "", null, null, false);
			CS$<>8__locals1.innerLayout = CS$<>8__locals1.msgBox.Content;
			CS$<>8__locals1.innerLayout.Stretch = true;
			GUITextBlock header = CS$<>8__locals1.<Select>g__textBlock|5(TextManager.Get("ModDownloadHeader"), GUIStyle.Font, Alignment.CenterLeft);
			CS$<>8__locals1.<Select>g__innerLayoutSpacing|4(0.05f);
			GUIListBox msgBoxModList = new GUIListBox(new RectTransform(Vector2.One, CS$<>8__locals1.innerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), false, null, "", true, false);
			CS$<>8__locals1.<Select>g__innerLayoutSpacing|4(0.05f);
			GUITextBlock footer = CS$<>8__locals1.<Select>g__textBlock|5(TextManager.Get(allowDownloads ? "ModDownloadFooter" : "ModDownloadFooterFail"), GUIStyle.Font, Alignment.Center);
			CS$<>8__locals1.<Select>g__innerLayoutSpacing|4(0.05f);
			CS$<>8__locals1.buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), CS$<>8__locals1.innerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
			if (allowDownloads)
			{
				CS$<>8__locals1.<Select>g__buttonContainerSpacing|6(0.1f);
				CS$<>8__locals1.<Select>g__button|7(TextManager.Get("Yes"), delegate
				{
					CS$<>8__locals1.<>4__this.confirmDownload = true;
				}, 0.3f);
				CS$<>8__locals1.<Select>g__buttonContainerSpacing|6(0.2f);
				CS$<>8__locals1.<Select>g__button|7(TextManager.Get("No"), delegate
				{
					GameClient client = GameMain.Client;
					if (client != null)
					{
						client.Quit();
					}
					GameMain.MainMenuScreen.Select();
				}, 0.3f);
				CS$<>8__locals1.<Select>g__buttonContainerSpacing|6(0.1f);
			}
			else
			{
				CS$<>8__locals1.<Select>g__buttonContainerSpacing|6(0.15f);
				CS$<>8__locals1.<Select>g__button|7(TextManager.Get("Cancel"), delegate
				{
					GameClient client = GameMain.Client;
					if (client != null)
					{
						client.Quit();
					}
					GameMain.MainMenuScreen.Select();
				}, 0.7f);
				CS$<>8__locals1.<Select>g__buttonContainerSpacing|6(0.15f);
			}
			CS$<>8__locals1.missingIds = (from id in (from p in missingPackages
			where p.IsMandatory
			select p into mp
			select ContentPackageId.Parse(mp.UgcId)).NotNone<ContentPackageId>()
			where ContentPackageManager.WorkshopPackages.All((ContentPackage wp) => !wp.UgcId.Equals(id))
			select id).ToArray<ContentPackageId>();
			if (CS$<>8__locals1.missingIds.Any<ContentPackageId>() && SteamManager.IsInitialized)
			{
				CS$<>8__locals1.buttonContainer = new GUILayoutGroup(new RectTransform(new Vector2(1f, 0.1f), CS$<>8__locals1.innerLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
				CS$<>8__locals1.<Select>g__buttonContainerSpacing|6(0.15f);
				CS$<>8__locals1.<Select>g__button|7(TextManager.Get("SubscribeToAllOnWorkshop"), delegate
				{
					if (GameMain.Client != null)
					{
						BulkDownloader.SubscribeToServerMods(from id in CS$<>8__locals1.missingIds.OfType<SteamWorkshopId>()
						select id.Value, new ConnectCommand(GameMain.Client.ServerName, GameMain.Client.ClientPeer.ServerEndpoint));
						GameMain.Client.Quit();
					}
					GameMain.MainMenuScreen.Select();
				}, 0.7f);
				CS$<>8__locals1.<Select>g__buttonContainerSpacing|6(0.15f);
			}
			using (IEnumerator<ServerContentPackage> enumerator = (from p in missingPackages
			where p.IsMandatory
			select p).GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					ServerContentPackage p = enumerator.Current;
					this.pendingDownloads.Enqueue(p);
					new GUITextBlock(new RectTransform(new ValueTuple<float, float>(1f, 0.1f), msgBoxModList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), p.Name, null, null, Alignment.Left, false, "", null).CanBeFocused = false;
					GUIFrame downloadFrame = new GUIFrame(new RectTransform(new ValueTuple<float, float>(1f, 0.06f), downloadList.Content.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "ListBoxElement", null)
					{
						UserData = p,
						CanBeFocused = false
					};
					new GUITextBlock(new RectTransform(new ValueTuple<float, float>(0.5f, 1f), downloadFrame.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), p.Name, null, null, Alignment.Left, false, "", null).CanBeFocused = false;
					GUIProgressBar downloadProgress = new GUIProgressBar(new RectTransform(new ValueTuple<float, float>(0.5f, 0.75f), downloadFrame.RectTransform, Anchor.CenterRight, null, null, null, ScaleBasis.Normal), 0f, new Color?(GUIStyle.Green), "", true);
					Action<GUITextBlock> <>9__27;
					downloadProgress.ProgressGetter = delegate()
					{
						if (CS$<>8__locals1.<>4__this.currentDownload == p)
						{
							if (downloadProgress.GetAnyChild<GUITextBlock>() == null)
							{
								ModDownloadScreen.<>c__DisplayClass9_6 CS$<>8__locals4;
								CS$<>8__locals4.progressBarLayout = new GUILayoutGroup(new RectTransform(Vector2.One, downloadProgress.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), true, Anchor.TopLeft);
								ModDownloadScreen.<Select>g__progressBarText|9_29(0.475f, Alignment.CenterRight, delegate
								{
									FileReceiver.FileTransferIn fileTransferIn2 = ModDownloadScreen.<Select>g__getTransfer|9_26();
									return MathUtils.GetBytesReadable((long)((fileTransferIn2 != null) ? fileTransferIn2.Received : 0));
								}, ref CS$<>8__locals4);
								ModDownloadScreen.<Select>g__progressBarText|9_29(0.05f, Alignment.Center, () => "/", ref CS$<>8__locals4);
								ModDownloadScreen.<Select>g__progressBarText|9_29(0.475f, Alignment.CenterLeft, delegate
								{
									FileReceiver.FileTransferIn fileTransferIn2 = ModDownloadScreen.<Select>g__getTransfer|9_26();
									return MathUtils.GetBytesReadable((long)((fileTransferIn2 != null) ? fileTransferIn2.FileSize : 0));
								}, ref CS$<>8__locals4);
							}
							FileReceiver.FileTransferIn fileTransferIn = ModDownloadScreen.<Select>g__getTransfer|9_26();
							if (fileTransferIn == null)
							{
								return 0f;
							}
							return fileTransferIn.Progress;
						}
						else
						{
							if (!CS$<>8__locals1.<>4__this.pendingDownloads.Contains(p))
							{
								IEnumerable<GUITextBlock> source = downloadProgress.GetAllChildren<GUITextBlock>().ToArray<GUITextBlock>();
								Action<GUITextBlock> action;
								if ((action = <>9__27) == null)
								{
									action = (<>9__27 = delegate(GUITextBlock c)
									{
										downloadProgress.RemoveChild(c);
									});
								}
								source.ForEach(action);
								return 1f;
							}
							return 0f;
						}
					};
				}
			}
		}

		// Token: 0x0600259B RID: 9627 RVA: 0x001869C0 File Offset: 0x00184BC0
		public override void Update(double deltaTime)
		{
			base.Update(deltaTime);
			if (GameMain.Client == null)
			{
				return;
			}
			if (!this.confirmDownload)
			{
				return;
			}
			if (this.currentDownload != null)
			{
				if (GameMain.Client.FileReceiver.ActiveTransfers.None(null))
				{
					GameMain.Client.RequestFile(FileTransferType.Mod, this.currentDownload.Name, this.currentDownload.Hash.StringRepresentation);
				}
				return;
			}
			if (this.pendingDownloads.TryDequeue(out this.currentDownload))
			{
				GameMain.Client.RequestFile(FileTransferType.Mod, this.currentDownload.Name, this.currentDownload.Hash.StringRepresentation);
				return;
			}
			ImmutableArray<ServerContentPackage> serverPackages = GameMain.Client.ClientPeer.ServerContentPackages;
			CorePackage corePackage2;
			if ((corePackage2 = (this.downloadedPackages.FirstOrDefault((ContentPackage p) => p is CorePackage) as CorePackage)) == null)
			{
				ServerContentPackage serverContentPackage = serverPackages.FirstOrDefault((ServerContentPackage p) => p.CorePackage != null);
				if ((corePackage2 = ((serverContentPackage != null) ? serverContentPackage.CorePackage : null)) == null)
				{
					throw new Exception("Failed to find core package to enable");
				}
			}
			CorePackage corePackage = corePackage2;
			List<RegularPackage> regularPackages = new List<RegularPackage>();
			ImmutableArray<ServerContentPackage>.Enumerator enumerator = serverPackages.GetEnumerator();
			while (enumerator.MoveNext())
			{
				ServerContentPackage p = enumerator.Current;
				if (p.CorePackage == null && !corePackage.Hash.Equals(p.Hash))
				{
					RegularPackage matchingPackage = p.RegularPackage ?? (this.downloadedPackages.FirstOrDefault((ContentPackage d) => d is RegularPackage && d.Hash.Equals(p.Hash)) as RegularPackage);
					if (matchingPackage == null)
					{
						if (p.IsMandatory)
						{
							throw new Exception("Could not find regular package \"" + p.Name + "\"");
						}
					}
					else
					{
						regularPackages.Add(matchingPackage);
					}
				}
			}
			foreach (RegularPackage regularPackage in regularPackages)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(14, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Enabling \"");
				defaultInterpolatedStringHandler.AppendFormatted(regularPackage.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" (");
				defaultInterpolatedStringHandler.AppendFormatted(regularPackage.Dir);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				DebugConsole.NewMessage(defaultInterpolatedStringHandler.ToStringAndClear(), new Color?(Color.Lime), false);
			}
			regularPackages.AddRange(from p in ContentPackageManager.EnabledPackages.Regular
			where !p.HasMultiplayerSyncedContent && !regularPackages.Contains(p)
			select p);
			ContentPackageManager.EnabledPackages.BackUp();
			ContentPackageManager.EnabledPackages.SetCore(corePackage);
			ContentPackageManager.EnabledPackages.SetRegular(regularPackages);
			using (List<SubmarineInfo>.Enumerator enumerator3 = GameMain.Client.ServerSubmarines.GetEnumerator())
			{
				while (enumerator3.MoveNext())
				{
					SubmarineInfo serverSub = enumerator3.Current;
					if (!File.Exists(serverSub.FilePath))
					{
						SubmarineInfo matchingSub = SubmarineInfo.SavedSubmarines.FirstOrDefault((SubmarineInfo s) => s.Name == serverSub.Name && s.MD5Hash == serverSub.MD5Hash);
						if (matchingSub != null)
						{
							serverSub.FilePath = matchingSub.FilePath;
						}
					}
				}
			}
			GameMain.NetLobbyScreen.UpdateSubList(GameMain.NetLobbyScreen.SubList, GameMain.Client.ServerSubmarines);
			GameMain.NetLobbyScreen.Select();
		}

		// Token: 0x0600259C RID: 9628 RVA: 0x00186D58 File Offset: 0x00184F58
		public void CurrentDownloadFinished(FileReceiver.FileTransferIn transfer)
		{
			if (this.currentDownload == null)
			{
				throw new Exception("Current download is null");
			}
			string path = transfer.FilePath;
			if (!path.EndsWith(".barodir.gz", StringComparison.OrdinalIgnoreCase))
			{
				return;
			}
			string dir = path.RemoveFromEnd(".barodir.gz", StringComparison.OrdinalIgnoreCase);
			SaveUtil.DecompressToDirectory(path, dir);
			Result<ContentPackage, Exception> result = ContentPackage.TryLoad(Path.Combine(new string[]
			{
				dir,
				"filelist.xml"
			}).CleanUpPathCrossPlatform(true, ""));
			ContentPackage newPackage;
			if (!result.TryUnwrapSuccess(out newPackage))
			{
				Exception exception;
				throw new Exception("Failed to load downloaded mod \"" + this.currentDownload.Name + "\"", result.TryUnwrapFailure(out exception) ? exception : null);
			}
			if (!this.currentDownload.Hash.Equals(newPackage.Hash))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 3);
				defaultInterpolatedStringHandler.AppendLiteral("Hash mismatch for downloaded mod \"");
				defaultInterpolatedStringHandler.AppendFormatted(this.currentDownload.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" (expected ");
				defaultInterpolatedStringHandler.AppendFormatted<Md5Hash>(this.currentDownload.Hash);
				defaultInterpolatedStringHandler.AppendLiteral(", got ");
				defaultInterpolatedStringHandler.AppendFormatted<Md5Hash>(newPackage.Hash);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			this.downloadedPackages.Add(newPackage);
			this.currentDownload = null;
		}

		// Token: 0x0600259D RID: 9629 RVA: 0x00186EA8 File Offset: 0x001850A8
		public override void Draw(double deltaTime, GraphicsDevice graphics, SpriteBatch spriteBatch)
		{
			spriteBatch.Begin(SpriteSortMode.Deferred, null, GUI.SamplerState, null, GameMain.ScissorTestEnable, null, null);
			GameMain.MainMenuScreen.DrawBackground(graphics, spriteBatch);
			GUI.Draw(this.Cam, spriteBatch);
			spriteBatch.End();
		}

		// Token: 0x0600259F RID: 9631 RVA: 0x00186F0E File Offset: 0x0018510E
		[NullableContext(2)]
		[CompilerGenerated]
		internal static FileReceiver.FileTransferIn <Select>g__getTransfer|9_26()
		{
			GameClient client = GameMain.Client;
			if (client == null)
			{
				return null;
			}
			return client.FileReceiver.ActiveTransfers.FirstOrDefault((FileReceiver.FileTransferIn t) => t.FileType == FileTransferType.Mod);
		}

		// Token: 0x060025A0 RID: 9632 RVA: 0x00186F4C File Offset: 0x0018514C
		[CompilerGenerated]
		internal static void <Select>g__progressBarText|9_29(float width, Alignment textAlignment, Func<string> getter, ref ModDownloadScreen.<>c__DisplayClass9_6 A_3)
		{
			GUIFrame textContainer = new GUIFrame(new RectTransform(new ValueTuple<float, float>(width, 1f), A_3.progressBarLayout.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, null);
			GUITextBlock textShadow = new GUITextBlock(new RectTransform(Vector2.One, textContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal)
			{
				AbsoluteOffset = new Point(GUI.IntScale(3f))
			}, "", new Color?(Color.Black), null, textAlignment, false, "", null);
			GUITextBlock text = new GUITextBlock(new RectTransform(Vector2.One, textContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), "", null, null, textAlignment, false, "", null);
			new GUICustomComponent(new RectTransform(Vector2.Zero, textContainer.RectTransform, Anchor.TopLeft, null, null, null, ScaleBasis.Normal), null, delegate(float f, GUICustomComponent component)
			{
				string str = getter();
				RichString text = text.Text;
				if (((text != null) ? text.SanitizedValue : null) != str)
				{
					text.Text = str;
					textShadow.Text = str;
				}
			});
		}

		// Token: 0x040012D7 RID: 4823
		private readonly Queue<ServerContentPackage> pendingDownloads = new Queue<ServerContentPackage>();

		// Token: 0x040012D8 RID: 4824
		[Nullable(2)]
		private ServerContentPackage currentDownload;

		// Token: 0x040012D9 RID: 4825
		private readonly List<ContentPackage> downloadedPackages = new List<ContentPackage>();

		// Token: 0x040012DA RID: 4826
		private bool confirmDownload;
	}
}
