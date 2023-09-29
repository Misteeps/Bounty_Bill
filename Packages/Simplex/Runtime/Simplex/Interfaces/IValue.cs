using System;
using System.Reflection;

using UnityEngine;


namespace Simplex
{
	public interface IValue
	{
		public string Name { get; }
		public object Value { get; set; }

		public event Action<object> Changed;

		public void Set(object value);
		public void Set(object value, object source);
	}

	public interface IValue<T> : IValue
	{
		object IValue.Value { get => Value; set => Value = (T)value; }
		public new T Value { get; set; }

		void IValue.Set(object value) => Set((T)value);
		void IValue.Set(object value, object source) => Set((T)value, source);

		public void Set(T value);
		public void Set(T value, object source);
	}


	#region Variable Value
	public class VariableValue : IValue
	{
		protected object value;

		public string Name { get; set; }
		public object Value
		{
			get => value;
			set => Set(value, null);
		}

		public event Action<object> Changed;


		public VariableValue(object value) : this(null, value) { }
		public VariableValue(string name, object value)
		{
			this.value = value;
			Name = name;
		}

		public virtual void Set(object value) => this.value = value;
		public virtual void Set(object value, object source)
		{
			Set(value);
			Changed?.Invoke(source);
		}
	}
	#endregion Variable Value
	#region Variable Value <T>
	public class VariableValue<T> : IValue<T>
	{
		protected T value;

		public string Name { get; set; }
		public T Value
		{
			get => value;
			set => Set(value, null);
		}

		public event Action<object> Changed;


		public VariableValue(T value) : this(null, value) { }
		public VariableValue(string name, T value)
		{
			this.value = value;
			Name = name;
		}

		public virtual void Set(T value) => this.value = value;
		public virtual void Set(T value, object source)
		{
			Set(value);
			Changed?.Invoke(source);
		}
	}
	#endregion Variable Value <T>

	#region Delegate Value
	public class DelegateValue : IValue
	{
		public Func<object> getValue;
		public Action<object> setValue;

		public string Name { get; set; }
		public object Value
		{
			get => getValue?.Invoke() ?? default;
			set => Set(value, null);
		}

		public event Action<object> Changed;


		public DelegateValue(Func<object> getValue) : this(null, getValue, null) { }
		public DelegateValue(Func<object> getValue, Action<object> setValue) : this(null, getValue, setValue) { }
		public DelegateValue(string name, Func<object> getValue, Action<object> setValue)
		{
			this.getValue = getValue;
			this.setValue = setValue;
			Name = name;
		}

		public virtual void Set(object value) => setValue?.Invoke(value);
		public virtual void Set(object value, object source)
		{
			Set(value);
			Changed?.Invoke(source);
		}
	}
	#endregion Delegate Value
	#region Delegate Value <T>
	public class DelegateValue<T> : IValue<T>
	{
		public Func<T> getValue;
		public Action<T> setValue;

		public string Name { get; set; }
		public T Value
		{
			get => (getValue == null) ? default : getValue.Invoke();
			set => Set(value, null);
		}

		public event Action<object> Changed;


		public DelegateValue(Func<T> getValue) : this(null, getValue, null) { }
		public DelegateValue(Func<T> getValue, Action<T> setValue) : this(null, getValue, setValue) { }
		public DelegateValue(string name, Func<T> getValue, Action<T> setValue)
		{
			this.getValue = getValue;
			this.setValue = setValue;
			Name = name;
		}

		public virtual void Set(T value) => setValue?.Invoke(value);
		public virtual void Set(T value, object source)
		{
			Set(value);
			Changed?.Invoke(source);
		}
	}
	#endregion Delegate Value <T>

	#region Field Value
	public class FieldValue : IValue
	{
		public object container;
		public FieldInfo field;

		public string Name => field.Name;
		public object Value
		{
			get => field?.GetValue(container) ?? default;
			set => Set(value, null);
		}

		public event Action<object> Changed;


		public FieldValue(FieldInfo field) : this(null, field) { }
		public FieldValue(Type type, string field, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(null, type.GetField(field, flags)) { }
		public FieldValue(object container, string field, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(container, container.GetType().GetField(field, flags)) { }
		public FieldValue(object container, FieldInfo field)
		{
			this.container = container;
			this.field = field;
		}

		public virtual void Set(object value) => field?.SetValue(container, value);
		public virtual void Set(object value, object source)
		{
			Set(value);
			Changed?.Invoke(source);
		}
	}
	#endregion Field Value
	#region Field Value <T>
	public class FieldValue<T> : IValue<T>
	{
		public object container;
		public FieldInfo field;

		public string Name => field.Name;
		public T Value
		{
			get => (T)field?.GetValue(container) ?? default;
			set => Set(value, null);
		}

		public event Action<object> Changed;

		public FieldValue(FieldInfo field) : this(null, field) { }
		public FieldValue(Type type, string field, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(null, type.GetField(field, flags)) { }
		public FieldValue(object container, string field, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(container, container.GetType().GetField(field, flags)) { }
		public FieldValue(object container, FieldInfo field)
		{
			this.container = container;
			this.field = field;
		}

		public virtual void Set(T value) => field?.SetValue(container, value);
		public virtual void Set(T value, object source)
		{
			Set(value);
			Changed?.Invoke(source);
		}
	}
	#endregion Field Value

	#region Property Value
	public class PropertyValue : IValue
	{
		public object container;
		public PropertyInfo property;

		public string Name => property.Name;
		public object Value
		{
			get => property?.GetValue(container) ?? default;
			set => Set(value, null);
		}

		public event Action<object> Changed;


		public PropertyValue(PropertyInfo property) : this(null, property) { }
		public PropertyValue(Type type, string property, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(null, type.GetProperty(property, flags)) { }
		public PropertyValue(object container, string property, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(container, container.GetType().GetProperty(property, flags)) { }
		public PropertyValue(object container, PropertyInfo property)
		{
			this.container = container;
			this.property = property;
		}

		public virtual void Set(object value) => property?.SetValue(container, value);
		public virtual void Set(object value, object source)
		{
			Set(value);
			Changed?.Invoke(source);
		}
	}
	#endregion Property Value
	#region Property Value <T>
	public class PropertyValue<T> : IValue<T>
	{
		public object container;
		public PropertyInfo property;

		public string Name => property.Name;
		public T Value
		{
			get => (T)property?.GetValue(container) ?? default;
			set => Set(value, null);
		}

		public event Action<object> Changed;


		public PropertyValue(PropertyInfo property) : this(null, property) { }
		public PropertyValue(Type type, string property, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(null, type.GetProperty(property, flags)) { }
		public PropertyValue(object container, string property, BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) : this(container, container.GetType().GetProperty(property, flags)) { }
		public PropertyValue(object container, PropertyInfo property)
		{
			this.container = container;
			this.property = property;
		}

		public virtual void Set(T value) => property?.SetValue(container, value);
		public virtual void Set(T value, object source)
		{
			Set(value);
			Changed?.Invoke(source);
		}
	}
	#endregion Property Value
}