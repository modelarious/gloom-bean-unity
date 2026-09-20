param([Parameter(Mandatory=$true)][string]$ConfigPath)
$ErrorActionPreference='Stop'
$config=Get-Content (Resolve-Path $ConfigPath).Path -Raw | ConvertFrom-Json
if(-not $config.cases -or -not $config.atlas -or -not $config.reports){throw 'Missing acceptance configuration'}
$dir=[IO.Path]::GetFullPath($config.reports)
if(Test-Path $dir){throw "Refusing to reuse a prior receipt directory: $dir"}
New-Item -ItemType Directory -Force $dir | Out-Null
function Q([string]$x){if($x.Contains('"')){throw 'Invalid quoted argument'};'"'+$x+'"'}
function Source([string]$root,[string]$expected){
 $head=(& git -C $root rev-parse HEAD).Trim();if($LASTEXITCODE -ne 0 -or $head -ne $expected){throw "Source HEAD mismatch: $root"}
 $changes=& git -C $root status --porcelain -- Assets Packages ProjectSettings
 if($LASTEXITCODE -ne 0 -or $changes){throw "Uncommitted engine input in $root"}
 return @{head=$head;assets=(& git -C $root rev-parse 'HEAD:Assets').Trim();packages=(& git -C $root rev-parse 'HEAD:Packages').Trim();settings=(& git -C $root rev-parse 'HEAD:ProjectSettings').Trim()}
}
$jobs=@();$names=@{}
foreach($c in $config.cases){
 if($c.name -notmatch '^[A-Za-z0-9-]+$' -or $names.ContainsKey($c.name)){throw 'Duplicate or invalid case name'}
 if($c.suite -notin @('Foundation','Mechanics','Parish','Orchard','City','Fall','Empyrean')){throw 'Unknown native suite'}
 if($c.suite -notin @('Foundation','Mechanics') -and $c.route -notmatch '^(W[1-5]|GB-L[0-2][0-9]|GB-B[1-5])$'){throw 'Invalid native route'}
 $j=@{name=$c.name;case=$c;state='PENDING'};$jobs+=$j;$names[$c.name]=$j
}
foreach($j in $jobs){if($j.case.parent -and (-not $names.ContainsKey($j.case.parent) -or $j.case.parent -eq $j.name)){throw 'Unknown or self parent'} }
$status=@{status='RUNNING';phase='source-check';started=(Get-Date).ToString('o');cases=@()}
function Receipt {
 $status.cases=@($jobs|ForEach-Object {@{name=$_.name;status=$_.state;assertions=$_.assertions;failed=$_.failed;exceptions=$_.exceptions;exit=$_.exit;expected_denial=[bool]$_.case.expectedDenial;parent=$_.case.parent;suite=$_.case.suite;route=$_.case.route}})
 $tmp=Join-Path $dir 'runner.pending.json';$status|ConvertTo-Json -Depth 12|Set-Content -Encoding UTF8 $tmp;Move-Item -Force $tmp (Join-Path $dir 'runner.json')
}
Receipt
try {
 $status.atlas_source=Source $config.atlas $config.atlas_commit
 if($config.foundation){$status.foundation_source=Source $config.foundation $config.foundation_commit}
 $status.phase='clean-builds';Receipt
 if($config.foundation){& "$($config.foundation)\Tools\Build-Windows.ps1" -ProjectPath $config.foundation | Out-File "$dir\foundation-build-output.txt";Copy-Item "$($config.foundation)\Reports\build-result.json" "$dir\foundation-build.json"}
 & "$($config.atlas)\Tools\Build-Windows.ps1" -ProjectPath $config.atlas | Out-File "$dir\atlas-build-output.txt"
 Copy-Item "$($config.atlas)\Reports\build-result.json" "$dir\atlas-build.json"
 $null=Source $config.atlas $config.atlas_commit
 if($config.foundation){$null=Source $config.foundation $config.foundation_commit}
 $status.phase='native-input-and-earned-save-chain';Receipt;$allStarted=Get-Date
 while(@($jobs|Where-Object {$_.state -in @('PENDING','RUNNING')}).Count -gt 0){
  if(((Get-Date)-$allStarted).TotalSeconds -gt 1800){throw 'Overall acceptance deadline exceeded'}
  foreach($j in $jobs){
   $c=$j.case
   if($j.state -eq 'PENDING'){
    if($c.parent){$parent=$names[$c.parent];if($parent.state -in @('PENDING','RUNNING')){continue};if($parent.state -ne 'PASS'){$j.state='DEPENDENCY_FAILED';continue}}
    $out=Join-Path $dir $j.name;New-Item -ItemType Directory $out | Out-Null;$j.out=$out
    if($c.parent){
     $seed=Join-Path (Join-Path $dir $c.parent) 'test-save.json';if(-not(Test-Path $seed)){throw "Parent did not earn a persistent save: $($c.parent)"}
     Copy-Item $seed "$out\test-save.json";Copy-Item $seed "$out\earned-parent-save.json"
     $sha=(Get-FileHash $seed -Algorithm SHA256).Hash.ToLower()
     if((Get-FileHash "$out\test-save.json" -Algorithm SHA256).Hash.ToLower() -ne $sha){throw 'Parent-save copy mismatch'}
     @{parent=$c.parent;sha256=$sha;policy='Exact unmodified save from prior real-input completion'} | ConvertTo-Json | Set-Content -Encoding UTF8 "$out\SAVE_PROVENANCE.json"
    }
    $mode=switch($c.suite){'Foundation'{'-gb-verify'} 'Mechanics'{'-gb-verify'} 'Parish'{'-gb-parish-verify'} 'Orchard'{'-gb-orchard-verify'} 'City'{'-gb-city-verify'} 'Fall'{'-gb-fall-verify'} 'Empyrean'{'-gb-empyrean-verify'}}
    $j.file=switch($c.suite){'Foundation'{'verification.json'} 'Mechanics'{'verification.json'} 'Parish'{'parish-result.json'} 'Orchard'{'orchard-result.json'} 'City'{'city-result.json'} 'Fall'{'fall-result.json'} 'Empyrean'{'empyrean-result.json'}}
    if($c.route){$mode+=' -gb-route-id '+$c.route};if($c.secrets){$mode+=' -gb-with-secrets'};if($c.practice){$mode+=' -gb-practice-witness'};if($c.requireEarned -or $c.expectedDenial){$mode+=' -gb-require-earned-world'}
    $root=if($c.suite -eq 'Foundation'){$config.foundation}else{$config.atlas}
    if(-not $root){throw 'Foundation case has no root'}
    $flags='-batchmode '+$mode+' -gb-reports '+(Q $out)+' -logFile '+(Q "$out\player.log")+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
    $j.process=Start-Process (Join-Path $root 'Builds\Windows\GloomBean.exe') -ArgumentList $flags -PassThru;$j.started=Get-Date;$j.state='RUNNING'
   }
   if($j.state -eq 'RUNNING'){
    $proc=$j.process;$proc.Refresh()
    if(-not $proc.HasExited){if(((Get-Date)-$j.started).TotalSeconds -gt 540){Stop-Process -Id $proc.Id -Force;$j.state='TIMEOUT';[IO.File]::WriteAllText("$($j.out)\player.exit",'TIMEOUT')};continue}
    $proc.WaitForExit();$proc.Refresh();$j.exit=$proc.ExitCode;[IO.File]::WriteAllText("$($j.out)\player.exit",[string]$j.exit)
    $resultFile=Join-Path $j.out $j.file
    if(-not(Test-Path $resultFile)){$j.state='NO_RECEIPT';continue}
    $result=Get-Content $resultFile -Raw | ConvertFrom-Json
    if($null -eq $result.failed){$j.state='INVALID_RECEIPT';continue}
    $j.assertions=if($null -ne $result.passed){[int]$result.passed+[int]$result.failed}else{[int]$result.checks};$j.failed=[int]$result.failed;$j.exceptions=$result.exceptions
    if($c.expectedDenial){
     $observation=Join-Path $j.out ($c.suite.ToLower()+'-observations.txt')
     $denial=Test-Path $observation
     if($denial){$denial=(Get-Content $observation -Raw).Contains($c.suite+' was not earned by the supplied real save')}
     $j.state=if($j.exit -eq 1 -and $j.failed -eq 1 -and $denial){'PASS'}else{'FAIL'}
    }else{$j.state=if($j.exit -eq 0 -and $j.failed -eq 0 -and ($null -eq $j.exceptions -or $j.exceptions -eq 0) -and $j.assertions -gt 0){'PASS'}else{'FAIL'}}
   }
  }
  Receipt
  if(@($jobs|Where-Object {$_.state -eq 'RUNNING'}).Count -eq 0 -and @($jobs|Where-Object {$_.state -eq 'PENDING'}).Count -gt 0){throw 'Blocked or cyclic parent-save graph'}
  Start-Sleep -Milliseconds 350
 }
 $null=Source $config.atlas $config.atlas_commit
 if($config.foundation){$null=Source $config.foundation $config.foundation_commit}
 $status.status=if(@($jobs|Where-Object {$_.state -ne 'PASS'}).Count -eq 0){'PASS'}else{'FAIL'};$status.phase='finished'
}catch{$status.status='FAIL';$status.error=$_.Exception.Message}
finally{foreach($j in $jobs){if($j.process){$j.process.Refresh();if(-not $j.process.HasExited){Stop-Process -Id $j.process.Id -Force};$j.process.Dispose()}};$status.finished=(Get-Date).ToString('o');Receipt}
if($status.status -ne 'PASS'){exit 1}
Write-Output "NATIVE_CAMPAIGN_ACCEPTANCE_PASS $dir"
