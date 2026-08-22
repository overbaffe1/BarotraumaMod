using System;
using System.Collections.Generic;

namespace Barotrauma
{
	// Token: 0x0200028D RID: 653
	internal class ConditionalSprite
	{
		// Token: 0x17000D72 RID: 3442
		// (get) Token: 0x06002DD9 RID: 11737 RVA: 0x0012FEB1 File Offset: 0x0012E0B1
		// (set) Token: 0x06002DDA RID: 11738 RVA: 0x0012FEB9 File Offset: 0x0012E0B9
		public bool IsActive { get; private set; } = true;

		// Token: 0x17000D73 RID: 3443
		// (get) Token: 0x06002DDB RID: 11739 RVA: 0x0012FEC2 File Offset: 0x0012E0C2
		// (set) Token: 0x06002DDC RID: 11740 RVA: 0x0012FECA File Offset: 0x0012E0CA
		public ISerializableEntity Target { get; private set; }

		// Token: 0x17000D74 RID: 3444
		// (get) Token: 0x06002DDD RID: 11741 RVA: 0x0012FED3 File Offset: 0x0012E0D3
		// (set) Token: 0x06002DDE RID: 11742 RVA: 0x0012FEDB File Offset: 0x0012E0DB
		public Sprite Sprite { get; private set; }

		// Token: 0x17000D75 RID: 3445
		// (get) Token: 0x06002DDF RID: 11743 RVA: 0x0012FEE4 File Offset: 0x0012E0E4
		// (set) Token: 0x06002DE0 RID: 11744 RVA: 0x0012FEEC File Offset: 0x0012E0EC
		public DeformableSprite DeformableSprite { get; private set; }

		// Token: 0x17000D76 RID: 3446
		// (get) Token: 0x06002DE1 RID: 11745 RVA: 0x0012FEF5 File Offset: 0x0012E0F5
		public Sprite ActiveSprite
		{
			get
			{
				return this.Sprite ?? this.DeformableSprite.Sprite;
			}
		}

		// Token: 0x06002DE2 RID: 11746 RVA: 0x0012FF0C File Offset: 0x0012E10C
		public ConditionalSprite(ContentXElement element, ISerializableEntity target, string file = "", bool lazyLoad = false, float sourceRectScale = 1f)
		{
			this.Target = target;
			this.Exclusive = element.GetAttributeBool("exclusive", this.Exclusive);
			this.LogicalOperator = element.GetAttributeEnum<PropertyConditional.LogicalOperatorType>("comparison", this.LogicalOperator);
			foreach (ContentXElement subElement in element.Elements())
			{
				string a = subElement.Name.ToString().ToLowerInvariant();
				if (!(a == "conditional"))
				{
					if (!(a == "sprite"))
					{
						if (a == "deformablesprite")
						{
							this.DeformableSprite = new DeformableSprite(subElement, null, null, file, lazyLoad, false, sourceRectScale);
						}
					}
					else
					{
						this.Sprite = new Sprite(subElement, "", file, lazyLoad, sourceRectScale);
					}
				}
				else
				{
					this.conditionals.AddRange(PropertyConditional.FromXElement(subElement, null));
				}
			}
		}

		// Token: 0x06002DE3 RID: 11747 RVA: 0x0013003C File Offset: 0x0012E23C
		public void CheckConditionals()
		{
			this.IsActive = (this.Target != null && PropertyConditional.CheckConditionals(this.Target, this.conditionals, this.LogicalOperator));
		}

		// Token: 0x0400166F RID: 5743
		public readonly List<PropertyConditional> conditionals = new List<PropertyConditional>();

		// Token: 0x04001671 RID: 5745
		public readonly PropertyConditional.LogicalOperatorType LogicalOperator;

		// Token: 0x04001672 RID: 5746
		public readonly bool Exclusive;
	}
}
