﻿using UnityEngine;
using Model.State;
using Model.Redux;
using Model.Action;

namespace View {
    public class Highlight : MonoBehaviour, Subscriber<UIState>{
        [SerializeField] private Color highlightColor;
        private Color initialColor;
        private Renderer rend;
        private bool paused;

        void Start () {
            rend = GetComponent<Renderer>();
            GameManager.Instance().uiStore.Subscribe(this);
            paused = false;
            initialColor = rend.material.color;
        }

        public Color GetColor() {
            return this.initialColor;
        }

        void OnMouseEnter() {
            if (!paused && enabled) {
              rend.material.color = highlightColor;
            }
        }

        void OnMouseExit() {
            if (!paused && enabled) {
              rend.material.color = initialColor;
            }
        }

        void resetColor() {
            paused = true;
            rend.material.color = initialColor;
        }

        public void StateDidUpdate(UIState state) {
            switch (state.Selected) {
              case UIState.OpenUI.Playing:
                  paused = false;
                  break;
              case UIState.OpenUI.Intro:
                  paused = false;
                  GameManager.Instance().uiStore.Unsubscribe(this);
                  break;
              case UIState.OpenUI.Login:
                  GameManager.Instance().uiStore.Unsubscribe(this);
                  break;
              default:
                  resetColor();
                  break;
              }
        }
    }
}
