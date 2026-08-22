using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Steamworks.Ugc;

namespace Barotrauma
{
	// Token: 0x02000215 RID: 533
	internal class LuaCsSteam
	{
		// Token: 0x0600254F RID: 9551 RVA: 0x000F52E0 File Offset: 0x000F34E0
		private static void CopyFolder(string sourceDirName, string destDirName, bool copySubDirs, bool overwriteExisting = false)
		{
			DirectoryInfo dir = new DirectoryInfo(sourceDirName);
			if (!dir.Exists)
			{
				throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
			}
			IEnumerable<DirectoryInfo> dirs = dir.GetDirectories();
			if (!Directory.Exists(destDirName))
			{
				Directory.CreateDirectory(destDirName);
			}
			IEnumerable<FileInfo> files = dir.GetFiles();
			foreach (FileInfo file in files)
			{
				string tempPath = Path.Combine(destDirName, file.Name);
				if (overwriteExisting || !File.Exists(tempPath))
				{
					file.CopyTo(tempPath, true);
				}
			}
			if (copySubDirs)
			{
				foreach (DirectoryInfo subdir in dirs)
				{
					string tempPath2 = Path.Combine(destDirName, subdir.Name);
					LuaCsSteam.CopyFolder(subdir.FullName, tempPath2, copySubDirs, overwriteExisting);
				}
			}
		}

		// Token: 0x06002550 RID: 9552 RVA: 0x000F53E0 File Offset: 0x000F35E0
		private void DownloadWorkshopItemAsync(LuaCsSteam.WorkshopItemDownload download, bool startDownload = false)
		{
			LuaCsSteam.<DownloadWorkshopItemAsync>d__5 <DownloadWorkshopItemAsync>d__;
			<DownloadWorkshopItemAsync>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<DownloadWorkshopItemAsync>d__.<>4__this = this;
			<DownloadWorkshopItemAsync>d__.download = download;
			<DownloadWorkshopItemAsync>d__.startDownload = startDownload;
			<DownloadWorkshopItemAsync>d__.<>1__state = -1;
			<DownloadWorkshopItemAsync>d__.<>t__builder.Start<LuaCsSteam.<DownloadWorkshopItemAsync>d__5>(ref <DownloadWorkshopItemAsync>d__);
		}

		// Token: 0x06002551 RID: 9553 RVA: 0x000F5428 File Offset: 0x000F3628
		public void DownloadWorkshopItem(ulong id, string destination, LuaCsAction callback)
		{
			LuaCsSteam.<DownloadWorkshopItem>d__6 <DownloadWorkshopItem>d__;
			<DownloadWorkshopItem>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<DownloadWorkshopItem>d__.<>4__this = this;
			<DownloadWorkshopItem>d__.id = id;
			<DownloadWorkshopItem>d__.destination = destination;
			<DownloadWorkshopItem>d__.callback = callback;
			<DownloadWorkshopItem>d__.<>1__state = -1;
			<DownloadWorkshopItem>d__.<>t__builder.Start<LuaCsSteam.<DownloadWorkshopItem>d__6>(ref <DownloadWorkshopItem>d__);
		}

		// Token: 0x06002552 RID: 9554 RVA: 0x000F5478 File Offset: 0x000F3678
		public void DownloadWorkshopItem(Item item, string destination, LuaCsAction callback)
		{
			this.DownloadWorkshopItemAsync(new LuaCsSteam.WorkshopItemDownload
			{
				Item = item,
				Destination = destination,
				Callback = callback
			}, true);
		}

		// Token: 0x06002553 RID: 9555 RVA: 0x000F54B0 File Offset: 0x000F36B0
		public void GetWorkshopItem(ulong id, LuaCsAction callback)
		{
			LuaCsSteam.<GetWorkshopItem>d__8 <GetWorkshopItem>d__;
			<GetWorkshopItem>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<GetWorkshopItem>d__.id = id;
			<GetWorkshopItem>d__.callback = callback;
			<GetWorkshopItem>d__.<>1__state = -1;
			<GetWorkshopItem>d__.<>t__builder.Start<LuaCsSteam.<GetWorkshopItem>d__8>(ref <GetWorkshopItem>d__);
		}

		// Token: 0x06002554 RID: 9556 RVA: 0x000F54F0 File Offset: 0x000F36F0
		public void Update()
		{
			if (this.itemsBeingDownloaded.Count > 0 && Timing.TotalTime > this.lastTimeChecked)
			{
				foreach (LuaCsSteam.WorkshopItemDownload item in this.itemsBeingDownloaded.ToArray())
				{
					this.DownloadWorkshopItemAsync(item, false);
				}
				this.lastTimeChecked = Timing.TotalTime + 15.0;
			}
		}

		// Token: 0x04001251 RID: 4689
		private double lastTimeChecked;

		// Token: 0x04001252 RID: 4690
		private List<LuaCsSteam.WorkshopItemDownload> itemsBeingDownloaded = new List<LuaCsSteam.WorkshopItemDownload>();

		// Token: 0x020009F0 RID: 2544
		private struct WorkshopItemDownload
		{
			// Token: 0x040034C1 RID: 13505
			public Item Item;

			// Token: 0x040034C2 RID: 13506
			public string Destination;

			// Token: 0x040034C3 RID: 13507
			public LuaCsAction Callback;
		}
	}
}
