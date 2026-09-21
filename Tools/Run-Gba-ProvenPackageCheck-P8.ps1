$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
$identity=Get-Content "$p/Delivery/V6-GBA-P8/PACKAGE_IDENTITY.json" -Raw|ConvertFrom-Json
& "$PSScriptRoot/Verify-PackagedEndings.ps1" `
 -Player (Join-Path $identity.extracted 'GloomBeanWindows/GloomBean.exe') `
 -Archive $identity.archive -ArchiveSha256 $identity.sha256 -AssemblySha256 $identity.assembly_sha256 `
 -EarnedReports "$p/Reports/V6-GBA-Full-P4" -Reports "$p/Reports/V6-GBA-Packaged-P8"
