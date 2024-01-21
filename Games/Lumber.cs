using System.Reactive.Disposables;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using EyeAuras.OpenCVAuras.Scaffolding;
using Polly;
namespace EyeAuras.Web.Repl.Component;

public partial class Main
{

    private readonly SerialDisposable activeLumberAnchors;
     
    private async Task StartLumber()
    {   
        try
        {
            using var lumberAnchors = new CompositeDisposable().AssignTo(activeLumberAnchors);
            

            var cancellationTokenSource = new CancellationTokenSource(); 
            Disposable.Create(() => cancellationTokenSource.Cancel()).AddTo(lumberAnchors); 
            Disposable.Create(() => Lumber = false).AddTo(lumberAnchors); 
            this.WhenAnyValue(x => x.Lumber)
                .Where(x => !x)
                .Subscribe(() => cancellationTokenSource.Cancel())
                .AddTo(lumberAnchors);

            
            ReportNotification("Starting lumber");

            AuraTree.Aura["ImageE"] = CalculateTargetRectangle(0.0604f, 0.0389f, 50, 50);
            AuraTree.Aura["ImageRange"] = CalculateTargetRectangle(0.9349f, 0.9287f, 200, 50);
            
            DifY = WinExists.ActiveWindow.DwmFrameBounds.Height - WinExists.ActiveWindow.ClientRect.Height;
            MLLumber.Refresh();
            
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
                    await PickUpLumber(token);

                    await Task.Delay(2500);
                    throw new InvalidStateException($"Error for loop lumber");
                    




                }, cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            Log.Warn("Lumber routine cancelled");
        }
        catch (Exception e)
        {
            Log.Error($"Lumber routine failed with exception: {e.Message}");
            ReportError("Lumber failed", e);
        }
    }
    
    
    private async Task PickUpLumber(CancellationToken cancellationToken)
    {
       
        var button = await Policy
            .HandleResult<bool?>(x => x.HasValue && x.Value)
            .WaitAndRetryForeverAsync(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (x <= 3)
                {
                    return TimeSpan.FromMilliseconds(500);
                }
                else
                {
                    throw new InvalidStateException($"To long {x} attempts");
                }
            })
            .ExecuteAsync(async (token) =>
            {
                token.ThrowIfCancellationRequested();
                var result = await MLLumber.FetchNextResult();
                if (result.Success == true)
                {
                    var window = MLLumber.ActiveWindow.ClientRect;
                    
                    foreach (var prediction in result.Predictions)
                    {
                        //var centerPoint = ConvertToOriginalCoordinates(prediction.Rectangle, window);
                        var centerPoint = await ConverterImage(prediction.Rectangle, window);
                        await SendBackMouse(centerPoint with{ Y = centerPoint.Y});
                        await Task.Delay(_config.CastSpeed * 50 + 300);
                    }


                    return result.Success;
                }
                    
                return result.Success; 
                    
            }, cancellationToken);
        
    }

}