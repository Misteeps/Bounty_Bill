using System;
using System.Collections.Generic;

using UnityEngine;

using Simplex;
using Simplex.UI;


namespace Game.UI
{
	public class Settings : Layer<Settings>
	{
		public Settings()
		{
			Div panel = this.Attach(new Div() { Name = "panel" });
			Div header = panel.Attach(new Div() { Name = "header", Classes = "row" });
			panel.Attach(new VerticalSpace(Size.Huge));
			panel.Attach(new VerticalSpace(Size.Huge));
			Div top = panel.Attach(new Div() { Name = "top", Classes = "row", Flexible = true });
			Div bottom = panel.Attach(new Div() { Name = "bottom", Classes = "row", Flexible = true });

			header.Attach(new Label() { Name = "title", Text = "Settings", Size = Size.Huge, Flexible = true });
			header.Attach(new Button() { Name = "close", Text = "X" }).Bind(_ => Hide());

			Div general = top.Attach(new Div() { Name = "general", Classes = "section", Flexible = true });
			general.AttachField(Game.Settings.customCursor, new ToggleSlide());
			general.AttachField(Game.Settings.cameraEffects, new ToggleSlide());
			general.AttachField(Game.Settings.bulletWarnings, new ToggleSlide());
			top.Attach(new HorizontalSpace(Size.Huge));
			top.Attach(new HorizontalSpace(Size.Huge));
			Div graphics = top.Attach(new Div() { Name = "graphics", Classes = "section", Flexible = true });
			graphics.AttachField(Game.Settings.windowMode, new Dropdown<FullScreenMode>().BindDirectory(Game.Settings.WindowModes));
			graphics.AttachField(Game.Settings.resolution, new Dropdown<(int, int)>().BindDirectory(Game.Settings.Resolutions));
			graphics.Attach(new VerticalSpace());
			graphics.AttachField(Game.Settings.fpsLimit, new IntInputSlider() { SliderMin = 30, SliderMax = 301, InputOverrides = new Dictionary<int, string>() { { 301, "\u221E" } } });
			graphics.AttachField(Game.Settings.fpsCounter, new ToggleSlide());
			graphics.AttachField(Game.Settings.vSync, new ToggleSlide());

			Div audio = bottom.Attach(new Div() { Name = "audio", Classes = "section", Flexible = true });
			audio.AttachField(Game.Settings.masterVolume, new IntInputSlider() { Min = 0, Max = 100 });
			audio.Attach(new VerticalSpace());
			audio.AttachField(Game.Settings.uiVolume, new IntInputSlider() { Min = 0, Max = 100 });
			audio.AttachField(Game.Settings.sfxVolume, new IntInputSlider() { Min = 0, Max = 100 });
			audio.AttachField(Game.Settings.voiceVolume, new IntInputSlider() { Min = 0, Max = 100 });
			audio.AttachField(Game.Settings.ambianceVolume, new IntInputSlider() { Min = 0, Max = 100 });
			audio.AttachField(Game.Settings.musicVolume, new IntInputSlider() { Min = 0, Max = 100 });
			bottom.Attach(new HorizontalSpace(Size.Huge));
			bottom.Attach(new HorizontalSpace(Size.Huge));
			Div keybinds = bottom.Attach(new Div() { Name = "keybinds", Classes = "section", Flexible = true });
			keybinds.AttachField(Game.Settings.shoot, new KeyBindGroup() { LockPrimary = true });
			keybinds.AttachField(Game.Settings.dodge, new KeyBindGroup());
			keybinds.AttachField(Game.Settings.moveUp, new KeyBindGroup());
			keybinds.AttachField(Game.Settings.moveDown, new KeyBindGroup());
			keybinds.AttachField(Game.Settings.moveLeft, new KeyBindGroup());
			keybinds.AttachField(Game.Settings.moveRight, new KeyBindGroup());
			keybinds.AttachField(Game.Settings.escape, new KeyBindGroup() { LockPrimary = true });

			Hidden += () => Monolith.Paused = false;
		}
	}
}