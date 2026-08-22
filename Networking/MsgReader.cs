using System;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020004A4 RID: 1188
	internal static class MsgReader
	{
		// Token: 0x06004EBB RID: 20155 RVA: 0x002AD664 File Offset: 0x002AB864
		internal static bool ReadBoolean(byte[] buf, ref int bitPos)
		{
			byte retval = NetBitWriter.ReadByte(buf, 1, bitPos);
			bitPos++;
			return retval > 0;
		}

		// Token: 0x06004EBC RID: 20156 RVA: 0x002AD688 File Offset: 0x002AB888
		internal static void ReadPadBits(ref int bitPos)
		{
			int bitOffset = bitPos % 8;
			bitPos += (8 - bitOffset) % 8;
		}

		// Token: 0x06004EBD RID: 20157 RVA: 0x002AD6A4 File Offset: 0x002AB8A4
		internal static byte ReadByte(byte[] buf, ref int bitPos)
		{
			byte retval = NetBitWriter.ReadByte(buf, 8, bitPos);
			bitPos += 8;
			return retval;
		}

		// Token: 0x06004EBE RID: 20158 RVA: 0x002AD6C4 File Offset: 0x002AB8C4
		internal static byte PeekByte(byte[] buf, ref int bitPos)
		{
			return NetBitWriter.ReadByte(buf, 8, bitPos);
		}

		// Token: 0x06004EBF RID: 20159 RVA: 0x002AD6DC File Offset: 0x002AB8DC
		internal static ushort ReadUInt16(byte[] buf, ref int bitPos)
		{
			uint retval = (uint)NetBitWriter.ReadUInt16(buf, 16, bitPos);
			bitPos += 16;
			return (ushort)retval;
		}

		// Token: 0x06004EC0 RID: 20160 RVA: 0x002AD6FD File Offset: 0x002AB8FD
		internal static short ReadInt16(byte[] buf, ref int bitPos)
		{
			return (short)MsgReader.ReadUInt16(buf, ref bitPos);
		}

		// Token: 0x06004EC1 RID: 20161 RVA: 0x002AD708 File Offset: 0x002AB908
		internal static uint ReadUInt32(byte[] buf, ref int bitPos)
		{
			uint retval = NetBitWriter.ReadUInt32(buf, 32, bitPos);
			bitPos += 32;
			return retval;
		}

		// Token: 0x06004EC2 RID: 20162 RVA: 0x002AD728 File Offset: 0x002AB928
		internal static int ReadInt32(byte[] buf, ref int bitPos)
		{
			return (int)MsgReader.ReadUInt32(buf, ref bitPos);
		}

		// Token: 0x06004EC3 RID: 20163 RVA: 0x002AD734 File Offset: 0x002AB934
		internal static ulong ReadUInt64(byte[] buf, ref int bitPos)
		{
			ulong low = (ulong)NetBitWriter.ReadUInt32(buf, 32, bitPos);
			bitPos += 32;
			ulong high = (ulong)NetBitWriter.ReadUInt32(buf, 32, bitPos);
			ulong retval = low + (high << 32);
			bitPos += 32;
			return retval;
		}

		// Token: 0x06004EC4 RID: 20164 RVA: 0x002AD76F File Offset: 0x002AB96F
		internal static long ReadInt64(byte[] buf, ref int bitPos)
		{
			return (long)MsgReader.ReadUInt64(buf, ref bitPos);
		}

		// Token: 0x06004EC5 RID: 20165 RVA: 0x002AD778 File Offset: 0x002AB978
		internal static float ReadSingle(byte[] buf, ref int bitPos)
		{
			if ((bitPos & 7) == 0)
			{
				float retval = BitConverter.ToSingle(buf, bitPos >> 3);
				bitPos += 32;
				return retval;
			}
			byte[] bytes = MsgReader.ReadBytes(buf, ref bitPos, 4);
			return BitConverter.ToSingle(bytes, 0);
		}

		// Token: 0x06004EC6 RID: 20166 RVA: 0x002AD7B0 File Offset: 0x002AB9B0
		internal static double ReadDouble(byte[] buf, ref int bitPos)
		{
			if ((bitPos & 7) == 0)
			{
				double retval = BitConverter.ToDouble(buf, bitPos >> 3);
				bitPos += 64;
				return retval;
			}
			byte[] bytes = MsgReader.ReadBytes(buf, ref bitPos, 8);
			return BitConverter.ToDouble(bytes, 0);
		}

		// Token: 0x06004EC7 RID: 20167 RVA: 0x002AD7E8 File Offset: 0x002AB9E8
		internal static Color ReadColorR8G8B8(byte[] buf, ref int bitPos)
		{
			byte r = MsgReader.ReadByte(buf, ref bitPos);
			byte g = MsgReader.ReadByte(buf, ref bitPos);
			byte b = MsgReader.ReadByte(buf, ref bitPos);
			return new Color(r, g, b, byte.MaxValue);
		}

		// Token: 0x06004EC8 RID: 20168 RVA: 0x002AD81C File Offset: 0x002ABA1C
		internal static Color ReadColorR8G8B8A8(byte[] buf, ref int bitPos)
		{
			byte r = MsgReader.ReadByte(buf, ref bitPos);
			byte g = MsgReader.ReadByte(buf, ref bitPos);
			byte b = MsgReader.ReadByte(buf, ref bitPos);
			byte a = MsgReader.ReadByte(buf, ref bitPos);
			return new Color(r, g, b, a);
		}

		// Token: 0x06004EC9 RID: 20169 RVA: 0x002AD854 File Offset: 0x002ABA54
		internal static uint ReadVariableUInt32(byte[] buf, ref int bitPos)
		{
			int bitLength = buf.Length * 8;
			int result = 0;
			int shift = 0;
			while (bitLength - bitPos >= 8)
			{
				byte chunk = MsgReader.ReadByte(buf, ref bitPos);
				result |= (int)(chunk & 127) << shift;
				shift += 7;
				if ((chunk & 128) == 0)
				{
					return (uint)result;
				}
			}
			return (uint)result;
		}

		// Token: 0x06004ECA RID: 20170 RVA: 0x002AD898 File Offset: 0x002ABA98
		internal static string ReadString(byte[] buf, ref int bitPos)
		{
			int bitLength = buf.Length * 8;
			int byteLen = (int)MsgReader.ReadVariableUInt32(buf, ref bitPos);
			if (byteLen <= 0)
			{
				return string.Empty;
			}
			if ((long)(bitLength - bitPos) < (long)byteLen * 8L)
			{
				return null;
			}
			if ((bitPos & 7) == 0)
			{
				string retval = Encoding.UTF8.GetString(buf, bitPos >> 3, byteLen);
				bitPos += 8 * byteLen;
				return retval;
			}
			byte[] bytes = MsgReader.ReadBytes(buf, ref bitPos, byteLen);
			return Encoding.UTF8.GetString(bytes, 0, bytes.Length);
		}

		// Token: 0x06004ECB RID: 20171 RVA: 0x002AD904 File Offset: 0x002ABB04
		internal static int ReadRangedInteger(byte[] buf, ref int bitPos, int min, int max)
		{
			uint range = (uint)(max - min);
			int numBits = NetUtility.BitsToHoldUInt(range);
			uint rvalue = NetBitWriter.ReadUInt32(buf, numBits, bitPos);
			bitPos += numBits;
			return (int)((long)min + (long)((ulong)rvalue));
		}

		// Token: 0x06004ECC RID: 20172 RVA: 0x002AD934 File Offset: 0x002ABB34
		internal static float ReadRangedSingle(byte[] buf, ref int bitPos, float min, float max, int bitCount)
		{
			int maxInt = (1 << bitCount) - 1;
			int intVal = MsgReader.ReadRangedInteger(buf, ref bitPos, 0, maxInt);
			float range = max - min;
			return min + range * (float)intVal / (float)maxInt;
		}

		// Token: 0x06004ECD RID: 20173 RVA: 0x002AD964 File Offset: 0x002ABB64
		internal static byte[] ReadBytes(byte[] buf, ref int bitPos, int numberOfBytes)
		{
			byte[] retval = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(buf, numberOfBytes, bitPos, retval, 0);
			bitPos += 8 * numberOfBytes;
			return retval;
		}
	}
}
