using TMPro;
using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [Header("Hit UI")]
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Animator _hitAnimator;
    [SerializeField] private TMP_Text _hitDamageText;
    [SerializeField] private float _criticalHitFont = 80f;
    [SerializeField] private Color _criticalHitColor = Color.red;
    [SerializeField] private float _normalHitFont = 65f;
    [SerializeField] private Color _normalHitColor = Color.white;

    private void Update()
    {
        if (Camera.main != null)
        {
            // رو به دوربین، بدون برعکس شدن عدد
            Transform cam = Camera.main.transform;
            Transform canvasTransform = _canvas.transform;

            // به جای نگاه کردن «به» دوربین، همان جهت دید دوربین را می‌گیریم
            canvasTransform.LookAt(
                canvasTransform.position + cam.rotation * Vector3.forward,
                cam.rotation * Vector3.up
            );
        }
    }
    public void PlayHitEffect(float amount, bool isCritical)
    {
        Debug.Log("Hit");
        int random = Random.Range(0, 3);
        Debug.Log("random number: " + random);
        _hitDamageText.text = amount.ToString();
        if (isCritical)
        {
            _hitDamageText.color = _criticalHitColor;
            _hitDamageText.fontSize = _criticalHitFont;
        }
        else
        {
            _hitDamageText.color = _normalHitColor;
            _hitDamageText.fontSize = _normalHitFont;
        }

        switch (random)
        {
            case 0:
                _hitAnimator.Play("Hit");
                break;
            case 1:
                _hitAnimator.Play("Hit 1");
                break;
            case 2:
                _hitAnimator.Play("Hit 2");
                break;
        }
    }
}