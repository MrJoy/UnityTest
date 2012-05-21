//-----------------------------------------------------------------
//  UnityTestController
//  Copyright 2009-2012 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-18 - jdf - Refactor testunner and GUI code considerably.
//  2009-09-05 - jdf - Deal with formatting a bit more gracefully and allow us
//                     to jam together a bit more data about how to format
//                     results.  Add support for an external skin with a
//                     fixed-width font.
//                   - Ditch console output until I can do a better job of it.
//                   - Make in-game GUI show in-editor, since Unity/iPhone
//                     throws gunk into the logs with custom windows.
//                   - Add support for captureFrameRate, including the ability
//                     to run multiple iterations under a variety of frame
//                     rates.
//  2009-07-03 - jdf - Capture results and add testrunner UI.
//  2009-07-02 - jdf - Initial version.
//
//-----------------------------------------------------------------
// A utility to iterate through all the scenes in a build, executing whichever
// test suites it encounters along the way in sequence before proceeding to the
// next scene.
//
// Drop an instance into scene 0 and enable iterateScenes to go through all your
// test scenes (as configured in your build), OR drop an instance into a scene
// with multiple UnityTest instances in it (with iterateScenes DISABLED) to
// coordinate the execution of several unit test suites.
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class SuiteResults {
  public SuiteResults(Type t, string v, ArrayList r) {
    suite = t;
    variant = v;
    results = r;
  }
  public Type suite;
  public string variant;
  public ArrayList results;
}

public class UnityTestController : MonoBehaviour {
  public bool iterateScenes = false;
  public string[] scenes;

  public int[] captureFramerates;

  [HideInInspector]
  public ArrayList aggregateResults = new ArrayList();

  private TestResultsViewState viewState = new TestResultsViewState();
  private ArrayList testSuites = new ArrayList();

  public static UnityTestController Instance;

  public void Awake() {
    Instance = this;
    if(UnityTest.TestRunner == null)
      UnityTest.TestRunner = this;
  }
  public void OnEnable() {
    Instance = this;
    if(UnityTest.TestRunner == null)
      UnityTest.TestRunner = this;
  }
  public void OnApplicationQuit() {
    if(UnityTest.TestRunner == this)
      UnityTest.TestRunner = null;
  }
  public void OnLevelWasLoaded() {
    testSuites.Clear();
    Instance = this;
    if(UnityTest.TestRunner == null)
      UnityTest.TestRunner = this;
    StartCoroutine(Start());
  }

  public void OnGUI() {
    if(UnityTest.TestRunner == this) {
      GUI.depth = -10;
      GUILayout.BeginArea(GUITools.FullScreen, "Unit Tests", "window");
        viewState = TestRunnerGUI.ShowAggregateResults(viewState, aggregateResults);
      GUILayout.EndArea();
    }
  }

  public IEnumerator Start() {
    // Skip a frame before, to let tests register themselves, etc.
    yield return 0;

    if((captureFramerates == null) || (captureFramerates.Length == 0))
      captureFramerates = new int[] { 0 };

    foreach(int fps in captureFramerates) {
      string variant = null;
      if(fps != 0) variant = " [@" + fps + " fps]";
      foreach(UnityTest suite in testSuites) {
        suite.captureFramerate = fps;
        SuiteResults current = new SuiteResults(suite.GetType(), variant, suite.results);
        aggregateResults.Add(current);
        yield return StartCoroutine(suite.TestAll());
        suite.results = new ArrayList();
      }
    }

    if(iterateScenes) {
      DontDestroyOnLoad(this);
      if(Application.loadedLevel < (Application.levelCount - 1)) {
        Application.LoadLevel(Application.loadedLevel + 1);
      }
    }
  }

  public void AddSuite(UnityTest suite) {
    testSuites.Add(suite);
  }
}
