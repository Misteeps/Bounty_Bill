using System;

using UnityEngine;
using UnityEngine.AI;

using Simplex;


namespace Game
{
	public class Cowboy : MonoBehaviour
	{
		#region Sprites
		[Serializable]
		public class Sprites
		{
			public Sprite normal;
			public Sprite hit;
			public Sprite dead;
		}
		#endregion Sprites


		public SpriteRenderer SpriteRenderer;
		public BoxCollider2D HitTrigger;
		public CapsuleCollider2D MoveCollider;
		public Rigidbody2D RigidBody;
		public NavMeshAgent Agent;
		public Transform Gun;
		public Transform GunTip;
		public SpriteRenderer GunTipRenderer;
		public SpriteRenderer CoinRenderer;
		public SpriteAnimation CoinAnimater;
		public LineRenderer LineRenderer;
		public bool HasBullet = true;
		public bool IsShooting = false;
		public float MoveSpeed = 1f;

		private float wiggle = 0;
		private bool wiggleDirection = false;
		private Sprites sprites;

		public event Action Died;
		public event Action Disposed;


		public void Initialize()
		{
			SpriteRenderer.sprite = sprites.normal;
			HitTrigger.enabled = true;
			MoveCollider.enabled = true;
			if (Agent) Agent.enabled = true;
			Gun.gameObject.SetActive(true);
			CoinRenderer.gameObject.SetActive(false);
			HasBullet = true;
			IsShooting = false;
		}

		public void Move(Vector2 movement) => RigidBody.MovePosition(RigidBody.position + movement * MoveSpeed);
		public void LookAt(Vector2 target)
		{
			Vector2 direction;
			if (target.x < transform.position.x)
			{
				transform.localScale = new Vector3(1, 1, 1);
				direction = (Vector2)transform.position - target;
			}
			else
			{
				transform.localScale = new Vector3(-1, 1, 1);
				direction = target - (Vector2)transform.position;
			}

			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			Gun.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
		}
		public async void Shoot(float delay)
		{
			if (HasBullet)
			{
				IsShooting = true;

				if (delay != 0)
				{
					DrawLaser(delay);
					await Awaitable.WaitForSecondsAsync(delay);
					EraseLaser();

					if (!Gun.gameObject.activeSelf)
						return;
				}

				Quaternion rotation = (transform.localScale.x < 0) ? Gun.transform.rotation : Quaternion.Euler(new Vector3(0, 0, Gun.transform.eulerAngles.z + 180));
				BulletActive.Spawn(GunTip.position, rotation, gameObject);
				MuzzleFlash();

				await Awaitable.WaitForSecondsAsync(0.2f);
				HasBullet = false;
				IsShooting = false;
			}
			else
			{
				// No bitches?
			}
		}

		private async void DrawLaser(float duration)
		{
			if (!LineRenderer) return;

			LineRenderer.SetPosition(0, new Vector3(0.2f, 0, 0));
			LineRenderer.SetPosition(1, new Vector3(0.2f, 0, 0));
			LineRenderer.enabled = Settings.bulletWarnings;

			if (!Settings.bulletWarnings)
				return;

			Transition end = new Transition(() => LineRenderer.GetPosition(1).x, value => LineRenderer.SetPosition(1, new Vector3(value, 0, 0)), HashCode.Combine(LineRenderer, "end"));
			await end.Modify(0.2f, -8f, duration, EaseFunction.Quadratic, EaseDirection.InOut).Await();
		}
		private async void EraseLaser()
		{
			if (!LineRenderer) return;

			if (!Settings.bulletWarnings)
			{
				LineRenderer.enabled = false;
				return;
			}

			Transition start = new Transition(() => LineRenderer.GetPosition(0).x, value => LineRenderer.SetPosition(0, new Vector3(value, 0, 0)), HashCode.Combine(LineRenderer, "start"));
			await start.Modify(0.2f, -8f, 0.1f, EaseFunction.Linear, EaseDirection.InOut).Await();

			LineRenderer.SetPosition(0, new Vector3(0.2f, 0, 0));
			LineRenderer.SetPosition(1, new Vector3(0.2f, 0, 0));
			LineRenderer.enabled = false;
		}
		private async void MuzzleFlash()
		{
			GunTipRenderer.enabled = true;
			GunTipRenderer.sprite = Monolith.Refs.muzzleFlash1;
			await Awaitable.WaitForSecondsAsync(0.08f);
			GunTipRenderer.sprite = Monolith.Refs.muzzleFlash2;
			await Awaitable.WaitForSecondsAsync(0.08f);
			GunTipRenderer.sprite = Monolith.Refs.muzzleFlash3;
			await Awaitable.WaitForSecondsAsync(0.08f);
			GunTipRenderer.enabled = false;
		}

		public void Wiggle(float amount)
		{
			if (wiggleDirection)
			{
				wiggle += Time.deltaTime * amount;
				if (wiggle > 1)
				{
					wiggle = 1;
					wiggleDirection = false;
				}
			}
			else
			{
				wiggle -= Time.deltaTime * amount;
				if (wiggle < -1)
				{
					wiggle = -1;
					wiggleDirection = true;
				}
			}

			transform.rotation = Quaternion.Euler(new Vector3(0, 0, wiggle * 6));
		}
		public void SetSprites(Sprites sprites)
		{
			this.sprites = sprites;
			SpriteRenderer.sprite = sprites.normal;
		}
		public async void ShowCoin()
		{
			CoinRenderer.gameObject.SetActive(true);
			CoinRenderer.transform.localPosition = new Vector2(0, 0.8f);
			CoinRenderer.color = Color.white;

			CoinAnimater.Restart();
			CoinRenderer.transform.TransitionLocalPositionY().Modify(0.8f, 1.2f, 0.5f, EaseFunction.Back, EaseDirection.Out).Run();
			await CoinRenderer.TransitionColorA().Modify(1, 0, 1.2f, EaseFunction.Linear, EaseDirection.InOut).Await();

			CoinRenderer.gameObject.SetActive(false);
		}

		public async void Die()
		{
			Died?.Invoke();

			HitTrigger.enabled = false;
			MoveCollider.enabled = false;
			if (Agent) Agent.enabled = false;
			Gun.gameObject.SetActive(false);

			SpriteRenderer.sprite = sprites.hit;
			await Awaitable.WaitForSecondsAsync(0.2f);
			SpriteRenderer.sprite = sprites.dead;

			for (int i = 0; i < 4; i++)
			{
				await Awaitable.WaitForSecondsAsync(0.8f);
				SpriteRenderer.color = new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b, 0.6f);
				await Awaitable.WaitForSecondsAsync(0.4f);
				SpriteRenderer.color = new Color(SpriteRenderer.color.r, SpriteRenderer.color.g, SpriteRenderer.color.b, 1);
			}

			Disposed?.Invoke();

			Died = null;
			Disposed = null;
		}
	}
}