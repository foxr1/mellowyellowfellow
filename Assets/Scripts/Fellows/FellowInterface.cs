using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface FellowInterface
{
    bool PowerupActive();
    Vector3 GetPosition();
    int PelletsEaten();
    string GetDirection();
    void SetScore(int newScore);
    Collider GetCollider();
}
