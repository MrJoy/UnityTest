using UnityEngine;
using System.Collections;

public class FrameworkReloading : MonoBehaviour {
  public int a1 = 0;
  protected int a2 = 0;
  private int a3 = 0;
  [HideInInspector]
  public int a4 = 0;
  [System.NonSerialized]
  public int a5 = 0;

  public string b1 = "n/a";
  protected string b2 = "n/a";
  private string b3 = "n/a";
  [HideInInspector]
  public string b4 = "n/a";
  [System.NonSerialized]
  public string b5 = "n/a";

  public string[] c1 = new string[] { "n/a" };
  protected string[] c2 = new string[] { "n/a" };
  private string[] c3 = new string[] { "n/a" };
  [HideInInspector]
  public string[] c4 = new string[] { "n/a" };
  [System.NonSerialized]
  public string[] c5 = new string[] { "n/a" };

  public ArrayList d1 = new ArrayList();
  protected ArrayList d2 = new ArrayList();
  private ArrayList d3 = new ArrayList();
  [HideInInspector]
  public ArrayList d4 = new ArrayList();
  [System.NonSerialized]
  public ArrayList d5 = new ArrayList();

  void Awake() { Debug.Log("FrameworkReloading.Awake()"); }
  void Start() { 
    Debug.Log("FrameworkReloading.Start()"); 
    a1 = 1; a2 = 2; a3 = 3; a4 = 4; a5 = 5;
    b1 = "b1"; b2 = "b2"; b3 = "b3"; b4 = "b4"; b5 = "b5";
    c1 = new string[] { "c1" }; c2 = new string[] { "c2" }; c3 = new string[] { "c3" }; c4 = new string[] { "c4" }; c5 = new string[] { "c5" };
    d1.Add("d1"); d2.Add("d2"); d3.Add("d3"); d4.Add("d4"); d5.Add("d5");
  }

  void OnEnable() { Debug.Log("FrameworkReloading.OnEnable()"); }
  void OnDisable() { Debug.Log("FrameworkReloading.OnDisable()"); }
  void OnLevelWasLoaded() { Debug.Log("FrameworkReloading.OnLevelWasLoaded()"); }
  void OnApplicationQuit() { Debug.Log("FrameworkReloading.OnApplicationQuit()"); }

  public void OnGUI() {
    GUILayout.Label("While in play mode, edit and save a source file in the project.");
    GUILayout.Label("This will show you what Unity will, and won't resurrect -- and what events happen at what times.");
    GUILayout.Space(20);
    GUILayout.Label("int: public a1 = " + a1 + "; protected a2 = " + a2 + "; private a3 = " + a3 + "; [HideInInspector] a4 = " + a4 + "; [NonSerialized] a5 = " + a5);
    GUILayout.Space(20);
    GUILayout.Label("string; public b1 = " + b1 + "; protected b2 = " + b2 + "; private b3 = " + b3 + "; [HideInInspector] b4 = " + b4 + "; [NonSerialized] b5 = " + b5);
    GUILayout.Space(20);
    GUILayout.Label("string[]; public c1 = " + A2S(c1) + "; protected c2 = " + A2S(c2) + "; private c3 = " + A2S(c3) + "; [HideInInspector] c4 = " + A2S(c4) + "; [NonSerialized] c5 = " + A2S(c5));
    GUILayout.Space(20);
    GUILayout.Label("ArrayList; public d1 = " + A2S(d1) + "; protected d2 = " + A2S(d2) + "; private d3 = " + A2S(d3) + "; [HideInInspector] d4 = " + A2S(d4) + "; [NonSerialized] d5 = " + A2S(d5));
  }

  private static System.Text.StringBuilder sb = new System.Text.StringBuilder();
  private static string A2S(string[] a) {
    sb.Length = 0;
    sb.Append("{");
    for(int i = 0; i < a.Length; i++) {
      sb.Append(a[i]);
      if(i < (a.Length - 1)) sb.Append(",");
    }
    sb.Append("}");
    return sb.ToString();
  }

  private static string A2S(ArrayList a) {
    sb.Length = 0;
    sb.Append("{");
    for(int i = 0; i < a.Count; i++) {
      sb.Append(a[i]);
      if(i < (a.Count - 1)) sb.Append(",");
    }
    sb.Append("}");
    return sb.ToString();
  }
}
