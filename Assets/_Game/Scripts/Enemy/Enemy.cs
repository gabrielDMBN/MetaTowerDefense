using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float timePerTile = 1f;
    [SerializeField] private List<EPath> paths = new List<EPath>();
    [SerializeField] private Animator animator;
    [SerializeField] private Transform hitPoint;
    [SerializeField] private GameObject hitParticle; //prefab da particula de hit
    
    private List<Material> materials;
    private bool finishedPath = false;
    private int damageTaken = 0;
    
    // controle de tokens por material para coordenar chamadas sobrepostas
    private Dictionary<Material, int> _hitTokens = new Dictionary<Material, int>();
    private int _nextHitToken = 0;
    
    void Awake()
    {
        //pega todos os materiais do inimigo
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        materials = new List<Material>();
        foreach (var renderer in renderers)
        {
            materials.AddRange(renderer.materials);
        }
    }
    void Start()
    {
        if (paths == null || paths.Count == 0) return;
        StartCoroutine(WalkPath());
    }

    private void Update()
    {
        //se apertar T, toma dano
        if (Input.GetKeyDown(KeyCode.T))
        {
            GetHit(10);
        }
    }

    private IEnumerator WalkPath()
    {
        for (int i = 0; i < paths.Count; i++)
        {
            yield return new WaitForSeconds(timePerTile);

            Vector3 target = paths[i].transform.position;
            Vector3 current = transform.position;
            transform.position = new Vector3(target.x, current.y, target.z);

            //acabou o caminho
            if (i == paths.Count - 1)
            {
                finishedPath = true;
                animator.SetBool("End", true);
            }
        }
        
        
    }
    
    public void GetHit(int damage)
    {
        if (finishedPath) return;
        
        
        if (hitPoint != null)
            Instantiate(hitParticle, hitPoint.position, Quaternion.identity, hitPoint);
        else
            Instantiate(hitParticle, transform.position, Quaternion.identity);
        
        damageTaken += damage;
        
        StartCoroutine(HitIndicator());
        
        PointsUI.instance.UpdateText(damageTaken);
    }
    
    private IEnumerator HitIndicator()
{
    if (materials == null || materials.Count == 0) yield break;

    // inicia uma coroutine por material; cada chamada gera um token único por material
    foreach (var mat in materials)
    {
        if (mat == null) continue;
        _nextHitToken++;
        int token = _nextHitToken;
        _hitTokens[mat] = token;
        //StartCoroutine(HitIndicatorForMaterial(mat, token));
    }

    yield break;

    IEnumerator HitIndicatorForMaterial(Material material, int token)
    {
        if (material == null) yield break;

        if (!material.IsKeywordEnabled("_EMISSION")) material.EnableKeyword("_EMISSION");

        Color baseEmission = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : material.color;
        
        float minIntensity = 1f;
        float maxIntensity = 9f;
        float upDuration = 0.08f;   // rápido subida
        float downDuration = 0.25f; // queda um pouco mais lenta

        // subida
        float t = 0f;
        while (t < upDuration)
        {
            // se token mudou, aborta subida e faz fade até min
            if (_hitTokens.TryGetValue(material, out int current) && current != token)
            {
                yield return StartCoroutine(FadeToMin(material, baseEmission));
                yield break;
            }

            float intensity = Mathf.Lerp(minIntensity, maxIntensity, t / upDuration);
            material.SetColor("_EmissionColor", baseEmission * intensity);
            t += Time.deltaTime;
            yield return null;
        }

        // se token mudou após subida, garante fade até min
        if (_hitTokens.TryGetValue(material, out int currentAfterUp) && currentAfterUp != token)
        {
            yield return StartCoroutine(FadeToMin(material, baseEmission));
            yield break;
        }

        material.SetColor("_EmissionColor", baseEmission * maxIntensity);

        // descida
        t = 0f;
        while (t < downDuration)
        {
            if (_hitTokens.TryGetValue(material, out int currentDown) && currentDown != token)
            {
                yield return StartCoroutine(FadeToMin(material, baseEmission));
                yield break;
            }

            float intensity = Mathf.Lerp(maxIntensity, minIntensity, t / downDuration);
            material.SetColor("_EmissionColor", baseEmission * intensity);
            t += Time.deltaTime;
            yield return null;
        }

        material.SetColor("_EmissionColor", baseEmission * minIntensity);
        yield break;
    }

    IEnumerator FadeToMin(Material material, Color baseEmission)
    {
        if (material == null) yield break;

        Color currentColor = material.HasProperty("_EmissionColor") ? material.GetColor("_EmissionColor") : material.color;
        float duration = 0.12f;
        float t = 0f;
        while (t < duration)
        {
            float lerp = t / duration;
            material.SetColor("_EmissionColor", Color.Lerp(currentColor, baseEmission * 1f, lerp));
            t += Time.deltaTime;
            yield return null;
        }
        material.SetColor("_EmissionColor", baseEmission * 1f);
        yield break;
    }
}
}