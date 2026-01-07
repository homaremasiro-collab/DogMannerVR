using UnityEngine;

public class DogMoverSimple : MonoBehaviour
{
    [Header("Move Target (usually this dogmover root)")]
    [SerializeField] private Transform dogRoot;

    [Header("Move")]
    [SerializeField] private float speed = 0.8f;
    [SerializeField] private Vector3 worldDirection = Vector3.forward;

    [Header("Optional: Animator gating (move only while in walk state)")]
    [SerializeField] private Animator animator;
    [Tooltip("歩行ステート名（例: アーマチュア|walk_back / アーマチュア|walk_back1 など）")]
    [SerializeField] private string[] walkStateNames =
    {
        "アーマチュア|walk_back",
        "アーマチュア|walk_back1",
        "walk_back",
        "walk_back1",
    };
    [SerializeField] private bool moveOnlyWhenWalkingState = true;

    [Header("Optional: Flow gating (move only when flow says OK)")]
    [SerializeField] private DogStage2Flow flow;
    [SerializeField] private bool moveOnlyWhenFlowCanMove = true;

    private void Awake()
    {
        if (!dogRoot) dogRoot = transform;
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!flow) flow = FindObjectOfType<DogStage2Flow>();
        worldDirection = worldDirection.normalized;
    }

    private void Update()
    {
        // ① Flowが「動いていい」と言う時だけ動く
        if (moveOnlyWhenFlowCanMove && flow != null && !flow.CanMove)
            return;

        // ② 歩きステートの時だけ動く（happy / nervous / eat / scared_pose では動かない）
        if (moveOnlyWhenWalkingState && animator != null)
        {
            var st = animator.GetCurrentAnimatorStateInfo(0);

            bool isWalk = false;
            foreach (var name in walkStateNames)
            {
                // IsName は "Base Layer.ステート名" 形式が必要なことが多いので両対応
                if (st.IsName(name) || st.IsName("Base Layer." + name))
                {
                    isWalk = true;
                    break;
                }
            }

            if (!isWalk) return;
        }

        // ③ まっすぐ前に進む
        dogRoot.position += worldDirection * speed * Time.deltaTime;
    }
}
