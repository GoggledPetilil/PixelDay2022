using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

// ***************************************************
// ** PUT YOUR SAVE PATH NAME IN THE VARIABLE BELOW **
// ***************************************************

// This class acts sort of as an emulator for
// PlayerPrefs, but saves to a specified
// place in IndexedDB so saved games don't
// get wiped when game updates are uploaded

// PlayerPrefs is similar (but not identical)
// to a hash of ints, floats, and strings, so
// this uses a Dictionary in C# (like a hash)

// Note that in this implementation all of
// the values are stored as strings even if
// they're really int or float, but they
// get parsed when values are returned

// Also note that I call Save() any time
// a value gets changed, because I think
// the risk of not saving stuff leading to
// bugginess outweighs the computational
// cost of saving more frequently.
// That could potentially be changed fairly
// easily as long as you know it's happening
// and that you just need to comment out the
// Save() statements at the end of the Set
// methods.

#if UNITY_EDITOR
#elif UNITY_WEBGL
public static class PlayerPrefs {
// **********************************
// ** PUT YOUR SAVE PATH NAME HERE **
// **********************************
  static string savePathName = "830329";
  static string fileName;
  static string[] fileContents;
  static Dictionary<string, string> saveData = new Dictionary<string, string>();

  // This is the static constructor for the class
  // When invoked, it looks for a savegame file
  // and reads the keys and values
  static PlayerPrefs() {
    fileName = "/idbfs/" + savePathName + "/NGsave.dat";

    // Open the savegame file and read all of the lines
    // into fileContents
    // First make sure the directory and save file exist,
    // and make them if they don't already
    // (If the file is created, the filestream needs to be
    // closed afterward so it can be saved to later)
    if (!Directory.Exists("/idbfs/" + savePathName)) {
      Directory.CreateDirectory("/idbfs/" + savePathName);
    }
    if (!File.Exists(fileName)) {
      FileStream fs = File.Create(fileName);
      fs.Close();
    } else {
      // Read the file if it already existed
      fileContents = File.ReadAllLines(fileName);

      // If you want to use encryption/decryption, add your
      // code for decrypting here
      //   ******* decryption algorithm ********

      // Put all of the values into saveData
      for (int i=0; i<fileContents.Length; i += 2) {
        saveData.Add(fileContents[i], fileContents[i+1]);
      }
    }
  }

  // This saves the saveData to the player's IndexedDB
  public static void Save() {
    // Put the saveData dictionary into the fileContents
    // array of strings
    Array.Resize(ref fileContents, 2 * saveData.Count);
    int i=0;
    foreach (string key in saveData.Keys) {
      fileContents[i++] = key;
      fileContents[i++] = saveData[key];
    }

    // If you want to use encryption/decryption, add your
    // code for encrypting here
    //   ******* encryption algorithm ********

    // Write fileContents to the save file
    File.WriteAllLines(fileName, fileContents);
  }

  // The following methods emulate what PlayerPrefs does
  public static void DeleteAll() {
    saveData.Clear();
    Save();
  }

  public static void DeleteKey(string key) {
    saveData.Remove(key);
    Save();
  }

  public static float GetFloat(string key) {
    return float.Parse(saveData[key]);
  }
  public static float GetFloat(string key, float defaultValue) {
    if (saveData.ContainsKey(key)) {
      return float.Parse(saveData[key]);
    } else {
      return defaultValue;
    }
  }

  public static int GetInt(string key) {
    return int.Parse(saveData[key]);
  }
  public static int GetInt(string key, int defaultValue) {
    if (saveData.ContainsKey(key)) {
      return int.Parse(saveData[key]);
    } else {
      return defaultValue;
    }
  }

  public static string GetString(string key) {
    return saveData[key];
  }
  public static string GetString(string key, string defaultValue) {
    if (saveData.ContainsKey(key)) {
      return saveData[key];
    } else {
      return defaultValue;
    }
  }

  public static bool HasKey(string key) {
    return saveData.ContainsKey(key);
  }

  public static void SetFloat(string key, float setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue.ToString();
    } else {
      saveData.Add(key, setValue.ToString());
    }
    Save();
  }

  public static void SetInt(string key, int setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue.ToString();
    } else {
      saveData.Add(key, setValue.ToString());
    }
    Save();
  }

  public static void SetString(string key, string setValue) {
    if (saveData.ContainsKey(key)) {
      saveData[key] = setValue;
    } else {
      saveData.Add(key, setValue);
    }
    Save();
  }
}
#endif

public class StartScript : MonoBehaviour {
  public void startGame() {
    Debug.Log("Starting the game...");
  }
}
