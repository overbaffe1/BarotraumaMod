using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net;
using System.Net.Cache;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Networking;
using RestSharp;

namespace Barotrauma
{
	// Token: 0x02000138 RID: 312
	[NullableContext(1)]
	[Nullable(0)]
	internal static class SpamServerFilters
	{
		// Token: 0x060028AA RID: 10410 RVA: 0x001C4360 File Offset: 0x001C2560
		internal static bool TryCompileAndCacheRegex(string pattern)
		{
			if (SpamServerFilters.CompiledRegexCache.ContainsKey(pattern))
			{
				return true;
			}
			bool result;
			try
			{
				Regex regex = new Regex(pattern, RegexOptions.Compiled);
				SpamServerFilters.CompiledRegexCache[pattern] = regex;
				result = true;
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Invalid regex pattern in spam filter: \"" + pattern + "\"", e, null, false, false);
				result = false;
			}
			return result;
		}

		// Token: 0x060028AB RID: 10411 RVA: 0x001C43C4 File Offset: 0x001C25C4
		[return: Nullable(2)]
		internal static Regex GetCachedRegex(string pattern)
		{
			return SpamServerFilters.CompiledRegexCache.GetValueOrDefault(pattern);
		}

		// Token: 0x060028AC RID: 10412 RVA: 0x001C43D4 File Offset: 0x001C25D4
		static SpamServerFilters()
		{
			XDocument doc;
			if (!File.Exists(SpamServerFilter.SavePath))
			{
				XComment comment = new XComment("\r\nThis file contains a list of filters that can be used to hide servers from the server list.\r\nYou can add filters by right-clicking a server in the server list and selecting \"Hide server\" or by reporting the server and choosing \"Report and hide server\".\r\nThe filters are saved in this file, which you can edit manually if you want to.\r\n\r\nThe available filter types are:\r\n- NameEquals: The server name must equal the specified value. Homoglyphs are also checked.\r\n- NameContains: The server name must contain the specified value. Homoglyphs are also checked.\r\n- NameMatchesRegex: The server name must match the specified regular expression pattern. Use inline options like (?i) for case-insensitive matching.\r\n- MessageEquals: The server description must equal the specified value. Homoglyphs are also checked.\r\n- MessageContains: The server description must contain the specified value. Homoglyphs are also checked.\r\n- MessageMatchesRegex: The server description must match the specified regular expression pattern. Use inline options like (?i) for case-insensitive matching.\r\n- PlayerCountLarger: The player count must be larger than the specified value.\r\n- PlayerCountExact: The player count must match the specified value exactly.\r\n- MaxPlayersLarger: The max player count must be larger than the specified value.\r\n- MaxPlayersExact: The max player count must match the specified value exactly.\r\n- GameModeEquals: The game mode identifier must match the specified value exactly. Homoglyphs are also checked.\r\n- PlayStyleEquals: The play style must match the specified value exactly.\r\n- Endpoint: The server endpoint, which is a Steam ID or an IP address, must match the specified value exactly. Steam ID is in the format of STEAM_X:Y:Z.\r\n- LanguageEquals: The server language must match the specified value exactly.\r\n- LobbyId: The Steam lobby ID must match the specified value exactly. This is the most efficient way to filter Steam P2P lobbies, when we have already identified harmful ones.\r\n\r\nThe filter values are case-insensitive and adding multiple conditions on one filter will require all of them to be met.\r\nHomoglyph comparison is used for name, message, and game mode filters, which means that it checks whether the words look the same, meaning you can't abuse identical-looking but different symbols to work around the filter. For example \"lmaobox\" and \"lmаobox\" (with a cyrillic a) are considered equal, and \"dіscord.gg\" (with a cyrillic i) will be caught by a \"discord.gg\" contains filter.\r\n\r\nExamples:\r\n<Filters>\r\n  <Filter namecontains=\"discord.gg\" />\r\n  <Filter messagecontains=\"discord.gg\" />\r\n  <Filter nameequals=\"get good get lmaobox\" maxplayersexact=\"999\" />\r\n  <Filter lobbyid=\"109775241070418378\" />\r\n  <Filter namematchesregex=\"(?i)(buy|sell|trade).*cheap\" />\r\n  <Filter messagematchesregex=\"(?i)join.*discord\\.(gg|com)\" />\r\n</Filters>\r\nThese will hide all servers that have a discord.gg link in their name or description, servers with the name \"get good get lmaobox\" that have 999 max players, the specific lobby with ID 109775241070418378, servers with names matching the pattern for buying/selling/trading (case-insensitive), and servers with messages containing discord links (case-insensitive)..\r\n");
				doc = new XDocument(new object[]
				{
					comment,
					new XElement("Filters")
				});
				try
				{
					using (XmlWriter writer = XmlWriter.Create(SpamServerFilter.SavePath, new XmlWriterSettings
					{
						Indent = true
					}))
					{
						doc.SaveSafe(writer);
					}
					goto IL_8D;
				}
				catch (Exception e)
				{
					DebugConsole.ThrowError("Saving spam filter failed.", e, null, false, false);
					goto IL_8D;
				}
			}
			doc = XMLExtensions.TryLoadXml(SpamServerFilter.SavePath);
			IL_8D:
			XElement root = (doc != null) ? doc.Root : null;
			if (root != null)
			{
				SpamServerFilters.LocalSpamFilter = Option.Some<SpamServerFilter>(new SpamServerFilter(root));
			}
		}

		// Token: 0x060028AD RID: 10413 RVA: 0x001C44AC File Offset: 0x001C26AC
		public static bool IsFiltered(ServerInfo info)
		{
			SpamServerFilter localFilter;
			SpamServerFilter globalFilter;
			return (SpamServerFilters.LocalSpamFilter.TryUnwrap(out localFilter) && localFilter.IsFiltered(info)) || (SpamServerFilters.GlobalSpamFilter.TryUnwrap(out globalFilter) && globalFilter.IsFiltered(info));
		}

		// Token: 0x060028AE RID: 10414 RVA: 0x001C44EC File Offset: 0x001C26EC
		public static void AddServerToLocalSpamList(ServerInfo info)
		{
			SpamServerFilter localFilter;
			if (!SpamServerFilters.LocalSpamFilter.TryUnwrap(out localFilter))
			{
				return;
			}
			if (localFilter.IsFiltered(info))
			{
				return;
			}
			ImmutableArray<SpamFilter> filters = localFilter.Filters.Add(new SpamFilter(ImmutableHashSet.Create<ValueTuple<SpamServerFilterType, string, string>>(new ValueTuple<SpamServerFilterType, string, string>(SpamServerFilterType.NameEquals, info.ServerName, info.NormalizedServerName))));
			SpamServerFilter newFilter = new SpamServerFilter(filters);
			newFilter.Save(SpamServerFilter.SavePath);
			SpamServerFilters.LocalSpamFilter = Option.Some<SpamServerFilter>(newFilter);
		}

		// Token: 0x060028AF RID: 10415 RVA: 0x001C4558 File Offset: 0x001C2758
		public static void ClearLocalSpamFilter()
		{
			SpamServerFilter newFilter = new SpamServerFilter(ImmutableArray<SpamFilter>.Empty);
			newFilter.Save(SpamServerFilter.SavePath);
			SpamServerFilters.LocalSpamFilter = Option.Some<SpamServerFilter>(newFilter);
		}

		// Token: 0x060028B0 RID: 10416 RVA: 0x001C4588 File Offset: 0x001C2788
		public static void RequestGlobalSpamFilter()
		{
			if (GameSettings.CurrentConfig.DisableGlobalSpamList)
			{
				return;
			}
			string remoteContentUrl = GameSettings.CurrentConfig.RemoteMainMenuContentUrl;
			if (string.IsNullOrEmpty(remoteContentUrl))
			{
				return;
			}
			try
			{
				RestClient client = RestFactory.CreateClient(remoteContentUrl + "spamfilter");
				client.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
				client.AddDefaultHeader("Cache-Control", "no-cache");
				client.AddDefaultHeader("Pragma", "no-cache");
				RestRequest request = RestFactory.CreateRequest("serve_spamlist.php", Method.GET);
				string name = "RequestGlobalSpamFilter";
				Task task = client.ExecuteAsync(request, default(CancellationToken));
				Action<Task> onCompletion;
				if ((onCompletion = SpamServerFilters.<>O.<0>__RemoteContentReceived) == null)
				{
					onCompletion = (SpamServerFilters.<>O.<0>__RemoteContentReceived = new Action<Task>(SpamServerFilters.<RequestGlobalSpamFilter>g__RemoteContentReceived|10_0));
				}
				TaskPool.Add(name, task, onCompletion);
			}
			catch (Exception e)
			{
				GameAnalyticsManager.AddErrorEventOnce("SpamServerFilters.RequestGlobalSpamFilter:Exception", GameAnalyticsManager.ErrorSeverity.Error, "Fetching global spam list failed. " + e.Message);
			}
		}

		// Token: 0x060028B1 RID: 10417 RVA: 0x001C466C File Offset: 0x001C286C
		[CompilerGenerated]
		internal static void <RequestGlobalSpamFilter>g__RemoteContentReceived|10_0(Task t)
		{
			try
			{
				IRestResponse remoteContentResponse;
				if (!t.TryGetResult(out remoteContentResponse))
				{
					throw new Exception("Task did not return a valid result");
				}
				if (remoteContentResponse.ErrorException != null)
				{
					DebugConsole.AddWarning("Connection error: Failed to receive global spam filter (" + remoteContentResponse.ErrorException.Message + ").", null);
				}
				else if (remoteContentResponse.StatusCode != HttpStatusCode.OK)
				{
					string str = "Failed to receive global spam filter. ";
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(65, 1);
					defaultInterpolatedStringHandler.AppendLiteral("The master server might be temporarily unavailable, HTTP status: ");
					defaultInterpolatedStringHandler.AppendFormatted<HttpStatusCode>(remoteContentResponse.StatusCode);
					DebugConsole.AddWarning(str + defaultInterpolatedStringHandler.ToStringAndClear(), null);
				}
				else
				{
					string data = remoteContentResponse.Content;
					if (!string.IsNullOrWhiteSpace(data))
					{
						XElement root = XDocument.Parse(data).Root;
						if (root != null)
						{
							SpamServerFilters.GlobalSpamFilter = Option.Some<SpamServerFilter>(new SpamServerFilter(root));
						}
					}
				}
			}
			catch (Exception e)
			{
				GameAnalyticsManager.AddErrorEventOnce("SpamServerFilters.RemoteContentReceived:Exception", GameAnalyticsManager.ErrorSeverity.Error, "Reading received global spam filter failed. " + e.Message);
			}
		}

		// Token: 0x040014D2 RID: 5330
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<SpamServerFilter> LocalSpamFilter;

		// Token: 0x040014D3 RID: 5331
		[Nullable(new byte[]
		{
			0,
			1
		})]
		public static Option<SpamServerFilter> GlobalSpamFilter;

		// Token: 0x040014D4 RID: 5332
		private static readonly Dictionary<string, Regex> CompiledRegexCache = new Dictionary<string, Regex>();

		// Token: 0x040014D5 RID: 5333
		public const string LocalFilterComment = "\r\nThis file contains a list of filters that can be used to hide servers from the server list.\r\nYou can add filters by right-clicking a server in the server list and selecting \"Hide server\" or by reporting the server and choosing \"Report and hide server\".\r\nThe filters are saved in this file, which you can edit manually if you want to.\r\n\r\nThe available filter types are:\r\n- NameEquals: The server name must equal the specified value. Homoglyphs are also checked.\r\n- NameContains: The server name must contain the specified value. Homoglyphs are also checked.\r\n- NameMatchesRegex: The server name must match the specified regular expression pattern. Use inline options like (?i) for case-insensitive matching.\r\n- MessageEquals: The server description must equal the specified value. Homoglyphs are also checked.\r\n- MessageContains: The server description must contain the specified value. Homoglyphs are also checked.\r\n- MessageMatchesRegex: The server description must match the specified regular expression pattern. Use inline options like (?i) for case-insensitive matching.\r\n- PlayerCountLarger: The player count must be larger than the specified value.\r\n- PlayerCountExact: The player count must match the specified value exactly.\r\n- MaxPlayersLarger: The max player count must be larger than the specified value.\r\n- MaxPlayersExact: The max player count must match the specified value exactly.\r\n- GameModeEquals: The game mode identifier must match the specified value exactly. Homoglyphs are also checked.\r\n- PlayStyleEquals: The play style must match the specified value exactly.\r\n- Endpoint: The server endpoint, which is a Steam ID or an IP address, must match the specified value exactly. Steam ID is in the format of STEAM_X:Y:Z.\r\n- LanguageEquals: The server language must match the specified value exactly.\r\n- LobbyId: The Steam lobby ID must match the specified value exactly. This is the most efficient way to filter Steam P2P lobbies, when we have already identified harmful ones.\r\n\r\nThe filter values are case-insensitive and adding multiple conditions on one filter will require all of them to be met.\r\nHomoglyph comparison is used for name, message, and game mode filters, which means that it checks whether the words look the same, meaning you can't abuse identical-looking but different symbols to work around the filter. For example \"lmaobox\" and \"lmаobox\" (with a cyrillic a) are considered equal, and \"dіscord.gg\" (with a cyrillic i) will be caught by a \"discord.gg\" contains filter.\r\n\r\nExamples:\r\n<Filters>\r\n  <Filter namecontains=\"discord.gg\" />\r\n  <Filter messagecontains=\"discord.gg\" />\r\n  <Filter nameequals=\"get good get lmaobox\" maxplayersexact=\"999\" />\r\n  <Filter lobbyid=\"109775241070418378\" />\r\n  <Filter namematchesregex=\"(?i)(buy|sell|trade).*cheap\" />\r\n  <Filter messagematchesregex=\"(?i)join.*discord\\.(gg|com)\" />\r\n</Filters>\r\nThese will hide all servers that have a discord.gg link in their name or description, servers with the name \"get good get lmaobox\" that have 999 max players, the specific lobby with ID 109775241070418378, servers with names matching the pattern for buying/selling/trading (case-insensitive), and servers with messages containing discord links (case-insensitive)..\r\n";

		// Token: 0x02000D7E RID: 3454
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04004F7D RID: 20349
			[Nullable(new byte[]
			{
				0,
				1
			})]
			public static Action<Task> <0>__RemoteContentReceived;
		}
	}
}
