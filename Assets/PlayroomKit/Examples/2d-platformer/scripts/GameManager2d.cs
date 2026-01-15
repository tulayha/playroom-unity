using Playroom;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;


public class GameManager2d : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

    /// <summary>
    /// player scores and UI to display score of the game.
    /// </summary>
    [Header("Score and UI")]
    [SerializeField]
    private int score = 0;
    [SerializeField]
    private TextMeshProUGUI playerCount;
    [SerializeField]
    private TextMeshProUGUI scoreTemplate;
    [SerializeField]
    private Transform scoreList;

    private static bool playerJoined;

    /// <summary>
    /// List of players and their gameObjects.
    /// </summary>
    private static readonly Dictionary<string, PlayerEntity> players = new();

    private PlayroomKit _playroomKit;


    void Awake()
    {
        _playroomKit = new();
    }

    /// <summary>
    /// Initialize PlayroomKit, starts multiplayer.
    /// </summary>
    private void Initialize()
    {
        _playroomKit.InsertCoin(new InitOptions()
        {
            maxPlayersPerRoom = 4,
            defaultPlayerStates = new()
            {
                { "score", 0 },
            },
        }, () =>
        {
            _playroomKit.OnPlayerJoin(AddPlayer);
            Debug.Log($"[Unity Log] isHost: {_playroomKit.IsHost()}");
        }, () =>
        {
            Debug.LogWarning("OnDisconnect Callback Called");
        });

        scoreTemplate.gameObject.SetActive(false);
    }

    /// <summary>
    /// Register the RPC method to update the score.
    /// </summary>
    void Start()
    {
        Initialize();

        _playroomKit.RpcRegister("ShootBullet", HandleScoreUpdate, "You shot a bullet!");
        _playroomKit.WaitForState("test", (s) => { Debug.LogWarning($"After waiting for test: {s}"); });

    }

    /// <summary>
    /// Update the Score UI of the player and sync.
    /// </summary>
    void HandleScoreUpdate(string data, string caller)
    {
        var player = _playroomKit.GetPlayer(caller);
        Debug.Log($"Caller: {caller}, Player Name: {player?.GetProfile().name}, Data: {data}");

        if (players.TryGetValue(caller, out PlayerEntity playerEntity))
        {
            var playerController = playerEntity.Controller;
            if (playerController != null)
            {
                if (TryParseShootPayload(data, out int parsedScore, out Vector3 shootPos, out float shootDir))
                {
                    if (_playroomKit.MyPlayer() == null || _playroomKit.MyPlayer().id != caller)
                    {
                        playerController.ShootBullet(shootPos, shootDir);
                    }
                    playerEntity.UpdateScoreText(parsedScore);
                }
                else
                {
                    Debug.LogError($"Unable to parse score data for player: {caller}");
                }
            }
            else
            {
                Debug.LogError($"PlayerController not found on GameObject for caller: {caller}");
            }
        }
        else
        {
            Debug.LogError($"No GameObject found for caller: {caller}");
        }
    }

    /// <summary>
    /// Update the player position and sync.
    /// </summary>
    private void Update()
    {
        if (playerJoined && _playroomKit.MyPlayer() != null)
        {
            var myPlayerId = _playroomKit.MyPlayer().id;

            if (players.TryGetValue(myPlayerId, out PlayerEntity myPlayerEntity) && myPlayerEntity.GameObject != null)
            {   
                myPlayerEntity.Controller.Move();
                myPlayerEntity.Controller.Jump();
                myPlayerEntity.Player.SetState("pos", myPlayerEntity.GameObject.transform.position);
                myPlayerEntity.Player.SetState("facing", myPlayerEntity.Controller.FacingDir);

                if (Input.GetKeyDown(KeyCode.Space))
                {
                    ShootBullet(myPlayerEntity);
                }

                foreach (var playerEntry in players)
                {
                    if (playerEntry.Key == myPlayerId) continue;

                    var playerEntity = playerEntry.Value;

                    if (playerEntity.Player != null && playerEntity.GameObject != null)
                    {
                        var pos = playerEntity.Player.GetState<Vector3>("pos");
                        playerEntity.GameObject.transform.position = pos;

                        var facing = playerEntity.Player.GetState<float>("facing");
                        playerEntity.Controller.ApplyFacing(facing);

                    }
                    else
                    {
                        Debug.LogError($"PlayerEntity not found for player: {playerEntry.Key}");
                    }

                }
            }
            else
            {
                Debug.LogError($"No PlayerEntity found for my player ID: {myPlayerId}");
            }
        }
    }


    /// <summary>
    /// Shoot bullet and update the score.
    /// </summary>
    private void ShootBullet(PlayerEntity playerEntity)
    {
        Vector3 playerPosition = playerEntity.GameObject.transform.position;
        float shootDir = playerEntity.Controller.FacingDir;

        playerEntity.Controller.ShootBullet(playerPosition, shootDir);
        score += 10;
        
        playerEntity.Player.SetState("score", score);

        var payload = SerializeShootPayload(score, playerPosition, shootDir);
        _playroomKit.RpcCall("ShootBullet", payload, PlayroomKit.RpcMode.ALL,
            () => { Debug.Log("Shooting bullet"); });

    }

    /// <summary>
    /// Adds the "player" to the game scene.
    /// </summary>
    public void AddPlayer(PlayroomKit.Player player)
    {
        if (players.ContainsKey(player.id))
        {
            Debug.LogWarning($"Player {player.id} already exists in the game.");
            return;
        }

        Vector3 spawnPos;
        if (_playroomKit.MyPlayer() != null && player.id == _playroomKit.MyPlayer().id)
        {
            spawnPos = new Vector3(Random.Range(-4, 4), Random.Range(1, 5), 0);
            player.SetState("facing", 1f);
            player.SetState("pos", spawnPos);
            InstantiatePlayer(player, spawnPos);

        }
        else
        {
            player.WaitForState("pos", response =>
            {
                var pos = player.GetState<Vector3>("pos");
                spawnPos = pos;
                InstantiatePlayer(player, spawnPos);
                Debug.Log($"Player {player.id} joined.");
            });
        }
    }

    private void InstantiatePlayer(PlayroomKit.Player player, Vector3 spawnPos)
    {
        GameObject playerObj = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        var scoreObj = Instantiate(scoreTemplate, scoreList);

        scoreObj.text = $"{player.GetProfile().name}: {player.GetState<int>("score")}";

        var playerEntity = new PlayerEntity(player, playerObj, scoreObj);
        players.Add(player.id, playerEntity);

        playerJoined = true;
        player.OnQuit(RemovePlayer);

        UpdatePlayerCount();
    }

    /// <summary>
    /// Remove player from the game, called when the player leaves / closes the game.
    /// </summary>
    private void RemovePlayer(string playerID)
    {
        if (players.TryGetValue(playerID, out PlayerEntity playerEntity))
        {
            players.Remove(playerID);
            playerEntity.DestroyObjects();
            UpdatePlayerCount();
            
            Debug.Log($"Player {playerID} removed successfully.");
        }
        else
        {
            Debug.LogWarning($"Player {playerID} is not in dictionary.");
        }
    }

    private void UpdatePlayerCount()
    {
        playerCount.text = $"Players: {players.Count}";
    }

    private sealed class PlayerEntity
    {
        public PlayroomKit.Player Player { get; }
        public GameObject GameObject { get; }
        public PlayerController2d Controller { get; }
        public SpriteRenderer SpriteRenderer { get; }

        public PlayerEntity(PlayroomKit.Player player, GameObject gameObject, TextMeshProUGUI scoreObject)
        {
            Player = player;
            GameObject = gameObject;
            scoreObject.gameObject.SetActive(true);
            Controller = gameObject.GetComponent<PlayerController2d>();
            SpriteRenderer = gameObject.GetComponent<SpriteRenderer>();

            var color = player.GetProfile().color;

            SpriteRenderer.color = color;
            scoreObject.color = color;

            gameObject.GetComponent<PlayerController2d>().scoreText = scoreObject;

        }

        public void UpdateScoreText(int score)
        {
            if (Controller != null && Controller.scoreText != null && Player != null && Player.GetProfile() != null)
            {
                Controller.scoreText.text = $"{Player.GetProfile().name}: {score}";
            }
        }

        public void DestroyObjects()
        {
            Destroy(Controller.scoreText.gameObject);
            Destroy(GameObject);
        }
    }

    private static string SerializeShootPayload(int scoreValue, Vector3 position, float direction)
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}|{1},{2},{3}|{4}",
            scoreValue,
            position.x,
            position.y,
            position.z,
            direction);
    }

    private static bool TryParseShootPayload(string data, out int scoreValue, out Vector3 position, out float direction)
    {
        scoreValue = 0;
        position = Vector3.zero;
        direction = 1f;

        if (string.IsNullOrWhiteSpace(data))
        {
            return false;
        }

        var parts = data.Split('|');
        if (parts.Length != 3)
        {
            return false;
        }

        if (!int.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out scoreValue))
        {
            return false;
        }

        var posParts = parts[1].Split(',');
        if (posParts.Length != 3)
        {
            return false;
        }

        if (!float.TryParse(posParts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) ||
            !float.TryParse(posParts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) ||
            !float.TryParse(posParts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
        {
            return false;
        }

        if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out direction))
        {
            return false;
        }

        position = new Vector3(x, y, z);
        return true;
    }
}

