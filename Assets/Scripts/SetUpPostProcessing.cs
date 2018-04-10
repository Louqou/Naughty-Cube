using UnityEngine.PostProcessing;
using UnityEngine;

public class SetUpPostProcessing : MonoBehaviour
{
    public PostProcessingProfile compPostStack;
    public PostProcessingProfile andPostStack;
    public SleekRender.SleekRenderSettings andPostSleek;

    private void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID || UNITY_WP8 || UNITY_IPHONE
        gameObject.AddComponent<PostProcessingBehaviour>().profile = andPostStack;
        gameObject.AddComponent<SleekRender.SleekRenderPostProcess>().settings = andPostSleek;
#else
        gameObject.AddComponent<PostProcessingBehaviour>().profile = compPostStack;
#endif
    }
}
