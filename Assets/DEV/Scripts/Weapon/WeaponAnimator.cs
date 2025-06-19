using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WeaponAnimType { Null, Cartridge }
public class WeaponAnimator : MonoBehaviour
{
    [SerializeField] public Transform cartridgeSpawnPos;
    public List<WeaponAnim> anims;
    private Animator animator;

    [Space(6)]

    [Title("Run Anim")]
    public Vector3 runAnimRot;
    public Vector3 runAnimPos;
    public float runPosSpeed;
    public float runRotSpeed;


    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        
        anims.ForEach(anim => anim.Init());
        anims.ForEach(anim => anim.AssignWeaponAnimator(this));
    }

    // Update is called once per frame
    void Update()
    {
        anims.ForEach(anim => anim.Update());
    }

    public void ActiveAllAnim()
    {
        anims.ForEach(anim => anim.Active());
    }

    [Button(size: ButtonSizes.Large)]
    public void Reload()
    {
        animator.enabled = true;
        animator.SetTrigger("Reload");
        ReloadUIController.instance.PlayAnim().Forget();
    }
}

[System.Serializable]
public class WeaponAnim
{
    public WeaponAnimType animType;
    public bool active;
    public Transform animObj;
    public Vector3 defaultPos;
    public Vector3 targetPos;
    public float speed;
    private Vector3 pos;
    private WeaponAnimator animator;
    public void Init()
    {
        defaultPos = animObj.localPosition;
        pos = defaultPos;
    }

    public void AssignWeaponAnimator(WeaponAnimator animator)
    {
        this.animator = animator;
    }

    public void Update()
    {
        pos = active ? targetPos : defaultPos;

        animObj.localPosition = Vector3.MoveTowards(animObj.localPosition, pos, speed * Time.deltaTime);

        if (active)
        {
            if (animObj.localPosition == pos)
            {
                active = !active;
            }
        }

    }

    public void Active()
    {
        active = true;

        if(animType == WeaponAnimType.Cartridge)
        {
            //Fake Spawn Cartridge
            Cartridge cartridge = BulletManager.GetCartridge();
            cartridge.gameObject.SetActive(true);

            //Cartridge Pos Update
            cartridge.transform.parent = animator.cartridgeSpawnPos;
            cartridge.transform.localPosition = Vector3.zero;

            //Play Anim
            cartridge.PlayAnim().Forget();
        }
    }


}
