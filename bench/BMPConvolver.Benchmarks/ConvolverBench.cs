using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BMPConvolver.Core;
using BMPConvolver.Core.WorkPartitioning;

namespace BMPConvolver.Benchmarks;

[Config(typeof(Config))]
[HideColumns(Column.RatioSD, Column.AllocRatio, Column.Gen0, Column.Gen1, Column.Gen2)]
public class ConvolutionBench
{
    private sealed class Config : ManualConfig
    {
        public Config()
        {
            AddJob(Job.Default.WithWarmupCount(3).WithIterationCount(15));
        }
    }
    private const int Seed = 1;

    [Params(1024, 2048, 4096)]
    public int Width { get; set; }

    [Params(1024, 2048, 4096)]
    public int Height { get; set; }

    [Params(BorderMode.Zero, BorderMode.Clamp)]
    public BorderMode Border { get; set; }

    public int KernelSize { get; set; } = 3;

    public int Grid { get; set; } = 8;

    private GrayImage _input = null!;
    private Kernel _kernel = null!;

    [GlobalSetup]
    public void Setup()
    {
        _input = RandomImage(Width, Height);
        _kernel = RandomKernelOddFrom(KernelSize, KernelSize);
    }

    [Benchmark(Baseline = true, Description = "Sequential")]
    public GrayImage Sequential()
        => Convolver.ConvolveSequential(_input, _kernel, Border);

    [Benchmark(Description = "Parallel.Pixels")]
    public GrayImage ParallelByPixels()
        => Convolver.ConvolveParallel(_input, _kernel, Border, PartitioningMode.Pixels, gridX: Grid, gridY: Grid);

    [Benchmark(Description = "Parallel.Rows")]
    public GrayImage ParallelByRows()
        => Convolver.ConvolveParallel(_input, _kernel, Border, PartitioningMode.Rows, gridX: Grid, gridY: Grid);

    [Benchmark(Description = "Parallel.Columns")]
    public GrayImage ParallelByColumns()
        => Convolver.ConvolveParallel(_input, _kernel, Border, PartitioningMode.Columns, gridX: Grid, gridY: Grid);

    [Benchmark(Description = "Parallel.Grid")]
    public GrayImage ParallelByGrid()
        => Convolver.ConvolveParallel(_input, _kernel, Border, PartitioningMode.Grid, gridX: Grid, gridY: Grid);

    private static GrayImage RandomImage(int width, int height)
    {
        var rnd = new Random(Seed);
        var pixels = new float[width * height];
        for (var i = 0; i < pixels.Length; i++)
            pixels[i] = (float)rnd.NextDouble();
        return new GrayImage(width, height, pixels);
    }

    private static Kernel RandomKernelOddFrom(int widthRequested, int heightRequested)
    {
        var width = widthRequested <= 1 ? 1 : (widthRequested % 2 == 1 ? widthRequested : widthRequested + 1);
        var height = heightRequested <= 1 ? 1 : (heightRequested % 2 == 1 ? heightRequested : heightRequested + 1);

        var rnd = new Random(Seed);
        var weights = new float[width * height];
        for (var i = 0; i < weights.Length; i++)
            weights[i] = ((float)rnd.NextDouble() - 0.5f) * 0.5f;
        return new Kernel(width, height, width / 2, height / 2, weights);
    }
}

