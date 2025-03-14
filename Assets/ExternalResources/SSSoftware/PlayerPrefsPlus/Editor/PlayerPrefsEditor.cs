/*
 * This File and its contents are Copyright SteveSmith.Software 2018. All rights Reserved.
 *
 * https://stevesmith.software
 *
 * This software is licensed under a Creative Commons Attribution-NoDerivatives 4.0 International License
 * http://creativecommons.org/licenses/by-nd/4.0/
 *
 */
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PlayerPrefsEditor : EditorWindow
{
  public string version = "V.1.0.2";

  string message = "Powered by SQL for Unity V.1.0.7";
  static EditorWindow window;
  Rect DBrect = new Rect(10, 20, 300, 650);
  bool NewPref = false;
  Rect PrefRect = new Rect(1150, 30, 300, 200);
  bool NewPlayer = false;
  Rect PlayerRect = new Rect(1200, 50, 300, 200);
  Rect msgRect = new Rect(10, 680, 1000, 20);
  Vector2 scroll = Vector2.zero;
  string inputVal = string.Empty;
  int maxID = -1;
  int typeIx = 0;
  int prefIx = 0;

  SQL4Unity.SQLExecute sql = null;
  SQL4Unity.SQLResult sqlresult = new SQL4Unity.SQLResult();
  PlayerPrefsSetup pps;

  Dictionary<int, PrefsPlayer> Players = new Dictionary<int, PrefsPlayer>();
  Dictionary<string, playerprefsplus_pref> Prefs =
    new Dictionary<string, playerprefsplus_pref>();
  string[] dataTypes = new string[]
  {
    "int",
    "string",
    "bool",
    "float",
    "long",
    "byte",
    "DateTime",
    "Vector2",
    "Vector3",
    "Vector4",
    "Color",
    "Quaternion",
    "Rect",
    "Sprite",
    "Resource",
    "GameObject",
  };
  List<string> PrefDD = new List<string>();

  [MenuItem("Tools/Player Prefs Plus/Editor")]
  public static void ShowWindow()
  {
    window = EditorWindow.GetWindow(typeof(PlayerPrefsEditor));
    window.minSize = new UnityEngine.Vector2(1505, 700);
    window.titleContent = new GUIContent("Prefs Editor");
  }

  void OnEnable()
  {
    try
    {
      if (sql == null)
        sql = new SQL4Unity.SQLExecute("PlayerPrefsPlus");
      pps = new PlayerPrefsSetup();
      ReadPlayers();
      ReadPrefs();
    }
    catch (Exception ex)
    {
      if (ex.GetType() == typeof(FileNotFoundException))
      {
        Debug.Log("Missing Database");
        Debug.Log(
          "Please refer to Assets/SSSoftware/PlayerPrefsPlus/ReadMe.txt"
        );
      }
    }
  }

  void ReadPlayers()
  {
    string query = "Select * from Player Order By ID";
    sql.Command(query, sqlresult);
    playerprefsplus_player[] players = sqlresult.Get<playerprefsplus_player>();
    for (int i = 0; i < players.Length; i++)
    {
      if (!Players.ContainsKey(players[i].ID))
      {
        PrefsPlayer player = new PrefsPlayer();
        player.name = players[i].Title;
        player.id = players[i].ID;
        Players.Add(player.id, player);
      }
      else
      {
        PrefsPlayer p = Players[players[i].ID];
        p.name = players[i].Title;
      }
      maxID = players[i].ID;
    }
  }

  void ReadPrefs()
  {
    string query = "Select * from Pref Order By Title";
    sql.Command(query, sqlresult);
    playerprefsplus_pref[] prefs = sqlresult.Get<playerprefsplus_pref>();
    Prefs.Clear();
    PrefDD.Clear();
    PrefDD.Add("Select...");
    for (int i = 0; i < prefs.Length; i++)
    {
      Prefs.Add(prefs[i].Title, prefs[i]);
      PrefDD.Add(prefs[i].Title);
    }
  }

  void OnGUI()
  {
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.LabelField(
      "Player Prefs Plus Editor version " + version + ".",
      EditorStyles.boldLabel
    );
    if (GUILayout.Button("New Preference", GUILayout.ExpandWidth(false)))
      NewPref = !NewPref;
    if (GUILayout.Button("New Player", GUILayout.ExpandWidth(false)))
      NewPlayer = !NewPlayer;
    EditorGUILayout.EndHorizontal();
    BeginWindows();
    GUILayout.Window(0, DBrect, showPlayers, "Players");
    foreach (KeyValuePair<int, PrefsPlayer> kvp in Players)
    {
      PrefsPlayer player = kvp.Value;
      if (player.isOpen)
      {
        player.rect = GUILayout.Window(
          kvp.Key,
          player.rect,
          showPrefs,
          player.name
        );
      }
    }
    if (NewPref)
      GUILayout.Window(maxID + 1, PrefRect, CreatePref, "New Preference");
    if (NewPlayer)
      GUILayout.Window(maxID + 2, PlayerRect, CreatePlayer, "New Player");
    EndWindows();
    EditorGUI.LabelField(msgRect, message, EditorStyles.boldLabel);
  }

  void CreatePref(int winID)
  {
    GUILayout.Space(10);
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.BeginVertical();
    EditorGUILayout.LabelField(
      "Preference",
      EditorStyles.boldLabel,
      GUILayout.Width(75)
    );
    GUILayout.Space(5);
    EditorGUILayout.LabelField(
      "Data Type",
      EditorStyles.boldLabel,
      GUILayout.Width(75)
    );
    EditorGUILayout.EndVertical();
    EditorGUILayout.BeginVertical();
    inputVal = EditorGUILayout.TextField(inputVal, GUILayout.Width(200));
    GUILayout.Space(5);
    typeIx = EditorGUILayout.Popup(typeIx, dataTypes, GUILayout.Width(120));
    string dataType = dataTypes[typeIx];
    EditorGUILayout.EndVertical();
    EditorGUILayout.EndHorizontal();
    GUILayout.Space(10);
    if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
      SavePref(inputVal, dataType);
  }

  void SavePref(string pref, string datatype)
  {
    if (string.IsNullOrEmpty(pref))
    {
      message = "A Preference Name must be entered";
      return;
    }
    if (pps.AddPref(pref, datatype))
    {
      ReadPrefs();
      NewPref = false;
      inputVal = string.Empty;
      message = "New Preference " + pref + " Created";
    }
    else
    {
      message = "Invalid or Duplicate Preference name";
    }
  }

  void CreatePlayer(int winID)
  {
    GUILayout.Space(10);
    EditorGUILayout.BeginHorizontal();
    EditorGUILayout.LabelField(
      "Player",
      EditorStyles.boldLabel,
      GUILayout.Width(75)
    );
    inputVal = EditorGUILayout.TextField(inputVal, GUILayout.Width(200));
    EditorGUILayout.EndHorizontal();
    GUILayout.Space(10);
    if (GUILayout.Button("Save", GUILayout.ExpandWidth(false)))
      SavePlayer(inputVal);
  }

  void SavePlayer(string newPlayer)
  {
    if (string.IsNullOrEmpty(newPlayer))
    {
      message = "A Player Name must be entered";
      return;
    }
    if (pps.AddPlayer(newPlayer))
    {
      ReadPlayers();
      NewPlayer = false;
      inputVal = string.Empty;
      message = "New Player " + newPlayer + " Created";
    }
    else
    {
      message = "Invalid or Duplicate Player name";
    }
  }

  void showPlayers(int winID)
  {
    EditorGUILayout.BeginVertical();
    scroll = EditorGUILayout.BeginScrollView(scroll);
    foreach (KeyValuePair<int, PrefsPlayer> kvp in Players)
    {
      PrefsPlayer player = kvp.Value;
      if (player.isOpen)
        EditorGUILayout.BeginHorizontal();
      bool wasOpen = player.isOpen;
      player.isOpen = EditorGUILayout.Foldout(player.isOpen, player.name);
      if (player.isOpen)
      {
        if (!wasOpen)
          EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Options", GUILayout.Width(70)))
        {
          player.currPref = string.Empty;
          GenericMenu menu = new GenericMenu();
          menu.AddItem(new GUIContent("Refresh"), false, RefreshPlayer, player);
          menu.AddItem(
            new GUIContent("Delete Player"),
            false,
            DeletePlayer,
            player
          );
          menu.AddItem(
            new GUIContent("Delete ALL Preferences"),
            false,
            DeleteAllPlayerPref,
            player
          );
          menu.AddItem(
            new GUIContent("Delete ALL Preference Values"),
            false,
            DeleteAllPlayerPrefVals,
            player
          );
          if (player.id > 1)
            menu.AddItem(
              new GUIContent("Set Default Preferences & Values"),
              false,
              DefaultPlayerPrefVals,
              player
            );
          menu.ShowAsContext();
        }
        EditorGUILayout.EndHorizontal();

        if (player.prefs == null)
          player.Get(Prefs);
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(
          "Preferences",
          EditorStyles.boldLabel,
          GUILayout.Width(120)
        );
        foreach (string key in player.prefs.Keys)
        {
          string datatype = Prefs[key].Type;
          EditorGUILayout.BeginHorizontal();
          EditorGUILayout.LabelField(key, GUILayout.Width(120));
          EditorGUILayout.LabelField(datatype, GUILayout.Width(80));
          if (GUILayout.Button("Options", GUILayout.Width(70)))
          {
            player.currPref = key;
            GenericMenu menu = new GenericMenu();
            menu.AddItem(
              new GUIContent("Delete Preference"),
              false,
              DeletePlayerPref,
              player
            );
            menu.AddItem(
              new GUIContent("Delete Preference Value(s)"),
              false,
              DeletePlayerPrefVals,
              player
            );
            menu.ShowAsContext();
          }
          EditorGUILayout.EndHorizontal();
        }
        GUILayout.Space(5);
        prefIx = EditorGUILayout.Popup(
          prefIx,
          PrefDD.ToArray(),
          GUILayout.Width(120)
        );
        if (prefIx > 0)
        {
          string newPref = PrefDD[prefIx];
          if (!player.prefs.ContainsKey(newPref))
          {
            PlayerPrefsSQL sql1 = new PlayerPrefsSQL();
            sql1.PlayerSet(player.id, newPref, null);
            sql1.Close();
            player.prefs = null;
            prefIx = 0;
          }
        }
        GUILayout.Space(10);
        EditorGUILayout.EndVertical();
      }
      if (wasOpen && !player.isOpen)
        EditorGUILayout.EndHorizontal();
    }
    EditorGUILayout.EndScrollView();
    EditorGUILayout.EndVertical();
  }

  void RefreshPlayer(object parm)
  {
    PrefsPlayer player = (PrefsPlayer)parm;
    player.prefs = null;
  }

  void DeletePlayer(object parm)
  {
    PrefsPlayer player = (PrefsPlayer)parm;
    player.isOpen = false;
    pps.DeletePlayer(player.id);
    Players.Remove(player.id);
  }

  void DeleteAllPlayerPref(object parm)
  {
    PrefsPlayer player = (PrefsPlayer)parm;
    PlayerPrefsSQL sql = new PlayerPrefsSQL();
    sql.PlayerDeleteAll(player.id);
    sql.Close();
    player.prefs = null;
  }

  void DeleteAllPlayerPrefVals(object parm)
  {
    PrefsPlayer player = (PrefsPlayer)parm;
    PlayerPrefsSQL sql = new PlayerPrefsSQL();
    sql.PlayerDeleteValues(player.id);
    sql.Close();
    player.prefs = null;
  }

  void DeletePlayerPref(object parm)
  {
    PrefsPlayer player = (PrefsPlayer)parm;
    PlayerPrefsSQL sql = new PlayerPrefsSQL();
    sql.PlayerDeleteKey(player.id, player.currPref);
    sql.Close();
    player.prefs = null;
  }

  void DeletePlayerPrefVals(object parm)
  {
    PrefsPlayer player = (PrefsPlayer)parm;
    PlayerPrefsSQL sql = new PlayerPrefsSQL();
    sql.PlayerDeleteKeyValues(player.id, player.currPref);
    sql.Close();
    player.prefs = null;
  }

  void DefaultPlayerPrefVals(object parm)
  {
    PrefsPlayer player = (PrefsPlayer)parm;
    PlayerPrefsSQL sql = new PlayerPrefsSQL();
    sql.SetPlayerDefaults(player.id);
    sql.Close();
    player.prefs = null;
  }

  void showPrefs(int winID)
  {
    PrefsPlayer player = Players[winID];
    EditorGUILayout.BeginVertical();

    player.scroll = EditorGUILayout.BeginScrollView(player.scroll);
    EditorGUILayout.BeginHorizontal();
    foreach (KeyValuePair<string, playerprefsplus_pref> kvp in Prefs)
    {
      if (player.prefs.ContainsKey(kvp.Key))
      {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(
          kvp.Key,
          EditorStyles.boldLabel,
          GUILayout.Width(120)
        );
        GUILayout.Space(15);
        EditorGUILayout.BeginHorizontal();
        object prefVal;
        if (player.newPrefs.ContainsKey(kvp.Key))
        {
          prefVal = showValue(
            kvp.Key,
            player.newPrefs[kvp.Key],
            -1,
            kvp.Value.Type,
            player
          );
        }
        else
        {
          prefVal = showValue(kvp.Key, null, -1, kvp.Value.Type, player);
        }
        if (GUILayout.Button("Add", GUILayout.Width(40)))
        {
          bool ok = true;
          if (PrefsPlayer.useString(kvp.Value.Type))
          {
            object o;
            if (
              !SQL4Unity.DataType.isValid(
                kvp.Value.Type,
                SQL4Unity.DataType.ToString(prefVal, true),
                ref message,
                out o
              )
            )
            {
              ok = false;
            }
          }
          if (ok)
          {
            player.isChanged = true;
            if (!player.chgPrefs.Contains(kvp.Key))
              player.chgPrefs.Add(kvp.Key);
            object val = player.prefs[kvp.Key];
            if (val == null)
            {
              player.prefs[kvp.Key] = prefVal;
            }
            else
            {
              // Add to existing preference values;
              if (val.GetType().IsArray)
              {
                object[] vals = (object[])val;
                object[] newVals = new object[vals.Length + 1];
                for (int i = 0; i < vals.Length; i++)
                {
                  newVals[i] = vals[i];
                }
                newVals[vals.Length] = prefVal;
                player.prefs[kvp.Key] = newVals;
              }
              else
              {
                object[] newVals = new object[2];
                newVals[0] = val;
                newVals[1] = prefVal;
                player.prefs[kvp.Key] = newVals;
              }
            }
            player.newPrefs.Remove(kvp.Key);
          }
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10);
        object value = player.prefs[kvp.Key];
        if (value != null)
        {
          if (value.GetType().IsArray)
          {
            object[] vals = (object[])value;
            for (int i = 0; i < vals.Length; i++)
            {
              showValue(kvp.Key, vals[i], i, kvp.Value.Type, player);
            }
          }
          else
          {
            showValue(kvp.Key, value, 0, kvp.Value.Type, player);
          }
        }
        EditorGUILayout.EndVertical();
      }
    }
    EditorGUILayout.EndHorizontal();
    EditorGUILayout.EndScrollView();
    if (player.isChanged)
    {
      if (GUILayout.Button("Save", GUILayout.Width(70)))
      {
        bool isValid = true;
        for (int i = 0; i < player.chgPrefs.Count; i++)
        {
          playerprefsplus_pref Pref = Prefs[player.chgPrefs[i]];
          object value = player.prefs[Pref.Title];
          if (value != null)
          {
            if (value.GetType().IsArray)
            {
              object[] vals = (object[])value;
              for (int j = 0; j < vals.Length; j++)
              {
                object newVal = vals[j];
                if (PrefsPlayer.useString(Pref.Type))
                {
                  string strVal = SQL4Unity.DataType.ToString(vals[j], true);
                  if (
                    !SQL4Unity.DataType.isValid(
                      Pref.Type,
                      strVal,
                      ref message,
                      out newVal
                    )
                  )
                  {
                    isValid = false;
                    GUI.FocusControl("Pref" + Pref.Title + j);
                    break;
                  }
                  vals[j] = SQL4Unity.DataType.convert(Pref.Type, newVal, null);
                }
                if (SQL4Unity.DataType.isObject(Pref.Type))
                {
                  vals[j] = getObject(Pref.Type, newVal);
                }
              }
              if (!isValid)
                break;
              player.prefs[Pref.Title] = vals;
            }
            else
            {
              if (SQL4Unity.DataType.isObject(Pref.Type))
              {
                player.prefs[Pref.Title] = getObject(Pref.Type, value);
              }
              if (PrefsPlayer.useString(Pref.Type))
              {
                object newVal;
                string strVal = SQL4Unity.DataType.ToString(value, true);
                if (
                  !SQL4Unity.DataType.isValid(
                    Pref.Type,
                    strVal,
                    ref message,
                    out newVal
                  )
                )
                {
                  isValid = false;
                  GUI.FocusControl("Pref" + Pref.Title + "0");
                  break;
                }
                //player.prefs[Pref.Title] = SQL4Unity.DataType.convert(Pref.Type, newVal);
                player.prefs[Pref.Title] = newVal;
              }
            }
          }
        }
        if (isValid)
        {
          player.Set();
          player.Get(Prefs);
        }
      }
    }
    EditorGUILayout.EndVertical();
    GUI.DragWindow();
  }

  SQL4Unity.Object getObject(string dataType, object o)
  {
    string path = UnityEditor.AssetDatabase.GetAssetPath((UnityEngine.Object)o);
    SQL4Unity.Object so = new SQL4Unity.Object(
      path,
      dataType,
      (UnityEngine.Object)o,
      null
    );
    return so;
  }

  object showValue(
    string key,
    object value,
    int ix,
    string dataType,
    PrefsPlayer player
  )
  {
    GUI.SetNextControlName("Pref" + key + ix);
    object newVal = null;
    switch (dataType.ToLower())
    {
      case "int":
        if (value == null)
          value = (int)0;
        newVal = EditorGUILayout.IntField(
          GUIContent.none,
          (int)value,
          GUILayout.Width(120)
        );
        break;
      case "float":
        if (value == null)
          value = 0.0F;
        newVal = EditorGUILayout.FloatField(
          GUIContent.none,
          (float)value,
          GUILayout.Width(120)
        );
        break;
      case "long":
        if (value == null)
          value = (long)0;
        newVal = EditorGUILayout.LongField(
          GUIContent.none,
          (long)value,
          GUILayout.Width(120)
        );
        break;
      case "bool":
      case "datetime":
      case "vector2":
      case "vector3":
      case "vector4":
      case "quaternion":
      case "rect":
      case "byte":
      case "string":
        newVal = EditorGUILayout.TextField(
          SQL4Unity.DataType.ToString(value, true),
          GUILayout.Width(120)
        );
        break;
      case "gameobject":
        newVal = (GameObject)
          EditorGUILayout.ObjectField(
            (GameObject)value,
            typeof(GameObject),
            true
          );
        break;
      case "color":
        if (value == null)
          value = new Color(0, 0, 0, 0);
        newVal = EditorGUILayout.ColorField(
          GUIContent.none,
          (Color)value,
          GUILayout.Width(120)
        );
        break;
      case "sprite":
        newVal = (Sprite)
          EditorGUILayout.ObjectField((Sprite)value, typeof(Sprite), false);
        break;
      case "resource":
        newVal = EditorGUILayout.ObjectField(
          (UnityEngine.Object)value,
          typeof(UnityEngine.Object),
          false
        );
        break;
      default:
        newVal = null;
        break;
    }
    if (newVal != null)
    {
      if (!newVal.Equals(value))
      {
        if (ix == -1)
        {
          if (player.newPrefs.ContainsKey(key))
          {
            player.newPrefs[key] = newVal;
          }
          else
          {
            player.newPrefs.Add(key, newVal);
          }
        }
        else
        {
          player.isChanged = true;
          if (!player.chgPrefs.Contains(key))
            player.chgPrefs.Add(key);
          object val = player.prefs[key];
          // Updating existing preference value;
          if (val.GetType().IsArray)
          {
            object[] vals = (object[])val;
            vals[ix] = newVal;
            player.prefs[key] = vals;
          }
          else
          {
            player.prefs[key] = newVal;
          }
        }
      }
    }
    if (
      ix >= 0
      && Event.current.button == 1
      && GUI.GetNameOfFocusedControl() == "Pref" + key + ix
    )
    {
      player.currPref = key;
      player.currPrefIx = ix;
      GenericMenu menu = new GenericMenu();
      menu.AddItem(new GUIContent("Delete Value"), false, DeleteValue, player);
      menu.ShowAsContext();
    }
    return newVal;
  }

  void DeleteValue(object parm)
  {
    PrefsPlayer player = (PrefsPlayer)parm;
    object o = player.prefs[player.currPref];
    if (o.GetType().IsArray)
    {
      object[] oldVals = (object[])o;
      object[] newVals = new object[oldVals.Length - 1];
      int j = 0;
      for (int i = 0; i < oldVals.Length; i++)
      {
        if (i != player.currPrefIx)
        {
          newVals[j] = oldVals[i];
          j++;
        }
      }
      if (newVals.Length == 0)
        newVals = null;
      o = newVals;
    }
    else
    {
      o = null;
    }
    player.prefs[player.currPref] = o;
    player.isChanged = true;
    if (!player.chgPrefs.Contains(player.currPref))
      player.chgPrefs.Add(player.currPref);
  }

  void OnDestroy()
  {
    if (pps != null)
      pps.Close();
    pps = null;
    if (sql != null)
      sql.Close(false);
    sql = null;
  }
}

public class PrefsPlayer
{
  public bool isOpen = false;
  public Rect rect = new Rect(320, 20, 650, 650);
  public string name;
  public int id;
  public Dictionary<string, object> prefs = null;
  public Dictionary<string, object> newPrefs = new Dictionary<string, object>();
  public string currPref = string.Empty;
  public int currPrefIx = 0;
  public bool isChanged = false;
  public List<string> chgPrefs = new List<string>();
  public Vector2 scroll = Vector2.zero;

  public void Get(Dictionary<string, playerprefsplus_pref> Prefs)
  {
    prefs = new Dictionary<string, object>();
    Dictionary<string, object> dbPrefs;
    PlayerPrefsSQL sql = new PlayerPrefsSQL();
    sql.PlayerGet(id, out dbPrefs);
    foreach (KeyValuePair<string, object> kvp in dbPrefs)
    {
      playerprefsplus_pref Pref = Prefs[kvp.Key];
      if (useString(Pref.Type))
      {
        if (kvp.Value != null)
        {
          if (kvp.Value.GetType().IsArray)
          {
            object[] o = (object[])kvp.Value;
            string[] sa = new string[o.Length];
            for (int i = 0; i < o.Length; i++)
            {
              sa[i] = SQL4Unity.DataType.ToString(o[i], true);
            }
            prefs.Add(kvp.Key, sa);
          }
          else
          {
            prefs.Add(kvp.Key, SQL4Unity.DataType.ToString(kvp.Value, true));
          }
        }
        else
        {
          prefs.Add(kvp.Key, kvp.Value);
        }
      }
      else
      {
        prefs.Add(kvp.Key, kvp.Value);
      }
    }
    sql.Close();
  }

  public static bool useString(string dataType)
  {
    switch (dataType.ToLower())
    {
      // Switch certain data types to strings because of Unity GUI bug
      case "bool":
      case "byte":
      case "datetime":
      case "vector2":
      case "vector3":
      case "vector4":
      case "quaternion":
      case "rect":
        return true;
    }
    return false;
  }

  public void Set()
  {
    if (isChanged)
    {
      PlayerPrefsSQL sql = new PlayerPrefsSQL();
      sql.AutoSave(false);
      for (int i = 0; i < chgPrefs.Count; i++)
      {
        object values = prefs[chgPrefs[i]];
        if (values == null)
        {
          sql.PlayerDeleteKeyValues(id, chgPrefs[i]);
        }
        else
        {
          sql.PlayerSet(id, chgPrefs[i], values);
        }
      }
      sql.Save();
      sql.Close();
      newPrefs.Clear();
      chgPrefs.Clear();
      isChanged = false;
      prefs = null;
    }
  }
}
