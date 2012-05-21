//-----------------------------------------------------------------
//  TestUnityTestExceptionHandling v0.1 (2009-09-05)
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-09-05 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Validate various scenarios (from a code generation standpoint) in which an
// exception can be thrown within a test method, and ensure they do what we
// expect.
//
// Note that we allow the exceptions to be thrown here because we WANT to see
// that UnityTest actually produces an error when this happens.  Previously,
// an uncaught exception in UnityTest tended to produce a passing test result.
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class TestUnityTestExceptionHandling : UnityTest {
  public IEnumerator TestFailViaUncaughtExceptionWithSubsequentYield() {
    if(Random.Range(0, 1) < 100f) // Suppress a compiler warning.
      throw new System.Exception();
    yield return 0;;
  }

  public IEnumerator TestFailViaUncaughtExceptionWithPrecedingYield() {
    yield return 0;
    throw new System.Exception();
  }

  // Not actually a coroutine, so it throws from a different place.
  public IEnumerator TestFailViaUncaughtExceptionNoYields() {
    throw new System.Exception();
  }

  public void TestFailViaUncaughtException() {
    throw new System.Exception();
  }
}
