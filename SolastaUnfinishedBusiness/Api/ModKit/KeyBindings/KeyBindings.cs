﻿using System;
using System.Collections.Generic;
using System.Linq;
using SolastaUnfinishedBusiness.Api.Infrastructure;
using static UnityModManagerNet.UnityModManager;

namespace SolastaUnfinishedBusiness.Api.ModKit;

internal static partial class UI
{
    private static IEnumerable<string> Conflicts(this KeyBind keyBind)
    {
        return KeyBindings.Conflicts
            .GetValueOrDefault(keyBind.BindCode, new List<string>())
            .Where(id => id != keyBind.ID);
    }

    public static class KeyBindings
    {
        private static ModEntry _modEntry;
        private static SerializableDictionary<string, KeyBind> _bindings;
        private static readonly Dictionary<string, Action> Actions = new();
        internal static Dictionary<string, List<string>> Conflicts = new();
        private static bool _bindingsDidChange;

        private static KeyBind _lastTriggered;

        public static bool IsActive(string identifier)
        {
            return GetBinding(identifier).IsActive;
        }

        public static Action GetAction(string identifier)
        {
            return Actions.GetValueOrDefault(identifier);
        }

        public static void RegisterAction(string identifier, Action action)
        {
            Actions[identifier] = action;
        }

        internal static KeyBind GetBinding(string identifier)
        {
            _bindingsDidChange = true;
            return _bindings.GetValueOrDefault(identifier, new KeyBind(identifier));
        }

        internal static void SetBinding(string identifier, KeyBind binding)
        {
            _bindings[identifier] = binding;
            _modEntry.SaveSettings("bindings.json", _bindings);
            _bindingsDidChange = true;
        }

        private static void UpdateConflicts()
        {
            Conflicts.Clear();
            foreach (var binding in _bindings)
            {
                var keyBind = binding.Value;
                
                if (keyBind.IsEmpty)
                {
                    continue;
                }

                var identifier = binding.Key;
                var bindCode = keyBind.ToString();
                var conflict = Conflicts.GetValueOrDefault(bindCode, new List<string>());
                conflict.Add(identifier);
                Conflicts[bindCode] = conflict;
            }

            Conflicts = Conflicts.Filter(kvp => kvp.Value.Count > 1);
            //Logger.Log($"conflicts: {String.Join(", ", conflicts.Select(kvp => $"{kvp.Key.orange()} : {kvp.Value.Count}".cyan())).yellow()}");
        }

        public static void OnLoad(ModEntry modEntryIn)
        {
            _modEntry ??= modEntryIn;

            if (_bindings != null)
            {
                return;
            }

            modEntryIn.LoadSettings("bindings.json", ref _bindings);
            _bindingsDidChange = true;
        }

        public static void OnGUI()
        {
            if (!_bindingsDidChange)
            {
                return;
            }

            UpdateConflicts();
            _bindingsDidChange = false;
        }

        public static void OnUpdate()
        {
            if (_lastTriggered is { IsActive: false })
                //if (debugKeyBind)
                //    Logger.Log($"    lastTriggered: {lastTriggered} - IsActive: {lastTriggered.IsActive}");
            {
                //if (debugKeyBind)
                //    Logger.Log($"    lastTriggered: {lastTriggered} - Finished".green());
                _lastTriggered = null;
            }

            //if (debugKeyBind)
            //    Logger.Log($"looking for {Event.current.keyCode}");
            foreach (var item in _bindings)
            {
                var identifier = item.Key;
                var binding = item.Value;
                var active = binding.IsActive;
                //if (debugKeyBind)
                //    Logger.Log($"    checking: {binding.ToString()} - IsActive: {(active ? "True".cyan() : "False")} action: {actions.ContainsKey(identifier)}");
                if (!active || !Actions.ContainsKey(identifier))
                {
                    continue;
                }

                //if (debugKeyBind)
                //    Logger.Log($"    binding: {binding.ToString()} - lastTriggered: {lastTriggered}");
                if (_lastTriggered != null && binding == _lastTriggered)
                {
                    continue;
                }

                //if (debugKeyBind)
                //    Logger.Log($"    firing action: {identifier}".cyan());
                Actions.TryGetValue(identifier, out var action);
                action();
                _lastTriggered = binding;
            }
        }
    }
}
