using System;
using System.Collections;
using System.Collections.Generic;
using App;
using TMPro;
using UnityEngine;
using Wtf;

public class GameStartCountdown : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI CountdownText;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RoutineCountdown());
    }

    private IEnumerator RoutineCountdown()
    {
        var lockstepSystem = Context.Inst.GetSystem<Lockstep.LockStepSystem>();
        var timeSystem = Context.Inst.GetSystem<TimeSystem>();

        while (true)
        {
            if (!timeSystem.HasSynced)
            {
                yield return null;
                continue;
            }

            if (lockstepSystem.GameStartServerTick <= 0)
            {
                yield return null;
                continue;
            }

            // Log.D("GameStartCountdown", "RoutineCountdown", timeSystem.ServerTickTimeMs, lockstepSystem.GameStartServerTick);
            var countdown = lockstepSystem.GameStartServerTick - timeSystem.ServerTickTimeMs;
            if (countdown <= 0)
            {
                CountdownText.text = "GO!";
                yield return Yielders.WaitForSeconds(0.5f);
                break;
            }
            else
            {
                CountdownText.text = ((countdown / 1000) + 1).ToString();
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
