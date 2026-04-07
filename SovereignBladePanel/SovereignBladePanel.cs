using Godot;
using System;

namespace sovereignbladetracker
{
	public partial class SovereignBladePanel : Control
	{
		private TextureRect? _backgroundRect;
		private RichTextLabel? _damageLabel;
		private Font? _font;

		private bool _isDragging = false;
		private Vector2 _dragOffset = Vector2.Zero;
		private Vector2? _customPosition = null;
		private Vector2 _defaultPosition = new Vector2(80, 80);
		private bool _draggable = true;

		private string _lastDisplayText = "";

		// 매 프레임 호출되는 표시 문자열 공급자 — 패치 쪽에서 설정
		// 반환 형식: "36/27/19" (활성) / "(45)/36/19" (소멸 포함) / null (갱신 없음)
		public Func<string?>? GetDisplayText;

		public override void _Process(double delta)
		{
			if (GetDisplayText == null || !Visible) return;
			var text = GetDisplayText();
			if (text != null && text != _lastDisplayText)
			{
				_lastDisplayText = text;
				if (_damageLabel != null)
					_damageLabel.Text = text;  // BbcodeEnabled = true 이므로 BBCode 파싱됨
			}
		}

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
				// 에너지 UI와 동일한 Kreon Bold 폰트 (게임 PCK에 포함)
				_font = ResourceLoader.Load<Font>("res://themes/kreon_bold_shared.tres");
			}
			catch
			{
				_font = null;
			}

			_damageLabel = new RichTextLabel
			{
				BbcodeEnabled = true,
				Text = "[center]10[/center]",
				FitContent = true,
				ScrollActive = false,
				MouseFilter = MouseFilterEnum.Ignore
			};
			_damageLabel.AnchorLeft = 0f;
			_damageLabel.AnchorTop = 0f;
			_damageLabel.AnchorRight = 0f;
			_damageLabel.AnchorBottom = 0f;
			// ── 라벨 위치/크기 조정 시 이 두 줄을 수정 ──────────────────────
			_damageLabel.Position = new Vector2(10, 96);
			_damageLabel.Size = new Vector2(180, 60);
			// ────────────────────────────────────────────────────────────
			_damageLabel.AddThemeFontSizeOverride("normal_font_size", 30);
			if (_font != null)
				_damageLabel.AddThemeFontOverride("normal_font", _font);
			// 활성 카드: 흰 글자 / 소멸 카드: 회색(BBCode로 처리) / 파란 테두리
			_damageLabel.AddThemeColorOverride("default_color", new Color(1f, 1f, 1f));
			_damageLabel.AddThemeColorOverride("font_outline_color", new Color(0.05f, 0.15f, 0.8f, 1.0f));
			_damageLabel.AddThemeConstantOverride("outline_size", 4);
			AddChild(_damageLabel);

			// ── 패널 크기 조정 시 이 두 줄을 수정 ────────────────────────
			CustomMinimumSize = new Vector2(200, 200);
			Size = new Vector2(200, 200);
			// ────────────────────────────────────────────────────────────
			Position = _defaultPosition;

			// _Ready() 마지막에 적용 — SetDraggable이 AddChild 전에 호출됐어도 올바르게 반영됨
			MouseFilter = _draggable ? MouseFilterEnum.Stop : MouseFilterEnum.Pass;
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
