using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class retirementScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable retireButton;
    public KMSelectable cycleLeft;
    public KMSelectable cycleRight;
    public GameObject banner;
    public GameObject bunting;

    public String[] retirementHomeOptions;
    private List<int> selectedHomesIndices = new List<int>();
    private String[] selectedHomes = new String[5];
    private int[] selectedHomesScores = new int[5];
    private string[] selectedHomesOrdered = new String[5];
    private int displayedHomeIndex = 0;
    public TextMesh displayedHomeText;
    public Color[] homeBoardColours;

    private String[] wifeHomesOrdered = new String[5];
    private int[] wifeHomesScores = new int[5];
    private String[] wifeHomesAlphabetised = new String[5];

    public String[] potentialWifeNames;
    public String[] potentialChildNames;
    public String[] potentialSiblingNames;

    private string wifeName = "";
    private string childName = "";
    private string siblingName = "";

    private string correctHome = "";

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        retireButton.OnInteract += delegate () { OnRetireButton(); return false; };
        cycleLeft.OnInteract += delegate () { OnCycleLeft(); return false; };
        cycleRight.OnInteract += delegate () { OnCycleRight(); return false; };
    }


    void Start()
    {
        ClearHomes();
        SelectHomes();
        DetermineFamily();
        CalculateHomeScores();
    }

    void ClearHomes()
    {
        for(int i = 0; i <= 4; i++)
        {
            wifeHomesOrdered[i] = "";
            wifeHomesScores[i] = 0;
            wifeHomesAlphabetised[i] = "";
        }
        banner.SetActive(false);
        bunting.SetActive(false);
    }

    void SelectHomes()
    {
        for(int i = 0; i <= 4; i++)
        {
            int index = UnityEngine.Random.Range(0,10);
            while(selectedHomesIndices.Contains(index))
            {
                index = UnityEngine.Random.Range(0,10);
            }
            selectedHomesIndices.Add(index);
            selectedHomes[i] = retirementHomeOptions[index];
            selectedHomesOrdered[i] = retirementHomeOptions[index];
        }
        selectedHomesIndices.Clear();
        displayedHomeIndex = UnityEngine.Random.Range(0,5);
        displayedHomeText.text = selectedHomes[displayedHomeIndex];
        displayedHomeText.color = homeBoardColours[displayedHomeIndex];
    }

    void DetermineFamily()
    {
        int batteryCount = (Bomb.GetBatteryCount() % 5);
        for(int i = 0; i <= 4; i++)
        {
            if(batteryCount == i)
            {
                wifeName = potentialWifeNames[i];
            }
        }
        int portsIndic = (Bomb.GetPorts().Count() + Bomb.GetIndicators().Count()) % 10;
        {
            for(int i = 0; i <= 9; i++)
            {
                if(portsIndic == i)
                {
                    childName = potentialChildNames[i];
                }
            }
        }
        char firstCharacter = Bomb.GetSerialNumber().First();
        char secondCharacter = Bomb.GetSerialNumber()[1];
        char[] odd = new char[5]{'1','3','5','7','9'};
        char[] even = new char[5]{'2','4','6','8','0'};
        char[] letters = new char[26]{'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'};
        char[] vowels = new char[5]{'A','E','I','O','U'};
        char[] consonants = new char[21]{'B','C','D','F','G','H','J','K','L','M','N','P','Q','R','S','T','V','W','X','Y','Z'};
        if(odd.Contains(firstCharacter) && letters.Contains(secondCharacter))
        {
            siblingName = potentialSiblingNames[0];
        }
        else if(even.Contains(firstCharacter) && letters.Contains(secondCharacter))
        {
            siblingName = potentialSiblingNames[1];
        }
        else if(odd.Contains(secondCharacter) && letters.Contains(firstCharacter))
        {
            siblingName = potentialSiblingNames[2];
        }
        else if(even.Contains(secondCharacter) && letters.Contains(firstCharacter))
        {
            siblingName = potentialSiblingNames[3];
        }
        else if((vowels.Contains(firstCharacter) && vowels.Contains(secondCharacter)) || (consonants.Contains(firstCharacter) && consonants.Contains(secondCharacter)))
        {
            siblingName = potentialSiblingNames[4];
        }
        else if((vowels.Contains(firstCharacter) && consonants.Contains(secondCharacter)) || (vowels.Contains(secondCharacter) && consonants.Contains(firstCharacter)))
        {
            siblingName = potentialSiblingNames[5];
        }
        else
        {
            siblingName = potentialSiblingNames[6];
        }
        Debug.LogFormat("[Retirement #{0}] {1} is Bob's wife.", moduleId, wifeName);
        Debug.LogFormat("[Retirement #{0}] {1} is Bob's child.", moduleId, childName);
        Debug.LogFormat("[Retirement #{0}] {1} is Bob's sibling.", moduleId, siblingName);
    }

    void CalculateHomeScores()
    {
        for(int i = 0; i <= 4; i++)
        {
            int wifeScore = (selectedHomes[i].ToLower().Where((x) => wifeName.ToLower().Contains(x)).Count()) * 3;
            int childScore = (selectedHomes[i].ToLower().Where((x) => childName.ToLower().Contains(x)).Count()) * 2;
            int siblingScore = (selectedHomes[i].ToLower().Where((x) => siblingName.ToLower().Contains(x)).Count()) * 1;
            selectedHomesScores[i] = wifeScore + childScore + siblingScore;
            Debug.LogFormat("[Retirement #{0}] For {1}, {2} scores {3}, {4} scores {5} and {6} scores {7}. The total score for {1} is {8}.", moduleId, selectedHomes[i], wifeName, wifeScore, childName, childScore, siblingName, siblingScore, selectedHomesScores[i]);
        }
        Array.Sort(selectedHomesScores, selectedHomesOrdered);
        if(selectedHomesScores[4] != selectedHomesScores[3])
        {
            correctHome = selectedHomesOrdered[4];
            Debug.LogFormat("[Retirement #{0}] The correct retirement home is {1}.", moduleId, correctHome);
        }
        else
        {
            TieBreaker();
        }
    }

    void TieBreaker()
    {
        Debug.LogFormat("[Retirement #{0}] There is a tie.", moduleId);
        for(int i = 0; i <= 4; i++)
        {
            int wifeScore = (selectedHomesOrdered[i].ToLower().Where((x) => wifeName.ToLower().Contains(x)).Count()) * 3;
            wifeHomesScores[i] = wifeScore;
        }
        Array.Sort(wifeHomesScores, selectedHomesOrdered);
        if(wifeHomesScores[4] != wifeHomesScores[3])
        {
            correctHome = selectedHomesOrdered[4];
            Debug.LogFormat("[Retirement #{0}] {1}'s preferred home is {2}.", moduleId, wifeName, correctHome);
        }
        else
        {
            for(int i = 0; i <= 4; i++)
            {
                if(wifeHomesScores[i] == wifeHomesScores[4])
                {
                    wifeHomesAlphabetised[i] = selectedHomesOrdered[i];
                }
                else
                {
                    wifeHomesAlphabetised[i] = "z";
                }
            }
            Array.Sort(wifeHomesAlphabetised);
            correctHome = wifeHomesAlphabetised[0];
        }
        Debug.LogFormat("[Retirement #{0}] {1} is first alphabetically out of {2}'s preferred homes.", moduleId, correctHome, wifeName);
    }

    public void OnRetireButton()
    {
        if(moduleSolved)
        {
            return;
        }
        retireButton.AddInteractionPunch();
        if(displayedHomeText.text == correctHome)
        {
            Audio.PlaySoundAtTransform("bell", transform);
            moduleSolved = true;
            banner.SetActive(true);
            bunting.SetActive(true);
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[Retirement #{0}] Bob has retired to {1}. That is correct. Module disarmed.", moduleId, correctHome);
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            Debug.LogFormat("[Retirement #{0}] You tried to retire Bob to {1}. That is incorrect.", moduleId, displayedHomeText.text);
            Start();
        }
    }

    public void OnCycleLeft()
    {
        if(moduleSolved)
        {
            return;
        }
        cycleLeft.AddInteractionPunch(0.5f);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        displayedHomeIndex = (displayedHomeIndex + 9) % 5;
        displayedHomeText.text = selectedHomes[displayedHomeIndex];
        displayedHomeText.color = homeBoardColours[displayedHomeIndex];
    }

    public void OnCycleRight()
    {
        if(moduleSolved)
        {
            return;
        }
        cycleRight.AddInteractionPunch(0.5f);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        displayedHomeIndex = (displayedHomeIndex + 1) % 5;
        displayedHomeText.text = selectedHomes[displayedHomeIndex];
        displayedHomeText.color = homeBoardColours[displayedHomeIndex];
    }
}
