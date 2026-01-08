using UnityEngine;
using UnityEngine.UI;
using TMPro; // 1. این کتابخانه برای TextMeshPro ضروری است

public class EnemyUI : MonoBehaviour
{
    // 2. تغییر نوع متغیر از Text به TMP_Text
    public TMP_Text statusText;
    public Image healthFill;

    private Health myHealth;
    private Camera mainCam;

    void Start()
    {
        myHealth = GetComponentInParent<Health>();
        mainCam = Camera.main;

        // پیدا کردن خودکار (اگر دستی ست نشده باشد)
        if (!statusText) statusText = GetComponentInChildren<TMP_Text>();
    }

    void Update()
    {
        // بیلبوردینگ (چرخش به سمت دوربین)
        if (mainCam && statusText)
        {
            // نکته: برای TextMeshPro گاهی لازم است کل Canvas را بچرخانیم یا خود متن را
            // اینجا فرض بر این است که Text داخل Canvas است
            statusText.transform.parent.LookAt(transform.position + mainCam.transform.rotation * Vector3.forward,
                                               mainCam.transform.rotation * Vector3.up);
        }

        if (myHealth && healthFill)
        {
            healthFill.fillAmount = myHealth.currentHealth / myHealth.maxHealth;
        }
    }

    public void UpdateStatus(string state, string action, bool isDanger)
    {
        if (statusText)
        {
            statusText.text = $"{state}\n<size=80%>[{action}]</size>"; // سایز اکشن رو کمی کوچکتر کردم
            statusText.color = isDanger ? Color.red : Color.green;

            // اگر خواستی خیلی خوشگل بشه، میتونی Outline رو هم اینجا فعال کنی (از توی ادیتور راحت تره)
        }
    }
}