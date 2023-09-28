using System;

using UnityEngine;
using UnityEngine.UIElements;


namespace Simplex.UI
{
	public class ChangeEvent : EventBase<ChangeEvent>, IChangeEvent
	{
		public object previousValue { get; protected set; }
		public object newValue { get; protected set; }


		public static ChangeEvent GetPooled(object previousValue, object newValue)
		{
			ChangeEvent changeEvent = GetPooled();
			changeEvent.previousValue = previousValue;
			changeEvent.newValue = newValue;
			return changeEvent;
		}
	}
}