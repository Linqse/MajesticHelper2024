using EyeAuras.OpenCVAuras.Scaffolding;
using Polly;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    
    private async Task SellAllTrash()
    {
        var sell1 = CalculateTargetRectangle(0.2784f,0.3372f, 1, 1);
        var sell2 = CalculateTargetRectangle(0.2784f,0.5306f, 1, 1);
        var sell3 = CalculateTargetRectangle(0.2768f, 0.7239f, 1,1);
        var sell4 = CalculateTargetRectangle(0.2768f,0.9101f , 1,1 );
        var sell5 = CalculateTargetRectangle(0.6363f , 0.3390f, 1, 1);
        var sell6 = CalculateTargetRectangle(0.6363f,0.5306f , 1,1);
        var sell7 = CalculateTargetRectangle(0.6363f,0.7239f , 1 ,1);
        
        await SendBackMouse(new Point(sell1.X, sell1.Y));
        await Task.Delay(1500);
        await SendBackMouse(new Point(sell2.X, sell2.Y));
        await Task.Delay(1500);
        await SendBackMouse(new Point(sell3.X, sell3.Y));
        await Task.Delay(1500);
        await SendBackMouse(new Point(sell4.X, sell4.Y));
        await Task.Delay(1500);
        await SendBackMouse(new Point(sell5.X, sell5.Y));
        await Task.Delay(1500);
        await SendBackMouse(new Point(sell6.X, sell6.Y));
        await Task.Delay(1500);
        await SendBackMouse(new Point(sell7.X, sell7.Y));
        await Task.Delay(1500);
        
    }

    
    
    
    private async Task CheckStatus()
    {
        
        if (!cheatIntegration) return;
        
        var rectangle = CalculateTargetRectangle(0.4068f,0.9491f, 50, 50);
        AuraTree.Aura["CheckStatus"] = rectangle;
        
        var successTask = ImageSuccess.FetchNextResult();
        var errorTask = ImageError.FetchNextResult();
        var warnTask = ImageWarn.FetchNextResult();
        
        await Task.WhenAll(successTask, errorTask, warnTask);

        if (successTask.Result.Success == true) return;
        Log.Info($"Error task : {errorTask.Result.Success}");
        if (errorTask.Result.Success == true)
        {
            AuraTree.Aura["TextSearch"] = rectangle with { Width = 400 };
            var result = await TextSearch.FetchNextResult();
            if (result.Text.Contains("свободного места"))
            {
                if (cheatTeleport)
                {
                    if (Orange) await TeleportAndSellOrage();
                    
                    
                }
            }

            if (result.Text.Contains("сейчас рубит"))
            {
                if (Lumber) await SendKeyBack("F24");
            }
        }
    }
    
    private async Task WaitForE(CancellationToken cancellationToken)
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
                    if(cheatIntegration) SendSecret("F24");
                    
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
        
    }
    
    private async Task WaitForRange(CancellationToken cancellationToken)
    {
            
        
        var button = await Policy
            .HandleResult<bool?>(x => x.HasValue && !x.Value)
            .WaitAndRetryForeverAsync(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (x <= 30)
                {
                    return TimeSpan.FromMilliseconds(200);
                }
                else
                {
                    throw new InvalidStateException($"Failed to find range {x} attempts");
                }
            })
            .ExecuteAsync(async (token) =>
            {
                token.ThrowIfCancellationRequested();
                var result = await ImageRange.FetchNextResult().TimeoutAfter(TimeSpan.FromMilliseconds(200));
                return result.Success;
            }, cancellationToken);
        
    }
    
    private async Task PickUpRange(CancellationToken cancellationToken)
    {
            
        
        var button = await Policy
            .HandleResult<bool?>(x => x.HasValue && x.Value)
            .WaitAndRetryForeverAsync(x =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (x <= 100)
                {
                    return TimeSpan.FromMilliseconds(200);
                }
                else
                {
                    throw new InvalidStateException($"To long {x} attempts");
                }
            })
            .ExecuteAsync(async (token) =>
            {
                token.ThrowIfCancellationRequested();
                var result = await ImageRange.FetchNextResult().TimeoutAfter(TimeSpan.FromMilliseconds(200));
                if (result is {Success: true})
                {
                    SendKey("MouseLeft");
                    return result.Success;
                }
                else
                {
                    return result.Success;
                }
            }, cancellationToken);
        
    }


    private async Task PickUpML(CancellationToken cancellationToken)
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
                    var result = await MLGather.FetchNextResult();
                    if (result.Success == true)
                    {
                        var window = MLGather.ActiveWindow.WindowRect;
                        var sw = new Stopwatch();
                        sw.Start();
                        foreach (var prediction in result.Predictions)
                        {
                            var centerPoint = ConvertToOriginalCoordinates(prediction.Rectangle, window);
                            
                            await SendBackMouse(centerPoint with{ Y = centerPoint.Y - DifY});
                            await Task.Delay(_config.CastSpeed * 50 + 200);
                            Log.Info($"Stopwatch time : {sw.ElapsedMilliseconds}");
                            sw.Restart();
                        }
                        sw.Stop();
                        return result.Success;
                    }
                    
                    return result.Success; 
                    
                }, cancellationToken);
        
    }
    
}