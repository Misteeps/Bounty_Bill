using System;

using UnityEngine;


namespace Game
{
	public class PickupDetection : MonoBehaviour
	{
		[SerializeField] private LayerMask inactiveBulletLayer;
		public event Action TouchedAmmo;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if ((inactiveBulletLayer.value & (1 << collision.transform.gameObject.layer)) > 0)
				TouchedAmmo?.Invoke();
		}
	}
}