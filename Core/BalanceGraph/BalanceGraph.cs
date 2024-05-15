using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BALANCE
{
    public class BalanceGraph : MonoBehaviour
    {
        private Stats stats;

        [SerializeField]
        private Material radarMaterial;

        [SerializeField]
        private UnityEngine.UI.Image Background;

        [SerializeField]
        private CanvasGroup canvas;

        private DIALOGUE.CanvasGroupController cg = null;

        [SerializeField]
        private CanvasRenderer radarMeshRenderer;

        private Coroutine co_stating = null;
        public bool is_stating => co_stating != null;

        public static BalanceGraph instance;

        private List<float> target;
        private Dictionary<Stats.StatType, float> current;

        private void StatsChanged(object sender, EventArgs e)
        {
            UpdateStatsVisual();
        }

        private void Awake()
        {
            instance = this;
            Init();
        }

        private void Init()
        {
            this.stats = new Stats(5, 5, 5, 5, 5);
            current = new Dictionary<Stats.StatType, float>();
            current.Add(Stats.StatType.Hope, stats.GetStat(Stats.StatType.Hope));
            current.Add(Stats.StatType.Logic, stats.GetStat(Stats.StatType.Logic));
            current.Add(Stats.StatType.Emotions, stats.GetStat(Stats.StatType.Emotions));
            current.Add(Stats.StatType.Loyality, stats.GetStat(Stats.StatType.Loyality));
            current.Add(Stats.StatType.Purpose, stats.GetStat(Stats.StatType.Purpose));
            stats.OnStatsChanged += StatsChanged;
            cg = new DIALOGUE.CanvasGroupController(this, canvas);
            cg.Hide();
            cg.SetInteractableState(false);
            SetStats(new Stats(5, 5, 5, 5, 5));
        }

        public IEnumerator Display(string trait)
        {
            Color brightColor = PanelDesigner.instance.GetBrightColor();
            radarMaterial.color = brightColor;
            Background.color = brightColor;
            yield return cg.Show();
            Stats.StatType type = stats.TryGetType(trait);
            float value = stats.GetStat(type) + 2.5f;
            yield return SetOneStat(type, value);
            yield return new WaitForSeconds(2);
            yield return cg.Hide();
        }

        public Coroutine SetStats(Stats stats)
        {
            if (is_stating)
            {
                StopCoroutine(co_stating);
                return co_stating;
            }
            co_stating = StartCoroutine(SettingStats(stats));
            //barColor = PanelDesigner.instance.GetBrightColor();
            
            //bar.GetComponent<UnityEngine.UI.Image>().color = barColor;
            return co_stating;
        }

        public bool HasTrait(string trait) => stats.TryGetType(trait) != Stats.StatType.None;

        public Coroutine SetOneStat(Stats.StatType stat, float target)
        {
            if (is_stating)
            {
                StopCoroutine(co_stating);
                return co_stating;
            }
            co_stating = StartCoroutine(SettingOneStat(stat, target));
            //barColor = PanelDesigner.instance.GetBrightColor();

            //bar.GetComponent<UnityEngine.UI.Image>().color = barColor;
            return co_stating;
        }

        private IEnumerator SettingStats(Stats stats)
        {
            target = new List<float>() { stats.GetStat(Stats.StatType.Hope), stats.GetStat(Stats.StatType.Logic), stats.GetStat(Stats.StatType.Loyality), stats.GetStat(Stats.StatType.Emotions), stats.GetStat(Stats.StatType.Purpose) };
            List<float> currentValues = current.Values.ToList();
            float speed = 0.25f;
            for (int i = 0; i < currentValues.Count; i++)
            {
                while (currentValues[i] != target[i])
                {
                    currentValues[i] = Mathf.MoveTowards(currentValues[i], target[i], speed * Time.time);
                    this.stats.SetStatAmmount(current.Keys.ToList()[i], currentValues[i]);
                    Debug.Log(currentValues[i]);
                    yield return null;
                }
                //Debug.LogWarning($"towards - {current.Keys.ToList()[i].ToString()} / {currentValues.ToList()[i]}");
            }

            co_stating = null;
        }

        private IEnumerator SettingOneStat(Stats.StatType stat, float target)
        {
            float speed = 0.75f;
            
                while (current[stat] != target)
                {
                current[stat] = Mathf.MoveTowards(current[stat], target, speed * Time.deltaTime);
                    this.stats.SetStatAmmount(stat, current[stat]);
                    yield return null;
                }
                //Debug.LogWarning($"towards - {current.Keys.ToList()[i].ToString()} / {currentValues.ToList()[i]}");
            

            co_stating = null;
        }

        private void UpdateStatsVisual()
        {
            Mesh mesh = new Mesh();
            Vector3[] verticles = new Vector3[6];
            Vector2[] uv = new Vector2[6];
            int[] triangles = new int[3 * 5];

            float RadarSize = 73.5f;
            float angleIncrement = 360 / 5;


            Vector3 hopeVertex = Quaternion.Euler(0, 0, -angleIncrement * 0) * Vector3.up * RadarSize * stats.GetStatNormalized(Stats.StatType.Hope);
            int HopeVertexIndex = 1;
            Vector3 logicVertex = Quaternion.Euler(0, 0, -angleIncrement * 1) * Vector3.up * RadarSize * stats.GetStatNormalized(Stats.StatType.Logic);
            int LogicVertexIndex = 2;
            Vector3 LoayalityVertex = Quaternion.Euler(0, 0, -angleIncrement * 2) * Vector3.up * RadarSize * stats.GetStatNormalized(Stats.StatType.Loyality);
            int LoyalityVertexIndex = 3;
            Vector3 purposeVertex = Quaternion.Euler(0, 0, -angleIncrement * 3) * Vector3.up * RadarSize * stats.GetStatNormalized(Stats.StatType.Purpose);
            int PurposeVertexIndex = 4;
            Vector3 emotionsVertex = Quaternion.Euler(0, 0, -angleIncrement * 4) * Vector3.up * RadarSize * stats.GetStatNormalized(Stats.StatType.Emotions);
            int EmotionsVertexIndex = 5;



            verticles[0] = Vector3.zero;
            verticles[HopeVertexIndex] = hopeVertex;
            verticles[LogicVertexIndex] = logicVertex;
            verticles[EmotionsVertexIndex] = emotionsVertex;
            verticles[PurposeVertexIndex] = purposeVertex;
            verticles[LoyalityVertexIndex] = LoayalityVertex;

            uv[0] = Vector2.zero;
            uv[HopeVertexIndex] = Vector2.one;
            uv[LogicVertexIndex] = Vector2.one;
            uv[EmotionsVertexIndex] = Vector2.one;
            uv[PurposeVertexIndex] = Vector2.one;
            uv[LoyalityVertexIndex] = Vector2.one;

            triangles[0] = 0;
            triangles[1] = HopeVertexIndex;
            triangles[2] = LogicVertexIndex;

            triangles[3] = 0;
            triangles[4] = LogicVertexIndex;
            triangles[5] = LoyalityVertexIndex;

            triangles[6] = 0;
            triangles[7] = LoyalityVertexIndex;
            triangles[8] = PurposeVertexIndex;

            triangles[9] = 0;
            triangles[10] = PurposeVertexIndex;
            triangles[11] = EmotionsVertexIndex;

            triangles[12] = 0;
            triangles[13] = EmotionsVertexIndex;
            triangles[14] = HopeVertexIndex;

            mesh.vertices = verticles;
            mesh.uv = uv;
            mesh.triangles = triangles;

            

            radarMeshRenderer.SetMesh(mesh);
            radarMeshRenderer.SetMaterial(radarMaterial, null);
        }
    }
}