using UnityEngine;
using UnityEngine.AI;

public class EnemyDogWalker : MonoBehaviour
{
    public Transform target;
    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (agent == null || target == null) return;

        agent.SetDestination(target.position);

        // ★ 状態ログ（重要）
        Debug.Log(
            $"OnNavMesh:{agent.isOnNavMesh} " +
            $"HasPath:{agent.hasPath} " +
            $"Stopped:{agent.isStopped} " +
            $"Remain:{agent.remainingDistance}"
        );
    }
}
