//-----------------------------------------------------------------
//  LifecycleBehaviour v0.1
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-09-05 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Set of utilities/helpers for tracking events that get fired against an object
// in order to produce tests thereof.
//
// Use ExpectedEvent and its subclasses to describe the details of the events 
// you're concerned with, and Evt to filter out irrelevant events.
//
// Quirks of Unity behavior will be documented via sub-classes of ExpectedEvent.
//
// Subclass LifecycleBehaviour to manipulate the object over its lifecycle.
//
// Subclass EventTest to get a couple extra helpful asserts.
//-----------------------------------------------------------------
using UnityEngine;
using System;
using System.Collections;

// This is *NOT* an exhaustive list of events!
[Flags]
public enum Evt : long {
  Awake              = 0x00000001,
  Start              = 0x00000002,
  Reset              = 0x00000004,
  OnLevelWasLoaded   = 0x00000008,
  OnEnable           = 0x00000010,
  OnDisable          = 0x00000020,
  Update             = 0x00000040,
  LateUpdate         = 0x00000080,
  FixedUpdate        = 0x00000100,
  OnCollisionEnter   = 0x00000200,
  OnCollisionStay    = 0x00000400,
  OnCollisionLeave   = 0x00000800,
  OnTriggerEnter     = 0x00001000,
  OnTriggerStay      = 0x00002000,
  OnTriggerLeave     = 0x00004000,
  OnGUI              = 0x00008000,
  OnApplicationPause = 0x00010000,
  OnApplicationQuit  = 0x00020000,
  
  SetEnabled         = 0x00040000,
  
  //                   Awake      Start      Reset      OnLevelWasLoaded OnEnable   OnDisable  OnApplicationQuit SetEnable
  Lifecycle          = 0x00000001+0x00000002+0x00000004+0x00000008      +0x00000010+0x00000020+0x00020000       +0x00040000,
  //                   OnCollision*
  Collisions         = 0x00000200+0x00000400+0x00000800,
  //                   OnTrigger*
  Triggers           = 0x00001000+0x00002000+0x00004000,
  //                   FixedUpdate OnCollision*                     OnTrigger*
  AllPhysics         = 0x00000100 +0x00000200+0x00000400+0x00000800+0x00001000+0x00002000+0x00004000,

  AllEvents          = 0xFFFFFFFF
}

public class EventHistory {
  public Evt name;
  public bool enabledState, activeState;
  public int frame;
  public float realtimeSinceStartup;
  public string otherData;

  protected EventHistory() {
    throw new System.Exception("Nope.");
  }

  public EventHistory(Evt n) {
    name = n;
    enabledState = true;
    activeState = true;
    frame = 0;
    realtimeSinceStartup = 0f;
    otherData = null;
  }

  public EventHistory(Evt n, bool e, bool a) : this(n) {
    enabledState = e;
    activeState = a;
  }

  public EventHistory(Evt n, int f) : this(n) {
    frame = f;
  }

  public EventHistory(Evt n, int f, float r) : this(n, f) {
    realtimeSinceStartup = r;
  }

  public EventHistory(Evt n, bool e, bool a, int f) : this(n, e, a) {
    frame = f;
  }

  public EventHistory(Evt n, bool e, bool a, int f, float r) : this(n, e, a, f) {
    realtimeSinceStartup = r;
  }

  public EventHistory(Evt n, bool e, bool a, int f, float r, string o) : this(n, e, a, f, r) {
    otherData = o;
  }

  public override string ToString() {
    string tmp = String.Format("{1:0000}@{0:f4}  {3}{4}  {2}", realtimeSinceStartup, frame, name, activeState ? "+" : "-", enabledState ? "+" : "-");
    if(otherData != null)
      tmp += "\t\t" + otherData;
    return tmp;
  }
  
  public static string ToString(Evt mask, ArrayList eventHistory) {
    System.Text.StringBuilder sb = new System.Text.StringBuilder();
    foreach(EventHistory h in eventHistory) {
      if((mask & h.name) != 0)
        sb.Append(h.ToString()).Append("\n");
    }
    return sb.ToString();
  }

  public static string ToString(EventHistory[] eventHistory) {
    System.Text.StringBuilder sb = new System.Text.StringBuilder();
    foreach(EventHistory h in eventHistory) {
      sb.Append(h.ToString()).Append("\n");
    }
    return sb.ToString();
  }
}

public class ExpectedEvent : EventHistory {
  public ExpectedEvent(Evt n) : base(n) {}
  public ExpectedEvent(Evt n, bool e, bool a) : base(n, e, a) {}
  public ExpectedEvent(Evt n, int f) : base(n, f) {}
  public ExpectedEvent(Evt n, int f, float r) : base(n, f, r) {}
  public ExpectedEvent(Evt n, bool e, bool a, int f) : base(n, e, a, f) {}
  public ExpectedEvent(Evt n, bool e, bool a, int f, float r) : base(n, e, a, f, r) {}
  public ExpectedEvent(Evt n, bool e, bool a, int f, float r, string o) : base(n, e, a, f, r, o) {}

  protected virtual void TimeToString(System.Text.StringBuilder sb) {
    if(frame > 0) sb.AppendFormat("{0:0000}", frame);
    else          sb.Append("****");

    if(realtimeSinceStartup == 0f) {
      sb.Append("@******");
    } else {
      sb.Append((realtimeSinceStartup < 0f) ? "+" : ">")
        .AppendFormat("{f4}", Mathf.Abs(realtimeSinceStartup));
    }
  }
  
  protected virtual void StatesToString(System.Text.StringBuilder sb) {
    sb.Append(activeState ? "+" : "-")
      .Append(enabledState ? "+" : "-");
  }

  protected virtual void OtherToString(System.Text.StringBuilder sb) {
    sb.Append(otherData);
  }

  public override string ToString() {
    System.Text.StringBuilder sb = new System.Text.StringBuilder();

    TimeToString(sb);
    sb.Append("  ");
    StatesToString(sb);
    sb.Append("  ");
    sb.Append(name);
    if(otherData != null) {
      sb.Append("\t\t");
      OtherToString(sb);
    }

    return sb.ToString();
  }

  protected virtual bool CommonChecks(EventHistory actualEvent) {
    if(actualEvent == null) return true;

    // Make sure we're looking at the right event type.
    if(name != actualEvent.name) return true;
    
    return false;
  }
  
  protected virtual bool CheckTimes(float lastTime, EventHistory actualEvent) {
    // Make sure we're on the exact frame we expect.
    if((frame > 0) && (frame != actualEvent.frame)) return true;
    // Make sure that at the specified time value is in the past (lets us test 
    // that a certain amount of time has been waited).
    if((realtimeSinceStartup > 0) && (realtimeSinceStartup > actualEvent.realtimeSinceStartup)) return true;
    else if(realtimeSinceStartup < 0) {
      // Use negative values to specify relative times.
      float tmp = Mathf.Abs(realtimeSinceStartup) + lastTime;
      if(tmp > actualEvent.realtimeSinceStartup) return true;
    }

    return false;
  }

  protected virtual bool CheckState(EventHistory actualEvent) {
    if((enabledState != actualEvent.enabledState) || (activeState != actualEvent.activeState)) return true;
    
    return false;
  }
  
  protected virtual bool CheckOther(EventHistory actualEvent) {
    // TODO: Make this less sucky.  Come up with something that's clean and easy
    // for various event types...
    if((otherData != null) && (otherData != actualEvent.otherData)) return true;

    return false;
  }

  protected virtual bool CheckAgainst(float lastTime, EventHistory actualEvent) {
    if(CommonChecks(actualEvent)) return false;
    if(CheckTimes(lastTime, actualEvent)) return false;
    if(CheckState(actualEvent)) return false;
    if(CheckOther(actualEvent)) return false;

    return true;
  }

  public static bool Assert(Evt mask, ExpectedEvent[] expected, ArrayList actual) {
    float lastTime = 0f;

    // TODO: Report meaningful and specific errors.
    int i = 0, j = 0, k = 0, upperBounds = 0;
    upperBounds = Mathf.Max(actual.Count, expected.Length);
    for(i = 0; i < upperBounds; i++) {
//Debug.Log("i=" + i + ", j=" + j + ", k=" + k + ", expected.Length=" + expected.Length + ", actual.Count=" + actual.Count);
      if(k >= actual.Count) return false;
      EventHistory a = actual[k] as EventHistory;
      if(a == null) throw new System.Exception("Got something other than EventHistory!  Got: " + actual[i]);
      if((a.name & mask) != 0) {
        if(j >= expected.Length) return false;
        if(!expected[j].CheckAgainst(lastTime, a)) return false;
        lastTime = a.realtimeSinceStartup;
        j++;
      }
      k++;
    }

    return true;
  }
}

// Helper for dealing with the realities of an OnDisable caused by a Destroy 
// against the gameObject.
//
// Special things to note about this event:
//
// 1) You cannot count on gameObject.active to have any specific value.  It may 
//    OR MAY NOT have the value it had before Destroy was called!
public class OnDisableFromDestroy : ExpectedEvent {
  public OnDisableFromDestroy() : base(Evt.OnDisable) {
    otherData = "F";
  }

  public OnDisableFromDestroy(bool e) : this() {
    enabledState = e;
  }

  public OnDisableFromDestroy(int f) : this() {
    frame = f;
  }

  public OnDisableFromDestroy(int f, float r) : this(f) {
    realtimeSinceStartup = r;
  }

  public OnDisableFromDestroy(bool e, int f) : this(e) {
    frame = f;
  }

  public OnDisableFromDestroy(bool e, int f, float r) : this(e, f) {
    realtimeSinceStartup = r;
  }

  protected override void StatesToString(System.Text.StringBuilder sb) {
    sb.Append("*")
      .Append(enabledState ? "+" : "-");
  }

  protected override bool CheckState(EventHistory actualEvent) {
    if(enabledState != actualEvent.enabledState) return true;
    
    return false;
  }

}

public class LifecycleBehaviour : MonoBehaviour {
  public ArrayList eventHistory = new ArrayList();

  public new bool enabled {
    get { return base.enabled; }
    set { 
      AddEvent(Evt.SetEnabled, value ? "T" : "F");
      base.enabled = value; 
    }
  }
  public virtual void Awake() { AddEvent(Evt.Awake); }
  public virtual void Start() { AddEvent(Evt.Start); }
  public virtual void Reset() { AddEvent(Evt.Reset); }

  public virtual void OnLevelWasLoaded(int l) { AddEvent(Evt.OnLevelWasLoaded, l.ToString()); }

  public virtual void OnEnable() { AddEvent(Evt.OnEnable); }
  public virtual void OnDisable() { AddEvent(Evt.OnDisable, (this == null) ? "T" : "F"); }

  public virtual void Update() { AddEvent(Evt.Update); }
  public virtual void LateUpdate() { AddEvent(Evt.LateUpdate); }
  public virtual void FixedUpdate() { AddEvent(Evt.FixedUpdate); }

  public virtual void OnCollisionEnter(Collision c) { AddEvent(Evt.OnCollisionEnter, c.collider.name + " [" + c.collider.GetInstanceID() + "]"); }
  public virtual void OnCollisionStay(Collision c) { AddEvent(Evt.OnCollisionStay, c.collider.name + " [" + c.collider.GetInstanceID() + "]"); }
  public virtual void OnCollisionLeave(Collision c) { AddEvent(Evt.OnCollisionLeave, c.collider.name + " [" + c.collider.GetInstanceID() + "]"); }

  public virtual void OnTriggerEnter(Collider c) { AddEvent(Evt.OnTriggerEnter, c.name + " [" + c.GetInstanceID() + "]"); }
  public virtual void OnTriggerStay(Collider c) { AddEvent(Evt.OnTriggerStay, c.name + " [" + c.GetInstanceID() + "]"); }
  public virtual void OnTriggerLeave(Collider c) { AddEvent(Evt.OnTriggerLeave, c.name + " [" + c.GetInstanceID() + "]"); }

  public virtual void OnGUI() { AddEvent(Evt.OnGUI, Event.current.type.ToString()); }

  public virtual void OnApplicationPause(bool p) { AddEvent(Evt.OnApplicationPause, p ? "T" : "F"); }
  public virtual void OnApplicationQuit() { AddEvent(Evt.OnApplicationQuit); }

  private void AddEvent(Evt name) { AddEvent(name, null); }

  private void AddEvent(Evt name, string otherData) {
    eventHistory.Add(new EventHistory(name, enabled, gameObject.active, Time.frameCount, Time.realtimeSinceStartup, otherData));
  }
}

// Helper class with some extra asserts.
public class EventTest : UnityTest {
  public void AssertEvents(Evt mask, ExpectedEvent[] expected, ArrayList actual, string msg) {
    AssertTrue(ExpectedEvent.Assert(mask, expected, actual), msg);
  }
  
  public void AssertEvents(Evt mask, ExpectedEvent[] expected, ArrayList actual) {
    AssertEvents(mask, expected, actual, "Event history did not match expected result.  Got:\n" + EventHistory.ToString(mask, actual) + "\n\nExpected:\n" + EventHistory.ToString(expected));
  }
}
