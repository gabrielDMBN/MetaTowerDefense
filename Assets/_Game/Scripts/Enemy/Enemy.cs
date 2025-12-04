using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float timePerTile = 1f;
    [SerializeField] private List<EPath> paths = new List<EPath>();

    void Start()
    {
        if (paths == null || paths.Count == 0) return;
        StartCoroutine(WalkPath());
    }

    private IEnumerator WalkPath()
    {
        for (int i = 0; i < paths.Count; i++)
        {
            yield return new WaitForSeconds(timePerTile);

            Vector3 target = paths[i].transform.position;
            Vector3 current = transform.position;
            transform.position = new Vector3(target.x, current.y, target.z);
        }
    }
}