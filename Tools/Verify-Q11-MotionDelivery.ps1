$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot/CampaignGraph.ps1"
$identity=Get-Content "$p/Delivery/WholeGame-Q11/PACKAGE_IDENTITY.json" -Raw|ConvertFrom-Json
$root="$p/Reports/Delivery-WholeGame-Q11"
if(Test-Path $root){throw 'Refuse overwriting exact-package evidence'}
New-Item -ItemType Directory $root|Out-Null
$status=@{status='RUNNING';phase='waiting-for-current-tests';archive_sha256=$identity.sha256;assembly_sha256=$identity.assembly_sha256;started=(Get-Date).ToString('o');actions=@()}
function Receipt {Write-CampaignReceipt -Path "$root/runner.json" -Value $status}
function Q([string]$v){if($v.Contains('"')){throw 'Invalid path quote'};'"'+$v+'"'}
Receipt;$proc=$null
try {
 $deadline=(Get-Date).AddMinutes(8)
 do {$previous=Read-CampaignPredecessor -Path "$p/Reports/Orchard-v04/QualityBar-Q11B-regression/runner.json";if($previous.status -eq 'RUNNING'){if((Get-Date) -gt $deadline){throw 'Current test wait timeout'};Start-Sleep -Seconds 2}}while($previous.status -eq 'RUNNING')
 if($previous.status -ne 'PASS'){throw 'Current native suite is not PASS'}
 $player=Join-Path $identity.extracted 'GloomBeanWindows/GloomBean.exe'
 $status.phase='actual-three-ending-proofs';Receipt
 & "$PSScriptRoot/Verify-PackagedEndings.ps1" -Player $player -Archive $identity.path -ArchiveSha256 $identity.sha256 -AssemblySha256 $identity.assembly_sha256 -EarnedReports 'C:\Users\micha\Projects\GloomBeanUnity\Reports\V6-GBA-Full-P4' -Reports "$root/Endings"
 $ending=Get-Content "$root/Endings/runner.json" -Raw|ConvertFrom-Json
 if($ending.status -ne 'PASS'){throw 'Exact package endings failed'}
 $status.endings=$ending.cases;$status.phase='actual-input-motion-frames';Receipt
 foreach($spec in @(@{name='Laundry';flag='-gb-parish-verify -gb-route-id GB-L03';result='parish-result.json'},@{name='Mirror';flag='-gb-city-verify -gb-route-id GB-L09';result='city-result.json'},@{name='RootDitch';flag='-gb-orchard-verify -gb-route-id GB-L07';result='orchard-result.json'},@{name='Weight';flag='-gb-fall-verify -gb-route-id GB-B4';result='fall-result.json'},@{name='Usher';flag='-gb-parish-verify -gb-route-id GB-B1';result='parish-result.json'},@{name='Judge';flag='-gb-orchard-verify -gb-route-id GB-B2';result='orchard-result.json'},@{name='Surveyor';flag='-gb-city-verify -gb-route-id GB-B3';result='city-result.json'},@{name='FinalHost';flag='-gb-empyrean-verify -gb-route-id GB-B5 -gb-final-pair magnet-shadow';result='empyrean-result.json'})){
  $out=Join-Path $root $spec.name;New-Item -ItemType Directory $out|Out-Null
  $args=$spec.flag+' -gb-practice-witness -gb-motion-captures -gb-render-fps 60 -gb-reports '+(Q $out)+' -logFile '+(Q "$out/player.log")+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
  $proc=Start-Process $player -ArgumentList $args -PassThru
  if(-not $proc.WaitForExit(300000)){Stop-Process -Id $proc.Id -Force;throw ('Owned action timeout: '+$spec.name)}
  $proc.Refresh();$exit=$proc.ExitCode;[IO.File]::WriteAllText("$out/player.exit",[string]$exit);$proc.Dispose();$proc=$null
  $result=Get-Content (Join-Path $out $spec.result) -Raw|ConvertFrom-Json
  if($exit -ne 0 -or $null -eq $result.failed -or $result.failed -ne 0){throw ('Actual route failed: '+$spec.name)}
  $action=Get-Content "$out/MotionAction/native-action.json" -Raw|ConvertFrom-Json
  if(@($action.shots).Count -lt 2){throw 'Missing actual native motion sequence'}
  if(@($action.shots|Where-Object {$_.input -notin @('ScriptedInput','WitnessInput') -or -not $_.cameraFollow}).Count){throw 'Motion sequence is not the actual input-driven follow-camera path'}
  $status.actions+=@{name=$spec.name;exit=$exit;failed=$result.failed;checks=$result.checks;native_frames=@($action.shots).Count;scope='Read-only 0.125s observer of the real production-input route; up to240 frames, no camera or actor staging'};Receipt
 }
 $status.phase='native-keyboard-ui';Receipt
 & "$PSScriptRoot/Review-NativeUI.ps1" -Player $player -Reports "$root/KeyboardUI"
 $ui=Get-Content "$root/KeyboardUI/ui-result.json" -Raw|ConvertFrom-Json
 if($ui.status -eq 'FAIL' -or -not $ui.isolated_save_has_no_progress -or @($ui.screens).Count -ne 14){throw 'Keyboard capture failed or leaked progress'}
 $status.keyboard_images=@($ui.screens).Count;$status.status='PASS_NATIVE_RUNS_CAPTURE_REVIEW_PENDING';$status.phase='native-captures-complete'
} catch {$status.status='FAIL';$status.error=$_.Exception.Message;$status.stack=$_.ScriptStackTrace}
finally {if($proc){$proc.Refresh();if(-not $proc.HasExited){Stop-Process -Id $proc.Id -Force};$proc.Dispose()};$status.finished=(Get-Date).ToString('o');Receipt}
if($status.status -eq 'FAIL'){throw $status.error}
