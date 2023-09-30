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

			SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>();
			spriteRenderer.color = Color.HSVToRGB(0, 0, RandomFloat(0.8f, 1f));
			spriteRenderer.sprite = RandomInt(1, 4) switch
			{
				1 => Monolith.Refs.cowboy2,
				2 => Monolith.Refs.cowboy3,
				3 => Monolith.Refs.cowboy4,
				_ => throw new Exception("Invalid cowboy sprite")
			};

			NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
			agent.updateRotation = false;
			agent.updateUpAxis = false;
			agent.speed = RandomFloat(4f, 8f);

			Cowboy cowboy = obj.GetComponent<Cowboy>();
			cowboy.bullet = true;
			cowboy.speed = agent.speed;

			return cowboy;
		}

		public static void Update()
		{
			if (Inputs.Dodge.Down)
				Spawn(Vector2.zero);

			foreach (Cowboy cowboy in Hunting.ToArray())
				try
				{
					if (cowboy.shooting) continue;

					cowboy.LookAt(PlayerPosition);

					if (Vector2.Distance(cowboy.agent.destination, PlayerPosition) > 6)
					{
						cowboy.agent.destination = RandomPosition(6);
					}
					else if (cowboy.agent.isStopped)
					{
						cowboy.Shoot(1.2f);
						Hunting.Remove(cowboy);
						Reloading.Add(cowboy, null);
					}
				}
				catch (Exception exception) { exception.Error($"Failed updating hunting enemy {cowboy?.gameObject}"); }

			foreach ((Cowboy cowboy, BulletInactive bullet) in Reloading.ToArray())
				try
				{
					if (cowboy.shooting) continue;

					cowboy.gun.rotation = new Quaternion();

					if (bullet != null)
					{
						cowboy.agent.destination = bullet.transform.position;
					}
					else if (inactiveBullets.Count > 0)
					{
						int index = RandomInt(0, inactiveBullets.Count - 1);
						Reloading[cowboy] = inactiveBullets[index];
						inactiveBullets.RemoveAt(index);
					}
					else if (cowboy.agent.isStopped)
					{
						cowboy.agent.destination = RandomPosition(16);
					}
				}
				catch (Exception exception) { exception.Error($"Failed updating reloading enemy {cowboy?.gameObject}"); }
		}

		private static int RandomInt(int min, int max) => UnityEngine.Random.Range(min, max);
		private static float RandomFloat(float min, float max) => UnityEngine.Random.Range(min, max);
		private static Vector2 RandomPosition(float radius) => PlayerPosition + (UnityEngine.Random.insideUnitCircle * radius);
	}
}