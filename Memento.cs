using System;
using System.Collections.Generic;
using System.Linq;

namespace Barotrauma
{
	// Token: 0x02000339 RID: 825
	public class Memento<T>
	{
		// Token: 0x17001167 RID: 4455
		// (get) Token: 0x06004154 RID: 16724 RVA: 0x00244FF1 File Offset: 0x002431F1
		// (set) Token: 0x06004155 RID: 16725 RVA: 0x00244FF9 File Offset: 0x002431F9
		public T Current { get; private set; }

		// Token: 0x17001168 RID: 4456
		// (get) Token: 0x06004156 RID: 16726 RVA: 0x00245002 File Offset: 0x00243202
		public int UndoCount
		{
			get
			{
				return this.undoStack.Count;
			}
		}

		// Token: 0x17001169 RID: 4457
		// (get) Token: 0x06004157 RID: 16727 RVA: 0x0024500F File Offset: 0x0024320F
		public int RedoCount
		{
			get
			{
				return this.redoStack.Count;
			}
		}

		// Token: 0x06004158 RID: 16728 RVA: 0x0024501C File Offset: 0x0024321C
		public void Store(T newState)
		{
			this.redoStack.Clear();
			if (this.Current != null)
			{
				T t = this.Current;
				if (!t.Equals(default(T)))
				{
					this.undoStack.Push(this.Current);
				}
			}
			this.Current = newState;
		}

		// Token: 0x06004159 RID: 16729 RVA: 0x0024507D File Offset: 0x0024327D
		public T Undo()
		{
			if (this.undoStack.Any<T>())
			{
				this.redoStack.Push(this.Current);
				this.Current = this.undoStack.Pop();
			}
			return this.Current;
		}

		// Token: 0x0600415A RID: 16730 RVA: 0x002450B4 File Offset: 0x002432B4
		public T Redo()
		{
			if (this.redoStack.Any<T>())
			{
				this.undoStack.Push(this.Current);
				this.Current = this.redoStack.Pop();
			}
			return this.Current;
		}

		// Token: 0x0600415B RID: 16731 RVA: 0x002450EC File Offset: 0x002432EC
		public void Clear()
		{
			this.undoStack.Clear();
			this.redoStack.Clear();
			this.Current = default(T);
		}

		// Token: 0x04002216 RID: 8726
		private Stack<T> undoStack = new Stack<T>();

		// Token: 0x04002217 RID: 8727
		private Stack<T> redoStack = new Stack<T>();
	}
}
