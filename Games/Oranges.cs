using System.Reactive.Disposables;
using EyeAuras.OpenCVAuras.Scaffolding;
using Polly;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    
    private readonly SerialDisposable activeOrangeAnchors;
    
    private async Task StartOrange()
    {   
        try
        {
            using var orangeAnchors = new CompositeDisposable().AssignTo(activeOrangeAnchors);
            

            var cancellationTokenSource = new CancellationTokenSource(); 
            Disposable.Create(() => cancellationTokenSource.Cancel()).AddTo(orangeAnchors); 
            Disposable.Create(() => Orange = false).AddTo(orangeAnchors); 
            this.WhenAnyValue(x => x.Orange)
                .Where(x => !x)
                .Subscribe(() => cancellationTokenSource.Cancel())
                .AddTo(orangeAnchors);

            DifY = WinExists.ActiveWindow.DwmFrameBounds.Height - WinExists.ActiveWindow.ClientRect.Height;
            
            ReportNotification("Starting oranges");
            
            AuraTree.Aura["ImageE"] = CalculateTargetRectangle(0.0604f, 0.0389f, 50, 50);

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
                    await WaitForLKM(token);
                    token.ThrowIfCancellationRequested();
                    await PickUpOranges(token);
                    token.ThrowIfCancellationRequested();

                    throw new InvalidStateException($"Error for loop oranges");




                }, cancellationTokenSource.Token);

        }
        catch (OperationCanceledException)
        {
            Log.Warn("Orange routine cancelled");
        }
        catch (Exception e)
        {
            Log.Error($"Orange routine failed with exception: {e.Message}");
            ReportError("Orange failed", e);
        }
        
        
    }



    private async Task PickUpOranges(CancellationToken cancellationToken)
    {
        var oranges = await MLGather.FetchNextResult();
            if (oranges.Success == true)
            {
                var window = MLGather.ActiveWindow.ClientRect;

                foreach (var prediction in oranges.Predictions)
                {
                    var centerPoint = ConvertToOriginalCoordinates(prediction.Rectangle, window);
                    await SendBackMouse( centerPoint);
                    await Task.Delay(_config.CastSpeed * 50);
                }
            }
    }

    private async Task WaitForLKM(CancellationToken cancellationToken)
    {
        var button = await Policy
            .HandleResult<bool?>(x => x.HasValue && !x.Value)
            .WaitAndRetryForeverAsync(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return x switch
                {
                    <= 30 => TimeSpan.FromMilliseconds(200),
                    _ => throw new InvalidStateException($"Failed to find lkm after {x} attempts")
                };
            })
            .ExecuteAsync(async (token) =>
            {
                token.ThrowIfCancellationRequested();
                var result = await ImageLkm.FetchNextResult().TimeoutAfter(TimeSpan.FromSeconds(1));
                if (result is {Success: true})
                {
                    return result.Success;
                }
                else
                {
                    return result.Success;
                }
            }, cancellationToken);
    }
    
    

    
    
    
    
    
    
    
    
}
