using Godot;
using System;
using System.Collections.Generic;

namespace sovereignbladetracker
{
	/// <summary>
	/// 각 칼날 VFX 위에 단조 수치를 표시하는 오버레이.
	/// 파일 스캔은 Patch 쪽의 GetBladeData 델리게이트가 담당한다.
	/// </summary>
	public partial class BladeCounterOverlay : Control
	{
		// (spineNode, damage) 목록을 반환하는 델리게이트 — Patch에서 설정
		public Func<List<(Node2D? spine, int damage)>?>? GetBladeData;

		private readonly Dictionary<Node2D, Label> _labels = new();
		private Font? _font;
		private bool  _counterEnabled = true;
		private int   _fontSize       = 50;

		public void SetCounterEnabled(bool enabled) => _counterEnabled = enabled;

		public void SetFontSize(int size)
		{
			_fontSize = size;
			foreach (var lbl in _labels.Values)
				if (GodotObject.IsInstanceValid(lbl))
					lbl.AddThemeFontSizeOverride("font_size", size);
		}

		public override void _Ready()
		{
			try { _font = ResourceLoader.Load<Font>("res://themes/kreon_bold_shared.tres"); }
			catch { _font = null; }
			MouseFilter = MouseFilterEnum.Ignore;
			GD.Print("[BladeCounterOverlay] _Ready");
		}

		public override void _Process(double delta)
		{
			if (!Visible || !_counterEnabled || GetBladeData == null)
			{
				foreach (var lbl in _labels.Values)
					if (GodotObject.IsInstanceValid(lbl)) lbl.Visible = false;
				return;
			}

			var bladeData = GetBladeData();
			if (bladeData == null) return;

			var current = new HashSet<Node2D>();

			foreach (var (spine, damage) in bladeData)
			{
				if (spine == null || !GodotObject.IsInstanceValid(spine)) continue;
				current.Add(spine);

				if (!_labels.TryGetValue(spine, out var label))
				{
					label = new Label
					{
						HorizontalAlignment = HorizontalAlignment.Center,
						MouseFilter         = MouseFilterEnum.Ignore,
						ZIndex              = 0,
						Size                = new Vector2(50, 30)
					};
					label.AddThemeFontSizeOverride("font_size", _fontSize);
					if (_font != null) label.AddThemeFontOverride("font", _font);
					label.AddThemeColorOverride("font_color",         new Color(1f, 1f, 1f));
					label.AddThemeColorOverride("font_outline_color", new Color(0.05f, 0.15f, 0.8f, 1.0f));
					label.AddThemeConstantOverride("outline_size", 4);
					AddChild(label);
					_labels[spine] = label;
					GD.Print($"[BladeCounterOverlay] Label created for spine={spine.Name}, damage={damage}");
				}

				label.Text    = damage.ToString();
				label.Visible = true;

				// 월드 좌표 → 스크린 좌표 변환
				var screenPos        = spine.GetViewportTransform() * spine.GlobalPosition;
				label.GlobalPosition = screenPos - new Vector2(25f, 15f);
			}

			// 사라진 칼날 라벨 제거
			var toRemove = new List<Node2D>();
			foreach (var kv in _labels)
			{
				if (!current.Contains(kv.Key) || !GodotObject.IsInstanceValid(kv.Key))
				{
					if (GodotObject.IsInstanceValid(kv.Value)) kv.Value.QueueFree();
					toRemove.Add(kv.Key);
				}
			}
			foreach (var key in toRemove) _labels.Remove(key);
		}
	}
}
