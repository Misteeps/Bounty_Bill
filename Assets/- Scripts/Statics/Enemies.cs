using System;
using System.Linq;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;

using Simplex;


namespace Game
{
	public static class Enemies
	{
		#region Difficulty
		public readonly struct Difficulty
		{
			public readonly int stars;
			public readonly float wavesDelay;
			public readonly int wavesThreshold;
			public readonly (int min, int max) waveDensity;
			public readonly (float min, float max) speed;
			public readonly float shootDelay;
			public readonly float shootDistance;

			public Difficulty(int stars, float wavesDelay, int wavesThreshold, (int min, int max) waveDensity, (float min, float max) speed, float shootDelay, float shootDistance)
			{
				this.stars = stars;
				this.wavesDelay = wavesDelay;
				this.wavesThreshold = wavesThreshold;
				this.waveDensity = waveDensity;
				this.speed = speed;
				this.shootDelay = shootDelay;
				this.shootDistance = shootDistance;
			}
		}
		#endregion Difficulty


		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => GameObject.Instantiate(Monolith.Refs.cowboyPrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);
		public static List<Cowboy> Hunting { get; private set; } = new List<Cowboy>();
		public static Dictionary<Cowboy, BulletInactive> Reloading { get; private set; } = new Dictionary<Cowboy, BulletInactive>();
		public static List<BulletInactive> inactiveBullets { get; private set; } = new List<BulletInactive>();

		private static Vector2 PlayerPosition => Monolith.Player.transform.position;
		public static int Count => Hunting.Count + Reloading.Count;
		public static Difficulty[] Difficulties { get; } = new Difficulty[]
		{
			new Difficulty(stars: 0, wavesDelay: 10, wavesThreshold: 0, waveDensity: (1, 2), speed: (1, 3), shootDelay: 1.1f, shootDistance: 5),
			new Difficulty(stars: 1, wavesDelay: 10, wavesThreshold: 1, waveDensity: (2, 3), speed: (1, 4), shootDelay: 9f, shootDistance: 5),
			new Difficulty(stars: 2, wavesDelay: 15, wavesThreshold: 1, waveDensity: (3, 4), speed: (1.5f, 5), shootDelay: 0.7f, shootDistance: 6),
			new Difficulty(stars: 3, wavesDelay: 20, wavesThreshold: 2, waveDensity: (4, 6), speed: (2, 6), shootDelay: 0.6f, shootDistance: 6),
			new Difficulty(stars: 4, wavesDelay: 25, wavesThreshold: 2, waveDensity: (6, 8), speed: (2.5f, 6), shootDelay: 0.5f, shootDistance: 7),
			new Difficulty(stars: 5, wavesDelay: 30, wavesThreshold: 3, waveDensity: (8, 12), speed: (3, 7), shootDelay: 0.4f, shootDistance: 8),
		};

		public static Difficulty difficulty;
		public static float timer;


		public static void OnGet(GameObject obj)
		{
			obj.SetActive(true);
			Hunting.Add(obj.GetComponent<Cowboy>());
		}
		public static void OnRelease(GameObject obj)
		{
			obj.SetActive(false);
			Hunting.Remove(obj.GetComponent<Cowboy>());
			Reloading.Remove(obj.GetComponent<Cowboy>());
		}
		public static Cowboy Spawn(Vector2 position)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.cowboyRoot;
			obj.transform.position = position;

			NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
			agent.updateRotation = false;
			agent.updateUpAxis = false;
			agent.speed = RandomFloat(difficulty.speed.min, difficulty.speed.max);

			Cowboy cowboy = obj.GetComponent<Cowboy>();
			cowboy.SetSprites(RandomInt(2, 5) switch
			{
				2 => Monolith.Refs.cowboy2,
				3 => Monolith.Refs.cowboy3,
				4 => Monolith.Refs.cowboy4,
				_ => throw new Exception("Invalid cowboy sprite")
			});
			cowboy.Initialize();
			cowboy.MoveSpeed = agent.speed;
			cowboy.SpriteRenderer.color = Color.HSVToRGB(0, 0, RandomFloat(0.8f, 1f));

			cowboy.Died += () => Remove(cowboy);
			cowboy.Disposed += () => pool.Release(cowboy.gameObject);

			MoveEnemy(cowboy, RandomPosition(difficulty.shootDistance));
			return cowboy;
		}
		public static void Remove(Cowboy cowboy)
		{
			Hunting.Remove(cowboy);
			Reloading.Remove(cowboy);
		}

		public static void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tilde)) difficulty = Difficulties[0];
			else if (Input.GetKeyDown(KeyCode.Alpha1)) difficulty = Difficulties[1];
			else if (Input.GetKeyDown(KeyCode.Alpha2)) difficulty = Difficulties[2];
			else if (Input.GetKeyDown(KeyCode.Alpha3)) difficulty = Difficulties[3];
			else if (Input.GetKeyDown(KeyCode.Alpha4)) difficulty = Difficulties[4];
			else if (Input.GetKeyDown(KeyCode.Alpha5)) difficulty = Difficulties[5];

			timer += Time.deltaTime;
			if (timer > difficulty.wavesDelay || Count <= difficulty.wavesThreshold)
			{
				timer = 0;
				int density = RandomInt(difficulty.waveDensity.min, difficulty.waveDensity.max + 1);
				for (int i = 0; i < density; i++)
					Spawn(RandomSpawn(20, 20));
			}

			foreach (Cowboy cowboy in Hunting.ToArray())
				try
				{
					if (cowboy.IsShooting) continue;
					cowboy.Wiggle(cowboy.Agent.velocity.magnitude * 2);

					if (!cowboy.HasBullet)
					{
						Hunting.Remove(cowboy);
						Reloading.Add(cowboy, null);
					}

					else if (Vector2.Distance(cowboy.Agent.destination, PlayerPosition) > difficulty.shootDistance)
					{
						MoveEnemy(cowboy, RandomPosition(difficulty.shootDistance));
					}

					else if (cowboy.Agent.remainingDistance < 0.4f && Vector2.Distance(cowboy.transform.position, PlayerPosition) < difficulty.shootDistance)
					{
						cowboy.LookAt(PlayerPosition);
						cowboy.Shoot(difficulty.shootDelay);
					}
				}
				catch (Exception exception) { exception.Error($"Failed updating hunting enemy {cowboy?.gameObject}"); }

			foreach ((Cowboy cowboy, BulletInactive bullet) in Reloading.ToArray())
				try
				{
					if (cowboy.IsShooting) continue;
					cowboy.Wiggle(cowboy.Agent.velocity.magnitude * 2);

					if (bullet != null)
					{
						if (bullet.gameObject.activeSelf)
							MoveEnemy(cowboy, bullet.transform.position);
						else
							Reloading[cowboy] = null;
					}

					else if (inactiveBullets.Count > 0)
					{
						int index = RandomInt(0, inactiveBullets.Count - 1);
						Reloading[cowboy] = inactiveBullets[index];
						inactiveBullets.RemoveAt(index);
					}

					else if (cowboy.Agent.remainingDistance < 0.05f)
					{
						MoveEnemy(cowboy, RandomPosition(20));
					}
				}
				catch (Exception exception) { exception.Error($"Failed updating reloading enemy {cowboy?.gameObject}"); }
		}

		public static void ReloadEnemy(Cowboy cowboy)
		{
			if (cowboy.HasBullet) throw new Exception("Cowboy already has bullet");
			cowboy.HasBullet = true;

			if (Reloading.ContainsKey(cowboy))
			{
				Reloading.Remove(cowboy);
				Hunting.Add(cowboy);

				MoveEnemy(cowboy, RandomPosition(6));
			}
		}
		private static void MoveEnemy(Cowboy cowboy, Vector2 position)
		{
			cowboy.Agent.destination = position;
			cowboy.LookAt(position);
		}

		private static int RandomInt(int min, int max) => UnityEngine.Random.Range(min, max);
		private static float RandomFloat(float min, float max) => UnityEngine.Random.Range(min, max);
		private static Vector2 RandomPosition(float radius) => PlayerPosition + (UnityEngine.Random.insideUnitCircle * radius);
		private static Vector2 RandomSpawn(int sizeX, int sizeY)
		{
			for (int i = 0; i < 100; i++)
			{
				Vector2 position = new Vector2(RandomFloat(-sizeX, sizeX), RandomFloat(-sizeY, sizeY));
				if (Vector2.Distance(position, PlayerPosition) > 16)
					return position;
			}

			return Vector2.zero;
		}

		public static void CleanUp()
		{
			pool.Clear();
			Hunting.Clear();
			Reloading.Clear();
			inactiveBullets.Clear();

			for (int i = Monolith.Refs.cowboyRoot.childCount - 1; i >= 0; i--)
				try { GameObject.Destroy(Monolith.Refs.cowboyRoot.GetChild(i).gameObject); }
				catch (Exception exception) { exception.Error($"Failed cleaning enemy cowboys"); }
		}
	}
}