using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    public static FMODEvents Instance { get; private set; }

    [field: Header("Sound Effects - Chapter 1")]
    [field: SerializeField] public EventReference Plane_Fly_By { get; private set; }
    [field: SerializeField] public EventReference ComputerOperating { get; private set; }
    [field: SerializeField] public EventReference ComputerOperating_Glitch { get; private set; }
    [field: SerializeField] public EventReference Password_Correct { get; private set; }
    [field: SerializeField] public EventReference Password_Incorrect { get; private set; }
    [field: SerializeField] public EventReference News_Notification { get; private set; }
    [field: SerializeField] public EventReference Plastic_Folder { get; private set; }
    [field: SerializeField] public EventReference Typing_Password { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of FMODEvents detected! Handled.");
        }
    }
}
