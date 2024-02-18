using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerCamera : MonoBehaviour
{
    Transform mTrans;
    Transform mFollowTrans;
    Vector3 mFollowCamTrans;
    Transform mPlayerTrans;
    Transform mTarget;
    BossMapInfo mBossMapInfo;

    // Start is called before the first frame update

    [Header("Parameter")]
    public float clampAngle = 70f;
    public float sensitivity;
    public float followSpeed = 5f;
    public float fadeSpeed = 3.0f;


    private float rotX;
    private float rotY;

    public Vector3 margin;
    public Vector3 NPCmargin;
    public Vector3 dirNormalized;
    public Vector3 finalDir;
    float standardMinZoom;
    float standardMaxZoom;
    public float minDistance;
    public float maxDistance;
    public float finalDistance;
    public float smoothness = 10f;

    enum CameraMode
    {
        FOLLOW,NPC,INTRO,BOSS_END,ZOOM
    }
    CameraMode mMode;

    enum ZoomMode
    {
        IN,OUT
    }
    ZoomMode mZoom;

    public bool bShakingCam;
    public float mShakeAmount;
    public float mShakingTime;
    private float mShakingTimeOrigin=0.1f;
    float mShakingTimer;
    IEnumerator eShakingCoroutine;

    public Material[] cameraEffectMat;
    MainCamEffect mCamEffect;

    float grayScale = 0.0f;
    float fadeValue = 0.0f;
    float appliedTime = 2.0f;

    public UIManager uiM;

    public void setPlayerTrans(Transform playerTrans) { 
        mFollowTrans = playerTrans;
        mFollowCamTrans = playerTrans.position;
    }

    public void setCameraMode(int camMode, Transform followTrans)
    {
        mMode = (CameraMode)camMode;
        if (mMode == CameraMode.NPC)
        {
            NPCmargin = (mFollowTrans.position+mFollowTrans.right*2f) - (followTrans.position + Vector3.up * 1.2f);
            mTrans.position += NPCmargin * 2;
            mFollowTrans = followTrans;
        }
        else if(mMode == CameraMode.FOLLOW)
        {
            mFollowTrans = followTrans;
            //mFollowTrans.position = mFollowCamTrans; 
            mTrans.localRotation = Quaternion.Euler(Vector3.right * 45f);
        }
    }

    //public void IntroCamera(Transform bossStartPos,Transform bossInitialPos,Transform playerInitialPos)
    public void IntroCamera(BossMapInfo bossMapInfo)
    {
        mMode = CameraMode.INTRO;
        mFollowCamTrans = mTrans.transform.position;
        mTrans.transform.position = bossMapInfo.bossInitialPos.position+ bossMapInfo.bossInitialPos.forward*20f;
        mTrans.transform.LookAt(bossMapInfo.bossStartPos.position);
        mTarget = bossMapInfo.bossStartPos;
        mBossMapInfo = bossMapInfo;
        StartCoroutine("IntroCameraWalking");
        
    }

    public void ShakingCamera(float delayTime)
    {
        bShakingCam = true;
        mShakingTime = delayTime + mShakingTimeOrigin;
        mCamEffect.enabled = true;
        StartCoroutine("CameraShaking", delayTime);
    }

    public void GameOverEffect()
    {
        mCamEffect.material = cameraEffectMat[0];
        mCamEffect.enabled = true;
        StartCoroutine(gameOver());
    }

    public void FadeINOUTEffect()
    {
        mCamEffect.material = cameraEffectMat[1];
        mCamEffect.enabled = true;
        StartCoroutine(fadeINOUT());
    }

    void Start()
    {
        mTrans = this.transform.GetChild(0);
        mMode = CameraMode.FOLLOW;
        mZoom = ZoomMode.OUT;
        eShakingCoroutine = CameraShaking(mShakingTime);

        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;

        mPlayerTrans = mFollowTrans.parent;

        margin = mTrans.position-transform.position;

        dirNormalized = margin.normalized;
        finalDistance = margin.magnitude;

        standardMaxZoom = finalDistance;
        standardMinZoom = minDistance;

        mShakingTime = mShakingTimeOrigin;

        mCamEffect =transform.GetChild(0).GetComponent<MainCamEffect>();
        cameraEffectMat[0].SetFloat("_Grayscale", 0);
        cameraEffectMat[1].SetFloat("_Fade", 0);


    }

    // Update is called once per frame
    void Update()
    {
        if (mMode == CameraMode.INTRO)
        {
            mTrans.position = Vector3.Lerp(mTrans.position, mFollowTrans.position+ mTrans.forward*25f+mTrans.up*2f, Time.deltaTime);
            mTrans.transform.rotation = Quaternion.LookRotation(mTarget.position - mTrans.transform.position);
        }
        else
        {
            if (Input.GetMouseButton(1))
            {
                rotX += -Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;
                rotY += Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;

                rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle);

                Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
                transform.rotation = rot;
                Quaternion rotP = Quaternion.Euler(0, rotY, 0);
                mPlayerTrans.transform.rotation = rotP;
            }
        }
        
    }

    private void FixedUpdate()
    {
        //player 따라 다니기
        if (mMode == CameraMode.FOLLOW)
        {
            if (mFollowTrans != null)
            {
                transform.position = mFollowTrans.position;
                finalDir = transform.TransformPoint(dirNormalized * maxDistance);

                RaycastHit hit;
                if(Physics.Linecast(transform.position,finalDir,out hit))
                {
                    finalDistance = Mathf.Clamp(hit.distance, minDistance, maxDistance);
                }
                else
                {
                    finalDistance = maxDistance;
                }
                mTrans.localPosition = Vector3.Lerp(mTrans.localPosition, dirNormalized * finalDistance, Time.deltaTime * smoothness);
            }
        }
        else if(mMode == CameraMode.NPC)
        {
            mTrans.position = Vector3.Lerp(mTrans.position, transform.position+NPCmargin, Time.deltaTime * smoothness);
            mTrans.rotation = Quaternion.LookRotation((mFollowTrans.position+Vector3.up*1.1f) - mTrans.position);
        }
        else if(mMode==CameraMode.ZOOM)
        {
            transform.position = mFollowTrans.position;
            finalDir = transform.TransformPoint(dirNormalized * maxDistance);

            mTrans.localPosition = Vector3.Lerp(mTrans.localPosition, dirNormalized * finalDistance, Time.deltaTime * smoothness);
        }

    }

    public void ZoomInOut()
    {
        StartCoroutine("zoomINOUT");
        mMode = CameraMode.ZOOM;
    }

    IEnumerator IntroCameraWalking()
    {
        yield return new WaitForSeconds(10.0f);

        FadeINOUTEffect();
        //StartCoroutine(fadeINOUT());

        yield return new WaitForSeconds(appliedTime);

        mTrans.transform.position = mFollowCamTrans;
        mTrans.transform.localRotation = Quaternion.Euler(45, 0, 0);
        mPlayerTrans.GetComponent<UserPlayerCtrl>().mPlayerState = 0;
        mMode = CameraMode.FOLLOW;
        mBossMapInfo.startBattle(mBossMapInfo.player);
        uiM.ActiveBossState(mBossMapInfo.mBossInfo);
    }

    IEnumerator CameraShaking(float time)
    {
        yield return new WaitForSeconds(time);

        while (mShakingTimer < mShakingTime)
        {
            mShakingTimer += Time.deltaTime;
            transform.position = UnityEngine.Random.insideUnitSphere * mShakeAmount + transform.position;
            yield return new WaitForEndOfFrame();
        }

        mShakingTimer = 0.0f;
        bShakingCam = false;
        Time.timeScale = 1.0f;

        mCamEffect.enabled = false;

        yield break;
    }

    IEnumerator gameOver()
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < appliedTime)
        {
            elapsedTime += Time.deltaTime;

            grayScale = elapsedTime / appliedTime;
            cameraEffectMat[0].SetFloat("_Grayscale", grayScale);
            yield return null;
        }

        uiM.GameOverUI();
    }

    IEnumerator zoomINOUT()
    {
        mZoom = ZoomMode.IN;
        while (finalDistance > maxDistance/2)
        {
            finalDistance -= Time.deltaTime*4.0f;

            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        mZoom = ZoomMode.OUT;
        while (finalDistance == maxDistance)
        {
            finalDistance += Time.deltaTime * 5;

            yield return null;
        }
        mMode = CameraMode.FOLLOW;
    }

    IEnumerator fadeINOUT()
    {
        uiM.SetActiveUI(false);

        float elapsedTime = 0.0f;
        appliedTime = fadeSpeed / 2;
        while (elapsedTime < appliedTime)
        {
            elapsedTime += Time.deltaTime;

            fadeValue = elapsedTime / appliedTime;
            cameraEffectMat[1].SetFloat("_Fade", fadeValue);
            yield return null;
        }

        yield return new WaitForSeconds(fadeSpeed);

        while (elapsedTime > 0)
        {
            elapsedTime -= Time.deltaTime;

            fadeValue = elapsedTime / appliedTime;
            cameraEffectMat[1].SetFloat("_Fade", fadeValue);
            yield return null;
        }

        mCamEffect.enabled = false;
        uiM.SetActiveUI(true);
    }

}
