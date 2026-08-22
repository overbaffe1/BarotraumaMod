using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020001DD RID: 477
	[NetworkSerialize(66)]
	internal struct WalletChangedData : INetSerializableStruct
	{
		// Token: 0x060022AD RID: 8877 RVA: 0x000E8C88 File Offset: 0x000E6E88
		public readonly WalletChangedData MergeInto(WalletChangedData other)
		{
			other.BalanceChanged = WalletChangedData.<MergeInto>g__AddOptionalInt|2_0(other.BalanceChanged, this.BalanceChanged);
			other.RewardDistributionChanged = WalletChangedData.<MergeInto>g__AddOptionalInt|2_0(other.RewardDistributionChanged, this.RewardDistributionChanged);
			other.BalanceChanged = WalletChangedData.<MergeInto>g__TurnToNoneIfZero|2_1(other.BalanceChanged);
			other.RewardDistributionChanged = WalletChangedData.<MergeInto>g__TurnToNoneIfZero|2_1(other.RewardDistributionChanged);
			return other;
		}

		// Token: 0x060022AE RID: 8878 RVA: 0x000E8CEC File Offset: 0x000E6EEC
		[CompilerGenerated]
		internal static Option<int> <MergeInto>g__AddOptionalInt|2_0(Option<int> a, Option<int> b)
		{
			int value;
			bool hasValue = a.TryUnwrap(out value);
			int value2;
			bool hasValue2 = b.TryUnwrap(out value2);
			if (!hasValue)
			{
				if (!hasValue2)
				{
					Option.UnspecifiedNone none = Option.None;
					return none;
				}
				return Option.Some<int>(value2);
			}
			else
			{
				if (!hasValue2)
				{
					return Option.Some<int>(value);
				}
				return Option.Some<int>(value + value2);
			}
		}

		// Token: 0x060022AF RID: 8879 RVA: 0x000E8D3B File Offset: 0x000E6F3B
		[CompilerGenerated]
		internal static Option<int> <MergeInto>g__TurnToNoneIfZero|2_1(Option<int> option)
		{
			return option.Bind<int>(delegate(int i)
			{
				if (i != 0)
				{
					return Option.Some<int>(i);
				}
				Option.UnspecifiedNone none = Option.None;
				return none;
			});
		}

		// Token: 0x040010B1 RID: 4273
		public Option<int> RewardDistributionChanged;

		// Token: 0x040010B2 RID: 4274
		public Option<int> BalanceChanged;
	}
}
