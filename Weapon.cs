using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

[Serializable]
public class Weapon : MonoBehaviour
{

    /* .JSON FILE FORMAT:
     *      name the .json file WEAPON_NAME + "Data" ex. BeamKatanaData
     *      each field in the .json file must match one of the public fields below in both name and type
     *      all public fields must be filled in order to work
     *      refer to BeamKatanaData.json for a template
     */

    public string name;
    public string description;

    public string modelPath; // file path to the .png file of this weapon; name the .png file WEAPON_NAME + "Model" ex. BeamKatanaModel
    public Vector3 modelOffset; // offset the position of the model so it fits in hand

    public Vector3 attackPoint;
    public float attackRadius;
    public string[] attackNames;
    public float[] attackDamage;
    public float[] attackStartDelay;
    public float[] attackFinishDelay;

    public Vector2[] knockback;
    public float knockbackMultiplier;

    public bool[] freezesPlayer;
    public bool[] freezesEnemy;

    private SpriteRenderer model; // renders the .png file

    private LayerMask Enemy;

    private bool attackInProgress = false;
    private bool multihitAttackInProgress = false;

    private int forwardGroundedCounter = 0; // current number of times the grounded multihit attack has been executed; resets at 3
    private int forwardAerialCounter = 0; // current number of times the aerial multihit attack has been executed; resets at 3

    public void LoadWeaponFromJson(string path) // loads a weapon from a .json file (all weapon .json files are located in Assets/Resources/Weapons)
    {
        TextAsset jsonRaw = Resources.Load<TextAsset>("Weapons/" + path);
        JsonUtility.FromJsonOverwrite(jsonRaw.ToString(), this); // sets weapon properties from .json file

        model = GetComponentInChildren<SpriteRenderer>();
        model.transform.localPosition = modelOffset; // offsets sprite model

        Sprite weaponSprite = Resources.Load<Sprite>("Weapons/" + modelPath);
        model.sprite = weaponSprite; // loads sprite image

        this.transform.Find("Attack Point").localPosition = attackPoint; // sets attack point
    }

    public void Start()
    {
        Enemy = LayerMask.GetMask("Enemy");
    }

    public void Attack(float attackIndex)
    {
        
        // checks to see if there is an attack in progess; if not, checks if this attack is part of a multihit attack, updates the attack and counters accordingly, 
        // then starts a coroutine to execute the attack
        if (!attackInProgress)
        {
            float attackToExecute = attackIndex;
            if (attackIndex == 0) // forward grounded attack
            {
                multihitAttackInProgress = (forwardGroundedCounter != 2);
                attackToExecute += forwardGroundedCounter;
                forwardGroundedCounter = (forwardGroundedCounter + 1) % 3;
            }
            else if (attackIndex == 8) // forward aerial attack
            {
                multihitAttackInProgress = (forwardAerialCounter != 2);
                attackToExecute += forwardAerialCounter;
                forwardAerialCounter = (forwardAerialCounter + 1) % 3;
            }
            else
            {
                forwardGroundedCounter = 0;
                forwardAerialCounter = 0;
            }

            StartCoroutine(ExecuteDelayedAttack(attackToExecute, attackStartDelay[(int)attackToExecute], multihitAttackInProgress ? 0 : attackFinishDelay[(int)attackToExecute])); // if a multihit attack is in progress, 
            // then there is no attack finish delay
            print("Executed " + attackNames[(int)attackToExecute]);
        }

    }

    public IEnumerator ExecuteDelayedAttack(float attackIndex, float aSD, float aFD)
    {
        attackInProgress = true;
        yield return new WaitForSeconds(aSD / 1000); // attack start delay
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(this.transform.Find("Attack Point").position, attackRadius, Enemy); // gets all colliders within attack range

        if(freezesPlayer[(int)attackIndex])
        {
            this.gameObject.GetComponentInParent<Rigidbody2D>().velocity = new Vector2(0, 0); // freezes player
        }

        foreach (Collider2D enemy in hitEnemies)
        {
            if (freezesEnemy[(int)attackIndex])
            {
                enemy.gameObject.GetComponent<Enemy>().ApplyDamageAndKnockback((attackDamage[(int)attackIndex]), new Vector2(0, 0));
                enemy.gameObject.GetComponent<Enemy>().GetComponent<Rigidbody2D>().velocity = new Vector2(0, 5);
            }
            else
            {
                enemy.gameObject.GetComponent<Enemy>().ApplyDamageAndKnockback(attackDamage[(int)attackIndex], 
                    new Vector2(knockback[(int)attackIndex].x * knockbackMultiplier * (this.gameObject.GetComponentInParent<CharacterController2D>().FacingRight() ? 1 : -1), knockback[(int)attackIndex].y * knockbackMultiplier));
            }
            print("Hit " + enemy.name);
        }

        yield return new WaitForSeconds(aFD / 1000); // attack finish delay
        attackInProgress = false;
        yield return null; // finishes attack and sets attackInProgress to false, allowing another attack to start
    }

}
