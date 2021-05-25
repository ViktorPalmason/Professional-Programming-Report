using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;

public class CreateMod : MonoBehaviour
{
    [SerializeField] private InputField _modNameIF;
    [SerializeField] private InputField _factionNameIF;
    [SerializeField] private InputField _unitNameIF;
    [SerializeField] private Text _resultText;

    private string _streamingAssetsPath;

    /// <summary>
    ///  The structure that stores the names of existing mods
    /// </summary>
    private Dictionary<string, string> modNamesList;
    /// <summary>
    /// The structure that stores names of existing factions and the names of the their units
    /// </summary>
    private Dictionary<string, FactionElement> factionUnitNamesList;

    private string unitLuaFile;

    /// <summary>
    /// These are the names inputed by a student when generating the folder structure
    /// </summary>
    private string _modName;
    private string _factionName;
    private string _unitName;

    // Start is called before the first frame update
    void Start()
    {
        if (_resultText == null)
            Debug.LogError("(CreateMod)Start(): The Result text is NULL");

        _streamingAssetsPath = Application.streamingAssetsPath;

        if (!File.Exists(Application.dataPath + "/Resources/ModNames.json"))
        {
            StreamWriter strWriter = File.CreateText(Application.dataPath + "/Resources/ModNames.json");
            strWriter.Close();
        }

        if (!File.Exists(Application.dataPath + "/Resources/FactionUnitNames.json"))
        {
            StreamWriter strWriter = File.CreateText(Application.dataPath + "/Resources/FactionUnitNames.json");
            strWriter.Close();
        }

        //TextAsset modnames = Resources.Load<TextAsset>("ModNames");
        TextAsset modnames = new TextAsset(File.ReadAllText(Application.dataPath + "/Resources/ModNames.json"));
        //TextAsset factionUnitNames = Resources.Load<TextAsset>("FactionUnitNames");
        TextAsset factionUnitNames = new TextAsset(File.ReadAllText(Application.dataPath + "/Resources/FactionUnitNames.json"));

        if (modnames != null)
        {
            modNamesList = JsonConvert.DeserializeObject<Dictionary<string, string>>(modnames.text);
        }
        else
        {
            Debug.LogError("(CreateMod)Start(): The JSON file with the Mod names was not found");
            _resultText.text = "(CreateMod)Start(): The JSON file with the Mod names was not found";
        }

        if (factionUnitNames != null)
            factionUnitNamesList = JsonConvert.DeserializeObject<Dictionary<string, FactionElement>>(factionUnitNames.text);
        else
        {
            Debug.LogError("(CreateMod)Start(): The JSON file with the Faction and Unit names was not found");
            _resultText.text = "(CreateMod)Start(): The JSON file with the Mod names was not found";
        }

        unitLuaFile = methodStubCreator("OnMoveFinished") + System.Environment.NewLine +
            methodStubCreator("OnTileExit");
    }

    public void SetModName()
    {
        if (_modNameIF != null)
            _modName = _modNameIF.text;
        else
            Debug.LogError("(CreateMod)SetModName: The mod name input field is NULL");
    }
    public void SetFactionName()
    {
        if (_factionNameIF != null)
            _factionName = _factionNameIF.text;
        else
            Debug.LogError("(CreateMod)SetModName: The mod name input field is NULL");
    }
    public void SetUnitName()
    {
        if (_unitNameIF != null)
            _unitName = _unitNameIF.text;
        else
            Debug.LogError("(CreateMod)SetModName: The mod name input field is NULL");
    }

    public string methodStubCreator(string luaFuncName)
    {
        return "function " + luaFuncName + "()" + System.Environment.NewLine + "end";
    }

    private string stringArrayToString(string[] strArray)
    {
        string returnStr = "";
        foreach (string str in strArray)
        {
            returnStr += str;
            returnStr += "\n";
        }
        return returnStr;
    }

    public void GenerateFolderStruct()
    {
        /// To make sure that all input fields are not empty
        if (_modName.Length != 0 && _factionName.Length != 0 && _unitName.Length != 0 && _streamingAssetsPath != null)
        {
            if(modNamesList == null)
            {
                _resultText.text += "mod names list is null\n";
                modNamesList = new Dictionary<string, string>();
            }

            if(factionUnitNamesList == null)
            {
                _resultText.text += "faction unit names list is null\n";
                factionUnitNamesList = new Dictionary<string, FactionElement>();
            }
            /// Check if the mod name is taken. 
            if (modNamesList.ContainsKey(_modName))
            {
                _resultText.text += _modName + " is taken"; return;
            }
            /// Check if the faction name is taken
            else if (factionUnitNamesList.ContainsKey(_factionName))
            {
                _resultText.text += _factionName + " is taken"; return;
            }

            /// Create the new Mod directory
            DirectoryInfo modDir = Directory.CreateDirectory(Path.Combine(_streamingAssetsPath, _modName));
            /// Create the Lua file that stores the information about the Mod
            File.CreateText(modDir.FullName + "/ModDefiniton.lua");

            /// Create the utility folder to store the Lua files with utility functions.
            var utilityDir = modDir.CreateSubdirectory("Utility");
            /// Create the Utility Lua file. 
            File.CreateText(utilityDir.FullName + "/utility.lua");

            /// Create the faction folder
            DirectoryInfo factionDir = modDir.CreateSubdirectory(_factionName);
            /// Create the faction Lua file
            File.CreateText(factionDir + "/" + _factionName + ".lua");

            /// Create the Unit folder
            var unitDir = factionDir.CreateSubdirectory(_unitName);
            /// Create the unit Lua file
            File.WriteAllText(unitDir.FullName + "/" + _unitName + ".lua", unitLuaFile);


            try                                                                                 //Read from unit_tempaltes folder.
                                                                                                //Fetch Lua files and create new ones inside the mod folder.
            {
                File.WriteAllText(unitDir.FullName + "/" + "Archer.lua", Resources.Load<TextAsset>("Archer").text);
                File.WriteAllText(unitDir.FullName + "/" + "Paladin.lua", Resources.Load<TextAsset>("Paladin").text);
                File.WriteAllText(unitDir.FullName + "/" + "Spearman.lua", Resources.Load<TextAsset>("Spearman").text);
            }
            catch (System.NullReferenceException ex)
            {
                Debug.Log("Archer.txt, Sperman.txt and Paladin.txt does not exist in Resources folder.");
                Debug.Log(ex.Message);
            }
            /// Create the JSON file for the Faction data.
            string JsonStr = JsonConvert.SerializeObject(new factionInstance(_factionName, _modName, factionDir.FullName, _unitName, unitDir.FullName), Formatting.Indented);
            File.WriteAllText(factionDir.FullName + "/FactionData.json", JsonStr);

            /// Create the JSON file for the Unit data.
            string jsonString = JsonConvert.SerializeObject(new UnitData(_unitName, _factionName, _modName), Formatting.Indented);
            File.WriteAllText(unitDir.FullName + "/UnitData.json", jsonString);

            /// Create the Equipment folder
            DirectoryInfo equipDir = factionDir.CreateSubdirectory("Equipment");
            /// Create the folders for the unit equipment 
            DirectoryInfo weapon = equipDir.CreateSubdirectory("Weapon");
            DirectoryInfo armor = equipDir.CreateSubdirectory("Armor");
            DirectoryInfo ammo = equipDir.CreateSubdirectory("Ammo");
            /// Create the Lua files for the unit equipment
            File.CreateText(weapon.FullName + "/weapon.lua");
            File.CreateText(armor.FullName + "/armor.lua");
            File.CreateText(ammo.FullName + "/ammo.lua");

            string _modJsonPath = Application.dataPath + "/Resources/ModNames.json";
            string _factionUnitJsonPath = Application.dataPath + "/Resources/FactionUnitNames.json";

            /// Add the new mod name and its path to the list.
            modNamesList.Add(_modName, modDir.FullName);
            /// Add the new faction and unit name to the list.
            factionUnitNamesList.Add(_factionName, new FactionElement(_factionName, factionDir.FullName, new List<string> { _unitName }));

            /// Write to the Mod Names JSON files the new mod name and its path
            string modJson = JsonConvert.SerializeObject(modNamesList, Formatting.Indented);
            File.WriteAllText(_modJsonPath, modJson);
            _resultText.text += "The Mod names were writen to file";

            /// Write to the Faction Unit Names JSON files the new mod name and its path
            string factionUnitJson = JsonConvert.SerializeObject(factionUnitNamesList, Formatting.Indented);
            File.WriteAllText(_factionUnitJsonPath, factionUnitJson);
            _resultText.text += "The faction and unit names were writen to file";

            //AssetDatabase.Refresh();

        }
        else
            _resultText.text = "You have to fill in all the input fields";
    }

    public void Log(string debugMessage)                                           //Will perhaps be useful to see debug messages when building the prototype.
    {
        string path = Application.streamingAssetsPath + "/debuglog.txt";
        System.IO.StreamWriter writer = new System.IO.StreamWriter(path, true);
        writer.WriteLine(debugMessage + "\n");
        writer.Close();
    }

    public void GoBack()
    {
        gameObject.SetActive(false);
    }
}
