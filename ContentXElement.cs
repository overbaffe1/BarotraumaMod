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
	// Token: 0x02000258 RID: 600
	[NullableContext(1)]
	[Nullable(0)]
	public sealed class ContentXElement
	{
		// Token: 0x17000EAD RID: 3757
		// (get) Token: 0x060037D8 RID: 14296 RVA: 0x00216926 File Offset: 0x00214B26
		// (set) Token: 0x060037D9 RID: 14297 RVA: 0x0021692E File Offset: 0x00214B2E
		[Nullable(2)]
		public ContentPackage ContentPackage { [NullableContext(2)] get; [NullableContext(2)] private set; }

		// Token: 0x060037DA RID: 14298 RVA: 0x00216937 File Offset: 0x00214B37
		public ContentXElement([Nullable(2)] ContentPackage contentPackage, XElement element)
		{
			this.ContentPackage = contentPackage;
			this.Element = element;
		}

		// Token: 0x060037DB RID: 14299 RVA: 0x0021694D File Offset: 0x00214B4D
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

		// Token: 0x17000EAE RID: 3758
		// (get) Token: 0x060037DC RID: 14300 RVA: 0x0021695A File Offset: 0x00214B5A
		public XName Name
		{
			get
			{
				return this.Element.Name;
			}
		}

		// Token: 0x060037DD RID: 14301 RVA: 0x00216967 File Offset: 0x00214B67
		public Identifier NameAsIdentifier()
		{
			return this.Element.NameAsIdentifier();
		}

		// Token: 0x17000EAF RID: 3759
		// (get) Token: 0x060037DE RID: 14302 RVA: 0x00216974 File Offset: 0x00214B74
		public string BaseUri
		{
			get
			{
				return this.Element.BaseUri;
			}
		}

		// Token: 0x17000EB0 RID: 3760
		// (get) Token: 0x060037DF RID: 14303 RVA: 0x00216981 File Offset: 0x00214B81
		[Nullable(2)]
		public XDocument Document
		{
			[NullableContext(2)]
			get
			{
				return this.Element.Document;
			}
		}

		// Token: 0x060037E0 RID: 14304 RVA: 0x0021698E File Offset: 0x00214B8E
		[NullableContext(2)]
		public ContentXElement FirstElement()
		{
			return this.Elements().FirstOrDefault<ContentXElement>();
		}

		// Token: 0x17000EB1 RID: 3761
		// (get) Token: 0x060037E1 RID: 14305 RVA: 0x0021699B File Offset: 0x00214B9B
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

		// Token: 0x17000EB2 RID: 3762
		// (get) Token: 0x060037E2 RID: 14306 RVA: 0x002169C2 File Offset: 0x00214BC2
		public bool HasElements
		{
			get
			{
				return this.Element.HasElements;
			}
		}

		// Token: 0x060037E3 RID: 14307 RVA: 0x002169CF File Offset: 0x00214BCF
		public bool IsOverride()
		{
			return this.Element.IsOverride();
		}

		// Token: 0x060037E4 RID: 14308 RVA: 0x002169DC File Offset: 0x00214BDC
		public bool ComesAfter(ContentXElement other)
		{
			return this.Element.ComesAfter(other.Element);
		}

		// Token: 0x060037E5 RID: 14309 RVA: 0x002169F0 File Offset: 0x00214BF0
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

		// Token: 0x060037E6 RID: 14310 RVA: 0x00216A1C File Offset: 0x00214C1C
		public IEnumerable<ContentXElement> Elements()
		{
			return from e in this.Element.Elements()
			select new ContentXElement(this.ContentPackage, e);
		}

		// Token: 0x060037E7 RID: 14311 RVA: 0x00216A3A File Offset: 0x00214C3A
		public IEnumerable<ContentXElement> ElementsBeforeSelf()
		{
			return from e in this.Element.ElementsBeforeSelf()
			select new ContentXElement(this.ContentPackage, e);
		}

		// Token: 0x060037E8 RID: 14312 RVA: 0x00216A58 File Offset: 0x00214C58
		public IEnumerable<ContentXElement> Descendants()
		{
			return from e in this.Element.Descendants()
			select new ContentXElement(this.ContentPackage, e);
		}

		// Token: 0x060037E9 RID: 14313 RVA: 0x00216A78 File Offset: 0x00214C78
		public IEnumerable<ContentXElement> GetChildElements(string name)
		{
			return from e in this.Elements()
			where string.Equals(name, e.Name.LocalName, StringComparison.OrdinalIgnoreCase)
			select e;
		}

		// Token: 0x060037EA RID: 14314 RVA: 0x00216AA9 File Offset: 0x00214CA9
		[return: Nullable(2)]
		public XAttribute GetAttribute(string name)
		{
			return this.Element.GetAttribute(name, StringComparison.OrdinalIgnoreCase);
		}

		// Token: 0x060037EB RID: 14315 RVA: 0x00216AB8 File Offset: 0x00214CB8
		public IEnumerable<XAttribute> Attributes()
		{
			return this.Element.Attributes();
		}

		// Token: 0x060037EC RID: 14316 RVA: 0x00216AC5 File Offset: 0x00214CC5
		public IEnumerable<XAttribute> Attributes(string name)
		{
			return this.Element.Attributes(name);
		}

		// Token: 0x060037ED RID: 14317 RVA: 0x00216AD8 File Offset: 0x00214CD8
		public string ElementInnerText()
		{
			return this.Element.ElementInnerText();
		}

		// Token: 0x060037EE RID: 14318 RVA: 0x00216AE5 File Offset: 0x00214CE5
		public Identifier GetAttributeIdentifier(string key, string def)
		{
			return this.Element.GetAttributeIdentifier(key, def);
		}

		// Token: 0x060037EF RID: 14319 RVA: 0x00216AF4 File Offset: 0x00214CF4
		public Identifier GetAttributeIdentifier(string key, Identifier def)
		{
			return this.Element.GetAttributeIdentifier(key, def);
		}

		// Token: 0x060037F0 RID: 14320 RVA: 0x00216B03 File Offset: 0x00214D03
		[return: NotNullIfNotNull("def")]
		public Identifier[] GetAttributeIdentifierArray(Identifier[] def, params string[] keys)
		{
			return this.Element.GetAttributeIdentifierArray(def, keys);
		}

		// Token: 0x060037F1 RID: 14321 RVA: 0x00216B12 File Offset: 0x00214D12
		[return: NotNullIfNotNull("def")]
		public Identifier[] GetAttributeIdentifierArray(string key, Identifier[] def, bool trim = true)
		{
			return this.Element.GetAttributeIdentifierArray(key, def, trim);
		}

		// Token: 0x060037F2 RID: 14322 RVA: 0x00216B22 File Offset: 0x00214D22
		[return: NotNullIfNotNull("def")]
		public ImmutableHashSet<Identifier> GetAttributeIdentifierImmutableHashSet(string key, [Nullable(2)] ImmutableHashSet<Identifier> def, bool trim = true)
		{
			return this.Element.GetAttributeIdentifierImmutableHashSet(key, def, trim);
		}

		// Token: 0x060037F3 RID: 14323 RVA: 0x00216B32 File Offset: 0x00214D32
		[NullableContext(2)]
		[return: NotNullIfNotNull("def")]
		public string GetAttributeString([Nullable(1)] string key, string def)
		{
			return this.Element.GetAttributeString(key, def);
		}

		// Token: 0x060037F4 RID: 14324 RVA: 0x00216B41 File Offset: 0x00214D41
		public string GetAttributeStringUnrestricted(string key, string def)
		{
			return this.Element.GetAttributeStringUnrestricted(key, def);
		}

		// Token: 0x060037F5 RID: 14325 RVA: 0x00216B50 File Offset: 0x00214D50
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

		// Token: 0x060037F6 RID: 14326 RVA: 0x00216B61 File Offset: 0x00214D61
		[return: Nullable(2)]
		public ContentPath GetAttributeContentPath(string key)
		{
			return this.Element.GetAttributeContentPath(key, this.ContentPackage);
		}

		// Token: 0x060037F7 RID: 14327 RVA: 0x00216B75 File Offset: 0x00214D75
		public int? GetAttributeNullableInt(string key)
		{
			return this.Element.GetAttributeNullableInt(key);
		}

		// Token: 0x060037F8 RID: 14328 RVA: 0x00216B83 File Offset: 0x00214D83
		public int GetAttributeInt(string key, int def)
		{
			return this.Element.GetAttributeInt(key, def);
		}

		// Token: 0x060037F9 RID: 14329 RVA: 0x00216B92 File Offset: 0x00214D92
		public ushort GetAttributeUInt16(string key, ushort def)
		{
			return this.Element.GetAttributeUInt16(key, def);
		}

		// Token: 0x060037FA RID: 14330 RVA: 0x00216BA1 File Offset: 0x00214DA1
		[NullableContext(2)]
		public int[] GetAttributeIntArray([Nullable(1)] string key, int[] def)
		{
			return this.Element.GetAttributeIntArray(key, def);
		}

		// Token: 0x060037FB RID: 14331 RVA: 0x00216BB0 File Offset: 0x00214DB0
		[NullableContext(2)]
		public ushort[] GetAttributeUshortArray([Nullable(1)] string key, ushort[] def)
		{
			return this.Element.GetAttributeUshortArray(key, def);
		}

		// Token: 0x060037FC RID: 14332 RVA: 0x00216BBF File Offset: 0x00214DBF
		public float? GetAttributeNullableFloat(string key)
		{
			return this.Element.GetAttributeNullableFloat(key);
		}

		// Token: 0x060037FD RID: 14333 RVA: 0x00216BCD File Offset: 0x00214DCD
		public float GetAttributeFloat(string key, float def)
		{
			return this.Element.GetAttributeFloat(key, def);
		}

		// Token: 0x060037FE RID: 14334 RVA: 0x00216BDC File Offset: 0x00214DDC
		[NullableContext(2)]
		public float[] GetAttributeFloatArray([Nullable(1)] string key, float[] def)
		{
			return this.Element.GetAttributeFloatArray(key, def);
		}

		// Token: 0x060037FF RID: 14335 RVA: 0x00216BEB File Offset: 0x00214DEB
		public float GetAttributeFloat(float def, params string[] keys)
		{
			return this.Element.GetAttributeFloat(def, keys);
		}

		// Token: 0x06003800 RID: 14336 RVA: 0x00216BFA File Offset: 0x00214DFA
		public bool GetAttributeBool(string key, bool def)
		{
			return this.Element.GetAttributeBool(key, def);
		}

		// Token: 0x06003801 RID: 14337 RVA: 0x00216C09 File Offset: 0x00214E09
		public Point GetAttributePoint(string key, in Point def)
		{
			return this.Element.GetAttributePoint(key, def);
		}

		// Token: 0x06003802 RID: 14338 RVA: 0x00216C1D File Offset: 0x00214E1D
		public Vector2 GetAttributeVector2(string key, in Vector2 def)
		{
			return this.Element.GetAttributeVector2(key, def);
		}

		// Token: 0x06003803 RID: 14339 RVA: 0x00216C31 File Offset: 0x00214E31
		public Vector4 GetAttributeVector4(string key, in Vector4 def)
		{
			return this.Element.GetAttributeVector4(key, def);
		}

		// Token: 0x06003804 RID: 14340 RVA: 0x00216C45 File Offset: 0x00214E45
		public Color GetAttributeColor(string key, in Color def)
		{
			return this.Element.GetAttributeColor(key, def);
		}

		// Token: 0x06003805 RID: 14341 RVA: 0x00216C59 File Offset: 0x00214E59
		public Color? GetAttributeColor(string key)
		{
			return this.Element.GetAttributeColor(key);
		}

		// Token: 0x06003806 RID: 14342 RVA: 0x00216C67 File Offset: 0x00214E67
		[NullableContext(2)]
		public Color[] GetAttributeColorArray([Nullable(1)] string key, Color[] def)
		{
			return this.Element.GetAttributeColorArray(key, def);
		}

		// Token: 0x06003807 RID: 14343 RVA: 0x00216C76 File Offset: 0x00214E76
		public Rectangle GetAttributeRect(string key, in Rectangle def)
		{
			return this.Element.GetAttributeRect(key, def);
		}

		// Token: 0x06003808 RID: 14344 RVA: 0x00216C8A File Offset: 0x00214E8A
		public Version GetAttributeVersion(string key, Version def)
		{
			return this.Element.GetAttributeVersion(key, def);
		}

		// Token: 0x06003809 RID: 14345 RVA: 0x00216C99 File Offset: 0x00214E99
		[NullableContext(0)]
		public T GetAttributeEnum<T>([Nullable(1)] string key, in T def) where T : struct, Enum
		{
			return this.Element.GetAttributeEnum(key, def);
		}

		// Token: 0x0600380A RID: 14346 RVA: 0x00216CAD File Offset: 0x00214EAD
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

		// Token: 0x0600380B RID: 14347 RVA: 0x00216CBC File Offset: 0x00214EBC
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

		// Token: 0x0600380C RID: 14348 RVA: 0x00216CD0 File Offset: 0x00214ED0
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

		// Token: 0x0600380D RID: 14349 RVA: 0x00216CE0 File Offset: 0x00214EE0
		[NullableContext(0)]
		public Range<int> GetAttributeRange([Nullable(1)] string key, in Range<int> def)
		{
			return this.Element.GetAttributeRange(key, def);
		}

		// Token: 0x0600380E RID: 14350 RVA: 0x00216CF4 File Offset: 0x00214EF4
		public Identifier VariantOf()
		{
			return this.Element.VariantOf();
		}

		// Token: 0x0600380F RID: 14351 RVA: 0x00216D01 File Offset: 0x00214F01
		public bool DoesAttributeReferenceFileNameAlone(string key)
		{
			return this.Element.DoesAttributeReferenceFileNameAlone(key);
		}

		// Token: 0x06003810 RID: 14352 RVA: 0x00216D0F File Offset: 0x00214F0F
		public string ParseContentPathFromUri()
		{
			return this.Element.ParseContentPathFromUri();
		}

		// Token: 0x06003811 RID: 14353 RVA: 0x00216D1C File Offset: 0x00214F1C
		public void SetAttributeValue(string key, string val)
		{
			this.Element.SetAttributeValue(key, val);
		}

		// Token: 0x06003812 RID: 14354 RVA: 0x00216D30 File Offset: 0x00214F30
		public void Add(ContentXElement elem)
		{
			this.Element.Add(elem.Element);
			elem.ContentPackage = this.ContentPackage;
		}

		// Token: 0x06003813 RID: 14355 RVA: 0x00216D4F File Offset: 0x00214F4F
		public void AddFirst(ContentXElement elem)
		{
			this.Element.AddFirst(elem.Element);
			elem.ContentPackage = this.ContentPackage;
		}

		// Token: 0x06003814 RID: 14356 RVA: 0x00216D6E File Offset: 0x00214F6E
		public void AddAfterSelf(ContentXElement elem)
		{
			this.Element.AddAfterSelf(elem.Element);
			elem.ContentPackage = this.ContentPackage;
		}

		// Token: 0x06003815 RID: 14357 RVA: 0x00216D8D File Offset: 0x00214F8D
		public void Remove()
		{
			this.Element.Remove();
		}

		// Token: 0x06003816 RID: 14358 RVA: 0x00216D9C File Offset: 0x00214F9C
		[NullableContext(2)]
		public override bool Equals(object obj)
		{
			ContentXElement element = obj as ContentXElement;
			return element != null && this == element;
		}

		// Token: 0x06003817 RID: 14359 RVA: 0x00216DBE File Offset: 0x00214FBE
		public override int GetHashCode()
		{
			return HashCode.Combine<ContentPackage, XElement>(this.ContentPackage, this.Element);
		}

		// Token: 0x06003818 RID: 14360 RVA: 0x00216DD1 File Offset: 0x00214FD1
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

		// Token: 0x06003819 RID: 14361 RVA: 0x00216E11 File Offset: 0x00215011
		[NullableContext(2)]
		public static bool operator !=(in ContentXElement a, in ContentXElement b)
		{
			return !(a == b);
		}

		// Token: 0x04001C31 RID: 7217
		public readonly XElement Element;
	}
}
