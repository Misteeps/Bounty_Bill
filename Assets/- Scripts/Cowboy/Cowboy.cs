using UnityEngine;

namespace Game
{
	public class Cowboy : MonoBehaviour
	{
		public bool bullet;
		public float speed = 0;
		public float dodgeCooldown = 0;
		public float dodgeTimer = 0;


		public void Move(Vector2 movement)
		{
			transform.position += new Vector3(movement.x, movement.y, 0);
		}
		public void LookAt(Vector2 target)
		{

		}
		public async void Shoot(float delay)
		{
			await Awaitable.WaitForSecondsAsync(delay);
		}
	}
}