using System.Reactive.Disposables;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using EyeAuras.OpenCVAuras.Scaffolding;
using Polly;
namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    private readonly SerialDisposable activeFishAnchors;
    
    private async Task StartFish()
    {   
        try
        {
            using var fishAnchors = new CompositeDisposable().AssignTo(activeFishAnchors);
            

            var cancellationTokenSource = new CancellationTokenSource(); 
            Disposable.Create(() => cancellationTokenSource.Cancel()).AddTo(fishAnchors); 
            Disposable.Create(() => Fish = false).AddTo(fishAnchors); 
            this.WhenAnyValue(x => x.Fish)
                .Where(x => !x)
                .Subscribe(() => cancellationTokenSource.Cancel())
                .AddTo(fishAnchors);

            
            ReportNotification("Starting fishing");
            
            AuraTree.Aura["ImageAD"] = CalculateTargetRectangle(0.1375f, 0.0380f, 100, 50);
            AuraTree.Aura["ImageRange"] = CalculateTargetRectangle(0.9349f, 0.9287f, 200, 50);
            
            DifY = WinExists.ActiveWindow.DwmFrameBounds.Height - WinExists.ActiveWindow.ClientRect.Height;
            MLFish.Refresh();
            
            await StartFishPreset();
            
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
                    SendKeyBack("E");
                    token.ThrowIfCancellationRequested();
                    await WaitForAD(token);
                    token.ThrowIfCancellationRequested();
                    await FishingTime(token);
                    token.ThrowIfCancellationRequested();
                    await PickUpFishRange(token);
                    token.ThrowIfCancellationRequested();
                    await CheckStatus();
                    await Task.Delay(2000);
                    throw new InvalidStateException($"Error for loop fishing");





                }, cancellationTokenSource.Token);
            
        }
        catch (OperationCanceledException)
        {
            Log.Warn("Fishing routine cancelled");
        }
        catch (Exception e)
        {
            Log.Error($"Fishing routine failed with exception: {e.Message}");
            ReportError("Fishing failed", e);
        }
    }
    
    private async Task WaitForAD(CancellationToken cancellationToken)
    {

        var button = await Policy
            .HandleResult<bool?>(x => x.HasValue && !x.Value)
            .WaitAndRetryForeverAsync(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return x switch
                {
                    <= 15 => TimeSpan.FromMilliseconds(2000),
                    <= 35 => TimeSpan.FromMilliseconds(1000),
                    <= 100 => TimeSpan.FromMilliseconds(200),
                    _ => throw new InvalidStateException($"Failed to find fish after {x} attempts")
                };
            })
            .ExecuteAsync(async (token) =>
            {
                token.ThrowIfCancellationRequested();
                var result = await ImageAD.FetchNextResult().TimeoutAfter(TimeSpan.FromMilliseconds(200));
                return result.Success;
                
            }, cancellationToken);
        
    }

    private async Task FishingTime(CancellationToken cancellationToken)
    {
        string lastKeyPressed = string.Empty;
        RectangleF saveLocation = new RectangleF();
        var fistingTime = await Policy
            .HandleResult<bool?>(x => x.HasValue && !x.Value)
            .WaitAndRetryForeverAsync(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return x switch
                {
                    <= 50 => TimeSpan.FromMilliseconds(500),
                    _ => throw new InvalidStateException($"Failed to catch fish after {x} attempts")
                };
            })
            .ExecuteAsync(async (token) =>
            {
                token.ThrowIfCancellationRequested();
                var result = await ImageRange.FetchNextResult().TimeoutAfter(TimeSpan.FromMilliseconds(200));
                if (result.Success == true)
                {
                    await SendKey(lastKeyPressed, inputEventType : "KeyUp");
                    return result.Success;
                }
                else
                {
                    var ml = await MLFish.FetchNextResult();
                    if (ml.Success == true)
                    {
                        var prediction = ml.Predictions.FirstOrDefault();
                        if (prediction != null) 
                        {
                            var newLocation = prediction.Rectangle;
                            if (newLocation.X > saveLocation.X && lastKeyPressed != "A")
                            {
                                if (lastKeyPressed == "D")
                                {
                                    await SendKey("D", inputEventType : "KeyUp");
                                }

                                await SendKey("A", inputEventType :"KeyDown");
                                lastKeyPressed = "A";
                            }
                            else if (newLocation.X < saveLocation.X && lastKeyPressed != "D")
                            {
                                if (lastKeyPressed == "A")
                                {
                                    await SendKey("A", inputEventType :"KeyUp");
                                }

                                await SendKey("D", inputEventType :"KeyDown");
                                lastKeyPressed = "D";
                            }

                            saveLocation = newLocation;
                            await Task.Delay(200); 
                        }
                    }
                    return result.Success;
                }
                
            }, cancellationToken);
    }
    
    
    private async Task StartFishPreset()
    {
        SendKeyBack("I");
        await Task.Delay(2000);
        SendKeyBack("I");
        await Task.Delay(500);
    }
}