using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x0200039A RID: 922
	public readonly struct SerializableDateTime : IComparable<SerializableDateTime>
	{
		// Token: 0x060044E9 RID: 17641 RVA: 0x00266E18 File Offset: 0x00265018
		public bool Equals(SerializableDateTime other)
		{
			return this.ToUtc().value.Equals(other.ToUtc().value);
		}

		// Token: 0x060044EA RID: 17642 RVA: 0x00266E44 File Offset: 0x00265044
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			if (obj is SerializableDateTime)
			{
				SerializableDateTime other = (SerializableDateTime)obj;
				return this.Equals(other);
			}
			return false;
		}

		// Token: 0x060044EB RID: 17643 RVA: 0x00266E69 File Offset: 0x00265069
		private static DateTime UnixEpoch(DateTimeKind kind)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, kind);
		}

		// Token: 0x060044EC RID: 17644 RVA: 0x00266E7C File Offset: 0x0026507C
		public SerializableDateTime(DateTime value)
		{
			this = new SerializableDateTime(value, default(SerializableTimeZone));
			if (value.Kind == DateTimeKind.Unspecified)
			{
				throw new Exception("Timezone required when constructing SerializableDateTime from DateTime of unspecified kind");
			}
			this.TimeZone = SerializableTimeZone.FromDateTime(value);
		}

		// Token: 0x060044ED RID: 17645 RVA: 0x00266EB9 File Offset: 0x002650B9
		public SerializableDateTime(DateTime value, SerializableTimeZone timeZone)
		{
			this.value = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, DateTimeKind.Unspecified);
			this.TimeZone = timeZone;
		}

		// Token: 0x170011D6 RID: 4566
		// (get) Token: 0x060044EE RID: 17646 RVA: 0x00266EF8 File Offset: 0x002650F8
		public static SerializableDateTime LocalNow
		{
			get
			{
				return new SerializableDateTime(DateTime.Now);
			}
		}

		// Token: 0x170011D7 RID: 4567
		// (get) Token: 0x060044EF RID: 17647 RVA: 0x00266F04 File Offset: 0x00265104
		public static SerializableDateTime UtcNow
		{
			get
			{
				return new SerializableDateTime(DateTime.UtcNow);
			}
		}

		// Token: 0x060044F0 RID: 17648 RVA: 0x00266F10 File Offset: 0x00265110
		public SerializableDateTime ToUtc()
		{
			return new SerializableDateTime(DateTime.SpecifyKind(this.value - this.TimeZone.Value, DateTimeKind.Utc));
		}

		// Token: 0x060044F1 RID: 17649 RVA: 0x00266F33 File Offset: 0x00265133
		public SerializableDateTime ToLocal()
		{
			return new SerializableDateTime(new DateTime(this.value.Ticks) - this.TimeZone.Value + SerializableTimeZone.LocalTimeZone.Value, SerializableTimeZone.LocalTimeZone);
		}

		// Token: 0x170011D8 RID: 4568
		// (get) Token: 0x060044F2 RID: 17650 RVA: 0x00266F6E File Offset: 0x0026516E
		public long Ticks
		{
			get
			{
				return this.value.Ticks;
			}
		}

		// Token: 0x060044F3 RID: 17651 RVA: 0x00266F7B File Offset: 0x0026517B
		public DateTime ToUtcValue()
		{
			return this.ToUtc().value;
		}

		// Token: 0x060044F4 RID: 17652 RVA: 0x00266F88 File Offset: 0x00265188
		public DateTime ToLocalValue()
		{
			return this.ToLocal().value;
		}

		// Token: 0x060044F5 RID: 17653 RVA: 0x00266F95 File Offset: 0x00265195
		public static SerializableDateTime FromLocalUnixTime(long unixTime)
		{
			return new SerializableDateTime(SerializableDateTime.UnixEpoch(DateTimeKind.Local) + TimeSpan.FromSeconds((double)unixTime));
		}

		// Token: 0x060044F6 RID: 17654 RVA: 0x00266FAE File Offset: 0x002651AE
		public static SerializableDateTime FromUtcUnixTime(long unixTime)
		{
			return new SerializableDateTime(SerializableDateTime.UnixEpoch(DateTimeKind.Utc) + TimeSpan.FromSeconds((double)unixTime));
		}

		// Token: 0x060044F7 RID: 17655 RVA: 0x00266FC8 File Offset: 0x002651C8
		public long ToUnixTime()
		{
			return (this.value - SerializableDateTime.UnixEpoch(this.value.Kind)).Ticks / 10000000L;
		}

		// Token: 0x060044F8 RID: 17656 RVA: 0x00266FFF File Offset: 0x002651FF
		[NullableContext(1)]
		private static string MakeString([TupleElementNames(new string[]
		{
			"Value",
			"Suffix"
		})] [Nullable(new byte[]
		{
			1,
			0,
			1
		})] params ValueTuple<long, string>[] parts)
		{
			return string.Join<string>(' ', from p in parts
			select p.Item1.ToString().PadLeft(2, '0') + p.Item2);
		}

		// Token: 0x060044F9 RID: 17657 RVA: 0x00267030 File Offset: 0x00265230
		[NullableContext(1)]
		public override string ToString()
		{
			string str = SerializableDateTime.MakeString(new ValueTuple<long, string>[]
			{
				new ValueTuple<long, string>((long)this.value.Year, "Y"),
				new ValueTuple<long, string>((long)this.value.Month, "M"),
				new ValueTuple<long, string>((long)this.value.Day, "D"),
				new ValueTuple<long, string>((long)this.value.Hour, "HR"),
				new ValueTuple<long, string>((long)this.value.Minute, "MIN"),
				new ValueTuple<long, string>((long)this.value.Second, "SEC")
			});
			DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(1, 1);
			defaultInterpolatedStringHandler.AppendLiteral(" ");
			defaultInterpolatedStringHandler.AppendFormatted<SerializableTimeZone>(this.TimeZone);
			return str + defaultInterpolatedStringHandler.ToStringAndClear();
		}

		// Token: 0x060044FA RID: 17658 RVA: 0x00267124 File Offset: 0x00265324
		[NullableContext(1)]
		public string ToLocalUserString()
		{
			return this.ToLocalValue().ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x060044FB RID: 17659 RVA: 0x00267144 File Offset: 0x00265344
		public override int GetHashCode()
		{
			return HashCode.Combine<int, int, int, int, int, int, int>(this.value.Year, this.value.Month, this.value.Day, this.value.Hour, this.value.Minute, this.value.Second, this.TimeZone.GetHashCode());
		}

		// Token: 0x060044FC RID: 17660 RVA: 0x002671AC File Offset: 0x002653AC
		public static Option<SerializableDateTime> Parse([Nullable(1)] string str)
		{
			long unixTime;
			if (long.TryParse(str, out unixTime) && unixTime > 0L && (double)unixTime < (DateTime.MaxValue - SerializableDateTime.UnixEpoch(DateTimeKind.Utc)).TotalSeconds)
			{
				return Option<SerializableDateTime>.Some(SerializableDateTime.FromUtcUnixTime(unixTime));
			}
			string[] split = str.Split(' ', StringSplitOptions.None);
			int year = 0;
			int month = 0;
			int day = 0;
			int hour = 0;
			int minute = 0;
			int second = 0;
			SerializableTimeZone timeZone = default(SerializableTimeZone);
			foreach (string part in split)
			{
				SerializableTimeZone parsedTimeZone;
				if (SerializableTimeZone.Parse(part).TryUnwrap(out parsedTimeZone))
				{
					timeZone = parsedTimeZone;
				}
				else
				{
					string separator = "";
					IEnumerable<char> source = part;
					Func<char, bool> predicate;
					if ((predicate = SerializableDateTime.<>O.<0>__IsLetter) == null)
					{
						predicate = (SerializableDateTime.<>O.<0>__IsLetter = new Func<char, bool>(char.IsLetter));
					}
					Identifier suffix = string.Join<char>(separator, source.Where(predicate)).ToIdentifier();
					if (part.EndsWith(suffix.Value))
					{
						string text = part;
						int length = suffix.Value.Length;
						int value;
						if (int.TryParse(text.Substring(0, text.Length - length), NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
						{
							if (suffix == "Y")
							{
								year = value;
							}
							else if (suffix == "M")
							{
								month = value;
							}
							else if (suffix == "D")
							{
								day = value;
							}
							else if (suffix == "HR")
							{
								hour = value;
							}
							else if (suffix == "MIN")
							{
								minute = value;
							}
							else if (suffix == "SEC")
							{
								second = value;
							}
						}
					}
				}
			}
			if (year > 0 && month > 0 && day > 0)
			{
				return Option<SerializableDateTime>.Some(new SerializableDateTime(new DateTime(year, month, day, hour, minute, second), timeZone));
			}
			return Option<SerializableDateTime>.None();
		}

		// Token: 0x060044FD RID: 17661 RVA: 0x00267370 File Offset: 0x00265570
		public int CompareTo(SerializableDateTime other)
		{
			return this.ToUtc().value.CompareTo(other.ToUtc().value);
		}

		// Token: 0x060044FE RID: 17662 RVA: 0x0026739C File Offset: 0x0026559C
		public static bool operator <(in SerializableDateTime a, in SerializableDateTime b)
		{
			return a.CompareTo(b) < 0;
		}

		// Token: 0x060044FF RID: 17663 RVA: 0x002673AD File Offset: 0x002655AD
		public static bool operator >(in SerializableDateTime a, in SerializableDateTime b)
		{
			return a.CompareTo(b) > 0;
		}

		// Token: 0x06004500 RID: 17664 RVA: 0x002673BE File Offset: 0x002655BE
		public static bool operator ==(in SerializableDateTime a, in SerializableDateTime b)
		{
			return a.CompareTo(b) == 0;
		}

		// Token: 0x06004501 RID: 17665 RVA: 0x002673CF File Offset: 0x002655CF
		public static bool operator !=(in SerializableDateTime a, in SerializableDateTime b)
		{
			return !(a == b);
		}

		// Token: 0x06004502 RID: 17666 RVA: 0x002673DB File Offset: 0x002655DB
		public static SerializableDateTime operator +(in SerializableDateTime dt, in TimeSpan ts)
		{
			return new SerializableDateTime(dt.value + ts, dt.TimeZone);
		}

		// Token: 0x06004503 RID: 17667 RVA: 0x002673F9 File Offset: 0x002655F9
		public static SerializableDateTime operator -(in SerializableDateTime dt, in TimeSpan ts)
		{
			return new SerializableDateTime(dt.value - ts, dt.TimeZone);
		}

		// Token: 0x06004504 RID: 17668 RVA: 0x00267417 File Offset: 0x00265617
		public static TimeSpan operator -(in SerializableDateTime a, in SerializableDateTime b)
		{
			return a.ToUtc().value - b.ToUtc().value;
		}

		// Token: 0x04002411 RID: 9233
		private readonly DateTime value;

		// Token: 0x04002412 RID: 9234
		public readonly SerializableTimeZone TimeZone;

		// Token: 0x020010C2 RID: 4290
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x04005998 RID: 22936
			public static Func<char, bool> <0>__IsLetter;
		}
	}
}
