using System;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Barotrauma.Steam;

namespace Barotrauma
{
	// Token: 0x0200012D RID: 301
	internal static class SocialExtensions
	{
		// Token: 0x0600283D RID: 10301 RVA: 0x001BF4E0 File Offset: 0x001BD6E0
		public static LocalizedString ViewProfileLabel(this AccountId accountId)
		{
			LocalizedString result;
			if (!(accountId is SteamId))
			{
				if (!(accountId is EpicAccountId))
				{
					result = "View profile of unknown origin";
				}
				else
				{
					result = TextManager.Get("ViewEpicProfile");
				}
			}
			else
			{
				result = TextManager.Get("ViewSteamProfile");
			}
			return result;
		}

		// Token: 0x0600283E RID: 10302 RVA: 0x001BF528 File Offset: 0x001BD728
		public static void OpenProfile(this AccountId accountId)
		{
			SteamId steamId = accountId as SteamId;
			string text;
			if (steamId == null)
			{
				EpicAccountId epicAccountId = accountId as EpicAccountId;
				if (epicAccountId == null)
				{
					text = "";
				}
				else
				{
					text = "https://store.epicgames.com/u/" + epicAccountId.EosStringRepresentation;
				}
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(36, 1);
				defaultInterpolatedStringHandler.AppendLiteral("https://steamcommunity.com/profiles/");
				defaultInterpolatedStringHandler.AppendFormatted<ulong>(steamId.Value);
				text = defaultInterpolatedStringHandler.ToStringAndClear();
			}
			string url = text;
			if (SteamManager.IsInitialized)
			{
				SteamManager.OverlayCustomUrl(url);
				return;
			}
			GameMain.ShowOpenUriPrompt(url, "openlinkinbrowserprompt", null);
		}
	}
}
