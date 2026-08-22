using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma.Networking
{
	// Token: 0x0200049C RID: 1180
	internal sealed class MessageDefragmenter
	{
		// Token: 0x06004E5F RID: 20063 RVA: 0x002ACD48 File Offset: 0x002AAF48
		public Option<ImmutableArray<byte>> ProcessIncomingFragment(MessageFragment fragment)
		{
			Option.UnspecifiedNone none;
			if (!this.partialMessages.ContainsKey(fragment.FragmentId.MessageId))
			{
				this.partialMessages[fragment.FragmentId.MessageId] = new MessageFragment[(int)fragment.FragmentId.FragmentCount];
			}
			else if (this.partialMessages[fragment.FragmentId.MessageId].Length != (int)fragment.FragmentId.FragmentCount)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(69, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Got a fragment for message ");
				defaultInterpolatedStringHandler.AppendFormatted<ushort>(fragment.FragmentId.MessageId);
				defaultInterpolatedStringHandler.AppendLiteral(" ");
				defaultInterpolatedStringHandler.AppendLiteral("with a mismatched expected fragment count");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear(), null);
				none = Option.None;
				return none;
			}
			MessageFragment[] fragmentBuffer = this.partialMessages[fragment.FragmentId.MessageId];
			if ((int)fragment.FragmentId.FragmentIndex >= fragmentBuffer.Length)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(101, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Got a fragment for message ");
				defaultInterpolatedStringHandler2.AppendFormatted<ushort>(fragment.FragmentId.MessageId);
				defaultInterpolatedStringHandler2.AppendLiteral(" ");
				defaultInterpolatedStringHandler2.AppendLiteral("with an index greater than or equal to the expected fragment count (");
				defaultInterpolatedStringHandler2.AppendFormatted<ushort>(fragment.FragmentId.FragmentIndex);
				defaultInterpolatedStringHandler2.AppendLiteral(" >= ");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(fragmentBuffer.Length);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear(), null);
				none = Option.None;
				return none;
			}
			fragmentBuffer[(int)fragment.FragmentId.FragmentIndex] = fragment;
			if (fragmentBuffer.All((MessageFragment f) => !f.Data.IsDefault && f.FragmentId.MessageId == fragment.FragmentId.MessageId))
			{
				this.partialMessages.Remove(fragment.FragmentId.MessageId);
				return Option.Some<ImmutableArray<byte>>(fragmentBuffer.SelectMany((MessageFragment f) => f.Data).ToImmutableArray<byte>());
			}
			none = Option.None;
			return none;
		}

		// Token: 0x040029B4 RID: 10676
		private readonly Dictionary<ushort, MessageFragment[]> partialMessages = new Dictionary<ushort, MessageFragment[]>();
	}
}
