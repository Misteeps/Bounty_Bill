using System;
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
		public static List<Cowboy> Active { get; private set; } = new List<Cowboy>();


		public static void OnGet(GameObject obj)
		{
			obj.SetActive(true);
			Active.Add(obj.GetComponent<Cowboy>());
		}
		public static void OnRelease(GameObject obj)
		{
			obj.SetActive(false);
			Active.Remove(obj.GetComponent<Cowboy>());
		}
		public static Cowboy Spawn(Vector2 position)
		{
			GameObject obj = pool.Get();
			obj.transform.parent = Monolith.Refs.cowboyRoot;
			obj.transform.position = position;

			NavMeshAgent agent = obj.GetComponent<NavMeshAgent>();
			agent.updateRotation = false;
			agent.updateUpAxis = false;

			obj.GetComponent<SpriteRenderer>().color = RNG.Generic.Color();

			Cowboy cowboy = obj.GetComponent<Cowboy>();

			return cowboy;
		}

		public static void Update()
		{
			if (Inputs.Dodge.Down)
				Spawn(Vector2.zero);

		}
		public static void FixedUpdate()
		{
			Active.ForEach(cowboy => cowboy.agent.SetDestination(Monolith.Player.transform.position));
		}
	}
}