/*
 * This File and its contents are Copyright SteveSmith.Software 2018.
 * 
 * https://stevesmith.software
 * 
 * This software is licensed under a Modified MIT License
 * 
 */
using System;
using UnityEngine;

public class PlayerPrefsSetup {

    PlayerPrefsSQL sql = null;
    string query = string.Empty;
    SQL4Unity.SQLResult result;

    ~PlayerPrefsSetup()
    {
        Close();
    }

    public PlayerPrefsSetup()
    {
        sql = new PlayerPrefsSQL();
    }

    /// <summary>
    /// Constructor if Targeting WebGL
    /// </summary>
    /// <param name="mono">A valid MonoBehaviour</param>
    /// <param name="callback"> A method void Callback(bool ok) to call when database open is complete</param>
    public PlayerPrefsSetup(MonoBehaviour mono, Action<bool> callback)
    {
        sql = new PlayerPrefsSQL(mono);
        sql.OpenAsync(callback);
    }

    public bool AddPref(string key, string dataType)
    {
        if (sql == null) sql = new PlayerPrefsSQL();
        try
        {
            sql.getColName(dataType);
        }
        catch
        {
            Debug.Log("Invalid Data Type " + dataType);
            return false;
        }
        query = "Select * from Pref where Title=" + sql.tostring(key);
        result = sql.SQL(query);
        if (result.rowsAffected == 0)
        {
            query = string.Format("Insert into Pref (Title,Type) values ({0},{1})", sql.tostring(key), sql.tostring(dataType));
            result = sql.SQL(query);
            if (result.rowsAffected == 1) return true;
        }
        Debug.Log($"Preference already exists {key}");
        return false;
    }

    public bool RenamePref(string oldKey, string newKey)
    {
        if (sql == null) sql = new PlayerPrefsSQL();
        query = string.Format("Update Pref set Title={0} where Title={1}", sql.tostring(newKey), sql.tostring(oldKey));
        result = sql.SQL(query);
        if (result.rowsAffected == 1) return true;
        Debug.Log(result.message);
        return false;
    }

    public bool DeletePref(string key)
    {
        if (sql == null) sql = new PlayerPrefsSQL();
        query = "Select * from Pref where Title=" + sql.tostring(key);
        result = sql.SQL(query);
        if (result.rowsAffected == 1)
        {
            int prefID = (int)result.Get(0, "ID");
            return DeletePref(prefID);
        }
        Debug.Log(result.message);
        return false;
    }

    public bool DeletePref(int prefID)
    {
        if (sql == null) sql = new PlayerPrefsSQL();
        sql.AutoSave(false);
        query = "Select * from PlayerPref where PrefID=" + sql.tostring(prefID);
        result = sql.SQL(query);
        if (result.rowsAffected > 0)
        {
            playerprefsplus_playerpref[] prefs = result.Get<playerprefsplus_playerpref>();
            for (int i=0;i<prefs.Length;i++)
            {
                query = "Delete from PlayerPrefValue where PlayerPrefID=" + sql.tostring(prefs[i].ID);
                result = sql.SQL(query);
            }
            query = "Delete from PlayerPref where PrefID=" + sql.tostring(prefID);
            result = sql.SQL(query);
        }
        sql.AutoSave(true);
        query = "Delete from Pref where ID=" + sql.tostring(prefID);
        result = sql.SQL(query);
        if (result.rowsAffected == 1)
        {
            return true;
        }
        Debug.Log(result.message);
        return false;
    }

    public bool AddPlayer(string name)
    {
        if (sql == null) sql = new PlayerPrefsSQL();
        // Check for Duplicates
        // Remove the following code if you want to allow duplicate player names
        query = "Select * from Player where Title=" + sql.tostring(name);
        result = sql.SQL(query);
        if (result.rowsAffected == 1)
        {
            Debug.Log("Duplicate player " + name);
            return false;
        }
        // Add new Player
        query = string.Format("Insert into Player (Title) values ({0})",sql.tostring(name));
        result = sql.SQL(query);
        if (result.rowsAffected == 1) return true;
        Debug.Log(result.message);
        return false;
    }

    public bool RenamePlayer(string oldName, string newName)
    {
        if (sql == null) sql = new PlayerPrefsSQL();
        query = string.Format("Update Player set Title={0} where Title={1}", sql.tostring(newName), sql.tostring(oldName));
        result = sql.SQL(query);
        if (result.rowsAffected == 1) return true;
        Debug.Log(result.message);
        return false;
    }

    public bool DeletePlayer(string name)
    {
        if (sql == null) sql = new PlayerPrefsSQL();
        query = "Select * from Player where Title=" + sql.tostring(name);
        result = sql.SQL(query);
        if (result.rowsAffected == 1)
        {
            int playerid = (int)result.Get(0, "ID");
            return DeletePlayer(playerid);
        }
        Debug.Log(result.message);
        return false;
    }

    public bool DeletePlayer(int playerID)
    {
        if (sql == null) sql = new PlayerPrefsSQL();
        sql.PlayerDeleteAll(playerID);
        query = "Delete from Player where ID=" + sql.tostring(playerID);
        result = sql.SQL(query);
        if (result.rowsAffected == 1) return true;
        Debug.Log(result.message);
        return false;
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
