# Final exact-archive proof; predecessor proof is retained, not overwritten.
$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
. "$PSScriptRoot\CampaignGraph.ps1"
$deadline=[DateTime]::UtcNow.AddMinutes(10)
do {
 $r=Read-CampaignPredecessor -Path (Join-Path $p 'Reports\Packaged-Endings-M1\runner.json')
 if($r.status -ne 'RUNNING'){break}
 if([DateTime]::UtcNow -ge $deadline){throw 'Owned predecessor packaged ending is still running; refuse a concurrent visible game'}
 Start-Sleep -Seconds 1
} while($true)
if($r.status -ne 'PASS' -or @($r.cases).Count -ne 3){throw 'Previous exact player ending check failed; preserve it and stop'}
& "$PSScriptRoot\Verify-PackagedEndings.ps1" `
 -Player 'C:\Users\micha\Projects\GloomBean Full Campaign Final Extracted M1\GloomBeanWindows\GloomBean.exe' `
 -Archive 'C:\Users\micha\Projects\GloomBeanUnity\Delivery\Full-Campaign-M1\Final\GloomBean_Windows_Full_Campaign.zip' `
 -ArchiveSha256 '37ed7162514e4901b58467ec6cfdd49f77597907d15b87936ab1bb330fb637aa' `
 -AssemblySha256 '8ccc3b3ac2d8aba179e74333bd999190b8f8d0ac7a1f871fa268297389795e3e' `
 -EarnedReports (Join-Path $p 'Reports\Full-Campaign-M1') `
 -Reports (Join-Path $p 'Reports\Packaged-Endings-M1-Final')
Write-Output 'M1_FINAL_EXACT_ARCHIVE_PASS'
