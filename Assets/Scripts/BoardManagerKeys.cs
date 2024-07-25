using UnityEngine;
using AudioHelm;

public class BoardManagerWithKey : MonoBehaviour
{
    public static BoardManagerWithKey Instance;   // Singleton instance
    public GameObject keyPrefab;
    public AudioHelm.HelmSequencer helm;

    public Key[,] boardKeys; // 2D array to store references to all board keys

    private float stepCount;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (helm == null)
        {
            Debug.LogError("HelmSequencer is not assigned in BoardManagerWithKey.");
        }
    }

    void Start()
    {
        if (helm == null)
        {
            helm = GameObject.Find("HelmSequencer").GetComponent<AudioHelm.HelmSequencer>();
            if (helm == null)
            {
                Debug.LogError("HelmSequencer component not found in the scene.");
                return;
            }
        }

        InitializeBoard();
    }

    void Update()
    {
        if (helm == null) return;

        foreach (Key key in boardKeys)
        {
            if (key != null)
            {
                if (helm.currentIndex == key.step)
                {
                    HighlightKeyOnStep(helm.currentIndex);
                }
                else
                {
                    key.GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
    }

    private void InitializeBoard()
    {
        // Assuming a fixed size board of 8x8 keys
        int boardSizeX = 8;
        int boardSizeY = 8;

        // Initialize the boardKeys 2D array
        boardKeys = new Key[boardSizeX, boardSizeY];

        // Loop through each key position and instantiate a new key
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                Vector3 keyPosition = new Vector3(y, x, 0); // Adjust position as needed

                // Instantiate a new key prefab (assuming you have a prefab assigned in the inspector)
                GameObject keyObject = Instantiate(keyPrefab, keyPosition, Quaternion.identity);

                // Parent the key under the BoardManagerWithKey for organization (optional)
                keyObject.transform.parent = transform;
                keyObject.name = "Key_" + stepCount.ToString();
                Key key = keyObject.GetComponent<Key>();
                
                if (key == null)
                {
                    Debug.LogError("Key component not found on instantiated key prefab.");
                    continue;
                }

                key.step = stepCount;
                stepCount++;

                // Set default sprite for the key
                key.SetSprite(key.defaultSprite);

                // Store the key in the boardKeys array
                boardKeys[x, y] = key;

                Debug.Log($"Instantiated Key at position ({x}, {y}) with note {key.step}");
            }
        }
    }

    // Example method to reset the board to its default state
    public void ResetBoard()
    {
        foreach (Key key in boardKeys)
        {
            if (key != null)
            {
                key.SetSprite(key.defaultSprite);
                // Optionally reset other properties of the key
            }
        }
    }

    // Method to get a reference to the key at specified coordinates
    public Key GetKey(int x, int y)
    {
        if (x >= 0 && x < boardKeys.GetLength(0) && y >= 0 && y < boardKeys.GetLength(1))
        {
            return boardKeys[x, y];
        }
        else
        {
            Debug.LogError("Attempted to access key out of board bounds.");
            return null;
        }
    }

    // Method to get the position of a key on the board
    public Vector2Int GetKeyPosition(Key key)
    {
        for (int x = 0; x < boardKeys.GetLength(0); x++)
        {
            for (int y = 0; y < boardKeys.GetLength(1); y++)
            {
                if (boardKeys[x, y] == key)
                {
                    return new Vector2Int(x, y);
                }
            }
        }

        Debug.LogError("Key not found on the board.");
        return Vector2Int.zero;
    }

    // Method to get the size of the board (x, y)
    public Vector2Int GetBoardSize()
    {
        return new Vector2Int(boardKeys.GetLength(0), boardKeys.GetLength(1));
    }

    // Method to highlight a key when helm.currentIndex matches key note
    private void HighlightKeyOnStep(float noteIndex)
    {
        // Iterate through all board keys
        for (int x = 0; x < boardKeys.GetLength(0); x++)
        {
            for (int y = 0; y < boardKeys.GetLength(1); y++)
            {
                Key key = boardKeys[x, y];
                if (key != null && key.step == noteIndex)
                {
                    HighlightKey(key);
                }
            }
        }
    }

    // Method to highlight a single key by changing its sprite color to grey
    private void HighlightKey(Key key)
    {
        // Change sprite color to grey
        SpriteRenderer spriteRenderer = key.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.grey;
        }
        else
        {
            Debug.LogError("SpriteRenderer component not found on key.");
        }
    }
}
