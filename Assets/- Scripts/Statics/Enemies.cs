using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Pool;

using Simplex;


namespace Game
{
	public static class Enemies
	{
		private static ObjectPool<GameObject> pool = new ObjectPool<GameObject>(() => GameObject.Instantiate(Monolith.Refs.cowboyPrefab), actionOnGet: obj => obj.SetActive(true), actionOnRelease: obj => obj.SetActive(false));
		public static List<Cowboy> Active { get; private set; } = new List<Cowboy>();


		public static void Update() { }
		public static void FixedUpdate() { }
	}
}