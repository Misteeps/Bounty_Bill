using UnityEngine;

namespace Game
{
	public class Cowboy : MonoBehaviour
	{
		[SerializeField] private bool bullet;
        [SerializeField] private float speed = 0;
        [SerializeField] private float dodgeCooldown = 0;
        [SerializeField] private float dodgeTimer = 0;
		[SerializeField] private GameObject bulletPrefab;
		[SerializeField] private Transform gunTip;
        [SerializeField] private Transform gunObject;
        Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        public void Move(Vector2 movement)
		{
            //transform.position += new Vector3(movement.x, movement.y, 0);
            rb.MovePosition(rb.position + movement * (20f * Time.fixedDeltaTime));
        }
		public void LookAt(Vector2 target)
		{
            Vector3 direction = target - (Vector2)gunObject.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gunObject.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
		public async void Shoot(float delay)
		{
			await Awaitable.WaitForSecondsAsync(delay);
			Instantiate(bulletPrefab, gunTip.position, Quaternion.Euler(gunObject.transform.rotation.eulerAngles));
            //need to check for bullets here
		}
	}
}