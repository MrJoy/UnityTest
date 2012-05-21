//-----------------------------------------------------------------
//  SelfDisableBehaviour v0.1
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-09-05 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Event lifecycle helper to see what happens when something disables itself 
// from Awake.
//-----------------------------------------------------------------
using UnityEngine;

public class SelfDisableBehaviour : LifecycleBehaviour {
  public override void Awake() {
    base.Awake();
    enabled = false;
  }
}
