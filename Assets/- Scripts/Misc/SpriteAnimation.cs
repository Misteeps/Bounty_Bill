using UnityEngine;


namespace Game
{
	public class SpriteAnimation : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer spriteRenderer;
		[SerializeField] private float frameTime = 0.1f;
		[SerializeField] private bool loop = true;
		[SerializeField] private Sprite[] sprites;
		private float timer;
		private int frame;


		public void Update()
		{
			timer += Time.deltaTime;
			if (timer >= frameTime)
			{
				timer -= frameTime;
				frame++;

				if (frame >= sprites.Length)
				{
					if (loop) frame = 0;
					else return;
				}

				spriteRenderer.sprite = sprites[frame];
			}
		}

		public void Restart()
		{
			spriteRenderer.sprite = sprites[0];
			frame = 0;
			timer = 0;
		}
	}
}