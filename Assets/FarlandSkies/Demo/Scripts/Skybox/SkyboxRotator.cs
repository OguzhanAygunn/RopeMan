using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    public float RotationPerSecond = 1;
    [SerializeField] bool active;

    protected void Update()
    {
        if (active) RenderSettings.skybox.SetFloat("_Rotation", Time.time * RotationPerSecond);
    }

    public void ToggleSkyboxRotation()
    {
        active = !active;
    }
}