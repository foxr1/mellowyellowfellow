using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface GhostInterface
{
    void died();
    Vector3 GetStartPos();
    void SetStartPos(Vector3 startPos);
    void ResetGhost();
    bool HasRespawned();
    void ResetRespawn();
    void SetNavMeshAgent(bool enabled);
    void SetSpeed(float speed);
    void SetPlayerTarget(FellowInterface fellow);
}
