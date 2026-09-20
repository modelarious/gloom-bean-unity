# One-shot delivery acceptance for the exact M1 archive. No source edits or progress grants.
$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot\CampaignGraph.ps1"
$pre=Join-Path $p 'Reports\Full-Campaign-M1\runner.json'
$deadline=[DateTime]::UtcNow.AddMinutes(15)
do {
 $r=Read-CampaignPredecessor -Path $pre
 if($r.status -ne 'RUNNING'){break}
 if([DateTime]::UtcNow -ge $deadline){throw 'Timed out waiting for the owned full M1 acceptance; no packaged game was started'}
 Start-Sleep -Seconds 1
} while($true)
if($r.status -ne 'PASS' -or $r.phase -ne 'finished' -or @($r.cases).Count -ne 54 -or @($r.cases|Where-Object {$_.status -ne 'PASS'}).Count -ne 0){throw 'Full M1 acceptance is not complete/pass; no packaged ending claim'}
& "$PSScriptRoot\Verify-PackagedEndings.ps1" `
 -Player 'C:\Users\micha\Projects\GloomBean Full Campaign Extracted M1\GloomBeanWindows\GloomBean.exe' `
 -Archive (Join-Path $p 'Delivery\Full-Campaign-M1\GloomBean_Windows_Full_Campaign.zip') `
 -ArchiveSha256 'fe7241c2bc8b4786f2fee961865a287e4175f5dab52f63a67d1a2d289b8abc27' `
 -AssemblySha256 '8ccc3b3ac2d8aba179e74333bd999190b8f8d0ac7a1f871fa268297389795e3e' `
 -EarnedReports (Join-Path $p 'Reports\Full-Campaign-M1') `
 -Reports (Join-Path $p 'Reports\Packaged-Endings-M1')
Write-Output 'M1_PACKAGED_DELIVERY_CHECKS_PASS'
