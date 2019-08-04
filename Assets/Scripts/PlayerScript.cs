﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour {
    public float speed = 5f;
    public bool gridMove = true;
    public Vector2 gridSpace = new Vector2(2.0f, 2.0f);
    public Vector2Int gridSize = new Vector2Int(12, 12);
    public Vector2 gridOrigin = new Vector2(2.0f, 2.0f);
    public float gridMoveTime = 0.2f;

    public GameObject wallPrefab;

    public GameObject basicEnemyPrefab;
    
    private Rigidbody2D playerRb;

    private Vector2 startPos;
    private Vector2 endPos;
    private Vector2 velocity;
    private Vector2Int facing = new Vector2Int(1, 0);

    private bool collisionDetected = false;
    private bool enemyTurn = false;

    private Transform highlight;

    public List<Vector2Int> wallLocations;

    // Start is called before the first frame update
    void Start () {
        playerRb = this.gameObject.GetComponent<Rigidbody2D>();

        startPos = playerRb.position;
        endPos = playerRb.position;

        highlight = gameObject.transform.GetChild(0);

        wallLocations = new List<Vector2Int>();
    }

    // Update is called once per frame
    void Update () {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        
        float lH = Input.GetAxisRaw("Look Horizontal");
        float lV = Input.GetAxisRaw("Look Vertical");

        // Change which direction character is looking
        if ((lH == 0) ^ (lV == 0)) {
            facing = Vector2Int.CeilToInt((new Vector2(lH, lV)).normalized);
        }

        // See if square character is looking at is in the grid
        Vector2Int facingLoc = GetPlayerLocation() + facing;
        Vector2Int facingLocClamped = facingLoc;
        facingLocClamped.Clamp(new Vector2Int(0, 0), gridSize - new Vector2Int(1, 1));
        if (facingLoc != facingLocClamped || wallLocations.Contains(facingLoc)) {
            highlight.gameObject.SetActive(false);
        } else {
            highlight.gameObject.SetActive(true);
        }

        bool turn = false;
        if (gridMove) {
            if (enemyTurn) {
                if (Random.Range(0, 5) == 0) {
                    // Spawn a new enemy
                    List<Vector2Int> spawnLocations = new List<Vector2Int>();
                    Vector2Int playerLoc = GetPlayerLocation();
                    for (int x = 0; x < gridSize.x; x++) {
                        for (int y = 0; y < gridSize.y; y++) {
                            if (Mathf.Abs(x - playerLoc.x) + Mathf.Abs(y - playerLoc.y) > 2) {
                                if (!wallLocations.Contains(new Vector2Int(x, y))) {
                                    spawnLocations.Add(new Vector2Int(x, y));
                                }
                            }
                        }
                    }
                    Vector2Int enemyLocation = spawnLocations[Random.Range(0, spawnLocations.Count)];
                    Vector2 enemyPos = GridLocationToCoordinates(enemyLocation) + new Vector2(1.1f, 1.12f);
                    Instantiate(basicEnemyPrefab, enemyPos, new Quaternion());
                }
                enemyTurn = false;
            } else if (collisionDetected) {
                // If player ran into a wall
                endPos = startPos;
                collisionDetected = false;
            } else if (playerRb.position != endPos) {
                // If player is moving
                playerRb.position = Vector2.MoveTowards(playerRb.position, endPos, 0.2f);
            } else if (Input.GetButtonDown("Place Wall") && highlight.gameObject.activeSelf) {
                // If player is placing wall
                Vector2Int wallLoc = GetPlayerLocation() + facing;
                wallLocations.Add(wallLoc);
                Vector2 wallPos = GridLocationToCoordinates(wallLoc);
                wallPos += new Vector2(0.84f, 2.4f);
                Instantiate(wallPrefab, wallPos, new Quaternion());
                turn = true;
            } else if (v != 0 || h != 0) {
                // If player is starting a motion
                Vector2 move;
                if (v != 0) {
                    move = new Vector2(0f, v);
                } else {
                    move = new Vector2(h, 0f);
                }
                move.Normalize();
                move = Vector2.Scale(move, gridSpace);

                startPos = playerRb.position;
                endPos = startPos + move;
                facing = Vector2Int.CeilToInt((endPos - startPos).normalized);
                turn = true;
            } else {
                // Do nothing
            }

            if (turn) {
                enemyTurn = true;
            }
        } else {
            playerRb.velocity = new Vector2(h * speed, v * speed);
        }

        highlight.localPosition = new Vector2(0.1535f, -0.463f) + Vector2.Scale(facing, gridSpace);
    }

    void OnCollisionEnter2D (Collision2D collision) {
        collisionDetected = true;
    }

    Vector2Int GetPlayerLocation () {
        return CoordinatesToGridLocation(gameObject.transform.position - new Vector3(0, 1, 0));
    }

    Vector2Int CoordinatesToGridLocation (Vector2 coords) {
        Vector2 scaleFactor = new Vector2(1f / gridSpace.x, 1f / gridSpace.y);
        Vector2 pos = Vector2.Scale(coords, scaleFactor);
        pos -= Vector2.Scale(gridOrigin, scaleFactor);
        return Vector2Int.RoundToInt(pos);
    }

    Vector2 GridLocationToCoordinates (Vector2Int loc) {
        Vector2 pos = Vector2.Scale(loc, gridSpace);
        pos += gridOrigin;
        return pos;
    }
}
