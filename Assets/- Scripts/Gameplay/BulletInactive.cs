using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;

using Simplex;


namespace Game
{
	public class BulletInactive : MonoBehaviour
	{
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.bulletInactivePrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);

		[SerializeField] private CircleCollider2D hitbox;
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
			bullet.triggered = false;
			bullet.Bounce((Vector2)obj.transform.position + direction);

			return bullet;
		}
		public void Dispose() => pool.Release(gameObject);

		public async void Bounce(Vector2 position)
		{
			transform.TransitionPositionX().Modify(position.x, 1, EaseFunction.Quadratic, EaseDirection.Out).Run();
			transform.TransitionPositionY().Modify(position.y, 1, EaseFunction.Quadratic, EaseDirection.Out).Run();

			transform.TransitionLocalScaleX().Modify(1, 1.4f, 0.4f, EaseFunction.Back, EaseDirection.Out).Run();
			await transform.TransitionLocalScaleY().Modify(1, 1.4f, 0.4f, EaseFunction.Back, EaseDirection.Out).Await();

			transform.TransitionLocalScaleX().Modify(1.5f, 1, 0.4f, EaseFunction.Bounce, EaseDirection.Out).Run();
			await transform.TransitionLocalScaleY().Modify(1.5f, 1, 0.4f, EaseFunction.Bounce, EaseDirection.Out).Await();

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

				if (cowboy.bullet)
				{
					triggered = false;
					return;
				}

				cowboy.bullet = true;

				if (Enemies.Reloading.ContainsKey(cowboy))
				{
					Enemies.Reloading.Remove(cowboy);
					Enemies.Hunting.Add(cowboy);
				}
			}
			catch (Exception exception) { exception.Error($"Inactive bullet triggered unexpectedly by {collision.gameObject}"); }

			Dispose();
		}
	}
}