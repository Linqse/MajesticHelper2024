using System.Reactive.Subjects;
using AntDesign;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    
    private readonly ISubject<NotificationConfig> notifications = new Subject<NotificationConfig>();
    private readonly ISubject<AntConfirmRequest> confirmRequests = new Subject<AntConfirmRequest>();

    private bool cheatIntegration;
    private bool cheatTeleport;
    public IObservable<NotificationConfig> Notifications => notifications;

    public IObservable<AntConfirmRequest> ConfirmRequests => confirmRequests;

    private IWebUIAuraOverlay Overlay => AuraTree.Aura.Overlays.Items.OfType<IWebUIAuraOverlay>().First();

    private IHotkeyIsActiveTrigger OverlayKey => AuraTree.FindAuraByPath(@"..\Key").Triggers.Items.OfType<IHotkeyIsActiveTrigger>().First();
    
    private void LockUnlock()
    {
        Overlay.IsLocked = !Overlay.IsLocked;
    }
    private void OverClose()
    {
        OverlayKey.TriggerValue = false;
        /*if (ProcessInfo.ProcessHandle != IntPtr.Zero)
            {
                UnsafeNative.ActivateWindow(ProcessInfo.ProcessHandle);
            }*/
    }
    
    private bool _lumber;

    private bool Lumber
    {
        get => _lumber;
        set
        {
            Log.Info($"Setting Lumber to {value}");
            this.RaiseAndSetIfChanged(ref _lumber, value);
        }
    }
    
    private bool _orange;

    private bool Orange
    {
        get => _orange;
        set
        {
            Log.Info($"Setting Orange to {value}");
            this.RaiseAndSetIfChanged(ref _orange, value);
        }
    }
    
    private bool _miner;

    private bool Miner
    {
        get => _miner;
        set
        {
            Log.Info($"Setting Miner to {value}");
            this.RaiseAndSetIfChanged(ref _miner, value);
        }
    }

    private bool _career;
    
    private bool Career
    {
        get => _career;
        set
        {
            Log.Info($"Setting Miner to {value}");
            this.RaiseAndSetIfChanged(ref _career, value);
        }
    }
    
    private bool integration; 
    
    private void ReportError(string message, Exception ex)
    {
        notifications.OnNext(new NotificationConfig()
        {
            Placement = NotificationPlacement.Bottom,
            Message = $"<b>{message}</b>",
            Description = $"{ex.Message}",
            NotificationType = NotificationType.Error,
            Duration = 3
        });
    }

    private void ReportNotification(string message)
    {
        Log.Info(message);
        notifications.OnNext(new NotificationConfig()
        {
            Placement = NotificationPlacement.Bottom,
            Description = message,
            NotificationType = NotificationType.Info,
            Duration = 1
        });
    }
}
