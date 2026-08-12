$ErrorActionPreference = 'Stop'
$json = Get-Content (Join-Path $PSScriptRoot '..\..\test\Resources\items\items.json') -Raw -Encoding UTF8 | ConvertFrom-Json
$items = $json.data
$out = New-Object System.Collections.Generic.List[string]
$out.Add("# items.json analysis ($($items.Count) items)")
$out.Add("")

# 1. tag enum
$out.Add("## 1. All tags (count)")
$tagCounts = $items | ForEach-Object { $_.tags } | Where-Object { $_ } | Group-Object | Sort-Object Count -Descending
$out.Add("| tag | items |")
$out.Add("|---|---|")
foreach ($g in $tagCounts) { $out.Add("| $($g.Name) | $($g.Count) |") }
$out.Add("")

# 2. tag combos
$out.Add("## 2. Tag set combinations")
$combos = $items | ForEach-Object {
    if ($_.tags) { ($_.tags | Sort-Object) -join ',' } else { '(no-tags)' }
} | Group-Object | Sort-Object Count -Descending
$out.Add("Total combos: $($combos.Count). Top 50:")
$out.Add("| tag set | items |")
$out.Add("|---|---|")
foreach ($g in ($combos | Select-Object -First 50)) { $out.Add("| $($g.Name) | $($g.Count) |") }
$out.Add("")

# 3. field-non-null combos
$out.Add("## 3. Field presence combos (id/slug/gameRef/tags/i18n-langs)")
$fieldCombos = $items | ForEach-Object {
    $sig = ""
    $sig += if ($_.id) { "id" } else { "-" }
    $sig += if ($_.slug) { ",slug" } else { ",-" }
    $sig += if ($_.gameRef) { ",gameRef" } else { ",-" }
    $sig += if ($_.tags -and $_.tags.Count -gt 0) { ",tags" } else { ",-" }
    $langs = @($_.i18n.PSObject.Properties.Name)
    $sig += ",i18n:$($langs -join '+')"
    $sig
} | Group-Object | Sort-Object Count -Descending
foreach ($g in $fieldCombos) { $out.Add("| $($g.Name) | $($g.Count) |") }
$out.Add("")

# 4. gameRef first 2 segments
$out.Add("## 4. gameRef path prefix (2 segments)")
$refs = $items | ForEach-Object {
    if ($_.gameRef) {
        $p = $_.gameRef.TrimStart('/') -split '/'
        "$($p[0])/$($p[1])"
    } else { '(no-gameRef)' }
} | Group-Object | Sort-Object Count -Descending
$out.Add("| gameRef prefix | items |")
$out.Add("|---|---|")
foreach ($g in $refs) { $out.Add("| $($g.Name) | $($g.Count) |") }
$out.Add("")

# 5. tag x gameRef cross
$out.Add("## 5. Key tag -> gameRef top distribution")
foreach ($t in @('mod', 'prime', 'arcane', 'weapon', 'blueprint', 'relic', 'skin', 'sentinel', 'gear', 'fish', 'gem', 'railjack', 'necramech', 'archwing', 'kavat', 'companion')) {
    $sub = $items | Where-Object { $_.tags -contains $t }
    if (-not $sub) { continue }
    $refs2 = $sub | ForEach-Object {
        if ($_.gameRef) { (($_.gameRef.TrimStart('/') -split '/')[0..1]) -join '/' } else { '(none)' }
    } | Group-Object | Sort-Object Count -Descending | Select-Object -First 6
    $top = ($refs2 | ForEach-Object { "$($_.Name)x$($_.Count)" }) -join '; '
    $out.Add("- **$t** ($($sub.Count)): $top")
}
$out.Add("")

# 6. single-tag items
$out.Add("## 6. Items with exactly one tag")
$singles = $items | Where-Object { $_.tags -and $_.tags.Count -eq 1 } | Group-Object { $_.tags[0] } | Sort-Object Count -Descending
foreach ($g in $singles) { $out.Add("- $($g.Name): $($g.Count)") }
$out.Add("")

$dir = Split-Path -Parent $MyInvocation.MyCommand.Path
$out | Set-Content (Join-Path $dir 'report.md') -Encoding UTF8
"Total items: $($items.Count)"
"tag kinds: $($tagCounts.Count), tag combos: $($combos.Count)"
"field combos: $($fieldCombos.Count)"
"gameRef prefixes: $($refs.Count)"
"Top 12 tags: " + (($tagCounts | Select-Object -First 12 | ForEach-Object { "$($_.Name)=$($_.Count)" }) -join ' ')
"report.md written"
