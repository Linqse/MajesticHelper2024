using System.Runtime.InteropServices;
using EyeAuras.Roxy.Shared.Actions.SendInput;
using PoeShared.UI;
using System.Collections.Generic;
namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    [Dependency] public ISendInputController SendInputController { get; init; }
    
    [Dependency] public IHotkeyConverter HotkeyConverter { get; init; }
    
    private Dictionary<string, byte> keyMappings = new Dictionary<string, byte>
    {
        {"F13", 0x7C},
        {"F14", 0x7D},
        {"F15", 0x7E},
        {"F16", 0x7F},
        {"F17", 0x80},
        {"F18", 0x81},
        {"F19", 0x82},
        {"F20", 0x83},
        {"F21", 0x84},
        {"F22", 0x85},
        {"F23", 0x86},
        {"F24", 0x87}
    };
  
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    
    private static readonly SendInputArgs DefaultSendInputArgs = new()
    {
        MinDelay = TimeSpan.FromMilliseconds(25),
        MaxDelay = TimeSpan.FromMilliseconds(35),
        InputSimulatorId = "Windows Input",
        InputEventType = InputEventType.KeyPress
    };
    
    private static readonly SendInputArgs DefaultSendInputArgsBackground = new()
    {
        
        InputSimulatorId = "Windows Message API",
        InputEventType = InputEventType.KeyPress
    };

    private async Task SendSecret(string key)
    {
        Log.Info($"Secret press : {key}");
        try
        {
            if (keyMappings.TryGetValue(key, out byte keyCode))
            {
                keybd_event(keyCode, 0, 0, UIntPtr.Zero);
                keybd_event(keyCode, 0, 0x0002, UIntPtr.Zero);
            }
            else
            {
                Log.Info("Failed integration with 0xCheats");
            }
        }
        catch
        {
            Log.Info("Error Secret");
        }
    }
    
    
    private async Task SendKey(string key, Point point = default, string inputEventType = null)
    {
        try
        {
            var activeWindow = WinExists.ActiveWindow;
            if (activeWindow == null)
            {
                Log.Info("Window is null, break Send Input");
                return;
            }

            var hotkey = HotkeyConverter.ConvertFromString(key);

            InputEventType eventType = InputEventType.KeyPress; // Default value
            if (inputEventType == "KeyDown")
            {
                eventType = InputEventType.KeyDown;
            }
            else if (inputEventType == "KeyUp")
            {
                eventType = InputEventType.KeyUp;
            }

            await SendInputController.Send(DefaultSendInputArgs with
            {
                MouseLocation = point,
                Window = activeWindow,
                Gesture = hotkey,
                InputEventType = eventType
            }, CancellationToken.None);
        }
        catch
        {
            Log.Error("Error : SendKey");
        }
    }
    
    
    
    
    private async Task SendKeyBack(string key)
    {
        try
        {
            var activeWindow = WinExists.ActiveWindow;
            if (activeWindow == null)
            {
                Log.Info("Window is null, break Send Input");
                return;
            }

            var hotkey = HotkeyConverter.ConvertFromString(key);

            await SendInputController.Send(DefaultSendInputArgsBackground with
            {
                Window = activeWindow,
                Gesture = hotkey
            }, CancellationToken.None);
        }
        catch
        {
            Log.Error("Error : SendBack");
        }
    }

    private async Task SendBackMouse(Point point)
    {
        try
        {
            var activeWindow = WinExists.ActiveWindow;
            if (activeWindow == null)
            {
                Log.Info("Window is null, break Send Input");
                return;
            }

            var hotkey = HotkeyConverter.ConvertFromString("MouseLeft");

            await SendInputController.Send(DefaultSendInputArgsBackground with
            {
                MouseLocation = point,
                Window = activeWindow,
                Gesture = hotkey
            }, CancellationToken.None);
        }
        catch
        {
            Log.Error("Error : SendBack");
        }
    }
}