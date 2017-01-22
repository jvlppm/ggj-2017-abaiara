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
    public UI.SkillButton [] skills;

	// Use this for initialization
	void Start () {
        
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

    public void SetSelection(Character character)
    {
        Selected = character;

        if (!character) {
            if (CharacterName != null) {
                CharacterName.text = "";
            }
            Char_Avatar.sprite = null;
            hp_info.text = "";
            ap_info.text = "";
            foreach (var skill in skills)
            {
                skill.gameObject.SetActive(false);
            }
            return;
        }

        if (CharacterName != null)
        {
            CharacterName.text = character.displayName;
        }
        Char_Avatar.sprite = character.avatar;
        hp_info.text = character.hp.ToString();
        ap_info.text = character.ap.ToString();
        int i = 0;
        for (; i < character.skills.Length; i++)
        {
            skills[i].gameObject.SetActive(true);
            skills[i].SetSkill( character.skills[i] );
        }
        for (; i < skills.Length; i++) {
            skills[i].gameObject.SetActive(false);
        }
    }

   /* public void error ()
    {
        if (actionPoint < 0)
        {
            error.gameObject.SetActive(true);
        }
    }*/

}
