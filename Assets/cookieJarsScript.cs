using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KModkit;
using System.Linq;

public class cookieJarsScript : MonoBehaviour {

    public KMBombModule Module;
    public KMAudio Audio;
    public KMBombInfo Info;
    public KMSelectable jar, left, right;
    public TextMesh cookieAmountText, jarText;
    public MeshRenderer[] leds;
    public Material unlit, lit;
    public Transform jarTransform;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool solved;

    private int[] cookies = { 99, 99, 99 };
    private readonly string[] cookieNames = { "chocolate\nchip cookies", "sugar\ncookies", "m&m\ncookies", "oatmeal\nraisin\ncookies", "snickerdoodles", "peanut\nbutter\ncookies", "fortune\ncookies",
                                              "butter\ncookies", "gingerbread\ncookies", "OREOs" };
    private readonly string[] debugCookies = { "chocolate chip", "sugar", "m&m", "oatmeal raisin", "snickerdoodle", "peanut butter", "fortune", "butter", "gingerbread", "OREO" };
    private int shownJar = 0;
    private int[] cookieAmounts = { 0, 0, 0 };
    private int lastEaten, lastLastEaten;
    private int hunger = 0;

    private bool[] correctBtns = { false, false, false };
    private int highestCookie = 0, secondHighestCookie = 0, lowestCookie;

    int solves = 0;
    private readonly string[] ignoredModules = { "Forget Me Not", "Forget Everything", "Souvenir", "The Time Keeper", "Turn the Key", "The Swan", "Simon's Stages", "Cookie Jars" };
    
    void Start () {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += SetUpButtons;

        GenerateModule();
    }

    void SetUpButtons()
    {
        jar.OnInteract += delegate ()
        {
            if (!solved)
            {
                EatCookie();
            }

            jar.AddInteractionPunch();
            return false;
        };

        left.OnInteract += delegate ()
        {
            if (!solved)
            {
                ChangeJar(-1);
            }

            left.AddInteractionPunch();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
            return false;
        };

        right.OnInteract += delegate ()
        {
            if (!solved)
            {
                ChangeJar(1);
            }

            right.AddInteractionPunch();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
            return false;
        };

        leds[0].material = unlit;
        leds[1].material = unlit;
        leds[2].material = unlit;
        leds[3].material = unlit;
        leds[4].material = unlit;
    }

    void EatCookie()
    {
        int solvedModules = 0, solvableModules = 0;

        foreach (var module in Info.GetSolvedModuleNames())
        {
            if (!ignoredModules.Contains(module))
            {
                solvedModules++;
            }
        }

        foreach (var module in Info.GetSolvableModuleNames())
        {
            if (!ignoredModules.Contains(module))
            {
                solvableModules++;
            }
        }

        if ((solvedModules == solvableModules || hunger != 0) && cookieAmounts[shownJar] > 0)
        {
            CheckCookies();

            if (correctBtns[shownJar])
            {
                lastLastEaten = lastEaten;
                lastEaten = cookies[shownJar];

                DebugMsg("You ate a " + debugCookies[cookies[shownJar]] + " cookie. That was right!");
                cookieAmounts[shownJar]--;
                hunger = 0;
                Audio.PlaySoundAtTransform("OmNom", Module.transform);

                leds[0].material = unlit;
                leds[1].material = unlit;
                leds[2].material = unlit;
                leds[3].material = unlit;
                leds[4].material = unlit;

                if (cookieAmounts[shownJar] == 0)
                {
                    cookieAmountText.text = "[ No cookies! :( ]";
                }

                else if (cookieAmounts[shownJar] == 1)
                {
                    cookieAmountText.text = "[ 1 cookie! :| ]";
                }

                else
                {
                    cookieAmountText.text = "[ " + cookieAmounts[shownJar] + " cookies! :) ]";
                }
                
                if (cookieAmounts[0] == 0 && cookieAmounts[1] == 0 && cookieAmounts[2] == 0)
                {
                    Module.HandlePass();

                    DebugMsg("You ate all the cookies and solved the module!");

                    jarText.text = "GG!";
                    cookieAmountText.text = "[ No cookies!!! D: ]";

                    leds[0].material = lit;
                    leds[1].material = lit;
                    leds[2].material = lit;
                    leds[3].material = lit;
                    leds[4].material = lit;

                    solved = true;
                }
            }

            else
            {
                DebugMsg("You ate a " + debugCookies[cookies[shownJar]] + " cookie. You got food poisoning and died! STRIKE!");
                hunger = 0;

                Module.HandleStrike();
                Audio.PlaySoundAtTransform("OhNo", Module.transform);
            }
        }
    }

    void ChangeJar(int changeNum)
    {
        shownJar += changeNum;

        if (shownJar < 0)
        {
            shownJar += 3;
        }

        if (shownJar > 2)
        {
            shownJar -= 3;
        }

        StartCoroutine("Spin", changeNum);

        if (cookieAmounts[shownJar] == 0)
        {
            cookieAmountText.text = "[ No cookies! :( ]";
        }

        else if (cookieAmounts[shownJar] == 1)
        {
            cookieAmountText.text = "[ 1 cookie! :| ]";
        }

        else
        {
            cookieAmountText.text = "[ " + cookieAmounts[shownJar] + " cookies! :) ]";
        }
    }

    void GenerateModule()
    {
        for (int i = 0; i < cookies.Length; i++)
        {
            int rndCookie = Random.Range(0, 10);

            while (cookies.Contains(rndCookie))
            {
                rndCookie = (rndCookie + 1) % 10;
            }

            cookies[i] = rndCookie;

            if (cookies[i] < cookies[(i + 1) % cookies.Length] && cookies[i] < cookies[(i + 2) % cookies.Length])
            {
                highestCookie = i;
            }

            else if (cookies[i] < cookies[(i + 1) % cookies.Length] || cookies[i] < cookies[(i + 2) % cookies.Length])
            {
                secondHighestCookie = i;
            }

            else
            {
                lowestCookie = i;
            }
        }
        
        float averageCookies = Info.GetSolvableModuleNames().Count / 10f;
        int slightlyLessAccurateAverageCookies = Info.GetSolvableModuleNames().Count / 10;

        if (slightlyLessAccurateAverageCookies - averageCookies < .3f)
        {
            cookieAmounts[0] = slightlyLessAccurateAverageCookies;
            cookieAmounts[1] = slightlyLessAccurateAverageCookies;
            cookieAmounts[2] = slightlyLessAccurateAverageCookies;
        }

        else if (slightlyLessAccurateAverageCookies - averageCookies < .6f)
        {
            cookieAmounts[0] = slightlyLessAccurateAverageCookies + 1;
            cookieAmounts[1] = slightlyLessAccurateAverageCookies;
            cookieAmounts[2] = slightlyLessAccurateAverageCookies;
        }

        else
        {
            cookieAmounts[0] = slightlyLessAccurateAverageCookies + 1;
            cookieAmounts[1] = slightlyLessAccurateAverageCookies + 1;
            cookieAmounts[2] = slightlyLessAccurateAverageCookies;
        }

    // *starts 967 module bomb*
    // *gets 290 cookies* 
    // *https://i.kym-cdn.com/entries/icons/original/000/027/475/Screen_Shot_2018-10-25_at_11.02.15_AM.png*

        if (cookieAmounts[2] == 0)
        {
            cookieAmounts[0] = 1;
            cookieAmounts[1] = 1;
            cookieAmounts[2] = 1;
        }

        jarText.text = cookieNames[cookies[shownJar]];

        if (cookieAmounts[shownJar] == 0)
        {
            cookieAmountText.text = "[ No cookies! :( ]";
        }

        else if (cookieAmounts[shownJar] == 1)
        {
            cookieAmountText.text = "[ 1 cookie! :| ]";
        }

        else
        {
            cookieAmountText.text = "[ " + cookieAmounts[shownJar] + " cookies! :) ]";
        }

        lastEaten = Info.GetSerialNumberNumbers().First();
        lastLastEaten = Info.GetSerialNumberNumbers().Skip(1).First();

        for (int i = 0; i < 3; i++)
        {
            DebugMsg("One of the jars has " + cookieAmounts[i] + " " + cookieNames[cookies[i]].Replace("\n", " ") + " inside.");
        }

        DebugMsg("The last eaten cookie was a " + debugCookies[lastEaten] + " cookie and the cookie eaten before that was a " + debugCookies[lastLastEaten] + " cookie.");
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Cookie Jars #{0}] {1}", _moduleId, msg.Replace("\n", " "));
    }

    void CheckCookies()
    {
        for (int i = 0; i < 3; i++)
        {
            correctBtns[i] = false;

            if (cookies[i] == 0 && lastEaten != lastLastEaten)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 1 && lastEaten == lastLastEaten)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 2 && lastEaten < lastLastEaten)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 3 && lastEaten > lastLastEaten)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 4 && lastEaten == 4)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 5 && lastEaten != 5)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 6 && lastEaten % 2 == Info.GetSolvedModuleNames().Count() % 2)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 7 && lastEaten % 2 != Info.GetSolvedModuleNames().Count() % 2)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 8 && cookieAmounts[i] % 2 == Info.GetSolvedModuleNames().Count() % 2)
            {
                correctBtns[i] = true;
            }

            else if (cookies[i] == 9 && cookieAmounts[i] % 2 != Info.GetSolvedModuleNames().Count() % 2)
            {
                correctBtns[i] = true;
            }

            if (cookieAmounts[i] == 0)
            {
                correctBtns[i] = false;
            }
        }

        if (!correctBtns.Contains(true))
        {
            if (cookieAmounts[highestCookie] != 0)
            {
                correctBtns[highestCookie] = true;
            }

            else if (cookieAmounts[secondHighestCookie] != 0)
            {
                correctBtns[secondHighestCookie] = true;
            }

            else
            {
                correctBtns[lowestCookie] = true;
            }
        }
    }

    private void Update()
    {
        if (Info.GetSolvedModuleNames().Count > solves && !solved)
        {
            StopCoroutine("StrikeAnimation");
            solves++;
            hunger++;

            if (hunger == 1)
            {
                leds[0].material = lit;
            }

            if (hunger == 2)
            {
                leds[0].material = lit;
                leds[1].material = lit;
            }

            if (hunger == 3)
            {
                leds[0].material = lit;
                leds[1].material = lit;
                leds[2].material = lit;
            }

            if (hunger == 4)
            {
                leds[0].material = lit;
                leds[1].material = lit;
                leds[2].material = lit;
                leds[3].material = lit;
            }

            if (hunger == 5)
            {
                Module.HandleStrike();
                DebugMsg("You didn't eat a cookie. You starved to death! STRIKE!");

                StartCoroutine("HungerAnimation");
            }
        }
    }

    IEnumerator HungerAnimation()
    {
        hunger = 0;
        Audio.PlaySoundAtTransform("OhNo", Module.transform);

        for (int i = 0; i < 20; i++)
        {
            leds[0].material = lit;
            leds[1].material = lit;
            leds[2].material = lit;
            leds[3].material = lit;
            leds[4].material = lit;

            yield return new WaitForSeconds(.05f);

            leds[0].material = unlit;
            leds[1].material = unlit;
            leds[2].material = unlit;
            leds[3].material = unlit;
            leds[4].material = unlit;

            yield return new WaitForSeconds(.05f);
        }
       
    }

    IEnumerator Spin(int spinDirection)
    {
        for (int i = 0; i < 24; i++)
        {
            jarTransform.transform.Rotate(Vector3.down, 15 * spinDirection);
            yield return new WaitForSeconds(.005f);

            if (i == 12)
            {
                jarText.text = cookieNames[cookies[shownJar]];
            }
        }
    }

    public string TwitchHelpMessage = "!{0} cycle will cycle through the jars. !{0} eat will eat a cookie from the jar. !{0} left/!{0} right move to the left/right jars respectively.";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        if (cmd.ToLowerInvariant() == "cycle")
        {
            yield return null;
            yield return new KMSelectable[] { right };
            yield return new WaitForSeconds(2.5f);
            yield return new KMSelectable[] { right };
            yield return new WaitForSeconds(2.5f);
            yield return new KMSelectable[] { right };
            yield return new WaitForSeconds(1f);
        }

        else if (cmd.ToLowerInvariant() == "eat")
        {
            yield return null;
            yield return new KMSelectable[] { jar };
        }

        else if (cmd.ToLowerInvariant() == "left")
        {
            yield return null;
            yield return new KMSelectable[] { left };
        }

        else if (cmd.ToLowerInvariant() == "right")
        {
            yield return null;
            yield return new KMSelectable[] { right };
        }

        else
        {
            yield break;
        }
    }
}
