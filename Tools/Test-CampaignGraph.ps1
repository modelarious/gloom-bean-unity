$ErrorActionPreference='Stop'
. "$PSScriptRoot\CampaignGraph.ps1"
$count=0
function Check([string]$label,[bool]$ok){if(-not $ok){throw "FAIL $label"};$script:count++;Write-Output "PASS $label"}
function Case([string]$name,[string]$parent,[string]$state,[string]$route){@{name=$name;case=@{parent=$parent;route=$route};state=$state}}
$a=Case 'child' 'parent' 'PENDING' 'GB-B5';$b=Case 'parent' '' 'PASS' 'GB-L20';$c=Case 'later-gui' '' 'RUNNING' 'GB-B5'
$jobs=@($a,$b,$c);$names=@{child=$a;parent=$b;'later-gui'=$c}
Assert-CampaignGraph $jobs $names;Check 'out-of-order acyclic graph accepted' $true
Check 'visible players are serialized' (-not(Test-CampaignCanAdvance $jobs $names))
$c.state='PASS';Check 'earlier skipped ending becomes ready after later GUI finishes' (Test-CampaignCanAdvance $jobs $names)
$b.state='FAIL';Check 'failed dependency must be propagated, not reported as cycle' (Test-CampaignCanAdvance $jobs $names)
$a.state='DEPENDENCY_FAILED';Check 'terminal graph has no pending launch' (-not(Test-CampaignCanAdvance $jobs $names))
$a.state='PENDING';$b.state='PENDING';$b.case.parent='child';$caught=$false;try{Assert-CampaignGraph $jobs $names}catch{$caught=$_.Exception.Message -eq 'Cyclic parent-save graph'};Check 'real cycle refused before launching' $caught
$b.case.parent='missing';$caught=$false;try{Assert-CampaignGraph $jobs $names}catch{$caught=$_.Exception.Message -eq 'Unknown or self parent'};Check 'missing parent refused' $caught
$b.case.parent='parent';$caught=$false;try{Assert-CampaignGraph $jobs $names}catch{$caught=$_.Exception.Message -eq 'Unknown or self parent'};Check 'self parent refused' $caught
$b.case.parent='';$b.state='RUNNING';Check 'pending child waits for its running parent' (-not(Test-CampaignCanAdvance $jobs $names))
$a.case.parent='';$a.case.route='GB-L19';$c.state='RUNNING';Check 'independent non-GUI case can run while ending paints' (Test-CampaignCanAdvance $jobs $names)
Write-Output "CAMPAIGN_GRAPH_TESTS_PASS $count"
