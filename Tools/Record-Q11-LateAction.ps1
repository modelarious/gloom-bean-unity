param([ValidateSet('Weight','FinalHost')][string]$Route)
$ErrorActionPreference='Stop'
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
 [DllImport("user32.dll")] public static extern uint MapVirtualKey(uint key,uint mode);
 [DllImport("user32.dll")] public static extern void keybd_event(byte key,byte scan,uint flags,UIntPtr extra);
 [DllImport("user32.dll")] public static extern bool PostMessage(IntPtr w,uint msg,IntPtr a,IntPtr b);
 [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr w,IntPtr after,int x,int y,int width,int height,uint flags);
 public static IntPtr Find(uint pid){IntPtr result=IntPtr.Zero;EnumWindows((w,p)=>{uint n;GetWindowThreadProcessId(w,out n);if(n==pid&&IsWindowVisible(w)){result=w;return false;}return true;},IntPtr.Zero);return result;}
}
'@
[GloomWindow]::SetProcessDPIAware()|Out-Null
$p=Split-Path $PSScriptRoot -Parent
$id=Get-Content "$p/Delivery/WholeGame-Q11/PACKAGE_IDENTITY.json" -Raw|ConvertFrom-Json
$player=Join-Path $id.extracted 'GloomBeanWindows/GloomBean.exe'
$out=Join-Path $p ('Reports/Q11-LateAction/'+$Route)
if(Test-Path $out){throw 'Refusing to reuse native action evidence'}
if(@(Get-CimInstance Win32_Process -Filter "Name='GloomBean.exe'"|Where-Object {$_.CommandLine -notmatch '-batchmode'}).Count){throw 'Another visible game owns the display'}
New-Item -ItemType Directory "$out/Frames" -Force|Out-Null
$mode=if($Route -eq 'Weight'){'-gb-fall-verify -gb-route-id GB-B4'}else{'-gb-empyrean-verify -gb-route-id GB-B5 -gb-final-pair magnet-shadow'}
$wait=if($Route -eq 'Weight'){45}else{28};$resultName=if($Route -eq 'Weight'){'fall-result.json'}else{'empyrean-result.json'}
$state=@{status='RUNNING';scope='Read-only foreground-owned client screenshots during an actual production-input route. No keyboard events, camera/actor/time manipulation or manufactured progress.';screens=@();started=(Get-Date).ToString('o');route=$Route;capture_start_seconds=$wait;assembly_sha256=$id.assembly_sha256}
function Receipt {$state|ConvertTo-Json -Depth 8|Set-Content -Encoding UTF8 "$out/runner.json"}
$proc=$null;Receipt
try{
 $args=$mode+' -gb-practice-witness -gb-render-fps 60 -gb-reports "'+$out+'" -logFile "'+$out+'/player.log" -screen-width 1280 -screen-height 800 -screen-fullscreen 0'
 $proc=Start-Process $player -ArgumentList $args -PassThru;$clock=[Diagnostics.Stopwatch]::StartNew();$window=[IntPtr]::Zero
 for($i=0;$i -lt 60;$i++){Start-Sleep -Milliseconds 200;$window=[GloomWindow]::Find([uint32]$proc.Id);if($window -ne [IntPtr]::Zero){break}}
 if($window -eq [IntPtr]::Zero){throw 'Own game window did not appear'}
 [GloomWindow]::SetWindowPos($window,[IntPtr]::Zero,20,20,1296,839,4)|Out-Null
 [GloomWindow]::SetForegroundWindow($window)|Out-Null
 $next=$wait
 while(-not $proc.HasExited){
  if($clock.Elapsed.TotalSeconds -gt 240){throw 'Owned route deadline exceeded'}
  if($clock.Elapsed.TotalSeconds -lt $next){Start-Sleep -Milliseconds 30;$proc.Refresh();continue}
  if([GloomWindow]::GetForegroundWindow() -ne $window){throw 'Foreground changed; no other window is captured'}
  $rect=New-Object GloomWindow+Rect;$point=New-Object GloomWindow+Point
  [GloomWindow]::GetClientRect($window,[ref]$rect)|Out-Null;[GloomWindow]::ClientToScreen($window,[ref]$point)|Out-Null
  if($rect.Right -lt 600 -or $rect.Bottom -lt 400){throw 'Invalid client capture'}
  $name=('{0:D4}.png' -f $state.screens.Count);$time=$clock.Elapsed.TotalSeconds
  $image=New-Object Drawing.Bitmap($rect.Right,$rect.Bottom);$graphics=[Drawing.Graphics]::FromImage($image)
  try{$graphics.CopyFromScreen($point.X,$point.Y,0,0,(New-Object Drawing.Size($rect.Right,$rect.Bottom)));$image.Save("$out/Frames/$name",[Drawing.Imaging.ImageFormat]::Png)}finally{$graphics.Dispose();$image.Dispose()}
  $state.screens+=@{file=$name;time=$time};$next=$time+.25;$proc.Refresh()
 }
 $proc.Refresh();[IO.File]::WriteAllText("$out/player.exit",[string]$proc.ExitCode)
 $r=Get-Content "$out/$resultName" -Raw|ConvertFrom-Json
 if($proc.ExitCode -ne 0 -or $r.failed -ne 0 -or @($state.screens).Count -lt 4){throw 'Route or captured action incomplete'}
 $state.status='PASS_NATIVE_REVIEW_PENDING';$state.checks=$r.checks;$state.exit=$proc.ExitCode;$state.captured=@($state.screens).Count
}catch{$state.status='FAIL';$state.error=$_.Exception.Message}
finally{if($proc){$proc.Refresh();if(-not $proc.HasExited){Stop-Process -Id $proc.Id -Force};$proc.Dispose()};$state.finished=(Get-Date).ToString('o');Receipt}
