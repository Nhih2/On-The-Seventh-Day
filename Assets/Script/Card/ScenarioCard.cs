using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScenarioCard : MonoBehaviour
{
    public Scenario scenario { get; private set; }

    [SerializeField] private TextMeshProUGUI des, req, suc, fal, multiplier;
    [SerializeField] private Image image;

    private ScenarioManager Manager;

    void Awake()
    {
        multiplier.gameObject.SetActive(false);
        image.gameObject.SetActive(false);
    }


    public void Initialize(ScenarioManager manager, int type, Vector3 ogPos)
    {
        scenario = GameSetting.Scenarios[type];
        Manager = manager;
        transform.position = ogPos;
    }

    //

    public float ShowMultiplier(string mul, bool isMult)
    {
        multiplier.gameObject.SetActive(true);
        image.gameObject.SetActive(true);
        multiplier.text = mul;
        return ShowAnimation(isMult);
    }

    public void HideMultiplier()
    {
        multiplier.gameObject.SetActive(false);
    }

    public void FlashRequirement()
    {
        StartCoroutine(FlashText(req));
    }

    public void FlashSuccess()
    {
        StartCoroutine(FlashText(suc));
    }

    public void FlashFailure()
    {
        StartCoroutine(FlashText(fal));
    }

    private IEnumerator FlashText(TextMeshProUGUI text)
    {
        float delay = 0.5f;
        Color flashColor = Color.white;
        Color textColor = text.color;
        text.color = flashColor;
        yield return new WaitForSeconds(delay);
        text.color = textColor;
    }

    private float ShowAnimation(bool isMult)
    {
        Vector3 targetScale = new(1, 1, 1), baseScale = new(0, 0, 0), imgScale = new(0.75f, 0.75f, 0.75f);
        Vector3 targetRotate = new(0, 0, UnityEngine.Random.Range(45, 90)), baseRotation = new(0, 0, 0);
        Ease tweenEaseIn = Ease.OutBack, tweenEaseOut = Ease.InBack;
        float fadeTime = 0.5f, delayTime = 0.15f;

        multiplier.transform.localScale = baseScale;
        image.color = GameSetting.MULT_COLOR;
        if (!isMult)
            image.color = GameSetting.SCORE_COLOR;
        image.color = image.color.ChangeTransparency(1);
        image.transform.localScale = baseScale;
        image.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 45));



        Sequence seq = DOTween.Sequence();
        seq.Append(multiplier.transform.DOScale(targetScale, delayTime).SetEase(tweenEaseIn));
        seq.AppendInterval(delayTime);
        seq.Append(multiplier.transform.DOScale(baseScale, delayTime).SetEase(tweenEaseOut));

        Sequence seqImg = DOTween.Sequence();
        seqImg.Append(image.transform.DOScale(imgScale, fadeTime - delayTime).SetEase(tweenEaseIn));
        seqImg.Join(image.GetComponent<RectTransform>()
            .DORotate(new Vector3(0, 0, targetRotate.z), fadeTime - delayTime, RotateMode.FastBeyond360)
            .SetEase(tweenEaseIn));
        seqImg.Join(image.DOFade(0, fadeTime).SetEase(tweenEaseIn));
        seqImg.AppendCallback(() =>
        {
            image.gameObject.SetActive(false);
            multiplier.gameObject.SetActive(false);
        });

        return fadeTime;
    }

    //

    void Update()
    {
        HandleDescription();
        HandleRequirement();
        HandleSuccess();
        HandleFailure();
    }

    private void HandleSuccess()
    {
        suc.text = "";
        int cnt = 0;
        if (scenario.success.multiplier != 0)
        {
            cnt++;
            suc.text += (scenario.success.multiplier < 0 ? "" : "+") + scenario.success.multiplier.FloatDisplay() + " Mult";
        }
        if (scenario.success.energy != 0)
        {
            if (cnt > 0) suc.text += "/ ";
            cnt++;
            suc.text += "Energy: " + scenario.success.energy;
        }
        if (scenario.success.heart != 0)
        {
            if (cnt > 0) suc.text += "/ ";
            cnt++;
            suc.text += "Heart: " + scenario.success.heart;
        }
    }

    private void HandleFailure()
    {
        fal.text = "";
        int cnt = 0;
        if (scenario.failure.multiplier != 0)
        {
            cnt++;
            fal.text += (scenario.failure.multiplier < 0 ? "" : "+") + scenario.failure.multiplier.FloatDisplay() + " Mult";
        }
        if (scenario.failure.energy != 0)
        {
            if (cnt > 0) fal.text += "/ ";
            cnt++;
            fal.text += "Energy: " + scenario.failure.energy;
        }
        if (scenario.failure.heart != 0)
        {
            if (cnt > 0) fal.text += "/ ";
            cnt++;
            fal.text += "Heart: " + scenario.failure.heart;
        }
    }

    private void HandleDescription()
    {
        des.text = scenario.name;
    }

    private void HandleRequirement()
    {
        int cnt = 0;
        req.text = "";
        if (scenario.GetDex() > 0)
        {
            req.text += $"DEX: {scenario.GetDex()}";
            cnt++;
        }
        if (scenario.stat_str > 0)
        {
            if (cnt > 0) req.text += "/ ";
            req.text += $"STR: {scenario.stat_str}";
            cnt++;
        }
        if (cnt >= 2)
        {
            req.text += "\n";
            cnt -= 2;
        }
        if (scenario.stat_int > 0)
        {
            if (cnt > 0) req.text += "/ ";
            req.text += "INT: " + scenario.stat_int;
            cnt++;
        }
        if (cnt >= 2)
        {
            req.text += "\n";
            cnt -= 2;
        }
        if (scenario.stat_sym > 0)
        {
            if (cnt > 0) req.text += "/ ";
            req.text += "SYM: " + scenario.stat_sym;
            cnt++;
        }
        if (cnt >= 2)
        {
            req.text += "\n";
            cnt -= 2;
        }
        if (scenario.required.energy != 0)
        {
            if (cnt > 0) req.text += "/ ";
            req.text += "Energy: " + scenario.required.energy;
            cnt++;
        }
        if (cnt >= 2)
        {
            req.text += "\n";
            cnt -= 2;
        }
        if (scenario.required.heart != 0)
        {
            if (cnt > 0) req.text += "/ ";
            req.text += "Heart: " + scenario.required.heart;
            cnt++;
        }
    }
}
