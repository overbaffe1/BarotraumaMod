using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Barotrauma.IO
{
	// Token: 0x020003A7 RID: 935
	[NullableContext(1)]
	[Nullable(0)]
	public class FileStream : Stream
	{
		// Token: 0x06004580 RID: 17792 RVA: 0x0026900A File Offset: 0x0026720A
		public FileStream(string fn, FileStream stream)
		{
			this.innerStream = stream;
			this.fileName = fn;
		}

		// Token: 0x170011E6 RID: 4582
		// (get) Token: 0x06004581 RID: 17793 RVA: 0x00269020 File Offset: 0x00267220
		public override bool CanRead
		{
			get
			{
				return this.innerStream.CanRead;
			}
		}

		// Token: 0x170011E7 RID: 4583
		// (get) Token: 0x06004582 RID: 17794 RVA: 0x0026902D File Offset: 0x0026722D
		public override bool CanSeek
		{
			get
			{
				return this.innerStream.CanSeek;
			}
		}

		// Token: 0x170011E8 RID: 4584
		// (get) Token: 0x06004583 RID: 17795 RVA: 0x0026903A File Offset: 0x0026723A
		public override bool CanTimeout
		{
			get
			{
				return this.innerStream.CanTimeout;
			}
		}

		// Token: 0x170011E9 RID: 4585
		// (get) Token: 0x06004584 RID: 17796 RVA: 0x00269047 File Offset: 0x00267247
		public override bool CanWrite
		{
			get
			{
				return Validation.CanWrite(this.fileName, false) && this.innerStream.CanWrite;
			}
		}

		// Token: 0x170011EA RID: 4586
		// (get) Token: 0x06004585 RID: 17797 RVA: 0x00269064 File Offset: 0x00267264
		public override long Length
		{
			get
			{
				return this.innerStream.Length;
			}
		}

		// Token: 0x170011EB RID: 4587
		// (get) Token: 0x06004586 RID: 17798 RVA: 0x00269071 File Offset: 0x00267271
		// (set) Token: 0x06004587 RID: 17799 RVA: 0x0026907E File Offset: 0x0026727E
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

		// Token: 0x06004588 RID: 17800 RVA: 0x0026908C File Offset: 0x0026728C
		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.innerStream.Read(buffer, offset, count);
		}

		// Token: 0x06004589 RID: 17801 RVA: 0x0026909C File Offset: 0x0026729C
		public override void Write(byte[] buffer, int offset, int count)
		{
			if (Validation.CanWrite(this.fileName, false))
			{
				this.innerStream.Write(buffer, offset, count);
				return;
			}
			DebugConsole.ThrowError("Cannot write to file \"" + this.fileName + "\": modifying the files in this folder/with this extension is not allowed.", null, null, false, false);
		}

		// Token: 0x0600458A RID: 17802 RVA: 0x002690D9 File Offset: 0x002672D9
		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.innerStream.Seek(offset, origin);
		}

		// Token: 0x0600458B RID: 17803 RVA: 0x002690E8 File Offset: 0x002672E8
		public override void SetLength(long value)
		{
			this.innerStream.SetLength(value);
		}

		// Token: 0x0600458C RID: 17804 RVA: 0x002690F6 File Offset: 0x002672F6
		public override void Flush()
		{
			this.innerStream.Flush();
		}

		// Token: 0x0600458D RID: 17805 RVA: 0x00269103 File Offset: 0x00267303
		protected override void Dispose(bool notCalledByFinalizer)
		{
			if (notCalledByFinalizer)
			{
				this.innerStream.Dispose();
			}
		}

		// Token: 0x04002426 RID: 9254
		private readonly FileStream innerStream;

		// Token: 0x04002427 RID: 9255
		private readonly string fileName;
	}
}
