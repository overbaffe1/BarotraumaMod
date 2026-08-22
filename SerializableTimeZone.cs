using System;
using System.Runtime.CompilerServices;

namespace Barotrauma
{
	// Token: 0x02000399 RID: 921
	public readonly struct SerializableTimeZone
	{
		// Token: 0x060044E2 RID: 17634 RVA: 0x00266B34 File Offset: 0x00264D34
		public SerializableTimeZone(TimeSpan value)
		{
			this.Value = new TimeSpan(value.Hours, value.Minutes, 0);
			this.hours = Math.Abs(value.Hours);
			this.minutes = Math.Abs(value.Minutes);
			this.sign = ((this.Value.Ticks < 0L) ? '-' : '+');
		}

		// Token: 0x060044E3 RID: 17635 RVA: 0x00266B9C File Offset: 0x00264D9C
		[NullableContext(1)]
		public override string ToString()
		{
			int num = this.hours;
			int num2 = this.minutes;
			if (num == 0)
			{
				if (num2 == 0)
				{
					return "UTC";
				}
			}
			else if (num2 == 0)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(3, 2);
				defaultInterpolatedStringHandler.AppendLiteral("UTC");
				defaultInterpolatedStringHandler.AppendFormatted<char>(this.sign);
				defaultInterpolatedStringHandler.AppendFormatted<int>(this.hours);
				return defaultInterpolatedStringHandler.ToStringAndClear();
			}
			string result;
			if (num2 >= 10)
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler2 = new DefaultInterpolatedStringHandler(4, 3);
				defaultInterpolatedStringHandler2.AppendLiteral("UTC");
				defaultInterpolatedStringHandler2.AppendFormatted<char>(this.sign);
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.hours);
				defaultInterpolatedStringHandler2.AppendLiteral(":");
				defaultInterpolatedStringHandler2.AppendFormatted<int>(this.minutes);
				result = defaultInterpolatedStringHandler2.ToStringAndClear();
			}
			else
			{
				DefaultInterpolatedStringHandler defaultInterpolatedStringHandler3 = new DefaultInterpolatedStringHandler(5, 3);
				defaultInterpolatedStringHandler3.AppendLiteral("UTC");
				defaultInterpolatedStringHandler3.AppendFormatted<char>(this.sign);
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.hours);
				defaultInterpolatedStringHandler3.AppendLiteral(":0");
				defaultInterpolatedStringHandler3.AppendFormatted<int>(this.minutes);
				result = defaultInterpolatedStringHandler3.ToStringAndClear();
			}
			return result;
		}

		// Token: 0x060044E4 RID: 17636 RVA: 0x00266CB6 File Offset: 0x00264EB6
		public override int GetHashCode()
		{
			return HashCode.Combine<bool, int, int>(this.Value.Ticks < 0L, this.hours, this.minutes);
		}

		// Token: 0x060044E5 RID: 17637 RVA: 0x00266CD8 File Offset: 0x00264ED8
		public static SerializableTimeZone FromDateTime(DateTime dateTime)
		{
			if (dateTime.Kind == DateTimeKind.Unspecified)
			{
				throw new InvalidOperationException("Cannot determine timezone for DateTime of unspecified kind");
			}
			DateTime utcDateTime = dateTime.ToUniversalTime();
			return new SerializableTimeZone(dateTime - utcDateTime);
		}

		// Token: 0x170011D5 RID: 4565
		// (get) Token: 0x060044E6 RID: 17638 RVA: 0x00266D0D File Offset: 0x00264F0D
		public static SerializableTimeZone LocalTimeZone
		{
			get
			{
				return SerializableTimeZone.FromDateTime(DateTime.Now);
			}
		}

		// Token: 0x060044E7 RID: 17639 RVA: 0x00266D1C File Offset: 0x00264F1C
		public static Option<SerializableTimeZone> Parse([Nullable(1)] string str)
		{
			if (!str.StartsWith("UTC", StringComparison.OrdinalIgnoreCase))
			{
				return Option<SerializableTimeZone>.None();
			}
			string timeZoneStr = str.Substring(3);
			SerializableTimeZone.<>c__DisplayClass10_0 CS$<>8__locals1;
			CS$<>8__locals1.negative = timeZoneStr.StartsWith("-");
			if (!CS$<>8__locals1.negative && !timeZoneStr.StartsWith("+"))
			{
				return Option<SerializableTimeZone>.None();
			}
			timeZoneStr = str.Substring(4);
			int hrMinSeparator = timeZoneStr.IndexOf(':');
			int timeZoneHours2;
			if (hrMinSeparator > 0)
			{
				int timeZoneHours;
				int timeZoneMinutes;
				if (int.TryParse(timeZoneStr.Substring(0, hrMinSeparator), out timeZoneHours) && int.TryParse(timeZoneStr.Substring(hrMinSeparator + 1), out timeZoneMinutes))
				{
					return Option<SerializableTimeZone>.Some(new SerializableTimeZone(SerializableTimeZone.<Parse>g__makeTimeSpan|10_0(timeZoneHours, timeZoneMinutes, ref CS$<>8__locals1)));
				}
			}
			else if (int.TryParse(timeZoneStr, out timeZoneHours2))
			{
				return Option<SerializableTimeZone>.Some(new SerializableTimeZone(SerializableTimeZone.<Parse>g__makeTimeSpan|10_0(timeZoneHours2, 0, ref CS$<>8__locals1)));
			}
			return Option<SerializableTimeZone>.None();
		}

		// Token: 0x060044E8 RID: 17640 RVA: 0x00266DE9 File Offset: 0x00264FE9
		[CompilerGenerated]
		internal static TimeSpan <Parse>g__makeTimeSpan|10_0(int hours, int minutes, ref SerializableTimeZone.<>c__DisplayClass10_0 A_2)
		{
			return new TimeSpan(((long)hours * 36000000000L + (long)minutes * 600000000L) * (A_2.negative ? -1L : 1L));
		}

		// Token: 0x0400240D RID: 9229
		public readonly TimeSpan Value;

		// Token: 0x0400240E RID: 9230
		private readonly int hours;

		// Token: 0x0400240F RID: 9231
		private readonly int minutes;

		// Token: 0x04002410 RID: 9232
		private readonly char sign;
	}
}
