using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace RestSharp.Contrib
{
	// Token: 0x02000011 RID: 17
	internal class HttpEncoder
	{
		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000055 RID: 85 RVA: 0x000041A8 File Offset: 0x000023A8
		private static IDictionary<string, char> Entities
		{
			get
			{
				object obj = HttpEncoder.entitiesLock;
				IDictionary<string, char> result;
				lock (obj)
				{
					if (HttpEncoder.entities == null)
					{
						HttpEncoder.InitEntities();
					}
					result = HttpEncoder.entities;
				}
				return result;
			}
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000056 RID: 86 RVA: 0x000041F4 File Offset: 0x000023F4
		public static HttpEncoder Current
		{
			get
			{
				return HttpEncoder.currentEncoder;
			}
		}

		// Token: 0x1700000B RID: 11
		// (get) Token: 0x06000057 RID: 87 RVA: 0x000041FB File Offset: 0x000023FB
		public static HttpEncoder Default
		{
			get
			{
				return HttpEncoder.defaultEncoder;
			}
		}

		// Token: 0x0600005A RID: 90 RVA: 0x00004239 File Offset: 0x00002439
		internal static void HeaderNameValueEncode(string headerName, string headerValue, out string encodedHeaderName, out string encodedHeaderValue)
		{
			if (string.IsNullOrEmpty(headerName))
			{
				encodedHeaderName = headerName;
			}
			else
			{
				encodedHeaderName = HttpEncoder.EncodeHeaderString(headerName);
			}
			if (string.IsNullOrEmpty(headerValue))
			{
				encodedHeaderValue = headerValue;
				return;
			}
			encodedHeaderValue = HttpEncoder.EncodeHeaderString(headerValue);
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00004264 File Offset: 0x00002464
		private static void StringBuilderAppend(string s, ref StringBuilder sb)
		{
			if (sb == null)
			{
				sb = new StringBuilder(s);
				return;
			}
			sb.Append(s);
		}

		// Token: 0x0600005C RID: 92 RVA: 0x0000427C File Offset: 0x0000247C
		private static string EncodeHeaderString(string input)
		{
			StringBuilder sb = null;
			foreach (char ch in input)
			{
				if ((ch < ' ' && ch != '\t') || ch == '\u007f')
				{
					HttpEncoder.StringBuilderAppend(string.Format("%{0:x2}", (int)ch), ref sb);
				}
			}
			if (sb != null)
			{
				return sb.ToString();
			}
			return input;
		}

		// Token: 0x0600005D RID: 93 RVA: 0x000042D8 File Offset: 0x000024D8
		internal static string UrlPathEncode(string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}
			MemoryStream result = new MemoryStream();
			int length = value.Length;
			for (int i = 0; i < length; i++)
			{
				HttpEncoder.UrlPathEncodeChar(value[i], result);
			}
			return Encoding.ASCII.GetString(result.ToArray());
		}

		// Token: 0x0600005E RID: 94 RVA: 0x00004328 File Offset: 0x00002528
		internal static byte[] UrlEncodeToBytes(byte[] bytes, int offset, int count)
		{
			if (bytes == null)
			{
				throw new ArgumentNullException("bytes");
			}
			int blen = bytes.Length;
			if (blen == 0)
			{
				return Array.Empty<byte>();
			}
			if (offset < 0 || offset >= blen)
			{
				throw new ArgumentOutOfRangeException("offset");
			}
			if (count < 0 || count > blen - offset)
			{
				throw new ArgumentOutOfRangeException("count");
			}
			MemoryStream result = new MemoryStream(count);
			int end = offset + count;
			for (int i = offset; i < end; i++)
			{
				HttpEncoder.UrlEncodeChar((char)bytes[i], result, false);
			}
			return result.ToArray();
		}

		// Token: 0x0600005F RID: 95 RVA: 0x000043A0 File Offset: 0x000025A0
		internal static string HtmlEncode(string s)
		{
			if (s == null)
			{
				return null;
			}
			if (s.Length == 0)
			{
				return string.Empty;
			}
			bool needEncode = false;
			foreach (char c in s)
			{
				if (c == '&' || c == '"' || c == '<' || c == '>' || c > '\u009f')
				{
					needEncode = true;
					break;
				}
			}
			if (!needEncode)
			{
				return s;
			}
			StringBuilder output = new StringBuilder();
			int len = s.Length;
			int j = 0;
			while (j < len)
			{
				char c2 = s[j];
				if (c2 <= '<')
				{
					if (c2 != '"')
					{
						if (c2 != '&')
						{
							if (c2 != '<')
							{
								goto IL_10B;
							}
							output.Append("&lt;");
						}
						else
						{
							output.Append("&amp;");
						}
					}
					else
					{
						output.Append("&quot;");
					}
				}
				else if (c2 != '>')
				{
					if (c2 != '＜')
					{
						if (c2 != '＞')
						{
							goto IL_10B;
						}
						output.Append("&#65310;");
					}
					else
					{
						output.Append("&#65308;");
					}
				}
				else
				{
					output.Append("&gt;");
				}
				IL_15C:
				j++;
				continue;
				IL_10B:
				char ch = s[j];
				if (ch > '\u009f' && ch < 'Ā')
				{
					output.Append("&#");
					StringBuilder stringBuilder = output;
					int num = (int)ch;
					stringBuilder.Append(num.ToString(CultureInfo.InvariantCulture));
					output.Append(";");
					goto IL_15C;
				}
				output.Append(ch);
				goto IL_15C;
			}
			return output.ToString();
		}

		// Token: 0x06000060 RID: 96 RVA: 0x00004520 File Offset: 0x00002720
		internal static string HtmlAttributeEncode(string s)
		{
			if (s == null)
			{
				return null;
			}
			if (s.Length == 0)
			{
				return string.Empty;
			}
			bool needEncode = false;
			foreach (char c in s)
			{
				if (c == '&' || c == '"' || c == '<')
				{
					needEncode = true;
					break;
				}
			}
			if (!needEncode)
			{
				return s;
			}
			StringBuilder output = new StringBuilder();
			int len = s.Length;
			for (int j = 0; j < len; j++)
			{
				char c2 = s[j];
				if (c2 != '"')
				{
					if (c2 != '&')
					{
						if (c2 != '<')
						{
							output.Append(s[j]);
						}
						else
						{
							output.Append("&lt;");
						}
					}
					else
					{
						output.Append("&amp;");
					}
				}
				else
				{
					output.Append("&quot;");
				}
			}
			return output.ToString();
		}

		// Token: 0x06000061 RID: 97 RVA: 0x000045F4 File Offset: 0x000027F4
		internal static string HtmlDecode(string s)
		{
			if (s == null)
			{
				return null;
			}
			if (s.Length == 0)
			{
				return string.Empty;
			}
			if (s.IndexOf('&') == -1)
			{
				return s;
			}
			StringBuilder entity = new StringBuilder();
			StringBuilder output = new StringBuilder();
			int len = s.Length;
			int state = 0;
			int number = 0;
			bool is_hex_value = false;
			bool have_trailing_digits = false;
			for (int i = 0; i < len; i++)
			{
				char c = s[i];
				if (state == 0)
				{
					if (c == '&')
					{
						entity.Append(c);
						state = 1;
					}
					else
					{
						output.Append(c);
					}
				}
				else if (c == '&')
				{
					state = 1;
					if (have_trailing_digits)
					{
						entity.Append(number.ToString(CultureInfo.InvariantCulture));
						have_trailing_digits = false;
					}
					output.Append(entity.ToString());
					entity.Length = 0;
					entity.Append('&');
				}
				else if (state == 1)
				{
					if (c == ';')
					{
						state = 0;
						output.Append(entity.ToString());
						output.Append(c);
						entity.Length = 0;
					}
					else
					{
						number = 0;
						is_hex_value = false;
						if (c != '#')
						{
							state = 2;
						}
						else
						{
							state = 3;
						}
						entity.Append(c);
					}
				}
				else if (state == 2)
				{
					entity.Append(c);
					if (c == ';')
					{
						string key = entity.ToString();
						if (key.Length > 1 && HttpEncoder.Entities.ContainsKey(key.Substring(1, key.Length - 2)))
						{
							key = HttpEncoder.Entities[key.Substring(1, key.Length - 2)].ToString();
						}
						output.Append(key);
						state = 0;
						entity.Length = 0;
					}
				}
				else if (state == 3)
				{
					if (c == ';')
					{
						if (number > 65535)
						{
							output.Append("&#");
							output.Append(number.ToString(CultureInfo.InvariantCulture));
							output.Append(";");
						}
						else
						{
							output.Append((char)number);
						}
						state = 0;
						entity.Length = 0;
						have_trailing_digits = false;
					}
					else if (is_hex_value && Uri.IsHexDigit(c))
					{
						number = number * 16 + Uri.FromHex(c);
						have_trailing_digits = true;
					}
					else if (char.IsDigit(c))
					{
						number = number * 10 + (int)(c - '0');
						have_trailing_digits = true;
					}
					else if (number == 0 && (c == 'x' || c == 'X'))
					{
						is_hex_value = true;
					}
					else
					{
						state = 2;
						if (have_trailing_digits)
						{
							entity.Append(number.ToString(CultureInfo.InvariantCulture));
							have_trailing_digits = false;
						}
						entity.Append(c);
					}
				}
			}
			if (entity.Length > 0)
			{
				output.Append(entity.ToString());
			}
			else if (have_trailing_digits)
			{
				output.Append(number.ToString(CultureInfo.InvariantCulture));
			}
			return output.ToString();
		}

		// Token: 0x06000062 RID: 98 RVA: 0x000048A0 File Offset: 0x00002AA0
		internal static bool NotEncoded(char c)
		{
			return c == '!' || c == '(' || c == ')' || c == '*' || c == '-' || c == '.' || c == '_';
		}

		// Token: 0x06000063 RID: 99 RVA: 0x000048C8 File Offset: 0x00002AC8
		internal static void UrlEncodeChar(char c, Stream result, bool isUnicode)
		{
			if (c > 'ÿ')
			{
				result.WriteByte(37);
				result.WriteByte(117);
				int idx = (int)(c >> 12);
				result.WriteByte((byte)HttpEncoder.hexChars[idx]);
				idx = (int)(c >> 8 & '\u000f');
				result.WriteByte((byte)HttpEncoder.hexChars[idx]);
				idx = (int)(c >> 4 & '\u000f');
				result.WriteByte((byte)HttpEncoder.hexChars[idx]);
				idx = (int)(c & '\u000f');
				result.WriteByte((byte)HttpEncoder.hexChars[idx]);
				return;
			}
			if (c > ' ' && HttpEncoder.NotEncoded(c))
			{
				result.WriteByte((byte)c);
				return;
			}
			if (c == ' ')
			{
				result.WriteByte(43);
				return;
			}
			if (c < '0' || (c < 'A' && c > '9') || (c > 'Z' && c < 'a') || c > 'z')
			{
				if (isUnicode && c > '\u007f')
				{
					result.WriteByte(37);
					result.WriteByte(117);
					result.WriteByte(48);
					result.WriteByte(48);
				}
				else
				{
					result.WriteByte(37);
				}
				int idx2 = (int)(c >> 4);
				result.WriteByte((byte)HttpEncoder.hexChars[idx2]);
				idx2 = (int)(c & '\u000f');
				result.WriteByte((byte)HttpEncoder.hexChars[idx2]);
				return;
			}
			result.WriteByte((byte)c);
		}

		// Token: 0x06000064 RID: 100 RVA: 0x000049E4 File Offset: 0x00002BE4
		internal static void UrlPathEncodeChar(char c, Stream result)
		{
			if (c < '!' || c > '~')
			{
				byte[] bIn = Encoding.UTF8.GetBytes(c.ToString());
				for (int i = 0; i < bIn.Length; i++)
				{
					result.WriteByte(37);
					int idx = bIn[i] >> 4;
					result.WriteByte((byte)HttpEncoder.hexChars[idx]);
					idx = (int)(bIn[i] & 15);
					result.WriteByte((byte)HttpEncoder.hexChars[idx]);
				}
				return;
			}
			if (c == ' ')
			{
				result.WriteByte(37);
				result.WriteByte(50);
				result.WriteByte(48);
				return;
			}
			result.WriteByte((byte)c);
		}

		// Token: 0x06000065 RID: 101 RVA: 0x00004A74 File Offset: 0x00002C74
		private static void InitEntities()
		{
			HttpEncoder.entities = new SortedDictionary<string, char>(StringComparer.Ordinal);
			HttpEncoder.entities.Add("nbsp", '\u00a0');
			HttpEncoder.entities.Add("iexcl", '¡');
			HttpEncoder.entities.Add("cent", '¢');
			HttpEncoder.entities.Add("pound", '£');
			HttpEncoder.entities.Add("curren", '¤');
			HttpEncoder.entities.Add("yen", '¥');
			HttpEncoder.entities.Add("brvbar", '¦');
			HttpEncoder.entities.Add("sect", '§');
			HttpEncoder.entities.Add("uml", '¨');
			HttpEncoder.entities.Add("copy", '©');
			HttpEncoder.entities.Add("ordf", 'ª');
			HttpEncoder.entities.Add("laquo", '«');
			HttpEncoder.entities.Add("not", '¬');
			HttpEncoder.entities.Add("shy", '­');
			HttpEncoder.entities.Add("reg", '®');
			HttpEncoder.entities.Add("macr", '¯');
			HttpEncoder.entities.Add("deg", '°');
			HttpEncoder.entities.Add("plusmn", '±');
			HttpEncoder.entities.Add("sup2", '²');
			HttpEncoder.entities.Add("sup3", '³');
			HttpEncoder.entities.Add("acute", '´');
			HttpEncoder.entities.Add("micro", 'µ');
			HttpEncoder.entities.Add("para", '¶');
			HttpEncoder.entities.Add("middot", '·');
			HttpEncoder.entities.Add("cedil", '¸');
			HttpEncoder.entities.Add("sup1", '¹');
			HttpEncoder.entities.Add("ordm", 'º');
			HttpEncoder.entities.Add("raquo", '»');
			HttpEncoder.entities.Add("frac14", '¼');
			HttpEncoder.entities.Add("frac12", '½');
			HttpEncoder.entities.Add("frac34", '¾');
			HttpEncoder.entities.Add("iquest", '¿');
			HttpEncoder.entities.Add("Agrave", 'À');
			HttpEncoder.entities.Add("Aacute", 'Á');
			HttpEncoder.entities.Add("Acirc", 'Â');
			HttpEncoder.entities.Add("Atilde", 'Ã');
			HttpEncoder.entities.Add("Auml", 'Ä');
			HttpEncoder.entities.Add("Aring", 'Å');
			HttpEncoder.entities.Add("AElig", 'Æ');
			HttpEncoder.entities.Add("Ccedil", 'Ç');
			HttpEncoder.entities.Add("Egrave", 'È');
			HttpEncoder.entities.Add("Eacute", 'É');
			HttpEncoder.entities.Add("Ecirc", 'Ê');
			HttpEncoder.entities.Add("Euml", 'Ë');
			HttpEncoder.entities.Add("Igrave", 'Ì');
			HttpEncoder.entities.Add("Iacute", 'Í');
			HttpEncoder.entities.Add("Icirc", 'Î');
			HttpEncoder.entities.Add("Iuml", 'Ï');
			HttpEncoder.entities.Add("ETH", 'Ð');
			HttpEncoder.entities.Add("Ntilde", 'Ñ');
			HttpEncoder.entities.Add("Ograve", 'Ò');
			HttpEncoder.entities.Add("Oacute", 'Ó');
			HttpEncoder.entities.Add("Ocirc", 'Ô');
			HttpEncoder.entities.Add("Otilde", 'Õ');
			HttpEncoder.entities.Add("Ouml", 'Ö');
			HttpEncoder.entities.Add("times", '×');
			HttpEncoder.entities.Add("Oslash", 'Ø');
			HttpEncoder.entities.Add("Ugrave", 'Ù');
			HttpEncoder.entities.Add("Uacute", 'Ú');
			HttpEncoder.entities.Add("Ucirc", 'Û');
			HttpEncoder.entities.Add("Uuml", 'Ü');
			HttpEncoder.entities.Add("Yacute", 'Ý');
			HttpEncoder.entities.Add("THORN", 'Þ');
			HttpEncoder.entities.Add("szlig", 'ß');
			HttpEncoder.entities.Add("agrave", 'à');
			HttpEncoder.entities.Add("aacute", 'á');
			HttpEncoder.entities.Add("acirc", 'â');
			HttpEncoder.entities.Add("atilde", 'ã');
			HttpEncoder.entities.Add("auml", 'ä');
			HttpEncoder.entities.Add("aring", 'å');
			HttpEncoder.entities.Add("aelig", 'æ');
			HttpEncoder.entities.Add("ccedil", 'ç');
			HttpEncoder.entities.Add("egrave", 'è');
			HttpEncoder.entities.Add("eacute", 'é');
			HttpEncoder.entities.Add("ecirc", 'ê');
			HttpEncoder.entities.Add("euml", 'ë');
			HttpEncoder.entities.Add("igrave", 'ì');
			HttpEncoder.entities.Add("iacute", 'í');
			HttpEncoder.entities.Add("icirc", 'î');
			HttpEncoder.entities.Add("iuml", 'ï');
			HttpEncoder.entities.Add("eth", 'ð');
			HttpEncoder.entities.Add("ntilde", 'ñ');
			HttpEncoder.entities.Add("ograve", 'ò');
			HttpEncoder.entities.Add("oacute", 'ó');
			HttpEncoder.entities.Add("ocirc", 'ô');
			HttpEncoder.entities.Add("otilde", 'õ');
			HttpEncoder.entities.Add("ouml", 'ö');
			HttpEncoder.entities.Add("divide", '÷');
			HttpEncoder.entities.Add("oslash", 'ø');
			HttpEncoder.entities.Add("ugrave", 'ù');
			HttpEncoder.entities.Add("uacute", 'ú');
			HttpEncoder.entities.Add("ucirc", 'û');
			HttpEncoder.entities.Add("uuml", 'ü');
			HttpEncoder.entities.Add("yacute", 'ý');
			HttpEncoder.entities.Add("thorn", 'þ');
			HttpEncoder.entities.Add("yuml", 'ÿ');
			HttpEncoder.entities.Add("fnof", 'ƒ');
			HttpEncoder.entities.Add("Alpha", 'Α');
			HttpEncoder.entities.Add("Beta", 'Β');
			HttpEncoder.entities.Add("Gamma", 'Γ');
			HttpEncoder.entities.Add("Delta", 'Δ');
			HttpEncoder.entities.Add("Epsilon", 'Ε');
			HttpEncoder.entities.Add("Zeta", 'Ζ');
			HttpEncoder.entities.Add("Eta", 'Η');
			HttpEncoder.entities.Add("Theta", 'Θ');
			HttpEncoder.entities.Add("Iota", 'Ι');
			HttpEncoder.entities.Add("Kappa", 'Κ');
			HttpEncoder.entities.Add("Lambda", 'Λ');
			HttpEncoder.entities.Add("Mu", 'Μ');
			HttpEncoder.entities.Add("Nu", 'Ν');
			HttpEncoder.entities.Add("Xi", 'Ξ');
			HttpEncoder.entities.Add("Omicron", 'Ο');
			HttpEncoder.entities.Add("Pi", 'Π');
			HttpEncoder.entities.Add("Rho", 'Ρ');
			HttpEncoder.entities.Add("Sigma", 'Σ');
			HttpEncoder.entities.Add("Tau", 'Τ');
			HttpEncoder.entities.Add("Upsilon", 'Υ');
			HttpEncoder.entities.Add("Phi", 'Φ');
			HttpEncoder.entities.Add("Chi", 'Χ');
			HttpEncoder.entities.Add("Psi", 'Ψ');
			HttpEncoder.entities.Add("Omega", 'Ω');
			HttpEncoder.entities.Add("alpha", 'α');
			HttpEncoder.entities.Add("beta", 'β');
			HttpEncoder.entities.Add("gamma", 'γ');
			HttpEncoder.entities.Add("delta", 'δ');
			HttpEncoder.entities.Add("epsilon", 'ε');
			HttpEncoder.entities.Add("zeta", 'ζ');
			HttpEncoder.entities.Add("eta", 'η');
			HttpEncoder.entities.Add("theta", 'θ');
			HttpEncoder.entities.Add("iota", 'ι');
			HttpEncoder.entities.Add("kappa", 'κ');
			HttpEncoder.entities.Add("lambda", 'λ');
			HttpEncoder.entities.Add("mu", 'μ');
			HttpEncoder.entities.Add("nu", 'ν');
			HttpEncoder.entities.Add("xi", 'ξ');
			HttpEncoder.entities.Add("omicron", 'ο');
			HttpEncoder.entities.Add("pi", 'π');
			HttpEncoder.entities.Add("rho", 'ρ');
			HttpEncoder.entities.Add("sigmaf", 'ς');
			HttpEncoder.entities.Add("sigma", 'σ');
			HttpEncoder.entities.Add("tau", 'τ');
			HttpEncoder.entities.Add("upsilon", 'υ');
			HttpEncoder.entities.Add("phi", 'φ');
			HttpEncoder.entities.Add("chi", 'χ');
			HttpEncoder.entities.Add("psi", 'ψ');
			HttpEncoder.entities.Add("omega", 'ω');
			HttpEncoder.entities.Add("thetasym", 'ϑ');
			HttpEncoder.entities.Add("upsih", 'ϒ');
			HttpEncoder.entities.Add("piv", 'ϖ');
			HttpEncoder.entities.Add("bull", '•');
			HttpEncoder.entities.Add("hellip", '…');
			HttpEncoder.entities.Add("prime", '′');
			HttpEncoder.entities.Add("Prime", '″');
			HttpEncoder.entities.Add("oline", '‾');
			HttpEncoder.entities.Add("frasl", '⁄');
			HttpEncoder.entities.Add("weierp", '℘');
			HttpEncoder.entities.Add("image", 'ℑ');
			HttpEncoder.entities.Add("real", 'ℜ');
			HttpEncoder.entities.Add("trade", '™');
			HttpEncoder.entities.Add("alefsym", 'ℵ');
			HttpEncoder.entities.Add("larr", '←');
			HttpEncoder.entities.Add("uarr", '↑');
			HttpEncoder.entities.Add("rarr", '→');
			HttpEncoder.entities.Add("darr", '↓');
			HttpEncoder.entities.Add("harr", '↔');
			HttpEncoder.entities.Add("crarr", '↵');
			HttpEncoder.entities.Add("lArr", '⇐');
			HttpEncoder.entities.Add("uArr", '⇑');
			HttpEncoder.entities.Add("rArr", '⇒');
			HttpEncoder.entities.Add("dArr", '⇓');
			HttpEncoder.entities.Add("hArr", '⇔');
			HttpEncoder.entities.Add("forall", '∀');
			HttpEncoder.entities.Add("part", '∂');
			HttpEncoder.entities.Add("exist", '∃');
			HttpEncoder.entities.Add("empty", '∅');
			HttpEncoder.entities.Add("nabla", '∇');
			HttpEncoder.entities.Add("isin", '∈');
			HttpEncoder.entities.Add("notin", '∉');
			HttpEncoder.entities.Add("ni", '∋');
			HttpEncoder.entities.Add("prod", '∏');
			HttpEncoder.entities.Add("sum", '∑');
			HttpEncoder.entities.Add("minus", '−');
			HttpEncoder.entities.Add("lowast", '∗');
			HttpEncoder.entities.Add("radic", '√');
			HttpEncoder.entities.Add("prop", '∝');
			HttpEncoder.entities.Add("infin", '∞');
			HttpEncoder.entities.Add("ang", '∠');
			HttpEncoder.entities.Add("and", '∧');
			HttpEncoder.entities.Add("or", '∨');
			HttpEncoder.entities.Add("cap", '∩');
			HttpEncoder.entities.Add("cup", '∪');
			HttpEncoder.entities.Add("int", '∫');
			HttpEncoder.entities.Add("there4", '∴');
			HttpEncoder.entities.Add("sim", '∼');
			HttpEncoder.entities.Add("cong", '≅');
			HttpEncoder.entities.Add("asymp", '≈');
			HttpEncoder.entities.Add("ne", '≠');
			HttpEncoder.entities.Add("equiv", '≡');
			HttpEncoder.entities.Add("le", '≤');
			HttpEncoder.entities.Add("ge", '≥');
			HttpEncoder.entities.Add("sub", '⊂');
			HttpEncoder.entities.Add("sup", '⊃');
			HttpEncoder.entities.Add("nsub", '⊄');
			HttpEncoder.entities.Add("sube", '⊆');
			HttpEncoder.entities.Add("supe", '⊇');
			HttpEncoder.entities.Add("oplus", '⊕');
			HttpEncoder.entities.Add("otimes", '⊗');
			HttpEncoder.entities.Add("perp", '⊥');
			HttpEncoder.entities.Add("sdot", '⋅');
			HttpEncoder.entities.Add("lceil", '⌈');
			HttpEncoder.entities.Add("rceil", '⌉');
			HttpEncoder.entities.Add("lfloor", '⌊');
			HttpEncoder.entities.Add("rfloor", '⌋');
			HttpEncoder.entities.Add("lang", '〈');
			HttpEncoder.entities.Add("rang", '〉');
			HttpEncoder.entities.Add("loz", '◊');
			HttpEncoder.entities.Add("spades", '♠');
			HttpEncoder.entities.Add("clubs", '♣');
			HttpEncoder.entities.Add("hearts", '♥');
			HttpEncoder.entities.Add("diams", '♦');
			HttpEncoder.entities.Add("quot", '"');
			HttpEncoder.entities.Add("amp", '&');
			HttpEncoder.entities.Add("lt", '<');
			HttpEncoder.entities.Add("gt", '>');
			HttpEncoder.entities.Add("OElig", 'Œ');
			HttpEncoder.entities.Add("oelig", 'œ');
			HttpEncoder.entities.Add("Scaron", 'Š');
			HttpEncoder.entities.Add("scaron", 'š');
			HttpEncoder.entities.Add("Yuml", 'Ÿ');
			HttpEncoder.entities.Add("circ", 'ˆ');
			HttpEncoder.entities.Add("tilde", '˜');
			HttpEncoder.entities.Add("ensp", '\u2002');
			HttpEncoder.entities.Add("emsp", '\u2003');
			HttpEncoder.entities.Add("thinsp", '\u2009');
			HttpEncoder.entities.Add("zwnj", '‌');
			HttpEncoder.entities.Add("zwj", '‍');
			HttpEncoder.entities.Add("lrm", '‎');
			HttpEncoder.entities.Add("rlm", '‏');
			HttpEncoder.entities.Add("ndash", '–');
			HttpEncoder.entities.Add("mdash", '—');
			HttpEncoder.entities.Add("lsquo", '‘');
			HttpEncoder.entities.Add("rsquo", '’');
			HttpEncoder.entities.Add("sbquo", '‚');
			HttpEncoder.entities.Add("ldquo", '“');
			HttpEncoder.entities.Add("rdquo", '”');
			HttpEncoder.entities.Add("bdquo", '„');
			HttpEncoder.entities.Add("dagger", '†');
			HttpEncoder.entities.Add("Dagger", '‡');
			HttpEncoder.entities.Add("permil", '‰');
			HttpEncoder.entities.Add("lsaquo", '‹');
			HttpEncoder.entities.Add("rsaquo", '›');
			HttpEncoder.entities.Add("euro", '€');
		}

		// Token: 0x0400005C RID: 92
		private static char[] hexChars = "0123456789abcdef".ToCharArray();

		// Token: 0x0400005D RID: 93
		private static object entitiesLock = new object();

		// Token: 0x0400005E RID: 94
		private static SortedDictionary<string, char> entities;

		// Token: 0x0400005F RID: 95
		private static HttpEncoder defaultEncoder = new HttpEncoder();

		// Token: 0x04000060 RID: 96
		private static HttpEncoder currentEncoder = HttpEncoder.defaultEncoder;
	}
}
