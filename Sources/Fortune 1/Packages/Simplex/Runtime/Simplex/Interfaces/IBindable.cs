using System;

using UnityEngine;


namespace Simplex
{
	public interface IBindable
	{
		public IValue IValue { get; set; }
	}

	public interface IBindable<T>
	{
		public IValue<T> IValue { get; set; }
	}
}