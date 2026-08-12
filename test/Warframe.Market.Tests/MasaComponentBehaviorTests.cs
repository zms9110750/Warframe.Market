using Bunit;
using Masa.Blazor;
using Xunit;
using Xunit.Abstractions;

namespace zms9110750.Warframe.Market.Tests;

/// <summary>
/// Masa 组件行为测试：验证 MButton 的事件绑定机制与点击派发。
/// 目的：确认 MButton.OnClick 是否走 Blazor 标准事件管道（bUnit 可触发），
/// 并观察渲染出的 DOM 结构（onclick / __internal_* 属性）以及点击伴随的 JS 调用，
/// 以定位 PhotinoX 下的失效触发机制。
/// </summary>
public class MasaComponentBehaviorTests
{
    private readonly ITestOutputHelper _output;

    public MasaComponentBehaviorTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static BunitContext CreateCtx()
    {
        var ctx = new BunitContext();
        ctx.Services.AddMasaBlazor();
        // Loose 模式：未配置的 JS 调用返回默认值，并记录 Invocations 供断言
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        return ctx;
    }

    [Fact]
    public async Task MButton_renders_expected_structure()
    {
        await using var ctx = CreateCtx();
        var cut = ctx.Render<MButton>(p => p
            .Add(m => m.Small, true)
            .AddChildContent("搜索"));

        // MButton 应渲染出 <button>，且带 Masa 的 ripple 指令属性（JS 层波纹效果的钩子）
        Assert.NotNull(cut.Find("button"));
        Assert.Contains("ripple", cut.Markup);
        Assert.Contains("m-btn", cut.Markup);
    }

    [Fact]
    public async Task MIcon_renders_mdi_class_and_content()
    {
        // 左侧抽屉图标显示为文字的问题：先确认 MIcon 渲染的 class 是否正确。
        // 正确应渲染 <i class="mdi mdi-xxx">mdi-xxx</i>（:before 由 mdi 字体 css 提供 glyph）。
        // 若 class 正确 → 问题在 PhotinoX 下 mdi css/字体加载；若 class 缺失 → MasaBlazor Icons 配置。
        await using var ctx = CreateCtx();
        var cut = ctx.Render<MIcon>(p => p.AddChildContent("mdi-account-search"));

        _output.WriteLine("MIcon markup: " + cut.Markup);
        Assert.Contains("mdi-account-search", cut.Markup);
        Assert.Contains("mdi ", cut.Markup); // class 应含 "mdi " 前缀（mdi mdi-account-search）
    }

    [Fact]
    public async Task MButton_OnClick_fires_via_standard_blazor_event()
    {
        await using var ctx = CreateCtx();
        var clicked = false;
        var cut = ctx.Render<MButton>(p => p
            .Add(m => m.OnClick, () => clicked = true)
            .AddChildContent("搜索"));

        // bUnit 触发 button 点击：走的是 Blazor 标准 @onclick 事件管道
        cut.Find("button").Click();

        Assert.True(clicked);
    }

    [Fact]
    public async Task MButton_click_invokes_masa_js_in_addition_to_blazor_event()
    {
        // 机制验证：MButton 点击不仅在 Blazor 事件管道派发 OnClick，
        // 还伴随 Masa 的 JS interop 调用（ripple/transition 等）。
        // PhotinoX 下若这些 JS 调用失败/竞态，点击链会整体断裂。
        await using var ctx = CreateCtx();
        var clicked = false;
        var cut = ctx.Render<MButton>(p => p
            .Add(m => m.OnClick, () => clicked = true)
            .AddChildContent("搜索"));

        cut.Find("button").Click();

        // 点击确实触发了 Blazor 回调
        Assert.True(clicked);

        // 点击伴随的 JS 调用（记录在 bUnit Invocations）——揭示 PhotinoX 下点击链的 JS 依赖
        var jsCalls = ctx.JSInterop.Invocations.Select(i => i.Identifier).Distinct().ToArray();
        Assert.NotEmpty(jsCalls);
        _output.WriteLine("MButton 点击触发的 JS 调用: " + string.Join(", ", jsCalls));
    }

    [Fact]
    public async Task Native_button_OnClick_fires()
    {
        // 对照：原生 button 的 @onclick（Razor 编译产物）在 bUnit 中同样可触发
        await using var ctx = CreateCtx();
        var clicked = false;
        var cut = ctx.Render<NativeButtonProbe>(p => p
            .Add(m => m.OnClicked, () => clicked = true));

        cut.Find("button").Click();

        Assert.True(clicked);
    }
}

/// <summary>原生 button 探针组件：与 MButton 对照</summary>
public class NativeButtonProbe : Microsoft.AspNetCore.Components.ComponentBase
{
    [Microsoft.AspNetCore.Components.Parameter]
    public Microsoft.AspNetCore.Components.EventCallback OnClicked { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder __builder)
    {
        __builder.OpenElement(0, "button");
        __builder.AddAttribute(1, "onclick", Microsoft.AspNetCore.Components.EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.MouseEventArgs>(this, OnClicked));
        __builder.AddContent(2, "搜索");
        __builder.CloseElement();
    }
}
