using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Steamworks.Ugc;

namespace Barotrauma
{
	// Token: 0x020002FD RID: 765
	internal class LuaCsSteam
	{
		// Token: 0x06003E36 RID: 15926 RVA: 0x00232C70 File Offset: 0x00230E70
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

		// Token: 0x06003E37 RID: 15927 RVA: 0x00232D70 File Offset: 0x00230F70
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

		// Token: 0x06003E38 RID: 15928 RVA: 0x00232DB8 File Offset: 0x00230FB8
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

		// Token: 0x06003E39 RID: 15929 RVA: 0x00232E08 File Offset: 0x00231008
		public void DownloadWorkshopItem(Item item, string destination, LuaCsAction callback)
		{
			this.DownloadWorkshopItemAsync(new LuaCsSteam.WorkshopItemDownload
			{
				Item = item,
				Destination = destination,
				Callback = callback
			}, true);
		}

		// Token: 0x06003E3A RID: 15930 RVA: 0x00232E40 File Offset: 0x00231040
		public void GetWorkshopItem(ulong id, LuaCsAction callback)
		{
			LuaCsSteam.<GetWorkshopItem>d__8 <GetWorkshopItem>d__;
			<GetWorkshopItem>d__.<>t__builder = AsyncVoidMethodBuilder.Create();
			<GetWorkshopItem>d__.id = id;
			<GetWorkshopItem>d__.callback = callback;
			<GetWorkshopItem>d__.<>1__state = -1;
			<GetWorkshopItem>d__.<>t__builder.Start<LuaCsSteam.<GetWorkshopItem>d__8>(ref <GetWorkshopItem>d__);
		}

		// Token: 0x06003E3B RID: 15931 RVA: 0x00232E80 File Offset: 0x00231080
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

		// Token: 0x0400207D RID: 8317
		private double lastTimeChecked;

		// Token: 0x0400207E RID: 8318
		private List<LuaCsSteam.WorkshopItemDownload> itemsBeingDownloaded = new List<LuaCsSteam.WorkshopItemDownload>();

		// Token: 0x02000FD4 RID: 4052
		private struct WorkshopItemDownload
		{
			// Token: 0x040056AC RID: 22188
			public Item Item;

			// Token: 0x040056AD RID: 22189
			public string Destination;

			// Token: 0x040056AE RID: 22190
			public LuaCsAction Callback;
		}
	}
}
