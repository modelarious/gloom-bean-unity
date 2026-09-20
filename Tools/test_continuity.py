#!/usr/bin/env python3
import importlib.util,json,subprocess,tempfile,unittest,shutil,os,stat
from pathlib import Path
BASE=Path(__file__).resolve().parent
def module(name):
 spec=importlib.util.spec_from_file_location(name,BASE/(name+'.py'));obj=importlib.util.module_from_spec(spec);spec.loader.exec_module(obj);return obj
status=module('agent_status');work=module('checkpoint_work')
class ContinuityTests(unittest.TestCase):
 def setUp(self):
  self.root=Path(tempfile.mkdtemp(prefix='gloom-continuity-'));self.git('init','-b','main');self.git('config','user.name','Checkpoint test');self.git('config','user.email','test@example.invalid');(self.root/'Assets').mkdir();(self.root/'Assets/test.cs').write_text('class Test {}\n');(self.root/'.gitignore').write_text('/.continuity/\n');self.git('add','Assets/test.cs','.gitignore');self.git('commit','-m','Initial fixture');self.head=self.git('rev-parse','HEAD').strip()
 def tearDown(self):
  def writable(func,path,exc):os.chmod(path,stat.S_IWRITE|stat.S_IREAD);func(path)
  shutil.rmtree(self.root,onerror=writable)
 def git(self,*args):return subprocess.run(['git','-C',str(self.root),*args],capture_output=True,text=True,check=True,timeout=20).stdout
 def test_read_and_write_probe_are_bounded(self):
  v=status.inspect(self.root,True);self.assertEqual(v['write_probe'],'PASS');self.assertEqual(v['head'],self.head);self.assertEqual(v['source_fingerprint']['files'],1);self.assertFalse(list((self.root/'.continuity').glob('probe-*')));self.assertFalse(self.git('status','--porcelain'))
 def test_missing_receipt_is_not_pass(self):self.assertEqual(status.evidence(self.root,{'name':'missing','path':'missing.json'})['status'],'MISSING')
 def test_failed_child_never_passes(self):
  (self.root/'receipt.json').write_text('{"failed":2,"checks":28}');(self.root/'player.exit').write_text('1');self.assertEqual(status.evidence(self.root,{'name':'bad','path':'receipt.json'})['status'],'FAIL_OR_INCOMPLETE')
 def test_valid_child_requires_exit_receipt(self):
  (self.root/'receipt.json').write_text('{"failed":0,"checks":31}');i={'name':'good','path':'receipt.json'};self.assertEqual(status.evidence(self.root,i)['status'],'FAIL_OR_INCOMPLETE');(self.root/'player.exit').write_text('0');self.assertEqual(status.evidence(self.root,i)['status'],'PASS')
 def test_expected_head_mismatch_does_not_stage(self):
  with self.assertRaises(RuntimeError):work.checkpoint(self.root,'wrong','test',['Assets/test.cs'])
  self.assertFalse(self.git('diff','--cached','--name-only'))
 def test_unowned_staged_changes_refused(self):
  (self.root/'other.txt').write_text('other agent');self.git('add','other.txt')
  with self.assertRaises(RuntimeError):work.checkpoint(self.root,self.head,'test',['Assets/test.cs'])
  self.assertEqual(self.git('diff','--cached','--name-only').strip(),'other.txt')
 def test_unsafe_scope_refused(self):
  for name in ['.','../escape','Library/cache','.env','secret.pem']:
   with self.assertRaises(ValueError):work.checkpoint(self.root,self.head,'test',[name])
 def test_commits_owned_files_and_bundle_recovers(self):
  (self.root/'Assets/test.cs').write_text('class Test { int fixedBug; }\n');(self.root/'unowned.txt').write_text('leave this');r=work.checkpoint(self.root,self.head,'WIP owned chunk',['Assets/test.cs'],True);self.assertEqual(r['publication'],'BLOCKED_NO_ORIGIN');self.assertNotEqual(r['commit'],self.head);self.assertEqual(r['committed_paths'],['Assets/test.cs']);target=self.root/'restore';subprocess.run(['git','clone',r['bundle']['path'],str(target)],capture_output=True,check=True,timeout=20);self.assertEqual((target/'Assets/test.cs').read_text(),(self.root/'Assets/test.cs').read_text());self.assertFalse((target/'unowned.txt').exists());self.assertFalse((self.root/'.continuity/checkpoint.lock').exists())
 def test_source_match_distinguishes_doc_only_change(self):
  f=self.root/'Documentation/Continuity';f.mkdir(parents=True);(f/'CURRENT_CHECKPOINT.json').write_text(json.dumps({'tested_commit':self.head}));self.assertEqual(status.inspect(self.root)['source_vs_tested'],'MATCH');(self.root/'Assets/test.cs').write_text('different');self.assertEqual(status.inspect(self.root)['source_vs_tested'],'DIFF')
 def test_runner_cannot_hide_failed_case(self):
  (self.root/'r.json').write_text(json.dumps({'status':'PASS','results':[{'case':'bad','status':'PASS','failed':2}]}))
  self.assertEqual(status.evidence(self.root,{'name':'r','path':'r.json'})['status'],'FAIL_OR_INCOMPLETE')
 def test_empty_runner_is_unknown(self):
  (self.root/'r.json').write_text('{"status":"PASS","cases":[]}')
  self.assertEqual(status.evidence(self.root,{'name':'r','path':'r.json'})['status'],'UNKNOWN_EMPTY_CASES')
 def test_expected_denial_has_explicit_oracle(self):
  (self.root/'r.json').write_text(json.dumps({'status':'PASS','cases':[{'name':'deny','status':'PASS','failed':1,'exit':1,'expected_denial':True}]}))
  self.assertEqual(status.evidence(self.root,{'name':'r','path':'r.json'})['status'],'PASS')
if __name__=='__main__':unittest.main(verbosity=2)
