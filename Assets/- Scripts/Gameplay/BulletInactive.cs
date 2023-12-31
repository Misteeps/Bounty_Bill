using System;

using UnityEngine;
using UnityEngine.Pool;

using Simplex;


namespace Game
{
	public class BulletInactive : MonoBehaviour
	{
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.bulletInactivePrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);

		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private CircleCollider2D hitbox;
		private float timer;
		private bool triggered;


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
		public static BulletInactive Spawn(Vector2 position, Vector2 direction)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.bulletInactiveRoot;
			obj.transform.position = position;
			obj.transform.localScale = Vector3.one;

			BulletInactive bullet = obj.GetComponent<BulletInactive>();
			bullet.hitbox.enabled = false;
			bullet.timer = 0;
			bullet.triggered = false;
			bullet.spriteRenderer.enabled = true;
			bullet.Bounce((Vector2)obj.transform.position + direction);

			return bullet;
		}
		public void Dispose() => pool.Release(gameObject);

		private void Update()
		{
			timer += Time.deltaTime;
			if (timer < 10) return;
			if (timer > 14) { Dispose(); return; }

			float decimals = timer % 1f;
			if (decimals < 0.5f) spriteRenderer.enabled = false;
			else spriteRenderer.enabled = true;
		}

		public async void Bounce(Vector2 position)
		{
			transform.TransitionPositionX().Modify(position.x, 1, EaseFunction.Quadratic, EaseDirection.Out).Run();
			transform.TransitionPositionY().Modify(position.y, 1, EaseFunction.Quadratic, EaseDirection.Out).Run();

			transform.TransitionLocalScaleX().Modify(1, 1.4f, 0.3f, EaseFunction.Back, EaseDirection.Out).Run();
			await transform.TransitionLocalScaleY().Modify(1, 1.4f, 0.3f, EaseFunction.Back, EaseDirection.Out).Await();

			transform.TransitionLocalScaleX().Modify(1.4f, 1, 0.5f, EaseFunction.Bounce, EaseDirection.Out).Run();
			await transform.TransitionLocalScaleY().Modify(1.4f, 1, 0.5f, EaseFunction.Bounce, EaseDirection.Out).Await();

			hitbox.enabled = true;
		}

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (triggered) return;
			triggered = true;

			try
			{
				if (!collision.TryGetComponent<Cowboy>(out Cowboy cowboy))
					throw new Exception("Collision is not a cowboy");

				if (cowboy.HasBullet)
				{
					triggered = false;
					return;
				}

				if (collision.gameObject == Monolith.PlayerObject)
				{
					cowboy.HasBullet = true;
					cowboy.AudioSource.PlayOneShot(Monolith.Refs.reload, 0.6f);
					UI.Hud.Instance.SetBullet(true);
				}
				else if (!cowboy.HasBullet)
				{
					Enemies.ReloadEnemy(cowboy);
				}
			}
			catch (Exception exception) { exception.Error($"Inactive bullet triggered unexpectedly by {collision.gameObject}"); }

			Dispose();
		}

		public static void CleanUp()
		{
			pool.Clear();

			for (int i = Monolith.Refs.bulletInactiveRoot.childCount - 1; i >= 0; i--)
				try { Destroy(Monolith.Refs.bulletInactiveRoot.GetChild(i).gameObject); }
				catch (Exception exception) { exception.Error($"Failed cleaning inactive bullets"); }
		}
	}
}