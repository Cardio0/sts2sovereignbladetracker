using Godot;

namespace sovereignbladetracker
{
	public partial class ModSettingsPanelNode : MarginContainer
	{
		private static readonly Color White = new Color(1f, 1f, 1f);

		private SpinBox?     _panelXInput;
		private SpinBox?     _panelYInput;
		private SpinBox?     _bladeFontSizeInput;
		private CheckButton? _showPanelCheck;
		private CheckButton? _counterOnBladeCheck;
		private CheckButton? _draggableCheck;
		private CheckButton? _rememberCheck;

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

			// Panel X [spinbox] Panel Y [spinbox] 한 행
			var posRow = new HBoxContainer();
			posRow.AddThemeConstantOverride("separation", 8);

			var labelX = new Label { Text = "Panel X", CustomMinimumSize = new Vector2(60, 0) };
			labelX.AddThemeColorOverride("font_color", White);
			_panelXInput = new SpinBox { MinValue = 0, MaxValue = 3840, Value = settings.PanelX, Step = 1 };
			_panelXInput.SizeFlagsHorizontal = SizeFlags.ExpandFill;

			var labelY = new Label { Text = "Panel Y", CustomMinimumSize = new Vector2(60, 0) };
			labelY.AddThemeColorOverride("font_color", White);
			_panelYInput = new SpinBox { MinValue = 0, MaxValue = 2160, Value = settings.PanelY, Step = 1 };
			_panelYInput.SizeFlagsHorizontal = SizeFlags.ExpandFill;

			posRow.AddChild(labelX);
			posRow.AddChild(_panelXInput);
			posRow.AddChild(labelY);
			posRow.AddChild(_panelYInput);
			vbox.AddChild(posRow);

			// Blade Font Size 행
			vbox.AddChild(MakeRow("Blade Font Size", out _bladeFontSizeInput, 8, 200, settings.BladeFontSize));

			// 표시 관련 토글 행: [Show Panel] [Counter on Blade]
			var displayRow = new HBoxContainer();
			displayRow.AddThemeConstantOverride("separation", 16);
			_showPanelCheck      = new CheckButton { Text = "Show Panel",       ButtonPressed = settings.ShowPanel };
			_counterOnBladeCheck = new CheckButton { Text = "Counter on Blade", ButtonPressed = settings.CounterOnBlade };
			displayRow.AddChild(_showPanelCheck);
			displayRow.AddChild(_counterOnBladeCheck);
			vbox.AddChild(displayRow);

			// 패널 동작 관련 토글 행: [Draggable] [Remember Position]
			var behaviorRow = new HBoxContainer();
			behaviorRow.AddThemeConstantOverride("separation", 16);
			_draggableCheck = new CheckButton { Text = "Draggable",         ButtonPressed = settings.Draggable };
			_rememberCheck  = new CheckButton { Text = "Remember Position", ButtonPressed = settings.RememberPosition };
			behaviorRow.AddChild(_draggableCheck);
			behaviorRow.AddChild(_rememberCheck);
			vbox.AddChild(behaviorRow);

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

			// SpinBox 하단 선 흰색으로 변경 (AddChild 이후 적용)
			Callable.From(() =>
			{
				ApplySpinBoxStyle(_panelXInput);
				ApplySpinBoxStyle(_panelYInput);
				ApplySpinBoxStyle(_bladeFontSizeInput);
			}).CallDeferred();
		}

		private static void ApplySpinBoxStyle(SpinBox? spinBox)
		{
			if (spinBox == null) return;
			var lineEdit = spinBox.GetLineEdit();
			if (lineEdit == null) return;

			var style = new StyleBoxFlat
			{
				BgColor          = new Color(0f, 0f, 0f, 0f), // 투명 배경
				BorderColor      = new Color(1f, 1f, 1f),      // 흰색 선
				BorderWidthBottom = 1,
				BorderWidthLeft   = 0,
				BorderWidthRight  = 0,
				BorderWidthTop    = 0,
				ContentMarginLeft   = 4,
				ContentMarginRight  = 4,
				ContentMarginTop    = 2,
				ContentMarginBottom = 2,
			};
			lineEdit.AddThemeStyleboxOverride("normal", style);
			lineEdit.AddThemeStyleboxOverride("focus",  style);
			lineEdit.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
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
				_showPanelCheck == null || _counterOnBladeCheck == null ||
				_draggableCheck == null || _rememberCheck == null)
				return;

			var settings = ModSettings.Load();
			settings.PanelX           = (int)_panelXInput.Value;
			settings.PanelY           = (int)_panelYInput.Value;
			settings.BladeFontSize    = (int)_bladeFontSizeInput.Value;
			settings.ShowPanel        = _showPanelCheck.ButtonPressed;
			settings.CounterOnBlade   = _counterOnBladeCheck.ButtonPressed;
			settings.Draggable        = _draggableCheck.ButtonPressed;
			settings.RememberPosition = _rememberCheck.ButtonPressed;
			settings.Save();
			SovereignBladeInjectionPatch.ApplySettings(settings);
		}

		private void OnResetToDefaults()
		{
			var defaults = new ModSettings();
			defaults.Save();
			SovereignBladeInjectionPatch.ApplySettings(defaults);
			Refresh(defaults);
		}

		public void Refresh() => Refresh(ModSettings.Load());

		private void Refresh(ModSettings settings)
		{
			if (_panelXInput == null || _panelYInput == null || _bladeFontSizeInput == null ||
				_showPanelCheck == null || _counterOnBladeCheck == null ||
				_draggableCheck == null || _rememberCheck == null)
				return;

			_panelXInput.Value                 = settings.PanelX;
			_panelYInput.Value                 = settings.PanelY;
			_bladeFontSizeInput.Value          = settings.BladeFontSize;
			_showPanelCheck.ButtonPressed      = settings.ShowPanel;
			_counterOnBladeCheck.ButtonPressed = settings.CounterOnBlade;
			_draggableCheck.ButtonPressed      = settings.Draggable;
			_rememberCheck.ButtonPressed       = settings.RememberPosition;
		}
	}
}
