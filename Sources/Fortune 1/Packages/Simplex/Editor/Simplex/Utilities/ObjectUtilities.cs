using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using Object = UnityEngine.Object;

using UnityEditor;
using UnityEditor.SceneManagement;


namespace Simplex.Editor
{
	public static class ObjectUtilities
	{
		private static Dictionary<int, DelegateValue> iValues = new Dictionary<int, DelegateValue>();


		private const BindingFlags defaultFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		private static MemberInfo GetMember(Type type, string member, BindingFlags flags = defaultFlags) => type.GetField(member, flags) as MemberInfo ?? type.GetProperty(member, flags);

		public static IValue IValue(this Object obj, string member, BindingFlags flags = defaultFlags) => IValue(obj, obj, GetMember(obj.GetType(), member, flags));
		public static IValue IValue(this Object obj, MemberInfo member) => IValue(obj, obj, member);
		public static IValue IValue(this object obj, Object origin, string member, BindingFlags flags = defaultFlags) => IValue(obj, origin, GetMember(obj.GetType(), member, flags));
		public static IValue IValue(this object obj, Object origin, MemberInfo member)
		{
			try
			{
				if (obj == null) throw new NullReferenceException("Null object");
				if (origin == null) throw new NullReferenceException("Null origin");
				if (origin.GetType().IsValueType) ConsoleUtilities.Warn($"IValue from structs are not supported");
				if (member == null) throw new NullReferenceException("Null member info");

				int hash = HashCode.Combine(obj, origin, member);
				if (iValues.TryGetValue(hash, out DelegateValue iValue))
					return iValue;

				iValue = (member) switch
				{
					FieldInfo field => new DelegateValue(member.Name, () => field.GetValue(obj), value => origin.Edit(() => field.SetValue(obj, value), $"{field.Name} = {value}")),
					PropertyInfo property => iValue = new DelegateValue(member.Name, () => property.GetValue(obj), value => origin.Edit(() => property.SetValue(obj, value), $"{property.Name} = {value}")),
					_ => throw new ArgumentException("Unexpected member type"),
				};

				iValues.Add(hash, iValue);
				return iValue;
			}
			catch (Exception exception) { exception.Error($"Failed getting IValue {member:ref} in {origin:ref} / {obj:ref}"); }
			return null;
		}

		public static void Edit(this Object obj, Action action, string description)
		{
			try
			{
				Undo.RegisterCompleteObjectUndo(obj, ConsoleUtilities.Format($"{obj:type} {obj.name} : {description}"));
				action.Invoke();

				EditorUtility.SetDirty(obj);
				EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
			}
			catch (Exception exception) { exception.Error($"Failed editing {obj:ref} : {description:info}"); }
		}
	}
}