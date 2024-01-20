using System.Reactive.Disposables;
using EyeAuras.OpenCVAuras.Scaffolding;
using Polly;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{

    private readonly SerialDisposable activeCaptchaAnchors;

    private async Task StartCaptcha()
    {
        try
        {
            using var captchaAnchors = new CompositeDisposable().AssignTo(activeCaptchaAnchors);


            var cancellationTokenSource = new CancellationTokenSource();
            Disposable.Create(() => cancellationTokenSource.Cancel()).AddTo(captchaAnchors);
            Disposable.Create(() => Captcha = false).AddTo(captchaAnchors);
            this.WhenAnyValue(x => x.Captcha)
                .Where(x => !x)
                .Subscribe(() => cancellationTokenSource.Cancel())
                .AddTo(captchaAnchors);

            
            DifY = WinExists.ActiveWindow.DwmFrameBounds.Height - WinExists.ActiveWindow.ClientRect.Height;
            
            AuraTree.Aura["ImageE"] = CalculateTargetRectangle(0.0604f, 0.0389f, 50, 50);
            AuraTree.Aura["ImageCaptcha"] = CalculateTargetRectangle(0.5021f, 0.9306f, 160, 160);
            
            ReportNotification("Starting captcha");
            


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
                    token.ThrowIfCancellationRequested();
                    await PickUpCaptcha(token);

                    throw new InvalidStateException($"Error for loop captcha");




                }, cancellationTokenSource.Token);

        }
        catch (OperationCanceledException)
        {
            Log.Warn("Captcha routine cancelled");
        }
        catch (Exception e)
        {
            Log.Error($"Captcha routine failed with exception: {e.Message}");
            ReportError("Captcha failed", e);
        }


    }


}