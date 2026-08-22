using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma.LuaCs
{
	// Token: 0x020003FF RID: 1023
	public readonly struct PendingLog : IEquatable<PendingLog>
	{
		// Token: 0x06003ADC RID: 15068 RVA: 0x0018974B File Offset: 0x0018794B
		public PendingLog(string Message, Color? Color, ServerLog.MessageType MessageType)
		{
			this.Message = Message;
			this.Color = Color;
			this.MessageType = MessageType;
		}

		// Token: 0x17000FBE RID: 4030
		// (get) Token: 0x06003ADD RID: 15069 RVA: 0x00189762 File Offset: 0x00187962
		// (set) Token: 0x06003ADE RID: 15070 RVA: 0x0018976A File Offset: 0x0018796A
		public string Message { get; set; }

		// Token: 0x17000FBF RID: 4031
		// (get) Token: 0x06003ADF RID: 15071 RVA: 0x00189773 File Offset: 0x00187973
		// (set) Token: 0x06003AE0 RID: 15072 RVA: 0x0018977B File Offset: 0x0018797B
		public Color? Color { get; set; }

		// Token: 0x17000FC0 RID: 4032
		// (get) Token: 0x06003AE1 RID: 15073 RVA: 0x00189784 File Offset: 0x00187984
		// (set) Token: 0x06003AE2 RID: 15074 RVA: 0x0018978C File Offset: 0x0018798C
		public ServerLog.MessageType MessageType { get; set; }

		// Token: 0x06003AE3 RID: 15075 RVA: 0x00189798 File Offset: 0x00187998
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

		// Token: 0x06003AE4 RID: 15076 RVA: 0x001897E4 File Offset: 0x001879E4
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

		// Token: 0x06003AE5 RID: 15077 RVA: 0x00189859 File Offset: 0x00187A59
		[CompilerGenerated]
		public static bool operator !=(PendingLog left, PendingLog right)
		{
			return !(left == right);
		}

		// Token: 0x06003AE6 RID: 15078 RVA: 0x00189865 File Offset: 0x00187A65
		[CompilerGenerated]
		public static bool operator ==(PendingLog left, PendingLog right)
		{
			return left.Equals(right);
		}

		// Token: 0x06003AE7 RID: 15079 RVA: 0x0018986F File Offset: 0x00187A6F
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return (EqualityComparer<string>.Default.GetHashCode(this.<Message>k__BackingField) * -1521134295 + EqualityComparer<Microsoft.Xna.Framework.Color?>.Default.GetHashCode(this.<Color>k__BackingField)) * -1521134295 + EqualityComparer<ServerLog.MessageType>.Default.GetHashCode(this.<MessageType>k__BackingField);
		}

		// Token: 0x06003AE8 RID: 15080 RVA: 0x001898AF File Offset: 0x00187AAF
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is PendingLog && this.Equals((PendingLog)obj);
		}

		// Token: 0x06003AE9 RID: 15081 RVA: 0x001898C8 File Offset: 0x00187AC8
		[CompilerGenerated]
		public bool Equals(PendingLog other)
		{
			return EqualityComparer<string>.Default.Equals(this.<Message>k__BackingField, other.<Message>k__BackingField) && EqualityComparer<Microsoft.Xna.Framework.Color?>.Default.Equals(this.<Color>k__BackingField, other.<Color>k__BackingField) && EqualityComparer<ServerLog.MessageType>.Default.Equals(this.<MessageType>k__BackingField, other.<MessageType>k__BackingField);
		}

		// Token: 0x06003AEA RID: 15082 RVA: 0x0018991D File Offset: 0x00187B1D
		[CompilerGenerated]
		public void Deconstruct(out string Message, out Color? Color, out ServerLog.MessageType MessageType)
		{
			Message = this.Message;
			Color = this.Color;
			MessageType = this.MessageType;
		}
	}
}
