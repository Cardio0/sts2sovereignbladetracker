using Godot;

namespace sovereignbladetracker
{
	public partial class ModSettingsPanelNode : MarginContainer
	{
		private static readonly Color White = new Color(1f, 1f, 1f);

		private SpinBox?     _panelXInput;
		private SpinBox?     _panelYInput;
		private SpinBox?     _bladeFontSizeInput;
		private CheckButton? _draggableCheck;
		private CheckButton? _rememberCheck;
		private CheckButton? _counterOnBladeCheck;

		public override void _Ready()
		{
			AddThemeConstantOverride("margin_left",   22);
			AddThemeConstantOverride("margin_right",  22);
			AddThemeConstantOverride("margin_top",    12);
			AddThemeConstantOverride("margin_bottom", 12);

			var vbox = new VBoxContainer();
			vbox.AddThemeConstantOverride("separation", 10);
			AddChild(vbox);

			var settings = ModSettings.Load();

			vbox.AddChild(MakeRow("Panel X",         out _panelXInput,        0,  3840, settings.PanelX));
			vbox.AddChild(MakeRow("Panel Y",         out _panelYInput,        0,  2160, settings.PanelY));
			vbox.AddChild(MakeRow("Blade Font Size", out _bladeFontSizeInput, 8,   200, settings.BladeFontSize));

			// 토글 3개 가로 배치
			var toggleRow = new HBoxContainer();
			toggleRow.AddThemeConstantOverride("separation", 16);
			_draggableCheck      = new CheckButton { Text = "Draggable",          ButtonPressed = settings.Draggable };
			_rememberCheck       = new CheckButton { Text = "Remember Position",  ButtonPressed = settings.RememberPosition };
			_counterOnBladeCheck = new CheckButton { Text = "Counter on Blade",   ButtonPressed = settings.CounterOnBlade };
			toggleRow.AddChild(_draggableCheck);
			toggleRow.AddChild(_rememberCheck);
			toggleRow.AddChild(_counterOnBladeCheck);
			vbox.AddChild(toggleRow);

			var btnRow = new HBoxContainer();
			btnRow.AddThemeConstantOverride("separation", 8);

			var applyBtn = new Button { Text = "Apply" };
			applyBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			applyBtn.Pressed += OnApply;
			btnRow.AddChild(applyBtn);

			var resetBtn = new Button { Text = "Reset to Defaults" };
			resetBtn.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			resetBtn.Pressed += OnResetToDefaults;
			btnRow.AddChild(resetBtn);

			vbox.AddChild(btnRow);
		}

		private static HBoxContainer MakeRow(string labelText, out SpinBox spinBox, int min, int max, int value)
		{
			var row   = new HBoxContainer();
			var label = new Label { Text = labelText, CustomMinimumSize = new Vector2(120, 0) };
			label.AddThemeColorOverride("font_color", White);
			row.AddChild(label);
			spinBox = new SpinBox { MinValue = min, MaxValue = max, Value = value, Step = 1 };
			spinBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			row.AddChild(spinBox);
			return row;
		}

		private void OnApply()
		{
			if (_panelXInput == null || _panelYInput == null || _bladeFontSizeInput == null ||
				_draggableCheck == null || _rememberCheck == null || _counterOnBladeCheck == null)
				return;

			var settings = ModSettings.Load();
			settings.PanelX           = (int)_panelXInput.Value;
			settings.PanelY           = (int)_panelYInput.Value;
			settings.BladeFontSize    = (int)_bladeFontSizeInput.Value;
			settings.Draggable        = _draggableCheck.ButtonPressed;
			settings.RememberPosition = _rememberCheck.ButtonPressed;
			settings.CounterOnBlade   = _counterOnBladeCheck.ButtonPressed;
			settings.Save();
			SovereignBladeInjectionPatch.ApplySettings(settings);
		}

		private void OnResetToDefaults()
		{
			var defaults = new ModSettings(); // 코드 기본값으로 초기화
			defaults.Save();
			SovereignBladeInjectionPatch.ApplySettings(defaults);
			Refresh(defaults);
		}

		public void Refresh() => Refresh(ModSettings.Load());

		private void Refresh(ModSettings settings)
		{
			if (_panelXInput == null || _panelYInput == null || _bladeFontSizeInput == null ||
				_draggableCheck == null || _rememberCheck == null || _counterOnBladeCheck == null)
				return;

			_panelXInput.Value             = settings.PanelX;
			_panelYInput.Value             = settings.PanelY;
			_bladeFontSizeInput.Value      = settings.BladeFontSize;
			_draggableCheck.ButtonPressed  = settings.Draggable;
			_rememberCheck.ButtonPressed   = settings.RememberPosition;
			_counterOnBladeCheck.ButtonPressed = settings.CounterOnBlade;
		}
	}
}
