using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000258 RID: 600
	internal class PriceInfo
	{
		// Token: 0x17000CCE RID: 3278
		// (get) Token: 0x06002B21 RID: 11041 RVA: 0x0011BD8E File Offset: 0x00119F8E
		public int Price { get; }

		// Token: 0x17000CCF RID: 3279
		// (get) Token: 0x06002B22 RID: 11042 RVA: 0x0011BD96 File Offset: 0x00119F96
		public bool CanBeBought { get; }

		// Token: 0x17000CD0 RID: 3280
		// (get) Token: 0x06002B23 RID: 11043 RVA: 0x0011BD9E File Offset: 0x00119F9E
		public int MinAvailableAmount { get; }

		// Token: 0x17000CD1 RID: 3281
		// (get) Token: 0x06002B24 RID: 11044 RVA: 0x0011BDA6 File Offset: 0x00119FA6
		public int MaxAvailableAmount { get; }

		// Token: 0x17000CD2 RID: 3282
		// (get) Token: 0x06002B25 RID: 11045 RVA: 0x0011BDAE File Offset: 0x00119FAE
		public bool CanBeSpecial { get; }

		// Token: 0x17000CD3 RID: 3283
		// (get) Token: 0x06002B26 RID: 11046 RVA: 0x0011BDB6 File Offset: 0x00119FB6
		public int MinLevelDifficulty { get; }

		// Token: 0x17000CD4 RID: 3284
		// (get) Token: 0x06002B27 RID: 11047 RVA: 0x0011BDBE File Offset: 0x00119FBE
		public float BuyingPriceMultiplier { get; } = 1f;

		// Token: 0x17000CD5 RID: 3285
		// (get) Token: 0x06002B28 RID: 11048 RVA: 0x0011BDC6 File Offset: 0x00119FC6
		public bool DisplayNonEmpty { get; }

		// Token: 0x17000CD6 RID: 3286
		// (get) Token: 0x06002B29 RID: 11049 RVA: 0x0011BDCE File Offset: 0x00119FCE
		public Identifier StoreIdentifier { get; }

		// Token: 0x17000CD7 RID: 3287
		// (get) Token: 0x06002B2A RID: 11050 RVA: 0x0011BDD6 File Offset: 0x00119FD6
		public bool RequiresUnlock { get; }

		// Token: 0x17000CD8 RID: 3288
		// (get) Token: 0x06002B2B RID: 11051 RVA: 0x0011BDDE File Offset: 0x00119FDE
		// (set) Token: 0x06002B2C RID: 11052 RVA: 0x0011BDE6 File Offset: 0x00119FE6
		public Identifier RequiredFaction { get; private set; }

		// Token: 0x17000CD9 RID: 3289
		// (get) Token: 0x06002B2D RID: 11053 RVA: 0x0011BDEF File Offset: 0x00119FEF
		public IReadOnlyDictionary<Identifier, float> MinReputation
		{
			get
			{
				return this.minReputation;
			}
		}

		// Token: 0x06002B2E RID: 11054 RVA: 0x0011BDF8 File Offset: 0x00119FF8
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

		// Token: 0x06002B2F RID: 11055 RVA: 0x0011BEB8 File Offset: 0x0011A0B8
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

		// Token: 0x06002B30 RID: 11056 RVA: 0x0011BF48 File Offset: 0x0011A148
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

		// Token: 0x06002B31 RID: 11057 RVA: 0x0011BFD4 File Offset: 0x0011A1D4
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

		// Token: 0x06002B32 RID: 11058 RVA: 0x0011C1E0 File Offset: 0x0011A3E0
		private static int GetMinAmount(XElement element, int defaultValue)
		{
			if (element == null)
			{
				return defaultValue;
			}
			return element.GetAttributeInt("minamount", element.GetAttributeInt("minavailable", defaultValue));
		}

		// Token: 0x06002B33 RID: 11059 RVA: 0x0011C1FE File Offset: 0x0011A3FE
		private static int GetMaxAmount(XElement element, int defaultValue)
		{
			if (element == null)
			{
				return defaultValue;
			}
			return element.GetAttributeInt("maxamount", element.GetAttributeInt("maxavailable", defaultValue));
		}

		// Token: 0x06002B34 RID: 11060 RVA: 0x0011C21C File Offset: 0x0011A41C
		public static bool HasMinAmountDefined(XElement element)
		{
			return element != null && (element.GetAttribute("minamount", StringComparison.OrdinalIgnoreCase) != null || element.GetAttribute("minavailable", StringComparison.OrdinalIgnoreCase) != null);
		}

		// Token: 0x06002B35 RID: 11061 RVA: 0x0011C242 File Offset: 0x0011A442
		public static bool HasMaxAmountDefined(XElement element)
		{
			return element != null && (element.GetAttribute("maxamount", StringComparison.OrdinalIgnoreCase) != null || element.GetAttribute("maxavailable", StringComparison.OrdinalIgnoreCase) != null);
		}

		// Token: 0x06002B36 RID: 11062 RVA: 0x0011C268 File Offset: 0x0011A468
		public static bool HasSoldDefined(XElement element)
		{
			return element != null && element.GetAttribute("sold", StringComparison.OrdinalIgnoreCase) != null;
		}

		// Token: 0x06002B37 RID: 11063 RVA: 0x0011C27E File Offset: 0x0011A47E
		public static string GetMinAmountString(XElement element)
		{
			if (element == null)
			{
				return null;
			}
			return element.GetAttributeString("minamount", null) ?? element.GetAttributeString("minavailable", null);
		}

		// Token: 0x06002B38 RID: 11064 RVA: 0x0011C2A1 File Offset: 0x0011A4A1
		public static string GetMaxAmountString(XElement element)
		{
			if (element == null)
			{
				return null;
			}
			return element.GetAttributeString("maxamount", null) ?? element.GetAttributeString("maxavailable", null);
		}

		// Token: 0x06002B39 RID: 11065 RVA: 0x0011C2C4 File Offset: 0x0011A4C4
		public static bool GetSold(XElement element, bool defaultValue = true)
		{
			if (element == null)
			{
				return defaultValue;
			}
			return element.GetAttributeBool("sold", defaultValue);
		}

		// Token: 0x06002B3A RID: 11066 RVA: 0x0011C2D7 File Offset: 0x0011A4D7
		public static int GetMinLevelDifficulty(XElement element, int defaultValue = 0)
		{
			if (element == null)
			{
				return defaultValue;
			}
			return element.GetAttributeInt("minleveldifficulty", defaultValue);
		}

		// Token: 0x06002B3B RID: 11067 RVA: 0x0011C2EA File Offset: 0x0011A4EA
		public static string GetStoreIdentifier(XElement element, string defaultValue = "unknown")
		{
			return ((element != null) ? element.GetAttributeString("storeidentifier", defaultValue) : null) ?? defaultValue;
		}

		// Token: 0x04001528 RID: 5416
		private const int DefaultMinAmount = 1;

		// Token: 0x04001529 RID: 5417
		private const int DefaultMaxAmount = 5;

		// Token: 0x0400152B RID: 5419
		private readonly Dictionary<Identifier, float> minReputation = new Dictionary<Identifier, float>();
	}
}
