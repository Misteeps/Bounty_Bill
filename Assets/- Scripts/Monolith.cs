using System;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;

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
			public Transform bulletSpecialRoot;
			public GameObject bulletSpecialPrefab;
			public Transform bulletInactiveRoot;
			public GameObject bulletInactivePrefab;
			public Transform coinBagRoot;
			public GameObject coinBagPrefab;
			public Cowboy.Sprites cowboy1;
			public Cowboy.Sprites cowboy2;
			public Cowboy.Sprites cowboy3;
			public Cowboy.Sprites cowboy4;
			public Sprite revolver;
			public Sprite shotgun;
			public Sprite muzzleFlash1;
			public Sprite muzzleFlash2;
			public Sprite muzzleFlash3;
			public Sprite[] bountyStars;
			public Sprite[] bountySparks;
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

		public static float time;
		public static int bounty;
		public static int fortune;
		public static int special;


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
			UI.Overlay.Instance.UpdateCrosshair(false);

			Settings.Defaults();

			Game.Camera.VirtualCamera.enabled = false;
			Game.Camera.VignetteTransition.Modify(0.2f, 0.2f, 0, EaseFunction.Linear, EaseDirection.InOut).Run();

			Player.SetSprites(Refs.cowboy1);

			Instance.enabled = false;
		}

		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Tab))
			{
				special = 7;
				UI.Hud.Instance.SetSpecial(7);
			}

			UI.Overlay.Instance.UpdateCrosshair();

			if (Inputs.Escape.Down)
			{
				Paused = !Paused;
				if (Paused) UI.Settings.Show();
				else UI.Settings.Hide();
			}

			if (Paused)
				return;

			time += Time.deltaTime;

			PlayerLook();
			PlayerShoot();
			PlayerSpecial();
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
			if (Inputs.Shoot.Down && !Player.IsShooting)
			{
				if (Player.InSpecial)
				{
					Player.ShootSpecial(0);
					UI.Hud.Instance.SetBullet(false);
				}
				else if (Player.HasBullet)
				{
					Player.Shoot(0);
					UI.Hud.Instance.SetBullet(false);
				}
				else
				{
					if (UI.Hud.warnTimer == -1) UI.Hud.Instance.WarnBullet();
					UI.Hud.warnTimer = 3;
				}
			}
		}
		private void PlayerSpecial()
		{
			if (Inputs.Special.Down && special >= 7 && !Player.InSpecial && !Player.IsShooting)
				Player.ActivateSpecial();
		}

		public static async void GameStart()
		{
			time = 0;
			bounty = 10;
			fortune = 0;
			special = 0;

			UI.Hud.Instance.UpdateWanted();
			UI.Hud.Instance.UpdateFortune();
			UI.Hud.Instance.UpdateTime();
			UI.Hud.Instance.SetBullet(true);
			UI.Hud.Instance.SetSpecial(0);

			Enemies.SetDifficulty(0);

			Player.Initialize();
			Player.transform.position = new Vector2(0, -6);
			Player.Gun.localScale = new Vector2(0, 0);
			Player.GunRenderer.sprite = Refs.revolver;
			Player.Died += GameEnd;

#if !UNITY_EDITOR
			void Walk(float position)
			{
				Player.transform.position = new Vector2(0, position);
				Player.Wiggle(6);
			}

			Game.Camera.VignetteTransition.Modify(0.2f, 1f, 2f, EaseFunction.Circular, EaseDirection.Out).Run();
			new Transition(() => Player.transform.position.y, Walk).Modify(-6, 0, 2f, EaseFunction.Circular, EaseDirection.Out).Run();
			await Awaitable.WaitForSecondsAsync(1.6f);

			UI.Overlay.Instance.ShowIntro();
			await Awaitable.WaitForSecondsAsync(3.6f);
#endif

			Game.Camera.VirtualCamera.enabled = true;
			Game.Camera.VignetteTransition.Modify(1f, 0.2f, 1f, EaseFunction.Circular, EaseDirection.InOut).Run();
			Player.Gun.TransitionLocalScaleX().Modify(0, 1, 0.6f, EaseFunction.Back, EaseDirection.Out).Run();
			Player.Gun.TransitionLocalScaleY().Modify(0, 1, 0.6f, EaseFunction.Back, EaseDirection.Out).Run();
			await Awaitable.WaitForSecondsAsync(0.6f);

			UI.Hud.Show();

			Instance.enabled = true;
			Paused = false;
		}
		private static async void GameEnd()
		{
			UI.Overlay.Instance.UpdateCrosshair(false);
			Instance.enabled = false;

			Game.Camera.VignetteTransition.Modify(0.2f, 1f, 2f, EaseFunction.Circular, EaseDirection.Out).Run();
			UI.Hud.Hide();
			await Awaitable.WaitForSecondsAsync(1.6f);

			UI.Overlay.Instance.ShowResults();
			while (true)
			{
				await Awaitable.NextFrameAsync();
				if (Inputs.Shoot.Down)
					break;
			}
			UI.Overlay.Instance.HideResults();

			UI.Overlay.Faded = true;
			await Awaitable.WaitForSecondsAsync(1.2f);

			Game.Camera.VirtualCamera.enabled = false;
			Game.Camera.VignetteTransition.Modify(0.2f, 0.2f, 0, EaseFunction.Linear, EaseDirection.InOut).Run();
			Camera.transform.position = Vector3.zero;
			Player.transform.position = new Vector2(0, -6);

			Enemies.CleanUp();
			BulletActive.CleanUp();
			BulletInactive.CleanUp();
			CoinBag.CleanUp();

			UI.Menu.Instance.Show(0);
			UI.Overlay.Faded = false;
		}
	}
}