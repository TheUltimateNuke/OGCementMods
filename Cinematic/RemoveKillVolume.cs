using UnityEngine;
using UnityEngine.SceneManagement;
using CementTools.Modules.InputModule;
using CementTools;
using Femur;

public class RemoveKillVolume : CementMod
{
    bool disabled;
    GameObject[] killVolumes = new GameObject[0];

    public void Awake()
    {
        SceneManager.sceneLoaded += OnSceneChanged;
    }

    public void OnSceneChanged(Scene scene, LoadSceneMode _)
    {
        killVolumes = new GameObject[0];
        disabled = false;
    }

    public void ToggleKillVolumes()
    {
        if (killVolumes.Length == 0)
        {
            killVolumes = GameObject.FindGameObjectsWithTag("Helper (Kill Volume)");
        }

        if (disabled)
        {
            foreach (GameObject killVolume in killVolumes)
            {
                killVolume.SetActive(true);
            }
        }
        else
        {
            foreach (GameObject killVolume in killVolumes)
            {
                killVolume.SetActive(false);
            }
        }

        disabled = !disabled;
    }
}
