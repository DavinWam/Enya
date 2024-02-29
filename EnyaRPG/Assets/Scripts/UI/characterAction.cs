using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class characterAction : MonoBehaviour
{
    public BattleController bc;
    void Start(){
        bc = FindObjectOfType<BattleController>();
    } 
    // Start is called before the first frame update
    public void Attack()
    {
        bc.activeCharacter.GetComponent<PlayerCharacter>().Attack();
    }
    public void Spell()
    { 
        bc.activeCharacter.GetComponent<PlayerCharacter>().CastSpell();
    }
    public void Heal()
    {
       bc.activeCharacter.GetComponent<PlayerCharacter>().UseItem();
    }
    public void Ignite()
    {
        bc.activeCharacter.GetComponent<PlayerCharacter>().Ignite();
    }

}
