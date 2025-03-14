/*
 * This File and its contents are Copyright SteveSmith.Software 2018.
 *
 * https://stevesmith.software
 *
 * This software is licensed under a Modified MIT License
 *
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPrefsPlus
{
  public string Name;
  public int ID = 0;
  PlayerPrefsSQL sql = null;

  public PlayerPrefsPlus()
  {
    sql = new PlayerPrefsSQL();
  }

  ~PlayerPrefsPlus()
  {
    Close();
  }

  public PlayerPrefsPlus(string PlayerName)
  {
    sql = new PlayerPrefsSQL();
    GetPlayerByName(PlayerName);
  }

  public PlayerPrefsPlus(int PlayerID)
  {
    sql = new PlayerPrefsSQL();
    GetPlayerByID(PlayerID);
  }

  /// <summary>
  /// Constructor if targeting WebGL
  /// </summary>
  /// <param name="mono"> A valid MonoBehaviour</param>
  public PlayerPrefsPlus(MonoBehaviour mono)
  {
    sql = new PlayerPrefsSQL(mono);
  }

  /// <summary>
  /// Open the Player Prefs Database. Only if targeting WebGL
  /// </summary>
  /// <param name="callback"> A method void Callback(bool ok) to call when database open is complete</param>
  public void OpenAsync(Action<bool> callback)
  {
    if (sql != null)
      sql.OpenAsync(callback);
  }

  public void GetPlayerByName(string PlayerName)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    Name = PlayerName;
    ID = sql.GetPlayerByName(PlayerName);
  }

  public void GetPlayerByID(int PlayerID)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    Name = sql.GetPlayerByID(PlayerID);
    ID = PlayerID;
  }

  public int GetAllPlayers(out playerprefsplus_player[] Players)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.GetAllPlayers(out Players);
  }

  public int GetPlayerCount()
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.GetPlayerCount();
  }

  public bool SetDefaults()
  {
    if (ID == 0)
      return false;
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.SetPlayerDefaults(ID);
  }

  public bool HasKey(string key)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerHasKey(ID, key);
  }

  public bool DeleteAll()
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerDeleteAll(ID);
  }

  public bool DeleteAllValues()
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerDeleteValues(ID);
  }

  public bool DeleteKey(string key)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerDeleteKey(ID, key);
  }

  public Dictionary<string, object> Get()
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    Dictionary<string, object> values;
    if (sql.PlayerGet(ID, out values))
      return values;
    return null;
  }

  public object Get(string key)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    object value;
    if (sql.PlayerGet(ID, key, out value))
      return value;
    return null;
  }

  public object Get(string key, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    object value;
    if (sql.PlayerGet(ID, key, seq, out value))
      return value;
    return null;
  }

  public bool Set(string key, string value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, int value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, bool value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, Vector2 value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, Vector3 value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, Vector4 value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, float value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, byte value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, long value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, GameObject value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, DateTime value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, Color value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, Quaternion value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, Rect value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, Sprite value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, UnityEngine.Object value)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value);
  }

  public bool Set(string key, string[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, int[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, bool[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, Vector2[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, Vector3[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, Vector4[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, float[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, byte[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, long[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, GameObject[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, DateTime[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, Color[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, Quaternion[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, Rect[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, Sprite[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, UnityEngine.Object[] values)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, values);
  }

  public bool Set(string key, string value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, int value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, bool value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, Vector2 value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, Vector3 value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, Vector4 value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, float value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, byte value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, long value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, GameObject value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, DateTime value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, Color value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, Quaternion value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, Rect value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, Sprite value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public bool Set(string key, UnityEngine.Object value, int seq)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    return sql.PlayerSet(ID, key, value, seq);
  }

  public void AutoSave(bool save)
  {
    if (sql == null)
      sql = new PlayerPrefsSQL();
    sql.AutoSave(save);
  }

  public void Save()
  {
    if (sql == null)
    {
      Debug.Log("Database is not open");
      return;
    }
    sql.Save();
  }

  public void Open()
  {
    Close();
    sql = new PlayerPrefsSQL();
  }

  public void Close()
  {
    if (sql != null)
    {
      sql.Close();
      sql = null;
    }
  }
}
