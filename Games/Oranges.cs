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

            
            ReportNotification("Starting oranges");
        

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
                    await Task.Delay(1000);
                    await CheckStatus();
                    if(cheatIntegration) await SendSecret("F17");
                    await Task.Delay(2500);
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
    
    private async Task WaitForSuccess(CancellationToken cancellationToken)
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

    /*private async Task WaitForE(CancellationToken cancellationToken)
    {

        
        
        var button = await Policy
            .HandleResult<bool?>(x => x.HasValue && !x.Value)
            .WaitAndRetryForeverAsync(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (x <= 50)
                {
                    return TimeSpan.FromMilliseconds(200);
                }
                else
                {
                    if(cheatIntegration) SendSecret("F17");
                    
                    throw new InvalidStateException($"Failed to find button E {x} attempts");
                }
            })
            .ExecuteAsync(async (token) =>
            {
                token.ThrowIfCancellationRequested();
                var result = await ImagebuttonE.FetchNextResult().TimeoutAfter(TimeSpan.FromMilliseconds(200));
                if (result is {Success: true})
                {
                    await SendKeyBack("E");
                    return result.Success;
                }
                else
                {
                    
                    return result.Success;
                }
            }, cancellationToken);
        
    }*/
    
    
    private async Task TeleportAndSellOrage()
    {
        var button1 = CalculateTargetRectangle(0.5775f, 0.8768f , 1 ,1);
        var button2 = CalculateTargetRectangle(0.5463f, 0.1394f, 1, 1);
        
        await SendSecret("F19");
        await Task.Delay(5000);
        await SendKeyBack("E");
        await Task.Delay(1500);
        await SendBackMouse(new Point(button1.X, button1.Y));
        await Task.Delay(1500);
        await SendBackMouse(new Point(button2.X, button2.Y));
        await Task.Delay(1500);
        await SellAllTrash(); // SELL ALL TRASH
        await SendKeyBack("Esc");
        await Task.Delay(500);
        await SendSecret("F18");
        await Task.Delay(5000);

    }
    
    
    
    /*private async Task WaitForE(CancellationToken cancellationToken)
    {
       
        var button = await Policy
            .HandleResult<bool?>(x => x.HasValue && !x.Value)
            .WaitAndRetryForeverAsync(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return x switch
                {
                    <= 50 => TimeSpan.FromMilliseconds(200),
                    _ => throw new InvalidStateException($"Failed to find button E after {x} attempts")
                };
            })
            .ExecuteAsync(async (token) =>
            {
                token.ThrowIfCancellationRequested();
                Log.Info($"Cancel token : {token.IsCancellationRequested}");
                var result = await buttonE.FetchNextResult().TimeoutAfter(TimeSpan.FromSeconds(1));
                if (result is {Success: true})
                {
                    await SendKey("E");
                    return result.Success;
                }
                else
                {
                    return result.Success;
                }
            }, cancellationToken);
        
    }*/
}
