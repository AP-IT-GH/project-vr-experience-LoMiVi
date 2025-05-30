using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections; // For Coroutines if needed, or just a simple timer

public class WeepingAngelAgent : Agent
{
    [Header("Agent Models")]
    public GameObject idleModel;
    public GameObject pointingModel; // Assign your "Pointing" pose model
    public GameObject attackModel;

    [Header("Behavior Timings")]
    public float pointingDuration = 1.5f; // How long to point before attacking
    private float pointingTimer;

    private enum UnobservedBehaviorState
    {
        None,      // Not relevant when observed
        Pointing,
        Attacking
    }
    private UnobservedBehaviorState currentUnobservedState = UnobservedBehaviorState.None;
    private bool wasPreviouslyObserved = true; // To detect transition from observed to unobserved

    [Header("Agent Movement")]
    public float moveSpeed = 2f;
    public float turnSpeed = 180f;

    [Header("Player Interaction")]
    public Transform playerCameraTransform;
    public float playerDetectionAngle = 70f;
    public float playerDetectionDistance = 20f;
    public LayerMask sightObscuringLayers;

    [Header("Target (Player)")]
    public Transform playerTarget;

    private Rigidbody rb;
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private bool isPlayerLookingAtMe = false;
    private float previousDistanceToPlayer;
    private bool _movedThisStep = false;


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
        startingPosition = transform.localPosition;
        startingRotation = transform.localRotation;

        if (playerCameraTransform == null) Debug.LogError("Player Camera Transform not assigned!", this);
        if (playerTarget == null) Debug.LogError("Player Target Transform not assigned!", this);
        if (idleModel == null) Debug.LogError("Idle Model not assigned!", this);
        if (pointingModel == null) Debug.LogError("Pointing Model not assigned!", this);
        if (attackModel == null) Debug.LogError("Attack Model not assigned!", this);

        SetVisualState(true); // Start in idle/observed state
        currentUnobservedState = UnobservedBehaviorState.None; // Explicitly set
        wasPreviouslyObserved = true;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = startingPosition;
        transform.localRotation = startingRotation;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (playerTarget != null)
        {
            previousDistanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        }
        _movedThisStep = false;
        isPlayerLookingAtMe = CheckIfPlayerIsLooking();
        wasPreviouslyObserved = isPlayerLookingAtMe; // Sync this
        pointingTimer = 0f; // Reset timer

        if (isPlayerLookingAtMe)
        {
            currentUnobservedState = UnobservedBehaviorState.None;
        }
        else
        {
            currentUnobservedState = UnobservedBehaviorState.Pointing; // Start pointing if initially unobserved
        }
        SetVisualState(isPlayerLookingAtMe);
    }

    private bool CheckIfPlayerIsLooking()
    {
        // ... (same as before)
        if (playerCameraTransform == null || playerTarget == null) return false;
        Vector3 directionToAgent = (transform.position - playerCameraTransform.position);
        if (directionToAgent.sqrMagnitude > playerDetectionDistance * playerDetectionDistance) return false;
        float angle = Vector3.Angle(playerCameraTransform.forward, directionToAgent.normalized);
        if (angle < playerDetectionAngle / 2f)
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCameraTransform.position, directionToAgent.normalized, out hit, playerDetectionDistance, ~sightObscuringLayers, QueryTriggerInteraction.Ignore))
            {
                if (hit.collider.transform.IsChildOf(this.transform) || hit.collider.gameObject == this.gameObject) return true;
            }
        }
        return false;
    }

    private void SetVisualState(bool isObserved)
    {
        idleModel.SetActive(isObserved);

        if (isObserved)
        {
            pointingModel.SetActive(false);
            attackModel.SetActive(false);
        }
        else // Unobserved
        {
            pointingModel.SetActive(currentUnobservedState == UnobservedBehaviorState.Pointing);
            attackModel.SetActive(currentUnobservedState == UnobservedBehaviorState.Attacking);
        }
    }

    // Call this method in Update or FixedUpdate to handle unobserved state logic
    private void UpdateUnobservedBehavior()
    {
        if (isPlayerLookingAtMe)
        {
            if (!wasPreviouslyObserved) // Transitioned from Unobserved to Observed
            {
                currentUnobservedState = UnobservedBehaviorState.None;
                pointingTimer = 0f;
                SetVisualState(true);
            }
            wasPreviouslyObserved = true;
            return; // Do nothing else if observed
        }

        // --- Player is NOT Looking ---
        if (wasPreviouslyObserved) // Transitioned from Observed to Unobserved
        {
            currentUnobservedState = UnobservedBehaviorState.Pointing;
            pointingTimer = 0f;
            SetVisualState(false); // Update visuals to show pointing model
        }
        wasPreviouslyObserved = false;


        if (currentUnobservedState == UnobservedBehaviorState.Pointing)
        {
            // Orient towards player while pointing (optional, but good)
            if (playerTarget != null)
            {
                Vector3 directionToPlayer = playerTarget.position - transform.position;
                directionToPlayer.y = 0; // Keep upright
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed * 0.5f); // Slower turn for pointing
                }
            }

            pointingTimer += Time.deltaTime;
            if (pointingTimer >= pointingDuration)
            {
                currentUnobservedState = UnobservedBehaviorState.Attacking;
                SetVisualState(false); // Update visuals to show attack model
            }
        }
        // No specific logic needed here for Attacking state as ML-Agent handles movement
        // SetVisualState(false) will already be called or have been called.
    }


    // It's generally better to run behavior logic like timers in Update or FixedUpdate
    // rather than CollectObservations if it's not directly an observation.
    void Update()
    {
        UpdateUnobservedBehavior();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        isPlayerLookingAtMe = CheckIfPlayerIsLooking(); // Recalculate for current observation frame

        sensor.AddObservation(isPlayerLookingAtMe);
        sensor.AddObservation(currentUnobservedState == UnobservedBehaviorState.Pointing); // Observe if pointing
        sensor.AddObservation(currentUnobservedState == UnobservedBehaviorState.Attacking); // Observe if attacking

        if (playerTarget == null)
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
            sensor.AddObservation(false);
            return;
        }

        Vector3 dirToPlayer = (playerTarget.position - transform.position).normalized;
        sensor.AddObservation(transform.InverseTransformDirection(dirToPlayer));
        sensor.AddObservation(Vector3.Distance(transform.position, playerTarget.position) / playerDetectionDistance);
        float facingPlayer = Vector3.Dot(transform.forward, dirToPlayer);
        sensor.AddObservation(facingPlayer);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Visual state update should ideally happen based on UpdateUnobservedBehavior()
        // But we ensure it's correct before action logic
        SetVisualState(isPlayerLookingAtMe);

        int moveAction = actions.DiscreteActions[0];
        int turnAction = actions.DiscreteActions[1];
        _movedThisStep = (moveAction != 0 || turnAction != 0);

        if (isPlayerLookingAtMe)
        {
            if (_movedThisStep)
            {
                AddReward(-1.0f); // Penalty for trying to move while observed
                return; // Frozen
            }
            else
            {
                AddReward(0.05f); // Reward for staying still when observed
            }
        }
        else // Not being looked at
        {
            if (currentUnobservedState == UnobservedBehaviorState.Pointing)
            {
                // Agent should NOT move while pointing.
                // If ML-Agent tries to output movement, we can ignore it or even penalize.
                // For simplicity, we just don't apply movement.
                // The rotation towards player is handled in UpdateUnobservedBehavior.
                if (_movedThisStep)
                {
                    // AddReward(-0.05f); // Optional small penalty for trying to move during pointing
                }
            }
            else if (currentUnobservedState == UnobservedBehaviorState.Attacking)
            {
                // Apply actual movement as decided by ML-Agent
                Vector3 moveInput = Vector3.zero;
                if (moveAction == 1) moveInput.z = 1;
                else if (moveAction == 2) moveInput.z = -1;

                float turnInput = 0f;
                if (turnAction == 1) turnInput = -1;
                else if (turnAction == 2) turnInput = 1; // Corrected based on typical branch size (0,1,2)

                Vector3 moveDirection = transform.TransformDirection(moveInput) * moveSpeed * Time.fixedDeltaTime;
                rb.MovePosition(rb.position + moveDirection);

                Quaternion turn = Quaternion.Euler(0f, turnInput * turnSpeed * Time.fixedDeltaTime, 0f);
                rb.MoveRotation(rb.rotation * turn);

                // Rewards for attacking phase
                if (playerTarget != null)
                {
                    float currentDistanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
                    float distanceDelta = previousDistanceToPlayer - currentDistanceToPlayer;
                    if (distanceDelta > 0.01f) // Made progress
                    {
                        AddReward(distanceDelta * 0.1f);
                    }
                    previousDistanceToPlayer = currentDistanceToPlayer;

                    if (currentDistanceToPlayer < 1.5f)
                    {
                        SetReward(1.0f);
                        Debug.Log("Agent caught player!");
                        EndEpisode();
                    }
                }
            }
        }
        AddReward(-1f / MaxStep);
    }

    // ... Heuristic, OnDrawGizmosSelected, OnCollisionEnter remain largely the same ...
    // Make sure Heuristic can test the visual states if desired.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // ... (previous heuristic code for W,A,S,D) ...
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut.Clear();

        if (Input.GetKey(KeyCode.W)) discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.S)) discreteActionsOut[0] = 2;
        else discreteActionsOut[0] = 0;

        if (Input.GetKey(KeyCode.A)) discreteActionsOut[1] = 1;
        else if (Input.GetKey(KeyCode.D)) discreteActionsOut[1] = 2;
        else discreteActionsOut[1] = 0;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool looking = CheckIfPlayerIsLooking();
            Debug.Log("Is Player Looking (Heuristic Check): " + looking + ", Unobserved State: " + currentUnobservedState);
            // Manual state update for testing. This doesn't run the timer logic.
            SetVisualState(looking);
        }
        if (Input.GetKeyDown(KeyCode.P)) // Test force pointing
        {
            isPlayerLookingAtMe = false;
            wasPreviouslyObserved = true; // Force transition to unobserved
            UpdateUnobservedBehavior(); // Trigger logic
            Debug.Log("Forced to Pointing. Current Unobserved State: " + currentUnobservedState);
        }
    }
}