using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x02000512 RID: 1298
	public readonly struct PendingLog : IEquatable<PendingLog>
	{
		// Token: 0x060053F8 RID: 21496 RVA: 0x002CDEE3 File Offset: 0x002CC0E3
		public PendingLog(string Message, Color? Color, ServerLog.MessageType MessageType)
		{
			this.Message = Message;
			this.Color = Color;
			this.MessageType = MessageType;
		}

		// Token: 0x17001505 RID: 5381
		// (get) Token: 0x060053F9 RID: 21497 RVA: 0x002CDEFA File Offset: 0x002CC0FA
		// (set) Token: 0x060053FA RID: 21498 RVA: 0x002CDF02 File Offset: 0x002CC102
		public string Message { get; set; }

		// Token: 0x17001506 RID: 5382
		// (get) Token: 0x060053FB RID: 21499 RVA: 0x002CDF0B File Offset: 0x002CC10B
		// (set) Token: 0x060053FC RID: 21500 RVA: 0x002CDF13 File Offset: 0x002CC113
		public Color? Color { get; set; }

		// Token: 0x17001507 RID: 5383
		// (get) Token: 0x060053FD RID: 21501 RVA: 0x002CDF1C File Offset: 0x002CC11C
		// (set) Token: 0x060053FE RID: 21502 RVA: 0x002CDF24 File Offset: 0x002CC124
		public ServerLog.MessageType MessageType { get; set; }

		// Token: 0x060053FF RID: 21503 RVA: 0x002CDF30 File Offset: 0x002CC130
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("PendingLog");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06005400 RID: 21504 RVA: 0x002CDF7C File Offset: 0x002CC17C
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Message = ");
			builder.Append(this.Message);
			builder.Append(", Color = ");
			builder.Append(this.Color.ToString());
			builder.Append(", MessageType = ");
			builder.Append(this.MessageType.ToString());
			return true;
		}

		// Token: 0x06005401 RID: 21505 RVA: 0x002CDFF1 File Offset: 0x002CC1F1
		[CompilerGenerated]
		public static bool operator !=(PendingLog left, PendingLog right)
		{
			return !(left == right);
		}

		// Token: 0x06005402 RID: 21506 RVA: 0x002CDFFD File Offset: 0x002CC1FD
		[CompilerGenerated]
		public static bool operator ==(PendingLog left, PendingLog right)
		{
			return left.Equals(right);
		}

		// Token: 0x06005403 RID: 21507 RVA: 0x002CE007 File Offset: 0x002CC207
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<string>.Default.GetHashCode(this.<Message>k__BackingField) * -1521134295 + EqualityComparer<Microsoft.Xna.Framework.Color?>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<ServerLog.MessageType>.Default.GetHashCode(this.<MessageType>k__BackingField);
		}

		// Token: 0x06005404 RID: 21508 RVA: 0x002CE047 File Offset: 0x002CC247
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is PendingLog && this.Equals((PendingLog)obj);
		}

		// Token: 0x06005405 RID: 21509 RVA: 0x002CE060 File Offset: 0x002CC260
		[CompilerGenerated]
		public bool Equals(PendingLog other)
		{
			return EqualityComparer<string>.Default.Equals(this.<Message>k__BackingField, other.<Message>k__BackingField) && EqualityComparer<Microsoft.Xna.Framework.Color?>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<ServerLog.MessageType>.Default.Equals(this.<MessageType>k__BackingField, other.<MessageType>k__BackingField);
		}

		// Token: 0x06005406 RID: 21510 RVA: 0x002CE0B5 File Offset: 0x002CC2B5
		[CompilerGenerated]
		public void Deconstruct(out string Message, out Color? Color, out ServerLog.MessageType MessageType)
		{
			Message = this.Message;
			Color = this.Color;
			MessageType = this.MessageType;
		}
	}
}
