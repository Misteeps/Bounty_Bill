using System;

using UnityEngine;
using UnityEngine.AI;


namespace Game
{
	public class Cowboy : MonoBehaviour
	{
		[SerializeField] public BoxCollider2D hitbox;
		[SerializeField] public CapsuleCollider2D movebox;
		[SerializeField] public new Rigidbody2D rigidbody;
		[SerializeField] public NavMeshAgent agent;
		[SerializeField] public Transform gun;
		[SerializeField] public Transform gunTip;
		[SerializeField] public bool bullet = true;
		[SerializeField] public float speed = 1f;
		[SerializeField] public float dodgeCooldown = 3f;
		[SerializeField] public float dodgeTimer = 0;

		public event Action Died;


		public void Move(Vector2 movement) => rigidbody.MovePosition(rigidbody.position + movement * speed);
		public void LookAt(Vector2 target)
		{
			transform.localScale = new Vector3((target.x < transform.position.x) ? -1 : 1, 1, 1);

			Vector3 direction = target - (Vector2)gun.position;
			float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
			gun.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
		}
		public async void Shoot(float delay)
		{
			if (bullet)
			{
				await Awaitable.WaitForSecondsAsync(delay);
				BulletActive.Spawn(gunTip.position, gun.transform.rotation);
				//bullet = false;
			}
			else
			{
				// No bitches?
			}
		}

		public void Die() => Died?.Invoke();
		public void Dispose() => Died = null;
	}
}