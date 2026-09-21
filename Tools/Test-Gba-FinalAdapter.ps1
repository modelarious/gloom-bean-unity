$ErrorActionPreference='Stop'
function Valid($s){$s.input -eq 'WitnessInput' -and $s.cameraFollow}
if(-not (Valid @{input='WitnessInput';cameraFollow=$true})){throw 'Expected adapter rejected'}
foreach($bad in @(@{input='ScriptedInput';cameraFollow=$true},@{input='HumanInput';cameraFollow=$true},@{input='WitnessInput';cameraFollow=$false},@{cameraFollow=$true})) {if(Valid $bad){throw 'Invalid adapter/follow passed'}}
'GBA_FINAL_ADAPTER_CONTROLS_PASS 5'
