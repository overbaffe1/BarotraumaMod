using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020003A8 RID: 936
	[NullableContext(1)]
	[Nullable(0)]
	public class DirectoryInfo
	{
		// Token: 0x0600458E RID: 17806 RVA: 0x00269113 File Offset: 0x00267313
		public DirectoryInfo(string path)
		{
			this.innerInfo = new DirectoryInfo(path);
		}

		// Token: 0x0600458F RID: 17807 RVA: 0x00269127 File Offset: 0x00267327
		private DirectoryInfo(DirectoryInfo info)
		{
			this.innerInfo = info;
		}

		// Token: 0x170011EC RID: 4588
		// (get) Token: 0x06004590 RID: 17808 RVA: 0x00269136 File Offset: 0x00267336
		public bool Exists
		{
			get
			{
				return this.innerInfo.Exists;
			}
		}

		// Token: 0x170011ED RID: 4589
		// (get) Token: 0x06004591 RID: 17809 RVA: 0x00269143 File Offset: 0x00267343
		public string Name
		{
			get
			{
				return this.innerInfo.Name;
			}
		}

		// Token: 0x170011EE RID: 4590
		// (get) Token: 0x06004592 RID: 17810 RVA: 0x00269150 File Offset: 0x00267350
		public string FullName
		{
			get
			{
				return this.innerInfo.FullName;
			}
		}

		// Token: 0x170011EF RID: 4591
		// (get) Token: 0x06004593 RID: 17811 RVA: 0x0026915D File Offset: 0x0026735D
		public FileAttributes Attributes
		{
			get
			{
				return this.innerInfo.Attributes;
			}
		}

		// Token: 0x06004594 RID: 17812 RVA: 0x0026916A File Offset: 0x0026736A
		public IEnumerable<DirectoryInfo> GetDirectories()
		{
			DirectoryInfo.<GetDirectories>d__11 <GetDirectories>d__ = new DirectoryInfo.<GetDirectories>d__11(-2);
			<GetDirectories>d__.<>4__this = this;
			return <GetDirectories>d__;
		}

		// Token: 0x06004595 RID: 17813 RVA: 0x0026917A File Offset: 0x0026737A
		public IEnumerable<FileInfo> GetFiles()
		{
			DirectoryInfo.<GetFiles>d__12 <GetFiles>d__ = new DirectoryInfo.<GetFiles>d__12(-2);
			<GetFiles>d__.<>4__this = this;
			return <GetFiles>d__;
		}

		// Token: 0x06004596 RID: 17814 RVA: 0x0026918A File Offset: 0x0026738A
		public void Delete()
		{
			if (!Validation.CanWrite(this.innerInfo.FullName, false))
			{
				DebugConsole.ThrowError("Cannot delete directory \"" + this.Name + "\": modifying the contents of this folder/using this extension is not allowed.", null, null, false, false);
				return;
			}
			this.innerInfo.Delete();
		}

		// Token: 0x04002428 RID: 9256
		private DirectoryInfo innerInfo;
	}
}
