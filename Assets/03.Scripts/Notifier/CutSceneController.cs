using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using System.Collections;

public class CutsceneController : MonoBehaviour
{
    public static CutsceneController Instance { get; private set; }
    public static bool IsBusy { get; private set; } = false;


    public RectTransform cutscenePanel;
    public RawImage cutsceneImage;
    public VideoPlayer videoPlayer;

    private void Awake()
    {
        Instance = this;

        cutscenePanel.gameObject.SetActive(false);
        cutscenePanel.localScale = new Vector3(1f, 0f, 1f);
    }
    
    public IEnumerator PlayCutscene(VideoClip clip)
    {
        IsBusy = true;
        cutscenePanel.gameObject.SetActive(true);
        cutscenePanel.localScale = new Vector3(1f, 0f, 1f);

        videoPlayer.clip = clip;

        // 1) 팝업 (Y=0 → 1로 DOScale)
        yield return cutscenePanel.DOScaleY(1f, 0.35f)
            .SetEase(Ease.OutQuad)
            .WaitForCompletion();

        videoPlayer.Play();

        // 3) 영상 종료 대기
        while (videoPlayer.isPlaying)
            yield return null;

        // 4) 사라짐
        yield return cutscenePanel.DOScaleY(0f, 0.35f)
            .SetEase(Ease.InQuad)
            .WaitForCompletion();
        IsBusy = false;
        cutscenePanel.gameObject.SetActive(false);
    
    }
}
