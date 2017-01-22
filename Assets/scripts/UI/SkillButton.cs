using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Button))]
    public class SkillButton : MonoBehaviour
    {
        public bool canUse
        {
            set {
                icon.color = new Color(icon.color.r, icon.color.g, icon.color.b, value? 1 : 0.3f);
                button.interactable = value;
            }
        }

        public interface IHandler : IEventSystemHandler
        {
            void OnClick(SkillButton skill);
        }

        Button button;

        public Image icon;
        [System.NonSerialized] public Skill skill;

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// </summary>
        void Awake()
        {
            button = GetComponent<Button>();
        }

        public void SetSkill(Skill skill)
        {
            this.skill = skill;
            icon.sprite = skill.icon;
        }

        public void OnClick()
        {
            ExecuteEvents.ExecuteHierarchy<IHandler>(gameObject, null, (x,y)=> x.OnClick(this));
        }
    }
}
