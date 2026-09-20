$ErrorActionPreference='Stop'
. "$PSScriptRoot\CampaignGraph.ps1"
$dir=Join-Path ([IO.Path]::GetTempPath()) ('gloom-predecessor-'+[Guid]::NewGuid().ToString('N'));New-Item -ItemType Directory $dir|Out-Null;$file=Join-Path $dir 'receipt.json';$job=$null;$count=0
function Check([string]$name,[bool]$ok){if(-not $ok){throw "FAIL $name"};$script:count++;Write-Output "PASS $name"}
try {
 foreach($state in @('RUNNING','PASS','FAIL')){@{status=$state}|ConvertTo-Json|Set-Content -Encoding UTF8 $file;$r=Read-CampaignPredecessor -Path $file;Check ('known '+$state) ($r.status -eq $state)}
 foreach($text in @('not json','{"status":"UNKNOWN"}')){[IO.File]::WriteAllText($file,$text);$refused=$false;try{Read-CampaignPredecessor -Path $file -TimeoutMilliseconds 100|Out-Null}catch{$refused=$true};Check 'invalid state remains refused' $refused}
 Remove-Item $file;$refused=$false;try{Read-CampaignPredecessor -Path $file -TimeoutMilliseconds 100|Out-Null}catch{$refused=$true};Check 'permanently missing file refused' $refused
 $job=Start-Job -ScriptBlock {param($path)Start-Sleep -Milliseconds 350;[IO.File]::WriteAllText($path,'{"status":"RUNNING"}')} -ArgumentList $file
 $r=Read-CampaignPredecessor -Path $file -TimeoutMilliseconds 6000;Check 'transient disappearance waits for a real receipt' ($r.status -eq 'RUNNING');Wait-Job $job|Out-Null
} finally {if($job){Remove-Job $job -Force -ErrorAction SilentlyContinue};Remove-Item $dir -Recurse -Force}
Write-Output "PREDECESSOR_READER_TESTS_PASS $count"
