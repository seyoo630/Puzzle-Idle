using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class BoardNotifier : MonoBehaviour
{
    public static BoardNotifier Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Animation Settings")]
    public float fadeInTime = 0.25f;
    public float stayTime = 0.9f;
    public float fadeOutTime = 0.25f;
    public float scalePunch = 1.2f;

    private Queue<string> messageQueue = new();
    private bool isPlaying = false;

    // ★ 현재 실행 중인 트윈을 저장하여 안전하게 Kill()하기 위함
    private Sequence activeSeq;

    private void Awake()
    {
        Instance = this;
        panelGroup.alpha = 0f;
        messageText.text = "";
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;
    }

    // ======================================================
    //  외부에서 호출할 공개 함수들
    // ======================================================

    public void ShowCombo(int comboCount)
    {
        if (comboCount <= 1) return;
        EnqueueMessage($"COMBO x{comboCount}");
    }

    public void ShowShuffle()
    {
        EnqueueMessage("SHUFFLE!");
    }

    public void ShowDeadBoard()
    {
        EnqueueMessage("NO MOVES\nSHUFFLING...");
    }

    public void ShowExtraTurn()
    {
        EnqueueMessage("EXTRA TURN!");
    }

    public void ShowChainMatch()
    {
        EnqueueMessage("CHAIN MATCH!");
    }

    public void ShowCustom(string msg)
    {
        EnqueueMessage(msg.ToUpper());
    }

    // ======================================================
    //  메시지 처리 로직
    // ======================================================

    private void EnqueueMessage(string msg)
    {
        messageQueue.Enqueue(msg);
        if (!isPlaying)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isPlaying = true;

        while (messageQueue.Count > 0)
        {
            string msg = messageQueue.Dequeue();
            yield return PlayMessage(msg);
        }

        isPlaying = false;
    }

    // ======================================================
    //  DOTween 메시지 연출
    // ======================================================

    private IEnumerator PlayMessage(string msg)
    {
        messageText.text = msg;

        // ★ 기존 트윈 먼저 제거 (잔상, 중복 출력 방지)
        if (activeSeq != null && activeSeq.IsActive())
            activeSeq.Kill();

        panelGroup.alpha = 0f;
        messageText.transform.localScale = Vector3.one * 0.75f;
        panelGroup.interactable = false;
        panelGroup.blocksRaycasts = false;

        activeSeq = DOTween.Sequence();

        // Fade-in + Scale Punch
        activeSeq.Append(panelGroup.DOFade(1f, fadeInTime));
        activeSeq.Join(messageText.transform.DOScale(scalePunch, fadeInTime).SetEase(Ease.OutBack));

        // 유지 시간
        activeSeq.AppendInterval(stayTime);

        // Fade-out
        activeSeq.Append(panelGroup.DOFade(0f, fadeOutTime));
        activeSeq.Join(messageText.transform.DOScale(0.85f, fadeOutTime));

        activeSeq.SetEase(Ease.OutQuad);

        activeSeq.Play();

        yield return activeSeq.WaitForCompletion();
    }
}
