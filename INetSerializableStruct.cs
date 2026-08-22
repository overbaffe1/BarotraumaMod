using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using Barotrauma.Networking;

namespace Barotrauma
{
	// Token: 0x0200033F RID: 831
	[NullableContext(1)]
	internal interface INetSerializableStruct
	{
		// Token: 0x060041AA RID: 16810 RVA: 0x0024679C File Offset: 0x0024499C
		public static T Read<[Nullable(0)] T>(IReadMessage inc) where T : INetSerializableStruct
		{
			T result;
			try
			{
				ReadOnlyBitField bitField = new ReadOnlyBitField(inc);
				result = INetSerializableStruct.ReadInternal<T>(inc, bitField);
			}
			catch (Exception e)
			{
				throw new NetStructReadException("Failed to read INetSerializableStruct", e);
			}
			return result;
		}

		// Token: 0x060041AB RID: 16811 RVA: 0x002467D8 File Offset: 0x002449D8
		public static T ReadInternal<[Nullable(0)] T>(IReadMessage inc, ReadOnlyBitField bitField) where T : INetSerializableStruct
		{
			object newObject = Activator.CreateInstance(typeof(T));
			if (newObject == null)
			{
				return default(T);
			}
			foreach (NetSerializableProperties.CachedReflectedVariable property in NetSerializableProperties.GetPropertiesAndFields(typeof(T)))
			{
				object value = property.Behavior.ReadAction(inc, property.Attribute, bitField);
				try
				{
					property.SetValue(newObject, value);
				}
				catch (Exception exception)
				{
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(28, 5);
					defaultInterpolatedStringHandler.AppendLiteral("Failed to assign");
					defaultInterpolatedStringHandler.AppendLiteral(" ");
					defaultInterpolatedStringHandler.AppendFormatted<object>(value ?? "[NULL]");
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted(((value != null) ? value.GetType().Name : null) ?? "[NULL]");
					defaultInterpolatedStringHandler.AppendLiteral(")");
					defaultInterpolatedStringHandler.AppendLiteral(" to ");
					defaultInterpolatedStringHandler.AppendFormatted(typeof(T).Name);
					defaultInterpolatedStringHandler.AppendLiteral(".");
					defaultInterpolatedStringHandler.AppendFormatted(property.Name);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted(property.Type.Name);
					defaultInterpolatedStringHandler.AppendLiteral(")");
					throw new NetStructReadException(defaultInterpolatedStringHandler.ToStringAndClear(), exception);
				}
			}
			return (T)((object)newObject);
		}

		// Token: 0x060041AC RID: 16812 RVA: 0x00246964 File Offset: 0x00244B64
		void Write(IWriteMessage msg)
		{
			WriteOnlyBitField bitField = new WriteOnlyBitField();
			IWriteMessage structWriteMsg = new WriteOnlyMessage();
			this.WriteInternal(structWriteMsg, bitField);
			bitField.WriteToMessage(msg);
			msg.WriteBytes(structWriteMsg.Buffer, 0, structWriteMsg.LengthBytes);
		}

		// Token: 0x060041AD RID: 16813 RVA: 0x002469A0 File Offset: 0x00244BA0
		void WriteInternal(IWriteMessage msg, WriteOnlyBitField bitField)
		{
			foreach (NetSerializableProperties.CachedReflectedVariable property in NetSerializableProperties.GetPropertiesAndFields(base.GetType()))
			{
				object value = property.GetValue(this);
				property.Behavior.WriteAction(value, property.Attribute, msg, bitField);
			}
		}

		// Token: 0x060041AE RID: 16814 RVA: 0x002469FC File Offset: 0x00244BFC
		public static bool TryRead<[Nullable(0)] T>(IReadMessage inc, AccountInfo sender, [Nullable(2)] [NotNullWhen(true)] out T data) where T : INetSerializableStruct
		{
			INetSerializableStruct.<>c__DisplayClass4_0<T> CS$<>8__locals1;
			CS$<>8__locals1.inc = inc;
			CS$<>8__locals1.sender = sender;
			bool result;
			try
			{
				data = INetSerializableStruct.Read<T>(CS$<>8__locals1.inc);
				result = true;
			}
			catch (Exception e)
			{
				INetSerializableStruct.<TryRead>g__LogError|4_0<T>(e, ref CS$<>8__locals1);
				data = default(T);
				result = false;
			}
			return result;
		}

		// Token: 0x060041AF RID: 16815 RVA: 0x00246A54 File Offset: 0x00244C54
		[NullableContext(0)]
		[CompilerGenerated]
		internal static void <TryRead>g__LogError|4_0<T>([Nullable(1)] Exception e, ref INetSerializableStruct.<>c__DisplayClass4_0<T> A_1) where T : INetSerializableStruct
		{
			int prevPos = A_1.inc.BitPosition;
			StringBuilder hexData = new StringBuilder();
			A_1.inc.BitPosition = 0;
			while (A_1.inc.BitPosition < A_1.inc.LengthBits && A_1.inc.BytePosition < 500)
			{
				byte b = A_1.inc.ReadByte();
				StringBuilder stringBuilder = hexData;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(1, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendFormatted<byte>(b, "X2");
				appendInterpolatedStringHandler.AppendLiteral(" ");
				stringBuilder2.Append(ref appendInterpolatedStringHandler);
			}
			if (hexData.Length > 0)
			{
				StringBuilder stringBuilder3 = hexData;
				int length = stringBuilder3.Length;
				stringBuilder3.Length = length - 1;
			}
			if (A_1.inc.BytePosition >= 500)
			{
				StringBuilder stringBuilder = hexData;
				StringBuilder stringBuilder4 = stringBuilder;
				StringBuilder.AppendInterpolatedStringHandler appendInterpolatedStringHandler = new StringBuilder.AppendInterpolatedStringHandler(45, 1, stringBuilder);
				appendInterpolatedStringHandler.AppendLiteral(" (data truncated, ");
				appendInterpolatedStringHandler.AppendFormatted<int>(A_1.inc.LengthBytes);
				appendInterpolatedStringHandler.AppendLiteral(" bytes in the full message)");
				stringBuilder4.Append(ref appendInterpolatedStringHandler);
			}
			A_1.inc.BitPosition = prevPos;
			string accountInfoName = INetSerializableStruct.<TryRead>g__AccountInfoToName|4_1<T>(A_1.sender);
			string identifier = "INetSerializableStruct.TryRead:" + accountInfoName;
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(38, 2);
			defaultInterpolatedStringHandler.AppendLiteral("Failed to read a message by ");
			defaultInterpolatedStringHandler.AppendFormatted(accountInfoName);
			defaultInterpolatedStringHandler.AppendLiteral(". Data: \"");
			defaultInterpolatedStringHandler.AppendFormatted<StringBuilder>(hexData);
			defaultInterpolatedStringHandler.AppendLiteral("\"");
			DebugConsole.ThrowErrorOnce(identifier, defaultInterpolatedStringHandler.ToStringAndClear(), e);
		}

		// Token: 0x060041B0 RID: 16816 RVA: 0x00246BC8 File Offset: 0x00244DC8
		[CompilerGenerated]
		internal static string <TryRead>g__AccountInfoToName|4_1<T>(AccountInfo info) where T : INetSerializableStruct
		{
			NetworkMember networkMember = GameMain.NetworkMember;
			IReadOnlyList<Client> connectedClients = ((networkMember != null) ? networkMember.ConnectedClients : null) ?? Array.Empty<Client>();
			foreach (Client c in connectedClients)
			{
				if (c.AccountInfo == info)
				{
					return c.Name;
				}
			}
			AccountId accountId;
			if (!info.AccountId.TryUnwrap(out accountId))
			{
				return "Unknown";
			}
			return accountId.StringRepresentation;
		}
	}
}
