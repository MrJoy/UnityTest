//-----------------------------------------------------------------
//  TestUnityTestStates v0.1 (2009-09-05)
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-09-05 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Validate that pass, fail, and ongoing methods get colored appropriately in 
// the test runner.
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class TestUnityTestStates : UnityTest {
  public float timeToPauseForObservation = 5.0f;

  public IEnumerator TestPassWithNoAsserts() {
    yield break;
  }
  
  public IEnumerator TestPassWithAssert() {
    AssertTrue(true, "Truth is truth.");
    
    yield break;
  }
  
  public IEnumerator TestFail() {
    Fail("Expected failure.");

    yield break;
  }

  public IEnumerator TestFailWithAssert() {
    AssertTrue(false, "False is not truth.");

    yield break;
  }

  // Wait for a while so we see the yellow "ongoing" indicator.
  public IEnumerator TestOngoingThenPass() {
    // Sit and spin for a while so we can observe this state.
    yield return new WaitForSeconds(timeToPauseForObservation);
  }
}
