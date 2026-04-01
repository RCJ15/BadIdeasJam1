using System;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    [SerializeField] private MeshRenderer belt;

    [SerializeField] private float speed;

    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform spawn;
    [SerializeField] private float killX;
    [SerializeField] private float timeBtwSpawns;
    private float _timer;

    private List<Transform> _objects = new();

    private void Start()
    {
        float length = belt.transform.localScale.x;

        belt.material.mainTextureScale = new Vector4(length, 1);

        // Simulate 600 frames at 60 fps
        for (int i = 0; i < 600; i++)
        {
            Frame(1 / 60f);
        }
    }

    private void Update()
    {
        Vector2 offset = belt.material.mainTextureOffset;
        offset.x += speed * Time.deltaTime;
        offset.x %= 1f;
        belt.material.mainTextureOffset = offset;

        Frame(Time.deltaTime);
    }

    private void Frame(float deltaTime)
    {
        if (_timer <= 0)
        {
            Transform newSpawn = Instantiate(spawn, spawn.parent);
            newSpawn.gameObject.SetActive(true);
            newSpawn.localPosition = spawnPoint.localPosition;
            newSpawn.localRotation = spawn.localRotation;

            _objects.Add(newSpawn);

            _timer = timeBtwSpawns;
        }
        else
        {
            _timer -= deltaTime;
        }

        List<Transform> delete = new();

        foreach (Transform obj in _objects)
        {
            Vector3 localPosition = obj.localPosition;
            localPosition.x += speed * deltaTime;
            obj.localPosition = localPosition;

            if (speed > 0 ? localPosition.x > killX : localPosition.x < killX)
            {
                delete.Add(obj);
            }
        }

        foreach (var item in delete)
        {
            _objects.Remove(item);
            Destroy(item.gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (spawnPoint == null) return;

        Gizmos.color = Color.red;

        Matrix4x4 startMatrix = Gizmos.matrix;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.DrawWireSphere(spawnPoint.localPosition, 0.2f);
        Gizmos.DrawWireSphere(new(killX, spawnPoint.localPosition.y), 0.2f);

        Gizmos.matrix = startMatrix;
    }
}
