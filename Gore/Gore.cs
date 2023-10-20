using System;
using UnityEngine;
using CementTools;
using HarmonyLib;
using Femur;
using System.IO;
using System.Reflection;
using CementTools.Modules.PoolingModule;
using System.Collections;

// This is an example Mod class
public class Gore : CementMod
{
    public static float cachedVelocityThreshold;
    private static GameObject bloodParticlePrefab;
    private static float particleDuration;

    public void Start()
    {
        AssetBundle goreAssetBundle = AssetBundle.LoadFromFile(Path.Combine(modDirectoryPath, "gore"));
        bloodParticlePrefab = goreAssetBundle.LoadAsset<GameObject>("Blood Particles");
        goreAssetBundle.Unload(false);

        particleDuration = bloodParticlePrefab.GetComponent<ParticleSystem>().main.duration;

        Pool.RegisterPrefab(bloodParticlePrefab, ResetBlood);

        Harmony harmony = new Harmony("com.dotpy.gore");
        harmony.PatchAll();

        MethodInfo onCollisionInfo = typeof(CollisionHandeler).GetMethod("OnCollisionEnter", BindingFlags.NonPublic | BindingFlags.Instance);
        HarmonyMethod patch = new HarmonyMethod(typeof(Gore).GetMethod("Prefix", BindingFlags.Static | BindingFlags.Public));
        harmony.Patch(onCollisionInfo, prefix: patch);

        CacheVelocityThreshold();
    }

    private void CacheVelocityThreshold()
    {
        cachedVelocityThreshold = float.Parse(modFile.GetString("VelocityThreshold"));
    }

    public void ResetBlood(GameObject bloodParticles)
    {
        ParticleSystem particles = bloodParticles.GetComponent<ParticleSystem>();
        particles.Clear();
        particles.Play();
    }

    private static IEnumerator PoolBlood(GameObject gameObject, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        Pool.PoolObject(gameObject);
    }

    public static void Prefix(CollisionHandeler __instance, Collision collision)
    {
        CollisionHandeler other = collision.gameObject.GetComponentInParent<CollisionHandeler>();
        if (other != null && __instance.actor != other.actor)
        {
            Vector3 relativeVelocity = collision.relativeVelocity;
            float magnitude = relativeVelocity.magnitude;

            if (magnitude < cachedVelocityThreshold)
            {
                return;
            }
            ContactPoint contact = collision.contacts[0];
            GameObject bloodParticles = Pool.Instantiate
            (
                Gore.bloodParticlePrefab,
                contact.point,
                Quaternion.FromToRotation(Vector3.up, contact.normal)
            );
            CementModSingleton.Get<Gore>().StartCoroutine(PoolBlood(bloodParticles, particleDuration));
        }
    }
}
