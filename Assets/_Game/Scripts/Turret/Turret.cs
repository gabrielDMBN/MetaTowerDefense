using System.Collections.Generic;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] private float range = 5f;
    [SerializeField] private int damage = 10;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private List<Transform> firePoint;
    [SerializeField] private LayerMask enemyLayer;

    private float fireCountdown = 0f;
    private readonly HashSet<Collider> enemiesInRange = new HashSet<Collider>();
    private SphereCollider rangeTrigger;

    void Awake()
    {
        // garante que exista um SphereCollider configurado como trigger para detectar entradas/saídas
        rangeTrigger = GetComponent<SphereCollider>();
        if (rangeTrigger == null)
            rangeTrigger = gameObject.AddComponent<SphereCollider>();

        rangeTrigger.isTrigger = true;
        rangeTrigger.radius = range;
    }

    void OnValidate()
    {
        // mantém o raio do trigger sincronizado no editor
        var sc = GetComponent<SphereCollider>();
        if (sc != null)
            sc.radius = range;
    }

    void Update()
    {
        fireCountdown -= Time.deltaTime;
        if (fireCountdown <= 0f)
        {
            if (enemiesInRange.Count > 0)
            {
                Shoot();
                fireCountdown = 1f / fireRate;
            }
        }
    }

    private void Shoot()
    {
        // remove entradas inválidas
        enemiesInRange.RemoveWhere(c => c == null || c.gameObject == null);

        if (enemiesInRange.Count == 0) return;

        // escolhe o primeiro alvo válido (pode trocar para o mais próximo se desejar)
        Collider targetCollider = null;
        foreach (var c in enemiesInRange)
        {
            targetCollider = c;
            break;
        }

        if (targetCollider == null) return;

        Enemy enemy = targetCollider.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.GetHit(damage);
        }

        SpawnParticleEffect();
    }

    private void SpawnParticleEffect()
    {
        if (particlePrefab != null && firePoint != null)
        {
            foreach (var point in firePoint)
            {
                Instantiate(particlePrefab, point.position, point.rotation, point);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // verifica se a layer do objeto bate com a LayerMask
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;

        // adiciona somente se realmente tiver um collider (o próprio 'other' é um collider)
        enemiesInRange.Add(other);
    }

    private void OnTriggerExit(Collider other)
    {
        if ((enemyLayer.value & (1 << other.gameObject.layer)) == 0) return;
        enemiesInRange.Remove(other);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}