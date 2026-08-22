using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x020002CF RID: 719
	public readonly struct SerializableDateTime : IComparable<SerializableDateTime>
	{
		// Token: 0x0600306B RID: 12395 RVA: 0x0014D3BC File Offset: 0x0014B5BC
		public bool Equals(SerializableDateTime other)
		{
			return this.ToUtc().value.Equals(other.ToUtc().value);
		}

		// Token: 0x0600306C RID: 12396 RVA: 0x0014D3E8 File Offset: 0x0014B5E8
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

		// Token: 0x0600306D RID: 12397 RVA: 0x0014D40D File Offset: 0x0014B60D
		private static DateTime UnixEpoch(DateTimeKind kind)
		{
			return new DateTime(1970, 1, 1, 0, 0, 0, kind);
		}

		// Token: 0x0600306E RID: 12398 RVA: 0x0014D420 File Offset: 0x0014B620
		public SerializableDateTime(DateTime value)
		{
			this = new SerializableDateTime(value, default(SerializableTimeZone));
			if (value.Kind == DateTimeKind.Unspecified)
			{
				throw new Exception("Timezone required when constructing SerializableDateTime from DateTime of unspecified kind");
			}
			this.TimeZone = SerializableTimeZone.FromDateTime(value);
		}

		// Token: 0x0600306F RID: 12399 RVA: 0x0014D45D File Offset: 0x0014B65D
		public SerializableDateTime(DateTime value, SerializableTimeZone timeZone)
		{
			this.value = new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, DateTimeKind.Unspecified);
			this.TimeZone = timeZone;
		}

		// Token: 0x17000DE1 RID: 3553
		// (get) Token: 0x06003070 RID: 12400 RVA: 0x0014D49C File Offset: 0x0014B69C
		public static SerializableDateTime LocalNow
		{
			get
			{
				return new SerializableDateTime(DateTime.Now);
			}
		}

		// Token: 0x17000DE2 RID: 3554
		// (get) Token: 0x06003071 RID: 12401 RVA: 0x0014D4A8 File Offset: 0x0014B6A8
		public static SerializableDateTime UtcNow
		{
			get
			{
				return new SerializableDateTime(DateTime.UtcNow);
			}
		}

		// Token: 0x06003072 RID: 12402 RVA: 0x0014D4B4 File Offset: 0x0014B6B4
		public SerializableDateTime ToUtc()
		{
			return new SerializableDateTime(DateTime.SpecifyKind(this.value - this.TimeZone.Value, DateTimeKind.Utc));
		}

		// Token: 0x06003073 RID: 12403 RVA: 0x0014D4D7 File Offset: 0x0014B6D7
		public SerializableDateTime ToLocal()
		{
			return new SerializableDateTime(new DateTime(this.value.Ticks) - this.TimeZone.Value + SerializableTimeZone.LocalTimeZone.Value, SerializableTimeZone.LocalTimeZone);
		}

		// Token: 0x17000DE3 RID: 3555
		// (get) Token: 0x06003074 RID: 12404 RVA: 0x0014D512 File Offset: 0x0014B712
		public long Ticks
		{
			get
			{
				return this.value.Ticks;
			}
		}

		// Token: 0x06003075 RID: 12405 RVA: 0x0014D51F File Offset: 0x0014B71F
		public DateTime ToUtcValue()
		{
			return this.ToUtc().value;
		}

		// Token: 0x06003076 RID: 12406 RVA: 0x0014D52C File Offset: 0x0014B72C
		public DateTime ToLocalValue()
		{
			return this.ToLocal().value;
		}

		// Token: 0x06003077 RID: 12407 RVA: 0x0014D539 File Offset: 0x0014B739
		public static SerializableDateTime FromLocalUnixTime(long unixTime)
		{
			return new SerializableDateTime(SerializableDateTime.UnixEpoch(DateTimeKind.Local) + TimeSpan.FromSeconds((double)unixTime));
		}

		// Token: 0x06003078 RID: 12408 RVA: 0x0014D552 File Offset: 0x0014B752
		public static SerializableDateTime FromUtcUnixTime(long unixTime)
		{
			return new SerializableDateTime(SerializableDateTime.UnixEpoch(DateTimeKind.Utc) + TimeSpan.FromSeconds((double)unixTime));
		}

		// Token: 0x06003079 RID: 12409 RVA: 0x0014D56C File Offset: 0x0014B76C
		public long ToUnixTime()
		{
			return (this.value - SerializableDateTime.UnixEpoch(this.value.Kind)).Ticks / 10000000L;
		}

		// Token: 0x0600307A RID: 12410 RVA: 0x0014D5A3 File Offset: 0x0014B7A3
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

		// Token: 0x0600307B RID: 12411 RVA: 0x0014D5D4 File Offset: 0x0014B7D4
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

		// Token: 0x0600307C RID: 12412 RVA: 0x0014D6C8 File Offset: 0x0014B8C8
		[NullableContext(1)]
		public string ToLocalUserString()
		{
			return this.ToLocalValue().ToString(CultureInfo.InvariantCulture);
		}

		// Token: 0x0600307D RID: 12413 RVA: 0x0014D6E8 File Offset: 0x0014B8E8
		public override int GetHashCode()
		{
			return HashCode.Combine<int, int, int, int, int, int, int>(this.value.Year, this.value.Month, this.value.Day, this.value.Hour, this.value.Minute, this.value.Second, this.TimeZone.GetHashCode());
		}

		// Token: 0x0600307E RID: 12414 RVA: 0x0014D750 File Offset: 0x0014B950
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

		// Token: 0x0600307F RID: 12415 RVA: 0x0014D914 File Offset: 0x0014BB14
		public int CompareTo(SerializableDateTime other)
		{
			return this.ToUtc().value.CompareTo(other.ToUtc().value);
		}

		// Token: 0x06003080 RID: 12416 RVA: 0x0014D940 File Offset: 0x0014BB40
		public static bool operator <(in SerializableDateTime a, in SerializableDateTime b)
		{
			return a.CompareTo(b) < 0;
		}

		// Token: 0x06003081 RID: 12417 RVA: 0x0014D951 File Offset: 0x0014BB51
		public static bool operator >(in SerializableDateTime a, in SerializableDateTime b)
		{
			return a.CompareTo(b) > 0;
		}

		// Token: 0x06003082 RID: 12418 RVA: 0x0014D962 File Offset: 0x0014BB62
		public static bool operator ==(in SerializableDateTime a, in SerializableDateTime b)
		{
			return a.CompareTo(b) == 0;
		}

		// Token: 0x06003083 RID: 12419 RVA: 0x0014D973 File Offset: 0x0014BB73
		public static bool operator !=(in SerializableDateTime a, in SerializableDateTime b)
		{
			return !(a == b);
		}

		// Token: 0x06003084 RID: 12420 RVA: 0x0014D97F File Offset: 0x0014BB7F
		public static SerializableDateTime operator +(in SerializableDateTime dt, in TimeSpan ts)
		{
			return new SerializableDateTime(dt.value + ts, dt.TimeZone);
		}

		// Token: 0x06003085 RID: 12421 RVA: 0x0014D99D File Offset: 0x0014BB9D
		public static SerializableDateTime operator -(in SerializableDateTime dt, in TimeSpan ts)
		{
			return new SerializableDateTime(dt.value - ts, dt.TimeZone);
		}

		// Token: 0x06003086 RID: 12422 RVA: 0x0014D9BB File Offset: 0x0014BBBB
		public static TimeSpan operator -(in SerializableDateTime a, in SerializableDateTime b)
		{
			return a.ToUtc().value - b.ToUtc().value;
		}

		// Token: 0x0400183F RID: 6207
		private readonly DateTime value;

		// Token: 0x04001840 RID: 6208
		public readonly SerializableTimeZone TimeZone;

		// Token: 0x02000B71 RID: 2929
		[CompilerGenerated]
		private static class <>O
		{
			// Token: 0x0400397A RID: 14714
			public static Func<char, bool> <0>__IsLetter;
		}
	}
}
