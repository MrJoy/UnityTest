//-----------------------------------------------------------------
//  UnityTest
//  Copyright 2009-2012 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-18 - jdf - Get rid of support for legacy assert syntax.
//                   - Add some support for backtraces.
//                   - Refactor testunner and GUI code considerably.
//  2009-10-14 - jdf - Add support for non-coroutine test methods.
//                   - Add support for expected exceptions.
//  2009-09-05 - jdf - Ditch console output until I can do a better job of it.
//                   - Make in-game GUI show in-editor, since Unity/iPhone
//                     throws gunk into the logs with custom windows.
//                   - Space test methods out a frame in order to avoid
//                     clobbering subsequent methods.
//                   - Add support for captureFrameRate.
//                   - Add proper exception handling support by controlling
//                     coroutine execution flow. (W00t!)
//                   - Eliminate need for ugly hack syntax of:
//                       if(AssertBlah(...)) { yield break; }
//  2009-07-16 - jdf - Handle tests that do no actual assertions better.
//  2009-07-03 - jdf - Capture results, add basic testunner UI.
//  2009-07-02 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Base class for unit tests.  Provides functionality for assertions and the
// like.  To use it, create a sub-class, and create various public instance
// methods with names beginning with "Test".
//
// Override "Setup" and "TearDown" to set up state that's needed across tests.
//
// TODO:
// -Yank out TestRunner/UI code into common class(es).
// -Write docs on best-practices, and general usage stuff.
// -Make a console testrunner.
// -Attempt to inspect scene to find leaked objects between test methods.
// -Collapse/show for detail info from messages.
// -Show current-status for ongoing test by looking at yield instruction.
// -Hide in-game GUI when editor GUI is active.
// -Make editor GUI *always* use multi-suite runner UI.
// -Make editor test runner provide meta-control for breakOnFailure.
// -Make editor test runner actually run tests, and provide an asset type that
//  can specify a set of scenes not in the build config for scene iteration that
//  doesn't interfere with workflow.
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Diagnostics;
using System.Text;
using System;

[System.Serializable]
public class UnityTestException : Exception {}

[System.Serializable]
public class TestMessage {
  public TestMessage(string m, StackTrace t, Exception e) {
    message = m;
    if(t != null)
      trace = FilterStackTrace(t);
    exception = e;
    FilterStackTrace(t);
  }
  public string message;
  public string trace;
  public Exception exception;

  private static string _AssetDirectory = String.Empty;
  private string AssetDirectory {
    get {
      if(_AssetDirectory == String.Empty) {
        string fullPath = new StackTrace(true).GetFrame(0).GetFileName();
        int idx = fullPath.IndexOf("/Assets/");
        _AssetDirectory = fullPath.Substring(0, idx + 8);
      }
      return _AssetDirectory;
    }
  }

  private string FilterStackTrace(StackTrace t) {
    StackFrame[] frames = t.GetFrames();
    StringBuilder sb = new StringBuilder();
    foreach(StackFrame f in frames) {
      try {
        string fileName = f.GetFileName();
        // Filter out UnityTest itself...
        if(fileName.EndsWith("UnityTest.cs")) continue;

        // Shorten filenames down to the salient bits.  Would use
        // Application.dataPath, BUUUT that approach doesn't help unless we're
        // running in the editor.
        fileName = fileName.Replace(AssetDirectory, "");

        // Add output in roughly the standard stack trace format:
        //   at Class.Method(Params) in FileName:line LineNum
        // TODO: Make this follow Unity conventions.
        sb
          .Append("\tat ").Append(f.GetMethod().ToString()
            .Replace("System.", "")).Append(" in ")
          .Append(fileName).Append(":line ").Append(f.GetFileLineNumber())
          .Append("\n");
      } catch(NullReferenceException) {
        // GetFileName can throw an NRE.  Presumably from generates code that
        // has no corresponding file?
      }
    }
    return sb.ToString();
  }
}

[System.Serializable]
public class TestResult {
  public TestResult(Type s, MethodInfo m, ArrayList msgs) {
    suite = s;
    method = m;
    messages = msgs;
    isRunning = true;
  }

  public Type suite;
  public MethodInfo method;
  public ArrayList messages;
  public bool isRunning;
  public int assertions, assertionsFailed;

  public bool IsRunning { get { return isRunning; } }
  public bool Passed { get { return assertionsFailed == 0; } }

  public void Done() {
    isRunning = false;
  }
}

public abstract class UnityTest : MonoBehaviour {
  public const string VERSION = "0.5.0";

  // ===========================================================================
  // Interface for sub-classes.
  // ===========================================================================
  // Enabling breakOnFailure will trigger the editor to pause when an assertion
  // fails, by Any Means Necessary in order to ensure that you can examine the
  // scene state in peace.
  public bool breakOnFailure = false;

  // Set this to test at a specific FPS.
  public int captureFramerate = 0;

  protected virtual void Setup() {}
  protected virtual void TearDown() {}

  protected void AssertTrue(bool condition) { AssertTrue(condition, "Expected true, got false"); }
  protected void AssertTrue(bool condition, string msg) {
    if(!condition) {
      _Fail(msg);
      throw new UnityTestException();
    } else {
      _Pass();
    }
  }

  protected void Fail() { Fail("Unexpected condition"); }
  protected void Fail(string msg) { AssertTrue(false, msg); }

  protected void AssertNotNull(object obj) { AssertNotNull(obj, "Expected non-null value, but got null."); }
  protected void AssertNotNull(object obj, string msg) { AssertTrue(obj != null, msg); }

  protected void AssertNull(object obj) { AssertNull(obj, "Expected null, but got: " + obj); }
  protected void AssertNull(object obj, string msg) { AssertTrue(obj == null, msg); }

  protected void AssertFalse(bool condition) { AssertTrue(!condition, "Expected false, but got true"); }
  protected void AssertFalse(bool condition, string msg) { AssertTrue(!condition, msg); }

  protected void AssertEqual(bool expectedValue, bool actualValue) { AssertEqual(expectedValue, actualValue, "Expected " + expectedValue + ", but got " + actualValue); }
  protected void AssertEqual(bool expectedValue, bool actualValue, string msg) { AssertTrue(expectedValue == actualValue, msg); }

  protected void AssertEqual(byte expectedValue, byte actualValue) { AssertEqual(expectedValue, actualValue, "Expected " + expectedValue + ", but got " + actualValue); }
  protected void AssertEqual(byte expectedValue, byte actualValue, string msg) { AssertTrue(expectedValue == actualValue, msg); }

  protected void AssertEqual(short expectedValue, short actualValue) { AssertEqual(expectedValue, actualValue, "Expected " + expectedValue + ", but got " + actualValue); }
  protected void AssertEqual(short expectedValue, short actualValue, string msg) { AssertTrue(expectedValue == actualValue, msg); }

  protected void AssertEqual(int expectedValue, int actualValue) { AssertEqual(expectedValue, actualValue, "Expected " + expectedValue + ", but got " + actualValue); }
  protected void AssertEqual(int expectedValue, int actualValue, string msg) { AssertTrue(expectedValue == actualValue, msg); }

  protected void AssertEqual(long expectedValue, long actualValue) { AssertEqual(expectedValue, actualValue, "Expected " + expectedValue + ", but got " + actualValue); }
  protected void AssertEqual(long expectedValue, long actualValue, string msg) { AssertTrue(expectedValue == actualValue, msg); }

  protected void AssertEqual(float expectedValue, float actualValue) { AssertEqual(expectedValue, actualValue, "Expected " + expectedValue + ", but got " + actualValue); }
  protected void AssertEqual(float expectedValue, float actualValue, string msg) { AssertTrue(expectedValue == actualValue, msg); }

  protected void AssertEqual(double expectedValue, double actualValue) { AssertEqual(expectedValue, actualValue, "Expected " + expectedValue + ", but got " + actualValue); }
  protected void AssertEqual(double expectedValue, double actualValue, string msg) { AssertTrue(expectedValue == actualValue, msg); }

  protected void AssertEqual(string expectedValue, string actualValue) { AssertEqual(expectedValue, actualValue, "Expected " + expectedValue + ", but got " + actualValue); }
  protected void AssertEqual(string expectedValue, string actualValue, string msg) { AssertTrue(expectedValue == actualValue, msg); }


  // ===========================================================================
  // Internal interface and implementation.
  // ===========================================================================
  private IEnumerator TestVanillaMethod(MethodInfo m) {
    try {
      m.Invoke(this, null);
    } catch(UnityTestException) {
      // Should already be marked as failed, so nothing to do here really.
      hasValue = false;
    } catch(System.Exception ex) {
      // This happens if there's no yield to turn the method into a
      // coroutine...
      exceptionThrown = ex;
      hasValue = false;
    }
    yield break;
  }

  private IEnumerator TestCoroutine(MethodInfo m) {
    IEnumerator e = null;
    try {
      e = (IEnumerator)m.Invoke(this, null);
    } catch(UnityTestException) {
      // Should already be marked as failed, so nothing to do here really.
      hasValue = false;
    } catch(System.Exception ex) {
      // This happens if there's no yield to turn the method into a
      // coroutine...
      exceptionThrown = ex;
      hasValue = false;
    }

    while(hasValue) {
      try {
        hasValue = e.MoveNext();
      } catch(UnityTestException) {
        // Should already be marked as failed, so nothing to do here really.
        hasValue = false;
      } catch(System.Exception ex) {
        exceptionThrown = ex;
        hasValue = false;
      }
      if(hasValue) yield return e.Current;
    }
  }

  // Can't do ref params to a coroutine so we carry this as instance state.
  // Alternative is a monolithic test method.  Yuck.
  private System.Exception exceptionThrown = null;
  private bool hasValue = true;

  private void DoSetup() {
    try {
      Setup();
    } catch(UnityTestException) {
      // Should already be marked as failed, so nothing to do here really.
      hasValue = false;
    } catch(System.Exception ex) {
      // Don't allow exceptions in Setup/Teardown.
      _Fail("Uncaught exception in Setup()", ex);
      hasValue = false;
    }
  }

  private void DoTearDown() {
    try {
      TearDown();
    } catch(System.Exception ex) {
      // Don't allow exceptions in Setup/Teardown.
      _Fail("Uncaught exception in TearDown()", ex);
      if(breakOnFailure)
        UnityEngine.Debug.Break();
      hasValue = false;
    }
  }

  private void CheckExceptions(MethodInfo m) {
    ShouldThrowExceptionAttribute[] attrs = (ShouldThrowExceptionAttribute[])m.GetCustomAttributes(typeof(ShouldThrowExceptionAttribute), true);
    ArrayList allowedExceptionTypes = null;
    if(attrs.Length > 0) {
      allowedExceptionTypes = new ArrayList();
      foreach(ShouldThrowExceptionAttribute s in attrs)
        allowedExceptionTypes.Add(s.allowedExceptionType);
    }

    bool wantException = (allowedExceptionTypes != null) && (allowedExceptionTypes.Count > 0);
    bool gotException = exceptionThrown != null;

    if(wantException && gotException) {
      bool gotRightKindOfException = false;
      foreach(System.Type et in allowedExceptionTypes) {
        if(et.IsAssignableFrom(exceptionThrown.GetType())) {
          gotRightKindOfException = true;
          break;
        }
      }
      if(!gotRightKindOfException) {
        // FAIL.
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append("Expected one type of exception, but got another.  Expected one of:");
        foreach(System.Type tt in allowedExceptionTypes)
          sb.Append(" ").Append(tt.Name);
        _Fail(sb.ToString(), exceptionThrown);
      }
    } else if(wantException && !gotException) {
      // FAIL.
      _Fail("Expected an exception, but did not get one.");
    } else if(!wantException && gotException) {
      // FAIL.
      _Fail("Unexpected exception.", exceptionThrown);
    }
  }

  public IEnumerator TestAll() {
    Time.captureFramerate = captureFramerate;
    Instance = this;
    Type t = GetType();
    MethodInfo[] methods = t.GetMethods(BindingFlags.Public | BindingFlags.Instance);
    // TODO: Sanity check things.  Make sure the method has no params, isn't
    // generic, isn't abstract, etc.

    // Order is randomized to minimize the likelihood of tests winding up
    // being accidentally order-dependant.
    ArrayList methodsInRandomOrder = new ArrayList();
    foreach(MethodInfo m in methods)
      methodsInRandomOrder.Insert(Mathf.RoundToInt(UnityEngine.Random.Range(0, methodsInRandomOrder.Count)), m);

    int testMethods = 0;
    foreach(MethodInfo m in methodsInRandomOrder) {
      if(m.Name.StartsWith("Test") && (m.Name != "TestAll")) {
        testMethods++;
        ResetTestState(t, m);

        DoSetup();

        // ---------------------------------------------------------------------
        if(hasValue) {
          if(typeof(IEnumerator).IsAssignableFrom(m.ReturnType))
            yield return StartCoroutine(TestCoroutine(m));
          else if(typeof(void).IsAssignableFrom(m.ReturnType))
            yield return StartCoroutine(TestVanillaMethod(m));
          else
            _Fail("Test method " + m.Name + " must be a coroutine or return void.  Returns: " + m.ReturnType.Name);
        }
        // ---------------------------------------------------------------------

        CheckExceptions(m);

        // Want to break out BEFORE teardown to make debugging possible.
        if(breakOnFailure && !currentResult.Passed) {
          UnityEngine.Debug.Break();
          yield return 0;
        }

        DoTearDown();

        AccumulateStats();

        // Skip a frame inbetween tests to avoid clobbering one another.
        yield return 0;
      }
    }
  }

  private void ResetTestState(Type t, MethodInfo m) {
    currentResult = new TestResult(t, m, new ArrayList());
    results.Add(currentResult);
    exceptionThrown = null;
    hasValue = true;
  }

  private void AccumulateStats() {
    currentResult.Done();
    totalTests++;
    if(currentResult.assertionsFailed > 0) totalTestsFailed++;
    totalAssertions += currentResult.assertions;
    totalAssertionsFailed += currentResult.assertionsFailed;
  }

  public ArrayList results = new ArrayList();

  private TestResult currentResult;

  public IEnumerator Start() {
    if(UnityTestController.Instance == null) {
      // Wait a frame for the engine to settle down wrt lifecycle weirdness.
      yield return 0;
      yield return StartCoroutine(TestAll());
    } else {
      UnityTestController.Instance.AddSuite(this);
    }
  }

  private void _Fail(string msg) {
    _Fail(msg, null);
  }

  private void _Fail(string msg, System.Exception ex) {
    currentResult.assertions++;
    currentResult.assertionsFailed++;
#if UNITY_IPHONE
    // Unity/iPhone can't pull a stack-trace on-device without crashing.  Ergo,
    // we must not call new StackTrace on-device.
    if(Application.platform == RuntimePlatform.iPhonePlayer)
      currentResult.messages.Add(new TestMessage(msg, null, ex));
    else
#endif
    currentResult.messages.Add(new TestMessage(msg, new StackTrace(true), ex));
  }

  private void _Pass() {
    currentResult.assertions++;
  }


  protected int totalTests = 0, totalTestsFailed = 0, totalAssertions = 0, totalAssertionsFailed = 0;

  protected Vector2 scrollPosition = Vector2.zero;
  public void OnGUI() {
    if(TestRunner == null) {
      GUI.depth = -10;
      GUILayout.BeginArea(GUITools.FullScreen, GetType().Name, "window");
        scrollPosition = TestRunnerGUI.ShowSuiteResults(scrollPosition, results);
      GUILayout.EndArea();
    }
  }

  public static UnityEngine.Object TestRunner = null;
  public static UnityTest Instance = null;
  public static Queue EditorSignals = new Queue();
}
