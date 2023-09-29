using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

using UnityEngine;


namespace Simplex
{
	public class Transition
	{
		private static Dictionary<int, Transition> transitions = new Dictionary<int, Transition>(0);
		public static int Active => transitions.Count;

		private readonly IValue<float> iValue;
		public readonly int hash;

		public float start;
		public float end;
		public float duration;
		public ICurve curve;
		public float speed;
		public bool realTime;
		public bool rewinding;
		public Action onEnd;

		public float timer;
		public float Value { get => iValue.Value; set => iValue.Value = value; }

		private float clampedStart;
		private CancellationTokenSource canceller;


		public Transition(Func<float> getValue, Action<float> setValue) : this(new DelegateValue<float>(getValue, setValue)) { }
		public Transition(Func<float> getValue, Action<float> setValue, string hash) : this(new DelegateValue<float>(getValue, setValue), hash.GetHashCode()) { }
		public Transition(Func<float> getValue, Action<float> setValue, int hash) : this(new DelegateValue<float>(getValue, setValue), hash) { }
		public Transition(IValue<float> iValue) : this(iValue, iValue.GetHashCode()) { }
		public Transition(IValue<float> iValue, string hash) : this(iValue, hash.GetHashCode()) { }
		public Transition(IValue<float> iValue, int hash)
		{
			this.iValue = iValue;
			this.hash = hash;

			float current = Value;
			Modify(current, current, 0);
		}

		public Transition Modify(float target, float duration, EaseFunction function, EaseDirection direction, float speed = 1, bool realTime = false, bool rewinding = false, Action onEnd = null) => Modify(target, target, duration, ICurve.GetEaseCurve(function, direction), speed, realTime, rewinding, onEnd);
		public Transition Modify(float target, float duration, ICurve curve = null, float speed = 1, bool realTime = false, bool rewinding = false, Action onEnd = null) => Modify(target, target, duration, curve, speed, realTime, rewinding, onEnd);
		public Transition Modify(float start, float end, float duration, EaseFunction function, EaseDirection direction, float speed = 1, bool realTime = false, bool rewinding = false, Action onEnd = null) => Modify(start, end, duration, ICurve.GetEaseCurve(function, direction), speed, realTime, rewinding, onEnd);
		public Transition Modify(float start, float end, float duration, ICurve curve = null, float speed = 1, bool realTime = false, bool rewinding = false, Action onEnd = null)
		{
			this.start = start;
			this.end = end;
			this.duration = duration;
			this.curve = curve ?? this.curve ?? new Linear();
			this.speed = speed;
			this.realTime = realTime;
			this.rewinding = rewinding;
			this.onEnd = onEnd;

			return this;
		}

		protected virtual async Awaitable Execute()
		{
			CancellationToken cancel;

			try
			{
				if (transitions.TryGetValue(hash, out Transition existing))
					existing.Stop(false, false);

				if (speed >= 100) { Stop(true, true); return; }
				if (rewinding && Value == start) { Stop(true, true); return; }
				if (!rewinding && Value == end) { Stop(true, true); return; }

				canceller?.Cancel();
				canceller = new CancellationTokenSource();
				cancel = canceller.Token;

				transitions.Add(hash, this);
				Initialize();
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.transitionTag, $"Transition failed to initialize"); }

			try
			{
				while (timer < duration)
				{
					Increment();
					await Awaitable.NextFrameAsync();

					if (cancel.IsCancellationRequested)
						return;
				}
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.transitionTag, $"Transition failed to increment"); }

			Stop(true, true);
		}

		protected virtual void Initialize()
		{
			float current = Value;
			clampedStart = start;
			if (start < end)
			{
				if (current > end)
					clampedStart = (current < end + (end - start)) ? end + (end - start) : current;
				else if (current < start)
					clampedStart = current;
			}
			else
			{
				if (current < end)
					clampedStart = (current > end - (start - end)) ? end - (start - end) : current;
				else if (current > start)
					clampedStart = current;
			}

			float y = Mathf.InverseLerp(clampedStart, end, current);
			float x = curve.Inverse(y);
			timer = Mathf.Lerp(0, duration, x);
		}
		protected virtual void Increment()
		{
			float deltaTime = ((realTime) ? Time.unscaledDeltaTime : Time.deltaTime) * speed;
			timer = (rewinding) ? timer - deltaTime : timer + deltaTime;

			float x = Mathf.InverseLerp(0, duration, timer);
			float y = curve.Evaluate(x);
			Value = Mathf.LerpUnclamped(clampedStart, end, y);
		}

		public void Run() => _ = Execute();
		public async Awaitable Await() => await Execute();

		public void Pause() => Stop(false, false);
		public void Stop() => Stop(true, true);
		public virtual void Stop(bool snapToEnd, bool invokeOnEnd)
		{
			try
			{
				canceller?.Cancel();
				canceller = null;

				if (snapToEnd) Value = (rewinding) ? clampedStart : end;
				if (invokeOnEnd) onEnd?.Invoke();
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.transitionTag, $"Transition failed to end"); }

			try { transitions.Remove(hash); }
			catch (Exception exception) { exception.Error(ConsoleUtilities.transitionTag, $"Transition failed to be removed"); }
		}

		public static void PauseAll() => StopAll(false, false);
		public static void StopAll() => StopAll(true, true);
		public static void StopAll(bool snapToEnd, bool invokeOnEnd)
		{
			try
			{
				foreach (KeyValuePair<int, Transition> kvp in transitions.ToArray())
					kvp.Value.Stop(snapToEnd, invokeOnEnd);
			}
			catch (Exception exception) { exception.Error(ConsoleUtilities.transitionTag, $"Failed stopping all transitions"); }
		}
		public static Transition[] GetAll() => transitions.Select(kvp => kvp.Value).ToArray();

		public static Transition Find<TComponent>(TComponent component, string field) => Find(HashCode.Combine(component, field));
		public static Transition Find(IValue<float> iValue) => Find(iValue.GetHashCode());
		public static Transition Find(string hash) => Find(hash.GetHashCode());
		public static Transition Find(int hash) => (transitions.TryGetValue(hash, out Transition transition)) ? transition : null;

		public override bool Equals(object obj) => obj is Transition transition && transition.hash == hash;
		public override int GetHashCode() => hash;
	}


	public static class TransitionExtensions
	{
		#region Transform
		public static Transition TransitionPositionX(this Transform transform) => new Transition(() => transform.position.x, value => transform.position = new Vector3(value, transform.position.y, transform.position.z), HashCode.Combine(transform, "Position.X"));
		public static Transition TransitionPositionY(this Transform transform) => new Transition(() => transform.position.y, value => transform.position = new Vector3(transform.position.x, value, transform.position.z), HashCode.Combine(transform, "Position.Y"));
		public static Transition TransitionPositionZ(this Transform transform) => new Transition(() => transform.position.z, value => transform.position = new Vector3(transform.position.x, transform.position.y, value), HashCode.Combine(transform, "Position.Z"));

		public static Transition TransitionLocalPositionX(this Transform transform) => new Transition(() => transform.localPosition.x, value => transform.localPosition = new Vector3(value, transform.localPosition.y, transform.localPosition.z), HashCode.Combine(transform, "LocalPosition.X"));
		public static Transition TransitionLocalPositionY(this Transform transform) => new Transition(() => transform.localPosition.y, value => transform.localPosition = new Vector3(transform.localPosition.x, value, transform.localPosition.z), HashCode.Combine(transform, "LocalPosition.Y"));
		public static Transition TransitionLocalPositionZ(this Transform transform) => new Transition(() => transform.localPosition.z, value => transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, value), HashCode.Combine(transform, "LocalPosition.Z"));

		public static Transition TransitionRotationX(this Transform transform) => new Transition(() => transform.rotation.x, value => transform.rotation = new Quaternion(value, transform.rotation.y, transform.rotation.z, transform.rotation.w), HashCode.Combine(transform, "Rotation.X"));
		public static Transition TransitionRotationY(this Transform transform) => new Transition(() => transform.rotation.y, value => transform.rotation = new Quaternion(transform.rotation.x, value, transform.rotation.z, transform.rotation.w), HashCode.Combine(transform, "Rotation.Y"));
		public static Transition TransitionRotationZ(this Transform transform) => new Transition(() => transform.rotation.z, value => transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, value, transform.rotation.w), HashCode.Combine(transform, "Rotation.Z"));
		public static Transition TransitionRotationW(this Transform transform) => new Transition(() => transform.rotation.w, value => transform.rotation = new Quaternion(transform.rotation.x, transform.rotation.y, transform.rotation.z, value), HashCode.Combine(transform, "Rotation.W"));

		public static Transition TransitionLocalRotationX(this Transform transform) => new Transition(() => transform.localRotation.x, value => transform.localRotation = new Quaternion(value, transform.localRotation.y, transform.localRotation.z, transform.localRotation.w), HashCode.Combine(transform, "LocalRotation.X"));
		public static Transition TransitionLocalRotationY(this Transform transform) => new Transition(() => transform.localRotation.y, value => transform.localRotation = new Quaternion(transform.localRotation.x, value, transform.localRotation.z, transform.localRotation.w), HashCode.Combine(transform, "LocalRotation.Y"));
		public static Transition TransitionLocalRotationZ(this Transform transform) => new Transition(() => transform.localRotation.z, value => transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, value, transform.localRotation.w), HashCode.Combine(transform, "LocalRotation.Z"));
		public static Transition TransitionLocalRotationW(this Transform transform) => new Transition(() => transform.localRotation.w, value => transform.localRotation = new Quaternion(transform.localRotation.x, transform.localRotation.y, transform.localRotation.z, value), HashCode.Combine(transform, "LocalRotation.W"));

		public static Transition TransitionEulerAnglesX(this Transform transform) => new Transition(() => transform.eulerAngles.x, value => transform.eulerAngles = new Vector3(value, transform.eulerAngles.y, transform.eulerAngles.z), HashCode.Combine(transform, "EulerAngles.X"));
		public static Transition TransitionEulerAnglesY(this Transform transform) => new Transition(() => transform.eulerAngles.y, value => transform.eulerAngles = new Vector3(transform.eulerAngles.x, value, transform.eulerAngles.z), HashCode.Combine(transform, "EulerAngles.Y"));
		public static Transition TransitionEulerAnglesZ(this Transform transform) => new Transition(() => transform.eulerAngles.z, value => transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, value), HashCode.Combine(transform, "EulerAngles.Z"));

		public static Transition TransitionLocalEulerAnglesX(this Transform transform) => new Transition(() => transform.localEulerAngles.x, value => transform.localEulerAngles = new Vector3(value, transform.localEulerAngles.y, transform.localEulerAngles.z), HashCode.Combine(transform, "LocalEulerAngles.X"));
		public static Transition TransitionLocalEulerAnglesY(this Transform transform) => new Transition(() => transform.localEulerAngles.y, value => transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, value, transform.localEulerAngles.z), HashCode.Combine(transform, "LocalEulerAngles.Y"));
		public static Transition TransitionLocalEulerAnglesZ(this Transform transform) => new Transition(() => transform.localEulerAngles.z, value => transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, value), HashCode.Combine(transform, "LocalEulerAngles.Z"));

		public static Transition TransitionLocalScaleX(this Transform transform) => new Transition(() => transform.localScale.x, value => transform.localScale = new Vector3(value, transform.localScale.y, transform.localScale.z), HashCode.Combine(transform, "LocalScale.X"));
		public static Transition TransitionLocalScaleY(this Transform transform) => new Transition(() => transform.localScale.y, value => transform.localScale = new Vector3(transform.localScale.x, value, transform.localScale.z), HashCode.Combine(transform, "LocalScale.Y"));
		public static Transition TransitionLocalScaleZ(this Transform transform) => new Transition(() => transform.localScale.z, value => transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, value), HashCode.Combine(transform, "LocalScale.Z"));

		public static Transition Transition(this Transform transform, string field) => (field) switch
		{
			"Position.X" => transform.TransitionPositionX(),
			"Position.Y" => transform.TransitionPositionY(),
			"Position.Z" => transform.TransitionPositionZ(),

			"LocalPosition.X" => transform.TransitionLocalPositionX(),
			"LocalPosition.Y" => transform.TransitionLocalPositionY(),
			"LocalPosition.Z" => transform.TransitionLocalPositionZ(),

			"Rotation.X" => transform.TransitionRotationX(),
			"Rotation.Y" => transform.TransitionRotationY(),
			"Rotation.Z" => transform.TransitionRotationZ(),
			"Rotation.W" => transform.TransitionRotationW(),

			"LocalRotation.X" => transform.TransitionLocalRotationX(),
			"LocalRotation.Y" => transform.TransitionLocalRotationY(),
			"LocalRotation.Z" => transform.TransitionLocalRotationZ(),
			"LocalRotation.W" => transform.TransitionLocalRotationW(),

			"EulerAngles.X" => transform.TransitionEulerAnglesX(),
			"EulerAngles.Y" => transform.TransitionEulerAnglesY(),
			"EulerAngles.Z" => transform.TransitionEulerAnglesZ(),

			"LocalEulerAngles.X" => transform.TransitionLocalEulerAnglesX(),
			"LocalEulerAngles.Y" => transform.TransitionLocalEulerAnglesY(),
			"LocalEulerAngles.Z" => transform.TransitionLocalEulerAnglesZ(),

			"LocalScale.X" => transform.TransitionLocalScaleX(),
			"LocalScale.Y" => transform.TransitionLocalScaleY(),
			"LocalScale.Z" => transform.TransitionLocalScaleZ(),

			_ => throw new ArgumentException("Invalid field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {transform:type} {field:info} in {transform.gameObject:ref}"),
		};
		#endregion Transform

		#region Sprite Renderer
		public static Transition TransitionSizeX(this SpriteRenderer spriteRenderer) => new Transition(() => spriteRenderer.size.x, value => spriteRenderer.size = new Vector2(value, spriteRenderer.size.y), HashCode.Combine(spriteRenderer, "Size.X"));
		public static Transition TransitionSizeY(this SpriteRenderer spriteRenderer) => new Transition(() => spriteRenderer.size.y, value => spriteRenderer.size = new Vector2(spriteRenderer.size.x, value), HashCode.Combine(spriteRenderer, "Size.Y"));

		public static Transition TransitionColorR(this SpriteRenderer spriteRenderer) => new Transition(() => spriteRenderer.color.r, value => spriteRenderer.color = new Color(value, spriteRenderer.color.g, spriteRenderer.color.b, spriteRenderer.color.a), HashCode.Combine(spriteRenderer, "Color.R"));
		public static Transition TransitionColorG(this SpriteRenderer spriteRenderer) => new Transition(() => spriteRenderer.color.g, value => spriteRenderer.color = new Color(spriteRenderer.color.r, value, spriteRenderer.color.b, spriteRenderer.color.a), HashCode.Combine(spriteRenderer, "Color.G"));
		public static Transition TransitionColorB(this SpriteRenderer spriteRenderer) => new Transition(() => spriteRenderer.color.b, value => spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, value, spriteRenderer.color.a), HashCode.Combine(spriteRenderer, "Color.B"));
		public static Transition TransitionColorA(this SpriteRenderer spriteRenderer) => new Transition(() => spriteRenderer.color.a, value => spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, value), HashCode.Combine(spriteRenderer, "Color.A"));

		public static Transition Transition(this SpriteRenderer spriteRenderer, string field) => (field) switch
		{
			"Size.X" => spriteRenderer.TransitionSizeX(),
			"Size.Y" => spriteRenderer.TransitionSizeY(),

			"Color.R" => spriteRenderer.TransitionColorR(),
			"Color.G" => spriteRenderer.TransitionColorG(),
			"Color.B" => spriteRenderer.TransitionColorB(),
			"Color.A" => spriteRenderer.TransitionColorA(),

			_ => throw new ArgumentException("Invalid field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {spriteRenderer:type} {field:info} in {spriteRenderer.gameObject:ref}"),
		};
		#endregion Sprite Renderer

#if UNITY_AUDIO
		#region Audio Source
		public static Transition TransitionTime(this AudioSource audioSource) => new Transition(() => audioSource.time, value => audioSource.time = value, HashCode.Combine(audioSource, "Time"));
		public static Transition TransitionVolume(this AudioSource audioSource) => new Transition(() => audioSource.volume, value => audioSource.volume = value, HashCode.Combine(audioSource, "Volume"));
		public static Transition TransitionPitch(this AudioSource audioSource) => new Transition(() => audioSource.pitch, value => audioSource.pitch = value, HashCode.Combine(audioSource, "Pitch"));
		public static Transition TransitionPanStereo(this AudioSource audioSource) => new Transition(() => audioSource.panStereo, value => audioSource.panStereo = value, HashCode.Combine(audioSource, "PanStereo"));

		public static Transition TransitionSpatialBlend(this AudioSource audioSource) => new Transition(() => audioSource.spatialBlend, value => audioSource.spatialBlend = value, HashCode.Combine(audioSource, "SpatialBlend"));
		public static Transition TransitionReverbZoneMix(this AudioSource audioSource) => new Transition(() => audioSource.reverbZoneMix, value => audioSource.reverbZoneMix = value, HashCode.Combine(audioSource, "ReverbZoneMix"));
		public static Transition TransitionDopplerLevel(this AudioSource audioSource) => new Transition(() => audioSource.dopplerLevel, value => audioSource.dopplerLevel = value, HashCode.Combine(audioSource, "DopplerLevel"));
		public static Transition TransitionSpread(this AudioSource audioSource) => new Transition(() => audioSource.spread, value => audioSource.spread = value, HashCode.Combine(audioSource, "Spread"));

		public static Transition TransitionMinDistance(this AudioSource audioSource) => new Transition(() => audioSource.minDistance, value => audioSource.minDistance = value, HashCode.Combine(audioSource, "MinDistance"));
		public static Transition TransitionMaxDistance(this AudioSource audioSource) => new Transition(() => audioSource.maxDistance, value => audioSource.maxDistance = value, HashCode.Combine(audioSource, "MaxDistance"));

		public static Transition Transition(this AudioSource audioSource, string field) => (field) switch
		{
			"Time" => audioSource.TransitionTime(),
			"Volume" => audioSource.TransitionVolume(),
			"Pitch" => audioSource.TransitionPitch(),
			"PanStereo" => audioSource.TransitionPanStereo(),

			"SpatialBlend" => audioSource.TransitionSpatialBlend(),
			"ReverbZoneMix" => audioSource.TransitionReverbZoneMix(),
			"DopplerLevel" => audioSource.TransitionDopplerLevel(),
			"Spread" => audioSource.TransitionSpread(),

			"MinDistance" => audioSource.TransitionMinDistance(),
			"MaxDistance" => audioSource.TransitionMaxDistance(),

			_ => throw new ArgumentException("Invalid field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {audioSource:type} {field:info} in {audioSource.gameObject:ref}"),
		};
		#endregion Audio Source
#endif

#if UNITY_UGUI
		#region Rect Transform
		public static Transition TransitionAnchoredPositionX(this RectTransform rectTransform) => new Transition(() => rectTransform.anchoredPosition.x, value => rectTransform.anchoredPosition = new Vector2(value, rectTransform.anchoredPosition.y), HashCode.Combine(rectTransform, "AnchoredPosition.X"));
		public static Transition TransitionAnchoredPositionY(this RectTransform rectTransform) => new Transition(() => rectTransform.anchoredPosition.y, value => rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, value), HashCode.Combine(rectTransform, "AnchoredPosition.Y"));

		public static Transition TransitionAnchoredPosition3DX(this RectTransform rectTransform) => new Transition(() => rectTransform.anchoredPosition3D.x, value => rectTransform.anchoredPosition3D = new Vector3(value, rectTransform.anchoredPosition3D.y, rectTransform.anchoredPosition3D.z), HashCode.Combine(rectTransform, "AnchoredPosition3D.X"));
		public static Transition TransitionAnchoredPosition3DY(this RectTransform rectTransform) => new Transition(() => rectTransform.anchoredPosition3D.y, value => rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, value, rectTransform.anchoredPosition3D.z), HashCode.Combine(rectTransform, "AnchoredPosition3D.Y"));
		public static Transition TransitionAnchoredPosition3DZ(this RectTransform rectTransform) => new Transition(() => rectTransform.anchoredPosition3D.z, value => rectTransform.anchoredPosition3D = new Vector3(rectTransform.anchoredPosition3D.x, rectTransform.anchoredPosition3D.y, value), HashCode.Combine(rectTransform, "AnchoredPosition3D.Z"));

		public static Transition TransitionSizeDeltaX(this RectTransform rectTransform) => new Transition(() => rectTransform.sizeDelta.x, value => rectTransform.sizeDelta = new Vector2(value, rectTransform.sizeDelta.y), HashCode.Combine(rectTransform, "SizeDelta.X"));
		public static Transition TransitionSizeDeltaY(this RectTransform rectTransform) => new Transition(() => rectTransform.sizeDelta.y, value => rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, value), HashCode.Combine(rectTransform, "SizeDelta.Y"));

		public static Transition TransitionAnchorMinX(this RectTransform rectTransform) => new Transition(() => rectTransform.anchorMin.x, value => rectTransform.anchorMin = new Vector2(value, rectTransform.anchorMin.y), HashCode.Combine(rectTransform, "AnchorMin.X"));
		public static Transition TransitionAnchorMinY(this RectTransform rectTransform) => new Transition(() => rectTransform.anchorMin.y, value => rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, value), HashCode.Combine(rectTransform, "AnchorMin.Y"));

		public static Transition TransitionAnchorMaxX(this RectTransform rectTransform) => new Transition(() => rectTransform.anchorMax.x, value => rectTransform.anchorMax = new Vector2(value, rectTransform.anchorMax.y), HashCode.Combine(rectTransform, "AnchorMax.X"));
		public static Transition TransitionAnchorMaxY(this RectTransform rectTransform) => new Transition(() => rectTransform.anchorMax.y, value => rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, value), HashCode.Combine(rectTransform, "AnchorMax.Y"));

		public static Transition TransitionOffsetMinX(this RectTransform rectTransform) => new Transition(() => rectTransform.offsetMin.x, value => rectTransform.offsetMin = new Vector2(value, rectTransform.offsetMin.y), HashCode.Combine(rectTransform, "OffsetMin.X"));
		public static Transition TransitionOffsetMinY(this RectTransform rectTransform) => new Transition(() => rectTransform.offsetMin.y, value => rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, value), HashCode.Combine(rectTransform, "OffsetMin.Y"));

		public static Transition TransitionOffsetMaxX(this RectTransform rectTransform) => new Transition(() => rectTransform.offsetMax.x, value => rectTransform.offsetMax = new Vector2(value, rectTransform.offsetMax.y), HashCode.Combine(rectTransform, "OffsetMax.X"));
		public static Transition TransitionOffsetMaxY(this RectTransform rectTransform) => new Transition(() => rectTransform.offsetMax.y, value => rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, value), HashCode.Combine(rectTransform, "OffsetMax.Y"));

		public static Transition TransitionPivotX(this RectTransform rectTransform) => new Transition(() => rectTransform.pivot.x, value => rectTransform.pivot = new Vector2(value, rectTransform.pivot.y), HashCode.Combine(rectTransform, "Pivot.X"));
		public static Transition TransitionPivotY(this RectTransform rectTransform) => new Transition(() => rectTransform.pivot.y, value => rectTransform.pivot = new Vector2(rectTransform.pivot.x, value), HashCode.Combine(rectTransform, "Pivot.Y"));

		public static Transition Transition(this RectTransform rectTransform, string field) => (field) switch
		{
			"AnchoredPosition.X" => rectTransform.TransitionAnchoredPositionX(),
			"AnchoredPosition.Y" => rectTransform.TransitionAnchoredPositionY(),

			"AnchoredPosition3D.X" => rectTransform.TransitionAnchoredPosition3DX(),
			"AnchoredPosition3D.Y" => rectTransform.TransitionAnchoredPosition3DY(),
			"AnchoredPosition3D.Z" => rectTransform.TransitionAnchoredPosition3DZ(),

			"SizeDelta.X" => rectTransform.TransitionSizeDeltaX(),
			"SizeDelta.Y" => rectTransform.TransitionSizeDeltaY(),

			"AnchorMin.X" => rectTransform.TransitionAnchorMinX(),
			"AnchorMin.Y" => rectTransform.TransitionAnchorMinY(),

			"AnchorMax.X" => rectTransform.TransitionAnchorMaxX(),
			"AnchorMax.Y" => rectTransform.TransitionAnchorMaxY(),

			"OffsetMin.X" => rectTransform.TransitionOffsetMinX(),
			"OffsetMin.Y" => rectTransform.TransitionOffsetMinY(),

			"OffsetMax.X" => rectTransform.TransitionOffsetMaxX(),
			"OffsetMax.Y" => rectTransform.TransitionOffsetMaxY(),

			"Pivot.X" => rectTransform.TransitionPivotX(),
			"Pivot.Y" => rectTransform.TransitionPivotY(),

			_ => throw new ArgumentException("Invalid field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {rectTransform:type} {field:info} in {rectTransform.gameObject:ref}"),
		};
		#endregion Rect Transform

		#region Canvas Group
		public static Transition TransitionAlpha(this CanvasGroup canvasGroup) => new Transition(() => canvasGroup.alpha, value => canvasGroup.alpha = value, HashCode.Combine(canvasGroup, "Alpha"));

		public static Transition Transition(this CanvasGroup canvasGroup, string field) => (field) switch

		{
			"Alpha" => canvasGroup.TransitionAlpha(),

			_ => throw new ArgumentException("Invalid field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {canvasGroup:type} {field:info} in {canvasGroup.gameObject:ref}"),
		};
		#endregion Canvas Group

		#region Graphic
		public static Transition TransitionColorR(this UnityEngine.UI.Graphic graphic) => new Transition(() => graphic.color.r, value => graphic.color = new Color(value, graphic.color.g, graphic.color.b, graphic.color.a), HashCode.Combine(graphic, "Color.R"));
		public static Transition TransitionColorG(this UnityEngine.UI.Graphic graphic) => new Transition(() => graphic.color.g, value => graphic.color = new Color(graphic.color.r, value, graphic.color.b, graphic.color.a), HashCode.Combine(graphic, "Color.G"));
		public static Transition TransitionColorB(this UnityEngine.UI.Graphic graphic) => new Transition(() => graphic.color.b, value => graphic.color = new Color(graphic.color.r, graphic.color.g, value, graphic.color.a), HashCode.Combine(graphic, "Color.B"));
		public static Transition TransitionColorA(this UnityEngine.UI.Graphic graphic) => new Transition(() => graphic.color.a, value => graphic.color = new Color(graphic.color.r, graphic.color.g, graphic.color.b, value), HashCode.Combine(graphic, "Color.A"));

		public static Transition Transition(this UnityEngine.UI.Graphic graphic, string field) => (field) switch
		{
			"Color.R" => graphic.TransitionColorR(),
			"Color.G" => graphic.TransitionColorG(),
			"Color.B" => graphic.TransitionColorB(),
			"Color.A" => graphic.TransitionColorA(),

			_ => throw new ArgumentException("Invalid field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {graphic:type} {field:info} in {graphic.gameObject:ref}"),
		};
		#endregion Graphic

		#region Shadow
		public static Transition TransitionEffectColorR(this UnityEngine.UI.Shadow shadow) => new Transition(() => shadow.effectColor.r, value => shadow.effectColor = new Color(value, shadow.effectColor.g, shadow.effectColor.b, shadow.effectColor.a), HashCode.Combine(shadow, "EffectColor.R"));
		public static Transition TransitionEffectColorG(this UnityEngine.UI.Shadow shadow) => new Transition(() => shadow.effectColor.g, value => shadow.effectColor = new Color(shadow.effectColor.r, value, shadow.effectColor.b, shadow.effectColor.a), HashCode.Combine(shadow, "EffectColor.G"));
		public static Transition TransitionEffectColorB(this UnityEngine.UI.Shadow shadow) => new Transition(() => shadow.effectColor.b, value => shadow.effectColor = new Color(shadow.effectColor.r, shadow.effectColor.g, value, shadow.effectColor.a), HashCode.Combine(shadow, "EffectColor.B"));
		public static Transition TransitionEffectColorA(this UnityEngine.UI.Shadow shadow) => new Transition(() => shadow.effectColor.a, value => shadow.effectColor = new Color(shadow.effectColor.r, shadow.effectColor.g, shadow.effectColor.b, value), HashCode.Combine(shadow, "EffectColor.A"));

		public static Transition TransitionEffectDistanceX(this UnityEngine.UI.Shadow shadow) => new Transition(() => shadow.effectDistance.x, value => shadow.effectDistance = new Vector2(value, shadow.effectDistance.y), HashCode.Combine(shadow, "EffectDistance.X"));
		public static Transition TransitionEffectDistanceY(this UnityEngine.UI.Shadow shadow) => new Transition(() => shadow.effectDistance.y, value => shadow.effectDistance = new Vector2(shadow.effectDistance.x, value), HashCode.Combine(shadow, "EffectDistance.Y"));

		public static Transition Transition(this UnityEngine.UI.Shadow shadow, string field) => (field) switch
		{
			"EffectColor.R" => shadow.TransitionEffectColorR(),
			"EffectColor.G" => shadow.TransitionEffectColorG(),
			"EffectColor.B" => shadow.TransitionEffectColorB(),
			"EffectColor.A" => shadow.TransitionEffectColorA(),

			"EffectDistance.X" => shadow.TransitionEffectDistanceX(),
			"EffectDistance.Y" => shadow.TransitionEffectDistanceY(),

			_ => throw new ArgumentException("Invalid field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {shadow:type} {field:info} in {shadow.gameObject:ref}"),
		};
		#endregion Shadow
#endif

#if UNITY_UIELEMENTS
		#region Visual Element
		public static Transition TransitionOpacity(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.opacity, value => element.style.opacity = value, HashCode.Combine(element, "Opacity"));

		// Transition Position may be incorrect: Resolved style does not return expected position values
		public static Transition TransitionPositionTop(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.top, value => element.style.top = value, HashCode.Combine(element, "Position.Top"));
		public static Transition TransitionPositionBottom(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.bottom, value => element.style.bottom = value, HashCode.Combine(element, "Position.Bottom"));
		public static Transition TransitionPositionLeft(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.left, value => element.style.left = value, HashCode.Combine(element, "Position.Left"));
		public static Transition TransitionPositionRight(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.right, value => element.style.right = value, HashCode.Combine(element, "Position.Right"));

		public static Transition TransitioMarginTop(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.marginTop, value => element.style.marginTop = value, HashCode.Combine(element, "Margin.Top"));
		public static Transition TransitioMarginBottom(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.marginBottom, value => element.style.marginBottom = value, HashCode.Combine(element, "Margin.Bottom"));
		public static Transition TransitioMarginLeft(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.marginLeft, value => element.style.marginLeft = value, HashCode.Combine(element, "Margin.Left"));
		public static Transition TransitioMarginRight(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.marginRight, value => element.style.marginRight = value, HashCode.Combine(element, "Margin.Right"));

		public static Transition TransitionPaddingTop(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.paddingTop, value => element.style.paddingTop = value, HashCode.Combine(element, "Padding.Top"));
		public static Transition TransitionPaddingBottom(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.paddingBottom, value => element.style.paddingBottom = value, HashCode.Combine(element, "Padding.Bottom"));
		public static Transition TransitionPaddingLeft(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.paddingLeft, value => element.style.paddingLeft = value, HashCode.Combine(element, "Padding.Left"));
		public static Transition TransitionPaddingRight(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.paddingRight, value => element.style.paddingRight = value, HashCode.Combine(element, "Padding.Right"));

		public static Transition TransitionWidth(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.width, value => element.style.width = value, HashCode.Combine(element, "Width"));
		public static Transition TransitionHeight(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.height, value => element.style.height = value, HashCode.Combine(element, "Height"));

		public static Transition TransitionFontSize(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.fontSize, value => element.style.fontSize = value, HashCode.Combine(element, "FontSize"));

		public static Transition TransitionColorR(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.color.r, value => element.style.color = new Color(value, element.resolvedStyle.color.g, element.resolvedStyle.color.b, element.resolvedStyle.color.a), HashCode.Combine(element, "Color.R"));
		public static Transition TransitionColorG(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.color.g, value => element.style.color = new Color(element.resolvedStyle.color.r, value, element.resolvedStyle.color.b, element.resolvedStyle.color.a), HashCode.Combine(element, "Color.G"));
		public static Transition TransitionColorB(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.color.b, value => element.style.color = new Color(element.resolvedStyle.color.r, element.resolvedStyle.color.g, value, element.resolvedStyle.color.a), HashCode.Combine(element, "Color.B"));
		public static Transition TransitionColorA(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.color.a, value => element.style.color = new Color(element.resolvedStyle.color.r, element.resolvedStyle.color.g, element.resolvedStyle.color.b, value), HashCode.Combine(element, "Color.A"));

		public static Transition TransitionBorderWidthTop(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderTopWidth, value => element.style.borderTopWidth = value, HashCode.Combine(element, "BorderWidth.Top"));
		public static Transition TransitionBorderWidthBottom(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderBottomWidth, value => element.style.borderBottomWidth = value, HashCode.Combine(element, "BorderWidth.Bottom"));
		public static Transition TransitionBorderWidthLeft(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderLeftWidth, value => element.style.borderLeftWidth = value, HashCode.Combine(element, "BorderWidth.Left"));
		public static Transition TransitionBorderWidthRight(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderRightWidth, value => element.style.borderRightWidth = value, HashCode.Combine(element, "BorderWidth.Right"));

		public static Transition TransitionBorderRadius(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderTopLeftRadius, value => SetBorderRadius(element, value), HashCode.Combine(element, "BorderRadius"));
		private static void SetBorderRadius(UnityEngine.UIElements.VisualElement element, float radius)
		{
			element.style.borderTopLeftRadius = radius;
			element.style.borderTopRightRadius = radius;
			element.style.borderBottomLeftRadius = radius;
			element.style.borderBottomRightRadius = radius;
		}

		public static Transition TransitionBorderColorR(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderTopColor.r, value => SetBorderColor(element, new Color(value, element.resolvedStyle.borderTopColor.g, element.resolvedStyle.borderTopColor.b, element.resolvedStyle.borderTopColor.a)), HashCode.Combine(element, "BorderColor.R"));
		public static Transition TransitionBorderColorG(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderTopColor.g, value => SetBorderColor(element, new Color(element.resolvedStyle.borderTopColor.r, value, element.resolvedStyle.borderTopColor.b, element.resolvedStyle.borderTopColor.a)), HashCode.Combine(element, "BorderColor.G"));
		public static Transition TransitionBorderColorB(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderTopColor.b, value => SetBorderColor(element, new Color(element.resolvedStyle.borderTopColor.r, element.resolvedStyle.borderTopColor.g, value, element.resolvedStyle.borderTopColor.a)), HashCode.Combine(element, "BorderColor.B"));
		public static Transition TransitionBorderColorA(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.borderTopColor.a, value => SetBorderColor(element, new Color(element.resolvedStyle.borderTopColor.r, element.resolvedStyle.borderTopColor.g, element.resolvedStyle.borderTopColor.b, value)), HashCode.Combine(element, "BorderColor.A"));
		private static void SetBorderColor(UnityEngine.UIElements.VisualElement element, Color color)
		{
			element.style.borderTopColor = color;
			element.style.borderBottomColor = color;
			element.style.borderLeftColor = color;
			element.style.borderRightColor = color;
		}

		public static Transition TransitionBackgroundColorR(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.backgroundColor.r, value => element.style.backgroundColor = new Color(value, element.resolvedStyle.backgroundColor.g, element.resolvedStyle.backgroundColor.b, element.resolvedStyle.backgroundColor.a), HashCode.Combine(element, "BackgroundColor.R"));
		public static Transition TransitionBackgroundColorG(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.backgroundColor.g, value => element.style.backgroundColor = new Color(element.resolvedStyle.backgroundColor.r, value, element.resolvedStyle.backgroundColor.b, element.resolvedStyle.backgroundColor.a), HashCode.Combine(element, "BackgroundColor.G"));
		public static Transition TransitionBackgroundColorB(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.backgroundColor.b, value => element.style.backgroundColor = new Color(element.resolvedStyle.backgroundColor.r, element.resolvedStyle.backgroundColor.g, value, element.resolvedStyle.backgroundColor.a), HashCode.Combine(element, "BackgroundColor.B"));
		public static Transition TransitionBackgroundColorA(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.backgroundColor.a, value => element.style.backgroundColor = new Color(element.resolvedStyle.backgroundColor.r, element.resolvedStyle.backgroundColor.g, element.resolvedStyle.backgroundColor.b, value), HashCode.Combine(element, "BackgroundColor.A"));

		public static Transition TransitionBackgroundPositionX(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.backgroundPositionX.offset.value, value => element.style.backgroundPositionX = new UnityEngine.UIElements.BackgroundPosition(UnityEngine.UIElements.BackgroundPositionKeyword.Center, value), HashCode.Combine(element, "BackgroundPosition.X"));
		public static Transition TransitionBackgroundPositionY(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.backgroundPositionY.offset.value, value => element.style.backgroundPositionY = new UnityEngine.UIElements.BackgroundPosition(UnityEngine.UIElements.BackgroundPositionKeyword.Center, value), HashCode.Combine(element, "BackgroundPosition.Y"));

		public static Transition TransitionBackgroundSizeX(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.backgroundSize.x.value, value => element.style.backgroundSize = new UnityEngine.UIElements.BackgroundSize(value, element.resolvedStyle.backgroundSize.y.value), HashCode.Combine(element, "BackgroundSize.X"));
		public static Transition TransitionBackgroundSizeY(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.backgroundSize.y.value, value => element.style.backgroundSize = new UnityEngine.UIElements.BackgroundSize(element.resolvedStyle.backgroundSize.x.value, value), HashCode.Combine(element, "BackgroundSize.Y"));

		public static Transition TransitionBackgroundTintR(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.unityBackgroundImageTintColor.r, value => element.style.unityBackgroundImageTintColor = new Color(value, element.resolvedStyle.unityBackgroundImageTintColor.g, element.resolvedStyle.unityBackgroundImageTintColor.b, element.resolvedStyle.unityBackgroundImageTintColor.a), HashCode.Combine(element, "BackgroundTint.R"));
		public static Transition TransitionBackgroundTintG(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.unityBackgroundImageTintColor.g, value => element.style.unityBackgroundImageTintColor = new Color(element.resolvedStyle.unityBackgroundImageTintColor.r, value, element.resolvedStyle.unityBackgroundImageTintColor.b, element.resolvedStyle.unityBackgroundImageTintColor.a), HashCode.Combine(element, "BackgroundTint.G"));
		public static Transition TransitionBackgroundTintB(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.unityBackgroundImageTintColor.b, value => element.style.unityBackgroundImageTintColor = new Color(element.resolvedStyle.unityBackgroundImageTintColor.r, element.resolvedStyle.unityBackgroundImageTintColor.g, value, element.resolvedStyle.unityBackgroundImageTintColor.a), HashCode.Combine(element, "BackgroundTint.B"));
		public static Transition TransitionBackgroundTintA(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.unityBackgroundImageTintColor.a, value => element.style.unityBackgroundImageTintColor = new Color(element.resolvedStyle.unityBackgroundImageTintColor.r, element.resolvedStyle.unityBackgroundImageTintColor.g, element.resolvedStyle.unityBackgroundImageTintColor.b, value), HashCode.Combine(element, "BackgroundTint.A"));

		public static Transition TransitionTranslateX(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.translate.x, value => element.style.translate = new UnityEngine.UIElements.Translate(value, element.resolvedStyle.translate.y, element.resolvedStyle.translate.z), HashCode.Combine(element, "Translate.X"));
		public static Transition TransitionTranslateY(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.translate.y, value => element.style.translate = new UnityEngine.UIElements.Translate(element.resolvedStyle.translate.x, value, element.resolvedStyle.translate.z), HashCode.Combine(element, "Translate.Y"));
		public static Transition TransitionTranslateZ(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.translate.z, value => element.style.translate = new UnityEngine.UIElements.Translate(element.resolvedStyle.translate.x, element.resolvedStyle.translate.y, value), HashCode.Combine(element, "Translate.Z"));

		public static Transition TransitionRotate(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.rotate.angle.value, value => element.style.rotate = new UnityEngine.UIElements.Rotate(value), HashCode.Combine(element, "Rotate"));

		public static Transition TransitionScaleX(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.scale.value.x, value => element.style.scale = new Vector2(value, element.resolvedStyle.scale.value.y), HashCode.Combine(element, "Scale.X"));
		public static Transition TransitionScaleY(this UnityEngine.UIElements.VisualElement element) => new Transition(() => element.resolvedStyle.scale.value.y, value => element.style.scale = new Vector2(element.resolvedStyle.scale.value.x, value), HashCode.Combine(element, "Scale.Y"));

		public static Transition Transition(this UnityEngine.UIElements.VisualElement element, string field) => (field) switch
		{
			"Opacity" => element.TransitionOpacity(),

			"Position.Top" => element.TransitionPositionTop(),
			"Position.Bottom" => element.TransitionPositionBottom(),
			"Position.Left" => element.TransitionPositionLeft(),
			"Position.Right" => element.TransitionPositionRight(),

			"Margin.Top" => element.TransitioMarginTop(),
			"Margin.Bottom" => element.TransitioMarginBottom(),
			"Margin.Left" => element.TransitioMarginLeft(),
			"Margin.Right" => element.TransitioMarginRight(),

			"Padding.Top" => element.TransitionPaddingTop(),
			"Padding.Bottom" => element.TransitionPaddingBottom(),
			"Padding.Left" => element.TransitionPaddingLeft(),
			"Padding.Right" => element.TransitionPaddingRight(),

			"Width" => element.TransitionWidth(),
			"Height" => element.TransitionHeight(),

			"FontSize" => element.TransitionFontSize(),

			"Color.R" => element.TransitionColorR(),
			"Color.G" => element.TransitionColorG(),
			"Color.B" => element.TransitionColorB(),
			"Color.A" => element.TransitionColorA(),

			"BorderWidth.Top" => element.TransitionBorderWidthTop(),
			"BorderWidth.Bottom" => element.TransitionBorderWidthBottom(),
			"BorderWidth.Left" => element.TransitionBorderWidthLeft(),
			"BorderWidth.Right" => element.TransitionBorderWidthRight(),

			"BorderRadius" => element.TransitionBorderRadius(),

			"BorderColor.R" => element.TransitionBorderColorR(),
			"BorderColor.G" => element.TransitionBorderColorG(),
			"BorderColor.B" => element.TransitionBorderColorB(),
			"BorderColor.A" => element.TransitionBorderColorA(),

			"BackgroundColor.R" => element.TransitionBackgroundColorR(),
			"BackgroundColor.G" => element.TransitionBackgroundColorG(),
			"BackgroundColor.B" => element.TransitionBackgroundColorB(),
			"BackgroundColor.A" => element.TransitionBackgroundColorA(),

			"BackgroundPosition.X" => element.TransitionBackgroundPositionX(),
			"BackgroundPosition.Y" => element.TransitionBackgroundPositionY(),

			"BackgroundSize.X" => element.TransitionBackgroundSizeX(),
			"BackgroundSize.Y" => element.TransitionBackgroundSizeY(),

			"BackgroundTint.R" => element.TransitionBackgroundTintR(),
			"BackgroundTint.G" => element.TransitionBackgroundTintG(),
			"BackgroundTint.B" => element.TransitionBackgroundTintB(),
			"BackgroundTint.A" => element.TransitionBackgroundTintA(),

			"Translate.X" => element.TransitionTranslateX(),
			"Translate.Y" => element.TransitionTranslateY(),
			"Translate.Z" => element.TransitionTranslateZ(),

			"Rotate" => element.TransitionRotate(),

			"Scale.X" => element.TransitionScaleX(),
			"Scale.Y" => element.TransitionScaleY(),

			_ => throw new ArgumentException("Invalid field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {element:type} {field:info} in {element:ref}"),
		};
		#endregion Visual Element
#endif


		public static Transition Transition(this GameObject gameObject, string field)
		{
			int index = field.IndexOf('.');
			if (field.OutOfRange(index)) throw new ArgumentException("Period splitter out of range").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {field:info} in {gameObject:ref}");

			string component = field[..(index - 1)];
			string address = field[(index + 1)..];

			return (address) switch
			{
				"Transform" => gameObject.GetComponent<Transform>().Transition(address),
				"SpriteRenderer" => gameObject.GetComponent<SpriteRenderer>().Transition(address),
#if UNITY_AUDIO
				"AudioSource" => gameObject.GetComponent<AudioSource>().Transition(address),
#endif
#if UNITY_UGUI

				"RectTransform" => gameObject.GetComponent<RectTransform>().Transition(address),
				"CanvasGroup" => gameObject.GetComponent<CanvasGroup>().Transition(address),
				"Graphic" => gameObject.GetComponent<UnityEngine.UI.Graphic>().Transition(address),
				"Shadow" => gameObject.GetComponent<UnityEngine.UI.Shadow>().Transition(address),
#endif
				_ => throw new ArgumentException("Invalid component field address").Overwrite(ConsoleUtilities.transitionTag, $"Failed transitioning {field:info} in {gameObject:ref}"),
			};
		}
	}
}