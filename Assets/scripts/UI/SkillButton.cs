using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SkillButton : MonoBehaviour
    {
        public interface IHandler : IEventSystemHandler
        {
            void OnClick(Skill skill);
        }

        public Image icon;
        [System.NonSerialized] public Skill skill;

        public void SetSkill(Skill skill)
        {
            this.skill = skill;
            icon.sprite = skill.icon;
        }

        public void OnClick()
        {
            ExecuteEvents.ExecuteHierarchy<IHandler>(gameObject, null, (x,y)=> x.OnClick(skill));
        }
    }
}
