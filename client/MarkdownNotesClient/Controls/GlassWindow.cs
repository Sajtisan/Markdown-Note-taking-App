using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace MarkdownNotesClient.Controls;

public class GlassWindow : Window 
{
    protected override Type StyleKeyOverride => typeof(GlassWindow);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        var titleBar = e.NameScope.Find<Grid>("PART_TitleBar");
        if (titleBar != null)
        {
            titleBar.PointerPressed += (s, ev) => BeginMoveDrag(ev);
        }
        var minBtn = e.NameScope.Find<Button>("PART_MinBtn");
        if (minBtn != null) 
            minBtn.Click += (s, ev) => WindowState = WindowState.Minimized;

        var maxBtn = e.NameScope.Find<Button>("PART_MaxBtn");
        if (maxBtn != null) 
            maxBtn.Click += (s, ev) => WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;

        var closeBtn = e.NameScope.Find<Button>("PART_CloseBtn");
        if (closeBtn != null) 
            closeBtn.Click += (s, ev) => Close();
    }
}