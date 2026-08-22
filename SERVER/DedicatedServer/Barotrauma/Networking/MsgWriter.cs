using System;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020003A6 RID: 934
	internal static class MsgWriter
	{
		// Token: 0x060036D2 RID: 14034 RVA: 0x001746FB File Offset: 0x001728FB
		internal static void UpdateBitLength(ref int bitLength, int bitPos)
		{
			bitLength = Math.Max(bitLength, bitPos);
		}

		// Token: 0x060036D3 RID: 14035 RVA: 0x00174708 File Offset: 0x00172908
		internal static void WriteBoolean(ref byte[] buf, ref int bitPos, ref int bitLength, bool val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 1);
			int bytePos = bitPos / 8;
			int bitOffset = bitPos % 8;
			byte bitFlag = (byte)(1 << bitOffset);
			byte bitMask = ~bitFlag & byte.MaxValue;
			byte[] array = buf;
			int num = bytePos;
			array[num] &= bitMask;
			if (val)
			{
				byte[] array2 = buf;
				int num2 = bytePos;
				array2[num2] |= bitFlag;
			}
			bitPos++;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036D4 RID: 14036 RVA: 0x00174768 File Offset: 0x00172968
		internal static void WritePadBits(ref byte[] buf, ref int bitPos, ref int bitLength)
		{
			int bitOffset = bitPos % 8;
			bitPos += (8 - bitOffset) % 8;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
			MsgWriter.EnsureBufferSize(ref buf, bitPos);
		}

		// Token: 0x060036D5 RID: 14037 RVA: 0x00174794 File Offset: 0x00172994
		internal static void WriteByte(ref byte[] buf, ref int bitPos, ref int bitLength, byte val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 8);
			NetBitWriter.WriteByte(val, 8, buf, bitPos);
			bitPos += 8;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036D6 RID: 14038 RVA: 0x001747B9 File Offset: 0x001729B9
		internal static void WriteUInt16(ref byte[] buf, ref int bitPos, ref int bitLength, ushort val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 16);
			NetBitWriter.WriteUInt16(val, 16, buf, bitPos);
			bitPos += 16;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036D7 RID: 14039 RVA: 0x001747E1 File Offset: 0x001729E1
		internal static void WriteInt16(ref byte[] buf, ref int bitPos, ref int bitLength, short val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 16);
			NetBitWriter.WriteUInt16((ushort)val, 16, buf, bitPos);
			bitPos += 16;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036D8 RID: 14040 RVA: 0x0017480A File Offset: 0x00172A0A
		internal static void WriteUInt32(ref byte[] buf, ref int bitPos, ref int bitLength, uint val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 32);
			NetBitWriter.WriteUInt32(val, 32, buf, bitPos);
			bitPos += 32;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036D9 RID: 14041 RVA: 0x00174833 File Offset: 0x00172A33
		internal static void WriteInt32(ref byte[] buf, ref int bitPos, ref int bitLength, int val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 32);
			NetBitWriter.WriteUInt32((uint)val, 32, buf, bitPos);
			bitPos += 32;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036DA RID: 14042 RVA: 0x0017485C File Offset: 0x00172A5C
		internal static void WriteUInt64(ref byte[] buf, ref int bitPos, ref int bitLength, ulong val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 64);
			NetBitWriter.WriteUInt64(val, 64, buf, bitPos);
			bitPos += 64;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036DB RID: 14043 RVA: 0x00174885 File Offset: 0x00172A85
		internal static void WriteInt64(ref byte[] buf, ref int bitPos, ref int bitLength, long val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 64);
			NetBitWriter.WriteUInt64((ulong)val, 64, buf, bitPos);
			bitPos += 64;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036DC RID: 14044 RVA: 0x001748B0 File Offset: 0x00172AB0
		internal static void WriteSingle(ref byte[] buf, ref int bitPos, ref int bitLength, float val)
		{
			SingleUIntUnion su;
			su.UIntValue = 0U;
			su.SingleValue = val;
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 32);
			NetBitWriter.WriteUInt32(su.UIntValue, 32, buf, bitPos);
			bitPos += 32;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036DD RID: 14045 RVA: 0x001748FC File Offset: 0x00172AFC
		internal static void WriteDouble(ref byte[] buf, ref int bitPos, ref int bitLength, double val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 64);
			byte[] bytes = BitConverter.GetBytes(val);
			MsgWriter.WriteBytes(ref buf, ref bitPos, ref bitLength, bytes, 0, 8);
		}

		// Token: 0x060036DE RID: 14046 RVA: 0x00174926 File Offset: 0x00172B26
		internal static void WriteColorR8G8B8(ref byte[] buf, ref int bitPos, ref int bitLength, Color val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 24);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.R);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.G);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.B);
		}

		// Token: 0x060036DF RID: 14047 RVA: 0x00174960 File Offset: 0x00172B60
		internal static void WriteColorR8G8B8A8(ref byte[] buf, ref int bitPos, ref int bitLength, Color val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 32);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.R);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.G);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.B);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.A);
		}

		// Token: 0x060036E0 RID: 14048 RVA: 0x001749B4 File Offset: 0x00172BB4
		internal static void WriteString(ref byte[] buf, ref int bitPos, ref int bitLength, string val)
		{
			if (string.IsNullOrEmpty(val))
			{
				MsgWriter.WriteVariableUInt32(ref buf, ref bitPos, ref bitLength, 0U);
				return;
			}
			byte[] bytes = Encoding.UTF8.GetBytes(val);
			MsgWriter.WriteVariableUInt32(ref buf, ref bitPos, ref bitLength, (uint)bytes.Length);
			MsgWriter.WriteBytes(ref buf, ref bitPos, ref bitLength, bytes, 0, bytes.Length);
		}

		// Token: 0x060036E1 RID: 14049 RVA: 0x001749F8 File Offset: 0x00172BF8
		internal static void WriteVariableUInt32(ref byte[] buf, ref int bitPos, ref int bitLength, uint value)
		{
			uint remainingValue;
			for (remainingValue = value; remainingValue >= 128U; remainingValue >>= 7)
			{
				MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, (byte)(remainingValue | 128U));
			}
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, (byte)remainingValue);
		}

		// Token: 0x060036E2 RID: 14050 RVA: 0x00174A30 File Offset: 0x00172C30
		internal static void WriteRangedInteger(ref byte[] buf, ref int bitPos, ref int bitLength, int val, int min, int max)
		{
			uint range = (uint)(max - min);
			int numberOfBits = NetUtility.BitsToHoldUInt(range);
			MsgWriter.EnsureBufferSize(ref buf, bitPos + numberOfBits);
			uint rvalue = (uint)(val - min);
			NetBitWriter.WriteUInt32(rvalue, numberOfBits, buf, bitPos);
			bitPos += numberOfBits;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036E3 RID: 14051 RVA: 0x00174A74 File Offset: 0x00172C74
		internal static void WriteRangedSingle(ref byte[] buf, ref int bitPos, ref int bitLength, float val, float min, float max, int numberOfBits)
		{
			float range = max - min;
			float unit = (val - min) / range;
			int maxVal = (1 << numberOfBits) - 1;
			MsgWriter.EnsureBufferSize(ref buf, bitPos + numberOfBits);
			NetBitWriter.WriteUInt32((uint)((float)maxVal * unit), numberOfBits, buf, bitPos);
			bitPos += numberOfBits;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036E4 RID: 14052 RVA: 0x00174AC3 File Offset: 0x00172CC3
		internal static void WriteBytes(ref byte[] buf, ref int bitPos, ref int bitLength, byte[] val, int pos, int length)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + length * 8);
			NetBitWriter.WriteBytes(val, pos, length, buf, bitPos);
			bitPos += length * 8;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x060036E5 RID: 14053 RVA: 0x00174AF4 File Offset: 0x00172CF4
		internal static void EnsureBufferSize(ref byte[] buf, int numberOfBits)
		{
			int byteLen = (numberOfBits + 7) / 8;
			if (buf == null)
			{
				buf = new byte[byteLen + 4];
				return;
			}
			if (buf.Length < byteLen)
			{
				Array.Resize<byte>(ref buf, byteLen + 4);
			}
		}
	}
}
