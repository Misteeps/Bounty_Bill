using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;


namespace Game
{
	public class BulletInactive : MonoBehaviour
	{
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.bulletInactivePrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);

		[SerializeField] private CircleCollider2D hitbox;


		public static void OnGet(GameObject obj)
		{
			obj.SetActive(true);
			Enemies.inactiveBullets.Add(obj.GetComponent<BulletInactive>());
		}
		public static void OnRelease(GameObject obj)
		{
			obj.SetActive(false);
			Enemies.inactiveBullets.Remove(obj.GetComponent<BulletInactive>());
		}
		public static BulletInactive Spawn(Vector2 position, Vector2 velocity)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.bulletActiveRoot;
			obj.transform.position = position;

			BulletInactive bullet = obj.GetComponent<BulletInactive>();
			bullet.hitbox.enabled = false;

			return bullet;
		}
		public void Dispose() => pool.Release(gameObject);
	}
}