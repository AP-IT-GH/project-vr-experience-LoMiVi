using NUnit.Framework;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class StatueAgent : Agent
{
    public float speedMultiplier = 0.1f;
    public float rotationMultiplier = 5;
    public GameObject playersParent;

    private List<GameObject> players = new List<GameObject>();
    private GameObject activePlayer;
    private Rigidbody rb;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private HashSet<Vector2Int> visitedPositions = new HashSet<Vector2Int>();

    private bool isFrozen = false;
    public int AttackChangeDistance = 3;
    public GameObject angelIdle;
    public GameObject angelAttack;
    public Transform player;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        // Populate the players list from children of the parent
        if (playersParent != null)
        {
            foreach (Transform child in playersParent.transform)
            {
                players.Add(child.gameObject);
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        visitedPositions = new HashSet<Vector2Int>();

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        if (players.Count > 0)
        {
            foreach (GameObject player in players)
            {
                player.SetActive(false);
            }

            int randomIndex = Random.Range(0, players.Count);
            activePlayer = players[randomIndex];
            activePlayer.SetActive(true);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(transform.forward);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isFrozen) return; // Skip processing if frozen
        int moveAction = actions.DiscreteActions[0]; // 0 = idle, 1 = forward, 2 = backward
        int turnAction = actions.DiscreteActions[1]; // 0 = none, 1 = left, 2 = right

        Vector3 moveDir = Vector3.zero;

        if (moveAction == 1) moveDir = transform.forward;
        else if (moveAction == 2) moveDir = -transform.forward;

        rb.MovePosition(rb.position + moveDir * speedMultiplier * Time.fixedDeltaTime);

        float turnAmount = 0f;
        if (turnAction == 1) turnAmount = -1f;
        else if (turnAction == 2) turnAmount = 1f;

        Quaternion turnOffset = Quaternion.Euler(0, turnAmount * rotationMultiplier * Time.fixedDeltaTime, 0);
        rb.MoveRotation(rb.rotation * turnOffset);

        Vector2Int gridPos = new Vector2Int(
            Mathf.RoundToInt(transform.localPosition.x),
            Mathf.RoundToInt(transform.localPosition.z)
        );

        if (!visitedPositions.Contains(gridPos))
        {
            visitedPositions.Add(gridPos);
            AddReward(0.05f); // Reward for exploring new tile
        }

        if (this.transform.position.y < -1)
        {
            transform.SetPositionAndRotation(startPosition, startRotation);
            EndEpisode();
        }

        if (StepCount > 9999)
        {
            AddReward(-0.5f); // Penalty for not reaching player
            EndEpisode();     // Force reset
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            Debug.Log("Statue hit Player");
            SetReward(10.0f);
            EndEpisode();
        }
        if (other.gameObject.tag == "wall")
        {
            Debug.Log("Statue hit wall");
            SetReward(-0.02f);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discrete = actionsOut.DiscreteActions;
        discrete[0] = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? 2 : 0;
        discrete[1] = Input.GetKey(KeyCode.A) ? 1 : Input.GetKey(KeyCode.D) ? 2 : 0;
    }

    public void FreezeMovement()
    {
        if (!isFrozen && Vector3.Distance(transform.position, player.position) <= AttackChangeDistance)
        {
            angelAttack.SetActive(true);
            angelIdle.SetActive(false);
        }
        isFrozen = true;
    }

    public void UnfreezeMovement()
    {
        angelAttack.SetActive(false);
        angelIdle.SetActive(true);
        isFrozen = false;
    }
}
