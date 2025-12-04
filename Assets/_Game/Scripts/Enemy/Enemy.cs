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
    
    private bool finishedPath = false;
    private int damageTaken = 0;

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

        PointsUI.instance.UpdateText(damageTaken);
    }
}