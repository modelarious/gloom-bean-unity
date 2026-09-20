param(
 [Parameter(Mandatory=$true)][string]$Player,
 [Parameter(Mandatory=$true)][string]$Archive,
 [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{64}$')][string]$ArchiveSha256,
 [Parameter(Mandatory=$true)][ValidatePattern('^[a-f0-9]{64}$')][string]$AssemblySha256,
 [Parameter(Mandatory=$true)][string]$EarnedReports,
 [Parameter(Mandatory=$true)][string]$Reports
)
$ErrorActionPreference='Stop'
. "$PSScriptRoot\CampaignGraph.ps1"
function Q([string]$s){if($s.Contains('"')){throw 'Quote in path'};'"'+$s+'"'}
if(Test-Path -LiteralPath $Reports){throw 'Refuse reused delivery-smoke evidence'}
if((Get-FileHash -LiteralPath $Archive -Algorithm SHA256).Hash.ToLower() -ne $ArchiveSha256){throw 'Distributed archive hash differs'}
$assembly=Join-Path (Split-Path $Player -Parent) 'GloomBean_Data\Managed\Assembly-CSharp.dll'
if((Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash.ToLower() -ne $AssemblySha256){throw 'Extracted player differs from tested assembly'}
New-Item -ItemType Directory -Path $Reports|Out-Null
$state=@{status='RUNNING';scope='Exact extracted Windows ZIP; three native input-only final victories from unchanged genuinely earned pre-final saves';archive_sha256=$ArchiveSha256;assembly_sha256=$AssemblySha256;started=(Get-Date).ToString('o');cases=@()}
$proc=$null
function Receipt {Write-CampaignReceipt -Path (Join-Path $Reports 'runner.json') -Value $state}
Receipt
try {
 foreach($spec in @(@{name='Ordinary';parent='Final-ordinary';mercies=0;pair='magnet-shadow'},@{name='Nineteen';parent='Final-nineteen-not-restored';mercies=19;pair='echo-ink'},@{name='Restored';parent='Final-restored';mercies=20;pair='stitch-coffin'})){
  if(@(Get-CimInstance Win32_Process -Filter "Name='GloomBean.exe'"|Where-Object {$_.CommandLine -notmatch '-batchmode'}).Count -gt 0){throw 'Another visible game is active; do not interfere with its ending'}
  $out=Join-Path $Reports $spec.name;New-Item -ItemType Directory $out|Out-Null
  $seed=Join-Path (Join-Path $EarnedReports $spec.parent) 'earned-parent-save.json'
  $save=Get-Content -LiteralPath $seed -Raw|ConvertFrom-Json
  if(@($save.cleared|Where-Object {$_ -match '^GB-L(0[1-9]|1[0-9]|20)$'}).Count -ne 20 -or @($save.cleared|Where-Object {$_ -match '^GB-B[1-4]$'}).Count -ne 4 -or $save.cleared -contains 'GB-B5' -or @($save.mercies).Count -ne $spec.mercies){throw 'Seed was not genuinely earned through the twenty-level pre-final campaign'}
  Copy-Item -LiteralPath $seed -Destination "$out\test-save.json";Copy-Item -LiteralPath $seed -Destination "$out\earned-parent-save.json"
  $seedHash=(Get-FileHash -LiteralPath $seed -Algorithm SHA256).Hash.ToLower()
  if((Get-FileHash "$out\test-save.json" -Algorithm SHA256).Hash.ToLower() -ne $seedHash){throw 'Pre-final save copy changed'}
  @{source=$seed;sha256=$seedHash;policy='Unchanged native earned pre-final save; no grants or edits'}|ConvertTo-Json|Set-Content -Encoding UTF8 "$out\SAVE_PROVENANCE.json"
  $flags='-gb-empyrean-verify -gb-route-id GB-B5 -gb-require-earned-world -gb-render-fps 60 -gb-final-pair '+$spec.pair+' -gb-expected-mercies '+$spec.mercies+' -gb-reports '+(Q $out)+' -logFile '+(Q "$out\player.log")+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
  $proc=Start-Process -FilePath $Player -ArgumentList $flags -PassThru
  if(-not $proc.WaitForExit(240000)){Stop-Process -Id $proc.Id -Force;throw 'Packaged ending exceeded four minutes'}
  $proc.Refresh();$exit=$proc.ExitCode;[IO.File]::WriteAllText("$out\player.exit",[string]$exit);$proc.Dispose();$proc=$null
  $result=Get-Content "$out\empyrean-result.json" -Raw|ConvertFrom-Json;$ending=Get-Content "$out\ENDING.json" -Raw|ConvertFrom-Json;$final=Get-Content "$out\test-save.json" -Raw|ConvertFrom-Json
  if($exit -ne 0 -or $null -eq $result.failed -or $result.failed -ne 0 -or $result.checks -le 0){throw 'Packaged native victory failed'}
  $restored=$spec.mercies -eq 20
  if(-not $ending.earned_campaign -or $ending.mercies -ne $spec.mercies -or $ending.restored_ending -ne $restored -or $ending.normal_figure_drawn -ne $restored -or -not $ending.gameplay_corruption_retained){throw 'Packaged ending boundary differs'}
  if(@($final.cleared).Count -ne 25 -or @($final.mercies).Count -ne $spec.mercies -or -not $final.corrupted){throw 'Packaged saved result differs'}
  if(-not(Test-Path "$out\ending-screen.png") -or (Get-Item "$out\ending-screen.png").Length -lt 1000){throw 'Packaged ending did not paint a native backbuffer'}
  $state.cases+=@{name=$spec.name;status='PASS';checks=$result.checks;exit=$exit;mercies=$spec.mercies;restored=$restored;seed_sha256=$seedHash;ending_image_sha256=(Get-FileHash "$out\ending-screen.png" -Algorithm SHA256).Hash.ToLower()};Receipt
 }
 if($state.cases[0].ending_image_sha256 -eq $state.cases[2].ending_image_sha256){throw 'Ordinary/restored packaged backbuffers identical'}
 $state.status='PASS'
}catch{$state.status='FAIL';$state.error=$_.Exception.Message;$state.error_stack=$_.ScriptStackTrace}
finally{if($proc){$proc.Refresh();if(-not $proc.HasExited){Stop-Process -Id $proc.Id -Force};$proc.Dispose()};$state.finished=(Get-Date).ToString('o');Receipt}
if($state.status -ne 'PASS'){throw $state.error}
Write-Output 'PACKAGED_ENDINGS_PASS'
