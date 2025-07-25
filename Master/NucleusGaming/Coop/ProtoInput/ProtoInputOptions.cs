﻿using System;

namespace Nucleus.Gaming.Coop.ProtoInput
{
    public class ProtoInputOptions
    {
        public bool InjectStartup;
        public bool InjectRuntime_RemoteLoadMethod;
        public bool InjectRuntime_EasyHookMethod;
        public bool InjectRuntime_EasyHookStealthMethod;

        public bool RegisterRawInputHook;
        public bool GetRawInputDataHook;
        public bool MessageFilterHook;
        public bool GetCursorPosHook;
        public bool SetCursorPosHook;
        public bool GetKeyStateHook;
        public bool GetAsyncKeyStateHook;
        public bool GetKeyboardStateHook;
        public bool CursorVisibilityHook;
        public bool ClipCursorHook;
        public bool ClipCursorHookCreatesFakeClip;
        public bool FocusHooks;
        public bool RenameHandlesHook;
        public bool XinputHook;
        public bool DinputDeviceHook;
        public SetWindowPosHook SetWindowPosHook;
        public bool BlockRawInputHook;
        public bool FindWindowHook;
        public bool CreateSingleHIDHook;
        public bool SetWindowStyleHook;
        public MoveWindowHook MoveWindowHook;
        public bool AdjustWindowRectHook;
        public SetRemoveBorderHook SetRemoveBorderHook;

        public bool RawInputFilter;
        public bool MouseMoveFilter;
        public bool MouseActivateFilter;
        public bool WindowActivateFilter;
        public bool WindowActvateAppFilter;
        public bool MouseWheelFilter;
        public bool MouseButtonFilter;
        public bool KeyboardButtonFilter;

        public uint[] BlockedMessages;

        public string[] RenameHandles;
        public string[] RenameNamedPipes;

        public bool SendMouseWheelMessages;
        public bool SendMouseButtonMessages;
        public bool SendMouseMovementMessages;
        public bool SendKeyboardButtonMessages;
        public bool SendMouseDblClkMessages;

        public bool EnableFocusMessageLoop;
        public int FocusLoopIntervalMilliseconds = 5;
        public bool FocusLoop_WM_ACTIVATE;
        public bool FocusLoop_WM_ACTIVATEAPP;
        public bool FocusLoop_WM_NCACTIVATE;
        public bool FocusLoop_WM_SETFOCUS;
        public bool FocusLoop_WM_MOUSEACTIVATE;

        public DrawFakeCursor DrawFakeCursor;
        public bool AllowFakeCursorOutOfBounds;
        public bool ExtendFakeCursorBounds;
        public bool EnableToggleFakeCursorVisibilityShortcut;
        public uint ToggleFakeCursorVisibilityShortcutVkey = 0x24; // VK_HOME
        public bool DontShowCursorWhenImageUpdated;
        public PutMouseInsideWindow PutMouseInsideWindow;

        public bool UseDinputRedirection;

        public bool UseOpenXinput;

        public bool MultipleProtoControllers;

        public bool DinputHookAlsoHooksGetDeviceState;

        public bool FreezeExternalInputWhenInputNotLocked;

        public Action OnInputLocked;
        public Action OnInputUnlocked;

        public bool AutoHideTaskbar;
    }
}
