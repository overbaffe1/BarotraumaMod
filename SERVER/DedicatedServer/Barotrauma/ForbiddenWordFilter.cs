using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x020001CE RID: 462
	internal static class ForbiddenWordFilter
	{
		// Token: 0x06002235 RID: 8757 RVA: 0x000E5F34 File Offset: 0x000E4134
		static ForbiddenWordFilter()
		{
			try
			{
				ForbiddenWordFilter.forbiddenWords = (from s in File.ReadAllLines(ForbiddenWordFilter.fileListPath, null, false)
				select s.ToLowerInvariant()).ToHashSet<string>();
			}
			catch (IOException e)
			{
				DebugConsole.ThrowError("Failed to load the list of forbidden words from " + ForbiddenWordFilter.fileListPath + ".", e, null, false, false);
			}
		}

		// Token: 0x06002236 RID: 8758 RVA: 0x000E5FC0 File Offset: 0x000E41C0
		public static bool IsForbidden(string text)
		{
			string text2;
			return ForbiddenWordFilter.IsForbidden(text, out text2);
		}

		// Token: 0x06002237 RID: 8759 RVA: 0x000E5FD8 File Offset: 0x000E41D8
		public static bool IsForbidden(string text, out string forbiddenWord)
		{
			forbiddenWord = string.Empty;
			if (ForbiddenWordFilter.forbiddenWords == null)
			{
				return false;
			}
			char[] delimiters = new char[]
			{
				' ',
				'-',
				'.',
				'_',
				':',
				';',
				'\''
			};
			HashSet<string> words = new HashSet<string>();
			foreach (char delimiter in delimiters)
			{
				foreach (string word in text.Split(delimiter, StringSplitOptions.None))
				{
					words.Add(word.ToLowerInvariant());
				}
			}
			text = text.ToLowerInvariant();
			foreach (string forbidden in ForbiddenWordFilter.forbiddenWords)
			{
				if (forbidden.Contains(' '))
				{
					if (words.Contains(forbidden.Trim()))
					{
						forbiddenWord = forbidden.Trim();
						return true;
					}
				}
				else if (text.Contains(forbidden))
				{
					forbiddenWord = forbidden.Trim();
					return true;
				}
			}
			return false;
		}

		// Token: 0x04001066 RID: 4198
		private static readonly string fileListPath = Path.Combine(new string[]
		{
			"Data",
			"forbiddenwordlist.txt"
		});

		// Token: 0x04001067 RID: 4199
		private static readonly HashSet<string> forbiddenWords;
	}
}
