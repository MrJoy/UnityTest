//-----------------------------------------------------------------
//  TestRunnerGUI
//  Copyright 2009-2012 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-18 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Helpers for GUI-based test runners.
//-----------------------------------------------------------------
using UnityEngine;
using System.Collections;

[System.Serializable]
public class TestResultStyle {
  public Color color = Color.white;
  public string statusMsg = "This bird is no more.  It has ceased to be.";

  protected TestResultStyle(Color c, string m) {
    color = c;
    statusMsg = m;
  }

  public static TestResultStyle Pass    = new TestResultStyle(new Color(0f,0.75f,0f,1f), "PASS");
  public static TestResultStyle Fail    = new TestResultStyle(new Color(1f,0.25f,0.25f,1f), "FAIL");
  public static TestResultStyle Ongoing = new TestResultStyle(new Color(0.85f,0.85f,0f,1f), "...");
}

[System.Serializable]
public class TestResultsViewState {
  public Vector2 innerScrollPosition, outerScrollPosition;
  public int selectedIndex;
  public string[] labels = new string[] {};
}

public static class TestRunnerGUI {
  public static TestResultsViewState ShowAggregateResults(TestResultsViewState state, ArrayList aggregateResults) {
    if(state.labels.Length != aggregateResults.Count) {
      state.labels = new string[aggregateResults.Count];
      for(int i = 0; i < state.labels.Length; i++) {
        SuiteResults sr = ((SuiteResults)(aggregateResults[i]));
        state.labels[i] = sr.suite.Name + ((sr.variant != null) ? sr.variant : "");
      }
    }
    GUILayout.BeginHorizontal(GUITools.ConstrainedBox);
      GUILayout.BeginVertical();
        GUITools.TextColorState tcs;
        state.outerScrollPosition = GUILayout.BeginScrollView(state.outerScrollPosition, false, false, GUILayout.Width(180));
          if((state.labels.Length > 0) && (state.selectedIndex >= 0)) {
            for(int i = 0; i < state.labels.Length; i++) {
              SuiteResults sr = ((SuiteResults)(aggregateResults[i]));
              TestResultStyle style = TestResultStyle.Pass;
              // TODO: UGH!  This SUCKS!  We shouldn't have to iterate every
              // result multiple times every frame!!!!
              bool isFailed = false;
              foreach(TestResult r in sr.results) {
                // If ANY result is ongoing, the result is Ongoing.
                // If there's no ongoing test AND there is ANY failed test, the
                // result is Failed.
                // Otherwise, the result is Passed.
                if(r.IsRunning) {
                  style = TestResultStyle.Ongoing;
                  if(Application.isPlaying)
                    state.selectedIndex = i;
                  break;
                } else if(r.Passed) { if(!isFailed) style = TestResultStyle.Pass; }
                else { style = TestResultStyle.Fail; isFailed = true; continue; }
              }

              tcs = GUITools.SetTextColor(style.color, "button");
              if(GUILayout.Toggle((state.selectedIndex == i), state.labels[i], "button", GUILayout.Width(150)))
                state.selectedIndex = i;
              GUITools.RestoreTextColor(tcs);
            }
          }
        GUILayout.EndScrollView();
      GUILayout.EndVertical();
      GUILayout.BeginVertical();
        if(state.labels.Length > 0) {
          if(state.selectedIndex >= state.labels.Length)
            state.selectedIndex = state.labels.Length - 1;
          GUILayout.Label(state.labels[state.selectedIndex]);
          if((state.selectedIndex >= 0) && (aggregateResults.Count > 0))
            state.innerScrollPosition = TestRunnerGUI.ShowSuiteResults(state.innerScrollPosition, ((SuiteResults)(aggregateResults[state.selectedIndex])).results);
        }
      GUILayout.EndVertical();
    GUILayout.EndHorizontal();

    return state;
  }

  public static Vector2 ShowSuiteResults(Vector2 scrollPosition, ArrayList results) {
    int passes = 0, fails = 0;
    int totalAssertions = 0, totalAssertionsFailed = 0;
    GUITools.TextColorState tcs;

    scrollPosition = GUILayout.BeginScrollView(scrollPosition, "box");
      foreach(TestResult r in results) {
        TestResultStyle rStyle;
        if(r.IsRunning) { rStyle = TestResultStyle.Ongoing; }
        else if(r.Passed) { rStyle = TestResultStyle.Pass; passes++; }
        else { rStyle = TestResultStyle.Fail; fails++; }

        tcs = GUITools.SetTextColor(rStyle.color, "label");
        GUILayout.Label(r.method.Name + ": " + rStyle.statusMsg);
        GUILayout.BeginHorizontal();
          GUILayout.Space(20);
          GUILayout.BeginVertical();
            foreach(TestMessage msg in r.messages) {
              GUILayout.Label(msg.message);
              if(msg.exception != null)
                GUILayout.Label(msg.exception.ToString());
              if(msg.trace != null)
                GUILayout.Label(msg.trace.ToString());
            }
          GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUITools.RestoreTextColor(tcs);

        totalAssertions += r.assertions;
        totalAssertionsFailed += r.assertionsFailed;
      }
    GUILayout.EndScrollView();

    GUILayout.BeginVertical("box");
      GUILayout.Label(totalAssertions + " assertions total, " + totalAssertionsFailed + " failed.");
      GUILayout.Label((passes+fails) + " tests, " + fails + " failures.");
    GUILayout.EndVertical();
    GUILayout.BeginHorizontal();
      GUILayout.FlexibleSpace();
    GUILayout.EndHorizontal();
    return scrollPosition;
  }

}
