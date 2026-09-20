# Pure scheduling decisions. Dot-sourcing starts no process and touches no files.
function Assert-CampaignGraph {
 param([object[]]$Jobs,[hashtable]$Names)
 $settled=@{}
 while($settled.Count -lt $Jobs.Count){
  $progress=$false
  foreach($job in $Jobs){
   if($settled.ContainsKey($job.name)){continue}
   $parent=$job.case.parent
   if($parent -and (-not $Names.ContainsKey($parent) -or $parent -eq $job.name)){throw 'Unknown or self parent'}
   if(-not $parent -or $settled.ContainsKey($parent)){$settled[$job.name]=$true;$progress=$true}
  }
  if(-not $progress){throw 'Cyclic parent-save graph'}
 }
}
function Test-CampaignCanAdvance {
 param([object[]]$Jobs,[hashtable]$Names)
 foreach($job in $Jobs){
  if($job.state -ne 'PENDING'){continue}
  $parent=$job.case.parent
  if($parent){
   if(-not $Names.ContainsKey($parent)){throw 'Unknown parent'}
   if($Names[$parent].state -in @('PENDING','RUNNING')){continue}
   # A failed parent is actionable: propagate DEPENDENCY_FAILED on the next pass.
   if($Names[$parent].state -ne 'PASS'){return $true}
  }
  $gui=[bool]$job.case.windowed -or $job.case.route -eq 'GB-B5'
  if($gui -and @($Jobs|Where-Object {$_.state -eq 'RUNNING' -and ($_.case.windowed -or $_.case.route -eq 'GB-B5')}).Count -gt 0){continue}
  return $true
 }
 return $false
}

function Read-CampaignPredecessor {
 param([Parameter(Mandatory=$true)][string]$Path,[int]$TimeoutMilliseconds=3000)
 $until=[DateTime]::UtcNow.AddMilliseconds($TimeoutMilliseconds)
 do {
  try {
   # A writer replacing runner.json can briefly remove the old directory entry.
   $value=[IO.File]::ReadAllText($Path)|ConvertFrom-Json
   if($value.status -notin @('RUNNING','PASS','FAIL')){throw 'Unknown predecessor state'}
   return $value
  } catch {if([DateTime]::UtcNow -ge $until){throw 'Predecessor receipt unavailable after bounded retry; visible test not started'};Start-Sleep -Milliseconds 75}
 } while($true)
}

function Write-CampaignReceipt {
 param([Parameter(Mandatory=$true)][string]$Path,[Parameter(Mandatory=$true)][object]$Value,[int]$TimeoutMilliseconds=5000)
 $tmp=$Path+'.'+[Guid]::NewGuid().ToString('N')+'.pending'
 $json=$Value|ConvertTo-Json -Depth 16
 [IO.File]::WriteAllText($tmp,$json,(New-Object Text.UTF8Encoding($false)))
 $until=[DateTime]::UtcNow.AddMilliseconds($TimeoutMilliseconds)
 try {
  do {
   try {
    if([IO.File]::Exists($Path)){[IO.File]::Replace($tmp,$Path,($tmp+'.previous'))}else{[IO.File]::Move($tmp,$Path)}
    return
   } catch [IO.IOException] {if([DateTime]::UtcNow -ge $until){throw};Start-Sleep -Milliseconds 50}
     catch [UnauthorizedAccessException] {if([DateTime]::UtcNow -ge $until){throw};Start-Sleep -Milliseconds 50}
  }while($true)
 } finally {if([IO.File]::Exists($tmp)){[IO.File]::Delete($tmp)};if([IO.File]::Exists($tmp+'.previous')){[IO.File]::Delete($tmp+'.previous')}}
}
