using System;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003A7 RID: 935
	internal static class MsgReader
	{
		// Token: 0x060036E6 RID: 14054 RVA: 0x00174B28 File Offset: 0x00172D28
		internal static bool ReadBoolean(byte[] buf, ref int bitPos)
		{
			byte retval = NetBitWriter.ReadByte(buf, 1, bitPos);
			bitPos++;
			return retval > 0;
		}

		// Token: 0x060036E7 RID: 14055 RVA: 0x00174B4C File Offset: 0x00172D4C
		internal static void ReadPadBits(ref int bitPos)
		{
			int bitOffset = bitPos % 8;
			bitPos += (8 - bitOffset) % 8;
		}

		// Token: 0x060036E8 RID: 14056 RVA: 0x00174B68 File Offset: 0x00172D68
		internal static byte ReadByte(byte[] buf, ref int bitPos)
		{
			byte retval = NetBitWriter.ReadByte(buf, 8, bitPos);
			bitPos += 8;
			return retval;
		}

		// Token: 0x060036E9 RID: 14057 RVA: 0x00174B88 File Offset: 0x00172D88
		internal static byte PeekByte(byte[] buf, ref int bitPos)
		{
			return NetBitWriter.ReadByte(buf, 8, bitPos);
		}

		// Token: 0x060036EA RID: 14058 RVA: 0x00174BA0 File Offset: 0x00172DA0
		internal static ushort ReadUInt16(byte[] buf, ref int bitPos)
		{
			uint retval = (uint)NetBitWriter.ReadUInt16(buf, 16, bitPos);
			bitPos += 16;
			return (ushort)retval;
		}

		// Token: 0x060036EB RID: 14059 RVA: 0x00174BC1 File Offset: 0x00172DC1
		internal static short ReadInt16(byte[] buf, ref int bitPos)
		{
			return (short)MsgReader.ReadUInt16(buf, ref bitPos);
		}

		// Token: 0x060036EC RID: 14060 RVA: 0x00174BCC File Offset: 0x00172DCC
		internal static uint ReadUInt32(byte[] buf, ref int bitPos)
		{
			uint retval = NetBitWriter.ReadUInt32(buf, 32, bitPos);
			bitPos += 32;
			return retval;
		}

		// Token: 0x060036ED RID: 14061 RVA: 0x00174BEC File Offset: 0x00172DEC
		internal static int ReadInt32(byte[] buf, ref int bitPos)
		{
			return (int)MsgReader.ReadUInt32(buf, ref bitPos);
		}

		// Token: 0x060036EE RID: 14062 RVA: 0x00174BF8 File Offset: 0x00172DF8
		internal static ulong ReadUInt64(byte[] buf, ref int bitPos)
		{
			ulong low = (ulong)NetBitWriter.ReadUInt32(buf, 32, bitPos);
			bitPos += 32;
			ulong high = (ulong)NetBitWriter.ReadUInt32(buf, 32, bitPos);
			ulong retval = low + (high << 32);
			bitPos += 32;
			return retval;
		}

		// Token: 0x060036EF RID: 14063 RVA: 0x00174C33 File Offset: 0x00172E33
		internal static long ReadInt64(byte[] buf, ref int bitPos)
		{
			return (long)MsgReader.ReadUInt64(buf, ref bitPos);
		}

		// Token: 0x060036F0 RID: 14064 RVA: 0x00174C3C File Offset: 0x00172E3C
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

		// Token: 0x060036F1 RID: 14065 RVA: 0x00174C74 File Offset: 0x00172E74
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

		// Token: 0x060036F2 RID: 14066 RVA: 0x00174CAC File Offset: 0x00172EAC
		internal static Color ReadColorR8G8B8(byte[] buf, ref int bitPos)
		{
			byte r = MsgReader.ReadByte(buf, ref bitPos);
			byte g = MsgReader.ReadByte(buf, ref bitPos);
			byte b = MsgReader.ReadByte(buf, ref bitPos);
			return new Color(r, g, b, byte.MaxValue);
		}

		// Token: 0x060036F3 RID: 14067 RVA: 0x00174CE0 File Offset: 0x00172EE0
		internal static Color ReadColorR8G8B8A8(byte[] buf, ref int bitPos)
		{
			byte r = MsgReader.ReadByte(buf, ref bitPos);
			byte g = MsgReader.ReadByte(buf, ref bitPos);
			byte b = MsgReader.ReadByte(buf, ref bitPos);
			byte a = MsgReader.ReadByte(buf, ref bitPos);
			return new Color(r, g, b, a);
		}

		// Token: 0x060036F4 RID: 14068 RVA: 0x00174D18 File Offset: 0x00172F18
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

		// Token: 0x060036F5 RID: 14069 RVA: 0x00174D5C File Offset: 0x00172F5C
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

		// Token: 0x060036F6 RID: 14070 RVA: 0x00174DC8 File Offset: 0x00172FC8
		internal static int ReadRangedInteger(byte[] buf, ref int bitPos, int min, int max)
		{
			uint range = (uint)(max - min);
			int numBits = NetUtility.BitsToHoldUInt(range);
			uint rvalue = NetBitWriter.ReadUInt32(buf, numBits, bitPos);
			bitPos += numBits;
			return (int)((long)min + (long)((ulong)rvalue));
		}

		// Token: 0x060036F7 RID: 14071 RVA: 0x00174DF8 File Offset: 0x00172FF8
		internal static float ReadRangedSingle(byte[] buf, ref int bitPos, float min, float max, int bitCount)
		{
			int maxInt = (1 << bitCount) - 1;
			int intVal = MsgReader.ReadRangedInteger(buf, ref bitPos, 0, maxInt);
			float range = max - min;
			return min + range * (float)intVal / (float)maxInt;
		}

		// Token: 0x060036F8 RID: 14072 RVA: 0x00174E28 File Offset: 0x00173028
		internal static byte[] ReadBytes(byte[] buf, ref int bitPos, int numberOfBytes)
		{
			byte[] retval = new byte[numberOfBytes];
			NetBitWriter.ReadBytes(buf, numberOfBytes, bitPos, retval, 0);
			bitPos += 8 * numberOfBytes;
			return retval;
		}
	}
}
