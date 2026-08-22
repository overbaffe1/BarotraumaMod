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
	// Token: 0x0200026B RID: 619
	[NullableContext(1)]
	[Nullable(0)]
	internal static class NetSerializableProperties
	{
		// Token: 0x06002C4C RID: 11340 RVA: 0x0012517C File Offset: 0x0012337C
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

		// Token: 0x06002C4D RID: 11341 RVA: 0x00125244 File Offset: 0x00123444
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

		// Token: 0x06002C4E RID: 11342 RVA: 0x00125293 File Offset: 0x00123493
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

		// Token: 0x06002C4F RID: 11343 RVA: 0x001252D2 File Offset: 0x001234D2
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

		// Token: 0x06002C50 RID: 11344 RVA: 0x00125314 File Offset: 0x00123514
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

		// Token: 0x06002C51 RID: 11345 RVA: 0x00125364 File Offset: 0x00123564
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

		// Token: 0x06002C52 RID: 11346 RVA: 0x001253B8 File Offset: 0x001235B8
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

		// Token: 0x06002C53 RID: 11347 RVA: 0x00125409 File Offset: 0x00123609
		[return: Nullable(new byte[]
		{
			0,
			1
		})]
		private static ImmutableArray<T> ReadImmutableArray<T>(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return NetSerializableProperties.ReadArray<T>(inc, attribute, bitField).ToImmutableArray<T>();
		}

		// Token: 0x06002C54 RID: 11348 RVA: 0x0012541D File Offset: 0x0012361D
		private static void WriteImmutableArray<T>([Nullable(new byte[]
		{
			0,
			1
		})] ImmutableArray<T> array, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<ImmutableArray<T>>(array);
			NetSerializableProperties.WriteIReadOnlyCollection<T>(array, attribute, msg, bitField);
		}

		// Token: 0x06002C55 RID: 11349 RVA: 0x00125434 File Offset: 0x00123634
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

		// Token: 0x06002C56 RID: 11350 RVA: 0x001254D3 File Offset: 0x001236D3
		private static void WriteArray<T>(T[] array, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			ToolBox.ThrowIfNull<T[]>(array);
			NetSerializableProperties.WriteIReadOnlyCollection<T>(array, attribute, msg, bitField);
		}

		// Token: 0x06002C57 RID: 11351 RVA: 0x001254E4 File Offset: 0x001236E4
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

		// Token: 0x06002C58 RID: 11352 RVA: 0x001255A0 File Offset: 0x001237A0
		private static T ReadINetSerializableStruct<[Nullable(0)] T>(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField) where T : INetSerializableStruct
		{
			return INetSerializableStruct.ReadInternal<T>(inc, bitField);
		}

		// Token: 0x06002C59 RID: 11353 RVA: 0x001255A9 File Offset: 0x001237A9
		private static void WriteINetSerializableStruct<[Nullable(0)] T>(T serializableStruct, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField) where T : INetSerializableStruct
		{
			ToolBox.ThrowIfNull<T>(serializableStruct);
			serializableStruct.WriteInternal(msg, bitField);
		}

		// Token: 0x06002C5A RID: 11354 RVA: 0x001255C0 File Offset: 0x001237C0
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

		// Token: 0x06002C5B RID: 11355 RVA: 0x001256A4 File Offset: 0x001238A4
		private static void WriteEnum<[Nullable(0)] T>(T value, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField) where T : Enum
		{
			ToolBox.ThrowIfNull<T>(value);
			Range<int> range = NetSerializableProperties.GetEnumRange(typeof(T));
			bitField.WriteInteger((int)Convert.ChangeType(value, value.GetTypeCode()), range.Start, range.End);
		}

		// Token: 0x06002C5C RID: 11356 RVA: 0x001256F8 File Offset: 0x001238F8
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

		// Token: 0x06002C5D RID: 11357 RVA: 0x00125729 File Offset: 0x00123929
		private static void WriteNullable<[Nullable(0)] T>([Nullable(0)] T? value, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField) where T : struct
		{
			NetSerializableProperties.WriteOption<T>((value != null) ? Option<T>.Some(value.Value) : Option<T>.None(), attribute, msg, bitField);
		}

		// Token: 0x06002C5E RID: 11358 RVA: 0x00125750 File Offset: 0x00123950
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

		// Token: 0x06002C5F RID: 11359 RVA: 0x001257D8 File Offset: 0x001239D8
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

		// Token: 0x06002C60 RID: 11360 RVA: 0x0012581E File Offset: 0x00123A1E
		private static bool ReadBoolean(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return bitField.ReadBoolean();
		}

		// Token: 0x06002C61 RID: 11361 RVA: 0x00125826 File Offset: 0x00123A26
		private static void WriteBoolean(bool b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			bitField.WriteBoolean(b);
		}

		// Token: 0x06002C62 RID: 11362 RVA: 0x0012582F File Offset: 0x00123A2F
		private static byte ReadByte(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadByte();
		}

		// Token: 0x06002C63 RID: 11363 RVA: 0x00125837 File Offset: 0x00123A37
		private static void WriteByte(byte b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteByte(b);
		}

		// Token: 0x06002C64 RID: 11364 RVA: 0x00125840 File Offset: 0x00123A40
		private static ushort ReadUInt16(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadUInt16();
		}

		// Token: 0x06002C65 RID: 11365 RVA: 0x00125848 File Offset: 0x00123A48
		private static void WriteUInt16(ushort b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteUInt16(b);
		}

		// Token: 0x06002C66 RID: 11366 RVA: 0x00125851 File Offset: 0x00123A51
		private static short ReadInt16(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadInt16();
		}

		// Token: 0x06002C67 RID: 11367 RVA: 0x00125859 File Offset: 0x00123A59
		private static void WriteInt16(short b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteInt16(b);
		}

		// Token: 0x06002C68 RID: 11368 RVA: 0x00125862 File Offset: 0x00123A62
		private static uint ReadUInt32(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadUInt32();
		}

		// Token: 0x06002C69 RID: 11369 RVA: 0x0012586A File Offset: 0x00123A6A
		private static void WriteUInt32(uint b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteUInt32(b);
		}

		// Token: 0x06002C6A RID: 11370 RVA: 0x00125873 File Offset: 0x00123A73
		private static int ReadInt32(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			if (NetSerializableProperties.IsRanged(attribute.MinValueInt, attribute.MaxValueInt))
			{
				return bitField.ReadInteger(attribute.MinValueInt, attribute.MaxValueInt);
			}
			return inc.ReadInt32();
		}

		// Token: 0x06002C6B RID: 11371 RVA: 0x001258A1 File Offset: 0x00123AA1
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

		// Token: 0x06002C6C RID: 11372 RVA: 0x001258D7 File Offset: 0x00123AD7
		private static ulong ReadUInt64(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadUInt64();
		}

		// Token: 0x06002C6D RID: 11373 RVA: 0x001258DF File Offset: 0x00123ADF
		private static void WriteUInt64(ulong b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteUInt64(b);
		}

		// Token: 0x06002C6E RID: 11374 RVA: 0x001258E8 File Offset: 0x00123AE8
		private static long ReadInt64(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadInt64();
		}

		// Token: 0x06002C6F RID: 11375 RVA: 0x001258F0 File Offset: 0x00123AF0
		private static void WriteInt64(long b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteInt64(b);
		}

		// Token: 0x06002C70 RID: 11376 RVA: 0x001258F9 File Offset: 0x00123AF9
		private static float ReadSingle(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			if (NetSerializableProperties.IsRanged(attribute.MinValueFloat, attribute.MaxValueFloat))
			{
				return bitField.ReadFloat(attribute.MinValueFloat, attribute.MaxValueFloat, attribute.NumberOfBits);
			}
			return inc.ReadSingle();
		}

		// Token: 0x06002C71 RID: 11377 RVA: 0x0012592D File Offset: 0x00123B2D
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

		// Token: 0x06002C72 RID: 11378 RVA: 0x00125969 File Offset: 0x00123B69
		private static double ReadDouble(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadDouble();
		}

		// Token: 0x06002C73 RID: 11379 RVA: 0x00125971 File Offset: 0x00123B71
		private static void WriteDouble(double b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteDouble(b);
		}

		// Token: 0x06002C74 RID: 11380 RVA: 0x0012597A File Offset: 0x00123B7A
		private static NetLimitedString ReadNetLString(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return new NetLimitedString(inc.ReadString());
		}

		// Token: 0x06002C75 RID: 11381 RVA: 0x00125987 File Offset: 0x00123B87
		private static void WriteNetLString(NetLimitedString b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteString(b.Value);
		}

		// Token: 0x06002C76 RID: 11382 RVA: 0x00125995 File Offset: 0x00123B95
		private static string ReadString(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadString();
		}

		// Token: 0x06002C77 RID: 11383 RVA: 0x0012599D File Offset: 0x00123B9D
		private static void WriteString(string b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteString(b);
		}

		// Token: 0x06002C78 RID: 11384 RVA: 0x001259A6 File Offset: 0x00123BA6
		private static Identifier ReadIdentifier(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			return inc.ReadIdentifier();
		}

		// Token: 0x06002C79 RID: 11385 RVA: 0x001259AE File Offset: 0x00123BAE
		private static void WriteIdentifier(Identifier b, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteIdentifier(b);
		}

		// Token: 0x06002C7A RID: 11386 RVA: 0x001259B8 File Offset: 0x00123BB8
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

		// Token: 0x06002C7B RID: 11387 RVA: 0x001259F5 File Offset: 0x00123BF5
		private static void WriteAccountId(AccountId accountId, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteString(accountId.StringRepresentation);
		}

		// Token: 0x06002C7C RID: 11388 RVA: 0x00125A03 File Offset: 0x00123C03
		private static Color ReadColor(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			if (!attribute.IncludeColorAlpha)
			{
				return inc.ReadColorR8G8B8();
			}
			return inc.ReadColorR8G8B8A8();
		}

		// Token: 0x06002C7D RID: 11389 RVA: 0x00125A1A File Offset: 0x00123C1A
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

		// Token: 0x06002C7E RID: 11390 RVA: 0x00125A3C File Offset: 0x00123C3C
		private static Vector2 ReadVector2(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField)
		{
			float x = NetSerializableProperties.ReadSingle(inc, attribute, bitField);
			float y = NetSerializableProperties.ReadSingle(inc, attribute, bitField);
			return new Vector2(x, y);
		}

		// Token: 0x06002C7F RID: 11391 RVA: 0x00125A64 File Offset: 0x00123C64
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

		// Token: 0x06002C80 RID: 11392 RVA: 0x00125A9C File Offset: 0x00123C9C
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

		// Token: 0x06002C81 RID: 11393 RVA: 0x00125B80 File Offset: 0x00123D80
		private static void WriteSerializableDateTime(SerializableDateTime dateTime, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField)
		{
			msg.WriteInt64(dateTime.Ticks);
			msg.WriteInt16((short)(dateTime.TimeZone.Value.Ticks / 600000000L));
		}

		// Token: 0x06002C82 RID: 11394 RVA: 0x00125BAE File Offset: 0x00123DAE
		private static bool IsRanged(float minValue, float maxValue)
		{
			return minValue > float.MinValue || maxValue < float.MaxValue;
		}

		// Token: 0x06002C83 RID: 11395 RVA: 0x00125BC2 File Offset: 0x00123DC2
		private static bool IsRanged(int minValue, int maxValue)
		{
			return minValue > int.MinValue || maxValue < int.MaxValue;
		}

		// Token: 0x06002C84 RID: 11396 RVA: 0x00125BD8 File Offset: 0x00123DD8
		[NullableContext(0)]
		private static Range<int> GetEnumRange([Nullable(1)] Type type)
		{
			ImmutableArray<int> values = Enum.GetValues(type).Cast<int>().ToImmutableArray<int>();
			return new Range<int>(values.Min(), values.Max());
		}

		// Token: 0x06002C85 RID: 11397 RVA: 0x00125C14 File Offset: 0x00123E14
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

		// Token: 0x06002C86 RID: 11398 RVA: 0x00125C50 File Offset: 0x00123E50
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

		// Token: 0x06002C87 RID: 11399 RVA: 0x00125CEC File Offset: 0x00123EEC
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

		// Token: 0x06002C88 RID: 11400 RVA: 0x00125F4C File Offset: 0x0012414C
		private static bool IsOfGenericType(Type type, Type comparedTo)
		{
			return type.IsGenericType && type.GetGenericTypeDefinition() == comparedTo;
		}

		// Token: 0x06002C8A RID: 11402 RVA: 0x0012640C File Offset: 0x0012460C
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

		// Token: 0x040015E1 RID: 5601
		[Nullable(new byte[]
		{
			1,
			1,
			0
		})]
		private static readonly Dictionary<Type, ImmutableArray<NetSerializableProperties.CachedReflectedVariable>> CachedVariables = new Dictionary<Type, ImmutableArray<NetSerializableProperties.CachedReflectedVariable>>();

		// Token: 0x040015E2 RID: 5602
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

		// Token: 0x040015E3 RID: 5603
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

		// Token: 0x040015E4 RID: 5604
		[Nullable(0)]
		private static readonly Range<long> ValidTickRange = new Range<long>(DateTime.MinValue.Ticks, DateTime.MaxValue.Ticks);

		// Token: 0x040015E5 RID: 5605
		[Nullable(0)]
		private static readonly Range<short> ValidTimeZoneMinuteRange = new Range<short>((short)TimeSpan.FromHours(-12.0).TotalMinutes, (short)TimeSpan.FromHours(14.0).TotalMinutes);

		// Token: 0x02000ADA RID: 2778
		public interface IReadWriteBehavior
		{
			// Token: 0x170015A2 RID: 5538
			// (get) Token: 0x06005E9A RID: 24218
			NetSerializableProperties.IReadWriteBehavior.ReadDelegate ReadAction { get; }

			// Token: 0x170015A3 RID: 5539
			// (get) Token: 0x06005E9B RID: 24219
			NetSerializableProperties.IReadWriteBehavior.WriteDelegate WriteAction { get; }

			// Token: 0x02000EB6 RID: 3766
			// (Invoke) Token: 0x06006ADF RID: 27359
			[NullableContext(0)]
			[return: Nullable(2)]
			public delegate object ReadDelegate(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField);

			// Token: 0x02000EB7 RID: 3767
			// (Invoke) Token: 0x06006AE3 RID: 27363
			[NullableContext(0)]
			public delegate void WriteDelegate([Nullable(2)] object obj, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField);
		}

		// Token: 0x02000ADB RID: 2779
		[Nullable(0)]
		public readonly struct ReadWriteBehavior<[Nullable(2)] T> : NetSerializableProperties.IReadWriteBehavior
		{
			// Token: 0x170015A4 RID: 5540
			// (get) Token: 0x06005E9C RID: 24220 RVA: 0x00205675 File Offset: 0x00203875
			public NetSerializableProperties.IReadWriteBehavior.ReadDelegate ReadAction { get; }

			// Token: 0x170015A5 RID: 5541
			// (get) Token: 0x06005E9D RID: 24221 RVA: 0x0020567D File Offset: 0x0020387D
			public NetSerializableProperties.IReadWriteBehavior.WriteDelegate WriteAction { get; }

			// Token: 0x170015A6 RID: 5542
			// (get) Token: 0x06005E9E RID: 24222 RVA: 0x00205685 File Offset: 0x00203885
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

			// Token: 0x170015A7 RID: 5543
			// (get) Token: 0x06005E9F RID: 24223 RVA: 0x0020568D File Offset: 0x0020388D
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

			// Token: 0x06005EA0 RID: 24224 RVA: 0x00205698 File Offset: 0x00203898
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

			// Token: 0x02000EB8 RID: 3768
			// (Invoke) Token: 0x06006AE7 RID: 27367
			[NullableContext(0)]
			public delegate T ReadDelegate(IReadMessage inc, NetworkSerialize attribute, ReadOnlyBitField bitField);

			// Token: 0x02000EB9 RID: 3769
			// (Invoke) Token: 0x06006AEB RID: 27371
			[NullableContext(0)]
			public delegate void WriteDelegate(T obj, NetworkSerialize attribute, IWriteMessage msg, WriteOnlyBitField bitField);
		}

		// Token: 0x02000ADC RID: 2780
		[Nullable(0)]
		public readonly struct CachedReflectedVariable
		{
			// Token: 0x06005EA1 RID: 24225 RVA: 0x002056F8 File Offset: 0x002038F8
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

			// Token: 0x04003770 RID: 14192
			public readonly string Name;

			// Token: 0x04003771 RID: 14193
			public readonly Type Type;

			// Token: 0x04003772 RID: 14194
			public readonly NetSerializableProperties.IReadWriteBehavior Behavior;

			// Token: 0x04003773 RID: 14195
			public readonly NetworkSerialize Attribute;

			// Token: 0x04003774 RID: 14196
			public readonly NetSerializableProperties.CachedReflectedVariable.SetValueDelegate SetValue;

			// Token: 0x04003775 RID: 14197
			public readonly NetSerializableProperties.CachedReflectedVariable.GetValueDelegate GetValue;

			// Token: 0x04003776 RID: 14198
			public readonly bool HasOwnAttribute;

			// Token: 0x02000EBB RID: 3771
			// (Invoke) Token: 0x06006AF2 RID: 27378
			[NullableContext(0)]
			public delegate object GetValueDelegate(object obj);

			// Token: 0x02000EBC RID: 3772
			// (Invoke) Token: 0x06006AF6 RID: 27382
			[NullableContext(0)]
			public delegate void SetValueDelegate(object obj, object value);
		}

		// Token: 0x02000ADD RID: 2781
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04003777 RID: 14199
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<object[]>.ReadDelegate <0>__ReadArray;

			// Token: 0x04003778 RID: 14200
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<object[]>.WriteDelegate <1>__WriteArray;

			// Token: 0x04003779 RID: 14201
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<INetSerializableStruct>.ReadDelegate <2>__ReadINetSerializableStruct;

			// Token: 0x0400377A RID: 14202
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<INetSerializableStruct>.WriteDelegate <3>__WriteINetSerializableStruct;

			// Token: 0x0400377B RID: 14203
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<Enum>.ReadDelegate <4>__ReadEnum;

			// Token: 0x0400377C RID: 14204
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<Enum>.WriteDelegate <5>__WriteEnum;

			// Token: 0x0400377D RID: 14205
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<int?>.ReadDelegate <6>__ReadNullable;

			// Token: 0x0400377E RID: 14206
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<int?>.WriteDelegate <7>__WriteNullable;

			// Token: 0x0400377F RID: 14207
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<Option<object>>.ReadDelegate <8>__ReadOption;

			// Token: 0x04003780 RID: 14208
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<Option<object>>.WriteDelegate <9>__WriteOption;

			// Token: 0x04003781 RID: 14209
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<ImmutableArray<object>>.ReadDelegate <10>__ReadImmutableArray;

			// Token: 0x04003782 RID: 14210
			[Nullable(0)]
			public static NetSerializableProperties.ReadWriteBehavior<ImmutableArray<object>>.WriteDelegate <11>__WriteImmutableArray;

			// Token: 0x04003783 RID: 14211
			[Nullable(0)]
			public static Func<PropertyInfo, bool> <12>__NotStatic;

			// Token: 0x04003784 RID: 14212
			[Nullable(0)]
			public static Func<FieldInfo, bool> <13>__NotStatic;
		}
	}
}
