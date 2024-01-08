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
    
    private bool _orange;

    public bool Orange
    {
        get => _orange;
        set
        {
            Log.Info($"Setting Orange to {value}");
            this.RaiseAndSetIfChanged(ref _orange, value);
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
