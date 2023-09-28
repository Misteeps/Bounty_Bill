using System;

using UnityEngine;

using UnityEditor;


namespace Simplex.Editor
{
	public class EditorAttribute : Attribute
	{
		public Type type;


		public EditorAttribute(Type type)
		{
			this.type = type;
		}
	}
}