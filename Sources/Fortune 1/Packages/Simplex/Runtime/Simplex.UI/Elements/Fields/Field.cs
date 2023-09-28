using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public abstract class Field : Label
	{
		protected override string[] DefaultClasses => new string[] { "field" };
		protected override Size DefaultSize => Size.Medium;
		protected override bool DefaultFocusable => true;
		protected override bool DefaultNavigable => true;
		protected override bool DefaultPickable => true;
	}


	public abstract class Field<T> : Field, IBindable
	{
		protected override bool RefreshOnAttach => true;

		IValue IBindable.IValue
		{
			get => iValue;
			set
			{
				if (iValue != null)
				{
					iValue.Changed -= OnIValueChanged;
					if (Name == iValue.Name) Name = null;
				}

				iValue = value;

				if (iValue != null)
				{
					iValue.Changed += OnIValueChanged;
					if (Name.Empty()) Name = iValue.Name;
				}
			}
		}
		private IValue iValue;
		private T currentValue;

		public T BindedValue
		{
			get => (iValue == null) ? CurrentValue : (T)iValue.Value;
			set
			{
				if (iValue == null) CurrentValue = value;
				else if (!EqualityComparer<T>.Default.Equals(value, BindedValue))
					iValue.Set(value, this);

				this.Refresh();
			}
		}
		public virtual T CurrentValue
		{
			get => currentValue;
			set
			{
				if (EqualityComparer<T>.Default.Equals(value, currentValue)) return;

				using ChangeEvent evnt1 = ChangeEvent.GetPooled(currentValue, value);
				evnt1.target = this;
				SendEvent(evnt1);

				using ChangeEvent<T> evnt2 = ChangeEvent<T>.GetPooled(currentValue, value);
				evnt2.target = this;
				SendEvent(evnt2);

				currentValue = value;
			}
		}


		public Field()
		{
			RegisterCallback<AttachToPanelEvent>(OnAttach);
			RegisterCallback<DetachFromPanelEvent>(OnDetach);
			RegisterCallback<FocusInEvent>(OnFocusIn);
			RegisterCallback<FocusOutEvent>(OnFocusOut);
			RegisterCallback<RefreshEvent>(OnRefresh);
		}

		protected virtual void OnAttach(AttachToPanelEvent evnt)
		{
			if (iValue == null) return;

			iValue.Changed -= OnIValueChanged;
			iValue.Changed += OnIValueChanged;
		}
		protected virtual void OnDetach(DetachFromPanelEvent evnt)
		{
			if (iValue == null) return;

			iValue.Changed -= OnIValueChanged;
		}
		protected virtual void OnFocusIn(FocusInEvent evnt) => this.Refresh();
		protected virtual void OnFocusOut(FocusOutEvent evnt) => BindedValue = CurrentValue;
		protected virtual void OnRefresh(RefreshEvent evnt) => CurrentValue = BindedValue;
		protected virtual void OnIValueChanged(object source)
		{
			if (source != this)
				this.Refresh();
		}
	}
}