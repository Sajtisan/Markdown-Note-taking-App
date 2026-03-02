using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;
using System;

namespace MarkdownNotesClient.Controls;

public class BackgroundManager : Control
{
    private float _time = 0;
    private readonly DispatcherTimer _timer;
    private SKRuntimeEffect? _currentEffect;

    public BackgroundManager()
    {
        // 60 FPS Timer
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _timer.Tick += (s, e) =>
        {
            if (!this.IsVisible) return; // GPU Kill-switch
            _time += 0.016f;
            InvalidateVisual(); // Redraw
        };
    }

    // Call this to swap themes on the fly!
    public void LoadShader(string sksl)
    {
        _currentEffect = SKRuntimeEffect.CreateShader(sksl, out string errors);
        if (_currentEffect == null)
        {
            Console.WriteLine("SHADER ERROR: " + errors);
        }
        _time = 0; // Reset the animation timer for the new theme
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _timer.Start();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _timer.Stop();
    }

    public override void Render(DrawingContext context)
    {
        if (_currentEffect != null)
        {
            context.Custom(new ShaderDrawOperation(Bounds, _time, _currentEffect));
        }
    }
}

public class ShaderDrawOperation : ICustomDrawOperation
{
    private readonly float _time;
    private readonly SKRuntimeEffect _effect;
    public Rect Bounds { get; }

    public ShaderDrawOperation(Rect bounds, float time, SKRuntimeEffect effect)
    {
        Bounds = bounds;
        _time = time;
        _effect = effect;
    }

    public void Dispose() { }
    public bool HitTest(Point p) => false;
    public bool Equals(ICustomDrawOperation? other) => false;

    public void Render(ImmediateDrawingContext context)
    {
        var leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
        if (leaseFeature == null) return;

        using var lease = leaseFeature.Lease();
        var canvas = lease.SkCanvas;

        var inputs = new SKRuntimeEffectUniforms(_effect);
        inputs["u_time"] = _time;
        inputs["u_resolution"] = new[] { (float)Bounds.Width, (float)Bounds.Height };

        using var shader = _effect.ToShader(inputs);
        using var paint = new SKPaint { Shader = shader };

        canvas.DrawRect(SKRect.Create(0, 0, (float)Bounds.Width, (float)Bounds.Height), paint);
    }
}