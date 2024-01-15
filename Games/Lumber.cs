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
                    if (cheatIntegration)
                    {
                        await Task.Delay(500);
                        await CheckStatus();
                    }
                    token.ThrowIfCancellationRequested();
                    await WaitForRange(token);
                    token.ThrowIfCancellationRequested();
                    await PickUpRange(token);
                    token.ThrowIfCancellationRequested();
                    await PickUpML(token);
                    token.ThrowIfCancellationRequested();
                    if(cheatIntegration) await SendSecret("F24");
                    await Task.Delay(2500);
                    throw new InvalidStateException($"Error for loop lumber");
                    /*
                    await WaitForLKM(token);
                    token.ThrowIfCancellationRequested();
                    await PickUpOranges(token);
                    token.ThrowIfCancellationRequested();
                    await Task.Delay(1000);
                    await CheckStatus();
                    await SendSecret("F17");
                    await Task.Delay(2500);
                    throw new InvalidStateException($"Error for loop oranges");*/




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
                    Log.Info($"Window : {window}");
                    /*foreach (var prediction in result.Predictions)
                    {
                        var maskBound = prediction.Mask.MaskBounds;
                        var centerOfMask = FindMaskCenter(prediction.Mask.MaskImage);

                        var centerPoint = ConvertToOriginalCoordinates(prediction.Mask.MaskBounds, window);

                        Log.Info($"\n Window : {window} \n Find mask : {centerOfMask} \n adjustedCenterPoint : {new Point((int)(centerOfMask.X + maskBound.X), (int)(centerOfMask.Y + maskBound.Y))} \n Center Point {centerPoint}");
                        await SendKey("MouseLeft", centerPoint);
                        await Task.Delay(_config.CastSpeed * 50 + 200);
                    }*/
                    foreach (var prediction in result.Predictions)
                    {
                        var centerPoint = ConvertToOriginalCoordinates(prediction.Rectangle, window);
                        await SendKey("MouseLeft", centerPoint);
                        await Task.Delay(_config.CastSpeed * 50 + 200);
                    }


                    return result.Success;
                }
                    
                return result.Success; 
                    
            }, cancellationToken);
        
    }
    
    public Point FindMaskCenter(Image<Gray, float> mask)
    {
        var largestContour = FindLargestContour(mask);
        if (largestContour != null)
        {
            var moments = CvInvoke.Moments(largestContour);
            int centerX = (int)(moments.M10 / moments.M00);
            int centerY = (int)(moments.M01 / moments.M00);
            Log.Info($"centerX : {centerX}, centerY : {centerY}");
            return new Point(centerX, centerY);
        }

        return Point.Empty;
    }

    private VectorOfPoint FindLargestContour(Image<Gray, float> mask)
    {
        VectorOfPoint largestContour = null;
        double largestArea = 0;
        // Конвертируем Image<Gray, float> в Image<Gray, byte>
        using (var temp = mask.Convert<Gray, byte>())
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hierachy = new Mat();

            CvInvoke.FindContours(temp, contours, hierachy, Emgu.CV.CvEnum.RetrType.External, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxSimple);
            for (int i = 0; i < contours.Size; i++)
            {
                double area = CvInvoke.ContourArea(contours[i]);
                if (area > largestArea)
                {
                    largestArea = area;
                    largestContour = contours[i];
                }
            }
        }

        return largestContour;
    }
    
}