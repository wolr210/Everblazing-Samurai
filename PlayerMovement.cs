using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public CharacterController2D player; // the controller
    public Weapon weapon;

    public float runSpeed = 40f;  // speed of the player

    float horizontalMove = 0f; // stores the distance that the player will move

    bool jump = false; // stores whether the player wants to jump or not
    bool dash = false; // stores whether the player wants to dash or not
    bool attack = false; // stores whether the player wants to attack or not

    float attackIndex = 0; // which attack the player does

    public float dashAttackWindow = 5; // # of frames player has to both input a dash and an attack
    bool[] wasDashing;

    private void Start()
    {
        wasDashing = new bool[(int)dashAttackWindow];
        Physics2D.IgnoreLayerCollision(10, 9, true); // prevents rigidbody collisions between enemies and players
        Physics2D.IgnoreLayerCollision(9, 9, true); // prevents rigidbody collisions between enemies
        weapon.LoadWeaponFromJson("BeamKatanaData"); // loads Beam Katana as default weapon
    }

    // Update is called once per frame
    void Update()
    {
        horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed; // negative if the player is moving left, positive if moving right

        // sets dash to true if space (temporary button placement) is pressed
        if (Input.GetButtonDown("Dash"))
        {
            dash = true;
        }

        // sets jump to true if up is pressed
        if (Input.GetButtonDown("Jump"))
        {
            jump = true;
        }

        //attack checks
        if (Input.GetButtonDown("ForwardAttack"))
        {

            attack = true;
            if (player.grounded()) {
                if (DashedInWindow() && player.canDash()) {
                    attackIndex = 5;
                } else {
                    attackIndex = 0;
                }
            } else {
                if (DashedInWindow() && player.canDash()) {
                    attackIndex = 13;
                } else {
                    attackIndex = 8;
                }
            }

        }

        if (Input.GetButtonDown("UpwardAttack"))
        {

            attack = true;
            if (player.grounded())
            {
                if (DashedInWindow() && player.canDash())
                {
                    attackIndex = 6;
                }
                else
                {
                    attackIndex = 3;
                }
            }
            else
            {
                if (DashedInWindow() && player.canDash())
                {
                    attackIndex = 14;
                }
                else
                {
                    attackIndex = 11;
                }
            }

        }

        if (Input.GetButtonDown("DownwardAttack"))
        {

            attack = true;
            if (player.grounded())
            {
                if (DashedInWindow() && player.canDash())
                {
                    attackIndex = 7;
                }
                else
                {
                    attackIndex = 4;
                }
            }
            else
            {
                if (DashedInWindow() && player.canDash())
                {
                    attackIndex = 15;
                }
                else
                {
                    attackIndex = 12;
                }
            }

        }

        if (Input.GetButtonDown("Weapon1"))
        {
            weapon.LoadWeaponFromJson("BeamKatanaData");
        }

        if (Input.GetButtonDown("Weapon2"))
        {
            weapon.LoadWeaponFromJson("BattleAxeData");
        }

    }

        // called everytime the OnLandEvent occurs
        public void OnLand()
    {
        player.Land();
    }

    void FixedUpdate()
    {
        UpdateDashingWindow();
        player.Move(horizontalMove * Time.fixedDeltaTime, jump, dash);
        if(attack)
        {
            weapon.Attack(attackIndex);
        }
        jump = false;
        dash = false;
        attack = false;
    }

    public void ApplyDamage(float damage)
    {



    }

    public bool DashedInWindow()
    {

        foreach(bool dashed in wasDashing)
        {
            if (dashed) return true;
        }
        return false;

    }

    public void UpdateDashingWindow()
    {

        for(int i = 0; i < wasDashing.Length - 1; i++)
        {

            wasDashing[i] = wasDashing[i + 1];

        }

        wasDashing[wasDashing.Length - 1] = dash;

    }

}