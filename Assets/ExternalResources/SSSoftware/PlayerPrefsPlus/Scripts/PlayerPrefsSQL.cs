/*
 * This File and its contents are Copyright SteveSmith.Software 2018.
 * 
 * https://stevesmith.software
 * 
 * This software is licensed under a Modified MIT License
 * 
 */
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerPrefsSQL {

    SQL4Unity.SQLExecute sql = null;
    SQL4Unity.SQLResult result = new SQL4Unity.SQLResult();

    bool autoSave = true;

    public PlayerPrefsSQL()
    {
        // Initialise the SQLExecute to use the PlayerPrefsPlus Database
        if (sql == null) sql = new SQL4Unity.SQLExecute("PlayerPrefsPlus");
    }

    /// <summary>
    /// Constructor if targeting WebGL
    /// </summary>
    /// <param name="mono">A valid MonoBehaviour</param>
    public PlayerPrefsSQL(MonoBehaviour mono)
    {
        // Initialise the SQLExecute
        if (sql == null) sql = new SQL4Unity.SQLExecute(mono);
    }

    ~PlayerPrefsSQL()
    {
        // Destructor to make sure the database is closed;
        Close();
    }
    /// <summary>
    /// Open the Player Prefs Database. Only if targeting WebGL
    /// </summary>
    /// <param name="callback"> A method void Callback(bool ok) to call when database open is complete</param>
    public void OpenAsync(Action<bool> callback)
    {
        if (sql != null) sql.OpenAsync("PlayerPrefsPlus", callback);
    }

    public int GetPlayerByName(string PlayerName)
    {
        // Retrieve the Player Data By Name or Insert a new record if the player is not found
        string query = "Select * from Player where Title=" + tostring(PlayerName);
        sql.Command(query, result);
        if (result.rowsAffected > 0)
        {
            playerprefsplus_player player = result.Get<playerprefsplus_player>(0);
            return player.ID;
        }
        else
        {
            query = string.Format("Insert into Player (Title) values ({0})", tostring(PlayerName));
            sql.Command(query, result);
            return result.lastID;
        }
    }

    public string GetPlayerByID(int PlayerID)
    {
        // Retrieve the player data by ID
        string query = "Select * from Player where ID=" + tostring(PlayerID);
        sql.Command(query, result);
        if (result.rowsAffected > 0)
        {
            playerprefsplus_player player = result.Get<playerprefsplus_player>(0);
            return player.Title;
        }
        return "UNKNOWN";
    }

    public int GetAllPlayers(out playerprefsplus_player[] Players)
    {
        // Retrieve all the player data
        string query = "Select * from Player";
        sql.Command(query, result);
        if (result.rowsAffected > 0)
        {
            Players = result.Get<playerprefsplus_player>();
            return result.rowsAffected;
        }
        Players = new playerprefsplus_player[0];
        return 0;
    }

    public int GetPlayerCount()
    {
        // Retrieve the number of players in the DB
        string query = "Select count(*) from Player";
        sql.Command(query, result);
        double count = (double)result.Get(0, 0);
        return (int)count;
    }

    public bool SetPlayerDefaults(int PlayerID)
    {
        // Copy Default Preference Values to the current player
        return CopyPlayerValues(PlayerID, 1);
    }

    public bool CopyPlayerValues(int PlayerID, int CopyID)
    {
        // Copy Preference Values to the current player from another player
        bool ret = true;
        sql.AutoCommit(false);
        SQL4Unity.SQLResult insResult = new SQL4Unity.SQLResult();
        try
        {
            string query = "Select * from Pref";
            sql.Command(query, result);
            if (result.rowsAffected == 0) return false;
            playerprefsplus_pref[] prefs = result.Get<playerprefsplus_pref>();
            for (int i = 0; i < prefs.Length; i++)
            {
                playerprefsplus_pref pref = prefs[i];
                string colName = getColName(pref.Type);
                query = string.Format("Select * from PlayerPref where PlayerID={0} and PrefID={1}", tostring(CopyID), tostring(pref.ID));
                sql.Command(query, result);
                if (result.rowsAffected == 0) continue;
                playerprefsplus_playerpref defaultpref = result.Get<playerprefsplus_playerpref>(0);
                query = string.Format("Insert into PlayerPref (PlayerID, PrefID) values ({0},{1})", tostring(PlayerID), tostring(defaultpref.PrefID));
                sql.Command(query, result);
                int playerprefid = result.lastID;
                query = string.Format("Select * from PlayerPrefValue where PlayerPrefID={0} Order By Seq", defaultpref.ID);
                sql.Command(query, result);
                for (int k = 0; k < result.rowsAffected; k++)
                {
                    int seq = (int)result.Get(k, "Seq");
                    object value = result.Get(k, colName);
                    query = string.Format("Insert into PlayerPrefValue (PlayerPrefID, Seq, {0}) values ({1},{2},{3})", colName, tostring(playerprefid), tostring(seq), tostring(value));
                    sql.Command(query, insResult);
                }
            }
            if (autoSave) sql.Commit();
        }
        catch (Exception ex)
        {
            Debug.Log("Invalid Data Type found in Preference?");
            Debug.Log(ex.Message);
            sql.Rollback();
            ret = false;
        }
        finally {
            sql.AutoCommit(autoSave);
        }
        return ret;
    }

    public bool PlayerHasKey(int PlayerID, string key)
    {
        // Does the current player have a particular preference ?
        string query = string.Format("Select * from Pref, PlayerPref where Pref.ID=PrefID and PlayerID={0} and Title={1}", tostring(PlayerID), tostring(key));
        sql.Command(query, result);
        if (result.rowsAffected > 0) return true;
        return false;
    }

    public bool PlayerDeleteAll(int PlayerID)
    {
        // Remove all preferences for the player
        bool ret = true;
        sql.AutoCommit(false);
        try
        {
            string query = "Select * from PlayerPref where PlayerID=" + tostring(PlayerID);
            sql.Command(query, result);
            playerprefsplus_playerpref[] playerprefs = result.Get<playerprefsplus_playerpref>();
            for (int i=0;i<playerprefs.Length;i++)
            {
                playerprefsplus_playerpref playerpref = playerprefs[i];
                query = "Delete from PlayerPrefValue where PlayerPrefID=" + tostring(playerpref.ID);
                sql.Command(query, result);
            }
            query = "Delete from PlayerPref where PlayerID=" + tostring(PlayerID);
            sql.Command(query, result);
            if (autoSave) sql.Commit();
        }
        catch
        {
            sql.Rollback();
            ret = false;
        }
        finally
        {
            sql.AutoCommit(autoSave);
        }
        return ret;
    }

    public bool PlayerDeleteValues(int PlayerID)
    {
        // Remove all preferences for the player
        bool ret = true;
        sql.AutoCommit(false);
        try
        {
            string query = "Select * from PlayerPref where PlayerID=" + tostring(PlayerID);
            sql.Command(query, result);
            playerprefsplus_playerpref[] playerprefs = result.Get<playerprefsplus_playerpref>();
            for (int i = 0; i < playerprefs.Length; i++)
            {
                playerprefsplus_playerpref playerpref = playerprefs[i];
                query = "Delete from PlayerPrefValue where PlayerPrefID=" + tostring(playerpref.ID);
                sql.Command(query, result);
            }
            if (autoSave) sql.Commit();
        }
        catch
        {
            sql.Rollback();
            ret = false;
        }
        finally
        {
            sql.AutoCommit(autoSave);
        }
        return ret;
    }

    public bool PlayerDeleteKey(int PlayerID, string key)
    {
        // Remove a specific preference from the player
        bool ret = true;
        sql.AutoCommit(false);
        try
        {
            string query = string.Format("Select * from Pref, PlayerPref where Pref.ID=PrefID and PlayerID={0} and Title={1}", tostring(PlayerID), tostring(key));
            sql.Command(query, result);
            playerprefsplus_playerpref playerpref = result.Get<playerprefsplus_playerpref>(0);
            query = "Delete from PlayerPrefValue where PlayerPrefID=" + tostring(playerpref.ID);
            sql.Command(query, result);
            query = "Delete from PlayerPref where ID=" + tostring(playerpref.ID);
            sql.Command(query, result);
            if (autoSave) sql.Commit();
        }
        catch
        {
            sql.Rollback();
            ret = false;
        }
        finally
        {
            sql.AutoCommit(autoSave);
        }
        return ret;
    }

    public bool PlayerDeleteKeyValues(int PlayerID, string key)
    {
        // Remove all values for a preference for the player
        bool ret = true;
        string query = string.Format("Select * from PlayerPref, Pref where Pref.ID=PrefID and PlayerID={0} and Title={1}", tostring(PlayerID), tostring(key));
        sql.Command(query, result);
        if (result.rowsAffected > 0)
        {
            playerprefsplus_playerpref playerpref = result.Get<playerprefsplus_playerpref>(0);
            query = "Delete from PlayerPrefValue where PlayerPrefID=" + tostring(playerpref.ID);
            sql.Command(query, result);
        }
        return ret;
    }

    public bool PlayerGet(int PlayerID, out Dictionary<string, object> values)
    {
        // Retrieve ALL preferences for a player
        values = new Dictionary<string, object>();
        string query = "Select * from Pref";
        sql.Command(query, result);
        if (result.rowsAffected == 0) return false;
        playerprefsplus_pref[] prefs = result.Get<playerprefsplus_pref>();
        for (int i = 0; i < prefs.Length; i++)
        {
            playerprefsplus_pref pref = prefs[i];
            string colName = getColName(pref.Type);
            query = string.Format("Select * from PlayerPref where PlayerID={0} and PrefID={1}", tostring(PlayerID), tostring(pref.ID));
            sql.Command(query, result);
            if (result.rowsAffected == 0) continue;
            playerprefsplus_playerpref playerpref = result.Get<playerprefsplus_playerpref>(0);
            query = string.Format("Select * from PlayerPrefValue where PlayerPrefID={0} Order By Seq", playerpref.ID);
            sql.Command(query, result);
            if (result.rowsAffected == 0)
            {
                values.Add(pref.Title, null);
            }
            else
            {
                if (result.rowsAffected == 1)
                {
                    object value = result.Get(0, colName);
                    values.Add(pref.Title, value);
                }
                else
                {
                    object[] prefVals = new object[result.rowsAffected];
                    for (int k = 0; k < result.rowsAffected; k++)
                    {
                        object value = result.Get(k, colName);
                        prefVals[k] = value;
                    }
                    values.Add(pref.Title, prefVals);
                }
            }
        }
        return true;
    }

    public bool PlayerGet(int PlayerID, string key, out object value)
    {
        // Retrieve a specific preference for the player
        value = null;
        string query = "Select * from Pref where Title=" + tostring(key);
        sql.Command(query, result);
        if (result.rowsAffected == 0) return false;
        playerprefsplus_pref pref = result.Get<playerprefsplus_pref>(0);
        string colName = getColName(pref.Type);
        query = string.Format("Select * from PlayerPref where PlayerID={0} and PrefID={1}", tostring(PlayerID), tostring(pref.ID));
        sql.Command(query, result);
        if (result.rowsAffected == 0) return false;
        playerprefsplus_playerpref playerpref = result.Get<playerprefsplus_playerpref>(0);
        query = string.Format("Select * from PlayerPrefValue where PlayerPrefID={0} Order By Seq", playerpref.ID);
        sql.Command(query, result);
        if (result.rowsAffected == 0) return false;
        if (result.rowsAffected == 1)
        {
            value = result.Get(0, colName);
        }
        else
        {
            object[] prefVals = new object[result.rowsAffected];
            for (int k = 0; k < result.rowsAffected; k++)
            {
                object prefVal = result.Get(k, colName);
                prefVals[k] = prefVal;
            }
            value = prefVals;
        }
        return true;
    }

    public bool PlayerGet(int PlayerID, string key, int seq, out object value)
    {
        // Retrieve a specific preference value where the preference has more than one value for the player
        value = null;
        string query = "Select * from Pref where Title=" + tostring(key);
        sql.Command(query, result);
        if (result.rowsAffected == 0) return false;
        playerprefsplus_pref pref = result.Get<playerprefsplus_pref>(0);
        string colName = getColName(pref.Type);
        query = string.Format("Select * from PlayerPref where PlayerID={0} and PrefID={1}", tostring(PlayerID), tostring(pref.ID));
        sql.Command(query, result);
        if (result.rowsAffected == 0) return false;
        playerprefsplus_playerpref playerpref = result.Get<playerprefsplus_playerpref>(0);
        query = string.Format("Select * from PlayerPrefValue where PlayerPrefID={0} and Seq={1}", tostring(playerpref.ID), tostring(seq));
        sql.Command(query, result);
        if (result.rowsAffected == 0) return false;
        if (result.rowsAffected == 1)
        {
            value = result.Get(0, colName);
        }
        else
        {
            object[] prefVals = new object[result.rowsAffected];
            for (int k = 0; k < result.rowsAffected; k++)
            {
                object prefVal = result.Get(k, colName);
                prefVals[k] = prefVal;
            }
            value = prefVals;
        }
        return true;
    }

    public bool PlayerSet(int PlayerID, string key, object value)
    {
        // Set a preference value
        if (value != null)
            if (value.GetType().IsArray) return PlayerSetArray(PlayerID, key, value);
        return PlayerSet(PlayerID, key, value, 0);
    }

    public bool PlayerSet(int PlayerID, string key, object value, int seq)
    {
        // Set a specific preference value where the preference have more than one value
        bool ret = true;
        sql.AutoCommit(false);
        try
        {
            string query = "Select * from Pref where Title=" + tostring(key);
            sql.Command(query, result);
            if (result.rowsAffected == 0) return false;
            playerprefsplus_pref pref = result.Get<playerprefsplus_pref>(0);
            isValid(pref.Type, value);
            string colName = getColName(pref.Type);
            query = string.Format("Select * from PlayerPref where PlayerID={0} and PrefID={1}", tostring(PlayerID), tostring(pref.ID));
            sql.Command(query, result);
            int playerprefid = 0;
            if (result.rowsAffected == 0)
            {
                query = string.Format("Insert into PlayerPref (PlayerID, PrefID) values ({0},{1})", tostring(PlayerID), tostring(pref.ID));
                sql.Command(query, result);
                playerprefid = result.lastID;
            }
            else
            {
                playerprefid = (int)result.Get(0, "ID");
                query = string.Format("Delete from PlayerPrefValue where PlayerPrefID={0} and Seq={1}", tostring(playerprefid), tostring(seq));
                sql.Command(query, result);
            }
            if (value != null)
            {
                query = string.Format("Insert into PlayerPrefValue (PlayerPrefID, Seq, {0}) values ({1},{2},{3})", colName, tostring(playerprefid), tostring(seq), tostring(value));
                sql.Command(query, result);
            }
            if (autoSave) sql.Commit();
        }
        catch (Exception ex)
        {
            Debug.Log("Invalid Data Type found for Preference?");
            Debug.Log(ex.Message);
            sql.Rollback();
            ret = false;
        }
        finally
        {
            sql.AutoCommit(autoSave);
        }
        return ret;
    }

    public bool PlayerSetArray(int PlayerID, string key, object value)
    {
        // Set multiple values for a preference
        bool ret = true;
        sql.AutoCommit(false);
        try
        {
            string query = "Select * from Pref where Title=" + tostring(key);
            sql.Command(query, result);
            if (result.rowsAffected == 0) return false;
            playerprefsplus_pref pref = result.Get<playerprefsplus_pref>(0);
            object[] values;
            isValidArray(pref.Type, value, out values);
            if (values == null) return false;
            string colName = getColName(pref.Type);
            query = string.Format("Select * from PlayerPref where PlayerID={0} and PrefID={1}", tostring(PlayerID), tostring(pref.ID));
            sql.Command(query, result);
            int playerprefid = 0;
            if (result.rowsAffected == 0)
            {
                query = string.Format("Insert into PlayerPref (PlayerID, PrefID) values ({0},{1})", tostring(PlayerID), tostring(pref.ID));
                sql.Command(query, result);
                playerprefid = result.lastID;
            }
            else
            {
                playerprefid = (int)result.Get(0, "ID");
                query = "Delete from PlayerPrefValue where PlayerPrefID=" + tostring(playerprefid);
                sql.Command(query, result);
            }
            for (int i = 0; i < values.Length; i++)
            {
                query = string.Format("Insert into PlayerPrefValue (PlayerPrefID, Seq, {0}) values ({1},{2},{3})", colName, tostring(playerprefid), tostring(i), tostring(values[i]));
                sql.Command(query, result);
            }
            if (autoSave) sql.Commit();
        }
        catch (Exception ex)
        {
            Debug.Log("Invalid Data Type found for Preference?");
            Debug.Log(ex.Message);
            sql.Rollback();
            ret = false;
        }
        finally
        {
            sql.AutoCommit(autoSave);
        }
        return ret;
    }

    public string getColName(string type)
    {
        // Evaluate which table column to use to save/retrieve the preference value
        string colName = string.Empty;
        switch (type)
        {
            case "string": colName = "StringVal"; break;
            case "int": colName = "IntVal"; break;
            case "bool": colName = "BoolVal"; break;
            case "Vector2": colName = "V2Val"; break;
            case "Vector3": colName = "V3Val"; break;
            case "Vector4": colName = "V4Val"; break;
            case "GameObject": colName = "GoVal"; break;
            case "float": colName = "FloatVal"; break;
            case "long": colName = "LongVal"; break;
            case "byte": colName = "ByteVal"; break;
            case "DateTime": colName = "DateVal"; break;
            case "Color": colName = "ColVal"; break;
            case "Quaternion": colName = "QuatVal"; break;
            case "Rect": colName = "RectVal"; break;
            case "Sprite": colName = "SpriteVal"; break;
            case "Resource": colName = "ResVal"; break;
            default:
                throw new ArgumentException();
        }
        return colName;
    }

    bool isValid(string type, object value)
    {
        // Evaluate if the passed value object is the correct datatype for the preference
        if (value == null) return true;
        Type t = value.GetType();
        switch(type)
        {
            case "string": if (t.Equals(typeof(string))) return true; break;
            case "int": if (t.Equals(typeof(int))) return true; break;
            case "bool": if (t.Equals(typeof(bool))) return true; break;
            case "Vector2": if (t.Equals(typeof(Vector2))) return true; break;
            case "Vector3": if (t.Equals(typeof(Vector3))) return true; break;
            case "Vector4": if (t.Equals(typeof(Vector4))) return true; break;
            case "float": if (t.Equals(typeof(float))) return true; break;
            case "long": if (t.Equals(typeof(long))) return true; break;
            case "byte": if (t.Equals(typeof(byte))) return true; break;
            case "DateTime": if (t.Equals(typeof(DateTime))) return true; break;
            case "Color": if (t.Equals(typeof(Color))) return true; break;
            case "Quaternion": if (t.Equals(typeof(Quaternion))) return true; break;
            case "Rect": if (t.Equals(typeof(Rect))) return true; break;
            case "Sprite":
                if (t.Equals(typeof(Sprite))) return true;
                if (t.Equals(typeof(SQL4Unity.Object))) return true;
                break;
            case "Resource": if (t.Equals(typeof(UnityEngine.Object))) return true;
                if (t.Equals(typeof(SQL4Unity.Object))) return true;
                break;
            case "GameObject": if (t.Equals(typeof(GameObject))) return true;
                if (t.Equals(typeof(SQL4Unity.Object))) return true;
                break;
        }
        throw new ArgumentException();
    }

    bool isValidArray(string type, object values, out object[] outVals)
    {
        // Evaluate if the passed values object is an array and is of the correct datatype for the preference
        outVals = null;
        Type t = values.GetType();
        if (!t.IsArray) return false;
        if (t.Equals(typeof(object[])))
        {
            outVals = (object[])values;
            return true;
        }
        switch (type)
        {
            case "string": if (t.Equals(typeof(string[])))
                {
                    outVals = (string[])values;
                    return true;
                }
                break;
            case "int": if (t.Equals(typeof(int[])))
                {
                    int[] tempVals = (int[])values;
                    outVals = new object[tempVals.Length];
                    for (int i=0;i<tempVals.Length;i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "bool": if (t.Equals(typeof(bool[])))
                {
                    bool[] tempVals = (bool[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "Vector2": if (t.Equals(typeof(Vector2[])))
                {
                    Vector2[] tempVals = (Vector2[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "Vector3": if (t.Equals(typeof(Vector3[])))
                {
                    Vector3[] tempVals = (Vector3[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "Vector4": if (t.Equals(typeof(Vector4[])))
                {
                    Vector4[] tempVals = (Vector4[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "float": if (t.Equals(typeof(float[])))
                {
                    float[] tempVals = (float[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "long": if (t.Equals(typeof(long[])))
                {
                    long[] tempVals = (long[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "byte": if (t.Equals(typeof(byte[])))
                {
                    byte[] tempVals = (byte[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "DateTime": if (t.Equals(typeof(DateTime[])))
                {
                    DateTime[] tempVals = (DateTime[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "Color": if (t.Equals(typeof(Color[])))
                {
                    Color[] tempVals = (Color[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "Quaternion": if (t.Equals(typeof(Quaternion[])))
                {
                    Quaternion[] tempVals = (Quaternion[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "Rect": if (t.Equals(typeof(Rect[])))
                {
                    Rect[] tempVals = (Rect[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "Sprite":
                if (t.Equals(typeof(Sprite[])))
                {
                    Sprite[] tempVals = (Sprite[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                if (t.Equals(typeof(SQL4Unity.Object[])))
                {
                    SQL4Unity.Object[] tempVals = (SQL4Unity.Object[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "Resource":
                if (t.Equals(typeof(UnityEngine.Object[])))
                {
                    UnityEngine.Object[] tempVals = (UnityEngine.Object[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                if (t.Equals(typeof(SQL4Unity.Object[])))
                {
                    SQL4Unity.Object[] tempVals = (SQL4Unity.Object[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
            case "GameObject":
                if (t.Equals(typeof(GameObject[])))
                {
                    outVals = (GameObject[])values;
                    return true;
                }
                if (t.Equals(typeof(SQL4Unity.Object[])))
                {
                    SQL4Unity.Object[] tempVals = (SQL4Unity.Object[])values;
                    outVals = new object[tempVals.Length];
                    for (int i = 0; i < tempVals.Length; i++)
                    {
                        outVals[i] = tempVals[i];
                    }
                    return true;
                }
                break;
        }
        throw new ArgumentException();
    }

    public SQL4Unity.SQLResult SQL(string query)
    {
        if (!sql.Command(query, result))
        {
            Debug.Log(result.message);
        }
        return result;
    }

    public void AutoSave(bool save)
    {
        // Set whether changes should be automatically saved
        autoSave = save;
        sql.AutoCommit(save);
    }

    public void Save()
    {
        // Save changes to the database
        sql.Commit();
    }

    public void Close()
    {
        if (sql != null)
        {
            sql.Close(false);
            sql = null;
        } 
    }

    public string tostring(object value)
    {
        // Returns the string value of an object formatted for use in an SQL statement
        if (value.GetType().Equals(typeof(DateTime))) return SQL4Unity.SQLDate.ToString((DateTime)value);
        return SQL4Unity.DataType.ToString(value);
    }
}
