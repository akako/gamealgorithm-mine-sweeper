using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

/// <summary>
/// セル
/// </summary>
[RequireComponent(typeof(Button))]
public class GameScene_Cell : UIBehaviour
{
    public int X { get { return x; } }

    public int Y { get { return y; } }

    public bool IsOpened { get { return !overlay.activeSelf; } }

    public bool IsChecked { get { return checkMark.activeSelf; } }

    public bool HasMine { get { return mine.activeSelf; } }

    public int Number { set { numberText.text = value == 0 ? "" : string.Format("{0}", value); } }

    [SerializeField]
    Text numberText;
    [SerializeField]
    GameObject mine;
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
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    /// <summary>
    /// マスの初期化処理です
    /// </summary>
    /// <param name="controller">Controller.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    public void Initialize(GameScene_Controller controller, int x, int y)
    {
        this.controller = controller;
        this.x = x;
        this.y = y;

        numberText.gameObject.SetActive(true);
        mine.SetActive(false);
        overlay.SetActive(true);
        checkMark.SetActive(false);
    }

    /// <summary>
    /// 地雷をセットします
    /// </summary>
    /// <param name="hasMine">If set to <c>true</c> has mine.</param>
    public void SetMine(bool hasMine)
    {
        mine.SetActive(hasMine);
    }

    /// <summary>
    /// マスのをリベールします（見た目のみの処理）
    /// </summary>
    public void Reveal()
    {
        overlay.SetActive(false);
    }

    /// <summary>
    /// マスを開きます
    /// </summary>
    public bool Open()
    {
        if (IsOpened || IsChecked)
        {
            return true;
        }
        Reveal();
        return controller.OnCellOpen(this);
    }

    /// <summary>
    /// マスがクリックされた際の処理
    /// </summary>
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
                Open();
                break;
            case GameScene_Controller.Modes.Check:
                checkMark.SetActive(!checkMark.activeSelf);
                break;
        }
    }
}
