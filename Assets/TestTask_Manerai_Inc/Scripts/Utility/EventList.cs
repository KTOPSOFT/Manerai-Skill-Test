using UnityEngine;

using UnityEngine.InputSystem;

using System.Collections;
using System.Collections.Generic;

namespace YukiOno.SkillTest
{
    public class EventList : MonoBehaviour
    {
        public List<GameEvent> events = new List<GameEvent>();

        public ScreenFade screenFade;

        public void FadeScreen(int index)
        {
            if (screenFade != null && index < events.Count)
            {
                screenFade.FadeTo(events[index]);
            }
        }

        public void InvokeEvent(int index)
        {
            if (index < events.Count)
            {
                events[index].Invoke();
            }
        }
    }
}




