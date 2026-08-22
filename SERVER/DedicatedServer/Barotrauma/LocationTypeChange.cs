using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000246 RID: 582
	internal class LocationTypeChange
	{
		// Token: 0x06002958 RID: 10584 RVA: 0x0010CACC File Offset: 0x0010ACCC
		public IReadOnlyList<string> GetMessages(Faction faction)
		{
			if (faction != null && TextManager.ContainsTag(this.messageTag + "." + faction.Prefab.Identifier.ToString()))
			{
				return TextManager.GetAll(this.messageTag + "." + faction.Prefab.Identifier.ToString()).ToImmutableArray<string>();
			}
			if (TextManager.ContainsTag(this.messageTag))
			{
				return TextManager.GetAll(this.messageTag).ToImmutableArray<string>();
			}
			if (this.requireChangeMessages)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(53, 2);
				defaultInterpolatedStringHandler.AppendLiteral("No messages defined for the location type change ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.CurrentType);
				defaultInterpolatedStringHandler.AppendLiteral(" -> ");
				defaultInterpolatedStringHandler.AppendFormatted<Identifier>(this.ChangeToType);
				DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
			}
			return Enumerable.Empty<string>().ToImmutableArray<string>();
		}

		// Token: 0x06002959 RID: 10585 RVA: 0x0010CBC8 File Offset: 0x0010ADC8
		public LocationTypeChange(Identifier currentType, ContentXElement element, bool requireChangeMessages, float defaultProbability = 0f)
		{
			this.CurrentType = currentType;
			this.ChangeToType = element.GetAttributeIdentifier("type", element.GetAttributeIdentifier("to", ""));
			this.RequireDiscovered = element.GetAttributeBool("requirediscovered", false);
			this.DisallowedAdjacentLocations = element.GetAttributeIdentifierArray("disallowedadjacentlocations", Array.Empty<Identifier>(), true).ToImmutableArray<Identifier>();
			this.DisallowedProximity = Math.Max(element.GetAttributeInt("disallowedproximity", 1), 1);
			string key = "requireddurationrange";
			Point zero = Point.Zero;
			this.RequiredDurationRange = element.GetAttributePoint(key, zero);
			this.Probability = element.GetAttributeFloat("probability", defaultProbability);
			this.CooldownAfterChange = Math.Max(element.GetAttributeInt("cooldownafterchange", 0), 0);
			if (element.GetAttribute("requiredlocations") != null)
			{
				this.Requirements.Add(new LocationTypeChange.Requirement(element, this));
			}
			if (element.GetAttribute("requiredduration") != null)
			{
				this.RequiredDurationRange = new Point(element.GetAttributeInt("requiredduration", 0));
			}
			this.requireChangeMessages = requireChangeMessages;
			this.messageTag = element.GetAttributeString("messagetag", "LocationChange." + currentType.ToString() + ".ChangeTo." + this.ChangeToType.ToString());
			foreach (ContentXElement subElement in element.Elements())
			{
				if (subElement.Name.ToString().Equals("requirement", StringComparison.OrdinalIgnoreCase))
				{
					this.Requirements.Add(new LocationTypeChange.Requirement(subElement, this));
				}
			}
		}

		// Token: 0x0600295A RID: 10586 RVA: 0x0010CD88 File Offset: 0x0010AF88
		public float DetermineProbability(Location location)
		{
			if (this.RequireDiscovered && !location.Discovered)
			{
				return 0f;
			}
			if (location.IsCriticallyRadiated())
			{
				return 0f;
			}
			if (location.LocationTypeChangeCooldown > 0)
			{
				return 0f;
			}
			if (location.IsGateBetweenBiomes)
			{
				return 0f;
			}
			if (this.DisallowedAdjacentLocations.Any<Identifier>() && Map.LocationOrConnectionWithinDistance(location, this.DisallowedProximity, (Location otherLocation) => this.DisallowedAdjacentLocations.Contains(otherLocation.Type.Identifier), null))
			{
				return 0f;
			}
			float probability = this.Probability;
			foreach (LocationTypeChange.Requirement requirement in this.Requirements)
			{
				if (requirement.AnyWithinDistance(location, requirement.RequiredProximity))
				{
					if (requirement.Function == LocationTypeChange.Requirement.FunctionType.Add)
					{
						probability += requirement.Probability;
					}
					else
					{
						probability *= requirement.Probability;
					}
				}
				if (location.ProximityTimer.ContainsKey(requirement) && requirement.AnyWithinDistance(location, requirement.RequiredProximityForProbabilityIncrease))
				{
					if (requirement.Function == LocationTypeChange.Requirement.FunctionType.Add)
					{
						probability += requirement.ProximityProbabilityIncrease * (float)location.ProximityTimer[requirement];
					}
					else
					{
						probability *= requirement.ProximityProbabilityIncrease * (float)location.ProximityTimer[requirement];
					}
				}
			}
			return probability;
		}

		// Token: 0x0400145B RID: 5211
		public readonly Identifier CurrentType;

		// Token: 0x0400145C RID: 5212
		public readonly Identifier ChangeToType;

		// Token: 0x0400145D RID: 5213
		public readonly float Probability;

		// Token: 0x0400145E RID: 5214
		public readonly bool RequireDiscovered;

		// Token: 0x0400145F RID: 5215
		public List<LocationTypeChange.Requirement> Requirements = new List<LocationTypeChange.Requirement>();

		// Token: 0x04001460 RID: 5216
		private readonly bool requireChangeMessages;

		// Token: 0x04001461 RID: 5217
		private readonly string messageTag;

		// Token: 0x04001462 RID: 5218
		public readonly ImmutableArray<Identifier> DisallowedAdjacentLocations;

		// Token: 0x04001463 RID: 5219
		public readonly int DisallowedProximity;

		// Token: 0x04001464 RID: 5220
		public readonly int CooldownAfterChange;

		// Token: 0x04001465 RID: 5221
		public readonly Point RequiredDurationRange;

		// Token: 0x02000A5A RID: 2650
		public class Requirement
		{
			// Token: 0x06005CCF RID: 23759 RVA: 0x00201AD0 File Offset: 0x001FFCD0
			public Requirement(ContentXElement element, LocationTypeChange change)
			{
				this.RequiredLocations = element.GetAttributeIdentifierArray("requiredlocations", element.GetAttributeIdentifierArray("requiredadjacentlocations", Array.Empty<Identifier>(), true), true).ToImmutableArray<Identifier>();
				this.RequiredProximity = Math.Max(element.GetAttributeInt("requiredproximity", 1), 0);
				this.ProximityProbabilityIncrease = element.GetAttributeFloat("proximityprobabilityincrease", 0f);
				this.RequiredProximityForProbabilityIncrease = element.GetAttributeInt("requiredproximityforprobabilityincrease", -1);
				this.RequireBeaconStation = element.GetAttributeBool("requirebeaconstation", false);
				this.RequireHuntingGrounds = element.GetAttributeBool("requirehuntinggrounds", false);
				string functionStr = element.GetAttributeString("function", "Add");
				if (!Enum.TryParse<LocationTypeChange.Requirement.FunctionType>(functionStr, true, out this.Function))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(77, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Invalid location type change in location type \"");
					defaultInterpolatedStringHandler.AppendFormatted<Identifier>(change.CurrentType);
					defaultInterpolatedStringHandler.AppendLiteral("\". ");
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					defaultInterpolatedStringHandler.AppendFormatted(functionStr);
					defaultInterpolatedStringHandler.AppendLiteral("\" is not a valid function.");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				this.Probability = element.GetAttributeFloat("probability", 1f);
				if (this.RequiredProximityForProbabilityIncrease > 0 || this.ProximityProbabilityIncrease > 0f)
				{
					if (!this.RequiredLocations.Any<Identifier>() && !this.RequireBeaconStation && !this.RequireHuntingGrounds)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(50, 1);
						defaultInterpolatedStringHandler2.AppendLiteral("Invalid location type change in location type \"");
						defaultInterpolatedStringHandler2.AppendFormatted<Identifier>(change.CurrentType);
						defaultInterpolatedStringHandler2.AppendLiteral("\". ");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler2.ToStringAndClear() + "Probability is configured to increase when near some other type of location, but the RequiredLocations attribute is not set.", element.ContentPackage);
					}
					if (this.Probability >= 1f)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(50, 1);
						defaultInterpolatedStringHandler3.AppendLiteral("Invalid location type change in location type \"");
						defaultInterpolatedStringHandler3.AppendFormatted<Identifier>(change.CurrentType);
						defaultInterpolatedStringHandler3.AppendLiteral("\". ");
						DebugConsole.AddWarning(defaultInterpolatedStringHandler3.ToStringAndClear() + "Probability is configured to increase when near some other type of location, but the base probability is already 100%", element.ContentPackage);
					}
				}
			}

			// Token: 0x06005CD0 RID: 23760 RVA: 0x00201CE2 File Offset: 0x001FFEE2
			public bool AnyWithinDistance(Location startLocation, int distance)
			{
				return Map.LocationOrConnectionWithinDistance(startLocation, distance, new Func<Location, bool>(this.MatchesLocation), new Func<LocationConnection, bool>(this.MatchesConnection));
			}

			// Token: 0x06005CD1 RID: 23761 RVA: 0x00201D03 File Offset: 0x001FFF03
			public bool MatchesLocation(Location location)
			{
				return this.RequiredLocations.Contains(location.Type.Identifier) && !location.IsCriticallyRadiated();
			}

			// Token: 0x06005CD2 RID: 23762 RVA: 0x00201D28 File Offset: 0x001FFF28
			public bool MatchesConnection(LocationConnection connection)
			{
				return (this.RequireBeaconStation && connection.LevelData.HasBeaconStation && connection.LevelData.IsBeaconActive) || (this.RequireHuntingGrounds && connection.LevelData.HasHuntingGrounds);
			}

			// Token: 0x040035E6 RID: 13798
			public readonly LocationTypeChange.Requirement.FunctionType Function;

			// Token: 0x040035E7 RID: 13799
			public readonly ImmutableArray<Identifier> RequiredLocations;

			// Token: 0x040035E8 RID: 13800
			public readonly int RequiredProximity;

			// Token: 0x040035E9 RID: 13801
			public readonly float Probability;

			// Token: 0x040035EA RID: 13802
			public readonly int RequiredProximityForProbabilityIncrease;

			// Token: 0x040035EB RID: 13803
			public readonly float ProximityProbabilityIncrease;

			// Token: 0x040035EC RID: 13804
			public readonly bool RequireBeaconStation;

			// Token: 0x040035ED RID: 13805
			public readonly bool RequireHuntingGrounds;

			// Token: 0x02000EB1 RID: 3761
			public enum FunctionType
			{
				// Token: 0x0400431E RID: 17182
				Add,
				// Token: 0x0400431F RID: 17183
				Multiply
			}
		}
	}
}
