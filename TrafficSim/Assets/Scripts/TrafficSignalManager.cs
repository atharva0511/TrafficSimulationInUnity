using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficSignalManager : MonoBehaviour
{
    public int greenLightTime = 30;
    public int bufferTime = 10;
    public static Action<bool[]> SignalChange4way;
    public static Action<bool[]> SignalChange3way;

    public static bool[] signals4way;
    public static bool[] signals3way;

    public bool pauseSignals = false;
    public bool allGreen = false;

    float changeTime1 = 0;
    float changeTime2 = 0;

    private void Awake()
    {
        signals4way = new bool[4];
        signals3way = new bool[3];

        StartCoroutine(SignalCycle3way());
        StartCoroutine(SignalCycle4way());

        if (allGreen)
        {
            signals4way = new bool[] { true, true,true,true };
            signals3way = new bool[] { true, true,true };
        }

    }
    
    IEnumerator SignalCycle4way()
    {
        while (!pauseSignals && !allGreen)
        {
            signals4way = new bool[] { true, false, false, false};
            SignalChange4way?.Invoke(signals4way);
            yield return new WaitForSeconds(greenLightTime);
            
            signals4way = new bool[] { false, true, false, false};
            SignalChange4way?.Invoke(signals4way);
            yield return new WaitForSeconds(greenLightTime);
            
            signals4way = new bool[] { false, false, true, false,};
            SignalChange4way?.Invoke(signals4way);
            yield return new WaitForSeconds(greenLightTime);

            signals4way = new bool[] { false, false, false, true};
            SignalChange4way?.Invoke(signals4way);
            yield return new WaitForSeconds(greenLightTime);

            signals4way = new bool[] { false, false, false, false };
            SignalChange4way?.Invoke(signals4way);
            yield return new WaitForSeconds(bufferTime);
        }

    }

    IEnumerator SignalCycle3way()
    {
        while (!pauseSignals && !allGreen)
        {
            signals3way = new bool[] { true, false, false};
            SignalChange3way?.Invoke(signals3way);
            yield return new WaitForSeconds(greenLightTime);

            signals3way = new bool[] { false, true, false};
            SignalChange3way?.Invoke(signals3way);
            yield return new WaitForSeconds(greenLightTime);

            signals3way = new bool[] { false, false, true,};
            SignalChange3way?.Invoke(signals3way);
            yield return new WaitForSeconds(greenLightTime);

            signals3way = new bool[] { false, false, false};
            SignalChange3way?.Invoke(signals3way);
            yield return new WaitForSeconds(bufferTime);
        }

    }
}
