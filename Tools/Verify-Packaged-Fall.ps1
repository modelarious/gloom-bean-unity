param([Parameter(Mandatory=$true)][string]$ConfigPath)
$ErrorActionPreference='Stop'
$c=Get-Content $ConfigPath -Raw | ConvertFrom-Json
if((Get-FileHash -LiteralPath $c.archive -Algorithm SHA256).Hash.ToLower() -ne $c.archive_sha256){throw 'Package identity changed'}
if(-not(Test-Path -LiteralPath $c.player)){throw 'Extracted player missing'}
if(Test-Path -LiteralPath $c.reports){throw 'Use a new smoke-report directory'}
New-Item -ItemType Directory $c.reports | Out-Null
$r=@{status='RUNNING';scope='Final extracted ZIP: actual-input B4 completion and isolated save';started=(Get-Date).ToString('o');archive_sha256=$c.archive_sha256;tested_source=$c.tested_source}
function Receipt {$r|ConvertTo-Json -Depth 8|Set-Content -Encoding UTF8 (Join-Path $c.reports 'runner.json')}
function Q([string]$v){if($v.Contains('"')){throw 'Quote in argument'};'"'+$v+'"'}
$proc=$null;Receipt
try{
 $flags='-batchmode -gb-fall-verify -gb-route-id GB-B4 -gb-reports '+(Q $c.reports)+' -logFile '+(Q (Join-Path $c.reports 'player.log'))+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
 $proc=Start-Process -FilePath $c.player -ArgumentList $flags -PassThru
 if(-not $proc.WaitForExit(150000)){Stop-Process -Id $proc.Id -Force;throw 'Packaged player timed out'}
 $proc.Refresh();$r.exit=$proc.ExitCode;[IO.File]::WriteAllText((Join-Path $c.reports 'player.exit'),[string]$r.exit)
 $data=Get-Content (Join-Path $c.reports 'fall-result.json') -Raw|ConvertFrom-Json;$r.assertions=$data.checks;$r.failed=$data.failed
 $save=Get-Content (Join-Path $c.reports 'test-save.json') -Raw|ConvertFrom-Json
 $r.status=if($r.exit -eq 0 -and $r.failed -eq 0 -and $r.assertions -gt 0 -and $save.cleared -contains 'GB-B4'){'PASS'}else{'FAIL'}
}catch{$r.status='FAIL';$r.error=$_.Exception.Message}
finally{if($proc){$proc.Refresh();if(-not $proc.HasExited){Stop-Process -Id $proc.Id -Force};$proc.Dispose()};$r.finished=(Get-Date).ToString('o');Receipt}
if($r.status -ne 'PASS'){exit 1};'PACKAGED_FALL_INPUT_PASS'
