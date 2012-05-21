//-----------------------------------------------------------------
//  GUITools v0.2
//  Copyright 2009 MrJoy, Inc.
//  All rights reserved
//
//  2009-10-18 - jdf - Refactor testunner and GUI code considerably.
//  2009-09-05 - jdf - Initial version.
//
//-----------------------------------------------------------------
// Set of utilities/helpers for manipulating GUI state across UnityEditor AND
// normal UnityGUI (runtime) environment.
//
// TODO: Clone (and cache) style, don't modify it.
//-----------------------------------------------------------------
using UnityEngine;

public class GUITools {
  private static Rect _fs;
  public static Rect FullScreen {
    get {
      if((_fs.width != Screen.width) || (_fs.height != Screen.height))
        _fs = new Rect(0, 0, Screen.width, Screen.height);
      return _fs;
    }
  }

  public struct TextColorState {
    public TextColorState(string n, Color s, Color c) {
      styleName = n;
      styleColor = s;
      contentColor = c;
    }
    public string styleName;
    public Color styleColor;
    public Color contentColor;
  }

  public static TextColorState SetTextColor(Color newColor, string styleName) {
    GUIStyle style = GUI.skin.GetStyle(styleName);
    TextColorState tcs = new TextColorState(styleName, style.normal.textColor, GUI.contentColor);

    style.normal.textColor = newColor;
    GUI.contentColor = newColor;

    return tcs;
  }

  public static void RestoreTextColor(TextColorState oldColor) {
    GUIStyle style = GUI.skin.GetStyle(oldColor.styleName);
    style.normal.textColor = oldColor.styleColor;
    GUI.contentColor = oldColor.contentColor;
  }

  private static GUIStyle _ConstrainedBox;
  public static GUIStyle ConstrainedBox {
    get {
      if(_ConstrainedBox == null) {
        _ConstrainedBox = new GUIStyle(GUI.skin.GetStyle("box"));
        _ConstrainedBox.normal.background = null;
        _ConstrainedBox.border = new RectOffset();
        _ConstrainedBox.padding = new RectOffset();
        _ConstrainedBox.margin = new RectOffset();

        _ConstrainedBox.clipping = TextClipping.Clip;
        _ConstrainedBox.stretchWidth = false;
        _ConstrainedBox.stretchHeight = false;
        _ConstrainedBox.wordWrap = true;
      }
      return _ConstrainedBox;
    }
  }
}
