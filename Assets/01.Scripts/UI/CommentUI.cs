using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class CommentUI : BaseBehaviour
{
    // Start is called before the first frame update
    public Transform comment;
    public Text CommentTxt;
    public Text NameTxt;


    private bool isComment = false;
    [SerializeField] private bool _isEndComment = false;
    public bool IsEndComment { get { return _isEndComment; } }
    // Start is called before the first frame update


    public void startNPCComment()         //움직일 수 있는지 없는지 반환
    {
        comment.gameObject.SetActive(true);
        _isEndComment = true;
    }


    public void endNPCComment()
    {
        comment.gameObject.SetActive(false);
        _isEndComment = false;               //추후 모든 말이 끝났을때로 변경해야함
    }

#if UNITY_EDITOR

    protected override void OnBindField()
    {
        base.OnBindField();
        comment = this.transform;
        CommentTxt = FindGameObjectInChildren<Text>("CommentTxt");
        NameTxt = FindGameObjectInChildren<Text>("NameTxt");
    }

#endif
}
