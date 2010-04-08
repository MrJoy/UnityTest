//-----------------------------------------------------------------
//  TriggerTestBehavior v0.1
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-07-02 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Helper class for TestTriggers to measure the behavior of triggers.  Also
// implements a workaround for purposes of testing its viability.
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class TriggerTestBehavior : MonoBehaviour {
  public bool commitSuicide = false, fixTermination = true;

  public Hashtable collidersInContact = new Hashtable();

  void OnTriggerEnter(Collider c) {
    ((TestTriggers)UnityTest.Instance).DoEnterCallback(name);
    if(fixTermination)
      collidersInContact[c] = true;

    if(commitSuicide)
      Destroy(gameObject);
  }

  void OnDisable() {
    if(fixTermination) {
      ArrayList tmp = new ArrayList(collidersInContact.Keys);
      foreach(Collider cc in tmp) {
        if(cc != null) {
          OnTriggerExit(cc);
          cc.gameObject.SendMessage("OnTriggerExit", collider, SendMessageOptions.DontRequireReceiver);
        }
      }
    }
  }

  void OnTriggerExit(Collider c) {
    ((TestTriggers)UnityTest.Instance).DoExitCallback(name);
    if(fixTermination && collidersInContact.ContainsKey(c))
      collidersInContact.Remove(c);
  }
}
