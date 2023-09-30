using System;

using UnityEngine;


namespace Game
{
	public class PickupDetection : MonoBehaviour
	{
		[SerializeField] private LayerMask inactiveBulletLayer;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if ((inactiveBulletLayer.value & (1 << collision.transform.gameObject.layer)) > 0)
			{
				collision.GetComponent<BulletInactive>().Dispose();
				GetComponent<Cowboy>().bullet = true;
			}
		}
	}
}