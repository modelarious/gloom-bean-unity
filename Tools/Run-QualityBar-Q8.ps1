$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
& "$p/Tools/Run-BroadVisual.ps1" -RunId 'Q8'
$report=Get-Content "$p/Reports/BroadVisual/Q8/runner.json" -Raw|ConvertFrom-Json
if($report.status -ne 'PASS'){throw 'Q8 native capture failed; do not run tests on a stale build'}
& "$p/Tools/Run-Campaign-Checks.ps1"
