using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x0200015E RID: 350
	public readonly struct CachedLocation : IEquatable<CachedLocation>
	{
		// Token: 0x06002AC2 RID: 10946 RVA: 0x001D85E0 File Offset: 0x001D67E0
		public CachedLocation(Vector2 Location, double RecalculationTime)
		{
			this.Location = Location;
			this.RecalculationTime = RecalculationTime;
		}

		// Token: 0x17000ABF RID: 2751
		// (get) Token: 0x06002AC3 RID: 10947 RVA: 0x001D85F0 File Offset: 0x001D67F0
		// (set) Token: 0x06002AC4 RID: 10948 RVA: 0x001D85F8 File Offset: 0x001D67F8
		public Vector2 Location { get; set; }

		// Token: 0x17000AC0 RID: 2752
		// (get) Token: 0x06002AC5 RID: 10949 RVA: 0x001D8601 File Offset: 0x001D6801
		// (set) Token: 0x06002AC6 RID: 10950 RVA: 0x001D8609 File Offset: 0x001D6809
		public double RecalculationTime { get; set; }

		// Token: 0x06002AC7 RID: 10951 RVA: 0x001D8614 File Offset: 0x001D6814
		[CompilerGenerated]
		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("CachedLocation");
			stringBuilder.Append(" { ");
			if (this.PrintMembers(stringBuilder))
			{
				stringBuilder.Append(' ');
			}
			stringBuilder.Append('}');
			return stringBuilder.ToString();
		}

		// Token: 0x06002AC8 RID: 10952 RVA: 0x001D8660 File Offset: 0x001D6860
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Location = ");
			builder.Append(this.Location.ToString());
			builder.Append(", RecalculationTime = ");
			builder.Append(this.RecalculationTime.ToString());
			return true;
		}

		// Token: 0x06002AC9 RID: 10953 RVA: 0x001D86BC File Offset: 0x001D68BC
		[CompilerGenerated]
		public static bool operator !=(CachedLocation left, CachedLocation right)
		{
			return !(left == right);
		}

		// Token: 0x06002ACA RID: 10954 RVA: 0x001D86C8 File Offset: 0x001D68C8
		[CompilerGenerated]
		public static bool operator ==(CachedLocation left, CachedLocation right)
		{
			return left.Equals(right);
		}

		// Token: 0x06002ACB RID: 10955 RVA: 0x001D86D2 File Offset: 0x001D68D2
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Vector2>.Default.GetHashCode(this.<Location>k__BackingField) * -1521134295 + EqualityComparer<double>.Default.GetHashCode(this.<RecalculationTime>k__BackingField);
		}

		// Token: 0x06002ACC RID: 10956 RVA: 0x001D86FB File Offset: 0x001D68FB
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CachedLocation && this.Equals((CachedLocation)obj);
		}

		// Token: 0x06002ACD RID: 10957 RVA: 0x001D8713 File Offset: 0x001D6913
		[CompilerGenerated]
		public bool Equals(CachedLocation other)
		{
			return EqualityComparer<Vector2>.Default.Equals(this.<Location>k__BackingField, other.<Location>k__BackingField) && EqualityComparer<double>.Default.Equals(this.<RecalculationTime>k__BackingField, other.<RecalculationTime>k__BackingField);
		}

		// Token: 0x06002ACE RID: 10958 RVA: 0x001D8745 File Offset: 0x001D6945
		[CompilerGenerated]
		public void Deconstruct(out Vector2 Location, out double RecalculationTime)
		{
			Location = this.Location;
			RecalculationTime = this.RecalculationTime;
		}
	}
}
