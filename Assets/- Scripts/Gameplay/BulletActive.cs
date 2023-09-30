using UnityEngine;
using UnityEngine.Pool;


namespace Game
{
	public class BulletActive : MonoBehaviour
	{
		private static ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.bulletActivePrefab), actionOnGet: obj => obj.SetActive(true), actionOnRelease: obj => obj.SetActive(false));

		[SerializeField] private new Rigidbody2D rigidbody;
		[SerializeField] public Vector2 direction;
		[SerializeField] public float speed;
		[SerializeField] public float timer;


		public static BulletActive Spawn(Vector2 position, Quaternion rotation)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.bulletActiveRoot;
			obj.transform.position = position;
			obj.transform.rotation = rotation;

			BulletActive bullet = obj.GetComponent<BulletActive>();
			bullet.direction = new Vector2(Mathf.Cos(obj.transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(obj.transform.eulerAngles.z * Mathf.Deg2Rad));
			bullet.timer = 0;

			return bullet;
		}
		public void Dispose()
		{
			pool.Release(gameObject);
			// Instantiate Inactive Bullet
		}

		private void Update()
		{
			timer += Time.deltaTime;
			if (timer > 5) Dispose();
		}
		private void FixedUpdate()
		{
			rigidbody.velocity = direction * speed;
		}

		private void OnCollisionEnter2D(Collision2D collision) => Dispose();
	}
}