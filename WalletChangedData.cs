using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002CA RID: 714
	[NetworkSerialize(66)]
	internal struct WalletChangedData : INetSerializableStruct
	{
		// Token: 0x06003CAF RID: 15535 RVA: 0x0022BCE8 File Offset: 0x00229EE8
		public readonly WalletChangedData MergeInto(WalletChangedData other)
		{
			other.BalanceChanged = WalletChangedData.<MergeInto>g__AddOptionalInt|2_0(other.BalanceChanged, this.BalanceChanged);
			other.RewardDistributionChanged = WalletChangedData.<MergeInto>g__AddOptionalInt|2_0(other.RewardDistributionChanged, this.RewardDistributionChanged);
			other.BalanceChanged = WalletChangedData.<MergeInto>g__TurnToNoneIfZero|2_1(other.BalanceChanged);
			other.RewardDistributionChanged = WalletChangedData.<MergeInto>g__TurnToNoneIfZero|2_1(other.RewardDistributionChanged);
			return other;
		}

		// Token: 0x06003CB0 RID: 15536 RVA: 0x0022BD4C File Offset: 0x00229F4C
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

		// Token: 0x06003CB1 RID: 15537 RVA: 0x0022BD9B File Offset: 0x00229F9B
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

		// Token: 0x04001F4D RID: 8013
		public Option<int> RewardDistributionChanged;

		// Token: 0x04001F4E RID: 8014
		public Option<int> BalanceChanged;
	}
}
