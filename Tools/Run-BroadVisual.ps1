param([string]$RunId="broad_baseline")
$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
if($RunId -notmatch '^[A-Za-z0-9_-]+$'){throw 'Invalid run ID'}
$out=Join-Path $p ("Reports/BroadVisual/"+$RunId)
if(Test-Path $out){throw 'Refusing to overwrite visual evidence'}
New-Item -ItemType Directory -Force -Path $out | Out-Null
$status=@{status='RUNNING';source=(& git -C $p rev-parse HEAD).Trim();scope='Native visual fixtures, not input-only gameplay';started=(Get-Date).ToString('o')}
function Receipt {$status|ConvertTo-Json -Depth 8|Set-Content -Encoding UTF8 "$out/runner.json"}
Receipt
try {
 & "$p/Tools/Build-Windows.ps1" -ProjectPath $p | Out-File "$out/build-output.txt"
 Copy-Item "$p/Reports/build-result.json" "$out/build-result.json"
 $status.assembly=(Get-FileHash "$p/Builds/Windows/GloomBean_Data/Managed/Assembly-CSharp.dll").Hash
 $proc=Start-Process "$p/Builds/Windows/GloomBean.exe" -ArgumentList ('-gb-broad-review -gb-reports "'+$out+'" -logFile "'+$out+'/player.log" -screen-width 1280 -screen-height 800 -screen-fullscreen 0') -PassThru
 if(-not $proc.WaitForExit(180000)){Stop-Process -Id $proc.Id -Force;throw 'Visual fixture timeout'}
 $proc.Refresh();$status.exit=$proc.ExitCode
 $r=Get-Content "$out/broad-result.json" -Raw|ConvertFrom-Json
 $status.images=@($r.shots).Count;$status.status=if($proc.ExitCode -eq 0 -and $r.status -eq 'PASS'){'PASS'}else{'FAIL'}
} catch {$status.status='FAIL';$status.error=$_.Exception.Message}
$status.finished=(Get-Date).ToString('o');Receipt
