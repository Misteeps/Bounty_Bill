using UnityEngine;

namespace Game
{
	public class Bullet : MonoBehaviour
	{
		private Rigidbody2D rb;

		private void Awake()
		{
			rb = GetComponent<Rigidbody2D>();
		}

		private void Start()
		{

		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			//probably call async function here to manage switching to an inactive state such as swapping colliders
		}
	}
}
