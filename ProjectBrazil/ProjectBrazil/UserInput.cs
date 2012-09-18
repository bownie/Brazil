﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xyglo.Brazil
{
    /// <summary>
    /// What features for input are available
    /// </summary>
    /*
    public enum InputFeatures
    {
        Dumb     = 0,           // No input
        Keyboard = 1 << 0,      // Keyboard
        Mouse    = 1 << 1,      // Mouse
        Gesture  = 1 << 2       // Kinect or Leap
    }
    */

    /// <summary>
    /// Project Brazil Keys - this list could be cut down I think
    /// </summary>
    public enum Keys
    {
        None,
        LButton,
        RButton,
        Cancel,
        MButton,
        XButton1,
        XButton2,
        Back,
        Tab,
        LineFeed,
        Clear,
        Return,
        Enter,
        ShiftKey,
        ControlKey,
        Menu,
        Pause,
        Capital,
        CapsLock,
        KanaMode,
        HanguelMode,
        HangulMode,
        JunjaMode,
        FinalMode,
        HanjaMode,
        KanjiMode,
        Escape,
        IMEConvert,
        IMENonconvert,
        IMEAccept,
        IMEAceept,
        IMEModeChange,
        Space,
        Prior,
        PageUp,
        Next,
        PageDown,
        End,
        Home,
        Left,
        Up,
        Right,
        Down,
        Select,
        Print,
        Execute,
        Snapshot,
        PrintScreen,
        Insert,
        Delete,
        Help,
        D0,
        D1,
        D2,
        D3,
        D4,
        D5,
        D6,
        D7,
        D8,
        D9,
        A,
        B,
        C,
        D,
        E,
        F,
        G,
        H,
        I,
        J,
        K,
        L,
        M,
        N,
        O,
        P,
        Q,
        R,
        S,
        T,
        U,
        V,
        W,
        X,
        Y,
        Z,
        LWin,
        RWin,
        Apps,
        Sleep,
        NumPad0,
        NumPad1,
        NumPad2,
        NumPad3,
        NumPad4,
        NumPad5,
        NumPad6,
        NumPad7,
        NumPad8,
        NumPad9,
        Multiply,
        Add,
        Separator,
        Subtract,
        Decimal,
        Divide,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        F13,
        F14,
        F15,
        F16,
        F17,
        F18,
        F19,
        F20,
        F21,
        F22,
        F23,
        F24,
        NumLock,
        Scroll,
        LShiftKey,
        RShiftKey,
        LControlKey,
        RControlKey,
        LMenu,
        RMenu,
        BrowserBack,
        BrowserForward,
        BrowserRefresh,
        BrowserStop,
        BrowserSearch,
        BrowserFavorites,
        BrowserHome,
        VolumeMute,
        VolumeDown,
        VolumeUp,
        MediaNextTrack,
        MediaPreviousTrack,
        MediaStop,
        MediaPlayPause,
        LaunchMail,
        SelectMedia,
        LaunchApplication1,
        LaunchApplication2,
        OemSemicolon,
        Oem1,
        Oemplus,
        Oemcomma,
        OemMinus,
        OemPeriod,
        OemQuestion,
        Oem2,
        Oemtilde,
        Oem3,
        OemOpenBrackets,
        Oem4,
        OemPipe,
        Oem5,
        OemCloseBrackets,
        Oem6,
        OemQuotes,
        Oem7,
        Oem8,
        OemBackslash,
        Oem102,
        ProcessKey,
        Packet,
        Attn,
        Crsel,
        Exsel,
        EraseEof,
        Play,
        Zoom,
        NoName,
        Pa1,
        LeftControl,
        RightControl,
        LeftAlt,
        RightAlt,
        LeftShift,
        RightShift,
        LeftWindows,
        RightWindows,
        OemTilde,
        OemPlus,
        OemComma,
        OemClear,
        Shift,    // modifier according to spec
        Control,  // modifier according to spec
        Alt       // modifier according to spec
    }

    public enum Pointer
    {
        On,
        Off
    }


    public class UserInput
    {
    }

    /// <summary>
    /// Vector2 is based on XNA's Vector2
    /// </summary>
    public class BrazilVector2
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        BrazilVector2()
        {
            X = 0.0f;
            Y = 0.0f;
        }

        /// <summary>
        /// Single constructor
        /// </summary>
        /// <param name="single"></param>
        BrazilVector2(float single)
        {
            X = single;
            Y = single;
        }

        /// <summary>
        /// Double constructor
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="y1"></param>
        BrazilVector2(float x1, float y1)
        {
            X = x1;
            Y = y1;
        }

        //static Vector2()
        //{
        //}

        public float X {get; set; } 
        public float Y {get; set; }

        public static BrazilVector2 Zero { get { return ZeroVector2; } }

        static public BrazilVector2 ZeroVector2 = new BrazilVector2(0, 0);
    }
}
