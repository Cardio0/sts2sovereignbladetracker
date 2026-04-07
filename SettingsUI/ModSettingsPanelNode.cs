using Godot;

namespace sovereignbladetracker
{
	public partial class ModSettingsPanelNode : MarginContainer
	{
		private static readonly Color White = new Color(1f, 1f, 1f);

		private SpinBox?     _panelXInput;
		private SpinBox?     _panelYInput;
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

			vbox.AddChild(MakeRow("Panel X", out _panelXInput, 0, 3840, settings.PanelX));
			vbox.AddChild(MakeRow("Panel Y", out _panelYInput, 0, 2160, settings.PanelY));

			_draggableCheck = new CheckButton { Text = "Draggable", ButtonPressed = settings.Draggable };
			vbox.AddChild(_draggableCheck);

			_rememberCheck = new CheckButton { Text = "Remember Position", ButtonPressed = settings.RememberPosition };
			vbox.AddChild(_rememberCheck);

			var applyBtn = new Button { Text = "Apply" };
			applyBtn.Pressed += OnApply;
			vbox.AddChild(applyBtn);
		}

		private static HBoxContainer MakeRow(string labelText, out SpinBox spinBox, int min, int max, int value)
		{
			var row = new HBoxContainer();
			var label = new Label { Text = labelText, CustomMinimumSize = new Vector2(90, 0) };
			label.AddThemeColorOverride("font_color", White);
			row.AddChild(label);
			spinBox = new SpinBox { MinValue = min, MaxValue = max, Value = value, Step = 1 };
			spinBox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
			row.AddChild(spinBox);
			return row;
		}

		private void OnApply()
		{
			if (_panelXInput == null || _panelYInput == null || _draggableCheck == null || _rememberCheck == null)
				return;

			var settings = ModSettings.Load();
			settings.PanelX          = (int)_panelXInput.Value;
			settings.PanelY          = (int)_panelYInput.Value;
			settings.Draggable       = _draggableCheck.ButtonPressed;
			settings.RememberPosition = _rememberCheck.ButtonPressed;
			settings.Save();
			SovereignBladeInjectionPatch.ApplySettings(settings);
		}

		public void Refresh()
		{
			if (_panelXInput == null || _panelYInput == null || _draggableCheck == null || _rememberCheck == null)
				return;

			var settings = ModSettings.Load();
			_panelXInput.Value            = settings.PanelX;
			_panelYInput.Value            = settings.PanelY;
			_draggableCheck.ButtonPressed  = settings.Draggable;
			_rememberCheck.ButtonPressed   = settings.RememberPosition;
		}
	}
}
