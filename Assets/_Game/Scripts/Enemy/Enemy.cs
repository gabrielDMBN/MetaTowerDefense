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

    // nova: cor base original da emissão de cada material
    private Dictionary<Material, Color> _baseEmissionColors = new Dictionary<Material, Color>();
    
    void Awake()
    {
        //pega todos os materiais do inimigo
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        materials = new List<Material>();
        foreach (var renderer in renderers)
        {
            materials.AddRange(renderer.materials);
        }

        // guarda a cor de emissao original de cada material
        foreach (var mat in materials)
        {
            if (mat == null) continue;

            if (!mat.IsKeywordEnabled("_EMISSION"))
                mat.EnableKeyword("_EMISSION");

            Color baseEmission;

            if (mat.HasProperty("_EmissionColor"))
                baseEmission = mat.GetColor("_EmissionColor");
            else
                baseEmission = mat.color;

            _baseEmissionColors[mat] = baseEmission;
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
                PointsUI.instance.MoveToCenter();
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

            // AGORA DE FATO DISPARA A CORROTINA POR MATERIAL
            StartCoroutine(HitIndicatorForMaterial(mat, token));
        }

        yield break;
    }

    private IEnumerator HitIndicatorForMaterial(Material material, int token)
    {
        if (material == null) yield break;

        if (!material.IsKeywordEnabled("_EMISSION"))
            material.EnableKeyword("_EMISSION");

        // usa SEMPRE a cor base salva, não a cor atual
        if (!_baseEmissionColors.TryGetValue(material, out var baseEmission))
        {
            baseEmission = material.HasProperty("_EmissionColor") 
                ? material.GetColor("_EmissionColor") 
                : material.color;
        }

        float minIntensity = 1f;
        float maxIntensity = 15f;
        float upDuration = 0.08f;   // subida rápida
        float downDuration = 0.25f; // descida um pouco mais lenta

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

        // checagem pós-subida
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

        // garante que voltou exatamente pro “1x”
        material.SetColor("_EmissionColor", baseEmission * minIntensity);
    }

    private IEnumerator FadeToMin(Material material, Color baseEmission)
    {
        if (material == null) yield break;

        Color currentColor = material.HasProperty("_EmissionColor")
            ? material.GetColor("_EmissionColor")
            : material.color;

        float duration = 0.12f;
        float t = 0f;
        while (t < duration)
        {
            float lerp = t / duration;
            Color target = baseEmission * 1f; // intensidade mínima fixa
            material.SetColor("_EmissionColor", Color.Lerp(currentColor, target, lerp));

            t += Time.deltaTime;
            yield return null;
        }

        material.SetColor("_EmissionColor", baseEmission * 1f);
    }
}