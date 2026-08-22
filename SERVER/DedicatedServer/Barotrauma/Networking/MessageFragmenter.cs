using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Barotrauma.Networking
{
	// Token: 0x020003A1 RID: 929
	internal sealed class MessageFragmenter
	{
		// Token: 0x06003699 RID: 13977 RVA: 0x00174618 File Offset: 0x00172818
		public ImmutableArray<MessageFragment> FragmentMessage(ReadOnlySpan<byte> bytes)
		{
			ushort msgId = this.nextFragmentedMessageId;
			this.nextFragmentedMessageId += 1;
			int roundedByteCount = bytes.Length;
			roundedByteCount += (1100 - roundedByteCount % 1100) % 1100;
			int fragmentCount = roundedByteCount / 1100;
			this.fragments.Clear();
			this.fragments.EnsureCapacity(fragmentCount);
			for (int i = 0; i < fragmentCount; i++)
			{
				ReadOnlySpan<byte> subset = bytes.Slice(i * 1100);
				if (subset.Length > 1100)
				{
					subset = subset.Slice(0, 1100);
				}
				this.fragments.Add(new MessageFragment(new MessageFragment.Id((ushort)i, (ushort)fragmentCount, msgId), subset.ToArray().ToImmutableArray<byte>()));
			}
			return this.fragments.ToImmutableArray<MessageFragment>();
		}

		// Token: 0x04001BC1 RID: 7105
		private ushort nextFragmentedMessageId;

		// Token: 0x04001BC2 RID: 7106
		private readonly List<MessageFragment> fragments = new List<MessageFragment>();
	}
}
