using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;


namespace Game
{
	public class BulletInactive : MonoBehaviour
	{
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.bulletInactivePrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);


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
		public static BulletInactive Spawn(Vector2 position, Quaternion rotation)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.bulletActiveRoot;
			obj.transform.position = position;
			obj.transform.rotation = rotation;

			BulletInactive bullet = obj.GetComponent<BulletInactive>();
			return bullet;
		}
		public void Dispose() => pool.Release(gameObject);
	}
}