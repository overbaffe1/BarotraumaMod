using System;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Xml;
using System.Xml.Linq;
using Barotrauma.IO;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x02000137 RID: 311
	[NullableContext(1)]
	[Nullable(0)]
	internal sealed class SpamServerFilter
	{
		// Token: 0x060028A5 RID: 10405 RVA: 0x001C41B0 File Offset: 0x001C23B0
		public bool IsFiltered(ServerInfo info)
		{
			foreach (SpamFilter f in this.Filters)
			{
				if (f.IsFiltered(info))
				{
					return true;
				}
			}
			return false;
		}

		// Token: 0x060028A6 RID: 10406 RVA: 0x001C41EC File Offset: 0x001C23EC
		public SpamServerFilter(XElement element)
		{
			ImmutableArray<SpamFilter>.Builder builder = ImmutableArray.CreateBuilder<SpamFilter>();
			foreach (XElement subElement in element.Elements())
			{
				SpamFilter filter;
				if (SpamFilter.TryParse(subElement, out filter))
				{
					builder.Add(filter);
				}
			}
			this.Filters = builder.ToImmutable();
		}

		// Token: 0x060028A7 RID: 10407 RVA: 0x001C425C File Offset: 0x001C245C
		[NullableContext(0)]
		public SpamServerFilter(ImmutableArray<SpamFilter> filters)
		{
			this.Filters = filters;
		}

		// Token: 0x060028A8 RID: 10408 RVA: 0x001C426C File Offset: 0x001C246C
		public void Save(string path)
		{
			XComment comment = new XComment("\r\nThis file contains a list of filters that can be used to hide servers from the server list.\r\nYou can add filters by right-clicking a server in the server list and selecting \"Hide server\" or by reporting the server and choosing \"Report and hide server\".\r\nThe filters are saved in this file, which you can edit manually if you want to.\r\n\r\nThe available filter types are:\r\n- NameEquals: The server name must equal the specified value. Homoglyphs are also checked.\r\n- NameContains: The server name must contain the specified value. Homoglyphs are also checked.\r\n- NameMatchesRegex: The server name must match the specified regular expression pattern. Use inline options like (?i) for case-insensitive matching.\r\n- MessageEquals: The server description must equal the specified value. Homoglyphs are also checked.\r\n- MessageContains: The server description must contain the specified value. Homoglyphs are also checked.\r\n- MessageMatchesRegex: The server description must match the specified regular expression pattern. Use inline options like (?i) for case-insensitive matching.\r\n- PlayerCountLarger: The player count must be larger than the specified value.\r\n- PlayerCountExact: The player count must match the specified value exactly.\r\n- MaxPlayersLarger: The max player count must be larger than the specified value.\r\n- MaxPlayersExact: The max player count must match the specified value exactly.\r\n- GameModeEquals: The game mode identifier must match the specified value exactly. Homoglyphs are also checked.\r\n- PlayStyleEquals: The play style must match the specified value exactly.\r\n- Endpoint: The server endpoint, which is a Steam ID or an IP address, must match the specified value exactly. Steam ID is in the format of STEAM_X:Y:Z.\r\n- LanguageEquals: The server language must match the specified value exactly.\r\n- LobbyId: The Steam lobby ID must match the specified value exactly. This is the most efficient way to filter Steam P2P lobbies, when we have already identified harmful ones.\r\n\r\nThe filter values are case-insensitive and adding multiple conditions on one filter will require all of them to be met.\r\nHomoglyph comparison is used for name, message, and game mode filters, which means that it checks whether the words look the same, meaning you can't abuse identical-looking but different symbols to work around the filter. For example \"lmaobox\" and \"lmаobox\" (with a cyrillic a) are considered equal, and \"dіscord.gg\" (with a cyrillic i) will be caught by a \"discord.gg\" contains filter.\r\n\r\nExamples:\r\n<Filters>\r\n  <Filter namecontains=\"discord.gg\" />\r\n  <Filter messagecontains=\"discord.gg\" />\r\n  <Filter nameequals=\"get good get lmaobox\" maxplayersexact=\"999\" />\r\n  <Filter lobbyid=\"109775241070418378\" />\r\n  <Filter namematchesregex=\"(?i)(buy|sell|trade).*cheap\" />\r\n  <Filter messagematchesregex=\"(?i)join.*discord\\.(gg|com)\" />\r\n</Filters>\r\nThese will hide all servers that have a discord.gg link in their name or description, servers with the name \"get good get lmaobox\" that have 999 max players, the specific lobby with ID 109775241070418378, servers with names matching the pattern for buying/selling/trading (case-insensitive), and servers with messages containing discord links (case-insensitive)..\r\n");
			XDocument doc = new XDocument(new object[]
			{
				comment,
				new XElement("Filters")
			});
			foreach (SpamFilter filter in this.Filters)
			{
				XElement root = doc.Root;
				if (root != null)
				{
					root.Add(filter.Serialize());
				}
			}
			try
			{
				using (XmlWriter writer = XmlWriter.Create(path, new XmlWriterSettings
				{
					Indent = true
				}))
				{
					doc.SaveSafe(writer);
				}
			}
			catch (Exception e)
			{
				DebugConsole.ThrowError("Saving spam filter failed.", e, null, false, false);
			}
		}

		// Token: 0x040014D0 RID: 5328
		[Nullable(0)]
		public readonly ImmutableArray<SpamFilter> Filters;

		// Token: 0x040014D1 RID: 5329
		public static readonly string SavePath = Path.Combine(new string[]
		{
			"Data",
			"serverblacklist.xml"
		});
	}
}
