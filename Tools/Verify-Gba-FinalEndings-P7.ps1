$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
$identity=Get-Content "$p/Delivery/V6-GBA-P7/PACKAGE_IDENTITY.json" -Raw|ConvertFrom-Json
$player=Join-Path $identity.extracted 'GloomBeanWindows/GloomBean.exe'
$root=Join-Path $p 'Reports/V6-GBA-Packaged-P7-R3'
if(Test-Path $root){throw 'Refusing to overwrite packaged evidence'}
New-Item -ItemType Directory $root|Out-Null
. "$PSScriptRoot/CampaignGraph.ps1"
$status=@{status='RUNNING';phase='native-packaged-gameplay';archive_sha256=$identity.sha256;assembly_sha256=$identity.assembly_sha256;render_engine='d221ea49253f885caf381e853e72bf1fb9c8f758';seed_engine='cc40096f1afa50b1d102c0cc7b75308bfd63d819';started=(Get-Date).ToString('o');cases=@()}
function Receipt {Write-CampaignReceipt -Path "$root/runner.json" -Value $status}
function Q([string]$v){if($v.Contains('"')){throw 'Invalid quoted argument'};'"'+$v+'"'}
$proc=$null;Receipt
try {
 $waitStart=Get-Date
 do {$previous=Read-CampaignPredecessor -Path "$p/Reports/V6-GBA-Packaged-P7-R2/runner.json";if($previous.status -eq 'RUNNING'){if(((Get-Date)-$waitStart).TotalSeconds -gt 360){throw 'Owned predecessor wait exceeded'};Start-Sleep -Seconds 1}}while($previous.status -eq 'RUNNING')
 $prior=Read-CampaignPredecessor -Path "$p/Reports/V6-GBA-Full-P4/runner.json"
 if($prior.status -ne 'PASS'){throw 'Earned predecessor campaign is not PASS'}
 if((Get-FileHash (Join-Path $identity.extracted 'GloomBeanWindows/GloomBean_Data/Managed/Assembly-CSharp.dll') -Algorithm SHA256).Hash.ToLower() -ne $identity.assembly_sha256){throw 'Extracted assembly mismatch'}
 if((Get-FileHash $identity.archive -Algorithm SHA256).Hash.ToLower() -ne $identity.sha256){throw 'Frozen archive changed'}
 $cases=@(
 @{name='Ordinary';mode='-gb-empyrean-verify -gb-route-id GB-B5 -gb-require-earned-world -gb-final-pair magnet-shadow -gb-expected-mercies 0';result='empyrean-result.json';parent='L20-ordinary';mercies=0;restored=$false},
 @{name='Nineteen';mode='-gb-empyrean-verify -gb-route-id GB-B5 -gb-require-earned-world -gb-final-pair echo-ink -gb-expected-mercies 19';result='empyrean-result.json';parent='L20-nineteen-Mercies';mercies=19;restored=$false},
 @{name='Restored';mode='-gb-empyrean-verify -gb-route-id GB-B5 -gb-require-earned-world -gb-final-pair stitch-coffin -gb-expected-mercies 20 -gb-with-secrets';result='empyrean-result.json';parent='L20-secret';mercies=20;restored=$true}
 )
 foreach($case in $cases){
  $out=Join-Path $root $case.name;New-Item -ItemType Directory $out|Out-Null
  $parentHash=$null
  if($case.parent){$seed=Join-Path (Join-Path "$p/Reports/V6-GBA-Full-P4" $case.parent) 'test-save.json';$parentHash=(Get-FileHash $seed -Algorithm SHA256).Hash.ToLower();Copy-Item $seed "$out/test-save.json";Copy-Item $seed "$out/earned-parent-save.json";@{source=$seed;sha256=$parentHash;source_engine=$status.seed_engine;policy='Unmodified genuinely earned P4 output loaded by the new P7 presentation; no grant or reset'}|ConvertTo-Json|Set-Content "$out/SAVE_PROVENANCE.json"}
  $args=$case.mode+' -gb-action-captures -gb-render-fps 60 -gb-reports '+(Q $out)+' -logFile '+(Q "$out/player.log")+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
  $proc=Start-Process $player -ArgumentList $args -PassThru
  if(-not $proc.WaitForExit(300000)){Stop-Process -Id $proc.Id -Force;throw ('Owned gameplay timeout: '+$case.name)}
  $proc.Refresh();[IO.File]::WriteAllText("$out/player.exit",[string]$proc.ExitCode)
  $result=Get-Content (Join-Path $out $case.result) -Raw|ConvertFrom-Json
  if($proc.ExitCode -ne 0 -or $result.failed -ne 0){throw ('Native gameplay failed: '+$case.name)}
  $frames=Get-Content "$out/NativeAction/native-action.json" -Raw|ConvertFrom-Json
  if(@($frames.shots).Count -lt 2){throw ('Insufficient actual action captures: '+$case.name)}
  if(@($frames.shots|Where-Object {$_.input -ne 'WitnessInput' -or -not $_.cameraFollow}).Count){throw 'Final-boss action must use its actual WitnessInput adapter and live camera follow'}
  $ending=$null
  if($case.ContainsKey('mercies')){
   $ending=Get-Content "$out/ending-state.json" -Raw|ConvertFrom-Json
   if($ending.mercies -ne $case.mercies -or $ending.restored_ending -ne $case.restored -or -not $ending.earned_campaign -or -not $ending.gameplay_corruption_retained){throw 'Incorrect ending boundary'}
   if(-not (Test-Path "$out/ending-screen.png")){throw 'Missing actual ending backbuffer'}
   if((Get-FileHash "$out/earned-parent-save.json" -Algorithm SHA256).Hash.ToLower() -ne $parentHash){throw 'Parent save changed'}
  }
  $status.cases+=@{name=$case.name;status='PASS';exit=$proc.ExitCode;checks=$result.checks;failed=$result.failed;action_images=@($frames.shots).Count;parent_sha256=$parentHash;ending=$ending};Receipt
 }
 $status.status='PASS';$status.phase='finished'
} catch {$status.status='FAIL';$status.error=$_.Exception.Message}
finally {if($proc){$proc.Refresh();if(-not $proc.HasExited){Stop-Process -Id $proc.Id -Force}}}
$status.finished=(Get-Date).ToString('o');Receipt
