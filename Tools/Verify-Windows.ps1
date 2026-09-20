param(
    [string]$ProjectPath=(Split-Path $PSScriptRoot -Parent),
    [ValidateSet('Mechanics','OpeningRoute','Parish','Orchard','City','Fall','Campaign')][string]$Suite='Mechanics',
    [ValidatePattern('^(W[1-4]|GB-L(0[1-9]|1[0-6])|GB-B[1-4])$')][string]$Route='W2',
    [switch]$WithSecrets,
    [switch]$Practice,
    [switch]$Describe,
    [ValidateRange(30,540)][int]$TimeoutSeconds=540
)
$ErrorActionPreference='Stop'
# Route to an implemented witness, never silently fall back to another chapter.
$chapter=$null
if($Suite -notin @('Mechanics','OpeningRoute')){
 $world=if($Route -match '^W([1-4])$'){[int]$Matches[1]}elseif($Route -match '^GB-B([1-4])$'){[int]$Matches[1]}else{[int][Math]::Ceiling([int]$Route.Substring(4)/4.0)}
 $chapter=@('Parish','Orchard','City','Fall')[$world-1]
 if($Suite -ne 'Campaign' -and $Suite -ne $chapter){throw "Route $Route belongs to $chapter, not $Suite"}
}
$mode=if($chapter){'-gb-'+$chapter.ToLower()+'-verify -gb-route-id '+$Route}elseif($Suite -eq 'Mechanics'){'-gb-verify'}else{'-gb-route-verify'}
$resultName=if($chapter){$chapter.ToLower()+'-result.json'}elseif($Suite -eq 'Mechanics'){'verification.json'}else{'route-result.json'}
if($Describe){@{suite=$Suite;chapter=$chapter;route=$Route;flags=$mode;receipt=$resultName}|ConvertTo-Json -Compress;return}
$ProjectPath=(Resolve-Path $ProjectPath).Path
$player=Join-Path $ProjectPath 'Builds\Windows\GloomBean.exe'
if(-not (Test-Path $player)){throw 'Build the Windows player first with Build-Windows.ps1.'}
function Q([string]$s){if($s.Contains('"')){throw 'Quotes are not valid in these arguments.'};return '"'+$s+'"'}
$report=Join-Path $ProjectPath ('Reports\'+$Suite+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force $report | Out-Null
$flag=$mode
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
$file=$resultName
$resultPath=Join-Path $report $file
if(-not (Test-Path $resultPath)){throw "No verification receipt: $log"}
$result=Get-Content $resultPath -Raw | ConvertFrom-Json
if($code -ne 0 -or $result.failed -ne 0){throw "Verification failed: $resultPath"}
Write-Output "PASS $Suite : $resultPath"
