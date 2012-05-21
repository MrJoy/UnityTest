//-----------------------------------------------------------------
//  TestUnityTestExceptionHandlingOverrides v0.1 (2009-10-14)
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-14 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Validate various scenarios (from a code generation standpoint) in which an
// exception can be thrown within a test method, and ensure they do what we
// expect.
//
// Here we test that the ShouldThrowException attribute does what it is 
// intended to, which is allow a test to pass ONLY if an uncaught exception is
// thrown.
//
// TODO: Extend ShouldThrowException to allow specifying WHICH exception can be
// thrown.
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;

public class TestUnityTestExceptionHandlingOverrides : UnityTest {
  [ShouldThrowException]
  public IEnumerator TestPassViaUncaughtExceptionWithSubsequentYield() {
    if(Random.Range(0, 1) < 100f) // Suppress a compiler warning.
      throw new System.Exception();
    yield return 0;;
  }

  [ShouldThrowException]
  public IEnumerator TestPassViaUncaughtExceptionWithPrecedingYield() {
    yield return 0;
    throw new System.Exception();
  }

  // Not actually a coroutine, so it throws from a different place.
  [ShouldThrowException]
  public IEnumerator TestPassViaUncaughtExceptionNoYields() {
    throw new System.Exception();
  }

  [ShouldThrowException(typeof(System.NullReferenceException))]
  public IEnumerator TestFailCoroutineViaUncaughtExceptionOfWrongType() {
    yield return 0;
    throw new System.Exception();
  }

  [ShouldThrowException]
  public void TestPassViaUncaughtException() {
    throw new System.Exception();
  }

  [ShouldThrowException(typeof(System.NullReferenceException))]
  public void TestFailMethodViaUncaughtExceptionOfWrongType() {
    throw new System.Exception();
  }
}
