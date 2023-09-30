using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

using Simplex;


namespace Game
{
	public class Monolith : MonoBehaviour
	{
		#region References
		[Serializable]
		public class References
		{
			public UniversalRenderPipelineAsset quality;
			public VolumeProfile volumeProfile;
			public AudioMixer audioMixer;
			public UIDocument uiDocument;
			public StyleSheet uiStyle;
			public Transform cowboyRoot;
			public GameObject cowboyPrefab;
			public Transform bulletActiveRoot;
			public GameObject bulletActivePrefab;
			public Transform bulletInactiveRoot;
			public GameObject bulletInactivePrefab;
			public Cowboy.Sprites cowboy1;
			public Cowboy.Sprites cowboy2;
			public Cowboy.Sprites cowboy3;
			public Cowboy.Sprites cowboy4;
		}
		#endregion References


		[SerializeField] private References refs;
		public static References Refs => Instance.refs;

		public static GameObject InstanceObject { get; private set; }
		public static Monolith Instance { get; private set; }

		public static GameObject CameraObject { get; private set; }
		public static UnityEngine.Camera Camera { get; private set; }

		public static GameObject PlayerObject { get; private set; }
		public static Cowboy Player { get; private set; }

		public static bool Paused { get => Time.timeScale == 0; set => Time.timeScale = (value) ? 0 : 1; }


		private void Awake()
		{
#if UNITY_EDITOR
			// References validator
			int nullRefs = 0;
			Validate(refs);

			if (nullRefs > 0)
			{
				ConsoleUtilities.Error($"Missing {nullRefs:info} references");
				UnityEditor.EditorApplication.isPlaying = false;
			}

			void Validate<T>(T references) where T : class
			{
				foreach (var field in references.GetType().GetFields())
				{
					object value = field.GetValue(references);
					if (value == null || value.ToString() == "null")
					{
						ConsoleUtilities.Warn($"Reference {field.FieldType:type} {field.Name:info} missing");
						nullRefs += 1;
					}
				}
			}
#endif

			InstanceObject = GameObject.FindWithTag("GameController");
			Instance = InstanceObject.GetComponent<Monolith>();

			CameraObject = GameObject.FindWithTag("MainCamera");
			Camera = CameraObject.GetComponent<UnityEngine.Camera>();

			PlayerObject = GameObject.FindWithTag("Player");
			Player = PlayerObject.GetComponent<Cowboy>();
		}
		private void Start()
		{
			UI.Hud.Hide();
			UI.Menu.Show();
			UI.Settings.Hide();
			UI.Overlay.Show();

			Settings.Defaults();

			Game.Camera.VirtualCamera.enabled = false;

			Player.SetSprites(Refs.cowboy1);
		}

		private void Update()
		{
			if (Inputs.Escape.Down)
			{
				Paused = !Paused;
				if (Paused) UI.Settings.Show();
				else UI.Settings.Hide();
			}

			if (Paused)
				return;

			PlayerLook();
			PlayerShoot();
			Enemies.Update();
		}
		private void FixedUpdate()
		{
			PlayerMove();
		}

		private void PlayerMove()
		{
			float x = 0;
			float y = 0;

			if (Inputs.MoveUp.Held) y += 1;
			if (Inputs.MoveDown.Held) y -= 1;
			if (Inputs.MoveRight.Held) x += 1;
			if (Inputs.MoveLeft.Held) x -= 1;

			if (x == 0 && y == 0)
				return;

			Player.Move(Vector2.ClampMagnitude(new Vector2(x, y), 0.1f));
			Player.Wiggle(10);
		}
		private void PlayerLook()
		{
			Inputs.UpdateWorldCursor(0);
			Player.LookAt(Inputs.worldCursor);
		}
		private void PlayerShoot()
		{
			if (Inputs.Shoot.Down)
				Player.Shoot(0);
		}

		public static async void GameStart()
		{
			Player.Initialize();
		}
		private static async void GameEnd()
		{

		}
	}
}