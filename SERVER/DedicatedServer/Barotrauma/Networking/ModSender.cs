using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Barotrauma.IO;

namespace Barotrauma.Networking
{
	// Token: 0x0200036B RID: 875
	internal class ModSender : IDisposable
	{
		// Token: 0x17000E67 RID: 3687
		// (get) Token: 0x060033CE RID: 13262 RVA: 0x0015E4A3 File Offset: 0x0015C6A3
		// (set) Token: 0x060033CF RID: 13263 RVA: 0x0015E4AB File Offset: 0x0015C6AB
		public bool Ready { get; private set; }

		// Token: 0x060033D0 RID: 13264 RVA: 0x0015E4B4 File Offset: 0x0015C6B4
		public ModSender()
		{
			this.DeleteDir();
			Directory.CreateDirectory("TempMods_Upload", false);
			TaskPool.Add("ModSender", Task.WhenAll((from p in ContentPackageManager.EnabledPackages.All
			where p != ContentPackageManager.VanillaCorePackage && p.HasMultiplayerSyncedContent
			select p).Select(new Func<ContentPackage, Task>(this.CompressMod))), delegate(Task t)
			{
				AggregateException exception = t.Exception;
				Exception innermostException = (exception != null) ? exception.GetInnermost() : null;
				if (innermostException != null)
				{
					DebugConsole.ThrowError("An error occurred when compressing mods", innermostException, null, false, false);
				}
				this.Ready = true;
			});
		}

		// Token: 0x060033D1 RID: 13265 RVA: 0x0015E530 File Offset: 0x0015C730
		public static string GetCompressedModPath(ContentPackage mod)
		{
			string dir = mod.Dir;
			ContentPackageId ugcId;
			string resultFileName = dir.StartsWith("LocalMods") ? ("Local_" + mod.Name) : ("Workshop_" + mod.Name + "_" + (mod.UgcId.TryUnwrap(out ugcId) ? ugcId.ToString() : "NULL"));
			resultFileName = ToolBox.RemoveInvalidFileNameChars(resultFileName.Replace('\\', '_').Replace('/', '_'));
			resultFileName += ".barodir.gz";
			return Path.Combine(new string[]
			{
				"TempMods_Upload",
				resultFileName
			});
		}

		// Token: 0x060033D2 RID: 13266 RVA: 0x0015E5D4 File Offset: 0x0015C7D4
		public Task CompressMod(ContentPackage mod)
		{
			ModSender.<CompressMod>d__8 <CompressMod>d__;
			<CompressMod>d__.<>t__builder = AsyncTaskMethodBuilder.Create();
			<CompressMod>d__.mod = mod;
			<CompressMod>d__.<>1__state = -1;
			<CompressMod>d__.<>t__builder.Start<ModSender.<CompressMod>d__8>(ref <CompressMod>d__);
			return <CompressMod>d__.<>t__builder.Task;
		}

		// Token: 0x060033D3 RID: 13267 RVA: 0x0015E617 File Offset: 0x0015C817
		private void DeleteDir()
		{
			if (Directory.Exists("TempMods_Upload"))
			{
				Directory.Delete("TempMods_Upload", true, true);
			}
		}

		// Token: 0x17000E68 RID: 3688
		// (get) Token: 0x060033D4 RID: 13268 RVA: 0x0015E631 File Offset: 0x0015C831
		// (set) Token: 0x060033D5 RID: 13269 RVA: 0x0015E639 File Offset: 0x0015C839
		public bool IsDisposed { get; private set; }

		// Token: 0x060033D6 RID: 13270 RVA: 0x0015E642 File Offset: 0x0015C842
		public void Dispose()
		{
			this.IsDisposed = true;
			this.DeleteDir();
		}

		// Token: 0x040019E6 RID: 6630
		public const string UploadFolder = "TempMods_Upload";

		// Token: 0x040019E7 RID: 6631
		public const string Extension = ".barodir.gz";
	}
}
