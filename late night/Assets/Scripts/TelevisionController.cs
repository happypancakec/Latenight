using UnityEngine;

public class TelevisionController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private CouchInteraction couchInteraction;

    [Tooltip(
        "Drag the TV object named Emissor here."
    )]
    [SerializeField]
    private Renderer emissorRenderer;

    [Header("Screen Materials")]
    [Tooltip(
        "Material used when the television is off."
    )]
    [SerializeField]
    private Material televisionOffMaterial;

    [Tooltip(
        "Add one material for each channel."
    )]
    [SerializeField]
    private Material[] channelMaterials;

    [Tooltip(
        "Usually 0. Change this only if Emissor " +
        "uses multiple material slots."
    )]
    [SerializeField, Min(0)]
    private int screenMaterialIndex;

    [Header("Starting State")]
    [SerializeField]
    private bool televisionStartsOn;

    [SerializeField, Min(0)]
    private int startingChannel;

    [Header("Controls")]
    [SerializeField]
    private KeyCode changeChannelKey =
        KeyCode.Mouse0;

    [SerializeField]
    private KeyCode powerKey =
        KeyCode.Mouse1;

    [Header("Optional Audio")]
    [SerializeField]
    private AudioSource televisionAudioSource;

    [SerializeField]
    private AudioClip powerOnSound;

    [SerializeField]
    private AudioClip powerOffSound;

    [SerializeField]
    private AudioClip channelChangeSound;

    private const string RemoteSaveKey =
        "HasTVRemote";

    private bool televisionIsOn;
    private int currentChannel;

    public bool TelevisionIsOn =>
        televisionIsOn;

    public int CurrentChannel =>
        currentChannel;

    private void Start()
    {
        if (
            channelMaterials != null &&
            channelMaterials.Length > 0
        )
        {
            currentChannel = Mathf.Clamp(
                startingChannel,
                0,
                channelMaterials.Length - 1
            );
        }
        else
        {
            currentChannel = 0;
        }

        televisionIsOn =
            televisionStartsOn;

        RefreshScreenMaterial();
    }

    private void Update()
    {
        if (
            couchInteraction == null ||
            !couchInteraction.IsSeated
        )
        {
            return;
        }

        if (!PlayerHasRemote())
        {
            return;
        }

        if (Input.GetKeyDown(powerKey))
        {
            TogglePower();
            return;
        }

        if (Input.GetKeyDown(changeChannelKey))
        {
            ChangeChannel();
        }
    }

    private bool PlayerHasRemote()
    {
        return PlayerPrefs.GetInt(
            RemoteSaveKey,
            0
        ) == 1;
    }

    private void TogglePower()
    {
        televisionIsOn =
            !televisionIsOn;

        RefreshScreenMaterial();

        if (televisionIsOn)
        {
            PlaySound(powerOnSound);
        }
        else
        {
            PlaySound(powerOffSound);
        }

        Debug.Log(
            televisionIsOn
                ? "The television was turned on."
                : "The television was turned off."
        );
    }

    private void ChangeChannel()
    {
        if (
            channelMaterials == null ||
            channelMaterials.Length == 0
        )
        {
            Debug.LogWarning(
                "TelevisionController has no " +
                "channel materials assigned."
            );

            return;
        }

        if (!televisionIsOn)
        {
            televisionIsOn = true;
            currentChannel = 0;

            RefreshScreenMaterial();
            PlaySound(powerOnSound);

            Debug.Log(
                "The television was turned on " +
                "to channel 1."
            );

            return;
        }

        currentChannel++;

        if (
            currentChannel >=
            channelMaterials.Length
        )
        {
            currentChannel = 0;
        }

        RefreshScreenMaterial();
        PlaySound(channelChangeSound);

        Debug.Log(
            $"Changed television to channel " +
            $"{currentChannel + 1}."
        );
    }

    private void RefreshScreenMaterial()
    {
        if (emissorRenderer == null)
        {
            Debug.LogWarning(
                "The TV Emissor Renderer has not " +
                "been assigned."
            );

            return;
        }

        Material selectedMaterial;

        if (!televisionIsOn)
        {
            selectedMaterial =
                televisionOffMaterial;
        }
        else if (
            channelMaterials != null &&
            channelMaterials.Length > 0
        )
        {
            currentChannel = Mathf.Clamp(
                currentChannel,
                0,
                channelMaterials.Length - 1
            );

            selectedMaterial =
                channelMaterials[currentChannel];
        }
        else
        {
            selectedMaterial =
                televisionOffMaterial;
        }

        if (selectedMaterial == null)
        {
            return;
        }

        Material[] materials =
            emissorRenderer.sharedMaterials;

        if (
            screenMaterialIndex < 0 ||
            screenMaterialIndex >=
            materials.Length
        )
        {
            Debug.LogError(
                "TV Screen Material Index is outside " +
                "the Emissor material array. Try 0."
            );

            return;
        }

        materials[screenMaterialIndex] =
            selectedMaterial;

        emissorRenderer.sharedMaterials =
            materials;
    }

    private void PlaySound(AudioClip sound)
    {
        if (
            televisionAudioSource == null ||
            sound == null
        )
        {
            return;
        }

        televisionAudioSource.PlayOneShot(
            sound
        );
    }
}
