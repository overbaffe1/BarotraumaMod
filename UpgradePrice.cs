using System;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000380 RID: 896
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct UpgradePrice
	{
		// Token: 0x06004415 RID: 17429 RVA: 0x002564BC File Offset: 0x002546BC
		public UpgradePrice(UpgradePrefab prefab, ContentXElement element)
		{
			this.IncreaseLow = UpgradePrefab.ParsePercentage(element.GetAttributeString("increaselow", string.Empty), "IncreaseLow".ToIdentifier(), element, prefab.SuppressWarnings);
			this.IncreaseHigh = UpgradePrefab.ParsePercentage(element.GetAttributeString("increasehigh", string.Empty), "IncreaseHigh".ToIdentifier(), element, prefab.SuppressWarnings);
			this.BasePrice = element.GetAttributeInt("baseprice", -1);
			if (this.BasePrice == -1 && !prefab.SuppressWarnings)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(50, 1);
				defaultInterpolatedStringHandler.AppendLiteral("Price attribute \"baseprice\" is not defined for ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier?>((prefab != null) ? new Identifier?(prefab.Identifier) : null);
				defaultInterpolatedStringHandler.AppendLiteral(".\n ");
				DebugConsole.AddWarning(defaultInterpolatedStringHandler.ToStringAndClear() + "The value has been assumed to be '1000'.", prefab.ContentPackage);
				this.BasePrice = 1000;
			}
		}

		// Token: 0x06004416 RID: 17430 RVA: 0x002565B8 File Offset: 0x002547B8
		public int GetBuyPrice(UpgradePrefab prefab, int level, [Nullable(2)] Location location = null, [Nullable(new byte[]
		{
			2,
			1
		})] ImmutableHashSet<Character> characterList = null)
		{
			float price = (float)this.BasePrice;
			int maxLevel = prefab.MaxLevel;
			float lerpAmount = (maxLevel == 0) ? ((float)level) : ((float)level / (float)maxLevel);
			float priceMultiplier = MathHelper.Lerp((float)this.IncreaseLow, (float)this.IncreaseHigh, lerpAmount);
			price += price * (priceMultiplier / 100f);
			int? num = (location != null) ? new int?(location.GetAdjustedMechanicalCost((int)price)) : null;
			price = ((num != null) ? ((float)num.GetValueOrDefault()) : price);
			GameSession gameSession = GameMain.GameSession;
			CampaignMode campaign = (gameSession != null) ? gameSession.Campaign : null;
			if (campaign != null)
			{
				price *= campaign.Settings.ShipyardPriceMultiplier;
			}
			if (characterList == null)
			{
				characterList = GameSession.GetSessionCrewCharacters(CharacterType.Both);
			}
			if (characterList.Any<Character>())
			{
				Faction faction = (location != null) ? location.Faction : null;
				if (faction != null && Faction.GetPlayerAffiliationStatus(faction) == FactionAffiliation.Positive)
				{
					price *= 1f - characterList.Max((Character c) => c.GetStatValue(StatTypes.ShipyardBuyMultiplierAffiliated, true));
				}
				price *= 1f - characterList.Max((Character c) => c.GetStatValue(StatTypes.ShipyardBuyMultiplier, true));
			}
			return (int)price;
		}

		// Token: 0x040023B3 RID: 9139
		public readonly int BasePrice;

		// Token: 0x040023B4 RID: 9140
		public readonly int IncreaseLow;

		// Token: 0x040023B5 RID: 9141
		public readonly int IncreaseHigh;
	}
}
