using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ComputerInteraction : MonoBehaviour, Interactable
{
    [Header("Scene")]
    [SerializeField]
    private string computerSceneName =
        "ComputerScene";

    [Header("Player")]
    [SerializeField] private FirstPersonController playerController;
    [SerializeField] private PlayerInteraction playerInteraction;

    private bool computerOpen;
    private bool changingScene;

    public void Interact()
    {
        if (!computerOpen && !changingScene)
        {
            StartCoroutine(OpenComputer());
        }
    }

    private IEnumerator OpenComputer()
    {
        changingScene = true;

        if (playerController != null)
        {
            playerController.enabled = false;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AsyncOperation operation =
            SceneManager.LoadSceneAsync(
                computerSceneName,
                LoadSceneMode.Additive
            );

        if (operation == null)
        {
            Debug.LogError(
                "ComputerScene could not be loaded."
            );

            RestorePlayerControls();
            changingScene = false;
            yield break;
        }

        yield return operation;

        computerOpen = true;
        changingScene = false;

        ComputerUIController computerUI =
            FindFirstObjectByType<ComputerUIController>();

        if (computerUI != null)
        {
            computerUI.Initialize(this);
        }
        else
        {
            Debug.LogError(
                "ComputerScene has no ComputerUIController."
            );
        }
    }

    public void CloseComputer()
    {
        if (computerOpen && !changingScene)
        {
            StartCoroutine(CloseComputerRoutine());
        }
    }

    private IEnumerator CloseComputerRoutine()
    {
        changingScene = true;

        AsyncOperation operation =
            SceneManager.UnloadSceneAsync(
                computerSceneName
            );

        if (operation != null)
        {
            yield return operation;
        }

        computerOpen = false;
        changingScene = false;

        RestorePlayerControls();
    }

    private void RestorePlayerControls()
    {
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}