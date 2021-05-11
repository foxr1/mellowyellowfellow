using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface FellowInterface
{
    public bool PowerupActive();
    public Vector3 GetPosition();
    public int PelletsEaten();
    public string GetDirection();
    public void SetScore(int newScore);
}
