using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020003A9 RID: 937
	[NullableContext(1)]
	[Nullable(0)]
	public class FileInfo
	{
		// Token: 0x06004597 RID: 17815 RVA: 0x002691C9 File Offset: 0x002673C9
		public FileInfo(string path)
		{
			this.innerInfo = new FileInfo(path);
		}

		// Token: 0x06004598 RID: 17816 RVA: 0x002691DD File Offset: 0x002673DD
		public FileInfo(FileInfo info)
		{
			this.innerInfo = info;
		}

		// Token: 0x170011F0 RID: 4592
		// (get) Token: 0x06004599 RID: 17817 RVA: 0x002691EC File Offset: 0x002673EC
		public bool Exists
		{
			get
			{
				return this.innerInfo.Exists;
			}
		}

		// Token: 0x170011F1 RID: 4593
		// (get) Token: 0x0600459A RID: 17818 RVA: 0x002691F9 File Offset: 0x002673F9
		public string Name
		{
			get
			{
				return this.innerInfo.Name;
			}
		}

		// Token: 0x170011F2 RID: 4594
		// (get) Token: 0x0600459B RID: 17819 RVA: 0x00269206 File Offset: 0x00267406
		public string FullName
		{
			get
			{
				return this.innerInfo.FullName;
			}
		}

		// Token: 0x170011F3 RID: 4595
		// (get) Token: 0x0600459C RID: 17820 RVA: 0x00269213 File Offset: 0x00267413
		public long Length
		{
			get
			{
				return this.innerInfo.Length;
			}
		}

		// Token: 0x170011F4 RID: 4596
		// (get) Token: 0x0600459D RID: 17821 RVA: 0x00269220 File Offset: 0x00267420
		// (set) Token: 0x0600459E RID: 17822 RVA: 0x00269230 File Offset: 0x00267430
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

		// Token: 0x0600459F RID: 17823 RVA: 0x002692B0 File Offset: 0x002674B0
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

		// Token: 0x060045A0 RID: 17824 RVA: 0x00269328 File Offset: 0x00267528
		public void Delete()
		{
			if (!Validation.CanWrite(this.innerInfo.FullName, false))
			{
				DebugConsole.ThrowError("Cannot delete file \"" + this.Name + "\": modifying the files in this folder/with this extension is not allowed.", null, null, false, false);
				return;
			}
			this.innerInfo.Delete();
		}

		// Token: 0x04002429 RID: 9257
		private FileInfo innerInfo;
	}
}
