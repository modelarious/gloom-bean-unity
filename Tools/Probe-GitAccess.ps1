$ErrorActionPreference='Stop'
$root=Split-Path $PSScriptRoot -Parent
$out=Join-Path $root '.continuity\git-access-L2'
New-Item -ItemType Directory -Force $out|Out-Null
$env:GIT_TERMINAL_PROMPT='0';$env:GCM_INTERACTIVE='never';$env:GCM_GUI_PROMPT='0'
$status=@{scope='Noninteractive native git with existing credential helper; no secrets extracted, prompts, writes or repository creation';started=(Get-Date).ToString('o');status='UNKNOWN'}
$git=Get-Command git -ErrorAction Stop
$process=Start-Process $git.Source -ArgumentList @('-c','credential.interactive=never','ls-remote','--exit-code','https://github.com/modelarious/obsidian-notes.git','refs/heads/main') -RedirectStandardOutput "$out\output.tmp" -RedirectStandardError "$out\error.tmp" -PassThru
if(-not $process.WaitForExit(20000)){Stop-Process -Id $process.Id -Force;$status.status='TIMEOUT'}else{
 $process.Refresh();$status.exit=$process.ExitCode
 $text=[IO.File]::ReadAllText("$out\output.tmp")
 if($process.ExitCode -eq 0 -and $text -match '^([a-f0-9]{40})\s+refs/heads/main'){$status.status='PRIVATE_REPO_READ_PASS';$status.observed_commit=$Matches[1]}else{$status.status='NONINTERACTIVE_GIT_AUTH_NOT_AVAILABLE'}
}
$status.finished=(Get-Date).ToString('o')
$status|ConvertTo-Json|Set-Content -Encoding UTF8 "$out\receipt.json"
Remove-Item -LiteralPath "$out\output.tmp","$out\error.tmp" -ErrorAction SilentlyContinue
