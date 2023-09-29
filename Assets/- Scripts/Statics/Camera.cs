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

		public static float FOV { get => VirtualCamera.m_Lens.FieldOfView; set => VirtualCamera.m_Lens.FieldOfView = value; }
	}
}