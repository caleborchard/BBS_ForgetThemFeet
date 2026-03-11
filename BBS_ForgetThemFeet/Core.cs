using System;
using Il2Cpp;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(BBS_ForgetThemFeet.Core), "BBS_ForgetThemFeet", "1.1.2", "Caleb Orchard", null)]
[assembly: MelonGame("DefaultCompany", "BabySteps")]

namespace BBS_ForgetThemFeet
{
    public class Core : MelonMod
    {
        PlayerMovement pm;
        bool hasHiddenFeet = false;
        float nextWarningTime = 0f;

        // Config
        private MelonPreferences_Category cfg;
        private MelonPreferences_Entry<float> colorR;
        private MelonPreferences_Entry<float> colorG;
        private MelonPreferences_Entry<float> colorB;
        private MelonPreferences_Entry<float> scaleMultiplier;

        public override void OnInitializeMelon()
        {
            cfg = MelonPreferences.CreateCategory("ForgetThemFeet");

            colorR = cfg.CreateEntry("MudColor_R", 0.537f);
            colorG = cfg.CreateEntry("MudColor_G", 0.318f);
            colorB = cfg.CreateEntry("MudColor_B", 0.161f);

            scaleMultiplier = cfg.CreateEntry("ScaleMultiplier", 1.05f);

            cfg.SaveToFile();
        }

        public override void OnUpdate()
        {
            if (hasHiddenFeet) return;

            try
            {
                if (pm == null || pm.gameObject == null)
                {
                    GameObject dudest = GameObject.Find("Dudest");
                    if (dudest == null) return;
                    pm = dudest.GetComponent<PlayerMovement>();
                }
                if (pm == null || pm.gameObject == null) return;
                if (pm.feet == null || pm.feet.Count == 0) return;

                Color mudColor = new Color(colorR.Value, colorG.Value, colorB.Value);
                float scaleMul = scaleMultiplier.Value;
                bool appliedMudMeshChanges = false;

                for (int i = 0; i < pm.feet.Count; i++)
                {
                    FootData footData = pm.feet[i];
                    if (footData == null) continue;

                    Transform foot = footData.coll?.transform;
                    if (foot == null) continue;

                    Transform parent = foot.parent;
                    if (parent == null) continue;

                    Transform mudMesh = null;
                    for (int childIndex = 0; childIndex < parent.childCount; childIndex++)
                    {
                        Transform child = parent.GetChild(childIndex);
                        if (child != null && child.name.Contains("_mudMesh"))
                        {
                            mudMesh = child;
                            break;
                        }
                    }
                    if (mudMesh == null) continue;

                    mudMesh.gameObject.layer = 0;

                    MeshRenderer renderer = mudMesh.GetComponent<MeshRenderer>();
                    if (renderer != null) renderer.material.color = mudColor;

                    mudMesh.localScale *= scaleMul;
                    appliedMudMeshChanges = true;
                }

                if (appliedMudMeshChanges)
                {
                    hasHiddenFeet = true;
                }
            }
            catch (Exception ex)
            {
                pm = null;
                if (Time.unscaledTime >= nextWarningTime)
                {
                    MelonLogger.Warning($"ForgetThemFeet retrying after transient setup error: {ex.Message}");
                    nextWarningTime = Time.unscaledTime + 2f;
                }
            }
        }
    }
}
