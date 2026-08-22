using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Barotrauma
{
	// Token: 0x0200036E RID: 878
	[NullableContext(1)]
	[Nullable(0)]
	public class ServerMsgLString : LocalizedString
	{
		// Token: 0x0600435D RID: 17245 RVA: 0x00252D1C File Offset: 0x00250F1C
		public ServerMsgLString(string serverMsg)
		{
			this.serverMessage = serverMsg;
			this.messageSplit = this.serverMessage.Split("/", StringSplitOptions.None).ToImmutableArray<string>();
		}

		// Token: 0x0600435E RID: 17246 RVA: 0x00252D4C File Offset: 0x00250F4C
		private static bool IsServerMessageWithVariables(string message)
		{
			return ServerMsgLString.serverMessageCharacters.All(new Func<char, bool>(message.Contains));
		}

		// Token: 0x170011A6 RID: 4518
		// (get) Token: 0x0600435F RID: 17247 RVA: 0x00252D64 File Offset: 0x00250F64
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

		// Token: 0x06004360 RID: 17248 RVA: 0x00252D80 File Offset: 0x00250F80
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

		// Token: 0x06004362 RID: 17250 RVA: 0x00252ED0 File Offset: 0x002510D0
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

		// Token: 0x0400234F RID: 9039
		private static readonly Regex reFormattedMessage = new Regex("^(?<variable>[\\[\\].a-z0-9_]+?)=(?<formatter>[a-z0-9_]+?)\\((?<value>.+?)\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		// Token: 0x04002350 RID: 9040
		private static readonly Regex reReplacedMessage = new Regex("^(?<variable>[\\[\\].a-z0-9_]+?)=(?<message>.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		// Token: 0x04002351 RID: 9041
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

		// Token: 0x04002352 RID: 9042
		private static readonly ImmutableHashSet<char> serverMessageCharacters = new char[]
		{
			'~',
			'[',
			']',
			'='
		}.ToImmutableHashSet<char>();

		// Token: 0x04002353 RID: 9043
		private readonly string serverMessage;

		// Token: 0x04002354 RID: 9044
		[Nullable(new byte[]
		{
			0,
			1
		})]
		private readonly ImmutableArray<string> messageSplit;

		// Token: 0x04002355 RID: 9045
		private LocalizedString.LoadedSuccessfully loadedSuccessfully;
	}
}
