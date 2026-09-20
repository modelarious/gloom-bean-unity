param([Parameter(Mandatory=$true)][string]$Player,[Parameter(Mandatory=$true)][string]$Reports)
$ErrorActionPreference='Stop'
if(Test-Path $Reports){throw 'A UI evidence directory must be new.'}
New-Item -ItemType Directory $Reports|Out-Null
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class GloomWindow {
 public delegate bool EnumProc(IntPtr w,IntPtr p);
 [StructLayout(LayoutKind.Sequential)] public struct Point {public int X,Y;}
 [StructLayout(LayoutKind.Sequential)] public struct Rect {public int Left,Top,Right,Bottom;}
 [DllImport("user32.dll")] public static extern bool EnumWindows(EnumProc p,IntPtr x);
 [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr w,out uint pid);
 [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr w);
 [DllImport("user32.dll")] public static extern IntPtr GetForegroundWindow();
 [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr w);
 [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
 [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr w,out Rect r);
 [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr w,ref Point p);
 [DllImport("user32.dll")] public static extern void keybd_event(byte key,byte scan,uint flags,UIntPtr extra);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr w,uint msg,IntPtr a,IntPtr b);
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr w,IntPtr after,int x,int y,int width,int height,uint flags);
 public static IntPtr Find(uint pid){IntPtr result=IntPtr.Zero;EnumWindows((w,p)=>{uint n;GetWindowThreadProcessId(w,out n);if(n==pid&&IsWindowVisible(w)){result=w;return false;}return true;},IntPtr.Zero);return result;}
}
'@
[GloomWindow]::SetProcessDPIAware()|Out-Null
function Q([string]$x){if($x.Contains('"')){throw 'Quote in argument'};'"'+$x+'"'}
$status=@{status='RUNNING';scope='Real keyboard and native window screenshots, not a complete playthrough';screens=@();actions=@();started=(Get-Date).ToString('o')}
function Receipt {$status|ConvertTo-Json -Depth 8|Set-Content -Encoding UTF8 (Join-Path $Reports 'ui-result.json')}
Receipt
$proc=$null;$window=[IntPtr]::Zero
function Focus {
 if($window -eq [IntPtr]::Zero){throw 'Own native window missing'}
 [GloomWindow]::SetForegroundWindow($window)|Out-Null;Start-Sleep -Milliseconds 100
 if([GloomWindow]::GetForegroundWindow() -ne $window){throw 'Foreground belongs to another window; no keyboard input sent'}
}
function Key([byte]$key,[int]$hold=100){Focus;[GloomWindow]::keybd_event($key,0,0,[UIntPtr]::Zero);try{Start-Sleep -Milliseconds $hold}finally{[GloomWindow]::keybd_event($key,0,2,[UIntPtr]::Zero)};$status.actions+=@{key=$key;milliseconds=$hold};Start-Sleep -Milliseconds 350}
function Picture([string]$name){Focus;Start-Sleep -Milliseconds 250;$rect=New-Object GloomWindow+Rect;$point=New-Object GloomWindow+Point;[GloomWindow]::GetClientRect($window,[ref]$rect)|Out-Null;[GloomWindow]::ClientToScreen($window,[ref]$point)|Out-Null;$w=$rect.Right;$h=$rect.Bottom;if($w -lt 600 -or $h -lt 400){throw 'Invalid native client rectangle'};$image=New-Object System.Drawing.Bitmap($w,$h);$g=[Drawing.Graphics]::FromImage($image);try{$g.CopyFromScreen($point.X,$point.Y,0,0,(New-Object Drawing.Size($w,$h)));$image.Save((Join-Path $Reports ($name+'.png')),[Drawing.Imaging.ImageFormat]::Png)}finally{$g.Dispose();$image.Dispose()};$status.screens+=$name;Receipt}
try {
 $deadline=(Get-Date).AddSeconds(90)
 while(@(Get-CimInstance Win32_Process -Filter "Name='GloomBean.exe'"|Where-Object {$_.CommandLine -notmatch '-batchmode'}).Count -gt 0){if((Get-Date) -gt $deadline){throw 'Other windowed game tests still active; no UI inputs sent'};Start-Sleep -Seconds 2}
 $arguments='-gb-save '+(Q (Join-Path $Reports 'isolated-save.json'))+' -logFile '+(Q (Join-Path $Reports 'player.log'))+' -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
 $proc=Start-Process $Player -ArgumentList $arguments -PassThru
 for($i=0;$i -lt 60;$i++){Start-Sleep -Milliseconds 250;$window=[GloomWindow]::Find([uint32]$proc.Id);if($window -ne [IntPtr]::Zero){break}}
 if($window -eq [IntPtr]::Zero){throw 'Own Unity player window never appeared'}
 [GloomWindow]::SetWindowPos($window,[IntPtr]::Zero,20,20,1296,839,4)|Out-Null;Start-Sleep -Seconds 3
 Key 115;Picture '01-cute-title';Key 112;Picture '02-controls';Key 112
 Key 13;Picture '03-cute-world-selection';Key 13;Picture '04-cute-level-selection';Key 13;Start-Sleep -Seconds 2;Picture '05-sunday-entry'
 Focus;[GloomWindow]::keybd_event(39,0,0,[UIntPtr]::Zero);Start-Sleep -Milliseconds 400;[GloomWindow]::keybd_event(32,0,0,[UIntPtr]::Zero);Start-Sleep -Milliseconds 200;[GloomWindow]::keybd_event(32,0,2,[UIntPtr]::Zero);[GloomWindow]::keybd_event(39,0,2,[UIntPtr]::Zero);Picture '06-keyboard-jump'
 Key 27;Picture '07-real-pause';Key 40;Key 40;Key 13;Picture '08-returned-level-select';Key 27;Key 27
 Key 40;Key 40;Key 13;Picture '09-practice-world-select';Key 40;Key 40;Key 40;Key 40;Key 13;Picture '10-final-world-practice';Key 40;Key 40;Key 40;Key 40;Key 13;Start-Sleep -Seconds 2;Picture '11-final-boss-native'
 $save=Join-Path $Reports 'isolated-save.json';if(Test-Path $save){$d=Get-Content $save -Raw|ConvertFrom-Json;if(@($d.cleared).Count -gt 0 -or @($d.mercies).Count -gt 0){throw 'This UI check unexpectedly earned progression'}}
 $status.status='CAPTURED_PENDING_VISUAL_REVIEW';$status.isolated_save_has_no_progress=$true
} catch {$status.status='FAIL';$status.error=$_.Exception.Message}
finally {foreach($k in @(13,27,32,37,38,39,40,112,115)){[GloomWindow]::keybd_event([byte]$k,0,2,[UIntPtr]::Zero)};if($window -ne [IntPtr]::Zero){[GloomWindow]::PostMessage($window,16,[IntPtr]::Zero,[IntPtr]::Zero)|Out-Null};if($proc -and -not $proc.WaitForExit(10000)){Stop-Process -Id $proc.Id -Force};$status.finished=(Get-Date).ToString('o');Receipt}
if($status.status -eq 'FAIL'){exit 1}
Write-Output 'NATIVE_UI_CAPTURE_COMPLETE_REVIEW_REQUIRED'
