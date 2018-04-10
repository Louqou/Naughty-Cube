using System.Collections;
using UnityEngine;
using UnityEngine.PostProcessing;

public class PPEffects : MonoBehaviour
{
    private ChromaticAberrationModel chromAModel;
    private float defaultChromAI;
    private float chromAReturnTime;

    private ColorGradingModel colorGModel;
    private float defaultColorGTemp;
    private float colorGReturnTime;

    public GameObject mainCam;
    private float effectLen = 1f;

    private bool stopEffect = true;

    private void Start()
    {
        chromAModel = mainCam.GetComponent<PostProcessingBehaviour>().profile.chromaticAberration;
        colorGModel = mainCam.GetComponent<PostProcessingBehaviour>().profile.colorGrading;
        defaultChromAI = chromAModel.settings.intensity;
        defaultColorGTemp = colorGModel.settings.basic.temperature;
    }

    private void Update()
    {
        ChromEffect();
        ColorGEffect();
    }

    private void ChromEffect()
    {
        if (Time.time < chromAReturnTime) {
            ChangeChromAI(Mathf.Lerp(chromAModel.settings.intensity, 1f, Time.deltaTime));
        }
        else if (stopEffect && System.Math.Round(chromAModel.settings.intensity, 2) > defaultChromAI) {
            ChangeChromAI(Mathf.Lerp(chromAModel.settings.intensity, defaultChromAI, Time.deltaTime));
        }
    }

    private void ColorGEffect()
    {
        if (Time.time < colorGReturnTime) {
            ChangeColorGTemp(Mathf.Lerp(colorGModel.settings.basic.temperature, 100f, Time.deltaTime * 2));
        }
        else if (stopEffect && System.Math.Round(colorGModel.settings.basic.temperature, 2) > defaultColorGTemp) {
            ChangeColorGTemp(Mathf.Lerp(colorGModel.settings.basic.temperature, defaultColorGTemp, Time.deltaTime));
        }
    }

    public void StartChromEffect(bool waitReturn)
    {
        stopEffect = !waitReturn;
        chromAReturnTime = Time.time + effectLen;
    }

    public void StartColorEffect(bool waitReturn)
    {
        stopEffect = !waitReturn;
        colorGReturnTime = Time.time + effectLen;
    }

    public void StopChromEffect()
    {
        stopEffect = true;
    }

    public void StopColorEffect()
    {
        stopEffect = true;
    }

    private void OnDestroy()
    {
        RestoreDefault();
    }

    public void RestoreDefault()
    {
        ChangeChromAI(defaultChromAI);
        ChangeColorGTemp(defaultColorGTemp);
    }

    private void ChangeChromAI(float intensity)
    {
        ChromaticAberrationModel.Settings newSet = chromAModel.settings;
        newSet.intensity = intensity;
        chromAModel.settings = newSet;
    }

    private void ChangeColorGTemp(float intensity)
    {
        ColorGradingModel.Settings newSet = colorGModel.settings;
        newSet.basic.temperature = intensity;
        colorGModel.settings = newSet;
    }
}
