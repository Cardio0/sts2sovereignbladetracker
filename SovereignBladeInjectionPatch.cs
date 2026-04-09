using HarmonyLib;
using Godot;
using System.Collections.Generic;
using System.Linq;
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
		internal static SovereignBladePanel?    _panel;
		internal static BladeCounterOverlay?    _overlay;
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
				_panel.CustomMinimumSize = new Vector2(200, 200);
				_panel.Size = new Vector2(200, 200);
				_panel.Position = pos;

				if (_savedCustomPos.HasValue)
					_panel.SetCustomPosition(_savedCustomPos.Value);

				// 매 프레임 전체 파일을 스캔해서 표시 문자열을 구성
				// 활성(드로우/손패/버린카드): 내림차순 정렬 → "36/27/19"
				// 소멸 zone: 괄호 처리 후 뒤에 붙임 → "(45)/36/27"
				// 변화된 카드: SovereignBlade 인스턴스가 아니므로 자연스럽게 제외됨
				var capturedPlayer = localPlayer;
				// ExhaustPile 참조를 Player에서 직접 가져와 캐싱 (람다 외부에서 한 번만 실행)
				_panel.GetDisplayText = () =>
				{
					try
					{
						var activeValues    = new List<decimal>();
						var exhaustedValues = new List<decimal>();

						foreach (var pile in capturedPlayer.Piles)
						{
							if (pile?.Cards == null) continue;

							// CardPile.Type 프로퍼티로 소멸 파일 판별
							var pileTypeName = pile.GetType()
								.GetProperty("Type",
									System.Reflection.BindingFlags.Public |
									System.Reflection.BindingFlags.NonPublic |
									System.Reflection.BindingFlags.Instance)
								?.GetValue(pile)
								?.ToString() ?? "";
							bool isExhaust = pileTypeName.Contains("Exhaust");

							foreach (var card in pile.Cards)
							{
								if (card is SovereignBlade sb)
								{
									if (isExhaust)
										exhaustedValues.Add(sb.DynamicVars.Damage.BaseValue);
									else
										activeValues.Add(sb.DynamicVars.Damage.BaseValue);
								}
							}
						}

						// 카드가 하나도 없으면 (전부 변화됨) 기본값 표시
						if (activeValues.Count == 0 && exhaustedValues.Count == 0)
							return "[center]10[/center]";

						// 내림차순 정렬
						activeValues.Sort((a, b) => b.CompareTo(a));
						exhaustedValues.Sort((a, b) => b.CompareTo(a));

						var parts = new List<string>();
						foreach (var v in activeValues)
							parts.Add(((int)v).ToString());
						foreach (var v in exhaustedValues)
							// 소멸 카드: 회색으로 표시
							parts.Add($"[color=#aaaaaa]{(int)v}[/color]");

						// 4개마다 줄바꿈
						var display = new System.Text.StringBuilder("[center]");
						for (int i = 0; i < parts.Count; i++)
						{
							if (i > 0 && i % 4 == 0) display.Append('\n');
							else if (i > 0)            display.Append('/');
							display.Append(parts[i]);
						}
						display.Append("[/center]");
						return display.ToString();
					}
					catch { return null; }
				};

				__instance.AddChild(_panel);

				// AddChild 이후에 호출해야 _Ready()가 이미 실행된 상태에서 적용됨
				_panel.Visible = settings.ShowPanel;
				_panel.SetDraggable(settings.Draggable);

				// ── BladeCounterOverlay 주입 ─────────────────────────────────────
				// SovereignBlade.GetVfxNode() 리플렉션으로 각 칼날의 spineNode 위치를 가져옴
				_overlay = new BladeCounterOverlay();
				_overlay.SetCounterEnabled(settings.CounterOnBlade);
				_overlay.SetFontSize(settings.BladeFontSize);

				var getVfxNodeMethod = typeof(SovereignBlade).GetMethod(
					"GetVfxNode",
					System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

				_overlay.GetBladeData = () =>
				{
					try
					{
						var result = new List<(Node2D? spine, int damage)>();
						foreach (var pile in capturedPlayer.Piles)
						{
							if (pile?.Cards == null) continue;
							var pileTypeName = pile.GetType()
								.GetProperty("Type",
									System.Reflection.BindingFlags.Public |
									System.Reflection.BindingFlags.NonPublic |
									System.Reflection.BindingFlags.Instance)
								?.GetValue(pile)?.ToString() ?? "";
							if (pileTypeName.Contains("Exhaust")) continue; // 소멸 카드는 제외

							foreach (var card in pile.Cards)
							{
								if (card is not SovereignBlade sb) continue;

								// GetVfxNode(player, card) 호출
								var vfxNode = getVfxNodeMethod?
									.Invoke(null, new object[] { capturedPlayer, card }) as Node2D;
								if (vfxNode == null || !GodotObject.IsInstanceValid(vfxNode)) continue;

								// _spineNode 필드로 실제 궤도 위치 취득
								Node2D? spineNode = null;
								var spineField = vfxNode.GetType().GetField(
									"_spineNode",
									System.Reflection.BindingFlags.NonPublic |
									System.Reflection.BindingFlags.Instance);
								spineNode = spineField?.GetValue(vfxNode) as Node2D;

								// _spineNode 실패 시 vfxNode 자체를 폴백
								result.Add((spineNode ?? vfxNode, (int)sb.DynamicVars.Damage.BaseValue));
							}
						}
						return result;
					}
					catch { return null; }
				};

				__instance.AddChild(_overlay);
				// ─────────────────────────────────────────────────────────────────
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
					_panel.Visible = settings.ShowPanel;
					_panel.SetDefaultPosition(new Vector2(settings.PanelX, settings.PanelY));
					_panel.SetDraggable(settings.Draggable);
				}
				if (_overlay != null && GodotObject.IsInstanceValid(_overlay))
				{
					_overlay.SetCounterEnabled(settings.CounterOnBlade);
					_overlay.SetFontSize(settings.BladeFontSize);
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
				if (SovereignBladeInjectionPatch._overlay != null &&
					GodotObject.IsInstanceValid(SovereignBladeInjectionPatch._overlay))
				{
					SovereignBladeInjectionPatch._overlay.Visible = false;
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
