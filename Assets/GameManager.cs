using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private Transform _boardRoot;

    [SerializeField] private Tile _tilePrefab;

    [SerializeField] private Text _elapsedTimeFromStartText;

    [SerializeField] private Text _scoreText;

    [SerializeField] private Text _comboText;

    [SerializeField] private AudioSource _addSource;

    [SerializeField] private AudioSource _whistleSource;

    [SerializeField] private AudioSource _explosionSource;


    [SerializeField] private ParticleSystem _eliminateEffect;


    public GameOverScreen GameOverScreen;

    private bool isGameActive = true;
    private const int Width = 7;
    private const int Height = 10;
    private const int MiddleX = Width / 2;
    private const int Top = Height - 1;

    private const float MaxElapsedTimePerStep = 0.3f;
    private const float MaxElapsedTimeFromStart = 60f;

    private float _elapsedTimeFromStart;

    private float ElapsedTimeFromStart
    {
        get => _elapsedTimeFromStart;

        set
        {
            _elapsedTimeFromStart = value;
            _elapsedTimeFromStartText.text = _elapsedTimeFromStart.ToString("F2");
        }
    }

    private int _combo;

    private int Combo {
        get => _combo;

        set
        {
            _combo = value;
            _comboText.text = _combo.ToString() + "X";
        }
    }

    private int _score;

    private int Score
    {
        get => _score;

        set
        {
            _score = value;
            _scoreText.text = _score.ToString();
        }
    }

    private List<Tile> tiles = new List<Tile>();


    private void Start()
    {

        StartCoroutine(UpdateGame());
    }

    private IEnumerator UpdateGame()
    {
        while (isGameActive)
        {
            yield return PlayGame();
        }

        GameOver();
    }                                                                            

    private IEnumerator PlayGame()
    {


        var tile = CreateNewTile(tiles);

        Combo = 1;
        ElapsedTimeFromStart = 0f;
        Score = 0;

    
        var elapsedTime = 0f;

        while (ElapsedTimeFromStart <= MaxElapsedTimeFromStart)
        {
            ElapsedTimeFromStart += Time.deltaTime;
            elapsedTime += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveTileLeft(tiles, tile);
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveTileRight(tiles, tile);
            }

            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                if (!MoveTileDown(tiles, tile))
                {
                    tile = CreateNewTile(tiles);

                    if (tile == null) break;
                }
            }
            if (Input.GetKeyDown(KeyCode.Space)) {

                MoveTileToBottom(tiles, tile);
                tile = CreateNewTile(tiles);
                if (tile == null) break;
            }

            if (elapsedTime >= MaxElapsedTimePerStep)
            {
                if (!MoveTileDown(tiles, tile))
                {
                    tile = CreateNewTile(tiles);

                    if (tile == null) break;
                }

                elapsedTime = 0;
            }

            yield return null;
        }
    
        // GameOver
        ElapsedTimeFromStart = Mathf.Min(ElapsedTimeFromStart, MaxElapsedTimeFromStart);
        Destroy(tile?.gameObject);

        foreach (var t in tiles)
        {
            Destroy(t.gameObject);
        }
        isGameActive = false;
        
    }
    private Tile CreateNewTile(IEnumerable<Tile> tiles)
    {
        int number = Random.Range(0, 8);
        if (tiles.Any(other => other.X == MiddleX && other.Y == Top))
            return null;

        var tile = Instantiate(_tilePrefab, _boardRoot);
        tile.X = MiddleX;
        tile.Y = Top;
        tile.number = number;
        tile.numberText.text = number.ToString();
        return tile;
    }
    private bool CanTileMoveTo(IEnumerable<Tile> tiles, int x, int y, int width)
    {
        if (x < 0 || x >= width || y < 0)
            return false;

        return !tiles.Any(other => other.X == x && other.Y == y);
    }

    private void MoveTileLeft(IEnumerable<Tile> tiles, Tile tile)
    {
        if (!CanTileMoveTo(tiles, tile.X - 1, tile.Y, Width)) return;
        tile.X--;
    }
    private void MoveTileRight(IEnumerable<Tile> tiles, Tile tile)
    {
        if (!CanTileMoveTo(tiles, tile.X + 1, tile.Y, Width)) return;
        tile.X++;
    }

    private bool MoveTileDown(ICollection<Tile> tiles, Tile tile)
    {
        if (CanTileMoveTo(tiles, tile.X, tile.Y - 1, Width))
        {
            tile.Y--;
            return true;
        }

        tile.Hit();
        tiles.Add(tile);
        Eliminate();
        return false;
    }

    private void MoveTileToBottom(ICollection<Tile> tiles, Tile tile) {

        while (CanTileMoveTo(tiles, tile.X, tile.Y - 1, Width))
        {
            tile.Y--;
        }
        tile.Hit();
        tiles.Add(tile);
        Eliminate();
    }
    public void Fill()
    {
        for (var x = 0; x < Width; x++)
        {
            for (var y = 0; y < Height; y++)
            {
                var tile = Instantiate(_tilePrefab, _boardRoot);
                tile.X = x;
                tile.Y = y;
            }
        }
    }

    private bool isRowFull(int y)
    {

        var row = tiles.Where(other => other.Y == y).ToList();
        return row.Count==7; 
    }

    private void Eliminate()
    {
        
        if (isRowFull(0))
        {
            var score = tiles.Where(other => other.Y == 0).Sum(tile => tile.number);
            if (score == 23)
            {
                UpdateScore(23*Combo + 7);
                _whistleSource.Play();
                _explosionSource.PlayDelayed(0.3f);
                _eliminateEffect.Play();
                Combo += 1;
                Debug.Log(Combo);

            }
            else
            {
                UpdateScore(7);
                //_eliminateEffect.Play();
                _addSource.Play();
                if (Combo > 1) Combo = 1;
            }

            foreach (var t in tiles)
            {
                if (t.Y == 0)
                {
                    Destroy(t.gameObject);
                }
            }

            tiles = tiles.Where(other => other.Y != 0).ToList();

            tiles.ForEach(other => other.Y -= 1);



        }

    }

    public void UpdateScore(int scoreToAdd)
    {

        Score += scoreToAdd;
    }

    public void GameOver() {
        GameOverScreen.Setup(Score);
    }
}
