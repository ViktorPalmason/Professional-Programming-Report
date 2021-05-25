using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using TbsFramework.Players;

public class SetupGameUI : MonoBehaviour
{
    [SerializeField] private Transform CellsParent;
    [SerializeField] private Transform UnitsParent;
    [SerializeField] private Transform PlayersParent;

    [SerializeField] private Image FactionSelectionImage;
    [SerializeField] public Image InfoImage;

    [SerializeField] private GameObject p0_List;
    [SerializeField] private GameObject p1_List;

    [SerializeField] private Text P0_Header;
    [SerializeField] private Text P1_Header;

    [SerializeField] private GameObject FactionElementPrefab;

    [SerializeField] private GameObject UpperUIDisplay;

    private Dictionary<string, FactionElement> FactionList;

    private string Player0FactionDataPath;
    private string Player1FactionDataPath;

    private string debugPath;

    // Start is called before the first frame update
    void Start()
    {

        debugPath = Application.streamingAssetsPath + "/debuglog.txt";
        System.IO.File.WriteAllText(debugPath, string.Empty);                //clear the output file each time the game starts. 

        gameObject.SetActive(true);
        CellsParent.gameObject.SetActive(false);
        UnitsParent.gameObject.SetActive(false);
        PlayersParent.gameObject.SetActive(false);
        UpperUIDisplay.gameObject.SetActive(false);

        ReadInFactions();
    }

    public void RunGame()
    {
        Debug.Log("SetupGameUI: Running the game");
        if (PlayersParent != null)
        {
            PlayersParent.GetChild(0).GetComponent<HumanPlayer>().AssignPlayerFaction(Player0FactionDataPath);
            PlayersParent.GetChild(1).GetComponent<HumanPlayer>().AssignPlayerFaction(Player1FactionDataPath);
        }
        else
            Debug.LogError("Players parent is NULL");

        FactionSelectionImage.gameObject.SetActive(false);
        CellsParent.gameObject.SetActive(true);
        UnitsParent.gameObject.SetActive(true);
        PlayersParent.gameObject.SetActive(true);
        UpperUIDisplay.gameObject.SetActive(true);
    }

    public void Player0FactionSelect(string name)
    {
        P0_Header.text = "Player0 Faction:" + name;
        FactionElement playerFaction;
        if (FactionList.TryGetValue(name, out playerFaction))
        {
            Player0FactionDataPath = playerFaction.FactionPath;
        }
    }

    public void Player1FactionSelect(string name)
    {
        P1_Header.text = "Player1 Faction:" + name;
        FactionElement playerFaction;
        if(FactionList.TryGetValue(name, out playerFaction))
        {
            Player1FactionDataPath = playerFaction.FactionPath;
        }
    }

    private void ReadInFactions()
    {
        try
        {
            string jsonTxt = System.IO.File.ReadAllText(Path.Combine(Application.dataPath, "Resources", "FactionUnitNames.json"));
            FactionList = JsonConvert.DeserializeObject<Dictionary<string, FactionElement>>(jsonTxt);
            Log("text: " + jsonTxt);

            JObject obj = JObject.Parse(jsonTxt);

            foreach (string name in FactionList.Keys)
            {
                GameObject p0NewFaction = GameObject.Instantiate(FactionElementPrefab, p0_List.transform);
                GameObject p1NewFaction = GameObject.Instantiate(FactionElementPrefab, p1_List.transform);

                p0NewFaction.name = p1NewFaction.name = name;
                Log("faction: " + name + "\n");

                p0NewFaction.GetComponentInChildren<Text>().text = p1NewFaction.GetComponentInChildren<Text>().text = name;

                p0NewFaction.GetComponentInChildren<Button>().onClick.AddListener(() => Player0FactionSelect(name));
                p1NewFaction.GetComponentInChildren<Button>().onClick.AddListener(() => Player1FactionSelect(name));

            }
        }
        catch (System.Exception ex)
        {
            Log(ex.Message);
            Debug.Log(ex.Message);
        }
    }

    public void Log(string debugMessage)                                           //Debug messages to output.txt in streaming assets folder. Works with build.
    {
        System.IO.StreamWriter writer = new System.IO.StreamWriter(debugPath, true);
        writer.WriteLine(debugMessage + "\n");
        writer.Close();
    }
}

public class FactionElement
{
    public string name;
    public string FactionPath;

    public List<string> unitList;

    public FactionElement(string factionName, string factionPath, List<string> units)
    {
        name = factionName;
        FactionPath = factionPath;
        unitList = units;
    }
}
