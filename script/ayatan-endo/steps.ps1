function Endo([int]$B, [double]$M, [int]$S, [int]$A, [int]$C) { [math]::Round(($B + 50*$C + 100*$A) * (1 + $M*($C+$A)/$S), [MidpointRounding]::AwayFromZero) }
$cat = @(
    @{ n='Anasa';    B=2000; A=2; C=2 },
    @{ n='Ayr';      B=325;  A=0; C=3 },
    @{ n='Chattraka';B=450;  A=1; C=2 },
    @{ n='Orta';     B=650;  A=1; C=3 },
    @{ n='Piv';      B=375;  A=1; C=2 },
    @{ n='Sah';      B=300;  A=1; C=2 },
    @{ n='Valana';   B=325;  A=1; C=2 },
    @{ n='Vaya';     B=400;  A=1; C=2 },
    @{ n='Zambuka';  B=450;  A=1; C=2 },
    @{ n='Kitha';    B=450;  A=1; C=4 },
    @{ n='Hemakara'; B=450;  A=1; C=2 }
)
$allMinDiff = [int]::MaxValue
foreach ($e in $cat) {
    $M = if ($e.B -eq 2000) { 0.5 } elseif ($e.B -eq 450) { 3.0 } else { 2.0 }
    $S = $e.A + $e.C
    $vals = @()
    for ($a = 0; $a -le $e.A; $a++) { for ($c = 0; $c -le $e.C; $c++) { $vals += Endo $e.B $M $S $a $c } }
    $uniq = $vals | Sort-Object -Unique
    $diffs = @()
    for ($i = 1; $i -lt $uniq.Count; $i++) { $diffs += ($uniq[$i] - $uniq[$i-1]) }
    $minD = if ($diffs.Count) { ($diffs | Measure-Object -Minimum).Minimum } else { 0 }
    if ($minD -lt $allMinDiff) { $allMinDiff = $minD }
    $all5 = ($uniq | Where-Object { $_ % 5 -ne 0 }).Count -eq 0
    $all50 = ($uniq | Where-Object { $_ % 50 -ne 0 }).Count -eq 0
    "  $($e.n): values=$($uniq -join ',') minDiff=$minD all%5=$all5 all%50=$all50"
}
"ALL min diff = $allMinDiff"
"step 5 可行? minDiff>=5; 但还需 minDiff 是 5 的倍数 -> minDiff % 5 = $($allMinDiff % 5)"
