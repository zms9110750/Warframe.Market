using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using ZiggyCreatures.Caching.Fusion;

namespace WarframeMarketQueryWPF.Shared;

public class CarryList : ComponentBase
{
    bool init;
    [Inject]
    IFusionCache Fusion { get; set; } = default!;
    public List<string> Tags { get; set; } = [];
    [Parameter]
    [AllowNull]
    public string CacheKey { get => field ?? GetType().FullName!; set; }

    [CascadingParameter(Name = "CanWrite")]
    public bool CanWrite
    {
        get; set
        {
            field = value;
            if (!CanWrite && init)
            {
                Fusion.Set(CacheKey, Tags);
            }
        }
    }
    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Tags.AddRange(await Fusion.GetOrDefaultAsync<List<string>>(CacheKey) ?? []);
        init = true;
    }
}
