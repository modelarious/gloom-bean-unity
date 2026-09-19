param(
    [string]$ProjectPath=(Split-Path $PSScriptRoot -Parent),
    [ValidateSet('Mechanics','OpeningRoute','Parish')][string]$Suite='Mechanics',
    [ValidatePattern('^(W1|GB-L0[1-4]|GB-B1)$')][string]$Route='W1',
    [switch]$WithSecrets,
    [switch]$Practice,
    [ValidateRange(30,540)][int]$TimeoutSeconds=540
)
$ErrorActionPreference='Stop'
$ProjectPath=(Resolve-Path $ProjectPath).Path
$player=Join-Path $ProjectPath 'Builds\Windows\GloomBean.exe'
if(-not (Test-Path $player)){throw 'Build the Windows player first with Build-Windows.ps1.'}
function Q([string]$s){if($s.Contains('"')){throw 'Quotes are not valid in these arguments.'};return '"'+$s+'"'}
$report=Join-Path $ProjectPath ('Reports\'+$Suite+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force $report | Out-Null
$flag=switch($Suite){'Mechanics'{'-gb-verify'} 'OpeningRoute'{'-gb-route-verify'} 'Parish'{'-gb-parish-verify -gb-route-id '+$Route}}
if($WithSecrets){$flag+=' -gb-with-secrets'}
if($Practice){$flag+=' -gb-practice-witness'}
$log=Join-Path $report 'player.log'
$arguments=$flag+' -gb-reports '+(Q $report)+' -logFile '+(Q $log)+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
# All suites force an isolated test save below $report; the player's real save is untouched.
$process=Start-Process -FilePath $player -ArgumentList $arguments -PassThru
if(-not $process.WaitForExit($TimeoutSeconds*1000)){
    Stop-Process -Id $process.Id -Force
    [IO.File]::WriteAllText((Join-Path $report 'exit-code.txt'),'TIMEOUT')
    throw 'Verification timed out; only the test player was stopped.'
}
$process.Refresh();$code=$process.ExitCode
[IO.File]::WriteAllText((Join-Path $report 'exit-code.txt'),[string]$code)
$file=switch($Suite){'Mechanics'{'verification.json'} 'OpeningRoute'{'route-result.json'} 'Parish'{'parish-result.json'}}
$resultPath=Join-Path $report $file
if(-not (Test-Path $resultPath)){throw "No verification receipt: $log"}
$result=Get-Content $resultPath -Raw | ConvertFrom-Json
if($code -ne 0 -or $result.failed -ne 0){throw "Verification failed: $resultPath"}
Write-Output "PASS $Suite : $resultPath"
