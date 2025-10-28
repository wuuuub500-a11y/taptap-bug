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

    [field: Header("Sound Effects - Chapter 3")]
    [field: SerializeField] public EventReference Notification { get; private set; }
    [field: SerializeField] public EventReference Boss_Voice { get; private set; }
    [field: SerializeField] public EventReference Count_Down { get; private set; }
    [field: SerializeField] public EventReference Get_File_Success { get; private set; }
    [field: SerializeField] public EventReference Get_File_Fail { get; private set; }
    [field: SerializeField] public EventReference Insect_Moving { get; private set; }
    [field: SerializeField] public EventReference Light_Static { get; private set; }

    [field: Header("Music")]
    [field: SerializeField] public EventReference MUSIC_Chapter_0 { get; private set; }
    [field: SerializeField] public EventReference MUSIC_Chapter_1{ get; private set; }
    [field: SerializeField] public EventReference MUSIC_Chapter_2 { get; private set; }
    [field: SerializeField] public EventReference MUSIC_Chapter_3 { get; private set; }
    
    [field: Header("News Sound Effects")]
    [field: SerializeField] public EventReference NEWS_Chapter_0 { get; private set; }
    [field: SerializeField] public EventReference NEWS_Bad_Ending { get; private set; }
    [field: SerializeField] public EventReference NEWS_Happy_Ending { get; private set; }
    [field: SerializeField] public EventReference NEWS_Happy_Ending_Special { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }
}
