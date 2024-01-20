using System.Reactive.Disposables;
using EyeAuras.OpenCVAuras.Scaffolding;
using Polly;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{

    private readonly SerialDisposable activeShroomsAnchors;

    private async Task StartShrooms()
    {
        try
        {
            using var shroomsAnchors = new CompositeDisposable().AssignTo(activeShroomsAnchors);


            var cancellationTokenSource = new CancellationTokenSource();
            Disposable.Create(() => cancellationTokenSource.Cancel()).AddTo(shroomsAnchors);
            Disposable.Create(() => Shrooms = false).AddTo(shroomsAnchors);
            this.WhenAnyValue(x => x.Shrooms)
                .Where(x => !x)
                .Subscribe(() => cancellationTokenSource.Cancel())
                .AddTo(shroomsAnchors);

            
            DifY = WinExists.ActiveWindow.DwmFrameBounds.Height - WinExists.ActiveWindow.ClientRect.Height;
            
            AuraTree.Aura["ImageE"] = CalculateTargetRectangle(0.0604f, 0.0389f, 50, 50);
            AuraTree.Aura["ImageCaptcha"] = CalculateTargetRectangle(0.5021f, 0.9306f, 160, 160);
            
            ReportNotification("Starting Shrooms");
            


            await Policy
                .Handle<Exception>(ex =>
                {
                    if (ex is OperationCanceledException)
                    {
                        return false;
                    }

                    Log.Warn(ex.Message);
                    return true;
                })
                .WaitAndRetryForeverAsync((attempt) => TimeSpan.FromMilliseconds(200))
                .ExecuteAsync(async token =>
                {
                    await WaitForE(token);
                    await Task.Delay(11000);

                    throw new InvalidStateException($"Error for loop Shrooms");




                }, cancellationTokenSource.Token);

        }
        catch (OperationCanceledException)
        {
            Log.Warn("Shrooms routine cancelled");
        }
        catch (Exception e)
        {
            Log.Error($"Shrooms routine failed with exception: {e.Message}");
            ReportError("Shrooms failed", e);
        }


    }


}