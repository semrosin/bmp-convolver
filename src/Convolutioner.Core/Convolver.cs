namespace Convolutioner.Core;

public static class Convolver
{
    public static GrayImage ConvolveSequential(GrayImage input, Kernel kernel, BorderMode borderMode)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(kernel);

        var output = new float[input.Pixels.Length];
        ConvolveInternal(input, output, kernel, borderMode);
        return new GrayImage(input.Width, input.Height, output);
    }

    private static void ConvolveInternal(GrayImage input, float[] output, Kernel kernel, BorderMode borderMode)
    {
        var src = input.Pixels;

        var kernelWidth = kernel.Width;
        var kernelHeight = kernel.Height;
        var kernelCenterX = kernel.CenterX;
        var kernelCenterY = kernel.CenterY;
        var weights = kernel.Weights;

        for (var y = 0; y < input.Height; y++)
        for (var x = 0; x < input.Width; x++)
        {
            float sum = 0f;

            for (var ky = 0; ky < kernelHeight; ky++)
            {
                var iy = y + (ky - kernelCenterY);
                if (borderMode == BorderMode.Zero) continue;
                if (borderMode == BorderMode.Clamp) iy = iy < 0 ? 0 : (input.Height - 1);

                var srcRow = iy * input.Width;
                var kernelRow = ky * kernelWidth;

                for (var kx = 0; kx < kernelWidth; kx++)
                {
                    var ix = x + (kx - kernelCenterX);
                    if (borderMode == BorderMode.Zero) continue;
                    if (borderMode == BorderMode.Clamp) ix = ix < 0 ? 0 : (input.Width - 1);

                    var pixel = src[srcRow + ix];
                    var weight = weights[kernelRow + kx];
                    sum += pixel * weight;
                }
            }

            output[(y * input.Width) + x] = sum;
        }
    }
}

