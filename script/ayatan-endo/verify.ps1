function Endo([int]$B, [double]$M, [int]$S, [int]$A, [int]$C) { [math]::Round(($B + 50*$C + 100*$A) * (1 + $M*($C+$A)/$S)) }

"=== Anasa (B=2000, M=0.5, S=4) vs table 2000/2306/2363/2625/2688/2750/-/3025/3094/-/3450 ==="
$anasa = @(@(0,0,2000),@(1,0,2306),@(0,1,2363),@(2,0,2625),@(1,1,2688),@(0,2,2750),@(2,1,3025),@(1,2,3094),@(2,2,3450))
foreach ($p in $anasa) {
    $A = $p[0]; $C = $p[1]; $exp = $p[2]
    $got = Endo 2000 0.5 4 $A $C
    $ok = if ($got -eq $exp) { 'OK' } else { 'MISMATCH' }
    "${ok}: A=$A C=$C table=$exp calc=$got"
}

"=== M inference: B=450 group (Zambuka/Chattraka/Hemakara S=3, Kitha S=5) ==="
"Zam/Cha/Hem 1C: M=2 -> $(Endo 450 2 3 0 1)  M=3 -> $(Endo 450 3 3 0 1)  table=1000"
"Kitha 1C:      M=2 -> $(Endo 450 2 5 0 1)  M=3 -> $(Endo 450 3 5 0 1)  table=800"
"Kitha 1A:      M=2 -> $(Endo 450 2 5 1 0)  M=3 -> $(Endo 450 3 5 1 0)  table=880"
"Kitha full(1A4C): M=3 -> $(Endo 450 3 5 1 4)  table=3000"

"=== Orta (B=650, M=2, S=4) ==="
"Orta 1C: calc=$(Endo 650 2 4 0 1) table=1050"
"Orta full(1A3C): calc=$(Endo 650 2 4 1 3) table=2700"

"=== Piv (B=375, M=2, S=3) ==="
"Piv 1C: calc=$(Endo 375 2 3 0 1) table=708"
"Piv full(1A2C): calc=$(Endo 375 2 3 1 2) table=1725"

"=== Ayr (B=325, M=2, S=3) ==="
"Ayr 1C: calc=$(Endo 325 2 3 0 1) table=625"
"Ayr full(3C): calc=$(Endo 325 2 3 0 3) table=1425"

"=== Vaya (B=400, M=2, S=3) ==="
"Vaya 1C: calc=$(Endo 400 2 3 0 1) table=750"
"Vaya full(1A2C): calc=$(Endo 400 2 3 1 2) table=1800"
