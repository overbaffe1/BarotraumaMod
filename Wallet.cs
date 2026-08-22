using System;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000066 RID: 102
	internal class Wallet
	{
		// Token: 0x170003D8 RID: 984
		// (get) Token: 0x06000E8F RID: 3727 RVA: 0x0008A750 File Offset: 0x00088950
		public bool IsOwnWallet
		{
			get
			{
				GameSession gameSession = GameMain.GameSession;
				CampaignMode campaignMode = (gameSession != null) ? gameSession.Campaign : null;
				bool result;
				if (campaignMode != null)
				{
					SinglePlayerCampaign spCampaign = campaignMode as SinglePlayerCampaign;
					if (spCampaign == null)
					{
						MultiPlayerCampaign mpCampaign = campaignMode as MultiPlayerCampaign;
						result = (mpCampaign != null && this == mpCampaign.PersonalWallet);
					}
					else
					{
						result = (this == spCampaign.Bank);
					}
				}
				else
				{
					result = false;
				}
				return result;
			}
		}

		// Token: 0x170003D9 RID: 985
		// (get) Token: 0x06000E90 RID: 3728 RVA: 0x0008A7A7 File Offset: 0x000889A7
		// (set) Token: 0x06000E91 RID: 3729 RVA: 0x0008A7AF File Offset: 0x000889AF
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

		// Token: 0x170003DA RID: 986
		// (get) Token: 0x06000E92 RID: 3730 RVA: 0x0008A7BD File Offset: 0x000889BD
		// (set) Token: 0x06000E93 RID: 3731 RVA: 0x0008A7C8 File Offset: 0x000889C8
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

		// Token: 0x06000E94 RID: 3732 RVA: 0x0008A80B File Offset: 0x00088A0B
		public Wallet(Option<Character> owner)
		{
			this.Owner = owner;
		}

		// Token: 0x06000E95 RID: 3733 RVA: 0x0008A81A File Offset: 0x00088A1A
		public Wallet(Option<Character> owner, XElement element) : this(owner)
		{
			this.balance = Wallet.ClampBalance(element.GetAttributeInt("balance", 0));
			this.rewardDistribution = Wallet.ClampBalance(element.GetAttributeInt("rewarddistribution", 0));
		}

		// Token: 0x06000E96 RID: 3734 RVA: 0x0008A854 File Offset: 0x00088A54
		public XElement Save()
		{
			return new XElement("Wallet", new object[]
			{
				new XAttribute("balance", this.Balance),
				new XAttribute("rewarddistribution", this.RewardDistribution)
			});
		}

		// Token: 0x06000E97 RID: 3735 RVA: 0x0008A8B2 File Offset: 0x00088AB2
		public bool TryDeduct(int price)
		{
			if (!this.CanAfford(price))
			{
				return false;
			}
			this.Deduct(price);
			return true;
		}

		// Token: 0x06000E98 RID: 3736 RVA: 0x0008A8C7 File Offset: 0x00088AC7
		public bool CanAfford(int price)
		{
			return this.Balance >= price;
		}

		// Token: 0x06000E99 RID: 3737 RVA: 0x0008A8D5 File Offset: 0x00088AD5
		public void Refund(int price)
		{
			this.Give(price);
		}

		// Token: 0x06000E9A RID: 3738 RVA: 0x0008A8DE File Offset: 0x00088ADE
		public void Give(int amount)
		{
			this.Balance += amount;
			this.SettingsChanged(Option<int>.Some(amount), Option<int>.None());
		}

		// Token: 0x06000E9B RID: 3739 RVA: 0x0008A8FF File Offset: 0x00088AFF
		public void Deduct(int price)
		{
			this.Balance -= price;
			this.SettingsChanged(Option<int>.Some(-price), Option<int>.None());
		}

		// Token: 0x06000E9C RID: 3740 RVA: 0x0008A924 File Offset: 0x00088B24
		public void SetRewardDistribution(int value)
		{
			int oldValue = this.RewardDistribution;
			this.RewardDistribution = value;
			this.SettingsChanged(Option<int>.None(), Option<int>.Some(this.RewardDistribution - oldValue));
		}

		// Token: 0x06000E9D RID: 3741 RVA: 0x0008A958 File Offset: 0x00088B58
		public WalletInfo CreateWalletInfo()
		{
			return new WalletInfo
			{
				Balance = this.Balance,
				RewardDistribution = this.RewardDistribution
			};
		}

		// Token: 0x06000E9E RID: 3742 RVA: 0x0008A988 File Offset: 0x00088B88
		public string GetOwnerLogName()
		{
			Character character;
			if (!this.Owner.TryUnwrap(out character))
			{
				return "the bank";
			}
			return character.Name;
		}

		// Token: 0x06000E9F RID: 3743 RVA: 0x0008A9B0 File Offset: 0x00088BB0
		private void SettingsChanged(Option<int> balanceChanged, Option<int> rewardChanged)
		{
			Character character;
			if (this.Owner.TryUnwrap(out character) && !character.IsPlayer)
			{
				return;
			}
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			WalletChangedData data = new WalletChangedData
			{
				BalanceChanged = balanceChanged,
				RewardDistributionChanged = rewardChanged
			};
			if (campaign != null)
			{
				campaign.OnMoneyChanged.Invoke(new WalletChangedEvent(this, data, this.CreateWalletInfo()));
			}
		}

		// Token: 0x06000EA0 RID: 3744 RVA: 0x0008AA1C File Offset: 0x00088C1C
		private static int ClampBalance(int value)
		{
			return Math.Clamp(value, 0, 1073741823);
		}

		// Token: 0x06000EA1 RID: 3745 RVA: 0x0008AA2A File Offset: 0x00088C2A
		private static int ClampRewardDistribution(int value)
		{
			return Math.Clamp(value, 0, 100);
		}

		// Token: 0x04000772 RID: 1906
		public static readonly Wallet Invalid = new InvalidWallet();

		// Token: 0x04000773 RID: 1907
		public const string LowerCaseSaveElementName = "wallet";

		// Token: 0x04000774 RID: 1908
		private const string AttributeNameBalance = "balance";

		// Token: 0x04000775 RID: 1909
		private const string AttributeNameRewardDistribution = "rewarddistribution";

		// Token: 0x04000776 RID: 1910
		private const string SaveElementName = "Wallet";

		// Token: 0x04000777 RID: 1911
		public readonly Option<Character> Owner;

		// Token: 0x04000778 RID: 1912
		private int balance;

		// Token: 0x04000779 RID: 1913
		private int rewardDistribution;
	}
}
