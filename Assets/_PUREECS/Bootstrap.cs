using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
    public static Bootstrap Instance;
    public static MeshInstanceRenderer MatterRenderer;

    [SerializeField]
    private MeshInstanceRenderer _matterRenderer;

    public float spawningRange = 50F;
    public float minMassRange = 0.5F;
    public float maxMassRange = 100F;
    public int SpawnNumber = 50;

    //public static EntityArchetype MatterObjectArchetype;

    public Vector3 mostmassiveObjectPos = Vector3.zero;
    public float mostmassiveObjectRadius = 1F;
    public float mostmassiveObjectMass = 1F;
    public Vector3 mostmassiveObjectVelocity = Vector3.zero;
    public Transform emmiter = null;

    public void SetupBiggest(Vector3 pos, float radius, float mass, Vector3 vel)
    {
        mostmassiveObjectPos = pos;
        mostmassiveObjectRadius = radius;
        mostmassiveObjectMass = mass;
        mostmassiveObjectVelocity = vel;
        if (emmiter != null)
        {
            emmiter.position = mostmassiveObjectPos;
            emmiter.localScale = Vector3.one * mostmassiveObjectRadius;
        }
    }

    private void Awake()
    {
        MatterRenderer = _matterRenderer;
        Instance = this;
    }

    public static void Initialize()
    {
        //var entityManager = World.Active.GetOrCreateManager<EntityManager>();

        //MatterObjectArchetype = entityManager.CreateArchetype(typeof(Position), typeof(MeshInstanceRenderer), typeof(Rotation), typeof(MatterObject));
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Initialize();
    }
}
