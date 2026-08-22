using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Barotrauma
{
	// Token: 0x020002A2 RID: 674
	[NullableContext(1)]
	[Nullable(0)]
	public class ServerMsgLString : LocalizedString
	{
		// Token: 0x06002EC8 RID: 11976 RVA: 0x001388DC File Offset: 0x00136ADC
		public ServerMsgLString(string serverMsg)
		{
			this.serverMessage = serverMsg;
			this.messageSplit = this.serverMessage.Split("/", StringSplitOptions.None).ToImmutableArray<string>();
		}

		// Token: 0x06002EC9 RID: 11977 RVA: 0x0013890C File Offset: 0x00136B0C
		private static bool IsServerMessageWithVariables(string message)
		{
			return ServerMsgLString.serverMessageCharacters.All(new Func<char, bool>(message.Contains));
		}

		// Token: 0x17000DA4 RID: 3492
		// (get) Token: 0x06002ECA RID: 11978 RVA: 0x00138924 File Offset: 0x00136B24
		public override bool Loaded
		{
			get
			{
				if (this.loadedSuccessfully == LocalizedString.LoadedSuccessfully.Unknown)
				{
					this.RetrieveValue();
				}
				return this.loadedSuccessfully == LocalizedString.LoadedSuccessfully.Yes;
			}
		}

		// Token: 0x06002ECB RID: 11979 RVA: 0x00138940 File Offset: 0x00136B40
		public override void RetrieveValue()
		{
			ServerMsgLString.<>c__DisplayClass11_0 CS$<>8__locals1;
			CS$<>8__locals1.replacedMessages = new Dictionary<string, string>();
			CS$<>8__locals1.translationsFound = false;
			try
			{
				string translatedServerMessage = "";
				for (int i = 0; i < this.messageSplit.Length; i++)
				{
					string message = ServerMsgLString.<RetrieveValue>g__TranslateMessage|11_0(this.messageSplit[i], ref CS$<>8__locals1);
					if (message != null)
					{
						translatedServerMessage += message;
					}
				}
				this.cachedValue = (CS$<>8__locals1.translationsFound ? translatedServerMessage : this.serverMessage);
				this.loadedSuccessfully = LocalizedString.LoadedSuccessfully.Yes;
			}
			catch (IndexOutOfRangeException exception)
			{
				string errorMsg = "Failed to translate server message \"" + this.serverMessage + "\".";
				GameAnalyticsManager.AddErrorEventOnce("TextManager.GetServerMessage:" + this.serverMessage, GameAnalyticsManager.ErrorSeverity.Error, errorMsg);
				this.cachedValue = errorMsg;
				this.loadedSuccessfully = LocalizedString.LoadedSuccessfully.No;
			}
			base.UpdateLanguage();
		}

		// Token: 0x06002ECD RID: 11981 RVA: 0x00138A90 File Offset: 0x00136C90
		[CompilerGenerated]
		[return: Nullable(2)]
		internal static string <RetrieveValue>g__TranslateMessage|11_0(string input, ref ServerMsgLString.<>c__DisplayClass11_0 A_1)
		{
			string message = input;
			if (message.EndsWith("~", StringComparison.Ordinal))
			{
				message = message.Substring(0, message.Length - 1);
			}
			if (!ServerMsgLString.IsServerMessageWithVariables(message) && !message.Contains('='))
			{
				foreach (KeyValuePair<string, string> replacedMessage in A_1.replacedMessages)
				{
					message = message.Replace(replacedMessage.Key, replacedMessage.Value);
				}
				if (message.Contains(" "))
				{
					return message;
				}
				LocalizedString msg = TextManager.Get(message);
				if (msg.Loaded)
				{
					message = msg.Value;
					A_1.translationsFound = true;
				}
			}
			else
			{
				string messageVariable = null;
				Match matchFormatted = ServerMsgLString.reFormattedMessage.Match(message);
				if (matchFormatted.Success)
				{
					Identifier formatter = matchFormatted.Groups["formatter"].ToString().ToIdentifier();
					Func<string, string> formatterFn;
					if (ServerMsgLString.messageFormatters.TryGetValue(formatter, out formatterFn))
					{
						string formattedValue = formatterFn(matchFormatted.Groups["value"].ToString());
						if (formattedValue != null)
						{
							messageVariable = matchFormatted.Groups["variable"].ToString();
							message = formattedValue;
						}
					}
				}
				if (messageVariable == null)
				{
					Match matchReplaced = ServerMsgLString.reReplacedMessage.Match(message);
					if (matchReplaced.Success)
					{
						messageVariable = matchReplaced.Groups["variable"].ToString();
						message = matchReplaced.Groups["message"].ToString();
					}
				}
				foreach (KeyValuePair<string, string> replacedMessage2 in A_1.replacedMessages)
				{
					message = message.Replace(replacedMessage2.Key, replacedMessage2.Value);
				}
				string[] messageWithVariables = message.Split('~', StringSplitOptions.None);
				LocalizedString msg2 = TextManager.Get(messageWithVariables[0]);
				if (msg2.Loaded)
				{
					message = msg2.Value;
					A_1.translationsFound = true;
				}
				else if (messageVariable == null)
				{
					return message;
				}
				for (int i = 1; i < messageWithVariables.Length; i++)
				{
					string[] variableAndValue = messageWithVariables[i].Split('=', StringSplitOptions.None);
					message = message.Replace(variableAndValue[0], (variableAndValue[1].Length > 1 && variableAndValue[1][0] == '§') ? TextManager.Get(variableAndValue[1].Substring(1)).Value : variableAndValue[1]);
				}
				if (messageVariable != null)
				{
					A_1.replacedMessages[messageVariable] = message;
					message = null;
				}
			}
			return message;
		}

		// Token: 0x0400176F RID: 5999
		private static readonly Regex reFormattedMessage = new Regex("^(?<variable>[\\[\\].a-z0-9_]+?)=(?<formatter>[a-z0-9_]+?)\\((?<value>.+?)\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		// Token: 0x04001770 RID: 6000
		private static readonly Regex reReplacedMessage = new Regex("^(?<variable>[\\[\\].a-z0-9_]+?)=(?<message>.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		// Token: 0x04001771 RID: 6001
		[Nullable(new byte[]
		{
			1,
			1,
			1,
			2
		})]
		private static readonly ImmutableDictionary<Identifier, Func<string, string>> messageFormatters = new Dictionary<Identifier, Func<string, string>>
		{
			{
				"duration".ToIdentifier(),
				delegate(string secondsValue)
				{
					double seconds;
					if (!double.TryParse(secondsValue, out seconds))
					{
						return null;
					}
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(0, 1);
					defaultInterpolatedStringHandler.AppendFormatted<TimeSpan>(TimeSpan.FromSeconds(seconds), "g");
					return defaultInterpolatedStringHandler.ToStringAndClear();
				}
			}
		}.ToImmutableDictionary<Identifier, Func<string, string>>();

		// Token: 0x04001772 RID: 6002
		private static readonly ImmutableHashSet<char> serverMessageCharacters = new char[]
		{
			'~',
			'[',
			']',
			'='
		}.ToImmutableHashSet<char>();

		// Token: 0x04001773 RID: 6003
		private readonly string serverMessage;

		// Token: 0x04001774 RID: 6004
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<string> messageSplit;

		// Token: 0x04001775 RID: 6005
		private LocalizedString.LoadedSuccessfully loadedSuccessfully;
	}
}
