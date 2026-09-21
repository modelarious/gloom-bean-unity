$ErrorActionPreference='Stop'
$p=Split-Path $PSScriptRoot -Parent
& "$p/Tools/Run-BroadVisual.ps1" -RunId 'Q7'
$report=Get-Content "$p/Reports/BroadVisual/Q7/runner.json" -Raw|ConvertFrom-Json
if($report.status -ne 'PASS'){throw 'Q7 native capture failed; do not run tests on a stale build'}
& "$p/Tools/Run-Campaign-Checks.ps1"
