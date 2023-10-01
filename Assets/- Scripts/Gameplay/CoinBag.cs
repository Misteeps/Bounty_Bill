using System;

using UnityEngine;
using UnityEngine.Pool;

using Simplex;


namespace Game
{
	public class CoinBag : MonoBehaviour
	{
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => Instantiate(Monolith.Refs.coinBagPrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);

		private bool triggered;


		public static void OnGet(GameObject obj) => obj.SetActive(true);
		public static void OnRelease(GameObject obj) => obj.SetActive(false);
		public static CoinBag Spawn(Vector2 position)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.coinBagRoot;
			obj.transform.position = position;

			CoinBag bag = obj.GetComponent<CoinBag>();
			bag.triggered = false;

			return bag;
		}
		public void Dispose() => pool.Release(gameObject);

		private void OnTriggerEnter2D(Collider2D collision)
		{
			if (collision.gameObject != Monolith.PlayerObject || triggered) return;
			triggered = true;

			try
			{
				Monolith.Player.ShowCoin();
				Monolith.fortune += 1000;
				UI.Hud.Instance.UpdateFortune();
			}
			catch (Exception exception) { exception.Error($"Coin Bag triggered unexpectedly by {collision.gameObject}"); }

			Dispose();
		}

		public static void CleanUp()
		{
			pool.Clear();

			for (int i = Monolith.Refs.coinBagRoot.childCount - 1; i >= 0; i--)
				try { Destroy(Monolith.Refs.coinBagRoot.GetChild(i).gameObject); }
				catch (Exception exception) { exception.Error($"Failed cleaning coin bags"); }
		}
	}
}