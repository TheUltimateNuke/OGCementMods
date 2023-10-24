using UnityEngine;
using UnityEngine.InputSystem;
using CementTools.Modules.InputModule;
using CementTools;
using System.Collections;
using UnityEngine.SceneManagement;
using Femur;

// Token: 0x02000005 RID: 5
public class CinematicMod : CementMod
{
    private bool locked;
    private GameObject virtualCamera;

    private GameObject UI;
    private Vector3 cameraPosition;
    private Quaternion cameraRotation;

    private GameObject camera;

    private bool camenabler;

    private float speed = 25f;

    private float mouseSensitivity = 10f;

    private bool usingCustomCam;

    public CinematicKeybindManager keybindManager;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneChanged;
        SceneManager.sceneUnloaded += OnSceneUnload;

        modFile.ChangedValues += Rebind;
        
        StartCoroutine(WaitToStart());
    }


    public IEnumerator WaitToStart()
    {
        yield return new WaitUntil(() => SceneManager.GetActiveScene().name == "Menu");
        camera = Camera.main.gameObject;
        keybindManager = new CinematicKeybindManager(modFile);
        
        keybindManager.BindForward(MoveForward);
        keybindManager.BindBackward(MoveBackward);
        keybindManager.BindRight(MoveRight);
        keybindManager.BindLeft(MoveLeft);
        keybindManager.BindUp(MoveUp);
        keybindManager.BindDown(MoveDown);
        keybindManager.BindToggleCam(ToggleCam, false);
        keybindManager.BindToggleLock(ToggleLock, false);
        keybindManager.BindToggleKillVolumes(CementModSingleton.Get<RemoveKillVolume>().ToggleKillVolumes, false);
    }

    public void Rebind()
    {
        keybindManager.BindForward(MoveForward);
        keybindManager.BindBackward(MoveBackward);
        keybindManager.BindRight(MoveRight);
        keybindManager.BindLeft(MoveLeft);
        keybindManager.BindUp(MoveUp);
        keybindManager.BindDown(MoveDown);
        keybindManager.BindToggleCam(ToggleCam, false);
        keybindManager.BindToggleLock(ToggleLock, false);
        keybindManager.BindToggleKillVolumes(CementModSingleton.Get<RemoveKillVolume>().ToggleKillVolumes, false);
    }

    private void OnSceneUnload(Scene _)
    {
        cameraPosition = camera.transform.position;
        cameraRotation = camera.transform.rotation;
    }

    private void OnSceneChanged(Scene _, LoadSceneMode __)
    {
        StartCoroutine(WaitToAdjustCamera());
    }

    private IEnumerator WaitToAdjustCamera()
    {
        yield return new WaitUntil(() => virtualCamera != null);
        yield return new WaitUntil(() => UI != null);
        if (usingCustomCam)
        {
            EnableCustomCamera();
        }
    }

    private void EnableCustomCamera()
    {
        virtualCamera.SetActive(false);
        UI.SetActive(false);
        camera.transform.position = cameraPosition;
        camera.transform.rotation = cameraRotation;
    }

    private void DisableCustomCamera()
    {
        virtualCamera.SetActive(true);
        UI.SetActive(true);
        cameraPosition = camera.transform.position;
        cameraRotation = camera.transform.rotation;
    }

    private void MoveForward()
    {
        if (!usingCustomCam || locked)
        {
            return;
        }

        camera.transform.position += speed * Time.deltaTime * camera.transform.forward;
    }

    private void MoveUp()
    {
        if (!usingCustomCam || locked)
        {
            return;
        }

        camera.transform.position += speed * Time.deltaTime * Vector3.up;
    }

    private void MoveDown()
    {
        if (!usingCustomCam || locked)
        {
            return;
        }

        camera.transform.position -= speed * Time.deltaTime * Vector3.up;
    }

    private void MoveBackward()
    {
        if (!usingCustomCam || locked)
        {
            return;
        }

        camera.transform.position -= speed * Time.deltaTime * camera.transform.forward;
    }

    private void MoveRight()
    {
        if (!usingCustomCam || locked)
        {
            return;
        }

        camera.transform.position += speed * Time.deltaTime * camera.transform.right;
    }

    private void MoveLeft()
    {
        if (!usingCustomCam || locked)
        {
            return;
        }

        camera.transform.position -= speed * Time.deltaTime * camera.transform.right;
    }

    private void ToggleLock()
    {
        locked = !locked;
    }

    private void ToggleCam()
    {
        usingCustomCam = !usingCustomCam;
        if (usingCustomCam)
        {
            EnableCustomCamera();
        }
        else
        {
            DisableCustomCamera();
        }   
    }

    public void Update()
    {
        if (virtualCamera == null)
        {
            virtualCamera = GameObject.Find("VirtualCamera");
        }

        if (UI == null)
        {
            UI = GameObject.Find("UI");
        }

        if (keybindManager == null)
        {
            return;
        }

        keybindManager.CheckInputs();

        if (!usingCustomCam || locked)
        {
            return;
        }

        if (Mouse.current.rightButton.isPressed)
        {
            float mouseX = Mouse.current.delta.x.ReadValue();
            float mouseY = -Mouse.current.delta.y.ReadValue();

            float newYRot = camera.transform.eulerAngles.y + mouseX * mouseSensitivity * Time.deltaTime;
            float xRot = camera.transform.eulerAngles.x;
            if (xRot > 180)
            {
                xRot -= 360;
            }

            float newXRot = Mathf.Clamp(xRot + mouseY * mouseSensitivity * Time.deltaTime, -90f, 90f);

            camera.transform.eulerAngles = new Vector3(newXRot, newYRot, 0);
        }
    }

    private void LateUpdate()
    {
        if (usingCustomCam)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    
}