using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{

    public string nextScene;

    public GameObject mTitleCanvas;
    public GameObject mLoadingCanvas;
    public Image progressBar;

    public Text loadingTxt;

    private void Start()
    {
        nextScene = "InGame";
    }

    public void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        mTitleCanvas.SetActive(false);
        mLoadingCanvas.SetActive(true);
        StartCoroutine("LoadingScene");
    }

    IEnumerator LoadingScene()
    {
        yield return null;

        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0.0f;

        while (!op.isDone)
        {
            timer += Time.deltaTime;

            loadingTxt.text = (Mathf.Round(progressBar.fillAmount * 100.0f)).ToString() + "%";

            //if (timer < 10)
            //{
            //    if (op.progress > 0.9f)
            //        progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress * timer, timer);
            //    else
            //        progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, op.progress, timer);
            //}
            //else
            {
                progressBar.fillAmount = Mathf.Lerp(progressBar.fillAmount, 1f, timer);
                op.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
