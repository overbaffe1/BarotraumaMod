using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Barotrauma.IO;

namespace Barotrauma
{
	// Token: 0x0200038C RID: 908
	[NullableContext(1)]
	[Nullable(0)]
	public class Md5Hash
	{
		// Token: 0x06004456 RID: 17494 RVA: 0x00264000 File Offset: 0x00262200
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

		// Token: 0x06004457 RID: 17495 RVA: 0x00264050 File Offset: 0x00262250
		private static void CalculateHash(byte[] bytes, out string stringRepresentation, out byte[] byteRepresentation)
		{
			using (MD5 md5 = MD5.Create())
			{
				byte[] byteHash = md5.ComputeHash(bytes);
				byteRepresentation = byteHash;
				stringRepresentation = Md5Hash.ByteRepresentationToStringRepresentation(byteHash);
			}
		}

		// Token: 0x06004458 RID: 17496 RVA: 0x00264094 File Offset: 0x00262294
		private static string ByteRepresentationToStringRepresentation(byte[] byteHash)
		{
			return ToolBoxCore.ByteArrayToHexString(byteHash);
		}

		// Token: 0x06004459 RID: 17497 RVA: 0x0026409C File Offset: 0x0026229C
		private static byte[] StringRepresentationToByteRepresentation(string strHash)
		{
			return ToolBoxCore.HexStringToByteArray(strHash);
		}

		// Token: 0x0600445A RID: 17498 RVA: 0x002640A4 File Offset: 0x002622A4
		public static string GetShortHash(string fullHash)
		{
			if (fullHash.Length >= 7)
			{
				return fullHash.Substring(0, 7);
			}
			return fullHash;
		}

		// Token: 0x0600445B RID: 17499 RVA: 0x002640B9 File Offset: 0x002622B9
		private Md5Hash(string md5Hash)
		{
			this.StringRepresentation = md5Hash;
			this.ByteRepresentation = Md5Hash.StringRepresentationToByteRepresentation(this.StringRepresentation);
			this.ShortRepresentation = Md5Hash.GetShortHash(md5Hash);
		}

		// Token: 0x0600445C RID: 17500 RVA: 0x002640E8 File Offset: 0x002622E8
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

		// Token: 0x0600445D RID: 17501 RVA: 0x00264136 File Offset: 0x00262336
		public static Md5Hash StringAsHash(string hash)
		{
			if (!Md5Hash.stringHashRegex.IsMatch(hash))
			{
				throw new ArgumentException(hash + " is not a valid hash");
			}
			return new Md5Hash(hash);
		}

		// Token: 0x0600445E RID: 17502 RVA: 0x0026415C File Offset: 0x0026235C
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

		// Token: 0x0600445F RID: 17503 RVA: 0x002641DC File Offset: 0x002623DC
		public static Md5Hash CalculateForBytes(byte[] bytes)
		{
			return new Md5Hash(bytes, true);
		}

		// Token: 0x06004460 RID: 17504 RVA: 0x002641E5 File Offset: 0x002623E5
		public static Md5Hash BytesAsHash(byte[] bytes)
		{
			return new Md5Hash(bytes, false);
		}

		// Token: 0x06004461 RID: 17505 RVA: 0x002641F0 File Offset: 0x002623F0
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

		// Token: 0x06004462 RID: 17506 RVA: 0x00264248 File Offset: 0x00262448
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

		// Token: 0x06004463 RID: 17507 RVA: 0x0026429D File Offset: 0x0026249D
		public override string ToString()
		{
			return this.StringRepresentation;
		}

		// Token: 0x06004464 RID: 17508 RVA: 0x002642A8 File Offset: 0x002624A8
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

		// Token: 0x06004465 RID: 17509 RVA: 0x0026432A File Offset: 0x0026252A
		public override int GetHashCode()
		{
			return this.ShortRepresentation.GetHashCode(StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06004466 RID: 17510 RVA: 0x00264338 File Offset: 0x00262538
		[NullableContext(2)]
		public static bool operator ==(Md5Hash a, Md5Hash b)
		{
			return object.Equals(a, b);
		}

		// Token: 0x06004467 RID: 17511 RVA: 0x00264341 File Offset: 0x00262541
		[NullableContext(2)]
		public static bool operator !=(Md5Hash a, Md5Hash b)
		{
			return !(a == b);
		}

		// Token: 0x040023D1 RID: 9169
		public const int MaxHashLength = 32;

		// Token: 0x040023D2 RID: 9170
		public static readonly Md5Hash Blank = new Md5Hash(new string('0', 32));

		// Token: 0x040023D3 RID: 9171
		private static readonly Regex stringHashRegex = new Regex("^[0-9a-fA-F]{7,32}$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

		// Token: 0x040023D4 RID: 9172
		public readonly byte[] ByteRepresentation;

		// Token: 0x040023D5 RID: 9173
		public readonly string StringRepresentation;

		// Token: 0x040023D6 RID: 9174
		public readonly string ShortRepresentation;

		// Token: 0x020010B6 RID: 4278
		[NullableContext(0)]
		[Flags]
		public enum StringHashOptions
		{
			// Token: 0x04005978 RID: 22904
			BytePerfect = 0,
			// Token: 0x04005979 RID: 22905
			IgnoreCase = 1,
			// Token: 0x0400597A RID: 22906
			IgnoreWhitespace = 2
		}
	}
}
