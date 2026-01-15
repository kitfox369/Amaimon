using UnityEngine;
using UnityEngine.UI;

public class CommentTipUI : BaseBehaviour
{

    public Transform CommentTip;
    [SerializeField] private Text _commentTipTxt;

    public void activeCommentTip(string npcName)
    {
        _commentTipTxt.text = "'" + npcName + "'과(와) 대화하기";
        CommentTip.gameObject.SetActive(true);
    }

#if UNITY_EDITOR

    protected override void OnBindField()
    {
        base.OnBindField();
        CommentTip = this.transform;
        _commentTipTxt = FindGameObjectInChildren<Text>("CommentTipTxt");
    }

#endif
}
