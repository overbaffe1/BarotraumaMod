using System;
using System.Text;
using Lidgren.Network;
using Microsoft.Xna.Framework;

namespace Barotrauma.Networking
{
	// Token: 0x020004A3 RID: 1187
	internal static class MsgWriter
	{
		// Token: 0x06004EA7 RID: 20135 RVA: 0x002AD237 File Offset: 0x002AB437
		internal static void UpdateBitLength(ref int bitLength, int bitPos)
		{
			bitLength = Math.Max(bitLength, bitPos);
		}

		// Token: 0x06004EA8 RID: 20136 RVA: 0x002AD244 File Offset: 0x002AB444
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

		// Token: 0x06004EA9 RID: 20137 RVA: 0x002AD2A4 File Offset: 0x002AB4A4
		internal static void WritePadBits(ref byte[] buf, ref int bitPos, ref int bitLength)
		{
			int bitOffset = bitPos % 8;
			bitPos += (8 - bitOffset) % 8;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
			MsgWriter.EnsureBufferSize(ref buf, bitPos);
		}

		// Token: 0x06004EAA RID: 20138 RVA: 0x002AD2D0 File Offset: 0x002AB4D0
		internal static void WriteByte(ref byte[] buf, ref int bitPos, ref int bitLength, byte val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 8);
			NetBitWriter.WriteByte(val, 8, buf, bitPos);
			bitPos += 8;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x06004EAB RID: 20139 RVA: 0x002AD2F5 File Offset: 0x002AB4F5
		internal static void WriteUInt16(ref byte[] buf, ref int bitPos, ref int bitLength, ushort val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 16);
			NetBitWriter.WriteUInt16(val, 16, buf, bitPos);
			bitPos += 16;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x06004EAC RID: 20140 RVA: 0x002AD31D File Offset: 0x002AB51D
		internal static void WriteInt16(ref byte[] buf, ref int bitPos, ref int bitLength, short val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 16);
			NetBitWriter.WriteUInt16((ushort)val, 16, buf, bitPos);
			bitPos += 16;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x06004EAD RID: 20141 RVA: 0x002AD346 File Offset: 0x002AB546
		internal static void WriteUInt32(ref byte[] buf, ref int bitPos, ref int bitLength, uint val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 32);
			NetBitWriter.WriteUInt32(val, 32, buf, bitPos);
			bitPos += 32;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x06004EAE RID: 20142 RVA: 0x002AD36F File Offset: 0x002AB56F
		internal static void WriteInt32(ref byte[] buf, ref int bitPos, ref int bitLength, int val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 32);
			NetBitWriter.WriteUInt32((uint)val, 32, buf, bitPos);
			bitPos += 32;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x06004EAF RID: 20143 RVA: 0x002AD398 File Offset: 0x002AB598
		internal static void WriteUInt64(ref byte[] buf, ref int bitPos, ref int bitLength, ulong val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 64);
			NetBitWriter.WriteUInt64(val, 64, buf, bitPos);
			bitPos += 64;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x06004EB0 RID: 20144 RVA: 0x002AD3C1 File Offset: 0x002AB5C1
		internal static void WriteInt64(ref byte[] buf, ref int bitPos, ref int bitLength, long val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 64);
			NetBitWriter.WriteUInt64((ulong)val, 64, buf, bitPos);
			bitPos += 64;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x06004EB1 RID: 20145 RVA: 0x002AD3EC File Offset: 0x002AB5EC
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

		// Token: 0x06004EB2 RID: 20146 RVA: 0x002AD438 File Offset: 0x002AB638
		internal static void WriteDouble(ref byte[] buf, ref int bitPos, ref int bitLength, double val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 64);
			byte[] bytes = BitConverter.GetBytes(val);
			MsgWriter.WriteBytes(ref buf, ref bitPos, ref bitLength, bytes, 0, 8);
		}

		// Token: 0x06004EB3 RID: 20147 RVA: 0x002AD462 File Offset: 0x002AB662
		internal static void WriteColorR8G8B8(ref byte[] buf, ref int bitPos, ref int bitLength, Color val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 24);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.R);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.G);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.B);
		}

		// Token: 0x06004EB4 RID: 20148 RVA: 0x002AD49C File Offset: 0x002AB69C
		internal static void WriteColorR8G8B8A8(ref byte[] buf, ref int bitPos, ref int bitLength, Color val)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + 32);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.R);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.G);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.B);
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, val.A);
		}

		// Token: 0x06004EB5 RID: 20149 RVA: 0x002AD4F0 File Offset: 0x002AB6F0
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

		// Token: 0x06004EB6 RID: 20150 RVA: 0x002AD534 File Offset: 0x002AB734
		internal static void WriteVariableUInt32(ref byte[] buf, ref int bitPos, ref int bitLength, uint value)
		{
			uint remainingValue;
			for (remainingValue = value; remainingValue >= 128U; remainingValue >>= 7)
			{
				MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, (byte)(remainingValue | 128U));
			}
			MsgWriter.WriteByte(ref buf, ref bitPos, ref bitLength, (byte)remainingValue);
		}

		// Token: 0x06004EB7 RID: 20151 RVA: 0x002AD56C File Offset: 0x002AB76C
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

		// Token: 0x06004EB8 RID: 20152 RVA: 0x002AD5B0 File Offset: 0x002AB7B0
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

		// Token: 0x06004EB9 RID: 20153 RVA: 0x002AD5FF File Offset: 0x002AB7FF
		internal static void WriteBytes(ref byte[] buf, ref int bitPos, ref int bitLength, byte[] val, int pos, int length)
		{
			MsgWriter.EnsureBufferSize(ref buf, bitPos + length * 8);
			NetBitWriter.WriteBytes(val, pos, length, buf, bitPos);
			bitPos += length * 8;
			MsgWriter.UpdateBitLength(ref bitLength, bitPos);
		}

		// Token: 0x06004EBA RID: 20154 RVA: 0x002AD630 File Offset: 0x002AB830
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
