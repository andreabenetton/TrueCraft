using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using TrueCraft.Core;

namespace TrueCraft.Client.Input
{
    public enum InputAction
    {
        MoveForward,
        MoveBack,
        MoveLeft,
        MoveRight,
        Jump,
        Exit,
        Screenshot,
        OpenInventory,
        OpenChat,
        ToggleMouseCapture,
        ToggleDebugOverlay
    }

    /// <summary>
    ///     Resolves a logical <see cref="InputAction"/> to the <see cref="Keys"/> that
    ///     trigger it. Bindings are read from <see cref="UserSettings.Local"/> when
    ///     present, otherwise the built-in defaults are used.
    /// </summary>
    public static class InputBindings
    {
        private static readonly Dictionary<InputAction, Keys[]> Defaults =
            new Dictionary<InputAction, Keys[]>
            {
                { InputAction.MoveForward, new[] { Keys.W, Keys.Up } },
                { InputAction.MoveBack, new[] { Keys.S, Keys.Down } },
                { InputAction.MoveLeft, new[] { Keys.A, Keys.Left } },
                { InputAction.MoveRight, new[] { Keys.D, Keys.Right } },
                { InputAction.Jump, new[] { Keys.Space } },
                { InputAction.Exit, new[] { Keys.Escape } },
                { InputAction.Screenshot, new[] { Keys.F2 } },
                { InputAction.OpenInventory, new[] { Keys.E } },
                { InputAction.OpenChat, new[] { Keys.T } },
                { InputAction.ToggleMouseCapture, new[] { Keys.Tab } },
                { InputAction.ToggleDebugOverlay, new[] { Keys.F3 } }
            };

        public static bool Matches(InputAction action, Keys key)
        {
            // Custom bindings (UserSettings) override defaults when present.
            if (UserSettings.Local?.KeyBindings != null &&
                UserSettings.Local.KeyBindings.TryGetValue(action.ToString(), out var custom) &&
                Enum.TryParse<Keys>(custom, true, out var customKey))
                return customKey == key;

            if (!Defaults.TryGetValue(action, out var defaults))
                return false;
            for (var i = 0; i < defaults.Length; i++)
                if (defaults[i] == key)
                    return true;
            return false;
        }
    }
}
