using UnityEngine;
using System;

public class SanityManager : MonoBehaviour
{
    public static SanityManager Instance { get; private set; }

    [Header("Sanity Settings")]
    [SerializeField] private float maxSanity = 100f;
    [SerializeField] private float currentSanity = 100f;

    // 事件：当 San 值改变时触发
    public event Action<float> OnSanityChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public float GetSanity() => currentSanity;
    public float GetSanityNormalized() => currentSanity / maxSanity;

    public void ChangeSanity(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity + amount, 0f, maxSanity);
        Debug.Log($"<color=magenta>[Sanity] San 值改变: {currentSanity} (增量: {amount})</color>");
        OnSanityChanged?.Invoke(currentSanity);
    }

    public void SetSanity(float value)
    {
        currentSanity = Mathf.Clamp(value, 0f, maxSanity);
        OnSanityChanged?.Invoke(currentSanity);
    }
}
