using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Barotrauma.Networking;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200033E RID: 830
	[NullableContext(1)]
	[Nullable(0)]
	internal static class NetSerializableProperties
	{
		// Token: 0x0600416B RID: 16747 RVA: 0x002454C0 File Offset: 0x002436C0
		private static NetSerializableProperties.IReadWriteBehavior CreateBehavior<[Nullable(2)] TDelegateBase>(Type behaviorGenericParam, Type funcGenericParam, NetSerializableProperties.ReadWriteBehavior<TDelegateBase>.ReadDelegate readFunc, NetSerializableProperties.ReadWriteBehavior<TDelegateBase>.WriteDelegate writeFunc)
		{
			Type behaviorType = typeof(NetSerializableProperties.ReadWriteBehavior<>).MakeGenericType(new Type[]
			{
				behaviorGenericParam
			});
			Type readDelegateType = typeof(NetSerializableProperties.ReadWriteBehavior<>.ReadDelegate).MakeGenericType(new Type[]
			{
				behaviorGenericParam
			});
			Type writeDelegateType = typeof(NetSerializableProperties.ReadWriteBehavior<>.WriteDelegate).MakeGenericType(new Type[]
			{
				behaviorGenericParam
			});
			ConstructorInfo constructor = behaviorType.GetConstructor(new Type[]
			{
				readDelegateType,
				writeDelegateType
			});
			return constructor.Invoke(new object[]
			{
				readFunc.Method.GetGenericMethodDefinition().MakeGenericMethod(new Type[]
				{
					funcGenericParam
				}).CreateDelegate(readDelegateType),
				writeFunc.Method.GetGenericMethodDefinition().MakeGenericMethod(new Type[]
				{
					funcGenericParam
				}).CreateDelegate(writeDelegateType)
			}) as NetSerializableProperties.IReadWriteBehavior;
		}

		// Token: 0x0600416C RID: 16748 RVA: 0x00245588 File Offset: 0x00243788
		private static NetSerializableProperties.IReadWriteBehavior CreateArrayBehavior(Type arrayType)
		{
			Type elementType = arrayType.GetElementType();
			NetSerializableProperties.ReadWriteBehavior<object[]>.ReadDelegate readFunc;
			if ((readFunc = NetSerializableProperties.<>O.<0>__ReadArray) == null)
			{
				readFunc = (NetSerializableProperties.<>O.<0>__ReadArray = new NetSerializableProperties.ReadWriteBehavior<object[]>.ReadDelegate(NetSerializableProperties.ReadArray<object>));
			}
			NetSerializableProperties.ReadWriteBehavior<object[]>.WriteDelegate writeFunc;
			if ((writeFunc = NetSerializableProperties.<>O.<1>__WriteArray) == null)
			{
				writeFunc = (NetSerializableProperties.<>O.<1>__WriteArray = new NetSerializableProperties.ReadWriteBehavior<object[]>.WriteDelegate(NetSerializableProperties.WriteArray<object>));
			}
			return NetSerializableProperties.CreateBehavior<object[]>(arrayType, elementType, readFunc, writeFunc);
		}

		// Token: 0x0600416D RID: 16749 RVA: 0x002455D7 File Offset: 0x002437D7
		private static NetSerializableProperties.IReadWriteBehavior CreateINetSerializableStructBehavior(Type structType)
		{
			NetSerializableProperties.ReadWriteBehavior<INetSerializableStruct>.ReadDelegate readFunc;
			if ((readFunc = NetSerializableProperties.<>O.<2>__ReadINetSerializableStruct) == null)
			{
				readFunc = (NetSerializableProperties.<>O.<2>__ReadINetSerializableStruct = new NetSerializableProperties.ReadWriteBehavior<INetSerializableStruct>.ReadDelegate(NetSerializableProperties.ReadINetSerializableStruct<INetSerializableStruct>));
			}
			NetSerializableProperties.ReadWriteBehavior<INetSerializableStruct>.WriteDelegate writeFunc;
			if ((writeFunc = NetSerializableProperties.<>O.<3>__WriteINetSerializableStruct) == null)
			{
				writeFunc = (NetSerializableProperties.<>O.<3>__WriteINetSerializableStruct = new NetSerializableProperties.ReadWriteBehavior<INetSerializableStruct>.WriteDelegate(NetSerializableProperties.WriteINetSerializableStruct<INetSerializableStruct>));
			}
			return NetSerializableProperties.CreateBehavior<INetSerializableStruct>(structType, structType, readFunc, writeFunc);
		}

		// Token: 0x0600416E RID: 16750 RVA: 0x00245616 File Offset: 0x00243816
		private static NetSerializableProperties.IReadWriteBehavior CreateEnumBehavior(Type enumType)
		{
			NetSerializableProperties.ReadWriteBehavior<Enum>.ReadDelegate readFunc;
			if ((readFunc = NetSerializableProperties.<>O.<4>__ReadEnum) == null)
			{
				readFunc = (NetSerializableProperties.<>O.<4>__ReadEnum = new NetSerializableProperties.ReadWriteBehavior<Enum>.ReadDelegate(NetSerializableProperties.ReadEnum<Enum>));
			}
			NetSerializableProperties.ReadWriteBehavior<Enum>.WriteDelegate writeFunc;
			if ((writeFunc = NetSerializableProperties.<>O.<5>__WriteEnum) == null)
			{
				writeFunc = (NetSerializableProperties.<>O.<5>__WriteEnum = new NetSerializableProperties.ReadWriteBehavior<Enum>.WriteDelegate(NetSerializableProperties.WriteEnum<Enum>));
			}
			return NetSerializableProperties.CreateBehavior<Enum>(enumType, enumType, readFunc, writeFunc);
		}

		// Token: 0x0600416F RID: 16751 RVA: 0x00245658 File Offset: 0x00243858
		private static NetSerializableProperties.IReadWriteBehavior CreateNullableStructBehavior(Type nullableType)
		{
			Type underlyingType = Nullable.GetUnderlyingType(nullableType);
			NetSerializableProperties.ReadWriteBehavior<int?>.ReadDelegate readFunc;
			if ((readFunc = NetSerializableProperties.<>O.<6>__ReadNullable) == null)
			{
				readFunc = (NetSerializableProperties.<>O.<6>__ReadNullable = new NetSerializableProperties.ReadWriteBehavior<int?>.ReadDelegate(NetSerializableProperties.ReadNullable<int>));
			}
			NetSerializableProperties.ReadWriteBehavior<int?>.WriteDelegate writeFunc;
			if ((writeFunc = NetSerializableProperties.<>O.<7>__WriteNullable) == null)
			{
				writeFunc = (NetSerializableProperties.<>O.<7>__WriteNullable = new NetSerializableProperties.ReadWriteBehavior<int?>.WriteDelegate(NetSerializableProperties.WriteNullable<int>));
			}
			return NetSerializableProperties.CreateBehavior<int?>(nullableType, underlyingType, readFunc, writeFunc);
		}

		// Token: 0x06004170 RID: 16752 RVA: 0x002456A8 File Offset: 0x002438A8
		private static NetSerializableProperties.IReadWriteBehavior CreateOptionBehavior(Type optionType)
		{
			Type funcGenericParam = optionType.GetGenericArguments()[0];
			NetSerializableProperties.ReadWriteBehavior<Option<object>>.ReadDelegate readFunc;
			if ((readFunc = NetSerializableProperties.<>O.<8>__ReadOption) == null)
			{
				readFunc = (NetSerializableProperties.<>O.<8>__ReadOption = new NetSerializableProperties.ReadWriteBehavior<Option<object>>.ReadDelegate(NetSerializableProperties.ReadOption<object>));
			}
			NetSerializableProperties.ReadWriteBehavior<Option<object>>.WriteDelegate writeFunc;
			if ((writeFunc = NetSerializableProperties.<>O.<9>__WriteOption) == null)
			{
				writeFunc = (NetSerializableProperties.<>O.<9>__WriteOption = new NetSerializableProperties.ReadWriteBehavior<Option<object>>.WriteDelegate(NetSerializableProperties.WriteOption<object>));
			}
			return NetSerializableProperties.CreateBehavior<Option<object>>(optionType, funcGenericParam, readFunc, writeFunc);
		}

		// Token: 0x06004171 RID: 16753 RVA: 0x002456FC File Offset: 0x002438FC
		private static NetSerializableProperties.IReadWriteBehavior CreateImmutableArrayBehavior(Type arrayType)
		{
			Type funcGenericParam = arrayType.GetGenericArguments()[0];
			NetSerializableProperties.ReadWriteBehavior<ImmutableArray<object>>.ReadDelegate readFunc;
			if ((readFunc = NetSerializableProperties.<>O.<10>__ReadImmutableArray) == null)
			{
				readFunc = (NetSerializableProperties.<>O.<10>__ReadImmutableArray = new NetSerializableProperties.ReadWriteBehavior<ImmutableArray<object>>.ReadDelegate(NetSerializableProperties.ReadImmutableArray<object>));
			}
			NetSerializableProperties.ReadWriteBehavior<ImmutableArray<object>>.WriteDelegate writeFunc;
			if ((writeFunc = NetSerializableProperties.<>O.<11>__WriteImmutableArray) == null)
			{
				writeFunc = (NetSerializableProperties.<>O.<11>__WriteImmutableArray = new NetSerializableProperties.ReadWriteBehavior<ImmutableArray<object>>.WriteDelegate(NetSerializableProperties.WriteImmutableArray<object>));
			}
			return NetSerializableProperties.CreateBehavior<ImmutableArray<object>>(arrayType, funcGenericParam, readFunc, writeFunc);
		}

		// Token: 0x06004172 RID: 16754 RVA: 0x0024574D File Offset: 0x0024394D
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private static ImmutableArray<T> ReadImmutableArray<T>(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return NetSerializableProperties.ReadArray<T>(inc, attribute, bitField).ToImmutableArray<T>();
		}

		// Token: 0x06004173 RID: 16755 RVA: 0x00245761 File Offset: 0x00243961
		private static void WriteImmutableArray<T>([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<T> array, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<ImmutableArray<T>>(array);
			NetSerializableProperties.WriteIReadOnlyCollection<T>(array, attribute, msg, bitField);
		}

		// Token: 0x06004174 RID: 16756 RVA: 0x00245778 File Offset: 0x00243978
		private static T[] ReadArray<T>(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			int length = bitField.ReadInteger(0, attribute.ArrayMaxSize);
			T[] array = new T[length];
			NetSerializableProperties.ReadWriteBehavior<T> behavior;
			if (!NetSerializableProperties.TryFindBehavior<T>(out behavior))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find suitable behavior for type ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(T));
				defaultInterpolatedStringHandler.AppendLiteral(" in ");
				defaultInterpolatedStringHandler.AppendFormatted("ReadArray");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			for (int i = 0; i < length; i++)
			{
				array[i] = behavior.ReadActionDirect(inc, attribute, bitField);
			}
			return array;
		}

		// Token: 0x06004175 RID: 16757 RVA: 0x00245817 File Offset: 0x00243A17
		private static void WriteArray<T>(T[] array, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<T[]>(array);
			NetSerializableProperties.WriteIReadOnlyCollection<T>(array, attribute, msg, bitField);
		}

		// Token: 0x06004176 RID: 16758 RVA: 0x00245828 File Offset: 0x00243A28
		private static void WriteIReadOnlyCollection<T>(IReadOnlyCollection<T> array, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			bitField.WriteInteger(array.Count, 0, attribute.ArrayMaxSize);
			NetSerializableProperties.ReadWriteBehavior<T> behavior;
			if (!NetSerializableProperties.TryFindBehavior<T>(out behavior))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Could not find suitable behavior for type ");
				defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(T));
				defaultInterpolatedStringHandler.AppendLiteral(" in ");
				defaultInterpolatedStringHandler.AppendFormatted("WriteArray");
				throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			foreach (T o in array)
			{
				behavior.WriteActionDirect(o, attribute, msg, bitField);
			}
		}

		// Token: 0x06004177 RID: 16759 RVA: 0x002458E4 File Offset: 0x00243AE4
		private static T ReadINetSerializableStruct<[Nullable(0)] T>(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField) where T : INetSerializableStruct
		{
			return INetSerializableStruct.ReadInternal<T>(inc, bitField);
		}

		// Token: 0x06004178 RID: 16760 RVA: 0x002458ED File Offset: 0x00243AED
		private static void WriteINetSerializableStruct<[Nullable(0)] T>(T serializableStruct, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField) where T : INetSerializableStruct
		{
			ToolBox.ThrowIfNull<T>(serializableStruct);
			serializableStruct.WriteInternal(msg, bitField);
		}

		// Token: 0x06004179 RID: 16761 RVA: 0x00245904 File Offset: 0x00243B04
		private static T ReadEnum<[Nullable(0)] T>(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField) where T : Enum
		{
			Type type = typeof(T);
			Range<int> range = NetSerializableProperties.GetEnumRange(type);
			int enumIndex = bitField.ReadInteger(range.Start, range.End);
			if (typeof(T).GetCustomAttribute<FlagsAttribute>() != null)
			{
				return (T)((object)enumIndex);
			}
			foreach (T e in (T[])Enum.GetValues(type))
			{
				if ((int)((object)e) == enumIndex)
				{
					return e;
				}
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(43, 3);
			defaultInterpolatedStringHandler.AppendLiteral("An enum ");
			defaultInterpolatedStringHandler.AppendFormatted<Type>(type);
			defaultInterpolatedStringHandler.AppendLiteral(" with value ");
			defaultInterpolatedStringHandler.AppendFormatted<int>(enumIndex);
			defaultInterpolatedStringHandler.AppendLiteral(" could not be found in ");
			defaultInterpolatedStringHandler.AppendFormatted("ReadEnum");
			throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x0600417A RID: 16762 RVA: 0x002459E8 File Offset: 0x00243BE8
		private static void WriteEnum<[Nullable(0)] T>(T value, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField) where T : Enum
		{
			ToolBox.ThrowIfNull<T>(value);
			Range<int> range = NetSerializableProperties.GetEnumRange(typeof(T));
			bitField.WriteInteger((int)Convert.ChangeType(value, value.GetTypeCode()), range.Start, range.End);
		}

		// Token: 0x0600417B RID: 16763 RVA: 0x00245A3C File Offset: 0x00243C3C
		[return: Nullable(0)]
		private static T? ReadNullable<[Nullable(0)] T>(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField) where T : struct
		{
			T value;
			if (!NetSerializableProperties.ReadOption<T>(inc, attribute, bitField).TryUnwrap(out value))
			{
				return null;
			}
			return new T?(value);
		}

		// Token: 0x0600417C RID: 16764 RVA: 0x00245A6D File Offset: 0x00243C6D
		private static void WriteNullable<[Nullable(0)] T>([Nullable(0)] T? value, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField) where T : struct
		{
			NetSerializableProperties.WriteOption<T>((value != null) ? Option<T>.Some(value.Value) : Option<T>.None(), attribute, msg, bitField);
		}

		// Token: 0x0600417D RID: 16765 RVA: 0x00245A94 File Offset: 0x00243C94
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private static Option<T> ReadOption<T>(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			if (!bitField.ReadBoolean())
			{
				return Option<T>.None();
			}
			NetSerializableProperties.ReadWriteBehavior<T> behavior;
			if (NetSerializableProperties.TryFindBehavior<T>(out behavior))
			{
				return Option<T>.Some(behavior.ReadActionDirect(inc, attribute, bitField));
			}
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(46, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Could not find suitable behavior for type ");
			defaultInterpolatedStringHandler.AppendFormatted<Type>(typeof(T));
			defaultInterpolatedStringHandler.AppendLiteral(" in ");
			defaultInterpolatedStringHandler.AppendFormatted("ReadOption");
			throw new InvalidOperationException(defaultInterpolatedStringHandler.ToStringAndClear());
		}

		// Token: 0x0600417E RID: 16766 RVA: 0x00245B1C File Offset: 0x00243D1C
		private static void WriteOption<T>([Nullable(new byte[]
		{
			0,
			1
		})] Option<T> option, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<Option<T>>(option);
			T value;
			if (option.TryUnwrap(out value))
			{
				bitField.WriteBoolean(true);
				NetSerializableProperties.ReadWriteBehavior<T> behavior;
				if (NetSerializableProperties.TryFindBehavior<T>(out behavior))
				{
					behavior.WriteActionDirect(value, attribute, msg, bitField);
					return;
				}
			}
			else
			{
				bitField.WriteBoolean(false);
			}
		}

		// Token: 0x0600417F RID: 16767 RVA: 0x00245B62 File Offset: 0x00243D62
		private static bool ReadBoolean(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return bitField.ReadBoolean();
		}

		// Token: 0x06004180 RID: 16768 RVA: 0x00245B6A File Offset: 0x00243D6A
		private static void WriteBoolean(bool b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			bitField.WriteBoolean(b);
		}

		// Token: 0x06004181 RID: 16769 RVA: 0x00245B73 File Offset: 0x00243D73
		private static byte ReadByte(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadByte();
		}

		// Token: 0x06004182 RID: 16770 RVA: 0x00245B7B File Offset: 0x00243D7B
		private static void WriteByte(byte b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteByte(b);
		}

		// Token: 0x06004183 RID: 16771 RVA: 0x00245B84 File Offset: 0x00243D84
		private static ushort ReadUInt16(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadUInt16();
		}

		// Token: 0x06004184 RID: 16772 RVA: 0x00245B8C File Offset: 0x00243D8C
		private static void WriteUInt16(ushort b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteUInt16(b);
		}

		// Token: 0x06004185 RID: 16773 RVA: 0x00245B95 File Offset: 0x00243D95
		private static short ReadInt16(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadInt16();
		}

		// Token: 0x06004186 RID: 16774 RVA: 0x00245B9D File Offset: 0x00243D9D
		private static void WriteInt16(short b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteInt16(b);
		}

		// Token: 0x06004187 RID: 16775 RVA: 0x00245BA6 File Offset: 0x00243DA6
		private static uint ReadUInt32(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadUInt32();
		}

		// Token: 0x06004188 RID: 16776 RVA: 0x00245BAE File Offset: 0x00243DAE
		private static void WriteUInt32(uint b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteUInt32(b);
		}

		// Token: 0x06004189 RID: 16777 RVA: 0x00245BB7 File Offset: 0x00243DB7
		private static int ReadInt32(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			if (NetSerializableProperties.IsRanged(attribute.MinValueInt, attribute.MaxValueInt))
			{
				return bitField.ReadInteger(attribute.MinValueInt, attribute.MaxValueInt);
			}
			return inc.ReadInt32();
		}

		// Token: 0x0600418A RID: 16778 RVA: 0x00245BE5 File Offset: 0x00243DE5
		private static void WriteInt32(int i, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<int>(i);
			if (NetSerializableProperties.IsRanged(attribute.MinValueInt, attribute.MaxValueInt))
			{
				bitField.WriteInteger(i, attribute.MinValueInt, attribute.MaxValueInt);
				return;
			}
			msg.WriteInt32(i);
		}

		// Token: 0x0600418B RID: 16779 RVA: 0x00245C1B File Offset: 0x00243E1B
		private static ulong ReadUInt64(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadUInt64();
		}

		// Token: 0x0600418C RID: 16780 RVA: 0x00245C23 File Offset: 0x00243E23
		private static void WriteUInt64(ulong b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteUInt64(b);
		}

		// Token: 0x0600418D RID: 16781 RVA: 0x00245C2C File Offset: 0x00243E2C
		private static long ReadInt64(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadInt64();
		}

		// Token: 0x0600418E RID: 16782 RVA: 0x00245C34 File Offset: 0x00243E34
		private static void WriteInt64(long b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteInt64(b);
		}

		// Token: 0x0600418F RID: 16783 RVA: 0x00245C3D File Offset: 0x00243E3D
		private static float ReadSingle(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			if (NetSerializableProperties.IsRanged(attribute.MinValueFloat, attribute.MaxValueFloat))
			{
				return bitField.ReadFloat(attribute.MinValueFloat, attribute.MaxValueFloat, attribute.NumberOfBits);
			}
			return inc.ReadSingle();
		}

		// Token: 0x06004190 RID: 16784 RVA: 0x00245C71 File Offset: 0x00243E71
		private static void WriteSingle(float f, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<float>(f);
			if (NetSerializableProperties.IsRanged(attribute.MinValueFloat, attribute.MaxValueFloat))
			{
				bitField.WriteFloat(f, attribute.MinValueFloat, attribute.MaxValueFloat, attribute.NumberOfBits);
				return;
			}
			msg.WriteSingle(f);
		}

		// Token: 0x06004191 RID: 16785 RVA: 0x00245CAD File Offset: 0x00243EAD
		private static double ReadDouble(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadDouble();
		}

		// Token: 0x06004192 RID: 16786 RVA: 0x00245CB5 File Offset: 0x00243EB5
		private static void WriteDouble(double b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteDouble(b);
		}

		// Token: 0x06004193 RID: 16787 RVA: 0x00245CBE File Offset: 0x00243EBE
		private static NetLimitedString ReadNetLString(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return new NetLimitedString(inc.ReadString());
		}

		// Token: 0x06004194 RID: 16788 RVA: 0x00245CCB File Offset: 0x00243ECB
		private static void WriteNetLString(NetLimitedString b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteString(b.Value);
		}

		// Token: 0x06004195 RID: 16789 RVA: 0x00245CD9 File Offset: 0x00243ED9
		private static string ReadString(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadString();
		}

		// Token: 0x06004196 RID: 16790 RVA: 0x00245CE1 File Offset: 0x00243EE1
		private static void WriteString(string b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteString(b);
		}

		// Token: 0x06004197 RID: 16791 RVA: 0x00245CEA File Offset: 0x00243EEA
		private static Identifier ReadIdentifier(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadIdentifier();
		}

		// Token: 0x06004198 RID: 16792 RVA: 0x00245CF2 File Offset: 0x00243EF2
		private static void WriteIdentifier(Identifier b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteIdentifier(b);
		}

		// Token: 0x06004199 RID: 16793 RVA: 0x00245CFC File Offset: 0x00243EFC
		private static AccountId ReadAccountId(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			string str = inc.ReadString();
			AccountId accountId;
			if (!AccountId.Parse(str).TryUnwrap(out accountId))
			{
				throw new InvalidCastException("Could not parse \"" + str + "\" as an AccountId");
			}
			return accountId;
		}

		// Token: 0x0600419A RID: 16794 RVA: 0x00245D39 File Offset: 0x00243F39
		private static void WriteAccountId(AccountId accountId, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteString(accountId.StringRepresentation);
		}

		// Token: 0x0600419B RID: 16795 RVA: 0x00245D47 File Offset: 0x00243F47
		private static Color ReadColor(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			if (!attribute.IncludeColorAlpha)
			{
				return inc.ReadColorR8G8B8();
			}
			return inc.ReadColorR8G8B8A8();
		}

		// Token: 0x0600419C RID: 16796 RVA: 0x00245D5E File Offset: 0x00243F5E
		private static void WriteColor(Color color, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<Color>(color);
			if (attribute.IncludeColorAlpha)
			{
				msg.WriteColorR8G8B8A8(color);
				return;
			}
			msg.WriteColorR8G8B8(color);
		}

		// Token: 0x0600419D RID: 16797 RVA: 0x00245D80 File Offset: 0x00243F80
		private static Vector2 ReadVector2(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			float x = NetSerializableProperties.ReadSingle(inc, attribute, bitField);
			float y = NetSerializableProperties.ReadSingle(inc, attribute, bitField);
			return new Vector2(x, y);
		}

		// Token: 0x0600419E RID: 16798 RVA: 0x00245DA8 File Offset: 0x00243FA8
		private static void WriteVector2(Vector2 vector2, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<Vector2>(vector2);
			Vector2 vector3 = vector2;
			float num;
			float num2;
			vector3.Deconstruct(out num, out num2);
			float x = num;
			float y = num2;
			NetSerializableProperties.WriteSingle(x, attribute, msg, bitField);
			NetSerializableProperties.WriteSingle(y, attribute, msg, bitField);
		}

		// Token: 0x0600419F RID: 16799 RVA: 0x00245DE0 File Offset: 0x00243FE0
		private static SerializableDateTime ReadSerializableDateTime(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			long ticks = inc.ReadInt64();
			short timezone = inc.ReadInt16();
			if (!NetSerializableProperties.ValidTickRange.Contains(ticks))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(70, 2);
				defaultInterpolatedStringHandler.AppendLiteral("Incoming SerializableDateTime ticks out of range (ticks: ");
				defaultInterpolatedStringHandler.AppendFormatted<long>(ticks);
				defaultInterpolatedStringHandler.AppendLiteral(", timezone: ");
				defaultInterpolatedStringHandler.AppendFormatted<short>(timezone);
				defaultInterpolatedStringHandler.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
			}
			if (!NetSerializableProperties.ValidTimeZoneMinuteRange.Contains(timezone))
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(73, 2);
				defaultInterpolatedStringHandler2.AppendLiteral("Incoming SerializableDateTime timezone out of range (ticks: ");
				defaultInterpolatedStringHandler2.AppendFormatted<long>(ticks);
				defaultInterpolatedStringHandler2.AppendLiteral(", timezone: ");
				defaultInterpolatedStringHandler2.AppendFormatted<short>(timezone);
				defaultInterpolatedStringHandler2.AppendLiteral(")");
				throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
			}
			return new SerializableDateTime(new DateTime(ticks), new SerializableTimeZone(TimeSpan.FromMinutes((double)timezone)));
		}

		// Token: 0x060041A0 RID: 16800 RVA: 0x00245EC4 File Offset: 0x002440C4
		private static void WriteSerializableDateTime(SerializableDateTime dateTime, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteInt64(dateTime.Ticks);
			msg.WriteInt16((short)(dateTime.TimeZone.Value.Ticks / 600000000L));
		}

		// Token: 0x060041A1 RID: 16801 RVA: 0x00245EF2 File Offset: 0x002440F2
		private static bool IsRanged(float minValue, float maxValue)
		{
			return minValue > float.MinValue || maxValue < float.MaxValue;
		}

		// Token: 0x060041A2 RID: 16802 RVA: 0x00245F06 File Offset: 0x00244106
		private static bool IsRanged(int minValue, int maxValue)
		{
			return minValue > int.MinValue || maxValue < int.MaxValue;
		}

		// Token: 0x060041A3 RID: 16803 RVA: 0x00245F1C File Offset: 0x0024411C
		[NullableContext(0)]
		private static Range<int> GetEnumRange([Nullable(1)] Type type)
		{
			ImmutableArray<int> values = Enum.GetValues(type).Cast<int>().ToImmutableArray<int>();
			return new Range<int>(values.Min(), values.Max());
		}

		// Token: 0x060041A4 RID: 16804 RVA: 0x00245F58 File Offset: 0x00244158
		private static bool TryFindBehavior<T>([Nullable(new byte[]
		{
			0,
			1
		})] out NetSerializableProperties.ReadWriteBehavior<T> behavior)
		{
			NetSerializableProperties.IReadWriteBehavior bhvr;
			bool found = NetSerializableProperties.TryFindBehavior(typeof(T), out bhvr);
			behavior = (found ? ((NetSerializableProperties.ReadWriteBehavior<T>)bhvr) : default(NetSerializableProperties.ReadWriteBehavior<T>));
			return found;
		}

		// Token: 0x060041A5 RID: 16805 RVA: 0x00245F94 File Offset: 0x00244194
		private static bool TryFindBehavior(Type type, out NetSerializableProperties.IReadWriteBehavior behavior)
		{
			NetSerializableProperties.IReadWriteBehavior outBehavior;
			if (NetSerializableProperties.TypeBehaviors.TryGetValue(type, out outBehavior))
			{
				behavior = outBehavior;
				return true;
			}
			foreach (KeyValuePair<Predicate<Type>, Func<Type, NetSerializableProperties.IReadWriteBehavior>> keyValuePair in NetSerializableProperties.BehaviorFactories)
			{
				Predicate<Type> predicate2;
				Func<Type, NetSerializableProperties.IReadWriteBehavior> func;
				keyValuePair.Deconstruct(out predicate2, out func);
				Predicate<Type> predicate = predicate2;
				Func<Type, NetSerializableProperties.IReadWriteBehavior> factory = func;
				if (predicate(type))
				{
					behavior = factory(type);
					NetSerializableProperties.TypeBehaviors.Add(type, behavior);
					return true;
				}
			}
			behavior = null;
			return false;
		}

		// Token: 0x060041A6 RID: 16806 RVA: 0x00246030 File Offset: 0x00244230
		[NullableContext(0)]
		public static ImmutableArray<NetSerializableProperties.CachedReflectedVariable> GetPropertiesAndFields([Nullable(1)] Type type)
		{
			NetSerializableProperties.<>c__DisplayClass67_0 CS$<>8__locals1 = new NetSerializableProperties.<>c__DisplayClass67_0();
			CS$<>8__locals1.type = type;
			ImmutableArray<NetSerializableProperties.CachedReflectedVariable> cached;
			if (NetSerializableProperties.CachedVariables.TryGetValue(CS$<>8__locals1.type, out cached))
			{
				return cached;
			}
			List<NetSerializableProperties.CachedReflectedVariable> variables = new List<NetSerializableProperties.CachedReflectedVariable>();
			IEnumerable<PropertyInfo> source = CS$<>8__locals1.type.GetProperties().Where(new Func<PropertyInfo, bool>(CS$<>8__locals1.<GetPropertiesAndFields>g__HasAttribute|2));
			Func<PropertyInfo, bool> predicate;
			if ((predicate = NetSerializableProperties.<>O.<12>__NotStatic) == null)
			{
				predicate = (NetSerializableProperties.<>O.<12>__NotStatic = new Func<PropertyInfo, bool>(NetSerializableProperties.<GetPropertiesAndFields>g__NotStatic|67_3));
			}
			IEnumerable<PropertyInfo> propertyInfos = source.Where(predicate);
			IEnumerable<FieldInfo> source2 = CS$<>8__locals1.type.GetFields().Where(new Func<FieldInfo, bool>(CS$<>8__locals1.<GetPropertiesAndFields>g__HasAttribute|2));
			Func<FieldInfo, bool> predicate2;
			if ((predicate2 = NetSerializableProperties.<>O.<13>__NotStatic) == null)
			{
				predicate2 = (NetSerializableProperties.<>O.<13>__NotStatic = new Func<FieldInfo, bool>(NetSerializableProperties.<GetPropertiesAndFields>g__NotStatic|67_3));
			}
			IEnumerable<FieldInfo> fieldInfos = source2.Where(predicate2);
			foreach (PropertyInfo info in propertyInfos)
			{
				if (info.SetMethod != null)
				{
					NetSerializableProperties.IReadWriteBehavior behavior;
					if (!NetSerializableProperties.TryFindBehavior(info.PropertyType, out behavior))
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 1);
						defaultInterpolatedStringHandler.AppendLiteral("Unable to serialize type \"");
						defaultInterpolatedStringHandler.AppendFormatted<Type>(CS$<>8__locals1.type);
						defaultInterpolatedStringHandler.AppendLiteral("\".");
						throw new Exception(defaultInterpolatedStringHandler.ToStringAndClear());
					}
					variables.Add(new NetSerializableProperties.CachedReflectedVariable(info, behavior, CS$<>8__locals1.type));
				}
			}
			foreach (FieldInfo info2 in fieldInfos)
			{
				NetSerializableProperties.IReadWriteBehavior behavior2;
				if (!NetSerializableProperties.TryFindBehavior(info2.FieldType, out behavior2))
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(28, 1);
					defaultInterpolatedStringHandler2.AppendLiteral("Unable to serialize type \"");
					defaultInterpolatedStringHandler2.AppendFormatted<Type>(CS$<>8__locals1.type);
					defaultInterpolatedStringHandler2.AppendLiteral("\".");
					throw new Exception(defaultInterpolatedStringHandler2.ToStringAndClear());
				}
				variables.Add(new NetSerializableProperties.CachedReflectedVariable(info2, behavior2, CS$<>8__locals1.type));
			}
			ImmutableArray<NetSerializableProperties.CachedReflectedVariable> immutableArray;
			if (!variables.All((NetSerializableProperties.CachedReflectedVariable v) => v.HasOwnAttribute))
			{
				immutableArray = variables.ToImmutableArray<NetSerializableProperties.CachedReflectedVariable>();
			}
			else
			{
				immutableArray = (from v in variables
				orderby v.Attribute.OrderKey
				select v).ToImmutableArray<NetSerializableProperties.CachedReflectedVariable>();
			}
			ImmutableArray<NetSerializableProperties.CachedReflectedVariable> array = immutableArray;
			NetSerializableProperties.CachedVariables.Add(CS$<>8__locals1.type, array);
			return array;
		}

		// Token: 0x060041A7 RID: 16807 RVA: 0x00246290 File Offset: 0x00244490
		private static bool IsOfGenericType(Type type, Type comparedTo)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == comparedTo;
		}

		// Token: 0x060041A9 RID: 16809 RVA: 0x00246750 File Offset: 0x00244950
		[CompilerGenerated]
		internal static bool <GetPropertiesAndFields>g__NotStatic|67_3(MemberInfo info)
		{
			PropertyInfo property = info as PropertyInfo;
			bool result;
			if (property == null)
			{
				FieldInfo field = info as FieldInfo;
				result = (field != null && !field.IsStatic);
			}
			else
			{
				MethodInfo getMethod = property.GetGetMethod();
				result = (getMethod != null && !getMethod.IsStatic);
			}
			return result;
		}

		// Token: 0x04002227 RID: 8743
		[Nullable(new byte[]
		{
			1,
			1,
			0
		})]
		private static readonly Dictionary<Type, ImmutableArray<NetSerializableProperties.CachedReflectedVariable>> CachedVariables = new Dictionary<Type, ImmutableArray<NetSerializableProperties.CachedReflectedVariable>>();

		// Token: 0x04002228 RID: 8744
		private static readonly Dictionary<Type, NetSerializableProperties.IReadWriteBehavior> TypeBehaviors = new Dictionary<Type, NetSerializableProperties.IReadWriteBehavior>
		{
			{
				typeof(bool),
				new NetSerializableProperties.ReadWriteBehavior<bool>(new NetSerializableProperties.ReadWriteBehavior<bool>.ReadDelegate(NetSerializableProperties.ReadBoolean), new NetSerializableProperties.ReadWriteBehavior<bool>.WriteDelegate(NetSerializableProperties.WriteBoolean))
			},
			{
				typeof(byte),
				new NetSerializableProperties.ReadWriteBehavior<byte>(new NetSerializableProperties.ReadWriteBehavior<byte>.ReadDelegate(NetSerializableProperties.ReadByte), new NetSerializableProperties.ReadWriteBehavior<byte>.WriteDelegate(NetSerializableProperties.WriteByte))
			},
			{
				typeof(ushort),
				new NetSerializableProperties.ReadWriteBehavior<ushort>(new NetSerializableProperties.ReadWriteBehavior<ushort>.ReadDelegate(NetSerializableProperties.ReadUInt16), new NetSerializableProperties.ReadWriteBehavior<ushort>.WriteDelegate(NetSerializableProperties.WriteUInt16))
			},
			{
				typeof(short),
				new NetSerializableProperties.ReadWriteBehavior<short>(new NetSerializableProperties.ReadWriteBehavior<short>.ReadDelegate(NetSerializableProperties.ReadInt16), new NetSerializableProperties.ReadWriteBehavior<short>.WriteDelegate(NetSerializableProperties.WriteInt16))
			},
			{
				typeof(uint),
				new NetSerializableProperties.ReadWriteBehavior<uint>(new NetSerializableProperties.ReadWriteBehavior<uint>.ReadDelegate(NetSerializableProperties.ReadUInt32), new NetSerializableProperties.ReadWriteBehavior<uint>.WriteDelegate(NetSerializableProperties.WriteUInt32))
			},
			{
				typeof(int),
				new NetSerializableProperties.ReadWriteBehavior<int>(new NetSerializableProperties.ReadWriteBehavior<int>.ReadDelegate(NetSerializableProperties.ReadInt32), new NetSerializableProperties.ReadWriteBehavior<int>.WriteDelegate(NetSerializableProperties.WriteInt32))
			},
			{
				typeof(ulong),
				new NetSerializableProperties.ReadWriteBehavior<ulong>(new NetSerializableProperties.ReadWriteBehavior<ulong>.ReadDelegate(NetSerializableProperties.ReadUInt64), new NetSerializableProperties.ReadWriteBehavior<ulong>.WriteDelegate(NetSerializableProperties.WriteUInt64))
			},
			{
				typeof(long),
				new NetSerializableProperties.ReadWriteBehavior<long>(new NetSerializableProperties.ReadWriteBehavior<long>.ReadDelegate(NetSerializableProperties.ReadInt64), new NetSerializableProperties.ReadWriteBehavior<long>.WriteDelegate(NetSerializableProperties.WriteInt64))
			},
			{
				typeof(float),
				new NetSerializableProperties.ReadWriteBehavior<float>(new NetSerializableProperties.ReadWriteBehavior<float>.ReadDelegate(NetSerializableProperties.ReadSingle), new NetSerializableProperties.ReadWriteBehavior<float>.WriteDelegate(NetSerializableProperties.WriteSingle))
			},
			{
				typeof(double),
				new NetSerializableProperties.ReadWriteBehavior<double>(new NetSerializableProperties.ReadWriteBehavior<double>.ReadDelegate(NetSerializableProperties.ReadDouble), new NetSerializableProperties.ReadWriteBehavior<double>.WriteDelegate(NetSerializableProperties.WriteDouble))
			},
			{
				typeof(string),
				new NetSerializableProperties.ReadWriteBehavior<string>(new NetSerializableProperties.ReadWriteBehavior<string>.ReadDelegate(NetSerializableProperties.ReadString), new NetSerializableProperties.ReadWriteBehavior<string>.WriteDelegate(NetSerializableProperties.WriteString))
			},
			{
				typeof(Identifier),
				new NetSerializableProperties.ReadWriteBehavior<Identifier>(new NetSerializableProperties.ReadWriteBehavior<Identifier>.ReadDelegate(NetSerializableProperties.ReadIdentifier), new NetSerializableProperties.ReadWriteBehavior<Identifier>.WriteDelegate(NetSerializableProperties.WriteIdentifier))
			},
			{
				typeof(AccountId),
				new NetSerializableProperties.ReadWriteBehavior<AccountId>(new NetSerializableProperties.ReadWriteBehavior<AccountId>.ReadDelegate(NetSerializableProperties.ReadAccountId), new NetSerializableProperties.ReadWriteBehavior<AccountId>.WriteDelegate(NetSerializableProperties.WriteAccountId))
			},
			{
				typeof(Color),
				new NetSerializableProperties.ReadWriteBehavior<Color>(new NetSerializableProperties.ReadWriteBehavior<Color>.ReadDelegate(NetSerializableProperties.ReadColor), new NetSerializableProperties.ReadWriteBehavior<Color>.WriteDelegate(NetSerializableProperties.WriteColor))
			},
			{
				typeof(Vector2),
				new NetSerializableProperties.ReadWriteBehavior<Vector2>(new NetSerializableProperties.ReadWriteBehavior<Vector2>.ReadDelegate(NetSerializableProperties.ReadVector2), new NetSerializableProperties.ReadWriteBehavior<Vector2>.WriteDelegate(NetSerializableProperties.WriteVector2))
			},
			{
				typeof(SerializableDateTime),
				new NetSerializableProperties.ReadWriteBehavior<SerializableDateTime>(new NetSerializableProperties.ReadWriteBehavior<SerializableDateTime>.ReadDelegate(NetSerializableProperties.ReadSerializableDateTime), new NetSerializableProperties.ReadWriteBehavior<SerializableDateTime>.WriteDelegate(NetSerializableProperties.WriteSerializableDateTime))
			},
			{
				typeof(NetLimitedString),
				new NetSerializableProperties.ReadWriteBehavior<NetLimitedString>(new NetSerializableProperties.ReadWriteBehavior<NetLimitedString>.ReadDelegate(NetSerializableProperties.ReadNetLString), new NetSerializableProperties.ReadWriteBehavior<NetLimitedString>.WriteDelegate(NetSerializableProperties.WriteNetLString))
			}
		};

		// Token: 0x04002229 RID: 8745
		private static readonly ImmutableDictionary<Predicate<Type>, Func<Type, NetSerializableProperties.IReadWriteBehavior>> BehaviorFactories = new Dictionary<Predicate<Type>, Func<Type, NetSerializableProperties.IReadWriteBehavior>>
		{
			{
				(Type type) => type.IsArray,
				new Func<Type, NetSerializableProperties.IReadWriteBehavior>(NetSerializableProperties.CreateArrayBehavior)
			},
			{
				(Type type) => typeof(INetSerializableStruct).IsAssignableFrom(type),
				new Func<Type, NetSerializableProperties.IReadWriteBehavior>(NetSerializableProperties.CreateINetSerializableStructBehavior)
			},
			{
				(Type type) => type.IsEnum,
				new Func<Type, NetSerializableProperties.IReadWriteBehavior>(NetSerializableProperties.CreateEnumBehavior)
			},
			{
				(Type type) => Nullable.GetUnderlyingType(type) != null,
				new Func<Type, NetSerializableProperties.IReadWriteBehavior>(NetSerializableProperties.CreateNullableStructBehavior)
			},
			{
				(Type type) => NetSerializableProperties.IsOfGenericType(type, typeof(ImmutableArray<>)),
				new Func<Type, NetSerializableProperties.IReadWriteBehavior>(NetSerializableProperties.CreateImmutableArrayBehavior)
			},
			{
				(Type type) => NetSerializableProperties.IsOfGenericType(type, typeof(Option<>)),
				new Func<Type, NetSerializableProperties.IReadWriteBehavior>(NetSerializableProperties.CreateOptionBehavior)
			}
		}.ToImmutableDictionary<Predicate<Type>, Func<Type, NetSerializableProperties.IReadWriteBehavior>>();

		// Token: 0x0400222A RID: 8746
		[Nullable(0)]
		private static readonly Range<long> ValidTickRange = new Range<long>(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);

		// Token: 0x0400222B RID: 8747
		[Nullable(0)]
		private static readonly Range<short> ValidTimeZoneMinuteRange = new Range<short>((short)TimeSpan.FromHours(-12.0).TotalMinutes, (short)TimeSpan.FromHours(14.0).TotalMinutes);

		// Token: 0x02001052 RID: 4178
		public interface IReadWriteBehavior
		{
			// Token: 0x17001C5F RID: 7263
			// (get) Token: 0x06008C28 RID: 35880
			NetSerializableProperties.IReadWriteBehavior.ReadDelegate ReadAction { get; }

			// Token: 0x17001C60 RID: 7264
			// (get) Token: 0x06008C29 RID: 35881
			NetSerializableProperties.IReadWriteBehavior.WriteDelegate WriteAction { get; }

			// Token: 0x02001576 RID: 5494
			// (Invoke) Token: 0x06009DFB RID: 40443
			[NullableContext(0)]
			[return: Nullable(2)]
			public delegate object ReadDelegate(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField);

			// Token: 0x02001577 RID: 5495
			// (Invoke) Token: 0x06009DFF RID: 40447
			[NullableContext(0)]
			public delegate void WriteDelegate([Nullable(2)] object obj, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField);
		}

		// Token: 0x02001053 RID: 4179
		[Nullable(0)]
		public readonly struct ReadWriteBehavior<[Nullable(2)] T> : NetSerializableProperties.IReadWriteBehavior
		{
			// Token: 0x17001C61 RID: 7265
			// (get) Token: 0x06008C2A RID: 35882 RVA: 0x003AEE8E File Offset: 0x003AD08E
			public NetSerializableProperties.IReadWriteBehavior.ReadDelegate ReadAction { get; }

			// Token: 0x17001C62 RID: 7266
			// (get) Token: 0x06008C2B RID: 35883 RVA: 0x003AEE96 File Offset: 0x003AD096
			public NetSerializableProperties.IReadWriteBehavior.WriteDelegate WriteAction { get; }

			// Token: 0x17001C63 RID: 7267
			// (get) Token: 0x06008C2C RID: 35884 RVA: 0x003AEE9E File Offset: 0x003AD09E
			[Nullable(new byte[]
			{
				1,
				0
			})]
			public NetSerializableProperties.ReadWriteBehavior<T>.ReadDelegate ReadActionDirect { [return: Nullable(new byte[]
			{
				1,
				0
			})] get; }

			// Token: 0x17001C64 RID: 7268
			// (get) Token: 0x06008C2D RID: 35885 RVA: 0x003AEEA6 File Offset: 0x003AD0A6
			[Nullable(new byte[]
			{
				1,
				0
			})]
			public NetSerializableProperties.ReadWriteBehavior<T>.WriteDelegate WriteActionDirect { [return: Nullable(new byte[]
			{
				1,
				0
			})] get; }

			// Token: 0x06008C2E RID: 35886 RVA: 0x003AEEB0 File Offset: 0x003AD0B0
			public ReadWriteBehavior([Nullable(new byte[]
			{
				1,
				0
			})] NetSerializableProperties.ReadWriteBehavior<T>.ReadDelegate readAction, [Nullable(new byte[]
			{
				1,
				0
			})] NetSerializableProperties.ReadWriteBehavior<T>.WriteDelegate writeAction)
			{
				this.ReadAction = ((IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField) => readAction(inc, attribute, bitField));
				this.WriteAction = delegate([Nullable(2)] object o, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
				{
					writeAction((T)((object)o), attribute, msg, bitField);
				};
				this.ReadActionDirect = readAction;
				this.WriteActionDirect = writeAction;
			}

			// Token: 0x02001578 RID: 5496
			// (Invoke) Token: 0x06009E03 RID: 40451
			[NullableContext(0)]
			public delegate T ReadDelegate(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField);

			// Token: 0x02001579 RID: 5497
			// (Invoke) Token: 0x06009E07 RID: 40455
			[NullableContext(0)]
			public delegate void WriteDelegate(T obj, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField);
		}

		// Token: 0x02001054 RID: 4180
		[Nullable(0)]
		public readonly struct CachedReflectedVariable
		{
			// Token: 0x06008C2F RID: 35887 RVA: 0x003AEF10 File Offset: 0x003AD110
			public CachedReflectedVariable(MemberInfo info, NetSerializableProperties.IReadWriteBehavior behavior, Type baseClassType)
			{
				this.Behavior = behavior;
				this.Name = info.Name;
				PropertyInfo pi = info as PropertyInfo;
				if (pi == null)
				{
					FieldInfo fi = info as FieldInfo;
					if (fi == null)
					{
						DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(25, 3);
						defaultInterpolatedStringHandler.AppendLiteral("Expected ");
						defaultInterpolatedStringHandler.AppendFormatted("FieldInfo");
						defaultInterpolatedStringHandler.AppendLiteral(" or ");
						defaultInterpolatedStringHandler.AppendFormatted("PropertyInfo");
						defaultInterpolatedStringHandler.AppendLiteral(" but found ");
						defaultInterpolatedStringHandler.AppendFormatted<Type>(info.GetType());
						defaultInterpolatedStringHandler.AppendLiteral(".");
						throw new ArgumentException(defaultInterpolatedStringHandler.ToStringAndClear(), "info");
					}
					this.Type = fi.FieldType;
					this.GetValue = new NetSerializableProperties.CachedReflectedVariable.GetValueDelegate(fi.GetValue);
					this.SetValue = new NetSerializableProperties.CachedReflectedVariable.SetValueDelegate(fi.SetValue);
				}
				else
				{
					this.Type = pi.PropertyType;
					this.GetValue = new NetSerializableProperties.CachedReflectedVariable.GetValueDelegate(pi.GetValue);
					this.SetValue = new NetSerializableProperties.CachedReflectedVariable.SetValueDelegate(pi.SetValue);
				}
				NetworkSerialize ownAttriute = info.GetCustomAttribute<NetworkSerialize>();
				if (ownAttriute != null)
				{
					this.HasOwnAttribute = true;
					this.Attribute = ownAttriute;
					return;
				}
				NetworkSerialize globalAttribute = baseClassType.GetCustomAttribute<NetworkSerialize>();
				if (globalAttribute != null)
				{
					this.HasOwnAttribute = false;
					this.Attribute = globalAttribute;
					return;
				}
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(58, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("Unable to serialize \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Type>(this.Type);
				defaultInterpolatedStringHandler2.AppendLiteral("\" in \"");
				defaultInterpolatedStringHandler2.AppendFormatted<Type>(baseClassType);
				defaultInterpolatedStringHandler2.AppendLiteral("\" because it has no ");
				defaultInterpolatedStringHandler2.AppendFormatted("NetworkSerialize");
				defaultInterpolatedStringHandler2.AppendLiteral(" attribute.");
				throw new InvalidOperationException(defaultInterpolatedStringHandler2.ToStringAndClear());
			}

			// Token: 0x04005817 RID: 22551
			public readonly string Name;

			// Token: 0x04005818 RID: 22552
			public readonly Type Type;

			// Token: 0x04005819 RID: 22553
			public readonly NetSerializableProperties.IReadWriteBehavior Behavior;

			// Token: 0x0400581A RID: 22554
			public readonly NetworkSerialize Attribute;

			// Token: 0x0400581B RID: 22555
			public readonly NetSerializableProperties.CachedReflectedVariable.SetValueDelegate SetValue;

			// Token: 0x0400581C RID: 22556
			public readonly NetSerializableProperties.CachedReflectedVariable.GetValueDelegate GetValue;

			// Token: 0x0400581D RID: 22557
			public readonly bool HasOwnAttribute;

			// Token: 0x0200157B RID: 5499
			// (Invoke) Token: 0x06009E0E RID: 40462
			[NullableContext(0)]
			public delegate object GetValueDelegate(object obj);

			// Token: 0x0200157C RID: 5500
			// (Invoke) Token: 0x06009E12 RID: 40466
			[NullableContext(0)]
			public delegate void SetValueDelegate(object obj, object value);
		}

		// Token: 0x02001055 RID: 4181
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400581E RID: 22558
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<object[]>.ReadDelegate <0>__ReadArray;

			// Token: 0x0400581F RID: 22559
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<object[]>.WriteDelegate <1>__WriteArray;

			// Token: 0x04005820 RID: 22560
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<INetSerializableStruct>.ReadDelegate <2>__ReadINetSerializableStruct;

			// Token: 0x04005821 RID: 22561
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<INetSerializableStruct>.WriteDelegate <3>__WriteINetSerializableStruct;

			// Token: 0x04005822 RID: 22562
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<Enum>.ReadDelegate <4>__ReadEnum;

			// Token: 0x04005823 RID: 22563
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<Enum>.WriteDelegate <5>__WriteEnum;

			// Token: 0x04005824 RID: 22564
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<int?>.ReadDelegate <6>__ReadNullable;

			// Token: 0x04005825 RID: 22565
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<int?>.WriteDelegate <7>__WriteNullable;

			// Token: 0x04005826 RID: 22566
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<Option<object>>.ReadDelegate <8>__ReadOption;

			// Token: 0x04005827 RID: 22567
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<Option<object>>.WriteDelegate <9>__WriteOption;

			// Token: 0x04005828 RID: 22568
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<ImmutableArray<object>>.ReadDelegate <10>__ReadImmutableArray;

			// Token: 0x04005829 RID: 22569
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<ImmutableArray<object>>.WriteDelegate <11>__WriteImmutableArray;

			// Token: 0x0400582A RID: 22570
			[Nullable(0)]
			public static Func<PropertyInfo, bool> <12>__NotStatic;

			// Token: 0x0400582B RID: 22571
			[Nullable(0)]
			public static Func<FieldInfo, bool> <13>__NotStatic;
		}
	}
}
