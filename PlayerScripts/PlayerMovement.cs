using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    [SerializeField] private float moveSpeed = CONSTANTS.PLAYER_MOVE_SPEED;
    [SerializeField] private float gravity = CONSTANTS.GRAVITY;
    [SerializeField] private CharacterController controller;
    private Vector2 moveInput;
    private Vector2 mouseDelta;
    private Vector3 velocity;

    [SerializeField] private float sprintSpeedMultiplier = 1.5f;
    /// <summary>
    ///     Player camera rotation variables
    /// </summary>
    private float xRotation = 0f;
    private float topClamp = 90.0f;
    private float bottomClamp = -90.0f;

    [SerializeField] private float mouseSensitivity = 0.1f;

    [SerializeField] private Camera playerCamera;

    [Header("Raycast Detection")]
    [SerializeField] private float raycastDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;
    private GameObject currentInteractable;

    [Header("UI State")]
    private bool canOpenInventory = false;
    private bool inventoryUnlocked = false;
    private bool movementLocked = false;
    private bool cameraLocked = false;

    /// <summary>
    ///     Keeps reference of last interacted door
    ///     Used for closing the last interacted door when the player interacts with a different door to prevent multiple doors (portals) being open at the same time
    /// </summary>
    private PortalDoorComponent lastInteractedDoor = null;

    [SerializeField] private Door EndingDoor;

    void Update()
    {
        if (!cameraLocked)
        {
            transform.Rotate(Vector3.up * mouseDelta.x * mouseSensitivity);

            xRotation -= mouseDelta.y * mouseSensitivity;
            xRotation = Mathf.Clamp(xRotation, bottomClamp, topClamp);
            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        }

        DetectInteractableWithRaycast();
    }

    private void DetectInteractableWithRaycast()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * raycastDistance, Color.yellow);

        if (Physics.Raycast(ray, out hit, raycastDistance, interactableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            if (hitObject.CompareTag("Door") ||
                hitObject.CompareTag("PortalDoor") ||
                hitObject.CompareTag("LockedDoor") ||
                hitObject.CompareTag("StoryLockedDoor") ||
                hitObject.CompareTag("Pickupable") ||
                hitObject.CompareTag("Book") ||
                hitObject.CompareTag("Dialogue") ||
                hitObject.CompareTag("InteractableItem") ||
                hitObject.CompareTag("RitualPaper") ||
                hitObject.CompareTag("BadEnding") ||
                hitObject.CompareTag("HappyEnding"))
            {
                if (currentInteractable != hitObject)
                    currentInteractable = hitObject;


                UserInterface.Instance.ShowInteractPrompt(BuildPromptFor(hitObject));
            }
            else
            {
                ClearCurrentInteractable();
            }
        }
        else
        {
            ClearCurrentInteractable();
        }
    }

    private string BuildPromptFor(GameObject hitObject)
    {
        if (hitObject == null) return "Interact [F]";

        if (hitObject.TryGetComponent(out CustomInteractPrompt custom) && custom.HasPrompt)
            return custom.Prompt;

        if (hitObject.CompareTag("Door"))
        {
            if (hitObject.TryGetComponent(out PhysicalDoor door))
                return door.IsOpen ? "Close [F]" : "Open [F]";

            return "Open [F]";
        }

        if (hitObject.CompareTag("LockedDoor"))
            return "X Bound Door X";

        if (hitObject.CompareTag("StoryLockedDoor"))
            return "X Locked Door X";

        if (hitObject.CompareTag("PortalDoor"))
            return "Use Door [F]";

        if (hitObject.CompareTag("Pickupable"))
            return "Pick Up [F]";

        if (hitObject.CompareTag("Book"))
            return "Pick Up Book [F]";

        if (hitObject.CompareTag("Dialogue"))
            return "Talk [F]";

        if (hitObject.CompareTag("InteractableItem"))
            return "Interact [F]";

        if (hitObject.CompareTag("RitualPaper"))
            return "Perform Ritual [F]";

        if (hitObject.CompareTag("BadEnding"))
            return "Select [F]";

        if (hitObject.CompareTag("HappyEnding"))
            return "Select [F]";

        return "Interact [F]";
    }

    public void ClearCurrentInteractable()
    {
        currentInteractable = null;

        UserInterface.Instance.HideInteractPrompt();
    }

    void FixedUpdate()
    {
        if (controller == null || !controller.enabled) return;

        Vector3 horizontalVelocity = Vector3.zero;

        if (!movementLocked)
        {
            Vector3 cameraForward = transform.forward;
            Vector3 forwardDirection = Vector3.ProjectOnPlane(cameraForward, Vector3.up).normalized;
            Vector3 rightDirection = transform.right;

            Vector3 desiredMove = (forwardDirection * moveInput.y) + (rightDirection * moveInput.x);
            desiredMove = desiredMove.normalized;

            horizontalVelocity = desiredMove * moveSpeed;
        }

        if (!controller.isGrounded)
            velocity.y += gravity * Time.fixedDeltaTime;
        else if (velocity.y < 0f)
            velocity.y = -2f;

        Vector3 finalMove = (horizontalVelocity * Time.fixedDeltaTime) + (velocity * Time.fixedDeltaTime);
        controller.Move(finalMove);

        if (DialogueManager.Instance.IsDialogueRunning())
        {
            DisableMovement();
        }
        else
        {
            EnableMovement();
        }
    }

    public void OnMove(InputAction.CallbackContext value)
    {
        if (movementLocked)
        {
            moveInput = Vector2.zero;
            return;
        }

        moveInput = value.ReadValue<Vector2>();
        AudioManager.Instance.PlayWithVariation("Walk");
    }

    public void OnSprint(InputAction.CallbackContext value)
    {
        if (value.performed)
        {
            moveSpeed = CONSTANTS.PLAYER_MOVE_SPEED * sprintSpeedMultiplier;
        }
        else if (value.canceled)
        {
            moveSpeed = CONSTANTS.PLAYER_MOVE_SPEED;
        }
    }

    public void OnMouseRotation(InputAction.CallbackContext value)
    {
        if (Cursor.visible)
        {
            mouseDelta = Vector2.zero;
            return;
        }

        mouseDelta = value.ReadValue<Vector2>();
    }

    public void OnOpenInventory(InputAction.CallbackContext value)
    {
        if (movementLocked) return;

        if (value.performed && canOpenInventory)
        {
            PlayerControllerToggle(UserInterface.Instance.ToggleInventory());
            ClearCurrentInteractable();
        }
    }

    public void SetCanOpenInventory(bool canOpen)
    {
        canOpenInventory = canOpen;
        Debug.Log($"Inventory access: {(canOpen ? "Enabled" : "Disabled")}");
    }

    public void UnlockInventory()
    {
        inventoryUnlocked = true;
        canOpenInventory = true;
        Debug.Log("<color=blue>Inventory system unlocked</color>");
    }

    public void OnInteract(InputAction.CallbackContext value)
    {
        if (movementLocked) return;

        if (value.performed && currentInteractable != null)
        {
            if (currentInteractable.CompareTag("PortalDoor"))
            {
                PortalDoorComponent portalDoor = currentInteractable.GetComponent<PortalDoorComponent>();
                if (portalDoor.DoorComponent.IsAnimating) return;

                StartCoroutine(portalDoor.DoorComponent.TogglePortalDoor(portalDoor));

                // Close last interacted door and set new interacted door
                if (lastInteractedDoor != null && lastInteractedDoor != portalDoor)
                {
                    if (lastInteractedDoor.DoorComponent.IsOpen)
                    {
                        StartCoroutine(lastInteractedDoor.DoorComponent.ForceCloseDoor(lastInteractedDoor));
                        if (lastInteractedDoor.ParentDoor.TargetDoor != null) {
                            lastInteractedDoor.DeactivatePortal();
                        }
                    }
                }

                lastInteractedDoor = portalDoor;
            }
            else if (currentInteractable.CompareTag("Door"))
            {
                PhysicalDoor doorComponent = currentInteractable.GetComponent<PhysicalDoor>();
                if (doorComponent.IsAnimating) return;

                StartCoroutine(doorComponent.ToggleDoor());
            }
            else if (currentInteractable.CompareTag("Book"))
            {
                UnlockInventory();
                Destroy(currentInteractable);
            }
            else if (currentInteractable.CompareTag("Pickupable"))
            {
                ItemObject pickupable = currentInteractable.GetComponent<ItemObject>();

                if (!inventoryUnlocked)
                {
                    Debug.Log("You need to find the book first!");
                    return;
                }

                AudioManager.Instance.Play("Pickup");

                if (pickupable.Item.ItemName == "Canvas with eyes")
                {
                    GameManager.Instance.MixUpRooms();
                }

                pickupable.PickupItem();

                ClearCurrentInteractable();
            }
            else if (currentInteractable.CompareTag("InteractableItem"))
            {
                InteractableItem interactable = currentInteractable.GetComponent<InteractableItem>();
                GameManager.Instance.InteractedItem = interactable;
                GameManager.Instance.NextDoor = interactable.LinkedDoor;
                UserInterface.Instance.OpenEasel(interactable.CorrectItemName);
                PlayerControllerToggle(false);
                ClearCurrentInteractable();
            }
            else if (currentInteractable.CompareTag("Dialogue"))
            {
                Dialogue dialogue = currentInteractable.GetComponent<Dialogue>();
                if (dialogue != null)
                    dialogue.TriggerDialogue();
            }
            else if (currentInteractable.CompareTag("RitualPaper"))
            {
                RitualPaperItemObject paper = currentInteractable.GetComponent<RitualPaperItemObject>();
                if (paper != null)
                {
                    paper.Interact();
                    PlayerControllerToggle(false);
                    ClearCurrentInteractable();
                }
            }
            else if (currentInteractable.CompareTag("BadEnding"))
            {
                EndingDoor.TargetDoor = GameManager.Instance.BadEndingDoor;
                StartCoroutine(EndingDoor.PortalDoorComponent.DoorComponent.TogglePortalDoor(EndingDoor.PortalDoorComponent));
                GameManager.Instance.DisableEndingStuff();
            }
            else if (currentInteractable.CompareTag("HappyEnding"))
            {
                EndingDoor.TargetDoor = GameManager.Instance.HappyEndingDoor;
                StartCoroutine(EndingDoor.PortalDoorComponent.DoorComponent.TogglePortalDoor(EndingDoor.PortalDoorComponent));
                GameManager.Instance.DisableEndingStuff();
            }
        }
    }

    public void Warp()
    {
        if (RoomTraversalManager.inPortal == null || RoomTraversalManager.outPortal == null) return;
        if (controller != null) controller.enabled = false;

        // If the player enters the BE_Door
        if (RoomTraversalManager.outPortal.PortalDoor.ParentDoor.DoorName == "BE_Door")
        {
            Debug.Log("BAD ENDING");
            SceneManager.LoadScene("Assets/Scenes/badending.unity");
            return;
        }
        // If the player enters the HE_Door
        else if (RoomTraversalManager.outPortal.PortalDoor.ParentDoor.DoorName == "HE_Door")
        {
            Debug.Log("GOOD ENDING");
            SceneManager.LoadScene("Assets/Scenes/happyending.unity");
            return;
        }

        var inTransform = RoomTraversalManager.inPortal.transform;
        var outTransform = RoomTraversalManager.outPortal.transform;

        Vector3 relativePos = inTransform.InverseTransformPoint(transform.position);
        relativePos = CONSTANTS.HALFTURN * relativePos;
        transform.position = outTransform.TransformPoint(relativePos);

        Quaternion relativeRot = Quaternion.Inverse(inTransform.rotation) * transform.rotation;
        relativeRot = CONSTANTS.HALFTURN * relativeRot;
        transform.rotation = outTransform.rotation * relativeRot;

        Vector3 relativeVel = inTransform.InverseTransformDirection(velocity);
        relativeVel = CONSTANTS.HALFTURN * relativeVel;
        velocity = outTransform.TransformDirection(relativeVel);

        transform.rotation = Quaternion.LookRotation(transform.forward, Vector3.up);

        if (controller != null) controller.enabled = true;

        Debug.Log("Out door position: " + outTransform.TransformPoint(relativePos));
        Debug.Log("Player position: " + transform.position);


        RoomTraversalManager.Instance.PlayerTraversedRoom();

        lastInteractedDoor = null;
    }

    public void PlayerControllerToggle(bool isEnabled)
    {
        if (controller != null) controller.enabled = isEnabled;
    }

    public float MovementSpeed => moveSpeed;

    /// <summary>
    ///     Teleports player to given position
    ///     Used for loading into checkpoint
    /// </summary>
    /// <param name="tpPosition"> Position to teleport to </param>
    public void TeleportPlayer(Vector3 tpPosition, Quaternion tpRotation = default)
    {
        if (controller != null) controller.enabled = false;
        transform.SetPositionAndRotation(tpPosition, tpRotation);
        if (controller != null) controller.enabled = true;
    }

    public void OnResetGame(InputAction.CallbackContext value)
    {
        if (!value.performed) return;

        var persistentRoot = GameObject.Find("PersistentObjects");
        if (persistentRoot != null) Destroy(persistentRoot);

        Debug.Log("Resetting game: reloading scene 0");
        SceneManager.LoadScene(0);
    }

    public void OnAdvanceDialogue(InputAction.CallbackContext value)
    {
        if (!value.performed) return;

        if (DialogueManager.Instance.IsDialogueRunning())
        {
            DialogueManager.Instance.RequestDialogueAdvance();
        }
    }

    public void OnEscPress(InputAction.CallbackContext value)
    {
        if (!value.started) return;
        if (UserInterface.Instance.IsSpellInfoPanelVisible) return;
        if (PauseMenuManager.Instance.IsHelpMenuVisible) return;
        PauseMenuManager.Instance.TogglePauseMenu();
    }

    public void DisableMovement()
    {
        movementLocked = true;
        moveInput = Vector2.zero;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        ClearCurrentInteractable();
    }

    public void EnableMovement()
    {
        movementLocked = false;
        if (UserInterface.Instance.IsSpellInfoPanelVisible) return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnSwitchSpellUse(InputAction.CallbackContext value)
    {
        if (!value.performed) return;
        if (Cursor.visible) return;

        UserInterface.Instance.CastSwitchSpell();
    }

    public void OnRotateSpellUse(InputAction.CallbackContext value)
    {
        if (!value.performed) return;
        if (Cursor.visible) return;
        if (currentInteractable == null || !currentInteractable.CompareTag("PortalDoor")) return;

        PortalDoorComponent portalDoor = currentInteractable.GetComponent<PortalDoorComponent>();
        UserInterface.Instance.CastRotateSpell(portalDoor);
    }

    public void OnFlashSpellUse(InputAction.CallbackContext value)
    {
        if (!value.performed) return;
        if (Cursor.visible) return;

        UserInterface.Instance.CastFlashSpell();
    }

    public void OnSave(InputAction.CallbackContext value)
    {
        if (!value.performed) return;

        GameManager.Instance.CheckpointSave(transform.position, transform.rotation);

        Debug.Log("<color=green>Game saved</color>");
    }

    public void OnLoad(InputAction.CallbackContext value)
    {
        if (!value.performed) return;

        GameManager.Instance.LoadCheckpoint(out Vector3 savedPlayerPos, out Quaternion savedPlayerRot);
        TeleportPlayer(savedPlayerPos, savedPlayerRot);
        Debug.Log("<color=green>Game loaded</color>");
    }

    /// <summary>
    ///     When player presses 1
    /// </summary>
    /// <param name="value"></param>
    public void OnSkipToLevel3(InputAction.CallbackContext value)
    {
        if (!value.performed) return;

        GameManager.Instance.CurrentLevel = CurrentLevel.LEVEL3;
        Debug.Log("Set current level to " + GameManager.Instance.CurrentLevel);

        GameManager.Instance.UnlockSpell(SpellType.SWITCH_ROOM, true);
        GameManager.Instance.UnlockSpell(SpellType.ROTATE_ROOM, true);
        GameManager.Instance.UnlockSpell(SpellType.FLASH, true);
        Debug.Log("Learned all spells");
    }

    /// <summary>
    ///     When player presses 2
    /// </summary>
    /// <param name="value"></param>
    public void OnWallTransition(InputAction.CallbackContext value)
    {
        if (!value.performed) return;
        WallTransitionManager.Instance.IncreaseDecreaseSpeed(1f);
    }

    /// <summary>
    ///     When player presses 3
    /// </summary>
    /// <param name="value"></param>
    public void OnSkipToEnding(InputAction.CallbackContext value)
    {
        if (!value.performed) return;
        RoomTraversalManager.Instance.EndingSetup();
    }

    /// <summary>
    ///     When player presses 4
    /// </summary>
    /// <param name="value"></param>
    public void ResetWallTransition(InputAction.CallbackContext value)
    {
        if (!value.performed) return;
        WallTransitionManager.Instance.ResetFloats();
    }

    public Camera GetCamera()
    {
        return playerCamera;
    }

    public void LockCamera()
    {
        cameraLocked = true;
        mouseDelta = Vector2.zero;
    }

    public void UnlockCamera()
    {
        cameraLocked = false;
    }
}