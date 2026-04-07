using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;

namespace sovereignbladetracker
{
	[HarmonyPatch]
	public static class SovereignBladeInjectionPatch
	{
		internal static SovereignBladePanel? _panel;
		internal static Vector2? _savedCustomPos;
		internal static bool _isReturningToMainMenu = false;

		static System.Reflection.MethodInfo TargetMethod()
		{
			var type = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Combat.NCombatUi");
			return AccessTools.Method(type, "Activate",
				new[] { typeof(CombatState) });
		}

		public static void Postfix(NCombatUi __instance, CombatState state)
		{
			try
			{
				_isReturningToMainMenu = false;

				var localPlayer = LocalContext.GetMe(state);
				if (localPlayer?.Character is not Regent)
					return;

				var settings = ModSettings.Load();
				var pos = new Vector2(settings.PanelX, settings.PanelY);

				_panel = new SovereignBladePanel();
				_panel.SetDefaultPosition(pos);
				_panel.CustomMinimumSize = new Vector2(400, 400);
				_panel.Size = new Vector2(400, 400);
				_panel.Position = pos;

				if (_savedCustomPos.HasValue)
					_panel.SetCustomPosition(_savedCustomPos.Value);

				// Initialize with current SovereignBlade damage
				foreach (var pile in localPlayer.Piles)
				{
					if (pile?.Cards == null) continue;
					foreach (var card in pile.Cards)
					{
						if (card is SovereignBlade sb)
						{
							_panel.SetDamage(sb.DynamicVars.Damage.BaseValue);
							break;
						}
					}
				}

				__instance.AddChild(_panel);

				// AddChild 이후에 호출해야 _Ready()가 이미 실행된 상태에서 적용됨
				_panel.SetDraggable(settings.Draggable);
			}
			catch (System.Exception ex)
			{
				GD.PrintErr($"[SovereignBladeInjectionPatch] Failed to inject panel: {ex.Message}");
				GD.PrintErr($"[SovereignBladeInjectionPatch] Stack trace: {ex.StackTrace}");
			}
		}

		public static void ApplySettings(ModSettings settings)
		{
			try
			{
				if (_panel != null && GodotObject.IsInstanceValid(_panel))
				{
					_panel.SetDefaultPosition(new Vector2(settings.PanelX, settings.PanelY));
					_panel.SetDraggable(settings.Draggable);
				}
			}
			catch (System.Exception ex)
			{
				GD.PrintErr($"[SovereignBladeInjectionPatch] ApplySettings failed: {ex.Message}");
			}
		}

		internal static void SaveCustomPosition(Vector2 pos)
		{
			_savedCustomPos = pos;
		}

		internal static void ClearCustomPosition()
		{
			_savedCustomPos = null;
		}
	}

	[HarmonyPatch(typeof(SovereignBlade), nameof(SovereignBlade.AddDamage))]
	public static class SovereignBladeAddDamagePatch
	{
		public static void Postfix(SovereignBlade __instance)
		{
			try
			{
				if (SovereignBladeInjectionPatch._panel != null &&
					GodotObject.IsInstanceValid(SovereignBladeInjectionPatch._panel))
				{
					SovereignBladeInjectionPatch._panel.SetDamage(__instance.DynamicVars.Damage.BaseValue);
				}
			}
			catch (System.Exception ex)
			{
				GD.PrintErr($"[SovereignBladeAddDamagePatch] Failed to update damage: {ex.Message}");
			}
		}
	}

	[HarmonyPatch(typeof(NCombatUi), "OnCombatWon")]
	public static class SovereignBladeCombatWonPatch
	{
		public static void Postfix()
		{
			try
			{
				if (SovereignBladeInjectionPatch._panel != null &&
					GodotObject.IsInstanceValid(SovereignBladeInjectionPatch._panel))
				{
					SovereignBladeInjectionPatch._panel.Visible = false;
				}
			}
			catch (System.Exception ex)
			{
				GD.PrintErr($"[SovereignBladeCombatWonPatch] Failed to hide panel: {ex.Message}");
			}
		}
	}

	[HarmonyPatch]
	public static class SovereignBladeReturnToMenuPatch
	{
		static System.Reflection.MethodInfo TargetMethod()
		{
			var type = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.NGame");
			return AccessTools.Method(type, "ReturnToMainMenu");
		}

		static void Prefix()
		{
			try
			{
				SovereignBladeInjectionPatch._isReturningToMainMenu = true;

				var settings = ModSettings.Load();
				if (settings.RememberPosition)
				{
					var customPos = SovereignBladeInjectionPatch._panel?.GetCustomPosition();
					// 커스텀 위치가 있으면 저장, 없으면 현재 기본 설정값 유지
					if (customPos.HasValue)
					{
						settings.PanelX = (int)customPos.Value.X;
						settings.PanelY = (int)customPos.Value.Y;
					}
					settings.Save(); // 커스텀 위치 유무 상관없이 저장 (config 파일 생성 보장)
				}

				SovereignBladeInjectionPatch._savedCustomPos = null;
			}
			catch (System.Exception ex)
			{
				GD.PrintErr($"[SovereignBladeReturnToMenuPatch] Failed: {ex.Message}");
			}
		}
	}

	[HarmonyPatch]
	public static class ModdingScreenSettingsPatch
	{
		static System.Reflection.MethodInfo TargetMethod()
		{
			var type = AccessTools.TypeByName("MegaCrit.Sts2.Core.Nodes.Screens.ModdingScreen.NModInfoContainer");
			var modType = AccessTools.TypeByName("MegaCrit.Sts2.Core.Modding.Mod");
			return AccessTools.Method(type, "Fill", new[] { modType });
		}

		static void Postfix(object __instance, object mod)
		{
			try
			{
				ModSettings.RefreshForSelection(__instance, mod);
			}
			catch (System.Exception ex)
			{
				GD.PrintErr($"[ModdingScreenSettingsPatch] Failed: {ex.Message}");
				GD.PrintErr($"[ModdingScreenSettingsPatch] Stack trace: {ex.StackTrace}");
			}
		}
	}
}
