//-----------------------------------------------------------------
//  ShouldThrowExceptionAttribute
//  Copyright 2009-2012 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-14 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Attribute to indicate that a test method is SUPPOSED to throw an exception.
//-----------------------------------------------------------------
using UnityEngine;

[System.AttributeUsage(System.AttributeTargets.Method)]
public class ShouldThrowExceptionAttribute : System.Attribute {
  public System.Type allowedExceptionType = typeof(System.Exception);
  public ShouldThrowExceptionAttribute() {}
  public ShouldThrowExceptionAttribute(System.Type t) {
    if(!(typeof(System.Exception).IsAssignableFrom(t))) {
      Debug.LogWarning("The class " + t.Name + " does not extend System.Exception!");
    } else {
      allowedExceptionType = t;
    }
  }
}
