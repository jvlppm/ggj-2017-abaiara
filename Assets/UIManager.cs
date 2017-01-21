using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    public Character Selected;
    public Text CharacterName;
    public Image Char_Avatar;
    public Image hp_bar;
    public Image ap_bar;
    public Text hp_info;
    public Text ap_info;
    public Image [] skills;

	// Use this for initialization
	void Start () {
        CharacterName.text = Selected.displayName;
        Char_Avatar.sprite = Selected.avatar;
        hp_info.text = Selected.hp.ToString();
        ap_info.text = Selected.ap.ToString();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Skill1_Onclicked ()
    {

    }
    
    public void Skill2_Onclicked ()
    {

    }

    public void Skill3_Onclicked ()
    {

    }

    public void Skill4_Onclicked ()
    {

    }

   /* public void error ()
    {
        if (actionPoint < 0)
        {
            error.gameObject.SetActive(true);
        }
    }*/

}
