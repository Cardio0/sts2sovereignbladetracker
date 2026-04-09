using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace sovereignbladetracker
{
	public class ModSettings
	{
		[JsonPropertyName("panelX")]
		public int PanelX { get; set; } = 177;

		[JsonPropertyName("panelY")]
		public int PanelY { get; set; } = 845;

		[JsonPropertyName("draggable")]
		public bool Draggable { get; set; } = true;

		[JsonPropertyName("rememberPosition")]
		public bool RememberPosition { get; set; } = true;

		[JsonPropertyName("showPanel")]
		public bool ShowPanel { get; set; } = true;

		[JsonPropertyName("counterOnBlade")]
		public bool CounterOnBlade { get; set; } = true;

		[JsonPropertyName("bladeFontSize")]
		public int BladeFontSize { get; set; } = 50;

		private static readonly string ConfigPath = Path.Combine(
			System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData),
			"SlaytheSpire2",
			"SovereignBladeTracker.config.json"
		);

		public static ModSettings Load()
		{
			try
			{
				if (File.Exists(ConfigPath))
				{
					string json = File.ReadAllText(ConfigPath);
					var loaded = JsonSerializer.Deserialize<ModSettings>(json);
					if (loaded != null)
						return loaded;
				}
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[ModSettings] Failed to load: {ex.Message}");
			}
			return new ModSettings();
		}

		public void Save()
		{
			try
			{
				Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
				string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
				File.WriteAllText(ConfigPath, json);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[ModSettings] Failed to save: {ex.Message}");
			}
		}

		// ── 설정 UI ──────────────────────────────────────────────────────────

		private static ModSettingsPanelNode? _currentPanel = null;

		public static void RefreshForSelection(object infoContainer, object mod)
		{
			try
			{
				var container = (Node)infoContainer;

				if (!IsThisMod(mod))
				{
					// Game's Fill() already restored the default nodes; just hide our panel.
					if (_currentPanel != null && GodotObject.IsInstanceValid(_currentPanel) && _currentPanel.IsInsideTree())
						_currentPanel.Visible = false;
					return;
				}

				// Our mod: game's Fill() already showed title / image / description.
				// Just add our settings panel below ModDescription.

				if (_currentPanel == null || !GodotObject.IsInstanceValid(_currentPanel) || !_currentPanel.IsInsideTree())
				{
					_currentPanel = new ModSettingsPanelNode();
					_currentPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
					_currentPanel.SizeFlagsVertical   = Control.SizeFlags.ShrinkBegin;
					container.AddChild(_currentPanel);
				}
				else
				{
					_currentPanel.Refresh();
					_currentPanel.Visible = true;
				}

				// Defer positioning so the container layout is fully computed first.
				var panelRef = _currentPanel;
				Callable.From(() =>
				{
					try
					{
						if (panelRef == null || !GodotObject.IsInstanceValid(panelRef)) return;
						var desc = container.GetNodeOrNull("ModDescription") as Control;
						if (desc != null)
						{
							// ── 조정 가능한 오프셋 ────────────────────────────────────────
							// desc.Position.Y = 이미지 아래(Author 시작점)
							// 이 값을 올리면 패널이 내려가고, 낮추면 올라감
							const float settingsPanelYOffset = 180f;
							// ────────────────────────────────────────────────────────────
							panelRef.Position = new Vector2(0f, desc.Position.Y + settingsPanelYOffset);
							GD.Print($"[ModSettings] desc.Position.Y={desc.Position.Y}  panel set to Y={desc.Position.Y + settingsPanelYOffset}");
						}
					}
					catch { }
				}).CallDeferred();
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[ModSettings] RefreshForSelection failed: {ex.Message}");
				GD.PrintErr($"[ModSettings] Stack trace: {ex.StackTrace}");
			}
		}

		private static bool IsThisMod(object mod)
		{
			if (mod == null) return false;
			try
			{
				var field = mod.GetType().GetField("assembly",
					BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
				if (field != null)
				{
					string val = field.GetValue(mod)?.ToString() ?? "";
					if (val.Contains("SovereignBladeTracker"))
						return true;
				}
			}
			catch { }
			return false;
		}

	}
}
