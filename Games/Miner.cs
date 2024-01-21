using System.Reactive.Disposables;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using EyeAuras.OpenCVAuras.Scaffolding;
using Polly;
namespace EyeAuras.Web.Repl.Component;

public partial class Main
{

    private readonly SerialDisposable activeMinerAnchors;
     
    private async Task StartMiner()
    {   
        try
        {
            using var minerAnchors = new CompositeDisposable().AssignTo(activeMinerAnchors);
            

            var cancellationTokenSource = new CancellationTokenSource(); 
            Disposable.Create(() => cancellationTokenSource.Cancel()).AddTo(minerAnchors); 
            Disposable.Create(() => Miner = false).AddTo(minerAnchors); 
            this.WhenAnyValue(x => x.Miner)
                .Where(x => !x)
                .Subscribe(() => cancellationTokenSource.Cancel())
                .AddTo(minerAnchors);

            
            ReportNotification("Starting miner");

            AuraTree.Aura["ImageE"] = CalculateTargetRectangle(0.0604f, 0.0389f, 50, 50);
            AuraTree.Aura["ImageRange"] = CalculateTargetRectangle(0.9349f, 0.9287f, 200, 50);

            DifY = WinExists.ActiveWindow.DwmFrameBounds.Height - WinExists.ActiveWindow.ClientRect.Height;
            MLGather.Refresh();
            
            await Policy
                .Handle<Exception>(ex =>
                {
                    if (ex is OperationCanceledException )
                    {
                        return false;
                    }

                    Log.Warn(ex.Message);
                    return true;
                })
                .WaitAndRetryForeverAsync((attempt) => TimeSpan.FromMilliseconds(200))
                .ExecuteAsync(async token =>
                {
                    
                    token.ThrowIfCancellationRequested();
                    await WaitForE(token);
                    token.ThrowIfCancellationRequested();
                    await WaitForRange(token);
                    token.ThrowIfCancellationRequested();
                    await PickUpRange(token);
                    token.ThrowIfCancellationRequested();
                    await PickUpML(token);
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(1000);
                    throw new InvalidStateException($"Error for loop miner");
                    
                }, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Log.Warn("Miner routine cancelled");
        }
        catch (Exception e)
        {
            Log.Error($"Miner routine failed with exception: {e.Message}");
            ReportError("Miner failed", e);
        }
    }
    
    
    
    
    
    
}