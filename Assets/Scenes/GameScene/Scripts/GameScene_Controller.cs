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

    /// <summary>
    /// マスの操作モード
    /// </summary>
    /// <value>The mode.</value>
    public Modes Mode { get { return mode; } }

    /// <summary>
    /// ゲームクリアしたかどうか
    /// </summary>
    /// <value><c>true</c> if this instance is game clear; otherwise, <c>false</c>.</value>
    bool IsGameClear { get { return gameClearText.activeSelf; } }

    /// <summary>
    /// ゲームオーバーになったかどうか
    /// </summary>
    /// <value><c>true</c> if this instance is game over; otherwise, <c>false</c>.</value>
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
    GameObject gameClearText;
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
        if ((IsGameClear || IsGameOver) && Input.GetMouseButtonDown(0))
        {
            Initialize();
        }
    }

    /// <summary>
    /// ゲームの初期化処理を行います
    /// </summary>
    public void Initialize()
    {
        gameClearText.SetActive(false);
        gameOverText.SetActive(false);
        cellPrefab.gameObject.SetActive(true);

        // 前のゲームのマスがある場合は削除
        foreach (var cell in cells)
        {
            Destroy(cell.gameObject);
        }
        cells.Clear();

        // セルを生成・配置
        var cellContainerRect = cellContainer.GetComponent<RectTransform>();
        cellContainer.cellSize = new Vector2(cellContainerRect.sizeDelta.x / size.x, cellContainerRect.sizeDelta.y / size.y);
        for (var x = 0; x < size.x; x++)
        {
            for (var y = 0; y < size.y; y++)
            {
                var cell = Instantiate(cellPrefab);
                cell.Initialize(this, x, y);
                cell.transform.SetParent(cellContainer.transform);
                cell.transform.localScale = Vector3.one;
                cells.Add(cell);
            }
        }
        // セルに地雷を配置
        for (var i = 0; i < size.x * size.y * 0.1; i++)
        {
            var noMineSells = cells.Where(x => !x.HasMine);
            noMineSells.ElementAt(UnityEngine.Random.Range(0, noMineSells.Count())).SetMine(true);
        }

        cellPrefab.gameObject.SetActive(false);
    }

    /// <summary>
    /// マスを開いた際の処理です
    /// </summary>
    /// <param name="cell">Cell.</param>
    public bool OnCellOpen(GameScene_Cell cell)
    {
        if (cell.HasMine)
        {
            GameOver();
            return false;
        }

        // 周りのマスに地雷が何個あるかチェック
        var aroundCells = GetAroundCells(cell);
        var mineCountOnAroundCell = aroundCells.Count(x => x.HasMine);
        cell.Number = mineCountOnAroundCell;
        if (mineCountOnAroundCell == 0)
        {
            // 周りに地雷が無い場合、周りのマスを連鎖的に開く
            foreach (var aroundCell in aroundCells)
            {
                if (!aroundCell.Open())
                {
                    return false;
                }
            }
        }

        // 地雷が無いマスが全部開かれたならゲームクリア
        if (!cells.Any(x => !x.HasMine && !x.IsOpened))
        {
            GameClear();
            return false;
        }

        return true;
    }

    /// <summary>
    /// 指定マスの周囲のマスを取得します
    /// </summary>
    /// <returns>The around cells.</returns>
    /// <param name="targetCell">Target cell.</param>
    List<GameScene_Cell> GetAroundCells(GameScene_Cell targetCell)
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
    /// ゲームクリア処理
    /// </summary>
    void GameClear()
    {
        gameClearText.SetActive(true);
    }

    /// <summary>
    /// ゲームオーバー処理
    /// </summary>
    void GameOver()
    {
        gameOverText.SetActive(true);
        foreach (var cell in cells.Where(x => x.HasMine))
        {
            cell.Reveal();
        }
    }

    /// <summary>
    /// OPENモードのボタン押下時の処理
    /// </summary>
    void OnSetOpenButtonClick()
    {
        mode = Modes.Open;
        setOpenButton.image.color = Color.red;
        setCheckButton.image.color = Color.white;
    }

    /// <summary>
    /// CHECKモードのボタン押下時の処理
    /// </summary>
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
