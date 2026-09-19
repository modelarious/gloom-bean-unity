param(
    [string]$ProjectPath=(Split-Path $PSScriptRoot -Parent),
    [ValidateSet('Mechanics','OpeningRoute')][string]$Suite='Mechanics',
    [ValidateRange(30,540)][int]$TimeoutSeconds=540
)
$ErrorActionPreference='Stop'
$ProjectPath=(Resolve-Path $ProjectPath).Path
$player=Join-Path $ProjectPath 'Builds\Windows\GloomBean.exe'
if(-not (Test-Path $player)){throw 'Build the Windows player first with Build-Windows.ps1.'}
function Q([string]$s){if($s.Contains('"')){throw 'Quotes are not valid in these arguments.'};return '"'+$s+'"'}
$report=Join-Path $ProjectPath ('Reports\'+$Suite+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force $report | Out-Null
$flag=if($Suite -eq 'Mechanics'){'-gb-verify'}else{'-gb-route-verify'}
$log=Join-Path $report 'player.log'
$arguments=$flag+' -gb-reports '+(Q $report)+' -logFile '+(Q $log)+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
# Both suites force an isolated test save below $report; the player's real save is untouched.
$process=Start-Process -FilePath $player -ArgumentList $arguments -PassThru
if(-not $process.WaitForExit($TimeoutSeconds*1000)){
    Stop-Process -Id $process.Id -Force
    [IO.File]::WriteAllText((Join-Path $report 'exit-code.txt'),'TIMEOUT')
    throw 'Verification timed out; only the test player was stopped.'
}
$process.Refresh();$code=$process.ExitCode
[IO.File]::WriteAllText((Join-Path $report 'exit-code.txt'),[string]$code)
$file=if($Suite -eq 'Mechanics'){'verification.json'}else{'route-result.json'}
$resultPath=Join-Path $report $file
if(-not (Test-Path $resultPath)){throw "No verification receipt: $log"}
$result=Get-Content $resultPath -Raw | ConvertFrom-Json
if($code -ne 0 -or $result.failed -ne 0){throw "Verification failed: $resultPath"}
Write-Output "PASS $Suite : $resultPath"
