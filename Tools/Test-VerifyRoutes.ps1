$ErrorActionPreference='Stop'
$wrapper=Join-Path $PSScriptRoot 'Verify-Windows.ps1'
$count=0
function Check([string]$name,[bool]$ok){if(-not $ok){throw "FAIL $name"};$script:count++;Write-Output "PASS $name"}
function Describe([string[]]$Extra){$out=& powershell.exe -NoProfile -File $wrapper -Describe @Extra 2>&1;$code=$LASTEXITCODE;if($code -ne 0){throw "Unexpected rejected describe: $out"};return (($out -join "`n")|ConvertFrom-Json)}
foreach($n in 1..20){$id='GB-L'+$n.ToString('00');$r=Describe @('-Suite','Campaign','-Route',$id);$world=[int][Math]::Ceiling($n/4.0);$chapter=@('Parish','Orchard','City','Fall','Empyrean')[$world-1];Check "route $id -> $chapter" ($r.chapter -eq $chapter -and $r.receipt -eq ($chapter.ToLower()+'-result.json') -and $r.flags.Contains('-gb-route-id '+$id))}
foreach($n in 1..5){$r=Describe @('-Suite','Campaign','-Route',('GB-B'+$n));Check ('boss '+$n) ($r.chapter -eq @('Parish','Orchard','City','Fall','Empyrean')[$n-1])}
$r=Describe @('-Suite','Empyrean','-Route','GB-B5','-FinalPair','mirror-parallax','-FinalChoice','Wax','-ExpectedMercies','0','-RequireEarned');Check 'zero-Mercy boundary is not omitted' ($r.flags.Contains('-gb-expected-mercies 0') -and $r.flags.Contains('-gb-require-earned-world') -and $r.flags.Contains('-gb-final-pair mirror-parallax'))
$r=Describe @('-Suite','Empyrean','-Route','GB-L19','-Practice','-WithSecrets','-RenderFps','30');Check 'practice secret cadence' ($r.flags.Contains('-gb-with-secrets') -and $r.flags.Contains('-gb-practice-witness') -and $r.flags.Contains('-gb-render-fps 30'))
foreach($extra in @(@('-Suite','City','-Route','GB-L17'),@('-Suite','Campaign','-Route','W5'),@('-Suite','Empyrean','-Route','GB-L19','-FinalPair','echo-ink'),@('-Suite','Empyrean','-Route','GB-B5','-Practice','-RequireEarned'),@('-Suite','Empyrean','-Route','GB-L19','-Practice','-SaveSeed','unused.json'),@('-Suite','Campaign','-Route','GB-L21'))){
 $saved=$ErrorActionPreference;$ErrorActionPreference='Continue';$out=& powershell.exe -NoProfile -File $wrapper -Describe @extra 2>&1;$code=$LASTEXITCODE;$ErrorActionPreference=$saved;Check ('invalid combination '+($extra -join ' ')) ($code -ne 0)
}
$r=Describe @('-Suite','Mechanics');Check 'independent mechanics remains supported' ($r.flags -eq '-gb-verify' -and $r.receipt -eq 'verification.json')
Write-Output "VERIFY_WRAPPER_TESTS_PASS $count"
