using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x020002C1 RID: 705
	[NullableContext(1)]
	[Nullable(0)]
	public class Md5Hash
	{
		// Token: 0x06002FD8 RID: 12248 RVA: 0x0014A55C File Offset: 0x0014875C
		private static string RemoveWhitespace(string s)
		{
			StringBuilder sb = new StringBuilder(s.Length / 2);
			for (int i = 0; i < s.Length; i++)
			{
				if (!char.IsWhiteSpace(s[i]))
				{
					sb.Append(s[i]);
				}
			}
			return sb.ToString();
		}

		// Token: 0x06002FD9 RID: 12249 RVA: 0x0014A5AC File Offset: 0x001487AC
		private static void CalculateHash(byte[] bytes, out string stringRepresentation, out byte[] byteRepresentation)
		{
			using (MD5 md5 = MD5.Create())
			{
				byte[] byteHash = md5.ComputeHash(bytes);
				byteRepresentation = byteHash;
				stringRepresentation = Md5Hash.ByteRepresentationToStringRepresentation(byteHash);
			}
		}

		// Token: 0x06002FDA RID: 12250 RVA: 0x0014A5F0 File Offset: 0x001487F0
		private static string ByteRepresentationToStringRepresentation(byte[] byteHash)
		{
			return ToolBoxCore.ByteArrayToHexString(byteHash);
		}

		// Token: 0x06002FDB RID: 12251 RVA: 0x0014A5F8 File Offset: 0x001487F8
		private static byte[] StringRepresentationToByteRepresentation(string strHash)
		{
			return ToolBoxCore.HexStringToByteArray(strHash);
		}

		// Token: 0x06002FDC RID: 12252 RVA: 0x0014A600 File Offset: 0x00148800
		public static string GetShortHash(string fullHash)
		{
			if (fullHash.Length >= 7)
			{
				return fullHash.Substring(0, 7);
			}
			return fullHash;
		}

		// Token: 0x06002FDD RID: 12253 RVA: 0x0014A615 File Offset: 0x00148815
		private Md5Hash(string md5Hash)
		{
			this.StringRepresentation = md5Hash;
			this.ByteRepresentation = Md5Hash.StringRepresentationToByteRepresentation(this.StringRepresentation);
			this.ShortRepresentation = Md5Hash.GetShortHash(md5Hash);
		}

		// Token: 0x06002FDE RID: 12254 RVA: 0x0014A644 File Offset: 0x00148844
		private Md5Hash(byte[] bytes, bool calculate)
		{
			if (calculate)
			{
				Md5Hash.CalculateHash(bytes, out this.StringRepresentation, out this.ByteRepresentation);
			}
			else
			{
				this.StringRepresentation = Md5Hash.ByteRepresentationToStringRepresentation(bytes);
				this.ByteRepresentation = bytes;
			}
			this.ShortRepresentation = Md5Hash.GetShortHash(this.StringRepresentation);
		}

		// Token: 0x06002FDF RID: 12255 RVA: 0x0014A692 File Offset: 0x00148892
		public static Md5Hash StringAsHash(string hash)
		{
			if (!Md5Hash.stringHashRegex.IsMatch(hash))
			{
				throw new ArgumentException(hash + " is not a valid hash");
			}
			return new Md5Hash(hash);
		}

		// Token: 0x06002FE0 RID: 12256 RVA: 0x0014A6B8 File Offset: 0x001488B8
		public static Md5Hash MergeHashes(IEnumerable<Md5Hash> hashes)
		{
			Md5Hash result;
			using (IncrementalHash incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5))
			{
				foreach (Md5Hash hash in hashes)
				{
					incrementalHash.AppendData(hash.ByteRepresentation);
				}
				result = Md5Hash.BytesAsHash(incrementalHash.GetHashAndReset());
			}
			return result;
		}

		// Token: 0x06002FE1 RID: 12257 RVA: 0x0014A738 File Offset: 0x00148938
		public static Md5Hash CalculateForBytes(byte[] bytes)
		{
			return new Md5Hash(bytes, true);
		}

		// Token: 0x06002FE2 RID: 12258 RVA: 0x0014A741 File Offset: 0x00148941
		public static Md5Hash BytesAsHash(byte[] bytes)
		{
			return new Md5Hash(bytes, false);
		}

		// Token: 0x06002FE3 RID: 12259 RVA: 0x0014A74C File Offset: 0x0014894C
		public static Md5Hash CalculateForFile(string path, Md5Hash.StringHashOptions options)
		{
			if (options.HasFlag(Md5Hash.StringHashOptions.IgnoreWhitespace) || options.HasFlag(Md5Hash.StringHashOptions.IgnoreCase))
			{
				string str = File.ReadAllText(path, Encoding.UTF8, true);
				return Md5Hash.CalculateForString(str, options);
			}
			byte[] bytes = File.ReadAllBytes(path, true);
			return Md5Hash.CalculateForBytes(bytes);
		}

		// Token: 0x06002FE4 RID: 12260 RVA: 0x0014A7A4 File Offset: 0x001489A4
		public static Md5Hash CalculateForString(string str, Md5Hash.StringHashOptions options)
		{
			if (options.HasFlag(Md5Hash.StringHashOptions.IgnoreCase))
			{
				str = str.ToLowerInvariant();
			}
			if (options.HasFlag(Md5Hash.StringHashOptions.IgnoreWhitespace))
			{
				str = Md5Hash.RemoveWhitespace(str);
			}
			byte[] bytes = Encoding.UTF8.GetBytes(str);
			return Md5Hash.CalculateForBytes(bytes);
		}

		// Token: 0x06002FE5 RID: 12261 RVA: 0x0014A7F9 File Offset: 0x001489F9
		public override string ToString()
		{
			return this.StringRepresentation;
		}

		// Token: 0x06002FE6 RID: 12262 RVA: 0x0014A804 File Offset: 0x00148A04
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			Md5Hash md5Hash = obj as Md5Hash;
			if (md5Hash != null)
			{
				string otherStr = md5Hash.StringRepresentation;
				if (otherStr != null)
				{
					string selfStr = (otherStr.Length < this.StringRepresentation.Length) ? this.StringRepresentation.Substring(0, otherStr.Length) : this.StringRepresentation;
					otherStr = ((this.StringRepresentation.Length < otherStr.Length) ? otherStr.Substring(0, this.StringRepresentation.Length) : otherStr);
					return selfStr.Equals(otherStr, StringComparison.OrdinalIgnoreCase);
				}
			}
			return false;
		}

		// Token: 0x06002FE7 RID: 12263 RVA: 0x0014A886 File Offset: 0x00148A86
		public override int GetHashCode()
		{
			return this.ShortRepresentation.GetHashCode(StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06002FE8 RID: 12264 RVA: 0x0014A894 File Offset: 0x00148A94
		[NullableContext(2)]
		public static bool operator ==(Md5Hash a, Md5Hash b)
		{
			return object.Equals(a, b);
		}

		// Token: 0x06002FE9 RID: 12265 RVA: 0x0014A89D File Offset: 0x00148A9D
		[NullableContext(2)]
		public static bool operator !=(Md5Hash a, Md5Hash b)
		{
			return !(a == b);
		}

		// Token: 0x040017FF RID: 6143
		public const int MaxHashLength = 32;

		// Token: 0x04001800 RID: 6144
		public static readonly Md5Hash Blank = new Md5Hash(new string('0', 32));

		// Token: 0x04001801 RID: 6145
		private static readonly Regex stringHashRegex = new Regex("^[0-9a-fA-F]{7,32}$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Token: 0x04001802 RID: 6146
		public readonly byte[] ByteRepresentation;

		// Token: 0x04001803 RID: 6147
		public readonly string StringRepresentation;

		// Token: 0x04001804 RID: 6148
		public readonly string ShortRepresentation;

		// Token: 0x02000B65 RID: 2917
		[NullableContext(0)]
		[Flags]
		public enum StringHashOptions
		{
			// Token: 0x0400395B RID: 14683
			BytePerfect = 0,
			// Token: 0x0400395C RID: 14684
			IgnoreCase = 1,
			// Token: 0x0400395D RID: 14685
			IgnoreWhitespace = 2
		}
	}
}
