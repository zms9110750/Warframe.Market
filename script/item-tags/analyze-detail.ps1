$ErrorActionPreference = 'Stop'
$dir = Join-Path $PSScriptRoot '..\..\test\Resources\items\detail'
$names = @{ 'creeping_bullseye' = 'mod(步枪)'; 'redeemer_prime_blade' = 'prime部件'; 'ayatan_orta_sculpture' = '安魂'; 'arcane_barrier' = '赋能'; 'requiem_iv_relic' = '遗物'; 'secura_dual_cestra' = '武器' }
$out = New-Object System.Collections.Generic.List[string]
$out.Add("# items detail field matrix (6 representative)")
$out.Add("")
foreach ($s in ($names.Keys)) {
    $j = Get-Content (Join-Path $dir "$s.json") -Raw -Encoding UTF8 | ConvertFrom-Json
    $d = $j.data
    $out.Add("## $s ($($names[$s]))")
    $out.Add("- tags: $(($d.tags) -join ',')")
    $out.Add("- rarity: $($d.rarity) | maxRank: $($d.maxRank) | tradingTax: $($d.tradingTax) | tradable: $($d.tradable)")
    $other = $d.PSObject.Properties.Name | Where-Object { $_ -notin @('id','slug','gameRef','tags','rarity','maxRank','tradingTax','tradable','i18n','urlName','setRoot','setParts','quantityInSet','modSetValue','isItemSet','wikiaThumbnail','wikiaUrl','thumbnail','icon','iconFormat','thumb','subtypes','vaulted','ducats','maxAmberStars','maxCyanStars','baseEndo','endoMultiplier','bulkTradable','maxCharges','vosfor','reqMasteryRank') }
    if ($other) { $out.Add("- other fields: $(($other | ForEach-Object { "$_=$($d.$_)" }) -join '; ')") }
    foreach ($f in @('subtypes','vaulted','ducats','maxAmberStars','maxCyanStars','baseEndo','endoMultiplier','bulkTradable','maxCharges','vosfor','reqMasteryRank','urlName','setRoot','setParts','quantityInSet')) {
        $has = $d.PSObject.Properties.Name -contains $f
        if ($has) { $v = $d.$f; if ($v -is [array]) { $v = ($v -join ',') }; $out.Add("- $f = $v") }
    }
    $out.Add("")
}
$out | Set-Content (Join-Path $PSScriptRoot 'detail-report.md') -Encoding UTF8
Get-Content (Join-Path $PSScriptRoot 'detail-report.md') -Encoding UTF8
