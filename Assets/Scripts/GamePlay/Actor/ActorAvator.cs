using UnityEngine;
using System.Collections;

public class ActorAvator : IActorComponent
{
    private Transform           mRootTrans;
    private Transform           mShadow;
    private GameObject          mWeapon1;
    private GameObject          mWeapon2;
    private Material            mWeaponMat1;
    private Material            mWeaponMat2;
    private Shader              mWeaponShader1;
    private Shader              mWeaponShader2;
    private SkinnedMeshRenderer mSkinRenderer;
    private Material            mSkinMaterial;
    private Shader              mSkinShader;
    private Animator            mAnimator;

    private GameObject          mRootObj;
    private Transform           mHandTran1;
    private Transform           mHandTran2;
    private Transform           mBodyTrans;
    private Transform           mHeadTrans;
    private Transform           mBuffTrans;
    private Transform           mRidePoint;
    private int[]               mEquipArray;

    public void Initial(Actor actor)
    {
        this.Initial(actor.Obj.transform);
    }

    public void Initial(Transform root)
    {
        this.mRootTrans    = root;
        this.mRootObj      = mRootTrans.gameObject;
        this.mRidePoint    = GTTools.GetBone(mRootTrans, "Bone026");
        this.mHandTran1    = GTTools.GetBone(mRootTrans, "Bip01 Prop1");
        this.mHandTran2    = GTTools.GetBone(mRootTrans, "Bip01 Prop2");
        this.mBodyTrans    = GTTools.GetBone(mRootTrans, "BP_Spine");
        this.mHeadTrans    = GTTools.GetBone(mRootTrans, "BP_Head");
        this.mBuffTrans    = GTTools.GetBone(mRootTrans, "BP_Buff");
        this.mAnimator     = mRootObj.GetComponent<Animator>();
        this.mShadow       = mRootTrans.Find("shadow");
        this.SetShadowActive(false);
        this.mSkinRenderer = mRootTrans.GetComponentInChildren<SkinnedMeshRenderer>();
        this.mSkinMaterial = this.mSkinRenderer == null ? null : this.mSkinRenderer.material;
        this.mSkinShader   = this.mSkinMaterial == null ? null : this.mSkinMaterial.shader;
        this.mEquipArray   = new int[8];
    }

    public void Execute()
    {

    }

    public void Release()
    {
        GTResourceManager.Instance.DestroyObj(mWeaponMat1);
        GTResourceManager.Instance.DestroyObj(mWeaponMat2);
        GTResourceManager.Instance.DestroyObj(mWeapon1);
        GTResourceManager.Instance.DestroyObj(mWeapon2);
    }
    public void SetWeaponActive(bool active)
    {
        if (mWeapon1 != null)
        {
            mWeapon1.SetActive(active);
        }
        if (mWeapon2 != null)
        {
            mWeapon2.SetActive(active);
        }
    }

    public void SetShadowActive(bool active)
    {
        if (mShadow != null)
        {
            mShadow.gameObject.SetActive(active);
        }
    }

    public void ChangeAvatar(int pos, int id)
    {
        if (mEquipArray[pos - 1] == id)
        {
            return;
        }
        switch (pos)
        {
            case 1:
                ChangeHelmet(id);
                break;
            case 2:
                ChangeNecklace(id);
                break;
            case 3:
                ChangeArmor(id);
                break;
            case 4:
                ChangeShoes(id);
                break;
            case 5:
                ChangeWrist(id);
                break;
            case 6:
                ChangeRing(id);
                break;
            case 7:
                ChangeTalisman(id);
                break;
            case 8:
                ChangeWeapon(id);
                break;
        }
        mEquipArray[pos - 1] = id;
    }

    public void PlayAnim(string animName, Callback onFinish)
    {
        GTAction.Get(mAnimator).Play(animName, onFinish);
    }

    public Transform  GetBindTransform(EBind bind)
    {
        switch (bind)
        {
            case EBind.Head:
                return this.mHeadTrans == null? mRootTrans: mHeadTrans;
            case EBind.Body:
                return this.mBodyTrans == null ? mRootTrans : mBodyTrans;
            case EBind.Foot:
                return this.mRootTrans;
            case EBind.HandL:
                return this.mHandTran1 == null ? mRootTrans : mHandTran1;
            case EBind.HandR:
                return this.mHandTran2 == null ? mRootTrans : mHandTran2;
            case EBind.Buff:
                return this.mBuffTrans == null ? mRootTrans : mBuffTrans;
            default:
                return mRootTrans;
        }
    }

    public Vector3    GetBindPosition(EBind bind)
    {
        switch (bind)
        {
            case EBind.Head:
                return this.mHeadTrans == null ? this.mRootTrans.position + new Vector3(0, 2, 0) : this.mHeadTrans.position;
            case EBind.Body:
                return this.mBodyTrans == null ? this.mRootTrans.position + new Vector3(0, 1, 0) : this.mBodyTrans.position;
            case EBind.Foot:
                return this.mRootTrans.position;
            case EBind.HandL:
                return this.mHandTran1 == null ? this.mRootTrans.position + new Vector3(0, 1, 0) : this.mHandTran1.position;
            case EBind.HandR:
                return this.mHandTran2 == null ? this.mRootTrans.position + new Vector3(0, 1, 0) : this.mHandTran2.position;
            case EBind.Buff:
                return this.mBuffTrans == null ? this.mRootTrans.position + new Vector3(0, 1, 0) : this.mBuffTrans.position;
            default:
                return this.mRootTrans.position;
        }
    }

    public Transform  GetRidePoint()
    {
        return mRidePoint;
    }

    public GameObject GetRootObj()
    {
        return mRootObj;
    }

    void ChangeHelmet(int id)
    {

    }

    void ChangeNecklace(int id)
    {

    }

    void ChangeArmor(int id)
    {

    }

    void ChangeShoes(int id)
    {

    }

    void ChangeWrist(int id)
    {

    }

    void ChangeRing(int id)
    {

    }

    void ChangeTalisman(int id)
    {

    }

    void ChangeWeapon(int id)
    {
        DItem itemDB = ReadCfgItem.GetDataById(id);
        if (itemDB == null)
        {
            return;
        }
        GTResourceManager.Instance.DestroyObj(mWeaponMat1);
        GTResourceManager.Instance.DestroyObj(mWeaponMat2);
        GTResourceManager.Instance.DestroyObj(mWeapon1);
        GTResourceManager.Instance.DestroyObj(mWeapon2);
        if (mHandTran1 != null && !string.IsNullOrEmpty(itemDB.Model_R))
        {
            mWeapon1 = GTResourceManager.Instance.Load<GameObject>(itemDB.Model_R, true);
            if (mWeapon1 != null)
            {
                NGUITools.SetLayer(mWeapon1, mRootTrans.gameObject.layer);
                GTTools.ResetLocalTransform(mWeapon1.transform, mHandTran1);
                MeshRenderer renderer = mWeapon1.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    renderer = mWeapon1.GetComponentInChildren<MeshRenderer>();
                }
                mWeaponMat1 = renderer == null ? null : renderer.material;
                mWeaponShader1 = mWeaponMat1 == null ? null : mWeaponMat1.shader;
            }
        }
        if (mHandTran2 != null && !string.IsNullOrEmpty(itemDB.Model_L))
        {
            mWeapon2 = GTResourceManager.Instance.Load<GameObject>(itemDB.Model_L, true);
            if (mWeapon2 != null)
            {
                NGUITools.SetLayer(mWeapon2, mRootTrans.gameObject.layer);
                GTTools.ResetLocalTransform(mWeapon2.transform, mHandTran2);
                MeshRenderer renderer = mWeapon2.GetComponent<MeshRenderer>();
                if (renderer == null)
                {
                    renderer = mWeapon2.GetComponentInChildren<MeshRenderer>();
                }
                mWeaponMat2 = renderer == null ? null : renderer.material;
                mWeaponShader2 = mWeaponMat2 == null ? null : mWeaponMat2.shader;
            }
        }
    }
}
