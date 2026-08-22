using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Barotrauma.Networking
{
	// Token: 0x0200049E RID: 1182
	internal sealed class MessageFragmenter
	{
		// Token: 0x06004E6E RID: 20078 RVA: 0x002AD154 File Offset: 0x002AB354
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

		// Token: 0x040029B8 RID: 10680
		private ushort nextFragmentedMessageId;

		// Token: 0x040029B9 RID: 10681
		private readonly List<MessageFragment> fragments = new List<MessageFragment>();
	}
}
