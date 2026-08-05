# Fetch real JSON responses from the Warframe.Market public API and back them up
# into test/Resources/<resource>/ folders. Tests read fake data from these files.
# NOTE: public API rate limit is 3 req/s; script keeps a 600ms delay between calls.
$ErrorActionPreference = 'Stop'
$base = 'https://api.warframe.market'
$headers = @{ 'User-Agent' = 'Warframe.Market-Tests/1.0 (data backup)' }
$testRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$resRoot = Join-Path $testRoot 'Resources'

function Save-Json {
    param([string]$Rel, [string]$Url)
    $path = Join-Path $resRoot $Rel
    $dir = Split-Path -Parent $path
    New-Item -ItemType Directory -Force -Path $dir | Out-Null
    Invoke-WebRequest -Uri "$base$Url" -Headers $headers -UseBasicParsing -OutFile $path
    Write-Host "OK  $Rel"
    Start-Sleep -Milliseconds 600
}

# ===== list endpoints first (used later to extract slugs) =====
Save-Json 'items/items.json' '/v2/items'
$items = (Get-Content -Raw (Join-Path $resRoot 'items/items.json') | ConvertFrom-Json).data
$itemSlug = $items[0].slug

Save-Json 'rivens/weapons.json' '/v2/riven/weapons'
$rivenSlug = (Get-Content -Raw (Join-Path $resRoot 'rivens/weapons.json') | ConvertFrom-Json).data[0].slug

Save-Json 'liches/weapons.json' '/v2/lich/weapons'
$lichSlug = (Get-Content -Raw (Join-Path $resRoot 'liches/weapons.json') | ConvertFrom-Json).data[0].slug

Save-Json 'sisters/weapons.json' '/v2/sister/weapons'
$sisterSlug = (Get-Content -Raw (Join-Path $resRoot 'sisters/weapons.json') | ConvertFrom-Json).data[0].slug

# ===== remaining public endpoints =====
Save-Json 'versions/versions.json' '/v2/versions'
Save-Json 'items/item.json' "/v2/item/$itemSlug"
Save-Json 'items/item-set.json' "/v2/item/$itemSlug/set"
Save-Json 'rivens/weapon.json' "/v2/riven/weapon/$rivenSlug"
Save-Json 'rivens/attributes.json' '/v2/riven/attributes'
Save-Json 'liches/weapon.json' "/v2/lich/weapon/$lichSlug"
Save-Json 'liches/ephemeras.json' '/v2/lich/ephemeras'
Save-Json 'liches/quirks.json' '/v2/lich/quirks'
Save-Json 'sisters/weapon.json' "/v2/sister/weapon/$sisterSlug"
Save-Json 'sisters/ephemeras.json' '/v2/sister/ephemeras'
Save-Json 'sisters/quirks.json' '/v2/sister/quirks'
Save-Json 'locations/locations.json' '/v2/locations'
Save-Json 'npcs/npcs.json' '/v2/npcs'
Save-Json 'missions/missions.json' '/v2/missions'
Save-Json 'orders/recent.json' '/v2/orders/recent'
Save-Json 'orders/orders-item.json' "/v2/orders/item/$itemSlug"
Save-Json 'orders/top.json' "/v2/orders/item/$itemSlug/top"

# take one online user slug from the recent orders
$userSlug = (Get-Content -Raw (Join-Path $resRoot 'orders/recent.json') | ConvertFrom-Json).data[0].user.slug
Save-Json 'users/user.json' "/v2/user/$userSlug"
Save-Json 'orders/orders-user.json' "/v2/orders/user/$userSlug"
Save-Json 'achievements/achievements.json' '/v2/achievements'
Save-Json 'achievements/user.json' "/v2/achievements/user/$userSlug"
Save-Json 'dashboard/showcase.json' '/v2/dashboard/showcase'

Write-Host 'backup done.'
