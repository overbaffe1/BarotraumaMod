using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020002DE RID: 734
	[NullableContext(1)]
	[Nullable(0)]
	public class DirectoryInfo
	{
		// Token: 0x06003142 RID: 12610 RVA: 0x00150C9F File Offset: 0x0014EE9F
		public DirectoryInfo(string path)
		{
			this.innerInfo = new DirectoryInfo(path);
		}

		// Token: 0x06003143 RID: 12611 RVA: 0x00150CB3 File Offset: 0x0014EEB3
		private DirectoryInfo(DirectoryInfo info)
		{
			this.innerInfo = info;
		}

		// Token: 0x17000DF8 RID: 3576
		// (get) Token: 0x06003144 RID: 12612 RVA: 0x00150CC2 File Offset: 0x0014EEC2
		public bool Exists
		{
			get
			{
				return this.innerInfo.Exists;
			}
		}

		// Token: 0x17000DF9 RID: 3577
		// (get) Token: 0x06003145 RID: 12613 RVA: 0x00150CCF File Offset: 0x0014EECF
		public string Name
		{
			get
			{
				return this.innerInfo.Name;
			}
		}

		// Token: 0x17000DFA RID: 3578
		// (get) Token: 0x06003146 RID: 12614 RVA: 0x00150CDC File Offset: 0x0014EEDC
		public string FullName
		{
			get
			{
				return this.innerInfo.FullName;
			}
		}

		// Token: 0x17000DFB RID: 3579
		// (get) Token: 0x06003147 RID: 12615 RVA: 0x00150CE9 File Offset: 0x0014EEE9
		public FileAttributes Attributes
		{
			get
			{
				return this.innerInfo.Attributes;
			}
		}

		// Token: 0x06003148 RID: 12616 RVA: 0x00150CF6 File Offset: 0x0014EEF6
		public IEnumerable<DirectoryInfo> GetDirectories()
		{
			DirectoryInfo.<GetDirectories>d__11 <GetDirectories>d__ = new DirectoryInfo.<GetDirectories>d__11(-2);
			<GetDirectories>d__.<>4__this = this;
			return <GetDirectories>d__;
		}

		// Token: 0x06003149 RID: 12617 RVA: 0x00150D06 File Offset: 0x0014EF06
		public IEnumerable<FileInfo> GetFiles()
		{
			DirectoryInfo.<GetFiles>d__12 <GetFiles>d__ = new DirectoryInfo.<GetFiles>d__12(-2);
			<GetFiles>d__.<>4__this = this;
			return <GetFiles>d__;
		}

		// Token: 0x0600314A RID: 12618 RVA: 0x00150D16 File Offset: 0x0014EF16
		public void Delete()
		{
			if (!Validation.CanWrite(this.innerInfo.FullName, false))
			{
				DebugConsole.ThrowError("Cannot delete directory \"" + this.Name + "\": modifying the contents of this folder/using this extension is not allowed.", null, null, false, false);
				return;
			}
			this.innerInfo.Delete();
		}

		// Token: 0x0400185A RID: 6234
		private DirectoryInfo innerInfo;
	}
}
