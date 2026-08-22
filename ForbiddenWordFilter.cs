using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x020002BD RID: 701
	internal static class ForbiddenWordFilter
	{
		// Token: 0x06003C5A RID: 15450 RVA: 0x00229DE4 File Offset: 0x00227FE4
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

		// Token: 0x06003C5B RID: 15451 RVA: 0x00229E70 File Offset: 0x00228070
		public static bool IsForbidden(string text)
		{
			string text2;
			return ForbiddenWordFilter.IsForbidden(text, out text2);
		}

		// Token: 0x06003C5C RID: 15452 RVA: 0x00229E88 File Offset: 0x00228088
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

		// Token: 0x04001F07 RID: 7943
		private static readonly string fileListPath = Path.Combine(new string[]
		{
			"Data",
			"forbiddenwordlist.txt"
		});

		// Token: 0x04001F08 RID: 7944
		private static readonly HashSet<string> forbiddenWords;
	}
}
