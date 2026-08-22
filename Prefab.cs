using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace Barotrauma
{
	// Token: 0x02000346 RID: 838
	[NullableContext(1)]
	[Nullable(0)]
	public abstract class Prefab
	{
		// Token: 0x060041E5 RID: 16869 RVA: 0x00247B20 File Offset: 0x00245D20
		public static void DisallowCallFromConstructor()
		{
			if (!Prefab.potentialCallFromConstructor)
			{
				return;
			}
			StackTrace st = new StackTrace(2, false);
			for (int i = st.FrameCount - 1; i >= 0; i--)
			{
				StackFrame frame = st.GetFrame(i);
				MethodBase methodBase = (frame != null) ? frame.GetMethod() : null;
				if (methodBase != null && methodBase.IsConstructor)
				{
					Type declaringType = methodBase.DeclaringType;
					if (declaringType != null && Prefab.Types.Contains(declaringType))
					{
						throw new Exception("Called disallowed method from within a prefab's constructor!");
					}
				}
			}
			Prefab.potentialCallFromConstructor = false;
		}

		// Token: 0x1700117A RID: 4474
		// (get) Token: 0x060041E6 RID: 16870 RVA: 0x00247B97 File Offset: 0x00245D97
		[Nullable(2)]
		public ContentPackage ContentPackage
		{
			[NullableContext(2)]
			get
			{
				ContentFile contentFile = this.ContentFile;
				if (contentFile == null)
				{
					return null;
				}
				return contentFile.ContentPackage;
			}
		}

		// Token: 0x1700117B RID: 4475
		// (get) Token: 0x060041E7 RID: 16871 RVA: 0x00247BAA File Offset: 0x00245DAA
		public ContentPath FilePath
		{
			get
			{
				return this.ContentFile.Path;
			}
		}

		// Token: 0x060041E8 RID: 16872 RVA: 0x00247BB8 File Offset: 0x00245DB8
		public Prefab(ContentFile file, Identifier identifier)
		{
			Prefab.potentialCallFromConstructor = true;
			this.ContentFile = file;
			this.Identifier = identifier;
			if (this.Identifier.IsEmpty)
			{
				throw new ArgumentException("Error creating " + base.GetType().Name + ": Identifier cannot be empty");
			}
		}

		// Token: 0x060041E9 RID: 16873 RVA: 0x00247C0C File Offset: 0x00245E0C
		public Prefab(ContentFile file, ContentXElement element)
		{
			Prefab.potentialCallFromConstructor = true;
			this.ContentFile = file;
			this.Identifier = this.DetermineIdentifier(element);
			if (this.Identifier.IsEmpty)
			{
				throw new ArgumentException("Error creating " + base.GetType().Name + ": Identifier cannot be empty");
			}
		}

		// Token: 0x060041EA RID: 16874 RVA: 0x00247C6B File Offset: 0x00245E6B
		protected virtual Identifier DetermineIdentifier(XElement element)
		{
			return element.GetAttributeIdentifier("identifier", Identifier.Empty);
		}

		// Token: 0x060041EB RID: 16875
		public abstract void Dispose();

		// Token: 0x04002256 RID: 8790
		public static readonly ImmutableHashSet<Type> Types = ReflectionUtils.GetDerivedNonAbstract<Prefab>().ToImmutableHashSet<Type>();

		// Token: 0x04002257 RID: 8791
		private static bool potentialCallFromConstructor;

		// Token: 0x04002258 RID: 8792
		public readonly Identifier Identifier;

		// Token: 0x04002259 RID: 8793
		public readonly ContentFile ContentFile;
	}
}
