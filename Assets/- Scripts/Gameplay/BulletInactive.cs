using UnityEngine;
using UnityEngine.Pool;


namespace Game
{
	public class BulletInactive : MonoBehaviour
	{
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.bulletInactivePrefab), actionOnGet: obj => obj.SetActive(true), actionOnRelease: obj => obj.SetActive(false));


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