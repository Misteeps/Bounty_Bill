using UnityEngine;
using UnityEngine.Pool;


namespace Game
{
	public class BulletActive : MonoBehaviour
	{
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.bulletActivePrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);

		[SerializeField] private new Rigidbody2D rigidbody;
		[SerializeField] public Vector2 direction;
		[SerializeField] public float speed;
		[SerializeField] public float timer;
		[SerializeField] private LayerMask cowboyLayer;
		[SerializeField] private LayerMask obsticleLayer;
		private GameObject origin;
		private bool hit;


		public static void OnGet(GameObject obj) => obj.SetActive(true);
		public static void OnRelease(GameObject obj) => obj.SetActive(false);
		public static BulletActive Spawn(Vector2 position, Quaternion rotation, GameObject origin)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.bulletActiveRoot;
			obj.transform.position = position;
			obj.transform.rotation = rotation;

			BulletActive bullet = obj.GetComponent<BulletActive>();
			bullet.direction = new Vector2(Mathf.Cos(obj.transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(obj.transform.eulerAngles.z * Mathf.Deg2Rad));
			bullet.timer = 0;
			bullet.origin = origin;
			bullet.hit = false;

			return bullet;
		}
		public void Dispose()
		{
			pool.Release(gameObject);
			BulletInactive.Spawn(transform.position, rigidbody.velocity);
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

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.gameObject == origin || hit) return;
			hit = true;

			if ((cowboyLayer.value & (1 << collision.gameObject.layer)) > 0)
				collision.GetComponent<Cowboy>().Die();

			Dispose();
		}
	}
}