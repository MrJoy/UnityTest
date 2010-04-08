//-----------------------------------------------------------------
//  TestLifecycle v0.2
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-18 - jdf - Start addressing behavior of references.
//  2009-09-05 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Attempts at documenting exact semantics of Unity object lifecycle under 
// various common conditions.
//-----------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;

public class TestLifecycle : EventTest {
  public IEnumerator TestSimpleLifecycleStuff() {
    int f = Time.frameCount;
    Evt eventsOfInterest = Evt.AllEvents & ~(Evt.AllPhysics | Evt.OnGUI);
    ExpectedEvent[] expectedResult = new ExpectedEvent[] {
      new ExpectedEvent(Evt.Awake, f),
      new ExpectedEvent(Evt.OnEnable, f),
      new ExpectedEvent(Evt.Start, f),
      new ExpectedEvent(Evt.LateUpdate, f),

      new ExpectedEvent(Evt.Update, f+1),
      new ExpectedEvent(Evt.LateUpdate, f+1),
      new OnDisableFromDestroy(f+1),
    };

    GameObject tmp = new GameObject("Simple Lifecycle", new Type[] { typeof(LifecycleBehaviour) });
    ArrayList eventHistory = ((LifecycleBehaviour)tmp.GetComponent(typeof(LifecycleBehaviour))).eventHistory;
    yield return 0;

    Destroy(tmp);
    yield return 0;

    AssertEvents(eventsOfInterest, expectedResult, eventHistory);
  }
  
  public IEnumerator TestSelfDisablingBehaviour() {
    int f = Time.frameCount;
    Evt eventsOfInterest = Evt.AllEvents & ~(Evt.AllPhysics | Evt.OnGUI);
    ExpectedEvent[] expectedResult = new ExpectedEvent[] {
      new ExpectedEvent(Evt.Awake, f),
      new ExpectedEvent(Evt.SetEnabled, f),
      new ExpectedEvent(Evt.OnDisable, false, true, f)
    };

    GameObject tmp = new GameObject("Self-Affecting Lifecycle", new Type[] { typeof(SelfDisableBehaviour) });
    ArrayList eventHistory = ((SelfDisableBehaviour)tmp.GetComponent(typeof(SelfDisableBehaviour))).eventHistory;
    yield return 0;
    yield return 0;

    Destroy(tmp);
    yield return 0;

    AssertEvents(eventsOfInterest, expectedResult, eventHistory);
  }

  public IEnumerator TestDanglingReferences() {
    GameObject go = new GameObject();
    EmptyComponent c = (EmptyComponent)go.AddComponent(typeof(EmptyComponent));
    AssertNotNull(go);
    AssertNotNull(c);

    yield return 0;

    AssertNotNull(go);
    AssertNotNull(c);

    Destroy(go);

    AssertNotNull(go);
    AssertNotNull(c);

    yield return 0;

    // Now, here's an interesting Unity-ism for ya.  The following is just a 
    // wee bit wacky, and can be damned bizarre if you aren't expecting it.

    // This part makes total sense:
    AssertTrue(go == null);
    // So does this:
    if(go) Fail();
    // But this is wacky.  Probably a consequence of the "object recycling"
    // that Unity allegedly does under the hood:
    AssertNotNull(go);

    // Makes total sense:
    AssertTrue(c == null);
    // Makes total sense:
    if(c) Fail();
    // Wacky:
    AssertNotNull(c);
  }

  public IEnumerator TestDanglingReferencesRedux() {
    GameObject go = new GameObject();
    EmptyComponent c = (EmptyComponent)go.AddComponent(typeof(EmptyComponent));

    yield return 0;

    Destroy(c);

    AssertNotNull(c);

    yield return 0;

    // Now, here's an interesting Unity-ism for ya.  The following is just a 
    // wee bit wacky, and can be damned bizarre if you aren't expecting it.

    // This part makes total sense:
    AssertTrue(c == null);
    // So does this:
    if(c) Fail();
    // But this is wacky.  Probably a consequence of the "object recycling"
    // that Unity allegedly does under the hood:
    AssertNotNull(c);
  }

}
