using System;

using UnityEngine;
using UnityEngine.AI;


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
		public new Rigidbody2D RigidBody;
		public NavMeshAgent Agent;
		public Transform Gun;
		public Transform GunTip;
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
			if (Agent != null) Agent.enabled = true;
			Gun.gameObject.SetActive(true);
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
					// Draw laser
					await Awaitable.WaitForSecondsAsync(delay);
					// Erase laser

					if (!Gun.gameObject.activeSelf) return;
				}

				Quaternion rotation = (transform.localScale.x < 0) ? Gun.transform.rotation : Quaternion.Euler(new Vector3(0, 0, Gun.transform.eulerAngles.z + 180));
				BulletActive.Spawn(GunTip.position, rotation, gameObject);

				HasBullet = false;
				IsShooting = false;
			}
			else
			{
				// No bitches?
			}
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

		public async void Die()
		{
			Died?.Invoke();

			HitTrigger.enabled = false;
			MoveCollider.enabled = false;
			if (Agent != null) Agent.enabled = false;
			Gun.gameObject.SetActive(false);

			SpriteRenderer.sprite = sprites.hit;
			await Awaitable.WaitForSecondsAsync(0.2f);
			SpriteRenderer.sprite = sprites.dead;
			await Awaitable.WaitForSecondsAsync(2f);

			Disposed?.Invoke();

			Died = null;
			Disposed = null;
		}
	}
}