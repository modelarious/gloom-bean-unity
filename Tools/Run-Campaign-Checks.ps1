$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
$r=Get-Content "$p\Tools\orchard-request.json" -Raw | ConvertFrom-Json
$dir=Join-Path $p ('Reports\Orchard-v04\'+$r.id)
New-Item -ItemType Directory -Force $dir | Out-Null
$source=(& git -C $p rev-parse HEAD).Trim()
$status=@{id=$r.id;status='RUNNING';phase='build';started=(Get-Date).ToString('o');source_commit=$source;request_sha256=(Get-FileHash "$p\Tools\orchard-request.json" -Algorithm SHA256).Hash.ToLower()}
function Receipt {$status | ConvertTo-Json -Depth 8 | Set-Content "$dir\runner.json"}
function Q([string]$s){'"'+$s+'"'}
$jobs=@();Receipt
try {
 if((& git -C $p diff --name-only HEAD -- Assets Packages ProjectSettings)){throw 'Uncommitted runtime source: checkpoint before building'}
 if($r.build){& "$p\Tools\Build-Windows.ps1" -ProjectPath $p | Out-File "$dir\build-output.txt"}
 if((& git -C $p diff --name-only $source -- Assets Packages ProjectSettings)){throw 'Runtime source changed during build; this binary is not certified'}
 $status.player_sha256=(Get-FileHash "$p\Builds\Windows\GloomBean.exe" -Algorithm SHA256).Hash.ToLower()
 $cases=if($r.cases){@($r.cases)}else{@($r.routes | ForEach-Object {@{name=$_;route=$_;suite='Parish';secrets=$r.secrets}})}
 $status.managed_assembly_sha256=(Get-FileHash (Join-Path $p 'Builds\Windows\GloomBean_Data\Managed\Assembly-CSharp.dll') -Algorithm SHA256).Hash.ToLower();$status.phase='native-acceptance';Receipt
 foreach($c in $cases){
  $out=Join-Path $dir $c.name;New-Item -ItemType Directory -Force $out | Out-Null
  $mode=switch($c.suite){'Mechanics'{'-gb-verify'} 'OpeningRoute'{'-gb-route-verify'} 'Orchard'{'-gb-orchard-verify -gb-route-id '+$c.route} 'City'{'-gb-city-verify -gb-route-id '+$c.route} 'Fall'{'-gb-fall-verify -gb-route-id '+$c.route} 'Empyrean'{'-gb-empyrean-verify -gb-route-id '+$c.route} default {'-gb-parish-verify -gb-route-id '+$c.route}}
  $gui=[bool]$c.windowed -or $c.route -eq 'GB-B5';$flags=$(if($gui){''}else{'-batchmode '})+$mode+' -gb-reports '+(Q $out)+' -logFile '+(Q "$out\player.log")+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
  if($c.secrets){$flags+=' -gb-with-secrets'}
  if($c.practice){$flags+=' -gb-practice-witness'}
  if($r.trace){$flags+=' -gb-echo-trace'}
  if($null -ne $c.startDelay){$flags+=' -gb-start-delay '+([double]$c.startDelay).ToString([Globalization.CultureInfo]::InvariantCulture)}
  if($c.renderFps){$flags+=' -gb-render-fps '+[int]$c.renderFps}
  if($null -ne $c.expectedMercies){if([int]$c.expectedMercies -lt 0 -or [int]$c.expectedMercies -gt 20){throw 'Invalid Mercy boundary'};$flags+=' -gb-expected-mercies '+[int]$c.expectedMercies}
  if($c.railProbe){$flags+=' -gb-rail-probe 1'}
  if($c.whiteAlternatives){$flags+=' -gb-white-alternatives 1'}
  if($c.noInkControl){$flags+=' -gb-scripture-no-ink 1'}
  if($c.finalPair){if($c.finalPair -notmatch '^(magnet-shadow|echo-ink|wax-gullet|stitch-coffin|mirror-parallax)$'){throw 'Invalid final pair'};$flags+=' -gb-final-pair '+$c.finalPair}
  if($c.finalChoice){if($c.finalChoice -notmatch '^(Echo|Wax|Parallax)$'){throw 'Invalid final choice'};$flags+=' -gb-final-choice '+$c.finalChoice}
  $proc=Start-Process "$p\Builds\Windows\GloomBean.exe" -ArgumentList $flags -PassThru
  $jobs+=@{process=$proc;directory=$out;case=$c;started=Get-Date}
 }
 $outcomes=@()
 foreach($j in $jobs){
  $proc=$j.process;$out=$j.directory;$c=$j.case;$remain=[Math]::Max(1,540-((Get-Date)-$j.started).TotalSeconds)
  if(-not $proc.WaitForExit([int]($remain*1000))){Stop-Process -Id $proc.Id -Force;[IO.File]::WriteAllText("$out\player.exit",'TIMEOUT');$outcomes+=@{case=$c.name;status='TIMEOUT'};continue}
  $proc.Refresh();[IO.File]::WriteAllText("$out\player.exit",[string]$proc.ExitCode)
  $file=switch($c.suite){'Mechanics'{'verification.json'} 'OpeningRoute'{'route-result.json'} 'Orchard'{'orchard-result.json'} 'City'{'city-result.json'} 'Fall'{'fall-result.json'} 'Empyrean'{'empyrean-result.json'} default {'parish-result.json'}}
  if(Test-Path "$out\$file"){$result=Get-Content "$out\$file" -Raw | ConvertFrom-Json;$ok=($proc.ExitCode -eq 0 -and $result.failed -eq 0);$outcomes+=@{case=$c.name;status=$(if($ok){'PASS'}else{'FAIL'});passed=$result.passed;checks=$result.checks;failed=$result.failed;exceptions=$result.exceptions}}
  else {$outcomes+=@{case=$c.name;status='NO_RECEIPT';exit=$proc.ExitCode}}
 }
 $status.results=$outcomes;$status.status=if(@($outcomes | Where-Object {$_.status -ne 'PASS'}).Count -eq 0){'PASS'}else{'FAIL'}
} catch {$status.status='FAIL';$status.error=$_.Exception.Message}
finally {foreach($j in $jobs){$j.process.Refresh();if(-not $j.process.HasExited){Stop-Process -Id $j.process.Id -Force}}}
$status.finished=(Get-Date).ToString('o');Receipt
