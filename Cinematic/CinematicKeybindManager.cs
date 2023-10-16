using System.Collections.Generic;
using System;
using UnityEngine.InputSystem;


public class CinematicKeybindManager
{
    private class KeybindData
    {
        public readonly Action action;
        public readonly bool held;
        public KeybindData(Action a, bool h)
        {
            action = a;
            held = h;
        }
    }

    ModFile _file;
    Dictionary<string, KeybindData> _keybinds = new Dictionary<string, KeybindData>();
    Dictionary<string, bool> _pressedLastFrame = new Dictionary<string, bool>();

    public CinematicKeybindManager(ModFile file)
    {
        _file = file;
    }

    private bool WasPressedLastFrame(string p)
    {
        if (!_pressedLastFrame.ContainsKey(p))
        {
            _pressedLastFrame[p] = false;
            return false;
        }

        return _pressedLastFrame[p];
    }
    
    public void CheckInputs()
    {
        foreach (InputDevice device in InputSystem.devices)
        {
            foreach (string path in _keybinds.Keys)
            {
                InputControl control = device.TryGetChildControl(path);
                if (control == null)
                {
                    continue;
                }
                if (control.IsPressed())
                {
                    if (!_keybinds[path].held && WasPressedLastFrame(path))
                    {
                        continue;
                    }

                    _keybinds[path].action();
                    _pressedLastFrame[path] = true;
                }
                else
                {
                    _pressedLastFrame[path] = false;
                }
            }

        }
    }

    public void BindForward(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindMoveForward")] = new KeybindData(a, held);
    }

    public void BindBackward(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindMoveBackward")] = new KeybindData(a, held);
    }

    public void BindRight(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindMoveRight")] = new KeybindData(a, held);
    }

    public void BindLeft(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindMoveLeft")] = new KeybindData(a, held);
    }

    public void BindUp(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindMoveUp")] = new KeybindData(a, held);
    }

    public void BindDown(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindMoveDown")] = new KeybindData(a, held);
    }

    public void BindToggleCam(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindToggleFreecam")] = new KeybindData(a, held);
    }

    public void BindToggleLock(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindLockFreecam")] = new KeybindData(a, held);
    }

    public void BindToggleKillVolumes(Action a, bool held=true)
    {
        _keybinds[_file.GetString("KeybindToggleKillVolumes")] = new KeybindData(a, held);
    }
}