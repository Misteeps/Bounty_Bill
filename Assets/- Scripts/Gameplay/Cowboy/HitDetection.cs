using System;

using UnityEngine;


namespace Game
{
	public class HitDetection : MonoBehaviour
	{
		[SerializeField] private LayerMask activeBulletLayer;
		public event Action HitByBullet;

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if ((activeBulletLayer.value & (1 << collision.transform.gameObject.layer)) > 0)
			{
				collision.GetComponent<BulletActive>().Dispose();
				HitByBullet?.Invoke();
			}
		}
	}
}