param(
    [string]$ProjectPath=(Split-Path $PSScriptRoot -Parent),
    [string]$UnityPath,
    [ValidateRange(30,540)][int]$TimeoutSeconds=540
)
$ErrorActionPreference='Stop'
$ProjectPath=(Resolve-Path $ProjectPath).Path
$versionFile=Join-Path $ProjectPath 'ProjectSettings\ProjectVersion.txt'
if(-not (Test-Path $versionFile)){throw "Not a Unity project: $ProjectPath"}
$version=((Get-Content $versionFile | Where-Object {$_ -match '^m_EditorVersion: '}) -replace '^m_EditorVersion: ','').Trim()
if(-not $UnityPath){$UnityPath=Join-Path ${env:ProgramFiles} "Unity\Hub\Editor\$version\Editor\Unity.exe"}
if(-not (Test-Path $UnityPath)){throw "Unity $version not found. Pass -UnityPath with the installed Editor executable. No installation or licence changes were attempted."}
function Q([string]$s){if($s.Contains('"')){throw 'Quotes are not valid in these arguments.'};return '"'+$s+'"'}
$run=Get-Date -Format 'yyyyMMdd-HHmmss-fff'
$report=Join-Path $ProjectPath "Reports\Build-$run"
New-Item -ItemType Directory -Force $report | Out-Null
$log=Join-Path $report 'editor.log'
$arguments='-batchmode -quit -projectPath '+(Q $ProjectPath)+' -buildTarget Win64 -executeMethod GloomBean.Editor.BuildTools.BuildWindows -logFile '+(Q $log)
# Run as your normal licensed desktop user, not a SYSTEM service account.
$process=Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru
if(-not $process.WaitForExit($TimeoutSeconds*1000)){
    Stop-Process -Id $process.Id -Force
    [IO.File]::WriteAllText((Join-Path $report 'exit-code.txt'),'TIMEOUT')
    throw "This build exceeded $TimeoutSeconds seconds. Only its own Editor process was stopped. Log: $log"
}
$process.Refresh();$code=$process.ExitCode
[IO.File]::WriteAllText((Join-Path $report 'exit-code.txt'),[string]$code)
if($code -ne 0){throw "Unity build failed with exit $code. Log: $log"}
$receipt=Join-Path $ProjectPath 'Reports\build-result.json'
if(-not (Test-Path $receipt)){throw 'Build exited without the expected result receipt.'}
$result=Get-Content $receipt -Raw | ConvertFrom-Json
if($result.result -ne 'Succeeded' -or $result.errors -ne 0){throw 'Build receipt did not certify success.'}
Write-Output "Built: $(Join-Path $ProjectPath 'Builds\Windows\GloomBean.exe')"
Write-Output "Log: $log"
