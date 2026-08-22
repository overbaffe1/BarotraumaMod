using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000136 RID: 310
	[NullableContext(1)]
	[Nullable(0)]
	internal readonly struct SpamFilter : IEquatable<SpamFilter>
	{
		// Token: 0x06002893 RID: 10387 RVA: 0x001C3BC3 File Offset: 0x001C1DC3
		public SpamFilter([TupleElementNames(new string[]
		{
			"Type",
			"Value",
			"NormalizedValue"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] ImmutableHashSet<ValueTuple<SpamServerFilterType, string, string>> Filters)
		{
			this.Filters = Filters;
		}

		// Token: 0x17000A67 RID: 2663
		// (get) Token: 0x06002894 RID: 10388 RVA: 0x001C3BCC File Offset: 0x001C1DCC
		// (set) Token: 0x06002895 RID: 10389 RVA: 0x001C3BD4 File Offset: 0x001C1DD4
		[TupleElementNames(new string[]
		{
			"Type",
			"Value",
			"NormalizedValue"
		})]
		[Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})]
		public ImmutableHashSet<ValueTuple<SpamServerFilterType, string, string>> Filters { [return: TupleElementNames(new string[]
		{
			"Type",
			"Value",
			"NormalizedValue"
		})] [return: Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] get; [param: TupleElementNames(new string[]
		{
			"Type",
			"Value",
			"NormalizedValue"
		})] [param: Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] set; }

		// Token: 0x06002896 RID: 10390 RVA: 0x001C3BE0 File Offset: 0x001C1DE0
		public bool IsFiltered(ServerInfo info)
		{
			if (this.Filters.IsEmpty)
			{
				return false;
			}
			foreach (ValueTuple<SpamServerFilterType, string, string> valueTuple in this.Filters)
			{
				SpamServerFilterType type = valueTuple.Item1;
				string value = valueTuple.Item2;
				string normalizedValue = valueTuple.Item3;
				try
				{
					if (!SpamFilter.IsFiltered(info, type, value, normalizedValue))
					{
						return false;
					}
				}
				catch (Exception e)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(49, 2);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to check filter type ");
					defaultInterpolatedStringHandler.AppendFormatted<SpamServerFilterType>(type);
					defaultInterpolatedStringHandler.AppendLiteral(" on the server info ");
					defaultInterpolatedStringHandler.AppendFormatted(info.ServerName ?? "null");
					defaultInterpolatedStringHandler.AppendLiteral(".");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), e, null, false, false);
				}
			}
			return true;
		}

		// Token: 0x06002897 RID: 10391 RVA: 0x001C3CDC File Offset: 0x001C1EDC
		private static bool IsFiltered(ServerInfo info, SpamServerFilterType type, string value, string normalizedValue)
		{
			if (info == null)
			{
				return true;
			}
			int parsedInt;
			int.TryParse(value, out parsedInt);
			bool result;
			switch (type)
			{
			case SpamServerFilterType.NameEquals:
				result = SpamFilter.<IsFiltered>g__CompareEquals|6_0(info.NormalizedServerName, normalizedValue);
				break;
			case SpamServerFilterType.NameContains:
				result = SpamFilter.<IsFiltered>g__CompareContains|6_1(info.NormalizedServerName, normalizedValue);
				break;
			case SpamServerFilterType.NameMatchesRegex:
				result = SpamFilter.<IsFiltered>g__CompareRegex|6_2(info.NormalizedServerName, value);
				break;
			case SpamServerFilterType.MessageEquals:
				result = SpamFilter.<IsFiltered>g__CompareEquals|6_0(info.NormalizedServerMessage, normalizedValue);
				break;
			case SpamServerFilterType.MessageContains:
				result = SpamFilter.<IsFiltered>g__CompareContains|6_1(info.NormalizedServerMessage, normalizedValue);
				break;
			case SpamServerFilterType.MessageMatchesRegex:
				result = SpamFilter.<IsFiltered>g__CompareRegex|6_2(info.NormalizedServerMessage, value);
				break;
			case SpamServerFilterType.PlayerCountLarger:
				result = (info.PlayerCount > parsedInt);
				break;
			case SpamServerFilterType.PlayerCountExact:
				result = (info.PlayerCount == parsedInt);
				break;
			case SpamServerFilterType.MaxPlayersLarger:
				result = (info.MaxPlayers > parsedInt);
				break;
			case SpamServerFilterType.MaxPlayersExact:
				result = (info.MaxPlayers == parsedInt);
				break;
			case SpamServerFilterType.GameModeEquals:
				result = SpamFilter.<IsFiltered>g__CompareEquals|6_0(info.NormalizedGameMode, normalizedValue);
				break;
			case SpamServerFilterType.PlayStyleEquals:
			{
				Identifier identifier = info.PlayStyle.ToIdentifier<PlayStyle>();
				result = (identifier == value);
				break;
			}
			case SpamServerFilterType.Endpoint:
				result = (new ImmutableArray<Endpoint>?(info.Endpoints) != null && info.Endpoints.First<Endpoint>().StringRepresentation.Equals(value, StringComparison.OrdinalIgnoreCase));
				break;
			case SpamServerFilterType.LanguageEquals:
				result = (info.Language.Value == value);
				break;
			case SpamServerFilterType.LobbyId:
			{
				ServerInfo.DataSource dataSource;
				bool flag;
				if (info.MetadataSource.TryUnwrap(out dataSource))
				{
					SteamP2PServerProvider.DataSource steamDataSource = dataSource as SteamP2PServerProvider.DataSource;
					ulong lobbyIdToFilter;
					if (steamDataSource != null && ulong.TryParse(value, out lobbyIdToFilter))
					{
						flag = (steamDataSource.Lobby.Id == lobbyIdToFilter);
						goto IL_13F;
					}
				}
				flag = false;
				IL_13F:
				result = flag;
				break;
			}
			default:
				result = false;
				break;
			}
			return result;
		}

		// Token: 0x06002898 RID: 10392 RVA: 0x001C3EA8 File Offset: 0x001C20A8
		public XElement Serialize()
		{
			XElement element = new XElement("Filter");
			foreach (ValueTuple<SpamServerFilterType, string, string> valueTuple in this.Filters)
			{
				SpamServerFilterType type = valueTuple.Item1;
				string value = valueTuple.Item2;
				element.Add(new XAttribute(type.ToString().ToLowerInvariant(), value));
			}
			return element;
		}

		// Token: 0x06002899 RID: 10393 RVA: 0x001C3F34 File Offset: 0x001C2134
		public static bool TryParse(XElement element, out SpamFilter filter)
		{
			ImmutableHashSet<ValueTuple<SpamServerFilterType, string, string>>.Builder builder = ImmutableHashSet.CreateBuilder<ValueTuple<SpamServerFilterType, string, string>>();
			foreach (XAttribute attribute in element.Attributes())
			{
				SpamServerFilterType e;
				if (!Enum.TryParse<SpamServerFilterType>(attribute.Name.ToString(), true, out e))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(40, 1);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to parse spam filter attribute \"");
					defaultInterpolatedStringHandler.AppendFormatted<XName>(attribute.Name);
					defaultInterpolatedStringHandler.AppendLiteral("\"");
					DebugConsole.ThrowError(defaultInterpolatedStringHandler.ToStringAndClear(), null, null, false, false);
				}
				else if (e != SpamServerFilterType.Invalid)
				{
					string value = attribute.Value;
					bool flag = e == SpamServerFilterType.NameMatchesRegex || e == SpamServerFilterType.MessageMatchesRegex;
					if (!flag || SpamServerFilters.TryCompileAndCacheRegex(value))
					{
						flag = (e - SpamServerFilterType.NameEquals <= 1 || e - SpamServerFilterType.MessageEquals <= 1 || e == SpamServerFilterType.GameModeEquals);
						string normalizedValue = flag ? Homoglyphs.Normalize(value) : value;
						builder.Add(new ValueTuple<SpamServerFilterType, string, string>(e, value, normalizedValue));
					}
				}
			}
			if (builder.Any<ValueTuple<SpamServerFilterType, string, string>>())
			{
				filter = new SpamFilter(builder.ToImmutable());
				return true;
			}
			filter = default(SpamFilter);
			return false;
		}

		// Token: 0x0600289A RID: 10394 RVA: 0x001C4064 File Offset: 0x001C2264
		public override string ToString()
		{
			if (this.Filters.Any<ValueTuple<SpamServerFilterType, string, string>>())
			{
				return string.Join(", ", this.Filters.Select(delegate([TupleElementNames(new string[]
				{
					"Type",
					"Value",
					"NormalizedValue"
				})] ValueTuple<SpamServerFilterType, string, string> f)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(2, 2);
					defaultInterpolatedStringHandler.AppendFormatted<SpamServerFilterType>(f.Item1);
					defaultInterpolatedStringHandler.AppendLiteral(": ");
					defaultInterpolatedStringHandler.AppendFormatted(f.Item2);
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}));
			}
			return "Invalid Filter";
		}

		// Token: 0x0600289B RID: 10395 RVA: 0x001C40B8 File Offset: 0x001C22B8
		[NullableContext(0)]
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Filters = ");
			builder.Append(this.Filters);
			return true;
		}

		// Token: 0x0600289C RID: 10396 RVA: 0x001C40D4 File Offset: 0x001C22D4
		[CompilerGenerated]
		public static bool operator !=(SpamFilter left, SpamFilter right)
		{
			return !(left == right);
		}

		// Token: 0x0600289D RID: 10397 RVA: 0x001C40E0 File Offset: 0x001C22E0
		[CompilerGenerated]
		public static bool operator ==(SpamFilter left, SpamFilter right)
		{
			return left.Equals(right);
		}

		// Token: 0x0600289E RID: 10398 RVA: 0x001C40EA File Offset: 0x001C22EA
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<ImmutableHashSet<ValueTuple<SpamServerFilterType, string, string>>>.Default.GetHashCode(this.<Filters>k__BackingField);
		}

		// Token: 0x0600289F RID: 10399 RVA: 0x001C40FC File Offset: 0x001C22FC
		[NullableContext(0)]
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is SpamFilter && this.Equals((SpamFilter)obj);
		}

		// Token: 0x060028A0 RID: 10400 RVA: 0x001C4114 File Offset: 0x001C2314
		[CompilerGenerated]
		public bool Equals(SpamFilter other)
		{
			return EqualityComparer<ImmutableHashSet<ValueTuple<SpamServerFilterType, string, string>>>.Default.Equals(this.<Filters>k__BackingField, other.<Filters>k__BackingField);
		}

		// Token: 0x060028A1 RID: 10401 RVA: 0x001C412C File Offset: 0x001C232C
		[CompilerGenerated]
		public void Deconstruct([TupleElementNames(new string[]
		{
			"Type",
			"Value",
			"NormalizedValue"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] out ImmutableHashSet<ValueTuple<SpamServerFilterType, string, string>> Filters)
		{
			Filters = this.Filters;
		}

		// Token: 0x060028A2 RID: 10402 RVA: 0x001C4136 File Offset: 0x001C2336
		[NullableContext(2)]
		[CompilerGenerated]
		internal static bool <IsFiltered>g__CompareEquals|6_0(string normalizedA, string normalizedB)
		{
			if (normalizedA == null || normalizedB == null)
			{
				return normalizedA == normalizedB;
			}
			return normalizedA.Equals(normalizedB, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060028A3 RID: 10403 RVA: 0x001C414E File Offset: 0x001C234E
		[NullableContext(2)]
		[CompilerGenerated]
		internal static bool <IsFiltered>g__CompareContains|6_1(string normalizedA, string normalizedB)
		{
			if (normalizedA == null || normalizedB == null)
			{
				return normalizedA == normalizedB;
			}
			return normalizedA.Contains(normalizedB, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060028A4 RID: 10404 RVA: 0x001C4168 File Offset: 0x001C2368
		[NullableContext(2)]
		[CompilerGenerated]
		internal static bool <IsFiltered>g__CompareRegex|6_2(string a, string pattern)
		{
			if (a == null || pattern == null)
			{
				return a == pattern;
			}
			Regex regex = SpamServerFilters.GetCachedRegex(pattern);
			if (regex != null)
			{
				return regex.IsMatch(a);
			}
			DebugConsole.ThrowError("Regex pattern somehow not found in cache: \"" + pattern + "\"", null, null, false, false);
			return false;
		}
	}
}
