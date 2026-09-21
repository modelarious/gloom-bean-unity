$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
& "$p/Tools/Run-BroadVisual.ps1" -RunId 'Q6'
$report=Get-Content "$p/Reports/BroadVisual/Q6/runner.json" -Raw|ConvertFrom-Json
if($report.status -ne 'PASS'){throw 'Q6 native capture failed; do not run tests on a stale build'}
& "$p/Tools/Run-Campaign-Checks.ps1"
