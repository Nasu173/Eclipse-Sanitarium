using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class SanityEffectController : MonoBehaviour
{
    [Header("资源设置")]
    [SerializeField] private Shader distortionShader;
    private Material effectMaterial;

    [Header("调试设置 (0=崩坏, 100=正常)")]
    public bool useTestSanityOverride = false; // 勾选此项后，强制使用下面的滑块
    [Range(0f, 100f)]
    public float testSanity = 100f;
    
    [Header("开发者调试")]
    public bool debugForceRed = false;

    private int logCounter = 0;

    private void Start()
    {
        Debug.Log("[Sanity] 控制器已挂载并启动");
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (debugForceRed)
        {
            Graphics.Blit(null, destination);
            RenderTexture.active = destination;
            GL.Clear(false, true, Color.red);
            return;
        }

        if (distortionShader == null)
        {
            distortionShader = Shader.Find("Hidden/SanityDistortion");
            if (distortionShader == null)
            {
                Graphics.Blit(source, destination);
                return;
            }
        }

        if (effectMaterial == null || effectMaterial.shader != distortionShader)
        {
            effectMaterial = new Material(distortionShader);
            effectMaterial.hideFlags = HideFlags.HideAndDontSave;
            Debug.Log("[Sanity] 材质球已成功建立: " + distortionShader.name);
        }

        // 核心修正逻辑：
        float san = 100f;
        if (useTestSanityOverride)
        {
            san = testSanity;
        }
        else if (Application.isPlaying) 
        {
            if (SanityManager.Instance != null)
            {
                san = SanityManager.Instance.GetSanity();
            }
            else
            {
                // Manager 还没初始化完成时默认100，不报错
                san = 100f; 
            }
        }
        else
        {
            san = testSanity;
        }

        float normalizedSanity = san / 100f;

        // 映射参数，确保在 San=100 时效果为 0
        float distortion = Mathf.Lerp(-0.8f, 0f, normalizedSanity);
        float vignette = Mathf.Lerp(2.0f, 0.05f, normalizedSanity);
        float chromatic = Mathf.Lerp(0.08f, 0f, normalizedSanity);

        effectMaterial.SetFloat("_Distortion", distortion);
        effectMaterial.SetFloat("_Vignette", vignette);
        effectMaterial.SetFloat("_Chromatic", chromatic);

        // 每秒打一次日志(假设60fps)
        if (logCounter++ >= 60)
        {
            Debug.Log($"[Sanity] 实时渲染参数 - 当前San: {san:F1}, 扭曲值: {distortion:F2}");
            logCounter = 0;
        }

        Graphics.Blit(source, destination, effectMaterial);
    }

    private void OnDisable()
    {
        if (effectMaterial != null)
            DestroyImmediate(effectMaterial);
    }
}
