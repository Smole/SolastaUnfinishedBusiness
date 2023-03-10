﻿// Copyright < 2021 > Narria (github user Cabarius) - License: MIT

using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using UnityEngine;
using static SolastaUnfinishedBusiness.Api.ModKit.UI;

namespace SolastaUnfinishedBusiness.Displays;

public static class PartyEditor
{
    private static ToggleChoice _selectedToggle = ToggleChoice.None;
    private static int _selectedCharacterIndex;
    private static bool _editingFromPool;

    private static (string, string) _nameEditState = (null, null);

    private static List<RulesetCharacter> _characterPool;
    private static ICharacterPoolService PoolService => ServiceRepository.GetService<ICharacterPoolService>();

    public static List<RulesetCharacter> CharacterPool
    {
        get
        {
            if (_characterPool == null)
            {
                RefreshPool();
            }

            return _characterPool;
        }
        set => _characterPool = value;
    }

    public static void OnGUI()
    {
        Label("Experimental Preview:".Localized().Orange().Bold() + " " +
              "This simple party editor lets you edit characters in a loaded game session. Right now it lets you edit your character's first and last name. More features are coming soon (tm). Please click on the following to report issues:"
                  .Localized().Green());
        LinkButton("https://github.com/SolastaMods/SolastaUnfinishedBusiness/issues",
            "https://github.com/SolastaMods/SolastaUnfinishedBusiness/issues");
        var characters = GetCharacterList();
        if (characters == null)
        {
            Label("****** Party Editor unavailable: Please load a save game ******".Localized()
                .Yellow().Bold());
        }
        else
        {
            var commandService = ServiceRepository.GetService<ICommandService>();
            Space(15);
            HStack("Quickies".Localized(), 2,
                () => ActionButton("Long Rest",
                    () => commandService.StartRest(RuleDefinitions.RestType.LongRest, false),
                    AutoWidth())
            );
            Div();
            Label("Current Party".Localized().Cyan().Bold());
            using (VerticalScope())
            {
                foreach (var ch in characters)
                {
                    var selectedCharacter = GetSelectedCharacter();
                    var changed = false;
                    var level = ch.TryGetAttributeValue("CharacterLevel");
                    using (HorizontalScope())
                    {
                        var name = ch.Name;
                        if (ch is RulesetCharacterHero hero)
                        {
                            name = hero.Name + " " + hero.SurName;
                            if (EditableLabel(ref name, ref _nameEditState, 200, n => n.Orange().Bold(),
                                    MinWidth(100),
                                    MaxWidth(600)))
                            {
                                var parts = name.Split();
                                switch (parts.Length)
                                {
                                    case > 1:
                                        hero.Name = parts[0];
                                        hero.SurName = String.Join(" ", parts.Skip(1).ToArray()).Trim();
                                        break;
                                    case 1:
                                        hero.Name = parts[0];
                                        hero.SurName = "";
                                        break;
                                }

                                changed = true;
                            }
                        }
                        else if (EditableLabel(ref name, ref _nameEditState, 200, n => n.Orange().Bold(),
                                     MinWidth(100),
                                     MaxWidth(600)))
                        {
                            ch.Name = name;
                            changed = true;
                        }

                        Space(5);
                        Label((level < 10 ? "   lvl" : "   lv").Green() + $" {level}", Width(90));
                        Space(5);
                        var showStats = ch == selectedCharacter && _selectedToggle == ToggleChoice.Stats;
                        if (DisclosureToggle("Stats", ref showStats, 125))
                        {
                            if (showStats)
                            {
                                selectedCharacter = ch;
                                _selectedToggle = ToggleChoice.Stats;
                            }
                            else { _selectedToggle = ToggleChoice.None; }
                        }
                    }

                    if (ch == selectedCharacter && _selectedToggle == ToggleChoice.Stats)
                    {
                        Div(100, 20, 755);

                        foreach (var attr in ch.Attributes)
                        {
                            var attrName = attr.Key;
                            var attribute = attr.Value;
                            var baseValue = attribute.baseValue;
                            var modifiers = attribute.ActiveModifiers.Where(m => m.Value != 0).Select(m =>
                                    $"{m.Value:+0;-#} {String.Join(" ", m.Tags).TrimStart('0', '1', '2', '3', '4', '5', '6', '7', '8', '9').Cyan()}")
                                .ToArray();
                            var modifiersString = String.Join(" ", modifiers);
                            using (HorizontalScope())
                            {
                                Space(100);
                                Label(attrName, Width(400f));
                                Space(25);
                                ActionButton(" < ", () =>
                                {
                                    attribute.baseValue -= 1;
                                    changed = true;
                                }, GUI.skin.box, AutoWidth());
                                Space(20);
                                Label($"{attribute.currentValue}".Orange().Bold(), Width(50f));
                                ActionButton(" > ", () =>
                                {
                                    attribute.baseValue += 1;
                                    changed = true;
                                }, GUI.skin.box, AutoWidth());
                                Space(25);
                                ActionIntTextField(ref baseValue, v =>
                                {
                                    attribute.baseValue = v;
                                    changed = true;
                                }, Width(75));
                                Space(10);
                                Label($"{modifiersString}");
                            }
                        }

                        if (changed)
                        {
                            ch.RefreshAll();
                        }
                    }

                    if (changed && _editingFromPool && ch is RulesetCharacterHero h)
                    {
                        // ReSharper disable once InvocationIsSkipped
                        Main.Log(String.Format("Saving Pool Character: " + h.Name));
                        // ReSharper disable once InvocationIsSkipped
                        Main.Log(PoolService.SaveCharacter(h));
                        // h.RefreshAll();
                        // RefreshPool();
                    }

                    if (selectedCharacter != GetSelectedCharacter())
                    {
                        _selectedCharacterIndex = GetCharacterList().IndexOf(selectedCharacter);
                    }
                }
            }
        }
    }

    private static List<RulesetCharacter> GetCharacterList()
    {
        _editingFromPool = false;

#pragma warning disable IDE0031
        // don't use ? or ?? or a type deriving from an UnityEngine.Object to avoid bypassing lifetime check
        var chars = Gui.GameCampaign == null
            ? null
            : Gui.GameCampaign.Party.CharactersList.Select(ch => ch.RulesetCharacter).ToList();
#if DEBUG
        if (chars != null)
        {
            return chars;
        }

        chars = CharacterPool;
        _editingFromPool = true;
#endif
#pragma warning restore IDE0031
        return chars;
    }

    private static RulesetCharacter GetSelectedCharacter()
    {
        var characterList = GetCharacterList();
        if (characterList == null || characterList.Count == 0)
        {
            return null;
        }

        if (_selectedCharacterIndex >= characterList.Count)
        {
            _selectedCharacterIndex = 0;
        }

        return characterList[_selectedCharacterIndex];
    }

    private static void RefreshPool()
    {
        _characterPool = new List<RulesetCharacter>();
        PoolService.EnumeratePool();

        foreach (var filename in PoolService.Pool.Select(item => item.Key))
        {
            // ReSharper disable once InvocationIsSkipped
            Main.Log("Loading: " + filename);
            PoolService.LoadCharacter(filename, out var h, out _);
#if false
                        Mod.Debug(h.Name + " " + h);
                        PropertyInfo[] infos = h.GetType().GetProperties();
                        Mod.Debug("" +  infos);
                        foreach (PropertyInfo info in infos)
                        {
                            Mod.Debug(String.Format("    {0} : {1}", info.Name, info.GetValue(h, null)?.ToString()) ?? "null");
                        }
#endif
            _characterPool.Add(h);
        }

        // ReSharper disable once InvocationIsSkipped
        Main.Log($"{_characterPool.Count} Characters Loaded");
    }

    private enum ToggleChoice
    {
        Classes,
        Stats,
        Facts,
        Features,
        Buffs,
        Abilities,
        Spells,
        None
    }
}
