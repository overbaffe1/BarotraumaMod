using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020002DD RID: 733
	[NullableContext(1)]
	[Nullable(0)]
	public class FileStream : Stream
	{
		// Token: 0x06003134 RID: 12596 RVA: 0x00150B96 File Offset: 0x0014ED96
		public FileStream(string fn, FileStream stream)
		{
			this.innerStream = stream;
			this.fileName = fn;
		}

		// Token: 0x17000DF2 RID: 3570
		// (get) Token: 0x06003135 RID: 12597 RVA: 0x00150BAC File Offset: 0x0014EDAC
		public override bool CanRead
		{
			get
			{
				return this.innerStream.CanRead;
			}
		}

		// Token: 0x17000DF3 RID: 3571
		// (get) Token: 0x06003136 RID: 12598 RVA: 0x00150BB9 File Offset: 0x0014EDB9
		public override bool CanSeek
		{
			get
			{
				return this.innerStream.CanSeek;
			}
		}

		// Token: 0x17000DF4 RID: 3572
		// (get) Token: 0x06003137 RID: 12599 RVA: 0x00150BC6 File Offset: 0x0014EDC6
		public override bool CanTimeout
		{
			get
			{
				return this.innerStream.CanTimeout;
			}
		}

		// Token: 0x17000DF5 RID: 3573
		// (get) Token: 0x06003138 RID: 12600 RVA: 0x00150BD3 File Offset: 0x0014EDD3
		public override bool CanWrite
		{
			get
			{
				return Validation.CanWrite(this.fileName, false) && this.innerStream.CanWrite;
			}
		}

		// Token: 0x17000DF6 RID: 3574
		// (get) Token: 0x06003139 RID: 12601 RVA: 0x00150BF0 File Offset: 0x0014EDF0
		public override long Length
		{
			get
			{
				return this.innerStream.Length;
			}
		}

		// Token: 0x17000DF7 RID: 3575
		// (get) Token: 0x0600313A RID: 12602 RVA: 0x00150BFD File Offset: 0x0014EDFD
		// (set) Token: 0x0600313B RID: 12603 RVA: 0x00150C0A File Offset: 0x0014EE0A
		public override long Position
		{
			get
			{
				return this.innerStream.Position;
			}
			set
			{
				this.innerStream.Position = value;
			}
		}

		// Token: 0x0600313C RID: 12604 RVA: 0x00150C18 File Offset: 0x0014EE18
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.innerStream.Read(buffer, offset, count);
		}

		// Token: 0x0600313D RID: 12605 RVA: 0x00150C28 File Offset: 0x0014EE28
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Validation.CanWrite(this.fileName, false))
			{
				this.innerStream.Write(buffer, offset, count);
				return;
			}
			DebugConsole.ThrowError("Cannot write to file \"" + this.fileName + "\": modifying the files in this folder/with this extension is not allowed.", null, null, false, false);
		}

		// Token: 0x0600313E RID: 12606 RVA: 0x00150C65 File Offset: 0x0014EE65
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.innerStream.Seek(offset, origin);
		}

		// Token: 0x0600313F RID: 12607 RVA: 0x00150C74 File Offset: 0x0014EE74
		public override void SetLength(long value)
		{
			this.innerStream.SetLength(value);
		}

		// Token: 0x06003140 RID: 12608 RVA: 0x00150C82 File Offset: 0x0014EE82
		public override void Flush()
		{
			this.innerStream.Flush();
		}

		// Token: 0x06003141 RID: 12609 RVA: 0x00150C8F File Offset: 0x0014EE8F
		protected override void Dispose(bool notCalledByFinalizer)
		{
			if (notCalledByFinalizer)
			{
				this.innerStream.Dispose();
			}
		}

		// Token: 0x04001858 RID: 6232
		private readonly FileStream innerStream;

		// Token: 0x04001859 RID: 6233
		private readonly string fileName;
	}
}
