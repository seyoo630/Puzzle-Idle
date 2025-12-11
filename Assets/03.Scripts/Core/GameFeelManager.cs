using UnityEngine;
using DG.Tweening;
using System.Collections;

public class GameFeelManager : MonoBehaviour
{
    public static GameFeelManager Instance;

    [Header("Camera Shake Settings")]
    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.3f;
    public int shakeVibrato = 20;

    [Header("Hit Stop Settings")]
    public float hitStopDuration = 0.05f; // 짧을수록 툭, 길수록 콰광!

    private Camera mainCam;
    private Vector3 originalCamPos;
    private bool isShaking = false;

    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
        if (mainCam != null) originalCamPos = mainCam.transform.position;
    }

    // 1. 화면 흔들기 (극한의 타격감 필수)
    public void ShakeCamera(float strengthMultiplier = 1.0f)
    {
        if (mainCam == null) return;

        // 이미 흔들리고 있으면 덮어씌우기 위해 트윈 킬
        mainCam.transform.DOKill(true);
        mainCam.transform.position = originalCamPos;

        float str = shakeStrength * strengthMultiplier;
        mainCam.transform.DOShakePosition(shakeDuration, str, shakeVibrato, 90f, false, true)
            .OnComplete(() => mainCam.transform.position = originalCamPos);
    }

    // 2. 시간 정지 (히트 스탑) - 뇌가 타격을 인식하게 만듦
    public void DoHitStop(float durationMultiplier = 1.0f)
    {
        StopAllCoroutines();
        StartCoroutine(HitStopRoutine(hitStopDuration * durationMultiplier));
    }

    IEnumerator HitStopRoutine(float duration)
    {
        // 시간을 거의 멈춤 (0으로 하면 버그 날 수 있으니 아주 느리게)
        Time.timeScale = 0.001f;

        // 현실 시간(Realtime) 기준으로 대기
        yield return new WaitForSecondsRealtime(duration);

        // 시간 복구
        Time.timeScale = 1.0f;
    }

    // 3. 적 피격 연출 (하얗게 깜빡임 + 밀림)
    public void ApplyImpactToTarget(Transform target, Vector3 hitDirection)
    {
        // 넉백 (뒤로 살짝 밀렸다가 돌아옴)
        target.DOKill(true);
        Vector3 punchDir = hitDirection.normalized * 0.3f; // 밀리는 거리
        target.DOPunchPosition(punchDir, 0.2f, 10, 1f);

        // 화이트 플래시 (스프라이트가 있다면)
        var sprite = target.GetComponentInChildren<SpriteRenderer>(); // SPUM 구조에 따라 위치 다를 수 있음
        if (sprite != null)
        {
            // 원래 색 저장 필요하지만 간단히 하얗게 만들었다 복구
            Color originalColor = Color.white;
            sprite.material.DOKill();

            // 쉐이더를 이용한 Flash가 제일 좋지만, 없다면 Color 변경으로 대체
            // 순식간에 빨갛게 변했다가 돌아오기
            sprite.color = new Color(1f, 0.5f, 0.5f); // 붉은색 틴트
            sprite.DOColor(Color.white, 0.15f);
        }
    }
}