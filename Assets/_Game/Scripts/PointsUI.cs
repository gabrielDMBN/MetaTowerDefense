using System;
using System.Collections;
using UnityEngine;

public class PointsUI : MonoBehaviour
{
    public static PointsUI instance;

    [SerializeField] private TMPro.TextMeshProUGUI pointsText;

    [Header("Animação")]
    [SerializeField] private float punchScaleMultiplier = 1.2f; // quanto o texto cresce
    [SerializeField] private float punchDuration = 0.25f;        // duração total da animação
    [SerializeField] private float maxRotationZ = 10f;           // rotação máxima em graus
    
    [SerializeField] private Material fontMaterial;
    [SerializeField] private float rainbowSpeed = 0.6f;

    private Vector3 _initialScale;
    private Quaternion _initialRotation;
    private Coroutine _punchRoutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // Se quiser que este objeto sobreviva de cena em cena:
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Guarda o estado inicial para sempre voltar pra ele
        _initialScale = transform.localScale;
        _initialRotation = transform.localRotation;

        pointsText.text = "0";
    }
    
    private void Start()
    {
        // Inicia a animação rainbow no contorno do texto
        StartCoroutine(RaimbowAnimation());
    }

    public void UpdateText(int points)
    {
        pointsText.text = points.ToString();

        // Sempre que atualizar o texto, reinicia a animação
        if (_punchRoutine != null)
        {
            StopCoroutine(_punchRoutine);
        }

        _punchRoutine = StartCoroutine(PunchAnimation());
    }

    private IEnumerator PunchAnimation()
    {
        float halfDuration = punchDuration * 0.5f;

        // Sorteia se gira para a esquerda ou direita (mas dentro do limite)
        float dir = UnityEngine.Random.value < 0.5f ? -1f : 1f;

        // Fase 1: cresce e gira
        float t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / halfDuration);

            float scaleFactor = Mathf.Lerp(1f, punchScaleMultiplier, normalized);
            // Garante que nunca passa do limite
            scaleFactor = Mathf.Min(scaleFactor, punchScaleMultiplier);

            float zRot = Mathf.Lerp(0f, maxRotationZ, normalized) * dir;

            transform.localScale = _initialScale * scaleFactor;
            transform.localRotation = _initialRotation * Quaternion.Euler(0f, 0f, zRot);

            yield return null;
        }

        // Fase 2: volta ao normal
        t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / halfDuration);

            float scaleFactor = Mathf.Lerp(punchScaleMultiplier, 1f, normalized);
            scaleFactor = Mathf.Clamp(scaleFactor, 1f, punchScaleMultiplier);

            float zRot = Mathf.Lerp(maxRotationZ, 0f, normalized) * dir;

            transform.localScale = _initialScale * scaleFactor;
            transform.localRotation = _initialRotation * Quaternion.Euler(0f, 0f, zRot);

            yield return null;
        }

        // Garante que termina exatamente no estado inicial
        transform.localScale = _initialScale;
        transform.localRotation = _initialRotation;

        _punchRoutine = null;
    }

    private IEnumerator RaimbowAnimation()
    {
        float hue = 0f;

        while (true)
        {
            // Incrementa o hue continuamente
            hue += Time.deltaTime * rainbowSpeed;

            // Mantém valor no intervalo [0,1]
            if (hue > 1f) hue -= 1f;

            // Converte HSV para RGB (saturação e valor no máximo)
            Color rainbowColor = Color.HSVToRGB(hue, 1f, 1f);

            // Aplica no OUTLINE do TextMeshPro
            fontMaterial.SetColor("_OutlineColor", rainbowColor);

            yield return null;
        }
    }
}
