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
		}
		#endregion References


		[SerializeField] private References refs;
		public static References Refs => Instance.refs;

		public static GameObject InstanceObject { get; private set; }
		public static Monolith Instance { get; private set; }

		public static GameObject CameraObject { get; private set; }
		public static UnityEngine.Camera Camera { get; private set; }


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
			DontDestroyOnLoad(InstanceObject);

			CameraObject = GameObject.FindWithTag("MainCamera");
			Camera = CameraObject.GetComponent<UnityEngine.Camera>();
			DontDestroyOnLoad(CameraObject);
		}
		private void Start()
		{
			UI.Overlay.Show();
			UI.Menu.Show();
			UI.Settings.Hide();
			UI.Hud.Hide();
		}
	}
}