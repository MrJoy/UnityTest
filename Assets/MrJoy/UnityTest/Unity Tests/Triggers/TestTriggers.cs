//-----------------------------------------------------------------
//  TestTriggers v0.32
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-18 - jdf - Update to get rid of legacy assert syntax.
//  2009-10-14 - jdf - Update so Setup/TearDown match access control from 
//                     UnityTest now that it's changed.
//  2009-09-05 - jdf - Do cleanup in TearDown to avoid polluting future 
//                     iterations.  Other minor cleanups and rigor added.
//  2009-07-04 - jdf - Updated to assert correctly given that we don't use
//                     exceptions anymore.
//  2009-07-02 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Example test suite to demonstrate usage of UnitTest framework by testing 
// various trigger behaviors and testing the viability of a workaround for an
// undesirable behavior.
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class TestTriggers : UnityTest {
  public TriggerTestBehavior dummyTower, dummyUnit; 

  [System.NonSerialized]
  private int callbackCount = 0;
  [System.NonSerialized]
  private bool towerEnter = false, towerExit = false, unitEnter = false, unitExit = false;
  [System.NonSerialized]
  private TriggerTestBehavior tower = null, unit = null;

  public void Awake() {
    Instance = this;
  }

  protected override void Setup() {    
    tower = (TriggerTestBehavior)Instantiate(dummyTower, Vector3.zero, Quaternion.identity);
    unit = (TriggerTestBehavior)Instantiate(dummyUnit, Vector3.zero, Quaternion.identity);
  }
  
  protected override void TearDown() {
    Destroy(tower.gameObject);
    Destroy(unit.gameObject);
    callbackCount = 0;
    towerEnter = towerExit = unitEnter = unitExit = false;
  }

  public void DoEnterCallback(string name) {
    if(name == (dummyTower.name + "(Clone)")) { towerEnter = true; callbackCount++; }
    else if(name == (dummyUnit.name + "(Clone)")) { unitEnter = true; callbackCount++; }
    else Debug.LogWarning("Unknown object name: " + name);
  }

  public void DoExitCallback(string name) {
    if(name == (dummyTower.name + "(Clone)")) { towerExit = true; callbackCount++; }
    else if(name == (dummyUnit.name + "(Clone)")) { unitExit = true; callbackCount++; }
    else Debug.LogWarning("Unknown object name: " + name);
  }

  private void AssertFlagStates(int callbacks, bool tEnter, bool tExit, bool uEnter, bool uExit) {
    AssertEqual(callbacks, callbackCount, "Expected " + callbacks + " callbacks, but got " + callbackCount);
    AssertEqual(tEnter, towerEnter, "Expected tower entry status of " + tEnter + " but got " + towerEnter);
    AssertEqual(tExit, towerExit, "Expected tower exit status of " + tExit + " but got " + towerExit);
    AssertEqual(uEnter, unitEnter, "Expected unit entry status of " + uEnter + " but got " + unitEnter);
    AssertEqual(uExit, unitExit, "Expected unit exit status of " + uExit + " but got " + unitExit);
  }
  
  public IEnumerator TestThatInstantiateInPlaceTriggersEnter() {
    // Shouldn't have anything until after a FixedUpdate.
    AssertFlagStates(0, false, false, false, false);

    yield return new WaitForFixedUpdate();

    AssertFlagStates(2, true, false, true, false);
  }

  public IEnumerator TestIncursionViaMovement() {
    Vector3 startPos = Vector3.one * 3f, endPos = Vector3.one * -3f;

    // Replacing default objects with ones with a different starting pos.
    TearDown();
    tower = (TriggerTestBehavior)Instantiate(dummyTower, Vector3.zero, Quaternion.identity);
    unit = (TriggerTestBehavior)Instantiate(dummyUnit, startPos, Quaternion.identity);

    // Shouldn't have anything until after a FixedUpdate.
    AssertFlagStates(0, false, false, false, false);

    // Move the object around.
    float elapsedTime = 0f;
    while(elapsedTime < 0.2f) {
      elapsedTime += Time.fixedDeltaTime;
      unit.rigidbody.position = Vector3.Lerp(startPos, endPos, elapsedTime/0.2f);
      yield return new WaitForFixedUpdate();
    }

    AssertFlagStates(4, true, true, true, true);
  }

  // Here we want to confirm the behavior that 
  public IEnumerator TestExitViaDisable() {
    // Shouldn't have anything until after a FixedUpdate.
    AssertFlagStates(0, false, false, false, false);

    yield return new WaitForFixedUpdate();

    // SHOULD have our enter events.
    AssertFlagStates(2, true, false, true, false);

    // Shut it down...
    unit.gameObject.SetActiveRecursively(false);

    yield return new WaitForFixedUpdate();

    // OH NOES!  No exit events!
    AssertFlagStates(2, true, false, true, false);
  }

  public IEnumerator TestExitWorkaround() {
    // Shouldn't have anything until after a FixedUpdate.
    AssertFlagStates(0, false, false, false, false);
    
    // Enable our hacks.
    tower.fixTermination = true;
    unit.fixTermination = true;

    yield return new WaitForFixedUpdate();

    // Should have our enter events...
    AssertFlagStates(2, true, false, true, false);

    unit.gameObject.SetActiveRecursively(false);

    // One gotcha:  We get our events IMMEDIATELY with this workaround, NOT 
    // after the next FixedUpdate!
    AssertFlagStates(4, true, true, true, true);

    yield return new WaitForFixedUpdate();

    // Should be good to go.
    AssertFlagStates(4, true, true, true, true);
  }
}
