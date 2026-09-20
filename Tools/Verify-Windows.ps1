param(
    [string]$ProjectPath=(Split-Path $PSScriptRoot -Parent),
    [ValidateSet('Mechanics','OpeningRoute','Parish','Orchard','City','Fall','Empyrean','Campaign')][string]$Suite='Mechanics',
    [ValidatePattern('^(W[1-5]|GB-L(0[1-9]|1[0-9]|20)|GB-B[1-5])$')][string]$Route='W2',
    [switch]$WithSecrets,
    [switch]$Practice,
    [switch]$Describe,
    [string]$SaveSeed,
    [switch]$RequireEarned,
    [ValidateSet('magnet-shadow','echo-ink','wax-gullet','stitch-coffin','mirror-parallax')][string]$FinalPair,
    [ValidateSet('Echo','Wax','Parallax')][string]$FinalChoice,
    [ValidateRange(0,20)][Nullable[int]]$ExpectedMercies,
    [ValidateRange(30,120)][int]$RenderFps=60,
    [ValidateRange(30,540)][int]$TimeoutSeconds=540
)
$ErrorActionPreference='Stop'
# Route to an implemented witness, never silently fall back to another chapter.
$chapter=$null
if($Suite -notin @('Mechanics','OpeningRoute')){
 $world=if($Route -match '^W([1-5])$'){[int]$Matches[1]}elseif($Route -match '^GB-B([1-5])$'){[int]$Matches[1]}else{[int][Math]::Ceiling([int]$Route.Substring(4)/4.0)}
 $chapter=@('Parish','Orchard','City','Fall','Empyrean')[$world-1]
 if($Suite -ne 'Campaign' -and $Suite -ne $chapter){throw "Route $Route belongs to $chapter, not $Suite"}
}
$mode=if($chapter){'-gb-'+$chapter.ToLower()+'-verify -gb-route-id '+$Route}elseif($Suite -eq 'Mechanics'){'-gb-verify'}else{'-gb-route-verify'}
$resultName=if($chapter){$chapter.ToLower()+'-result.json'}elseif($Suite -eq 'Mechanics'){'verification.json'}else{'route-result.json'}
if($Route -eq 'W5' -and $chapter){throw 'World 5 witnesses are bounded per stage: use GB-L17 through GB-L20 or GB-B5; the complete earned chain uses Run-Campaign-Acceptance.ps1.'}
if(($FinalPair -or $FinalChoice -or $null -ne $ExpectedMercies) -and ($chapter -ne 'Empyrean' -or $Route -ne 'GB-B5')){throw 'Final-pair, choice and exact ending expectations require GB-B5.'}
if($Practice -and $RequireEarned){throw 'Practice and earned-progress verification are distinct modes.'}
if($SaveSeed -and $Practice){throw 'Practice isolation uses an empty save; do not seed it with real progress.'}
$flag=$mode
if($WithSecrets){$flag+=' -gb-with-secrets'}
if($Practice){$flag+=' -gb-practice-witness'}
if($RequireEarned){$flag+=' -gb-require-earned-world'}
if($chapter -eq 'Empyrean'){$flag+=' -gb-render-fps '+$RenderFps}
if($FinalPair){$flag+=' -gb-final-pair '+$FinalPair}
if($FinalChoice){$flag+=' -gb-final-choice '+$FinalChoice}
if($null -ne $ExpectedMercies){$flag+=' -gb-expected-mercies '+$ExpectedMercies}
if($Describe){@{suite=$Suite;chapter=$chapter;route=$Route;flags=$flag;receipt=$resultName;save_policy='New isolated report folder; optional seed copied unchanged'}|ConvertTo-Json -Compress;return}
$ProjectPath=(Resolve-Path $ProjectPath).Path
$player=Join-Path $ProjectPath 'Builds\Windows\GloomBean.exe'
if(-not (Test-Path $player)){throw 'Build the Windows player first with Build-Windows.ps1.'}
function Q([string]$s){if($s.Contains('"')){throw 'Quotes are not valid in these arguments.'};return '"'+$s+'"'}
$report=Join-Path $ProjectPath ('Reports\'+$Suite+'-'+(Get-Date -Format 'yyyyMMdd-HHmmss-fff'))
New-Item -ItemType Directory -Force $report | Out-Null
if($SaveSeed){
 $seed=(Resolve-Path -LiteralPath $SaveSeed).Path
 $raw=Get-Content -LiteralPath $seed -Raw
 $parsed=$raw|ConvertFrom-Json
 if($null -eq $parsed.version -or $null -eq $parsed.cleared -or $null -eq $parsed.mercies){throw 'Seed is not a supported save object.'}
 Copy-Item -LiteralPath $seed -Destination (Join-Path $report 'test-save.json')
 Copy-Item -LiteralPath $seed -Destination (Join-Path $report 'earned-parent-save.json')
 $hash=(Get-FileHash -LiteralPath $seed -Algorithm SHA256).Hash.ToLower()
 if((Get-FileHash (Join-Path $report 'test-save.json') -Algorithm SHA256).Hash.ToLower() -ne $hash){throw 'Seed copy hash differs.'}
 @{source=$seed;sha256=$hash;policy='Unmodified caller-supplied earned save; original not written'}|ConvertTo-Json|Set-Content -Encoding UTF8 (Join-Path $report 'SAVE_PROVENANCE.json')
}
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
if($code -ne 0 -or $null -eq $result.failed -or $result.failed -ne 0 -or ($null -ne $result.exceptions -and $result.exceptions -ne 0)){throw "Verification failed: $resultPath"}
Write-Output "PASS $Suite : $resultPath"
