
using System.Numerics;
using System.Windows.Media;
using EyeAuras.Graphics.ImageEffects;
using EyeAuras.Graphics.Scaffolding;

namespace EyeAuras.Web.Repl.Component;

public partial class Main
{
    private Rectangle CalculateTargetRectangle( float relativeX, float relativeY, int targetWidth, int targetHeight)
    {
        if (WinExists.ActiveWindow != null)
        {
            var dwm = WinExists.ActiveWindow.ClientRect;

            
            int centerX = (int)(dwm.Width * relativeX);
            int centerY = (int)(dwm.Height * relativeY);

            
            int newX = centerX - targetWidth / 2;
            int newY = centerY - targetHeight / 2;

            return new Rectangle(newX, newY, targetWidth, targetHeight);
        }

        return default;
    }
    
    
    private async Task<Point> ConverterImage(RectangleF rect, Rectangle window)
    {

        var targetSize = new Size(window.Width, window.Height);
        var originalSize = new Size(640, 640);
        var vec = new Vector2(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);
        var method = ResizeImageMethod.Letterbox;
        var transformation = method.CalculateTransformation(targetSize, originalSize);

        Matrix3x2.Invert(transformation, out var invertedMatrix);
        
        Vector2 transformedPoint = Vector2.Transform(vec, invertedMatrix);

        
        return new Point((int)transformedPoint.X, (int)transformedPoint.Y);
    }
    
    
    private Point ConvertToOriginalCoordinates(RectangleF predictionRectangle, Rectangle windowBounds)
    {
        const int letterBoxedImageSize = 640;

        
        double scaleX = (double)windowBounds.Width / letterBoxedImageSize;
        double scaleY = (double)windowBounds.Height / letterBoxedImageSize;

        
        double letterBoxingX = 0;
        double letterBoxingY = 0;
        if (windowBounds.Width > windowBounds.Height)
        {
            double aspectRatio = (double)windowBounds.Width / windowBounds.Height;
            double imageAspectRatio = 1.0; 
            if (aspectRatio > imageAspectRatio)
            {
                
                scaleY = scaleX;
                letterBoxingY = (letterBoxedImageSize - (windowBounds.Height / scaleY)) / 2;
            }
            else
            {
                
                scaleX = scaleY;
                letterBoxingX = (letterBoxedImageSize - (windowBounds.Width / scaleX)) / 2;
            }
        }

        
        var originalX = (predictionRectangle.X - letterBoxingX) * scaleX;
        var originalY = (predictionRectangle.Y - letterBoxingY) * scaleY;
        var originalWidth = predictionRectangle.Width * scaleX;
        var originalHeight = predictionRectangle.Height * scaleY;

        
        var centerX = originalX + (originalWidth / 2);
        var centerY = originalY + (originalHeight / 2);

        
        return new Point((int)Math.Round(centerX), (int)Math.Round(centerY));
    }
}