using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {
    [System.NonSerialized] public Character Avatar;
    public Character Selected;
    public Text CharacterName;
    public Image Char_Avatar;
    public Image hp_bar;
    public Image ap_bar;
    public Text hp_info;
    public Text ap_info;
    public UI.SkillButton [] skills;

    public Transform avatarShake;

    Vector3 baseAvatarPosition;
    IEnumerator currentShake;

	// Use this for initialization
	void Start () {
        if (avatarShake) {
            baseAvatarPosition = avatarShake.transform.position;
        }
	}
	
	// Update is called once per frame
	void Update () {
        if (Selected) {
            ap_bar.fillAmount = Mathf.Lerp(ap_bar.fillAmount, Selected.ap / Selected.maxAp, 0.1f);
            ap_info.text = (int)Mathf.Ceil(Selected.ap) + "/" + Selected.maxAp;
        }

        UpdateAvatar(Avatar != null? Avatar : Selected);
	}

    void UpdateAvatar(Character character) {
        if (character != null) {
            hp_bar.fillAmount = Mathf.Lerp(hp_bar.fillAmount, character.hp / character.maxHp, 0.1f);
            hp_info.text = (int)Mathf.Ceil(character.hp) + "/" + character.maxHp;
        }
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

    public void HitAvatar(Character character)
    {
        if (avatarShake) {
            if (currentShake != null) {
                StopCoroutine(currentShake);
            }
            SetAvatarView(character);
            currentShake = ShakeAvatar(character);
            StartCoroutine(currentShake);
        }
    }

    IEnumerator ShakeAvatar(Character character)
    {
        Avatar = character;
        int times = 14;
        float duration = 1.2f;
        Vector3 offset = new Vector3(5, 0, 0);
        

        for (float i = 0; i < 1; i += Time.deltaTime / (duration )) {
            var pos = Mathf.Sin(i * Mathf.PI * times + Mathf.PI) * 2 - 1;
			avatarShake.position = baseAvatarPosition + offset * pos;
            yield return null;
        }

        avatarShake.transform.position = baseAvatarPosition;
        yield return new WaitForSeconds(0.5f);
        Avatar = null;
        SetAvatarView(Selected);
    }

    void SetAvatarView(Character character) {
        if (!character) {
            if (CharacterName != null) {
                CharacterName.text = "";
            }
            Char_Avatar.gameObject.SetActive(false);

            hp_info.text = "";
            
            hp_bar.fillAmount = 1;
            
            return;
        }

        Char_Avatar.gameObject.SetActive(true);

        if (CharacterName != null)
        {
            CharacterName.text = character.displayName;
        }
        Char_Avatar.sprite = character.avatar;
        hp_info.text = (int)Mathf.Ceil(character.hp) + "/" + character.maxHp;

        hp_bar.fillAmount = character.hp / character.maxHp;
    }

    public void SetSelection(Character character)
    {
        Selected = character;
        Avatar = null;
        SetAvatarView(character);

        if (!character) {
            ap_info.text = "";
            ap_bar.fillAmount = 1;
            foreach (var skill in skills)
            {
                skill.gameObject.SetActive(false);
            }
            return;
        }

        ap_info.text = (int)Mathf.Ceil(character.ap) + "/" + character.maxAp;
        ap_bar.fillAmount = character.ap / character.maxAp;

        int i = 0;
        for (; i < character.skills.Length; i++)
        {
            var skill = character.skills[i];
            skills[i].canUse = skill.activation == Skill.Activation.Button && character.ap >= skill.ap;
            
            skills[i].gameObject.SetActive(true);
            skills[i].SetSkill( skill );
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
