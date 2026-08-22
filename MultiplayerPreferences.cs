using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000125 RID: 293
	[NullableContext(1)]
	[Nullable(0)]
	internal class MultiplayerPreferences
	{
		// Token: 0x17000A52 RID: 2642
		// (get) Token: 0x060027D7 RID: 10199 RVA: 0x001BBBFC File Offset: 0x001B9DFC
		// (set) Token: 0x060027D8 RID: 10200 RVA: 0x001BBC03 File Offset: 0x001B9E03
		public static MultiplayerPreferences Instance { get; private set; } = new MultiplayerPreferences();

		// Token: 0x060027D9 RID: 10201 RVA: 0x001BBC0C File Offset: 0x001B9E0C
		private MultiplayerPreferences()
		{
		}

		// Token: 0x060027DA RID: 10202 RVA: 0x001BBC80 File Offset: 0x001B9E80
		private MultiplayerPreferences(IEnumerable<XElement> elements)
		{
			foreach (XElement element in elements)
			{
				this.PlayerName = element.GetAttributeString("name", this.PlayerName);
				this.TagSet.UnionWith(element.GetAttributeIdentifierArray("tags", Array.Empty<Identifier>(), true));
				this.HairIndex = element.GetAttributeInt("HairIndex", this.HairIndex);
				this.BeardIndex = element.GetAttributeInt("BeardIndex", this.BeardIndex);
				this.MoustacheIndex = element.GetAttributeInt("MoustacheIndex", this.MoustacheIndex);
				this.FaceAttachmentIndex = element.GetAttributeInt("FaceAttachmentIndex", this.FaceAttachmentIndex);
				this.HairColor = element.GetAttributeColor("HairColor", this.HairColor);
				this.FacialHairColor = element.GetAttributeColor("FacialHairColor", this.FacialHairColor);
				this.SkinColor = element.GetAttributeColor("SkinColor", this.SkinColor);
				foreach (XElement subElement in element.GetChildElements("job", StringComparison.OrdinalIgnoreCase))
				{
					this.JobPreferences.Add(new MultiplayerPreferences.JobPreference(subElement));
				}
			}
		}

		// Token: 0x060027DB RID: 10203 RVA: 0x001BBE64 File Offset: 0x001BA064
		public static void Init([Nullable(new byte[]
		{
			1,
			2
		})] params XElement[] elements)
		{
			MultiplayerPreferences.Instance = new MultiplayerPreferences(from e in elements
			where e != null
			select e);
		}

		// Token: 0x060027DC RID: 10204 RVA: 0x001BBE98 File Offset: 0x001BA098
		public void SaveTo(XElement element)
		{
			element.SetAttributeValue("name", this.PlayerName);
			element.SetAttributeValue("tags", string.Join<Identifier>(",", this.TagSet));
			element.SetAttributeValue("HairIndex", this.HairIndex);
			element.SetAttributeValue("BeardIndex", this.BeardIndex);
			element.SetAttributeValue("MoustacheIndex", this.MoustacheIndex);
			element.SetAttributeValue("FaceAttachmentIndex", this.FaceAttachmentIndex);
			element.SetAttributeValue("HairColor", this.HairColor.ToStringHex());
			element.SetAttributeValue("FacialHairColor", this.FacialHairColor.ToStringHex());
			element.SetAttributeValue("SkinColor", this.SkinColor.ToStringHex());
			foreach (MultiplayerPreferences.JobPreference jobPreference in this.JobPreferences)
			{
				element.Add(new XElement("job", new object[]
				{
					new XAttribute("identifier", jobPreference.JobIdentifier.Value),
					new XAttribute("variant", jobPreference.Variant.ToString(CultureInfo.InvariantCulture))
				}));
			}
		}

		// Token: 0x060027DD RID: 10205 RVA: 0x001BC034 File Offset: 0x001BA234
		public bool AreJobPreferencesEqual(IReadOnlyList<MultiplayerPreferences.JobPreference> other)
		{
			return this.JobPreferences.SequenceEqual(other);
		}

		// Token: 0x04001442 RID: 5186
		public readonly List<MultiplayerPreferences.JobPreference> JobPreferences = new List<MultiplayerPreferences.JobPreference>();

		// Token: 0x04001443 RID: 5187
		public CharacterTeamType TeamPreference;

		// Token: 0x04001444 RID: 5188
		public string PlayerName = string.Empty;

		// Token: 0x04001445 RID: 5189
		public readonly HashSet<Identifier> TagSet = new HashSet<Identifier>();

		// Token: 0x04001446 RID: 5190
		public int HairIndex = -1;

		// Token: 0x04001447 RID: 5191
		public int BeardIndex = -1;

		// Token: 0x04001448 RID: 5192
		public int MoustacheIndex = -1;

		// Token: 0x04001449 RID: 5193
		public int FaceAttachmentIndex = -1;

		// Token: 0x0400144A RID: 5194
		public Color HairColor = Color.Black;

		// Token: 0x0400144B RID: 5195
		public Color FacialHairColor = Color.Black;

		// Token: 0x0400144C RID: 5196
		public Color SkinColor = Color.Black;

		// Token: 0x02000D33 RID: 3379
		[NullableContext(0)]
		public readonly struct JobPreference
		{
			// Token: 0x0600803B RID: 32827 RVA: 0x00393F92 File Offset: 0x00392192
			public JobPreference(Identifier jobIdentifier, int variant)
			{
				this.JobIdentifier = jobIdentifier;
				this.Variant = variant;
			}

			// Token: 0x0600803C RID: 32828 RVA: 0x00393FA2 File Offset: 0x003921A2
			[NullableContext(1)]
			public JobPreference(XElement element)
			{
				this = new MultiplayerPreferences.JobPreference(element.GetAttributeIdentifier("identifier", Identifier.Empty), element.GetAttributeInt("variant", -1));
			}

			// Token: 0x0600803D RID: 32829 RVA: 0x00393FC6 File Offset: 0x003921C6
			public static bool operator ==(MultiplayerPreferences.JobPreference a, MultiplayerPreferences.JobPreference b)
			{
				return a.JobIdentifier == b.JobIdentifier && a.Variant == b.Variant;
			}

			// Token: 0x0600803E RID: 32830 RVA: 0x00393FED File Offset: 0x003921ED
			public static bool operator !=(MultiplayerPreferences.JobPreference a, MultiplayerPreferences.JobPreference b)
			{
				return !(a == b);
			}

			// Token: 0x0600803F RID: 32831 RVA: 0x00393FFC File Offset: 0x003921FC
			[NullableContext(2)]
			public override bool Equals(object obj)
			{
				if (obj is MultiplayerPreferences.JobPreference)
				{
					MultiplayerPreferences.JobPreference jp = (MultiplayerPreferences.JobPreference)obj;
					return jp == this;
				}
				return false;
			}

			// Token: 0x06008040 RID: 32832 RVA: 0x00394026 File Offset: 0x00392226
			public bool Equals(MultiplayerPreferences.JobPreference other)
			{
				return other == this;
			}

			// Token: 0x06008041 RID: 32833 RVA: 0x00394034 File Offset: 0x00392234
			public override int GetHashCode()
			{
				return HashCode.Combine<Identifier, int>(this.JobIdentifier, this.Variant);
			}

			// Token: 0x04004E9F RID: 20127
			public readonly Identifier JobIdentifier;

			// Token: 0x04004EA0 RID: 20128
			public readonly int Variant;
		}
	}
}
