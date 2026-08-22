using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Barotrauma
{
	// Token: 0x02000163 RID: 355
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class ContentXElement
	{
		// Token: 0x1700084F RID: 2127
		// (get) Token: 0x06001CF5 RID: 7413 RVA: 0x000D0D0E File Offset: 0x000CEF0E
		// (set) Token: 0x06001CF6 RID: 7414 RVA: 0x000D0D16 File Offset: 0x000CEF16
		[Nullable(2)]
		public ContentPackage ContentPackage { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x06001CF7 RID: 7415 RVA: 0x000D0D1F File Offset: 0x000CEF1F
		public ContentXElement([Nullable(2)] ContentPackage contentPackage, XElement element)
		{
			this.ContentPackage = contentPackage;
			this.Element = element;
		}

		// Token: 0x06001CF8 RID: 7416 RVA: 0x000D0D35 File Offset: 0x000CEF35
		[NullableContext(2)]
		[return: NotNullIfNotNull("cxe")]
		public static implicit operator XElement(ContentXElement cxe)
		{
			if (cxe == null)
			{
				return null;
			}
			return cxe.Element;
		}

		// Token: 0x17000850 RID: 2128
		// (get) Token: 0x06001CF9 RID: 7417 RVA: 0x000D0D42 File Offset: 0x000CEF42
		public XName Name
		{
			get
			{
				return this.Element.Name;
			}
		}

		// Token: 0x06001CFA RID: 7418 RVA: 0x000D0D4F File Offset: 0x000CEF4F
		public Identifier NameAsIdentifier()
		{
			return this.Element.NameAsIdentifier();
		}

		// Token: 0x17000851 RID: 2129
		// (get) Token: 0x06001CFB RID: 7419 RVA: 0x000D0D5C File Offset: 0x000CEF5C
		public string BaseUri
		{
			get
			{
				return this.Element.BaseUri;
			}
		}

		// Token: 0x17000852 RID: 2130
		// (get) Token: 0x06001CFC RID: 7420 RVA: 0x000D0D69 File Offset: 0x000CEF69
		[Nullable(2)]
		public XDocument Document
		{
			[NullableContext(2)]
			get
			{
				return this.Element.Document;
			}
		}

		// Token: 0x06001CFD RID: 7421 RVA: 0x000D0D76 File Offset: 0x000CEF76
		[NullableContext(2)]
		public ContentXElement FirstElement()
		{
			return this.Elements().FirstOrDefault<ContentXElement>();
		}

		// Token: 0x17000853 RID: 2131
		// (get) Token: 0x06001CFE RID: 7422 RVA: 0x000D0D83 File Offset: 0x000CEF83
		[Nullable(2)]
		public ContentXElement Parent
		{
			[NullableContext(2)]
			get
			{
				if (this.Element.Parent != null)
				{
					return new ContentXElement(this.ContentPackage, this.Element.Parent);
				}
				return null;
			}
		}

		// Token: 0x17000854 RID: 2132
		// (get) Token: 0x06001CFF RID: 7423 RVA: 0x000D0DAA File Offset: 0x000CEFAA
		public bool HasElements
		{
			get
			{
				return this.Element.HasElements;
			}
		}

		// Token: 0x06001D00 RID: 7424 RVA: 0x000D0DB7 File Offset: 0x000CEFB7
		public bool IsOverride()
		{
			return this.Element.IsOverride();
		}

		// Token: 0x06001D01 RID: 7425 RVA: 0x000D0DC4 File Offset: 0x000CEFC4
		public bool ComesAfter(ContentXElement other)
		{
			return this.Element.ComesAfter(other.Element);
		}

		// Token: 0x06001D02 RID: 7426 RVA: 0x000D0DD8 File Offset: 0x000CEFD8
		[return: Nullable(2)]
		public ContentXElement GetChildElement(string name)
		{
			XElement elem = this.Element.GetChildElement(name, StringComparison.OrdinalIgnoreCase);
			if (elem == null)
			{
				return null;
			}
			return new ContentXElement(this.ContentPackage, elem);
		}

		// Token: 0x06001D03 RID: 7427 RVA: 0x000D0E04 File Offset: 0x000CF004
		public IEnumerable<ContentXElement> Elements()
		{
			return from e in this.Element.Elements()
			select new ContentXElement(this.ContentPackage, e);
		}

		// Token: 0x06001D04 RID: 7428 RVA: 0x000D0E22 File Offset: 0x000CF022
		public IEnumerable<ContentXElement> ElementsBeforeSelf()
		{
			return from e in this.Element.ElementsBeforeSelf()
			select new ContentXElement(this.ContentPackage, e);
		}

		// Token: 0x06001D05 RID: 7429 RVA: 0x000D0E40 File Offset: 0x000CF040
		public IEnumerable<ContentXElement> Descendants()
		{
			return from e in this.Element.Descendants()
			select new ContentXElement(this.ContentPackage, e);
		}

		// Token: 0x06001D06 RID: 7430 RVA: 0x000D0E60 File Offset: 0x000CF060
		public IEnumerable<ContentXElement> GetChildElements(string name)
		{
			return from e in this.Elements()
			where string.Equals(name, e.Name.LocalName, StringComparison.OrdinalIgnoreCase)
			select e;
		}

		// Token: 0x06001D07 RID: 7431 RVA: 0x000D0E91 File Offset: 0x000CF091
		[return: Nullable(2)]
		public XAttribute GetAttribute(string name)
		{
			return this.Element.GetAttribute(name, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x06001D08 RID: 7432 RVA: 0x000D0EA0 File Offset: 0x000CF0A0
		public IEnumerable<XAttribute> Attributes()
		{
			return this.Element.Attributes();
		}

		// Token: 0x06001D09 RID: 7433 RVA: 0x000D0EAD File Offset: 0x000CF0AD
		public IEnumerable<XAttribute> Attributes(string name)
		{
			return this.Element.Attributes(name);
		}

		// Token: 0x06001D0A RID: 7434 RVA: 0x000D0EC0 File Offset: 0x000CF0C0
		public string ElementInnerText()
		{
			return this.Element.ElementInnerText();
		}

		// Token: 0x06001D0B RID: 7435 RVA: 0x000D0ECD File Offset: 0x000CF0CD
		public Identifier GetAttributeIdentifier(string key, string def)
		{
			return this.Element.GetAttributeIdentifier(key, def);
		}

		// Token: 0x06001D0C RID: 7436 RVA: 0x000D0EDC File Offset: 0x000CF0DC
		public Identifier GetAttributeIdentifier(string key, Identifier def)
		{
			return this.Element.GetAttributeIdentifier(key, def);
		}

		// Token: 0x06001D0D RID: 7437 RVA: 0x000D0EEB File Offset: 0x000CF0EB
		[return: NotNullIfNotNull("def")]
		public Identifier[] GetAttributeIdentifierArray(Identifier[] def, params string[] keys)
		{
			return this.Element.GetAttributeIdentifierArray(def, keys);
		}

		// Token: 0x06001D0E RID: 7438 RVA: 0x000D0EFA File Offset: 0x000CF0FA
		[return: NotNullIfNotNull("def")]
		public Identifier[] GetAttributeIdentifierArray(string key, Identifier[] def, bool trim = true)
		{
			return this.Element.GetAttributeIdentifierArray(key, def, trim);
		}

		// Token: 0x06001D0F RID: 7439 RVA: 0x000D0F0A File Offset: 0x000CF10A
		[return: NotNullIfNotNull("def")]
		public ImmutableHashSet<Identifier> GetAttributeIdentifierImmutableHashSet(string key, [Nullable(2)] ImmutableHashSet<Identifier> def, bool trim = true)
		{
			return this.Element.GetAttributeIdentifierImmutableHashSet(key, def, trim);
		}

		// Token: 0x06001D10 RID: 7440 RVA: 0x000D0F1A File Offset: 0x000CF11A
		[NullableContext(2)]
		[return: NotNullIfNotNull("def")]
		public string GetAttributeString([Nullable(1)] string key, string def)
		{
			return this.Element.GetAttributeString(key, def);
		}

		// Token: 0x06001D11 RID: 7441 RVA: 0x000D0F29 File Offset: 0x000CF129
		public string GetAttributeStringUnrestricted(string key, string def)
		{
			return this.Element.GetAttributeStringUnrestricted(key, def);
		}

		// Token: 0x06001D12 RID: 7442 RVA: 0x000D0F38 File Offset: 0x000CF138
		[return: Nullable(new byte[]
		{
			2,
			1
		})]
		public string[] GetAttributeStringArray(string key, [Nullable(new byte[]
		{
			2,
			1
		})] string[] def, bool convertToLowerInvariant = false)
		{
			return this.Element.GetAttributeStringArray(key, def, convertToLowerInvariant, false);
		}

		// Token: 0x06001D13 RID: 7443 RVA: 0x000D0F49 File Offset: 0x000CF149
		[return: Nullable(2)]
		public ContentPath GetAttributeContentPath(string key)
		{
			return this.Element.GetAttributeContentPath(key, this.ContentPackage);
		}

		// Token: 0x06001D14 RID: 7444 RVA: 0x000D0F5D File Offset: 0x000CF15D
		public int? GetAttributeNullableInt(string key)
		{
			return this.Element.GetAttributeNullableInt(key);
		}

		// Token: 0x06001D15 RID: 7445 RVA: 0x000D0F6B File Offset: 0x000CF16B
		public int GetAttributeInt(string key, int def)
		{
			return this.Element.GetAttributeInt(key, def);
		}

		// Token: 0x06001D16 RID: 7446 RVA: 0x000D0F7A File Offset: 0x000CF17A
		public ushort GetAttributeUInt16(string key, ushort def)
		{
			return this.Element.GetAttributeUInt16(key, def);
		}

		// Token: 0x06001D17 RID: 7447 RVA: 0x000D0F89 File Offset: 0x000CF189
		[NullableContext(2)]
		public int[] GetAttributeIntArray([Nullable(1)] string key, int[] def)
		{
			return this.Element.GetAttributeIntArray(key, def);
		}

		// Token: 0x06001D18 RID: 7448 RVA: 0x000D0F98 File Offset: 0x000CF198
		[NullableContext(2)]
		public ushort[] GetAttributeUshortArray([Nullable(1)] string key, ushort[] def)
		{
			return this.Element.GetAttributeUshortArray(key, def);
		}

		// Token: 0x06001D19 RID: 7449 RVA: 0x000D0FA7 File Offset: 0x000CF1A7
		public float? GetAttributeNullableFloat(string key)
		{
			return this.Element.GetAttributeNullableFloat(key);
		}

		// Token: 0x06001D1A RID: 7450 RVA: 0x000D0FB5 File Offset: 0x000CF1B5
		public float GetAttributeFloat(string key, float def)
		{
			return this.Element.GetAttributeFloat(key, def);
		}

		// Token: 0x06001D1B RID: 7451 RVA: 0x000D0FC4 File Offset: 0x000CF1C4
		[NullableContext(2)]
		public float[] GetAttributeFloatArray([Nullable(1)] string key, float[] def)
		{
			return this.Element.GetAttributeFloatArray(key, def);
		}

		// Token: 0x06001D1C RID: 7452 RVA: 0x000D0FD3 File Offset: 0x000CF1D3
		public float GetAttributeFloat(float def, params string[] keys)
		{
			return this.Element.GetAttributeFloat(def, keys);
		}

		// Token: 0x06001D1D RID: 7453 RVA: 0x000D0FE2 File Offset: 0x000CF1E2
		public bool GetAttributeBool(string key, bool def)
		{
			return this.Element.GetAttributeBool(key, def);
		}

		// Token: 0x06001D1E RID: 7454 RVA: 0x000D0FF1 File Offset: 0x000CF1F1
		public Point GetAttributePoint(string key, in Point def)
		{
			return this.Element.GetAttributePoint(key, def);
		}

		// Token: 0x06001D1F RID: 7455 RVA: 0x000D1005 File Offset: 0x000CF205
		public Vector2 GetAttributeVector2(string key, in Vector2 def)
		{
			return this.Element.GetAttributeVector2(key, def);
		}

		// Token: 0x06001D20 RID: 7456 RVA: 0x000D1019 File Offset: 0x000CF219
		public Vector4 GetAttributeVector4(string key, in Vector4 def)
		{
			return this.Element.GetAttributeVector4(key, def);
		}

		// Token: 0x06001D21 RID: 7457 RVA: 0x000D102D File Offset: 0x000CF22D
		public Color GetAttributeColor(string key, in Color def)
		{
			return this.Element.GetAttributeColor(key, def);
		}

		// Token: 0x06001D22 RID: 7458 RVA: 0x000D1041 File Offset: 0x000CF241
		public Color? GetAttributeColor(string key)
		{
			return this.Element.GetAttributeColor(key);
		}

		// Token: 0x06001D23 RID: 7459 RVA: 0x000D104F File Offset: 0x000CF24F
		[NullableContext(2)]
		public Color[] GetAttributeColorArray([Nullable(1)] string key, Color[] def)
		{
			return this.Element.GetAttributeColorArray(key, def);
		}

		// Token: 0x06001D24 RID: 7460 RVA: 0x000D105E File Offset: 0x000CF25E
		public Rectangle GetAttributeRect(string key, in Rectangle def)
		{
			return this.Element.GetAttributeRect(key, def);
		}

		// Token: 0x06001D25 RID: 7461 RVA: 0x000D1072 File Offset: 0x000CF272
		public Version GetAttributeVersion(string key, Version def)
		{
			return this.Element.GetAttributeVersion(key, def);
		}

		// Token: 0x06001D26 RID: 7462 RVA: 0x000D1081 File Offset: 0x000CF281
		[NullableContext(0)]
		public T GetAttributeEnum<T>([Nullable(1)] string key, in T def) where T : struct, Enum
		{
			return this.Element.GetAttributeEnum(key, def);
		}

		// Token: 0x06001D27 RID: 7463 RVA: 0x000D1095 File Offset: 0x000CF295
		[return: Nullable(new byte[]
		{
			1,
			0
		})]
		public T[] GetAttributeEnumArray<[Nullable(0)] T>(string key, [Nullable(new byte[]
		{
			1,
			0
		})] T[] def) where T : struct, Enum
		{
			return this.Element.GetAttributeEnumArray(key, def);
		}

		// Token: 0x06001D28 RID: 7464 RVA: 0x000D10A4 File Offset: 0x000CF2A4
		[NullableContext(2)]
		[return: Nullable(new byte[]
		{
			0,
			1,
			1
		})]
		public ValueTuple<T1, T2> GetAttributeTuple<T1, T2>([Nullable(1)] string key, [Nullable(new byte[]
		{
			0,
			1,
			1
		})] in ValueTuple<T1, T2> def)
		{
			return this.Element.GetAttributeTuple(key, def);
		}

		// Token: 0x06001D29 RID: 7465 RVA: 0x000D10B8 File Offset: 0x000CF2B8
		[NullableContext(2)]
		[return: Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})]
		public ValueTuple<T1, T2>[] GetAttributeTupleArray<T1, T2>([Nullable(1)] string key, [Nullable(new byte[]
		{
			1,
			0,
			1,
			1
		})] in ValueTuple<T1, T2>[] def)
		{
			return this.Element.GetAttributeTupleArray(key, def);
		}

		// Token: 0x06001D2A RID: 7466 RVA: 0x000D10C8 File Offset: 0x000CF2C8
		[NullableContext(0)]
		public Range<int> GetAttributeRange([Nullable(1)] string key, in Range<int> def)
		{
			return this.Element.GetAttributeRange(key, def);
		}

		// Token: 0x06001D2B RID: 7467 RVA: 0x000D10DC File Offset: 0x000CF2DC
		public Identifier VariantOf()
		{
			return this.Element.VariantOf();
		}

		// Token: 0x06001D2C RID: 7468 RVA: 0x000D10E9 File Offset: 0x000CF2E9
		public bool DoesAttributeReferenceFileNameAlone(string key)
		{
			return this.Element.DoesAttributeReferenceFileNameAlone(key);
		}

		// Token: 0x06001D2D RID: 7469 RVA: 0x000D10F7 File Offset: 0x000CF2F7
		public string ParseContentPathFromUri()
		{
			return this.Element.ParseContentPathFromUri();
		}

		// Token: 0x06001D2E RID: 7470 RVA: 0x000D1104 File Offset: 0x000CF304
		public void SetAttributeValue(string key, string val)
		{
			this.Element.SetAttributeValue(key, val);
		}

		// Token: 0x06001D2F RID: 7471 RVA: 0x000D1118 File Offset: 0x000CF318
		public void Add(ContentXElement elem)
		{
			this.Element.Add(elem.Element);
			elem.ContentPackage = this.ContentPackage;
		}

		// Token: 0x06001D30 RID: 7472 RVA: 0x000D1137 File Offset: 0x000CF337
		public void AddFirst(ContentXElement elem)
		{
			this.Element.AddFirst(elem.Element);
			elem.ContentPackage = this.ContentPackage;
		}

		// Token: 0x06001D31 RID: 7473 RVA: 0x000D1156 File Offset: 0x000CF356
		public void AddAfterSelf(ContentXElement elem)
		{
			this.Element.AddAfterSelf(elem.Element);
			elem.ContentPackage = this.ContentPackage;
		}

		// Token: 0x06001D32 RID: 7474 RVA: 0x000D1175 File Offset: 0x000CF375
		public void Remove()
		{
			this.Element.Remove();
		}

		// Token: 0x06001D33 RID: 7475 RVA: 0x000D1184 File Offset: 0x000CF384
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			ContentXElement element = obj as ContentXElement;
			return element != null && this == element;
		}

		// Token: 0x06001D34 RID: 7476 RVA: 0x000D11A6 File Offset: 0x000CF3A6
		public override int GetHashCode()
		{
			return HashCode.Combine<ContentPackage, XElement>(this.ContentPackage, this.Element);
		}

		// Token: 0x06001D35 RID: 7477 RVA: 0x000D11B9 File Offset: 0x000CF3B9
		[NullableContext(2)]
		public static bool operator ==(in ContentXElement a, in ContentXElement b)
		{
			ContentXElement contentXElement = a;
			ContentPackage contentPackage = (contentXElement != null) ? contentXElement.ContentPackage : null;
			ContentXElement contentXElement2 = b;
			if (contentPackage == ((contentXElement2 != null) ? contentXElement2.ContentPackage : null))
			{
				ContentXElement contentXElement3 = a;
				XElement xelement = (contentXElement3 != null) ? contentXElement3.Element : null;
				ContentXElement contentXElement4 = b;
				return xelement == ((contentXElement4 != null) ? contentXElement4.Element : null);
			}
			return false;
		}

		// Token: 0x06001D36 RID: 7478 RVA: 0x000D11F9 File Offset: 0x000CF3F9
		[NullableContext(2)]
		public static bool operator !=(in ContentXElement a, in ContentXElement b)
		{
			return !(a == b);
		}

		// Token: 0x04000D2C RID: 3372
		public readonly XElement Element;
	}
}
