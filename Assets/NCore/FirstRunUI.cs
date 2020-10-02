using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using NCore.Managers;

public class FirstRunUI : MonoBehaviour
{
    public GameManagerComponent gmc;

    public Transform firstPanel;
    public Transform secondPanel;
    public Transform thirdPanel;

    public float lerpSpeed;

    public float fWantedPosition;
    public float fCurrentPosition;

    public float sWantedPosition;
    public float sCurrentPosition;

    public float tWantedPosition;
    public float tCurrentPosition;

    private void Start()
    {
        StartCoroutine("moveToFirstPanel");
    }

    public void SecondPanel()
    {
        StartCoroutine("moveToSecondPanel");
        NDebug.Log(new NDebug.Info("Test"));
    }

    public void ThirdPanel()
    {
        StartCoroutine("moveToThirdPanel");
    }

    public void Finish()
    {
        StartCoroutine("finish");
    }

    IEnumerator moveToFirstPanel()
    {
        yield return new WaitForSeconds(1);
        fWantedPosition = 0;
    }

    IEnumerator moveToSecondPanel()
    {
        fWantedPosition = 1000;
        yield return new WaitForSeconds(0.25f);
        sWantedPosition = 0;
    }

    IEnumerator moveToThirdPanel()
    {
        sWantedPosition = 1000;
        yield return new WaitForSeconds(0.25f);
        tWantedPosition = 0;
    }

    IEnumerator finish()
    {
        tWantedPosition = 1000;
        yield return new WaitForSeconds(0.25f);
        GameManager.NoticeReadBeginGame();
    }

    private void FixedUpdate()
    {
        fCurrentPosition = Mathf.Lerp(fCurrentPosition, fWantedPosition, lerpSpeed);
        sCurrentPosition = Mathf.Lerp(sCurrentPosition, sWantedPosition, lerpSpeed);
        tCurrentPosition = Mathf.Lerp(tCurrentPosition, tWantedPosition, lerpSpeed);

        firstPanel.position = new Vector3(Screen.width / 2, fCurrentPosition + (Screen.height / 2), 0);
        secondPanel.position = new Vector3(Screen.width / 2, sCurrentPosition + (Screen.height / 2), 0);
        thirdPanel.position = new Vector3(Screen.width / 2, tCurrentPosition + (Screen.height / 2), 0);
    }
}
