using System;

using UnityEngine;
using UnityEngine.Pool;

using Simplex;


namespace Game
{
	public class BulletSpecial : MonoBehaviour
	{
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.bulletSpecialPrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);

		[SerializeField] private new Rigidbody2D rigidbody;
		[SerializeField] public Vector2 direction;
		[SerializeField] public float speed;
		[SerializeField] public float timer;
		private GameObject origin;


		public static void OnGet(GameObject obj) => obj.SetActive(true);
		public static void OnRelease(GameObject obj) => obj.SetActive(false);
		public static BulletSpecial Spawn(Vector2 position, Quaternion rotation, GameObject origin)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.bulletSpecialRoot;
			obj.transform.position = position;
			obj.transform.rotation = rotation;

			BulletSpecial bullet = obj.GetComponent<BulletSpecial>();
			bullet.direction = new Vector2(Mathf.Cos(obj.transform.eulerAngles.z * Mathf.Deg2Rad), Mathf.Sin(obj.transform.eulerAngles.z * Mathf.Deg2Rad));
			bullet.timer = 0;
			bullet.origin = origin;

			return bullet;
		}
		public void Dispose() => pool.Release(gameObject);

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
			if (collision.gameObject == origin) return;

			try
			{
				if (collision.TryGetComponent<Cowboy>(out Cowboy cowboy))
				{
					cowboy.Die(transform.position);
					if (origin == Monolith.PlayerObject)
					{
						cowboy.ShowCoin();
						Monolith.bounty += 100;
						Monolith.fortune += UnityEngine.Random.Range(0, 200);
						UI.Hud.Instance.UpdateWanted();
						UI.Hud.Instance.UpdateFortune();
					}
				}
			}
			catch (Exception exception) { exception.Error($"Special bullet triggered unexpectedly by {collision.gameObject}"); }
		}

		public static void CleanUp()
		{
			pool.Clear();

			for (int i = Monolith.Refs.bulletSpecialRoot.childCount - 1; i >= 0; i--)
				try { Destroy(Monolith.Refs.bulletSpecialRoot.GetChild(i).gameObject); }
				catch (Exception exception) { exception.Error($"Failed cleaning active bullets"); }
		}
	}
}