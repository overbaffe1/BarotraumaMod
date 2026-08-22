using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020002DF RID: 735
	[NullableContext(1)]
	[Nullable(0)]
	public class FileInfo
	{
		// Token: 0x0600314B RID: 12619 RVA: 0x00150D55 File Offset: 0x0014EF55
		public FileInfo(string path)
		{
			this.innerInfo = new FileInfo(path);
		}

		// Token: 0x0600314C RID: 12620 RVA: 0x00150D69 File Offset: 0x0014EF69
		public FileInfo(FileInfo info)
		{
			this.innerInfo = info;
		}

		// Token: 0x17000DFC RID: 3580
		// (get) Token: 0x0600314D RID: 12621 RVA: 0x00150D78 File Offset: 0x0014EF78
		public bool Exists
		{
			get
			{
				return this.innerInfo.Exists;
			}
		}

		// Token: 0x17000DFD RID: 3581
		// (get) Token: 0x0600314E RID: 12622 RVA: 0x00150D85 File Offset: 0x0014EF85
		public string Name
		{
			get
			{
				return this.innerInfo.Name;
			}
		}

		// Token: 0x17000DFE RID: 3582
		// (get) Token: 0x0600314F RID: 12623 RVA: 0x00150D92 File Offset: 0x0014EF92
		public string FullName
		{
			get
			{
				return this.innerInfo.FullName;
			}
		}

		// Token: 0x17000DFF RID: 3583
		// (get) Token: 0x06003150 RID: 12624 RVA: 0x00150D9F File Offset: 0x0014EF9F
		public long Length
		{
			get
			{
				return this.innerInfo.Length;
			}
		}

		// Token: 0x17000E00 RID: 3584
		// (get) Token: 0x06003151 RID: 12625 RVA: 0x00150DAC File Offset: 0x0014EFAC
		// (set) Token: 0x06003152 RID: 12626 RVA: 0x00150DBC File Offset: 0x0014EFBC
		public bool IsReadOnly
		{
			get
			{
				return this.innerInfo.IsReadOnly;
			}
			set
			{
				if (!Validation.CanWrite(this.innerInfo.FullName, false))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(103, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Cannot set read-only to ");
					defaultInterpolatedStringHandler.AppendFormatted<bool>(value);
					defaultInterpolatedStringHandler.AppendLiteral(" for \"");
					defaultInterpolatedStringHandler.AppendFormatted(this.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\": modifying the files in this folder/with this extension is not allowed.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
					return;
				}
				this.innerInfo.IsReadOnly = value;
			}
		}

		// Token: 0x06003153 RID: 12627 RVA: 0x00150E3C File Offset: 0x0014F03C
		public void CopyTo(string dest, bool overwriteExisting = false)
		{
			if (!Validation.CanWrite(dest, false))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(86, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Cannot copy \"");
				defaultInterpolatedStringHandler.AppendFormatted(this.Name);
				defaultInterpolatedStringHandler.AppendLiteral("\" to \"");
				defaultInterpolatedStringHandler.AppendFormatted(dest);
				defaultInterpolatedStringHandler.AppendLiteral("\": modifying the contents of the destination folder is not allowed.");
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				return;
			}
			this.innerInfo.CopyTo(dest, overwriteExisting);
		}

		// Token: 0x06003154 RID: 12628 RVA: 0x00150EB4 File Offset: 0x0014F0B4
		public void Delete()
		{
			if (!Validation.CanWrite(this.innerInfo.FullName, false))
			{
				DebugConsole.ThrowError("Cannot delete file \"" + this.Name + "\": modifying the files in this folder/with this extension is not allowed.", null, null, false, false);
				return;
			}
			this.innerInfo.Delete();
		}

		// Token: 0x0400185B RID: 6235
		private FileInfo innerInfo;
	}
}
