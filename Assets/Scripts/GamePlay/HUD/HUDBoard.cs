using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

namespace HUD
{
    [DefaultExecutionOrder(3000)]
    public class HUDBoard : MonoBehaviour
    {
        private bool        mNeedUpdate = true;
        private float       mHeight = 0f;
        private Transform   mTarget;
        private string      mPath;
        private UILabel     mText;

        void Awake()
        {
            mText = transform.Find("Text").GetComponent<UILabel>();
			mText.height = 60;
			mText.width = 100;
        }

        void Update()
        {
            if (mNeedUpdate == false || mTarget == null)
            {
                return;
            }
            UpdatePosition();
        }

        public void SetVisable(bool val)
        {
            gameObject.SetActive(val);
        }

        public void SetTarget(Transform target)
        {
            mTarget = target;
            UpdatePosition();
        }

        public void SetPath(string path)
        {
            mPath = path;
        }

        public void SetHeight(float height)
        {
            mHeight = height;
        }

        public void UpdatePosition()
        {
            if (GTCameraManager.Instance.MainCamera == null ||
                GTCameraManager.Instance.NGUICamera == null)
            {
                return;
            }
            if (GTWorld.Instance.Plot != null)
            {
                if (GTWorld.Instance.Plot.IsPlaying())
                {
                    transform.position = new Vector3(100000, 0, 0);
                    return;
                }
            }
            Vector3 pos_3d = mTarget.position + new Vector3(0, mHeight, 0);
            Vector3 pos_screen = GTCameraManager.Instance.MainCamera.WorldToScreenPoint(pos_3d);
            if (pos_screen.z <= 0)
            {
                transform.position = new Vector3(100000, 0, 0);
            }
            else
            {
                transform.localScale = Vector3.one * 0.001f * pos_screen.z / mTarget.localScale.x;
                transform.position = pos_3d;
                transform.rotation = GTCameraManager.Instance.CameraCtrl.OriginalRotation; 
            }
        }

        public void Show(string text1, string text2 = null, int titleID = 0)
        {
            string text = string.Empty;
            if (string.IsNullOrEmpty(text1) == false)
            {
                text = text1;
            }
            if (string.IsNullOrEmpty(text2) == false)
            {
                text = text + '\n' + text2;
            }
            mText.text = text;
        }

        public void Release()
        {
            this.mTarget = null;
            GTPoolManager.Instance.ReleaseGo(mPath, gameObject);
        }
    }
}