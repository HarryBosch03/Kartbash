using System;
using System.Collections.Generic;
using UnityEngine;

namespace Runtime
{
    public class Toasts : MonoBehaviour
    {
        private static Toasts privateInstance;

        public static Toasts instance
        {
            get
            {
                if (privateInstance == null) privateInstance = FindFirstObjectByType<Toasts>();
                return privateInstance;
            }
        }

        private List<Message> messages = new();

        public static void ShowMessage(Message message)
        {
            var instance = Toasts.instance;
            if (instance == null) return;

            if (!instance.messages.Contains(message))
            {
                instance.messages.Add(message);
            }

            message.startTime = Time.time;
        }

        public static void RemoveMessage(Message message)
        {
            var instance = Toasts.instance;
            if (instance == null) return;

            instance.messages.Remove(message);
        }

        private void Update() { messages.RemoveAll(e => !e.persistent && Time.time - e.startTime > e.duration); }

        private void OnGUI()
        {
            var rect = new Rect();
            rect.xMin = Screen.width / 2f - 150f;
            rect.xMax = Screen.width / 2f + 150f;
            rect.yMin = Screen.height / 3f * 2f - 9f;
            rect.yMax = Screen.height;

            using (new GUILayout.AreaScope(rect))
            {
                for (var i = messages.Count - 1; i >= 0; i--)
                {
                    var message = messages[i];
                    var style = new GUIStyle();
                    var color = message.color;
                    color.a *= Mathf.Clamp01(message.duration - (Time.time - message.startTime));
                    style.normal.textColor = color;
                    GUILayout.Label(message.text);
                }
            }
        }

        public class Message
        {
            public string text;
            public Color color = Color.white;
            public float duration = 6f;
            public float startTime;
            public bool persistent;

            public Message() { }
            public Message(string text, float duration = 6f) : this(text, Color.white, duration) { }

            public Message(string text, Color color, float duration = 6f)
            {
                this.text = text;
                this.duration = duration;
                this.color = color;

                startTime = Time.time;
                persistent = false;
            }

            public static implicit operator Message(string text) => new Message(text);
        }
    }
}