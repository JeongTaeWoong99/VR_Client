using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Password : MonoBehaviour
{
    public int[] currentPassword;
    public GameObject[] removal;
    public GameObject success;
    public GameObject meteor;
    public Sprite[] digits;
    public Image[] currentDigits;
    public int[] _numbers;

    private void Start()
    {
        for (int i = 0; i < currentDigits.Length; i++)
        {
            currentDigits[i].sprite = digits[0];
        }
    }

    public void CheckPassword()
    {
        if (_numbers[3] == currentPassword[3] && _numbers[2] == currentPassword[2] &&
            _numbers[1] == currentPassword[1] && _numbers[0] == currentPassword[0])
        {
            foreach (var VARIABLE in removal)
            {
                VARIABLE.SetActive(false);
            }
            success.SetActive(true);
            AudioManager.instance.Play("Success");

            StartCoroutine(DelayAndPlayMeteor());

        }
        else
        {
            AudioManager.instance.Play("Fail");
        }
    }

    private void AdjustNumber(int curDigit, int addNumber)
    {
        _numbers[curDigit] += addNumber;
        if (_numbers[curDigit] < 0)
        {
            _numbers[curDigit] = 9;
        }

        _numbers[curDigit] %= 10;

        currentDigits[curDigit].sprite = digits[_numbers[curDigit]];
    }
    private IEnumerator DelayAndPlayMeteor()
    {
        // 6초 대기
        yield return new WaitForSeconds(6f);

        // 이후 Meteor 관련 로직 실행
        AudioManager.instance.Play("Meteor");
        meteor.SetActive(true);
    }
    
    public void IncreaseNumber4()
    {
        AdjustNumber(3, 1);
    }
    public void IncreaseNumber3()
    {
        AdjustNumber(2, 1);
    }
    public void IncreaseNumber2()
    {
        AdjustNumber(1, 1);
    }
    public void IncreaseNumber1()
    {
        AdjustNumber(0, 1);
    }
    public void DecreaseNumber4()
    {
        AdjustNumber(3, -1);
    }
    public void DecreaseNumber3()
    {
        AdjustNumber(2, -1);
    }
    public void DecreaseNumber2()
    {
        AdjustNumber(1, -1);
    }
    public void DecreaseNumber1()
    {
        AdjustNumber(0, -1);
    }
}