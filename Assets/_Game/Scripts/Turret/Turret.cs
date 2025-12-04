using UnityEngine;

public class Turret : MonoBehaviour
{
    [Header("Ataque")]
    [SerializeField] private float range = 5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Partículas")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private Transform[] firePoints;

    [Header("Rotação")]
    [SerializeField] private GameObject turretHead;
    [SerializeField] private Transform turretJoint;
    [SerializeField] private float rotationLerpSpeed = 5f;

    [Header("Color")]
    [SerializeField] private Color GizmosColor = Color.gray;
    
    
    private float fireCountdown = 0f;
    private SphereCollider rangeTrigger;

    // Como só existe 1 inimigo, guardamos só um alvo
    private Enemy currentTarget;

    void Awake()
    {
        // Garante um SphereCollider como trigger de alcance
        rangeTrigger = GetComponent<SphereCollider>();
        if (rangeTrigger == null)
            rangeTrigger = gameObject.AddComponent<SphereCollider>();

        rangeTrigger.isTrigger = true;
        rangeTrigger.radius = range;
    }

    void OnValidate()
    {
        var sc = GetComponent<SphereCollider>();
        if (sc != null)
            sc.radius = range;
    }
    

    private void Update()
    {
        // se não tem alvo, não faz nada
        if (currentTarget == null) return;

        // gira a cabeça da torre
        RotateHeadTowards(currentTarget.transform.position);

        // countdown do tiro
        if (fireCountdown <= 0f)
        {
            ShootCurrentTarget();          // aqui você chama o método que já existe
            fireCountdown = 1f / fireRate; // ex.: fireRate = 1 -> 1 tiro por segundo
        }

        fireCountdown -= Time.deltaTime;
        
        //se apertar T, toma dano
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("Turret manual shoot");
            SpawnParticleEffect();
        }
    }

    private void RotateHeadTowards(Vector3 targetPos)
    {
        if (turretHead == null || turretJoint == null) return;

        // Direção do joint até o alvo
        Vector3 dir = targetPos - turretJoint.position;

        // Trava rotação só no plano horizontal (eixo Y)
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);

        // Lerp suave da rotação da cabeça
        turretHead.transform.rotation = Quaternion.Lerp(
            turretHead.transform.rotation,
            targetRot,
            rotationLerpSpeed * Time.deltaTime
        );
    }

    private void ShootCurrentTarget()
    {
        if (currentTarget == null || currentTarget.HasFinishedPath()) return;

        Debug.Log("Turret shooting enemy: " + currentTarget.gameObject.name);
        currentTarget.GetHit(damage);

        SpawnParticleEffect();
    }

    private void SpawnParticleEffect()
    {
        if (particlePrefab == null || firePoints == null) return;

        foreach (var point in firePoints)
        {
            if (point == null) continue;
            Instantiate(particlePrefab, point.position, point.rotation, point);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Layer mask
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;
    
        var enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null)
        {
            Debug.Log("Turret OnTriggerEnter: No Enemy component found in " + other.gameObject.name);
            return;
        }
    
        Debug.Log("Turret OnTriggerEnter: " + enemy.gameObject.name);
        currentTarget = enemy;

        // 🔥 garante que o primeiro tiro seja imediato
        fireCountdown = 0f;
    }

    private void OnTriggerExit(Collider other)
    {
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        var enemy = other.GetComponentInParent<Enemy>();
        if (enemy == null) return;

        Debug.Log("Turret OnTriggerExit: " + enemy.gameObject.name);

        if (currentTarget == enemy)
        {
            currentTarget = null;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = GizmosColor;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}
