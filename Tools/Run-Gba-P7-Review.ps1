$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
$out=Join-Path $p 'Reports/VisualV6/gba_review_p7_batch'
if(Test-Path $out){throw 'Refusing to overwrite an earlier batch receipt'}
New-Item -ItemType Directory $out | Out-Null
. "$PSScriptRoot/CampaignGraph.ps1"
$status=@{status='RUNNING';phase='waiting-for-owned-predecessor';source=(& git -C $p rev-parse HEAD).Trim();started=(Get-Date).ToString('o')}
function Receipt {Write-CampaignReceipt -Path "$out/runner.json" -Value $status}
Receipt
try {
 $start=Get-Date
 do {
  if(((Get-Date)-$start).TotalSeconds -gt 900){throw 'Predecessor wait deadline exceeded'}
  $prior=Read-CampaignPredecessor -Path "$p/Reports/V6-GBA-Full-P4/runner.json"
  if($prior.status -eq 'RUNNING'){Start-Sleep -Seconds 2}
 } while($prior.status -eq 'RUNNING')
 $status.predecessor=$prior.status;$status.phase='native-art-fixtures';Receipt
 & "$PSScriptRoot/Run-VisualV6.ps1" -RunId 'gba_p7'
 $v=Get-Content "$p/Reports/VisualV6/gba_p7/runner.json" -Raw|ConvertFrom-Json
 if($v.status -ne 'PASS'){throw 'Native visual capture did not pass'}
 $status.phase='actual-keyboard-ui';Receipt
 & "$PSScriptRoot/Review-NativeUI.ps1" -Player "$p/Builds/Windows/GloomBean.exe" -Reports "$p/Reports/VisualV6/gba_ui_p7"
 $ui=Get-Content "$p/Reports/VisualV6/gba_ui_p7/ui-result.json" -Raw|ConvertFrom-Json
 if(-not $ui.isolated_save_has_no_progress -or @($ui.screens).Count -ne 14){throw 'Keyboard review incomplete or leaked progress'}
 $status.status='CAPTURED';$status.phase='awaiting-visual-and-pixel-audit';$status.visual_images=$v.images;$status.ui_images=@($ui.screens).Count
} catch {$status.status='FAIL';$status.error=$_.Exception.Message}
$status.finished=(Get-Date).ToString('o');Receipt
