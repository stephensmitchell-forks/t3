﻿using System;
using System.Numerics;
using ImGuiNET;
using T3.Core;
using T3.Core.Operator.Slots;
using T3.Gui.Interaction;
using T3.Gui.Styling;
using UiHelpers;

namespace T3.Gui.ChildUi
{
    public static class ValueLabel
    {
        public static bool Draw(ImDrawListPtr drawList, ImRect screenRect, Vector2 alignment, InputSlot<float> remapValue)
        {
            var modified = false;
            var valueText = $"{remapValue.Value:G5}";
            var hashCode = remapValue.GetHashCode();
            ImGui.PushID(hashCode);
            
            var editingUnlocked = ImGui.GetIO().KeyCtrl || _jogDialValue != null;
            var highlight = editingUnlocked;
            // InputGizmo
            {
                var labelSize = screenRect.GetSize() / 4 + Vector2.One * 4;
                var space = screenRect.GetSize() - labelSize;
                var position = screenRect.Min + space * alignment - Vector2.One * 2;
                ImGui.SetCursorScreenPos(position);
                if (editingUnlocked)
                {
                    ImGui.InvisibleButton("button", labelSize);
                    
                    double value = remapValue.TypedInputValue.Value;
                    if (ImGui.IsItemActivated() && ImGui.GetIO().KeyCtrl)
                    {
                        _jogDailCenter = ImGui.GetIO().MousePos;
                        _jogDialValue = remapValue;
                        drawList.AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), Color.White);
                    }
                    
                    if (ImGui.IsItemActive() || !ImGui.IsAnyItemActive())
                    {
                        drawList.AddRectFilled(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), HoverRegionColor);
                    }
                    else
                    {
                        highlight = false;
                    }

                    if (_jogDialValue == remapValue)
                    {
                        if (ImGui.IsItemActive())
                        {
                            modified = JogDialOverlay.Draw(ref value, ImGui.IsItemActivated(), _jogDailCenter, Double.NegativeInfinity, Double.PositiveInfinity,
                                                           0.01f);
                            if (modified)
                            {
                                remapValue.TypedInputValue.Value = (float)value;
                                remapValue.Input.IsDefault = false;
                                remapValue.DirtyFlag.Invalidate();
                            }
                        }
                        else
                        {
                            _jogDialValue = null;
                        }
                    }
                }
            }
            
            // Draw aligned label
            {
                ImGui.PushFont(Fonts.FontSmall);
                var labelSize = ImGui.CalcTextSize(valueText);
                var space = screenRect.GetSize() - labelSize;
                var position = screenRect.Min + space * alignment;
                drawList.AddText(MathUtils.Floor(position), highlight ? T3Style.Colors.ValueLabelColorHover : T3Style.Colors.ValueLabelColor, valueText);
                ImGui.PopFont();
            }            
            ImGui.PopID();
            return modified;
        }

        private static readonly Color HoverRegionColor = new Color(0, 0, 0, 0.2f);
        private static Vector2 _jogDailCenter;
        private static InputSlot<float> _jogDialValue;        
    }
}