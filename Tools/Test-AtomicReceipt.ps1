$ErrorActionPreference='Stop'
. "$PSScriptRoot\CampaignGraph.ps1"
$dir=Join-Path ([IO.Path]::GetTempPath()) ('gloom-write-'+[Guid]::NewGuid().ToString('N'));New-Item -ItemType Directory $dir|Out-Null;$file=Join-Path $dir 'runner.json';$job=$null;$count=0
function Check([string]$name,[bool]$ok){if(-not $ok){throw "FAIL $name"};$script:count++;Write-Output "PASS $name"}
try {
 Write-CampaignReceipt $file @{status='RUNNING';sequence=0}
 Check 'new receipt initialized' ((Read-CampaignPredecessor $file).sequence -eq 0)
 $job=Start-Job -ScriptBlock {param($path,$ready,$stop,$helper)
  . $helper
  [IO.File]::WriteAllText($ready,'ready');$n=0;$bad=0;$missing=0;$first='';$until=[DateTime]::UtcNow.AddSeconds(12)
  while(-not [IO.File]::Exists($stop) -and [DateTime]::UtcNow -lt $until){try{$v=Read-CampaignPredecessor -Path $path -TimeoutMilliseconds 3000;if($v.status -ne 'RUNNING' -or $null -eq $v.sequence){$bad++};$n++}catch [IO.FileNotFoundException]{$missing++}catch{$bad++;if(-not $first){$first=$_.Exception.ToString()}}}
  @{reads=$n;invalid=$bad;missing=$missing;first=$first}
 } -ArgumentList $file,(Join-Path $dir 'ready'),(Join-Path $dir 'stop'),(Join-Path $PSScriptRoot 'CampaignGraph.ps1')
 $limit=[DateTime]::UtcNow.AddSeconds(6);while(-not(Test-Path (Join-Path $dir 'ready'))){if([DateTime]::UtcNow -gt $limit){throw 'Reader startup timeout'};Start-Sleep -Milliseconds 20}
 foreach($i in 1..150){Write-CampaignReceipt $file @{status='RUNNING';sequence=$i;payload=('x'*5000)};Start-Sleep -Milliseconds 2}
 [IO.File]::WriteAllText((Join-Path $dir 'stop'),'stop');$job|Wait-Job -Timeout 15|Out-Null;$r=Receive-Job $job
 Write-Output ($r|ConvertTo-Json -Compress)
 Check 'concurrent reader actually ran' ($r.reads -gt 0)
 Check 'no incomplete result escapes the bounded reader' ($r.invalid -eq 0)
 Check 'no unresolved missing receipt after bounded retry' ($r.missing -eq 0)
 Check 'latest complete record retained' ((Read-CampaignPredecessor $file).sequence -eq 150)
 Check 'no temporary file leaked' (@(Get-ChildItem $dir -Filter '*.pending').Count -eq 0)
 Write-CampaignReceipt $file @{status='PASS';sequence=151};Check 'terminal result safely published' ((Read-CampaignPredecessor $file).status -eq 'PASS')
 Write-Output ('Concurrent reads '+$r.reads)
}finally{if($job){Remove-Job $job -Force -ErrorAction SilentlyContinue};Remove-Item $dir -Recurse -Force}
Write-Output "ATOMIC_RECEIPT_TESTS_PASS $count"
