//-----------------------------------------------------------------
//  EditorTestRunner (formerly ResultViewer) v0.4
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-18 - jdf - Refactor testunner and GUI code considerably.
//  2009-10-12 - jdf - Remove spurious static var when it isn't needed.
//  2009-09-05 - jdf - A little refactoring of the UI code.
//                   - Ditch console output until I can do a better job of it.
//                   - Make in-game GUI show in-editor, since Unity/iPhone 
//                     throws gunk into the logs with custom windows.
//                   - ResultViewer and UnityTestController no longer 
//                     accidentally share state.
//  2009-07-16 - jdf - Handle tests that do no actual assertions better.
//  2009-07-03 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Edit-mode test runner class.
//-----------------------------------------------------------------
using UnityEditor;
using UnityEngine;
using System.Collections;

public class EditorTestRunner : EditorWindow {
#if UNITY_IPHONE
  private static EditorTestRunner window = null;
#endif
  
  [MenuItem("Window/UnityTest")]
  public static void ShowTestResults() {
#if UNITY_IPHONE
    if(window == null)
      window = new EditorTestRunner();

    window.Show(true);
#else
    EditorTestRunner window = (EditorTestRunner)EditorWindow.GetWindow(typeof(EditorTestRunner));
    window.Show();
    window.title = "UnityTest";
#endif
  }

  private TestResultsViewState viewState = new TestResultsViewState();
  private ArrayList aggregateResults = null;

  public void OnGUI() {
    if(UnityTestController.Instance != null)
      aggregateResults = UnityTestController.Instance.aggregateResults;
    GUILayout.BeginVertical(GUITools.ConstrainedBox);
      if(UnityTestController.Instance != null)
        viewState = TestRunnerGUI.ShowAggregateResults(viewState, aggregateResults);
      else if(UnityTest.Instance != null)
        viewState.innerScrollPosition = TestRunnerGUI.ShowSuiteResults(viewState.innerScrollPosition, UnityTest.Instance.results);
      else {
        GUILayout.Label("No test data...");
        GUILayout.FlexibleSpace();
      }
    GUILayout.EndVertical();
  }

  public void Update() {
    Repaint();
  }

#if UNITY_IPHONE
  public void OnCloseWindow() { 
    window = null;
    if(UnityTest.TestRunner == this)
      UnityTest.TestRunner = null;
    DestroyImmediate(this);
    Debug.Log("EditorTestRunner.OnCloseWindow");
  }
#endif

  public void OnEnable() {
#if UNITY_IPHONE
    if(window == null)
      window = this;
    Show(true);
#endif

    if(UnityTest.TestRunner == null)
      UnityTest.TestRunner = this;
  }

  public void OnDisable() {
    if(UnityTest.TestRunner == this)
      UnityTest.TestRunner = null;
  }
  
  public void OnDestroy() {
    if(UnityTest.TestRunner == this)
      UnityTest.TestRunner = null;
  }
}
