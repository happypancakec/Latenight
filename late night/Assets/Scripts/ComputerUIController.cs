using UnityEngine;

public class ComputerUIController : MonoBehaviour
{
    [Header("Application Windows")]
    [SerializeField] private GameObject emailWindow;
    [SerializeField] private GameObject suppliesWindow;
    [SerializeField] private GameObject residentsWindow;
    [SerializeField] private GameObject camerasWindow;
    [SerializeField] private GameObject saveWindow;

    [Header("Email")]
    [SerializeField] private EmailUIController emailUIController;

    [Header("Window Behavior")]
    [Tooltip(
        "Opening an application closes any other open window."
    )]
    [SerializeField] private bool onlyOneWindowAtATime = true;

    private ComputerInteraction computerInteraction;
    private bool exitingComputer;

    private void Awake()
    {
        CloseAllWindows();
    }

    private void Start()
    {
        if (emailUIController == null &&
            emailWindow != null)
        {
            emailUIController =
                emailWindow.GetComponent<EmailUIController>();
        }

        if (emailUIController != null)
        {
            emailUIController.RefreshNotificationBadge();
        }
    }

    private void Update()
    {
        if (exitingComputer)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (AnyWindowIsOpen())
            {
                CloseAllWindows();
            }
            else
            {
                ExitComputer();
            }
        }
    }

    public void Initialize(
        ComputerInteraction interaction
    )
    {
        computerInteraction = interaction;
        exitingComputer = false;

        CloseAllWindows();

        if (emailUIController == null &&
            emailWindow != null)
        {
            emailUIController =
                emailWindow.GetComponent<EmailUIController>();
        }

        if (emailUIController != null)
        {
            emailUIController.RefreshNotificationBadge();
        }
    }

    // -------------------------------------------------
    // Open application windows
    // -------------------------------------------------

    public void OpenEmail()
    {
        OpenWindow(emailWindow);
    }

    public void OpenSupplies()
    {
        OpenWindow(suppliesWindow);
    }

    public void OpenResidents()
    {
        OpenWindow(residentsWindow);
    }

    public void OpenCameras()
    {
        OpenWindow(camerasWindow);
    }

    public void OpenSave()
    {
        OpenWindow(saveWindow);
    }

    private void OpenWindow(GameObject selectedWindow)
    {
        if (selectedWindow == null)
        {
            Debug.LogError(
                "The selected computer window has not been assigned " +
                "on ComputerUIController attached to " +
                gameObject.name +
                "."
            );

            return;
        }

        if (onlyOneWindowAtATime)
        {
            CloseAllWindows();
        }

        selectedWindow.SetActive(true);
        selectedWindow.transform.SetAsLastSibling();
    }

    // -------------------------------------------------
    // Close individual application windows
    // -------------------------------------------------

    public void CloseEmail()
    {
        CloseWindow(emailWindow);
    }

    public void CloseSupplies()
    {
        CloseWindow(suppliesWindow);
    }

    public void CloseResidents()
    {
        CloseWindow(residentsWindow);
    }

    public void CloseCameras()
    {
        CloseWindow(camerasWindow);
    }

    public void CloseSave()
    {
        CloseWindow(saveWindow);
    }

    public void CloseAllWindows()
    {
        CloseWindow(emailWindow);
        CloseWindow(suppliesWindow);
        CloseWindow(residentsWindow);
        CloseWindow(camerasWindow);
        CloseWindow(saveWindow);
    }

    private void CloseWindow(GameObject window)
    {
        if (window != null)
        {
            window.SetActive(false);
        }
    }

    private bool AnyWindowIsOpen()
    {
        return
            IsWindowOpen(emailWindow) ||
            IsWindowOpen(suppliesWindow) ||
            IsWindowOpen(residentsWindow) ||
            IsWindowOpen(camerasWindow) ||
            IsWindowOpen(saveWindow);
    }

    private bool IsWindowOpen(GameObject window)
    {
        return
            window != null &&
            window.activeSelf;
    }

    // -------------------------------------------------
    // Exit the computer completely
    // -------------------------------------------------

    public void ExitComputer()
    {
        if (exitingComputer)
        {
            return;
        }

        Debug.Log("Exit Computer button clicked.");

        if (computerInteraction == null)
        {
            computerInteraction =
                FindAnyObjectByType<ComputerInteraction>();
        }

        if (computerInteraction == null)
        {
            Debug.LogError(
                "ComputerUIController could not find " +
                "ComputerInteraction in the gameplay scene. " +
                "Open the computer through the physical computer " +
                "instead of starting directly in ComputerScene."
            );

            return;
        }

        exitingComputer = true;
        CloseAllWindows();

        computerInteraction.CloseComputer();
    }
}