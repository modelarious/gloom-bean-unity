using System;
using System.Collections.Generic;
using UnityEngine;

namespace GloomBean.Foundation
{
    public sealed class HumanInput : MonoBehaviour, IActorInput
    {
        InputFrame held, pending;
        public bool disabled;
        void Update()
        {
            if (disabled) { held=pending=default; return; }
            float x=(Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)?1:0)-(Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow)?1:0);
            float y=(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.UpArrow)?1:0)-(Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.DownArrow)?1:0);
            if (Input.GetJoystickNames().Length>0)
            {
                var joy=new Vector2(Input.GetAxisRaw("JoyX"),-Input.GetAxisRaw("JoyY"));
                if(Mathf.Abs(joy.x)>.2f) x=joy.x; if(Mathf.Abs(joy.y)>.2f) y=joy.y;
            }
            held.move=Vector2.ClampMagnitude(new Vector2(x,y),1);
            held.run=Input.GetKey(KeyCode.LeftShift)||Input.GetKey(KeyCode.RightShift)||Input.GetKey(KeyCode.JoystickButton4);
            held.jumpHeld=Input.GetKey(KeyCode.Space)||Input.GetKey(KeyCode.Z)||Input.GetKey(KeyCode.JoystickButton0);
            pending.jump |= Input.GetKeyDown(KeyCode.Space)||Input.GetKeyDown(KeyCode.Z)||Input.GetKeyDown(KeyCode.JoystickButton0);
            pending.attack |= Input.GetKeyDown(KeyCode.J)||Input.GetKeyDown(KeyCode.X)||Input.GetKeyDown(KeyCode.JoystickButton2);
            pending.grab |= Input.GetKeyDown(KeyCode.K)||Input.GetKeyDown(KeyCode.C)||Input.GetKeyDown(KeyCode.JoystickButton3);
            pending.interact |= Input.GetKeyDown(KeyCode.E)||Input.GetKeyDown(KeyCode.JoystickButton1);
            pending.action |= Input.GetKeyDown(KeyCode.U)||Input.GetKeyDown(KeyCode.JoystickButton5);
            pending.alternate |= Input.GetKeyDown(KeyCode.I)||Input.GetKeyDown(KeyCode.JoystickButton6);
            pending.pound |= Input.GetKeyDown(KeyCode.L);
        }
        public InputFrame Consume()
        {
            var result=pending; result.move=held.move; result.run=held.run; result.jumpHeld=held.jumpHeld;
            pending=default; return result;
        }
    }
}
