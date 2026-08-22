using System;
using System.Xml.Linq;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000032 RID: 50
	internal class CharacterCampaignData
	{
		// Token: 0x170001A1 RID: 417
		// (get) Token: 0x06000634 RID: 1588 RVA: 0x0003A5C0 File Offset: 0x000387C0
		public bool HasItemData
		{
			get
			{
				return this.itemData != null;
			}
		}

		// Token: 0x06000635 RID: 1589 RVA: 0x0003A5CC File Offset: 0x000387CC
		public CharacterCampaignData(Client client)
		{
			this.Name = client.Name;
			this.ClientAddress = client.Connection.Endpoint.Address;
			this.AccountId = client.AccountId;
			this.CharacterInfo = client.CharacterInfo;
			this.healthData = new XElement("health");
			Character character2;
			if ((character2 = client.Character) == null)
			{
				CharacterInfo characterInfo = this.CharacterInfo;
				character2 = ((characterInfo != null) ? characterInfo.Character : null);
			}
			Character character = character2;
			if (character != null)
			{
				CharacterHealth characterHealth = character.CharacterHealth;
				if (characterHealth != null)
				{
					characterHealth.Save(this.healthData);
				}
			}
			if (((character != null) ? character.Inventory : null) != null)
			{
				this.itemData = new XElement("inventory");
				Character.SaveInventory(character.Inventory, this.itemData);
			}
			this.OrderData = new XElement("orders");
			if (this.CharacterInfo != null)
			{
				CharacterInfo.SaveOrderData(this.CharacterInfo, this.OrderData);
			}
			XElement walletSave = (character != null) ? character.Wallet.Save() : null;
			if (walletSave != null)
			{
				this.WalletData = walletSave;
			}
		}

		// Token: 0x06000636 RID: 1590 RVA: 0x0003A6E4 File Offset: 0x000388E4
		public void Refresh(Character character, bool refreshHealthData)
		{
			if (refreshHealthData)
			{
				this.healthData = new XElement("health");
				character.CharacterHealth.Save(this.healthData);
			}
			if (character.Inventory != null)
			{
				this.itemData = new XElement("inventory");
				Character.SaveInventory(character.Inventory, this.itemData);
			}
			this.OrderData = new XElement("orders");
			CharacterInfo.SaveOrderData(character.Info, this.OrderData);
			this.WalletData = character.Wallet.Save();
		}

		// Token: 0x06000637 RID: 1591 RVA: 0x0003A780 File Offset: 0x00038980
		public CharacterCampaignData(XElement element)
		{
			this.Name = element.GetAttributeString("name", "Unnamed");
			string text;
			if ((text = element.GetAttributeString("address", null)) == null)
			{
				text = (element.GetAttributeString("endpoint", null) ?? element.GetAttributeString("ip", ""));
			}
			string clientEndPointStr = text;
			this.ClientAddress = Address.Parse(clientEndPointStr).Fallback(new UnknownAddress());
			string accountIdStr = element.GetAttributeString("accountid", null) ?? element.GetAttributeString("steamid", "");
			this.AccountId = Barotrauma.Networking.AccountId.Parse(accountIdStr);
			this.ChosenNewBotViaShuttle = element.GetAttributeBool("waitingforshuttle", false);
			foreach (XElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "character") && !(a == "characterinfo"))
				{
					if (!(a == "inventory"))
					{
						if (!(a == "health"))
						{
							if (!(a == "orders"))
							{
								if (a == "wallet")
								{
									this.WalletData = subElement;
								}
							}
							else
							{
								this.OrderData = subElement;
							}
						}
						else
						{
							this.healthData = subElement;
						}
					}
					else
					{
						this.itemData = subElement;
					}
				}
				else
				{
					this.CharacterInfo = new CharacterInfo(new ContentXElement(null, subElement), default(Identifier));
				}
			}
		}

		// Token: 0x06000638 RID: 1592 RVA: 0x0003A920 File Offset: 0x00038B20
		public bool MatchesClient(Client client)
		{
			AccountId accountId;
			AccountId clientId;
			if (this.AccountId.TryUnwrap(out accountId) && client.AccountId.TryUnwrap(out clientId))
			{
				return accountId == clientId;
			}
			return this.ClientAddress == client.Connection.Endpoint.Address;
		}

		// Token: 0x06000639 RID: 1593 RVA: 0x0003A971 File Offset: 0x00038B71
		public bool IsDuplicate(CharacterCampaignData other)
		{
			return this.AccountId == other.AccountId && other.ClientAddress == this.ClientAddress;
		}

		// Token: 0x0600063A RID: 1594 RVA: 0x0003A999 File Offset: 0x00038B99
		public void Reset()
		{
			this.itemData = null;
			this.healthData = null;
			this.WalletData = null;
		}

		// Token: 0x0600063B RID: 1595 RVA: 0x0003A9B0 File Offset: 0x00038BB0
		public void ApplyPermadeath()
		{
			this.Reset();
			this.CharacterInfo.PermanentlyDead = true;
			GameSession gameSession = GameMain.GameSession;
			if (gameSession != null)
			{
				gameSession.IncrementPermadeath(this.AccountId);
			}
			DebugConsole.NewMessage("Permadeath applied on " + this.Name + "'s CharacterCampaignData.CharacterInfo.", null, false);
		}

		// Token: 0x0600063C RID: 1596 RVA: 0x0003AA0C File Offset: 0x00038C0C
		public void SpawnInventoryItems(Character character, Inventory inventory)
		{
			if (character == null)
			{
				throw new InvalidOperationException("Failed to spawn inventory items. Character was null.");
			}
			if (this.itemData == null)
			{
				throw new InvalidOperationException("Failed to spawn inventory items for the character \"" + character.Name + "\". No saved inventory data.");
			}
			character.SpawnInventoryItems(inventory, this.itemData.FromPackage(null));
		}

		// Token: 0x0600063D RID: 1597 RVA: 0x0003AA5D File Offset: 0x00038C5D
		public void ApplyHealthData(Character character, Func<AfflictionPrefab, bool> afflictionPredicate = null)
		{
			CharacterInfo.ApplyHealthData(character, this.healthData, afflictionPredicate);
		}

		// Token: 0x0600063E RID: 1598 RVA: 0x0003AA6C File Offset: 0x00038C6C
		public void ApplyOrderData(Character character)
		{
			CharacterInfo.ApplyOrderData(character, this.OrderData);
		}

		// Token: 0x0600063F RID: 1599 RVA: 0x0003AA7A File Offset: 0x00038C7A
		public void ApplyWalletData(Character character)
		{
			character.Wallet = new Wallet(Option.Some<Character>(character), this.WalletData);
		}

		// Token: 0x06000640 RID: 1600 RVA: 0x0003AA94 File Offset: 0x00038C94
		public XElement Save()
		{
			AccountId accountId;
			XElement element = new XElement("CharacterCampaignData", new object[]
			{
				new XAttribute("name", this.Name),
				new XAttribute("address", this.ClientAddress),
				new XAttribute("accountid", this.AccountId.TryUnwrap(out accountId) ? accountId.StringRepresentation : ""),
				new XAttribute("waitingforshuttle", this.ChosenNewBotViaShuttle)
			});
			CharacterInfo characterInfo = this.CharacterInfo;
			if (characterInfo != null)
			{
				characterInfo.Save(element);
			}
			if (this.itemData != null)
			{
				element.Add(this.itemData);
			}
			if (this.healthData != null)
			{
				element.Add(this.healthData);
			}
			if (this.OrderData != null)
			{
				element.Add(this.OrderData);
			}
			if (this.WalletData != null)
			{
				element.Add(this.WalletData);
			}
			return element;
		}

		// Token: 0x170001A2 RID: 418
		// (get) Token: 0x06000641 RID: 1601 RVA: 0x0003AB96 File Offset: 0x00038D96
		// (set) Token: 0x06000642 RID: 1602 RVA: 0x0003AB9E File Offset: 0x00038D9E
		public XElement OrderData { get; private set; }

		// Token: 0x04000313 RID: 787
		public bool HasSpawned;

		// Token: 0x04000314 RID: 788
		public bool ChosenNewBotViaShuttle;

		// Token: 0x04000315 RID: 789
		public readonly CharacterInfo CharacterInfo;

		// Token: 0x04000316 RID: 790
		public readonly string Name;

		// Token: 0x04000317 RID: 791
		public readonly Address ClientAddress;

		// Token: 0x04000318 RID: 792
		public readonly Option<AccountId> AccountId;

		// Token: 0x04000319 RID: 793
		private XElement itemData;

		// Token: 0x0400031A RID: 794
		private XElement healthData;

		// Token: 0x0400031C RID: 796
		public XElement WalletData;
	}
}
