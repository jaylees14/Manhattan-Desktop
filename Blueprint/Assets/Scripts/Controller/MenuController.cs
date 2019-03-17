using System.Collections;
using System.Collections.Generic;
using Model;
using Model.Action;
using Model.Redux;
using Model.State;
using Model.Action;
using UnityEngine;

/* Attached to Inventory, listens for key press to show/hide panel */
namespace Controller {
    public class MenuController : MonoBehaviour, Subscriber<UIState> {
        private Canvas inventoryCanvas;
        private Canvas cursorCanvas;

        void Start() {
            inventoryCanvas = GetComponent<Canvas>();
            inventoryCanvas.enabled = false;
            cursorCanvas = GameObject.FindGameObjectWithTag("Cursor").GetComponent<Canvas>();
            
            GameManager.Instance().uiStore.Dispatch(new OpenPlayingUI());
            GameManager.Instance().inventoryStore.Dispatch(new AddItemToInventory(1, 1, "Wood"));
            
            GameManager.Instance().uiStore.Subscribe(this);
        }

        void Update() {
            if (Input.GetKeyDown(KeyMapping.Inventory)) {
                if (!inventoryCanvas.enabled) {
                    GameManager.Instance().uiStore.Dispatch(new OpenInventoryUI());
                } else {
                    GameManager.Instance().uiStore.Dispatch(new CloseUI());
                }
            }
        }

        private void PauseGame() {
            Time.timeScale = 0;
            inventoryCanvas.enabled = true;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            cursorCanvas.enabled = false;
        }

        private void ContinueGame() {
            Time.timeScale = 1;
            inventoryCanvas.enabled = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            cursorCanvas.enabled = true;
        }

        public void StateDidUpdate(UIState state) {
            if (state.Selected == UIState.OpenUI.Inventory) {
                PauseGame();
            } else if (state.Selected == UIState.OpenUI.Playing) {
                ContinueGame();
            } else {
                throw new System.Exception("Not in expected state.");
            }
        }
    }
}