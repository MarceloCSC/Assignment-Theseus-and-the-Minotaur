using System;
using UnityEngine;

public class LevelEnd : MonoBehaviour
{
    public event Action OnLevelFinished = delegate { };

    private void OnTriggerEnter(Collider other)
    {
        // if the player crosses this trigger
        if (other.CompareTag("Player"))
        {
            // fires event informing that the level is finished
            OnLevelFinished();
        }
    }
}