using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Chessman
{

    //pawn specific variables
    public bool isOnStartingPoint = true;
    public bool isDestroyed = false;
    public Pawn selectedPawn;
    //status effects
    public bool isInvulnerable = false;
    public bool isStunned = false;

    //tile movement modifier
    public bool moveLikeKing = false;
    public int extraTiles = 0;


    //passive skill tracking
    public bool passiveActive = false;

    //buffs array or list something
    private List<string> activeBuffs = new List<string>();

    // Active skill 1 PLACE HOLDER properties
    public int skill1SPCost = 1;
    public int skill1Cooldown = 0;
    public int skill1Duration = 0;
    private int skill1CooldownCounter = 0;
    private int skill1DurationCounter = 0;


    public void CheckPassiveSkill()
    {

    }


    private void UpdateBuffs()
    {


    }

    //cooldown logic
    private void UpdateCooldowns()
    {

    }

    //activate later
    public bool CanUseSkill1(int currentSP)
    {
        return currentSP >= skill1SPCost && skill1CooldownCounter == 0;
    }



    public void UseSkill1()
    {
        /*  if (controller == null)
              controller = GameObject.FindGameObjectWithTag("GameController");
          extraTiles = 1;
          Debug.Log("Skill Activated");
          InitiateMovePlates(); */

    }

    private void OnMouseUp()
    {
        /*
        controller.GetComponent<Game>().selectedPawn = this; // 🐒 Remember this pawn!
        */

    }

    public void OnSkillButtonClick()
    {
        /* if (selectedPawn != null) 
         {
             selectedPawn.UseSkill1(); // 🐒 Use skill on the selected pawn!
         }*/
    }

    public override void InitiateMovePlates()
    {
        /*
        Game sc = controller.GetComponent<Game>();
        int direction = (player == "white") ? 1 : -1;
        int startRow = (player == "white") ? 1 : 6;

        // Normal single move
        PawnMovePlate(xBoard, yBoard + direction);

        // Double move from starting position
        if (yBoard == startRow && sc.GetPosition(xBoard, yBoard + direction) == null && sc.GetPosition(xBoard, yBoard + 2 * direction) == null)
        {
            MovePlateSpawn(xBoard, yBoard + 2 * direction);
        }

        // Extra move from skill
        if (extraTiles > 0)
        {
            Debug.Log("Extra move activated");
            // Only allow if path is clear
            for (int i = 1; i <= extraTiles; i++)
            {
                int targetY = yBoard + (i + 1) * direction;
                if (sc.PositionOnBoard(xBoard, targetY) && sc.GetPosition(xBoard, targetY) == null)
                {
                    MovePlateSpawn(xBoard, targetY);
                }
                else
                {
                    break; // Stop if blocked
                }
            }
        }
    }*/



    }

}
