using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200032F RID: 815
	internal class PriceInfo
	{
		// Token: 0x17001151 RID: 4433
		// (get) Token: 0x06004123 RID: 16675 RVA: 0x0024405E File Offset: 0x0024225E
		public int Price { get; }

		// Token: 0x17001152 RID: 4434
		// (get) Token: 0x06004124 RID: 16676 RVA: 0x00244066 File Offset: 0x00242266
		public bool CanBeBought { get; }

		// Token: 0x17001153 RID: 4435
		// (get) Token: 0x06004125 RID: 16677 RVA: 0x0024406E File Offset: 0x0024226E
		public int MinAvailableAmount { get; }

		// Token: 0x17001154 RID: 4436
		// (get) Token: 0x06004126 RID: 16678 RVA: 0x00244076 File Offset: 0x00242276
		public int MaxAvailableAmount { get; }

		// Token: 0x17001155 RID: 4437
		// (get) Token: 0x06004127 RID: 16679 RVA: 0x0024407E File Offset: 0x0024227E
		public bool CanBeSpecial { get; }

		// Token: 0x17001156 RID: 4438
		// (get) Token: 0x06004128 RID: 16680 RVA: 0x00244086 File Offset: 0x00242286
		public int MinLevelDifficulty { get; }

		// Token: 0x17001157 RID: 4439
		// (get) Token: 0x06004129 RID: 16681 RVA: 0x0024408E File Offset: 0x0024228E
		public float BuyingPriceMultiplier { get; } = 1f;

		// Token: 0x17001158 RID: 4440
		// (get) Token: 0x0600412A RID: 16682 RVA: 0x00244096 File Offset: 0x00242296
		public bool DisplayNonEmpty { get; }

		// Token: 0x17001159 RID: 4441
		// (get) Token: 0x0600412B RID: 16683 RVA: 0x0024409E File Offset: 0x0024229E
		public Identifier StoreIdentifier { get; }

		// Token: 0x1700115A RID: 4442
		// (get) Token: 0x0600412C RID: 16684 RVA: 0x002440A6 File Offset: 0x002422A6
		public bool RequiresUnlock { get; }

		// Token: 0x1700115B RID: 4443
		// (get) Token: 0x0600412D RID: 16685 RVA: 0x002440AE File Offset: 0x002422AE
		// (set) Token: 0x0600412E RID: 16686 RVA: 0x002440B6 File Offset: 0x002422B6
		public Identifier RequiredFaction { get; private set; }

		// Token: 0x1700115C RID: 4444
		// (get) Token: 0x0600412F RID: 16687 RVA: 0x002440BF File Offset: 0x002422BF
		public IReadOnlyDictionary<Identifier, float> MinReputation
		{
			get
			{
				return this.minReputation;
			}
		}

		// Token: 0x06004130 RID: 16688 RVA: 0x002440C8 File Offset: 0x002422C8
		public PriceInfo(XElement element)
		{
			this.Price = element.GetAttributeInt("buyprice", 0);
			this.MinLevelDifficulty = PriceInfo.GetMinLevelDifficulty(element, 0);
			this.BuyingPriceMultiplier = element.GetAttributeFloat("buyingpricemultiplier", 1f);
			this.CanBeBought = 1;
			this.MinAvailableAmount = Math.Min(PriceInfo.GetMinAmount(element, 1), 100);
			int maxAmount = PriceInfo.GetMaxAmount(element, 5);
			this.MaxAvailableAmount = MathHelper.Clamp(maxAmount, this.MinAvailableAmount, 100);
			this.RequiresUnlock = element.GetAttributeBool("requiresunlock", false);
			this.RequiredFaction = element.GetAttributeIdentifier("RequiredFaction", Identifier.Empty);
		}

		// Token: 0x06004131 RID: 16689 RVA: 0x00244188 File Offset: 0x00242388
		public PriceInfo(int price, bool canBeBought, int minAmount = 0, int maxAmount = 0, bool canBeSpecial = true, int minLevelDifficulty = 0, float buyingPriceMultiplier = 1f, bool displayNonEmpty = false, bool requiresUnlock = false, string storeIdentifier = null)
		{
			this.Price = price;
			this.CanBeBought = canBeBought;
			this.MinAvailableAmount = Math.Min(minAmount, 100);
			this.MaxAvailableAmount = Math.Max(Math.Min(maxAmount, 100), minAmount);
			this.BuyingPriceMultiplier = buyingPriceMultiplier;
			this.MinLevelDifficulty = minLevelDifficulty;
			this.CanBeSpecial = canBeSpecial;
			this.DisplayNonEmpty = displayNonEmpty;
			this.StoreIdentifier = new Identifier(storeIdentifier);
			this.RequiresUnlock = requiresUnlock;
		}

		// Token: 0x06004132 RID: 16690 RVA: 0x00244218 File Offset: 0x00242418
		private void LoadReputationRestrictions(XElement priceInfoElement)
		{
			foreach (XElement childElement in priceInfoElement.GetChildElements("reputation", StringComparison.OrdinalIgnoreCase))
			{
				Identifier factionId = childElement.GetAttributeIdentifier("faction", Identifier.Empty);
				float rep = childElement.GetAttributeFloat("min", 0f);
				if (!factionId.IsEmpty && rep > 0f)
				{
					this.minReputation.Add(factionId, rep);
				}
			}
		}

		// Token: 0x06004133 RID: 16691 RVA: 0x002442A4 File Offset: 0x002424A4
		public static List<PriceInfo> CreatePriceInfos(XElement element, out PriceInfo defaultPrice)
		{
			List<PriceInfo> priceInfos = new List<PriceInfo>();
			defaultPrice = null;
			int basePrice = element.GetAttributeInt("baseprice", 0);
			int minAmount = PriceInfo.GetMinAmount(element, 1);
			int maxAmount = PriceInfo.GetMaxAmount(element, 5);
			int minLevelDifficulty = PriceInfo.GetMinLevelDifficulty(element, 0);
			bool canBeSpecial = element.GetAttributeBool("canbespecial", true);
			float buyingPriceMultiplier = element.GetAttributeFloat("buyingpricemultiplier", 1f);
			bool displayNonEmpty = element.GetAttributeBool("displaynonempty", false);
			bool soldByDefault = PriceInfo.GetSold(element, element.GetAttributeBool("soldbydefault", true));
			bool requiresUnlock = element.GetAttributeBool("requiresunlock", false);
			Identifier requiredFactionByDefault = element.GetAttributeIdentifier("RequiredFaction", Identifier.Empty);
			foreach (XElement childElement in element.GetChildElements("price", StringComparison.OrdinalIgnoreCase))
			{
				float priceMultiplier = childElement.GetAttributeFloat("multiplier", 1f);
				bool sold = PriceInfo.GetSold(childElement, soldByDefault);
				int storeMinLevelDifficulty = PriceInfo.GetMinLevelDifficulty(childElement, minLevelDifficulty);
				float storeBuyingMultiplier = childElement.GetAttributeFloat("buyingpricemultiplier", buyingPriceMultiplier);
				string backwardsCompatibleIdentifier = childElement.GetAttributeString("locationtype", "");
				if (!string.IsNullOrEmpty(backwardsCompatibleIdentifier))
				{
					backwardsCompatibleIdentifier = "merchant" + backwardsCompatibleIdentifier;
				}
				string storeIdentifier = PriceInfo.GetStoreIdentifier(childElement, backwardsCompatibleIdentifier);
				PriceInfo priceInfo = new PriceInfo((int)(priceMultiplier * (float)basePrice), sold, sold ? PriceInfo.GetMinAmount(childElement, minAmount) : 0, sold ? PriceInfo.GetMaxAmount(childElement, maxAmount) : 0, canBeSpecial, storeMinLevelDifficulty, storeBuyingMultiplier, displayNonEmpty, requiresUnlock, storeIdentifier)
				{
					RequiredFaction = childElement.GetAttributeIdentifier("RequiredFaction", requiredFactionByDefault)
				};
				priceInfo.LoadReputationRestrictions(childElement);
				priceInfos.Add(priceInfo);
			}
			bool soldElsewhere = soldByDefault && element.GetAttributeBool("soldelsewhere", element.GetAttributeBool("soldeverywhere", false));
			defaultPrice = new PriceInfo(basePrice, soldElsewhere, soldElsewhere ? minAmount : 0, soldElsewhere ? maxAmount : 0, canBeSpecial, minLevelDifficulty, buyingPriceMultiplier, displayNonEmpty, requiresUnlock, null)
			{
				RequiredFaction = requiredFactionByDefault
			};
			defaultPrice.LoadReputationRestrictions(element);
			return priceInfos;
		}

		// Token: 0x06004134 RID: 16692 RVA: 0x002444B0 File Offset: 0x002426B0
		private static int GetMinAmount(XElement element, int defaultValue)
		{
			if (element == null)
			{
				return defaultValue;
			}
			return element.GetAttributeInt("minamount", element.GetAttributeInt("minavailable", defaultValue));
		}

		// Token: 0x06004135 RID: 16693 RVA: 0x002444CE File Offset: 0x002426CE
		private static int GetMaxAmount(XElement element, int defaultValue)
		{
			if (element == null)
			{
				return defaultValue;
			}
			return element.GetAttributeInt("maxamount", element.GetAttributeInt("maxavailable", defaultValue));
		}

		// Token: 0x06004136 RID: 16694 RVA: 0x002444EC File Offset: 0x002426EC
		public static bool HasMinAmountDefined(XElement element)
		{
			return element != null && (element.GetAttribute("minamount", StringComparison.OrdinalIgnoreCase) != null || element.GetAttribute("minavailable", StringComparison.OrdinalIgnoreCase) != null);
		}

		// Token: 0x06004137 RID: 16695 RVA: 0x00244512 File Offset: 0x00242712
		public static bool HasMaxAmountDefined(XElement element)
		{
			return element != null && (element.GetAttribute("maxamount", StringComparison.OrdinalIgnoreCase) != null || element.GetAttribute("maxavailable", StringComparison.OrdinalIgnoreCase) != null);
		}

		// Token: 0x06004138 RID: 16696 RVA: 0x00244538 File Offset: 0x00242738
		public static bool HasSoldDefined(XElement element)
		{
			return element != null && element.GetAttribute("sold", StringComparison.OrdinalIgnoreCase) != null;
		}

		// Token: 0x06004139 RID: 16697 RVA: 0x0024454E File Offset: 0x0024274E
		public static string GetMinAmountString(XElement element)
		{
			if (element == null)
			{
				return null;
			}
			return element.GetAttributeString("minamount", null) ?? element.GetAttributeString("minavailable", null);
		}

		// Token: 0x0600413A RID: 16698 RVA: 0x00244571 File Offset: 0x00242771
		public static string GetMaxAmountString(XElement element)
		{
			if (element == null)
			{
				return null;
			}
			return element.GetAttributeString("maxamount", null) ?? element.GetAttributeString("maxavailable", null);
		}

		// Token: 0x0600413B RID: 16699 RVA: 0x00244594 File Offset: 0x00242794
		public static bool GetSold(XElement element, bool defaultValue = true)
		{
			if (element == null)
			{
				return defaultValue;
			}
			return element.GetAttributeBool("sold", defaultValue);
		}

		// Token: 0x0600413C RID: 16700 RVA: 0x002445A7 File Offset: 0x002427A7
		public static int GetMinLevelDifficulty(XElement element, int defaultValue = 0)
		{
			if (element == null)
			{
				return defaultValue;
			}
			return element.GetAttributeInt("minleveldifficulty", defaultValue);
		}

		// Token: 0x0600413D RID: 16701 RVA: 0x002445BA File Offset: 0x002427BA
		public static string GetStoreIdentifier(XElement element, string defaultValue = "unknown")
		{
			return ((element != null) ? element.GetAttributeString("storeidentifier", defaultValue) : null) ?? defaultValue;
		}

		// Token: 0x040021EB RID: 8683
		private const int DefaultMinAmount = 1;

		// Token: 0x040021EC RID: 8684
		private const int DefaultMaxAmount = 5;

		// Token: 0x040021EE RID: 8686
		private readonly Dictionary<Identifier, float> minReputation = new Dictionary<Identifier, float>();
	}
}
