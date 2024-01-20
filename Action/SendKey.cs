using System.Runtime.InteropServices;
using EyeAuras.Roxy.Shared.Actions.SendInput;
using PoeShared.UI;
using System.Collections.Generic;
namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    [Dependency] public ISendInputController SendInputController { get; init; }
    
    [Dependency] public IHotkeyConverter HotkeyConverter { get; init; }
    
    
    
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