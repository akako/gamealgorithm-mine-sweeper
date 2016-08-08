using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// セル
/// </summary>
public class GameScene_Cell : Button
{
    public int X { get { return x; } }

    public int Y { get { return y; } }

    public bool IsOpened { get { return !overlay.activeSelf; } }

    public bool IsChecked { get { return checkMark.activeSelf; } }

    public bool HasBom { get { return bom.activeSelf; } }

    [SerializeField]
    Text number;
    [SerializeField]
    GameObject bom;
    [SerializeField]
    GameObject overlay;
    [SerializeField]
    GameObject checkMark;

    GameScene_Controller controller;
    int x;
    int y;

    protected override void Start()
    {
        base.Start();
        onClick.AddListener(OnClick);
    }

    public void Initialize(GameScene_Controller controller, int x, int y)
    {
        this.controller = controller;
        this.x = x;
        this.y = y;

        number.gameObject.SetActive(true);
        bom.SetActive(false);
        overlay.SetActive(true);
        checkMark.SetActive(false);
    }

    public void SetBom(bool hasBom)
    {
        bom.SetActive(hasBom);
        number.gameObject.SetActive(!hasBom);
    }

    public bool Open()
    {
        if (IsOpened || IsChecked)
        {
            // オープン済みorチェックされたマスなら何もしない
            return true;
        }

        Reveal();
        if (HasBom)
        {
            return false;
        }

        var aroundCells = controller.GetAroundCells(this);
        var bomCountOnAroundCell = aroundCells.Count(x => x.HasBom);
        if (bomCountOnAroundCell == 0)
        {
            number.text = "";
            foreach (var cell in aroundCells.Where(c => (Mathf.Abs(c.X - x) + Mathf.Abs(c.Y - y)) == 1))
            {
                if (!cell.Open())
                {
                    return false;
                }
            }
        }
        else
        {
            number.text = string.Format("{0}", bomCountOnAroundCell);
        }

        return true;
    }

    public void Reveal()
    {
        overlay.SetActive(false);
    }

    void OnClick()
    {
        if (IsOpened)
        {
            // 既にオープンされたマスなら何もしない
            return;
        }
        switch (controller.Mode)
        {
            case GameScene_Controller.Modes.Open:
                if (!Open())
                {
                    controller.GameOver();
                }
                break;
            case GameScene_Controller.Modes.Check:
                checkMark.SetActive(!checkMark.activeSelf);
                break;
        }
    }
}
