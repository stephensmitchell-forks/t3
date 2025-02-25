﻿using System;
using ImGuiNET;
using System.Runtime.InteropServices;
using T3.Core.Operator;
using T3.Gui.Graph.Interaction;
using T3.Gui.InputUi;
using T3.Gui.Styling;
using T3.Gui.TypeColors;
using T3.Gui.Windows;

namespace T3.Gui.UiHelpers
{
    /// <summary>
    /// Shows a tree of all defined symbols sorted by namespace.
    /// This is used by <see cref="SymbolBrowser"/> and <see cref="T3Ui.DrawAppMenu"/>
    /// </summary>
    public static class SymbolTreeMenu
    {
        public static void Draw()
        {
            TreeNode.PopulateCompleteTree();
            DrawNodesRecursively(TreeNode);
        }
        
        private static void DrawNodesRecursively(NamespaceTreeNode subtree)
        {
            if (subtree.Name == NamespaceTreeNode.RootNodeId)
            {
                DrawContent(subtree);
            }
            else
            {
                ImGui.PushID(subtree.Name);
                if (ImGui.BeginMenu(subtree.Name))
                {
                    DrawContent(subtree);

                    ImGui.EndMenu();
                }

                ImGui.PopID();
            }
        }

        private static void DrawContent(NamespaceTreeNode subtree)
        {
            foreach (var subspace in subtree.Children)
            {
                DrawNodesRecursively(subspace);
            }

            foreach (var symbol in subtree.Symbols)
            {
                DrawSymbolItem(symbol);
            }
        }

        public static void DrawSymbolItem(Symbol symbol)
        {
            ImGui.PushID(symbol.Id.GetHashCode());
            {
                var color = symbol.OutputDefinitions.Count > 0
                                ? TypeUiRegistry.GetPropertiesForType(symbol.OutputDefinitions[0]?.ValueType).Color
                                : Color.Gray;

                ImGui.PushStyleColor(ImGuiCol.Button, ColorVariations.Operator.Apply(color).Rgba);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, ColorVariations.OperatorHover.Apply(color).Rgba);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, ColorVariations.OperatorInputZone.Apply(color).Rgba);
                ImGui.PushStyleColor(ImGuiCol.Text, ColorVariations.OperatorLabel.Apply(color).Rgba);

                if (ImGui.Button(symbol.Name))
                {
                    //_selectedSymbol = symbol;
                }

                HandleDragAndDropForSymbolItem(symbol);
                
                if (SymbolUiRegistry.Entries.TryGetValue(symbol.Id, out var symbolUi))
                {
                    if (!string.IsNullOrEmpty(symbolUi.Description))
                    {
                        ImGui.SameLine();
                        ImGui.TextDisabled("(?)");
                        if (ImGui.IsItemHovered())
                        {
                            ImGui.BeginTooltip();
                            ImGui.PushTextWrapPos(ImGui.GetFontSize() * 25.0f);
                            ImGui.TextUnformatted(symbolUi.Description);
                            ImGui.PopTextWrapPos();
                            ImGui.EndTooltip();
                        }
                    }
                }

                if (ExampleSymbolLinking.ExampleSymbols.TryGetValue(symbol.Id, out var examples))
                {
                    ImGui.PushFont(Fonts.FontSmall);
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                    for (var index = 0; index < examples.Count; index++)
                    {
                        var exampleId = examples[index];
                        ImGui.SameLine();
                        ImGui.Button($"EXAMPLE");
                        HandleDragAndDropForSymbolItem(SymbolRegistry.Entries[exampleId]);
                    }
                    ImGui.PopStyleVar();
                    ImGui.PopFont();
                }

                ImGui.PopStyleColor(4);
            }
            ImGui.PopID();
        }

        public static void HandleDragAndDropForSymbolItem(Symbol symbol)
        {
            if (ImGui.IsItemActive())
            {
                if (ImGui.BeginDragDropSource())
                {
                    if (_dropData == new IntPtr(0))
                    {
                        _guidSting = symbol.Id + "|";
                        _dropData = Marshal.StringToHGlobalUni(_guidSting);
                        T3Ui.DraggingIsInProgress = true;
                    }

                    ImGui.SetDragDropPayload("Symbol", _dropData, (uint)(_guidSting.Length * sizeof(Char)));

                    ImGui.Button(symbol.Name + " (creating instance)");
                    ImGui.EndDragDropSource();
                }
            }
            else if (ImGui.IsItemDeactivated())
            {
                _dropData = new IntPtr(0);
            }
        }

        private static readonly NamespaceTreeNode TreeNode = new NamespaceTreeNode(NamespaceTreeNode.RootNodeId);
        
        private static IntPtr _dropData = new IntPtr(0);
        private static string _guidSting;
    }
}