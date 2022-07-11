using UnityEngine;

public class UIController : MonoBehaviour
{
    GameModeEnum mode = GameModeEnum.Edit;
    public HexGameUI gameUI;
    public HexMapEditor editor;
    // Start is called before the first frame update

    private void OnEnable() {
        SetModeEdit();
        Debug.Log(mode);
    }
    public void SetModeStart(){
        mode  = GameModeEnum.Start;
        gameUI.gameObject.SetActive(true);
        editor.gameObject.SetActive(false);
    }
    public void SetModeEdit(){
        mode  = GameModeEnum.Edit;
        gameUI.gameObject.SetActive(false);
        editor.gameObject.SetActive(true);
    }

}
