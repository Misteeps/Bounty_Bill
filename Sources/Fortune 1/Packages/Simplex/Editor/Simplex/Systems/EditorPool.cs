using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;

using UnityEditor;


namespace Simplex.Editor
{
	public class EditorPool<TObject, TElement> where TElement : class
	{
		public readonly ObjectPool<TElement> basic;
		public readonly Dictionary<Type, ObjectPool<TElement>> types;


		public EditorPool(bool hasBasic = true, int defaultCapacity = 0, int maxCapacity = 2048)
		{
			basic = (hasBasic) ? NewPool(typeof(TElement), defaultCapacity, maxCapacity) : null;
			types = new Dictionary<Type, ObjectPool<TElement>>();

			Type objType = typeof(TObject);
			Type elementType = typeof(TElement);

			foreach (Type type in TypeCache.GetTypesWithAttribute<EditorAttribute>())
				try
				{
					EditorAttribute attribute = type.GetCustomAttribute<EditorAttribute>() ?? throw new Exception("Missing editor attribute");
					if (!attribute.type.IsSubclassOf(objType)) continue;
					if (!type.IsSubclassOf(elementType)) continue;
					types.Add(attribute.type, NewPool(type, defaultCapacity, maxCapacity));
				}
				catch (Exception exception) { exception.Error($"Failed registering {type:type}"); }
		}

		private ObjectPool<TElement> NewPool(Type type, int defaultCapacity, int maxCapacity) => new ObjectPool<TElement>(() => (TElement)Activator.CreateInstance(type), defaultCapacity: defaultCapacity, maxSize: maxCapacity);

		public TElement Get(TObject obj)
		{
			if (obj == null) throw new ArgumentNullException("Cannot get pooled editor for null object");

			if (types.TryGetValue(obj.GetType(), out ObjectPool<TElement> pool))
				return pool.Get();

			if (basic != null)
				return basic.Get();

			throw new Exception($"Editor pool not found for {obj.GetTypeName()}");
		}
		public void Release(TObject obj, TElement element)
		{
			if (obj == null) return;
			if (element == null) return;

			if (types.TryGetValue(obj.GetType(), out ObjectPool<TElement> pool))
			{
				pool.Release(element);
				return;
			}

			if (basic != null)
			{
				basic.Release(element);
				return;
			}

			throw new Exception($"Editor pool not found for {obj.GetTypeName()}");
		}
	}
}