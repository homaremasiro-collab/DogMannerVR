using System.Collections;
using UnityEngine;

public class DogMoveOnTouch : MonoBehaviour
{
    [Header("References")]
    public Animator animator;       // 犬の Animator
    public Transform dogRoot;       // 犬のルート（このオブジェクト）
    public Transform walkTarget;    // 歩いて行くゴール（DogGoal）

    [Header("Move Settings")]
    public float turnSpeed = 180f;  // その場で回転する速さ（度/秒）
    public float moveSpeed = 1.5f;  // 歩く速さ
    public float reactionTime = 0.3f; // 触られてから動き出すまでの間

    bool hasStarted = false;

    private void OnTriggerEnter(Collider other)
    {
        // すでに一度反応していたら何もしない
        if (hasStarted) return;

        // Player タグのオブジェクト（XR Origin）が入ってきたら
        if (other.CompareTag("Player"))
        {
            hasStarted = true;
            StartCoroutine(TurnAndWalk());
        }
    }

    IEnumerator TurnAndWalk()
    {
        // 少し間をあけてから動き出す（ビクッと気づいた感じ）
        yield return new WaitForSeconds(reactionTime);

        if (dogRoot == null || walkTarget == null)
            yield break;

        // ── ① まずゴール方向に向く（ターン） ──
        Vector3 toTarget = walkTarget.position - dogRoot.position;
        toTarget.y = 0f; // 水平成分だけ見る

        if (toTarget.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(toTarget);

            // 角度差が1度以下になるまで徐々に回転
            while (Quaternion.Angle(dogRoot.rotation, targetRot) > 1f)
            {
                dogRoot.rotation = Quaternion.RotateTowards(
                    dogRoot.rotation,
                    targetRot,
                    turnSpeed * Time.deltaTime
                );

                yield return null;
            }
        }

        // ── ② 向き終わったら歩きアニメに切り替える ──
        if (animator != null)
        {
            animator.SetTrigger("ToWalk");
        }

        // ── ③ ゴールに向かって前進 ──
        while (true)
        {
            Vector3 toTarget2 = walkTarget.position - dogRoot.position;
            toTarget2.y = 0f;

            // ほぼ到着したら終了
            if (toTarget2.sqrMagnitude < 0.01f)
                break;

            // 向きがズレないように少しずつ修正しながら前進
            if (toTarget2 != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(toTarget2);
                dogRoot.rotation = Quaternion.RotateTowards(
                    dogRoot.rotation,
                    targetRot,
                    turnSpeed * 0.5f * Time.deltaTime
                );
            }

            dogRoot.position += dogRoot.forward * moveSpeed * Time.deltaTime;

            yield return null;
        }
    }
}
