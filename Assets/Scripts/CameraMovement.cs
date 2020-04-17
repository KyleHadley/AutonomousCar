#region Includes
using System;
using UnityEngine;
#endregion


/// <summary>
/// Component for simple camera target following.
/// </summary>
public class CameraMovement : MonoBehaviour
{
    #region Members
    // Distance of Camera to target in Z direction, to be set in Unity Editor.
    [SerializeField]
    private int CamZ = -15;
    // The initial Camera target, to be set in Unity Editor.
    [SerializeField]
    public GameObject CurrentTarget { get; private set; }
    // The speed of camera movement, to be set in Unity Editor.
    [SerializeField]
    private float CamSpeed = 5f;
    // The speed of camera movement when reacting to user input, to be set in Unity Editor.
    [SerializeField]
    private float UserInputSpeed = 50f;
    // Whether the camera can be controlled by user input, to be set in Unity Editor.
    [SerializeField]
    private bool AllowUserInput;

    private Vector3 _startPosition;

    /// <summary>
    /// The bounds the camera may move in.
    /// </summary>
    public RectTransform MovementBounds
    {
        get;
        set;
    }

    private Vector3 targetCamPos;
    #endregion

    #region Methods
    /// <summary>
    /// Sets the target to follow.
    /// </summary>
    /// <param name="target">The target to follow.</param>
    public void SetTarget(Car target)
    {
        //Set position instantly if previous target was null
        if (CurrentTarget == null && !AllowUserInput && target != null)
            SetCamPosInstant(target.transform.position);

        this.CurrentTarget = target.gameObject;
    }

    private void Awake()
    {
        // Set start position on startup
        _startPosition = this.transform.position;
    }

    // Unity method for updating the simulation
    void FixedUpdate ()
    {
        //Check movement direction
        if (AllowUserInput)
        {
            CheckUserInput();
        }
        else if (CurrentTarget != null)
        {
            targetCamPos = CurrentTarget.transform.position;
        }
        else if (CurrentTarget == null)
        {
            // Find new camera target
            var firstCar = EvolutionManager.Singleton.activeFirstCar;
            if (firstCar != null)
            {
                SetTarget(firstCar);
            }
            else
            {
                ResetToStartPosition();
            }

        }

        targetCamPos.z = CamZ; //Always set z to cam distance
        this.transform.position = Vector3.Lerp(this.transform.position, targetCamPos, CamSpeed * Time.deltaTime); //Move camera with interpolation

        //Check if out of bounds
        if (MovementBounds != null)
        {
            float vertExtent = Camera.main.orthographicSize;
            float horzExtent = vertExtent * Screen.width / Screen.height;

            float rightDiff = (this.transform.position.x + horzExtent) - (MovementBounds.position.x + MovementBounds.rect.width / 2);
            float leftDiff = (this.transform.position.x - horzExtent) - (MovementBounds.position.x - MovementBounds.rect.width / 2);
            float upDiff = (this.transform.position.y + vertExtent) - (MovementBounds.position.y + MovementBounds.rect.height / 2);
            float downDiff = (this.transform.position.y - vertExtent) - (MovementBounds.position.y - MovementBounds.rect.height / 2);

            if (rightDiff > 0)
            {
                this.transform.position = new Vector3(this.transform.position.x - rightDiff, this.transform.position.y, this.transform.position.z);
                targetCamPos.x = this.transform.position.x;
            }
            else if (leftDiff < 0)
            {
                this.transform.position = new Vector3(this.transform.position.x - leftDiff, this.transform.position.y, this.transform.position.z);
                targetCamPos.x = this.transform.position.x;
            }

            if (upDiff > 0)
            {
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - upDiff, this.transform.position.z);
                targetCamPos.y = this.transform.position.y;
            }
            else if (downDiff < 0)
            {
                this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y - downDiff, this.transform.position.z);
                targetCamPos.y = this.transform.position.y;
            }
        }
    }

    private void ResetToStartPosition()
    {
        SetCamPosInstant(_startPosition);
    }

    /// <summary>
    /// Instantly sets the camera position to the given position, without interpolation.
    /// </summary>
    /// <param name="camPos">The position to set the camera to.</param>
    public void SetCamPosInstant(Vector3 camPos)
    {
        camPos.z = CamZ;
        this.transform.position = camPos;
        targetCamPos = this.transform.position;
    }

    private void CheckUserInput()
    {
        float horizontalInput, verticalInput;

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        targetCamPos += new Vector3(horizontalInput * UserInputSpeed * Time.deltaTime, verticalInput * UserInputSpeed * Time.deltaTime);
    }
    #endregion
}
