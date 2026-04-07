using Godot;
using System;

namespace sovereignbladetracker
{
	public partial class SovereignBladePanel : Control
	{
		private TextureRect? _backgroundRect;
		private Label? _damageLabel;
		private Font? _font;

		private bool _isDragging = false;
		private Vector2 _dragOffset = Vector2.Zero;
		private Vector2? _customPosition = null;
		private Vector2 _defaultPosition = new Vector2(80, 80);
		private bool _draggable = true;

		private decimal _currentDamage = 10m;

		public override void _Ready()
		{
			try
			{
				var tex = ResourceLoader.Load<Texture2D>("res://SovereignBlade.png");
				_backgroundRect = new TextureRect
				{
					Texture = tex,
					ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
					StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
					MouseFilter = MouseFilterEnum.Pass
				};
				_backgroundRect.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
				AddChild(_backgroundRect);
			}
			catch (Exception ex)
			{
				GD.PrintErr($"[SovereignBladePanel] Failed to load texture: {ex.Message}");
			}

			try
			{
				_font = ResourceLoader.Load<Font>("res://fonts/kreon_regular.ttf");
			}
			catch
			{
				_font = null;
			}

			_damageLabel = new Label
			{
				Text = "10",
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				MouseFilter = MouseFilterEnum.Ignore
			};
			// 회색 원 위치: 이미지 기준 가로 중앙, 세로 ~75-80% 지점
			// 400x400 패널 기준으로 원 중심 ≈ (200, 310), 반지름 ≈ 60px
			// 수치 조정이 필요하면 Position과 Size를 변경
			_damageLabel.AnchorLeft = 0f;
			_damageLabel.AnchorTop = 0f;
			_damageLabel.AnchorRight = 0f;
			_damageLabel.AnchorBottom = 0f;
			_damageLabel.Position = new Vector2(68, 188);
			_damageLabel.Size = new Vector2(240, 120);
			_damageLabel.AddThemeFontSizeOverride("font_size", 36);
			if (_font != null)
				_damageLabel.AddThemeFontOverride("font", _font);
			_damageLabel.AddThemeColorOverride("font_color", new Color(0f, 0f, 0f));
			_damageLabel.AddThemeColorOverride("font_shadow_color", new Color(1f, 1f, 1f, 0.8f));
			_damageLabel.AddThemeConstantOverride("shadow_offset_x", 2);
			_damageLabel.AddThemeConstantOverride("shadow_offset_y", 2);
			AddChild(_damageLabel);

			CustomMinimumSize = new Vector2(400, 400);
			Size = new Vector2(400, 400);
			Position = _defaultPosition;

			// _Ready() 마지막에 적용 — SetDraggable이 AddChild 전에 호출됐어도 올바르게 반영됨
			MouseFilter = _draggable ? MouseFilterEnum.Stop : MouseFilterEnum.Pass;
		}

		public void SetDamage(decimal damage)
		{
			_currentDamage = damage;
			if (_damageLabel != null)
				_damageLabel.Text = ((int)damage).ToString();
		}

		public void SetDefaultPosition(Vector2 pos)
		{
			_defaultPosition = pos;
			if (!_customPosition.HasValue)
				Position = pos;
		}

		public void SetCustomPosition(Vector2 pos)
		{
			_customPosition = pos;
			Position = pos;
		}

		public Vector2? GetCustomPosition() => _customPosition;

		public void SetDraggable(bool draggable)
		{
			_draggable = draggable;
			MouseFilter = draggable ? MouseFilterEnum.Stop : MouseFilterEnum.Pass;
		}

		public override void _GuiInput(InputEvent @event)
		{
			if (!_draggable)
				return;

			if (@event is InputEventMouseButton mb)
			{
				if (mb.ButtonIndex == MouseButton.Left)
				{
					if (mb.Pressed)
					{
						_isDragging = true;
						_dragOffset = GetGlobalMousePosition() - GlobalPosition;
					}
					else
					{
						_isDragging = false;
						_customPosition = Position;
						SovereignBladeInjectionPatch.SaveCustomPosition(Position);
					}
				}
				else if (mb.ButtonIndex == MouseButton.Right && mb.Pressed)
				{
					_customPosition = null;
					Position = _defaultPosition;
					SovereignBladeInjectionPatch.ClearCustomPosition();
				}
			}
			else if (@event is InputEventMouseMotion && _isDragging)
			{
				Position = GetGlobalMousePosition() - _dragOffset;
			}
		}
	}
}
