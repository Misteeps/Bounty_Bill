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
		private static readonly ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => GameObject.Instantiate(Monolith.Refs.cowboyPrefab), actionOnGet: OnGet, actionOnRelease: OnRelease);
		public static List<Cowboy> Hunting { get; private set; } = new List<Cowboy>();
		public static Dictionary<Cowboy, BulletInactive> Reloading { get; private set; } = new Dictionary<Cowboy, BulletInactive>();
		public static List<BulletInactive> inactiveBullets { get; private set; } = new List<BulletInactive>();

		private static Vector2 PlayerPosition => Monolith.Player.transform.position;


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
			agent.speed = RandomFloat(1f, 6f);

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

			MoveEnemy(cowboy, RandomPosition(6));
			return cowboy;
		}
		public static void Remove(Cowboy cowboy)
		{
			Hunting.Remove(cowboy);
			Reloading.Remove(cowboy);
		}

		public static void Update()
		{
			if (RandomInt(0, 100) < 4)
				Spawn(RandomSpawn(20, 20));

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

					else if (Vector2.Distance(cowboy.Agent.destination, PlayerPosition) > 6)
					{
						MoveEnemy(cowboy, RandomPosition(6));
					}

					else if (cowboy.Agent.remainingDistance < 0.4f && Vector2.Distance(cowboy.transform.position, PlayerPosition) < 6)
					{
						cowboy.LookAt(PlayerPosition);
						cowboy.Shoot(RandomFloat(0.4f, 0.8f));
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