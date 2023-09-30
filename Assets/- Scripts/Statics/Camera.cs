using System;

using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering.Universal;

using Cinemachine;
using Simplex;


namespace Game
{
	public static class Camera
	{
		public static CinemachineVirtualCamera VirtualCamera { get; } = Monolith.Camera.GetComponent<CinemachineVirtualCamera>();
		public static CinemachineOrbitalTransposer Transposer { get; } = VirtualCamera.GetCinemachineComponent<CinemachineOrbitalTransposer>();

		public static Bloom Bloom { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is Bloom) as Bloom;
		public static Vignette Vignette { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is Vignette) as Vignette;
		public static FilmGrain FilmGrain { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is FilmGrain) as FilmGrain;
		public static LensDistortion LensDistortion  { get; } = Monolith.Refs.volumeProfile.components.Find(component => component is LensDistortion) as LensDistortion;

		public static float Size { get => VirtualCamera.m_Lens.OrthographicSize; set => VirtualCamera.m_Lens.OrthographicSize = value; }
	}
}