using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000321 RID: 801
	internal class LocationTypeChange
	{
		// Token: 0x06004010 RID: 16400 RVA: 0x0023C30C File Offset: 0x0023A50C
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

		// Token: 0x06004011 RID: 16401 RVA: 0x0023C408 File Offset: 0x0023A608
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

		// Token: 0x06004012 RID: 16402 RVA: 0x0023C5C8 File Offset: 0x0023A7C8
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

		// Token: 0x04002156 RID: 8534
		public readonly Identifier CurrentType;

		// Token: 0x04002157 RID: 8535
		public readonly Identifier ChangeToType;

		// Token: 0x04002158 RID: 8536
		public readonly float Probability;

		// Token: 0x04002159 RID: 8537
		public readonly bool RequireDiscovered;

		// Token: 0x0400215A RID: 8538
		public List<LocationTypeChange.Requirement> Requirements = new List<LocationTypeChange.Requirement>();

		// Token: 0x0400215B RID: 8539
		private readonly bool requireChangeMessages;

		// Token: 0x0400215C RID: 8540
		private readonly string messageTag;

		// Token: 0x0400215D RID: 8541
		public readonly ImmutableArray<Identifier> DisallowedAdjacentLocations;

		// Token: 0x0400215E RID: 8542
		public readonly int DisallowedProximity;

		// Token: 0x0400215F RID: 8543
		public readonly int CooldownAfterChange;

		// Token: 0x04002160 RID: 8544
		public readonly Point RequiredDurationRange;

		// Token: 0x02001016 RID: 4118
		public class Requirement
		{
			// Token: 0x06008B40 RID: 35648 RVA: 0x003AC920 File Offset: 0x003AAB20
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

			// Token: 0x06008B41 RID: 35649 RVA: 0x003ACB32 File Offset: 0x003AAD32
			public bool AnyWithinDistance(Location startLocation, int distance)
			{
				return Map.LocationOrConnectionWithinDistance(startLocation, distance, new Func<Location, bool>(this.MatchesLocation), new Func<LocationConnection, bool>(this.MatchesConnection));
			}

			// Token: 0x06008B42 RID: 35650 RVA: 0x003ACB53 File Offset: 0x003AAD53
			public bool MatchesLocation(Location location)
			{
				return this.RequiredLocations.Contains(location.Type.Identifier) && !location.IsCriticallyRadiated();
			}

			// Token: 0x06008B43 RID: 35651 RVA: 0x003ACB78 File Offset: 0x003AAD78
			public bool MatchesConnection(LocationConnection connection)
			{
				return (this.RequireBeaconStation && connection.LevelData.HasBeaconStation && connection.LevelData.IsBeaconActive) || (this.RequireHuntingGrounds && connection.LevelData.HasHuntingGrounds);
			}

			// Token: 0x04005753 RID: 22355
			public readonly LocationTypeChange.Requirement.FunctionType Function;

			// Token: 0x04005754 RID: 22356
			public readonly ImmutableArray<Identifier> RequiredLocations;

			// Token: 0x04005755 RID: 22357
			public readonly int RequiredProximity;

			// Token: 0x04005756 RID: 22358
			public readonly float Probability;

			// Token: 0x04005757 RID: 22359
			public readonly int RequiredProximityForProbabilityIncrease;

			// Token: 0x04005758 RID: 22360
			public readonly float ProximityProbabilityIncrease;

			// Token: 0x04005759 RID: 22361
			public readonly bool RequireBeaconStation;

			// Token: 0x0400575A RID: 22362
			public readonly bool RequireHuntingGrounds;

			// Token: 0x02001571 RID: 5489
			public enum FunctionType
			{
				// Token: 0x04006882 RID: 26754
				Add,
				// Token: 0x04006883 RID: 26755
				Multiply
			}
		}
	}
}
