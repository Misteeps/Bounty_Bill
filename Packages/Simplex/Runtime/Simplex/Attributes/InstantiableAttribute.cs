using System;

using UnityEngine;


namespace Simplex
{
	public class InstantiableAttribute : Attribute
	{
		public string path;
		public int order;


		public InstantiableAttribute(string path, int order = 500)
		{
			this.path = path;
			this.order = order;
		}
	}
}