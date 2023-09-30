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


		[SerializeField] public SpriteRenderer spriteRenderer;
		[SerializeField] public BoxCollider2D hitbox;
		[SerializeField] public CapsuleCollider2D movebox;
		[SerializeField] public new Rigidbody2D rigidbody;
		[SerializeField] public NavMeshAgent agent;
		[SerializeField] public Transform gun;
		[SerializeField] public Transform gunTip;
		[SerializeField] public bool bullet = true;
		[SerializeField] public bool shooting = false;
		[SerializeField] public float speed = 1f;

		private float wiggle = 0;
		private bool wiggleDirection = false;
		private Sprites sprites;

		public event Action Died;
		public event Action Disposed;


		public void Initialize()
		{
			spriteRenderer.sprite = sprites.normal;
			hitbox.enabled = true;
			movebox.enabled = true;
			if (agent != null) agent.enabled = true;
			gun.gameObject.SetActive(true);
			bullet = true;
			shooting = false;
		}

		public void Move(Vector2 movement) => rigidbody.MovePosition(rigidbody.position + movement * speed);
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
			gun.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
		}
		public async void Shoot(float delay)
		{
			if (bullet)
			{
				shooting = true;

				if (delay != 0)
				{
					// Draw laser
					await Awaitable.WaitForSecondsAsync(delay);
					// Erase laser

					if (!gun.gameObject.activeSelf) return;
				}

				Quaternion rotation = (transform.localScale.x < 0) ? gun.transform.rotation : Quaternion.Euler(new Vector3(0, 0, gun.transform.eulerAngles.z + 180));
				BulletActive.Spawn(gunTip.position, rotation, gameObject);

				bullet = false;
				shooting = false;
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
			spriteRenderer.sprite = sprites.normal;
		}

		public async void Die()
		{
			Died?.Invoke();

			hitbox.enabled = false;
			movebox.enabled = false;
			if (agent != null) agent.enabled = false;
			gun.gameObject.SetActive(false);

			spriteRenderer.sprite = sprites.hit;
			await Awaitable.WaitForSecondsAsync(0.2f);
			spriteRenderer.sprite = sprites.dead;
			await Awaitable.WaitForSecondsAsync(2f);

			Disposed?.Invoke();

			Died = null;
			Disposed = null;
		}
	}
}