using System;
using UnityEngine;

namespace Game
{
	public class HitDetection : MonoBehaviour
	{
		public event Action HitByBullet;
		[SerializeField] private LayerMask activeBulletLayer;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if ((activeBulletLayer.value & (1 << collision.transform.gameObject.layer)) > 0)
			{
				Debug.Log("touched active bullet", collision.gameObject);
				HitByBullet?.Invoke();
			}
		}
	}
}
