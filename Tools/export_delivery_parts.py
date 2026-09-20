#!/usr/bin/env python3
"""Split an immutable delivery file for bounded transport; never alters its bytes."""
from __future__ import annotations
import argparse,hashlib,json,sys
from pathlib import Path

def split(source:Path,destination:Path,mib:int=80)->dict:
    if not 1<=mib<=90:raise ValueError('Part size must be1..90MiB, below the observed100MiB transport ceiling')
    source=source.resolve(strict=True)
    if not source.is_file():raise ValueError('Expected a file')
    destination.mkdir(parents=True,exist_ok=False)
    result={'source_name':source.name,'source_bytes':source.stat().st_size,'parts':[]}
    whole=hashlib.sha256()
    with source.open('rb') as src:
        index=0
        while data:=src.read(mib*1024*1024):
            name=f'part-{index:03d}.bin'
            with (destination/name).open('xb') as out:out.write(data)
            whole.update(data);result['parts'].append({'name':name,'bytes':len(data),'sha256':hashlib.sha256(data).hexdigest()});index+=1
    result['source_sha256']=whole.hexdigest()
    with (destination/'PARTS.json').open('x',encoding='utf-8') as out:json.dump(result,out,indent=2);out.write('\n')
    return result

def main():
    ap=argparse.ArgumentParser(description=__doc__);ap.add_argument('source',type=Path);ap.add_argument('destination',type=Path);ap.add_argument('--mib',type=int,default=80);a=ap.parse_args()
    try:print(json.dumps(split(a.source,a.destination,a.mib),indent=2));return 0
    except (OSError,ValueError) as e:print(json.dumps({'status':'FAIL','error':str(e)}));return 1
if __name__=='__main__':sys.exit(main())
