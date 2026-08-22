using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000030 RID: 48
	internal class Wallet
	{
		// Token: 0x060005BF RID: 1471 RVA: 0x00035C5D File Offset: 0x00033E5D
		public void ForceUpdate()
		{
			this.SettingsChanged(Option<int>.Some(0), Option<int>.None());
			this.ShouldForceUpdate = true;
		}

		// Token: 0x060005C0 RID: 1472 RVA: 0x00035C77 File Offset: 0x00033E77
		public bool HasTransactions()
		{
			return this.transactions.Count > 0;
		}

		// Token: 0x060005C1 RID: 1473 RVA: 0x00035C88 File Offset: 0x00033E88
		public NetWalletTransaction DequeueAndMergeTransactions(ushort id)
		{
			Option<ushort> targetCharacterID = (id == 0) ? Option<ushort>.None() : Option<ushort>.Some(id);
			WalletChangedData changedData = new WalletChangedData
			{
				BalanceChanged = Option<int>.None(),
				RewardDistributionChanged = Option<int>.None()
			};
			WalletChangedData transactionOut;
			while (this.transactions.TryDequeue(out transactionOut))
			{
				changedData = changedData.MergeInto(transactionOut);
			}
			return new NetWalletTransaction
			{
				CharacterID = targetCharacterID,
				ChangedData = changedData,
				Info = this.CreateWalletInfo()
			};
		}

		// Token: 0x17000191 RID: 401
		// (get) Token: 0x060005C2 RID: 1474 RVA: 0x00035D09 File Offset: 0x00033F09
		// (set) Token: 0x060005C3 RID: 1475 RVA: 0x00035D11 File Offset: 0x00033F11
		public virtual int Balance
		{
			get
			{
				return this.balance;
			}
			set
			{
				this.balance = Wallet.ClampBalance(value);
			}
		}

		// Token: 0x17000192 RID: 402
		// (get) Token: 0x060005C4 RID: 1476 RVA: 0x00035D1F File Offset: 0x00033F1F
		// (set) Token: 0x060005C5 RID: 1477 RVA: 0x00035D28 File Offset: 0x00033F28
		public virtual int RewardDistribution
		{
			get
			{
				return this.rewardDistribution;
			}
			set
			{
				this.rewardDistribution = Wallet.ClampRewardDistribution(value);
				Character character;
				if (this.Owner.TryUnwrap(out character))
				{
					CharacterInfo info = character.Info;
					if (info != null)
					{
						info.LastRewardDistribution = Option.Some<int>(this.rewardDistribution);
					}
				}
			}
		}

		// Token: 0x060005C6 RID: 1478 RVA: 0x00035D6B File Offset: 0x00033F6B
		public Wallet(Option<Character> owner)
		{
			this.Owner = owner;
		}

		// Token: 0x060005C7 RID: 1479 RVA: 0x00035D85 File Offset: 0x00033F85
		public Wallet(Option<Character> owner, XElement element) : this(owner)
		{
			this.balance = Wallet.ClampBalance(element.GetAttributeInt("balance", 0));
			this.rewardDistribution = Wallet.ClampBalance(element.GetAttributeInt("rewarddistribution", 0));
		}

		// Token: 0x060005C8 RID: 1480 RVA: 0x00035DBC File Offset: 0x00033FBC
		public XElement Save()
		{
			return new XElement("Wallet", new object[]
			{
				new XAttribute("balance", this.Balance),
				new XAttribute("rewarddistribution", this.RewardDistribution)
			});
		}

		// Token: 0x060005C9 RID: 1481 RVA: 0x00035E1A File Offset: 0x0003401A
		public bool TryDeduct(int price)
		{
			if (!this.CanAfford(price))
			{
				return false;
			}
			this.Deduct(price);
			return true;
		}

		// Token: 0x060005CA RID: 1482 RVA: 0x00035E2F File Offset: 0x0003402F
		public bool CanAfford(int price)
		{
			return this.Balance >= price;
		}

		// Token: 0x060005CB RID: 1483 RVA: 0x00035E3D File Offset: 0x0003403D
		public void Refund(int price)
		{
			this.Give(price);
		}

		// Token: 0x060005CC RID: 1484 RVA: 0x00035E46 File Offset: 0x00034046
		public void Give(int amount)
		{
			this.Balance += amount;
			this.SettingsChanged(Option<int>.Some(amount), Option<int>.None());
		}

		// Token: 0x060005CD RID: 1485 RVA: 0x00035E67 File Offset: 0x00034067
		public void Deduct(int price)
		{
			this.Balance -= price;
			this.SettingsChanged(Option<int>.Some(-price), Option<int>.None());
		}

		// Token: 0x060005CE RID: 1486 RVA: 0x00035E8C File Offset: 0x0003408C
		public void SetRewardDistribution(int value)
		{
			int oldValue = this.RewardDistribution;
			this.RewardDistribution = value;
			this.SettingsChanged(Option<int>.None(), Option<int>.Some(this.RewardDistribution - oldValue));
		}

		// Token: 0x060005CF RID: 1487 RVA: 0x00035EC0 File Offset: 0x000340C0
		public WalletInfo CreateWalletInfo()
		{
			return new WalletInfo
			{
				Balance = this.Balance,
				RewardDistribution = this.RewardDistribution
			};
		}

		// Token: 0x060005D0 RID: 1488 RVA: 0x00035EF0 File Offset: 0x000340F0
		public string GetOwnerLogName()
		{
			Character character;
			if (!this.Owner.TryUnwrap(out character))
			{
				return "the bank";
			}
			return character.Name;
		}

		// Token: 0x060005D1 RID: 1489 RVA: 0x00035F18 File Offset: 0x00034118
		private void SettingsChanged(Option<int> balanceChanged, Option<int> rewardChanged)
		{
			this.transactions.Enqueue(new WalletChangedData
			{
				BalanceChanged = balanceChanged,
				RewardDistributionChanged = rewardChanged
			});
		}

		// Token: 0x060005D2 RID: 1490 RVA: 0x00035F49 File Offset: 0x00034149
		private static int ClampBalance(int value)
		{
			return Math.Clamp(value, 0, 1073741823);
		}

		// Token: 0x060005D3 RID: 1491 RVA: 0x00035F57 File Offset: 0x00034157
		private static int ClampRewardDistribution(int value)
		{
			return Math.Clamp(value, 0, 100);
		}

		// Token: 0x040002DE RID: 734
		private readonly Queue<WalletChangedData> transactions = new Queue<WalletChangedData>();

		// Token: 0x040002DF RID: 735
		public bool ShouldForceUpdate;

		// Token: 0x040002E0 RID: 736
		public static readonly Wallet Invalid = new InvalidWallet();

		// Token: 0x040002E1 RID: 737
		public const string LowerCaseSaveElementName = "wallet";

		// Token: 0x040002E2 RID: 738
		private const string AttributeNameBalance = "balance";

		// Token: 0x040002E3 RID: 739
		private const string AttributeNameRewardDistribution = "rewarddistribution";

		// Token: 0x040002E4 RID: 740
		private const string SaveElementName = "Wallet";

		// Token: 0x040002E5 RID: 741
		public readonly Option<Character> Owner;

		// Token: 0x040002E6 RID: 742
		private int balance;

		// Token: 0x040002E7 RID: 743
		private int rewardDistribution;
	}
}
