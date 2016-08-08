using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// ゲームシーンコントローラ
/// </summary>
public class GameScene_Controller : MonoBehaviour
{
    public enum Modes
    {
        // マスを開くモード
        Open,
        // マスをチェック状態にするモード
        Check
    }

    public Modes Mode { get { return mode; } }

    bool IsGameOver { get { return gameOverText.activeSelf; } }

    [SerializeField]
    Size size;
    [SerializeField]
    Button setOpenButton;
    [SerializeField]
    Button setCheckButton;
    [SerializeField]
    GridLayoutGroup cellContainer;
    [SerializeField]
    GameObject gameOverText;
    [SerializeField]
    GameScene_Cell cellPrefab;

    Modes mode;
    List<GameScene_Cell> cells = new List<GameScene_Cell>();

    void Start()
    {
        setOpenButton.onClick.AddListener(OnSetOpenButtonClick);
        setCheckButton.onClick.AddListener(OnSetCheckButtonClick);
        OnSetOpenButtonClick();

        Initialize();
    }

    void Update()
    {
        if (IsGameOver && Input.GetMouseButtonDown(0))
        {
            Initialize();
        }
    }

    public void Initialize()
    {
        gameOverText.SetActive(false);
        cellPrefab.gameObject.SetActive(true);

        foreach (var cell in cells)
        {
            Destroy(cell.gameObject);
        }
        cells.Clear();

        var cellContainerRect = cellContainer.GetComponent<RectTransform>();
        cellContainer.cellSize = new Vector2(cellContainerRect.sizeDelta.x / size.x, cellContainerRect.sizeDelta.y / size.y);
        for (var x = 0; x < size.x; x++)
        {
            for (var y = 0; y < size.y; y++)
            {
                var cell = Instantiate(cellPrefab);
                cell.Initialize(this, x, y);
                cell.transform.SetParent(cellContainer.transform);
                cells.Add(cell);
            }
        }
        for (var i = 0; i < size.x * size.y * 0.1; i++)
        {
            var noBomSells = cells.Where(x => !x.HasBom);
            noBomSells.ElementAt(UnityEngine.Random.Range(0, noBomSells.Count())).SetBom(true);
        }
        cellPrefab.gameObject.SetActive(false);
    }

    public List<GameScene_Cell> GetAroundCells(GameScene_Cell targetCell)
    {
        var aroundCells = new List<GameScene_Cell>();
        Action<int, int> addCellIfExists = (x, y) =>
        {
            var cell = cells.FirstOrDefault(c => c.X == x && c.Y == y);
            if (null != cell)
            {
                aroundCells.Add(cell);
            }
        };
        addCellIfExists(targetCell.X - 1, targetCell.Y - 1);
        addCellIfExists(targetCell.X - 1, targetCell.Y);
        addCellIfExists(targetCell.X - 1, targetCell.Y + 1);
        addCellIfExists(targetCell.X, targetCell.Y - 1);
        addCellIfExists(targetCell.X, targetCell.Y + 1);
        addCellIfExists(targetCell.X + 1, targetCell.Y - 1);
        addCellIfExists(targetCell.X + 1, targetCell.Y);
        addCellIfExists(targetCell.X + 1, targetCell.Y + 1);
        return aroundCells;
    }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    public void GameOver()
    {
        gameOverText.SetActive(true);
        foreach (var cell in cells.Where(x => x.HasBom))
        {
            cell.Reveal();
        }
    }

    void OnSetOpenButtonClick()
    {
        mode = Modes.Open;
        setOpenButton.image.color = Color.red;
        setCheckButton.image.color = Color.white;
    }

    void OnSetCheckButtonClick()
    {
        mode = Modes.Check;
        setOpenButton.image.color = Color.white;
        setCheckButton.image.color = Color.red;
    }

    [Serializable]
    class Size
    {
        public int x = 10;
        public int y = 10;
    }
}
