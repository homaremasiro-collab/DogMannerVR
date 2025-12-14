using UnityEngine;
using TMPro;

public class DogTouchDetector : MonoBehaviour
{
    [Header("References")]
    public Transform headPoint;
    public Transform nosePoint;
    public Transform playerHead;
    public Animator dogAnimator;
    public TextMeshProUGUI messageText;

    [Header("Animator Trigger Names (Case Sensitive)")]
    public string triggerHappy = "Happy";
    public string triggerNervous = "Nervous";
    public string triggerScared = "Scared";

    [Header("Settings")]
    public float crouchHeight = 1.2f;
    public float noseHeightTolerance = 0.2f;

    [Header("Result")]
    public int likePoint = 0;

    [Header("Debug / Safety")]
    public bool printStayLog = false;      // うるさければOFF
    public float cooldownSeconds = 0.5f;   // 連続判定防止
    private float nextAcceptTime = 0f;

    private void Start()
    {
        // 参照が切れてたらここで分かる
        Debug.Log($"[DogTouchDetector] Start on {name}");

        if (headPoint == null) Debug.LogError("[DogTouchDetector] headPoint is NULL");
        if (nosePoint == null) Debug.LogError("[DogTouchDetector] nosePoint is NULL");
        if (playerHead == null) Debug.LogError("[DogTouchDetector] playerHead is NULL");
        if (dogAnimator == null) Debug.LogError("[DogTouchDetector] dogAnimator is NULL");
        else Debug.Log($"[DogTouchDetector] Animator Controller = {dogAnimator.runtimeAnimatorController?.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[DogTouchDetector] ENTER name={other.name} tag={other.tag}");

        if (Time.time < nextAcceptTime)
        {
            Debug.Log("[DogTouchDetector] -> Cooldown. Ignored.");
            return;
        }

        if (!other.CompareTag("Hand"))
        {
            Debug.Log("[DogTouchDetector] -> Not Hand tag. Ignored.");
            return;
        }

        nextAcceptTime = Time.time + cooldownSeconds;

        if (headPoint == null || nosePoint == null || playerHead == null)
        {
            Debug.LogError("[DogTouchDetector] Missing references. Stop.");
            return;
        }

        Vector3 handPos = other.transform.position;
        float handY = handPos.y;

        float headY = headPoint.position.y;
        float noseY = nosePoint.position.y;
        float playerHeadY = playerHead.position.y;

        Debug.Log($"[DogTouchDetector] values handY={handY:F3} headY={headY:F3} noseY={noseY:F3} playerHeadY={playerHeadY:F3}");

        bool isCrouching = playerHeadY < crouchHeight;
        bool isNearNoseHeight = Mathf.Abs(handY - noseY) < noseHeightTolerance;

        string resultText;

        if (isCrouching && isNearNoseHeight)
        {
            likePoint += 1;
            FireTrigger(triggerHappy);
            resultText = "あなたはしゃがんで手を差し出しました。犬は安心して匂いをかぎやすくなります。";
            Debug.Log("[DogTouchDetector] -> Happy");
        }
        else if (handY > headY + 0.1f)
        {
            FireTrigger(triggerNervous);
            resultText = "あなたは頭の上から手を出しました。犬によっては怖いと感じることがあります。";
            Debug.Log("[DogTouchDetector] -> Nervous");
        }
        else
        {
            likePoint -= 1;
            FireTrigger(triggerScared);
            resultText = "いきなり触られると、犬は驚いたり、不安を感じることがあります。";
            Debug.Log("[DogTouchDetector] -> Scared");
        }

        if (messageText != null) messageText.text = resultText;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!printStayLog) return;
        Debug.Log($"[DogTouchDetector] STAY name={other.name} tag={other.tag}");
    }

    private void FireTrigger(string triggerName)
    {
        if (dogAnimator == null)
        {
            Debug.LogError("[DogTouchDetector] dogAnimator is NULL");
            return;
        }

        // 連続で押しても安全にする（存在しないならConsoleに出る）
        dogAnimator.ResetTrigger(triggerHappy);
        dogAnimator.ResetTrigger(triggerNervous);
        dogAnimator.ResetTrigger(triggerScared);

        dogAnimator.SetTrigger(triggerName);
    }
}
