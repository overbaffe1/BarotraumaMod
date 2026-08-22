using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000053 RID: 83
	public readonly struct CachedLocation : IEquatable<CachedLocation>
	{
		// Token: 0x06000C2D RID: 3117 RVA: 0x000741E4 File Offset: 0x000723E4
		public CachedLocation(Vector2 Location, double RecalculationTime)
		{
			this.Location = Location;
			this.RecalculationTime = RecalculationTime;
		}

		// Token: 0x17000336 RID: 822
		// (get) Token: 0x06000C2E RID: 3118 RVA: 0x000741F4 File Offset: 0x000723F4
		// (set) Token: 0x06000C2F RID: 3119 RVA: 0x000741FC File Offset: 0x000723FC
		public Vector2 Location { get; set; }

		// Token: 0x17000337 RID: 823
		// (get) Token: 0x06000C30 RID: 3120 RVA: 0x00074205 File Offset: 0x00072405
		// (set) Token: 0x06000C31 RID: 3121 RVA: 0x0007420D File Offset: 0x0007240D
		public double RecalculationTime { get; set; }

		// Token: 0x06000C32 RID: 3122 RVA: 0x00074218 File Offset: 0x00072418
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

		// Token: 0x06000C33 RID: 3123 RVA: 0x00074264 File Offset: 0x00072464
		[CompilerGenerated]
		private bool PrintMembers(StringBuilder builder)
		{
			builder.Append("Location = ");
			builder.Append(this.Location.ToString());
			builder.Append(", RecalculationTime = ");
			builder.Append(this.RecalculationTime.ToString());
			return true;
		}

		// Token: 0x06000C34 RID: 3124 RVA: 0x000742C0 File Offset: 0x000724C0
		[CompilerGenerated]
		public static bool operator !=(CachedLocation left, CachedLocation right)
		{
			return !(left == right);
		}

		// Token: 0x06000C35 RID: 3125 RVA: 0x000742CC File Offset: 0x000724CC
		[CompilerGenerated]
		public static bool operator ==(CachedLocation left, CachedLocation right)
		{
			return left.Equals(right);
		}

		// Token: 0x06000C36 RID: 3126 RVA: 0x000742D6 File Offset: 0x000724D6
		[CompilerGenerated]
		public override int GetHashCode()
		{
			return EqualityComparer<Vector2>.Default.GetHashCode(this.<Location>k__BackingField) * -1521134295 + EqualityComparer<double>.Default.GetHashCode(this.<RecalculationTime>k__BackingField);
		}

		// Token: 0x06000C37 RID: 3127 RVA: 0x000742FF File Offset: 0x000724FF
		[CompilerGenerated]
		public override bool Equals(object obj)
		{
			return obj is CachedLocation && this.Equals((CachedLocation)obj);
		}

		// Token: 0x06000C38 RID: 3128 RVA: 0x00074317 File Offset: 0x00072517
		[CompilerGenerated]
		public bool Equals(CachedLocation other)
		{
			return EqualityComparer<Vector2>.Default.Equals(this.<Location>k__BackingField, other.<Location>k__BackingField) && EqualityComparer<double>.Default.Equals(this.<RecalculationTime>k__BackingField, other.<RecalculationTime>k__BackingField);
		}

		// Token: 0x06000C39 RID: 3129 RVA: 0x00074349 File Offset: 0x00072549
		[CompilerGenerated]
		public void Deconstruct(out Vector2 Location, out double RecalculationTime)
		{
			Location = this.Location;
			RecalculationTime = this.RecalculationTime;
		}
	}
}
