using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TextArchitect
{
    private TextMeshProUGUI tmpro_ui;
    private TextMeshPro tmpro_world;

    public TMP_Text tmpro
    {
        get
        {
            if (tmpro_ui != null) return tmpro_ui;
            else return tmpro_world;
        }
    }
    public string current_text => tmpro.text;
    public string target_text { get; private set; } = "";
    public string pre_text { get; private set; } = "";
    private int pre_text_len = 0;

    public string full_target_text => pre_text + target_text;

    public enum BuildMethod { instant, typewriter, fade }
    
    public BuildMethod Method = BuildMethod.fade;

    public Color textcolor { get { return tmpro.color; } set { tmpro.color = value; } }
    public float speed { get { return base_speed * speed_multiplier; } set { speed_multiplier = value; } }
    public float speed_multiplier = 1;
    public const float base_speed = 1;

    public int character_per_cycle
    {
        get
        {
            if (speed <= 2f) return character_multiplier;
            else
            {
                if (speed <= 2.5f)
                {
                    return character_multiplier * 2;
                }
                else return character_multiplier * 3;
            }
        }
    }
    private int character_multiplier = 1;

    public bool hurryup = false;

    public TextArchitect(TextMeshProUGUI ui)
    {
        this.tmpro_ui = ui;
    }
    public TextArchitect(TextMeshPro world)
    {
        this.tmpro_world = world;
    }
    public Coroutine Build (string build)
    {
        pre_text = "";
        target_text = build;

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;

    }
    //add text to what is all already in the text architecht
    public Coroutine Append(string build)
    {
        pre_text = tmpro.text;
        target_text = build;

        buildProcess = tmpro.StartCoroutine(Building());
        return buildProcess;

    }
    private Coroutine buildProcess = null;
    public bool isBuilding => buildProcess != null;
    public void Stop()
    {
        if (!isBuilding) return;
        tmpro.StopCoroutine(buildProcess);
        buildProcess = null;
    }
    IEnumerator Building()
    {
        Prepare(); 
        switch (Method)
        {
            case BuildMethod.typewriter:
                yield return Build_Typewriter();
                break;
            case BuildMethod.fade:
                yield return Build_Fade();
                break;
        }

    }

    private IEnumerator Build_Fade()
    {
        int minRange = pre_text_len;
        int maxRange = minRange + 1;
        byte alphaThreshold = 15;
        TMP_TextInfo textInfo = tmpro.textInfo;
        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        float[] alphas = new float[textInfo.characterCount];
        while (true)
        {
            float fadeSpeed;
            if (hurryup)
            {
                fadeSpeed = character_per_cycle * 5 * speed * 4f;
            }
            else fadeSpeed = character_per_cycle * speed * 4f;

            for (int i = minRange; i < maxRange; i++ )
            {

                TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
                if (!characterInfo.isVisible) continue;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                if(i >= alphas.Length)
                {
                    alphas = new float[textInfo.characterCount];
                }
                alphas[i] = Mathf.MoveTowards(alphas[i], 255, fadeSpeed);
                
                for (int v = 0; v < 4; v++)
                {
                    vertexColors[characterInfo.vertexIndex + v].a = (byte)alphas[i];
                }
                if (alphas[i] >= 255)
                {
                    minRange++;
                }
            }
            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            bool lastCharacterInvisible = !textInfo.characterInfo[maxRange - 1].isVisible;
            try
            {
                if (alphas[maxRange - 1] > alphaThreshold || lastCharacterInvisible)
                {
                    if (maxRange < textInfo.characterCount)
                    {
                        maxRange++;
                    }
                    else if (alphas[maxRange - 1] >= 255 || lastCharacterInvisible)
                    {
                        DIALOGUE.ConversationManager.userPrompt = true;
                        break;
                    }
                }
            }
            catch
            {
                Clear();
            }
            yield return new WaitForEndOfFrame();
        }
        

    }

    private IEnumerator Build_Typewriter()
    {
        while(tmpro.maxVisibleCharacters < tmpro.textInfo.characterCount)
        {
            if (hurryup)
            {
                tmpro.maxVisibleCharacters += character_per_cycle * 5;
            }
            else tmpro.maxVisibleCharacters += character_per_cycle;
            yield return new WaitForSeconds(0.055f / speed);
        }
    }

    public void ForceComplete()
    {
        switch (Method)
        {
            case BuildMethod.typewriter:
                tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
                break;
            case BuildMethod.fade:
                tmpro.ForceMeshUpdate();
                break;       
        }
        Stop();
        OnComplete();
    }
    private void Prepare()
    {
        switch (Method)
        {
            case BuildMethod.typewriter:
                PrepareTypewriter();
                break;
            case BuildMethod.fade:
                PrepareFade();
                break;
            case BuildMethod.instant:
                PrepareInstant();
                break;
            default:
                break;
        }
    }

    private void PrepareInstant()
    {
        tmpro.color = tmpro.color;
        tmpro.text = full_target_text;
        tmpro.ForceMeshUpdate();
        tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
    }

    private void PrepareTypewriter()
    {
        tmpro.color = tmpro.color;
        tmpro.maxVisibleCharacters = 0;
        tmpro.text = pre_text;
        if (pre_text != null)
        {
            tmpro.ForceMeshUpdate();
            tmpro.maxVisibleCharacters = tmpro.textInfo.characterCount;
        }
        tmpro.text += target_text;
        tmpro.ForceMeshUpdate();
    }

    private void PrepareFade()
    {
        tmpro.text = pre_text;
        if (pre_text == "")
        {
            tmpro.ForceMeshUpdate();
            pre_text_len = tmpro.textInfo.characterCount;
        }
        else pre_text_len = 0;
        tmpro.text += target_text;
        tmpro.maxVisibleCharacters = int.MaxValue;
        tmpro.ForceMeshUpdate();

        TMP_TextInfo textInfo = tmpro.textInfo;

        Color visible = new Color(textcolor.r, textcolor.g, textcolor.b, 1);
        Color invisible = new Color(textcolor.r, textcolor.g, textcolor.b, 0);

        Color32[] vertexColors = textInfo.meshInfo[textInfo.characterInfo[0].materialReferenceIndex].colors32;
        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo characterInfo = textInfo.characterInfo[i];
            if (!characterInfo.isVisible) continue;
            if (i < pre_text_len)
            {
                for (int v = 0; v <  4; v++)
                {
                    vertexColors[characterInfo.vertexIndex + v] = visible;
                }
            }
            else
            {
                for (int v = 0; v < 4; v++)
                {
                    vertexColors[characterInfo.vertexIndex + v] = invisible;
                }
            }
            tmpro.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

    }

    private void OnComplete()
    {
        buildProcess = null;
        hurryup = false;
    }

    public void Clear()
    {
        tmpro.ClearMesh();
    }
}
