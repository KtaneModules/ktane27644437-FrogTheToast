using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using KModkit;
//using KMHelper;

public class frog27644437 : MonoBehaviour {

    public class ModSettingsJSON
    {
        public int countdownTime;
        public string note;
    }

    //public KMAudio[] ledSounds;

    public CalcPartition CalcFile;

    public KMAudio Audio;
    public KMBombInfo Info;
    public KMBombModule Module;
    public KMModSettings modSettings;

    public AudioClip[] ledAudio;

    public TextMesh serial;
    public TextMesh selectedSubset;
    public MeshRenderer[] parts;
    public Color solv;
    public Material solvedMat;

    public KMSelectable left, right;
    public KMSelectable[] ledSelect;

    public Material[] ledColor;
    public MeshRenderer[] ledRender;
    public Light[] ledLight;
    public MeshRenderer Wire;
    public MeshRenderer WireTemplate;

    int subsetIndex = 0;
    int[] subsetsCycle = new int[13] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };
    //static string subsetsCycle = "ABCDEFGHIJKLM";
    //static string subsetsCycle = "RGBCMYWVLSJPO";

    int[] subsets = new int[13] { 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13, 13 };  // user answer          index = elements     values = subsets
    int[] finalSubsets = new int[13];                                                    // module solution      index = elements     values = subsets

    List<int>[] DebugAnswer = new List<int>[13];
    List<int>[] answer = new List<int>[14];      // index = subset       lists = elements in subsets
    bool AnimActive = false;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved = false;


    void Awake()
    {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable led in ledSelect)
        {
            KMSelectable pressedLed = led;
            led.OnInteract += delegate () { PressLed(pressedLed); return false; };
        }

        left.OnInteract += delegate () { PressLeft(); return false; };
        right.OnInteract += delegate () { PressRight(); return false; };
    }

    void Start()
    {
        answer[13] = new List<int>();
       
        int i = 0;
        for (i = 0; i < 13; i++)
        {
            answer[13].Add(i);
            DebugAnswer[i] = new List<int>();
            answer[i] = new List<int>();
        };
 
        double stupidDouble = UnityEngine.Random.Range(60466176, 2176782336);
        long solution = (long)Math.Round(stupidDouble);
        //solution = 2100243584;
        //solution = 27644436;

        serial.text = Base36(solution);
        Debug.LogFormat("[27644437 #{0}] Module Serial: {1}", moduleId, solution);

        finalSubsets = CalcFile.CalcAnswer(solution, moduleId);


        for (int i12 = 0; i12 < 13; i12++)
        {
            DebugAnswer[finalSubsets[i12]].Add(i12);
        }
        string dddd = "";
        foreach (List<int> item in DebugAnswer) if (item.Count != 0) { { dddd += "{"; foreach (int fucker in item) { dddd += (fucker + ", "); } dddd += "}"; } }
        Debug.LogFormat("[27644437 #{0}] {1}", moduleId, dddd);

        string aaaa = ""; foreach (int item in finalSubsets) { aaaa += (item + ", "); }
        Debug.LogFormat("[27644437 #{0}] {1}", moduleId, aaaa);
    }

    void PressLed(KMSelectable led)
    {
        if (moduleSolved) { return; };
        

        //Debug.Log("Pressed LED " + (Int32.Parse(led.name) - 1));
        SetLed(Int32.Parse(led.name));

        /*
        string cccc = "";
        foreach (List<int> item in answer) if (item.Count != 0) { { cccc += "{"; foreach (int fucker in item) { cccc += (fucker + ", "); } cccc += "}"; } }
        Debug.LogFormat("[27644437 #{0}] {1}", moduleId, cccc);
        */

        //string aaaa = ""; foreach (int item in finalSubsets) { aaaa += (item + ", "); } Debug.Log(aaaa);
        //string bbbb = ""; foreach (int item in subsets) { bbbb += (item + ", "); }
        //Debug.LogFormat("[27644437 #{0}] {1}", moduleId, bbbb);

        if (CheckIfWon(subsets, finalSubsets) == true)
        {
            moduleSolved = true;
            GetComponent<KMBombModule>().HandlePass();
            Debug.LogFormat("[27644437 #{0}] is freaking solved", moduleId);

            AnimActive = true;
        }
        else
        {
            Audio.PlaySoundAtTransform(ledAudio[Int32.Parse(led.name)].name, transform);
        }

    }
    
    IEnumerator SolveAnimation(int count)
    {
        
        AnimActive = false;
        for (int i9 = 13; answer[i9 - 13].Count > 0; i9++)
        {
            Audio.PlaySoundAtTransform(ledAudio[i9 - count].name, transform);
            //Debug.Log("playing " + ledAudio[i9 - count].name);
            UpdateWires(answer[i9 - 13], i9 - 13, 1);

            foreach (int led in answer[i9 - 13])
            {
                ledRender[led].material = ledColor[1];
                SetColor(1, led);
                ledLight[led].range = 0.03f;
            }
            
            if (answer[i9 - 12].Count < 1)
            {
                foreach (MeshRenderer yellow in parts)
                {
                    yellow.material = solvedMat;
                }
                serial.color = solv;
                selectedSubset.color = solv;
            }
            
            yield return new WaitForSeconds(.2f);
        }

        /*
        foreach (MeshRenderer yellow in parts)
        {
            yellow.material = solvedMat;
        }
        serial.color = solv;
        selectedSubset.color = solv;
        */
        yield return new WaitForSeconds(.5f);
        

        for (int i10 = 0; answer[i10].Count > 0; i10++)
        {
            UpdateWires(answer[i10], i10, i10);
            foreach (int led1 in answer[i10])
            {
                ledRender[led1].material = ledColor[i10];
                SetColor(i10, led1);
                ledLight[led1].range = 0.02f;
            }
        }
    }
    
    public bool CheckIfWon(int[] answer, int[] final)
    {
        if (answer.Length != final.Length)
            return false;
        for (int i = 0; i < answer.Length; i++)
        {
            if (answer[i] != final[i])
                return false;
        }
        return true;
    }

    void UpdateWires(List<int> subset, int Index1, int Color)
    {

        if (GameObject.Find(moduleId + "wire" + Index1) != null) {
            DestroyImmediate(GameObject.Find(moduleId + "wire" + Index1)); };

        if (subset.Count() > 1)
        {
            GameObject stupidObject = new GameObject(moduleId + "wire" + Index1);
            subset.Sort();

            if (subset.Count() == 2) { DrawWire(subset[0], subset[1], stupidObject, Color); }
            else
            {
                int i = 0;
                for (i = 0; i < subset.Count(); i++)
                {
                    if (i == subset.Count() - 1) { DrawWire(subset[0], subset[i], stupidObject, Color); }
                                            else { DrawWire(subset[i], subset[i + 1], stupidObject, Color); };
                };
            };

            stupidObject.transform.parent = Wire.transform;
            //Debug.LogFormat("[27644437 #{0}] adding {1} to {2}", moduleId, stupidObject.name, Wire.name);
        };
    }

    void DrawWire(int led1, int led2, GameObject Parent, int Index)
    {
        Vector3 point1;     point1 = ledLight[led1].transform.position;
        Vector3 point2;     point2 = ledLight[led2].transform.position;

        Vector3 wirePos;    wirePos = Vector3.Lerp(point1, point2, 0.5f);
        Vector3 wireScale = Vector3.zero;

        MeshRenderer newWire;

        newWire = Instantiate(WireTemplate, new Vector3(wirePos[0], wirePos[1], wirePos[2]), Quaternion.Euler(0,0,0));   // the WireTemplate has been Reborn.

        newWire.transform.LookAt(point1);    // me, the new wire, am lookin at u. (point1)

        wireScale.Set(0.00075f, 0.00075f, (Vector3.Distance(point1, point2)));   // my variable is extendin
        newWire.transform.localScale = wireScale;   // now im extendin (to point1 and point2)
        newWire.material = ledColor[Index];   // now im a little gayer and thats ok (by about 17%)
        newWire.name = led1 + ":" + led2;

        newWire.transform.parent = Parent.transform;
        //Debug.LogFormat("[27644437 #{0}] adding {1} to {2}", moduleId, newWire.name, Parent.name);
        
    }

    void SetLed(int led)
    {
        if (subsets[led] == subsetIndex && subsets[led] < 13)       // resetting LED
        {
            ledRender[led].material = ledColor[13];
            ledLight[led].intensity = 0;

            subsets[led] = 13;
            answer[subsetIndex].Remove(led);
            answer[13].Add(led);

            //Audio.PlaySoundAtTransform("bulb", transform);
            UpdateWires(answer[subsetIndex], subsetIndex, subsetIndex);
        }
        else        // setting LED
        {
            if (subsets[led] != subsetIndex && subsets[led] != 13)         //swapping LED
            {
                answer[subsets[led]].Remove(led);
                UpdateWires(answer[subsets[led]], subsets[led], subsets[led]);
            }
            else
            {
                //Audio.PlaySoundAtTransform("bulb", transform);
            }

            ledRender[led].material = ledColor[subsetIndex];
            subsets[led] = subsetIndex;

            ledLight[led].intensity = 2;
            SetColor(subsetIndex, led);

            answer[subsetIndex].Add(led);
            answer[13].Remove(led);

            //Debug.Log("Set LED " + (led + 1) + " to: " + (subsetIndex + 1));
            //foreach (int bleh in answer[subsetIndex]) { Debug.Log("Subset " + subsetIndex + ": " + bleh); }
        };

        if (answer[subsetIndex].Count > 1)
        {
            UpdateWires(answer[subsetIndex], subsetIndex, subsetIndex);
        }

        //Debug.Log(subsetIndex + "     " + ledAudio[subsetIndex].name);
        

    }

    void SetColor(int indexer, int LED)
    {
        if (indexer == 0) { ledLight[LED].color = Color.red; }
        else if (indexer == 1) { ledLight[LED].color = Color.green; }
        else if (indexer == 2) { ledLight[LED].color = Color.blue; }
        else if (indexer == 3) { ledLight[LED].color = Color.cyan; }
        else if (indexer == 4) { ledLight[LED].color = Color.magenta; }
        else if (indexer == 5) { ledLight[LED].color = Color.yellow; }
        else if (indexer == 6) { ledLight[LED].color = Color.white; }
        else if (indexer == 7) { ledLight[LED].color = Color.Lerp(Color.red, Color.blue, 0.75f); }          // violet
        else if (indexer == 8) { ledLight[LED].color = Color.Lerp(Color.green, Color.yellow, 0.5f); }       // lime
        else if (indexer == 9) { ledLight[LED].color = Color.Lerp(Color.blue, Color.white, 0.25f); }        // sky
        else if (indexer == 10) { ledLight[LED].color = Color.Lerp(Color.green, Color.white, 0.25f); }      // jade
        else if (indexer == 11) { ledLight[LED].color = Color.Lerp(Color.magenta, Color.white, 0.25f); }    // pink
        else if (indexer == 12) { ledLight[LED].color = Color.Lerp(Color.red, Color.yellow, 0.25f); };       // orange
    }

    void PressLeft()
    {
        
        Audio.PlaySoundAtTransform("button_tick", transform);

        if (moduleSolved){return;};

        subsetIndex = subsetIndex - 1;
        if (subsetIndex > -1) { selectedSubset.text = subsetsCycle[subsetIndex].ToString(); }
        else { subsetIndex = 0; };
        
        //Debug.Log("Pressed left.");
    }

    void PressRight()
    {

        Audio.PlaySoundAtTransform("button_tick", transform);

        if (moduleSolved) { return; };

        subsetIndex = subsetIndex + 1;
        if (subsetIndex < 13) { selectedSubset.text = subsetsCycle[subsetIndex].ToString(); }
        else { subsetIndex = 12; };

        //Debug.Log("Pressed right.");
    }

    public static string Base36(long value)
    {
        int Base = 36;
        string Chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string result = "";

        while (value > 0)
        {
            result = Chars[Convert.ToInt32(value % Base)] + result;
            value /= Base;
        };
        return result;
    }

    void Update()
    {
        if (AnimActive == true)
        {
            int[] countA = new int[13];
            countA = subsets;
            Array.Sort(countA);
            Array.Reverse(countA);

            int count1 = countA[0] + 1;

            StartCoroutine(SolveAnimation(count1));
        }   
    }

}
