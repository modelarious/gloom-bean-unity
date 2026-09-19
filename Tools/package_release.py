#!/usr/bin/env python3
"""Package tracked Unity source or a built Windows player; include per-file SHA-256s.
No Library/cache, credentials, unrelated files, or raw editor logs are collected.
"""
from __future__ import annotations
import argparse,hashlib,json,subprocess,zipfile
from pathlib import Path,PurePosixPath

def digest(data:bytes)->str:return hashlib.sha256(data).hexdigest()
def git(root:Path,*args:str)->bytes:
    return subprocess.run(["git","-C",str(root),*args],check=True,capture_output=True).stdout

def main()->None:
    ap=argparse.ArgumentParser();ap.add_argument("--project",type=Path,required=True);ap.add_argument("--out",type=Path,required=True)
    ap.add_argument("--prefix",default="GloomBeanUnity");ap.add_argument("--kind",choices=("source","player"),required=True);ap.add_argument("--bundle",type=Path);ap.add_argument("--atlas",type=Path)
    args=ap.parse_args();
    if not args.prefix or "/" in args.prefix or "\\" in args.prefix or args.prefix in (".",".."):raise ValueError("Unsafe package prefix")
    root=args.project.resolve();entries:dict[str,bytes]={}
    if args.out.exists():raise FileExistsError("Use a new versioned output filename: "+str(args.out))
    if args.kind=="source":
        dirty=git(root,"status","--porcelain","--untracked-files=no").decode().strip()
        if dirty:raise RuntimeError("Commit or reconcile tracked changes before packaging source: "+dirty)
    if args.kind=="source":
        for raw in git(root,"ls-files","-z").decode().split("\0"):
            if not raw:continue
            rel=PurePosixPath(raw)
            if rel.is_absolute() or ".." in rel.parts:raise ValueError("Unsafe tracked path")
            entries[str(PurePosixPath(args.prefix)/rel)]=(root/raw).read_bytes()
        if args.bundle:
            subprocess.run(["git","-C",str(root),"bundle","verify",str(args.bundle.resolve())],check=True)
            entries["History/gloom-bean.bundle"]=args.bundle.read_bytes()
        if args.atlas:
            data=args.atlas.read_bytes()
            if digest(data)!="c006f063e336651e89109251fbc74cac1114ca8d939867acdd209646a90bc5fc":raise ValueError("Atlas differs from pinned source")
            entries["Reference/Gloom_Bean_Host_Cycle_Design_Atlas.pdf"]=data
    else:
        base=root/"Builds/Windows"
        if not (base/"GloomBean.exe").is_file():raise FileNotFoundError("Build the Windows player first")
        for f in sorted(base.rglob("*")):
            if not f.is_file() or f.suffix.lower() in (".pdb",".mdb",".log"):continue
            if any("DoNotShip" in part or "dontship" in part.lower() for part in f.parts):continue
            entries[str(PurePosixPath("GloomBeanWindows")/PurePosixPath(f.relative_to(base).as_posix()))]=f.read_bytes()
        entries["GloomBeanWindows/START_HERE.txt"]=b"Run GloomBean.exe. Keep the Data, DLL and MonoBleedingEdge folders together. FOUNDATION is the mechanics playground; HOST CYCLE is the experimental atlas campaign. Escape pauses; F1 controls; F2 design notes; F4 toggles original action sounds. Read the included release status for first-chapter route evidence and remaining campaign limits."
    manifest={"schema":1,"kind":args.kind,"source_commit":git(root,"rev-parse","HEAD").decode().strip(),"files":{k:{"bytes":len(v),"sha256":digest(v)} for k,v in sorted(entries.items())}}
    entries["PAYLOAD_SHA256.json"]=json.dumps(manifest,indent=2).encode()
    args.out.parent.mkdir(parents=True,exist_ok=True)
    with zipfile.ZipFile(args.out,"w",zipfile.ZIP_DEFLATED,compresslevel=6) as z:
        for name,data in sorted(entries.items()):
            info=zipfile.ZipInfo(name,date_time=(2026,9,19,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.external_attr=0o644<<16;z.writestr(info,data)
    with zipfile.ZipFile(args.out) as z:
        if z.testzip() is not None:raise RuntimeError("ZIP CRC verification failed")
        for name,entry in manifest["files"].items():
            if digest(z.read(name))!=entry["sha256"]:raise RuntimeError("ZIP payload mismatch: "+name)
    print(json.dumps({"path":str(args.out),"bytes":args.out.stat().st_size,"sha256":digest(args.out.read_bytes()),"payload_files":len(manifest["files"])}))
if __name__=="__main__":main()
