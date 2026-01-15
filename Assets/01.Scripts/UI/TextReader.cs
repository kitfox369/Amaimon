using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextReader : MonoBehaviour
{
    public CommentUI CommentUI;

    private int pageNum = 0;
    private int pageCount = 10;
    private int eventNum = -1;
    private int contextNum = 0;
    private int contextCount = 0;

    private List<string> tutorialTxt;
    private List<string> tutorialTxt2;      //나중에 한개의 더미로 합칠것

    private int commentNum = 0;
    private int commentCount = 0;
    private int guestKindNum = 0;

    private bool isTyping = false;

    public Comment currentCom;
    ShopComment currentShopCom;

    //대화창 시작
    //eventNum : 이벤트 id
    public void startComment(Comment com)
    {
        //uiCommentObj.SetActive(true);
        //GameManager.Instance.switchComment(true);
        currentCom = com;
        StartCoroutine("textReadEvent");
    }

    public void nextPage()
    {
        //페이지의 번호가 페이지 갯수보다 작을 때
        if (pageNum < pageCount)
        {
            if (isTyping)
            {
                if (eventNum == 0)
                {
                    CommentUI.CommentTxt.text = tutorialTxt[pageNum];
                }
                else if (eventNum == 1)
                {

                    CommentUI.CommentTxt.text = tutorialTxt2[pageNum];
                }
                StopCoroutine("textReadEvent");
                isTyping = false;
            }
            else if (isTyping == false)
            {
                if (++pageNum >= pageCount) endComment();
                else
                {
                    endParagraph();
                    //Debug.Log("stop textRead:"+tutorialTxt[pageNum]);
                    isTyping = true;
                    StartCoroutine("textReadEvent");
                }
            }
        }
    }

    private void endParagraph()
    {
        CommentUI.CommentTxt.text = "";
        contextNum = 0;
    }

    private void endComment()
    {
        CommentUI.CommentTxt.text = "";
        contextNum = 0;
        //uiCommentObj.SetActive(false);
        //GameManager.Instance.switchComment(false);
        pageNum = 0;

        isTyping = false;
        StopCoroutine("textReadEvent");
    }

    IEnumerator textReadEvent()
    {
        isTyping = true;
        if (eventNum == 0) { contextCount = tutorialTxt[pageNum].Length; }
        else if (eventNum == 1)
        {
            //%를 닉네임으로 바꾼후
            if (tutorialTxt2[pageNum].Contains("%"))
            {
                //tutorialTxt2[pageNum] = tutorialTxt2[pageNum].Replace("%", GameManager.Instance.playerName);
            }
            //문장의 길이를 계산함
            contextCount = tutorialTxt2[pageNum].Length;
        }
        while (contextCount > contextNum)
        {
            if (eventNum == 0)
            {
                CommentUI.CommentTxt.text += tutorialTxt[pageNum][contextNum];
            }
            else if (eventNum == 1)
            {
                CommentUI.CommentTxt.text += tutorialTxt2[pageNum][contextNum];
            }
            contextNum++;
            yield return new WaitForSeconds(0.08f);
        }
        if (isTyping)
        {
            isTyping = false;
        }
    }

    public void startNPCComment(Comment com,int questState)
    {
        CommentUI.CommentTxt.text = "";
        currentCom = com;
        CommentUI.NameTxt.text = currentCom.NPCName;
        isTyping = false;
        commentNum = 0;
        StartCoroutine("textReadComment", questState);
    }

    public void startNPCComment(ShopComment com, int shopKind)
    {
        CommentUI.CommentTxt.text = "";
        currentShopCom = com;
        CommentUI.NameTxt.text = currentShopCom.NPCName;
        isTyping = false;
        commentNum = 0;
        StartCoroutine("textReadShopComment");
    }

    public void endNPCComment()
    {
        CommentUI.CommentTxt.text = "";
        contextNum = 0;
        //uiCommentObj.SetActive(false);
        //GameManager.Instance.switchComment(false);
        pageNum = 0;

        isTyping = false;
        StopCoroutine("textReadComment");
    }

    public void endShopNPCComment()
    {
        CommentUI.CommentTxt.text = "";
        contextNum = 0;
        pageNum = 0;

        isTyping = false;
        StopCoroutine("textReadShopComment");
    }

    public string getNPCComment(int guestKindNum)
    {
        return currentCom.NPCDescription;
    }

    IEnumerator textReadComment(int questState)
    {
        isTyping = true;
        string npcComment = " ";
        if(questState==0) npcComment= currentCom.NPCDescription;
        else if(questState==1) npcComment = currentCom.hintDescript;
        else if (questState == 2) npcComment = currentCom.finishDescript;
        commentCount = npcComment.Length;
        while (commentCount > commentNum)
        {
            CommentUI.CommentTxt.text += npcComment[commentNum];

            commentNum++;
            yield return new WaitForSeconds(0.08f);
        }
        if (isTyping)
        {
            isTyping = false;
        }
    }

    IEnumerator textReadShopComment()
    {
        isTyping = true;
        string npcComment = " ";
        npcComment = currentShopCom.NPCDescription;
        commentCount = npcComment.Length;
        while (commentCount > commentNum)
        {
            CommentUI.CommentTxt.text += npcComment[commentNum];

            commentNum++;
            yield return new WaitForSeconds(0.08f);
        }
        if (isTyping)
        {
            isTyping = false;
        }
    }

}

[System.Serializable]
public struct Comment
{
    public string NPCName;
    public string NPCDescription;
    public QuestType Type;
    public string QuestDescription;
    public string hintDescript;
    public string finishDescript;
    

    public Comment(string name,string description,QuestType type,string questDescription,string hintMent,string finishMent)
    {
        NPCName = name;
        NPCDescription = description;
        Type = type;
        QuestDescription = questDescription;
        hintDescript = hintMent;
        finishDescript = finishMent;
    }
}

[System.Serializable]
public struct ShopComment
{
    public string NPCName;
    public string NPCDescription;
    public string finishDescript;


    public ShopComment(string name, string description, string finishMent)
    {
        NPCName = name;
        NPCDescription = description;
        finishDescript = finishMent;
    }
}