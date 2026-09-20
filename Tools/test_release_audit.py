import json,tempfile,unittest,struct
from pathlib import Path
from audit_release import audit_case,audit_ending,AuditFailure,sha
class AuditTests(unittest.TestCase):
 def setUp(self):self.t=tempfile.TemporaryDirectory();self.root=Path(self.t.name);self.d=self.root/'x';self.d.mkdir();self.spec={'name':'x','suite':'Empyrean'};self.case={'name':'x','status':'PASS','exit':0,'assertions':1};self.j(self.d/'empyrean-result.json',{'failed':0,'checks':1});(self.d/'empyrean-observations.txt').write_text('PASS actual input\n');(self.d/'player.exit').write_text('0')
 def tearDown(self):self.t.cleanup()
 def j(self,p,v):p.write_text(json.dumps(v),encoding='utf-8')
 def run_case(self):return audit_case(self.root,self.case,self.spec)
 def test_consistent_fixture(self):self.assertEqual(self.run_case()['assertions'],1)
 def test_nonzero_native_exit(self):(self.d/'player.exit').write_text('1');self.assertRaises(AuditFailure,self.run_case)
 def test_missing_result(self):(self.d/'empyrean-result.json').unlink();self.assertRaises(OSError,self.run_case)
 def test_summary_cannot_hide_failure(self):self.j(self.d/'empyrean-result.json',{'failed':1,'checks':1});self.assertRaises(AuditFailure,self.run_case)
 def test_trace_count_not_assertions(self):self.j(self.d/'empyrean-result.json',{'failed':0,'checks':2});self.case['assertions']=2;self.assertRaises(AuditFailure,self.run_case)
 def test_error_text_not_pass(self):(self.d/'empyrean-observations.txt').write_text('PASS actual input\nERROR lost input');self.assertRaises(AuditFailure,self.run_case)
 def test_pending_not_pass(self):self.case['status']='PENDING';self.assertRaises(AuditFailure,self.run_case)
 def test_exact_denial(self):self.spec['expectedDenial']=True;self.j(self.d/'empyrean-result.json',{'failed':1,'checks':0});(self.d/'player.exit').write_text('1');(self.d/'empyrean-observations.txt').write_text('FAIL Empyrean was not earned by the supplied real save');self.assertTrue(self.run_case()['expected_denial'])
 def test_wrong_denial(self):self.spec['expectedDenial']=True;self.assertRaises(AuditFailure,self.run_case)
 def test_practice_leak(self):self.spec['practice']=True;self.j(self.d/'saved-progress.json',{'cleared':['GB-L19'],'mercies':[]});self.assertRaises(AuditFailure,self.run_case)
 def test_parent_save_tamper(self):parent=self.root/'p';parent.mkdir();self.j(parent/'test-save.json',{'cleared':[]});self.spec['parent']='p';self.j(self.d/'SAVE_PROVENANCE.json',{'parent':'p','sha256':sha(parent/'test-save.json')});self.j(self.d/'earned-parent-save.json',{'cleared':['GB-L19']});self.assertRaises(AuditFailure,self.run_case)
 def ending(self,n):
  self.j(self.d/'test-save.json',{'cleared':[f'GB-L{i:02d}' for i in range(1,21)]+[f'GB-B{i}' for i in range(1,6)],'mercies':[f'GB-L{i:02d}-MERCY' for i in range(1,n+1)],'corrupted':True});self.j(self.d/'ENDING.json',{'earned_campaign':True,'mercies':n,'restored_ending':n==20,'normal_figure_drawn':n==20,'gameplay_corruption_retained':True});(self.d/'ending-screen.png').write_bytes(b'\x89PNG\r\n\x1a\n'+b'\x00'*8+struct.pack('>II',1280,800)+b'fixture'*200)
 def test_ending_boundary(self):self.ending(19);self.assertFalse(audit_ending(self.root,'x',19)['restored'])
 def test_restoration_requires_all_twenty(self):self.ending(19);self.assertRaises(AuditFailure,audit_ending,self.root,'x',20)
 def test_duplicate_secret_does_not_count(self):self.ending(20);s=json.loads((self.d/'test-save.json').read_text());s['mercies'][-1]=s['mercies'][0];self.j(self.d/'test-save.json',s);self.assertRaises(AuditFailure,audit_ending,self.root,'x',20)
 def test_cannot_erase_corruption(self):self.ending(20);s=json.loads((self.d/'test-save.json').read_text());s['corrupted']=False;self.j(self.d/'test-save.json',s);self.assertRaises(AuditFailure,audit_ending,self.root,'x',20)
 def test_missing_backbuffer(self):self.ending(20);(self.d/'ending-screen.png').unlink();self.assertRaises(OSError,audit_ending,self.root,'x',20)
if __name__=='__main__':unittest.main(verbosity=2)
